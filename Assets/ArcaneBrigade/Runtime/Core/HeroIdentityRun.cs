using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class RunSession
    {
        public HeroIdentityDefinition HeroIdentityDefinition => HeroIdentities.For(State.ChampionId);
        public bool CanConfigureHero => State.Phase == SessionPhase.Shop && !SecretRoomOpen && State.OutsideStage != OutsideStageStatus.Reward;
        public bool ChooseHeroApproach(int approach)
        {
            if (!CanConfigureHero || approach < 0 || approach > 2 || State.HeroIdentity.Approach == approach) return false;
            State.HeroIdentity.Approach = approach; Notice = HeroIdentityDefinition.Approaches[approach] + " ready for the next battle."; return true;
        }
        public bool CycleFeaturedFriend()
        {
            if (!CanConfigureHero || State.ChampionId != "PatchworkHost" || State.Roster.Count == 0) return false;
            var h = State.HeroIdentity; int index = State.Roster.FindIndex(r => r.UnitId == h.LeadUnit && r.WeaponId == h.LeadWeapon);
            RosterEntry next = State.Roster[(index + 1) % State.Roster.Count]; h.LeadUnit = next.UnitId; h.LeadWeapon = next.WeaponId;
            Notice = "Featured: " + catalog.Unit(next.UnitId).DisplayName + " / " + RunUICopy.WeaponName(next.WeaponId); return true;
        }
        public bool CycleMemoryOrgan(int slot)
        {
            if (!CanConfigureHero || State.ChampionId != "LoneJelly" || slot < 0 || slot >= 3) return false;
            var available = new List<string>(); foreach (string id in State.AbsorbedSkills) if (!available.Contains(id)) available.Add(id);
            if (available.Count == 0) return false;
            int current = available.IndexOf(State.HeroIdentity.Organs[slot]);
            for (int offset = 1; offset <= available.Count; offset++)
            {
                string next = available[(current + offset) % available.Count];
                if (Array.IndexOf(State.HeroIdentity.Organs, next) >= 0) continue;
                State.HeroIdentity.Organs[slot] = next; Notice = "Memory organ: " + catalog.Unit(next).DisplayName; return true;
            }
            return false;
        }
        public bool RerollHeroDie()
        {
            if (!CanConfigureHero || State.ChampionId != "GlassDice" || State.HeroIdentity.DiceRerollWave == State.Wave || State.Gold < 6) return false;
            State.Gold -= 6; State.HeroIdentity.DiceRerollWave = State.Wave;
            State.HeroIdentity.DiceFace = 1 + (State.HeroIdentity.DiceFace - 1 + 1 + Next(ref State.WeaponRandom, 5)) % 6;
            Notice = "Next die face: " + State.HeroIdentity.DiceFace; return true;
        }
        public int HeroOrderDeposit => (int)Math.Min(40, 8L + State.Wave);
        private UnitDefinition[] HeroNurseryPool(UnitDefinition[] pool)
        {
            if (State.ChampionId != "VerdantMatron") return pool;
            int approach = State.HeroIdentity.Approach;
            var preferred = Array.FindAll(pool, u => approach == 0 ? u.Passive == PassiveKind.HealChampion || Array.Exists(u.CompanionModifiers ?? Array.Empty<StatModifier>(), m => m.Effect == ItemEffect.Health || m.Effect == ItemEffect.Armor) : approach == 1 ? u.Weapon.Kind == WeaponKind.Melee : u.Passive != PassiveKind.None || (u.CompanionModifiers?.Length ?? 0) > 0);
            return preferred.Length > 0 ? preferred : pool;
        }
        public bool PlaceHeroOrder()
        {
            var h = State.HeroIdentity;
            if (!CanConfigureHero || State.ChampionId != "PennyBroker" || State.Wave == int.MaxValue || h.OrderPlacedWave == State.Wave || !string.IsNullOrEmpty(h.OrderItem) || State.Gold < HeroOrderDeposit) return false;
            var candidates = new List<ItemDefinition>();
            RelicCategory category = (RelicCategory)h.Approach;
            foreach (ItemDefinition i in catalog.Items)
                if (i.Category == category && i.UnlockWave <= (long)State.Wave + 1 && i.ShopWeight > 0 && ReviewBalance.ExclusivePool(i) == ShopPool.Gold && OctoberItemEligible(i) && StackCount(i.Id) < i.StackLimit) candidates.Add(i);
            if (candidates.Count == 0) { Notice = "No eligible forward order in this category."; return false; }
            ItemDefinition item = candidates[Next(ref State.ShopRandom, candidates.Count)];
            int basePrice = Mathf.CeilToInt(item.Price * .85f * (1 + (int)item.Rarity * .4f) * CarnivalBalance.ShopPrice(State.Wave + 1));
            h.OrderPrice = Mathf.Max(1, Mathf.CeilToInt(basePrice * (1 - Mathf.Clamp(Bonus(ItemEffect.Discount) + Bonus(ItemEffect.RelicDiscount), -1, .5f)) * (1 - KitValue(KitPassive.ForwardOrderDiscount))));
            h.OrderDeposit = HeroOrderDeposit; State.Gold -= h.OrderDeposit; h.OrderItem = item.Id;
            h.OrderPlacedWave = State.Wave; h.OrderWave = State.Wave + 1;
            Notice = "Ordered " + item.Name + ": " + h.OrderPrice + " gold on delivery. Investment " + h.OrderDeposit + " gold."; return true;
        }
        private void ApplyHeroOrder()
        {
            var h = State.HeroIdentity;
            if (State.ChampionId != "PennyBroker" || string.IsNullOrEmpty(h.OrderItem) || State.Wave < h.OrderWave) return;
            ItemDefinition item = catalog.Item(h.OrderItem);
            if (item == null || StackCount(item.Id) >= item.StackLimit)
            { State.Gold = BoundedCounter((long)State.Gold + h.OrderDeposit); h.OrderItem = ""; h.OrderDeposit = 0; Notice = "Forward order unavailable: investment refunded."; return; }
            if (State.Offers.Count == 0) return;
            var old = State.Offers[State.Offers.Count - 1];
            State.Offers[State.Offers.Count - 1] = new ShopOffer { Token = old.Token, Kind = OfferKind.Item, Id = h.OrderItem, Price = h.OrderPrice, Currency = ShopCurrency.Gold };
        }
        private void HeroIdentityBought(ShopOffer offer)
        {
            var h = State.HeroIdentity;
            if (State.ChampionId == "PennyBroker" && offer.Kind == OfferKind.Item && offer.Id == h.OrderItem && State.Wave >= h.OrderWave) { h.OrderItem = ""; h.OrderDeposit = 0; }
        }
        public string HeroIdentityShopSummary
        {
            get
            {
                var h = State.HeroIdentity; var d = HeroIdentityDefinition;
                if (d == null) return "";
                string result = d.Name + " / " + d.Approaches[h.Approach];
                if (State.ChampionId == "GlassDice") result += "\nNext face " + h.DiceFace + (h.DiceRerollWave == State.Wave ? " / reroll used" : " / reroll: 6 gold");
                if (State.ChampionId == "PatchworkHost") result += "\nFeatured: " + (string.IsNullOrEmpty(h.LeadUnit) ? "Automatic" : catalog.Unit(h.LeadUnit)?.DisplayName + " / " + RunUICopy.WeaponName(h.LeadWeapon));
                if (State.ChampionId == "PennyBroker") result += string.IsNullOrEmpty(h.OrderItem) ? "\nOrder investment: " + HeroOrderDeposit + " gold" : "\nDelivery: " + catalog.Item(h.OrderItem)?.Name + " / " + h.OrderPrice + " gold / wave " + h.OrderWave;
                return result;
            }
        }
        private void PrepareHeroIdentity()
        {
            var h = State.HeroIdentity;
            CombatCatalog.EnableHeroIdentities = catalog.HeroRevision > 0 && HeroIdentityDefinition != null;
            CombatCatalog.HeroApproach = h.Approach; CombatCatalog.HeroDiceFace = h.DiceFace;
            CombatCatalog.HeroFood = State.OctoberFood; CombatCatalog.HeroDrinks = State.OctoberDrinks; CombatCatalog.HeroWallet = State.Gold;
            CombatCatalog.HeroLeadUnit = h.LeadUnit; CombatCatalog.HeroLeadWeapon = h.LeadWeapon;
            CombatCatalog.HeroBondLevels = KitBondLevels();
            var organs = new List<UnitDefinition>();
            foreach (string id in h.Organs) if (!string.IsNullOrEmpty(id) && State.AbsorbedSkills.Contains(id)) organs.Add(catalog.Unit(id));
            for (int i = State.AbsorbedSkills.Count - 1; organs.Count < 3 && i >= 0; i--) { var u = catalog.Unit(State.AbsorbedSkills[i]); if (!organs.Contains(u)) organs.Add(u); }
            CombatCatalog.HeroOrgans = organs.ToArray();
            CombatCatalog.HeroPetals = new int[4];
            foreach (ItemStack stack in State.Items)
            {
                if (SecretItemSuspended(stack.Id)) continue;
                bool[] nourished = new bool[4];
                foreach (StatModifier m in catalog.Item(stack.Id).EffectiveModifiers) { int p = HeroIdentities.Petal(m.Effect); if (p >= 0 && m.Value > 0) nourished[p] = true; }
                for (int p = 0; p < 4; p++) if (nourished[p]) CombatCatalog.HeroPetals[p] = Mathf.Min(3, CombatCatalog.HeroPetals[p] + 1);
            }
        }
        private void ValidateHeroIdentity(RunSnapshot snapshot)
        {
            if (snapshot?.HeroIdentity == null) return; // Pre-identity saves get defaults on Restore.
            var h = snapshot.HeroIdentity;
            if (h.Revision != 1 || h.Approach < 0 || h.Approach > 2 || h.DiceFace < 1 || h.DiceFace > 6 || h.Organs == null || h.Organs.Length != 3 || h.DiceRerollWave < 0 || h.DiceRerollWave > snapshot.Wave || h.OrderPlacedWave < 0 || h.OrderPlacedWave > snapshot.Wave || h.OrderWave < 0 || (long)h.OrderWave > (long)snapshot.Wave + 1 || h.OrderPrice < 0 || h.OrderDeposit < 0 || h.OrderDeposit > 40) throw new InvalidOperationException("Invalid hero approach or forward order");
            var seen = new HashSet<string>();
            foreach (string id in h.Organs) if (!string.IsNullOrEmpty(id) && (catalog.Unit(id) == null || snapshot.AbsorbedSkills == null || !snapshot.AbsorbedSkills.Contains(id) || !seen.Add(id))) throw new InvalidOperationException("Invalid equipped hero memory");
            if (!string.IsNullOrEmpty(h.LeadUnit) && (!Array.Exists(catalog.Companions, u => u.Id == h.LeadUnit) || catalog.Weapon(h.LeadWeapon) == null)) throw new InvalidOperationException("Invalid featured performer");
            if (!string.IsNullOrEmpty(h.OrderItem) && (snapshot.ChampionId != "PennyBroker" || catalog.Item(h.OrderItem) == null || h.OrderPrice < 1 || h.OrderWave < 1 || h.OrderDeposit < 1)) throw new InvalidOperationException("Invalid forward order item");
        }
    }
}
