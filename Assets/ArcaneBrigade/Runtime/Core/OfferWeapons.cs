using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class ShopOffer
    {
        /// <summary>A friend offer's weapon, rolled when the offer is first shown or bought (requirement 22/10/2026: the weapon is
        /// known before buying). Empty on offers saved before this: they roll on first use.</summary>
        public string WeaponId = "";
    }

    /// <summary>The weapon a friend offer comes with: rolled once per offer, deterministically from the run seed and the offer's
    /// token (it does not advance the run's weapon random, so inspecting never changes later rolls), with the same rules the
    /// purchase used to roll by (eligible, a variant the roster does not hold, melee / ranged balance, dream-shop specials).
    /// Buying uses exactly this weapon; a rerolled shop has new tokens, so new weapons.</summary>
    public sealed partial class RunSession
    {
        public WeaponDefinition OfferWeapon(ShopOffer offer)
        {
            if (offer == null || offer.Kind != OfferKind.Companion) return null;
            UnitDefinition unit = catalog.Unit(offer.Id);
            if (unit == null) return null;
            var choices = new List<WeaponDefinition>();
            foreach (WeaponDefinition weapon in catalog.Weapons)
                if (RecruitWeaponEligible(offer.Id, weapon) && VariantFree(offer.Id, weapon)) choices.Add(weapon);
            if (choices.Count == 0) return null;
            if (offer.Token < 0 && unit.Weapon != null && choices.Contains(unit.Weapon)) return unit.Weapon; // a gift carries its own weapon
            if (!string.IsNullOrEmpty(offer.WeaponId))
            {
                WeaponDefinition kept = choices.Find(weapon => weapon.Id == offer.WeaponId);
                if (kept != null) return kept;
            }
            uint random = OfferHash(offer);
            List<WeaponDefinition> pool = choices;
            if (catalog.HeroRevision > 0)
            {
                int melee = ActiveRoster.FindAll(entry => catalog.Weapon(entry.WeaponId).Kind == WeaponKind.Melee).Count;
                int ranged = ActiveRoster.FindAll(entry => catalog.Weapon(entry.WeaponId).Kind == WeaponKind.Projectile).Count;
                WeaponKind kind = melee == ranged ? Next(ref random, 2) == 0 ? WeaponKind.Melee : WeaponKind.Projectile : melee < ranged ? WeaponKind.Melee : WeaponKind.Projectile;
                List<WeaponDefinition> preferred = pool.FindAll(weapon => weapon.Kind == kind);
                if (preferred.Count > 0) pool = preferred;
            }
            if (unit.SpecialRecruit)
            {
                List<WeaponDefinition> special = pool.FindAll(weapon => weapon.SpecialOnly);
                if (special.Count > 0) pool = special;
            }
            WeaponDefinition chosen = pool[Next(ref random, pool.Count)];
            offer.WeaponId = chosen.Id;
            return chosen;
        }

        private uint OfferHash(ShopOffer offer)
        {
            unchecked
            {
                uint h = 2166136261u;
                void Mix(int value) { for (int i = 0; i < 4; i++) { h ^= (byte)(value >> (i * 8)); h *= 16777619u; } }
                Mix(State.Seed); Mix(offer.Token); Mix(State.Wave);
                foreach (char c in offer.Id ?? "") { h ^= c; h *= 16777619u; }
                return h == 0 ? 1u : h;
            }
        }

        /// <summary>The attack of exactly the weapon this offer comes with (one choice), built with the current run.</summary>
        public RecruitAttackReadout InspectOfferAttack(ShopOffer offer)
        {
            WeaponDefinition weapon = OfferWeapon(offer);
            return weapon == null ? InspectRecruitAttack(offer.Id, offer.Tier) : InspectRecruitAttack(offer.Id, offer.Tier, weapon);
        }
    }
}
