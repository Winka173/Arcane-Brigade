using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Puts the bond rework in (Docs/Bond_Companion_Hero_Rework_Design_2026-10-27.md, section 17): 16 major bonds with steps 2/4/6/8
    /// and 16 minor bonds, the bond tags of the 96 companions, and the heroes' recruit themes translated to the new bond ids. Stat packages only;
    /// the floor objects, counters and skills come with the engine phases. Safe to run again.</summary>
    public static class BondReworkBuilder
    {
        // id, name, major, primary effect, value per 2 members, steps (count, effect, value)
        private sealed class Spec
        {
            public string Id, Name; public bool Major; public ItemEffect Effect; public float Value; public (int, ItemEffect, float)[] Steps = new (int, ItemEffect, float)[0];
        }
        private static Spec M(string id, string name, ItemEffect e, float v, params (int, ItemEffect, float)[] steps) => new Spec { Id = id, Name = name, Major = true, Effect = e, Value = v, Steps = steps };
        private static Spec N(string id, string name, ItemEffect e, float v) => new Spec { Id = id, Name = name, Major = false, Effect = e, Value = v };
        private static readonly Spec[] Specs =
        {
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
            N("sweet-tooth", "Sweet Tooth", ItemEffect.Income, 1f), N("ball", "Bouncing Ball", ItemEffect.BulletBounce, 1f), N("gauntlet", "Gauntlet", ItemEffect.MeleeDamage, .06f),
            N("sea-shanty", "Sea Shanty", ItemEffect.CompanionAttackSpeed, .05f), N("paper-wings", "Paper Wings", ItemEffect.BulletDistance, .08f), N("rain-script", "Rain Script", ItemEffect.Discount, .03f),
            N("root", "Root", ItemEffect.Health, .05f), N("mask", "Masks", ItemEffect.Crit, .04f), N("hat", "Hats", ItemEffect.RecruitDiscount, .04f), N("clover", "Clover", ItemEffect.Luck, .06f),
            N("big-top", "Big Top", ItemEffect.Damage, .03f), N("horn", "Horns", ItemEffect.Knockback, .10f), N("rune", "Runes", ItemEffect.Crit, .05f), N("exhibit-hall", "Exhibit Hall", ItemEffect.Health, .06f),
            N("shoe", "Quicksoles", ItemEffect.MoveSpeed, .06f), N("gear", "Gears", ItemEffect.AttackSpeed, .04f),
        };
        // the old bonds that no longer have members keep their entry (saves and hero data may still name them); their ids map to a new theme for the heroes
        private static readonly Dictionary<string, string> RetiredTheme = new Dictionary<string, string>
        { { "fruit", "hearth" }, { "archer", "windborne" }, { "sprout", "orchard" }, { "knight", "regal" }, { "morning", "astral" }, { "hide", "dusk" }, { "hare", "shoe" } };
        private static readonly Dictionary<string, string[]> CompanionTags = new Dictionary<string, string[]>
        {
            { "TeapotPilgrim", new[] { "ember", "Sweet-tooth" } },
            { "CandleSnail", new[] { "hearth", "ember", "Sweet-tooth" } },
            { "MailboxImp", new[] { "ember", "Ball" } },
            { "HoodWisp", new[] { "lantern-guild", "Rune" } },
            { "BreadCrab", new[] { "hearth", "Sweet-tooth" } },
            { "KettleBat", new[] { "windborne", "Paper-wings" } },
            { "BookmarkGolem", new[] { "archive", "Rune", "Exhibit-hall" } },
            { "SnowSpoon", new[] { "frost", "Gauntlet" } },
            { "FridgeWisp", new[] { "frost", "Gauntlet" } },
            { "PearMoth", new[] { "orchard", "venom" } },
            { "CometKite", new[] { "static-club", "windborne", "Paper-wings" } },
            { "ShellPostman", new[] { "tidal", "Sea-shanty" } },
            { "CoralTeacup", new[] { "frost", "tidal", "Sea-shanty" } },
            { "AnchorSnail", new[] { "frost", "tidal" } },
            { "SailSquid", new[] { "frost", "tidal", "Gauntlet" } },
            { "DiceCrab", new[] { "ember", "Ball" } },
            { "MaskPepper", new[] { "ember", "Ball" } },
            { "CoilKettle", new[] { "hearth", "ember" } },
            { "ThimbleMoth", new[] { "archive", "lantern-guild" } },
            { "ReelOwl", new[] { "archive", "lantern-guild" } },
            { "RainbowTailor", new[] { "frost", "Gauntlet" } },
            { "SecretPaperBird", new[] { "windborne", "Paper-wings" } },
            { "SaffronMechanist", new[] { "hearth", "ember" } },
            { "VelvetChampion", new[] { "archive", "lantern-guild" } },
            { "IronLocomotive", new[] { "clock", "stone", "Horn" } },
            { "BalloonMallet", new[] { "tidal", "Gauntlet" } },
            { "TeacupCricket", new[] { "lantern-guild", "Exhibit-hall" } },
            { "RibbonOtter", new[] { "static-club", "Rain-script" } },
            { "SpindleWren", new[] { "windborne", "Paper-wings" } },
            { "BakerMouse", new[] { "hearth", "Sweet-tooth" } },
            { "TeaDragon", new[] { "hearth", "ember" } },
            { "SundaeGolem", new[] { "hearth", "Sweet-tooth" } },
            { "LensOwl", new[] { "archive", "lantern-guild", "Rune" } },
            { "EndlessIndex", new[] { "archive", "Rune" } },
            { "ShovelPenguin", new[] { "frost", "Gauntlet" } },
            { "BobaTadpole", new[] { "hearth", "Sweet-tooth", "Ball" } },
            { "RainHeron", new[] { "static-club", "Rain-script" } },
            { "PaperPlanePigeon", new[] { "windborne", "Paper-wings" } },
            { "RaincoatFrog", new[] { "static-club", "Rain-script" } },
            { "LighthouseWhale", new[] { "lantern-guild", "Rune" } },
            { "KiteDragon", new[] { "static-club", "windborne", "Paper-wings" } },
            { "StaticFerret", new[] { "static-club", "windborne" } },
            { "StormKitten", new[] { "static-club", "windborne" } },
            { "VoltEel", new[] { "static-club", "Rain-script" } },
            { "ShantySeaLion", new[] { "tidal", "Sea-shanty" } },
            { "BottleGull", new[] { "tidal", "Sea-shanty" } },
            { "PinnedBeetle", new[] { "archive", "Exhibit-hall" } },
            { "FossilArmadillo", new[] { "stone", "Horn" } },
            { "PillowOracle", new[] { "astral", "dusk" } },
            { "MushroomLibrarian", new[] { "orchard", "venom" } },
            { "JellySwordsman", new[] { "oddity", "regal" } },
            { "BrassAcorn", new[] { "oddity", "regal", "Mask" } },
            { "TinTick", new[] { "clock", "Gear" } },
            { "SpringDuck", new[] { "clock", "Gear" } },
            { "ButtonBeetle", new[] { "oddity", "regal" } },
            { "StapleMole", new[] { "stone", "clock" } },
            { "IceCreamOwl", new[] { "dusk", "Hat" } },
            { "CitrusToad", new[] { "orchard", "Shoe" } },
            { "RadishJester", new[] { "oddity", "Mask", "Big-top" } },
            { "SeedHedgehog", new[] { "orchard", "venom", "Root" } },
            { "MoonJelly", new[] { "astral", "Hat" } },
            { "StarSock", new[] { "astral", "Clover" } },
            { "EclipseEgg", new[] { "astral", "dusk" } },
            { "OrbitMoth", new[] { "astral", "dusk", "Hat" } },
            { "DreamCompass", new[] { "clock", "Horn" } },
            { "BubbleKoi", new[] { "astral", "Clover" } },
            { "SockCyclops", new[] { "venom", "Shoe" } },
            { "CrayonGremlin", new[] { "oddity", "Big-top" } },
            { "MirrorNewt", new[] { "venom", "Shoe" } },
            { "RibbonSpider", new[] { "venom", "Shoe" } },
            { "RainbowBell", new[] { "stone", "clock" } },
            { "SecretPatchBear", new[] { "dusk", "Hat" } },
            { "SecretDreamLamb", new[] { "astral", "dusk" } },
            { "ClockworkPiper", new[] { "clock", "Gear" } },
            { "PaperInventor", new[] { "archive", "Rune" } },
            { "ReturnCaptain", new[] { "regal", "Mask" } },
            { "DoughForeman", new[] { "regal", "Mask" } },
            { "VioletRemnant", new[] { "dusk", "Hat" } },
            { "VelvetEnchantress", new[] { "regal", "Mask" } },
            { "GrandSprout", new[] { "orchard", "Root" } },
            { "BristleBall", new[] { "stone", "Horn" } },
            { "PaperDevil", new[] { "venom", "Shoe" } },
            { "LittleSprout", new[] { "astral", "Hat", "Clover" } },
            { "MonsterMarble", new[] { "stone", "Horn" } },
            { "CarouselPony", new[] { "oddity", "regal" } },
            { "LampGoat", new[] { "lantern-guild", "Exhibit-hall" } },
            { "TopBeetle", new[] { "stone", "clock", "Gear" } },
            { "WateringSnail", new[] { "orchard", "venom", "Root" } },
            { "YoyoSquirrel", new[] { "orchard", "Shoe" } },
            { "FirstSeed", new[] { "orchard", "Root" } },
            { "JackInTheBox", new[] { "oddity", "Big-top" } },
            { "LostCarousel", new[] { "oddity", "Big-top" } },
            { "SpecimenSlime", new[] { "frost", "tidal" } },
            { "CourtHerald", new[] { "regal", "Mask" } },
            { "StitchedPangolin", new[] { "stone", "Horn" } },
            { "SaddleMoth", new[] { "dusk", "Clover" } },
        };

        [MenuItem("Arcane Brigade/Content/Put the bond rework in")]
        public static void Run()
        {
            var catalog = Resources.Load<RunCatalog>("RunCatalog");
            var bonds = catalog.Bonds.ToList();
            foreach (Spec spec in Specs)
            {
                BondDefinition bond = bonds.Find(b => b.Id == spec.Id);
                if (bond == null) { bond = new BondDefinition { Id = spec.Id, Icon = bonds.Select(b => b.Icon).FirstOrDefault(i => i != null) }; bonds.Add(bond); }
                bond.Name = spec.Name; bond.Threshold = 2; bond.Effect = spec.Effect; bond.Value = spec.Value; bond.ExtraBonuses = new BondThresholdBonus[0];
                bond.Steps = spec.Steps.Select(s => new BondStep { Count = s.Item1, Effect = s.Item2, Value = s.Item3 }).ToArray();
            }
            catalog.Bonds = bonds.ToArray();
            EditorUtility.SetDirty(catalog);
            int companions = 0, heroes = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:UnitDefinition", new[] { "Assets/ArcaneBrigade/Data", "Assets/ArcaneBrigade/Resources" }))
            {
                var unit = AssetDatabase.LoadAssetAtPath<UnitDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (unit == null) continue;
                if (CompanionTags.TryGetValue(unit.Id, out string[] tags)) { unit.Tags = tags; EditorUtility.SetDirty(unit); companions++; }
                else if (unit.RecruitThemes != null && unit.RecruitThemes.Length > 0 && !(unit.Id.StartsWith("Secret") || unit.GiftOnly))
                {
                    var themes = new List<string>();
                    foreach (string theme in unit.RecruitThemes)
                    {
                        if (theme.StartsWith("rival-")) continue; // the rival-bond exclusion belonged to the old shared theme pairs
                        string mapped = RetiredTheme.TryGetValue(theme, out string to) ? to : theme;
                        if (mapped == "resolve") continue;
                        if (!themes.Contains(mapped)) themes.Add(mapped);
                    }
                    unit.RecruitThemes = themes.ToArray(); EditorUtility.SetDirty(unit); heroes++;
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("BOND_REWORK bonds " + catalog.Bonds.Length + ", companions " + companions + ", heroes " + heroes);
        }
    }
}
