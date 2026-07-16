using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The light of the ceremonies past three stars (requirement 27/10/2026): the 4th star is purple (10 s) and the 5th rainbow
    /// (16 s), both played over the merge ceremony, which stamps four purple or five rainbow stars over the friend. The look is light,
    /// not strokes: the StarAura shader (core, pillar, god rays, caustic shimmer, sparks, shock rings) over the whole view, soft glows
    /// on the friend, and a few slow petals. Without the shader only the glows and petals play.</summary>
    public sealed class GrandStarCeremony : MonoBehaviour
    {
        public static readonly Color Purple = new Color(.72f, .5f, 1f);
        /// <summary>The 4th star's two purples, bright enough to read on the teal floor.</summary>
        public static readonly InkPalette Amethyst = new InkPalette(new Color(.16f, .06f, .32f), new Color(.68f, .42f, 1f), new Color(.93f, .85f, 1f), new Color(.78f, .55f, 1f), new Color(1f, .72f, .95f)),
            Orchid = new InkPalette(new Color(.2f, .05f, .3f), new Color(.92f, .5f, 1f), new Color(1f, .88f, 1f), new Color(.95f, .6f, 1f), new Color(.75f, .6f, 1f));

        private static Material material;
        private static bool shaderTried;
        private static Mesh quad;
        private ArsenalInk ink;
        private MeshRenderer aura;
        private Vector2 at;
        private float born, life;
        private int tier, said, heard, mode;
        private bool lightOnly;
        private float power = 1, spark = 1;

        public static int Alive;
        private void Awake() { Alive++; }
        public static float Duration(int tier) => tier >= 5 ? 16f : 10f;

        /// <summary>A palette of one hue (0 to 1): the rainbow's colours.</summary>
        public static InkPalette Hue(float h)
        {
            h -= Mathf.Floor(h);
            return new InkPalette(Color.HSVToRGB(h, .7f, .22f), Color.HSVToRGB(h, .62f, 1f), Color.HSVToRGB(h, .18f, 1f), Color.HSVToRGB(h, .5f, 1f), Color.HSVToRGB((h + .08f) % 1f, .75f, 1f));
        }
        /// <summary>The colour of star k of a friend of this many stars: gold below four, purple at four, a spectrum at five.</summary>
        public static InkPalette StarPalette(int tier, int k, float clock)
            => tier >= 5 ? Hue(k / 5f + clock * .15f) : tier == 4 ? (k % 2 == 0 ? Amethyst : Orchid) : tier == 3 ? ArsenalInk.Gold : ArsenalInk.Silver;

        /// <summary>The same light, smaller, for the first stars: a half star, two or three stars (gold; a half past three in purple).</summary>
        public static void PlayLight(Vector2 at, float stars)
        {
            var go = new GameObject("StarLight");
            GrandStarCeremony c = go.AddComponent<GrandStarCeremony>();
            bool half = !Mathf.Approximately(stars, Mathf.Floor(stars));
            c.at = at; c.tier = 3; c.lightOnly = true; c.born = Time.unscaledTime;
            c.life = half ? 2.8f : stars >= 3f ? 4.8f : 3.6f;
            c.power = half ? .6f : stars >= 3f ? 1f : .8f; c.spark = half ? 1.1f : stars >= 3f ? 1.6f : 1.3f;
            c.mode = stars >= 3.5f ? 0 : 2;
        }

        public static void Play(Vector2 at, int tier)
        {
            var go = new GameObject("GrandStarCeremony");
            GrandStarCeremony c = go.AddComponent<GrandStarCeremony>();
            c.at = at; c.tier = Mathf.Clamp(tier, 4, 5); c.born = Time.unscaledTime; c.life = Duration(c.tier);
            c.mode = 0; c.power = c.tier >= 5 ? 1.25f : 1f; c.spark = c.tier >= 5 ? 2.6f : 1.6f;
        }

        private Color Tone(float h, float s = .45f) => mode == 2 ? Color.Lerp(new Color(1f, .84f, .45f), Color.white, s * .5f) : tier >= 5 ? Color.HSVToRGB(h - Mathf.Floor(h), s, 1f) : Color.Lerp(Purple, Color.white, s * .5f);
        private static float Rand(int i, int salt) { float v = Mathf.Sin(i * 12.9898f + salt * 78.233f) * 43758.5453f; return v - Mathf.Floor(v); }

        private void Word(float age, int slot, float when, string text, float h, float scale)
        {
            if (age < when || (said & (1 << slot)) != 0) return;
            said |= 1 << slot;
            ImpactWords.Pop(text, at + Vector2.up * 1.9f, Tone(h, .2f), 3, scale, -4f);
        }
        private void Sound(float age, float when, string id, int bit)
        {
            if (age < when || (heard & (1 << bit)) != 0) return;
            heard |= 1 << bit; CheckpointAudio.PlayInterfaceId(id);
        }

        public static Mesh Quad()
        {
            if (quad != null) return quad;
            quad = new Mesh { name = "StarAuraQuad", hideFlags = HideFlags.DontSave };
            quad.vertices = new[] { new Vector3(-.5f, -.5f), new Vector3(.5f, -.5f), new Vector3(-.5f, .5f), new Vector3(.5f, .5f) };
            quad.triangles = new[] { 0, 2, 1, 2, 3, 1 };
            quad.bounds = new Bounds(Vector3.zero, new Vector3(100, 100, 1));
            return quad;
        }

        private void EnsureAura()
        {
            if (aura != null || shaderTried && material == null) return;
            if (!shaderTried)
            {
                shaderTried = true;
                Shader shader = Resources.Load<Shader>("Shaders/StarAura");
                if (shader != null && shader.isSupported) material = new Material(shader) { name = "Star aura", hideFlags = HideFlags.DontSave };
            }
            if (material == null) return;
            var go = new GameObject("StarAura"); go.transform.SetParent(transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = Quad();
            aura = go.AddComponent<MeshRenderer>();
            aura.sharedMaterial = new Material(material) { hideFlags = HideFlags.DontSave };
            aura.sortingOrder = 2300; aura.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; aura.receiveShadows = false;
        }

        private static float Ease(float x) { x = Mathf.Clamp01(x); return x * x * (3 - 2 * x); }

        private void Update()
        {
            float age = Time.unscaledTime - born;
            if (age >= life) { Destroy(gameObject); return; }
            if (ink == null) ink = new ArsenalInk(transform, 66, 2410);
            EnsureAura();
            bool still = PresentationSettings.ReducedMotion, calm = PresentationSettings.ReducedEffects, rainbow = tier >= 5, soft = PresentationSettings.ReducedFlashes;
            float fade = Ease(age / .6f) * Ease((life - age) / 2f);
            float hue0 = still ? 0 : age * .16f;
            Vector2 heart = at + Vector2.up * .8f;

            // the flashes: the first as the star lands (2.6 s), the rainbow adds three more
            float open = lightOnly ? .3f : 2.6f;
            float flash = Pulse(age, open, power);
            if (soft) flash *= .35f;
            float rise = lightOnly ? Ease((age - .05f) / .7f) : Ease((age - .6f) / 2f);

            if (aura != null)
            {
                Camera view = Camera.main;
                if (view != null && view.orthographic)
                {
                    float halfH = view.orthographicSize, halfW = halfH * view.aspect;
                    aura.transform.position = new Vector3(view.transform.position.x, view.transform.position.y, 0);
                    aura.transform.localScale = new Vector3(halfW * 2 + 2, halfH * 2 + 2, 1);
                }
                Material m = aura.sharedMaterial;
                m.SetVector("_Heart", new Vector4(heart.x, heart.y, 0, 0));
                m.SetFloat("_Age", still ? 3f : age); m.SetFloat("_Rise", rise); m.SetFloat("_Flash", flash); m.SetFloat("_Rainbow", rainbow ? 1 : 0);
                m.SetFloat("_Fade", fade * (calm ? .6f : 1f)); m.SetFloat("_Calm", calm ? 1 : 0); m.SetFloat("_Ground", at.y - heart.y - .15f);
                m.SetFloat("_Mode", mode); m.SetFloat("_Power", power); m.SetFloat("_Spark", spark);
                float[] launches = lightOnly ? new[] { .3f, life > 4f ? .6f : -1f, -1f, -1f, -1f, -1f, -1f, -1f } : rainbow ? new[] { 2.6f, 2.8f, 3f, 3.2f, 3.4f, 3.6f, 3.8f, -1f } : new[] { 2.6f, 3f, 3.4f, -1f, -1f, -1f, -1f, -1f };
                var ring = new float[8];
                for (int i = 0; i < 8; i++) ring[i] = launches[i] < 0 || age < launches[i] ? -1 : age - launches[i];
                m.SetVector("_Rings", new Vector4(ring[0], ring[1], ring[2], ring[3])); m.SetVector("_Rings2", new Vector4(ring[4], ring[5], ring[6], ring[7]));
            }

            // soft light on the friend whatever the shader does, and a slow rain of petals and moons in the star's colours
            ink.Begin();
            Color glow = Tone(hue0 + .2f, .5f);
            ink.Light(heart, 2.6f + flash * 5f, glow, (.32f * rise + flash * .5f) * fade);
            ink.Light(heart + Vector2.up * .5f, 1.4f, Color.white, (.18f * rise + flash * .5f) * fade);
            if (age > 2.6f && !lightOnly)
            {
                float g = Ease((age - 2.6f) / .8f) * fade;
                for (int i = 0; i < (calm ? 8 : 22); i++)
                {
                    float cycle = 4.2f + Rand(i, 4) * 2.6f, q = ((age - 2.6f) / cycle + Rand(i, 5)) % 1f;
                    Vector2 p = new Vector2(at.x + (Rand(i, 6) - .5f) * 8f + Mathf.Sin(q * 6 + i) * .5f, at.y + 5f - q * 6.4f);
                    InkPalette pal = rainbow ? Hue(hue0 + i * .067f) : (i % 3 == 2 ? Orchid : Amethyst);
                    ink.TokenAt(i % 3 == 0 ? InkToken.Moon : i % 3 == 1 ? InkToken.Petal : InkToken.Note, pal, p, .26f, still ? 0 : q * 200f + i * 30, g * Mathf.Sin(q * Mathf.PI));
                }
            }
            ink.End();

            if (lightOnly) return;
            Sound(age, 2.55f, "up.merge", 0);
            if (rainbow)
            {
            }
        }

        /// <summary>A flash that opens at <paramref name="when"/> and dies away over a second.</summary>
        private static float Pulse(float age, float when, float strength) => age < when ? 0 : strength * Mathf.Clamp01(1 - (age - when) / 1f) * Mathf.Clamp01((age - when) / .06f + .0f) * (1 - Mathf.Clamp01((age - when) / 1f) * .2f);

        private void OnDestroy() { Alive--; ink?.Destroy(); }
    }
}
