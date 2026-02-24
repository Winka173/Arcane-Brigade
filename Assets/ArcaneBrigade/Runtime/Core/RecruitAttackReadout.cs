using System;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class RecruitAttackReadout
    {
        public int Choices;
        public float MinDamage = float.PositiveInfinity, MaxDamage, MinCycle = float.PositiveInfinity, MaxCycle, MinRange = float.PositiveInfinity, MaxRange;
        public bool Melee, Ranged;
        public Sprite Icon;
        public static string Range(float low, float high) => Mathf.Approximately(low, high) ? low.ToString("0.##") : low.ToString("0.##") + "–" + high.ToString("0.##");
    }

    public sealed partial class RunSession
    {
        // Inspection never spends currency, rolls a weapon, advances RNG or changes the live build.
        // Bounds describe eligible weapons with the CURRENT build, before this purchase/merge.
        public RecruitAttackReadout InspectRecruitAttack(string id, int tier)
        {
            var result = new RecruitAttackReadout();
            using var scratch = new RunSession(catalog); scratch.Restore(State);
            scratch.CombatCatalog = scratch.Clone(catalog.Arena);
            var hero = catalog.Unit(State.ChampionId);
            foreach (var weapon in catalog.Weapons)
            {
                if (!RecruitWeaponEligible(id, weapon) || State.Roster.Exists(r => r.UnitId == id && r.WeaponId == weapon.Id)) continue;
                scratch.CombatCatalog.Champion = scratch.BuildUnit(hero, hero.Weapon, 1, false);
                var unit = scratch.BuildUnit(catalog.Unit(id), weapon, tier, true);
                scratch.CombatCatalog.StartingCompanions = new[] { unit };
                scratch.CompileOctoberRoster();
                var built = unit.Weapon;
                if (result.Icon == null || weapon == catalog.Unit(id).Weapon) result.Icon = weapon.Sprite;
                result.Choices++;
                result.Melee |= built.Kind != WeaponKind.Projectile; result.Ranged |= built.Kind == WeaponKind.Projectile;
                result.MinDamage = Mathf.Min(result.MinDamage, built.Damage); result.MaxDamage = Mathf.Max(result.MaxDamage, built.Damage);
                result.MinCycle = Mathf.Min(result.MinCycle, built.Cooldown); result.MaxCycle = Mathf.Max(result.MaxCycle, built.Cooldown);
                result.MinRange = Mathf.Min(result.MinRange, built.Range); result.MaxRange = Mathf.Max(result.MaxRange, built.Range);
            }
            if (result.Choices == 0) result.MinDamage = result.MinCycle = result.MinRange = 0;
            return result;
        }
    }
}
