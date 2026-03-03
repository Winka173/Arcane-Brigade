using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K8CombatChecks
    {
        [MenuItem("Arcane Brigade/K8/Combat focused checks")]
        public static void Run()
        {
            RunFocused();
            ReproduceBossTimeout();
        }

        public static void RunFocused()
        {
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            string original = JsonUtility.ToJson(catalog);
            using (var fixture = new Fixture(catalog, ExpandedWeapon.None))
            {
                UnitState first = fixture.Enemy(new Vector2(1, 0));
                UnitState second = fixture.Enemy(new Vector2(2, 0));
                fixture.Source.Definition.Weapon.ProjectilePierce = 1;
                fixture.Simulation.K8CombatProjectileFixture(fixture.Source, Vector2.zero, Vector2.right * 100, 10);
                fixture.Tick();
                Require(first.Health == 990 && second.Health == 990 && fixture.Simulation.Projectiles.Count == 0, "Swept pierce resolves both victims in one step");
                Require(fixture.Simulation.Events.Any(value => value.Kind == CombatEventKind.ProjectilePierce), "Pierce audio event emitted by swept continuation");
                fixture.Simulation.K8CombatIncomingDamageFixture(100000);
                Require(fixture.Source.Alive && fixture.Simulation.LastDamageAmount <= fixture.Source.Definition.Health * 0.22f + 0.001f, "Incoming hit capped against Champion maximum HP " + fixture.Simulation.LastDamageAmount + " of " + fixture.Source.Definition.Health);
                fixture.Source.Health = 1;
                fixture.Simulation.K8CombatIncomingDamageFixture(2);
                Require(!fixture.Source.Alive, "Damage cap does not grant immortality at low HP");
            }
            using (var fixture = new Fixture(catalog, ExpandedWeapon.TetherBow))
            {
                UnitState first = fixture.Enemy(new Vector2(1, 0));
                UnitState next = fixture.Enemy(new Vector2(1, 1));
                ProjectileState projectile = fixture.Simulation.K8CombatProjectileFixture(fixture.Source, Vector2.zero, Vector2.right * 100, 10, 2);
                fixture.Tick();
                Debug.Log("K8_RICOCHET_SAMPLE first=" + first.Health + " next=" + next.Health + " victims=" + projectile.Victims.Count + " depth=" + projectile.Cause.Depth);
                Require(first.Health == 990 && next.Health < 1000 && projectile.Victims.Count == 2 && projectile.Cause.Depth > 0, "Ricochet changes direction and hits a new victim with child attribution");
                Require(fixture.Simulation.Events.Any(value => value.Kind == CombatEventKind.ProjectileBounce), "Ricochet audio event emitted without changing damage attribution");
                Require(fixture.Simulation.Events.Where(value => value.Kind == CombatEventKind.Hit && value.Subject == next).All(value => value.Cause.RootId == projectile.Cause.RootId && value.Cause.SourceUnitId == fixture.Source.Id), "Ricochet preserves root and owner attribution");
            }
            using (var fixture = new Fixture(catalog, ExpandedWeapon.Fish))
            {
                float edge = fixture.Catalog.ArenaSize.x * 0.5f;
                UnitState target = fixture.Enemy(new Vector2(edge - 2, 0));
                ProjectileState projectile = fixture.Simulation.K8CombatProjectileFixture(fixture.Source, new Vector2(edge - 0.1f, 0), Vector2.right * 50, 10, 1);
                fixture.Tick();
                Require(target.Health == 990 && projectile.RicochetsRemaining == 0 && projectile.Returning, "Wall bounce uses reflected swept segment before Fish return");
                Require(fixture.Simulation.Events.Any(value => value.Kind == CombatEventKind.ProjectileBounce) && fixture.Simulation.Events.Count(value => value.Kind == CombatEventKind.ProjectileReturn) == 1, "Fish wall bounce and return transition each emit their audio event");
                fixture.Tick();
                Require(target.Health == 990, "Returning Fish cannot repeat-hit the same victim");
                Require(!fixture.Simulation.Events.Any(value => value.Kind == CombatEventKind.ProjectileReturn), "Returning Fish does not repeat its transition audio");
            }
            using (var fixture = new Fixture(catalog, ExpandedWeapon.Tea))
            {
                UnitState first = fixture.Enemy(new Vector2(1, 0));
                UnitState upper = fixture.Enemy(new Vector2(2, 0.8f));
                UnitState lower = fixture.Enemy(new Vector2(2, -0.8f));
                fixture.Simulation.K8CombatProjectileFixture(fixture.Source, Vector2.zero, Vector2.right * 40, 10, split: true, cause: new CombatCause(900, fixture.Source.Id, "fixture", 2));
                fixture.Tick();
                Require(fixture.Simulation.Projectiles.Count == 2 && fixture.Simulation.Projectiles.All(value => value.IsFragment && !value.SplitOnImpact && value.Cause.Depth == 3 && value.Cause.RootId == 900 && value.SourceUnitId == fixture.Source.Id && value.Faction == fixture.Source.Faction && value.Victims.Contains(first.Id)), "Impact creates exactly two nonrecursive owned child fragments");
                fixture.Tick();
                Require(upper.Health < 1000 && lower.Health < 1000 && fixture.Simulation.ExpandedFields.Count(value => value.Kind == ExpandedFieldKind.Tea) == 1, "Split fragments hit diverging targets without multiplying Tea fields");
                Require(fixture.Simulation.Events.Where(value => value.Kind == CombatEventKind.Hit).All(value => value.Cause.Depth > 0), "Secondary projectile hits remain ineligible for root-only procs");
            }
            using (var fixture = new Fixture(catalog, ExpandedWeapon.Tea))
            {
                UnitState hostile = fixture.Enemy(new Vector2(-1, 0));
                hostile.Definition.Weapon.Family = ExpandedWeapon.Tea;
                float before = hostile.Health;
                fixture.Simulation.K8CombatProjectileFixture(hostile, new Vector2(-1, 0), Vector2.right * 40, 1000, split: true);
                fixture.Tick();
                Require(hostile.Health == before && fixture.Source.Alive && fixture.Simulation.Projectiles.Count == 2 && fixture.Simulation.Projectiles.All(value => value.Faction == Faction.Enemy && value.SourceUnitId == hostile.Id), "Enemy split keeps ownership and never damages hostile units");
                fixture.Catalog.RunWave = 1;
                fixture.Tick();
                Require(fixture.Simulation.Projectiles.Count == 0, "Wave one blocks hostile fragments at simulation boundary");
            }
            using (var fixture = new Fixture(catalog, ExpandedWeapon.None))
            {
                fixture.Catalog.CompanionInterceptor = true;
                UnitState ally = fixture.Simulation.AddFixture(fixture.Catalog.Champion, Faction.Companion, new Vector2(-1, 0));
                ally.NextAttack = float.PositiveInfinity;
                UnitState hostile = fixture.Enemy(new Vector2(-3, 0));
                fixture.Simulation.K8CombatProjectileFixture(hostile, new Vector2(-3, 0), Vector2.right * 100, 1000, 2, true);
                float health = fixture.Source.Health;
                fixture.Tick();
                Require(ally.Alive && fixture.Source.Health == health && fixture.Simulation.Projectiles.Count == 0, "Companion interception consumes hostile projectile before split or ricochet");
            }
            using (var fixture = new Fixture(catalog, ExpandedWeapon.None))
            {
                UnitState enemy = fixture.Enemy(new Vector2(1, 0));
                fixture.Catalog.Procs = new[] { new ActiveProc { Stacks = 1, Rule = new ProcRule { Id = "k8-kill", ItemId = "k8-kill", Trigger = ProcTrigger.EnemyKilled, Action = ProcAction.HealChampion, Power = 5 } } };
                fixture.Simulation.K8CombatProjectileFixture(fixture.Source, Vector2.zero, Vector2.right * 40, 1001, cause: new CombatCause(901, fixture.Source.Id, "projectile-split", 1));
                fixture.Tick();
                Require(!enemy.Alive && !fixture.Simulation.Events.Any(value => value.Kind == CombatEventKind.Proc), "Child projectile kills cannot recursively trigger item procs");
                enemy = fixture.Enemy(new Vector2(1, 0));
                fixture.Simulation.K8CombatProjectileFixture(fixture.Source, Vector2.zero, Vector2.right * 40, 1001);
                fixture.Tick();
                Require(!enemy.Alive && fixture.Simulation.Events.Count(value => value.Kind == CombatEventKind.Proc) == 1, "Root projectile kill still triggers an eligible item proc once");
            }
            using (var fixture = new Fixture(catalog, ExpandedWeapon.None))
            {
                fixture.Catalog.RunWave = 1;
                UnitState boss = fixture.Enemy(new Vector2(0, 3));
                boss.Definition.IsBoss = true;
                boss.NextBossPattern = 0;
                UnitState ranged = fixture.Enemy(new Vector2(1, 0));
                ranged.Definition.Weapon.Kind = WeaponKind.Projectile;
                ranged.Definition.Weapon.ProjectileSpeed = 10;
                ranged.NextAttack = 0;
                ranged.PendingAttack = true;
                ranged.ImpactTime = 0;
                for (int tick = 0; tick < 30; tick++) fixture.Tick();
                Require(!fixture.Simulation.Projectiles.Any(value => value.Faction == Faction.Enemy), "Wave one blocks real enemy attack and boss pattern emitters");
                fixture.Catalog.RunWave = 2;
                ranged.Position = Vector2.right * 3;
                ranged.AttackDirection = Vector2.left;
                ranged.PendingAttack = true;
                ranged.ImpactTime = 0;
                fixture.Tick();
                Require(fixture.Simulation.Projectiles.Any(value => value.Faction == Faction.Enemy) && fixture.Simulation.Projectiles.Where(value => value.Faction == Faction.Enemy).All(value => value.Velocity.magnitude <= 2.4001f), "Wave two restores ranged fire with enemy-only speed cap");
            }
            using (var fixture = new Fixture(catalog, ExpandedWeapon.None))
            {
                fixture.Source.Definition.Weapon.Signature = SignatureWeapon.Bell;
                fixture.Source.Definition.Weapon.SignatureDelay = 0.05f;
                fixture.Source.Definition.CombatTier = 2;
                fixture.Simulation.K8CombatSignatureFixture(fixture.Source, new CombatCause(902, fixture.Source.Id, "harvest-receipt", 2));
                fixture.Tick();
                Require(fixture.Simulation.Events.Any(value => value.Kind == CombatEventKind.SignaturePulse && value.Cause.RootId == 902 && value.Cause.Depth == 3), "Signature payoff increments secondary cause depth rather than resetting it");
            }
            using (var fixture = new Fixture(catalog, ExpandedWeapon.None, boss: true))
            {
                fixture.Catalog.WaveDuration = 0.05f;
                fixture.Catalog.EnemyBudget = 2; fixture.Catalog.BossSwarmBudget = 0; // the boss's own swarm is a separate, later feature
                for (int tick = 0; tick < 20; tick++) fixture.Tick();
                Require(fixture.Simulation.Phase == RunPhase.Combat && fixture.Simulation.IsBossOvertime && fixture.Simulation.BossOvertimeSeconds > 0 && fixture.Simulation.Units.Count(value => value.Faction == Faction.Enemy) == 1, "[" + fixture.Simulation.Phase + " overtime " + fixture.Simulation.IsBossOvertime + " " + fixture.Simulation.BossOvertimeSeconds + " enemies " + fixture.Simulation.Units.Count(value => value.Faction == Faction.Enemy) + "] Boss overtime stops reinforcements without granting timer victory");
                float overtime = fixture.Simulation.BossOvertimeSeconds;
                fixture.Simulation.Pause(true);
                fixture.Tick();
                Require(fixture.Simulation.IsBossOvertime && fixture.Simulation.BossOvertimeSeconds == overtime, "Paused boss overtime remains visible and frozen for UI");
                fixture.Simulation.Pause(false);
                fixture.Simulation.DamageFixture(fixture.Simulation.Boss, 1000000);
                fixture.Tick();
                Require(fixture.Simulation.Phase == RunPhase.Victory && !fixture.Simulation.IsBossOvertime, "Boss death alone resolves overtime");
            }
            Require(original == JsonUtility.ToJson(catalog), "Focused checks leave authored catalog unchanged");
            Debug.Log("K8_COMBAT_CHECKS_PASS");
        }

        private sealed class Fixture : IDisposable
        {
            private readonly RunSession session;
            private readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();
            public readonly CheckpointCatalog Catalog;
            public readonly CombatSimulation Simulation;
            public UnitState Source => Simulation.Champion;

            public Fixture(RunCatalog catalog, ExpandedWeapon family, bool boss = false)
            {
                session = new RunSession(catalog);
                session.Start(catalog.Champions[0].Id, 8008);
                session.State.Roster.Clear();
                if (boss) session.State.Wave = 10; // Act I boss waves are 5, 10, 15, 20; 10 is an ordinary catalog body
                Catalog = session.PrepareCombat();
                Catalog.RunWave = 2;
                Catalog.EnemyBudget = 0;
                if (!boss) Catalog.Boss = null;
                Catalog.WaveDuration = 1000;
                Catalog.EnableArenaToys = Catalog.EnableBossArenaPhase = false;
                Catalog.ExtendedLoot = false;
                Catalog.PassiveHeal = 0;
                Catalog.Procs = new ActiveProc[0];
                Catalog.Champion.Passive = PassiveKind.None;
                Catalog.Champion.Weapon.Family = family;
                Catalog.Champion.Weapon.ProjectilePierce = 0;
                Simulation = new CombatSimulation(Catalog);
                Simulation.Start(8008);
                Source.NextAttack = float.PositiveInfinity;
            }

            public UnitState Enemy(Vector2 position)
            {
                UnitDefinition definition = UnityEngine.Object.Instantiate(Catalog.MeleeEnemy);
                definition.Weapon = UnityEngine.Object.Instantiate(Catalog.MeleeEnemy.Weapon);
                owned.Add(definition);
                owned.Add(definition.Weapon);
                definition.Health = 1000;
                definition.Speed = 0;
                definition.ContactDamage = 0;
                definition.Behavior = EnemyBehavior.Standard;
                UnitState unit = Simulation.AddFixture(definition, Faction.Enemy, position);
                unit.NextAttack = unit.NextContact = float.PositiveInfinity;
                return unit;
            }

            public void Tick() => Simulation.Step(0.05f, Vector2.zero);
            public void Dispose()
            {
                foreach (UnityEngine.Object asset in owned) UnityEngine.Object.DestroyImmediate(asset);
                session.Dispose();
            }
        }

        [MenuItem("Arcane Brigade/K8/Combat boss regression")]
        public static void ReproduceBossTimeout()
        {
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            string original = JsonUtility.ToJson(catalog);
            using (var session = new RunSession(catalog))
            {
                session.Start("LanternKnight", 77200, 3);
                for (int hall = 1; hall <= 8; hall++)
                {
                    CheckpointCatalog combat = session.PrepareCombat();
                    var simulation = new CombatSimulation(combat);
                    simulation.Start(combat.Seed);
                    int bossHits = 0;
                    int limit = Mathf.CeilToInt((catalog.WaveSeconds + 120) * 60);
                    for (int tick = 0; tick < limit && simulation.Phase == RunPhase.Combat; tick++)
                    {
                        simulation.Step(1f / 60, Steer(simulation));
                        bossHits += simulation.Events.Count(value => value.Kind == CombatEventKind.Hit && value.Subject == simulation.Boss);
                        if (hall == 8 && tick % 1800 == 0)
                            Debug.Log("K8_BOSS_SAMPLE seconds=" + simulation.Elapsed + " hp=" + (simulation.Boss == null ? 0 : simulation.Boss.Health) + " hits=" + bossHits + " enemies=" + simulation.Units.Count(unit => unit.Alive && unit.Faction == Faction.Enemy));
                    }
                    Debug.Log("K8_BOSS_HALL " + hall + " phase=" + simulation.Phase + " seconds=" + simulation.Elapsed + " bossHp=" + (simulation.Boss == null ? 0 : simulation.Boss.Health) + " hits=" + bossHits + " roster=" + session.State.Roster.Count);
                    if (simulation.Phase != RunPhase.Victory) throw new InvalidOperationException("K8 boss regression failed at hall " + hall);
                    session.Settle(simulation);
                    Purchase(session);
                    session.ContinueWave();
                }
            }
            Require(original == JsonUtility.ToJson(catalog), "Boss bot leaves authored catalog unchanged");
            Debug.Log("K8_BOSS_REGRESSION_PASS LanternKnight difficulty3 seed77200 hall8");
        }

        private static Vector2 Steer(CombatSimulation simulation)
        {
            Vector2 position = simulation.Champion.Position;
            Vector2 target = new Vector2(Mathf.Cos(simulation.Elapsed * 0.27f) * 8, Mathf.Sin(simulation.Elapsed * 0.27f) * 5);
            Vector2 direction = target - position;
            foreach (UnitState unit in simulation.Units)
            {
                if (unit.Faction != Faction.Enemy || !unit.Alive) continue;
                Vector2 offset = position - unit.Position;
                float distance = offset.magnitude;
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

        private static void Require(bool condition, string caption)
        {
            if (!condition) throw new InvalidOperationException("K8 combat: " + caption);
            Debug.Log("K8_COMBAT_PASS " + caption);
        }
    }
}
