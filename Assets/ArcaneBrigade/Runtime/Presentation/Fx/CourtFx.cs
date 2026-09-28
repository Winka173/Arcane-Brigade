using System;
using System.Collections.Generic;
using UnityEngine;
using static ArcaneBrigade.FxKit;

namespace ArcaneBrigade
{
    /// <summary>Court bond: gold seals, stamps, decrees, crowns and heraldry. court-seal = a brass stamp thumps the target and leaves a
    /// glowing wax rosette; court-decree = a parchment unrolls over the crowd and a giant seal slams down; court-royal-seal = a crown
    /// falls with two banners and showers gold coins; court-fanfare = a sunburst and heraldic banners dropping across the arena.</summary>
    public static class CourtFx
    {
        public static void Register(Dictionary<string, Func<FxEffect>> map)
        {
            map["court-seal"] = () => new SealStamp();
            map["court-decree"] = () => new Decree();
            map["court-royal-seal"] = () => new RoyalSeal();
            map["court-fanfare"] = () => new Fanfare();
        }

        private const FxShape Drop = (FxShape)32, Seal = (FxShape)33, Crown = (FxShape)34, Ribbon = (FxShape)37, CardS = (FxShape)38;
        private static readonly Color Gold = new Color(1f, .8f, .24f), Crimson = new Color(.9f, .12f, .2f), Royal = new Color(.25f, .38f, 1f), Ivory = new Color(1f, .92f, .68f);

        private static void Wax(FxBatch b, Vector2 at, float r, float t, int seed, int n, float height)
        {
            for (int k = 0; k < n; k++)
            {
                Vector2 d = Dir(seed, k, n, .75f); float life = .55f + .35f * H(seed, k + 4), u = t / life; if (u <= 0 || u >= 1) continue;
                Vector2 p = at + d * r * (.5f + .9f * H(seed, k + 9)) * EaseOut(u) + Vector2.up * (height * u * (1 - u) * 4 * .5f + .2f);
                b.Quad(Drop, p, .26f + .1f * H(seed, k + 2), Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg - 90 + u * 100, A(k % 3 == 0 ? Gold : Crimson, 1.15f * (1 - u * u)), 0, seed + k);
            }
        }

        /// <summary>Step 2: a gold-and-brass rubber stamp plummets onto the enemy and thumps down; it lifts away leaving a glowing wax
        /// rosette on the floor while red wax spatters and a small gold shock jumps out.</summary>
        private sealed class SealStamp : FxEffect
        {
            protected override float Life => 1.7f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Mathf.Max(.9f, Radius);
                if (t < 0)
                {
                    float u = pre * pre; Vector2 p = Vector2.Lerp(at + Vector2.up * 5.4f, at + Vector2.up * .45f, u);
                    b.Floor(FxShape.Shade, at, r * 1.2f * pre, A(new Color(.1f, .02f, .02f), .5f * pre), 0, Seed);
                    DrawStamp(b, p, 1, Seed);
                    return;
                }
                float press = t < .2f ? 0 : EaseOut(Rise(t, .2f, .55f)), lift = 2.5f * press * press, imprint = Fade(t, 1.1f, 1.7f);
                Vector2 sp = at + Vector2.up * (.45f + lift);
                if (t < .6f) DrawStamp(b, sp, Fade(t, .3f, .6f) + .01f, Seed);
                b.Quad(FxShape.Glow, at + Vector2.up * .3f, r * 2f, A(Gold, .8f * Fade(t, 0, .25f)), 0, Seed);
                float pop = 1 + .35f * Fade(t, 0, .18f);
                b.Floor(FxShape.Glow, at, r * 1.6f, A(Crimson, .5f * imprint), 0, Seed);
                b.Floor(Seal, at, r * 1.3f * pop, A(Crimson, 1.3f * imprint), t, Seed);
                b.Floor(Seal, at, r * .95f * pop, A(Gold, .85f * imprint * (.8f + .2f * Mathf.Sin(now * 12))), t, Seed + 3);
                b.Floor(FxShape.Shock, at, r * 1.7f, A(Gold, .9f), Rise(t, 0, .5f), Seed);
                Wax(b, at, r * 1.4f, t, Seed, 10, 2f);
                for (int k = 0; k < 8; k++) { float u = (t * .8f + H(Seed, k)) % 1; Glint(b, at + Dir(Seed, k, 8, .7f) * r * .9f + Vector2.up * u * 1.2f, .22f, Gold, .9f * imprint * Mathf.Sin(u * Mathf.PI), k * 40 + now * 90); }
            }
            private static void DrawStamp(FxBatch b, Vector2 p, float alpha, int seed)
            {
                b.Quad(FxShape.Glow, p + Vector2.up * .5f, 1.6f, A(Gold, .45f * alpha), 0, seed);
                b.Quad(Seal, p + Vector2.up * 1.25f, new Vector2(.42f, .42f), 0, A(Crimson, alpha), 0, seed, 0);          // round knob
                b.Quad(CardS, p + Vector2.up * .65f, new Vector2(.2f, .42f), 0, A(Gold, alpha), 0, seed + 1, 0);           // neck
                b.Quad(CardS, p, new Vector2(1.0f, .26f), 0, A(Gold, alpha), 0, seed + 2, 0);                              // brass plate
                b.Quad(CardS, p + Vector2.down * .18f, new Vector2(.85f, .12f), 0, A(Crimson, alpha), 0, seed + 3, 0);
            }
        }

        /// <summary>Step 6: a golden parchment drifts over the crowd and unrolls, red ribbon ties flapping and its lines of writing lighting
        /// up in turn; then a giant wax seal slams onto the middle, flinging wax and a golden shock while the scroll sinks and fades.</summary>
        private sealed class Decree : FxEffect
        {
            protected override float Life => 2.8f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Radius, W = r * 1.15f, Hh = r * .75f;
                if (t < 0)
                {
                    float e = EaseOut(pre), unroll = EaseOut(Rise(pre, .05f, .75f));
                    Vector2 c = Vector2.Lerp(at + Vector2.up * 5.8f, at + Vector2.up * (r * .95f + 1.1f), e);
                    ScrollDraw(b, c, W, Hh, unroll, 1, now, Seed, pre);
                    b.Floor(FxShape.Shade, at, r * 1.2f * pre, A(new Color(.12f, .04f, .02f), .45f * pre), 0, Seed);
                    return;
                }
                float sink = Rise(t, .3f, 1.1f), keep = Fade(t, .5f, 1.4f);
                Vector2 sc = at + Vector2.up * (r * .95f + 1.1f - (r * .95f + .2f) * sink);
                if (keep > 0) ScrollDraw(b, sc, W, Hh, 1, keep, now, Seed, 1);
                float s0 = EaseOut(Rise(t, 0, .13f)), scale = Mathf.Lerp(2.4f, 1f, s0), after = Fade(t, 1.8f, 2.8f);
                if (t > 0)
                {
                    float vis = Rise(t, 0, .04f);
                    b.Quad(FxShape.Glow, at + Vector2.up * .4f, r * 2.6f, A(Gold, .9f * Fade(t, .1f, .45f)), 0, Seed);
                    b.Floor(FxShape.Glow, at, r * 1.7f, A(Crimson, .5f * after * vis), 0, Seed);
                    b.Floor(Seal, at, r * 1.05f * scale, A(Crimson, 1.3f * after * vis), t, Seed);
                    b.Floor(Seal, at, r * .75f * scale, A(Gold, .9f * after * vis * (.85f + .15f * Mathf.Sin(now * 10))), t, Seed + 3);
                    b.Floor(FxShape.Shock, at, r * 1.45f, A(Gold, .95f), Rise(t, .1f, .95f), Seed);
                    b.Quad(FxShape.Rays, at + Vector2.up * .2f, r * 2.3f, A(Gold, .7f * Fade(t, .1f, .8f)), t, Seed, 4);
                    Wax(b, at, r * 1.55f, t - .1f, Seed, 18, 2.6f);
                    for (int k = 0; k < 14; k++) { float u = (t * .6f + H(Seed, k)) % 1; Glint(b, at + Dir(Seed + 4, k, 14, .65f) * r * (.3f + .9f * H(Seed, k + 5)) + Vector2.up * u * 1.5f, .24f, k % 2 == 0 ? Gold : White, .9f * after * Mathf.Sin(u * Mathf.PI), k * 33 + now * 80); }
                }
            }
            private void ScrollDraw(FxBatch b, Vector2 c, float W, float Hh, float unroll, float alpha, float now, int seed, float pre)
            {
                b.Quad(FxShape.Glow, c, new Vector2(W * 1.5f, Hh * 1.6f), 0, A(Gold, .35f * alpha), 0, seed);
                b.Quad(CardS, c, new Vector2(W, Hh), 0, A(Ivory, 1.15f * alpha), unroll, seed, 1);
                float edge = W * unroll * .9f;
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector2 e = c + new Vector2(s * edge, 0);
                    b.Quad(Ribbon, e + new Vector2(s * .2f, -Hh * .6f), new Vector2(.95f, .22f), -90 + Mathf.Sin(now * 8 + s) * 14, A(Crimson, 1.2f * alpha), now * 3, seed + s, 0);
                }
                for (int k = 0; k < 6; k++)
                {
                    float u = (now * 1.1f + k * .17f) % 1; if (u > unroll) continue;
                    Glint(b, c + new Vector2((u * 2 - 1) * W * .75f, (-.45f + k * .17f) * Hh), .28f, Gold, .95f * alpha, now * 60 + k * 30);
                }
            }
        }

        /// <summary>Step 8: a crown tumbles down trailing a crimson and a blue banner, lands and bounces on the crowd, throws a gold
        /// sunburst and showers spinning coins while a glowing royal sigil is branded on the floor beneath it.</summary>
        private sealed class RoyalSeal : FxEffect
        {
            protected override float Life => 2.2f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Mathf.Max(1.2f, Radius);
                if (t < 0)
                {
                    float u = pre * pre; Vector2 p = Vector2.Lerp(at + new Vector2(-.6f, 5.8f), at + Vector2.up * 1.0f, u);
                    float tilt = Mathf.Sin(pre * 9) * 22 * (1 - pre);
                    b.Floor(FxShape.Shade, at, r * 1.2f * pre, A(new Color(.12f, .08f, .0f), .45f * pre), 0, Seed);
                    b.Quad(Ribbon, p + new Vector2(-.55f, 1.2f), new Vector2(1.5f, .32f), 90 + tilt, A(Crimson, 1.2f), now * 4, Seed, 0);
                    b.Quad(Ribbon, p + new Vector2(.55f, 1.2f), new Vector2(1.5f, .32f), 90 - tilt, A(Royal, 1.2f), now * 4, Seed + 3, 0);
                    b.Quad(FxShape.Glow, p, 1.5f, A(Gold, .55f), 0, Seed);
                    b.Quad(Crown, p, new Vector2(.85f, .85f), tilt, A(Gold, 1.25f), 0, Seed, 0);
                    for (int k = 1; k <= 5; k++) Glint(b, p + new Vector2(Mathf.Sin(now * 9 + k) * .5f, 1f + k * .22f), .22f, Gold, .9f * (1 - k / 6f), k * 50 + now * 80);
                    return;
                }
                float hop = t < .35f ? 4 * .75f * (t / .35f) * (1 - t / .35f) : 0, fade = Fade(t, 1.3f, 2.1f), seal = Fade(t, 1.4f, 2.2f);
                Vector2 c = at + Vector2.up * (.9f + hop);
                b.Quad(FxShape.Glow, at + Vector2.up * .5f, r * 2.2f, A(White, .6f * Fade(t, 0, .15f)), 0, Seed);
                b.Quad(FxShape.Rays, at + Vector2.up * .8f, r * 2.6f, A(Gold, .5f * Fade(t, 0, 1.0f)), t * .7f, Seed, 6);
                b.Floor(FxShape.Glow, at, r * 1.6f, A(Gold, .25f * seal), 0, Seed);
                b.Floor(Seal, at, r * 1.45f * (1 + .1f * Fade(t, 0, .3f)), A(Gold, .65f * seal * Rise(t, 0, .1f)), t, Seed);
                b.Floor(Seal, at, r * 1.0f, A(Crimson, .9f * seal * Rise(t, 0, .1f)), t, Seed + 2);
                b.Quad(FxShape.Glow, c, 1.8f, A(Gold, .3f * fade), 0, Seed);
                b.Quad(Crown, c + Vector2.up * .4f, new Vector2(1.25f, 1.25f), Mathf.Sin(t * 20) * 4 * Fade(t, 0, .5f), A(Gold, 1.25f * fade), 0, Seed, 0);
                // banners fall away to the sides
                for (int s = -1; s <= 1; s += 2)
                {
                    float u = Rise(t, 0, 1.1f);
                    b.Quad(Ribbon, c + new Vector2(s * (.8f + 1.0f * EaseOut(u)), 1.0f - 1.7f * u * u), new Vector2(1.4f, .32f), 90 + s * 35 * u, A(s < 0 ? Crimson : Royal, 1.2f * fade), now * 4, Seed + s, 0);
                }
                // spinning coins
                for (int k = 0; k < 16; k++)
                {
                    Vector2 d = Dir(Seed, k, 16, .8f); float life = .85f + .35f * H(Seed, k + 4), u = t / life; if (u <= 0 || u >= 1) continue;
                    Vector2 p = c + d * r * 1.35f * EaseOut(u) + Vector2.up * (3.4f * H(Seed, k + 9) * u - 4.6f * u * u);
                    float sz = .26f + .08f * H(Seed, k + 2), flip = Mathf.Abs(Mathf.Cos(t * 11 + k * 1.7f));
                    b.Quad(Seal, p, new Vector2(sz * (.12f + flip), sz), 0, A(Gold, 1.3f * (1 - u * u)), 0, Seed + k, 0);
                    Glint(b, p, .3f * (1 - flip) + .05f, White, .8f * (1 - u), k * 30 + now * 100);
                }
            }
        }

        /// <summary>Step 8 weather: a trumpet fanfare. A golden sunburst opens over the arena, heraldic banners with crowns and seals
        /// drop from the top edge in a ripple, and gold dust floats up.</summary>
        private sealed class Fanfare : FxEffect
        {
            protected override float Life => 1.5f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Rect a = Arena; float dur = Mathf.Max(.5f, Moment.Duration), g = Fade(t, dur * .72f, dur), rise = Rise(t, 0, .15f);
                b.Quad(FxShape.Shade, a.center, new Vector2(a.width * .75f, a.height * .75f), 0, A(new Color(.12f, .06f, 0f), .28f * rise * g), 0, Seed);
                Vector2 top = new Vector2(a.center.x, a.yMax + 1.5f);
                b.Quad(FxShape.Glow, top, new Vector2(a.width * .5f, 6f), 0, A(Gold, .5f * rise * g), 0, Seed);
                b.Quad(FxShape.Rays, top, new Vector2(a.width * .55f, a.width * .55f), 0, A(Gold, .8f * rise * g), t * .7f, Seed, 7);
                int n = 7;
                for (int k = 0; k < n; k++)
                {
                    float x = a.xMin + a.width * (k + .5f) / n, drop = EaseOut3(Rise(t, .04f * Mathf.Abs(k - 3), .04f * Mathf.Abs(k - 3) + .3f));
                    float len = 4.6f * drop; if (len < .1f) continue;
                    Color c = k % 2 == 0 ? Crimson : Royal;
                    b.Quad(Ribbon, new Vector2(x, a.yMax + .3f - len * .5f), new Vector2(len * .5f, 1.15f), -90 + Mathf.Sin(t * 5 + k) * 3, A(c, 1.2f * g), t * 2 + k, Seed + k, .5f);
                    Vector2 foot = new Vector2(x, a.yMax + .3f - len);
                    b.Quad(FxShape.Glow, foot, 1.6f, A(Gold, .4f * g), 0, Seed);
                    b.Quad(Crown, foot + Vector2.up * .75f, new Vector2(.9f, .9f), 0, A(Gold, 1.25f * g), 0, Seed + k, 0);
                    b.Quad(Seal, foot + Vector2.down * .45f, new Vector2(.7f, .7f), 0, A(Gold, 1.1f * g), 0, Seed + k, 0);
                    Glint(b, foot + new Vector2(.5f, .9f), .4f * (.7f + .3f * Mathf.Sin(now * 14 + k)), White, .9f * g * drop, now * 50 + k * 20);
                }
                for (int k = 0; k < 50; k++)
                {
                    float u = (t * (.7f + .5f * H(Seed, k)) + H(Seed, k + 10)) % 1;
                    Vector2 p = new Vector2(a.xMin + a.width * H(Seed, k + 20) + Mathf.Sin(t * 6 + k) * .3f, a.yMin + u * a.height);
                    Glint(b, p, .2f, k % 3 == 0 ? White : Gold, .95f * g * rise * Mathf.Sin(u * Mathf.PI), now * 70 + k * 20);
                }
            }
        }
    }
}
