using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Content expansion step 4: the new friends (and the units they bring into a fight) from the character sheets in
    /// Art/Expansion (Docs/Expansion_Art_Worklist.json: 4 poses per row - idle, walk, wind-up, release). A friend enters the catalog
    /// once its sheet is painted; re-runnable.</summary>
    public static class ExpansionUnits
    {
        private const string Art = "Assets/ArcaneBrigade/Art/Expansion/", Data = "Assets/ArcaneBrigade/Data/Expansion/";
        public static readonly (string sheet, string[] rows)[] Sheets =
        {
            ("Characters1", new[] { "KiteAdmiral", "SundaeDuchess", "BonsaiClocksmith", "JugglingJester" }),
            ("Characters2", new[] { "BellDiver", "PaperPlanePigeon", "BobaTadpole", "BakerMouse" }),
            ("Characters3", new[] { "WateringSnail", "RaincoatFrog", "LampGoat", "YoyoSquirrel" }),
            ("Characters4", new[] { "ShovelPenguin", "TopBeetle", "LensOwl", "RainHeron" }),
            ("Characters5", new[] { "TeaDragon", "LighthouseWhale", "JackInTheBox", "KiteDragon" }),
            ("Characters6", new[] { "SundaeGolem", "LostCarousel", "EndlessIndex", "FirstSeed" }),
            ("Characters7", new[] { "EclipseEgg_Ranged", "EclipseEgg_Melee", "EclipseEgg_Support", "FirstSeed_Sapling" }),
            ("Characters8", new[] { "StaticFerret", "StormKitten", "VoltEel", "ShantySeaLion" }),
            ("Characters9", new[] { "BottleGull", "PinnedBeetle", "FossilArmadillo", "SpecimenSlime" }),
            ("Characters10", new[] { "CourtHerald" }),
        };
        /// <summary>Name, default weapon (Mythic and Prismatic friends then get their locked one) and, for friends without an own
        /// skill, a line about them.</summary>
        public static readonly Dictionary<string, (string name, string weapon, string copy)> Friends = new Dictionary<string, (string, string, string)>
        {
            ["PaperPlanePigeon"] = ("Paper Plane Pigeon", "PaperPlaneSquadron", "A pigeon who folds the post into paper planes and lets them chase the trouble."),
            ["BobaTadpole"] = ("Boba Tadpole", "BobaStraw", "A tadpole in a milk-tea cup who sips, then spits pearls in swaying waves."),
            ["BakerMouse"] = ("Baker Mouse", "RollingPin", "A floury mouse baker who rolls everything flat."),
            ["WateringSnail"] = ("Watering Snail", "WateringCan", "A slow snail who waters the garden, and the enemies in it."),
            ["RaincoatFrog"] = ("Raincoat Frog", "PlungerPopper", "A frog in a yellow raincoat with a plunger for every puddle."),
            ["LampGoat"] = ("Lamp Goat", "DragonLanternGlaive", "A goat whose horns hold lanterns, swinging a lantern glaive."),
            ["YoyoSquirrel"] = ("Yo-yo Squirrel", "YoYo", "A springy squirrel who never misses with a yo-yo."),
            ["ShovelPenguin"] = ("Shovel Penguin", "SnowShovel", "A penguin who heaves snow at anyone in the way."),
            ["TopBeetle"] = ("Top Beetle", "SpinningTop", "A clockwork beetle who spins tops into the crowd."),
            ["LensOwl"] = ("Lens Owl", "SunLens", "An owl scholar who focuses sunlight through a reading lens."),
            ["RainHeron"] = ("Rain Heron", "CorkRifle", "A heron who copies the rain into its notebook and takes any weapon offered."),
            ["TeaDragon"] = ("Tea Dragon", "HearthBrick", null),
            ["LighthouseWhale"] = ("Harbor Whale", "PaperLanternBomb", null),
            ["JackInTheBox"] = ("Ringmaster Jack", "PinataBomb", null),
            ["KiteDragon"] = ("Kite Dragon", "TwinMoonStaff", null),
            ["SundaeGolem"] = ("Sundae Golem", "StarKnuckle", null),
            ["LostCarousel"] = ("The Lost Carousel", "CherryFuseBomb", null),
            ["EndlessIndex"] = ("The Endless Index", "PaperShuriken", null),
            ["FirstSeed"] = ("The First Seed", "AcornCluster", null),
            ["StaticFerret"] = ("Static Ferret", "PawMitten", null),
            ["StormKitten"] = ("Storm Kitten", "DowsingRod", null),
            ["VoltEel"] = ("Volt Eel", "SquirtTrio", null),
            ["ShantySeaLion"] = ("Shanty Sea Lion", "SquirtTrio", null),
            ["BottleGull"] = ("Bottle Gull", "FizzBottle", null),
            ["PinnedBeetle"] = ("Pinned Beetle", "CandyDrill", null),
            ["FossilArmadillo"] = ("Fossil Armadillo", "DrumstickClub", null),
            ["SpecimenSlime"] = ("Specimen Slime", "InkyOctopus", null),
            ["CourtHerald"] = ("Court Herald", "CorkRifle", null),
        };
        private static readonly Dictionary<string, Sprite[]> sheets = new Dictionary<string, Sprite[]>();

        /// <summary>The four poses of a row, or null while its sheet is not painted.</summary>
        public static Sprite[] Poses(string id)
        {
            foreach (var (sheet, ids) in Sheets)
            {
                int row = Array.IndexOf(ids, id); if (row < 0) continue;
                string path = Art + sheet + "_v1.png";
                if (!File.Exists(path)) return null;
                if (!sheets.TryGetValue(sheet, out Sprite[] all) || all == null || all.Length == 0 || all[0] == null)
                    sheets[sheet] = all = ids.Length == 1
                        ? K25AtlasImporter.Import(path, "Expansion" + sheet, 4, 1, Enumerable.Range(0, 4).ToArray(), null, true, true)
                        : K25AtlasImporter.Import(path, "Expansion" + sheet, 4, 4, Enumerable.Range(0, 16).ToArray(), new[] { 0, 256, 512, 768, 1024 }, true, true);
                return all.Skip(row * 4).Take(4).ToArray();
            }
            return null;
        }
        private static T Asset<T>(string path) where T : ScriptableObject
        {
            T a = AssetDatabase.LoadAssetAtPath<T>(path);
            if (a == null) { a = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(a, path); }
            return a;
        }
        public static void Animate(UnitDefinition u, Sprite[] frames)
        {
            u.Body = frames[0]; u.AnimationFrames = frames;
            AuthoredMotionSet m = Asset<AuthoredMotionSet>(Data + u.Id + "Motion.asset");
            m.Idle = new[] { frames[0], frames[0] }; m.Walk = new[] { frames[0], frames[1], frames[0], frames[1] };
            m.Windup = new[] { frames[2], frames[2] }; m.Release = new[] { frames[3], frames[3] }; m.Recovery = new[] { frames[3], frames[0] };
            u.AuthoredMotion = m; EditorUtility.SetDirty(m);
        }

        /// <summary>Every new friend whose sheet exists joins the catalog (its bonds, rarity and locked weapon are then set by
        /// ContentExpansionBuilder); the summoned units and Eclipse Egg's forms go to ExpansionArtwork.</summary>
        public static void BuildCompanions(RunCatalog catalog, ContentExpansionBuilder.Plan plan)
        {
            Directory.CreateDirectory(Data);
            var list = catalog.Companions.ToList();
            string archive = catalog.Unit("ArchiveCurator")?.ReferenceHero ?? "";
            foreach (var row in plan.companionList.Where(r => r.isNew))
            {
                Sprite[] frames = Poses(row.id); if (frames == null) continue;
                var spec = Friends[row.id]; ItemRarity rarity = ContentExpansionBuilder.Rarity(row.rarity);
                UnitDefinition u = Asset<UnitDefinition>(Data + row.id + ".asset");
                if (string.IsNullOrEmpty(u.Id)) { EditorUtility.CopySerialized(catalog.Unit("BrassAcorn"), u); u.name = row.id; }
                u.Id = row.id; u.DisplayName = spec.name; u.ReferenceCompanion = ""; u.ReferenceHero = ""; u.HeroMechanic = HeroMechanic.None;
                u.Description = CombatSimulation.SkillCopy.TryGetValue(row.id, out string skill) ? skill : spec.copy;
                u.Tags = new[] { row.theme, row.minor }; u.RecruitThemes = Array.Empty<string>(); u.Rarity = rarity;
                u.SpecialRecruit = rarity >= ItemRarity.Mythic; u.GiftOnly = false; u.RecruitThemeLocked = false;
                u.ExclusiveHero = row.minor == "exhibit-hall" ? archive : "";
                u.UnlockWave = rarity == ItemRarity.Rainbow ? int.MaxValue : rarity == ItemRarity.Mythic ? 4 : rarity == ItemRarity.Rare ? 3 : rarity == ItemRarity.Uncommon ? 1 : 0;
                u.Price = rarity == ItemRarity.Common ? 22 : rarity == ItemRarity.Uncommon ? 26 : rarity == ItemRarity.Rare ? 28 : rarity == ItemRarity.Mythic ? 32 : 40;
                u.Weapon = catalog.Weapon(spec.weapon) ?? throw new InvalidOperationException("Weapon missing for " + row.id + ": " + spec.weapon);
                u.CompanionModifiers = Array.Empty<StatModifier>(); u.ChampionModifiers = Array.Empty<StatModifier>(); u.Passive = PassiveKind.None;
                u.Tint = Color.white; u.BodyHeight = .75f; u.BodyScale = 1; u.Health = 100; u.Speed = 4; u.Radius = .22f; u.CombatTier = 1; u.IsBoss = false;
                Animate(u, frames); EditorUtility.SetDirty(u);
                if (!list.Contains(u)) list.Add(u);
            }
            catalog.Companions = list.ToArray(); EditorUtility.SetDirty(catalog);

            ExpansionArtwork art = Asset<ExpansionArtwork>("Assets/ArcaneBrigade/Resources/ExpansionArtwork.asset");
            UnitDefinition Summon(string id, string copyOf, string weapon, float scale)
            {
                Sprite[] poses = Poses(id); UnitDefinition source = catalog.Unit(copyOf);
                if (poses == null || source == null) return null;
                UnitDefinition s = Asset<UnitDefinition>(Data + id + ".asset");
                EditorUtility.CopySerialized(source, s);
                s.name = id; s.Id = id; s.BodyScale = scale; s.Price = 0; s.UnlockWave = int.MaxValue; s.GiftOnly = true;
                s.Weapon = catalog.Weapon(weapon) ?? source.Weapon;
                Animate(s, poses); EditorUtility.SetDirty(s);
                return s;
            }
            art.EclipseForms = new[] { Summon("EclipseEgg_Ranged", "EclipseEgg", "SaucerBlaster", 1), Summon("EclipseEgg_Melee", "EclipseEgg", "StarKnuckle", 1), Summon("EclipseEgg_Support", "EclipseEgg", "FortuneGlobe", 1) }
                .Where(f => f != null).ToArray();
            art.Sapling = Summon("FirstSeed_Sapling", "FirstSeed", "AcornSling", .7f);
            if (art.Sapling != null) { art.Sapling.DisplayName = "Seedling"; art.Sapling.Tags = Array.Empty<string>(); }
            if (catalog.Unit("SpecimenSlime") is UnitDefinition slime)
            {
                UnitDefinition mini = Asset<UnitDefinition>(Data + "SpecimenSlime_Mini.asset");
                EditorUtility.CopySerialized(slime, mini);
                mini.name = "SpecimenSlime_Mini"; mini.Id = "SpecimenSlime_Mini"; mini.DisplayName = "Little Slime"; mini.BodyScale = .6f; mini.GiftOnly = true; mini.UnlockWave = int.MaxValue;
                mini.Tags = Array.Empty<string>(); EditorUtility.SetDirty(mini); art.MiniSlime = mini;
            }
            // the shop visitors: sheets of four, two per row, two poses each (Tools/Art/visitors_art.py)
            var visitors = new Sprite[ShopVisitors.All.Length * 2];
            for (int sheet = 0; sheet * 4 < ShopVisitors.All.Length; sheet++)
            {
                string path = Art + "Visitors" + (sheet + 1) + "_v1.png";
                if (!File.Exists(path)) continue;
                int count = Math.Min(4, ShopVisitors.All.Length - sheet * 4), rows = (count + 1) / 2;
                Sprite[] cells = rows == 2
                    ? K25AtlasImporter.Import(path, "ExpansionVisitors" + (sheet + 1), 4, 2, Enumerable.Range(0, count * 2).ToArray(), new[] { 0, 512, 1024 }, true, true)
                    : K25AtlasImporter.Import(path, "ExpansionVisitors" + (sheet + 1), 4, 1, Enumerable.Range(0, count * 2).ToArray(), null, true, true);
                for (int k = 0; k < count; k++) { visitors[(sheet * 4 + k) * 2] = cells[k * 2]; visitors[(sheet * 4 + k) * 2 + 1] = cells[k * 2 + 1]; }
            }
            art.Visitors = visitors;
            EditorUtility.SetDirty(art);
        }
    }
}
