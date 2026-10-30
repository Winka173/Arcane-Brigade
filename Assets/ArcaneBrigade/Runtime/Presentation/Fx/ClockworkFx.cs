using System;
using System.Collections.Generic;
using UnityEngine;
using static ArcaneBrigade.FxKit;

namespace ArcaneBrigade
{
    /// <summary>Clockwork bond: brass gears, springs, ticking and ripples in time. Gears are thrown, the clock face freezes the room, a
    /// giant pendulum sweeps a lane.</summary>
    public static class ClockworkFx
    {
        // owner 29/10/2026: small glints and sparks were drawn far too big: small ones shrink, big set-piece flashes keep their size
        private static void Glint(FxBatch b, Vector2 at, float size, Color tint, float alpha, float turn = 0)
            => FxKit.Glint(b, at, size <= .08f ? size : size < .5f ? .08f + (size - .08f) * .45f : size - .231f, tint, alpha, turn);
        private const FxShape Gear = (FxShape)40, Face = (FxShape)46, Coil = (FxShape)48, Rod = (FxShape)48;
        private static readonly Color Brass = new Color(1f, .74f, .26f, 1), Copper = new Color(1f, .5f, .22f, 1), Steel = new Color(.7f, .92f, 1f, 1), Teal = new Color(.3f, .95f, .85f, 1);

        public static void Register(Dictionary<string, Func<FxEffect>> map)
        {
            map["clock-gears"] = () => new ThrownGear();
            map["time-stop"] = () => new TimeStop();
            map["clock-pendulum"] = () => new Pendulum();
        }

        /// <summary>A solid brass cog (owner 29/10/2026: gears read as glowing light): the shape is shaded metal with an ink edge, its
        /// rotation passed so the light stays at the top-left while it turns; only a faint warm halo behind it.</summary>
        private static void Cog(FxBatch b, Vector2 p, float s, float rot, float teeth, Color c, float alpha, int seed)
        {
            if (alpha <= .004f) return;
            b.Quad(FxShape.Glow, p, s * 1.35f, A(c, alpha * .08f), 0, seed);
            b.Quad(Gear, p, new Vector2(s, s), rot, A(c, alpha), Mathf.Repeat(rot / 360f, 1), seed, teeth);
        }
        /// <summary>Step 2 gears. A big brass cog is flung by the caster, spinning on a lob; it bites into the enemy and grinds there while
        /// a steel pinion meshes against its teeth turning the other way, shavings curl off, oil spits, small cogs pop off and it vents
        /// steam as it winds down.</summary>
        private sealed class ThrownGear : FxEffect
        {
            protected override float Life => 1.05f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 from = From, at = At;
                float teeth = 10 + (Seed % 3) * 2, dir = Seed % 2 == 0 ? 1 : -1, R = 1.35f;
                if (t < 0)
                {
                    float u = EaseOut(pre); Vector2 p = Vector2.Lerp(from, at, u) + Vector2.up * (Mathf.Sin(pre * Mathf.PI) * 1.6f + .55f);
                    b.Floor(FxShape.Shade, Vector2.Lerp(from, at, u), .7f, A(new Color(.06f, .04f, .02f), .45f * pre), 0, Seed);
                    Cog(b, p, R * .85f, dir * pre * 1100, teeth, Brass, 1, Seed);
                    for (int k = 1; k <= 6; k++)
                    {
                        float back = Mathf.Max(0, pre - k * .04f); Vector2 q = Vector2.Lerp(from, at, EaseOut(back)) + Vector2.up * (Mathf.Sin(back * Mathf.PI) * 1.6f + .55f);
                        b.Link(q + Vector2.up * R * .8f, q + Vector2.down * R * .8f, .05f, A(Brass, .25f * (1 - k / 7f)), Seed + k, .7f);
                        Glint(b, q + Vector2.down * R * .7f, .16f * (1 - k / 7f), k % 2 == 0 ? Brass : Steel, .9f * (1 - k / 7f), k * 90);
                    }
                    return;
                }
                float fade = Fade(t, .7f, 1.05f), shake = Mathf.Sin(t * 90) * .05f * Fade(t, 0, .5f), grind = Fade(t, .6f, 1.0f);
                Vector2 c = at + new Vector2(shake, .6f);
                float spin = dir * (t * 700 * grind + 60 + t * 120);
                b.Floor(FxShape.Shade, at, 1.4f, A(new Color(.06f, .04f, .02f), .5f * fade), 0, Seed);
                Cog(b, c, R * (1 + .12f * Fade(t, 0, .12f)), spin, teeth, Brass, fade, Seed);
                // the meshing steel pinion on its flank turns the other way
                Vector2 pin = c + new Vector2(dir * R * 1.42f, -.25f);
                Cog(b, pin, R * .52f, -spin * teeth / 7f + 13, 7, Steel, fade * Rise(t, 0, .08f), Seed + 4);
                // small cogs pop off the teeth
                for (int k = 0; k < 3; k++)
                {
                    float u = (t - .05f - k * .07f) / .6f; if (u <= 0 || u >= 1) continue;
                    Vector2 p = c + new Vector2(Mathf.Cos(k * 2.1f + Seed) * 1.6f * EaseOut(u), 1.6f * u - 2.6f * u * u + .3f);
                    Cog(b, p, .34f, u * 900 + k * 40, 6, k == 1 ? Copper : Brass, 1f * (1 - u * u), Seed + k);
                }
                // the bite: hot sparks out of the teeth where they grind into the enemy
                for (int k = 0; k < 12; k++)
                {
                    float u = ((t * 3.2f + H(Seed, k)) % 1); if (t > .75f) break;
                    Vector2 d0 = Dir(Seed + 3, k, 12, .7f); Vector2 o = c + Vector2.down * R * .9f;
                    Vector2 p = o + new Vector2(d0.x * 1.4f, Mathf.Abs(d0.y) * .9f) * EaseOut(u) + Vector2.down * u * u * .5f;
                    b.Link(Vector2.Lerp(o, p, .65f), p, .04f, A(k % 3 == 0 ? White : Brass, 1 - u), Seed + k, .7f);
                }
                // metal shavings curl off as bright little streaks, oil spits
                for (int k = 0; k < 8; k++)
                {
                    float u = (t - .03f - (k % 4) * .08f) / .35f; if (u <= 0 || u >= 1) continue;
                    Vector2 d0 = Dir(Seed + 5, k, 8, .7f), p = c + d0 * (R * 1.05f + 1.1f * EaseOut(u)) + Vector2.down * u * u * .7f;
                    b.Link(p - d0 * .3f + Vector2.up * .1f * Mathf.Sin(u * 9), p, .05f, A(k % 2 == 0 ? Brass : Steel, 1 - u), Seed + k, .7f);
                }
                for (int k = 0; k < 5; k++)
                {
                    float u = (t - .1f - k * .05f) / .45f; if (u <= 0 || u >= 1) continue;
                    Vector2 p = c + new Vector2((k - 2) * .35f, -.2f) + new Vector2((k - 2) * .45f * u, 1.1f * u - 2.6f * u * u);
                    b.Quad(FxShape.Shade, p, .12f, A(new Color(.08f, .05f, .02f), .95f * (1 - u)), 0, Seed + k);
                    b.Quad(FxShape.Glow, p + new Vector2(-.03f, .03f), .05f, A(Brass, .8f * (1 - u)), 0, Seed + k);
                }
                // a low puff of grit kicked out under the bite, and steam vents as it winds down
                float du = Rise(t, 0, .7f);
                if (du < 1) for (int s = -1; s <= 1; s += 2) b.Quad(FxShape.Dust, at + new Vector2(s * (.5f + .9f * du), .05f), new Vector2(.55f + .4f * du, .25f + .12f * du), 0, A(new Color(.55f, .47f, .38f), .7f * (1 - du)), du, Seed + s);
                for (int k = 0; k < 5; k++)
                {
                    float u = Rise(t, .2f + k * .08f, .9f + k * .04f); if (u <= 0 || u >= 1) continue;
                    Vector2 p = c + new Vector2((H(Seed, k + 40) - .5f) * 1.4f + Mathf.Sin(u * 5 + k) * .2f, .4f + 1.5f * EaseOut(u));
                    b.Quad(FxShape.Smoke, p, .3f + .45f * u, 0, A(new Color(.85f, .82f, .76f), .65f * Mathf.Min(1, u * 4) * (1 - u)), u + k, Seed + k);
                }
            }
        }
        /// <summary>Step 6 time stop. A great clock face fades onto the arena floor; its hands race, slow and stop at twelve with a tick,
        /// the room tints to cold teal, gears and sparks hang frozen in the air and a ripple of broken seconds shivers over everything.
        /// As a strike (one creature) a small face stops above it.</summary>
        private sealed class TimeStop : FxEffect
        {
            protected override float Life => 1.5f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                bool weather = Moment.Kind == BondMomentKind.Weather;
                if (!weather && t >= 0) SandFreeze(b, now - Born, At, Mathf.Max(1.1f, Radius) * 1.3f);
                if (!weather && t < 0) return;
                float age = now - Born, total = weather ? Mathf.Max(1f, End - Born) : Life;
                float on = Rise(age, 0, .35f) * Fade(age, total - .6f, total);
                Rect a = Arena; Vector2 c = weather ? a.center : At;
                float size = weather ? 6.6f : Mathf.Max(1.1f, Radius) * 1.3f;
                // hands: race for the first moments, then ease to a dead stop at twelve (angle 0 / 2pi*k)
                float run = Rise(age, 0, .9f), angle = (1 - Mathf.Pow(1 - run, 3)) * Mathf.PI * 2 * 3;
                float tick = Fade(age, .9f, 1.1f);
                if (weather)
                {
                    b.Quad(FxShape.Shade, c, new Vector2(a.width * .8f, a.height * .8f), 0, A(new Color(.02f, .1f, .14f), .18f * on), 0, Seed);
                }
                b.Floor(FxShape.Glow, c, size * 1.1f, A(Teal, .35f * on), 0, Seed);
                b.Quad(Face, c, new Vector2(size, size * .62f), 0, A(Teal, 1.0f * on), 0, Seed, angle);
                b.Quad(Face, c + Vector2.up * .02f, new Vector2(size * 1.03f, size * .64f), 0, A(Steel, .35f * on), 0, Seed, angle);
                if (tick > 0) b.Floor(FxShape.Glow, c, size * .45f, A(Steel, .9f * tick), 0, Seed);
                // frozen gears and sparks hanging in the air
                int n = weather ? 12 : 5;
                for (int k = 0; k < n; k++)
                {
                    Vector2 p = weather ? new Vector2(a.xMin + a.width * H(Seed, k), a.yMin + a.height * H(Seed, k + 30)) : c + Dir(Seed, k, n, .6f) * size * 1.1f + Vector2.up * .7f;
                    float s = .22f + .22f * H(Seed, k + 9);
                    Cog(b, p + Vector2.up * Mathf.Sin(Mathf.Min(age, 1.1f) * 3 + k) * .1f, s, k * 40 + angle * 30 * (1 - run), 8, Steel, .85f * on, Seed + k);
                }
                for (int k = 0; k < (weather ? 40 : 14); k++)
                {
                    Vector2 p = weather ? new Vector2(a.xMin + a.width * H(Seed, k + 100), a.yMin + a.height * H(Seed, k + 160)) : c + Dir(Seed, k + 50, 14, .6f) * size * (.5f + H(Seed, k));
                    Glint(b, p, .14f, Teal, .75f * on * (.5f + .5f * Mathf.Sin(age * (k % 3 == 0 ? 14 : 2) + k)), k * 20);
                }
                // time ripples: thin broken arcs that shiver over the face
                for (int k = 0; k < 3; k++)
                {
                    float u = ((age * .9f) + k / 3f) % 1;
                    b.Floor(FxShape.Shock, c, size * (1.05f + .45f * u), A(Teal, .55f * on * (1 - u)), u, Seed + k);
                }
                // the tick that stops time: the face jolts and a crack of broken seconds splits across the glass
                float crack = Rise(age, .9f, 1f) * on;
                if (crack > 0)
                {
                    for (int k = 0; k < 6; k++)
                    {
                        Vector2 e = c + Dir(Seed + 8, k, 6, .62f) * size * .85f * crack, m = Vector2.Lerp(c, e, .5f) + Dir(Seed + 9, k, 6, .62f) * .2f;
                        b.Link(c, m, .04f, A(White, .8f * crack), Seed + k, .7f);
                        b.Link(m, e, .03f, A(Steel, .8f * crack), Seed + k + 9, .7f);
                    }
                    Glint(b, c + Vector2.up * size * .55f, .7f, White, Fade(age, .9f, 1.2f), 45);
                }
            }

            /// <summary>Grains of sand pour down round the face, then hang still in the air the instant the hands stop.</summary>
            private void SandFreeze(FxBatch b, float age, Vector2 c, float size)
            {
                float fall = Mathf.Min(age, .95f), on = Fade(age, 1.1f, 1.5f);
                for (int k = 0; k < 18; k++)
                {
                    float x = (H(Seed, k + 200) - .5f) * size * 2.2f, top = 1.6f + 1.4f * H(Seed, k + 201), sp = 2 + 2 * H(Seed, k + 202);
                    float y = top - ((fall * sp + H(Seed, k + 203) * 2.5f) % 2.6f);
                    Vector2 p = c + new Vector2(x, y);
                    b.Quad(FxShape.Glow, p, .09f, A(Brass, .9f * on), 0, Seed);
                    if (age > .95f) Glint(b, p, .16f, Teal, .7f * on * (.6f + .4f * Mathf.Sin(age * 9 + k)), k * 20);
                    else b.Beam(p, p + Vector2.up * .25f, .03f, A(Brass, .5f * on), 0, k, .5f);
                }
            }
        }

        /// <summary>Step 8 pendulum (line). A great brass pendulum hangs from a pivot cog above the field on a solid rod; its heavy engraved
        /// bob swings down along the lane, skimming the crowd at the bottom of its arc and rising at the far end, then swings back a little
        /// and settles. A soft swoosh of light trails the bob, its wind drags dust along the lane and its keel strikes sparks.</summary>
        private sealed class Pendulum : FxEffect
        {
            protected override float Life => 1.9f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 a = From, z = At, mid = (a + z) * .5f; float half = (z - a).magnitude * .5f;
                float w = Mathf.Max(.9f, Moment.Radius), rodLen = Mathf.Max(6.5f, half * 1.2f), bobR = .95f + .25f * w;
                Vector2 dir = (z - a).normalized, pivot = mid + Vector2.up * (rodLen + bobR * .55f);
                float u0 = t < 0 ? 0 : Mathf.Clamp01(t / 1.0f), back = t < 1 ? 0 : Mathf.Sin(Mathf.Clamp01((t - 1) / .9f) * Mathf.PI) * .14f;
                float s01 = t < 0 ? .02f * Mathf.Sin(now * 5) : (1 - Mathf.Cos(u0 * Mathf.PI)) * .5f - back;
                float fade = t < 0 ? Rise(pre, 0, .4f) : Fade(t, 1.35f, 1.9f);
                Vector2 pos = Bob(a, z, half, rodLen, bobR, Mathf.Clamp01(s01)); if (t < 0) pos += Vector2.up * (1 - pre) * 1.5f;
                Vector2 rod = pos - pivot; float rodDeg = Mathf.Atan2(rod.y, rod.x) * Mathf.Rad2Deg + 90;
                // the swoosh: faint arcs of light on the bob's recent path
                if (t >= 0 && t < 1.2f) for (int g = 1; g <= 10; g++)
                {
                    float sg = Mathf.Clamp01((1 - Mathf.Cos(Mathf.Max(0, u0 - g * .03f) * Mathf.PI)) * .5f), sh = Mathf.Clamp01((1 - Mathf.Cos(Mathf.Max(0, u0 - (g - 1) * .03f) * Mathf.PI)) * .5f);
                    float al = .4f * (1 - g / 11f) * Fade(t, .9f, 1.2f);
                    Vector2 pg = Bob(a, z, half, rodLen, bobR, sg), ph = Bob(a, z, half, rodLen, bobR, sh);
                    b.Link(pg, ph, bobR * .75f * (1 - g / 13f), A(Brass, al * .45f), Seed + g, 1);
                    b.Link(pg + Vector2.down * bobR * .55f, ph + Vector2.down * bobR * .55f, .06f, A(White, al), Seed + g, .8f);
                }
                // the rod and the pivot
                b.Quad(Rod, (pivot + pos) * .5f, new Vector2(.24f, rod.magnitude * .5f), rodDeg, A(Brass, fade), Mathf.Repeat(rodDeg / 360f, 1), Seed, 20);
                Cog(b, pivot, .9f, now * 30 + rodDeg * 2, 12, Copper, fade, Seed + 40);
                Cog(b, pivot, .38f, -now * 60, 8, Brass, fade, Seed + 41);
                // the bob, and its shadow running on the floor under it
                b.Floor(FxShape.Shade, new Vector2(pos.x, Vector2.Lerp(a, z, Mathf.Clamp01(s01)).y), bobR * (1.1f - .4f * Mathf.Clamp01((pos.y - mid.y) / 4f)), A(new Color(.06f, .04f, .02f), .55f * fade), 0, Seed);
                b.Quad(FxShape.Glow, pos, bobR * 1.4f, A(Brass, .1f * fade), 0, Seed);
                b.Quad(Rod, pos, new Vector2(bobR * 1.16f, bobR * 1.16f), rodDeg, A(new Color(.9f, .62f, .22f), fade), Mathf.Repeat(rodDeg / 360f, 1), Seed + 90, 21);
                if (t < 0) return;
                float skim = 1 - Mathf.Abs(s01 * 2 - 1);
                // ticking sparks shed along the lane where it passed
                for (int k = 0; k < 18; k++)
                {
                    float spawn = H(Seed, k + 70) * u0; float u = (u0 - spawn) / .25f; if (u <= 0 || u >= 1) continue;
                    Vector2 p = Vector2.Lerp(a, z, (1 - Mathf.Cos(spawn * Mathf.PI)) * .5f) + new Vector2(0, (H(Seed, k) - .5f) * w) + Vector2.up * (1.4f * u - 2f * u * u);
                    Glint(b, p, .16f, k % 2 == 0 ? Steel : Brass, 1 - u, k * 40 + t * 200);
                }
                // the bob's wind drags grainy dust along the lane under it, and its keel strikes sparks where it skims lowest
                for (int k = 0; k < 10; k++)
                {
                    float spawn = (k + .5f) / 10f, u = (s01 - spawn) / .3f; if (u <= 0 || u >= 1) continue;
                    Vector2 p = Vector2.Lerp(a, z, spawn) + dir * u * 1.2f;
                    b.Quad(FxShape.Dust, p, new Vector2(w * .9f, w * .45f) * (.6f + .7f * u), 0, A(new Color(.6f, .5f, .36f), .75f * (1 - u) * fade), u, Seed + k);
                }
                if (t < 1.1f && skim > .55f) for (int k = 0; k < 8; k++)
                {
                    float v = (t * 5 + H(Seed, k + 300)) % 1; Vector2 o = new Vector2(pos.x, pos.y - bobR * .9f);
                    Glint(b, o + new Vector2(-dir.x * v * 1.4f * (1 + H(Seed, k)), v * (1.2f - 1.6f * v)), .2f, k % 2 == 0 ? White : Brass, (1 - v) * skim, k * 33);
                }
            }
            /// <summary>The bob's path: along the lane, rising toward both ends on the pendulum's circle.</summary>
            private static Vector2 Bob(Vector2 a, Vector2 z, float half, float rodLen, float bobR, float s01)
            {
                float x = (s01 * 2 - 1) * half, k = Mathf.Clamp(x / rodLen, -.95f, .95f);
                return Vector2.Lerp(a, z, s01) + Vector2.up * (bobR * .55f + rodLen * (1 - Mathf.Sqrt(1 - k * k)) * 1.2f);
            }
        }
    }
}
