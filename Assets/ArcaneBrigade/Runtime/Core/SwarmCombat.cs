using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class CombatSimulation
    {
        private int swarmSpawns;
        private int swarmGoldKills;
        public int SwarmSpawnCount => swarmSpawns;
        private UnitDefinition SwarmDefinition()
        {
            UnitDefinition fallback = null;
            foreach (UnitDefinition candidate in catalog.EnemyPool)
                if (!candidate.IsBoss && candidate.SpecialEnemy == SpecialEnemyKind.None && candidate.Weapon.Kind != WeaponKind.Projectile && candidate.Behavior == EnemyBehavior.Standard && (fallback == null || candidate.BodyHeight < fallback.BodyHeight)) fallback = candidate;
            return fallback ?? catalog.MeleeEnemy;
        }
        private int SwarmCoinValue(UnitState target)
        {
            if (target.Variant != null && target.Variant.Summoned) return 0;
            if (!target.SwarmMinion) return catalog.GoldPerKill;
            swarmGoldKills++;
            return swarmGoldKills % 6 == 0 ? catalog.GoldPerKill : 0;
        }
    }
}
