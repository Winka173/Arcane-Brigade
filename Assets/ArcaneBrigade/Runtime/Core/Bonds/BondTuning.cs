namespace ArcaneBrigade
{
    /// <summary>Design v3 numbers; unspecified draft balance values live here too.</summary>
    public static partial class BondTuning
    {
        public const float HearthDouble = 2f;
        public const int BurnStacks = 3, BurnAfterBurst = 1, BurnSpreadStacks = 1, ChillStacks = 5, PoisonHitCap = 5, PoisonSpreadCap = 20, RotStacks = 8;
        public const float BurnSeconds = 4, BurnDps = .1f, BurstDamage = 1, BurstRadius = 2;
        public const float EmberSeconds = 4, PhoenixSeconds = 15, InfernoSeconds = 5;
        public const int InfernoHeat = 100;
        public const float InfernoMultiplier = 3, ChillSlow = .06f, FreezeSeconds = 1.5f, FrozenBonus = .25f;
        public const float ShatterDamage = 1.5f, IceSoldierSeconds = 12, BlizzardPeriod = 20;
        public const int IceSoldierLimit = 4, WinterStacks = 2;
        public const int StormEvery = 6, ChainTargets = 3, DischargeStacks = 5, ThunderHits = 20;
        /// <summary>The longest leap a storm chain makes from one enemy to the next.</summary>
        public const float ChainJump = 3.5f;
        public const float ChainDamage = .6f, DischargeDamage = .5f, UnderdogHealth = .5f, UnderdogBonus = .15f;
        public const float ThunderStun = 1, CloudPeriod = 2, CloudDamage = 1;
        public const float PoisonSeconds = 4, PoisonDps = .1f, RotMaxHealthDps = .03f, PlaguePeriod = 1, PlagueSlow = .2f;
        public const int DuskMarks = 10;
        public const float StrengthSteal = .12f, ExecuteHealth = .12f, ShadeSeconds = 12;
        public const float EclipseStart = 8, EclipsePeriod = 3.5f, EclipseFraction = .05f;
        public const float GritPerHit = .02f, GritCap = .1f, GritHighCap = .2f, GritIdle = 4;
        public const int HornEvery = 10, HornFourEvery = 8, HornSixEvery = 6, GearEvery = 6, GearFourEvery = 4;
        public const int RuneEvery = 10, RuneFourEvery = 8, RuneSixEvery = 6, ShantyEvery = 4, ShantyFourEvery = 3;
        public const float HornStun = .3f, HornPush = 1, RuneDamage = .8f, RuneInnerRadius = .5f, RuneRadius = 2;
        public const float ShantySpeed = .03f, ShantySeconds = 4;
        public const int MaskEvery = 5, MaskFourEvery = 4, MaskSixEvery = 3;
        public const float MaskMultiplier = 2, ComboPerLink = .01f, ComboCap = .1f, ComboHighCap = .2f;
        public const float StillSeconds = 2, LanternBonus = .2f, LanternRadius = 6, LitBonus = .08f, LighthouseBonus = .25f;
        public const float RootRegen = .2f, RootHighRegen = .4f, RootArmor = 2, RootHighArmor = 4;
        public const int CloverEvery = 20, CloverFourEvery = 12, SnackEvery = 15, SnackFourEvery = 10;
        public const float SnackHeal = .03f, ApplauseWindow = 5;
    }
}
