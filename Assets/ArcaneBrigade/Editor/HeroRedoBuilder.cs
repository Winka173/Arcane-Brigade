using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Puts the redrawn heroes in (requirement 10/11/2026: heroes had 4-8 body frames against a boss's 12, and the hero
    /// in the main-menu room walked on two). A hero folder <c>Art/HeroRedo/&lt;Id&gt;/&lt;Id&gt;_c01..c12.png</c> replaces its combat
    /// frames (idle 3, moving 4, windup 2, release 2, recovery 1, as the redrawn bosses); <c>Resources/HeroHub/&lt;Id&gt;/&lt;Id&gt;_h01..h22.png</c>
    /// are the room's clips (HeroHubClips). Tools/Art/hero_redo_frames.py prepares both.</summary>
    public static class HeroRedoBuilder
    {
        public const string Folder = "Assets/ArcaneBrigade/Art/HeroRedo", HubFolder = "Assets/ArcaneBrigade/Resources/HeroHub";

        [MenuItem("Arcane Brigade/Art/Put redrawn heroes in")]
        public static void Build()
        {
            if (!AssetDatabase.IsValidFolder(Folder)) { Debug.Log("HERO_REDO no folder"); return; }
            var units = new List<UnitDefinition>();
            foreach (string guid in AssetDatabase.FindAssets("t:UnitDefinition", new[] { "Assets/ArcaneBrigade/Data", "Assets/ArcaneBrigade/Resources" }))
            {
                var unit = AssetDatabase.LoadAssetAtPath<UnitDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (unit != null && !string.IsNullOrEmpty(unit.Id)) units.Add(unit);
            }
            int done = 0, rooms = 0;
            foreach (string directory in Directory.GetDirectories(Folder).Select(d => d.Replace('\\', '/')).OrderBy(d => d, System.StringComparer.Ordinal))
            {
                string id = Path.GetFileName(directory);
                UnitDefinition[] matches = units.Where(u => u.Id == id).ToArray();
                if (matches.Length == 0) { Debug.Log("HERO_REDO no unit " + id); continue; }
                var frames = new Sprite[12];
                for (int i = 0; i < 12; i++)
                {
                    string path = directory + "/" + id + "_c" + (i + 1).ToString("00") + ".png";
                    frames[i] = File.Exists(path) ? BigRedoBuilder.Frame(path) : null;
                    if (frames[i] == null) { frames = null; break; }
                }
                if (frames == null) { Debug.Log("HERO_REDO incomplete " + id); continue; }
                foreach (UnitDefinition unit in matches)
                {
                    AuthoredMotionSet motion = unit.AuthoredMotion;
                    if (motion == null) continue;
                    motion.Idle = frames.Take(3).ToArray(); motion.Walk = frames.Skip(3).Take(4).ToArray();
                    motion.Windup = frames.Skip(7).Take(2).ToArray(); motion.Release = frames.Skip(9).Take(2).ToArray(); motion.Recovery = new[] { frames[11], frames[0] };
                    motion.IllustratedHeight = 0; // the first idle frame sets the scale
                    EditorUtility.SetDirty(motion);
                    unit.Body = frames[0];
                    unit.AnimationFrames = new[] { frames[0], motion.Walk[0], motion.Windup[0], motion.Release[0] };
                    EditorUtility.SetDirty(unit);
                }
                string hub = HubFolder + "/" + id;
                if (AssetDatabase.IsValidFolder(hub))
                {
                    int count = 0;
                    foreach (string file in Directory.GetFiles(hub, "*.png").Select(f => f.Replace('\\', '/')).OrderBy(f => f, System.StringComparer.Ordinal)) { BigRedoBuilder.Frame(file); count++; }
                    if (count >= 22) rooms++;
                }
                done++;
            }
            AssetDatabase.SaveAssets();
            SpriteInkMetricsBuilder.Build();
            Debug.Log("HERO_REDO heroes " + done + " rooms " + rooms);
        }
    }
}
