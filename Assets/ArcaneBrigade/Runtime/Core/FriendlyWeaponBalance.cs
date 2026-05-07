namespace ArcaneBrigade
{
    /// <summary>Shared hero/friend base-weapon tuning, before stars, rarity and build bonuses.
    /// Two-seed production fixtures at stars 1/2/3, wave 10; see Friendly_Scale_And_Damage_Audit.md.
    /// Keep weapon assets intact so swaps, upgrades and catalog builders share one rule.</summary>
    public static class FriendlyWeaponBalance
    {
        public static float Scale(string id)
        {
            switch (id)
            {
                case "TeapotPopgun": return .6f;
                case "BambooVase": return .65f;
                case "WateringCan": return .75f;
                case "MoonseedOrb": return .75f;
                case "DreamOrbStaff": return .8f;
                case "StampHammer": case "PizzaWheel": case "MaracaHammer": return .85f;
                case "PaperLanternBomb": case "ParasolNeedle": return .9f;
                default: return 1;
            }
        }
        public static float BaseDamage(WeaponDefinition weapon) => weapon.Damage * Scale(weapon.Id);
    }
}
