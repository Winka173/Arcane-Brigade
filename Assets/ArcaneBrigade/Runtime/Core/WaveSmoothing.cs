using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Requirement 4/11/2026: some nights jumped in strength (waves 4, 7, 10: the crowd grew, was cut at wave 5 and capped
    /// at wave 10, and each map's species differ in health), and there were too many small enemies. After a night is set up its
    /// swarm is laid on smooth curves: fewer enemies, the big ones three times as likely, the swarm's total health growing by the
    /// same step every night and its blows likewise. Bosses, elites and secret fights keep their own numbers; pacts, visitors and
    /// relics still change the count as before.</summary>
    public static class WaveSmoothingRules
    {
        /// <summary>Enemies in a night at Peaceful before pacts (wave 1: 56, wave 19: about 101).</summary>
        public static float Count(int wave) => 56f * (1 + .045f * (wave - 1));
        /// <summary>The swarm's whole health at Peaceful: wave 1 3,160, growing a little faster early and easing late (wave 10
        /// about 61k, wave 19 about 663k), the old curve without its jumps.</summary>
        public static float TotalHealth(int wave) { float n = wave - 1; return 3160f * Mathf.Exp(.36f * n - .0035f * n * n); }
        /// <summary>An average enemy's contact blow at Peaceful (wave 1: about 5.8; each night 11% more); a fifth more than
        /// before since there are fewer of them.</summary>
        public static float Blow(int wave) => 4.8f * 1.2f * Mathf.Pow(1.113f, wave - 1);
        /// <summary>How much more often a big enemy is chosen than a small one.</summary>
        public const int BigWeight = 3;
        public static bool Big(UnitDefinition unit) => unit != null && (unit.BodyHeight >= 1.5f || unit.Radius >= .6f);
    }

    public sealed partial class RunSession
    {
        private int smoothBudgetBase;

        private void SmoothWave(bool actOne)
        {
            CheckpointCatalog c = CombatCatalog;
            if (!actOne || State.Endless || SecretFighting || c.EnemyBudget <= 0 || c.EnemyPool == null || c.EnemyPool.Length == 0) return;
            int w = State.Wave, d = State.Difficulty;
            float density = JourneyBalance.EnemyDensity(d, w);
            float pacts = smoothBudgetBase > 0 ? c.EnemyBudget / (float)smoothBudgetBase : 1;
            float count = WaveSmoothingRules.Count(w) * density;
            int before = c.EnemyBudget;
            c.EnemyBudget = Mathf.Max(8, Mathf.CeilToInt(count * pacts));
            // fewer enemies must not mean less gold: each coin is worth what the old crowd paid in all (bot campaign 4/11/2026:
            // half the income left teams too weak for the bosses on waves 8, 12 and 16)
            c.CoinValueScale *= Mathf.Clamp(before / Mathf.Max(1f, c.EnemyBudget), 1, 5);
            if (c.BossSwarmBudget > 0) c.BossSwarmBudget = Mathf.CeilToInt(c.EnemyBudget * 0.22f); // the boss night's light swarm follows
            c.SpawnInterval = Mathf.Clamp(c.SpawnInterval * before / Mathf.Max(1f, c.EnemyBudget), c.SpawnInterval, 2.5f); // the fewer enemies come over the same night

            // the big ones three times as likely
            var pool = new List<UnitDefinition>(c.EnemyPool);
            bool small = pool.Exists(u => u != null && !WaveSmoothingRules.Big(u));
            if (small)
                foreach (UnitDefinition unit in c.EnemyPool)
                    if (WaveSmoothingRules.Big(unit)) for (int i = 1; i < WaveSmoothingRules.BigWeight; i++) pool.Add(unit);
            c.EnemyPool = pool.ToArray();

            float health = 0, blow = 0; int n = 0;
            foreach (UnitDefinition unit in pool) if (unit != null) { health += unit.Health; blow += unit.ContactDamage; n++; }
            if (n == 0 || health <= 0) return;
            health /= n; blow /= n;
            float healthShape = JourneyBalance.EnemyHealth(d, w) / JourneyBalance.EnemyHealth(0, w);
            float damageShape = JourneyBalance.EnemyDamage(d, w) / JourneyBalance.EnemyDamage(0, w);
            float healthScale = WaveSmoothingRules.TotalHealth(w) / WaveSmoothingRules.Count(w) * healthShape / health;
            float blowScale = blow > 0 ? WaveSmoothingRules.Blow(w) * damageShape / blow : 1;

            var units = new HashSet<UnitDefinition>(); var weapons = new HashSet<WeaponDefinition>();
            void Add(UnitDefinition unit) { if (unit != null && units.Add(unit) && unit.Weapon != null) weapons.Add(unit.Weapon); }
            foreach (UnitDefinition unit in pool) Add(unit);
            foreach (UnitDefinition unit in c.HerdPool ?? System.Array.Empty<UnitDefinition>()) Add(unit);
            foreach (UnitDefinition unit in c.AffixedEnemies ?? System.Array.Empty<UnitDefinition>()) Add(unit);
            Add(c.MeleeEnemy); Add(c.RangedEnemy);
            units.Remove(c.Boss); units.Remove(c.MidBoss); units.Remove(c.MapElite);
            foreach (UnitDefinition unit in units) { unit.Health *= healthScale; unit.ContactDamage *= blowScale; }
            if (c.Boss?.Weapon != null) weapons.Remove(c.Boss.Weapon);
            if (c.MidBoss?.Weapon != null) weapons.Remove(c.MidBoss.Weapon);
            if (c.MapElite?.Weapon != null) weapons.Remove(c.MapElite.Weapon);
            foreach (WeaponDefinition weapon in weapons) weapon.Damage *= blowScale;
        }
    }
}
