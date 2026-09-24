using System;
using System.Collections.Generic;
using UnityEngine;
using static ArcaneBrigade.FxKit;

namespace ArcaneBrigade
{
    /// <summary>Storm bond. Step 2's chain lightning is drawn by the existing hop code (kept: it reads well). Here are the moments
    /// the ladder adds: step 6 a thunderbolt that lights the whole crowd's ground, step 8 a thunderstorm over the arena.</summary>
    public static class StormFx
    {
        public static void Register(Dictionary<string, Func<FxEffect>> map)
        {
            map["storm-thunderbolt"] = () => new Thunderbolt();
            map["storm-thunderstorm"] = () => new Thunderstorm();
        }

        /// <summary>Step 6. The sky goes dark over the crowd and a cloud swells; a white flash blinds; a pillar of three forked
        /// bolts slams down, the shock runs out across the floor, glowing cracks spread under the enemies, sparks fly and smoke rises.</summary>
        private sealed class Thunderbolt : FxEffect
        {
            protected override float Life => 1.7f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Radius * 1.25f;
                if (t < 0)
                {
                    // gathering: a dark storm cloud swells over the crowd, violet light pulses inside it, small arcs crackle down to
                    // the enemies and the ground under them darkens
                    float g = EaseOut(pre), flick = .7f + .3f * Mathf.Sin(now * 47);
                    Vector2 cloud = at + Vector2.up * 4.1f;
                    b.Quad(FxShape.Shade, at, new Vector2(r * 1.8f, r * 1.1f) * (.5f + .5f * g), 0, A(new Color(.02f, .01f, .1f), .7f * g), 0, Seed);
                    for (int k = 0; k < 9; k++)
                    {
                        Vector2 p = cloud + new Vector2((k - 4) * r * .34f, Mathf.Sin(k * 2.3f) * .5f + (k % 2) * .35f) * g;
                        float lit = flick * (.3f + .7f * H(Seed + 9, k + (int)(now * 11)));
                        b.Quad(FxShape.Glow, p + Vector2.down * .5f, r * .8f * g, A(Violet, .32f * g * lit), 0, Seed);
                        b.Quad(FxShape.Smoke, p, new Vector2(r * (.85f + .4f * H(Seed, k)), r * .6f) * g, 0, A(new Color(.015f, .012f, .06f), 1f * g), pre, Seed + k);
                        b.Quad(FxShape.Smoke, p + new Vector2(.15f, .3f), new Vector2(r * .6f, r * .4f) * g, 0, A(new Color(.03f, .025f, .1f), .9f * g), pre + .3f, Seed + k + 40);
                    }
                    int crackle = Mathf.FloorToInt(now * 22);
                    for (int k = 0; k < 4; k++)
                    {
                        if (H(Seed + crackle, k) > .25f + .5f * g) continue;
                        Vector2 from = cloud + new Vector2((H(Seed + crackle, k + 5) - .5f) * r * 2, -.4f), to = at + Dir(Seed + crackle, k, 4) * r * .9f;
                        Bolt(b, from, to, .06f, Cyan, Seed + crackle * 7 + k, .5f, .75f * g, 7);
                    }
                    for (int k = 0; k < 10; k++)
                    {
                        float u = (pre * 1.6f + H(Seed, k)) % 1; Vector2 p = at + Dir(Seed, k, 10) * r * .9f + Vector2.up * u * 2f;
                        Glint(b, p, .25f, Cyan, .9f * g * (1 - u), k * 30 + now * 90);
                    }
                    return;
                }
                float flash = Mathf.Pow(Fade(t, 0, .22f), 2);
                b.Quad(FxShape.Glow, at + Vector2.up * 1.2f, r * 3f, A(White, .8f * flash), 0, Seed);
                b.Quad(FxShape.Glow, at, r * 2.1f, A(Cyan, .55f * Fade(t, 0, .9f)), 0, Seed);
                // the pillar: a wide column of light with three forked bolts racing down it
                float strike = Fade(t, .05f, .5f);
                if (strike > 0)
                {
                    b.Beam(at + Vector2.up * 12, at, r * .38f, A(Cyan, .4f * strike), t, Seed, 0);
                    int frame = Mathf.FloorToInt(t * 24);
                    for (int k = 0; k < 3; k++)
                    {
                        Vector2 top = at + new Vector2((H(Seed, k) - .5f) * r * 1.1f, 9.5f), foot = at + new Vector2((H(Seed, k + 9) - .5f) * r * .5f, 0);
                        Bolt(b, top, foot, k == 0 ? .26f : .17f, k == 1 ? Violet : Cyan, Seed * 3 + k * 17 + frame * 5, 1.3f, strike);
                        for (int f = 0; f < 3; f++)
                        {
                            float at01 = .25f + .6f * H(Seed, k * 7 + f); Vector2 root = Vector2.Lerp(top, foot, at01);
                            Vector2 tip = root + new Vector2((H(Seed, f + k * 11) - .5f) * 6, -2.6f * H(Seed, f + 31));
                            Bolt(b, root, tip, .09f, Cyan, Seed + frame * 3 + f + k * 13, .7f, strike * .9f, 8);
                        }
                    }
                }
                // the shock runs out across the floor, then cracks of light spread under the whole crowd
                b.Floor(FxShape.Shock, at, r * 1.7f, A(Cyan, .95f), Rise(t, 0, .8f), Seed);
                float fissure = Fade(t, .55f, 1.6f), grow = EaseOut3(Rise(t, 0, .16f));
                for (int k = 0; k < 11; k++)
                {
                    Vector2 end = at + Dir(Seed + 3, k, 11, .5f) * r * (1.05f + .6f * H(Seed, k + 60)) * grow;
                    Bolt(b, at, end, .045f, Cyan, Seed + k * 11, .55f, fissure, 9);
                    if (k % 2 == 0) { Vector2 mid = Vector2.Lerp(at, end, .55f); Bolt(b, mid, mid + Dir(Seed + k, k + 2, 7, .5f) * r * .5f * grow, .03f, Cyan, Seed + k * 3, .3f, fissure * .8f, 5); }
                }
                b.Floor(FxShape.Smoke, at, r * 1.1f, A(new Color(.02f, .02f, .05f), .5f * Fade(t, .3f, 1.6f)), Rise(t, 0, 1), Seed + 2);
                b.Quad(FxShape.Rays, at, r * 2.6f, A(White, .7f * Fade(t, 0, .45f)), t, Seed, 4);
                // sparks arc out with gravity
                for (int k = 0; k < 26; k++)
                {
                    float life = .6f + .5f * H(Seed, k + 70), u = t / life; if (u <= 0 || u >= 1) continue;
                    Vector2 dir = Dir(Seed, k + 40, 26, .5f), p = at + dir * r * 1.5f * EaseOut(u) + Vector2.up * (3.2f * H(Seed, k + 90) * u - 4.4f * u * u + .2f);
                    Glint(b, p, .3f * (1 - u * .5f), k % 3 == 0 ? White : Cyan, 1 - u * u, k * 40 + t * 380);
                }
                SmokeColumn(b, at, r * .5f, 3.6f, t, new Color(.05f, .05f, .1f), Seed);
            }
        }

        /// <summary>Step 8. As weather: the arena goes dark under a roiling cloud ceiling, rain streams down at an angle, and sheet
        /// lightning flickers across the clouds. As a strike: a violet bolt from the ceiling with a rain splash and a crown of ground arcs.</summary>
        private sealed class Thunderstorm : FxEffect
        {
            protected override float Life => 1.4f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                if (Moment.Kind == BondMomentKind.Weather) { Sky(b, now); return; }
                Vector2 at = At; float r = Mathf.Max(1.1f, Radius);
                // a ball of lightning: it sinks out of the clouds on a spiral of sparks, hangs over the crowd pulsing three times,
                // each pulse flinging short arcs at the ground, then collapses into one bright point
                Vector2 hover = at + Vector2.up * 1.1f;
                if (t < 0)
                {
                    float u = pre * pre; Vector2 p = Vector2.Lerp(at + new Vector2(-1.4f, 8.5f), hover, u);
                    Orb(b, p, .9f + .4f * pre, now, 1, Seed);
                    for (int k = 1; k <= 9; k++)
                    {
                        float back = Mathf.Max(0, u - k * .05f); Vector2 q = Vector2.Lerp(at + new Vector2(-1.4f, 8.5f), hover, back) + new Vector2(Mathf.Sin(k * 1.7f + now * 18) * .22f, 0);
                        Glint(b, q, .22f * (1 - k / 10f), Violet, .9f * (1 - k / 10f), k * 50);
                    }
                    b.Floor(FxShape.Glow, at, r * .9f * pre, A(Violet, .4f * pre), 0, Seed);
                    return;
                }
                float collapse = Rise(t, .85f, 1.15f), size = (1 + .12f * Mathf.Sin(t * 22)) * (1 - collapse * .9f);
                Orb(b, hover, 1.25f * size, now, 1 - collapse * .5f, Seed);
                int pulse = Mathf.FloorToInt(t / .26f); float within = (t - pulse * .26f) / .26f;
                if (t < .85f && pulse < 3)
                {
                    for (int k = 0; k < 7; k++)
                    {
                        Vector2 end = at + Dir(Seed + pulse * 13, k, 7, .5f) * r * (1.1f + .5f * H(Seed + pulse, k)) * EaseOut(Rise(within, 0, .35f));
                        Bolt(b, hover, end, .11f, k % 2 == 0 ? Violet : Cyan, Seed + pulse * 31 + k * 7 + Mathf.FloorToInt(within * 9), .5f, Fade(within, .25f, .8f), 7);
                    }
                    b.Quad(FxShape.Glow, hover, r * 1.8f, A(Violet, .55f * Fade(within, 0, .5f)), 0, Seed);
                }
                if (collapse > 0)
                {
                    for (int k = 0; k < 14; k++)
                    {
                        Vector2 from = hover + Dir(Seed, k, 14, .8f) * r * 1.6f;
                        Glint(b, Vector2.Lerp(from, hover, EaseOut(collapse)), .24f, White, 1 - collapse, k * 20);
                    }
                    b.Quad(FxShape.Glow, hover, r * 2.4f * (1 - collapse), A(White, .9f * collapse), 0, Seed);
                }
            }

            /// <summary>The ball itself: a glowing core inside a spinning vortex of plasma.</summary>
            private static void Orb(FxBatch b, Vector2 at, float radius, float now, float alpha, int seed)
            {
                b.Quad(FxShape.Glow, at, radius * 3f, A(Violet, .5f * alpha), 0, seed);
                b.Quad(FxShape.Vortex, at, radius * 1.5f, now * 140, A(Violet, 1.3f * alpha), now * .8f, seed, 1);
                b.Quad(FxShape.Vortex, at, radius * 1.1f, -now * 220, A(Cyan, 1.2f * alpha), now * 1.3f, seed + 5, 0);
                b.Quad(FxShape.Glow, at, radius * .7f, A(White, alpha), 0, seed);
            }

            private void Sky(FxBatch b, float now)
            {
                float age = now - Born, on = Rise(age, 0, 2.2f) * Fade(now, End - 1.5f, End);
                Rect a = Arena; Vector2 c = a.center;
                b.Quad(FxShape.Shade, c, new Vector2(a.width * .75f, a.height * .75f), 0, A(new Color(.02f, .03f, .12f), .42f * on), 0, Seed);
                // cloud ceiling along the top edge, drifting
                for (int k = 0; k < 11; k++)
                {
                    float x = a.xMin + ((k * .1f + now * .012f) % 1.1f) * a.width * 1.1f - a.width * .05f;
                    Vector2 p = new Vector2(x, a.yMax + .2f + Mathf.Sin(k * 1.9f + now * .35f) * .5f);
                    b.Quad(FxShape.Smoke, p, new Vector2(2.6f + H(Seed, k), 1.5f), 0, A(new Color(.08f, .1f, .2f), .95f * on), now * .05f + k, Seed + k);
                }
                // sheet lightning inside the clouds, a quick double flicker every few seconds
                float cycle = now / 3.1f; int n = Mathf.FloorToInt(cycle); float f = (cycle - n) * 3.1f;
                float sheet = Mathf.Max(Fade(f, 0, .12f), .7f * Fade(f - .2f, 0, .1f)) * (H(Seed, n) > .25f ? 1 : 0);
                if (sheet > 0)
                {
                    Vector2 p = new Vector2(a.xMin + a.width * H(Seed, n + 5), a.yMax - .3f);
                    b.Quad(FxShape.Glow, p, new Vector2(a.width * .35f, 3.2f), 0, A(new Color(.7f, .8f, 1f), .85f * sheet * on), 0, Seed);
                    b.Quad(FxShape.Glow, c, new Vector2(a.width * .6f, a.height * .6f), 0, A(Violet, .16f * sheet * on), 0, Seed);
                }
                // rain: slanted streaks falling and wrapping
                for (int k = 0; k < 120; k++)
                {
                    float sp = 14 + 6 * H(Seed, k + 300), u = (H(Seed, k) + now * sp / (a.height + 4)) % 1;
                    Vector2 p = new Vector2(a.xMin + a.width * H(Seed, k + 150) + (1 - u) * 1.6f, a.yMax + 2 - u * (a.height + 4));
                    b.Beam(p, p + new Vector2(-.18f, -.95f), .022f, A(new Color(.65f, .8f, 1f), .5f * on), 0, k, 0);
                }
            }
        }
    }
}
