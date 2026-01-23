using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K15Builder
    {
        [MenuItem("Arcane Brigade/K15/Build quality candidate")]
        public static void Build()
        {
            const string path = "Assets/ArcaneBrigade/Art/K15/ImpactFamilies_v1.png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 256; importer.maxTextureSize = 4096; importer.isReadable = true;
            importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture.GetPixel(0, 0).a > 0.02f) throw new InvalidOperationException("K15 impact source requires transparent background");
            var factory = new SpriteDataProviderFactories(); factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var previous = provider.GetSpriteRects().ToDictionary(rectangle => rectangle.name, rectangle => rectangle.spriteID);
            var rectangles = new SpriteRect[16];
            for (int row = 0; row < 4; row++)
            for (int column = 0; column < 4; column++)
            {
                int index = row * 4 + column; string name = "ImpactFamilies_" + index.ToString("D2");
                rectangles[index] = new SpriteRect { name = name, rect = new Rect(column * texture.width / 4f, (3 - row) * texture.height / 4f, texture.width / 4f, texture.height / 4f), pivot = Vector2.one * 0.5f, alignment = SpriteAlignment.Center, spriteID = previous.TryGetValue(name, out GUID identity) ? identity : GUID.Generate() };
            }
            provider.SetSpriteRects(rectangles); provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rectangles.Select(rectangle => new SpriteNameFileIdPair(rectangle.name, rectangle.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings { name = "Standalone", overridden = true, maxTextureSize = 4096, format = TextureImporterFormat.BC7 });
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings { name = "Android", overridden = true, maxTextureSize = 4096, format = TextureImporterFormat.ASTC_6x6 });
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings { name = "iPhone", overridden = true, maxTextureSize = 4096, format = TextureImporterFormat.ASTC_6x6 });
            importer.isReadable = false; importer.SaveAndReimport();
            Sprite[] frames = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(sprite => sprite.name).ToArray();
            const string libraryPath = "Assets/ArcaneBrigade/Resources/ImpactAnimationLibrary.asset";
            ImpactAnimationLibrary library = AssetDatabase.LoadAssetAtPath<ImpactAnimationLibrary>(libraryPath);
            if (library == null) { library = ScriptableObject.CreateInstance<ImpactAnimationLibrary>(); AssetDatabase.CreateAsset(library, libraryPath); }
            library.Cut = frames.Take(4).ToArray(); library.Slam = frames.Skip(4).Take(4).ToArray(); library.Pierce = frames.Skip(8).Take(4).ToArray(); library.Orb = frames.Skip(12).Take(4).ToArray();
            EditorUtility.SetDirty(library); AssetDatabase.SaveAssets();
            Verify();
        }
        public static void Verify()
        {
            K15Checks.Run(); K14Verification.RunChecks(); K15BalanceDiagnostics.Run();
            FullRunBuilder.BuildWindows();
            Directory.CreateDirectory("PreviewExports/K15");
            File.Copy("PreviewExports/FullRun_Build.txt", "PreviewExports/K15/Windows_Build.txt", true);
        }
        public static void Campaign() { K14CampaignDiagnostics.RunInto("PreviewExports/K15"); }
    }
}
