using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class RunSnapshot
    {
        /// <summary>Hero kit bookkeeping (18/10/2026): purchases in the current shop (for first/second purchase discounts) and
        /// merges made in this run (for kits that grow with merging).</summary>
        public int KitPurchasesThisShop, KitMerges, KitRareBuys;
    }

    public sealed partial class ShopOffer
    {
        /// <summary>The offer's price before hero-kit discounts (0 on offers saved before kits: the price is the base).</summary>
        public int BasePrice;
    }

    /// <summary>The run side of the hero kits: team stats folded into the run's bonuses, the shop, rerolls, interest and income,
    /// eggs, recruiting, merging and bonds. Every effect reads the hero's kit (HeroKits) and does nothing for other heroes.</summary>
    public sealed partial class RunSession
    {
        public HeroKit Kit => HeroKits.For(State?.ChampionId);
        private float KitValue(KitPassive kind) => Kit?.Value(kind) ?? 0;
        private bool KitHas(KitPassive kind) => Kit != null && Kit.Has(kind);
        private bool kitRarerRoll;

        /// <summary>Static team passives as run bonuses (they then show in stats and descriptions like any relic).</summary>
        private float KitBonus(ItemEffect effect, ModifierScope target)
        {
            HeroKit kit = Kit; if (kit == null) return 0;
            bool companions = target == ModifierScope.Companions, run = target == ModifierScope.Run;
            switch (effect)
            {
                case ItemEffect.CompanionDamage: return companions ? kit.Value(KitPassive.TeamDamage) + Mathf.Min(.25f, State.KitRareBuys * kit.Value(KitPassive.RareBuyDamage)) : 0;
                case ItemEffect.CompanionAttackSpeed: return companions ? kit.Value(KitPassive.TeamAttackSpeed) : 0;
                case ItemEffect.Crit: return companions ? kit.Value(KitPassive.TeamCrit) : 0;
                case ItemEffect.MoveSpeed: return companions ? kit.Value(KitPassive.TeamMoveSpeed) : 0;
                case ItemEffect.BulletSpeed: return companions ? kit.Value(KitPassive.TeamProjectileSpeed) : 0;
                case ItemEffect.BulletPierce: return companions ? kit.Value(KitPassive.TeamPierce) : 0;
                case ItemEffect.AttackRange: return companions ? kit.Value(KitPassive.TeamRange) : 0;
                case ItemEffect.Pickup: return run ? kit.Value(KitPassive.PickupRadius) : 0;
                case ItemEffect.Interest: return run ? kit.Value(KitPassive.InterestCap) : 0;
                case ItemEffect.Income: return run ? kit.Value(KitPassive.Income) + (kit.Has(KitPassive.DrinkIncome) ? Mathf.Min(kit.Value(KitPassive.DrinkIncome), State.OctoberDrinks) : 0) : 0;
                default: return 0;
            }
        }
        /// <summary>How much more a relic gives for this hero: a second copy is worth 50% more (PetalCitadel), food on the
        /// hero works better (NightMaw, SupperMortar).</summary>
        private float KitItemScale(ItemDefinition item, int count, ModifierScope target)
        {
            float scale = 1;
            if (count >= 2 && KitHas(KitPassive.DuplicateRelicBonus)) scale *= 1 + KitValue(KitPassive.DuplicateRelicBonus) * (count - 1) / count;
            if (item.Category == RelicCategory.Food && target == ModifierScope.Champion) scale *= 1 + KitValue(KitPassive.FoodPotency);
            return scale;
        }
        /// <summary>Kit adjustments made while a unit is compiled for battle.</summary>
        private void KitCompileUnit(UnitDefinition unit, UnitDefinition source, bool companion)
        {
            HeroKit kit = Kit; if (kit == null) return;
            if (!companion && kit.Has(KitPassive.MergeHeroDamage)) unit.Weapon.Damage *= 1 + Mathf.Min(.25f, State.KitMerges * kit.Value(KitPassive.MergeHeroDamage));
            if (companion && unit.Weapon.Kind == WeaponKind.Melee && kit.Has(KitPassive.TeamMeleeAttackSpeed)) unit.Weapon.Cooldown /= 1 + kit.Value(KitPassive.TeamMeleeAttackSpeed);
            if (companion && (source.Id == "GrandSprout" || source.Id == "LittleSprout")) unit.Weapon.Damage *= 1 + kit.Value(KitPassive.SproutDamage);
        }

        /// <summary>Recruiting: one more new species a wave (VelvetRegent).</summary>
        private int KitNewRecruits => Mathf.RoundToInt(KitValue(KitPassive.NewRecruitPlusOne));
        /// <summary>More offers on the shelf.</summary>
        private int KitExtraSlots => Mathf.RoundToInt(KitValue(KitPassive.ExtraShopSlot));

        /// <summary>The reroll price for this hero: the first in a shop is free, a full purse makes it cheaper.</summary>
        private int KitRerollCost(int cost)
        {
            if (KitHas(KitPassive.FreeFirstReroll) && State.RerollsThisShop == 0) return 0;
            if (KitHas(KitPassive.RerollCheaperWithGold) && State.Gold >= 50) cost = Mathf.Max(1, cost - Mathf.RoundToInt(KitValue(KitPassive.RerollCheaperWithGold)));
            return cost;
        }
        /// <summary>After a paid reroll: a 1 in 6 refund, or a 1 in 6 rarer roll (GlassDice).</summary>
        private bool KitRerollLuck(int paid)
        {
            if (!KitHas(KitPassive.RerollLuck)) return false;
            int face = Next(ref State.ShopRandom, 6);
            if (face == 0 && paid > 0) { State.Gold = BoundedCounter((long)State.Gold + paid); Notice = "The dice rolled a one: this reroll was free."; }
            if (face == 5) { kitRarerRoll = true; return true; }
            return false;
        }
        private float KitRarityBonus => kitRarerRoll ? 2f : 0f;

        /// <summary>Hero-kit prices: discounts on the first or later purchases, drinks and food, the royal pick and the cheapest
        /// offer. Applied after every roll and every purchase, from each offer's base price.</summary>
        private void ApplyKitPrices()
        {
            HeroKit kit = Kit; if (kit == null) return;
            ShopOffer rarest = null, cheapest = null;
            foreach (ShopOffer offer in State.Offers)
            {
                if (offer.Sold || offer.Currency != ShopCurrency.Gold) continue;
                if (offer.BasePrice <= 0) offer.BasePrice = offer.Price;
                if (offer.Kind == OfferKind.Item && (rarest == null || catalog.Item(offer.Id).Rarity > catalog.Item(rarest.Id).Rarity)) rarest = offer;
                if (cheapest == null || offer.BasePrice < cheapest.BasePrice) cheapest = offer;
            }
            foreach (ShopOffer offer in State.Offers)
            {
                if (offer.Sold || offer.Currency != ShopCurrency.Gold || offer.BasePrice <= 0) continue;
                float off = 0;
                if (State.KitPurchasesThisShop == 0) off += kit.Value(KitPassive.FirstPurchaseDiscount);
                else off += kit.Value(KitPassive.SecondPurchaseDiscount);
                if (offer.Kind == OfferKind.Item)
                {
                    RelicCategory category = catalog.Item(offer.Id).Category;
                    if (category == RelicCategory.Drink) off += kit.Value(KitPassive.DrinkDiscount);
                    if (category == RelicCategory.Food) off += kit.Value(KitPassive.FoodDiscount);
                }
                if (offer == rarest) off += kit.Value(KitPassive.RoyalPick);
                int price = Mathf.Max(1, Mathf.CeilToInt(offer.BasePrice * (1 - Mathf.Clamp(off, 0, .6f))));
                if (offer == cheapest && kit.Has(KitPassive.CheapestDiscountWithGold) && State.Gold >= 20) price = Mathf.Max(1, price - 1);
                offer.Price = price;
            }
        }
        /// <summary>A purchase was made: count it and re-price the rest of the shelf.</summary>
        private void KitBought(ShopOffer offer)
        {
            if (offer == null) return;
            if (offer.Currency == ShopCurrency.Gold && State.Offers.Contains(offer)) State.KitPurchasesThisShop++;
            ItemRarity rarity = offer.Kind == OfferKind.Companion ? catalog.Unit(offer.Id)?.Rarity ?? ItemRarity.Common : catalog.Item(offer.Id)?.Rarity ?? ItemRarity.Common;
            if (rarity == ItemRarity.Mythic || rarity == ItemRarity.Rainbow) State.KitRareBuys++;
            if (offer.Kind != OfferKind.Companion && KitHas(KitPassive.DuplicateRelicGold) && (State.Items.Find(s => s.Id == offer.Id)?.Count ?? 0) >= 2)
            { State.Gold = BoundedCounter((long)State.Gold + Mathf.RoundToInt(KitValue(KitPassive.DuplicateRelicGold))); Notice = "Twin petals: 3 gold back."; }
            ApplyKitPrices();
        }
        /// <summary>A recruit arriving: the greenhouse may raise a one-star to two (VerdantMatron).</summary>
        private void KitRecruited(RosterEntry recruited, bool alreadyOwned, List<WeaponDefinition> weapons)
        {
            HeroKit kit = Kit; if (kit == null) return;
            if (recruited.Tier == 1 && kit.Has(KitPassive.BuyTwoStarChance) && Next(ref State.WeaponRandom, 1000) < kit.Value(KitPassive.BuyTwoStarChance) * 1000) recruited.Tier = 2;
            if (alreadyOwned && kit.Has(KitPassive.DuplicateCopyChance) && Next(ref State.WeaponRandom, 1000) < kit.Value(KitPassive.DuplicateCopyChance) * 1000)
            {
                var spare = weapons.FindAll(w => w.Id != recruited.WeaponId && !State.Roster.Exists(e => e.UnitId == recruited.UnitId && e.WeaponId == w.Id));
                if (spare.Count > 0) { State.Roster.Add(new RosterEntry { UnitId = recruited.UnitId, Tier = recruited.Tier, WeaponId = spare[Next(ref State.WeaponRandom, spare.Count)].Id }); Notice = "Reflection: a second copy came with it."; }
            }
        }
        /// <summary>A merge happened.</summary>
        private void KitMerged()
        {
            State.KitMerges++;
            int gold = Mathf.RoundToInt(KitValue(KitPassive.MergeGold));
            if (gold > 0) State.Gold = BoundedCounter((long)State.Gold + gold);
        }
        private void KitAbsorbed()
        {
            int gold = Mathf.RoundToInt(KitValue(KitPassive.AbsorbGold));
            if (gold > 0) State.Gold = BoundedCounter((long)State.Gold + gold);
        }
        /// <summary>The purse interest is reckoned on (ThornTreasurer reckons it on 150%).</summary>
        private int KitInterestBase(int gold) => Mathf.FloorToInt(gold * (1 + KitValue(KitPassive.InterestOnGold)));
        /// <summary>Wave-end kit income: dividends, boss bounties, the end-of-wave die, dream eggs. Returns the gold added.</summary>
        private int KitWaveEnd(bool bossDefeated)
        {
            HeroKit kit = Kit; if (kit == null) return 0;
            int gold = 0;
            if (kit.Has(KitPassive.Dividends)) gold += Mathf.Min(Mathf.RoundToInt(kit.Value(KitPassive.Dividends)), State.Items.Count);
            if (bossDefeated) gold += Mathf.RoundToInt(kit.Value(KitPassive.BossGold));
            if (kit.Has(KitPassive.WaveEndDice) && Next(ref State.ShopRandom, 6) == 5) { gold += Mathf.RoundToInt(kit.Value(KitPassive.WaveEndDice)); Notice = "The end-of-wave die shows a six: +" + Mathf.RoundToInt(kit.Value(KitPassive.WaveEndDice)) + " gold."; }
            int every = Mathf.RoundToInt(kit.Value(KitPassive.EggEveryWaves));
            if (every > 0 && State.Wave % every == 0) State.Eggs = BoundedCounter((long)State.Eggs + 1);
            return gold;
        }
        /// <summary>Bond counting extras: the largest bond, friends in two bonds, the lead friend, named bonds.</summary>
        private readonly Dictionary<string, int> kitBondAdded = new Dictionary<string, int>();
        /// <summary>Members the hero's kit added to each bond on top of the bodies counted (checks and tools).</summary>
        public int KitBondExtra(string bond) => kitBondAdded.TryGetValue(bond, out int n) ? n : 0;
        public static readonly string[] ThemeBonds = { "hearth", "archive", "clock", "frost", "orchard", "astral", "tidal", "oddity" };
        private void KitBonds()
        {
            kitBondAdded.Clear();
            HeroKit kit = Kit; if (kit == null) return;
            void Add(string id, int amount)
            {
                if (amount <= 0 || string.IsNullOrEmpty(id)) return;
                BondCounts[id] = (BondCounts.TryGetValue(id, out int c) ? c : 0) + amount;
                kitBondAdded[id] = (kitBondAdded.TryGetValue(id, out int a) ? a : 0) + amount;
            }
            if (kit.Has(KitPassive.BondDualPlusOne))
            {
                var bonds = new HashSet<string>();
                foreach (RosterEntry entry in State.Roster)
                {
                    if (Stranger(entry.UnitId)) continue;
                    var tags = catalog.Unit(entry.UnitId).Tags;
                    if (tags != null && tags.Length >= 2) foreach (string tag in tags) bonds.Add(tag);
                }
                foreach (string id in bonds) Add(id, 1);
            }
            if (kit.Has(KitPassive.BondLeadDouble) && State.HeroIdentity != null && !string.IsNullOrEmpty(State.HeroIdentity.LeadUnit) && State.Roster.Exists(e => e.UnitId == State.HeroIdentity.LeadUnit))
                foreach (string tag in catalog.Unit(State.HeroIdentity.LeadUnit).Tags ?? Array.Empty<string>()) Add(tag, 1);
            if (kit.Has(KitPassive.SignatureBondLevel))
            {
                // the hero's signature minor bond (its RecruitTheme that names a minor bond) counts one level higher
                string signature = Array.Find(catalog.Unit(State.ChampionId).RecruitThemes ?? Array.Empty<string>(), t => !t.StartsWith("fam-") && Array.IndexOf(ThemeBonds, t) < 0);
                BondDefinition bond = signature == null ? null : Array.Find(catalog.Bonds, b => b.Id == signature);
                if (bond != null && BondCounts.TryGetValue(signature, out int members) && members > 0) Add(signature, Mathf.Max(1, bond.Threshold));
            }
            if (kit.Has(KitPassive.BondPlusOneSprout) && BondCounts.ContainsKey("sprout")) Add("sprout", 1);
            if (kit.Has(KitPassive.BondPlusOneRegal) && BondCounts.ContainsKey("regal")) Add("regal", 1);
            if (kit.Has(KitPassive.BondBiggestPlusOne))
            {
                string biggest = null; int most = 0;
                foreach (var pair in BondCounts) if (pair.Value > most || pair.Value == most && string.CompareOrdinal(pair.Key, biggest) < 0) { most = pair.Value; biggest = pair.Key; }
                if (biggest != null && most > 0) Add(biggest, 1);
            }
        }
        /// <summary>Active bond levels (for TinVirtuoso's harmony).</summary>
        public int KitBondLevels()
        {
            int levels = 0;
            foreach (BondDefinition bond in catalog.Bonds) if (BondCounts.TryGetValue(bond.Id, out int count)) levels += bond.ActiveLevel(count);
            return levels;
        }
    }
}
