using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    // The Lamp Hall (waves 12-14; Docs/Map_Mechanics_2026-10-25.md section 4). Requirement 25/10/2026: each map's state, tunings
    // and rules live in its own file; MapEnvironmentCombat and MapBoosts only dispatch to the hooks below.

    public sealed partial class MapEnvironmentState
    {
        // Lamp Hall: the lamps and their sweeping light, the snuffer's round, the moths
        public sealed class Lamp { public Vector2 Base, Light; public float Phase, SnuffedAt = -100, SnuffedUntil = -100; public bool Lit = true; }
        public readonly List<Lamp> Lamps = new List<Lamp>();
        public int SnuffTarget = -1; public float ReachAt = -100, NextSnuff;
        public float SnuffAt => ReachAt + MapEnvironmentRules.SnuffReach;
        public readonly List<Vector2> Moths = new List<Vector2>();
        public bool HeroLit, HeroRevealed;
    }

    public static partial class MapEnvironmentRules
    {
        public const float LightRadiusX = 2.8f, LightRadiusY = 1.8f, LightDamage = 1.15f, DarkSpeed = 1.03f, SnuffEvery = 15, SnuffReach = 1.4f, SnuffSeconds = 9, MothSpeed = 2.3f, RevealSpeed = 1.05f;
        // requirement 23/10/2026: friends quicker in the lamp light, enemies tougher in the dark, a snuffed lamp relit by walking to it
        public const float LightArmor = 2, DarkCycle = 1.25f; // the lamp's light lends 2 armour; the drowsy dark slows enemy attacks 20%
        public const float LightHaste = 1.1f, DarkGuard = .85f, RelightReach = 1.3f;
    }

    public sealed partial class CombatSimulation
    {
        private void InitLampHall(MapEnvironmentState s)
        {
            Vector2 a = s.Anchor;
            Vector2[] bases = { new Vector2(-6.2f, 2.6f), new Vector2(6.2f, 2.6f), new Vector2(0, -3.4f) };
            for (int i = 0; i < bases.Length; i++) s.Lamps.Add(new MapEnvironmentState.Lamp { Base = a + bases[i], Phase = i * 2.1f + MapNext(0, 1), Light = a + bases[i] });
            int into = catalog.RunWave - MapIdentity.FirstWave(catalog.RunWave);
            for (int i = 0; i < (into >= 3 ? 2 : 1); i++) s.Moths.Add(s.Lamps[i].Base + Vector2.up * 2);
            s.NextSnuff = 7;
        }

        private void StepLampHall(float deltaTime) => StepLamps(deltaTime);

        /// <summary>A stat the hero gets while standing in a lit lamp's light: +15% damage and 2 armour.</summary>
        private float LampHallStandingBoost(MapStat stat)
        {
            if (!InMapLight(Champion.Position)) return 0;
            return stat == MapStat.Damage ? MapEnvironmentRules.LightDamage - 1 : stat == MapStat.Armor ? MapEnvironmentRules.LightArmor : 0;
        }
        /// <summary>How this map changes one enemy along one axis (1 = unchanged): in the dark it walks a little faster (faster
        /// still near a hero the moths reveal) and attacks slower (the drowsy dark).</summary>
        private float LampHallEnemyFactor(UnitState unit, MapEnemyAxis axis)
        {
            MapEnvironmentState s = mapEnv;
            switch (axis)
            {
                case MapEnemyAxis.Speed:
                    float scale = 1;
                    if (!InMapLight(unit.Position)) scale *= MapEnvironmentRules.DarkSpeed;
                    if (s.HeroRevealed && (unit.Position - Champion.Position).sqrMagnitude < 64) scale *= MapEnvironmentRules.RevealSpeed;
                    return scale;
                case MapEnemyAxis.Cycle:
                    return !InMapLight(unit.Position) ? MapEnvironmentRules.DarkCycle : 1; // the drowsy dark
                default: return 1;
            }
        }
        /// <summary>The attack cooldown factor of the hero or a friend: a friend standing in the light attacks 10% faster.</summary>
        private float LampHallAllyCycle(UnitState unit) => unit.Faction == Faction.Companion && InMapLight(unit.Position) ? 1 / MapEnvironmentRules.LightHaste : 1;
        /// <summary>The hero's pace on this map beyond the move-speed boosts.</summary>
        private float LampHallHeroPace() => 1;
        /// <summary>A fallen enemy's coins on this map.</summary>
        private int LampHallCoins(UnitState target, int coins) => coins;

        private void StepLamps(float deltaTime)
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            s.Shown.Add(MapIdentity.Element.Light);
            foreach (MapEnvironmentState.Lamp lamp in s.Lamps)
            {
                float phase = lamp.Phase + t * .32f;
                lamp.Light = ClampPosition(lamp.Base + new Vector2(Mathf.Cos(phase), Mathf.Sin(phase) * .55f) * 2.4f + Vector2.up * .3f, .5f);
                lamp.Lit = !(t >= lamp.SnuffedAt && t < lamp.SnuffedUntil);
                // the hero walking up to a snuffed lamp lights it again
                if (!lamp.Lit && t >= lamp.SnuffedAt + .6f && ((Champion.Position - lamp.Base).sqrMagnitude < MapEnvironmentRules.RelightReach * MapEnvironmentRules.RelightReach || (Champion.Position - lamp.Light).sqrMagnitude < MapEnvironmentRules.RelightReach * MapEnvironmentRules.RelightReach))
                { lamp.SnuffedUntil = t; lamp.Lit = true; }
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
            s.HeroLit = InMapLight(Champion.Position);
            s.HeroRevealed = false;
            if (t >= 4)
            {
                s.Shown.Add(MapIdentity.Element.Moths);
                for (int i = 0; i < s.Moths.Count; i++)
                {
                    Vector2 goal = Champion.Position + new Vector2(Mathf.Sin(t * 1.7f + i * 2) * .35f, .9f + Mathf.Cos(t * 2.3f + i) * .2f);
                    s.Moths[i] = Vector2.MoveTowards(s.Moths[i], goal, MapEnvironmentRules.MothSpeed * deltaTime);
                    if ((s.Moths[i] - goal).sqrMagnitude < 1) s.HeroRevealed = true;
                }
            }
        }
        /// <summary>The hero stands in a lit lamp's light (the presentation says so over the hero's head).</summary>
        public bool HeroInMapLight => Champion != null && InMapLight(Champion.Position);
        private bool InMapLight(Vector2 position)
        {
            if (mapEnv == null || mapEnv.Theme != MapTheme.LampHall) return false;
            foreach (MapEnvironmentState.Lamp lamp in mapEnv.Lamps)
            {
                if (!lamp.Lit) continue;
                Vector2 d = position - lamp.Light; // the painted light is an oval on the floor; the bonus follows its edge
                if (d.x * d.x / (MapEnvironmentRules.LightRadiusX * MapEnvironmentRules.LightRadiusX) + d.y * d.y / (MapEnvironmentRules.LightRadiusY * MapEnvironmentRules.LightRadiusY) < 1) return true;
            }
            return false;
        }

        /// <summary>For captures: two lamps by the hero, one being snuffed, and the moths.</summary>
        private void LampHallCaptureFixture()
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            s.Lamps[0].Base = Champion.Position + new Vector2(-3.4f, 1.2f); s.Lamps[1].Base = Champion.Position + new Vector2(3.6f, 1.4f);
            s.Lamps[0].Phase = -t * .32f - 0.3f; s.Lamps[1].Phase = -t * .32f + 2.4f;
            s.SnuffTarget = 1; s.ReachAt = t - .7f; s.Lamps[1].SnuffedAt = s.SnuffAt; s.Lamps[1].SnuffedUntil = s.SnuffAt + 9; s.NextSnuff = t + 30;
            for (int i = 0; i < s.Moths.Count; i++) s.Moths[i] = Champion.Position + new Vector2(.6f * i, 1.1f);
        }
    }
}
