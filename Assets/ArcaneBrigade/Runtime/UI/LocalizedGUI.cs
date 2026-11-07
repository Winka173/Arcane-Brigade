using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Translate at the presentation boundary, for drawing AND text measurement.</summary>
    public static class LocalizedGUI
    {
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
        public static void Label(Rect bounds, string text, GUIStyle style) => GUI.Label(bounds, Content(text), FontStyle(style));
        public static void Label(Rect bounds, string text) => GUI.Label(bounds, Content(text));
        public static void Label(Rect bounds, GUIContent content, GUIStyle style) => GUI.Label(bounds, Content(content), FontStyle(style));
        public static void Label(Rect bounds, GUIContent content) => GUI.Label(bounds, Content(content));
        public static void Label(Rect bounds, Texture image, GUIStyle style) => GUI.Label(bounds, image, style);
        public static bool Button(Rect bounds, GUIContent content, GUIStyle style) => GUI.Button(bounds, Content(content), style);
    }
}
