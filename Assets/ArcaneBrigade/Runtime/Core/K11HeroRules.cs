using System;
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
            if (champion.ReferenceHero == "Belle") return true;
            foreach (string theme in champion.RecruitThemes)
                if (Array.IndexOf(companion.Tags, theme) >= 0) return true;
            return false;
        }
        public static RosterEntry Starter(RunCatalog catalog, UnitDefinition champion)
        {
            string unit = champion.Id == "LanternKnight" ? "TeapotPilgrim" : champion.Id == "Postmaster" ? "BrassAcorn" : "HoodWisp";
            string weapon = champion.Id == "LanternKnight" ? "SpoonSabre" : champion.Id == "Postmaster" ? "InkPistol" : "SnowGlobeOrb";
            if (champion.HeroMechanic >= HeroMechanic.RootedOracle || !string.IsNullOrEmpty(champion.ReferenceHero) && champion.Id != "LanternKnight" && champion.Id != "Postmaster" && champion.Id != "DrowsySeer")
            {
                UnitDefinition candidate = Array.Find(catalog.Companions, ally => !ally.SpecialRecruit && Eligible(champion, ally));
                if (candidate == null) throw new InvalidOperationException("No themed starter: " + champion.Id);
                unit = candidate.Id; weapon = champion.Weapon.Kind == WeaponKind.Melee ? "SpoonSabre" : "InkPistol";
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
                HeroMechanic.SoloMaw => "No companions. Growing attacks and an automatic whirl. Shops sell relics only.",
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
