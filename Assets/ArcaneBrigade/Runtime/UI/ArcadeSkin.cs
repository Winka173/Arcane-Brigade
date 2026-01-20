using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>
    /// "Midnight Carnival Poster" UI skin: forward-leaning ticket buttons with a baked hard offset shadow,
    /// ink panels with a cream keyline and slanted accent tab, halftone corners. All textures are generated
    /// at runtime so the look needs no imported UI art.
    /// </summary>
    public static class ArcadeSkin
    {
        private static GUISkin skin;
        private static GUIStyle primary;
        private static GUIStyle accessionStyle;
        private static GUIStyle headerStyle;
        private static GUIStyle stampStyle;
        private static Texture2D halftone;
        private static Texture2D slant;
        private static int builtBundle = -1;
        public static int Bundle { get; set; }
        public static readonly Color Ink = new Color(0.07f, 0.06f, 0.12f);
        public static readonly Color Purple = new Color(0.97f, 0.35f, 0.28f);
        public static readonly Color Cream = new Color(1f, 0.94f, 0.76f);
        public static readonly Color Teal = new Color(0.28f, 0.94f, 0.86f);
        public static readonly Color Gold = new Color(1f, 0.78f, 0.25f);
        public static readonly Color Danger = new Color(1f, 0.16f, 0.45f);
        public static readonly Color Surface = new Color(0.11f, 0.09f, 0.18f, 0.97f);
        public static Color RarityColor(ItemRarity rarity) => rarity == ItemRarity.Mythic ? Gold : rarity == ItemRarity.Rare ? new Color(0.76f, 0.47f, 1f) : rarity == ItemRarity.Uncommon ? new Color(0.36f, 0.72f, 1f) : new Color(0.82f, 0.80f, 0.74f);
        public static string RarityName(ItemRarity rarity) => rarity == ItemRarity.Mythic ? "MYTHIC" : rarity == ItemRarity.Rare ? "RARE" : rarity == ItemRarity.Uncommon ? "UNCOMMON" : "COMMON";
        public static Color Paper => Bundle == 1 ? new Color(0.87f, 0.96f, 0.91f) : Bundle == 2 ? new Color(1f, 0.89f, 0.90f) : new Color(0.98f, 0.93f, 0.81f);
        public static Color Accent => Bundle == 1 ? new Color(0.35f, 0.90f, 0.83f) : Bundle == 2 ? new Color(1f, 0.50f, 0.66f) : new Color(0.97f, 0.35f, 0.28f);

        public static GUISkin Skin
        {
            get
            {
                if (skin != null && builtBundle == Bundle) return skin;
                builtBundle = Bundle;
                skin = Object.Instantiate(GUI.skin);
                if (ArcadeTypography.Body != null) skin.font = ArcadeTypography.Body;
                skin.button = TicketStyle(skin.button, Paper, Teal, Gold, Ink, Ink);
                skin.label.normal.textColor = Cream;
                skin.horizontalSlider.normal.background = Solid(new Color(Cream.r, Cream.g, Cream.b, 0.35f));
                skin.horizontalSlider.fixedHeight = 6;
                skin.horizontalSliderThumb.normal.background = skin.horizontalSliderThumb.hover.background = skin.horizontalSliderThumb.active.background = Solid(Accent);
                skin.horizontalSliderThumb.fixedWidth = 18; skin.horizontalSliderThumb.fixedHeight = 24;
                primary = TicketStyle(skin.button, Accent, Gold, Cream, Cream, Ink);
                return skin;
            }
        }

        /// <summary>Call-to-action button (NEW RUN, START, BUY, PLAY AGAIN). Size/font are copied from <paramref name="like"/>.</summary>
        public static GUIStyle Primary(GUIStyle like)
        {
            GUISkin _ = Skin;
            var style = new GUIStyle(primary) { font = like.font, fontSize = like.fontSize, fontStyle = like.fontStyle };
            return style;
        }

        private static GUIStyle TicketStyle(GUIStyle source, Color face, Color hover, Color active, Color text, Color hoverText)
        {
            var style = new GUIStyle(source)
            {
                border = new RectOffset(18, 18, 0, 0), padding = new RectOffset(14, 18, 2, 6), clipping = TextClipping.Overflow, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter,
                overflow = new RectOffset(0, 0, 0, 0)
            };
            style.normal.background = style.onNormal.background = TicketTexture(face, Ink, Accent);
            style.hover.background = style.onHover.background = style.focused.background = TicketTexture(hover, Ink, Ink);
            style.active.background = style.onActive.background = TicketTexture(active, Ink, Ink);
            style.normal.textColor = style.onNormal.textColor = text;
            style.hover.textColor = style.onHover.textColor = style.focused.textColor = style.active.textColor = style.onActive.textColor = hoverText;
            return style;
        }

        private static Texture2D Solid(Color color)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave };
            texture.SetPixel(0, 0, color); texture.Apply(false, true);
            return texture;
        }

        // Parallelogram leaning forward: left edge runs from x=lean (top) to x=0 (bottom). Texture y=0 is the bottom row.
        private static bool InSlant(float x, float y, float left, float right, float bottom, float top, float lean)
        {
            if (y < bottom || y > top) return false;
            float shift = lean * (y - bottom) / Mathf.Max(1, top - bottom);
            return x >= left + shift && x <= right - lean + shift;
        }

        private static Texture2D TicketTexture(Color face, Color edge, Color stripe)
        {
            const int width = 72, height = 48, lean = 12, drop = 4;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false) { name = "ArcadeTicket", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
            var pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    float px = x + 0.5f, py = y + 0.5f;
                    bool body = InSlant(px, py, 0, width - drop, drop, height, lean);
                    bool shadow = InSlant(px - drop, py + drop, 0, width - drop, drop, height, lean);
                    bool inner = InSlant(px, py, 2.5f, width - drop - 2.5f, drop + 2.5f, height - 2.5f, lean);
                    bool band = inner && InSlant(px, py, 7, 13, drop, height, lean);
                    Color color = Color.clear;
                    if (shadow) color = new Color(0.02f, 0.015f, 0.04f, 0.85f);
                    if (body) color = inner ? (band ? stripe : face) : edge;
                    pixels[y * width + x] = color;
                }
            texture.SetPixels(pixels); texture.Apply(false, true);
            return texture;
        }

        private static Texture2D Halftone()
        {
            if (halftone != null) return halftone;
            halftone = new Texture2D(12, 12, TextureFormat.RGBA32, false) { name = "ArcadeHalftone", wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave };
            var pixels = new Color[144];
            for (int y = 0; y < 12; y++)
                for (int x = 0; x < 12; x++)
                    pixels[y * 12 + x] = new Color(1, 1, 1, Mathf.Clamp01(2.6f - Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(6, 6))));
            halftone.SetPixels(pixels); halftone.Apply(false, true);
            return halftone;
        }

        /// <summary>Forward-leaning filled bar (tabs, accent strips, HUD plates).</summary>
        public static void Slant(Rect rectangle, Color color)
        {
            if (slant == null)
            {
                slant = new Texture2D(64, 16, TextureFormat.RGBA32, false) { name = "ArcadeSlant", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave };
                var pixels = new Color[64 * 16];
                for (int y = 0; y < 16; y++)
                    for (int x = 0; x < 64; x++) pixels[y * 64 + x] = InSlant(x + 0.5f, y + 0.5f, 0, 64, 0, 16, 6) ? Color.white : Color.clear;
                slant.SetPixels(pixels); slant.Apply(false, true);
            }
            if (Event.current.type != EventType.Repaint) return;
            Color previous = GUI.color;
            GUI.color = color;
            // ends keep a fixed lean (no top/bottom border, so the diagonal stretches uniformly)
            SlantStyle.Draw(rectangle, GUIContent.none, false, false, false, false);
            GUI.color = previous;
        }
        private static GUIStyle slantStyle;
        private static GUIStyle SlantStyle
        {
            get
            {
                if (slantStyle != null) return slantStyle;
                slantStyle = new GUIStyle { border = new RectOffset(6, 6, 0, 0) };
                slantStyle.normal.background = slant;
                return slantStyle;
            }
        }

        private static Texture2D glow;
        /// <summary>Soft elliptical spotlight (hero stage, selected cards).</summary>
        public static void Glow(Rect rectangle, Color color)
        {
            if (glow == null)
            {
                glow = new Texture2D(64, 64, TextureFormat.RGBA32, false) { name = "ArcadeGlow", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave };
                var pixels = new Color[64 * 64];
                for (int y = 0; y < 64; y++)
                    for (int x = 0; x < 64; x++)
                    {
                        float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(32, 32)) / 32f;
                        pixels[y * 64 + x] = new Color(1, 1, 1, Mathf.Pow(Mathf.Clamp01(1 - d), 1.8f));
                    }
                glow.SetPixels(pixels); glow.Apply(false, true);
            }
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rectangle, glow);
            GUI.color = previous;
        }

        /// <summary>Dark HUD plate behind a value (coins, eggs, timer) with a slanted accent edge.</summary>
        public static void Plate(Rect rectangle, Color edge)
        {
            Slant(new Rect(rectangle.x + 3, rectangle.y + 4, rectangle.width, rectangle.height), new Color(0, 0, 0, 0.45f));
            Slant(rectangle, new Color(Ink.r, Ink.g, Ink.b, 0.86f));
            Slant(new Rect(rectangle.x, rectangle.y, 12, rectangle.height), edge);
        }

        public static void Panel(Rect rectangle, Color color)
        {
            Color surface = PresentationSettings.HighReadability ? new Color(0.025f, 0.03f, 0.05f) : Color.Lerp(Surface, color, 0.25f);
            surface.a = 0.97f;
            Line(new Rect(rectangle.x + 7, rectangle.y + 8, rectangle.width, rectangle.height), new Color(0.01f, 0.01f, 0.02f, 0.75f));
            Line(rectangle, surface);
            if (!PresentationSettings.HighReadability && Event.current.type == EventType.Repaint && rectangle.width > 300 && rectangle.height > 200)
            {
                Rect dots = new Rect(rectangle.xMax - Mathf.Min(220, rectangle.width * 0.4f), rectangle.yMax - Mathf.Min(120, rectangle.height * 0.5f), Mathf.Min(220, rectangle.width * 0.4f) - 4, Mathf.Min(120, rectangle.height * 0.5f) - 4);
                Color previous = GUI.color;
                GUI.color = new Color(Accent.r, Accent.g, Accent.b, 0.08f);
                GUI.DrawTextureWithTexCoords(dots, Halftone(), new Rect(0, 0, dots.width / 12f, dots.height / 12f));
                GUI.color = previous;
            }
            Color keyline = new Color(Cream.r, Cream.g, Cream.b, 0.85f);
            Line(new Rect(rectangle.x, rectangle.y, rectangle.width, 2), keyline);
            Line(new Rect(rectangle.x, rectangle.yMax - 2, rectangle.width, 2), keyline);
            Line(new Rect(rectangle.x, rectangle.y, 2, rectangle.height), keyline);
            Line(new Rect(rectangle.xMax - 2, rectangle.y, 2, rectangle.height), keyline);
            if (rectangle.width > 300 && rectangle.height > 200) Slant(new Rect(rectangle.x + 14, rectangle.y - 5, Mathf.Min(150, rectangle.width * 0.3f), 10), Accent);
        }

        public static void Ticket(Rect rectangle, Color paper, Color stamp)
        {
            Line(new Rect(rectangle.x + 4, rectangle.y + 5, rectangle.width, rectangle.height), new Color(0.01f, 0.01f, 0.02f, 0.6f));
            Line(rectangle, paper);
            Line(new Rect(rectangle.x, rectangle.y, 6, rectangle.height), stamp);
            for (float horizontal = rectangle.x + 10; horizontal < rectangle.xMax - 5; horizontal += 12)
                Line(new Rect(horizontal, rectangle.yMax - 3, 5, 3), Ink);
        }

        /// <summary>Keyboard/controller focus marker: a slanted chevron left of the control plus an underline.</summary>
        public static void Focus(Rect bounds)
        {
            float pulse = PresentationSettings.ReducedMotion ? 0 : Mathf.PingPong(Time.unscaledTime * 6, 3);
            Slant(new Rect(bounds.x - 18 - pulse, bounds.y + bounds.height * 0.3f, 10, bounds.height * 0.4f), Teal);
            Slant(new Rect(bounds.x + 18, bounds.yMax - 2, bounds.width - 40, 4), Teal);
        }

        public static void Line(Rect rectangle, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rectangle, Texture2D.whiteTexture);
            GUI.color = previous;
        }
        public static void Divider(Rect rectangle, Color color)
        {
            for (float horizontal = rectangle.x; horizontal < rectangle.xMax; horizontal += 14)
                Line(new Rect(horizontal, rectangle.y, Mathf.Min(7, rectangle.xMax - horizontal), 2), color);
        }
        public static void Header(Rect rectangle, string eyebrow, string heading)
        {
            GUIStyle label = accessionStyle ?? (accessionStyle = ArcadeTypography.Style(13, false, true));
            label.normal.textColor = Accent;
            GUI.Label(new Rect(rectangle.x, rectangle.y, rectangle.width, 24), eyebrow, label);
            if (headerStyle == null)
            {
                headerStyle = ArcadeTypography.Style(36, true);
                if (ArcadeTypography.Display != null) headerStyle.font = ArcadeTypography.Display;
            }
            Vector2 size = headerStyle.CalcSize(new GUIContent(heading));
            Slant(new Rect(rectangle.x - 4, rectangle.y + 27 + size.y - 2, Mathf.Min(rectangle.width, size.x + 30), 8), new Color(Accent.r, Accent.g, Accent.b, 0.9f));
            GUI.Label(new Rect(rectangle.x, rectangle.y + 27, rectangle.width, rectangle.height - 27), heading, headerStyle);
        }
        public static void Stamp(Rect rectangle, string caption)
        {
            Divider(new Rect(rectangle.x, rectangle.y, rectangle.width, 2), Accent);
            GUIStyle label = stampStyle ?? (stampStyle = ArcadeTypography.Style(15, true));
            label.normal.textColor = Accent;
            label.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(rectangle.x, rectangle.y + 6, rectangle.width, rectangle.height - 12), caption, label);
            Divider(new Rect(rectangle.x, rectangle.yMax - 2, rectangle.width, 2), Accent);
        }
    }
}
