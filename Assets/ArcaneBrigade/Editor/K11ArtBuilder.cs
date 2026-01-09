using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K11ArtBuilder
    {
        public static void Apply()
        {
            PaintedAnimationLibrary art = Resources.Load<PaintedAnimationLibrary>("PaintedAnimationLibrary");
            art.GiantGoldMerchant = Import("GiantGoldMerchant_v1"); art.GiantEggMerchant = Import("GiantEggMerchant_v1");
            EditorUtility.SetDirty(art); AssetDatabase.SaveAssets();
        }
        private static Sprite[] Import(string name)
        {
            string path = "Assets/ArcaneBrigade/Art/K11/" + name + ".png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 256; importer.maxTextureSize = 4096; importer.isReadable = true;
            importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.filterMode = FilterMode.Bilinear; importer.SaveAndReimport();
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path); Color32[] pixels = texture.GetPixels32();
            if (pixels[0].a > 4) throw new InvalidOperationException("Merchant source requires true alpha: " + name);
            int bottom = texture.height, top = 0;
            for (int vertical = 0; vertical < texture.height; vertical++)
                for (int horizontal = 0; horizontal < texture.width; horizontal++)
                    if (pixels[vertical * texture.width + horizontal].a > 128) { bottom = Mathf.Min(bottom, vertical); top = Mathf.Max(top, vertical); }
            var factory = new SpriteDataProviderFactories(); factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var previous = provider.GetSpriteRects().ToDictionary(rectangle => rectangle.name, rectangle => rectangle.spriteID);
            var rectangles = new List<SpriteRect>();
            for (int index = 0; index < 4; index++)
            {
                string frameName = name + "_" + index; int left = index * texture.width / 4; int right = (index + 1) * texture.width / 4;
                rectangles.Add(new SpriteRect { name = frameName, rect = new Rect(left, bottom, right - left, top - bottom + 1), alignment = SpriteAlignment.Custom, pivot = Vector2.one * 0.5f, spriteID = previous.TryGetValue(frameName, out GUID identity) ? identity : GUID.Generate() });
            }
            provider.SetSpriteRects(rectangles.ToArray()); provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rectangles.Select(rectangle => new SpriteNameFileIdPair(rectangle.name, rectangle.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings { name = "Standalone", overridden = true, maxTextureSize = 4096, format = TextureImporterFormat.BC7, compressionQuality = 100 });
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings { name = "Android", overridden = true, maxTextureSize = 4096, format = TextureImporterFormat.ASTC_6x6, compressionQuality = 100 });
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings { name = "iPhone", overridden = true, maxTextureSize = 4096, format = TextureImporterFormat.ASTC_6x6, compressionQuality = 100 });
            importer.SaveAndReimport();
            Dictionary<string, Sprite> sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToDictionary(sprite => sprite.name);
            return rectangles.Select(rectangle => sprites[rectangle.name]).ToArray();
        }
    }
}
