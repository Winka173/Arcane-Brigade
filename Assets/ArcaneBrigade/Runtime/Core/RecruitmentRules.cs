using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class RunSession
    {
        public int NewRecruitLimit => 1 + KitNewRecruits;
        public int CompanionLimit => int.MaxValue;
        // Regular shop pacing (owner, 19/10/2026): one NEW species every two waves, and one more body of a species you already
        // have each wave. The dream shop keeps its own rules (one new species per wave, sharing the old wave-10 cadence).
        public const int CopyRecruitLimit = 1;
        public const int CompanionPurchaseLimit = 2; // the most regular companion purchases in one wave: one new + one copy
        public int CopyRecruitsRemaining => Math.Max(0, CopyRecruitLimit - (State.NewRecruitsThisWave - State.NewSpeciesRecruitsThisWave));
        public int CompanionPurchasesRemaining => NewRecruitsRemaining + CopyRecruitsRemaining;
        public bool NewSpeciesCooldown => State.Wave>=10 && State.LastNewShopSpeciesWave>=10 && State.Wave-State.LastNewShopSpeciesWave<2;
        /// <summary>The regular shop's new species rests the wave after one is recruited.</summary>
        /// <summary>Play-test 20/10/2026 (D12): one new species every three waves (was two); no body cap.</summary>
        public const int NewSpeciesEvery = 3;
        public bool RegularNewSpeciesCooldown => State.LastRegularNewSpeciesWave >= 0 && State.LastRegularNewSpeciesWave < State.Wave && State.Wave - State.LastRegularNewSpeciesWave < NewSpeciesEvery;
        public int NewRecruitsRemaining => RegularNewSpeciesCooldown ? 0 : Math.Max(0, NewRecruitLimit - State.NewSpeciesRecruitsThisWave);
        /// <summary>Whether the regular shop could still sell this species this wave (a copy of one you have, or a new one).</summary>
        public bool RegularRecruitOpen(string id) => OwnsCompanionSpecies(id) ? CopyRecruitsRemaining > 0 : NewRecruitsRemaining > 0;
        public int SpecialNewRecruitsRemaining => NewSpeciesCooldown?0:Math.Max(0,1-State.SpecialNewSpeciesRecruitsThisWave);
        public bool OwnsCompanionSpecies(string id) => State.Roster.Exists(r => r.UnitId == id) || (State.AbsorbedSkills?.Contains(id) ?? false);
        // Source shop matters, not currency or the companion's catalog rarity.
        private bool CountsTowardRecruitLimit(ShopOffer offer) => offer != null && offer.Kind == OfferKind.Companion && !State.SpecialOffers.Contains(offer);
        private bool CountsAsNewRegularRecruit(ShopOffer offer) => offer != null && offer.Kind == OfferKind.Companion &&
            !State.SpecialOffers.Contains(offer) && !OwnsCompanionSpecies(offer.Id);
        public bool RecruitQuotaBlocks(ShopOffer offer) => offer!=null && offer.Kind==OfferKind.Companion &&
            (State.SpecialOffers.Contains(offer) ? !OwnsCompanionSpecies(offer.Id) && SpecialNewRecruitsRemaining==0 : !RegularRecruitOpen(offer.Id));
        private void RegisterNewShopSpecies(bool special)
        {
            if(special)State.SpecialNewSpeciesRecruitsThisWave++;else { State.NewSpeciesRecruitsThisWave++; State.LastRegularNewSpeciesWave=State.Wave; }
            if(State.Wave>=10)State.LastNewShopSpeciesWave=State.Wave;
        }
        /// <summary>A species the regular shelf may show at all (before the quotas): ordinary, unlocked, fits the hero, and a
        /// weapon variant is still free for it.</summary>
        private bool RegularShelfSpecies(UnitDefinition unit) => !unit.SpecialRecruit && !StorySpecies(unit.Id) && unit.UnlockWave <= State.Wave &&
            (catalog.HeroRevision == 0 || K11HeroRules.Eligible(catalog.Unit(State.ChampionId), unit)) &&
            Array.Exists(catalog.Weapons, weapon => RecruitWeaponEligible(unit.Id, weapon) && !State.Roster.Exists(entry => entry.UnitId == unit.Id && entry.WeaponId == weapon.Id));
        /// <summary>Owner 22/10/2026: the shop never sells what can't be bought. Rolls already leave new species out while the
        /// new-species quota is closed; a new species still on the regular shelf when the quota closes in this shop (another new
        /// friend bought, a gift relic, an undone refresh) is replaced at once, in its place, by a copy of a species you own (when
        /// the copy quota is open and no copy is on the shelf) or a relic. The dream shop keeps its own rule.</summary>
        private void RetireClosedNewSpecies()
        {
            if (State.Phase != SessionPhase.Shop || NewRecruitsRemaining > 0) return;
            bool changed = false;
            for (int index = 0; index < State.Offers.Count; index++)
            {
                ShopOffer stale = State.Offers[index];
                if (stale.Sold || stale.Kind != OfferKind.Companion || OwnsCompanionSpecies(stale.Id)) continue;
                ShopOffer standIn = RegularStandIn();
                if (standIn == null) continue;
                State.Offers[index] = standIn; changed = true;
            }
            if (changed) ApplyKitPrices();
        }
        private ShopOffer RegularStandIn()
        {
            if (State.NextOffer == int.MaxValue) return null;
            var copies = new List<UnitDefinition>();
            if (CopyRecruitsRemaining > 0 && !State.Offers.Exists(o => !o.Sold && o.Kind == OfferKind.Companion && OwnsCompanionSpecies(o.Id)))
                foreach (UnitDefinition unit in catalog.Companions) if (OwnsCompanionSpecies(unit.Id) && RegularShelfSpecies(unit)) copies.Add(unit);
            var items = new List<ItemDefinition>();
            if (copies.Count == 0) foreach (ItemDefinition item in catalog.Items) if (Eligible(item, ShopPool.Gold)) items.Add(item);
            if (copies.Count == 0 && items.Count == 0) return null;
            bool recruit = copies.Count > 0;
            UnitDefinition friend = recruit ? copies[RecruitChoice(copies)] : null;
            ItemDefinition relic = recruit ? null : items[WeightedItem(items)];
            int tier = recruit ? OctoberOfferTier(friend.Id) : 1;
            int basePrice = Mathf.CeilToInt((recruit ? friend.Price * VeteranPrice(friend.Id) * (tier == 3 ? 3.5f : tier == 2 ? 1.8f : 1) : relic.Price * 0.85f * (1 + (int)relic.Rarity * 0.4f)) * StagePriceFactor);
            float discount = Bonus(ItemEffect.Discount) + Bonus(recruit ? ItemEffect.RecruitDiscount : ItemEffect.RelicDiscount);
            return new ShopOffer { Token = ++State.NextOffer, Kind = recruit ? OfferKind.Companion : OfferKind.Item, Id = recruit ? friend.Id : relic.Id, Tier = tier, Price = Mathf.Max(1, Mathf.CeilToInt(basePrice * (1f - Mathf.Clamp(discount, -1, 0.5f)))) };
        }
        public bool CompanionCapacityBlocks(ShopOffer offer)
        {
            return false; // Recruitment pacing uses purchase/species quotas, never a body cap.
        }
        public string RecruitmentSummary => "New friend: " + (NewRecruitsRemaining > 0 ? NewRecruitsRemaining + " this wave" : "next in wave " + ((long)State.LastRegularNewSpeciesWave + NewSpeciesEvery))
            + " (one every 3 waves).\nMore of a friend you have: " + CopyRecruitsRemaining + "/" + CopyRecruitLimit + " this wave.\nDream shop: " + SpecialNewRecruitsRemaining + "/1 new friend.";
        public string RecruitQuotaDenialFor(ShopOffer offer)
        {
            if (offer == null) return "";
            if (State.SpecialOffers.Contains(offer)) return NewSpeciesCooldown ? "NEW SPECIES AVAILABLE IN WAVE " + (State.LastNewShopSpeciesWave + 2) : "DREAM SHOP: 1 NEW SPECIES PER WAVE";
            if (OwnsCompanionSpecies(offer.Id)) return "1 COPY PER WAVE · BACK NEXT WAVE";
            return RegularNewSpeciesCooldown ? "NEW FRIEND IN WAVE " + (State.LastRegularNewSpeciesWave + NewSpeciesEvery) : "1 NEW FRIEND EVERY 3 WAVES";
        }
    }
}
