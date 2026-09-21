using UnityEngine;
namespace ArcaneBrigade
{
    public sealed class SkillProfile
    {
        public float Bar, Damage=1, Duration, Radius=3, Length=10, Status=3, Amount=.2f, Secondary, Heal, FlatHeal, Revive, Warning, Growth, WakeDamage;
        public int Friends=11, Repeats=1, Every=1, RandomSides, Economy, StackCap=5;
        public bool Self, Once, Cleanse;
        public SkillShape Shape=SkillShape.Disc;
        public SkillFriendSelection Selection=SkillFriendSelection.Nearest;
        public SkillEnemyEffect Enemy;
        public SkillFriendEffect Buff;
    }
    public static class CompanionSkillTuning
    {
        public const int HeldCastCapacity = 1024;
        public const int CompanionCount=120, ProtectionCount=27, MaxFriends=11, MaxZones=256, MaxCastsPerTick=4;
        public const int GridSize=33, GridHalf=16;
        public const float TickInterval=1, DefaultShieldDuration=6, CellSize=3, LineHalfWidth=.75f, ConeCosine=.5f, RingInnerFraction=.2f;
        public const float Epsilon=.0001f, ReturnPassDelay=.3f, CopyStrength=1, RandomUnitDamage=.25f, FallbackHeal=.02f;
        public const float CitrusBlind=.3f, CitrusBurst=2, RootDuration=4;
        public static readonly SkillProfile[] Profiles={
            new SkillProfile { Bar=CompanionClassTuning.ShooterBar, Duration=4f, Enemy=SkillEnemyEffect.Burn, Heal=0.02f, Selection=SkillFriendSelection.Hero }, // TeapotPilgrim
            new SkillProfile { Bar=CompanionClassTuning.ControllerBar, Enemy=SkillEnemyEffect.Burn | SkillEnemyEffect.Slow, Duration=4f }, // CandleSnail
            new SkillProfile { Bar=CompanionClassTuning.SupportBar, Buff=SkillFriendEffect.Damage, Amount=0.2f, Duration=6f, Friends=3, Damage=1f }, // MailboxImp
            new SkillProfile { Bar=CompanionClassTuning.GuardBar, Buff=SkillFriendEffect.DamageReduction, Amount=0.4f, Duration=3f, Self=true, Enemy=SkillEnemyEffect.Taunt, Damage=1f }, // BreadCrab
            new SkillProfile { Bar=CompanionClassTuning.SupportBar, Enemy=SkillEnemyEffect.Burn, RandomSides=6 }, // DiceCrab
            new SkillProfile { Bar=CompanionClassTuning.StrikerBar, Enemy=SkillEnemyEffect.Burn, Duration=4f }, // MaskPepper
            new SkillProfile { Bar=CompanionClassTuning.ControllerBar, Enemy=SkillEnemyEffect.Stun, Status=0.8f, Repeats=3 }, // CoilKettle
            new SkillProfile { Bar=CompanionClassTuning.GuardBar, Enemy=SkillEnemyEffect.Push | SkillEnemyEffect.Lit, Warning=1f }, // SaffronMechanist
            new SkillProfile { Bar=CompanionClassTuning.StrikerBar, Enemy=SkillEnemyEffect.Slow, Repeats=2, Heal=0.03f }, // BakerMouse
            new SkillProfile { Bar=CompanionClassTuning.ShooterBar, Duration=2f, Enemy=SkillEnemyEffect.Burn, FlatHeal=1f, Shape=SkillShape.Cone }, // TeaDragon
            new SkillProfile { Bar=CompanionClassTuning.ControllerBar, Enemy=SkillEnemyEffect.Freeze, Status=2f, Every=3, Heal=0.08f }, // SundaeGolem
            new SkillProfile { Bar=CompanionClassTuning.SummonerBar, Duration=5f }, // BobaTadpole
            new SkillProfile { Bar=CompanionClassTuning.SupportBar, Enemy=SkillEnemyEffect.Mark, Amount=0.15f, Status=5f, Shape=SkillShape.Cone, Damage=0f }, // SnowSpoon
            new SkillProfile { Bar=CompanionClassTuning.GuardBar, Buff=SkillFriendEffect.ShieldFraction, Amount=0.12f, Duration=5f, Friends=3, Enemy=SkillEnemyEffect.Chill, Damage=0f }, // FridgeWisp
            new SkillProfile { Bar=CompanionClassTuning.ShooterBar, Enemy=SkillEnemyEffect.Soak }, // ShellPostman
            new SkillProfile { Bar=CompanionClassTuning.SupportBar, Heal=0.08f, Damage=0f, Economy=1 }, // CoralTeacup
            new SkillProfile { Bar=CompanionClassTuning.ControllerBar, Enemy=SkillEnemyEffect.Pull | SkillEnemyEffect.Soak, Shape=SkillShape.Line }, // AnchorSnail
            new SkillProfile { Bar=CompanionClassTuning.ControllerBar, Enemy=SkillEnemyEffect.Weaken | SkillEnemyEffect.ReduceHealing, Amount=0.15f, Status=5f, Secondary=0.5f, Damage=0f }, // SailSquid
            new SkillProfile { Bar=CompanionClassTuning.StrikerBar, Duration=5f, Enemy=SkillEnemyEffect.Slow, Shape=SkillShape.Line }, // RainbowTailor
            new SkillProfile { Bar=CompanionClassTuning.GuardBar, Enemy=SkillEnemyEffect.Push | SkillEnemyEffect.Freeze | SkillEnemyEffect.Soak, Warning=1f }, // BalloonMallet
            new SkillProfile { Bar=CompanionClassTuning.ShooterBar, Enemy=SkillEnemyEffect.Freeze, Status=2.5f }, // ShovelPenguin
            new SkillProfile { Bar=CompanionClassTuning.ShooterBar, Buff=SkillFriendEffect.AttackSpeed, Amount=0.15f, Duration=6f, Damage=0f }, // ShantySeaLion
            new SkillProfile { Bar=CompanionClassTuning.SummonerBar, Enemy=SkillEnemyEffect.Soak, Radius=5f }, // BottleGull
            new SkillProfile { Bar=CompanionClassTuning.SummonerBar, Duration=6f, Repeats=2, Enemy=SkillEnemyEffect.Slow }, // SpecimenSlime
            new SkillProfile { Bar=CompanionClassTuning.ControllerBar, Duration=3f, Enemy=SkillEnemyEffect.Push }, // KettleBat
            new SkillProfile { Bar=CompanionClassTuning.SupportBar, Buff=SkillFriendEffect.SkillSpeed, Amount=0.2f, Duration=5f, Damage=0f, Shape=SkillShape.Line }, // CometKite
            new SkillProfile { Bar=CompanionClassTuning.StrikerBar, Duration=5f, Shape=SkillShape.Line }, // SecretPaperBird
            new SkillProfile { Bar=CompanionClassTuning.GuardBar, Buff=SkillFriendEffect.Retaliate, Amount=1f, Duration=5f, Damage=0f, Selection=SkillFriendSelection.Hero }, // RibbonOtter
            new SkillProfile { Bar=CompanionClassTuning.SupportBar, Buff=SkillFriendEffect.ShieldFraction, Amount=0.08f, Secondary=0.1f, Duration=6f, Friends=4, Damage=0f }, // SpindleWren
            new SkillProfile { Bar=CompanionClassTuning.ShooterBar, Enemy=SkillEnemyEffect.Soak, Shape=SkillShape.Line }, // RainHeron
            new SkillProfile { Bar=CompanionClassTuning.SummonerBar, Repeats=4, Duration=4f }, // PaperPlanePigeon
            new SkillProfile { Bar=CompanionClassTuning.GuardBar, Duration=5f, Enemy=SkillEnemyEffect.Slow | SkillEnemyEffect.Soak, Amount=0.2f }, // RaincoatFrog
            new SkillProfile { Bar=CompanionClassTuning.ShooterBar, Duration=3f, Radius=5f, Shape=SkillShape.Ring }, // KiteDragon
            new SkillProfile { Bar=CompanionClassTuning.StrikerBar, Repeats=5, Enemy=SkillEnemyEffect.Charge }, // StaticFerret
            new SkillProfile { Bar=CompanionClassTuning.ShooterBar, Duration=4f, Enemy=SkillEnemyEffect.Charge, Growth=0.5f }, // StormKitten
            new SkillProfile { Bar=CompanionClassTuning.ControllerBar, Duration=4f, Enemy=SkillEnemyEffect.Charge }, // VoltEel
            new SkillProfile { Bar=CompanionClassTuning.StrikerBar, Enemy=SkillEnemyEffect.Poison, Shape=SkillShape.Line }, // PearMoth
            new SkillProfile { Bar=CompanionClassTuning.ControllerBar, Enemy=SkillEnemyEffect.Silence | SkillEnemyEffect.Poison, Status=3f, Damage=0f }, // MushroomLibrarian
            new SkillProfile { Bar=CompanionClassTuning.SupportBar, RandomSides=4 }, // CitrusToad
            new SkillProfile { Bar=CompanionClassTuning.GuardBar, Enemy=SkillEnemyEffect.Root, Status=3f, Duration=5f }, // SeedHedgehog
            new SkillProfile { Bar=CompanionClassTuning.StrikerBar, Enemy=SkillEnemyEffect.Stun | SkillEnemyEffect.Poison, Status=0.5f, Repeats=3 }, // SockCyclops
            new SkillProfile { Bar=CompanionClassTuning.SummonerBar, Buff=SkillFriendEffect.ReflectHits, Amount=6f, Duration=10f, Damage=0f }, // MirrorNewt
            new SkillProfile { Bar=CompanionClassTuning.StrikerBar, Enemy=SkillEnemyEffect.Poison | SkillEnemyEffect.Slow, Duration=5f, StackCap=8 }, // RibbonSpider
            new SkillProfile { Bar=CompanionClassTuning.GuardBar, Buff=SkillFriendEffect.Reflect, Amount=0.3f, Duration=4f, Self=true, Enemy=SkillEnemyEffect.Taunt }, // GrandSprout
            new SkillProfile { Bar=CompanionClassTuning.ControllerBar, Duration=4f, Enemy=SkillEnemyEffect.Poison, Heal=0.01f, Shape=SkillShape.Ring }, // PaperDevil
            new SkillProfile { Bar=CompanionClassTuning.SupportBar, Duration=5f, Enemy=SkillEnemyEffect.Slow, Damage=0f }, // WateringSnail
            new SkillProfile { Bar=CompanionClassTuning.ShooterBar, Repeats=2, Enemy=SkillEnemyEffect.Poison }, // YoyoSquirrel
            new SkillProfile { Bar=CompanionClassTuning.ShooterBar, Duration=6f }, // FirstSeed
            new SkillProfile { Bar=CompanionClassTuning.GuardBar, Enemy=SkillEnemyEffect.Sleep | SkillEnemyEffect.DuskMark, Status=1.5f, Warning=0f, Radius=6f, WakeDamage=2f }, // PillowOracle
            new SkillProfile { Bar=CompanionClassTuning.ControllerBar, Enemy=SkillEnemyEffect.AttackSlow, Amount=0.3f, Status=5f, Shape=SkillShape.Cone, Damage=0f }, // IceCreamOwl
            new SkillProfile { Bar=CompanionClassTuning.ShooterBar, Duration=3f, Enemy=SkillEnemyEffect.Weaken, Amount=0.15f }, // MoonJelly
            new SkillProfile { Bar=CompanionClassTuning.ShooterBar, Warning=1.0f, Damage=2f }, // StarSock
            new SkillProfile { Bar=CompanionClassTuning.SupportBar, Duration=3600f }, // EclipseEgg
            new SkillProfile { Bar=CompanionClassTuning.SummonerBar, Duration=4f, Repeats=4 }, // OrbitMoth
            new SkillProfile { Bar=CompanionClassTuning.ControllerBar, Enemy=SkillEnemyEffect.Slow, Amount=0.8f, Status=3f, Duration=3f, Damage=0f }, // BubbleKoi
            new SkillProfile { Bar=CompanionClassTuning.SupportBar, Heal=0.15f, Friends=3, Selection=SkillFriendSelection.LowestHealth, Revive=6f, Damage=0f }, // SecretPatchBear
            new SkillProfile { Bar=CompanionClassTuning.ShooterBar, Duration=5f, Enemy=SkillEnemyEffect.Blind, Amount=0.3f, Damage=0f }, // SecretDreamLamb
            new SkillProfile { Bar=CompanionClassTuning.GuardBar, Buff=SkillFriendEffect.DamageReduction, Amount=0.5f, Duration=3f, Self=true, Enemy=SkillEnemyEffect.Taunt, Heal=0.02f }, // VioletRemnant
            new SkillProfile { Bar=CompanionClassTuning.SupportBar, Buff=SkillFriendEffect.ShieldFraction, Amount=0.1f, Secondary=0.1f, Duration=6f, Damage=0f }, // LittleSprout
            new SkillProfile { Bar=CompanionClassTuning.StrikerBar, Buff=SkillFriendEffect.ShieldFraction, Amount=0.08f, Friends=1, Selection=SkillFriendSelection.LowestHealth, Damage=0f }, // SaddleMoth
            new SkillProfile { Bar=CompanionClassTuning.StrikerBar, Enemy=SkillEnemyEffect.Push | SkillEnemyEffect.Slow, Shape=SkillShape.Cone }, // JellySwordsman
            new SkillProfile { Bar=CompanionClassTuning.GuardBar, Enemy=SkillEnemyEffect.Push, Shape=SkillShape.Line }, // BrassAcorn
            new SkillProfile { Bar=CompanionClassTuning.SupportBar, Enemy=SkillEnemyEffect.Stun, Status=1f }, // ButtonBeetle
            new SkillProfile { Bar=CompanionClassTuning.SupportBar, RandomSides=9 }, // RadishJester
            new SkillProfile { Bar=CompanionClassTuning.ControllerBar, Duration=3f, Enemy=SkillEnemyEffect.Root, Damage=0f }, // CrayonGremlin
            new SkillProfile { Bar=CompanionClassTuning.ShooterBar, Buff=SkillFriendEffect.BarRefund, Amount=0.25f, Friends=3, Damage=0f }, // ReturnCaptain
            new SkillProfile { Bar=CompanionClassTuning.GuardBar, Enemy=SkillEnemyEffect.Push, Warning=1f }, // DoughForeman
            new SkillProfile { Bar=CompanionClassTuning.ControllerBar, Enemy=SkillEnemyEffect.Weaken, Amount=1f, Status=2f, Damage=0f }, // VelvetEnchantress
            new SkillProfile { Bar=CompanionClassTuning.StrikerBar, Enemy=SkillEnemyEffect.Stun, Status=1f }, // CarouselPony
            new SkillProfile { Bar=CompanionClassTuning.ShooterBar, Enemy=SkillEnemyEffect.Pull, Duration=3f, Damage=0f }, // JackInTheBox
            new SkillProfile { Bar=CompanionClassTuning.SummonerBar, Duration=6f }, // LostCarousel
            new SkillProfile { Bar=CompanionClassTuning.ShooterBar, Economy=1 }, // CourtHerald
            new SkillProfile { Bar=CompanionClassTuning.ShooterBar, Enemy=SkillEnemyEffect.Pull | SkillEnemyEffect.Lit, Radius=8f, Duration=2f, Damage=0f }, // HoodWisp
            new SkillProfile { Bar=CompanionClassTuning.SupportBar, Enemy=SkillEnemyEffect.Mark, Amount=0.12f, Status=3600f, Shape=SkillShape.Species, Damage=0f }, // BookmarkGolem
            new SkillProfile { Bar=CompanionClassTuning.SupportBar, Duration=2f, Enemy=SkillEnemyEffect.Slow }, // ThimbleMoth
            new SkillProfile { Bar=CompanionClassTuning.ShooterBar, Repeats=3 }, // ReelOwl
            new SkillProfile { Bar=CompanionClassTuning.GuardBar, Buff=SkillFriendEffect.ShareDamage, Amount=0.4f, Duration=6f, Selection=SkillFriendSelection.Strongest, Friends=1, Damage=0f }, // VelvetChampion
            new SkillProfile { Bar=CompanionClassTuning.GuardBar, Buff=SkillFriendEffect.ShieldFraction, Amount=0.1f, Duration=3f, Friends=3, Damage=0f }, // TeacupCricket
            new SkillProfile { Bar=CompanionClassTuning.ShooterBar, Repeats=3, Damage=0.5f }, // LensOwl
            new SkillProfile { Bar=CompanionClassTuning.ControllerBar, Damage=1f }, // EndlessIndex
            new SkillProfile { Bar=CompanionClassTuning.GuardBar, Duration=5f, Enemy=SkillEnemyEffect.Weaken | SkillEnemyEffect.Lit, Amount=0.2f }, // LighthouseWhale
            new SkillProfile { Bar=CompanionClassTuning.SummonerBar, Enemy=SkillEnemyEffect.Root, Status=1f }, // PinnedBeetle
            new SkillProfile { Bar=CompanionClassTuning.SupportBar, Buff=SkillFriendEffect.ShieldFraction, Amount=0.1f, Duration=5f, Damage=0f, Shape=SkillShape.Line }, // PaperInventor
            new SkillProfile { Bar=CompanionClassTuning.StrikerBar, Buff=SkillFriendEffect.SkillSpeed, Amount=0.15f, Duration=6f, Damage=0f }, // LampGoat
            new SkillProfile { Bar=CompanionClassTuning.StrikerBar, Enemy=SkillEnemyEffect.Push, Shape=SkillShape.Line, Length=24f }, // IronLocomotive
            new SkillProfile { Bar=CompanionClassTuning.GuardBar, Duration=6f, Damage=0f, Shape=SkillShape.Line }, // FossilArmadillo
            new SkillProfile { Bar=CompanionClassTuning.ControllerBar, Warning=3f, Damage=2f }, // TinTick
            new SkillProfile { Bar=CompanionClassTuning.SupportBar, Enemy=SkillEnemyEffect.Push | SkillEnemyEffect.Stun, Status=1f }, // SpringDuck
            new SkillProfile { Bar=CompanionClassTuning.SupportBar, Buff=SkillFriendEffect.ReviveNow, Amount=0.2f, Once=true, Selection=SkillFriendSelection.Fallen, Friends=1, Damage=0f }, // StapleMole
            new SkillProfile { Bar=CompanionClassTuning.ShooterBar, Economy=20, Damage=0f }, // DreamCompass
            new SkillProfile { Bar=CompanionClassTuning.ShooterBar, Every=3, Damage=3f, Shape=SkillShape.Ring }, // RainbowBell
            new SkillProfile { Bar=CompanionClassTuning.SummonerBar, Buff=SkillFriendEffect.SkillSpeed, Amount=0.2f, Duration=6f, Damage=0f }, // ClockworkPiper
            new SkillProfile { Bar=CompanionClassTuning.GuardBar, Buff=SkillFriendEffect.DamageReduction, Amount=0.5f, Secondary=0.3f, Duration=3f, Self=true, Enemy=SkillEnemyEffect.Taunt }, // BristleBall
            new SkillProfile { Bar=CompanionClassTuning.ControllerBar, Enemy=SkillEnemyEffect.Pull, Duration=1.5f }, // MonsterMarble
            new SkillProfile { Bar=CompanionClassTuning.StrikerBar, Repeats=5 }, // TopBeetle
            new SkillProfile { Bar=CompanionClassTuning.GuardBar, Buff=SkillFriendEffect.ShieldFraction, Amount=0.1f, Friends=5, Damage=0f }, // StitchedPangolin
            new SkillProfile { Bar=CompanionClassTuning.GuardBar, Duration=6f, Friends=4, Selection=SkillFriendSelection.Danger, Damage=0f }, // TowerRook
            new SkillProfile { Bar=CompanionClassTuning.GuardBar, Buff=SkillFriendEffect.ShieldFraction, Amount=0.1f, Friends=1, Selection=SkillFriendSelection.LowestHealth, Revive=7f, Damage=0f }, // LifebuoyHippo
            new SkillProfile { Bar=CompanionClassTuning.SupportBar, Heal=0.2f, Friends=3, Selection=SkillFriendSelection.LowestHealth, Revive=5f, Cleanse=true, Damage=0f }, // NurseHen
            new SkillProfile { Bar=CompanionClassTuning.SupportBar, Buff=SkillFriendEffect.RebirthFeather, Amount=0.3f, Once=true, Damage=1f }, // PhoenixChick
            new SkillProfile { Bar=CompanionClassTuning.SupportBar, Heal=0.1f, Duration=5f, Friends=4, Revive=5f, Damage=0f }, // DandelionPuff
            new SkillProfile { Bar=CompanionClassTuning.SummonerBar, Buff=SkillFriendEffect.DamageReduction, Amount=0.2f, Duration=6f, Radius=5f, Secondary=0.3f, Damage=0f }, // TotemPole
            new SkillProfile { Bar=CompanionClassTuning.GuardBar, Buff=SkillFriendEffect.DamageReduction, Amount=0.2f, Duration=5f, Friends=5, Enemy=SkillEnemyEffect.Push, Shape=SkillShape.Cone }, // BrassLion
            new SkillProfile { Bar=CompanionClassTuning.GuardBar, Buff=SkillFriendEffect.ShieldFraction, Amount=0.15f, Duration=6f, Friends=5, Damage=0f }, // QuiltWalrus
            new SkillProfile { Bar=CompanionClassTuning.SupportBar, Heal=0.12f, Damage=0f }, // PlasterPanda
            new SkillProfile { Bar=CompanionClassTuning.StrikerBar, Radius=3f, Heal=0.02f }, // PumpkinReaper
            new SkillProfile { Bar=CompanionClassTuning.ShooterBar, Repeats=3 }, // BoneXylophone
            new SkillProfile { Bar=CompanionClassTuning.ControllerBar, Enemy=SkillEnemyEffect.Fear, Status=1.5f, Secondary=0.03f, Shape=SkillShape.Line }, // ScarecrowCrow
            new SkillProfile { Bar=CompanionClassTuning.SupportBar, Buff=SkillFriendEffect.OnBeatDamage, Amount=0.2f, Duration=6f, Damage=0f }, // Metronome
            new SkillProfile { Bar=CompanionClassTuning.SupportBar, Heal=0.08f, Duration=4f, Enemy=SkillEnemyEffect.Weaken, Amount=0.15f, Damage=0f, Shape=SkillShape.Ring }, // HarpSwan
            new SkillProfile { Bar=CompanionClassTuning.ControllerBar, Enemy=SkillEnemyEffect.Stun | SkillEnemyEffect.Weaken, Status=1f, Amount=0.1f, Duration=5f, Shape=SkillShape.Ring, Damage=0f }, // BagpipeOctopus
            new SkillProfile { Bar=CompanionClassTuning.GuardBar, Buff=SkillFriendEffect.DamageReduction, Amount=0.25f, Duration=4f, Enemy=SkillEnemyEffect.Stun, Status=1f }, // AnvilOx
            new SkillProfile { Bar=CompanionClassTuning.ControllerBar, Enemy=SkillEnemyEffect.Pull | SkillEnemyEffect.Slow, Amount=0.4f, Duration=3f }, // SinkholeWorm
            new SkillProfile { Bar=CompanionClassTuning.SummonerBar, Duration=6f, Buff=SkillFriendEffect.AbsorbProjectiles, Amount=1f, Self=true }, // PlanetHamster
            new SkillProfile { Bar=CompanionClassTuning.StrikerBar, Enemy=SkillEnemyEffect.Stun, Status=0.5f, Shape=SkillShape.Ring }, // CymbalMonkey
            new SkillProfile { Bar=CompanionClassTuning.SummonerBar, Repeats=2 }, // ChoirCanary
            new SkillProfile { Bar=CompanionClassTuning.ControllerBar, Duration=4f, Shape=SkillShape.Line }, // PianoCrocodile
            new SkillProfile { Bar=CompanionClassTuning.ShooterBar, Repeats=3 }, // NewtonApple
            new SkillProfile { Bar=CompanionClassTuning.GuardBar, Buff=SkillFriendEffect.ShieldFraction, Amount=0.06f, Enemy=SkillEnemyEffect.Push, Shape=SkillShape.Line }, // BowlingSloth
            new SkillProfile { Bar=CompanionClassTuning.ShooterBar, Warning=1f, Enemy=SkillEnemyEffect.Pull, Shape=SkillShape.Line }, // SatelliteBee
        };
    }
}
