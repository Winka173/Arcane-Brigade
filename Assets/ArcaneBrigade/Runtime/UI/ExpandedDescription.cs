namespace ArcaneBrigade
{
    public static class ExpandedDescription
    {
        public static string Describe(WeaponDefinition weapon, int tier)
        {
            if (weapon.Mechanic != CarnivalWeapon.None) return weapon.Mechanic switch
            {
                CarnivalWeapon.YoYo => tier >= 2 ? "Returning yo-yo. Pulls its victim; the string slows up to " + tier + " nearby foes." : "Returning yo-yo. Pulls and briefly slows its victim.",
                CarnivalWeapon.Sewing => tier >= 2 ? "Three piercing needles. Stitches " + (tier - 1) + " nearby foes for 18% damage each; no recursive chains." : "Three piercing needles. Briefly slows victims.",
                CarnivalWeapon.Projector => tier >= 2 ? "Lingering spotlight. Gathers and slows ordinary foes; bosses resist the pull." : "Lingering spotlight. Repeated area damage.",
                CarnivalWeapon.Magnet => "Fires a magnet bolt. The foe it strikes is yanked toward you and up to " + (2 + UnityEngine.Mathf.Clamp(tier, 1, 3)) + " neighbours are dragged in after it for 60% damage. Bosses resist the pull.",
                CarnivalWeapon.SpringHammer => "Charged splash and knockback. Crits and primary-hit relics apply.",
                _ => "Two crossing cuts. The follow-up moves with its wielder."
            };
            string text = weapon.Family switch
            {
                ExpandedWeapon.Tea => "Tea splits into two droplets on impact, each dealing 35% damage without further splitting. The main droplet leaves a hot zone for 1.8 seconds, ticking every 0.4 seconds. Coasters widen but weaken the zone. Burning targets let hammers create steam.",
                ExpandedWeapon.TetherBow => "Arrows bounce up to " + (tier >= 2 ? 3 : 2) + " times between targets within 3 units; each bounce retains 70% damage. The first hit links nearby targets within 2 units for 25% damage per link. One projectile cannot hit the same target again.",
                ExpandedWeapon.Scissors => "Two narrow cuts 0.12 seconds apart, each dealing 65% damage. Position to land both; this is not a circular sweep.",
                ExpandedWeapon.Camera => "A flash cone hits up to " + (tier >= 2 ? 4 : 3) + " targets, exposing weak points for 2 seconds. Friend attacks gain 15% critical chance against those targets.",
                ExpandedWeapon.Fish => "Flies out, then returns after the first hit or 0.7 seconds on a miss. The return path can hit other targets; each target takes only one hit per throw, without doubled return damage.",
                ExpandedWeapon.Iron => "The forward strike leaves a hot trail " + (tier >= 2 ? "wider " : "") + "for 1.4 seconds, ticking every 0.4 seconds. Thermal Ink connects active marks.",
                ExpandedWeapon.Hammer => "Every third strike winds up 50% longer, has a 35% longer cycle and deals 55% area damage. Burning targets release extra steam for 30% damage.",
                ExpandedWeapon.Alarm => "Locks a floor position, telegraphs for 0.9 seconds, then detonates " + (tier >= 2 ? "wider" : "once") + ". Enemies can dodge; the shot does not track after launch.",
                ExpandedWeapon.MusicBox => "Soft notes home in; every third note extends active marks to at least 0.6 seconds. Scratch Record adds exactly one delayed echo.",
                _ => ""
            };
            return text + "\nSecondary effects cannot loop indefinitely. Attack beats reset each wave; no skill buttons or friend cap.";
        }
    }
}
