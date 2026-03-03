using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K32AltarChecks
    {
        [MenuItem("Arcane Brigade/K3/Check altar economy and build")]
        public static void BuildAndCheck() { Run(); FullRunBuilder.BuildWindows(); }

        public static void Run()
        {
            K32Checks.Run();
            var results = new List<string>();
            Action<bool, string> require = (condition, name) => { if (!condition) throw new InvalidOperationException(name); results.Add("PASS " + name); };
            RunCatalog source = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            RunCatalog catalog = UnityEngine.Object.Instantiate(source);
            try
            {
                using (var session = new RunSession(catalog))
                using (var restored = new RunSession(catalog))
                {
                    session.Start(catalog.Champions[0].Id, 32133);
                    session.State.Phase = SessionPhase.Shop;
                    session.State.Gold = 100;
                    session.State.Eggs = 8;
                    session.State.Health = 25;
                    var offer = new ShopOffer { Token = ++session.State.NextOffer, Kind = OfferKind.Item, Id = "key-oil", Currency = ShopCurrency.Health, Price = 10 };
                    session.State.SpecialOffers.Add(offer);
                    float unpaidMaximum = session.PrepareCombat().Champion.Health; // maximum HP is compiled (wave scaling, hero rules), not the raw definition
                    string before = JsonUtility.ToJson(session.State);
                    require(!RunTransaction.TryCommit(session, catalog, "HP fail", () => session.Buy(offer.Token), () => false, out _) && before == JsonUtility.ToJson(session.State), "Failed HP save restores health, item, offer, receipt and RNG");
                    require(RunTransaction.TryCommit(session, catalog, "HP purchase", () => session.Buy(offer.Token), () => true, out _) && session.State.Health == 15 && session.State.Gold == 100 && session.State.Eggs == 8, "HP offer spends only current health");
                    require(session.State.Receipts[0].Changes.Exists(change => change == "Current HP: 25 → 15"), "Receipt records exact health payment");
                    float maximum = session.PrepareCombat().Champion.Health;
                    require(maximum >= unpaidMaximum, "HP payment never reduces maximum health (" + unpaidMaximum + " -> " + maximum + ")");
                    var lethal = new ShopOffer { Token = ++session.State.NextOffer, Kind = OfferKind.Item, Id = "key-oil", Currency = ShopCurrency.Health, Price = 15 };
                    session.State.SpecialOffers.Add(lethal);
                    before = JsonUtility.ToJson(session.State);
                    require(!session.CanBuy(lethal) && !session.Buy(lethal.Token) && before == JsonUtility.ToJson(session.State), "HP offer cannot kill Champion or consume RNG");
                    lethal.Price = 14;
                    require(session.CanBuy(lethal) && session.Buy(lethal.Token) && session.State.Health == 1, "HP offer may leave exactly one current HP");
                    session.State.Eggs = int.MaxValue;
                    require(session.AltarActivationChance == 1f, "Egg activation chance clamps safely without overflow");
                    session.State.Eggs = 8;
                    require(Mathf.Abs(session.AltarActivationChance - 0.5f) < 0.0001f, "Authored chance displays base plus eggs contribution");
                    catalog.AltarBaseChance = 0; catalog.AltarChancePerEgg = 0;
                    session.State.Phase = SessionPhase.Victory;
                    session.EnterEndless();
                    require(session.State.SpecialOffers.Count == 0 && session.State.Eggs == 8, "Zero chance does not activate or spend eggs");
                    catalog.AltarBaseChance = 1;
                    session.State.Phase = SessionPhase.Victory;
                    restored.Restore(session.State);
                    session.EnterEndless(); restored.EnterEndless();
                    require(session.State.SpecialOffers.Count == 3 && session.State.ShopRandom == restored.State.ShopRandom && SameOffers(session.State.SpecialOffers, restored.State.SpecialOffers), "Certain activation rolls reproducible offers from saved shop RNG");
                    RunSnapshot snapshot = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(session.State));
                    uint random = snapshot.ShopRandom;
                    restored.Restore(snapshot);
                    require(restored.State.ShopRandom == random && SameOffers(restored.State.SpecialOffers, snapshot.SpecialOffers), "Resume keeps existing altar without a new activation roll");
                    string specialBefore = JsonUtility.ToJson(session.State);
                    require(!RunTransaction.TryCommit(session, catalog, "Reroll fail", session.Reroll, () => false, out _) && specialBefore == JsonUtility.ToJson(session.State), "Failed reroll keeps altar and activation random boundary");
                    string root = Path.Combine(Application.temporaryCachePath, "ArcaneAltarChecks", Guid.NewGuid().ToString("N"));
                    var storage = new RunStorage(root);
                    session.State.SpecialOffers[0].Currency = ShopCurrency.Health;
                    storage.SaveRun(session.State);
                    restored.Restore(storage.LoadRun(restored));
                    require(JsonUtility.ToJson(restored.State) == JsonUtility.ToJson(session.State), "Health currency round-trips through checksum storage");
                }
                string original = JsonUtility.ToJson(source);
                K32AltarBuilder.Apply();
                require(original == JsonUtility.ToJson(source), "Altar authoring migration is idempotent");
            }
            finally { UnityEngine.Object.DestroyImmediate(catalog); }
            Directory.CreateDirectory("PreviewExports/K3/AltarEconomy");
            File.WriteAllLines("PreviewExports/K3/AltarEconomy/Checks.txt", results);
            Debug.Log("ARCANE_K32_ALTAR_CHECKS_PASSED " + results.Count);
        }

        private static bool SameOffers(List<ShopOffer> first, List<ShopOffer> second)
        {
            if (first.Count != second.Count) return false;
            for (int index = 0; index < first.Count; index++) if (JsonUtility.ToJson(first[index]) != JsonUtility.ToJson(second[index])) return false;
            return true;
        }
    }
}
