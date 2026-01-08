using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class RunInvoice
    {
        private Vector2 scroll;
        private string identity;
        private string cachedContent;
        private RunSnapshot cachedBuild;
        private GUIStyle headingStyle;
        private GUIStyle bodyStyle;
        private GUIStyle numericStyle;
        private bool scrollToEnd;
        public void ScrollToEnd() { scrollToEnd = true; }
        public static string Describe(RunSnapshot run, RunCatalog catalog)
        {
            var lines = new StringBuilder();
            UnitDefinition champion = catalog.Unit(run.ChampionId);
            lines.AppendLine("NGƯỜI DẪN ĐOÀN   " + (champion == null ? RunUICopy.UnitName(run.ChampionId) : RunUICopy.UnitName(champion)));
            lines.AppendLine("Vũ khí: " + (champion == null ? "Không rõ" : RunUICopy.WeaponName(champion.Weapon.Id)));
            lines.AppendLine("\nĐỘI QUÂN THỰC TẾ   " + run.Roster.Count + " đơn vị");
            foreach (RosterEntry entry in run.Roster)
                lines.AppendLine(RunUICopy.UnitName(entry.UnitId, catalog.Unit(entry.UnitId)?.DisplayName) + " · " + RunUICopy.WeaponName(entry.WeaponId) + " · cấp " + entry.Tier);
            lines.AppendLine("\nNGĂN KÉO CỔ VẬT");
            foreach (ItemStack stack in run.Items) lines.AppendLine((catalog.Item(stack.Id)?.Name ?? stack.Id) + " ×" + stack.Count);
            if (run.Items.Count == 0) lines.AppendLine("Không có cổ vật.");
            lines.AppendLine("\nĐÓNG GÓP ĐÃ ĐO   /   lượng máu thực tế");
            var contributions = new List<ContributionEntry>(run.Contributions);
            contributions.Sort((first, second) => first.Damage == second.Damage ? string.CompareOrdinal(first.Key, second.Key) : second.Damage.CompareTo(first.Damage));
            foreach (ContributionEntry entry in contributions)
            {
                lines.AppendLine(RunUICopy.UnitName(entry.UnitId, catalog.Unit(entry.UnitId)?.DisplayName) + " · " + RunUICopy.WeaponName(entry.WeaponId) + " · cấp " + entry.Tier + " / " + RunUICopy.ContributionName(entry.EffectId, catalog));
                lines.AppendLine("  Sát thương " + entry.Damage.ToString("0.#") + "  |  hồi máu " + entry.Healing.ToString("0.#") + "  |  hạ gục " + entry.Kills + "  |  vàng " + entry.Coins + "  |  kích hoạt " + entry.Activations);
            }
            if (contributions.Count == 0) lines.AppendLine("Chưa có đóng góp được đo; không suy đoán từ cấp.");
            lines.AppendLine("Không tính sát thương vượt máu còn lại; hiệu ứng phụ chỉ ghi một lần theo lượng máu thực tế.\n");
            if (run.LegacyContributionKills) lines.AppendLine("Lượt lưu cũ chưa đo số hạ gục theo nguồn. Chỉ ghi từ khi nâng phiên bản, không dựng lại số liệu cũ.");
            if (run.Phase == SessionPhase.Defeat) lines.AppendLine("ĐÒN KẾT THÚC\n" + (string.IsNullOrEmpty(run.LastDamageSource) ? "Không rõ nguồn trong lượt lưu cũ" : RunUICopy.DamageSource(run.LastDamageSource, catalog)) + " · " + RunUICopy.DamageKind(run.LastDamageKind) + " · " + run.LastDamageAmount.ToString("0.#") + " sát thương");
            lines.AppendLine("\nMã ngẫu nhiên " + run.Seed + " · mã lượt " + run.RunId);
            if (run.Endless) lines.AppendLine("Chặng vô tận · lượt trước " + run.ParentRunId);
            lines.AppendLine("\nĐIỂM: 1.000 / vượt màn + 10 / hạ gục + 500 / trùm + 5.000 / thắng.\nNhân 1 + 0,2 × bậc khó; sát thương, hồi máu và vàng không cho điểm.");
            return lines.ToString();
        }
        public void Draw(Rect rectangle, RunSnapshot run, RunCatalog catalog, string nickname = null, int frozenScore = -1)
        {
            if (identity != run.RunId) { identity = run.RunId; scroll = Vector2.zero; cachedBuild = null; }
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.PageDown) { scroll.y += 300; Event.current.Use(); }
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.PageUp) { scroll.y = Mathf.Max(0, scroll.y - 300); Event.current.Use(); }
            ArcadeSkin.Line(new Rect(rectangle.x + 5, rectangle.y + 7, rectangle.width, rectangle.height), ArcadeSkin.Ink);
            ArcadeSkin.Line(rectangle, ArcadeSkin.Paper);
            for (float horizontal = rectangle.x; horizontal < rectangle.xMax; horizontal += 22)
                ArcadeSkin.Line(new Rect(horizontal, rectangle.yMax - 6, 10, 6), ArcadeSkin.Ink);
            GUIStyle heading = headingStyle ?? (headingStyle = ArcadeTypography.Style(26, true));
            GUIStyle body = bodyStyle ?? (bodyStyle = ArcadeTypography.Style(17));
            GUIStyle number = numericStyle ?? (numericStyle = ArcadeTypography.Style(19, false, true));
            heading.normal.textColor = body.normal.textColor = number.normal.textColor = ArcadeSkin.Ink;
            GUI.Label(new Rect(rectangle.x + 24, rectangle.y + 15, rectangle.width - 48, 45), nickname ?? RunMeta.Nickname(run), heading);
            string summary = (run.Endless ? "VÔ TẬN" : "CHUYẾN ĐI") + " / " + RunMeta.DifficultyNames[Mathf.Clamp(run.Difficulty, 0, 4)] + " / " + (run.Phase == SessionPhase.Victory ? "BÌNH MINH" : "KHÉP LẠI");
            GUI.Label(new Rect(rectangle.x + 24, rectangle.y + 65, rectangle.width - 48, 35), summary, body);
            GUI.Label(new Rect(rectangle.x + 24, rectangle.y + 105, rectangle.width - 48, 55), "ĐIỂM " + (frozenScore < 0 ? RunMeta.Score(run) : frozenScore).ToString("N0") + "  ·  MÀN " + run.Wave + "\n" + run.TotalKills + " hạ gục / " + run.BossKills + " trùm / " + (run.CombatSeconds / 60).ToString("0.0") + " phút", number);
            ArcadeSkin.Divider(new Rect(rectangle.x + 24, rectangle.y + 170, rectangle.width - 48, 2), ArcadeSkin.Ink);
            if (cachedBuild != run) { cachedBuild = run; cachedContent = Describe(run, catalog); }
            string content = cachedContent;
            float gridHeight = Mathf.CeilToInt(run.Roster.Count / Mathf.Max(1, Mathf.Floor((rectangle.width - 70) / 90))) * 88;
            float height = Mathf.Max(rectangle.height - 200, gridHeight + body.CalcHeight(new GUIContent(content), rectangle.width - 85) + 20);
            if (scrollToEnd) { scroll.y = height; scrollToEnd = false; }
            scroll = GUI.BeginScrollView(new Rect(rectangle.x + 20, rectangle.y + 185, rectangle.width - 40, rectangle.height - 208), scroll, new Rect(0, 0, rectangle.width - 62, height));
            int columns = Mathf.Max(1, Mathf.FloorToInt((rectangle.width - 70) / 90));
            for (int index = 0; index < run.Roster.Count; index++)
            {
                RosterEntry entry = run.Roster[index];
                float left = index % columns * 90;
                float top = index / columns * 88;
                RunHud.Sprite(new Rect(left + 10, top, 48, 50), catalog.Unit(entry.UnitId)?.Body);
                RunHud.Sprite(new Rect(left + 45, top + 25, 32, 32), catalog.Weapon(entry.WeaponId)?.Sprite);
                GUI.Label(new Rect(left + 5, top + 57, 80, 27), "Cấp " + entry.Tier, body);
            }
            GUI.Label(new Rect(6, gridHeight, rectangle.width - 85, height - gridHeight), content, body);
            GUI.EndScrollView();
        }
        public static string ExportFrame(string runId)
        {
            string directory = Path.Combine(RunPaths.Root, "Receipts");
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, "Receipt-" + runId + "-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + ".png");
            ScreenCapture.CaptureScreenshot(path);
            return path;
        }
    }
}
