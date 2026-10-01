using System;
using System.Collections.Generic;
using UnityEngine;
using static ArcaneBrigade.FxKit;

namespace ArcaneBrigade
{
    /// <summary>Drawing helpers of worker D (Bulwark, Spirit, Melody, Gravity, minors). Not an effect family (name does not end in Fx).</summary>
    internal static class DKit
    {
        public const FxShape Hex = (FxShape)50, Note = (FxShape)51, Arcs = (FxShape)52, Scythe = (FxShape)53, Wisp = (FxShape)54, Hole = (FxShape)55,
            Shield = (FxShape)56, Star = (FxShape)57, Pulse = (FxShape)58, Glyph = (FxShape)59;
        public static readonly Color Steel = new Color(.4f, .66f, 1f, 1), Ice = new Color(.82f, .94f, 1f, 1);
        public static readonly Color Teal = new Color(.4f, 1f, .86f, 1), Ghost = new Color(.8f, 1f, .96f, 1);
        public static readonly Color Pink = new Color(1f, .55f, .8f, 1), Butter = new Color(1f, .93f, .45f, 1), Mint = new Color(.55f, 1f, .78f, 1), Sky = new Color(.5f, .78f, 1f, 1), Lilac = new Color(.8f, .6f, 1f, 1);
        public static readonly Color Purple = new Color(.66f, .34f, 1f, 1), Lavender = new Color(.92f, .82f, 1f, 1), Dark = new Color(.015f, .01f, .04f, 1);
        public static Color Pastel(int i) { switch (((i % 5) + 5) % 5) { case 0: return Pink; case 1: return Butter; case 2: return Mint; case 3: return Sky; default: return Lilac; } }
        public static Vector2 Arc(Vector2 a, Vector2 z, float t, float h) => Vector2.Lerp(a, z, t) + Vector2.up * (4 * h * t * (1 - t));
        public static float Deg(Vector2 d) => Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
        public static Vector2 Sq(float r) => new Vector2(r, r * .62f);
        public static float EaseIn(float t) { t = Mathf.Clamp01(t); return t * t; }
        public static float Bump(float t, float a, float b) { return Mathf.Clamp01(Mathf.Min((t - a) / Mathf.Max(.001f, (b - a) * .35f), (b - t) / Mathf.Max(.001f, (b - a) * .65f))); }
        /// <summary>A glowing wavy line (vines, roots, ribbons).</summary>
        public static void Chain(FxBatch b, Vector2 a, Vector2 z, float amp, float freq, float phase, float width, Color tint, float alpha, int seed, int segs = 14, float thin = .6f)
        {
            Vector2 d = z - a; float len = d.magnitude; if (len < .01f || alpha < .004f) return; Vector2 n = new Vector2(-d.y, d.x) / len, prev = a;
            for (int s = 1; s <= segs; s++)
            {
                float t = s / (float)segs; Vector2 p = s == segs ? z : a + d * t + n * Mathf.Sin(t * freq * 6.2832f + phase) * amp * Mathf.Sin(t * Mathf.PI);
                b.Link(prev, p, width * 2.2f, A(tint, alpha * .35f), seed + s, 1); b.Link(prev, p, width, A(tint, alpha), seed + s, thin); prev = p;
            }
        }
    }

    /// <summary>Bulwark bond: shield plates, hexagon light, blue-steel domes of metal.</summary>
    public static class BulwarkFx
    {
        public static void Register(Dictionary<string, Func<FxEffect>> map)
        {
            map["guard-shockwave"] = () => new GuardShockwave();
            map["guard-line"] = () => new GuardLine();
            map["bulwark-shield-bolt"] = () => new ShieldBolt();
            map["bulwark-fortress"] = () => new Fortress();
        }

        /// <summary>A giant hexagon plate slams flat onto the crowd, three hexagonal shock outlines run out, plates and metal sparks fly.</summary>
        private sealed class GuardShockwave : FxEffect
        {
            protected override float Life => 1.5f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                if (t < 0) return; Vector2 at = At; float r = Radius * 1.3f;
                b.Quad(FxShape.Glow, at, r * 2.2f, A(DKit.Ice, .8f * Fade(t, 0, .2f)), 0, Seed);
                float drop = 1.7f - .7f * EaseOut(Rise(t, 0, .12f));
                b.Quad(DKit.Hex, at, new Vector2(r * drop, r * drop * .62f), 12, A(DKit.Steel, 1.1f * Fade(t, .5f, 1.3f)), t, Seed, 0);
                b.Quad(DKit.Hex, at, new Vector2(r * drop * .6f, r * drop * .37f), 12, A(DKit.Ice, .9f * Fade(t, .3f, .9f)), t, Seed + 1, 0);
                for (int k = 0; k < 3; k++)
                {
                    float u = Rise(t, .06f + k * .09f, .7f + k * .1f); if (u <= 0 || u >= 1) continue;
                    b.Quad(DKit.Hex, at, DKit.Sq(r * (.7f + 1.5f * EaseOut(u))), 12 + k * 20, A(k == 1 ? DKit.Ice : DKit.Steel, 1.1f * Fade(u, .35f, 1)), u, Seed + k, 1);
                }
                b.Floor(FxShape.Shock, at, r * 1.8f, A(DKit.Steel, .8f), Rise(t, 0, .7f), Seed);
                for (int k = 0; k < 7; k++)
                {
                    float life = .6f + .3f * H(Seed, k), u = t / life; if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + Dir(Seed, k, 7, .6f) * r * 1.8f * EaseOut(u) + Vector2.up * (1.6f * Mathf.Sin(u * Mathf.PI));
                    b.Quad(DKit.Hex, p, .34f * (1 - u * .4f), t * 500 * (k % 2 == 0 ? 1 : -1), A(DKit.Ice, 1 - u * u), u, Seed + k, 0);
                }
                for (int k = 0; k < 16; k++)
                {
                    float life = .35f + .35f * H(Seed, k + 30), u = t / life; if (u <= 0 || u >= 1) continue;
                    Glint(b, at + Dir(Seed, k + 9, 16, .6f) * r * 1.6f * EaseOut(u) + Vector2.up * .6f * u, .26f, k % 3 == 0 ? White : DKit.Steel, 1 - u, k * 40 + t * 300);
                }
            }
        }

        /// <summary>Iron stun: hexagon bars drop in a clamp of six plates around the crowd and lock.</summary>
        private sealed class GuardLine : FxEffect
        {
            protected override float Life => 1.1f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                if (t < 0) return; Vector2 at = At; float r = Radius * 1.1f;
                for (int k = 0; k < 6; k++)
                {
                    float u = Rise(t, k * .035f, .2f + k * .035f), fade = Fade(t, .65f, 1.1f); if (u <= 0) continue;
                    float ang = k * 60 * Mathf.Deg2Rad; Vector2 p = at + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang) * .62f) * r * (1.25f - .25f * EaseOut(u)) + Vector2.up * (1 - u) * 3f;
                    b.Quad(DKit.Shield, p + Vector2.up * .5f, new Vector2(.55f, .75f), 0, A(DKit.Steel, 1.1f * fade), t, Seed + k, 0);
                    b.Quad(FxShape.Glow, p, .8f, A(DKit.Ice, .35f * fade * u), 0, Seed);
                }
                b.Floor(DKit.Hex, at, r * 1.3f, A(DKit.Ice, .9f * Fade(t, .15f, 1)), t, Seed, 1);
                b.Floor(FxShape.Glow, at, r * 1.3f, A(DKit.Steel, .35f * Fade(t, 0, .9f)), 0, Seed);
            }
        }

        /// <summary>A big heater shield spins end over end along an arc, leaves hex ghosts behind it, bangs flat into the crowd and rings.</summary>
        private sealed class ShieldBolt : FxEffect
        {
            protected override float Life => 1.0f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At;
                if (t < 0)
                {
                    float u = pre;
                    for (int k = 6; k >= 1; k--)
                    {
                        float v = Mathf.Max(0, u - k * .045f); Vector2 q = DKit.Arc(From, at, v, 2.2f);
                        b.Quad(DKit.Hex, q, .5f * (1 - k / 8f), v * 700, A(DKit.Steel, .55f * (1 - k / 7f)), v, Seed + k, 1);
                    }
                    Vector2 p = DKit.Arc(From, at, u, 2.2f);
                    b.Quad(FxShape.Glow, p, 1.8f, A(DKit.Steel, .5f), 0, Seed);
                    b.Quad(DKit.Shield, p, new Vector2(.95f, 1.25f), u * 760, A(DKit.Ice, 1.15f), u, Seed, 0);
                    b.Floor(FxShape.Glow, at, .9f * u, A(DKit.Steel, .4f * u), 0, Seed);
                    return;
                }
                float sq = Fade(t, 0, .12f);
                b.Quad(FxShape.Glow, at, 2.0f, A(White, .8f * sq), 0, Seed);
                b.Quad(DKit.Shield, at + Vector2.up * .1f, new Vector2(1.2f + .35f * sq, (1.25f - .4f * sq) * .75f), 8, A(DKit.Ice, 1.1f * Fade(t, .4f, 1)), t, Seed, 0);
                for (int k = 0; k < 2; k++) { float u = Rise(t, k * .1f, .6f + k * .1f); if (u > 0 && u < 1) b.Quad(DKit.Hex, at, DKit.Sq(1.1f + 2f * EaseOut(u)), 30 * k, A(DKit.Steel, 1.1f * Fade(u, .3f, 1)), u, Seed + k, 1); }
                for (int k = 0; k < 8; k++)
                {
                    float u = t / (.5f + .2f * H(Seed, k)); if (u <= 0 || u >= 1) continue;
                    Glint(b, at + Dir(Seed, k, 8, .6f) * 1.6f * EaseOut(u) + Vector2.up * 1.1f * Mathf.Sin(u * 3), .24f, k % 2 == 0 ? DKit.Ice : DKit.Steel, 1 - u, k * 30 + t * 300);
                }
            }
        }

        /// <summary>Weather: a hexagonal fortress dome. Honeycomb floor that pulses cell by cell, six standing shields on the corners,
        /// a hovering ceiling plate and hex pings that run out from the middle.</summary>
        private sealed class Fortress : FxEffect
        {
            protected override float Life => 5f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                if (Moment.Kind != BondMomentKind.Weather) return;
                Vector2 at = At; float age = now - Born, on = Rise(age, 0, .5f) * Fade(now, End - .6f, End), R = 4.4f;
                float rise = EaseOut3(Rise(age, 0, .6f));
                b.Quad(DKit.Hex, at, DKit.Sq(R * 1.02f), 0, A(DKit.Steel, .55f * on), now, Seed, 2);
                b.Quad(FxShape.Glow, at + Vector2.up * .8f, new Vector2(R * 1.1f, R * .78f), 0, A(DKit.Steel, .2f * on), 0, Seed);
                b.Quad(DKit.Hex, at, DKit.Sq(R * 1.04f), now * 4, A(DKit.Ice, 1.1f * on), now, Seed + 1, 1);
                b.Quad(DKit.Hex, at, DKit.Sq(R * .96f), -now * 6, A(DKit.Steel, .8f * on), now, Seed + 2, 1);
                for (int k = 0; k < 6; k++)
                {
                    float ang = (k * 60 + 30) * Mathf.Deg2Rad; Vector2 p = at + new Vector2(Mathf.Cos(ang) * R * 1.03f, Mathf.Sin(ang) * R * .64f) + Vector2.up * (.55f + .12f * Mathf.Sin(now * 2 + k));
                    float rs = rise * on;
                    b.Quad(FxShape.Glow, p, 1.1f, A(DKit.Ice, .3f * rs), 0, Seed);
                    b.Quad(DKit.Shield, p + Vector2.up * (1 - rise) * -1.2f, new Vector2(.62f, .84f) * (.5f + .5f * rise), 0, A(DKit.Ice, 1.1f * rs), now, Seed + k, 0);
                }
                b.Quad(FxShape.Rays, at + Vector2.up * 3.1f, R * .9f, A(DKit.Ice, .25f * on), now * .2f, Seed, 3);
                b.Quad(DKit.Hex, at + Vector2.up * 3.1f, DKit.Sq(R * .75f), now * 10, A(DKit.Steel, .5f * on), now, Seed + 3, 0);
                for (int k = 0; k < 2; k++)
                {
                    float u = ((age * .8f) + k * .5f) % 1f; b.Quad(DKit.Hex, at, DKit.Sq(R * (.2f + .8f * EaseOut(u))), 30, A(DKit.Ice, .9f * on * Fade(u, .3f, 1)), u, Seed + 5, 1);
                }
                for (int k = 0; k < 16; k++)
                {
                    float u = (H(Seed, k) + now * (.15f + .1f * H(Seed, k + 40))) % 1f; Vector2 p = at + new Vector2((H(Seed, k + 9) - .5f) * R * 1.7f, (H(Seed, k + 19) - .5f) * R * 1.0f) + Vector2.up * u * 2.4f;
                    Glint(b, p, .2f, DKit.Ice, on * Mathf.Sin(u * Mathf.PI), k * 33 + now * 40);
                }
            }
        }
    }
}
