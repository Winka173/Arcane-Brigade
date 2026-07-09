using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class RunSession
    {
        public int NewRecruitLimit => 1;
        public int CompanionLimit => int.MaxValue;
        public const int CopyRecruitLimit = 1;
        public const int CompanionPurchaseLimit = 2;
        public const int NewSpeciesEvery = 2;
        public int CopyRecruitLimitThisWave => CopyRecruitLimit; // requirement 22/10/2026: one upgrade copy a wave, wave 10 on too (was 2 from wave 10)
        private static int BaseNewSpeciesAllowance(int wave) => wave <= 9 ? 1 + (wave - 1) / NewSpeciesEvery : 5 + (wave - 9) / 3;
        public int EarnedNewSpeciesAllowance => BoundedCounter((long)BaseNewSpeciesAllowance(State.Wave) + KitNewRecruits + State.RegularAllowanceCarryover);
        public int NextRegularAllowanceWave
        {
            get
            {
                long calendar = State.Wave < 9 ? ((long)(State.Wave - 1) / 2 + 1) * 2 + 1 : 9 + ((long)(State.Wave - 9) / 3 + 1) * 3;
                long spent = (long)State.RegularNewSpeciesPurchased - KitNewRecruits - State.RegularAllowanceCarryover;
                long affordable = spent < 5 ? spent * 2 + 1 : 9 + (spent - 4) * 3;
                return BoundedCounter(Math.Max(calendar, affordable));
            }
        }
        public int BankedNewSpeciesAllowance => Math.Max(0, EarnedNewSpeciesAllowance - State.RegularNewSpeciesPurchased);
        public int CopyRecruitsRemaining => Math.Max(0, CopyRecruitLimitThisWave - (State.NewRecruitsThisWave - State.NewSpeciesRecruitsThisWave));
        public int CompanionPurchasesRemaining => NewRecruitsRemaining + CopyRecruitsRemaining;
        public bool NewSpeciesCooldown => false; // the dream shop is independent of the regular allowance
        public bool RegularNewSpeciesCooldown => BankedNewSpeciesAllowance <= 0;
        /// <summary>requirement 27/10/2026: from wave 20 on the brigade is closed to new friends (the shops before it included, since
        /// what is bought there fights in it); copies of friends you own still merge.</summary>
        public const int NoNewAlliesFrom = 20;
        public bool NewAlliesClosed => State.Wave + 1 >= NoNewAlliesFrom;
        public int NewRecruitsRemaining => !NewAlliesClosed && BankedNewSpeciesAllowance > 0 && State.NewSpeciesRecruitsThisWave == 0 ? 1 : 0;
        public bool RegularRecruitOpen(string id) => OwnsCompanionSpecies(id) ? CopyRecruitsRemaining > 0 : NewRecruitsRemaining > 0;
        public int SpecialNewRecruitsRemaining => NewAlliesClosed ? 0 : Math.Max(0, 1 - State.SpecialNewSpeciesRecruitsThisWave);
        public bool OwnsCompanionSpecies(string id) => State.Roster.Exists(r => r.UnitId == id) || (State.AbsorbedSkills?.Contains(id) ?? false);
        // Source shop matters, not currency or the companion's catalog rarity.
        private bool CountsTowardRecruitLimit(ShopOffer offer) => offer != null && offer.Kind == OfferKind.Companion && !State.SpecialOffers.Contains(offer);
        private bool CountsAsNewRegularRecruit(ShopOffer offer) => offer != null && offer.Kind == OfferKind.Companion &&
            !State.SpecialOffers.Contains(offer) && !OwnsCompanionSpecies(offer.Id);
        public bool RecruitQuotaBlocks(ShopOffer offer) => offer!=null && offer.Kind==OfferKind.Companion &&
            (State.SpecialOffers.Contains(offer) ? !OwnsCompanionSpecies(offer.Id) && SpecialNewRecruitsRemaining==0 : !RegularRecruitOpen(offer.Id));
        private void RegisterNewShopSpecies(bool special)
        {
            if(special)State.SpecialNewSpeciesRecruitsThisWave++;else { State.NewSpeciesRecruitsThisWave++; State.LastRegularNewSpeciesWave=State.Wave; State.RegularNewSpeciesPurchased = BoundedCounter((long)State.RegularNewSpeciesPurchased + 1); }
            if (!special) State.LastNewShopSpeciesWave = State.Wave;
        }
        /// <summary>A species the regular shelf may show at all (before the quotas): ordinary, unlocked, fits the hero, and a
        /// weapon variant is still free for it.</summary>
        private bool RegularShelfSpecies(UnitDefinition unit) => !unit.SpecialRecruit && !StorySpecies(unit.Id) && unit.UnlockWave <= State.Wave &&
            (catalog.HeroRevision == 0 || K11HeroRules.Eligible(catalog.Unit(State.ChampionId), unit)) &&
            Array.Exists(catalog.Weapons, weapon => RecruitWeaponEligible(unit.Id, weapon) && VariantFree(unit.Id, weapon));
        /// <summary>Requirement 22/10/2026: the shop never sells what can't be bought. Rolls already leave new species out while the
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
            items = HeroRelicPools.Pool(catalog.Unit(State.ChampionId), items);
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
        public string RecruitmentSummary => "Regular shop: " + NewRecruitsRemaining + "/1 new species this wave."
            + "\nSaved allowance: " + BankedNewSpeciesAllowance + " (one every 2 waves through wave 9, then every 3; unused allowance carries over)."
            + "\nNext allowance: wave " + NextRegularAllowanceWave + "."
            + "\nUpgrade materials: " + CopyRecruitsRemaining + "/" + CopyRecruitLimitThisWave + " regular purchases this wave."
            + "\nDream shop: " + SpecialNewRecruitsRemaining + "/1 new species this visit; independent of regular allowance.";
        public string RecruitQuotaDenialFor(ShopOffer offer)
        {
            if (offer == null) return "";
            if (State.SpecialOffers.Contains(offer)) return "DREAM SHOP: 1 NEW SPECIES PER VISIT";
            if (OwnsCompanionSpecies(offer.Id)) return CopyRecruitLimitThisWave + " UPGRADE MATERIAL PURCHASES PER WAVE";
            return State.NewSpeciesRecruitsThisWave > 0 ? "1 NEW SPECIES PER WAVE - SAVED ALLOWANCE CARRIES OVER"
                : "NEXT REGULAR ALLOWANCE IN WAVE " + NextRegularAllowanceWave;
        }
    }
}
