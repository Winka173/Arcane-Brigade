namespace ArcaneBrigade
{
    public static class RulesV3Tuning
    {
        public const int ConstructCapacity = 256, CopyCapacity = 128, RecruitPrice = 20, RelicPrice = 30, RelicWeight = 100;
        public const float ConstructTick = 1f, ConstructSeconds = 6f, ConstructRadius = 3f;
        public const float WaveSeconds = 100000f, MinimumHealth = 1f, PushDistance = 1f;
        public const float StatusSlow = .2f, MarkBonus = .12f, DotDamage = .1f;
        public const float ContactPadding = .05f, ContactWarning = .22f, ContactRetry = .15f;
        public const float Epsilon = .000001f, FeatherHealth = .4f;
        public const int SpawnBatch = 4, EliteEvery = 12, EliteFirstWave = 10;
        public const float SpawnEdgePadding = .5f, SpawnPeriodMinimum = .12f;
        public const int TurretUpgradeLimit=3;
        public const int PierceAll=10000;
        public const float TurretUpgradeDamage=.1f, AllyShield=.02f, RulePickupRadius=.75f;
        public const float SeedHeal=.1f;
        public const int SeedCoins=3;
        public const int CompanionLimit=10;
        public const float ChestTierChance=.1f;
    }
}
