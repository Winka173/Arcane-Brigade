using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class VisualQualityBuilder
    {
        private const string Root = "Assets/ArcaneBrigade/";
        private const string Art = Root + "Art/QualityHD/";
        private const string Data = Root + "Data/QualityHD/";
        [MenuItem("Arcane Brigade/Visual Quality/Apply HD assets map and check build")]
        public static void ApplyAndBuild() { Apply(); VisualQualityChecks.Run(); FullRunBuilder.BuildWindows(); }

        public static void Apply()
        {
            Directory.CreateDirectory(Data); AssetDatabase.Refresh();
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>(Root + "Resources/RunCatalog.asset");
            string contract = GameplayContract(catalog);
            var bodyNames = new[] { "LanternKnight", "TeapotPilgrim", "CandleSnail", "MailboxImp", "PillowOracle", "MushroomLibrarian" };
            var bodyAreas = new Rect[6];
            for (int index = 0; index < bodyAreas.Length; index++) bodyAreas[index] = new Rect(index % 3 / 3f, index < 3 ? 0.455f : 0, 1 / 3f, index < 3 ? 0.545f : 0.455f);
            Dictionary<string, Sprite> bodies = Import(Art + "Characters_v2.png", bodyNames, bodyAreas, true);
            var weaponNames = new[] { "KeyCleaver", "TeapotPopgun", "UmbrellaLance", "MailboxLauncher", "BellFork", "BookPaddle", "CrescentBow", "RuneStaff" };
            float[] upper = { 0, 0.28f, 0.5f, 0.76f, 1 }; float[] lower = { 0, 0.28f, 0.51f, 0.68f, 1 };
            var weaponAreas = new Rect[8];
            for (int index = 0; index < weaponAreas.Length; index++)
            {
                int column = index % 4; float[] edges = index < 4 ? upper : lower;
                weaponAreas[index] = new Rect(edges[column], index < 4 ? 0.59f : 0, edges[column + 1] - edges[column], index < 4 ? 0.41f : 0.59f);
            }
            Dictionary<string, Sprite> weapons = Import(Art + "Weapons_v2.png", weaponNames, weaponAreas, false);
            foreach (string guid in AssetDatabase.FindAssets("t:UnitDefinition", new[] { Root + "Data/Run" }))
            {
                UnitDefinition unit = AssetDatabase.LoadAssetAtPath<UnitDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (unit.Body != null && bodies.TryGetValue(unit.Body.name, out Sprite body)) { unit.Body = body; EditorUtility.SetDirty(unit); }
            }
            foreach (WeaponDefinition weapon in catalog.Weapons)
            {
                weapon.Sprite = weapons[weapon.Id];
                if (weapon.Id == "CrescentBow") weapon.VisualWidth = weapon.Sprite.bounds.size.x / weapon.Sprite.bounds.size.y * 0.9f;
                Vector2 tip = weapon.Id == "TeapotPopgun" ? new Vector2(0.98f, 0.57f) : weapon.Id == "MailboxLauncher" ? new Vector2(0.98f, 0.5f) : weapon.Id == "BellFork" ? new Vector2(0.83f, 0.45f) : weapon.Id == "CrescentBow" ? new Vector2(0.94f, 0.5f) : new Vector2(0.97f, 0.5f);
                weapon.ProjectileMuzzleOffset = (Vector2.Scale(weapon.Sprite.rect.size, tip) - weapon.Sprite.pivot) / weapon.Sprite.pixelsPerUnit * (weapon.VisualWidth / weapon.Sprite.bounds.size.x);
                EditorUtility.SetDirty(weapon);
            }
            foreach (ItemDefinition item in catalog.Items)
            {
                if (item.Icon == null) continue;
                if (bodies.TryGetValue(item.Icon.name, out Sprite body)) item.Icon = body;
                else if (weapons.TryGetValue(item.Icon.name, out Sprite weapon)) item.Icon = weapon;
            }
            catalog.CombatViewHeight = 3.9f; catalog.RestViewHeight = 4.8f;
            EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets();
            BuildMap(catalog);
            if (GameplayContract(catalog) != contract) throw new InvalidOperationException("Visual pass changed gameplay stats or IDs");
            Directory.CreateDirectory("PreviewExports/VisualQuality");
            File.WriteAllText("PreviewExports/VisualQuality/Gameplay_Contract.txt", "PASS IDs, damage, health, speed, cooldown, range, contact, tier, economics, modifier and proc authoring unchanged. Only body/weapon/icon assets, camera and visual muzzle/size change.\n" + contract);
        }

        public static string GameplayContract(RunCatalog catalog)
        {
            string result = catalog.Version + "/" + catalog.WorldSize + "/" + catalog.WaveSeconds + "/" + catalog.NormalWaves + "/" + catalog.TierDamageMultiplier + "/" + catalog.TierSupportMultiplier + "/" + catalog.Income + "/" + catalog.StartingGold + "/" + catalog.RerollPrice + "/" + catalog.RerollPriceStep + "/" + catalog.AltarBaseChance + "/" + catalog.AltarChancePerEgg;
            foreach (UnitDefinition unit in catalog.Champions.Concat(catalog.Companions).Concat(catalog.Enemies).Concat(catalog.Bosses)) result += "\n" + unit.Id + "/" + unit.Health + "/" + unit.Speed + "/" + unit.Radius + "/" + unit.ContactDamage + "/" + unit.ContactInterval + "/" + unit.Passive + "/" + unit.PassiveStrength + "/" + unit.CriticalChance + "/" + string.Join(",", unit.Tags) + "/" + unit.Weapon.Id;
            foreach (WeaponDefinition weapon in catalog.Weapons) result += "\n" + weapon.Id + "/" + weapon.Kind + "/" + weapon.Damage + "/" + weapon.Cooldown + "/" + weapon.Range + "/" + weapon.ProjectileSpeed + "/" + weapon.ProjectileCount + "/" + weapon.Windup + "/" + weapon.Recovery + "/" + weapon.Element;
            foreach (ItemDefinition item in catalog.Items) result += "\n" + item.Id + "/" + item.Price + "/" + item.UnlockWave + "/" + item.ShopWeight + "/" + item.Pools + "/" + item.StackLimit + "/" + item.AltarCurrency + "/" + item.AltarPrice + "/" + JsonUtility.ToJson(new ModifierList { Values = item.EffectiveModifiers });
            foreach (ProcRule rule in catalog.ProcRules) result += "\n" + JsonUtility.ToJson(rule);
            return result;
        }
        [Serializable] private sealed class ModifierList { public StatModifier[] Values; }

        private static Dictionary<string, Sprite> Import(string path, string[] names, Rect[] areas, bool body)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 256; importer.maxTextureSize = 4096; importer.isReadable = true;
            importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.filterMode = FilterMode.Bilinear;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path); Color32[] pixels = texture.GetPixels32();
            if (pixels[0].a > 0) throw new InvalidOperationException("HD sprites require true transparent padding");
            var factory = new SpriteDataProviderFactories(); factory.Init();
            ISpriteEditorDataProvider provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            Dictionary<string, GUID> existing = provider.GetSpriteRects().ToDictionary(rect => rect.name, rect => rect.spriteID);
            var rects = new List<SpriteRect>();
            for (int index = 0; index < names.Length; index++)
            {
                Rect area = areas[index]; int minimumX = Mathf.RoundToInt(area.xMin * texture.width); int maximumX = Mathf.RoundToInt(area.xMax * texture.width);
                int minimumY = Mathf.RoundToInt(area.yMin * texture.height); int maximumY = Mathf.RoundToInt(area.yMax * texture.height);
                int left = maximumX; int right = minimumX; int bottom = maximumY; int top = minimumY;
                for (int vertical = minimumY; vertical < maximumY; vertical++)
                    for (int horizontal = minimumX; horizontal < maximumX; horizontal++)
                        if (pixels[vertical * texture.width + horizontal].a >= 160)
                        {
                            left = Mathf.Min(left, horizontal); right = Mathf.Max(right, horizontal); bottom = Mathf.Min(bottom, vertical); top = Mathf.Max(top, vertical);
                        }
                if (right <= left || top <= bottom) throw new InvalidOperationException("Empty HD sprite: " + names[index]);
                Vector2 pivot = body ? new Vector2(0.5f, 0) : Pivot(names[index]);
                rects.Add(new SpriteRect { name = names[index], rect = new Rect(left, bottom, right - left + 1, top - bottom + 1), pivot = pivot, alignment = SpriteAlignment.Custom, spriteID = existing.TryGetValue(names[index], out GUID identifier) ? identifier : GUID.Generate() });
            }
            provider.SetSpriteRects(rects.ToArray()); provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(rect => new SpriteNameFileIdPair(rect.name, rect.spriteID))); provider.Apply(); importer.SaveAndReimport();
            importer.isReadable = false; importer.SaveAndReimport();
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToDictionary(sprite => sprite.name);
        }
        private static Vector2 Pivot(string id) => id == "TeapotPopgun" ? new Vector2(0.25f, 0.14f) : id == "MailboxLauncher" ? new Vector2(0.14f, 0.2f) : id == "CrescentBow" ? new Vector2(0.60f, 0.5f) : id == "KeyCleaver" || id == "BookPaddle" || id == "RuneStaff" ? new Vector2(0.21f, 0.5f) : new Vector2(0.14f, 0.5f);

        private static void BuildMap(RunCatalog catalog)
        {
            string tilePath = Art + "FloorTile_v2.png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(tilePath);
            importer.textureType = TextureImporterType.Default; importer.maxTextureSize = 4096; importer.mipmapEnabled = true;
            importer.filterMode = FilterMode.Trilinear; importer.anisoLevel = 4; importer.wrapMode = TextureWrapMode.Mirror;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            Texture2D tile = AssetDatabase.LoadAssetAtPath<Texture2D>(tilePath);
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(Art + "FloorHD.shader");
            Material floor = AssetDatabase.LoadAssetAtPath<Material>(Data + "FloorHD.mat");
            if (floor == null) { floor = new Material(shader); AssetDatabase.CreateAsset(floor, Data + "FloorHD.mat"); }
            floor.shader = shader; floor.mainTexture = tile;
            floor.SetVector("_TileScale", new Vector4(catalog.WorldSize.x / 6f, catalog.WorldSize.y / (6f * tile.height / tile.width), 0, 0));
            floor.SetColor("_Color", new Color(0.94f, 1.0f, 1.02f)); floor.SetFloat("_Saturation", 1.12f);
            floor.SetColor("_BaseColor", new Color(0.12f, 0.25f, 0.34f)); floor.SetFloat("_Detail", 0.6f); EditorUtility.SetDirty(floor);
            EditorSceneManager.OpenScene(Root + "Scenes/RunArena.unity");
            GameObject previous = GameObject.Find("HDWorld"); if (previous != null) UnityEngine.Object.DestroyImmediate(previous);
            foreach (SpriteRenderer renderer in UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include)) if (renderer.sortingOrder < 0) renderer.gameObject.SetActive(false);
            var world = new GameObject("HDWorld");
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Quad); ground.name = "HD_TiledFloor"; ground.transform.SetParent(world.transform);
            UnityEngine.Object.DestroyImmediate(ground.GetComponent<Collider>()); ground.transform.localScale = new Vector3(catalog.WorldSize.x, catalog.WorldSize.y, 1);
            ground.GetComponent<MeshRenderer>().sharedMaterial = floor; ground.GetComponent<MeshRenderer>().sortingOrder = -100;
            BuildBoundary(world.transform, catalog.WorldSize);
            Camera camera = UnityEngine.Object.FindAnyObjectByType<Camera>(); camera.allowMSAA = true;
            QualitySettings.antiAliasing = 4;
            PlayerSettings.resizableWindow = true;
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene()); AssetDatabase.SaveAssets();
        }

        private static void BuildBoundary(Transform parent, Vector2 size)
        {
            var vertices = new List<Vector3>(); var colors = new List<Color>(); var triangles = new List<int>();
            for (int side = 0; side < 4; side++)
            {
                bool horizontal = side < 2; float length = horizontal ? size.x : size.y; int count = Mathf.CeilToInt(length / 1.15f);
                for (int index = 0; index < count; index++)
                {
                    float along = -length * 0.5f + (index + 0.5f) * length / count;
                    Vector2 center = horizontal ? new Vector2(along, side == 0 ? -size.y * 0.5f : size.y * 0.5f) : new Vector2(side == 2 ? -size.x * 0.5f : size.x * 0.5f, along);
                    for (int layer = 0; layer < 2; layer++)
                    {
                        int first = vertices.Count;
                        Color tint = layer == 0 ? new Color(0.06f, 0.10f, 0.19f) : index % 3 == 0 ? new Color(0.31f, 0.27f, 0.52f) : new Color(0.17f, 0.31f, 0.45f);
                        vertices.Add(center); colors.Add(tint);
                        for (int point = 0; point <= 8; point++)
                        {
                            float angle = point * Mathf.PI / 4; float irregular = 0.92f + Mathf.Sin((point % 8) * 2.7f + index) * 0.08f;
                            Vector2 radius = horizontal ? new Vector2(length / count * 0.65f, 0.55f) : new Vector2(0.55f, length / count * 0.65f);
                            vertices.Add(center + Vector2.Scale(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)), radius) * irregular * (layer == 0 ? 1 : 0.83f)); colors.Add(point < 4 && layer > 0 ? Color.Lerp(tint, new Color(0.35f, 0.47f, 0.61f), 0.35f) : tint);
                            if (point > 0) { triangles.Add(first); triangles.Add(first + point); triangles.Add(first + point + 1); }
                        }
                    }
                }
            }
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(Data + "Boundary.asset");
            if (mesh == null) { mesh = new Mesh(); AssetDatabase.CreateAsset(mesh, Data + "Boundary.asset"); }
            mesh.Clear(); mesh.SetVertices(vertices); mesh.SetColors(colors); mesh.SetTriangles(triangles, 0); mesh.RecalculateBounds(); EditorUtility.SetDirty(mesh);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(Data + "Boundary.mat");
            if (material == null) { material = new Material(Shader.Find("Sprites/Default")); AssetDatabase.CreateAsset(material, Data + "Boundary.mat"); }
            var boundary = new GameObject("HD_PaintedBoundary"); boundary.transform.SetParent(parent); boundary.AddComponent<MeshFilter>().sharedMesh = mesh; MeshRenderer renderer = boundary.AddComponent<MeshRenderer>(); renderer.sharedMaterial = material; renderer.sortingOrder = -80;
        }
    }
}
