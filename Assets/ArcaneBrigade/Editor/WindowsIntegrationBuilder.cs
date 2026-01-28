using UnityEditor;

namespace ArcaneBrigade.Editor
{
    public static class WindowsIntegrationBuilder
    {
        [MenuItem("Arcane Brigade/Full Run/Verify integrations and build Windows")]
        public static void Build()
        {
            AnimationPolishBuilder.ImportAndCheck();
            BuildMorphChecks.Run();
            K23CooperationChecks.Run();
            NightmareBuilder.Regressions();
            NightmareVisualChecks.Run();
            NightmareFarewellChecks.Run();
            ToyboxAudioChecks.Run();
            FullRunBuilder.BuildWindows();
        }
    }
}
