using System;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>One side mechanic of a map: everything a map does beyond its main one.</summary>
    public enum MapSide
    {
        None, All,
        RoomTickets, RoomDust, RoomWaiters, RoomClock,
        PostWind, PostStamps, PostParcel, PostPlanes,
        GlassCarriers, GlassPanes, GlassBuds,
        LampOil, LampLantern,
        TheatreAudience, TheatreCurtain, TheatreFirework,
        VaultPages, VaultWorms, VaultPot, VaultStamp,
    }

    /// <summary>Fewer things at once (requirement 1/11/2026: the maps' many mechanics on top of the foes made a fight hard to read).
    /// Each map keeps its main mechanic always on (the Waiting Room's number board, the Post Office's letter chute, the Glasshouse's
    /// sun, rain and blooms, the Lamp Hall's lamps, the Theatre's spotlight, the Archive Vault's ink fog). Its other mechanics take
    /// turns, one per wave: a map's first wave has its main mechanic alone, each later wave adds one side mechanic. While a boss or
    /// an elite is on the field the side mechanic starts nothing new.</summary>
    public static class MapPacing
    {
        private static readonly MapSide[] RoomSides = { MapSide.RoomTickets, MapSide.RoomDust, MapSide.RoomWaiters, MapSide.RoomClock };
        private static readonly MapSide[] PostSides = { MapSide.PostWind, MapSide.PostStamps, MapSide.PostPlanes, MapSide.PostParcel };
        private static readonly MapSide[] GlassSides = { MapSide.GlassPanes, MapSide.GlassBuds, MapSide.GlassCarriers };
        private static readonly MapSide[] LampSides = { MapSide.LampOil, MapSide.LampLantern };
        private static readonly MapSide[] TheatreSides = { MapSide.TheatreAudience, MapSide.TheatreFirework, MapSide.TheatreCurtain };
        private static readonly MapSide[] VaultSides = { MapSide.VaultPages, MapSide.VaultWorms, MapSide.VaultStamp, MapSide.VaultPot };

        private static MapSide[] SidesOf(MapTheme map) => map switch
        {
            MapTheme.WaitingRoom => RoomSides,
            MapTheme.PostOffice => PostSides,
            MapTheme.Glasshouse => GlassSides,
            MapTheme.LampHall => LampSides,
            MapTheme.Theatre => TheatreSides,
            MapTheme.ArchiveVault => VaultSides,
            _ => Array.Empty<MapSide>()
        };

        /// <summary>The side mechanic of this wave: none on the map's first fighting wave, then one per wave, starting from a place
        /// the run's seed picks so runs differ.</summary>
        public static MapSide For(MapTheme map, int wave, int seed)
        {
            MapSide[] sides = SidesOf(map);
            if (sides.Length == 0) return MapSide.None;
            int before = 0;
            for (int w = Mathf.Max(2, MapIdentity.FirstWave(wave)); w < wave; w++) if (!ActOneCanon.BossWave(w)) before++;
            if (before == 0) return MapSide.None;
            int start = Mathf.Abs(seed * 7 + (int)map * 13) % sides.Length;
            return sides[(start + before - 1) % sides.Length];
        }

        /// <summary>The wave-start line naming the side mechanic.</summary>
        public static string Name(MapSide side) => side switch
        {
            MapSide.RoomTickets => "Queue tickets tonight",
            MapSide.RoomDust => "Ceiling dust tonight",
            MapSide.RoomWaiters => "Impatient waiters tonight",
            MapSide.RoomClock => "The clock strikes tonight",
            MapSide.PostWind => "Paper wind tonight",
            MapSide.PostStamps => "Stamps tonight",
            MapSide.PostParcel => "A giant parcel tonight",
            MapSide.PostPlanes => "Paper planes tonight",
            MapSide.GlassCarriers => "Pollen carriers tonight",
            MapSide.GlassPanes => "Falling glass tonight",
            MapSide.GlassBuds => "Flower turrets tonight",
            MapSide.LampOil => "Lamp oil tonight",
            MapSide.LampLantern => "A lantern tonight",
            MapSide.TheatreAudience => "The audience throws tonight",
            MapSide.TheatreCurtain => "The curtain falls tonight",
            MapSide.TheatreFirework => "Fireworks tonight",
            MapSide.VaultPages => "Flying pages tonight",
            MapSide.VaultWorms => "Bookworms tonight",
            MapSide.VaultPot => "The inkpot tips tonight",
            MapSide.VaultStamp => "The giant stamp tonight",
            _ => null
        };
    }

    public sealed partial class MapEnvironmentState
    {
        /// <summary>This wave's one side mechanic (MapSide.All in captures, which show everything).</summary>
        public MapSide Side;
        /// <summary>True while a boss or an elite is on the field: the side mechanic starts nothing new.</summary>
        public bool Quiet;
        public bool SideSaid;
    }

    public sealed partial class CombatSimulation
    {
        /// <summary>The side mechanic is this wave's, so its things are on the map (a turret flower, the firework machine).</summary>
        public bool MapSideShown(MapSide side) => mapEnv != null && (mapEnv.Side == side || mapEnv.Side == MapSide.All);
        /// <summary>The side mechanic may start something now.</summary>
        private bool MapSideOn(MapSide side) => MapSideShown(side) && (!mapEnv.Quiet || mapEnv.Side == MapSide.All);
        /// <summary>Keeps a side mechanic's next start at least two seconds away while it may not start anything.</summary>
        private void MapSideHold(MapSide side, ref float next) { if (!MapSideOn(side)) next = Mathf.Max(next, Elapsed + 2); }

        /// <summary>Each step: is a boss or an elite on the field, and the wave-start line naming the side mechanic.</summary>
        private void StepMapPacing()
        {
            bool quiet = false;
            foreach (UnitState u in StableUnits)
                if (u.Alive && u.Faction == Faction.Enemy && (u.Elite || u.Definition.IsBoss)) { quiet = true; break; }
            mapEnv.Quiet = quiet;
            if (mapEnv.SideSaid || Elapsed < 1.2f) return;
            mapEnv.SideSaid = true;
            string line = MapPacing.Name(mapEnv.Side);
            if (line != null) MapSay(line, new Color(1f, .9f, .62f));
        }
    }
}
