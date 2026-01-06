using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K7BalanceDiagnostics
    {
        [MenuItem("Arcane Brigade/K7/Movement bot balance diagnostics")]
        public static void Run()
        {
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            string original = JsonUtility.ToJson(catalog);
            var rows = new List<string> { "champion,difficulty,seed,outcome,hall,kills,bosses,seconds,allies,score,endless_hall" };
            foreach (UnitDefinition champion in catalog.Champions)
                for (int difficulty = 0; difficulty < 5; difficulty++)
                {
                    using (var session = new RunSession(catalog))
                    {
                        int seed = 77107 + difficulty * 31; session.Start(champion.Id, seed, difficulty);
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
                            Purchase(session);
                            session.ContinueWave();
                        }
                        int normalHall = session.State.Wave; int kills = session.State.TotalKills; int bosses = session.State.BossKills;
                        float seconds = session.State.CombatSeconds; int allies = session.State.Roster.Count; int score = RunMeta.Score(session.State);
                        if (session.State.Phase == SessionPhase.Victory)
                        {
                            session.EnterEndless(); Purchase(session); session.ContinueWave();
                            for (int hall = 0; hall < 3; hall++)
                            {
                                CheckpointCatalog combat = session.PrepareCombat(); var simulation = new CombatSimulation(combat); simulation.Start(combat.Seed);
                                for (int tick = 0; tick < Mathf.CeilToInt((catalog.WaveSeconds + 120) * 60) && simulation.Phase == RunPhase.Combat; tick++) simulation.Step(1f / 60, Steer(simulation));
                                if (simulation.Phase == RunPhase.Combat) break;
                                session.Settle(simulation); endlessHall = session.State.Wave;
                                if (session.State.Phase == SessionPhase.Defeat) break;
                                Purchase(session); session.ContinueWave();
                            }
                        }
                        rows.Add(string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},{5},{6},{7:0.0},{8},{9},{10}", champion.Id, difficulty, seed, outcome, normalHall, kills, bosses, seconds, allies, score, endlessHall));
                    }
                }
            if (original != JsonUtility.ToJson(catalog)) throw new InvalidOperationException("Balance diagnostics mutated authored catalog");
            Directory.CreateDirectory("PreviewExports/K7"); File.WriteAllLines("PreviewExports/K7/Movement_Bot_Balance.csv", rows);
            Debug.Log("K7_MOVEMENT_BOT_DIAGNOSTICS_FINISHED " + (rows.Count - 1) + "; not human balance approval");
        }
        private static Vector2 Steer(CombatSimulation simulation)
        {
            Vector2 position = simulation.Champion.Position;
            Vector2 target = new Vector2(Mathf.Cos(simulation.Elapsed * 0.27f) * 8, Mathf.Sin(simulation.Elapsed * 0.27f) * 5);
            Vector2 direction = target - position;
            foreach (UnitState unit in simulation.Units)
            {
                if (unit.Faction != Faction.Enemy || !unit.Alive) continue;
                Vector2 offset = position - unit.Position; float distance = offset.magnitude;
                if (distance < 3 && distance > 0.01f) direction += offset.normalized * (3 - distance) * 3;
            }
            return Vector2.ClampMagnitude(direction, 1);
        }
        private static void Purchase(RunSession session)
        {
            for (int pass = 0; pass < 2; pass++)
            {
                var offers = new List<ShopOffer>(session.State.Offers);
                offers.Sort((first, second) => first.Kind == second.Kind ? first.Price.CompareTo(second.Price) : first.Kind == OfferKind.Companion ? -1 : 1);
                foreach (ShopOffer offer in offers) if (session.CanBuy(offer)) session.Buy(offer.Token);
                if (pass == 0 && session.State.Gold > session.RerollCost + 30) session.Reroll();
            }
        }
    }
}
