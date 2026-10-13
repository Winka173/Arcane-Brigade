using System;
using System.Collections.Generic;
using UnityEngine;
using static ArcaneBrigade.FxKit;
using static ArcaneBrigade.AKit;

namespace ArcaneBrigade
{
    /// <summary>Tide bond: water, splashes, foam and currents.</summary>
    public static class TideFx
    {
        // owner 29/10/2026: small glints and sparks were drawn far too big: small ones shrink, big set-piece flashes keep their size
        private static void Glint(FxBatch b, Vector2 at, float size, Color tint, float alpha, float turn = 0)
            => FxKit.Glint(b, at, size <= .08f ? size : size < .5f ? .08f + (size - .08f) * .45f : size - .231f, tint, alpha, turn);
        public static void Register(Dictionary<string, Func<FxEffect>> map)
        {
            map["tide-splash"] = () => new Splash();
            map["riptide"] = () => new Riptide();
            map["tide-whirlpool"] = () => new Whirlpool();
            map["tide-whirlpool-burst"] = () => new Geyser();
            map["tide-leviathan"] = () => new Leviathan();
            map["tide-rain"] = () => new Rain();
        }
        private static readonly Color Aqua = new Color(.25f, .8f, 1f), Sea = new Color(.08f, .45f, .9f), Foam = new Color(.9f, 1f, 1f), Navy = new Color(.01f, .05f, .14f);

        /// <summary>A droplet flying on a gravity arc, its point turned along its motion.</summary>
        private static void Drip(FxBatch b, Vector2 from, Vector2 vel, float g, float t, float life, float size, Color c, int seed)
        {
            float u = t / life; if (u <= 0 || u >= 1) return;
            Vector2 p = from + vel * t + Vector2.down * g * t * t * .5f, v = vel + Vector2.down * g * t;
            b.Quad(Drop, p, new Vector2(size * .75f, size), Ang(v) - 90, A(c, 1 - u * u), 0, seed, 0);
        }

        /// <summary>A soaked enemy: a column of water slaps up out of it and collapses into a crown of drops falling back, foam bubbles rise, a wet sheen spreads.</summary>
        private sealed class Splash : FxEffect
        {
            protected override float Life => 1.3f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                float r = Mathf.Clamp(Radius, 1.1f, 2.2f); Vector2 at = At;
                b.Floor(FxShape.Glow, at, r * 1.5f, A(Sea, .55f * Fade(t, .2f, 1.3f)), 0, Seed);
                b.Floor(FxShape.Smoke, at, r * (.6f + .7f * EaseOut(Rise(t, 0, .5f))), A(new Color(.7f, .92f, 1f), .45f * Fade(t, .3f, 1.2f)), t, Seed);
                float col = Mathf.Sin(Mathf.Clamp01(t / .45f) * Mathf.PI);
                for (int k = 0; k < 3; k++) Puff(b, at + new Vector2((k - 1) * r * .5f, r * (.6f + .8f * t)), r * (.4f + .4f * t), new Color(.8f, .95f, 1f), .35f * Rise(t, .1f, .3f) * Fade(t, .4f, 1.2f), t + k, Seed + 20 + k);
                for (int k = 0; k < 10; k++)
                {
                    float land = .55f + .45f * H(Seed, k + 40), a = (t - land) / .35f; if (a <= 0 || a >= 1) continue;
                    Vector2 p = at + Pt(Seed, k + 40, r * 1.5f, r * .9f);
                    b.Floor(FxShape.Glow, p, .4f, A(Aqua, .6f * (1 - a)), 0, Seed);
                    for (int j = -1; j <= 1; j += 2) Drip(b, p, new Vector2(j * .9f, 1.8f), 12, a * .3f, .3f, .09f, Foam, Seed + k * 3 + j);
                }
                for (int k = 0; k < 10; k++)
                {
                    Vector2 dd = Dir(Seed + 3, k, 10, .7f);
                    Fleck(b, at + Vector2.up * r * .5f, new Vector2(dd.x * 3.5f, 4 + 2 * H(Seed, k + 50)), 12, t - .1f, .6f, .06f, Foam, Seed + k + 50);
                }
                for (int k = 0; k < 4; k++)
                {
                    float h = (.7f + k * .7f) * r * col, s = r * (.95f - k * .16f) * (.4f + .6f * col);
                    b.Quad(Drop, at + Vector2.up * h, new Vector2(s * .8f, s), Mathf.Sin(t * 20 + k) * 8, A(k % 2 == 0 ? Aqua : Foam, .9f * col), 0, Seed + k, 0);
                }
                for (int k = 0; k < 34; k++)
                {
                    Vector2 dd = Dir(Seed, k, 34, .6f);
                    Drip(b, at + Vector2.up * .4f * r, new Vector2(dd.x * (2.2f + 1.6f * H(Seed, k)) * r, 3.5f + 2.5f * H(Seed, k + 5) + dd.y * 1.5f), 13, t - .12f - .03f * H(Seed, k + 9), .8f, .1f + .05f * H(Seed, k + 3), k % 3 == 0 ? Foam : Aqua, Seed + k);
                }
                for (int k = 0; k < 16; k++)
                {
                    float u = Rise(t, .1f + k * .025f, .9f + k * .025f); if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + new Vector2((H(Seed, k + 30) - .5f) * r * 1.6f + Mathf.Sin(t * 8 + k) * .15f, .2f + u * 2f);
                    b.Quad(Bubble, p, .08f + .07f * H(Seed, k), A(Foam, .9f * (1 - u)), 0, Seed + k);
                }
            }
        }

        /// <summary>The riptide drags enemies: a flat spiral current turns on the floor, six water ribbons stream inward along it carrying foam beads, a dark dip opens in the middle.</summary>
        private sealed class Riptide : FxEffect
        {
            protected override float Life => 1.4f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                float r = Mathf.Clamp(Radius, 1.2f, 2.4f) * 1.35f, on = Rise(t, 0, .2f) * Fade(t, .9f, 1.4f); Vector2 at = At;
                float gulp = Rise(t, .55f, .6f) * Fade(t, .6f, 1.1f);
                for (int k = 0; k < 12; k++)
                {
                    float u = Rise(t, .55f + k * .02f, 1.05f + k * .02f); if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + new Vector2((H(Seed, k + 70) - .5f) * r * .5f, .1f + u * (1.2f + H(Seed, k + 71)));
                    b.Quad(Bubble, p, .07f + .06f * H(Seed, k + 72) * (1 + u), A(Foam, .9f * (1 - u)), 0, Seed + k);
                }
                Puff(b, at + Vector2.up * (.2f + .6f * gulp), r * .45f, new Color(.75f, .92f, 1f), .4f * gulp, t, Seed + 9);
                for (int k = 0; k < 10; k++)
                {
                    float th = k / 10f * 6.2832f + t * 2.5f; Vector2 rim = at + new Vector2(Mathf.Cos(th) * r * 1.1f, Mathf.Sin(th) * r * .66f);
                    Fleck(b, rim, new Vector2(-Mathf.Sin(th) * 1.5f, 2.2f), 9, (t * 1.4f + k * .1f) % .5f, .5f, .06f, Foam, Seed + k + 40);
                }
                b.Floor(FxShape.Shade, at, r * 1.2f, A(Navy, .6f * on), 0, Seed);
                b.Floor(Swirl, at, r * 1.25f, A(Aqua, .7f * on), t * 3.5f, Seed, -2);
                b.Floor(FxShape.Shade, at, r * .35f, A(new Color(0, .02f, .06f), .9f * on), 0, Seed);
                for (int k = 0; k < 6; k++)
                {
                    float th0 = k / 6f * 6.2832f + t * 2.5f; Vector2 prev = Vector2.zero;
                    for (int s = 0; s <= 9; s++)
                    {
                        float f = s / 9f, rho = r * (1.15f - f * .95f), th = th0 + f * 2.4f;
                        Vector2 p = at + new Vector2(Mathf.Cos(th) * rho, Mathf.Sin(th) * rho * .6f);
                        if (s > 0) b.Link(prev, p, .14f * (1.2f - f), A(s % 2 == 0 ? Aqua : Foam, .85f * on * (1 - f * .5f)), Seed + k * 10 + s, .5f);
                        prev = p;
                    }
                    for (int j = 0; j < 5; j++)
                    {
                        float f = (t * 1.1f + j / 5f + k * .13f) % 1, rho = r * (1.15f - f * .95f), th = th0 + f * 2.4f;
                        b.Quad(Bubble, at + new Vector2(Mathf.Cos(th) * rho, Mathf.Sin(th) * rho * .6f), .12f * (1 - f * .4f), A(Foam, on * (1 - f)), 0, Seed + j);
                    }
                }
            }
        }

        /// <summary>Step 6 zone. A flat whirlpool: three swirl layers turning against each other around a black eye, a foam rim, drops and bubbles spiralling in, spray flung up at the edge.</summary>
        private sealed class Whirlpool : FxEffect
        {
            protected override float Life => Moment.Kind == BondMomentKind.Zone ? Mathf.Max(1f, Moment.Duration) + .2f : 2f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                float on = Env(this, now, .5f, .4f); if (on <= 0) return;
                float R = Radius * .95f * (.6f + .4f * EaseOut(Rise(now - Born, 0, .6f))); Vector2 at = At;
                for (int k = 0; k < 4; k++)
                {
                    float a = now * .9f + k * 1.57f;
                    Puff(b, at + new Vector2(Mathf.Cos(a) * R * 1.1f, Mathf.Sin(a) * R * .68f + .3f), R * .45f, new Color(.78f, .93f, 1f), .3f * on, now * .5f + k, Seed + 40 + k);
                }
                for (int k = 0; k < 14; k++)
                {
                    float cyc = .7f, ph = (now + k * .05f) / cyc, u = ph - Mathf.Floor(ph), a = k * .45f + Mathf.Floor(ph) * 1.7f;
                    Fleck(b, at + new Vector2(Mathf.Cos(a) * R * 1.12f, Mathf.Sin(a) * R * .7f), new Vector2(-Mathf.Sin(a) * 1.8f, 2.2f), 9, u * .5f, .5f, .055f, Foam, Seed + k + 60);
                }
                b.Floor(FxShape.Shade, at, R * 1.45f, A(Navy, .7f * on), 0, Seed);
                b.Floor(FxShape.Glow, at, R * 1.5f, A(Sea, .45f * on), 0, Seed);
                b.Floor(Swirl, at, R * 1.4f, A(Aqua, .6f * on), now * 1.6f, Seed, 3);
                b.Floor(Swirl, at, R * 1f, A(Foam, .55f * on), now * 2.6f, Seed + 5, -1);
                b.Floor(Swirl, at, R * .6f, A(Aqua, .8f * on), now * 4f, Seed + 9, 1);
                b.Floor(FxShape.Shade, at, R * .3f, A(new Color(0, .01f, .04f), .95f * on), 0, Seed);
                for (int k = 0; k < 44; k++)
                {
                    float a = k / 44f * 6.2832f + now * 1.4f, rr = R * (1.18f + .06f * Mathf.Sin(k * 3 + now * 5));
                    b.Quad(Bubble, at + new Vector2(Mathf.Cos(a) * rr, Mathf.Sin(a) * rr * .62f), .08f + .05f * H(Seed, k), A(Foam, .85f * on), 0, Seed + k);
                }
                for (int k = 0; k < 20; k++)
                {
                    float f = (H(Seed, k) + now * .45f) % 1, a = k * 2.1f + f * 7f, rho = R * (1.1f - f), s = .13f * (1 - f * .5f);
                    Vector2 p = at + new Vector2(Mathf.Cos(a) * rho, Mathf.Sin(a) * rho * .62f), tan = new Vector2(-Mathf.Sin(a), Mathf.Cos(a) * .62f);
                    if (k % 3 == 0) b.Quad(Bubble, p, s * .8f, A(Foam, on * (1 - f)), 0, Seed + k);
                    else b.Quad(Drop, p, new Vector2(s * .75f, s), Ang(tan) - 90, A(Aqua, on * (1 - f)), 0, Seed + k, 0);
                }
                for (int k = 0; k < 16; k++)
                {
                    float cyc = .8f + .3f * H(Seed, k), u = ((now + H(Seed, k + 3) * cyc) % cyc) / cyc, a = H(Seed, k + 7) * 6.2832f + Mathf.Floor((now + H(Seed, k + 3) * cyc) / cyc) * 2.3f;
                    Vector2 rim = at + new Vector2(Mathf.Cos(a) * R * 1.15f, Mathf.Sin(a) * R * .7f);
                    Drip(b, rim, new Vector2(Mathf.Cos(a) * 1.2f, 3.2f), 11, u * .6f, .6f, .1f, Foam, Seed + k);
                }
            }
        }

        /// <summary>The whirlpool's end: a geyser of water bursts out of the eye, a flood crest of drops runs outward over the crowd, foam spreads and spray rains down.</summary>
        private sealed class Geyser : FxEffect
        {
            protected override float Life => 1.8f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                float r = Mathf.Max(1.6f, Radius); Vector2 at = At;
                float up = EaseOut3(Rise(t, 0, .25f)), fall = Fade(t, .55f, 1.2f);
                b.Floor(FxShape.Glow, at, r * 1.6f, A(Sea, .6f * Fade(t, 0, 1.6f)), 0, Seed);
                b.Floor(FxShape.Smoke, at, r * (.4f + 1f * EaseOut(Rise(t, 0, .9f))), A(new Color(.75f, .93f, 1f), .5f * Fade(t, .2f, 1.6f)), t, Seed);
                b.Floor(Swirl, at, r * .8f * fall, A(Foam, .7f * fall), t * 5, Seed, 2);
                for (int k = 0; k < 7; k++)
                {
                    float f = k / 6f, h = f * 4.2f * up * (1 - (1 - fall) * f), s = r * (.62f - f * .38f);
                    if (k % 2 == 1) Puff(b, at + new Vector2(Mathf.Sin(k * 2.3f) * r * .4f, h + .2f), s * 1.3f, new Color(.8f, .95f, 1f), .35f * fall * Rise(t, .1f, .3f), t + k, Seed + 30 + k);
                    Vector2 p = at + new Vector2(Mathf.Sin(t * 14 + k) * .12f, h);
                    b.Quad(Drop, p, new Vector2(s * .85f, s * 1.1f), 0, A(k % 2 == 0 ? Aqua : Foam, .9f * fall), 0, Seed + k, 0);
                    b.Quad(FxShape.Glow, p, s * 1.6f, A(Aqua, .25f * fall), 0, Seed);
                }
                float crest = EaseOut(Rise(t, .05f, .8f)), cAl = Fade(t, .6f, 1.1f) * Rise(t, .05f, .1f);
                for (int k = 0; k < 36; k++)
                {
                    Vector2 dd = Dir(Seed + 4, k, 36, .62f), p = at + dd * r * (.3f + 1.05f * crest);
                    b.Quad(Drop, p + Vector2.up * (.1f + .15f * H(Seed, k + 3)), new Vector2(.1f, .15f), dd.x * -25, A(k % 2 == 0 ? Foam : Aqua, .9f * cAl), 0, Seed + k, 0);
                    b.Quad(Bubble, p, .08f + .03f * H(Seed, k + 5), A(Foam, .7f * cAl), 0, Seed + k);
                }
                for (int k = 0; k < 40; k++)
                {
                    Vector2 dd = Dir(Seed, k, 40, .6f);
                    Drip(b, at + Vector2.up * 3.3f * up, new Vector2(dd.x * r * (1f + 1.2f * H(Seed, k)), 1.5f + 2f * H(Seed, k + 5)), 11, t - .25f, 1f, .09f + .05f * H(Seed, k + 3), k % 3 == 0 ? Foam : Aqua, Seed + k);
                }
                Grit(b, at, r * (.8f + .6f * EaseOut(Rise(t, .1f, 1f))), new Color(.7f, .9f, 1f), .35f * Fade(t, .5f, 1.7f), t, Seed + 5);
                for (int k = 0; k < 16; k++)
                {
                    Vector2 dd = Dir(Seed + 11, k, 16, .7f);
                    Fleck(b, at + Vector2.up * 3.3f * up, new Vector2(dd.x * 3, 1 + 2.5f * H(Seed, k + 80)), 10, t - .2f - .02f * k, .8f, .06f, Foam, Seed + k + 80);
                }
            }
        }

        /// <summary>Step 8. A sea serpent surfaces at one end and swims across the arena along the line: a weaving body of scaled segments with fins, a horned head with glowing eyes, foam and spray in its wake.</summary>
        private sealed class Leviathan : FxEffect
        {
            private const int N = 15; private static readonly Vector2[] sp = new Vector2[N], st = new Vector2[N]; private static readonly float[] ssz = new float[N], sem = new float[N]; private static readonly bool[] on = new bool[N];
            protected override float Life => 2.2f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 d = At - From; float len = d.magnitude; if (len < .1f) return;
                Vector2 dir = d / len, nrm = new Vector2(-dir.y, dir.x); float w = Mathf.Max(1.1f, Radius) * 1.15f;
                const float gap = .85f; float bodyLen = N * gap;
                float age = now - Born, span = (Land - Born) + 1.9f, head = Mathf.Lerp(-1.5f, len + bodyLen + 1, Mathf.Clamp01(age / span));
                b.Beam(From, At, w * 1.1f, A(Sea, .3f * Rise(age, 0, .3f) * Fade(t, 1.4f, 2.2f)), now, Seed, .2f);
                if (head < 1.2f) for (int k = 0; k < 14; k++) Drip(b, From, new Vector2((H(Seed, k) - .5f) * 3, 3 + 2 * H(Seed, k + 1)), 10, (age * 1.6f + k * .08f) % .7f, .7f, .11f, Foam, Seed + k);
                // the serpent, painted opaque: every segment first a size bigger in deep navy (the outline), then the teal skin,
                // a pale belly stripe and a lit back; additive scales, fins and spray on top
                for (int j = 0; j < N; j++)
                {
                    float s = head - j * gap; on[j] = s >= -.8f && s <= len + .8f; if (!on[j]) continue;
                    float emerge = Mathf.Clamp01((s + .8f) / 1.4f) * Mathf.Clamp01((len + .8f - s) / 1.4f), taper = 1 - j / (float)N * .7f;
                    float ph = s * .9f - now * 5f;
                    sp[j] = From + dir * s + nrm * Mathf.Sin(ph) * w * .6f; st[j] = (dir + nrm * Mathf.Cos(ph) * w * .55f).normalized;
                    ssz[j] = w * taper * (j == 0 ? 1.2f : 1f) * (.55f + .45f * emerge); sem[j] = emerge;
                }
                Color outl = new Color(.01f, .05f, .12f), skin = new Color(.06f, .36f, .58f), back = new Color(.16f, .55f, .8f), belly = new Color(.55f, .88f, .9f);
                for (int j = N - 1; j >= 0; j--) if (on[j])
                {
                    float sz = ssz[j], ta = Ang(st[j]); Vector2 n2 = new Vector2(-st[j].y, st[j].x);
                    b.Quad(FxShape.Shade, sp[j] - Vector2.up * sz * .35f, new Vector2(sz * 1.1f, sz * .5f), ta, A(Navy, .5f * sem[j]), 0, Seed);
                    if (j % 2 == 1 && j < N - 2) for (int side = -1; side <= 1; side += 2)
                    {
                        Vector2 fp = sp[j] + n2 * side * sz * .62f - st[j] * sz * .2f; float fa = ta + 180 + side * 50;
                        Solid(b, fp, new Vector2(sz * .3f + .06f, sz * .12f + .06f), fa, outl, sem[j]);
                        Solid(b, fp, new Vector2(sz * .3f, sz * .12f), fa, back, sem[j]);
                    }
                    Solid(b, sp[j], new Vector2(Mathf.Max(sz * .72f, gap * .85f) + .07f, sz * .5f + .07f), ta, outl, sem[j]);
                }
                for (int j = N - 1; j >= 0; j--) if (on[j])
                {
                    float sz = ssz[j], ta = Ang(st[j]); Vector2 n2 = new Vector2(-st[j].y, st[j].x);
                    Solid(b, sp[j], new Vector2(Mathf.Max(sz * .72f, gap * .85f), sz * .5f), ta, skin, sem[j]);
                    Solid(b, sp[j] - n2 * sz * .2f, new Vector2(Mathf.Max(sz * .56f, gap * .7f), sz * .16f), ta, belly, sem[j]);
                    Solid(b, sp[j] + n2 * sz * .22f, new Vector2(Mathf.Max(sz * .5f, gap * .65f), sz * .12f), ta, back, sem[j]);
                }
                if (on[0])
                {
                    float sz = ssz[0], ta = Ang(st[0]); Vector2 p = sp[0], tan = st[0], n2 = new Vector2(-tan.y, tan.x);
                    float jaw = .5f + .5f * Mathf.Sin(now * 9);
                    Solid(b, p + tan * sz * .75f + n2 * sz * .08f, new Vector2(sz * .55f + .07f, sz * .3f + .07f), ta + 8, outl, sem[0]);
                    Solid(b, p + tan * sz * .65f - n2 * sz * (.2f + .08f * jaw), new Vector2(sz * .45f + .07f, sz * .15f + .07f), ta - 10 - 12 * jaw, outl, sem[0]);
                    Solid(b, p + tan * sz * .65f - n2 * sz * (.2f + .08f * jaw), new Vector2(sz * .45f, sz * .15f), ta - 10 - 12 * jaw, belly, sem[0]);
                    Solid(b, p + tan * sz * .75f + n2 * sz * .08f, new Vector2(sz * .55f, sz * .3f), ta + 8, skin, sem[0]);
                    Solid(b, p + tan * sz * .8f + n2 * sz * .2f, new Vector2(sz * .4f, sz * .1f), ta + 8, back, sem[0]);
                    for (int side = -1; side <= 1; side += 2)
                        b.Quad(Shard, p - tan * sz * .1f + n2 * (sz * .45f + side * sz * .12f), new Vector2(sz * .55f, sz * .14f), ta + 160 + side * 18, A(Foam, .9f * sem[0]), 0, Seed + side, 0);
                    b.Quad(FxShape.Glow, p + tan * sz * .55f + n2 * sz * .2f, sz * .28f, A(new Color(1f, .85f, .2f), sem[0]), 0, Seed);
                    Glint(b, p + tan * sz * .55f + n2 * sz * .2f, sz * .22f, new Color(1f, .95f, .5f), sem[0], now * 90);
                    for (int k = 0; k < 4; k++) Fleck(b, p + tan * sz * 1.1f, tan * 2.5f + n2 * (k - 1.5f) * 1.5f + Vector2.up * 2, 9, (now * 2.2f + k * .12f) % .45f, .45f, .06f, Foam, Seed + k + 90);
                    Puff(b, p + tan * sz * 1.2f, sz * .7f, new Color(.8f, .95f, 1f), .35f * sem[0], now, Seed + 31);
                }
                for (int j = 1; j < N; j++) if (on[j])
                {
                    float sz = ssz[j], ta = Ang(st[j]); Vector2 p = sp[j], tan = st[j], n2 = new Vector2(-tan.y, tan.x);
                    b.Quad(Shard, p + n2 * sz * .3f, new Vector2(sz * .45f, sz * .1f), ta + 180, A(Aqua, .55f * sem[j]), 0, Seed + j, 0);
                    Glint(b, p + n2 * sz * .25f + tan * sz * .2f * Mathf.Sin(now * 3 + j), .14f * sz, Foam, .7f * sem[j] * (.5f + .5f * Mathf.Sin(now * 6 + j * 1.7f)), j * 40);
                    if (j % 3 == 1) Puff(b, p - Vector2.up * sz * .3f, sz * .7f, new Color(.78f, .93f, 1f), .3f * sem[j], now + j, Seed + j + 30);
                    if (j % 3 == 0)
                        for (int side = -1; side <= 1; side += 2)
                            Drip(b, p, n2 * side * 2.2f + Vector2.up * 2.4f, 10, (now * 1.7f + j * .2f) % .5f, .5f, .1f, Foam, Seed + j + side);
                }
                for (int k = 0; k < 36; k++)
                {
                    float s = len * H(Seed, k + 40), a = (head - s) / 3f; if (a <= 0 || a >= 1) continue;
                    Vector2 p = From + dir * s + nrm * (H(Seed, k + 60) - .5f) * w * 3.2f;
                    b.Quad(Bubble, p, .07f + .07f * H(Seed, k), A(Foam, .9f * (1 - a)), 0, Seed + k);
                }
            }
        }

        /// <summary>Step 8 weather. Warm sea rain: the arena tints aqua, straight silver-blue rain falls, drops bounce up where they land, slow bands of light roll across like a tide.</summary>
        private sealed class Rain : FxEffect
        {
            protected override float Life => 2f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                float on = Env(this, now, 2.2f, 1.2f); if (on <= 0) return;
                Rect a = Arena; float H0 = a.height + 4;
                for (int k = 0; k < 6; k++)
                {
                    float u = (H(Seed, k + 500) + now * .05f) % 1; Vector2 p = new Vector2(a.xMin - 2 + u * (a.width + 4), a.yMin + a.height * H(Seed, k + 501));
                    Grit(b, p, 1.4f + .6f * H(Seed, k + 502), new Color(.6f, .85f, .95f), .2f * on * Mathf.Sin(u * 3.14f), now + k, Seed + k);
                }
                for (int k = 0; k < 90; k++)
                {
                    float sp = 15 + 5 * H(Seed, k + 300), u = (H(Seed, k) + now * sp / H0) % 1;
                    Vector2 p = new Vector2(a.xMin + a.width * H(Seed, k + 150) + u * .3f, a.yMax + 2 - u * H0);
                    b.Beam(p, p + new Vector2(.06f, 1f), .028f, A(new Color(.6f, .9f, 1f), .5f * on), 0, k, 0);

                }
                for (int k = 0; k < 34; k++)
                {
                    float cyc = .55f + .4f * H(Seed, k + 9), ph = (now + H(Seed, k) * cyc) / cyc; int n = Mathf.FloorToInt(ph); float u = ph - n;
                    Vector2 p = new Vector2(a.xMin + a.width * H(Seed + n, k + 3), a.yMin + a.height * H(Seed + n, k + 4));
                    b.Floor(FxShape.Glow, p, .5f, A(Aqua, .35f * on * (1 - u)), 0, Seed);
                    for (int j = -1; j <= 1; j += 2) Drip(b, p, new Vector2(j * 1f, 2.2f), 12, u * .38f, .38f, .08f, Foam, Seed + k + j);
                }
            }
        }
    }
}
