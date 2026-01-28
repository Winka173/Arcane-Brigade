using UnityEngine;

namespace ArcaneBrigade
{
    public static class AnimationDetailBudget
    {
        public static int Units { get; private set; }
        public static int Shots { get; private set; }
        public static int Level => Units >= 420 || Shots >= 600 ? 2 : Units >= 180 || Shots >= 300 ? 1 : 0;
        public static bool SurfaceDetail => Level == 0 && !PresentationSettings.ReducedEffects && !PresentationSettings.HighReadability;
        public static void SetLoad(int units, int shots) { Units = Mathf.Max(0, units); Shots = Mathf.Max(0, shots); }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState() { Units = Shots = 0; }
    }
}
