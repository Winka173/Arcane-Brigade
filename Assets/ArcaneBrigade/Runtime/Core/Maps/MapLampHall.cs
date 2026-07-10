using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    // The Lamp Hall (waves 12-14; Docs/Map_Mechanics_2026-10-25.md section 4): lamp light and lamp oil, the drowsy dark, night
    // prowlers, the snuffing hand, the carried lantern. Requirement 25/10/2026: each map's state, tunings and rules live in its own
    // file; MapEnvironmentCombat and MapBoosts only dispatch to the hooks below.

    public sealed partial class MapEnvironmentState
    {
        // Lamp Hall: the lamps and their sweeping light, the snuffer's round
        public sealed class Lamp { public Vector2 Base, Light; public float Phase, SnuffedAt = -100, SnuffedUntil = -100, FlareAt = -100; public bool Lit = true; }
        public readonly List<Lamp> Lamps = new List<Lamp>();
        public int SnuffTarget = -1; public float ReachAt = -100, NextSnuff;
        public float SnuffAt => ReachAt + MapEnvironmentRules.SnuffReach;
        public bool HeroLit; public float LightSaidAt = -100;
        // lamp oil: a vial a lit lamp drops (0 attack speed, 1 gold a kill, 2 move speed)
        public sealed class LampOil { public Vector2 Position, From; public float Born; public int Kind; }
        public readonly List<LampOil> LampOils = new List<LampOil>();
        public float NextLampOil;
        // night prowlers: who stands in a light this step, who was dazzled stepping into one and until when
        public readonly HashSet<int> LampLitFoes = new HashSet<int>();
        public readonly Dictionary<int, float> LampDazzled = new Dictionary<int, float>();
        public readonly Dictionary<int, float> LampDazzleReady = new Dictionary<int, float>();
        // the carried lantern: lying (0..1 held to pick up), held until LanternUntil; the foes its flame burns
        public bool LanternLying; public Vector2 LanternAt; public float LanternHold, LanternBorn = -100, LanternUntil = -100, NextLantern, NextLanternBurn;
        public bool LanternHeld(float t) => t < LanternUntil;
        public readonly Dictionary<int, float> LanternBurnt = new Dictionary<int, float>();
        public float LanternBurnAt = -100;
        /// <summary>Where the carried light is (the hero), for the air: the ash parts round it.</summary>
        public Vector2 LanternLightAt;
    }

    public static partial class MapEnvironmentRules
    {
        // the lamp's light is a 2.8 x 1.8 oval: the hero in it deals +15% damage with 2 armour, and friends attack 10% faster
        public const float LightRadiusX = 2.8f, LightRadiusY = 1.8f, LightDamage = 1.15f, LightArmor = 2, LightHaste = 1.1f;
        // the snuffing hand: one lamp every 15 s, dark for 9 s; relit within 3 s (walk within 1.3 units) it flares and stuns the
        // enemies in its light for 1.5 s
        public const float SnuffEvery = 15, SnuffReach = 1.4f, SnuffSeconds = 9, RelightReach = 1.3f, FlareWindow = 3, FlareStun = 1.5f;
        // the drowsy dark: enemies outside the light attack 20% slower; night prowlers walk 20% faster in it; stepping into a light an
        // enemy is dazzled: frozen 1 s and +20% damage taken for 3 s, at most once per 5 s
        public const float DarkCycle = 1.25f, DarkSpeed = 1.2f, DazzleFreeze = 1, DazzleSeconds = 3, DazzleTaken = 1.2f, DazzleEvery = 5;
        // lamp oil: every 15 s a lit lamp drops a vial worth 10 s of +20% attack speed, +1 gold a kill or +15% move speed
        public const float LampOilEvery = 15, LampOilSeconds = 10, LampOilLife = 14, LampOilHaste = .2f, LampOilGold = 1, LampOilSpeed = .15f; public const int LampOilMax = 3;
        // the carried lantern: one every 20 s; held by for 1.5 s it is carried 10 s, a moving 2-unit light whose flame burns up to 6
        // enemies within 2 units for 40% of the hero hit a second; burnt enemies attack 20% slower
        public const float LanternEvery = 20, LanternHoldSeconds = 1.5f, LanternReach = 1.2f, LanternSeconds = 10, LanternRadius = 2, LanternBurnHit = .4f, LanternBurnCycle = 1.25f;
        public const int LanternBurnMax = 6;
    }

    public sealed partial class CombatSimulation
    {
        private void InitLampHall(MapEnvironmentState s)
        {
            Vector2 a = s.Anchor;
            Vector2[] bases = { new Vector2(-6.2f, 2.6f), new Vector2(6.2f, 2.6f), new Vector2(0, -3.4f) };
            for (int i = 0; i < bases.Length; i++) s.Lamps.Add(new MapEnvironmentState.Lamp { Base = a + bases[i], Phase = i * 2.1f + MapNext(0, 1), Light = a + bases[i] });
            s.NextSnuff = 7; s.NextLampOil = 6; s.NextLantern = 10;
        }

        private void StepLampHall(float deltaTime)
        {
            StepLamps(deltaTime);
            StepLampOil();
            StepLantern(deltaTime);
            StepNightProwlers();
        }

        /// <summary>A stat the hero gets while standing in a lit lamp's light or carrying the lantern: +15% damage and 2 armour.</summary>
        private float LampHallStandingBoost(MapStat stat)
        {
            if (stat != MapStat.Damage && stat != MapStat.Armor || !HeroInMapLight) return 0;
            return stat == MapStat.Damage ? MapEnvironmentRules.LightDamage - 1 : MapEnvironmentRules.LightArmor;
        }
        /// <summary>How this map changes one enemy along one axis (1 = unchanged): in the dark it prowls faster and attacks drowsily;
        /// dazzled by a light it takes more damage; burnt by the lantern it attacks slower.</summary>
        private float LampHallEnemyFactor(UnitState unit, MapEnemyAxis axis)
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            bool dark = !s.LampLitFoes.Contains(unit.Id);
            switch (axis)
            {
                case MapEnemyAxis.Speed: return dark ? MapEnvironmentRules.DarkSpeed : 1; // night prowlers
                case MapEnemyAxis.Cycle:
                    float cycle = dark ? MapEnvironmentRules.DarkCycle : 1; // the drowsy dark
                    if (s.LanternBurnt.Count > 0 && s.LanternBurnt.TryGetValue(unit.Id, out float burnt) && burnt > t) cycle *= MapEnvironmentRules.LanternBurnCycle;
                    return cycle;
                case MapEnemyAxis.DamageTaken:
                    return s.LampDazzled.Count > 0 && s.LampDazzled.TryGetValue(unit.Id, out float dazzled) && dazzled > t ? MapEnvironmentRules.DazzleTaken : 1;
                default: return 1;
            }
        }
        /// <summary>The attack cooldown factor of the hero or a friend: while the hero stands in a light, friends attack 10% faster.</summary>
        private float LampHallAllyCycle(UnitState unit) => unit.Faction == Faction.Companion && mapEnv.HeroLit ? 1 / MapEnvironmentRules.LightHaste : 1;
        /// <summary>The hero's pace on this map beyond the move-speed boosts.</summary>
        private float LampHallHeroPace() => 1;
        /// <summary>A fallen enemy's coins on this map.</summary>
        private int LampHallCoins(UnitState target, int coins) => coins;

        private void StepLamps(float deltaTime)
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            s.Shown.Add(MapIdentity.Element.Light);
            const float reach2 = MapEnvironmentRules.RelightReach * MapEnvironmentRules.RelightReach;
            for (int i = 0; i < s.Lamps.Count; i++)
            {
                MapEnvironmentState.Lamp lamp = s.Lamps[i];
                float phase = lamp.Phase + t * .32f;
                lamp.Light = ClampPosition(lamp.Base + new Vector2(Mathf.Cos(phase), Mathf.Sin(phase) * .55f) * 2.4f + Vector2.up * .3f, .5f);
                lamp.Lit = !(t >= lamp.SnuffedAt && t < lamp.SnuffedUntil);
                // the hero walking up to a snuffed lamp lights it again; within 3 s of the snuff it flares and stuns the foes in its light
                if (lamp.Lit || t < lamp.SnuffedAt + .2f) continue;
                if ((Champion.Position - lamp.Base).sqrMagnitude >= reach2 && (Champion.Position - lamp.Light).sqrMagnitude >= reach2) continue;
                bool flare = t < lamp.SnuffedAt + MapEnvironmentRules.FlareWindow;
                lamp.SnuffedUntil = t; lamp.Lit = true;
                if (!flare) continue;
                lamp.FlareAt = t;
                int stunned = 0;
                foreach (UnitState u in StableUnits)
                    if (u.Alive && u.Faction == Faction.Enemy && InLampOval(u.Position, lamp.Light)) { Sleep(u, MapEnvironmentRules.FlareStun); stunned++; }
                MapSay("Flare!", new Color(1f, .86f, .45f));
            }
            if (t >= s.NextSnuff)
            {
                s.NextSnuff = t + MapEnvironmentRules.SnuffEvery;
                int best = -1; float bestDistance = float.MaxValue;
                for (int i = 0; i < s.Lamps.Count; i++)
                {
                    float d = (s.Lamps[i].Light - Champion.Position).sqrMagnitude;
                    if (s.Lamps[i].Lit && d < bestDistance) { best = i; bestDistance = d; }
                }
                if (best >= 0)
                {
                    s.SnuffTarget = best; s.ReachAt = t;
                    s.Lamps[best].SnuffedAt = s.SnuffAt; s.Lamps[best].SnuffedUntil = s.SnuffAt + MapEnvironmentRules.SnuffSeconds;
                    s.Shown.Add(MapIdentity.Element.Snuffer);
                }
            }
            bool lit = InMapLight(Champion.Position);
            // stepping into the light says what it gives over the hero's head (once in a while, not at every edge)
            if (lit && !s.HeroLit && t - s.LightSaidAt > 4)
            {
                s.LightSaidAt = t;
                // requirement 26/10/2026: one plain line per stat, like a skill's gain
                MapSay(MapBoostText.Label(MapStat.Damage, MapEnvironmentRules.LightDamage - 1), new Color(1f, .9f, .55f));
                MapSay(MapBoostText.Label(MapStat.Armor, MapEnvironmentRules.LightArmor), new Color(1f, .9f, .55f));
                MapSay("Friends attack speed +" + Mathf.RoundToInt((MapEnvironmentRules.LightHaste - 1) * 100) + "%", new Color(1f, .86f, .4f));
            }
            s.HeroLit = lit;
        }

        /// <summary>Every 15 s a lit lamp drops an oil vial under it; walking through one gives 10 s of attack speed, gold or speed.</summary>
        private void StepLampOil()
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            if (t >= s.NextLampOil)
            {
                s.NextLampOil = t + MapEnvironmentRules.LampOilEvery;
                int lit = 0, pick = -1;
                for (int i = 0; i < s.Lamps.Count; i++) if (s.Lamps[i].Lit && mapRandom.Next(++lit) == 0) pick = i;
                if (pick >= 0 && s.LampOils.Count < MapEnvironmentRules.LampOilMax)
                {
                    Vector2 from = s.Lamps[pick].Base + Vector2.up * 1.9f;
                    s.LampOils.Add(new MapEnvironmentState.LampOil { From = from, Position = ClampPosition(s.Lamps[pick].Base + new Vector2(MapNext(-.9f, .9f), -.75f), .5f), Born = t, Kind = mapRandom.Next(3) });
                }
            }
            float reach = Champion.Definition.Radius + .5f;
            for (int i = s.LampOils.Count - 1; i >= 0; i--)
            {
                MapEnvironmentState.LampOil oil = s.LampOils[i];
                if (t > oil.Born + MapEnvironmentRules.LampOilLife) { s.LampOils.RemoveAt(i); continue; }
                if (t < oil.Born + .5f || (Champion.Position - oil.Position).sqrMagnitude > reach * reach) continue; // still falling
                switch (oil.Kind)
                {
                    case 0: GrantMapBoost(MapStat.AttackSpeed, MapEnvironmentRules.LampOilHaste, MapEnvironmentRules.LampOilSeconds, "lamp-oil-0", "LAMP OIL"); break;
                    case 1: GrantMapBoost(MapStat.Gold, MapEnvironmentRules.LampOilGold, MapEnvironmentRules.LampOilSeconds, "lamp-oil-1", "LAMP OIL"); break;
                    default: GrantMapBoost(MapStat.MoveSpeed, MapEnvironmentRules.LampOilSpeed, MapEnvironmentRules.LampOilSeconds, "lamp-oil-2", "LAMP OIL"); break;
                }
                MapCaught(oil.Position, new Color(1f, .74f, .36f));
                s.LampOils.RemoveAt(i);
            }
        }

        /// <summary>The carried lantern: one lies in the dark every 20 s; held by for 1.5 s it is carried for 10 s, and its flame
        /// burns up to 6 enemies within 2 units each second.</summary>
        private void StepLantern(float deltaTime)
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            bool held = s.LanternHeld(t);
            if (t >= s.NextLantern)
            {
                s.NextLantern = t + MapEnvironmentRules.LanternEvery;
                if (!held && !s.LanternLying)
                {
                    // somewhere in the dark a few steps from the hero
                    Vector2 spot = Champion.Position;
                    for (int k = 0; k < 8; k++)
                    {
                        float angle = MapNext(0, Mathf.PI * 2), distance = MapNext(3.2f, 5);
                        spot = ClampPosition(Champion.Position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * .8f) * distance, 1);
                        if (!InMapLight(spot)) break;
                    }
                    s.LanternLying = true; s.LanternAt = spot; s.LanternHold = 0; s.LanternBorn = t;
                }
            }
            if (s.LanternLying)
            {
                const float reach2 = MapEnvironmentRules.LanternReach * MapEnvironmentRules.LanternReach;
                if ((Champion.Position - s.LanternAt).sqrMagnitude < reach2) s.LanternHold = Mathf.Min(1, s.LanternHold + deltaTime / MapEnvironmentRules.LanternHoldSeconds);
                else s.LanternHold = Mathf.Max(0, s.LanternHold - deltaTime);
                if (s.LanternHold >= 1)
                {
                    s.LanternLying = false; s.LanternHold = 0; s.LanternUntil = t + MapEnvironmentRules.LanternSeconds; s.NextLanternBurn = t + .25f;
                    MapSay("Lantern carried", new Color(1f, .78f, .4f));
                    held = true;
                }
            }
            if (held) s.LanternLightAt = Champion.Position;
            if (!held || t < s.NextLanternBurn) return;
            s.NextLanternBurn = t + 1; s.LanternBurnAt = t;
            // the six nearest enemies in the flame's reach
            const float r2 = MapEnvironmentRules.LanternRadius * MapEnvironmentRules.LanternRadius;
            lanternTargets.Clear();
            foreach (UnitState u in StableUnits)
                if (u.Alive && CanHit(Faction.Champion, u) && u.EntranceUntil <= t && (u.Position - Champion.Position).sqrMagnitude < r2) lanternTargets.Add(u);
            lanternTargets.Sort((a, b) => (a.Position - Champion.Position).sqrMagnitude.CompareTo((b.Position - Champion.Position).sqrMagnitude));
            CombatCause flame = NewCause(Champion.Id).Child("map-lantern-flame");
            float hit = Champion.Definition.Weapon.Damage * MapEnvironmentRules.LanternBurnHit;
            for (int i = 0; i < lanternTargets.Count && i < MapEnvironmentRules.LanternBurnMax; i++)
            {
                UnitState u = lanternTargets[i];
                s.LanternBurnt[u.Id] = t + 1.25f;
                ApplyDamage(u, hit, Faction.Champion, "Lantern flame", u.Position - Champion.Position, element: DamageElement.Ember, cause: flame);
                if (Phase != RunPhase.Combat) return;
            }
        }
        private readonly List<UnitState> lanternTargets = new List<UnitState>();
        private readonly List<int> lampStale = new List<int>(); private float lampSweepAt;

        /// <summary>Who stands in a light this step; an enemy stepping into one is dazzled (frozen 1 s, +20% damage taken for 3 s),
        /// at most once per 5 s each.</summary>
        private void StepNightProwlers()
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            foreach (UnitState u in StableUnits)
            {
                if (!u.Alive || u.Faction != Faction.Enemy) continue;
                bool lit = InMapLight(u.Position), was = s.LampLitFoes.Contains(u.Id);
                if (lit == was) continue;
                if (!lit) { s.LampLitFoes.Remove(u.Id); continue; }
                s.LampLitFoes.Add(u.Id);
                if (u.EntranceUntil > t || s.LampDazzleReady.TryGetValue(u.Id, out float ready) && ready > t) continue;
                Sleep(u, MapEnvironmentRules.DazzleFreeze);
                s.LampDazzled[u.Id] = t + MapEnvironmentRules.DazzleSeconds; s.LampDazzleReady[u.Id] = t + MapEnvironmentRules.DazzleEvery;
            }
            // forget the fallen now and then
            if (t < lampSweepAt && t > lampSweepAt - 1) return;
            lampSweepAt = t + .5f; lampStale.Clear();
            foreach (KeyValuePair<int, float> d in s.LampDazzleReady) if (d.Value <= t) lampStale.Add(d.Key);
            foreach (int id in lampStale) { s.LampDazzleReady.Remove(id); s.LampDazzled.Remove(id); }
            lampStale.Clear();
            foreach (KeyValuePair<int, float> d in s.LanternBurnt) if (d.Value <= t) lampStale.Add(d.Key);
            foreach (int id in lampStale) s.LanternBurnt.Remove(id);
            lampStale.Clear();
            foreach (int id in s.LampLitFoes) { UnitState u = UnitWithId(id); if (u == null || !u.Alive) lampStale.Add(id); }
            foreach (int id in lampStale) s.LampLitFoes.Remove(id);
        }

        /// <summary>The hero stands in a light: a lit lamp's oval, or carrying the lantern.</summary>
        public bool HeroInMapLight => Champion != null && mapEnv != null && mapEnv.Theme == MapTheme.LampHall && (mapEnv.LanternHeld(Elapsed) || InMapLight(Champion.Position));
        private bool InMapLight(Vector2 position)
        {
            if (mapEnv == null || mapEnv.Theme != MapTheme.LampHall) return false;
            foreach (MapEnvironmentState.Lamp lamp in mapEnv.Lamps)
                if (lamp.Lit && InLampOval(position, lamp.Light)) return true;
            // the carried lantern makes the hero a moving light
            return mapEnv.LanternHeld(Elapsed) && Champion != null && (position - Champion.Position).sqrMagnitude < MapEnvironmentRules.LanternRadius * MapEnvironmentRules.LanternRadius;
        }
        private static bool InLampOval(Vector2 position, Vector2 light)
        {
            Vector2 d = position - light; // the painted light is an oval on the floor; the bonus follows its edge
            return d.x * d.x / (MapEnvironmentRules.LightRadiusX * MapEnvironmentRules.LightRadiusX) + d.y * d.y / (MapEnvironmentRules.LightRadiusY * MapEnvironmentRules.LightRadiusY) < 1;
        }

        /// <summary>For captures: two lamps by the hero, one being snuffed, a flare, an oil vial, the lantern lying half picked up.</summary>
        private void LampHallCaptureFixture()
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed; Vector2 h = Champion.Position;
            s.Lamps[0].Base = h + new Vector2(-3.4f, 1.2f); s.Lamps[1].Base = h + new Vector2(3.6f, 1.4f);
            s.Lamps[0].Phase = -t * .32f - 0.3f; s.Lamps[1].Phase = -t * .32f + 2.4f;
            s.Lamps[0].FlareAt = t - .2f;
            s.SnuffTarget = 1; s.ReachAt = t - .7f; s.Lamps[1].SnuffedAt = s.SnuffAt; s.Lamps[1].SnuffedUntil = s.SnuffAt + 9; s.NextSnuff = t + 30;
            s.LampOils.Clear();
            for (int k = 0; k < 3; k++) s.LampOils.Add(new MapEnvironmentState.LampOil { From = h, Position = h + new Vector2(-1.6f + k * 1.1f, -1.8f), Born = t - 2, Kind = k });
            s.NextLampOil = t + 30;
            s.LanternLying = true; s.LanternAt = h + new Vector2(2.2f, -2.4f); s.LanternHold = .6f; s.LanternBorn = t - 3; s.NextLantern = t + 30;
        }
    }
}
