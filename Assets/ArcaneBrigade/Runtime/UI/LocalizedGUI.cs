using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Translate at the presentation boundary, for drawing AND text measurement.</summary>
    public static class LocalizedGUI
    {
        private static readonly System.Collections.Generic.Dictionary<string, GUIContent> contentCache = new System.Collections.Generic.Dictionary<string, GUIContent>(System.StringComparer.Ordinal);
        private static readonly System.Collections.Generic.Dictionary<(GUIStyle, Font, string, int, FontStyle, bool, float, float, int, int), Vector2Int> fitCache = new System.Collections.Generic.Dictionary<(GUIStyle, Font, string, int, FontStyle, bool, float, float, int, int), Vector2Int>();
        private static int cacheRevision = -1;
        private static readonly System.Collections.Generic.Dictionary<(GUIStyle, Font, int, FontStyle, string, string, float), GUIContent> clippedCache = new System.Collections.Generic.Dictionary<(GUIStyle, Font, int, FontStyle, string, string, float), GUIContent>();
        private static void EnsureRevision()
        {
            if (cacheRevision == GameLocalization.Revision) return;
            cacheRevision = GameLocalization.Revision; contentCache.Clear(); fitCache.Clear(); clippedCache.Clear(); fittedStyles.Clear(); vietnameseStyles.Clear();
        }
        private static readonly System.Collections.Generic.Dictionary<(GUIStyle, int, bool), GUIStyle> fittedStyles = new System.Collections.Generic.Dictionary<(GUIStyle, int, bool), GUIStyle>();
        /// <summary>Fixed labels must fit both dimensions after translation. Long reading panels keep their existing scroll layout.</summary>
        public static GUIStyle FitLabelStyle(Rect bounds, GUIContent content, GUIStyle source)
        {
            source = FontStyle(source);
            if (!GameLocalization.IsVietnamese || source == null || string.IsNullOrEmpty(content?.text) || bounds.width <= 0 || bounds.height <= 0) return source;
            int original = source.fontSize > 0 ? source.fontSize : 16;
            EnsureRevision();
            var key = (source, source.font, content.text, original, source.fontStyle, source.wordWrap, bounds.width, bounds.height, source.padding.horizontal, source.padding.vertical);
            if (content.image == null && fitCache.TryGetValue(key, out Vector2Int decision)) return Fitted(source, decision.x, decision.y != 0);
            bool wrap = source.wordWrap && bounds.height >= Mathf.Max(source.lineHeight, original) * 1.8f;
            GUIStyle probe = Fitted(source, original, wrap);
            int minimum = Mathf.Min(original, Mathf.Max(14, Mathf.CeilToInt(original * .65f)));
            int size = original;
            while (size > minimum && (wrap ? probe.CalcHeight(content, bounds.width) > bounds.height : probe.CalcSize(content).x > bounds.width || probe.CalcHeight(content, 100000) > bounds.height))
                probe = Fitted(source, --size, wrap);
            if (!wrap && source.wordWrap && probe.CalcSize(content).x > bounds.width)
            {
                GUIStyle twoLines = Fitted(source, size, true);
                if (twoLines.CalcHeight(content, bounds.width) <= bounds.height) probe = twoLines;
            }
            if (fitCache.Count >= 2048) fitCache.Clear();
            if (content.image == null) fitCache[key] = new Vector2Int(probe.fontSize, probe.wordWrap ? 1 : 0);
            return probe;
        }
        private static GUIStyle Fitted(GUIStyle source, int size, bool wrap)
        {
            var key = (source, size, wrap);
            if (!fittedStyles.TryGetValue(key, out GUIStyle fitted))
            {
                if (fittedStyles.Count >= 2048) fittedStyles.Clear();
                fittedStyles[key] = fitted = new GUIStyle(source);
            }
            fitted.font = source.font; fitted.fontSize = size; fitted.fontStyle = source.fontStyle;
            fitted.normal.textColor = source.normal.textColor;
            fitted.alignment = source.alignment; fitted.wordWrap = wrap;
            fitted.padding = source.padding; fitted.contentOffset = source.contentOffset;
            fitted.clipping = TextClipping.Clip;
            return fitted;
        }
        public static GUIContent FitContent(Rect bounds, GUIContent content, GUIStyle fitted)
        {
            if (!GameLocalization.IsVietnamese || fitted == null || fitted.wordWrap || string.IsNullOrEmpty(content?.text)) return content;
            EnsureRevision();
            var key = (fitted, fitted.font, fitted.fontSize, fitted.fontStyle, content.text, content.tooltip, bounds.width);
            if (content.image == null && clippedCache.TryGetValue(key, out GUIContent cached)) return cached;
            if (clippedCache.Count >= 2048) clippedCache.Clear();
            if (fitted.CalcSize(content).x <= bounds.width)
            {
                if (content.image == null) clippedCache[key] = content;
                return content;
            }
            // An exceptionally long name remains readable; its full text is available in the detail panel / tooltip.
            var shortened = new GUIContent(content) { tooltip = string.IsNullOrEmpty(content.tooltip) ? content.text : content.tooltip };
            int[] graphemes = System.Globalization.StringInfo.ParseCombiningCharacters(content.text);
            int count = graphemes.Length;
            do { count--; shortened.text = content.text.Substring(0, count > 0 ? graphemes[count] : 0).TrimEnd() + "…"; }
            while (count > 0 && fitted.CalcSize(shortened).x > bounds.width);
            if (content.image == null) clippedCache[key] = shortened;
            return shortened;
        }
        private static readonly System.Collections.Generic.Dictionary<GUIStyle, GUIStyle> vietnameseStyles = new System.Collections.Generic.Dictionary<GUIStyle, GUIStyle>();
        private static GUIStyle FontStyle(GUIStyle style)
        {
            if (!GameLocalization.IsVietnamese || style?.font == null || !style.font.name.StartsWith("Lilita")) return style;
            if (!vietnameseStyles.TryGetValue(style, out GUIStyle translatedStyle))
            {
                if (vietnameseStyles.Count >= 1024) vietnameseStyles.Clear();
                vietnameseStyles[style] = translatedStyle = new GUIStyle(style);
            }
            // Styles are frequently reused with different sizes and colours between calls.
            translatedStyle.font = ArcadeTypography.Heading;
            translatedStyle.fontSize = style.fontSize;
            translatedStyle.normal.textColor = style.normal.textColor;
            translatedStyle.alignment = style.alignment;
            translatedStyle.wordWrap = style.wordWrap;
            return translatedStyle;
        }
        public static GUIContent Content(string text)
        {
            EnsureRevision();
            text = GameLocalization.Text(text) ?? "";
            if (!contentCache.TryGetValue(text, out GUIContent content))
            {
                if (contentCache.Count >= 2048) contentCache.Clear();
                contentCache[text] = content = new GUIContent(text);
            }
            return content;
        }
        public static GUIContent Content(string text, string tooltip) => new GUIContent(GameLocalization.Text(text), GameLocalization.Text(tooltip));
        public static GUIContent Content(string text, Texture image) => new GUIContent(GameLocalization.Text(text), image);
        public static GUIContent Content(string text, Texture image, string tooltip) => new GUIContent(GameLocalization.Text(text), image, GameLocalization.Text(tooltip));
        public static GUIContent Content(Texture image) => new GUIContent(image);
        public static GUIContent Content(Texture image, string tooltip) => new GUIContent(image, GameLocalization.Text(tooltip));
        public static GUIContent Content(GUIContent content)
        {
            if (content == null || !GameLocalization.IsVietnamese) return content;
            string text = GameLocalization.Text(content.text), tooltip = GameLocalization.Text(content.tooltip);
            return text == content.text && tooltip == content.tooltip ? content : new GUIContent(text, content.image, tooltip);
        }
        public static void Label(Rect bounds, string text, GUIStyle style) => Label(bounds, Content(text), style);
        public static void Label(Rect bounds, string text) => GUI.Label(bounds, Content(text));
        public static void Label(Rect bounds, GUIContent content, GUIStyle style)
        {
            GUIContent translated = Content(content);
            GUIStyle fitted = FitLabelStyle(bounds, translated, style);
            GUIContent shown = FitContent(bounds, translated, fitted);
            GUI.Label(bounds, shown, fitted);
            ReadableTooltip.Observe(bounds, shown);
        }
        public static void Label(Rect bounds, GUIContent content) => GUI.Label(bounds, Content(content));
        public static void Label(Rect bounds, Texture image, GUIStyle style) => GUI.Label(bounds, image, style);
        public static bool Button(Rect bounds, GUIContent content, GUIStyle style)
        {
            GUIContent translated = Content(content);
            GUIStyle fitted = FitLabelStyle(bounds, translated, style);
            GUIContent shown = FitContent(bounds, translated, fitted);
            ReadableTooltip.Observe(bounds, shown);
            return GUI.Button(bounds, shown, fitted);
        }
    }
}
