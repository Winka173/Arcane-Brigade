using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>The Lone line (owner 23/10/2026): eight hero-only relics for a hero who fights alone or nearly so. Rules live in
    /// RunRelics.LoneBonus / RelicNewItems; this adds (or refreshes) the items in the run catalog. Icons come from
    /// Art/Playtest/LoneRelics/&lt;Id&gt;.png once painted; until then each borrows a fitting existing relic's icon.</summary>
    public static class LoneRelicsBuilder
    {
        private const string ArtFolder = "Assets/ArcaneBrigade/Art/Playtest/LoneRelics/";
        private static readonly string[] Ids = { "LoneLantern", "EmptyChairs", "SingleTicket", "NightAlone", "SelfPortrait", "LastLight", "IronResolve", "SoloOverture" };

        [MenuItem("Arcane Brigade/Build Lone Relics")]
        public static void Build()
        {
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            var items = catalog.Items.Where(item => !Ids.Contains(item.Id)).ToList();
            Sprite Icon(string id, string borrowed)
            {
                string path = ArtFolder + id + ".png";
                if (AssetImporter.GetAtPath(path) is TextureImporter importer && importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single; importer.alphaIsTransparency = true; importer.SaveAndReimport();
                }
                return AssetDatabase.LoadAssetAtPath<Sprite>(path) ?? catalog.Items.FirstOrDefault(item => item.Id == borrowed)?.Icon;
            }
            ItemDefinition Relic(string id, string name, string text, ItemRarity rarity, int price, RelicMechanic mechanic, string borrowed, params StatModifier[] modifiers)
                => new ItemDefinition
                {
                    Id = id, Name = name, Description = text, Icon = Icon(id, borrowed), Price = price, Rarity = rarity, Stacking = StackPolicy.Unique, MaximumStacks = 1,
                    ShopWeight = 100, Pools = ShopPool.Gold, AltarPrice = rarity == ItemRarity.Rainbow ? 15 : 4, Mechanic = mechanic, Category = RelicCategory.Curio,
                    Effect = ItemEffect.ChampionDamage, Value = 0, Modifiers = modifiers, UnlockWave = 1, ShopThemes = new string[0]
                };
            StatModifier Hero(ItemEffect effect, float value) => new StatModifier { Effect = effect, Scope = ModifierScope.Champion, Value = value };

            items.Add(Relic("LoneLantern", "Lone Lantern", "Hero damage +45%, 8% less for each friend in the fight.", ItemRarity.Rare, 60, RelicMechanic.LoneLantern, "CurioSorrowLantern"));
            items.Add(Relic("EmptyChairs", "Empty Chairs", "Hero attack speed +6% for each empty seat.", ItemRarity.Rare, 60, RelicMechanic.EmptyChairs, "starlight-quilt"));
            items.Add(Relic("SingleTicket", "Single Ticket", "With 2 friends or fewer: one more shot that pierces one more.", ItemRarity.Rare, 55, RelicMechanic.SingleTicket, "OctoberRelic3"));
            items.Add(Relic("NightAlone", "Night Alone", "Alone: the gauge fills 30% faster, transformations last 3s longer.", ItemRarity.Mythic, 80, RelicMechanic.NightAlone, "CurioWatchfulTea"));
            items.Add(Relic("SelfPortrait", "Self-Portrait", "Hero damage +25%, max HP +60.", ItemRarity.Uncommon, 35, RelicMechanic.None, "tarnished-crown",
                Hero(ItemEffect.ChampionDamage, .25f), Hero(ItemEffect.Health, 60)));
            items.Add(Relic("LastLight", "Last Light", "Once a wave under 30% health: +60% damage and attack speed for 6s.", ItemRarity.Rare, 55, RelicMechanic.LastLight, "royal-key"));
            items.Add(Relic("IronResolve", "Iron Resolve", "Hero armor +0.25 and speed +15%, less for each friend.", ItemRarity.Uncommon, 40, RelicMechanic.IronResolve, "CurioStoneCrust"));
            items.Add(Relic("SoloOverture", "Solo Overture", "The hero deals double damage. Friends no longer fight.", ItemRarity.Rainbow, 200, RelicMechanic.SoloOverture, "RainbowCrown"));
            catalog.Items = items.ToArray();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log("Lone relics: " + Ids.Length + " items in the catalog (" + Ids.Count(id => AssetDatabase.LoadAssetAtPath<Sprite>(ArtFolder + id + ".png") != null) + " with their own icon).");
        }
    }
}
