using System;
using System.Collections.Generic;
using UnityEngine;
using static ArcaneBrigade.FxKit;

namespace ArcaneBrigade
{
    /// <summary>Frost and ember strikes in the language of the falling star (requirement 1/11/2026: the old frost crystals and cel
    /// flames read as solid stickers over the painting): light, not bodies. Something arrives on a slant trailing light, lands in a
    /// short flash and a few thin pieces, and is gone within about a second. Sized to the target, never to the crowd.
    /// frost-flake = a spinning snowflake streaks in, a cold blue-white flash, thin ice slivers and diamond dust drifting up;
    /// ember-spark = a hot spark spirals in, one bright flare licks up, embers fly out on arcs and wink out as they fall.</summary>
    public static class ElementStrikeFx
    {
        public static void Register(Dictionary<string, Func<FxEffect>> map)
        {
            map["frost-flake"] = () => new FrostFlake();
            map["ember-spark"] = () => new EmberSpark();
        }

        private const FxShape Shard = (FxShape)22, Flake = (FxShape)26, Streak = (FxShape)39;
        private static readonly Color Ice = new Color(.55f, .85f, 1f), IceDeep = new Color(.3f, .6f, 1f), Frost = new Color(.85f, .97f, 1f);
        private static readonly Color Fire = new Color(1f, .45f, .12f), Hot = new Color(1f, .78f, .3f), Heat = new Color(1f, .95f, .7f);

        private static void Trail(FxBatch b, Vector2 head, Vector2 dir, float len, float width, Color c, int seed, float q)
            => b.Quad(Streak, head - dir * (.62f * len), new Vector2(len, width), Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg, c, q, seed, 0);

        /// <summary>A snowflake drops in from the upper right on a slant, spinning, trailing a thin icy streak; on landing a cold flash
        /// and a ring of light on the floor, six thin ice slivers spring out and melt, the flake hops once and fades turning, and diamond
        /// dust glitters upward.</summary>
        private sealed class FrostFlake : FxEffect
        {
            protected override float Life => 1.5f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Mathf.Min(Radius, 1.6f), size = .34f + .14f * r, tf = .28f;
                Vector2 start = at + new Vector2(4.2f, 6.2f), dir = (at - start).normalized;
                if (t < tf)
                {
                    float u = t / tf; Vector2 head = Vector2.Lerp(start, at + Vector2.up * .3f, u * u * (3 - 2 * u));
                    Trail(b, head, dir, 4.4f, .44f, A(Ice, .9f), Seed, now * 4);
                    Trail(b, head, dir, 2.8f, .16f, Frost, Seed + 1, now * 4);
                    b.Quad(FxShape.Glow, head, size * 2.4f, A(Ice, .5f), 0, Seed);
                    b.Quad(Flake, head, new Vector2(size, size), -t * 900, A(Frost, 1f), 0, Seed, 0);
                    return;
                }
                float tb = t - tf;
                // the landing: a cold flash, a soft ring of light on the floor, short rays
                b.Quad(FxShape.Glow, at + Vector2.up * .4f, r * 1.5f, A(Frost, .8f * Fade(tb, 0, .18f)), 0, Seed);
                b.Floor(FxShape.Glow, at, r * 1.25f, A(Ice, .55f * Fade(tb, 0, .9f)), 0, Seed);
                b.Floor(FxShape.Shock, at, r * (.5f + .8f * EaseOut3(tb / .35f)), A(Frost, .6f * Fade(tb, .05f, .4f)), tb / .4f, Seed);
                b.Quad(FxShape.Rays, at + Vector2.up * .35f, r * 1.4f, A(Ice, .65f * Fade(tb, 0, .45f)), tb, Seed, 6);
                // six thin slivers of ice spring out of the floor and melt (light, a hair of body)
                for (int k = 0; k < 6; k++)
                {
                    float u = tb / .55f; if (u >= 1) break;
                    Vector2 d = Dir(Seed, k, 6, .6f); float reach = r * (.45f + .35f * H(Seed, k + 5)) * EaseOut3(Mathf.Min(1, u * 2.2f));
                    float deg = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
                    b.Quad(Shard, at + d * reach * .55f + Vector2.up * .12f, new Vector2(reach, .09f + .05f * H(Seed, k)), deg, A(Ice, .9f * (1 - u)), 0, Seed + k, 0);
                }
                // the flake hops once, turns and fades
                float hop = tb < .4f ? .9f * 4 * (tb / .4f) * (1 - tb / .4f) : 0, life = Fade(tb, .55f, 1.05f);
                Vector2 p = at + Vector2.up * (.3f + hop);
                b.Quad(FxShape.Glow, p, size * 2f, A(Ice, .45f * life), 0, Seed);
                b.Quad(Flake, p, new Vector2(size, size) * (1 + .25f * Fade(tb, 0, .2f)), -tb * 260, A(Frost, life), 0, Seed, 0);
                // diamond dust: tiny glints rising and twinkling
                for (int k = 0; k < 12; k++)
                {
                    float born = .04f * k, u = (tb - born) / (.7f + .3f * H(Seed, k + 30)); if (u <= 0 || u >= 1) continue;
                    Vector2 d = Dir(Seed + 7, k, 12, .7f);
                    Vector2 q = at + d * r * (.3f + .8f * EaseOut(u)) + Vector2.up * (.25f + .9f * u);
                    float tw = .6f + .4f * Mathf.Sin(now * 18 + k * 2.1f);
                    Glint(b, q, .1f + .06f * H(Seed, k), k % 3 == 0 ? Frost : Ice, .95f * (1 - u) * tw, k * 40 + now * 90);
                }
            }
        }

        /// <summary>A hot spark spirals in from the upper left trailing fire; on landing a bright flash, one tall flare licks up and
        /// dies down, heat glows on the floor, and a dozen embers fly out on arcs, fall and wink out. No smoke.</summary>
        private sealed class EmberSpark : FxEffect
        {
            protected override float Life => 1.35f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Mathf.Min(Radius, 1.6f), size = .3f + .14f * r, tf = .26f;
                Vector2 start = at + new Vector2(-4.4f, 6f), dir = (at - start).normalized, side = new Vector2(-dir.y, dir.x);
                if (t < tf)
                {
                    float u = t / tf; Vector2 head = Vector2.Lerp(start, at + Vector2.up * .3f, u * u * (3 - 2 * u)) + side * Mathf.Sin(u * Mathf.PI * 2) * .35f * (1 - u);
                    Trail(b, head, dir, 4.6f, .5f, A(Fire, .9f), Seed, now * 5);
                    Trail(b, head, dir, 3f, .18f, Heat, Seed + 1, now * 5);
                    b.Quad(FxShape.Glow, head, size * 2.6f, A(Hot, .6f), 0, Seed);
                    Glint(b, head, size * 1.4f, Heat, 1f, now * 600);
                    return;
                }
                float tb = t - tf;
                b.Quad(FxShape.Glow, at + Vector2.up * .4f, r * 1.5f, A(Heat, .85f * Fade(tb, 0, .16f)), 0, Seed);
                b.Floor(FxShape.Glow, at, r * 1.3f, A(Fire, .6f * Fade(tb, 0, .85f)), 0, Seed);
                b.Floor(FxShape.Shock, at, r * (.5f + .8f * EaseOut3(tb / .3f)), A(Hot, .55f * Fade(tb, .04f, .35f)), tb / .35f, Seed);
                b.Quad(FxShape.Rays, at + Vector2.up * .35f, r * 1.4f, A(Hot, .6f * Fade(tb, 0, .4f)), tb, Seed, 7);
                // one flare licks up and dies down (the light flame, no painted body)
                float flare = Rise(tb, 0, .08f) * Fade(tb, .25f, .6f);
                if (flare > 0)
                {
                    float h = r * (1.1f + .5f * Fade(tb, 0, .35f));
                    b.Quad(FxShape.Flame, at + Vector2.up * h * .45f, new Vector2(r * .75f, h), 0, A(Fire, .9f * flare), now * 3, Seed);
                    b.Quad(FxShape.Flame, at + Vector2.up * h * .38f, new Vector2(r * .45f, h * .75f), 0, A(Heat, .9f * flare), now * 3 + 1, Seed + 1);
                }
                // embers fly out on arcs, fall, wink out
                for (int k = 0; k < 12; k++)
                {
                    float fly = .5f + .35f * H(Seed, k + 3), u = tb / fly; if (u >= 1) continue;
                    Vector2 d = Dir(Seed, k, 12, .7f); float reach = r * (.6f + .7f * H(Seed, k + 9));
                    Vector2 q = at + d * reach * EaseOut(u) + Vector2.up * (.3f + 1.6f * u * (1 - u) * (1 + H(Seed, k)));
                    float tw = .65f + .35f * Mathf.Sin(now * 22 + k * 1.7f);
                    b.Quad(FxShape.Glow, q, .26f, A(Fire, .55f * (1 - u)), 0, Seed + k);
                    Glint(b, q, .09f + .05f * H(Seed, k + 1), k % 2 == 0 ? Hot : Heat, (1 - u) * tw, k * 50 + now * 200);
                }
            }
        }
    }
}
