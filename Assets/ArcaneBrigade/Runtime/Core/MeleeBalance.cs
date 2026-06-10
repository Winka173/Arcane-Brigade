namespace ArcaneBrigade
{
    public static class MeleeBalance
    {
        public const float Damage = 1.05f; // restore ~17% over 0.9, still 30% below the old 1.5 multiplier
        public const float Cooldown = 1.25f; // 20% fewer swings; preparation and recovery slow with the attack cycle
        public const float Range = 1.1f;  // was 1.35: melee reach felt too long (requirement 19/10/2026)
        /// <summary>Our weapons' area hits (bursts, fuses, echoes, puddles) are drawn and resolved at this share of their authored radius (requirement 19/10/2026).</summary>
        public const float Area = 0.8f;
        public const float Visual = 2.15f;
        public const float Speed = .8f; // 22/10 (later): .9 -> .8, melee friends run slower still (requirement); // 22/10: melee friends move ~22% slower than the previous 1.15; hero speed is unchanged.
        public const float RoamBonus = 2f;
        public static void Apply(WeaponDefinition weapon)
        {
            if (weapon.Kind != WeaponKind.Melee) return;
            weapon.Damage *= Damage; weapon.Cooldown *= Cooldown; weapon.Windup *= Cooldown; weapon.Recovery *= Cooldown;
            weapon.Range *= Range; weapon.VisualWidth *= Visual;
        }
    }
}
