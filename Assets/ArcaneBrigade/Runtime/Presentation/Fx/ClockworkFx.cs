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
        private const FxShape Gear = (FxShape)40, Face = (FxShape)46, Coil = (FxShape)48;
        private static readonly Color Brass = new Color(1f, .74f, .26f, 1), Copper = new Color(1f, .5f, .22f, 1), Steel = new Color(.7f, .92f, 1f, 1), Teal = new Color(.3f, .95f, .85f, 1);

        public static void Register(Dictionary<string, Func<FxEffect>> map)
        {
            map["clock-gears"] = () => new ThrownGear();
            map["time-stop"] = () => new TimeStop();
            map["clock-pendulum"] = () => new Pendulum();
        }

        private static void Cog(FxBatch b, Vector2 p, float s, float rot, float teeth, Color c, float alpha, int seed)
        {
            b.Quad(FxShape.Glow, p, s * 1.6f, A(c, alpha * .18f), 0, seed);
            b.Quad(Gear, p, new Vector2(s, s), rot, A(c, alpha), 0, seed, teeth);
        }

        /// <summary>Step 2 gears. A brass cog is flung by the caster, rolling through the air on a lob with ticking sparks; it bites into
        /// the enemy, grinds there with three tiny cogs popping off the teeth, and rusts away into steam.</summary>
        private sealed class ThrownGear : FxEffect
        {
            protected override float Life => .95f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 from = From, at = At;
                float teeth = 8 + (Seed % 3) * 2, dir = Seed % 2 == 0 ? 1 : -1;
                if (t < 0)
                {
                    float u = EaseOut(pre); Vector2 p = Vector2.Lerp(from, at, u) + Vector2.up * Mathf.Sin(pre * Mathf.PI) * 1.5f;
                    Cog(b, p, .8f, dir * pre * 1100, teeth, Brass, .95f, Seed);
                    for (int k = 1; k <= 5; k++)
                    {
                        float back = Mathf.Max(0, pre - k * .05f); Vector2 q = Vector2.Lerp(from, at, EaseOut(back)) + Vector2.up * Mathf.Sin(back * Mathf.PI) * 1.5f;
                        Glint(b, q, .16f * (1 - k / 6f), k % 2 == 0 ? Brass : Steel, .9f * (1 - k / 6f), k * 90);
                    }
                    return;
                }
                float fade = Fade(t, .55f, .95f), shake = Mathf.Sin(t * 90) * .04f * Fade(t, 0, .5f);
                b.Floor(FxShape.Glow, at, Radius * 2f, A(Brass, .3f * fade), 0, Seed);
                Cog(b, at + new Vector2(shake, .45f), 1.05f * (1 + .15f * Fade(t, 0, .12f)), dir * (t * 700 + 60), teeth, Brass, .75f * fade, Seed);
                b.Quad(Gear, at + new Vector2(shake, .45f), new Vector2(.5f, .5f), -dir * t * 900, A(Copper, .8f * fade), 0, Seed + 4, 6);
                b.Quad(FxShape.Sparkle, at + Vector2.up * .35f, new Vector2(1.3f, 1.3f), 0, A(Steel, .5f * Fade(t, 0, .25f)), 0, 0, 2);
                for (int k = 0; k < 3; k++)
                {
                    float u = (t - .05f - k * .05f) / .55f; if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + new Vector2(Mathf.Cos(k * 2.1f + Seed) * 1.1f * EaseOut(u), .35f + 1.4f * u - 2.2f * u * u + .2f);
                    Cog(b, p, .3f, u * 900, 6, Brass, 1f * (1 - u), Seed + k);
                }
                for (int k = 0; k < 8; k++)
                {
                    float u = (t - .02f) / (.35f + .2f * H(Seed, k)); if (u <= 0 || u >= 1) continue;
                    Glint(b, at + Dir(Seed, k, 8, .6f) * .9f * EaseOut(u) + Vector2.up * .3f, .13f, k % 2 == 0 ? Steel : Brass, 1 - u, k * 30);
                }
                // metal shavings curl off the bite as bright little streaks, oil spits and the grinding gear vents puffs of steam
                for (int k = 0; k < 10; k++)
                {
                    float u = (t - .03f - (k % 5) * .07f) / .3f; if (u <= 0 || u >= 1) continue;
                    Vector2 d0 = Dir(Seed + 3, k, 10, .7f), p = at + Vector2.up * .45f + d0 * (.8f + 1.3f * EaseOut(u)) + Vector2.down * u * u * .6f;
                    b.Link(p - d0 * .32f, p, .045f, A(k % 2 == 0 ? Brass : White, 1 - u), Seed + k, .7f);
                }
                for (int k = 0; k < 5; k++)
                {
                    float u = (t - .1f - k * .05f) / .45f; if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + new Vector2((k - 2) * .35f, .2f) + new Vector2((k - 2) * .4f * u, 1.1f * u - 2.4f * u * u);
                    b.Quad(FxShape.Shade, p, .1f, A(new Color(.08f, .05f, .02f), .9f * (1 - u)), 0, Seed + k);
                    b.Quad(FxShape.Glow, p + new Vector2(-.03f, .03f), .06f, A(Brass, .8f * (1 - u)), 0, Seed + k);
                }
                for (int k = 0; k < 5; k++)
                {
                    float u = Rise(t, .08f + k * .09f, .75f + k * .05f); if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + new Vector2((H(Seed, k + 40) - .5f) * 1.2f + Mathf.Sin(u * 5 + k) * .2f, .7f + 1.5f * EaseOut(u));
                    b.Quad(FxShape.Smoke, p, .3f + .45f * u, 0, A(new Color(.85f, .82f, .76f), .7f * Mathf.Min(1, u * 4) * (1 - u)), u + k, Seed + k);
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

        /// <summary>Step 8 pendulum (line). A giant brass pendulum hangs from the top of the screen on a chain of links and swings along the
        /// lane, its heavy gear-bob dragging blurred copies of itself, ticking sparks and a brass shock behind it.</summary>
        private sealed class Pendulum : FxEffect
        {
            protected override float Life => 1.9f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 a = From, z = At, mid = (a + z) * .5f; float half = (z - a).magnitude * .5f;
                float w = Mathf.Max(.9f, Moment.Radius);
                Vector2 pivot = mid + Vector2.up * 9.5f;
                // swing: starts raised at the first end, crosses the lane in 1.0s, then settles
                Func<float, Vector2> bob = u =>
                {
                    float s = -Mathf.Cos(Mathf.Clamp01(u) * Mathf.PI);                  // -1..1
                    return new Vector2(mid.x + s * half, mid.y + 3.2f * (1 - Mathf.Cos(s * 1.15f)) / (1 - Mathf.Cos(1.15f)) * .0f + Mathf.Abs(s) * 2.4f + .6f);
                };
                float u0 = t < 0 ? 0 : Mathf.Clamp01(t / 1.1f), fade = t < 0 ? Rise(pre, 0, .4f) : Fade(t, 1.4f, 1.9f);
                Vector2 pos = t < 0 ? bob(0) + new Vector2(Mathf.Sin(now * 5) * .08f, 0) : bob(u0);
                // the lane being swept: a warm brass wash that grows with the bob
                float sweep = t < 0 ? 0 : u0;
                Vector2 dir = (z - a).normalized;
                b.Beam(a - dir * .3f, a + (z - a) * (t < 0 ? 0 : sweep), w, A(Brass, .35f * fade), t, Seed, 0);
                if (t < 0) b.Beam(a, z, w * .5f, A(Brass, .3f * pre), now, Seed, .6f);
                // chain
                int links = 12;
                for (int k = 0; k <= links; k++)
                {
                    Vector2 p = Vector2.Lerp(pivot, pos + Vector2.up * .5f, k / (float)links);
                    b.Quad(Gear, p, new Vector2(.2f, .2f), k * 30 + now * 40, A(Brass, .9f * fade), 0, Seed + k, 6);
                    if (k < links) b.Link(p, Vector2.Lerp(pivot, pos + Vector2.up * .5f, (k + 1) / (float)links), .05f, A(Copper, .7f * fade), Seed, .5f);
                }
                Cog(b, pivot, .7f, now * 30, 10, Copper, fade, Seed + 40);
                // motion-blurred ghosts of the bob
                if (t >= 0) for (int g = 5; g >= 1; g--)
                {
                    float ug = Mathf.Max(0, u0 - g * .045f); Vector2 gp = bob(ug);
                    Cog(b, gp, 1.15f * w * .75f, -ug * 600, 12, Brass, .28f * fade * (1 - g / 6f), Seed + g);
                }
                float big = 1.4f * w;
                Cog(b, pos, big, -u0 * 700 + now * 40, 12, Brass, 1.2f * fade, Seed + 90);
                b.Quad(Gear, pos, new Vector2(big * .55f, big * .55f), u0 * 900, A(Copper, 1f * fade), 0, Seed + 91, 6);
                b.Floor(FxShape.Shade, new Vector2(pos.x, mid.y), big * .9f, A(new Color(.06f, .04f, .02f), .5f * fade), 0, Seed);
                // ticking sparks shed along the lane
                if (t >= 0) for (int k = 0; k < 18; k++)
                {
                    float spawn = H(Seed, k + 70) * u0; float u = (u0 - spawn) / .25f; if (u <= 0 || u >= 1) continue;
                    Vector2 p = Vector2.Lerp(a, z, spawn) + new Vector2(0, (H(Seed, k) - .5f) * w) + Vector2.up * (1.4f * u - 2f * u * u);
                    Glint(b, p, .16f, k % 2 == 0 ? Steel : Brass, 1 - u, k * 40 + t * 200);
                }
                if (t >= 0) b.Floor(FxShape.Shock, new Vector2(pos.x, mid.y), w * 1.6f, A(Brass, .7f * Fade(t, 0, 1.3f)), Mathf.Min(1, t * .8f), Seed);
                // the bob's wind drags grainy dust along the lane under it, and its keel strikes sparks where it skims lowest
                if (t >= 0) for (int k = 0; k < 8; k++)
                {
                    float spawn = (k + .5f) / 8f, u = (u0 - spawn) / .3f; if (u <= 0 || u >= 1) continue;
                    Vector2 p = Vector2.Lerp(a, z, spawn) + dir * u * 1.2f;
                    b.Quad(FxShape.Dust, p, new Vector2(w * .9f, w * .45f) * (.6f + .7f * u), 0, A(new Color(.6f, .5f, .36f), .75f * (1 - u) * fade), u, Seed + k);
                }
                float skim = 1 - Mathf.Abs(-Mathf.Cos(u0 * Mathf.PI));
                if (t >= 0 && t < 1.1f && skim > .55f) for (int k = 0; k < 6; k++)
                {
                    float v = (t * 5 + H(Seed, k + 300)) % 1; Vector2 o = new Vector2(pos.x, mid.y + .2f);
                    Glint(b, o + new Vector2(-dir.x * v * 1.4f * (1 + H(Seed, k)), v * (1.2f - 1.6f * v)), .18f, k % 2 == 0 ? White : Brass, (1 - v) * skim, k * 33);
                }

            }
        }
    }
}
