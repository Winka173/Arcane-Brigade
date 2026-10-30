using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Drawing helpers shared by the effects: jagged bolts, sparks, smoke columns. Pure functions of time, so an effect
    /// can be drawn at any moment (pause, replay, the preview renderer) without keeping state.</summary>
    public static class FxKit
    {
        public static float H(int seed, int i) { float v = Mathf.Sin(seed * 12.9898f + i * 78.233f) * 43758.5453f; return v - Mathf.Floor(v); }
        public static float EaseOut(float t) { t = Mathf.Clamp01(t); return 1 - (1 - t) * (1 - t); }
        public static float EaseOut3(float t) { t = Mathf.Clamp01(t); return 1 - (1 - t) * (1 - t) * (1 - t); }
        public static float Fade(float t, float from, float to) => 1 - Mathf.Clamp01((t - from) / Mathf.Max(.0001f, to - from));
        public static float Rise(float t, float from, float to) => Mathf.Clamp01((t - from) / Mathf.Max(.0001f, to - from));
        public static Color A(Color c, float a) { c.a = Mathf.Clamp01(a); return c; }
        public static Vector2 Dir(int seed, int i, int n, float squash = .62f) { float a = (i + H(seed, i) * .6f) / n * Mathf.PI * 2; return new Vector2(Mathf.Cos(a), Mathf.Sin(a) * squash); }

        public static readonly Color Cyan = new Color(.35f, .85f, 1f, 1), Violet = new Color(.62f, .42f, 1f, 1), White = new Color(1, 1, 1, 1);
        public static readonly Color Ink = new Color(.05f, .04f, .12f, 1);

        /// <summary>A lightning bolt as a glowing jagged line: a wide soft halo, a coloured body and a white-hot core, each a chain of
        /// beam quads along the same random walk. <paramref name="flicker"/> re-rolls the path so it crackles.</summary>
        public static void Bolt(FxBatch b, Vector2 a, Vector2 z, float width, Color tint, int seed, float jag, float alpha, int segments = 14)
        {
            if (alpha <= .003f) return;
            Vector2 d = z - a; float len = d.magnitude; if (len < .01f) return; Vector2 n = new Vector2(-d.y, d.x) / len;
            Vector2 prev = a; float swing = 0;
            for (int s = 1; s <= segments; s++)
            {
                float t = s / (float)segments; swing = Mathf.Lerp(swing, (H(seed, s) - .5f) * 2 * jag, .72f);
                Vector2 p = s == segments ? z : a + d * t + n * swing * Mathf.Sin(t * Mathf.PI) * (.4f + .6f * H(seed, s + 50));
                b.Link(prev, p, width * 3.2f, A(tint, alpha * .38f), seed + s, 1);          // halo
                b.Link(prev, p, width * 1.15f, A(tint, alpha), seed + s, .6f);                // body
                b.Link(prev, p, width * .45f, A(White, alpha), seed + s, .9f);                // core
                prev = p;
            }
        }

        /// <summary>A four-point glint with a soft halo, for sparks and stars.</summary>
        public static void Glint(FxBatch b, Vector2 at, float size, Color tint, float alpha, float turn = 0)
        {
            if (alpha <= .003f) return;
            b.Quad(FxShape.Sparkle, at, new Vector2(size, size), turn, A(tint, alpha), 0, 0, 1.5f);
        }

        /// <summary>A column of dark smoke billowing up from a point.</summary>
        public static void SmokeColumn(FxBatch b, Vector2 at, float radius, float height, float t, Color tint, int seed, int puffs = 6)
        {
            for (int k = 0; k < puffs; k++)
            {
                float born = k / (float)puffs * .45f, u = Mathf.Clamp01((t - born) / .9f); if (u <= 0 || u >= 1) continue;
                Vector2 p = at + new Vector2((H(seed, k) - .5f) * radius * .8f * (1 + u), height * EaseOut(u));
                b.Quad(FxShape.Smoke, p, radius * (.5f + .7f * u), A(tint, .6f * (1 - u)), u, seed + k);
            }
        }
    }
}
