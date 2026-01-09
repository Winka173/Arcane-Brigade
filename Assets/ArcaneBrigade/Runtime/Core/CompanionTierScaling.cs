using UnityEngine;

namespace ArcaneBrigade
{
    public static class CompanionTierScaling
    {
        public static float Body(int tier) => 1 + Mathf.Clamp(tier - 1, 0, 2) * 0.18f;
        public static void Apply(WeaponDefinition weapon, int tier)
        {
            int steps = Mathf.Clamp(tier - 1, 0, 2);
            float range = 1 + steps * 0.18f;
            weapon.Range *= range;
            weapon.VisualWidth *= 1 + steps * 0.15f;
            if (weapon.Kind == WeaponKind.Projectile)
            {
                weapon.ProjectileCount += steps; weapon.ProjectilePierce += steps;
                weapon.ProjectileRadius *= 1 + steps * 0.18f;
                weapon.ProjectileVisualWidth *= 1 + steps * 0.18f;
                weapon.ProjectileLifetime *= range;
            }
            else
            {
                weapon.ConeDot = Mathf.Max(-1, weapon.ConeDot - steps * 0.1f);
                weapon.Cooldown /= 1 + steps * 0.12f;
            }
        }
    }
}
