using UnityEngine;

namespace ArcaneBrigade
{
    public static class PresentationSettings
    {
        public static float MusicVolume { get; private set; } = 0.8f;
        public static float SfxVolume { get; private set; } = 0.85f;
        public static bool ReducedAudio { get; private set; }
        public static bool ReducedMotion { get; private set; }
        public static bool DamageNumbers { get; private set; } = true;
        public static bool LootIndicators { get; private set; } = true;
        public static bool HighReadability { get; private set; }
        public static bool ReducedShake { get; private set; }
        public static bool ReducedFlashes { get; private set; }
        public static bool ReducedEffects { get; private set; }
        public static float DangerUntil { get; private set; }
        public static void Apply(PlayerProfile profile)
        {
            MusicVolume = profile.MusicVolume; SfxVolume = profile.SfxVolume;
            ReducedAudio = profile.ReducedAudio; ReducedMotion = profile.ReducedMotion;
            DamageNumbers = profile.ShowDamageNumbers; LootIndicators = profile.ShowLootIndicators;
            HighReadability = profile.HighReadability;
            ReducedShake = profile.ReducedShake; ReducedFlashes = profile.ReducedFlashes; ReducedEffects = profile.ReducedEffects;
            AudioListener.volume = profile.Volume;
            ArcadeSkin.Bundle = profile.Cosmetic;
        }
        public static void Danger(float seconds = 0.5f) { DangerUntil = Mathf.Max(DangerUntil, Time.unscaledTime + seconds); }
        public static float MusicDuck => Time.unscaledTime < DangerUntil ? 0.48f : 1f;
    }
}
