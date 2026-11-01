using System;
using System.Collections.Generic;
using UnityEngine;
using static ArcaneBrigade.FxKit;

namespace ArcaneBrigade
{
    /// <summary>The falling star's fiery cousin (requirement 1/11/2026): a fireball streaks down at the front of its own trail of
    /// flame, lands in a hot flash, hops once, and throws little flames out on arcs that land around it and flicker out among
    /// winking embers, as the falling star scatters its little stars. Light only, no smoke or solid bodies.</summary>
    public static class EmberFallFx
    {
        public static void Register(Dictionary<string, Func<FxEffect>> map) => map["ember-fall"] = () => new EmberFall();

        private const FxShape Streak = (FxShape)39;
        private static readonly Color Fire = new Color(1f, .42f, .1f), Hot = new Color(1f, .7f, .22f), Heat = new Color(1f, .93f, .66f), Deep = new Color(.95f, .22f, .08f);

        /// <summary>A streak whose front edge is at the fireball (the streak's glow sits behind its front: the fireball leads).</summary>
        private static void Trail(FxBatch b, Vector2 head, Vector2 dir, float len, float width, Color c, int seed, float q)
            => b.Quad(Streak, head - dir * (.97f * len), new Vector2(len, width), Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg, c, q, seed, 0);

        /// <summary>A small flame of light standing on <paramref name="p"/>, flickering.</summary>
        private static void Flamelet(FxBatch b, Vector2 p, float h, float now, int seed, float alpha)
        {
            if (alpha <= .01f) return;
            float flick = .82f + .18f * Mathf.Sin(now * 19 + seed * 1.7f);
            b.Quad(FxShape.Glow, p + Vector2.up * h * .3f, h * 1.6f, A(Fire, .45f * alpha), 0, seed);
            b.Quad(FxShape.Flame, p + Vector2.up * h * .45f, new Vector2(h * .42f, h * flick), 0, A(Fire, .95f * alpha), now * 3, seed);
            b.Quad(FxShape.Flame, p + Vector2.up * h * .36f, new Vector2(h * .24f, h * .66f * flick), 0, A(Heat, .95f * alpha), now * 3 + 1, seed + 1);
        }

        private sealed class EmberFall : FxEffect
        {
            protected override float Life => 1.6f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Radius, tf = .32f, size = .46f + .2f * r;
                Vector2 start = at + new Vector2(4.6f, 6.6f), dir = (at - start).normalized;
                if (t < tf)
                {
                    float u = t / tf; Vector2 head = Vector2.Lerp(start, at + Vector2.up * .3f, u);
                    Trail(b, head, dir, 5.2f, .7f, A(Deep, .9f), Seed, now * 5);
                    Trail(b, head, dir, 3.6f, .3f, Hot, Seed + 1, now * 5);
                    b.Quad(FxShape.Glow, head, size * 2.8f, A(Fire, .6f), 0, Seed);
                    Flamelet(b, head + Vector2.down * size * .4f, size * 1.4f, now, Seed, 1);
                    Glint(b, head, size * 1.1f, Heat, 1f, now * 500);
                    return;
                }
                float tb = t - tf;
                // the landing, wide: a big hot flash, a broad pool of firelight on the floor, rays, and a burst of flame tongues
                b.Quad(FxShape.Glow, at + Vector2.up * .6f, r * 2.6f, A(Heat, .85f * Fade(tb, 0, .25f)), 0, Seed);
                b.Floor(FxShape.Glow, at, r * 2.4f, A(Fire, .55f * Fade(tb, 0, .9f)), 0, Seed);
                b.Floor(FxShape.Glow, at, r * 1.3f, A(Hot, .5f * Fade(tb, 0, .45f)), 0, Seed + 1);
                b.Quad(FxShape.Rays, at + Vector2.up * .4f, r * 2.4f, A(Hot, .75f * Fade(tb, 0, .4f)), tb, Seed, 3);
                for (int k = 0; k < 6; k++)
                {
                    float life = Fade(tb, .05f * H(Seed, k + 60), .35f + .25f * H(Seed, k + 61)); if (life <= 0) continue;
                    Vector2 q = at + Dir(Seed, k, 6, .5f) * r * (.25f + .45f * H(Seed, k + 62));
                    Flamelet(b, q, r * (.7f + .5f * H(Seed, k + 63)) * (.6f + .4f * EaseOut(tb / .2f)), now, Seed + k * 5, life);
                }
                float burn = Fade(tb, .2f, .8f);
                Flamelet(b, at + Vector2.up * .2f, size * 1.8f * (.5f + .5f * burn), now, Seed, burn);
                // sparks thrown out like a volcano: each at its own random bearing, height and reach, rising on an arc and falling back
                for (int k = 0; k < 14; k++)
                {
                    float fly = .45f + .4f * H(Seed, k + 3), u = (tb - .04f * H(Seed, k + 4)) / fly;
                    if (u <= 0 || u >= 1.25f) continue;
                    float ang = H(Seed, k + 5) * Mathf.PI * 2, reach = r * (.9f + 1.6f * H(Seed, k + 6)), high = .9f + 2.2f * H(Seed, k + 7);
                    Vector2 d = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang) * .55f);
                    Color c = H(Seed, k + 8) < .5f ? Hot : Heat;
                    if (u >= 1) { Glint(b, at + d * reach, .22f, c, 1 - (u - 1) / .25f, now * 90 + k * 40); continue; } // lands and winks out
                    Vector2 q = at + Vector2.up * .3f + d * reach * u + Vector2.up * high * 4 * u * (1 - u);
                    Vector2 v = d * reach + Vector2.up * high * 4 * (1 - 2 * u);
                    Trail(b, q, v.normalized, Mathf.Min(.9f, v.magnitude * .12f), .11f, A(c, .95f), Seed + k, now * 5);
                    b.Quad(FxShape.Glow, q, .32f, A(Fire, .6f), 0, Seed + k);
                }
            }
        }
    }
}
