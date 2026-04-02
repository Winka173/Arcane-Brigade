using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K21BossEncounterChecks
    {
        public static void Run()
        {
            var notes = new List<string>();
            void Require(bool condition, string caption) { if (!condition) throw new InvalidOperationException("K21 " + caption); notes.Add("PASS " + caption); }
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            MethodInfo build = typeof(RunSession).GetMethod("BuildEnemy", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo signature = typeof(CombatSimulation).GetMethod("ResolveBossSignature", BindingFlags.Instance | BindingFlags.NonPublic);
            var names = new HashSet<string>();
            foreach (UnitDefinition source in catalog.Bosses)
            {
                Require(AssetDatabase.GetAssetPath(source.Body).Contains("/K21/"), "Detailed core drawing " + source.Id);
                AuthoredMotionSet motion = source.AuthoredMotion;
                Sprite[] poses = motion.Idle.Concat(motion.Walk).Concat(motion.Windup).Concat(motion.Release).Concat(motion.Recovery).ToArray();
                Require(poses.All(pose => pose != null && pose.pivot.y == 0) && poses.Distinct().Count() == 8, "Eight real ground-anchored core poses " + source.Id);
                using var session = new RunSession(catalog); session.Start("LanternKnight", 21077); session.State.Wave = 4; session.State.Roster.Clear();
                float height = source.BodyHeight, health = source.Health;
                UnitDefinition actual = (UnitDefinition)build.Invoke(session, new object[] { source, 1f });
                // bosses are drawn at 80% of the K21 scale since the expanded-content feedback pass (8b849cfb)
                Require(actual.BodyHeight >= height * 2.099f * .8f && actual.BodyHeight / (height * 1.35f * .8f) > 1.55f, "Size at least 55 percent above previous runtime (at the 80% boss scale) " + source.Id + " actual=" + actual.BodyHeight + " source=" + height);
                Require(actual.Health >= health * catalog.BossHealthMultiplier * CarnivalBalance.BossBulk(4) * CarnivalBalance.EnemyHealth(4) * 1.1f * .8f * 1.599f, "Sixty percent extra health " + source.Id + " actual=" + actual.Health + " needed=" + health * catalog.BossHealthMultiplier * CarnivalBalance.BossBulk(4) * CarnivalBalance.EnemyHealth(4) * 1.599f);
                Require(source.Health == health && source.BodyHeight == height && actual.Weapon != source.Weapon, "No source stats or weapon mutation " + source.Id);
                for (int variation = 0; variation < 2; variation++)
                {
                    Require(names.Add(BossEncounterRules.SignatureName(source.BossPattern, variation)), "Distinct signature " + source.Id + "/" + variation);
                    CheckpointCatalog arena = session.PrepareCombat();
                    actual = (UnitDefinition)build.Invoke(session, new object[] { source, 1f });
                    arena.Boss = actual; arena.EnemyBudget = 0;
                    arena.EnableBossArenaPhase = arena.EnableArenaToys = arena.EnableSpectacles = false;
                    var simulation = new CombatSimulation(arena); simulation.Start(21077);
                    UnitState boss = simulation.Boss; boss.EntranceUntil = 0; boss.Position = boss.PreviousPosition = new Vector2(-2, 0);
                    boss.ActiveBossSignature = variation; boss.TelegraphDirection = Vector2.right; boss.AbilityTarget = new Vector2(2, 0); boss.NextBossPattern = float.PositiveInfinity;
                    boss.NextContact = simulation.Champion.NextAttack = float.PositiveInfinity;
                    boss.BossRecoveryUntil = float.PositiveInfinity;
                    simulation.Champion.Position = simulation.Champion.PreviousPosition = new Vector2(8, -6);
                    signature.Invoke(simulation, new object[] { boss });
                    Require(simulation.ExpandedFields.Count > 0 && simulation.Projectiles.Count == 0, "Signature creates non-projectile attacks " + source.Id + "/" + variation);
                    Require(simulation.ExpandedFields.All(field => field.Source == boss && field.At > simulation.Elapsed && field.Damage > 0), "Real delayed damage with boss attribution " + source.Id + "/" + variation);
                    float before = simulation.Elapsed; simulation.Pause(true); simulation.Step(0.05f, Vector2.zero);
                    Require(simulation.Elapsed == before && boss.BodyLift == 0, "Pause freezes signature " + source.Id + "/" + variation); simulation.Pause(false);
                    if (BossEncounterRules.Leaps(source.BossPattern, variation))
                    {
                        for (int tick = 0; tick < 10; tick++) simulation.Step(0.05f, Vector2.zero);
                        Require(boss.BodyLift > 0.6f && boss.Position.x > -2, "Actual airborne travel rather than just scale " + source.Id + "/" + variation);
                        for (int tick = 0; tick < 14; tick++) simulation.Step(0.05f, Vector2.zero);
                        Require(boss.BodyLift == 0 && Vector2.Distance(boss.Position, boss.AbilityTarget) < 0.01f, "Landing at locked destination " + source.Id + "/" + variation);
                    }
                    simulation.EnterRest(); Require(simulation.ExpandedFields.Count == 0, "Shop removes signature damage " + source.Id + "/" + variation);
                }
                var posed = new UnitState { Definition = source, BossPoseTime = 0 };
                Vector2 first = BossPartRules.Position(posed, 1); posed.BossPoseTime = 0.8f;
                Require(Vector2.Distance(first, BossPartRules.Position(posed, 1)) > 0.005f, "Organs move on shared simulation pose clock " + source.Id);
            }
            using (var session = new RunSession(catalog))
            {
                session.Start("LanternKnight", 21077); session.State.Roster.Clear(); session.State.Wave = 1;
                CheckpointCatalog arena = session.PrepareCombat(); arena.EnemyBudget = 0; arena.EnableArenaToys = arena.EnableSpectacles = false;
                var simulation = new CombatSimulation(arena); simulation.Start(21077);
                simulation.Champion.NextAttack = float.PositiveInfinity;
                UnitDefinition target = UnityEngine.Object.Instantiate(arena.MeleeEnemy); target.Speed = 0; target.Health = 1000; target.ContactDamage = 5;
                UnitState enemy = simulation.AddFixture(target, Faction.Enemy, Vector2.zero); enemy.NextAttack = float.PositiveInfinity;
                UnitState second = simulation.AddFixture(target, Faction.Enemy, new Vector2(0.1f, 0)); second.NextAttack = float.PositiveInfinity;
                float championHealth = simulation.Champion.Health;
                for (int tick = 0; tick < 4; tick++) simulation.Step(0.05f, Vector2.zero);
                Require(enemy.Health == 1000 && simulation.Champion.Health == championHealth, "Contact windup warns before both directions of damage");
                for (int tick = 0; tick < 2; tick++) simulation.Step(0.05f, Vector2.zero);
                Require(enemy.Health < 1000 && simulation.Champion.Health < championHealth, "One collision impact damages both sides");
                Require(second.Health < 1000, "Each touching enemy gets independent outgoing collision damage");
                Require(simulation.Contributions.Any(record => record.UnitId == simulation.Champion.Definition.Id && record.Damage > 0), "Collision attributed to champion build");
                float hurt = enemy.Health; simulation.Step(0.05f, Vector2.zero);
                Require(enemy.Health == hurt, "Contact damage cannot repeat every frame");
                second.Position = second.PreviousPosition = new Vector2(8, 8);
                simulation.Champion.Position = new Vector2(4, 0); enemy.Health = 1; enemy.NextContact = enemy.NextCollision = enemy.ContactImpactAt = 0;
                simulation.Step(0.05f, Vector2.zero); simulation.Champion.Position = Vector2.zero;
                championHealth = simulation.Champion.Health;
                for (int tick = 0; tick < 7; tick++) simulation.Step(0.05f, Vector2.zero);
                Require(!enemy.Alive && simulation.Champion.Health < championHealth, "Lethal collision still resolves reciprocal enemy damage");
                UnityEngine.Object.DestroyImmediate(target);
            }
            using (var session = new RunSession(catalog))
            {
                session.Start("LanternKnight", 21077); session.State.Roster.Clear();
                float baseline = session.PrepareCombat().Champion.CollisionDamage;
                session.State.Items.Add(new ItemStack { Id = "royal-key", Count = 2 });
                Require(session.PrepareCombat().Champion.CollisionDamage > baseline, "Self damage relic also scales collision");
                session.State.Items.Add(new ItemStack { Id = "k10-collisiondamage", Count = 3 });
                Require(session.PrepareCombat().Champion.CollisionDamage > baseline + 6, "Collision relic stacks combine with self damage");
                CheckpointCatalog compiled = session.PrepareCombat();
                Require(RunBuildReadModel.Capture(session, catalog, session.State.Health).Stats.Any(row => row.Name == "Touch damage" && row.Summary.Contains(compiled.Champion.CollisionDamage.ToString("0.0"))), "Notebook shows actual collision damage, not just bonus");
                session.State.Wave = 20; session.State.Phase = SessionPhase.Shop; session.State.Gold = 100000;
                // since the 19/10/2026 balance pass the first reroll grows with the shop's price factor late in the run (2 early on)
                Require(session.RerollCost >= 2 && session.RerollCost <= Mathf.CeilToInt((2 + 2 * session.State.RerollsThisShop) * Mathf.Max(1, session.StagePriceFactor * .5f)) + 1, "Late-wave first reroll is the base two scaled by the shop price factor (" + session.RerollCost + ")");
                int firstReroll = session.RerollCost;
                Require(session.Reroll() && session.RerollCost > firstReroll && session.RerollCost <= Mathf.CeilToInt((2 + Mathf.Max(1, catalog.RerollPriceStep) * session.State.RerollsThisShop) * Mathf.Max(1, session.StagePriceFactor * .5f)) + 1, "Paid reroll increases only within its wave");
                session.State.Wave++;
                typeof(RunSession).GetMethod("RollShop", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(session, new object[] { true });
                Require(session.RerollCost == Mathf.CeilToInt(2 * Mathf.Max(1, session.StagePriceFactor * .5f)) && session.State.RerollsThisShop == 0, "Next shop boundary resets price to the first-roll fee");
                int gold = session.State.Gold; session.State.Gold = 1;
                Require(!session.Reroll() && session.State.RerollsThisShop == 0 && session.State.Gold == 1, "Denied reroll changes no counters or gold"); session.State.Gold = gold;
            }
            foreach (ItemEffect effect in new[] { ItemEffect.Health, ItemEffect.Armor, ItemEffect.ChampionDamage, ItemEffect.CollisionDamage, ItemEffect.CollisionFrequency, ItemEffect.MeleeDamage, ItemEffect.RangedDamage, ItemEffect.BulletPierce, ItemEffect.BulletSplit })
                Require(catalog.Items.Any(item => item.EffectiveModifiers.Any(modifier => modifier.Effect == effect && modifier.Value > 0 && RunModifierRules.Applies(modifier.Scope, ModifierScope.Champion))), "Purchasable self-build support: " + effect);
            foreach (float titleHeight in new[] { 24f, 38f, 52f })
            {
                Rect description = RunMuseum.DescriptionBounds(428, titleHeight);
                Require(description.y >= 428 + titleHeight + 12 && description.x >= 864 && description.xMax <= 1191 && description.yMax <= 588, "Collection text padding with heading height " + titleHeight);
            }
            Directory.CreateDirectory("PreviewExports/K21"); File.WriteAllLines("PreviewExports/K21/Focused_Checks.txt", notes);
            Debug.Log("K21_BOSS_CHECKS_PASSED " + notes.Count);
        }
    }
}
