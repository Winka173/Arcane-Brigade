using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Imports optional hand-painted hero mechanic tokens from Art/MechanicEffects/Tokens/{InkToken}_v1.png into
    /// MechanicEffectArtwork.Tokens. Missing files keep the procedural ink token. Run with -executeMethod
    /// ArcaneBrigade.Editor.HeroMechanicArtImporter.Import after new art arrives.</summary>
    public static class HeroMechanicArtImporter
    {
        public const string Folder = "Assets/ArcaneBrigade/Art/MechanicEffects/Tokens/";
        public static void Import()
        {
            var art = AssetDatabase.LoadAssetAtPath<MechanicEffectArtwork>("Assets/ArcaneBrigade/Resources/MechanicEffectArtwork.asset");
            if (art == null) throw new InvalidOperationException("MechanicEffectArtwork asset missing");
            var names = Enum.GetNames(typeof(InkToken)); var tokens = new Sprite[names.Length]; int found = 0;
            for (int i = 0; i < names.Length; i++)
            {
                string path = Folder + names[i] + "_v1.png";
                if (!File.Exists(path) || names[i] == nameof(InkToken.Die)) continue;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true; importer.mipmapEnabled = true; importer.spritePixelsPerUnit = 256;
                importer.wrapMode = TextureWrapMode.Clamp; importer.maxTextureSize = 1024; importer.spritePivot = new Vector2(.5f, .5f);
                importer.textureCompression = TextureImporterCompression.CompressedHQ; importer.SaveAndReimport();
                tokens[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path); if (tokens[i] != null) found++;
            }
            art.Tokens = found > 0 ? tokens : Array.Empty<Sprite>(); EditorUtility.SetDirty(art); AssetDatabase.SaveAssets();
            Debug.Log("HERO_MECHANIC_TOKENS imported " + found + " painted tokens");
        }
    }
}
