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
        public void ScrollToEnd() { scroll.y = 10000000; }
        public static string Describe(RunSnapshot run, RunCatalog catalog)
        {
            var lines = new StringBuilder();
            UnitDefinition champion = catalog.Unit(run.ChampionId);
            lines.AppendLine("NGƯỜI DẪN ĐOÀN   " + (champion == null ? run.ChampionId : champion.DisplayName));
            lines.AppendLine("Vũ khí: " + (champion == null ? "?" : champion.Weapon.Id));
            lines.AppendLine("\nĐỘI QUÂN THỰC TẾ   " + run.Roster.Count + " đơn vị");
            foreach (RosterEntry entry in run.Roster)
                lines.AppendLine((catalog.Unit(entry.UnitId)?.DisplayName ?? entry.UnitId) + " · " + entry.WeaponId + " · cấp " + entry.Tier);
            lines.AppendLine("\nNGĂN KÉO CỔ VẬT");
            foreach (ItemStack stack in run.Items) lines.AppendLine((catalog.Item(stack.Id)?.Name ?? stack.Id) + " ×" + stack.Count);
            if (run.Items.Count == 0) lines.AppendLine("Không có cổ vật.");
            lines.AppendLine("\nĐÓNG GÓP ĐÃ ĐO   /   HP hiệu lực");
            var contributions = new List<ContributionEntry>(run.Contributions);
            contributions.Sort((first, second) => first.Damage == second.Damage ? string.CompareOrdinal(first.Key, second.Key) : second.Damage.CompareTo(first.Damage));
            foreach (ContributionEntry entry in contributions)
            {
                lines.AppendLine((catalog.Unit(entry.UnitId)?.DisplayName ?? entry.UnitId) + " · " + entry.WeaponId + " · cấp " + entry.Tier + " / " + entry.EffectId);
                lines.AppendLine("  Damage " + entry.Damage.ToString("0.#") + "  |  hồi " + entry.Healing.ToString("0.#") + "  |  kills " + entry.Kills + "  |  coin " + entry.Coins + "  |  kích hoạt " + entry.Activations);
            }
            if (contributions.Count == 0) lines.AppendLine("Chưa có đóng góp được đo; không suy đoán từ tier.");
            lines.AppendLine("Không overkill; secondary được ghi một lần theo HP thật.\n");
            if (run.LegacyContributionKills) lines.AppendLine("Ledger cũ chưa đo kills theo nguồn; chỉ kill sau migration được ghi. Không dựng lại số liệu cũ.");
            if (run.Phase == SessionPhase.Defeat) lines.AppendLine("ĐÒN KẾT THÚC\n" + (string.IsNullOrEmpty(run.LastDamageSource) ? "Nguồn không có trong save cũ" : run.LastDamageSource) + " · " + run.LastDamageKind + " · " + run.LastDamageAmount.ToString("0.#") + " damage");
            lines.AppendLine("\nSeed " + run.Seed + " · ID " + run.RunId);
            if (run.Endless) lines.AppendLine("Endless segment · lượt trước " + run.ParentRunId);
            lines.AppendLine("\nĐIỂM: 1.000 / hall clear + 10 / kill + 500 / boss + 5.000 / thắng.\nNhân 1 + 0,2 × bậc khó; không điểm từ damage/heal/coin.");
            return lines.ToString();
        }
        public void Draw(Rect rectangle, RunSnapshot run, RunCatalog catalog, string nickname = null, int frozenScore = -1)
        {
            if (identity != run.RunId) { identity = run.RunId; scroll = Vector2.zero; }
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.PageDown) { scroll.y += 300; Event.current.Use(); }
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.PageUp) { scroll.y = Mathf.Max(0, scroll.y - 300); Event.current.Use(); }
            ArcadeSkin.Line(new Rect(rectangle.x + 5, rectangle.y + 7, rectangle.width, rectangle.height), ArcadeSkin.Ink);
            ArcadeSkin.Line(rectangle, ArcadeSkin.Paper);
            for (float horizontal = rectangle.x; horizontal < rectangle.xMax; horizontal += 22)
                ArcadeSkin.Line(new Rect(horizontal, rectangle.yMax - 6, 10, 6), ArcadeSkin.Ink);
            GUIStyle heading = ArcadeTypography.Style(26, true);
            GUIStyle body = ArcadeTypography.Style(17);
            GUIStyle number = ArcadeTypography.Style(19, false, true);
            heading.normal.textColor = body.normal.textColor = number.normal.textColor = ArcadeSkin.Ink;
            GUI.Label(new Rect(rectangle.x + 24, rectangle.y + 15, rectangle.width - 48, 45), nickname ?? RunMeta.Nickname(run), heading);
            string summary = (run.Endless ? "ENDLESS" : "NORMAL") + " / " + RunMeta.DifficultyNames[Mathf.Clamp(run.Difficulty, 0, 4)] + " / " + (run.Phase == SessionPhase.Victory ? "BÌNH MINH" : "KHÉP LẠI");
            GUI.Label(new Rect(rectangle.x + 24, rectangle.y + 65, rectangle.width - 48, 35), summary, body);
            GUI.Label(new Rect(rectangle.x + 24, rectangle.y + 105, rectangle.width - 48, 55), "SCORE " + (frozenScore < 0 ? RunMeta.Score(run) : frozenScore).ToString("N0") + "  ·  HALL " + run.Wave + "\n" + run.TotalKills + " kills / " + run.BossKills + " bosses / " + (run.CombatSeconds / 60).ToString("0.0") + " min", number);
            ArcadeSkin.Divider(new Rect(rectangle.x + 24, rectangle.y + 170, rectangle.width - 48, 2), ArcadeSkin.Ink);
            string content = Describe(run, catalog);
            float gridHeight = Mathf.CeilToInt(run.Roster.Count / Mathf.Max(1, Mathf.Floor((rectangle.width - 70) / 90))) * 88;
            float height = Mathf.Max(rectangle.height - 200, gridHeight + body.CalcHeight(new GUIContent(content), rectangle.width - 85) + 20);
            scroll = GUI.BeginScrollView(new Rect(rectangle.x + 20, rectangle.y + 185, rectangle.width - 40, rectangle.height - 208), scroll, new Rect(0, 0, rectangle.width - 62, height));
            int columns = Mathf.Max(1, Mathf.FloorToInt((rectangle.width - 70) / 90));
            for (int index = 0; index < run.Roster.Count; index++)
            {
                RosterEntry entry = run.Roster[index];
                float left = index % columns * 90;
                float top = index / columns * 88;
                RunHud.Sprite(new Rect(left + 10, top, 48, 50), catalog.Unit(entry.UnitId)?.Body);
                RunHud.Sprite(new Rect(left + 45, top + 25, 32, 32), catalog.Weapon(entry.WeaponId)?.Sprite);
                GUI.Label(new Rect(left + 15, top + 57, 65, 27), "T" + entry.Tier, body);
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
