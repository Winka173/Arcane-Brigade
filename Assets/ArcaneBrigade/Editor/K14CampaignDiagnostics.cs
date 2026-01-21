using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K14CampaignDiagnostics
    {
        [MenuItem("Arcane Brigade/K14/Campaign diagnostics")]
        public static void Run()
        {
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            string original = JsonUtility.ToJson(catalog);
            var rows = new List<string> { "champion,difficulty,seed,outcome,hall,kills,bosses,seconds,allies,score,endless_hall" };
            foreach (UnitDefinition champion in catalog.Champions)
                for (int difficulty = 0; difficulty < 5; difficulty++)
                foreach (int seed in difficulty == 0 ? new[] { 77107, 77138, 77169 } : new[] { 77107 + difficulty * 31 })
                {
                    using (var session = new RunSession(catalog))
                    {
                        session.Start(champion.Id, seed, difficulty);
                        string outcome = "TIMEOUT"; int endlessHall = 0;
                        for (int hall = 0; hall < 25; hall++)
                        {
                            CheckpointCatalog combat = session.PrepareCombat();
                            var simulation = new CombatSimulation(combat); simulation.Start(combat.Seed);
                            int limit = Mathf.CeilToInt((catalog.WaveSeconds + 120) * 60);
                            for (int tick = 0; tick < limit && simulation.Phase == RunPhase.Combat; tick++)
                            {
                                Vector2 direction = Steer(simulation);
                                simulation.Step(1f / 60, direction);
                            }
                            if (simulation.Phase == RunPhase.Combat) break;
                            session.Settle(simulation);
                            if (session.State.Phase == SessionPhase.Defeat) { outcome = "DEFEAT"; break; }
                            if (session.State.Endless) endlessHall = session.State.Wave;
                            if (session.State.Phase == SessionPhase.Victory) { outcome = "VICTORY"; break; }
                            K13MovementPolicy.Purchase(session, catalog);
                            session.ContinueWave();
                        }
                        int normalHall = session.State.Wave; int kills = session.State.TotalKills; int bosses = session.State.BossKills;
                        float seconds = session.State.CombatSeconds; int allies = session.State.Roster.Count; int score = RunMeta.Score(session.State);
                        if (session.State.Phase == SessionPhase.Victory)
                        {
                            session.EnterEndless(); K13MovementPolicy.Purchase(session, catalog); session.ContinueWave();
                            for (int hall = 0; hall < 3; hall++)
                            {
                                CheckpointCatalog combat = session.PrepareCombat(); var simulation = new CombatSimulation(combat); simulation.Start(combat.Seed);
                                for (int tick = 0; tick < Mathf.CeilToInt((catalog.WaveSeconds + 120) * 60) && simulation.Phase == RunPhase.Combat; tick++) simulation.Step(1f / 60, Steer(simulation));
                                if (simulation.Phase == RunPhase.Combat) break;
                                session.Settle(simulation); endlessHall = session.State.Wave;
                                if (session.State.Phase == SessionPhase.Defeat) break;
                                K13MovementPolicy.Purchase(session, catalog); session.ContinueWave();
                            }
                        }
                        rows.Add(string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},{5},{6},{7:0.0},{8},{9},{10}", champion.Id, difficulty, seed, outcome, normalHall, kills, bosses, seconds, allies, score, endlessHall));
                    }
                }
            if (original != JsonUtility.ToJson(catalog)) throw new InvalidOperationException("Balance diagnostics mutated authored catalog");
            Directory.CreateDirectory("PreviewExports/K14"); File.WriteAllLines("PreviewExports/K14/Movement_Bot_Balance.csv", rows);
            foreach (UnitDefinition champion in catalog.Champions)
                if (!rows.Any(row => row.StartsWith(champion.Id + ",0,") && row.Contains(",VICTORY,"))) throw new InvalidOperationException("No complete normal route for " + champion.Id);
            Debug.Log("K14_CAMPAIGN_DIAGNOSTICS_FINISHED " + (rows.Count - 1) + "; not human balance approval");
        }
        private static Vector2 Steer(CombatSimulation simulation) => K13MovementPolicy.Steer(simulation);
    }
}
