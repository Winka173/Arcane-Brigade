using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class BossReliefChecks
    {
        public static void Run()
        {
            var notes = new List<string>();
            void Check(bool ok, string message)
            {
                if (!ok) throw new InvalidOperationException(message);
                notes.Add("PASS " + message);
            }
            var catalog = Resources.Load<RunCatalog>("RunCatalog");
            var build = typeof(RunSession).GetMethod("BuildEnemy", BindingFlags.Instance | BindingFlags.NonPublic);
            for (int difficulty = 0; difficulty < RunMeta.DifficultyNames.Length; difficulty++)
            {
                foreach (int wave in new[] { 5, 10, 15, 20, 24 })
                using (var run = new RunSession(catalog))
                {
                    run.Start("LanternKnight", 17026, difficulty);
                    run.State.Wave = wave; run.State.Endless = wave > 20;
                    var arena = run.PrepareCombat();
                    Check(arena.Boss != null && arena.Boss.Health > 0, "Boss prepared " + difficulty + "/" + wave);
                    float health = arena.Boss.Health;
                    Check(Mathf.Approximately(run.PrepareCombat().Boss.Health, health), "Rebuilding does not stack relief " + difficulty + "/" + wave);
                    float scaling = (1 + (wave - 1) * .1f) * JourneyBalance.EnemyHealth(difficulty, wave);
                    var ordinary = (UnitDefinition)build.Invoke(run, new object[] { catalog.Enemies[(wave - 1) % catalog.Enemies.Length], scaling });
                    Check(Mathf.Approximately(arena.MeleeEnemy.Health, ordinary.Health * OctoberBalance.EnemyHealthRelief * OctoberBalance.NormalEnemyHealth), "Ordinary enemy health gets only the shared relief and the round-3 normal-wave factor " + difficulty + "/" + wave);
                    if (wave == 20)
                        Check(Mathf.Approximately(health, NightmareAssets.Current.Customer.Health * .8f * (1 + difficulty * .2f) * WaveCadence.StoryBossBulk(20) * OctoberBalance.EnemyHealthRelief * OctoberBalance.BossHealthRelief), "Last Customer override gets the shared and boss HP relief (was hard-coded .75 x .6; relief has been .95 since 18/10) " + difficulty);
                }
                using (var run = new RunSession(catalog))
                {
                    run.Start("LanternKnight", 17026, difficulty);
                    run.State.Wave = 13; run.State.Endless = true; run.State.Phase = SessionPhase.Shop; run.State.OutsideTransfers = 3;
                    Check(run.AcceptOutsideStage(), "Keeper challenge available " + difficulty);
                    var arena = run.PrepareCombat();
                    float expected = OutsideStageAssets.Current.Boss.Health * 20 * .8f * 1.8f * (1 + difficulty * .25f) * OctoberBalance.EnemyHealthRelief * OctoberBalance.BossHealthRelief; // was hard-coded .75 x .6; relief .95 since 18/10
                    Check(Mathf.Approximately(arena.Boss.Health, expected), "Keeper override gets 40 percent HP reduction " + difficulty);
                }
                using (var run = new RunSession(catalog))
                {
                    run.Start("LanternKnight", 17026, difficulty);
                    run.State.Wave = 10; run.State.Phase = SessionPhase.Shop;
                    Check(run.AcceptSitCloser(), "Bedside challenge available " + difficulty);
                    var arena = run.PrepareCombat();
                    run.State.Wave = 10 + ActOneCanon.SitWaveBonus;
                    float scaling = (1 + (run.State.Wave - 1) * .1f) * JourneyBalance.EnemyHealth(difficulty, run.State.Wave);
                    var raw = (UnitDefinition)build.Invoke(run, new object[] { Array.Find(catalog.Bosses, b => b.Id == "UmbraCradle"), scaling });
                    Check(Mathf.Approximately(arena.Boss.Health, raw.Health * WaveCadence.StoryBossBulk(10) * OctoberBalance.EnemyHealthRelief * OctoberBalance.BossHealthRelief), "Bedside gets 40 percent HP reduction " + difficulty);
                }
            }
            // Resolve actual damage after armor/caps. Child causes cover delayed skills and projectiles after the boss dies.
            using (var run = new RunSession(catalog))
            {
                run.Start("LanternKnight", 17026); run.State.Wave = 5;
                var arena = run.PrepareCombat(); arena.Dodge = 0; arena.Armor = 0;
                var sim = new CombatSimulation(arena); sim.Start(17026);
                sim.Champion.EntranceUntil = 0;
                var apply = typeof(CombatSimulation).GetMethod("ApplyDamage", BindingFlags.Instance | BindingFlags.NonPublic);
                float Hit(float amount, CombatCause cause, bool injected = false)
                {
                    sim.Champion.Health = sim.Champion.Definition.Health;
                    float before = sim.Champion.Health;
                    apply.Invoke(sim, new object[] { sim.Champion, amount, Faction.Enemy, "relief check", Vector2.zero, null, false, DamageElement.Physical, cause, injected, false });
                    return before - sim.Champion.Health;
                }
                foreach (float amount in new[] { 5f, 100000f })
                {
                    float ordinary = Hit(amount, new CombatCause(1, 0));
                    float boss = Hit(amount, new CombatCause(2, sim.Boss.Id));
                    Check(Mathf.Abs(boss - ordinary * OctoberBalance.BossDamageRelief) < .001f, "Boss damage reduced after HP cap (relief .7 since round 3, was .9): " + amount);
                    sim.Boss.Alive = false;
                    ((Dictionary<int, UnitState>)typeof(CombatSimulation).GetField("unitLookup", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(sim)).Remove(sim.Boss.Id);
                    Check(Mathf.Abs(Hit(amount, new CombatCause(3, sim.Boss.Id).Child("delayed-skill")) - boss) < .001f, "Delayed boss cause retains reduction: " + amount);
                }
                Check(Mathf.Abs(Hit(5, new CombatCause(4, sim.Boss.Id), true) - 5) < .001f, "Injected health costs are unchanged");
            }
            ShopFlowChecks.Run();
            Directory.CreateDirectory("PreviewExports/BossRelief");
            File.WriteAllLines("PreviewExports/BossRelief/Checks.txt", notes);
            Debug.Log("BOSS_RELIEF_CHECKS " + notes.Count + " PASS");
        }
    }
}
