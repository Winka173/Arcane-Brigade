using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class BalanceSimTuning
    {
        public const string CatalogPath = "Assets/ArcaneBrigade/Resources/RunCatalog.asset";
        public const string OutputDirectory = "PreviewExports/Balance";
        public const float StepSeconds = 0.05f;
        public const float WaveLimitSeconds = 240f;
        public const double BatchLimitSeconds = 540;
        public const float CircleRadius = 0.24f, CircleRadiansPerSecond = 0.32f;
        public const int CompanionCount = 10, Tier = 1;
        public static readonly int[] Seeds = { 281026, 7919, 104729 };
        public static readonly int[] Waves = { 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,40,60 };
    }

    public sealed class BalanceSimResult
    {
        public string Build, Hero, RequestedBonds, AchievedBonds, RecipeGap, Outcome;
        public int Wave, Seed, Steps, Falls, PeakEnemies;
        public double Seconds, ClearSeconds, DownSeconds, StepMeanMs, StepWorstMs, TotalDamage;
        public List<BalanceSimSource> Sources;
    }

    // No scenes, GameObjects, render loop, shop actions or changes to combat tuning.
    public static class BalanceSim
    {
        public static void Run()
        {
            BalanceSimChecks.Run();
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>(BalanceSimTuning.CatalogPath);
            if (catalog == null) throw new InvalidOperationException("Missing balance catalog: " + BalanceSimTuning.CatalogPath);
            var results = new List<BalanceSimResult>();
            var clock = Stopwatch.StartNew();
            bool complete = false;
            try
            {
                RulesV3CatalogAdapter.Configure(catalog);
                foreach (BalanceSimRecipe recipe in BalanceSimRecipes.All)
                foreach (int seed in BalanceSimTuning.Seeds)
                using (var session = new RunSession(catalog))
                {
                    session.Start(recipe.HeroId, seed);
                    session.State.Roster.Clear();
                    if (recipe.CompanionIds.Length != BalanceSimTuning.CompanionCount)
                        throw new InvalidOperationException("Recipe must deploy ten companions: " + recipe.Name);
                    foreach (string id in recipe.CompanionIds)
                    {
                        UnitDefinition unit = catalog.Unit(id);
                        if (unit == null || unit.Weapon == null) throw new InvalidOperationException("Missing recipe companion: " + id);
                        session.State.Roster.Add(new RosterEntry { UnitId = id, WeaponId = unit.Weapon.Id, Tier = BalanceSimTuning.Tier });
                    }
                    foreach (int wave in BalanceSimTuning.Waves)
                    {
                        if (clock.Elapsed.TotalSeconds >= BalanceSimTuning.BatchLimitSeconds)
                            throw new TimeoutException("BalanceSim batch budget exhausted; partial report written");
                        session.State.Wave = wave;
                        // Every checkpoint is attempted after a loss; no synthetic wins or skipped-seed losses.
                        session.State.Phase = SessionPhase.Combat;
                        CheckpointCatalog combat = session.PrepareCombat();
                        var simulation = new CombatSimulation(combat);
                        simulation.Start(combat.Seed);
                        var result = Measure(simulation, combat.ArenaSize, clock);
                        result.Build = recipe.Name; result.Hero = recipe.HeroId;
                        result.Wave = wave; result.Seed = seed;
                        DescribeBonds(recipe, combat.RuntimeBondCounts, result);
                        results.Add(result);
                        // Only the sampled waves actually won award run growth. No shop or story settlement.
                        if (simulation.Phase == RunPhase.Victory)
                        {
                            simulation.Bonds.OnWaveSurvived();
                            simulation.HeroRulesV3.OnWaveSurvived();
                            simulation.TraitsV3.OnWaveSurvived();
                        }
                        if (result.Outcome == "budget_exhausted")
                            throw new TimeoutException("BalanceSim batch budget exhausted; partial report written");
                    }
                    UnityEngine.Debug.Log("BalanceSim: " + recipe.Name + " seed " + seed + " completed");
                }
                complete = true;
            }
            finally
            {
                BalanceSimReport.Write(BalanceSimTuning.OutputDirectory, results, complete, clock.Elapsed.TotalSeconds);
                UnityEngine.Debug.Log("BalanceSim: " + results.Count + " wave samples; complete=" + complete + "; " + Path.GetFullPath(BalanceSimTuning.OutputDirectory));
            }
            if (!complete) throw new InvalidOperationException("BalanceSim incomplete; inspect balance_summary.md");
        }

        internal static BalanceSimResult Measure(CombatSimulation simulation, Vector2 arenaSize, Stopwatch batchClock)
        {
            var telemetry = new BalanceSimTelemetry(simulation);
            var result = new BalanceSimResult();
            long ticks = 0, worst = 0;
            telemetry.Sample();
            while (simulation.Phase == RunPhase.Combat && simulation.Elapsed < BalanceSimTuning.WaveLimitSeconds)
            {
                if (batchClock.Elapsed.TotalSeconds >= BalanceSimTuning.BatchLimitSeconds) break;
                Vector2 movement = CircleMovement(simulation.Champion.Position, simulation.Elapsed, arenaSize);
                long start = Stopwatch.GetTimestamp();
                simulation.Step(BalanceSimTuning.StepSeconds, movement);
                long elapsed = Stopwatch.GetTimestamp() - start;
                ticks += elapsed; if (elapsed > worst) worst = elapsed;
                result.Steps++;
                telemetry.Sample();
            }
            telemetry.Finish();
            result.Outcome = simulation.Phase == RunPhase.Victory ? "won" : simulation.Phase == RunPhase.Defeat ? "lost" :
                simulation.Elapsed >= BalanceSimTuning.WaveLimitSeconds ? "timeout" : "budget_exhausted";
            result.Seconds = simulation.Elapsed;
            result.ClearSeconds = simulation.Phase == RunPhase.Victory ? simulation.Elapsed : double.NaN;
            result.StepMeanMs = result.Steps == 0 ? 0 : ticks * 1000.0 / Stopwatch.Frequency / result.Steps;
            result.StepWorstMs = worst * 1000.0 / Stopwatch.Frequency;
            result.Falls = telemetry.Falls; result.DownSeconds = telemetry.DownSeconds;
            result.PeakEnemies = telemetry.PeakEnemies; result.TotalDamage = telemetry.TotalDamage;
            result.Sources = new List<BalanceSimSource>(telemetry.Sources);
            return result;
        }

        internal static Vector2 CircleMovement(Vector2 position, float seconds, Vector2 arenaSize)
        {
            float radius = Mathf.Min(arenaSize.x, arenaSize.y) * BalanceSimTuning.CircleRadius;
            float angle = seconds * BalanceSimTuning.CircleRadiansPerSecond;
            Vector2 destination = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            return (destination - position).normalized;
        }

        private static void DescribeBonds(BalanceSimRecipe recipe, Dictionary<string,int> counts, BalanceSimResult result)
        {
            var requested = new List<string>(); var achieved = new List<string>(); var gaps = new List<string>();
            for (int i = 0; i < recipe.TargetBondIds.Length; i++)
            {
                string id = recipe.TargetBondIds[i]; int target = recipe.TargetBondCounts[i];
                counts.TryGetValue(id, out int actual);
                requested.Add(id + " " + target);
                if (actual < target) gaps.Add(id + " " + actual + "/" + target);
            }
            foreach (string id in recipe.MinorBondIds)
            {
                requested.Add(id + " 2"); counts.TryGetValue(id, out int actual);
                if (actual < 2) gaps.Add(id + " " + actual + "/2");
            }
            var keys = new List<string>(counts.Keys); keys.Sort(StringComparer.Ordinal);
            foreach (string id in keys) if (counts[id] >= 2) achieved.Add(id + " " + counts[id]);
            result.RequestedBonds = string.Join("; ", requested);
            result.AchievedBonds = string.Join("; ", achieved);
            result.RecipeGap = string.Join("; ", gaps);
        }
    }
}
