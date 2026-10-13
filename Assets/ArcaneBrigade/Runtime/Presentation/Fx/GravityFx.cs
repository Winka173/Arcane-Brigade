using System;
using System.Collections.Generic;
using UnityEngine;
using static ArcaneBrigade.FxKit;

namespace ArcaneBrigade
{
    /// <summary>Gravity bond: black holes, lensing rings, debris pulled inward, purple-white accretion.</summary>
    public static class GravityFx
    {
        // owner 29/10/2026: small glints and sparks were drawn far too big: small ones shrink, big set-piece flashes keep their size
        private static void Glint(FxBatch b, Vector2 at, float size, Color tint, float alpha, float turn = 0)
            => FxKit.Glint(b, at, size <= .08f ? size : size < .5f ? .08f + (size - .08f) * .45f : size - .231f, tint, alpha, turn);
        public static void Register(Dictionary<string, Func<FxEffect>> map)
        {
            map["gravity-clump"] = () => new Clump();
            map["singularity-collapse"] = () => new Collapse();
            map["gravity-orb"] = () => new Orb();
            map["gravity-pull"] = () => new Pull();
            map["gravity-heavy"] = () => new Heavy();
        }

        private static readonly Color Soot = new Color(.12f, .08f, .2f, 1), Grit = new Color(.42f, .36f, .52f, 1);

        /// <summary>Beams streaking inward: the 'pulled to the centre' vocabulary of the bond.</summary>
        private static void Inflow(FxBatch b, Vector2 at, float r, float u, int seed, int n, Color c, float alpha)
        {
            for (int k = 0; k < n; k++)
            {
                float v = (u + H(seed, k) * .6f) % 1f; Vector2 dir = Dir(seed, k, n, .62f);
                Vector2 p = at + dir * r * (1 - v * v), tail = at + dir * r * (1 - Mathf.Max(0, v - .22f) * Mathf.Max(0, v - .22f));
                b.Link(tail, p, .05f + .06f * v, A(c, alpha * (.3f + .7f * v)), seed + k, .5f);
            }
        }

        /// <summary>Low grit dragged along the floor towards a point (dust being sucked in).</summary>
        private static void DustIn(FxBatch b, Vector2 at, float r, float u, int seed, int n, float alpha)
        {
            for (int k = 0; k < n; k++)
            {
                float v = (u + H(seed, k + 80)) % 1f; Vector2 dir = Dir(seed, k + 80, n, .62f);
                b.Quad(FxShape.Dust, at + dir * r * (1 - EaseOut(v)), new Vector2(.5f * (1 - v * .5f), .2f), DKit.Deg(dir), A(Grit, alpha * Mathf.Sin(v * Mathf.PI)), v, seed + k);
            }
        }

        /// <summary>A small purple rock falls onto the pile of crowded enemies trailing smoke: converging debris first, then a thump with a
        /// crater web of cracks, a ring of dust, rubble bouncing out, violet sparks and a slow column of soot.</summary>
        private sealed class Clump : FxEffect
        {
            protected override float Life => 1.25f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                if (t < 0) return; Vector2 at = At; float r = Mathf.Max(1f, Radius) * 1.0f; const float fall = .22f;
                if (t < fall)
                {
                    float u = t / fall; Vector2 p = at + Vector2.up * 5.5f * (1 - DKit.EaseIn(u));
                    Inflow(b, at, r * 1.2f, t * 5, Seed, 8, DKit.Lavender, .8f);
                    DustIn(b, at, r * 1.3f, t * 4, Seed, 6, .6f);
                    b.Floor(FxShape.Shade, at, r * .8f * u, A(DKit.Dark, .6f * u), 0, Seed);
                    for (int k = 1; k <= 4; k++) b.Quad(FxShape.Smoke, p + Vector2.up * k * .4f, .35f + .1f * k, A(Soot, .55f * (1 - k / 5f)), u, Seed + k);
                    b.Quad(FxShape.Glow, p, 1.0f, A(DKit.Purple, .6f), 0, Seed);
                    b.Quad(FxShape.Smoke, p, .6f, A(new Color(.1f, .06f, .18f), 1), 0, Seed);
                    b.Quad(DKit.Star, p, .7f, t * 300, A(DKit.Lavender, 1.1f), 0, Seed, 7);
                    return;
                }
                float s = t - fall;
                b.Quad(FxShape.Glow, at, r * 1.4f, A(DKit.Lavender, .75f * Fade(s, 0, .18f)), 0, Seed);
                b.Floor(FxShape.Shade, at, r * .8f, A(DKit.Dark, .55f * Fade(s, .5f, 1f)), 0, Seed + 1);
                b.Floor(FxShape.Cracks, at, r * 1.3f, A(DKit.Purple, 1.1f * Fade(s, .3f, 1f)), Rise(s, 0, .2f), Seed, 1);
                b.Floor(FxShape.Shock, at, r * 1.25f, A(DKit.Lavender, .9f), Rise(s, 0, .55f), Seed);
                DKit.DustRing(b, at, r * 1.0f, s, Grit, Seed, 9, .95f, .7f);
                FxKit.SmokeColumn(b, at + Vector2.up * .2f, .6f, 1.3f, s, Soot, Seed + 4, 5);
                if (s < .7f) b.Quad(DKit.Star, at + Vector2.up * .1f, .6f * (1 - s * .8f), 40, A(DKit.Lavender, .9f * Fade(s, .35f, .7f)), 0, Seed, 7);
                DKit.Debris(b, at, s, 10, r * 1.3f, 1.4f, DKit.Purple, Seed, DKit.Star, 6, .2f, .85f, Grit);
                DKit.Sparks(b, at + Vector2.up * .2f, s, 10, 4f, 3f, DKit.Lavender, Seed, .45f, .09f);
            }
        }

        /// <summary>The hole: a purple-white accretion disc spins up and shrinks while rubble and light streak into it and dust is dragged
        /// over the floor into it; then it implodes, a white flash, a lensing ring runs out and the stuff is thrown back with a dust ring
        /// and violet sparks.</summary>
        private sealed class Collapse : FxEffect
        {
            protected override float Life => 1.7f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                if (t < 0) return; Vector2 at = At; float r = Mathf.Max(1.2f, Radius) * 1.0f; const float imp = .55f;
                if (t < imp)
                {
                    float u = t / imp, size = r * (1.3f - .7f * EaseOut(u));
                    b.Floor(FxShape.Shade, at, size * .85f, A(DKit.Dark, .85f), 0, Seed);
                    b.Quad(FxShape.Glow, at, size * 1.1f, A(DKit.Purple, .4f), 0, Seed);
                    b.Quad(DKit.Hole, at, DKit.Sq(size) * 1.05f, 0, A(DKit.Lavender, 1.2f), t * 2.2f, Seed, 2);
                    b.Quad(DKit.Hole, at, DKit.Sq(size), 0, A(DKit.Lavender, 1.3f), t * 1.6f, Seed, 0);
                    b.Floor(FxShape.Shade, at, size * .26f, A(DKit.Dark, 1), 0, Seed + 1);
                    b.Floor(DKit.Hole, at, size * 1.3f, A(DKit.Purple, .8f * u), t, Seed + 2, 1);
                    Inflow(b, at, r * 1.4f, t * 3.2f, Seed, 12, DKit.Lavender, .9f);
                    DustIn(b, at, r * 1.5f, t * 2.5f, Seed, 10, .7f);
                    for (int k = 0; k < 10; k++)
                    {
                        float v = (u * 1.3f + H(Seed, k)) % 1f, a = k * .63f + v * 5; float rad = r * 1.35f * (1 - v);
                        b.Quad(DKit.Star, at + new Vector2(Mathf.Cos(a) * rad, Mathf.Sin(a) * rad * .62f), .16f * (1 - v * .5f), t * 700, A(k % 3 == 0 ? White : DKit.Purple, 1.1f), 0, Seed + k, 5);
                    }
                    return;
                }
                float s = t - imp;
                b.Quad(FxShape.Glow, at, r * 1.4f * (.5f + .5f * Fade(s, 0, .2f)), A(White, .9f * Fade(s, 0, .2f)), 0, Seed);
                b.Quad(FxShape.Glow, at, r * 1.3f, A(DKit.Purple, .45f * Fade(s, 0, .7f)), 0, Seed);
                b.Floor(FxShape.Shade, at, r * .7f, A(DKit.Dark, .55f * Fade(s, .4f, 1.1f)), 0, Seed + 3);
                for (int k = 0; k < 3; k++)
                {
                    float u = Rise(s, k * .1f, .8f + k * .1f); if (u <= 0 || u >= 1) continue;
                    b.Floor(DKit.Hole, at, r * (.6f + 1.0f * EaseOut(u)), A(k == 1 ? DKit.Lavender : DKit.Purple, 1.2f * Fade(u, .3f, 1)), s + k, Seed + k, 1);
                }
                b.Floor(FxShape.Shock, at, r * 1.45f, A(DKit.Lavender, .9f), Rise(s, 0, .8f), Seed);
                b.Quad(FxShape.Rays, at, r * 1.6f, A(DKit.Lavender, .55f * Fade(s, 0, .4f)), s, Seed, 4);
                b.Floor(FxShape.Cracks, at, r * 1.2f, A(DKit.Purple, .9f * Fade(s, .1f, .9f)), Rise(s, 0, .3f), Seed, 1);
                DKit.DustRing(b, at, r * 1.1f, s, Grit, Seed, 10, 1.0f, .7f);
                DKit.Sparks(b, at + Vector2.up * .2f, s, 12, 5f, 3f, DKit.Lavender, Seed + 5, .5f, .09f);
                DKit.Debris(b, at, s, 12, r * 1.4f, 1.0f, DKit.Purple, Seed + 2, DKit.Star, 5, .18f, .9f, Grit);
            }
        }

        /// <summary>A small ringed planet drops out of the sky onto the crowd trailing a smoke wake: its shadow spreads, a lensing halo
        /// tightens round it and stuff rises into it; the slam throws a lensing shock, a crack web, a rolling cloud of dust and rubble,
        /// sparks spray, and the planet sinks glowing into the floor while soot curls up.</summary>
        private sealed class Orb : FxEffect
        {
            protected override float Life => 1.9f;
            private static void Planet(FxBatch b, Vector2 p, float size, float alpha, int seed, float now)
            {
                b.Quad(FxShape.Glow, p, size * 2.2f, A(DKit.Purple, .5f * alpha), 0, seed);
                b.Quad(DKit.Hole, p, size * 2.0f, A(DKit.Lavender, .7f * alpha), now, seed, 1);
                b.Quad(FxShape.Shade, p, size * 1.7f, A(new Color(.1f, .04f, .22f), alpha), 0, seed);
                b.Quad(FxShape.Shade, p, size * 1.45f, A(new Color(.18f, .08f, .34f), alpha), 0, seed + 1);
                b.Quad(FxShape.Disc, p, size, A(new Color(.4f, .2f, .8f), .35f * alpha), 0, seed);
                b.Quad(FxShape.Disc, p + new Vector2(-.2f, .22f) * size, size * .55f, A(new Color(.6f, .45f, 1f), .3f * alpha), 0, seed);
                b.Quad(FxShape.Glow, p + new Vector2(-.35f, .35f) * size, size * .45f, A(White, .5f * alpha), 0, seed);
                for (int k = 0; k < 3; k++) b.Quad(FxShape.Smoke, p + new Vector2(0, (k - 1) * size * .4f), new Vector2(size * .9f, size * .13f), 0, A(new Color(.1f, .05f, .3f), .5f * alpha), now * .1f, seed + k);
                b.Quad(FxShape.Shock, p, new Vector2(size * 2.1f, size * .75f), 18, A(DKit.Lavender, .9f * alpha), .3f, seed);
                for (int k = 0; k < 5; k++) { float a = now * 2.2f + k * 1.2566f; Vector2 o = new Vector2(Mathf.Cos(a) * size * 2.0f, Mathf.Sin(a) * size * .7f); o = new Vector2(o.x * .951f - o.y * .309f, o.x * .309f + o.y * .951f); Glint(b, p + o, .14f, White, alpha * (Mathf.Sin(a) < 0 ? .4f : 1), k * 40); }
            }

            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Mathf.Max(1.8f, Radius) * .9f, pr = 1.0f;
                if (t < 0)
                {
                    float u = DKit.EaseIn(pre); Vector2 pp = at + Vector2.up * 7.5f * (1 - u);
                    b.Floor(FxShape.Shade, at, r * 1.0f * u, A(DKit.Dark, .7f * u), 0, Seed);
                    b.Floor(DKit.Hole, at, r * 1.4f * (1.2f - .4f * pre), A(DKit.Purple, .8f * pre), now, Seed, 1);
                    Inflow(b, at, r * 1.5f, pre * 2, Seed, 10, DKit.Lavender, .7f * pre);
                    DustIn(b, at, r * 1.4f, pre * 1.5f, Seed, 8, .6f * pre);
                    for (int k = 1; k <= 6; k++) b.Quad(FxShape.Smoke, pp + new Vector2(Mathf.Sin(k * 1.7f) * .2f, k * .55f), .4f + .1f * k, A(Soot, .5f * (1 - k / 7f) * pre), pre + k * .1f, Seed + k);
                    Planet(b, pp, pr * (.5f + .5f * u), 1, Seed, now);
                    return;
                }
                float sink = Rise(t, .35f, 1.5f), alpha = Fade(t, .9f, 1.7f);
                b.Quad(FxShape.Glow, at, r * 1.5f, A(White, .65f * Fade(t, 0, .2f)), 0, Seed);
                b.Floor(FxShape.Cracks, at, r * 1.3f, A(DKit.Purple, 1.2f * Fade(t, .5f, 1.6f)), Rise(t, 0, .3f), Seed, 3);
                for (int k = 0; k < 3; k++)
                {
                    float u = Rise(t, k * .12f, .9f + k * .1f); if (u <= 0 || u >= 1) continue;
                    b.Floor(DKit.Hole, at, r * (.7f + 1.0f * EaseOut(u)), A(k == 1 ? DKit.Lavender : DKit.Purple, 1.2f * Fade(u, .3f, 1)), t + k, Seed + k, 1);
                }
                b.Floor(FxShape.Shock, at, r * 1.45f, A(DKit.Lavender, .9f), Rise(t, 0, .8f), Seed);
                DKit.DustRing(b, at, r * 1.2f, t, Grit, Seed, 12, 1.4f, .8f);
                FxKit.SmokeColumn(b, at + Vector2.up * .3f, .8f, 1.8f, t * .8f, Soot, Seed + 9, 6);
                Planet(b, at + Vector2.down * sink * .5f + Vector2.up * .35f * Fade(t, 0, .4f), pr * (1 - sink * .25f), alpha, Seed, now);
                DKit.Debris(b, at, t, 14, r * 1.5f, 1.8f, DKit.Purple, Seed + 1, DKit.Star, 6, .22f, 1.0f, Grit);
                DKit.Sparks(b, at + Vector2.up * .3f, t, 14, 5.5f, 3.5f, DKit.Lavender, Seed + 3, .55f, .1f);
            }
        }

        /// <summary>A hit that drags an enemy: short lavender streaks and a skid of grit race into a tiny dark dimple with a swirl.</summary>
        private sealed class Pull : FxEffect
        {
            protected override float Life => .5f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                if (t < 0) return; Vector2 at = At; float r = Mathf.Max(.9f, Radius) * .9f, f = Fade(t, .15f, .5f);
                b.Floor(FxShape.Shade, at, r * .55f, A(DKit.Dark, .6f * f), 0, Seed);
                b.Quad(DKit.Hole, at, DKit.Sq(r * 1.1f), 0, A(DKit.Lavender, 1.1f * f), t * 2, Seed, 2);
                Inflow(b, at, r * 1.2f, t * 3, Seed, 7, DKit.Lavender, f);
                DustIn(b, at, r * 1.1f, t * 2.4f, Seed, 4, .55f * f);
                for (int k = 0; k < 4; k++) { float v = (t * 2.5f + k * .25f) % 1f; Glint(b, at + Dir(Seed, k, 4, .62f) * r * (1 - v), .13f, White, f * Mathf.Sin(v * Mathf.PI), k * 40); }
            }
        }

        /// <summary>Step 8 weather: heavy gravity over the arena, a light touch. Thin sagging bands of force creep down the field, small
        /// lensing ripples drift over the floor, grit sinks out of the air and settles as low dust, and now and then a pebble drops
        /// with a tiny thud.</summary>
        private sealed class Heavy : FxEffect
        {
            protected override float Life => 1f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                if (Moment.Kind != BondMomentKind.Weather) return;
                float age = now - Born, on = Rise(age, 0, 2f) * Fade(now, End - 1.5f, End); Rect a = Arena; Vector2 c = a.center;
                b.Quad(FxShape.Shade, c, new Vector2(a.width * .5f, a.height * .5f), 0, A(new Color(.06f, .02f, .14f), .14f * on), 0, Seed);
                // bands of force sag across the arena and creep downward
                for (int k = 0; k < 5; k++)
                {
                    float u = (k / 5f + age * .05f) % 1f, y = a.yMax - u * a.height, al = on * Mathf.Sin(u * Mathf.PI);
                    Vector2 l = new Vector2(a.xMin - .5f, y), r = new Vector2(a.xMax + .5f, y);
                    DKit.Chain(b, l, r, -.7f, .5f, 0, .05f, DKit.Purple, .4f * al, Seed + k * 9, 24, .5f);
                }
                // lensing ripples drift slowly over the floor
                for (int k = 0; k < 5; k++)
                {
                    float u = (age * .1f + k * .2f) % 1f; Vector2 p = new Vector2(a.xMin + a.width * ((H(Seed, k) + age * .015f * (k % 2 == 0 ? 1 : -1)) % 1f + 1f) % 1f, a.yMin + a.height * (.2f + .6f * H(Seed, k + 7)));
                    b.Floor(DKit.Hole, p, 1.0f + 1.0f * u, A(DKit.Purple, .5f * on * Mathf.Sin(u * Mathf.PI)), age * .3f + k, Seed + k, 1);
                    b.Floor(FxShape.Shade, p, .6f * (1 - u * .5f), A(DKit.Dark, .25f * on * Mathf.Sin(u * Mathf.PI)), 0, Seed + k);
                }
                // grit sinks out of the air, streaking, and settles as low dust
                for (int k = 0; k < 44; k++)
                {
                    float sp = 1.6f + 1.4f * H(Seed, k + 300), u = (H(Seed, k) + age * sp / (a.height + 2)) % 1f;
                    Vector2 p = new Vector2(a.xMin + a.width * H(Seed, k + 150), a.yMax + 1 - u * (a.height + 2));
                    b.Beam(p + Vector2.up * .45f, p, .025f, A(DKit.Lavender, .3f * on), 0, k, 0);
                    b.Quad(DKit.Star, p, .1f + .07f * H(Seed, k + 9), age * 90 + k * 30, A(k % 4 == 0 ? White : DKit.Purple, .8f * on), 0, Seed + k, 5 + k % 3);
                }
                for (int k = 0; k < 8; k++)
                {
                    float u = (age * .12f + H(Seed, k + 600)) % 1f; Vector2 p = new Vector2(a.xMin + a.width * H(Seed, k + 620), a.yMin + a.height * (.1f + .8f * H(Seed, k + 640)));
                    b.Quad(FxShape.Dust, p + Vector2.right * u * .6f, new Vector2(1.1f, .32f), 0, A(Grit, .35f * on * Mathf.Sin(u * Mathf.PI)), u, Seed + k);
                }
                // pebbles drop and thud
                for (int k = 0; k < 4; k++)
                {
                    float cyc = 1.7f + .6f * H(Seed, k + 700), ph = (age + H(Seed, k + 710) * cyc) % cyc, n = Mathf.Floor((age + H(Seed, k + 710) * cyc) / cyc);
                    Vector2 land = new Vector2(a.xMin + 2 + (a.width - 4) * H(Seed + (int)n, k + 720), a.yMin + 1.5f + (a.height - 3) * H(Seed + (int)n, k + 730));
                    if (ph < .3f) { float u = ph / .3f; b.Quad(DKit.Star, land + Vector2.up * 3f * (1 - u * u), .2f, age * 400, A(DKit.Lavender, on), 0, Seed + k, 6); }
                    else { float u = Rise(ph, .3f, .9f); if (u < 1) { b.Quad(FxShape.Dust, land, new Vector2(.35f + .5f * u, .16f + .16f * u), 0, A(Grit, .6f * on * (1 - u)), u, Seed + k); b.Floor(FxShape.Shock, land, .7f, A(DKit.Lavender, .7f * on), u, Seed + k); } }
                }
            }
        }
    }
}
