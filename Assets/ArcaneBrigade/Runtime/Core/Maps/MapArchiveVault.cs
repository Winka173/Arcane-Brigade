using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    // The Archive Vault (waves 18-20; Docs/Map_Mechanics_2026-10-25.md section 6). Requirement 25/10/2026: each map's state,
    // tunings and rules live in its own file; MapEnvironmentCombat and MapBoosts only dispatch to the hooks below.

    public sealed partial class MapEnvironmentState
    {
        // Archive Vault: the clock's chime, the shifting shelves, the ledger's ink ring
        public Vector2 Clock; public float NextChime, ChimeAt = -100, SlowUntil = -100, RushUntil = -100; public int Strikes; public bool Twelfth;
        public sealed class Shelf { public Vector2 Home, Position, From, To; public float WarnAt = -100, MoveAt = -100, MoveEnd = -100; public int Moves; }
        public readonly List<Shelf> Shelves = new List<Shelf>();
        public float NextShelf; public int ShelfTurn;
        public Vector2 Ledger; public int LedgerKills;
    }

    public static partial class MapEnvironmentRules
    {
        public const float ChimeEvery = 20, ChimeWarn = 1, ChimeSlow = .45f, ChimeSlowSeconds = 1.5f, RushSpeed = 1.08f, RushHero = 1.15f, RushSeconds = 3;
        public const float ShelfEvery = 16, ShelfWarn = 1.2f, ShelfMove = 1.6f, ShelfHalf = .7f, ShelfRadius = .4f, LedgerRadius = 2.4f;
        public const int LedgerKillsPerDebt = 5, LedgerMaxDebt = 8;
        // requirement 23/10/2026: the chime slows longer with the hero at the clock
        public const float ClockReach = 3f, ClockSlowLonger = 2.5f;
    }

    public sealed partial class CombatSimulation
    {
        private void InitArchiveVault(MapEnvironmentState s)
        {
            Vector2 a = s.Anchor;
            s.Clock = a + new Vector2(0, 4.4f);
            foreach (Vector2 home in new[] { new Vector2(-4.6f, -.6f), new Vector2(4.6f, .8f) })
                s.Shelves.Add(new MapEnvironmentState.Shelf { Home = a + home, Position = a + home, From = a + home, To = a + home });
            s.Ledger = a + new Vector2(mapRandom.Next(2) == 0 ? -6.6f : 6.6f, -3.2f);

            s.NextChime = 9; s.NextShelf = 6;
        }

        // requirement 23/10/2026: nothing on a map blocks or pushes anyone - the moving shelves stay where they stand as scenery
        private void StepArchiveVault(float deltaTime) => StepClock();

        /// <summary>A stat the hero gets while standing somewhere on this map (MapBoosts.MapStandingBoost).</summary>
        private float ArchiveVaultStandingBoost(MapStat stat) => 0;
        /// <summary>How this map changes one enemy along one axis (1 = unchanged): a chime slows enemies, the twelfth strike rushes them.</summary>
        private float ArchiveVaultEnemyFactor(UnitState unit, MapEnemyAxis axis)
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            if (axis != MapEnemyAxis.Speed) return 1;
            if (s.SlowUntil > t) return MapEnvironmentRules.ChimeSlow;
            if (s.RushUntil > t) return MapEnvironmentRules.RushSpeed;
            return 1;
        }
        /// <summary>The attack cooldown factor of the hero or a friend on this map (below 1 = faster).</summary>
        private float ArchiveVaultAllyCycle(UnitState unit) => 1;
        /// <summary>The hero's pace on this map beyond the move-speed boosts: the twelfth strike rushes the hero too.</summary>
        private float ArchiveVaultHeroPace() => mapEnv.RushUntil > Elapsed && mapEnv.SlowUntil <= Elapsed ? MapEnvironmentRules.RushHero : 1;
        /// <summary>A fallen enemy's coins on this map: double inside the ledger's ring (which adds debt).</summary>
        private int ArchiveVaultCoins(UnitState target, int coins)
        {
            MapEnvironmentState s = mapEnv;
            if ((target.Position - s.Ledger).sqrMagnitude < MapEnvironmentRules.LedgerRadius * MapEnvironmentRules.LedgerRadius) { coins *= 2; s.LedgerKills++; }
            return coins;
        }
        /// <summary>Debt owed by the ledger after this wave (each point: +1% enemies next wave).</summary>
        public int LedgerDebt => mapEnv == null ? 0 : Mathf.Min(MapEnvironmentRules.LedgerMaxDebt, mapEnv.LedgerKills / MapEnvironmentRules.LedgerKillsPerDebt);

        private void StepClock()
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            s.Shown.Add(MapIdentity.Element.Ledger);
            if (t >= s.NextChime - MapEnvironmentRules.ChimeWarn && s.ChimeAt < s.NextChime - MapEnvironmentRules.ChimeWarn - .01f)
            {
                s.ChimeAt = s.NextChime; s.Twelfth = s.Strikes == 1; // the second chime of a wave is the twelfth strike
                s.Shown.Add(MapIdentity.Element.Clock);
            }
            if (t >= s.NextChime)
            {
                s.Strikes++; s.NextChime = t + MapEnvironmentRules.ChimeEvery;
                bool atClock = (Champion.Position - s.Clock).sqrMagnitude < MapEnvironmentRules.ClockReach * MapEnvironmentRules.ClockReach;
                if (s.Twelfth) s.RushUntil = t + MapEnvironmentRules.RushSeconds; else s.SlowUntil = t + MapEnvironmentRules.ChimeSlowSeconds * (atClock ? MapEnvironmentRules.ClockSlowLonger : 1);
            }
        }

        private void StepShelves()
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            if (t >= s.NextShelf && s.Shelves.Count > 0)
            {
                s.NextShelf = t + MapEnvironmentRules.ShelfEvery;
                MapEnvironmentState.Shelf shelf = s.Shelves[s.ShelfTurn++ % s.Shelves.Count];
                Vector2[] steps = { new Vector2(0, 2.4f), new Vector2(2.6f, 0), new Vector2(0, -2.4f), new Vector2(-2.6f, 0) };
                Vector2 to = shelf.Home + (shelf.Moves % 2 == 0 ? steps[(shelf.Moves / 2 + (shelf.Home.x < s.Anchor.x ? 0 : 2)) % 4] : Vector2.zero);
                shelf.From = shelf.Position; shelf.To = ClampPosition(to, 1); shelf.Moves++;
                shelf.WarnAt = t; shelf.MoveAt = t + MapEnvironmentRules.ShelfWarn; shelf.MoveEnd = shelf.MoveAt + MapEnvironmentRules.ShelfMove;
                s.Shown.Add(MapIdentity.Element.Shelves);
            }
            foreach (MapEnvironmentState.Shelf shelf in s.Shelves)
                if (t >= shelf.MoveAt && shelf.MoveEnd > shelf.MoveAt)
                    shelf.Position = Vector2.Lerp(shelf.From, shelf.To, Mathf.SmoothStep(0, 1, Mathf.Clamp01((t - shelf.MoveAt) / MapEnvironmentRules.ShelfMove)));
        }

        /// <summary>For captures: the clock about to strike twelve, a shelf about to move and the ledger by the hero.</summary>
        private void ArchiveVaultCaptureFixture()
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            s.Clock = Champion.Position + new Vector2(-5.2f, .4f);
            s.ChimeAt = t + .4f; s.NextChime = t + .4f; s.Twelfth = true; s.Strikes = 1;
            s.Shelves[0].Home = s.Shelves[0].From = s.Shelves[0].Position = Champion.Position + new Vector2(-3.6f, -1);
            s.Shelves[0].To = s.Shelves[0].From + new Vector2(0, 2.4f); s.Shelves[0].WarnAt = t - .4f; s.Shelves[0].MoveAt = t + .8f; s.Shelves[0].MoveEnd = t + 2.4f;
            s.Shelves[1].Home = s.Shelves[1].From = s.Shelves[1].To = s.Shelves[1].Position = Champion.Position + new Vector2(4.2f, .2f);
            s.Ledger = Champion.Position + new Vector2(2.6f, -2.6f); s.NextShelf = t + 30;
            s.Shown.Add(MapIdentity.Element.Shelves); s.Shown.Add(MapIdentity.Element.Clock);
        }
    }
}
