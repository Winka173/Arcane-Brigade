using System;
using System.Collections.Generic;
using UnityEngine;
using static ArcaneBrigade.FxKit;

namespace ArcaneBrigade
{
    /// <summary>Storm bond. Step 2's chain lightning is drawn by the existing hop code (kept: it reads well). Here are the moments
    /// the ladder adds: step 6 a thunderbolt out of a boiling cloud over the crowd, step 8 a thunderstorm over the arena.</summary>
    public static class StormFx
    {
        public static void Register(Dictionary<string, Func<FxEffect>> map)
        {
            map["storm-thunderbolt"] = () => new Thunderbolt();
            map["storm-thunderstorm"] = () => new Thunderstorm();
        }

        private static readonly Color Belly = new Color(.07f, .06f, .15f, 1), Billow = new Color(.36f, .36f, .56f, 1), Cap = new Color(.7f, .72f, .92f, 1);
        private static readonly Color Ash = new Color(.42f, .43f, .54f, 1), Rain = new Color(.65f, .8f, 1f, 1);

        /// <summary>Step 6. A storm cloud boils up over the crowd (dark bellies, lit billows, lightning glowing inside it, rain wisps); a
        /// white flash; a pillar of three forked bolts slams down, the shock and a ring of dust run out across the floor, glowing cracks
        /// spread under the enemies, the cloud tears apart, sparks bounce and lit grey smoke boils up out of the scorched crater.</summary>
        private sealed class Thunderbolt : FxEffect
        {
            protected override float Life => 1.7f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Radius * 1.1f; Vector2 cloud = at + Vector2.up * 3.7f;
                if (t < 0)
                {
                    float g = EaseOut(pre);
                    b.Floor(FxShape.Shade, at, r * 1.35f * (.5f + .5f * g), A(new Color(.02f, .01f, .1f), .55f * g), 0, Seed);
                    Cloud(b, cloud, r, g, 0, now, Seed);
                    // lightning inside the cloud: a few billows light up at random, joined by a short crawling arc
                    int fl = Mathf.FloorToInt(now * 14);
                    for (int k = 0; k < 3; k++)
                    {
                        if (H(Seed + fl, k + 3) > .35f + .4f * g) continue;
                        Vector2 p = cloud + new Vector2((H(Seed + fl, k) - .5f) * r * 2, .3f + H(Seed + fl, k + 7) * .5f);
                        b.Quad(FxShape.Glow, p, r * .75f, A(k == 0 ? White : Violet, .55f * g), 0, Seed);
                        Bolt(b, p, p + new Vector2((H(Seed + fl, k + 9) - .5f) * r * 1.4f, (H(Seed + fl, k + 11) - .5f) * .6f), .04f, Cyan, Seed + fl * 5 + k, .3f, .8f * g, 5);
                    }
                    // rain wisps hanging under the belly
                    for (int k = 0; k < 12; k++)
                    {
                        float u = (now * 2.2f + H(Seed, k + 60)) % 1; Vector2 p = cloud + new Vector2((H(Seed, k + 61) - .5f) * r * 2, -.6f - u * 1.6f);
                        b.Beam(p, p + new Vector2(-.05f, -.45f), .03f, A(Rain, .45f * g * (1 - u)), 0, k, 0);
                    }
                    int crackle = Mathf.FloorToInt(now * 22);
                    for (int k = 0; k < 4; k++)
                    {
                        if (H(Seed + crackle, k) > .25f + .5f * g) continue;
                        Vector2 from = cloud + new Vector2((H(Seed + crackle, k + 5) - .5f) * r * 2, -.6f), to = at + Dir(Seed + crackle, k, 4) * r * .9f;
                        Bolt(b, from, to, .05f, Cyan, Seed + crackle * 7 + k, .5f, .7f * g, 7);
                    }
                    for (int k = 0; k < 10; k++)
                    {
                        float u = (pre * 1.6f + H(Seed, k)) % 1; Vector2 p = at + Dir(Seed, k, 10) * r * .9f + Vector2.up * u * 2f;
                        Glint(b, p, .22f, Cyan, .9f * g * (1 - u), k * 30 + now * 90);
                    }
                    return;
                }
                // the cloud tears apart after the bolt: its billows drift outward and thin out
                Cloud(b, cloud, r, 1 - Rise(t, 0, 1.1f), Rise(t, 0, 1.1f), now, Seed);
                float flash = Mathf.Pow(Fade(t, 0, .22f), 2);
                b.Quad(FxShape.Glow, at + Vector2.up * 1.2f, r * 1.5f, A(White, .5f * flash), 0, Seed);
                b.Quad(FxShape.Glow, at, r * 1.5f, A(Cyan, .5f * Fade(t, 0, .9f)), 0, Seed);
                // the pillar: a column of light with three forked bolts racing down it out of the cloud
                float strike = Fade(t, .05f, .5f);
                if (strike > 0)
                {
                    b.Beam(cloud, at, r * .3f, A(Cyan, .4f * strike), t, Seed, 0);
                    int frame = Mathf.FloorToInt(t * 24);
                    for (int k = 0; k < 3; k++)
                    {
                        Vector2 top = cloud + new Vector2((H(Seed, k) - .5f) * r * 1.1f, .2f), foot = at + new Vector2((H(Seed, k + 9) - .5f) * r * .5f, 0);
                        Bolt(b, top, foot, k == 0 ? .24f : .15f, k == 1 ? Violet : Cyan, Seed * 3 + k * 17 + frame * 5, .9f, strike);
                        for (int f = 0; f < 2; f++)
                        {
                            float at01 = .25f + .6f * H(Seed, k * 7 + f); Vector2 root = Vector2.Lerp(top, foot, at01);
                            Vector2 tip = root + new Vector2((H(Seed, f + k * 11) - .5f) * 3.2f, -1.3f * H(Seed, f + 31));
                            Bolt(b, root, tip, .07f, Cyan, Seed + frame * 3 + f + k * 13, .5f, strike * .9f, 6);
                        }
                    }
                }
                // the shock runs out across the floor, then cracks of light spread under the crowd
                b.Floor(FxShape.Shock, at, r * 1.45f, A(Cyan, .95f), Rise(t, 0, .8f), Seed);
                float fissure = Fade(t, .55f, 1.6f), grow = EaseOut3(Rise(t, 0, .16f));
                for (int k = 0; k < 11; k++)
                {
                    Vector2 end = at + Dir(Seed + 3, k, 11, .5f) * r * (.95f + .45f * H(Seed, k + 60)) * grow;
                    Bolt(b, at, end, .045f, Cyan, Seed + k * 11, .55f, fissure, 9);
                    if (k % 2 == 0) { Vector2 mid = Vector2.Lerp(at, end, .55f); Bolt(b, mid, mid + Dir(Seed + k, k + 2, 7, .5f) * r * .45f * grow, .03f, Cyan, Seed + k * 3, .3f, fissure * .8f, 5); }
                }
                b.Quad(FxShape.Rays, at, r * 1.6f, A(White, .7f * Fade(t, 0, .45f)), t, Seed, 4);
                // scorched crater and a low ring of dust kicked out by the shock
                b.Floor(FxShape.Shade, at, r * .75f, A(new Color(.03f, .02f, .05f), .7f * Rise(t, 0, .1f) * Fade(t, 1.1f, 1.7f)), 0, Seed + 2);
                float du = Rise(t, .02f, 1.3f);
                if (du < 1) for (int k = 0; k < 10; k++)
                {
                    Vector2 p = at + Dir(Seed + 5, k, 10, .55f) * r * (.35f + 1.05f * EaseOut(du));
                    b.Quad(FxShape.Dust, p, new Vector2(r * .42f, r * .26f) * (.7f + .6f * du), 0, A(new Color(.5f, .48f, .56f), .8f * (1 - du)), du, Seed + k);
                }
                // after the strike: lit grey smoke boils up out of the crater, embers winking inside it
                for (int k = 0; k < 12; k++)
                {
                    float u = Rise(t, .06f + k * .04f, 1.5f + k * .02f); if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + new Vector2((H(Seed, k + 120) - .5f) * r * 1.1f * (1 + u) + Mathf.Sin(u * 4 + k) * .2f, .1f + 2.4f * EaseOut(u) * (.4f + .6f * H(Seed, k + 121)));
                    float sz = r * (.34f + .4f * u) * (.8f + .4f * H(Seed, k + 122));
                    b.Quad(FxShape.Smoke, p, sz, 0, A(Ash, Mathf.Min(1, u * 5) * (1 - u * u)), u + k, Seed + k + 7);
                    if (k % 2 == 0) Glint(b, p + Vector2.down * sz * .3f, .12f, Cyan, .8f * (1 - u) * (.5f + .5f * Mathf.Sin(t * 30 + k)), k * 33);
                }
                // sparks arc out with gravity and bounce once on the floor
                for (int k = 0; k < 26; k++)
                {
                    float life = .6f + .5f * H(Seed, k + 70), u = t / life; if (u <= 0 || u >= 1) continue;
                    Vector2 dir = Dir(Seed, k + 40, 26, .5f), p = at + dir * r * 1.3f * EaseOut(u) + Vector2.up * Mathf.Abs(2.8f * H(Seed, k + 90) * u - 4f * u * u + .2f);
                    Glint(b, p, .28f * (1 - u * .5f), k % 3 == 0 ? White : Cyan, 1 - u * u, k * 40 + t * 380);
                }
            }

            /// <summary>The storm cloud: dark bellies along the bottom, lit slate billows rolling on top with pale caps where the light
            /// catches them. <paramref name="g"/> is how much of it there is, <paramref name="spread"/> pushes it apart.</summary>
            private static void Cloud(FxBatch b, Vector2 c, float r, float g, float spread, float now, int seed)
            {
                if (g <= .01f) return;
                float grow = spread > 0 ? 1 : Mathf.Min(1, g * 1.4f);
                for (int k = 0; k < 7; k++)
                {
                    float x = (k - 3) / 3f, sz = r * (.42f + .14f * H(seed, k)) * grow;
                    Vector2 p = c + new Vector2(x * r * (1.15f + spread) + Mathf.Sin(now * 1.3f + k) * .12f, -.35f - .25f * x * x + spread * .4f);
                    b.Quad(FxShape.Smoke, p, new Vector2(sz * 1.35f, sz * .8f), 0, A(Belly, .9f * g), now * .35f, seed + k);
                }
                for (int k = 0; k < 9; k++)
                {
                    float x = (k - 4) / 4f, sz = r * (.34f + .2f * H(seed, k + 20)) * grow * (1 - .35f * x * x) * (1 + .4f * spread);
                    Vector2 p = c + new Vector2(x * r * (1.05f + spread) + Mathf.Sin(now * 1.7f + k * 2) * .1f, .15f + (1 - x * x) * .55f + H(seed, k + 30) * .3f + .2f * Mathf.Sin(now * 2 + k) + spread * .6f);
                    b.Quad(FxShape.Smoke, p, sz, 0, A(Billow, .95f * g), now * .4f + k, seed + k + 40);
                    b.Quad(FxShape.Smoke, p + new Vector2(-.18f, .2f) * sz, sz * .55f, 0, A(Cap, .55f * g), now * .5f + k, seed + k + 80);
                }
            }
        }

        /// <summary>Step 8. As weather: a light storm over the field: a roiling cloud ceiling along the top edge, thin slanted rain with
        /// splashes ringing on the floor, sheet lightning flickering in the clouds. As a strike: a ball of lightning sinks out of the sky,
        /// pulses arcs at the ground three times and collapses with a puff of ozone smoke.</summary>
        private sealed class Thunderstorm : FxEffect
        {
            protected override float Life => 1.4f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                if (Moment.Kind == BondMomentKind.Weather) { Sky(b, now); return; }
                Vector2 at = At; float r = Mathf.Max(1.1f, Radius);
                // a ball of lightning: it sinks out of the clouds on a spiral of sparks, hangs over the crowd pulsing three times,
                // each pulse flinging short arcs at the ground, then collapses into one bright point and a puff of smoke
                Vector2 hover = at + Vector2.up * 1.1f;
                if (t < 0)
                {
                    float u = pre * pre; Vector2 p = Vector2.Lerp(at + new Vector2(-1.4f, 6.5f), hover, u);
                    Orb(b, p, .9f + .4f * pre, now, 1, Seed);
                    for (int k = 1; k <= 9; k++)
                    {
                        float back = Mathf.Max(0, u - k * .05f); Vector2 q = Vector2.Lerp(at + new Vector2(-1.4f, 6.5f), hover, back) + new Vector2(Mathf.Sin(k * 1.7f + now * 18) * .22f, 0);
                        Glint(b, q, .22f * (1 - k / 10f), Violet, .9f * (1 - k / 10f), k * 50);
                    }
                    b.Floor(FxShape.Glow, at, r * .9f * pre, A(Violet, .4f * pre), 0, Seed);
                    return;
                }
                float collapse = Rise(t, .85f, 1.15f), size = (1 + .12f * Mathf.Sin(t * 22)) * (1 - collapse * .9f);
                Orb(b, hover, 1.25f * size, now, 1 - collapse * .5f, Seed);
                // motes of plasma circling the ball
                for (int k = 0; k < 8; k++)
                {
                    float ang = now * (6 + k * .7f) + k * .8f; Vector2 p = hover + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang) * .45f) * 1.5f * size;
                    Glint(b, p, .14f, k % 2 == 0 ? White : Cyan, .9f * (1 - collapse), k * 45);
                }
                int pulse = Mathf.FloorToInt(t / .26f); float within = (t - pulse * .26f) / .26f;
                if (t < .85f && pulse < 3)
                {
                    for (int k = 0; k < 7; k++)
                    {
                        Vector2 end = at + Dir(Seed + pulse * 13, k, 7, .5f) * r * (1.1f + .5f * H(Seed + pulse, k)) * EaseOut(Rise(within, 0, .35f));
                        Bolt(b, hover, end, .11f, k % 2 == 0 ? Violet : Cyan, Seed + pulse * 31 + k * 7 + Mathf.FloorToInt(within * 9), .5f, Fade(within, .25f, .8f), 7);
                        // each arc kicks a small spark splash where it lands
                        if (within < .5f) Glint(b, end, .25f, White, Fade(within, .15f, .5f), k * 20);
                    }
                    b.Quad(FxShape.Glow, hover, r * 1.5f, A(Violet, .55f * Fade(within, 0, .5f)), 0, Seed);
                    b.Floor(FxShape.Shock, at, r * 1.2f, A(Violet, .6f), Rise(within, 0, 1), Seed + pulse);
                }
                if (collapse > 0)
                {
                    for (int k = 0; k < 14; k++)
                    {
                        Vector2 from = hover + Dir(Seed, k, 14, .8f) * r * 1.6f;
                        Glint(b, Vector2.Lerp(from, hover, EaseOut(collapse)), .24f, White, 1 - collapse, k * 20);
                    }
                    b.Quad(FxShape.Glow, hover, r * 1.8f * (1 - collapse), A(White, .9f * collapse), 0, Seed);
                }
                // ozone: a little scorched patch under the ball and a puff of violet-grey smoke when it is gone
                b.Floor(FxShape.Shade, at, r * .6f, A(new Color(.04f, .02f, .08f), .5f * Rise(t, 0, .3f) * Fade(t, 1f, 1.4f)), 0, Seed);
                for (int k = 0; k < 5; k++)
                {
                    float u = Rise(t, .95f + k * .03f, 1.4f); if (u <= 0 || u >= 1) continue;
                    Vector2 p = hover + Dir(Seed + 2, k, 5, .7f) * .5f * (1 + u) + Vector2.up * u * .8f;
                    b.Quad(FxShape.Smoke, p, .45f + .5f * u, 0, A(new Color(.5f, .45f, .65f), .8f * (1 - u)), u + k, Seed + k);
                }
            }

            /// <summary>The ball itself: a glowing core inside a spinning vortex of plasma.</summary>
            private static void Orb(FxBatch b, Vector2 at, float radius, float now, float alpha, int seed)
            {
                b.Quad(FxShape.Glow, at, radius * 2.4f, A(Violet, .5f * alpha), 0, seed);
                b.Quad(FxShape.Vortex, at, radius * 1.5f, now * 140, A(Violet, 1.3f * alpha), now * .8f, seed, 1);
                b.Quad(FxShape.Vortex, at, radius * 1.1f, -now * 220, A(Cyan, 1.2f * alpha), now * 1.3f, seed + 5, 0);
                b.Quad(FxShape.Glow, at, radius * .7f, A(White, alpha), 0, seed);
            }

            private void Sky(FxBatch b, float now)
            {
                float age = now - Born, on = Rise(age, 0, 2.2f) * Fade(now, End - 1.5f, End);
                Rect a = Arena; Vector2 c = a.center;
                b.Quad(FxShape.Shade, c, new Vector2(a.width * .75f, a.height * .75f), 0, A(new Color(.02f, .03f, .12f), .2f * on), 0, Seed);
                // cloud ceiling along the top edge, drifting: dark bellies with lit billows on top
                for (int k = 0; k < 11; k++)
                {
                    float x = a.xMin + ((k * .1f + now * .012f) % 1.1f) * a.width * 1.1f - a.width * .05f;
                    Vector2 p = new Vector2(x, a.yMax - .5f + Mathf.Sin(k * 1.9f + now * .35f) * .3f);
                    b.Quad(FxShape.Smoke, p, new Vector2(2f + .8f * H(Seed, k), 1.1f), 0, A(new Color(.1f, .1f, .2f), .85f * on), now * .05f + k, Seed + k);
                    b.Quad(FxShape.Smoke, p + new Vector2(.4f, .55f), new Vector2(1.3f, .8f), 0, A(new Color(.36f, .38f, .55f), .75f * on), now * .06f + k, Seed + k + 30);
                }
                // sheet lightning inside the clouds, a quick double flicker every few seconds, with a crawling arc
                float cycle = now / 3.1f; int n = Mathf.FloorToInt(cycle); float f = (cycle - n) * 3.1f;
                float sheet = Mathf.Max(Fade(f, 0, .12f), .7f * Fade(f - .2f, 0, .1f)) * (H(Seed, n) > .25f ? 1 : 0);
                if (sheet > 0)
                {
                    Vector2 p = new Vector2(a.xMin + a.width * H(Seed, n + 5), a.yMax - .3f);
                    b.Quad(FxShape.Glow, p, new Vector2(3.6f, 1.8f), 0, A(new Color(.7f, .8f, 1f), .6f * sheet * on), 0, Seed);
                    Bolt(b, p + Vector2.left * 2.5f, p + new Vector2(2.5f, -.4f), .05f, Cyan, Seed + n, .6f, .8f * sheet * on, 8);
                }
                // rain: thin slanted streaks falling and wrapping
                for (int k = 0; k < 90; k++)
                {
                    float sp = 14 + 6 * H(Seed, k + 300), u = (H(Seed, k) + now * sp / (a.height + 4)) % 1;
                    Vector2 p = new Vector2(a.xMin + a.width * H(Seed, k + 150) + (1 - u) * 1.6f, a.yMax + 2 - u * (a.height + 4));
                    b.Beam(p, p + new Vector2(-.18f, -.95f), .03f, A(Rain, .55f * on), 0, k, 0);
                }
                // splashes: tiny rings and a hop of droplets where drops hit the floor
                for (int k = 0; k < 28; k++)
                {
                    float u = (now * 1.7f + H(Seed, k + 600)) % 1; int gen = Mathf.FloorToInt(now * 1.7f + H(Seed, k + 600));
                    Vector2 p = new Vector2(a.xMin + a.width * H(Seed + gen, k + 610), a.yMin + a.height * .85f * H(Seed + gen, k + 620));
                    b.Floor(FxShape.Shock, p, .32f, A(Rain, .5f * on), u, Seed + k);
                    if (u < .4f) for (int d = 0; d < 2; d++)
                    {
                        float v = u / .4f; Vector2 q = p + new Vector2((d * 2 - 1) * .18f * v, .3f * v * (1 - v) * 4 * .25f);
                        Glint(b, q, .05f, Rain, .7f * on * (1 - v), 0);
                    }
                }
            }
        }
    }
}
