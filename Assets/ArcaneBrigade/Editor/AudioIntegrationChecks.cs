using System.IO;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class AudioIntegrationChecks
    {
        public static void Build()
        {
            ToyboxAudioChecks.Run();
            K8AudioChecks.Run();
            K8CombatChecks.RunFocused();
            K13Builder.VerifyAndBuild();
            File.Copy("Docs/Audio/Provenance.md", "Builds/FullRun/ThirdPartyNotices/Arcane-Audio-Provenance.md", true);
            File.Copy("Docs/Audio/Toybox_Delivery.md", "Builds/FullRun/ThirdPartyNotices/Toybox_Delivery.md", true);
            File.Copy("Docs/Audio/Toybox_Asset_Manifest.json", "Builds/FullRun/ThirdPartyNotices/Toybox_Asset_Manifest.json", true);
            Directory.CreateDirectory("Builds/FullRun/ThirdPartyNotices/ThirdParty");
            File.Copy("Docs/Audio/ThirdParty/GeneralUser-GS-LICENSE.txt", "Builds/FullRun/ThirdPartyNotices/ThirdParty/GeneralUser-GS-LICENSE.txt", true);
            Directory.CreateDirectory("PreviewExports/AudioIntegration");
            File.Copy("Library/ArcaneAudioChecks.txt", "PreviewExports/AudioIntegration/Library_Checks.txt", true);
            File.Copy("PreviewExports/FullRun_Build.txt", "PreviewExports/AudioIntegration/Windows_Build.txt", true);
            Debug.Log("AUDIO_INTEGRATION_BUILD_PASSED");
        }
    }
}
