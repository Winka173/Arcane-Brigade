using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Puts the unified-style single-pose companion and hero paintings in (Docs/Style_Brief_2026-10-27.md). A painting
    /// Art/Playtest/Redo2/&lt;UnitId&gt;.png (already trimmed to its silhouette) becomes the unit's body at the old body's on-screen height,
    /// feet on the pivot; the old authored motion, animation frames, gait and acting frames of that unit are replaced by the one painting,
    /// so no old drawing can show through. Missing files are skipped, so the set can arrive in parts.</summary>
    public static class UnitArtSwap
    {
        private const string Folder = "Assets/ArcaneBrigade/Art/Playtest/Redo2/";

        [MenuItem("Arcane Brigade/Art/Put unified paintings in")]
        public static void Run()
        {
            if (!AssetDatabase.IsValidFolder(Folder.TrimEnd('/'))) { Debug.Log("UNIT_ART_SWAP no folder"); return; }
            var all = new List<UnitDefinition>();
            foreach (string guid in AssetDatabase.FindAssets("t:UnitDefinition", new[] { "Assets/ArcaneBrigade/Data", "Assets/ArcaneBrigade/Resources/October" }))
            { var u = AssetDatabase.LoadAssetAtPath<UnitDefinition>(AssetDatabase.GUIDToAssetPath(guid)); if (u != null && !string.IsNullOrEmpty(u.Id)) all.Add(u); }
            var acting = AssetDatabase.LoadAssetAtPath<CharacterActingLibrary>("Assets/ArcaneBrigade/Resources/CharacterActingLibrary.asset");
            var entries = acting == null ? new List<CharacterActingEntry>() : acting.Entries.ToList();
            var performance = AssetDatabase.LoadAssetAtPath<ExpandedPerformanceLibrary>("Assets/ArcaneBrigade/Resources/ExpandedPerformanceLibrary.asset");
            int swapped = 0;
            foreach (string path in Directory.GetFiles(Folder, "*.png").Select(p => p.Replace('\\', '/')).OrderBy(p => p, System.StringComparer.Ordinal))
            {
                string id = Path.GetFileNameWithoutExtension(path);
                var units = all.Where(u => u.Id == id).ToList();
                if (units.Count == 0) { Debug.Log("UNIT_ART_SWAP no unit " + id); continue; }
                Sprite old = units.Select(u => u.Body).FirstOrDefault(b => b != null);
                if (old == null) { Debug.Log("UNIT_ART_SWAP no old body " + id); continue; }
                float height = old.bounds.size.y;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.npotScale = TextureImporterNPOTScale.None;
                importer.maxTextureSize = 512; importer.filterMode = FilterMode.Bilinear; importer.wrapMode = TextureWrapMode.Clamp;
                importer.textureCompression = TextureImporterCompression.CompressedHQ; importer.compressionQuality = 80;
                importer.spritePixelsPerUnit = texture.height / height;
                var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
                settings.spriteAlignment = (int)SpriteAlignment.BottomCenter; settings.spriteMeshType = SpriteMeshType.Tight; importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
                Sprite body = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (body == null) throw new System.InvalidOperationException("Not a sprite: " + path);
                foreach (UnitDefinition u in units)
                {
                    u.Body = body; u.AuthoredMotion = null; u.AnimationFrames = new[] { body };
                    EditorUtility.SetDirty(u);
                }
                var gait = performance == null ? null : performance.Find("locomotion", id);
                if (gait != null) { gait.Frames = new[] { body }; gait.ReferenceHeight = body.bounds.size.y; }
                foreach (var entry in entries) if (entry.Alias == id) entry.Alias = "";
                var own = entries.FirstOrDefault(e => e.Id == id);
                if (own == null) { own = new CharacterActingEntry { Id = id }; entries.Add(own); }
                own.ReferenceHeight = body.bounds.size.y;
                own.Curious = new[] { body }; own.Cheer = new[] { body }; own.Hurt = new[] { body }; own.Defeat = new[] { body };
                swapped++;
            }
            if (acting != null) { acting.Entries = entries.ToArray(); EditorUtility.SetDirty(acting); }
            if (performance != null) EditorUtility.SetDirty(performance);
            AssetDatabase.SaveAssets();
            ExpandedPerformanceLibrary.Invalidate();
            SpriteInkMetricsBuilder.Build();
            Debug.Log("UNIT_ART_SWAP units " + swapped);
        }
    }
}
