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
        // owner 29/10/2026: small glints and sparks were drawn far too big: small ones shrink, big set-piece flashes keep their size
        private static void Glint(FxBatch b, Vector2 at, float size, Color tint, float alpha, float turn = 0)
            => FxKit.Glint(b, at, size <= .08f ? size : size < .5f ? .08f + (size - .08f) * .45f : size - .231f, tint, alpha, turn);
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

        private static float Ang(Vector2 d) => Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
        private static Vector2 Vec(float deg) { float r = deg * Mathf.Deg2Rad; return new Vector2(Mathf.Cos(r), Mathf.Sin(r)); }
        private static void Blob(FxBatch b, Vector2 p, Vector2 size, float deg, float alpha, float prog, int seed) => b.Quad(FxShape.Smoke, p, size, deg, A(Ink, alpha), prog, seed, 0);
        private static void Blob(FxBatch b, Vector2 p, float size, float deg, float alpha, float prog, int seed) => Blob(b, p, new Vector2(size, size), deg, alpha, prog, seed);

        /// <summary>Trait: the reaping. A huge shadow scythe forms above the target, swings down through it in one stroke leaving a
        /// violet smear of light, the stroke opens as one long rift spurting ink, and the target's pale soul is torn out of the cut and
        /// whirled away up along the blade's path.</summary>
        private sealed class Execute : FxEffect
        {
            protected override float Life => 1.7f;
            private static Vector2 Tip(Vector2 pivot, float th, float L) => pivot + new Vector2(Mathf.Cos(th * Mathf.Deg2Rad), Mathf.Sin(th * Mathf.Deg2Rad) * .8f) * L;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Mathf.Max(1.3f, Radius * 1.2f), off = Fade(t, 1.1f, 1.7f);
                Vector2 c = at + Vector2.up * .45f, pivot = c + new Vector2(-r * .2f, r * 1.25f);
                float form = EaseOut(Rise(t, 0, .14f)), swing = EaseOut3(Rise(t, .12f, .36f)), gone = Rise(t, .5f, .9f), L = r * 1.75f;
                float th = Mathf.Lerp(150, -75, swing);
                b.Floor(FxShape.Shade, at, r * 1.3f, A(Ink, .8f * Rise(t, 0, .2f) * off), 0, Seed);
                // the smear left by the stroke: a fan of fading violet arcs behind the blade's edge, white at the leading edge
                if (swing > 0)
                    for (int k = 0; k < 14; k++)
                    {
                        float a0 = Mathf.Lerp(150, th, k / 14f), a1 = Mathf.Lerp(150, th, (k + 1) / 14f), f = (k + 1) / 14f, fa = f * f * (1 - gone);
                        b.Link(Tip(pivot, a0, L * .96f), Tip(pivot, a1, L * .96f), .1f + .3f * f, A(Violet, .55f * fa), Seed + k, .4f);
                        b.Link(Tip(pivot, a0, L * .75f), Tip(pivot, a1, L * .75f), .06f + .22f * f, A(Magenta, .4f * fa), Seed + k + 20, .5f);
                        if (k > 9) b.Link(Tip(pivot, a0, L), Tip(pivot, a1, L), .05f, A(White, .8f * fa), Seed + k + 40, .9f);
                    }
                // the scythe: a dark shaft from the pivot and a long crescent blade on its end, violet edge, fading after the stroke
                float sa = form * (1 - gone);
                if (sa > .01f)
                {
                    Vector2 end = Tip(pivot, th, L * .35f), back = Tip(pivot, th + 180, L * .45f), blade = Tip(pivot, th, L * .72f);
                    b.Link(back, end, .11f, A(Violet, .5f * sa), Seed, .6f);
                    b.Quad(FxShape.Shade, (back + end) * .5f, new Vector2((back - end).magnitude * .5f + .1f, .12f), Ang(end - back), A(Ink, sa), 0, Seed);
                    float bd = th - 75;
                    b.Quad(Ring, blade, new Vector2(r * 1.05f, r * .55f), bd, A(Ink, sa), 0, Seed, 6);
                    b.Quad(Ring, blade, new Vector2(r, r * .5f), bd, A(Violet, 1.1f * sa), 0, Seed + 1, 6);
                    b.Quad(Ring, blade + Vec(bd + 180) * r * .03f, new Vector2(r * .96f, r * .47f), bd, A(White, .5f * sa), 0, Seed + 2, 6);
                    b.Quad(FxShape.Glow, blade, r * .8f, A(Magenta, .3f * sa), 0, Seed);
                }
                // the cut: one long rift along the stroke, ink spurting out of it, a thin white line as it snaps shut
                float open = EaseOut(Rise(t, .26f, .4f)) * (1 - Rise(t, .85f, 1.05f));
                float cutDeg = -38;
                if (open > 0)
                {
                    b.Quad(FxShape.Glow, c, new Vector2(r * 1.5f, r * .35f), cutDeg, A(Magenta, .5f * open), 0, Seed);
                    b.Quad(Rift, c, new Vector2(r * 1.7f, r * .7f), cutDeg, A(Violet, 1.3f * off), open, Seed, 1);
                    b.Quad(Rift, c, new Vector2(r * 1.5f, r * .45f), cutDeg, A(White, .8f * open), open, Seed + 3, 0);
                }
                float snap = Rise(t, .85f, .9f) * Fade(t, .9f, 1.1f);
                b.Link(c - Vec(cutDeg) * r * 1.4f, c + Vec(cutDeg) * r * 1.4f, .1f, A(White, .9f * snap), Seed, .7f);
                for (int k = 0; k < 14; k++)
                {
                    float u = (t - .3f) / (.5f + .3f * H(Seed, k + 90)); if (u <= 0 || u >= 1) continue;
                    Vector2 along = c + Vec(cutDeg) * (H(Seed, k + 91) - .5f) * r * 2f, d = Dir(Seed + 8, k, 14, .7f);
                    Blob(b, along + d * r * .9f * EaseOut(u) + Vector2.up * (1.4f * u - 2.4f * u * u), .14f + .1f * H(Seed, k), 0, .95f * (1 - u * u), u, Seed + k + 90);
                }
                // the soul: a pale wisp torn out of the cut and whirled up and away along the stroke
                float su = Rise(t, .4f, 1.5f);
                if (su > 0 && su < 1)
                {
                    for (int k = 0; k < 6; k++)
                    {
                        float v = Mathf.Max(0, su - k * .04f); Vector2 p = Vector2.Lerp(c, Tip(pivot, 160, L * 1.1f), EaseOut(v)) + new Vector2(Mathf.Sin(v * 9 + k) * .25f, 0);
                        b.Quad(FxShape.Glow, p, .55f * (1 - k / 7f), A(k == 0 ? White : new Color(.8f, .75f, 1f), .7f * (1 - su) * (1 - k / 7f)), 0, Seed);
                        if (k == 0) b.Quad(FxShape.Smoke, p, .4f, 0, A(new Color(.75f, .7f, .95f), .6f * (1 - su)), su, Seed + 70);
                    }
                    for (int k = 0; k < 5; k++) Glint(b, Vector2.Lerp(c, Tip(pivot, 160, L * 1.1f), EaseOut(su)) + Dir(Seed, k, 5, 1) * .4f, .2f, Violet, 1 - su, now * 80 + k * 50);
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
                    float du = (now * 1.3f + H(Seed, k + 70)) % 1;
                    Blob(b, root + new Vector2(sway * g + w * .6f, h * g * (.7f - .7f * du * du)), new Vector2(.09f, .14f), 0, .95f * g * (1 - du), 0, Seed + k + 70);
                    b.Quad(FxShape.Smoke, root + new Vector2(0, .1f), new Vector2(.45f, .25f) * (1 + g), 0, A(Indigo * 1.5f, .6f * g), t * .5f, Seed + k + 80);
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
            protected override float Life => Moment.Duration + 1.8f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                // the pit holds its full size for the whole zone, then swallows itself: it shrinks to a point over a second, its rim
                // flaring, and leaves a stain that fades last
                Vector2 at = At; float dur = Moment.Duration, shut = EaseOut(Rise(t, dur + .3f, dur + 1.4f)), r = Radius * (1 - .85f * shut);
                float on = EaseOut(Rise(t, 0, .3f)) * Fade(t, dur + 1.2f, dur + 1.5f), pulse = 1 + .03f * Mathf.Sin(now * 9) + .12f * Mathf.Sin(shut * Mathf.PI);
                float stain = Rise(t, dur, dur + .3f) * Fade(t, dur + 1.2f, dur + 1.8f);
                b.Floor(FxShape.Shade, At, Radius * 1.1f, A(Ink, .6f * stain), 0, Seed + 77);
                b.Floor(FxShape.Glow, At, Radius * 1.2f * (1 - shut * .5f), A(Magenta, .5f * Mathf.Sin(shut * Mathf.PI)), 0, Seed);
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
                for (int k = 0; k < 5; k++)
                {
                    float bc = now * .9f + H(Seed, k + 100) * 5; int bn = Mathf.FloorToInt(bc); float u = bc - bn;
                    Vector2 bp = at + new Vector2((H(Seed + bn, k) - .5f) * r * 1.1f, (H(Seed + bn, k + 1) - .5f) * r * .5f);
                    if (u < .75f) b.Quad((FxShape)30, bp + Vector2.up * u * .2f, .06f + .1f * EaseOut(u / .75f), 0, A(Violet, .9f * on), u, Seed + k, 0);
                    else { float s = (u - .75f) / .25f; for (int j = 0; j < 4; j++) Glint(b, bp + Dir(Seed + k, j, 4, .8f) * .45f * s + Vector2.up * .3f, .14f, Magenta, on * (1 - s), j * 40); }
                    float hu = (now * .35f + k * .2f) % 1;
                    b.Quad(FxShape.Smoke, at + new Vector2((H(Seed, k + 120) - .5f) * r, .3f + hu * 1.8f), .35f + .5f * hu, 0, A(Indigo * 1.4f, .45f * on * Mathf.Sin(hu * Mathf.PI)), hu, Seed + k + 120);
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
                    b.Quad(FxShape.Smoke, root + Vector2.up * .15f, new Vector2(.5f, .3f) * (.6f + g), 0, A(Indigo * 1.5f, .65f * g), t, Seed + k + 90);
                }
                for (int k = 0; k < 20; k++)
                {
                    float life = .7f + .5f * H(Seed, k + 40), u = (t - .2f) / life; if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + Dir(Seed + 5, k, 20, .7f) * r * 1.1f * EaseOut(u) + Vector2.up * (3.8f * H(Seed, k + 60) * u - 5.2f * u * u + .4f);
                    if (k % 2 == 0) Blob(b, p, .2f + .12f * H(Seed, k), 0, .95f * (1 - u), u, Seed + k);
                    else Glint(b, p, .26f, Violet, 1 - u, k * 60 + now * 200);
                }
                // the ink rain lands: flat splats with a violet glint
                for (int k = 0; k < 10; k++)
                {
                    float st = .75f + .5f * H(Seed, k + 140), u = (t - st) / .5f; if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + Dir(Seed + 11, k, 10, .62f) * r * (.4f + .8f * H(Seed, k + 141));
                    Blob(b, p, new Vector2(.2f + .3f * EaseOut(u * 3), .12f + .12f * EaseOut(u * 3)), 0, .9f * (1 - u), u, Seed + k + 140);
                    Glint(b, p + Vector2.up * .1f, .2f * (1 - u), Magenta, 1 - u, k * 33);
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
                b.Quad(FxShape.Glow, m, R * 2.6f, A(Indigo, .5f * alpha * corona), 0, seed);
                b.Quad(FxShape.Rays, m, R * 2.3f, A(Violet, .45f * alpha * corona), now * .2f, seed, 5);
                b.Quad(Ring, m, R * 2.2f, 0, A(Violet, 1.2f * alpha * corona), now * .3f, seed, 3);
                b.Quad(Ring, m, R * 1.9f, 0, A(Magenta, .8f * alpha * corona), -now * .5f, seed + 2, 4);
                for (int k = 0; k < 7; k++) Blob(b, m + Dir(seed, k, 7, 1) * R * .42f, R * .9f, 0, alpha, k * .3f, seed + k);
                Blob(b, m, R * 1.08f, 0, alpha, .2f, seed + 9);
                b.Quad(Ring, m, R * 1.12f, 215, A(new Color(.9f, .8f, 1f), .6f * alpha), 0, seed, 6);
                Glint(b, m + new Vector2(-.7f, -.62f) * R * 1.02f, R * .5f * (.8f + .2f * Mathf.Sin(now * 5)), White, alpha, now * 30);
                for (int k = 0; k < 3; k++) { float u = (now * .7f + k / 3f) % 1; Blob(b, m + new Vector2((k - 1) * R * .45f, -R * (.85f + 1.2f * u * u)), new Vector2(.1f, .16f) * (1 - u * .5f), 0, alpha * (1 - u), 0, seed + k + 30); }
            }

            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Rect a = Arena;
                if (Moment.Kind == BondMomentKind.Weather)
                {
                    float on = Rise(t, 0, 2.5f) * Fade(now, End - 1.5f, End);
                    Vector2 m = new Vector2(a.center.x - a.width * .18f, a.yMax - 3f); // high in the sky, off the crowd
                    b.Quad(FxShape.Shade, a.center, new Vector2(a.width * .75f, a.height * .75f), 0, A(new Color(.07f, .02f, .2f), .3f * on), 0, Seed);
                    b.Quad(FxShape.Shade, new Vector2(a.center.x, a.yMax), new Vector2(a.width * .5f, a.height * .22f), 0, A(Ink, .35f * on), 0, Seed + 1);
                    Moon(b, m, .9f, on, now, Seed, .85f + .15f * Mathf.Sin(now * 1.3f));
                    for (int k = 0; k < 34; k++)
                    {
                        float u = (now * (.1f + .06f * H(Seed, k)) + H(Seed, k + 10)) % 1;
                        Vector2 p = m + new Vector2((H(Seed, k + 20) - .5f) * 7 * u + Mathf.Sin(now + k) * .3f, -u * (a.height * .8f));
                        Glint(b, p, .18f, k % 3 == 0 ? Magenta : Violet, .85f * on * Mathf.Sin(u * Mathf.PI), now * 40 + k * 20);
                    }
                    for (int k = 0; k < 6; k++)
                    {
                        float x = a.xMin + (((k * .17f + now * .015f) % 1.2f) - .1f) * a.width;
                        Blob(b, new Vector2(x, a.yMin + .8f + 1.2f * H(Seed, k)), new Vector2(2.4f, .9f), 0, .35f * on, now * .05f + k, Seed + k);
                    }
                    return;
                }
                // trait: a total eclipse that sweeps over the arena
                float sweep = Mathf.Clamp01(t / 1.5f), dim = Mathf.Sin(Mathf.Clamp01(t / 2.2f) * Mathf.PI);
                Vector2 mm = new Vector2(Mathf.Lerp(a.xMin - 1.5f, a.xMax + 1.5f, sweep), a.yMax - 3f);
                b.Quad(FxShape.Shade, a.center, new Vector2(a.width * .75f, a.height * .75f), 0, A(new Color(.06f, .02f, .18f), .4f * dim), 0, Seed);
                for (int k = 0; k < 5; k++) { Vector2 foot = new Vector2(mm.x + (k - 2) * 2.6f, a.yMin + 2.5f); b.Beam(mm + Vector2.down * 2.2f, foot, .6f, A(Violet, .32f * dim), t, Seed + k, 0); b.Floor(FxShape.Glow, foot, 1.4f, A(Magenta, .3f * dim), 0, Seed); }
                Moon(b, mm, 1.1f, dim > .02f ? 1 : 0, now, Seed, dim);
                float tot = Mathf.Max(0, 1 - Mathf.Abs(sweep - .5f) * 7);
                b.Quad(Ring, mm, 2.3f, 0, A(White, 1.1f * tot), t, Seed + 7, .7f);
                for (int k = 0; k < 24; k++)
                {
                    float u = (t * .8f + H(Seed, k)) % 1;
                    Vector2 p = new Vector2(mm.x + (H(Seed, k + 20) - .5f) * 5, mm.y - u * a.height * .9f);
                    Glint(b, p, .2f, k % 3 == 0 ? Magenta : Violet, .9f * dim * Mathf.Sin(u * Mathf.PI), now * 60 + k * 20);
                }
            }
        }

        /// <summary>Step 2/4 trait: the enemy is marked. A great violet eye of dusk opens over it, its iris rolling to stare down; black
        /// ink tendrils drop out of it, writhe and grip the target while ink splats out under its feet; then the eye blinks shut and
        /// sinks into the target, burning a crescent brand on the floor.</summary>
        private sealed class Mark : FxEffect
        {
            protected override float Life => 1.6f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Mathf.Max(1.3f, Radius * 1.2f), off = Fade(t, 1.1f, 1.6f);
                float open = EaseOut(Rise(t, 0, .25f)) * (1 - EaseOut(Rise(t, .8f, 1f))), sink = EaseOut(Rise(t, .9f, 1.2f));
                Vector2 eye = Vector2.Lerp(at + Vector2.up * 2f, at + Vector2.up * .5f, sink);
                float es = r * (1 - .6f * sink);
                b.Floor(FxShape.Shade, at, r * 1.3f, A(Ink, .85f * off), 0, Seed);
                for (int k = 0; k < 7; k++)
                {
                    Vector2 d = Dir(Seed, k, 7, .62f); float sp = EaseOut(Rise(t, .3f, .5f));
                    Blob(b, at + d * r * (.3f + .8f * sp), new Vector2(.5f, .3f) * (.5f + .5f * sp), Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg, .95f * off, t, Seed + k);
                }
                // tendrils: wavy ink strands from the eye down to the target, writhing
                float reach = EaseOut(Rise(t, .2f, .45f)) * (1 - sink);
                for (int k = 0; k < 5; k++)
                {
                    Vector2 from = eye + new Vector2((k - 2) * es * .32f, -es * .2f), to = at + new Vector2((k - 2) * .28f, .25f + .25f * (k % 2)); Vector2 prev = from;
                    for (int s = 1; s <= 8; s++)
                    {
                        float f = s / 8f; if (f > reach) break;
                        Vector2 p = Vector2.Lerp(from, to, f) + new Vector2(Mathf.Sin(f * 7 + now * 8 + k * 1.3f) * .18f * Mathf.Sin(f * 3.14f), 0);
                        b.Link(prev, p, .07f * (1 - f * .5f), A(Violet, .5f * off), Seed + k * 9 + s, .5f);
                        b.Quad(FxShape.Shade, (prev + p) * .5f, new Vector2((p - prev).magnitude * .5f + .06f, .07f * (1 - f * .5f) + .02f), Ang(p - prev), A(Ink, off), 0, Seed + s);
                        prev = p;
                    }
                }
                // the eye: an ink-lidded almond of violet light with a magenta iris and a black slit pupil that looks round
                if (open > .01f)
                {
                    Vector2 look = new Vector2(Mathf.Sin(now * 3.1f) * .25f, -.12f - .08f * Mathf.Sin(now * 2.3f)) * es * open;
                    b.Quad(FxShape.Glow, eye, es * 1.5f, A(Violet, .55f * open), 0, Seed);
                    b.Quad(FxShape.Shade, eye, new Vector2(es * 1.25f, es * .75f * open + .05f), 0, A(Ink, 1), 0, Seed);
                    b.Quad(Rift, eye, new Vector2(es * 1.2f, es * 2.1f), 0, A(Violet, 1.4f * off), open, Seed, 2);
                    b.Quad(FxShape.Glow, eye + look, es * .55f * open, A(Magenta, .95f), 0, Seed);
                    b.Quad((FxShape)30, eye + look, es * .36f * open, A(Magenta, 1.1f), 0, Seed + 1, 0);
                    b.Quad(FxShape.Shade, eye + look, new Vector2(es * .09f, es * .3f * open), 0, A(Ink, 1), 0, Seed + 2);
                    Glint(b, eye + look + new Vector2(-.08f, .08f) * es, es * .3f, White, open, now * 40);
                    for (int k = 0; k < 6; k++) { float u = (now * .8f + k / 6f) % 1; Glint(b, eye + Dir(Seed, k, 6, .45f) * es * (.8f + .6f * u), .18f, k % 2 == 0 ? Violet : Magenta, (1 - u) * open, k * 40 + now * 60); }
                }
                // the brand left on the floor after the eye sinks in
                float brand = Rise(t, 1.0f, 1.15f) * off;
                b.Floor(Ring, at, r * .9f, A(Magenta, 1.1f * brand), 0, Seed + 5, 6);
                b.Floor(FxShape.Glow, at, r * 1.1f, A(Violet, .5f * brand), 0, Seed);
                for (int k = 0; k < 3; k++) { float u = (t * 1.2f + k / 3f) % 1; b.Quad(FxShape.Smoke, at + new Vector2((k - 1) * .4f, .2f + u * 1.2f), .2f + .35f * u, 0, A(Indigo * 1.6f, .55f * brand * Mathf.Sin(u * Mathf.PI)), u, Seed + k + 60); }
            }
        }
    }
}
