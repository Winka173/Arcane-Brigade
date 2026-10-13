using System;
using System.Collections.Generic;
using UnityEngine;
using static ArcaneBrigade.FxKit;

namespace ArcaneBrigade
{
    /// <summary>Feature 8 (owner 29/10/2026): when a wave or the boss is won, the bond element that dealt the most bond damage that
    /// wave plays a short celebration of its own over the field (2.6 s, its own effect director, so it carries on into the shop on
    /// the same floor). It tallies bond damage requests itself and reads the phase; it never changes the run.</summary>
    public sealed class VictoryFinale : MonoBehaviour
    {
        private ArenaController owner;
        private FxDirector fx;
        private CombatSimulation sim;
        private BondRuntime bonds;
        private RunPhase phase;
        private readonly Dictionary<ElementLook, float> tally = new Dictionary<ElementLook, float>();
        public ElementLook LastWinner { get; private set; }

        public void Configure(ArenaController controller) { owner = controller; fx = new FxDirector(transform); }

        private void OnBondDamage(BondDamageRequest d)
        {
            if (d.Target == null || d.Rule == null) return;
            ElementLook look = RulesV3Ink.LookFor(d.Rule);
            float dealt = Mathf.Min(d.Damage, Mathf.Max(0, d.Target.Health)); // an execute counts what it took, not its overkill
            tally[look] = (tally.TryGetValue(look, out float v) ? v : 0) + dealt;
        }

        private void LateUpdate()
        {
            if (fx == null) return;
            CombatSimulation now = owner != null ? owner.Simulation : null;
            if (now != sim) { sim = now; phase = sim != null ? sim.Phase : RunPhase.Preparing; }
            if (sim != null && sim.Bonds != bonds)
            {
                if (bonds != null) bonds.DamageRequested -= OnBondDamage;
                bonds = sim.Bonds; if (bonds != null) bonds.DamageRequested += OnBondDamage;
            }
            if (sim != null)
            {
                RunPhase p = sim.Phase;
                if (p == RunPhase.Combat && phase != RunPhase.Combat && phase != RunPhase.Paused) tally.Clear(); // a new wave
                if (p == RunPhase.Victory && phase != RunPhase.Victory) Play();
                phase = p;
            }
            fx.Draw(Time.unscaledTime);
        }

        private void Play()
        {
            ElementLook best = ElementLook.None; float most = 0;
            foreach (var pair in tally) if (pair.Value > most) { most = pair.Value; best = pair.Key; }
            tally.Clear(); LastWinner = best;
            if (best == ElementLook.None || most <= 0 || PresentationSettings.ReducedEffects) return;
            Camera cam = Camera.main;
            Vector2 centre = cam != null ? (Vector2)cam.transform.position : sim.Champion != null ? sim.Champion.Position : Vector2.zero;
            if (sim.BossDefeated && sim.Boss != null) centre = Vector2.Lerp(centre, sim.Boss.Position, .4f);
            fx.Spawn(new BondMomentRequest(null, FinaleFx.Key(best), 8, BondMomentKind.Strike, centre, centre, 2, 0, 0), Time.unscaledTime);
        }

        private void OnDestroy() { if (bonds != null) bonds.DamageRequested -= OnBondDamage; }
    }

    /// <summary>Registers one victory finale per element with the effect director.</summary>
    public static class FinaleFx
    {
        public static string Key(ElementLook look) => "finale-" + look.ToString().ToLowerInvariant();
        public static void Register(Dictionary<string, Func<FxEffect>> map)
        {
            foreach (ElementLook look in Enum.GetValues(typeof(ElementLook)))
                if (look != ElementLook.None) { ElementLook l = look; map[Key(l)] = () => new FinaleEffect(l); }
        }
    }

    /// <summary>One element's celebration over the field: many small pieces spread over a 13 x 6 band around the centre (the HUD stays
    /// clear at the screen edges), in from .3 s, out by 2.6 s. Nothing radiates from a friend; nothing is a ring.</summary>
    public sealed class FinaleEffect : FxEffect
    {
        private readonly ElementLook look;
        public FinaleEffect(ElementLook look) { this.look = look; }
        protected override float Life => 2.6f;
        private const float W = 6.5f, Hh = 3f;

        public override void Draw(FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return;
            float e = Rise(t, 0, .3f) * Fade(t, 2f, 2.6f); if (e <= .005f) return;
            Vector2 c = At; int s = Seed;
            switch (look)
            {
                case ElementLook.Fire: // sky lanterns: flame-hearted lights rise from the floor in a staggered drift, trailing sparks
                    for (int i = 0; i < 14; i++)
                    {
                        float u = Mathf.Clamp01((t - H(s, i) * .7f) / 1.8f); if (u <= 0) continue;
                        float x = c.x + (i / 13f - .5f) * 2 * W * .9f + Mathf.Sin(t * 2 + i) * .3f, y = c.y - Hh + EaseOut(u) * Hh * 2.1f;
                        float a = e * Mathf.Sin(Mathf.Min(1, u * 1.4f) * Mathf.PI * .5f) * Fade(u, .75f, 1);
                        b.Quad(FxShape.Glow, new Vector2(x, y), 1f, A(new Color(1, .45f, .1f), .75f * a)); b.Quad(FxShape.Glow, new Vector2(x, y), .35f, A(new Color(1, .9f, .6f), .8f * a));
                        b.Quad(FxShape.Flame, new Vector2(x, y + .05f), new Vector2(.32f, .55f), 0, A(new Color(1, .75f, .3f), a), t * 2, s + i, 1);
                        for (int k = 1; k <= 3; k++) Glint(b, new Vector2(x + Mathf.Sin(t * 7 + k + i) * .15f, y - .45f - k * .3f), .14f, new Color(1, .7f, .3f), a * (1 - k * .28f), t * 300);
                    }
                    break;
                case ElementLook.Ice: // slow snow of crystal glints across the field, frost breath along the floor
                    for (int i = 0; i < 40; i++)
                    {
                        float u = Mathf.Repeat(t * (.28f + .14f * H(s, i)) + H(s, i + 1), 1);
                        Vector2 p = new Vector2(c.x + (H(s, i + 2) - .5f) * 2 * W + Mathf.Sin(t * 1.6f + i) * .35f, c.y + Hh - u * 2 * Hh);
                        Glint(b, p, .12f + .1f * H(s, i + 3), new Color(.85f, .97f, 1f), e * Mathf.Sin(u * Mathf.PI) * (.6f + .4f * Mathf.Sin(t * 9 + i)), t * 60 + i * 30);
                    }
                    for (int i = 0; i < 5; i++) b.Floor(FxShape.Dust, new Vector2(c.x + (i - 2) * W * .4f, c.y - Hh * .8f), 1.4f, A(new Color(.8f, .95f, 1f), .25f * e), t * .3f, s + i);
                    break;
                case ElementLook.Water: // two arching fountains of drops cross over the middle and rain back down
                    for (int side = -1; side <= 1; side += 2)
                        for (int i = 0; i < 24; i++)
                        {
                            float u = (t - i * .045f) / 1.3f; if (u < 0 || u > 1.25f) continue;
                            float x = c.x + side * W * (.85f - 1.7f * Mathf.Min(u, 1) * .55f), y = c.y - Hh * .7f + 4 * Mathf.Min(u, 1) * (1 - Mathf.Min(u, 1)) * Hh * 1.5f - Mathf.Max(0, u - 1) * 4;
                            b.Quad(FxShape.Glow, new Vector2(x, y), new Vector2(.11f, .15f), 0, A(new Color(.5f, .78f, 1f), e * .9f));
                            if (i % 4 == 0) b.Quad(FxShape.Glow, new Vector2(x, y), .35f, A(new Color(.15f, .4f, 1f), .3f * e));
                        }
                    break;
                case ElementLook.Storm: // a cloud bank overhead flickers and arcs leap cloud to cloud (never down at the field)
                {
                    Vector2[] clouds = new Vector2[5];
                    for (int i = 0; i < 5; i++) { clouds[i] = new Vector2(c.x + (i - 2) * W * .42f, c.y + Hh * .85f + H(s, i) * .4f); b.Quad(FxShape.Smoke, clouds[i], 1.3f, A(new Color(.18f, .2f, .32f), .75f * e), t * .3f, s + i); }
                    int f = Mathf.FloorToInt(t * 9);
                    for (int i = 0; i < 4; i++)
                    {
                        if (H(s + f, i) < .45f) continue;
                        b.Quad(FxShape.Glow, clouds[i], 1.1f, A(new Color(.5f, .7f, 1f), .35f * e));
                        Bolt(b, clouds[i] + Vector2.down * .2f, clouds[i + 1] + Vector2.down * .2f, .07f, new Color(.45f, .7f, 1f), s + f * 13 + i, .6f, e);
                    }
                    break;
                }
                case ElementLook.Wind: // ribbons of breeze sweep across with leaves tumbling in them
                    for (int r = 0; r < 4; r++)
                    {
                        float y0 = c.y + (r - 1.5f) * Hh * .55f, head = -W + (t - r * .2f) * W * 1.2f;
                        for (int k = 0; k < 16; k++)
                        {
                            float x = head - k * .35f; if (x < -W || x > W) continue;
                            Vector2 p = new Vector2(c.x + x, y0 + Mathf.Sin(x * .9f + r) * .45f);
                            b.Quad(FxShape.Glow, p, new Vector2(.32f, .09f), Mathf.Cos(x * .9f + r) * 22, A(new Color(.7f, 1f, .85f), e * .55f * (1 - k / 16f)));
                            if (k % 5 == 2) b.Quad(FxShape.Disc, p + Vector2.up * .2f, new Vector2(.12f, .06f), t * 500 + k * 40, A(new Color(.55f, .9f, .4f), e));
                        }
                    }
                    break;
                case ElementLook.Bloom: // petals spin down from above while little flowers pop open across the floor
                    for (int i = 0; i < 30; i++)
                    {
                        float u = Mathf.Repeat(t * (.35f + .15f * H(s, i)) + H(s, i + 1), 1);
                        Vector2 p = new Vector2(c.x + (H(s, i + 2) - .5f) * 2 * W + Mathf.Sin(t * 2.4f + i) * .4f, c.y + Hh - u * 2 * Hh);
                        b.Quad(FxShape.Disc, p, new Vector2(.13f, .07f * (.3f + .7f * Mathf.Abs(Mathf.Sin(t * 4 + i)))), t * 120 + i * 50, A(i % 4 == 0 ? new Color(1, .95f, .8f) : new Color(1, .55f, .75f), e * Mathf.Sin(u * Mathf.PI)));
                    }
                    for (int i = 0; i < 9; i++)
                    {
                        float o = EaseOut3(Rise(t, .3f + i * .12f, .7f + i * .12f)); if (o <= 0) continue;
                        Vector2 p = new Vector2(c.x + (H(s, i + 40) - .5f) * 2 * W * .9f, c.y - Hh + H(s, i + 41) * Hh * .8f);
                        b.Quad(FxShape.Glow, p, .5f * o, A(new Color(1, .5f, .7f), .4f * e));
                        b.Quad(FxShape.Sparkle, p, .3f * o, 45 * o, A(new Color(1, .85f, .9f), e), 0, 0, 2);
                    }
                    break;
                case ElementLook.Poison: // bubbles well up all over the floor, swell, wobble and pop
                    for (int i = 0; i < 26; i++)
                    {
                        float u = Mathf.Repeat(t * (.5f + .3f * H(s, i)) + H(s, i + 1), 1);
                        Vector2 p = new Vector2(c.x + (H(s, i + 2) - .5f) * 2 * W + Mathf.Sin(t * 5 + i) * .1f, c.y - Hh + u * Hh * 1.6f);
                        if (u < .9f) { b.Quad(FxShape.Glow, p, .12f + .25f * u, A(new Color(.5f, 1f, .2f), .55f * e)); b.Quad(FxShape.Glow, p + new Vector2(-.06f, .06f) * (1 + 2 * u), .05f, A(White, .6f * e)); }
                        else Glint(b, p, .3f, new Color(.75f, 1f, .4f), e * (1 - u) * 10, i * 40);
                    }
                    for (int i = 0; i < 4; i++) b.Floor(FxShape.Smoke, new Vector2(c.x + (i - 1.5f) * W * .5f, c.y - Hh * .8f), 1.5f, A(new Color(.3f, .45f, .12f), .3f * e), t * .3f, s + i);
                    break;
                case ElementLook.Light: // sunbeams break through from above and gold motes settle down them
                    b.Quad(FxShape.Rays, c + Vector2.up * (Hh + .5f), new Vector2(4.2f, 3.6f), 0, A(new Color(1, .95f, .75f), .5f * e), t * .5f, s, 2);
                    for (int i = 0; i < 4; i++) b.Beam(new Vector2(c.x + (i - 1.5f) * 2.6f, c.y + Hh + 1), new Vector2(c.x + (i - 1.5f) * 3.6f, c.y - Hh), .55f, A(new Color(1, .93f, .7f), .14f * e * (.7f + .3f * Mathf.Sin(t * 3 + i))), t, s + i, .1f);
                    for (int i = 0; i < 30; i++)
                    {
                        float u = Mathf.Repeat(t * .4f + H(s, i), 1);
                        Glint(b, new Vector2(c.x + (H(s, i + 1) - .5f) * 2 * W * .8f, c.y + Hh - u * 2 * Hh), .1f, new Color(1, .9f, .6f), e * Mathf.Sin(u * Mathf.PI), i * 15);
                    }
                    break;
                case ElementLook.Void: // a slow dark whorl turns overhead and lets go of violet stars that drift down
                {
                    Vector2 eye = c + Vector2.up * Hh * .75f;
                    b.Quad(FxShape.Shade, eye, new Vector2(2.2f, 1.4f), 0, A(new Color(.05f, 0, .1f), .55f * e));
                    b.Quad(FxShape.Vortex, eye, new Vector2(1.8f, 1.1f), 0, A(new Color(.65f, .4f, 1f), .7f * e), t * .4f, s, 1);
                    for (int i = 0; i < 24; i++)
                    {
                        float u = Mathf.Clamp01((t - H(s, i) * 1.2f) / 1.3f); if (u <= 0) continue;
                        float a = H(s, i + 1) * 6.28f + u * 2.5f;
                        Vector2 p = eye + new Vector2(Mathf.Cos(a) * (1 + u * W * .8f), Mathf.Sin(a) * .6f * (1 + u * 2) - u * u * Hh * 1.2f);
                        Glint(b, p, .12f, new Color(.85f, .65f, 1f), e * Mathf.Sin(u * Mathf.PI), a * 60);
                    }
                    break;
                }
                case ElementLook.Earth: // pebbles are flung up from the floor in bouncing arcs, each landing puffs dust
                    for (int i = 0; i < 18; i++)
                    {
                        float u = (t - H(s, i) * .9f) / .9f; if (u < 0 || u > 1.6f) continue;
                        float x0 = c.x + (H(s, i + 1) - .5f) * 2 * W, dir = H(s, i + 2) < .5f ? -1 : 1;
                        float hop = u < 1 ? 4 * u * (1 - u) * 1.8f : 4 * (u - 1) / .6f * (1 - (u - 1) / .6f) * .5f;
                        Vector2 p = new Vector2(x0 + dir * Mathf.Min(u, 1.6f) * 1.2f, c.y - Hh * .6f + hop);
                        b.Quad(FxShape.Disc, p, new Vector2(.13f, .1f), u * 400 * dir, A(new Color(.62f, .45f, .3f), e));
                        float land = Mathf.Abs(u - 1); if (land < .25f) b.Floor(FxShape.Dust, new Vector2(p.x, c.y - Hh * .6f), .6f, A(new Color(.6f, .5f, .38f), (1 - land * 4) * .6f * e), t, s + i);
                    }
                    break;
                case ElementLook.Gold: // confetti and coins fountain up from both lower corners and flutter down
                    for (int side = -1; side <= 1; side += 2)
                        for (int i = 0; i < 22; i++)
                        {
                            float u = (t - H(s, i + side * 50) * .6f) / 1.9f; if (u < 0 || u > 1) continue;
                            float vx = (.4f + .6f * H(s, i + 3)) * -side * W * .9f, rise = EaseOut(Mathf.Min(1, u * 2.2f)) * Hh * (1.4f + .5f * H(s, i + 4)) - Mathf.Max(0, u - .45f) * Hh * 1.2f;
                            Vector2 p = new Vector2(c.x + side * W * .85f + vx * EaseOut(u) + Mathf.Sin(t * 6 + i) * .15f, c.y - Hh + rise);
                            Color col = i % 3 == 0 ? new Color(1, .8f, .25f) : Color.HSVToRGB(H(s, i + 5), .6f, 1);
                            b.Quad(i % 3 == 0 ? FxShape.Disc : FxShape.Glow, p, new Vector2(.1f, .1f * Mathf.Abs(Mathf.Cos(t * 9 + i))), t * 300 + i * 30, A(col, e * Fade(u, .8f, 1)));
                        }
                    break;
                case ElementLook.Paper: // loose pages flutter down in falling-leaf swings with ink flecks
                    for (int i = 0; i < 30; i++)
                    {
                        float u = Mathf.Repeat(t * (.3f + .1f * H(s, i)) + H(s, i + 1), 1), sw = Mathf.Sin(t * 2.6f + i * 1.3f);
                        Vector2 p = new Vector2(c.x + (H(s, i + 2) - .5f) * 2 * W + sw * .6f, c.y + Hh - u * 2 * Hh + Mathf.Abs(sw) * .2f);
                        b.Quad(FxShape.Disc, p, new Vector2(.14f, .1f * (.4f + .6f * Mathf.Abs(Mathf.Cos(t * 2 + i)))), sw * 35, A(new Color(1, .94f, .8f), e * Mathf.Sin(u * Mathf.PI)));
                        b.Quad(FxShape.Glow, p, .45f, A(new Color(1, .8f, .5f), .15f * e * Mathf.Sin(u * Mathf.PI)));
                    }
                    break;
                case ElementLook.Time: // a great clock hand sweeps once round over the field and leaves golden sand falling behind it
                {
                    float ang = 90 - 360 * EaseOut(Mathf.Clamp01(t / 1.8f)), rad = ang * Mathf.Deg2Rad;
                    Vector2 tip = c + new Vector2(Mathf.Cos(rad) * W * .5f, Mathf.Sin(rad) * Hh * .8f);
                    b.Beam(c, tip, .12f, A(new Color(1, .9f, .6f), .7f * e), t, s, .4f);
                    b.Quad(FxShape.Glow, c, .4f, A(new Color(1, .85f, .5f), .6f * e));
                    for (int i = 0; i < 36; i++)
                    {
                        float when = i / 36f * 1.8f, age = t - when; if (age < 0 || age > 1.1f) continue;
                        float r2 = (90 - 360 * EaseOut(when / 1.8f)) * Mathf.Deg2Rad, along = .35f + .6f * H(s, i);
                        Vector2 p = c + new Vector2(Mathf.Cos(r2) * W * .5f * along, Mathf.Sin(r2) * Hh * .8f * along - age * age * 2.5f);
                        b.Quad(FxShape.Glow, p, .07f, A(new Color(1, .88f, .5f), e * (1 - age / 1.1f)));
                    }
                    break;
                }
                case ElementLook.Crystal: // a lattice of crystal grows sideways across the field, then breaks into falling glints
                {
                    float grow = Mathf.Clamp01(t / 1.2f), crack = Rise(t, 1.4f, 2.2f);
                    for (int i = 0; i < 16; i++)
                    {
                        Vector2 a = new Vector2(c.x + (i / 15f - .5f) * 2 * W * .85f, c.y + (H(s, i) - .5f) * Hh * .55f);
                        Vector2 z = new Vector2(c.x + ((i + 1) / 15f - .5f) * 2 * W * .85f, c.y + (H(s, i + 1) - .5f) * Hh * .55f);
                        float on = Mathf.Clamp01(grow * 16 - Mathf.Abs(i - 7.5f) * 2); if (on <= 0) continue;
                        Vector2 fall = Vector2.down * crack * crack * 3;
                        if (crack < .2f && i < 15) { b.Link(a, Vector2.Lerp(a, z, on), .22f, A(new Color(.45f, .8f, 1f), .5f * e * (1 - crack * 5)), s + i, 0); b.Link(a, Vector2.Lerp(a, z, on), .1f, A(new Color(.85f, .98f, 1f), .9f * e * (1 - crack * 5)), s + i, .6f); }
                        b.Quad(FxShape.Glow, a + fall, .6f, A(new Color(.4f, .8f, 1f), .45f * e * on)); Glint(b, a + fall, .45f + .12f * Mathf.Sin(t * 8 + i), new Color(.85f, .98f, 1f), e * on, i * 25 + crack * 200);
                    }
                    break;
                }
                case ElementLook.Dream: // dream bubbles rise slowly with stars winking inside them through a lavender haze
                    for (int i = 0; i < 4; i++) b.Quad(FxShape.Smoke, new Vector2(c.x + (i - 1.5f) * W * .5f, c.y), 1.6f, A(new Color(.55f, .45f, .8f), .25f * e), t * .2f, s + i);
                    for (int i = 0; i < 14; i++)
                    {
                        float u = Mathf.Repeat(t * .3f + H(s, i), 1);
                        Vector2 p = new Vector2(c.x + (H(s, i + 1) - .5f) * 2 * W + Mathf.Sin(t * 1.5f + i) * .4f, c.y - Hh + u * 2 * Hh);
                        float a = e * Mathf.Sin(u * Mathf.PI);
                        b.Quad(FxShape.Glow, p, .4f, A(new Color(.75f, .55f, 1f), .4f * a));
                        Glint(b, p + new Vector2(.08f, .06f), .16f, new Color(1, .85f, 1f), a * Mathf.Max(0, Mathf.Sin(t * 6 + i)), 45);
                    }
                    break;
                case ElementLook.Sound: // a stave appears and notes hop along it from left to right, bouncing in a wave
                    for (int l = 0; l < 5; l++) b.Beam(new Vector2(c.x - W, c.y + (l - 2) * .28f), new Vector2(c.x + W, c.y + (l - 2) * .28f), .05f, A(new Color(1, .6f, 1f), .3f * e), 0, s + l, .8f);
                    for (int i = 0; i < 12; i++)
                    {
                        float u = (t - i * .1f) / 1.6f; if (u < 0 || u > 1) continue;
                        float x = c.x - W + u * 2 * W, hop = Mathf.Abs(Mathf.Sin(u * Mathf.PI * 5 + i)) * .9f;
                        Vector2 head = new Vector2(x, c.y - .4f + hop + (i % 3) * .28f);
                        b.Quad(FxShape.Glow, head, new Vector2(.2f, .15f), -20, A(new Color(1, .7f, 1f), e));
                        b.Beam(head + new Vector2(.17f, 0), head + new Vector2(.17f, .6f), .05f, A(new Color(1, .8f, 1f), e), 0, s + i, .8f);
                    }
                    break;
                default: // a gentle shower of glints in the element's colour
                {
                    Color col = Color.Lerp(RulesV3Ink.LookColour(look), White, .3f);
                    for (int i = 0; i < 24; i++) { float u = Mathf.Repeat(t * .4f + H(s, i), 1); Glint(b, new Vector2(c.x + (H(s, i + 1) - .5f) * 2 * W, c.y + Hh - u * 2 * Hh), .12f, col, e * Mathf.Sin(u * Mathf.PI), i * 20); }
                    break;
                }
            }
        }
    }
}
