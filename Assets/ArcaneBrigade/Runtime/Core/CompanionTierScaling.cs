using UnityEngine;

namespace ArcaneBrigade
{
    public static class CompanionTierScaling
    {
        public static float Body(int tier) => tier >= 3 ? 1.8f : tier == 2 ? 1.35f : 1;
        public static void Apply(WeaponDefinition weapon, int tier)
        {
            int steps = Mathf.Clamp(tier - 1, 0, 2);
            float range = 1 + steps * .09f;
            weapon.Range *= range;
            weapon.VisualWidth *= Body(tier);
            if (weapon.Kind == WeaponKind.Projectile)
            {
                weapon.ProjectileCount += steps; weapon.ProjectilePierce += steps;
                weapon.ProjectileRadius *= Body(tier);
                weapon.ProjectileVisualWidth *= Body(tier);
                weapon.ProjectileLifetime *= range;
            }
            else
            {
                weapon.ConeDot = Mathf.Max(-1, weapon.ConeDot - steps * 0.1f);
                weapon.Cooldown /= 1 + steps * 0.04f;
                if (weapon.Family == ExpandedWeapon.Fish)
                {
                    weapon.ProjectileRadius *= range;
                    weapon.ProjectileVisualWidth *= range;
                    weapon.ProjectileLifetime *= range;
                }
            }
        }
    }
}
