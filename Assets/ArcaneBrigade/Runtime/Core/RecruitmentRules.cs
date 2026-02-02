using System;

namespace ArcaneBrigade
{
    public sealed partial class RunSession
    {
        public int NewRecruitLimit => 1;
        public int CompanionLimit => int.MaxValue;
        public const int CompanionPurchaseLimit = 3;
        public int CompanionPurchasesRemaining => Math.Max(0, CompanionPurchaseLimit - State.NewRecruitsThisWave);
        public bool NewSpeciesCooldown => State.Wave>=10 && State.LastNewShopSpeciesWave>=10 && State.Wave-State.LastNewShopSpeciesWave<2;
        public int NewRecruitsRemaining => NewSpeciesCooldown?0:Math.Min(CompanionPurchasesRemaining, Math.Max(0, NewRecruitLimit - State.NewSpeciesRecruitsThisWave));
        public int SpecialNewRecruitsRemaining => NewSpeciesCooldown?0:Math.Max(0,1-State.SpecialNewSpeciesRecruitsThisWave);
        public bool OwnsCompanionSpecies(string id) => State.Roster.Exists(r => r.UnitId == id) || (State.AbsorbedSkills?.Contains(id) ?? false);
        // Source shop matters, not currency or the companion's catalog rarity.
        private bool CountsTowardRecruitLimit(ShopOffer offer) => offer != null && offer.Kind == OfferKind.Companion && !State.SpecialOffers.Contains(offer);
        private bool CountsAsNewRegularRecruit(ShopOffer offer) => offer != null && offer.Kind == OfferKind.Companion &&
            !State.SpecialOffers.Contains(offer) && !OwnsCompanionSpecies(offer.Id);
        public bool RecruitQuotaBlocks(ShopOffer offer) => offer!=null && offer.Kind==OfferKind.Companion &&
            (CountsTowardRecruitLimit(offer) && CompanionPurchasesRemaining==0 || !OwnsCompanionSpecies(offer.Id) && (State.SpecialOffers.Contains(offer)?SpecialNewRecruitsRemaining:NewRecruitsRemaining)==0);
        private void RegisterNewShopSpecies(bool special)
        {
            if(special)State.SpecialNewSpeciesRecruitsThisWave++;else State.NewSpeciesRecruitsThisWave++;
            if(State.Wave>=10)State.LastNewShopSpeciesWave=State.Wave;
        }
        public bool CompanionCapacityBlocks(ShopOffer offer)
        {
            return false; // Recruitment pacing uses purchase/species quotas, never a body cap.
        }
        public string RecruitmentSummary => "NEW SPECIES: " + NewRecruitsRemaining + "/" + NewRecruitLimit +
            " remaining. DREAM NEW SPECIES: "+SpecialNewRecruitsRemaining+"/1. From wave 10, both shops share one new species every two waves."+(NewSpeciesCooldown?" Next available: wave "+((long)State.LastNewShopSpeciesWave+2)+".":"")+" COMPANION PURCHASES: " + CompanionPurchasesRemaining + "/3 regular purchases remaining, including upgrade copies. Dream-shop upgrade copies use no regular purchase quota. No brigade body cap.";
        public string RecruitQuotaDenialFor(ShopOffer offer)=>NewSpeciesCooldown && offer!=null && !OwnsCompanionSpecies(offer.Id)?"NEW SPECIES AVAILABLE IN WAVE "+(State.LastNewShopSpeciesWave+2):offer!=null && State.SpecialOffers.Contains(offer)?"DREAM SHOP: 1 NEW SPECIES PER WAVE":CompanionPurchasesRemaining==0?"3 COMPANIONS BOUGHT THIS ROUND":"NEW SPECIES LIMIT: 1 THIS ROUND";
    }
}
