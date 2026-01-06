using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class FullRunBuilder
    {
        private const string Root = "Assets/ArcaneBrigade/";
        private const string DataRoot = Root + "Data/Run/";
        private const string CatalogPath = Root + "Resources/RunCatalog.asset";
        private static readonly string[] ScenePaths = { Root + "Scenes/RunBoot.unity", Root + "Scenes/RunFrontend.unity", Root + "Scenes/RunArena.unity" };

        [MenuItem("Arcane Brigade/Full Run/Build content, scenes and checks")]
        public static void BuildAndCheck()
        {
            Directory.CreateDirectory(DataRoot);
            Directory.CreateDirectory(Root + "Resources");
            Directory.CreateDirectory("PreviewExports");
            AssetDatabase.Refresh();
            Dictionary<string, Sprite> bodies = ImportGrid(Root + "Art/Run/Characters_v1.png", new[] { "LanternKnight", "TeapotPilgrim", "CandleSnail", "MailboxImp", "PillowOracle", "MushroomLibrarian" }, true);
            Dictionary<string, Sprite> arms = ImportGrid(Root + "Art/Run/Weapons_v1.png", new[] { "KeyCleaver", "TeapotPopgun", "UmbrellaLance", "MailboxLauncher", "BellFork", "BookPaddle" }, false);
            RunCatalog catalog = Asset<RunCatalog>(CatalogPath);
            CheckpointCatalog baseArena = AssetDatabase.LoadAssetAtPath<CheckpointCatalog>(Root + "Data/Checkpoint/CheckpointCatalog.asset");
            catalog.Arena = baseArena;
            catalog.TierDamageMultiplier = 3.6f;
            catalog.TierSupportMultiplier = 3f;
            WeaponDefinition[] weapons = new WeaponDefinition[6];
            string[] weaponNames = { "KeyCleaver", "TeapotPopgun", "UmbrellaLance", "MailboxLauncher", "BellFork", "BookPaddle" };
            WeaponKind[] kinds = { WeaponKind.Melee, WeaponKind.Projectile, WeaponKind.Melee, WeaponKind.Projectile, WeaponKind.Projectile, WeaponKind.Melee };
            WeaponMotion[] motions = { WeaponMotion.Slam, WeaponMotion.Recoil, WeaponMotion.Thrust, WeaponMotion.Recoil, WeaponMotion.Bell, WeaponMotion.Slash };
            DamageElement[] elements = { DamageElement.Physical, DamageElement.Ember, DamageElement.Frost, DamageElement.Physical, DamageElement.Arcane, DamageElement.Physical };
            for (int index = 0; index < 6; index++)
            {
                WeaponDefinition weapon = Asset<WeaponDefinition>(DataRoot + weaponNames[index] + ".asset");
                weapon.Id = weaponNames[index];
                weapon.Sprite = arms[weapon.Id];
                weapon.Kind = kinds[index];
                weapon.Motion = motions[index];
                weapon.BodyAttackTilt = motions[index] == WeaponMotion.Slam ? 35f : motions[index] == WeaponMotion.Slash ? 22f : motions[index] == WeaponMotion.Thrust ? 10f : 8f;
                weapon.Element = elements[index];
                weapon.Damage = index == 0 ? 15f : index == 4 ? 11f : 10f;
                weapon.Cooldown = index == 5 ? 0.5f : index == 4 ? 1.15f : 0.85f;
                weapon.Range = weapon.Kind == WeaponKind.Melee ? index == 2 ? 2.4f : 1.9f : 7.8f;
                weapon.VisualWidth = index == 2 ? 0.9f : 0.78f;
                weapon.SwingAngle = index == 0 ? 85f : 65f;
                weapon.MuzzleDistance = 0.65f;
                Vector2 tip = index == 1 ? new Vector2(0.97f, 0.55f) : index == 3 ? new Vector2(0.99f, 0.49f) : index == 4 ? new Vector2(0.82f, 0.46f) : new Vector2(0.97f, 0.5f);
                Vector2 tipPixels = Vector2.Scale(weapon.Sprite.rect.size, tip);
                weapon.ProjectileMuzzleOffset = (tipPixels - weapon.Sprite.pivot) / weapon.Sprite.pixelsPerUnit * (weapon.VisualWidth / weapon.Sprite.bounds.size.x);
                weapon.HasMuzzleOffset = true;
                weapon.ProjectileLifetime = 4f;
                weapon.ProjectileColor = index == 1 ? new Color(1f, 0.7f, 0.4f) : Color.white;
                weapons[index] = weapon;
                EditorUtility.SetDirty(weapon);
            }
            catalog.Weapons = weapons;
            string[] bodyNames = { "LanternKnight", "TeapotPilgrim", "CandleSnail", "MailboxImp", "PillowOracle", "MushroomLibrarian" };
            var units = new UnitDefinition[6];
            PassiveKind[] passives = { PassiveKind.Rally, PassiveKind.HealChampion, PassiveKind.ChillAura, PassiveKind.Scavenge, PassiveKind.Rally, PassiveKind.HealChampion };
            string[][] tags = { new[] { "clock", "hearth" }, new[] { "hearth", "archive" }, new[] { "frost", "hearth" }, new[] { "clock", "archive" }, new[] { "archive", "frost" }, new[] { "archive", "hearth" } };
            string[] descriptions = { "Lantern rally: damage grows with brigade size.", "Quiet tea heals the champion automatically.", "A sleepy aura slows nearby nightmares.", "Finds spare coins during combat.", "Reads the crowd and scales with brigade size.", "Warm pages restore the champion's health." };
            for (int index = 0; index < 6; index++)
            {
                units[index] = Unit(bodyNames[index], bodies[bodyNames[index]], weapons[index], 120f, 5.1f, 0.9f);
                units[index].Tags = tags[index];
                units[index].Passive = passives[index];
                units[index].PassiveStrength = index == 1 || index == 5 ? 0.5f : 0.8f;
                units[index].Description = descriptions[index];
                units[index].Price = 20 + index * 3;
            }
            units[0].Speed = 4.8f;
            UnitDefinition mailboxChampion = Unit("Postmaster", bodies["MailboxImp"], weapons[3], 110f, 5f, 0.92f);
            mailboxChampion.Passive = PassiveKind.Scavenge;
            mailboxChampion.PassiveStrength = 2f;
            mailboxChampion.Description = "Automatic parcel fire; earns spare coins in combat.";
            mailboxChampion.UnlockWave = 5;
            UnitDefinition pillowChampion = Unit("DrowsySeer", bodies["PillowOracle"], weapons[4], 130f, 4.5f, 0.95f);
            pillowChampion.Passive = PassiveKind.HealChampion;
            pillowChampion.Description = "Bell magic and gentle self-healing. No active skills.";
            pillowChampion.UnlockWave = 10;
            catalog.Champions = new[] { units[0], mailboxChampion, pillowChampion };
            UnitDefinition wisp = Unit("HoodWisp", baseArena.Companion.Body, weapons[4], 1f, 5.2f, 0.85f);
            wisp.Tags = new[] { "archive", "frost" };
            wisp.Description = "A hood with a bell and an uncertain opinion.";
            UnitDefinition jelly = Unit("JellySwordsman", baseArena.MeleeEnemy.Body, weapons[0], 1f, 5.1f, 0.75f);
            jelly.Tags = new[] { "hearth", "clock" };
            jelly.Description = "Grumpy but loyal; weapons are rolled on recruitment.";
            catalog.Companions = new[] { units[1], units[2], units[3], units[4], units[5], wisp, jelly };
            catalog.Bonds = new[]
            {
                new BondDefinition { Id = "hearth", Name = "Hearth", Effect = ItemEffect.Heal, Value = 0.15f },
                new BondDefinition { Id = "archive", Name = "Archive", Effect = ItemEffect.Damage, Value = 0.08f },
                new BondDefinition { Id = "clock", Name = "Clockwork", Effect = ItemEffect.AttackSpeed, Value = 0.07f },
                new BondDefinition { Id = "frost", Name = "Frost", Effect = ItemEffect.Roam, Value = 0.15f }
            };
            catalog.Items = new[]
            {
                Item("key-oil", "Key Oil", "+12% damage", 18, ItemEffect.Damage, 0.12f, weapons[0].Sprite),
                Item("clock-spring", "Clock Spring", "+10% attack speed", 20, ItemEffect.AttackSpeed, 0.1f, weapons[3].Sprite),
                Item("honey-pot", "Honey Pot", "+20 max health and current health", 22, ItemEffect.Health, 20f, units[1].Body),
                Item("felt-boots", "Felt Boots", "+6% movement speed", 18, ItemEffect.MoveSpeed, 0.06f, units[2].Body),
                Item("coin-compass", "Coin Compass", "+0.25 pickup radius", 15, ItemEffect.Pickup, 0.25f, baseArena.Coin),
                Item("wandering-map", "Wandering Map", "+0.4 companion leash radius", 18, ItemEffect.Roam, 0.4f, weapons[5].Sprite),
                Item("guardian-canopy", "Guardian Canopy", "Allies block enemy bullets; stacks do not add extra blocks", 45, ItemEffect.Interceptor, 1f, weapons[2].Sprite, 4),
                Item("warm-tea", "Warm Tea", "+0.15 health per second", 24, ItemEffect.Heal, 0.15f, units[1].Body),
                Item("oracle-lens", "Oracle Lens", "+4% critical chance; effective chance capped at 100%", 22, ItemEffect.Crit, 0.04f, units[4].Body),
                Item("twin-seal", "Twin Seal", "+1 projectile per ranged attack", 35, ItemEffect.Projectiles, 1f, weapons[4].Sprite, 5),
                Item("piggy-mail", "Piggy Mail", "+2 maximum interest; one interest per ten gold", 18, ItemEffect.Interest, 2f, units[3].Body),
                Item("pillow-mail", "Pillow Mail", "+0.12 armor; received damage divided by 1 + armor", 20, ItemEffect.Armor, 0.12f, units[4].Body),
                Item("royal-key", "Royal Key", "+22% champion damage only", 28, ItemEffect.ChampionDamage, 0.22f, weapons[0].Sprite),
                Item("brigade-banner", "Brigade Banner", "+15% companion damage only", 30, ItemEffect.CompanionDamage, 0.15f, units[3].Body),
                Item("marching-bell", "Marching Bell", "+12% companion attack speed only", 26, ItemEffect.CompanionAttackSpeed, 0.12f, weapons[4].Sprite),
                Item("coupon-book", "Coupon Book", "Future shop rolls cost 5% less; total discount capped at 50%", 24, ItemEffect.Discount, 0.05f, weapons[5].Sprite),
                Item("long-atlas", "Long Atlas", "+1.0 ally leash radius", 36, ItemEffect.Roam, 1f, weapons[5].Sprite, 8),
                Item("starlight-quilt", "Starlight Quilt", "+45 max and current health", 42, ItemEffect.Health, 45f, units[4].Body, 8),
                Item("echo-postage", "Echo Postage", "+2 projectiles per ranged attack", 60, ItemEffect.Projectiles, 2f, weapons[3].Sprite, 10),
                Item("tarnished-crown", "Tarnished Crown", "+40% champion damage only", 48, ItemEffect.ChampionDamage, 0.4f, units[0].Body, 10)
            };
            catalog.Recipes = new[]
            {
                new AltarRecipe { Id = "dream-pillow", FirstUnit = "HoodWisp", SecondUnit = "JellySwordsman", ResultUnit = "PillowOracle" },
                new AltarRecipe { Id = "warm-library", FirstUnit = "TeapotPilgrim", SecondUnit = "CandleSnail", ResultUnit = "MushroomLibrarian" }
            };
            catalog.WaveModifiers = new[]
            {
                new WaveModifier { Name = "Clockstorm: denser waves", Every = 7, SpawnMultiplier = 0.75f, BudgetMultiplier = 1.3f },
                new WaveModifier { Name = "Lost treasury: richer drops", Every = 5, GoldPerKill = 3 },
                new WaveModifier { Name = "Quiet stacks: gentle regeneration", Every = 11, Healing = 0.25f }
            };
            Sprite[] enemyBodies = { baseArena.MeleeEnemy.Body, baseArena.Companion.Body, baseArena.MeleeEnemy.Body, baseArena.Companion.Body, units[4].Body, units[1].Body };
            string[] enemyNames = { "HushMite", "ClockBat", "BristleImp", "RuneWisp", "InkMoth", "SirenSeal" };
            catalog.Enemies = new UnitDefinition[6];
            for (int index = 0; index < 6; index++)
            {
                WeaponDefinition attack = Asset<WeaponDefinition>(DataRoot + enemyNames[index] + "Attack.asset");
                attack.Id = enemyNames[index] + "Attack";
                attack.Kind = index % 2 == 0 ? WeaponKind.Contact : WeaponKind.Projectile;
                attack.Sprite = attack.Kind == WeaponKind.Contact ? null : weapons[index % 6].Sprite;
                attack.Damage = 4f;
                attack.Cooldown = 2.3f;
                attack.Range = 8f;
                attack.Windup = 0.4f;
                attack.ProjectileSpeed = 4.8f;
                attack.ProjectileLifetime = 6f;
                attack.ProjectileColor = new Color(1f, 0.38f, 0.32f);
                catalog.Enemies[index] = Unit(enemyNames[index], enemyBodies[index], attack, 28f + index * 7f, 1.35f + index * 0.1f, 0.65f);
                catalog.Enemies[index].Tint = index % 2 == 0 ? new Color(0.8f, 0.6f, 0.65f) : new Color(0.75f, 0.55f, 0.7f);
                EditorUtility.SetDirty(attack);
            }
            catalog.Bosses = new UnitDefinition[5];
            string[] bossNames = { "UndeliveredKing", "ScarletIndex", "SleepCollector", "WaxAdmiral", "LastLantern" };
            int[] bossBodies = { 3, 5, 4, 2, 0 };
            for (int index = 0; index < 5; index++)
            {
                WeaponDefinition attack = Asset<WeaponDefinition>(DataRoot + bossNames[index] + "Attack.asset");
                attack.Id = bossNames[index] + "Attack";
                attack.Kind = WeaponKind.Contact;
                attack.Damage = 12f;
                catalog.Bosses[index] = Unit(bossNames[index], units[bossBodies[index]].Body, attack, 650f + index * 140f, 0.65f, 2.2f + index * 0.08f);
                catalog.Bosses[index].Radius = 0.8f;
                catalog.Bosses[index].IsBoss = true;
                catalog.Bosses[index].BossPattern = index;
                catalog.Bosses[index].ContactDamage = 8f;
                EditorUtility.SetDirty(attack);
            }
            EditorUtility.SetDirty(mailboxChampion);
            EditorUtility.SetDirty(pillowChampion);
            EditorUtility.SetDirty(wisp);
            EditorUtility.SetDirty(jelly);
            foreach (UnitDefinition unit in units.Concat(catalog.Enemies).Concat(catalog.Bosses)) EditorUtility.SetDirty(unit);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            FullRunChecks.Run(catalog);
            BuildScenes();
            CaptureWorld();
            CaptureGallery();
            Debug.Log("ARCANE_FULLRUN_CONTENT_COMPLETE");
        }

        private static T Asset<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) { asset = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(asset, path); }
            return asset;
        }

        private static UnitDefinition Unit(string id, Sprite sprite, WeaponDefinition weapon, float health, float speed, float height)
        {
            UnitDefinition unit = Asset<UnitDefinition>(DataRoot + id + ".asset");
            unit.Id = unit.DisplayName = id;
            unit.Body = sprite;
            unit.Weapon = weapon;
            unit.Health = health;
            unit.Speed = speed;
            unit.BodyHeight = height;
            unit.ContactDamage = 4f;
            return unit;
        }

        private static ItemDefinition Item(string id, string name, string description, int price, ItemEffect effect, float value, Sprite sprite, int unlock = 0)
            => new ItemDefinition { Id = id, Name = name, Description = description, Price = price, Effect = effect, Value = value, Icon = sprite, UnlockWave = unlock };

        public static Dictionary<string, Sprite> ImportGrid(string path, string[] names, bool body, int columns = 3, float lowerRowFraction = 0.5f)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 256f;
            importer.isReadable = true;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            Color32[] pixels = texture.GetPixels32();
            var factory = new SpriteDataProviderFactories();
            factory.Init();
            ISpriteEditorDataProvider provider = factory.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            Dictionary<string, GUID> ids = provider.GetSpriteRects().ToDictionary(rect => rect.name, rect => rect.spriteID);
            var rects = new List<SpriteRect>();
            for (int index = 0; index < names.Length; index++)
            {
                int minimumX = index % columns * texture.width / columns, maximumX = (index % columns + 1) * texture.width / columns;
                int split = Mathf.RoundToInt(texture.height * lowerRowFraction);
                int minimumY = index / columns == 0 ? split : 0, maximumY = index / columns == 0 ? texture.height : split;
                int left = maximumX, right = minimumX, bottom = maximumY, top = minimumY;
                for (int vertical = minimumY; vertical < maximumY; vertical++)
                    for (int horizontal = minimumX; horizontal < maximumX; horizontal++)
                    {
                        if (pixels[vertical * texture.width + horizontal].a < 64) continue;
                        left = Mathf.Min(left, horizontal); right = Mathf.Max(right, horizontal);
                        bottom = Mathf.Min(bottom, vertical); top = Mathf.Max(top, vertical);
                    }
                if (right < left) throw new InvalidOperationException("Empty generated sprite: " + names[index]);
                rects.Add(new SpriteRect { name = names[index], spriteID = ids.TryGetValue(names[index], out GUID id) ? id : GUID.Generate(), rect = new Rect(left, bottom, right - left + 1, top - bottom + 1), alignment = SpriteAlignment.Custom, pivot = body ? new Vector2(0.5f, 0f) : WeaponPivot(names[index]) });
            }
            provider.SetSpriteRects(rects.ToArray());
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(rect => new SpriteNameFileIdPair(rect.name, rect.spriteID)));
            provider.Apply();
            importer.SaveAndReimport();
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToDictionary(sprite => sprite.name);
        }

        private static Vector2 WeaponPivot(string id)
        {
            if (id == "KeyCleaver") return new Vector2(0.23f, 0.5f);
            if (id == "TeapotPopgun") return new Vector2(0.22f, 0.13f);
            if (id == "MailboxLauncher") return new Vector2(0.12f, 0.18f);
            if (id == "BookPaddle") return new Vector2(0.22f, 0.5f);
            if (id == "CrescentBow") return new Vector2(0.57f, 0.5f);
            if (id == "RuneStaff") return new Vector2(0.20f, 0.5f);
            if (id == "ArrowBolt" || id == "RuneOrb") return new Vector2(0.5f, 0.5f);
            return new Vector2(0.14f, 0.5f);
        }

        private static void BuildScenes()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("RunBootLoader").AddComponent<RunBootLoader>();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePaths[0]);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Camera camera = new GameObject("MenuCamera").AddComponent<Camera>();
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.orthographic = true;
            camera.backgroundColor = new Color(0.055f, 0.095f, 0.12f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            new GameObject("RunMenu").AddComponent<RunMenu>().Configure(AssetDatabase.LoadAssetAtPath<RunCatalog>(CatalogPath));
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePaths[1]);
            EditorSceneManager.OpenScene(Root + "Scenes/Arena.unity");
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>(CatalogPath);
            UnityEngine.Object.FindAnyObjectByType<ArenaController>().ConfigureRun(catalog, UnityEngine.Object.FindAnyObjectByType<Camera>());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePaths[2]);
            EditorBuildSettings.scenes = ScenePaths.Select(path => new EditorBuildSettingsScene(path, true)).ToArray();
            AssetDatabase.SaveAssets();
        }

        private static void CaptureWorld()
        {
            ArenaController controller = UnityEngine.Object.FindAnyObjectByType<ArenaController>();
            controller.CaptureRunFixture(AssetDatabase.LoadAssetAtPath<RunCatalog>(CatalogPath), 131026, 12f);
            Camera camera = UnityEngine.Object.FindAnyObjectByType<Camera>();
            Capture(camera, "PreviewExports/Unity_FullRun_World.png");
            EditorSceneManager.OpenScene(ScenePaths[2]);
        }

        [MenuItem("Arcane Brigade/Full Run/Capture model gallery without resetting")]
        public static void CaptureGallery()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>(CatalogPath);
            Camera camera = new GameObject("GalleryCamera").AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.4f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.transform.position = new Vector3(0, 0, -10);
            camera.backgroundColor = new Color(0.055f, 0.095f, 0.12f);
            string[] ids = { "LanternKnight", "TeapotPilgrim", "CandleSnail", "MailboxImp", "PillowOracle", "MushroomLibrarian" };
            Label("ARCANE BRIGADE / ORIGINAL MODULAR CAST", new Vector2(0, 4.45f), 0.33f);
            Label("No arms. Hands stay attached to weapons. " + catalog.Weapons.Length + " interchangeable prop weapons.", new Vector2(0, 3.85f), 0.17f);
            for (int index = 0; index < ids.Length; index++)
            {
                UnitDefinition definition = catalog.Unit(ids[index]);
                Vector2 position = new Vector2((index % 3 - 1) * 5.2f - 0.4f, index < 3 ? 1.75f : -1.5f);
                UnitVisual visual = UnityEngine.Object.Instantiate(catalog.Arena.UnitPrefab).GetComponent<UnitVisual>();
                var state = new UnitState { Id = index + 1, Definition = definition, Faction = index == 0 ? Faction.Champion : Faction.Companion, Position = position, PreviousPosition = position, Facing = Vector2.right, Health = 100 };
                visual.Bind(state, catalog.Arena);
                visual.transform.localScale = Vector3.one * 1.7f;
                Label(definition.DisplayName, position + new Vector2(0.4f, -1.35f), 0.22f);
                Label(definition.Weapon.Id + " / " + definition.Weapon.Motion, position + new Vector2(0.4f, -1.75f), 0.15f);
            }
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), Root + "Scenes/RunModelGallery.unity");
            Capture(camera, "PreviewExports/Unity_FullRun_Model_Gallery.png");
            EditorSceneManager.OpenScene(ScenePaths[2]);
        }

        private static void Label(string value, Vector2 position, float size)
        {
            var label = new GameObject("GalleryLabel").AddComponent<TextMesh>();
            label.text = value;
            label.fontSize = 48;
            label.characterSize = size * 0.26f;
            label.anchor = TextAnchor.MiddleCenter;
            label.color = new Color(0.92f, 0.88f, 0.73f);
            label.transform.position = new Vector3(position.x, position.y, -1);
        }

        private static void Capture(Camera camera, string path)
        {
            var target = new RenderTexture(1920, 1080, 24);
            RenderTexture previous = RenderTexture.active;
            camera.targetTexture = target;
            camera.rect = new Rect(0, 0, 1, 1);
            camera.aspect = 16f / 9f;
            camera.Render();
            RenderTexture.active = target;
            var image = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
            image.Apply();
            File.WriteAllBytes(path, image.EncodeToPNG());
            RenderTexture.active = previous;
            camera.targetTexture = null;
            target.Release();
            UnityEngine.Object.DestroyImmediate(image);
            UnityEngine.Object.DestroyImmediate(target);
        }

        [MenuItem("Arcane Brigade/Full Run/Build Windows")]
        public static void BuildWindows()
        {
            Directory.CreateDirectory("Builds/FullRun");
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = ScenePaths, locationPathName = "Builds/FullRun/ArcaneBrigade.exe", target = BuildTarget.StandaloneWindows64, options = BuildOptions.None });
            File.WriteAllText("PreviewExports/FullRun_Build.txt", "Result: " + report.summary.result + "\nErrors: " + report.summary.totalErrors + "\nWarnings: " + report.summary.totalWarnings + "\nSize: " + report.summary.totalSize + "\n");
            Directory.CreateDirectory("PreviewExports/K7");
            File.WriteAllLines("PreviewExports/K7/Build_Messages.txt", report.steps.SelectMany(step => step.messages).Where(message => message.type == LogType.Warning || message.type == LogType.Error).Select(message => message.type + ": " + message.content));
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Full run build failed");
            Directory.CreateDirectory("Builds/FullRun/ThirdPartyNotices");
            File.Copy(Root + "Resources/Fonts/OFL.txt", "Builds/FullRun/ThirdPartyNotices/Noto-OFL.txt", true);
            File.Copy(Root + "Resources/Fonts/BeVietnamPro-OFL.txt", "Builds/FullRun/ThirdPartyNotices/BeVietnamPro-OFL.txt", true);
            if (File.Exists(Root + "Resources/Fonts/Coiny-OFL.txt")) File.Copy(Root + "Resources/Fonts/Coiny-OFL.txt", "Builds/FullRun/ThirdPartyNotices/Coiny-OFL.txt", true);
            string licensed = Root + "Resources/Audio/Licensed";
            if (Directory.Exists(licensed))
                foreach (string notice in Directory.GetFiles(licensed, "*.txt")) File.Copy(notice, "Builds/FullRun/ThirdPartyNotices/" + Path.GetFileName(notice), true);
        }

        [MenuItem("Arcane Brigade/Full Run/Check existing content without resetting")]
        public static void CheckExisting()
        {
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>(CatalogPath);
            if (catalog == null) throw new InvalidOperationException("Build full run content first");
            Directory.CreateDirectory("PreviewExports");
            FullRunChecks.Run(catalog);
        }
    }
}
