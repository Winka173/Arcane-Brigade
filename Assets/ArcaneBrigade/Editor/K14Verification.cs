using System.IO;

namespace ArcaneBrigade.Editor
{
    public static class K14Verification
    {
        public static void MeasureBalance()
        {
            K14BalanceDiagnostics.Measure();
            K14CampaignDiagnostics.Run();
        }

        public static void Run()
        {
            K14Checks.Run(); K8CombatChecks.RunFocused();
            K12Checks.Run(); K12PolishChecks.Run(); MarketPolishChecks.Run(); K11Checks.Run(); K13Checks.Run();
            K11MeleeChecks.Run(); K10Checks.Run();
            FullRunBuilder.BuildWindows();
            File.Copy("PreviewExports/FullRun_Build.txt", "PreviewExports/K14/Windows_Build.txt", true);
        }
    }
}
