using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K21BossEncounterBuilder
    {
        [MenuItem("Arcane Brigade/K21/Import detailed boss bodies")]
        public static void Build()
        {
            string[] sheets = { "RoyalArchive", "SleepWax", "LanternStage", "UmbraWidow", "ChoirLuggage" };
            string[] bosses = { "UndeliveredKing", "ScarletIndex", "SleepCollector", "WaxAdmiral", "LastLantern", "MarqueeMaw", "UmbraCradle", "CoinWidow", "ChoirEngine", "SuitcaseLeviathan" };
            for (int sheet = 0; sheet < sheets.Length; sheet++)
            {
                Sprite[] artwork = K20BossAnatomyBuilder.ImportAtlas("Assets/ArcaneBrigade/Art/K21/" + sheets[sheet] + "_v1.png", "BossCore_" + sheets[sheet], true);
                for (int half = 0; half < 2; half++)
                {
                    string id = bosses[sheet * 2 + half];
                    Sprite[] frames = artwork.Skip(half * 8).Take(8).ToArray();
                    string path = "Assets/ArcaneBrigade/Resources/" + id + "Motion.asset";
                    AuthoredMotionSet motion = AssetDatabase.LoadAssetAtPath<AuthoredMotionSet>(path);
                    if (motion == null) { motion = ScriptableObject.CreateInstance<AuthoredMotionSet>(); AssetDatabase.CreateAsset(motion, path); }
                    motion.Idle = frames.Take(2).ToArray(); motion.Walk = frames.Skip(2).Take(2).ToArray();
                    motion.Windup = frames.Skip(4).Take(2).ToArray(); motion.Release = new[] { frames[6], frames[6] }; motion.Recovery = new[] { frames[7], frames[0] };
                    UnitDefinition unit = AssetDatabase.LoadAssetAtPath<UnitDefinition>("Assets/ArcaneBrigade/Data/Run/" + id + ".asset");
                    unit.Body = frames[0]; unit.AnimationFrames = frames.Take(4).ToArray(); unit.AuthoredMotion = motion;
                    unit.Description = BossEncounterRules.SignatureName(unit.BossPattern, 0) + ". " + BossEncounterRules.SignatureName(unit.BossPattern, 1) + ". Break organs to weaken attacks.";
                    EditorUtility.SetDirty(unit); EditorUtility.SetDirty(motion);
                }
            }
            AssetDatabase.SaveAssets(); Verify();
        }
        public static void Verify()
        {
            Directory.CreateDirectory("PreviewExports/K21");
            K21BossEncounterChecks.Run(); K20BossAnatomyChecks.Run();
        }
        public static void Regressions()
        {
            K19EncounterChecks.Checks(); K10Checks.Run(); K16Checks.Run(); K4MergeChecks.Run(); K17Builder.Checks(); K18ExperienceBuilder.Checks(); K8MerchantChecks.Run();
        }
        public static void BuildWindows() { FullRunBuilder.BuildWindows(); File.Copy("PreviewExports/FullRun_Build.txt", "PreviewExports/K21/Windows_Build.txt", true); }
        public static void VerifyAndBuild() { Verify(); Regressions(); BuildWindows(); }
    }
}
