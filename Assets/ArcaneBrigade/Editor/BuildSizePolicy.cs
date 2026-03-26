using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Build-size import policy (Docs/Build_Size_Proposal.md, approved 19/10/2026). Runs after every other texture
    /// postprocessor, so a builder or importer that forces Uncompressed / 4096 / mipmaps can no longer undo it:
    /// - Standalone BC7 at the painting's own size, Android and iOS ASTC 6x6 at the phone size (per-texture caps live in the
    ///   texture's userData "BuildSize:std=..;mob=..;mips=..;w=..;h=..;ppu=..[;piv=x,y]", written when the PNG was resized).
    /// - A resized painting keeps its world size: its pixels-per-unit (and pivot, when the canvas was padded) are enforced
    ///   while the PNG still has the size recorded in userData, even if a builder resets them to 256.
    /// - Unity only block-compresses a non-power-of-two texture without mipmaps, so mipmaps are dropped on NPOT art.
    /// - No sprite physics shapes (nothing in the game uses sprite colliders).
    /// - Gauge order maps (Art/Arsenal/Generated/Gauge2_*_Order) are data for GaugeReveal: linear, point, no mipmaps, RG16.
    /// A texture a builder has made readable keeps its plain import while the builder reads it; the build clears readable.
    /// Phase 2 (20/10/2026): soft art ("soft=1": rings, flashes, puffs, floors, backdrops) is ASTC 8x8 on phones; sheets
    /// repacked without empty canvas carry "packed=<crc of their rects>" and slicers leave them alone (IsPacked); music
    /// is Vorbis 0.45 on Android/iOS only (PC audio is untouched).</summary>
    public sealed class BuildSizePolicy : AssetPostprocessor, IPreprocessBuildWithReport
    {
        public const uint PolicyVersion = 2;
        public override uint GetVersion() => PolicyVersion;
        public override int GetPostprocessOrder() => 1000;
        public int callbackOrder => 0;

        public sealed class Entry
        {
            public int Std, Mob, W, H; public bool Mips, Npot, Soft; public string Packed; public float Ppu; public Vector2? Pivot;
            public static Entry Parse(string userData)
            {
                if (string.IsNullOrEmpty(userData) || !userData.StartsWith("BuildSize:", StringComparison.Ordinal)) return null;
                var e = new Entry();
                foreach (string pair in userData.Substring(10).Split(';'))
                {
                    string[] kv = pair.Split('='); if (kv.Length != 2) continue;
                    float F(string s) => float.Parse(s, CultureInfo.InvariantCulture);
                    switch (kv[0])
                    {
                        case "std": e.Std = int.Parse(kv[1]); break;
                        case "mob": e.Mob = int.Parse(kv[1]); break;
                        case "w": e.W = int.Parse(kv[1]); break;
                        case "h": e.H = int.Parse(kv[1]); break;
                        case "mips": e.Mips = kv[1] == "1"; break;
                        case "npot": e.Npot = kv[1] == "1"; break;
                        case "soft": e.Soft = kv[1] == "1"; break;
                        case "packed": e.Packed = kv[1]; break;
                        case "ppu": e.Ppu = F(kv[1]); break;
                        case "piv": string[] p = kv[1].Split(','); e.Pivot = new Vector2(F(p[0]), F(p[1])); break;
                    }
                }
                return e.Std > 0 ? e : null;
            }
        }

        public static bool IsOrderMap(string path) => path.Contains("/Art/Arsenal/Generated/") && path.EndsWith("_Order.png", StringComparison.Ordinal);
        private static bool IsPot(int v) => v > 0 && (v & (v - 1)) == 0;
        public static int PotCeil(int v) { int p = 1; while (p < v) p *= 2; return p; }

        public static bool SourceSize(string path, TextureImporter importer, out int w, out int h)
        {
            w = h = 0;
            try
            {
                if (path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) && File.Exists(path))
                    using (var f = File.OpenRead(path))
                    {
                        var b = new byte[24];
                        if (f.Read(b, 0, 24) == 24 && b[1] == 'P' && b[2] == 'N' && b[3] == 'G')
                        {
                            w = b[16] << 24 | b[17] << 16 | b[18] << 8 | b[19]; h = b[20] << 24 | b[21] << 16 | b[22] << 8 | b[23];
                            return true;
                        }
                    }
                importer.GetSourceTextureWidthAndHeight(out w, out h);
                return w > 0 && h > 0;
            }
            catch { return false; }
        }

        private void OnPreprocessTexture()
        {
            string path = assetPath.Replace('\\', '/');
            if (!path.StartsWith("Assets/ArcaneBrigade/", StringComparison.Ordinal)) return;
            var importer = (TextureImporter)assetImporter;
            if (!SourceSize(path, importer, out int w, out int h)) return;
            int longest = Mathf.Max(w, h);
            if (IsOrderMap(path))
            {
                importer.textureType = TextureImporterType.Default; importer.sRGBTexture = false; importer.mipmapEnabled = false;
                importer.alphaSource = TextureImporterAlphaSource.None; importer.filterMode = FilterMode.Point; importer.wrapMode = TextureWrapMode.Clamp;
                importer.textureCompression = TextureImporterCompression.Uncompressed; importer.isReadable = false;
                Platforms(importer, PotCeil(longest), PotCeil(longest), TextureImporterFormat.RG16, TextureImporterFormat.RG16);
                return;
            }
            Entry entry = Entry.Parse(importer.userData);
            bool current = entry != null && entry.W == w && entry.H == h;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            if (current && importer.textureType == TextureImporterType.Sprite)
            {
                settings.spritePixelsPerUnit = entry.Ppu;
                if (entry.Pivot.HasValue) { settings.spriteAlignment = (int)SpriteAlignment.Custom; settings.spritePivot = entry.Pivot.Value; }
            }
            settings.spriteGenerateFallbackPhysicsShape = false;
            bool mips = current ? entry.Mips : settings.mipmapEnabled;
            // a tiled plain texture (floors) keeps its mipmaps: Unity scales it up to a power of two, which it can compress
            if (current && entry.Npot && importer.textureType != TextureImporterType.Sprite) { settings.mipmapEnabled = true; if (settings.npotScale == TextureImporterNPOTScale.None) settings.npotScale = TextureImporterNPOTScale.ToLarger; }
            else if (importer.textureType != TextureImporterType.Sprite && settings.npotScale != TextureImporterNPOTScale.None) settings.mipmapEnabled = mips;
            else settings.mipmapEnabled = mips && IsPot(w) && IsPot(h);
            importer.SetTextureSettings(settings);
            if (importer.isReadable) return; // a builder is reading this painting's pixels right now; the build clears readable
            int std = current ? entry.Std : Mathf.Min(PotCeil(longest), Mathf.Max(32, importer.maxTextureSize));
            int mob = current ? entry.Mob : Mathf.Max(32, Mathf.Min(std, PotCeil(Mathf.RoundToInt(longest * 0.6f))));
            Platforms(importer, std, mob, TextureImporterFormat.BC7, current && entry.Soft ? TextureImporterFormat.ASTC_8x8 : TextureImporterFormat.ASTC_6x6);
        }

        /// <summary>True while a repacked sheet still has the size it was packed at: its cells no longer sit on the painted grid,
        /// so a builder must keep its sprites instead of slicing the sheet again (a repainted sheet has another size).</summary>
        public static bool IsPacked(string path)
        {
            if (!(AssetImporter.GetAtPath(path) is TextureImporter importer)) return false;
            Entry entry = Entry.Parse(importer.userData);
            return entry != null && !string.IsNullOrEmpty(entry.Packed) && SourceSize(path, importer, out int w, out int h) && w == entry.W && h == entry.H;
        }
        public static Sprite[] PackedSprites(string path) => AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToArray();

        private static bool IsMusic(string path) => path.Contains("/Resources/Audio/") && (path.Contains("/Music/") || path.Contains("/Licensed/") || path.Count(c => c == '/') == 4);
        private void OnPreprocessAudio()
        {
            string path = assetPath.Replace('\\', '/');
            if (!path.StartsWith("Assets/ArcaneBrigade/", StringComparison.Ordinal) || !IsMusic(path)) return;
            var importer = (AudioImporter)assetImporter;
            foreach (string platform in new[] { "Android", "iOS" })
            {
                AudioImporterSampleSettings s = importer.defaultSampleSettings;
                s.compressionFormat = AudioCompressionFormat.Vorbis; s.quality = Mathf.Min(s.quality, 0.45f);
                importer.SetOverrideSampleSettings(platform, s);
            }
        }

        public static void Platforms(TextureImporter importer, int std, int mob, TextureImporterFormat pc, TextureImporterFormat phone)
        {
            void Set(string name, int max, TextureImporterFormat format)
            {
                var s = importer.GetPlatformTextureSettings(name);
                s.name = name; s.overridden = true; s.maxTextureSize = max; s.format = format; s.compressionQuality = 50;
                s.textureCompression = TextureImporterCompression.CompressedHQ; s.crunchedCompression = false;
                importer.SetPlatformTextureSettings(s);
            }
            Set("Standalone", std, pc); Set("Android", mob, phone); Set("iPhone", mob, phone);
        }

        /// <summary>Builders make paintings readable to slice them; nothing reads pixels in the game, so the player never
        /// ships a readable texture (it would sit uncompressed in resources.assets and use twice the memory).</summary>
        public void OnPreprocessBuild(BuildReport report) => ClearReadable();
        public static int ClearReadable()
        {
            int count = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/ArcaneBrigade" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetImporter.GetAtPath(path) is TextureImporter importer && importer.isReadable) { importer.isReadable = false; importer.SaveAndReimport(); count++; }
            }
            if (count > 0) Debug.Log("BUILD_SIZE_POLICY readable cleared on " + count + " textures");
            return count;
        }

        /// <summary>Phase 2 follow-ups after the sheets were repacked and the outlines lightened (scratchpad repack.py):
        /// ink boxes checked (they are relative to each sprite's pivot, so they stay), stat-icon pixel rects measured again.</summary>
        public static void ApplyPhase2()
        {
            var log = new List<string>();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            log.Add("readable cleared: " + ClearReadable());
            log.Add(RebakeInkMetrics());
            log.Add(RemeasureStatIcons());
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory("PreviewExports/BuildSize");
            File.WriteAllLines("PreviewExports/BuildSize/ApplyPhase2.txt", log);
            Debug.Log("BUILD_SIZE_PHASE2_DONE\n" + string.Join("\n", log));
            BuildSizeReport.Run();
        }

        /// <summary>Phase 1 follow-ups after the PNGs were resized (Tools: scratchpad resample.py): order maps rebaked at the new
        /// gauge size, old gauge entries and dead references dropped, ink boxes checked, stat-icon pixel rects measured again.</summary>
        public static void ApplyPhase1()
        {
            var log = new List<string>();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            log.Add("readable cleared: " + ClearReadable());

            var gauges = AssetDatabase.LoadAssetAtPath<HeroGaugeArtwork>("Assets/ArcaneBrigade/Resources/HeroGaugeArtwork.asset");
            gauges.Orders = GaugeOrderBaker.Bake(log).ToArray(); EditorUtility.SetDirty(gauges);

            var arsenal = AssetDatabase.LoadAssetAtPath<ArsenalArtwork>("Assets/ArcaneBrigade/Resources/ArsenalArtwork.asset");
            int before = arsenal.Entries.Length;
            arsenal.Entries = arsenal.Entries.Where(e => e != null && !e.Name.StartsWith("Gauge_", StringComparison.Ordinal) && e.Frames != null && e.Frames.Any(f => f != null)).ToArray();
            EditorUtility.SetDirty(arsenal); log.Add("ArsenalArtwork entries " + before + " -> " + arsenal.Entries.Length);

            log.Add(RebakeInkMetrics());
            log.Add(RemeasureStatIcons());
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory("PreviewExports/BuildSize");
            File.WriteAllLines("PreviewExports/BuildSize/ApplyPhase1.txt", log);
            Debug.Log("BUILD_SIZE_PHASE1_DONE\n" + string.Join("\n", log));
            BuildSizeReport.Run();
        }

        /// <summary>Same measurement as SpriteInkMetricsBuilder.Build, for every entry already recorded (all builders merge into
        /// this asset), dropping entries whose painting was archived.</summary>
        public static string RebakeInkMetrics()
        {
            const string path = "Assets/ArcaneBrigade/Resources/SpriteInkMetrics.asset";
            var data = AssetDatabase.LoadAssetAtPath<SpriteInkMetrics>(path);
            int before = data.Entries.Length, changed = 0;
            var entries = new List<SpriteInkMetrics.Entry>(); var stale = new List<string>();
            foreach (var group in data.Entries.Where(e => e.Sprite != null).GroupBy(e => AssetDatabase.GetAssetPath(e.Sprite.texture)))
            {
                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                try
                {
                    if (!texture.LoadImage(File.ReadAllBytes(group.Key))) throw new InvalidOperationException("Cannot read sprite ink " + group.Key);
                    Color32[] pixels = texture.GetPixels32();
                    foreach (var old in group)
                    {
                        Sprite sprite = old.Sprite; Rect cell = sprite.rect;
                        float ratioX = (float)texture.width / sprite.texture.width, ratioY = (float)texture.height / sprite.texture.height;
                        int left = Mathf.CeilToInt(cell.xMax * ratioX), bottom = Mathf.CeilToInt(cell.yMax * ratioY), right = Mathf.FloorToInt(cell.xMin * ratioX), top = Mathf.FloorToInt(cell.yMin * ratioY);
                        for (int y = Mathf.FloorToInt(cell.yMin * ratioY); y < Mathf.Min(texture.height, Mathf.CeilToInt(cell.yMax * ratioY)); y++)
                            for (int x = Mathf.FloorToInt(cell.xMin * ratioX); x < Mathf.Min(texture.width, Mathf.CeilToInt(cell.xMax * ratioX)); x++)
                                if (pixels[y * texture.width + x].a >= 40) { left = Mathf.Min(left, x); right = Mathf.Max(right, x + 1); bottom = Mathf.Min(bottom, y); top = Mathf.Max(top, y + 1); }
                        if (right <= left || top <= bottom) { entries.Add(old); continue; }
                        float scale = sprite.pixelsPerUnit;
                        var ink = new Rect((left / ratioX - cell.x - sprite.pivot.x) / scale, (bottom / ratioY - cell.y - sprite.pivot.y) / scale, (right - left) / ratioX / scale, (top - bottom) / ratioY / scale);
                        // The resize keeps every ink box in world units (checked against the original PNGs to <1.5%), so the
                        // recorded boxes stay as they are: some were measured on older paintings and differ from today's
                        // pixels, and replacing them would move art the owner has already approved. They are listed instead.
                        float size = Mathf.Max(0.001f, Mathf.Max(old.Ink.width, old.Ink.height));
                        float drift = Mathf.Max(Mathf.Max(Mathf.Abs(ink.x - old.Ink.x), Mathf.Abs(ink.y - old.Ink.y)), Mathf.Max(Mathf.Abs(ink.width - old.Ink.width), Mathf.Abs(ink.height - old.Ink.height))) / size;
                        if (drift > 0.05f) { changed++; stale.Add(drift.ToString("0.00", CultureInfo.InvariantCulture) + " " + AssetDatabase.GetAssetPath(sprite) + " " + sprite.name + " recorded " + old.Ink + " painted " + ink); }
                        entries.Add(old);
                    }
                }
                finally { UnityEngine.Object.DestroyImmediate(texture); }
            }
            data.Entries = entries.OrderBy(e => e.Sprite.name).ThenBy(e => AssetDatabase.GetAssetPath(e.Sprite)).ToArray();
            EditorUtility.SetDirty(data); SpriteInkMetrics.Invalidate();
            Directory.CreateDirectory("PreviewExports/BuildSize");
            File.WriteAllLines("PreviewExports/BuildSize/InkMetrics_Stale.txt", stale.OrderByDescending(s => s));
            return "SpriteInkMetrics entries " + before + " -> " + data.Entries.Length + " (kept as recorded; " + changed + " recorded boxes differ >5% from today's paintings, see InkMetrics_Stale.txt)";
        }

        /// <summary>PaintedStatIcons keeps ink rects in texture pixels (relative to the sprite's textureRect): measured again
        /// the way PaintedStatIconBuilder.Ink does (alpha >= 40, 2 px margin), from the PNG, which needs no GPU in batch mode.</summary>
        public static string RemeasureStatIcons()
        {
            var icons = AssetDatabase.LoadAssetAtPath<PaintedStatIcons>("Assets/ArcaneBrigade/Resources/PaintedStatIcons.asset");
            int n = 0;
            foreach (PaintedStatIcon icon in icons.Entries)
            {
                if (icon.Picture != null) { icon.Ink = Ink(icon.Picture); n++; }
                if (icon.Badge != null) icon.BadgeInk = Ink(icon.Badge);
            }
            EditorUtility.SetDirty(icons);
            return "PaintedStatIcons ink rects measured again: " + n;
        }
        private static Rect Ink(Sprite sprite)
        {
            var png = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                png.LoadImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(sprite.texture)));
                float rx = (float)png.width / sprite.texture.width, ry = (float)png.height / sprite.texture.height;
                Rect rect = sprite.textureRect; Color32[] pixels = png.GetPixels32();
                int left = (int)rect.width, bottom = (int)rect.height, right = -1, top = -1;
                for (int y = 0; y < (int)rect.height; y++) for (int x = 0; x < (int)rect.width; x++)
                {
                    int px = Mathf.Min(png.width - 1, Mathf.FloorToInt(((int)rect.x + x) * rx)), py = Mathf.Min(png.height - 1, Mathf.FloorToInt(((int)rect.y + y) * ry));
                    if (pixels[py * png.width + px].a >= 40) { left = Mathf.Min(left, x); bottom = Mathf.Min(bottom, y); right = Mathf.Max(right, x); top = Mathf.Max(top, y); }
                }
                if (right < left) throw new InvalidOperationException("Empty painted sprite " + sprite.name);
                return Rect.MinMaxRect(Mathf.Max(0, left - 2), Mathf.Max(0, bottom - 2), Mathf.Min(rect.width, right + 3), Mathf.Min(rect.height, top + 3));
            }
            finally { UnityEngine.Object.DestroyImmediate(png); }
        }
    }
}
