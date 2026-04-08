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
        private static bool ScaleFor(KitPiece piece, Sprite sprite, string key, ref Rect r, out float k, bool audit = true)
        {
            k = 0;
            if (Crafted) return false; // the crafted style draws every piece in code
            Rect asked = r;
            Texture2D t = sprite.texture; Rect body = BodyOf(key, t);
            Vector4 b = sprite.border; float centre = CentreOf(piece) * t.width;
            float needW = b.x + b.z + centre - (t.width - body.width), needH = b.y + b.w - (t.height - body.height);
            if (piece == KitPiece.Dialogue || piece == KitPiece.Card || piece == KitPiece.Tooltip)
            {
                float side = Mathf.Min(r.width, r.height);
                k = piece == KitPiece.Dialogue ? Mathf.Clamp(side / 760f, .34f, .7f) : piece == KitPiece.Card ? Mathf.Clamp(side / 640f, .4f, .85f) : Mathf.Clamp(side / 260f, .4f, .9f);
                if (needW > 0) k = Mathf.Min(k, r.width / needW);
                if (needH > 0) k = Mathf.Min(k, r.height / needH);
                if (k < .28f && audit) Audit(key, asked, "frame too small for its corners");
                return k >= .28f;
            }
            // ribbons, plates, buttons and tabs keep the painting's aspect: one scale from the height, the nine-slice only
            // stretches sideways. A button too narrow for its caps is drawn smaller and centred (a little only); anything else
            // that would distort gets the code-drawn plate.
            float kh = r.height / body.height, kw = needW > 0 ? r.width / needW : 99;
            k = kh;
            if (kw < kh)
            {
                if (piece != KitPiece.Button || kw < kh * .75f) { if (audit) Audit(key, asked, "too narrow for its caps at this height"); return false; }
                k = kw; r = new Rect(r.x, r.center.y - body.height * k * .5f, r.width, body.height * k);
                if (audit) Audit(key, asked, "drawn shorter than asked (narrow), centred");
            }
            if (audit && (k < .3f || k > 2f)) Audit(key, asked, k < .3f ? "too short for the painting" : "too tall for the painting");
            return k >= .3f && k <= 2f;
        }

        /// <summary>Draws a painted piece so that its body is exactly <paramref name="r"/>; ornaments bleed outside. Returns false
        /// (drawing nothing) when the art is missing or cannot size cleanly.</summary>
        private static bool DrawPiece(Rect r, KitPiece piece, string key, float limit = 99)
        {
            Sprite sprite = KitSprite(key);
            if (sprite == null || !ScaleFor(piece, sprite, key, ref r, out float k)) return false;
            if (piece == KitPiece.Dialogue || piece == KitPiece.Card || piece == KitPiece.Tooltip) k = Mathf.Min(k, limit);
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
            if (sprite != null && ScaleFor(piece, sprite, key, ref r, out float k, false)) inset = InsetOf(piece, BodyOf(key, sprite.texture)) * (piece == KitPiece.Dialogue || piece == KitPiece.Card || piece == KitPiece.Tooltip ? Mathf.Min(k, limit) : k);
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
            // the painted style: a walnut, oxblood or gold plate up to 110 px tall is the painted plain plate (it keeps its aspect;
            // DrawPiece refuses and audits any size it cannot draw cleanly)
            if (!Crafted && r.height <= 110 && (tone == CodeTone.Walnut || tone == CodeTone.Oxblood || tone == CodeTone.Gold)
                && DrawPiece(r, KitPiece.Plate, "Buttons/" + (tone == CodeTone.Gold ? "Primary" : tone == CodeTone.Oxblood ? "Danger" : "Secondary") + States[Mathf.Clamp(state, 0, 3)] + "Plain")) return;
            Audit("CodePlate " + tone, r, "code-drawn plate in the painted style");
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

        // ---------------------------------------------------------------- audit ("-ui-audit"): every painted piece that cannot be
        // drawn at the asked size, and every code-drawn plate shown in the painted style, once per piece, reason, size and caller
        private static readonly HashSet<string> audited = new HashSet<string>();
        private static int auditFlag = -1;
        private static bool Auditing { get { if (auditFlag < 0) auditFlag = System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-ui-audit") >= 0 ? 1 : 0; return auditFlag == 1; } }
        private static void Audit(string what, Rect r, string why)
        {
            if (!Auditing || Crafted || Event.current == null || Event.current.type != EventType.Repaint) return;
            string caller = Caller();
            string key = what + "|" + why + "|" + caller + "|" + Mathf.RoundToInt(r.width / 10) + "x" + Mathf.RoundToInt(r.height / 4);
            if (audited.Add(key)) Debug.Log("UIKIT_AUDIT " + what + " | " + why + " | " + Mathf.RoundToInt(r.width) + "x" + Mathf.RoundToInt(r.height) + " | " + caller);
        }
        private static string Caller()
        {
            foreach (System.Diagnostics.StackFrame frame in new System.Diagnostics.StackTrace(2, false).GetFrames() ?? new System.Diagnostics.StackFrame[0])
            {
                System.Reflection.MethodBase method = frame.GetMethod(); System.Type type = method?.DeclaringType;
                while (type != null && type.Name.StartsWith("<") && type.DeclaringType != null) type = type.DeclaringType;
                if (type == null || type == typeof(ArcadeSkin) || type == typeof(ArcadeNavigation) || type == typeof(RunHud) && method.Name == "Fill") continue;
                return type.Name + "." + method.Name;
            }
            return "?";
        }

        // ---------------------------------------------------------------- the skin's buttons
        private static readonly Dictionary<Texture2D, KitButton> codeKinds = new Dictionary<Texture2D, KitButton>();
        private static readonly Dictionary<GUIStyle, GUIStyle> bareStyles = new Dictionary<GUIStyle, GUIStyle>();
        private static CodeTone ToneOf(KitButton kind) => kind == KitButton.Primary ? CodeTone.Gold : kind == KitButton.Danger ? CodeTone.Oxblood : CodeTone.Walnut;

        /// <summary>A skin button style drawn in code (the style's own background): its corners stay a fixed size, so it never
        /// distorts at any rect.</summary>
        private static GUIStyle CodeButtonStyle(GUIStyle source, KitButton kind)
        {
            CodeTone tone = ToneOf(kind);
            var style = new GUIStyle(source)
            {
                border = new RectOffset(13, 13, 13, 13), overflow = new RectOffset(0, 0, 0, 0), padding = new RectOffset(18, 18, 4, 4),
                clipping = TextClipping.Overflow, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter
            };
            style.normal.background = style.onNormal.background = CodeTexture(tone, 0, true);
            style.hover.background = style.onHover.background = style.focused.background = CodeTexture(tone, 1, true);
            style.active.background = style.onActive.background = CodeTexture(tone, 2, true);
            codeKinds[style.normal.background] = kind;
            Color text = KitText(kind);
            style.normal.textColor = style.onNormal.textColor = style.active.textColor = style.onActive.textColor = text;
            style.hover.textColor = style.onHover.textColor = style.focused.textColor = kind == KitButton.Secondary ? new Color(1f, .88f, .60f) : text;
            return style;
        }

        /// <summary>The style to draw a skin button with in this rect. In the painted style the painted plate is drawn here (only
        /// where it keeps its aspect) and a text-only copy of the style is returned; otherwise the code-drawn style itself.</summary>
        public static GUIStyle Fit(GUIStyle style, Rect r)
        {
            if (style == null || style.normal.background == null || Crafted || !codeKinds.TryGetValue(style.normal.background, out KitButton kind)) return style;
            int state = !GUI.enabled ? 3 : State(r, true);
            if (!DrawPiece(r, KitPiece.Plate, "Buttons/" + kind + States[state] + "Plain")) return style;
            if (!bareStyles.TryGetValue(style, out GUIStyle bare) || bare == null)
            {
                bare = new GUIStyle(style);
                foreach (GUIStyleState s in new[] { bare.normal, bare.hover, bare.active, bare.focused, bare.onNormal, bare.onHover, bare.onActive, bare.onFocused }) s.background = null;
                bareStyles[style] = bare;
            }
            return bare;
        }
        /// <summary>GUI.Button through the kit: the painted plate where it fits, else the code-drawn one.</summary>
        public static bool KitGuiButton(Rect r, string text, GUIStyle style) => GUI.Button(r, text, Fit(style, r));
    }
}
