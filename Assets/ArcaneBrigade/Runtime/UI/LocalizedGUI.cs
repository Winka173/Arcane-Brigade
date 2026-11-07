using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Translate at the presentation boundary, for drawing AND text measurement.</summary>
    public static class LocalizedGUI
    {
        private static readonly System.Collections.Generic.Dictionary<(GUIStyle, int, bool), GUIStyle> fittedStyles = new System.Collections.Generic.Dictionary<(GUIStyle, int, bool), GUIStyle>();
        /// <summary>Fixed labels must fit both dimensions after translation. Long reading panels keep their existing scroll layout.</summary>
        public static GUIStyle FitLabelStyle(Rect bounds, GUIContent content, GUIStyle source)
        {
            source = FontStyle(source);
            if (!GameLocalization.IsVietnamese || source == null || string.IsNullOrEmpty(content?.text) || bounds.width <= 0 || bounds.height <= 0) return source;
            int original = source.fontSize > 0 ? source.fontSize : 16;
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
            return probe;
        }
        private static GUIStyle Fitted(GUIStyle source, int size, bool wrap)
        {
            var key = (source, size, wrap);
            if (!fittedStyles.TryGetValue(key, out GUIStyle fitted))
                fittedStyles[key] = fitted = new GUIStyle(source);
            fitted.font = source.font; fitted.fontSize = size; fitted.fontStyle = source.fontStyle;
            fitted.normal.textColor = source.normal.textColor;
            fitted.alignment = source.alignment; fitted.wordWrap = wrap;
            fitted.padding = source.padding; fitted.contentOffset = source.contentOffset;
            fitted.clipping = TextClipping.Clip;
            return fitted;
        }
        public static GUIContent FitContent(Rect bounds, GUIContent content, GUIStyle fitted)
        {
            if (!GameLocalization.IsVietnamese || fitted == null || fitted.wordWrap || string.IsNullOrEmpty(content?.text) || fitted.CalcSize(content).x <= bounds.width) return content;
            // An exceptionally long name remains readable; its full text is available in the detail panel / tooltip.
            var shortened = new GUIContent(content) { tooltip = string.IsNullOrEmpty(content.tooltip) ? content.text : content.tooltip };
            int[] graphemes = System.Globalization.StringInfo.ParseCombiningCharacters(content.text);
            int count = graphemes.Length;
            do { count--; shortened.text = content.text.Substring(0, count > 0 ? graphemes[count] : 0).TrimEnd() + "…"; }
            while (count > 0 && fitted.CalcSize(shortened).x > bounds.width);
            return shortened;
        }
        private static readonly System.Collections.Generic.Dictionary<GUIStyle, GUIStyle> vietnameseStyles = new System.Collections.Generic.Dictionary<GUIStyle, GUIStyle>();
        private static GUIStyle FontStyle(GUIStyle style)
        {
            if (!GameLocalization.IsVietnamese || style?.font == null || !style.font.name.StartsWith("Lilita")) return style;
            if (!vietnameseStyles.TryGetValue(style, out GUIStyle translatedStyle))
                vietnameseStyles[style] = translatedStyle = new GUIStyle(style);
            // Styles are frequently reused with different sizes and colours between calls.
            translatedStyle.font = ArcadeTypography.Heading;
            translatedStyle.fontSize = style.fontSize;
            translatedStyle.normal.textColor = style.normal.textColor;
            translatedStyle.alignment = style.alignment;
            translatedStyle.wordWrap = style.wordWrap;
            return translatedStyle;
        }
        public static GUIContent Content(string text) => new GUIContent(GameLocalization.Text(text));
        public static GUIContent Content(string text, string tooltip) => new GUIContent(GameLocalization.Text(text), GameLocalization.Text(tooltip));
        public static GUIContent Content(string text, Texture image) => new GUIContent(GameLocalization.Text(text), image);
        public static GUIContent Content(string text, Texture image, string tooltip) => new GUIContent(GameLocalization.Text(text), image, GameLocalization.Text(tooltip));
        public static GUIContent Content(Texture image) => new GUIContent(image);
        public static GUIContent Content(Texture image, string tooltip) => new GUIContent(image, GameLocalization.Text(tooltip));
        public static GUIContent Content(GUIContent content)
        {
            if (content == null || !GameLocalization.IsVietnamese) return content;
            return new GUIContent(GameLocalization.Text(content.text), content.image, GameLocalization.Text(content.tooltip));
        }
        public static void Label(Rect bounds, string text, GUIStyle style) => Label(bounds, Content(text), style);
        public static void Label(Rect bounds, string text) => GUI.Label(bounds, Content(text));
        public static void Label(Rect bounds, GUIContent content, GUIStyle style)
        {
            GUIContent translated = Content(content);
            GUIStyle fitted = FitLabelStyle(bounds, translated, style);
            GUI.Label(bounds, FitContent(bounds, translated, fitted), fitted);
        }
        public static void Label(Rect bounds, GUIContent content) => GUI.Label(bounds, Content(content));
        public static void Label(Rect bounds, Texture image, GUIStyle style) => GUI.Label(bounds, image, style);
        public static bool Button(Rect bounds, GUIContent content, GUIStyle style)
        {
            GUIContent translated = Content(content);
            GUIStyle fitted = FitLabelStyle(bounds, translated, style);
            return GUI.Button(bounds, FitContent(bounds, translated, fitted), fitted);
        }
    }
}
