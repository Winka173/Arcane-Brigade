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
        public static readonly Color DustBrown = new Color(.55f, .47f, .4f, 1), DustGrey = new Color(.6f, .6f, .68f, 1);
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

        /// <summary>Hot little sparks thrown out under gravity, each a short streak with a glint at its head.</summary>
        public static void Sparks(FxBatch b, Vector2 at, float t, int n, float speed, float lift, Color c, int seed, float life = .55f, float size = .12f)
        {
            if (t <= 0) return;
            for (int k = 0; k < n; k++)
            {
                float lf = life * (.6f + .7f * H(seed, k + 200)), u = t / lf; if (u <= 0 || u >= 1) continue;
                Vector2 d = Dir(seed, k + 200, n, .75f); float sp = speed * (.5f + .8f * H(seed, k + 230)), up = lift * (.4f + H(seed, k + 260));
                float u0 = Mathf.Max(0, u - .12f);
                Vector2 p = SparkAt(at, d, sp, up, u * lf, u), q = SparkAt(at, d, sp, up, u0 * lf, u0);
                float a = 1 - u * u;
                b.Link(q, p, size * .5f, A(c, a), seed + k, .75f);
                Glint(b, p, size * 1.5f, k % 3 == 0 ? White : c, a, k * 37 + t * 200);
            }
        }
        private static Vector2 SparkAt(Vector2 at, Vector2 d, float sp, float up, float s, float v) => at + d * sp * s * (1 - .45f * v) + Vector2.up * (up * s - 5f * s * s);

        /// <summary>A ring of grainy ground dust kicked out low from an impact.</summary>
        public static void DustRing(FxBatch b, Vector2 at, float r, float t, Color c, int seed, int n = 8, float life = .9f, float alpha = .7f)
        {
            float u = t / life; if (u <= 0 || u >= 1) return;
            for (int k = 0; k < n; k++)
            {
                Vector2 d = Dir(seed, k + 300, n, .62f); float s = .5f + .35f * H(seed, k + 330);
                float a = alpha * Mathf.Clamp01(u * 6) * (1 - u) * (1 - u * .3f);
                b.Quad(FxShape.Dust, at + d * r * (.4f + .7f * EaseOut(u)) + Vector2.up * .3f * u, new Vector2(r * s * (.45f + .55f * u), r * s * .3f * (.6f + .8f * u)), 0, A(c, a), u, seed + k);
            }
        }

        /// <summary>Chunks thrown out on arcs that land and make one small bounce, with a puff of dust where they land.</summary>
        public static void Debris(FxBatch b, Vector2 at, float t, int n, float spread, float height, Color c, int seed, FxShape shape, float par, float size, float life = .8f, Color? dust = null)
        {
            if (t <= 0) return;
            for (int k = 0; k < n; k++)
            {
                float lf = life * (.7f + .5f * H(seed, k + 400)), u = t / lf; if (u <= 0 || u >= 1) continue;
                Vector2 d = Dir(seed, k + 400, n, .62f); float h = height * (.5f + .7f * H(seed, k + 430)), reach = spread * (.6f + .6f * H(seed, k + 460));
                float y = u < .7f ? 4 * h * (u / .7f) * (1 - u / .7f) : 4 * h * .16f * ((u - .7f) / .3f) * (1 - (u - .7f) / .3f);
                Vector2 ground = at + d * reach * (.25f + .75f * EaseOut(Mathf.Min(1, u / .7f)) + .12f * Mathf.Max(0, u - .7f));
                float spin = (u < .7f ? t * 520 : .7f * lf * 520 + (t - .7f * lf) * 160) * (k % 2 == 0 ? 1 : -1) + k * 50;
                b.Quad(shape, ground + Vector2.up * y, size * (.7f + .6f * H(seed, k + 490)), spin, A(c, 1.15f * Fade(u, .78f, 1)), 0, seed + k, par);
                if (dust.HasValue && u > .66f) { float v = (u - .66f) / .34f; b.Quad(FxShape.Dust, ground + Vector2.up * .12f * v, new Vector2(.3f + .35f * v, .16f + .14f * v), 0, A(dust.Value, .6f * (1 - v)), v, seed + k); }
            }
        }

        /// <summary>Tiny flecks drifting up through an area forever (a soft looping detail layer).</summary>
        public static void Motes(FxBatch b, Vector2 at, float w, float h, float now, int n, Color c, int seed, float alpha, float size = .1f, float speed = .35f)
        {
            if (alpha < .004f) return;
            for (int k = 0; k < n; k++)
            {
                float u = (H(seed, k + 500) + now * speed * (.6f + .8f * H(seed, k + 520)) / h) % 1f;
                Vector2 p = at + new Vector2((H(seed, k + 540) - .5f) * w + Mathf.Sin(now * 1.7f + k) * .15f, (u - .3f) * h);
                Glint(b, p, size * (.7f + .6f * H(seed, k + 560)), c, alpha * Mathf.Sin(u * Mathf.PI), now * 60 + k * 40);
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

        private static readonly Color Spark = new Color(1f, .82f, .5f, 1);
        private static float Bump(float t, float a, float b) => DKit.Bump(t, a, b);

        /// <summary>A giant hexagon plate slams flat onto the crowd, three hexagonal shock outlines run out, plates and metal sparks fly,
        /// grey dust rolls out low and the plate rings once more (a second, thinner hex pulse) before it fades.</summary>
        private sealed class GuardShockwave : FxEffect
        {
            protected override float Life => 1.5f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Radius * 1.05f;
                if (t < 0) { b.Floor(FxShape.Shade, at, r * .9f * pre, A(DKit.Dark, .45f * pre), 0, Seed); b.Floor(DKit.Hex, at, r * (1.4f - .4f * pre), A(DKit.Steel, .5f * pre), 0, Seed, 1); return; }
                b.Quad(FxShape.Glow, at, r * 1.5f, A(DKit.Ice, .7f * Fade(t, 0, .2f)), 0, Seed);
                b.Floor(FxShape.Shade, at, r * 1.0f, A(DKit.Dark, .5f * Fade(t, .6f, 1.3f)), 0, Seed + 3);
                float drop = 1.6f - .6f * EaseOut(Rise(t, 0, .12f));
                b.Quad(DKit.Hex, at, new Vector2(r * drop, r * drop * .62f), 12, A(DKit.Steel, 1.1f * Fade(t, .5f, 1.3f)), t, Seed, 0);
                b.Quad(DKit.Hex, at, new Vector2(r * drop * .6f, r * drop * .37f), 12, A(DKit.Ice, .9f * Fade(t, .3f, .9f)), t, Seed + 1, 0);
                // rivets on the plate's corners flash in turn
                for (int k = 0; k < 6; k++)
                {
                    float ang = (k * 60 + 12) * Mathf.Deg2Rad; Vector2 p = at + new Vector2(Mathf.Cos(ang) * r * drop * .8f, Mathf.Sin(ang) * r * drop * .5f);
                    Glint(b, p, .3f, White, Bump(t, .05f + k * .04f, .45f + k * .04f) * Fade(t, .5f, 1.2f), k * 30);
                }
                for (int k = 0; k < 3; k++)
                {
                    float u = Rise(t, .06f + k * .09f, .7f + k * .1f); if (u <= 0 || u >= 1) continue;
                    b.Quad(DKit.Hex, at, DKit.Sq(r * (.7f + 1.1f * EaseOut(u))), 12 + k * 20, A(k == 1 ? DKit.Ice : DKit.Steel, 1.1f * Fade(u, .35f, 1)), u, Seed + k, 1);
                }
                // second beat: the plate rings
                float ring = Rise(t, .42f, .95f); if (ring > 0 && ring < 1) b.Quad(DKit.Hex, at, DKit.Sq(r * (1.05f + .25f * ring)), 12 + Mathf.Sin(ring * 30) * 3, A(White, .7f * (1 - ring)), ring, Seed + 9, 1);
                b.Floor(FxShape.Shock, at, r * 1.45f, A(DKit.Steel, .8f), Rise(t, 0, .7f), Seed);
                DKit.DustRing(b, at, r * 1.15f, t, DKit.DustGrey, Seed, 9, 1.1f, .65f);
                DKit.Debris(b, at, t, 7, r * 1.3f, 1.3f, DKit.Ice, Seed, DKit.Hex, 0, .26f, .85f, DKit.DustGrey);
                DKit.Sparks(b, at + Vector2.up * .2f, t, 14, 5f, 3.2f, Spark, Seed, .55f, .1f);
                for (int k = 0; k < 10; k++)
                {
                    float life = .35f + .3f * H(Seed, k + 30), u = t / life; if (u <= 0 || u >= 1) continue;
                    Glint(b, at + Dir(Seed, k + 9, 10, .6f) * r * 1.2f * EaseOut(u) + Vector2.up * .5f * u, .22f, k % 3 == 0 ? White : DKit.Steel, 1 - u, k * 40 + t * 300);
                }
            }
        }

        /// <summary>Iron stun: six heater shields drop out of the air one after another into a clamp around the crowd, each landing with
        /// a clank of sparks and a dust puff; then steel bars snap between them and the clamp locks with a hex flash.</summary>
        private sealed class GuardLine : FxEffect
        {
            protected override float Life => 1.1f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                if (t < 0) return; Vector2 at = At; float r = Radius * 1.0f, fade = Fade(t, .65f, 1.1f);
                Vector2[] spot = new Vector2[6];
                for (int k = 0; k < 6; k++)
                {
                    float land = .2f + k * .035f, u = Rise(t, k * .035f, land); float ang = k * 60 * Mathf.Deg2Rad;
                    spot[k] = at + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang) * .62f) * r;
                    if (u <= 0) continue;
                    Vector2 p = spot[k] + Vector2.up * (1 - DKit.EaseIn(u)) * 3f;
                    float wob = u >= 1 ? Mathf.Sin((t - land) * 40) * 6 * Fade(t, land, land + .3f) : 0;
                    b.Quad(FxShape.Shade, spot[k], new Vector2(.5f, .22f), 0, A(DKit.Dark, .5f * fade * u), 0, Seed);
                    b.Quad(DKit.Shield, p + Vector2.up * .5f, new Vector2(.5f, .68f), wob, A(DKit.Steel, 1.1f * fade), t, Seed + k, 0);
                    b.Quad(FxShape.Glow, p + Vector2.up * .4f, .7f, A(DKit.Ice, .3f * fade * u), 0, Seed);
                    if (u >= 1)
                    {
                        float s = t - land;
                        DKit.Sparks(b, spot[k] + Vector2.up * .1f, s, 5, 3f, 2.4f, Spark, Seed + k * 11, .35f, .08f);
                        float v = Rise(s, 0, .5f); if (v < 1) b.Quad(FxShape.Dust, spot[k] + Vector2.up * .1f, new Vector2(.5f + .5f * v, .22f + .15f * v), 0, A(DKit.DustGrey, .7f * (1 - v)), v, Seed + k);
                    }
                }
                // the lock: bars snap from shield to shield
                float lk = Rise(t, .38f, .5f);
                if (lk > 0)
                    for (int k = 0; k < 6; k++)
                    {
                        Vector2 a = spot[k] + Vector2.up * .45f, z = Vector2.Lerp(a, spot[(k + 1) % 6] + Vector2.up * .45f, lk);
                        b.Link(a, z, .1f, A(DKit.Steel, .9f * fade), Seed + k, .5f); b.Link(a, z, .04f, A(White, .8f * fade), Seed + k, .9f);
                    }
                float flash = Bump(t, .48f, .75f);
                b.Floor(DKit.Hex, at, r * 1.2f, A(DKit.Ice, (.4f + .7f * flash) * Fade(t, .15f, 1)), t, Seed, 1);
                b.Floor(FxShape.Glow, at, r * 1.15f, A(DKit.Steel, (.25f + .3f * flash) * Fade(t, 0, .9f)), 0, Seed);
                for (int k = 0; k < 8; k++) Glint(b, at + Dir(Seed, k, 8, .6f) * r * .8f * (.5f + .5f * flash) + Vector2.up * (.3f + flash * .6f), .2f, White, flash, k * 45);
            }
        }

        /// <summary>A big heater shield spins end over end along an arc shedding sparks and hex ghosts, bangs flat into the crowd and
        /// rings: it wobbles, sparks spray off its rim, a dent of cracks and a puff of dust stay under it.</summary>
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
                        float v = Mathf.Max(0, u - k * .045f); Vector2 q = DKit.Arc(From, at, v, 2.0f);
                        b.Quad(DKit.Hex, q, .42f * (1 - k / 8f), v * 700, A(DKit.Steel, .5f * (1 - k / 7f)), v, Seed + k, 1);
                    }
                    for (int k = 0; k < 10; k++)
                    {
                        float v = Mathf.Max(0, u - .02f - k * .025f); Vector2 q = DKit.Arc(From, at, v, 2.0f) + new Vector2(H(Seed, k) - .5f, -(k * .07f)) * .5f;
                        Glint(b, q, .14f, k % 2 == 0 ? Spark : DKit.Ice, .9f * (1 - k / 10f), k * 50 + u * 400);
                    }
                    Vector2 p = DKit.Arc(From, at, u, 2.0f);
                    b.Quad(FxShape.Glow, p, 1.3f, A(DKit.Steel, .5f), 0, Seed);
                    b.Quad(DKit.Shield, p, new Vector2(.8f, 1.05f), u * 760, A(DKit.Ice, 1.15f), u, Seed, 0);
                    b.Floor(FxShape.Shade, at, .8f * u, A(DKit.Dark, .45f * u), 0, Seed);
                    return;
                }
                float sq = Fade(t, 0, .12f), wob = Mathf.Sin(t * 38) * 9 * Fade(t, 0, .6f);
                b.Quad(FxShape.Glow, at, 1.5f, A(White, .75f * sq), 0, Seed);
                b.Floor(FxShape.Cracks, at, 1.1f, A(DKit.Steel, .9f * Fade(t, .4f, 1)), Rise(t, 0, .12f), Seed, 1);
                b.Quad(DKit.Shield, at + Vector2.up * .15f, new Vector2(1.0f + .3f * sq, (1.05f - .35f * sq) * .78f), 8 + wob, A(DKit.Ice, 1.1f * Fade(t, .45f, 1)), t, Seed, 0);
                for (int k = 0; k < 2; k++) { float u = Rise(t, k * .1f, .6f + k * .1f); if (u > 0 && u < 1) b.Quad(DKit.Hex, at, DKit.Sq(.9f + 1.3f * EaseOut(u)), 30 * k, A(DKit.Steel, 1.1f * Fade(u, .3f, 1)), u, Seed + k, 1); }
                DKit.Sparks(b, at + Vector2.up * .5f, t, 12, 4.5f, 3.5f, Spark, Seed, .5f, .1f);
                DKit.DustRing(b, at, .9f, t, DKit.DustGrey, Seed, 6, .8f, .6f);
                for (int k = 0; k < 6; k++)
                {
                    float u = t / (.45f + .2f * H(Seed, k)); if (u <= 0 || u >= 1) continue;
                    Glint(b, at + Dir(Seed, k, 6, .6f) * 1.2f * EaseOut(u) + Vector2.up * .9f * Mathf.Sin(u * 3), .22f, k % 2 == 0 ? DKit.Ice : DKit.Steel, 1 - u, k * 30 + t * 300);
                }
            }
        }

        /// <summary>Weather: a light hexagonal fortress over the hero's side. A faint honeycomb floor pulses cell by cell, six standing shields
        /// on the corners with rivets that glint, a hovering ceiling plate, sparks that run round the rim and hex pings.</summary>
        private sealed class Fortress : FxEffect
        {
            protected override float Life => 5f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                if (Moment.Kind != BondMomentKind.Weather) return;
                Vector2 at = At; float age = now - Born, on = Rise(age, 0, .5f) * Fade(now, End - .6f, End), R = 3.0f;
                float rise = EaseOut3(Rise(age, 0, .6f));
                b.Quad(DKit.Hex, at, DKit.Sq(R * 1.02f), 0, A(DKit.Steel, .32f * on), now, Seed, 2);
                b.Quad(FxShape.Glow, at + Vector2.up * .6f, new Vector2(R * 1.05f, R * .7f), 0, A(DKit.Steel, .12f * on), 0, Seed);
                b.Quad(DKit.Hex, at, DKit.Sq(R * 1.04f), now * 4, A(DKit.Ice, .85f * on), now, Seed + 1, 1);
                b.Quad(DKit.Hex, at, DKit.Sq(R * .96f), -now * 6, A(DKit.Steel, .5f * on), now, Seed + 2, 1);
                Vector2[] corner = new Vector2[6];
                for (int k = 0; k < 6; k++)
                {
                    float ang = (k * 60 + 30) * Mathf.Deg2Rad; corner[k] = at + new Vector2(Mathf.Cos(ang) * R * .98f, Mathf.Sin(ang) * R * .61f);
                    Vector2 p = corner[k] + Vector2.up * (.45f + .1f * Mathf.Sin(now * 2 + k));
                    float rs = rise * on;
                    b.Quad(FxShape.Glow, p, .8f, A(DKit.Ice, .22f * rs), 0, Seed);
                    b.Quad(DKit.Shield, p + Vector2.up * (1 - rise) * -1f, new Vector2(.46f, .62f) * (.5f + .5f * rise), 0, A(DKit.Ice, 1.0f * rs), now, Seed + k, 0);
                    Glint(b, p + new Vector2(-.22f, .3f), .16f, White, rs * Mathf.Pow(.5f + .5f * Mathf.Sin(now * 3.1f + k * 1.7f), 6), now * 50);
                }
                // sparks run along the rim from corner to corner
                for (int k = 0; k < 3; k++)
                {
                    float u = (age * .7f + k / 3f) % 1f * 6; int i = Mathf.FloorToInt(u); float f = u - i;
                    Vector2 p = Vector2.Lerp(corner[i % 6], corner[(i + 1) % 6], f);
                    Glint(b, p, .3f, White, on * .9f, now * 300); b.Quad(FxShape.Glow, p, .5f, A(DKit.Steel, .4f * on), 0, Seed);
                }
                b.Quad(DKit.Hex, at + Vector2.up * 2.3f, DKit.Sq(R * .5f), now * 10, A(DKit.Steel, .35f * on), now, Seed + 3, 0);
                for (int k = 0; k < 2; k++)
                {
                    float u = ((age * .8f) + k * .5f) % 1f; b.Quad(DKit.Hex, at, DKit.Sq(R * (.2f + .8f * EaseOut(u))), 30, A(DKit.Ice, .6f * on * Fade(u, .3f, 1)), u, Seed + 5, 1);
                }
                DKit.Motes(b, at, R * 1.6f, 2.6f, now, 14, DKit.Ice, Seed, .8f * on, .13f, .45f);
            }
        }
    }
}
