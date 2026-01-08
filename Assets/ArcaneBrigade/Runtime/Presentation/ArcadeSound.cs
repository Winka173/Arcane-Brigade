using UnityEngine;

namespace ArcaneBrigade
{
    public static class ArcadeSound
    {
        public const int SampleRate = 44100;
        public static float[] MusicStudy(bool shop) => NightCabaretSound.Compose(0, shop);
        public static float[] NightShiftStem(int stem) => NightCabaretSound.Compose(stem);
        public static AudioClip Cue(int kind)
        {
            AudioClip source = AcquireCue(kind, out bool owned);
            return owned || source == null ? source : Object.Instantiate(source);
        }

        public static int MaterialCue(BodyMaterial material, bool death) => (death ? 27 : 33) + (int)material;

        public static AudioClip AcquireCue(int kind, out bool owned)
        {
            owned = false;
            if (kind < 0 || kind >= 39) return null;
            int sourceKind = kind;
            if (kind >= 33)
            {
                int[] impacts = { 15, 10, 3, 14, 2, 16 };
                sourceKind = impacts[kind - 33];
            }
            AudioClip source = Resources.Load<AudioClip>("Audio/Licensed/Cue-" + sourceKind.ToString("00"));
            if (source != null) return source;
            owned = true;
            return NightCabaretSound.Foley(kind);
        }
    }
}
