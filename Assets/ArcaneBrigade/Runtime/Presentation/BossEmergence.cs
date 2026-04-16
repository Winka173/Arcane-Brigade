using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>A boss that rises from below comes up out of the dark, not out of the floorboards: a pool of violet-black ink
    /// spreads where it will stand, the body rises through its surface as a silhouette and its colour comes in as it clears,
    /// ink smoke curls up round it, drops run off it and fall back, and the pool draws in and closes after it. The rise itself
    /// (and its 1.2 s) stays UnitVisual's; this only paints around it. Reduced Motion keeps a still pool that fades.</summary>
    public sealed class BossEmergence : MonoBehaviour
    {
        private const int Wisps = 6, Drops = 8;
        public const float Tail = 0.8f; // the pool's closing after the body is up
        private SpriteRenderer pool, rim, glow;
        private readonly SpriteRenderer[] wisps = new SpriteRenderer[Wisps], drops = new SpriteRenderer[Drops];
        private static Sprite soft, drop;
        private Vector2 foot;
        private float width, height, closing = -1;
        private bool built;
        private static readonly Color Ink = new Color(0.07f, 0.04f, 0.11f);

        private void Build()
        {
            built = true;
            soft ??= Make("EmergenceSoft", 96, (x, y) => Mathf.Pow(Mathf.Clamp01(1 - Mathf.Sqrt(x * x + y * y)), 1.8f));
            drop ??= Make("EmergenceDrop", 32, (x, y) => Mathf.Clamp01((1 - Mathf.Sqrt(x * x * 2.2f + (y + .2f) * (y + .2f))) * 3));
            pool = Part("EmergencePool"); rim = Part("EmergenceRim"); glow = Part("EmergenceGlow");
            for (int i = 0; i < Wisps; i++) wisps[i] = Part("EmergenceWisp" + i);
            for (int i = 0; i < Drops; i++) drops[i] = Part("EmergenceDrop" + i);
        }
        private SpriteRenderer Part(string name)
        {
            var go = new GameObject(name); go.transform.SetParent(transform.parent, false);
            var view = go.AddComponent<SpriteRenderer>(); view.enabled = false; return view;
        }
        private static Sprite Make(string name, int size, System.Func<float, float, float> alpha)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = name, filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave };
            var pixels = new Color32[size * size]; float half = (size - 1) * .5f;
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++) pixels[y * size + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(alpha((x - half) / half, (y - half) / half)) * 255));
            texture.SetPixels32(pixels); texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), size, 0, SpriteMeshType.FullRect);
        }
        private static void Fit(SpriteRenderer view, Sprite sprite, Vector2 at, float wide, float tall, float angle, Color color, int order)
        {
            view.enabled = sprite != null && color.a > .004f; if (!view.enabled) return;
            view.sprite = sprite; view.color = color; view.sortingOrder = order;
            Rect ink = SpriteInkMetrics.BoundsOf(sprite);
            view.transform.position = at; view.transform.rotation = Quaternion.Euler(0, 0, angle);
            view.transform.localScale = new Vector3(wide / Mathf.Max(.01f, ink.width), tall / Mathf.Max(.01f, ink.height), 1);
        }

        /// <summary>Called every frame of the rise: progress 0 (under the floor) .. 1 (up). Returns the body tint.</summary>
        public Color Rise(Vector2 floor, float bodyWidth, float bodyHeight, float progress, int bodyOrder, float time)
        {
            if (!built) Build();
            foot = floor; width = Mathf.Max(1.2f, bodyWidth); height = Mathf.Max(1, bodyHeight); closing = -1;
            Paint(progress, 0, bodyOrder, time);
            // a silhouette coming up through ink, its colour arriving as it clears the surface
            float lit = Mathf.SmoothStep(0, 1, Mathf.Clamp01((progress - .25f) / .7f));
            return Color.Lerp(new Color(.16f, .12f, .22f), Color.white, lit);
        }
        /// <summary>Called once the body is up: the pool draws in and closes over Tail seconds.</summary>
        public void Settle(int bodyOrder, float time)
        {
            if (!built) return;
            if (closing < 0) closing = time;
            float shut = Mathf.Clamp01((time - closing) / Tail);
            Paint(1, shut, bodyOrder, time);
            if (shut >= 1) { foreach (SpriteRenderer view in GetParts()) if (view != null) view.enabled = false; }
        }
        private System.Collections.Generic.IEnumerable<SpriteRenderer> GetParts()
        {
            yield return pool; yield return rim; yield return glow;
            foreach (SpriteRenderer view in wisps) yield return view;
            foreach (SpriteRenderer view in drops) yield return view;
        }
        private void Paint(float progress, float shut, int bodyOrder, float time)
        {
            bool still = PresentationSettings.ReducedMotion, sparse = PresentationSettings.ReducedEffects;
            float open = Mathf.SmoothStep(0, 1, Mathf.Clamp01(progress / .25f)) * (1 - Mathf.SmoothStep(0, 1, shut));
            // drawn in code (owner 22/10/2026: no painted lava pool or purple smoke): a soft dark pool, a violet rim, soft wisps
            Sprite poolFrame = soft;
            float wide = width * 1.7f * (still ? 1 : .55f + .45f * open);
            // the pool on the floor, flattened to the room's perspective, with a faint violet rim and a low glow under it
            Fit(pool, poolFrame, foot, wide, wide * .42f, 0, new Color(Ink.r * 2.4f, Ink.g * 2.4f, Ink.b * 2.4f, .92f * open), bodyOrder - 6);
            Fit(rim, soft, foot, wide * 1.15f, wide * .5f, 0, new Color(.45f, .2f, .7f, .35f * open * (still ? 1 : .8f + .2f * Mathf.Sin(time * 6))), bodyOrder - 7);
            Fit(glow, soft, foot + Vector2.up * height * .35f, width * 1.4f, height * 1.3f, 0, new Color(.3f, .12f, .45f, .28f * open * (1 - progress * .6f)), bodyOrder - 1);
            // ink smoke curling up round the body while it rises
            for (int i = 0; i < Wisps; i++)
            {
                if (still || sparse && i >= 2) { wisps[i].enabled = false; continue; }
                float life = Mathf.Repeat(progress * 1.6f + i / (float)Wisps, 1f);
                float side = (i % 2 == 0 ? -1 : 1) * (.25f + .2f * (i / 2));
                Vector2 at = foot + new Vector2(side * width * .5f + Mathf.Sin(time * 2 + i) * .12f, life * height * 1.1f);
                Fit(wisps[i], soft, at, width * .5f * (.6f + .6f * life), width * .5f * (.6f + .6f * life), time * 30 * (i % 2 == 0 ? 1 : -1),
                    new Color(Ink.r * 3, Ink.g * 3, Ink.b * 3, Mathf.Sin(life * Mathf.PI) * .7f * open), i % 2 == 0 ? bodyOrder + 1 : bodyOrder - 2);
            }
            // drops running off the body as it clears the surface, falling back into the pool
            for (int i = 0; i < Drops; i++)
            {
                if (still || sparse && i >= 3) { drops[i].enabled = false; continue; }
                float start = .35f + i * .07f, life = Mathf.Clamp01((progress - start) / .45f);
                if (life <= 0 || life >= 1 || shut > 0) { drops[i].enabled = false; continue; }
                float x = (i / (float)(Drops - 1) - .5f) * width * .8f;
                Vector2 at = foot + new Vector2(x, height * (.75f - .6f * life * life) * (.6f + .4f * Mathf.Sin(i * 1.7f)));
                Fit(drops[i], drop, at, .09f, .14f + .1f * life, 0, new Color(Ink.r * 1.6f, Ink.g * 1.6f, Ink.b * 1.6f, .9f * (1 - life * .5f)), bodyOrder + 2);
            }
        }
        private void OnDestroy() { foreach (SpriteRenderer view in GetParts()) if (view != null) Destroy(view.gameObject); }
    }
}
