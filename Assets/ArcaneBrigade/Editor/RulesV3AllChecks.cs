namespace ArcaneBrigade.Editor
{
    // Runs every rules suite in one batch session; any failure throws and fails the run.
    public static class RulesV3AllChecks
    {
        public static void Run()
        {
            BondEngineChecks.Run();
            BondV5RulesChecks.Run();
            MortalityChecks.Run();
            CompanionRulesV3Checks.Run();
            HeroRulesV3Checks.Run();
            EnemyRulesV3Checks.Run();
            RulesV3IntegrationChecks.Run();
            UnityEngine.Debug.Log("RulesV3AllChecks: PASS");
        }
    }
}
