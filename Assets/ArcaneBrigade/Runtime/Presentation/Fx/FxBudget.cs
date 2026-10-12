using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Feature 12 (owner 29/10/2026): one effect budget for the whole field. It measures the crowd (living enemies) and a
    /// smoothed frame time against the frame pace the game asks for, and settles on a tier. Small effects ask it before drawing;
    /// big ones never do. Order of what gives way: 1) enemy element states and number ornaments, 2) then step 2/4 trait effects
    /// (repeats of one rule merge into one), never step 6/8 moments, weather, forewarnings or victory finales. Presentation only.</summary>
    public static class FxBudget
    {
        /// <summary>0 full, 1 trimmed (fewer marks and ornaments), 2 crowded (no ornaments, few marks, traits merge), 3 lean.</summary>
        public static int Tier { get; private set; }
        /// <summary>Living enemies when last measured, and the smoothed frame time in milliseconds.</summary>
        public static int Crowd { get; private set; }
        public static float FrameMs { get; private set; } = 16.7f;
        /// <summary>The frame time the game is paced for (target frame rate or v-sync), so a 30 fps profile is not read as slow.</summary>
        public static float PaceMs { get; private set; } = 16.7f;
        /// <summary>Forces a tier (the sandbox's "crowded field" toggle, checks); -1 measures.</summary>
        public static int ForcedTier = -1;
        /// <summary>Set when the sandbox forced the tier: closing the sandbox hands the budget back to the measurement.</summary>
        public static bool ForcedBySandbox;
        /// <summary>Trait effects skipped because a repeat of the same rule was already showing (read by checks and the sandbox).</summary>
        public static int Merged { get; private set; }

        public static readonly int[] CrowdAt = { 70, 130, 200 };
        public static readonly float[] SlowAt = { 1.25f, 1.5f, 1.9f }; // frame time as a multiple of the pace
        private const float CalmFor = 1.5f;
        private static float calmSince = -1, clock;
        private static int lastFrame = -1;
        private static readonly Dictionary<string, float> lastTrait = new Dictionary<string, float>();

        /// <summary>Once a frame (RulesV3Ink): counts the living enemies and folds this frame's time in.</summary>
        public static void Measure(CombatSimulation sim)
        {
            if (Time.frameCount == lastFrame) return; lastFrame = Time.frameCount;
            if (ForcedBySandbox && !FxSandbox.IsOpen) { ForcedTier = -1; ForcedBySandbox = false; }
            int crowd = 0;
            if (sim != null) { IReadOnlyList<UnitState> units = sim.Units; for (int i = 0; i < units.Count; i++) { UnitState u = units[i]; if (u != null && u.Alive && u.Faction == Faction.Enemy) crowd++; } }
            Measure(crowd, Time.unscaledDeltaTime, sim != null && sim.Phase == RunPhase.Combat);
        }

        /// <summary>The pure step: <paramref name="dt"/> seconds this frame; frame time only counts while the fight runs.</summary>
        public static void Measure(int crowd, float dt, bool running)
        {
            Crowd = crowd; clock += dt;
            float pace = 1000f / 60f;
            if (Application.targetFrameRate > 0) pace = Mathf.Max(pace, 1000f / Application.targetFrameRate);
            if (QualitySettings.vSyncCount > 0) { double hz = Screen.currentResolution.refreshRateRatio.value; if (hz > 1) pace = Mathf.Max(pace, (float)(1000.0 * QualitySettings.vSyncCount / hz)); }
            PaceMs = pace;
            if (running && dt > 0 && dt < .25f) FrameMs = Mathf.Lerp(FrameMs, dt * 1000f, .06f); // hitches (loading, alt-tab) are not load
            int want = 0;
            for (int t = 0; t < 3; t++) if (crowd >= CrowdAt[t] || FrameMs >= PaceMs * SlowAt[t]) want = t + 1;
            if (want >= Tier) { Tier = want; calmSince = -1; }
            else
            {
                // step down only after a calm spell well under the thresholds, so the look does not flicker between tiers
                int t = Tier - 1; bool calm = crowd < CrowdAt[t] * .85f && FrameMs < PaceMs * SlowAt[t] * .9f;
                if (!calm) calmSince = -1;
                else if (calmSince < 0) calmSince = clock;
                else if (clock - calmSince >= CalmFor) { Tier--; calmSince = -1; }
            }
        }

        public static int Effective => ForcedTier >= 0 ? Mathf.Min(ForcedTier, 3) : Tier;

        /// <summary>How many enemy element states (EnemyElementState) get their full animation; the rest show a tint.</summary>
        public static int MarkDetail(int full) { int t = Effective; return t == 0 ? full : t == 1 ? full / 2 : t == 2 ? Mathf.Min(full / 4, 10) : 0; }
        /// <summary>Whether the <paramref name="index"/>-th tint-only mark is still drawn.</summary>
        public static bool MarkTint(int index) => Effective < 3 || index < 60;
        /// <summary>Whether the <paramref name="index"/>-th damage number keeps its element ornament (NumberOrnament).</summary>
        public static bool Ornament(int index) { int t = Effective; return t == 0 || t == 1 && (index & 1) == 0; }

        /// <summary>Whether a trait effect of <paramref name="rule"/> (step <paramref name="step"/>) plays now. Step 6 and up always
        /// plays; in a crowd, a repeat of a rule already showing merges into it instead of stacking another copy.</summary>
        public static bool Trait(string rule, int step, float now)
        {
            if (rule == null || step >= 6) return true;
            int t = Effective; float window = t >= 3 ? .8f : t == 2 ? .4f : 0;
            if (window > 0 && lastTrait.TryGetValue(rule, out float last) && now >= last && now - last < window) { Merged++; return false; }
            if (lastTrait.Count > 256) lastTrait.Clear();
            lastTrait[rule] = now; return true;
        }

        /// <summary>Most enemies a lamp (dynamic light) tints in one frame.</summary>
        public static int LampTints => Effective >= 2 ? 24 : 48;

        public static void Reset() { Tier = 0; FrameMs = PaceMs = 16.7f; Crowd = 0; calmSince = -1; lastTrait.Clear(); Merged = 0; }
    }
}
