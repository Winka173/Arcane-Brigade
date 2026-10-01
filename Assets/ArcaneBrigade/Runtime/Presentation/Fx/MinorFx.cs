using System;
using System.Collections.Generic;
using UnityEngine;
using static ArcaneBrigade.FxKit;

namespace ArcaneBrigade
{
    /// <summary>The 32 minor bonds. Smaller than the majors, each with its own silhouette: letters, roots, clouds, monolith, shards,
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
            Rose = new Color(1f, .4f, .55f, 1), Candy = new Color(1f, .5f, .75f, 1), Sea = new Color(.4f, .85f, 1f, 1), Iron = new Color(.7f, .72f, .8f, 1), Spark = new Color(1f, .6f, .2f, 1);

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
            Add(map, "paper-crane", 1.1f, PaperCrane);
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

        // rain of letters: paper cards flutter down, ink splats and a blue blot spreads
        private static void Letters(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            Vector2 at = e.At; float r = Mathf.Max(1.2f, e.Radius) * 1.1f, age = now - e.Born, s = e.Seed;
            if (t < 0) b.Floor(FxShape.Glow, at, r * pre, A(Ink, .25f * pre), 0, e.Seed);
            for (int k = 0; k < 7; k++)
            {
                float start = k * .09f, u = (age - start) / .55f; if (u <= 0) continue;
                Vector2 land = at + new Vector2((H(e.Seed, k) - .5f) * r * 1.7f, (H(e.Seed, k + 9) - .5f) * r * .8f);
                if (u < 1) { Vector2 p = land + new Vector2(Mathf.Sin(u * 9 + k) * .5f, 6.5f * (1 - DKit.EaseIn(u))); b.Quad(FxShape.Glow, p, 1f, A(Ink, .35f), 0, e.Seed); b.Quad(DKit.Glyph, p, .85f, Mathf.Sin(u * 9 + k) * 40, A(Paper, 1.1f), 0, e.Seed + k, 2); }
                else
                {
                    float v = u - 1; b.Quad(FxShape.Smoke, land, DKit.Sq(1.1f + .5f * v), 0, A(new Color(.03f, .08f, .45f), .95f * Fade(v, .5f, 2)), 0, e.Seed + k);
                    b.Floor(FxShape.Glow, land, 1.3f, A(Ink, .6f * Fade(v, 0, 1)), 0, e.Seed);
                    b.Quad(DKit.Glyph, land + Vector2.up * .1f, .95f, 0, A(new Color(.55f, .75f, 1f), 1.3f * Fade(v, .3f, 1.5f)), 0, e.Seed + k, 0);
                    for (int j = 0; j < 4; j++) Glint(b, land + Dir(e.Seed + k, j, 4, .7f) * 1.3f * EaseOut(v * 2), .26f, Ink, Fade(v, 0, .6f), j * 80);
                }
            }
        }

        // roots burst out of the floor in thick brown tendrils that writhe, fork, and sprout leaves
        private static void Roots(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            Vector2 at = e.At; float r = Mathf.Max(1.3f, e.Radius) * 1.1f;
            if (t < 0) { b.Floor(FxShape.Cracks, at, r, A(Wood, .6f * pre), pre, e.Seed, 1); return; }
            float grow = EaseOut(Rise(t, 0, .3f)), fade = Fade(t, .75f, 1.5f);
            b.Floor(FxShape.Smoke, at, r * 1.0f, A(new Color(.2f, .12f, .06f), .6f * fade), Rise(t, 0, 1), e.Seed);
            for (int k = 0; k < 9; k++)
            {
                Vector2 end = at + Dir(e.Seed, k, 9, .62f) * r * 1.3f * grow + Vector2.up * (.6f + 1.2f * H(e.Seed, k + 5)) * grow;
                Root(b, at, end, .45f, t * 7 + k, .13f, fade, e.Seed + k * 5);
                Vector2 mid = Vector2.Lerp(at, end, .6f);
                if (k % 2 == 0) Root(b, mid, mid + Dir(e.Seed + 3, k, 5, .62f) * r * .6f * grow, .2f, t * 5, .08f, fade * .9f, e.Seed + k * 7);
                b.Quad(DKit.Star, end, new Vector2(.4f, .4f), DKit.Deg(end - at), A(Leaf, 1.1f * fade * grow), 0, e.Seed + k, -2);
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

        // a puffy white rain cloud sails over the crowd, shadow below, streaks of rain and a slow drift
        private static void CloudDrift(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            float age = now - e.Born, on = Rise(age, 0, .5f) * Fade(now, e.End - .6f, e.End); Vector2 at = e.At + new Vector2(Mathf.Sin(age * .6f) * .8f, 0); float r = Mathf.Max(2f, e.Radius) * 1.15f;
            b.Floor(FxShape.Shade, at, r * 1.1f, A(new Color(.02f, .05f, .15f), .45f * on), 0, e.Seed);
            Vector2 c = at + Vector2.up * 2.1f;
            for (int k = 0; k < 8; k++)
            {
                Vector2 p = c + new Vector2((k - 3.5f) * r * .3f, Mathf.Sin(k * 2.1f + age) * .25f + (k % 2) * .3f);
                b.Quad(FxShape.Smoke, p + Vector2.down * .08f, new Vector2(r * .45f, r * .3f), 0, A(new Color(.6f, .68f, .85f), .95f * on), age * .04f, e.Seed + k);
                b.Quad(FxShape.Smoke, p + Vector2.up * .12f, new Vector2(r * .38f, r * .26f), 0, A(Cloud, .95f * on), age * .04f + .3f, e.Seed + k + 20);
            }
            b.Quad(FxShape.Glow, c, new Vector2(r * 1.1f, .6f), 0, A(Cloud, .18f * on), 0, e.Seed);
            for (int k = 0; k < 26; k++)
            {
                float u = (H(e.Seed, k) + age * 1.6f) % 1f; Vector2 p = c + new Vector2((H(e.Seed, k + 40) - .5f) * r * 1.6f, -.3f - u * 2.1f);
                b.Beam(p, p + new Vector2(-.05f, -.45f), .02f, A(new Color(.6f, .8f, 1f), .7f * on * (1 - u * .5f)), 0, k, 0);
            }
        }

        // a standing carved slab (monolith) slams down, stamping glowing runes, dust leaps and slivers of rock fly
        private static void MonolithStomp(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            Vector2 at = e.At; float r = Mathf.Max(1.4f, e.Radius) * 1.2f;
            if (t < 0) { b.Floor(FxShape.Shade, at, r * .7f * pre, A(DKit.Dark, .5f * pre), 0, e.Seed); return; }
            float drop = Rise(t, 0, .12f), sink = Fade(t, .5f, 1.0f);
            b.Quad(FxShape.Glow, at, r * 1.4f, A(Amber, .6f * Fade(t, 0, .2f)), 0, e.Seed);
            b.Quad(DKit.Shield, at + Vector2.up * (1.7f + 5f * (1 - drop) - .4f * (1 - sink)), new Vector2(1.1f, 1.9f), 0, A(Amber, 1.15f * sink), t, e.Seed, 2);
            b.Floor(FxShape.Shock, at, r * 1.5f, A(Amber, .9f), Rise(t, 0, .6f), e.Seed);
            b.Floor(FxShape.Smoke, at, r * 1.1f, A(new Color(.5f, .4f, .3f), .6f * Fade(t, .1f, .9f)), Rise(t, 0, 1), e.Seed + 2);
            for (int k = 0; k < 8; k++) { float u = t / (.5f + .2f * H(e.Seed, k)); if (u <= 0 || u >= 1) continue; b.Quad(DKit.Shield, at + Dir(e.Seed, k, 8, .6f) * r * 1.3f * EaseOut(u) + Vector2.up * 1.3f * Mathf.Sin(u * 3), new Vector2(.28f, .2f), t * 400 + k * 40, A(Amber, 1 - u), 0, e.Seed + k, 1); }
        }

        // the stone pulls: fissures glow, boulders spiral into the middle on orbits
        private static void MonolithPull(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 at = e.At; float r = Mathf.Max(1.4f, e.Radius) * 1.2f, f = Fade(t, .6f, 1.1f);
            b.Floor(FxShape.Cracks, at, r * 1.4f, A(Amber, 1.1f * f), Rise(t, 0, .2f), e.Seed, 2);
            b.Floor(FxShape.Glow, at, r * .8f, A(Amber, .35f * f), 0, e.Seed);
            for (int k = 0; k < 8; k++)
            {
                float u = (t * 1.4f + H(e.Seed, k)) % 1f, a = k * .78f + u * 6, rad = r * 1.3f * (1 - u);
                b.Quad(DKit.Shield, at + new Vector2(Mathf.Cos(a) * rad, Mathf.Sin(a) * rad * .62f), new Vector2(.26f, .2f), t * 300 + k * 50, A(Amber, f * (1 - u * .4f)), 0, e.Seed + k, 1);
            }
        }

        // a golden-white shield shard darts in a flat fast arc, spins, sticks upright in the crowd and cracks into three pieces
        private static void BastionShard(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            Vector2 at = e.At;
            if (t < 0)
            {
                Vector2 p = DKit.Arc(e.From, at, pre, .7f), q = DKit.Arc(e.From, at, Mathf.Max(0, pre - .08f), .7f);
                b.Link(q, p, .22f, A(Gold, .8f), e.Seed, .5f);
                b.Quad(FxShape.Glow, p, 1.2f, A(Gold, .4f), 0, e.Seed); b.Quad(DKit.Shield, p, new Vector2(1f, .7f), DKit.Deg(at - e.From), A(Gold, 1.2f), 0, e.Seed, 1);
                return;
            }
            b.Quad(FxShape.Glow, at, 2f, A(Gold, .7f * Fade(t, 0, .25f)), 0, e.Seed);
            b.Quad(DKit.Shield, at + Vector2.up * .6f, new Vector2(.9f, .65f), 70, A(Gold, 1.1f * Fade(t, .2f, .6f)), 0, e.Seed, 1);
            for (int k = 0; k < 3; k++) { float u = t / .55f; if (u > 0 && u < 1) b.Quad(DKit.Shield, at + Dir(e.Seed, k, 3, .6f) * 1.5f * EaseOut(u) + Vector2.up * .6f, new Vector2(.45f, .35f), t * 600 + k * 120, A(Gold, 1 - u), 0, e.Seed + k, 1); }
            b.Floor(FxShape.Shock, at, 1.9f, A(Gold, .9f), Rise(t, 0, .4f), e.Seed);
        }

        // a top hat flies along the line spinning like a saw: dark crown, rose ribbon, a whirling white blur, a slice left in the air, confetti
        private static void HatDisc(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            Vector2 a = e.From, z = e.At; float u = t < 0 ? 0 : EaseOut(Rise(t, 0, .5f)), fade = Fade(t, .55f, .9f);
            if (t < 0) { b.Quad(FxShape.Glow, a, .8f * pre, A(Rose, .5f * pre), 0, e.Seed); return; }
            Vector2 head = Vector2.Lerp(a, z, u), dir = (z - a).normalized; float spin = t * 1400;
            b.Link(a, head, .16f, A(Rose, .35f * Fade(t, .3f, .9f)), e.Seed, 1);
            b.Link(a, head, .07f, A(White, .8f * Fade(t, .2f, .8f)), e.Seed, .6f);
            for (int k = 5; k >= 1; k--) { Vector2 p = Vector2.Lerp(a, z, Mathf.Max(0, u - k * .045f)); b.Quad(FxShape.Disc, p, new Vector2(.6f, .36f), 0, A(Rose, .22f * (1 - k / 6f) * fade), 0, e.Seed); }
            b.Quad(FxShape.Glow, head, 1.5f, A(Rose, .45f * fade), 0, e.Seed);
            b.Quad(FxShape.Shade, head, new Vector2(1.05f, .7f), 0, A(new Color(.03f, .02f, .05f), fade), 0, e.Seed);
            b.Quad(FxShape.Shock, head, new Vector2(.85f, .52f), 0, A(Rose, 1.2f * fade), .35f, e.Seed);
            b.Quad(FxShape.Disc, head, new Vector2(.42f, .26f), 0, A(new Color(.5f, .15f, .3f), .5f * fade), 0, e.Seed);
            b.Quad(DKit.Scythe, head, new Vector2(1.0f, .62f), spin, A(White, 1.1f * fade), .75f, e.Seed, 0);
            b.Quad(DKit.Scythe, head, new Vector2(1.0f, .62f), spin + 180, A(Gold, .9f * fade), .75f, e.Seed + 1, 0);
            for (int k = 0; k < 14; k++) { float v = Rise(t, k * .03f, k * .03f + .6f); if (v > 0 && v < 1) b.Quad(FxShape.Disc, Vector2.Lerp(a, head, H(e.Seed, k)) + new Vector2(Mathf.Sin(v * 8 + k) * .3f, -1.2f * v + .3f), new Vector2(.12f, .07f), k * 40 + v * 500, A(DKit.Pastel(k), .9f * (1 - v)), 0, e.Seed); }
            for (int k = 0; k < 4; k++) Glint(b, head - dir * (.3f + k * .25f) + new Vector2(0, (k % 2 - .5f) * .4f), .25f, White, fade * (1 - k / 4f), spin * .2f + k * 30);
        }

        // a lucky four-leaf clover glides in on a trail of gold, stamps itself huge on the floor in a flare of gold stars
        private static void Clover(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            Vector2 at = e.At;
            if (t < 0) { Vector2 p = DKit.Arc(e.From, at, pre, 2.5f); for (int k = 1; k < 6; k++) Glint(b, DKit.Arc(e.From, at, Mathf.Max(0, pre - k * .05f), 2.5f), .2f, Gold, 1 - k / 6f, k * 70); b.Quad(DKit.Star, p, .6f, pre * 500, A(Leaf, 1.2f), 0, e.Seed, 0); return; }
            b.Quad(FxShape.Glow, at, 2.4f, A(Gold, .8f * Fade(t, 0, .3f)), 0, e.Seed);
            b.Quad(DKit.Star, at, DKit.Sq(1.6f * (1 + .15f * Fade(t, 0, .2f))), t * 30, A(Leaf, 1.15f * Fade(t, .5f, 1.1f)), 0, e.Seed, 0);
            b.Quad(FxShape.Rays, at, 2.6f, A(Gold, .8f * Fade(t, 0, .5f)), t, e.Seed, 5);
            for (int k = 0; k < 12; k++) { float u = t / (.5f + .5f * H(e.Seed, k)); if (u > 0 && u < 1) b.Quad(DKit.Star, at + Dir(e.Seed, k, 12, .7f) * 1.8f * EaseOut(u) + Vector2.up * u * 1.1f, .2f, t * 300 + k * 40, A(Gold, 1.2f * (1 - u)), 0, e.Seed + k, 5); }
        }

        // a stone exhibit: a marble blob under a museum spotlight with a velvet-rope ring; dust floats in the beam
        private static void Statue(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            float age = now - e.Born, on = Rise(age, 0, .3f) * Fade(now, e.End - .3f, e.End); Vector2 at = e.At; float r = Mathf.Max(1.4f, e.Radius);
            b.Quad(FxShape.Rays, at + Vector2.up * 3f, new Vector2(r * 1.4f, 3.8f), 0, A(Marble, .5f * on), age * .2f, e.Seed, 1);
            b.Floor(FxShape.Glow, at, r * 1.2f, A(Marble, .45f * on), 0, e.Seed);
            b.Quad(FxShape.Smoke, at + Vector2.up * .3f, new Vector2(.8f, 1f), 0, A(new Color(.78f, .78f, .88f), .95f * on), 0, e.Seed);
            b.Quad(FxShape.Cracks, at + Vector2.up * .3f, new Vector2(.8f, 1f), 0, A(Gold, .8f * on * (.6f + .4f * Mathf.Sin(age * 6))), age, e.Seed, 1);
            for (int k = 0; k < 14; k++)
            {
                float a = k / 14f * 6.2832f; Vector2 p = at + new Vector2(Mathf.Cos(a) * r * 1.05f, Mathf.Sin(a) * r * .65f);
                b.Quad(FxShape.Disc, p + Vector2.up * .12f, .1f, A(Gold, .95f * on), 0, e.Seed);
                if (k > 0) { float a0 = (k - 1) / 14f * 6.2832f; Vector2 q = at + new Vector2(Mathf.Cos(a0) * r * 1.05f, Mathf.Sin(a0) * r * .65f); b.Link(q + Vector2.up * .1f, p + Vector2.up * .1f, .03f, A(Rose, .8f * on), e.Seed, .5f); }
            }
            for (int k = 0; k < 10; k++) { float u = (H(e.Seed, k) + age * .15f) % 1f; Glint(b, at + new Vector2((H(e.Seed, k + 4) - .5f) * r * .9f, u * 3f), .13f, White, on * Mathf.Sin(u * 3.14f), k * 40); }
        }

        // the statue shatters: marble chunks and a gold flash, dust billows
        private static void StatueBurst(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 at = e.At; float r = Mathf.Max(1.4f, e.Radius) * 1.2f;
            b.Quad(FxShape.Glow, at, r * 2f, A(Gold, .8f * Fade(t, 0, .25f)), 0, e.Seed);
            b.Floor(FxShape.Cracks, at, r * 1.3f, A(Gold, 1f * Fade(t, .2f, .9f)), Rise(t, 0, .2f), e.Seed, 2);
            b.Floor(FxShape.Smoke, at, r * 1.3f, A(new Color(.7f, .7f, .8f), .6f * Fade(t, .1f, 1.1f)), Rise(t, 0, 1.1f), e.Seed + 2);
            for (int k = 0; k < 12; k++) { float u = t / (.55f + .3f * H(e.Seed, k)); if (u > 0 && u < 1) b.Quad(DKit.Shield, at + Dir(e.Seed, k, 12, .62f) * r * 1.4f * EaseOut(u) + Vector2.up * 1.7f * Mathf.Sin(u * 3.14f), new Vector2(.3f, .22f), t * 400 + k * 30, A(Marble, 1.2f * (1 - u * u)), 0, e.Seed + k, 1); }
        }

        // a green mending vine grows along the line as a twisted double strand with leaves, and little pulses of light run up it
        private static void VineMend(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 a = e.From, z = e.At; float grow = EaseOut(Rise(t, 0, .35f)), fade = Fade(t, .6f, 1.1f);
            Vector2 end = Vector2.Lerp(a, z, grow);
            b.Link(a, end, .5f, A(Leaf, .25f * fade), e.Seed, 1);
            DKit.Chain(b, a, end, .45f, 2.5f, t * 8, .14f, Leaf, fade, e.Seed, 18);
            DKit.Chain(b, a, end, .45f, 2.5f, t * 8 + 3.14f, .14f, new Color(.7f, 1f, .5f), fade, e.Seed + 40, 14);
            for (int k = 1; k < 12; k++)
            {
                float f = k / 12f; if (f > grow) break; Vector2 p = Vector2.Lerp(a, z, f); Vector2 n = new Vector2(-(z - a).y, (z - a).x).normalized;
                b.Quad(DKit.Star, p + n * .5f * (k % 2 * 2 - 1), .5f, DKit.Deg(n * (k % 2 * 2 - 1)), A(Leaf, 1.1f * fade), 0, e.Seed + k, -2);
                Glint(b, p, .3f, Gold, fade * .8f * (.5f + .5f * Mathf.Sin(t * 12 + k)), k * 40);
            }
            float pulse = Rise(t, .2f, .8f); if (pulse > 0 && pulse < 1) b.Quad(FxShape.Glow, Vector2.Lerp(a, z, pulse), 1.4f, A(new Color(.7f, 1f, .6f), .9f), 0, e.Seed);
        }

        // thorns burst out of the floor one after another along the line, tall green spikes with rose tips, the earth split under them
        private static void ThornSpikes(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 a = e.From, z = e.At; float fade = Fade(t, .55f, 1f); int n = Mathf.Clamp(Mathf.RoundToInt((z - a).magnitude * 1.6f), 6, 14);
            b.Link(a, Vector2.Lerp(a, z, EaseOut(Rise(t, 0, .3f))), .12f, A(new Color(.35f, .2f, .08f), .8f * fade), e.Seed, .4f);
            for (int k = 0; k < n; k++)
            {
                float st = k * .025f, u = EaseOut(Rise(t, st, st + .12f)); if (u <= 0) continue; float f = (k + .5f) / n; Vector2 p = Vector2.Lerp(a, z, f) + new Vector2(0, (H(e.Seed, k) - .5f) * .5f);
                float h = (1.3f + .9f * H(e.Seed, k + 5)) * (1 + .15f * Fade(t, st + .1f, st + .25f)), lean = (H(e.Seed, k + 9) - .5f) * 30, rad = (90 + lean) * Mathf.Deg2Rad;
                Vector2 up = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                b.Floor(FxShape.Glow, p, .9f, A(Rose, .4f * fade * u), 0, e.Seed);
                b.Floor(FxShape.Shade, p, .5f, A(DKit.Dark, .6f * fade), 0, e.Seed);
                b.Quad(DKit.Shield, p + up * h * .5f * u, new Vector2(h * .55f * u, .3f), 90 + lean + 180, A(Leaf, 1.15f * fade), 0, e.Seed + k, 1);
                b.Quad(DKit.Shield, p + up * h * .92f * u, new Vector2(.32f * u, .14f), 90 + lean + 180, A(Rose, 1.25f * fade), 0, e.Seed + k + 3, 1);
                if (u < 1) for (int j = 0; j < 3; j++) Glint(b, p + Dir(e.Seed + k, j, 3, .6f) * .5f * u + Vector2.up * .3f, .18f, Leaf, 1 - u, j * 60);
            }
        }

        // a thorn-ball bristles and clacks back: a spiky rose star whips round, thorns jab in eight directions
        private static void ThornReflect(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 at = e.At; float r = Mathf.Max(.9f, e.Radius), f = Fade(t, .3f, .9f), jab = EaseOut(Rise(t, 0, .15f));
            b.Quad(FxShape.Glow, at, r * 1.6f, A(Rose, .5f * f), 0, e.Seed);
            b.Quad(DKit.Star, at, r * 1.15f * (.6f + .4f * jab), t * 220, A(Rose, 1.2f * f), 0, e.Seed, 9);
            for (int k = 0; k < 8; k++) b.Quad(DKit.Shield, at + Dir(e.Seed, k, 8, .8f) * r * 1.0f * jab, new Vector2(r * .75f * jab, .14f), k * 45 + 10, A(Leaf, 1.1f * f), 0, e.Seed + k, 1);
        }

        // wrapped sweets rain onto the crowd, twist-ends spinning, and burst into pastel sugar swirls
        private static void CandyVolley(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 at = e.At; float r = Mathf.Max(1.2f, e.Radius) * 1.2f;
            for (int k = 0; k < 8; k++)
            {
                float start = k * .05f, u = (t - start) / .3f; if (u <= 0) continue; Color c = DKit.Pastel(k + e.Seed); Vector2 land = at + new Vector2((H(e.Seed, k) - .5f) * r * 1.6f, (H(e.Seed, k + 9) - .5f) * r * .7f);
                if (u < 1) { Vector2 p = land + Vector2.up * 6f * (1 - DKit.EaseIn(u)); b.Quad(FxShape.Disc, p, .45f, A(c, .55f), 0, e.Seed); b.Quad(DKit.Star, p + Vector2.right * .6f, new Vector2(.55f, .35f), t * 500, A(c, 1.1f), 0, e.Seed, -2); b.Quad(DKit.Star, p - Vector2.right * .6f, new Vector2(.55f, .35f), 180 + t * 500, A(c, 1.1f), 0, e.Seed, -2); }
                else { float v = u - 1; b.Quad(FxShape.Vortex, land, DKit.Sq(1.5f), v * 300, A(c, 1.2f * Fade(v, 0, 1.4f)), v, e.Seed + k, 1); b.Quad(FxShape.Glow, land, 1.8f, A(c, .4f * Fade(v, 0, 1)), 0, e.Seed); }
            }
        }

        // an origami crane stoops out of the sky onto the crowd, unfolds, and bursts into folded paper scraps
        private static void PaperCrane(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 at = e.At; const float dive = .22f;
            if (t < dive)
            {
                float u = t / dive; Vector2 p = at + new Vector2(-3f, 5f) * (1 - DKit.EaseIn(u)); float flap = Mathf.Sin(t * 70) * 25;
                b.Quad(DKit.Shield, p + new Vector2(.2f, .5f), new Vector2(1.2f, .7f), 160 + flap, A(Paper, 1.2f), 0, e.Seed, 1);
                b.Quad(DKit.Shield, p + new Vector2(.2f, -.5f), new Vector2(1.2f, .7f), -160 - flap, A(Paper, 1.2f), 0, e.Seed + 1, 1);
                b.Quad(DKit.Shield, p, new Vector2(1f, .4f), DKit.Deg(new Vector2(3, -5)), A(Rose, 1.2f), 0, e.Seed + 2, 1);
                b.Link(p - new Vector2(-3, 5).normalized * 1.4f, p, .05f, A(Paper, .5f), e.Seed, .6f); return;
            }
            float s = t - dive; b.Quad(FxShape.Glow, at, 2.6f, A(Paper, .7f * Fade(s, 0, .25f)), 0, e.Seed);
            for (int k = 0; k < 10; k++) { float u = s / (.5f + .3f * H(e.Seed, k)); if (u > 0 && u < 1) b.Quad(DKit.Shield, at + Dir(e.Seed, k, 10, .65f) * 2.4f * EaseOut(u) + Vector2.up * (1.8f * Mathf.Sin(u * 3.14f)), new Vector2(.5f, .35f), s * 500 + k * 40, A(k % 3 == 0 ? Rose : Paper, 1.2f * (1 - u)), 0, e.Seed + k, 1); }
            b.Floor(FxShape.Shock, at, 2.6f, A(Paper, .9f), Rise(s, 0, .5f), e.Seed);
        }

        // a heavy gauntlet punch: a comic impact star slams in, speed streaks lash out and a cone of three crescent shocks follows
        private static void Haymaker(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 at = e.At; float r = Mathf.Max(1.1f, e.Radius) * 1.25f, f = Fade(t, .3f, .8f);
            b.Quad(FxShape.Glow, at, r * 2f, A(Spark, .7f * Fade(t, 0, .2f)), 0, e.Seed);
            b.Quad(DKit.Star, at, r * 1.4f * (1.3f - .3f * EaseOut(Rise(t, 0, .1f))), t * 25 + 8, A(Gold, 1.2f * f), 0, e.Seed, 9);
            b.Quad(DKit.Star, at, r * .9f, -t * 30, A(White, 1.2f * f), 0, e.Seed + 1, 7);
            for (int k = 0; k < 3; k++) { float u = Rise(t, k * .05f, .5f + k * .05f); if (u > 0 && u < 1) b.Quad(DKit.Scythe, at, DKit.Sq(r * (1 + k * .5f) * (.6f + .6f * u)), 0, A(Spark, 1.1f * (1 - u)), u, e.Seed + k, 0); }
            for (int k = 0; k < 9; k++) { float u = Rise(t, 0, .25f); Vector2 d = Dir(e.Seed, k, 9, .7f); b.Link(at + d * r * (.5f + .5f * u), at + d * r * (1.2f + 1.2f * u), .07f, A(White, .9f * f), e.Seed + k, .6f); }
        }

        // a splash of bubbles: ring bubbles with glints float up and pop in a spray of water arcs
        private static void ShantySplash(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 at = e.At; float r = Mathf.Max(1.1f, e.Radius) * 1.2f;
            b.Floor(FxShape.Glow, at, r * 1.2f, A(Sea, .45f * Fade(t, 0, .5f)), 0, e.Seed);
            for (int k = 0; k < 12; k++)
            {
                float u = Rise(t, k * .025f, .8f + k * .02f); if (u <= 0 || u >= 1) continue; Vector2 p = at + new Vector2((H(e.Seed, k) - .5f) * r * 1.6f, (H(e.Seed, k + 9) - .5f) * r * .5f) + Vector2.up * u * 1.8f; float s = .35f + .35f * H(e.Seed, k + 4);
                b.Quad(FxShape.Shock, p, s * 1.8f, A(Sea, 1.1f * Fade(u, .8f, 1)), .33f, e.Seed + k);
                Glint(b, p + new Vector2(-.3f, .3f) * s, .2f, White, 1 - u, 0);
                if (u > .8f) for (int j = 0; j < 4; j++) Glint(b, p + Dir(e.Seed + k, j, 4, 1f) * s * 2 * (u - .8f) * 5, .1f, Sea, 1, 0);
            }
            for (int k = 0; k < 6; k++) { float u = t / (.5f + .1f * k); if (u > 0 && u < 1) b.Quad(FxShape.Glow, at + Dir(e.Seed, k, 6, .5f) * r * EaseOut(u) + Vector2.up * 1.6f * Mathf.Sin(u * 3.14f), .2f, A(Sea, 1 - u), 0, e.Seed); }
        }

        // a glass bubble pops: a blue sphere flares, glass shards fly in a fan of triangles with rainbow edge
        private static void BubbleBurst(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 at = e.At; float r = Mathf.Max(1.1f, e.Radius) * 1.25f;
            b.Quad(FxShape.Shock, at, DKit.Sq(r * 1.3f), 0, A(DKit.Steel, 1.1f * Fade(t, 0, .25f)), .35f, e.Seed);
            b.Quad(FxShape.Glow, at, r * 1.5f, A(Sea, .7f * Fade(t, 0, .2f)), 0, e.Seed);
            for (int k = 0; k < 9; k++) { float u = t / (.55f + .2f * H(e.Seed, k)); if (u > 0 && u < 1) b.Quad(DKit.Shield, at + Dir(e.Seed, k, 9, .65f) * r * 1.9f * EaseOut(u), new Vector2(.6f, .42f), DKit.Deg(Dir(e.Seed, k, 9, 1f)), A(k % 2 == 0 ? Sea : DKit.Ice, 1.2f * (1 - u * u)), 0, e.Seed + k, 1); }
            b.Floor(FxShape.Shock, at, r * 2f, A(Sea, .9f), Rise(t, 0, .6f), e.Seed);
        }

        // spinning shuriken blades orbit the crowd in a whirl, each with a crescent motion trail
        private static void Bladenado(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 at = e.At; float r = Mathf.Max(1.2f, e.Radius) * 1.5f, f = Fade(t, .8f, 1.3f), g = EaseOut(Rise(t, 0, .15f));
            b.Floor(FxShape.Glow, at, r * 1.2f, A(Iron, .3f * f), 0, e.Seed);
            for (int k = 0; k < 4; k++)
            {
                float a = k * 1.5708f + t * 11; Vector2 p = at + new Vector2(Mathf.Cos(a) * r * g, Mathf.Sin(a) * r * .62f * g) + Vector2.up * .35f;
                b.Quad(DKit.Star, p, .75f, t * 900, A(Iron, 1.2f * f), 0, e.Seed + k, 4);
            }
            b.Quad(DKit.Scythe, at + Vector2.up * .35f, Sq(r * 1.1f), 0, A(White, .8f * f), Mathf.Repeat(t * 1.7f, 1), e.Seed, 0);
            b.Quad(DKit.Scythe, at + Vector2.up * .35f, Sq(r * 1.1f), 180, A(Iron, .6f * f), Mathf.Repeat(t * 1.7f + .5f, 1), e.Seed + 1, 0);
        }

        // crush: a ring of grit snaps shut into a flat starburst, with pressure lines chevron-ing inwards
        private static void Crush(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 at = e.At; float r = Mathf.Max(1.1f, e.Radius) * 1.25f, c = Rise(t, 0, .25f);
            b.Floor(DKit.Hole, at, r * (1.6f - 1.1f * EaseOut(c)), A(DKit.Purple, 1.1f * Fade(t, .3f, .8f)), t, e.Seed, 1);
            for (int k = 0; k < 10; k++) { Vector2 d = Dir(e.Seed, k, 10, .7f); b.Link(at + d * r * (1.3f - 1.1f * EaseOut(c)), at + d * r * (1.6f - 1.3f * EaseOut(c)), .09f, A(DKit.Lavender, .9f * Fade(t, .15f, .5f)), e.Seed + k, .5f); }
            b.Quad(DKit.Star, at, Sq(r * .9f * Fade(t, .2f, .8f)), 0, A(White, 1.2f * Fade(t, .2f, .8f)), 0, e.Seed, 8);
            b.Floor(FxShape.Glow, at, r, A(DKit.Purple, .7f * Fade(t, .2f, .7f)), 0, e.Seed);
        }

        // horns ram: two bull-horn crescents slam together and canary stars spin off the bonk
        private static void HornRam(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 at = e.At; float r = Mathf.Max(1.1f, e.Radius) * 1.1f, close = 1 - EaseOut(Rise(t, 0, .12f));
            for (int s = -1; s <= 1; s += 2) b.Quad(DKit.Scythe, at + new Vector2(s * (.2f + 1.0f * close) * r, 0), Sq(r * 1.4f), s > 0 ? 180 : 0, A(Amber, 1.2f * Fade(t, .35f, .9f)), .85f, e.Seed + s, 0);
            b.Quad(FxShape.Glow, at, r * 1.8f, A(Gold, .8f * Fade(t, .05f, .35f)), 0, e.Seed);
            b.Floor(FxShape.Shock, at, r * 1.4f, A(Gold, .9f), Rise(t, .05f, .5f), e.Seed);
            for (int k = 0; k < 3; k++) { float a = t * 9 + k * 2.09f; b.Quad(DKit.Star, at + new Vector2(Mathf.Cos(a) * r * .9f, Mathf.Sin(a) * r * .45f + 1.1f), .42f, t * 300, A(Gold, 1.2f * Fade(t, .4f, 1)), 0, e.Seed + k, 5); }
        }

        // a heavy bash leaves dizzy canary stars orbiting an impact pad
        private static void DizzyStars(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 at = e.At; float r = Mathf.Max(1f, e.Radius), f = Fade(t, .55f, 1f);
            b.Quad(FxShape.Glow, at, r * 1.5f, A(Gold, .6f * Fade(t, 0, .25f)), 0, e.Seed);
            b.Floor(DKit.Hex, at, r * 1.5f, A(Gold, 1f * f), 0, e.Seed, 1);
            for (int k = 0; k < 5; k++) { float a = t * 8 + k * 1.2566f; b.Quad(DKit.Star, at + new Vector2(Mathf.Cos(a) * r * 1.1f, Mathf.Sin(a) * r * .45f + 1.2f), .45f, t * 200 + k * 30, A(k % 2 == 0 ? Gold : White, 1.2f * f), 0, e.Seed + k, 5); }
        }

        // a demolition cap goes off: a dusty mushroom of smoke with flying planks and a yellow-black flash
        private static void BlastCap(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 at = e.At; float r = Mathf.Max(1.1f, e.Radius) * 1.2f;
            b.Quad(FxShape.Glow, at, r * 1.8f, A(Spark, .8f * Fade(t, 0, .2f)), 0, e.Seed);
            for (int k = 0; k < 5; k++) b.Quad(FxShape.Smoke, at + new Vector2((k - 2) * .35f * r, 0) + Vector2.up * (EaseOut(Rise(t, 0, .9f)) * 1.5f + (k % 2) * .3f), new Vector2(r * .75f, r * .55f), 0, A(new Color(.55f, .48f, .42f), .95f * Fade(t, .2f, 1.1f)), t, e.Seed + k);
            b.Floor(FxShape.Shock, at, r * 1.5f, A(Spark, .9f), Rise(t, 0, .5f), e.Seed);
            for (int k = 0; k < 7; k++) { float u = t / (.5f + .2f * H(e.Seed, k)); if (u > 0 && u < 1) b.Quad(DKit.Shield, at + Dir(e.Seed, k, 7, .6f) * r * 1.4f * EaseOut(u) + Vector2.up * 1.8f * Mathf.Sin(u * 3), new Vector2(.6f, .18f), t * 500 + k * 60, A(Gold, 1.2f * (1 - u)), 0, e.Seed + k, 1); }
        }

        // a cog bursts into a ring of spinning gears and glints
        private static void GearBurst(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 at = e.At; float r = Mathf.Max(.9f, e.Radius), f = Fade(t, .3f, .9f);
            b.Quad(FxShape.Glow, at, r * 1.4f, A(Gold, .5f * f), 0, e.Seed);
            b.Quad(DKit.Star, at, r * 1.3f, t * 200, A(Gold, 1.2f * f), 0, e.Seed, 108);
            for (int k = 0; k < 6; k++) { float u = Rise(t, 0, .7f); b.Quad(DKit.Star, at + Dir(e.Seed, k, 6, .7f) * r * 2f * EaseOut(u), .55f, -t * 400, A(Iron, 1.1f * Fade(u, .5f, 1)), 0, e.Seed + k, 106); }
        }

        // a dash leaves a wedge of chevron wind that shoves the crowd back
        private static void DashWave(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 at = e.At; float r = Mathf.Max(1.1f, e.Radius) * 1.2f, f = Fade(t, .2f, .8f);
            for (int k = 0; k < 3; k++) { float u = Rise(t, k * .06f, .6f); if (u <= 0 || u >= 1) continue; b.Quad(DKit.Arcs, at + new Vector2(.95f * r * 1.1f, 0), new Vector2(r * 1.1f, r * .8f), k % 2 * 0, A(Sea, 1.2f * Fade(u, .4f, 1)), u, e.Seed + k, 1.3f); b.Quad(DKit.Arcs, at - new Vector2(.95f * r * 1.1f, 0), new Vector2(r * 1.1f, r * .8f), 180, A(Sea, 1.0f * Fade(u, .4f, 1)), u, e.Seed + k + 9, 1.3f); }
        }

        // horns: one great amber horn hooks up from under the enemy in an uppercut, flinging dust and a burst of hoof-shaped chips
        private static void HornGore(FxEffect e, FxBatch b, float now, float pre, float t)
        {
            if (t < 0) return; Vector2 at = e.At; float r = Mathf.Max(1f, e.Radius), up = EaseOut(Rise(t, 0, .18f)), f = Fade(t, .4f, .9f);
            b.Floor(FxShape.Smoke, at, r * 1.1f, A(new Color(.5f, .38f, .25f), .6f * f), Rise(t, 0, .9f), e.Seed);
            b.Quad(DKit.Scythe, at + Vector2.up * (up * .9f - .3f), new Vector2(r * .9f, r * 1.2f), 90, A(Amber, 1.2f * f), .35f + .5f * up, e.Seed, 0);
            b.Quad(FxShape.Glow, at + Vector2.up * .9f * up, r * 1.2f, A(Amber, .5f * Fade(t, .05f, .4f)), 0, e.Seed);
            for (int k = 0; k < 6; k++) { float u = Rise(t, .08f, .7f); if (u > 0 && u < 1) b.Quad(DKit.Shield, at + new Vector2((k - 2.5f) * .3f * r, 0) + Vector2.up * (2.4f * u - 2.2f * u * u + .3f), new Vector2(.2f, .14f), t * 500 + k * 50, A(Amber, 1.1f * (1 - u)), 0, e.Seed + k, 1); }
            for (int k = 0; k < 6; k++) b.Link(at + new Vector2((k - 2.5f) * .25f, -.2f), at + new Vector2((k - 2.5f) * .3f, 1.8f * up), .04f, A(White, .7f * Fade(t, .05f, .35f)), e.Seed + k, .6f);
        }
    }
}
