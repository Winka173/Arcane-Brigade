using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Maximum HP given up at the altar (balance pass round 4, Docs/Balance_Pass_2026-10-19.md). Old saves read 0.</summary>
    public sealed partial class RunSnapshot { public float BloodPaid; }

    /// <summary>Round 4 rule: an altar item paid in HP lowers MAXIMUM HP for the rest of the run, not just current HP. The cut is
    /// taken from the compiled maximum (after items, bonds, corrections and the late-game cap), so "−N max HP" is literally N less
    /// than the hero would otherwise have. A purchase may not push the maximum below a quarter of the uncut maximum (at least 5).</summary>
    public static class BloodPrice
    {
        public const float FloorShare = .25f;
        public const float MinimumFloor = 5;
        public static float Floor(float uncutMaximum) => Mathf.Max(MinimumFloor, uncutMaximum * FloorShare);
        public static float Apply(float uncutMaximum, float paid) => Mathf.Max(Mathf.Min(uncutMaximum, Floor(uncutMaximum)), uncutMaximum - Mathf.Max(0, paid));
        /// <summary>A price in HP is affordable when it leaves at least 1 current HP and keeps the maximum on or above the floor.</summary>
        public static bool Affordable(float currentHealth, float uncutMaximum, float paid, int price)
            => currentHealth - price >= 1f && uncutMaximum - Mathf.Max(0, paid) - price >= Floor(uncutMaximum);
    }

    public sealed partial class RunSession
    {
        /// <summary>Maximum HP before the altar's cut.</summary>
        public float UncutMaximumHealth => catalog.StatRevision > 0 ? K10StatRules.UncutMaximumHealth(this, catalog) : Mathf.Max(1f, catalog.Unit(State.ChampionId).Health + Bonus(ItemEffect.Health));
        public float MaximumHealthNow => MaximumHealth();
        public bool BloodAffordable(int price) => BloodPrice.Affordable(State.Health, UncutMaximumHealth, State.BloodPaid, price);
    }
}
