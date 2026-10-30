using System;
using System.Collections.Generic;
using UnityEngine;
using static ArcaneBrigade.FxKit;

namespace ArcaneBrigade
{
    /// <summary>Feature 4 (owner 29/10/2026): the floor answers step-8 weather for as long as it lasts: puddles spread under rain, frost
    /// creeps under a blizzard, spores bed down under plague, scorch and ash under a feast of fire... Drawn by a floor-level effect
    /// director (under every unit and under enemy danger zones, so nothing is ever hidden), fading in over seconds and out after.
    /// Feature 8: big effects (step 6/8 moments) throw their element's light on the floor like a lamp, and tint the enemies in
    /// reach (LampTint). Both register here so the frame preview can render them (rules "floor-..." and "lamp-...").</summary>
    public static class GroundFx
    {
        public enum Floor { Rain, Thunder, Frost, Spores, Stars, Shadow, Sheen, Crush, Gale, Scorch, Crystal }
        public const int LightOrder = 60, DustOrder = 59; // over the floor art, under the hostile field light (62) and danger ink (66)

        private static readonly Dictionary<string, Floor> ByRule = new Dictionary<string, Floor>
        {
            { "tide-rain", Floor.Rain }, { "storm-thunderstorm", Floor.Thunder }, { "frost-blizzard", Floor.Frost }, { "plague-mist", Floor.Spores },
            { "starry-night", Floor.Stars }, { "eclipse", Floor.Shadow }, { "lighthouse", Floor.Sheen }, { "gravity-heavy", Floor.Crush },
            { "wind-gale", Floor.Gale }, { "hearth-feast", Floor.Scorch }, { "bulwark-fortress", Floor.Crystal },
        };

        public static Floor ForLook(ElementLook look) => look switch
        {
            ElementLook.Water => Floor.Rain, ElementLook.Storm => Floor.Thunder, ElementLook.Ice => Floor.Frost, ElementLook.Poison => Floor.Spores,
            ElementLook.Light or ElementLook.Gold => Floor.Stars, ElementLook.Void or ElementLook.Dream => Floor.Shadow, ElementLook.Wind => Floor.Gale,
            ElementLook.Earth or ElementLook.Time => Floor.Crush, ElementLook.Crystal => Floor.Crystal, ElementLook.Bloom => Floor.Spores, _ => Floor.Scorch,
        };

        /// <summary>The floor effect for a weather rule (its own look when known, else its element's).</summary>
        public static string FloorKey(string weatherRule)
        {
            if (weatherRule != null && ByRule.ContainsKey(weatherRule)) return "floor-" + weatherRule;
            Floor f = ForLook(RulesV3Ink.LookFor(weatherRule));
            foreach (KeyValuePair<string, Floor> pair in ByRule) if (pair.Value == f) return "floor-" + pair.Key;
            return null;
        }
        public static string LampKey(ElementLook look) => "lamp-" + look.ToString().ToLowerInvariant();

        public static void Register(Dictionary<string, Func<FxEffect>> map)
        {
            foreach (KeyValuePair<string, Floor> pair in ByRule) { Floor f = pair.Value; map["floor-" + pair.Key] = () => new FloorWeatherEffect(f); }
            foreach (ElementLook look in Enum.GetValues(typeof(ElementLook)))
                if (look != ElementLook.None) { ElementLook l = look; map[LampKey(l)] = () => new LampEffect(l); }
        }
    }

    /// <summary>What the floor does under one weather: about fifteen patches spread over the arena (a jittered grid, so they never
    /// clump), each spreading from nothing over a few seconds and living with its own small motion.</summary>
    public sealed class FloorWeatherEffect : FxEffect
    {
        private readonly GroundFx.Floor kind;
        public FloorWeatherEffect(GroundFx.Floor kind) { this.kind = kind; }
        protected override float Life => 1;

        public override void Draw(FxBatch b, float now, float pre, float t)
        {
            float age = now - Born, on = Rise(age, 0, 3f) * Fade(now, End - 1.8f, End);
            if (on <= .01f) return;
            Rect a = Arena; const int cols = 5, rows = 3;
            float cw = (a.width - 2f) / cols, ch = (a.height - 1.6f) / rows;
            for (int i = 0; i < cols * rows; i++)
            {
                int s = Seed * 31 + i * 17;
                Vector2 p = new Vector2(a.xMin + 1f + cw * (i % cols + .15f + .7f * H(s, 1)), a.yMin + .8f + ch * (i / cols + .15f + .7f * H(s, 2)));
                float grow = EaseOut(Rise(age, H(s, 3) * 2.2f, H(s, 3) * 2.2f + 3f)); if (grow <= 0) continue;
                float r = (1.15f + .9f * H(s, 4)) * (.4f + .6f * grow);
                Patch(b, p, r, on, age, now, s);
            }
        }

        private void Patch(FxBatch b, Vector2 p, float r, float k, float age, float now, int s)
        {
            switch (kind)
            {
                case GroundFx.Floor.Rain: case GroundFx.Floor.Thunder:
                {
                    bool storm = kind == GroundFx.Floor.Thunder;
                    // a dark puddle, a sky sheen along its top edge, rain rippling in it; under a thunderstorm the puddles
                    // throw back the sheet lightning (same 3.1 s beat as the storm's sky)
                    float cyc = now / 3.1f, f = (cyc - Mathf.Floor(cyc)) * 3.1f;
                    float flash = storm ? Mathf.Max(Fade(f, 0, .12f), .7f * Fade(f - .2f, 0, .1f)) : 0;
                    b.Floor(FxShape.Shade, p, r, A(storm ? new Color(.02f, .03f, .09f) : new Color(.03f, .08f, .18f), .72f * k), 0, s);
                    b.Quad(FxShape.Glow, p + new Vector2(.1f * r, -.12f * r), new Vector2(r * .45f, r * .07f), -6, A(White, (.1f + .4f * flash) * k), 0, s);
                    b.Quad(FxShape.Glow, p + new Vector2(-.15f * r, .18f * r), new Vector2(r * .8f, r * .2f), -6, A(storm ? new Color(.7f, .85f, 1f) : new Color(.35f, .7f, 1f), (.26f + .6f * flash) * k), 0, s);
                    for (int d = 0; d < 3; d++)
                    {
                        float period = .5f + .3f * H(s, d + 10), u = now / period + H(s, d + 20); int n = Mathf.FloorToInt(u); u -= n;
                        Vector2 q = p + new Vector2((H(s + n, d + 30) - .5f) * r * 1.1f, (H(s + n, d + 40) - .5f) * r * .55f);
                        b.Quad(FxShape.Shock, q, new Vector2(.12f + .18f * u, (.12f + .18f * u) * .55f), 0, A(new Color(.6f, .85f, 1f), .7f * k * (1 - u)), u, s + d);
                        if (u < .2f) Glint(b, q + Vector2.up * .05f, .07f, White, .8f * k * (1 - u * 5), 0);
                    }
                    if (flash > .05f) for (int d = 0; d < 2; d++) Glint(b, p + new Vector2((H(s, d + 50) - .5f) * r, (H(s, d + 60) - .5f) * r * .4f), .18f, Cyan, flash * k, now * 90);
                    break;
                }
                case GroundFx.Floor.Frost:
                    // frost: a pale crust of crystal veins creeping out, a drift of snow on it, glints winking
                    b.Floor(FxShape.Dust, p + Vector2.left * .1f * Mathf.Sin(now * .4f + s), r * 1.05f, A(new Color(.82f, .93f, 1f), .55f * k), now * .03f, s);
                    for (int d = 0; d < 3; d++) b.Floor(FxShape.Dust, p + new Vector2((H(s, d + 40) - .5f) * r * 2.2f, (H(s, d + 44) - .5f) * r), r * .35f, A(new Color(.85f, .95f, 1f), .45f * k), now * .02f, s + d);
                    b.Floor(FxShape.Cracks, p, r * .8f, A(new Color(.55f, .85f, 1f), .22f * k), Mathf.Clamp01(age * .4f), s, 1.5f);
                    for (int d = 0; d < 3; d++) { float tw = Mathf.Max(0, Mathf.Sin(now * (1.6f + H(s, d)) + d * 2.1f)); Glint(b, p + new Vector2((H(s, d + 5) - .5f) * r * 1.2f, (H(s, d + 9) - .5f) * r * .6f), .07f + .12f * tw, White, .9f * tw * k, 45); }
                    break;
                case GroundFx.Floor.Spores:
                    // a bed of mould, spores puffing up from it and popping
                    b.Floor(FxShape.Dust, p, r, A(new Color(.22f, .32f, .1f), .55f * k), now * .02f, s);
                    b.Floor(FxShape.Glow, p, r * .7f, A(new Color(.55f, .9f, .25f), .12f * k * (.8f + .2f * Mathf.Sin(now * 1.3f + s))), 0, s);
                    for (int d = 0; d < 4; d++)
                    {
                        float u = Mathf.Repeat(now * (.35f + .2f * H(s, d)) + H(s, d + 70), 1);
                        Vector2 q = p + new Vector2((H(s, d + 80) - .5f) * r * 1.2f + Mathf.Sin(u * 5 + d) * .1f, (H(s, d + 90) - .5f) * r * .5f + u * .9f);
                        b.Quad(FxShape.Glow, q, .07f + .05f * u, A(new Color(.7f, 1f, .35f), .8f * k * Mathf.Sin(u * Mathf.PI)), 0, s);
                        if (u > .85f) b.Quad(FxShape.Shock, q, .14f, A(new Color(.7f, 1f, .35f), .6f * k), (u - .85f) / .15f, s + d);
                    }
                    break;
                case GroundFx.Floor.Stars:
                    // a still pool of night that reflects stars: deep blue, specks twinkling and slowly turning
                    b.Floor(FxShape.Shade, p, r, A(new Color(.03f, .04f, .16f), .62f * k), 0, s);
                    for (int d = 0; d < 8; d++)
                    {
                        float tw = .5f + .5f * Mathf.Sin(now * (1 + 2 * H(s, d)) + d * 1.3f), ang = now * .08f + d * 1.26f;
                        Vector2 q = p + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang) * .55f) * r * (.25f + .5f * H(s, d + 7));
                        Glint(b, q, .06f + .1f * tw * H(s, d + 3), d == 0 ? new Color(1f, .9f, .6f) : new Color(.75f, .85f, 1f), (.4f + .6f * tw) * k, ang * 30);
                    }
                    break;
                case GroundFx.Floor.Shadow:
                    // a shadow pool creeping wider and back, its rim catching a violet glow, dark motes sinking into it
                    {
                        float breathe = 1 + .08f * Mathf.Sin(now * .7f + s);
                        b.Floor(FxShape.Shade, p, r * 1.1f * breathe, A(new Color(.04f, 0, .08f), .6f * k), 0, s);
                        b.Floor(FxShape.Glow, p, r * 1.15f * breathe, A(new Color(.5f, .25f, .9f), .1f * k), 0, s);
                        for (int d = 0; d < 3; d++) { float u = Mathf.Repeat(now * .3f + H(s, d), 1); Vector2 q = p + new Vector2((H(s, d + 5) - .5f) * r, .7f - u * .8f); b.Quad(FxShape.Shade, q, .1f, A(new Color(.08f, 0, .14f), .7f * k * Mathf.Sin(u * Mathf.PI)), 0, s); }
                    }
                    break;
                case GroundFx.Floor.Sheen:
                    // wet stones by the lighthouse: warm caustic light sliding over the floor, salt glinting
                    for (int d = 0; d < 2; d++)
                    {
                        Vector2 q = p + new Vector2(Mathf.Sin(now * .5f + s + d * 2) * .3f * r, Mathf.Cos(now * .4f + d) * .15f * r);
                        b.Quad(FxShape.Glow, q, new Vector2(r * (.75f - d * .2f), r * (.3f - d * .06f)), 8 * Mathf.Sin(now * .3f + d), A(new Color(1f, .82f, .45f), (.24f + .1f * Mathf.Sin(now * 1.7f + s + d)) * k), 0, s);
                    }
                    for (int d = 0; d < 3; d++) { float tw = Mathf.Max(0, Mathf.Sin(now * 2.3f + H(s, d) * 6)); Glint(b, p + new Vector2((H(s, d + 4) - .5f) * r * 1.3f, (H(s, d + 8) - .5f) * r * .5f), .05f + .09f * tw, new Color(1, .95f, .8f), tw * k, 0); }
                    break;
                case GroundFx.Floor.Crush:
                    // the floor pressed down: a crazed dent with violet in the cracks, dust held flat, pebbles trembling
                    b.Floor(FxShape.Shade, p, r * .9f, A(new Color(.06f, .03f, .1f), .45f * k), 0, s);
                    b.Floor(FxShape.Cracks, p, r, A(new Color(.6f, .35f, 1f), .28f * k * (.85f + .15f * Mathf.Sin(now * 3 + s))), 1, s, 2.5f);
                    b.Floor(FxShape.Dust, p + Vector2.down * .1f, r * 1.1f, A(new Color(.35f, .3f, .4f), .3f * k), now * .01f, s);
                    for (int d = 0; d < 4; d++) { float j = Mathf.Abs(Mathf.Sin(now * 9 + d * 2.3f + s)) * .03f; b.Quad(FxShape.Disc, p + new Vector2((H(s, d + 3) - .5f) * r * 1.1f, (H(s, d + 6) - .5f) * r * .5f + j), new Vector2(.06f, .045f), 0, A(new Color(.5f, .45f, .55f), .8f * k), 0, s); }
                    break;
                case GroundFx.Floor.Gale:
                    // dust streaming along the floor in long low streaks, leaves skittering through
                    for (int d = 0; d < 3; d++)
                    {
                        float u = Mathf.Repeat(now * (.35f + .15f * H(s, d)) + H(s, d + 11), 1);
                        Vector2 q = p + new Vector2((u - .5f) * r * 2.4f, (H(s, d + 13) - .5f) * r * .6f);
                        b.Quad(FxShape.Dust, q, new Vector2(r * .9f, r * .16f), -4, A(new Color(.6f, .55f, .5f), .5f * k * Mathf.Sin(u * Mathf.PI)), now * .4f, s + d);
                    }
                    for (int d = 0; d < 3; d++)
                    {
                        float u = Mathf.Repeat(now * (.6f + .3f * H(s, d + 20)) + H(s, d + 21), 1), hop = Mathf.Abs(Mathf.Sin(u * 9 + d)) * .12f;
                        Vector2 q = p + new Vector2((u - .5f) * r * 2.6f, (H(s, d + 22) - .5f) * r * .5f + hop);
                        Color leaf = H(s, d + 23) > .5f ? new Color(.85f, .6f, .25f) : new Color(.55f, .75f, .3f);
                        b.Quad(FxShape.Disc, q, new Vector2(.11f, .045f), now * 400 * (.6f + H(s, d)) + d * 60, A(leaf, .85f * k * Mathf.Sin(u * Mathf.PI)), 0, s);
                    }
                    break;
                case GroundFx.Floor.Scorch:
                    // scorched boards: a charred patch, embers breathing in it, ash flakes lifting and drifting off
                    b.Floor(FxShape.Shade, p, r, A(new Color(.05f, .025f, .01f), .8f * k), 0, s);
                    b.Floor(FxShape.Dust, p, r * .85f, A(new Color(.32f, .29f, .27f), .45f * k), now * .02f, s);
                    for (int d = 0; d < 4; d++)
                    {
                        float br = .55f + .45f * Mathf.Sin(now * (1.5f + H(s, d)) + d * 1.7f);
                        b.Floor(FxShape.Glow, p + new Vector2((H(s, d + 4) - .5f) * r * 1.1f, (H(s, d + 8) - .5f) * r * .5f), .14f + .06f * br, A(new Color(1f, .45f, .12f), .7f * br * k), 0, s);
                    }
                    for (int d = 0; d < 3; d++)
                    {
                        float u = Mathf.Repeat(now * .4f + H(s, d + 30), 1);
                        Vector2 q = p + new Vector2((H(s, d + 31) - .5f) * r + u * .5f + Mathf.Sin(u * 7 + d) * .12f, u * 1.1f);
                        b.Quad(FxShape.Disc, q, new Vector2(.05f, .035f), u * 300 + d * 50, A(new Color(.65f, .62f, .6f), .7f * k * Mathf.Sin(u * Mathf.PI)), 0, s);
                    }
                    break;
                default: // Crystal: a glassy rime with sharp facets glinting in turn
                    b.Floor(FxShape.Shade, p, r * .9f, A(new Color(.02f, .1f, .12f), .35f * k), 0, s);
                    b.Floor(FxShape.Dust, p, r * .9f, A(new Color(.5f, .8f, .85f), .3f * k), now * .02f, s);
                    b.Floor(FxShape.Cracks, p, r * .75f, A(new Color(.4f, 1f, .9f), .2f * k), 1, s, .8f);
                    for (int d = 0; d < 5; d++) { float u = Mathf.Repeat(now * .5f + d / 5f + H(s, d), 1), tw = Fade(u, 0, .3f); Glint(b, p + new Vector2((H(s, d + 3) - .5f) * r, (H(s, d + 6) - .5f) * r * .5f), .1f + .14f * tw, new Color(.85f, 1f, 1f), tw * k, 45); }
                    break;
            }
        }
    }

    /// <summary>A big effect's light on the floor: it switches on as the moment lands, flickers in its element's way and dies down
    /// in under a second. Pure function of time; the enemies in reach are tinted by <see cref="LampTint"/> with the same curve.</summary>
    public sealed class LampEffect : FxEffect
    {
        public const float Span = .95f;
        private readonly ElementLook look;
        public LampEffect(ElementLook look) { this.look = look; }
        protected override float Life => Span;

        /// <summary>Brightness 0..1 at <paramref name="t"/> seconds after landing.</summary>
        public static float Intensity(ElementLook look, float t, int seed)
        {
            if (t < 0 || t > Span) return 0;
            float d = 1 - Rise(t, .05f, Span), k = Rise(t, 0, .05f) * d * Mathf.Sqrt(d);
            if (look == ElementLook.Storm) k *= H(seed, Mathf.FloorToInt(t * 24)) > .35f ? 1 : .35f;
            else if (look == ElementLook.Fire) k *= .82f + .18f * Mathf.PerlinNoise(t * 14, seed * .1f);
            return k;
        }
        /// <summary>How far the light reaches (floor units), bounded so a lamp never lights most of the field.</summary>
        public static float Reach(float radius) => Mathf.Clamp(radius * 1.5f, 1.6f, 3.2f);

        public override void Draw(FxBatch b, float now, float pre, float t)
        {
            float k = Intensity(look, t, Seed); if (k <= .01f) return;
            Color c = Color.Lerp(RulesV3Ink.LookColour(look), White, .15f), hot = Color.Lerp(c, look == ElementLook.Fire ? new Color(1, .8f, .35f) : White, .5f);
            if (Moment.Kind == BondMomentKind.Line)
            {
                Vector2 d = Moment.To - Moment.From; int n = Mathf.Clamp(Mathf.RoundToInt(d.magnitude / 2.2f), 2, 5); float r = Mathf.Clamp(Moment.Radius * 1.4f, 1.2f, 2.2f);
                for (int i = 0; i < n; i++) { Vector2 p = Moment.From + d * ((i + .5f) / n); b.Floor(FxShape.Glow, p, r * 1.3f, A(c, .7f * k), 0, Seed + i); b.Floor(FxShape.Glow, p, r * .6f, A(hot, .5f * k), 0, Seed + i); }
                return;
            }
            float R = Reach(Moment.Radius);
            b.Floor(FxShape.Glow, At, R * 1.35f, A(c, .85f * k), 0, Seed);
            b.Floor(FxShape.Glow, At, R * .7f, A(hot, .6f * k), 0, Seed);
            b.Floor(FxShape.Glow, At, R * .22f, A(White, .35f * k), 0, Seed);
        }
    }

    /// <summary>The second half of a lamp: enemies within its reach take its colour for the moment it shines. Runs after every
    /// actor has set its own tint this frame, and puts back any colour it changed if the actor did not repaint it.</summary>
    [DefaultExecutionOrder(1100)]
    public sealed class LampTint : MonoBehaviour
    {
        public struct Lamp { public Vector2 From, To; public bool Line; public float Land, Reach; public ElementLook Look; public Color Colour; public int Seed; }
        private readonly List<Lamp> lamps = new List<Lamp>(8);
        private readonly Dictionary<SpriteRenderer, (Color was, Color set)> touched = new Dictionary<SpriteRenderer, (Color, Color)>();
        private CombatSimulation sim; private float now;
        public int Tinted { get; private set; }
        public int Live => lamps.Count;

        /// <summary>A lamp; false when one of the same element is already shining there (merged, so rapid strikes stay one light).</summary>
        public bool Add(Lamp lamp)
        {
            for (int i = lamps.Count - 1; i >= 0; i--)
            {
                Lamp o = lamps[i];
                if (o.Look == lamp.Look && !o.Line && !lamp.Line && Mathf.Abs(o.Land - lamp.Land) < .2f && (o.To - lamp.To).sqrMagnitude < 2.2f) return false;
            }
            if (lamps.Count >= 6) lamps.RemoveAt(0);
            lamps.Add(lamp); return true;
        }

        public void Frame(CombatSimulation simulation, float time) { sim = simulation; now = time; }
        public void Clear() { lamps.Clear(); Restore(); }

        private void Restore()
        {
            foreach (KeyValuePair<SpriteRenderer, (Color was, Color set)> p in touched) if (p.Key != null && p.Key.color == p.Value.set) p.Key.color = p.Value.was;
            touched.Clear();
        }

        private void LateUpdate()
        {
            Restore(); Tinted = 0;
            for (int i = lamps.Count - 1; i >= 0; i--) if (now - lamps[i].Land > LampEffect.Span || now < lamps[i].Land - 5) lamps.RemoveAt(i);
            if (lamps.Count == 0 || sim == null || PresentationSettings.ReducedFlashes) return;
            IReadOnlyList<UnitState> units = sim.Units; int cap = FxBudget.LampTints;
            for (int u = 0; u < units.Count && Tinted < cap; u++)
            {
                UnitState unit = units[u];
                if (unit == null || !unit.Alive || unit.Faction != Faction.Enemy) continue;
                float best = 0; Color colour = default;
                foreach (Lamp l in lamps)
                {
                    float k = LampEffect.Intensity(l.Look, now - l.Land, l.Seed); if (k <= .01f) continue;
                    Vector2 d = unit.Position - (l.Line ? Closest(l.From, l.To, unit.Position) : l.To); d.y /= .62f;
                    float reach = l.Line ? l.Reach * .8f : l.Reach, fall = 1 - Mathf.Clamp01(d.magnitude / reach); fall *= fall * (3 - 2 * fall);
                    if (k * fall > best) { best = k * fall; colour = l.Colour; }
                }
                if (best <= .02f) continue;
                UnitVisual v = UnitVisual.Live(unit.Id); SpriteRenderer r = v != null ? v.BodyRenderer : null;
                if (r == null || touched.ContainsKey(r)) continue;
                Color was = r.color, set = Color.Lerp(was, Lit(was, colour), .5f * best); set.a = was.a;
                r.color = set; touched[r] = (was, set); Tinted++;
            }
        }

        /// <summary>A sprite colour bathed in a light: the light's hue at full strength over the sprite's own (vertex colours cannot
        /// go over white, so the floor glow under it carries the brightness).</summary>
        public static Color Lit(Color was, Color light)
        {
            float m = Mathf.Max(.001f, Mathf.Max(light.r, Mathf.Max(light.g, light.b)));
            return new Color(Mathf.Lerp(was.r, light.r / m, .8f), Mathf.Lerp(was.g, light.g / m, .8f), Mathf.Lerp(was.b, light.b / m, .8f), was.a);
        }

        private static Vector2 Closest(Vector2 a, Vector2 b, Vector2 p)
        {
            Vector2 d = b - a; float l = d.sqrMagnitude; if (l < .0001f) return a;
            return a + d * Mathf.Clamp01(Vector2.Dot(p - a, d) / l);
        }
    }
}
