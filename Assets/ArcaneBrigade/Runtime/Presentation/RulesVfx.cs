using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The painted layer under the rules ink: shader-drawn areas (organic boiling edge, swirling colour, never a rim),
    /// particle sprays of painted pieces, and flipbook bursts from the painted effect library. Areas and flipbooks are drawn
    /// immediate-mode between Begin and End; particles live on after they are emitted. Pools are bounded so a 100-enemy wave
    /// stays cheap.</summary>
    public sealed class RulesVfx : MonoBehaviour
    {
        private const int GroundOrder = 66, AreaOrder = 65, LoopOrder = 72, BurstOrder = 2150, ParticleOrder = 2160, MaxParticlesPerSystem = 300;
        private sealed class AreaView { public GameObject Go; public MeshRenderer Renderer; }
        private sealed class FlipView { public SpriteRenderer Renderer; }
        private readonly List<AreaView> areas = new List<AreaView>();
        private readonly List<FlipView> flips = new List<FlipView>();
        private readonly Dictionary<Sprite, ParticleSystem> systems = new Dictionary<Sprite, ParticleSystem>();
        private readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        private Material areaMaterial, glowMaterial, particleMaterial;
        private Mesh quad;
        private int usedAreas, usedFlips;
        public RulesVfxLibrary Library { get; private set; }
        public int Areas => usedAreas;
        public bool Ready => areaMaterial != null && areaMaterial.shader != null && areaMaterial.shader.isSupported;
        public int Flips => usedFlips;
        public int Particles { get { int n = 0; foreach (ParticleSystem s in systems.Values) if (s != null) n += s.particleCount; return n; } }

        private static readonly int ColorA = Shader.PropertyToID("_ColorA"), ColorB = Shader.PropertyToID("_ColorB"), Params = Shader.PropertyToID("_Params"),
            Params2 = Shader.PropertyToID("_Params2"), ShapeId = Shader.PropertyToID("_Shape"), MainTex = Shader.PropertyToID("_MainTex");

        private void Awake()
        {
            Library = RulesVfxLibrary.Load();
            Shader shader = Resources.Load<Shader>("Shaders/InkArea");
            if (shader != null)
            {
                areaMaterial = new Material(shader) { name = "Rules ink area" };
                // friendly areas glow: their colour is added to the floor so they read as light, not as paint over it
                glowMaterial = new Material(shader) { name = "Rules ink glow area" };
                glowMaterial.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha); glowMaterial.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
            }
            particleMaterial = new Material(Shader.Find("Sprites/Default")) { name = "Rules ink particle" };
            quad = new Mesh { name = "Rules ink quad" };
            quad.vertices = new[] { new Vector3(-.5f, -.5f), new Vector3(.5f, -.5f), new Vector3(.5f, .5f), new Vector3(-.5f, .5f) };
            quad.uv = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
            quad.triangles = new[] { 0, 2, 1, 0, 3, 2 };
            quad.RecalculateBounds();
        }

        public void Begin() { usedAreas = usedFlips = 0; }
        public void End()
        {
            for (int i = usedAreas; i < areas.Count; i++) if (areas[i].Go.activeSelf) areas[i].Go.SetActive(false);
            for (int i = usedFlips; i < flips.Count; i++) if (flips[i].Renderer.enabled) flips[i].Renderer.enabled = false;
        }

        public enum Shape { Ellipse, Lane, Cone }

        /// <summary>One painted area. Ellipse: centre a, radius r (flattened to the floor). Lane: from a to b, half width r.
        /// Cone: apex a toward b, reach r, half angle in radians. Uniform: one colour from centre to edge (enemy danger).</summary>
        public void Area(Shape shape, Vector2 a, Vector2 b, float r, float coneHalf, Color core, Color outer, float grow, float fade, float boil, float seed, bool uniform, Sprite ground = null)
        {
            if (areaMaterial == null || fade <= .01f || r <= .01f) return;
            AreaView view = NextArea();
            view.Renderer.sharedMaterial = uniform ? areaMaterial : glowMaterial;
            Transform t = view.Go.transform;
            float shapeKind = 0, straight = 0;
            switch (shape)
            {
                case Shape.Lane:
                {
                    Vector2 d = b - a; float length = d.magnitude + 2 * r;
                    t.position = (Vector2)((a + b) * .5f); t.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
                    t.localScale = new Vector3(length, 2 * r, 1); shapeKind = 1; straight = Mathf.Clamp01((length - 2 * r) / Mathf.Max(.01f, length));
                    break;
                }
                case Shape.Cone:
                {
                    Vector2 d = (b - a).sqrMagnitude > .0001f ? (b - a).normalized : Vector2.right;
                    t.position = a + d * r * .5f; t.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
                    t.localScale = new Vector3(r, 2 * r * Mathf.Tan(Mathf.Clamp(coneHalf, .05f, 1.4f)), 1); shapeKind = 2;
                    break;
                }
                default:
                    t.position = a; t.rotation = Quaternion.identity; t.localScale = new Vector3(2 * r, 2 * r * .62f, 1);
                    break;
            }
            block.Clear();
            block.SetColor(ColorA, core); block.SetColor(ColorB, outer);
            block.SetVector(Params, new Vector4(Mathf.Clamp01(grow) * .97f, Mathf.Clamp01(fade), boil, seed));
            block.SetVector(Params2, new Vector4(uniform ? 0 : 1.1f, uniform ? 1 : 0, ground != null ? .55f : 0, uniform ? 0 : 1));
            block.SetVector(ShapeId, new Vector4(shapeKind, straight, coneHalf, 0));
            if (ground != null) block.SetTexture(MainTex, ground.texture);
            view.Renderer.SetPropertyBlock(block);
        }

        private AreaView NextArea()
        {
            if (usedAreas == areas.Count)
            {
                var go = new GameObject("Rules ink area"); go.transform.SetParent(transform, false);
                go.AddComponent<MeshFilter>().sharedMesh = quad;
                var r = go.AddComponent<MeshRenderer>(); r.sharedMaterial = areaMaterial; r.sortingOrder = AreaOrder;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false;
                areas.Add(new AreaView { Go = go, Renderer = r });
            }
            AreaView view = areas[usedAreas++];
            if (!view.Go.activeSelf) view.Go.SetActive(true);
            return view;
        }

        /// <summary>One frame of a painted flipbook burst: t in [0,1] picks the frame; it grows a little as it plays.</summary>
        public void Flip(Sprite[] frames, Vector2 at, float size, float t, float angle, Color tint)
        {
            if (frames == null || frames.Length == 0 || t < 0 || t >= 1) return;
            Sprite sprite = frames[Mathf.Clamp((int)(t * frames.Length), 0, frames.Length - 1)];
            if (sprite == null) return;
            if (usedFlips == flips.Count)
            {
                var go = new GameObject("Rules ink burst"); go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>(); sr.sortingOrder = BurstOrder;
                flips.Add(new FlipView { Renderer = sr });
            }
            SpriteRenderer r = flips[usedFlips++].Renderer;
            r.enabled = true; r.sprite = sprite; r.color = tint; r.sortingOrder = BurstOrder;
            float unit = Mathf.Max(.01f, Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y)), scale = size * (.85f + .3f * t) / unit;
            r.transform.position = at; r.transform.rotation = Quaternion.Euler(0, 0, angle); r.transform.localScale = new Vector3(scale, scale, 1);
        }

        /// <summary>One frame of a looping painted motif (flame, swirl, sparks...): phase in cycles picks the frame; constant size.</summary>
        public void Loop(Sprite[] frames, Vector2 at, float size, float phase, float angle, Color tint)
        {
            if (frames == null || frames.Length == 0 || size <= .01f) return;
            Sprite sprite = frames[Mathf.Clamp((int)(Mathf.Repeat(phase, 1) * frames.Length), 0, frames.Length - 1)];
            if (sprite == null) return;
            if (usedFlips == flips.Count)
            {
                var go = new GameObject("Rules ink burst"); go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>(); sr.sortingOrder = BurstOrder;
                flips.Add(new FlipView { Renderer = sr });
            }
            SpriteRenderer r = flips[usedFlips++].Renderer;
            // element loops stand on the floor: over the floor ink, under the bodies
            r.enabled = true; r.sprite = sprite; r.color = tint; r.sortingOrder = LoopOrder;
            float unit = Mathf.Max(.01f, Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y)), scale = size / unit;
            r.transform.position = at; r.transform.rotation = Quaternion.Euler(0, 0, angle); r.transform.localScale = new Vector3(scale, scale, 1);
        }

        /// <summary>Throw painted pieces: count pieces from at, outward speed, upward kick, size and life; they spin, fall and fade.</summary>
        public void Spray(Sprite[] pieces, Vector2 at, int count, float speed, float upward, float size, float life, Color tint, Vector2 bias = default)
        {
            if (pieces == null || pieces.Length == 0 || count <= 0) return;
            if (PresentationSettings.ReducedEffects) count = Mathf.Max(1, count / 3);
            for (int i = 0; i < count; i++)
            {
                Sprite sprite = pieces[Random.Range(0, pieces.Length)];
                ParticleSystem system = SystemFor(sprite);
                if (system == null) continue;
                float ang = Random.value * Mathf.PI * 2, s = speed * (.5f + Random.value);
                var emit = new ParticleSystem.EmitParams
                {
                    position = new Vector3(at.x, at.y, 0),
                    velocity = new Vector3(Mathf.Cos(ang) * s + bias.x, Mathf.Sin(ang) * s * .7f + upward * (.6f + .8f * Random.value) + bias.y, 0),
                    startSize = size * (.7f + .6f * Random.value),
                    startLifetime = life * (.7f + .6f * Random.value),
                    rotation = Random.value * 360,
                    angularVelocity = (Random.value - .5f) * 540,
                    startColor = tint,
                };
                system.Emit(emit, 1);
            }
        }

        private ParticleSystem SystemFor(Sprite sprite)
        {
            if (sprite == null) return null;
            if (systems.TryGetValue(sprite, out ParticleSystem found)) return found;
            var go = new GameObject("Rules ink pieces " + sprite.name); go.transform.SetParent(transform, false);
            var system = go.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = system.main; main.loop = false; main.playOnAwake = false; main.maxParticles = MaxParticlesPerSystem;
            main.simulationSpace = ParticleSystemSimulationSpace.World; main.gravityModifier = .55f; main.startSpeed = 0;
            var emission = system.emission; emission.enabled = false;
            var shape = system.shape; shape.enabled = false;
            var size = system.sizeOverLifetime; size.enabled = true; size.size = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe(0, .6f), new Keyframe(.15f, 1.1f), new Keyframe(1, .2f)));
            var colour = system.colorOverLifetime; colour.enabled = true;
            var gradient = new Gradient(); gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) }, new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, .6f), new GradientAlphaKey(0, 1) });
            colour.color = gradient;
            if (sprite.rect.width < sprite.texture.width - 1 || sprite.rect.height < sprite.texture.height - 1)
            {
                // a piece cut from a sheet: show only its own cell
                var sheet = system.textureSheetAnimation; sheet.enabled = true; sheet.mode = ParticleSystemAnimationMode.Sprites; sheet.AddSprite(sprite);
            }
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            var material = new Material(particleMaterial) { mainTexture = sprite.texture, name = "Rules ink piece " + sprite.name };
            renderer.sharedMaterial = material; renderer.sortingOrder = ParticleOrder; renderer.renderMode = ParticleSystemRenderMode.Billboard;
            system.Play();
            systems[sprite] = system;
            return system;
        }

        public void ClearParticles()
        {
            foreach (ParticleSystem s in systems.Values) if (s != null) s.Clear();
            foreach (ParticleSystem s in rampSystems.Values) if (s != null) s.Clear();
        }

        // ------------------------------------------------------------------ element particles with colour ramps
        /// <summary>How a particle of an element lives: its colour over life (hot core to element colour to nothing), whether it
        /// adds light or covers (smoke, dust), how gravity takes it (flames and smoke rise, sparks fall) and how it grows.</summary>
        public sealed class Ramp
        {
            public Gradient Colour; public bool Additive = true; public float Gravity; public AnimationCurve Size;
            /// <summary>Cartoon matter: four flat bands (outline, outer, inner, heart) painted on the shape; the colour ramp
            /// then only thins the shape over life.</summary>
            public Color[] Toon;
            /// <summary>Stretched along its flight (sparks and drops streak); lies on the floor under the bodies (scorch, floor light).</summary>
            public bool Stretch, Ground;
            public Ramp Streaks() { Stretch = true; return this; }
            public Ramp Floor() { Ground = true; return this; }
            public Ramp Cartoon(params Color[] bands) { Toon = bands; Additive = false; return this; }
            public Ramp(bool additive, float gravity, AnimationCurve size, params (float t, Color c)[] keys)
            {
                Additive = additive; Gravity = gravity; Size = size;
                Colour = new Gradient();
                var colours = new GradientColorKey[keys.Length]; var alphas = new GradientAlphaKey[keys.Length];
                for (int i = 0; i < keys.Length; i++) { colours[i] = new GradientColorKey(keys[i].c, keys[i].t); alphas[i] = new GradientAlphaKey(keys[i].c.a, keys[i].t); }
                Colour.SetKeys(colours, alphas);
            }
        }
        private static AnimationCurve Curve(params float[] v) { var c = new AnimationCurve(); for (int i = 0; i < v.Length; i++) c.AddKey(i / (float)(v.Length - 1), v[i]); return c; }
        private static Color C(float r, float g, float b, float a = 1) => new Color(r, g, b, a);
        /// <summary>A cartoon puff of an element: grows a little as it flies, thins away band by band.</summary>
        private static Ramp Puff(float gravity, Color outline, Color outer, Color inner, Color heart)
            => new Ramp(false, gravity, Curve(.55f, 1f, 1.15f), (0, C(1, 1, 1, .6f)), (.15f, C(1, 1, 1, 1)), (.6f, C(1, 1, 1, .85f)), (1, C(1, 1, 1, .15f))).Cartoon(outline, outer, inner, heart);
        public static readonly Dictionary<string, Ramp> Ramps = new Dictionary<string, Ramp>
        {
            { "fire",   new Ramp(true, -.25f, Curve(.7f, 1f, .75f, .35f), (0, C(1, .95f, .7f)), (.25f, C(1, .62f, .16f)), (.6f, C(.95f, .25f, .05f, .8f)), (1, C(.4f, .05f, .02f, 0))) },
            { "ember",  new Ramp(true, -.15f, Curve(1f, .8f, 0f), (0, C(1, .9f, .5f)), (.5f, C(1, .45f, .1f)), (1, C(.8f, .15f, .02f, 0))).Streaks() },
            { "smoke",  new Ramp(false, -.12f, Curve(.5f, 1f, 1.6f), (0, C(.25f, .2f, .22f, 0)), (.2f, C(.22f, .18f, .2f, .45f)), (1, C(.15f, .12f, .14f, 0))) },
            { "storm",  new Ramp(true, 0, Curve(1f, .7f, 0f), (0, C(1, 1, 1)), (.3f, C(.6f, .9f, 1)), (1, C(.2f, .45f, 1, 0))) },
            { "spark",  new Ramp(true, .9f, Curve(1f, .8f, 0f), (0, C(1, 1, .9f)), (.4f, C(1, .85f, .4f)), (1, C(1, .5f, .1f, 0))).Streaks() },
            { "ice",    new Ramp(true, .1f, Curve(.6f, 1f, .2f), (0, C(1, 1, 1)), (.35f, C(.75f, .93f, 1)), (1, C(.4f, .7f, 1, 0))) },
            { "water",  new Ramp(true, .6f, Curve(1f, .9f, .3f), (0, C(.9f, 1, 1)), (.3f, C(.4f, .85f, 1)), (1, C(.1f, .5f, .8f, 0))).Streaks() },
            { "poison", new Ramp(true, -.1f, Curve(.6f, 1f, .5f), (0, C(.85f, 1, .5f)), (.35f, C(.4f, .9f, .25f)), (1, C(.3f, .5f, .1f, 0))) },
            { "bloom",  new Ramp(true, .15f, Curve(.8f, 1f, .2f), (0, C(1, 1, .85f)), (.4f, C(.55f, .95f, .45f)), (1, C(1, .6f, .75f, 0))) },
            { "void",   new Ramp(true, -.05f, Curve(.6f, 1f, .3f), (0, C(.9f, .8f, 1)), (.4f, C(.6f, .35f, .95f)), (1, C(.25f, .1f, .5f, 0))) },
            { "light",  new Ramp(true, -.05f, Curve(.4f, 1f, 0f), (0, C(1, 1, 1)), (.4f, C(1, .9f, .55f)), (1, C(1, .7f, .2f, 0))) },
            { "sound",  new Ramp(true, -.1f, Curve(.6f, 1f, .2f), (0, C(1, .95f, 1)), (.4f, C(1, .6f, .85f)), (1, C(.8f, .4f, 1, 0))) },
            { "time",   new Ramp(true, 0, Curve(.6f, 1f, .2f), (0, C(1, 1, .95f)), (.4f, C(.9f, .85f, .6f)), (1, C(.7f, .7f, .8f, 0))) },
            { "dust",   new Ramp(false, .35f, Curve(.6f, 1f, 1.3f), (0, C(.6f, .45f, .3f, 0)), (.15f, C(.55f, .42f, .28f, .6f)), (1, C(.4f, .3f, .2f, 0))) },
            // cartoon flame on a flipbook's moving shape: dark red outline, red, orange, yellow heart; it shrinks as it dies
            { "flame",  new Ramp(false, -.05f, Curve(.85f, 1f, 1f), (0, C(1, 1, 1)), (.5f, C(1, 1, 1, .95f)), (1, C(1, 1, 1, .25f))).Cartoon(C(.55f, .07f, .06f), C(.93f, .22f, .1f), C(1, .55f, .1f), C(1, .9f, .45f)) },
            // cartoon smoke puffs: soft plum-greys in flat bands, thinning away
            { "toonsmoke", new Ramp(false, -.1f, Curve(.6f, 1f, 1.3f), (0, C(1, 1, 1, .5f)), (.2f, C(1, 1, 1, 1)), (1, C(1, 1, 1, .2f))).Cartoon(C(.3f, .26f, .31f), C(.45f, .41f, .47f), C(.58f, .54f, .6f), C(.7f, .66f, .72f)) },
            { "toonpoison", new Ramp(false, -.1f, Curve(.6f, 1f, 1.2f), (0, C(1, 1, 1, .5f)), (.2f, C(1, 1, 1, 1)), (1, C(1, 1, 1, .2f))).Cartoon(C(.13f, .3f, .1f), C(.3f, .58f, .16f), C(.5f, .8f, .22f), C(.78f, .95f, .45f)) },
            { "toondust", new Ramp(false, .1f, Curve(.6f, 1f, 1.3f), (0, C(1, 1, 1, .5f)), (.15f, C(1, 1, 1, 1)), (1, C(1, 1, 1, .2f))).Cartoon(C(.45f, .34f, .24f), C(.64f, .52f, .38f), C(.78f, .67f, .52f), C(.9f, .82f, .68f)) },
            { "toonvoid", new Ramp(false, -.05f, Curve(.6f, 1f, 1.2f), (0, C(1, 1, 1, .5f)), (.2f, C(1, 1, 1, 1)), (1, C(1, 1, 1, .2f))).Cartoon(C(.15f, .07f, .28f), C(.33f, .16f, .55f), C(.52f, .3f, .82f), C(.78f, .62f, 1f)) },
            // every element's matter as cartoon puffs in its own four bands, like the flames: outline, outer, inner, heart
            { "toonwater", Puff(.25f, C(.08f, .22f, .5f), C(.15f, .45f, .85f), C(.4f, .75f, 1f), C(.85f, .97f, 1f)) },
            { "toonice", Puff(.05f, C(.25f, .45f, .7f), C(.55f, .8f, .95f), C(.78f, .93f, 1f), C(1, 1, 1)) },
            { "toonwind", Puff(0, C(.42f, .58f, .64f), C(.72f, .88f, .9f), C(.88f, .97f, .97f), C(1, 1, 1)) },
            { "toonlight", Puff(-.05f, C(.75f, .45f, .1f), C(1, .75f, .2f), C(1, .9f, .45f), C(1, 1, .85f)) },
            { "toonbloom", Puff(.05f, C(.55f, .15f, .35f), C(.95f, .45f, .65f), C(1, .7f, .82f), C(1, .95f, .95f)) },
            { "toonsound", Puff(-.05f, C(.45f, .15f, .5f), C(.85f, .4f, .85f), C(1, .65f, .95f), C(1, .92f, 1)) },
            { "toontime", Puff(0, C(.4f, .33f, .2f), C(.7f, .6f, .38f), C(.88f, .8f, .58f), C(1, .96f, .82f)) },
            { "toonstorm", Puff(0, C(.15f, .2f, .55f), C(.35f, .55f, 1f), C(.65f, .85f, 1f), C(1, 1, 1)) },
            // a burnt patch left on the floor under a blast, fading slowly
            { "toonscorch", new Ramp(false, 0, Curve(.8f, 1f, 1f), (0, C(1, 1, 1, .2f)), (.05f, C(1, 1, 1, 1)), (.7f, C(1, 1, 1, .85f)), (1, C(1, 1, 1, .1f))).Cartoon(C(.07f, .05f, .06f, .55f), C(.13f, .09f, .09f, .6f), C(.2f, .13f, .1f, .6f), C(.28f, .17f, .1f, .55f)).Floor() },
            // the light an effect throws on the floor round it (tinted by the element), quick and flickering
            { "floorlight", new Ramp(true, 0, Curve(.8f, 1.05f, 1.1f), (0, C(1, 1, 1, 0)), (.08f, C(1, 1, 1, .9f)), (.4f, C(1, 1, 1, .5f)), (1, C(1, 1, 1, 0))).Floor() },
            { "glow",   new Ramp(true, 0, Curve(.5f, 1.15f, 1.3f), (0, C(1, 1, 1, .9f)), (.3f, C(1, 1, 1, .45f)), (1, C(1, 1, 1, 0))) },
        };
        private readonly Dictionary<string, ParticleSystem> rampSystems = new Dictionary<string, ParticleSystem>();
        private Material additiveMaterial;
        private Material toonMaterial;

        /// <summary>Emit one element particle: a texture from the particle library, its element's ramp (tinted by the optional
        /// tint, e.g. a glow in the element's colour), a velocity, a size, a life and a spin.</summary>
        public void Emit(Sprite sprite, string ramp, Vector2 at, Vector2 velocity, float size, float life, float spin, Color? tint = null)
        {
            if (sprite == null || !Ramps.TryGetValue(ramp, out Ramp r)) return;
            ParticleSystem system = RampSystem(sprite, ramp, r);
            if (system == null) return;
            var emit = new ParticleSystem.EmitParams
            {
                position = new Vector3(at.x, at.y, 0), velocity = new Vector3(velocity.x, velocity.y, 0),
                startSize3D = new Vector3(size, size, 1), startLifetime = life, rotation = Random.value * 360, angularVelocity = spin,
                startColor = tint ?? Color.white,
            };
            system.Emit(emit, 1);
        }

        /// <summary>Emit one particle with its own width and height (a floor light or a scorch lies flat: wide and low).</summary>
        public void EmitSized(Sprite sprite, string ramp, Vector2 at, Vector2 size, float life, Color? tint = null)
        {
            if (sprite == null || !Ramps.TryGetValue(ramp, out Ramp r)) return;
            ParticleSystem system = RampSystem(sprite, ramp, r);
            if (system == null) return;
            system.Emit(new ParticleSystem.EmitParams { position = new Vector3(at.x, at.y, 0), startSize3D = new Vector3(size.x, size.y, 1), startLifetime = life, rotation = 0, startColor = tint ?? Color.white }, 1);
        }

        /// <summary>Emit one flipbook particle (a filmed flame, fireball, explosion or smoke from the UL_ library): it plays its
        /// frames once over its life, keeps the frame's shape (a tall flame stays tall) and stands on its point when grounded.</summary>
        public void EmitFlip(string key, string ramp, Vector2 at, Vector2 velocity, float height, float life, float spin = 0, Color? tint = null, bool grounded = false)
        {
            Sprite[] frames = Library?.Get(key);
            if (frames == null || frames.Length == 0 || !Ramps.TryGetValue(ramp, out Ramp r)) return;
            ParticleSystem system = RampSystem(frames[0], ramp, r, frames);
            if (system == null) return;
            float aspect = frames[0].rect.width / Mathf.Max(1, frames[0].rect.height);
            var emit = new ParticleSystem.EmitParams
            {
                position = new Vector3(at.x, at.y + (grounded ? height * .45f : 0), 0), velocity = new Vector3(velocity.x, velocity.y, 0),
                startSize3D = new Vector3(height * aspect, height, 1), startLifetime = life, rotation = spin == 0 ? 0 : Random.value * 360, angularVelocity = spin,
                startColor = tint ?? Color.white,
            };
            system.Emit(emit, 1);
        }

        private ParticleSystem RampSystem(Sprite sprite, string ramp, Ramp r, Sprite[] flipbook = null)
        {
            string key = ramp + "|" + sprite.GetInstanceID();
            if (rampSystems.TryGetValue(key, out ParticleSystem found)) return found;
            if (additiveMaterial == null && Resources.Load<Shader>("Shaders/RulesAdditive") is Shader add) additiveMaterial = new Material(add) { name = "Rules additive particle" };
            var go = new GameObject("Rules ink " + ramp + " " + sprite.name); go.transform.SetParent(transform, false);
            var system = go.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = system.main; main.loop = false; main.playOnAwake = false; main.maxParticles = MaxParticlesPerSystem;
            main.simulationSpace = ParticleSystemSimulationSpace.World; main.gravityModifier = r.Gravity; main.startSpeed = 0;
            var emission = system.emission; emission.enabled = false;
            var shape = system.shape; shape.enabled = false;
            var size = system.sizeOverLifetime; size.enabled = true; size.size = new ParticleSystem.MinMaxCurve(1, r.Size);
            var colour = system.colorOverLifetime; colour.enabled = true; colour.color = r.Colour;
            var limit = system.limitVelocityOverLifetime; limit.enabled = true; limit.dampen = .08f; limit.limit = 30;
            main.startSize3D = true;
            if (flipbook == null && (sprite.rect.width < sprite.texture.width - 1 || sprite.rect.height < sprite.texture.height - 1))
            {
                // one cell of a sheet shown still (a scorch splat cut from a flipbook)
                var cell = system.textureSheetAnimation; cell.enabled = true; cell.mode = ParticleSystemAnimationMode.Sprites; cell.AddSprite(sprite);
            }
            if (flipbook != null && flipbook.Length > 1)
            {
                var sheet = system.textureSheetAnimation; sheet.enabled = true; sheet.mode = ParticleSystemAnimationMode.Sprites;
                foreach (Sprite frame in flipbook) sheet.AddSprite(frame);
                sheet.frameOverTime = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, 0, 1, 1)); sheet.cycleCount = 1;
            }
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            if (toonMaterial == null && Resources.Load<Shader>("Shaders/RulesToon") is Shader toon) toonMaterial = new Material(toon) { name = "Rules cartoon particle" };
            Material baseMaterial = r.Toon != null && toonMaterial != null ? toonMaterial : r.Additive && additiveMaterial != null ? additiveMaterial : particleMaterial;
            var made = new Material(baseMaterial) { mainTexture = sprite.texture, name = "Rules " + ramp + " " + sprite.name };
            if (r.Toon != null) for (int b = 0; b < 4; b++) made.SetColor("_C" + b, r.Toon[b]);
            renderer.sharedMaterial = made;
            renderer.sortingOrder = r.Ground ? GroundOrder + (r.Additive ? 1 : 0) : r.Additive ? ParticleOrder + 2 : ParticleOrder;
            renderer.renderMode = r.Stretch ? ParticleSystemRenderMode.Stretch : ParticleSystemRenderMode.Billboard;
            if (r.Stretch) { renderer.velocityScale = .06f; renderer.lengthScale = 1.2f; }
            system.Play();
            rampSystems[key] = system;
            return system;
        }

        /// <summary>A texture family from the particle library (K_flame, K_smoke, K_spark, K_trace, K_light...), one at random.</summary>
        public Sprite Tex(string family)
        {
            Sprite[] all = Library?.Get("K_" + family);
            return all == null || all.Length == 0 ? null : all[Random.Range(0, all.Length)];
        }
    }
}
