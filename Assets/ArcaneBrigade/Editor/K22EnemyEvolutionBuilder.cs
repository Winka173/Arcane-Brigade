using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K22EnemyEvolutionBuilder
    {
        public static readonly string[] Ids = { "HushMite", "ClockBat", "BristleImp", "RuneWisp", "InkMoth", "SirenSeal", "DoorBearer", "MoonMagnet", "SleepMimic", "CurtainWisp", "FurnaceToad", "WardrobeCrab", "HopperKeg", "ChargerRam", "CoinLeech", "BlastPuffer", "SporeBishop", "BroomSkater", "RibbonHydra", "BurrowBell" };
        private static readonly string[] Names = { "Split-Eye Pounce", "Broken-Wing Crossfire", "Antler Rush", "Twisted Rune Spiral", "Ragged Molting", "Gill Echo", "Splinter Slam", "Three-Prong Salvo", "Seam-Ripper Lunge", "Curtain Collapse", "Chimney Fan", "Cabinet Scissors", "Double-Spring Rebound", "Forked-Horn Feint", "Coin-Cheek Barrage", "Second-Skin Echo", "Spore Garden", "Hooked-Comb Rake", "Five-Head Weave", "Double-Bell Eruption" };
        private static readonly string[] Summaries = { "Pounces at your old position.", "Sidesteps while spitting curved shots.", "Lowers its horns, then rushes.", "Unwinds six slow spiral shots.", "Sheds two short-lived ink pools.", "Sings two staggered impact waves.", "Slams its door-body forward.", "Pulls loose coins; spits metal.", "Opens its seam, then lunges.", "Drops three strips of living cloth.", "Spits five slow furnace globs.", "Snaps two crossed wooden jaws.", "Two jumps, each with a landing pulse.", "Steps back before a locked rush.", "Steals loose coins to fuel a fan.", "Releases a large pulse, then a small echo.", "Plants three spaced spore pools.", "Rakes three narrow bristle lanes.", "Five heads spit weaving strands.", "Tunnels, then erupts at your old position." };

        [MenuItem("Arcane Brigade/K22/Import enemy evolution art")]
        public static void Build()
        {
            const string path = "Assets/ArcaneBrigade/Resources/EnemyEvolutionLibrary.asset";
            EnemyEvolutionLibrary library = AssetDatabase.LoadAssetAtPath<EnemyEvolutionLibrary>(path);
            if (library == null) { library = ScriptableObject.CreateInstance<EnemyEvolutionLibrary>(); AssetDatabase.CreateAsset(library, path); }
            library.Species = new EnemyEvolutionEntry[Ids.Length];
            for (int index = 0; index < Ids.Length; index++)
            {
                string id = Ids[index];
                Sprite[] frames = K20BossAnatomyBuilder.ImportAtlas("Assets/ArcaneBrigade/Art/K22/" + id + "_v1.png", id + "Evolution", true);
                AuthoredMotionSet normal = Motion(id, frames.Take(8).ToArray(), false);
                AuthoredMotionSet mutated = Motion(id, frames.Skip(8).ToArray(), true);
                UnitDefinition unit = AssetDatabase.LoadAssetAtPath<UnitDefinition>("Assets/ArcaneBrigade/Data/Run/" + id + ".asset");
                unit.Body = normal.Idle[0]; unit.AuthoredMotion = normal;
                unit.AnimationFrames = new[] { normal.Idle[0], normal.Idle[1], normal.Walk[0], normal.Walk[1] };
                unit.EvolutionStage = 0; unit.Mutation = EnemyMutation.None;
                library.Species[index] = new EnemyEvolutionEntry { Id = id, MutationName = Names[index], Summary = Summaries[index], Ability = (EnemyMutation)(index + 1), Normal = normal, Mutated = mutated };
                EditorUtility.SetDirty(unit);
            }
            library.BodyFragments = K20BossAnatomyBuilder.ImportAtlas("Assets/ArcaneBrigade/Art/K22/BodyFragments_v1.png", "EnemyBodyFragment");
            EditorUtility.SetDirty(library); AssetDatabase.SaveAssets(); EnemyEvolutionLibrary.Invalidate();
            Verify();
        }
        private static AuthoredMotionSet Motion(string id, Sprite[] frames, bool mutated)
        {
            string path = "Assets/ArcaneBrigade/Resources/" + id + (mutated ? "MutatedMotion" : "Motion") + ".asset";
            AuthoredMotionSet motion = AssetDatabase.LoadAssetAtPath<AuthoredMotionSet>(path);
            if (motion == null) { motion = ScriptableObject.CreateInstance<AuthoredMotionSet>(); AssetDatabase.CreateAsset(motion, path); }
            motion.Idle = frames.Take(2).ToArray(); motion.Walk = new[] { frames[2], frames[3], frames[2], frames[3] };
            motion.Windup = frames.Skip(4).Take(2).ToArray(); motion.Release = new[] { frames[6], frames[6] }; motion.Recovery = new[] { frames[7], frames[0] };
            EditorUtility.SetDirty(motion); return motion;
        }
        public static void Verify() { Directory.CreateDirectory("PreviewExports/K22"); K22EnemyEvolutionChecks.Run(); }
        public static void VerifyAndBuild()
        {
            Verify(); K21BossEncounterBuilder.Verify(); K21BossEncounterBuilder.Regressions();
            FullRunBuilder.BuildWindows(); File.Copy("PreviewExports/FullRun_Build.txt", "PreviewExports/K22/Windows_Build.txt", true);
        }
        public static void CodeCheck() => Debug.Log("K22_CODE_COMPILED");
        public static void Campaign() => K17CampaignDiagnostics.RunInto("PreviewExports/K22");
    }
}
