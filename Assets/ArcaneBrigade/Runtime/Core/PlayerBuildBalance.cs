namespace ArcaneBrigade
{
    /// <summary>Hero offence is worth several ordinary bodies; relic investment can carry a smaller brigade.
    /// Applied to compiled damage, never to weapon assets or saved stacks.</summary>
    public static class PlayerBuildBalance
    {
        public static float HeroPower(string hero) => hero switch
        {
            "LoneJelly" => 6.5f,
            "GlassDice" => 3.6f,
            "RootOracle" => 3.6f,
            "PatchworkHost" => 4.5f,
            _ => 3f
        };
        public static float DamageSlope(bool companion) => companion ? .3f : .6f;
        public static float AttackSpeedSlope(bool companion) => companion ? .12f : .3f;
        public static float AttackSpeedCap(bool companion) => companion ? 2.2f : 4f;
    }
}
