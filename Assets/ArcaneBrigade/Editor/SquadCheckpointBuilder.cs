using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class SquadCheckpointBuilder
    {
        private const string DataRoot = "Assets/ArcaneBrigade/Data/Checkpoint/";
        private const string ArenaPath = "Assets/ArcaneBrigade/Scenes/Arena.unity";

        [MenuItem("Arcane Brigade/Squad/Upgrade, check and capture")]
        public static void UpgradeAndCapture()
        {
            Directory.CreateDirectory("PreviewExports");
            CheckpointCatalog catalog = AssetDatabase.LoadAssetAtPath<CheckpointCatalog>(DataRoot + "CheckpointCatalog.asset");
            if (catalog == null) throw new InvalidOperationException("Build the first checkpoint assets before upgrading squad");
            if (catalog.DataVersion != "checkpoint-0.2")
            {
                WeaponDefinition crossbow = CloneAsset(catalog.RangedEnemy.Weapon, "SquadCrossbow");
                crossbow.Id = "SquadCrossbow";
                crossbow.Damage = 9f;
                crossbow.Cooldown = 0.95f;
                crossbow.Windup = 0.15f;
                crossbow.ProjectileSpeed = 7f;
                crossbow.ProjectileColor = Color.white;
                UnitDefinition knight = CloneAsset(catalog.Champion, "KnightArbalist");
                knight.Id = knight.DisplayName = "KnightArbalist";
                knight.Weapon = crossbow;
                knight.CriticalChance = 0f;
                UnitDefinition jelly = CloneAsset(catalog.MeleeEnemy, "JellySwordsman");
                jelly.Id = jelly.DisplayName = "JellySwordsman";
                jelly.Weapon = catalog.Champion.Weapon;
                jelly.Speed = 5.1f;
                jelly.BodyHeight = 0.75f;
                catalog.StartingCompanions = new[] { catalog.Companion, knight, jelly };
                catalog.CompanionLeash = 3.2f;
                catalog.CompanionRoamInnerRadius = 0.9f;
                catalog.CompanionRoamOuterRadius = 2.7f;
                catalog.DataVersion = "checkpoint-0.2";
                EditorUtility.SetDirty(crossbow);
                EditorUtility.SetDirty(knight);
                EditorUtility.SetDirty(jelly);
                EditorUtility.SetDirty(catalog);
                AssetDatabase.SaveAssets();
            }
            CheckpointChecks.Run(catalog);
            SquadCheckpointChecks.Run(catalog);
            EditorSceneManager.OpenScene(ArenaPath);
            catalog = AssetDatabase.LoadAssetAtPath<CheckpointCatalog>(DataRoot + "CheckpointCatalog.asset");
            CaptureCombat(catalog);
            CaptureRoaming(catalog);
            EditorSceneManager.OpenScene(ArenaPath);
            File.WriteAllText("PreviewExports/Checkpoint02_Capture.txt", "Actual Unity Camera.Render captures.\nEditor: " + Application.unityVersion + "\nGPU: " + SystemInfo.graphicsDeviceName + "\nResolution: 1920 x 1080\nCombat: seeded simulation after 16 seconds.\nRoaming: same seeded idle simulation at 2, 6 and 10 seconds, no enemies, champion stays at origin.\nCircles and labels are editor-only diagnostics.\n");
            Debug.Log("ARCANE_SQUAD_CAPTURE_COMPLETE");
        }

        [MenuItem("Arcane Brigade/Squad/Build Windows playable")]
        public static void BuildWindows() => CheckpointBuilder.BuildWindowsTo("Builds/Checkpoint02", "PreviewExports/Checkpoint02_Build.txt");

        private static T CloneAsset<T>(T source, string name) where T : ScriptableObject
        {
            string path = DataRoot + name + ".asset";
            T existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;
            T clone = UnityEngine.Object.Instantiate(source);
            clone.name = name;
            AssetDatabase.CreateAsset(clone, path);
            return clone;
        }

        private static void CaptureCombat(CheckpointCatalog catalog)
        {
            ArenaController controller = UnityEngine.Object.FindAnyObjectByType<ArenaController>();
            controller.CaptureFixture(16f);
            Camera camera = UnityEngine.Object.FindAnyObjectByType<Camera>();
            Label("ARCANE BRIGADE  /  SQUAD CHECKPOINT 02", new Vector2(0f, 6.55f), 0.24f);
            Label("3 ROAMING ALLIES   |   RADIUS " + catalog.CompanionLeash.ToString("0.0") + "   |   HP " + controller.Simulation.Champion.Health, new Vector2(0f, 6f), 0.16f);
            Label("Independent patrol + spacing + catch-up. Immortal companions; no bullet blocking.", new Vector2(0f, -6.45f), 0.16f);
            Capture(camera, "PreviewExports/Unity_Checkpoint02_Squad.png");
        }

        private static void CaptureRoaming(CheckpointCatalog source)
        {
            EditorSceneManager.OpenScene(ArenaPath);
            source = AssetDatabase.LoadAssetAtPath<CheckpointCatalog>(DataRoot + "CheckpointCatalog.asset");
            CheckpointCatalog catalog = UnityEngine.Object.Instantiate(source);
            catalog.EnemyBudget = 0;
            ArenaController controller = UnityEngine.Object.FindAnyObjectByType<ArenaController>();
            controller.Configure(catalog, UnityEngine.Object.FindAnyObjectByType<Camera>());
            Label("ROAM CHECK  /  CHAMPION STATIONARY  /  NO ENEMIES", new Vector2(0f, 6.55f), 0.23f);
            Label("Allies change position independently inside the circle. No fixed formation slot.", new Vector2(0f, -6.45f), 0.16f);
            var circle = new GameObject("CaptureRadius").AddComponent<LineRenderer>();
            var material = new Material(Shader.Find("Sprites/Default"));
            circle.sharedMaterial = material;
            circle.loop = true;
            circle.useWorldSpace = false;
            circle.positionCount = 96;
            circle.widthMultiplier = 0.035f;
            circle.startColor = circle.endColor = new Color(0.55f, 0.85f, 0.9f, 0.5f);
            circle.sortingOrder = 600;
            for (int index = 0; index < 96; index++)
            {
                float angle = index * Mathf.PI * 2f / 96;
                circle.SetPosition(index, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * catalog.CompanionLeash);
            }
            foreach (int seconds in new[] { 2, 6, 10 })
            {
                controller.CaptureFixture(seconds, true);
                Capture(UnityEngine.Object.FindAnyObjectByType<Camera>(), "PreviewExports/Unity_Checkpoint02_Roam_" + seconds.ToString("00") + "s.png");
            }
            UnityEngine.Object.DestroyImmediate(material);
            UnityEngine.Object.DestroyImmediate(catalog);
        }

        private static void Label(string value, Vector2 position, float size)
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
            text.color = new Color(0.9f, 0.87f, 0.73f);
        }

        private static void Capture(Camera camera, string path)
        {
            var target = new RenderTexture(1920, 1080, 24);
            RenderTexture previous = RenderTexture.active;
            Rect viewport = camera.rect;
            camera.rect = new Rect(0f, 0f, 1f, 1f);
            camera.aspect = 16f / 9f;
            camera.targetTexture = target;
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
