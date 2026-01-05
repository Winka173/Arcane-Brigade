using UnityEngine;

namespace ArcaneBrigade
{
    public enum Faction { Champion, Companion, Enemy }

    [CreateAssetMenu(menuName = "Arcane Brigade/Unit")]
    public sealed class UnitDefinition : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public Sprite Body;
        public WeaponDefinition Weapon;
        public float Health = 100f;
        public float Speed = 4.8f;
        public float Radius = 0.25f;
        public float BodyHeight = 0.8f;
        public float ContactDamage = 4f;
        public float ContactInterval = 0.8f;
        public float CriticalChance;
        public Color Tint = Color.white;
        public string[] Tags = new string[0];
        public PassiveKind Passive;
        public float PassiveStrength = 1f;
        public string Description;
        public int Price = 20;
        public int UnlockWave;
        public bool IsBoss;
        public int BossPattern;
        public int CombatTier = 1;
    }

    public enum PassiveKind { None, HealChampion, Scavenge, Rally, ChillAura }
}
