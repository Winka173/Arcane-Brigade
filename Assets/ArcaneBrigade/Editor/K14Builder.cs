using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K14Builder
    {
        private const string Root = "Assets/ArcaneBrigade/Data/Run/";
        public static void Apply()
        {
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            Sprite[] arms = ImportGrid("ClockworkArsenal_v3", 3, 2);
            Sprite[] bodies = ImportGrid("CarnivalCompanions_v1", 4, 3);
            Sprite[] shots = ImportGrid("CarnivalProjectiles_v1", 3, 1);
            string[] ids = { "ClockworkYoYo", "ScrapMagnet", "NeedleSinger", "SpringKnocker", "DreamProjector", "RibbonWheel" };
            CarnivalWeapon[] mechanics = { CarnivalWeapon.YoYo, CarnivalWeapon.Magnet, CarnivalWeapon.Sewing, CarnivalWeapon.SpringHammer, CarnivalWeapon.Projector, CarnivalWeapon.RibbonWheel };
            float[] damages = { 15, 10, 5, 24, 13, 17 };
            float[] ranges = { 3.8f, 3.6f, 6.4f, 2.4f, 6.2f, 2.3f };
            float[] cooldowns = { 1.1f, 1.2f, 0.95f, 1.45f, 1.65f, 1.05f };
            string[] weaponSummaries = { "Homing throw. Returns on its tether.", "Magnet bolt yanks its target and 3 neighbours in.", "3 piercing needles. Briefly slows on hit.", "Slow charge. Heavy splash and knockback.", "Projects a lingering damage pool. Dream only.", "Two delayed ribbon cuts in a wide arc." };
            for (int index = 0; index < ids.Length; index++)
            {
                WeaponDefinition weapon = LoadOrCreate<WeaponDefinition>(ids[index]);
                weapon.Id = ids[index]; weapon.Sprite = arms[index]; weapon.Mechanic = mechanics[index];
                weapon.GameplaySummary = weaponSummaries[index];
                weapon.Kind = index == 0 || index == 3 || index == 5 ? WeaponKind.Melee : WeaponKind.Projectile;
                weapon.Motion = index == 3 ? WeaponMotion.Slam : index == 0 || index == 5 ? WeaponMotion.Slash : index == 2 ? WeaponMotion.Recoil : WeaponMotion.Cast;
                weapon.Damage = damages[index]; weapon.Range = ranges[index]; weapon.Cooldown = cooldowns[index];
                weapon.Windup = index == 3 ? 0.42f : index == 4 ? 0.3f : 0.2f;
                weapon.Recovery = index == 3 ? 0.32f : 0.24f;
                weapon.VisualWidth = weapon.Kind == WeaponKind.Melee ? 1.15f : 0.95f;
                weapon.Element = index == 3 || index == 5 ? DamageElement.Physical : DamageElement.Arcane;
                weapon.BodyAttackTilt = index == 3 ? 18 : 10; weapon.SwingAngle = index == 5 ? 100 : 70;
                weapon.MountDistance = 0.34f; weapon.MuzzleDistance = 0.65f;
                weapon.ProjectileCount = index == 2 ? 3 : 1; weapon.ProjectileSpread = index == 2 ? 0.085f : 0.14f;
                weapon.ProjectileSpeed = index == 0 ? 5.5f : 7;
                weapon.ProjectileLifetime = 2.4f; weapon.ProjectileRadius = index == 0 ? 0.18f : 0.05f;
                weapon.ProjectileVisualWidth = index == 0 ? 0.5f : index == 2 ? 0.42f : 0.16f;
                weapon.ProjectileSprite = index == 0 ? shots[0] : index == 2 ? shots[1] : shots[2];
                weapon.ProjectileColor = index == 2 ? new Color(0.8f, 0.5f, 1) : Color.white;
                weapon.ProjectilePierce = index == 2 ? 1 : 0;
                weapon.Family = index == 0 ? ExpandedWeapon.Fish : ExpandedWeapon.None;
                weapon.HomingOutbound = index == 0; weapon.SpecialOnly = index == 4;
                weapon.PushDistance = index == 3 ? 0.7f : 0.12f;
                if (!catalog.Weapons.Contains(weapon)) catalog.Weapons = catalog.Weapons.Concat(new[] { weapon }).ToArray();
                EditorUtility.SetDirty(weapon);
            }
            string[] recruits = { "CoilKettle", "ThimbleMoth", "ReelOwl" };
            string[] names = { "Coil Kettle", "Thimble Moth", "Reel Owl" };
            string[] tags = { "hearth", "archive", "astral" };
            string[] summaries = { "YOU: +4% knockback, -1% speed. Heals every 4s.", "ALLIES: +4% attack speed, -3% damage. Fast stitch support.", "ALLIES: +6% range, -2% attack speed. Dream recruit." };
            for (int row = 0; row < recruits.Length; row++)
            {
                UnitDefinition unit = LoadOrCreate<UnitDefinition>(recruits[row]);
                unit.Id = recruits[row]; unit.DisplayName = names[row]; unit.Description = summaries[row];
                unit.AnimationFrames = bodies.Skip(row * 4).Take(4).ToArray(); unit.Body = unit.AnimationFrames[0];
                unit.Weapon = catalog.Weapon(ids[row == 0 ? 3 : row == 1 ? 2 : 4]);
                unit.Tags = new[] { tags[row] }; unit.RecruitThemes = new[] { tags[row] };
                unit.SpecialRecruit = row == 2; unit.UnlockWave = row == 2 ? 6 : 2;
                unit.RecruitThemeLocked = true;
                unit.Health = 100; unit.Speed = row == 0 ? 4.6f : 5.1f; unit.BodyHeight = row == 0 ? 0.98f : 0.92f;
                unit.Radius = 0.27f; unit.Price = row == 2 ? 4 : 26; unit.Tint = Color.white;
                unit.Material = row == 0 ? BodyMaterial.Metal : row == 1 ? BodyMaterial.Fabric : BodyMaterial.Glass;
                unit.Passive = row == 0 ? PassiveKind.HealChampion : PassiveKind.None; unit.PassiveStrength = 0.45f;
                unit.CompanionModifiers = row == 0 ? new[] { new StatModifier { Effect = ItemEffect.Knockback, Scope = ModifierScope.Champion, Value = 0.04f }, new StatModifier { Effect = ItemEffect.MoveSpeed, Scope = ModifierScope.Champion, Value = -0.01f } } : row == 1 ? new[] { new StatModifier { Effect = ItemEffect.AttackSpeed, Scope = ModifierScope.Companions, Value = 0.04f }, new StatModifier { Effect = ItemEffect.Damage, Scope = ModifierScope.Companions, Value = -0.03f } } : new[] { new StatModifier { Effect = ItemEffect.AttackRange, Scope = ModifierScope.Companions, Value = 0.06f }, new StatModifier { Effect = ItemEffect.AttackSpeed, Scope = ModifierScope.Companions, Value = -0.02f } };
                if (!catalog.Companions.Contains(unit)) catalog.Companions = catalog.Companions.Concat(new[] { unit }).ToArray();
                EditorUtility.SetDirty(unit);
            }
            EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets();
            K14Checks.Run(); K14BalanceDiagnostics.Measure();
            K14Verification.Run();
            File.Copy("PreviewExports/FullRun_Build.txt", "PreviewExports/K14/Windows_Build.txt", true);
        }

        private static T LoadOrCreate<T>(string id) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(Root + id + ".asset");
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(asset, Root + id + ".asset"); return asset;
        }

        private static Sprite[] ImportGrid(string id, int columns, int rows)
        {
            string path = "Assets/ArcaneBrigade/Art/K14/" + id + ".png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 256; importer.maxTextureSize = 4096; importer.isReadable = true;
            importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path); Color32[] pixels = texture.GetPixels32();
            if (pixels[0].a > 4) throw new InvalidOperationException("K14 source requires alpha: " + id);
            var factory = new SpriteDataProviderFactories(); factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var previous = provider.GetSpriteRects().ToDictionary(rectangle => rectangle.name, rectangle => rectangle.spriteID);
            var rectangles = new SpriteRect[columns * rows];
            for (int row = 0; row < rows; row++)
            for (int column = 0; column < columns; column++)
            {
                int index = row * columns + column;
                int left = column * texture.width / columns, right = (column + 1) * texture.width / columns;
                if (id == "CarnivalProjectiles_v1" && column == 0) right = left + Mathf.RoundToInt((right - left) * 0.85f);
                int bottom = (rows - row - 1) * texture.height / rows, top = (rows - row) * texture.height / rows;
                int minX = right, minY = top, maxX = left, maxY = bottom;
                for (int vertical = bottom; vertical < top; vertical++)
                for (int horizontal = left; horizontal < right; horizontal++)
                    if (pixels[vertical * texture.width + horizontal].a > 128) { minX = Mathf.Min(minX, horizontal); minY = Mathf.Min(minY, vertical); maxX = Mathf.Max(maxX, horizontal); maxY = Mathf.Max(maxY, vertical); }
                string name = id + "_" + index.ToString("D2");
                Vector2 pivot = Vector2.one * 0.5f;
                if (id == "ClockworkArsenal_v3") pivot = new[] { new Vector2(0.25f, 0.72f), new Vector2(0.22f, 0.28f), new Vector2(0.24f, 0.22f), new Vector2(0.19f, 0.24f), new Vector2(0.22f, 0.2f), new Vector2(0.16f, 0.24f) }[index];
                rectangles[index] = new SpriteRect { name = name, rect = new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1), alignment = SpriteAlignment.Custom, pivot = pivot, spriteID = previous.TryGetValue(name, out GUID identity) ? identity : GUID.Generate() };
            }
            provider.SetSpriteRects(rectangles); provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rectangles.Select(rectangle => new SpriteNameFileIdPair(rectangle.name, rectangle.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings { name = "Standalone", overridden = true, maxTextureSize = 4096, format = TextureImporterFormat.BC7 });
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings { name = "Android", overridden = true, maxTextureSize = 4096, format = TextureImporterFormat.ASTC_6x6 });
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings { name = "iPhone", overridden = true, maxTextureSize = 4096, format = TextureImporterFormat.ASTC_6x6 });
            importer.SaveAndReimport();
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(sprite => sprite.name).ToArray();
        }
    }
}
