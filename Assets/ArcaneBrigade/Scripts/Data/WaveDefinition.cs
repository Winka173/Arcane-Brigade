using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade.Data
{
    /// <summary>
    /// Defines a single wave: ordered list of spawn entries with per-entry counts
    /// and delays. The wave manager schedules spawns and ends the wave when all
    /// spawned enemies are dead.
    /// </summary>
    [CreateAssetMenu(menuName = "Arcane Brigade/Wave", fileName = "Wave_")]
    public class WaveDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string DisplayName;
        public bool IsBossWave;
        public int GoldBonusOnClear = 20;

        [Header("Spawn Plan")]
        public List<SpawnEntry> Spawns = new();

        [Serializable]
        public struct SpawnEntry
        {
            public EnemyDefinition Enemy;
            [Min(1)]    public int Count;
            [Min(0f)]   public float DelayBeforeFirstSeconds;
            [Min(0.1f)] public float IntervalBetweenSeconds;
        }
    }
}
