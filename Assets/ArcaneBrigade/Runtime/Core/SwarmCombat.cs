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
            // owner 23/10/2026: coins are thinned only when the field is crowded, never in a quiet early fight - every kill
            // pays in full under 30 living enemies; a coin per 4 / 8 / 12 kills (90% of the reward) at 30 / 60 / 90 and more
            int alive = 0; foreach (UnitState u in units) if (u.Alive && u.Faction == Faction.Enemy) alive++;
            int batch = alive < 30 ? 1 : alive < 60 ? 4 : alive < 90 ? 8 : 12;
            pendingKillCoinHundredths += (long)value * (alive < 30 ? 100 : Mathf.Clamp(catalog.KillCoinRewardPercent, 0, 100));
            pendingKillCoinPosition = target.Position;
            pendingKillCoinKills++;
            if (pendingKillCoinKills >= batch || target.Definition.IsBoss || target.Elite)
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
