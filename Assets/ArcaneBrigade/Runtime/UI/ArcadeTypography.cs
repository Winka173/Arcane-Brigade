using UnityEngine;

namespace ArcaneBrigade
{
    public static class ArcadeTypography
    {
        private static Font body;
        private static Font heading;
        private static Font numbers;
        public static Font Body => body != null ? body : body = Resources.Load<Font>("Fonts/NotoSans-Regular");
        public static Font Heading => heading != null ? heading : heading = Resources.Load<Font>("Fonts/NotoSans-Black");
        public static Font Numbers => numbers != null ? numbers : numbers = Resources.Load<Font>("Fonts/NotoSansMono-Bold");

        public static GUIStyle Style(int size, bool title = false, bool numeric = false)
        {
            return new GUIStyle(GUI.skin.label) { font = numeric ? Numbers : title ? Heading : Body, fontSize = size, wordWrap = true, alignment = TextAnchor.UpperLeft };
        }

        public static Matrix4x4 BeginViewport(Rect viewport, float designWidth, float designHeight)
        {
            Rect safe = Screen.safeArea;
            float left = Mathf.Max(viewport.xMin, safe.xMin);
            float right = Mathf.Min(viewport.xMax, safe.xMax);
            float bottom = Mathf.Max(viewport.yMin, safe.yMin);
            float top = Mathf.Min(viewport.yMax, safe.yMax);
            float scale = Mathf.Min((right - left) / designWidth, (top - bottom) / designHeight);
            Matrix4x4 previous = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(new Vector3(left + (right - left - designWidth * scale) * 0.5f, Screen.height - top + (top - bottom - designHeight * scale) * 0.5f, 0), Quaternion.identity, Vector3.one * scale);
            return previous;
        }
    }
}
