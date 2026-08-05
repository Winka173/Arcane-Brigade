using System;
using System.Collections.Generic;
namespace ArcaneBrigade
{
    /// <summary>Canonical membership compiled from DesignInput/roster120.json; no art assets are required.</summary>
    public static class BondV3Catalog
    {
        private static readonly Dictionary<string, string[]> CompanionTags = new Dictionary<string, string[]>(StringComparer.Ordinal) {
            { "TeapotPilgrim", new[] { "ember", "hearth", "sweet-tooth" } },
            { "CandleSnail", new[] { "hearth", "ember", "sweet-tooth" } },
            { "MailboxImp", new[] { "ember", "ball", "sweet-tooth" } },
            { "HoodWisp", new[] { "lantern-guild", "archive", "rune" } },
            { "BreadCrab", new[] { "hearth", "sweet-tooth", "ball" } },
            { "KettleBat", new[] { "windborne", "static-club", "paper-wings" } },
            { "BookmarkGolem", new[] { "archive", "lantern-guild", "rune" } },
            { "SnowSpoon", new[] { "frost", "tidal", "gauntlet" } },
            { "FridgeWisp", new[] { "frost", "gauntlet", "sea-shanty" } },
            { "PearMoth", new[] { "orchard", "venom", "shoe" } },
            { "CometKite", new[] { "static-club", "windborne", "paper-wings" } },
            { "ShellPostman", new[] { "tidal", "frost", "sea-shanty" } },
            { "CoralTeacup", new[] { "frost", "tidal", "sea-shanty" } },
            { "AnchorSnail", new[] { "frost", "tidal", "gauntlet" } },
            { "SailSquid", new[] { "frost", "tidal", "gauntlet" } },
            { "DiceCrab", new[] { "ember", "ball", "bastion" } },
            { "MaskPepper", new[] { "ember", "ball", "cloud" } },
            { "CoilKettle", new[] { "hearth", "ember", "demolitionist" } },
            { "ThimbleMoth", new[] { "archive", "lantern-guild", "rune" } },
            { "ReelOwl", new[] { "archive", "lantern-guild", "exhibit-hall" } },
            { "RainbowTailor", new[] { "frost", "gauntlet", "bladenado" } },
            { "SecretPaperBird", new[] { "windborne", "paper-wings", "sniper" } },
            { "SaffronMechanist", new[] { "hearth", "ember", "unstoppable" } },
            { "VelvetChampion", new[] { "archive", "lantern-guild", "exhibit-hall" } },
            { "IronLocomotive", new[] { "clock", "stone", "horn" } },
            { "BalloonMallet", new[] { "tidal", "gauntlet", "monolith" } },
            { "TeacupCricket", new[] { "lantern-guild", "exhibit-hall", "spiky-shell" } },
            { "RibbonOtter", new[] { "static-club", "windborne", "rain-script" } },
            { "SpindleWren", new[] { "windborne", "paper-wings", "rain-script" } },
            { "BakerMouse", new[] { "hearth", "ember", "sweet-tooth" } },
            { "TeaDragon", new[] { "hearth", "ember", "bludgeoner" } },
            { "SundaeGolem", new[] { "hearth", "sweet-tooth", "demolitionist" } },
            { "LensOwl", new[] { "archive", "lantern-guild", "rune" } },
            { "EndlessIndex", new[] { "archive", "rune", "multicaster" } },
            { "ShovelPenguin", new[] { "frost", "gauntlet", "bastion" } },
            { "BobaTadpole", new[] { "hearth", "sweet-tooth", "ball" } },
            { "RainHeron", new[] { "static-club", "rain-script", "paper-wings" } },
            { "PaperPlanePigeon", new[] { "windborne", "paper-wings", "sniper" } },
            { "RaincoatFrog", new[] { "static-club", "rain-script", "sniper" } },
            { "LighthouseWhale", new[] { "lantern-guild", "rune", "bludgeoner" } },
            { "KiteDragon", new[] { "static-club", "windborne", "paper-wings" } },
            { "StaticFerret", new[] { "static-club", "windborne", "bladenado" } },
            { "StormKitten", new[] { "static-club", "windborne", "sniper" } },
            { "VoltEel", new[] { "static-club", "rain-script", "rapidfire" } },
            { "ShantySeaLion", new[] { "tidal", "sea-shanty", "desperado" } },
            { "BottleGull", new[] { "tidal", "sea-shanty", "rapidfire" } },
            { "PinnedBeetle", new[] { "archive", "exhibit-hall", "multicaster" } },
            { "FossilArmadillo", new[] { "stone", "clock", "horn" } },
            { "PillowOracle", new[] { "astral", "dusk", "hat" } },
            { "MushroomLibrarian", new[] { "orchard", "venom", "root" } },
            { "JellySwordsman", new[] { "oddity", "regal", "mask" } },
            { "BrassAcorn", new[] { "oddity", "regal", "mask" } },
            { "TinTick", new[] { "clock", "stone", "gear" } },
            { "SpringDuck", new[] { "clock", "gear", "horn" } },
            { "ButtonBeetle", new[] { "oddity", "regal", "big-top" } },
            { "StapleMole", new[] { "stone", "clock", "gear" } },
            { "IceCreamOwl", new[] { "dusk", "astral", "hat" } },
            { "CitrusToad", new[] { "orchard", "venom", "shoe" } },
            { "RadishJester", new[] { "oddity", "regal", "mask" } },
            { "SeedHedgehog", new[] { "orchard", "venom", "root" } },
            { "MoonJelly", new[] { "astral", "dusk", "hat" } },
            { "StarSock", new[] { "astral", "clover", "cloud" } },
            { "EclipseEgg", new[] { "astral", "dusk", "clover" } },
            { "OrbitMoth", new[] { "astral", "dusk", "hat" } },
            { "DreamCompass", new[] { "clock", "horn", "rapidfire" } },
            { "BubbleKoi", new[] { "astral", "clover", "cloud" } },
            { "SockCyclops", new[] { "venom", "shoe", "bludgeoner" } },
            { "CrayonGremlin", new[] { "oddity", "big-top", "demolitionist" } },
            { "MirrorNewt", new[] { "venom", "shoe", "bludgeoner" } },
            { "RibbonSpider", new[] { "venom", "shoe", "bladenado" } },
            { "RainbowBell", new[] { "stone", "clock", "bludgeoner" } },
            { "SecretPatchBear", new[] { "dusk", "hat", "cloud" } },
            { "SecretDreamLamb", new[] { "astral", "dusk", "rapidfire" } },
            { "ClockworkPiper", new[] { "clock", "gear", "multicaster" } },
            { "PaperInventor", new[] { "archive", "rune", "sniper" } },
            { "ReturnCaptain", new[] { "regal", "oddity", "mask" } },
            { "DoughForeman", new[] { "regal", "mask", "big-top" } },
            { "VioletRemnant", new[] { "dusk", "hat", "monolith" } },
            { "VelvetEnchantress", new[] { "regal", "mask", "demolitionist" } },
            { "GrandSprout", new[] { "orchard", "root", "spiky-shell" } },
            { "BristleBall", new[] { "stone", "horn", "monolith" } },
            { "PaperDevil", new[] { "venom", "orchard", "shoe" } },
            { "LittleSprout", new[] { "astral", "hat", "clover" } },
            { "MonsterMarble", new[] { "stone", "horn", "unstoppable" } },
            { "CarouselPony", new[] { "oddity", "regal", "desperado" } },
            { "LampGoat", new[] { "lantern-guild", "exhibit-hall", "bladenado" } },
            { "TopBeetle", new[] { "stone", "clock", "gear" } },
            { "WateringSnail", new[] { "orchard", "venom", "root" } },
            { "YoyoSquirrel", new[] { "orchard", "shoe", "spiky-shell" } },
            { "FirstSeed", new[] { "orchard", "root", "spiky-shell" } },
            { "JackInTheBox", new[] { "oddity", "big-top", "desperado" } },
            { "LostCarousel", new[] { "oddity", "big-top", "desperado" } },
            { "SpecimenSlime", new[] { "frost", "tidal", "bastion" } },
            { "CourtHerald", new[] { "regal", "mask", "desperado" } },
            { "StitchedPangolin", new[] { "stone", "horn", "monolith" } },
            { "SaddleMoth", new[] { "dusk", "clover", "cloud" } },
            { "TowerRook", new[] { "bulwark", "spirit", "shield" } },
            { "LifebuoyHippo", new[] { "bulwark", "spirit", "mend" } },
            { "NurseHen", new[] { "bulwark", "spirit", "mend" } },
            { "PhoenixChick", new[] { "bulwark", "spirit", "mend" } },
            { "DandelionPuff", new[] { "bulwark", "spirit", "mend" } },
            { "TotemPole", new[] { "bulwark", "spirit", "shield" } },
            { "BrassLion", new[] { "bulwark", "shield", "bastion" } },
            { "QuiltWalrus", new[] { "bulwark", "shield", "monolith" } },
            { "PlasterPanda", new[] { "bulwark", "mend", "bastion" } },
            { "PumpkinReaper", new[] { "spirit", "shield", "unstoppable" } },
            { "BoneXylophone", new[] { "spirit", "shield", "unstoppable" } },
            { "ScarecrowCrow", new[] { "spirit", "shield", "unstoppable" } },
            { "Metronome", new[] { "melody", "gravity", "echo" } },
            { "HarpSwan", new[] { "melody", "gravity", "echo" } },
            { "BagpipeOctopus", new[] { "melody", "gravity", "echo" } },
            { "AnvilOx", new[] { "melody", "gravity", "singularity" } },
            { "SinkholeWorm", new[] { "melody", "gravity", "singularity" } },
            { "PlanetHamster", new[] { "melody", "gravity", "singularity" } },
            { "CymbalMonkey", new[] { "melody", "echo", "bladenado" } },
            { "ChoirCanary", new[] { "melody", "echo", "multicaster" } },
            { "PianoCrocodile", new[] { "melody", "echo", "multicaster" } },
            { "NewtonApple", new[] { "gravity", "singularity", "demolitionist" } },
            { "BowlingSloth", new[] { "gravity", "singularity", "spiky-shell" } },
            { "SatelliteBee", new[] { "gravity", "echo", "rapidfire" } },
        };
        public static IEnumerable<string> CompanionIds => CompanionTags.Keys;
        public static string[] Tags(string companionId) => companionId != null && CompanionTags.TryGetValue(companionId, out string[] tags) ? (string[])tags.Clone() : Array.Empty<string>();
        public static string BondId(string name)
        {
            if (name == null) return null;
            for (int i = 0; i < BondTuning.V3Definitions.Length; i++)
            { BondDefinition bond = BondTuning.V3Definitions[i]; if (string.Equals(name, bond.Name, StringComparison.OrdinalIgnoreCase) || string.Equals(name, bond.Id, StringComparison.OrdinalIgnoreCase)) return bond.Id; }
            return name.ToLowerInvariant().Replace(" ", "-");
        }
        public static bool IsMajor(string id)
        {
            for (int i = 0; i < BondTuning.MajorCount; i++) if (BondTuning.V3Definitions[i].Id == id) return true;
            return false;
        }
        public static BondDefinition[] CreateDefinitions()
        {
            var result = new BondDefinition[BondTuning.V3Definitions.Length];
            for (int i = 0; i < result.Length; i++)
            {
                BondDefinition source = BondTuning.V3Definitions[i]; var steps = new BondStep[source.Steps.Length];
                for (int j = 0; j < steps.Length; j++) steps[j] = new BondStep { Count = source.Steps[j].Count, Effect = source.Steps[j].Effect, Value = source.Steps[j].Value };
                result[i] = new BondDefinition { RulesV3 = true, Id = source.Id, Name = source.Name, Threshold = source.Threshold, Effect = source.Effect, Value = source.Value, Steps = steps };
            }
            return result;
        }
        public static void Configure(RunCatalog catalog)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            BondDefinition[] definitions = CreateDefinitions();
            if (catalog.Bonds != null) foreach (BondDefinition old in catalog.Bonds)
                if (old != null) for (int i = 0; i < definitions.Length; i++) if (definitions[i].Id == old.Id) definitions[i].Icon = old.Icon;
            catalog.Bonds = definitions;
            if (catalog.Companions != null) foreach (UnitDefinition unit in catalog.Companions)
                if (unit != null && CompanionTags.TryGetValue(unit.Id, out string[] tags)) unit.Tags = (string[])tags.Clone();
        }
    }
}
