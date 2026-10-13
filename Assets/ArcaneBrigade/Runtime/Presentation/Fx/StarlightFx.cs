using System;
using System.Collections.Generic;
using UnityEngine;
using static ArcaneBrigade.FxKit;

namespace ArcaneBrigade
{
    /// <summary>Starlight bond: gold stars, comets, twinkle and shooting trails. falling-star = one tumbling star that bounces and sheds
    /// little stars; star-shower = thin meteor streaks landing in glitter fountains; star-fall = a slow vertical wish-star that floats
    /// back up as a lantern; star-comet = a huge comet that ends in a shock, star shards and a glowing constellation.</summary>
    public static class StarlightFx
    {
        // owner 29/10/2026: small glints and sparks were drawn far too big: small ones shrink, big set-piece flashes keep their size
        private static void Glint(FxBatch b, Vector2 at, float size, Color tint, float alpha, float turn = 0)
            => FxKit.Glint(b, at, size <= .08f ? size : size < .5f ? .08f + (size - .08f) * .45f : size - .231f, tint, alpha, turn);
        public static void Register(Dictionary<string, Func<FxEffect>> map)
        {
            map["falling-star"] = () => new FallingStar();
            map["star-shower"] = () => new Shower();
            map["star-fall"] = () => new StarFall();
            map["star-comet"] = () => new Comet();
            map["starry-night"] = () => new StarryNight();
        }

        private const FxShape Star = (FxShape)31, Streak = (FxShape)39;
        private static readonly Color Gold = new Color(1f, .8f, .22f), Pale = new Color(1f, .96f, .7f), Sky = new Color(.5f, .68f, 1f), Rose = new Color(1f, .6f, .85f);

        /// <summary>A comet streak whose bright head is at <paramref name="head"/>, trailing opposite to <paramref name="dir"/>.</summary>
        private static void Trail(FxBatch b, Vector2 head, Vector2 dir, float len, float width, Color c, int seed, float q)
        {
            Vector2 centre = head - dir * (.62f * len);
            b.Quad(Streak, centre, new Vector2(len, width), Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg, c, q, seed, 0);
        }
        /// <summary>A puff of pale star-dust and grey floor dust where something lands.</summary>
        private static void Puff(FxBatch b, Vector2 at, float r, float t, int seed, int n = 6)
        {
            if (t <= 0 || t >= 1) return;
            float e = EaseOut3(t);
            for (int k = 0; k < n; k++)
            {
                Vector2 d = Dir(seed, k, n, .5f);
                b.Quad(FxShape.Dust, at + d * r * (.3f + .7f * e) + Vector2.up * .2f * e, new Vector2(.5f + .45f * e, .3f + .22f * e), 0, A(new Color(.62f, .56f, .45f), .6f * (1 - t) * (1 - t)), t, seed + k);
                Glint(b, at + d * r * (.4f + .9f * e) + Vector2.up * (.5f * e), .14f, Pale, .9f * (1 - t), seed * 7 + k * 40);
            }
        }
        private static void StarAt(FxBatch b, Vector2 p, float size, float deg, Color c, float alpha, int seed, float points = 5)
            => b.Quad(Star, p, new Vector2(size, size), deg, A(c, alpha), 0, seed, points);

        /// <summary>Trait: a star drops out of the sky on a slant, hits the target, bounces twice spinning, and scatters little stars
        /// that twinkle on the floor before fading.</summary>
        private sealed class FallingStar : FxEffect
        {
            protected override float Life => 2.1f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Radius, tf = .32f, size = .5f + .22f * r;
                Vector2 start = at + new Vector2(-4.6f, 6.6f), dir = (at - start).normalized;
                if (t < tf)
                {
                    float u = t / tf; Vector2 head = Vector2.Lerp(start, at + Vector2.up * .3f, u);
                    Trail(b, head, dir, 5f, .6f, Gold, Seed, now * 4);
                    Trail(b, head, dir, 3.4f, .24f, White, Seed + 1, now * 4);
                    b.Quad(FxShape.Glow, head, size * 2.6f, A(Gold, .55f), 0, Seed);
                    StarAt(b, head, size, t * 700, Pale, 1.2f, Seed);
                    return;
                }
                float tb = t - tf, hop = 0, squash = 1;
                if (tb < .42f) { float u = tb / .42f; hop = 1.7f * 4 * u * (1 - u); squash = u < .12f ? .6f + 3.3f * u : 1; }
                else if (tb < .72f) { float u = (tb - .42f) / .3f; hop = .65f * 4 * u * (1 - u); }
                float rest = Fade(tb, 1.1f, 1.7f);
                Vector2 p = at + Vector2.up * (.3f + hop) + (tb > 1.1f ? Vector2.up * (tb - 1.1f) * 1.2f : Vector2.zero);
                b.Floor(FxShape.Glow, at, r * 1.3f, A(Gold, .45f * Fade(tb, 0, 1.4f)), 0, Seed);
                b.Quad(FxShape.Glow, at + Vector2.up * .5f, r * 1.7f, A(White, .75f * Fade(tb, 0, .22f)), 0, Seed);
                Puff(b, at, r * .9f, tb / .9f, Seed); Puff(b, at, r * .5f, (tb - .42f) / .6f, Seed + 3, 4);
                for (int k = 0; k < 6; k++) { float back = k * .05f; if (tb - back < 0 || tb > 1.1f) continue; Glint(b, p + new Vector2((H(Seed, k + 70) - .5f) * .5f, -.15f - k * .14f), .16f * (1 - k / 7f), k % 2 == 0 ? Gold : Rose, .9f * (1 - k / 7f) * rest, k * 30 + now * 120); }
                b.Quad(FxShape.Rays, at + Vector2.up * .3f, r * 1.7f, A(Gold, .75f * Fade(tb, 0, .6f)), tb, Seed, 3);
                b.Quad(FxShape.Glow, p, size * 2.4f, A(Gold, .5f * rest), 0, Seed);
                b.Quad(Star, p, new Vector2(size * (2 - squash), size * squash), tb * 380, A(Pale, 1.2f * rest), 0, Seed, 5);
                // scattered baby stars: each flies out on an arc, lands, and twinkles
                for (int k = 0; k < 9; k++)
                {
                    Vector2 d = Dir(Seed, k, 9, .75f); float fly = .55f + .2f * H(Seed, k + 3), u = tb / fly;
                    Vector2 land = at + d * r * (.7f + .65f * H(Seed, k + 9));
                    Vector2 q = u < 1 ? Vector2.Lerp(at, land, EaseOut(u)) + Vector2.up * (1.7f * u * (1 - u)) : land;
                    float twinkle = .65f + .35f * Mathf.Sin(now * 14 + k * 2), life = Fade(tb, 1.0f + .06f * k, 1.6f + .06f * k);
                    if (life <= 0) continue;
                    float s = (.2f + .12f * H(Seed, k + 12)) * (u < 1 ? 1 : twinkle);
                    b.Quad(FxShape.Glow, q, s * 2.2f, A(Gold, .4f * life), 0, Seed);
                    StarAt(b, q, s, tb * 300 * (u < 1 ? 1 : 0) + k * 24, k % 3 == 0 ? Rose : k % 3 == 1 ? Gold : Pale, 1.15f * life, Seed + k);
                    if (u >= 1) Glint(b, q, .34f * twinkle, White, .9f * life, now * 40 + k * 90);
                }
                for (int k = 0; k < 12; k++)
                {
                    float u = tb / (.5f + .5f * H(Seed, k + 40)); if (u <= 0 || u >= 1) continue;
                    Vector2 d = Dir(Seed + 3, k, 12, .8f);
                    Glint(b, at + d * r * 1.5f * EaseOut(u) + Vector2.up * (.8f * u - 1.4f * u * u + .3f), .22f, Gold, 1 - u, k * 50 + now * 120);
                }
            }
        }

        /// <summary>Step 6: three thin meteor streaks come down in a slanting rain, each landing in a fountain of glitter that throws
        /// four-point stars and gold dust.</summary>
        private sealed class Shower : FxEffect
        {
            protected override float Life => 1.7f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Radius, delay = Mathf.Max(.01f, Moment.Delay);
                Vector2 dir = new Vector2(-.3f, -1).normalized;
                for (int i = 0; i < 3; i++)
                {
                    float start = i * .12f, end = .76f + i * .12f, u = Mathf.Clamp01((pre - start) / (end - start));
                    Vector2 land = at + new Vector2((i - 1) * r * .85f, (i == 1 ? .25f : -.15f) * r), top = land - dir * 8f;
                    float ai = t + delay * (1 - end); // seconds since this streak landed
                    if (ai >= 0)
                    {
                        Puff(b, land, r * .6f, ai / .8f, Seed + i * 5, 5);
                        for (int k = 0; k < 4; k++) { float tw = Mathf.Pow(Mathf.Abs(Mathf.Sin(now * 9 + k * 1.9f + i)), 3); Vector2 g = land + new Vector2((H(Seed + i, k + 50) - .5f) * r * 1.2f, (H(Seed + i, k + 51) - .5f) * r * .4f); Glint(b, g, .3f * tw, k % 2 == 0 ? White : Gold, Rise(ai, .3f, .4f) * Fade(ai, .9f, 1.3f), k * 45); }
                    }
                    if (ai < 0 && u > 0)
                    {
                        Vector2 head = Vector2.Lerp(top, land, u * u);
                        Trail(b, head, dir, 4.2f, .36f, i == 1 ? Pale : Gold, Seed + i, now * 6);
                        Trail(b, head, dir, 2.8f, .14f, White, Seed + i + 9, now * 6);
                        b.Quad(FxShape.Glow, head, .65f, A(Pale, .7f), 0, Seed);
                        Glint(b, head, .55f, White, 1, now * 200 + i * 40);
                    }
                    if (ai < 0) continue;
                    b.Quad(FxShape.Glow, land + Vector2.up * .15f, r * 1.4f, A(Gold, .8f * Fade(ai, 0, .35f)), 0, Seed);
                    b.Quad(FxShape.Glow, land, r * .8f, A(White, .9f * Fade(ai, 0, .15f)), 0, Seed);
                    b.Floor(FxShape.Glow, land, r * 1.25f, A(Gold, .4f * Fade(ai, 0, .9f)), 0, Seed);
                    float pop = EaseOut(Rise(ai, 0, .22f)) * Fade(ai, .3f, .9f);
                    StarAt(b, land + Vector2.up * .35f, .55f * pop + .05f, ai * 260 + i * 30, Pale, 1.2f * Mathf.Clamp01(pop * 3), Seed + i, 4);
                    for (int k = 0; k < 12; k++)
                    {
                        float life = .55f + .4f * H(Seed + i, k), v = ai / life; if (v <= 0 || v >= 1) continue;
                        float spread = (H(Seed + i, k + 20) - .5f) * 2 * r * 1.1f;
                        Vector2 p = land + new Vector2(spread * EaseOut(v), 3.0f * H(Seed + i, k + 5) * v - 3.8f * v * v + .15f);
                        if (k % 3 == 0) StarAt(b, p, .22f * (1 - v * .5f), ai * 400 + k * 30, Gold, 1.1f * (1 - v * v), Seed + k, 4);
                        else Glint(b, p, .26f * (1 - v * .4f), k % 2 == 0 ? White : Gold, 1 - v * v, k * 40 + ai * 300);
                    }
                }
            }
        }

        /// <summary>Step 8 starry night: one big soft star sinks straight down on a spiral of dust, lands in a bloom of light and rays,
        /// then floats back up like a lantern, turning, while wish-stars rise after it.</summary>
        private sealed class StarFall : FxEffect
        {
            protected override float Life => 1.9f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Mathf.Max(1f, Radius), size = .95f;
                if (t < 0)
                {
                    float u = pre * pre * (3 - 2 * pre), spin = pre * 360 * 1.5f; Vector2 top = at + new Vector2(0, 8f), head = Vector2.Lerp(top, at + Vector2.up * .45f, u);
                    b.Beam(top, head, .16f, A(Pale, .32f), pre, Seed, 0);
                    for (int k = 0; k < 16; k++)
                    {
                        float back = k * .035f, uu = Mathf.Max(0, u - back); Vector2 pp = Vector2.Lerp(top, at + Vector2.up * .45f, uu);
                        float ang = k * .9f + now * 8, rad = .25f + k * .04f;
                        Glint(b, pp + new Vector2(Mathf.Cos(ang) * rad, Mathf.Sin(ang) * rad * .5f), .26f * (1 - k / 17f), k % 3 == 0 ? Rose : k % 3 == 1 ? Gold : Sky, .95f * (1 - k / 17f), k * 60 + now * 90);
                    }
                    b.Quad(FxShape.Glow, head, size * 3f, A(Gold, .5f), 0, Seed);
                    StarAt(b, head, size, spin, Pale, 1.2f, Seed);
                    b.Floor(FxShape.Glow, at, r * .9f * pre, A(Sky, .3f * pre), 0, Seed);
                    return;
                }
                float lift = EaseOut(Rise(t, .15f, 1.5f)), fade = Fade(t, 1.0f, 1.9f);
                b.Quad(FxShape.Glow, at + Vector2.up * .6f, r * 1.9f, A(White, .75f * Fade(t, 0, .3f)), 0, Seed);
                Puff(b, at, r * .9f, t / .9f, Seed, 8);
                b.Quad(FxShape.Rays, at + Vector2.up * 1.0f, r * 2.0f, A(Pale, .65f * fade), t * .5f, Seed, 5);
                b.Floor(FxShape.Glow, at, r * 1.5f, A(Gold, .45f * fade), 0, Seed);
                Vector2 p = at + Vector2.up * (.45f + 2.6f * lift);
                b.Quad(FxShape.Glow, p, size * 3.2f, A(Gold, .5f * fade), 0, Seed);
                StarAt(b, p, size * (1 - .35f * lift), t * 420, Pale, 1.2f * fade, Seed);
                for (int k = 0; k < 8; k++)
                {
                    float u = (t * 1.4f + k / 8f) % 1; if (t < .2f) continue;
                    Glint(b, p + new Vector2(Mathf.Sin(k * 2.3f + t * 3) * .35f, -.4f - u * 1.4f), .18f * (1 - u), k % 2 == 0 ? Gold : Sky, .95f * fade * (1 - u), k * 50 + now * 90);
                }
                for (int k = 0; k < 7; k++)
                {
                    float born = .1f + k * .12f, u = (t - born) / 1.1f; if (u <= 0 || u >= 1) continue;
                    float ang = k * 1.25f + u * 5; Vector2 q = at + new Vector2(Mathf.Cos(ang) * r * .75f * (1 - u * .4f), .2f + 3.2f * EaseOut(u) + Mathf.Sin(ang) * .3f);
                    float tw = .7f + .3f * Mathf.Sin(now * 15 + k);
                    b.Quad(FxShape.Glow, q, .5f, A(k % 2 == 0 ? Gold : Rose, .35f * (1 - u)), 0, Seed);
                    StarAt(b, q, (.2f + .1f * H(Seed, k)) * tw, now * 120 + k * 20, k % 2 == 0 ? Pale : Rose, 1.2f * (1 - u * u), Seed + k, 4);
                }
            }
        }

        /// <summary>Step 8: a giant comet streaks down from the corner of the sky trailing gold fire and star-dust, hits the crowd in a
        /// golden shock with cracked-light fissures and flying star shards, then a constellation of twinkling stars connects over the crowd.</summary>
        private sealed class Comet : FxEffect
        {
            protected override float Life => 2.7f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Radius;
                Vector2 start = at + new Vector2(-6.5f, 7.5f), dir = (at - start).normalized;
                if (t < 0)
                {
                    float u = Mathf.Pow(pre, 1.6f); Vector2 head = Vector2.Lerp(start, at, u);
                    b.Floor(FxShape.Shade, at, r * 1.2f * pre, A(new Color(.05f, .06f, .25f), .5f * pre), 0, Seed);
                    Trail(b, head, dir, 4.4f, 1.05f, new Color(1f, .6f, .15f), Seed, now * 5);
                    Trail(b, head - dir * 3.4f, dir, 4.2f, .7f, new Color(1f, .5f, .12f), Seed + 4, now * 5);
                    Trail(b, head, dir, 4.2f, .62f, Gold, Seed + 1, now * 5);
                    Trail(b, head, dir, 3.2f, .28f, White, Seed + 2, now * 5);
                    Trail(b, head + new Vector2(.2f, .4f), dir, 4.4f, 1.3f, new Color(.3f, .4f, .9f), Seed + 3, now * 3);
                    for (int k = 0; k < 5; k++) { float su = (now * 2.2f + k / 5f) % 1; b.Quad(FxShape.Smoke, head - dir * (1 + su * 4.5f) + Vector2.up * su * .6f, .35f + .6f * su, 0, A(new Color(.3f, .2f, .3f), .45f * (1 - su)), su, Seed + k); }
                    for (int k = 0; k < 20; k++)
                    {
                        float back = .6f + k * .42f; Vector2 perp = new Vector2(-dir.y, dir.x);
                        Vector2 p = head - dir * back + perp * Mathf.Sin(k * 2.7f + now * 9) * (.25f + k * .05f) + Vector2.down * k * .03f;
                        float fadeK = 1 - k / 21f; StarAt(b, p, .2f + .12f * H(Seed, k), now * 200 + k * 30, k % 2 == 0 ? Pale : Gold, 1.1f * fadeK, Seed + k, 4);
                    }
                    b.Quad(FxShape.Glow, head, 3.2f, A(Gold, .6f), 0, Seed);
                    b.Quad(FxShape.Glow, head, 1.5f, A(White, .9f), 0, Seed);
                    StarAt(b, head, 1.15f, now * 300, Pale, 1.2f, Seed, 8);
                    return;
                }
                float flash = Fade(t, 0, .3f), fade = Fade(t, 1.9f, 2.7f);
                b.Quad(FxShape.Glow, at + Vector2.up * .5f, r * 1.9f, A(White, .8f * flash), 0, Seed);
                SmokeColumn(b, at, r * .9f, 2.4f, t / 1.6f, new Color(.25f, .2f, .28f), Seed, 7);
                Puff(b, at, r * 1.1f, t / 1.1f, Seed + 2, 9);
                b.Quad(FxShape.Glow, at, r * 1.7f, A(Gold, .5f * Fade(t, 0, 1.2f)), 0, Seed);
                b.Floor(FxShape.Shock, at, r * 1.45f, A(Gold, .95f), Rise(t, 0, .85f), Seed);
                b.Floor(FxShape.Cracks, at, r * 1.25f, A(Gold, .85f * Fade(t, .3f, 1.9f)), Rise(t, 0, .5f), Seed, 1);
                b.Quad(FxShape.Rays, at + Vector2.up * .3f, r * 1.9f, A(Pale, .75f * Fade(t, 0, .7f)), t, Seed, 4);
                for (int k = 0; k < 14; k++)
                {
                    float life = .9f + .5f * H(Seed, k + 4), u = t / life; if (u <= 0 || u >= 1) continue;
                    Vector2 d = Dir(Seed, k, 14, .8f), p = at + d * r * 1.5f * EaseOut(u) + Vector2.up * (3.4f * H(Seed, k + 9) * u - 4.2f * u * u + .3f);
                    b.Quad(FxShape.Glow, p, .5f, A(Gold, .4f * (1 - u)), 0, Seed);
                    StarAt(b, p, .4f * (1 - u * .5f), t * 360 + k * 30, k % 3 == 0 ? White : Pale, 1.15f * (1 - u * u), Seed + k);
                }
                for (int k = 0; k < 20; k++)
                {
                    float u = (t * .6f + H(Seed, k)) % 1; if (t < .3f) continue;
                    Vector2 p = at + Dir(Seed + 5, k, 20, .7f) * r * (.2f + .9f * H(Seed, k + 7)) + Vector2.up * u * 1.8f;
                    Glint(b, p, .2f, Gold, .9f * fade * Mathf.Sin(u * Mathf.PI), now * 70 + k * 20);
                }
                // constellation: nodes appear one by one and are joined by thin gold lines
                int n = 8; var nodes = new Vector2[n];
                for (int k = 0; k < n; k++) nodes[k] = at + Dir(Seed + 11, k, n, .65f) * r * (.55f + .45f * H(Seed, k + 30)) + Vector2.up * .6f;
                for (int k = 0; k < n; k++)
                {
                    float on = Rise(t, .45f + k * .09f, .6f + k * .09f) * fade;
                    if (on <= 0) continue;
                    Vector2 nx = nodes[(k + 1) % n], to = Vector2.Lerp(nodes[k], nx, EaseOut(Rise(t, .5f + k * .09f, .75f + k * .09f)));
                    b.Link(nodes[k], to, .035f, A(Pale, .85f * on), Seed + k, .4f);
                    float tw = .7f + .3f * Mathf.Sin(now * 12 + k * 1.7f);
                    b.Quad(FxShape.Glow, nodes[k], .6f * tw, A(Gold, .45f * on), 0, Seed);
                    StarAt(b, nodes[k], .3f * tw * (1 + .5f * (k % 2)), now * 40 + k * 15, k % 2 == 0 ? Pale : Gold, 1.2f * on, Seed + k, 4);
                }
            }
        }

        /// <summary>Step 8 weather: the starry night. The arena sinks into deep blue, a milky way of soft light arcs across the sky, a field
        /// of gold and blue stars twinkles in and out, and every few seconds a shooting star scratches a bright line across the top.</summary>
        private sealed class StarryNight : FxEffect
        {
            protected override float Life => 3;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Rect a = Arena; float on = Rise(t, 0, 2.5f) * Fade(now, End - 1.5f, End);
                b.Quad(FxShape.Shade, a.center, new Vector2(a.width * .75f, a.height * .75f), 0, A(new Color(.02f, .04f, .16f), .3f * on), 0, Seed);
                // the milky way: a band of soft blue and rose glows across the upper arena, slowly drifting
                for (int k = 0; k < 9; k++)
                {
                    float x = a.xMin + a.width * (k + .5f) / 9f, y = a.center.y + a.height * (.18f + .14f * Mathf.Sin(k * .7f + 1)) + Mathf.Sin(now * .2f + k) * .3f;
                    b.Quad(FxShape.Glow, new Vector2(x, y), new Vector2(3.2f, 1.8f), -12, A(k % 3 == 0 ? Rose : Sky, .22f * on), 0, Seed);
                }
                for (int k = 0; k < 110; k++)
                {
                    Vector2 p = new Vector2(a.xMin + a.width * H(Seed, k), a.yMin + a.height * H(Seed, k + 200));
                    float tw = Mathf.Pow(.5f + .5f * Mathf.Sin(now * (1.5f + 2.5f * H(Seed, k + 300)) + k * 2.3f), 3);
                    Color c = k % 4 == 0 ? Sky : k % 4 == 1 ? Pale : Gold;
                    if (k % 5 == 0) StarAt(b, p, (.26f + .2f * H(Seed, k + 9)) * (.5f + .5f * tw), now * 20 + k * 30, c, 1.1f * on * (.35f + .65f * tw), Seed + k, 5);
                    else Glint(b, p, .2f + .32f * tw, c, on * (.25f + .75f * tw), k * 17);
                }
                // shooting stars: one every ~2.3 s, across the top half
                float cyc = now / 1.4f; int n = Mathf.FloorToInt(cyc); float u = (cyc - n) * 1.4f / .55f;
                if (u < 1)
                {
                    Vector2 s0 = new Vector2(a.xMin + a.width * (.1f + .5f * H(Seed, n)), a.yMax - 1 - 2.5f * H(Seed, n + 7)), dir = new Vector2(1, -.38f).normalized;
                    Vector2 head = s0 + dir * 12 * EaseOut(u);
                    Trail(b, head, dir, 4.6f, .42f, Gold, Seed + n, now * 6);
                    Trail(b, head, dir, 2.2f, .12f, White, Seed + n + 1, now * 6);
                    Glint(b, head, .5f, White, on * (1 - u * u), now * 300);
                    for (int k = 1; k <= 6; k++) { float bu = Mathf.Max(0, u - k * .06f); Glint(b, s0 + dir * 12 * EaseOut(bu) + Vector2.down * k * .12f, .18f * (1 - k / 7f), k % 2 == 0 ? Gold : Sky, on * (1 - k / 7f) * (1 - u), k * 50 + now * 100); }
                }
            }
        }
    }
}
