using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

namespace ArcaneBrigade.Editor
{
    /// <summary>The phone builds (requirement 24/10/2026): an Android APK (IL2CPP, ARM64) and an iOS Xcode project, landscape turning
    /// either way. Run with -buildTarget Android / iOS so the editor imports for that platform (the textures' phone sizes and
    /// ASTC come from BuildSizePolicy's per-platform overrides). The iOS project is finished and signed in Xcode on a Mac.</summary>
    public static class MobileBuilder
    {
        public const string Identifier = "com.winka.arcanebrigade";

        /// <summary>The launcher icon (requirement 4/11/2026: the bundle showed Unity's default icon): the painted Lantern Knight.</summary>
        public const string IconPath = "Assets/ArcaneBrigade/Art/Store/AppIcon.png";
        private static void Common(NamedBuildTarget target)
        {
            PlayerSettings.SetApplicationIdentifier(target, Identifier);
            // Deluxe is bought through Google Play (requirement 5/11/2026): without a target store the Android player fell back to
            // Unity's fake store ("$0.01", purchases that cost nothing); the package writes its choice only after the editor has
            // finished loading, which a batch build never waits for
            if (target == NamedBuildTarget.Android) UnityEditor.Purchasing.UnityPurchasingEditor.TargetAndroidStore(UnityEngine.Purchasing.AppStore.GooglePlay);
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath) is Texture2D icon) PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any);
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

        /// <summary>Signs with the Google Play upload key (requirement 4/11/2026). The keystore and its passwords live outside the
        /// repository, in ~/.secrets/arcane-brigade-upload.properties (keystore=, alias=, storePassword=, keyPassword=) or the
        /// file named by the ARCANE_UPLOAD_KEY environment variable; without it the build is signed with the debug key and
        /// cannot be uploaded to Google Play.</summary>
        private static void Signing()
        {
            string file = Environment.GetEnvironmentVariable("ARCANE_UPLOAD_KEY");
            if (string.IsNullOrEmpty(file)) file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".secrets", "arcane-brigade-upload.properties");
            if (!File.Exists(file)) { PlayerSettings.Android.useCustomKeystore = false; Debug.LogWarning("MobileBuilder: no upload key, signed with the debug key"); return; }
            var values = new System.Collections.Generic.Dictionary<string, string>();
            foreach (string line in File.ReadAllLines(file))
            {
                int eq = line.IndexOf('='); if (eq <= 0 || line.TrimStart().StartsWith("#")) continue;
                values[line.Substring(0, eq).Trim()] = line.Substring(eq + 1).Trim();
            }
            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = values["keystore"];
            PlayerSettings.Android.keystorePass = values["storePassword"];
            PlayerSettings.Android.keyaliasName = values["alias"];
            PlayerSettings.Android.keyaliasPass = values["keyPassword"];
            Debug.Log("MobileBuilder: signed with the upload key " + values["alias"]);
        }

        [MenuItem("Arcane Brigade/Build Android (APK)")]
        public static void BuildAndroid()
        {
            Common(NamedBuildTarget.Android);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            EditorUserBuildSettings.buildAppBundle = false;
            PlayerSettings.Android.splitApplicationBinary = false;
            Directory.CreateDirectory("Builds/Android");
            Build("Builds/Android/ArcaneBrigade.apk", BuildTarget.Android);
        }

        /// <summary>The Google Play upload (requirement 3/11/2026: the APK is 288 MB, Play takes at most 200 MB in the base module): an
        /// app bundle whose game data rides in an install-time asset pack, so the base keeps only the code and the player still
        /// installs once. Signed with the upload key (<see cref="Signing"/>).</summary>
        [MenuItem("Arcane Brigade/Build Android (Play bundle)")]
        public static void BuildAndroidBundle()
        {
            Common(NamedBuildTarget.Android);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            EditorUserBuildSettings.buildAppBundle = true;
            PlayerSettings.Android.splitApplicationBinary = true;
            Signing();
            Directory.CreateDirectory("Builds/Android");
            Build("Builds/Android/ArcaneBrigade.aab", BuildTarget.Android);
        }

        /// <summary>An APK for PC Android emulators such as BlueStacks (requirement 5/11/2026): x86-64 code next to ARM64 so the
        /// emulator runs the game natively (its ARM translator crashed in the frame-pacing library), OpenGL ES 3 instead of Vulkan
        /// (it stalled there), frame pacing off. Not for Google Play.</summary>
        [MenuItem("Arcane Brigade/Build Android (emulator APK)")]
        public static void BuildAndroidEmulator()
        {
            Common(NamedBuildTarget.Android);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.X86_64;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });
            PlayerSettings.Android.optimizedFramePacing = false;
            EditorUserBuildSettings.buildAppBundle = false;
            PlayerSettings.Android.splitApplicationBinary = false;
            Signing();
            Directory.CreateDirectory("Builds/Android");
            Build("Builds/Android/ArcaneBrigade-Emulator.apk", BuildTarget.Android);
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
