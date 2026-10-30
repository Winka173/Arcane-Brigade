using System;
using System.Collections.Generic;
using UnityEngine;
using static ArcaneBrigade.FxKit;
using static ArcaneBrigade.AKit;

namespace ArcaneBrigade
{
    /// <summary>Small drawing helpers of the family-A effect files (Wind, Ember, Frost, Tide, Garden) and the ids of their shader shapes.</summary>
    internal static class AKit
    {
        public const FxShape Blade = (FxShape)20, Leaf = (FxShape)21, Shard = (FxShape)22, Drop = (FxShape)23, Bubble = (FxShape)24, Swirl = (FxShape)25,
            Flake = (FxShape)26, Curl = (FxShape)27, Petal = (FxShape)28, Root = (FxShape)29;
        public const FxShape Tube = (FxShape)29;
        /// <summary>FxBatch draws pieces under 1.1 half-size at .72 of the asked size: the size to ask for so a chain piece lands at <paramref name="s"/>.</summary>
        public static Vector2 Fit(Vector2 s) { float m = Mathf.Max(s.x, s.y); return m < .792f ? s / .72f : m < 1.1f ? s * (1.1f / m) : s; }
        /// <summary>The points of the last <see cref="Chain"/>.</summary>
        public static readonly Vector2[] CP = new Vector2[48];
        /// <summary>A soft curving chain (root, vine, tentacle, serpent) grown from <paramref name="p0"/>: it heads at <paramref name="deg0"/>, bends by
        /// <paramref name="curl"/> degrees towards its tip and whips with a wave travelling up it (<paramref name="amp"/> degrees, <paramref name="phase"/>).
        /// Only the grown part (0..1) is laid out. Returns the number of points written to <see cref="CP"/>.</summary>
        public static int Chain(Vector2 p0, float deg0, float len, int n, float grow, float curl, float amp, float phase, float waves = 1.2f)
        {
            n = Mathf.Clamp(n, 1, CP.Length - 1); CP[0] = p0; Vector2 p = p0; float seg = len / n, g = Mathf.Clamp01(grow) * n; int m = 0;
            for (int i = 1; i <= n; i++)
            {
                if (i - 1 >= g) break;
                float f = (i - .5f) / n, deg = deg0 + curl * f * f + amp * f * Mathf.Sin(phase - f * waves * 6.2832f);
                p += Vec(deg) * Mathf.Min(1, g - (i - 1)) * seg; CP[i] = p; m = i;
            }
            return m + 1;
        }
        /// <summary>Paints the chain in <see cref="CP"/> as one tapering tube: a dark ink outline under it all, the body (material 0 wood, 1 vine,
        /// 2 scales) and a lit strip along its top-left side. Solid, on the alpha layer; the light layer gets only a sheen.</summary>
        public static void TubeDraw(FxBatch b, int count, float w0, float w1, Color ink, Color body, Color hi, float a, int mat, int seed, float pad = .045f)
        {
            if (count < 2 || a <= .003f) return;
            for (int pass = 0; pass < 3; pass++)
                for (int i = 0; i < count - 1; i++)
                {
                    Vector2 p = CP[i], q = CP[i + 1], d = q - p; float l = d.magnitude; if (l < .002f) continue;
                    float f = (i + .5f) / (count - 1), w = Mathf.Lerp(w0, w1, f), deg = Ang(d); Vector2 n = new Vector2(-d.y, d.x) / l;
                    if (n.y - n.x < 0) { deg += 180; n = -n; }
                    Vector2 mid = (p + q) * .5f; float L = l * .5f + w;
                    if (pass == 0) b.Quad(Tube, mid, Fit(new Vector2(L + pad, w + pad)), deg, A(ink, a), (l * .5f) / (L + pad), seed + i, 9);
                    else if (pass == 1) b.Quad(Tube, mid, Fit(new Vector2(L, w)), deg, A(body, a), (l * .5f) / L, seed + i, mat);
                    else if (w > .05f) b.Quad(Tube, mid + n * w * .42f, Fit(new Vector2(l * .5f + w * .3f, w * .22f)), deg, A(hi, .5f * a), (l * .5f) / (l * .5f + w * .3f), seed + i, 9);
                }
        }
        public static float Ang(Vector2 d) => Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
        public static Vector2 Vec(float deg) { float r = deg * Mathf.Deg2Rad; return new Vector2(Mathf.Cos(r), Mathf.Sin(r)); }
        public static Vector2 Pt(int seed, int i, float rx, float ry) { float a = H(seed, i) * 6.2832f, d = Mathf.Sqrt(H(seed + 7, i)); return new Vector2(Mathf.Cos(a) * d * rx, Mathf.Sin(a) * d * ry); }
        public static Vector2 Arc(Vector2 a, Vector2 z, float u, float h) => Vector2.Lerp(a, z, u) + Vector2.up * (h * 4 * u * (1 - u));
        public static void Lf(FxBatch b, Vector2 p, float s, float deg, Color c, float a, int seed, bool feather = false)
            => b.Quad(Leaf, p, new Vector2(s, s * (feather ? .45f : .72f)), deg, A(c, a), 0, seed, feather ? 1 : 0);
        /// <summary>A flame tongue standing on <paramref name="baseP"/>, leaning by degrees (0 = straight up).</summary>
        public static void Tongue(FxBatch b, Vector2 baseP, float lean, float h, float w, Color c, float a, float prog, int seed, float par = 0)
            => b.Quad(Bubble, baseP + Vec(90 + lean) * h, new Vector2(w * 1.5f, h), lean, A(c, a), prog, seed, 10 + par);
        public static float Env(FxEffect e, float now, float fin, float fout) => Rise(now - e.Born, 0, fin) * Fade(now, e.End - fout, e.End);
        public static float Overshoot(float t) { t = Mathf.Clamp01(t); return EaseOut3(t) + .18f * Mathf.Sin(t * Mathf.PI) * (1 - t); }
        /// <summary>An opaque painted piece on the alpha layer (a soft wash pushed past full so its edge stays crisp): wood, rock, bodies.</summary>
        public static void Solid(FxBatch b, Vector2 p, Vector2 size, float deg, Color c, float a) { if (a <= .003f) return; c.a = 12f * Mathf.Clamp01(a); b.Quad(FxShape.Shade, p, size, deg, c, 0, 0, 0); }
        public static void Puff(FxBatch b, Vector2 p, float s, Color c, float a, float prog, int seed) => b.Quad(FxShape.Smoke, p, s, A(c, a), prog, seed);
        public static void Grit(FxBatch b, Vector2 p, float s, Color c, float a, float prog, int seed) => b.Floor(FxShape.Dust, p, s, A(c, a), prog, seed);
        /// <summary>A small hot fleck on a gravity arc with a short streak behind it.</summary>
        public static void Fleck(FxBatch b, Vector2 from, Vector2 vel, float g, float t, float life, float size, Color c, int seed)
        {
            float u = t / life; if (u <= 0 || u >= 1) return;
            Vector2 p = from + vel * t + Vector2.down * g * t * t * .5f, v = vel + Vector2.down * g * t; float al = 1 - u * u;
            b.Link(p, p - v * .05f, size * .45f, A(c, .75f * al), seed, 1);
            b.Quad(FxShape.Glow, p, size * 1.7f, A(c, .8f * al), 0, seed);
        }
    }

    /// <summary>Windborne bond: crescent blades, spiral funnels, flying leaves and feathers, long streaks.</summary>
    public static class WindFx
    {
        // owner 29/10/2026: small glints and sparks were drawn far too big: small ones shrink, big set-piece flashes keep their size
        private static void Glint(FxBatch b, Vector2 at, float size, Color tint, float alpha, float turn = 0)
            => FxKit.Glint(b, at, size <= .08f ? size : size < .5f ? .08f + (size - .08f) * .45f : size - .231f, tint, alpha, turn);
        public static void Register(Dictionary<string, Func<FxEffect>> map)
        {
            map["piercing-volley"] = () => new Volley();
            map["wind-tornado"] = () => new Tornado();
            map["wind-gale"] = () => new Gale();
        }
        private static readonly Color Mint = new Color(.7f, 1f, .85f), Teal = new Color(.3f, .85f, .8f), Cream = new Color(1f, .95f, .72f), Green = new Color(.5f, .9f, .3f);

        /// <summary>Steps 2/4. Seven crescent blades of wind race down the firing line, side by side and staggered, each trailing streaks; feathers shed off the lane.</summary>
        private sealed class Volley : FxEffect
        {
            protected override float Life => 1.2f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 d = At - From; float len = d.magnitude; if (len < .1f) return;
                Vector2 dir = d / len, nrm = new Vector2(-dir.y, dir.x); float ang = Ang(dir);
                float lane = Fade(t, .1f, 1f) * Rise(t, 0, .06f);
                b.Beam(From, At, .8f, A(Mint, .3f * lane), t, Seed, .3f);
                for (int k = 0; k < 16; k++)
                {
                    float u = (H(Seed, k + 20) + t * 3.4f) % 1, lat = (H(Seed, k) - .5f) * 1.9f, L = 1.3f + 1.7f * H(Seed, k + 40);
                    Vector2 p = From + dir * len * u + nrm * lat;
                    b.Link(p, p - dir * L, .04f, A(k % 3 == 0 ? Cream : Mint, .6f * lane * Mathf.Sin(u * 3.1416f)), Seed + k, 1);
                }
                for (int i = 0; i < 7; i++)
                {
                    float lag = i * .05f, u = Rise(t, lag, lag + .5f); if (u <= 0) continue;
                    float fade = Fade(t, lag + .5f, lag + .8f); if (fade <= 0) continue;
                    int s = (i + 1) / 2; float lat = (i % 2 == 1 ? -1 : 1) * s * .34f, sc = 1f + .25f * H(Seed, i), e = 1 - (1 - u) * (1 - u);
                    Vector2 p = From + dir * len * (.04f + e * .98f) + nrm * (lat + Mathf.Sin(u * 9 + i) * .12f);
                    b.Quad(Blade, p - dir * .15f, new Vector2(1.9f * sc, 1.4f * sc), ang + 180, A(Teal, .45f * fade), 0, Seed + i + 9);
                    b.Quad(Blade, p, new Vector2(1.35f * sc, 1.05f * sc), ang + 180, A(Mint, fade), 0, Seed + i);
                    for (int k = 0; k < 3; k++) b.Link(p - dir * .35f + nrm * (k - 1) * .3f * sc, p - dir * (1.9f + .8f * k) * sc + nrm * (k - 1) * .5f * sc, .055f, A(White, .55f * fade), Seed + i * 3 + k, 1);
                    Glint(b, p + dir * .5f, .3f, White, .9f * fade, t * 300 + i * 20);
                    for (int j = 0; j < 2; j++)
                    {
                        float back = .1f + j * .14f; if (u < back) continue; float e2 = 1 - (1 - (u - back)) * (1 - (u - back));
                        Grit(b, From + dir * len * (.04f + e2 * .98f) + nrm * lat - Vector2.up * .35f, .55f + .25f * j, new Color(.62f, .6f, .48f), .32f * fade * (1 - j * .3f), t + j, Seed + i * 7 + j);
                    }
                    for (int j = 0; j < 3; j++) Fleck(b, p + nrm * (j - 1) * .4f, nrm * (j - 1) * 1.6f - dir * 1.2f + Vector2.up * .8f, 3, t - lag - .12f - j * .06f, .45f, .07f, j == 1 ? White : Mint, Seed + i * 5 + j);
                }
                for (int k = 0; k < 4; k++)
                {
                    float a = Rise(t, .42f + k * .07f, .95f + k * .07f); if (a <= 0 || a >= 1) continue;
                    Vector2 c = At + nrm * (k - 1.5f) * .55f + dir * a * 1.1f;
                    b.Quad(Swirl, c, new Vector2(.6f + a * .5f, .4f + a * .3f), ang, A(k % 2 == 0 ? Mint : White, .65f * (1 - a)), a * 2, Seed + k, k % 2 == 0 ? 1 : -1);
                }
                for (int k = 0; k < 18; k++)
                {
                    float tf = .04f + k * .035f; if (t < tf) continue; float a = (t - tf) / .95f; if (a >= 1) continue;
                    Vector2 p0 = From + dir * len * (.12f + .8f * H(Seed, k + 60)) + nrm * (H(Seed, k + 80) - .5f) * 1.3f;
                    Vector2 p = p0 + nrm * (k % 2 == 0 ? 1 : -1) * a * 1f + Vector2.down * a * a * 1.2f;
                    Lf(b, p, .2f, k * 40 + a * 360, k % 3 == 0 ? Green : Cream, 1 - a, Seed + k, k % 3 != 0);
                }
            }
        }

        /// <summary>Step 6 and 8. A thick, layered tornado: a dusty funnel body (dark core, a bright wind rim, streaks wrapping round it)
        /// leaning out into a wide churning top, a ploughed foot of brown dust rolling round its base, and soil, twigs, leaves and pebbles
        /// spiralling up around it (bright in front, dim behind).</summary>
        private sealed class Tornado : FxEffect
        {
            protected override float Life => Moment.Kind == BondMomentKind.Zone ? Mathf.Max(1.5f, Moment.Duration) + .3f : 2f;
            private static readonly Color Body = new Color(.3f, .33f, .32f), Core = new Color(.1f, .11f, .12f), Sheath = new Color(.5f, .55f, .5f),
                Rim = new Color(.75f, 1f, .92f), Dirt = new Color(.52f, .42f, .3f), Soil = new Color(.24f, .16f, .09f);
            /// <summary>Where a mote at height f (0 foot .. 1 top) and angle a sits on the funnel; front is true while it passes in front.</summary>
            private Vector2 OnFunnel(float f, float a, float R, float tall, float now, float out1, out bool front)
            {
                float y = -.1f + f * tall, lean = (.22f * f * f - .08f + .07f * Mathf.Sin((f * 2 - 1) * 4.5f - now * 2.6f + Seed % 997 * .731f)) * R * 1.15f;
                float w = R * 1.15f * Mathf.Lerp(.11f, .86f, Mathf.Pow(f, 1.35f)) * out1;
                front = Mathf.Sin(a) < 0;
                return At + new Vector2(lean + Mathf.Cos(a) * w, y + Mathf.Sin(a) * w * .3f);
            }
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                float on = Env(this, now, .5f, .6f); if (on <= 0) return;
                float R = Radius * .95f, tall = 3.6f; Vector2 at = At, mid = at + Vector2.up * (tall * .5f - .1f);
                // the ploughed ground: a dark scar, a ring of brown dust rolling round the foot, puffs of soil thrown out
                b.Floor(FxShape.Shade, at, R * 1.15f, A(new Color(.05f, .04f, .03f), .55f * on), 0, Seed);
                b.Floor(Swirl, at, R * 1.25f, A(Dirt, .55f * on), now * 2.2f, Seed + 3, -4);
                for (int k = 0; k < 7; k++)
                {
                    float a = now * (2.2f + .3f * k) + k * .9f, rr = R * (.45f + .12f * (k % 3));
                    Grit(b, at + new Vector2(Mathf.Cos(a) * rr, Mathf.Sin(a) * rr * .45f + .05f), R * (.42f + .1f * (k % 2)), Dirt, .6f * on, now * .9f + k, Seed + 4 + k);
                }
                for (int k = 0; k < 6; k++)
                {
                    float u = (now * .9f + k / 6f) % 1, a = now * 3.4f + k * 1.05f;
                    Puff(b, at + new Vector2(Mathf.Cos(a) * R * (.35f + .45f * u), .15f + u * .9f), .32f + .35f * u, Dirt, .55f * on * Mathf.Sin(u * 3.14f), u, Seed + 20 + k);
                }
                // the funnel: a pale outer sheath for thickness, the dusty body, its dark core, then the light (rim + wrapping streaks)
                b.Quad(Swirl, mid + Vector2.right * .05f, new Vector2(R * 1.3f, tall * .54f), 0, A(Sheath, .38f * on), now * .8f, Seed + 1, 10);
                b.Quad(Swirl, mid, new Vector2(R * 1.15f, tall * .5f), 0, A(Body, .95f * on), now, Seed, 10);
                b.Quad(Swirl, mid + Vector2.right * .04f, new Vector2(R * .62f, tall * .5f), 0, A(Core, .55f * on), now * 1.3f, Seed + 2, 10);
                b.Quad(Swirl, mid, new Vector2(R * 1.15f, tall * .5f), 0, A(Rim, .95f * on), now, Seed, 11);
                b.Quad(Swirl, mid, new Vector2(R * 1.2f, tall * .52f), 0, A(Teal, .5f * on), now * 1.4f + 3, Seed + 5, 11);
                // the wide churning top: grey billows turning round the funnel's mouth
                for (int k = 0; k < 7; k++)
                {
                    float a = now * 1.6f + k * .9f; Vector2 p = OnFunnel(.97f, a, R, tall, now, 1.05f, out bool fr);
                    Puff(b, p + Vector2.up * .1f, R * (.38f + .08f * (k % 3)), fr ? Sheath : Body, (fr ? .75f : .5f) * on, now * .3f + k, Seed + 40 + k);
                }
                // debris spiralling up: soil clods, twigs, pebbles and leaves, faster and wider as they climb
                for (int k = 0; k < 34; k++)
                {
                    float f = (H(Seed, k + 9) + now * (.22f + .1f * H(Seed, k + 3))) % 1, a = now * (3.2f + 2f * H(Seed, k)) * (1 - .3f * f) + k * 1.9f;
                    Vector2 p = OnFunnel(f, a, R, tall, now, 1.12f + .1f * H(Seed, k + 5), out bool fr);
                    float al = on * Mathf.Sin(f * 3.14f) * (fr ? 1 : .45f), spin = now * (300 + 200 * H(Seed, k)) + k * 40;
                    int kind = k % 5;
                    if (kind == 0) Lf(b, p, .17f, spin, k % 2 == 0 ? Green : new Color(.75f, .6f, .25f), al, Seed + k);
                    else if (kind == 1) Solid(b, p, new Vector2(.16f, .035f), spin, new Color(.35f, .22f, .1f), al);
                    else if (kind == 2) { Solid(b, p, new Vector2(.08f, .065f), spin, Soil, al); Solid(b, p + new Vector2(-.02f, .02f), new Vector2(.04f, .03f), spin, new Color(.45f, .35f, .25f), al); }
                    else Puff(b, p, .16f + .12f * f, Dirt, .5f * al, f, Seed + k);
                }
                // streaks of wind racing round the front of the funnel, and a pulse of light climbing it now and then
                for (int k = 0; k < 6; k++)
                {
                    float f = .12f + k * .15f, a = now * 6 + k * 2.2f; Vector2 p0 = OnFunnel(f, a, R, tall, now, 1.05f, out bool fr), p1 = OnFunnel(f + .03f, a + .7f, R, tall, now, 1.05f, out _);
                    if (fr) b.Link(p0, p1, .045f, A(Rim, .55f * on), Seed + k, 1);
                }
                float gp = (now - Born) % 1.6f / .9f;
                if (gp < 1) b.Quad(FxShape.Glow, OnFunnel(EaseOut(gp), 0, R, tall, now, 0, out _), new Vector2(R * .9f, .35f), 0, A(Rim, .3f * on * Mathf.Sin(gp * 3.14f)), 0, Seed);
            }
        }

        /// <summary>Step 8 weather. A steady gale across the field: long coloured wind ribbons (mint, teal, pale gold, sky) ripple past at
        /// different heights, each with a paler twin strand; flocks of leaves and a feather tumble along together; low dust skims the floor.</summary>
        private sealed class Gale : FxEffect
        {
            protected override float Life => 2f;
            private static readonly Color[] RibbonC = { new Color(.55f, 1f, .8f), new Color(.3f, .85f, .85f), new Color(1f, .9f, .55f), new Color(.65f, .8f, 1f) };
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                float on = Env(this, now, 2f, 1.2f); if (on <= 0) return;
                Rect a = Arena; float W = a.width + 14;
                for (int k = 0; k < 7; k++)
                {
                    float spd = 7 + 4 * H(Seed, k + 200), u = (H(Seed, k + 201) + now * spd / W) % 1; Vector2 p = new Vector2(a.xMin - 5 + u * W, a.yMin + a.height * H(Seed, k + 202));
                    Grit(b, p, 1.2f + .5f * H(Seed, k + 203), new Color(.62f, .6f, .48f), .3f * on * Mathf.Sin(u * 3.14f), now * 2 + k, Seed + k);
                }
                // ribbons: a band of light snaking along, thick in the middle and thin at both ends, with a paler twin
                for (int k = 0; k < 8; k++)
                {
                    float spd = 6 + 3 * H(Seed, k + 5), u = (H(Seed, k) + now * spd / W) % 1, x = a.xMin - 7 + u * W, L = 6 + 3 * H(Seed, k + 90);
                    float y0 = a.yMin + .8f + (a.height - 1.6f) * ((k + .5f + .4f * (H(Seed, k + 50) - .5f)) / 8f), amp = .35f + .3f * H(Seed, k + 7), fq = .55f + .25f * H(Seed, k + 8);
                    Color c = RibbonC[k % 4]; float al = on * Mathf.Min(1, Mathf.Sin(u * 3.14f) * 2.5f);
                    for (int strand = 0; strand < 2; strand++)
                    {
                        Vector2 prev = Vector2.zero;
                        for (int s = 0; s <= 16; s++)
                        {
                            float f = s / 16f, xx = x - L * f, yy = y0 + Mathf.Sin(xx * fq + now * 2.4f + k) * amp + strand * (.22f + .1f * Mathf.Sin(xx * 1.3f + now * 3));
                            Vector2 p = new Vector2(xx, yy);
                            if (s > 0) b.Link(prev, p, (strand == 0 ? .22f : .08f) * Mathf.Sin(f * 3.14f) + .02f, A(strand == 0 ? c : White, (strand == 0 ? .7f : .45f) * al * (1 - f * .4f)), Seed + k * 20 + s, .3f);
                            prev = p;
                        }
                    }
                    if (k % 2 == 0) b.Quad(Swirl, new Vector2(x - L * .1f, y0 + Mathf.Sin((x - L * .1f) * fq + now * 2.4f + k) * amp), .42f, A(c, .55f * al), now * 7 + k, Seed + k, 2);
                }
                // flocks: tight bunches of leaves (and a feather) tumbling along together, rising and dipping as a group
                for (int g = 0; g < 6; g++)
                {
                    float spd = 5.5f + 2.5f * H(Seed, g + 300), u = (H(Seed, g + 301) + now * spd / W) % 1, x = a.xMin - 5 + u * W;
                    float y = a.yMin + 1 + (a.height - 2) * H(Seed, g + 302) + Mathf.Sin(now * 1.3f + g * 2) * .8f;
                    for (int k = 0; k < 11; k++)
                    {
                        Vector2 off = new Vector2((H(Seed + g, k) - .5f) * 2.2f + Mathf.Sin(now * 3 + k * 1.7f) * .25f, (H(Seed + g, k + 20) - .5f) * 1.1f + Mathf.Cos(now * 2.6f + k) * .2f);
                        bool feather = k == 0; Color c = feather ? Cream : (k % 3 == 0 ? Green : k % 3 == 1 ? new Color(.75f, 1f, .45f) : new Color(.95f, .7f, .3f));
                        Lf(b, new Vector2(x, y) + off, feather ? .3f : .2f + .05f * H(Seed + g, k + 40), now * (200 + 140 * H(Seed + g, k)) * (k % 2 == 0 ? 1 : -1) + k * 50, c, .95f * on * Mathf.Min(1, Mathf.Sin(u * 3.14f) * 3), Seed + g * 13 + k, feather);
                    }
                }
            }
        }
    }
}
