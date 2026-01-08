using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K5Checks
    {
        public static void Run()
        {
            var results = new List<string>();
            Action<bool, string> require = (condition, message) => { if (!condition) throw new InvalidOperationException("K5: " + message); results.Add("PASS " + message); };
            RunCatalog content = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            require(content.ExpansionRevision == 1, "Expansion revision active");
            string before = JsonUtility.ToJson(content); K5Builder.Apply();
            require(before == JsonUtility.ToJson(content), "Migration idempotent preserves tuning and stable sprite IDs");
            require(content.Weapons.Select(weapon => weapon.Id).Distinct().Count() == content.Weapons.Length, "Unique stable weapon IDs");
            require(content.Weapons.Where(weapon => weapon.Family != ExpandedWeapon.None).Select(weapon => weapon.Family).Distinct().Count() == 9 && content.Weapons.Count(weapon => weapon.Signature != SignatureWeapon.None) == 3, "Twelve signature concepts present");
            foreach (WeaponDefinition weapon in content.Weapons.Where(weapon => weapon.Family != ExpandedWeapon.None))
            {
                require(weapon.Sprite != null && weapon.Sprite.rect.width > (weapon.Motion == WeaponMotion.Bow ? 90 : 150) && weapon.Sprite.rect.height > 150, weapon.Id + " full lossless alpha concept sprite");
                require(SignatureDescription.Describe(weapon, 2).Length > 100, weapon.Id + " actual rules exposed in tooltip");
                for (int tier = 1; tier <= 2; tier++)
                    using (var fixture = new Fixture(content, weapon.Id, tier))
                    {
                        UnitState target = fixture.Enemy(new Vector2(Mathf.Min(weapon.Range * 0.55f, 2f), 0));
                        fixture.Enemy(target.Position + Vector2.up * 0.6f); fixture.Enemy(target.Position + Vector2.down * 0.6f);
                        fixture.Tick(140);
                        require(target.Health < 10000 && fixture.Fires > 0, weapon.Id + " tier " + tier + " auto targeting real damage");
                        require(fixture.Simulation.Projectiles.Count < 15 && fixture.Simulation.ExpandedFields.Count < 35, weapon.Id + " finite secondary effects");
                        fixture.Simulation.EnterRest(); require(fixture.Simulation.ExpandedFields.Count == 0, weapon.Id + " boundary clears delayed effects");
                    }
            }
            using (var fixture = new Fixture(content, "CrescentBow"))
            {
                UnitState main = fixture.Enemy(Vector2.right * 1.5f);
                UnitState linked = fixture.Enemy(new Vector2(1.9f, 0.7f));
                fixture.Tick(15); require(linked.Health < 10000 && main.Health < 10000, "Zipper hits distinct nearby targets through chain");
            }
            using (var fixture = new Fixture(content, "SleepCamera"))
            {
                UnitState enemy = fixture.Enemy(Vector2.right * 1.5f); fixture.Tick(6);
                require(enemy.CameraWeakUntil > fixture.Simulation.Elapsed && enemy.ExposedUntil > fixture.Simulation.Elapsed, "Camera creates timed visible weakness");
            }
            using (var fixture = new Fixture(content, "TinFish"))
            {
                UnitState enemy = fixture.Enemy(Vector2.right * 1.5f); fixture.Tick(10);
                float health = enemy.Health; fixture.Source.NextAttack = float.PositiveInfinity; fixture.Tick(20);
                require(enemy.Health == health && fixture.Simulation.Projectiles.Count == 0, "Fish return cannot duplicate its outgoing hit");
            }
            using (var fixture = new Fixture(content, "AlarmMortar"))
            {
                UnitState enemy = fixture.Enemy(Vector2.right * 1.5f); fixture.Tick(6);
                require(enemy.Health == 10000 && fixture.Simulation.ExpandedFields.Any(field => field.Kind == ExpandedFieldKind.Fuse), "Mortar warns before dealing any damage");
                enemy.Position = Vector2.right * 8; fixture.Tick(24); require(enemy.Health == 10000, "Moving out evades locked mortar floor blast");
            }
            foreach (string id in new[] { "AlarmMortar", "CloudScissors" })
                using (var fixture = new Fixture(content, id))
                {
                    fixture.Catalog.Procs = new[] { new ActiveProc { Stacks = 1, Rule = new ProcRule { Id = "k5-kill-proc", ItemId = "k5-kill-proc", Trigger = ProcTrigger.EnemyKilled, Action = ProcAction.HealChampion, Power = 2, Cooldown = 1, Chance = 1 } } };
                    fixture.Source.Health -= 10; UnitState enemy = fixture.Enemy(Vector2.right * 1.5f); enemy.Health = 1;
                    fixture.Tick(35);
                    require(!enemy.Alive && fixture.Simulation.Contributions.Any(entry => entry.EffectId == "k5-kill-proc" && entry.Healing == 2), id + " delayed primary kill still runs finite on-kill proc in same tick");
                }
            foreach (string id in new[] { "tea-coaster", "scratch-record", "treasure-ribbon", "patch-spring", "thermal-ink", "repair-coupon" })
                require(content.Item(id).Icon != null && content.Item(id).EffectiveModifiers.Any(modifier => modifier.Value < 0), id + " visible real tradeoff and art");
            using (var fixture = new Fixture(content, "MusicBox"))
            {
                fixture.Catalog.ChorusStacks = 1; fixture.Enemy(Vector2.right * 1.5f); fixture.Tick(100);
                require(fixture.Simulation.ExpandedPayoffs > 0 && fixture.Simulation.Contributions.Any(entry => entry.EffectId == "expanded-echo" && entry.Damage > 0), "Two-part chorus echo contributes effective damage");
            }
            using (var fixture = new Fixture(content, "RubberHammer"))
            {
                UnitState enemy = fixture.Enemy(Vector2.right * 1.5f); enemy.BurnUntil = 100;
                fixture.Tick(100); require(fixture.Simulation.ExpandedPayoffs > 0, "Burn and third hammer create finite steam/impact payoff");
            }
            using (var fixture = new Fixture(content, "DreamIron"))
            {
                fixture.Catalog.ThermalInk = true; UnitState enemy = fixture.Enemy(Vector2.right * 1.5f);
                UnitDefinition stamp = UnityEngine.Object.Instantiate(fixture.Source.Definition); fixture.Owned.Add(stamp); stamp.Weapon = content.Weapon("DreamStamp");
                UnitState marker = fixture.Simulation.AddFixture(stamp, Faction.Companion, Vector2.zero); marker.NextAttack = float.PositiveInfinity;
                fixture.Simulation.StampFixture(marker, enemy);
                fixture.Tick(30); require(fixture.Simulation.Contributions.Any(entry => entry.EffectId == "thermal-ink" && entry.Damage > 0), "Thermal mark connector measures real damage");
            }
            using (var fixture = new Fixture(content, "KeyCleaver"))
            {
                fixture.Catalog.EnableArenaToys = true; fixture.Catalog.RunWave = 10; fixture.Catalog.PatchSpring = true;
                fixture.Simulation.ResetArenaToys(10); require(fixture.Simulation.ArenaToys.Count == 6, "Six distinct props safely placed at late wave");
                foreach (ArenaToyState toy in fixture.Simulation.ArenaToys)
                    for (int root = 0; root < toy.RequiredCharge; root++) fixture.Simulation.NotifyArenaToyHit(fixture.Source, toy.Position, toy.Position, 0, Vector2.right, new CombatCause(9000 + toy.Id * 10 + root, fixture.Source.Id));
                fixture.Tick(28); require(fixture.Simulation.ArenaToyActivations >= 6, "All six charge warning activation pipelines run");
                require(fixture.Simulation.Champion.Health == fixture.Simulation.Champion.Definition.Health, "Props never hurt player or immortal allies");
                fixture.Tick(130); require(fixture.Simulation.ArenaToys.Any(toy => toy.Charge > 0 || toy.Mode == ArenaToyMode.Warning), "Patch spring recharges after finite cooldown");
            }
            foreach (EnemyBehavior behavior in Enum.GetValues(typeof(EnemyBehavior)))
            {
                if (behavior == EnemyBehavior.Standard) continue;
                require(content.Enemies.Any(enemy => enemy.Behavior == behavior && enemy.Body != null && enemy.UnlockWave >= 2), behavior + " unique enemy and staged unlock");
            }
            foreach (ArenaToyKind kind in Enum.GetValues(typeof(ArenaToyKind)))
                using (var fixture = new Fixture(content, "CrescentBow"))
                {
                    fixture.Catalog.EnableArenaToys = true; fixture.Simulation.ResetArenaToys(10);
                    ArenaToyState toy = fixture.Simulation.ArenaToys.First(candidate => candidate.Kind == kind);
                    fixture.Source.Position = toy.Position - Vector2.right * 0.8f; fixture.Source.NextAttack = 100;
                    UnitState enemy = fixture.Enemy(toy.Position + Vector2.right * 0.4f);
                    for (int root = 0; root < toy.RequiredCharge; root++) fixture.Simulation.NotifyArenaToyHit(fixture.Source, toy.Position, toy.Position, 0, Vector2.right, new CombatCause(12000 + root, fixture.Source.Id));
                    fixture.Tick(24); require(enemy.Health == 10000, kind + " warning protects targets before payoff");
                    fixture.Tick(14);
                    if (kind == ArenaToyKind.InkBottle) require(enemy.ChillUntil > fixture.Simulation.Elapsed, "Ink bottle creates lasting slow field, not only cosmetic ink");
                    else if (kind == ArenaToyKind.FloorMusicBox) require(fixture.Source.NextAttack < 100, "Floor music pulse advances nearby attack cooldown");
                    else require(enemy.Health < 10000, kind + " actual damage/copy payoff reaches target");
                    require(fixture.Simulation.Contributions.Any(entry => entry.EffectId.StartsWith("arena-") && entry.Activations > 0), kind + " arena activation contributes once");
                }
            using (var fixture = new Fixture(content, "CrescentBow"))
            {
                fixture.Catalog.TreasureStacks = 2; fixture.Source.Health -= 10;
                fixture.Simulation.LootFixture(LootKind.Chest, Vector2.zero, 10); fixture.Enemy(Vector2.right * 1.5f); fixture.Tick(6);
                require(fixture.Source.Health == fixture.Source.Definition.Health - 4, "Treasure chest heals actual HP from two ribbon stacks after spawn protection expires");
                fixture.Tick(20);
                require(fixture.Simulation.Contributions.Any(entry => entry.EffectId == "treasure-ribbon" && entry.Damage > 0 && entry.Healing > 0), "Treasure bow window separately measures bonus damage/healing without double credit");
            }
            using (var fixture = new Fixture(content, "KeyCleaver"))
            {
                UnitState door = fixture.Enemy(Vector2.right * 1.5f); door.Definition.Behavior = EnemyBehavior.Door; door.Facing = Vector2.left;
                fixture.Simulation.ExpandedDamageFixture(door, 20, Vector2.right, fixture.Source.Definition.Weapon);
                require(door.Health == 9990, "Door frontal direct hit is halved");
                fixture.Simulation.ExpandedDamageFixture(door, 20, Vector2.left, fixture.Source.Definition.Weapon);
                require(door.Health == 9970, "Door flank hit bypasses guard");
            }
            using (var fixture = new Fixture(content, "KeyCleaver"))
            {
                fixture.Source.NextAttack = float.PositiveInfinity;
                UnitState magnet = fixture.Enemy(Vector2.right * 4); magnet.Definition.Behavior = EnemyBehavior.Magnet;
                fixture.Simulation.LootFixture(LootKind.Coin, new Vector2(4.6f, 0), 2); PickupState coin = fixture.Simulation.Pickups[0]; coin.Velocity = Vector2.zero;
                float distance = Vector2.Distance(coin.Position, magnet.Position); fixture.Tick(10);
                require(Vector2.Distance(coin.Position, magnet.Position) < distance && fixture.Simulation.Gold == 0 && coin.Value == 2, "Magnet moves coins without stealing wallet or changing value");
            }
            using (var fixture = new Fixture(content, "KeyCleaver"))
            {
                fixture.Source.NextAttack = float.PositiveInfinity; UnitState curtain = fixture.Enemy(Vector2.right * 5); curtain.Definition.Behavior = EnemyBehavior.Curtain;
                fixture.Tick(1); require(fixture.Simulation.ExpandedFields.Any(field => field.Kind == ExpandedFieldKind.Curtain && field.At > fixture.Simulation.Elapsed + 1), "Curtain warns at locked old player position");
                fixture.Source.Position = Vector2.up * 3; fixture.Tick(35); require(fixture.Source.Health == fixture.Source.Definition.Health, "Movement evades curtain with no unavoidable hit");
            }
            using (var fixture = new Fixture(content, "KeyCleaver"))
            {
                UnitState mimic = fixture.Enemy(Vector2.right * 5); mimic.Definition.Behavior = EnemyBehavior.Mimic;
                fixture.Tick(5); require(!mimic.Awakened && mimic.Position == Vector2.right * 5, "Mimic sleeps outside wake radius");
                fixture.Simulation.Champion.Position = Vector2.right * 3; fixture.Tick(1);
                require(mimic.Awakened && mimic.TelegraphUntil > fixture.Simulation.Elapsed, "Mimic warns before pursuit/contact");
            }
            foreach (BossMotif motif in Enum.GetValues(typeof(BossMotif)))
                using (var fixture = new Fixture(content, "KeyCleaver"))
                {
                    fixture.Catalog.Boss = UnityEngine.Object.Instantiate(fixture.Catalog.MeleeEnemy); fixture.Owned.Add(fixture.Catalog.Boss);
                    fixture.Catalog.Boss.IsBoss = true; fixture.Catalog.Boss.Motif = motif; fixture.Catalog.Boss.Health = 100;
                    fixture.Simulation.Start(5505); fixture.Simulation.Boss.Health = 49; fixture.Simulation.Boss.NextBossPattern = float.PositiveInfinity;
                    fixture.Tick(1); require(fixture.Simulation.ExpandedFields.Count > 0 && fixture.Simulation.ExpandedFields.All(field => field.At > fixture.Simulation.Elapsed + 1), motif + " half-health arena warning phase");
                    fixture.Tick(50); require(fixture.Simulation.Champion.Health > 0, motif + " no base-health one-hit phase");
                }
            using (var session = new RunSession(content))
            {
                session.Start(content.Champions[0].Id, 5505); session.State.Roster.Clear();
                foreach (WeaponDefinition weapon in content.Weapons.Where(weapon => weapon.Family != ExpandedWeapon.None)) session.State.Roster.Add(new RosterEntry { UnitId = content.Companions[0].Id, WeaponId = weapon.Id, Tier = 2 });
                foreach (ItemDefinition item in content.Items.Where(item => item.SignatureRole >= SignatureRelicKind.TeaCoaster)) session.State.Items.Add(new ItemStack { Id = item.Id, Count = 1 });
                string root = Path.Combine(Path.GetTempPath(), "ArcaneK5-" + Guid.NewGuid().ToString("N")); var storage = new RunStorage(root); storage.SaveRun(session.State);
                using (var restored = new RunSession(content))
                {
                    restored.Restore(storage.LoadRun(restored)); CheckpointCatalog arena = restored.PrepareCombat();
                    require(arena.StartingCompanions.Length == content.Weapons.Count(weapon => weapon.Family != ExpandedWeapon.None) && arena.StartingCompanions.Length >= 9 && arena.ChorusStacks == 1 && arena.PatchSpring && arena.ThermalInk, "Save restores all expanded stable IDs and roles");
                }
                require(content.BuildLinks.Count(link => link.Id.StartsWith("signature-")) == 6, "Six build graphs available without hidden set bonus");
                foreach (string id in new[] { "signature-tea", "signature-chorus", "signature-treasure", "signature-toys" }) require(SignatureBuildGraph.IsActive(id, session, content), id + " component-based activation read model");
            }
            Directory.CreateDirectory("PreviewExports/K5"); File.WriteAllLines("PreviewExports/K5/Checks.txt", results);
            Debug.Log("K5 checks PASS: " + results.Count);
        }

        private sealed class Fixture : IDisposable
        {
            private readonly RunSession session;
            public readonly List<UnityEngine.Object> Owned = new List<UnityEngine.Object>();
            public readonly CheckpointCatalog Catalog;
            public readonly CombatSimulation Simulation;
            public UnitState Source => Simulation.Champion;
            public int Fires;
            public Fixture(RunCatalog content, string weapon, int tier = 1)
            {
                session = new RunSession(content); session.Start(content.Champions[0].Id, 5505); session.State.Roster.Clear();
                Catalog = session.PrepareCombat(); Catalog.EnemyBudget = 0; Catalog.Boss = null; Catalog.WaveDuration = 1000;
                Catalog.EnableArenaToys = Catalog.EnableBossArenaPhase = false; Catalog.ChestInterval = 1000; Catalog.PassiveHeal = 0;
                Catalog.ExtendedLoot = false; Catalog.Procs = new ActiveProc[0]; Catalog.Champion.Passive = PassiveKind.None;
                Catalog.Champion.Weapon = UnityEngine.Object.Instantiate(content.Weapon(weapon)); Owned.Add(Catalog.Champion.Weapon);
                Catalog.Champion.Speed = 0; Catalog.Champion.CriticalChance = 0; Catalog.Champion.CombatTier = tier;
                Simulation = new CombatSimulation(Catalog); Simulation.Start(5505);
            }
            public UnitState Enemy(Vector2 position)
            {
                UnitDefinition definition = UnityEngine.Object.Instantiate(Catalog.MeleeEnemy); Owned.Add(definition);
                definition.Health = 10000; definition.Speed = 0; definition.ContactDamage = 0; definition.Behavior = EnemyBehavior.Standard;
                UnitState enemy = Simulation.AddFixture(definition, Faction.Enemy, position); enemy.NextAttack = enemy.NextContact = float.PositiveInfinity; return enemy;
            }
            public void Tick(int ticks) { for (int index = 0; index < ticks; index++) { Simulation.Step(0.05f, Vector2.zero); Fires += Simulation.Events.Count(value => value.Kind == CombatEventKind.Fire); } }
            public void Dispose() { foreach (UnityEngine.Object asset in Owned) UnityEngine.Object.DestroyImmediate(asset); session.Dispose(); }
        }
    }
}
