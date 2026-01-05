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
        public bool Open => model != null;
        public int Tab => tab;
        public const int TabCount = 4;
        public void Show(RunSession session, RunCatalog catalog, float health) { model = RunBuildReadModel.Capture(session, catalog, health); selection = 0; scroll = explanationScroll = Vector2.zero; }
        public void Close() { model = null; }
        public void SelectTab(int index) { tab = Mathf.Clamp(index, 0, TabCount - 1); selection = 0; scroll = explanationScroll = Vector2.zero; }
        public void MoveSelection(int direction)
        {
            if (model == null) return;
            int count = tab == 0 ? model.Stats.Count : tab == 1 ? model.Companions.Count : tab == 2 ? model.Relics.Count : model.Links.Count;
            selection = Mathf.Clamp(selection + direction, 0, Mathf.Max(0, count - 1));
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
            GUIStyle title = ArcadeTypography.Style(44, true);
            GUIStyle text = ArcadeTypography.Style(25);
            GUIStyle number = ArcadeTypography.Style(27, false, true);
            GUIStyle button = new GUIStyle(GUI.skin.button) { fontSize = 26 };
            RunHud.Fill(new Rect(30, 20, 1860, 1040), new Color(0.09f, 0.075f, 0.16f, 0.99f));
            GUI.Label(new Rect(75, 50, 1300, 70), "HỒ SƠ ĐỘI QUÂN", title);
            GUI.Label(new Rect(75, 125, 1600, 50), "Read-only · không đổi build/RNG · I hoặc Escape để đóng", text);
            if (GUI.Button(new Rect(1610, 55, 220, 70), "ĐÓNG", button)) owner.ToggleDetails();
            string[] names = { "STATS", "ĐỒNG MINH", "VẬT PHẨM", "LIÊN KẾT" };
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
                if (GUI.Button(card, GUIContent.none, GUIStyle.none)) { selection = index; explanationScroll = Vector2.zero; }
                float contentLeft = row.Icon != null ? 120 : 28;
                if (row.Icon != null) RunHud.Sprite(new Rect(25, card.y + 20, 75, 80), row.Icon);
                GUI.Label(new Rect(contentLeft, card.y + 12, 820, 48), row.Name, text);
                GUI.Label(new Rect(contentLeft, card.y + 58, 820, rowHeight - 65), row.Summary, tab == 0 ? number : text);
            }
            if (rows.Count == 0) GUI.Label(new Rect(25, 35, 900, 120), "Chưa có vật phẩm trong lượt. Mua tại ba quầy ở rest; không nhận lựa chọn nâng cấp thủ công trong combat.", text);
            GUI.EndScrollView();
            RunHud.Fill(new Rect(1160, 300, 665, 665), new Color(0.15f, 0.12f, 0.24f));
            string explanation = rows.Count == 0 ? "" : rows[Mathf.Clamp(selection, 0, rows.Count - 1)].Explanation;
            if (tab == 2) explanation += "\nBIÊN LAI GẦN ĐÂY\n\n" + (string.IsNullOrEmpty(model.Receipts) ? "Chưa có giao dịch đã lưu." : model.Receipts);
            explanation += "\n\nBONDS\n\n" + model.Bonds;
            float explanationHeight = Mathf.Max(620, text.CalcHeight(new GUIContent(explanation), 570) + 30);
            explanationScroll = GUI.BeginScrollView(new Rect(1180, 325, 625, 610), explanationScroll, new Rect(0, 0, 585, explanationHeight));
            GUI.Label(new Rect(5, 5, 570, explanationHeight), explanation, text);
            GUI.EndScrollView();
            GUI.Label(new Rect(75, 995, 1730, 45), "Tab: đổi mục · click/tap một dòng để xem nguồn và scope · không bán/đổi đồ", text);
            GUI.matrix = previous;
            GUI.skin = previousSkin;
        }
    }
}
