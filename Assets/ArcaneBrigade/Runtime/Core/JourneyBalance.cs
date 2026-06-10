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
        // round 6 (21/10/2026, unmeasured nudge toward 89/78/72/56/44/28 from 89/89/67/44/39/22): Wandering harder (health 1 -> 1.05,
        // damage .8 -> .88), Nightmare easier (damage 1.6 -> 1.45), Sleepless/Beyond ~5% less damage (2.2 -> 2.1, 3 -> 2.85).
        // round 7 (requirement 22/10/2026, unmeasured): every enemy +20% health and +15% damage at its base (BaseHealth/BaseDamage), and the
        // difficulties spread further apart, the high ones much harder (was health .7/1.05/1.4/1.75/2.3/2.95, damage .38/.88/1.3/1.45/2.1/2.85,
        // density 1/1.05/1.1/1.15/1.2/1.25)
        public const float BaseHealth = 1.2f, BaseDamage = 1.15f;
        /// <summary>Enemy health added per wave (requirement 22/10/2026: steeper growth; was .10, wave 20 now about +20% health).</summary>
        public const float WaveHealthStep = .13f;
        // the last two (Lightless, The Last Night; 24/10/2026) open only with the whole Lamp Tree lit
        public static readonly float[] HealthFactor = { .7f, 1.1f, 1.55f, 2.1f, 2.85f, 3.8f, 5.1f, 6.8f };
        public static readonly float[] DamageFactor = { .38f, .92f, 1.4f, 1.8f, 2.55f, 3.5f, 4.6f, 6f };
        public static readonly float[] DensityFactor = { 1f, 1.05f, 1.12f, 1.2f, 1.3f, 1.42f, 1.56f, 1.72f };
        private static float Level(float[] table, int difficulty) => table[Mathf.Clamp(difficulty, 0, table.Length - 1)];

        public static float EnemyHealth(int difficulty, int wave) => BaseHealth * (1 + (Level(HealthFactor, difficulty) - 1) * Ramp(wave));
        public static float EnemyDamage(int difficulty, int wave) => BaseDamage * .62f * (1 + (Level(DamageFactor, difficulty) - 1) * Ramp(wave));
        public static float EnemyDensity(int difficulty, int wave) => 1 + (Level(DensityFactor, difficulty) - 1) * Ramp(wave);
        public static float LoneJellyDamage(int wave) => 1.2f + .3f * OctoberBalance.Late(wave);
    }
}
