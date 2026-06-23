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
            MapStat.Damage => "+" + Mathf.RoundToInt(value * 100) + "% DAMAGE",
            MapStat.AttackSpeed => "+" + Mathf.RoundToInt(value * 100) + "% ATTACK SPEED",
            MapStat.Armor => "+" + Mathf.RoundToInt(value) + " ARMOR",
            MapStat.Gold => "+" + Mathf.RoundToInt(value) + " GOLD A KILL",
            _ => "+" + Mathf.RoundToInt(value * 100) + "% SPEED"
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
    public enum MapEnemyAxis { Speed, Cycle, DamageDealt, DamageTaken }

    public sealed partial class MapEnvironmentState
    {
        public readonly List<MapBoost> Boosts = new List<MapBoost>();
        public readonly List<MapPop> Pops = new List<MapPop>();
    }

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
            MapSay((prefix != null ? prefix + " · " : "") + MapBoostText.Label(stat, held.Value) + " · " + Mathf.CeilToInt(held.Until - now) + "s", MapBoostText.Colour(stat));
        }
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

        /// <summary>A stat the hero gets only while standing somewhere (a lamp's light, a spotlight, a flower bed). Each map adds its own.</summary>
        private float MapStandingBoost(MapStat stat)
        {
            MapEnvironmentState s = mapEnv; if (s == null || Champion == null) return 0;
            switch (s.Theme)
            {
                case MapTheme.LampHall:
                    if (!InMapLight(Champion.Position)) return 0;
                    return stat == MapStat.Damage ? MapEnvironmentRules.LightDamage - 1 : stat == MapStat.Armor ? MapEnvironmentRules.LightArmor : 0;
                default: return 0;
            }
        }

        /// <summary>How the map changes one enemy along one axis right now (1 = unchanged). Each map adds its own rules.</summary>
        public float MapEnemyFactor(UnitState unit, MapEnemyAxis axis)
        {
            MapEnvironmentState s = mapEnv; if (s == null || unit == null || unit.Faction != Faction.Enemy) return 1;
            switch (s.Theme)
            {
                case MapTheme.LampHall:
                    if (axis == MapEnemyAxis.Cycle && !InMapLight(unit.Position)) return MapEnvironmentRules.DarkCycle; // the drowsy dark
                    return 1;
                default: return 1;
            }
        }
    }
}
