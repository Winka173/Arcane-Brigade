using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class RunSnapshot
    {
        /// <summary>Letters friends left when they were seen home: each brings Lamp Oil at the run's end (MetaProgress.Haul).</summary>
        public int Letters;
        /// <summary>The shop where a friend was last seen home, rowed across, or sold to the Last Customer (once each a shop).</summary>
        public int FarewellWave, FerryWave, MemorySaleWave;
    }

    /// <summary>Changing the brigade (requirement 31/10/2026). At the ferry a friend can be SEEN HOME (it leaves for good, half
    /// of what it cost comes back, its place for a new friend opens again before wave 20, and it leaves a letter worth Lamp Oil
    /// at the end of the run) or ROWED ACROSS (it is traded for one of three new friends at the same stars, for eggs). Now and
    /// then the Last Customer buys a friend outright: all it cost in gold, but no letter and no place back.</summary>
    public sealed partial class RunSession
    {
        public const int LettersCounted = 3, OilPerLetter = 10, MemorySaleChance = 20, MemorySaleFrom = 9;

        private bool FarewellShop => State.Phase == SessionPhase.Shop && !SecretRoomOpen && State.OutsideStage != OutsideStageStatus.Reward;
        /// <summary>Friends that can leave: owned, not one of the four story travellers, and never the last friend.</summary>
        public List<string> PartingSpecies()
        {
            var ids = new List<string>();
            foreach (RosterEntry entry in State.Roster) if (!ids.Contains(entry.UnitId) && !StorySpecies(entry.UnitId)) ids.Add(entry.UnitId);
            if (ids.Count < 2) ids.Clear();
            return ids;
        }
        private bool CanPart(string id) => FarewellShop && !string.IsNullOrEmpty(id) && PartingSpecies().Contains(id);

        /// <summary>Gold a friend cost, roughly: today's price for one copy at its star value.</summary>
        private int SpentOn(string id)
        {
            UnitDefinition unit = catalog.Unit(id);
            return unit == null ? 0 : Mathf.CeilToInt(unit.Price * StagePriceFactor * CompanionDeployment.OwnedValue(State.Roster, id));
        }
        public int SeeHomeRefund(string id) => Mathf.CeilToInt(SpentOn(id) * .5f);
        public int MemorySalePrice(string id) => SpentOn(id);
        public bool SeeHomeOpen => FarewellShop && State.FarewellWave != State.Wave && PartingSpecies().Count > 0;
        /// <summary>Its place opens again only while new friends still join (the shop before wave 20 is the last).</summary>
        public bool SeeHomeReturnsPlace(string id) => !NewAlliesClosed && catalog.Unit(id)?.SpecialRecruit == false;

        public bool SeeHome(string id)
        {
            if (!SeeHomeOpen || !CanPart(id)) return false;
            bool place = SeeHomeReturnsPlace(id);
            State.Gold = BoundedCounter((long)State.Gold + SeeHomeRefund(id));
            Part(id);
            if (place) State.RegularAllowanceCarryover = BoundedCounter((long)State.RegularAllowanceCarryover + 1);
            State.Letters = BoundedCounter((long)State.Letters + 1);
            State.FarewellWave = State.Wave;
            Notice = RunUICopy.UnitName(catalog.Unit(id)) + " went home and left you a letter.";
            return true;
        }

        private void Part(string id)
        {
            State.Roster.RemoveAll(entry => entry.UnitId == id);
            State.MergeDrought.RemoveAll(entry => entry.UnitId == id);
            RecountBonds();
        }

        // the ferry
        public bool FerryOpen => FarewellShop && State.Wave >= 2 && State.FerryWave != State.Wave && PartingSpecies().Count > 0;
        public int FerryEggs => Mathf.CeilToInt(3 * SpecialStagePriceFactor);
        /// <summary>Three friends waiting on the far bank for this one: new species of the same kind of shop (regular or dream),
        /// the same three each time the card opens in this shop.</summary>
        public List<string> FerryChoices(string id)
        {
            var result = new List<string>();
            UnitDefinition leaving = catalog.Unit(id);
            if (leaving == null) return result;
            var pool = new List<UnitDefinition>();
            foreach (UnitDefinition unit in catalog.Companions)
            {
                if (unit == null || OwnsCompanionSpecies(unit.Id) || unit.SpecialRecruit != leaving.SpecialRecruit) continue;
                bool fits = unit.SpecialRecruit
                    ? !StorySpecies(unit.Id) && unit.UnlockWave <= State.Wave && K11HeroRules.Eligible(catalog.Unit(State.ChampionId), unit) && Array.Exists(catalog.Weapons, weapon => RecruitWeaponEligible(unit.Id, weapon) && VariantFree(unit.Id, weapon))
                    : RegularShelfSpecies(unit);
                if (fits) pool.Add(unit);
            }
            var random = new System.Random(unchecked(State.Seed * 31 + State.Wave * 7919 + id.GetHashCode()));
            while (pool.Count > 0 && result.Count < 3)
            {
                int pick = random.Next(pool.Count);
                result.Add(pool[pick].Id); pool.RemoveAt(pick);
            }
            return result;
        }

        public bool Ferry(string id, string newId)
        {
            if (!FerryOpen || !CanPart(id) || State.Eggs < FerryEggs || !FerryChoices(id).Contains(newId)) return false;
            var entries = State.Roster.FindAll(entry => entry.UnitId == id);
            var crossing = new List<RosterEntry>();
            foreach (RosterEntry entry in entries)
            {
                var choices = new List<WeaponDefinition>();
                foreach (WeaponDefinition weapon in catalog.Weapons)
                    if (RecruitWeaponEligible(newId, weapon) && VariantFree(newId, weapon) && !crossing.Exists(c => c.WeaponId == weapon.Id)) choices.Add(weapon);
                if (choices.Count == 0) return false;
                crossing.Add(new RosterEntry { UnitId = newId, Tier = entry.Tier, WeaponId = BalancedWeapon(choices, catalog.Unit(newId).SpecialRecruit).Id });
            }
            State.Eggs -= FerryEggs;
            State.Roster.RemoveAll(entry => entry.UnitId == id);
            State.MergeDrought.RemoveAll(entry => entry.UnitId == id);
            foreach (RosterEntry entry in crossing) { State.Roster.Add(entry); RememberVariant(entry); }
            State.FerryWave = State.Wave;
            RecountBonds();
            Notice = RunUICopy.UnitName(catalog.Unit(newId)) + " came across the river at " + StarText(StarProgress(newId)) + " stars.";
            return true;
        }

        // the Last Customer's trade, after the Act I offer
        public bool MemorySaleOpen => !State.Endless && FarewellShop && State.Wave >= MemorySaleFrom && !CustomerOfferAvailable
            && State.MemorySaleWave != State.Wave && PartingSpecies().Count > 0
            && (forcedSaleWave == State.Wave || (int)(unchecked((uint)State.Seed * 2246822519u ^ (uint)State.Wave * 3266489917u) % 100u) < MemorySaleChance);
        private int forcedSaleWave = -1;
        /// <summary>Brings the Last Customer to this shop (captures).</summary>
        public void ForceMemorySale() => forcedSaleWave = State.Wave;
        public bool SellToCustomer(string id)
        {
            if (!MemorySaleOpen || !CanPart(id)) return false;
            State.Gold = BoundedCounter((long)State.Gold + MemorySalePrice(id));
            Part(id);
            State.MemorySaleWave = State.Wave;
            Notice = "The Last Customer paid. You no longer remember " + RunUICopy.UnitName(catalog.Unit(id)) + ".";
            return true;
        }
        public bool DeclineMemorySale()
        {
            if (!MemorySaleOpen) return false;
            State.MemorySaleWave = State.Wave; return true;
        }

        // the dream shop's Starlight Thread
        public const int ThreadFromWave = 15, ThreadChance = 10;
        private bool ThreadThisVisit => State.Wave >= ThreadFromWave && State.RainbowThreads == 0
            && ActiveRoster.Exists(entry => entry.Tier >= 3 && entry.Tier < catalog.MaximumTier && catalog.Unit(entry.UnitId)?.ReferenceCompanion != "Evil" && !StorySpecies(entry.UnitId))
            && (int)(unchecked((uint)State.Seed * 374761393u ^ (uint)State.Wave * 668265263u) % 100u) < ThreadChance;
        public int ThreadEggs => Mathf.CeilToInt(16 * SpecialStagePriceFactor);
        private void OfferStarlightThread()
        {
            if (!ThreadThisVisit) return;
            int slot = State.SpecialOffers.FindLastIndex(offer => offer.Kind == OfferKind.Item);
            if (slot < 0) return;
            State.SpecialOffers[slot] = new ShopOffer { Token = ++State.NextOffer, Kind = OfferKind.Item, Id = StarlightThreadItem.Id, Tier = 1, Price = ThreadEggs, BasePrice = ThreadEggs, Currency = ShopCurrency.Eggs };
        }
    }

    /// <summary>The Starlight Thread sold in the dream shop: a code-made item so the shelf and its card read it like a relic;
    /// buying it hands over a thread (GiveRainbowThread) and never puts it in the stash.</summary>
    public static class StarlightThreadItem
    {
        public const string Id = "StarlightThread";
        private static ItemDefinition item;
        public static bool Is(string id) => id == Id;
        public static ItemDefinition For(string id)
        {
            if (!Is(id)) return null;
            return item ??= new ItemDefinition
            {
                Id = Id, Name = "Starlight Thread", Icon = Resources.Load<Sprite>("UI/Items/StarlightThread"), Rarity = ItemRarity.Mythic, Price = 60,
                Stacking = StackPolicy.Unique, MaximumStacks = 1, Pools = ShopPool.Eggs, Category = RelicCategory.Curio, Effect = ItemEffect.ChampionDamage,
                Value = 0, Modifiers = new StatModifier[0], Description = "A 4-star friend becomes 5 stars. Waits for one if none is ready.",
                ShopThemes = new string[0], UnlockWave = RunSession.ThreadFromWave
            };
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() => item = null;
    }
}
