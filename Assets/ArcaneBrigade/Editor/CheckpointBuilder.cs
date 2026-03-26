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
    public static class CheckpointBuilder
    {
        private const string Root = "Assets/ArcaneBrigade";
        private const string Atlas = Root + "/Art/Checkpoint/ModularArcade_Atlas.png";
        private static readonly string[] SpriteNames = { "KnightBody", "WispBody", "JellyBody", "Sword", "Crossbow", "Staff", "Hand", "Projectile", "Coin" };
        private static readonly string[] ScenePaths = { Root + "/Scenes/Boot.unity", Root + "/Scenes/Frontend.unity", Root + "/Scenes/Arena.unity" };

        [MenuItem("Arcane Brigade/Checkpoint/Build assets, check and capture")]
        public static void BuildAndCapture()
        {
            foreach (string folder in new[] { "/Data/Checkpoint", "/Prefabs/Checkpoint", "/Scenes", "/Art/Checkpoint" }) Directory.CreateDirectory(Root + folder);
            Directory.CreateDirectory("PreviewExports");
            AssetDatabase.Refresh();
            Dictionary<string, Sprite> sprites = ImportAtlas();
            Sprite disc = CreateDisc();
            CheckpointCatalog catalog = CreateCatalog(sprites, disc);
            BuildScenes(catalog, disc);
            catalog = AssetDatabase.LoadAssetAtPath<CheckpointCatalog>(Root + "/Data/Checkpoint/CheckpointCatalog.asset");
            CheckpointChecks.Run(catalog);
            CaptureArena(catalog);
            CaptureModularGallery(catalog, sprites, disc);
            EditorSceneManager.OpenScene(ScenePaths[2]);
            AssetDatabase.SaveAssets();
            File.WriteAllText("PreviewExports/Checkpoint01_Capture.txt", "Unity Camera.Render capture.\nEditor: " + Application.unityVersion + "\nGPU: " + SystemInfo.graphicsDeviceName + "\nResolution: 1920 x 1080\nAtlas: original modular artwork.\nArena: actual simulation stepped in Editor; capture labels are editor-only.\nAnimation: procedural bob, squash, grip rotation and recoil; not final hand-drawn clips.\n");
            Debug.Log("ARCANE_CHECKPOINT_CAPTURE_COMPLETE");
        }

        [MenuItem("Arcane Brigade/Checkpoint/Build Windows playable")]
        public static void BuildWindows()
        {
            BuildWindowsTo("Builds/Checkpoint01", "PreviewExports/Checkpoint01_Build.txt");
        }

        public static void BuildWindowsTo(string folder, string reportPath)
        {
            Directory.CreateDirectory(folder);
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = ScenePaths,
                locationPathName = folder + "/ArcaneBrigade.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });
            File.WriteAllText(reportPath, "Target: Windows x64\nResult: " + report.summary.result + "\nErrors: " + report.summary.totalErrors + "\nWarnings: " + report.summary.totalWarnings + "\nSize: " + report.summary.totalSize + " bytes\n");
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Windows build failed: " + report.summary.result);
            Debug.Log("ARCANE_CHECKPOINT_WINDOWS_COMPLETE");
        }

        private static Dictionary<string, Sprite> ImportAtlas()
        {
            if (BuildSizePolicy.IsPacked(Atlas)) return BuildSizePolicy.PackedSprites(Atlas).ToDictionary(sprite => sprite.name); // repacked sheet: keep its sprites
            AssetDatabase.ImportAsset(Atlas, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(Atlas);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 256f;
            importer.isReadable = true;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 4096;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Atlas);
            Color32[] pixels = texture.GetPixels32();
            var factories = new SpriteDataProviderFactories();
            factories.Init();
            ISpriteEditorDataProvider provider = factories.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            Dictionary<string, GUID> ids = provider.GetSpriteRects().ToDictionary(rect => rect.name, rect => rect.spriteID);
            var rects = new List<SpriteRect>();
            float[] rows = { 0f, 0.45f, 0.76f, 1f };
            for (int index = 0; index < SpriteNames.Length; index++)
            {
                int column = index % 3;
                int row = index / 3;
                int minimumX = column * texture.width / 3;
                int maximumX = (column + 1) * texture.width / 3;
                int minimumY = Mathf.RoundToInt((1f - rows[row + 1]) * texture.height);
                int maximumY = Mathf.RoundToInt((1f - rows[row]) * texture.height);
                int left = maximumX, right = minimumX, bottom = maximumY, top = minimumY;
                for (int vertical = minimumY; vertical < maximumY; vertical++)
                    for (int horizontal = minimumX; horizontal < maximumX; horizontal++)
                    {
                        if (pixels[vertical * texture.width + horizontal].a < 64) continue;
                        left = Mathf.Min(left, horizontal);
                        right = Mathf.Max(right, horizontal);
                        bottom = Mathf.Min(bottom, vertical);
                        top = Mathf.Max(top, vertical);
                    }
                if (right < left || top < bottom) throw new InvalidOperationException("Empty atlas cell: " + SpriteNames[index]);
                rects.Add(new SpriteRect
                {
                    name = SpriteNames[index],
                    spriteID = ids.TryGetValue(SpriteNames[index], out GUID identifier) ? identifier : GUID.Generate(),
                    rect = new Rect(left, bottom, right - left + 1, top - bottom + 1),
                    alignment = SpriteAlignment.Custom,
                    pivot = row == 0 ? new Vector2(0.5f, 0f) : row == 1 ? new Vector2(0.13f, 0.5f) : new Vector2(0.5f, 0.5f)
                });
            }
            provider.SetSpriteRects(rects.ToArray());
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(rect => new SpriteNameFileIdPair(rect.name, rect.spriteID)));
            provider.Apply();
            importer.SaveAndReimport();
            return AssetDatabase.LoadAllAssetsAtPath(Atlas).OfType<Sprite>().ToDictionary(sprite => sprite.name);
        }

        private static Sprite CreateDisc()
        {
            string path = Root + "/Art/Checkpoint/Disc.png";
            var texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            var pixels = new Color[64 * 64];
            for (int vertical = 0; vertical < 64; vertical++)
                for (int horizontal = 0; horizontal < 64; horizontal++)
                {
                    float distance = new Vector2(horizontal - 31.5f, vertical - 31.5f).magnitude;
                    pixels[vertical * 64 + horizontal] = new Color(1f, 1f, 1f, Mathf.Clamp01(31.5f - distance));
                }
            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 64f;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
            string squarePath = Root + "/Art/Checkpoint/Square.png";
            var square = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            square.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
            square.Apply();
            File.WriteAllBytes(squarePath, square.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(square);
            AssetDatabase.ImportAsset(squarePath);
            var squareImporter = (TextureImporter)AssetImporter.GetAtPath(squarePath);
            squareImporter.textureType = TextureImporterType.Sprite;
            squareImporter.spriteImportMode = SpriteImportMode.Single;
            squareImporter.spritePixelsPerUnit = 2f;
            squareImporter.mipmapEnabled = false;
            squareImporter.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static T Asset<T>(string name) where T : ScriptableObject
        {
            string path = Root + "/Data/Checkpoint/" + name + ".asset";
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }
            return asset;
        }

        private static WeaponDefinition Weapon(string name, WeaponKind kind, Sprite sprite, float damage, float range, float cooldown)
        {
            WeaponDefinition weapon = Asset<WeaponDefinition>(name);
            weapon.Id = name;
            weapon.Kind = kind;
            weapon.Sprite = sprite;
            weapon.Damage = damage;
            weapon.Range = range;
            weapon.Cooldown = cooldown;
            EditorUtility.SetDirty(weapon);
            return weapon;
        }

        private static UnitDefinition Unit(string name, Sprite sprite, WeaponDefinition weapon, float health, float speed, float height)
        {
            UnitDefinition unit = Asset<UnitDefinition>(name);
            unit.Id = name;
            unit.DisplayName = name;
            unit.Body = sprite;
            unit.Weapon = weapon;
            unit.Health = health;
            unit.Speed = speed;
            unit.BodyHeight = height;
            unit.ContactDamage = 4f;
            unit.ContactInterval = 0.8f;
            EditorUtility.SetDirty(unit);
            return unit;
        }

        private static CheckpointCatalog CreateCatalog(Dictionary<string, Sprite> sprites, Sprite disc)
        {
            WeaponDefinition sword = Weapon("KnightSword", WeaponKind.Melee, sprites["Sword"], 14f, 1.8f, 0.6f);
            sword.VisualWidth = 0.75f;
            WeaponDefinition staff = Weapon("WispStaff", WeaponKind.Projectile, sprites["Staff"], 10f, 7.5f, 0.85f);
            staff.VisualWidth = 0.7f;
            WeaponDefinition crossbow = Weapon("EnemyCrossbow", WeaponKind.Projectile, sprites["Crossbow"], 5f, 8f, 1.8f);
            crossbow.VisualWidth = 0.65f;
            crossbow.Windup = 0.35f;
            crossbow.ProjectileSpeed = 5.5f;
            crossbow.ProjectileLifetime = 4f;
            crossbow.ProjectileColor = new Color(1f, 0.35f, 0.36f);
            WeaponDefinition contact = Weapon("JellyContact", WeaponKind.Contact, null, 4f, 0.55f, 0.8f);
            CheckpointCatalog catalog = Asset<CheckpointCatalog>("CheckpointCatalog");
            catalog.Champion = Unit("SleepyKnight", sprites["KnightBody"], sword, 100f, 4.8f, 0.95f);
            catalog.Champion.CriticalChance = 0.03f;
            catalog.Companion = Unit("HoodWisp", sprites["WispBody"], staff, 1f, 5.2f, 0.85f);
            catalog.MeleeEnemy = Unit("GrumpyJelly", sprites["JellyBody"], contact, 26f, 1.4f, 0.65f);
            catalog.RangedEnemy = Unit("JellyArbalist", sprites["JellyBody"], crossbow, 22f, 1.1f, 0.65f);
            catalog.RangedEnemy.Tint = new Color(0.75f, 0.8f, 1f);
            catalog.Hand = sprites["Hand"];
            catalog.Projectile = sprites["Projectile"];
            catalog.Coin = sprites["Coin"];
            catalog.ImpactSprite = disc;
            var unitRoot = new GameObject("ModularUnit");
            unitRoot.AddComponent<UnitVisual>().CreateHierarchy(disc);
            catalog.UnitPrefab = PrefabUtility.SaveAsPrefabAsset(unitRoot, Root + "/Prefabs/Checkpoint/ModularUnit.prefab");
            UnityEngine.Object.DestroyImmediate(unitRoot);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            return catalog;
        }

        private static Camera CreateCamera(float size)
        {
            Camera camera = new GameObject("ArenaCamera").AddComponent<Camera>();
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.orthographic = true;
            camera.orthographicSize = size;
            camera.backgroundColor = new Color(0.06f, 0.10f, 0.13f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.tag = "MainCamera";
            camera.gameObject.AddComponent<ArenaCamera>();
            return camera;
        }

        private static void Panel(string name, Vector2 position, Vector2 size, Color color, int order)
        {
            var child = new GameObject(name);
            child.transform.position = position;
            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Art/Checkpoint/Square.png");
            if (renderer.sprite == null) throw new InvalidOperationException("Missing floor sprite import");
            renderer.color = color;
            renderer.sortingOrder = order;
            child.transform.localScale = new Vector3(size.x, size.y, 1f);
        }

        private static void BuildScenes(CheckpointCatalog catalog, Sprite disc)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("BootLoader").AddComponent<BootLoader>();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePaths[0]);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera(7.1f);
            new GameObject("CheckpointMenu").AddComponent<CheckpointMenu>();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePaths[1]);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            catalog = AssetDatabase.LoadAssetAtPath<CheckpointCatalog>(Root + "/Data/Checkpoint/CheckpointCatalog.asset");
            disc = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Art/Checkpoint/Disc.png");
            Camera camera = CreateCamera(7.1f);
            Panel("OuterRim", Vector2.zero, catalog.ArenaSize + Vector2.one * 0.4f, new Color(0.25f, 0.38f, 0.41f), -100);
            Panel("ArenaFloor", Vector2.zero, catalog.ArenaSize, new Color(0.12f, 0.20f, 0.24f), -99);
            for (int row = 0; row < 6; row++)
                for (int column = 0; column < 12; column++)
                    Panel("Tile_" + row + "_" + column, new Vector2(-10.08f + column * 1.83f, -4.58f + row * 1.83f), new Vector2(1.80f, 1.80f), new Color(0.135f + (row + column) % 2 * 0.007f, 0.218f, 0.25f), -98);
            var seal = new GameObject("FloorSeal").AddComponent<SpriteRenderer>();
            seal.sprite = disc;
            seal.color = new Color(0.19f, 0.30f, 0.33f);
            seal.sortingOrder = -97;
            seal.transform.localScale = Vector3.one * 3.2f;
            for (int index = 0; index < 4; index++)
            {
                Vector2 position = new Vector2(index % 2 == 0 ? -11.7f : 11.7f, index < 2 ? -4f : 4f);
                Panel("Pillar" + index, position, new Vector2(0.6f, 1.6f), new Color(0.20f, 0.32f, 0.35f), -96);
                Panel("PillarGold" + index, position + Vector2.up * 0.45f, new Vector2(0.64f, 0.12f), new Color(0.70f, 0.58f, 0.32f), -95);
            }
            new GameObject("ArenaController").AddComponent<ArenaController>().Configure(catalog, camera);
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePaths[2]);
            EditorBuildSettings.scenes = ScenePaths.Select(path => new EditorBuildSettingsScene(path, true)).ToArray();
            PlayerSettings.productName = "Arcane Brigade";
            PlayerSettings.companyName = "Winka";
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.defaultIsNativeResolution = false;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        }

        private static void Text(string value, Vector2 position, float size, Color color)
        {
            var text = new GameObject("CaptureLabel").AddComponent<TextMesh>();
            text.transform.position = position;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.GetComponent<MeshRenderer>().sharedMaterial = text.font.material;
            text.GetComponent<MeshRenderer>().sortingOrder = 3000;
            text.text = value;
            text.fontSize = 80;
            text.characterSize = size * 0.1f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = color;
        }

        private static void CaptureArena(CheckpointCatalog catalog)
        {
            ArenaController controller = UnityEngine.Object.FindAnyObjectByType<ArenaController>();
            controller.CaptureFixture(22f);
            Text("ARCANE BRIGADE   /   COMBAT CHECKPOINT 01", new Vector2(0f, 6.6f), 0.27f, new Color(0.9f, 0.86f, 0.68f));
            Text("HP " + controller.Simulation.Champion.Health + "   |   GOLD " + controller.Simulation.Gold + "   |   KILLS " + controller.Simulation.Kills + "   |   WAVE 1", new Vector2(0f, 5.95f), 0.18f, Color.white);
            Text("Move only. Automatic sword + staff. Immortal ally. No arms; hands move with weapons.", new Vector2(0f, -6.35f), 0.17f, new Color(0.68f, 0.79f, 0.81f));
            Capture(UnityEngine.Object.FindAnyObjectByType<Camera>(), "PreviewExports/Unity_Checkpoint01_Arena.png");
        }

        private static void CaptureModularGallery(CheckpointCatalog catalog, Dictionary<string, Sprite> sprites, Sprite disc)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            catalog = AssetDatabase.LoadAssetAtPath<CheckpointCatalog>(Root + "/Data/Checkpoint/CheckpointCatalog.asset");
            sprites = AssetDatabase.LoadAllAssetsAtPath(Atlas).OfType<Sprite>().ToDictionary(sprite => sprite.name);
            Camera camera = CreateCamera(4.5f);
            Text("MODULAR ARCADE  /  BODY + FLOATING HAND + WEAPON", new Vector2(0f, 3.9f), 0.23f, new Color(0.9f, 0.86f, 0.68f));
            Text("Original silhouettes. No upper arms or forearms. Each body accepts each weapon.", new Vector2(0f, 3.4f), 0.15f, Color.white);
            Sprite[] bodies = { sprites["KnightBody"], sprites["WispBody"], sprites["JellyBody"] };
            WeaponDefinition[] weapons = { catalog.Champion.Weapon, catalog.RangedEnemy.Weapon, catalog.Companion.Weapon };
            for (int row = 0; row < 3; row++)
                for (int column = 0; column < 3; column++)
                {
                    var definition = ScriptableObject.CreateInstance<UnitDefinition>();
                    definition.Body = bodies[column];
                    definition.Weapon = weapons[row];
                    definition.BodyHeight = 1.1f;
                    definition.DisplayName = SpriteNames[column];
                    var state = new UnitState { Id = row * 3 + column + 1, Definition = definition, Position = new Vector2((column - 1) * 4.6f, 2.2f - row * 2f), AttackDirection = Vector2.right };
                    state.PreviousPosition = state.Position;
                    GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(catalog.UnitPrefab);
                    instance.GetComponent<UnitVisual>().Bind(state, catalog);
                    instance.GetComponent<UnitVisual>().Display(0f, 1f);
                    Text(weapons[row].Kind == WeaponKind.Melee ? "SWORD" : row == 1 ? "CROSSBOW" : "STAFF", state.Position + Vector2.down * 0.95f, 0.13f, new Color(0.67f, 0.78f, 0.8f));
                }
            Capture(camera, "PreviewExports/Unity_Checkpoint01_Modular.png");
        }

        private static void Capture(Camera camera, string path)
        {
            var target = new RenderTexture(1920, 1080, 24);
            RenderTexture previous = RenderTexture.active;
            Rect viewport = camera.rect;
            camera.rect = new Rect(0f, 0f, 1f, 1f);
            camera.targetTexture = target;
            camera.aspect = 16f / 9f;
            camera.Render();
            RenderTexture.active = target;
            var texture = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            camera.targetTexture = null;
            camera.rect = viewport;
            RenderTexture.active = previous;
            target.Release();
            UnityEngine.Object.DestroyImmediate(texture);
            UnityEngine.Object.DestroyImmediate(target);
        }
    }
}
