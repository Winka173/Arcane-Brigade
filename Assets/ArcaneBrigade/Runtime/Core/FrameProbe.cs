using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Diagnostics (5/11/2026, opening the brigade book and starting a run felt slow on phones): logs how long the next
    /// few frames take after an action, so a slow first draw shows in the device log as "[timing] book frame 1 123 ms".</summary>
    public sealed class FrameProbe : MonoBehaviour
    {
        private string label;
        private int left;
        private float last;
        public static void Watch(string label, int frames = 6)
        {
            var probe = new GameObject("Frame probe").AddComponent<FrameProbe>();
            probe.label = label; probe.left = frames; probe.last = Time.realtimeSinceStartup;
        }
        /// <summary>Any frame over 250 ms is logged with the frame number, to place a freeze among the other timings.</summary>
        private sealed class SlowFrames : MonoBehaviour
        {
            private float last;
            private void LateUpdate()
            {
                float now = Time.realtimeSinceStartup, ms = (now - last) * 1000; last = now;
                if (ms > 250 && Time.frameCount > 5) Debug.Log("[timing] slow frame " + Time.frameCount + " " + Mathf.RoundToInt(ms) + " ms");
            }
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot() { if (!RunPaths.Diagnostic) return; var go = new GameObject("Slow frames"); DontDestroyOnLoad(go); go.AddComponent<SlowFrames>(); }

        private void Update()
        {
            float now = Time.realtimeSinceStartup;
            Debug.Log("[timing] " + label + " frame " + (7 - left) + " (" + Time.frameCount + ") " + Mathf.RoundToInt((now - last) * 1000) + " ms");
            last = now;
            if (--left <= 0) Destroy(gameObject);
        }
    }
}
