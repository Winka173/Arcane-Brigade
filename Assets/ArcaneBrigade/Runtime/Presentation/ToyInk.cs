using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The toy drops drawn in ink (requirement 25/10/2026): the jack-in-the-box's spring boinging up where it opened, snow
    /// drifting over the ground a globe froze, a crust of ice on every frozen enemy, and the horseshoe hanging over the hero while
    /// it pulls the coins (it shrinks away in its last second).</summary>
    public static class ToyInk
    {
        private struct Moment { public Vector2 At; public float Born; public bool Spring; public int Seed; }
        private static readonly List<Moment> moments = new List<Moment>();
        private static InkCanvas ink, light;
        private static GameObject holder;
        private const float SpringLife = .75f, SnowLife = 1.5f;

        public static void Observe(IReadOnlyList<CombatEvent> events)
        {
            foreach (CombatEvent e in events)
                if ((e.Kind == CombatEventKind.SpringBurst || e.Kind == CombatEventKind.GlobeFreeze) && moments.Count < 6)
                    moments.Add(new Moment { At = e.Position, Born = Time.time, Spring = e.Kind == CombatEventKind.SpringBurst, Seed = moments.Count * 17 + (int)(Time.time * 10) });
        }

        public static void Draw(CombatSimulation simulation, Vector2 hero)
        {
            float now = Time.time;
            moments.RemoveAll(m => now - m.Born > (m.Spring ? SpringLife : SnowLife));
            bool magnet = simulation != null && simulation.Elapsed < simulation.MagnetUntil && simulation.Phase == RunPhase.Combat;
            bool frozen = false;
            if (simulation != null) foreach (UnitState u in simulation.Units) if (u.Alive && u.Faction == Faction.Enemy && u.FrozenUntil > simulation.Elapsed) { frozen = true; break; }
            if (moments.Count == 0 && !magnet && !frozen) { if (holder != null && ink.Visible) { ink.Hide(); light.Hide(); } return; }
            if (holder == null) { holder = new GameObject("Toy ink"); ink = new InkCanvas(holder.transform, "Toy ink", 2100); light = new InkCanvas(holder.transform, "Toy light", 2102, true); }
            ink.Clear(); light.Clear();
            bool still = PresentationSettings.ReducedMotion;
            foreach (Moment m in moments)
            {
                float t = (now - m.Born) / (m.Spring ? SpringLife : SnowLife);
                if (m.Spring)
                {
                    // the spring shoots up, overshoots and sags back, a wine-red jester ball on its tip
                    float rise = still ? 1 : Mathf.Sin(Mathf.Min(1, t * 1.6f) * Mathf.PI * .5f) * (1 + .25f * Mathf.Sin(t * 18) * (1 - t));
                    float fade = 1 - Mathf.SmoothStep(.7f, 1, t);
                    Vector2 top = m.At + Vector2.up * (2.4f * rise);
                    ink.Stroke(InkCanvas.Line(m.At, top, .32f, 0, true, m.Seed), InkStroke.Of(ArsenalInk.Gold, .14f, fade, m.Seed));
                    ink.Stroke(InkCanvas.Arc(top + Vector2.up * .3f, .2f, .2f, 0, 360), InkStroke.Of(ArsenalInk.Wine, .32f, fade, m.Seed + 1));
                    light.GlowRibbon(InkCanvas.Arc(top + Vector2.up * .3f, .14f, .14f, 200, 320), 1, 0, .1f, new Color(1f, .9f, .75f, .8f * fade));
                    for (int side = -1; side <= 1; side += 2) // the boing: two short bent strokes either side
                        ink.Stroke(InkCanvas.Arc(top + Vector2.right * side * .7f, .4f, .55f, side < 0 ? 120 : -60, side < 0 ? 240 : 60), InkStroke.Of(ArsenalInk.Cream, .1f, fade * (1 - t), m.Seed + 2 + side));
                }
                else
                {
                    // snow drifts down over the frozen ground and melts away
                    for (int i = 0; i < 16; i++)
                    {
                        float a = (m.Seed * 13 + i * 137.5f) * Mathf.Deg2Rad, r = CombatSimulation.GlobeRadius * Mathf.Sqrt(((i * 7 + m.Seed) % 16) / 16f);
                        Vector2 p = m.At + new Vector2(Mathf.Cos(a) * r, Mathf.Sin(a) * r * .7f) + Vector2.down * (still ? 0 : t * .5f) + Vector2.up * .6f;
                        Flake(p, .22f + .07f * (i % 3), (1 - t) * .95f, i);
                    }
                }
            }
            if (frozen)
                foreach (UnitState u in simulation.Units)
                {
                    if (!u.Alive || u.Faction != Faction.Enemy || u.FrozenUntil <= simulation.Elapsed) continue;
                    float left = Mathf.Clamp01((u.FrozenUntil - simulation.Elapsed) / .3f), size = Mathf.Clamp(u.Definition.Radius * 1.4f, .35f, 1.2f);
                    Vector2 foot = u.Position + Vector2.down * u.Definition.Radius * .6f;
                    for (int i = 0; i < 6; i++) // a crust of ice crystals round the feet
                    {
                        float x = (i / 5f - .5f) * size * 1.6f, h = size * (.45f + .35f * ((i * 3 + u.Id) % 4) / 3f);
                        Vector2 b = foot + Vector2.right * x, tip = b + new Vector2(x * .25f, h);
                        ink.Stroke(InkCanvas.Line(b, tip), InkStroke.Of(ArsenalInk.Glass, .15f, .9f * left, u.Id + i));
                    }
                    light.GlowRibbon(InkCanvas.Line(foot + new Vector2(-size * .5f, size * .5f), foot + new Vector2(size * .3f, size * .9f)), 1, 0, .05f, new Color(.85f, .97f, 1f, .7f * left));
                }
            if (magnet)
            {
                // the horseshoe over the hero: red, silver-tipped, bobbing; it shrinks away in its last second
                float scale = Mathf.Clamp01(simulation.MagnetUntil - simulation.Elapsed);
                Vector2 c = hero + Vector2.up * (2.1f + (still ? 0 : Mathf.Sin(now * 3) * .08f));
                float rx = .5f * scale, ry = .58f * scale;
                ink.Stroke(InkCanvas.Arc(c, rx, ry, 180, 360), InkStroke.Of(ArsenalInk.Wine, .26f * scale, 1, 7));
                for (int side = -1; side <= 1; side += 2)
                {
                    ink.Stroke(InkCanvas.Line(c + Vector2.right * side * rx, c + new Vector2(side * rx, ry * .55f)), InkStroke.Of(ArsenalInk.Wine, .26f * scale, 1, 8 + side));
                    ink.Stroke(InkCanvas.Line(c + new Vector2(side * rx, ry * .55f), c + new Vector2(side * rx, ry * .95f)), InkStroke.Of(ArsenalInk.Silver, .26f * scale, 1, 10 + side));
                }
                light.GlowRibbon(InkCanvas.Arc(c, rx * .8f, ry * .8f, 200, 260), 1, 0, .1f * scale, new Color(1f, .85f, .8f, .7f));
            }
            ink.Commit(); light.Commit();
        }
        private static void Flake(Vector2 p, float size, float alpha, int seed)
        {
            for (int k = 0; k < 3; k++)
            {
                float a = (k * 60 + seed * 11) * Mathf.Deg2Rad; Vector2 d = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * size;
                ink.Stroke(InkCanvas.Line(p - d, p + d), InkStroke.Of(ArsenalInk.Glass, .06f, alpha, seed + k));
            }
            light.GlowRibbon(InkCanvas.Line(p - Vector2.right * size * .4f, p + Vector2.right * size * .4f), 1, 0, size * .5f, new Color(.9f, .98f, 1f, .6f * alpha));
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { moments.Clear(); holder = null; ink = light = null; }
    }
}
