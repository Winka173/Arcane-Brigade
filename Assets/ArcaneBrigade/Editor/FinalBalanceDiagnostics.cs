using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class FinalBalanceDiagnostics
    {
        public static void Run()
        {
            Directory.CreateDirectory("PreviewExports/FinalQuality");
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            var rows = new List<string> { "hero,approach,difficulty,seed,wave,result,seconds,kills,gold_earned,gold_before_shop,gold_after_shop,eggs,allies,items,hp,max_hp" };
            int runs = 0;
            foreach (UnitDefinition hero in catalog.Champions)
                for (int approach = 0; approach < 3; approach++)
                    foreach (int difficulty in new[] { 0, RunMeta.DifficultyNames.Length - 1 })
                    {
                        const int seed = 161026;
                        using (var session = new RunSession(catalog))
                        {
                            session.Start(hero.Id, seed, difficulty); session.State.HeroIdentity.Approach = approach;
                            for (int encounter = 0; encounter < 20; encounter++)
                            {
                                int wave = session.State.Wave;
                                CheckpointCatalog combat = session.PrepareCombat();
                                var simulation = new CombatSimulation(combat); simulation.Start(unchecked(seed + wave * 397));
                                for (int tick = 0; tick < 1800 && simulation.Phase == RunPhase.Combat; tick++) simulation.Step(.05f, K13MovementPolicy.Steer(simulation));
                                if (!float.IsFinite(simulation.Champion.Health) || simulation.Gold < 0) throw new InvalidOperationException("Invalid balance state " + hero.Id);
                                bool settled = simulation.Phase == RunPhase.Victory || simulation.Phase == RunPhase.Defeat;
                                if (settled) session.Settle(simulation);
                                int beforeShop = session.State.Gold;
                                if (session.State.Phase == SessionPhase.Shop) K13MovementPolicy.Purchase(session, catalog);
                                int items = 0; foreach (ItemStack stack in session.State.Items) items += stack.Count;
                                rows.Add(string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},{5},{6:0.00},{7},{8},{9},{10},{11},{12},{13},{14:0.00},{15:0.00}", hero.Id, approach, difficulty, seed, wave, simulation.Phase, simulation.Elapsed, simulation.Kills, simulation.Gold, beforeShop, session.State.Gold, session.State.Eggs, session.State.Roster.Count, items, simulation.Champion.Health, simulation.Champion.Definition.Health));
                                File.WriteAllLines("PreviewExports/FinalQuality/Natural_Progression.csv", rows);
                                if (!settled || session.State.Phase != SessionPhase.Shop) break;
                                session.ContinueWave();
                            }
                        }
                        runs++;
                        if (runs % 12 == 0) Debug.Log("FINAL_BALANCE_PROGRESS " + runs + "/186");
                    }
            File.WriteAllText("PreviewExports/FinalQuality/Balance_Method.txt", runs + " natural runs, 31 heroes x 3 approaches x 2 difficulties, one fixed seed. Production stats, actual shop prices/quotas/merge rules, no injected income/immortality. K13 deterministic steering/purchasing is not human skill or a build optimiser. Each wave stops at 90 seconds and is recorded as unresolved if still in combat. No timeout is credited as a win. Coverage of all approaches is not approval of every item combination.\n");
            Debug.Log("FINAL_BALANCE_COMPLETE " + runs + " natural runs; " + (rows.Count - 1) + " wave samples");
        }
    }
}
