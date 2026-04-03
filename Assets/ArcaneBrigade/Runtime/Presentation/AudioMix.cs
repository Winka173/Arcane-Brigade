using UnityEngine;
using UnityEngine.Audio;

namespace ArcaneBrigade
{
    /// <summary>The mixer buses every sound goes through (Resources/Audio/ArcaneMix.mixer: Master -> Music, SFX, UI, Voice, Boss).</summary>
    public enum MixBus { Music, Sfx, UI, Voice, Boss }

    /// <summary>
    /// The one place the game's sound reaches the speakers (playtest item 2a). Every AudioSource the game creates is routed
    /// with <see cref="Route"/>; the settings sliders set the bus levels (<see cref="ApplySettings"/>, called by
    /// PresentationSettings.Apply); the music bus sits a step lower than before and ducks under dense combat SFX and boss cues
    /// (a sidechain by script: <see cref="ReportSfx"/> feeds the loudness of every SFX start, <see cref="Tick"/> moves the duck).
    /// New sound files only need a cue row in cues.json (its bus) or a Route() call: they land in the same buses.
    /// Without the mixer asset everything still plays, with the sliders applied on each source as before (<see cref="SourceGain"/>).
    /// </summary>
    public static class AudioMix
    {
        public const string MixerResource = "Audio/ArcaneMix";
        public static readonly string[] GroupNames = { "Music", "SFX", "UI", "Voice", "Boss" };
        public static readonly string[] VolumeParams = { "MusicVol", "SfxVol", "UIVol", "VoiceVol", "BossVol" };
        public const string MasterParam = "MasterVol";
        /// <summary>Music sits one step (3 dB) lower than before for focus; the slider still goes from silence to this level.</summary>
        public const float MusicTrimDb = -3f;
        /// <summary>Deepest music duck under dense combat or a boss cue, and how fast it goes down and comes back.</summary>
        public const float DuckDepthDb = -5.5f, DuckAttack = 0.06f, DuckRelease = 0.6f;
        /// <summary>Recent SFX loudness (decaying sum of start gains, time constant <see cref="EnergyDecay"/>) where the duck starts and is full.</summary>
        public const float DuckThreshold = 0.8f, DuckFull = 2.4f, EnergyDecay = 0.25f, BossHold = 0.5f;

        private static AudioMixer mixer;
        private static AudioMixerGroup[] groups;
        private static bool tried;
        private static float energy, duckDb, bossUntil, lastTick = -1f;
        private static int tickFrame = -1;

        public static AudioMixer Mixer { get { Ensure(); return mixer; } }
        /// <summary>True when the mixer asset loaded with all five buses: the sliders then live on the buses, not on the sources.</summary>
        public static bool Live { get { Ensure(); return groups != null; } }
        public static float DuckDb => duckDb;
        public static float Energy => energy;

        private static void Ensure()
        {
            if (tried) return;
            tried = true;
            mixer = Resources.Load<AudioMixer>(MixerResource);
            if (mixer == null) { Debug.LogWarning("AudioMix: " + MixerResource + " missing; sources keep their own slider gain."); return; }
            var found = new AudioMixerGroup[GroupNames.Length];
            for (int bus = 0; bus < GroupNames.Length; bus++)
            {
                foreach (AudioMixerGroup group in mixer.FindMatchingGroups("Master/" + GroupNames[bus]))
                    if (group != null && group.name == GroupNames[bus]) { found[bus] = group; break; }
                if (found[bus] == null) { Debug.LogWarning("AudioMix: bus " + GroupNames[bus] + " missing in the mixer."); return; }
            }
            groups = found;
            Push();
        }

        public static AudioMixerGroup Group(MixBus bus) { Ensure(); return groups == null ? null : groups[(int)bus]; }

        /// <summary>Sends a source to a bus (no-op without the mixer). Returns the source for chaining.</summary>
        public static AudioSource Route(AudioSource source, MixBus bus)
        {
            if (source != null) source.outputAudioMixerGroup = Group(bus);
            return source;
        }

        /// <summary>The player's slider for a bus (UI and Boss follow the sound-effects slider).</summary>
        public static float Slider(MixBus bus) => bus == MixBus.Music ? PresentationSettings.MusicVolume : bus == MixBus.Voice ? PresentationSettings.VoiceVolume : PresentationSettings.SfxVolume;

        /// <summary>Gain a routed source applies itself: the bus carries the slider, so 1 (0 when muted, so muted sources stay silent);
        /// without the mixer the slider itself, as before.</summary>
        public static float SourceGain(MixBus bus)
        {
            float slider = Slider(bus);
            return Live ? (slider > 0.0001f ? 1f : 0f) : slider;
        }

        public static float ToDb(float linear) => linear <= 0.0001f ? -80f : Mathf.Max(-80f, 20f * Mathf.Log10(linear));
        public static float FromDb(float db) => Mathf.Pow(10f, db / 20f);

        /// <summary>Level of a bus in dB for a slider value and the current music duck.</summary>
        public static float BusDb(MixBus bus, float slider, float duck)
        {
            float db = ToDb(slider);
            if (db <= -80f) return -80f;
            return Mathf.Max(-80f, db + (bus == MixBus.Music ? MusicTrimDb + duck : 0f));
        }

        // ------------------------------------------------------------------ sidechain duck
        /// <summary>Every SFX start reports its gain here; combat and boss SFX build up the duck, a boss cue ducks at once.</summary>
        public static void ReportSfx(float linearGain, MixBus bus)
        {
            if (bus == MixBus.Boss) bossUntil = Time.unscaledTime + BossHold;
            if (bus == MixBus.Sfx || bus == MixBus.Boss) energy += Mathf.Max(0f, linearGain);
        }

        /// <summary>Duck depth (dB, 0 or below) wanted for a recent SFX loudness; a boss cue always takes the full depth.</summary>
        public static float DuckTarget(float loudness, bool boss)
            => boss ? DuckDepthDb : DuckDepthDb * Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(DuckThreshold, DuckFull, loudness));

        /// <summary>One step of the duck envelope: down within <see cref="DuckAttack"/>, back up within <see cref="DuckRelease"/> (full depth).</summary>
        public static float StepDuck(float current, float target, float dt)
            => Mathf.MoveTowards(current, target, dt * -DuckDepthDb / (target < current ? DuckAttack : DuckRelease));

        /// <summary>Once per frame (music and SFX owners both call it; the second call is free): decay the loudness, move the duck, set the buses.</summary>
        public static void Tick()
        {
            if (Time.frameCount == tickFrame) return;
            tickFrame = Time.frameCount;
            float now = Time.unscaledTime;
            float dt = lastTick < 0f ? 0f : Mathf.Clamp(now - lastTick, 0f, 0.1f);
            lastTick = now;
            energy *= Mathf.Exp(-dt / EnergyDecay);
            duckDb = StepDuck(duckDb, DuckTarget(energy, now < bossUntil), dt);
            Push();
        }

        /// <summary>Sliders changed (PresentationSettings.Apply): set the buses now.</summary>
        public static void ApplySettings() { Ensure(); Push(); }

        /// <summary>Checks only: forget the loudness history.</summary>
        public static void ResetDuck() { energy = 0f; duckDb = 0f; bossUntil = 0f; }

        private static void Push()
        {
            if (mixer == null || groups == null) return;
            mixer.SetFloat(MasterParam, 0f); // master stays at unity: the master slider is AudioListener.volume, so unrouted sources follow it too
            for (int bus = 0; bus < VolumeParams.Length; bus++)
                mixer.SetFloat(VolumeParams[bus], BusDb((MixBus)bus, Slider((MixBus)bus), duckDb));
        }
    }
}
