using System.IO;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K11Builder
    {
        public static void Build()
        {
            K10Builder.Apply(); K11ArtBuilder.Apply(); K11Checks.Run(); K10Checks.Run(); K9Checks.Run(); K8Builder.Check();
            FullRunBuilder.BuildWindows();
            Directory.CreateDirectory("PreviewExports/K11");
            File.Copy("PreviewExports/FullRun_Build.txt", "PreviewExports/K11/Windows_Build.txt", true);
            Debug.Log("K11_TRANSITION_CANDIDATE_BUILT");
        }
    }
}
