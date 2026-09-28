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
    }

    /// <summary>Windborne bond: crescent blades, spiral funnels, flying leaves and feathers, long streaks.</summary>
    public static class WindFx
    {
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
                }
                for (int k = 0; k < 10; k++)
                {
                    float tf = .04f + k * .06f; if (t < tf) continue; float a = (t - tf) / .95f; if (a >= 1) continue;
                    Vector2 p0 = From + dir * len * (.12f + .8f * H(Seed, k + 60)) + nrm * (H(Seed, k + 80) - .5f) * 1.3f;
                    Vector2 p = p0 + nrm * (k % 2 == 0 ? 1 : -1) * a * 1f + Vector2.down * a * a * 1.2f;
                    Lf(b, p, .45f, k * 40 + a * 360, k % 3 == 0 ? Green : Cream, 1 - a, Seed + k, k % 3 != 0);
                }
            }
        }

        /// <summary>Step 6 and 8. A tall funnel of stacked spiral streaks, swaying, with leaves, feathers and four crescent blades circling it and dust at the foot.</summary>
        private sealed class Tornado : FxEffect
        {
            protected override float Life => Moment.Kind == BondMomentKind.Zone ? Mathf.Max(1.5f, Moment.Duration) + .3f : 2f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                float on = Env(this, now, .5f, .6f); if (on <= 0) return;
                float R = Mathf.Max(1.6f, Moment.Radius) * 1.15f, tall = 3.9f; Vector2 at = At;
                b.Floor(FxShape.Shade, at, R * 1.5f, A(new Color(.02f, .08f, .08f), .55f * on), 0, Seed);
                b.Floor(Swirl, at, R * 1.7f, A(Mint, .55f * on), now * 2.4f, Seed + 3, 4);
                b.Floor(FxShape.Smoke, at + new Vector2(Mathf.Sin(now * 1.7f) * .4f, 0), R * 1.1f, A(new Color(.6f, .55f, .4f), .38f * on), now * .3f, Seed + 4);
                for (int i = 0; i < 12; i++)
                {
                    float f = i / 11f, sway = Mathf.Sin(now * 2.3f + f * 3.2f + Seed) * .32f * f, w = R * (.34f + .95f * f * f + .2f * f);
                    Vector2 c = at + new Vector2(sway, -.1f + f * tall);
                    b.Quad(Swirl, c, new Vector2(w * 1.15f, w * .42f), 0, A(i % 2 == 0 ? Mint : Teal, (.85f - .3f * f) * on), now * (2.6f + f * 1.5f) * (i % 2 == 0 ? 1 : -1), Seed + i, i % 2 == 0 ? 2 : -3);
                    b.Quad(FxShape.Glow, c, new Vector2(w * 1.1f, w * .5f), 0, A(Teal, .1f * on), 0, Seed);
                }
                for (int k = 0; k < 16; k++)
                {
                    float spd = 3.5f + 2.5f * H(Seed, k), f = (H(Seed, k + 9) + now * (.28f + .12f * H(Seed, k + 3))) % 1, a = now * spd + k * 1.9f;
                    float rw = R * (.42f + .95f * f * f + .2f * f) * 1.1f, sway = Mathf.Sin(now * 2.3f + f * 3.2f + Seed) * .32f * f;
                    Vector2 p = at + new Vector2(sway + Mathf.Cos(a) * rw * 1.15f, -.1f + f * tall + Mathf.Sin(a) * rw * .42f), tan = new Vector2(-Mathf.Sin(a) * 1.15f, Mathf.Cos(a) * .42f);
                    float al = on * Mathf.Sin(f * 3.14f) * 1.3f;
                    if (k % 4 == 3) Lf(b, p, .5f, Ang(tan) + Mathf.Sin(now * 9 + k) * 30, Cream, al, Seed + k, true);
                    else Lf(b, p, .42f, Ang(tan) + now * 200 * (k % 2 == 0 ? 1 : -1), k % 3 == 0 ? Green : new Color(.7f, 1f, .4f), al, Seed + k);
                }
                for (int k = 0; k < 4; k++)
                {
                    float f = .25f + k * .22f, a = now * (5.5f - k * .6f) + k * 1.57f, rw = R * (.5f + .95f * f * f + .25f * f) * 1.3f, sway = Mathf.Sin(now * 2.3f + f * 3.2f + Seed) * .32f * f;
                    Vector2 p = at + new Vector2(sway + Mathf.Cos(a) * rw * 1.15f, -.1f + f * tall + Mathf.Sin(a) * rw * .42f), tan = new Vector2(-Mathf.Sin(a) * 1.15f, Mathf.Cos(a) * .42f);
                    b.Quad(Blade, p, new Vector2(.85f, .65f), Ang(tan) + 180, A(White, .95f * on));
                    for (int j = 1; j < 4; j++) { float a2 = a - j * .22f; Vector2 q = at + new Vector2(sway + Mathf.Cos(a2) * rw * 1.15f, -.1f + f * tall + Mathf.Sin(a2) * rw * .42f); Glint(b, q, .22f, Mint, .6f * on * (1 - j / 4f), j * 60); }
                }
            }
        }

        /// <summary>Step 8 weather. A steady gale: long curling streaks, blown leaves and feathers, and now and then a huge crescent gust sweeping across.</summary>
        private sealed class Gale : FxEffect
        {
            protected override float Life => 2f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                float on = Env(this, now, 2f, 1.2f); if (on <= 0) return;
                Rect a = Arena; float W = a.width + 10;
                b.Quad(FxShape.Shade, a.center, new Vector2(a.width * .75f, a.height * .75f), 0, A(new Color(.2f, .5f, .45f), .17f * on), 0, Seed);
                for (int k = 0; k < 26; k++)
                {
                    float spd = 7 + 6 * H(Seed, k + 5), u = (H(Seed, k) + now * spd / W) % 1, y0 = a.yMin + a.height * H(Seed, k + 50), x = a.xMin - 5 + u * W, L = 3 + 3 * H(Seed, k + 90);
                    Vector2 prev = new Vector2(x, y0 + Mathf.Sin(x * .5f + k) * .4f);
                    for (int s = 1; s <= 3; s++)
                    {
                        float xx = x - L * s / 3f; Vector2 p = new Vector2(xx, y0 + Mathf.Sin(xx * .5f + k) * .4f);
                        b.Link(prev, p, .075f, A(k % 4 == 0 ? Cream : Mint, (.75f - .15f * s) * on), Seed + k, 1); prev = p;
                    }
                    if (k % 3 == 0) b.Quad(Swirl, new Vector2(x, y0 + Mathf.Sin(x * .5f + k) * .4f), .38f, A(White, .55f * on), now * 9 + k, Seed + k, 2);
                }
                for (int k = 0; k < 40; k++)
                {
                    bool feather = k % 4 == 0; float spd = 6 + 7 * H(Seed, k + 7), u = (H(Seed, k + 20) + now * spd / W) % 1, y0 = a.yMin + a.height * H(Seed, k + 60), x = a.xMin - 5 + u * W;
                    Vector2 p = new Vector2(x, y0 + Mathf.Sin(x * .45f + k * 2) * .9f);
                    Lf(b, p, feather ? .75f : .52f, now * (150 + 90 * H(Seed, k)) * (k % 2 == 0 ? 1 : -1) + k * 50, feather ? Cream : (k % 3 == 0 ? Green : new Color(.75f, 1f, .45f)), .9f * on, Seed + k, feather);
                }
                for (int g = 0; g < 3; g++)
                {
                    float cyc = 3.4f, tt = (now + g * 1.15f) % cyc / 1.3f; if (tt >= 1) continue;
                    int n = Mathf.FloorToInt((now + g * 1.15f) / cyc); float y0 = a.yMin + a.height * (.2f + .6f * H(Seed + n, g)), x = a.xMin - 3 + tt * (a.width + 6);
                    float al = Mathf.Sin(tt * 3.14f) * .55f * on;
                    b.Quad(Blade, new Vector2(x, y0), new Vector2(3.4f, 2.4f), 180 + Mathf.Sin(tt * 6) * 8, A(Mint, al), 0, Seed + g);
                    b.Quad(Blade, new Vector2(x - .6f, y0), new Vector2(4.4f, 3f), 180, A(Teal, al * .4f), 0, Seed + g + 4);
                    for (int s = 0; s < 4; s++) b.Link(new Vector2(x - 1.2f, y0 + (s - 1.5f) * .7f), new Vector2(x - 5.5f - s, y0 + (s - 1.5f) * 1.1f), .05f, A(White, al), Seed + s, 1);
                }
            }
        }
    }
}
