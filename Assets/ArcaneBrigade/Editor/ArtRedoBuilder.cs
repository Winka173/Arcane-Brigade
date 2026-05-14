using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Puts the repainted characters and weapons in (Docs/Art_Redo_Requests.md, 24/10/2026). A character sheet
    /// Art/Playtest/Redo/&lt;Id&gt;.png is a 4×2 grid of eight frames (idle, idle, walk, walk, windup, windup, release,
    /// recovery): it becomes the unit's body, its four-frame cycle and its authored motion. A weapon sheet
    /// Art/Playtest/Redo/Weapon_&lt;Id&gt;.png is one held weapon pointing right: it becomes the weapon's sprite. Missing sheets
    /// are skipped, so the set can arrive in parts.</summary>
    public static class ArtRedoBuilder
    {
        private const string Folder = "Assets/ArcaneBrigade/Art/Playtest/Redo/";

        [MenuItem("Arcane Brigade/Art/Put repainted characters and weapons in")]
        public static void Build()
        {
            if (!AssetDatabase.IsValidFolder(Folder.TrimEnd('/'))) { Debug.Log("ART_REDO no folder"); return; }
            var units = new Dictionary<string, UnitDefinition>(); var weapons = new Dictionary<string, WeaponDefinition>();
            foreach (string guid in AssetDatabase.FindAssets("t:UnitDefinition", new[] { "Assets/ArcaneBrigade/Data", "Assets/ArcaneBrigade/Resources/October" }))
            { var u = AssetDatabase.LoadAssetAtPath<UnitDefinition>(AssetDatabase.GUIDToAssetPath(guid)); if (u != null && !string.IsNullOrEmpty(u.Id) && !units.ContainsKey(u.Id)) units[u.Id] = u; }
            foreach (string guid in AssetDatabase.FindAssets("t:WeaponDefinition", new[] { "Assets/ArcaneBrigade/Data", "Assets/ArcaneBrigade/Resources/October" }))
            { var w = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(AssetDatabase.GUIDToAssetPath(guid)); if (w != null && !string.IsNullOrEmpty(w.Id) && !weapons.ContainsKey(w.Id)) weapons[w.Id] = w; }
            int characters = 0, arms = 0;
            foreach (string path in Directory.GetFiles(Folder, "*.png").Select(p => p.Replace('\\', '/')))
            {
                string name = Path.GetFileNameWithoutExtension(path);
                if (name.StartsWith("Weapon_"))
                {
                    if (!weapons.TryGetValue(name.Substring(7), out WeaponDefinition w)) { Debug.Log("ART_REDO no weapon " + name); continue; }
                    Sprite s = Single(path); if (s == null) continue;
                    w.Sprite = s; EditorUtility.SetDirty(w); arms++;
                    continue;
                }
                if (!units.TryGetValue(name, out UnitDefinition unit)) { Debug.Log("ART_REDO no unit " + name); continue; }
                Sprite[] frames = Grid(path, name, 4, 2); if (frames == null) continue;
                string motionPath = "Assets/ArcaneBrigade/Resources/" + name + "RedoMotion.asset";
                var motion = AssetDatabase.LoadAssetAtPath<AuthoredMotionSet>(motionPath);
                if (motion == null) { motion = ScriptableObject.CreateInstance<AuthoredMotionSet>(); AssetDatabase.CreateAsset(motion, motionPath); }
                motion.Idle = new[] { frames[0], frames[1] }; motion.Walk = new[] { frames[2], frames[3], frames[2], frames[3] };
                motion.Windup = new[] { frames[4], frames[5] }; motion.Release = new[] { frames[6], frames[6] }; motion.Recovery = new[] { frames[7], frames[0] };
                EditorUtility.SetDirty(motion);
                unit.Body = frames[0]; unit.AuthoredMotion = motion; unit.AnimationFrames = new[] { frames[0], frames[1], frames[2], frames[3] };
                EditorUtility.SetDirty(unit); characters++;
            }
            AssetDatabase.SaveAssets();
            Debug.Log("ART_REDO characters " + characters + ", weapons " + arms);
        }

        private static TextureImporter Prepare(string path)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
            importer.spritePixelsPerUnit = 256; importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 2048;
            return importer;
        }
        private static Sprite Single(string path)
        {
            var importer = Prepare(path); importer.spriteImportMode = SpriteImportMode.Single;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings); settings.spriteAlignment = (int)SpriteAlignment.Center; importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        private static Sprite[] Grid(string path, string id, int cols, int rows)
        {
            var importer = Prepare(path); importer.SaveAndReimport();
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null || texture.width % cols != 0 || texture.height % rows != 0) { Debug.Log("ART_REDO bad grid " + path); return null; }
            int w = texture.width / cols, h = texture.height / rows;
            var cells = new List<SpriteMetaData>();
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    cells.Add(new SpriteMetaData { name = id + "_Redo_" + (r * cols + c).ToString("D2"), rect = new Rect(c * w, texture.height - (r + 1) * h, w, h), alignment = (int)SpriteAlignment.BottomCenter, pivot = new Vector2(.5f, 0) });
            importer.spriteImportMode = SpriteImportMode.Multiple;
#pragma warning disable 618
            importer.spritesheet = cells.ToArray();
#pragma warning restore 618
            importer.SaveAndReimport();
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToArray();
            return sprites.Length == cols * rows ? sprites : null;
        }
    }
}
