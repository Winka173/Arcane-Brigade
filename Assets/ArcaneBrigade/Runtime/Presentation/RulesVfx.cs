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
        private const int AreaOrder = 65, LoopOrder = 72, BurstOrder = 2150, ParticleOrder = 2160, MaxParticlesPerSystem = 300;
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

        public void ClearParticles() { foreach (ParticleSystem s in systems.Values) if (s != null) s.Clear(); }
    }
}
