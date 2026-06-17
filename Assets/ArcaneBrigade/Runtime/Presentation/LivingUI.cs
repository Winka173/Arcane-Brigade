using UnityEngine;

namespace ArcaneBrigade
{
    public static class LivingUI
    {
        public static float Reveal(float age, bool reduced) => reduced ? 1 : Mathf.SmoothStep(0, 1, Mathf.Clamp01(age / 0.24f));
        public static float Press(float age, bool reduced) => reduced || age < 0 || age >= 0.22f ? 0 : Mathf.Sin(age / 0.22f * Mathf.PI) * (1 - age / 0.22f);
        /// <summary>Every popup opens with motion (requirement 24/10/2026): it grows from 86% with a small overshoot and fades in
        /// over about a fifth of a second, about its own centre (reduced motion: the fade only). The caller restores GUI.matrix
        /// and GUI.color afterwards.</summary>
        public static void Pop(float age, Vector2 pivot)
        {
            float x = Mathf.Clamp01(age / .22f);
            if (!PresentationSettings.ReducedMotion)
            {
                const float c1 = 1.1f, c3 = c1 + 1;
                float back = 1 + c3 * Mathf.Pow(x - 1, 3) + c1 * Mathf.Pow(x - 1, 2);
                ArcadeSkin.ScaleAround(Vector2.one * Mathf.LerpUnclamped(.86f, 1, back), pivot);
            }
            GUI.color *= new Color(1, 1, 1, Mathf.SmoothStep(0, 1, Mathf.Clamp01(age / .16f)));
        }
        // ---- every popup opens and closes with motion (requirement 25/10/2026, repeated many times) ----------------------
        private sealed class Life { public float OpenedAt, ClosingAt = -1; public int Frame; public System.Action Then; }
        private static readonly System.Collections.Generic.Dictionary<string, Life> lives = new System.Collections.Generic.Dictionary<string, Life>();
        public const float CloseSeconds = .16f;
        /// <summary>Call once per frame where a popup is drawn, before drawing it: it pops in whenever it appears again (not drawn
        /// for a few frames), and shrinks and fades while a close asked with <see cref="CloseThen"/> plays, after which the close's
        /// action runs. The caller restores GUI.matrix and GUI.color after the popup. Returns false once a close has finished
        /// (draw nothing more this frame).</summary>
        /// <param name="pop">False for a popup that already opens with its own motion (only the close is added).</param>
        public static bool Auto(string key, Vector2 pivot, bool pop = true)
        {
            if (!lives.TryGetValue(key, out Life life)) lives[key] = life = new Life { OpenedAt = Time.unscaledTime };
            if (Time.frameCount - life.Frame > 3 && life.ClosingAt < 0) life.OpenedAt = Time.unscaledTime;
            life.Frame = Time.frameCount;
            if (life.ClosingAt >= 0)
            {
                float k = (Time.unscaledTime - life.ClosingAt) / CloseSeconds;
                if (k >= 1)
                {
                    System.Action then = life.Then; life.ClosingAt = -1; life.Then = null; life.Frame = 0;
                    then?.Invoke(); return false;
                }
                if (!PresentationSettings.ReducedMotion) ArcadeSkin.ScaleAround(Vector2.one * Mathf.Lerp(1, .9f, k), pivot);
                GUI.color *= new Color(1, 1, 1, 1 - k);
                return true;
            }
            if (pop) Pop(Time.unscaledTime - life.OpenedAt, pivot);
            return true;
        }
        /// <summary>Closes the popup drawn under this key with motion, then runs the action (a choice, a close). Ignored while one
        /// is already closing.</summary>
        public static void CloseThen(string key, System.Action then)
        {
            if (!lives.TryGetValue(key, out Life life)) { then?.Invoke(); return; }
            if (life.ClosingAt >= 0) return;
            life.ClosingAt = Time.unscaledTime; life.Then = then;
            if (runner == null) { var go = new GameObject("Popup closes"); Object.DontDestroyOnLoad(go); runner = go.AddComponent<LivingUIRunner>(); }
        }
        private static LivingUIRunner runner;
        /// <summary>A close whose popup stopped being drawn still runs its action (a safety net).</summary>
        internal static void Sweep()
        {
            foreach (var pair in lives)
            {
                Life life = pair.Value;
                if (life.ClosingAt < 0 || Time.unscaledTime - life.ClosingAt < CloseSeconds + .15f) continue;
                System.Action then = life.Then; life.ClosingAt = -1; life.Then = null; then?.Invoke(); return; // one per frame: the dictionary may change
            }
        }
        /// <summary>Whether the popup under this key is playing its close (its buttons should not take another press).</summary>
        public static bool Closing(string key) => lives.TryGetValue(key, out Life life) && life.ClosingAt >= 0;
        public static void Enter(float age, Vector2 pivot)
        {
            float reveal = Reveal(age, PresentationSettings.ReducedMotion);
            ArcadeSkin.ScaleAround(Vector2.one * Mathf.Lerp(0.97f, 1, reveal), pivot);
            GUI.color *= new Color(1, 1, 1, reveal);
        }
    }
    public sealed class LivingUIRunner : MonoBehaviour { private void Update() => LivingUI.Sweep(); }
}
