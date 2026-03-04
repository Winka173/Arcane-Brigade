using UnityEngine;
namespace ArcaneBrigade
{
    public static class OctoberBalance
    {
        public const int AllySoftLimit = 15;
        public const float WaveSeconds = 45;
        public const float EnemyHealthRelief = .75f;
        public const float EnemyDamageRelief = .8f;
        public const float BossHealthRelief = .6f;
        public const float BossDamageRelief = .9f;
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
        public static float Push(UnitDefinition target, float requested) => target.IsBoss ? 0 : Mathf.Clamp(requested, 0, .45f) * (1 - Mathf.Clamp01(target.KnockbackResistance));
        public static void CompileAlly(UnitDefinition unit, int wave)
        {
            WeaponDefinition w = unit.Weapon; float late = Late(wave);
            w.Cooldown = Mathf.Max(.28f, w.Cooldown * Mathf.Lerp(1.12f, 1.35f, late));
            w.Windup = Mathf.Min(w.Windup, w.Cooldown * .35f); w.Recovery = Mathf.Min(w.Recovery, w.Cooldown * .4f);
            float before = w.Range;
            w.Range = Mathf.Clamp(w.Range * Mathf.Lerp(1, .86f, late), .4f, w.Kind == WeaponKind.Melee ? 3.4f : 8);
            w.Damage *= Mathf.Lerp(1, .83f, late) * (w.Kind == WeaponKind.Melee ? Mathf.Clamp(Mathf.Sqrt(2.2f / Mathf.Max(2.2f, before)), .68f, 1) : 1);
            w.SignatureRadius = Mathf.Min(w.SignatureRadius, 3.2f); w.PushDistance = Mathf.Clamp(w.PushDistance, 0, .45f);
            w.ProjectileCount = Mathf.Clamp(w.ProjectileCount, 1, 10);
            w.ProjectileLifetime = Mathf.Min(w.ProjectileLifetime, 9 / Mathf.Max(1, w.ProjectileSpeed));
            unit.Speed = Mathf.Min(unit.Speed, 10);
            unit.CollisionDamage = Mathf.Min(unit.CollisionDamage, Mathf.Lerp(200, 80, late));
        }
    }
}
