using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Batch gate for the Act I story rework (Docs/Mien_Cho_Act_One_Story.md): runs every check suite the
    /// canon touches and records each result, so one failing suite does not hide the others.</summary>
    public static class ActOneStoryChecks
    {
        /// <summary>Wider regression sweep: suites that build real fights, where the story party now walks in too.</summary>
        public static void RunWide() => Execute("ActOneWide", new (string name, Action run)[]
        {
            ("Recruitment", RecruitmentChecks.Run),
            ("K23Cooperation", K23CooperationChecks.Run),
            ("CombatRepair", CombatRepairChecks.Verify),
            ("October", OctoberChecks.Run),
            ("NightmareVisual", NightmareVisualChecks.Run),
            ("DreamLevitation", DreamLevitationChecks.Run),
            ("K10", K10Checks.Run),
            ("K16", K16Checks.Run),
            ("K4Arena", K4ArenaChecks.Run),
            ("HeroIdentity", HeroIdentityChecks.Run),
            ("BuildMorph", BuildMorphChecks.Run),
            ("K14", K14Checks.Run),
            ("K4Signature", K4SignatureChecks.RunSignatureRules),
            ("K4Final", K4FinalChecks.Run),
            ("K15Feel", K15Checks.Run),
            ("FeelTelegraph", FeelTelegraphChecks.Run),
            ("HeroAwakening", HeroAwakeningChecks.Run),
            ("ContentExpansion", ContentExpansionChecks.Run),
            ("K8Audio", K8AudioChecks.Run),
            ("K7", K7Checks.Run),
        });

        /// <summary>Play-test review phase A (20/10/2026): the suites its changes touch, in one launch.</summary>
        public static void RunPhaseA() => Execute("PhaseA", new (string name, Action run)[]
        {
            ("BuildSizePolicy", BuildSizeChecks.Policy),
            ("ShopDescription", ShopDescriptionChecks.Run),
            ("Visitor", VisitorChecks.Run),
            ("HeroIdentity", HeroIdentityChecks.Run),
            ("FeelTelegraph", FeelTelegraphChecks.Run),
            ("K15Feel", K15Checks.Run),
            ("RangedFeel", RangedFeelChecks.Run),
            ("MienCho", MienChoChecks.Run),
            ("ArtMaster", ArtMasterChecks.Run),
            ("K6", K6Checks.Run),
            ("K8UI", K8UIChecks.Run),
            ("K8Motion", K8MotionChecks.Run),
            ("K8Presentation", K8PresentationChecks.Run),
            ("SecretEvent", SecretEventChecks.Run),
            ("Pacing", PacingChecks.Run),
        });
        /// <summary>K32 alone: its K1 large-roster Details check runs for well over 30 minutes (quadratic in roster size,
        /// unrelated to phase A), so it is not in the phase A batch.</summary>
        public static void RunK32() => Execute("K32", new (string name, Action run)[] { ("K32", K32Checks.Run) });

        public static void Run() => Execute("ActOneStory", new (string name, Action run)[]
            {
                ("MienCho", MienChoChecks.Run),
                ("WaitingActOne", WaitingActOneChecks.Run),
                ("Nightmare", NightmareChecks.Run),
                ("NightmareFarewell", NightmareFarewellChecks.Run),
                ("K8Combat", K8CombatChecks.Run),
                ("K11Merchant", K11Checks.Run),
                ("K20BossAnatomy", K20BossAnatomyChecks.Run),
                ("K21BossEncounter", K21BossEncounterChecks.Run),
                ("K25EnemyAffix", K25EnemyAffixChecks.Run),
                ("ShopFlow", ShopFlowChecks.Run),
                ("K24OutsideStage", K24OutsideStageChecks.Run),
                ("SecretEvent", SecretEventChecks.Run),
                ("FinalQuality", FinalQualityChecks.Run),
                ("RosterCapAndBond", RosterCapAndBondChecks.Run),
                ("Pacing", PacingChecks.Run),
            });

        private static void Execute(string folder, (string name, Action run)[] suites)
        {
            var lines = new List<string>();
            int failed = 0;
            foreach (var (name, run) in suites)
            {
                try { run(); lines.Add("PASS " + name); }
                catch (Exception error)
                {
                    failed++;
                    Exception root = error.InnerException ?? error;
                    string[] stack = (root.StackTrace ?? "").Split('\n');
                    string where = root is InvalidOperationException ? "" : " @ " + string.Join(" | ", stack, 0, Math.Min(3, stack.Length)).Trim();
                    lines.Add("FAIL " + name + ": " + root.Message + where);
                }
                Debug.Log("SUITE_RESULT " + lines[lines.Count - 1]); // each result as it lands (a hung suite does not hide the rest)
            }
            Directory.CreateDirectory("PreviewExports/" + folder);
            File.WriteAllLines("PreviewExports/" + folder + "/Checks.txt", lines);
            Debug.Log(folder.ToUpperInvariant() + "_CHECKS " + (lines.Count - failed) + "/" + lines.Count + " PASS\n" + string.Join("\n", lines));
            if (Application.isBatchMode) EditorApplication.Exit(failed == 0 ? 0 : 1);
        }
    }
}
