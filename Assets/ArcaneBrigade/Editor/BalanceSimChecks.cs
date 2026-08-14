using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class BalanceSimCheckTuning
    {
        public const int Enemies = 100, Seed = 281026;
        public const float Health = 1000, Radius = .25f, Duration = 100, Frame = .05f;
        public const float Damage = 12, Cooldown = 1;
        public const int Frames = 4;
    }

    public static class BalanceSimChecks
    {
        private static void Require(bool value, string message)
        { if (!value) throw new InvalidOperationException("BalanceSim check: " + message); }

        public static void Run()
        {
            Require(BalanceSimRecipes.All.Length == BondBuildRecipes.All.Length, "all twelve design recipes present");
            foreach (BalanceSimRecipe recipe in BalanceSimRecipes.All)
            {
                Require(HeroRulesV3Catalog.Find(recipe.HeroId) != null, "known hero " + recipe.HeroId);
                Require(recipe.CompanionIds.Length == BalanceSimTuning.CompanionCount, "ten companions");
                var ids = new HashSet<string>(StringComparer.Ordinal);
                foreach (string id in recipe.CompanionIds)
                    Require(ids.Add(id) && BondV3Catalog.Tags(id).Length > 0, "known unique companion " + id);
            }
            CombatCause cause = new CombatCause(1, 2, "bond-execute").WithAttribution("bond:execute");
            Require(cause.ItemId == "bond-execute" && cause.AttributionId == "bond:execute" && cause.Depth == 0,
                "attribution preserves gameplay cause");
            Require(cause.Child("other").AttributionId == null, "secondary effects own their attribution");
            double first = Fixture(), second = Fixture();
            Require(first == second, "same seed produces identical effective damage in a hundred-enemy fixture");
            Debug.Log("BalanceSimChecks: PASS (recipes, cause semantics, deterministic 100-enemy telemetry)");
        }

        private static double Fixture()
        {
            var assets = new List<UnityEngine.Object>();
            try
            {
                WeaponDefinition weapon = ScriptableObject.CreateInstance<WeaponDefinition>(); assets.Add(weapon);
                weapon.Id = "BalanceCheckContact"; weapon.Kind = WeaponKind.Contact;
                weapon.Damage = BalanceSimCheckTuning.Damage; weapon.Cooldown = BalanceSimCheckTuning.Cooldown;
                UnitDefinition hero = Body(assets, "BalanceCheckHero", weapon);
                UnitDefinition enemy = Body(assets, "BalanceCheckEnemy", weapon);
                CheckpointCatalog catalog = ScriptableObject.CreateInstance<CheckpointCatalog>(); assets.Add(catalog);
                catalog.Champion = hero; catalog.MeleeEnemy = catalog.RangedEnemy = enemy;
                catalog.ExplicitStartingRoster = true; catalog.EnemyBudget = 0;
                catalog.WaveDuration = BalanceSimCheckTuning.Duration;
                var simulation = new CombatSimulation(catalog); simulation.Start(BalanceSimCheckTuning.Seed);
                for (int i = 0; i < BalanceSimCheckTuning.Enemies; i++)
                    simulation.AddFixture(enemy, Faction.Enemy, Vector2.zero);
                var telemetry = new BalanceSimTelemetry(simulation);
                foreach (UnitState unit in simulation.Units)
                    if (unit.Faction == Faction.Enemy)
                        simulation.WeaponDamageFixture(unit, BalanceSimCheckTuning.Health * BalanceSimCheckTuning.Damage, DamageElement.Physical);
                simulation.Champion.Health = 0;
                simulation.Mortality.OnLethal(simulation.Champion);
                // A terminal fall at the wave endpoint contributes zero further seconds.
                telemetry.Sample(); telemetry.Finish(); telemetry.Finish();
                Require(telemetry.PeakEnemies == BalanceSimCheckTuning.Enemies, "peak counts only living enemies");
                Require(telemetry.Falls == 1 && telemetry.DownSeconds == 0, "terminal down interval clipped and Finish idempotent");
                Require(telemetry.TotalDamage == BalanceSimCheckTuning.Health * BalanceSimCheckTuning.Enemies,
                    "hundred-enemy effective damage excludes overkill");
                // A second standing friend permits continued stepping while the hero is down.
                simulation.Start(BalanceSimCheckTuning.Seed);
                UnitState companion = simulation.AddFixture(Body(assets, "BalanceCheckFriend", weapon), Faction.Companion, Vector2.zero);
                UnitState target = simulation.AddFixture(enemy, Faction.Enemy, Vector2.zero);
                var measured = new BalanceSimTelemetry(simulation);
                simulation.WeaponDamageFixture(target, BalanceSimCheckTuning.Damage, DamageElement.Physical);
                simulation.Champion.Health = 0; simulation.Mortality.OnLethal(simulation.Champion);
                for (int i = 0; i < BalanceSimCheckTuning.Frames; i++)
                { simulation.Step(BalanceSimCheckTuning.Frame, Vector2.zero); measured.Sample(); }
                measured.Finish();
                Require(measured.DownSeconds > 0 && measured.Falls == 1, "partial fallen interval measured");
                Require(measured.TotalDamage > 0, "fixture produces effective enemy damage");
                double sum = 0; foreach (BalanceSimSource source in measured.Sources) sum += source.Damage;
                Require(sum == measured.TotalDamage, "sources reconcile to effective damage");
                return measured.TotalDamage;
            }
            finally { foreach (UnityEngine.Object asset in assets) UnityEngine.Object.DestroyImmediate(asset); }
        }

        private static UnitDefinition Body(List<UnityEngine.Object> assets, string id, WeaponDefinition weapon)
        {
            UnitDefinition unit = ScriptableObject.CreateInstance<UnitDefinition>(); assets.Add(unit);
            unit.Id = id; unit.Weapon = weapon; unit.Health = BalanceSimCheckTuning.Health;
            unit.Radius = BalanceSimCheckTuning.Radius; unit.Speed = 0;
            return unit;
        }
    }
}
