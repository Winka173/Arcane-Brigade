using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K3Checks
    {
        [MenuItem("Arcane Brigade/K3/Check transactions and build")]
        public static void BuildAndCheck() { Run(); FullRunBuilder.BuildWindows(); }

        public static void Run()
        {
            K2Checks.Run();
            var results = new List<string>();
            Action<bool, string> require = (condition, name) => { if (!condition) throw new InvalidOperationException(name); results.Add("PASS " + name); };
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 131026);
                session.State.Phase = SessionPhase.Shop;
                session.State.Gold = 1000;
                session.State.Eggs = 100;
                session.Reroll();
                session.State.Gold = 1000;
                session.State.RerollsThisShop = 0;
                string initial = JsonUtility.ToJson(session.State);
                bool saved = false;
                require(!RunTransaction.TryCommit(session, catalog, "Refused", () => { session.Reroll(); return false; }, () => { saved = true; return true; }, out _) && !saved && initial == JsonUtility.ToJson(session.State), "False operation rolls back currency, offers, counters and RNG without saving");
                require(!RunTransaction.TryCommit(session, catalog, "Throw", () => { session.Reroll(); throw new IOException("fixture"); }, () => true, out string error) && error.Contains("fixture") && initial == JsonUtility.ToJson(session.State), "Operation exception rolls back exactly");
                require(!RunTransaction.TryCommit(session, catalog, "Save refused", session.Reroll, () => false, out _) && initial == JsonUtility.ToJson(session.State), "Rejected save leaves no receipt or gameplay changes");
                require(!RunTransaction.TryCommit(session, catalog, "Save throws", session.Reroll, () => throw new IOException("write fixture"), out _) && initial == JsonUtility.ToJson(session.State), "Save exception restores both random streams");
                RunSnapshot persisted = null;
                require(RunTransaction.TryCommit(session, catalog, "Reroll", session.Reroll, () => { persisted = Copy(session.State); return true; }, out _), "Successful transaction commits");
                require(persisted.NextReceipt == 1 && persisted.Receipts.Count == 1 && persisted.RerollsThisShop == 1 && persisted.Gold == 1000 - catalog.RerollPrice, "Receipt and reroll counter persist with the purchase boundary");
                require(session.State.Receipts[0].Changes.Exists(change => change.StartsWith("Vàng:")) && session.State.Receipts[0].Changes.Exists(change => change.StartsWith("Reroll tại shop:")), "Receipt records actual wallet and reroll before/after");
                ShopOffer recruit = session.State.Offers.Find(offer => offer.Kind == OfferKind.Companion);
                require(RunTransaction.TryCommit(session, catalog, "Recruit", () => session.Buy(recruit.Token), () => true, out _), "Recruit purchase through atomic pipeline");
                require(session.State.Receipts[1].Changes.Exists(change => change.StartsWith("+ ")), "Recruit receipt includes the rolled weapon and tier");
                string sold = JsonUtility.ToJson(session.State);
                require(!RunTransaction.TryCommit(session, catalog, "Repeat", () => session.Buy(recruit.Token), () => true, out _) && sold == JsonUtility.ToJson(session.State), "Sold offer cannot duplicate receipt, weapon roll or charge");
                ItemDefinition item = catalog.Items[0];
                var eggOffer = new ShopOffer { Token = ++session.State.NextOffer, Kind = OfferKind.Item, Id = item.Id, Price = 2, Currency = ShopCurrency.Eggs };
                session.State.SpecialOffers.Add(eggOffer);
                int gold = session.State.Gold;
                require(RunTransaction.TryCommit(session, catalog, "Egg purchase", () => session.Buy(eggOffer.Token), () => true, out _) && session.State.Gold == gold && session.State.Eggs == 98, "Egg transaction never spends gold");
                require(session.State.Receipts[2].Changes.Exists(change => change.StartsWith("Trứng:")) && session.State.Receipts[2].Changes.Exists(change => change.StartsWith(item.Name + " ×:")), "Egg and item stack deltas use committed values");
                ItemDefinition companionDamage = Array.Find(catalog.Items, candidate => candidate.Effect == ItemEffect.CompanionDamage);
                var damageOffer = new ShopOffer { Token = ++session.State.NextOffer, Kind = OfferKind.Item, Id = companionDamage.Id, Price = 1, Currency = ShopCurrency.Eggs };
                session.State.SpecialOffers.Add(damageOffer);
                require(RunTransaction.TryCommit(session, catalog, "Companion damage", () => session.Buy(damageOffer.Token), () => true, out _) && session.State.Receipts[session.State.Receipts.Count - 1].Changes.Exists(change => change.Contains(" / ") && change.Contains("Damage") && change.Contains(" → ")), "Companion-only upgrade records actual existing weapon stat changes");
                string root = Path.Combine(Application.temporaryCachePath, "ArcaneK3Checks", Guid.NewGuid().ToString("N"));
                var storage = new RunStorage(root);
                storage.SaveRun(session.State);
                string roundtrip = JsonUtility.ToJson(session.State);
                session.Restore(storage.LoadRun(session));
                require(roundtrip == JsonUtility.ToJson(session.State), "Checksum save/resume preserves receipt and RNG");
                RunSnapshot legacy = Copy(session.State);
                legacy.SchemaRevision = 1; legacy.Receipts = null; legacy.NextReceipt = 0; legacy.RerollsThisShop = 0;
                session.Restore(legacy);
                require(session.State.SchemaRevision == RunSnapshot.CurrentSchemaRevision && session.State.Receipts.Count == 0 && session.State.ShopRandom == legacy.ShopRandom && session.State.WeaponRandom == legacy.WeaponRandom && session.State.Roster.Count == legacy.Roster.Count, "Schema one migrates without consuming RNG or dropping roster");
                for (int index = 0; index < 12; index++) require(RunTransaction.TryCommit(session, catalog, "Reroll", session.Reroll, () => true, out _), "History rollover transaction " + index);
                require(session.State.Receipts.Count == 8 && session.State.Receipts[0].Id == 5 && session.State.NextReceipt == 12, "History bounds UI records, not gameplay or roster");
                string readBefore = JsonUtility.ToJson(session.State);
                RunBuildReadModel model = RunBuildReadModel.Capture(session, catalog, session.State.Health);
                require(model.Receipts.Contains("#12") && readBefore == JsonUtility.ToJson(session.State), "Details history reads without changing random streams");
                session.State.Roster.Clear();
                string unitId = catalog.Companions[0].Id;
                session.State.Roster.Add(new RosterEntry { UnitId = unitId, WeaponId = catalog.Weapons[0].Id });
                session.State.Roster.Add(new RosterEntry { UnitId = unitId, WeaponId = catalog.Weapons[1].Id });
                var mergeOffer = new ShopOffer { Token = ++session.State.NextOffer, Kind = OfferKind.Companion, Id = unitId, Price = 1 };
                session.State.Offers.Clear();
                session.State.Offers.Add(mergeOffer);
                session.Restore(session.State);
                require(RunTransaction.TryCommit(session, catalog, "Merge", () => session.Buy(mergeOffer.Token), () => true, out _) && session.State.Roster.Count == 1 && session.State.Roster[0].Tier == 2, "Triple purchase merges into one higher-tier body");
                RunTransactionReceipt merge = session.State.Receipts[session.State.Receipts.Count - 1];
                require(merge.Changes.FindAll(change => change.StartsWith("− ")).Count == 2 && merge.Changes.FindAll(change => change.StartsWith("+ ")).Count == 1, "Merge receipt shows consumed variants and actual rolled result");
                ItemDefinition healthItem = Array.Find(catalog.Items, candidate => candidate.Effect == ItemEffect.Health);
                var healthOffer = new ShopOffer { Token = ++session.State.NextOffer, Kind = OfferKind.Item, Id = healthItem.Id, Price = 1 };
                session.State.Offers.Add(healthOffer);
                require(RunTransaction.TryCommit(session, catalog, "Health", () => session.Buy(healthOffer.Token), () => true, out _), "Health upgrade commits");
                CheckpointCatalog combat = session.PrepareCombat();
                require(Mathf.Approximately(combat.Champion.Health, catalog.Champions[0].Health + session.Bonus(ItemEffect.Health)) && RunBuildReadModel.Capture(session, catalog, session.State.Health).Stats[0].Summary.EndsWith(combat.Champion.Health.ToString("0.0")), "Committed Details maximum HP matches prepared combat");
                AltarRecipe recipe = catalog.Recipes[0];
                session.State.Roster.Clear();
                session.State.Roster.Add(new RosterEntry { UnitId = recipe.FirstUnit, WeaponId = catalog.Weapons[0].Id });
                session.State.Roster.Add(new RosterEntry { UnitId = recipe.SecondUnit, WeaponId = catalog.Weapons[1].Id });
                session.Restore(session.State);
                string altarBefore = JsonUtility.ToJson(session.State);
                require(!RunTransaction.TryCommit(session, catalog, "Altar fail", () => session.Altar(recipe.Id), () => false, out _) && altarBefore == JsonUtility.ToJson(session.State), "Failed altar save restores materials, eggs and result weapon RNG");
                int eggsBefore = session.State.Eggs;
                require(RunTransaction.TryCommit(session, catalog, "Altar", () => session.Altar(recipe.Id), () => true, out _) && session.State.Eggs == eggsBefore - recipe.EggCost && session.State.Roster.Count == 1 && session.State.Roster[0].UnitId == recipe.ResultUnit, "Existing altar recipe commits materials and egg cost together");
                session.State.Phase = SessionPhase.Combat;
                string combatBefore = JsonUtility.ToJson(session.State);
                require(!RunTransaction.TryCommit(session, catalog, "Combat", () => { session.State.Gold--; return true; }, () => true, out _) && combatBefore == JsonUtility.ToJson(session.State), "Shop transaction cannot execute in combat");
            }
            Directory.CreateDirectory("PreviewExports/K3");
            File.WriteAllLines("PreviewExports/K3/K3_Transaction_Checks.txt", results);
            Debug.Log("ARCANE_K3_TRANSACTION_CHECKS_PASSED " + results.Count);
        }

        private static RunSnapshot Copy(RunSnapshot state) => JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(state));
    }
}
