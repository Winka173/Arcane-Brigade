using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Choices as cards (requirement 22/10/2026: a choice must say what it means and wait for a confirm): each card holds the
    /// option and, under it, what it leads to; a tap picks it (gold rim, glow and focus light), and CONFIRM takes it. The cards
    /// share the area above the button equally; every line stays in the paper's padded inner area.</summary>
    public static class ChoiceCards
    {
        private static GUIStyle title, body, button;
        public const float ButtonH = 84;

        /// <summary>Draws the cards and the confirm button; returns the confirmed option's index, or -1.</summary>
        public static int Draw(Rect area, string[] options, string[] meanings, ref int picked, string confirm = "CONFIRM")
        {
            if (title == null)
            {
                title = ArcadeTypography.Style(26, true); title.alignment = TextAnchor.MiddleCenter; title.wordWrap = true;
                body = ArcadeTypography.Style(21); body.alignment = TextAnchor.UpperCenter; body.wordWrap = true;
                button = ArcadeTypography.Style(28, true); button.alignment = TextAnchor.MiddleCenter;
            }
            int n = options == null ? 0 : options.Length;
            if (n == 0) return -1;
            const float gap = 30;
            float cardW = (area.width - gap * (n - 1)) / n, cardH = area.height - ButtonH - 20;
            Color ink = new Color(.22f, .16f, .1f);
            for (int i = 0; i < n; i++)
            {
                Rect card = new Rect(area.x + i * (cardW + gap), area.y, cardW, cardH);
                bool on = picked == i;
                if (on) ArcadeSkin.Glow(new Rect(card.x - 20, card.y - 16, card.width + 40, card.height + 32), new Color(1f, .84f, .45f, .5f));
                ArcadeSkin.Ticket(card, ArcadeSkin.Paper, on ? ArcadeSkin.Gold : ArcadeSkin.Accent);
                Rect inner = ArcadeSkin.Content(card, KitFrame.Card);
                title.normal.textColor = on ? ArcadeSkin.Wax : ink;
                float titleH = Mathf.Min(inner.height * .45f, title.CalcHeight(LocalizedGUI.Content(options[i]), inner.width) + 4);
                LocalizedGUI.Label(new Rect(inner.x, inner.y, inner.width, titleH), options[i], title);
                string meaning = meanings != null && i < meanings.Length ? meanings[i] : null;
                if (!string.IsNullOrEmpty(meaning))
                {
                    body.normal.textColor = ink;
                    LocalizedGUI.Label(new Rect(inner.x, inner.y + titleH + 6, inner.width, inner.height - titleH - 6), meaning, body);
                }
                if (TouchTargets.Button(card, GUIContent.none, GUIStyle.none)) { if (picked != i) CheckpointAudio.PlayInterfaceId("ui.select"); picked = i; }
            }
            bool ready = picked >= 0 && picked < n;
            Rect go = new Rect(area.center.x - 200, area.yMax - ButtonH, 400, ButtonH);
            if (!ready) { Hint(go); return -1; } // nothing picked yet: a plain hint, not a greyed button that looks broken
            return ArcadeSkin.Button(go, confirm, KitButton.Primary, true, button) ? picked : -1;
        }
        private static GUIStyle hintStyle;
        /// <summary>The line shown where the confirm button will be, until a card is picked.</summary>
        public static void Hint(Rect where)
        {
            hintStyle ??= new GUIStyle(ArcadeTypography.Style(26, true)) { alignment = TextAnchor.MiddleCenter };
            hintStyle.normal.textColor = ArcadeSkin.KitCream;
            LocalizedGUI.Label(where, "Tap a card to choose", hintStyle);
        }
    }
}
