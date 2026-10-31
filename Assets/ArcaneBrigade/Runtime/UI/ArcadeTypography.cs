using UnityEngine;

namespace ArcaneBrigade
{
    public static class ArcadeTypography
    {
        private static Font body;
        private static Font heading;
        private static Font numbers;
        private static Font display;
        // UI type (requirement 20/10/2026: fonts must match the painted storybook UI). All SIL OFL 1.1, Vietnamese-complete:
        // body = Alegreya Sans Bold, a humanist bookish sans that stays legible at HUD sizes; headings and buttons = Fraunces
        // SemiBold, a soft old-style storybook serif; titles = Fraunces Black; numbers = Alegreya Sans ExtraBold with tabular
        // digits so counters don't jitter. Built with fontTools (Tools/Fonts/make_fonts.py): outlines scaled so x-height and
        // line height stay near the old Be Vietnam Pro layout, hinting removed; Fraunces falls back to the body face for arrows.
        public static Font Body => body != null ? body : body = Resources.Load<Font>("Fonts/AlegreyaSans-Body") ?? Resources.Load<Font>("Fonts/BeVietnamPro-SemiBold");
        public static Font Heading => heading != null ? heading : heading = Resources.Load<Font>("Fonts/Fraunces-Heading") ?? Resources.Load<Font>("Fonts/BeVietnamPro-Black");
        public static Font Numbers => numbers != null ? numbers : numbers = Resources.Load<Font>("Fonts/AlegreyaSans-Numbers") ?? Resources.Load<Font>("Fonts/NotoSansMono-Bold");
        public static Font Display => display != null ? display : display = Resources.Load<Font>("Fonts/Fraunces-Display") ?? Heading;
        private static Font punch;
        /// <summary>Lilita One (SIL OFL 1.1): thick rounded lettering for celebrations and signs in the scene (STAR UP, MASTERED),
        /// always drawn with an ink outline (InkedText). Latin only, so never for body text.</summary>
        public static Font Punch => punch != null ? punch : punch = Resources.Load<Font>("Fonts/LilitaOne-Regular") ?? Heading;
        private static Font hand;
        /// <summary>Handwriting (Caveat, SIL OFL 1.1, static weight 520): notes, companions' own words, the hero's coaching. Use sparingly.</summary>
        public static Font Hand => hand != null ? hand : hand = Resources.Load<Font>("Fonts/Caveat-Hand") ?? Body;
        public static string DisplayText(string value) => string.IsNullOrEmpty(value) || value.IsNormalized(System.Text.NormalizationForm.FormC) ? value : value.Normalize(System.Text.NormalizationForm.FormC);

        /// <summary>A label with a soft dark drop shadow, for text drawn straight over the scene with no panel behind it.</summary>
        public static void ShadowLabel(Rect rectangle, string text, GUIStyle style, float offset = 2)
        {
            Color colour = style.normal.textColor;
            style.normal.textColor = new Color(0.04f, 0.03f, 0.07f, colour.a * 0.85f);
            GUI.Label(new Rect(rectangle.x + offset, rectangle.y + offset, rectangle.width, rectangle.height), text, style);
            style.normal.textColor = colour;
            GUI.Label(rectangle, text, style);
        }

        /// <summary>The UI standard's five sizes (Docs/UI_Standard.md) in units of the 1920x1080 canvas; nothing is set smaller than SizeNote.</summary>
        public const int SizeNote = 28, SizeBody = 34, SizeItem = 44, SizeTitle = 64, SizeDisplay = 110;
        /// <summary>The same size on the 1280x720 menu canvas.</summary>
        public static int On720(int size) => Mathf.RoundToInt(size * 2f / 3f);

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
