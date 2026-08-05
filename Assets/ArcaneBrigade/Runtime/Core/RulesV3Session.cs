using System;
using UnityEngine;
namespace ArcaneBrigade
{
    public sealed partial class RunSnapshot
    {
        public BondRunProgress RulesV3Bonds = new BondRunProgress();
        public HeroRulesV3RunState RulesV3Hero = new HeroRulesV3RunState();
        public int RulesV3FreeRerolls, RulesV3RerollDiscount;
        public int RulesV3RecruitWave;
        public System.Collections.Generic.List<RulesV3RecruitReceipt> RulesV3Receipts = new System.Collections.Generic.List<RulesV3RecruitReceipt>();
    }
    [Serializable]
    public sealed class RulesV3RecruitReceipt { public string UnitId;public int Paid; }
    public sealed partial class RunSession
    {
        public BondRunProgress RulesV3BondProgress { get => State.RulesV3Bonds ??= new BondRunProgress(); private set => State.RulesV3Bonds=value; }
        public HeroRulesV3RunState RulesV3HeroProgress { get => State.RulesV3Hero ??= new HeroRulesV3RunState(); private set => State.RulesV3Hero=value; }
        public int RulesV3FreeRerolls { get => State.RulesV3FreeRerolls; private set => State.RulesV3FreeRerolls=value; }
        public int RulesV3RerollDiscount { get => State.RulesV3RerollDiscount; private set => State.RulesV3RerollDiscount=value; }
        private void RulesV3Rerolled()
        {
            var bonds=new BondRuntime(catalog.Bonds,BondCounts,RulesV3BondProgress);
            bonds.OnReroll();
        }
        private void RulesV3Purchased(ShopOffer offer)
        {
            RulesV3HeroProgress.Purchases++;
            if(offer.Kind!=OfferKind.Companion)
            {
                HeroRuleV3Definition hero=HeroRulesV3Catalog.Find(State.ChampionId);
                if(hero!=null)foreach(HeroRelicV3Definition relic in hero.Relics)
                    if(relic.Id==offer.Id)RulesV3HeroProgress.Relics.Add(relic.Id);
            }
            else
            {
                State.RulesV3Receipts.Add(new RulesV3RecruitReceipt {UnitId=offer.Id,Paid=offer.Currency==ShopCurrency.Gold?offer.Price:0});
                var bonds=new BondRuntime(catalog.Bonds,BondCounts,RulesV3BondProgress);
                bonds.FirstRecruitDiscount(State.RulesV3RecruitWave!=State.Wave,true);State.RulesV3RecruitWave=State.Wave;
                if(State.ChampionId=="LoneJelly")foreach(PersonalTraitV3Definition trait in HeroRulesV3Catalog.Traits)
                    if(trait.CompanionId==offer.Id) { RulesV3HeroProgress.AbsorbedTraits.Add(offer.Id);RulesV3HeroProgress.AbsorbedCount++;break; }
            }
        }
        private bool RulesV3HeroFits(ItemDefinition item)
        {
            foreach(HeroRuleV3Definition hero in HeroRulesV3Catalog.Definitions)
                foreach(HeroRelicV3Definition relic in hero.Relics)
                    if(relic.Id==item.Id)return hero.Id==State.ChampionId;
            return HeroRelicPools.Fits(catalog.Unit(State.ChampionId),item);
        }
        private float RulesV3RecruitDiscount()
        {
            float discount=State.ChampionId=="PennyBroker" ? (RulesV3HeroProgress.Relics.Contains("PennyBroker-0")?HeroRulesV3Tuning.HaggleRelic:HeroRulesV3Tuning.Haggle) : 0;
            var bonds=new BondRuntime(catalog.Bonds,BondCounts,RulesV3BondProgress);
            return Mathf.Clamp01(discount+bonds.FirstRecruitDiscount(State.RulesV3RecruitWave!=State.Wave));
        }
        private int RulesV3ExtraShopSlots => State.ChampionId=="IvorySovereign" && RulesV3HeroProgress.Decree==0?1:0;
        public bool ChooseRulesV3Decree(int choice)
        {
            if(State.ChampionId!="IvorySovereign" || choice<0 || choice>=(RulesV3HeroProgress.Relics.Contains("IvorySovereign-0")?4:3))return false;
            if(RulesV3HeroProgress.Decree>=0 && RulesV3HeroProgress.DecreeUntilWave>=State.Wave)
            {if(!RulesV3HeroProgress.Relics.Contains("IvorySovereign-3") || RulesV3HeroProgress.ClemencyUsed)return false;RulesV3HeroProgress.ClemencyUsed=true;}
            RulesV3HeroProgress.Decree=choice;RulesV3HeroProgress.DecreeUntilWave=State.Wave+(RulesV3HeroProgress.Relics.Contains("IvorySovereign-1")?1:0);return true;
        }
        public bool SellRulesV3Friend(string unitId)
        {
            if(State.Phase!=SessionPhase.Shop || State.ChampionId!="PennyBroker" || !RulesV3HeroProgress.Relics.Contains("PennyBroker-2"))return false;
            int index=State.Roster.FindIndex(entry=>entry.UnitId==unitId);if(index<0)return false;
            int refund=0;
            for(int i=State.RulesV3Receipts.Count-1;i>=0;i--)if(State.RulesV3Receipts[i].UnitId==unitId)
            {refund=State.RulesV3Receipts[i].Paid;State.RulesV3Receipts.RemoveAt(i);break;}
            State.Roster.RemoveAt(index);State.Gold=BoundedCounter((long)State.Gold+refund);RecountBonds();return true;
        }
    }
}
