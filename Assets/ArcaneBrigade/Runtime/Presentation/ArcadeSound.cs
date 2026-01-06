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
            AudioClip source = Resources.Load<AudioClip>("Audio/Licensed/Cue-" + kind.ToString("00"));
            return source != null ? Object.Instantiate(source) : NightCabaretSound.Foley(kind);
        }
    }
}
