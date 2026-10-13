using System;
using System.Collections.Generic;
using UnityEngine;
using static ArcaneBrigade.FxKit;
using static ArcaneBrigade.AKit;

namespace ArcaneBrigade
{
    /// <summary>Frost bond: crystals, shards, snow, white-blue light.</summary>
    public static class FrostFx
    {
        // owner 29/10/2026: small glints and sparks were drawn far too big: small ones shrink, big set-piece flashes keep their size
        private static void Glint(FxBatch b, Vector2 at, float size, Color tint, float alpha, float turn = 0)
            => FxKit.Glint(b, at, size <= .08f ? size : size < .5f ? .08f + (size - .08f) * .45f : size - .231f, tint, alpha, turn);
        public static void Register(Dictionary<string, Func<FxEffect>> map)
        {
            map["freeze"] = () => new Freeze();
            map["frozen-hit"] = () => new FrozenHit();
            map["shatter"] = () => new Shatter();
            map["frost-ice-spikes"] = () => new IceSpikes();
            map["frost-blizzard-hail"] = () => new Hail();
            map["frost-blizzard"] = () => new Blizzard();
        }
        // owner 29/10/2026: crowds of frost hits burned to white; the ice keeps its blue, white only in tiny glints
        private static readonly Color Ice = new Color(.32f, .68f, 1f), Deep = new Color(.15f, .36f, .95f), Snow = new Color(.66f, .86f, 1f);

        private static void Chip(FxBatch b, Vector2 from, Vector2 vel, float g, float t, float life, float size, Color c, int seed)
        {
            float u = t / life; if (u <= 0 || u >= 1) return;
            Vector2 p = from + vel * t + Vector2.down * g * t * t * .5f, v = vel + Vector2.down * g * t;
            b.Quad(Shard, p, new Vector2(size, size * .42f), Ang(v) + t * 160 * (seed % 2 == 0 ? 1 : -1), A(c, 1 - u * u), 0, seed);
            Glint(b, p, size * .5f, Snow, .7f * (1 - u), t * 300 + seed);
        }

        /// <summary>An enemy is frozen: seven crystals grow up out of the floor around it and lean in like a cage, a snowflake spins over its head, rime spreads on the floor.</summary>
        private sealed class Freeze : FxEffect
        {
            protected override float Life => 1.7f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                float r = Mathf.Clamp(Radius, 1.2f, 2.4f), grow = Overshoot(Rise(t, 0, .32f)), o = Fade(t, 1.1f, 1.7f); Vector2 at = At;
                b.Floor(FxShape.Glow, at, r * 1.5f, A(Deep, .5f * o), 0, Seed);
                b.Floor(Flake, at, r * 1.3f * EaseOut(Rise(t, 0, .3f)), A(Ice, .6f * o), 0, Seed);
                b.Floor(FxShape.Cracks, at, r * 1.15f, A(Snow, (.7f + .6f * Fade(t, .42f, .7f) * Rise(t, .38f, .42f)) * o), 1, Seed, 1);
                b.Quad(FxShape.Glow, at + Vector2.up * .3f, r * 1.4f, A(Snow, .5f * Fade(t, 0, .15f)), 0, Seed);
                Grit(b, at, r * 1.1f, new Color(.8f, .92f, 1f), .4f * Rise(t, 0, .3f) * o, t, Seed + 2);
                for (int k = 0; k < 3; k++) Puff(b, at + new Vector2((k - 1) * r * .45f, .1f + .3f * t), r * (.4f + .2f * t), new Color(.75f, .88f, 1f), .3f * Rise(t, .05f, .3f) * o, t + k, Seed + 30 + k);
                for (int k = 0; k < 8; k++)
                {
                    Vector2 dd = Dir(Seed + 9, k, 8, .6f);
                    Fleck(b, at + dd * r * .6f + Vector2.up * .8f, new Vector2(dd.x * 2, 2.2f + H(Seed, k + 70)), 8, t - .4f, .55f, .07f, k % 2 == 0 ? Snow : Ice, Seed + k + 70);
                }
                for (int k = 0; k < 8; k++)
                {
                    Vector2 dd = Dir(Seed, k, 8, .55f), bp = at + dd * r * .62f;
                    float ang = 90 - Mathf.Sign(dd.x) * (14 + 10 * H(Seed, k + 5)), len = (1.5f + 1f * H(Seed, k + 3)) * Mathf.Min(1.2f, r) * grow;
                    Vector2 c = bp + Vec(ang) * len * .42f;
                    b.Quad(Glow(), bp, .55f, A(Ice, .5f * o), 0, Seed);
                    b.Quad(Shard, c, new Vector2(len * .55f, len * .25f), ang, A(k % 2 == 0 ? Ice : Deep, .95f * o), 0, Seed + k);
                    b.Quad(Shard, c + Vec(ang) * len * .05f, new Vector2(len * .4f, len * .14f), ang, A(Snow, .6f * o), 0, Seed + k + 9);
                }
                b.Quad(Flake, at + Vector2.up * (1.1f * grow), new Vector2(r * .85f * grow, r * .85f * grow), t * 40, A(Snow, .9f * o), 0, Seed);
                b.Quad(FxShape.Glow, at + Vector2.up * 1.1f, r * 1.1f * grow, A(Ice, .35f * o), 0, Seed);
                for (int k = 0; k < 12; k++)
                {
                    float u = (t * 1.2f + H(Seed, k)) % 1; Vector2 p = at + Dir(Seed, k, 12, .6f) * r * .9f + Vector2.up * (.2f + u * 1.8f);
                    Glint(b, p, .22f, Snow, .85f * o * (1 - u), k * 30 + t * 120);
                }
            }
            private static FxShape Glow() => FxShape.Glow;
        }

        /// <summary>A hammer blow on a frozen enemy: a white flash and a four-point starburst, glass cracks racing across the floor, nine chips of ice flung away spinning.</summary>
        private sealed class FrozenHit : FxEffect
        {
            protected override float Life => 1.0f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                float r = Mathf.Clamp(Radius, 1f, 1.9f); Vector2 at = At;
                b.Quad(FxShape.Glow, at, r * 1.6f, A(Snow, .6f * Fade(t, 0, .1f)), 0, Seed);
                b.Quad(FxShape.Glow, at, r * 1.5f, A(Ice, .4f * Fade(t, 0, .6f)), 0, Seed);
                Glint(b, at, r * 1.7f, Snow, Fade(t, 0, .35f), 45);
                Glint(b, at, r * 1.2f, Snow, Fade(t, 0, .25f), 0);
                b.Quad(FxShape.Cracks, at, new Vector2(r * 1.6f, r * 1f), 0, A(Snow, (.8f + .7f * Fade(t, .3f, .5f) * Rise(t, .22f, .3f)) * Fade(t, .2f, .95f)), 1, Seed, 1);
                Puff(b, at + Vector2.up * (.2f + .5f * t), r * (.45f + .5f * t), new Color(.88f, .95f, 1f), .45f * Fade(t, .05f, .7f), t, Seed + 5);
                Grit(b, at, r * (.7f + .5f * t), new Color(.85f, .93f, 1f), .4f * Fade(t, .3f, 1f), t, Seed + 6);
                for (int k = 0; k < 12; k++)
                {
                    Vector2 dd = Dir(Seed + 4, k, 12, .7f);
                    Fleck(b, at + Vector2.up * .3f, new Vector2(dd.x * (2.5f + 2 * H(Seed, k + 40)), 1.5f + 3 * H(Seed, k + 41)), 9, t - .02f, .5f, .06f, k % 2 == 0 ? Snow : Ice, Seed + k + 40);
                }
                b.Floor(FxShape.Glow, at, r * 1.4f, A(Deep, .5f * Fade(t, 0, .8f)), 0, Seed);
                for (int k = 0; k < 20; k++)
                {
                    Vector2 dd = Dir(Seed, k, 20, .6f);
                    Chip(b, at + Vector2.up * .2f, new Vector2(dd.x * (3 + 2 * H(Seed, k)), 2.5f + 3f * H(Seed, k + 7) + dd.y * 2), 11, t, .75f, .13f + .08f * H(Seed, k + 3), k % 2 == 0 ? Ice : Snow, Seed + k);
                }
                b.Quad(FxShape.Smoke, at + Vector2.up * .3f, r * (.7f + t), A(new Color(.75f, .88f, 1f), .35f * Fade(t, 0, .8f)), t, Seed);
            }
        }

        /// <summary>The frozen enemy bursts: a fountain of twenty-four big shards flung high and falling, a snowflake bloom on the floor, a spreading frost mist and flakes drifting up.</summary>
        private sealed class Shatter : FxEffect
        {
            protected override float Life => 1.8f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                float r = Mathf.Clamp(Radius, 1.4f, 3.2f); Vector2 at = At;
                b.Quad(FxShape.Glow, at + Vector2.up * .4f, r * 1.7f, A(Snow, .6f * Fade(t, 0, .12f)), 0, Seed);
                b.Quad(FxShape.Glow, at + Vector2.up * .4f, r * 1.5f, A(Ice, .4f * Fade(t, 0, .8f)), 0, Seed);
                for (int k = 0; k < 12; k++)
                {
                    float land = .55f + .6f * H(Seed, k + 50), a = (t - land) / .55f; if (a <= 0 || a >= 1) continue;
                    Vector2 p = at + Pt(Seed, k + 50, r * 1.15f, r * .7f);
                    Grit(b, p, .35f + .35f * a, new Color(.85f, .93f, 1f), .5f * (1 - a), a, Seed + k);
                    Glint(b, p + Vector2.up * .1f, .28f * (1 - a), Snow, 1 - a, k * 40);
                }
                for (int k = 0; k < 16; k++)
                {
                    float u = Rise(t, .3f + H(Seed, k + 60) * .3f, 1.7f); if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + new Vector2((H(Seed, k + 61) - .5f) * r * 2f + Mathf.Sin(t * 3 + k) * .3f, 2.6f - u * 2.6f);
                    Glint(b, p, .12f, Snow, .9f * (1 - u), t * 300 + k * 30);
                }
                b.Floor(Flake, at, r * 1.45f * EaseOut(Rise(t, 0, .45f)), A(Ice, .75f * Fade(t, .5f, 1.7f)), 0, Seed);
                b.Floor(FxShape.Glow, at, r * 1.6f, A(Deep, .5f * Fade(t, 0, 1.5f)), 0, Seed);
                b.Floor(FxShape.Smoke, at, r * (.5f + .9f * EaseOut(Rise(t, 0, .8f))), A(new Color(.7f, .85f, 1f), .5f * Fade(t, .1f, 1.4f)), t, Seed);
                for (int k = 0; k < 40; k++)
                {
                    Vector2 dd = Dir(Seed, k, 40, .55f);
                    Chip(b, at + Vector2.up * .3f, new Vector2(dd.x * r * (1.6f + 1.2f * H(Seed, k)), 3f + 4.5f * H(Seed, k + 7) + dd.y * 2), 12, t, 1.15f + .4f * H(Seed, k + 2), .1f + .08f * H(Seed, k + 3), k % 3 == 0 ? Snow : (k % 3 == 1 ? Ice : Deep), Seed + k);
                }
                for (int k = 0; k < 8; k++)
                {
                    float u = Rise(t, .05f + .04f * k, 1.5f); if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + new Vector2((k - 3.5f) * r * .3f, .5f + u * (1.4f + .5f * H(Seed, k)));
                    b.Quad(Flake, p, .2f + .08f * H(Seed, k + 4), t * 70 * (k % 2 == 0 ? 1 : -1), A(Snow, .8f * (1 - u)), 0, Seed + k, 0);
                }
            }
        }

        /// <summary>Step 6. Frost races across the ground, then eighteen tall ice spikes erupt in a wave from the middle of the crowd, each with a lighter facet and a glow at its foot, and crumble into glints.</summary>
        private sealed class IceSpikes : FxEffect
        {
            protected override float Life => 2.3f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                float r = Radius; Vector2 at = At;
                if (t < 0)
                {
                    float g = EaseOut(pre);
                    b.Floor(FxShape.Glow, at, r * 1.3f, A(Deep, .45f * g), 0, Seed);
                    b.Floor(FxShape.Cracks, at, r * 1.15f * g, A(Snow, .85f * g), 1, Seed, 2);
                    b.Floor(Flake, at, r * 1.1f * g, A(Ice, .35f * g), 0, Seed);
                    for (int k = 0; k < 10; k++) Glint(b, at + Pt(Seed, k, r, r * .6f), .22f, Snow, .8f * g * (.5f + .5f * Mathf.Sin(now * 14 + k)), k * 40 + now * 90);
                    return;
                }
                float o = Fade(t, 1.3f, 2.2f);
                b.Floor(FxShape.Glow, at, r * 1.4f, A(Deep, .5f * o), 0, Seed);
                b.Floor(FxShape.Cracks, at, r * 1.15f, A(Snow, .8f * o), 1, Seed, 2);
                b.Floor(Flake, at, r * 1.15f, A(Ice, .3f * o), 0, Seed);
                b.Quad(FxShape.Glow, at + Vector2.up * 1f, r * 1.7f, A(Snow, .8f * Fade(t, 0, .2f)), 0, Seed);
                for (int k = 0; k < 18; k++)
                {
                    Vector2 off = Pt(Seed, k, r * .95f, r * .62f), bp = at + off; float d01 = Mathf.Clamp01(off.magnitude / r);
                    float ti = d01 * .24f, grow = Overshoot(Rise(t, ti, ti + .15f)); if (grow <= 0) continue;
                    float h = (1.5f + 1.7f * H(Seed, k + 5)) * (1.15f - .35f * d01) * grow, ang = 90 + (H(Seed, k + 9) - .5f) * 26;
                    Vector2 dir = Vec(ang), c = bp + dir * h * .42f;
                    b.Quad(FxShape.Glow, bp, .85f, A(Ice, .55f * o), 0, Seed);
                    b.Quad(Shard, c, new Vector2(h * .55f, h * .24f), ang, A(k % 3 == 0 ? Deep : Ice, .95f * o), 0, Seed + k);
                    b.Quad(Shard, c + dir * h * .06f, new Vector2(h * .42f, h * .14f), ang, A(Snow, .6f * o), 0, Seed + k + 40);
                    if (t > 1.3f + ti) Glint(b, bp + dir * h * .8f, .3f, Snow, Fade(t, 1.3f + ti, 2.2f), t * 200 + k * 30);
                    b.Quad(FxShape.Smoke, bp, .55f, A(new Color(.7f, .85f, 1f), .35f * Fade(t, ti, ti + 1f)), t, Seed + k);
                    Grit(b, bp, .5f + .4f * Rise(t, ti, ti + .5f), new Color(.85f, .93f, 1f), .45f * Fade(t, ti + .2f, ti + 1.1f), t, Seed + k + 3);
                    if (k % 2 == 0) for (int j = 0; j < 2; j++) Fleck(b, bp + Vector2.up * .2f, new Vector2((j - .5f) * 2.4f, 2.4f + H(Seed, k + j) * 1.5f), 9, t - ti, .5f, .06f, Snow, Seed + k * 3 + j);
                    if (k % 3 == 0) for (int j = 0; j < 3; j++) Chip(b, bp + dir * h * (.3f + .25f * j), new Vector2((H(Seed, k * 3 + j) - .5f) * 2, .5f), 9, t - 1.35f - ti, .6f, .1f, j == 1 ? Snow : Ice, Seed + k * 7 + j);
                }
            }
        }

        /// <summary>Step 8. Fourteen hailstones plunge slanted out of the sky onto the footprint, each with a white streak, and burst on the ground into a puff of snow, a frost splat and chips of ice.</summary>
        private sealed class Hail : FxEffect
        {
            protected override float Life => 1.5f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                float r = Mathf.Max(1.6f, Radius); Vector2 at = At, slant = new Vector2(-3.2f, 8f);
                if (t < 0) b.Floor(FxShape.Glow, at, r * 1.2f, A(Deep, .35f * EaseOut(pre)), 0, Seed);
                for (int k = 0; k < 15; k++)
                {
                    Vector2 target = at + Pt(Seed, k, r * 1.05f, r * .65f); float li = -.15f + .65f * H(Seed, k + 9), fall = .4f, tt = (t - (li - fall)) / fall;
                    float size = (k == 14 ? .75f : .3f + .2f * H(Seed, k + 2));
                    if (tt < 0) continue;
                    if (tt < 1)
                    {
                        Vector2 p = Vector2.Lerp(target + slant, target, tt * tt * .4f + tt * .6f);
                        b.Link(p, p + slant.normalized * 1.8f, size * .2f, A(Snow, .7f), Seed + k, 1);
                        b.Quad(FxShape.Glow, p, size * 2.2f, A(Ice, .55f), 0, Seed);
                        for (int f = 0; f < 3; f++) b.Quad(Shard, p, new Vector2(size * .5f, size * .22f), tt * 200 + k * 30 + f * 60, A(f == 1 ? Snow : Ice, .95f), 0, Seed + k + f);
                        b.Quad(FxShape.Glow, p, size * .8f, A(Snow, .6f), 0, Seed);
                        continue;
                    }
                    float a = t - li; if (a > 1f) continue;
                    b.Quad(FxShape.Glow, target, size * 2.6f, A(Snow, .4f * Fade(a, 0, .12f)), 0, Seed);
                    b.Floor(Flake, target, size * 3f, A(Ice, .75f * Fade(a, .2f, 1f)), 0, Seed + k);
                    b.Quad(FxShape.Smoke, target + Vector2.up * a * .6f, size * (2f + a * 2), A(new Color(.8f, .9f, 1f), .5f * Fade(a, 0, .7f)), a, Seed + k);
                    Grit(b, target, size * (2.2f + 2f * a), new Color(.85f, .93f, 1f), .3f * Fade(a, .1f, .9f), a, Seed + k + 7);
                    for (int j = 0; j < 6; j++) Chip(b, target, new Vector2((j - 2.5f) * 1.1f, 2.4f + H(Seed, k + j) * 2), 10, a, .6f, .07f + size * .2f, Ice, Seed + k * 5 + j);
                }
            }
        }

        /// <summary>Step 8 weather. A cold whiteout: the arena tints blue, a hundred and fifty snowflakes blow slantwise, big flakes turn slowly, rime creeps in at the edges, long white gusts streak by.</summary>
        private sealed class Blizzard : FxEffect
        {
            protected override float Life => 2f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                float on = Env(this, now, 2.5f, 1.2f); if (on <= 0) return;
                Rect a = Arena; float H0 = a.height + 4;
                for (int k = 0; k < 8; k++)
                {
                    Vector2 e = new Vector2(a.xMin + a.width * (k % 4 / 3f), k < 4 ? a.yMax : a.yMin);
                    b.Quad(Flake, e, 1f + .4f * H(Seed, k), now * 4 * (k % 2 == 0 ? 1 : -1) + k * 20, A(Ice, .25f * on), 0, Seed + k, 0);
                }
                for (int k = 0; k < 7; k++)
                {
                    float u = (H(Seed, k + 400) + now * (.07f + .03f * H(Seed, k + 401))) % 1; Vector2 p = new Vector2(a.xMax + 3 - u * (a.width + 6), a.yMin + a.height * H(Seed, k + 402));
                    Grit(b, p, 1.3f + .6f * H(Seed, k + 403), new Color(.82f, .92f, 1f), .3f * on * Mathf.Sin(u * 3.14f), now * 1.5f + k, Seed + k);
                }
                for (int k = 0; k < 140; k++)
                {
                    float sp = 2.6f + 3f * H(Seed, k + 300), u = (H(Seed, k) + now * sp / H0) % 1;
                    float x = a.xMin + a.width * H(Seed, k + 150) + (1 - u) * 3.5f + Mathf.Sin(now * 2 + k) * .4f - 1.5f, y = a.yMax + 2 - u * H0, sz = .1f + .12f * H(Seed, k + 40);
                    if (k % 10 == 0) b.Quad(Flake, new Vector2(x, y), sz * 1.3f, now * 60 + k, A(Snow, .8f * on), 0, Seed + k, 0);
                    else b.Quad(FxShape.Glow, new Vector2(x, y), sz * .6f, A(Snow, .8f * on), 0, Seed);

                }
                for (int k = 0; k < 9; k++)
                {
                    float u = (H(Seed, k + 7) + now * (.5f + .3f * H(Seed, k + 8))) % 1, y = a.yMin + a.height * H(Seed, k + 17), x = a.xMax + 4 - u * (a.width + 10);
                    b.Link(new Vector2(x, y), new Vector2(x + 5f, y + 1.1f), .1f, A(Snow, .38f * on * Mathf.Sin(u * 3.14f)), Seed + k, 1);
                }
            }
        }
    }
}
