namespace ArcaneBrigade
{
    public enum RelicCategory { Curio, Drink, Food }
    public enum RelicMechanic
    {
        None, CrowdedTape, CoinMending, CompoundInterest, RetortVolley, ExposedRecovery,
        TreasureLease, PrismEcho, RerollFeather, ChestLightning, DrinkVitality, DrinkTempo,
        PanicMagnet, ChestLuck, ChaosIncome, HoardPower, HalfHealthSeed, StillRecovery,
        CoinLightning, TurnDiscount, CloseFollowers, WaveGrowth, MeleeToCollision,
        SlowToArmor, DodgeMending, DamageToIncome, ChestOnlyRecovery, SorrowPulse,
        BleedingAlarm, CollisionOnly, SoloOverdrive, FreshLotus, EggGift, CompanionGift,
        FragilePower, StatFloor, LuckToHealth, GambleRecovery, FoodGrowth, FreeFollowers,
        IncomePoison, HurtGrowth, StillPower, StaticFollowers, VoidPulse, SixWeapons,
        /// <summary>Content expansion step 3 (Docs/Content_Expansion_v5.md §5): Mythics that only added stats get a small mechanic of
        /// their own, and the four old Prismatics are redesigned around one.</summary>
        RoyalBlow, QuiltGuard, ApronShove, QuietMending, EchoForward, BlowerReturn, InvoiceSettles, SealBurst, SpringDouble,
        BorrowedCrown, ClockworkHeart, ImpossiblePrism, LivingThread,
        /// <summary>The thirty new items of the content expansion (Docs/Content_Expansion_v5.md §10; Fish Cracker is stats only).</summary>
        TallyChalk, CoinSash, PostageStamp, FrostSalt, RefundSlip, WhetstoneWafer, LateFee, HagglersAbacus, SlipstreamScarf, BrainFreeze, WindChime, SpecimenPin, StarPolish, KiteTailRibbon, MeltingSundae,
        CuckooPendulum, DivingBellLantern, TwinTicket, TreasureMimic, DuelistsRibbon, FrostLedger, UndertowAnchor, FunhouseMirror, SpliceKit, EndlessEncore, MoonClockKey, MirrorOfTwins, StorybookEnding, DreamLoom,
        /// <summary>The Lone line (owner 23/10/2026): hero-only power for a hero who fights alone or nearly so (RunRelics / LoneRelics).</summary>
        LoneLantern, EmptyChairs, SingleTicket, NightAlone, LastLight, IronResolve, SoloOverture
    }
}
