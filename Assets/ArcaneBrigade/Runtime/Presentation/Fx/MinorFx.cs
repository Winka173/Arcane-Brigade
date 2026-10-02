using System;
using System.Collections.Generic;
using UnityEngine;
using static ArcaneBrigade.FxKit;

namespace ArcaneBrigade
{
    /// <summary>The minor bonds. Smaller than the majors, each with its own silhouette: letters, roots, clouds, monolith, shards,
    /// hats, clover, statues, vines, thorns, candy, cranes, runes, masks, fists, bubbles, blades, crushes, horns, stars, bombs.</summary>
    public static class MinorFx
    {
        private delegate void DrawFn(FxEffect e, FxBatch b, float now, float pre, float t);
        private sealed class Fn : FxEffect
        {
            private readonly float life; private readonly DrawFn draw;
            public Fn(float life, DrawFn draw) { this.life = life; this.draw = draw; }
            protected override float Life => life;
            public override void Draw(FxBatch b, float now, float pre, float t) => draw(this, b, now, pre, t);
        }
        private static void Add(Dictionary<string, Func<FxEffect>> m, string rule, float life, DrawFn d) { m[rule] = () => new Fn(life, d); }
        private static Vector2 Sq(float r) => DKit.Sq(r);
        private static readonly Color Ink = new Color(.25f, .45f, 1f, 1), Paper = new Color(1f, .96f, .8f, 1), Wood = new Color(.75f, .45f, .2f, 1), Leaf = new Color(.45f, 1f, .4f, 1),
            Amber = new Color(1f, .7f, .25f, 1), Cloud = new Color(.85f, .9f, 1f, 1), Gold = new Color(1f, .85f, .35f, 1), Marble = new Color(.92f, .92f, 1f, 1),
            Rose = new Color(1f, .4f, .55f, 1), Candy = new Color(1f, .5f, .75f, 1), Sea = new Color(.4f, .85f, 1f, 1), Iron = new Color(.7f, .72f, .8f, 1), Spark = new Color(1f, .6f, .2f, 1),
            Soil = new Color(.42f, .3f, .2f, 1), Stone = new Color(.58f, .52f, .48f, 1), Foam = new Color(.75f, .9f, 1f, 1), Soot = new Color(.22f, .19f, .17f, 1);

        public static void Register(Dictionary<string, Func<FxEffect>> map)
        {
            Add(map, "rain-letters", 1.5f, Letters);
            Add(map, "root-still-roots", 1.5f, Roots);
            Add(map, "cloud-drift", 4.4f, CloudDrift);
            Add(map, "stone-monolith-stomp", 1.1f, MonolithStomp);
            Add(map, "stone-monolith-pull", 1.1f, MonolithPull);
            Add(map, "bastion-shield-shard", .9f, BastionShard);
            Add(map, "hat-disc", .9f, HatDisc);
            Add(map, "lucky-clover", 1.2f, Clover);
            Add(map, "exhibit-statue", 3.3f, Statue);
            Add(map, "exhibit-statue-burst", 1.2f, StatueBurst);
            Add(map, "vine-mend", 1.1f, VineMend);
            Add(map, "thorn-spikes", 1.0f, ThornSpikes);
            Add(map, "thorn-reflect", .9f, ThornReflect);
            Add(map, "candy-volley", 1.2f, CandyVolley);
            Add(map, "paper-crane", 1.2f, PaperCrane);
            Add(map, "haymaker", .9f, Haymaker);
            Add(map, "shanty-splash", 1.0f, ShantySplash);
            Add(map, "shield-burst-damage", 1.0f, BubbleBurst);
            Add(map, "bladenado", 1.3f, Bladenado);
            Add(map, "minor-crush", 1.0f, Crush);
            Add(map, "horn-pile-up", 1.0f, HornRam);
            Add(map, "unstoppable-stun", 1.0f, DizzyStars);
            Add(map, "demolitionist-stun", 1.1f, BlastCap);
            Add(map, "gear", .9f, GearBurst);
            Add(map, "dash-shockwave", .8f, DashWave);
            Add(map, "horn", .9f, HornGore);
        }

        /// <summary>Small droplets flicked out that fall and leave a tiny splash ring.</summary>
        private static void Drops(FxBatch b, Vector2 at, float t, int n, float spread, float lift, Color c, int seed, float size = .09f, float life = .6f)
        {
            if (t <= 0) return;
            for (int k = 0; k < n; k++)
            {
                float lf = life * (.7f + .5f * H(seed, k + 90)), u = t / lf; if (u <= 0) continue; Vector2 d = Dir(seed, k + 90, n, .62f);
                Vector2 land = at + d * spread * (.6f + .5f * H(seed, k + 95));
                if (u < 1) { Vector2 p = Vector2.Lerp(at, land, u) + Vector2.up * (lift * 4 * u * (1 - u)); b.Quad(FxShape.Glow, p, size * 2.2f, A(c, .9f * (1 - u * .4f)), 0, seed); Glint(b, p, size * 1.3f, White, .7f, k * 30); }
                else { float v = (u - 1) * 2.5f; if (v < 1) b.Floor(FxShape.Shock, land, .35f, A(c, .8f * (1 - v)), v, seed + k); }
            }
        }

        // rain of letters: paper cards flutter down, ink splats with drips, paper scraps and a blue blot spreads
        private static void Letters(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            Vector2 at = e.At; float r = Mathf.Max(1.1f, e.Radius) * .9f, age = now - e.Born;
            if (t < 0) b.Floor(FxShape.Glow, at, r * pre, A(Ink, .22f * pre), 0, e.Seed);
            for (int k = 0; k < 6; k++)
            {
                float start = k * .09f, u = (age - start) / .5f; if (u <= 0) continue;
                Vector2 land = at + new Vector2((H(e.Seed, k) - .5f) * r * 1.6f, (H(e.Seed, k + 9) - .5f) * r * .7f);
                if (u < 1)
                {
                    float sw = Mathf.Sin(u * 9 + k); Vector2 p = land + new Vector2(sw * .45f, 5f * (1 - DKit.EaseIn(u)));
                    b.Quad(FxShape.Glow, p, .8f, A(Ink, .3f), 0, e.Seed);
                    b.Quad(DKit.Glyph, p, new Vector2(.7f, .7f * (.55f + .45f * Mathf.Abs(Mathf.Cos(u * 7 + k)))), sw * 40, A(Paper, 1.1f), 0, e.Seed + k, 2);
                    b.Floor(FxShape.Shade, land, .35f * u, A(DKit.Dark, .4f * u), 0, e.Seed);
                }
                else
                {
                    float v = u - 1;
                    b.Quad(FxShape.Smoke, land, DKit.Sq(.8f + .35f * v), 0, A(new Color(.03f, .08f, .45f), .9f * Fade(v, .5f, 2)), v * .3f, e.Seed + k);
                    b.Floor(FxShape.Glow, land, 1.0f, A(Ink, .55f * Fade(v, 0, 1)), 0, e.Seed);
                    b.Quad(DKit.Glyph, land + Vector2.up * .1f, .75f, 0, A(new Color(.55f, .75f, 1f), 1.3f * Fade(v, .3f, 1.5f)), 0, e.Seed + k, 0);
                    // ink specks splash out, then drips run down from the blot
                    for (int j = 0; j < 6; j++) { float w = Rise(v, 0, .25f); b.Quad(FxShape.Disc, land + Dir(e.Seed + k, j, 6, .62f) * (.4f + .6f * EaseOut(w)) * (.7f + .5f * H(e.Seed + k, j)), .06f + .04f * H(e.Seed, j + k), A(Ink, .95f * Fade(v, .4f, 1.2f)), 0, e.Seed); }
                    for (int j = 0; j < 2; j++) { float w = Rise(v, .1f + j * .15f, .8f + j * .15f); if (w > 0 && w < 1) { Vector2 p = land + new Vector2((j - .5f) * .5f, -.15f - .5f * DKit.EaseIn(w)); b.Quad(FxShape.Glow, p, .14f, A(Ink, 1 - w), 0, e.Seed); b.Link(land + new Vector2((j - .5f) * .5f, -.1f), p, .03f, A(Ink, .7f * (1 - w)), e.Seed + j, .5f); } }
                    for (int j = 0; j < 3; j++) Glint(b, land + Dir(e.Seed + k, j, 3, .7f) * 1.0f * EaseOut(v * 2), .2f, Ink, Fade(v, 0, .5f), j * 80);
                }
            }
            // torn paper scraps flutter down slowly over the spot
            for (int k = 0; k < 6; k++) { float u = Rise(age, .1f + k * .1f, 1.4f + k * .05f); if (u <= 0 || u >= 1) continue; Vector2 p = at + new Vector2((H(e.Seed, k + 30) - .5f) * r * 1.8f + Mathf.Sin(u * 8 + k) * .3f, 3.2f * (1 - u)); b.Quad(FxShape.Disc, p, new Vector2(.12f, .07f * Mathf.Abs(Mathf.Sin(u * 10 + k)) + .02f), u * 500 + k * 40, A(Paper, .9f * Mathf.Sin(u * Mathf.PI)), 0, e.Seed); }
        }

        // roots burst out of the floor in thick brown tendrils that writhe, fork and sprout leaves, throwing clods of soil and dust
        private static void Roots(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            Vector2 at = e.At; float r = Mathf.Max(1.2f, e.Radius) * .95f;
            if (t < 0) { b.Floor(FxShape.Cracks, at, r, A(Wood, .6f * pre), pre, e.Seed, 1); return; }
            float grow = EaseOut(Rise(t, 0, .3f)), fade = Fade(t, .75f, 1.5f);
            b.Floor(FxShape.Shade, at, r * .9f, A(DKit.Dark, .5f * fade), 0, e.Seed + 2);
            b.Floor(FxShape.Cracks, at, r * 1.1f, A(Wood, .8f * fade), 1, e.Seed, 1);
            b.Floor(FxShape.Smoke, at, r * .9f, A(new Color(.2f, .12f, .06f), .5f * fade), Rise(t, 0, 1), e.Seed);
            DKit.DustRing(b, at, r * .9f, t, Soil, e.Seed, 8, .9f, .65f);
            DKit.Debris(b, at, t, 8, r * 1.1f, 1.3f, Soil, e.Seed, DKit.Shield, 1, .16f, .75f, DKit.DustBrown);
            for (int k = 0; k < 8; k++)
            {
                Vector2 end = at + Dir(e.Seed, k, 8, .62f) * r * 1.15f * grow + Vector2.up * (.5f + 1.0f * H(e.Seed, k + 5)) * grow;
                Root(b, at, end, .4f, t * 7 + k, .12f, fade, e.Seed + k * 5);
                Vector2 mid = Vector2.Lerp(at, end, .6f);
                if (k % 2 == 0) Root(b, mid, mid + Dir(e.Seed + 3, k, 5, .62f) * r * .5f * grow, .18f, t * 5, .07f, fade * .9f, e.Seed + k * 7);
                b.Quad(DKit.Star, end, new Vector2(.34f, .34f), DKit.Deg(end - at) + Mathf.Sin(t * 9 + k) * 12, A(Leaf, 1.1f * fade * grow), 0, e.Seed + k, -2);
                if (k % 3 == 0) Glint(b, end + Vector2.up * .2f, .16f, Leaf, fade * grow * (.5f + .5f * Mathf.Sin(t * 14 + k)), k * 30);
            }
        }

        private static void Root(FxBatch b, Vector2 a, Vector2 z, float amp, float phase, float w, float alpha, int seed)
        {
            Vector2 d = z - a; float len = d.magnitude; if (len < .05f) return; Vector2 n = new Vector2(-d.y, d.x) / len; int segs = Mathf.Clamp(Mathf.CeilToInt(len / .12f), 4, 24); Vector2 prev = a;
            for (int s = 0; s <= segs; s++)
            {
                float t = s / (float)segs; Vector2 p = a + d * t + n * Mathf.Sin(t * 9.4f + phase) * amp * Mathf.Sin(t * Mathf.PI); float th = w * (1 - .75f * t);
                b.Quad(FxShape.Shade, p, th * 1.5f, A(new Color(.36f, .2f, .08f), alpha), 0, seed);
                if (s > 0) b.Link(prev, p, th * .35f, A(Amber, .55f * alpha), seed + s, .3f);
                prev = p;
            }
        }

        // a puffy rain cloud sails over the spot: lit billows, a shadow below, rain streaks that splash in tiny rings and a low mist
        private static void CloudDrift(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            float age = now - e.Born, on = Rise(age, 0, .5f) * Fade(now, e.End - .6f, e.End); Vector2 at = e.At + new Vector2(Mathf.Sin(age * .6f) * .6f, 0); float r = Mathf.Max(1.8f, e.Radius) * 1.0f;
            b.Floor(FxShape.Shade, at, r * 1.0f, A(new Color(.02f, .05f, .15f), .4f * on), 0, e.Seed);
            b.Quad(FxShape.Dust, at, new Vector2(r * 1.0f, r * .35f), 0, A(new Color(.5f, .6f, .75f), .35f * on), age * .1f, e.Seed + 3);
            Vector2 c = at + Vector2.up * 2.0f;
            for (int k = 0; k < 7; k++)
            {
                Vector2 p = c + new Vector2((k - 3f) * r * .3f, Mathf.Sin(k * 2.1f + age) * .2f + (k % 2) * .28f);
                b.Quad(FxShape.Smoke, p + Vector2.down * .08f, new Vector2(r * .42f, r * .28f), 0, A(new Color(.6f, .68f, .85f), .95f * on), age * .04f, e.Seed + k);
                b.Quad(FxShape.Smoke, p + Vector2.up * .12f, new Vector2(r * .35f, r * .24f), 0, A(Cloud, .95f * on), age * .04f + .3f, e.Seed + k + 20);
            }
            b.Quad(FxShape.Glow, c, new Vector2(r * 1.0f, .5f), 0, A(Cloud, .15f * on), 0, e.Seed);
            for (int k = 0; k < 24; k++)
            {
                float sp = 1.5f + .4f * H(e.Seed, k + 60), u = (H(e.Seed, k) + age * sp) % 1f; float x = (H(e.Seed, k + 40) - .5f) * r * 1.5f, gy = (H(e.Seed, k + 80) - .5f) * r * .5f;
                Vector2 top = c + new Vector2(x, -.3f), ground = at + new Vector2(x - .1f, gy); Vector2 p = Vector2.Lerp(top, ground, u);
                b.Beam(p, p + new Vector2(-.04f, -.4f), .02f, A(new Color(.6f, .8f, 1f), .7f * on), 0, k, 0);
                float sv = (u * 1f - .85f) / .15f; if (sv > 0) b.Floor(FxShape.Shock, ground, .28f, A(Foam, .8f * on * (1 - sv)), sv, e.Seed + k);
            }
        }

        // a standing carved slab (monolith) slams down, runes on it flare twice, dust leaps and chips of rock fly and bounce
        private static void MonolithStomp(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            Vector2 at = e.At; float r = Mathf.Max(1.3f, e.Radius) * 1.0f;
            if (t < 0) { b.Floor(FxShape.Shade, at, r * .7f * pre, A(DKit.Dark, .5f * pre), 0, e.Seed); return; }
            float drop = Rise(t, 0, .12f), sink = Fade(t, .5f, 1.0f), rune = .8f + .6f * DKit.Bump(t, .1f, .3f) + .5f * DKit.Bump(t, .38f, .6f);
            b.Quad(FxShape.Glow, at, r * 1.1f, A(Amber, .55f * Fade(t, 0, .2f)), 0, e.Seed);
            b.Floor(FxShape.Cracks, at, r * 1.0f, A(Amber, .9f * sink), Rise(t, 0, .15f), e.Seed, 1);
            b.Quad(DKit.Shield, at + Vector2.up * (1.5f + 4f * (1 - drop) - .4f * (1 - sink)), new Vector2(.9f, 1.6f), 0, A(Amber, 1.1f * sink * rune), t, e.Seed, 2);
            b.Floor(FxShape.Shock, at, r * 1.2f, A(Amber, .9f), Rise(t, 0, .6f), e.Seed);
            DKit.DustRing(b, at, r * .95f, t, Stone, e.Seed, 9, 1.0f, .75f);
            FxKit.SmokeColumn(b, at + Vector2.up * .2f, .5f, .9f, t * 1.3f, new Color(.5f, .44f, .38f), e.Seed + 3, 4);
            DKit.Debris(b, at, t, 9, r * 1.1f, 1.2f, Stone, e.Seed, DKit.Shield, 1, .18f, .75f, DKit.DustBrown);
            DKit.Sparks(b, at + Vector2.up * .3f, t, 6, 3f, 2.5f, Amber, e.Seed, .4f, .08f);
        }

        // the stone pulls: fissures glow, boulders spiral into the middle on orbits dragging dust, and grind together in a puff
        private static void MonolithPull(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 at = e.At; float r = Mathf.Max(1.3f, e.Radius) * 1.0f, f = Fade(t, .6f, 1.1f);
            b.Floor(FxShape.Cracks, at, r * 1.2f, A(Amber, 1.1f * f), Rise(t, 0, .2f), e.Seed, 2);
            b.Floor(FxShape.Glow, at, r * .7f, A(Amber, .3f * f), 0, e.Seed);
            for (int k = 0; k < 8; k++)
            {
                float u = (t * 1.4f + H(e.Seed, k)) % 1f, a = k * .78f + u * 6, rad = r * 1.15f * (1 - u);
                Vector2 p = at + new Vector2(Mathf.Cos(a) * rad, Mathf.Sin(a) * rad * .62f), tail = at + new Vector2(Mathf.Cos(a - .5f) * rad * 1.1f, Mathf.Sin(a - .5f) * rad * .68f);
                b.Quad(FxShape.Dust, Vector2.Lerp(tail, p, .4f), new Vector2(.4f, .18f), DKit.Deg(p - tail), A(Stone, .55f * f * (1 - u)), u, e.Seed + k);
                b.Quad(DKit.Shield, p, new Vector2(.24f, .18f), t * 300 + k * 50, A(Amber, f * (1 - u * .4f)), 0, e.Seed + k, 1);
            }
            float puff = Rise(t, .1f, 1f); b.Quad(FxShape.Dust, at + Vector2.up * .2f * puff, new Vector2(.6f + .4f * puff, .3f + .2f * puff), 0, A(Stone, .6f * Mathf.Sin(puff * Mathf.PI) * f), puff, e.Seed + 20);
            for (int k = 0; k < 5; k++) Glint(b, at + Dir(e.Seed, k, 5, .6f) * .35f, .16f, Amber, f * Mathf.Pow(.5f + .5f * Mathf.Sin(t * 25 + k * 2), 4), k * 40);
        }

        // a golden-white shield shard darts in a flat fast arc trailing glints, sticks upright in the crowd, then cracks into three
        private static void BastionShard(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            Vector2 at = e.At;
            if (t < 0)
            {
                Vector2 p = DKit.Arc(e.From, at, pre, .7f), q = DKit.Arc(e.From, at, Mathf.Max(0, pre - .08f), .7f);
                b.Link(q, p, .18f, A(Gold, .8f), e.Seed, .5f);
                for (int k = 1; k <= 5; k++) Glint(b, DKit.Arc(e.From, at, Mathf.Max(0, pre - k * .03f), .7f) + Vector2.down * k * .06f, .14f, Gold, 1 - k / 6f, k * 40);
                b.Quad(FxShape.Glow, p, 1.0f, A(Gold, .4f), 0, e.Seed); b.Quad(DKit.Shield, p, new Vector2(.85f, .6f), DKit.Deg(at - e.From), A(Gold, 1.2f), 0, e.Seed, 1);
                return;
            }
            float split = Rise(t, .22f, .25f);
            b.Quad(FxShape.Glow, at, 1.4f, A(Gold, .65f * Fade(t, 0, .25f)), 0, e.Seed);
            b.Floor(FxShape.Cracks, at, .9f, A(Gold, .9f * Fade(t, .3f, .9f)), Rise(t, 0, .1f), e.Seed, 1);
            if (split < 1) b.Quad(DKit.Shield, at + Vector2.up * .5f, new Vector2(.75f, .55f), 70 + Mathf.Sin(t * 50) * 4, A(Gold, 1.1f), 0, e.Seed, 1);
            for (int k = 0; k < 3; k++) { float u = Rise(t, .22f, .75f); if (u > 0 && u < 1) b.Quad(DKit.Shield, at + Dir(e.Seed, k, 3, .6f) * 1.2f * EaseOut(u) + Vector2.up * (.5f + .8f * Mathf.Sin(u * 3.14f)), new Vector2(.38f, .3f), 70 + u * 400 + k * 120, A(Gold, 1 - u), 0, e.Seed + k, 1); }
            DKit.Sparks(b, at + Vector2.up * .4f, t - .22f, 9, 3.5f, 2.8f, Gold, e.Seed, .45f, .09f);
            DKit.DustRing(b, at, .7f, t, Stone, e.Seed, 6, .7f, .55f);
            b.Floor(FxShape.Shock, at, 1.4f, A(Gold, .9f), Rise(t, 0, .4f), e.Seed);
        }

        // a top hat flies along the line spinning like a saw: dark crown, rose ribbon trail, a whirling white blur, sparks shaved off,
        // a skid of dust under it and confetti
        private static void HatDisc(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            Vector2 a = e.From, z = e.At; float u = t < 0 ? 0 : EaseOut(Rise(t, 0, .5f)), fade = Fade(t, .55f, .9f);
            if (t < 0) { b.Quad(FxShape.Glow, a, .7f * pre, A(Rose, .5f * pre), 0, e.Seed); return; }
            Vector2 head = Vector2.Lerp(a, z, u), dir = (z - a).normalized, nrm = new Vector2(-dir.y, dir.x); float spin = t * 1400;
            DKit.Chain(b, Vector2.Lerp(a, z, Mathf.Max(0, u - .35f)), head, .25f, 2.5f, t * 20, .06f, Rose, .8f * Fade(t, .3f, .9f), e.Seed, 12);
            b.Link(a, head, .06f, A(White, .6f * Fade(t, .2f, .8f)), e.Seed, .6f);
            for (int k = 5; k >= 1; k--) { Vector2 p = Vector2.Lerp(a, z, Mathf.Max(0, u - k * .04f)); b.Quad(FxShape.Disc, p, new Vector2(.5f, .3f), 0, A(Rose, .2f * (1 - k / 6f) * fade), 0, e.Seed); }
            for (int k = 0; k < 6; k++) { float v = (t * 3 + k / 6f) % 1f; Vector2 p = Vector2.Lerp(a, z, Mathf.Max(0, u - v * .3f)) + Vector2.down * .45f; b.Quad(FxShape.Dust, p, new Vector2(.35f + .3f * v, .14f + .1f * v), DKit.Deg(dir) * .2f, A(DKit.DustBrown, .5f * (1 - v) * fade), v, e.Seed + k); }
            b.Quad(FxShape.Glow, head, 1.2f, A(Rose, .45f * fade), 0, e.Seed);
            b.Quad(FxShape.Shade, head, new Vector2(.9f, .6f), 0, A(new Color(.03f, .02f, .05f), fade), 0, e.Seed);
            b.Quad(FxShape.Shock, head, new Vector2(.75f, .46f), 0, A(Rose, 1.2f * fade), .35f, e.Seed);
            b.Quad(FxShape.Disc, head, new Vector2(.36f, .22f), 0, A(new Color(.5f, .15f, .3f), .5f * fade), 0, e.Seed);
            b.Quad(DKit.Scythe, head, new Vector2(.88f, .55f), spin, A(White, 1.1f * fade), .75f, e.Seed, 0);
            b.Quad(DKit.Scythe, head, new Vector2(.88f, .55f), spin + 180, A(Gold, .9f * fade), .75f, e.Seed + 1, 0);
            for (int k = 0; k < 8; k++) { float v = (t * 4 + k / 8f) % 1f; Vector2 p = head + nrm * ((k % 2) * 2 - 1) * (.5f + .6f * v) - dir * .9f * v + Vector2.down * .4f * v * v; Glint(b, p, .12f, k % 3 == 0 ? White : Gold, fade * (1 - v), k * 40); }
            for (int k = 0; k < 12; k++) { float v = Rise(t, k * .03f, k * .03f + .6f); if (v > 0 && v < 1) b.Quad(FxShape.Disc, Vector2.Lerp(a, head, H(e.Seed, k)) + new Vector2(Mathf.Sin(v * 8 + k) * .3f, -1.0f * v + .3f), new Vector2(.11f, .06f), k * 40 + v * 500, A(DKit.Pastel(k), .9f * (1 - v)), 0, e.Seed); }
        }

        // a lucky four-leaf clover glides in on a trail of gold, stamps itself on the floor in a flare of gold stars, coins glint and
        // little leaves flutter down; the clover pulses a second time before it fades
        private static void Clover(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            Vector2 at = e.At;
            if (t < 0) { Vector2 p = DKit.Arc(e.From, at, pre, 2.2f); for (int k = 1; k < 6; k++) Glint(b, DKit.Arc(e.From, at, Mathf.Max(0, pre - k * .05f), 2.2f), .18f, Gold, 1 - k / 6f, k * 70); b.Quad(DKit.Star, p, .5f, pre * 500, A(Leaf, 1.2f), 0, e.Seed, 0); return; }
            float pulse = 1 + .15f * Fade(t, 0, .2f) + .1f * DKit.Bump(t, .45f, .7f);
            b.Quad(FxShape.Glow, at, 1.6f, A(Gold, .7f * Fade(t, 0, .3f) + .3f * DKit.Bump(t, .45f, .7f)), 0, e.Seed);
            b.Quad(DKit.Star, at, DKit.Sq(1.25f * pulse), t * 30, A(Leaf, 1.15f * Fade(t, .6f, 1.15f)), 0, e.Seed, 0);
            b.Quad(FxShape.Rays, at, 1.9f, A(Gold, .7f * Fade(t, 0, .5f)), t, e.Seed, 5);
            b.Floor(FxShape.Shock, at, 1.5f, A(Gold, .8f), Rise(t, 0, .45f), e.Seed);
            for (int k = 0; k < 10; k++) { float u = t / (.5f + .5f * H(e.Seed, k)); if (u > 0 && u < 1) b.Quad(DKit.Star, at + Dir(e.Seed, k, 10, .7f) * 1.4f * EaseOut(u) + Vector2.up * u * .9f, .17f, t * 300 + k * 40, A(Gold, 1.2f * (1 - u)), 0, e.Seed + k, 5); }
            DKit.Debris(b, at, t, 6, 1.2f, 1.1f, Gold, e.Seed + 4, FxShape.Disc, 0, .1f, .8f, null);
            for (int k = 0; k < 5; k++) { float u = Rise(t, .1f + k * .06f, 1.1f); if (u <= 0 || u >= 1) continue; Vector2 p = at + new Vector2((H(e.Seed, k + 50) - .5f) * 2f + Mathf.Sin(u * 7 + k) * .3f, 1.8f * (1 - u) - .2f); b.Quad(DKit.Star, p, .16f, Mathf.Sin(u * 9 + k) * 60, A(Leaf, Mathf.Sin(u * Mathf.PI)), 0, e.Seed + k, -1); }
        }

        // a stone exhibit: a marble blob under a museum spotlight with a velvet-rope ring; dust floats in the beam, the cracks breathe
        private static void Statue(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            float age = now - e.Born, on = Rise(age, 0, .3f) * Fade(now, e.End - .3f, e.End); Vector2 at = e.At; float r = Mathf.Max(1.2f, e.Radius) * .9f;
            b.Quad(FxShape.Rays, at + Vector2.up * 2.4f, new Vector2(r * 1.2f, 3.0f), 0, A(Marble, .45f * on), age * .2f, e.Seed, 1);
            b.Floor(FxShape.Glow, at, r * 1.1f, A(Marble, .4f * on), 0, e.Seed);
            b.Floor(FxShape.Shade, at, .7f, A(DKit.Dark, .5f * on), 0, e.Seed);
            b.Quad(FxShape.Smoke, at + Vector2.up * .3f, new Vector2(.7f, .9f), 0, A(new Color(.78f, .78f, .88f), .95f * on), 0, e.Seed);
            b.Quad(FxShape.Cracks, at + Vector2.up * .3f, new Vector2(.7f, .9f), 0, A(Gold, .8f * on * (.6f + .4f * Mathf.Sin(age * 6))), age, e.Seed, 1);
            Glint(b, at + new Vector2(-.25f, .9f), .3f, White, on * Mathf.Pow(.5f + .5f * Mathf.Sin(age * 2.3f), 8), age * 30);
            for (int k = 0; k < 12; k++)
            {
                float a = k / 12f * 6.2832f; Vector2 p = at + new Vector2(Mathf.Cos(a) * r * 1.0f, Mathf.Sin(a) * r * .62f);
                b.Quad(FxShape.Disc, p + Vector2.up * .12f, .09f, A(Gold, .95f * on), 0, e.Seed);
                { float a0 = (k - 1) / 12f * 6.2832f; Vector2 q = at + new Vector2(Mathf.Cos(a0) * r * 1.0f, Mathf.Sin(a0) * r * .62f); Vector2 mid = (q + p) * .5f + Vector2.down * .1f; b.Link(q + Vector2.up * .1f, mid + Vector2.up * .1f, .03f, A(Rose, .8f * on), e.Seed, .5f); b.Link(mid + Vector2.up * .1f, p + Vector2.up * .1f, .03f, A(Rose, .8f * on), e.Seed, .5f); }
            }
            DKit.Motes(b, at + Vector2.up * .8f, r * .9f, 3f, now, 12, White, e.Seed, .9f * on, .1f, .25f);
        }

        // the statue shatters: marble chunks bounce, a gold flash, gravel and a billowing dust cloud
        private static void StatueBurst(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 at = e.At; float r = Mathf.Max(1.2f, e.Radius) * 1.0f;
            b.Quad(FxShape.Glow, at, r * 1.4f, A(Gold, .75f * Fade(t, 0, .25f)), 0, e.Seed);
            b.Floor(FxShape.Cracks, at, r * 1.1f, A(Gold, 1f * Fade(t, .2f, .9f)), Rise(t, 0, .2f), e.Seed, 2);
            DKit.DustRing(b, at, r * 1.0f, t, new Color(.7f, .7f, .78f), e.Seed, 10, 1.1f, .7f);
            FxKit.SmokeColumn(b, at + Vector2.up * .3f, .7f, 1.2f, t, new Color(.62f, .62f, .7f), e.Seed + 2, 5);
            DKit.Debris(b, at, t, 9, r * 1.2f, 1.5f, Marble, e.Seed, DKit.Shield, 1, .24f, .9f, new Color(.7f, .7f, .78f));
            DKit.Debris(b, at, t, 10, r * .9f, .8f, new Color(.8f, .8f, .9f), e.Seed + 5, FxShape.Disc, 0, .06f, .6f, null);
            DKit.Sparks(b, at + Vector2.up * .4f, t, 8, 3.5f, 3f, Gold, e.Seed, .45f, .09f);
        }

        // a green mending vine grows along the line as a twisted double strand with leaves, buds bloom along it, pollen drifts off and
        // little pulses of light run up it
        private static void VineMend(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 a = e.From, z = e.At; float grow = EaseOut(Rise(t, 0, .35f)), fade = Fade(t, .6f, 1.1f);
            Vector2 end = Vector2.Lerp(a, z, grow), n = new Vector2(-(z - a).y, (z - a).x).normalized;
            b.Link(a, end, .4f, A(Leaf, .2f * fade), e.Seed, 1);
            DKit.Chain(b, a, end, .4f, 2.5f, t * 8, .12f, Leaf, fade, e.Seed, 18);
            DKit.Chain(b, a, end, .4f, 2.5f, t * 8 + 3.14f, .12f, new Color(.7f, 1f, .5f), fade, e.Seed + 40, 14);
            for (int k = 1; k < 12; k++)
            {
                float f = k / 12f; if (f > grow) break; Vector2 p = Vector2.Lerp(a, z, f); float open = Rise(t, f * .35f + .05f, f * .35f + .25f);
                b.Quad(DKit.Star, p + n * .45f * (k % 2 * 2 - 1), .42f * open, DKit.Deg(n * (k % 2 * 2 - 1)) + Mathf.Sin(t * 8 + k) * 10, A(Leaf, 1.1f * fade), 0, e.Seed + k, -2);
                if (k % 3 == 0) b.Quad(DKit.Star, p - n * .3f * (k % 2 * 2 - 1), .2f * open, t * 40, A(DKit.Pink, 1.1f * fade), 0, e.Seed + k, 5);
                Glint(b, p, .26f, Gold, fade * .8f * (.5f + .5f * Mathf.Sin(t * 12 + k)), k * 40);
            }
            for (int k = 0; k < 10; k++) { float v = (t * .9f + H(e.Seed, k)) % 1f, f = H(e.Seed, k + 20) * grow; Vector2 p = Vector2.Lerp(a, z, f) + n * Mathf.Sin(v * 5 + k) * .3f + Vector2.up * v * 1.2f; Glint(b, p, .1f, new Color(.9f, 1f, .6f), fade * Mathf.Sin(v * Mathf.PI), k * 30); }
            for (int k = 0; k < 2; k++) { float pulse = Rise(t, .2f + k * .2f, .7f + k * .2f); if (pulse > 0 && pulse < 1) b.Quad(FxShape.Glow, Vector2.Lerp(a, z, pulse), 1.0f, A(new Color(.7f, 1f, .6f), .85f), 0, e.Seed); }
        }

        // thorns burst out of the floor one after another along the line, green spikes with rose tips; clods of earth jump at each,
        // dust puffs and sap beads glint
        private static void ThornSpikes(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 a = e.From, z = e.At; float fade = Fade(t, .55f, 1f); int n = Mathf.Clamp(Mathf.RoundToInt((z - a).magnitude * 1.6f), 6, 14);
            b.Link(a, Vector2.Lerp(a, z, EaseOut(Rise(t, 0, .3f))), .1f, A(new Color(.35f, .2f, .08f), .8f * fade), e.Seed, .4f);
            for (int k = 0; k < n; k++)
            {
                float st = k * .025f, u = EaseOut(Rise(t, st, st + .12f)); if (u <= 0) continue; float f = (k + .5f) / n; Vector2 p = Vector2.Lerp(a, z, f) + new Vector2(0, (H(e.Seed, k) - .5f) * .5f);
                float h = (1.1f + .7f * H(e.Seed, k + 5)) * (1 + .15f * Fade(t, st + .1f, st + .25f)), lean = (H(e.Seed, k + 9) - .5f) * 30, rad = (90 + lean) * Mathf.Deg2Rad;
                Vector2 up = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                b.Floor(FxShape.Glow, p, .7f, A(Rose, .35f * fade * u), 0, e.Seed);
                b.Floor(FxShape.Shade, p, .45f, A(DKit.Dark, .6f * fade), 0, e.Seed);
                b.Quad(DKit.Shield, p + up * h * .5f * u, new Vector2(h * .55f * u, .26f), 90 + lean + 180, A(Leaf, 1.15f * fade), 0, e.Seed + k, 1);
                b.Quad(DKit.Shield, p + up * h * .92f * u, new Vector2(.28f * u, .12f), 90 + lean + 180, A(Rose, 1.25f * fade), 0, e.Seed + k + 3, 1);
                float s = t - st; float v = Rise(s, 0, .5f);
                if (v < 1) b.Quad(FxShape.Dust, p + Vector2.up * .1f, new Vector2(.35f + .35f * v, .16f + .1f * v), 0, A(Soil, .65f * (1 - v)), v, e.Seed + k);
                if (k % 2 == 0) DKit.Debris(b, p, s, 3, .6f, .8f, Soil, e.Seed + k * 3, DKit.Shield, 1, .09f, .5f, null);
                Glint(b, p + up * h * .7f * u + new Vector2(.08f, 0), .12f, White, fade * u * Mathf.Pow(.5f + .5f * Mathf.Sin(t * 9 + k), 3), k * 30);
            }
        }

        // a thorn-ball bristles and clacks back: a spiky rose star whips round, thorns jab out in eight directions, needles fly and
        // rose petals scatter
        private static void ThornReflect(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 at = e.At; float r = Mathf.Max(.9f, e.Radius) * .9f, f = Fade(t, .3f, .9f), jab = EaseOut(Rise(t, 0, .15f));
            b.Quad(FxShape.Glow, at, r * 1.3f, A(Rose, .45f * f), 0, e.Seed);
            b.Quad(DKit.Star, at, r * 1.0f * (.6f + .4f * jab), t * 220, A(Rose, 1.2f * f), 0, e.Seed, 9);
            b.Quad(DKit.Star, at, r * .5f * (.6f + .4f * jab), -t * 300, A(White, .9f * f), 0, e.Seed + 1, 9);
            for (int k = 0; k < 8; k++) b.Quad(DKit.Shield, at + Dir(e.Seed, k, 8, .8f) * r * .9f * jab, new Vector2(r * .65f * jab, .12f), k * 45 + 10, A(Leaf, 1.1f * f), 0, e.Seed + k, 1);
            for (int k = 0; k < 10; k++) { float u = Rise(t, .05f, .45f + .1f * H(e.Seed, k + 20)); if (u <= 0 || u >= 1) continue; Vector2 d = Dir(e.Seed, k + 20, 10, .7f); Vector2 p = at + d * r * (.6f + 1.4f * EaseOut(u)); b.Link(p - d * .3f, p, .035f, A(Leaf, 1 - u), e.Seed + k, .8f); Glint(b, p, .1f, Rose, 1 - u, k * 20); }
            for (int k = 0; k < 6; k++) { float u = Rise(t, .1f, .9f); if (u <= 0 || u >= 1) continue; Vector2 p = at + Dir(e.Seed, k + 40, 6, .62f) * r * 1.1f * EaseOut(u) + new Vector2(Mathf.Sin(u * 8 + k) * .2f, .5f - .9f * u); b.Quad(FxShape.Disc, p, new Vector2(.13f, .07f + .04f * Mathf.Abs(Mathf.Sin(u * 9 + k))), u * 400 + k * 60, A(Rose, .9f * (1 - u)), 0, e.Seed); }
        }

        // wrapped sweets rain onto the crowd, twist-ends spinning, and burst into pastel sugar swirls with sprinkles and powdered sugar
        private static void CandyVolley(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 at = e.At; float r = Mathf.Max(1.1f, e.Radius) * 1.0f;
            for (int k = 0; k < 7; k++)
            {
                float start = k * .05f, u = (t - start) / .3f; if (u <= 0) continue; Color c = DKit.Pastel(k + e.Seed); Vector2 land = at + new Vector2((H(e.Seed, k) - .5f) * r * 1.5f, (H(e.Seed, k + 9) - .5f) * r * .65f);
                if (u < 1)
                {
                    Vector2 p = land + Vector2.up * 5f * (1 - DKit.EaseIn(u));
                    b.Beam(p + Vector2.up * .2f, p + Vector2.up * 1.2f, .07f, A(c, .5f), 0, e.Seed + k, .5f);
                    b.Quad(FxShape.Disc, p, .38f, A(c, .55f), 0, e.Seed); b.Quad(FxShape.Glow, p + new Vector2(-.12f, .12f), .2f, A(White, .8f), 0, e.Seed);
                    b.Quad(DKit.Star, p + Vector2.right * .5f, new Vector2(.45f, .3f), t * 500, A(c, 1.1f), 0, e.Seed, -2); b.Quad(DKit.Star, p - Vector2.right * .5f, new Vector2(.45f, .3f), 180 + t * 500, A(c, 1.1f), 0, e.Seed, -2);
                    b.Floor(FxShape.Shade, land, .3f * u, A(DKit.Dark, .4f * u), 0, e.Seed);
                }
                else
                {
                    float v = u - 1, s = v * .3f;
                    b.Quad(FxShape.Vortex, land, DKit.Sq(1.1f), v * 300, A(c, 1.2f * Fade(v, 0, 1.4f)), v, e.Seed + k, 1); b.Quad(FxShape.Glow, land, 1.3f, A(c, .4f * Fade(v, 0, 1)), 0, e.Seed);
                    b.Quad(FxShape.Dust, land + Vector2.up * .15f * v, new Vector2(.5f + .35f * v, .25f + .15f * v), 0, A(Color.Lerp(c, White, .6f), .55f * Fade(v, 0, 1.6f)), v, e.Seed + k);
                    for (int j = 0; j < 6; j++) { float w = Mathf.Clamp01(s / .4f); if (w >= 1) break; Vector2 d = Dir(e.Seed + k, j, 6, .8f); Vector2 p = land + d * .8f * EaseOut(w) + Vector2.up * (1.4f * w - 1.6f * w * w); b.Quad(FxShape.Disc, p, new Vector2(.08f, .035f), j * 60 + w * 600, A(DKit.Pastel(j + k), 1 - w), 0, e.Seed); }
                }
            }
        }

        // an origami crane stoops out of the sky onto the crowd with wind streaks, unfolds, bursts into folded paper scraps, and the
        // smallest scraps see-saw down slowly afterwards
        private static void PaperCrane(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 at = e.At; const float dive = .22f;
            if (t < dive)
            {
                float u = t / dive; Vector2 dv = new Vector2(-2.6f, 4.2f), p = at + dv * (1 - DKit.EaseIn(u)); float flap = Mathf.Sin(t * 70) * 25;
                for (int k = 0; k < 4; k++) { Vector2 o = new Vector2(-dv.y, dv.x).normalized * ((k - 1.5f) * .3f); b.Beam(p + o + dv.normalized * (.6f + k * .2f), p + o + dv.normalized * (1.8f + k * .3f), .025f, A(White, .55f), 0, e.Seed + k, 0); }
                b.Quad(DKit.Shield, p + new Vector2(.2f, .42f), new Vector2(1.0f, .6f), 160 + flap, A(Paper, 1.2f), 0, e.Seed, 1);
                b.Quad(DKit.Shield, p + new Vector2(.2f, -.42f), new Vector2(1.0f, .6f), -160 - flap, A(Paper, 1.2f), 0, e.Seed + 1, 1);
                b.Quad(DKit.Shield, p, new Vector2(.85f, .34f), DKit.Deg(-dv), A(Rose, 1.2f), 0, e.Seed + 2, 1);
                b.Floor(FxShape.Shade, at, .6f * u, A(DKit.Dark, .4f * u), 0, e.Seed);
                return;
            }
            float s = t - dive; b.Quad(FxShape.Glow, at, 1.7f, A(Paper, .6f * Fade(s, 0, .25f)), 0, e.Seed);
            for (int k = 0; k < 9; k++) { float u = s / (.5f + .3f * H(e.Seed, k)); if (u > 0 && u < 1) b.Quad(DKit.Shield, at + Dir(e.Seed, k, 9, .65f) * 1.8f * EaseOut(u) + Vector2.up * (1.4f * Mathf.Sin(u * 3.14f)), new Vector2(.42f, .3f), s * 500 + k * 40, A(k % 3 == 0 ? Rose : Paper, 1.2f * (1 - u)), 0, e.Seed + k, 1); }
            for (int k = 0; k < 8; k++) { float u = Rise(s, .15f, .95f); if (u <= 0 || u >= 1) continue; float sw = Mathf.Sin(u * 10 + k * 1.3f); Vector2 p = at + new Vector2((H(e.Seed, k + 30) - .5f) * 2.4f + sw * .25f, 1.6f * (1 - u) + .1f); b.Quad(FxShape.Disc, p, new Vector2(.12f, .05f), sw * 40, A(k % 3 == 0 ? Rose : Paper, .9f * Mathf.Sin(u * Mathf.PI)), 0, e.Seed); }
            b.Floor(FxShape.Shock, at, 1.8f, A(Paper, .85f), Rise(s, 0, .5f), e.Seed);
            DKit.DustRing(b, at, .9f, s, DKit.DustGrey, e.Seed, 6, .7f, .45f);
        }

        // a heavy gauntlet punch: a comic impact star slams in, speed streaks lash out, a cone of crescent shocks follows, dust blows
        // back off the floor and sweat-drops fly
        private static void Haymaker(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 at = e.At; float r = Mathf.Max(1.0f, e.Radius) * 1.0f, f = Fade(t, .3f, .8f);
            b.Quad(FxShape.Glow, at, r * 1.5f, A(Spark, .65f * Fade(t, 0, .2f)), 0, e.Seed);
            b.Quad(DKit.Star, at, r * 1.15f * (1.3f - .3f * EaseOut(Rise(t, 0, .1f))), t * 25 + 8, A(Gold, 1.2f * f), 0, e.Seed, 9);
            b.Quad(DKit.Star, at, r * .7f, -t * 30, A(White, 1.2f * f), 0, e.Seed + 1, 7);
            for (int k = 0; k < 3; k++) { float u = Rise(t, k * .05f, .5f + k * .05f); if (u > 0 && u < 1) b.Quad(DKit.Scythe, at, DKit.Sq(r * (1 + k * .4f) * (.6f + .5f * u)), 0, A(Spark, 1.1f * (1 - u)), u, e.Seed + k, 0); }
            for (int k = 0; k < 9; k++) { float u = Rise(t, 0, .25f); Vector2 d = Dir(e.Seed, k, 9, .7f); b.Link(at + d * r * (.5f + .5f * u), at + d * r * (1.1f + 1.0f * u), .06f, A(White, .9f * f), e.Seed + k, .6f); }
            DKit.DustRing(b, at, r * .9f, t, DKit.DustBrown, e.Seed, 8, .75f, .6f);
            Drops(b, at + Vector2.up * .5f, t, 6, 1.4f, 1.0f, Sea, e.Seed, .07f, .5f);
            DKit.Sparks(b, at + Vector2.up * .2f, t, 6, 3.5f, 2f, Gold, e.Seed + 3, .35f, .08f);
        }

        // a splash of bubbles: ring bubbles with glints float up and pop; a crown of water jumps, droplets arc out and foam lies low
        private static void ShantySplash(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 at = e.At; float r = Mathf.Max(1.0f, e.Radius) * 1.0f;
            b.Floor(FxShape.Glow, at, r * 1.0f, A(Sea, .4f * Fade(t, 0, .5f)), 0, e.Seed);
            b.Quad(FxShape.Dust, at, new Vector2(r * .9f, r * .4f), 0, A(Foam, .55f * Fade(t, .3f, 1f) * Rise(t, 0, .1f)), t, e.Seed + 5);
            float cr = Rise(t, 0, .45f);
            if (cr < 1) for (int k = 0; k < 9; k++) { Vector2 d = Dir(e.Seed + 3, k, 9, .62f); Vector2 base0 = at + d * r * (.3f + .5f * cr); b.Link(base0, base0 + Vector2.up * (.9f * Mathf.Sin(cr * Mathf.PI)) + d * .2f, .06f, A(Foam, .9f * (1 - cr)), e.Seed + k, .6f); Glint(b, base0 + Vector2.up * (.9f * Mathf.Sin(cr * Mathf.PI)) + d * .25f, .12f, White, 1 - cr, k * 30); }
            for (int k = 0; k < 10; k++)
            {
                float u = Rise(t, k * .025f, .8f + k * .02f); if (u <= 0 || u >= 1) continue; Vector2 p = at + new Vector2((H(e.Seed, k) - .5f) * r * 1.5f + Mathf.Sin(u * 6 + k) * .15f, (H(e.Seed, k + 9) - .5f) * r * .45f) + Vector2.up * u * 1.6f; float s = .28f + .3f * H(e.Seed, k + 4);
                b.Quad(FxShape.Shock, p, s * 1.8f, A(Sea, 1.1f * Fade(u, .8f, 1)), .33f, e.Seed + k);
                Glint(b, p + new Vector2(-.3f, .3f) * s, .18f, White, 1 - u, 0);
                if (u > .8f) for (int j = 0; j < 4; j++) Glint(b, p + Dir(e.Seed + k, j, 4, 1f) * s * 2 * (u - .8f) * 5, .1f, Sea, 1, 0);
            }
            Drops(b, at + Vector2.up * .3f, t, 10, r * 1.3f, 1.4f, Sea, e.Seed, .08f, .6f);
        }

        // a glass bubble pops: a blue sphere flares, glass shards fly in a fan with glints, fine spray hangs as mist
        private static void BubbleBurst(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 at = e.At; float r = Mathf.Max(1.0f, e.Radius) * 1.0f;
            b.Quad(FxShape.Shock, at, DKit.Sq(r * 1.1f), 0, A(DKit.Steel, 1.1f * Fade(t, 0, .25f)), .35f, e.Seed);
            b.Quad(FxShape.Glow, at + new Vector2(-.3f, .3f) * r, r * .35f, A(White, .9f * Fade(t, 0, .15f)), 0, e.Seed);
            b.Quad(FxShape.Glow, at, r * 1.2f, A(Sea, .6f * Fade(t, 0, .2f)), 0, e.Seed);
            for (int k = 0; k < 9; k++) { float u = t / (.55f + .2f * H(e.Seed, k)); if (u > 0 && u < 1) { Vector2 p = at + Dir(e.Seed, k, 9, .65f) * r * 1.5f * EaseOut(u) + Vector2.down * .4f * u * u; b.Quad(DKit.Shield, p, new Vector2(.45f, .32f), DKit.Deg(Dir(e.Seed, k, 9, 1f)) + u * 200, A(k % 2 == 0 ? Sea : DKit.Ice, 1.2f * (1 - u * u)), 0, e.Seed + k, 1); Glint(b, p, .16f, White, (1 - u) * Mathf.Pow(.5f + .5f * Mathf.Sin(u * 30 + k), 3), k * 30); } }
            b.Quad(FxShape.Smoke, at + Vector2.up * .2f, DKit.Sq(r * (.6f + .4f * Rise(t, 0, 1))), 0, A(Foam, .35f * Mathf.Sin(Rise(t, 0, 1) * Mathf.PI)), t, e.Seed + 4);
            Drops(b, at + Vector2.up * .2f, t, 8, r * 1.1f, .9f, Foam, e.Seed + 7, .06f, .5f);
            b.Floor(FxShape.Shock, at, r * 1.5f, A(Sea, .9f), Rise(t, 0, .6f), e.Seed);
        }

        // spinning shuriken blades orbit the crowd in a whirl, each with a crescent motion trail, sparks shaved off them and a ring
        // of dust stirred round on the floor
        private static void Bladenado(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 at = e.At; float r = Mathf.Max(1.1f, e.Radius) * 1.15f, f = Fade(t, .8f, 1.3f), g = EaseOut(Rise(t, 0, .15f));
            b.Floor(FxShape.Glow, at, r * 1.0f, A(Iron, .25f * f), 0, e.Seed);
            for (int k = 0; k < 6; k++) { float a = k * 1.047f - t * 6; Vector2 p = at + new Vector2(Mathf.Cos(a) * r * .95f * g, Mathf.Sin(a) * r * .59f * g); b.Quad(FxShape.Dust, p, new Vector2(.6f, .25f), a * Mathf.Rad2Deg + 90, A(DKit.DustGrey, .45f * f * g), t + k * .1f, e.Seed + k); }
            for (int k = 0; k < 4; k++)
            {
                float a = k * 1.5708f + t * 11; Vector2 p = at + new Vector2(Mathf.Cos(a) * r * g, Mathf.Sin(a) * r * .62f * g) + Vector2.up * .35f;
                b.Quad(DKit.Star, p, .6f, t * 900, A(Iron, 1.2f * f), 0, e.Seed + k, 4);
                for (int j = 0; j < 3; j++) { float v = (t * 5 + j / 3f + k * .17f) % 1f; Vector2 tg = new Vector2(-Mathf.Sin(a), Mathf.Cos(a) * .62f); Vector2 q = p - tg * .8f * v + Vector2.down * .5f * v * v; Glint(b, q, .12f, j == 0 ? White : Spark, f * (1 - v), j * 40); }
            }
            b.Quad(DKit.Scythe, at + Vector2.up * .35f, Sq(r * 1.05f), 0, A(White, .8f * f), Mathf.Repeat(t * 1.7f, 1), e.Seed, 0);
            b.Quad(DKit.Scythe, at + Vector2.up * .35f, Sq(r * 1.05f), 180, A(Iron, .6f * f), Mathf.Repeat(t * 1.7f + .5f, 1), e.Seed + 1, 0);
        }

        // crush: a ring of grit snaps shut into a flat starburst, pressure lines chevron inward, dust squirts up out of the middle
        private static void Crush(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 at = e.At; float r = Mathf.Max(1.0f, e.Radius) * 1.0f, c = Rise(t, 0, .25f);
            b.Floor(DKit.Hole, at, r * (1.4f - .95f * EaseOut(c)), A(DKit.Purple, 1.1f * Fade(t, .3f, .8f)), t, e.Seed, 1);
            for (int k = 0; k < 10; k++) { Vector2 d = Dir(e.Seed, k, 10, .7f); b.Link(at + d * r * (1.2f - 1.0f * EaseOut(c)), at + d * r * (1.5f - 1.2f * EaseOut(c)), .08f, A(DKit.Lavender, .9f * Fade(t, .15f, .5f)), e.Seed + k, .5f); }
            for (int k = 0; k < 8; k++) { Vector2 d = Dir(e.Seed + 1, k, 8, .62f); b.Quad(FxShape.Dust, at + d * r * 1.1f * (1 - EaseOut(c)) , new Vector2(.45f, .2f), DKit.Deg(d), A(DKit.DustGrey, .6f * (1 - c * .6f) * Fade(t, .2f, .5f)), c, e.Seed + k); }
            b.Quad(DKit.Star, at, Sq(r * .8f * Fade(t, .2f, .8f)), 0, A(White, 1.2f * Fade(t, .2f, .8f)), 0, e.Seed, 8);
            b.Floor(FxShape.Glow, at, r * .9f, A(DKit.Purple, .65f * Fade(t, .2f, .7f)), 0, e.Seed);
            FxKit.SmokeColumn(b, at + Vector2.up * .1f, .4f, 1.1f, (t - .2f) * 1.5f, DKit.DustGrey, e.Seed + 3, 4);
            DKit.Debris(b, at, t - .22f, 6, .9f, 1.2f, DKit.Lavender, e.Seed, DKit.Star, 5, .13f, .6f, null);
        }

        // horns ram: two bull-horn crescents slam together, a dust puff blows out of the bonk and canary stars spin off it
        private static void HornRam(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 at = e.At; float r = Mathf.Max(1.0f, e.Radius) * 1.0f, close = 1 - EaseOut(Rise(t, 0, .12f));
            for (int s = -1; s <= 1; s += 2) b.Quad(DKit.Scythe, at + new Vector2(s * (.2f + 1.0f * close) * r, Mathf.Sin(Mathf.Max(0, t - .12f) * 50) * .05f * Fade(t, .12f, .4f)), Sq(r * 1.25f), s > 0 ? 180 : 0, A(Amber, 1.2f * Fade(t, .35f, .9f)), .85f, e.Seed + s, 0);
            b.Quad(FxShape.Glow, at, r * 1.3f, A(Gold, .75f * Fade(t, .05f, .35f)), 0, e.Seed);
            b.Floor(FxShape.Shock, at, r * 1.25f, A(Gold, .9f), Rise(t, .05f, .5f), e.Seed);
            DKit.DustRing(b, at, r * .8f, t - .1f, DKit.DustBrown, e.Seed, 7, .7f, .6f);
            DKit.Sparks(b, at + Vector2.up * .3f, t - .1f, 8, 3f, 3f, Gold, e.Seed, .4f, .08f);
            for (int k = 0; k < 3; k++) { float a = t * 9 + k * 2.09f; b.Quad(DKit.Star, at + new Vector2(Mathf.Cos(a) * r * .8f, Mathf.Sin(a) * r * .4f + 1.0f), .36f, t * 300, A(Gold, 1.2f * Fade(t, .4f, 1)), 0, e.Seed + k, 5); }
        }

        // a heavy bash leaves dizzy canary stars orbiting on a hex pad, each dragging a tiny sparkle tail; a bump puff and sparks
        private static void DizzyStars(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 at = e.At; float r = Mathf.Max(1f, e.Radius) * .9f, f = Fade(t, .55f, 1f);
            b.Quad(FxShape.Glow, at, r * 1.2f, A(Gold, .55f * Fade(t, 0, .25f)), 0, e.Seed);
            b.Floor(DKit.Hex, at, r * 1.25f, A(Gold, 1f * f), 0, e.Seed, 1);
            b.Quad(FxShape.Dust, at + Vector2.up * .2f * Rise(t, 0, .6f), new Vector2(.7f + .4f * Rise(t, 0, .6f), .3f), 0, A(DKit.DustBrown, .55f * Fade(t, .05f, .6f)), t, e.Seed);
            DKit.Sparks(b, at + Vector2.up * .3f, t, 6, 2.5f, 2.5f, Gold, e.Seed, .35f, .08f);
            for (int k = 0; k < 5; k++)
            {
                float a = t * 8 + k * 1.2566f; Vector2 P(float aa) => at + new Vector2(Mathf.Cos(aa) * r * 1.0f, Mathf.Sin(aa) * r * .4f + 1.1f);
                for (int j = 1; j <= 3; j++) Glint(b, P(a - j * .18f), .1f * (1 - j / 4f), Gold, f * (1 - j / 4f), j * 30);
                b.Quad(DKit.Star, P(a), .38f, t * 200 + k * 30, A(k % 2 == 0 ? Gold : White, 1.2f * f), 0, e.Seed + k, 5);
            }
        }

        // a demolition cap goes off: a fiery core, a dusty mushroom of smoke, flying planks, embers and soot specks
        private static void BlastCap(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 at = e.At; float r = Mathf.Max(1.0f, e.Radius) * 1.0f;
            b.Quad(FxShape.Glow, at, r * 1.4f, A(Spark, .75f * Fade(t, 0, .2f)), 0, e.Seed);
            b.Quad(FxShape.Flame, at + Vector2.up * .5f, new Vector2(.7f, 1.0f) * (1 + .5f * Rise(t, 0, .2f)), 0, A(Spark, 1.1f * Fade(t, .05f, .35f)), t, e.Seed, 2);
            b.Floor(FxShape.Shade, at, r * .8f, A(DKit.Dark, .55f * Fade(t, .6f, 1.1f)), 0, e.Seed + 1);
            for (int k = 0; k < 5; k++) b.Quad(FxShape.Smoke, at + new Vector2((k - 2) * .3f * r, 0) + Vector2.up * (EaseOut(Rise(t, 0, .9f)) * 1.3f + (k % 2) * .3f), new Vector2(r * .6f, r * .45f), 0, A(new Color(.55f, .48f, .42f), .95f * Fade(t, .2f, 1.1f)), t, e.Seed + k);
            b.Floor(FxShape.Shock, at, r * 1.25f, A(Spark, .9f), Rise(t, 0, .5f), e.Seed);
            DKit.DustRing(b, at, r * .9f, t, DKit.DustBrown, e.Seed, 8, .9f, .65f);
            for (int k = 0; k < 6; k++) { float u = t / (.5f + .2f * H(e.Seed, k)); if (u > 0 && u < 1) b.Quad(DKit.Shield, at + Dir(e.Seed, k, 6, .6f) * r * 1.2f * EaseOut(u) + Vector2.up * 1.5f * Mathf.Sin(u * 3), new Vector2(.5f, .15f), t * 500 + k * 60, A(Gold, 1.2f * (1 - u)), 0, e.Seed + k, 1); }
            DKit.Sparks(b, at + Vector2.up * .4f, t, 12, 4.5f, 3.5f, Spark, e.Seed + 2, .6f, .09f);
            DKit.Debris(b, at, t, 8, r * 1.0f, 1.0f, Soot, e.Seed + 4, FxShape.Shade, 0, .08f, .7f, null);
        }

        // a cog bursts into a ring of spinning gears, nuts and bolts tumble out, sparks fly and a drip of oil glints
        private static void GearBurst(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 at = e.At; float r = Mathf.Max(.9f, e.Radius) * .9f, f = Fade(t, .3f, .9f);
            b.Quad(FxShape.Glow, at, r * 1.2f, A(Gold, .45f * f), 0, e.Seed);
            b.Quad(DKit.Star, at, r * 1.1f, t * 200, A(Gold, .95f * f), 0, e.Seed, 108);
            for (int k = 0; k < 6; k++) { float u = Rise(t, 0, .7f); b.Quad(DKit.Star, at + Dir(e.Seed, k, 6, .7f) * r * 1.6f * EaseOut(u), .45f, -t * 400, A(Iron, 1.1f * Fade(u, .5f, 1)), 0, e.Seed + k, 106); }
            DKit.Debris(b, at, t, 7, r * 1.2f, 1.1f, Iron, e.Seed + 3, DKit.Hex, 0, .12f, .7f, DKit.DustGrey);
            DKit.Sparks(b, at + Vector2.up * .2f, t, 8, 3.5f, 2.5f, Spark, e.Seed, .4f, .08f);
            float dr = Rise(t, .1f, .8f); if (dr > 0 && dr < 1) { Vector2 p = at + new Vector2(.2f, -.1f - .6f * DKit.EaseIn(dr)); b.Quad(FxShape.Shade, p, .13f, A(new Color(.08f, .06f, .03f), .95f * (1 - dr)), 0, e.Seed); Glint(b, p + new Vector2(-.03f, .04f), .08f, White, 1 - dr, 0); }
        }

        // a dash leaves twin fans of chevron wind that shove the crowd back, kicked dust and pebbles skitter with them
        private static void DashWave(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 at = e.At; float r = Mathf.Max(1.0f, e.Radius) * 1.0f;
            for (int k = 0; k < 3; k++) { float u = Rise(t, k * .06f, .6f); if (u <= 0 || u >= 1) continue; b.Quad(DKit.Arcs, at + new Vector2(.95f * r, 0), new Vector2(r, r * .75f), 0, A(Sea, 1.2f * Fade(u, .4f, 1)), u, e.Seed + k, 1.3f); b.Quad(DKit.Arcs, at - new Vector2(.95f * r, 0), new Vector2(r, r * .75f), 180, A(Sea, 1.0f * Fade(u, .4f, 1)), u, e.Seed + k + 9, 1.3f); }
            for (int s = -1; s <= 1; s += 2)
            {
                float u = Rise(t, 0, .7f); if (u >= 1) continue;
                for (int k = 0; k < 3; k++) b.Quad(FxShape.Dust, at + new Vector2(s * r * (.3f + 1.3f * EaseOut(u)), (k - 1) * .35f - .2f), new Vector2(.5f + .4f * u, .18f + .1f * u), 0, A(DKit.DustBrown, .55f * (1 - u)), u, e.Seed + k + s * 5);
                for (int k = 0; k < 4; k++) { float v = Rise(t, k * .04f, .45f + k * .04f); if (v <= 0 || v >= 1) continue; Vector2 p = at + new Vector2(s * r * (.3f + 1.6f * EaseOut(v)), (H(e.Seed, k + s * 7) - .5f) * .8f - .2f + .5f * Mathf.Sin(v * Mathf.PI)); b.Quad(DKit.Shield, p, .08f, v * 500, A(Stone, 1.1f * (1 - v)), 0, e.Seed + k, 1); }
                for (int k = 0; k < 3; k++) { float v = Rise(t, .02f + k * .05f, .3f + k * .05f); if (v <= 0 || v >= 1) continue; Vector2 p = at + new Vector2(s * r * (.4f + 1.4f * v), (k - 1) * .4f); b.Beam(p, p - new Vector2(s * .8f, 0), .03f, A(White, .8f * (1 - v)), 0, e.Seed + k, 0); }
            }
        }

        // horns: one great amber horn hooks up from under the enemy in an uppercut, flinging dust, chips and sparks
        private static void HornGore(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 at = e.At; float r = Mathf.Max(1f, e.Radius) * .9f, up = EaseOut(Rise(t, 0, .18f)), f = Fade(t, .4f, .9f);
            b.Floor(FxShape.Smoke, at, r * 1.0f, A(new Color(.5f, .38f, .25f), .55f * f), Rise(t, 0, .9f), e.Seed);
            b.Floor(FxShape.Cracks, at, r * .8f, A(Amber, .8f * f), Rise(t, 0, .1f), e.Seed, 1);
            b.Quad(DKit.Scythe, at + Vector2.up * (up * .9f - .3f), new Vector2(r * .85f, r * 1.1f), 90, A(Amber, 1.2f * f), .35f + .5f * up, e.Seed, 0);
            b.Quad(FxShape.Glow, at + Vector2.up * .9f * up, r * 1.0f, A(Amber, .5f * Fade(t, .05f, .4f)), 0, e.Seed);
            DKit.DustRing(b, at, r * .8f, t, DKit.DustBrown, e.Seed, 7, .8f, .6f);
            for (int k = 0; k < 6; k++) { float u = Rise(t, .08f, .7f); if (u > 0 && u < 1) b.Quad(DKit.Shield, at + new Vector2((k - 2.5f) * .28f * r, 0) + Vector2.up * (2.2f * u - 2.0f * u * u + .3f), new Vector2(.18f, .13f), t * 500 + k * 50, A(Amber, 1.1f * (1 - u)), 0, e.Seed + k, 1); }
            for (int k = 0; k < 6; k++) b.Link(at + new Vector2((k - 2.5f) * .22f, -.2f), at + new Vector2((k - 2.5f) * .27f, 1.6f * up), .035f, A(White, .7f * Fade(t, .05f, .35f)), e.Seed + k, .6f);
            DKit.Sparks(b, at + Vector2.up * 1.0f * up, t - .1f, 6, 2.5f, 3f, Gold, e.Seed, .4f, .08f);
        }
    }
}
