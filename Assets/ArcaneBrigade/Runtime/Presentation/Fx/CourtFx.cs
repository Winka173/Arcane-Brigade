using System;
using System.Collections.Generic;
using UnityEngine;
using static ArcaneBrigade.FxKit;

namespace ArcaneBrigade
{
    /// <summary>Court bond: gold seals, stamps, decrees, crowns and heraldry. court-seal = a brass stamp thumps the target and leaves a
    /// glowing wax rosette; court-decree = a parchment unrolls over the crowd and a giant seal slams down; court-royal-seal = a crown
    /// falls with two banners and showers gold coins; court-fanfare = five heraldic swallowtail banners unfurling from the top edge.</summary>
    public static class CourtFx
    {
        // owner 29/10/2026: small glints and sparks were drawn far too big: small ones shrink, big set-piece flashes keep their size
        private static void Glint(FxBatch b, Vector2 at, float size, Color tint, float alpha, float turn = 0)
            => FxKit.Glint(b, at, size <= .08f ? size : size < .5f ? .08f + (size - .08f) * .45f : size - .231f, tint, alpha, turn);
        public static void Register(Dictionary<string, Func<FxEffect>> map)
        {
            map["court-seal"] = () => new SealStamp();
            map["court-decree"] = () => new Decree();
            map["court-royal-seal"] = () => new RoyalSeal();
            map["court-fanfare"] = () => new Fanfare();
        }

        private const FxShape Drop = (FxShape)32, Seal = (FxShape)33, Crown = (FxShape)34, Ribbon = (FxShape)37, CardS = (FxShape)38;
        private static readonly Color Gold = new Color(1f, .8f, .24f), Crimson = new Color(.9f, .12f, .2f), Royal = new Color(.25f, .38f, 1f),
            Parch = new Color(1f, .7f, .36f), Wood = new Color(.42f, .2f, .07f), DustC = new Color(.78f, .68f, .54f);

        /// <summary>A low ring of floor dust kicked up by something heavy landing at <paramref name="at"/>.</summary>
        private static void Thump(FxBatch b, Vector2 at, float r, float t, int seed, int n = 7)
        {
            if (t <= 0 || t >= 1.2f) return;
            float u = t / 1.2f, e = EaseOut3(u);
            for (int k = 0; k < n; k++)
            {
                Vector2 d = Dir(seed, k, n, .5f);
                b.Quad(FxShape.Dust, at + d * r * (.35f + .75f * e) + Vector2.up * .25f * e, new Vector2(.55f + .5f * e, .32f + .25f * e) * (.8f + .4f * H(seed, k)), 0, A(DustC, .7f * (1 - u) * (1 - u)), u, seed + k);
            }
            b.Floor(FxShape.Dust, at, r * (.6f + .5f * e), A(DustC, .45f * (1 - u)), u, seed);
        }

        private static void Wax(FxBatch b, Vector2 at, float r, float t, int seed, int n, float height)
        {
            for (int k = 0; k < n * 2; k++)
            {
                Vector2 d = Dir(seed, k, n * 2, .75f); float life = .55f + .35f * H(seed, k + 4), u = t / life; if (u <= 0 || u >= 1) continue;
                Vector2 p = at + d * r * (.5f + .9f * H(seed, k + 9)) * EaseOut(u) + Vector2.up * (height * u * (1 - u) * 4 * .5f + .2f);
                b.Quad(Drop, p, .09f + .05f * H(seed, k + 2), Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg - 90 + u * 100, A(k % 3 == 0 ? Gold : Crimson, 1.15f * (1 - u * u)), 0, seed + k);
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
                b.Quad(FxShape.Glow, at + Vector2.up * .3f, r * 1.7f, A(Gold, .55f * Fade(t, 0, .25f)), 0, Seed);
                float pop = 1 + .35f * Fade(t, 0, .18f);
                b.Floor(FxShape.Glow, at, r * 1.6f, A(Crimson, .5f * imprint), 0, Seed);
                b.Floor(Seal, at, r * 1.3f * pop, A(Crimson, 1.3f * imprint), t, Seed);
                b.Floor(Seal, at, r * .95f * pop, A(Gold, .85f * imprint * (.8f + .2f * Mathf.Sin(now * 12))), t, Seed + 3);
                b.Floor(FxShape.Shock, at, r * 1.7f, A(Gold, .9f), Rise(t, 0, .5f), Seed);
                Wax(b, at, r * 1.4f, t, Seed, 10, 2f);
                Thump(b, at, r, t, Seed);
                for (int k = 0; k < 10; k++)
                {
                    float u = t / (.3f + .2f * H(Seed, k + 30)); if (u <= 0 || u >= 1) continue;
                    Vector2 d = Dir(Seed + 2, k, 10, .5f);
                    b.Link(at + d * r * (.4f + .9f * u), at + d * r * (.25f + .9f * u * .8f), .05f, A(k % 2 == 0 ? Gold : White, 1 - u), Seed + k, .7f);
                }
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
                    b.Quad(FxShape.Glow, at + Vector2.up * .4f, r * 1.9f, A(Gold, .55f * Fade(t, .1f, .45f)), 0, Seed);
                    Thump(b, at, r * 1.1f, t, Seed, 9);
                    float beat = Rise(t, .75f, .8f) * Fade(t, .8f, 1.25f);
                    b.Floor(Seal, at, r * (1.05f + .25f * Rise(t, .75f, 1.25f)), A(Gold, .7f * beat), t, Seed + 8);
                    for (int k = 0; k < 12; k++)
                    {
                        float u = (t - .05f) / (1.1f + .5f * H(Seed, k + 70)); if (u <= 0 || u >= 1) continue;
                        Vector2 d = Dir(Seed + 6, k, 12, .7f), p = at + d * r * 1.2f * EaseOut(u) + new Vector2(Mathf.Sin(u * 9 + k) * .25f, 2.2f * H(Seed, k + 71) * u - 1.6f * u * u + .5f);
                        b.Quad(CardS, p, new Vector2(.15f * (.2f + Mathf.Abs(Mathf.Cos(u * 12 + k))), .11f), u * 300 + k * 40, A(Parch, .9f * (1 - u)), 0, Seed + k, 0);
                    }
                    b.Floor(FxShape.Glow, at, r * 1.7f, A(Crimson, .5f * after * vis), 0, Seed);
                    b.Floor(Seal, at, r * 1.05f * scale, A(Crimson, 1.0f * after * vis), t, Seed);
                    b.Floor(Seal, at, r * .75f * scale, A(Gold, .7f * after * vis * (.85f + .15f * Mathf.Sin(now * 10))), t, Seed + 3);
                    b.Floor(FxShape.Shock, at, r * 1.45f, A(Gold, .95f), Rise(t, .1f, .95f), Seed);
                    b.Quad(FxShape.Rays, at + Vector2.up * .2f, r * 1.8f, A(Gold, .65f * Fade(t, .1f, .8f)), t, Seed, 4);
                    Wax(b, at, r * 1.55f, t - .1f, Seed, 18, 2.6f);
                    for (int k = 0; k < 14; k++) { float u = (t * .6f + H(Seed, k)) % 1; Glint(b, at + Dir(Seed + 4, k, 14, .65f) * r * (.3f + .9f * H(Seed, k + 5)) + Vector2.up * u * 1.5f, .24f, k % 2 == 0 ? Gold : White, .9f * after * Mathf.Sin(u * Mathf.PI), k * 33 + now * 80); }
                }
            }
            private void ScrollDraw(FxBatch b, Vector2 c, float W, float Hh, float unroll, float alpha, float now, int seed, float pre)
            {
                b.Quad(FxShape.Glow, c, new Vector2(W * 1.4f, Hh * 1.5f), 0, A(new Color(1f, .55f, .2f), .28f * alpha), 0, seed);
                b.Quad(FxShape.Shade, c, new Vector2(W * 1.1f, Hh * 1.25f) * Mathf.Max(.15f, unroll), 0, A(new Color(.12f, .06f, .02f), .75f * alpha), 0, seed, 0);
                b.Quad(CardS, c, new Vector2(W, Hh), 0, A(Parch, alpha), unroll, seed, 1);
                float edge = W * Mathf.Max(.05f, unroll * .86f) - .05f;
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector2 e = c + new Vector2(s * edge, 0);
                    // wooden roller with gold knobs, and a red ribbon tie fluttering off it
                    b.Quad(FxShape.Shade, e, new Vector2(.26f, Hh * 1.05f), 0, A(new Color(.16f, .07f, .02f), .95f * alpha), 0, seed + s, 0);
                    b.Link(e + new Vector2(-.05f, -Hh * .8f), e + new Vector2(-.05f, Hh * .8f), .035f, A(Parch * .7f, .55f * alpha), seed + s, .8f);
                    for (int y = -1; y <= 1; y += 2) { b.Quad(FxShape.Glow, e + Vector2.up * y * Hh * .92f, .3f, A(Gold, .6f * alpha), 0, seed); Glint(b, e + Vector2.up * y * Hh * .92f, .2f, Gold, alpha, now * 40 + y * 30); }
                    b.Quad(Ribbon, e + new Vector2(s * .25f, -Hh * .75f), new Vector2(.8f, .18f), -90 + s * 20 + Mathf.Sin(now * 8 + s) * 14, A(Crimson, 1.1f * alpha), now * 3, seed + s, 0);
                }
                for (int k = 0; k < 6; k++)
                {
                    float u = (now * 1.1f + k * .17f) % 1; if (u > unroll) continue;
                    Glint(b, c + new Vector2((u * 2 - 1) * W * .7f, (.43f - k * .143f) * Hh * 1.2f), .24f, Gold, .9f * alpha, now * 60 + k * 30);
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
                b.Quad(FxShape.Glow, at + Vector2.up * .5f, r * 1.8f, A(White, .55f * Fade(t, 0, .15f)), 0, Seed);
                b.Quad(FxShape.Rays, at + Vector2.up * .8f, r * 1.9f, A(Gold, .5f * Fade(t, 0, 1.0f)), t * .7f, Seed, 6);
                Thump(b, at, r, t, Seed + 5); Thump(b, at, r * .7f, t - .35f, Seed + 9, 5);
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
                    if (u > .82f) Glint(b, p, .45f * Mathf.Sin((u - .82f) / .18f * Mathf.PI), Gold, 1, k * 45);
                }
            }
        }

        /// <summary>Step 8 all-cast (1.5 s): a royal fanfare. Five heraldic banners on gold bars unfurl down from the top edge one after another,
        /// each a swallowtail cloth in crimson or royal blue with gold trim and a crown or seal emblem, gold tassels swinging off the bar ends;
        /// a gold shimmer runs down every banner, a modest sunburst glows above and gold dust drifts up the field.</summary>
        private sealed class Fanfare : FxEffect
        {
            protected override float Life => 1.5f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Rect a = Arena; float dur = Mathf.Max(.5f, Moment.Duration), g = Fade(t, dur * .72f, dur), rise = Rise(t, 0, .15f);
                Vector2 top = new Vector2(a.center.x, a.yMax + .4f);
                b.Quad(FxShape.Glow, top, new Vector2(4.2f, 2.2f), 0, A(Gold, .45f * rise * g), 0, Seed);
                b.Quad(FxShape.Rays, top, 3.6f, A(Gold, .65f * rise * g), t * .7f, Seed, 7);
                int n = 5;
                for (int k = 0; k < n; k++)
                {
                    float x = a.xMin + a.width * (k + .5f) / n, start = .06f * Mathf.Abs(k - 2), drop = EaseOut3(Rise(t, start, start + .38f));
                    float barY = a.yMax - .35f, half = 1.75f, hh = Mathf.Max(.06f, half * drop), sway = Mathf.Sin(t * 3.2f + k * 1.3f) * 2.5f * drop;
                    Color cloth = k % 2 == 0 ? Crimson : Royal;
                    Vector2 c = new Vector2(x, barY - hh);
                    b.Quad(FxShape.Glow, c, new Vector2(1.3f, hh * 1.2f + .3f), 0, A(cloth, .35f * g * rise), 0, Seed);
                    b.Quad(CardS, c, new Vector2(1.1f, hh), sway, A(cloth, 1.15f * g * rise), t * 1.4f, Seed + k, 2);
                    b.Quad(CardS, c, new Vector2(1.1f, hh), sway, A(Gold, 1.1f * g * rise), t * 1.4f, Seed + k, 3);
                    // the rolled cloth at the foot while it unfurls
                    if (drop < .98f) b.Quad(CardS, new Vector2(x, barY - 2 * hh), new Vector2(.68f, .14f * (1 - drop) + .05f), 0, A(cloth, 1.2f * g), 0, Seed + k, 0);
                    // emblem: a crown or a wax seal on the cloth
                    float em = Rise(drop, .7f, 1) * g;
                    Vector2 emp = new Vector2(x, barY - hh * 1.0f);
                    b.Quad(FxShape.Glow, emp, .7f, A(Gold, .35f * em), 0, Seed);
                    if (k % 2 == 0) b.Quad(Crown, emp, new Vector2(.5f, .5f), sway, A(Gold, 1.25f * em), 0, Seed + k, 0);
                    else b.Quad(Seal, emp, new Vector2(.44f, .44f), sway, A(Gold, 1.2f * em), t, Seed + k, 0);
                    // gold bar with finials and swinging tassels
                    b.Link(new Vector2(x - 1.15f, barY + .05f), new Vector2(x + 1.15f, barY + .05f), .08f, A(Gold, 1.1f * g * rise), Seed + k, .5f);
                    for (int s = -1; s <= 1; s += 2)
                    {
                        Vector2 tip = new Vector2(x + s * 1.18f, barY + .05f);
                        Glint(b, tip, .3f, White, .9f * g * rise, now * 60 + k * 20 + s * 45);
                        float swing = Mathf.Sin(now * 5 + k + s) * 18 * drop;
                        Vector2 dir = new Vector2(Mathf.Sin(swing * Mathf.Deg2Rad), -Mathf.Cos(swing * Mathf.Deg2Rad));
                        b.Link(tip, tip + dir * .45f, .025f, A(Gold, .9f * g * rise), Seed + s, .8f);
                        b.Quad(Drop, tip + dir * .55f, new Vector2(.09f, .13f), 180 + swing, A(Gold, 1.1f * g * rise), 0, Seed + k + s, 0);
                    }
                    // the shimmer: a band of light sweeping down the banner once it has unfurled
                    float sh = Rise(t, start + .42f, start + .8f);
                    if (sh > 0 && sh < 1) b.Quad(FxShape.Glow, new Vector2(x, barY - .2f - 3.2f * sh), new Vector2(1.1f, .3f), sway, A(White, .55f * Mathf.Sin(sh * Mathf.PI) * g), 0, Seed);
                }
                for (int k = 0; k < 36; k++)
                {
                    float u = (t * (.6f + .4f * H(Seed, k)) + H(Seed, k + 10)) % 1;
                    Vector2 p = new Vector2(a.xMin + a.width * H(Seed, k + 20) + Mathf.Sin(t * 6 + k) * .3f, a.yMin + u * a.height * .8f);
                    Glint(b, p, .16f + .08f * H(Seed, k + 30), k % 3 == 0 ? White : Gold, .85f * g * rise * Mathf.Sin(u * Mathf.PI), now * 70 + k * 20);
                }
            }
        }
    }
}
