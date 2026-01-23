using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class ImpactAnimationLibrary : ScriptableObject
    {
        public Sprite[] Cut;
        public Sprite[] Slam;
        public Sprite[] Pierce;
        public Sprite[] Orb;
        public Sprite[] For(WeaponDefinition weapon)
        {
            if (weapon == null) return Pierce;
            if (weapon.Motion == WeaponMotion.Hammer || weapon.Motion == WeaponMotion.Slam || weapon.Mechanic == CarnivalWeapon.SpringHammer) return Slam;
            if (weapon.Motion == WeaponMotion.Bow || weapon.Mechanic == CarnivalWeapon.Sewing || weapon.Kind == WeaponKind.Projectile && weapon.Element == DamageElement.Physical) return Pierce;
            return weapon.Kind == WeaponKind.Melee && weapon.Family != ExpandedWeapon.Fish ? Cut : Orb;
        }
    }
}
