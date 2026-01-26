using System;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K19ArtworkBuilder
    {
        public static void Build()
        {
            const string path = "Assets/ArcaneBrigade/Art/K19/EvolutionOrgans_v1.png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 256; importer.maxTextureSize = 4096; importer.isReadable = true;
            importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            Color32[] pixels = texture.GetPixels32();
            if (pixels[0].a != 0) throw new InvalidOperationException("K19 requires real transparent artwork");
            if (texture.width != 1448 || texture.height != 1086) throw new InvalidOperationException("K19 v1 art regions require the original 1448x1086 source");
            Rect[] regions = {
                new Rect(54, 11, 292, 382), new Rect(393, 8, 294, 363), new Rect(735, 6, 320, 381), new Rect(1149, 10, 226, 405),
                new Rect(28, 409, 332, 248), new Rect(399, 402, 228, 246), new Rect(694, 405, 384, 269), new Rect(1090, 423, 330, 236),
                new Rect(31, 677, 360, 391), new Rect(399, 683, 390, 370), new Rect(800, 736, 292, 246), new Rect(1187, 657, 217, 410)
            };
            var factory = new SpriteDataProviderFactories(); factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var previous = provider.GetSpriteRects().ToDictionary(rectangle => rectangle.name, rectangle => rectangle.spriteID);
            var rectangles = new SpriteRect[regions.Length];
            for (int index = 0; index < regions.Length; index++)
            {
                Rect region = regions[index];
                int left = (int)region.xMin, right = (int)region.xMax;
                int bottom = texture.height - (int)region.yMax, top = texture.height - (int)region.yMin;
                int minX = right, minY = top, maxX = left, maxY = bottom;
                for (int vertical = bottom; vertical < top; vertical++)
                for (int horizontal = left; horizontal < right; horizontal++)
                    if (pixels[vertical * texture.width + horizontal].a > 32) { minX = Mathf.Min(minX, horizontal); minY = Mathf.Min(minY, vertical); maxX = Mathf.Max(maxX, horizontal); maxY = Mathf.Max(maxY, vertical); }
                if (maxX <= minX || maxY <= minY) throw new InvalidOperationException("K19 empty art cell " + index);
                string name = "Evolution_" + index.ToString("D2");
                rectangles[index] = new SpriteRect { name = name, rect = new Rect(left, bottom, right - left, top - bottom), pivot = new Vector2(0.5f, 0.5f), alignment = SpriteAlignment.Center, spriteID = previous.TryGetValue(name, out GUID identity) ? identity : GUID.Generate() };
            }
            provider.SetSpriteRects(rectangles); provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rectangles.Select(rectangle => new SpriteNameFileIdPair(rectangle.name, rectangle.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings { name = "Standalone", overridden = true, maxTextureSize = 4096, format = TextureImporterFormat.BC7 });
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings { name = "Android", overridden = true, maxTextureSize = 4096, format = TextureImporterFormat.ASTC_6x6 });
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings { name = "iPhone", overridden = true, maxTextureSize = 4096, format = TextureImporterFormat.ASTC_6x6 });
            importer.isReadable = false; importer.SaveAndReimport();
            const string asset = "Assets/ArcaneBrigade/Resources/EvolutionArtwork.asset";
            EvolutionArtwork artwork = AssetDatabase.LoadAssetAtPath<EvolutionArtwork>(asset);
            if (artwork == null) { artwork = ScriptableObject.CreateInstance<EvolutionArtwork>(); AssetDatabase.CreateAsset(artwork, asset); }
            artwork.Parts = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(sprite => sprite.name).ToArray();
            EditorUtility.SetDirty(artwork); AssetDatabase.SaveAssets();
            K19EncounterChecks.Verify();
        }
    }
}
