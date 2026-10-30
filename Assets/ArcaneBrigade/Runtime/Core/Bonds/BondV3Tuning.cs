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
        public const float MealBonus = .15f, MealFade = 20, FeastPeriod = 12, FeastHighPeriod = 6, FeastSeconds = 6, FeastSpeed = .15f, FeastHeal = .05f, HearthLeech = .01f;
        public const float SoakVulnerability = .08f, SoakSeconds = 10, RiptidePeriod = 10, RiptidePush = 2, WeatherPeriod = 20, WeatherBonus = .15f, TideWeatherBonus = .25f;
        public const int TideEvery = 5, PiercingVolley = 10, SproutLimit = 4, DieEvery = 5, DieFaces = 6, StarRerollEvery = 5, EchoEvery = 8, TurretLimit = 3;
        public const float MomentumPerSecond = .01f, MomentumCap = .15f, MomentumIdle = 2, GalePeriod = 8, GaleSpeed = .2f, GaleSeconds = 4, GrowthPerWave = .02f;
        public const float StarChance = .15f, StarDamage = 1, CelestialSeconds = 8, RecordBonus = .12f, RecordGrowth = .01f, CompleteSetWeakness = .15f;
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
        // Values are totals at the highest reached step; member and team shares add.
        public static readonly BondV5StatLine[] V5Stats = {
            new BondV5StatLine("hearth", ItemEffect.Damage, true, new[] { 0.06f, 0.14f, 0.24f, 0.4f }),
            new BondV5StatLine("hearth", ItemEffect.AttackSpeed, true, new[] { 0.06f, 0.14f, 0.24f, 0.4f }),
            new BondV5StatLine("hearth", ItemEffect.Damage, false, new[] { 0f, 0f, 0.05f, 0.1f }),
            new BondV5StatLine("hearth", ItemEffect.AttackSpeed, false, new[] { 0f, 0f, 0.05f, 0.1f }),
            new BondV5StatLine("ember", ItemEffect.Damage, true, new[] { 0.08f, 0.18f, 0.3f, 0.45f }),
            new BondV5StatLine("ember", ItemEffect.Damage, false, new[] { 0f, 0f, 0.06f, 0.12f }),
            new BondV5StatLine("frost", ItemEffect.Armor, true, new[] { 3f, 6f, 10f, 15f }),
            new BondV5StatLine("frost", ItemEffect.Armor, false, new[] { 0f, 0f, 2f, 4f }),
            new BondV5StatLine("tidal", ItemEffect.SkillHaste, true, new[] { 0.08f, 0.18f, 0.3f, 0.45f }),
            new BondV5StatLine("tidal", ItemEffect.Health, false, new[] { 0f, 0f, 0.06f, 0.12f }),
            new BondV5StatLine("static-club", ItemEffect.AttackSpeed, true, new[] { 0.08f, 0.18f, 0.3f, 0.45f }),
            new BondV5StatLine("static-club", ItemEffect.Crit, false, new[] { 0f, 0f, 0.04f, 0.08f }),
            new BondV5StatLine("windborne", ItemEffect.AttackRange, true, new[] { 0.08f, 0.16f, 0.26f, 0.4f }),
            new BondV5StatLine("windborne", ItemEffect.AttackSpeed, false, new[] { 0f, 0f, 0.05f, 0.1f }),
            new BondV5StatLine("orchard", ItemEffect.Health, true, new[] { 0.1f, 0.22f, 0.36f, 0.55f }),
            new BondV5StatLine("orchard", ItemEffect.Health, false, new[] { 0f, 0f, 0.06f, 0.12f }),
            new BondV5StatLine("venom", ItemEffect.Damage, true, new[] { 0.08f, 0.18f, 0.3f, 0.45f }),
            new BondV5StatLine("venom", ItemEffect.Damage, false, new[] { 0f, 0f, 0.06f, 0.12f }),
            new BondV5StatLine("astral", ItemEffect.Crit, true, new[] { 0.06f, 0.12f, 0.2f, 0.3f }),
            new BondV5StatLine("astral", ItemEffect.Crit, false, new[] { 0f, 0f, 0.04f, 0.08f }),
            new BondV5StatLine("dusk", ItemEffect.Damage, true, new[] { 0.08f, 0.18f, 0.3f, 0.45f }),
            new BondV5StatLine("dusk", ItemEffect.Damage, false, new[] { 0f, 0f, 0.06f, 0.12f }),
            new BondV5StatLine("oddity", ItemEffect.Luck, true, new[] { 0.08f, 0.18f, 0.3f, 0.45f }),
            new BondV5StatLine("oddity", ItemEffect.Luck, false, new[] { 0f, 0f, 0.05f, 0.1f }),
            new BondV5StatLine("regal", ItemEffect.Damage, true, new[] { 0.08f, 0.18f, 0.3f, 0.45f }),
            new BondV5StatLine("regal", ItemEffect.SkillHaste, false, new[] { 0f, 0f, 0.06f, 0.12f }),
            new BondV5StatLine("archive", ItemEffect.SkillHaste, true, new[] { 0.08f, 0.18f, 0.3f, 0.45f }),
            new BondV5StatLine("archive", ItemEffect.Damage, false, new[] { 0f, 0f, 0.06f, 0.12f }),
            new BondV5StatLine("lantern-guild", ItemEffect.AttackRange, true, new[] { 0.08f, 0.16f, 0.26f, 0.4f }),
            new BondV5StatLine("lantern-guild", ItemEffect.Crit, false, new[] { 0f, 0f, 0.04f, 0.08f }),
            new BondV5StatLine("stone", ItemEffect.Armor, true, new[] { 4f, 8f, 12f, 18f }),
            new BondV5StatLine("stone", ItemEffect.Armor, false, new[] { 0f, 0f, 2f, 4f }),
            new BondV5StatLine("clock", ItemEffect.SkillHaste, true, new[] { 0.1f, 0.22f, 0.36f, 0.55f }),
            new BondV5StatLine("clock", ItemEffect.AttackSpeed, false, new[] { 0f, 0f, 0.05f, 0.1f }),
            new BondV5StatLine("bulwark", ItemEffect.Armor, true, new[] { 4f, 8f, 12f, 18f }),
            new BondV5StatLine("bulwark", ItemEffect.Health, true, new[] { 0.06f, 0.12f, 0.2f, 0.3f }),
            new BondV5StatLine("bulwark", ItemEffect.Armor, false, new[] { 0f, 0f, 2f, 4f }),
            new BondV5StatLine("spirit", ItemEffect.Health, true, new[] { 0.1f, 0.22f, 0.36f, 0.55f }),
            new BondV5StatLine("spirit", ItemEffect.Damage, false, new[] { 0f, 0f, 0.06f, 0.12f }),
            new BondV5StatLine("melody", ItemEffect.AttackSpeed, true, new[] { 0.08f, 0.18f, 0.3f, 0.45f }),
            new BondV5StatLine("melody", ItemEffect.SkillHaste, false, new[] { 0f, 0f, 0.05f, 0.1f }),
            new BondV5StatLine("gravity", ItemEffect.Damage, true, new[] { 0.08f, 0.18f, 0.3f, 0.45f }),
            new BondV5StatLine("gravity", ItemEffect.Damage, false, new[] { 0f, 0f, 0.06f, 0.12f }),
            new BondV5StatLine("sweet-tooth", ItemEffect.Health, true, new[] { 0.08f, 0.16f, 0.28f }),
            new BondV5StatLine("ball", ItemEffect.Damage, true, new[] { 0.08f, 0.2f }),
            new BondV5StatLine("rune", ItemEffect.SkillHaste, true, new[] { 0.06f, 0.14f, 0.24f }),
            new BondV5StatLine("paper-wings", ItemEffect.AttackRange, true, new[] { 0.08f, 0.16f, 0.28f }),
            new BondV5StatLine("gauntlet", ItemEffect.Damage, true, new[] { 0.06f, 0.14f, 0.24f }),
            new BondV5StatLine("sea-shanty", ItemEffect.AttackSpeed, true, new[] { 0.08f, 0.2f }),
            new BondV5StatLine("shoe", ItemEffect.AttackSpeed, true, new[] { 0.06f, 0.14f, 0.24f }),
            new BondV5StatLine("bastion", ItemEffect.Armor, true, new[] { 4f, 10f }),
            new BondV5StatLine("cloud", ItemEffect.Health, true, new[] { 0.08f, 0.18f }),
            new BondV5StatLine("demolitionist", ItemEffect.SkillHaste, true, new[] { 0.08f, 0.18f }),
            new BondV5StatLine("exhibit-hall", ItemEffect.Health, true, new[] { 0.1f, 0.25f }),
            new BondV5StatLine("bladenado", ItemEffect.Damage, true, new[] { 0.06f, 0.14f }),
            new BondV5StatLine("sniper", ItemEffect.AttackRange, true, new[] { 0.1f, 0.2f }),
            new BondV5StatLine("unstoppable", ItemEffect.Health, true, new[] { 0.12f, 0.28f }),
            new BondV5StatLine("horn", ItemEffect.Health, true, new[] { 0.06f, 0.14f, 0.24f }),
            new BondV5StatLine("monolith", ItemEffect.Health, true, new[] { 0.08f, 0.18f }),
            new BondV5StatLine("spiky-shell", ItemEffect.Armor, true, new[] { 3f, 8f }),
            new BondV5StatLine("rain-script", ItemEffect.SkillHaste, true, new[] { 0.08f, 0.2f }),
            new BondV5StatLine("bludgeoner", ItemEffect.Damage, true, new[] { 0.06f, 0.14f }),
            new BondV5StatLine("multicaster", ItemEffect.SkillHaste, true, new[] { 0.1f, 0.22f }),
            new BondV5StatLine("rapidfire", ItemEffect.Damage, true, new[] { 0.06f, 0.14f }),
            new BondV5StatLine("desperado", ItemEffect.AttackSpeed, true, new[] { 0.08f, 0.18f }),
            new BondV5StatLine("hat", ItemEffect.Damage, true, new[] { 0.06f, 0.14f, 0.24f }),
            new BondV5StatLine("root", ItemEffect.Armor, true, new[] { 3f, 8f }),
            new BondV5StatLine("mask", ItemEffect.Crit, true, new[] { 0.05f, 0.1f, 0.18f }),
            new BondV5StatLine("gear", ItemEffect.AttackSpeed, true, new[] { 0.08f, 0.2f }),
            new BondV5StatLine("big-top", ItemEffect.Damage, true, new[] { 0.08f, 0.2f }),
            new BondV5StatLine("clover", ItemEffect.Luck, true, new[] { 0.08f, 0.2f }),
            new BondV5StatLine("shield", ItemEffect.Armor, true, new[] { 3f, 6f, 10f }),
            new BondV5StatLine("mend", ItemEffect.Health, true, new[] { 0.1f, 0.22f }),
            new BondV5StatLine("echo", ItemEffect.SkillHaste, true, new[] { 0.06f, 0.14f, 0.24f }),
            new BondV5StatLine("singularity", ItemEffect.Damage, true, new[] { 0.08f, 0.2f }),
        };
        public static float MemberStat(string id, ItemEffect effect, int count) => StatShare(id, effect, count, true);
        public static float TeamStat(string id, ItemEffect effect, int count) => StatShare(id, effect, count, false);
        public static float Stat(string id, ItemEffect effect, int count, bool isMember) => TeamStat(id, effect, count) + (isMember ? MemberStat(id, effect, count) : 0);
        private static float StatShare(string id, ItemEffect effect, int count, bool members)
        {
            float total = 0;
            for (int i = 0; i < V5Stats.Length; i++)
            {
                BondV5StatLine line = V5Stats[i];
                if (line.Id == id && line.Effect == effect && line.Members == members) total += line.At(count);
            }
            return id == "hearth" && count >= FourthStep ? total * HearthDouble : total;
        }
        public static readonly BondDefinition[] V3Definitions = {
            M("hearth", "Hearth"),
            M("ember", "Ember"),
            M("frost", "Frost"),
            M("tidal", "Tide"),
            M("static-club", "Storm"),
            M("windborne", "Windborne"),
            M("orchard", "Garden"),
            M("venom", "Venom"),
            M("astral", "Starlight"),
            M("dusk", "Dusk"),
            M("oddity", "Carnival"),
            M("regal", "Court"),
            M("archive", "Archive"),
            M("lantern-guild", "Lantern"),
            M("stone", "Stone"),
            M("clock", "Clockwork"),
            M("bulwark", "Bulwark"),
            M("spirit", "Spirit"),
            M("melody", "Melody"),
            M("gravity", "Gravity"),
            N("sweet-tooth", "Sweet Tooth", 7),
            N("ball", "Bouncing Ball", 5),
            N("rune", "Runes", 7),
            N("paper-wings", "Paper Wings", 7),
            N("gauntlet", "Gauntlet", 7),
            N("sea-shanty", "Sea Shanty", 5),
            N("shoe", "Quicksoles", 7),
            N("bastion", "Bastion", 5),
            N("cloud", "Cloud", 5),
            N("demolitionist", "Demolitionist", 5),
            N("exhibit-hall", "Exhibit Hall", 5),
            N("bladenado", "Bladenado", 5),
            N("sniper", "Sniper", 5),
            N("unstoppable", "Unstoppable", 5),
            N("horn", "Horns", 7),
            N("monolith", "Monolith", 5),
            N("spiky-shell", "Spiky Shell", 5),
            N("rain-script", "Rain Script", 5),
            N("bludgeoner", "Bludgeoner", 5),
            N("multicaster", "Multicaster", 5),
            N("rapidfire", "Rapidfire", 5),
            N("desperado", "Desperado", 5),
            N("hat", "Hats", 7),
            N("root", "Root", 5),
            N("mask", "Masks", 7),
            N("gear", "Gears", 5),
            N("big-top", "Big Top", 5),
            N("clover", "Clover", 5),
            N("shield", "Shield", 7),
            N("mend", "Mend", 5),
            N("echo", "Echo", 7),
            N("singularity", "Singularity", 5),
        };
        private static BondDefinition M(string id, string name)
        {
            return new BondDefinition { RulesV3 = true, Id = id, Name = name, Threshold = FirstStep, Value = 0,
                Steps = new[] { new BondStep { Count = SecondStep }, new BondStep { Count = ThirdStep }, new BondStep { Count = FourthStep } } };
        }
        private static BondDefinition N(string id, string name, int members)
        {
            return new BondDefinition { RulesV3 = true, Id = id, Name = name, Threshold = FirstStep, Value = 0,
                Steps = members == LargeMinorMembers ? new[] { new BondStep { Count = SecondStep }, new BondStep { Count = ThirdStep } } : new[] { new BondStep { Count = SecondStep } } };
        }
    }
    /// <summary>Signature totals at steps 2/4/6/8, or 2/4(/6) for minors.</summary>
    public sealed class BondV5StatLine
    {
        public readonly string Id;
        public readonly ItemEffect Effect;
        public readonly bool Members;
        public readonly float[] Values;
        public BondV5StatLine(string id, ItemEffect effect, bool members, float[] values)
        { Id = id; Effect = effect; Members = members; Values = values; }
        public float At(int count)
        {
            int index = Math.Min(Values.Length, Math.Max(0, count) / BondTuning.FirstStep) - 1;
            return index < 0 ? 0 : Values[index];
        }
    }
}
