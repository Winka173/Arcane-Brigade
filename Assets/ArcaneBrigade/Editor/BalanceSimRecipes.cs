using System;
using System.Collections.Generic;
using System.Text;

namespace ArcaneBrigade.Editor
{
    /// <summary>Section 4 supplies targets, not rosters; these explicit fixtures never invent memberships.</summary>
    public sealed class BalanceSimRecipe
    {
        public readonly string Name, HeroId;
        public readonly string[] CompanionIds, TargetBondIds, MinorBondIds;
        public readonly int[] TargetBondCounts;

        public BalanceSimRecipe(string name, string hero, string[] companions, string first, int firstCount,
            string second, int secondCount, string minorFirst, string minorSecond)
        {
            Name = name; HeroId = hero; CompanionIds = companions;
            TargetBondIds = new[] { BondV3Catalog.BondId(first), BondV3Catalog.BondId(second) };
            TargetBondCounts = new[] { firstCount, secondCount };
            MinorBondIds = new[] { BondV3Catalog.BondId(minorFirst), BondV3Catalog.BondId(minorSecond) };
        }

        public Dictionary<string, int> Counts()
        {
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (string id in CompanionIds)
                foreach (string tag in BondV3Catalog.Tags(id))
                    Add(counts, tag, BalanceSimRecipeTuning.MemberCount + HeroRulesV3Catalog.ExtraMembership(id, tag));
            foreach (string tag in HeroRulesV3Catalog.MinorIds(HeroId)) Add(counts, tag, BalanceSimRecipeTuning.MemberCount);
            return counts;
        }

        public string DescribeTargets()
        {
            var result = new StringBuilder();
            for (int i = 0; i < TargetBondIds.Length; i++)
            {
                if (result.Length != 0) result.Append("; ");
                result.Append(TargetBondIds[i]).Append(' ').Append(TargetBondCounts[i]);
            }
            foreach (string id in MinorBondIds) result.Append("; ").Append(id).Append(" >= ").Append(BalanceSimRecipeTuning.MinorActivationCount);
            return result.ToString();
        }

        public string DescribeAchieved()
        {
            Dictionary<string, int> counts = Counts();
            var result = new StringBuilder();
            foreach (string id in TargetBondIds) AppendAchieved(result, counts, id);
            foreach (string id in MinorBondIds) AppendAchieved(result, counts, id);
            return result.ToString();
        }

        public bool MeetsTargets
        {
            get
            {
                Dictionary<string, int> counts = Counts();
                for (int i = 0; i < TargetBondIds.Length; i++)
                    if (!counts.TryGetValue(TargetBondIds[i], out int count) || count < TargetBondCounts[i]) return false;
                foreach (string id in MinorBondIds)
                    if (!counts.TryGetValue(id, out int count) || count < BalanceSimRecipeTuning.MinorActivationCount) return false;
                return true;
            }
        }

        private static void Add(Dictionary<string, int> counts, string id, int amount)
        { counts.TryGetValue(id, out int count); counts[id] = count + amount; }

        private static void AppendAchieved(StringBuilder result, Dictionary<string, int> counts, string id)
        {
            counts.TryGetValue(id, out int count);
            int step = 0;
            foreach (BondDefinition definition in BondTuning.V3Definitions)
                if (definition.Id == id)
                    foreach (BondStep candidate in definition.Steps)
                        if (candidate.Count <= count && candidate.Count > step) step = candidate.Count;
            if (result.Length != 0) result.Append("; ");
            result.Append(id).Append(' ').Append(count).Append(" (step ").Append(step).Append(')');
        }
    }

    public static class BalanceSimRecipes
    {
        public static BalanceSimRecipe[] All => BalanceSimRecipeTuning.All;
        public const string Assumptions = "Section 4 specifies bond targets only, with no hero or companion lists. " +
            "Fixtures choose explicit heroes and ten distinct one-star companions. Where targets cannot coexist in ten slots, " +
            "the first major is prioritized, followed by the second and then the named minors. " +
            "Actual memberships and active steps are reported; unmet recipes are approximations, not results for the impossible target. " +
            "Tower Rook and Planet Hamster retain their runtime extra memberships. No membership or combat tuning is changed.";
    }

    public static class BalanceSimRecipeTuning
    {
        public const int RecipeCount = 12, CompanionCount = 10, MemberCount = 1, MinorActivationCount = 2;
        public static readonly BalanceSimRecipe[] All = {
            new BalanceSimRecipe("Burning Court", "StormFencer", new[] {
                "TeapotPilgrim", "MailboxImp", "DiceCrab", "MaskPepper", "CoilKettle", "SaffronMechanist", "BakerMouse", "TeaDragon", "CarouselPony", "JackInTheBox"
            }, "Ember", 8, "Carnival", 4, "Desperado", "Bladenado"),
            new BalanceSimRecipe("Frozen Shore", "DrowsySeer", new[] {
                "SnowSpoon", "FridgeWisp", "ShellPostman", "CoralTeacup", "AnchorSnail", "SailSquid", "RainbowTailor", "SpecimenSlime", "MaskPepper", "SecretPatchBear"
            }, "Frost", 8, "Tide", 6, "Gauntlet", "Cloud"),
            new BalanceSimRecipe("Static Gale", "CometRunner", new[] {
                "KettleBat", "CometKite", "SecretPaperBird", "RibbonOtter", "PaperPlanePigeon", "KiteDragon", "StaticFerret", "StormKitten", "VoltEel", "DreamCompass"
            }, "Storm", 6, "Windborne", 8, "Sniper", "Rapidfire"),
            new BalanceSimRecipe("Poison Grove", "VerdantMatron", new[] {
                "PearMoth", "MushroomLibrarian", "CitrusToad", "SeedHedgehog", "GrandSprout", "PaperDevil", "WateringSnail", "FirstSeed", "PlasterPanda", "NurseHen"
            }, "Garden", 8, "Venom", 6, "Root", "Spiky Shell"),
            new BalanceSimRecipe("Night Library", "MoonProwler", new[] {
                "HoodWisp", "BookmarkGolem", "ThimbleMoth", "LensOwl", "EndlessIndex", "PaperInventor", "VioletRemnant", "SecretPatchBear", "SaddleMoth", "PillowOracle"
            }, "Archive", 6, "Dusk", 6, "Runes", "Bludgeoner"),
            new BalanceSimRecipe("Iron Fortress", "LanternKnight", new[] {
                "IronLocomotive", "FossilArmadillo", "TinTick", "StapleMole", "RainbowBell", "BristleBall", "TopBeetle", "StitchedPangolin", "TowerRook", "BrassLion"
            }, "Stone", 8, "Bulwark", 6, "Bastion", "Monolith"),
            new BalanceSimRecipe("Ghost Choir", "SupperMortar", new[] {
                "TowerRook", "LifebuoyHippo", "NurseHen", "PhoenixChick", "DandelionPuff", "TotemPole", "PumpkinReaper", "Metronome", "HarpSwan", "BagpipeOctopus"
            }, "Spirit", 8, "Melody", 4, "Echo", "Mend"),
            new BalanceSimRecipe("Black Hole Clock", "BonsaiClocksmith", new[] {
                "PlanetHamster", "Metronome", "HarpSwan", "BagpipeOctopus", "AnvilOx", "SinkholeWorm", "NewtonApple", "TinTick", "StapleMole", "TopBeetle"
            }, "Gravity", 8, "Clockwork", 6, "Singularity", "Gears"),
            new BalanceSimRecipe("Lamp Court", "VelvetRegent", new[] {
                "HoodWisp", "BookmarkGolem", "ThimbleMoth", "ReelOwl", "VelvetChampion", "LensOwl", "JellySwordsman", "BrassAcorn", "RadishJester", "ReturnCaptain"
            }, "Lantern", 6, "Court", 6, "Hats", "Masks"),
            new BalanceSimRecipe("Feast Fire", "SupperMortar", new[] {
                "TeapotPilgrim", "BreadCrab", "CoilKettle", "SaffronMechanist", "BakerMouse", "TeaDragon", "SundaeGolem", "BobaTadpole", "MailboxImp", "NurseHen"
            }, "Hearth", 8, "Ember", 6, "Sweet Tooth", "Ball"),
            new BalanceSimRecipe("Cross build: Wet Thunder", "BellDiver", new[] {
                "SnowSpoon", "ShellPostman", "CoralTeacup", "AnchorSnail", "SailSquid", "SpecimenSlime", "RainHeron", "RibbonOtter", "RaincoatFrog", "VoltEel"
            }, "Tide", 6, "Storm", 6, "Rain Script", "Demolitionist"),
            new BalanceSimRecipe("Cross build: Shadow Garden", "DrowsySeer", new[] {
                "PillowOracle", "IceCreamOwl", "MoonJelly", "EclipseEgg", "OrbitMoth", "SaddleMoth", "GrandSprout", "YoyoSquirrel", "FirstSeed", "MushroomLibrarian"
            }, "Dusk", 6, "Garden", 6, "Clover", "Cloud")
        };
    }
}
