using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K32Checks
    {
        [MenuItem("Arcane Brigade/K3/Check modifier economy and build")]
        public static void BuildAndCheck() { Run(); FullRunBuilder.BuildWindows(); }

        public static void Run()
        {
            K3Checks.Run();
            var results = new List<string>();
            Action<bool, string> require = (condition, name) => { if (!condition) throw new InvalidOperationException(name); results.Add("PASS " + name); };
            RunCatalog source = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            require(source.EconomyRevision == 1 && source.RerollPriceStep == 2, "Authored economy migration is applied");
            string sourceBefore = JsonUtility.ToJson(source);
            K32Builder.Apply();
            require(sourceBefore == JsonUtility.ToJson(source), "Apply is idempotent and preserves authored tuning");
            RunCatalog catalog = UnityEngine.Object.Instantiate(source);
            catalog.AltarRevision = 0;
            try
            {
                using (var session = new RunSession(catalog))
                using (var restored = new RunSession(catalog))
                {
                    session.Start(catalog.Champions[0].Id, 32132);
                    session.State.Phase = SessionPhase.Shop;
                    session.State.Gold = 1000;
                    int first = session.RerollCost;
                    require(session.Reroll() && session.State.Gold == 1000 - first && session.RerollCost == first + 2, "Reroll pays displayed price and increases next price");
                    restored.Restore(session.State);
                    require(session.Reroll() && restored.Reroll() && JsonUtility.ToJson(session.State) == JsonUtility.ToJson(restored.State), "Weighted shop and reroll counter resume deterministically");
                    string before = JsonUtility.ToJson(session.State);
                    require(!RunTransaction.TryCommit(session, catalog, "Weighted fail", session.Reroll, () => false, out _) && before == JsonUtility.ToJson(session.State), "Failed weighted reroll restores price, pools, receipts and RNG");
                    session.State.Gold = session.RerollCost - 1;
                    before = JsonUtility.ToJson(session.State);
                    require(!session.Reroll() && before == JsonUtility.ToJson(session.State), "Insufficient reroll currency does not consume RNG or counter");
                    session.State.RerollsThisShop = int.MaxValue;
                    require(session.RerollCost == int.MaxValue, "Reroll pricing is overflow bounded");
                    session.State.RerollsThisShop = 0;
                    session.State.Gold = 1000;
                    ItemDefinition item = catalog.Items[0];
                    item.Modifiers = new[]
                    {
                        new StatModifier { Effect = ItemEffect.Damage, Scope = ModifierScope.Champion, Value = 0.3f },
                        new StatModifier { Effect = ItemEffect.AttackSpeed, Scope = ModifierScope.Companions, Value = 0.2f },
                        new StatModifier { Effect = ItemEffect.MoveSpeed, Scope = ModifierScope.Team, Value = -0.05f },
                        new StatModifier { Effect = ItemEffect.Health, Scope = ModifierScope.Champion, Value = 10 }
                    };
                    float health = session.State.Health;
                    var offer = new ShopOffer { Token = ++session.State.NextOffer, Kind = OfferKind.Item, Id = item.Id, Price = 1 };
                    session.State.Offers.Add(offer);
                    require(session.Buy(offer.Token) && session.State.Health == health + 10, "Multi-modifier purchase updates actual health once");
                    require(Mathf.Approximately(session.Bonus(ItemEffect.Damage, ModifierScope.Champion), 0.3f) && session.Bonus(ItemEffect.Damage, ModifierScope.Companions) == 0, "Champion scoped damage never leaks to allies");
                    require(session.Bonus(ItemEffect.AttackSpeed, ModifierScope.Champion) == 0 && Mathf.Approximately(session.Bonus(ItemEffect.AttackSpeed, ModifierScope.Companions), 0.2f), "Companion scoped attack speed never leaks to Champion");
                    CheckpointCatalog combat = session.PrepareCombat();
                    RunBuildReadModel model = RunBuildReadModel.Capture(session, catalog, session.State.Health);
                    UnitDefinition champion = catalog.Champions[0];
                    RosterEntry entry = session.State.Roster[0];
                    require(Mathf.Approximately(combat.Champion.Weapon.Damage, champion.Weapon.Damage * session.DamageMultiplier(champion, 1, false)), "Combat uses scoped Champion damage");
                    require(combat.StartingCompanions[0].Health == catalog.Unit(entry.UnitId).Health, "Champion health modifiers do not change immortal ally definitions");
                    require(model.Stats[2].Summary == combat.Champion.Weapon.Cooldown.ToString("0.00") + " s" && model.Companions[0].CombatValues["Chu kỳ"] == combat.StartingCompanions[0].Weapon.Cooldown.ToString("0.###") + " s", "Details attack cycles equal combat for both scopes");
                    require(model.Stats[3].Summary == combat.Champion.Speed.ToString("0.00") && model.Companions[0].CombatValues["Speed"] == combat.StartingCompanions[0].Speed.ToString("0.###"), "Negative movement tradeoff matches Details and combat");
                    require(model.Relics[0].Explanation.Contains("-5%") && model.Relics[0].Explanation.Contains("Champion") && model.Relics[0].Explanation.Contains("Đồng minh"), "Relic explanation exposes all positive and negative scopes");
                    item.Stacking = StackPolicy.Unique;
                    offer = new ShopOffer { Token = ++session.State.NextOffer, Kind = OfferKind.Item, Id = item.Id, Price = 1 };
                    session.State.Offers.Add(offer);
                    before = JsonUtility.ToJson(session.State);
                    require(!session.CanBuy(offer) && !session.Buy(offer.Token) && before == JsonUtility.ToJson(session.State), "Unique policy blocks redundant purchase without spending");
                    session.State.Items[0].Count = 4;
                    require(Mathf.Approximately(session.Bonus(ItemEffect.Damage, ModifierScope.Champion), 0.3f), "Legacy excess stacks retain ownership but obey effective unique policy");
                    item.Stacking = StackPolicy.Additive;
                    item.MaximumStacks = 2;
                    require(Mathf.Approximately(session.Bonus(ItemEffect.Damage, ModifierScope.Champion), 0.6f), "Capped additive policy limits effective stacks");
                    session.State.Items.Clear();
                    item.Modifiers = new StatModifier[0];
                    session.State.Items.Add(new ItemStack { Id = item.Id, Count = 2 });
                    require(Mathf.Approximately(session.Bonus(item.Effect), item.Value * 2), "Empty modifier list falls back to legacy Effect and Value");
                }
                using (var weighted = new RunSession(catalog))
                {
                    weighted.Start(catalog.Champions[0].Id, 132);
                    weighted.State.Phase = SessionPhase.Shop;
                    weighted.State.Gold = 100000;
                    foreach (ItemDefinition item in catalog.Items) item.ShopWeight = 0;
                    ItemDefinition only = catalog.Items[1];
                    only.ShopWeight = 7; only.Pools = ShopPool.Gold; only.UnlockWave = 0;
                    require(weighted.Reroll() && weighted.State.Offers.FindAll(offer => offer.Kind == OfferKind.Item && offer.Id == only.Id).Count == 2, "Zero-weight and wrong-pool items cannot appear");
                    only.UnlockWave = 99;
                    require(weighted.Reroll() && weighted.State.Offers.TrueForAll(offer => offer.Kind == OfferKind.Companion), "Locked weighted items fall back to eligible recruits");
                    only.UnlockWave = 0; only.MaximumStacks = 1;
                    weighted.State.Items.Add(new ItemStack { Id = only.Id, Count = 1 });
                    require(weighted.Reroll() && weighted.State.Offers.TrueForAll(offer => offer.Kind == OfferKind.Companion), "Capped items leave the future weighted pool");
                    weighted.State.Items.Clear();
                    foreach (UnitDefinition unit in catalog.Companions)
                        foreach (WeaponDefinition weapon in catalog.Weapons) weighted.State.Roster.Add(new RosterEntry { UnitId = unit.Id, WeaponId = weapon.Id });
                    weighted.State.Roster.RemoveAt(0);
                    require(weighted.Reroll() && weighted.State.Offers.TrueForAll(offer => offer.Kind == OfferKind.Item), "Weapon-exhausted recruit pool falls back to items");
                    weighted.State.Roster.Clear();
                    ItemDefinition minority = catalog.Items[0];
                    minority.ShopWeight = 1; minority.Pools = ShopPool.Gold; minority.UnlockWave = 0;
                    only.ShopWeight = 9; only.MaximumStacks = 100000;
                    int minorityOffers = 0;
                    for (int sample = 0; sample < 300; sample++)
                    {
                        weighted.State.Gold = 100000;
                        if (!weighted.Reroll()) throw new InvalidOperationException("Weighted distribution roll failed " + sample);
                        minorityOffers += weighted.State.Offers.FindAll(offer => offer.Kind == OfferKind.Item && offer.Id == minority.Id).Count;
                    }
                    require(minorityOffers >= 25 && minorityOffers <= 100, "One-to-nine authored weights produce a minority rather than uniform sampling");
                    ItemDefinition eggOnly = catalog.Items[2];
                    eggOnly.ShopWeight = 5; eggOnly.Pools = ShopPool.Eggs; eggOnly.UnlockWave = 0;
                    weighted.State.Wave = 2;
                    weighted.State.Phase = SessionPhase.Victory;
                    weighted.EnterEndless();
                    require(weighted.State.RerollsThisShop == 0 && weighted.RerollCost == catalog.RerollPrice, "New rest boundary resets reroll escalation");
                    require(weighted.State.SpecialOffers.Count == 3 && weighted.State.SpecialOffers.TrueForAll(offer => offer.Id == eggOnly.Id && offer.Currency == ShopCurrency.Eggs) && !weighted.State.Offers.Exists(offer => offer.Id == eggOnly.Id), "Egg-only weighted pool is separate from gold offers");
                    weighted.State.Items.Add(new ItemStack { Id = "key-oil", Count = 1 });
                    weighted.State.Items.Add(new ItemStack { Id = "clock-spring", Count = 1 });
                    string before = JsonUtility.ToJson(weighted.State);
                    RunBuildReadModel links = RunBuildReadModel.Capture(weighted, catalog, weighted.State.Health);
                    require(links.Links[0].Summary.Contains("2/2") && links.Links[1].Summary.Contains("0/2") && before == JsonUtility.ToJson(weighted.State), "Build links derive progress from owned IDs without consuming RNG");
                    weighted.State.Items.Add(new ItemStack { Id = "coupon-book", Count = 50 });
                    weighted.State.Gold = 100000;
                    weighted.Reroll();
                    require(weighted.State.Offers.TrueForAll(offer => offer.Kind == OfferKind.Item ? offer.Price == Mathf.Max(1, Mathf.CeilToInt(catalog.Item(offer.Id).Price * 0.5f)) : offer.Price == Mathf.Max(1, Mathf.CeilToInt(catalog.Unit(offer.Id).Price * 0.5f))), "Future offer discounts clamp at fifty percent");
                }
                catalog.Items[0].Modifiers = new[] { new StatModifier { Effect = ItemEffect.Armor, Scope = ModifierScope.Companions, Value = 1 } };
                bool rejected = false;
                try { RunModifierRules.Validate(catalog); } catch (InvalidOperationException) { rejected = true; }
                require(rejected, "Unsupported scope rejects authoring instead of advertising a fake effect");
            }
            finally { UnityEngine.Object.DestroyImmediate(catalog); }
            Directory.CreateDirectory("PreviewExports/K3/ModifierEconomy");
            File.WriteAllLines("PreviewExports/K3/ModifierEconomy/Checks.txt", results);
            Debug.Log("ARCANE_K32_CHECKS_PASSED " + results.Count);
        }
    }
}
