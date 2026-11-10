using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Puts the redrawn bosses and big enemies in (requirement 09/11/2026: they were drawn on screen larger than their
    /// pictures and moved on four frames). Each creature is a folder of frames <c>Art/BigRedo/&lt;Id&gt;/&lt;Id&gt;_fNN.png</c>, all on
    /// one canvas with the feet on one baseline (Tools/Art/big_redo_frames.py prepares them): bosses have 12 (idle 3, moving 4,
    /// windup 2, release 2, recovery 1), big enemies 10 (idle 2, moving 4, windup 2, release 1, recovery 1). A creature whose
    /// folder is missing or incomplete keeps its old art, so the set can arrive in parts.</summary>
    public static class BigRedoBuilder
    {
        public const string Folder = "Assets/ArcaneBrigade/Art/BigRedo";

        [MenuItem("Arcane Brigade/Art/Put redrawn bosses and big enemies in")]
        public static void Build()
        {
            if (!AssetDatabase.IsValidFolder(Folder)) { Debug.Log("BIG_REDO no folder"); return; }
            var units = new List<UnitDefinition>();
            foreach (string guid in AssetDatabase.FindAssets("t:UnitDefinition", new[] { "Assets/ArcaneBrigade/Data", "Assets/ArcaneBrigade/Resources" }))
            {
                var unit = AssetDatabase.LoadAssetAtPath<UnitDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (unit != null && !string.IsNullOrEmpty(unit.Id)) units.Add(unit);
            }
            int done = 0, reactions = 0;
            foreach (string directory in Directory.GetDirectories(Folder).Select(d => d.Replace('\\', '/')).OrderBy(d => d, System.StringComparer.Ordinal))
            {
                string id = Path.GetFileName(directory);
                UnitDefinition[] matches = units.Where(u => u.Id == id).ToArray();
                if (matches.Length == 0) { Debug.Log("BIG_REDO no unit " + id); continue; }
                bool boss = matches[0].IsBoss;
                int count = boss ? 12 : 10;
                var frames = new Sprite[count];
                for (int i = 0; i < count; i++)
                {
                    string path = directory + "/" + id + "_f" + (i + 1).ToString("00") + ".png";
                    if (!File.Exists(path)) { frames = null; break; }
                    frames[i] = Frame(path);
                    if (frames[i] == null) { frames = null; break; }
                }
                if (frames == null) { Debug.Log("BIG_REDO incomplete " + id); continue; }
                string motionPath = "Assets/ArcaneBrigade/Resources/" + id + "BigRedoMotion.asset";
                var motion = AssetDatabase.LoadAssetAtPath<AuthoredMotionSet>(motionPath);
                if (motion == null) { motion = ScriptableObject.CreateInstance<AuthoredMotionSet>(); AssetDatabase.CreateAsset(motion, motionPath); }
                if (boss)
                {
                    motion.Idle = frames.Take(3).ToArray(); motion.Walk = frames.Skip(3).Take(4).ToArray();
                    motion.Windup = frames.Skip(7).Take(2).ToArray(); motion.Release = frames.Skip(9).Take(2).ToArray(); motion.Recovery = new[] { frames[11], frames[0] };
                }
                else
                {
                    motion.Idle = frames.Take(2).ToArray(); motion.Walk = frames.Skip(2).Take(4).ToArray();
                    motion.Windup = frames.Skip(6).Take(2).ToArray(); motion.Release = new[] { frames[8] }; motion.Recovery = new[] { frames[9], frames[0] };
                }
                motion.IllustratedHeight = 0; // the first idle frame sets the scale
                EditorUtility.SetDirty(motion);
                foreach (UnitDefinition unit in matches)
                {
                    unit.Body = frames[0]; unit.AuthoredMotion = motion;
                    unit.AnimationFrames = new[] { frames[0], motion.Walk[0], motion.Windup[0], motion.Release[0] };
                    EditorUtility.SetDirty(unit);
                }
                if (Reactions(directory, id, boss, frames[0])) reactions++;
                done++;
            }
            AssetDatabase.SaveAssets();
            SpriteInkMetricsBuilder.Build();
            ExpandedPerformanceLibrary.Invalidate();
            Debug.Log("BIG_REDO creatures " + done + " reactions " + reactions);
        }

        /// <summary>The redrawn struck, rearing and slumped poses (<c>&lt;Id&gt;_r1..r3.png</c>, on the frames' pixel scale) replace
        /// the creature's old reaction sheet, which no longer matched its new body: struck when hit, struck then slumped when it
        /// dies. The evolved form keeps its own sheet because it keeps its own body.</summary>
        private static bool Reactions(string directory, string id, bool boss, Sprite calm)
        {
            var poses = new Sprite[3];
            for (int i = 0; i < 3; i++)
            {
                string path = directory + "/" + id + "_r" + (i + 1) + ".png";
                if (!File.Exists(path)) return false;
                poses[i] = Frame(path, calm.pixelsPerUnit);
                if (poses[i] == null) return false;
            }
            const string libraryPath = "Assets/ArcaneBrigade/Resources/ExpandedPerformanceLibrary.asset";
            var library = AssetDatabase.LoadAssetAtPath<ExpandedPerformanceLibrary>(libraryPath);
            if (library == null) return false;
            string kind = boss ? "bosses" : "enemies";
            ExpandedPerformanceEntry entry = library.Entries.FirstOrDefault(e => e.Kind == kind && e.Id == id && e.Evolution == 0);
            if (entry == null)
            {
                entry = new ExpandedPerformanceEntry { Id = id, Kind = kind, Evolution = 0 };
                library.Entries = library.Entries.Append(entry).ToArray();
            }
            entry.Frames = new[] { poses[1], poses[0], poses[0], poses[2] }; // rearing, struck, struck, slumped
            entry.ReferenceHeight = calm.bounds.size.y;
            entry.HurtOnly = true;
            EditorUtility.SetDirty(library);
            return true;
        }

        /// <summary>One frame: a single sprite, pivot at the feet, one canvas height to one unit (the game sizes bodies by their ink).
        /// The height is read from the file itself: before the first import Unity scales a texture to a power of two.</summary>
        private static Sprite Frame(string path, float pixelsPerUnit = 0)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 1024; importer.filterMode = FilterMode.Bilinear; importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.CompressedHQ; importer.compressionQuality = 80;
            importer.GetSourceTextureWidthAndHeight(out int _, out int height);
            importer.spritePixelsPerUnit = pixelsPerUnit > 0 ? pixelsPerUnit : height;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.BottomCenter; settings.spritePivot = new Vector2(.5f, 0); importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
