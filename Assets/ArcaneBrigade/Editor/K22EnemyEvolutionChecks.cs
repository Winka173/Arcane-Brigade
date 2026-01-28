using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K22EnemyEvolutionChecks
    {
        public static void Run()
        {
            var notes = new List<string>();
            void Require(bool condition, string label) { if (!condition) throw new InvalidOperationException("K22 " + label); notes.Add("PASS " + label); }
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            EnemyEvolutionLibrary library = EnemyEvolutionLibrary.Current;
            Require(library != null && library.Species.Length == catalog.Enemies.Count(enemy => enemy.SpecialEnemy == SpecialEnemyKind.None) && library.Species.Length == 20, "Every original ordinary species has a normal and mutated entry");
            Require(library.Species.Select(entry => entry.Ability).Distinct().Count() == 20 && library.Species.All(entry => entry.Ability != EnemyMutation.None), "Twenty independently authored mutation movesets");
            Require(library.BodyFragments.Length == 16 && library.BodyFragments.All(sprite => sprite != null) && library.BodyFragments.Distinct().Count() == 16, "Sixteen original body-fragment projectile drawings");
            var artwork = new HashSet<Sprite>();
            foreach (EnemyEvolutionEntry entry in library.Species)
            {
                UnitDefinition original = catalog.Enemies.First(enemy => enemy.Id == entry.Id);
                Require(entry.Normal != null && entry.Mutated != null && entry.Normal != entry.Mutated && original.AuthoredMotion == entry.Normal, "Catalog uses redrawn normal form " + entry.Id);
                foreach (AuthoredMotionSet motion in new[] { entry.Normal, entry.Mutated })
                {
                    Sprite[] poses = motion.Idle.Concat(motion.Walk).Concat(motion.Windup).Concat(motion.Release).Concat(motion.Recovery).Distinct().ToArray();
                    Require(poses.Length == 8 && poses.All(sprite => sprite != null && sprite.pivot.y == 0 && AssetDatabase.GetAssetPath(sprite).Contains("/K22/")), "Eight ground-anchored drawings for form " + motion.name);
                    foreach (Sprite sprite in poses) Require(artwork.Add(sprite), "Drawing not reused by another form: " + sprite.name);
                }
                int onset = EnemyEvolutionLibrary.MutationWave(entry.Id);
                Require(onset >= 6 && onset <= 12, "Mutation starts after the early wave difficulty ramp " + entry.Id);
                UnitDefinition normal = UnityEngine.Object.Instantiate(original);
                UnitDefinition mutated = UnityEngine.Object.Instantiate(original);
                WeaponDefinition weapon = UnityEngine.Object.Instantiate(original.Weapon); mutated.Weapon = weapon;
                float health = original.Health, speed = original.Speed, height = original.BodyHeight;
                EnemyEvolutionLibrary.Apply(normal, onset - 1); EnemyEvolutionLibrary.Apply(mutated, onset);
                Require(normal.EvolutionStage == 0 && mutated.EvolutionStage == 1 && mutated.Mutation == entry.Ability, "Deterministic stage boundary " + entry.Id);
                Require(mutated.AuthoredMotion == entry.Mutated && normal.Body != mutated.Body, "Different real anatomy, not tint " + entry.Id);
                Require(mutated.Health == health && mutated.Speed == speed && mutated.BodyHeight <= height * 1.121f, "No extra HP or speed cliff from evolution " + entry.Id);
                Require(original.Health == health && original.BodyHeight == height && original.EvolutionStage == 0 && original.Mutation == EnemyMutation.None, "Catalog remains immutable " + entry.Id);
                using (var session = new RunSession(catalog))
                {
                    session.Start("LanternKnight", 22077); session.State.Roster.Clear(); session.State.Wave = 13;
                    CheckpointCatalog arena = session.PrepareCombat(); arena.Boss = null; arena.EnemyBudget = 0; arena.EnableArenaToys = arena.EnableBossArenaPhase = arena.EnableSpectacles = false;
                    var simulation = new CombatSimulation(arena); simulation.Start(22077);
                    simulation.Champion.Position = simulation.Champion.PreviousPosition = new Vector2(3, 0); simulation.Champion.NextAttack = float.PositiveInfinity;
                    UnitState enemy = simulation.AddFixture(mutated, Faction.Enemy, Vector2.zero);
                    enemy.NextAttack = enemy.NextContact = enemy.NextBehavior = float.PositiveInfinity; enemy.Awakened = true; enemy.NextEvolutionAttack = 0;
                    if (entry.Ability == EnemyMutation.CoinBarrage) simulation.LootFixture(LootKind.Coin, Vector2.zero, 7);
                    simulation.Step(0.05f, Vector2.zero);
                    Require(enemy.EvolutionReleaseAt > simulation.Elapsed && enemy.EvolutionCasts == 0 && simulation.ExpandedFields.Count == 0 && simulation.Projectiles.Count == 0, "Body windup precedes real damage " + entry.Id);
                    Require(enemy.Definition.AuthoredMotion.Select(enemy, simulation.Elapsed + 0.2f, false) == entry.Mutated.Windup[0], "Visible anticipation drawing " + entry.Id);
                    float clock = simulation.Elapsed; simulation.Pause(true); simulation.Step(0.05f, Vector2.zero);
                    Require(clock == simulation.Elapsed && enemy.EvolutionCasts == 0, "Pause freezes mutation " + entry.Id); simulation.Pause(false);
                    for (int tick = 0; tick < 18; tick++) simulation.Step(0.05f, Vector2.zero);
                    Require(enemy.EvolutionCasts == 1 && enemy.NextEvolutionAttack > simulation.Elapsed + 5, "One real mutation cast with paced cooldown " + entry.Id);
                    bool travel = enemy.EvolutionTravelUntil > simulation.Elapsed;
                    Require(simulation.ExpandedFields.Count > 0 || simulation.Projectiles.Count > 0 || travel, "Moveset has actual projectiles, hazards or travel " + entry.Id);
                    Require(simulation.ExpandedFields.All(field => field.Source == enemy && field.Damage > 0 && field.At > field.Created), "Delayed attributed body hazards " + entry.Id);
                    Require(simulation.Projectiles.All(projectile => projectile.SourceUnitId == enemy.Id && projectile.Faction == Faction.Enemy && projectile.Velocity.magnitude <= 2.401f && projectile.Sprite != null), "Slow readable body projectiles, no weapon required " + entry.Id);
                    if (entry.Ability == EnemyMutation.CoinBarrage)
                    {
                        Require(enemy.StolenCoins == 7 && simulation.Projectiles.Count >= 2, "Loose coins actually fuel barrage");
                        simulation.DamageFixture(enemy, enemy.Health + 100);
                        Require(simulation.Pickups.Where(pickup => pickup.Kind == LootKind.Coin).Sum(pickup => pickup.Value) >= 7, "Stolen coins returned on death");
                    }
                    if (entry.Ability == EnemyMutation.KegRebound || entry.Ability == EnemyMutation.RamBackstep)
                    {
                        int repeats = enemy.EvolutionRepeats;
                        for (int tick = 0; tick < 19; tick++) simulation.Step(0.05f, Vector2.up);
                        Require(repeats == 1 && enemy.EvolutionRepeats == 0, "Second leg executes after first landing " + entry.Id);
                    }
                    simulation.EnterRest(); Require(simulation.ExpandedFields.Count == 0, "No hazards leak into shop " + entry.Id);
                }
                UnityEngine.Object.DestroyImmediate(normal); UnityEngine.Object.DestroyImmediate(mutated); UnityEngine.Object.DestroyImmediate(weapon);
            }
            Require(artwork.Count == 320, "320 separate species/form/pose drawings");
            Require(!artwork.Intersect(catalog.Items.Select(item => item.Icon)).Any() && !artwork.Intersect(catalog.Companions.Select(unit => unit.Body)).Any(), "Enemy art not reused by allies or relics");
            int previous = 0;
            for (int wave = 1; wave <= 13; wave++)
            {
                using var session = new RunSession(catalog); session.Start("LanternKnight", 22077); session.State.Wave = wave;
                CheckpointCatalog arena = session.PrepareCombat();
                int count = library.Species.Count(entry => wave >= EnemyEvolutionLibrary.MutationWave(entry.Id));
                Require(count >= previous && (wave > 5 || count == 0), "Gradual monotonic mutation roster, wave " + wave); previous = count;
                Require(arena.EnemyPool.Where(enemy => enemy.SpecialEnemy == SpecialEnemyKind.None).All(enemy => enemy.EvolutionStage == (wave >= EnemyEvolutionLibrary.MutationWave(enemy.Id) ? 1 : 0)), "Runtime original pools use correct evolutionary form, wave " + wave);
                if (wave == 1) Require(arena.EnemyPool.All(enemy => enemy.Weapon.Kind == WeaponKind.Contact && enemy.Mutation == EnemyMutation.None), "Wave one remains contact-only, with no mutated projectiles");
            }
            Require(previous == 20, "All species evolved by late waves");
            using (var session = new RunSession(catalog))
            {
                session.Start("LanternKnight", 22077); session.State.Wave = 13; session.State.Roster.Clear();
                CheckpointCatalog arena = session.PrepareCombat(); arena.Boss = null; arena.EnemyBudget = 0;
                arena.EnableArenaToys = arena.EnableSpectacles = arena.EnableBossArenaPhase = false;
                var simulation = new CombatSimulation(arena); simulation.Start(22077); simulation.Champion.NextAttack = float.PositiveInfinity;
                var definitions = new List<UnitDefinition>();
                for (int index = 0; index < 12; index++)
                {
                    UnitDefinition unit = UnityEngine.Object.Instantiate(catalog.Enemies[index]); definitions.Add(unit); EnemyEvolutionLibrary.Apply(unit, 13); unit.Speed = 0;
                    UnitState enemy = simulation.AddFixture(unit, Faction.Enemy, Vector2.right * (1.5f + index * 0.1f));
                    enemy.NextAttack = enemy.NextContact = enemy.NextBehavior = float.PositiveInfinity; enemy.NextEvolutionAttack = 0; enemy.Awakened = true;
                }
                simulation.Step(0.05f, Vector2.zero);
                Require(simulation.Units.Count(enemy => enemy.EvolutionReleaseAt > simulation.Elapsed) == 4, "At most four simultaneous mutation windups in a crowd");
                UnitState charging = simulation.Units.First(enemy => enemy.EvolutionReleaseAt > simulation.Elapsed);
                simulation.DamageFixture(charging, charging.Health + 1);
                for (int tick = 0; tick < 18; tick++) simulation.Step(0.05f, Vector2.zero);
                Require(charging.EvolutionCasts == 0 && !simulation.ExpandedFields.Any(field => field.Source == charging), "Dead enemy cannot release its pending mutation");
                Require(simulation.ExpandedFields.Count <= 48 && simulation.Projectiles.Count <= 4096, "Mutation pressure stays inside existing hazard/projectile budgets");
                simulation.EnterRest(); Require(simulation.ExpandedFields.Count == 0 && simulation.Projectiles.Count == 0, "Rest clears crowded mutation projectiles and hazards");
                foreach (UnitDefinition definition in definitions) UnityEngine.Object.DestroyImmediate(definition);
            }
            File.WriteAllLines("PreviewExports/K22/Focused_Checks.txt", notes); Debug.Log("K22_EVOLUTION_CHECKS_PASSED " + notes.Count);
        }
    }
}
