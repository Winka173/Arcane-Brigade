namespace ArcaneBrigade
{
    /// <summary>All authored and draft balance numbers for heroes and personal traits live here.</summary>
    public static class HeroRulesV3Tuning
    {
        public const int HeroCount=36, TraitCount=16, BuildsPerHero=3, RelicsPerHero=4, ExtraMembership=1;
        public const float Radius=4, ArenaRadius=1000, LineWidth=.65f, LineLength=8, Damage=1, DefaultDuration=4;
        public const float StrideDistance=12, StrideRadius=2.4f, StrideDamage=.65f, StrideCooldown=4, StrideWider=1.3f, LampSeconds=4, HeldArmor=3;
        public const int ParcelCoins=6, ExpressCoins=5, DiceCoins=6, LoadedCoins=5, TideCoins=8, TaxCoins=10, StampCoins=8;
        public const float DreamStill=2, DreamRadius=3, DreamPeriod=7, DreamChill=2, DreamSleep=1, NightCapHeal=.02f;
        public const int DreamBullets=3, SilverBullets=4;
        public const float WardSlow=.3f, WardWider=1.25f, RootHealth=.15f, RootPickupHeal=.01f, LoneRecovery=.01f, RecoveryRelic=1.5f, AbsorbedPower=1.2f;
        public const float DiceCritPerCoin=.02f, HouseEdge=1.2f, TideHeal=3, TidePush=1, PushRelic=1.3f, TaxHeal=.03f, TaxExtraHeal=.02f;
        public const int LargeCrowd=10, BoilRelicCrowd=8, SproutCoins=8, SproutCap=3, SproutRelicCap=4, ThornCoins=12, ThornRelicCoins=10;
        public const float CometDistance=10, CometChargeRelic=.8f, CometBurn=3, InitialSpeed=.1f, InitialSpeedSeconds=10;
        public const float MirrorStill=2, MirrorHeal=.01f, StockPerGold=.001f, StockCap=.15f, SproutHeal=.03f;
        public const float BoilCharge=3, BoilDuration=5, BoilSpeed=.3f, BoilHeal=.01f, StringsRadius=4, SilkRadius=5, StringsReduction=.2f, SharedCap=.1f, PowderReduction=.05f;
        public const float SlideDistance=10, SlideFillRelic=1.25f, SlideSlow=.4f, SlideSeconds=3, SlideLengthRelic=1.5f, BlindSeconds=1;
        public const float RoyalDamage=.25f, RoyalRelicDamage=.3f, RoyalSeconds=8, RoyalExtraSeconds=4, RoyalRevive=5, RoyalExtraRevive=3, RoyalShield=.2f;
        public const float ChargePeriod=6, ChargeRelicPeriod=5, ChargeSpeed=.2f, ChargeSpeedSeconds=3, ChargeShield=.1f;
        public const int MawKills=20, MawRelicKills=15, EliteKills=5;
        public const float MawHeal=.01f, MawRelicHeal=.015f, MawGrowth=.05f, MawHighHealth=.7f, MawScarf=.1f, ThornStun=.3f;
        public const int ThornCoinEvery=5;
        public const float SignPeriod=10, SignSeconds=10, SignExtraSeconds=6, SignArmor=3, SignRadius=2;
        public const float DecreeDamage=.1f, DecreeEnemyReduction=.1f, PetalStill=2, PetalGrowRelic=1.25f, PetalSeconds=3, PatchCopy=.5f, PatchRelicCopy=.6f, PatchPeriod=8, PatchShieldExtra=3;
        public const float CatalogueDamage=.01f, CatalogueMark=.05f, CatalogueStun=1, Haggle=.1f, HaggleRelic=.15f, PurchaseDamage=.03f, PurchaseCap=.09f, LooseChance=.1f;
        public const float RiposteDelay=3, RiposteRelicDelay=2, RiposteSpeed=.1f, RiposteSpeedSeconds=4, BeatPeriod=3, BeatRelicPeriod=2.5f, BeatWindow=.2f, BeatDamage=.4f, FriendBeatDamage=.15f;
        public const int RiposteClusters=3, RiposteRelicClusters=4, BeatChainEvery=3;
        public const float ProwlStill=2, ProwlRelicStill=1.5f, ProwlDamage=3, ProwlCrit=.2f, ProwlStun=.5f;
        public const float SupperHeal=.1f, SupperRadiusRelic=1.3f, WeaveSeconds=4, WeaveRelicDuration=1.5f, WeaveSlow=.2f;
        public const float HourglassPeriod=15, HourglassRelicPeriod=12, HourglassSlow=.4f, HourglassRelicSlow=.55f, HourglassSeconds=3, HourglassSkill=.2f, HourglassStun=.5f;
        public const float RainDelay=5, RainSpeed=.1f, RainHeal=.01f, RainDamage=.05f, FleetPeriod=8, FleetRelicPeriod=6, FleetRelicLength=1.3f;
        public const float SundaePeriod=10, SundaeHeal=.1f, SundaeRelicHeal=.15f, SundaeFreeze=2, SundaeRadiusRelic=1.3f;
        public const float PruneSpeed=.02f, PruneRelicSpeed=.03f, PruneDamage=.1f, PruneSeconds=5, MossSpeed=.01f;
        public const int JuggleCap=3, JuggleRelicCap=4, JuggleStunBounce=5;
        public const float JugglePeriod=1, JuggleStun=.5f, JuggleHeal=.01f, DivePeriod=10, DiveSeconds=2, DiveRelicSeconds=3, DiveMarkedDamage=.08f, DiveShield=.1f;
        public const uint InitialRandom=0x7A11D923u; public const int RandomShiftA=13, RandomShiftB=17, RandomShiftC=5, DieFaces=6;
        public const int ReflectionCopies=2, PearlCoins=3, SnakeEyesEvery=36; public const float StringsCrit=.1f, CritMultiplier=2;
        public const int LooseEvery=10, ClusterSide=8, ClusterBins=64; public const float ClusterExtent=16, ClusterCell=4;
        public const int CoalsCap=12, CoilClusters=2, PiperStacks=3, BellEvery=3, FruitAge=3;
        public const float CoalPower=1.5f, WindLaneSeconds=6, PiperSpeed=.1f, PiperSeconds=6, FeatherHealth=.6f, FeatherDamage=.2f, FeatherSeconds=8, PitchRefill=.1f, BastionDuration=2;
        public const float SpiritPullRadius=6, TraitRingSeconds=6, SpotlightSeconds=3;
    }
}

