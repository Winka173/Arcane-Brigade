using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace ArcaneBrigade.Editor
{
    public static class BalanceSimReportTuning
    {
        public const int SeedsPerWave = 3;
        public const int StepTaste = 2, StepOnline = 4, StepCore = 6, StepCapstone = 8;
        public const double TasteShare = .05, OnlineShare = .12, CoreShare = .25, CapstoneShare = .50;
        public const double OutlierMultiple = 2;
        public const int MaximumOutlierLines = 30;

        public static double ExpectedShare(int step)
        {
            switch (step)
            {
                case StepTaste: return TasteShare;
                case StepOnline: return OnlineShare;
                case StepCore: return CoreShare;
                case StepCapstone: return CapstoneShare;
                default: return 0;
            }
        }
    }

    /// <summary>Exports measured damage separately from the design's broader team-power targets.</summary>
    public static class BalanceSimReport
    {
        private sealed class BuildStats
        {
            public string Name, Hero, Requested, Achieved, Gap;
            public int Samples, Wins, Losses, ClearSamples;
            public double ClearSeconds;
            public readonly SortedDictionary<int, List<BalanceSimResult>> Waves = new SortedDictionary<int, List<BalanceSimResult>>();
        }

        private sealed class Outlier
        {
            public BalanceSimResult Result;
            public BalanceSimSource Source;
            public double Share, Expected;
            public bool Skill;
        }

        public static void Write(string directory, List<BalanceSimResult> results, bool complete, double wallSeconds)
        {
            if (results == null) throw new ArgumentNullException(nameof(results));
            Directory.CreateDirectory(directory);
            WriteCsv(Path.Combine(directory, "balance.csv"), results);
            File.WriteAllText(Path.Combine(directory, "balance_summary.md"), Summary(results, complete, wallSeconds), new UTF8Encoding(false));
        }

        private static void WriteCsv(string path, List<BalanceSimResult> results)
        {
            using (var writer = new StreamWriter(path, false, new UTF8Encoding(false)))
            {
                writer.WriteLine("build,hero,requested_bonds,achieved_bonds,recipe_gap,wave,seed,outcome,sim_seconds,time_to_clear_seconds,total_damage,friends_fallen,friend_seconds_down,enemies_alive_peak,steps,step_mean_ms,step_worst_ms,source_kind,source_id,bond_step,source_damage,damage_share");
                foreach (BalanceSimResult result in results)
                {
                    if (result.Sources == null || result.Sources.Count == 0) WriteRow(writer, result, null);
                    else foreach (BalanceSimSource source in result.Sources) WriteRow(writer, result, source);
                }
            }
        }

        private static void WriteRow(TextWriter writer, BalanceSimResult result, BalanceSimSource source)
        {
            string[] cells = {
                result.Build, result.Hero, result.RequestedBonds, result.AchievedBonds, result.RecipeGap,
                Number(result.Wave), Number(result.Seed), result.Outcome, Number(result.Seconds),
                Won(result.Outcome) && result.ClearSeconds >= 0 ? Number(result.ClearSeconds) : "",
                Number(result.TotalDamage), Number(result.Falls), Number(result.DownSeconds), Number(result.PeakEnemies),
                Number(result.Steps), Number(result.StepMeanMs), Number(result.StepWorstMs),
                source == null ? "summary" : source.Kind, source == null ? "" : source.Id,
                source == null || source.Step == 0 ? "" : Number(source.Step),
                Number(source == null ? 0 : source.Damage), Number(source == null || result.TotalDamage <= 0 ? 0 : source.Damage / result.TotalDamage)
            };
            for (int index = 0; index < cells.Length; index++)
            {
                if (index != 0) writer.Write(',');
                writer.Write(Csv(cells[index]));
            }
            writer.WriteLine();
        }

        public static string Summary(List<BalanceSimResult> results, bool complete, double wallSeconds)
        {
            var text = new StringBuilder();
            text.AppendLine("# Balance simulation");
            text.AppendLine();
            text.AppendLine((complete ? "Complete" : "INCOMPLETE: budget exhausted or samples missing") + "; " + Number(results.Count) + " build/wave/seed samples; " + Number(wallSeconds) + " s wall time.");
            text.AppendLine("Fixed seeds, fixed timestep, default difficulty, one-star companions, no purchases, scripted circular kiting, normal wave director. CSV repeats wave totals per source; group by build/wave/seed before aggregating wave metrics.");
            text.AppendLine();
            text.AppendLine("Damage is actual enemy health removed, excluding overkill. Shares describe directly attributed damage, not the full team power of buffs, healing, shields, control, economy or their interactions. Existing runtime effect labels can combine multiple mechanics; unclassified damage remains visible. Step timing measures CombatSimulation.Step only and varies by machine; it is not deterministic output. Time to clear is the runtime's victory time (survival waves can end with live enemies). Timeouts are censored, not losses.");
            text.AppendLine("Bond step is the highest active step, not a marginal decomposition of each unlocked tier. Stat and haste benefits remain credited to the damaging source. Stone/Spiky Shell reflection is combined under a shared source with unknown step; reflected skill damage names the protected recipient. Companion skill copies retain the runtime caster attribution. Falls and down seconds include the hero and companions. Peak enemies is sampled at step boundaries.");
            text.AppendLine("Wave trials continue after defeats with full-health teams. Successful sampled waves award runtime survival growth; waves 21-39 and 41-59 are skipped without synthetic growth. The 240 s wave cap censors boss overtime. The 540 s batch watchdog writes partial output and fails if exhausted; a full run under ten minutes still requires measurement in Unity batch mode.");
            text.AppendLine();
            var buildIndex = new Dictionary<string, BuildStats>(StringComparer.Ordinal);
            var ranked = new List<BuildStats>();
            var outliers = new List<Outlier>();
            foreach (BalanceSimResult result in results)
            {
                if (!buildIndex.TryGetValue(result.Build, out BuildStats stats))
                {
                    stats = new BuildStats { Name = result.Build, Hero = result.Hero, Requested = result.RequestedBonds, Achieved = result.AchievedBonds, Gap = result.RecipeGap };
                    buildIndex.Add(result.Build, stats); ranked.Add(stats);
                }
                stats.Samples++;
                if (Won(result.Outcome))
                {
                    stats.Wins++;
                    if (result.ClearSeconds >= 0) { stats.ClearSeconds += result.ClearSeconds; stats.ClearSamples++; }
                }
                if (Lost(result.Outcome)) stats.Losses++;
                if (!stats.Waves.TryGetValue(result.Wave, out List<BalanceSimResult> wave))
                { wave = new List<BalanceSimResult>(); stats.Waves.Add(result.Wave, wave); }
                wave.Add(result);
                if (result.TotalDamage <= 0 || result.Sources == null) continue;
                var screens = new Dictionary<string, BalanceSimSource>(StringComparer.Ordinal);
                foreach (BalanceSimSource item in result.Sources)
                {
                    if (!Contains(item.Kind, "bond") && !Contains(item.Kind, "skill")) continue;
                    int slash = item.Id.IndexOf('/');
                    string id = slash < 0 ? item.Id : item.Id.Substring(0, slash);
                    string key = item.Kind + "/" + id + "/" + item.Step;
                    if (!screens.TryGetValue(key, out BalanceSimSource total))
                    { total = new BalanceSimSource { Kind = item.Kind, Id = id, Step = item.Step }; screens.Add(key, total); }
                    total.Damage += item.Damage;
                }
                foreach (BalanceSimSource source in screens.Values)
                {
                    bool skill = Contains(source.Kind, "skill");
                    double expected = Contains(source.Kind, "bond") ? BalanceSimReportTuning.ExpectedShare(source.Step) : skill ? BalanceSimReportTuning.TasteShare : 0;
                    double share = source.Damage / result.TotalDamage;
                    if (expected > 0 && share > BalanceSimReportTuning.OutlierMultiple * expected)
                        outliers.Add(new Outlier { Result = result, Source = source, Share = share, Expected = expected, Skill = skill });
                }
            }
            ranked.Sort((left, right) =>
            {
                int order = right.Wins.CompareTo(left.Wins);
                if (order == 0) order = MeanClear(left).CompareTo(MeanClear(right));
                return order == 0 ? string.CompareOrdinal(left.Name, right.Name) : order;
            });
            text.AppendLine("## Builds ranked");
            text.AppendLine();
            text.AppendLine("Ranked by wins, then mean victory time; compare equal completed sample counts. These are diagnostic wave trials, including trials after a defeat, rather than run survival probabilities.");
            text.AppendLine();
            text.AppendLine("| Build | Hero | Wins / samples | Losses | Mean victory seconds |");
            text.AppendLine("|---|---|---:|---:|---:|");
            foreach (BuildStats stats in ranked)
                text.AppendLine("| " + Md(stats.Name) + " | " + Md(stats.Hero) + " | " + Number(stats.Wins) + " / " + Number(stats.Samples) + " | " + Number(stats.Losses) + " | " + (stats.ClearSamples == 0 ? "—" : Number(MeanClear(stats))) + " |");
            text.AppendLine();
            text.AppendLine("## Recipe audit");
            text.AppendLine();
            text.AppendLine("Section 4 supplies bond targets without exact heroes or companion rosters. Trials use reproducible selections; achieved counts below must be checked before treating a result as the intended recipe.");
            text.AppendLine();
            foreach (BuildStats stats in ranked)
                text.AppendLine("- **" + Md(stats.Name) + "**: requested " + Md(stats.Requested) + "; achieved " + Md(stats.Achieved) + (string.IsNullOrEmpty(stats.Gap) ? "." : ". " + Md(stats.Gap)));
            text.AppendLine();
            text.AppendLine("## Waves lost on every seed");
            text.AppendLine();
            bool anyLoss = false;
            foreach (BuildStats stats in ranked)
            {
                var failed = new List<string>();
                foreach (KeyValuePair<int, List<BalanceSimResult>> pair in stats.Waves)
                {
                    var seeds = new HashSet<int>();
                    bool allLost = pair.Value.Count == BalanceSimReportTuning.SeedsPerWave;
                    foreach (BalanceSimResult result in pair.Value) { seeds.Add(result.Seed); allLost &= Lost(result.Outcome); }
                    if (allLost && seeds.Count == BalanceSimReportTuning.SeedsPerWave) failed.Add(Number(pair.Key));
                }
                if (failed.Count == 0) continue;
                anyLoss = true; text.AppendLine("- " + Md(stats.Name) + ": " + string.Join(", ", failed) + ".");
            }
            if (!anyLoss) text.AppendLine("None among waves with all three completed seed trials.");
            text.AppendLine();
            text.AppendLine("## Damage-share outliers");
            text.AppendLine();
            text.AppendLine("Bond screens: direct damage share greater than twice 5% / 12% / 25% / 50% for active steps 2 / 4 / 6 / 8. These design targets describe team power, so this is a screening signal, not a causal balance verdict. Skills have no expected-share target in the design: skill share above 10% is listed only as a heuristic (twice the 5% step-2 proxy), not a design-target violation.");
            outliers.Sort((left, right) =>
            {
                int order = (right.Share / right.Expected).CompareTo(left.Share / left.Expected);
                if (order == 0) order = string.CompareOrdinal(left.Result.Build, right.Result.Build);
                if (order == 0) order = left.Result.Wave.CompareTo(right.Result.Wave);
                if (order == 0) order = left.Result.Seed.CompareTo(right.Result.Seed);
                return order == 0 ? string.CompareOrdinal(left.Source.Id, right.Source.Id) : order;
            });
            text.AppendLine();
            if (outliers.Count == 0) text.AppendLine("No direct-damage screening outliers in recorded samples.");
            else
            {
                text.AppendLine(Number(outliers.Count) + " flagged source samples; showing the largest " + Number(Math.Min(outliers.Count, BalanceSimReportTuning.MaximumOutlierLines)) + ". Full source shares are in balance.csv.");
                text.AppendLine();
                text.AppendLine("| Build | Wave / seed | Source | Step | Share | Screen |");
                text.AppendLine("|---|---|---|---:|---:|---|");
                for (int index = 0; index < Math.Min(outliers.Count, BalanceSimReportTuning.MaximumOutlierLines); index++)
                {
                    Outlier value = outliers[index];
                    text.AppendLine("| " + Md(value.Result.Build) + " | " + Number(value.Result.Wave) + " / " + Number(value.Result.Seed) + " | " + Md(value.Source.Id) + " | " + (value.Source.Step == 0 ? "—" : Number(value.Source.Step)) + " | " + value.Share.ToString("P2", CultureInfo.InvariantCulture) + " | " + (value.Skill ? "skill heuristic" : "bond damage proxy") + " |");
                }
            }
            return text.ToString();
        }

        private static bool Contains(string value, string fragment) => value != null && value.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0;
        private static bool Won(string outcome) => string.Equals(outcome, "won", StringComparison.OrdinalIgnoreCase) || string.Equals(outcome, "win", StringComparison.OrdinalIgnoreCase) || string.Equals(outcome, "victory", StringComparison.OrdinalIgnoreCase);
        private static bool Lost(string outcome) => string.Equals(outcome, "lost", StringComparison.OrdinalIgnoreCase) || string.Equals(outcome, "loss", StringComparison.OrdinalIgnoreCase) || string.Equals(outcome, "defeat", StringComparison.OrdinalIgnoreCase);
        private static double MeanClear(BuildStats value) => value.ClearSamples == 0 ? double.PositiveInfinity : value.ClearSeconds / value.ClearSamples;
        private static string Number(double value) => value.ToString("0.#########", CultureInfo.InvariantCulture);
        private static string Csv(string value) => "\"" + (value ?? "").Replace("\"", "\"\"") + "\"";
        private static string Md(string value) => (value ?? "").Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
    }
}
