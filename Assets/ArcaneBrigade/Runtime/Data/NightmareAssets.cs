using System;
using UnityEngine;

namespace ArcaneBrigade
{
    public enum NightmareEndingStage { Dreaming, Customer, Dawn, Complete }
    public enum NightmareFarewell { Unchosen, CloseShop, LeaveLight }
    [Serializable]
    public sealed class NightmareProgress
    {
        public int Depth;
        public NightmareEndingStage Ending;
        public bool CustomerDefeated;
        public int EndingPage;
        public int NarrativeRevision;
        public NightmareFarewell Farewell;
        public int LastWhisperWave;
        public int WhisperCursor;
        public int ChoiceMask;
        public int ResolvedMask;
        public int[] Choices = { -1,-1,-1,-1,-1,-1,-1,-1 };
    }
    public sealed partial class RunSnapshot { public NightmareProgress Nightmare = new NightmareProgress(); }
    public sealed class NightmareAssets : ScriptableObject
    {
        public Sprite[] Scenery = Array.Empty<Sprite>();
        public Sprite[] CustomerFrames = Array.Empty<Sprite>();
        public Sprite[] EndingPanels = Array.Empty<Sprite>();
        public Texture2D EndingIllustration;
        public Texture2D PromiseIllustration;
        public Sprite[] ManFrames = Array.Empty<Sprite>();
        public Sprite[] GrandStage = Array.Empty<Sprite>();
        public UnitDefinition Customer;
        public BossAnatomyEntry Anatomy;
        private static NightmareAssets cached;
        public static NightmareAssets Current => cached != null ? cached : cached = Resources.Load<NightmareAssets>("NightmareAssets");
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache() => cached = null;
    }
    public static class NightmareRules
    {
        public const string CustomerId = "TheLastCustomer";
        public static int Depth(int wave) => Mathf.Clamp((Mathf.Max(1,wave) - 1) / 4,0,4);
        public static bool Remembered(RunSnapshot state, SecretEventKind kind) => ((state.Nightmare?.ResolvedMask ?? 0) & (1 << (int)kind)) != 0;
        public static int LandmarkArt(RunSnapshot state, int index)
        {
            int depth = Depth(state.Wave);
            return index switch
            {
                0 => depth == 0 ? 0 : depth < 3 ? 1 : 2,
                1 => 3,
                2 => Remembered(state,SecretEventKind.Understudy) || depth == 0 ? 4 : depth < 2 ? 5 : 6,
                3 => Remembered(state,SecretEventKind.ImpossibleTailor) || depth < 2 ? 8 : 9,
                4 => Remembered(state,SecretEventKind.EmptyTheatre) || depth < 3 ? 10 : 11,
                5 => 12,6 => 13,_ => 14
            };
        }
        public static bool FinalHall(RunSnapshot state, RunCatalog catalog) => !state.Endless && state.Phase == SessionPhase.Combat && state.Wave >= catalog.NormalWaves && state.OutsideStage != OutsideStageStatus.Fighting && state.Secrets?.Stage != SecretEventStage.Fighting;
        public static Color FloorTint(Color original, int depth, bool dawn)
        {
            if (dawn) return Color.Lerp(original,new Color(.36f,.32f,.26f),.7f);
            Color[] targets = { new Color(.23f,.25f,.27f),new Color(.2f,.21f,.29f),new Color(.2f,.16f,.28f),new Color(.23f,.15f,.22f),new Color(.18f,.14f,.24f) };
            return Color.Lerp(original,targets[Mathf.Clamp(depth,0,4)],.55f);
        }
    }
}
