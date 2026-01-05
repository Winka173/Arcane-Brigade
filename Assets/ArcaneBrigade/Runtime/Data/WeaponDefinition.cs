using UnityEngine;

namespace ArcaneBrigade
{
    public enum WeaponKind { Contact, Melee, Projectile }

    [CreateAssetMenu(menuName = "Arcane Brigade/Weapon")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        public string Id;
        public WeaponKind Kind;
        public Sprite Sprite;
        public float Damage = 12f;
        public float Range = 1.6f;
        public float Cooldown = 0.7f;
        public float Windup = 0.13f;
        public float Recovery = 0.2f;
        public float ProjectileSpeed = 7f;
        public float ProjectileLifetime = 2.4f;
        public float VisualWidth = 0.55f;
        public float MountDistance = 0.34f;
        public float MuzzleDistance = 0.5f;
        public float ConeDot = 0.2f;
        public Color ProjectileColor = Color.cyan;
        public DamageElement Element;
        public WeaponMotion Motion;
        public int ProjectileCount = 1;
        public float SwingAngle = 65f;
        public float RecoilDistance = 0.09f;
        public float BodyAttackTilt = 10f;
        public bool HasMuzzleOffset;
        public Vector2 ProjectileMuzzleOffset;
        public Sprite ProjectileSprite;
        public float ProjectileVisualWidth = 0.22f;
        public SignatureWeapon Signature;
        public int SignatureBeats = 3;
        public float SignatureRadius = 2.2f;
        public float SignatureScale = 0.65f;
        public float SignatureDelay = 0.18f;
        public float MarkLifetime = 2.4f;
        public float PushDistance = 0.35f;
        public ExpandedWeapon Family;
    }

    public enum ExpandedWeapon { None, Tea, TetherBow, Scissors, Camera, Fish, Iron, Hammer, Alarm, MusicBox }

    public enum SignatureWeapon { None, Umbrella, Bell, Stamp }

    public enum DamageElement { Physical, Ember, Frost, Arcane }
    public enum WeaponMotion { Slash, Thrust, Slam, Recoil, Bell, Bow, Cast, Stamp, Scissors, Camera, Iron, Hammer, Music }
}
