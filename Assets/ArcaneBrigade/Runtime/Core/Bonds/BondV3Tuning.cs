using System;
namespace ArcaneBrigade
{
    public static partial class BondTuning
    {
        public static readonly string[] HitCounterKeys = { "horn", "rune", "gear", "sea-shanty", "tide-splash", "minor-singularity", "desperado", "star-reroll" };
        public const uint RandomSeed = 1979, RandomMultiplier = 1664525, RandomIncrement = 1013904223, RandomMask = 0x00FFFFFF;
        public const int TeamCapacity = 11, EnemyCapacity = 1024, RequestCapacity = 1024;
        public const float SingularityDamage = .5f;
        public const int MajorCount = 20, MinorCount = 32, RosterCount = 120, MajorMembers = 9, SmallMinorMembers = 5, LargeMinorMembers = 7;
        public const int FirstStep = 2, SecondStep = 4, ThirdStep = 6, FourthStep = 8;
        public const float MealBonus = .12f, MealFade = 20, FeastPeriod = 12, FeastHighPeriod = 6, FeastSeconds = 6, FeastSpeed = .15f, FeastHeal = .05f, HearthLeech = .01f;
        public const float SoakVulnerability = .08f, SoakSeconds = 10, RiptidePeriod = 10, RiptidePush = 2, WeatherPeriod = 20, WeatherBonus = .15f, TideWeatherBonus = .25f;
        public const int TideEvery = 5, PiercingVolley = 10, SproutLimit = 4, DieEvery = 5, DieFaces = 6, StarRerollEvery = 5, EchoEvery = 8, TurretLimit = 3;
        public const float MomentumPerSecond = .01f, MomentumCap = .1f, MomentumIdle = 2, GalePeriod = 8, GaleSpeed = .2f, GaleSeconds = 4, GrowthPerWave = .02f;
        public const float StarChance = .2f, StarDamage = 1, CelestialSeconds = 8, RecordBonus = .12f, RecordGrowth = .01f, CompleteSetWeakness = .15f;
        public const float StoneReflect = .3f, CollisionMultiplier = 1, EarthquakePeriod = 12, EarthquakeStun = .5f, ClockFill = .2f, ClockStopPeriod = 20, ClockStopSeconds = 1.5f, OverclockSeconds = 10;
        public const float BulwarkShield = .15f, DamageShare = .25f, ShareReduction = .3f, WallPeriod = 15, WallSeconds = 6, LastStandSeconds = 3, LastStandArmor = 3;
        public const float SpiritSeconds = 6, SpiritReviveReduction = 4, SpiritHeal = .01f, RevenantHealth = .5f, RealmSeconds = 15, SoulsDamage = .01f;
        public const int SoulsPerBonus = 10, MelodyEvery = 4, OrchestraCount = 3, ClusterMinimum = 3;
        public const float BeatSpeed = .1f, BeatSeconds = 4, ChorusPeriod = 10, ChorusStrength = .5f, GravityPull = 1, ClumpRadius = 2, ClumpBonus = .12f, SingularityPeriod = 12, SingularitySeconds = 3, GravitySlow = .25f, GravityRadius = 6;
        public const float MinorShield = .08f, ShieldPeriod = 20, MendHealthThreshold = .5f, MendRate = .01f, MendHighRate = .02f, BastionShield = .06f, BastionHighShield = .1f;
        public const float SpikyReflect = .4f, SpikyHighReflect = .8f, MonolithRadius = 4, SniperPerUnit = .02f, SniperCap = .16f, SniperHighCap = .3f, ArmorIgnore = .15f, ArmorIgnoreHigh = .3f;
        public const int MonolithCap = 6, MonolithHighCap = 10, DesperadoEvery = 8, DesperadoHighEvery = 6, DesperadoShots = 5, RapidStackCap = 10, DemolitionLimit = 10;
        public const float DesperadoDamage = .4f, BladeSplash = .25f, BladeHighSplash = .5f, RapidSpeed = .02f, RapidHighSpeed = .03f, RapidIdle = 3, DemolitionStun = .3f, DemolitionHighStun = .6f, Dodge = .06f, DodgeHigh = .12f;
        public const int MinorEchoEvery = 6, MinorEchoFour = 4, MinorEchoSix = 3, SingularityEvery = 12, SingularityHighEvery = 8, Multicasts = 1, MulticastsHigh = 2;
        public const float CopyStrength = .5f, RerollGrowth = .005f, RerollHighGrowth = .01f, HatDiscount = .25f, HatHighDiscount = .4f, DashPeriod = 8, DashHighPeriod = 6, DashDistance = 1, DashSeconds = .2f;
        public const float BallRetention = .7f, PaperDistance = .1f, ExhibitHeal = .2f, ChoiceBonus = .1f, JackpotSpeed = .1f, JackpotSeconds = 6, WaveBuffSeconds = 9999;
        public const int JackpotCasts = 5, JackpotCoins = 3, TreeCoins = 5, SmallChoiceCount = 3, CashoutWaves = 3, BoomerangHighReturns = 2;
        public static readonly BondDefinition[] V3Definitions = {
            M("hearth", "Hearth", ItemEffect.Damage, .04f, (4, ItemEffect.AttackSpeed, .08f), (6, ItemEffect.Health, .15f), (8, ItemEffect.Damage, .10f), (8, ItemEffect.AttackSpeed, .10f)),
            M("ember", "Ember", ItemEffect.Damage, .05f, (4, ItemEffect.AttackSpeed, .06f), (6, ItemEffect.Damage, .10f), (8, ItemEffect.Crit, .10f)),
            M("frost", "Frost", ItemEffect.Damage, .03f, (4, ItemEffect.Armor, 2f), (6, ItemEffect.Health, .12f), (8, ItemEffect.Damage, .15f)),
            M("tidal", "Tide", ItemEffect.AttackRange, .04f, (4, ItemEffect.Pickup, .10f), (6, ItemEffect.Damage, .08f), (8, ItemEffect.Income, 2f)),
            M("static-club", "Storm", ItemEffect.CompanionAttackSpeed, .05f, (4, ItemEffect.Crit, .05f), (6, ItemEffect.ChainChance, .05f), (8, ItemEffect.Damage, .12f)),
            M("windborne", "Windborne", ItemEffect.BulletSpeed, .06f, (4, ItemEffect.AttackRange, .08f), (6, ItemEffect.BulletPierce, 1f), (8, ItemEffect.Damage, .10f)),
            M("orchard", "Garden", ItemEffect.Health, .06f, (4, ItemEffect.HeartHealing, .10f), (6, ItemEffect.Income, 1f), (8, ItemEffect.Health, .15f), (8, ItemEffect.Damage, .08f)),
            M("venom", "Venom", ItemEffect.Damage, .04f, (4, ItemEffect.CollisionDamage, 2f), (6, ItemEffect.Crit, .06f), (8, ItemEffect.Damage, .14f)),
            M("astral", "Starlight", ItemEffect.Crit, .04f, (4, ItemEffect.Luck, .05f), (6, ItemEffect.Damage, .08f), (8, ItemEffect.Crit, .10f)),
            M("dusk", "Dusk", ItemEffect.Damage, .04f, (4, ItemEffect.CompanionDamage, .06f), (6, ItemEffect.ChampionDamage, .08f), (8, ItemEffect.Damage, .12f)),
            M("oddity", "Carnival", ItemEffect.Luck, .05f, (4, ItemEffect.Income, 1f), (6, ItemEffect.Discount, .04f), (8, ItemEffect.Damage, .10f)),
            M("regal", "Court", ItemEffect.CompanionDamage, .05f, (4, ItemEffect.Discount, .03f), (6, ItemEffect.RecruitDiscount, .05f), (8, ItemEffect.Damage, .10f)),
            M("archive", "Archive", ItemEffect.Damage, .03f, (4, ItemEffect.Crit, .04f), (6, ItemEffect.Luck, .05f), (8, ItemEffect.Damage, .10f)),
            M("lantern-guild", "Lantern", ItemEffect.AttackRange, .05f, (4, ItemEffect.Damage, .05f), (6, ItemEffect.ChampionDamage, .08f), (8, ItemEffect.Crit, .08f)),
            M("stone", "Stone", ItemEffect.Health, .06f, (4, ItemEffect.Knockback, .10f), (6, ItemEffect.Armor, 2f), (8, ItemEffect.Damage, .12f)),
            M("clock", "Clockwork", ItemEffect.CompanionAttackSpeed, .05f, (4, ItemEffect.AttackSpeed, .05f), (6, ItemEffect.MoveSpeed, .08f), (8, ItemEffect.AttackSpeed, .10f)),
            M("bulwark", "Bulwark", ItemEffect.Health, .06f, (4, ItemEffect.Armor, 2f), (6, ItemEffect.Health, .12f), (8, ItemEffect.Armor, 3f), (8, ItemEffect.Damage, .08f)),
            M("spirit", "Spirit", ItemEffect.Health, .05f, (4, ItemEffect.HeartHealing, .10f), (6, ItemEffect.CompanionDamage, .08f), (8, ItemEffect.Damage, .12f)),
            M("melody", "Melody", ItemEffect.CompanionAttackSpeed, .06f, (4, ItemEffect.AttackSpeed, .06f), (6, ItemEffect.Crit, .06f), (8, ItemEffect.Damage, .10f), (8, ItemEffect.AttackSpeed, .08f)),
            M("gravity", "Gravity", ItemEffect.Damage, .04f, (4, ItemEffect.Knockback, .10f), (6, ItemEffect.Damage, .08f), (8, ItemEffect.Damage, .14f)),
            N("sweet-tooth", "Sweet Tooth", ItemEffect.Damage, 0f, 7),
            N("ball", "Bouncing Ball", ItemEffect.Damage, 0f, 5),
            N("rune", "Runes", ItemEffect.Damage, 0f, 7),
            N("paper-wings", "Paper Wings", ItemEffect.Damage, 0f, 7),
            N("gauntlet", "Gauntlet", ItemEffect.Damage, 0f, 7),
            N("sea-shanty", "Sea Shanty", ItemEffect.Damage, 0f, 5),
            N("shoe", "Quicksoles", ItemEffect.MoveSpeed, .06f, 7),
            N("bastion", "Bastion", ItemEffect.Damage, 0f, 5),
            N("cloud", "Cloud", ItemEffect.Damage, 0f, 5),
            N("demolitionist", "Demolitionist", ItemEffect.Damage, 0f, 5),
            N("exhibit-hall", "Exhibit Hall", ItemEffect.Health, .06f, 5),
            N("bladenado", "Bladenado", ItemEffect.Damage, 0f, 5),
            N("sniper", "Sniper", ItemEffect.Damage, 0f, 5),
            N("unstoppable", "Unstoppable", ItemEffect.Damage, 0f, 5),
            N("horn", "Horns", ItemEffect.Damage, 0f, 7),
            N("monolith", "Monolith", ItemEffect.Damage, 0f, 5),
            N("spiky-shell", "Spiky Shell", ItemEffect.Damage, 0f, 5),
            N("rain-script", "Rain Script", ItemEffect.Damage, 0f, 5),
            N("bludgeoner", "Bludgeoner", ItemEffect.Damage, 0f, 5),
            N("multicaster", "Multicaster", ItemEffect.Damage, 0f, 5),
            N("rapidfire", "Rapidfire", ItemEffect.Damage, 0f, 5),
            N("desperado", "Desperado", ItemEffect.Damage, 0f, 5),
            N("hat", "Hats", ItemEffect.Damage, 0f, 7),
            N("root", "Root", ItemEffect.Damage, 0f, 5),
            N("mask", "Masks", ItemEffect.Crit, .04f, 7),
            N("gear", "Gears", ItemEffect.Damage, 0f, 5),
            N("big-top", "Big Top", ItemEffect.Damage, 0f, 5),
            N("clover", "Clover", ItemEffect.Damage, 0f, 5),
            N("shield", "Shield", ItemEffect.Damage, 0f, 7),
            N("mend", "Mend", ItemEffect.Damage, 0f, 5),
            N("echo", "Echo", ItemEffect.Damage, 0f, 7),
            N("singularity", "Singularity", ItemEffect.Damage, 0f, 5),
        };
        private static BondDefinition M(string id, string name, ItemEffect effect, float value, params (int, ItemEffect, float)[] steps)
        {
            var authored = new BondStep[steps.Length];
            for (int i = 0; i < steps.Length; i++) authored[i] = new BondStep { Count = steps[i].Item1, Effect = steps[i].Item2, Value = steps[i].Item3 };
            return new BondDefinition { Id = id, Name = name, Threshold = FirstStep, Effect = effect, Value = value, Steps = authored };
        }
        private static BondDefinition N(string id, string name, ItemEffect effect, float value, int members)
        {
            return new BondDefinition { Id = id, Name = name, Threshold = FirstStep, Effect = effect, Value = value,
                Steps = members == LargeMinorMembers ? new[] { new BondStep { Count = SecondStep }, new BondStep { Count = ThirdStep } } : new[] { new BondStep { Count = SecondStep } } };
        }
    }
}
