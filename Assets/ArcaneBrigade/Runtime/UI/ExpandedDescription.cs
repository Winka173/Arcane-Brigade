namespace ArcaneBrigade
{
    public static class ExpandedDescription
    {
        public static string Describe(WeaponDefinition weapon, int tier)
        {
            string text = weapon.Family switch
            {
                ExpandedWeapon.Tea => "Tea impact leaves heat for 1.8s, ticking every 0.4s. Coaster widens but weakens the field. Burning targets feed Hammer steam.",
                ExpandedWeapon.TetherBow => "Arrow links up to " + (tier >= 2 ? 3 : 2) + " distinct nearby targets at 25% damage. Each link searches within 2 units, never loops.",
                ExpandedWeapon.Scissors => "Two narrow cuts, 0.12s apart, each 65% damage. Position to land both; no full-circle sweep.",
                ExpandedWeapon.Camera => "Cone flash hits at most " + (tier >= 2 ? 4 : 3) + " targets and exposes them for 2s. Allied attacks gain 15% crit chance on exposed targets.",
                ExpandedWeapon.Fish => "One outbound hit, then harmless boomerang return. Returns after 0.7s even on miss. No return damage duplication.",
                ExpandedWeapon.Iron => "Forward hit leaves a " + (tier >= 2 ? "wider " : "") + "hot strip for 1.4s. Heat ticks every 0.4s; Thermal Ink links existing marks.",
                ExpandedWeapon.Hammer => "Third hit has 50% longer windup and 35% longer cadence, then a 55% area impact. Burning targets release a separate 30% steam puff.",
                ExpandedWeapon.Alarm => "Locks a floor position, warns 0.9s, then one " + (tier >= 2 ? "wide " : "") + "blast. Moving enemies can evade it; no homing after launch.",
                ExpandedWeapon.MusicBox => "Soft homing notes; every third note extends a live mark to at least 0.6s. Scratch Record adds exactly one delayed echo.",
                _ => ""
            };
            return text + "\nSecondary effects are finite. Cadence resets each hall; no manual skill or ally cap.";
        }
    }
}
