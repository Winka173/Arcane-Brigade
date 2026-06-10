using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>The phone builds (owner 24/10/2026): an Android APK (IL2CPP, ARM64) and an iOS Xcode project, landscape turning
    /// either way. Run with -buildTarget Android / iOS so the editor imports for that platform (the textures' phone sizes and
    /// ASTC come from BuildSizePolicy's per-platform overrides). The iOS project is finished and signed in Xcode on a Mac.</summary>
    public static class MobileBuilder
    {
        public const string Identifier = "com.winka.arcanebrigade";

        private static void Common(NamedBuildTarget target)
        {
            PlayerSettings.SetApplicationIdentifier(target, Identifier);
            PlayerSettings.SetScriptingBackend(target, ScriptingImplementation.IL2CPP);
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
        }
        private static void Build(string location, BuildTarget target)
        {
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = FullRunBuilder.ScenePaths, locationPathName = location, target = target, options = BuildOptions.None });
            Debug.Log("MobileBuilder " + target + ": " + report.summary.result + ", errors " + report.summary.totalErrors + ", size " + report.summary.totalSize / (1024 * 1024) + " MB");
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException(target + " build failed");
        }

        [MenuItem("Arcane Brigade/Build Android (APK)")]
        public static void BuildAndroid()
        {
            Common(NamedBuildTarget.Android);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            EditorUserBuildSettings.buildAppBundle = false;
            Directory.CreateDirectory("Builds/Android");
            Build("Builds/Android/ArcaneBrigade.apk", BuildTarget.Android);
        }

        [MenuItem("Arcane Brigade/Build iOS (Xcode project)")]
        public static void BuildIOS()
        {
            Common(NamedBuildTarget.iOS);
            Directory.CreateDirectory("Builds/iOS");
            Build("Builds/iOS", BuildTarget.iOS);
        }
    }
}
