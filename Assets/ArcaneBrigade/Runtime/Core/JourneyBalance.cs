using UnityEngine;

namespace ArcaneBrigade
{
    public static class JourneyBalance
    {
        public static float Ramp(int wave) => Mathf.Lerp(.4f, 1, Mathf.SmoothStep(0, 1, Mathf.Clamp01((wave - 1) / 7f)));

        // Balance pass round 2 (19/10/2026, Docs/Balance_Pass_2026-10-19.md): the six difficulties as a table of factors reached at
        // full ramp (wave 8+), eased in from wave 1 like before. Target bot win rate ~90/80/65/50/40/30%. The old curves (HP up to
        // x2.975, damage x1.95, density x1.6 over Peaceful) barely moved the bot's win rate: hero strength dominated and the extra
        // density paid for itself in coins. Peaceful is now gentler than the old base; density grows less (more enemies = more gold).
        // Iteration 2 table was health .8/1.05/1.35/1.75/2.25/2.9 and damage .6/.85/1.15/1.5/1.95/2.5.
        // Indexes: Peaceful, Wandering, Restless, Nightmare, Sleepless, Beyond the Lamp.
        // round 5: Restless only (78% bot wins vs ~65% target): health 1.3 -> 1.4, damage 1.15 -> 1.3
        public static readonly float[] HealthFactor = { .7f, 1f, 1.4f, 1.75f, 2.3f, 2.95f };
        public static readonly float[] DamageFactor = { .38f, .8f, 1.3f, 1.6f, 2.2f, 3f };
        public static readonly float[] DensityFactor = { 1f, 1.05f, 1.1f, 1.15f, 1.2f, 1.25f };
        private static float Level(float[] table, int difficulty) => table[Mathf.Clamp(difficulty, 0, table.Length - 1)];

        public static float EnemyHealth(int difficulty, int wave) => 1 + (Level(HealthFactor, difficulty) - 1) * Ramp(wave);
        public static float EnemyDamage(int difficulty, int wave) => .62f * (1 + (Level(DamageFactor, difficulty) - 1) * Ramp(wave));
        public static float EnemyDensity(int difficulty, int wave) => 1 + (Level(DensityFactor, difficulty) - 1) * Ramp(wave);
        public static float LoneJellyDamage(int wave) => 1.2f + .3f * OctoberBalance.Late(wave);
    }
}
