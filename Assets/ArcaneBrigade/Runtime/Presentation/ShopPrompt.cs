using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The one card that rises when the hero stands by something in the shop it can use (requirement 25/10/2026: the
    /// telephone, the table and the mirror each had their own look): a navy dialogue frame low on the screen, the thing's name in
    /// gold, one plain line of what it is, and its verb on the gold button at the right. It pops when it appears.</summary>
    public static class ShopPrompt
    {
        private static GUIStyle heading, line, primary;
        public static readonly Rect Frame = new Rect(400, 850, 1120, 148);
        private static int shownFrame = -10;
        /// <summary>True while the card is on screen: a tap on it belongs to its button, never to tap-to-walk.</summary>
        public static bool Showing => Time.frameCount - shownFrame <= 2;
        /// <summary>Draws the card in 1920x1080 GUI space; true when its button is pressed.</summary>
        public static bool Draw(string title, string plain, string verb, float age)
        {
            if (heading == null)
            {
                heading = new GUIStyle(GUI.skin.label) { font = ArcadeTypography.Display ?? ArcadeTypography.Body, fontSize = 30, alignment = TextAnchor.MiddleCenter, wordWrap = false };
                heading.normal.textColor = new Color(1, .79f, .39f);
                line = new GUIStyle(GUI.skin.label) { font = ArcadeTypography.Body, fontSize = 25, alignment = TextAnchor.MiddleCenter, wordWrap = true }; // readable on a phone (19 was too small)
                line.normal.textColor = new Color(.95f, .92f, .84f);
                primary = ArcadeSkin.Primary(new GUIStyle(ArcadeSkin.Skin.button) { font = ArcadeTypography.Body, fontSize = 20, wordWrap = true, alignment = TextAnchor.MiddleCenter });
            }
            shownFrame = Time.frameCount;
            Matrix4x4 m = GUI.matrix; Color c = GUI.color;
            LivingUI.Pop(age, Frame.center);
            ArcadeSkin.Frame(Frame, KitFrame.Dialogue, .6f);
            LocalizedGUI.Label(new Rect(420, 858, 1080, 44), title, heading);
            LocalizedGUI.Label(new Rect(430, 900, 810, 76), plain, line);
            bool pressed = ArcadeSkin.KitGuiButton(new Rect(1260, 916, 230, 56), verb, primary);
            GUI.matrix = m; GUI.color = c;
            return pressed;
        }
    }
}
