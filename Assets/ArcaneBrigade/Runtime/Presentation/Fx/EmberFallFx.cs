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
            protected override float Life => 2.0f;
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
                float tb = t - tf, hop = tb < .42f ? 1.4f * 4 * (tb / .42f) * (1 - tb / .42f) : 0, rest = Fade(tb, .9f, 1.5f);
                Vector2 p = at + Vector2.up * (.3f + hop);
                // the landing: a hot flash, warm light on the floor, a short ring of heat and rays
                b.Quad(FxShape.Glow, at + Vector2.up * .5f, r * 1.7f, A(Heat, .8f * Fade(tb, 0, .2f)), 0, Seed);
                b.Floor(FxShape.Glow, at, r * 1.3f, A(Fire, .5f * Fade(tb, 0, 1.3f)), 0, Seed);
                b.Floor(FxShape.Shock, at, r * (.5f + .9f * EaseOut3(tb / .38f)), A(Hot, .6f * Fade(tb, .05f, .45f)), tb / .45f, Seed);
                b.Quad(FxShape.Rays, at + Vector2.up * .3f, r * 1.6f, A(Hot, .7f * Fade(tb, 0, .55f)), tb, Seed, 3);
                // the fireball hops once and burns down
                Flamelet(b, p, size * 1.5f * (.6f + .4f * rest), now, Seed, rest);
                // little flames thrown out on arcs: each lands and flickers a moment, then winks out
                for (int k = 0; k < 9; k++)
                {
                    Vector2 d = Dir(Seed, k, 9, .75f); float fly = .5f + .2f * H(Seed, k + 3), u = tb / fly;
                    Vector2 land = at + d * r * (.7f + .65f * H(Seed, k + 9));
                    Vector2 q = u < 1 ? Vector2.Lerp(at, land, EaseOut(u)) + Vector2.up * (1.6f * u * (1 - u)) : land;
                    float life = Fade(tb, .95f + .06f * k, 1.55f + .06f * k);
                    if (life <= 0) continue;
                    float h = (.32f + .16f * H(Seed, k + 12)) * (u < 1 ? .8f : 1);
                    Flamelet(b, q, h, now, Seed + k * 7, life);
                    if (u >= 1) Glint(b, q + Vector2.up * h * .9f, .2f * (.6f + .4f * Mathf.Sin(now * 16 + k)), Heat, .8f * life, now * 60 + k * 70);
                }
                // embers sprayed up and falling back, winking
                for (int k = 0; k < 14; k++)
                {
                    float u = tb / (.55f + .5f * H(Seed, k + 40)); if (u <= 0 || u >= 1) continue;
                    Vector2 d = Dir(Seed + 3, k, 14, .8f);
                    Vector2 q = at + d * r * 1.6f * EaseOut(u) + Vector2.up * (1.1f * u - 1.5f * u * u + .3f);
                    Glint(b, q, .16f, k % 2 == 0 ? Hot : Heat, (1 - u) * (.6f + .4f * Mathf.Sin(now * 25 + k)), k * 50 + now * 150);
                }
            }
        }
    }
}
