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
        // requirement 31/10/2026: copies of owned friends have no per-wave cap; each one bought this wave makes the next rarer on the shelf
        public int CopyRecruitLimitThisWave => int.MaxValue / 4;
        private static readonly int[] CopyChancesEarly = { 60, 30, 10 }, CopyChancesLate = { 75, 50, 25, 5 };
        public int CopiesBoughtThisWave => Math.Max(0, State.NewRecruitsThisWave - State.NewSpeciesRecruitsThisWave);
        /// <summary>The chance (percent) that a shelf roll shows a copy of an owned friend, falling with each copy bought this wave:
        /// 60, 30, 10, then 1 before wave 10; 75, 50, 25, 5, then 1 from wave 10.</summary>
        public int CopyOfferChance
        {
            get { int[] chances = State.Wave >= 10 ? CopyChancesLate : CopyChancesEarly; return CopiesBoughtThisWave < chances.Length ? chances[CopiesBoughtThisWave] : 1; }
        }
        /// <summary>A friend a copy can still raise: below 4 stars (the fifth comes only from a Starlight Thread), not one that never merges.</summary>
        public bool CanStarUp(string id) => HighestOwnedTier(id) < CompanionDeployment.ShopMaxTier && catalog.Unit(id)?.ReferenceCompanion != "Evil";
        /// <summary>requirement 31/10/2026: a new-friend allowance at waves 1, 3, 6, 9, 12, 15 and in the shop before wave 20 (seven a run).</summary>
        private static readonly int[] NewFriendWaves = { 1, 3, 6, 9, 12, 15 };
        private static int BaseNewSpeciesAllowance(int wave)
        {
            int count = 0; foreach (int w in NewFriendWaves) if (wave >= w) count++;
            return count + (wave + 1 >= NoNewAlliesFrom ? 1 : 0);
        }
        public int EarnedNewSpeciesAllowance => BoundedCounter((long)BaseNewSpeciesAllowance(State.Wave) + KitNewRecruits + State.RegularAllowanceCarryover);
        public int NextRegularAllowanceWave
        {
            get
            {
                // the first later shop whose calendar allowance covers what has been spent
                long spent = (long)State.RegularNewSpeciesPurchased - KitNewRecruits - State.RegularAllowanceCarryover;
                for (int wave = State.Wave + 1; wave < State.Wave + 200; wave++)
                    if (BaseNewSpeciesAllowance(wave) > spent) return wave;
                return BoundedCounter((long)State.Wave + 200);
            }
        }
        public int BankedNewSpeciesAllowance => Math.Max(0, EarnedNewSpeciesAllowance - State.RegularNewSpeciesPurchased);
        public int CopyRecruitsRemaining => Math.Max(0, CopyRecruitLimitThisWave - (State.NewRecruitsThisWave - State.NewSpeciesRecruitsThisWave));
        public int CompanionPurchasesRemaining => NewRecruitsRemaining + CopyRecruitsRemaining;
        public bool NewSpeciesCooldown => false; // the dream shop is independent of the regular allowance
        public bool RegularNewSpeciesCooldown => BankedNewSpeciesAllowance <= 0;
        /// <summary>requirement 31/10/2026: the shop before wave 20 is the last open to new friends (it brings one more allowance);
        /// after it the brigade is closed to new friends. Copies of friends you own still raise their stars.</summary>
        public const int NoNewAlliesFrom = 20;
        public bool NewAlliesClosed => State.Wave + 1 > NoNewAlliesFrom;
        public int NewRecruitsRemaining => !NewAlliesClosed && BankedNewSpeciesAllowance > 0 && State.NewSpeciesRecruitsThisWave == 0 ? 1 : 0;
        public bool RegularRecruitOpen(string id) => OwnsCompanionSpecies(id) ? CanStarUp(id) : NewRecruitsRemaining > 0;
        public int SpecialNewRecruitsRemaining => NewAlliesClosed ? 0 : Math.Max(0, 1 - State.SpecialNewSpeciesRecruitsThisWave);
        /// <summary>Special friends owned (dream shop species).</summary>
        public int SpecialFriendsOwned { get { var seen = new HashSet<string>(); foreach (RosterEntry r in State.Roster) if (catalog.Unit(r.UnitId)?.SpecialRecruit == true) seen.Add(r.UnitId); return seen.Count; } }
        private static readonly int[] SpecialNewChances = { 100, 70, 40, 10 };
        public const int SpecialFriendsByWave20 = 3;
        /// <summary>The chance (percent) that this dream shop visit offers a new special friend: 100, 70, 40, 10 by special friends owned,
        /// then 1; none past four before wave 20.</summary>
        public int SpecialNewChance => SpecialFriendsOwned >= SpecialFriendsByWave20 && State.Wave + 1 <= NoNewAlliesFrom ? 0
            : SpecialFriendsOwned < SpecialNewChances.Length ? SpecialNewChances[SpecialFriendsOwned] : 1;
        /// <summary>Decided once a visit from the run's seed, the wave and the count owned, so a reroll of the stall never raises it.</summary>
        public bool SpecialNewThisVisit => SpecialNewRecruitsRemaining > 0 && (int)(unchecked((uint)State.Seed * 2654435761u ^ (uint)State.Wave * 40503u ^ (uint)SpecialFriendsOwned * 9973u) % 100u) < SpecialNewChance;
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
            + "\nCopies of your friends: no limit; the next shows up " + CopyOfferChance + "% of the time."
            + "\nDream shop: at most 1 new special friend a visit (" + SpecialNewChance + "% now).";
        public string RecruitQuotaDenialFor(ShopOffer offer)
        {
            if (offer == null) return "";
            if (State.SpecialOffers.Contains(offer)) return "DREAM SHOP: 1 NEW SPECIES PER VISIT";
            if (OwnsCompanionSpecies(offer.Id)) return CanStarUp(offer.Id) ? "" : "ALREADY AT MAX STARS";
            return State.NewSpeciesRecruitsThisWave > 0 ? "1 NEW SPECIES PER WAVE - SAVED ALLOWANCE CARRIES OVER"
                : "NEXT REGULAR ALLOWANCE IN WAVE " + NextRegularAllowanceWave;
        }
    }
}
