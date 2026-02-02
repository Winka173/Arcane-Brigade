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
        public static void Run()
        {
            var suites = new (string name, Action run)[]
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
            };
            var lines = new List<string>();
            int failed = 0;
            foreach (var (name, run) in suites)
            {
                try { run(); lines.Add("PASS " + name); }
                catch (Exception error) { failed++; lines.Add("FAIL " + name + ": " + (error.InnerException ?? error).Message); }
            }
            Directory.CreateDirectory("PreviewExports/ActOneStory");
            File.WriteAllLines("PreviewExports/ActOneStory/Checks.txt", lines);
            Debug.Log("ACT_ONE_STORY_CHECKS " + (lines.Count - failed) + "/" + lines.Count + " PASS\n" + string.Join("\n", lines));
            if (Application.isBatchMode) EditorApplication.Exit(failed == 0 ? 0 : 1);
        }
    }
}
