using System;
using System.Collections.Generic;
using UnityEngine;
using static ArcaneBrigade.FxKit;

namespace ArcaneBrigade
{
    /// <summary>Convenience overload for worker B's effects: a rotated square shape with no free parameter.</summary>
    public static class FxQuadB
    {
        public static void Quad(this FxBatch b, FxShape s, Vector2 at, float r, float degrees, Color c, float progress, int seed) => b.Quad(s, at, new Vector2(r, r), degrees, c, progress, seed, 0);
    }

    /// <summary>Venom bond: green-purple bubbles, potion flasks, toxic gas and drips. Every rule has its own silhouette:
    /// rot = bubbles that swell out of the ground and pop beside dripping ooze; pandemic = a wall of fog sweeping the arena that
    /// leaves giant spore pods bursting; flask = a thrown potion bottle that shatters; cloud = a churning gas pool;
    /// plague mist = slow ground fog with drifting spores and a dripping ceiling.</summary>
    public static class VenomFx
    {
        public static void Register(Dictionary<string, Func<FxEffect>> map)
        {
            map["rot"] = () => new Rot();
            map["pandemic"] = () => new Pandemic();
            map["venom-flask"] = () => new Flask();
            map["venom-cloud"] = () => new Cloud();
            map["plague-mist"] = () => new Mist();
        }

        private const FxShape Bubble = (FxShape)30, Drop = (FxShape)32, CardS = (FxShape)38;
        private static readonly Color Toxic = new Color(.55f, 1f, .2f), Purple = new Color(.68f, .28f, .95f), Slime = new Color(.18f, .75f, .35f),
            Murk = new Color(.04f, .13f, .06f), Mauve = new Color(.16f, .05f, .22f), Glass = new Color(.7f, 1f, .85f);
        private static Color Pick(int seed, int i) => H(seed, i + 77) > .5f ? Toxic : Purple;
        private static Vector2 Arc(Vector2 a, Vector2 b, float h, float u) => Vector2.Lerp(a, b, u) + Vector2.up * (4 * h * u * (1 - u));

        /// <summary>A bubble that wobbles up from <paramref name="at"/> and pops into a spray of droplets at the end of its life.</summary>
        private static void PoppingBubble(FxBatch b, Vector2 at, float size, float u, Color c, int seed, float rise)
        {
            if (u <= 0 || u >= 1.45f) return;
            if (u < 1)
            {
                float grow = EaseOut(u * 3), wob = 1 + .08f * Mathf.Sin(u * 40 + seed);
                Vector2 p = at + new Vector2(Mathf.Sin(u * 7 + seed) * .22f, rise * u * (.5f + .5f * u));
                b.Quad(FxShape.Glow, p, size * 1.8f * grow, A(c, .3f), 0, seed);
                b.Quad(Bubble, p, new Vector2(size * grow * wob, size * grow / wob), 0, A(c, 1.1f), u, seed, 0);
                return;
            }
            float s = (u - 1) / .45f; Vector2 top = at + new Vector2(Mathf.Sin(7 + seed) * .22f, rise);
            b.Quad(FxShape.Glow, top, size * 2.4f, A(White, .6f * (1 - s)), 0, seed);
            for (int k = 0; k < 7; k++)
            {
                Vector2 d = Dir(seed, k, 7, .8f); Vector2 p = top + d * size * 2.3f * EaseOut(s) + Vector2.down * 1.3f * s * s;
                b.Quad(Drop, p, size * .5f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg - 90 + s * 120, A(c, 1.1f * (1 - s)), 0, seed + k);
            }
        }

        /// <summary>Trait on an enemy: the flesh rots. Sickly bubbles swell out of a spreading puddle of ooze and pop, drips fall from above.</summary>
        private sealed class Rot : FxEffect
        {
            protected override float Life => 1.9f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Radius * 1.15f;
                float on = Rise(t, 0, .2f), off = Fade(t, 1.1f, 1.9f);
                b.Floor(FxShape.Shade, at, r * 1.5f, A(Murk, .7f * on * off), 0, Seed);
                b.Floor(FxShape.Smoke, at, r * (.5f + .7f * EaseOut(t * 1.6f)), A(Slime, .75f * on * off), t * .5f, Seed);
                b.Floor(FxShape.Smoke, at + new Vector2(.3f, -.1f), r * (.35f + .6f * EaseOut(t * 1.3f)), A(Purple * .6f, .7f * on * off), t * .4f, Seed + 4);
                b.Floor(FxShape.Glow, at, r * 1.1f, A(Toxic, .3f * on * off), 0, Seed);
                // dark rot blotches crawling on the target
                for (int k = 0; k < 4; k++) b.Quad(FxShape.Smoke, at + new Vector2((H(Seed, k) - .5f) * r * .9f, .2f + H(Seed, k + 9) * .8f), .4f + .25f * H(Seed, k + 3), 0, A(Murk, .85f * on * off), t, Seed + k);
                for (int k = 0; k < 11; k++)
                {
                    Vector2 root = at + new Vector2((H(Seed, k + 20) - .5f) * 2 * r * .9f, (H(Seed, k + 40) - .5f) * r * .5f);
                    float u = (t - H(Seed, k + 60) * .8f) / (.75f + .35f * H(Seed, k + 80));
                    PoppingBubble(b, root, .3f + .26f * H(Seed, k + 100), u, Pick(Seed, k), Seed + k * 3, 1.5f + H(Seed, k + 5));
                }
                // drips: heavy droplets falling out of the sky into the puddle, each leaving a tiny splash
                for (int k = 0; k < 8; k++)
                {
                    float born = .1f + H(Seed, k + 120) * .8f, u = (t - born) / .55f; if (u <= 0 || u > 1.5f) continue;
                    Vector2 land = at + new Vector2((H(Seed, k + 130) - .5f) * 2 * r * .8f, (H(Seed, k + 140) - .5f) * r * .4f);
                    if (u < 1) b.Quad(Drop, land + Vector2.up * (4.2f * (1 - u * u)), .36f, 0, A(Pick(Seed, k + 3), 1.1f), 0, Seed + k);
                    else for (int s = 0; s < 4; s++) { float w = (u - 1) * 2; b.Quad(Drop, land + new Vector2((s - 1.5f) * .3f * w, .5f * w * (1 - w)), .17f, (s - 1.5f) * 40, A(Toxic, 1.1f * (1 - w)), 0, Seed + s); }
                }
            }
        }

        /// <summary>Trait (step 8): the pandemic. A towering wall of fog rolls over the whole arena; in its wake giant spore pods swell and burst
        /// into umbrellas of toxic droplets.</summary>
        private sealed class Pandemic : FxEffect
        {
            protected override float Life => 3.6f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Rect a = Arena; float sweep = 2.2f, w = Mathf.Clamp01(t / sweep), front = Mathf.Lerp(a.xMin - 3.5f, a.xMax + 3.5f, w * w * (3 - 2 * w));
                float outT = Fade(t, 2.6f, 3.6f);
                b.Quad(FxShape.Shade, a.center, new Vector2(a.width * .75f, a.height * .75f), 0, A(Mauve, .4f * Rise(t, 0, .5f) * outT), 0, Seed);
                for (int k = 0; k < 15; k++)
                {
                    float y = a.yMin - .5f + (k / 14f) * (a.height + 1), lag = H(Seed, k) * 3.6f, wob = Mathf.Sin(k * 1.7f + now * 3) * .35f;
                    Vector2 p = new Vector2(front - lag + wob, y);
                    float trail = Mathf.Clamp01(1 - (lag * .18f)) * outT;
                    b.Quad(FxShape.Smoke, p, new Vector2(2.9f + H(Seed, k + 9), 1.9f), 0, A(k % 2 == 0 ? Slime : Mauve * 2.2f, .95f * trail), now * .15f + k, Seed + k);
                    b.Quad(FxShape.Glow, p + Vector2.right * .6f, 1.5f, A(k % 3 == 0 ? Purple : Toxic, .3f * trail), 0, Seed);
                }
                // a sickly haze lingers in the wake of the wall, and a bright toxic edge leads it
                for (int k = 0; k < 7; k++)
                {
                    float x = front - 2.2f - k * 3.4f; if (x < a.xMin - 3) break;
                    b.Quad(FxShape.Glow, new Vector2(x, a.center.y + Mathf.Sin(k * 2.1f) * 1.5f), new Vector2(3.4f, a.height * .55f), 0, A(k % 2 == 0 ? Slime : Purple, .2f * outT), 0, Seed);
                }
                for (int k = 0; k < 9; k++)
                    b.Quad(FxShape.Glow, new Vector2(front + .9f, a.yMin + a.height * (k + .5f) / 9f), new Vector2(.9f, 1.1f), 0, A(Toxic, .55f * outT * Fade(t, sweep, sweep + .4f)), 0, Seed);
                // spore pods in the wake of the wall
                for (int k = 0; k < 14; k++)
                {
                    Vector2 p = new Vector2(a.xMin + 1 + (a.width - 2) * H(Seed, k + 20), a.yMin + 1 + (a.height - 2) * H(Seed, k + 40));
                    float born = (p.x - a.xMin + 3.5f) / (a.width + 7) * sweep + .1f, u = (t - born) / 1.0f;
                    if (u <= 0 || u > 1.5f) continue;
                    Color c = Pick(Seed, k);
                    if (u < 1)
                    {
                        float grow = EaseOut(u * 1.4f), wob = 1 + .1f * Mathf.Sin(u * 30 + k);
                        b.Quad(FxShape.Glow, p, 1.9f * grow, A(c, .3f * outT), 0, Seed);
                        b.Quad(Bubble, p + Vector2.up * u * .5f, new Vector2(1.3f * grow * wob, 1.3f * grow / wob), 0, A(c, 1.1f * outT), u, Seed + k, 0);
                    }
                    else
                    {
                        float s = (u - 1) / .5f;
                        b.Quad(FxShape.Glow, p, 1.5f * (1 - s), A(White, .6f * (1 - s)), 0, Seed);
                        b.Floor(FxShape.Smoke, p, 1.3f * EaseOut(s), A(c * .8f, .65f * (1 - s)), s, Seed + k);
                        for (int d = 0; d < 10; d++)
                        {
                            Vector2 dir = Dir(Seed + k, d, 10, .9f); float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
                            b.Quad(Drop, p + dir * 1.7f * EaseOut(s) + Vector2.up * (.7f * s * (1 - s)), .3f, ang + s * 160, A(c, 1.1f * (1 - s)), 0, Seed + d);
                        }
                    }
                }
            }
        }

        /// <summary>Step 6: a potion flask is thrown on a high arc, spinning and dribbling, and shatters over the crowd: glass shards, a
        /// splash of liquid, and a mushroom of toxic gas.</summary>
        private sealed class Flask : FxEffect
        {
            protected override float Life => 2.0f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Radius;
                if (t < 0)
                {
                    float u = pre; Vector2 p = Arc(From, at, 3.6f, u);
                    b.Floor(FxShape.Shade, at, r * 1.1f * u, A(Murk, .45f * u), 0, Seed);
                    for (int k = 1; k <= 7; k++)
                    {
                        float back = Mathf.Max(0, u - k * .035f); Vector2 q = Arc(From, at, 3.6f, back) + new Vector2(0, -.05f * k);
                        b.Quad(Drop, q, .22f * (1 - k / 8f), 180, A(Toxic, 1f - k / 8f), 0, Seed + k);
                    }
                    b.Quad(FxShape.Glow, p, .9f, A(Toxic, .55f), 0, Seed);
                    b.Quad(Drop, p, .62f, u * 760, A(Glass, 1.2f), u, Seed, 1);
                    return;
                }
                float flash = Fade(t, 0, .25f);
                b.Quad(FxShape.Glow, at + Vector2.up * .4f, r * 1.8f, A(Toxic, .8f * flash), 0, Seed);
                b.Floor(FxShape.Smoke, at, r * (.5f + .7f * EaseOut(t * 2.2f)), A(Slime, .75f * Fade(t, .7f, 1.9f)), t, Seed);
                b.Floor(FxShape.Glow, at, r * 1.3f, A(Toxic, .35f * Fade(t, .2f, 1.4f)), 0, Seed);
                b.Floor(FxShape.Smoke, at, r * 1.2f, A(Mauve * 1.5f, .45f * Fade(t, .5f, 1.9f)), t * .4f, Seed + 3);
                // glass shards tumble out with gravity
                for (int k = 0; k < 14; k++)
                {
                    Vector2 d = Dir(Seed, k, 14, .8f); float life = .7f + .3f * H(Seed, k + 5), u = t / life; if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + Vector2.up * .5f + d * r * 1.15f * EaseOut(u) + Vector2.up * (2.2f * H(Seed, k + 9) * u - 3.4f * u * u);
                    b.Quad(CardS, p, new Vector2(.18f + .1f * H(Seed, k + 2), .08f), t * 600 + k * 40, A(Glass, 1.1f * (1 - u * u)), 0, Seed + k, 0);
                }
                // liquid spray
                for (int k = 0; k < 24; k++)
                {
                    Vector2 d = Dir(Seed + 7, k, 24, .75f); float life = .55f + .5f * H(Seed, k + 15), u = t / life; if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + d * r * (.5f + .75f * H(Seed, k + 25)) * EaseOut(u) + Vector2.up * (2.6f * u * (1 - u) + .3f);
                    float ang = (k % 2 == 0 ? 1 : -1) * 25 + Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg - 90;
                    b.Quad(Drop, p, .3f + .12f * H(Seed, k + 3), ang, A(k % 4 == 0 ? Purple : Toxic, 1.1f * (1 - u * u)), 0, Seed + k);
                }
                // the mushroom of gas
                for (int k = 0; k < 7; k++)
                {
                    float born = k * .06f, u = Mathf.Clamp01((t - born) / 1.2f); if (u <= 0 || u >= 1) continue;
                    float ang = k * 0.9f; Vector2 p = at + new Vector2(Mathf.Cos(ang) * r * .5f * u, 1.2f * EaseOut(u) + (k < 3 ? 0 : 1) * .9f * u + Mathf.Sin(ang) * .3f * u);
                    b.Quad(FxShape.Smoke, p, r * (.38f + .5f * u), A(k % 2 == 0 ? Slime : Purple * .8f, .75f * (1 - u)), u, Seed + k);
                }
                for (int k = 0; k < 5; k++) PoppingBubble(b, at + new Vector2((H(Seed, k + 60) - .5f) * r * 1.4f, 0), .3f + .15f * H(Seed, k), (t - .2f - k * .1f) / .8f, Pick(Seed, k), Seed + k, 1.6f);
            }
        }

        /// <summary>Step 6 zone: the flask's gas pool. Puffs of fog churn around inside the footprint, bubbles rise and pop from the
        /// brew, and plumes of gas curl up and away.</summary>
        private sealed class Cloud : FxEffect
        {
            protected override float Life => Moment.Duration + .6f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Radius, dur = Moment.Duration, on = Rise(t, 0, .45f) * Fade(t, dur - .1f, dur + .55f);
                b.Floor(FxShape.Shade, at, r * 1.2f, A(Murk, .7f * on), 0, Seed);
                b.Floor(FxShape.Glow, at, r * 1.1f, A(Toxic, .35f * on * (.75f + .25f * Mathf.Sin(now * 5))), 0, Seed);
                for (int k = 0; k < 11; k++)
                {
                    float ang = now * (k % 2 == 0 ? .7f : -.5f) + k * 0.57f, rad = r * (.25f + .6f * H(Seed, k));
                    Vector2 p = at + new Vector2(Mathf.Cos(ang) * rad, Mathf.Sin(ang) * rad * .62f + .35f);
                    b.Quad(FxShape.Smoke, p, r * (.45f + .2f * H(Seed, k + 7)), 0, A(k % 3 == 0 ? Purple * .75f : Slime, .75f * on), now * .2f + k, Seed + k);
                }
                for (int k = 0; k < 7; k++)
                {
                    float u = (now * .45f + H(Seed, k + 30)) % 1; Vector2 p = at + new Vector2((H(Seed, k + 40) - .5f) * r * 1.3f, .2f) + new Vector2(Mathf.Sin(u * 5 + k) * .4f, u * 3.2f);
                    b.Quad(FxShape.Smoke, p, r * (.3f + .35f * u), A(Slime, .55f * on * (1 - u)), u, Seed + k + 20);
                }
                for (int k = 0; k < 8; k++)
                {
                    float cyc = (now * .6f + H(Seed, k + 50) * 3) / (1.1f + .4f * H(Seed, k + 51));
                    int n = Mathf.FloorToInt(cyc); float u = (cyc - n) * 1.45f;
                    Vector2 root = at + new Vector2((H(Seed + n, k) - .5f) * 2 * r * .85f, (H(Seed + n, k + 9) - .5f) * r * .4f);
                    PoppingBubble(b, root, (.3f + .24f * H(Seed + n, k + 4)) * on, u, Pick(Seed + n, k), Seed + n * 7 + k, 1.4f);
                }
                for (int k = 0; k < 10; k++)
                {
                    float u = (now * .8f + H(Seed, k + 70)) % 1; Vector2 p = at + new Vector2((H(Seed, k + 80) - .5f) * r * 1.6f, u * 2.5f);
                    Glint(b, p, .22f, Toxic, .9f * on * Mathf.Sin(u * Mathf.PI), k * 33 + now * 80);
                }
            }
        }

        /// <summary>Step 8 weather: plague mist. Heavy green-violet fog banks crawl along the floor, spores glitter upward and tall
        /// bubbles drift by while droplets drip off the top of the screen.</summary>
        private sealed class Mist : FxEffect
        {
            protected override float Life => 3;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Rect a = Arena; float on = Rise(t, 0, 2.5f) * Fade(now, End - 1.5f, End);
                b.Quad(FxShape.Shade, a.center, new Vector2(a.width * .75f, a.height * .75f), 0, A(Murk, .4f * on), 0, Seed);
                for (int k = 0; k < 14; k++)
                {
                    float x = a.xMin + (((k * .11f + now * (.018f + .006f * (k % 3))) % 1.3f) - .15f) * a.width;
                    Vector2 p = new Vector2(x, a.yMin + a.height * (.12f + .76f * H(Seed, k)) + Mathf.Sin(now * .5f + k) * .4f);
                    b.Quad(FxShape.Smoke, p, new Vector2(4.4f + 1.4f * H(Seed, k + 9), 2.3f), 0, A(k % 3 == 0 ? Mauve * 2.2f : Slime, .6f * on), now * .05f + k, Seed + k);
                }
                b.Quad(FxShape.Glow, a.center, new Vector2(a.width * .6f, a.height * .55f), 0, A(Toxic, .09f * on), 0, Seed);
                for (int k = 0; k < 14; k++)
                {
                    float u = (now * (.08f + .04f * H(Seed, k)) + H(Seed, k + 20)) % 1;
                    Vector2 p = new Vector2(a.xMin + a.width * H(Seed, k + 30) + Mathf.Sin(now * .8f + k) * .5f, a.yMin - .5f + u * (a.height + 1));
                    float s = .28f + .3f * H(Seed, k + 40);
                    b.Quad(Bubble, p, s, 0, A(Pick(Seed, k), .9f * on), now, Seed + k, 0);
                }
                for (int k = 0; k < 45; k++)
                {
                    float u = (now * (.12f + .1f * H(Seed, k)) + H(Seed, k + 90)) % 1;
                    Vector2 p = new Vector2(a.xMin + a.width * H(Seed, k + 100) + Mathf.Sin(now + k) * .6f, a.yMin + u * a.height);
                    Glint(b, p, .16f, k % 3 == 0 ? Purple : Toxic, .85f * on * Mathf.Sin(u * Mathf.PI), now * 60 + k * 20);
                }
                for (int k = 0; k < 12; k++)
                {
                    float sp = 5 + 3 * H(Seed, k + 120), u = (H(Seed, k) + now * sp / (a.height + 3) * .5f) % 1;
                    Vector2 p = new Vector2(a.xMin + a.width * H(Seed, k + 130), a.yMax + 1 - u * (a.height + 2));
                    b.Quad(Drop, p, .25f, 0, A(Pick(Seed, k), .9f * on), 0, Seed + k);
                }
            }
        }
    }
}
