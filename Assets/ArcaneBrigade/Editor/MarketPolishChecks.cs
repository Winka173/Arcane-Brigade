using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class MarketPolishChecks
    {
        public static void Run()
        {
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            var notes = new List<string>();
            void Require(bool condition, string label)
            {
                if (!condition) throw new InvalidOperationException(label);
                notes.Add("PASS " + label);
            }
            foreach (int wave in new[] { 1, 5, 10, 20, 50 })
                using (var session = new RunSession(catalog))
                {
                    session.Start(catalog.Champions[0].Id, 13077);
                    session.State.Wave = wave; session.State.Phase = SessionPhase.Shop;
                    session.State.Gold = 1000000; session.State.Eggs = 10000;
                    Require(session.StagePriceFactor >= 1 && session.SpecialStagePriceFactor <= session.StagePriceFactor, "Special inflation gentler than gold wave" + wave);
                    for (int roll = 0; roll < 12; roll++)
                    {
                        session.State.Gold = 1000000; Require(session.Reroll(), "Reroll wave" + wave);
                        foreach (ShopOffer offer in session.State.Offers)
                        {
                            bool recruit = offer.Kind == OfferKind.Companion;
                            float basePrice = recruit ? catalog.Unit(offer.Id).Price : catalog.Item(offer.Id).Price * (1 + (int)catalog.Item(offer.Id).Rarity * 0.4f);
                            float discount = session.Bonus(ItemEffect.Discount) + session.Bonus(recruit ? ItemEffect.RecruitDiscount : ItemEffect.RelicDiscount);
                            int expected = Mathf.Max(1, Mathf.CeilToInt(Mathf.CeilToInt(basePrice * session.StagePriceFactor) * (1 - Mathf.Clamp(discount, -1, 0.5f))));
                            Require(offer.Price == expected, "Generated price includes stage, rarity, scoped discount " + wave + "/" + roll + "/" + offer.Id);
                        }
                    }
                    RunSnapshot saved = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(session.State));
                    using (var restored = new RunSession(catalog))
                    {
                        restored.Restore(saved);
                        Require(restored.State.Offers.Select(offer => offer.Price).SequenceEqual(session.State.Offers.Select(offer => offer.Price)), "Resume preserves displayed prices wave" + wave);
                    }
                    session.State.SpecialOffers.Add(new ShopOffer { Token = ++session.State.NextOffer, Id = catalog.Items[0].Id, Kind = OfferKind.Item, Currency = ShopCurrency.Eggs, Price = 1 });
                    for (int roll = 0; roll < 12; roll++)
                    {
                        session.State.Gold = 1000000; Require(session.RerollAltar(), "Special reroll wave" + wave);
                        foreach (ShopOffer offer in session.State.SpecialOffers)
                        {
                            float basePrice = offer.Kind == OfferKind.Companion ? 4 : (catalog.AltarRevision > 0 ? catalog.Item(offer.Id).AltarPrice : catalog.SpecialShopPrice) * (1 + (int)catalog.Item(offer.Id).Rarity * 0.25f);
                            Require(offer.Price == Mathf.Max(1, Mathf.CeilToInt(basePrice * session.SpecialStagePriceFactor)), "Generated special price includes stage " + wave + "/" + roll + "/" + offer.Id);
                        }
                    }
                }
            Require(MarketMotion.StockArrivalDelay + 2 * MarketMotion.StockStagger + 0.55f <= MarketMotion.ArrivalDuration, "Interaction waits for last stock landing");
            Require(MarketMotion.MerchantDepartureDelay + 0.8f < MarketMotion.DepartureDuration, "Wave waits for merchant departure");
            Require(MarketMotion.CarpetDepartureDelay + 0.75f < MarketMotion.DepartureDuration, "Wave waits for rolled carpet");
            Require(MarketMotion.HandoffDuration > MarketMotion.DepartureDuration, "Dream-to-gold handoff leaves a quiet beat");
            Require(MarketMotion.Progress(0.1f, 0.7f, 0.55f) == 0 && MarketMotion.Progress(2, 0.7f, 0.55f) == 1, "Delayed stock progress bounded");
            Require(MarketMotion.Settle(0.2f, 0.8f, 0.1f) == 0 && MarketMotion.Settle(2, 0.8f, 0.1f) == 0, "Landing overshoot finite and not premature");
            Directory.CreateDirectory("PreviewExports/K12");
            File.WriteAllLines("PreviewExports/K12/Market_Polish_Checks.txt", notes);
            Debug.Log("MARKET_POLISH_CHECKS_PASSED " + notes.Count);
        }
    }
}
