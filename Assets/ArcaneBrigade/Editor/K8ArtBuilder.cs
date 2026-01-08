using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K8ArtBuilder
    {
        public static void Apply()
        {
            var bosses = K7QualityBuilder.Import("BossesAndProps_v1", new[] { "ClockCrab", "InkKing", "PillowMaw", "ChandelierMoth", "DoorKnight", "Landmarks" }, new[] { 0, 256, 518, 760, 1020, 1280, 1536 });
            var heroes = K7QualityBuilder.Import("HeroIdentity_v1", new[] { "LanternHero", "PostHero", "DreamHero", "CandleGhost", "JellyKnight" }, new[] { 0, 330, 602, 904, 1250, 1536 });
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            string[] identities = { "ClockCrab", "InkKing", "PillowMaw", "ChandelierMoth", "DoorKnight" };
            string[] names = { "Cua Đồng Hồ", "Vua Mực", "Gối Háu Ăn", "Bướm Đèn Chùm", "Hiệp Sĩ Cánh Cửa" };
            for (int index = 0; index < catalog.Bosses.Length; index++)
            {
                UnitDefinition boss = catalog.Bosses[index]; boss.AnimationFrames = bosses[identities[index]]; boss.Body = boss.AnimationFrames[0]; boss.DisplayName = names[index]; EditorUtility.SetDirty(boss);
            }
            string[] units = { "LanternKnight", "Postmaster", "DrowsySeer", "HoodWisp", "JellySwordsman" };
            string[] forms = { "LanternHero", "PostHero", "DreamHero", "CandleGhost", "JellyKnight" };
            for (int index = 0; index < units.Length; index++)
            {
                UnitDefinition unit = catalog.Unit(units[index]);
                if (unit == null) throw new InvalidOperationException("Missing hero identity: " + units[index]);
                unit.AnimationFrames = heroes[forms[index]]; unit.Body = unit.AnimationFrames[0]; EditorUtility.SetDirty(unit);
            }
            PaintedAnimationLibrary art = Resources.Load<PaintedAnimationLibrary>("PaintedAnimationLibrary"); art.EnvironmentProps = bosses["Landmarks"]; EditorUtility.SetDirty(art);
            var merchants = K7QualityBuilder.Import("StrangeMerchants_v2", new[] { "ReceiptMaw", "EggOracle", "MarketDecor" }, new[] { 0, 524, 1126, 1536 });
            art.GoldMerchant = merchants["ReceiptMaw"]; art.EggMerchant = merchants["EggOracle"]; art.MarketDecor = merchants["MarketDecor"];
            art.MarketRugs = ImportRugs();
            AssetDatabase.SaveAssets();
            if (catalog.Bosses.Select(boss => boss.Body).Distinct().Count() != catalog.Bosses.Length) throw new InvalidOperationException("Boss silhouettes must be distinct");
        }
        private static Sprite[] ImportRugs()
        {
            const string path = "Assets/ArcaneBrigade/Art/AnimationHD/MarketRugs_v3.png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 256; importer.maxTextureSize = 4096; importer.isReadable = true;
            importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.npotScale = TextureImporterNPOTScale.None;
            importer.filterMode = FilterMode.Bilinear; importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            var factory = new SpriteDataProviderFactories(); factory.Init();
            ISpriteEditorDataProvider provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            Dictionary<string, GUID> prior = provider.GetSpriteRects().ToDictionary(rectangle => rectangle.name, rectangle => rectangle.spriteID);
            var rectangles = new List<SpriteRect>();
            for (int index = 0; index < 2; index++)
            {
                string name = index == 0 ? "GoldMarketRug" : "DreamMarketRug";
                rectangles.Add(new SpriteRect { name = name, rect = new Rect(0, (1 - index) * texture.height / 2, texture.width, texture.height / 2), alignment = SpriteAlignment.Center, pivot = Vector2.one * 0.5f, spriteID = prior.TryGetValue(name, out GUID identity) ? identity : GUID.Generate() });
            }
            provider.SetSpriteRects(rectangles.ToArray());
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rectangles.Select(rectangle => new SpriteNameFileIdPair(rectangle.name, rectangle.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
            var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToDictionary(sprite => sprite.name);
            return new[] { sprites["GoldMarketRug"], sprites["DreamMarketRug"] };
        }
    }
}
