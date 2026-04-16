using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class CombatSimulation
    {
        private int swarmSpawns;
        private int swarmGoldKills;
        private long pendingKillCoinHundredths;
        private int pendingKillCoinKills;
        private Vector2 pendingKillCoinPosition;
        public int SwarmSpawnCount => swarmSpawns;
        private UnitDefinition SwarmDefinition()
        {
            UnitDefinition fallback = null;
            foreach (UnitDefinition candidate in catalog.HerdPool ?? catalog.EnemyPool) // the swarm's fodder is not themed (item 24)
                if (!candidate.IsBoss && candidate.SpecialEnemy == SpecialEnemyKind.None && candidate.WaitingKind == WaitingEnemyKind.None && candidate.Weapon.Kind != WeaponKind.Projectile && candidate.Behavior == EnemyBehavior.Standard && (fallback == null || candidate.BodyHeight < fallback.BodyHeight)) fallback = candidate;
            return fallback ?? catalog.MeleeEnemy;
        }
        private UnitDefinition HerdDefinition()
        {
            var choices = new System.Collections.Generic.List<UnitDefinition>();
            foreach (UnitDefinition candidate in catalog.HerdPool ?? catalog.EnemyPool) // the swarm's fodder is not themed (item 24)
                if (!candidate.IsBoss && candidate.SpecialEnemy == SpecialEnemyKind.None && candidate.WaitingKind == WaitingEnemyKind.None && candidate.Weapon.Kind != WeaponKind.Projectile && candidate.Behavior == EnemyBehavior.Standard && candidate.BodyHeight <= 1.15f && !choices.Contains(candidate)) choices.Add(candidate);
            return choices.Count == 0 ? SwarmDefinition() : choices[spawnRandom.Next(choices.Count)];
        }
        private UnitDefinition PackEscortDefinition(UnitDefinition herd)
        {
            UnitDefinition selected = null, largest = null;
            int choices = 0;
            foreach (UnitDefinition candidate in catalog.EnemyPool)
            {
                if (candidate.IsBoss || candidate.SpecialEnemy != SpecialEnemyKind.None || candidate.WaitingKind != WaitingEnemyKind.None) continue;
                if (largest == null || candidate.BodyHeight > largest.BodyHeight) largest = candidate;
                if (candidate.BodyHeight < herd.BodyHeight * 1.12f) continue;
                if (spawnRandom.Next(++choices) == 0) selected = candidate;
            }
            return selected ?? largest ?? catalog.MeleeEnemy;
        }

        private void DropKillCoins(UnitState target, int value)
        {
            if (value <= 0) return;
            pendingKillCoinHundredths += (long)value * Mathf.Clamp(catalog.KillCoinRewardPercent, 0, 100);
            pendingKillCoinPosition = target.Position;
            pendingKillCoinKills++;
            if (pendingKillCoinKills >= Mathf.Max(1, catalog.KillCoinBatchSize) || target.Definition.IsBoss || target.Elite)
                FlushKillCoins();
        }

        private void FlushKillCoins()
        {
            int value = RunSession.BoundedCounter(pendingKillCoinHundredths / 100);
            if (value > 0)
            {
                pendingKillCoinHundredths -= (long)value * 100;
                DropLoot(LootKind.Coin, pendingKillCoinPosition, value);
            }
            pendingKillCoinKills = 0;
        }

        private int EscortCoinBonus(int oldSmall, int newSmall, int oldLeaders, int leaders, int member)
        {
            int missing = Mathf.Max(0, (oldSmall - newSmall) * catalog.MinionCoin + (oldLeaders - leaders) * catalog.GoldPerKill);
            return missing / leaders + (member < missing % leaders ? 1 : 0);
        }

        private int SwarmCoinValue(UnitState target)
        {
            if (target.Variant != null && target.Variant.Summoned) return 0;
            if (!target.SwarmMinion) return RunSession.BoundedCounter((long)catalog.GoldPerKill + target.PackCoinBonus);
            swarmGoldKills++;
            return Mathf.Max(0, catalog.MinionCoin);
        }
    }
}
