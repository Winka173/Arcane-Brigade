using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The secret-event and Keeper-gift screens drawn in code (play-test item 7, 21/10/2026): a title banner, DO / LOSE /
    /// GET plates in their own tints (cream, oxblood, green-gilt) and a relic pedestal (a velvet cushion on a brass stand).</summary>
    public static partial class ArcadeSkin
    {
        /// <summary>A title banner: a walnut plate with two oxblood ribbon tails behind it and a gilt thread along its edges.</summary>
        public static void TitleBanner(Rect r, string text, GUIStyle style)
        {
            float tail = Mathf.Min(90, r.width * .12f), drop = r.height * .3f;
            CodePlate(new Rect(r.x - tail * .55f, r.y + drop, tail, r.height * .8f), CodeTone.Oxblood);
            CodePlate(new Rect(r.xMax - tail * .45f, r.y + drop, tail, r.height * .8f), CodeTone.Oxblood);
            CodePlate(r, CodeTone.Walnut, 1);
            if (Event.current.type == EventType.Repaint)
            {
                Color thread = new Color(GiltLight.r, GiltLight.g, GiltLight.b, .55f);
                float inset = Mathf.Min(r.width, r.height) * .2f + 10;
                Line(new Rect(r.x + inset, r.y + 9, r.width - inset * 2, 1), thread);
                Line(new Rect(r.x + inset, r.yMax - 10, r.width - inset * 2, 1), thread);
            }
            if (string.IsNullOrEmpty(text) || style == null) return;
            Color keep = style.normal.textColor; TextAnchor align = style.alignment; bool wrap = style.wordWrap;
            style.alignment = TextAnchor.MiddleCenter; style.wordWrap = false;
            style.normal.textColor = new Color(0, 0, 0, .55f); GUI.Label(new Rect(r.x + 2, r.y + 3, r.width, r.height), text, style);
            style.normal.textColor = KitCream; GUI.Label(r, text, style);
            style.normal.textColor = keep; style.alignment = align; style.wordWrap = wrap;
        }

        /// <summary>A DO / LOSE / GET plate: its tinted code plate and a small gilt tag with the word. Returns the text area.</summary>
        public static Rect EventRow(Rect r, string word, CodeTone tone, GUIStyle tagStyle, bool dim = false)
        {
            CodePlate(r, tone, dim ? 3 : 0);
            Rect tag = new Rect(r.x + 10, r.y + 8, 64, 26);
            Tag(tag, word, tone == CodeTone.Cream ? CodeTone.Walnut : CodeTone.Gold, tagStyle);
            return new Rect(r.x + 82, r.y + 6, r.width - 92, r.height - 12);
        }

        // ---------------------------------------------------------------- the relic pedestal
        private const int PedestalW = 256, PedestalH = 200;
        private static Texture2D pedestal;
        private static readonly Color Velvet = new Color(.44f, .07f, .12f), VelvetLight = new Color(.78f, .22f, .26f);

        /// <summary>Where a relic sits on the pedestal drawn in r (on the cushion, a little above its middle).</summary>
        public static Rect PedestalSeat(Rect r)
        {
            float size = Mathf.Min(r.width * .5f, r.height * .56f);
            return new Rect(r.center.x - size * .5f, r.y + r.height * .44f - size, size, size);
        }

        /// <summary>A velvet cushion on a curved brass stand, drawn in code once (signed distances per pixel), with a soft glint
        /// that drifts over the cushion (still under Reduced Motion).</summary>
        public static void Pedestal(Rect r, float clock)
        {
            if (Event.current.type != EventType.Repaint) return;
            Glow(new Rect(r.x - r.width * .1f, r.y + r.height * .1f, r.width * 1.2f, r.height * .9f), new Color(1f, .78f, .45f, .16f));
            Glow(new Rect(r.x + r.width * .12f, r.yMax - r.height * .16f, r.width * .76f, r.height * .2f), new Color(0, 0, 0, .45f));
            GUI.DrawTexture(r, PedestalTexture(), ScaleMode.StretchToFill, true);
            float phase = PresentationSettings.ReducedMotion ? .3f : Mathf.Repeat(clock * .18f, 1f);
            float gx = Mathf.Lerp(.28f, .72f, Mathf.SmoothStep(0, 1, Mathf.PingPong(phase * 2, 1)));
            float glint = PresentationSettings.ReducedMotion ? .35f : .25f + .2f * Mathf.Sin(clock * 2.1f);
            Glow(new Rect(r.x + r.width * gx - r.width * .1f, r.y + r.height * .3f, r.width * .2f, r.height * .1f), new Color(1f, .9f, .85f, glint));
            Glow(new Rect(r.x + r.width * .44f, r.y + r.height * .66f, r.width * .06f, r.height * .1f), new Color(1f, .95f, .75f, glint * .9f));
        }

        public static Texture2D PedestalTexture()
        {
            if (pedestal != null) return pedestal;
            var pixels = new Color[PedestalW * PedestalH];
            for (int y = 0; y < PedestalH; y++)
                for (int x = 0; x < PedestalW; x++)
                {
                    float px = x + .5f, py = PedestalH - y - .5f; // py: 0 at the top
                    Color c = Color.clear;
                    c = Over(c, Foot(px, py));
                    c = Over(c, Stem(px, py));
                    c = Over(c, Dish(px, py));
                    c = Over(c, Cushion(px, py));
                    c = Over(c, Tassel(px, py, 38)); c = Over(c, Tassel(px, py, 218));
                    pixels[y * PedestalW + x] = c;
                }
            pedestal = new Texture2D(PedestalW, PedestalH, TextureFormat.RGBA32, false) { name = "RelicPedestal", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
            pedestal.SetPixels(pixels); pedestal.Apply(false, true);
            return pedestal;
        }
        private static Color Over(Color under, Color top)
        {
            float a = top.a + under.a * (1 - top.a);
            if (a <= 0) return Color.clear;
            Color c = (top * top.a + under * under.a * (1 - top.a)) / a; c.a = a; return c;
        }
        /// <summary>Brass shaded like a turned cylinder: light from the upper left, an ink rim at the silhouette.</summary>
        private static Color Brass(float across, float edge)
        {
            float lit = Mathf.Clamp01(.62f + .38f * Mathf.Cos((across + .35f) * Mathf.PI * .5f));
            Color c = Color.Lerp(GiltDark, GiltLight, lit);
            if (Mathf.Abs(across + .4f) < .08f) c = Color.Lerp(c, new Color(1f, .96f, .8f), .55f); // the specular stripe
            return Color.Lerp(InkLine, c, Mathf.Clamp01(edge / 1.6f));
        }
        private static Color Ellipse(float px, float py, float cx, float cy, float rx, float ry, out float inside, out float nx, out float ny)
        {
            nx = (px - cx) / rx; ny = (py - cy) / ry;
            float k = Mathf.Sqrt(nx * nx + ny * ny);
            inside = (1 - k) * Mathf.Min(rx, ry); // approximate distance to the rim, in pixels
            return new Color(1, 1, 1, Mathf.Clamp01(inside + .5f));
        }
        private static Color Foot(float px, float py)
        {
            Color mask = Ellipse(px, py, 128, 182, 72, 13, out float d, out float nx, out float ny);
            if (mask.a <= 0) return Color.clear;
            Color c = Brass(nx, d); if (ny < -.2f) c = Color.Lerp(c, GiltLight, .25f);
            c.a = mask.a; return c;
        }
        private static Color Stem(float px, float py)
        {
            if (py < 112 || py > 180) return Color.clear;
            float t = (py - 112) / 68f;
            float half = 11 + 30 * t * t * t + 9 * Mathf.Exp(-Mathf.Pow((py - 138) / 5.5f, 2)) + 5 * Mathf.Exp(-Mathf.Pow((py - 120) / 4f, 2));
            float d = half - Mathf.Abs(px - 128);
            if (d <= -.5f) return Color.clear;
            Color c = Brass((px - 128) / half, d);
            if (Mathf.Abs(py - 138) < 1.2f || Mathf.Abs(py - 131) < .8f) c = Color.Lerp(c, InkLine, .45f); // turned rings
            c.a = Mathf.Clamp01(d + .5f); return c;
        }
        private static Color Dish(float px, float py)
        {
            Color mask = Ellipse(px, py, 128, 110, 94, 15, out float d, out float nx, out float ny);
            if (mask.a <= 0) return Color.clear;
            Color c = Brass(nx, d); if (ny > .2f) c = Color.Lerp(c, GiltDark, .35f);
            c.a = mask.a; return c;
        }
        private static Color Cushion(float px, float py)
        {
            // a plump oval: wider at its belly, the four corners pulled in like a stuffed cushion
            float nx = (px - 128) / 100f, ny = (py - 86) / 36f;
            float pinch = 1 + .12f * Mathf.Pow(Mathf.Abs(nx * ny), 1.2f) * 4;
            float k = Mathf.Sqrt(nx * nx + ny * ny) * pinch;
            float d = (1 - k) * 34;
            if (d <= -.5f) return Color.clear;
            float lit = Mathf.Clamp01(.55f - .45f * ny - .25f * nx);
            Color c = Color.Lerp(Velvet * .7f, VelvetLight, lit * (1 - .55f * Mathf.Clamp01(k - .55f)));
            float seam = Mathf.Abs(k - .82f);
            if (seam < .02f) c = Color.Lerp(c, new Color(.95f, .78f, .42f), (1 - seam / .02f) * .7f); // gilt piping
            float button = Mathf.Sqrt(nx * nx * 25 + ny * ny * 9);
            if (button < 1) c = Color.Lerp(c, Velvet * .5f, (1 - button) * .8f); // the tufted button
            c = Color.Lerp(InkLine, c, Mathf.Clamp01(d / 1.4f));
            c.a = Mathf.Clamp01(d + .5f); return c;
        }
        private static Color Tassel(float px, float py, float cx)
        {
            // a gold drop hanging from the cushion's corner: a short cord and a teardrop
            float cord = 1.4f - Mathf.Abs(px - cx);
            if (py > 98 && py < 112 && cord > 0) { Color g = Color.Lerp(GiltMid, GiltLight, .4f); g.a = Mathf.Clamp01(cord); return g; }
            float ty = (py - 112) / 16f; if (ty < 0 || ty > 1) return Color.clear;
            float half = 6.5f * Mathf.Sin(ty * Mathf.PI) * (1 - .35f * ty) + 1;
            float d = half - Mathf.Abs(px - cx);
            if (d <= -.5f) return Color.clear;
            Color c = Color.Lerp(Color.Lerp(GiltDark, GiltLight, Mathf.Clamp01(.8f - ty)), InkLine, Mathf.Clamp01(1.2f - d) * .6f);
            c.a = Mathf.Clamp01(d + .5f); return c;
        }
    }

    /// <summary>The secret rooms' header pictures (Art/Playtest/UI/Secret_EventVignettes_v1.png cut into
    /// Resources/UI/SecretEvent/Vignettes): one per room when painted; a room without one shows none.</summary>
    public static class SecretEventVignettes
    {
        /// <summary>The sheet's order (4 x 3).</summary>
        public static readonly string[] Names = { "ImpossibleTailor", "BrassCrate", "ContrabandExpress", "EmptyTheatre", "HostileDuet", "LeadingActor", "OwnersCrate", "PaperChorus", "SleepingGarden", "ThreadCrate", "UnderstudysMirror", "Spare" };
        private static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();
        /// <summary>Which painting each room uses (-1: none painted for it yet).</summary>
        public static int Index(SecretEventKind kind)
        {
            switch (kind)
            {
                case SecretEventKind.ImpossibleTailor: return 0;
                case SecretEventKind.ContrabandTrain: return 2;
                case SecretEventKind.EmptyTheatre: return 3;
                case SecretEventKind.SleepingGarden: return 8;
                case SecretEventKind.Understudy: return 10;
                case SecretEventKind.LostProperty: return 6;   // the owner's sealed crate
                case SecretEventKind.ThirteenthHour: return 1; // the locked brass crate (a bank's strongbox)
                case SecretEventKind.DeadLetters: return 7;    // paper figures on strings
                default: return 11;                            // the spare curtain
            }
        }
        public static Sprite For(SecretEventKind kind) => Load(Index(kind));
        public static Sprite Load(int index)
        {
            if (index < 0 || index >= Names.Length) return null;
            string name = Names[index];
            if (!cache.TryGetValue(name, out Sprite s)) cache[name] = s = Resources.Load<Sprite>("UI/SecretEvent/Vignettes/" + name);
            return s;
        }
    }
}
