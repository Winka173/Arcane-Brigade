using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K23CooperationChecks
    {
        private static void Require(bool condition, string label)
        {
            if (!condition) throw new InvalidOperationException("K23 cooperation: " + label);
        }

        private static void Invoke(CombatSimulation simulation, string method, params object[] arguments)
        {
            typeof(CombatSimulation).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(simulation, arguments);
        }

        [MenuItem("Arcane Brigade/K23/Check allied cooperation")]
        public static void Run()
        {
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            foreach (string fan in new[] { "PaperFan", "MoonFeatherFan" }) CheckSpread(catalog, fan, "LanternStaff", "cooperation-fanned-fire");
            foreach (string hammer in new[] { "StampHammer", "SpringKnocker" })
                foreach (string orb in new[] { "DreamOrbStaff", "MoonseedOrb" }) CheckSpread(catalog, hammer, orb, "cooperation-shattered-orb");
            CheckCollection(catalog);
            CheckRejectedIngredients(catalog);
            CheckExpiredCollection(catalog);
            Debug.Log("K23 cooperation passed: conserved allied spreads, bounded fragments, real pull/sweep, attribution, pacing, pause and rest.");
        }

        private static void CheckSpread(RunCatalog catalog, string helperWeapon, string ingredientWeapon, string effect)
        {
            string firstSnapshot = null;
            for (int replay = 0; replay < 2; replay++)
            {
                using var fixture = new Fixture(catalog);
                CombatSimulation simulation = fixture.Simulation;
                UnitState helper = fixture.Unit(helperWeapon, Faction.Companion, Vector2.zero);
                UnitState owner = fixture.Unit(ingredientWeapon, Faction.Companion, Vector2.left);
                UnitState enemy = fixture.Unit("LanternStaff", Faction.Enemy, new Vector2(2, 0));
                ProjectileState shot = simulation.K8CombatProjectileFixture(owner, new Vector2(0.8f, 0), Vector2.right * 4, 30, 4, true, new CombatCause(700, owner.Id));
                shot.Sprite = owner.Definition.Weapon.ProjectileSprite;
                ProjectileState hostile = simulation.K8CombatProjectileFixture(enemy, Vector2.right, Vector2.left, 10, cause: new CombatCause(701, enemy.Id));
                ProjectileState self = simulation.K8CombatProjectileFixture(helper, Vector2.right, Vector2.right, 10, cause: new CombatCause(702, helper.Id));
                self.Weapon = owner.Definition.Weapon; self.Element = owner.Definition.Weapon.Element;
                ProjectileState recursive = simulation.K8CombatProjectileFixture(owner, Vector2.right, Vector2.right, 10, cause: new CombatCause(703, owner.Id, "child", 1));
                Invoke(simulation, "ResolveCooperationAttack", helper, 20f, new CombatCause(704, helper.Id, "child", 1));
                Invoke(simulation, "ResolveCooperationAttack", enemy, 20f, new CombatCause(705, enemy.Id));
                Require(simulation.CooperationPayoffs == 0, "enemy and secondary attacks cannot cooperate");
                simulation.Pause(true);
                Invoke(simulation, "ResolveCooperationAttack", helper, 20f, new CombatCause(706, helper.Id));
                Require(simulation.CooperationPayoffs == 0, "paused attack cannot cooperate"); simulation.Pause(false);
                helper.PendingAttack = true; helper.ImpactTime = 0;
                simulation.Step(0.05f, Vector2.zero);
                ProjectileState[] fragments = simulation.Projectiles.Where(projectile => projectile.Cause.ItemId == effect).ToArray();
                Require(simulation.CooperationPayoffs == 1 && fragments.Length == 3 && !simulation.Projectiles.Contains(shot), "real attack replaces one allied ingredient with exactly three fragments: " + helperWeapon);
                Require(simulation.Projectiles.Contains(hostile) && simulation.Projectiles.Contains(self) && simulation.Projectiles.Contains(recursive), "hostile, self and secondary ingredients untouched " + helperWeapon + " hostile=" + simulation.Projectiles.Contains(hostile) + " self=" + simulation.Projectiles.Contains(self) + " child=" + simulation.Projectiles.Contains(recursive));
                Require(Mathf.Abs(fragments.Sum(fragment => fragment.Damage) - 30) < 0.001f, "conserved direct damage budget");
                Require(fragments.All(fragment => fragment.SourceUnitId == owner.Id && fragment.Cause.RootId == 700 && fragment.Cause.Depth == 1 && fragment.IsFragment && fragment.Weapon == null && !fragment.SplitOnImpact && fragment.RicochetsRemaining == 0 && fragment.RandomBounces == 0 && fragment.Remaining <= 1.1f), "owner/root attribution, finite lifetime, no weapon payoffs or recursive splitting");
                string snapshot = string.Join("|", fragments.Select(fragment => fragment.Id + ":" + fragment.Position + ":" + fragment.Velocity + ":" + fragment.Damage));
                if (firstSnapshot == null) firstSnapshot = snapshot;
                else Require(firstSnapshot == snapshot, "identical seeds produce identical spread");
                ProjectileState second = simulation.K8CombatProjectileFixture(owner, new Vector2(0.8f, 0), Vector2.right * 4, 30);
                Invoke(simulation, "ResolveCooperationAttack", helper, 20f, new CombatCause(707, helper.Id));
                Require(simulation.CooperationPayoffs == 1 && simulation.Projectiles.Contains(second), "repeated attacks respect cooldown");
                float clock = simulation.Elapsed;
                simulation.Pause(true); simulation.Step(0.1f, Vector2.zero);
                Require(simulation.Elapsed == clock && fragments.All(fragment => simulation.Projectiles.Contains(fragment)), "pause freezes fragment lifetime"); simulation.Pause(false);
                for (int tick = 0; tick < 25; tick++) simulation.Step(0.05f, Vector2.zero);
                ContributionEntry damage = simulation.Contributions.FirstOrDefault(entry => entry.EffectId == effect && entry.UnitId == owner.Definition.Id);
                Require(damage != null && damage.Damage > 0 && damage.Damage <= 30.001f, "real collision damage credited only to ingredient owner");
                Require(!simulation.Projectiles.Any(projectile => projectile.Cause.ItemId == effect), "fragments expire or hit without descendants");
                Require(simulation.Contributions.Any(entry => entry.UnitId == helper.Definition.Id && entry.EffectId == effect + "-assist" && entry.Activations == 1 && entry.Damage == 0), "helper gets assist, not duplicate damage");
                simulation.EnterRest();
                Require(simulation.CooperationPayoffs == 0 && simulation.Projectiles.Count == 0, "rest clears recipe state and projectiles");
            }
        }

        private static void CheckCollection(RunCatalog catalog)
        {
            using var fixture = new Fixture(catalog);
            CombatSimulation simulation = fixture.Simulation;
            UnitState collector = fixture.Unit("ScrapMagnet", Faction.Companion, Vector2.zero);
            UnitState melee = fixture.Unit("PaperFan", Faction.Companion, Vector2.zero);
            var targets = new List<UnitState>();
            for (int index = 0; index < 5; index++) targets.Add(fixture.Unit("LanternStaff", Faction.Enemy, new Vector2(2.3f, index * 0.04f)));
            Invoke(simulation, "ResolveExpandedAttack", collector, 1f, false, new CombatCause(800, collector.Id)); Land(simulation);
            Require(targets.Take(4).All(target => target.Position.x < 2.3f), "magnet actually collects enemies before sweep");
            float[] health = targets.Select(target => target.Health).ToArray();
            Invoke(simulation, "ResolveCooperationAttack", collector, 20f, new CombatCause(801, collector.Id));
            Invoke(simulation, "ResolveCooperationAttack", melee, 20f, new CombatCause(802, melee.Id));
            Require(simulation.CooperationPayoffs == 1 && targets.Count(target => target.Health < health[targets.IndexOf(target)]) == 3, "different allied melee sweeps at most three collected enemies");
            Require(Mathf.Abs(targets.Sum(target => health[targets.IndexOf(target)] - target.Health) - 15) < 0.001f, "sweep has capped quarter-hit budget");
            Invoke(simulation, "ResolveCooperationAttack", melee, 20f, new CombatCause(803, melee.Id));
            Require(simulation.CooperationPayoffs == 1, "collected setup consumed once");
            Require(simulation.Contributions.Any(entry => entry.EffectId == "cooperation-collect-assist" && entry.UnitId == collector.Definition.Id && entry.Damage == 0), "collector receives assist only");
            simulation.EnterRest();
            Require(simulation.CooperationPayoffs == 0, "rest clears collect state");
            simulation.Start(23006);
            Require(simulation.CooperationPayoffs == 0, "restart clears all cooperation cooldowns");
        }

        // the magnet fires a bolt now: step until it has struck (or expired) so its pull has happened
        private static void Land(CombatSimulation simulation)
        {
            for (int tick = 0; tick < 20 && simulation.Projectiles.Count > 0; tick++) simulation.Step(0.05f, Vector2.zero);
        }

        private static void CheckRejectedIngredients(RunCatalog catalog)
        {
            for (int scenario = 0; scenario < 5; scenario++)
            {
                using var fixture = new Fixture(catalog);
                CombatSimulation simulation = fixture.Simulation;
                UnitState helper = fixture.Unit("PaperFan", Faction.Companion, Vector2.zero);
                UnitState owner = scenario == 1 ? helper : fixture.Unit("LanternStaff", scenario == 0 ? Faction.Enemy : Faction.Companion, Vector2.left);
                ProjectileState shot = simulation.K8CombatProjectileFixture(owner, Vector2.right, Vector2.right * 4, 30, cause: new CombatCause(900, owner.Id, "", scenario == 2 ? 1 : 0));
                shot.Element = DamageElement.Ember;
                shot.IsFragment = scenario == 3;
                if (scenario == 4) owner.Alive = false;
                Invoke(simulation, "ResolveCooperationAttack", helper, 20f, new CombatCause(901, helper.Id));
                Require(simulation.CooperationPayoffs == 0 && simulation.Projectiles.Contains(shot), "reject hostile/self/secondary/fragment/dead-owner ingredient " + scenario);
            }
        }

        private static void CheckExpiredCollection(RunCatalog catalog)
        {
            using var fixture = new Fixture(catalog);
            CombatSimulation simulation = fixture.Simulation;
            UnitState collector = fixture.Unit("ScrapMagnet", Faction.Companion, Vector2.zero);
            UnitState melee = fixture.Unit("PaperFan", Faction.Companion, Vector2.zero);
            UnitState target = fixture.Unit("LanternStaff", Faction.Enemy, new Vector2(2.3f, 0));
            target.Definition.KnockbackResistance = 1;
            Invoke(simulation, "ResolveExpandedAttack", collector, 1f, false, new CombatCause(910, collector.Id)); Land(simulation);
            Invoke(simulation, "ResolveCooperationAttack", melee, 20f, new CombatCause(911, melee.Id));
            Require(simulation.CooperationPayoffs == 0 && target.Position.x == 2.3f, "fully resisted pulls cannot prime sweep");
            target.Definition.KnockbackResistance = 0; target.Definition.IsBoss = true;
            Invoke(simulation, "ResolveExpandedAttack", collector, 1f, false, new CombatCause(912, collector.Id)); Land(simulation);
            Invoke(simulation, "ResolveCooperationAttack", melee, 20f, new CombatCause(913, melee.Id));
            Require(simulation.CooperationPayoffs == 0 && target.Position.x == 2.3f, "bosses cannot prime sweep");
            target.Definition.IsBoss = false;
            Invoke(simulation, "ResolveExpandedAttack", collector, 1f, false, new CombatCause(914, collector.Id)); Land(simulation);
            float health = target.Health;
            simulation.Pause(true); simulation.Step(0.1f, Vector2.zero); simulation.Pause(false);
            Require(target.Health == health, "paused setup does not deal damage");
            for (int tick = 0; tick < 21; tick++) simulation.Step(0.05f, Vector2.zero);
            Invoke(simulation, "ResolveCooperationAttack", melee, 20f, new CombatCause(915, melee.Id));
            Require(simulation.CooperationPayoffs == 0 && target.Health == health, "expired setup cannot sweep");
            simulation.EnterRest();
            Require(typeof(CombatSimulation).GetField("cooperationCollector", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(simulation) == null, "rest discards pending collector reference");
        }

        private sealed class Fixture : IDisposable
        {
            private readonly RunSession session;
            private readonly RunCatalog catalog;
            private readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();
            public readonly CombatSimulation Simulation;

            public Fixture(RunCatalog catalog)
            {
                this.catalog = catalog;
                session = new RunSession(catalog); session.Start("LanternKnight", 23006); session.State.Roster.Clear(); session.State.Wave = 2;
                CheckpointCatalog arena = session.PrepareCombat(); arena.Boss = null; arena.EnemyBudget = 0;
                arena.EnableArenaToys = arena.EnableBossArenaPhase = arena.EnableSpectacles = arena.EnableHeroMechanics = false;
                arena.StoryCompanions = new UnitDefinition[0]; // isolate the cooperation pair from Hob's own attacks
                arena.CompanionLeash = 20;
                Simulation = new CombatSimulation(arena); Simulation.Start(23006);
                Simulation.Champion.NextAttack = float.PositiveInfinity;
                Simulation.Champion.Position = Simulation.Champion.PreviousPosition = new Vector2(-6, 0);
            }

            public UnitState Unit(string weaponId, Faction faction, Vector2 position)
            {
                UnitDefinition definition = UnityEngine.Object.Instantiate(catalog.Companions[0]); owned.Add(definition);
                definition.Id = weaponId + "-fixture-" + owned.Count;
                definition.Weapon = UnityEngine.Object.Instantiate(catalog.Weapon(weaponId)); owned.Add(definition.Weapon);
                definition.Weapon.PushDistance = 0;
                definition.Speed = definition.CollisionDamage = definition.CriticalChance = definition.KnockbackResistance = 0;
                definition.Health = 10000; definition.Radius = 0.25f; definition.IsBoss = false;
                definition.Passive = PassiveKind.None; definition.Mutation = EnemyMutation.None; definition.Behavior = EnemyBehavior.Standard;
                if (faction == Faction.Enemy) definition.Weapon.Kind = WeaponKind.Contact;
                UnitState unit = Simulation.AddFixture(definition, faction, position);
                unit.NextAttack = unit.NextContact = unit.NextBehavior = float.PositiveInfinity;
                unit.AttackDirection = Vector2.right;
                return unit;
            }

            public void Dispose()
            {
                session.Dispose();
                foreach (UnityEngine.Object asset in owned) UnityEngine.Object.DestroyImmediate(asset);
            }
        }
    }
}
