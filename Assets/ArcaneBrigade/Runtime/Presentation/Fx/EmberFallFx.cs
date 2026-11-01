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
            protected override float Life => 1.0f;
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
                // the landing: a hot flash, warm light on the floor, and the fireball burning down where it struck
                b.Quad(FxShape.Glow, at + Vector2.up * .5f, r * 1.6f, A(Heat, .8f * Fade(tb, 0, .22f)), 0, Seed);
                b.Floor(FxShape.Glow, at, r * 1.2f, A(Fire, .5f * Fade(tb, 0, .8f)), 0, Seed);
                float burn = Fade(tb, .15f, .6f);
                Flamelet(b, at + Vector2.up * .2f, size * 1.4f * (.5f + .5f * burn), now, Seed, burn);
                // a few sparks flung out from the strike, each a short bright streak that slows, drops and goes out
                for (int k = 0; k < 7; k++)
                {
                    float fly = .32f + .14f * H(Seed, k + 3), u = tb / fly; if (u >= 1) continue;
                    Vector2 d = Dir(Seed, k, 7, .6f); d.y = d.y * .6f + .25f; d.Normalize();
                    float go = r * (1.1f + .5f * H(Seed, k + 9)) * EaseOut(u);
                    Vector2 q = at + Vector2.up * .3f + d * go + Vector2.down * (.9f * u * u);
                    Vector2 v = (d * (1 - u) + Vector2.down * 1.2f * u).normalized;
                    Trail(b, q, v, .5f * (1 - .6f * u), .09f, A(k % 2 == 0 ? Hot : Heat, 1 - u), Seed + k, now * 5);
                }
            }
        }
    }
}
