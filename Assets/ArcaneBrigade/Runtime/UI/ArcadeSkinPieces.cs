using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>What a rectangle means for each painted piece: the rect a caller passes is the piece's body (the solid plate);
    /// crests, curls, roses and tails are ornament bleed drawn outside it, and <see cref="ArcadeSkin.Content"/> gives the usable
    /// area inside the painted rim.</summary>
    public enum KitPiece { Button, Plate, Tab, Dialogue, Card, Tooltip, Plaque, WaveBanner }

    /// <summary>Palette of the code-drawn plates (no images): gilt gold, walnut, oxblood, night navy, cream paper.</summary>
    public enum CodeTone { Gold, Walnut, Oxblood, Navy, Cream, Green }

    public static partial class ArcadeSkin
    {
        /// <summary>Standard inner padding at 1080p, applied by <see cref="Content"/>.</summary>
        public const float PadX = 16, PadY = 12;

        private static Rect BodyOf(string key, Texture2D texture) =>
            KitBodies.TryGetValue(key, out RectInt b) ? new Rect(b.x, b.y, b.width, b.height) : new Rect(0, 0, texture.width, texture.height);

        private static string KeyOf(KitPiece piece) =>
            piece == KitPiece.Dialogue ? "Panels/Dialogue" : piece == KitPiece.Card ? "Panels/Card" : piece == KitPiece.Tooltip ? "Panels/Tooltip"
            : piece == KitPiece.Plaque ? "Panels/Plaque" : piece == KitPiece.WaveBanner ? "HUD/WaveBanner" : piece == KitPiece.Tab ? "Controls/TabInactive"
            : piece == KitPiece.Plate ? "Buttons/SecondaryNormalPlain" : "Buttons/SecondaryNormal";

        private static float CentreOf(KitPiece piece) => piece == KitPiece.Button ? ButtonCrest : piece == KitPiece.Tab ? TabCrest : piece == KitPiece.Tooltip ? TooltipTail : 0;

        /// <summary>Content inset inside the body, texture pixels (left, top, right, bottom): the painted rim plus how far the
        /// corner ornaments (roses, lanterns, scroll caps) reach in. Measured on the cut art (scratchpad uikit_metrics.py).</summary>
        private static Vector4 InsetOf(KitPiece piece, Rect body)
        {
            switch (piece)
            {
                case KitPiece.Dialogue: return new Vector4(30, 28, 30, 28);
                case KitPiece.Card: return new Vector4(16, 22, 16, 22);
                case KitPiece.Tooltip: return new Vector4(12, 10, 12, 8);
                case KitPiece.Plaque: return new Vector4(40, 10, 40, 10);
                case KitPiece.WaveBanner: return new Vector4(44, 8, 44, 9);
                case KitPiece.Tab: return new Vector4(body.width * .2f, body.height * .14f, body.width * .2f, body.height * .16f);
                default: return new Vector4(body.width * .12f, body.height * .14f, body.width * .12f, body.height * .14f);
            }
        }

        /// <summary>The border scale for this rectangle: frames from their smaller side, ribbons and buttons from their height,
        /// clamped, then lowered until the nine-slice's stretchable middle fits. False when the painting cannot be drawn cleanly
        /// at this size (the caller then draws the code-drawn plate).</summary>
        private static bool ScaleFor(KitPiece piece, Sprite sprite, string key, Rect r, out float k)
        {
            Texture2D t = sprite.texture; Rect body = BodyOf(key, t);
            float side = Mathf.Min(r.width, r.height);
            switch (piece)
            {
                case KitPiece.Dialogue: k = Mathf.Clamp(side / 760f, .34f, .7f); break;
                case KitPiece.Card: k = Mathf.Clamp(side / 640f, .4f, .85f); break;
                case KitPiece.Tooltip: k = Mathf.Clamp(side / 260f, .4f, .9f); break;
                default: k = Mathf.Clamp(r.height / body.height, .3f, 1.6f); break;
            }
            Vector4 b = sprite.border; float centre = CentreOf(piece) * t.width;
            float needW = b.x + b.z + centre - (t.width - body.width), needH = b.y + b.w - (t.height - body.height);
            if (needW > 0) k = Mathf.Min(k, r.width / needW);
            if (needH > 0) k = Mathf.Min(k, r.height / needH);
            return k >= .28f;
        }

        /// <summary>Draws a painted piece so that its body is exactly <paramref name="r"/>; ornaments bleed outside. Returns false
        /// (drawing nothing) when the art is missing or cannot size cleanly.</summary>
        private static bool DrawPiece(Rect r, KitPiece piece, string key, float limit = 99)
        {
            Sprite sprite = KitSprite(key);
            if (sprite == null || !ScaleFor(piece, sprite, key, r, out float k)) return false;
            k = Mathf.Min(k, limit);
            Texture2D t = sprite.texture; Rect body = BodyOf(key, t);
            Rect dest = new Rect(r.x - body.x * k, r.y - body.y * k, r.width + (t.width - body.width) * k, r.height + (t.height - body.height) * k);
            Sliced(dest, sprite, k, CentreOf(piece));
            return true;
        }

        /// <summary>The usable inner area of a piece drawn at <paramref name="r"/>: the painted rim and corner ornaments excluded,
        /// plus the standard padding (16 / 12 px at 1080p, less on small pieces). Code-drawn fallbacks report their own rim.</summary>
        public static Rect Content(Rect r, KitPiece piece, bool pad = true, float limit = 99)
        {
            string key = KeyOf(piece); Sprite sprite = KitSprite(key);
            Vector4 inset;
            if (sprite != null && ScaleFor(piece, sprite, key, r, out float k)) inset = InsetOf(piece, BodyOf(key, sprite.texture)) * Mathf.Min(k, limit);
            else { float rim = CodeRim(r); inset = new Vector4(rim, rim, rim, rim); }
            float px = pad ? Mathf.Min(PadX, r.width * .08f) : 0, py = pad ? Mathf.Min(PadY, r.height * .12f) : 0;
            return Rect.MinMaxRect(r.x + inset.x + px, r.y + inset.y + py, Mathf.Max(r.x + inset.x + px, r.xMax - inset.z - px), Mathf.Max(r.y + inset.y + py, r.yMax - inset.w - py));
        }

        // ---------------------------------------------------------------- code-drawn plates (no images)
        private static readonly Dictionary<int, Texture2D> codeTextures = new Dictionary<int, Texture2D>();
        private static readonly Color GiltLight = new Color(.95f, .82f, .48f), GiltMid = new Color(.79f, .63f, .29f), GiltDark = new Color(.50f, .34f, .12f), InkLine = new Color(.16f, .09f, .04f);

        private static Color ToneColor(CodeTone tone) =>
            tone == CodeTone.Gold ? new Color(.79f, .61f, .27f) : tone == CodeTone.Walnut ? new Color(.33f, .19f, .09f) : tone == CodeTone.Oxblood ? new Color(.46f, .07f, .09f)
            : tone == CodeTone.Navy ? new Color(.08f, .13f, .20f) : tone == CodeTone.Green ? new Color(.15f, .34f, .21f) : new Color(.95f, .89f, .76f);

        /// <summary>Label colour on a code-drawn plate (ink on gold and cream, cream on the dark tones).</summary>
        public static Color CodeText(CodeTone tone) => tone == CodeTone.Gold || tone == CodeTone.Cream ? KitInk : KitCream;

        private const int CodeBig = 96, CodeSmall = 40;
        private const float CodeBorderBig = 32, CodeBorderSmall = 13;

        /// <summary>A rounded plate from a signed-distance rounded rectangle: a vertical gradient body, a 1 px inner highlight, a
        /// gilt double rim with an ink edge and (on the large texture) a small inked curl in each corner. State: 0 normal, 1 lit,
        /// 2 pressed, 3 disabled.</summary>
        private static Texture2D CodeTexture(CodeTone tone, int state, bool small)
        {
            int key = ((int)tone * 4 + state) * 2 + (small ? 1 : 0);
            if (codeTextures.TryGetValue(key, out Texture2D cached) && cached != null) return cached;
            int n = small ? CodeSmall : CodeBig; float radius = small ? 10 : 24, margin = 1.5f;
            float edge = small ? 1.4f : 2f, gilt = small ? 3f : 4.6f, gap = small ? 4f : 6.2f, inner = small ? 5f : 7.6f;
            Color face = ToneColor(tone);
            if (state == 1) face = Color.Lerp(face, new Color(1f, .9f, .62f), .16f);
            if (state == 2) face *= .8f;
            if (state == 3) { float g = face.grayscale; face = Color.Lerp(face, new Color(g, g, g), .7f) * .9f; }
            Color light = state == 3 ? new Color(.72f, .70f, .66f) : state == 1 ? Color.Lerp(GiltLight, Color.white, .25f) : GiltLight;
            Color dark = state == 3 ? new Color(.40f, .38f, .36f) : GiltDark, mid = state == 3 ? new Color(.58f, .56f, .52f) : GiltMid;
            var pixels = new Color[n * n]; float half = n * .5f;
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float qx = Mathf.Abs(x + .5f - half) - (half - margin - radius), qy = Mathf.Abs(y + .5f - half) - (half - margin - radius);
                    float d = new Vector2(Mathf.Max(qx, 0), Mathf.Max(qy, 0)).magnitude + Mathf.Min(Mathf.Max(qx, qy), 0) - radius;
                    float alpha = Mathf.Clamp01(.5f - d);
                    if (alpha <= 0) { pixels[y * n + x] = Color.clear; continue; }
                    float s = -d, ty = (y + .5f) / n; // inside distance; 0 at the bottom, 1 at the top
                    Color body = Color.Lerp(face * .78f, face * 1.14f, ty);
                    Color goldGrad = Color.Lerp(dark, light, ty);
                    Color c;
                    if (s < edge) c = Color.Lerp(InkLine, goldGrad, Mathf.Clamp01(s / edge));
                    else if (s < gilt) c = goldGrad;
                    else if (s < gap) c = Color.Lerp(InkLine, body, .35f + .3f * Mathf.Clamp01((s - gilt) / (gap - gilt)));
                    else if (s < inner) c = Color.Lerp(mid, light, ty);
                    else
                    {
                        c = body;
                        if (s < inner + 1.3f && ty > .5f) c = Color.Lerp(c, Color.white, .22f); // the inner highlight along the top
                    }
                    if (!small && s > inner)
                    {
                        // a hand-inked curl in each corner: an open ring around a dot, its gap facing the corner
                        float lx = Mathf.Min(x + .5f, n - x - .5f) - 17f, ly = Mathf.Min(y + .5f, n - y - .5f) - 17f;
                        float rr = Mathf.Sqrt(lx * lx + ly * ly), angle = Mathf.Atan2(ly, lx);
                        float ring = angle < -Mathf.PI * .5f ? 0 : Mathf.Clamp01(1.25f - Mathf.Abs(rr - 5.4f));
                        float tail = ly > -1f && ly < 1.6f && lx > 4f && lx < 11f ? Mathf.Clamp01(1.1f - Mathf.Abs(ly - .3f)) * Mathf.Clamp01((11f - lx) / 3f) : 0;
                        float ink = Mathf.Max(Mathf.Max(ring, tail), Mathf.Clamp01(2.3f - rr));
                        if (ink > 0) c = Color.Lerp(c, Color.Lerp(goldGrad, light, .3f), ink * .95f);
                    }
                    c.a = alpha;
                    pixels[y * n + x] = c;
                }
            var texture = new Texture2D(n, n, TextureFormat.RGBA32, false) { name = "CodePlate", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
            texture.SetPixels(pixels); texture.Apply(false, true);
            codeTextures[key] = texture;
            return texture;
        }

        private static float CodeCorner(Rect r, bool small) => Mathf.Clamp(Mathf.Min(r.width, r.height) * (small ? .42f : .24f), 6, small ? 16 : 24);
        private static bool CodeIsSmall(Rect r) => Mathf.Min(r.width, r.height) < 56;
        /// <summary>The rim of a code-drawn plate at this size (its inner gilt line), in screen pixels.</summary>
        private static float CodeRim(Rect r) { bool small = CodeIsSmall(r); return CodeCorner(r, small) / (small ? CodeBorderSmall : CodeBorderBig) * (small ? 5f : 7.6f) + 2; }

        /// <summary>A code-drawn themed plate (crisp at any size): use it where a painted piece cannot size cleanly - small chips
        /// and tags, tiny buttons, very wide or short bars, animated highlights.</summary>
        public static void CodePlate(Rect r, CodeTone tone, int state = 0)
        {
            if (Event.current.type != EventType.Repaint || r.width < 4 || r.height < 4) return;
            bool small = CodeIsSmall(r);
            float border = small ? CodeBorderSmall : CodeBorderBig;
            SlicedTexture(r, CodeTexture(tone, state, small), new Vector4(border, border, border, border), CodeCorner(r, small) / border, 0);
        }

        /// <summary>A small code-drawn tag with its words centred (rarity, role, weapon, PAY / GET).</summary>
        public static void Tag(Rect r, string text, CodeTone tone, GUIStyle like = null, Color? colour = null)
        {
            CodePlate(r, tone);
            if (!string.IsNullOrEmpty(text)) KitCaption(new Rect(r.x + 4, r.y, r.width - 8, r.height), text, colour ?? CodeText(tone), like, true);
        }

        /// <summary>A light running round a highlighted thing (a bond the purchase lights, a selected plate, an active combo):
        /// a thin rim in the colour and two soft sparks chasing each other round it. Reduced Motion keeps the still rim.</summary>
        public static void RunningLight(Rect r, float clock, Color colour, float alpha = 1)
        {
            if (Event.current.type != EventType.Repaint) return;
            Color rim = new Color(colour.r, colour.g, colour.b, .75f * alpha);
            Line(new Rect(r.x + 1, r.y, r.width - 2, 2), rim); Line(new Rect(r.x + 1, r.yMax - 2, r.width - 2, 2), rim);
            Line(new Rect(r.x, r.y + 1, 2, r.height - 2), rim); Line(new Rect(r.xMax - 2, r.y + 1, 2, r.height - 2), rim);
            if (PresentationSettings.ReducedMotion || PresentationSettings.ReducedFlashes) return;
            float perimeter = 2 * (r.width + r.height), speed = Mathf.Clamp(perimeter * .35f, 120, 420);
            for (int k = 0; k < 2; k++)
            {
                float head = Mathf.Repeat(clock * speed + k * perimeter * .5f, perimeter);
                for (int s = 0; s < 12; s++)
                {
                    Vector2 p = OnRim(r, Mathf.Repeat(head - s * 6f, perimeter));
                    float size = Mathf.Lerp(18, 5, s / 12f);
                    Color spark = Color.Lerp(Color.white, colour, s / 12f); spark.a = alpha * (1 - s / 12f);
                    Glow(new Rect(p.x - size * .5f, p.y - size * .5f, size, size), spark);
                }
            }
        }
        private static Vector2 OnRim(Rect r, float d)
        {
            if (d < r.width) return new Vector2(r.x + d, r.y + 1);
            d -= r.width; if (d < r.height) return new Vector2(r.xMax - 1, r.y + d);
            d -= r.height; if (d < r.width) return new Vector2(r.xMax - d, r.yMax - 1);
            d -= r.width; return new Vector2(r.x + 1, r.yMax - d);
        }

        // ---------------------------------------------------------------- the skin's buttons at small sizes
        private static readonly Dictionary<Texture2D, CodeTone> plainTones = new Dictionary<Texture2D, CodeTone>();
        private static readonly Dictionary<GUIStyle, GUIStyle> fittedStyles = new Dictionary<GUIStyle, GUIStyle>();

        /// <summary>The style to draw a skin button with at this size: a painted plate cannot shrink below its caps, so a small
        /// button (under 40 px tall or 90 px wide) gets the code-drawn plate in the same tone, same font and colours.</summary>
        public static GUIStyle Fit(GUIStyle style, Rect r)
        {
            if (style == null || style.normal.background == null || r.height >= 40 && r.width >= 90) return style;
            if (!plainTones.TryGetValue(style.normal.background, out CodeTone tone)) return style;
            if (fittedStyles.TryGetValue(style, out GUIStyle small) && small != null) return small;
            small = new GUIStyle(style) { border = new RectOffset(13, 13, 13, 13), overflow = new RectOffset(0, 0, 0, 0), padding = new RectOffset(8, 8, 2, 2) };
            small.normal.background = small.onNormal.background = CodeTexture(tone, 0, true);
            small.hover.background = small.onHover.background = small.focused.background = CodeTexture(tone, 1, true);
            small.active.background = small.onActive.background = CodeTexture(tone, 2, true);
            fittedStyles[style] = small;
            return small;
        }
    }
}
