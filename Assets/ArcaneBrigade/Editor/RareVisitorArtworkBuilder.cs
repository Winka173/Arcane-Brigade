using System;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class RareVisitorArtworkBuilder
    {
        public static readonly string[] Ids = { "StarMender", "ThirdBellKeeper", "UnclaimedGiftKeeper" };
        [MenuItem("Arcane Brigade/Art/Import rare visitors")]
        public static void Build()
        {
            AssetDatabase.Refresh();
            foreach (string id in Ids)
            {
                string path = "Assets/ArcaneBrigade/Resources/RareVisitors/" + id + "_v1.png";
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                if (importer == null) throw new InvalidOperationException("Missing rare visitor sheet " + path);
                importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
                importer.alphaIsTransparency = true; importer.isReadable = true; importer.mipmapEnabled = false;
                importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 2048;
                importer.textureCompression = TextureImporterCompression.Uncompressed; importer.filterMode = FilterMode.Bilinear;
                importer.spritePixelsPerUnit = 256; importer.SaveAndReimport();
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                Color32[] pixels = texture.GetPixels32(); int width = texture.width / 2;
                // Same LOCAL rectangle for both frames: preserve identity, scale and baseline during pose transitions.
                // Alpha <= 16 is only generator quantization in empty margins, not a meaningful painted silhouette.
                int left = width, right = 0, bottom = texture.height, top = 0;
                for (int y = 0; y < texture.height; y++) for (int x = 0; x < texture.width; x++)
                {
                    if (pixels[y * texture.width + x].a <= 16) continue;
                    int local = x % width;
                    left = Mathf.Min(left, local); right = Mathf.Max(right, local + 1);
                    bottom = Mathf.Min(bottom, y); top = Mathf.Max(top, y + 1);
                }
                if (left <= 1 || right >= width - 1 || bottom <= 1 || top >= texture.height - 1)
                    throw new InvalidOperationException(id + " meaningful silhouette crosses a cell boundary");
                const int padding = 8;
                left = Mathf.Max(0, left - padding); right = Mathf.Min(width, right + padding);
                bottom = Mathf.Max(0, bottom - padding); top = Mathf.Min(texture.height, top + padding);
                var factory = new SpriteDataProviderFactories(); factory.Init();
                var provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
                var previous = provider.GetSpriteRects().ToDictionary(s => s.name, s => s.spriteID);
                var frames = new SpriteRect[2];
                for (int pose = 0; pose < 2; pose++)
                {
                    string name = id + (pose == 0 ? "_Waiting" : "_Offering");
                    frames[pose] = new SpriteRect { name = name, spriteID = previous.TryGetValue(name, out GUID guid) ? guid : GUID.Generate(),
                        rect = new Rect(pose * width + left, bottom, right - left, top - bottom),
                        alignment = SpriteAlignment.Custom, pivot = new Vector2(.5f, padding / (float)(top - bottom)) };
                }
                provider.SetSpriteRects(frames); provider.Apply();
                importer.isReadable = false; importer.SaveAndReimport();
            }
            AssetDatabase.SaveAssets(); RareVisitorChecks.Run();
        }
        public static void VerifyAndBuildPlayer()
        {
            Build();
            const string output = "Builds/RareVisitors/ArcaneBrigade.exe";
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(output));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { "RunBoot", "RunFrontend", "RunArena", "WaitingRoom" }.Select(s => "Assets/ArcaneBrigade/Scenes/" + s + ".unity").ToArray(),
                target = BuildTarget.StandaloneWindows64, locationPathName = output, options = BuildOptions.None });
            System.IO.File.WriteAllText("PreviewExports/RareVisitors-20261022/Build.txt", report.summary.result
                + "\nErrors: " + report.summary.totalErrors + "\nWarnings: " + report.summary.totalWarnings);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new InvalidOperationException("Rare visitor player build failed");
        }
    }
}
