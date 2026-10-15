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

        /// <summary>A faceted lump of ice: a navy ink copy a size bigger, then the facets (lit top-left) and a hint of light.</summary>
        private static void Chunk(FxBatch b, Vector2 p, float size, float deg, Color c, float a, int seed)
        {
            if (a <= .003f) return;
            b.Quad(Drop, p, Fit(new Vector2(size + .045f, size + .045f)), deg, A(new Color(.02f, .05f, .16f), a), 0, seed, 10);
            b.Quad(Drop, p, Fit(new Vector2(size, size)), deg, A(c, a), 0, seed, 10);
        }

        /// <summary>A hammer blow on a frozen enemy: the ice block round it shows for a beat, a dark crack zig-zags across it with a
        /// thin white line, then it splits into five big faceted chunks that slide apart and tumble, with chips, a puff of frost powder
        /// and frost stars on the floor. No white glare: blue light only at the crack.</summary>
        private sealed class FrozenHit : FxEffect
        {
            protected override float Life => 1.1f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                float r = Mathf.Clamp(Radius, 1f, 1.6f); Vector2 at = At, c = at + Vector2.up * .45f;
                float split = Rise(t, .12f, .14f), o = Fade(t, .75f, 1.1f);
                b.Floor(FxShape.Glow, at, r * 1.2f, A(Deep, .45f * Fade(t, 0, .9f)), 0, Seed);
                b.Floor(FxShape.Cracks, at, r * 1.1f, A(Snow, .75f * Fade(t, .3f, 1f) * Rise(t, .1f, .16f)), 1, Seed, 1);
                // the block, whole until the crack has run across it
                if (split < 1)
                {
                    Chunk(b, c, r * .95f, 8, Ice, .9f, Seed); b.Quad(FxShape.Glow, c, r * 1.1f, A(Deep, .3f), 0, Seed);
                    float run = Rise(t, 0, .12f); Vector2 p0 = c + new Vector2(-r * .7f, r * .45f), prev = p0;
                    for (int s = 1; s <= 6; s++)
                    {
                        float f = s / 6f; if (f > run + .01f) break;
                        Vector2 p = Vector2.Lerp(p0, c + new Vector2(r * .65f, -r * .5f), f) + new Vector2(0, (s % 2 == 0 ? .12f : -.12f) * r);
                        b.Link(prev, p, .07f, A(new Color(.02f, .05f, .16f), 1), Seed + s, 1); b.Link(prev, p, .035f, A(White, .9f), Seed + s, 1); prev = p;
                    }
                }
                // the blow: a short blue star at the strike point, ice powder
                Glint(b, c + new Vector2(-r * .55f, r * .4f), r * .9f, Ice, Fade(t, .05f, .3f), 20);
                for (int k = 0; k < 4; k++)
                {
                    float u = Rise(t, .1f + k * .04f, .9f); if (u <= 0 || u >= 1) continue;
                    Puff(b, c + Dir(Seed + 2, k, 4, .6f) * r * (.3f + .6f * u) + Vector2.up * u * .5f, r * (.3f + .35f * u), new Color(.82f, .92f, 1f), .55f * (1 - u), u, Seed + 5 + k);
                }
                Grit(b, at, r * (.7f + .5f * t), new Color(.85f, .93f, 1f), .45f * Fade(t, .3f, 1f), t, Seed + 6);
                // five chunks slide apart, tip over and drop
                if (split > 0)
                    for (int k = 0; k < 5; k++)
                    {
                        Vector2 d = Dir(Seed + 7, k, 5, .8f); float u = t - .12f;
                        Vector2 p = c + d * r * .35f + new Vector2(d.x * 2.2f, d.y * 1.2f + 1.6f) * u + Vector2.down * 6f * u * u;
                        p.y = Mathf.Max(p.y, at.y - .2f + d.y * .3f);
                        Chunk(b, p, r * (.36f + .1f * H(Seed, k)), 8 + d.x * u * 260, k % 2 == 0 ? Ice : Snow, o, Seed + k);
                    }
                for (int k = 0; k < 14; k++)
                {
                    Vector2 dd = Dir(Seed + 4, k, 14, .7f);
                    Chip(b, c, new Vector2(dd.x * (2.5f + 2 * H(Seed, k + 40)), 1.5f + 2.5f * H(Seed, k + 41)), 10, t - .12f, .6f, .1f + .05f * H(Seed, k), k % 2 == 0 ? Snow : Ice, Seed + k + 40);
                }
                for (int k = 0; k < 5; k++) Glint(b, at + Pt(Seed, k + 60, r, r * .5f), .12f, Snow, Rise(t, .3f, .4f) * Fade(t, .7f, 1.1f) * (.5f + .5f * Mathf.Sin(t * 20 + k)), k * 40);
            }
        }

        /// <summary>The frozen enemy bursts: an ice pillar heaves up, then blows apart into fourteen big faceted chunks thrown high that
        /// crash down all over the footprint (each landing in a puff of frost powder), a low ring of powder rolls out, chips and glitter
        /// rain and a column of snow dust rises.</summary>
        private sealed class Shatter : FxEffect
        {
            protected override float Life => 1.9f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                float r = Mathf.Clamp(Radius, 1.4f, 2.4f); Vector2 at = At;
                float heave = Overshoot(Rise(t, 0, .12f)) * (1 - Rise(t, .16f, .2f));
                b.Floor(FxShape.Glow, at, r * 1.3f, A(Deep, .5f * Fade(t, 0, 1.5f)), 0, Seed);
                b.Floor(FxShape.Shade, at, r * .8f, A(new Color(.01f, .03f, .1f), .5f * Fade(t, .8f, 1.8f)), 0, Seed + 1);
                if (heave > 0)
                    for (int k = 0; k < 5; k++)
                    {
                        float ang = 90 + (k - 2) * 16, h = r * (1.3f + .35f * (2 - Mathf.Abs(k - 2))) * heave;
                        b.Quad(Shard, at + new Vector2((k - 2) * .3f, .1f) + Vec(ang) * h * .45f, new Vector2(h * .55f, h * .2f), ang, A(k % 2 == 0 ? Ice : Deep, .95f), 0, Seed + k);
                    }
                b.Quad(FxShape.Glow, at + Vector2.up * .8f, r * 1.1f, A(Ice, .55f * Fade(t, .15f, .45f) * Rise(t, .14f, .17f)), 0, Seed);
                // the rolling ring of frost powder
                float ring = EaseOut(Rise(t, .16f, 1.1f));
                if (ring > 0 && ring < 1)
                    for (int k = 0; k < 12; k++)
                    {
                        Vector2 p = at + Dir(Seed + 3, k, 12, .55f) * r * (.3f + 1f * ring);
                        Puff(b, p + Vector2.up * .15f, r * (.22f + .2f * ring), new Color(.8f, .9f, 1f), .6f * (1 - ring), ring + k, Seed + k + 20);
                    }
                // snow dust column
                for (int k = 0; k < 6; k++)
                {
                    float u = Rise(t, .18f + k * .05f, 1.5f); if (u <= 0 || u >= 1) continue;
                    Puff(b, at + new Vector2((H(Seed, k + 30) - .5f) * r * .8f, .4f + u * 2.4f), r * (.25f + .3f * u), new Color(.75f, .87f, 1f), .5f * (1 - u), u, Seed + k + 30);
                }
                // big chunks thrown up and crashing down, each landing in a puff
                for (int k = 0; k < 14; k++)
                {
                    float u = t - .17f, life = .75f + .35f * H(Seed, k + 2); if (u <= 0) continue;
                    Vector2 land = at + Pt(Seed, k + 50, r * 1.1f, r * .65f), from = at + Vector2.up * .6f;
                    float h = 1.6f + 1.4f * H(Seed, k + 7);
                    if (u < life)
                    {
                        float v = u / life; Vector2 p = Vector2.Lerp(from, land, v) + Vector2.up * (h * 4 * v * (1 - v));
                        Chunk(b, p, .3f + .18f * H(Seed, k + 3), k * 40 + u * 400 * (k % 2 == 0 ? 1 : -1), k % 3 == 0 ? Snow : Ice, 1, Seed + k);
                        continue;
                    }
                    float a = (u - life) / .6f; if (a >= 1) continue;
                    Chunk(b, land, (.3f + .18f * H(Seed, k + 3)) * (1 - a * .3f), k * 40, k % 3 == 0 ? Snow : Ice, 1 - a * a, Seed + k);
                    Puff(b, land + Vector2.up * .1f, .3f + .3f * a, new Color(.85f, .93f, 1f), .55f * (1 - a), a, Seed + k + 60);
                    Glint(b, land + Vector2.up * .15f, .2f * (1 - a), Snow, 1 - a, k * 40);
                }
                for (int k = 0; k < 26; k++)
                {
                    Vector2 dd = Dir(Seed, k, 26, .55f);
                    Chip(b, at + Vector2.up * .4f, new Vector2(dd.x * r * (1.4f + 1.2f * H(Seed, k)), 3f + 4f * H(Seed, k + 7) + dd.y * 2), 12, t - .16f, 1f + .4f * H(Seed, k + 2), .08f + .05f * H(Seed, k + 3), k % 3 == 0 ? Snow : (k % 3 == 1 ? Ice : Deep), Seed + k);
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

        /// <summary>Step 8. Fourteen faceted hailstones plunge slanted out of the sky onto the footprint, trailing white streaks; each
        /// punches a dark dent with a star of cracks into the floor, bounces once and rolls to a stop as a lump of ice, kicking up a
        /// puff of powder and chips.</summary>
        private sealed class Hail : FxEffect
        {
            protected override float Life => 1.6f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                float r = Mathf.Max(1.6f, Radius); Vector2 at = At, slant = new Vector2(-3.2f, 8f);
                if (t < 0) b.Floor(FxShape.Glow, at, r * 1.2f, A(Deep, .35f * EaseOut(pre)), 0, Seed);
                for (int k = 0; k < 14; k++)
                {
                    Vector2 target = at + Pt(Seed, k, r * 1.05f, r * .65f); float li = -.15f + .65f * H(Seed, k + 9), fall = .4f, tt = (t - (li - fall)) / fall;
                    float size = k == 13 ? .42f : .2f + .1f * H(Seed, k + 2); float spin = k * 37;
                    if (tt < 0) continue;
                    if (tt < 1)
                    {
                        Vector2 p = Vector2.Lerp(target + slant, target, tt * tt * .4f + tt * .6f);
                        b.Link(p, p + slant.normalized * 2f, size * .35f, A(Snow, .6f), Seed + k, 1);
                        b.Quad(FxShape.Glow, p, size * 2.4f, A(Ice, .45f), 0, Seed);
                        Chunk(b, p, size, spin + tt * 300, Ice, 1, Seed + k);
                        continue;
                    }
                    float a = t - li; if (a > 1.1f) continue;
                    float o = Fade(a, .7f, 1.1f), hop = a < .35f ? .5f * size * 4 * (a / .35f) * (1 - a / .35f) : 0;
                    Vector2 rest = target + new Vector2(-size * 1.6f * EaseOut(Rise(a, 0, .6f)), 0);
                    b.Floor(FxShape.Shade, target, size * 2.2f, A(new Color(.01f, .03f, .1f), .6f * o), 0, Seed + k);
                    b.Floor(FxShape.Cracks, target, size * 3.2f, A(Snow, .8f * o * Rise(a, 0, .05f)), 1, Seed + k, 2);
                    b.Quad(FxShape.Glow, target, size * 2.4f, A(Ice, .5f * Fade(a, 0, .15f)), 0, Seed);
                    Chunk(b, rest + Vector2.up * (hop + size * .55f), size * .9f, spin + 300 + a * 260 * (1 - a), Ice, o, Seed + k);
                    Puff(b, target + Vector2.up * (.15f + a * .5f), size * (1.6f + a * 2.2f), new Color(.82f, .92f, 1f), .55f * Fade(a, 0, .7f), a, Seed + k);
                    Grit(b, target, size * (2f + 2f * a), new Color(.85f, .93f, 1f), .35f * Fade(a, .1f, .9f), a, Seed + k + 7);
                    for (int j = 0; j < 5; j++) Chip(b, target, new Vector2((j - 2) * 1.2f, 2.2f + H(Seed, k + j) * 2), 10, a, .55f, .06f + size * .15f, j % 2 == 0 ? Snow : Ice, Seed + k * 5 + j);
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
                for (int k = 0; k < 7; k++)
                {
                    float u = (H(Seed, k + 400) + now * (.07f + .03f * H(Seed, k + 401))) % 1; Vector2 p = new Vector2(a.xMax + 3 - u * (a.width + 6), a.yMin + a.height * H(Seed, k + 402));
                    Grit(b, p, .9f + .5f * H(Seed, k + 403), new Color(.82f, .92f, 1f), .2f * on * Mathf.Sin(u * 3.14f), now * 1.5f + k, Seed + k);
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
