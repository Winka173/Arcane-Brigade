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
            // the ground shadows the light throws: how many stand on the floor, and the first few with their draw order and strength
            GroundShadow[] shadows = FindObjectsByType<GroundShadow>(FindObjectsSortMode.None);
            int lit = 0; var sample = new System.Text.StringBuilder();
            foreach (GroundShadow shadow in shadows)
            {
                bool shown = shadow.Cast != null && shadow.Cast.enabled && shadow.Cast.gameObject.activeInHierarchy && shadow.Cast.color.a > .02f;
                if (shown) lit++;
                if (sample.Length < 2400 && shadow.isActiveAndEnabled && shadow.Cast != null) sample.Append(" " + shadow.name + "(on=" + shown + " a=" + shadow.Cast.color.a.ToString("0.00") + " order=" + shadow.Cast.sortingOrder + " at=" + (Vector2)shadow.Cast.bounds.center + " size=" + (Vector2)shadow.Cast.bounds.size + ")");
            }
            Debug.Log("FRAME_SHADOWS " + where + " lit=" + lit + "/" + shadows.Length + sample);
            // every painted thing standing in the world should throw one: list the visible ones with no ground shadow of their own or a parent's
            var bare = new System.Text.StringBuilder(); int missing = 0;
            foreach (SpriteRenderer r in FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
            {
                if (!r.enabled || !r.gameObject.activeInHierarchy || r.sprite == null || r.color.a < .3f || r.sortingOrder < 0) continue;
                Vector3 size = r.bounds.size; if (size.x < .5f && size.y < .5f || size.x > 12 || size.y > 12) continue;
                if (r.GetComponentInParent<GroundShadow>() != null) continue;
                string path = r.name; for (Transform t = r.transform.parent; t != null; t = t.parent) path = t.name + "/" + path;
                if (path.Contains("WorldGroundShadow")) continue;
                missing++; if (bare.Length < 1500) bare.Append(" " + path + "(" + r.sortingOrder + "," + size.x.ToString("0.0") + "x" + size.y.ToString("0.0") + ")");
            }
            var floors = new System.Text.StringBuilder();
            foreach (Renderer r in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
                if (r.enabled && r.gameObject.activeInHierarchy && r.sortingOrder >= 0 && r.sortingOrder < 1000 && (r.bounds.size.x > 5 || r.bounds.size.y > 5) && floors.Length < 900) floors.Append(" " + r.name + "(" + r.sortingOrder + "," + r.GetType().Name + "," + (Vector2)r.bounds.size + ")");
            Debug.Log("FRAME_BROAD " + where + floors);
            Debug.Log("FRAME_UNSHADOWED " + where + " count=" + missing + bare);
            frames.Clear(); started = Time.unscaledTime;
        }
    }
}
