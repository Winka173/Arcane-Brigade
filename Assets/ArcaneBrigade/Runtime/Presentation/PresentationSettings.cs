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
        /// <summary>Screen-shake strength, 0 (off) to 1.</summary>
        public static float ShakeScale { get; private set; } = 1f;
        public static bool ReducedFlashes { get; private set; }
        public static bool ReducedEffects { get; private set; }
        /// <summary>Halo under the hero, props fading off it, show-through and edge arrow (HeroFocus); on by default.</summary>
        public static bool HighlightHero { get; private set; } = true;
        public static float DangerUntil { get; private set; }
        public static void Apply(PlayerProfile profile)
        {
            MusicVolume = profile.MusicVolume; SfxVolume = profile.SfxVolume; VoiceVolume = profile.VoiceVolume;
            ReducedAudio = profile.ReducedAudio; ReducedMotion = profile.ReducedMotion;
            DamageNumbers = profile.ShowDamageNumbers; LootIndicators = profile.ShowLootIndicators;
            HighReadability = profile.HighReadability;
            ReducedShake = profile.ReducedShake; ShakeScale = profile.ReducedShake ? 0 : Mathf.Clamp01(profile.ShakeScale); ReducedFlashes = profile.ReducedFlashes; ReducedEffects = profile.ReducedEffects; HighlightHero = profile.HighlightHero;
            AudioListener.volume = profile.Volume;
            AudioMix.ApplySettings(); // the music / SFX / voice sliders drive the mixer buses
            ArcadeSkin.Bundle = profile.Cosmetic;
        }
        /// <summary>Window mode, resolution and vsync from the profile; only a player build changes the screen, and only when
        /// something differs (the editor, batch runs and captures keep their own).</summary>
        public static void ApplyDisplay(PlayerProfile profile)
        {
            if (profile == null || Application.isEditor || Application.isBatchMode || RunPaths.Diagnostic) return;
            QualitySettings.vSyncCount = profile.VSync ? 1 : 0;
            if (profile.WindowMode < 0 && (profile.ScreenWidth <= 0 || profile.ScreenHeight <= 0)) return;
            FullScreenMode mode = profile.WindowMode == 0 ? FullScreenMode.ExclusiveFullScreen : profile.WindowMode == 1 ? FullScreenMode.FullScreenWindow : profile.WindowMode == 2 ? FullScreenMode.Windowed : Screen.fullScreenMode;
            int width = profile.ScreenWidth > 0 ? profile.ScreenWidth : Screen.width, height = profile.ScreenHeight > 0 ? profile.ScreenHeight : Screen.height;
            if (Screen.fullScreenMode != mode || Screen.width != width || Screen.height != height) Screen.SetResolution(width, height, mode);
        }
        public static void Danger(float seconds = 0.5f) { DangerUntil = Mathf.Max(DangerUntil, Time.unscaledTime + seconds); }
        // a voiced line ducks the music lightly while it is audible (VoiceDirector)
        public static float VoiceVolume { get; private set; } = 1f;
        public static float VoiceDuckUntil { get; private set; }
        public static void VoiceDuck(float seconds) { VoiceDuckUntil = Mathf.Max(VoiceDuckUntil, Time.unscaledTime + seconds); }
        public static float MusicDuck => Time.unscaledTime < DangerUntil ? 0.48f : Time.unscaledTime < VoiceDuckUntil ? 0.72f : 1f;
    }
}
