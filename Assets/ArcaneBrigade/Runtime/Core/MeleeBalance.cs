namespace ArcaneBrigade
{
    public static class MeleeBalance
    {
        public const float Damage = 1.5f;
        public const float Range = 1.35f;
        public const float Visual = 2.15f;
        public const float Speed = 1.15f; // was 1.3: melee friends ran a little too fast (owner, 17/10/2026)
        public const float RoamBonus = 2f;
        public static void Apply(WeaponDefinition weapon)
        {
            if (weapon.Kind != WeaponKind.Melee) return;
            weapon.Damage *= Damage; weapon.Range *= Range; weapon.VisualWidth *= Visual;
        }
    }
}
