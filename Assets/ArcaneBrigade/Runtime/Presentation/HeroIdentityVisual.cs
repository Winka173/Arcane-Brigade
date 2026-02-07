using System.Collections.Generic;
using UnityEngine;
namespace ArcaneBrigade
{
    /// <summary>Paints every playable hero's identity mechanic. Two layers: a ground layer of watercolour washes and inked
    /// rims that show the exact area the simulation tests (never larger or smaller), and an air layer of brush strokes and
    /// signature tokens that animate anticipation, action and follow-through inside that area. Live mechanic state (marks,
    /// roots, dreams, stored reflections, routes) and the resource gauge are drawn from read-only simulation views.</summary>
    public sealed class HeroIdentityVisual : MonoBehaviour
    {
        private const int GroundOrder = 74, AirOrder = 2060, GroundTokenOrder = 77, AirTokenOrder = 2064;
        private const float CrescentHalf = 84.26f; // the simulation accepts dot(direction) >= .1
        private InkCanvas ground, air;
        private readonly List<SpriteRenderer> tokens = new List<SpriteRenderer>();
        private SpriteRenderer ghost;
        private int used;
        private float lastCast = -100, castShown = -100, gaugeFull = -100;
        private int lastStored, lastCharge;
        private float pipLitAt;
        private CombatSimulation sim;
        private HeroMechanicStyle style;
        private InkPalette ink;
        private float t, strength;
        private bool still, sparse, calm, plain;
        public int VisibleCount { get; private set; }
        public int TokenCount => used;

        private static float OutCubic(float x) { x = Mathf.Clamp01(x); return 1 - (1 - x) * (1 - x) * (1 - x); }
        private static float InQuad(float x) { x = Mathf.Clamp01(x); return x * x; }
        private static float OutBack(float x) { x = Mathf.Clamp01(x); const float c = 1.6f; float y = x - 1; return 1 + (c + 1) * y * y * y + c * y * y; }
        private static float Smooth(float a, float b, float x) => Mathf.SmoothStep(0, 1, Mathf.InverseLerp(a, b, x));
        private static Vector2 Dir(float degrees) => new Vector2(Mathf.Cos(degrees * Mathf.Deg2Rad), Mathf.Sin(degrees * Mathf.Deg2Rad));
        private static Color A(Color c, float alpha) { c.a = Mathf.Clamp01(alpha); return c; }

        private void Ensure()
        {
            if (ground != null) return;
            ground = new InkCanvas(transform, "Hero mechanic ground ink", GroundOrder);
            air = new InkCanvas(transform, "Hero mechanic air ink", AirOrder);
            var go = new GameObject("Dream identity portrait"); go.transform.SetParent(transform, false);
            ghost = go.AddComponent<SpriteRenderer>(); ghost.sortingOrder = 1090; ghost.enabled = false;
        }

        private SpriteRenderer Place(Sprite sprite, Vector2 at, Vector2 scale, float angle, Color color, bool airborne)
        {
            if (sprite == null || color.a <= .004f) return null;
            if (used == tokens.Count)
            {
                var go = new GameObject("Hero mechanic token " + used); go.transform.SetParent(transform, false);
                tokens.Add(go.AddComponent<SpriteRenderer>());
            }
            var r = tokens[used++]; r.enabled = true; r.sprite = sprite; r.color = color; r.sortingOrder = airborne ? AirTokenOrder : GroundTokenOrder;
            float unit = Mathf.Max(.01f, Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y)); // procedural tokens are 1 unit; painted art may differ
            r.transform.position = at; r.transform.rotation = Quaternion.Euler(0, 0, angle); r.transform.localScale = new Vector3(scale.x / unit, scale.y / unit, 1);
            return r;
        }
        private void Token(InkToken kind, Vector2 at, float size, float angle, float alpha, bool airborne = true, int face = 0, InkPalette? palette = null, float squash = 1)
            => Place(MechanicInk.Token(kind, palette ?? ink, face), at, new Vector2(size / squash, size * squash), still ? 0 : angle, new Color(1, 1, 1, alpha * strength), airborne);
        private void DimToken(InkToken kind, Vector2 at, float size, float alpha)
            => Place(MechanicInk.Token(kind, ink), at, Vector2.one * size, 0, new Color(.42f, .42f, .5f, alpha * strength), false);
        private void Glow(Vector2 at, float diameter, Color color, float alpha, bool airborne = false)
        {
            if (plain) alpha *= .55f;
            Place(MechanicInk.Glow, at, Vector2.one * diameter, 0, A(color, alpha * strength), airborne);
        }
        private InkStroke Brush(float width, float alpha, float seed) => InkStroke.Of(ink, width, alpha * strength, seed);
        private float Bloom => (calm ? .14f : plain ? .2f : .34f) * strength;

        public void Display(CombatSimulation simulation)
        {
            Ensure();
            ground.Clear(); air.Clear(); used = 0; VisibleCount = 0; ghost.enabled = false;
            bool live = simulation != null && simulation.HeroIdentityEnabled && simulation.Champion != null && (simulation.Phase == RunPhase.Combat || simulation.Phase == RunPhase.Paused);
            if (live)
            {
                sim = simulation; t = simulation.Elapsed;
                style = HeroMechanicStyle.For(simulation.Champion.Definition.Id); ink = style.PaletteFor(simulation.HeroIdentityApproach);
                still = PresentationSettings.ReducedMotion; sparse = PresentationSettings.ReducedEffects || AnimationDetailBudget.Level >= 2;
                calm = PresentationSettings.ReducedFlashes; plain = PresentationSettings.HighReadability;
                strength = Mathf.Lerp(1, .72f, EffectDensity.CrowdPressure) * (sparse ? .85f : 1);
                if (simulation.Champion.Alive)
                {
                    DrawState();
                    DrawGauge();
                    DrawCastCue();
                }
                foreach (var glyph in simulation.HeroIdentityGlyphs)
                {
                    if (VisibleCount >= EffectDensity.Limit(28)) break;
                    if (DrawGlyph(glyph)) VisibleCount++;
                }
            }
            ground.Commit(); air.Commit();
            for (int i = used; i < tokens.Count; i++) tokens[i].enabled = false;
        }

        // ------------------------------------------------------------------ gauge
        private int GaugeCount()
        {
            if (style.Gauge != HeroGauge.Pips) return 0;
            int n = Mathf.RoundToInt(sim.HeroIdentityRequired);
            return n >= 1 && n <= 8 ? n : 0;
        }
        /// <summary>A small inked gauge at the hero's feet in the hero's own tokens: unlit silhouettes fill one by one.</summary>
        private void DrawGauge()
        {
            Vector2 feet = Feet(sim.Champion) + Vector2.down * .04f;
            float charge = sim.HeroIdentityCharge, required = Mathf.Max(.01f, sim.HeroIdentityRequired), fraction = Mathf.Clamp01(charge / required);
            string id = style.Id;
            if (id == "PatchworkHost" && sim.HeroIdentityLead == null) return;
            int lit = Mathf.FloorToInt(charge + .001f);
            if (lit > lastCharge) pipLitAt = t; lastCharge = lit;
            if (fraction >= 1 && gaugeFull < 0) gaugeFull = t; else if (fraction < 1) gaugeFull = -1;
            float ready = fraction >= 1 && !still ? .5f + .5f * Mathf.Sin((t - gaugeFull) * 6) : 0;
            float rx = plain ? .76f : .68f, ry = rx * .44f, from = 204, to = 336;
            ground.Stroke(InkCanvas.Arc(feet, rx, ry, from - 4, to + 4), new InkStroke { Width = .035f, Head = 1, Alpha = .38f * strength, Ink = ink.Ink, Body = ink.Ink, Light = ink.Light, InkOnly = true, Taper = 1 });
            int count = GaugeCount();
            if (count > 0)
            {
                InkToken pip = id == "GlassDice" ? InkToken.Coin : style.Token;
                InkPalette? pal = id == "GlassDice" ? GoldPalette : (InkPalette?)null;
                float size = (plain ? .23f : .2f) * (count > 6 ? .86f : 1);
                for (int i = 0; i < count; i++)
                {
                    float a = count == 1 ? 270 : Mathf.Lerp(from + 10, to - 10, i / (float)(count - 1));
                    Vector2 at = feet + new Vector2(Mathf.Cos(a * Mathf.Deg2Rad) * rx, Mathf.Sin(a * Mathf.Deg2Rad) * ry);
                    if (id == "PetalCitadel") { PetalPip(i, at, size, i < lit); continue; }
                    if (i < lit)
                    {
                        float pop = i == lit - 1 && !still ? 1 + .45f * (1 - OutCubic((t - pipLitAt) / .25f)) : 1;
                        Token(pip, at + Vector2.up * ready * .03f, size * pop, (i - count * .5f) * 6, .95f, false, 0, pal);
                    }
                    else DimToken(pip, at, size * .82f, .62f);
                }
            }
            else
            {
                // continuous resources (distance, time, steam, debt): an inked arc that fills toward a token at its head
                float head = Mathf.Lerp(from, to, fraction);
                if (fraction > .01f) ground.Stroke(InkCanvas.Arc(feet, rx, ry, from, head), Brush(.085f, .9f, 2.3f));
                Vector2 at = feet + new Vector2(Mathf.Cos(head * Mathf.Deg2Rad) * rx, Mathf.Sin(head * Mathf.Deg2Rad) * ry);
                Token(style.Token, at + Vector2.up * ready * .03f, (fraction >= 1 ? .22f : .16f) * (1 + ready * .12f), 0, fraction > .01f ? .95f : .5f, false);
            }
            if (id == "GlassDice")
            {
                Vector2 die = feet + new Vector2(rx + .16f, -.02f);
                Token(InkToken.Die, die, .26f * (1 + ready * .1f), still ? 0 : Mathf.Sin(t * 2) * 6, 1, false, sim.HeroIdentityNextFace);
            }
            if (fraction >= 1 && !calm) Glow(feet, rx * 2.2f, ink.Glow, .1f + ready * .06f);
        }
        private static readonly InkPalette GoldPalette = new InkPalette(new Color(.24f, .13f, .03f), new Color(.96f, .74f, .28f), new Color(1, .95f, .74f), new Color(1, .8f, .4f), new Color(.6f, .38f, .16f));
        private static readonly Color[] PetalColours = { new Color(.96f, .48f, .56f), new Color(.34f, .7f, .78f), new Color(.5f, .86f, .5f), new Color(.5f, .66f, 1f) };
        private void PetalPip(int petal, Vector2 at, float size, bool lit)
        {
            int growth = sim.HeroIdentityPetal(petal);
            var p = ink; p.Body = PetalColours[petal % 4];
            float grown = growth <= 0 ? .7f : 1 + growth * .12f;
            if (lit && growth > 0) Token(InkToken.Petal, at, size * grown, (petal - 1.5f) * 18, .95f, false, 0, p);
            else Place(MechanicInk.Token(InkToken.Petal, p), at, Vector2.one * size * grown * .85f, (petal - 1.5f) * 18, new Color(.45f, .45f, .52f, (growth > 0 ? .6f : .3f) * strength), false);
        }

        // ------------------------------------------------------------------ live state
        // unit positions are body centres: feet sit half a body below, heads half a body above
        private static Vector2 Head(UnitState u) => u.Position + Vector2.up * (u.Definition.BodyHeight * .5f + .2f);
        private static Vector2 Chest(UnitState u) => u.Position + Vector2.up * u.Definition.BodyHeight * .05f;
        private static Vector2 Feet(UnitState u) => u.Position - Vector2.up * u.Definition.BodyHeight * .5f;
        private float Bob(float phase) => still ? 0 : Mathf.Sin(t * 3.1f + phase) * .045f;

        private void DrawState()
        {
            string id = style.Id; var hero = sim.Champion;
            // Marked addresses / knots
            if (id == "Postmaster" || id == "LanternWeaver")
            {
                int n = sim.HeroIdentityMarkCount; UnitState previous = null;
                for (int i = 0; i < n; i++)
                {
                    var u = sim.HeroIdentityMarked(i); if (u == null || !u.Alive) continue;
                    float life = Mathf.Clamp01((sim.HeroIdentityMarkUntil(i) - t) / .5f);
                    if (id == "Postmaster")
                    {
                        Token(InkToken.Seal, Head(u) + Vector2.up * Bob(i), .3f, -8 + i * 9, .95f * life);
                    }
                    else
                    {
                        Token(InkToken.Knot, Chest(u), .3f, still ? 0 : t * 40 + i * 50, .95f * life);
                        if (previous != null) Thread(Chest(previous), Chest(u), .55f * life, i);
                        previous = u;
                    }
                    VisibleCount++;
                }
                if (id == "LanternWeaver" && n > 0)
                {
                    int approach = sim.HeroIdentityApproach;
                    if (approach == 0 && n >= 3 && previous != null && sim.HeroIdentityMarked(0).Alive) Thread(Chest(previous), Chest(sim.HeroIdentityMarked(0)), .32f, 7);
                    if (approach == 1 && sim.HeroIdentityMarked(0).Alive) Thread(Chest(hero), Chest(sim.HeroIdentityMarked(0)), .32f, 9);
                    if (approach == 2) for (int i = 0; i < n; i++) if (sim.HeroIdentityMarked(i).Alive) Thread(Chest(hero), Chest(sim.HeroIdentityMarked(i)), .18f, 11 + i);
                }
            }
            var bounty = sim.HeroIdentityBounty;
            if (id == "TreasureAcorn" && bounty != null)
            {
                Vector2 tag = Head(bounty) + Vector2.up * Bob(1);
                Token(InkToken.Coin, tag, .32f, still ? 0 : Mathf.Sin(t * 2.4f) * 10, 1);
                int stamps = Mathf.FloorToInt(sim.HeroIdentityCharge + .01f);
                for (int i = 0; i < 3; i++)
                {
                    Vector2 at = tag + new Vector2((i - 1) * .17f, -.24f);
                    if (i < stamps) Token(InkToken.Seal, at, .13f, 0, 1, true, 0, StampPalette); else Place(MechanicInk.Token(InkToken.Seal, StampPalette), at, Vector2.one * .11f, 0, new Color(.4f, .4f, .46f, .55f * strength), true);
                }
                VisibleCount++;
            }
            if (id == "MoonProwler" && bounty != null && sim.HeroIdentityCharge >= 5)
            {
                // the rear cone the simulation accepts (dot <= -.25 behind the fixed facing)
                Vector2 rear = -sim.HeroIdentityRearDirection; if (rear.sqrMagnitude < .01f) rear = Vector2.left;
                float a = Mathf.Atan2(rear.y, rear.x) * Mathf.Rad2Deg, half = 75.5f, radius = 1.05f;
                float breathe = still ? 1 : .8f + .2f * Mathf.Sin(t * 3);
                Vector2 apex = Feet(bounty);
                ground.Sector(apex, radius, a - half, a + half, A(ink.Glow, .04f), A(ink.Body, .3f * breathe * strength));
                ground.Stroke(InkCanvas.Arc(apex, radius - .03f, radius - .03f, a - half, a + half), Brush(.06f, .7f, 4));
                Token(InkToken.Moon, bounty.Position + rear * .7f + Vector2.up * (.35f + Bob(2)), .3f, a, .95f);
                VisibleCount++;
            }
            if (id == "DrowsySeer" && sim.HeroIdentityDreaming) Dream();
            if (id == "RootOracle") Roots();
            if (id == "MirrorKeeper" && sim.HeroIdentityStored > 0)
            {
                int n = sim.HeroIdentityStored;
                for (int i = 0; i < n; i++)
                {
                    float a = (still ? 90 : t * 70) + i * 360f / n; Vector2 at = Chest(hero) + new Vector2(Mathf.Cos(a * Mathf.Deg2Rad) * .62f, Mathf.Sin(a * Mathf.Deg2Rad) * .3f);
                    Token(InkToken.Shard, at, .24f, a + 90, .95f);
                }
                VisibleCount++;
            }
            if ((id == "CometRunner" || id == "CitrusMarshal") && sim.HeroIdentityRoute.Count >= 2) Route();
            if (id == "PatchworkHost" && sim.HeroIdentityLead != null && sim.HeroIdentityLead.Alive)
            {
                var lead = sim.HeroIdentityLead; float ready = Mathf.Clamp01(sim.HeroIdentityCharge / 4);
                ground.Stroke(InkCanvas.Arc(Feet(lead), .5f, .23f, 0, 360), Brush(.05f, .35f + .4f * ready, 6), true);
                Token(InkToken.Star, Head(lead) + Vector2.up * Bob(3), .22f + .08f * ready, still ? 0 : t * 30, .9f, true, 0, GoldPalette);
            }
            if (id == "HourglassStag" && sim.HeroIdentityBankAge >= 0)
            {
                float due = sim.HeroIdentityApproach == 0 ? 4 : sim.HeroIdentityApproach == 1 ? 3 : 5, k = Mathf.Clamp01(sim.HeroIdentityBankAge / due);
                Vector2 feet = Feet(hero);
                ground.Stroke(InkCanvas.Arc(feet, .86f, .4f, 90, 90 - 360 * k), Brush(.06f, .7f, 8));
                Vector2 at = feet + new Vector2(Mathf.Cos((90 - 360 * k) * Mathf.Deg2Rad) * .86f, Mathf.Sin((90 - 360 * k) * Mathf.Deg2Rad) * .4f);
                Token(InkToken.Sand, at, .2f, 0, .9f, false);
            }
        }
        private static readonly InkPalette StampPalette = new InkPalette(new Color(.22f, .05f, .06f), new Color(.72f, .16f, .2f), new Color(1, .9f, .8f), new Color(1, .6f, .5f), new Color(.72f, .16f, .2f));
        private void Thread(Vector2 a, Vector2 b, float alpha, int seed)
        {
            var path = InkCanvas.Lob(a, b, -.12f, 14);
            air.Stroke(path, new InkStroke { Width = .05f, Head = 1, Alpha = alpha * strength, Ink = ink.Ink, Body = ink.Body, Light = ink.Light, NoLight = true, Taper = .4f, Seed = seed });
        }
        private void Dream()
        {
            Vector2 at = sim.HeroIdentityDreamPosition; float left = Mathf.Clamp01((sim.HeroIdentityDreamUntil - t) / 4f);
            float fadeIn = Mathf.Clamp01((4 - (sim.HeroIdentityDreamUntil - t)) / .35f), fade = Mathf.Min(fadeIn, Mathf.Clamp01((sim.HeroIdentityDreamUntil - t) / .3f));
            Vector2 floor = at - Vector2.up * sim.Champion.Definition.BodyHeight * .5f;
            ground.Stroke(InkCanvas.Arc(floor, .66f, .3f, 90, 90 + 360 * left), Brush(.08f, .9f * fade, 3));
            Glow(floor, 1.5f, ink.Glow, .22f * fade);
            var body = sim.Champion.Definition.Body;
            if (body != null)
            {
                Rect inkBox = SpriteInkMetrics.BoundsOf(body); float scale = sim.Champion.Definition.BodyHeight / Mathf.Max(.01f, inkBox.height);
                ghost.enabled = true; ghost.sprite = body; ghost.transform.localScale = Vector3.one * scale;
                ghost.transform.position = (Vector3)(at + Vector2.up * (.12f + Bob(0) * 2)) - (Vector3)(inkBox.center * scale);
                ghost.color = new Color(ink.Glow.r, ink.Glow.g, ink.Glow.b, .42f * fade * EffectDensity.AlliedAlpha / .72f);
            }
            if (!sparse && !still)
                for (int i = 0; i < 3; i++)
                {
                    float k = Mathf.Repeat(t * .55f + i / 3f, 1);
                    Token(InkToken.Star, at + new Vector2(.22f + i * .08f + Mathf.Sin(k * 6 + i) * .08f, sim.Champion.Definition.BodyHeight * .8f + k * .6f), .12f + .05f * (1 - k), k * 90, fade * Mathf.Sin(k * Mathf.PI));
                }
            VisibleCount++;
        }
        private void Roots()
        {
            var hero = sim.Champion; float charge = Mathf.Clamp01(sim.HeroIdentityCharge / 3); int next = sim.HeroIdentityRootTurn;
            for (int i = 0; i < 3; i++)
            {
                Vector2 at = hero.Position + (Vector2)(Quaternion.Euler(0, 0, 90 + i * 120) * Vector2.right) * 1.8f;
                Vector2 from = hero.Position + (at - hero.Position).normalized * .38f;
                bool turn = i == next; float k = turn ? charge : 0;
                ground.Stroke(InkCanvas.Line(from, at, .07f, 6, false, i * 2.1f), Brush(.07f + .03f * k, .42f + .3f * k, i));
                if (turn && k > .05f && !calm) Glow(at, .9f, ink.Glow, .12f + .18f * k);
                Token(InkToken.Leaf, at + Vector2.up * .06f, .26f + .08f * k * (still ? 0 : 1), 90 + i * 120 - 90 + (still ? 0 : Mathf.Sin(t * 2 + i) * 8), .7f + .3f * k, false);
            }
            VisibleCount++;
        }
        private void Route()
        {
            var route = sim.HeroIdentityRoute; float fraction = Mathf.Clamp01(sim.HeroIdentityCharge / Mathf.Max(.01f, sim.HeroIdentityRequired));
            for (int i = 1; i < route.Count; i++)
            {
                Vector2 a = route[i - 1], b = route[i]; float length = Vector2.Distance(a, b); if (length < .05f) continue;
                if (sim.HeroIdentityApproach == 1)
                    for (int sign = -1; sign <= 1; sign += 2) ground.Stroke(InkCanvas.Line(a + Vector2.right * .4f * sign, b + Vector2.right * .4f * sign), Brush(.035f, .18f + .3f * fraction, i));
                else ground.Stroke(InkCanvas.Line(a, b), Brush(.04f, .18f + .32f * fraction, i));
                if (sparse) continue;
                int dots = Mathf.Clamp(Mathf.FloorToInt(length / .45f), 1, 10);
                for (int d = 0; d < dots; d++)
                {
                    float k = (d + .5f) / dots; float flicker = still ? 1 : .75f + .25f * Mathf.Sin(t * 9 + d * 1.7f + i);
                    Token(style.Token, Vector2.Lerp(a, b, k), .1f + .05f * fraction, d * 40, (.25f + .5f * fraction) * flicker, false);
                }
            }
        }

        // ------------------------------------------------------------------ cast cue
        private void DrawCastCue()
        {
            if (sim.HeroIdentityLastCast > lastCast + .001f) { lastCast = sim.HeroIdentityLastCast; castShown = lastCast; }
            float k = (t - castShown) / .5f; if (k < 0 || k >= 1) return;
            Vector2 feet = Feet(sim.Champion); float e = OutCubic(k);
            float rx = still ? .8f : Mathf.Lerp(.32f, .95f, e);
            ground.Stroke(InkCanvas.Arc(feet, rx, rx * .46f, 0, 360), Brush(.1f * (1 - k * .7f), 1 - k, 5), true);
            if (!calm) Glow(feet + Vector2.up * .3f, 1.6f, ink.Glow, .26f * (1 - k));
            if (sparse) return;
            for (int i = 0; i < 3; i++)
            {
                float a = 60 + i * 30; Vector2 rise = still ? new Vector2((i - 1) * .3f, .9f) : new Vector2((i - 1) * .32f * e, .4f + .75f * e);
                Token(style.Token, feet + rise + Vector2.up * sim.Champion.Definition.BodyHeight * .3f, .24f * (1 - k * .35f), (i - 1) * 25 + k * 40, 1 - InQuad(k));
            }
        }

        // ------------------------------------------------------------------ glyphs
        private bool DrawGlyph(HeroIdentityGlyph g)
        {
            switch (g.Role)
            {
                case HeroGlyphRole.Mark: case HeroGlyphRole.Ambient: return false; // drawn from live state
                case HeroGlyphRole.Strike:
                    if (g.Shape == MorphGeometry.Line) LineStrike(g);
                    else if (g.Shape == MorphGeometry.Crescent) CrescentStrike(g);
                    else if (g.Shape == MorphGeometry.Ring) RingStrike(g);
                    else DiscStrike(g, g.Shape == MorphGeometry.Seal);
                    return true;
                case HeroGlyphRole.Link: Link(g); return true;
                case HeroGlyphRole.Lob: Lob(g); return true;
                case HeroGlyphRole.Catch: Catch(g); return true;
                case HeroGlyphRole.Poise: Poise(g); return true;
                default: Emblem(g); return true;
            }
        }
        private void Phases(HeroIdentityGlyph g, out bool pre, out float q, out float p, out float fade)
        {
            float anticipation = g.At - g.Created;
            pre = t < g.At;
            q = anticipation > .001f ? Mathf.Clamp01((t - g.Created) / anticipation) : 1;
            p = Mathf.Clamp01((t - g.At) / Mathf.Max(.05f, g.Until - g.At));
            fade = 1 - Smooth(.55f, 1, p);
        }
        private float Seed(HeroIdentityGlyph g) => (g.Seed * 37) % 360;

        private void DiscStrike(HeroIdentityGlyph g, bool seal)
        {
            Phases(g, out bool pre, out float q, out float p, out float fade);
            Vector2 c = g.Position; float r = Mathf.Max(.2f, g.Radius), s = Seed(g);
            if (pre)
            {
                ground.Sector(c, r, 0, 360, A(ink.Glow, .015f), A(ink.Body, (.06f + .12f * q) * strength));
                float reveal = still ? 1 : OutCubic(q);
                ground.Stroke(InkCanvas.Arc(c, r - .03f, r - .03f, s, s + 360 * reveal), new InkStroke { Width = .06f, Head = 1, Alpha = (.45f + .3f * q) * strength, Ink = ink.Ink, Body = ink.Body, Light = ink.Light, NoLight = true, Taper = .6f }, reveal >= 1);
                if (!sparse)
                    for (int i = 0; i < 4; i++)
                    {
                        float a = s + i * 90 + (still ? 0 : q * 40); float d = still ? r * .6f : Mathf.Lerp(r * .9f, r * .3f, OutCubic(q));
                        Token(style.Token, c + Dir(a) * d, .15f * (.6f + .4f * q), a, .8f * q);
                    }
                return;
            }
            float flash = calm ? 0 : .14f * (1 - Smooth(0, .2f, p));
            ground.Sector(c, r, 0, 360, A(ink.Glow, (.12f + flash) * fade * strength), A(ink.Body, (.42f + flash) * fade * strength));
            if (!calm) Glow(c, r * 2, ink.Glow, .22f * (1 - p));
            float w = Mathf.Lerp(.16f, .07f, OutCubic(p));
            ground.Stroke(InkCanvas.Arc(c, r - w * .5f, r - w * .5f, s, s + 360), Brush(w, fade, s), true);
            if (!sparse && r >= .55f) ground.Ticks(c, r - w, r >= 1.4f ? 16 : 8, Mathf.Min(.2f, r * .14f), .035f, s + (still ? 0 : OutCubic(p) * 12), A(ink.Ink, .75f * fade * strength));
            if (!still && !sparse) air.Stroke(InkCanvas.Arc(c, r * OutCubic(Mathf.Min(1, p / .5f)) * .97f, r * OutCubic(Mathf.Min(1, p / .5f)) * .97f, 0, 360), new InkStroke { Width = .05f, Head = 1, Alpha = .65f * (1 - p) * strength, Ink = ink.Light, Body = ink.Light, InkOnly = true, Taper = 1 }, true);
            int n = sparse ? 3 : 6;
            for (int i = 0; i < n; i++)
            {
                float a = s + i * 360f / n + (i % 2) * 17; float d = Mathf.Min(r - .12f, r * .82f * (still ? .7f : OutCubic(p)));
                Token(style.Token, c + Dir(a) * Mathf.Max(0, d), .22f * (1 + .5f * Mathf.Pow(1 - p, 3)), a + p * 120, fade);
            }
            if (seal || style.Id == "SupperMortar")
            {
                float stamp = still ? 1 : OutBack(Mathf.Min(1, p / .28f)), size = Mathf.Min(r * 1.25f, 1.15f) * Mathf.Lerp(1.5f, 1, stamp);
                float squash = still ? 1 : 1 - .16f * Mathf.Sin(Mathf.Clamp01(p / .4f) * Mathf.PI);
                Token(seal ? InkToken.Seal : style.Token, c + Vector2.up * .05f, size * (seal ? 1 : .55f), -6, Mathf.Clamp01(p / .12f) * fade, true, 0, null, squash);
            }
        }

        private void LineStrike(HeroIdentityGlyph g)
        {
            Phases(g, out bool pre, out float q, out float p, out float fade);
            Vector2 a = g.Position, b = g.End; float half = Mathf.Max(.1f, g.Width), length = Vector2.Distance(a, b);
            if (length < .05f) return;
            Vector2 axis = (b - a) / length, normal = new Vector2(-axis.y, axis.x);
            if (pre)
            {
                ground.Band(a, b, half, A(ink.Body, .02f * strength), A(ink.Body, (.05f + .1f * q) * strength));
                float reveal = still ? 1 : OutCubic(q);
                ground.Stroke(InkCanvas.Line(a, Vector2.Lerp(a, b, reveal)), new InkStroke { Width = .05f, Head = 1, Alpha = (.4f + .35f * q) * strength, Ink = ink.Ink, Body = ink.Body, NoLight = true, Taper = .5f });
                return;
            }
            ground.Band(a, b, half, A(ink.Glow, .16f * fade * strength), A(ink.Body, .42f * fade * strength));
            float head = still ? 1 : OutCubic(Mathf.Min(1, p / .3f)), tail = still ? 0 : InQuad(Mathf.Max(0, (p - .3f) / .7f));
            float seed = Seed(g) * .1f;
            List<Vector2> path; float width;
            switch (style.Brush)
            {
                case InkBrush.Bolt: path = InkCanvas.Line(a, b, half * .48f, 0, true, g.Seed); width = .17f; break;
                case InkBrush.Wave: path = InkCanvas.Line(a, b, half * .3f, 9, false, seed); width = .3f; break;
                case InkBrush.Thread: path = InkCanvas.Line(a, b, half * .12f, 5, false, seed); width = .2f; break;
                case InkBrush.Silk: path = InkCanvas.Line(a, b, half * .22f, 4.5f, false, seed); width = .38f; break;
                case InkBrush.Flame: path = InkCanvas.Line(a, b, half * .16f, 12, false, seed); width = .36f; break;
                case InkBrush.Ink: path = InkCanvas.Line(a, b); width = .3f; break;
                default: path = InkCanvas.Line(a, b); width = .34f; break;
            }
            var stroke = Brush(width, fade, seed); stroke.Head = head; stroke.Tail = tail; stroke.Taper = style.Brush == InkBrush.Ink ? 2 : 1.2f;
            air.GlowRibbon(path, head, tail, Mathf.Min(half * 2, width * 2.4f), A(ink.Glow, Bloom * fade));
            air.Stroke(path, stroke);
            if (style.Brush == InkBrush.Bolt && !calm && !still)
            {
                var flicker = InkCanvas.Line(a, b, half * .4f, 0, true, g.Seed + 1 + Mathf.FloorToInt(t * 20));
                air.Stroke(flicker, new InkStroke { Width = .06f, Head = head, Tail = tail, Alpha = .6f * fade * strength, Ink = ink.Light, Body = ink.Light, InkOnly = true, Taper = 1 });
            }
            if (!sparse)
                for (int i = 0; i < 4; i++)
                {
                    float f = .18f + i * .24f; if (head < f) continue;
                    float age = Mathf.Clamp01((head - f) * 2 + p * .6f);
                    Vector2 at = a + (b - a) * f + normal * (((i % 2) * 2 - 1) * half * .6f * (still ? .5f : age));
                    Token(style.Token, at, .17f * (1 - .4f * age), i * 70 + p * 160, (1 - age) * fade);
                }
            float impact = Mathf.Clamp01((p - .22f) / .5f);
            if (impact > 0 && impact < 1) Token(style.Token, b, .34f * (still ? 1 : OutBack(Mathf.Min(1, impact * 3))), Seed(g), (1 - impact) * fade);
            if (style.Brush == InkBrush.Thread) { Token(style.Token, a, .2f, 0, fade * .9f); Token(style.Token, b, .24f, 30, fade); }
        }

        private void CrescentStrike(HeroIdentityGlyph g)
        {
            Phases(g, out bool pre, out float q, out float p, out float fade);
            Vector2 c = g.Position; float r = Mathf.Max(.3f, g.Radius); Vector2 d = g.Direction.sqrMagnitude > .01f ? g.Direction : Vector2.right;
            float heading = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg, sweep = g.Seed % 2 == 0 ? 1 : -1;
            if (pre)
            {
                ground.Sector(c, r, heading - CrescentHalf, heading + CrescentHalf, A(ink.Glow, .01f), A(ink.Body, (.05f + .11f * q) * strength));
                float reveal = still ? 1 : OutCubic(q);
                ground.Stroke(InkCanvas.Arc(c, r - .03f, r - .03f, heading - CrescentHalf * reveal, heading + CrescentHalf * reveal), new InkStroke { Width = .05f, Head = 1, Alpha = (.4f + .3f * q) * strength, Ink = ink.Ink, Body = ink.Body, NoLight = true, Taper = .5f });
                return;
            }
            ground.Sector(c, r, heading - CrescentHalf, heading + CrescentHalf, A(ink.Glow, .1f * fade * strength), A(ink.Body, .38f * fade * strength));
            ground.Stroke(InkCanvas.Arc(c, r - .03f, r - .03f, heading - CrescentHalf, heading + CrescentHalf), Brush(.06f, .75f * fade, 2));
            if (!sparse && r >= .8f) ground.Ticks(c, r - .06f, r >= 2 ? 9 : 5, Mathf.Min(.18f, r * .1f), .03f, 0, A(ink.Ink, .7f * fade * strength), heading - CrescentHalf, heading + CrescentHalf);
            float head = still ? 1 : OutCubic(Mathf.Min(1, p / .32f)), tail = still ? 0 : InQuad(Mathf.Max(0, (p - .25f) / .75f));
            // the swing passes through the middle of the area; the inked rim and wash mark its true edge
            float width = Mathf.Min(Mathf.Clamp(r * .26f, .16f, .48f), r * .3f), radius = r * .62f;
            var arc = InkCanvas.Arc(c, radius, radius, heading - sweep * CrescentHalf * .96f, heading + sweep * CrescentHalf * .96f);
            var main = Brush(width, fade, Seed(g)); main.Head = head; main.Tail = tail; main.Taper = 1.6f;
            air.GlowRibbon(arc, head, tail, width * 2f, A(ink.Glow, Bloom * fade));
            air.Stroke(arc, main);
            Vector2 tip = InkCanvas.Sample(arc, head);
            if (!sparse)
            {
                float h2 = still ? 1 : OutCubic(Mathf.Clamp01((p - .08f) / .32f));
                var inner = InkCanvas.Arc(c, radius * .55f, radius * .55f, heading - sweep * CrescentHalf * .8f, heading + sweep * CrescentHalf * .8f);
                var echo = Brush(width * .5f, .75f * fade, Seed(g) + 1); echo.Head = h2; echo.Tail = tail; echo.Taper = 1.6f; air.Stroke(inner, echo);
                for (int i = 0; i < 3; i++)
                {
                    float f = Mathf.Clamp01(head - i * .14f); if (f <= 0) continue;
                    var at = InkCanvas.Arc(c, radius, radius, heading - sweep * CrescentHalf * .96f, heading + sweep * CrescentHalf * .96f);
                    Vector2 pos = InkCanvas.Sample(at, f); Vector2 outward = (pos - c).normalized;
                    Token(style.Token, pos - outward * (.1f + i * .06f), .18f * (1 - i * .2f), heading + i * 40 + p * 90, fade * (1 - i * .25f));
                }
            }
            if (head < 1 && !calm) Glow(tip, .7f, ink.Glow, .35f * fade, true);
        }

        private void RingStrike(HeroIdentityGlyph g)
        {
            Phases(g, out bool pre, out float q, out float p, out float fade);
            Vector2 c = g.Position; float r = Mathf.Max(.3f, g.Radius), half = Mathf.Max(.1f, g.Width), s = Seed(g);
            if (pre)
            {
                ground.Annulus(c, Mathf.Max(0, r - half), r + half, A(ink.Body, (.04f + .08f * q) * strength), A(ink.Body, .02f * strength));
                float reveal = still ? 1 : OutCubic(q);
                ground.Stroke(InkCanvas.Arc(c, r, r, s, s + 360 * reveal), new InkStroke { Width = .05f, Head = 1, Alpha = (.4f + .3f * q) * strength, Ink = ink.Ink, NoLight = true, InkOnly = true, Taper = .5f }, reveal >= 1);
                return;
            }
            ground.Annulus(c, Mathf.Max(0, r - half), r + half, A(ink.Body, .42f * fade * strength), A(ink.Glow, .16f * fade * strength));
            float head = still ? 1 : OutCubic(Mathf.Min(1, p / .4f)), tail = still ? 0 : InQuad(Mathf.Max(0, (p - .35f) / .65f));
            var ring = InkCanvas.Arc(c, r, r, s, s + 360);
            var main = Brush(half * 1.2f, fade, s); main.Head = head; main.Tail = tail; main.Taper = .8f;
            air.GlowRibbon(ring, head, tail, half * 2, A(ink.Glow, Bloom * fade));
            air.Stroke(ring, main, true);
            if (!sparse)
                for (int i = 0; i < 4; i++)
                {
                    float f = Mathf.Repeat(head - i * .08f, 1); if (head < i * .08f) continue;
                    Vector2 pos = c + Dir(s + 360 * f) * r;
                    Token(style.Token, pos, .2f * (1 - i * .15f), s + f * 360, fade * (1 - i * .2f));
                }
        }

        private void Link(HeroIdentityGlyph g)
        {
            float p = Mathf.Clamp01((t - g.Created) / Mathf.Max(.05f, g.Until - g.Created));
            Vector2 a = g.Position + Vector2.up * .35f, b = g.End + Vector2.up * .35f; if ((b - a).sqrMagnitude < .01f) return;
            var path = InkCanvas.Lob(a, b, Mathf.Min(.5f, Vector2.Distance(a, b) * .12f), 16);
            float head = still ? 1 : OutCubic(Mathf.Min(1, p / .45f)), tail = still ? 0 : InQuad(Mathf.Max(0, (p - .4f) / .6f));
            var s = Brush(style.Brush == InkBrush.Silk ? .1f : .06f, .85f * (1 - Smooth(.7f, 1, p)), Seed(g)); s.Head = head; s.Tail = tail; s.Taper = 2;
            air.Stroke(path, s);
            Token(InkToken.Glint, InkCanvas.Sample(InkCanvas.Lob(a, b, Mathf.Min(.5f, Vector2.Distance(a, b) * .12f), 16), head), .26f, still ? 0 : p * 180, 1 - Smooth(.75f, 1, p), true, 0, GlintPalette(ink));
        }
        /// <summary>The painted body of an identity bolt in flight (envelope, note, shard...); null for weapon-shaped echoes.</summary>
        public static Sprite Missile(ProjectileState state, Sprite generic, float time, out float turn, out float size)
        {
            turn = 0; size = 0;
            if (state == null || state.Faction == Faction.Enemy || state.Sprite != generic || state.Cause.ItemId == null || !state.Cause.ItemId.StartsWith("hero-identity-", System.StringComparison.Ordinal)) return null;
            var style = HeroMechanicStyle.For(state.Cause.ItemId.Substring(14)); InkToken kind = style.Missile;
            bool spin = kind == InkToken.Die || kind == InkToken.Coin || kind == InkToken.Seal || kind == InkToken.Star || kind == InkToken.Spark || kind == InkToken.Button || kind == InkToken.Citrus;
            turn = spin ? (PresentationSettings.ReducedMotion ? 0 : time * 520 + state.Id * 47) : kind == InkToken.Drop || kind == InkToken.Ember ? 90 : -90;
            size = (kind == InkToken.Envelope || kind == InkToken.Page ? .4f : .36f) * (PresentationSettings.HighReadability ? 1.1f : 1);
            return MechanicInk.Token(kind, style.Palette, kind == InkToken.Die ? 1 + state.Id % 6 : 0);
        }
        private static InkPalette GlintPalette(InkPalette p) => new InkPalette(p.Ink, p.Light, Color.white, p.Glow, p.Accent);

        private void Lob(HeroIdentityGlyph g)
        {
            float p = Mathf.Clamp01((t - g.Created) / Mathf.Max(.05f, g.Until - g.Created));
            Vector2 a = g.Position + Vector2.up * .3f, b = g.End; float dist = Vector2.Distance(a, b), height = Mathf.Clamp(dist * .35f + .6f, .7f, 2f);
            var path = InkCanvas.Lob(a, b, height, 20);
            if (!sparse)
            {
                var trail = new InkStroke { Width = .07f, Head = p, Tail = Mathf.Max(0, p - .3f), Alpha = .5f * strength, Ink = ink.Light, Body = ink.Light, InkOnly = true, Taper = 2 };
                air.Stroke(path, trail);
            }
            Vector2 shell = InkCanvas.Sample(InkCanvas.Lob(a, b, height, 20), p);
            Vector2 shadow = Vector2.Lerp(a - Vector2.up * .3f, b, p);
            Glow(shadow, .45f * (.6f + .4f * p), ink.Ink, .4f);
            float squash = still ? 1 : 1 + .12f * Mathf.Sin(p * Mathf.PI * 3);
            Token(style.Token, shell, .36f, still ? 0 : p * 420, 1, true, 0, null, squash);
        }

        private void Catch(HeroIdentityGlyph g)
        {
            float p = Mathf.Clamp01((t - g.Created) / Mathf.Max(.05f, g.Until - g.Created)); Vector2 c = g.Position; float r = Mathf.Max(.2f, g.Radius);
            float close = still ? .6f : 1.25f - .8f * OutCubic(p);
            air.Stroke(InkCanvas.Arc(c, r * close, r * close, 0, 360), Brush(.07f, 1 - p, 1), true);
            Token(InkToken.Glint, c, .34f * (1 - p) + .1f, p * 90, 1 - InQuad(p), true, 0, GlintPalette(ink));
            if ((g.End - g.Position).sqrMagnitude > .05f)
            {
                Vector2 to = g.End + Vector2.up * .45f; Vector2 at = Vector2.Lerp(c, to, still ? 1 : OutCubic(p));
                Token(style.Token, at, .24f, still ? 0 : p * 300, 1 - Smooth(.8f, 1, p));
            }
        }

        private void Poise(HeroIdentityGlyph g)
        {
            float p = Mathf.Clamp01((t - g.Created) / Mathf.Max(.05f, g.Until - g.Created)); Vector2 c = sim.Champion.Position + Vector2.up * .35f;
            float head = still ? 1 : OutCubic(p), from = 200;
            var s = Brush(.07f, 1 - Smooth(.6f, 1, p), 2); s.Head = 1; s.Taper = 2;
            air.Stroke(InkCanvas.Arc(c, .58f, .36f, from, from + 300 * head), s);
            Token(style.Token, c + Vector2.up * (.6f + (still ? 0 : .3f * OutCubic(p))), .22f, 0, 1 - InQuad(p));
        }

        private void Emblem(HeroIdentityGlyph g)
        {
            float p = Mathf.Clamp01((t - g.Created) / Mathf.Max(.05f, g.Until - g.Created));
            float size = Mathf.Clamp(g.Radius * 1.4f, .3f, 1.1f) * (still ? 1 : OutBack(Mathf.Min(1, p / .3f)));
            if (!calm) Glow(g.Position, size * 2.2f, ink.Glow, .3f * (1 - p), true);
            Token(style.Token, g.Position + Vector2.up * (.3f + (still ? 0 : .35f * p)), size, still ? 0 : Mathf.Sin(p * 6) * 12, 1 - Smooth(.6f, 1, p), true, style.Token == InkToken.Die ? 6 : 0);
        }
    }
}
