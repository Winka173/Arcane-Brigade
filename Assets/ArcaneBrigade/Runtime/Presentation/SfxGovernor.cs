using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Who a sound belongs to, for voice priority: hero, boss and UI sounds are never stolen by swarm hits.</summary>
    public enum SfxRole { Swarm, Normal, Hero, Boss, UI }

    /// <summary>
    /// SFX hygiene (playtest item 2a), one rule set for every sound effect: at most <see cref="MaxSameClip"/> copies of one clip at
    /// once and never two starts closer than <see cref="MinRetrigger"/>; a small random pitch (±4%) and volume (±2 dB) variation;
    /// one loudness target for all clips (per-clip gains measured by the editor step AudioMixBuilder into
    /// Resources/Audio/Arcane/loudness.json; a clip that is not in the table plays at 0 dB, so new files work at once and are
    /// levelled the next time the step runs); a role-based priority and mixer bus.
    /// </summary>
    public static class SfxGovernor
    {
        public const int MaxSameClip = 4;
        public const float MinRetrigger = 0.04f;
        public const float PitchSpread = 0.04f;
        public const float GainSpreadDb = 2f;
        public const string LoudnessResource = "Audio/Arcane/loudness";

        private sealed class ClipState { public float Last = -10f; public readonly float[] Ends = new float[MaxSameClip]; }
        private static readonly Dictionary<int, ClipState> clips = new Dictionary<int, ClipState>();

        /// <summary>
        /// Admits a start of a clip (key = clip instance id) at time <paramref name="now"/>. Refused when the same clip started less
        /// than 40 ms ago, or when four copies still ring (hero, boss and UI sounds take the oldest copy's place instead).
        /// </summary>
        public static bool Admit(int key, float length, float now, SfxRole role)
        {
            if (!clips.TryGetValue(key, out ClipState state)) clips[key] = state = new ClipState();
            if (now - state.Last < MinRetrigger) return false;
            int active = 0, slot = 0;
            for (int index = 0; index < MaxSameClip; index++)
            {
                if (state.Ends[index] > now) active++;
                if (state.Ends[index] < state.Ends[slot]) slot = index;
            }
            if (active >= MaxSameClip && role < SfxRole.Hero) return false;
            state.Last = now;
            state.Ends[slot] = now + Mathf.Max(0.01f, length);
            if (clips.Count > 1024) clips.Clear();
            return true;
        }

        public static bool Admit(AudioClip clip, float now, SfxRole role) => clip == null || Admit(clip.GetHashCode(), clip.length, now, role);

        /// <summary>Checks only.</summary>
        public static void ResetVoices() => clips.Clear();

        /// <summary>A pitch within ±<paramref name="spread"/> of <paramref name="basePitch"/>.</summary>
        public static float VaryPitch(float basePitch, float spread = PitchSpread) => basePitch * (1f + UnityEngine.Random.Range(-spread, spread));
        /// <summary>A linear gain within ±2 dB.</summary>
        public static float VaryGain() => AudioMix.FromDb(UnityEngine.Random.Range(-GainSpreadDb, GainSpreadDb));

        // ------------------------------------------------------------------ loudness table
        [Serializable] public sealed class LoudnessRow { public string clip; public float rmsDb; public float gainDb; }
        [Serializable] public sealed class LoudnessTable { public float targetDb; public LoudnessRow[] clips; }
        private static Dictionary<string, float> loudness;

        /// <summary>Linear gain that brings a clip to the shared loudness target (1 for clips the table does not know).</summary>
        public static float Loudness(AudioClip clip)
        {
            if (clip == null) return 1f;
            EnsureLoudness();
            return loudness.TryGetValue(clip.name, out float gain) ? gain : 1f;
        }

        public static int LoudnessEntries { get { EnsureLoudness(); return loudness.Count; } }

        private static void EnsureLoudness()
        {
            if (loudness == null)
            {
                loudness = new Dictionary<string, float>();
                TextAsset text = Resources.Load<TextAsset>(LoudnessResource);
                if (text != null)
                {
                    try
                    {
                        LoudnessTable table = JsonUtility.FromJson<LoudnessTable>(text.text);
                        if (table?.clips != null) foreach (LoudnessRow row in table.clips) loudness[row.clip] = AudioMix.FromDb(row.gainDb);
                    }
                    catch (Exception exception) { Debug.LogWarning("Loudness table unreadable: " + exception.Message); }
                }
            }
        }

        // ------------------------------------------------------------------ roles, priority, bus
        /// <summary>Role of a cue: the hero's own sounds (or any sound the hero's attack made), boss cues, UI; enemy hits,
        /// deaths and spit are the swarm.</summary>
        public static SfxRole RoleOf(string id, AudioBus bus, bool heroSource)
        {
            if (id == null) return SfxRole.Normal;
            if (bus == AudioBus.UI && !id.StartsWith("ui.cutin.boss", StringComparison.Ordinal)) return SfxRole.UI;
            if (IsBossCue(id)) return SfxRole.Boss;
            // a signature weapon's attack (hero.wpn.*, 4/11/2026) is the hero's only when the hero fired it; a friend wielding it stays in its own role
            if (heroSource || id.StartsWith("plr.", StringComparison.Ordinal) || id.StartsWith("hero.", StringComparison.Ordinal) && !id.StartsWith("hero.wpn.", StringComparison.Ordinal)) return SfxRole.Hero;
            if (id.StartsWith("cmb.hit", StringComparison.Ordinal) || id.StartsWith("enemy.", StringComparison.Ordinal) || id == "fx.knockback") return SfxRole.Swarm;
            return SfxRole.Normal;
        }

        public static bool IsBossCue(string id) => id != null && (id.StartsWith("boss.", StringComparison.Ordinal) || id == "ui.cutin.boss");

        /// <summary>Unity voice priority (0 = most important): UI, boss and hero sit far above anything the swarm can reach.</summary>
        public static int Priority(SfxRole role, int cuePriority)
        {
            switch (role)
            {
                case SfxRole.UI: return 8;
                case SfxRole.Boss: return 12;
                case SfxRole.Hero: return 24;
                case SfxRole.Swarm: return Mathf.Clamp(Mathf.Max(cuePriority, 200), 200, 255);
                default: return Mathf.Clamp(cuePriority, 64, 199);
            }
        }

        /// <summary>The bus of a cue: boss cues on Boss, UI on UI, voices on Voice, everything else (combat, foley, ambience) on SFX.</summary>
        public static MixBus BusOf(string id, AudioBus bus)
        {
            if (IsBossCue(id)) return MixBus.Boss;
            switch (bus)
            {
                case AudioBus.UI: return MixBus.UI;
                case AudioBus.Voice: return MixBus.Voice;
                case AudioBus.Music: return MixBus.Music;
                default: return MixBus.Sfx;
            }
        }
    }
}
