using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K25EnemyAffixChecks
    {
        private static void Invoke(CombatSimulation simulation, string method, params object[] args) => typeof(CombatSimulation).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(simulation, args);
        public static void Run()
        {
            var notes = new List<string>();
            void Check(bool valid, string caption) { if (!valid) throw new InvalidOperationException("K25 " + caption); notes.Add("PASS " + caption); }
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            EnemyAffixLibrary library = EnemyAffixLibrary.Current;
            Check(StageTransfer.CarrierFor(6) == StageCarrier.Hand && StageTransfer.CarrierFor(11) == StageCarrier.CarpetTongue && StageTransfer.CarrierFor(16) == StageCarrier.KeyholeEye && StageTransfer.CarrierFor(21) == StageCarrier.Hand, "Three anatomy carriers rotate across the Act I zone changes");
            foreach (StageCarrier carrier in Enum.GetValues(typeof(StageCarrier)))
            {
                Vector2 start = new Vector2(3, -2);
                Check(StageTransfer.PassengerPosition(start, 0, carrier) == start && Vector2.Distance(StageTransfer.PassengerPosition(start, 2.6f, carrier), new Vector2(0, -0.2f)) < 0.001f, "Carrier starts and lands at exact positions " + carrier);
                Vector2 previous = start;
                for (int tick = 1; tick <= 260; tick++)
                {
                    Vector2 position = StageTransfer.PassengerPosition(start, tick * 0.01f, carrier);
                    if ((position - previous).magnitude > 0.2f) throw new InvalidOperationException("Discontinuous carrier " + carrier);
                    previous = position;
                }
                Check(true, "Continuous pickup and throw path " + carrier);
            }
            Check(catalog.Enemies.Length == 28 && library != null && library.Species.Length == 8, "Twenty original and eight new species");
            Check(library.Effects.Length == 16 && library.Effects.Distinct().Count() == 16, "Sixteen dedicated smoke and flame effect drawings");
            var drawings = new HashSet<Sprite>();
            foreach (SpecialEnemyArt art in library.Species)
            {
                UnitDefinition source = catalog.Enemies.First(unit => unit.Id == art.Id);
                Check(source.SpecialEnemy != SpecialEnemyKind.None && source.AuthoredMotion == art.Normal && source.UnlockWave >= 6, "Specialist identity and safe unlock " + art.Id);
                foreach (AuthoredMotionSet motion in new[] { art.Normal, art.Evolved })
                {
                    Sprite[] unique = motion.Idle.Concat(motion.Walk).Concat(motion.Windup).Concat(motion.Release).Concat(motion.Recovery).Distinct().ToArray();
                    Check(unique.Length == 8 && unique.All(sprite => sprite != null && sprite.pivot.y == 0 && UnityEditor.AssetDatabase.GetAssetPath(sprite).Contains("/K25/")), "Eight dedicated ground-aligned poses " + motion.name);
                    foreach (Sprite sprite in unique) Check(drawings.Add(sprite), "No reused body drawing " + sprite.name);
                }
                foreach (int stage in new[] { 0, 1 })
                {
                    using var fixture = new Fixture(catalog, 19);
                    UnitDefinition definition = UnityEngine.Object.Instantiate(source);
                    EnemyAffixLibrary.ApplyArt(definition, stage == 0 ? source.UnlockWave : 30);
                    UnitState enemy = fixture.Enemy(definition, Vector2.zero);
                    enemy.Variant.NextSpecial = 0; enemy.Variant.NextAffix = float.PositiveInfinity;
                    if (source.SpecialEnemy == SpecialEnemyKind.AshSwallower)
                    {
                        for (int index = 0; index < 3; index++) { UnitState corpse = fixture.Enemy(fixture.Arena.MeleeEnemy, Vector2.left * 0.5f); fixture.Simulation.DamageFixture(corpse, corpse.Health + 1); }
                    }
                    fixture.Simulation.Step(0.05f, Vector2.zero);
                    Check(enemy.Variant.ReleaseAt > fixture.Simulation.Elapsed && enemy.Variant.SpecialCasts == 0, "Body windup before special " + source.Id + "/" + stage);
                    float clock = fixture.Simulation.Elapsed; fixture.Simulation.Pause(true); fixture.Simulation.Step(0.05f, Vector2.zero);
                    Check(fixture.Simulation.Elapsed == clock && enemy.Variant.SpecialCasts == 0, "Pause freezes specialist " + source.Id + "/" + stage); fixture.Simulation.Pause(false);
                    for (int tick = 0; tick < 22; tick++) fixture.Simulation.Step(0.05f, Vector2.zero);
                    Check(enemy.Variant.SpecialCasts == 1, "Actual specialist release " + source.Id + "/" + stage + " casts " + enemy.Variant.SpecialCasts + " releaseAt " + enemy.Variant.ReleaseAt + " now " + fixture.Simulation.Elapsed + " phase " + fixture.Simulation.Phase + " surge " + enemy.SurgeDirection + " role " + enemy.Role + " alive " + enemy.Alive);
                    bool actual = fixture.Simulation.Projectiles.Any(shot => shot.SourceUnitId == enemy.Id) || fixture.Simulation.ExpandedFields.Any(field => field.Source == enemy) || enemy.Variant.TravelEnd > fixture.Simulation.Elapsed;
                    Check(actual, "Special has real travel or attack " + source.Id + "/" + stage);
                    if (source.SpecialEnemy == SpecialEnemyKind.AshSwallower) Check(enemy.Variant.AshMeals == 3, "Swallower consumes bounded actual remains");
                    if (source.SpecialEnemy == SpecialEnemyKind.SmokingCoffin || source.SpecialEnemy == SpecialEnemyKind.EmberProcession)
                    {
                        UnitState[] children = fixture.Simulation.Units.Where(unit => unit.Variant != null && unit.Variant.Summoned).ToArray();
                        Check(children.Length >= 2 && children.Length <= 3 && children.All(child => child.EntranceUntil > 0 && child.Variant.Affix == EnemyAffix.None), "Telegraphed bounded summons, no recursive affixes " + source.Id);
                        fixture.Simulation.Pickups.ToList().ForEach(pickup => pickup.Value = 0);
                        foreach (UnitState child in children) fixture.Simulation.DamageFixture(child, child.Health + 1);
                        Check(fixture.Simulation.Pickups.All(pickup => pickup.Value == 0), "Summons cannot farm gold " + source.Id);
                    }
                    Check(definition.EvolutionStage == stage && definition.Health == source.Health && definition.Speed == source.Speed, "Evolution art without extra stat cliff " + source.Id + "/" + stage);
                    fixture.Simulation.EnterRest(); Check(fixture.Simulation.Projectiles.Count == 0 && fixture.Simulation.ExpandedFields.Count == 0 && enemy.Variant.ReleaseAt == 0, "Rest clears specialist effects " + source.Id);
                    UnityEngine.Object.DestroyImmediate(definition);
                }
            }
            Check(drawings.Count == 128 && !drawings.Intersect(catalog.Items.Select(item => item.Icon)).Any() && !drawings.Intersect(catalog.Companions.Select(unit => unit.Body)).Any(), "128 original poses independent of relics and allies");
            foreach (EnemyAffix affix in Enum.GetValues(typeof(EnemyAffix)))
            {
                if (affix == EnemyAffix.None) continue;
                using var fixture = new Fixture(catalog, 19);
                UnitState enemy = fixture.Enemy(fixture.Arena.MeleeEnemy, Vector2.zero);
                enemy.Variant.Affix = affix; enemy.Variant.NextAffix = 0; enemy.Variant.NextSpecial = float.PositiveInfinity;
                float health = enemy.Definition.Health; Sprite body = enemy.Definition.Body;
                if (affix == EnemyAffix.Blackfire) enemy.Health = health * 0.35f;
                fixture.Simulation.Step(0.05f, Vector2.zero);
                Check(enemy.Variant.ReleaseAt > fixture.Simulation.Elapsed && fixture.Simulation.Projectiles.Count == 0 && fixture.Simulation.ExpandedFields.Count == 0, "Affix anticipation precedes damage " + affix);
                for (int tick = 0; tick < 26; tick++) fixture.Simulation.Step(0.05f, Vector2.zero);
                Check(enemy.Variant.AffixCasts == 1 && (fixture.Simulation.Projectiles.Count > 0 || fixture.Simulation.ExpandedFields.Count > 0 || enemy.Variant.TravelEnd > fixture.Simulation.Elapsed || enemy.Variant.DecoyUntil > fixture.Simulation.Elapsed), "Affix real payoff " + affix);
                Check(enemy.Definition.Health == health && enemy.Definition.Body == body && enemy.Definition.SpecialEnemy == SpecialEnemyKind.None, "Affix preserves original species and stats " + affix);
                Check(fixture.Simulation.Projectiles.All(shot => shot.Faction == Faction.Enemy && shot.Velocity.magnitude < 2.5f && shot.SourceUnitId == enemy.Id), "Slow attributed affix projectiles " + affix);
                if (affix == EnemyAffix.Blackfire) Check(enemy.Variant.BlackfireSpent, "Blackfire is one low-health burst, not death explosion");
                float targetClock = fixture.Simulation.Elapsed; fixture.Simulation.Pause(true); fixture.Simulation.Step(0.05f, Vector2.zero);
                Check(fixture.Simulation.Elapsed == targetClock, "Pause also freezes affix travel " + affix);
                fixture.Simulation.Pause(false); fixture.Simulation.EnterRest(); Check(enemy.Variant.DecoyUntil == 0, "Rest clears smoke afterimage " + affix);
            }
            string schedule = null;
            for (int replay = 0; replay < 2; replay++)
            {
                using var fixture = new Fixture(catalog, 19);
                for (int index = 0; index < 80; index++) Invoke(fixture.Simulation, "InitializeEnemyVariant", fixture.Enemy(fixture.Arena.MeleeEnemy, new Vector2(8, 3)));
                UnitState[] assigned = fixture.Simulation.Units.Where(unit => unit.Variant != null && unit.Variant.Affix != EnemyAffix.None).ToArray();
                Check(assigned.Length > 0 && assigned.Length <= 4, "Natural affix population capped at four");
                string result = string.Join(",", assigned.Select(unit => unit.Id + ":" + unit.Variant.Affix));
                if (schedule != null) Check(schedule == result, "Affix assignment deterministic without shop RNG");
                schedule = result;
            }
            using (var fixture = new Fixture(catalog, 1))
            {
                UnitState enemy = fixture.Enemy(fixture.Arena.MeleeEnemy, Vector2.zero); Invoke(fixture.Simulation, "InitializeEnemyVariant", enemy);
                Check(enemy.Variant.Affix == EnemyAffix.None && fixture.Arena.EnemyPool.All(unit => unit.SpecialEnemy == SpecialEnemyKind.None), "Wave one has no specialists or affixes");
            }
            using (var fixture = new Fixture(catalog, 19))
            {
                UnitState coffin = fixture.Enemy(catalog.Enemies.First(unit => unit.SpecialEnemy == SpecialEnemyKind.SmokingCoffin), Vector2.zero);
                coffin.Health = coffin.Definition.Health * 0.4f; coffin.Variant.NextSpecial = 0;
                for (int tick = 0; tick < 24; tick++) fixture.Simulation.Step(0.05f, Vector2.zero);
                Check(coffin.Variant.CoffinBroken && coffin.Variant.TravelEnd > fixture.Simulation.Elapsed && !fixture.Simulation.Units.Any(unit => unit.Variant != null && unit.Variant.Summoned), "Damaged coffin changes to charge instead of more summons");
            }
            using (var fixture = new Fixture(catalog, 19))
            {
                UnitState leader = fixture.Enemy(catalog.Enemies.First(unit => unit.SpecialEnemy == SpecialEnemyKind.EmberProcession), Vector2.zero);
                leader.Variant.NextSpecial = 0;
                for (int tick = 0; tick < 24; tick++) fixture.Simulation.Step(0.05f, Vector2.zero);
                UnitState[] followers = fixture.Simulation.Units.Where(unit => unit.Variant != null && unit.Variant.Summoned).ToArray();
                Vector2 previous = followers[0].Position;
                for (int tick = 0; tick < 16; tick++) fixture.Simulation.Step(0.05f, Vector2.zero);
                Check(followers[0].Position != previous && followers.All(unit => unit.Variant.LeaderId == leader.Id), "Procession followers move into actual leader formation");
                fixture.Simulation.DamageFixture(leader, leader.Health + 1);
                Check(followers.All(unit => unit.Variant.LeaderLost && unit.Variant.RecoveryUntil > fixture.Simulation.Elapsed), "Killing procession leader breaks formation with recovery");
                fixture.Simulation.Champion.Health = 100000;
                foreach (UnitState follower in followers) follower.NextAttack = follower.NextContact = float.PositiveInfinity;
                int pickupCount = fixture.Simulation.Pickups.Count;
                for (int tick = 0; tick < 250; tick++) fixture.Simulation.Step(0.05f, Vector2.zero);
                Check(followers.All(unit => !unit.Alive) && fixture.Simulation.Pickups.Count <= pickupCount, "Summons expire without direct loot after twelve seconds");
            }
            using (var fixture = new Fixture(catalog, 19))
            {
                UnitState twin = fixture.Enemy(fixture.Arena.MeleeEnemy, Vector2.zero);
                twin.Variant.Affix = EnemyAffix.TwinFlame; twin.Variant.NextAffix = 0;
                for (int tick = 0; tick < 24; tick++) fixture.Simulation.Step(0.05f, Vector2.zero);
                Check(twin.Variant.AffixCasts == 1 && fixture.Simulation.Projectiles.Any(shot => shot.SourceUnitId == twin.Id), "Twin flame blue state launches shots");
                twin.Variant.RecoveryUntil = 0; twin.TelegraphUntil = 0; twin.Variant.NextAffix = 0;
                for (int tick = 0; tick < 24; tick++) fixture.Simulation.Step(0.05f, Vector2.zero);
                Check(twin.Variant.AffixCasts == 2 && twin.Variant.TravelEnd > fixture.Simulation.Elapsed, "Twin flame next state really charges");
            }
            using (var fixture = new Fixture(catalog, 19))
            {
                for (int index = 0; index < 12; index++)
                {
                    UnitState enemy = fixture.Enemy(fixture.Arena.MeleeEnemy, Vector2.zero);
                    enemy.Variant.Affix = EnemyAffix.BlueFlame; enemy.Variant.NextAffix = 0;
                }
                fixture.Simulation.Step(0.05f, Vector2.zero);
                Check(fixture.Simulation.Units.Count(unit => unit.Alive && unit.Variant != null && unit.Variant.ReleaseAt > fixture.Simulation.Elapsed) == 4, "Global four-windup limit holds under affix crowding");
                foreach (UnitState enemy in fixture.Simulation.Units.Where(unit => unit.Faction == Faction.Enemy).ToArray()) fixture.Simulation.DamageFixture(enemy, enemy.Health + 1);
                for (int tick = 0; tick < 30; tick++) fixture.Simulation.Step(0.05f, Vector2.zero);
                Check(fixture.Simulation.Projectiles.Count == 0 && fixture.Simulation.ExpandedFields.Count == 0, "Killing during windup cancels unreleased attacks");
            }
            using (var fixture = new Fixture(catalog, 19))
            {
                UnitState enemy = fixture.Enemy(fixture.Arena.MeleeEnemy, Vector2.zero);
                enemy.Variant.Affix = EnemyAffix.Blackfire; enemy.Variant.NextAffix = 0; enemy.Health = enemy.Definition.Health * 0.35f;
                fixture.Simulation.Champion.Position = fixture.Simulation.Champion.PreviousPosition = Vector2.right * 1.3f;
                float health = fixture.Simulation.Champion.Health;
                for (int tick = 0; tick < 60; tick++) fixture.Simulation.Step(0.05f, Vector2.zero);
                float loss = health - fixture.Simulation.Champion.Health;
                Check(loss > 0 && loss <= fixture.Simulation.Champion.Definition.Health * 0.08f + 0.01f, "Blackfire pulse deals actual bounded champion damage once");
            }
            using (var fixture = new Fixture(catalog, 19))
            {
                UnitState coffin = fixture.Enemy(catalog.Enemies.First(unit => unit.SpecialEnemy == SpecialEnemyKind.SmokingCoffin), Vector2.zero);
                Invoke(fixture.Simulation, "SummonVariantChildren", coffin, 100, false);
                Invoke(fixture.Simulation, "SummonVariantChildren", coffin, 100, false);
                Check(fixture.Simulation.Units.Count(unit => unit.Alive && unit.Variant != null && unit.Variant.Summoned) == 12, "Repeated summoning cannot exceed twelve live children");
                fixture.Arena.ExpandedContent = false;
                for (int index = 0; index < 80; index++) Invoke(fixture.Simulation, "InitializeEnemyVariant", fixture.Enemy(fixture.Arena.MeleeEnemy, new Vector2(8, 3)));
                Check(fixture.Simulation.Units.All(unit => unit.Variant == null || unit.Variant.Affix == EnemyAffix.None), "Legacy content cannot receive decorative-only affixes");
            }
            File.WriteAllLines("PreviewExports/K25/Focused_Checks.txt", notes); Debug.Log("K25_CHECKS_PASSED " + notes.Count);
        }
        private sealed class Fixture : IDisposable
        {
            private readonly RunSession session;
            public readonly CheckpointCatalog Arena;
            public readonly CombatSimulation Simulation;
            public Fixture(RunCatalog catalog, int wave)
            {
                session = new RunSession(catalog); session.Start("LanternKnight", 251077); session.State.Wave = wave; session.State.Roster.Clear();
                Arena = session.PrepareCombat(); Arena.Boss = null; Arena.EnemyBudget = 24;
                Arena.StoryCompanions = new UnitDefinition[0]; // the Act I party would walk in and finish the specimen first
                Arena.EnableArenaToys = Arena.EnableBossArenaPhase = Arena.EnableSpectacles = Arena.EnableHeroMechanics = false;
                Simulation = new CombatSimulation(Arena); Simulation.Start(251077);
                typeof(CombatSimulation).GetField("nextSpawn", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(Simulation, float.PositiveInfinity);
                Simulation.Champion.NextAttack = float.PositiveInfinity;
                Simulation.Champion.Position = Simulation.Champion.PreviousPosition = new Vector2(4.5f, 0);
            }
            public UnitState Enemy(UnitDefinition definition, Vector2 position)
            {
                UnitState enemy = Simulation.AddFixture(definition, Faction.Enemy, position);
                enemy.NextAttack = enemy.NextContact = enemy.NextBehavior = float.PositiveInfinity;
                enemy.Awakened = true; enemy.Variant.NextAffix = enemy.Variant.NextSpecial = float.PositiveInfinity;
                return enemy;
            }
            public void Dispose() => session.Dispose();
        }
    }
}
