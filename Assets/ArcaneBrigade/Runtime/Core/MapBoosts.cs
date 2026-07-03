using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The base stats a map can lend the hero for a while (requirement 25/10/2026: map boosts are base stats only).</summary>
    public enum MapStat { Damage, AttackSpeed, Armor, Gold, MoveSpeed }

    /// <summary>One timed boost from a map element: its stat, its size (a share for damage/attack speed/move speed, points for
    /// armour, coins a kill for gold), until when, and what gave it (the same source again extends it).</summary>
    public sealed class MapBoost { public MapStat Stat; public float Value; public float Until; public string Source; }

    /// <summary>A line to show over the hero's head (a boost gained, a map moment).</summary>
    public struct MapPop { public string Text; public Color Colour; }

    public static class MapBoostText
    {
        public static string Label(MapStat stat, float value) => stat switch
        {
            // requirement 26/10/2026: said plainly, like a stat gained from a skill ("Damage +15%")
            MapStat.Damage => "Damage +" + Mathf.RoundToInt(value * 100) + "%",
            MapStat.AttackSpeed => "Attack speed +" + Mathf.RoundToInt(value * 100) + "%",
            MapStat.Armor => "Armor +" + Mathf.RoundToInt(value),
            MapStat.Gold => "Gold a kill +" + Mathf.RoundToInt(value),
            _ => "Move speed +" + Mathf.RoundToInt(value * 100) + "%"
        };
        public static Color Colour(MapStat stat) => stat switch
        {
            MapStat.Damage => new Color(1f, .55f, .45f),
            MapStat.AttackSpeed => new Color(1f, .86f, .4f),
            MapStat.Armor => new Color(.78f, .84f, .92f),
            MapStat.Gold => new Color(1f, .78f, .3f),
            _ => new Color(.55f, .85f, 1f)
        };
    }

    /// <summary>How a map changes one enemy: its pace, its attack rhythm (a cooldown factor, above 1 = slower), the damage it
    /// deals and the damage it takes.</summary>
    /// <summary>Range: how far an enemy looks for a target (smoke shortens it). Knockback: how far our blows push it (rain lengthens it).</summary>
    public enum MapEnemyAxis { Speed, Cycle, DamageDealt, DamageTaken, Range, Knockback }

    public sealed partial class MapEnvironmentState
    {
        public readonly List<MapBoost> Boosts = new List<MapBoost>();
        public readonly List<MapPop> Pops = new List<MapPop>();
        /// <summary>Things picked up a moment ago (a letter, an oil vial): the presentation flashes them and flies their light to the hero.</summary>
        public readonly List<MapCatch> Catches = new List<MapCatch>();
    }
    public struct MapCatch { public Vector2 At; public float Time; public Color Colour; }

    public sealed partial class CombatSimulation
    {
        public const float MapBoostCap = 20;
        /// <summary>Lends the hero a stat for a while and says so over its head. The same source again keeps the larger value and
        /// adds its time (at most MapBoostCap seconds ahead).</summary>
        public void GrantMapBoost(MapStat stat, float value, float seconds, string source, string prefix = null)
        {
            MapEnvironmentState s = mapEnv; if (s == null || value <= 0 || seconds <= 0) return;
            float now = Elapsed;
            MapBoost held = s.Boosts.Find(b => b.Stat == stat && b.Source == source && b.Until > now);
            if (held != null) { held.Value = Mathf.Max(held.Value, value); held.Until = Mathf.Min(now + MapBoostCap, held.Until + seconds); }
            else s.Boosts.Add(held = new MapBoost { Stat = stat, Value = value, Until = now + seconds, Source = source });
            MapSay(MapBoostText.Label(stat, held.Value), MapBoostText.Colour(stat)); // only the stat and its size: no source, no seconds
        }
        /// <summary>Marks a pickup so it never just vanishes (requirement 25/10/2026: no moment without its animation).</summary>
        public void MapCaught(Vector2 at, Color colour) { if (mapEnv == null) return; if (mapEnv.Catches.Count >= 12) mapEnv.Catches.RemoveAt(0); mapEnv.Catches.Add(new MapCatch { At = at, Time = Elapsed, Colour = colour }); }
        /// <summary>A line over the hero's head (the presentation shows each once, stacked, never dropped).</summary>
        public void MapSay(string text, Color colour) { if (mapEnv != null && mapEnv.Pops.Count < 12) mapEnv.Pops.Add(new MapPop { Text = text, Colour = colour }); }
        /// <summary>The sum of a stat lent by the map right now (timed boosts plus standing-in effects such as a lamp's light).</summary>
        public float MapBoostValue(MapStat stat)
        {
            MapEnvironmentState s = mapEnv; if (s == null) return 0;
            float total = 0, now = Elapsed;
            for (int i = s.Boosts.Count - 1; i >= 0; i--)
            {
                if (s.Boosts[i].Until <= now) { s.Boosts.RemoveAt(i); continue; }
                if (s.Boosts[i].Stat == stat) total += s.Boosts[i].Value;
            }
            return total + MapStandingBoost(stat);
        }
        /// <summary>The boosts active now (for the HUD row).</summary>
        public IReadOnlyList<MapBoost> MapBoosts => mapEnv != null ? mapEnv.Boosts : (IReadOnlyList<MapBoost>)System.Array.Empty<MapBoost>();

        /// <summary>A stat the hero gets only while standing somewhere (a lamp's light, a spotlight, a flower bed). Each map adds its
        /// own in its file under Core/Maps (requirement 25/10/2026).</summary>
        private float MapStandingBoost(MapStat stat)
        {
            MapEnvironmentState s = mapEnv; if (s == null || Champion == null) return 0;
            return s.Theme switch
            {
                MapTheme.WaitingRoom => WaitingRoomStandingBoost(stat),
                MapTheme.PostOffice => PostOfficeStandingBoost(stat),
                MapTheme.Glasshouse => GlasshouseStandingBoost(stat),
                MapTheme.LampHall => LampHallStandingBoost(stat),
                MapTheme.Theatre => TheatreStandingBoost(stat),
                MapTheme.ArchiveVault => ArchiveVaultStandingBoost(stat),
                _ => 0
            };
        }

        /// <summary>How the map changes one enemy along one axis right now (1 = unchanged). Each map adds its own rules in its file
        /// under Core/Maps (requirement 25/10/2026).</summary>
        public float MapEnemyFactor(UnitState unit, MapEnemyAxis axis)
        {
            MapEnvironmentState s = mapEnv; if (s == null || unit == null || unit.Faction != Faction.Enemy) return 1;
            return s.Theme switch
            {
                MapTheme.WaitingRoom => WaitingRoomEnemyFactor(unit, axis),
                MapTheme.PostOffice => PostOfficeEnemyFactor(unit, axis),
                MapTheme.Glasshouse => GlasshouseEnemyFactor(unit, axis),
                MapTheme.LampHall => LampHallEnemyFactor(unit, axis),
                MapTheme.Theatre => TheatreEnemyFactor(unit, axis),
                MapTheme.ArchiveVault => ArchiveVaultEnemyFactor(unit, axis),
                _ => 1
            };
        }
    }
}
