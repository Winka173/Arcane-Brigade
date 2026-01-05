using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class PresentationBuilder
    {
        private const string Root = "Assets/ArcaneBrigade/";
        private const string CatalogPath = Root + "Resources/RunCatalog.asset";

        [MenuItem("Arcane Brigade/Presentation/Apply presentation and check")]
        public static void ApplyAndCheck()
        {
            Directory.CreateDirectory(Root + "Resources/Audio");
            Directory.CreateDirectory("PreviewExports");
            AssetDatabase.Refresh();
            Dictionary<string, Sprite> sprites = FullRunBuilder.ImportGrid(Root + "Art/Presentation/BowMagic_v2.png", new[] { "CrescentBow", "RuneStaff", "ArrowBolt", "RuneOrb" }, false, 2, 0.4f);
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>(CatalogPath);
            if (catalog == null) throw new InvalidOperationException("Full Run catalog required");
            catalog.ShopSlots = 3;
            catalog.ProjectileDisc = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "Art/Checkpoint/Disc.png");
            catalog.SpecialShopEvery = 2;
            catalog.SpecialShopPrice = 2;
            var weapons = catalog.Weapons.ToList();
            WeaponDefinition bow = Weapon("CrescentBow", sprites["CrescentBow"], sprites["ArrowBolt"], WeaponMotion.Bow, DamageElement.Physical, 14f, 0.95f, 0.25f, 0.5f);
            WeaponDefinition staff = Weapon("RuneStaff", sprites["RuneStaff"], sprites["RuneOrb"], WeaponMotion.Cast, DamageElement.Arcane, 11f, 1.05f, 0.32f, 0.3f);
            foreach (WeaponDefinition weapon in new[] { bow, staff }) if (!weapons.Any(existing => existing.Id == weapon.Id)) weapons.Add(weapon);
            catalog.Weapons = weapons.ToArray();
            foreach (WeaponDefinition weapon in catalog.Weapons)
            {
                if (weapon.Element == DamageElement.Arcane && weapon.Kind == WeaponKind.Projectile) { weapon.ProjectileSprite = sprites["RuneOrb"]; weapon.ProjectileVisualWidth = 0.3f; weapon.ProjectileColor = new Color(0.85f, 0.35f, 1f); }
                EditorUtility.SetDirty(weapon);
            }
            catalog.Unit("CandleSnail").Weapon = bow;
            catalog.Unit("HoodWisp").Weapon = staff;
            EditorUtility.SetDirty(catalog.Unit("CandleSnail"));
            EditorUtility.SetDirty(catalog.Unit("HoodWisp"));
            foreach (UnitDefinition enemy in catalog.Enemies) { enemy.Weapon.ProjectileColor = new Color(1f, 0.24f, 0.4f); EditorUtility.SetDirty(enemy.Weapon); }
            EditorUtility.SetDirty(catalog);
            string flashPath = Root + "Resources/UnitFlash.mat";
            Material flash = AssetDatabase.LoadAssetAtPath<Material>(flashPath);
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(Root + "Art/Presentation/UnitFlash.shader");
            if (flash == null) { flash = new Material(shader); AssetDatabase.CreateAsset(flash, flashPath); }
            else flash.shader = shader;
            EditorUtility.SetDirty(flash);
            WriteWav(Root + "Resources/Audio/CombatStudy.wav", ArcadeSound.MusicStudy(false));
            WriteWav(Root + "Resources/Audio/ShopStudy.wav", ArcadeSound.MusicStudy(true));
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
            ApplyMap();
            FullRunChecks.Run(AssetDatabase.LoadAssetAtPath<RunCatalog>(CatalogPath));
            PresentationChecks();
            FullRunBuilder.CaptureGallery();
            Debug.Log("ARCANE_PRESENTATION_READY");
        }

        private static WeaponDefinition Weapon(string id, Sprite body, Sprite projectile, WeaponMotion motion, DamageElement element, float damage, float cooldown, float windup, float projectileWidth)
        {
            string path = Root + "Data/Run/" + id + ".asset";
            WeaponDefinition weapon = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(path);
            if (weapon == null) { weapon = ScriptableObject.CreateInstance<WeaponDefinition>(); AssetDatabase.CreateAsset(weapon, path); }
            weapon.Id = id; weapon.Sprite = body; weapon.ProjectileSprite = projectile;
            weapon.Kind = WeaponKind.Projectile; weapon.Motion = motion; weapon.Element = element;
            weapon.Damage = damage; weapon.Cooldown = cooldown; weapon.Windup = windup; weapon.Recovery = 0.24f;
            weapon.Range = 8f; weapon.ProjectileSpeed = motion == WeaponMotion.Bow ? 10f : 6.5f;
            weapon.ProjectileLifetime = 3f; weapon.VisualWidth = motion == WeaponMotion.Bow ? 0.65f : 0.82f;
            weapon.BodyAttackTilt = motion == WeaponMotion.Bow ? 12f : 18f;
            weapon.ProjectileVisualWidth = projectileWidth;
            weapon.ProjectileColor = motion == WeaponMotion.Bow ? Color.white : new Color(0.87f, 0.4f, 1f);
            weapon.HasMuzzleOffset = true;
            Vector2 tip = new Vector2(motion == WeaponMotion.Bow ? 0.98f : 0.88f, 0.5f);
            weapon.ProjectileMuzzleOffset = (Vector2.Scale(body.rect.size, tip) - body.pivot) / body.pixelsPerUnit * (weapon.VisualWidth / body.bounds.size.x);
            EditorUtility.SetDirty(weapon);
            return weapon;
        }

        private static void ApplyMap()
        {
            string path = Root + "Art/Presentation/NightmareFloor_v1.png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            EditorSceneManager.OpenScene(Root + "Scenes/RunArena.unity");
            foreach (SpriteRenderer renderer in UnityEngine.Object.FindObjectsByType<SpriteRenderer>())
                if (renderer.sortingOrder < 0 && renderer.name != "NightmareBackdrop") renderer.gameObject.SetActive(false);
            GameObject existing = GameObject.Find("NightmareBackdrop");
            if (existing != null) UnityEngine.Object.DestroyImmediate(existing);
            SpriteRenderer floor = new GameObject("NightmareBackdrop").AddComponent<SpriteRenderer>();
            floor.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            Vector2 size = AssetDatabase.LoadAssetAtPath<RunCatalog>(CatalogPath).Arena.ArenaSize + Vector2.one * 0.65f;
            floor.transform.localScale = new Vector3(size.x / floor.sprite.bounds.size.x, size.y / floor.sprite.bounds.size.y, 1f);
            floor.sortingOrder = -90;
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }

        private static void WriteWav(string path, float[] samples)
        {
            using (var writer = new BinaryWriter(File.Create(path)))
            {
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + samples.Length * 2);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16);
                writer.Write((short)1); writer.Write((short)1); writer.Write(ArcadeSound.SampleRate);
                writer.Write(ArcadeSound.SampleRate * 2); writer.Write((short)2); writer.Write((short)16);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(samples.Length * 2);
                foreach (float sample in samples) writer.Write((short)Mathf.RoundToInt(Mathf.Clamp(sample, -1f, 1f) * 32767));
            }
        }

        private static void PresentationChecks()
        {
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>(CatalogPath);
            var results = new List<string>();
            foreach (string id in new[] { "CrescentBow", "RuneStaff" })
            {
                WeaponDefinition weapon = catalog.Weapon(id);
                Require(weapon != null && weapon.ProjectileSprite != null && weapon.HasMuzzleOffset, id + " dedicated projectile and muzzle", results);
            }
            Require(Resources.Load<Material>("UnitFlash") != null && Resources.Load<Material>("UnitFlash").shader.isSupported, "White flash shader supported", results);
            Require(Resources.Load<AudioClip>("Audio/CombatStudy") != null && Resources.Load<AudioClip>("Audio/ShopStudy") != null, "Original music studies imported", results);
            foreach (bool shop in new[] { false, true })
            {
                float[] samples = ArcadeSound.MusicStudy(shop);
                Require(samples.All(value => float.IsFinite(value) && Mathf.Abs(value) <= 0.85f) && samples.Any(value => Mathf.Abs(value) > 0.1f), "Music study finite non-silent " + shop, results);
            }
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 131026);
                CheckpointCatalog arena = session.PrepareCombat();
                arena.EnemyBudget = 0;
                arena.Champion.CriticalChance = 1f;
                var simulation = new CombatSimulation(arena);
                simulation.Start(arena.Seed);
                simulation.Units[1].NextAttack = float.PositiveInfinity;
                simulation.AddFixture(arena.MeleeEnemy, Faction.Enemy, new Vector2(1.2f, 0));
                bool critical = false;
                bool sword = false;
                for (int tick = 0; tick < 30; tick++)
                {
                    simulation.Step(1f / 60f, Vector2.zero);
                    foreach (CombatEvent value in simulation.Events)
                    {
                        if (value.Kind == CombatEventKind.Hit && value.Faction == Faction.Enemy && value.Critical) critical = true;
                        if (value.Kind == CombatEventKind.Fire && value.Weapon != null && value.Direction.sqrMagnitude > 0.9f) sword = true;
                    }
                }
                Require(critical && sword, "Critical feedback and attack-direction events originate in simulation", results);
            }
            File.WriteAllLines("PreviewExports/Presentation_Checks.txt", results);
            Debug.Log("ARCANE_PRESENTATION_CHECKS_PASSED " + results.Count);
        }

        private static void Require(bool condition, string description, List<string> results)
        {
            if (!condition) throw new InvalidOperationException(description);
            results.Add("PASS " + description);
        }
    }
}
