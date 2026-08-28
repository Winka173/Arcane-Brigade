using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Builds Resources/RulesVfxLibrary.asset: the painted effect art for the rules effects. New paintings in
    /// Art/RulesVFX (&lt;Look&gt;_Burst_f0..f3, &lt;Look&gt;_Ground, &lt;Look&gt;_Bits as a 4x2 sheet, Shield_f0..f3, Heal/Snow/Rain_Bits,
    /// Danger_Ground, Danger_Burst_f0..f3) win; until they exist each key falls back to the game's existing CombatVFX paintings.
    /// Run with -executeMethod ArcaneBrigade.Editor.RulesVfxLibraryBuilder.Run.</summary>
    public static class RulesVfxLibraryBuilder
    {
        private const string Summons = "Assets/ArcaneBrigade/Resources/BondSummons", NewArt = "Assets/ArcaneBrigade/Art/RulesVFX", Old = "Assets/ArcaneBrigade/Art/CombatVFX", Output = "Assets/ArcaneBrigade/Resources/RulesVfxLibrary.asset";
        private static readonly string[] Looks = { "Fire", "Ice", "Water", "Storm", "Wind", "Poison", "Bloom", "Void", "Light", "Paper", "Earth", "Time", "Sound", "Dream", "Crystal", "Gold" };
        private static readonly Dictionary<string, string[]> OldBits = new Dictionary<string, string[]>
        {
            { "Fire", new[] { "Allied/LanternEmber_v1" } }, { "Ice", new[] { "Allied/GlobeShard_v1" } }, { "Water", new[] { "Allied/TeaDrop_v1", "Allied/Bubble_v1" } },
            { "Storm", new[] { "Allied/AllySpark_v1", "Allied/MagnetBolt_v1" } }, { "Wind", new[] { "Juice/InkPuff_f0_v1", "Juice/InkPuff_f1_v1" } },
            { "Poison", new[] { "Allied/Bubble_v1" } }, { "Bloom", new[] { "Allied/LeafBolt_v1" } }, { "Void", new[] { "Juice/InkPuff_f2_v1", "Juice/InkPuff_f3_v1" } },
            { "Light", new[] { "Allied/Star_v1", "Allied/AllySpark_v1" } }, { "Paper", new[] { "Allied/Letter_v1" } },
            { "Earth", new[] { "Juice/Chip_0_v1", "Juice/Chip_1_v1", "Juice/Chip_2_v1", "Juice/Chip_3_v1", "Juice/Chip_4_v1", "Juice/Chip_5_v1" } },
            { "Time", new[] { "Allied/CompassDart_v1" } }, { "Sound", new[] { "Allied/MusicNote_v1", "Allied/BellChime_v1" } }, { "Dream", new[] { "Allied/Star_v1" } },
            { "Crystal", new[] { "Allied/GlobeShard_v1" } }, { "Gold", new[] { "Allied/AllySpark_v1", "Allied/Star_v1" } },
        };

        public static void Run()
        {
            if (Directory.Exists(NewArt)) PrepareNewArt();
            if (Directory.Exists(Summons)) PrepareSummons();
            // new enemy kinds and companions drawn later: single sprites, feet at the bottom, sized to their class on screen
            PrepareUnits("Assets/ArcaneBrigade/Resources/EnemiesV3", id =>
            {
                EnemyRuleDefinition row = EnemyRulesV3Catalog.Find(id);
                return row == null ? 1.3f : row.Tier == EnemyBodyTier.Small ? .9f : row.Tier == EnemyBodyTier.LargeSpecial ? 1.8f : 1.3f;
            });
            PrepareUnits("Assets/ArcaneBrigade/Resources/CompanionsV3", id => 1.2f);
            var library = AssetDatabase.LoadAssetAtPath<RulesVfxLibrary>(Output);
            if (library == null) { library = ScriptableObject.CreateInstance<RulesVfxLibrary>(); Directory.CreateDirectory(Path.GetDirectoryName(Output)); AssetDatabase.CreateAsset(library, Output); }
            library.Entries.Clear();
            int fresh = 0;
            foreach (string look in Looks)
            {
                fresh += Add(library, look + "_Burst", Frames(NewArt + "/" + look + "_Burst_f{0}.png", 4) ?? Frames(Old + "/Juice/BlastFlash_f{0}_v1.png", 4), Frames(NewArt + "/" + look + "_Burst_f{0}.png", 4) != null);
                fresh += Add(library, look + "_Ground", One(NewArt + "/" + look + "_Ground.png"), true);
                Sprite[] bits = Sheet(NewArt + "/" + look + "_Bits.png");
                fresh += Add(library, look + "_Bits", bits ?? OldBits[look].Select(p => SpriteAt(Old + "/" + p + ".png")).Where(s => s != null).ToArray(), bits != null);
            }
            fresh += Add(library, "Shield", Frames(NewArt + "/Shield_f{0}.png", 4), true);
            foreach (string key in new[] { "Heal_Bits", "Snow_Bits", "Rain_Bits" }) fresh += Add(library, key, Sheet(NewArt + "/" + key + ".png"), true);
            fresh += Add(library, "Danger_Ground", One(NewArt + "/Danger_Ground.png"), true);
            Sprite[] danger = Frames(NewArt + "/Danger_Burst_f{0}.png", 4);
            fresh += Add(library, "Danger_Burst", danger ?? Frames(Old + "/Juice/HostileBlast_f{0}_v1.png", 4), danger != null);
            EditorUtility.SetDirty(library); AssetDatabase.SaveAssets();
            Debug.Log("RulesVfxLibraryBuilder: " + library.Entries.Count + " keys, " + fresh + " from new paintings");
        }

        private static int Add(RulesVfxLibrary library, string key, Sprite[] frames, bool fresh)
        {
            if (frames == null || frames.Length == 0 || frames.Any(f => f == null)) return 0;
            library.Entries.Add(new RulesVfxLibrary.Entry { Key = key, Frames = frames });
            return fresh ? 1 : 0;
        }
        private static Sprite SpriteAt(string path) => AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
        private static Sprite[] One(string path) { Sprite s = File.Exists(path) ? SpriteAt(path) : null; return s != null ? new[] { s } : null; }
        private static Sprite[] Frames(string pattern, int count)
        {
            var list = new List<Sprite>();
            for (int i = 0; i < count; i++) { string path = string.Format(pattern, i); Sprite s = File.Exists(path) ? SpriteAt(path) : null; if (s == null) return null; list.Add(s); }
            return list.ToArray();
        }
        private static Sprite[] Sheet(string path)
        {
            if (!File.Exists(path)) return null;
            Sprite[] parts = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name, System.StringComparer.Ordinal).ToArray();
            return parts.Length > 1 ? parts : null;
        }

        private static void PrepareUnits(string folder, System.Func<string, float> height)
        {
            if (!Directory.Exists(folder)) return;
            foreach (string path in Directory.GetFiles(folder, "*.png").Select(p => p.Replace('\\', '/')))
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;
                importer.GetSourceTextureWidthAndHeight(out int w, out int h);
                float ppu = Mathf.Max(1, Mathf.Min(h, 512) / height(Path.GetFileNameWithoutExtension(path)));
                if (importer.textureType == TextureImporterType.Sprite && Mathf.Abs(importer.spritePixelsPerUnit - ppu) < .5f) continue;
                importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single; importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false; importer.maxTextureSize = 512; importer.spritePixelsPerUnit = ppu;
                var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings); settings.spriteAlignment = (int)SpriteAlignment.BottomCenter; importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
            }
        }

        /// <summary>Bond summon and pickup paintings (Resources/BondSummons/&lt;Kind&gt;.png) import as single sprites with the feet at
        /// the bottom, sized for the field.</summary>
        private static void PrepareSummons()
        {
            foreach (string path in Directory.GetFiles(Summons, "*.png").Select(p => p.Replace('\\', '/')))
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null || importer.textureType == TextureImporterType.Sprite && importer.spriteImportMode == SpriteImportMode.Single && importer.maxTextureSize == 256) continue;
                importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single; importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false; importer.maxTextureSize = 256;
                var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings); settings.spriteAlignment = (int)SpriteAlignment.BottomCenter; importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
            }
        }

        /// <summary>New paintings import as sprites; "_Bits" sheets are cut into a 4x2 grid; grounds wrap clamp.</summary>
        private static void PrepareNewArt()
        {
            foreach (string path in Directory.GetFiles(NewArt, "*.png").Select(p => p.Replace('\\', '/')))
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;
                bool sheet = path.EndsWith("_Bits.png");
                importer.textureType = TextureImporterType.Sprite; importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
                importer.maxTextureSize = path.Contains("_Ground") || sheet ? 512 : 256;
                importer.spriteImportMode = sheet ? SpriteImportMode.Multiple : SpriteImportMode.Single;
                if (sheet)
                {
                    importer.GetSourceTextureWidthAndHeight(out int w, out int h);
                    var cells = new List<SpriteMetaData>();
                    for (int row = 0; row < 2; row++) for (int col = 0; col < 4; col++)
                        cells.Add(new SpriteMetaData { name = Path.GetFileNameWithoutExtension(path) + "_" + (row * 4 + col), rect = new Rect(col * w / 4f, (1 - row) * h / 2f, w / 4f, h / 2f), alignment = (int)SpriteAlignment.Center, pivot = new Vector2(.5f, .5f) });
#pragma warning disable 618
                    importer.spritesheet = cells.ToArray();
#pragma warning restore 618
                }
                importer.SaveAndReimport();
            }
        }
    }
}
