using System;
using System.Collections.Generic;
using UnityEngine;
using static ArcaneBrigade.FxKit;

namespace ArcaneBrigade
{
    /// <summary>Carnival bond: confetti, balloons, streamers, fireworks in bright mixed colours. carnival-confetti = a firework shell
    /// that bursts over the crowd and rains confetti and streamers; carnival-burst = a balloon inflated on the target that pops;
    /// grand-finale = the whole arena: fireworks, rising balloons, streamer cannons, sweeping spotlights and a confetti blizzard.</summary>
    public static class CarnivalFx
    {
        public static void Register(Dictionary<string, Func<FxEffect>> map)
        {
            map["carnival-confetti"] = () => new Confetti();
            map["carnival-burst"] = () => new BalloonPop();
            map["grand-finale"] = () => new Finale();
        }

        private const FxShape Balloon = (FxShape)30, Star = (FxShape)31, Ribbon = (FxShape)37, CardS = (FxShape)38, Streak = (FxShape)39;
        private static readonly Color[] Pal = { new Color(1f, .3f, .55f), new Color(.3f, .9f, 1f), new Color(1f, .9f, .25f), new Color(1f, .55f, .15f), new Color(.45f, 1f, .4f), new Color(.72f, .4f, 1f) };
        private static Color Pc(int i) => Pal[((i % 6) + 6) % 6];
        private static Vector2 Arc(Vector2 a, Vector2 b, float h, float u) => Vector2.Lerp(a, b, u) + Vector2.up * (4 * h * u * (1 - u));

        private static void Chip(FxBatch b, Vector2 p, float size, float deg, float flip, Color c, float alpha, int seed)
            => b.Quad(CardS, p, new Vector2(size * (.12f + Mathf.Abs(Mathf.Cos(flip))), size * .6f), deg, A(c, alpha), 0, seed, 0);
        private static void Streamer(FxBatch b, Vector2 p, float len, float width, float deg, Color c, float alpha, float wave, int seed)
            => b.Quad(Ribbon, p, new Vector2(len, width), deg, A(c, alpha), wave, seed, 0);

        /// <summary>Spark burst of a firework: coloured four-point stars with short tails flying out and sinking.</summary>
        private static void Firework(FxBatch b, Vector2 c, float radius, float u, int seed, int n, int hue)
        {
            if (u <= 0 || u >= 1) return;
            b.Quad(FxShape.Glow, c, radius * 1.1f, A(White, .8f * Fade(u, 0, .2f)), 0, seed);
            b.Quad(FxShape.Glow, c, radius * .9f, A(Pc(hue), .35f * (1 - u)), 0, seed);
            for (int k = 0; k < n; k++)
            {
                Vector2 d = Dir(seed, k, n, 1); float e = EaseOut(u);
                Vector2 p = c + d * radius * e + Vector2.down * radius * .35f * u * u, back = c + d * radius * EaseOut(Mathf.Max(0, u - .18f)) + Vector2.down * radius * .35f * Mathf.Pow(Mathf.Max(0, u - .18f), 2);
                Color col = Pc(hue + k % 3);
                b.Link(back, p, .06f, A(col, 1f - u), seed + k, .5f);
                b.Quad(Star, p, new Vector2(.22f, .22f), u * 300 + k * 20, A(k % 4 == 0 ? White : col, 1.2f * (1 - u * u)), 0, seed + k, 4);
            }
        }

        /// <summary>Grey-lilac gunpowder smoke left hanging where a firework burst.</summary>
        private static void Powder(FxBatch b, Vector2 c, float radius, float u, int seed, int n = 5)
        {
            if (u <= 0 || u >= 1) return;
            for (int k = 0; k < n; k++)
            {
                Vector2 d = Dir(seed, k, n, .8f);
                b.Quad(FxShape.Smoke, c + d * radius * (.25f + .3f * u) + Vector2.down * .4f * u, radius * (.3f + .25f * u), 0, A(new Color(.42f, .38f, .5f), .5f * Mathf.Sin(u * Mathf.PI) * (1 - u * .5f)), u, seed + k);
            }
        }

        /// <summary>Step 6 and 8: a firework shell arcs up from the party, bursts over the crowd in coloured sparks, and confetti, paper
        /// streamers and glittering chips rain down on the footprint.</summary>
        private sealed class Confetti : FxEffect
        {
            protected override float Life => 2.3f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At, burst = at + new Vector2(0, 2.5f); float r = Mathf.Max(1.2f, Radius);
                int hue = Mathf.FloorToInt(H(Seed, 1) * 6);
                if (t < 0)
                {
                    float u = pre; Vector2 p = Arc(From, burst, 2.2f, u), dir = (Arc(From, burst, 2.2f, Mathf.Min(1, u + .03f)) - p).normalized;
                    Trail(b, p, dir, 2.6f, .22f, Pc(hue), Seed, now * 5);
                    for (int k = 1; k <= 8; k++) { float bu = Mathf.Max(0, u - k * .03f); Glint(b, Arc(From, burst, 2.2f, bu), .24f * (1 - k / 9f), Pc(hue + k), 1 - k / 9f, k * 60 + now * 200); }
                    b.Quad(FxShape.Glow, p, .6f, A(White, .8f), 0, Seed);
                    return;
                }
                Firework(b, burst, 2.2f + r * .4f, t / .9f, Seed, 16, hue);
                Powder(b, burst, 2.2f, t / 1.8f, Seed + 50);
                // crossettes: three sparks split again into little pops (second beat)
                for (int k = 0; k < 3; k++) Firework(b, burst + Dir(Seed + 3, k, 3, 1) * (2.2f + r * .4f) * .75f + Vector2.down * .4f, .8f, (t - .45f - k * .06f) / .45f, Seed + k * 31, 7, hue + k + 2);
                for (int k = 0; k < 70; k++)
                {
                    float fall = .9f + .8f * H(Seed, k + 5), u = t / fall, keep = Fade(t, 1.5f + .4f * H(Seed, k + 9), 2.3f);
                    Vector2 land = at + new Vector2((H(Seed, k + 20) - .5f) * 2 * r * 1.35f, (H(Seed, k + 30) - .5f) * r * 1.0f - .2f);
                    Vector2 spread = burst + new Vector2((H(Seed, k + 20) - .5f) * 4f, 0);
                    float uu = Mathf.Clamp01(u), e = uu * uu;
                    Vector2 p = Vector2.Lerp(spread, land, Mathf.Pow(uu, 1.2f)) + new Vector2(Mathf.Sin(t * 9 + k) * .35f * (1 - uu), 1.1f * Mathf.Sin(uu * Mathf.PI) * (1 - uu));
                    if (uu >= 1) p = land;
                    Chip(b, p, .17f + .1f * H(Seed, k + 3), uu < 1 ? t * 360 + k * 47 : k * 47, uu < 1 ? t * 14 + k : 0, Pc(k + hue), 1.05f * keep, Seed + k);
                }
                for (int k = 0; k < 6; k++)
                {
                    float fall = 1.1f + .5f * H(Seed, k + 40), u = Mathf.Clamp01((t - .05f) / fall), keep = Fade(t, 1.3f + .1f * k, 2.1f);
                    Vector2 land = at + new Vector2((k - 2.5f) * r * .5f, (H(Seed, k + 50) - .5f) * r * .6f);
                    Vector2 p = Vector2.Lerp(burst + new Vector2((k - 2.5f) * .5f, 0), land, u * u * .6f + u * .4f);
                    Streamer(b, p + Vector2.down * .3f, 1.1f, .3f, -90 + Mathf.Sin(t * 6 + k) * 20, Pc(k + hue + 2), 1.0f * keep, t * 3 + k, Seed + k);
                }
                for (int k = 0; k < 4; k++) b.Floor(FxShape.Glow, at + new Vector2((k - 1.5f) * r * .7f, 0), r * .9f, A(Pc(k + hue), .28f * Rise(t, .5f, 1f) * Fade(t, 1.4f, 2.3f)), 0, Seed + k);
            }
            private static void Trail(FxBatch b, Vector2 head, Vector2 dir, float len, float width, Color c, int seed, float q)
                => b.Quad(Streak, head - dir * (.62f * len), new Vector2(len, width), Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg, A(c, 1), q, seed, 0);
        }

        /// <summary>Trait: a party balloon swells on the target, wobbling, then POPs in a spiky flash with torn rubber, confetti and streamers.</summary>
        private sealed class BalloonPop : FxEffect
        {
            protected override float Life => 1.6f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Mathf.Max(1f, Radius), popAt = .55f; int hue = Mathf.FloorToInt(H(Seed, 2) * 6);
                Vector2 c = at + Vector2.up * .8f;
                if (t < popAt)
                {
                    float g = EaseOut(t / popAt), s = (.3f + .8f * g) * (.8f + .25f * r), wob = 1 + .07f * Mathf.Sin(t * 40);
                    Vector2 sway = new Vector2(Mathf.Sin(t * 12) * .1f * g, 0);
                    b.Quad(FxShape.Glow, c, s * 2f, A(Pc(hue), .35f), 0, Seed);
                    b.Link(c + sway + Vector2.down * s * .85f, c + Vector2.down * (s * .85f + .9f) + new Vector2(Mathf.Sin(t * 9) * .25f, 0), .025f, A(Pal[2], .8f), Seed, 1);
                    b.Quad(Balloon, c + sway, new Vector2(s * wob, s * 1.1f / wob), Mathf.Sin(t * 14) * 6, A(Pc(hue), 1.25f), 0, Seed, 1);
                    return;
                }
                float u = t - popAt, off = Fade(u, .6f, 1.05f);
                // the gust of air from the pop and the limp string and knot dropping to the floor
                for (int k = 0; k < 6; k++) { float v = Rise(u, 0, .6f); if (v >= 1) break; b.Quad(FxShape.Dust, c + Dir(Seed + 2, k, 6, .7f) * r * (.4f + 1.0f * EaseOut3(v)), .45f + .4f * v, 0, A(new Color(.8f, .75f, .85f), .45f * (1 - v)), v, Seed + k); }
                { float v = Rise(u, 0, .7f); Vector2 knot = c + Vector2.down * (.9f + 1.4f * v * v); if (v < 1) { b.Link(knot, knot + new Vector2(Mathf.Sin(u * 14) * .3f * (1 - v), -.9f * (1 - .5f * v)), .025f, A(Pal[2], .8f * (1 - v)), Seed, 1); b.Quad((FxShape)32, knot, .12f, 180, A(Pc(hue), 1.1f * (1 - v)), 0, Seed); } }
                b.Quad(FxShape.Glow, c, r * 2.4f, A(White, .6f * Fade(u, 0, .16f)), 0, Seed);
                b.Quad(Star, c, new Vector2(r * 1.5f, r * 1.5f) * (.6f + .5f * EaseOut(u * 5)), u * 40, A(Pal[2], .9f * Fade(u, 0, .28f)), 0, Seed, 12);
                b.Quad(Star, c, new Vector2(r * 1.0f, r * 1.0f), -u * 60 + 10, A(Pc(hue), 1.2f * Fade(u, 0, .22f)), 0, Seed + 4, 9);
                for (int k = 0; k < 12; k++)
                {
                    Vector2 d = Dir(Seed, k, 12, .85f); float e = EaseOut(Rise(u, 0, .22f));
                    b.Link(c + d * r * .35f * e, c + d * r * (.7f + .8f * e), .05f, A(Pal[2], 1f * Fade(u, 0, .25f)), Seed + k, .6f);
                }
                for (int k = 0; k < 14; k++)
                {
                    Vector2 d = Dir(Seed + 5, k, 14, .85f); float life = .6f + .3f * H(Seed, k), v = u / life; if (v <= 0 || v >= 1) continue;
                    Vector2 p = c + d * r * 1.5f * EaseOut(v) + Vector2.up * (.8f * v) + Vector2.down * 2.2f * v * v;
                    Chip(b, p, .24f, v * 700 + k * 30, v * 20 + k, Pc(hue), 1.1f * (1 - v * v), Seed + k);
                }
                for (int k = 0; k < 30; k++)
                {
                    float v = (u - .03f * (k % 6)) / (.85f + .5f * H(Seed, k + 9)); if (v <= 0 || v >= 1) continue;
                    Vector2 d = Dir(Seed + 9, k, 30, .85f);
                    Vector2 p = c + d * r * (.5f + 1.1f * H(Seed, k + 20)) * EaseOut(Mathf.Min(1, v * 2)) + Vector2.down * 1.9f * v * v + new Vector2(Mathf.Sin(v * 10 + k) * .15f, 0);
                    Chip(b, p, .15f + .08f * H(Seed, k), u * 300 + k * 51, u * 12 + k, Pc(k), 1.1f * (1 - v * v * v), Seed + k);
                }
                for (int k = 0; k < 4; k++)
                    Streamer(b, c + new Vector2((k - 1.5f) * .6f * r, -.2f - u * 1.6f), 1f, .28f, -90 + (k - 1.5f) * 25 + Mathf.Sin(u * 8 + k) * 12, Pc(hue + k + 1), 1f * off, u * 3 + k, Seed + k);
            }
        }

        /// <summary>Step 8 finale. For 1.5 seconds the whole arena is a parade: fireworks across the sky, a bunch of balloons racing up
        /// on strings, streamer cannons from both corners, coloured spotlights sweeping and a confetti blizzard.</summary>
        private sealed class Finale : FxEffect
        {
            protected override float Life => 1.5f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Rect a = Arena; float dur = Mathf.Max(.5f, Moment.Duration), g = Fade(t, dur * .7f, dur), rise = Rise(t, 0, .12f);
                b.Quad(FxShape.Shade, a.center, new Vector2(a.width * .75f, a.height * .75f), 0, A(new Color(.1f, .02f, .15f), .15f * rise * g), 0, Seed);
                // spotlights from the floor edge, sweeping
                for (int k = 0; k < 3; k++)
                {
                    Vector2 from = new Vector2(a.xMin + a.width * (.2f + .3f * k), a.yMin - 1), to = new Vector2(from.x + Mathf.Sin(now * 4 + k * 2) * 5, a.yMax + 1);
                    b.Beam(from, to, .85f, A(Pc(k * 2 + 1), .2f * rise * g), now, Seed + k, 0);
                    b.Beam(from, to, .25f, A(White, .14f * rise * g), now, Seed + k, 0);
                }
                // fireworks
                for (int k = 0; k < 7; k++)
                {
                    float start = k * .1f; Vector2 c = new Vector2(a.xMin + a.width * (.1f + .8f * H(Seed, k + 3)), a.center.y + a.height * (.02f + .36f * H(Seed, k + 8)));
                    float u = (t - start) / .85f;
                    if (u > 0 && u < .12f) { b.Quad(Streak, c + Vector2.down * 1.5f * (1 - u / .12f), new Vector2(1.6f, .2f), 90, A(Pc(k), 1), now, Seed + k, 0); }
                    Firework(b, c, 2.0f + .7f * H(Seed, k + 12), (u - .12f) / .88f, Seed + k * 13, 16, k);
                    Powder(b, c, 1.8f, (u - .15f) / 1.2f, Seed + k * 7, 4);
                }
                // balloons rising on strings
                for (int k = 0; k < 13; k++)
                {
                    float sp = 8 + 4 * H(Seed, k + 40), delay = .15f * H(Seed, k + 41);
                    Vector2 p = new Vector2(a.xMin + a.width * (.04f + .92f * H(Seed, k)) + Mathf.Sin(t * 7 + k) * .35f, a.yMin - 1.6f + Mathf.Max(0, t - delay) * sp);
                    if (p.y > a.yMax + 2) continue; float s = .7f + .35f * H(Seed, k + 45);
                    b.Link(p + Vector2.down * s * .85f, p + new Vector2(Mathf.Sin(t * 9 + k) * .35f, -s * .85f - 1.8f), .03f, A(White, .7f * g), Seed + k, 1);
                    b.Quad(Balloon, p, new Vector2(s, s * 1.1f), Mathf.Sin(t * 8 + k) * 8, A(Pc(k), 1.25f * g), 0, Seed + k, 1);
                }
                // streamer cannons from both bottom corners
                for (int side = 0; side < 2; side++)
                    for (int k = 0; k < 7; k++)
                    {
                        Vector2 origin = new Vector2(side == 0 ? a.xMin + .5f : a.xMax - .5f, a.yMin + .3f), v = new Vector2((side == 0 ? 1 : -1) * (3.5f + 3 * H(Seed, k + 60 + side)), 9 + 4 * H(Seed, k + 70 + side));
                        Vector2 p = origin + v * t + Vector2.down * 5.5f * t * t;
                        Vector2 vel = v + Vector2.down * 11f * t;
                        Streamer(b, p, .9f, .26f, Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg, Pc(k + side * 3), 1.05f * g, t * 4 + k, Seed + k + side * 9);
                    }
                // confetti blizzard
                for (int k = 0; k < 70; k++)
                {
                    float sp = 7 + 4 * H(Seed, k + 80), y0 = a.yMax + 1 - (H(Seed, k + 90) * 4) - t * sp; if (y0 < a.yMin - 1) continue;
                    Vector2 p = new Vector2(a.xMin + a.width * H(Seed, k + 100) + Mathf.Sin(t * 9 + k) * .4f, y0);
                    Chip(b, p, .17f + .1f * H(Seed, k), t * 400 + k * 33, t * 14 + k, Pc(k), 1.05f * g * rise, Seed + k);
                }
            }
        }
    }
}
