using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>What a rectangle means for each painted piece: the rect a caller passes is the piece's body (the solid plate);
    /// crests, curls, roses and tails are ornament bleed drawn outside it, and <see cref="ArcadeSkin.Content"/> gives the usable
    /// area inside the painted rim.</summary>
    public enum KitPiece { Button, Plate, Tab, Dialogue, Card, Tooltip, Plaque, WaveBanner, Tile, EventPlate }

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
            : piece == KitPiece.Plate ? "Buttons/SecondaryNormalPlain" : piece == KitPiece.Tile ? "Info/NavyTile" : piece == KitPiece.EventPlate ? "SecretEvent/Plate" : "Buttons/SecondaryNormal";
        /// <summary>Pieces scaled from their size and nine-sliced on both axes (frames, tiles, the event plates).</summary>
        private static bool IsFrame(KitPiece p) => p == KitPiece.Dialogue || p == KitPiece.Card || p == KitPiece.Tooltip || p == KitPiece.Tile || p == KitPiece.EventPlate;

        private static float CentreOf(KitPiece piece) => piece == KitPiece.Button ? ButtonCrest : piece == KitPiece.Tab ? TabCrest : piece == KitPiece.Tooltip ? TooltipTail : piece == KitPiece.EventPlate ? .34f : 0;

        /// <summary>Content inset inside the body, texture pixels (left, top, right, bottom): the painted rim plus how far the
        /// corner ornaments (roses, lanterns, scroll caps) reach in. Measured on the cut art (scratchpad uikit_metrics.py).</summary>
        private static Vector4 InsetOf(KitPiece piece, Rect body)
        {
            switch (piece)
            {
                case KitPiece.Dialogue: return new Vector4(30, 28, 30, 28);
                case KitPiece.Card: return new Vector4(16, 22, 16, 22);
                case KitPiece.Tooltip: return new Vector4(12, 10, 12, 8);
                case KitPiece.Tile: return new Vector4(12, 11, 12, 11);
                case KitPiece.EventPlate: return new Vector4(16, 22, 16, 14); // the badge socket dips into the top
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
            if (IsFrame(piece))
            {
                float side = Mathf.Min(r.width, r.height);
                k = piece == KitPiece.Dialogue ? Mathf.Clamp(side / 760f, .34f, .7f) : piece == KitPiece.Card ? Mathf.Clamp(side / 640f, .4f, .85f)
                    : piece == KitPiece.Tile ? Mathf.Clamp(r.height / body.height, .4f, 1.4f) : piece == KitPiece.EventPlate ? Mathf.Clamp(r.height / body.height, .6f, 1.2f) : Mathf.Clamp(side / 260f, .4f, .9f);
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
            if (audit && (k < .12f || k > 2f)) Audit(key, asked, k < .12f ? "too short for the painting" : "too tall for the painting");
            return k >= .12f && k <= 2f; // the buttons are painted at twice their old size (25/10/2026): they draw down to an eighth
        }

        /// <summary>Draws a painted piece so that its body is exactly <paramref name="r"/>; ornaments bleed outside. Returns false
        /// (drawing nothing) when the art is missing or cannot size cleanly.</summary>
        private static bool DrawPiece(Rect r, KitPiece piece, string key, float limit = 99)
        {
            Sprite sprite = KitSprite(key);
            if (sprite == null || !ScaleFor(piece, sprite, key, ref r, out float k)) return false;
            if (IsFrame(piece)) k = Mathf.Min(k, limit);
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
            if (sprite != null && ScaleFor(piece, sprite, key, ref r, out float k, false)) inset = InsetOf(piece, BodyOf(key, sprite.texture)) * (IsFrame(piece) ? Mathf.Min(k, limit) : k);
            else { float rim = CodeRim(r); inset = new Vector4(rim, rim, rim, rim); }
            float px = !pad ? 0 : r.width - inset.x - inset.z > PadX * 4 ? PadX : r.width * .08f;
            float py = !pad ? 0 : r.height - inset.y - inset.w > PadY * 4 ? PadY : r.height * .12f;
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
            if (!Crafted)
            {
                float aspect = r.width / Mathf.Max(1, r.height);
                if (tone == CodeTone.Navy && InfoTile(r, state == 1)) return;
                if (tone == CodeTone.Cream && PaperArt(r, r.height < 50 ? PaperKind.LabelTag : aspect >= 6 ? PaperKind.Receipt : PaperKind.LetterRead)) return;
                if (tone == CodeTone.Green && EventBand(r, CodeTone.Green)) return;
            }
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

        /// <summary>The blink of a thing a purchase will light (a bond tile, a chip): 0..1 on a ~0.9 s cycle; under Reduced Flashes
        /// (or Reduced Motion) a slow gentle pulse instead.</summary>
        public static float BuyBlink(float clock)
        {
            bool gentle = PresentationSettings.ReducedFlashes || PresentationSettings.ReducedMotion;
            float period = gentle ? 2.4f : .9f, s = .5f + .5f * Mathf.Sin(clock * Mathf.PI * 2 / period);
            return gentle ? .35f + .3f * s : s;
        }
        /// <summary>Pulses a lit tile or chip: a glow behind it and a soft brightening over it, by <see cref="BuyBlink"/>.</summary>
        public static void BlinkLit(Rect r, float clock, Color colour)
        {
            if (Event.current.type != EventType.Repaint) return;
            float b = BuyBlink(clock);
            Glow(new Rect(r.x - 14, r.y - 12, r.width + 28, r.height + 24), new Color(colour.r, colour.g, colour.b, .1f + .35f * b));
        }
        public static void BlinkOver(Rect r, float clock)
        {
            if (Event.current.type != EventType.Repaint) return;
            Slant(new Rect(r.x + 8, r.y + 8, r.width - 16, r.height - 16), new Color(1f, .93f, .75f, .1f * BuyBlink(clock)));
        }

        /// <summary>A highlighted thing (a bond the purchase lights, a selected plate, an active combo): two twinkling stars on its
        /// ends, never a rim or a box round it (requirement 25/10/2026: painted shapes are not outlined by rectangles). Reduced
        /// Motion keeps the stars still.</summary>
        public static void RunningLight(Rect r, float clock, Color colour, float alpha = 1)
        {
            if (Event.current.type != EventType.Repaint || OnLitPainted(r)) return;
            bool still = PresentationSettings.ReducedMotion || PresentationSettings.ReducedFlashes;
            float size = Mathf.Clamp(r.height * .5f, 12, 30);
            for (int k = 0; k < 2; k++)
            {
                float beat = still ? 1 : .6f + .4f * Mathf.Sin(clock * 3.2f + k * Mathf.PI);
                Vector2 c = new Vector2(k == 0 ? r.x + 2 : r.xMax - 2, r.center.y + (still ? 0 : Mathf.Sin(clock * 2.1f + k * 1.7f) * r.height * .1f));
                float s = size * (.8f + .2f * beat);
                Glow(new Rect(c.x - s, c.y - s, s * 2, s * 2), new Color(colour.r, colour.g, colour.b, .5f * alpha * beat));
                Color core = Color.Lerp(Color.white, colour, .25f); core.a = alpha * beat;
                Glow(new Rect(c.x - s * .95f, c.y - 2, s * 1.9f, 4), core);
                Glow(new Rect(c.x - 2, c.y - s * .95f, 4, s * 1.9f), core);
                Glow(new Rect(c.x - s * .3f, c.y - s * .3f, s * .6f, s * .6f), core);
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
        /// <summary>GUI.Button through the kit: the painted plate where it fits, else the code-drawn one; the label fitted.</summary>
        public static bool KitGuiButton(Rect r, string text, GUIStyle style) => GUI.Button(r, text, Fit(style, r, text));

        /// <summary>Shrinks a style's font step by step (down to 70% of its size) until the text fits the width, then lets it wrap
        /// to two lines (requirement 22/10/2026: a label never runs past its button's caps).</summary>
        public static void FitText(GUIStyle style, string text, float width)
        {
            if (style == null || string.IsNullOrEmpty(text) || width <= 0) return;
            var content = new GUIContent(text);
            if (style.CalcSize(content).x <= width) return;
            int size = style.fontSize > 0 ? style.fontSize : 16, floor = Mathf.Max(10, Mathf.RoundToInt(size * .7f));
            style.fontSize = size;
            while (style.fontSize > floor && style.CalcSize(content).x > width) style.fontSize--;
            if (style.CalcSize(content).x > width) style.wordWrap = true;
        }

        private static readonly Dictionary<(GUIStyle, int, bool), GUIStyle> fittedLabels = new Dictionary<(GUIStyle, int, bool), GUIStyle>();
        /// <summary>The style for a skin button with this caption in this rect: the plate as <see cref="Fit(GUIStyle, Rect)"/>, and
        /// the label fitted inside the plate's content width (shrunk, then wrapped).</summary>
        public static GUIStyle Fit(GUIStyle style, Rect r, string caption)
        {
            GUIStyle s = Fit(style, r);
            if (s == null || string.IsNullOrEmpty(caption)) return s;
            float room = Content(r, KitPiece.Plate).width;
            var content = new GUIContent(caption);
            if (s.CalcSize(content).x <= room) return s;
            var probe = new GUIStyle(s) { fontSize = s.fontSize > 0 ? s.fontSize : 16, wordWrap = false };
            FitText(probe, caption, room);
            var key = (s, probe.fontSize, probe.wordWrap);
            if (!fittedLabels.TryGetValue(key, out GUIStyle fitted) || fitted == null) fittedLabels[key] = fitted = probe;
            return fitted;
        }
    }
}
