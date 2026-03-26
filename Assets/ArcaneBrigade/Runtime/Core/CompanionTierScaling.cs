using UnityEngine;

namespace ArcaneBrigade
{
    public static class CompanionTierScaling
    {
        /// <summary>How much bigger a starred friend is drawn (was 1.35 / 1.8: too big, owner 19/10/2026).</summary>
        public static float Body(int tier) => tier >= 3 ? 1.3f : tier == 2 ? 1.15f : 1;
        /// <summary>How much further its shots and areas reach (unchanged gameplay; the body is drawn smaller than this now).</summary>
        public static float Reach(int tier) => tier >= 3 ? 1.8f : tier == 2 ? 1.35f : 1;
        public static void Apply(WeaponDefinition weapon, int tier)
        {
            int steps = Mathf.Clamp(tier - 1, 0, 2);
            float range = 1 + steps * .09f;
            weapon.Range *= range;
            weapon.VisualWidth *= Body(tier);
            if (weapon.Kind == WeaponKind.Projectile)
            {
                weapon.ProjectileCount += steps; weapon.ProjectilePierce += steps;
                weapon.ProjectileRadius *= Reach(tier);
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
