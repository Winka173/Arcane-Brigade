using UnityEngine;

namespace ArcaneBrigade
{
    public static class LivingUI
    {
        public static float Reveal(float age, bool reduced) => reduced ? 1 : Mathf.SmoothStep(0, 1, Mathf.Clamp01(age / 0.24f));
        public static float Press(float age, bool reduced) => reduced || age < 0 || age >= 0.22f ? 0 : Mathf.Sin(age / 0.22f * Mathf.PI) * (1 - age / 0.22f);
        public static void Enter(float age, Vector2 pivot)
        {
            float reveal = Reveal(age, PresentationSettings.ReducedMotion);
            GUIUtility.ScaleAroundPivot(Vector2.one * Mathf.Lerp(0.97f, 1, reveal), pivot);
            GUI.color *= new Color(1, 1, 1, reveal);
        }
    }
}
