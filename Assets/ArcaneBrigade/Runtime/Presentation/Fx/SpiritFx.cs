using System;
using System.Collections.Generic;
using UnityEngine;
using static ArcaneBrigade.FxKit;

namespace ArcaneBrigade
{
    /// <summary>Spirit bond: ghost wisps, pale teal souls, a reaper's scythe arc.</summary>
    public static class SpiritFx
    {
        public static void Register(Dictionary<string, Func<FxEffect>> map)
        {
            map["spirit-swarm"] = () => new Swarm();
            map["spirit-reaper"] = () => new Reaper();
        }

        /// <summary>A wisp: a pale teal ghost with a wavy tail and hollow eyes, pointing along <paramref name="dir"/>.</summary>
        internal static void Ghost(FxBatch b, Vector2 at, Vector2 dir, float size, float alpha, int seed, float now)
        {
            if (alpha < .004f) return;
            b.Quad(FxShape.Glow, at, size * 1.3f, A(DKit.Teal, .35f * alpha), 0, seed);
            b.Quad(DKit.Wisp, at - dir.normalized * size * .45f, new Vector2(size * 1.5f, size * 1.1f), DKit.Deg(dir), A(DKit.Ghost, 1.15f * alpha), now, seed, 0);
        }

        /// <summary>Three-to-five wisps stream off the hero in a loose corkscrew, dive into the crowd in a shrinking spiral, then the souls
        /// shoot up out of it as a pale teal mist ring spreads over the floor.</summary>
        private sealed class Swarm : FxEffect
        {
            protected override float Life => 1.2f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At, from = From; float r = Mathf.Max(1.2f, Radius) * 1.15f;
                const int N = 6;
                if (t < 0)
                {
                    for (int k = 0; k < N; k++)
                    {
                        float lag = k * .06f, u = Mathf.Clamp01(pre - lag * .6f) ; if (u <= 0) continue;
                        Vector2 d = (at - from); Vector2 n = new Vector2(-d.y, d.x).normalized;
                        Vector2 P(float v) => DKit.Arc(from, at, v, 1.6f + .4f * k) + n * Mathf.Sin(v * 11 + k * 1.7f) * (1 - v) * (.5f + .15f * k);
                        Vector2 p = P(u), q = P(Mathf.Max(0, u - .04f));
                        Ghost(b, p, p - q + Vector2.right * .001f, .62f, 1, Seed + k, now);
                        for (int s = 1; s <= 4; s++) { Vector2 tp = P(Mathf.Max(0, u - s * .05f)); Glint(b, tp, .16f * (1 - s / 5f), DKit.Teal, .8f * (1 - s / 5f), s * 40); }
                    }
                    b.Floor(FxShape.Glow, at, r * .9f * pre, A(DKit.Teal, .25f * pre), 0, Seed);
                    return;
                }
                float dive = Rise(t, 0, .42f);
                for (int k = 0; k < N; k++)
                {
                    float a0 = k / (float)N * 6.2832f + t * 9;
                    if (t < .42f)
                    {
                        float rad = r * (1 - EaseOut(dive)) * .95f + .05f; Vector2 p = at + new Vector2(Mathf.Cos(a0) * rad, Mathf.Sin(a0) * rad * .62f) + Vector2.up * (1.3f * (1 - dive));
                        Vector2 tang = new Vector2(-Mathf.Sin(a0), Mathf.Cos(a0) * .62f);
                        Ghost(b, p, tang - Vector2.up * dive * .6f, .62f * (1 - dive * .35f), 1, Seed + k, now);
                    }
                    else
                    {
                        float u = Rise(t, .42f, 1.1f); Vector2 p = at + new Vector2((H(Seed, k) - .5f) * r * .9f, 0) + Vector2.up * (EaseOut(u) * (2.8f + H(Seed, k + 5) * 1.5f));
                        Ghost(b, p, Vector2.up + Vector2.right * Mathf.Sin(k) * .4f, .6f + .2f * u, Fade(u, .45f, 1), Seed + k, now);
                    }
                }
                float ring = Rise(t, .38f, 1.1f);
                b.Floor(FxShape.Shock, at, r * 1.7f, A(DKit.Teal, .9f), ring, Seed);
                b.Quad(FxShape.Smoke, at, DKit.Sq(r * 1.3f), 0, A(new Color(.35f, .8f, .72f), .5f * Mathf.Sin(Mathf.Clamp01(ring) * Mathf.PI)), ring, Seed + 3);
                b.Quad(FxShape.Glow, at, r * 1.5f, A(DKit.Ghost, .55f * Fade(t, .38f, .75f)), 0, Seed);
                for (int k = 0; k < 14; k++)
                {
                    float u = Rise(t, .4f + .02f * k, 1.15f); if (u <= 0 || u >= 1) continue;
                    Glint(b, at + new Vector2((H(Seed, k + 40) - .5f) * r * 1.8f, (H(Seed, k + 60) - .5f) * r * .8f) + Vector2.up * u * 2.4f, .22f, DKit.Ghost, 1 - u, k * 40);
                }
            }
        }

        /// <summary>A hooded reaper rises over the crowd, winds up, then its scythe cuts a huge glowing arc through everything; torn souls
        /// fly up out of the crowd and a cold mist lies on the floor.</summary>
        private sealed class Reaper : FxEffect
        {
            protected override float Life => 1.9f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Mathf.Max(2f, Radius) * 1.0f;
                float fadeIn = t < 0 ? EaseOut(pre) : 1, fadeOut = Fade(t, 1.05f, 1.8f);
                float body = fadeIn * fadeOut;
                Vector2 fig = at + new Vector2(-r * .55f, r * .75f) + Vector2.up * (1 - fadeIn) * -1.5f;
                float lean = t < 0 ? -pre * .5f : Mathf.Lerp(-.5f, .9f, EaseOut(Rise(t, 0, .35f)));
                // robe: layered dark smoke with a faint teal rim, a hood and two white eyes
                b.Quad(FxShape.Shade, fig + Vector2.down * 1.3f, new Vector2(1.6f, 2.0f), 0, A(DKit.Dark, .85f * body), 0, Seed);
                for (int k = 0; k < 6; k++)
                {
                    Vector2 p = fig + new Vector2(Mathf.Sin(now * 2 + k) * .12f + lean * (.2f - k * .12f), -k * .38f + .3f);
                    b.Quad(FxShape.Glow, p, 1.0f - k * .06f, A(DKit.Teal, .12f * body), 0, Seed);
                    b.Quad(FxShape.Smoke, p, new Vector2(.75f + k * .17f, .6f), 0, A(new Color(.02f, .07f, .09f), .98f * body), now * .3f, Seed + k);
                }
                b.Quad(FxShape.Smoke, fig + new Vector2(lean * .15f, .75f), .62f, A(new Color(.01f, .04f, .06f), body), 0, Seed + 9);
                b.Quad(FxShape.Glow, fig + new Vector2(lean * .15f - .15f, .72f), .17f, A(White, body), 0, Seed);
                b.Quad(FxShape.Glow, fig + new Vector2(lean * .15f + .13f, .72f), .17f, A(White, body), 0, Seed);
                // the scythe: a long pale haft with a crescent head, swinging from behind the head to the front
                float ang = (t < 0 ? Mathf.Lerp(150f, 118f, pre) : Mathf.Lerp(118f, -62f, EaseOut(Rise(t, 0, .38f)))) * Mathf.Deg2Rad;
                Vector2 hand = fig + new Vector2(.5f, -.2f), tip = hand + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang) * .8f) * 3.6f;
                b.Link(hand - (tip - hand) * .25f, tip, .14f, A(DKit.Ghost, .95f * body), Seed, .5f);
                b.Quad(DKit.Scythe, tip + new Vector2(Mathf.Cos(ang + 1.57f), Mathf.Sin(ang + 1.57f) * .8f) * .7f, 1.1f, ang * Mathf.Rad2Deg + 200, A(DKit.Ghost, 1.2f * body), .8f, Seed + 3, 0);
                b.Quad(FxShape.Glow, tip, .9f, A(DKit.Teal, .5f * body), 0, Seed);
                if (t < 0) { b.Floor(FxShape.Glow, at, r * 1.2f * pre, A(DKit.Teal, .22f * pre), 0, Seed); return; }
                // the slash: a huge crescent trailing the blade, then a cold flash and the souls
                float sweep = Rise(t, 0, .38f), fade = Fade(t, .38f, 1.1f);
                b.Quad(DKit.Scythe, at + Vector2.up * .55f, new Vector2(r * 1.45f, r * 1.0f), 0, A(DKit.Ghost, 1.2f * (t < .38f ? 1 : fade)), sweep, Seed, 0);
                b.Quad(DKit.Scythe, at + Vector2.up * .55f, new Vector2(r * 1.62f, r * 1.12f), 0, A(DKit.Teal, .55f * (t < .38f ? 1 : fade)), Mathf.Clamp01(sweep - .06f), Seed, 0);
                b.Quad(FxShape.Glow, at, r * 2.0f, A(DKit.Teal, .5f * Fade(t, .3f, .8f)), 0, Seed);
                b.Floor(FxShape.Shock, at, r * 1.5f, A(DKit.Teal, .9f), Rise(t, .25f, 1.0f), Seed);
                b.Floor(FxShape.Smoke, at, r * 1.4f, A(new Color(.2f, .55f, .55f), .5f * Fade(t, .3f, 1.7f)), Rise(t, 0, 1.6f), Seed + 2);
                for (int k = 0; k < 9; k++)
                {
                    float born = .3f + k * .035f, u = Rise(t, born, born + .95f); if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + new Vector2((H(Seed, k) - .5f) * r * 1.9f, (H(Seed, k + 9) - .5f) * r * .8f) + Vector2.up * EaseOut(u) * 3.2f;
                    SpiritFx.Ghost(b, p, new Vector2(Mathf.Sin(k * 3f) * .35f, 1), .55f, Fade(u, .5f, 1), Seed + k, now);
                }
            }
        }
    }
}
