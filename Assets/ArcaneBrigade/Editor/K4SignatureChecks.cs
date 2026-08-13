using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K4SignatureChecks
    {
        [MenuItem("Arcane Brigade/K4/Check signature weapons and build")]
        public static void BuildAndCheck() { Run(); FullRunBuilder.BuildWindows(); }

        public static void Run()
        {
            VisualQualityChecks.Run();
            RunSignatures();
        }

        /// <summary>The signature rules alone (bell chimes, umbrella drops, stamp marks), without the visual suite.</summary>
        public static void RunSignatures()
        {
            var results = new List<string>();
            Action<bool, string> require = (condition, name) => { if (!condition) throw new InvalidOperationException(name); results.Add("PASS " + name); };
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            require(catalog.Weapons.Count(weapon => weapon.Id == "DreamStamp") == 1, "New stamp has exactly one stable pool ID");
            foreach (string id in new[] { "UmbrellaLance", "BellFork", "DreamStamp" })
            {
                WeaponDefinition weapon = catalog.Weapon(id);
                require(weapon != null && weapon.Sprite != null && weapon.Signature != SignatureWeapon.None, "Authored signature and art " + id);
                require(weapon.SignatureBeats >= 2 && weapon.SignatureDelay >= 0.05f && weapon.Cooldown > 0 && weapon.SignatureScale > 0 && weapon.MarkLifetime > 0, "Finite cadence " + id);
            }
            string before = JsonUtility.ToJson(catalog); K4SignatureBuilder.Apply();
            require(before == JsonUtility.ToJson(catalog), "Reapply does not duplicate pool or reset tuning");
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 13104);
                session.State.Roster.Clear(); session.State.Roster.Add(new RosterEntry { UnitId = catalog.Companions[0].Id, WeaponId = "DreamStamp", Tier = 2 });
                string state = JsonUtility.ToJson(session.State);
                RunBuildReadModel read = RunBuildReadModel.Capture(session, catalog, session.State.Health);
                require(state == JsonUtility.ToJson(session.State) && read.Companions[0].Explanation.Contains("one mark"), "Details reads actual tier twist without rolling RNG");
                CheckpointCatalog combat = session.PrepareCombat();
                require(combat.StartingCompanions[0].CombatTier == 2 && combat.StartingCompanions[0].Weapon.Signature == SignatureWeapon.Stamp, "Runtime clone retains signature and tier");
                string root = Path.Combine(Application.temporaryCachePath, "ArcaneK4", Guid.NewGuid().ToString("N"));
                new RunStorage(root).SaveRun(session.State);
                using (var loaded = new RunSession(catalog))
                {
                    loaded.Restore(new RunStorage(root).LoadRun(loaded));
                    require(loaded.PrepareCombat().StartingCompanions[0].Weapon.Id == "DreamStamp", "Stamp roster survives save restore without schema change");
                }
            }
            RunSignatureRules(catalog, results, require);
            Directory.CreateDirectory("PreviewExports/K4"); File.WriteAllLines("PreviewExports/K4/Signature_Checks.txt", results);
            Debug.Log("ARCANE_K4_SIGNATURE_CHECKS_PASSED " + results.Count);
        }

        /// <summary>The combat rules of the signatures: bell chimes, umbrella drops and stamp marks in real fixtures.</summary>
        public static void RunSignatureRules()
        {
            var results = new List<string>();
            Action<bool, string> require = (condition, name) => { if (!condition) throw new InvalidOperationException(name); results.Add("PASS " + name); };
            RunSignatureRules(AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset"), results, require);
            Directory.CreateDirectory("PreviewExports/K4"); File.WriteAllLines("PreviewExports/K4/Signature_Rules.txt", results);
        }

        private static void RunSignatureRules(RunCatalog catalog, List<string> results, Action<bool, string> require)
        {
            using (var fixture = new Fixture(catalog, "BellFork"))
            {
                UnitState front = fixture.Enemy(new Vector2(1, 0)); UnitState back = fixture.Enemy(new Vector2(-1, 0)); UnitState far = fixture.Enemy(new Vector2(8, 0));
                // every attack carries a lighter volley (18/10/2026; was: every third attack a full one)
                float share = 1f / Mathf.Max(2, fixture.Weapon.SignatureBeats);
                fixture.Simulation.SignatureBeatFixture(fixture.Source);
                require(fixture.Simulation.PendingSignaturePulses == 1 && front.Health == 1000, "Bell schedules a volley on every attack rather than instant damage");
                fixture.Simulation.Pause(true); fixture.Tick(20);
                require(fixture.Simulation.Elapsed == 0 && front.Health == 1000, "Pause freezes delayed wave");
                fixture.Simulation.Pause(false);
                bool secondary = false;
                for (int tick = 0; tick < 5; tick++) { fixture.Tick(1); secondary |= fixture.Simulation.Events.Any(value => value.Kind == CombatEventKind.SignaturePulse && value.Cause.Depth == 1); }
                require(fixture.Simulation.Projectiles.Count(shot => shot.SignatureShot) == 5, "Bell wave is thrown as a fan of five chimes, not a ring round the wielder");
                fixture.Tick(6);
                float expected = 1000 - fixture.Weapon.Damage * fixture.Weapon.SignatureScale * share;
                require(Mathf.Approximately(front.Health, expected) && back.Health == 1000 && far.Health == 1000, "Bell chimes strike the foe ahead once per volley, never behind the wielder or past their reach");
                require(secondary, "Bell secondary cause cannot activate root procs");
                fixture.Tick(30); require(Mathf.Approximately(front.Health, expected), "No echo recursion at tier one");
                fixture.Source.Definition.CombatTier = 2;
                fixture.Simulation.SignatureBeatFixture(fixture.Source);
                require(fixture.Simulation.PendingSignaturePulses == 2, "Tier bell schedules exactly wave plus weak echo");
                fixture.Tick(14);
                require(fixture.Simulation.PendingSignaturePulses == 0 && Mathf.Approximately(front.Health, expected - fixture.Weapon.Damage * fixture.Weapon.SignatureScale * share * 1.35f), "Tier echo resolves once with finite scale");
                for (int index = 0; index < 3; index++) fixture.Simulation.SignatureBeatFixture(fixture.Source);
                fixture.Simulation.EnterRest(); fixture.Tick(20);
                require(fixture.Simulation.PendingSignaturePulses == 0 && fixture.Source.AttackBeat == 0, "Rest clears pending volleys and echoes");
            }
            using (var fixture = new Fixture(catalog, "UmbrellaLance"))
            {
                UnitState front = fixture.Enemy(new Vector2(1, 0)); UnitState back = fixture.Enemy(new Vector2(-1, 0)); UnitState boss = fixture.Enemy(new Vector2(1, 0.4f), true);
                fixture.Simulation.SignatureBeatFixture(fixture.Source); // one attack, one volley of drops
                fixture.Tick(9);
                require(front.Health < 1000 && back.Health == 1000, "Umbrella drops fly forward only, not another full ring");
                require(front.Position.x > 1 && front.Position.x <= 1.8f, "Umbrella pushes enemy within bounded distance");
                require(boss.Position.x - 1 < front.Position.x - 1, "Boss resists umbrella push");
                UnitState ally = fixture.Simulation.AddCompanion(fixture.Source.Definition);
                require(ally.Alive && ally.Health == ally.Definition.Health && !float.IsInfinity(ally.Health), "Section 1 mortal ally joins at full finite health after umbrella volley");
                require(!fixture.Combat.CompanionInterceptor, "Umbrella does not silently grant bullet blocking");
                fixture.Source.Definition.CombatTier = 2;
                UnitState outer = fixture.Enemy(new Vector2(2.4f, 0));
                for (int index = 0; index < 3; index++) fixture.Simulation.SignatureBeatFixture(fixture.Source);
                fixture.Tick(14); require(outer.Health < 1000, "Tier umbrella drops fly farther");
            }
            using (var fixture = new Fixture(catalog, "BellFork"))
            {
                fixture.Source.Health = 20;
                fixture.Combat.Procs = new[] { new ActiveProc { Stacks = 1, Rule = new ProcRule { Id = "k4-kill-heal", ItemId = "fixture", Trigger = ProcTrigger.EnemyKilled, Action = ProcAction.HealChampion, Power = 10, Chance = 1, Cooldown = 0.1f } } };
                UnitState target = fixture.Enemy(new Vector2(1, 0)); target.Health = 1;
                for (int index = 0; index < 3; index++) fixture.Simulation.SignatureBeatFixture(fixture.Source);
                fixture.Tick(10);
                require(!target.Alive && fixture.Simulation.Kills == 1 && fixture.Source.Health == 20, "Secondary signature kill cannot trigger kill-heal proc");
                require(fixture.Simulation.Pickups.Count(value => value.Kind == LootKind.Coin) == 1, "Payoff kill produces normal loot once without multiplying it");
            }
            using (var fixture = new Fixture(catalog, "DreamStamp"))
            {
                UnitState target = fixture.Enemy(new Vector2(1, 0)); UnitState neighbor = fixture.Enemy(new Vector2(1.2f, 0.4f));
                fixture.Simulation.StampFixture(fixture.Source, target); fixture.Simulation.StampFixture(fixture.Source, target);
                require(target.Health == 1000 && target.VisibleMarks == 2, "Stamp marks accumulate without premature burst");
                fixture.Simulation.StampFixture(fixture.Source, target);
                require(Mathf.Approximately(target.Health, 1000 - fixture.Weapon.Damage * fixture.Weapon.SignatureScale) && neighbor.Health == 1000, "Stamp payoff is single target and thresholded");
                fixture.Source.Definition.CombatTier = 2;
                for (int index = 0; index < 3; index++) fixture.Simulation.StampFixture(fixture.Source, target);
                require(neighbor.VisibleMarks == 1 && neighbor.Health == 1000, "Tier transfers one mark, not burst or damage clone");
                fixture.Simulation.StampFixture(fixture.Source, target); fixture.Tick(55);
                require(fixture.Simulation.ActiveStampPairs == 0 && target.VisibleMarks == 0 && neighbor.VisibleMarks == 0, "Expired marks are pruned with no retained dead pairs");
                UnitState otherSource = fixture.Simulation.AddCompanion(fixture.Source.Definition);
                fixture.Simulation.StampFixture(fixture.Source, target); fixture.Simulation.StampFixture(otherSource, target);
                require(fixture.Simulation.ActiveStampPairs == 2, "Different sources do not pool marks into unintended burst");
                fixture.Simulation.DamageFixture(target, 2000); fixture.Tick(1);
                require(fixture.Simulation.ActiveStampPairs == 0 && fixture.Simulation.Kills == 1 && fixture.Simulation.Pickups.Count(value => value.Kind == LootKind.Coin) == 1, "Dead marks prune and logical kill drops exactly one coin");
                fixture.Simulation.EnterRest(); require(fixture.Simulation.ActiveStampPairs == 0, "No marks persist into shop");
                fixture.Simulation.Start(13104);
                require(fixture.Simulation.PendingSignaturePulses == 0 && fixture.Simulation.ActiveStampPairs == 0 && fixture.Simulation.StampBursts == 0, "Restart resets signature state and diagnostic counters");
            }
            using (var fixture = new Fixture(catalog, "DreamStamp"))
            {
                UnitState target = fixture.Enemy(new Vector2(1, 0)); UnitState neighbor = fixture.Enemy(new Vector2(1.3f, 0));
                fixture.Source.PendingAttack = true; fixture.Source.AttackTargetId = target.Id; fixture.Source.ImpactTime = 0.01f;
                fixture.Tick(1);
                require(target.Health == 1000 - fixture.Weapon.Damage && neighbor.Health == 1000, "Real stamp impact hits only locked target");
                fixture.Source.PendingAttack = true; fixture.Source.AttackTargetId = target.Id; fixture.Source.ImpactTime = fixture.Simulation.Elapsed + 0.01f;
                target.Position = target.PreviousPosition = new Vector2(8, 0); fixture.Tick(1);
                require(target.Health == 1000 - fixture.Weapon.Damage && neighbor.Health == 1000, "Out-of-range stamp misses instead of selecting free replacement");
            }
            foreach (string id in new[] { "CrescentBow", "RuneStaff" })
                using (var fixture = new Fixture(catalog, id))
                {
                    UnitState target = fixture.Enemy(new Vector2(2, 0)); fixture.Source.NextAttack = 0;
                    fixture.Tick(Mathf.CeilToInt((fixture.Weapon.Windup + 0.1f) / 0.05f));
                    require((fixture.Simulation.Projectiles.Count > 0 || target.Health < 1000) && fixture.Simulation.PendingSignaturePulses == 0, "Existing projectile behavior intact " + id);
                }
        }

        private sealed class Fixture : IDisposable
        {
            private readonly RunSession session;
            private readonly List<UnityEngine.Object> assets = new List<UnityEngine.Object>();
            public readonly CheckpointCatalog Combat;
            public readonly CombatSimulation Simulation;
            public UnitState Source => Simulation.Champion;
            public WeaponDefinition Weapon => Source.Definition.Weapon;
            public Fixture(RunCatalog catalog, string id)
            {
                session = new RunSession(catalog); session.Start(catalog.Champions[0].Id, 13104); session.State.Roster.Clear();
                Combat = session.PrepareCombat(); Combat.EnemyBudget = 0; Combat.Boss = null; Combat.WaveDuration = 1000;
                Combat.EnableArenaToys = Combat.EnableBossArenaPhase = false;
                Combat.EnableHeroIdentities = false; // weapon rules are measured without the hero's kit
                Combat.PassiveHeal = 0; Combat.ChestInterval = 1000; Combat.ExtendedLoot = false; Combat.Procs = new ActiveProc[0];
                Combat.Champion.Weapon = UnityEngine.Object.Instantiate(catalog.Weapon(id)); assets.Add(Combat.Champion.Weapon);
                Combat.Champion.Speed = 0; Combat.Champion.CriticalChance = 0;
                Simulation = new CombatSimulation(Combat); Simulation.Start(13104);
                Source.NextAttack = Source.NextPassive = float.PositiveInfinity; Source.AttackDirection = Vector2.right;
            }
            public UnitState Enemy(Vector2 position, bool boss = false)
            {
                UnitDefinition definition = UnityEngine.Object.Instantiate(Combat.MeleeEnemy); assets.Add(definition);
                definition.IsBoss = boss; definition.Health = 1000; definition.Speed = 0; definition.ContactDamage = 0;
                UnitState enemy = Simulation.AddFixture(definition, Faction.Enemy, position);
                enemy.NextAttack = enemy.NextContact = enemy.NextBossPattern = float.PositiveInfinity; return enemy;
            }
            public void Tick(int count) { for (int index = 0; index < count; index++) Simulation.Step(0.05f, Vector2.zero); }
            public void Dispose() { foreach (UnityEngine.Object asset in assets) UnityEngine.Object.DestroyImmediate(asset); session.Dispose(); }
        }
    }
}
