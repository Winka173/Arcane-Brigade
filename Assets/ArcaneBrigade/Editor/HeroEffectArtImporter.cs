using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Imports the delivered physical-object animation sheets and rebuilds the runtime sprite library.</summary>
    public static class HeroEffectArtImporter
    {
        [Serializable] private sealed class Manifest { public int cell_size; public Sheet[] jobs; }
        [Serializable] private sealed class Sheet { public string name; public string path; public int frames; }
        [MenuItem("Arcane Brigade/Art/Import hero effect sheets")]
        public static void Run()
        {
            var manifests = new[] { "Docs/Hero_Effect_Art_Manifest.json", "Docs/Hero_Effect_Art_Manifest_2.json" }
                .Where(File.Exists).Select(p => JsonUtility.FromJson<Manifest>(File.ReadAllText(p))).ToArray();
            if (manifests.Any(m => m.cell_size != 256 || m.jobs == null)) throw new Exception("Unexpected hero effect manifest");
            Sheet[] sheets = manifests.SelectMany(m => m.jobs).ToArray();
            if (sheets.Select(s => s.name).Distinct().Count() != sheets.Length) throw new Exception("Duplicate hero effect sheet");
            int total = 0;
            foreach (Sheet sheet in sheets)
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
                // The six-face dice strip is 1536px wide: a 1024 cap would shrink its 256px cells.
                importer.maxTextureSize = Mathf.Max(1024, Mathf.NextPowerOfTwo(sheet.frames * 256));
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
            if (total != sheets.Sum(s => s.frames)) throw new Exception("Unexpected hero effect frame count: " + total);
            HeroEffectArtLibraryBuilder.Build();
            var library = AssetDatabase.LoadAssetAtPath<HeroEffectArtLibrary>("Assets/ArcaneBrigade/Resources/HeroEffectArtLibrary.asset");
            if (library == null || sheets.Any(s => !library.Entries.Any(e => e.Key == s.name && e.Frames.Length == s.frames)))
                throw new Exception("Hero effect runtime library is incomplete");
            File.WriteAllText("Docs/Hero_Effect_Art_Unity_Verification_2.txt", sheets.Length + " sheets / " + total + " sprites imported and verified in Unity; equal 256x256 cells; NPOT preserved; alpha enabled; no mipmaps; bilinear; clamp; texture cap follows strip width. Runtime HeroEffectArtLibrary rebuilt and all manifest entries verified.\n");
            Debug.Log("HERO_EFFECT_ART_IMPORTED sheets=" + sheets.Length + " sprites=" + total);
        }
    }
}
