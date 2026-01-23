using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K16Builder
    {
        public static readonly string[] Units = { "LanternKnight", "CoilKettle", "ThimbleMoth", "ReelOwl", "WaxAdmiral" };
        [MenuItem("Arcane Brigade/K16/Build crafted characters")]
        public static void Build()
        {
            foreach (string id in Units)
            {
                Sprite[] frames = Import(id);
                string path = "Assets/ArcaneBrigade/Resources/" + id + "Motion.asset";
                AuthoredMotionSet motion = AssetDatabase.LoadAssetAtPath<AuthoredMotionSet>(path);
                if (motion == null) { motion = ScriptableObject.CreateInstance<AuthoredMotionSet>(); AssetDatabase.CreateAsset(motion, path); }
                motion.Idle = frames.Take(2).ToArray(); motion.Walk = frames.Skip(2).Take(4).ToArray();
                motion.Windup = frames.Skip(6).Take(2).ToArray(); motion.Release = frames.Skip(8).Take(2).ToArray(); motion.Recovery = frames.Skip(10).Take(2).ToArray();
                UnitDefinition unit = AssetDatabase.LoadAssetAtPath<UnitDefinition>("Assets/ArcaneBrigade/Data/Run/" + id + ".asset");
                unit.AuthoredMotion = motion; unit.Body = frames[0]; unit.AnimationFrames = new[] { frames[0], frames[1], frames[2], frames[4] };
                EditorUtility.SetDirty(unit); EditorUtility.SetDirty(motion);
            }
            AssetDatabase.SaveAssets(); Verify();
        }
        public static void Verify()
        {
            K16Checks.Run(); K15Checks.Run(); K14Verification.RunChecks();
            K14BalanceDiagnostics.MeasureInto("PreviewExports/K16"); K15BalanceDiagnostics.RunInto("PreviewExports/K16");
            FullRunBuilder.BuildWindows(); File.Copy("PreviewExports/FullRun_Build.txt", "PreviewExports/K16/Windows_Build.txt", true);
        }
        public static void Campaign() { K14CampaignDiagnostics.RunInto("PreviewExports/K16"); }
        private static Sprite[] Import(string id)
        {
            string path = "Assets/ArcaneBrigade/Art/K16/" + id + "_v1.png";
            string revised = "Assets/ArcaneBrigade/Art/K16/" + id + "_v2.png";
            if (File.Exists(revised)) path = revised;
            return ImportSheet(id, path);
        }
        public static Sprite[] ImportSheet(string id, string path)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 256; importer.maxTextureSize = 4096; importer.isReadable = true;
            importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path); Color32[] pixels = texture.GetPixels32();
            if (pixels[0].a > 4) throw new InvalidOperationException("K16 transparent source required " + id);
            var factory = new SpriteDataProviderFactories(); factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var previous = provider.GetSpriteRects().ToDictionary(rectangle => rectangle.name, rectangle => rectangle.spriteID);
            var rectangles = new SpriteRect[12];
            for (int row = 0; row < 3; row++)
            {
                int bottom = (2 - row) * texture.height / 3, top = (3 - row) * texture.height / 3;
                int[] columnCuts = { 0, FindDivider(pixels, texture.width, texture.height, true, bottom, top, texture.width / 4, texture.width / 20), FindDivider(pixels, texture.width, texture.height, true, bottom, top, texture.width / 2, texture.width / 20), FindDivider(pixels, texture.width, texture.height, true, bottom, top, texture.width * 3 / 4, texture.width / 20), texture.width };
                for (int column = 0; column < 4; column++)
                {
                    int index = row * 4 + column;
                    int left = columnCuts[column], right = columnCuts[column + 1];
                    int cellBottom = row == 2 ? 0 : FindDivider(pixels, texture.width, texture.height, false, left, right, bottom, texture.height / 18);
                    int cellTop = row == 0 ? texture.height : FindDivider(pixels, texture.width, texture.height, false, left, right, top, texture.height / 18);
                    int minX = right, minY = cellTop, maxX = left, maxY = cellBottom;
                    for (int vertical = cellBottom; vertical < cellTop; vertical++)
                    for (int horizontal = left; horizontal < right; horizontal++)
                        if (pixels[vertical * texture.width + horizontal].a > 32) { minX = Mathf.Min(minX, horizontal); minY = Mathf.Min(minY, vertical); maxX = Mathf.Max(maxX, horizontal); maxY = Mathf.Max(maxY, vertical); }
                    if (maxX <= minX || maxY <= minY) throw new InvalidOperationException("K16 empty pose " + id + " " + index);
                    minX = Mathf.Max(left, minX - 2); minY = Mathf.Max(cellBottom, minY - 2); maxX = Mathf.Min(right - 1, maxX + 2); maxY = Mathf.Min(cellTop - 1, maxY + 2);
                    string name = id + "Motion_" + index.ToString("D2");
                    rectangles[index] = new SpriteRect { name = name, rect = new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1), pivot = new Vector2(0.5f, 0), alignment = SpriteAlignment.Custom, spriteID = previous.TryGetValue(name, out GUID identity) ? identity : GUID.Generate() };
                }
            }
            provider.SetSpriteRects(rectangles); provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rectangles.Select(rectangle => new SpriteNameFileIdPair(rectangle.name, rectangle.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings { name = "Standalone", overridden = true, maxTextureSize = 4096, format = TextureImporterFormat.BC7 });
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings { name = "Android", overridden = true, maxTextureSize = 4096, format = TextureImporterFormat.ASTC_6x6 });
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings { name = "iPhone", overridden = true, maxTextureSize = 4096, format = TextureImporterFormat.ASTC_6x6 });
            importer.isReadable = false; importer.SaveAndReimport();
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(sprite => sprite.name).ToArray();
        }
        private static int FindDivider(Color32[] pixels, int width, int height, bool vertical, int start, int end, int nominal, int radius)
        {
            int lower = Mathf.Max(1, nominal - radius), upper = Mathf.Min((vertical ? width : height) - 2, nominal + radius);
            int minimum = int.MaxValue; int[] counts = new int[upper - lower + 1];
            for (int cut = lower; cut <= upper; cut++)
            {
                int count = 0;
                for (int sample = start; sample < end; sample++) if (pixels[vertical ? sample * width + cut : cut * width + sample].a > 128) count++;
                counts[cut - lower] = count; minimum = Mathf.Min(minimum, count);
            }
            if (minimum > 2) throw new InvalidOperationException("K16 source has no clean pose boundary near " + nominal + " vertical=" + vertical + " region=" + start + ":" + end + " opaque=" + minimum);
            int longest = 0, result = nominal, run = 0;
            for (int cut = lower; cut <= upper; cut++)
            {
                run = counts[cut - lower] == minimum ? run + 1 : 0;
                int middle = cut - run / 2;
                if (run > longest || run == longest && Mathf.Abs(middle - nominal) < Mathf.Abs(result - nominal)) { longest = run; result = middle; }
            }
            return result;
        }
    }
}
