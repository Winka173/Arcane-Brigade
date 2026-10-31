using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Draws the roster v2 enemies' marks (WaitingEnemies) in storybook ink, in enemy colours (wine, bile, ember):
    /// the seat ring and its amber glow, the CLOSED ink stamp, the telephone cord and its ring, the knock rings, the tally
    /// marks and their slash, the sitter's thread, the snuffer's smoke to its lantern, PAID stamps over enemies and the
    /// Indebted elite's receipt stamp (IndebtedStamp_v1, once painted). The floor layer sits just above the night fog
    /// (sorting 800) and below every body, so the warnings stay readable in the dark.</summary>
    public sealed class WaitingEnemyVisual : MonoBehaviour
    {
        private InkCanvas floor;
        private ArsenalInk ink;
        private bool drawn;
        private static readonly InkPalette Wine = ArsenalInk.Wine;
        private static readonly InkPalette Bile = new InkPalette(new Color(.2f, .04f, .1f), new Color(.62f, .14f, .32f), new Color(.98f, .68f, .76f), new Color(.95f, .35f, .45f), new Color(.98f, .72f, .35f));
        private static readonly InkPalette Ember = new InkPalette(new Color(.22f, .08f, .05f), new Color(.95f, .45f, .18f), new Color(1, .88f, .62f), new Color(1, .6f, .3f), new Color(.55f, .12f, .2f));
        private static readonly InkPalette Soot = new InkPalette(new Color(.04f, .03f, .06f), new Color(.14f, .12f, .2f), new Color(.55f, .5f, .65f), new Color(.4f, .35f, .6f), new Color(.9f, .5f, .3f));
        private static readonly Color AmberGlow = new Color(1, .66f, .28f);
        // what the last frame showed (read by checks)
        public int Marks { get; private set; }
        public int Stamps { get; private set; }

        private static Color A(Color c, float alpha) { c.a = Mathf.Clamp01(alpha); return c; }

        public void Display(CombatSimulation simulation)
        {
            Marks = Stamps = 0;
            EliteAnnouncer.Watch(simulation);
            bool live = simulation != null && (simulation.Phase == RunPhase.Combat || simulation.Phase == RunPhase.Paused);
            bool any = live && simulation.WaitingMarks.Count > 0;
            Sprite indebted = WaitingEnemyRoster.Current?.IndebtedStamp;
            if (live && !any)
                foreach (UnitState unit in simulation.Units)
                    if (unit.Alive && unit.Faction == Faction.Enemy && (unit.Waiting != null && (unit.Waiting.Paid || unit.Waiting.Kind == WaitingEnemyKind.LampSnuffer && unit.Waiting.Step == 1) || unit.Elite && indebted != null)) { any = true; break; }
            if (!any)
            {
                if (drawn) { floor.Clear(); floor.Commit(); ink.Begin(); ink.End(); drawn = false; }
                return;
            }
            if (floor == null) { floor = new InkCanvas(transform, "Waiting enemy floor ink", 802); ink = new ArsenalInk(transform, 804, 2140); }
            floor.Clear(); ink.Begin(); drawn = true;
            float now = simulation.Elapsed; bool still = PresentationSettings.ReducedMotion;
            foreach (WaitingMark mark in simulation.WaitingMarks) { Draw(mark, now, still); Marks++; }
            foreach (UnitState unit in simulation.Units)
            {
                if (!unit.Alive || unit.Faction != Faction.Enemy) continue;
                float height = unit.Definition.BodyHeight * Mathf.Max(.1f, unit.Definition.BodyScale);
                if (unit.Waiting != null && unit.Waiting.Paid)
                {
                    // PAID: a wine receipt seal stamped over the enemy, with a coin hanging from it
                    float pop = still ? 1 : Mathf.Clamp01((now - unit.Waiting.PaidAt) / .18f);
                    Vector2 over = unit.Position + Vector2.up * (height + .28f + unit.BodyLift);
                    ink.Token(InkToken.Seal, Wine, over, .34f * Mathf.Lerp(1.6f, 1, pop), still ? 0 : -14, pop);
                    ink.Token(InkToken.Coin, ArsenalInk.Gold, over + new Vector2(.16f, -.12f), .15f, 0, .9f * pop);
                    Stamps++;
                }
                if (unit.Elite && indebted != null)
                {
                    ink.Place(indebted, unit.Position + Vector2.up * (height * .5f + unit.BodyLift), .42f * Mathf.Max(.6f, unit.Definition.BodyScale), still ? 0 : -12, new Color(1, 1, 1, .92f), true);
                    Stamps++;
                }
                if (unit.Waiting != null && unit.Waiting.Kind == WaitingEnemyKind.LampSnuffer && unit.Waiting.Step == 1 && simulation.WaitingLantern(unit.Waiting.Lantern, out Vector2 lamp))
                {
                    // a thread of black fog pulled from the flame into the snuffer's cone
                    Vector2 flame = lamp + Vector2.up * 1.05f, hand = unit.Position + Vector2.up * height * .6f;
                    float dim = simulation.LanternDimming(unit.Waiting.Lantern);
                    var path = new List<Vector2>(InkCanvas.Lob(flame, hand, .35f, 14));
                    ink.Ribbon(path, Soot, .07f, 1, 0, .55f + .35f * dim, false);
                    for (int i = 0; i < 3; i++)
                    {
                        float t = still ? (i + .5f) / 3 : Mathf.Repeat(now * .7f + i / 3f, 1);
                        ink.Token(InkToken.Puff, Soot, InkCanvas.Sample(path, t) + Vector2.up * .05f * Mathf.Sin(now * 3 + i), .2f * (1 - .4f * t), 0, .7f * (1 - t * t));
                    }
                }
            }
            floor.Commit(); ink.End();
        }

        private void Draw(WaitingMark m, float now, bool still)
        {
            float lead = Mathf.Max(.05f, m.At - m.Created), charge = Mathf.Clamp01((now - m.Created) / lead);
            bool warning = now < m.At;
            float fade = Mathf.Clamp01((m.Until - now) / .3f);
            float seed = m.A.x * 13 + m.A.y * 7 + m.Index;
            // requirement 1/11/2026: only a boss marks the floor where its blow lands; an ordinary foe's seat, stamp and knock
            // circles are not drawn (the cord, slash lines and tallies are its weapon itself, and stay)
            bool boss = m.Source != null && m.Source.Definition != null && m.Source.Definition.IsBoss;
            if (!boss && (m.Kind == WaitingMarkKind.SeatRing || m.Kind == WaitingMarkKind.ClosedPatch || m.Kind == WaitingMarkKind.KnockRing || m.Kind == WaitingMarkKind.SlashLine || m.Kind == WaitingMarkKind.CordPreview)) return;
            switch (m.Kind)
            {
                case WaitingMarkKind.SeatRing:
                {
                    // the seat unfolds under the hero: amber light pooling toward a broken wine rim that closes as it snaps
                    float alpha = warning ? Mathf.SmoothStep(0, 1, charge) : fade;
                    float r = m.Radius * (!warning && m.Hit ? Mathf.Lerp(.82f, .9f, fade) : 1);
                    // one amber wash over the whole seat, no rim (requirement 25/10/2026)
                    float wash = (warning ? .3f + .15f * charge : .45f) * alpha;
                    floor.Sector(m.A, r, 0, 360, A(AmberGlow, wash), A(AmberGlow, wash));
                    if (!warning && m.Hit) ink.Token(InkToken.Knot, Wine, m.A + Vector2.up * .35f, .34f, still ? 0 : now * 60, fade);
                    break;
                }
                case WaitingMarkKind.ClosedPatch:
                    if (warning)
                    {
                        // the stamp coming down: a seal falling onto one solid patch (danger zones carry no rims or dashes)
                        floor.Sector(m.A, m.Radius, 0, 360, A(Wine.Body, .4f * charge), A(Wine.Body, .4f * charge));
                        ink.Token(InkToken.Seal, Wine, m.A + Vector2.up * (1 - charge) * .9f, .3f + .25f * charge, still ? 0 : -10, charge);
                    }
                    else
                    {
                        float life = Mathf.Clamp01((m.Until - now) / .6f);
                        Vector2 bar = new Vector2(m.Radius * .62f, m.Radius * .14f);
                        floor.Stroke(InkCanvas.Line(m.A - bar, m.A + bar), InkStroke.Of(Soot, .11f, .85f * life, seed));
                        floor.Stroke(InkCanvas.Line(m.A - bar + Vector2.up * .14f, m.A + bar + Vector2.up * .14f), InkStroke.Of(Soot, .05f, .6f * life, seed + 1));
                        ink.Token(InkToken.Seal, Wine, m.A + new Vector2(m.Radius * .55f, m.Radius * .35f), .26f, -10, .8f * life, false);
                    }
                    break;
                case WaitingMarkKind.CordPreview:
                {
                    // the telephone rings: notes jump off it and the cord's path is dotted across the floor
                    Vector2 axis = m.B - m.A; float length = axis.magnitude; if (length < .05f) break;
                    // its path as one solid band growing as it charges (no dotted line)
                    if (charge > .01f) floor.Band(m.A, m.A + axis * Mathf.Clamp01(charge), .16f, A(Wine.Body, .4f * charge), A(Wine.Body, .4f * charge));
                    for (int i = 0; i < 2; i++)
                        ink.Token(InkToken.Note, Bile, m.A + new Vector2((i - .5f) * .4f, .95f + (still ? 0 : Mathf.Abs(Mathf.Sin(now * 14 + i)) * .2f)), .22f, still ? 0 : Mathf.Sin(now * 20 + i) * 20, charge);
                    break;
                }
                case WaitingMarkKind.Cord:
                {
                    // the pinned cord: a coiled inked wire with sparks running along it
                    float alpha = Mathf.Min(fade, Mathf.Clamp01((now - m.At) / .12f));
                    // it writhes: the coil's waves run along it, and a pulse of current travels from the phone to its catch
                    float writhe = still ? 0 : now * 9;
                    // the hurting wire as one solid danger band (requirement 31/10/2026: the writhing inked lines went), sparks running along it
                    floor.Band(m.A, m.B, .18f, A(Wine.Body, .5f * alpha), A(Wine.Body, .5f * alpha));
                    for (int i = 0; i < 3; i++)
                    {
                        float t = still ? (i + .5f) / 3 : Mathf.Repeat(now * 1.3f + i / 3f, 1);
                        ink.Token(InkToken.Spark, Ember, Vector2.Lerp(m.A, m.B, t), .2f, still ? 0 : now * 400 + i * 90, alpha);
                    }
                    break;
                }
                case WaitingMarkKind.KnockRing:
                {
                    // the next knock's reach, inked in as its beat comes round (the knock itself is the shockwave hazard)
                    float k = Mathf.Clamp01(1 - (m.At - now) / CombatSimulation.KnockBeat);
                    if (k <= 0) break;
                    float alpha = warning ? k : fade;
                    floor.Sector(m.A, m.Radius, 0, 360, A(Ember.Body, .28f * alpha), A(Ember.Body, .28f * alpha)); // one wash, no rim
                    break;
                }
                case WaitingMarkKind.Tally:
                {
                    float draw = still ? 1 : Mathf.Clamp01((now - m.Created) / .15f), life = Mathf.Clamp01((m.Until - now) / .5f);
                    floor.Stroke(InkCanvas.Line(m.A, m.B), new InkStroke { Width = .085f, Head = draw, Tail = 0, Taper = 1.1f, Alpha = .95f * life, Ink = Soot.Ink, Body = Soot.Body, Light = Ember.Light, LightOffset = .12f, Seed = seed });
                    ink.Light((m.A + m.B) * .5f, .45f, Ember.Glow, .3f * life);
                    break;
                }
                case WaitingMarkKind.SlashLine:
                    // the ruler line the fifth stroke will follow, drawn out over the wind-up
                    floor.Band(m.A, m.B, m.Radius, A(Ember.Body, .3f * charge), A(Ember.Body, .3f * charge)); // one uniform band, no ruler line over it
                    break;
                case WaitingMarkKind.Slash:
                {
                    float p = Mathf.Clamp01((now - m.At) / Mathf.Max(.05f, m.Until - m.At));
                    float head = still ? 1 : Mathf.Clamp01(p * 3), tail = still ? 0 : Mathf.Clamp01((p - .3f) / .7f);
                    ink.Stroke(m.A, m.B, Ember, .17f, head, tail, 1 - p * p);
                    ink.Stroke(m.A, m.B, Wine, .07f, head, tail, .9f * (1 - p));
                    for (int i = 0; i < 3; i++) ink.Token(InkToken.Page, ArsenalInk.Cream, Vector2.Lerp(m.A, m.B, (i + 1) / 4f) + Vector2.up * p * .4f * (i % 2 == 0 ? 1 : -1), .2f, still ? 0 : p * 300 + i * 70, 1 - p);
                    break;
                }
                case WaitingMarkKind.Thread:
                {
                    // the sitter's ink thread, stretched taut from its sleeve to the hero
                    float alpha = Mathf.Min(fade, Mathf.Clamp01((now - m.At) / .15f));
                    Vector2 sleeve = m.A + Vector2.up * .55f, hero = m.B + Vector2.up * .4f;
                    ink.Cord(sleeve, hero, Soot, .07f, false, alpha);
                    ink.Token(InkToken.Puff, ArsenalInk.Dust, Vector2.Lerp(sleeve, hero, still ? .5f : Mathf.Repeat(now * .5f, 1)), .18f, 0, .6f * alpha);
                    break;
                }
            }
        }

        private void OnDestroy() { ink?.Destroy(); }
    }
}
