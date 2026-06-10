using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The crafted UI style (requirement 21/10/2026: "a version where every UI element is made by hand in code"): round
    /// medallions, ink-stroke glyphs, gilt rules with a lozenge, switches, the heart and the health tube, counter sockets, all
    /// generated at runtime from signed-distance shapes in the game's palette. Rectangular plates come from CodePlate.</summary>
    public static partial class ArcadeSkin
    {
        public static readonly Color InkRed = new Color(.48f, .07f, .08f), Crimson = new Color(.86f, .13f, .17f);
        private static readonly Dictionary<int, Texture2D> roundTextures = new Dictionary<int, Texture2D>();
        private static Texture2D heartTexture;

        /// <summary>A round medallion: ink edge, gilt ring with four studs, a dark gap, a thin inner gilt line, the face in a
        /// vertical gradient with a highlight along its top.</summary>
        private static Texture2D CodeRoundTexture(CodeTone tone, int state)
        {
            int key = (int)tone * 4 + state;
            if (roundTextures.TryGetValue(key, out Texture2D cached) && cached != null) return cached;
            const int n = 96; float R = n * .5f - 1.5f, c = n * .5f;
            Color face = ToneColor(tone);
            if (state == 1) face = Color.Lerp(face, new Color(1f, .9f, .62f), .16f);
            if (state == 2) face *= .8f;
            if (state == 3) { float g = face.grayscale; face = Color.Lerp(face, new Color(g, g, g), .7f) * .9f; }
            Color light = state == 3 ? new Color(.72f, .70f, .66f) : state == 1 ? Color.Lerp(GiltLight, Color.white, .25f) : GiltLight;
            Color dark = state == 3 ? new Color(.40f, .38f, .36f) : GiltDark;
            var pixels = new Color[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float dx = x + .5f - c, dy = y + .5f - c, d = Mathf.Sqrt(dx * dx + dy * dy) - R, s = -d, ty = (y + .5f) / n;
                    float alpha = Mathf.Clamp01(.5f - d);
                    if (alpha <= 0) { pixels[y * n + x] = Color.clear; continue; }
                    Color gold = Color.Lerp(dark, light, ty), body = Color.Lerp(face * .78f, face * 1.15f, ty), col;
                    if (s < 2.2f) col = Color.Lerp(InkLine, gold, s / 2.2f);
                    else if (s < 8f) col = gold;
                    else if (s < 10f) col = Color.Lerp(InkLine, body, .4f);
                    else if (s < 12f) col = Color.Lerp(GiltMid, light, ty);
                    else { col = body; if (s < 13.6f && ty > .55f) col = Color.Lerp(col, Color.white, .22f); }
                    // four studs on the ring (north, east, south, west)
                    float a = Mathf.Repeat(Mathf.Atan2(dy, dx) + Mathf.PI * .25f, Mathf.PI * .5f) - Mathf.PI * .25f;
                    float stud = Mathf.Clamp01(3.2f - Mathf.Sqrt(Mathf.Pow((s - 5f), 2) + Mathf.Pow(a * (R - 5f), 2)));
                    if (stud > 0) col = Color.Lerp(col, Color.Lerp(light, Color.white, .35f), stud);
                    col.a = alpha; pixels[y * n + x] = col;
                }
            var texture = new Texture2D(n, n, TextureFormat.RGBA32, false) { name = "CodeRound", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
            texture.SetPixels(pixels); texture.Apply(false, true);
            roundTextures[key] = texture;
            return texture;
        }

        /// <summary>A crafted round medallion fitted (square) into the rect; returns the circle drawn.</summary>
        public static Rect CodeRound(Rect r, CodeTone tone, int state = 0)
        {
            float size = Mathf.Min(r.width, r.height);
            Rect at = new Rect(r.center.x - size * .5f, r.center.y - size * .5f, size, size);
            if (Event.current.type == EventType.Repaint) GUI.DrawTexture(at, CodeRoundTexture(tone, state), ScaleMode.StretchToFill, true);
            return at;
        }

        /// <summary>An ink stroke from a to b with round ends (glyphs, ticks, arrows).</summary>
        public static void Stroke(Vector2 a, Vector2 b, float width, Color colour)
        {
            if (Event.current.type != EventType.Repaint) return;
            Vector2 d = b - a; float length = d.magnitude;
            if (length < .01f) return;
            // a chain of round dots along the stroke, no turned matrix (requirement 24/10/2026: RotateAroundPivot misplaces strokes and
            // lozenges under a scaled HUD - the 1280-wide menus drew them along a diagonal)
            int steps = Mathf.Max(1, Mathf.CeilToInt(length / Mathf.Max(.5f, width * .45f)));
            for (int i = 0; i <= steps; i++) { Vector2 p = Vector2.Lerp(a, b, i / (float)steps); Dot(new Rect(p.x - width * .5f, p.y - width * .5f, width, width), colour); }
        }

        /// <summary>A small lozenge (gilt, with an ink edge) centred on c.</summary>
        private static Texture2D lozenge;
        public static void Diamond(Vector2 c, float size, Color fill)
        {
            if (Event.current.type != EventType.Repaint) return;
            // a painted lozenge texture (ink edge baked in) drawn straight, no turned matrix (see Stroke)
            if (lozenge == null)
            {
                const int N = 48; lozenge = new Texture2D(N, N, TextureFormat.RGBA32, false) { name = "Lozenge", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave };
                for (int y = 0; y < N; y++) for (int x = 0; x < N; x++)
                {
                    float u = Mathf.Abs((x + .5f) / N * 2 - 1) + Mathf.Abs((y + .5f) / N * 2 - 1); // 1 on the lozenge's edge
                    float a = Mathf.Clamp01((1.02f - u) * N * .5f), edge = Mathf.Clamp01((u - .8f) * 8);
                    float lift = (y + .5f) / N > .62f && u < .8f ? .25f : 0; // the upper facet a little lighter (texture rows run bottom-up)
                    Color c2 = Color.Lerp(new Color(1, 1, 1), new Color(.16f, .1f, .07f), edge); c2 = Color.Lerp(c2, Color.white, lift); c2.a = a;
                    lozenge.SetPixel(x, y, c2);
                }
                lozenge.Apply(false, true);
            }
            Color keepColour = GUI.color; GUI.color = new Color(keepColour.r * fill.r, keepColour.g * fill.g, keepColour.b * fill.b, keepColour.a * fill.a);
            GUI.DrawTexture(new Rect(c.x - size * .5f - 1, c.y - size * .5f - 1, size + 2, size + 2), lozenge);
            GUI.color = keepColour;
        }

        /// <summary>The close / back / info / pause marks as ink strokes in a face rect.</summary>
        private static void CraftedGlyph(Rect face, KitMedallion which, Color ink)
        {
            float w = Mathf.Max(2.5f, face.width * .14f);
            Vector2 c = face.center; float h = face.width * .3f;
            switch (which)
            {
                case KitMedallion.Close:
                    Stroke(c + new Vector2(-h, -h), c + new Vector2(h, h), w, ink); Stroke(c + new Vector2(-h, h), c + new Vector2(h, -h), w, ink); break;
                case KitMedallion.Back:
                    Stroke(c + new Vector2(h, 0), c + new Vector2(-h, 0), w, ink);
                    Stroke(c + new Vector2(-h, 0), c + new Vector2(-h * .2f, -h * .8f), w, ink); Stroke(c + new Vector2(-h, 0), c + new Vector2(-h * .2f, h * .8f), w, ink); break;
                case KitMedallion.Info:
                    Dot(new Rect(c.x - w * .65f, c.y - h - w * .65f, w * 1.3f, w * 1.3f), ink);
                    Stroke(c + new Vector2(0, -h * .25f), c + new Vector2(0, h), w, ink); Stroke(c + new Vector2(-h * .35f, h), c + new Vector2(h * .35f, h), w * .8f, ink); break;
                default:
                    Line(new Rect(c.x - h * .75f, c.y - h, w * 1.2f, h * 2), ink); Line(new Rect(c.x + h * .75f - w * 1.2f, c.y - h, w * 1.2f, h * 2), ink); break;
            }
        }

        private static Texture2D Heart
        {
            get
            {
                if (heartTexture != null) return heartTexture;
                const int n = 96; var pixels = new Color[n * n];
                float F(float x, float y) { float a = x * x + y * y - 1; return a * a * a - x * x * y * y * y; }
                for (int py = 0; py < n; py++)
                    for (int px = 0; px < n; px++)
                    {
                        float x = (px + .5f - n * .5f) / (n * .38f), y = (py + .5f - n * .44f) / (n * .38f);
                        float f = F(x, y), gx = (F(x + .01f, y) - F(x - .01f, y)) / .02f, gy = (F(x, y + .01f) - F(x, y - .01f)) / .02f;
                        float d = f / Mathf.Max(.001f, Mathf.Sqrt(gx * gx + gy * gy)) * n * .38f; // approximate distance in pixels
                        float alpha = Mathf.Clamp01(.5f - d);
                        if (alpha <= 0) { pixels[py * n + px] = Color.clear; continue; }
                        float s = -d, ty = (py + .5f) / n;
                        Color col = s < 2.5f ? Color.Lerp(InkLine, GiltMid, s / 2.5f) : s < 6f ? Color.Lerp(GiltDark, GiltLight, ty)
                            : Color.Lerp(Crimson * .7f, Color.Lerp(Crimson, Color.white, .15f), ty);
                        if (s > 6f && x < -.15f && y > .25f && y < .75f && x > -.75f) col = Color.Lerp(col, Color.white, .28f); // the shine
                        col.a = alpha; pixels[py * n + px] = col;
                    }
                heartTexture = new Texture2D(n, n, TextureFormat.RGBA32, false) { name = "CodeHeart", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
                heartTexture.SetPixels(pixels); heartTexture.Apply(false, true);
                return heartTexture;
            }
        }

        /// <summary>Crafted gilt rule: a double line with a lozenge in the middle and a dot at each end.</summary>
        private static void CraftedDivider(Rect r)
        {
            float y = r.center.y, m = Mathf.Min(r.height * .9f, 22);
            Color gold = new Color(GiltMid.r, GiltMid.g, GiltMid.b, .95f), dim = new Color(GiltDark.r, GiltDark.g, GiltDark.b, .8f);
            Line(new Rect(r.x + 6, y - 2, r.width * .5f - m - 6, 1.6f), gold); Line(new Rect(r.center.x + m, y - 2, r.width * .5f - m - 6, 1.6f), gold);
            Line(new Rect(r.x + 14, y + 1.5f, r.width * .5f - m - 14, 1f), dim); Line(new Rect(r.center.x + m, y + 1.5f, r.width * .5f - m - 14, 1f), dim);
            Dot(new Rect(r.x, y - 3.5f, 6, 6), gold); Dot(new Rect(r.xMax - 6, y - 3.5f, 6, 6), gold);
            Diamond(new Vector2(r.center.x, y), m * .9f, GiltLight);
            Diamond(new Vector2(r.center.x - m * .95f, y), m * .38f, GiltMid); Diamond(new Vector2(r.center.x + m * .95f, y), m * .38f, GiltMid);
        }

        /// <summary>Crafted health bar: the heart on the left, a walnut tube with a gilt rim, a glossy crimson fill and the damage
        /// trail behind it. Returns the inside of the tube (for the number).</summary>
        private static Rect CraftedHealthBar(Rect r, float fraction, float trail)
        {
            float heart = r.height;
            Rect tubeFrame = new Rect(r.x + heart * .7f, r.y + r.height * .2f, r.width - heart * .7f, r.height * .6f);
            CodePlate(tubeFrame, CodeTone.Walnut);
            float rim = CodeRim(tubeFrame);
            Rect tube = new Rect(tubeFrame.x + heart * .3f + rim, tubeFrame.y + rim, tubeFrame.width - heart * .3f - rim * 2, tubeFrame.height - rim * 2);
            if (Event.current.type == EventType.Repaint)
            {
                Line(tube, new Color(.10f, .02f, .03f, .92f));
                if (trail > fraction) Slant(new Rect(tube.x, tube.y, tube.width * Mathf.Clamp01(trail), tube.height), new Color(.95f, .55f, .25f, .55f));
                if (fraction > 0)
                {
                    Rect fill = new Rect(tube.x, tube.y, Mathf.Max(tube.height, tube.width * Mathf.Clamp01(fraction)), tube.height);
                    Slant(fill, Crimson);
                    Slant(new Rect(fill.x + 3, fill.y + 2, fill.width - 6, fill.height * .38f), new Color(1, 1, 1, .28f));
                }
                GUI.DrawTexture(new Rect(r.x, r.center.y - heart * .5f, heart, heart), Heart, ScaleMode.ScaleToFit, true);
            }
            return tube;
        }

        /// <summary>Crafted counter: a walnut plate with a round navy socket on its left.</summary>
        private static Rect CraftedCounter(Rect r, out Rect text)
        {
            Rect plate = new Rect(r.x + r.height * .5f, r.y + r.height * .12f, r.width - r.height * .5f, r.height * .76f);
            CodePlate(plate, CodeTone.Walnut);
            Rect socket = CodeRound(new Rect(r.x, r.y, r.height, r.height), CodeTone.Navy);
            text = new Rect(socket.xMax + 12, r.y, r.xMax - socket.xMax - 12 - CodeRim(plate) - 4, r.height);
            float inner = socket.width * .62f;
            return new Rect(socket.center.x - inner * .5f, socket.center.y - inner * .5f, inner, inner);
        }

        /// <summary>The painted (or crafted) close / back / info / pause medallion, art only (the caller handles the tap).</summary>
        public static Rect MedallionArt(Rect r, KitMedallion which, int state, bool lit = false)
        {
            if (state == 1 || lit) Glow(new Rect(r.x - 12, r.y - 12, r.width + 24, r.height + 24), new Color(Gold.r, Gold.g, Gold.b, lit ? .55f : .35f));
            float grow = state == 2 ? .93f : state == 1 ? 1.04f : 1;
            Rect g = new Rect(r.center.x - r.width * grow * .5f, r.center.y - r.height * grow * .5f, r.width * grow, r.height * grow);
            Sprite sprite = Crafted ? null : KitSprite("Controls/" + which);
            if (sprite != null) return Fitted(g, sprite);
            Rect at = CodeRound(g, CodeTone.Cream, state == 1 ? 1 : state == 2 ? 2 : 0);
            float inner = at.width * .56f;
            CraftedGlyph(new Rect(at.center.x - inner * .5f, at.center.y - inner * .5f, inner, inner), which, InkRed);
            return at;
        }

        /// <summary>The painted (or crafted) round button base, art only; returns the face for a glyph or a picture.</summary>
        public static Rect RoundArt(Rect r, KitRound kind, int state)
        {
            float grow = state == 2 ? .94f : state == 1 ? 1.04f : 1;
            Rect g = new Rect(r.center.x - r.width * grow * .5f, r.center.y - r.height * grow * .5f, r.width * grow, r.height * grow);
            Sprite sprite = Crafted ? null : KitSprite("Buttons/Round" + kind);
            if (sprite != null)
            {
                Rect at = Fitted(g, sprite);
                return new Rect(at.x + at.width * .22f, at.y + at.height * .28f, at.width * .56f, at.height * .56f);
            }
            CodeTone tone = kind == KitRound.Brass ? CodeTone.Gold : kind == KitRound.Oxblood ? CodeTone.Oxblood : CodeTone.Walnut;
            Rect c = CodeRound(g, tone, kind == KitRound.Disabled ? 3 : state);
            float inner = c.width * .6f;
            return new Rect(c.center.x - inner * .5f, c.center.y - inner * .5f, inner, inner);
        }
    }
}
