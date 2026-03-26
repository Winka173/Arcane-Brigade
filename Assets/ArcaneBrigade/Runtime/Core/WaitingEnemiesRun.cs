using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class CheckpointCatalog
    {
        /// <summary>Filled by RunSession.PrepareCombat: lanterns relit in Act I (the Lamp Snuffer goes only for lit ones).</summary>
        [NonSerialized] public int LanternRelitMask;
    }

    /// <summary>Where the enemy roster v2 joins a run: each new enemy whose art exists walks into its own zone's waves
    /// (WaitingEnemyRoster.InRegularWaves); the Waking Sitters rise only in the hidden SIT CLOSER fight.</summary>
    public sealed partial class RunSession
    {
        private void AddWaitingEnemies(List<UnitDefinition> pool, float scaling)
        {
            WaitingEnemyRoster roster = WaitingEnemyRoster.Current;
            if (roster == null || roster.Enemies == null) return;
            foreach (UnitDefinition enemy in roster.Enemies)
                if (enemy != null && enemy.Weapon != null && WaitingEnemyRoster.InRegularWaves(enemy.WaitingKind, State.Wave, State.Endless))
                    pool.Add(BuildEnemy(enemy, scaling));
        }

        /// <summary>After the encounter is set: lantern state for the Snuffers, and the seated travellers the Bedside wakes.</summary>
        private void PrepareWaitingEnemies()
        {
            CombatCatalog.LanternRelitMask = State.ActOne?.RelitMask ?? 0;
            if ((State.ActOne?.SitCloser ?? 0) != ActOneProgress.Fighting || !CombatCatalog.ExpandedContent) return;
            UnitDefinition sitter = Array.Find(WaitingEnemyRoster.Current?.Enemies ?? Array.Empty<UnitDefinition>(), unit => unit != null && unit.Weapon != null && unit.WaitingKind == WaitingEnemyKind.WakingSitter);
            if (sitter == null) return;
            int real = State.Wave;
            State.Wave = Mathf.Min(real + ActOneCanon.SitWaveBonus, 40); // as strong as the rest of the fight (PrepareSitCloser)
            UnitDefinition built;
            try { built = BuildEnemy(sitter, (1f + (State.Wave - 1) * 0.10f) * JourneyBalance.EnemyHealth(State.Difficulty, State.Wave)); }
            finally { State.Wave = real; }
            var pool = new List<UnitDefinition>(CombatCatalog.EnemyPool ?? Array.Empty<UnitDefinition>()) { built, built };
            CombatCatalog.EnemyPool = pool.ToArray();
        }
    }
}
