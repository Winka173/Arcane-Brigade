using System.IO;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K8Builder
    {
        [MenuItem("Arcane Brigade/K8/Apply audit remediation")]
        public static void Apply()
        {
            K8ContentBuilder.Apply(); K8ArtBuilder.Apply(); K8AudioBuilder.Apply(); AssetDatabase.SaveAssets();
        }
        [MenuItem("Arcane Brigade/K8/Check and build Windows")]
        public static void Build()
        {
            Apply(); Check();
            K7BalanceDiagnostics.Run();
            Directory.CreateDirectory("PreviewExports/K8");
            File.Copy("PreviewExports/K7/Movement_Bot_Balance.csv", "PreviewExports/K8/Movement_Bot_Balance.csv", true);
            FullRunBuilder.BuildWindows();
            File.Copy("PreviewExports/FullRun_Build.txt", "PreviewExports/K8/Windows_Build.txt", true);
            Debug.Log("K8_WINDOWS_CANDIDATE_BUILT");
        }
        public static void Check()
        {
            K8PresentationChecks.Run(); K8CombatChecks.Run(); K8ContentChecks.Run(); K8MotionChecks.Run(); K8AudioChecks.Run(); K8UIChecks.Run(); K8MerchantChecks.Run();
            K7BChecks.Run(); K7Checks.Run(); K7QualityChecks.Run(); K6Checks.Run(); K5Checks.Run();
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            CheckpointChecks.Run(catalog.Arena); SquadCheckpointChecks.Run(catalog.Arena); FullRunChecks.Run(catalog);
            K3Checks.Run(); K32Checks.Run(); K32AltarChecks.Run(); K3FinalChecks.Run(); K4FinalChecks.Run();
            Debug.Log("K8_EDITOR_REGRESSION_PASSED");
        }
        public static void FinalizeCandidate()
        {
            Apply(); Check(); FullRunBuilder.BuildWindows();
            File.Copy("PreviewExports/FullRun_Build.txt", "PreviewExports/K8/Windows_Build.txt", true);
            Debug.Log("K8_FINAL_CANDIDATE_BUILT");
        }
    }
}
