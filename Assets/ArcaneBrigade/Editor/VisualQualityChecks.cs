using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class VisualQualityChecks
    {
        [MenuItem("Arcane Brigade/Visual Quality/Check existing visual pass and build")]
        public static void BuildAndCheck() { Run(); FullRunBuilder.BuildWindows(); }
        public static void Run()
        {
            K3FinalChecks.Run();
            var results = new List<string>();
            Action<bool, string> require = (condition, name) => { if (!condition) throw new InvalidOperationException(name); results.Add("PASS " + name); };
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            require(Mathf.Approximately(catalog.CombatViewHeight, 3.9f) && Mathf.Approximately(catalog.RestViewHeight, 4.8f), "Combat zoom +38 percent; rest zoom +25 percent");
            require(catalog.WorldSize == new Vector2(34, 22), "Large traversable map and gameplay bounds retained");
            foreach (string path in new[] { "Characters_v2.png", "Weapons_v2.png" })
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath("Assets/ArcaneBrigade/Art/QualityHD/" + path);
                require(importer.maxTextureSize == 4096 && !importer.isReadable && importer.textureCompression == TextureImporterCompression.Uncompressed && importer.alphaIsTransparency && importer.npotScale == TextureImporterNPOTScale.None, "Lossless true-alpha sprite import " + path);
                Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(importer.assetPath).OfType<Sprite>().ToArray();
                require(sprites.Length == (path.StartsWith("Characters") ? 6 : 8), "All HD cells sliced " + path);
                foreach (Sprite sprite in sprites) require(sprite.rect.width > 80 && sprite.rect.height > 100 && sprite.rect.xMin >= 0 && sprite.rect.yMin >= 0 && sprite.rect.xMax <= sprite.texture.width && sprite.rect.yMax <= sprite.texture.height, "Full-resolution in-bounds sprite " + sprite.name);
            }
            require(catalog.Champions.All(unit => AssetDatabase.GetAssetPath(unit.Body).Contains("QualityHD")), "All champion bodies use new art");
            require(catalog.Weapons.All(weapon => AssetDatabase.GetAssetPath(weapon.Sprite).Contains("QualityHD") && float.IsFinite(weapon.ProjectileMuzzleOffset.x) && float.IsFinite(weapon.ProjectileMuzzleOffset.y)), "All eight weapons use new art and valid visual muzzles");
            EditorSceneManager.OpenScene("Assets/ArcaneBrigade/Scenes/RunArena.unity");
            GameObject world = GameObject.Find("HDWorld"); require(world != null, "Persistent HD map in real gameplay scene");
            Material floor = world.transform.Find("HD_TiledFloor").GetComponent<MeshRenderer>().sharedMaterial;
            var tileImporter = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(floor.mainTexture));
            require(floor.shader.name == "ArcaneBrigade/FloorHD" && tileImporter.wrapMode == TextureWrapMode.Mirror && tileImporter.mipmapEnabled && tileImporter.filterMode == FilterMode.Trilinear, "Mirrored native tile map, no stretched whole-map bitmap");
            Vector4 repeats = floor.GetVector("_TileScale");
            require(repeats.x > 5 && repeats.y > 6 && floor.GetFloat("_Saturation") > 1, "Map texel density and richer palette");
            require(floor.GetFloat("_Detail") > 0 && floor.GetFloat("_Detail") < 1, "Quiet floor detail keeps bright combat silhouettes readable");
            require(world.transform.Find("HD_PaintedBoundary").GetComponent<MeshFilter>().sharedMesh.vertexCount > 1000, "Boundary geometry remains sharp at 4K");
            require(QualitySettings.antiAliasing == 4, "4x MSAA configured for desktop geometry");
            require(PlayerSettings.resizableWindow, "Desktop window supports user-selected resolutions");
            var effectObject = new GameObject("VisualQualityEffectFixture"); ArcadeEffects effects = effectObject.AddComponent<ArcadeEffects>();
            try
            {
                effects.Play(new CombatEvent(CombatEventKind.Heal, Vector2.zero, Faction.Champion));
                Mesh mesh = effectObject.GetComponentInChildren<MeshFilter>().sharedMesh;
                require(mesh.vertexCount >= 292 && mesh.colors.Any(color => color.a == 0), "72-segment ring with soft edge alpha, no coarse 24-sided ring");
                effects.Advance(1); require(effects.ActiveCount == 0, "Polished effects still expire through existing pool lifetime");
            }
            finally { UnityEngine.Object.DestroyImmediate(effectObject); }
            Directory.CreateDirectory("PreviewExports/VisualQuality"); File.WriteAllLines("PreviewExports/VisualQuality/Checks.txt", results);
            Debug.Log("ARCANE_VISUAL_QUALITY_CHECKS_PASSED " + results.Count);
        }
    }
}
