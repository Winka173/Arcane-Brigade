using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class RunSnapshot
    {
        /// <summary>Rainbow Threads held: each one weaves a 4-star friend into its 5th, rainbow star.</summary>
        public int RainbowThreads;
    }

    /// <summary>The fifth star (requirement 27/10/2026): the shop's copies stop at 4 stars (purple); the rainbow star only comes from a
    /// Rainbow Thread, a rare prize of special and hard events (a secret room won on Nightmare or harder, the Unclaimed Gift Keeper
    /// from night 20, a boss beaten on Sleepless or harder from night 30). A thread waits until a friend reaches 4 stars, then weaves it.</summary>
    public sealed partial class RunSession
    {
        public const int RainbowFirstGiftWave = 20, RainbowBossWave = 30, RainbowKeeperChance = 25;

        private RosterEntry RainbowCandidate()
        {
            foreach (RosterEntry entry in ActiveRoster)
                if (entry.Tier == CompanionDeployment.ShopMaxTier && catalog.Unit(entry.UnitId) != null && catalog.Unit(entry.UnitId).ReferenceCompanion != "Evil" && !StorySpecies(entry.UnitId)) return entry;
            return null;
        }

        /// <summary>Hands over a thread (and weaves it at once when a 4-star friend is ready); returns true when a friend was woven.</summary>
        public bool GiveRainbowThread()
        {
            State.RainbowThreads = BoundedCounter((long)State.RainbowThreads + 1);
            Notice = "A Starlight Thread. A 4-star friend can shine as never before.";
            return WeaveRainbow();
        }

        /// <summary>Weaves held threads into ready 4-star friends; runs after every merge too, so a thread never waits needlessly.</summary>
        public bool WeaveRainbow()
        {
            bool any = false;
            while (State.RainbowThreads > 0)
            {
                RosterEntry entry = RainbowCandidate();
                if (entry == null) break;
                entry.Tier = catalog.MaximumTier; State.RainbowThreads--; RememberVariant(entry); any = true;
                Notice = RunUICopy.UnitName(catalog.Unit(entry.UnitId)) + " shines as never before: 5 stars.";
            }
            if (any) RecountBonds();
            return any;
        }

        /// <summary>Merges stored copies and weaves held threads (for checks and tools).</summary>
        public void Rescan() => RecountBonds();

        /// <summary>The hard-event rule: a boss beaten late on a high difficulty leaves a thread.</summary>
        private void RainbowBossGift()
        {
            if (State.Difficulty >= 3 && State.Wave >= RainbowBossWave) GiveRainbowThread();
        }
    }
}
