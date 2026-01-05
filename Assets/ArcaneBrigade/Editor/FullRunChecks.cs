using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class FullRunChecks
    {
        public static void Run(RunCatalog catalog)
        {
            var results = new List<string>();
            using (var shopSession = new RunSession(catalog))
            using (var shopRestored = new RunSession(catalog))
            {
                shopSession.Start(catalog.Champions[0].Id, 1234);
                shopSession.State.Phase = SessionPhase.Shop;
                shopSession.State.Gold = 0;
                shopSession.State.Eggs = 3;
                var eggOffer = new ShopOffer { Token = ++shopSession.State.NextOffer, Kind = OfferKind.Item, Id = catalog.Items[0].Id, Price = 2, Currency = ShopCurrency.Eggs };
                shopSession.State.SpecialOffers.Add(eggOffer);
                Check(shopSession.Buy(eggOffer.Token) && shopSession.State.Gold == 0 && shopSession.State.Eggs == 1, "Special shop spends eggs without gold", results);
                Check(!shopSession.Buy(eggOffer.Token), "Special offer sold guard", results);
                var unavailable = new ShopOffer { Token = ++shopSession.State.NextOffer, Kind = OfferKind.Item, Id = catalog.Items[0].Id, Price = 2, Currency = ShopCurrency.Eggs };
                shopSession.State.SpecialOffers.Add(unavailable);
                shopSession.State.Gold = 500;
                Check(!shopSession.Buy(unavailable.Token) && shopSession.State.Gold == 500 && !unavailable.Sold, "Gold cannot substitute for eggs", results);
                shopRestored.Restore(shopSession.State);
                Check(JsonUtility.ToJson(shopSession.State) == JsonUtility.ToJson(shopRestored.State), "Special wallet and sold offers survive restore", results);
                string specialStock = JsonUtility.ToJson(shopSession.State.SpecialOffers[0]) + JsonUtility.ToJson(shopSession.State.SpecialOffers[1]);
                Check(shopSession.Reroll() && shopSession.State.Eggs == 1 && specialStock == JsonUtility.ToJson(shopSession.State.SpecialOffers[0]) + JsonUtility.ToJson(shopSession.State.SpecialOffers[1]), "Gold reroll cannot refresh special stock or spend eggs", results);
                CheckpointCatalog restCatalog = shopSession.PrepareCombat();
                var restSimulation = new CombatSimulation(restCatalog);
                restSimulation.Start(1234);
                restSimulation.EnterRest();
                float health = restSimulation.Champion.Health;
                for (int index = 0; index < 60; index++) restSimulation.StepRest(1f / 60f, Vector2.right);
                Check(restSimulation.Champion.Position.x > 1f && restSimulation.Elapsed == 0f && restSimulation.Champion.Health == health && restSimulation.Projectiles.Count == 0, "Rest allows movement without timer, attack or health changes", results);
                Vector2 position = restSimulation.Champion.Position;
                restSimulation.Pause(true);
                restSimulation.StepRest(0.1f, Vector2.right);
                restSimulation.Pause(false);
                Check(restSimulation.Phase == RunPhase.Rest && restSimulation.Champion.Position == position, "Rest pause returns to rest without movement", results);
            }
            Check(catalog.Champions.Length == 3 && catalog.Companions.Length >= 6 && catalog.Weapons.Length >= 6 && catalog.Bosses.Length == 5, "Content roster", results);
            var ids = new HashSet<string>();
            foreach (UnitDefinition unit in catalog.Champions)
                Check(unit != null && unit.Body != null && unit.Weapon != null && ids.Add(unit.Id), "Champion " + unit.Id, results);
            foreach (UnitDefinition unit in catalog.Companions)
                Check(unit != null && unit.Body != null && unit.Tags.Length > 0 && ids.Add(unit.Id), "Companion " + unit.Id, results);
            foreach (WeaponDefinition weapon in catalog.Weapons)
                Check(weapon.Sprite != null && weapon.Damage > 0 && weapon.Cooldown > 0, "Weapon " + weapon.Id, results);

            using (var session = new RunSession(catalog))
            using (var restored = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 131026);
                Check(session.State.Roster.Count == 1 && session.State.Gold == catalog.StartingGold, "New run boundary", results);
                Check(!session.Buy(1) && !session.Reroll(), "No shopping during combat", results);
                CheckpointCatalog combat = session.PrepareCombat();
                combat.EnemyBudget = 0;
                combat.WaveDuration = 0.1f;
                var simulation = new CombatSimulation(combat);
                simulation.Start(combat.Seed);
                simulation.Step(0.1f, Vector2.zero);
                session.Settle(simulation);
                int gold = session.State.Gold;
                session.Settle(simulation);
                Check(session.State.Phase == SessionPhase.Shop && session.State.Gold == gold && session.State.Offers.Count == catalog.ShopSlots, "Exactly once settlement and shop", results);
                ShopOffer unaffordable = session.State.Offers[0];
                session.State.Gold = 0;
                Check(!session.Buy(unaffordable.Token) && !unaffordable.Sold && !session.Reroll(), "Insufficient gold leaves offer and reroll unchanged", results);
                session.State.Gold = 2000;
                restored.Restore(session.State);
                Check(session.Reroll() && restored.Reroll() && JsonUtility.ToJson(session.State) == JsonUtility.ToJson(restored.State), "Saved shop RNG resumes deterministically", results);
                ShopOffer recruit = session.State.Offers.Find(offer => offer.Kind == OfferKind.Companion);
                Check(session.Buy(recruit.Token) && restored.Buy(recruit.Token) && JsonUtility.ToJson(session.State) == JsonUtility.ToJson(restored.State), "Saved weapon RNG resumes deterministically", results);
                Check(!session.Buy(recruit.Token), "Sold offer cannot be purchased twice", results);
                string itemId = catalog.Items[0].Id;
                for (int index = 0; index < 2; index++)
                {
                    var offer = new ShopOffer { Token = ++session.State.NextOffer, Kind = OfferKind.Item, Id = itemId, Price = 10 };
                    session.State.Offers.Add(offer);
                    Check(session.Buy(offer.Token), "Stacked upgrade purchase " + index, results);
                }
                Check(session.StackCount(itemId) == 2 && session.Bonus(catalog.Items[0].Effect) >= catalog.Items[0].Value * 2, "Items stack", results);
                session.State.Roster.Clear();
                string companionId = catalog.Companions[0].Id;
                session.State.Roster.Add(new RosterEntry { UnitId = companionId, WeaponId = catalog.Weapons[0].Id });
                session.State.Roster.Add(new RosterEntry { UnitId = companionId, WeaponId = catalog.Weapons[1].Id });
                var third = new ShopOffer { Token = ++session.State.NextOffer, Kind = OfferKind.Companion, Id = companionId, Price = 1 };
                session.State.Offers.Add(third);
                Check(session.Buy(third.Token) && session.State.Roster.Count == 1 && session.State.Roster[0].Tier == 2, "Three recruits merge to tier two", results);
                UnitDefinition ranked = session.PrepareCombat().StartingCompanions[0];
                Check(ranked.Weapon.Damage >= catalog.Weapon(session.State.Roster[0].WeaponId).Damage * 3f, "Merged unit has meaningful damage growth without slot cap", results);
                Check(Mathf.Abs(ranked.PassiveStrength - catalog.Companions[0].PassiveStrength * 3f) < 0.001f, "Merge preserves three copies of healing potency", results);
                foreach (string tag in catalog.Companions[0].Tags) Check(session.BondCounts[tag] == 1, "Bond counts bodies not stars: " + tag, results);
                session.Validate(session.State);
                var invalid = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(session.State));
                invalid.Roster.Add(invalid.Roster[0]);
                bool rejected = false;
                try { session.Validate(invalid); } catch (InvalidOperationException) { rejected = true; }
                Check(rejected, "Duplicate identity save rejected", results);

                string root = Path.Combine(".utmp", "FullRunChecks", Guid.NewGuid().ToString("N"));
                var storage = new RunStorage(root);
                storage.SaveRun(session.State);
                int savedGold = session.State.Gold;
                session.State.Gold++;
                storage.SaveRun(session.State);
                Check(storage.LoadRun(session).Gold == savedGold + 1, "Atomic save round trip", results);
                File.WriteAllText(Path.Combine(root, "run.json"), "corrupt diagnostic fixture");
                Check(storage.LoadRun(session).Gold == savedGold && storage.LastError.Length > 0, "Corrupt primary recovers previous backup", results);
                var profile = new PlayerProfile();
                session.State.Phase = SessionPhase.Victory;
                session.State.TotalKills = 7;
                storage.RecordProgress(profile, session.State);
                storage.RecordProgress(profile, session.State);
                Check(profile.Wins == 1 && profile.TotalKills == 7 && storage.LoadProfile().Wins == 1, "Profile terminal credit deduplicated", results);
                int rosterCount = session.State.Roster.Count;
                string runId = session.State.RunId;
                session.EnterEndless();
                Check(session.State.Endless && session.State.Phase == SessionPhase.Shop && session.State.Roster.Count == rosterCount && session.State.RunId != runId, "Endless keeps build with separate score segment", results);
                session.ContinueWave();
                Check(session.State.Phase == SessionPhase.Combat && session.State.Offers.Count == 0, "Next hall starts combat", results);

                session.State.Wave = catalog.BossEvery;
                combat = session.PrepareCombat();
                combat.EnemyBudget = 0;
                combat.WaveDuration = 0.1f;
                simulation = new CombatSimulation(combat);
                simulation.Start(combat.Seed);
                simulation.Step(0.1f, Vector2.zero);
                Check(simulation.Boss != null && simulation.Phase == RunPhase.Combat, "Boss hall cannot clear on timer", results);
                simulation.DamageFixture(simulation.Boss, 100000f);
                simulation.Step(0.1f, Vector2.zero);
                Check(simulation.BossDefeated && simulation.Phase == RunPhase.Victory, "Boss death clears hall", results);
                session.Settle(simulation);
                Check(session.State.BossKills == 1 && session.State.Eggs >= 3, "Boss reward settlement", results);

                session.State.Roster.Clear();
                AltarRecipe recipe = catalog.Recipes[0];
                session.State.Roster.Add(new RosterEntry { UnitId = recipe.FirstUnit, WeaponId = catalog.Weapons[0].Id });
                session.State.Roster.Add(new RosterEntry { UnitId = recipe.SecondUnit, WeaponId = catalog.Weapons[1].Id });
                session.State.Eggs = recipe.EggCost;
                Check(session.Altar(recipe.Id) && session.State.Roster.Count == 1 && session.State.Roster[0].UnitId == recipe.ResultUnit && session.State.Eggs == 0, "Altar consumes exact materials and eggs", results);
                Check(!session.Altar(recipe.Id) && session.State.Roster.Count == 1, "Altar cannot consume missing materials", results);
                Check(RunSession.BoundedCounter((long)int.MaxValue + 10) == int.MaxValue, "Counter additions do not wrap", results);
            }
            CombatChecks(catalog, results);
            ScheduleChecks(catalog, results);
            Benchmark(catalog, results);
            CheckpointChecks.Run(catalog.Arena);
            SquadCheckpointChecks.Run(catalog.Arena);
            File.WriteAllLines("PreviewExports/FullRun_Checks.txt", results);
            Debug.Log("Full run checks PASS: " + results.Count);
        }

        private static void CombatChecks(RunCatalog source, List<string> results)
        {
            using (var session = new RunSession(source))
            {
                session.Start(source.Champions[0].Id, 42);
                CheckpointCatalog catalog = session.PrepareCombat();
                catalog.EnemyBudget = 0;
                var simulation = new CombatSimulation(catalog);
                simulation.Start(42);
                UnitState enemy = simulation.AddFixture(catalog.MeleeEnemy, Faction.Enemy, Vector2.right * 5f);
                simulation.WeaponDamageFixture(enemy, 2f, DamageElement.Ember);
                Check(enemy.BurnUntil > 0, "Ember status applied", results);
                float before = enemy.Health;
                simulation.WeaponDamageFixture(enemy, 2f, DamageElement.Frost);
                Check(enemy.Health == before - 3f && enemy.BurnUntil == 0 && enemy.ChillUntil == 0, "Ember frost reaction consumes statuses once", results);
                simulation.WeaponDamageFixture(enemy, 2f, DamageElement.Frost);
                Check(enemy.ChillUntil > 0, "Frost status applied", results);
                UnitState neighbor = simulation.AddFixture(catalog.MeleeEnemy, Faction.Enemy, Vector2.right * 5.5f);
                before = neighbor.Health;
                simulation.WeaponDamageFixture(enemy, 2f, DamageElement.Arcane);
                Check(Mathf.Abs(neighbor.Health - (before - 0.7f)) < 0.001f, "Arcane chain applies one nonrecursive hit", results);
                simulation.Start(42);
                enemy = simulation.AddFixture(catalog.MeleeEnemy, Faction.Enemy, Vector2.right * 5f);
                neighbor = simulation.AddFixture(catalog.MeleeEnemy, Faction.Enemy, Vector2.right * 5.5f);
                simulation.DamageFixture(enemy, enemy.Health - 1f);
                before = neighbor.Health;
                simulation.WeaponDamageFixture(enemy, 2f, DamageElement.Arcane);
                Check(!enemy.Alive && Mathf.Abs(neighbor.Health - before + 0.7f) < 0.001f, "Lethal arcane hit still chains once", results);

                catalog.CompanionInterceptor = true;
                simulation.Start(42);
                simulation.Champion.NextPassive = float.PositiveInfinity;
                simulation.Units[1].NextPassive = float.PositiveInfinity;
                simulation.Units[1].Position = new Vector2(-0.5f, 0f);
                simulation.ProjectileFixture(new Vector2(-2f, 0), Vector2.right * 40f, 5f, Faction.Enemy);
                simulation.Step(0.1f, Vector2.zero);
                Check(simulation.Champion.Health == catalog.Champion.Health && simulation.Projectiles.Count == 0, "Interceptor absorbs enemy projectile without ally damage", results);
                catalog.Armor = 1f;
                simulation.DamageFixture(simulation.Champion, 10f);
                Check(simulation.Champion.Health == catalog.Champion.Health - 5f, "Armor shares received damage pipeline", results);
                Check(simulation.Events[simulation.Events.Count - 1].Value == 5f, "Feedback reports damage after armor", results);

                session.State.Items.Add(new ItemStack { Id = "royal-key", Count = 2 });
                session.State.Items.Add(new ItemStack { Id = "wandering-map", Count = 2 });
                session.State.Items.Add(new ItemStack { Id = "guardian-canopy", Count = 1 });
                catalog = session.PrepareCombat();
                Check(catalog.Champion.Weapon.Damage > source.Champions[0].Weapon.Damage * 1.44f && catalog.StartingCompanions[0].Weapon.Damage < source.Weapons[1].Damage * 1.44f, "Champion only damage build leaves allies unchanged", results);
                Check(Mathf.Abs(catalog.Champion.Weapon.Damage - source.Champions[0].Weapon.Damage * session.DamageMultiplier(source.Champions[0], 1, false)) < 0.001f, "HUD multiplier matches combat including rally passive", results);
                Check(Mathf.Abs(catalog.CompanionLeash - source.Arena.CompanionLeash - 0.8f) < 0.001f && catalog.CompanionInterceptor, "Stacked roam and interception reach simulation", results);
                session.State.Wave = 5;
                Check(session.PrepareCombat().GoldPerKill == 3 && session.ActiveModifier != null, "Treasury wave modifier", results);
                session.State.Wave = 7;
                Check(session.PrepareCombat().EnemyBudget > 51 && session.ActiveModifier.Name.Contains("Clockstorm"), "Clockstorm wave modifier", results);
                session.State.Wave = 11;
                Check(session.PrepareCombat().PassiveHeal >= 0.25f, "Quiet stacks wave modifier", results);
                session.State.Wave = 4;
                catalog = session.PrepareCombat();
                catalog.EnemyBudget = 0;
                simulation = new CombatSimulation(catalog);
                simulation.Start(42);
                for (int tick = 0; tick < 22; tick++) simulation.Step(0.1f, Vector2.zero);
                Check(simulation.Boss.TelegraphUntil > simulation.Elapsed, "Boss telegraph precedes firing", results);
                for (int tick = 0; tick < 8; tick++) simulation.Step(0.1f, Vector2.zero);
                Check(simulation.Boss.TelegraphUntil == 0 && simulation.Boss.NextBossPattern > simulation.Elapsed, "Boss telegraph commits one pattern then cooldown", results);
                session.State.Wave = 1;
                catalog = session.PrepareCombat();
                catalog.EnemyBudget = 0;
                catalog.StartingCompanions[0].Speed = 0;
                catalog.MeleeEnemy.Speed = 0;
                simulation = new CombatSimulation(catalog);
                simulation.Start(42);
                simulation.Champion.NextAttack = float.PositiveInfinity;
                simulation.Units[1].Position = Vector2.zero;
                enemy = simulation.AddFixture(catalog.MeleeEnemy, Faction.Enemy, Vector2.right * 0.5f);
                for (int tick = 0; tick < 20; tick++) simulation.Step(1f / 60f, Vector2.zero);
                Check(enemy.Health <= catalog.MeleeEnemy.Health - catalog.StartingCompanions[0].Weapon.Damage, "Ranged launch segment hits enemies inside muzzle distance", results);
            }
        }

        private static void ScheduleChecks(RunCatalog catalog, List<string> results)
        {
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 71);
                for (int wave = 1; wave <= catalog.NormalWaves; wave++)
                {
                    CheckpointCatalog combat = session.PrepareCombat();
                    combat.EnemyBudget = 0;
                    combat.WaveDuration = 0.1f;
                    var simulation = new CombatSimulation(combat);
                    simulation.Start(combat.Seed);
                    if (simulation.Boss != null) simulation.DamageFixture(simulation.Boss, 1000000f);
                    simulation.Step(0.1f, Vector2.zero);
                    session.Settle(simulation);
                    Check(session.State.Wave == wave && session.State.Phase == (wave == catalog.NormalWaves ? SessionPhase.Victory : SessionPhase.Shop), "Authored schedule hall " + wave + " (accelerated fixture)", results);
                    if (wave < catalog.NormalWaves) session.ContinueWave();
                }
                Check(session.State.BossKills == catalog.NormalWaves / catalog.BossEvery, "All five scheduled boss rewards", results);
                session.EnterEndless();
                session.ContinueWave();
                Check(session.State.Wave == catalog.NormalWaves + 1 && session.State.Endless && session.PrepareCombat() != null, "Endless hall 21 prepares intact build", results);
            }
        }

        private static void Benchmark(RunCatalog source, List<string> results)
        {
            var report = new List<string> { "Editor simulation only, 600 ticks at 60 Hz; not rendering/mobile FPS. Fixture allies bypass shop identity policy. Champion and enemies have raised HP for sustained combat. Initial 50 contact + 50 ranged enemies." };
            foreach (int count in new[] { 25, 50, 100, 200 })
            {
                using (var session = new RunSession(source))
                {
                    session.Start(source.Champions[0].Id, 92);
                    CheckpointCatalog catalog = session.PrepareCombat();
                    catalog.Champion.Health = 1000000;
                    catalog.MeleeEnemy.Health = catalog.RangedEnemy.Health = 1000000;
                    catalog.EnemyBudget = 0;
                    var simulation = new CombatSimulation(catalog);
                    simulation.Start(92);
                    for (int index = 1; index < count; index++) simulation.AddCompanion(catalog.StartingCompanions[0]);
                    for (int index = 0; index < 100; index++) simulation.AddFixture(index % 2 == 0 ? catalog.MeleeEnemy : catalog.RangedEnemy, Faction.Enemy, new Vector2((index % 20 - 10) * 0.65f, (index / 20 - 2) * 0.7f));
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    for (int tick = 0; tick < 600; tick++) simulation.Step(1f / 60f, new Vector2(Mathf.Cos(tick * 0.02f), Mathf.Sin(tick * 0.02f)));
                    watch.Stop();
                    Check(simulation.CompanionCount == count && simulation.Elapsed > 9.9f && simulation.Units.Count == count + 101, "Uncapped sustained army fixture " + count, results);
                    report.Add(count + " allies / initial 100 enemies: " + (watch.Elapsed.TotalMilliseconds / 600).ToString("0.000") + " ms/step, survivors " + simulation.Units.Count + ", projectiles " + simulation.Projectiles.Count);
                }
            }
            File.WriteAllLines("PreviewExports/FullRun_Simulation_Benchmark.txt", report);
        }

        private static void Check(bool condition, string label, List<string> results)
        {
            if (!condition) throw new InvalidOperationException("Full run check failed: " + label);
            results.Add("PASS " + label);
        }
    }
}
