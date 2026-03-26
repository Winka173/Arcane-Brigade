using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Build-size gate: the art/visual/UI suites after the texture policy and resampling (each suite recorded on
    /// its own), plus the policy's own invariants on every texture the build ships.</summary>
    public static class BuildSizeChecks
    {
        public static void RunGate()
        {
            var lines = new List<string>(); int failed = 0;
            foreach (var (name, run) in new (string, Action)[]
            {
                ("BuildSizePolicy", Policy),
                ("VisualQuality", VisualQualityChecks.Run),
                ("ShopDescription", ShopDescriptionChecks.Run),
                ("PolishPass", PolishPassChecks.Run),
                ("HeroAwakening", HeroAwakeningChecks.Run),
                ("K8Presentation", K8PresentationChecks.Run),
                ("K8UI", K8UIChecks.Run),
                ("K8Motion", K8MotionChecks.Run),
                ("ExpansionHero", ExpansionHeroChecks.Run),
                ("ArtMaster", ArtMasterChecks.Run),
                ("MienCho", MienChoChecks.Run),
            })
            {
                try { run(); lines.Add("PASS " + name); }
                catch (Exception error)
                {
                    failed++; Exception root = error.InnerException ?? error;
                    string[] stack = (root.StackTrace ?? "").Split('\n');
                    lines.Add("FAIL " + name + ": " + root.Message + " @ " + string.Join(" | ", stack, 0, Math.Min(3, stack.Length)).Trim());
                }
            }
            Directory.CreateDirectory("PreviewExports/BuildSize");
            File.WriteAllLines("PreviewExports/BuildSize/Checks.txt", lines);
            Debug.Log("BUILDSIZE_CHECKS " + (lines.Count - failed) + "/" + lines.Count + " PASS\n" + string.Join("\n", lines));
            if (Application.isBatchMode) EditorApplication.Exit(failed == 0 ? 0 : 1);
        }

        /// <summary>Every texture under the game's folders: no readable flag, a BC7 Standalone override and an ASTC phone
        /// override, sides compressible (multiple of 4; power of two when mipmapped); gauge order maps point/linear/RG16.</summary>
        public static void Policy()
        {
            var problems = new List<string>(); int checkedCount = 0;
            // what the player ships: the build scenes and every Resources asset, with their dependencies
            var roots = new[] { "RunBoot", "RunFrontend", "RunArena", "WaitingRoom" }.Select(s => "Assets/ArcaneBrigade/Scenes/" + s + ".unity").Where(File.Exists)
                .Concat(AssetDatabase.GetAllAssetPaths().Where(p => p.StartsWith("Assets/") && p.Contains("/Resources/") && !AssetDatabase.IsValidFolder(p) && !p.EndsWith(".cs")));
            foreach (string path in AssetDatabase.GetDependencies(roots.ToArray(), true).Distinct().Where(p => p.StartsWith("Assets/ArcaneBrigade/")).OrderBy(p => p))
            {
                if (!(AssetImporter.GetAtPath(path) is TextureImporter importer)) continue;
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path); if (texture == null) continue;
                checkedCount++;
                if (importer.isReadable) problems.Add("readable " + path);
                if (BuildSizePolicy.IsOrderMap(path))
                {
                    if (importer.mipmapEnabled || importer.sRGBTexture || importer.filterMode != FilterMode.Point || texture.format != TextureFormat.RG16) problems.Add("order map settings " + path + " " + texture.format);
                    continue;
                }
                if (texture.format != TextureFormat.BC7 && texture.format != TextureFormat.DXT1 && texture.format != TextureFormat.DXT5) problems.Add("not block-compressed " + path + " " + texture.format + " " + texture.width + "x" + texture.height);
                var phone = importer.GetPlatformTextureSettings("Android");
                if (!phone.overridden || phone.format != TextureImporterFormat.ASTC_6x6) problems.Add("no ASTC phone override " + path);
            }
            File.WriteAllLines("PreviewExports/BuildSize/PolicyProblems.txt", problems);
            if (problems.Count > 0) throw new InvalidOperationException(problems.Count + " of " + checkedCount + " textures break the build-size policy, first: " + string.Join("; ", problems.Take(5)));
            Debug.Log("BUILDSIZE_POLICY OK " + checkedCount + " textures");
        }
    }
}
