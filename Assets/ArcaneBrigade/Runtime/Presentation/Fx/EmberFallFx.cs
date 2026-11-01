using System;
using System.Collections.Generic;
using UnityEngine;
using static ArcaneBrigade.FxKit;

namespace ArcaneBrigade
{
    /// <summary>The falling star's fiery cousin (requirement 1/11/2026): a fireball with a shape of its own (a white-hot core, an
    /// orange body, a crimson rim crowned with flame tongues) streaks down at the front of its trail, splits the floor in molten
    /// cracks that cool from white through orange to red, burns down where it struck and throws sparks out on random volcano arcs.
    /// Light only, no smoke or solid bodies.</summary>
    public static class EmberFallFx
    {
        public static void Register(Dictionary<string, Func<FxEffect>> map) => map["ember-fall"] = () => new EmberFall();

        private const FxShape Streak = (FxShape)39;
        private static readonly Color Fire = new Color(1f, .42f, .1f), Hot = new Color(1f, .7f, .22f), Heat = new Color(1f, .93f, .66f), Deep = new Color(.95f, .22f, .08f);

        /// <summary>A streak whose front edge is at the fireball (the streak's glow sits behind its front: the fireball leads).</summary>
        private static void Trail(FxBatch b, Vector2 head, Vector2 dir, float len, float width, Color c, int seed, float q)
            => b.Quad(Streak, head - dir * (.97f * len), new Vector2(len, width), Deg(dir), c, q, seed, 0);

        private static float Deg(Vector2 d) => Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;

        /// <summary>The fireball's own shape, as the falling star has its five points: a white-hot core in an orange body inside a
        /// crimson rim, crowned with flame tongues that stream back along <paramref name="back"/>, flickering.</summary>
        private static void Fireball(FxBatch b, Vector2 p, Vector2 back, float s, float now, int seed, float alpha)
        {
            if (alpha <= .01f) return;
            b.Quad(FxShape.Glow, p, s * 3.2f, A(Deep, .55f * alpha), 0, seed);
            for (int k = 0; k < 7; k++)
            {
                float side = (k - 3) / 3f, flick = .8f + .2f * Mathf.Sin(now * 23 + k * 1.9f + seed);
                Vector2 d = Quaternion.Euler(0, 0, side * 26) * back;
                float len = s * (1.2f + 1.6f * (1 - Mathf.Abs(side))) * flick;
                b.Quad(FxShape.Flame, p + d * len * .8f, new Vector2(s * .45f, len), Deg(d) - 90, A(k % 2 == 0 ? Deep : Fire, .95f * alpha), now * 3 + k, seed + k);
            }
            for (int k = 0; k < 3; k++)
            {
                Vector2 d = Quaternion.Euler(0, 0, (k - 1) * 12) * back;
                float len = s * (1.6f - .4f * Mathf.Abs(k - 1)) * (.85f + .15f * Mathf.Sin(now * 29 + k));
                b.Quad(FxShape.Flame, p + d * len * .8f, new Vector2(s * .3f, len), Deg(d) - 90, A(Hot, alpha), now * 3 + k + 7, seed + k + 7);
            }
            b.Quad(FxShape.Glow, p, s * 1.5f, A(Hot, .9f * alpha), 0, seed);
            b.Quad(FxShape.Glow, p, s * .75f, A(White, .85f * alpha), 0, seed);
        }

        /// <summary>A crack of molten light running out across the floor from the strike: it shoots out white-hot and cools through
        /// orange to dull red before it goes out (<paramref name="cool"/> 0..1). One short side crack branches from it.</summary>
        private static void Crack(FxBatch b, Vector2 at, Vector2 d, float len, float grow, float cool, float alpha, int seed, bool branch = true)
        {
            if (alpha <= .01f || grow <= 0) return;
            const int n = 5;
            Vector2 side = new Vector2(-d.y, d.x * .62f), prev = at;
            Color hot = cool < .5f ? Color.Lerp(Heat, Fire, cool * 2) : Color.Lerp(Fire, Deep, cool * 2 - 1), halo = Color.Lerp(Fire, Deep, cool);
            for (int i = 1; i <= n; i++)
            {
                float u = i / (float)n, part = Mathf.Clamp01((grow - (u - 1f / n)) * n);
                if (part <= 0) break;
                Vector2 q = at + d * len * u + side * (H(seed, i) - .5f) * len * .24f * (i < n ? 1 : .4f);
                q = Vector2.Lerp(prev, q, part);
                float w = .055f * (1 - u * .55f); alpha *= 1 - .4f * cool * u;
                b.Link(prev, q, w * 3.4f, A(halo, .25f * alpha), seed + i, 1);
                b.Link(prev, q, w, A(hot, alpha), seed + i, .6f);
                if (cool < .35f) b.Link(prev, q, w * .4f, A(White, alpha * (1 - cool / .35f)), seed + i, .9f);
                if (branch && i == 2 && part >= 1)
                {
                    Vector2 bd = ((Vector2)(Quaternion.Euler(0, 0, H(seed, 9) < .5f ? 38 : -38) * d)).normalized;
                    Crack(b, q, bd, len * .35f, (grow - .4f) / .6f, cool, alpha * .85f, seed + 31, false);
                }
                prev = q;
            }
        }

        private sealed class EmberFall : FxEffect
        {
            protected override float Life => 1.7f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Radius, tf = .32f, size = .4f + .16f * r;
                Vector2 start = at + new Vector2(4.6f, 6.6f), dir = (at - start).normalized;
                if (t < tf)
                {
                    float u = t / tf; Vector2 head = Vector2.Lerp(start, at + Vector2.up * .3f, u);
                    Trail(b, head, dir, 5.2f, .7f, A(Deep, .9f), Seed, now * 5);
                    Trail(b, head, dir, 3.8f, .36f, Fire, Seed + 1, now * 5);
                    Trail(b, head, dir, 2.4f, .14f, Heat, Seed + 2, now * 5);
                    Fireball(b, head, -dir, size, now, Seed, 1);
                    return;
                }
                float tb = t - tf;
                // the strike: a white-hot flash, a broad pool of firelight, rays
                b.Quad(FxShape.Glow, at + Vector2.up * .5f, r * 2.2f, A(White, .4f * Fade(tb, 0, .14f)), 0, Seed);
                b.Quad(FxShape.Glow, at + Vector2.up * .5f, r * 2.6f, A(Hot, .6f * Fade(tb, 0, .35f)), 0, Seed + 1);
                b.Floor(FxShape.Glow, at, r * 2.4f, A(Fire, .5f * Fade(tb, 0, 1.1f)), 0, Seed);
                b.Quad(FxShape.Rays, at + Vector2.up * .4f, r * 2.2f, A(Hot, .65f * Fade(tb, 0, .4f)), tb, Seed, 3);
                // the floor splits in molten cracks that run out from the strike and cool from white through orange to dull red
                float grow = EaseOut3(tb / .16f), cool = Mathf.Clamp01((tb - .1f) / 1.05f), cracks = Fade(tb, .6f, 1.15f);
                for (int k = 0; k < 7; k++)
                {
                    float ang = (k + .7f * H(Seed, k + 20)) / 7f * Mathf.PI * 2;
                    Crack(b, at, new Vector2(Mathf.Cos(ang), Mathf.Sin(ang) * .62f), r * (1.1f + .8f * H(Seed, k + 21)), grow, cool, cracks, Seed + k * 13);
                }
                // the fireball stays where it struck as a fire burning down, its tongues now standing up
                float burn = Fade(tb, .25f, 1.2f), flare = 1 + .25f * Fade(tb, 0, .25f);
                Fireball(b, at + Vector2.up * .35f, Vector2.up, size * 1.25f * flare * (.45f + .55f * burn), now, Seed, burn);
                // sparks thrown out like a volcano: each at its own random bearing, height and reach, cooling from yellow to red as it falls
                for (int k = 0; k < 12; k++)
                {
                    float fly = .45f + .4f * H(Seed, k + 3), u = (tb - .04f * H(Seed, k + 4)) / fly;
                    if (u <= 0 || u >= 1.25f) continue;
                    float ang = H(Seed, k + 5) * Mathf.PI * 2, reach = r * (.9f + 1.6f * H(Seed, k + 6)), high = .9f + 2.2f * H(Seed, k + 7);
                    Vector2 d = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang) * .55f);
                    if (u >= 1) { Glint(b, at + d * reach, .2f, Fire, 1 - (u - 1) / .25f, now * 90 + k * 40); continue; } // lands and winks out
                    Vector2 q = at + Vector2.up * .3f + d * reach * u + Vector2.up * high * 4 * u * (1 - u);
                    Vector2 v = d * reach + Vector2.up * high * 4 * (1 - 2 * u);
                    Trail(b, q, v.normalized, Mathf.Min(.9f, v.magnitude * .12f), .1f, A(Color.Lerp(Heat, Deep, u * .8f), .95f), Seed + k, now * 5);
                    Glint(b, q, .14f, Heat, .9f * (1 - u * .6f), now * 200 + k * 40);
                }
            }
        }
    }
}
