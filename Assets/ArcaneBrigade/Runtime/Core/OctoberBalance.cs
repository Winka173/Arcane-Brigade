using UnityEngine;
namespace ArcaneBrigade
{
    public static class OctoberBalance
    {
        public const int AllySoftLimit = 15;
        public const float WaveSeconds = 52; // 45 before the play-test (+15%, D9)
        public const float EnemyHealthRelief = .82f; // requirement 25/10/2026 (third pass): every enemy a little softer at every difficulty (was .87, .92, 1.03)
        public const float EnemyDamageRelief = 1f;   // requirement 25/10/2026 (third pass): hits a little harder again at every difficulty (was .93, .86)
        /// <summary>From wave 5, at every difficulty, a night brings fewer enemies: a smaller crowd and slower arrivals.</summary>
        public const int LaterCrowdWave = 5; public const float LaterCrowd = .72f; // (second pass; was .85)
        /// <summary>From wave 10 the crowd thins again (requirement 25/10/2026: too many to count) and never passes a hard cap.</summary>
        public const int DeepCrowdWave = 10; public const float DeepCrowd = .55f; public const int CrowdCap = 220;
        public const float BossHealthRelief = .5f; // requirement 24/10/2026: was .6 - long boss fights farmed the overtime swarm's gold
        /// <summary>A boss night lasts this much longer before the boss's overtime swarm begins (requirement 24/10/2026).</summary>
        public const float BossExtraSeconds = 30;
        public const float BossDamageRelief = .7f; // round 3 (19/10/2026, play-test: boss waves too hard): was .9, -22% on everything a boss deals
        public const float NormalEnemyHealth = 1.1f; // round 3: ordinary (non-boss) enemies +10% health, normal waves felt a little easy
        public static float Crowd(int count) => Mathf.Clamp01((count - 10) / (float)(AllySoftLimit - 10));
        public static float NewAllyChance(int count, int wave = 4)
        {
            float crowd = Mathf.Clamp01((count - 6) / (float)(AllySoftLimit - 6));
            float chance = count < 6 ? .85f : Mathf.Lerp(.55f, .02f, 1 - (1 - crowd) * (1 - crowd));
            return wave <= 3 ? Mathf.Max(.7f, chance) : chance;
        }
        public static float RecruitChance(int count) => Mathf.Lerp(1, .32f, Crowd(count));
        public static float Late(int wave) => Mathf.Clamp01((wave - 8) / 12f);
        public static float Soften(float bonus, float knee, float slope) => bonus > knee ? knee + (bonus - knee) * slope : bonus;
        public static float MaximumHealth(float raw, int wave) => Mathf.Clamp(Soften(raw, 250, Mathf.Lerp(1, .25f, Late(wave))), 1, Mathf.Lerp(2000, 600, Late(wave)));
        public static float FlatDamage(float bonus, int wave) => Soften(bonus, 10, Mathf.Lerp(1, .35f, Late(wave)));
        public static float EndlessHealth(int wave, bool endless) => endless ? Mathf.Pow(1.16f, Mathf.Min(200, Mathf.Max(0, wave - 19))) : 1;
        public static float EndlessDamage(int wave, bool endless) => endless ? Mathf.Pow(1.075f, Mathf.Min(200, Mathf.Max(0, wave - 19))) : 1;
        /// <summary>Requirement 23/10/2026: enemies are pushed back less - every knockback at 60%.</summary>
        public const float EnemyKnockback = .6f;
        public static float Push(UnitDefinition target, float requested) => target.IsBoss ? 0 : Mathf.Clamp(requested, 0, .45f) * (1 - Mathf.Clamp01(target.KnockbackResistance)) * EnemyKnockback;
        public static void CompileAlly(UnitDefinition unit, int wave)
        {
            WeaponDefinition w = unit.Weapon; float late = Late(wave);
            w.Cooldown = Mathf.Max(.28f, w.Cooldown * Mathf.Lerp(1.12f, 1.35f, late));
            w.Windup = Mathf.Min(w.Windup, w.Cooldown * .35f); w.Recovery = Mathf.Min(w.Recovery, w.Cooldown * .4f);
            float before = w.Range;
            w.Range = FriendlyReach.Limit(w.Range * Mathf.Lerp(1, .86f, late), w.Kind);
            w.Damage *= Mathf.Lerp(1, .83f, late) * (w.Kind == WeaponKind.Melee ? Mathf.Clamp(Mathf.Sqrt(2.2f / Mathf.Max(2.2f, before)), .68f, 1) : 1);
            w.SignatureRadius = Mathf.Min(w.SignatureRadius, 3.2f); w.PushDistance = Mathf.Clamp(w.PushDistance, 0, .45f);
            w.ProjectileCount = Mathf.Clamp(w.ProjectileCount, 1, 10);
            w.ProjectileLifetime = Mathf.Min(w.ProjectileLifetime, 9 / Mathf.Max(1, w.ProjectileSpeed));
            unit.Speed = Mathf.Min(unit.Speed, 10);
            unit.CollisionDamage = Mathf.Min(unit.CollisionDamage, Mathf.Lerp(200, 80, late));
        }
    }
}
