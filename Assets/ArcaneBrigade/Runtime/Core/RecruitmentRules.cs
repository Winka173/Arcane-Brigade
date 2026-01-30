using System;

namespace ArcaneBrigade
{
    public sealed partial class RunSession
    {
        public int NewRecruitLimit => State.Wave <= 3 ? 2 : 1;
        public int CompanionLimit => OctoberBalance.AllySoftLimit;
        public const int CompanionPurchaseLimit = 3;
        public int CompanionPurchasesRemaining => Math.Max(0, CompanionPurchaseLimit - State.NewRecruitsThisWave);
        public int NewRecruitsRemaining => Math.Min(CompanionPurchasesRemaining, Math.Max(0, NewRecruitLimit - State.NewSpeciesRecruitsThisWave));
        public bool OwnsCompanionSpecies(string id) => State.Roster.Exists(r => r.UnitId == id) || (State.AbsorbedSkills?.Contains(id) ?? false);
        // Source shop matters, not currency or the companion's catalog rarity.
        private bool CountsTowardRecruitLimit(ShopOffer offer) => offer != null && offer.Kind == OfferKind.Companion && !State.SpecialOffers.Contains(offer);
        private bool CountsAsNewRegularRecruit(ShopOffer offer) => offer != null && offer.Kind == OfferKind.Companion &&
            !State.SpecialOffers.Contains(offer) && !OwnsCompanionSpecies(offer.Id);
        public bool RecruitQuotaBlocks(ShopOffer offer) => CountsTowardRecruitLimit(offer) &&
            (CompanionPurchasesRemaining == 0 || NewRecruitsRemaining == 0 && !OwnsCompanionSpecies(offer.Id));
        public bool CompanionCapacityBlocks(ShopOffer offer)
        {
            if (offer == null || offer.Kind != OfferKind.Companion || OctoberAbsorbs || State.Roster.Count < CompanionLimit) return false;
            int tier = Math.Clamp(offer.Tier, 1, 3);
            return tier >= catalog.MaximumTier || catalog.Unit(offer.Id)?.ReferenceCompanion == "Evil" || State.Roster.FindAll(r => r.UnitId == offer.Id && r.Tier == tier).Count < 2;
        }
        public string RecruitmentSummary => "NEW SPECIES: " + NewRecruitsRemaining + "/" + NewRecruitLimit +
            " remaining. COMPANION PURCHASES: " + CompanionPurchasesRemaining + "/3 remaining this round, including star-upgrade copies. Dream-shop recruits use neither quota. Brigade limit: 15 bodies; immediate merges remain possible when full.";
        public string RecruitQuotaDenial => CompanionPurchasesRemaining == 0 ? "3 COMPANIONS BOUGHT THIS ROUND" : "NEW SPECIES LIMIT: " + NewRecruitLimit + " THIS ROUND";
    }
}
