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
        private float revealedAt;
        public void ScrollToEnd() { scrollToEnd = true; }
        public static string Describe(RunSnapshot run, RunCatalog catalog)
        {
            var lines = new StringBuilder();
            UnitDefinition champion = catalog.Unit(run.ChampionId);
            lines.AppendLine("CHAMPION   " + (champion == null ? RunUICopy.UnitName(run.ChampionId) : RunUICopy.UnitName(champion)));
            lines.AppendLine("Weapon: " + (champion == null ? "Unknown" : RunUICopy.WeaponName(champion.Weapon.Id)));
            lines.AppendLine("\nACTUAL BRIGADE   " + run.Roster.Count + " units");
            foreach (RosterEntry entry in run.Roster)
                lines.AppendLine(RunUICopy.UnitName(entry.UnitId, catalog.Unit(entry.UnitId)?.DisplayName) + " · " + RunUICopy.WeaponName(entry.WeaponId) + " · tier " + entry.Tier);
            lines.AppendLine("\nRELIC DRAWER");
            foreach (ItemStack stack in run.Items) lines.AppendLine((catalog.Item(stack.Id)?.Name ?? stack.Id) + " ×" + stack.Count);
            if (run.Items.Count == 0) lines.AppendLine("No relics.");
            lines.AppendLine("\nMEASURED CONTRIBUTIONS   /   actual HP");
            var contributions = new List<ContributionEntry>(run.Contributions);
            contributions.Sort((first, second) => first.Damage == second.Damage ? string.CompareOrdinal(first.Key, second.Key) : second.Damage.CompareTo(first.Damage));
            foreach (ContributionEntry entry in contributions)
            {
                lines.AppendLine(RunUICopy.UnitName(entry.UnitId, catalog.Unit(entry.UnitId)?.DisplayName) + " · " + RunUICopy.WeaponName(entry.WeaponId) + " · tier " + entry.Tier + " / " + RunUICopy.ContributionName(entry.EffectId, catalog));
                lines.AppendLine("  Damage " + entry.Damage.ToString("0.#") + "  |  healing " + entry.Healing.ToString("0.#") + "  |  kills " + entry.Kills + "  |  gold " + entry.Coins + "  |  activations " + entry.Activations);
            }
            if (contributions.Count == 0) lines.AppendLine("No measured contributions yet; not inferred from tiers.");
            lines.AppendLine("Excludes damage beyond remaining HP; secondary effects are recorded once for actual HP changes.\n");
            if (run.LegacyContributionKills) lines.AppendLine("Old saves did not measure source-specific kills. Records begin at the upgrade; old figures are not reconstructed.");
            if (run.Phase == SessionPhase.Defeat) lines.AppendLine("FINAL BLOW\n" + (string.IsNullOrEmpty(run.LastDamageSource) ? "Unknown source in an older save" : RunUICopy.DamageSource(run.LastDamageSource, catalog)) + " · " + RunUICopy.DamageKind(run.LastDamageKind) + " · " + run.LastDamageAmount.ToString("0.#") + " damage");
            lines.AppendLine("\nSeed " + run.Seed + " · run ID " + run.RunId);
            if (run.Endless) lines.AppendLine("Endless journey · previous run " + run.ParentRunId);
            lines.AppendLine("\nSCORE: 1,000 / cleared wave + 10 / kill + 500 / boss + 5,000 / victory.\nMultiply by 1 + 0.2 × difficulty; damage, healing and gold grant no score.");
            if (catalog.StatRevision > 0) lines.AppendLine("Committed build multiplier: ×" + run.BuildScoreMultiplier.ToString("0.###") + ". Purchased stats last for this run, not new runs.");
            return lines.ToString();
        }
        public void Draw(Rect rectangle, RunSnapshot run, RunCatalog catalog, string nickname = null, int frozenScore = -1)
        {
            if (identity != run.RunId) { identity = run.RunId; scroll = Vector2.zero; cachedBuild = null; revealedAt = Time.unscaledTime; CheckpointAudio.PlayInterfaceId("ui.receipt"); }
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
            string summary = (run.Endless ? "ENDLESS" : "JOURNEY") + " / " + RunMeta.DifficultyNames[Mathf.Clamp(run.Difficulty, 0, 4)] + " / " + (run.Phase == SessionPhase.Victory ? "DAWN" : "CLOSED");
            GUI.Label(new Rect(rectangle.x + 24, rectangle.y + 65, rectangle.width - 48, 35), summary, body);
            GUI.Label(new Rect(rectangle.x + 24, rectangle.y + 105, rectangle.width - 48, 55), "SCORE " + (frozenScore < 0 ? RunMeta.Score(run) : frozenScore).ToString("N0") + "  ·  WAVE " + run.Wave + "\n" + run.TotalKills + " kills / " + run.BossKills + " bosses / " + (run.CombatSeconds / 60).ToString("0.0") + " minutes", number);
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
                float reveal = LivingUI.Reveal(Time.unscaledTime - revealedAt - Mathf.Min(index, 12) * 0.025f, PresentationSettings.ReducedMotion);
                Color previousTint = GUI.color;
                GUI.color *= new Color(1, 1, 1, reveal);
                RunHud.Sprite(new Rect(left + 10, top + (1 - reveal) * 8, 48, 50), catalog.Unit(entry.UnitId)?.Body);
                RunHud.Sprite(new Rect(left + 45, top + 25, 32, 32), catalog.Weapon(entry.WeaponId)?.Sprite);
                GUI.color = previousTint;
                GUI.Label(new Rect(left + 5, top + 57, 80, 27), "Tier " + entry.Tier, body);
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
