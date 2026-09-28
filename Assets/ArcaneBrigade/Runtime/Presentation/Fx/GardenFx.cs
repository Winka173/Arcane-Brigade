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
                for (int k = 0; k < 3; k++)
                    b.Floor(Curl, at + Dir(Seed, k, 3, .5f) * r * .45f, r * .75f, A(LeafG, .95f * o), g, Seed + k, k % 2 == 0 ? 1 : -1);
                for (int k = 0; k < 2; k++)
                {
                    float side = k == 0 ? -1 : 1;
                    Stalk(b, at + new Vector2(side * r * .45f, -.1f), side * -14, r * .85f, r * .28f, LeafG, o, g, Seed + k, .6f);
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
                if (lift < 1) Fist(b, fist, r, 1 - lift, t < 0 ? 0 : t, Seed);
                if (t < 0)
                {
                    float g = EaseOut(pre);
                    b.Floor(FxShape.Shade, at, r * 1.4f * (.4f + .6f * g), A(Mud, .7f * g), 0, Seed);
                    for (int k = 0; k < 10; k++)
                    {
                        float u = (pre * 1.3f + H(Seed, k)) % 1; Vector2 p = at + Pt(Seed, k, r * 1.2f, r * .7f) + Vector2.up * (3 - u * 3);
                        Lf(b, p, .4f, now * 160 + k * 40, k % 2 == 0 ? LeafG : DeepG, .9f * g * (1 - u), Seed + k);
                    }
                    return;
                }
                float o = Fade(t, 1.2f, 2.1f);
                b.Quad(FxShape.Glow, at, r * 1.6f, A(LeafG, .4f * Fade(t, 0, .3f)), 0, Seed);
                b.Floor(FxShape.Shade, at, r * 1.45f, A(Mud, .7f * o), 0, Seed);
                b.Quad(FxShape.Cracks, at, new Vector2(r * 1.4f, r * .9f), 0, A(LeafG, .9f * o), 1, Seed, 2);
                float spread = EaseOut3(Rise(t, .02f, .35f));
                for (int k = 0; k < 8; k++)
                {
                    Vector2 dd = Dir(Seed + 2, k, 8, .62f), mid = at + dd * r * .62f; float L = r * .7f * (.9f + .3f * H(Seed, k));
                    b.Quad(FxShape.Shade, mid, new Vector2(L * spread, r * .2f), Ang(dd), A(Mud, .5f * o), 0, Seed + k);
                    b.Quad(Root, mid, new Vector2(r * .2f, L), Ang(dd) - 90, A(Bark, o), spread, Seed + k, .4f);
                    if (spread > .9f) b.Quad(Petal, at + dd * r * 1.25f, .4f * Overshoot(Rise(t, .3f + k * .03f, .5f + k * .03f)), t * 40, A(k % 2 == 0 ? Pink : Pollen, o), 0, Seed + k, 0);
                }
                for (int k = 0; k < 20; k++)
                {
                    float life = .9f + .5f * H(Seed, k + 70), u = t / life; if (u >= 1) continue;
                    Vector2 dir = Dir(Seed, k + 40, 20, .5f), p = at + dir * r * 1.5f * EaseOut(u) + Vector2.up * (3.4f * H(Seed, k + 90) * u - 4.2f * u * u + .3f);
                    Lf(b, p, .42f, k * 50 + t * 400, k % 3 == 0 ? Pink : (k % 3 == 1 ? LeafG : DeepG), 1 - u * u, Seed + k);
                }
                for (int k = 0; k < 9; k++)
                {
                    Vector2 dd = Dir(Seed + 7, k, 9, .62f);
                    b.Quad(FxShape.Smoke, at + dd * r * (.4f + .9f * EaseOut(Rise(t, 0, .8f))), r * .45f, A(new Color(.42f, .32f, .2f), .55f * Fade(t, .1f, 1.2f)), t, Seed + k);
                }
            }
            /// <summary>The fist: a thick bark forearm reaching down out of the sky with four knuckles and leaves on its wrist.</summary>
            private static void Fist(FxBatch b, Vector2 p, float r, float a, float t, int seed)
            {
                float s = r * .6f; Color wood = new Color(.55f, .3f, .1f);
                b.Quad(FxShape.Glow, p, s * 2.2f, A(LeafG, .15f * a), 0, seed);
                b.Quad(FxShape.Shade, p + Vector2.up * s * 3.2f, new Vector2(s * 1.2f, s * 3.4f), 0, A(Mud, .85f * a), 0, seed);
                b.Quad(Root, p + Vector2.up * s * 3.6f, new Vector2(s * 1.3f, s * 3.6f), 180, A(Bark, a), 1, seed, .2f);
                b.Quad(FxShape.Shade, p, s * 1.4f, A(Mud, .9f * a), 0, seed);
                b.Quad(FxShape.Shade, p, new Vector2(s * 1.25f, s * 1.1f), 0, A(wood, a), 0, seed);
                b.Quad(FxShape.Cracks, p, s * .9f, 0, A(Bark, .5f * a), 1, seed, 1);
                for (int k = 0; k < 4; k++)
                {
                    Vector2 kp = p + new Vector2((k - 1.5f) * s * .52f, -s * .75f + Mathf.Abs(k - 1.5f) * s * .18f);
                    b.Quad(FxShape.Shade, kp, s * .42f, A(new Color(.7f, .42f, .16f), a), 0, seed);
                    b.Quad(FxShape.Glow, kp, s * .35f, A(Bark, .25f * a), 0, seed);
                }
                for (int k = 0; k < 4; k++) Lf(b, p + new Vector2((k - 1.5f) * s * .6f, s * 1.3f), .4f, 90 + (k - 1.5f) * 40 + Mathf.Sin(t * 9 + k) * 10, k % 2 == 0 ? LeafG : DeepG, a, seed + k);
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
                    if (u > .62f && u < 1.1f)
                    {
                        Vector2 hit = bp + Vec(90 + whip + slam) * r * 2.5f; float a = Rise(u, .62f, 1.1f);
                        b.Quad(FxShape.Smoke, hit, r * (.4f + .5f * a), A(new Color(.42f, .3f, .17f), .7f * (1 - a)), a, Seed + k);
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
