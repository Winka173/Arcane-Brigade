using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Bot campaign through Act I: per-wave pacing (length, kills, crowd) and economy (combat coins, chests, income, interest).
    /// Bot observations, not human balance acceptance. Batch: -executeMethod ArcaneBrigade.Editor.PacingEconomyDiagnostics.Run</summary>
    public static class PacingEconomyDiagnostics
    {
        private static readonly string[] Heroes = { "LanternKnight", "Postmaster", "DrowsySeer", "VerdantMatron", "TideSinger", "IvorySovereign" };
        public static void Run()
        {
            string root = "PreviewExports/Pacing";
            foreach (string arg in System.Environment.GetCommandLineArgs()) if (arg.StartsWith("-pacing-out=")) root = arg.Substring(12);
            int code = 0;
            try { RunInto(root); }
            catch (System.Exception exception) { Debug.LogException(exception); code = 1; }
            EditorApplication.Exit(code);
        }

        public static void RunInto(string root)
        {
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            var rows = new List<string> { "hero,wave,seconds,kills,peak_alive,mean_alive,combat_gold,chests,income,interest,wallet_after_settle,spent,outcome" };
            var summary = new List<string> { "hero,outcome,last_wave,total_combat_gold,total_income,total_interest,total_spent,total_kills" };
            var keeper = new List<string> { "hero,difficulty,keeper_hp,brigade,result,seconds,hero_hp_left,keeper_hp_left_pct,wave15_boss_seconds" };
            foreach (string heroId in Heroes)
            {
                if (catalog.Unit(heroId) == null) continue;
                const int seed = 424242;
                using var session = new RunSession(catalog); session.Start(heroId, seed);
                string outcome = "TIMEOUT";
                long gold = 0, income = 0, interest = 0, spent = 0, kills = 0;
                for (int wave = 0; wave < catalog.NormalWaves; wave++)
                {
                    var simulation = new CombatSimulation(session.PrepareCombat()); simulation.Start(seed + wave);
                    int limit = Mathf.CeilToInt((90 + 240) * 60);
                    int peak = 0; long aliveSum = 0, samples = 0;
                    for (int tick = 0; tick < limit && simulation.Phase == RunPhase.Combat; tick++)
                    {
                        simulation.Step(1f / 60, K13MovementPolicy.Steer(simulation));
                        if (tick % 30 == 0)
                        {
                            int alive = simulation.Units.Count(unit => unit.Alive && unit.Faction == Faction.Enemy);
                            peak = Mathf.Max(peak, alive); aliveSum += alive; samples++;
                        }
                    }
                    if (simulation.Phase == RunPhase.Combat) break;
                    int w = session.State.Wave; float seconds = simulation.Elapsed; int combatGold = simulation.Gold, chests = simulation.ChestsOpened, waveKills = simulation.Kills;
                    session.Settle(simulation);
                    gold += combatGold; kills += waveKills;
                    string state = session.State.Phase.ToString();
                    if (session.State.Phase == SessionPhase.Defeat) outcome = "DEFEAT";
                    if (session.State.Phase == SessionPhase.Victory) outcome = "VICTORY";
                    int wallet = session.State.Gold;
                    income += session.State.LastIncome; interest += session.State.LastInterest;
                    if (session.State.Phase == SessionPhase.Shop && session.OutsideChallengeAvailable) keeper.Add(KeeperProbe(catalog, session, rows));
                    if (session.State.Phase == SessionPhase.Shop) K13MovementPolicy.Purchase(session, catalog);
                    spent += wallet - session.State.Gold;
                    rows.Add(string.Format(CultureInfo.InvariantCulture, "{0},{1},{2:0.0},{3},{4},{5:0.0},{6},{7},{8},{9},{10},{11},{12}", heroId, w, seconds, waveKills, peak, samples == 0 ? 0 : aliveSum / (float)samples, combatGold, chests, session.State.LastIncome, session.State.LastInterest, wallet, wallet - session.State.Gold, state));
                    if (session.State.Phase != SessionPhase.Shop) break;
                    session.ContinueWave();
                }
                summary.Add(string.Join(",", heroId, outcome, session.State.Wave, gold, income, interest, spent, kills));
                Debug.Log("PACING_ROUTE " + summary[summary.Count - 1]);
            }
            Directory.CreateDirectory(root);
            File.WriteAllLines(Path.Combine(root, "Waves.csv"), rows);
            File.WriteAllLines(Path.Combine(root, "Summary.csv"), summary);
            File.WriteAllLines(Path.Combine(root, "Keeper17.csv"), keeper);
            Debug.Log("PACING_REPORT_FINISHED " + root);
        }

        /// <summary>The Act I Keeper's hand (shop 17), fought on a copy of the bot's run so the campaign itself goes on.</summary>
        private static string KeeperProbe(RunCatalog catalog, RunSession session, List<string> rows)
        {
            using var probe = new RunSession(catalog);
            probe.Restore(JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(session.State)));
            if (!probe.AcceptOutsideStage()) return session.State.ChampionId + ",could not accept";
            var simulation = new CombatSimulation(probe.PrepareCombat()); simulation.Start(5017);
            float keeperHp = simulation.Boss != null ? simulation.Boss.Definition.Health : 0;
            for (int tick = 0; tick < 300 * 60 && simulation.Phase == RunPhase.Combat; tick++) simulation.Step(1f / 60, K13MovementPolicy.Steer(simulation));
            string result = simulation.Phase == RunPhase.Combat ? "TIMEOUT" : simulation.BossDefeated ? "WIN" : "LOSS";
            string boss15 = rows.Where(r => r.StartsWith(session.State.ChampionId + ",15,")).Select(r => r.Split(',')[2]).FirstOrDefault() ?? "-";
            string line = string.Format(CultureInfo.InvariantCulture, "{0},{1},{2:0},{3},{4},{5:0.0},{6:0},{7:0},{8}", session.State.ChampionId, session.State.Difficulty, keeperHp, session.State.Roster.Count, result, simulation.Elapsed,
                simulation.Champion.Health, simulation.Boss == null || keeperHp <= 0 ? 0 : 100 * Mathf.Max(0, simulation.Boss.Health) / keeperHp, boss15);
            Debug.Log("KEEPER17 " + line);
            return line;
        }
    }
}
