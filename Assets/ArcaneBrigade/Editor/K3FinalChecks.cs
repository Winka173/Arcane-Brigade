using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K3FinalChecks
    {
        [MenuItem("Arcane Brigade/K3/Check final checkpoint and build")]
        public static void BuildAndCheck() { Run(); FullRunBuilder.BuildWindows(); }
        public static void Run()
        {
            K32AltarChecks.Run();
            var results = new List<string>();
            Action<bool, string> require = (condition, name) => { if (!condition) throw new InvalidOperationException(name); results.Add("PASS " + name); };
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 13103);
                session.State.Items.Add(new ItemStack { Id = "coin-salve", Count = 2 });
                session.State.Items.Add(new ItemStack { Id = "chest-thunder", Count = 1 });
                CheckpointCatalog combat = session.PrepareCombat();
                combat.EnemyBudget = 0; combat.EggDropChance = combat.HeartDropChance = 0; combat.ChestInterval = 1000; combat.PassiveHeal = 0;
                var simulation = new CombatSimulation(combat); simulation.Start(13103);
                foreach (UnitState unit in simulation.Units) unit.NextPassive = float.PositiveInfinity;
                simulation.Champion.Health = 30;
                simulation.LootFixture(LootKind.Coin, Vector2.zero, 3);
                simulation.LootFixture(LootKind.Egg, Vector2.zero, 2);
                simulation.LootFixture(LootKind.Heart, Vector2.zero, 8);
                simulation.Pause(true); Tick(simulation, 60);
                require(simulation.Gold == 0 && simulation.Eggs == 0 && simulation.Champion.Health == 30 && simulation.Pickups.Count == 3, "Pause freezes every loot type and proc");
                simulation.Pause(false); Tick(simulation, 120);
                require(simulation.Gold == 3 && simulation.Eggs == 2 && simulation.Champion.Health == 39 && simulation.Pickups.Count == 0, "Coin egg heart credit once; only coin triggers stack healing");
                foreach (UnitState unit in simulation.Units) { unit.NextAttack = float.PositiveInfinity; unit.PendingAttack = false; }
                UnitState enemy = simulation.AddFixture(combat.MeleeEnemy, Faction.Enemy, new Vector2(1, 0));
                enemy.Health = 5; enemy.NextAttack = float.PositiveInfinity;
                enemy.Definition.Speed = 0;
                simulation.LootFixture(LootKind.Chest, Vector2.zero, 11);
                int chestEvents = 0; int pulseKills = 0;
                for (int tick = 0; tick < 60; tick++)
                {
                    simulation.Step(1f / 60, Vector2.zero);
                    foreach (CombatEvent value in simulation.Events)
                    {
                        if (value.Kind == CombatEventKind.ChestOpened) chestEvents++;
                        if (value.Kind == CombatEventKind.Kill && value.SubjectId == enemy.Id && value.Cause.ItemId == "chest-thunder" && value.Cause.Depth == 1 && value.Cause.RootId > 0) pulseKills++;
                    }
                }
                require(simulation.ChestsOpened == 1 && chestEvents == 1 && pulseKills == 1, "Real chest opens once with attributed proc kill");
                require(simulation.Gold >= 14, "Chest gold credits through collecting real reward drops");
                combat.Procs = new[] { new ActiveProc { Stacks = 1, Rule = new ProcRule { Id = "kill-fixture", ItemId = "chest-thunder", Trigger = ProcTrigger.EnemyKilled, Action = ProcAction.DamagePulse, Power = 100, Radius = 5, Cooldown = 1 } } };
                simulation.Start(313);
                UnitState first = simulation.AddFixture(combat.MeleeEnemy, Faction.Enemy, Vector2.right);
                UnitState second = simulation.AddFixture(combat.MeleeEnemy, Faction.Enemy, Vector2.right * 2);
                first.Health = second.Health = 1;
                simulation.WeaponDamageFixture(first, 100, DamageElement.Physical); simulation.ProcessProcFixture();
                int activations = 0;
                foreach (CombatEvent value in simulation.Events) if (value.Kind == CombatEventKind.Proc) activations++;
                require(!second.Alive && activations == 1 && simulation.Events.Count < 20, "Proc kills cannot recurse into another proc");
                combat.Procs = new[] { new ActiveProc { Stacks = 1, Rule = new ProcRule { Id = "coin-cd-fixture", ItemId = "coin-salve", Trigger = ProcTrigger.CoinCollected, Action = ProcAction.HealChampion, Power = 1, Cooldown = 1 } } };
                simulation.Start(414); simulation.Champion.Health = 20;
                foreach (UnitState unit in simulation.Units) unit.NextPassive = float.PositiveInfinity;
                simulation.LootFixture(LootKind.Coin, Vector2.zero, 1); simulation.LootFixture(LootKind.Coin, Vector2.zero, 1); Tick(simulation, 60);
                require(simulation.Champion.Health == 21 && simulation.Gold == 2, "Proc cooldown prevents duplicate same-tick activation without losing coin credit");
                combat.Procs[0].Rule.Chance = 0;
                simulation.Start(414); simulation.Champion.Health = 20;
                foreach (UnitState unit in simulation.Units) unit.NextPassive = float.PositiveInfinity;
                simulation.LootFixture(LootKind.Coin, Vector2.zero, 1); Tick(simulation, 60);
                require(simulation.Champion.Health == 20 && simulation.Gold == 1, "Zero chance proc never activates");
                combat.Procs = new ActiveProc[0]; simulation.Start(13103);
                simulation.LootFixture(LootKind.Chest, new Vector2(10, 0), 10); Tick(simulation, 60);
                require(simulation.ChestsOpened == 0 && simulation.Pickups.Count == 1 && simulation.Pickups[0].Position == new Vector2(10, 0), "Offscreen chest persists without auto attraction");
                Rect world = new Rect(-5, -3, 10, 6); Rect safe = new Rect(140, 205, 1640, 650);
                simulation.LootFixture(LootKind.Egg, new Vector2(10, 0), 1); simulation.LootFixture(LootKind.Egg, new Vector2(11, 0), 1);
                simulation.LootFixture(LootKind.Coin, new Vector2(-10, 0), 1); simulation.LootFixture(LootKind.Heart, new Vector2(0, 9), 8);
                List<LootIndicators.Marker> markers = LootIndicators.Layout(simulation.Pickups, world, safe);
                require(markers.Count == 2 && markers[0].Kind == LootKind.Chest && markers[1].Count == 2, "Markers group eggs, prioritize chest and exclude coin/heart");
                foreach (LootIndicators.Marker marker in markers) require(marker.Position.x >= safe.xMin && marker.Position.x <= safe.xMax && marker.Position.y >= safe.yMin && marker.Position.y <= safe.yMax, "Safe marker layout " + marker.Kind);
                require(LootIndicators.Layout(simulation.Pickups, new Rect(-17, -11, 34, 22), safe).Count == 0, "Visible loot produces no offscreen marker");
                simulation.Champion.Position = simulation.Champion.PreviousPosition = new Vector2(10, 0); Tick(simulation, 90);
                require(simulation.ChestsOpened == 1 && LootIndicators.Layout(simulation.Pickups, world, safe).FindAll(marker => marker.Kind == LootKind.Chest).Count == 0, "Opened chest removes its marker target");
                session.State.Phase = SessionPhase.Shop; session.State.Gold = 1000;
                var offer = new ShopOffer { Token = ++session.State.NextOffer, Kind = OfferKind.Item, Id = "altar-ember", Price = 10, Currency = ShopCurrency.Health };
                session.State.SpecialOffers.Add(offer);
                BeforeAltar(session, catalog, require);
                string before = JsonUtility.ToJson(session.State); string preview = RunOfferPreview.Capture(session, catalog, offer);
                require(before == JsonUtility.ToJson(session.State) && preview.Contains("→") && preview.Contains("Champion health"), "HP stat preview reads real data without consuming live RNG");
                require(RunTransaction.TryCommit(session, catalog, "Exclusive", () => session.Buy(offer.Token), () => true, out _), "Altar-exclusive multi-modifier item commits");
                combat = session.PrepareCombat(); RunBuildReadModel read = RunBuildReadModel.Capture(session, catalog, session.State.Health);
                require(read.Stats.Find(row => row.Name == "Champion damage").Summary == combat.Champion.Weapon.Damage.ToString("0.0") && read.Stats.Find(row => row.Name == "Movement speed").Summary == combat.Champion.Speed.ToString("0.00"), "Committed UI matches prepared combat");
                string root = Path.Combine(Application.temporaryCachePath, "ArcaneK3Final", Guid.NewGuid().ToString("N"));
                var storage = new RunStorage(root); storage.SaveRun(session.State);
                using (var loaded = new RunSession(catalog))
                {
                    loaded.Restore(storage.LoadRun(loaded));
                    require(loaded.PrepareCombat().Procs.Length == 2 && loaded.StackCount("altar-ember") == 1, "Resume rebuilds procs from stable item IDs");
                }
                require(catalog.EggSprite != null && catalog.HeartSprite != null && catalog.ChestSprite != null, "Distinct loot art assets persist in source");
                AltarRecipe recipe = catalog.Recipes[0];
                session.State.Eggs = recipe.EggCost; session.State.Roster.Clear();
                session.State.Roster.Add(new RosterEntry { UnitId = recipe.FirstUnit, WeaponId = catalog.Weapons[0].Id });
                session.State.Roster.Add(new RosterEntry { UnitId = recipe.SecondUnit, WeaponId = catalog.Weapons[1].Id });
                session.Restore(session.State);
                before = JsonUtility.ToJson(session.State);
                require(session.CanAltar(recipe.Id) && session.RecipePreview(recipe.Id).Contains("READY") && before == JsonUtility.ToJson(session.State), "Recipe material/tier preview is read-only including both RNG streams");
                require(RunTransaction.TryCommit(session, catalog, "Recipe gate", () => session.Altar(recipe.Id), () => true, out _) && session.State.Roster.Count == 1 && session.State.Roster[0].Tier == recipe.ResultTier, "Recipe commits authored tier and actual consumed materials");
                combat = session.PrepareCombat(); combat.EnemyBudget = 0; combat.ChestInterval = 1000; combat.Procs = new ActiveProc[0];
                simulation = new CombatSimulation(combat); simulation.Start(510);
                simulation.LootFixture(LootKind.Egg, Vector2.zero, 3); Tick(simulation, 60);
                int eggsBefore = session.State.Eggs;
                session.State.Phase = SessionPhase.Combat;
                for (int tick = 0; tick < 5200 && simulation.Phase == RunPhase.Combat; tick++) simulation.Step(1f / 60, Vector2.zero);
                session.Settle(simulation);
                require(session.State.Eggs == eggsBefore + 4, "Real egg pickup plus authored hall reward settle exactly once");
                int settledEggs = session.State.Eggs; session.Settle(simulation);
                require(session.State.Eggs == settledEggs, "Repeated settle cannot duplicate eggs");
                var marked = new List<PickupState>();
                for (int index = 0; index < 16; index++) marked.Add(new PickupState { Kind = index % 2 == 0 ? LootKind.Chest : LootKind.Egg, Position = new Vector2(Mathf.Cos(index * Mathf.PI / 8), Mathf.Sin(index * Mathf.PI / 8)) * 20 });
                List<LootIndicators.Marker> capped = LootIndicators.Layout(marked, world, safe);
                require(capped.Count == 6 && capped[0].Kind == LootKind.Chest, "Indicator count is capped with chest priority");
                CheckpointCatalog replayCatalog = session.PrepareCombat(); replayCatalog.EnemyBudget = 0; replayCatalog.Boss = null; replayCatalog.ChestInterval = 4; replayCatalog.Procs = new ActiveProc[0];
                var replay = new CombatSimulation(replayCatalog); simulation = new CombatSimulation(replayCatalog);
                replay.Start(991); simulation.Start(991); Tick(replay, 600); Tick(simulation, 600);
                require(LootSignature(replay) == LootSignature(simulation) && replay.Pickups.Count > 0, "Seeded treasure RNG reproduces actual offscreen chest positions and values");
                replayCatalog.TreasureDetection = 2; replay.Start(991); Tick(replay, 600);
                require(replay.Pickups.Count >= simulation.Pickups.Count, "Treasure Detection accelerates actual chest creation");
                string authoring = JsonUtility.ToJson(catalog); K3FinalBuilder.Apply();
                require(authoring == JsonUtility.ToJson(catalog), "Final authoring migration does not duplicate items or overwrite tuning");
                replayCatalog.Champion.CriticalChance = 0.5f;
                replayCatalog.EnemyBudget = 0; replayCatalog.ChestInterval = 1000;
                replay.Start(889); simulation.Start(889);
                foreach (UnitState unit in replay.Units) if (unit.Faction == Faction.Companion) unit.NextAttack = float.PositiveInfinity;
                foreach (UnitState unit in simulation.Units) if (unit.Faction == Faction.Companion) unit.NextAttack = float.PositiveInfinity;
                UnitState targetFirst = replay.AddFixture(replayCatalog.MeleeEnemy, Faction.Enemy, Vector2.right * 0.8f);
                UnitState targetSecond = simulation.AddFixture(replayCatalog.MeleeEnemy, Faction.Enemy, Vector2.right * 0.8f);
                targetFirst.Health = targetSecond.Health = 100000; targetFirst.NextAttack = targetSecond.NextAttack = float.PositiveInfinity;
                targetFirst.NextContact = targetSecond.NextContact = float.PositiveInfinity;
                targetFirst.Definition.Speed = 0;
                replay.LootFixture(LootKind.Chest, Vector2.zero, 7);
                string criticalFirst = ""; string criticalSecond = ""; bool attribution = false;
                for (int tick = 0; tick < 240; tick++)
                {
                    replay.Step(1f / 60, Vector2.zero); simulation.Step(1f / 60, Vector2.zero);
                    foreach (CombatEvent value in replay.Events) if (value.Kind == CombatEventKind.Fire && value.Faction == Faction.Champion) { criticalFirst += value.Critical; attribution |= value.Cause.SourceUnitId == replay.Champion.Id && value.Cause.RootId > 0; }
                    foreach (CombatEvent value in simulation.Events) if (value.Kind == CombatEventKind.Fire && value.Faction == Faction.Champion) criticalSecond += value.Critical;
                }
                require(criticalFirst.Length > 0 && criticalFirst == criticalSecond && replay.ChestsOpened == 1, "Chest reward RNG consumption never changes combat critical rolls");
                require(attribution, "Real attacks expose stable source unit and root cause IDs");
            }
            Directory.CreateDirectory("PreviewExports/K3/Final"); File.WriteAllLines("PreviewExports/K3/Final/Checks.txt", results);
            Debug.Log("ARCANE_K3_FINAL_CHECKS_PASSED " + results.Count);
        }
        private static void Tick(CombatSimulation simulation, int count) { for (int tick = 0; tick < count; tick++) simulation.Step(1f / 60, Vector2.zero); }
        private static string LootSignature(CombatSimulation simulation)
        {
            string result = "";
            foreach (PickupState loot in simulation.Pickups) result += loot.Id + "/" + loot.Kind + "/" + loot.Position + "/" + loot.Value + ";";
            return result;
        }
        private static void BeforeAltar(RunSession session, RunCatalog catalog, Action<bool, string> require)
        {
            string before = JsonUtility.ToJson(session.State);
            require(!RunTransaction.TryCommit(session, catalog, "Altar reroll fail", session.RerollAltar, () => false, out _) && before == JsonUtility.ToJson(session.State), "Failed altar reroll restores stock, counter and both RNG streams");
            using (var fixture = new RunSession(catalog))
            {
                fixture.Restore(session.State);
                fixture.Reroll();
                int gold = fixture.State.Gold; int eggs = fixture.State.Eggs; float hp = fixture.State.Health; int price = fixture.RerollCost;
                string normalStock = "";
                foreach (ShopOffer offer in fixture.State.Offers) normalStock += JsonUtility.ToJson(offer);
                require(RunTransaction.TryCommit(fixture, catalog, "Altar reroll", fixture.RerollAltar, () => true, out _) && fixture.State.Gold == gold - price && fixture.State.Eggs == eggs && fixture.State.Health == hp && fixture.State.SpecialOffers.Count == 3, "Altar reroll spends gold only and creates three currency-aware offers");
                string afterStock = "";
                foreach (ShopOffer offer in fixture.State.Offers) afterStock += JsonUtility.ToJson(offer);
                require(normalStock == afterStock, "Altar reroll never refreshes gold stock or rolls activation");
            }
        }
    }
}
