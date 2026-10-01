using System;
using System.Collections.Generic;
using UnityEngine;
using static ArcaneBrigade.FxKit;

namespace ArcaneBrigade
{
    /// <summary>Gravity bond: black holes, lensing rings, debris pulled inward, purple-white accretion.</summary>
    public static class GravityFx
    {
        public static void Register(Dictionary<string, Func<FxEffect>> map)
        {
            map["gravity-clump"] = () => new Clump();
            map["singularity-collapse"] = () => new Collapse();
            map["gravity-orb"] = () => new Orb();
            map["gravity-pull"] = () => new Pull();
            map["gravity-heavy"] = () => new Heavy();
        }

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

        /// <summary>A small purple rock falls onto the pile of crowded enemies: converging debris first, then a thump with a crater web
        /// of cracks, a ring of dust, and rubble bouncing out.</summary>
        private sealed class Clump : FxEffect
        {
            protected override float Life => 1.2f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                if (t < 0) return; Vector2 at = At; float r = Mathf.Max(1f, Radius) * 1.25f; const float fall = .22f;
                if (t < fall)
                {
                    float u = t / fall; Vector2 p = at + Vector2.up * 7f * (1 - DKit.EaseIn(u));
                    Inflow(b, at, r * 1.3f, t * 5, Seed, 8, DKit.Lavender, .8f);
                    b.Floor(FxShape.Shade, at, r * .9f * u, A(DKit.Dark, .6f * u), 0, Seed);
                    b.Quad(FxShape.Glow, p, 1.2f, A(DKit.Purple, .6f), 0, Seed);
                    b.Quad(FxShape.Smoke, p, .75f, A(new Color(.1f, .06f, .18f), 1), 0, Seed);
                    b.Quad(DKit.Star, p, .85f, t * 300, A(DKit.Lavender, 1.1f), 0, Seed, 7);
                    return;
                }
                float s = t - fall;
                b.Quad(FxShape.Glow, at, r * 2f, A(DKit.Lavender, .8f * Fade(s, 0, .18f)), 0, Seed);
                b.Floor(FxShape.Cracks, at, r * 1.6f, A(DKit.Purple, 1.1f * Fade(s, .3f, 1f)), Rise(s, 0, .2f), Seed, 1);
                b.Floor(FxShape.Shock, at, r * 1.5f, A(DKit.Lavender, .9f), Rise(s, 0, .55f), Seed);
                b.Floor(FxShape.Smoke, at, r * 1.1f, A(new Color(.25f, .18f, .35f), .6f * Fade(s, .1f, .9f)), Rise(s, 0, 1), Seed + 2);
                b.Quad(FxShape.Smoke, at + Vector2.up * .2f, .8f * (1 + s), A(new Color(.08f, .05f, .14f), .9f * Fade(s, .3f, .95f)), 0, Seed + 4);
                if (s < .7f) b.Quad(DKit.Star, at, .75f * (1 - s * .8f), 40, A(DKit.Lavender, .9f * Fade(s, .35f, .7f)), 0, Seed, 7);
                for (int k = 0; k < 11; k++)
                {
                    float life = .5f + .35f * H(Seed, k), u = s / life; if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + Dir(Seed, k, 11, .6f) * r * 1.5f * EaseOut(u) + Vector2.up * (2.2f * Mathf.Sin(u * Mathf.PI) * H(Seed, k + 7));
                    b.Quad(DKit.Star, p, .2f + .12f * H(Seed, k + 3), s * 500 * (k % 2 == 0 ? 1 : -1), A(k % 3 == 0 ? White : DKit.Purple, 1.2f * (1 - u * u)), 0, Seed + k, 5 + k % 3);
                }
            }
        }

        /// <summary>The hole: a purple-white accretion disc spins up and shrinks while rubble and light streak into it and the ground
        /// darkens; then it implodes, a white flash, a lensing ring runs out over the floor and the stuff is thrown back.</summary>
        private sealed class Collapse : FxEffect
        {
            protected override float Life => 1.7f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                if (t < 0) return; Vector2 at = At; float r = Mathf.Max(1.2f, Radius) * 1.35f; const float imp = .55f;
                if (t < imp)
                {
                    float u = t / imp, size = r * (1.5f - .8f * EaseOut(u));
                    b.Floor(FxShape.Shade, at, size * .8f, A(DKit.Dark, .85f), 0, Seed);
                    b.Quad(FxShape.Glow, at, size * 1.3f, A(DKit.Purple, .45f), 0, Seed);
                    b.Quad(DKit.Hole, at, DKit.Sq(size) * 1.05f, 0, A(DKit.Lavender, 1.2f), t * 2.2f, Seed, 2);
                    b.Quad(DKit.Hole, at, DKit.Sq(size), 0, A(DKit.Lavender, 1.3f), t * 1.6f, Seed, 0);
                    b.Floor(FxShape.Shade, at, size * .26f, A(DKit.Dark, 1), 0, Seed + 1);
                    b.Floor(DKit.Hole, at, size * 1.5f, A(DKit.Purple, .8f * u), t, Seed + 2, 1);
                    Inflow(b, at, r * 1.7f, t * 3.2f, Seed, 12, DKit.Lavender, .9f);
                    for (int k = 0; k < 10; k++)
                    {
                        float v = (u * 1.3f + H(Seed, k)) % 1f, a = k * .63f + v * 5; float rad = r * 1.6f * (1 - v);
                        b.Quad(DKit.Star, at + new Vector2(Mathf.Cos(a) * rad, Mathf.Sin(a) * rad * .62f), .17f * (1 - v * .5f), t * 700, A(k % 3 == 0 ? White : DKit.Purple, 1.1f), 0, Seed + k, 5);
                    }
                    return;
                }
                float s = t - imp;
                b.Quad(FxShape.Glow, at, r * 3f * (.4f + .6f * Fade(s, 0, .2f)), A(White, .95f * Fade(s, 0, .22f)), 0, Seed);
                b.Quad(FxShape.Glow, at, r * 2f, A(DKit.Purple, .5f * Fade(s, 0, .7f)), 0, Seed);
                for (int k = 0; k < 3; k++)
                {
                    float u = Rise(s, k * .1f, .8f + k * .1f); if (u <= 0 || u >= 1) continue;
                    b.Floor(DKit.Hole, at, r * (.7f + 1.5f * EaseOut(u)) * 1.3f, A(k == 1 ? DKit.Lavender : DKit.Purple, 1.2f * Fade(u, .3f, 1)), s + k, Seed + k, 1);
                }
                b.Floor(FxShape.Shock, at, r * 1.8f, A(DKit.Lavender, .9f), Rise(s, 0, .8f), Seed);
                b.Quad(FxShape.Rays, at, r * 2.4f, A(DKit.Lavender, .6f * Fade(s, 0, .4f)), s, Seed, 4);
                b.Floor(FxShape.Cracks, at, r * 1.5f, A(DKit.Purple, .9f * Fade(s, .1f, .9f)), Rise(s, 0, .3f), Seed, 1);
                for (int k = 0; k < 18; k++)
                {
                    float life = .55f + .5f * H(Seed, k), u = s / life; if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + Dir(Seed, k + 20, 18, .62f) * r * 1.9f * EaseOut(u) + Vector2.up * 1.3f * Mathf.Sin(u * Mathf.PI);
                    b.Quad(DKit.Star, p, .18f + .1f * H(Seed, k + 3), s * 600, A(k % 3 == 0 ? White : DKit.Purple, 1.2f * (1 - u * u)), 0, Seed + k, 5 + k % 3);
                }
            }
        }

        /// <summary>A small ringed planet drops out of the sky onto the crowd: its shadow spreads, a lensing halo tightens round it and
        /// stuff rises into it; the slam throws a lensing shock, a great crack web and rubble, and the planet sinks glowing into the floor.</summary>
        private sealed class Orb : FxEffect
        {
            protected override float Life => 1.9f;
            private static void Planet(FxBatch b, Vector2 p, float size, float alpha, int seed, float now)
            {
                b.Quad(FxShape.Glow, p, size * 2.6f, A(DKit.Purple, .5f * alpha), 0, seed);
                b.Quad(DKit.Hole, p, size * 2.1f, A(DKit.Lavender, .7f * alpha), now, seed, 1);
                b.Quad(FxShape.Shade, p, size * 1.7f, A(new Color(.1f, .04f, .22f), alpha), 0, seed);
                b.Quad(FxShape.Shade, p, size * 1.45f, A(new Color(.18f, .08f, .34f), alpha), 0, seed + 1);
                b.Quad(FxShape.Disc, p, size, A(new Color(.4f, .2f, .8f), .35f * alpha), 0, seed);
                b.Quad(FxShape.Disc, p + new Vector2(-.2f, .22f) * size, size * .55f, A(new Color(.6f, .45f, 1f), .3f * alpha), 0, seed);
                b.Quad(FxShape.Glow, p + new Vector2(-.35f, .35f) * size, size * .45f, A(White, .5f * alpha), 0, seed);
                for (int k = 0; k < 3; k++) b.Quad(FxShape.Smoke, p + new Vector2(0, (k - 1) * size * .4f), new Vector2(size * .9f, size * .13f), 0, A(new Color(.1f, .05f, .3f), .5f * alpha), now * .1f, seed + k);
                b.Quad(FxShape.Shock, p, new Vector2(size * 2.1f, size * .75f), 18, A(DKit.Lavender, .9f * alpha), .3f, seed);
            }

            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Mathf.Max(2f, Radius) * .95f, pr = 1.25f;
                if (t < 0)
                {
                    float u = DKit.EaseIn(pre);
                    b.Floor(FxShape.Shade, at, r * 1.1f * u, A(DKit.Dark, .7f * u), 0, Seed);
                    b.Floor(DKit.Hole, at, r * 1.7f * (1.2f - .4f * pre), A(DKit.Purple, .8f * pre), now, Seed, 1);
                    Inflow(b, at, r * 1.8f, pre * 2, Seed, 10, DKit.Lavender, .7f * pre);
                    Planet(b, at + Vector2.up * 9f * (1 - u), pr * (.5f + .5f * u), 1, Seed, now);
                    return;
                }
                float sink = Rise(t, .35f, 1.5f), alpha = Fade(t, .9f, 1.7f);
                b.Quad(FxShape.Glow, at, r * 2.2f, A(White, .7f * Fade(t, 0, .2f)), 0, Seed);
                b.Floor(FxShape.Cracks, at, r * 1.5f, A(DKit.Purple, 1.2f * Fade(t, .5f, 1.6f)), Rise(t, 0, .3f), Seed, 3);
                for (int k = 0; k < 3; k++)
                {
                    float u = Rise(t, k * .12f, .9f + k * .1f); if (u <= 0 || u >= 1) continue;
                    b.Floor(DKit.Hole, at, r * (.8f + 1.5f * EaseOut(u)) * 1.15f, A(k == 1 ? DKit.Lavender : DKit.Purple, 1.2f * Fade(u, .3f, 1)), t + k, Seed + k, 1);
                }
                b.Floor(FxShape.Shock, at, r * 1.7f, A(DKit.Lavender, .9f), Rise(t, 0, .8f), Seed);
                b.Floor(FxShape.Smoke, at, r * 1.2f, A(new Color(.2f, .14f, .3f), .55f * Fade(t, .15f, 1.4f)), Rise(t, 0, 1.4f), Seed + 2);
                Planet(b, at + Vector2.down * sink * .5f + Vector2.up * .35f * Fade(t, 0, .4f), pr * (1 - sink * .25f), alpha, Seed, now);
                for (int k = 0; k < 20; k++)
                {
                    float life = .6f + .6f * H(Seed, k), u = t / life; if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + Dir(Seed, k + 20, 20, .62f) * r * 2f * EaseOut(u) + Vector2.up * 2.6f * Mathf.Sin(u * Mathf.PI) * H(Seed, k + 7);
                    b.Quad(DKit.Star, p, .22f + .14f * H(Seed, k + 3), t * 500, A(k % 3 == 0 ? White : DKit.Purple, 1.2f * (1 - u * u)), 0, Seed + k, 5 + k % 3);
                }
            }
        }

        /// <summary>A hit that drags an enemy: short lavender streaks race into a tiny dark dimple with a swirl.</summary>
        private sealed class Pull : FxEffect
        {
            protected override float Life => .5f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                if (t < 0) return; Vector2 at = At; float r = Mathf.Max(.9f, Radius) * 1.15f, f = Fade(t, .15f, .5f);
                b.Floor(FxShape.Shade, at, r * .6f, A(DKit.Dark, .6f * f), 0, Seed);
                b.Quad(DKit.Hole, at, DKit.Sq(r * 1.3f), 0, A(DKit.Lavender, 1.1f * f), t * 2, Seed, 2);
                Inflow(b, at, r * 1.4f, t * 3, Seed, 7, DKit.Lavender, f);
            }
        }

        /// <summary>Step 8 weather: heavy gravity over the whole arena. The air turns violet, sagging bands of force press down across
        /// the floor, slow lensing ripples drift over the crowd and grit sinks out of the air towards the ground.</summary>
        private sealed class Heavy : FxEffect
        {
            protected override float Life => 1f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                if (Moment.Kind != BondMomentKind.Weather) return;
                float age = now - Born, on = Rise(age, 0, 2f) * Fade(now, End - 1.5f, End); Rect a = Arena; Vector2 c = a.center;
                b.Quad(FxShape.Shade, c, new Vector2(a.width * .75f, a.height * .75f), 0, A(new Color(.06f, .02f, .14f), .4f * on), 0, Seed);
                for (int k = 0; k < 9; k++) b.Quad(FxShape.Shade, new Vector2(a.xMin + (k + .5f) * a.width / 9, a.yMax + .4f), new Vector2(2.6f, 2.2f), 0, A(new Color(.05f, .01f, .1f), .8f * on), 0, Seed + k);
                // bands of force sag across the arena and creep downward
                for (int k = 0; k < 6; k++)
                {
                    float u = (k / 6f + age * .05f) % 1f, y = a.yMax - u * a.height, al = on * Mathf.Sin(u * Mathf.PI);
                    Vector2 l = new Vector2(a.xMin - .5f, y), r = new Vector2(a.xMax + .5f, y);
                    DKit.Chain(b, l, r, -.9f, .5f, 0, .09f, DKit.Purple, .55f * al, Seed + k * 9, 24, .5f);
                }
                // lensing ripples drift slowly over the floor
                for (int k = 0; k < 5; k++)
                {
                    float u = (age * .08f + k * .2f) % 1f; Vector2 p = new Vector2(a.xMin + a.width * ((H(Seed, k) + age * .015f * (k % 2 == 0 ? 1 : -1)) % 1f), a.yMin + a.height * (.2f + .6f * H(Seed, k + 7)));
                    b.Floor(DKit.Hole, p, 2.2f + 2f * u, A(DKit.Purple, .55f * on * Mathf.Sin(u * Mathf.PI)), age * .3f + k, Seed + k, 1);
                    b.Floor(FxShape.Shade, p, 1.2f * (1 - u * .5f), A(DKit.Dark, .3f * on * Mathf.Sin(u * Mathf.PI)), 0, Seed + k);
                }
                // grit sinks out of the air, streaking
                for (int k = 0; k < 60; k++)
                {
                    float sp = 1.6f + 1.4f * H(Seed, k + 300), u = (H(Seed, k) + age * sp / (a.height + 2)) % 1f;
                    Vector2 p = new Vector2(a.xMin + a.width * H(Seed, k + 150), a.yMax + 1 - u * (a.height + 2));
                    b.Beam(p + Vector2.up * .5f, p, .03f, A(DKit.Lavender, .35f * on), 0, k, 0);
                    b.Quad(DKit.Star, p, .12f + .08f * H(Seed, k + 9), age * 90 + k * 30, A(k % 4 == 0 ? White : DKit.Purple, .9f * on), 0, Seed + k, 5 + k % 3);
                }
            }
        }
    }
}
