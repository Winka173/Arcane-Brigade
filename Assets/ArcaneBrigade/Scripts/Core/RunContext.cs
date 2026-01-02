using System.Collections.Generic;
using ArcaneBrigade.Data;

namespace ArcaneBrigade.Core
{
    /// <summary>
    /// Plain-C# container for everything that lives for the duration of one run.
    /// Reset between runs. Modified by shop purchases, kills, wave clears.
    /// </summary>
    public class RunContext
    {
        public ChampionDefinition Champion;
        public int Gold;
        public int WaveIndex;          // 0-based; wave 1 == index 0
        public int EnemiesKilled;
        public int ShopRefreshCount;   // cost escalates with refreshes

        public readonly List<BlessingDefinition> Blessings = new();
        public readonly List<RelicDefinition> Relics = new();
        public readonly List<AllyDefinition> Allies = new();

        public void Reset(ChampionDefinition champion)
        {
            Champion = champion;
            Gold = 0;
            WaveIndex = 0;
            EnemiesKilled = 0;
            ShopRefreshCount = 0;
            Blessings.Clear();
            Relics.Clear();
            Allies.Clear();
        }
    }
}
