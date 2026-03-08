using UnityEngine;

namespace ArcaneBrigade
{
    public static class ArcadeTypography
    {
        private static Font body;
        private static Font heading;
        private static Font numbers;
        private static Font display;
        public static Font Body => body != null ? body : body = Resources.Load<Font>("Fonts/BeVietnamPro-Medium");
        public static Font Heading => heading != null ? heading : heading = Resources.Load<Font>("Fonts/BeVietnamPro-Black");
        public static Font Numbers => numbers != null ? numbers : numbers = Resources.Load<Font>("Fonts/NotoSansMono-Bold");
        // Miền Chờ: titles, chapter names and the logo use Young Serif (SIL OFL 1.1) — a warm storybook / letter-writing serif.
        // UI controls and body stay on Be Vietnam Pro for small-size legibility; numbers on Noto Sans Mono.
        public static Font Display => display != null ? display : display = Resources.Load<Font>("Fonts/YoungSerif-Regular") ?? Heading;
        private static Font punch;
        /// <summary>Lilita One (SIL OFL 1.1): thick rounded lettering for celebrations and signs in the scene (STAR UP, MASTERED),
        /// always drawn with an ink outline (InkedText). Latin only, so never for body text.</summary>
        public static Font Punch => punch != null ? punch : punch = Resources.Load<Font>("Fonts/LilitaOne-Regular") ?? Heading;
        private static Font hand;
        /// <summary>Handwriting (Caveat, SIL OFL 1.1, static weight 520): notes, companions' own words, the hero's coaching. Use sparingly.</summary>
        public static Font Hand => hand != null ? hand : hand = Resources.Load<Font>("Fonts/Caveat-Hand") ?? Body;
        public static string DisplayText(string value) => string.IsNullOrEmpty(value) || value.IsNormalized(System.Text.NormalizationForm.FormC) ? value : value.Normalize(System.Text.NormalizationForm.FormC);

        public static GUIStyle Style(int size, bool title = false, bool numeric = false)
        {
            return new GUIStyle(GUI.skin.label) { font = numeric ? Numbers : title ? Heading : Body, fontSize = size, wordWrap = true, alignment = TextAnchor.UpperLeft };
        }

        public static Matrix4x4 BeginViewport(Rect viewport, float designWidth, float designHeight)
        {
            Matrix4x4 previous = GUI.matrix;
            GUI.matrix = FitViewport(viewport, Screen.safeArea, Screen.height, designWidth, designHeight);
            return previous;
        }
        public static Matrix4x4 FitViewport(Rect viewport, Rect safe, float screenHeight, float designWidth, float designHeight)
        {
            float left = Mathf.Max(viewport.xMin, safe.xMin);
            float right = Mathf.Min(viewport.xMax, safe.xMax);
            float bottom = Mathf.Max(viewport.yMin, safe.yMin);
            float top = Mathf.Min(viewport.yMax, safe.yMax);
            float scale = Mathf.Max(0.001f, Mathf.Min(Mathf.Max(0, right - left) / Mathf.Max(1, designWidth), Mathf.Max(0, top - bottom) / Mathf.Max(1, designHeight)));
            return Matrix4x4.TRS(new Vector3(left + (right - left - designWidth * scale) * 0.5f, screenHeight - top + (top - bottom - designHeight * scale) * 0.5f, 0), Quaternion.identity, Vector3.one * scale);
        }
    }
}
