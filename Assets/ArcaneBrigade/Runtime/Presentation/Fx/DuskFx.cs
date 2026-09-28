using System;
using System.Collections.Generic;
using UnityEngine;
using static ArcaneBrigade.FxKit;

namespace ArcaneBrigade
{
    /// <summary>Dusk bond: black ink, void, shadow, violet rim light and the eclipse. execute = a crossed double rift that swallows the
    /// target; shade-execute-growth = a shadow that swells with glowing eyes; dusk-pit = a black whirlpool of ink; dusk-pit-burst = the
    /// pit erupting into ink spikes; eclipse = a black moon with a corona over the arena.</summary>
    public static class DuskFx
    {
        public static void Register(Dictionary<string, Func<FxEffect>> map)
        {
            map["execute"] = () => new Execute();
            map["shade-execute-growth"] = () => new ShadeGrowth();
            map["dusk-pit"] = () => new Pit();
            map["dusk-pit-burst"] = () => new PitBurst();
            map["eclipse"] = () => new Eclipse();
            map["dusk-mark"] = () => new Mark();
        }

        private const FxShape Rift = (FxShape)35, Ring = (FxShape)36;
        private static readonly Color Ink = new Color(.01f, 0f, .035f), Violet = new Color(.6f, .3f, 1f), Magenta = new Color(.95f, .35f, .95f), Indigo = new Color(.14f, .07f, .36f);

        private static void Blob(FxBatch b, Vector2 p, Vector2 size, float deg, float alpha, float prog, int seed) => b.Quad(FxShape.Smoke, p, size, deg, A(Ink, alpha), prog, seed, 0);
        private static void Blob(FxBatch b, Vector2 p, float size, float deg, float alpha, float prog, int seed) => Blob(b, p, new Vector2(size, size), deg, alpha, prog, seed);

        /// <summary>Trait: two black slashes cross over the target and tear open as violet-rimmed rifts; the ink around is dragged into
        /// them, then pale souls drift up out of the cut.</summary>
        private sealed class Execute : FxEffect
        {
            protected override float Life => 1.7f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Radius * 1.3f, off = Fade(t, 1.0f, 1.7f), open = EaseOut(Rise(t, 0, .16f));
                Vector2 c = at + Vector2.up * .35f;
                b.Quad(FxShape.Glow, c, r * 1.9f, A(Violet, .8f * Fade(t, 0, .3f)), 0, Seed);
                b.Floor(FxShape.Shade, at, r * 1.4f, A(Ink, .8f * Rise(t, 0, .2f) * off), 0, Seed);
                // the two rifts, one after the other
                for (int k = 0; k < 2; k++)
                {
                    float o = Mathf.Clamp01(open * (k == 0 ? 1 : EaseOut(Rise(t, .08f, .24f)) )), deg = k == 0 ? 36 : -36;
                    b.Quad(FxShape.Glow, c, new Vector2(r * 1.4f, r * .35f), deg, A(Magenta, .45f * o * off), 0, Seed);
                    b.Quad(Rift, c, new Vector2(r * 1.55f, r * .8f), deg, A(Violet, 1.3f * off), o, Seed + k, 1);
                    b.Quad(Rift, c, new Vector2(r * 1.4f, r * .5f), deg, A(White, .8f * off * Fade(t, .1f, .6f)), o, Seed + k + 3, 0);
                }
                // the ink is pulled in
                for (int k = 0; k < 9; k++)
                {
                    float u = Rise(t, .05f + .02f * k, .55f); if (u <= 0 || u >= 1) continue;
                    Vector2 from = at + Dir(Seed, k, 9, .7f) * r * 1.5f, p = Vector2.Lerp(from, c, EaseOut(u));
                    Blob(b, p, new Vector2(.5f * (1 - u * .6f), .32f), Mathf.Atan2(c.y - from.y, c.x - from.x) * Mathf.Rad2Deg, .95f * (1 - u * u), u, Seed + k);
                    Glint(b, p + Vector2.up * .1f, .25f, Violet, 1 - u, k * 40 + now * 100);
                }
                Blob(b, c, new Vector2(r * .55f, r * .5f) * (1 - Rise(t, .4f, 1.0f) * .8f), 0, .95f * off * Rise(t, .2f, .4f), t, Seed + 20);
                // souls drift out of the cut
                for (int k = 0; k < 7; k++)
                {
                    float u = (t - .3f - k * .08f) / 1.0f; if (u <= 0 || u >= 1) continue;
                    Vector2 p = c + new Vector2(Mathf.Sin(u * 6 + k * 2) * .5f + (k - 3) * .18f, 2.7f * EaseOut(u));
                    b.Quad(FxShape.Glow, p, .5f, A(Magenta, .4f * (1 - u)), 0, Seed);
                    Glint(b, p, .3f * (1 - u * .4f), k % 2 == 0 ? Violet : White, 1.1f * (1 - u), now * 50 + k * 30);
                }
            }
        }

        /// <summary>Trait: the target's own shadow grows. Black columns of ink rise out of the ground around it, a tall shadow swells
        /// in the middle and opens two magenta eyes, violet motes spiral inwards; it then shrinks back into the floor.</summary>
        private sealed class ShadeGrowth : FxEffect
        {
            protected override float Life => 2f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Radius, on = Rise(t, 0, .2f), off = Fade(t, 1.5f, 2f);
                b.Floor(FxShape.Shade, at, r * 1.5f, A(Ink, .85f * on * off), 0, Seed);
                b.Floor(FxShape.Glow, at, r * 1.0f, A(Violet, .2f * on * off), 0, Seed);
                for (int k = 0; k < 8; k++)
                {
                    float born = .05f + .3f * H(Seed, k), g = EaseOut3(Rise(t, born, born + .45f)) * Fade(t, 1.3f + .1f * H(Seed, k + 3), 1.9f);
                    if (g <= 0) continue;
                    float h = (1.0f + 1.1f * H(Seed, k + 9)) * (.7f + .3f * r), w = .17f + .1f * H(Seed, k + 5);
                    Vector2 root = at + new Vector2((H(Seed, k) - .5f) * 2 * r * .95f, (H(Seed, k + 12) - .5f) * r * .5f);
                    float sway = Mathf.Sin(now * 4 + k * 2) * .12f;
                    Blob(b, root + new Vector2(sway * g, h * g * .5f), new Vector2(w * 2.2f, h * g * .55f), 0, .98f, t, Seed + k);
                    b.Quad(FxShape.Glow, root + new Vector2(sway * g, h * g * .5f), new Vector2(w * 2.6f, h * g * .62f), 0, A(Violet, .28f * g), 0, Seed);
                    Glint(b, root + new Vector2(sway * g, h * g), .28f, Magenta, .9f * g, now * 30 + k * 50);
                }
                float body = EaseOut3(Rise(t, .15f, .75f)) * Fade(t, 1.35f, 2f);
                float bh = r * 1.25f + .6f, bw = r * .55f + .3f;
                Blob(b, at + new Vector2(0, bh * body * .55f), new Vector2(bw * 1.25f, bh * body * .6f), 0, 1, t * .6f, Seed + 40);
                Blob(b, at + new Vector2(0, bh * body * .5f), new Vector2(bw * .95f, bh * body * .5f), 0, 1, t * .6f + .3f, Seed + 41);
                b.Quad(FxShape.Glow, at + new Vector2(0, bh * body * .55f), new Vector2(bw * 1.7f, bh * body * .75f), 0, A(Violet, .22f * body), 0, Seed);
                // two slit eyes open, blink once
                float eyes = Rise(t, .5f, .65f) * Fade(t, 1.25f, 1.45f) * (t > .95f && t < 1.02f ? .1f : 1);
                Vector2 eye = at + new Vector2(0, bh * body * .72f);
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector2 e = eye + new Vector2(s * bw * .32f, 0);
                    b.Quad(FxShape.Glow, e, new Vector2(.85f, .36f), 0, A(Magenta, .8f * eyes), 0, Seed);
                    b.Quad(FxShape.Glow, e, new Vector2(.5f, .16f), s * -12, A(White, 1f * eyes), 0, Seed);
                }
                for (int k = 0; k < 12; k++)
                {
                    float u = ((t * .9f) + H(Seed, k + 60)) % 1; float ang = k * .52f + u * 4, rad = r * 1.5f * (1 - u) + .15f;
                    Glint(b, at + new Vector2(Mathf.Cos(ang) * rad, Mathf.Sin(ang) * rad * .6f + .2f + u * .6f), .2f, k % 3 == 0 ? Magenta : Violet, .95f * Mathf.Sin(u * Mathf.PI) * off, k * 30 + now * 90);
                }
            }
        }

        /// <summary>Step 6 zone: a pit of black ink. A lumpy black pool with a violet rim light, ink strands and chunks dragged
        /// spiralling into the middle, a violet whirl and cracked light on the floor around.</summary>
        private sealed class Pit : FxEffect
        {
            protected override float Life => Moment.Duration + .5f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Radius, dur = Moment.Duration, on = EaseOut(Rise(t, 0, .3f)) * Fade(t, dur - .1f, dur + .5f), pulse = 1 + .03f * Mathf.Sin(now * 9);
                b.Floor(FxShape.Shade, at, r * 1.5f * on, A(Indigo, .7f * on), 0, Seed);
                b.Floor(FxShape.Cracks, at, r * 1.35f * on, A(Violet, .3f * on), Rise(t, 0, .4f), Seed, 1);
                for (int k = 0; k < 4; k++)
                {
                    float ang = k * 1.7f + now * .4f; Vector2 o = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang) * .62f) * r * .12f;
                    Blob(b, at + o, new Vector2(r * (1.05f - k * .04f) * pulse, r * (1.05f - k * .04f) * .62f * pulse) * on, now * 15 * (k % 2 == 0 ? 1 : -1), 1, now * .3f + k, Seed + k);
                }
                b.Quad(FxShape.Vortex, at, new Vector2(r * .95f, r * .95f * .62f) * on, 0, A(Violet, .22f * on), now * .7f, Seed, 2);
                b.Quad(FxShape.Vortex, at, new Vector2(r * .6f, r * .6f * .62f) * on, 0, A(Magenta, .22f * on), -now * .9f, Seed + 4, 1);
                b.Quad(Ring, at, new Vector2(r * 1.12f * pulse, r * 1.12f * .62f * pulse) * on, 0, A(Violet, .85f * on), now * 1.5f, Seed, .6f);
                b.Quad(Ring, at, new Vector2(r * .7f, r * .7f * .62f) * on, 0, A(Magenta, .8f * on), -now * 1.1f, Seed + 3, .3f);
                // ink strands pulled towards the centre, with chunks and sparks spiralling in
                for (int k = 0; k < 10; k++)
                {
                    float ang = k * .628f + now * 1.2f; Vector2 pos = at + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang) * .62f) * r * .92f;
                    Blob(b, pos, new Vector2(r * .3f, .1f) * on, (ang * Mathf.Rad2Deg + 150), .9f, now, Seed + 10 + k);
                }
                for (int k = 0; k < 22; k++)
                {
                    float u = (now * (.7f + .4f * H(Seed, k)) + H(Seed, k + 30)) % 1, ang = k * 0.9f + u * 5.5f, rad = r * 1.3f * (1 - u) + .05f;
                    Vector2 p = at + new Vector2(Mathf.Cos(ang) * rad, Mathf.Sin(ang) * rad * .62f);
                    float a = Mathf.Sin(u * Mathf.PI) * on;
                    if (k % 2 == 0) Glint(b, p, .24f * (1 - u * .5f), k % 4 == 0 ? Magenta : Violet, 1.1f * a, k * 50 + now * 80);
                    else Blob(b, p, .22f * (1 - u * .6f), 0, .95f * a, u, Seed + k);
                }
            }
        }

        /// <summary>Step 6 end: the pit erupts. Black ink spikes shoot up all over the footprint with violet rims, the ink slaps out across
        /// the floor, then falls back as inky rain.</summary>
        private sealed class PitBurst : FxEffect
        {
            protected override float Life => 1.7f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Radius, off = Fade(t, .9f, 1.7f);
                b.Quad(FxShape.Glow, at + Vector2.up * .6f, r * 1.7f, A(Violet, .9f * Fade(t, 0, .25f)), 0, Seed);
                b.Floor(FxShape.Shade, at, r * 1.5f, A(Ink, .8f * off), 0, Seed);
                float slap = EaseOut(Rise(t, 0, .25f));
                for (int k = 0; k < 4; k++) Blob(b, at + new Vector2((k - 1.5f) * r * .12f, 0), new Vector2(r * (.5f + .65f * slap), r * (.5f + .65f * slap) * .62f), k * 40, .98f * off, k * .3f, Seed + k);
                b.Quad(Ring, at, new Vector2(r * 1.15f * slap, r * 1.15f * slap * .62f), 0, A(Violet, 1.3f * Fade(t, 0, .9f)), t * 2, Seed, .8f);
                for (int k = 0; k < 12; k++)
                {
                    float s0 = .03f * k * 1.3f, g = EaseOut3(Rise(t, s0, s0 + .2f)) * Fade(t, .55f + .04f * k, 1.1f + .04f * k);
                    if (g <= 0) continue;
                    float h = 1.7f + 1.9f * H(Seed, k + 5), w = .2f + .12f * H(Seed, k + 9);
                    Vector2 root = at + Dir(Seed, k, 12, .6f) * r * (.25f + .65f * H(Seed, k + 17));
                    float lean = (H(Seed, k + 3) - .5f) * .8f;
                    Vector2 top = root + new Vector2(lean * h * g * .35f, h * g);
                    Blob(b, (root + top) * .5f, new Vector2(w * 2.4f, h * g * .56f), -lean * 18, 1, t, Seed + 30 + k);
                    Blob(b, (root + top) * .5f, new Vector2(w * 1.8f, h * g * .5f), -lean * 18, 1, t + .4f, Seed + 60 + k);
                    b.Quad(FxShape.Glow, (root + top) * .5f, new Vector2(w * 3.2f, h * g * .58f), -lean * 18, A(Violet, .3f * g), 0, Seed);
                    b.Quad(FxShape.Glow, root, w * 3f, A(Violet, .3f * g), 0, Seed);
                    Glint(b, top, .3f, Magenta, g, now * 40 + k * 30);
                }
                for (int k = 0; k < 20; k++)
                {
                    float life = .7f + .5f * H(Seed, k + 40), u = (t - .2f) / life; if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + Dir(Seed + 5, k, 20, .7f) * r * 1.1f * EaseOut(u) + Vector2.up * (3.8f * H(Seed, k + 60) * u - 5.2f * u * u + .4f);
                    if (k % 2 == 0) Blob(b, p, .2f + .12f * H(Seed, k), 0, .95f * (1 - u), u, Seed + k);
                    else Glint(b, p, .26f, Violet, 1 - u, k * 60 + now * 200);
                }
            }
        }

        /// <summary>Step 8 weather: the eclipse. The arena sinks into violet dusk, a black moon hangs over it ringed with a flaring corona
        /// and a thin bright rim, and motes of cold light drip down from it. As a trait it is a total eclipse flash: a black moon sweeps over
        /// the arena throwing god rays down onto the floor.</summary>
        private sealed class Eclipse : FxEffect
        {
            protected override float Life => 2.4f;
            private static void Moon(FxBatch b, Vector2 m, float R, float alpha, float now, int seed, float corona)
            {
                b.Quad(FxShape.Glow, m, R * 3.2f, A(Indigo, .55f * alpha * corona), 0, seed);
                b.Quad(Ring, m, R * 2.5f, 0, A(Violet, 1.2f * alpha * corona), now * .3f, seed, 3);
                b.Quad(Ring, m, R * 1.9f, 0, A(Magenta, .8f * alpha * corona), -now * .5f, seed + 2, 4);
                for (int k = 0; k < 7; k++) Blob(b, m + Dir(seed, k, 7, 1) * R * .42f, R * .9f, 0, alpha, k * .3f, seed + k);
                Blob(b, m, R * 1.08f, 0, alpha, .2f, seed + 9);
                b.Quad(Ring, m, R * 1.12f, 215, A(new Color(.9f, .8f, 1f), .6f * alpha), 0, seed, 6);
                Glint(b, m + new Vector2(-.7f, -.62f) * R * 1.02f, R * .5f * (.8f + .2f * Mathf.Sin(now * 5)), White, alpha, now * 30);
            }

            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Rect a = Arena;
                if (Moment.Kind == BondMomentKind.Weather)
                {
                    float on = Rise(t, 0, 2.5f) * Fade(now, End - 1.5f, End);
                    Vector2 m = new Vector2(a.center.x + a.width * .1f, a.center.y + a.height * .2f);
                    b.Quad(FxShape.Shade, a.center, new Vector2(a.width * .75f, a.height * .75f), 0, A(new Color(.07f, .02f, .2f), .52f * on), 0, Seed);
                    b.Quad(FxShape.Shade, new Vector2(a.center.x, a.yMax), new Vector2(a.width * .6f, a.height * .3f), 0, A(Ink, .6f * on), 0, Seed + 1);
                    Moon(b, m, 2.1f, on, now, Seed, .85f + .15f * Mathf.Sin(now * 1.3f));
                    for (int k = 0; k < 34; k++)
                    {
                        float u = (now * (.1f + .06f * H(Seed, k)) + H(Seed, k + 10)) % 1;
                        Vector2 p = m + new Vector2((H(Seed, k + 20) - .5f) * 7 * u + Mathf.Sin(now + k) * .3f, -u * (a.height * .8f));
                        Glint(b, p, .18f, k % 3 == 0 ? Magenta : Violet, .85f * on * Mathf.Sin(u * Mathf.PI), now * 40 + k * 20);
                    }
                    for (int k = 0; k < 6; k++)
                    {
                        float x = a.xMin + (((k * .17f + now * .015f) % 1.2f) - .1f) * a.width;
                        Blob(b, new Vector2(x, a.yMin + .8f + 1.2f * H(Seed, k)), new Vector2(3.2f, 1.2f), 0, .6f * on, now * .05f + k, Seed + k);
                    }
                    return;
                }
                // trait: a total eclipse that sweeps over the arena
                float sweep = Mathf.Clamp01(t / 1.5f), dim = Mathf.Sin(Mathf.Clamp01(t / 2.2f) * Mathf.PI);
                Vector2 mm = new Vector2(Mathf.Lerp(a.xMin - 1.5f, a.xMax + 1.5f, sweep), a.center.y + a.height * .2f);
                b.Quad(FxShape.Shade, a.center, new Vector2(a.width * .75f, a.height * .75f), 0, A(new Color(.06f, .02f, .18f), .62f * dim), 0, Seed);
                for (int k = 0; k < 5; k++) { Vector2 foot = new Vector2(mm.x + (k - 2) * 2.6f, a.yMin + 2.5f); b.Beam(mm + Vector2.down * 3.2f, foot, .9f, A(Violet, .35f * dim), t, Seed + k, 0); b.Floor(FxShape.Glow, foot, 1.8f, A(Magenta, .3f * dim), 0, Seed); }
                Moon(b, mm, 2.3f, dim > .02f ? 1 : 0, now, Seed, dim);
                float tot = Mathf.Max(0, 1 - Mathf.Abs(sweep - .5f) * 7);
                b.Quad(Ring, mm, 3.1f, 0, A(White, 1.2f * tot), t, Seed + 7, .7f);
                for (int k = 0; k < 24; k++)
                {
                    float u = (t * .8f + H(Seed, k)) % 1;
                    Vector2 p = new Vector2(mm.x + (H(Seed, k + 20) - .5f) * 5, mm.y - u * a.height * .9f);
                    Glint(b, p, .2f, k % 3 == 0 ? Magenta : Violet, .9f * dim * Mathf.Sin(u * Mathf.PI), now * 60 + k * 20);
                }
            }
        }

        /// <summary>Step 2 trait: the enemy is marked. A crescent moon drops onto its head and hangs there, ink splats out under its
        /// feet and drips from the crescent's horns, two thin violet arcs swing around it like a clock hand, then it all sinks away.</summary>
        private sealed class Mark : FxEffect
        {
            protected override float Life => 1.5f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Radius, drop = EaseOut3(Rise(t, 0, .22f)), off = Fade(t, 1.0f, 1.5f);
                Vector2 m = at + Vector2.up * (1.6f + 2.4f * (1 - drop));
                b.Floor(FxShape.Shade, at, r * 1.2f, A(Ink, .8f * off), 0, Seed);
                for (int k = 0; k < 6; k++)
                {
                    Vector2 d = Dir(Seed, k, 6, .62f); float sp = EaseOut(Rise(t, .18f, .4f));
                    Blob(b, at + d * r * (.3f + .7f * sp), new Vector2(.42f, .26f) * (.6f + .4f * sp), Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg, .95f * off, t, Seed + k);
                }
                b.Quad(FxShape.Glow, m, 1.6f, A(Violet, .45f * off), 0, Seed);
                b.Quad(Ring, m, new Vector2(.95f, .95f), -20 + Mathf.Sin(t * 6) * 8, A(Violet, 1.3f * off), 0, Seed, 6);
                b.Quad(Ring, m, new Vector2(.8f, .8f), -20 + Mathf.Sin(t * 6) * 8, A(White, .7f * off), 0, Seed + 1, 6);
                for (int s = 0; s < 2; s++)
                {
                    float u = ((t - .25f) * 1.6f + s * .5f) % 1; if (t < .25f) continue;
                    Vector2 horn = m + new Vector2(s == 0 ? -.55f : .45f, -.62f);
                    b.Quad((FxShape)32, horn + Vector2.down * u * 1.4f, .2f, 0, A(Magenta, 1.1f * off * (1 - u)), 0, Seed + s);
                }
                for (int k = 0; k < 2; k++)
                {
                    float ang = now * 4 * (k == 0 ? 1 : -1) + k * 3.1f; Vector2 tip = at + Vector2.up * .6f + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang) * .62f) * r * 1.05f;
                    b.Link(at + Vector2.up * .6f + (tip - at - Vector2.up * .6f) * .55f, tip, .07f, A(Violet, .9f * off * drop), Seed + k, .5f);
                    Glint(b, tip, .3f, Magenta, off * drop, now * 90 + k * 40);
                }
            }
        }
    }
}
