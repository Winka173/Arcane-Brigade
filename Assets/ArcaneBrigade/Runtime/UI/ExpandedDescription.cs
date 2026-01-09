namespace ArcaneBrigade
{
    public static class ExpandedDescription
    {
        public static string Describe(WeaponDefinition weapon, int tier)
        {
            string text = weapon.Family switch
            {
                ExpandedWeapon.Tea => "Tea splits into two droplets on impact, each dealing 35% damage without further splitting. The main droplet leaves a hot zone for 1.8 seconds, ticking every 0.4 seconds. Coasters widen but weaken the zone. Burning targets let hammers create steam.",
                ExpandedWeapon.TetherBow => "Arrows bounce up to " + (tier >= 2 ? 3 : 2) + " times between targets within 3 units; each bounce retains 70% damage. The first hit links nearby targets within 2 units for 25% damage per link. One projectile cannot hit the same target again.",
                ExpandedWeapon.Scissors => "Two narrow cuts 0.12 seconds apart, each dealing 65% damage. Position to land both; this is not a circular sweep.",
                ExpandedWeapon.Camera => "A flash cone hits up to " + (tier >= 2 ? 4 : 3) + " targets, exposing weak points for 2 seconds. Companion attacks gain 15% critical chance against those targets.",
                ExpandedWeapon.Fish => "Flies out, then returns after the first hit or 0.7 seconds on a miss. The return path can hit other targets; each target takes only one hit per throw, without doubled return damage.",
                ExpandedWeapon.Iron => "The forward strike leaves a hot trail " + (tier >= 2 ? "wider " : "") + "for 1.4 seconds, ticking every 0.4 seconds. Thermal Ink connects active marks.",
                ExpandedWeapon.Hammer => "Every third strike winds up 50% longer, has a 35% longer cycle and deals 55% area damage. Burning targets release extra steam for 30% damage.",
                ExpandedWeapon.Alarm => "Locks a floor position, telegraphs for 0.9 seconds, then detonates " + (tier >= 2 ? "wider" : "once") + ". Enemies can dodge; the shot does not track after launch.",
                ExpandedWeapon.MusicBox => "Soft notes home in; every third note extends active marks to at least 0.6 seconds. Scratch Record adds exactly one delayed echo.",
                _ => ""
            };
            return text + "\nSecondary effects cannot loop indefinitely. Attack beats reset each wave; no skill buttons or companion cap.";
        }
    }
}
