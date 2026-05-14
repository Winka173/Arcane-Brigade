using System;
using System.Collections.Generic;

namespace ArcaneBrigade
{
    /// <summary>The run side of enemy variety (owner 24/10/2026): each map's named elite on the map's third wave, and in Endless
    /// a new trait mixed into the ordinary pool every five waves after the twentieth, so the endless waves keep changing.</summary>
    public static class EnemyVarietyRun
    {
        /// <summary>Endless traits in the order they arrive (one more every five waves from wave 21).</summary>
        public static readonly EnemyBehavior[] EndlessOrder = { EnemyBehavior.Splitter, EnemyBehavior.Armored, EnemyBehavior.Shielder, EnemyBehavior.Blinker, EnemyBehavior.Stomper };
        private static readonly string[] TraitWord = { "Splitting", "Armored", "Shielding", "Blinking", "Stomping" };

        /// <summary>From wave 21 in Endless, a share of the plain melee enemies take on the traits unlocked so far.</summary>
        public static void EndlessTraits(List<UnitDefinition> pool, int wave, bool endless, Func<UnitDefinition, UnitDefinition> clone)
        {
            if (!endless || wave < 21 || pool == null) return;
            int tiers = Math.Min(EndlessOrder.Length, (wave - 21) / 5 + 1), made = 0;
            int count = pool.Count;
            for (int i = 0; i < count && made < tiers * 2; i++)
            {
                UnitDefinition source = pool[i];
                if (source == null || source.IsBoss || source.Behavior != EnemyBehavior.Standard || source.Weapon == null || source.Weapon.Kind != WeaponKind.Contact) continue;
                int t = made % tiers;
                UnitDefinition variant = clone(source);
                variant.Behavior = EndlessOrder[t];
                variant.DisplayName = TraitWord[t] + " " + RunUICopy.UnitName(source);
                pool.Add(variant); made++;
            }
        }

        /// <summary>The map's elite when this is the map's third wave and its art has been built in; otherwise null.</summary>
        public static UnitDefinition MapElite(RunCatalog catalog, int wave, bool endless, Func<UnitDefinition, UnitDefinition> build)
        {
            if (catalog?.MapElites == null || wave > 20 && !endless) return null;
            if (wave - MapIdentity.FirstWave(wave) != 2) return null;
            int map = (int)MapIdentity.For(wave);
            UnitDefinition elite = map < catalog.MapElites.Length ? catalog.MapElites[map] : null;
            return elite == null ? null : build(elite);
        }
    }
}
