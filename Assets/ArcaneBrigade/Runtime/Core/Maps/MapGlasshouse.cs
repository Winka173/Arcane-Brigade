using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    // The Glasshouse (waves 9-11; Docs/Map_Mechanics_2026-10-25.md section 3): blooms, sun and rain, pollen carriers, the cracking
    // roof, the shooting flower. Requirement 25/10/2026: the map's state, tunings and rules live in this file only;
    // MapEnvironmentCombat and MapBoosts dispatch to the hooks below.

    public sealed partial class MapEnvironmentState
    {
        // Glasshouse: six pots that bloom in five colours
        /// <summary>A pot: Growth runs 0..1 to its bloom; Colour is the bloom (0 red, 1 yellow, 2 brown, 3 orange, 4 blue) or -1 while
        /// it grows. PickedAt / PickedColour mark the last bloom walked through (the presentation bursts its petals).</summary>
        public sealed class GlassPot { public Vector2 Position; public float Growth, BloomedAt = -100, PickedAt = -100; public int Colour = -1, PickedColour = -1; }
        public readonly List<GlassPot> GlassPots = new List<GlassPot>();

        // sun and rain, turn about every 20 s from the start of the wave (sun first); captures shift the clock
        public int GlassWeatherTurn = -1; public float GlassWeatherShift;
        public bool GlassSunny(float t) => Mathf.FloorToInt(Mathf.Max(0, t + GlassWeatherShift) / MapEnvironmentRules.GlassWeatherSeconds) % 2 == 0;
        /// <summary>For the look: 1 full sun, 0 full rain, easing across a second either side of each turn.</summary>
        public float GlassSunBlend(float t)
        {
            float clock = Mathf.Max(0, t + GlassWeatherShift), period = MapEnvironmentRules.GlassWeatherSeconds;
            float boundary = Mathf.Round(clock / period) * period;
            float before = boundary <= 0 || Mathf.FloorToInt((boundary - .01f) / period) % 2 == 0 ? 1 : 0;
            float after = Mathf.FloorToInt((boundary + .01f) / period) % 2 == 0 ? 1 : 0;
            return Mathf.Lerp(before, after, Mathf.SmoothStep(0, 1, Mathf.InverseLerp(-1.1f, 1.1f, clock - boundary)));
        }

        // pollen carriers (an enemy with a gold aura) and the flower beds they leave
        public sealed class GlassCarrier { public int Id; public Vector2 Last; public float Since; }
        public readonly List<GlassCarrier> GlassCarriers = new List<GlassCarrier>();
        public float GlassNextCarrier;
        public sealed class GlassBed { public Vector2 Position; public float Born; public int Seed; }
        public readonly List<GlassBed> GlassBeds = new List<GlassBed>();
        public bool GlassHeroInBed; public float GlassBedSaidAt = -100;

        // the cracking roof: a pane cracks over a red area and falls
        public sealed class GlassPane { public Vector2 Position; public float WarnAt; public bool Broken; public int Seed; public float BreakAt => WarnAt + MapEnvironmentRules.GlassPaneWarn; }
        public readonly List<GlassPane> GlassPanes = new List<GlassPane>();
        public float GlassNextPane;

        // the shooting flowers: two big buds that open into turrets, and the seeds they fire
        public sealed class GlassBud
        {
            public Vector2 Position, Aim = Vector2.right; public float Hold, OpenAt = -100, ReadyAt, NextShot, LastShot = -100;
            public bool Open(float t) => t >= OpenAt && t < OpenAt + MapEnvironmentRules.GlassTurretSeconds;
        }
        public readonly List<GlassBud> GlassBuds = new List<GlassBud>();
        public sealed class GlassSeed { public Vector2 From, To; public int TargetId; public float Born, Arrive, Damage; }
        public readonly List<GlassSeed> GlassSeeds = new List<GlassSeed>();
    }

    public static partial class MapEnvironmentRules
    {
        // blooms: a pot blooms after 15 s (10 s in sun); each colour lends one base stat for 10 s
        public const float GlassPotGrow = 15, GlassPotGrowSun = 10, GlassBloomSeconds = 10, GlassPickReach = .85f;
        public const float GlassBloomDamage = .15f, GlassBloomHaste = .2f, GlassBloomArmor = 3, GlassBloomGold = 1, GlassBloomSpeed = .15f;
        // sun and rain: sun dazzles (attacks 15% slower), rain slows 15% and lengthens our knockback 50%; the Wax Admiral melts in
        // the sun (+30% damage taken, 20% slower) and hardens in the rain (20% less damage taken)
        public const float GlassWeatherSeconds = 20, GlassSunCycle = 1 / .85f, GlassRainSpeed = .85f, GlassRainKnockback = 1.5f;
        public const float GlassAdmiralSunTaken = 1.3f, GlassAdmiralSunPace = .8f, GlassAdmiralRainTaken = .8f;
        // pollen carriers: one every 12 s; enemies within 2.5 units attack 20% faster and walk 10% faster
        public const float GlassCarrierEvery = 12, GlassAuraRadius = 2.5f, GlassAuraCycle = 1 / 1.2f, GlassAuraSpeed = 1.1f; public const int GlassCarrierMax = 3;
        // a carrier's flower bed: 2 units, 8 s; the hero inside deals +15% damage and attacks 15% faster
        public const float GlassBedRadius = 2, GlassBedSeconds = 8, GlassBedDamage = .15f, GlassBedHaste = .15f;
        // the cracking roof: every 18 s, warned 2.5 s; the hero inside loses 10% of max health, enemies 30% (bosses 5%)
        public const float GlassPaneEvery = 18, GlassPaneWarn = 2.5f, GlassPaneRadius = 2, GlassPaneHero = .1f, GlassPaneEnemy = .3f, GlassPaneBoss = .05f;
        // the shooting flower: hold 2 s by a big bud; a 15 s turret firing at the nearest enemy within 7 units every 0.6 s for 60%
        // of the hero hit; back as a bud 20 s after it closes
        public const float GlassBudHold = 2, GlassBudReach = 1.4f, GlassTurretSeconds = 15, GlassTurretRegrow = 20, GlassTurretRange = 7, GlassTurretEvery = .6f, GlassTurretHit = .6f, GlassSeedFlight = .22f;
        public static readonly string[] GlassBloomNames = { "RED", "YELLOW", "BROWN", "ORANGE", "BLUE" };
    }

    public sealed partial class CombatSimulation
    {
        /// <summary>Sets the map up at the start of an ordinary wave (s.Anchor is the room's centre near the hero): six pots round the
        /// room, already part-grown so the first blooms open within the first quarter minute, and two big buds at its sides.</summary>
        private void InitGlasshouse(MapEnvironmentState s)
        {
            Vector2 a = s.Anchor;
            Vector2[] pots = { new Vector2(-5.4f, 2.9f), new Vector2(-1.9f, 4.2f), new Vector2(2.4f, 4.1f), new Vector2(5.9f, 2.5f), new Vector2(-4.7f, -3.7f), new Vector2(4.6f, -3.5f) };
            foreach (Vector2 p in pots) s.GlassPots.Add(new MapEnvironmentState.GlassPot { Position = ClampPosition(a + p, .8f), Growth = MapNext(.15f, .55f) });
            foreach (Vector2 p in new[] { new Vector2(-7.4f, -.5f), new Vector2(7.6f, -.2f) })
                s.GlassBuds.Add(new MapEnvironmentState.GlassBud { Position = ClampPosition(a + p, 1.1f) });
            s.GlassNextCarrier = 6; s.GlassNextPane = 9;
        }

        /// <summary>Runs the map each simulation step of the wave.</summary>
        private void StepGlasshouse(float deltaTime)
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            s.Shown.Add(MapIdentity.Element.Blooms);
            bool sunny = s.GlassSunny(t);
            StepGlassWeather(s, t, sunny);
            StepGlassPots(s, t, deltaTime, sunny);
            StepGlassCarriers(s, t);
            StepGlassPanes(s, t);
            StepGlassBuds(s, t, deltaTime);
        }

        /// <summary>A stat the hero gets while standing in a carrier's flower bed: +15% damage and +15% attack speed.</summary>
        private float GlasshouseStandingBoost(MapStat stat)
        {
            if (stat != MapStat.Damage && stat != MapStat.AttackSpeed || !GlassInBed(Champion.Position)) return 0;
            return stat == MapStat.Damage ? MapEnvironmentRules.GlassBedDamage : MapEnvironmentRules.GlassBedHaste;
        }
        /// <summary>How this map changes one enemy along one axis (1 = unchanged): sun dazzles (attacks 15% slower), rain slows 15%
        /// and lengthens our knockback 50%; a pollen carrier's aura quickens its neighbours; the Wax Admiral melts in the sun and
        /// hardens in the rain.</summary>
        private float GlasshouseEnemyFactor(UnitState unit, MapEnemyAxis axis)
        {
            MapEnvironmentState s = mapEnv; bool sunny = s.GlassSunny(Elapsed);
            switch (axis)
            {
                case MapEnemyAxis.Speed: return (sunny ? 1 : MapEnvironmentRules.GlassRainSpeed) * (GlassInAura(unit) ? MapEnvironmentRules.GlassAuraSpeed : 1);
                case MapEnemyAxis.Cycle: return (sunny ? MapEnvironmentRules.GlassSunCycle : 1) * (GlassInAura(unit) ? MapEnvironmentRules.GlassAuraCycle : 1);
                case MapEnemyAxis.Knockback: return sunny ? 1 : MapEnvironmentRules.GlassRainKnockback;
                case MapEnemyAxis.DamageTaken: return !GlassAdmiral(unit) ? 1 : sunny ? MapEnvironmentRules.GlassAdmiralSunTaken : MapEnvironmentRules.GlassAdmiralRainTaken;
                default: return 1;
            }
        }
        /// <summary>The attack cooldown factor of the hero or a friend on this map (below 1 = faster).</summary>
        private float GlasshouseAllyCycle(UnitState unit) => 1;
        /// <summary>The hero's pace on this map beyond the move-speed boosts.</summary>
        private float GlasshouseHeroPace() => 1;
        /// <summary>A fallen enemy's coins on this map.</summary>
        private int GlasshouseCoins(UnitState target, int coins) => coins;

        /// <summary>The Wax Admiral (the Glasshouse's mid-map boss) by its id.</summary>
        private static bool GlassAdmiral(UnitState unit) => unit != null && unit.Definition != null && unit.Definition.IsBoss && unit.Definition.Id != null && unit.Definition.Id.StartsWith("WaxAdmiral");
        private bool GlassInAura(UnitState unit)
        {
            List<MapEnvironmentState.GlassCarrier> carriers = mapEnv.GlassCarriers;
            const float r2 = MapEnvironmentRules.GlassAuraRadius * MapEnvironmentRules.GlassAuraRadius;
            for (int i = 0; i < carriers.Count; i++) if ((unit.Position - carriers[i].Last).sqrMagnitude < r2) return true;
            return false;
        }
        private bool GlassInBed(Vector2 at)
        {
            float t = Elapsed; const float r2 = MapEnvironmentRules.GlassBedRadius * MapEnvironmentRules.GlassBedRadius;
            foreach (MapEnvironmentState.GlassBed bed in mapEnv.GlassBeds)
                if (t < bed.Born + MapEnvironmentRules.GlassBedSeconds && (at - bed.Position).sqrMagnitude < r2) return true;
            return false;
        }

        /// <summary>Sun and rain turn every 20 s: each turn is said over the hero; in the sun the Wax Admiral walks 20% slower (bosses
        /// keep their own pace through MapMotion, so its step is shortened here).</summary>
        private void StepGlassWeather(MapEnvironmentState s, float t, bool sunny)
        {
            int turn = Mathf.FloorToInt(Mathf.Max(0, t + s.GlassWeatherShift) / MapEnvironmentRules.GlassWeatherSeconds);
            if (turn != s.GlassWeatherTurn)
            {
                s.GlassWeatherTurn = turn;
                if (sunny) MapSay("Sun", new Color(1f, .9f, .55f));
                else MapSay("Rain", new Color(.6f, .88f, .95f));
            }
            if (!sunny) return;
            foreach (UnitState u in units)
            {
                if (!u.Alive || !GlassAdmiral(u) || u.EntranceUntil > t) continue;
                Vector2 step = u.Position - u.PreviousPosition;
                if (step.sqrMagnitude < 1e-8f || step.sqrMagnitude > 1) continue; // a leap or a teleport is left alone
                u.Position = ClampPosition(u.PreviousPosition + step * MapEnvironmentRules.GlassAdmiralSunPace, u.Definition.Radius);
            }
        }

        /// <summary>Pots grow (faster in the sun) and bloom in a random colour; walking through a bloom picks its boost for 10 s.</summary>
        private void StepGlassPots(MapEnvironmentState s, float t, float deltaTime, bool sunny)
        {
            float reach = Champion.Definition.Radius + MapEnvironmentRules.GlassPickReach;
            foreach (MapEnvironmentState.GlassPot pot in s.GlassPots)
            {
                if (pot.Colour < 0)
                {
                    pot.Growth += deltaTime / (sunny ? MapEnvironmentRules.GlassPotGrowSun : MapEnvironmentRules.GlassPotGrow);
                    if (pot.Growth >= 1) { pot.Growth = 1; pot.Colour = mapRandom.Next(5); pot.BloomedAt = t; }
                    continue;
                }
                if ((Champion.Position - pot.Position).sqrMagnitude > reach * reach) continue;
                string prefix = MapEnvironmentRules.GlassBloomNames[pot.Colour] + " BLOOM", source = "glass-bloom-" + pot.Colour;
                float seconds = MapEnvironmentRules.GlassBloomSeconds;
                switch (pot.Colour)
                {
                    case 0: GrantMapBoost(MapStat.Damage, MapEnvironmentRules.GlassBloomDamage, seconds, source, prefix); break;
                    case 1: GrantMapBoost(MapStat.AttackSpeed, MapEnvironmentRules.GlassBloomHaste, seconds, source, prefix); break;
                    case 2: GrantMapBoost(MapStat.Armor, MapEnvironmentRules.GlassBloomArmor, seconds, source, prefix); break;
                    case 3: GrantMapBoost(MapStat.Gold, MapEnvironmentRules.GlassBloomGold, seconds, source, prefix); break;
                    default: GrantMapBoost(MapStat.MoveSpeed, MapEnvironmentRules.GlassBloomSpeed, seconds, source, prefix); break;
                }
                pot.PickedColour = pot.Colour; pot.PickedAt = t; pot.Colour = -1; pot.Growth = 0; // it regrows from a bud
            }
        }

        /// <summary>Every 12 s an enemy near the hero carries pollen (a gold aura); killed, it leaves a flower bed where it fell.</summary>
        private void StepGlassCarriers(MapEnvironmentState s, float t)
        {
            for (int i = s.GlassCarriers.Count - 1; i >= 0; i--)
            {
                MapEnvironmentState.GlassCarrier c = s.GlassCarriers[i];
                UnitState u = UnitWithId(c.Id);
                if (u != null && u.Alive) { c.Last = u.Position; continue; }
                s.GlassBeds.Add(new MapEnvironmentState.GlassBed { Position = c.Last, Born = t, Seed = mapRandom.Next(1000) });
                s.GlassCarriers.RemoveAt(i);
            }
            if (t >= s.GlassNextCarrier)
            {
                s.GlassNextCarrier = t + MapEnvironmentRules.GlassCarrierEvery;
                if (s.GlassCarriers.Count < MapEnvironmentRules.GlassCarrierMax)
                {
                    // a fair pick among the enemies in sight: ordinary bodies first, never a boss
                    UnitState pick = null; int seen = 0; bool ordinary = false;
                    foreach (UnitState u in units)
                    {
                        if (!u.Alive || u.Faction != Faction.Enemy || u.Definition.IsBoss || u.EntranceUntil > t) continue;
                        float d = (u.Position - Champion.Position).sqrMagnitude;
                        if (d < 4 || d > 100 || s.GlassCarriers.Exists(c => c.Id == u.Id)) continue;
                        bool plain = !u.SwarmMinion;
                        if (ordinary && !plain) continue;
                        if (plain && !ordinary) { ordinary = true; seen = 0; }
                        if (mapRandom.Next(++seen) == 0) pick = u;
                    }
                    if (pick != null) s.GlassCarriers.Add(new MapEnvironmentState.GlassCarrier { Id = pick.Id, Last = pick.Position, Since = t });
                }
            }
            s.GlassBeds.RemoveAll(b => t >= b.Born + MapEnvironmentRules.GlassBedSeconds);
            bool inBed = GlassInBed(Champion.Position);
            if (inBed && !s.GlassHeroInBed && t - s.GlassBedSaidAt > 2.5f)
            {
                s.GlassBedSaidAt = t;
                // requirement 26/10/2026: one plain line per stat, like a skill's gain
                MapSay(MapBoostText.Label(MapStat.Damage, MapEnvironmentRules.GlassBedDamage), new Color(.62f, .95f, .8f));
                MapSay(MapBoostText.Label(MapStat.AttackSpeed, MapEnvironmentRules.GlassBedHaste), new Color(.62f, .95f, .8f));
            }
            s.GlassHeroInBed = inBed;
        }

        /// <summary>Every 18 s a roof pane cracks over a red area by the hero (2.5 s warning) and falls: the hero inside loses 10% of
        /// max health, enemies inside 30% (bosses 5%).</summary>
        private void StepGlassPanes(MapEnvironmentState s, float t)
        {
            if (t >= s.GlassNextPane)
            {
                s.GlassNextPane = t + MapEnvironmentRules.GlassPaneEvery;
                float angle = MapNext(0, Mathf.PI * 2), off = MapNext(0, 1.2f);
                Vector2 at = ClampPosition(Champion.Position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * off, 1);
                s.GlassPanes.Add(new MapEnvironmentState.GlassPane { Position = at, WarnAt = t, Seed = mapRandom.Next(1000) });
            }
            const float r2 = MapEnvironmentRules.GlassPaneRadius * MapEnvironmentRules.GlassPaneRadius;
            foreach (MapEnvironmentState.GlassPane pane in s.GlassPanes)
            {
                if (pane.Broken || t < pane.BreakAt) continue;
                pane.Broken = true;
                if ((Champion.Position - pane.Position).sqrMagnitude < r2)
                    ApplyDamage(Champion, Champion.Definition.Health * MapEnvironmentRules.GlassPaneHero, Faction.Enemy, "A falling glass pane", Champion.Position - pane.Position, injectedDamage: true);
                if (Phase != RunPhase.Combat) return;
                CombatCause shards = NewCause(Champion.Id).Child("map-glass-pane");
                foreach (UnitState u in units.ToArray())
                    if (u.Alive && u.Faction == Faction.Enemy && (u.Position - pane.Position).sqrMagnitude < r2)
                        ApplyDamage(u, u.Definition.Health * (u.Definition.IsBoss ? MapEnvironmentRules.GlassPaneBoss : MapEnvironmentRules.GlassPaneEnemy), Faction.Champion, "Glass pane", u.Position - pane.Position, cause: shards);
            }
            s.GlassPanes.RemoveAll(p => p.Broken && t > p.BreakAt + 2);
        }

        /// <summary>The big buds: held by for 2 s they open into turrets for 15 s, firing seeds at the nearest enemy within 7 units;
        /// a seed lands a moment later through the normal damage path.</summary>
        private void StepGlassBuds(MapEnvironmentState s, float t, float deltaTime)
        {
            const float reach2 = MapEnvironmentRules.GlassBudReach * MapEnvironmentRules.GlassBudReach, range2 = MapEnvironmentRules.GlassTurretRange * MapEnvironmentRules.GlassTurretRange;
            foreach (MapEnvironmentState.GlassBud bud in s.GlassBuds)
            {
                if (bud.Open(t))
                {
                    if (t < bud.NextShot) continue;
                    UnitState target = null; float best = range2;
                    foreach (UnitState u in units)
                    {
                        if (!u.Alive || !CanHit(Faction.Champion, u) || u.EntranceUntil > t) continue;
                        float d = (u.Position - bud.Position).sqrMagnitude; if (d < best) { best = d; target = u; }
                    }
                    if (target == null) { bud.NextShot = t + .1f; continue; }
                    bud.NextShot = t + MapEnvironmentRules.GlassTurretEvery; bud.LastShot = t;
                    Vector2 muzzle = bud.Position + Vector2.up * 1.2f;
                    bud.Aim = (target.Position - muzzle).sqrMagnitude > .01f ? (target.Position - muzzle).normalized : bud.Aim;
                    s.GlassSeeds.Add(new MapEnvironmentState.GlassSeed { From = muzzle, To = target.Position, TargetId = target.Id, Born = t, Arrive = t + MapEnvironmentRules.GlassSeedFlight, Damage = Champion.Definition.Weapon.Damage * MapEnvironmentRules.GlassTurretHit });
                    continue;
                }
                if (t < bud.ReadyAt) { bud.Hold = 0; continue; }
                if ((Champion.Position - bud.Position).sqrMagnitude < reach2) bud.Hold = Mathf.Min(1, bud.Hold + deltaTime / MapEnvironmentRules.GlassBudHold);
                else bud.Hold = Mathf.Max(0, bud.Hold - deltaTime);
                if (bud.Hold >= 1)
                {
                    bud.Hold = 0; bud.OpenAt = t; bud.NextShot = t + .35f;
                    bud.ReadyAt = t + MapEnvironmentRules.GlassTurretSeconds + MapEnvironmentRules.GlassTurretRegrow;
                    MapSay("Flower opened", new Color(.7f, .95f, .6f));
                }
            }
            CombatCause seedCause = default;
            for (int i = s.GlassSeeds.Count - 1; i >= 0; i--)
            {
                MapEnvironmentState.GlassSeed seed = s.GlassSeeds[i];
                UnitState target = UnitWithId(seed.TargetId);
                if (target != null && target.Alive) seed.To = target.Position;
                if (t < seed.Arrive) continue;
                s.GlassSeeds.RemoveAt(i);
                if (target == null || !target.Alive) continue;
                if (seedCause.RootId == 0) seedCause = NewCause(Champion.Id).Child("map-shooting-flower");
                ApplyDamage(target, seed.Damage, Faction.Champion, "Shooting flower", seed.To - seed.From, cause: seedCause);
                if (Phase != RunPhase.Combat) return;
            }
        }

        /// <summary>For captures: blooms in every colour by the hero, a carrier and a flower bed, a pane cracking, one flower firing
        /// and the other being opened, in the rain.</summary>
        private void GlasshouseCaptureFixture()
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed; Vector2 h = Champion.Position;
            Vector2[] near = { new Vector2(-2.6f, 2.2f), new Vector2(-.6f, 2.9f), new Vector2(1.6f, 2.6f), new Vector2(3.4f, 1.4f), new Vector2(-3.6f, -.8f), new Vector2(2.8f, -2.4f) };
            for (int i = 0; i < s.GlassPots.Count && i < near.Length; i++)
            {
                MapEnvironmentState.GlassPot pot = s.GlassPots[i];
                pot.Position = h + near[i]; pot.Colour = i < 5 ? i : -1; pot.Growth = i < 5 ? 1 : .5f; pot.BloomedAt = t - 2;
            }
            s.GlassWeatherShift = MapEnvironmentRules.GlassWeatherSeconds * 1.5f - t; s.GlassWeatherTurn = -1; // mid-rain
            UnitState enemy = null;
            foreach (UnitState u in units) if (u.Alive && u.Faction == Faction.Enemy && !u.Definition.IsBoss && (enemy == null || (u.Position - h).sqrMagnitude < (enemy.Position - h).sqrMagnitude)) enemy = u;
            s.GlassCarriers.Clear();
            if (enemy != null) s.GlassCarriers.Add(new MapEnvironmentState.GlassCarrier { Id = enemy.Id, Last = enemy.Position, Since = t });
            s.GlassNextCarrier = t + 30;
            s.GlassBeds.Clear(); s.GlassBeds.Add(new MapEnvironmentState.GlassBed { Position = h + new Vector2(-1.2f, -2.6f), Born = t - 2, Seed = 7 });
            s.GlassPanes.Clear(); s.GlassPanes.Add(new MapEnvironmentState.GlassPane { Position = h + new Vector2(3.6f, -.6f), WarnAt = t - 1.6f, Seed = 3 }); s.GlassNextPane = t + 30;
            if (s.GlassBuds.Count >= 2)
            {
                s.GlassBuds[0].Position = h + new Vector2(-4.8f, 1.2f); s.GlassBuds[0].OpenAt = t - 3; s.GlassBuds[0].NextShot = t; s.GlassBuds[0].ReadyAt = t + 30;
                s.GlassBuds[1].Position = h + new Vector2(5.2f, -1.4f); s.GlassBuds[1].Hold = .55f; s.GlassBuds[1].ReadyAt = 0; s.GlassBuds[1].OpenAt = -100;
            }
        }
    }
}
