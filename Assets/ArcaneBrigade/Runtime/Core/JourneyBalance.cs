using UnityEngine;

namespace ArcaneBrigade
{
    public static class JourneyBalance
    {
        public static float Ramp(int wave) => Mathf.Lerp(.4f, 1, Mathf.SmoothStep(0, 1, Mathf.Clamp01((wave - 1) / 7f)));
        public static float EnemyHealth(int difficulty, int wave)
        {
            float level = Mathf.Clamp(difficulty, 0, 5);
            return 1 + (.17f * level + .045f * level * level) * Ramp(wave);
        }
        public static float EnemyDamage(int difficulty, int wave)
        {
            float level = Mathf.Clamp(difficulty, 0, 5);
            return .62f * (1 + (.15f * level + .008f * level * level) * Ramp(wave));
        }
        public static float EnemyDensity(int difficulty, int wave) => 1 + Mathf.Clamp(difficulty, 0, 5) * .12f * Ramp(wave);
        public static float LoneJellyDamage(int wave) => 1.2f + .3f * OctoberBalance.Late(wave);
    }
}
