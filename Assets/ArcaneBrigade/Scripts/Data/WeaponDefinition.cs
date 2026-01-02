using UnityEngine;
using ArcaneBrigade.Core;

namespace ArcaneBrigade.Data
{
    /// <summary>
    /// Static description of a weapon. Drives AI behaviour, damage, range and visuals.
    /// Live state (cooldown, current target) lives on the weapon component at runtime.
    /// </summary>
    [CreateAssetMenu(menuName = "Arcane Brigade/Weapon", fileName = "Weapon_")]
    public class WeaponDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string DisplayName;
        public WeaponKind Kind;
        public DamageType DamageType = DamageType.Physical;

        [Header("Damage")]
        [Tooltip("Damage per swing/projectile (before modifiers).")]
        public float BaseDamage = 100f;
        [Tooltip("Attacks per second.")]
        public float AttackRate = 1f;

        [Header("Range")]
        [Tooltip("Max distance at which the weapon can engage (Unity units).")]
        public float MaxRange = 2f;
        [Tooltip("Cleave / cone half-angle in degrees (melee only). 0 = single target.")]
        public float ConeHalfAngleDeg = 0f;

        [Header("Projectile (ranged only)")]
        public GameObject ProjectilePrefab;
        public float ProjectileSpeed = 12f;
        public float ExplosionRadius = 0f;

        [Header("Status / Effects")]
        public float KnockbackForce = 0f;
        public float SlowPercent = 0f;        // applied to enemies on hit
        public float SlowDuration = 0f;
        public float LifestealPercent = 0f;
        public float CritChanceBonus = 0f;

        public bool IsMelee =>
            Kind == WeaponKind.Blade ||
            Kind == WeaponKind.Blunt ||
            Kind == WeaponKind.Piercing ||
            Kind == WeaponKind.Claws ||
            Kind == WeaponKind.Shield;
    }
}
