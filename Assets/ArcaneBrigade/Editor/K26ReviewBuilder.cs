using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K26ReviewBuilder
    {
        [MenuItem("Arcane Brigade/K26/Build reviewed content")]
        public static void Build()
        {
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            var items = catalog.Items.Where(item => string.IsNullOrEmpty(item.ReferenceName) && !item.Id.StartsWith("K26Defense",StringComparison.Ordinal)).ToList();
            if (catalog.ReviewRevision == 0)
                foreach (ItemDefinition item in items)
                {
                    item.Pools = ReviewBalance.ExclusivePool(item);
                    item.Modifiers = item.EffectiveModifiers.Select(modifier => new StatModifier { Effect = modifier.Effect, Scope = modifier.Scope, Value = ReviewBalance.RelicValue(modifier.Effect,modifier.Value) }).ToArray();
                }
            ItemDefinition[] imported = ReferenceRelicCatalog.Create();
            for (int atlas = 0; atlas < 5; atlas++)
            {
                string path = "Assets/ArcaneBrigade/Art/K26/Relics" + (atlas + 1) + "_v1.png";
                if (!File.Exists(path)) continue;
                int count = Mathf.Min(16,imported.Length - atlas * 16);
                Sprite[] icons = K25AtlasImporter.Import(path,"K26Relic" + (atlas + 1),4,4,Enumerable.Range(0,count).ToArray());
                for (int index = 0; index < count; index++) { imported[atlas * 16 + index].Icon = icons[index]; items.Add(imported[atlas * 16 + index]); }
            }
            Sprite[] defensive = K25AtlasImporter.Import("Assets/ArcaneBrigade/Art/K26/DefensiveRelics_v1.png","K26Defense",4,2,Enumerable.Range(0,8).ToArray());
            string[] names = { "Marrow Tea","Porcelain Splint","Escape Thimble","Quicksole","Pulse Bell","Mercy Spindle","Iron Apron","Mending Moth" };
            ItemEffect[] first = { ItemEffect.Health,ItemEffect.Armor,ItemEffect.Dodge,ItemEffect.MoveSpeed,ItemEffect.HealthCorrection,ItemEffect.Heal,ItemEffect.Armor,ItemEffect.Heal };
            float[] values = { 50,.35f,.12f,.18f,.25f,2.8f,.65f,2.5f };
            ItemEffect[] second = { ItemEffect.Heal,ItemEffect.MoveSpeed,ItemEffect.MoveSpeed,ItemEffect.Health,ItemEffect.Heal,ItemEffect.Income,ItemEffect.MoveSpeed,ItemEffect.Armor };
            float[] others = { .8f,-.04f,.12f,20,1.4f,-2,-.08f,.12f };
            for (int index = 0; index < names.Length; index++)
            {
                var item = new ItemDefinition { Id = "K26Defense" + index,Name = names[index],Icon = defensive[index],Price = 35 + index * 5,AltarPrice = index >= 4 ? 4 : 2,
                    Rarity = (ItemRarity)(index / 2),Pools = index < 4 ? ShopPool.Gold : ShopPool.Eggs,MaximumStacks = 4,UnlockWave = index < 4 ? 1 : 4,
                    Modifiers = new[] { new StatModifier { Effect = first[index],Value = values[index],Scope = RunModifierRules.DefaultScope(first[index]) },new StatModifier { Effect = second[index],Value = others[index],Scope = RunModifierRules.DefaultScope(second[index]) } } };
                item.Description = ShortOfferCopy.Item(item); items.Add(item);
            }
            catalog.Items = items.ToArray(); catalog.ReviewRevision = 1;
            string carrierPath = "Assets/ArcaneBrigade/Resources/CarrierArtwork.asset";
            CarrierArtwork artwork = AssetDatabase.LoadAssetAtPath<CarrierArtwork>(carrierPath);
            if (artwork == null) { artwork = ScriptableObject.CreateInstance<CarrierArtwork>(); AssetDatabase.CreateAsset(artwork,carrierPath); }
            Sprite Load(string id) => K25AtlasImporter.Import("Assets/ArcaneBrigade/Art/K26/" + id + "_v1.png",id,1,1,new[] { 0 })[0];
            artwork.Hand = new[] { Load("HandOpen"),Load("HandGrip"),Load("HandToss") };
            artwork.Tongue = new[] { Load("TongueRoll"),Load("TongueCurl"),Load("TongueWhip") };
            artwork.Eye = new[] { Load("EyeOpen"),Load("EyeClosing"),Load("EyeClosed") };
            EditorUtility.SetDirty(artwork); EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets();
            Directory.CreateDirectory("PreviewExports/K26");
            File.WriteAllLines("PreviewExports/K26/Relic_Coverage.tsv",new[] { "Reference\tAdaptation\tMechanic\tInstalled" }.Concat(imported.Select(item => item.ReferenceName + "\t" + item.Name + "\t" + item.Mechanic + "\t" + (item.Icon != null ? "yes" : "no: art pending"))));
            K26RelicChecks.Run();
        }
        public static void Verify() => K26RelicChecks.Run();
    }
}
