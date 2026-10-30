using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    // Section 8 replaces the legacy universal three-tier transformations with 36 distinct hero rules.
    public static class HeroAwakeningChecks
    {
        [MenuItem("Arcane Brigade/Checks/Hero awakenings")]
        public static void Run()
        {
            var notes = new List<string>();
            void Require(bool value, string caption)
            {
                if (!value) throw new InvalidOperationException("HeroAwakening " + caption);
                notes.Add("PASS " + caption);
            }
            var catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            Require(catalog.Champions.Length == HeroRulesV3Tuning.HeroCount, "36 section 8 heroes installed");
            foreach (var definition in catalog.Champions)
            {
                var rule = HeroRulesV3Catalog.Find(definition.Id);
                Require(rule != null && !string.IsNullOrEmpty(rule.Mechanic), definition.Id + " has its own mechanic");
                Require(rule.BuildDirections.Length == HeroRulesV3Tuning.BuildsPerHero && rule.Relics.Length == HeroRulesV3Tuning.RelicsPerHero,
                    definition.Id + " has three build directions and four relics");
                Require(definition.Tags.SequenceEqual(HeroRulesV3Catalog.MinorIds(definition.Id)), definition.Id + " counts in its two design minors");
                using (var run = new RunSession(catalog))
                {
                    run.Start(definition.Id, 18020);
                    var combat = run.PrepareCombat(); combat.EnemyBudget = 0; combat.Boss = null;
                    var sim = new CombatSimulation(combat); sim.Start(18020);
                    Require(sim.HeroRulesV3 != null && sim.HeroRulesV3.HeroId == definition.Id, definition.Id + " starts its design runtime");
                    Require(!sim.HeroAwakenEnabled && !sim.HeroIdentityEnabled, definition.Id + " does not run the replaced transformation kit");
                    float before = sim.HeroRulesV3.Time;
                    sim.Pause(true); sim.Step(.1f, Vector2.right);
                    Require(sim.HeroRulesV3.Time == before, definition.Id + " pauses its mechanic clock");
                }
            }
            // This exercises real mechanic requests, charge consumption and persistence with a 100-enemy crowd.
            HeroRulesV3Checks.Run();
            Directory.CreateDirectory("PreviewExports/HeroAwakening");
            File.WriteAllLines("PreviewExports/HeroAwakening/Checks.txt", notes);
            Debug.Log("HERO_AWAKENING_CHECKS_PASSED " + notes.Count);
        }
    }
}
