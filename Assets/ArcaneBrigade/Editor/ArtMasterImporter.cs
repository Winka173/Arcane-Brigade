using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Imports the approved master-list deliveries and exports exact source sprites for image edits.</summary>
    public static class ArtMasterImporter
    {
        private const string Story = "Assets/ArcaneBrigade/Resources/MienCho/ActOne/";
        public static void Import()
        {
            AssetDatabase.Refresh();
            foreach (string folder in new[] { Story, "Assets/ArcaneBrigade/Art/ActOneBosses/", "Assets/ArcaneBrigade/Art/CombatVFX/" })
            {
                if (!Directory.Exists(folder)) continue;
                foreach (string path in Directory.GetFiles(folder, "*.png", SearchOption.AllDirectories))
                {
                    if (path.EndsWith("_Atlas.png", StringComparison.Ordinal)) continue;
                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer == null) continue;
                    bool floor = Path.GetFileName(path).StartsWith("Floor_", StringComparison.Ordinal);
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spritePixelsPerUnit = 256;
                    importer.alphaIsTransparency = !floor;
                    importer.mipmapEnabled = true;
                    importer.npotScale = TextureImporterNPOTScale.None;
                    importer.maxTextureSize = 2048;
                    importer.wrapMode = floor ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
                    importer.filterMode = FilterMode.Trilinear;
                    importer.textureCompression = TextureImporterCompression.CompressedHQ;
                    importer.SaveAndReimport();
                }
            }
            HeroMechanicArtImporter.Import();
            BuildArtwork();
            BuildCombatArtwork();
            RegisterInkBounds();
            foreach (var entry in AssetDatabase.LoadAssetAtPath<ActOneArtwork>("Assets/ArcaneBrigade/Resources/ActOneArtwork.asset").Entries)
                if (entry.Motion != null) { entry.Motion.IllustratedHeight = SpriteInkMetrics.BoundsOf(entry.Frames[0]).height; EditorUtility.SetDirty(entry.Motion); }
            AssetDatabase.SaveAssets();
            Debug.Log("ART_MASTER_IMPORT complete");
        }
        private static void RegisterInkBounds()
        {
            const string path = "Assets/ArcaneBrigade/Resources/SpriteInkMetrics.asset";
            var metrics = AssetDatabase.LoadAssetAtPath<SpriteInkMetrics>(path);
            if (metrics == null) { metrics = ScriptableObject.CreateInstance<SpriteInkMetrics>(); AssetDatabase.CreateAsset(metrics, path); }
            var entries = metrics.Entries.Where(e => e.Sprite != null).GroupBy(e => e.Sprite).ToDictionary(e => e.Key, e => e.Last().Ink);
            foreach (string folder in new[] { Story, "Assets/ArcaneBrigade/Art/ActOneBosses/", "Assets/ArcaneBrigade/Art/CombatVFX/", HeroMechanicArtImporter.Folder })
            {
                if (!Directory.Exists(folder)) continue;
                foreach (string imagePath in Directory.GetFiles(folder, "*.png", SearchOption.AllDirectories))
                {
                    var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    texture.LoadImage(File.ReadAllBytes(imagePath)); var pixels = texture.GetPixels32();
                    foreach (Sprite sprite in AssetDatabase.LoadAllAssetsAtPath(imagePath).OfType<Sprite>())
                    {
                        // Import downscaling maps textureRect back to the original delivery pixels.
                        Rect rect = sprite.textureRect; float rx = texture.width / (float)sprite.texture.width, ry = texture.height / (float)sprite.texture.height;
                        int left = Mathf.RoundToInt(rect.xMin * rx), right = Mathf.RoundToInt(rect.xMax * rx), bottom = Mathf.RoundToInt(rect.yMin * ry), top = Mathf.RoundToInt(rect.yMax * ry);
                        int minX = right, minY = top, maxX = left, maxY = bottom;
                        for (int y = bottom; y < top; y++) for (int x = left; x < right; x++)
                            if (pixels[y * texture.width + x].a >= 16) { minX = Mathf.Min(minX, x); minY = Mathf.Min(minY, y); maxX = Mathf.Max(maxX, x); maxY = Mathf.Max(maxY, y); }
                        if (maxX < minX || maxY < minY) throw new InvalidOperationException("Empty illustrated sprite " + sprite.name);
                        float ppu = sprite.pixelsPerUnit;
                        entries[sprite] = new Rect(((minX - left) / rx - sprite.pivot.x) / ppu, ((minY - bottom) / ry - sprite.pivot.y) / ppu, (maxX - minX + 1) / rx / ppu, (maxY - minY + 1) / ry / ppu);
                    }
                    UnityEngine.Object.DestroyImmediate(texture);
                }
            }
            metrics.Entries = entries.Select(e => new SpriteInkMetrics.Entry { Sprite = e.Key, Ink = e.Value }).ToArray();
            EditorUtility.SetDirty(metrics); SpriteInkMetrics.Invalidate();
        }
        private static void BuildCombatArtwork()
        {
            const string folder = "Assets/ArcaneBrigade/Art/CombatVFX/", path = "Assets/ArcaneBrigade/Resources/CombatVfxArtwork.asset";
            if (!Directory.Exists(folder)) return;
            var art = AssetDatabase.LoadAssetAtPath<CombatVfxArtwork>(path);
            if (art == null) { art = ScriptableObject.CreateInstance<CombatVfxArtwork>(); AssetDatabase.CreateAsset(art, path); }
            string Group(string file)
            {
                string name = Path.GetFileNameWithoutExtension(file);
                name = System.Text.RegularExpressions.Regex.Replace(name, "_v[0-9]+$", "");
                name = System.Text.RegularExpressions.Regex.Replace(name, "_f[0-9]+$", "");
                name = System.Text.RegularExpressions.Regex.Replace(name, "^(Mutation_[0-7])_[ab]$", "$1");
                return name;
            }
            art.Entries = Directory.GetFiles(folder, "*.png", SearchOption.AllDirectories).GroupBy(Group).Select(group => new CombatVfxEntry
            { Name = group.Key, Frames = group.OrderBy(x => x, StringComparer.Ordinal).Select(AssetDatabase.LoadAssetAtPath<Sprite>).ToArray() }).ToArray();
            EditorUtility.SetDirty(art);
        }
        private static void BuildArtwork()
        {
            const string assetPath = "Assets/ArcaneBrigade/Resources/ActOneArtwork.asset";
            var art = AssetDatabase.LoadAssetAtPath<ActOneArtwork>(assetPath);
            if (art == null) { art = ScriptableObject.CreateInstance<ActOneArtwork>(); AssetDatabase.CreateAsset(art, assetPath); }
            var entries = new System.Collections.Generic.List<ActOneArtEntry>();
            string[] folders = { Story, "Assets/ArcaneBrigade/Art/ActOneBosses/" };
            foreach (string folder in folders)
            {
                if (!Directory.Exists(folder)) continue;
                foreach (string path in Directory.GetFiles(folder, "*.png", SearchOption.TopDirectoryOnly))
                {
                    string name = Path.GetFileNameWithoutExtension(path);
                    if (name.EndsWith("_Atlas", StringComparison.Ordinal)) continue;
                    if (System.Text.RegularExpressions.Regex.IsMatch(name, "_f[0-9]+$")) continue;
                    var frames = new System.Collections.Generic.List<Sprite> { AssetDatabase.LoadAssetAtPath<Sprite>(path) };
                    for (int i = 1; i < 32; i++)
                    {
                        var frame = AssetDatabase.LoadAssetAtPath<Sprite>(folder + name + "_f" + i + ".png");
                        if (frame == null) break;
                        frames.Add(frame);
                    }
                    string atlas = folder + name + "_Atlas.png";
                    if (File.Exists(atlas) && frames.Count < 16)
                    {
                        int rows = folder.Contains("ActOneBosses") || name.StartsWith("LastCustomer_", StringComparison.Ordinal) ? 4 : 1;
                        // A few authored hands cross a mathematical grid line while remaining separate silhouettes.
                        // Slice connected ink, not the grid, so chains/fingers are not amputated by an equal-cell crop.
                        frames = K25AtlasImporter.Import(atlas, name, 4, rows, Enumerable.Range(0, 4 * rows).ToArray(),
                            Enumerable.Range(0, rows + 1).Select(i => i * 1024 / rows).ToArray(), componentRows: true).ToList();
                    }
                    var entry = new ActOneArtEntry { Name = name, Frames = frames.ToArray() };
                    bool actor = new[] { "Hob", "Latch", "Postscript", "Tally", "ChairPorter", "DoorThatStayedShut", "TheBedside", "LastCustomer_Pleasant", "LastCustomer_BorrowedFace" }.Contains(name);
                    if (actor)
                    {
                        string motionPath = "Assets/ArcaneBrigade/Resources/MienCho/ActOne/" + name + "Motion.asset";
                        var motion = AssetDatabase.LoadAssetAtPath<AuthoredMotionSet>(motionPath);
                        if (motion == null) { motion = ScriptableObject.CreateInstance<AuthoredMotionSet>(); AssetDatabase.CreateAsset(motion, motionPath); }
                        Sprite At(int i) => frames[Mathf.Min(i, frames.Count - 1)];
                        motion.Idle = new[] { At(0), At(1) }; motion.Walk = new[] { At(2), At(3), At(4) };
                        motion.Windup = new[] { At(frames.Count == 4 ? 1 : 5), At(frames.Count == 4 ? 1 : 6) };
                        motion.Release = new[] { At(frames.Count == 4 ? 2 : 7), At(frames.Count == 4 ? 3 : 8) };
                        motion.Recovery = new[] { At(frames.Count == 4 ? 1 : 9), At(frames.Count == 4 ? 0 : 10) };
                        entry.Motion = motion; EditorUtility.SetDirty(motion);
                    }
                    entries.Add(entry);
                }
            }
            art.Entries = entries.ToArray(); EditorUtility.SetDirty(art);
            var merchant = entries.Where(e => e.Name.StartsWith("Shopkeeper_", StringComparison.Ordinal)).ToDictionary(e => e.Name);
            if (new[] { "Idle", "Pour", "Wipe", "Warn" }.All(n => merchant.ContainsKey("Shopkeeper_" + n)))
            {
                var library = Resources.Load<PaintedAnimationLibrary>("PaintedAnimationLibrary");
                library.GiantGoldMerchant = new[] { "Idle", "Pour", "Wipe", "Warn" }.Select(n => merchant["Shopkeeper_" + n].Frames[0]).ToArray();
                EditorUtility.SetDirty(library);
            }
        }
        public static void ExportReferences()
        {
            Directory.CreateDirectory(".utmp/ArtRefs");
            var catalog = Resources.Load<RunCatalog>("RunCatalog");
            string[] ids = { "TeapotPilgrim", "HoodWisp", "PaperDevil" };
            string[] names = { "Hob", "Latch", "Tally" };
            for (int i = 0; i < ids.Length; i++)
            {
                var unit = catalog.Unit(ids[i]);
                Export(unit.Body, names[i]);
                for (int f = 0; f < unit.AnimationFrames.Length; f++) Export(unit.AnimationFrames[f], names[i] + "_f" + f);
            }
            var nightmare = NightmareAssets.Current;
            Export(nightmare.Customer.Body, "LastCustomer");
            for (int f = 0; f < nightmare.CustomerFrames.Length; f++) Export(nightmare.CustomerFrames[f], "LastCustomer_f" + f);
            Debug.Log("ART_MASTER_REFERENCES exported exact source pixels");
        }
        private static void Export(Sprite sprite, string name)
        {
            if (sprite == null) throw new InvalidOperationException("Missing reference " + name);
            var previous = RenderTexture.active;
            var target = RenderTexture.GetTemporary(sprite.texture.width, sprite.texture.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(sprite.texture, target);
            RenderTexture.active = target;
            Rect rectangle = sprite.textureRect;
            var pixels = new Texture2D((int)rectangle.width, (int)rectangle.height, TextureFormat.RGBA32, false);
            pixels.ReadPixels(rectangle, 0, 0); pixels.Apply();
            File.WriteAllBytes(".utmp/ArtRefs/" + name + ".png", pixels.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(pixels);
            RenderTexture.active = previous; RenderTexture.ReleaseTemporary(target);
        }
    }
}
