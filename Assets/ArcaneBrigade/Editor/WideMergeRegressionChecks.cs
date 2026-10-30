using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    // Explicit combat bodies must not be replaced by v3 population selection before rules are measured.
    public static class WideMergeRegressionChecks
    {
        private const float Health = 1000, Budget = 30, Frame = .05f;
        private static void Require(bool value, string message)
        { if (!value) throw new InvalidOperationException("Wide merge regression: " + message); }

        public static void Run()
        {
            var assets = new List<UnityEngine.Object>();
            try
            {
                WeaponDefinition contact = ScriptableObject.CreateInstance<WeaponDefinition>(); assets.Add(contact);
                contact.Id = "WideContact"; contact.Kind = WeaponKind.Contact; contact.Cooldown = 1;
                UnitDefinition hero = Body(assets, "WideHero", contact);
                UnitDefinition enemy = Body(assets, "WideExplicitEnemy", contact);
                CheckpointCatalog catalog = ScriptableObject.CreateInstance<CheckpointCatalog>(); assets.Add(catalog);
                catalog.ExplicitStartingRoster = true; catalog.Champion = hero; catalog.MeleeEnemy = catalog.RangedEnemy = enemy;
                catalog.ArenaSize = new Vector2(100, 100); catalog.WaveDuration = 100;
                catalog.EnemyBudget = 0; catalog.CompanionLeash = 20;
                var simulation = new CombatSimulation(catalog); simulation.Start(23006);
                simulation.Champion.Position = simulation.Champion.PreviousPosition = Vector2.left * 6;
                UnitState target = simulation.AddFixture(enemy, Faction.Enemy, Vector2.right * 2);
                target.NextAttack = target.NextContact = float.PositiveInfinity;
                Require(ReferenceEquals(target.Definition, enemy) && target.Health == Health,
                    "explicit spawn retains requested species, health and collision body");

                UnitDefinition crumbs = Body(assets, "CrumbSwarm", contact);
                UnitState oneHit = simulation.AddFixture(crumbs, Faction.Enemy, Vector2.up * 20);
                Require(ReferenceEquals(oneHit.Definition, crumbs) && oneHit.Health == EnemyRulesV3Tuning.OneHitHealth,
                    "explicit species still registers its designed one-hit health before any weapon attack");

                WeaponDefinition bell = ScriptableObject.CreateInstance<WeaponDefinition>(); assets.Add(bell);
                bell.Id = "WideBell"; bell.Kind = WeaponKind.Contact; bell.Signature = SignatureWeapon.Bell;
                bell.Damage = Budget; bell.SignatureBeats = 3; bell.SignatureDelay = .2f; bell.SignatureScale = 1;
                UnitState bellSource = simulation.AddFixture(Body(assets, "WideBellSource", bell), Faction.Companion, Vector2.zero);
                bellSource.AttackDirection = Vector2.right;
                simulation.SignatureBeatFixture(bellSource);
                Require(simulation.PendingSignaturePulses == 1 && target.Health == Health,
                    "bell schedules one delayed volley without immediate damage");
                simulation.Step(Frame,Vector2.zero);
                Require(simulation.PendingSignaturePulses == 1 && target.Health == Health,
                    "neutral weapon fixture retains health and one pending pulse during the delay");
                simulation.EnterRest(); simulation.Start(23006);
                simulation.Champion.Position = simulation.Champion.PreviousPosition = Vector2.left * 6;
                target = simulation.AddFixture(enemy, Faction.Enemy, Vector2.right * 2);
                target.NextAttack = target.NextContact = float.PositiveInfinity;
                WeaponDefinition fan = ScriptableObject.CreateInstance<WeaponDefinition>(); assets.Add(fan);
                fan.Id = "PaperFan"; fan.Kind = WeaponKind.Melee;
                fan.Range = 3; fan.Cooldown = 100;
                WeaponDefinition fire = ScriptableObject.CreateInstance<WeaponDefinition>(); assets.Add(fire);
                fire.Id = "WideFire"; fire.Kind = WeaponKind.Contact; fire.Element = DamageElement.Ember;
                UnitState helper = simulation.AddFixture(Body(assets, "WideHelper", fan), Faction.Companion, Vector2.zero);
                UnitState ingredient = simulation.AddFixture(Body(assets, "WideIngredient", fire), Faction.Companion, Vector2.left);
                helper.AttackDirection = Vector2.right; helper.NextAttack = float.PositiveInfinity;
                simulation.K8CombatProjectileFixture(ingredient, Vector2.right * .8f, Vector2.right * 4, Budget, cause: new CombatCause(700, ingredient.Id));
                typeof(CombatSimulation).GetMethod("ResolveCooperationAttack", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(simulation, new object[] { helper, Budget, new CombatCause(701, helper.Id) });
                Require(simulation.CooperationPayoffs == 1 && simulation.Projectiles.Count == 3, "fan emits three conserved fragments");
                for (int tick = 0; tick < 25; tick++) simulation.Step(Frame, Vector2.zero);
                float credited = 0, total = 0;
                foreach (ContributionEntry entry in simulation.Contributions)
                {
                    if (entry.Damage > 0)
                    {
                        Require(entry.UnitId == ingredient.Definition.Id, "direct and Ember status damage retain ingredient attribution");
                        total += entry.Damage;
                    }
                    if (entry.EffectId == "cooperation-fanned-fire")
                    {
                        Require(entry.UnitId == ingredient.Definition.Id, "fragment damage stays with ingredient source");
                        credited += entry.Damage;
                    }
                }
                Require(credited > 0 && credited <= Budget && Math.Abs(target.Health - (Health - total)) < .001f,
                    "conserved direct budget and attributed status damage match actual health loss");
                Debug.Log("WideMergeRegressionChecks: PASS (explicit spawn, delayed bell, conserved cooperation attribution)");
            }
            finally { foreach (UnityEngine.Object asset in assets) UnityEngine.Object.DestroyImmediate(asset); }
        }

        private static UnitDefinition Body(List<UnityEngine.Object> assets, string id, WeaponDefinition weapon)
        {
            UnitDefinition body = ScriptableObject.CreateInstance<UnitDefinition>(); assets.Add(body);
            body.Id = id; body.Weapon = weapon; body.Health = Health; body.Radius = .25f;
            body.Speed = body.ContactDamage = body.CriticalChance = 0;
            return body;
        }
    }
}

