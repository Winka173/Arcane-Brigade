using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public static class K11HeroRules
    {
        public static bool Eligible(UnitDefinition champion, UnitDefinition companion)
        {
            if (companion.GiftOnly) return false;
            if (champion.HeroMechanic == HeroMechanic.SoloMaw && champion.ReferenceHero != "Capped Gallant") return false;
            if (!string.IsNullOrEmpty(companion.ExclusiveHero)) return companion.ExclusiveHero == champion.ReferenceHero;
            // every hero, the host included, recruits only from its own themes: its fixed pool (Docs/Content_Expansion_Pools.md)
            // rival-bond rule (content expansion v5): on a shared theme pair the partner's signature friends (Common to Rare) stay out
            if (companion.Rarity <= ItemRarity.Rare && companion.Tags.Length > 1 && Array.IndexOf(champion.RecruitThemes, "rival-" + companion.Tags[1]) >= 0) return false;
            foreach (string theme in champion.RecruitThemes)
                if (Array.IndexOf(companion.Tags, theme) >= 0) return true;
            return false;
        }
        /// <summary>The hero's fixed bond pool (Docs/Content_Expansion_Pools.md): its theme bonds, then every minor bond that
        /// enough of the hero's own companions (painted ones) share to be activated.</summary>
        public static List<BondDefinition> PoolBonds(RunCatalog catalog, UnitDefinition champion)
        {
            var counts = new Dictionary<string, int>();
            foreach (UnitDefinition ally in catalog.Companions)
                if (ally.Body != null && Eligible(champion, ally))
                    foreach (string tag in ally.Tags) counts[tag] = counts.TryGetValue(tag, out int n) ? n + 1 : 1;
            var pool = new List<BondDefinition>();
            foreach (BondDefinition bond in catalog.Bonds) if (Array.IndexOf(champion.RecruitThemes, bond.Id) >= 0) pool.Add(bond);
            foreach (BondDefinition bond in catalog.Bonds)
                if (Array.IndexOf(champion.RecruitThemes, bond.Id) < 0 && counts.TryGetValue(bond.Id, out int n) && n >= bond.Threshold) pool.Add(bond);
            return pool;
        }
        public static RosterEntry Starter(RunCatalog catalog, UnitDefinition champion)
        {
            // each hero's own starting friend and its complementing weapon (HeroLoadout); the older themed pick below is the fallback
            if (HeroLoadout.Heroes.TryGetValue(champion.Id, out HeroLoadout.Entry plan) && catalog.Unit(plan.Friend) is UnitDefinition friend
                && catalog.Weapon(plan.FriendWeapon) != null && Eligible(champion, friend) && Array.IndexOf(ActOneCanon.StoryIds, friend.Id) < 0)
                return new RosterEntry { UnitId = plan.Friend, WeaponId = plan.FriendWeapon };
            string unit = champion.Id == "LanternKnight" ? "TeapotPilgrim" : champion.Id == "Postmaster" ? "BrassAcorn" : "HoodWisp";
            string weapon = champion.Id == "LanternKnight" ? "SpoonSabre" : champion.Id == "Postmaster" ? "InkPistol" : "SnowGlobeOrb";
            if (champion.HeroMechanic >= HeroMechanic.RootedOracle || !string.IsNullOrEmpty(champion.ReferenceHero) && champion.Id != "LanternKnight" && champion.Id != "Postmaster" && champion.Id != "DrowsySeer")
            {
                UnitDefinition candidate = Array.Find(catalog.Companions, ally => !ally.SpecialRecruit && Eligible(champion, ally));
                if (candidate == null) throw new InvalidOperationException("No themed starter: " + champion.Id);
                unit = candidate.Id; weapon = champion.Weapon.Kind == WeaponKind.Melee ? "SpoonSabre" : "InkPistol";
            }
            if (Array.IndexOf(ActOneCanon.StoryIds, unit) >= 0)
            {
                // Hob, Latch, Postscript and Tally travel as themselves; a second body of their kind in the brigade reads as a broken merge
                UnitDefinition candidate = Array.Find(catalog.Companions, ally => !ally.SpecialRecruit && Array.IndexOf(ActOneCanon.StoryIds, ally.Id) < 0 && Eligible(champion, ally));
                if (candidate != null) unit = candidate.Id;
            }
            if (catalog.Unit(unit) == null || catalog.Weapon(weapon) == null || !Eligible(champion, catalog.Unit(unit))) throw new InvalidOperationException("Invalid themed starting brigade: " + champion.Id);
            return new RosterEntry { UnitId = unit, WeaponId = weapon };
        }
        public static string Describe(UnitDefinition champion) => Describe(champion.HeroMechanic);
        public static string Describe(HeroMechanic mechanic)
        {
            return mechanic switch
            {
                HeroMechanic.LanternStride => "LANTERN STRIDE: travel 12 units to charge a melee pulse within 2.4 units; deals 65% weapon damage, minimum 4-second cooldown. Holds one pulse until an enemy is nearby. No charge in shops or while paused.",
                HeroMechanic.ParcelExpress => "PARCEL EXPRESS: collect 6 gold to charge an automatic parcel volley; deals 70% weapon damage, minimum 4-second cooldown. Passive gold and income do not count.",
                HeroMechanic.DreamBreath => "DREAM BREATH: stand still for 2 seconds to chill enemies within 3 units and remove up to 3 nearby hostile bullets; 7-second cooldown. Waits for nearby danger. Moving interrupts charge; grants no invulnerability.",
                HeroMechanic.RootedOracle => "Stationary in combat. Whole-arena pickup and a slowing ward. Walks in shops.",
                HeroMechanic.SoloMaw => "Travels without roster companions; absorbs purchased companions' traits. Strong attacks and natural recovery support relic-focused builds.",
                HeroMechanic.GlassGambler => "Collect 6 gold to burst nearby enemies. High luck and critical chance; low health.",
                HeroMechanic.TideCollector => "Collect 8 gold to heal 3 health and push nearby enemies back.",
                HeroMechanic.ClockRunner => "Travel 10 units to release a slowing pulse. Moving charges it; shops do not.",
                HeroMechanic.MirrorGuard => "Stand still for 2 seconds to reflect up to 3 nearby bullets. Moving resets charge.",
                HeroMechanic.TreasureTax => "Every 10 collected gold grants 1 extra gold and a small healing pulse.",
                _ => ""
            };
        }
    }
}
