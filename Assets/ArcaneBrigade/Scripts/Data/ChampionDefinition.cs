using UnityEngine;

namespace ArcaneBrigade.Data
{
    /// <summary>
    /// Static description of a playable Champion (Aldric, Zephyr, ...).
    /// </summary>
    [CreateAssetMenu(menuName = "Arcane Brigade/Champion", fileName = "Champion_")]
    public class ChampionDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string DisplayName;
        [TextArea] public string ShortDescription;

        [Header("Base Stats")]
        public int MaxHealth = 45;
        public int AttackPower = 30;
        public float MoveSpeed = 5f;       // unity units / second
        public float CritChance = 0.03f;

        [Header("Loadout")]
        public WeaponDefinition DefaultWeapon;

        [Header("Visuals (composed primitives)")]
        [Tooltip("Body sprite tint.")]
        public Color BodyColor = new Color(0.4f, 0.7f, 1f);
        [Tooltip("Weapon/accent tint.")]
        public Color AccentColor = new Color(1f, 0.85f, 0.3f);
    }
}
