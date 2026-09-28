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
                for (int k = 0; k < 4; k++)
                {
                    float h = (.7f + k * .7f) * r * col, s = r * (.95f - k * .16f) * (.4f + .6f * col);
                    b.Quad(Drop, at + Vector2.up * h, new Vector2(s * .8f, s), Mathf.Sin(t * 20 + k) * 8, A(k % 2 == 0 ? Aqua : Foam, .9f * col), 0, Seed + k, 0);
                }
                for (int k = 0; k < 14; k++)
                {
                    Vector2 dd = Dir(Seed, k, 14, .6f);
                    Drip(b, at + Vector2.up * .4f * r, new Vector2(dd.x * (2.2f + 1.6f * H(Seed, k)) * r, 3.5f + 2.5f * H(Seed, k + 5) + dd.y * 1.5f), 13, t - .12f, .8f, .5f + .3f * H(Seed, k + 3), k % 3 == 0 ? Foam : Aqua, Seed + k);
                }
                for (int k = 0; k < 9; k++)
                {
                    float u = Rise(t, .1f + k * .04f, .9f + k * .04f); if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + new Vector2((H(Seed, k + 30) - .5f) * r * 1.6f + Mathf.Sin(t * 8 + k) * .15f, .2f + u * 2f);
                    b.Quad(Bubble, p, .25f + .2f * H(Seed, k), A(Foam, .9f * (1 - u)), 0, Seed + k);
                }
            }
        }

        /// <summary>The riptide drags enemies: a flat spiral current turns on the floor, six water ribbons stream inward along it carrying foam beads, a dark dip opens in the middle.</summary>
        private sealed class Riptide : FxEffect
        {
            protected override float Life => 1.4f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                float r = Mathf.Clamp(Radius, 1.2f, 2.4f) * 1.7f, on = Rise(t, 0, .2f) * Fade(t, .9f, 1.4f); Vector2 at = At;
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
                    for (int j = 0; j < 3; j++)
                    {
                        float f = (t * 1.1f + j / 3f + k * .13f) % 1, rho = r * (1.15f - f * .95f), th = th0 + f * 2.4f;
                        b.Quad(Bubble, at + new Vector2(Mathf.Cos(th) * rho, Mathf.Sin(th) * rho * .6f), .32f * (1 - f * .5f), A(Foam, on * (1 - f)), 0, Seed + j);
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
                float R = Mathf.Max(1.6f, Moment.Radius) * (.6f + .4f * EaseOut(Rise(now - Born, 0, .6f))); Vector2 at = At;
                b.Floor(FxShape.Shade, at, R * 1.45f, A(Navy, .7f * on), 0, Seed);
                b.Floor(FxShape.Glow, at, R * 1.5f, A(Sea, .45f * on), 0, Seed);
                b.Floor(Swirl, at, R * 1.4f, A(Aqua, .6f * on), now * 1.6f, Seed, 3);
                b.Floor(Swirl, at, R * 1f, A(Foam, .55f * on), now * 2.6f, Seed + 5, -1);
                b.Floor(Swirl, at, R * .6f, A(Aqua, .8f * on), now * 4f, Seed + 9, 1);
                b.Floor(FxShape.Shade, at, R * .3f, A(new Color(0, .01f, .04f), .95f * on), 0, Seed);
                for (int k = 0; k < 26; k++)
                {
                    float a = k / 26f * 6.2832f + now * 1.4f, rr = R * (1.18f + .06f * Mathf.Sin(k * 3 + now * 5));
                    b.Quad(Bubble, at + new Vector2(Mathf.Cos(a) * rr, Mathf.Sin(a) * rr * .62f), .2f + .1f * H(Seed, k), A(Foam, .85f * on), 0, Seed + k);
                }
                for (int k = 0; k < 12; k++)
                {
                    float f = (H(Seed, k) + now * .45f) % 1, a = k * 2.1f + f * 7f, rho = R * (1.1f - f), s = .32f * (1 - f * .6f);
                    Vector2 p = at + new Vector2(Mathf.Cos(a) * rho, Mathf.Sin(a) * rho * .62f), tan = new Vector2(-Mathf.Sin(a), Mathf.Cos(a) * .62f);
                    if (k % 3 == 0) b.Quad(Bubble, p, s * .8f, A(Foam, on * (1 - f)), 0, Seed + k);
                    else b.Quad(Drop, p, new Vector2(s * .75f, s), Ang(tan) - 90, A(Aqua, on * (1 - f)), 0, Seed + k, 0);
                }
                for (int k = 0; k < 10; k++)
                {
                    float cyc = .8f + .3f * H(Seed, k), u = ((now + H(Seed, k + 3) * cyc) % cyc) / cyc, a = H(Seed, k + 7) * 6.2832f + Mathf.Floor((now + H(Seed, k + 3) * cyc) / cyc) * 2.3f;
                    Vector2 rim = at + new Vector2(Mathf.Cos(a) * R * 1.15f, Mathf.Sin(a) * R * .7f);
                    Drip(b, rim, new Vector2(Mathf.Cos(a) * 1.2f, 3.2f), 11, u * .6f, .6f, .3f, Foam, Seed + k);
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
                    float f = k / 6f, h = f * 5.2f * up * (1 - (1 - fall) * f), s = r * (.62f - f * .38f);
                    Vector2 p = at + new Vector2(Mathf.Sin(t * 14 + k) * .12f, h);
                    b.Quad(Drop, p, new Vector2(s * .85f, s * 1.1f), 0, A(k % 2 == 0 ? Aqua : Foam, .9f * fall), 0, Seed + k, 0);
                    b.Quad(FxShape.Glow, p, s * 1.6f, A(Aqua, .25f * fall), 0, Seed);
                }
                float crest = EaseOut(Rise(t, .05f, .8f)), cAl = Fade(t, .6f, 1.1f) * Rise(t, .05f, .1f);
                for (int k = 0; k < 22; k++)
                {
                    Vector2 dd = Dir(Seed + 4, k, 22, .62f), p = at + dd * r * (.3f + 1.05f * crest);
                    b.Quad(Drop, p + Vector2.up * .25f, new Vector2(.32f, .5f), dd.x * -25, A(k % 2 == 0 ? Foam : Aqua, .9f * cAl), 0, Seed + k, 0);
                    b.Quad(Bubble, p, .22f, A(Foam, .7f * cAl), 0, Seed + k);
                }
                for (int k = 0; k < 24; k++)
                {
                    Vector2 dd = Dir(Seed, k, 24, .6f);
                    Drip(b, at + Vector2.up * 4f * up, new Vector2(dd.x * r * (1f + 1.2f * H(Seed, k)), 1.5f + 2f * H(Seed, k + 5)), 11, t - .25f, 1f, .32f + .2f * H(Seed, k + 3), k % 3 == 0 ? Foam : Aqua, Seed + k);
                }
            }
        }

        /// <summary>Step 8. A sea serpent surfaces at one end and swims across the arena along the line: a weaving body of scaled segments with fins, a horned head with glowing eyes, foam and spray in its wake.</summary>
        private sealed class Leviathan : FxEffect
        {
            protected override float Life => 2.2f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 d = At - From; float len = d.magnitude; if (len < .1f) return;
                Vector2 dir = d / len, nrm = new Vector2(-dir.y, dir.x); float w = Mathf.Max(1.1f, Radius) * 1.25f;
                const int N = 15; const float gap = .85f; float body = N * gap;
                float age = now - Born, span = (Land - Born) + 1.9f, head = Mathf.Lerp(-1.5f, len + body + 1, Mathf.Clamp01(age / span));
                b.Beam(From, At, w * 1.1f, A(Sea, .3f * Rise(age, 0, .3f) * Fade(t, 1.4f, 2.2f)), now, Seed, .2f);
                if (head < 1.2f) for (int k = 0; k < 8; k++) Drip(b, From, new Vector2((H(Seed, k) - .5f) * 3, 3 + 2 * H(Seed, k + 1)), 10, (age * 1.6f + k * .13f) % .7f, .7f, .35f, Foam, Seed + k);
                for (int j = N - 1; j >= 0; j--)
                {
                    float s = head - j * gap; if (s < -.8f || s > len + .8f) continue;
                    float emerge = Mathf.Clamp01((s + .8f) / 1.4f) * Mathf.Clamp01((len + .8f - s) / 1.4f), taper = 1 - j / (float)N * .65f, sz = w * taper * (j == 0 ? 1.25f : 1f) * (.5f + .5f * emerge);
                    float wave = Mathf.Sin(s * .9f - now * 5f) * w * .9f;
                    Vector2 p = From + dir * s + nrm * wave, tan = (dir + nrm * Mathf.Cos(s * .9f - now * 5f) * w * .8f).normalized; float ta = Ang(tan);
                    b.Quad(FxShape.Shade, p, sz * 1.25f, A(Navy, .8f * emerge), 0, Seed);
                    b.Quad(FxShape.Glow, p, sz * 1.5f, A(Aqua, .35f * emerge), 0, Seed);
                    b.Quad(FxShape.Smoke, p, sz * .95f, A(new Color(.04f, .3f, .55f), .95f * emerge), 0, Seed + j);
                    b.Quad(Shard, p + new Vector2(-tan.y, tan.x) * sz * .15f, new Vector2(sz * .8f, sz * .35f), ta, A(Aqua, .8f * emerge), 0, Seed + j, 0);
                    if (j % 2 == 1 && j < N - 2)
                        for (int side = -1; side <= 1; side += 2)
                            b.Quad(Blade, p + new Vector2(-tan.y, tan.x) * side * sz * .85f, new Vector2(sz * .7f, sz * .5f), ta + 180 + side * 55, A(Foam, .75f * emerge), 0, Seed + j + side, 0);
                    if (j == 0)
                    {
                        Vector2 n2 = new Vector2(-tan.y, tan.x);
                        for (int side = -1; side <= 1; side += 2)
                        {
                            b.Quad(FxShape.Glow, p + tan * sz * .35f + n2 * side * sz * .38f, sz * .32f, A(new Color(1f, .9f, .3f), emerge), 0, Seed);
                            b.Quad(Shard, p - tan * sz * .2f + n2 * side * sz * .7f, new Vector2(sz * .7f, sz * .2f), ta + 180 - side * 25, A(Foam, .9f * emerge), 0, Seed + side, 0);
                        }
                        b.Quad(Shard, p + tan * sz * 1f, new Vector2(sz * .5f, sz * .3f), ta, A(Foam, .8f * emerge), 0, Seed + 70, 0);
                    }
                    if (j % 3 == 0)
                        for (int side = -1; side <= 1; side += 2)
                            Drip(b, p, new Vector2(-tan.y, tan.x) * side * 2.5f + Vector2.up * 2.5f, 10, (now * 1.7f + j * .2f) % .5f, .5f, .3f, Foam, Seed + j + side);
                }
                for (int k = 0; k < 24; k++)
                {
                    float s = len * H(Seed, k + 40), a = (head - s) / 3f; if (a <= 0 || a >= 1) continue;
                    Vector2 p = From + dir * s + nrm * (H(Seed, k + 60) - .5f) * w * 3.2f;
                    b.Quad(Bubble, p, .2f + .2f * H(Seed, k), A(Foam, .9f * (1 - a)), 0, Seed + k);
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
                b.Quad(FxShape.Shade, a.center, new Vector2(a.width * .75f, a.height * .75f), 0, A(new Color(.05f, .3f, .4f), .25f * on), 0, Seed);
                for (int k = 0; k < 3; k++)
                {
                    float u = (now * .06f + k / 3f) % 1; Vector2 c = new Vector2(a.xMin - 4 + u * (a.width + 8), a.center.y + (k - 1) * 3.5f);
                    b.Quad(FxShape.Glow, c, new Vector2(3.2f, a.height * .8f), 18, A(Aqua, .12f * on), 0, Seed);
                }
                for (int k = 0; k < 130; k++)
                {
                    float sp = 15 + 5 * H(Seed, k + 300), u = (H(Seed, k) + now * sp / H0) % 1;
                    Vector2 p = new Vector2(a.xMin + a.width * H(Seed, k + 150) + u * .3f, a.yMax + 2 - u * H0);
                    b.Beam(p, p + new Vector2(.06f, 1.1f), .03f, A(new Color(.6f, .9f, 1f), .55f * on), 0, k, 0);
                }
                for (int k = 0; k < 34; k++)
                {
                    float cyc = .55f + .4f * H(Seed, k + 9), ph = (now + H(Seed, k) * cyc) / cyc; int n = Mathf.FloorToInt(ph); float u = ph - n;
                    Vector2 p = new Vector2(a.xMin + a.width * H(Seed + n, k + 3), a.yMin + a.height * H(Seed + n, k + 4));
                    b.Floor(FxShape.Glow, p, .5f, A(Aqua, .35f * on * (1 - u)), 0, Seed);
                    for (int j = -1; j <= 1; j += 2) Drip(b, p, new Vector2(j * 1f, 2.2f), 12, u * .38f, .38f, .17f, Foam, Seed + k + j);
                }
            }
        }
    }
}
