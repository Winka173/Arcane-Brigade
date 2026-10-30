namespace ArcaneBrigade.Editor
{
    // Runs every rules suite in one batch session; any failure throws and fails the run.
    public static class RulesV3AllChecks
    {
        public static void Run()
        {
            // the older suites test the rules the step ladder replaced, with those rules switched back on
            BondRuntime.LegacyCapstones = true;
            try { BondEngineChecks.Run(); BondV5RulesChecks.Run(); } finally { BondRuntime.LegacyCapstones = false; }
            BondStepChecks.Run();
            MortalityChecks.Run();
            CompanionRulesV3Checks.Run();
            HeroRulesV3Checks.Run();
            EnemyRulesV3Checks.Run();
            RulesV3IntegrationChecks.Run();
            UnityEngine.Debug.Log("RulesV3AllChecks: PASS");
        }
    }
}
