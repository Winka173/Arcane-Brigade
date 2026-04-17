using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class RunSnapshot
    {
        /// <summary>New items (Docs/Content_Expansion_v5.md §10): Specimen Pin's species, Refund Slip's last relic and its use this
        /// shop.</summary>
        public List<string> KilledSpecies = new List<string>();
        public string RefundRelicId; public int RefundRelicPrice; public bool RefundUsed;
    }

    /// <summary>The thirty new items of the content expansion, run side.</summary>
    public sealed partial class RunSession
    {
        /// <summary>Refund Slip: the last relic bought this shop can go back for the full price, once per shop.</summary>
        public bool RefundAvailable => State.Phase == SessionPhase.Shop && !State.RefundUsed && MechanicStacks(RelicMechanic.RefundSlip) > 0
            && !string.IsNullOrEmpty(State.RefundRelicId) && State.RefundRelicId != "RefundSlip" && StackCount(State.RefundRelicId) > 0;
        public bool RefundLastRelic()
        {
            if (!RefundAvailable) return false;
            float previousMaximum = MaximumHealth();
            ItemStack stack = State.Items.Find(s => s.Id == State.RefundRelicId);
            if (--stack.Count <= 0) State.Items.Remove(stack);
            State.Gold = BoundedCounter((long)State.Gold + State.RefundRelicPrice); State.RefundUsed = true;
            Notice = "Refund Slip: " + catalog.Item(State.RefundRelicId).Name + " returned for " + State.RefundRelicPrice + " gold.";
            RecountBonds(); ReconcileHealth(previousMaximum);
            return true;
        }
        private void RememberRelicPurchase(ShopOffer offer)
        {
            if (offer.Kind != OfferKind.Item || offer.Currency != ShopCurrency.Gold) return;
            State.RefundRelicId = offer.Id; State.RefundRelicPrice = offer.Price;
        }
        private void NewShopForRelics() { State.RefundUsed = false; State.RefundRelicId = null; State.RefundRelicPrice = 0; }

        /// <summary>Twin Ticket: a merge into three stars brings a free one-star copy.</summary>
        private void RelicMerged(string unitId, int tier)
        {
            if (tier < 3 || MechanicStacks(RelicMechanic.TwinTicket) == 0) return;
            var weapons = new List<WeaponDefinition>();
            foreach (WeaponDefinition w in catalog.Weapons) if (RecruitWeaponEligible(unitId, w) && VariantFree(unitId, w)) weapons.Add(w);
            if (weapons.Count == 0) return;
            var copy = new RosterEntry { UnitId = unitId, Tier = 1, WeaponId = weapons[Next(ref State.WeaponRandom, weapons.Count)].Id };
            State.Roster.Add(copy); RememberVariant(copy);
            Notice = "Twin Ticket: a one-star " + catalog.Unit(unitId).DisplayName + " came along.";
        }

        /// <summary>Splice Kit: once per wave three different one-star friends become one random two-star friend of the hero's pool.</summary>
        private void SpliceFriends()
        {
            if (MechanicStacks(RelicMechanic.SpliceKit) == 0) return;
            var species = new List<string>();
            foreach (RosterEntry r in State.Roster)
            {
                UnitDefinition u = catalog.Unit(r.UnitId);
                if (r.Tier == 1 && !species.Contains(r.UnitId) && !u.SpecialRecruit && !StorySpecies(r.UnitId) && u.Rarity < ItemRarity.Mythic) species.Add(r.UnitId);
            }
            if (species.Count < 3) return;
            var pool = new List<UnitDefinition>();
            foreach (UnitDefinition u in catalog.Companions)
                if (!u.SpecialRecruit && !StorySpecies(u.Id) && u.Rarity < ItemRarity.Mythic && u.UnlockWave <= State.Wave && (catalog.HeroRevision == 0 || K11HeroRules.Eligible(catalog.Unit(State.ChampionId), u))) pool.Add(u);
            if (pool.Count == 0) return;
            for (int i = 0; i < 3; i++)
            {
                int pick = Next(ref State.WeaponRandom, species.Count);
                State.Roster.Remove(State.Roster.Find(r => r.UnitId == species[pick] && r.Tier == 1)); species.RemoveAt(pick);
            }
            UnitDefinition result = pool[Next(ref State.WeaponRandom, pool.Count)];
            var weapons = new List<WeaponDefinition>();
            foreach (WeaponDefinition w in catalog.Weapons) if (RecruitWeaponEligible(result.Id, w) && VariantFree(result.Id, w)) weapons.Add(w);
            if (weapons.Count == 0) weapons.Add(result.Weapon);
            var spliced = new RosterEntry { UnitId = result.Id, Tier = 2, WeaponId = weapons[Next(ref State.WeaponRandom, weapons.Count)].Id };
            State.Roster.Add(spliced); RememberVariant(spliced); MergeCompanions(result.Id);
            Notice = "Splice Kit: three friends became a two-star " + result.DisplayName + ".";
            RecountBonds();
        }

        /// <summary>After a fight: Specimen Pin keeps the species, Storybook Ending writes a page.</summary>
        private void SettleNewRelics(CombatSimulation simulation)
        {
            State.KilledSpecies ??= new List<string>();
            foreach (string id in simulation.KilledSpecies) if (!State.KilledSpecies.Contains(id) && State.KilledSpecies.Count < 512) State.KilledSpecies.Add(id);
            if (simulation.Phase != RunPhase.Victory || MechanicStacks(RelicMechanic.StorybookEnding) == 0) return;
            ItemEffect[] effects = { ItemEffect.Damage, ItemEffect.AttackSpeed, ItemEffect.Crit, ItemEffect.AttackRange, ItemEffect.BulletSpeed };
            float[] values = { .03f, .03f, .02f, .04f, .05f };
            int page = Next(ref State.WeaponRandom, effects.Length);
            GainRelic(effects[page], values[page], ModifierScope.Team);
            Notice = "Storybook Ending: a new page (+" + RunModifierRules.ValueText(effects[page], values[page]) + " " + RunModifierRules.ModifierName(effects[page]) + ").";
        }
    }
}
