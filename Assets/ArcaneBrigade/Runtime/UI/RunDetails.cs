using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class RunDetails
    {
        private RunBuildReadModel model;
        private int tab;
        private int selection;
        private Vector2 scroll;
        private Vector2 explanationScroll;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle numberStyle;
        private GUIStyle buttonStyle;
        private float openedAt;
        private float selectedAt;
        public bool Open => model != null;
        public int Tab => tab;
        public const int TabCount = 4;
        public void Show(RunSession session, RunCatalog catalog, float health, CombatSimulation simulation = null) { model = RunBuildReadModel.Capture(session, catalog, health, simulation); selection = 0; scroll = explanationScroll = Vector2.zero; openedAt = selectedAt = Time.unscaledTime; }
        public void Close() { model = null; GlossaryUI.Close(); }
        public void SelectTab(int index) { tab = Mathf.Clamp(index, 0, TabCount - 1); selection = 0; scroll = explanationScroll = Vector2.zero; selectedAt = Time.unscaledTime; }
        public void MoveSelection(int direction)
        {
            if (model == null) return;
            int count = tab == 0 ? model.Stats.Count : tab == 1 ? model.Companions.Count : tab == 2 ? model.Relics.Count : model.Links.Count;
            selection = Mathf.Clamp(selection + direction, 0, Mathf.Max(0, count - 1));
            selectedAt = Time.unscaledTime;
            float height = tab == 0 ? 115 : 145;
            scroll.y = Mathf.Clamp(scroll.y, Mathf.Max(0, (selection + 1) * height - 670), selection * height);
            explanationScroll = Vector2.zero;
        }

        public void Draw(ArenaController owner, Camera camera)
        {
            if (model == null) return;
            GUISkin previousSkin = GUI.skin;
            GUI.skin = ArcadeSkin.Skin;
            Matrix4x4 previous = ArcadeTypography.BeginViewport(camera.pixelRect, 1920, 1080);
            Color previousTint = GUI.color;
            LivingUI.Enter(Time.unscaledTime - openedAt, new Vector2(960, 540));
            GlossaryUI.BlockUnderlying(new Vector2(1920, 1080));
            GUIStyle title = titleStyle ?? (titleStyle = ArcadeTypography.Style(44, true));
            GUIStyle text = bodyStyle ?? (bodyStyle = ArcadeTypography.Style(25));
            GUIStyle number = numberStyle ?? (numberStyle = ArcadeTypography.Style(27, false, true));
            GUIStyle button = buttonStyle ?? (buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 26 });
            RunHud.Fill(new Rect(30, 20, 1860, 1040), new Color(0.09f, 0.075f, 0.16f, 0.99f));
            ArcadeSkin.Header(new Rect(75, 50, 1300, 100), "", "BRIGADE");
            ArcadeSkin.Divider(new Rect(75, 167, 1750, 2), ArcadeSkin.Accent);
            if (GUI.Button(new Rect(1610, 55, 220, 70), "CLOSE", button)) owner.ToggleDetails();
            string[] names = { "STATS", "COMPANIONS", "RELICS", "LINKS" };
            for (int index = 0; index < names.Length; index++)
            {
                Color previousColor = GUI.color;
                if (index == tab) GUI.color = ArcadeSkin.Teal;
                if (GUI.Button(new Rect(75 + index * 440, 200, 420, 70), names[index], button)) SelectTab(index);
                GUI.color = previousColor;
            }
            List<RunBuildReadModel.Row> rows = tab == 0 ? model.Stats : tab == 1 ? model.Companions : tab == 2 ? model.Relics : model.Links;
            float rowHeight = tab == 0 ? 115 : 145;
            scroll = GUI.BeginScrollView(new Rect(70, 295, 1060, 670), scroll, new Rect(0, 0, 1005, Mathf.Max(665, rows.Count * rowHeight)));
            for (int index = 0; index < rows.Count; index++)
            {
                RunBuildReadModel.Row row = rows[index];
                Rect card = new Rect(5, index * rowHeight + 5, 990, rowHeight - 12);
                RunHud.Fill(card, index == selection ? new Color(0.24f, 0.18f, 0.36f) : new Color(0.15f, 0.13f, 0.23f));
                float contentLeft = row.Icon != null ? 120 : 28;
                if (row.Icon != null) RunHud.Sprite(new Rect(25, card.y + 20, 75, 80), row.Icon);
                if (row.WeaponIcon != null) RunHud.Sprite(new Rect(68, card.y + 62, 42, 48), row.WeaponIcon);
                GlossaryUI.Text(new Rect(contentLeft, card.y + 12, 820, 48), row.Name, text);
                GUI.Label(new Rect(contentLeft, card.y + 58, 820, rowHeight - 65), row.Summary, tab == 0 ? number : text);
                if (GUI.Button(card, GUIContent.none, GUIStyle.none)) { selection = index; selectedAt = Time.unscaledTime; explanationScroll = Vector2.zero; }
            }
            if (rows.Count == 0) GUI.Label(new Rect(25, 35, 900, 120), tab == 1 ? "No companions yet. Recruit from shops at rest." : tab == 2 ? "No relics yet. Buy them from shops at rest." : "No links to inspect yet.", text);
            GUI.EndScrollView();
            RunHud.Fill(new Rect(1160, 300, 665, 665), new Color(0.15f, 0.12f, 0.24f));
            ArcadeSkin.Line(new Rect(1160, 300, 5, 665), Color.Lerp(ArcadeSkin.Accent, ArcadeSkin.Teal, LivingUI.Reveal(Time.unscaledTime - selectedAt, PresentationSettings.ReducedMotion)));
            string explanation = rows.Count == 0 ? "" : rows[Mathf.Clamp(selection, 0, rows.Count - 1)].Explanation;
            if (tab == 2) explanation += "\nRECENT RECEIPTS\n\n" + (string.IsNullOrEmpty(model.Receipts) ? "No saved transactions yet." : model.Receipts);
            float explanationHeight = Mathf.Max(620, GlossaryUI.Height(explanation, text, 570) + 30);
            explanationScroll = GUI.BeginScrollView(new Rect(1180, 325, 625, 610), explanationScroll, new Rect(0, 0, 585, explanationHeight));
            GlossaryUI.Text(new Rect(5, 5, 570, explanationHeight), explanation, text);
            GUI.EndScrollView();
            GUI.Label(new Rect(75, 988, 1750, 42), RunUIHints.DetailsNavigation, text);
            GlossaryUI.Draw(new Vector2(1920, 1080));
            GUI.matrix = previous;
            GUI.color = previousTint;
            GUI.skin = previousSkin;
        }
    }
}
