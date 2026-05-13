using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Imports only the delivered physical-object animation sheets. Does not replace runtime effects or art.</summary>
    public static class HeroEffectArtImporter
    {
        [Serializable] private sealed class Manifest { public int cell_size; public Sheet[] jobs; }
        [Serializable] private sealed class Sheet { public string name; public string path; public int frames; }
        [MenuItem("Arcane Brigade/Art/Import hero effect sheets")]
        public static void Run()
        {
            var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText("Docs/Hero_Effect_Art_Manifest.json"));
            if (manifest.cell_size != 256 || manifest.jobs.Length != 14) throw new Exception("Unexpected hero effect manifest");
            int total = 0;
            foreach (Sheet sheet in manifest.jobs)
            {
                if (!sheet.path.StartsWith("Assets/ArcaneBrigade/Art/Playtest/HeroEffects/", StringComparison.Ordinal) || !File.Exists(sheet.path))
                    throw new Exception("Missing or invalid art path: " + sheet.path);
                AssetDatabase.ImportAsset(sheet.path, ImportAssetOptions.ForceSynchronousImport);
                var importer = AssetImporter.GetAtPath(sheet.path) as TextureImporter;
                if (importer == null) throw new Exception("No texture importer: " + sheet.path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Multiple;
                importer.spritePixelsPerUnit = 256;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.maxTextureSize = 1024;
                importer.textureCompression = TextureImporterCompression.Compressed;
                importer.compressionQuality = 80;
#pragma warning disable 618
                importer.spritesheet = Enumerable.Range(0, sheet.frames).Select(i => new SpriteMetaData {
                    name = sheet.name + "_f" + i, rect = new Rect(i * 256, 0, 256, 256),
                    alignment = (int)SpriteAlignment.Center, pivot = new Vector2(.5f, .5f)
                }).ToArray();
#pragma warning restore 618
                importer.SaveAndReimport();
                var frames = AssetDatabase.LoadAllAssetsAtPath(sheet.path).OfType<Sprite>().OrderBy(s => s.name).ToArray();
                if (frames.Length != sheet.frames || frames.Any(s => s.rect.width != 256 || s.rect.height != 256))
                    throw new Exception("Sprite slicing failed: " + sheet.name);
                total += frames.Length;
            }
            if (total != 40) throw new Exception("Unexpected hero effect frame count: " + total);
            File.WriteAllText("Docs/Hero_Effect_Art_Unity_Verification.txt", "14 sheets / 40 sprites imported and verified in Unity; equal 256x256 cells; NPOT preserved; alpha enabled; no mipmaps; bilinear; clamp; max texture 1024. Runtime effect replacement is a separate integration step.\n");
            Debug.Log("HERO_EFFECT_ART_IMPORTED sheets=14 sprites=" + total);
        }
    }
}
