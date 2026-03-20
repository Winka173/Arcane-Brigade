using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Shaped hero gauges (Tools/Art/hero_gauges_v3.json, approved 18/10/2026): from each Gauge2 Empty/Full pair, the
    /// pixels that differ are the fill area; each gets the moment it shows (0..1) in the hero's own order - a sweep, a bloom from a
    /// point, a spiral, piece after piece, an hourglass that drains above while it fills below. Ranked, so the fill line moves
    /// through equal areas at an even pace. Baked to Art/Arsenal/Generated/Gauge2_Hero_Order.png (R = order, G = drains).</summary>
    public static class GaugeOrderBaker
    {
        private const string Art = "Assets/ArcaneBrigade/Art/Arsenal/", Generated = "Assets/ArcaneBrigade/Art/Arsenal/Generated/";
        [Serializable] private sealed class Shape { public string mode; }

        public static List<HeroGaugeArtwork.Order> Bake(List<string> log)
        {
            var orders = new List<HeroGaugeArtwork.Order>();
            string json = File.Exists("Tools/Art/hero_gauges_v3.json") ? File.ReadAllText("Tools/Art/hero_gauges_v3.json") : null;
            if (json == null) return orders;
            Directory.CreateDirectory(Generated);
            foreach (string hero in HeroIds(json))
            {
                string empty = Art + "Gauge2_" + hero + "_Empty_v1.png", full = Art + "Gauge2_" + hero + "_Full_v1.png";
                if (!File.Exists(empty) || !File.Exists(full)) continue;
                string mode = ModeOf(json, hero);
                var a = new Texture2D(2, 2, TextureFormat.RGBA32, false); var b = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                try
                {
                    if (!a.LoadImage(File.ReadAllBytes(empty)) || !b.LoadImage(File.ReadAllBytes(full)) || a.width != b.width || a.height != b.height) { log.Add("GAUGE ORDER " + hero + ": pair mismatch"); continue; }
                    Color32[] map = BakeOne(a.GetPixels32(), b.GetPixels32(), a.width, a.height, mode, out string note);
                    var output = new Texture2D(a.width, a.height, TextureFormat.RGBA32, false, true); output.SetPixels32(map); output.Apply();
                    string path = Generated + "Gauge2_" + hero + "_Order.png";
                    File.WriteAllBytes(path, output.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(output);
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                    if (AssetImporter.GetAtPath(path) is TextureImporter importer)
                    {
                        importer.textureType = TextureImporterType.Default; importer.sRGBTexture = false; importer.mipmapEnabled = false; importer.alphaSource = TextureImporterAlphaSource.FromInput;
                        importer.textureCompression = TextureImporterCompression.Uncompressed; importer.filterMode = FilterMode.Point; importer.wrapMode = TextureWrapMode.Clamp; importer.maxTextureSize = 2048; importer.SaveAndReimport();
                    }
                    orders.Add(new HeroGaugeArtwork.Order { Hero = "Gauge2_" + hero, Map = AssetDatabase.LoadAssetAtPath<Texture2D>(path) });
                    log.Add("GAUGE ORDER " + hero + " " + mode + " " + note);
                }
                finally { UnityEngine.Object.DestroyImmediate(a); UnityEngine.Object.DestroyImmediate(b); }
            }
            return orders;
        }
        private static IEnumerable<string> HeroIds(string json)
        {
            foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(json, "\"(\\w+)\": \\{")) yield return m.Groups[1].Value;
        }
        private static string ModeOf(string json, string hero)
        {
            int at = json.IndexOf("\"" + hero + "\": {", StringComparison.Ordinal);
            var m = System.Text.RegularExpressions.Regex.Match(json.Substring(at), "\"mode\": \"([^\"]+)\"");
            return m.Success ? m.Groups[1].Value : "rise";
        }

        /// <summary>The order map for one pair (public for checks).</summary>
        public static Color32[] BakeOne(Color32[] empty, Color32[] full, int w, int h, string mode, out string note)
        {
            int n = w * h; var mask = new bool[n]; int count = 0;
            for (int i = 0; i < n; i++)
            {
                Color32 e = empty[i], f = full[i];
                int diff = Mathf.Abs(e.r - f.r) + Mathf.Abs(e.g - f.g) + Mathf.Abs(e.b - f.b) + Mathf.Abs(e.a - f.a) * 2;
                if (f.a > 16 && diff > 60) { mask[i] = true; count++; }
            }
            var raw = new float[n]; var invert = new bool[n]; note = count + " px";
            var map = new Color32[n];
            if (count == 0) return map;
            float cx = 0, cy = 0, minY = h, maxY = 0, minX = w, maxX = 0;
            for (int i = 0; i < n; i++) if (mask[i]) { int x = i % w, y = i / w; cx += x; cy += y; minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y); minX = Mathf.Min(minX, x); maxX = Mathf.Max(maxX, x); }
            cx /= count; cy /= count;
            string[] part = mode.Split(':'); string kind = part[0];
            float P(int k, float fallback) => part.Length > 1 && part[1].Split(',').Length > k && float.TryParse(part[1].Split(',')[k], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float v) ? v : fallback;
            float Angle(int x, int y, float startDeg)
            {
                // clockwise from startDeg (90 = twelve o'clock), 0..360
                float a = Mathf.Atan2(y - cy, x - cx) * Mathf.Rad2Deg;
                return Mathf.Repeat(startDeg - a, 360);
            }
            if (kind == "pieces")
            {
                int want = (int)P(0, 3); string key = part.Length > 2 ? part[2] : "y";
                int[] label = Components(mask, w, h, out List<(float x, float y, int size)> parts);
                var order = Enumerable.Range(0, parts.Count).ToList();
                // the pieces in reading order of their kind
                order.Sort((p, q) =>
                {
                    float a = key == "x" ? parts[p].x : key == "y" ? parts[p].y : key == "angle" ? Mathf.Repeat(90 - Mathf.Atan2(parts[p].y - cy, parts[p].x - cx) * Mathf.Rad2Deg, 360) : -parts[p].y * 10000 + parts[p].x;
                    float c = key == "x" ? parts[q].x : key == "y" ? parts[q].y : key == "angle" ? Mathf.Repeat(90 - Mathf.Atan2(parts[q].y - cy, parts[q].x - cx) * Mathf.Rad2Deg, 360) : -parts[q].y * 10000 + parts[q].x;
                    if (key == "rows" || key == "dice") { float rowP = Mathf.Round(parts[p].y / (h * .08f)), rowQ = Mathf.Round(parts[q].y / (h * .08f)); if (rowP != rowQ) return rowQ.CompareTo(rowP); return parts[p].x.CompareTo(parts[q].x); }
                    return a.CompareTo(c);
                });
                var rankOf = new int[parts.Count]; for (int r = 0; r < order.Count; r++) rankOf[order[r]] = r;
                int pieces = parts.Count;
                note += ", " + pieces + " pieces (want " + want + ")";
                // inside a piece it rises; the piece's slot in the gauge is its rank
                var lowY = new float[pieces]; var highY = new float[pieces];
                for (int k = 0; k < pieces; k++) { lowY[k] = h; highY[k] = 0; }
                for (int i = 0; i < n; i++) if (mask[i]) { int k = label[i]; lowY[k] = Mathf.Min(lowY[k], i / w); highY[k] = Mathf.Max(highY[k], i / w); }
                for (int i = 0; i < n; i++) if (mask[i]) { int k = label[i]; float local = (i / w - lowY[k]) / Mathf.Max(1, highY[k] - lowY[k]); raw[i] = (rankOf[k] + Mathf.Clamp(local, .02f, .98f)) / pieces; }
                Write(map, mask, raw, invert, n, false);
                return map;
            }
            for (int i = 0; i < n; i++)
            {
                if (!mask[i]) continue;
                int x = i % w, y = i / w; float u = x / (float)w, v = y / (float)h;
                switch (kind)
                {
                    case "fall": raw[i] = -y; break;
                    case "sweep": raw[i] = x; break;
                    case "diagonal": { Vector2 a = new Vector2(P(0, 0), P(1, 0)), b = new Vector2(P(2, 1), P(3, 1)); raw[i] = Vector2.Dot(new Vector2(u, v) - a, (b - a).normalized); break; }
                    case "clock": raw[i] = Angle(x, y, P(0, 90)); break;
                    case "bloom": raw[i] = (new Vector2(u * w, v * h) - new Vector2(P(0, .5f) * w, P(1, .5f) * h)).sqrMagnitude; break;
                    case "iris": raw[i] = -((x - cx) * (x - cx) + (y - cy) * (y - cy)); break;
                    case "curtain": raw[i] = Mathf.Abs(x - cx); break;
                    case "spiral": { float r = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy)) / Mathf.Max(1, Mathf.Max(maxX - minX, maxY - minY) * .5f); raw[i] = r * 3 + Angle(x, y, 90) / 360f; break; }
                    case "wave": raw[i] = y + Mathf.Sin(x * .05f) * h * .015f; break;
                    case "crescent": raw[i] = (new Vector2(x, y) - new Vector2(P(0, .15f) * w, P(1, .5f) * h)).sqrMagnitude; break;
                    case "rain": raw[i] = -y + (Mathf.Abs(Mathf.Sin(Mathf.Floor(x / 10f) * 12.9898f) * 43758.5453f) % 1) * h * .12f; break;
                    case "hourglass":
                    case "melt":
                        // the upper part drains (the sand surface sinks, the scoop melts top down), the lower part fills from the bottom
                        if (y > cy) { invert[i] = true; raw[i] = -y; } else raw[i] = y;
                        break;
                    default: raw[i] = y; break;
                }
            }
            if (kind == "hourglass" || kind == "melt")
            {
                // rank each part on its own, so the upper one empties as the lower one fills
                Rank(raw, mask, invert, true, n); Rank(raw, mask, invert, false, n);
                for (int i = 0; i < n; i++) if (mask[i]) map[i] = new Color32((byte)Mathf.Clamp(Mathf.RoundToInt(raw[i] * 255), 1, 255), (byte)(invert[i] ? 255 : 0), 0, 255);
                return map;
            }
            Write(map, mask, raw, invert, n, true);
            return map;
        }
        private static void Rank(float[] raw, bool[] mask, bool[] invert, bool upper, int n)
        {
            var idx = new List<int>(); for (int i = 0; i < n; i++) if (mask[i] && invert[i] == upper) idx.Add(i);
            idx.Sort((p, q) => raw[p].CompareTo(raw[q]));
            for (int r = 0; r < idx.Count; r++) raw[idx[r]] = (r + 1f) / idx.Count;
        }
        private static void Write(Color32[] map, bool[] mask, float[] raw, bool[] invert, int n, bool rank)
        {
            if (rank) Rank(raw, mask, invert, false, n);
            for (int i = 0; i < n; i++) if (mask[i]) map[i] = new Color32((byte)Mathf.Clamp(Mathf.RoundToInt(raw[i] * 255), 1, 255), (byte)(invert[i] ? 255 : 0), 0, 255);
        }
        /// <summary>Connected pieces of the fill area (crumbs under 0.4% join the nearest real piece).</summary>
        private static int[] Components(bool[] mask, int w, int h, out List<(float x, float y, int size)> parts)
        {
            int n = w * h; var label = new int[n]; for (int i = 0; i < n; i++) label[i] = -1;
            var raw = new List<(float x, float y, int size)>(); var stack = new Stack<int>();
            for (int s = 0; s < n; s++)
            {
                if (!mask[s] || label[s] >= 0) continue;
                int id = raw.Count; long sx = 0, sy = 0; int size = 0; stack.Push(s); label[s] = id;
                while (stack.Count > 0)
                {
                    int i = stack.Pop(); int x = i % w, y = i / w; sx += x; sy += y; size++;
                    void Visit(int j) { if (mask[j] && label[j] < 0) { label[j] = id; stack.Push(j); } }
                    if (x > 0) Visit(i - 1); if (x < w - 1) Visit(i + 1); if (y > 0) Visit(i - w); if (y < h - 1) Visit(i + w);
                }
                raw.Add((sx / (float)size, sy / (float)size, size));
            }
            int total = raw.Sum(p => p.size);
            var big = Enumerable.Range(0, raw.Count).Where(k => raw[k].size >= total * .004f).ToList();
            if (big.Count == 0) big.Add(0);
            var remap = new int[raw.Count];
            for (int k = 0; k < raw.Count; k++)
            {
                int best = big[0]; float bd = float.MaxValue;
                foreach (int b in big) { float d = (raw[b].x - raw[k].x) * (raw[b].x - raw[k].x) + (raw[b].y - raw[k].y) * (raw[b].y - raw[k].y); if (d < bd) { bd = d; best = b; } }
                remap[k] = big.IndexOf(best);
            }
            parts = big.Select(k => raw[k]).ToList();
            for (int i = 0; i < n; i++) if (label[i] >= 0) label[i] = remap[label[i]];
            return label;
        }
    }
}
