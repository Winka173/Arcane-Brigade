using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class CheckpointCatalog
    {
        /// <summary>Filled by RunSession.PrepareCombat (MapIdentityRun): the map this fight belongs to (its enemy family and elite
        /// affix), or -1 for a fight outside the maps (secret rooms, the Keeper's test, the warm chair).</summary>
        [NonSerialized] public int MapThemed = -1;
        /// <summary>The wave's unthemed pool: the swarm's small fodder is drawn from it, so a map's theme changes who leads the
        /// packs, not how dense or tough the swarm is (phase D sweep).</summary>
        [NonSerialized] public UnitDefinition[] HerdPool;
    }

    /// <summary>Where the four map identities join a run (play-test item 24): the wave's enemy pool is themed to its map, and the
    /// map's living elements run in its ordinary waves.</summary>
    public sealed partial class RunSession
    {
        private void PrepareMapIdentity(bool bossWave)
        {
            CombatCatalog.MapThemed = CombatCatalog.MapEnvironment = -1;
            bool special = SecretRoomOpen || State.OutsideStage == OutsideStageStatus.Fighting || (State.ActOne?.SitCloser ?? 0) == ActOneProgress.Fighting;
            if (!CombatCatalog.ExpandedContent || special) return;
            MapTheme map = MapIdentity.For(State.Wave);
            CombatCatalog.MapThemed = MapIdentity.Off("pool") ? -1 : (int)map;
            CombatCatalog.HerdPool = null;
            if (CombatCatalog.EnemyPool != null && CombatCatalog.EnemyPool.Length > 0 && !MapIdentity.Off("pool"))
            {
                CombatCatalog.HerdPool = CombatCatalog.EnemyPool;
                CombatCatalog.EnemyPool = MapIdentity.Theme(new List<UnitDefinition>(CombatCatalog.EnemyPool), State.Wave, State.Seed).ToArray();
            }
            if (bossWave || CombatCatalog.Boss != null || State.Wave < 2 || MapIdentity.Off("env")) return;
            CombatCatalog.MapEnvironment = (int)map;
        }
    }
}
