using System;
using System.Collections.Generic;
using UnityEngine;
using static ArcaneBrigade.FxKit;
using static ArcaneBrigade.AKit;

namespace ArcaneBrigade
{
    /// <summary>Garden bond: vines, leaves, roots and blossoms.</summary>
    public static class GardenFx
    {
        // owner 29/10/2026: small glints and sparks were drawn far too big: small ones shrink, big set-piece flashes keep their size
        private static void Glint(FxBatch b, Vector2 at, float size, Color tint, float alpha, float turn = 0)
            => FxKit.Glint(b, at, size <= .08f ? size : size < .5f ? .08f + (size - .08f) * .45f : size - .231f, tint, alpha, turn);
        public static void Register(Dictionary<string, Func<FxEffect>> map)
        {
            map["garden-vine"] = () => new Vine();
            map["root-hit"] = () => new RootHit();
            map["treant-slam"] = () => new TreantSlam();
            map["root-eruption"] = () => new RootEruption();
        }
        private static readonly Color LeafG = new Color(.4f, .95f, .3f), DeepG = new Color(.15f, .6f, .2f), Bark = new Color(.8f, .5f, .22f), Pink = new Color(1f, .55f, .78f), Pollen = new Color(1f, .9f, .45f), Mud = new Color(.12f, .07f, .03f);

        /// <summary>A root or vine standing on <paramref name="baseP"/>, tilted by degrees (0 = up), grown to <paramref name="grow"/>.</summary>
        private static void Stalk(FxBatch b, Vector2 baseP, float tilt, float h, float w, Color c, float a, float grow, int seed, float curl = 0)
        {
            Vector2 c0 = baseP + Vec(90 + tilt) * h;
            b.Quad(FxShape.Shade, c0, new Vector2(w * 1.2f, h * grow), tilt, A(Mud, .55f * a), 0, seed);
            b.Quad(Root, c0, new Vector2(w, h), tilt, A(c, a), grow, seed, curl);
        }
        /// <summary>A wood splinter or pebble: an opaque sliver spinning on a gravity arc.</summary>
        private static void Splinter(FxBatch b, Vector2 from, Vector2 vel, float t, float life, float s, Color c, int seed)
        {
            float u = t / life; if (u <= 0 || u >= 1) return;
            Vector2 p = from + vel * t + Vector2.down * 7 * t * t;
            Solid(b, p, new Vector2(s, s * .32f), t * 720 * (seed % 2 == 0 ? 1 : -1) + seed * 37, c, 1 - u * u);
        }
        private static void Clod(FxBatch b, Vector2 from, Vector2 vel, float t, float life, float s, int seed)
        {
            float u = t / life; if (u <= 0 || u >= 1) return;
            Vector2 p = from + vel * t + Vector2.down * 6 * t * t;
            b.Quad(FxShape.Smoke, p, s, A(new Color(.22f, .13f, .06f), 1 - u * u), u, seed);
        }

        /// <summary>Step 2: vines wrap the hit enemy: three curls coil up out of the floor around it, two thin vines twine upward sprouting leaves, a pink blossom opens on top, pollen floats.</summary>
        private sealed class Vine : FxEffect
        {
            protected override float Life => 1.6f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                float r = Mathf.Clamp(Radius * 2.6f, 1.6f, 2.4f), g = EaseOut(Rise(t, 0, .45f)), o = Fade(t, 1.1f, 1.6f); Vector2 at = At;
                b.Floor(FxShape.Glow, at, r * 1.3f, A(DeepG, .55f * o), 0, Seed);
                for (int k = 0; k < 3; k++) Grit(b, at + Dir(Seed, k, 3, .5f) * r * .45f, r * (.4f + .3f * g), new Color(.45f, .36f, .22f), .45f * Fade(t, .2f, 1f), t + k, Seed + 50 + k);
                float sq = Mathf.Sin(Rise(t, .5f, .75f) * Mathf.PI);
                for (int k = 0; k < 10; k++)
                {
                    Vector2 dd = Dir(Seed + 5, k, 10, .7f);
                    Fleck(b, at + dd * r * .35f + Vector2.up * r * .8f, new Vector2(dd.x * 2.2f, 1.2f + H(Seed, k + 30)), 5, t - .52f, .55f, .06f, k % 2 == 0 ? LeafG : Pollen, Seed + k + 30);
                }
                for (int k = 0; k < 12; k++)
                {
                    float u = Rise(t, .3f + k * .05f, 1.3f + k * .025f); if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + new Vector2((H(Seed, k + 80) - .5f) * r * 1.4f + Mathf.Sin(u * 7 + k) * .35f, r * 1.7f - u * r * 1.6f);
                    Lf(b, p, .15f, Mathf.Sin(u * 9 + k) * 60 + 90, k % 2 == 0 ? LeafG : DeepG, .9f * Fade(u, .7f, 1), Seed + k + 80);
                }
                for (int k = 0; k < 3; k++)
                    b.Floor(Curl, at + Dir(Seed, k, 3, .5f) * r * .45f, r * .75f, A(LeafG, .95f * o), g, Seed + k, k % 2 == 0 ? 1 : -1);
                for (int k = 0; k < 2; k++)
                {
                    float side = k == 0 ? -1 : 1;
                    Stalk(b, at + new Vector2(side * r * (.45f - .06f * sq), -.1f), side * -14 * (1 + .5f * sq), r * .85f, r * .28f * (1 + .22f * sq), LeafG, o, g, Seed + k, .6f);
                    for (int j = 0; j < 4; j++)
                    {
                        float f = .25f + j * .2f, sg = Overshoot(Rise(t, .15f + j * .07f, .35f + j * .07f));
                        Vector2 p = at + new Vector2(side * r * (.45f - f * .25f) + Mathf.Sin(f * 8 + k) * .15f, f * r * 1.6f);
                        Lf(b, p + Vec(j % 2 == 0 ? 20 : 160) * .25f * sg, .42f * sg, j % 2 == 0 ? 25 : 155, j % 2 == 0 ? LeafG : DeepG, o, Seed + j * 3 + k);
                    }
                }
                float bloom = Overshoot(Rise(t, .35f, .6f));
                Vector2 top = at + Vector2.up * r * 1.55f;
                b.Quad(FxShape.Glow, top, r * .9f * bloom, A(Pink, .45f * o), 0, Seed);
                b.Quad(Petal, top, r * .55f * bloom, t * 30, A(Pink, o), 0, Seed, 0);
                b.Quad(Petal, top, r * .28f * bloom, -t * 50 + 36, A(Pollen, o), 0, Seed + 1, 0);
                for (int k = 0; k < 10; k++)
                {
                    float u = (t * .9f + H(Seed, k)) % 1; Vector2 p = at + Dir(Seed, k, 10, .6f) * r * .8f + Vector2.up * (.3f + u * 2f) + Vector2.right * Mathf.Sin(t * 4 + k) * .2f;
                    Glint(b, p, .2f, Pollen, .8f * o * (1 - u), k * 30 + t * 90);
                }
            }
        }

        /// <summary>Roots grab an enemy that stood still: six thorny brown roots burst from the floor in a cage and snap inward, clods of earth fly, dust puffs, the ground cracks green.</summary>
        private sealed class RootHit : FxEffect
        {
            protected override float Life => 1.4f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                float r = Mathf.Clamp(Radius, 1.1f, 2.2f), g = Overshoot(Rise(t, 0, .22f)), o = Fade(t, .95f, 1.4f); Vector2 at = At;
                b.Floor(FxShape.Shade, at, r * 1.3f, A(Mud, .6f * o), 0, Seed);
                b.Floor(FxShape.Glow, at, r * 1.2f, A(Bark, .35f * o), 0, Seed);
                for (int k = 0; k < 6; k++)
                {
                    Vector2 dd = Dir(Seed, k, 6, .55f); float snap = EaseOut(Rise(t, .2f, .35f));
                    float tilt = -Mathf.Sign(dd.x) * (10 + 30 * snap) * Mathf.Abs(dd.x);
                    Stalk(b, at + dd * r * .75f, tilt, r * (.75f + .3f * H(Seed, k)), r * .22f, Bark, o, g, Seed + k, .3f);
                    if (k % 2 == 0) Lf(b, at + dd * r * .75f + Vec(90 + tilt) * r * 1.2f * g, .3f * g, tilt + 60, LeafG, o, Seed + k);
                }
                for (int k = 0; k < 10; k++)
                {
                    Vector2 dd = Dir(Seed + 3, k, 10, .6f);
                    Clod(b, at + dd * r * .5f, new Vector2(dd.x * 2.4f, 3 + 2 * H(Seed, k)), t, .8f, .16f + .1f * H(Seed, k + 2), Seed + k);
                }
                b.Floor(FxShape.Smoke, at, r * (.6f + .8f * EaseOut(Rise(t, 0, .6f))), A(new Color(.4f, .3f, .18f), .4f * Fade(t, 0, .9f)), t, Seed);
                Grit(b, at, r * (.8f + .5f * EaseOut(Rise(t, 0, .8f))), new Color(.48f, .38f, .24f), .45f * Fade(t, .3f, 1.3f), t, Seed + 4);
                for (int k = 0; k < 8; k++)
                {
                    Vector2 dd = Dir(Seed + 6, k, 8, .6f);
                    Splinter(b, at + dd * r * .6f + Vector2.up * .3f, new Vector2(dd.x * 2.6f, 2.8f + 1.5f * H(Seed, k + 9)), t - .2f, .65f, .14f + .06f * H(Seed, k + 10), new Color(.62f, .4f, .18f), Seed + k);
                }
                for (int k = 0; k < 6; k++) Glint(b, at + Pt(Seed, k + 20, r, r * .6f) + Vector2.up * (.5f + .4f * t), .16f, Pollen, .8f * Rise(t, .25f, .35f) * Fade(t, .6f, 1.2f), k * 50 + t * 150);
            }
        }

        /// <summary>Step 6. The giant treant's wooden fist plunges from above onto the crowd: the ground cracks green, eight roots tear outward along the floor, leaves and blossoms are flung up, dust rolls out.</summary>
        private sealed class TreantSlam : FxEffect
        {
            protected override float Life => 2.1f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                float r = Mathf.Max(2f, Radius); Vector2 at = At;
                float drop = t < 0 ? pre * pre * pre : 1, lift = Rise(t, .35f, .9f);
                Vector2 fist = at + Vector2.up * (.6f + 8.5f * (1 - drop) + 7f * lift * lift);
                if (t < 0) for (int k = 0; k < 6; k++) b.Link(fist + new Vector2((k - 2.5f) * .45f, 1.6f + .3f * (k % 2)), fist + new Vector2((k - 2.5f) * .45f, 4f + .8f * H(Seed, k)), .05f, A(Pollen, .45f * pre), Seed + k, 1);
                if (t < 0)
                {
                    float g = EaseOut(pre);
                    b.Floor(FxShape.Shade, at, r * 1.4f * (.4f + .6f * g), A(Mud, .7f * g), 0, Seed);
                    for (int k = 0; k < 18; k++)
                    {
                        float u = (pre * 1.3f + H(Seed, k)) % 1; Vector2 p = at + Pt(Seed, k, r * 1.2f, r * .7f) + Vector2.up * (3 - u * 3);
                        Lf(b, p, .18f, now * 160 + k * 40, k % 2 == 0 ? LeafG : DeepG, .9f * g * (1 - u), Seed + k);
                    }
                    Fist(b, fist, r, 1, 0, Seed, 0);
                    return;
                }
                float o = Fade(t, 1.2f, 2.1f);
                b.Quad(FxShape.Glow, at - Vector2.up * .3f, new Vector2(r * 1.5f, r * .7f), 0, A(LeafG, .22f * Fade(t, 0, .3f)), 0, Seed);
                for (int k = 0; k < 10; k++)
                {
                    Vector2 dd = Dir(Seed + 12, k, 10, .62f); float u = EaseOut(Rise(t, 0, .9f));
                    Grit(b, at + dd * r * (.35f + .85f * u), r * (.35f + .25f * u), new Color(.5f, .4f, .26f), .55f * Fade(t, .4f, 1.6f), t + k, Seed + k + 60);
                }
                for (int k = 0; k < 14; k++)
                {
                    Vector2 dd = Dir(Seed + 14, k, 14, .7f);
                    Splinter(b, at + Vector2.up * .3f + dd * .6f, new Vector2(dd.x * (3 + 2 * H(Seed, k + 70)), 3 + 3 * H(Seed, k + 71)), t, .9f, .2f + .1f * H(Seed, k + 72), k % 3 == 0 ? new Color(.3f, .18f, .08f) : new Color(.66f, .43f, .2f), Seed + k + 70);
                }
                b.Floor(FxShape.Shade, at, r * 1.45f, A(Mud, .7f * o), 0, Seed);
                b.Quad(FxShape.Cracks, at, new Vector2(r * 1.4f, r * .9f), 0, A(LeafG, .9f * o * Rise(t, .28f, .6f)), 1, Seed, 2);

                float spread = EaseOut3(Rise(t, .02f, .35f));
                for (int k = 0; k < 8; k++)
                {
                    Vector2 dd = Dir(Seed + 2, k, 8, .62f); float L = r * .42f * (.9f + .3f * H(Seed, k)); Vector2 mid = at + dd * (r * .5f + L);
                    b.Quad(FxShape.Shade, mid, new Vector2(L * spread, r * .2f), Ang(dd), A(Mud, .5f * o), 0, Seed + k);
                    b.Quad(Root, mid, new Vector2(r * .2f, L), Ang(dd) - 90, A(Bark, o), spread, Seed + k, .4f);
                    if (spread > .9f) b.Quad(Petal, at + dd * r * 1.25f, .4f * Overshoot(Rise(t, .3f + k * .03f, .5f + k * .03f)), t * 40, A(k % 2 == 0 ? Pink : Pollen, o), 0, Seed + k, 0);
                }
                for (int k = 0; k < 34; k++)
                {
                    float life = .9f + .5f * H(Seed, k + 70), u = t / life; if (u >= 1) continue;
                    Vector2 dir = Dir(Seed, k + 40, 34, .5f), p = at + dir * r * (.55f + 1.1f * EaseOut(u)) + Vector2.up * (3.4f * H(Seed, k + 90) * u - 4.2f * u * u + .3f);
                    Lf(b, p, .18f, k * 50 + t * 400, k % 3 == 0 ? Pink : (k % 3 == 1 ? LeafG : DeepG), (1 - u * u) * Rise(u, 0, .1f), Seed + k);
                }
                for (int k = 0; k < 9; k++)
                {
                    Vector2 dd = Dir(Seed + 7, k, 9, .62f);
                    b.Quad(FxShape.Smoke, at + dd * r * (.4f + .9f * EaseOut(Rise(t, 0, .8f))), r * .45f, A(new Color(.42f, .32f, .2f), .55f * Fade(t, .1f, 1.2f)), t, Seed + k);
                }
                if (lift < 1) Fist(b, fist, r, 1 - lift, t, Seed, Mathf.Sin(Rise(t, 0, .22f) * Mathf.PI));
            }
            /// <summary>The fist, knuckles down: a bark forearm reaching out of the sky into a clenched wooden fist. Painted opaque on the
            /// alpha layer with a dark outline (each piece is drawn first a size bigger in near-black, then filled), four finger rolls
            /// with knuckle highlights and dark gaps, a thumb folded across the front, grain and moss on top, a leafy cuff at the wrist.</summary>
            private static readonly Vector2[] po = new Vector2[16], ps = new Vector2[16]; private static readonly float[] pd = new float[16]; private static readonly Color[] pc = new Color[16];
            private static void Fist(FxBatch b, Vector2 p, float r, float a, float t, int seed, float squash)
            {
                if (a <= .003f) return;
                float s = r * .58f; Color ol = new Color(.09f, .05f, .02f), wood = new Color(.36f, .21f, .09f), light = new Color(.58f, .38f, .18f), dark = new Color(.24f, .13f, .05f);
                Vector2 sq = new Vector2(1 + .12f * squash, 1 - .14f * squash);
                // pieces: centre offset (in s), half size (in s), degrees, fill
                int n = 0;
                po[n] = new Vector2(.05f, 4.2f); ps[n] = new Vector2(.6f, 2.6f); pd[n] = 3; pc[n] = dark; n++;
                po[n] = new Vector2(0, 1.9f); ps[n] = new Vector2(.64f, 1.4f); pd[n] = 0; pc[n] = wood; n++;
                for (int k = 0; k < 3; k++) { po[n] = new Vector2((k % 2 == 0 ? -1 : 1) * .5f, 1.6f + k * 1.1f); ps[n] = new Vector2(.16f, .3f); pd[n] = (k % 2 == 0 ? 1 : -1) * 20; pc[n] = wood; n++; }
                po[n] = new Vector2(0, .12f); ps[n] = new Vector2(.98f, .78f); pd[n] = 0; pc[n] = wood; n++;
                for (int k = 0; k < 4; k++) { po[n] = new Vector2((k - 1.5f) * .47f, -.5f + Mathf.Abs(k - 1.5f) * .07f); ps[n] = new Vector2(.27f, .34f); pd[n] = (k - 1.5f) * -4; pc[n] = wood; n++; }
                po[n] = new Vector2(-.22f, -.08f); ps[n] = new Vector2(.5f, .18f); pd[n] = -14; pc[n] = light; n++;
                for (int i = 0; i < n; i++) Solid(b, p + Vector2.Scale(po[i], sq) * s, (ps[i] * s + Vector2.one * .07f) * 1.06f, pd[i], ol, a);
                for (int i = 0; i < n; i++) Solid(b, p + Vector2.Scale(po[i], sq) * s, ps[i] * s, pd[i], pc[i], a);
                for (int k = 0; k < 4; k++)
                {
                    Vector2 kp = p + Vector2.Scale(new Vector2((k - 1.5f) * .47f - .06f, -.42f + Mathf.Abs(k - 1.5f) * .07f), sq) * s;
                    Solid(b, kp, new Vector2(.12f, .09f) * s, -20, light, a);
                    if (k < 3) Solid(b, p + Vector2.Scale(new Vector2((k - 1) * .47f, -.5f), sq) * s, new Vector2(.035f, .25f) * s, 0, ol, a);
                }
                Solid(b, p + Vector2.Scale(new Vector2(-.3f, -.02f), sq) * s, new Vector2(.28f, .06f) * s, -14, new Color(.8f, .58f, .3f), a);
                Solid(b, p + Vector2.Scale(new Vector2(.42f, .2f), sq) * s, new Vector2(.05f, .3f) * s, 10, dark, a);
                b.Quad(Petal, p + Vector2.up * s * 2.6f, new Vector2(s * .55f, s * 2.6f), 0, A(new Color(.85f, .6f, .3f), .3f * a), 0, seed, 10);
                b.Quad(Petal, p + Vector2.up * s * .15f, new Vector2(s * .9f, s * .7f), 90, A(new Color(.85f, .6f, .3f), .22f * a), 0, seed + 3, 10);
                for (int k = 0; k < 3; k++) Solid(b, p + new Vector2((k - 1) * .35f + .1f, 1.5f + k * .9f) * s, new Vector2(.16f, .08f) * s, 20 * k, new Color(.25f, .45f, .14f), a);
                for (int k = 0; k < 7; k++) Lf(b, p + new Vector2((k - 3) * s * .2f, s * (.95f + .08f * Mathf.Abs(k - 3))), .32f, 90 + (k - 3) * 28 + Mathf.Sin(t * 9 + k) * 10, k % 2 == 0 ? LeafG : DeepG, a, seed + k);
            }
        }

        /// <summary>Step 8. Three great roots erupt out of the floor, rear up, whip sideways and slam down across the crowd; earth sprays, green sap sparks, spores drift.</summary>
        private sealed class RootEruption : FxEffect
        {
            protected override float Life => 1.9f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                float r = Mathf.Max(1.4f, Radius); Vector2 at = At;
                if (t < 0)
                {
                    float g = EaseOut(pre), shake = Mathf.Sin(now * 40) * .05f;
                    b.Floor(FxShape.Shade, at, r * 1.3f * g, A(Mud, .6f * g), 0, Seed);
                    b.Quad(FxShape.Cracks, at + Vector2.right * shake, new Vector2(r * 1.2f, r * .75f) * g, 0, A(LeafG, .8f * g), 1, Seed, 2);
                    for (int k = 0; k < 6; k++) Clod(b, at + Pt(Seed, k, r, r * .6f), new Vector2(0, 1.5f), (now * 2 + H(Seed, k)) % .4f, .4f, .12f, Seed + k);
                    return;
                }
                float o = Fade(t, 1.3f, 1.9f);
                b.Floor(FxShape.Shade, at, r * 1.4f, A(Mud, .7f * o), 0, Seed);
                b.Quad(FxShape.Cracks, at, new Vector2(r * 1.3f, r * .82f), 0, A(Bark, .7f * o), 1, Seed, 1);
                b.Quad(FxShape.Glow, at, r * 1.5f, A(Bark, .45f * Fade(t, 0, .3f)), 0, Seed);
                for (int k = 0; k < 3; k++)
                {
                    float lag = k * .07f, side = k == 1 ? 1 : -1, u = t - lag; if (u < 0) continue;
                    float grow = EaseOut3(Rise(u, 0, .22f)), whip = Mathf.Sin(Rise(u, .22f, .55f) * Mathf.PI) * 35 * side, slam = EaseOut(Rise(u, .5f, .7f)) * 72 * -side;
                    Vector2 bp = at + new Vector2((k - 1) * r * .6f, (k == 1 ? -.3f : .2f));
                    Stalk(b, bp, whip + slam, r * 1.4f, r * .6f, Bark, o, grow, Seed + k, .5f + k * .3f);
                    Grit(b, bp, r * .6f, new Color(.48f, .38f, .24f), .5f * Fade(u, .1f, 1.2f) * Rise(u, 0, .1f), u, Seed + k + 40);
                    for (int j = 0; j < 3; j++) Splinter(b, bp + Vector2.up * .2f, new Vector2((j - 1) * 2f, 3 + H(Seed, j + k * 3) * 1.5f), u, .6f, .14f, new Color(.62f, .4f, .18f), Seed + j + k * 3);
                    if (u > .62f && u < 1.1f)
                    {
                        Vector2 hit = bp + Vec(90 + whip + slam) * r * 2.5f; float a = Rise(u, .62f, 1.1f);
                        b.Quad(FxShape.Smoke, hit, r * (.4f + .5f * a), A(new Color(.42f, .3f, .17f), .7f * (1 - a)), a, Seed + k);
                        Grit(b, hit, r * (.5f + .5f * a), new Color(.5f, .4f, .26f), .5f * (1 - a), a, Seed + k + 50);
                        for (int j = 0; j < 3; j++) Splinter(b, hit, new Vector2((j - 1) * 2.4f, 2 + H(Seed, j + k) * 1.5f), u - .62f, .55f, .13f, new Color(.66f, .43f, .2f), Seed + j + k * 7 + 20);

                        for (int j = 0; j < 4; j++) Clod(b, hit, new Vector2((j - 1.5f) * 1.8f, 2.6f + H(Seed, j + k) * 1.5f), u - .62f, .6f, .14f, Seed + j + k * 5);
                    }
                    for (int j = 0; j < 3; j++) Lf(b, bp + Vec(90 + whip + slam) * r * (.6f + j * .5f) * grow, .35f * grow, whip + slam + 90 + (j % 2 == 0 ? 50 : -50), j % 2 == 0 ? LeafG : DeepG, o, Seed + j + k * 3);
                }
                for (int k = 0; k < 14; k++)
                {
                    Vector2 dd = Dir(Seed + 5, k, 14, .6f);
                    Clod(b, at + dd * r * .4f, new Vector2(dd.x * 3, 3.5f + 2.5f * H(Seed, k)), t, .9f, .18f + .1f * H(Seed, k + 1), Seed + k);
                }
                for (int k = 0; k < 14; k++)
                {
                    float u = (t * .7f + H(Seed, k + 20)) % 1; Vector2 p = at + Pt(Seed, k + 30, r * 1.1f, r * .7f) + Vector2.up * u * 2.2f;
                    Glint(b, p, .2f, k % 2 == 0 ? LeafG : Pollen, .85f * o * (1 - u), k * 30 + t * 140);
                }
            }
        }
    }
}
