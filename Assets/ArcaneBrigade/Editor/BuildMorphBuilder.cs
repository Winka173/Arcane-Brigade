using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class BuildMorphBuilder
    {
        [MenuItem("Arcane Brigade/Build Forms/Import artwork")]
        public static void Import()
        {
            string path = "Assets/ArcaneBrigade/Resources/BuildMorphArtwork.asset";
            BuildMorphArtwork art = AssetDatabase.LoadAssetAtPath<BuildMorphArtwork>(path);
            if (art == null) { art = ScriptableObject.CreateInstance<BuildMorphArtwork>(); AssetDatabase.CreateAsset(art, path); }
            Sprite[] backs = K25AtlasImporter.Import("Assets/ArcaneBrigade/Art/BuildMorphs/BackAttachments_v3.png", "BuildMorphBack", 4, 3, Enumerable.Range(0, 12).ToArray());
            Sprite[] charms = K25AtlasImporter.Import("Assets/ArcaneBrigade/Art/BuildMorphs/Charms_v2.png", "BuildMorphCharm", 4, 6, Enumerable.Range(12, 12).ToArray(), new[] { 0, 171, 342, 542, 703, 852, 1024 });
            art.Parts = backs.Concat(charms).ToArray();
            EditorUtility.SetDirty(art); AssetDatabase.SaveAssets();
            Debug.Log("BUILD_MORPH_ART_IMPORTED 24 sprites");
        }
        [MenuItem("Arcane Brigade/Build Forms/Verify")]
        public static void Verify() => BuildMorphChecks.Run();
        public static void CheckContent() { Import(); Verify(); }
        public static void Gate()
        {
            Import(); Verify();
            K23CooperationChecks.Run(); SecretEventChecks.Run(); NightmareChecks.Run(); ToyboxAudioChecks.Run();
            Player();
        }
        public static void Player()
        {
            Directory.CreateDirectory("Builds/BuildMorphs");
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = "Builds/BuildMorphs/ArcaneBrigade.exe", target = BuildTarget.StandaloneWindows64, options = BuildOptions.None });
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Build forms player failed");
            Debug.Log("BUILD_MORPH_PLAYER_BUILT errors=" + report.summary.totalErrors + " warnings=" + report.summary.totalWarnings);
        }
    }
}
