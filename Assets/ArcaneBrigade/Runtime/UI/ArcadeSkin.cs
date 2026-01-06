using UnityEngine;

namespace ArcaneBrigade
{
    public static class ArcadeSkin
    {
        private static GUISkin skin;
        private static GUIStyle panel;
        private static Texture2D rounded;
        private static GUIStyle accessionStyle;
        private static GUIStyle headerStyle;
        private static GUIStyle stampStyle;
        public static int Bundle { get; set; }
        public static readonly Color Ink = new Color(0.10f, 0.09f, 0.19f);
        public static readonly Color Purple = new Color(0.62f, 0.28f, 0.92f);
        public static readonly Color Cream = new Color(1f, 0.94f, 0.76f);
        public static readonly Color Teal = new Color(0.28f, 0.94f, 0.86f);
        public static Color Paper => Bundle == 1 ? new Color(0.87f, 0.96f, 0.91f) : Bundle == 2 ? new Color(1f, 0.89f, 0.90f) : new Color(0.98f, 0.93f, 0.81f);
        public static Color Accent => Bundle == 1 ? new Color(0.35f, 0.90f, 0.83f) : Bundle == 2 ? new Color(1f, 0.50f, 0.66f) : new Color(1f, 0.72f, 0.32f);

        public static GUISkin Skin
        {
            get
            {
                if (skin != null) return skin;
                skin = Object.Instantiate(GUI.skin);
                if (ArcadeTypography.Body != null) skin.font = ArcadeTypography.Body;
                skin.button = new GUIStyle(skin.button) { border = new RectOffset(10, 10, 10, 10), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
                skin.button.normal.background = ButtonTexture(new Color(0.22f, 0.18f, 0.32f), new Color(0.88f, 0.65f, 0.34f));
                skin.button.hover.background = ButtonTexture(new Color(0.33f, 0.24f, 0.46f), Teal);
                skin.button.active.background = ButtonTexture(new Color(0.2f, 0.15f, 0.31f), Cream);
                skin.button.normal.textColor = skin.button.hover.textColor = skin.button.active.textColor = Cream;
                skin.button.onNormal = skin.button.normal;
                skin.label.normal.textColor = Cream;
                return skin;
            }
        }

        private static Texture2D ButtonTexture(Color face, Color edge)
        {
            var texture = new Texture2D(32, 32, TextureFormat.RGBA32, false) { name = "ArcadeButton", filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave };
            var pixels = new Color[1024];
            for (int vertical = 0; vertical < 32; vertical++)
                for (int horizontal = 0; horizontal < 32; horizontal++)
                {
                    float distance = RoundedDistance(horizontal, vertical);
                    Color color = distance > 7.8f ? Color.clear : distance > 5.2f || horizontal < 3 || horizontal > 28 || vertical < 3 || vertical > 28 ? edge : Color.Lerp(face * 0.8f, face, vertical / 31f);
                    color.a = distance > 7.8f ? 0f : 1f;
                    pixels[vertical * 32 + horizontal] = color;
                }
            texture.SetPixels(pixels); texture.Apply(false, true);
            return texture;
        }

        private static float RoundedDistance(int horizontal, int vertical)
        {
            float centerX = Mathf.Clamp(horizontal, 8, 23);
            float centerY = Mathf.Clamp(vertical, 8, 23);
            return Vector2.Distance(new Vector2(horizontal, vertical), new Vector2(centerX, centerY));
        }

        private static void Rounded(Rect rectangle, Color color)
        {
            if (rounded == null)
            {
                rounded = new Texture2D(32, 32, TextureFormat.RGBA32, false) { name = "ArcadeRounded", hideFlags = HideFlags.HideAndDontSave };
                var pixels = new Color[1024];
                for (int vertical = 0; vertical < 32; vertical++)
                    for (int horizontal = 0; horizontal < 32; horizontal++) pixels[vertical * 32 + horizontal] = new Color(1, 1, 1, Mathf.Clamp01(8f - RoundedDistance(horizontal, vertical)));
                rounded.SetPixels(pixels); rounded.Apply(false, true);
                panel = new GUIStyle { border = new RectOffset(9, 9, 9, 9) };
                panel.normal.background = rounded;
            }
            Color previous = GUI.color;
            GUI.color = color;
            if (Event.current.type == EventType.Repaint) panel.Draw(rectangle, GUIContent.none, false, false, false, false);
            GUI.color = previous;
        }

        public static void Panel(Rect rectangle, Color color)
        {
            if (PresentationSettings.HighReadability) color = new Color(color.r * 0.6f, color.g * 0.6f, color.b * 0.6f, 1);
            Rounded(new Rect(rectangle.x, rectangle.y + 5f, rectangle.width, rectangle.height), new Color(0.035f, 0.025f, 0.075f, 0.8f));
            Rounded(rectangle, Color.Lerp(Accent, Ink, 0.55f));
            Rounded(new Rect(rectangle.x + 2, rectangle.y + 2, rectangle.width - 4, rectangle.height - 4), color);
            Line(new Rect(rectangle.x + 16, rectangle.y + 9, Mathf.Min(32, rectangle.width - 32), 2), Accent);
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
            if (headerStyle == null) headerStyle = ArcadeTypography.Style(36, true);
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
