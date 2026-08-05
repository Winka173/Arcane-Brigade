namespace ArcaneBrigade
{
    public static class EnemyRulesV3Tuning
    {
        public const int KindCount=53, AffixCount=12, BossCount=6, MaxAffixes=3, MaxWarnings=512;
        public const int CrumbPackMin=30, CrumbPackMax=60, CrumbMerge=20, MaxSplitGeneration=2, SplitCount=2;
        public const int EggCount=3, SummonCount=6, SummonCap=6, AttackEvery=3, ChainTargets=4, Singers=3;
        public const int KitchenCount=12, CrumbWaveCount=30;
        public const float AuraTick=.5f, DotTick=1f, DefaultPeriod=6f, DefaultRadius=3f, DefaultWarning=1f;
        public const float DefaultStatus=3f, DefaultDamage=8f, DefaultHealth=100f, BossHealth=2500f;
        public const float DefaultSpeed=2f, DefaultRange=10f, DefaultCooldown=1f, DefaultRadiusBody=.25f;
        public const float BaseShield=.3f, RallierShield=.15f, Swift=.3f, Commanding=.1f, Armor=5f;
        public const float Vampire=.03f, Regeneration=.02f, Reflect=.2f, MirrorReflect=.3f, OutOfCombat=3f;
        public const float MarcherSpeed=.05f, MarcherCap=.3f, RankRadius=2f, GroupRadius=8f, BannerRadius=6f, BannerDamage=.2f;
        public const float GrowHealth=.1f, GrowDamage=.05f, StageSeconds=20f, Dizzy=1.5f, ChargeWidth=.7f;
        public const float SlamWarning=1f, SlamStun=.5f, SniperWarning=1f, MeteorWarning=1.5f;
        public const float HealFraction=.05f, BufferSpeed=.3f, BufferSeconds=4f, MournerSpeed=.4f, MournerSeconds=5f;
        public const float EggSeconds=5f, SilenceSeconds=3f, TimeRadius=6f, HalfSkill=.5f;
        public const float AmbushReveal=3f, BurstRadius=5f, FearSeconds=1f, LowHealth=.5f;
        public const float BinderShare=.4f, ReviveSeconds=5f, SummonSeconds=10f, MissChance=.3f, CharmSeconds=3f;
        public const float ArmorSeconds=4f, TideSlow=.3f, ChillSlow=.2f, TouchRadius=1f;
        public const float BlinkSeconds=6f, PoisonSeconds=5f, PoolSeconds=5f, DrainFraction=.03f;
        public const float ThiefRefund=1.1f, StealRadius=2f, BehindDistance=2f, PullDistance=1f;
        public const float FrontDot=0f, ReflectFrontDot=.2f, OneHitHealth=1f, ChargedScale=2f;
        public const float PastrySecond=.6f, PastryFinal=.25f, WardenSecond=.5f, WardenFinal=.2f;
        public const float StormSecond=.6f, StormFinal=.25f, OtherSecond=.5f, OtherFinal=.2f;
        public const float BossImmunity=5f, BossReturn=3f, BossCountdown=10f, SingTogetherSeconds=2f;
        public const float ChoirSleep=2f, SwallowSeconds=3f, QueenCharm=6f, BossRadius=5f, ArenaRadius=100f;
        public const float Epsilon=.0001f, ConeCosine=.5f, RingInnerFraction=.25f;
        public static readonly int[] BossFirstNights={10,20,25,35,45,55}, BossRepeatNights={30,40,50,60,70,80};
        public static readonly int[] Nights={1,1,1,1,1,1,1,1,1,1,1,5,8,10,10,10,12,12,14,14,15,16,16,18,18,20,22,22,24,24,26,26,28,28,30,30,32,34,36,40,40,42,44,46,48,50,55,60,65,30,30,40,40};
    }
}
