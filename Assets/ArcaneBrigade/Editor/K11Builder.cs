using System.IO;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K11Builder
    {
        public static void Build()
        {
            RunCatalog catalog = UnityEngine.Resources.Load<RunCatalog>("RunCatalog"); catalog.HeroRevision = 0;
            K10Builder.Apply(); K11ArtBuilder.Apply(); K11Checks.Run(); K10Checks.Run(); K9Checks.Run(); K8Builder.Check();
            K11ThemeBuilder.Apply(); K11ThemeChecks.Run();
            K7BalanceDiagnostics.Run();
            FullRunBuilder.BuildWindows();
            Directory.CreateDirectory("PreviewExports/K11");
            File.Copy("PreviewExports/FullRun_Build.txt", "PreviewExports/K11/Windows_Build.txt", true);
            File.Copy("PreviewExports/K7/Movement_Bot_Balance.csv", "PreviewExports/K11/Movement_Bot_Balance.csv", true);
            Debug.Log("K11_TRANSITION_CANDIDATE_BUILT");
        }
        public static void FinalizeCandidate()
        {
            K11Checks.Run(); K11ThemeChecks.Run();
            FullRunBuilder.BuildWindows();
            File.Copy("PreviewExports/FullRun_Build.txt", "PreviewExports/K11/Windows_Build.txt", true);
            Debug.Log("K11_THEME_FINAL_CANDIDATE_BUILT");
        }
    }
}
