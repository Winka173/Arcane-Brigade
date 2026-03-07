using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Finds walk poses that disagree with the rest of the hero's cycle: each walk frame's silhouette (alpha mask fitted
    /// to its painted ink) is compared with the first idle pose as drawn and mirrored. A frame that matches far better mirrored
    /// has its loose parts (hat star, ribbon) on the other side, so the hero seemed to flip every step; it is mapped to the
    /// cycle's first walk pose in Resources/HeroPoseRepairs.asset. Small differences are ordinary pose changes and are left.</summary>
    public static class HeroPoseRepairBuilder
    {
        private const int N = 48;
        private const float Margin = 0.15f;

        [MenuItem("Arcane Brigade/Heroes/Repair walk poses that flip the hero")]
        public static void Build()
        {
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            var textures = new Dictionary<string, Texture2D>();
            var from = new List<Sprite>(); var to = new List<Sprite>(); var lines = new List<string>();
            foreach (UnitDefinition hero in catalog.Champions)
            {
                // the walk cycle: locomotion frames 2..4 (after two idles), or an authored Walk set
                ExpandedPerformanceEntry gait = ExpandedPerformanceLibrary.Current?.Find("locomotion", hero.Id);
                AuthoredMotionSet set = hero.AuthoredMotion;
                Sprite reference; List<Sprite> frames;
                if (gait != null && gait.Frames.Length >= 5) { reference = gait.Frames[0]; frames = gait.Frames.Skip(2).Take(3).ToList(); }
                else if (set != null && set.Idle != null && set.Idle.Length > 0 && set.Walk != null && set.Walk.Length > 1) { reference = set.Idle[0]; frames = set.Walk.ToList(); }
                else { lines.Add(hero.Id + "\tno walk cycle"); continue; }
                frames = frames.Where(f => f != null).ToList();
                float[] refMask = reference != null ? Mask(reference, textures, false) : null;
                if (refMask == null || frames.Count < 2) { lines.Add(hero.Id + "\tno readable walk cycle"); continue; }
                foreach (Sprite frame in frames)
                {
                    float[] mask = Mask(frame, textures, false), flip = Mask(frame, textures, true);
                    if (mask == null) continue;
                    float same = Score(refMask, mask), turned = Score(refMask, flip);
                    bool odd = turned > same + Margin && frame != frames[0];
                    if (odd) { from.Add(frame); to.Add(frames[0]); }
                    lines.Add($"{hero.Id}\t{frame.name}\tas drawn {same:0.000}\tmirrored {turned:0.000}\t{(odd ? "DISAGREES -> drawn as " + frames[0].name : "ok")}");
                }
            }
            foreach (Texture2D t in textures.Values) Object.DestroyImmediate(t);
            const string path = "Assets/ArcaneBrigade/Resources/HeroPoseRepairs.asset";
            var art = AssetDatabase.LoadAssetAtPath<HeroPoseRepairs>(path);
            if (art == null) { art = ScriptableObject.CreateInstance<HeroPoseRepairs>(); AssetDatabase.CreateAsset(art, path); }
            art.From = from.ToArray(); art.To = to.ToArray(); EditorUtility.SetDirty(art); AssetDatabase.SaveAssets(); HeroPoseRepairs.Invalidate();
            Directory.CreateDirectory("PreviewExports/HeroPoseRepairs");
            File.WriteAllLines("PreviewExports/HeroPoseRepairs/Walk_Cycles.tsv", lines);
            Debug.Log("HERO_POSE_REPAIRS " + from.Count);
        }

        /// <summary>The sprite's silhouette on an N x N grid fitted to its painted ink (so pose scale and margins do not matter).</summary>
        private static float[] Mask(Sprite sprite, Dictionary<string, Texture2D> cache, bool flip)
        {
            string file = AssetDatabase.GetAssetPath(sprite.texture);
            if (!cache.TryGetValue(file, out Texture2D texture))
            {
                texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!texture.LoadImage(File.ReadAllBytes(file))) { Object.DestroyImmediate(texture); return null; }
                cache[file] = texture;
            }
            float rx = (float)texture.width / sprite.texture.width, ry = (float)texture.height / sprite.texture.height;
            Rect r = sprite.rect; int x0 = Mathf.FloorToInt(r.xMin * rx), x1 = Mathf.CeilToInt(r.xMax * rx), y0 = Mathf.FloorToInt(r.yMin * ry), y1 = Mathf.CeilToInt(r.yMax * ry);
            Color32[] px = texture.GetPixels32(); int w = texture.width;
            int left = x1, right = x0, bottom = y1, top = y0;
            for (int y = y0; y < y1; y++) for (int x = x0; x < x1; x++) if (px[y * w + x].a >= 60) { left = Mathf.Min(left, x); right = Mathf.Max(right, x + 1); bottom = Mathf.Min(bottom, y); top = Mathf.Max(top, y + 1); }
            if (right <= left || top <= bottom) return null;
            int span = Mathf.Max(right - left, top - bottom); float cx = (left + right) * .5f, cy = (bottom + top) * .5f;
            var mask = new float[N * N];
            for (int j = 0; j < N; j++) for (int i = 0; i < N; i++)
                {
                    int sx = Mathf.FloorToInt(cx + ((flip ? N - 1 - i : i) + .5f - N * .5f) / N * span), sy = Mathf.FloorToInt(cy + (j + .5f - N * .5f) / N * span);
                    mask[j * N + i] = sx >= x0 && sx < x1 && sy >= y0 && sy < y1 && px[sy * w + sx].a >= 60 ? 1 : 0;
                }
            return mask;
        }
        private static float Score(float[] a, float[] b)
        {
            float both = 0, any = 0;
            for (int i = 0; i < a.Length; i++) { both += a[i] * b[i]; any += Mathf.Max(a[i], b[i]); }
            return any > 0 ? both / any : 0;
        }
    }
}
