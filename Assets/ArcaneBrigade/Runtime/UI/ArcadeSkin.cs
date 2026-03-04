using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>
    /// "Lantern &amp; Letter" UI skin for Miền Chờ (the Waiting Place): night-glass panels with a thin lantern keyline,
    /// letter-paper cards, stitched dividers (letters opened and sewn shut again) and warm breathing focus light.
    /// Colour language: lantern amber = you / your company / choices; candle cream = text; moon = secondary info;
    /// ember red is reserved for hostile things (eyes, shots, bosses). All textures are generated at runtime.
    /// </summary>
    public static class ArcadeSkin
    {
        private static GUISkin skin;
        private static GUIStyle primary;
        private static GUIStyle accessionStyle;
        private static GUIStyle headerStyle;
        private static GUIStyle stampStyle;
        private static GUIStyle pillStyle;
        private static GUIStyle panelStyle;
        private static GUIStyle keylineStyle;
        private static Texture2D pill;
        private static Texture2D panelFill;
        private static Texture2D keyline;
        private static Texture2D glow;
        private static int builtBundle = -1;
        public static int Bundle { get; set; }

        public static readonly Color Ink = new Color(0.045f, 0.055f, 0.095f);            // night
        public static readonly Color Purple = new Color(0.93f, 0.62f, 0.36f);             // legacy name: warm accent
        public static readonly Color Cream = new Color(0.94f, 0.89f, 0.79f);              // candle text
        public static readonly Color Teal = new Color(0.56f, 0.80f, 0.86f);               // moonlight: secondary info, companions
        public static readonly Color Gold = new Color(1f, 0.80f, 0.44f);                  // lantern flame highlight
        public static readonly Color Danger = new Color(0.93f, 0.25f, 0.22f);             // ember eyes: hostile only
        public static readonly Color Surface = new Color(0.07f, 0.085f, 0.135f, 0.94f);   // night glass
        public static readonly Color Dawn = new Color(1f, 0.86f, 0.66f);
        // card / panel tints passed to Panel() and Fill(): night blue, never the old carnival purple
        public static readonly Color Card = new Color(0.09f, 0.11f, 0.17f, 0.95f);
        public static readonly Color CardLit = new Color(0.19f, 0.16f, 0.13f, 0.97f);   // warmed by your light
        public static readonly Color Deep = new Color(0.05f, 0.065f, 0.10f, 0.98f);
        public static readonly Color Better = new Color(0.62f, 0.90f, 0.64f);
        public static readonly Color Worse = new Color(0.95f, 0.60f, 0.52f);            // a warning, not the hostile red

        public static Color RarityColor(ItemRarity rarity) => rarity == ItemRarity.Mythic ? Gold : rarity == ItemRarity.Rare ? new Color(0.74f, 0.58f, 0.95f) : rarity == ItemRarity.Uncommon ? new Color(0.48f, 0.72f, 0.95f) : new Color(0.78f, 0.76f, 0.72f);
        public static string RarityName(ItemRarity rarity) => rarity == ItemRarity.Mythic ? "MYTHIC" : rarity == ItemRarity.Rare ? "RARE" : rarity == ItemRarity.Uncommon ? "UNCOMMON" : "COMMON";
        /// <summary>Letter paper (cosmetic bundles tint it: moonlit / rose).</summary>
        public static Color Paper => Bundle == 1 ? new Color(0.80f, 0.86f, 0.88f) : Bundle == 2 ? new Color(0.90f, 0.81f, 0.79f) : new Color(0.88f, 0.82f, 0.70f);
        /// <summary>Your light. Bundles change the lantern colour, never to the hostile red.</summary>
        public static Color Accent => Bundle == 1 ? new Color(0.58f, 0.82f, 0.90f) : Bundle == 2 ? new Color(0.96f, 0.66f, 0.70f) : new Color(0.96f, 0.71f, 0.38f);

        public static GUISkin Skin
        {
            get
            {
                if (skin != null && builtBundle == Bundle) return skin;
                builtBundle = Bundle;
                skin = Object.Instantiate(GUI.skin);
                if (ArcadeTypography.Body != null) skin.font = ArcadeTypography.Body;
                Color glass = new Color(0.085f, 0.10f, 0.16f, 0.94f);
                skin.button = ButtonStyle(skin.button, glass, new Color(Accent.r, Accent.g, Accent.b, 0.55f), Cream,
                    new Color(0.20f, 0.155f, 0.10f, 0.97f), Accent, Gold,
                    new Color(Accent.r * 0.85f, Accent.g * 0.85f, Accent.b * 0.85f, 1f), Ink);
                skin.label.normal.textColor = Cream;
                skin.horizontalSlider.normal.background = Solid(new Color(Cream.r, Cream.g, Cream.b, 0.25f));
                skin.horizontalSlider.fixedHeight = 4;
                skin.horizontalSliderThumb.normal.background = skin.horizontalSliderThumb.hover.background = skin.horizontalSliderThumb.active.background = RoundTexture(Accent, Accent, 0, 16, 7);
                skin.horizontalSliderThumb.fixedWidth = 18; skin.horizontalSliderThumb.fixedHeight = 18;
                skin.verticalScrollbar.normal.background = Solid(new Color(1, 1, 1, 0.05f));
                skin.verticalScrollbarThumb.normal.background = RoundTexture(new Color(Accent.r, Accent.g, Accent.b, 0.45f), Color.clear, 0, 16, 7);
                primary = ButtonStyle(skin.button, Accent, Gold, Ink, Gold, new Color(1f, 0.93f, 0.75f), Ink,
                    new Color(Accent.r * 0.8f, Accent.g * 0.8f, Accent.b * 0.8f, 1f), Ink);
                return skin;
            }
        }

        /// <summary>Call-to-action button (lantern-lit). Size/font are copied from <paramref name="like"/>.</summary>
        public static GUIStyle Primary(GUIStyle like)
        {
            GUISkin _ = Skin;
            return new GUIStyle(primary) { font = like.font, fontSize = like.fontSize, fontStyle = like.fontStyle };
        }

        private static GUIStyle ButtonStyle(GUIStyle source, Color face, Color edge, Color text, Color hoverFace, Color hoverEdge, Color hoverText, Color activeFace, Color activeText)
        {
            var style = new GUIStyle(source)
            {
                border = new RectOffset(10, 10, 10, 10), padding = new RectOffset(14, 14, 4, 4), clipping = TextClipping.Overflow,
                fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, overflow = new RectOffset(0, 0, 0, 0)
            };
            style.normal.background = style.onNormal.background = RoundTexture(face, edge, 1.4f, 32, 9);
            style.hover.background = style.onHover.background = style.focused.background = RoundTexture(hoverFace, hoverEdge, 2f, 32, 9);
            style.active.background = style.onActive.background = RoundTexture(activeFace, activeFace, 1.4f, 32, 9);
            style.normal.textColor = style.onNormal.textColor = text;
            style.hover.textColor = style.onHover.textColor = style.focused.textColor = hoverText;
            style.active.textColor = style.onActive.textColor = activeText;
            return style;
        }

        private static Texture2D Solid(Color color)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave };
            texture.SetPixel(0, 0, color); texture.Apply(false, true);
            return texture;
        }

        /// <summary>Rounded rectangle with an optional keyline and a faint inner top highlight (night glass).</summary>
        private static Texture2D RoundTexture(Color face, Color edge, float edgeWidth, int size, float radius)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "LanternRound", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float px = x + 0.5f, py = y + 0.5f;
                    float cx = Mathf.Clamp(px, radius, size - radius), cy = Mathf.Clamp(py, radius, size - radius);
                    float distance = radius - Vector2.Distance(new Vector2(px, py), new Vector2(cx, cy)); // >0 inside
                    float coverage = Mathf.Clamp01(distance + 0.5f);
                    if (coverage <= 0) { pixels[y * size + x] = Color.clear; continue; }
                    Color color = face;
                    if (y > size - radius * 0.9f && edgeWidth > 0) color = Color.Lerp(color, new Color(1, 1, 1, color.a), 0.05f);
                    if (edgeWidth > 0 && distance < edgeWidth) color = Color.Lerp(color, edge, Mathf.Clamp01(edgeWidth - distance + 0.5f) * edge.a + (1 - edge.a) * 0);
                    color.a *= coverage;
                    pixels[y * size + x] = color;
                }
            texture.SetPixels(pixels); texture.Apply(false, true);
            return texture;
        }

        private static GUIStyle Nine(ref GUIStyle style, ref Texture2D texture, Color face, Color edge, float edgeWidth)
        {
            if (style != null) return style;
            texture = RoundTexture(face, edge, edgeWidth, 32, 8);
            style = new GUIStyle { border = new RectOffset(10, 10, 10, 10) };
            style.normal.background = texture;
            return style;
        }

        private static void DrawTinted(GUIStyle style, Rect rectangle, Color color)
        {
            if (Event.current.type != EventType.Repaint) return;
            Color previous = GUI.color;
            GUI.color = Faded(color, previous);
            style.Draw(rectangle, GUIContent.none, false, false, false, false);
            GUI.color = previous;
        }

        /// <summary>Soft rounded bar (tags, plates, strips). Kept under the old name so every caller restyles at once.</summary>
        public static void Slant(Rect rectangle, Color color)
        {
            // a nine-slice cannot draw anything smaller than its borders; small marks become soft round dots instead
            if (rectangle.width < 20 || rectangle.height < 20) { Dot(rectangle, color); return; }
            DrawTinted(Nine(ref pillStyle, ref pill, Color.white, Color.white, 0), rectangle, color);
        }

        private static Texture2D dot;
        /// <summary>Small round mark (eyes, flames, notches, pips): a hard-edged disc stretched to the rectangle.</summary>
        public static void Dot(Rect rectangle, Color color)
        {
            if (Event.current.type != EventType.Repaint) return;
            if (dot == null)
            {
                dot = new Texture2D(32, 32, TextureFormat.RGBA32, false) { name = "LanternDot", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave };
                var pixels = new Color[32 * 32];
                for (int y = 0; y < 32; y++)
                    for (int x = 0; x < 32; x++)
                        pixels[y * 32 + x] = new Color(1, 1, 1, Mathf.Clamp01(15.5f - Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(16, 16))));
                dot.SetPixels(pixels); dot.Apply(false, true);
            }
            Color previous = GUI.color;
            GUI.color = Faded(color, previous);
            GUI.DrawTexture(rectangle, dot);
            GUI.color = previous;
        }

        /// <summary>Soft elliptical light (lantern glow, spotlight, focus).</summary>
        public static void Glow(Rect rectangle, Color color)
        {
            if (glow == null)
            {
                glow = new Texture2D(64, 64, TextureFormat.RGBA32, false) { name = "LanternGlow", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave };
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
            GUI.color = Faded(color, previous);
            GUI.DrawTexture(rectangle, glow);
            GUI.color = previous;
        }

        /// <summary>Dark glass plate behind a value (HP, coins, timer) with a thin coloured edge light.</summary>
        public static void Plate(Rect rectangle, Color edge)
        {
            Slant(new Rect(rectangle.x + 2, rectangle.y + 3, rectangle.width, rectangle.height), new Color(0, 0, 0, 0.35f));
            Slant(rectangle, new Color(Ink.r, Ink.g, Ink.b, 0.82f));
            Line(new Rect(rectangle.x + 6, rectangle.y + rectangle.height * 0.22f, 3, rectangle.height * 0.56f), edge);
            Glow(new Rect(rectangle.x - 8, rectangle.y, 34, rectangle.height), new Color(edge.r, edge.g, edge.b, 0.22f));
        }

        /// <summary>Night-glass panel with a faint lantern keyline and stitched seams.</summary>
        public static void Panel(Rect rectangle, Color color)
        {
            bool readable = PresentationSettings.HighReadability;
            Color surface = readable ? new Color(0.02f, 0.025f, 0.04f, 0.98f) : Color.Lerp(Surface, new Color(color.r, color.g, color.b, Surface.a), 0.2f);
            surface.a = readable ? 0.98f : 0.94f;
            Slant(new Rect(rectangle.x + 4, rectangle.y + 6, rectangle.width, rectangle.height), new Color(0, 0, 0, 0.45f));
            DrawTinted(Nine(ref panelStyle, ref panelFill, Color.white, Color.white, 0), rectangle, surface);
            DrawTinted(Nine(ref keylineStyle, ref keyline, Color.clear, Color.white, 1.2f), rectangle, new Color(Accent.r, Accent.g, Accent.b, readable ? 0.75f : 0.38f));
            if (rectangle.width > 140 && rectangle.height > 70)
            {
                Color stitch = new Color(Accent.r, Accent.g, Accent.b, 0.22f);
                Divider(new Rect(rectangle.x + 16, rectangle.y + 7, rectangle.width - 32, 1), stitch);
                Divider(new Rect(rectangle.x + 16, rectangle.yMax - 8, rectangle.width - 32, 1), stitch);
            }
            if (rectangle.width > 300 && rectangle.height > 200)
                Glow(new Rect(rectangle.x - 10, rectangle.y - 10, 44, 44), new Color(Accent.r, Accent.g, Accent.b, 0.35f));
        }

        /// <summary>Letter-paper card (speech, notes, price slips) with a coloured margin and stitched hem.</summary>
        public static void Ticket(Rect rectangle, Color paper, Color stamp)
        {
            Slant(new Rect(rectangle.x + 3, rectangle.y + 5, rectangle.width, rectangle.height), new Color(0, 0, 0, 0.45f));
            Slant(new Rect(rectangle.x, rectangle.y, rectangle.width, rectangle.height - 4), paper);
            TornEdge(new Rect(rectangle.x + 2, rectangle.yMax - 8, rectangle.width - 4, 9), paper);
            Line(new Rect(rectangle.x + 5, rectangle.y + 6, 3, rectangle.height - 16), stamp);
        }

        private static Texture2D torn;
        /// <summary>Ragged bottom of a page torn from a notebook: an irregular saw edge tiled along the rectangle.</summary>
        public static void TornEdge(Rect rectangle, Color paper)
        {
            if (Event.current.type != EventType.Repaint) return;
            if (torn == null)
            {
                const int w = 48, h = 12;
                torn = new Texture2D(w, h, TextureFormat.RGBA32, false) { name = "TornEdge", wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave };
                var pixels = new Color[w * h];
                for (int x = 0; x < w; x++)
                {
                    // irregular teeth: two saw waves of different periods
                    float tooth = Mathf.Abs(Mathf.Repeat(x / 7f, 1f) - 0.5f) * 2f * 0.6f + Mathf.Abs(Mathf.Repeat((x + 3) / 11f, 1f) - 0.5f) * 2f * 0.4f;
                    float edge = h * (0.25f + 0.7f * tooth); // paper fills from the top down to the edge
                    for (int y = 0; y < h; y++)
                    {
                        float fromTop = h - 1 - y + 0.5f;
                        pixels[y * w + x] = new Color(1, 1, 1, Mathf.Clamp01(edge - fromTop + 0.5f));
                    }
                }
                torn.SetPixels(pixels); torn.Apply(false, true);
            }
            Color previous = GUI.color;
            GUI.color = Faded(paper, previous);
            GUI.DrawTextureWithTexCoords(rectangle, torn, new Rect(0, 0, rectangle.width / 48f, 1));
            GUI.color = previous;
        }

        private static Texture2D wax;
        private static GUIStyle sealStyle;
        public static readonly Color Wax = new Color(0.50f, 0.11f, 0.12f); // oxblood, darker than the hostile ember
        /// <summary>A wax seal pressed on a letter or label, with an embossed letter.</summary>
        public static void Seal(Vector2 centre, float size, string letter)
        {
            if (wax == null)
            {
                const int n = 64;
                wax = new Texture2D(n, n, TextureFormat.RGBA32, false) { name = "WaxSeal", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave };
                var pixels = new Color[n * n];
                for (int y = 0; y < n; y++)
                    for (int x = 0; x < n; x++)
                    {
                        Vector2 p = new Vector2(x + 0.5f - n / 2f, y + 0.5f - n / 2f);
                        float angle = Mathf.Atan2(p.y, p.x);
                        float radius = n * 0.46f + Mathf.Sin(angle * 9f) * 1.2f + Mathf.Sin(angle * 4f + 1f) * 0.9f; // poured, uneven rim
                        float d = p.magnitude;
                        float alpha = Mathf.Clamp01(radius - d + 0.5f);
                        float rim = Mathf.Clamp01((d - (n * 0.30f)) / (n * 0.12f));
                        float light = 1.05f - 0.35f * rim + 0.25f * Mathf.Clamp01(-(p.x + p.y) / n);
                        pixels[y * n + x] = new Color(light, light, light, alpha);
                    }
                wax.SetPixels(pixels); wax.Apply(false, true);
            }
            Color previous = GUI.color;
            Rect rect = new Rect(centre.x - size * 0.5f, centre.y - size * 0.5f, size, size);
            GUI.color = Faded(new Color(0, 0, 0, 0.45f), previous);
            GUI.DrawTexture(new Rect(rect.x + 2, rect.y + 3, size, size), wax);
            GUI.color = Faded(Wax, previous);
            GUI.DrawTexture(rect, wax);
            GUI.color = previous;
            if (string.IsNullOrEmpty(letter)) return;
            if (sealStyle == null) { sealStyle = ArcadeTypography.Style(18, true); sealStyle.alignment = TextAnchor.MiddleCenter; sealStyle.wordWrap = false; if (ArcadeTypography.Display != null) sealStyle.font = ArcadeTypography.Display; }
            sealStyle.fontSize = Mathf.Max(10, Mathf.RoundToInt(size * 0.5f));
            sealStyle.normal.textColor = Faded(new Color(0.95f, 0.75f, 0.68f, 0.9f), previous);
            GUI.Label(rect, letter, sealStyle);
        }

        /// <summary>Keyboard/controller focus: a small candle light to the left and a warm underline that breathes.</summary>
        /// <summary>The hover cue for a thing in the room: four small inked gold corner brackets hugging it, breathing gently
        /// (never a long rule under it: under a wide thing that read as a stray line across the floor).</summary>
        public static void WorldFocus(Rect bounds)
        {
            float breathe = PresentationSettings.ReducedMotion ? 0.85f : 0.7f + 0.3f * Mathf.Sin(Time.unscaledTime * 3.2f);
            float arm = Mathf.Clamp(Mathf.Min(bounds.width, bounds.height) * 0.22f, 10, 26), pad = PresentationSettings.ReducedMotion ? 4 : 4 + 2 * (1 - breathe);
            Rect r = new Rect(bounds.x - pad, bounds.y - pad, bounds.width + pad * 2, bounds.height + pad * 2);
            Color ink = new Color(Ink.r, Ink.g, Ink.b, 0.75f), gold = new Color(Gold.r, Gold.g, Gold.b, 0.65f + 0.35f * breathe);
            for (int pass = 0; pass < 2; pass++)
            {
                float t = pass == 0 ? 4 : 2, o = pass == 0 ? 1 : 0; Color c = pass == 0 ? ink : gold;
                foreach (var (x, y, sx, sy) in new[] { (r.xMin, r.yMin, 1, 1), (r.xMax, r.yMin, -1, 1), (r.xMin, r.yMax, 1, -1), (r.xMax, r.yMax, -1, -1) })
                {
                    Line(new Rect(sx > 0 ? x - o : x - arm - o, sy > 0 ? y - o : y - t + o, arm + o * 2, t), c);
                    Line(new Rect(sx > 0 ? x - o : x - t + o, sy > 0 ? y - o : y - arm - o, t, arm + o * 2), c);
                }
            }
        }

        public static void Focus(Rect bounds)
        {
            float breathe = PresentationSettings.ReducedMotion ? 0.8f : 0.65f + 0.35f * Mathf.Sin(Time.unscaledTime * 3.2f) * Mathf.Sin(Time.unscaledTime * 7.1f + 1.3f);
            Glow(new Rect(bounds.x - 30, bounds.center.y - 22, 44, 44), new Color(Gold.r, Gold.g, Gold.b, 0.65f * breathe));
            Slant(new Rect(bounds.x - 12, bounds.center.y - 6, 6, 12), Gold);
            Line(new Rect(bounds.x + 14, bounds.yMax - 3, bounds.width - 28, 2), new Color(Gold.r, Gold.g, Gold.b, 0.6f + 0.4f * breathe));
        }

        /// <summary>Respect fades already applied to GUI.color (page entrance, reveals) instead of overwriting them.</summary>
        private static Color Faded(Color color, Color current) => new Color(color.r, color.g, color.b, color.a * current.a);

        public static void Line(Rect rectangle, Color color)
        {
            Color previous = GUI.color;
            GUI.color = Faded(color, previous);
            GUI.DrawTexture(rectangle, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        /// <summary>Stitched seam: short running stitches like a letter that was opened and sewn shut again.</summary>
        public static void Divider(Rect rectangle, Color color)
        {
            float height = Mathf.Max(1, rectangle.height);
            for (float horizontal = rectangle.x; horizontal < rectangle.xMax; horizontal += 11)
                Line(new Rect(horizontal, rectangle.y, Mathf.Min(6, rectangle.xMax - horizontal), height), color);
        }

        public static void Header(Rect rectangle, string eyebrow, string heading)
        {
            GUIStyle label = accessionStyle ?? (accessionStyle = ArcadeTypography.Style(13, false, true));
            label.normal.textColor = Accent;
            GUI.Label(new Rect(rectangle.x, rectangle.y, rectangle.width, 24), eyebrow, label);
            if (headerStyle == null) { headerStyle = ArcadeTypography.Style(36, true); if (ArcadeTypography.Display != null) headerStyle.font = ArcadeTypography.Display; }
            headerStyle.normal.textColor = Cream;
            Vector2 size = headerStyle.CalcSize(new GUIContent(heading));
            GUI.Label(new Rect(rectangle.x, rectangle.y + 24, rectangle.width, rectangle.height - 24), heading, headerStyle);
            Divider(new Rect(rectangle.x + 2, rectangle.y + 24 + Mathf.Min(size.y, 52) + 2, Mathf.Min(rectangle.width, size.x + 8), 2), new Color(Accent.r, Accent.g, Accent.b, 0.8f));
        }

        public static void Stamp(Rect rectangle, string caption)
        {
            Divider(new Rect(rectangle.x, rectangle.y, rectangle.width, 1), Accent);
            GUIStyle label = stampStyle ?? (stampStyle = ArcadeTypography.Style(15, true));
            label.normal.textColor = Accent;
            label.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(rectangle.x, rectangle.y + 6, rectangle.width, rectangle.height - 12), caption, label);
            Divider(new Rect(rectangle.x, rectangle.yMax - 1, rectangle.width, 1), Accent);
        }
    }
}
