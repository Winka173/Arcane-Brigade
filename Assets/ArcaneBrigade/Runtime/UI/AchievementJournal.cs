using UnityEngine;
namespace ArcaneBrigade
{
    public sealed class AchievementJournal
    {
        private Vector2 scroll;
        private bool unfinishedOnly;
        public void Draw(RunCatalog catalog, PlayerProfile profile)
        {
            var definitions = RunAchievements.Definitions(catalog); int earned = 0;
            foreach (var d in definitions) if (profile.Achievements.Contains(d.Id)) earned++;
            GUIStyle title = ArcadeTypography.Style(18, true), text = ArcadeTypography.Style(16);
            GUI.Label(new Rect(65, 150, 770, 35), earned + " / " + definitions.Count + " memories recorded. No permanent stat rewards.", text);
            unfinishedOnly = GUI.Toggle(new Rect(915, 150, 300, 35), unfinishedOnly, "Show unfinished only");
            int count = 0; foreach (var d in definitions) if (!unfinishedOnly || !profile.Achievements.Contains(d.Id)) count++;
            scroll = GUI.BeginScrollView(new Rect(60, 200, 1160, 455), scroll, new Rect(0, 0, 1120, Mathf.Max(450, Mathf.Ceil(count / 2f) * 130)));
            int index = 0;
            foreach (var d in definitions)
            {
                bool complete = profile.Achievements.Contains(d.Id); if (unfinishedOnly && complete) continue;
                Rect card = new Rect(index % 2 * 560, index / 2 * 130, 540, 116); index++;
                RunHud.Fill(card, ArcadeSkin.Card); ArcadeSkin.Line(new Rect(card.x, card.y, 4, card.height), complete ? ArcadeSkin.Gold : ArcadeSkin.Teal);
                GUI.Label(new Rect(card.x + 20, card.y + 10, 500, 30), (complete ? "✓ " : "") + d.Name, title);
                GUI.Label(new Rect(card.x + 20, card.y + 42, 500, 45), d.Description, text);
                float progress = complete ? d.Goal : RunAchievements.Progress(profile.AchievementEvidence, d);
                ArcadeSkin.Line(new Rect(card.x + 20, card.y + 98, 390, 4), new Color(.16f, .18f, .24f));
                ArcadeSkin.Line(new Rect(card.x + 20, card.y + 98, 390 * Mathf.Clamp01(progress / d.Goal), 4), complete ? ArcadeSkin.Gold : ArcadeSkin.Teal);
                GUI.Label(new Rect(card.x + 420, card.y + 87, 110, 25), progress.ToString("0.##") + " / " + d.Goal.ToString("0.##"), text);
            }
            GUI.EndScrollView();
        }
    }
}
