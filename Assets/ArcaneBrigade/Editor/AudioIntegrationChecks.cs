using System.IO;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class AudioIntegrationChecks
    {
        public static void Build()
        {
            ArcaneAudioChecks.Run();
            K8AudioChecks.Run();
            K13Builder.VerifyAndBuild();
            File.Copy("Docs/Audio/Provenance.md", "Builds/FullRun/ThirdPartyNotices/Arcane-Audio-Provenance.md", true);
            Directory.CreateDirectory("PreviewExports/AudioIntegration");
            File.Copy("Library/ArcaneAudioChecks.txt", "PreviewExports/AudioIntegration/Library_Checks.txt", true);
            File.Copy("PreviewExports/FullRun_Build.txt", "PreviewExports/AudioIntegration/Windows_Build.txt", true);
            Debug.Log("AUDIO_INTEGRATION_BUILD_PASSED");
        }
    }
}
