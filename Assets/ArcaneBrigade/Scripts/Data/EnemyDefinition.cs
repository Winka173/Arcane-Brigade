using UnityEngine;
using ArcaneBrigade.Core;

namespace ArcaneBrigade.Data
{
    /// <summary>
    /// Static description of an Enemy / Scourge unit.
    /// </summary>
    [CreateAssetMenu(menuName = "Arcane Brigade/Enemy", fileName = "Enemy_")]
    public class EnemyDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string DisplayName;

        [Header("Base Stats")]
        public int MaxHealth = 100;
        public float MoveSpeed = 2.5f;
        public int GoldReward = 5;

        [Header("Loadout")]
        public WeaponDefinition Weapon;

        [Header("Defense")]
        [Range(0f, 1f)] public float BlockChance = 0f;
        public DamageType Weakness = DamageType.Physical;
        [Range(1f, 5f)] public float WeaknessMultiplier = 1.5f;

        [Header("Visuals (composed primitives)")]
        public Color BodyColor = new Color(0.85f, 0.25f, 0.25f);
        public Color AccentColor = Color.black;
        public PrimitiveShape BodyShape = PrimitiveShape.Hexagon;
        [Range(0.5f, 3f)] public float BodyScale = 1f;
    }
}
