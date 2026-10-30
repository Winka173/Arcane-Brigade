using System;
using System.Collections.Generic;
using UnityEngine;
using static ArcaneBrigade.FxKit;
using static ArcaneBrigade.AKit;

namespace ArcaneBrigade
{
    /// <summary>Ember bond, storybook fire (owner 29/10/2026: fire must match the painted toybox art): every flame is a cel-painted
    /// silhouette of licking tongues layered dark ink, red, orange and a yellow heart (shape 24, par 20+), smoke is painted puffs
    /// with an ink rim and a lit lobe, embers are small specks.</summary>
    public static class EmberFx
    {
        public static void Register(Dictionary<string, Func<FxEffect>> map)
        {
            map["burn-burst"] = () => new BurnBurst();
            map["inferno-burst"] = () => new InfernoBurst();
            map["ember-meteor"] = () => new Meteor();
            map["phoenix-dive"] = () => new Phoenix();
        }
        // flame layer tints run past 1: FxBatch paints a piece's solid body at .82 of its tint
        private static readonly Color Ink = new Color(.24f, .045f, .03f), Red = new Color(1.15f, .2f, .05f), Orange = new Color(1.22f, .58f, .07f),
            Yellow = new Color(1.22f, .96f, .2f), Soot = new Color(.06f, .03f, .02f), Warm = new Color(1f, .38f, .08f),
            SmokeInk = new Color(.1f, .06f, .06f), SmokeBody = new Color(.3f, .23f, .22f), SmokeLit = new Color(.48f, .4f, .36f), Rock = new Color(.2f, .11f, .08f);
        private const FxShape FlameS = (FxShape)24;
        private static readonly float[] LayerScale = { 1.24f, 1f, .7f, .42f };
        private static readonly Color[] LayerTint = { Ink, Red, Orange, Yellow };

        /// <summary>FxBatch draws pieces under 1.1 half-size at .72; this returns the size to ask for so the piece lands at <paramref name="s"/>
        /// (sizes between .79 and .95 snap to .79), so the layers of one flame keep their proportions.</summary>
        private static Vector2 Exact(Vector2 s)
        {
            float m = Mathf.Max(s.x, s.y);
            return s * (m < .79f ? 1 / .72f : m < .95f ? 1.0999f / m : m < 1.1f ? 1.1f / m : 1);
        }

        /// <summary>A storybook flame standing on <paramref name="baseP"/>: half-height h, half-width w, leaning by degrees (0 = up),
        /// tips 0 (one tongue), 1 (three) or 2 (five); painted ink edge, red, orange and a yellow heart.</summary>
        private static void Flame(FxBatch b, Vector2 baseP, float h, float w, float lean, int tips, float a, float q, int seed)
        {
            if (a <= .003f || h < .015f) return;
            Vector2 up = Vec(90 + lean);
            for (int L = 0; L < 4; L++)
            {
                float sc = LayerScale[L]; int tp = L == 3 ? 0 : L == 2 ? Mathf.Max(0, tips - 1) : tips;
                Vector2 c = baseP + up * h * (.16f + .84f * sc + (1 - sc) * .12f);
                b.Quad(FlameS, c, Exact(new Vector2(w * sc * (L >= 2 ? 1.08f : 1), h * sc)), lean, A(LayerTint[L], a * (L == 0 ? .9f : .7f)), q + L * .05f, seed, 20 + tp);
            }
        }

        /// <summary>A painted smoke puff: three round lobes with a dark ink rim, a soot body and a lit lobe on the upper left. It
        /// shrinks rather than going see-through as <paramref name="a"/> falls.</summary>
        private static void PaintPuff(FxBatch b, Vector2 p, float s, float a, int seed)
        {
            if (a <= .02f || s < .02f) return;
            float tw = (H(seed, 1) - .5f) * .5f;
            for (int pass = 0; pass < 3; pass++)
                for (int l = 0; l < 4; l++)
                {
                    float ls = s * (l == 0 ? 1 : l == 1 ? .66f : l == 2 ? .72f : .58f);
                    Vector2 lp = p + (l == 0 ? Vector2.zero : l == 3 ? new Vector2(.12f - tw, .5f) * s : new Vector2((l == 1 ? -.66f : .64f) + tw, -.26f + .08f * l) * s);
                    if (pass == 0) Solid(b, lp, new Vector2(ls * 1.14f + .03f, ls * 1.1f + .03f), 0, SmokeInk, a);
                    else if (pass == 1) Solid(b, lp + new Vector2(.04f, -.05f) * ls, new Vector2(ls, ls * .96f), 0, SmokeBody, a);
                    else Solid(b, lp + new Vector2(-.24f, .24f) * ls, new Vector2(ls * .5f, ls * .46f), 0, SmokeLit, a * .9f);
                }
        }

        /// <summary>A small ember speck on a gravity arc (negative g drifts up), a hot dot with a faint glow.</summary>
        private static void Speck(FxBatch b, Vector2 from, Vector2 vel, float g, float t, float life, float size, Color c, int seed)
        {
            float u = t / life; if (u <= 0 || u >= 1) return;
            Vector2 p = from + vel * t + Vector2.down * g * t * t * .5f + new Vector2(Mathf.Sin(t * 9 + seed) * .06f, 0); float al = 1 - u * u;
            b.Quad(FxShape.Disc, p, size * (1 - .4f * u), A(c, al), 0, seed);
            b.Quad(FxShape.Glow, p, size * 3f, A(Warm, .45f * al), 0, seed);
        }

        /// <summary>A burning enemy pops: a squat bulb of fire swells and stretches into one tall three-tongued flame, it bursts
        /// into six flamelets thrown out on arcs that land and burn out on the floor, painted soot puffs roll up, specks drift.</summary>
        private sealed class BurnBurst : FxEffect
        {
            protected override float Life => 1.3f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                float r = Mathf.Clamp(Radius, 1.2f, 2.4f), k = r / 2f; Vector2 at = At;
                b.Floor(FxShape.Shade, at, r * .9f, A(Soot, .55f * Fade(t, .5f, 1.3f) * Rise(t, 0, .1f)), 0, Seed);
                b.Floor(FxShape.Glow, at, r * 1.1f, A(Red, .45f * Fade(t, .1f, 1f)), 0, Seed);
                b.Quad(FxShape.Glow, at + Vector2.up * .3f * k, r * .6f, A(Orange, .5f * Fade(t, 0, .15f)), 0, Seed);
                for (int j = 0; j < 3; j++)
                {
                    float u = Rise(t, .45f + j * .12f, 1.25f + j * .03f); if (u <= 0 || u >= 1) continue;
                    PaintPuff(b, at + new Vector2((j == 1 ? -.35f : j == 2 ? .3f : 0) * k + Mathf.Sin(u * 4 + j) * .12f, (1.9f + .25f * j + 1.1f * u) * k), (.24f - .04f * j) * k * Mathf.Sin(Mathf.Min(1, u * 2.5f) * 1.57f) * (1 - u * u), 1, Seed + 40 + j);
                }
                // the main flame: squat swell, stretch, then sink
                float grow = Overshoot(Rise(t, 0, .26f)), sink = Rise(t, .45f, 1f);
                float h = 1.3f * k * grow * (1 - .85f * sink), w = .6f * k * (1.35f - .35f * Rise(t, .05f, .3f)) * Mathf.Min(1, grow * 1.6f) * (1 - .5f * sink);
                Flame(b, at + Vector2.down * .1f, h, w, Mathf.Sin(t * 8) * 6, 1, Fade(t, .85f, 1.05f), t * 1.4f, Seed);
                // flamelets thrown out at .22 s; each lands and burns out where it falls
                for (int j = 0; j < 6; j++)
                {
                    float side = j % 2 == 0 ? -1 : 1, tt = t - .22f - .02f * j; if (tt <= 0) continue;
                    Vector2 v = new Vector2(side * (1.3f + 1.6f * H(Seed, j + 10)) * k, 3.4f + 1.4f * H(Seed, j + 11)); const float g = 11;
                    Vector2 o = at + Vector2.up * .7f * k; float tl = 2 * v.y / g + .06f;
                    if (tt < tl)
                    {
                        Vector2 p = o + v * tt + Vector2.down * g * tt * tt * .5f, vel = v + Vector2.down * g * tt;
                        Flame(b, p, .24f * k, .13f * k, Ang(-vel) - 90, 0, 1, now * 1.5f, Seed + j * 7);
                    }
                    else
                    {
                        float u = (tt - tl) / .45f; if (u >= 1) continue;
                        Vector2 land = o + v * tl + Vector2.down * g * tl * tl * .5f;
                        Flame(b, land, .24f * k * Mathf.Sin(Mathf.Lerp(.5f, 3.14f, u)), .14f * k, side * 8, 0, 1, now * 1.6f, Seed + j * 7);
                        if (u < .3f) Grit(b, land, .2f, new Color(.25f, .15f, .1f), .5f * (1 - u / .3f), u, Seed + j);
                    }
                }
                for (int j = 0; j < 16; j++)
                    Speck(b, at + new Vector2((H(Seed, j + 20) - .5f) * r * .8f, .4f * k), new Vector2((H(Seed, j + 21) - .5f) * 1.4f, 1.6f + 1.2f * H(Seed, j + 22)), -.6f,
                        t - .12f - .03f * j, .75f + .3f * H(Seed, j + 23), .045f, j % 3 == 0 ? Yellow : Orange, Seed + j);
            }
        }

        /// <summary>Burning spreads: a fuse of fire runs once around the enemy, flame after flame leaning outward lighting up in a
        /// chasing ring, lava cracks open inside, then the ring closes with one five-tongued flare in the middle and soot puffs.</summary>
        private sealed class InfernoBurst : FxEffect
        {
            protected override float Life => 1.6f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                float r = Mathf.Clamp(Radius, 1.2f, 2.4f), k = r / 2f, R = .62f * r; Vector2 at = At; const int N = 12;
                b.Floor(FxShape.Shade, at, R * 1.35f, A(Soot, .5f * Rise(t, 0, .3f) * Fade(t, 1f, 1.6f)), 0, Seed);
                b.Floor(FxShape.Glow, at, R * 1.4f, A(Red, .4f * Rise(t, 0, .3f) * Fade(t, .8f, 1.5f)), 0, Seed);
                b.Quad(FxShape.Cracks, at, new Vector2(R * 1.05f, R * .65f), 0, A(Orange, .75f * Rise(t, .15f, .45f) * Fade(t, .9f, 1.6f)), 1, Seed, 2);
                for (int pass = 0; pass < 2; pass++) // the far half of the ring first
                    for (int j = 0; j < N; j++)
                    {
                        float th = j / (float)N * 6.2832f + 1.2f, s = Mathf.Sin(th); if (pass == 0 ? s < 0 : s >= 0) continue;
                        float t0 = .035f * j, u = t - t0; if (u <= 0 || u > 1f) continue;
                        Vector2 p = at + new Vector2(Mathf.Cos(th) * R, s * R * .6f);
                        float hh = .42f * k * Overshoot(Rise(u, 0, .16f)) * (1 - Rise(u, .6f, 1f)) * (.85f + .3f * H(Seed, j));
                        Flame(b, p, hh, .24f * k, Mathf.Cos(th) * -22 + Mathf.Sin(now * 9 + j) * 6, j % 3 == 0 ? 1 : 0, 1, now * 1.4f + j, Seed + j * 5);
                        if (u < .5f) for (int e = 0; e < 2; e++)
                            Speck(b, p + Vector2.up * .2f * k, new Vector2(Mathf.Cos(th) * (.8f + e * .6f), 2f + e), 7, u - .05f, .5f, .04f, e == 0 ? Yellow : Orange, Seed + j * 3 + e);
                        if (j % 3 == 1 && u > .45f) PaintPuff(b, p + Vector2.up * (.45f + .8f * (u - .45f)) * k, .14f * k * Mathf.Sin(Mathf.Clamp01((u - .45f) / .55f) * 3.14f), 1, Seed + j);
                    }
                float c = t - .44f;
                if (c > 0 && c < .9f)
                {
                    float hh = .62f * k * Overshoot(Rise(c, 0, .18f)) * (1 - Rise(c, .45f, .9f));
                    Flame(b, at + Vector2.down * .05f, hh, .42f * k, Mathf.Sin(now * 7) * 5, 2, 1, now * 1.3f, Seed + 99);
                    for (int e = 0; e < 10; e++)
                    {
                        Vector2 dd = Dir(Seed + 5, e, 10, .7f);
                        Speck(b, at + Vector2.up * .5f * k, new Vector2(dd.x * 2.4f, 2.6f + 1.5f * H(Seed, e + 70)), 8, c - .05f, .7f, .045f, e % 2 == 0 ? Yellow : Orange, Seed + e + 70);
                    }
                }
                if (c > .3f) for (int j = 0; j < 3; j++)
                {
                    float u = Rise(c, .3f + j * .08f, 1.1f + j * .05f); if (u <= 0 || u >= 1) continue;
                    PaintPuff(b, at + new Vector2((j - 1) * .3f * k, (1f + 1f * u) * k), .2f * k * Mathf.Sin(u * 3.14f) + .04f, 1, Seed + 50 + j);
                }
            }
        }

        /// <summary>A rock wrapped in a flaring five-tongued flame drops from the upper left trailing soot puffs onto a darkening
        /// glow; on impact a squat bonfire flares, burning chunks fly and land, a soot mushroom rolls up, and little flames keep
        /// licking out of the lava cracks it leaves.</summary>
        private sealed class Meteor : FxEffect
        {
            protected override float Life => 2.7f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                float r = Mathf.Clamp(Radius, 1.2f, 2.4f), k = r / 1.6f; Vector2 at = At;
                if (t < 0)
                {
                    float g = EaseOut(pre), flick = .8f + .2f * Mathf.Sin(now * 24);
                    b.Floor(FxShape.Shade, at, r * (.5f + .6f * g), A(Soot, .5f * g), 0, Seed);
                    b.Floor(FxShape.Glow, at, r * (.5f + .7f * g), A(Red, .5f * g * flick), 0, Seed);
                    float u = 1 - (-t) / .6f; if (u <= 0) return;
                    Vector2 start = at + new Vector2(-6.5f, 10f), p = Vector2.Lerp(start, at + Vector2.up * .2f, u * u), back = (start - at).normalized;
                    for (int j = 0; j < 7; j++)
                    {
                        float f = (j + (now * 6) % 1) / 7f; Vector2 q = p + back * (.6f + f * 3.2f) * k + new Vector2(Mathf.Sin(j * 5.3f), Mathf.Cos(j * 3.1f)) * .12f;
                        PaintPuff(b, q, (.12f + .2f * f) * k * (1 - f * f), 1 - f, Seed + j);
                    }
                    Flame(b, p - back * .28f * k, 1.15f * k, .5f * k, Ang(back) - 90 + Mathf.Sin(now * 30) * 3, 2, 1, now * 2.2f, Seed);
                    b.Quad(FxShape.Disc, p, .3f * k, A(Ink, 1), 0, Seed);
                    b.Quad(FxShape.Disc, p + new Vector2(.03f, -.02f), .25f * k, A(Rock, 1), 0, Seed + 1);
                    b.Quad(FxShape.Cracks, p, .26f * k, now * 90, A(Yellow, 1f), 1, Seed, 1);
                    for (int j = 0; j < 10; j++)
                    {
                        float f = ((now * 3 + H(Seed, j)) % 1); Vector2 q = p + back * (f * 2.2f) * k + new Vector2(H(Seed, j + 5) - .5f, H(Seed, j + 6) - .5f) * .7f * k;
                        b.Quad(FxShape.Disc, q, .045f, A(j % 2 == 0 ? Yellow : Orange, 1 - f), 0, Seed + j);
                    }
                    return;
                }
                float up = Overshoot(Rise(t, 0, .22f)), sink = Rise(t, .5f, 1.3f);
                b.Quad(FxShape.Glow, at + Vector2.up * .3f, r * 1.1f, A(Yellow, .55f * Fade(t, 0, .2f)), 0, Seed);
                float lava = Fade(t, 1.6f, 2.7f) * (.8f + .2f * Mathf.Sin(t * 7));
                b.Floor(FxShape.Shade, at, r * 1.1f, A(Soot, .6f * Fade(t, 2f, 2.7f)), 0, Seed);
                b.Floor(FxShape.Glow, at, r * 1.1f, A(Red, .5f * lava), 0, Seed);
                b.Quad(FxShape.Cracks, at, new Vector2(r * 1.05f, r * .68f), 0, A(Orange, lava), 1, Seed, 2);
                for (int j = 0; j < 12; j++)
                {
                    Vector2 dd = Dir(Seed + 6, j, 12, .62f); float u = EaseOut(Rise(t, 0, .7f));
                    Grit(b, at + dd * r * (.3f + .7f * u), r * (.22f + .15f * u), new Color(.4f, .3f, .22f), .5f * Fade(t, .3f, 1.2f), t + j, Seed + j);
                }
                // the soot mushroom: a stalk of puffs, a wider cap rolling out on top
                for (int j = 0; j < 7; j++)
                {
                    float u = Rise(t, .12f + j * .04f, 1.5f + j * .06f); if (u <= 0 || u >= 1) continue;
                    bool cap = j >= 4; float hgt = (cap ? 3f + .3f * (j % 2) : 1.3f + .45f * j) * k * EaseOut(u), sx = cap ? (j - 5) * .5f * k * EaseOut(u) : Mathf.Sin(j * 2.1f) * .15f;
                    PaintPuff(b, at + new Vector2(sx, hgt), (cap ? .34f : .24f) * k * Mathf.Sin(Mathf.Min(1, u * 1.6f) * 3.14f * .5f) * (1 - u * u), 1, Seed + 30 + j);
                }
                // the bonfire: squat flare that sinks into the lava
                Flame(b, at + Vector2.down * .1f, 1.4f * k * up * (1 - .8f * sink), 1f * k * (1.2f - .2f * Rise(t, 0, .3f)) * (1 - .4f * sink), Mathf.Sin(t * 7) * 4, 2, Fade(t, 1.2f, 1.35f), t * 1.3f, Seed);
                // burning chunks
                for (int j = 0; j < 8; j++)
                {
                    Vector2 dd = Dir(Seed + 8, j, 8, .6f), v = new Vector2(dd.x * (2.2f + 1.5f * H(Seed, j + 30)) * k, 3f + 2f * H(Seed, j + 31)); const float g = 12;
                    float tl = 2 * v.y / g + Mathf.Max(0, -dd.y) * .12f, tt = t - .04f;
                    if (tt <= 0) continue;
                    if (tt < tl)
                    {
                        Vector2 p = at + Vector2.up * .3f + v * tt + Vector2.down * g * tt * tt * .5f, vel = v + Vector2.down * g * tt;
                        Flame(b, p - vel.normalized * .02f, .13f, .08f, Ang(-vel) - 90, 0, 1, now * 2, Seed + j * 9);
                        b.Quad(FxShape.Disc, p, .11f, A(Ink, 1), 0, Seed + j);
                        b.Quad(FxShape.Disc, p, .09f, A(Rock, 1), 0, Seed + j);
                    }
                    else
                    {
                        float u = (tt - tl) / 1.4f; if (u >= 1) continue;
                        Vector2 land = at + Vector2.up * .3f + v * tl + Vector2.down * g * tl * tl * .5f;
                        b.Quad(FxShape.Disc, land, .11f, A(Ink, 1 - u * u), 0, Seed + j);
                        b.Quad(FxShape.Disc, land, .09f, A(Color.Lerp(Orange, Rock, u), 1 - u * u), 0, Seed + j);
                        Flame(b, land + Vector2.up * .04f, .15f * Mathf.Sin(Mathf.Lerp(.6f, 3.14f, u)), .09f, Mathf.Sin(now * 8 + j) * 8, 0, 1, now * 1.6f, Seed + j * 9);
                    }
                }
                // little flames licking out of the cracks afterwards
                for (int j = 0; j < 6; j++)
                {
                    float cyc = .55f + .25f * H(Seed, j + 50), ph = (t - .8f + H(Seed, j + 51) * cyc) / cyc; if (t < .8f || t > 2.6f) continue;
                    int n = Mathf.FloorToInt(ph); float u = ph - n; Vector2 p = at + Pt(Seed + n * 13, j + 52, r * .75f, r * .45f);
                    Flame(b, p, .2f * k * Mathf.Sin(u * 3.14f) * lava, .12f * k, Mathf.Sin(now * 6 + j) * 10, 0, 1, now * 1.5f, Seed + j + n * 17);
                    if (u > .6f) Speck(b, p + Vector2.up * .25f * k, new Vector2((H(Seed + n, j) - .5f), 1.2f), -.3f, u - .6f, .4f, .04f, Yellow, Seed + j + n);
                }
            }
        }

        /// <summary>A firebird made of painted flames sweeps along the line: a flame body, two beating wings of flame feathers, three
        /// streaming tail flames, a yellow head with an ink eye and a beak; behind it a row of flames pops up on the floor, soot puffs
        /// rise and little flame feathers flutter down.</summary>
        private sealed class Phoenix : FxEffect
        {
            protected override float Life => 2.6f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 d = At - From; float len = d.magnitude; if (len < .1f) return;
                Vector2 dir = d / len, nrm = new Vector2(-dir.y, dir.x); float ang = Ang(dir), s = Mathf.Max(1f, Radius) * 1.1f;
                if (t < 0)
                {
                    float g = EaseOut(pre);
                    for (int j = 0; j < 30; j++)
                    {
                        float u = H(Seed, j), rise = (pre * 1.5f + H(Seed, j + 60)) % 1; Vector2 p = From + dir * len * u + nrm * (H(Seed, j + 30) - .5f) * .8f + Vector2.up * rise * .8f;
                        b.Quad(FxShape.Disc, p, .04f, A(j % 3 == 0 ? Yellow : Orange, g * (1 - rise)), 0, Seed + j);
                        b.Quad(FxShape.Glow, p, .14f, A(Warm, .35f * g * (1 - rise)), 0, Seed + j);
                    }
                    return;
                }
                float fly = Rise(t, 0, 1.1f), pos = len * (-.1f + 1.2f * fly); Vector2 hp = From + dir * pos;
                // the floor fire it leaves
                for (int i = 0; i < 16; i++)
                {
                    float f = i / 15f, a = t - f * 1.1f; if (a < 0 || a > 1.1f) continue;
                    Vector2 bp = From + dir * len * (-.1f + 1.2f * f) + nrm * (H(Seed, i) - .5f) * .7f;
                    float hh = .44f * s * Overshoot(Rise(a, 0, .15f)) * (1 - Rise(a, .55f, 1.1f));
                    b.Floor(FxShape.Glow, bp, .7f * s, A(Red, .35f * (1 - a / 1.1f)), 0, Seed);
                    Flame(b, bp, hh, .24f * s, Mathf.Sin(now * 8 + i) * 8 - Mathf.Sign(dir.x) * 10, i % 4 == 0 ? 1 : 0, 1, now * 1.4f + i, Seed + i * 3);
                    if (i % 3 == 0 && a > .35f) { float u = (a - .35f) / .75f; PaintPuff(b, bp + Vector2.up * (.45f + .8f * u) * s, .15f * s * Mathf.Sin(u * 3.14f), 1, Seed + i); }
                    Speck(b, bp + Vector2.up * .2f, new Vector2((H(Seed, i + 40) - .5f) * 1.2f, 1.6f), -.4f, a - .1f, .7f, .04f, i % 2 == 0 ? Yellow : Orange, Seed + i + 40);
                }
                // flame feathers fluttering down
                for (int j = 0; j < 12; j++)
                {
                    float f = H(Seed, j + 5), a = t - f * 1.1f - .05f; if (a < 0 || a > .9f) continue; float u = a / .9f;
                    Vector2 p = From + dir * len * (-.1f + 1.2f * f) + nrm * (H(Seed, j + 80) - .5f) * .8f + Vector2.up * (.9f - 1.1f * u) * s + Vector2.right * Mathf.Sin(a * 9 + j) * .15f;
                    Flame(b, p, .11f * (1 - u * .5f), .07f, 180 + Mathf.Sin(a * 9 + j) * 35, 0, 1 - u * u, now * 2, Seed + j * 11);
                }
                if (fly >= 1) return;
                float vis = Rise(fly, 0, .06f) * Fade(fly, .9f, 1f), sc = s * vis, flap = Mathf.Sin(now * 14);
                Vector2 lift = Vector2.up * .55f * s; hp += lift;
                b.Floor(FxShape.Glow, hp - lift, sc * 1.5f, A(Orange, .4f * vis), 0, Seed);
                float bAng = Ang(-dir) - 90; // a flame whose tongues stream back along the flight
                // far (raised) wing, tail, body, near (lowered) wing, head
                float fx = Mathf.Sign(dir.x == 0 ? 1 : dir.x);
                for (int pass = 0; pass < 2; pass++)
                {
                    float wing = pass == 0 ? 1 : -1; // +1 raised wing (behind), -1 lowered wing (in front)
                    if (pass == 1)
                    {
                        for (int j = -1; j <= 1; j++)
                            Flame(b, hp - dir * .3f * sc, (.7f + .2f * (1 - Mathf.Abs(j))) * sc, .26f * sc, bAng + j * 24 + Mathf.Sin(now * 11 + j * 2) * 10, j == 0 ? 1 : 0, 1, now * 2 + j, Seed + 20 + j);
                        Flame(b, hp + dir * .62f * sc, .7f * sc, .62f * sc, bAng, 1, 1, now * 2.2f, Seed + 30);
                    }
                    float beat = wing > 0 ? .6f + .4f * flap : .6f - .4f * flap;
                    float wa = ang + fx * wing * (60 + 70 * beat);
                    Vector2 root = hp + dir * .1f * sc + Vector2.up * wing * .1f * sc;
                    Flame(b, root, (.95f - .2f * pass) * sc, .48f * sc, wa - 90, 1, 1, now * 2 + pass, Seed + 40 + pass);
                    Flame(b, root - dir * .15f * sc, (.7f - .15f * pass) * sc, .3f * sc, wa - 90 - fx * wing * 28, 0, 1, now * 2.3f + pass, Seed + 44 + pass);
                }
                Vector2 head = hp + dir * .5f * sc + Vector2.up * .12f * sc;
                for (int j = -1; j <= 1; j++) Flame(b, head - dir * .08f * sc + Vector2.up * .1f * sc, (.22f - .05f * Mathf.Abs(j)) * sc, .09f * sc, bAng + 30 * fx + j * 24, 0, 1, now * 2.5f, Seed + 50 + j);
                b.Quad(FxShape.Disc, head, .3f * sc, A(Ink, 1), 0, Seed);
                b.Quad(FxShape.Disc, head, .23f * sc, A(new Color(.62f, .2f, .03f), 1), 0, Seed + 1);
                b.Quad(FxShape.Disc, head + new Vector2(-.05f, .06f) * sc, .11f * sc, A(new Color(.72f, .55f, .1f), 1), 0, Seed + 4);
                b.Quad(Shard, head + dir * .28f * sc - Vector2.up * .03f * sc, new Vector2(.18f * sc, .09f * sc), ang, A(new Color(.75f, .58f, .12f), 1), 0, Seed + 2, 0);
                b.Quad(FxShape.Disc, head + dir * .08f * sc + Vector2.up * .05f * sc, .065f * sc, A(Ink, 1), 0, Seed + 3);
            }
        }
    }
}
