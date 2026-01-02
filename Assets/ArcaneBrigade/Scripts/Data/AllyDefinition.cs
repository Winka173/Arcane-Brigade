using UnityEngine;
using ArcaneBrigade.Core;

namespace ArcaneBrigade.Data
{
    /// <summary>
    /// Static description of an Ally / Vassal. Weapon is FIXED per ally type — see
    /// design doc Section 6 ("Ally Weapon Framework").
    /// </summary>
    [CreateAssetMenu(menuName = "Arcane Brigade/Ally", fileName = "Ally_")]
    public class AllyDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string DisplayName;
        public Faction Faction;

        [Header("Base Stats")]
        public int MaxHealth = 80;
        public float MoveSpeed = 3.5f;

        [Header("Loadout")]
        public WeaponDefinition Weapon;

        [Header("Shop")]
        public int ShopCost = 10;

        [Header("Visuals (composed primitives)")]
        public Color BodyColor = Color.white;
        public Color AccentColor = Color.gray;

        [Header("Sprite Composition")]
        [Tooltip("Primary primitive shape — composed from Sprite Editor primitives at runtime.")]
        public PrimitiveShape BodyShape = PrimitiveShape.Circle;
    }

    /// <summary>Maps to Unity's built-in 2D primitive sprites (GameObject > 2D Object > Sprites).</summary>
    public enum PrimitiveShape
    {
        Square = 0,
        Circle = 1,
        Triangle = 2,
        Diamond = 3,
        Hexagon = 4,
        Capsule = 5,
    }
}
