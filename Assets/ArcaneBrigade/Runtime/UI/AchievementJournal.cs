using UnityEngine;
namespace ArcaneBrigade
{
    public sealed class AchievementJournal
    {
        private Vector2 scroll;
        private bool unfinishedOnly;
        /// <summary>The hero a memory belongs to ("Lantern Knight / First dawn"), for its portrait; null for the general ones.</summary>
        private static Sprite HeroOf(RunCatalog catalog, string name)
        {
            int cut = name.IndexOf(" / ", System.StringComparison.Ordinal); if (cut <= 0) return null;
            string hero = name.Substring(0, cut);
            foreach (UnitDefinition c in catalog.Champions) if (c != null && RunUICopy.UnitName(c) == hero) return c.Body;
            return null;
        }
        public void Draw(RunCatalog catalog, PlayerProfile profile)
        {
            var definitions = RunAchievements.Definitions(catalog); int earned = 0;
            foreach (var d in definitions) if (profile.Achievements.Contains(d.Id)) earned++;
            GUIStyle title = ArcadeTypography.Style(ArcadeTypography.On720(ArcadeTypography.SizeBody), true), text = ArcadeTypography.Style(ArcadeTypography.On720(ArcadeTypography.SizeNote));
            GUI.Label(new Rect(65, 150, 770, 35), earned + " / " + definitions.Count + " memories recorded. No permanent stat rewards.", text);
            unfinishedOnly = ArcadeSkin.Toggle(new Rect(915, 146, 320, 44), unfinishedOnly, "Show unfinished only"); // the painted switch (UI kit)
            int count = 0; foreach (var d in definitions) if (!unfinishedOnly || !profile.Achievements.Contains(d.Id)) count++;
            scroll = ScrollPanel.Begin(new Rect(60, 200, 1160, 455), scroll, new Rect(0, 0, 1120, Mathf.Max(450, Mathf.Ceil(count / 2f) * 150)));
            Sprite sealDone = ArcadeSkin.KitSprite("Info/SealDone"), sealEmpty = ArcadeSkin.KitSprite("Info/SealEmpty");
            int index = 0;
            foreach (var d in definitions)
            {
                bool complete = profile.Achievements.Contains(d.Id); if (unfinishedOnly && complete) continue;
                Rect card = new Rect(index % 2 * 560, index / 2 * 150, 540, 138); index++; // room for two lines of description above the bar
                // review 31/10/2026: each memory on a parchment card (golden once recorded), its hero's portrait and a wax seal,
                // instead of a wall of identical dark boxes
                bool painted = ArcadeSkin.KitSliced(card, complete ? "Info/MemoryCardDone" : "Info/MemoryCard", .8f);
                if (!painted) { RunHud.Fill(card, ArcadeSkin.Card); ArcadeSkin.Line(new Rect(card.x, card.y, 4, card.height), complete ? ArcadeSkin.Gold : ArcadeSkin.Teal); }
                Color ink = painted ? ArcadeSkin.KitInk : ArcadeSkin.Cream;
                title.normal.textColor = ink; text.normal.textColor = new Color(ink.r, ink.g, ink.b, .9f);
                Sprite portrait = HeroOf(catalog, d.Name);
                float left = card.x + (portrait != null ? 92 : 30);
                if (portrait != null) RunHud.Sprite(new Rect(card.x + 22, card.y + 24, 62, 84), portrait);
                Sprite seal = complete ? sealDone : sealEmpty;
                if (seal != null) RunHud.Sprite(new Rect(card.xMax - 70, card.y + 14, 50, 50), seal);
                float right = card.xMax - (seal != null ? 76 : 20);
                GUI.Label(new Rect(left, card.y + 10, right - left, 30), (complete && seal == null ? "✓ " : "") + d.Name, title);
                text.wordWrap = true; text.clipping = TextClipping.Clip;
                GUI.Label(new Rect(left, card.y + 40, right - left, 54), d.Description, text);
                float progress = complete ? d.Goal : RunAchievements.Progress(profile.AchievementEvidence, d);
                Rect bar = new Rect(left, card.y + 108, card.xMax - 150 - left, 14);
                if (ArcadeSkin.Strip(bar, "Info/RatioTrack", 24, 24))
                {
                    Rect fill = new Rect(bar.x + 2, bar.y + 2, (bar.width - 4) * Mathf.Clamp01(progress / d.Goal), bar.height - 4);
                    if (fill.width >= fill.height) ArcadeSkin.Strip(fill, "Info/RatioFill", 20, 20);
                }
                else
                {
                    ArcadeSkin.Line(bar, new Color(.16f, .18f, .24f));
                    ArcadeSkin.Line(new Rect(bar.x, bar.y, bar.width * Mathf.Clamp01(progress / d.Goal), bar.height), complete ? ArcadeSkin.Gold : ArcadeSkin.Teal);
                }
                GUI.Label(new Rect(card.xMax - 140, card.y + 100, 120, 28), progress.ToString("0.##") + " / " + d.Goal.ToString("0.##"), text);
            }
            ScrollPanel.End();
        }
    }
}
