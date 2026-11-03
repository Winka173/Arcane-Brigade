using UnityEngine;

namespace ArcaneBrigade
{
    public enum PropCallKind { Warmth, Ring, Candle, Glint }

    /// <summary>How a thing in the shop that can be used calls the traveller over without words (requirement 4/11/2026: a name tag
    /// over the warm chair and the telephone read as labels pasted on the room). Each speaks in its own way: the warm chair
    /// breathes heat from its seat, the telephone shivers with a silent ring, the reserved table's candle flickers with dust
    /// falling through its light, the mirror throws glints. A soft light pools under each. Fades in and out with Show.</summary>
    public sealed class PropCall : MonoBehaviour
    {
        private const int Strokes = 4, Wisps = 3, Motes = 5;
        private Transform stage;
        private SpriteRenderer pool;
        private readonly LineRenderer[] lines = new LineRenderer[Strokes];
        private readonly SpriteRenderer[] sparks = new SpriteRenderer[Motes];
        private float shown, phase;
        private static Sprite soft, glint;
        private static Texture2D softTexture, glintTexture;
        private static Material lineMaterial;

        /// <summary>Called every frame the prop is placed; wanted false lets the call fade away. Returns the shiver to add to the
        /// prop's own turn this frame (the telephone's silent ring), in degrees.</summary>
        public float Show(SpriteRenderer prop, PropCallKind kind, bool wanted, float clock)
        {
            if (stage == null) Build();
            shown = Mathf.MoveTowards(shown, wanted && prop != null && prop.enabled ? 1 : 0, Time.unscaledDeltaTime / .4f);
            bool visible = shown > .001f && prop != null && prop.sprite != null;
            if (stage.gameObject.activeSelf != visible) stage.gameObject.SetActive(visible);
            if (!visible) return 0;
            bool still = PresentationSettings.ReducedMotion;
            float t = clock + phase;
            Bounds b = prop.bounds;
            Vector2 foot = new Vector2(b.center.x, b.min.y), top = new Vector2(b.center.x, b.max.y);
            int order = prop.sortingOrder;
            Color tone = kind == PropCallKind.Warmth ? new Color(1f, .58f, .24f) : kind == PropCallKind.Ring ? new Color(1f, .86f, .5f)
                : kind == PropCallKind.Candle ? new Color(1f, .74f, .36f) : new Color(.7f, .88f, 1f);
            // the light pools on the floor round the thing's feet; the candle's flickers, the others breathe
            float breath = still ? .85f : kind == PropCallKind.Candle ? .75f + .25f * Mathf.PerlinNoise(t * 3.1f, 7.3f) : .78f + .22f * Mathf.Sin(t * 1.9f);
            pool.transform.position = foot + Vector2.up * b.size.y * .08f;
            pool.transform.localScale = new Vector3(b.size.x * 1.9f, b.size.x * .75f, 1);
            pool.color = new Color(tone.r, tone.g, tone.b, .3f * breath * shown);
            pool.sortingOrder = order - 4;
            foreach (LineRenderer line in lines) line.enabled = false;
            foreach (SpriteRenderer spark in sparks) spark.enabled = false;
            float shiver = 0;
            switch (kind)
            {
                case PropCallKind.Warmth: Heat(b, t, order, tone, still); break;
                case PropCallKind.Ring: shiver = Ring(b, t, order, tone, still); break;
                case PropCallKind.Candle: Dust(b, t, order, tone, still); break;
                case PropCallKind.Glint: Glints(b, t, order, still); break;
            }
            return shiver * shown;
        }

        public void Hide() { shown = 0; if (stage != null) stage.gameObject.SetActive(false); }

        // three thin strands of warmth curling up off the seat, as if someone has only just stood up
        private void Heat(Bounds b, float t, int order, Color tone, bool still)
        {
            for (int i = 0; i < Wisps; i++)
            {
                LineRenderer line = lines[i]; line.enabled = true;
                float life = still ? .5f : Mathf.Repeat(t * .42f + i / (float)Wisps, 1);
                Vector2 seat = new Vector2(b.center.x + (i - 1) * b.size.x * .16f, b.min.y + b.size.y * .5f);
                for (int p = 0; p < line.positionCount; p++)
                {
                    float k = p / (float)(line.positionCount - 1), rise = (life * .6f + k * .55f) * b.size.y;
                    line.SetPosition(p, seat + new Vector2(Mathf.Sin(k * 5.2f + t * 2.3f + i * 2.1f) * b.size.x * .06f * (.4f + k), rise));
                }
                float fade = Mathf.Sin(life * Mathf.PI);
                line.startColor = new Color(tone.r, tone.g, tone.b, .55f * fade * shown); line.endColor = new Color(1f, .9f, .7f, 0);
                line.widthMultiplier = b.size.x * .05f; line.sortingOrder = order + 3;
            }
        }

        // a silent ring every few seconds: the handset shivers and two pairs of short sound arcs spring off it
        private float Ring(Bounds b, float t, int order, Color tone, bool still)
        {
            float cycle = Mathf.Repeat(t, 2.8f), ring = cycle < .7f ? cycle / .7f : -1;
            if (ring < 0) return 0;
            for (int i = 0; i < 2; i++)
            for (int side = 0; side < 2; side++)
            {
                LineRenderer line = lines[i * 2 + side]; line.enabled = true;
                float sign = side == 0 ? -1 : 1, grow = Mathf.Clamp01(ring * 1.6f - i * .3f), radius = b.size.x * (.36f + .2f * i + .1f * grow);
                Vector2 centre = new Vector2(b.center.x, b.min.y + b.size.y * .72f);
                for (int p = 0; p < line.positionCount; p++)
                {
                    float a = Mathf.Lerp(-.55f, .55f, p / (float)(line.positionCount - 1));
                    line.SetPosition(p, centre + new Vector2(sign * Mathf.Cos(a), Mathf.Sin(a)) * radius);
                }
                float fade = Mathf.Sin(grow * Mathf.PI);
                line.startColor = line.endColor = new Color(tone.r, tone.g, tone.b, .8f * fade * shown);
                line.widthMultiplier = b.size.x * .035f; line.sortingOrder = order + 3;
            }
            return still ? 0 : Mathf.Sin(ring * Mathf.PI * 9) * 5 * (1 - ring);
        }

        // the candle on the reserved table: dust drifting down slowly through its light
        private void Dust(Bounds b, float t, int order, Color tone, bool still)
        {
            for (int i = 0; i < Motes; i++)
            {
                SpriteRenderer mote = sparks[i]; mote.enabled = true; mote.sprite = soft;
                float life = still ? (i + .5f) / Motes : Mathf.Repeat(t * .16f + i / (float)Motes, 1);
                float x = b.center.x + (Mathf.PerlinNoise(i * 3.7f, 1.3f) - .5f) * b.size.x * .9f + Mathf.Sin(t * .9f + i) * b.size.x * .05f;
                mote.transform.position = new Vector3(x, Mathf.Lerp(b.max.y + b.size.y * .25f, b.min.y + b.size.y * .3f, life), 0);
                mote.transform.localScale = Vector3.one * b.size.x * .06f;
                mote.color = new Color(1f, .93f, .78f, .75f * Mathf.Sin(life * Mathf.PI) * shown);
                mote.sortingOrder = order + 3;
            }
        }

        // the mirror catches light: a four-pointed glint opens and turns on its glass, now here, now there
        private void Glints(Bounds b, float t, int order, bool still)
        {
            for (int i = 0; i < 2; i++)
            {
                SpriteRenderer spark = sparks[i]; spark.enabled = true; spark.sprite = glint;
                float cycle = still ? .5f : Mathf.Repeat(t * .45f + i * .5f, 1), open = Mathf.Clamp01(1 - Mathf.Abs(cycle - .25f) / .25f);
                int spot = Mathf.FloorToInt(t * .45f + i * .5f);
                float u = Mathf.PerlinNoise(spot * 1.7f, i * 5.1f), v = Mathf.PerlinNoise(i * 2.3f, spot * 1.1f);
                spark.transform.position = new Vector3(b.min.x + b.size.x * (.25f + .5f * u), b.min.y + b.size.y * (.45f + .4f * v), 0);
                spark.transform.rotation = Quaternion.Euler(0, 0, still ? 0 : cycle * 90);
                spark.transform.localScale = Vector3.one * b.size.x * (i == 0 ? .34f : .22f) * open;
                spark.color = new Color(1f, 1f, 1f, .95f * open * shown);
                spark.sortingOrder = order + 3;
            }
        }

        private void Build()
        {
            phase = Random.value * 10;
            stage = new GameObject("PropCall").transform;
            if (gameObject.scene.IsValid() && gameObject.scene.isLoaded) UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(stage.gameObject, gameObject.scene);
            Material glow = Resources.Load<Material>("MienCho/WaitingGlow");
            pool = Child("CallingLight", soft ??= Soft()); if (glow != null) pool.sharedMaterial = glow;
            glint ??= Glint();
            for (int i = 0; i < Motes; i++) sparks[i] = Child("CallingMote", soft);
            lineMaterial ??= new Material(Shader.Find("Sprites/Default")) { hideFlags = HideFlags.HideAndDontSave };
            for (int i = 0; i < Strokes; i++)
            {
                var go = new GameObject("CallingStroke"); go.transform.SetParent(stage, false);
                LineRenderer line = go.AddComponent<LineRenderer>();
                line.sharedMaterial = lineMaterial; line.useWorldSpace = true; line.positionCount = 9; line.numCapVertices = 4; line.numCornerVertices = 2;
                line.widthCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, .25f));
                lines[i] = line;
            }
        }

        private SpriteRenderer Child(string label, Sprite sprite)
        {
            var go = new GameObject(label); go.transform.SetParent(stage, false);
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; return renderer;
        }

        private static Sprite Soft()
        {
            softTexture = new Texture2D(64, 64, TextureFormat.RGBA32, false) { name = "PropCallLight", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
            var pixels = new Color[64 * 64];
            for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f)) / 31.5f;
                pixels[y * 64 + x] = new Color(1, 1, 1, Mathf.Pow(Mathf.Clamp01(1 - d), 2.2f));
            }
            softTexture.SetPixels(pixels); softTexture.Apply(false, true);
            return Sprite.Create(softTexture, new Rect(0, 0, 64, 64), new Vector2(.5f, .5f), 64);
        }

        private static Sprite Glint()
        {
            glintTexture = new Texture2D(64, 64, TextureFormat.RGBA32, false) { name = "PropCallGlint", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
            var pixels = new Color[64 * 64];
            for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++)
            {
                float dx = Mathf.Abs(x - 31.5f) / 31.5f, dy = Mathf.Abs(y - 31.5f) / 31.5f;
                // two thin crossing rays, brightest at the heart, and a small round core
                float rays = Mathf.Max(Mathf.Clamp01(1 - dy * 14) * (1 - dx), Mathf.Clamp01(1 - dx * 14) * (1 - dy));
                float core = Mathf.Clamp01(1 - Mathf.Sqrt(dx * dx + dy * dy) * 4);
                pixels[y * 64 + x] = new Color(1, 1, 1, Mathf.Clamp01(Mathf.Pow(rays, 1.5f) + core));
            }
            glintTexture.SetPixels(pixels); glintTexture.Apply(false, true);
            return Sprite.Create(glintTexture, new Rect(0, 0, 64, 64), new Vector2(.5f, .5f), 64);
        }

        private void OnDisable() { if (stage != null) stage.gameObject.SetActive(false); }
        private void OnDestroy() { if (stage != null) { if (Application.isPlaying) Destroy(stage.gameObject); else DestroyImmediate(stage.gameObject); } }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache() { soft = glint = null; softTexture = glintTexture = null; lineMaterial = null; }
    }
}
