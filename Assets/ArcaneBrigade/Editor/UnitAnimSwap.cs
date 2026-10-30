using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Puts the unified-style animation sheets in (Docs/Style_Brief_2026-10-27.md). Art/Playtest/Redo3/&lt;UnitId&gt;.png is the
    /// 4×2 sheet of eight frames and &lt;UnitId&gt;_Acting.png (heroes) the 4×3 sheet, as ArtRedoBuilder reads them. Each sheet is scaled so its
    /// idle frame is as tall on screen as the unit's current body, so no unit changes size.</summary>
    public static class UnitAnimSwap
    {
        private const string Folder = "Assets/ArcaneBrigade/Art/Playtest/Redo3/";

        [MenuItem("Arcane Brigade/Art/Put unified animation sheets in")]
        public static void Run()
        {
            if (!AssetDatabase.IsValidFolder(Folder.TrimEnd('/'))) { Debug.Log("UNIT_ANIM_SWAP no folder"); return; }
            var heights = new Dictionary<string, float>();
            foreach (string guid in AssetDatabase.FindAssets("t:UnitDefinition", new[] { "Assets/ArcaneBrigade/Data", "Assets/ArcaneBrigade/Resources" }))
            {
                var u = AssetDatabase.LoadAssetAtPath<UnitDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (u != null && u.Body != null && !string.IsNullOrEmpty(u.Id) && !heights.ContainsKey(u.Id)) heights[u.Id] = u.Body.bounds.size.y;
            }
            var ppu = new Dictionary<string, float>();
            foreach (string path in Directory.GetFiles(Folder, "*.png").Select(p => p.Replace('\\', '/')))
            {
                string id = Path.GetFileNameWithoutExtension(path);
                if (id.EndsWith("_Acting", System.StringComparison.Ordinal) || !heights.TryGetValue(id, out float height)) continue;
                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                texture.LoadImage(File.ReadAllBytes(path));
                int cellW = texture.width / 4, cellH = texture.height / 2, top = texture.height - 1, high = texture.height - cellH;
                Color32[] pixels = texture.GetPixels32();
                int minY = int.MaxValue, maxY = -1;
                for (int y = high; y <= top; y++)
                    for (int x = 0; x < cellW; x++)
                        if (pixels[y * texture.width + x].a > 8) { if (y < minY) minY = y; if (y > maxY) maxY = y; }
                Object.DestroyImmediate(texture);
                if (maxY < 0) throw new System.InvalidOperationException("Empty idle frame: " + path);
                ppu[id] = (maxY - minY + 1) / height;
            }
            ArtRedoBuilder.Build(Folder, ppu);
            Debug.Log("UNIT_ANIM_SWAP sheets " + ppu.Count);
        }
    }
}
