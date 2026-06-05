using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class RunSnapshot { public MetaRunState Meta = new MetaRunState(); }

    public sealed partial class CheckpointCatalog
    {
        /// <summary>The Waiting Room in battle: Second Candle (revive share, seconds untouchable), the gauge's head start and
        /// the hero's second habit. Zero for a run without meta.</summary>
        public float MetaCandleShare, MetaCandleShield, MetaGaugeStart;
        public bool MetaSecondHabit;
        /// <summary>Night Pact curses in battle: elites every wave (from wave 1, more often), bosses enraged from the start.</summary>
        public bool MetaPactElites, MetaPactEnrage;
    }

    /// <summary>The run side of the Waiting Room (play-test item 27): the loadout a run starts with, its stats folded into the
    /// run's bonuses, the Ledger's shop rules, the Nursery's friends and heirloom bond, the Clock's candle and rewind.</summary>
    public sealed partial class RunSession
    {
        private MetaRunState Meta => State?.Meta;

        /// <summary>Applies a loadout right after <see cref="Start"/> (before the first battle).</summary>
        public void ApplyMeta(MetaRunState meta)
        {
            if (State == null || meta == null) return;
            State.Meta = meta;
            State.Gold = BoundedCounter((long)State.Gold + Mathf.Max(0, meta.StartGold));
            UnitDefinition champion = catalog.Unit(State.ChampionId);
            if (champion.HeroMechanic != HeroMechanic.SoloMaw)
            {
                if (!string.IsNullOrEmpty(meta.AltStarter) && catalog.Unit(meta.AltStarter) != null && State.Roster.Count > 0)
                {
                    RosterEntry usual = State.Roster[0];
                    State.AcquiredVariants.RemoveAll(v => v.UnitId == usual.UnitId && v.WeaponId == usual.WeaponId && v.Tier == usual.Tier);
                    State.Roster.RemoveAt(0);
                    WeaponDefinition planned = HeroLoadout.Heroes.TryGetValue(champion.Id, out HeroLoadout.Entry plan) ? catalog.Weapon(plan.FriendWeapon) : null;
                    MetaAddFriend(meta.AltStarter, planned);
                }
                if (!string.IsNullOrEmpty(meta.StartFriend) && catalog.Unit(meta.StartFriend) != null && !OwnsCompanionSpecies(meta.StartFriend)) MetaAddFriend(meta.StartFriend, null);
            }
            // the Postbox's keepsake relic rides along from the first wave
            if (!string.IsNullOrEmpty(meta.Keepsake) && catalog.Item(meta.Keepsake) != null && StackCount(meta.Keepsake) == 0) State.Items.Add(new ItemStack { Id = meta.Keepsake, Count = 1 });
            RecountBonds();
            State.Health = MaximumHealth();
        }
        private void MetaAddFriend(string unitId, WeaponDefinition preferred)
        {
            if (preferred != null && RecruitWeaponEligible(unitId, preferred) && VariantFree(unitId, preferred))
            { var planned = new RosterEntry { UnitId = unitId, WeaponId = preferred.Id }; State.Roster.Add(planned); RememberVariant(planned); return; }
            var choices = new List<WeaponDefinition>();
            foreach (WeaponDefinition weapon in catalog.Weapons) if (RecruitWeaponEligible(unitId, weapon) && VariantFree(unitId, weapon)) choices.Add(weapon);
            if (choices.Count == 0) return;
            var entry = new RosterEntry { UnitId = unitId, WeaponId = BalancedWeapon(choices).Id };
            State.Roster.Add(entry); RememberVariant(entry);
        }

        /// <summary>The Lantern's stats and the Ledger's interest as run bonuses (they show in stats like any relic).</summary>
        private float MetaBonus(ItemEffect effect, ModifierScope target)
        {
            MetaRunState m = Meta; if (m == null) return 0;
            switch (effect)
            {
                case ItemEffect.HealthCorrection: return target == ModifierScope.Champion ? m.Health : 0;
                case ItemEffect.MoveSpeed: return target == ModifierScope.Champion ? m.MoveSpeed : 0;
                case ItemEffect.Crit: return target == ModifierScope.Champion || target == ModifierScope.Companions ? m.Crit : 0;
                case ItemEffect.Damage: return target == ModifierScope.Team || target == ModifierScope.Champion || target == ModifierScope.Companions ? m.Damage : 0; // the Museum
                case ItemEffect.Pickup: return target == ModifierScope.Run ? m.Pickup : 0;
                case ItemEffect.Interest: return target == ModifierScope.Run ? m.Interest : 0;
                case ItemEffect.ChestQuality: return target == ModifierScope.Run ? m.ChestQuality : 0; // Treasure nose
                default: return 0;
            }
        }
        private int MetaExtraSlots => Mathf.Clamp(Meta?.ExtraSlots ?? 0, 0, 1) - (MetaPact(MetaProgression.PactBareShelf) ? 1 : 0);
        /// <summary>A Night Pact curse taken for this run.</summary>
        public bool MetaPact(int pact) => Meta != null && (Meta.Pacts & (1 << pact)) != 0;
        /// <summary>Night Pact "Swift night": every enemy +15% speed.</summary>
        private float MetaEnemySpeed => MetaPact(MetaProgression.PactSwift) ? 1.08f : 1; // was 1.15 (owner 23/10/2026: no sudden speed-ups)
        /// <summary>Night Pact "Closed market": no dream market this run.</summary>
        private bool MetaNoDreamShop => MetaPact(MetaProgression.PactNoDream);
        /// <summary>The Museum's gold gift: a share more of the gold a fight brings in.</summary>
        private int MetaGold(int gold) => Meta == null || Meta.Gold <= 0 || gold <= 0 ? 0 : Mathf.RoundToInt(gold * Mathf.Min(Meta.Gold, MetaProgression.PowerCap / 100f));
        /// <summary>The Ledger's free look: the first refresh in every shop costs nothing.</summary>
        public bool MetaFreeRefresh => Meta != null && Meta.FreeRefresh && State.RerollsThisShop == 0;

        /// <summary>Heirloom: the chosen family's first bond needs one friend fewer.</summary>
        private void MetaBonds()
        {
            string id = Meta?.Heirloom; if (string.IsNullOrEmpty(id)) return;
            BondDefinition bond = Array.Find(catalog.Bonds, b => b.Id == id);
            if (bond != null && bond.Threshold >= 2 && BondCounts.TryGetValue(id, out int count) && count == bond.Threshold - 1) BondCounts[id] = count + 1;
        }

        /// <summary>The Stork: a bought one-star friend may arrive at two stars (same price; seeded by the offer, so a reload
        /// gives the same answer).</summary>
        private bool MetaStork(ShopOffer offer)
        {
            float chance = Meta?.TwoStarChance ?? 0;
            if (chance <= 0 || offer == null) return false;
            uint h = unchecked((uint)State.Seed * 2654435761u ^ (uint)offer.Token * 40503u ^ 0x570Cu);
            h ^= h >> 15; h *= 2246822519u; h ^= h >> 13;
            return h % 10000 < chance * 10000;
        }

        /// <summary>Gold-star welcome (Lamp Tree): the first friend bought in a run arrives one star up.</summary>
        private bool MetaWelcomeStar(RosterEntry recruited)
        {
            MetaRunState m = Meta;
            if (m == null || !m.FirstFriendStar || m.FirstFriendUsed || recruited.Tier >= 3) return false;
            m.FirstFriendUsed = true; recruited.Tier++;
            return true;
        }
        /// <summary>Rare find (Lamp Tree): the run's first shop holds a rare relic (or better) on its last relic stall.</summary>
        private void MetaRareFind(List<ItemDefinition> items)
        {
            MetaRunState m = Meta;
            if (m == null || !m.FirstShopRare || m.FirstShopDone) return;
            m.FirstShopDone = true;
            var rare = items.FindAll(i => i.Rarity >= ItemRarity.Rare);
            int slot = State.Offers.FindLastIndex(o => o.Kind == OfferKind.Item);
            if (rare.Count == 0 || slot < 0) return;
            ItemDefinition pick = rare[Next(ref State.ShopRandom, rare.Count)];
            ShopOffer offer = State.Offers[slot];
            float discount = Bonus(ItemEffect.Discount) + Bonus(ItemEffect.RelicDiscount);
            offer.Id = pick.Id; offer.Tier = 1;
            offer.Price = Mathf.Max(1, Mathf.CeilToInt(Mathf.CeilToInt(pick.Price * 0.85f * (1 + (int)pick.Rarity * 0.4f) * StagePriceFactor) * (1f - Mathf.Clamp(discount, -1, 0.5f))));
        }

        /// <summary>Battle fields of the loadout (called while the battle is compiled).</summary>
        private void PrepareMeta()
        {
            MetaRunState m = Meta; if (m == null || CombatCatalog == null) return;
            CombatCatalog.ChestGold *= 1 + Mathf.Max(0, m.ChestGold);
            bool candle = m.CandleShare > 0 && !m.CandleUsed;
            CombatCatalog.MetaCandleShare = candle ? m.CandleShare : 0;
            CombatCatalog.MetaCandleShield = candle ? m.CandleShield : 0;
            CombatCatalog.MetaGaugeStart = m.GaugeStart;
            CombatCatalog.MetaSecondHabit = m.SecondHabit;
            CombatCatalog.MetaPactElites = MetaPact(MetaProgression.PactElites);
            CombatCatalog.MetaPactEnrage = MetaPact(MetaProgression.PactEnrage);
        }
        private void MetaSettled(CombatSimulation simulation)
        {
            if (simulation.CandleBurned && Meta != null) Meta.CandleUsed = true;
        }

        /// <summary>Rewind keeps the shop the traveller is leaving (once a run, while it is unused).</summary>
        private void MetaRememberShop()
        {
            MetaRunState m = Meta;
            if (m == null || !m.RewindOwned || m.RewindUsed || State.Phase != SessionPhase.Shop) return;
            m.RewindShop = "";
            m.RewindShop = JsonUtility.ToJson(State);
            m.RewindAt = State.Wave + 1;
        }
        public bool RewindReady => State != null && State.Phase == SessionPhase.Defeat && Meta != null && Meta.RewindOwned && !Meta.RewindUsed && !string.IsNullOrEmpty(Meta.RewindShop);
        /// <summary>The wave Rewind replays (the one after its kept shop).</summary>
        public int RewindWave => RewindReady ? Meta.RewindAt : 0;
        /// <summary>Rewind: the lost run goes back to the shop before the wave it lost, once a run. The candle, the finds and the
        /// loadout carry over; the rewind is spent.</summary>
        public bool Rewind()
        {
            if (!RewindReady) return false;
            RunSnapshot shop;
            try { shop = JsonUtility.FromJson<RunSnapshot>(Meta.RewindShop); } catch { return false; }
            if (shop == null || shop.RunId != State.RunId || shop.Phase != SessionPhase.Shop) return false;
            MetaRunState carried = JsonUtility.FromJson<MetaRunState>(JsonUtility.ToJson(Meta));
            carried.RewindUsed = true; carried.RewindShop = ""; carried.HaulOil = carried.HaulKeepsakes = 0; carried.HaulLines = new List<string>();
            shop.Meta = carried;
            RunSnapshot lost = State;
            try { Restore(shop); }
            catch { State = lost; return false; }
            Notice = "Rewound to the shop before wave " + (State.Wave + 1) + ".";
            return true;
        }
    }

    /// <summary>Second Candle in battle: the first fall of the run is undone once, at a share of max HP, with a short spell of
    /// being untouchable at the Clock's last rank.</summary>
    public sealed partial class CombatSimulation
    {
        private bool candleBurned;
        private float candleSafeUntil = -1;
        public bool CandleBurned => candleBurned;
        public bool CandleSafe => candleSafeUntil > Elapsed;
        /// <summary>A boss fights enraged below half health, or from the start under the Night Pact's "Short temper".</summary>
        private bool BossEnraged(UnitState boss) => catalog.MetaPactEnrage || boss.Health < boss.Definition.Health * .5f;
        private bool SecondCandle(UnitState target)
        {
            if (target != Champion || candleBurned || catalog.MetaCandleShare <= 0) return false;
            candleBurned = true;
            float health = Mathf.Max(1, target.Definition.Health * catalog.MetaCandleShare);
            target.Health = health;
            candleSafeUntil = Elapsed + Mathf.Max(0, catalog.MetaCandleShield);
            events.Add(new CombatEvent(CombatEventKind.Heal, target.Position, target.Faction, health));
            return true;
        }
        /// <summary>Warm gauge (Wardrobe): every battle starts with a quarter of the gauge.</summary>
        private void MetaGaugeHeadStart()
        {
            if (!HeroIdentityEnabled || catalog.MetaGaugeStart <= 0 || HeroIdentityRequired <= 0) return;
            float tiers = Mathf.Clamp01(catalog.MetaGaugeStart) * 3;
            for (int i = 0; i < Mathf.FloorToInt(tiers); i++) AwakenClimb();
            identityCharge += (tiers - Mathf.FloorToInt(tiers)) * HeroIdentityRequired;
        }
    }
}
