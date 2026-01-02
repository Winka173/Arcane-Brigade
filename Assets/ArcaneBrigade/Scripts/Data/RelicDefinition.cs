using UnityEngine;

namespace ArcaneBrigade.Data
{
    public enum RelicRarity
    {
        Common = 0,     // 10g
        Uncommon = 1,   // 25g
        Rare = 2,       // 50g
        Legendary = 3,  // 100g
    }

    /// <summary>
    /// Static relic / artifact. Provides permanent passive effects for a run.
    /// </summary>
    [CreateAssetMenu(menuName = "Arcane Brigade/Relic", fileName = "Relic_")]
    public class RelicDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string DisplayName;
        [TextArea] public string Description;
        public RelicRarity Rarity;

        [Header("Stat Modifiers (additive)")]
        public int FlatAttack = 0;
        public int FlatHealth = 0;
        public float MoveSpeedFlat = 0f;
        public float CritChanceBonus = 0f;
        public float LifestealPercent = 0f;
        public int ManaPerBlessing = 0;

        [Header("Effect Hook")]
        [Tooltip("Optional ID for special-cased effects.")]
        public string EffectId;

        public int ShopCost => Rarity switch
        {
            RelicRarity.Common    => 10,
            RelicRarity.Uncommon  => 25,
            RelicRarity.Rare      => 50,
            RelicRarity.Legendary => 100,
            _                     => 10,
        };
    }
}
