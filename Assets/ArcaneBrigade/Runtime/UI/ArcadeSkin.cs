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
        public static readonly Color Ink = new Color(0.07f, 0.12f, 0.16f);
        public static readonly Color Purple = new Color(0.97f, 0.35f, 0.28f);
        public static readonly Color Cream = new Color(1f, 0.94f, 0.76f);
        public static readonly Color Teal = new Color(0.28f, 0.94f, 0.86f);
        public static Color Paper => Bundle == 1 ? new Color(0.87f, 0.96f, 0.91f) : Bundle == 2 ? new Color(1f, 0.89f, 0.90f) : new Color(0.98f, 0.93f, 0.81f);
        public static Color Accent => Bundle == 1 ? new Color(0.35f, 0.90f, 0.83f) : Bundle == 2 ? new Color(1f, 0.50f, 0.66f) : new Color(0.97f, 0.35f, 0.28f);

        public static GUISkin Skin
        {
            get
            {
                if (skin != null) return skin;
                skin = Object.Instantiate(GUI.skin);
                if (ArcadeTypography.Body != null) skin.font = ArcadeTypography.Body;
                skin.button = new GUIStyle(skin.button) { border = new RectOffset(10, 10, 10, 10), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
                skin.button.normal.background = ButtonTexture(Cream, Ink);
                skin.button.hover.background = ButtonTexture(Teal, Ink);
                skin.button.active.background = ButtonTexture(new Color(1, 0.5f, 0.38f), Ink);
                skin.button.normal.textColor = skin.button.hover.textColor = skin.button.active.textColor = Ink;
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
                    bool punch = Vector2.Distance(new Vector2(horizontal, vertical), new Vector2(0, 15.5f)) < 3.8f || Vector2.Distance(new Vector2(horizontal, vertical), new Vector2(31, 15.5f)) < 3.8f;
                    Color color = punch ? Color.clear : horizontal < 2 || horizontal > 29 || vertical < 2 || vertical > 29 ? edge : horizontal < 5 ? Accent : face;
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
            Color surface = PresentationSettings.HighReadability ? new Color(0.025f, 0.045f, 0.06f) : Color.Lerp(Ink, Teal, 0.055f);
            Line(new Rect(rectangle.x + 5, rectangle.y + 7, rectangle.width, rectangle.height), new Color(0.025f, 0.03f, 0.035f, 0.72f));
            Line(rectangle, surface);
            Line(new Rect(rectangle.x, rectangle.y, 7, rectangle.height), Accent);
            Divider(new Rect(rectangle.x + 20, rectangle.yMax - 9, rectangle.width - 40, 2), Color.Lerp(Cream, surface, 0.65f));
        }

        public static void Ticket(Rect rectangle, Color paper, Color stamp)
        {
            Line(new Rect(rectangle.x + 3, rectangle.y + 4, rectangle.width, rectangle.height), new Color(Ink.r, Ink.g, Ink.b, 0.55f));
            Line(rectangle, paper);
            Line(new Rect(rectangle.x, rectangle.y, 6, rectangle.height), stamp);
            for (float horizontal = rectangle.x + 10; horizontal < rectangle.xMax - 5; horizontal += 12)
                Line(new Rect(horizontal, rectangle.yMax - 3, 5, 3), Ink);
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
