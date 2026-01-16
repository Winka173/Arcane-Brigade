using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K13Builder
    {
        public static void Build()
        {
            Directory.CreateDirectory("PreviewExports/K13");
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            Sprite[] ranged = ImportArms("DreamArms_v2", new[] { new Vector2(0.19f, 0.42f), new Vector2(0.42f, 0.53f), new Vector2(0.28f, 0.27f), new Vector2(0.17f, 0.3f) });
            Sprite[] melee = ImportArms("ToyBlades_v2", new[] { new Vector2(0.17f, 0.48f), new Vector2(0.48f, 0.5f), new Vector2(0.15f, 0.5f), new Vector2(0.2f, 0.5f) });
            Sprite[] projectiles = K11ArtBuilder.Import("DreamProjectiles_v1", "K13");
            WeaponDefinition Add(string id, Sprite sprite, WeaponKind kind, WeaponMotion motion, float damage, float range, float cooldown)
            {
                string path = "Assets/ArcaneBrigade/Data/Run/" + id + ".asset";
                WeaponDefinition weapon = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(path);
                if (weapon == null) { weapon = ScriptableObject.CreateInstance<WeaponDefinition>(); AssetDatabase.CreateAsset(weapon, path); }
                weapon.Id = id; weapon.Sprite = sprite; weapon.Kind = kind; weapon.Motion = motion;
                weapon.Damage = damage; weapon.Range = range; weapon.Cooldown = cooldown;
                weapon.Windup = motion == WeaponMotion.Slam ? 0.24f : 0.16f; weapon.Recovery = 0.24f;
                weapon.VisualWidth = kind == WeaponKind.Melee ? 0.94f : 0.78f;
                weapon.HasMuzzleOffset = true;
                weapon.ProjectileMuzzleOffset = (Vector2.Scale(sprite.rect.size, new Vector2(0.91f, 0.55f)) - sprite.pivot) / sprite.pixelsPerUnit * (weapon.VisualWidth / sprite.bounds.size.x);
                weapon.MountDistance = 0.34f; weapon.MuzzleDistance = 0.6f;
                weapon.ConeDot = motion == WeaponMotion.Thrust ? 0.75f : -0.15f;
                weapon.SwingAngle = motion == WeaponMotion.Thrust ? 20 : motion == WeaponMotion.Slam ? 110 : 85;
                weapon.PushDistance = motion == WeaponMotion.Slam ? 0.5f : 0.22f;
                weapon.ProjectileSpeed = 5; weapon.ProjectileLifetime = 2;
                weapon.ProjectileCount = 1; weapon.ProjectilePierce = 0; weapon.ProjectileSplit = 0;
                weapon.ProjectileSpread = 0.14f; weapon.ProjectileRadius = 0.075f; weapon.ProjectileVisualWidth = 0.3f;
                weapon.ProjectileSprite = catalog.ProjectileDisc; weapon.ProjectileColor = Color.white;
                weapon.Element = DamageElement.Arcane; weapon.SpecialOnly = false; weapon.HomingOutbound = false; weapon.Family = ExpandedWeapon.None;
                if (!catalog.Weapons.Contains(weapon)) catalog.Weapons = catalog.Weapons.Concat(new[] { weapon }).ToArray();
                EditorUtility.SetDirty(weapon); return weapon;
            }
            WeaponDefinition orb = Add("DreamOrbStaff", ranged[0], WeaponKind.Projectile, WeaponMotion.Cast, 24, 7, 1.3f);
            orb.SpecialOnly = true; orb.ProjectileSpeed = 3.2f; orb.ProjectileLifetime = 3; orb.ProjectileRadius = 0.28f;
            orb.ProjectileVisualWidth = 0.98f; orb.ProjectilePierce = 3; orb.ProjectileSprite = projectiles[0];
            WeaponDefinition foam = Add("FireExtinguisher", ranged[1], WeaponKind.Projectile, WeaponMotion.Recoil, 3.2f, 3.4f, 0.72f);
            foam.ProjectileCount = 6; foam.ProjectileSpread = 0.13f; foam.ProjectileSpeed = 4.2f; foam.ProjectileLifetime = 0.8f;
            foam.ProjectileSprite = projectiles[2]; foam.Element = DamageElement.Frost;
            WeaponDefinition plasma = Add("PlasmaTrident", ranged[2], WeaponKind.Projectile, WeaponMotion.Recoil, 8, 7, 1.15f);
            plasma.ProjectileCount = 3; plasma.ProjectileSpread = 0.12f; plasma.ProjectilePierce = 1;
            plasma.ProjectileSprite = projectiles[1]; plasma.ProjectileSpeed = 6;
            WeaponDefinition fan = Add("MoonFeatherFan", ranged[3], WeaponKind.Projectile, WeaponMotion.Cast, 4.5f, 6.5f, 0.82f);
            fan.ProjectileCount = 4; fan.ProjectileSpread = 0.23f; fan.ProjectileSprite = projectiles[3]; fan.ProjectilePierce = 1;
            WeaponDefinition blades = Add("TwinSabres", melee[0], WeaponKind.Melee, WeaponMotion.Scissors, 18, 1.9f, 0.72f);
            blades.Family = ExpandedWeapon.Scissors;
            WeaponDefinition crescent = Add("ClockCrescent", melee[1], WeaponKind.Melee, WeaponMotion.Slash, 22, 4, 1.15f);
            crescent.Family = ExpandedWeapon.Fish; crescent.HomingOutbound = true; crescent.SpecialOnly = true;
            crescent.ProjectileSprite = melee[1]; crescent.ProjectileSpeed = 5; crescent.ProjectileLifetime = 2.4f;
            crescent.ProjectileRadius = 0.2f; crescent.ProjectileVisualWidth = 0.8f;
            Add("ParasolNeedle", melee[2], WeaponKind.Melee, WeaponMotion.Thrust, 18, 2.8f, 0.9f);
            WeaponDefinition stamp = Add("StampHammer", melee[3], WeaponKind.Melee, WeaponMotion.Slam, 26, 1.85f, 1.2f);
            stamp.Family = ExpandedWeapon.Hammer;
            foreach (UnitDefinition hero in catalog.Champions)
            {
                hero.Speed = hero.Id == "Postmaster" ? 3.65f : hero.Id == "DrowsySeer" ? 3.1f : 3.3f;
                hero.Weapon = catalog.Weapon(hero.Id == "Postmaster" ? "MailboxLauncher" : hero.Id == "DrowsySeer" ? "BellFork" : "KeyCleaver");
                if (hero.Weapon == null) throw new System.InvalidOperationException("Missing fixed hero weapon " + hero.Id);
                if (hero.Id == "DrowsySeer")
                {
                    hero.Health = 150; hero.PassiveStrength = 3;
                    hero.ChampionModifiers = hero.ChampionModifiers.Where(modifier => modifier.Effect != ItemEffect.Projectiles).Concat(new[] { new StatModifier { Effect = ItemEffect.Projectiles, Value = 1, Scope = ModifierScope.Champion } }).ToArray();
                }
                EditorUtility.SetDirty(hero);
            }
            EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets();
            BuildMap(catalog);
            VerifyAndBuild();
        }
        private static Sprite[] ImportArms(string name, Vector2[] pivots)
        {
            K11ArtBuilder.Import(name, "K13");
            string path = "Assets/ArcaneBrigade/Art/K13/" + name + ".png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            var factory = new SpriteDataProviderFactories(); factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var rectangles = provider.GetSpriteRects().OrderBy(rectangle => rectangle.name).ToArray();
            for (int index = 0; index < rectangles.Length; index++) { rectangles[index].alignment = SpriteAlignment.Custom; rectangles[index].pivot = pivots[index]; }
            provider.SetSpriteRects(rectangles); provider.Apply(); importer.SaveAndReimport();
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(sprite => sprite.name).ToArray();
        }
        private static void BuildMap(RunCatalog catalog)
        {
            Texture2D ImportGround(string name, bool alpha)
            {
                string path = "Assets/ArcaneBrigade/Art/K13/" + name + ".png";
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Default; importer.maxTextureSize = 4096;
                importer.mipmapEnabled = true; importer.filterMode = FilterMode.Trilinear; importer.anisoLevel = 4;
                importer.wrapMode = alpha ? TextureWrapMode.Clamp : TextureWrapMode.Mirror;
                importer.alphaIsTransparency = alpha; importer.npotScale = TextureImporterNPOTScale.None;
                importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings { name = "Standalone", overridden = true, maxTextureSize = 4096, format = TextureImporterFormat.BC7, compressionQuality = 100 });
                importer.SaveAndReimport(); return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            Material floor = AssetDatabase.LoadAssetAtPath<Material>("Assets/ArcaneBrigade/Data/QualityHD/FloorHD.mat");
            floor.mainTexture = ImportGround("CarnivalStone_v1", false);
            floor.SetTexture("_PatinaTex", ImportGround("CarnivalPatina_v1", true));
            floor.SetVector("_TileScale", new Vector4(catalog.WorldSize.x / 8, catalog.WorldSize.y / 5, 0, 0));
            floor.SetFloat("_PatinaStrength", 0.26f); floor.SetFloat("_MicroStrength", 0.08f); floor.SetFloat("_Detail", 0.58f);
            EditorUtility.SetDirty(floor); AssetDatabase.SaveAssets();
        }
        public static void VerifyAndBuild()
        {
            Directory.CreateDirectory("PreviewExports/K13");
            K12Checks.Run(); K12PolishChecks.Run(); MarketPolishChecks.Run(); K11Checks.Run(); K13Checks.Run();
            FullRunBuilder.BuildWindows();
            File.Copy("PreviewExports/FullRun_Build.txt", "PreviewExports/K13/Windows_Build.txt", true);
            Debug.Log("K13_CANDIDATE_BUILT");
        }
        public static void Balance()
        {
            K13BalanceDiagnostics.Run();
        }
    }
}
