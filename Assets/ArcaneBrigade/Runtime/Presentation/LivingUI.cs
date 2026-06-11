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
        public static void Enter(float age, Vector2 pivot)
        {
            float reveal = Reveal(age, PresentationSettings.ReducedMotion);
            ArcadeSkin.ScaleAround(Vector2.one * Mathf.Lerp(0.97f, 1, reveal), pivot);
            GUI.color *= new Color(1, 1, 1, reveal);
        }
    }
}
