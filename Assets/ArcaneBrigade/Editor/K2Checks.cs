using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K2Checks
    {
        [MenuItem("Arcane Brigade/K0-K2/Check and build existing content")]
        public static void BuildAndCheck() { Run(); FullRunBuilder.BuildWindows(); }

        public static void Run()
        {
            K1Builder.CheckExisting();
            Directory.CreateDirectory("PreviewExports/K2");
            RunCatalog content = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            var results = new List<string>();
            Action<bool, string> require = (condition, name) => { if (!condition) throw new InvalidOperationException(name); results.Add("PASS " + name); };
            using (var session = new RunSession(content))
            {
                session.Start(content.Champions[0].Id, 131026);
                CheckpointCatalog catalog = session.PrepareCombat();
                catalog.EnemyBudget = 0;
                var simulation = new CombatSimulation(catalog);
                simulation.Start(131026);
                UnitState enemy = simulation.AddFixture(catalog.MeleeEnemy, Faction.Enemy, new Vector2(0.35f, 0));
                enemy.PendingAttack = true;
                simulation.DamageFixture(enemy, enemy.Health + 1);
                require(!enemy.Alive && !enemy.PendingAttack && simulation.Kills == 1, "Logical death immediately disables attacks");
                int deathEvents = 0;
                foreach (CombatEvent value in simulation.Events) if (value.Kind == CombatEventKind.Kill && value.SubjectId == enemy.Id && value.Subject == enemy) deathEvents++;
                simulation.DamageFixture(enemy, 100);
                require(deathEvents == 1 && simulation.Kills == 1 && simulation.Pickups.Count == 1, "One death event and one real drop per unit");
                simulation.Step(1f / 60, Vector2.zero);
                require(simulation.Gold == 0 && simulation.Pickups.Count == 1, "Scatter precedes collection");
                Vector2 coinPosition = simulation.Pickups[0].Position;
                float time = simulation.Elapsed;
                simulation.Pause(true);
                for (int tick = 0; tick < 60; tick++) simulation.Step(1f / 60, Vector2.right);
                require(simulation.Elapsed == time && simulation.Pickups[0].Position == coinPosition && simulation.Gold == 0, "Pause freezes loot motion and credit");
                simulation.Pause(false);
                int pickupEvents = 0;
                for (int tick = 0; tick < 90; tick++)
                {
                    simulation.Step(1f / 60, Vector2.zero);
                    foreach (CombatEvent value in simulation.Events)
                        if (value.Kind == CombatEventKind.Pickup && value.Value > 0) pickupEvents++;
                }
                require(pickupEvents == 1 && simulation.Gold == Mathf.FloorToInt(catalog.GoldPerKill * catalog.CoinValueScale + 1e-4f) && simulation.Pickups.Count == 0, "Attraction collects exactly once with value and stable ID");
                simulation.Start(131026);
                foreach (UnitState unit in simulation.Units) unit.NextPassive = float.PositiveInfinity;
                UnitState distant = simulation.AddFixture(catalog.MeleeEnemy, Faction.Enemy, new Vector2(9, 0));
                simulation.DamageFixture(distant, distant.Health + 1);
                for (int tick = 0; tick < 60; tick++) simulation.Step(1f / 60, Vector2.zero);
                require(simulation.Gold == 0 && simulation.Pickups.Count == 1, "Distant coin requires entering pickup radius (gold " + simulation.Gold + ", pickups " + simulation.Pickups.Count + ", radius " + catalog.PickupRadius + ")");
                for (int tick = 0; tick < 180; tick++) simulation.Step(1f / 60, Vector2.right);
                require(simulation.Gold == Mathf.FloorToInt(catalog.GoldPerKill * catalog.CoinValueScale + 1e-4f) && simulation.Pickups.Count == 0, "Walking to distant coin collects through normal movement"); // each coin is worth CoinValueScale gold (0.75 since round 3)
                simulation.DamageFixture(simulation.Champion, 10000);
                require(simulation.Phase == RunPhase.Defeat && !simulation.Champion.Alive, "Champion logical death immediately ends combat");
                bool championDeath = false;
                foreach (CombatEvent value in simulation.Events) if (value.Kind == CombatEventKind.Kill && value.SubjectId == simulation.Champion.Id) championDeath = true;
                require(championDeath, "Champion emits presentation death event");
                UnitVisual visual = UnityEngine.Object.Instantiate(catalog.UnitPrefab).GetComponent<UnitVisual>();
                var feedbackObject = new GameObject("K2DeathFixture");
                DeathFeedback feedback = feedbackObject.AddComponent<DeathFeedback>();
                feedback.Configure(catalog.ImpactSprite);
                visual.Bind(enemy, catalog);
                float health = enemy.Health;
                Vector2 position = enemy.Position;
                feedback.Play(visual, enemy);
                require(feedback.ActiveCount == 1 && health == enemy.Health && position == enemy.Position, "Death snapshot is independent of logical body");
                feedback.Advance(0.1f); // an ordinary enemy pops in ~0.2 s (squash and burst); still on screen after cleanup
                require(feedback.ActiveCount == 1, "Death remnant survives logical cleanup");
                feedback.Advance(2);
                require(feedback.ActiveCount == 0, "Death remnant and attached grip expire");
                foreach (WeaponDefinition definition in content.Weapons)
                {
                    UnitDefinition unit = UnityEngine.Object.Instantiate(catalog.Champion);
                    unit.Weapon = definition;
                    var state = new UnitState { Id = 50, Definition = unit, Faction = Faction.Champion, Facing = Vector2.right, AttackDirection = Vector2.right, AttackStarted = 0 };
                    visual.Bind(state, catalog);
                    Transform grip = visual.transform.Find("GripRoot");
                    visual.Display(definition.Windup - 0.0001f, 1);
                    Quaternion before = grip.localRotation;
                    Vector3 beforePosition = grip.localPosition;
                    visual.Display(definition.Windup + 0.0001f, 1);
                    require(Quaternion.Angle(before, grip.localRotation) < 1 && (beforePosition - grip.localPosition).magnitude < 0.2f, "Impact pose rotation continuity " + definition.Id + " (angle " + Quaternion.Angle(before, grip.localRotation) + ", move " + (beforePosition - grip.localPosition).magnitude + ")");
                    UnityEngine.Object.DestroyImmediate(unit);
                }
                UnityEngine.Object.DestroyImmediate(visual.gameObject);
                UnityEngine.Object.DestroyImmediate(feedbackObject);
                var coinObject = new GameObject("K2CoinFixture");
                SpriteRenderer renderer = coinObject.AddComponent<SpriteRenderer>();
                renderer.sprite = catalog.Coin;
                CoinVisual coin = coinObject.AddComponent<CoinVisual>();
                coin.Display(new PickupState { Id = 7, Position = Vector2.one, PreviousPosition = Vector2.zero }, renderer, content.ProjectileDisc, catalog.ImpactSprite, 0.13f, 0.5f);
                require(renderer.transform.position.y > 0.5f && renderer.transform.localScale.x > 0, "Coin arc and positive spin scale");
                SpriteRenderer shadow = coinObject.transform.Find("CoinShadow").GetComponent<SpriteRenderer>();
                require(Mathf.Abs(shadow.bounds.size.x - 0.34f * 0.92f) < 0.01f, "Coin shadow world size independent of spin"); // coins are drawn 0.34 wide (shadow 92%)
                UnityEngine.Object.DestroyImmediate(coinObject);
            }
            File.WriteAllLines("PreviewExports/K2/K2_Checks.txt", results);
            Debug.Log("ARCANE_K2_CHECKS_PASSED " + results.Count);
        }
    }
}
