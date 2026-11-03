using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Frame pacing for release testing (requirement 3/11/2026: mobile is released first and the game was never timed on a
    /// device): with -arcane-frame-log every five seconds the player log gets the mean, 95th and 99th percentile and worst frame
    /// times with the wave, phase and units on the field. Reading only; it changes nothing in play.</summary>
    public sealed class FrameStatsLog : MonoBehaviour
    {
        public static bool Requested => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-frame-log") >= 0;
        private const float Window = 5f;
        private readonly List<float> frames = new List<float>(512);
        private float started;
        private ArenaController arena;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (!Requested || FindAnyObjectByType<FrameStatsLog>() != null) return;
            var go = new GameObject("FrameStatsLog"); DontDestroyOnLoad(go); go.AddComponent<FrameStatsLog>();
        }

        private void Update()
        {
            frames.Add(Time.unscaledDeltaTime * 1000f);
            if (Time.unscaledTime - started < Window) return;
            if (arena == null) arena = FindAnyObjectByType<ArenaController>();
            frames.Sort();
            float sum = 0; foreach (float f in frames) sum += f;
            float P(float q) => frames[Mathf.Clamp(Mathf.CeilToInt(q * frames.Count) - 1, 0, frames.Count - 1)];
            int units = arena != null && arena.Simulation != null ? arena.Simulation.Units.Count : 0;
            string where = arena != null && arena.Session != null ? "wave=" + arena.Session.State.Wave + " phase=" + arena.Session.State.Phase : "menu";
            Debug.Log("FRAME_STATS " + where + " units=" + units + " frames=" + frames.Count + " mean=" + (sum / frames.Count).ToString("0.0")
                + "ms p95=" + P(.95f).ToString("0.0") + "ms p99=" + P(.99f).ToString("0.0") + "ms worst=" + frames[frames.Count - 1].ToString("0.0")
                + "ms memMB=" + (GC.GetTotalMemory(false) / 1048576));
            frames.Clear(); started = Time.unscaledTime;
        }
    }
}
