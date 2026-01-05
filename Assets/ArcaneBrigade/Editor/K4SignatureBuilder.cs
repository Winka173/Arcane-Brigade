using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K4SignatureBuilder
    {
        [MenuItem("Arcane Brigade/K4/Apply signature weapons check and build")]
        public static void ApplyAndBuild() { Apply(); K4SignatureChecks.Run(); FullRunBuilder.BuildWindows(); }

        public static void Apply()
        {
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            Configure(catalog.Weapon("UmbrellaLance"), SignatureWeapon.Umbrella, 0.45f, 2.1f);
            Configure(catalog.Weapon("BellFork"), SignatureWeapon.Bell, 0.65f, 2.6f);
            string path = "Assets/ArcaneBrigade/Art/QualityHD/DreamStamp_v1.png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 256; importer.maxTextureSize = 4096;
            importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.filterMode = FilterMode.Bilinear;
            importer.npotScale = TextureImporterNPOTScale.None; importer.textureCompression = TextureImporterCompression.Uncompressed;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom; settings.spritePivot = new Vector2(0.26f, 0.5f);
            importer.SetTextureSettings(settings); importer.SaveAndReimport();
            const string assetPath = "Assets/ArcaneBrigade/Data/Run/DreamStamp.asset";
            WeaponDefinition stamp = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(assetPath);
            if (stamp == null)
            {
                stamp = ScriptableObject.CreateInstance<WeaponDefinition>(); stamp.Id = "DreamStamp";
                stamp.Kind = WeaponKind.Melee; stamp.Motion = WeaponMotion.Stamp;
                stamp.Damage = 9; stamp.Range = 1.65f; stamp.Cooldown = 0.75f;
                stamp.Windup = 0.17f; stamp.Recovery = 0.24f; stamp.VisualWidth = 0.95f;
                stamp.Element = DamageElement.Physical; stamp.ConeDot = 0.5f;
                AssetDatabase.CreateAsset(stamp, assetPath);
            }
            stamp.Sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            Configure(stamp, SignatureWeapon.Stamp, 1.1f, 1.7f);
            if (!catalog.Weapons.Any(weapon => weapon.Id == stamp.Id))
            {
                var weapons = new List<WeaponDefinition>(catalog.Weapons) { stamp }; catalog.Weapons = weapons.ToArray();
            }
            EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets();
        }

        private static void Configure(WeaponDefinition weapon, SignatureWeapon signature, float scale, float radius)
        {
            if (weapon.Signature != SignatureWeapon.None) return;
            weapon.Signature = signature; weapon.SignatureScale = scale; weapon.SignatureRadius = radius;
            weapon.SignatureBeats = 3; weapon.SignatureDelay = 0.18f; weapon.MarkLifetime = 2.4f; weapon.PushDistance = 0.35f;
            EditorUtility.SetDirty(weapon);
        }
    }
}
