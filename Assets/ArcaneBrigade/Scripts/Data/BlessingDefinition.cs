using UnityEngine;

namespace ArcaneBrigade.Data
{
    public enum BlessingSchool
    {
        Battle = 0,   // Red — offensive
        Ward = 1,     // Blue — defensive
        Mystic = 2,   // Purple — utility
        Curse = 3,    // Green — debuffs
    }

    /// <summary>
    /// Static blessing effect. MVP blessings are simple stat modifiers — more complex
    /// trigger-based effects can subclass or use the <c>EffectId</c> hook.
    /// </summary>
    [CreateAssetMenu(menuName = "Arcane Brigade/Blessing", fileName = "Blessing_")]
    public class BlessingDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string DisplayName;
        [TextArea] public string Description;
        public BlessingSchool School;
        public int ShopCost = 15;

        [Header("Stat Modifiers (additive)")]
        public float MeleeDamagePercent = 0f;       // 0.20 = +20%
        public float RangedDamagePercent = 0f;
        public float AllyDamagePercent = 0f;
        public int FlatHealth = 0;
        public float MoveSpeedPercent = 0f;
        public float EnemySlowPercent = 0f;
        public float CritChanceBonus = 0f;

        [Header("Effect Hook")]
        [Tooltip("Optional ID for special-cased effects (e.g. \"frostbite_aura\"). Empty = stat modifiers only.")]
        public string EffectId;
    }
}
