using UnityEngine;
namespace ArcaneBrigade
{
    /// <summary>Final friendly reach, after stars, items and hero bonuses. Long weapons keep an advantage without
    /// reaching across the arena. Blast radii and explicit chain hops remain separate from direct shot travel.</summary>
    public static class FriendlyReach
    {
        public const float MeleeMaximum = 2.6f, RangedMaximum = 6.5f;
        /// <summary>How far a friendly shot flies, as a share of its weapon's attack range: the range decides when a friend
        /// fires, the shot then keeps going (requirement 31/10/2026: shots vanished at the end of the range, mid-flight).</summary>
        public const float FlightReach = 2.5f;
        public static float Limit(float range, WeaponKind kind)
        {
            float knee = kind == WeaponKind.Melee ? 2.2f : 5f;
            float slope = kind == WeaponKind.Melee ? .25f : .35f;
            return Mathf.Clamp(range <= knee ? range : knee + (range - knee) * slope, .4f,
                kind == WeaponKind.Melee ? MeleeMaximum : RangedMaximum);
        }
        public static float Flight(WeaponDefinition weapon, float muzzleOffset, float maximum = float.PositiveInfinity)
        {
            float range = Mathf.Min(weapon.Range, maximum);
            return Mathf.Max(.1f, range * FlightReach - Mathf.Min(muzzleOffset, range * .35f));
        }
        public static bool Returns(WeaponDefinition weapon) => weapon.Family == ExpandedWeapon.Fish || weapon.SecretRecall;
    }
}
