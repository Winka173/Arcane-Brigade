using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Requirement 23/10/2026: the thin hero families (kills, delay, merge, freeze, marks; 4-6 fitting relics each) get
    /// three relics of their own, sold only to heroes of that family. Stats only. Icons come from
    /// Art/Playtest/FamilyRelics/&lt;Id&gt;.png once painted; until then each borrows a fitting existing relic's icon.</summary>
    public static class FamilyRelicsBuilder
    {
        private const string ArtFolder = "Assets/ArcaneBrigade/Art/Playtest/FamilyRelics/";

        [MenuItem("Arcane Brigade/Build Family Relics")]
        public static void Build()
        {
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            var specs = new (string id, string name, string text, ItemRarity rarity, int price, string family, string borrowed, StatModifier[] mods)[]
            {
                ("SpecimenJar", "Specimen Jar", "Team damage +8%, crit +5%.", ItemRarity.Uncommon, 35, "fam-kills", "SpecimenPin", new[] { M(ItemEffect.Damage, .08f), M(ItemEffect.Crit, .05f) }),
                ("FieldNotes", "Field Notes", "Hero damage +20%, luck +0.1.", ItemRarity.Rare, 55, "fam-kills", "CurioHungryLedger", new[] { H(ItemEffect.ChampionDamage, .2f), M(ItemEffect.Luck, .1f) }),
                ("TrophyShelf", "Trophy Shelf", "Team damage +15%, wave income +4.", ItemRarity.Mythic, 75, "fam-kills", "TallyChalk", new[] { M(ItemEffect.Damage, .15f), M(ItemEffect.Income, 4) }),
                ("SlowSand", "Slow Sand", "Enemies 5% slower, attack range +10%.", ItemRarity.Uncommon, 35, "fam-delay", "LateFee", new[] { M(ItemEffect.EnemySpeed, -.05f), M(ItemEffect.AttackRange, .1f) }),
                ("PocketHourglass", "Pocket Hourglass", "Hero damage +25%.", ItemRarity.Rare, 55, "fam-delay", "clock-spring", new[] { H(ItemEffect.ChampionDamage, .25f) }),
                ("StoppedWatch", "Stopped Watch", "Team damage +15%, enemies 8% slower.", ItemRarity.Mythic, 75, "fam-delay", "CuckooPendulum", new[] { M(ItemEffect.Damage, .15f), M(ItemEffect.EnemySpeed, -.08f) }),
                ("GraftingTwine", "Grafting Twine", "Friends' damage +10%, max HP +10.", ItemRarity.Uncommon, 35, "fam-merge", "SpliceKit", new[] { M(ItemEffect.CompanionDamage, .1f), H(ItemEffect.Health, 10) }),
                ("SeedCatalogue", "Seed Catalogue", "Friends' attack speed +10%, friends cost 10% less.", ItemRarity.Rare, 55, "fam-merge", "StarPolish", new[] { M(ItemEffect.CompanionAttackSpeed, .1f), M(ItemEffect.RecruitDiscount, .1f) }),
                ("GardenShears", "Gardener's Shears", "Friends' damage +20%, attack speed +8%.", ItemRarity.Mythic, 75, "fam-merge", "RainbowHeart", new[] { M(ItemEffect.CompanionDamage, .2f), M(ItemEffect.CompanionAttackSpeed, .08f) }),
                ("IceCubeTray", "Ice Cube Tray", "Team damage +6%, max HP +15.", ItemRarity.Uncommon, 35, "fam-freeze", "FrostSalt", new[] { M(ItemEffect.Damage, .06f), H(ItemEffect.Health, 15) }),
                ("FrostDoily", "Frost Doily", "Attack speed +10%, enemies 5% slower.", ItemRarity.Rare, 55, "fam-freeze", "BrainFreeze", new[] { M(ItemEffect.AttackSpeed, .1f), M(ItemEffect.EnemySpeed, -.05f) }),
                ("SnowGlobe", "Snow Globe", "Team damage +15%, enemies 8% slower.", ItemRarity.Mythic, 75, "fam-freeze", "MeltingSundae", new[] { M(ItemEffect.Damage, .15f), M(ItemEffect.EnemySpeed, -.08f) }),
                ("RubberStamp", "Rubber Stamp", "Hero damage +10%, crit +6%.", ItemRarity.Uncommon, 35, "fam-marks", "PostageStamp", new[] { H(ItemEffect.ChampionDamage, .1f), M(ItemEffect.Crit, .06f) }),
                ("AddressBook", "Address Book", "Shots pierce 1 more, ranged damage +3.", ItemRarity.Rare, 55, "fam-marks", "CurioFangPin", new[] { M(ItemEffect.BulletPierce, 1), M(ItemEffect.RangedDamage, 3) }),
                ("WaxSealSet", "Wax Seal Set", "Team damage +15%, crit +8%.", ItemRarity.Mythic, 75, "fam-marks", "echo-postage", new[] { M(ItemEffect.Damage, .15f), M(ItemEffect.Crit, .08f) }),
            };
            var ids = specs.Select(s => s.id).ToArray();
            var items = catalog.Items.Where(item => !ids.Contains(item.Id)).ToList();
            Sprite Icon(string id, string borrowed)
            {
                string path = ArtFolder + id + ".png";
                if (AssetImporter.GetAtPath(path) is TextureImporter importer && importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single; importer.alphaIsTransparency = true; importer.SaveAndReimport();
                }
                return AssetDatabase.LoadAssetAtPath<Sprite>(path) ?? catalog.Items.FirstOrDefault(item => item.Id == borrowed)?.Icon;
            }
            foreach (var s in specs)
                items.Add(new ItemDefinition
                {
                    Id = s.id, Name = s.name, Description = s.text, Icon = Icon(s.id, s.borrowed), Price = s.price, Rarity = s.rarity, Stacking = StackPolicy.Additive,
                    MaximumStacks = 3, ShopWeight = 100, Pools = ShopPool.Gold, AltarPrice = 4, Mechanic = RelicMechanic.None, Category = RelicCategory.Curio,
                    Effect = ItemEffect.Damage, Value = 0, Modifiers = s.mods, UnlockWave = 1, ShopThemes = new[] { s.family }
                });
            catalog.Items = items.ToArray();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log("Family relics: " + specs.Length + " items (" + ids.Count(id => AssetDatabase.LoadAssetAtPath<Sprite>(ArtFolder + id + ".png") != null) + " with their own icon).");
        }
        private static StatModifier M(ItemEffect effect, float value) => new StatModifier { Effect = effect, Scope = RunModifierRules.DefaultScope(effect), Value = value };
        private static StatModifier H(ItemEffect effect, float value) => new StatModifier { Effect = effect, Scope = ModifierScope.Champion, Value = value };
    }
}
