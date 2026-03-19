using System.Collections.Generic;

namespace ArcaneBrigade
{
    /// <summary>Each playable character's own weapon and starting friend (Docs/Bizarre_Arsenal.md §2–3). No two heroes carry the
    /// same weapon; no two start with the same friend or the same friend's weapon, and the friend's weapon complements the
    /// hero's (melee hero, ranged friend and the other way round). A hero whose weapon is one of the new Bizarre Arsenal
    /// weapons carries its interim weapon until that weapon is in the catalog (it is added only when its painted art exists).</summary>
    public static class HeroLoadout
    {
        public readonly struct Entry
        {
            public readonly string Weapon, Interim, Friend, FriendWeapon;
            public Entry(string weapon, string interim, string friend, string friendWeapon) { Weapon = weapon; Interim = interim; Friend = friend; FriendWeapon = friendWeapon; }
        }

        public static readonly IReadOnlyDictionary<string, Entry> Heroes = new Dictionary<string, Entry>
        {
            ["LanternKnight"] = new Entry("KeyCleaver", null, "KettleBat", "CrescentBow"),
            ["Postmaster"] = new Entry("MailboxLauncher", null, "BookmarkGolem", "BookPaddle"),
            ["DrowsySeer"] = new Entry("MoonseedOrb", null, "CandleSnail", "ForkTrident"),
            ["RootOracle"] = new Entry("BambooVase", "RuneStaff", "MushroomLibrarian", "DreamIron"),
            ["LoneJelly"] = new Entry("InkyOctopus", "DreamProjector", "JellySwordsman", "TwinSabres"),
            ["GlassDice"] = new Entry("GlitchToybox", "DartCompass", "DiceCrab", "PaperFan"),
            ["TideSinger"] = new Entry("RibbonHarp", null, "AnchorSnail", "RubberHammer"),
            ["CometRunner"] = new Entry("ClockCrescent", null, "CometKite", "ShellCannon"),
            ["MirrorKeeper"] = new Entry("SnowGlobeOrb", null, "VioletRemnant", "UmbrellaHook"),
            ["TreasureAcorn"] = new Entry("AcornSling", null, "BrassAcorn", "SpoonSabre"),
            ["VerdantMatron"] = new Entry("LeafCrossbow", null, "LittleSprout", "TapeWhip"),
            ["CopperCauldron"] = new Entry("FireExtinguisher", null, "TeacupCricket", "SpringKnocker"),
            ["ChinaMarionette"] = new Entry("NeedleSinger", null, "RibbonSpider", "RibbonWheel"),
            ["SoapSkater"] = new Entry("BubbleWand", null, "SailSquid", "UmbrellaLance"),
            ["VelvetRegent"] = new Entry("StarforgeClaymore", "BellFork", "ButtonBeetle", "ButtonBow"),
            ["ButtonCavalier"] = new Entry("CorkscrewRapier", null, "TinTick", "InkPistol"),
            ["NightMaw"] = new Entry("DrumstickClub", "SpoonSabre", "MaskPepper", "DartCompass"),
            ["ThornTreasurer"] = new Entry("GumballGatling", "ParasolNeedle", "SeedHedgehog", "LollipopLance"),
            ["CitrusMarshal"] = new Entry("SignpostSlammer", "RubberHammer", "CitrusToad", "StarKazoo"),
            ["IvorySovereign"] = new Entry("DreamStamp", null, "IceCreamOwl", "RuneStaff"),
            ["PetalCitadel"] = new Entry("RoseBouquet", "PaperFan", "PearMoth", "PizzaWheel"),
            ["PatchworkHost"] = new Entry("ConfettiBlunderbuss", "InkPistol", "SockCyclops", "CloudScissors"),
            ["ArchiveCurator"] = new Entry("SleepCamera", null, "FridgeWisp", "ClockworkYoYo"),
            ["PennyBroker"] = new Entry("DetonatorBox", "CrescentBoomerang", "MailboxImp", "StampHammer"),
            ["StormFencer"] = new Entry("PlasmaTrident", null, "SpringDuck", "AfterimageSabre"),
            ["TinVirtuoso"] = new Entry("MusicBox", null, "StapleMole", "MaracaHammer"),
            ["MoonProwler"] = new Entry("MoonScythe", null, "StarSock", "MoonFeatherFan"),
            ["SupperMortar"] = new Entry("TeapotPopgun", null, "BreadCrab", "ToastCleaver"),
            ["LanternWeaver"] = new Entry("LanternStaff", null, "ThimbleMoth", "TeabagFlail"),
            ["HourglassStag"] = new Entry("AlarmMortar", null, "DreamCompass", "ParasolNeedle"),
            ["RainArchivist"] = new Entry("SquirtTrio", "DreamOrbStaff", "RibbonOtter", "TinFish"),
        };

        /// <summary>The weapon this hero carries now: its own, or its interim one while its own is not in the catalog yet.</summary>
        public static string Weapon(RunCatalog catalog, string heroId)
        {
            if (!Heroes.TryGetValue(heroId, out Entry entry)) return null;
            return entry.Interim == null || catalog.Weapon(entry.Weapon) != null ? entry.Weapon : entry.Interim;
        }
    }
}
