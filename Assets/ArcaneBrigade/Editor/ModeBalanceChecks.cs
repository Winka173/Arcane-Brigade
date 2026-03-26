using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class ModeBalanceChecks
    {
        public static void Run()
        {
            var notes = new List<string>();
            void Require(bool value, string caption) { if (!value) throw new InvalidOperationException("Mode balance: " + caption); notes.Add("PASS " + caption); }
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            foreach (int wave in new[] { 1, 3, 4, 8, 12, 20, 21, 30 })
            {
                Require(JourneyBalance.EnemyHealth(0, wave) <= 1 && JourneyBalance.EnemyDamage(0, wave) <= .62f && JourneyBalance.EnemyDensity(0, wave) == 1, "Peaceful is the gentlest layer (no tougher than the old base; balance pass round 2, 19/10/2026) " + wave);
                for (int difficulty = 1; difficulty < RunMeta.DifficultyNames.Length; difficulty++)
                {
                    Require(JourneyBalance.EnemyHealth(difficulty, wave) > JourneyBalance.EnemyHealth(difficulty - 1, wave), "Strict HP difficulty ladder " + difficulty + "/" + wave);
                    Require(JourneyBalance.EnemyDamage(difficulty, wave) > JourneyBalance.EnemyDamage(difficulty - 1, wave), "Strict damage difficulty ladder " + difficulty + "/" + wave);
                    Require(JourneyBalance.EnemyDensity(difficulty, wave) > JourneyBalance.EnemyDensity(difficulty - 1, wave), "Strict density difficulty ladder " + difficulty + "/" + wave);
                }
            }
            Require(JourneyBalance.EnemyHealth(5, 8) <= 3 && JourneyBalance.EnemyDamage(5, 8) > .62f + 5 * .095f, "Highest difficulty trades stacked HP for late danger");
            Require(JourneyBalance.Ramp(1) < JourneyBalance.Ramp(4) && JourneyBalance.Ramp(8) == JourneyBalance.Ramp(40), "Early build window then stable difficulty layer");
            Require(JourneyBalance.LoneJellyDamage(1) >= 1.2f && JourneyBalance.LoneJellyDamage(20) <= 1.5f, "Bounded solo catchup");
            foreach (UnitDefinition hero in catalog.Champions)
                for (int difficulty = 0; difficulty < RunMeta.DifficultyNames.Length; difficulty++)
                    using (var session = new RunSession(catalog))
                    {
                        session.Start(hero.Id, 161026, difficulty);
                        foreach (int wave in new[] { 1, 4, 8, 12, 20, 21, 30 })
                        {
                            session.State.Wave = wave; session.State.Endless = wave > 20;
                            CheckpointCatalog combat = session.PrepareCombat();
                            Require(float.IsFinite(combat.MeleeEnemy.Health) && combat.MeleeEnemy.Health > 0 && float.IsFinite(combat.MeleeEnemy.Weapon.Damage) && combat.MeleeEnemy.Weapon.Damage >= 0, "Finite compiled enemies " + hero.Id + "/" + difficulty + "/" + wave);
                            bool expectedCrowd = wave == 20 ? combat.EnemyBudget == 0 && combat.Boss?.Id == NightmareRules.CustomerId : combat.EnemyBudget > 0;
                            Require(float.IsFinite(combat.Champion.Health) && combat.Champion.Health > 0 && expectedCrowd, "Finite champion and production crowd or final duel " + hero.Id + "/" + difficulty + "/" + wave);
                            Require(session.SpecialStagePriceFactor == 1 && session.StagePriceFactor >= 1, "Special currency stays unscaled " + hero.Id + "/" + difficulty + "/" + wave);
                            if (wave == 1) Require(combat.MeleeEnemy.Weapon.Kind == WeaponKind.Contact && combat.RangedEnemy.Weapon.Kind == WeaponKind.Contact, "No ranged wave-one enemies " + hero.Id + "/" + difficulty);
                        }
                    }
            foreach (string mode in RunMeta.ChallengeIds.Skip(1))
            {
                int index = Array.IndexOf(RunMeta.ChallengeIds, mode);
                using var session = new RunSession(catalog);
                session.Start("LanternKnight", RunMeta.ChallengeSeed(index), 0, mode);
                Require(session.State.Difficulty == 0 && session.State.ChallengeId == mode, "Challenge retains fixed rules " + mode);
                bool rejected = false;
                try { session.Start("LanternKnight", RunMeta.ChallengeSeed(index), 1, mode); } catch (ArgumentException) { rejected = true; }
                Require(rejected, "Unsupported challenge difficulty remains rejected " + mode);
            }
            Directory.CreateDirectory("PreviewExports/ModeBalance"); File.WriteAllLines("PreviewExports/ModeBalance/Checks.txt", notes);
            Debug.Log("MODE_BALANCE_CHECKS " + notes.Count + " PASS");
        }
        public static void Gate()
        {
            Run(); FinalQualityChecks.CheckAndBuild();
        }
    }
}
