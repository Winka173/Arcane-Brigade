#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K4ArenaChecks
    {
        [MenuItem("Arcane Brigade/K4/Check arena toys")]
        public static void Run()
        {
            int passed = 0;
            Action<bool, string> require = (condition, name) =>
            {
                if (!condition) throw new InvalidOperationException("K4 arena: " + name);
                passed++;
                Debug.Log("PASS K4 arena: " + name);
            };
            using (var fixture = new Fixture())
            {
                CombatSimulation simulation = fixture.Simulation;
                require(simulation.ArenaToys.Count == 2, "seed creates bell and cart");
                Vector2[] layout = simulation.ArenaToys.Select(toy => toy.Position).ToArray();
                simulation.ResetArenaToys(2);
                require(layout.SequenceEqual(simulation.ArenaToys.Select(toy => toy.Position)), "seed and wave reproduce layout");
                simulation.ResetArenaToys(3);
                require(!layout.SequenceEqual(simulation.ArenaToys.Select(toy => toy.Position)), "wave changes layout");
                simulation.ResetArenaToys(2);
                require(simulation.ArenaToys.All(toy => Mathf.Abs(toy.Position.x) <= 9.9f && Mathf.Abs(toy.Position.y) <= 4.4f && toy.Position.magnitude >= 2.25f), "placement has wall and champion clearance");
                ArenaToyState bell = simulation.ArenaToys[0];
                fixture.Impact(bell, 1);
                fixture.Impact(bell, 1);
                require(bell.Charge == 1, "duplicate root cannot charge twice");
                simulation.NotifyArenaToyHit(fixture.Source, bell.Position, bell.Position, 0f, Vector2.right, new CombatCause(2, fixture.Source.Id, "echo", 1));
                simulation.NotifyArenaToyHit(simulation.Boss, bell.Position, bell.Position, 0f, Vector2.right, new CombatCause(3, simulation.Boss.Id));
                simulation.NotifyArenaToyHit(fixture.Source, bell.Position, bell.Position, float.NaN, Vector2.right, new CombatCause(4, fixture.Source.Id));
                require(bell.Charge == 1, "secondary, hostile and invalid impacts are rejected");
                UnitState enemy = fixture.Enemy(bell.Position);
                fixture.Impact(bell, 5);
                fixture.Impact(bell, 6);
                require(bell.Mode == ArenaToyMode.Warning && enemy.Health == 100f && bell.Until - simulation.Elapsed >= 1f, "full charge warns before damage");
                simulation.Champion.Position = bell.Position;
                fixture.Source.Position = bell.Position;
                float championHealth = simulation.Champion.Health;
                simulation.Pause(true);
                fixture.Tick(30);
                fixture.Impact(bell, 7);
                require(simulation.Elapsed == 0f && enemy.Health == 100f, "pause freezes charge, warning and damage");
                simulation.Pause(false);
                fixture.Tick(11);
                require(enemy.Health == 100f, "warning cannot fire before a second");
                fixture.Tick(3);
                require(enemy.Health == 86f && simulation.Champion.Health == championHealth && float.IsPositiveInfinity(fixture.Source.Health), "bell only damages enemies");
                require(simulation.Events.Any(value => value.Kind == CombatEventKind.Hit && value.Cause.Depth == 1 && value.Cause.SourceUnitId == 0 && value.Cause.ItemId == "arena-stage-bell"), "payoff carries secondary arena ledger cause");
                fixture.Impact(bell, 8);
                require(bell.Charge == 0 && bell.Mode == ArenaToyMode.Cooldown, "cooldown rejects impact");
                fixture.Tick(52);
                require(bell.Mode == ArenaToyMode.Charging && enemy.Health == 86f && simulation.ArenaToyActivations == 1, "finite cooldown and single payoff");
                fixture.Impact(bell, 1);
                require(bell.Charge == 0, "root dedup survives cooldown");
                simulation.NotifyArenaToyHit(new CombatEvent(CombatEventKind.Hit, bell.Position, Faction.Enemy, 1f, Vector2.right, cause: new CombatCause(9, fixture.Source.Id)));
                require(bell.Charge == 1, "event overload resolves eligible allied source");
                simulation.Champion.Position = Vector2.zero;
                fixture.Source.Position = new Vector2(1f, 0f);
                simulation.ResetArenaToys(2);
                ArenaToyState cart = simulation.ArenaToys[1];
                fixture.Impact(cart, 20);
                fixture.Impact(cart, 21);
                UnitState laneEnemy = fixture.Enemy(Vector2.Lerp(cart.LaneStart, cart.LaneEnd, 0.5f));
                UnitState outside = fixture.Enemy(cart.LaneStart + Vector2.up * 2f);
                fixture.Tick(24);
                require(laneEnemy.Health == 82f && outside.Health == 100f && cart.Mode == ArenaToyMode.Cooldown, "cart sweeps lane once and stops");
                require(Mathf.Abs(cart.Position.x) <= 10.35f && Mathf.Abs(cart.Position.y) <= 4.85f, "cart endpoint remains bounded");
                simulation.ResetArenaToys(2);
                bell = simulation.ArenaToys[0];
                simulation.Boss.Position = bell.Position;
                fixture.Impact(bell, 30); fixture.Impact(bell, 31); fixture.Impact(bell, 32);
                fixture.Tick(14);
                require(simulation.Boss.Health >= 92f, "boss resistance prevents prop one-hit");
                simulation.Boss.Position = new Vector2(0f, 3.85f);
                simulation.Boss.Health = 50f;
                fixture.Catalog.EnableBossArenaPhase = false;
                fixture.Tick(1);
                require(!simulation.ArenaShiftWarning && !simulation.ArenaShiftComplete, "boss phase has independent gate");
                fixture.Catalog.EnableBossArenaPhase = true;
                fixture.Tick(1);
                Vector2[] beforeShift = simulation.ArenaToys.Select(toy => toy.Position).ToArray();
                require(simulation.ArenaShiftWarning && simulation.ArenaShiftUntil - simulation.Elapsed >= 1f, "half HP starts one warned phase");
                fixture.Tick(10);
                require(beforeShift.SequenceEqual(simulation.ArenaToys.Select(toy => toy.Position)), "layout does not jump early");
                fixture.Tick(4);
                require(simulation.ArenaShiftComplete && !simulation.ArenaShiftWarning && simulation.Champion.Health == championHealth, "shift completes without champion harm");
                Vector2[] shifted = simulation.ArenaToys.Select(toy => toy.Position).ToArray();
                require(!beforeShift.SequenceEqual(shifted), "boss phase actually shifts available layout");
                simulation.Boss.Health = 40f;
                fixture.Tick(20);
                require(shifted.SequenceEqual(simulation.ArenaToys.Select(toy => toy.Position)) && !simulation.ArenaShiftWarning, "boss phase occurs only once");
                fixture.Catalog.EnableArenaToys = false;
                simulation.ResetArenaToys(2);
                require(simulation.ArenaToys.Count == 0 && !simulation.ArenaShiftComplete && simulation.ArenaToyActivations == 0, "gate and reset clear combat state");
                fixture.Catalog.EnableArenaToys = true;
                fixture.Catalog.ArenaSize = Vector2.one;
                simulation.ResetArenaToys(2);
                require(simulation.ArenaToys.Count == 0, "unsafe tiny arena safely omits props");
                simulation.ClearArenaToys();
                require(simulation.ArenaToys.Count == 0, "boundary clear is idempotent");
            }
            using (var fixture = new Fixture())
            {
                fixture.Simulation.Boss.Health = 50f;
                fixture.Tick(1);
                ArenaToyState toy = fixture.Simulation.ArenaToys[0];
                Vector2 original = toy.Position;
                fixture.Simulation.Champion.Position = toy.ShiftDestination;
                fixture.Tick(14);
                require(toy.Position == original, "occupied preview is cancelled rather than overlapping champion");
                fixture.Simulation.ResetArenaToys(2);
                fixture.Tick(1);
                fixture.Simulation.Boss.Alive = false;
                fixture.Tick(1);
                require(!fixture.Simulation.ArenaShiftWarning, "boss death cancels pending phase");
            }
            CheckStepIntegration(require);
            Debug.Log("K4 arena checks passed: " + passed);
        }

        private static void CheckStepIntegration(Action<bool, string> require)
        {
            using (var fixture = new Fixture(1))
            {
                require(fixture.Simulation.ArenaToys.Count == 1 && fixture.Simulation.ArenaToys[0].Kind == ArenaToyKind.StageBell, "actual Start introduces only stage bell on wave one");
                fixture.Catalog.RunWave = 2;
                fixture.Simulation.Start(fixture.Catalog.Seed);
                require(fixture.Simulation.ArenaToys.Count == 2, "actual Start reads RunWave for wave-two cart");
                fixture.Simulation.EnterRest();
                require(fixture.Simulation.ArenaToys.Count == 0, "actual EnterRest clears props");
            }
            using (var fixture = new Fixture())
            {
                CombatSimulation simulation = fixture.Simulation;
                ArenaToyState bell = simulation.ArenaToys[0];
                Vector2 heading = fixture.PrepareStepAttack(bell, WeaponKind.Melee);
                UnitState first = fixture.Enemy(bell.Position);
                UnitState second = fixture.Enemy(bell.Position + new Vector2(-heading.y, heading.x) * 0.1f);
                fixture.Step(1);
                require(fixture.Source.PendingAttack && bell.Charge == 0 && first.Health == 100f, "actual melee windup does not charge prop");
                fixture.Step(1);
                CombatEvent[] hits = simulation.Events.Where(value => value.Kind == CombatEventKind.Hit && value.Cause.SourceUnitId == fixture.Source.Id).ToArray();
                CombatEvent fire = simulation.Events.First(value => value.Kind == CombatEventKind.Fire && value.Cause.SourceUnitId == fixture.Source.Id);
                require(hits.Length == 2 && hits.All(value => value.Cause.RootId == fire.Cause.RootId && value.Cause.Depth == 0) && bell.Charge == 1, "actual multi-target melee shares root and charges once");
                for (int attack = 0; attack < 2; attack++)
                {
                    fixture.Source.NextAttack = simulation.Elapsed;
                    fixture.Step(2);
                }
                require(bell.Mode == ArenaToyMode.Warning && first.Health == 97f && second.Health == 97f, "distinct actual melee attacks reach warned charge");
                float pausedAt = simulation.Elapsed;
                simulation.Pause(true);
                fixture.Step(20);
                require(simulation.Elapsed == pausedAt && first.Health == 97f, "actual Step pause freezes prop warning");
                simulation.Pause(false);
                fixture.Step(13);
                ContributionEntry arena = simulation.Contributions.Single(value => value.UnitId == "ARENA" && value.EffectId == "arena-stage-bell");
                require(first.Health == 83f && second.Health == 83f && bell.Mode == ArenaToyMode.Cooldown && arena.Damage == 28f && arena.Activations == 1, "actual Step resolves once and ledger records effective arena damage and activation");
                require(simulation.ArenaToys[1].Charge == 0, "secondary payoff callbacks do not charge cart");
            }
            using (var fixture = new Fixture())
            {
                CombatSimulation simulation = fixture.Simulation;
                ArenaToyState bell = simulation.ArenaToys[0];
                Vector2 heading = fixture.PrepareStepAttack(bell, WeaponKind.Projectile);
                UnitState target = fixture.Enemy(bell.Position + heading);
                fixture.Step(1);
                require(bell.Charge == 0 && simulation.Projectiles.Count == 0, "actual projectile windup cannot charge prop");
                fixture.Step(1);
                ProjectileState projectile = simulation.Projectiles.Single();
                int root = projectile.Cause.RootId;
                require(root != 0 && projectile.Cause.SourceUnitId == fixture.Source.Id && projectile.Cause.Depth == 0 && bell.Charge == 1, "actual emission retains allied Fire root on swept projectile");
                require(simulation.Events.Any(value => value.Kind == CombatEventKind.Fire && value.Cause.RootId == root), "projectile cause matches its attack Fire event");
                simulation.Pause(true);
                Vector2 stoppedAt = projectile.Position;
                fixture.Step(4);
                require(projectile.Position == stoppedAt && bell.Charge == 1, "actual paused projectile neither travels nor recharges");
                simulation.Pause(false);
                fixture.Step(1);
                require(projectile.Cause.RootId == root && bell.Charge == 1, "overlapping consecutive flight segments retain and deduplicate root");
                fixture.Step(1);
                require(target.Health == 99f && simulation.Projectiles.Count == 0 && bell.Charge == 1 && simulation.Events.Any(value => value.Kind == CombatEventKind.Hit && value.SubjectId == target.Id && value.Cause.RootId == root), "projectile swept and enemy-hit callbacks share one root and one charge");
            }
            using (var fixture = new Fixture())
            {
                CombatSimulation simulation = fixture.Simulation;
                ArenaToyState bell = simulation.ArenaToys[0];
                Vector2 heading = fixture.PrepareStepAttack(bell, WeaponKind.Projectile);
                UnitState blocker = fixture.Enemy(fixture.Source.Position + heading * 0.6f);
                fixture.Source.Definition.Weapon.ProjectileSpeed = 60f;
                fixture.Step(2);
                require(blocker.Health == 99f && bell.Charge == 0 && simulation.Projectiles.Count == 0, "actual fast sweep truncates at blocking hit instead of charging prop behind it");
            }
            using (var fixture = new Fixture())
            {
                CombatSimulation simulation = fixture.Simulation;
                ArenaToyState bell = simulation.ArenaToys[0];
                Vector2 heading = fixture.PrepareStepAttack(bell, WeaponKind.Projectile);
                fixture.Source.Definition.Weapon.ProjectileCount = 3;
                fixture.Enemy(bell.Position + heading);
                fixture.Step(2);
                require(simulation.Projectiles.Count == 3 && simulation.Projectiles.Select(value => value.Cause.RootId).Distinct().Count() == 1 && bell.Charge == 1, "actual sibling projectiles share attack root without multiplying charge");
            }
            using (var fixture = new Fixture())
            {
                CombatSimulation simulation = fixture.Simulation;
                simulation.Boss.Health = 50f;
                fixture.Step(1);
                require(simulation.ArenaShiftWarning && simulation.ArenaShiftUntil - simulation.Elapsed >= 1f, "actual Step detects half-health boss phase");
                simulation.Pause(true);
                fixture.Step(20);
                require(simulation.Elapsed == 0.1f && simulation.ArenaShiftWarning, "actual Step pause freezes boss phase warning");
                simulation.Pause(false);
                fixture.Step(13);
                require(simulation.ArenaShiftComplete && !simulation.ArenaShiftWarning && simulation.Champion.Health == 100f, "actual Step completes warned phase without champion harm");
            }
        }

        private sealed class Fixture : IDisposable
        {
            private readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();
            private readonly UnitDefinition enemyDefinition;
            public readonly CheckpointCatalog Catalog;
            public readonly CombatSimulation Simulation;
            public readonly UnitState Source;

            public Fixture(int wave = 2)
            {
                Catalog = Make<CheckpointCatalog>();
                WeaponDefinition weapon = Make<WeaponDefinition>();
                weapon.Kind = WeaponKind.Contact;
                Catalog.Champion = Unit(weapon);
                Catalog.Companion = Unit(weapon);
                Catalog.Boss = Unit(weapon);
                Catalog.Boss.IsBoss = true;
                enemyDefinition = Unit(weapon);
                Catalog.MeleeEnemy = Catalog.RangedEnemy = enemyDefinition;
                Catalog.EnableArenaToys = true;
                Catalog.EnableBossArenaPhase = true;
                Catalog.Seed = 131044;
                Catalog.RunWave = wave;
                Catalog.EnemyBudget = 0;
                Simulation = new CombatSimulation(Catalog);
                Simulation.Start(Catalog.Seed);
                Source = Simulation.Units.First(unit => unit.Faction == Faction.Companion);
            }

            private T Make<T>() where T : ScriptableObject
            {
                T value = ScriptableObject.CreateInstance<T>();
                owned.Add(value);
                return value;
            }

            private UnitDefinition Unit(WeaponDefinition weapon)
            {
                UnitDefinition value = Make<UnitDefinition>();
                value.Weapon = weapon;
                value.Health = 100f;
                value.Speed = 0f;
                value.ContactDamage = 0f;
                return value;
            }

            public UnitState Enemy(Vector2 position) => Simulation.AddFixture(enemyDefinition, Faction.Enemy, position);
            public Vector2 PrepareStepAttack(ArenaToyState toy, WeaponKind kind)
            {
                WeaponDefinition weapon = Make<WeaponDefinition>();
                weapon.Id = "ArenaStepFixture";
                weapon.Kind = kind;
                weapon.Damage = 1f;
                weapon.Range = 4f;
                weapon.Windup = 0.1f;
                weapon.Cooldown = 1000f;
                weapon.ProjectileSpeed = 10f;
                weapon.ProjectileLifetime = 10f;
                weapon.MountDistance = weapon.MuzzleDistance = weapon.RecoilDistance = 0f;
                Source.Definition.Weapon = weapon;
                Source.Definition.Id = "ArenaStepAlly";
                Vector2 heading = toy.Position.x >= 0f ? Vector2.right : Vector2.left;
                Source.Position = toy.Position - heading * 1.4f;
                Simulation.Champion.Position = Source.Position;
                Simulation.Boss.Position = new Vector2(-heading.x * 9f, -4f);
                Simulation.Boss.NextBossPattern = float.PositiveInfinity;
                Catalog.WaveDuration = 1000f;
                Catalog.EnableBossArenaPhase = false;
                return heading;
            }
            public void Impact(ArenaToyState toy, int root) => Simulation.NotifyArenaToyHit(Source, toy.Position, toy.Position, 0f, Vector2.right, new CombatCause(root, Source.Id));
            public void Tick(int count) { for (int tick = 0; tick < count; tick++) Simulation.ArenaToyTickFixture(0.1f); }
            public void Step(int count) { for (int tick = 0; tick < count; tick++) Simulation.Step(0.1f, Vector2.zero); }
            public void Dispose() { foreach (UnityEngine.Object value in owned) UnityEngine.Object.DestroyImmediate(value); }
        }
    }
}
#endif
