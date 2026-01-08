using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K7BBuilder
    {
        [MenuItem("Arcane Brigade/K7B/Night market apply check build")]
        public static void Build()
        {
            var sprites = K7QualityBuilder.Import("NightMarket_v1", new[] { "GoldMerchant", "EggMerchant", "Fixtures" }, new[] { 0, 550, 985, 1536 });
            PaintedAnimationLibrary art = Resources.Load<PaintedAnimationLibrary>("PaintedAnimationLibrary");
            art.GoldMerchant = sprites["GoldMerchant"]; art.EggMerchant = sprites["EggMerchant"];
            art.RerollMachine = sprites["Fixtures"].Take(2).ToArray(); art.HallDoor = sprites["Fixtures"].Skip(2).ToArray();
            var bonds = K7QualityBuilder.Import("BondIcons_v1", new[] { "Bond" }, new[] { 0, 1536 });
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            string[] identities = { "hearth", "archive", "clock", "frost" };
            foreach (BondDefinition bond in catalog.Bonds) bond.Icon = bonds["Bond"][Array.IndexOf(identities, bond.Id)];
            catalog.FirstWaveSeconds = 35; catalog.WaveIncrement = 2; catalog.WaveSeconds = 55;
            catalog.BossHealthMultiplier = 0.55f;
            EditorUtility.SetDirty(catalog);
            EditorUtility.SetDirty(art); AssetDatabase.SaveAssets();
            Recheck();
        }
        public static void Recheck()
        {
            K7BChecks.Run(); K7Checks.Run(); K7QualityChecks.Run(); K6Checks.Run(); K5Checks.Run();
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            CheckpointChecks.Run(catalog.Arena); SquadCheckpointChecks.Run(catalog.Arena);
            FullRunChecks.Run(catalog); K3Checks.Run(); K32Checks.Run(); K32AltarChecks.Run(); K3FinalChecks.Run();
            K7BalanceDiagnostics.Run(); Directory.CreateDirectory("PreviewExports/K7B");
            File.Copy("PreviewExports/K7/Movement_Bot_Balance.csv", "PreviewExports/K7B/Movement_Bot_Balance.csv", true);
            FullRunBuilder.BuildWindows();
        }
        public static void FinalizeCandidate() { K7BChecks.Run(); K7Builder.RecheckCandidate(); }
    }
}
