using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>W2, the wave is swept away (19/10/2026): when the night's last moment comes, a sweep runs across the screen
    /// from left to right and each enemy left standing sinks into a pool of its own ink as the sweep reaches it (the arena
    /// controller fades the bodies; this draws the pools). Reduced motion: everything sinks at once.</summary>
    public sealed class WaveClearInk : MonoBehaviour
    {
        private struct Pool { public Vector2 At; public float Progress, Size; public int Seed; }
        private readonly List<Pool> pools = new List<Pool>(64);
        private ArsenalInk ink;
        private bool drawn;
        private static readonly InkPalette Night = new InkPalette(new Color(.04f, .03f, .07f), new Color(.14f, .1f, .2f), new Color(.45f, .4f, .6f), new Color(.3f, .25f, .45f), new Color(.6f, .45f, .7f));
        public int Shown { get; private set; }
        /// <summary>When the sweep reaches a point (seconds after the wave ends), from its place on screen.</summary>
        public static float SweepDelay(Vector2 position, Camera camera)
        {
            if (camera == null || PresentationSettings.ReducedMotion) return 0;
            float half = camera.orthographicSize * camera.aspect;
            return Mathf.Clamp01((position.x - (camera.transform.position.x - half)) / Mathf.Max(.01f, 2 * half)) * .6f;
        }
        public void Mark(Vector2 at, float progress, float size, int seed)
        {
            if (pools.Count < 64) pools.Add(new Pool { At = at, Progress = progress, Size = size, Seed = seed });
        }
        private void LateUpdate()
        {
            Shown = pools.Count;
            if (pools.Count == 0) { if (drawn) { ink.Begin(); ink.End(); drawn = false; } return; }
            if (ink == null) ink = new ArsenalInk(transform);
            ink.Begin(); drawn = true;
            foreach (Pool p in pools)
            {
                // the pool spreads as the body sinks into it, then dries away
                float spread = Mathf.SmoothStep(0, 1, Mathf.Clamp01(p.Progress * 1.6f)), dry = 1 - Mathf.Clamp01((p.Progress - .55f) / .45f);
                ink.Pool(p.At + Vector2.down * .05f, Mathf.Max(.3f, p.Size * 1.6f) * (.5f + .7f * spread), 1, Night, p.Seed, .85f * dry, .4f);
            }
            ink.End();
            pools.Clear();
        }
        private void OnDestroy() { ink?.Destroy(); }
    }
}
