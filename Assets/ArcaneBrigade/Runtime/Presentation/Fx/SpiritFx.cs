using System;
using System.Collections.Generic;
using UnityEngine;
using static ArcaneBrigade.FxKit;

namespace ArcaneBrigade
{
    /// <summary>Spirit bond: ghost wisps, pale teal souls, a reaper's scythe arc.</summary>
    public static class SpiritFx
    {
        // owner 29/10/2026: small glints and sparks were drawn far too big: small ones shrink, big set-piece flashes keep their size
        private static void Glint(FxBatch b, Vector2 at, float size, Color tint, float alpha, float turn = 0)
            => FxKit.Glint(b, at, size <= .08f ? size : size < .5f ? .08f + (size - .08f) * .45f : size - .231f, tint, alpha, turn);
        public static void Register(Dictionary<string, Func<FxEffect>> map)
        {
            map["spirit-swarm"] = () => new Swarm();
            map["spirit-reaper"] = () => new Reaper();
        }

        private static readonly Color Mist = new Color(.35f, .7f, .68f, 1), Robe = new Color(.02f, .07f, .09f, 1);

        /// <summary>A wisp: a pale teal ghost with a wavy tail and hollow eyes, pointing along <paramref name="dir"/>.</summary>
        internal static void Ghost(FxBatch b, Vector2 at, Vector2 dir, float size, float alpha, int seed, float now)
        {
            if (alpha < .004f) return;
            b.Quad(FxShape.Glow, at, size * 1.3f, A(DKit.Teal, .35f * alpha), 0, seed);
            b.Quad(DKit.Wisp, at - dir.normalized * size * .45f, new Vector2(size * 1.5f, size * 1.1f), DKit.Deg(dir), A(DKit.Ghost, 1.15f * alpha), now, seed, 0);
        }

        /// <summary>Six wisps stream off the hero in a loose corkscrew, each dragging a thin trail of cold mist and flecks, dive into the
        /// crowd in a shrinking spiral, then the souls shoot up out of it while a teal mist rolls out low over the floor and soul
        /// sparks wink out one by one.</summary>
        private sealed class Swarm : FxEffect
        {
            protected override float Life => 1.25f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At, from = From; float r = Mathf.Max(1.2f, Radius) * 1.0f;
                const int N = 6;
                if (t < 0)
                {
                    Vector2 d = at - from; Vector2 n = new Vector2(-d.y, d.x).normalized;
                    for (int k = 0; k < N; k++)
                    {
                        float u = Mathf.Clamp01(pre - k * .036f); if (u <= 0) continue; int kk = k;
                        Vector2 P(float v) => DKit.Arc(from, at, v, 1.4f + .3f * kk) + n * Mathf.Sin(v * 11 + kk * 1.7f) * (1 - v) * (.45f + .12f * kk);
                        Vector2 p = P(u), q = P(Mathf.Max(0, u - .04f));
                        for (int s = 1; s <= 4; s++)
                        {
                            Vector2 tp = P(Mathf.Max(0, u - s * .05f));
                            b.Quad(FxShape.Smoke, tp, .32f * (1 + s * .2f), 0, A(Mist, .3f * (1 - s / 5f)), s * .2f, Seed + k * 7 + s);
                            Glint(b, tp + new Vector2(0, -.1f * s), .13f * (1 - s / 5f), DKit.Teal, .8f * (1 - s / 5f), s * 40);
                        }
                        Ghost(b, p, p - q + Vector2.right * .001f, .55f, 1, Seed + k, now);
                    }
                    b.Floor(FxShape.Glow, at, r * .8f * pre, A(DKit.Teal, .22f * pre), 0, Seed);
                    return;
                }
                float dive = Rise(t, 0, .42f);
                for (int k = 0; k < N; k++)
                {
                    float a0 = k / (float)N * 6.2832f + t * 9;
                    if (t < .42f)
                    {
                        float rad = r * (1 - EaseOut(dive)) * .95f + .05f; Vector2 p = at + new Vector2(Mathf.Cos(a0) * rad, Mathf.Sin(a0) * rad * .62f) + Vector2.up * (1.1f * (1 - dive));
                        Vector2 tang = new Vector2(-Mathf.Sin(a0), Mathf.Cos(a0) * .62f);
                        Ghost(b, p, tang - Vector2.up * dive * .6f, .55f * (1 - dive * .35f), 1, Seed + k, now);
                        Glint(b, p - tang * .5f, .12f, DKit.Ghost, .8f, k * 30);
                    }
                    else
                    {
                        float u = Rise(t, .42f, 1.15f); Vector2 p = at + new Vector2((H(Seed, k) - .5f) * r * .9f + Mathf.Sin(u * 7 + k) * .2f, 0) + Vector2.up * (EaseOut(u) * (2.2f + H(Seed, k + 5) * 1.0f));
                        Ghost(b, p, Vector2.up + Vector2.right * Mathf.Sin(k + u * 6) * .4f, .52f + .15f * u, Fade(u, .45f, 1), Seed + k, now);
                        for (int s = 1; s <= 3; s++) Glint(b, p + Vector2.down * (.35f + s * .25f), .1f, DKit.Teal, Fade(u, .3f, 1) * (1 - s / 4f), s * 60);
                    }
                }
                float ring = Rise(t, .38f, 1.1f);
                b.Quad(FxShape.Glow, at, r * 1.2f, A(DKit.Ghost, .5f * Fade(t, .38f, .75f)), 0, Seed);
                b.Floor(FxShape.Shock, at, r * 1.35f, A(DKit.Teal, .85f), ring, Seed);
                b.Floor(FxShape.Shade, at, r * .8f, A(DKit.Dark, .45f * Fade(t, .5f, 1.2f) * Rise(t, .38f, .5f)), 0, Seed + 1);
                DKit.DustRing(b, at, r * 1.0f, t - .38f, Mist, Seed, 8, .85f, .55f);
                b.Quad(FxShape.Smoke, at + Vector2.up * .3f, DKit.Sq(r * .9f), 0, A(Mist, .4f * Mathf.Sin(Mathf.Clamp01(ring) * Mathf.PI)), ring, Seed + 3);
                for (int k = 0; k < 14; k++)
                {
                    float u = Rise(t, .4f + .025f * k, 1.2f); if (u <= 0 || u >= 1) continue;
                    Glint(b, at + new Vector2((H(Seed, k + 40) - .5f) * r * 1.6f, (H(Seed, k + 60) - .5f) * r * .7f) + Vector2.up * u * 1.8f, .18f, DKit.Ghost, Mathf.Sin(u * Mathf.PI) * (.6f + .4f * Mathf.Sin(t * 30 + k)), k * 40);
                }
            }
        }

        /// <summary>A hooded reaper rises over the crowd with ragged hems flapping and eyes flickering, winds up, then its scythe cuts a
        /// glowing arc through everything with a spray of cold sparks along the blade; an afterimage of the cut echoes a beat later, torn
        /// souls fly up and frost mist lies low on the floor.</summary>
        private sealed class Reaper : FxEffect
        {
            protected override float Life => 1.9f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Mathf.Max(1.8f, Radius) * .95f;
                float fadeIn = t < 0 ? EaseOut(pre) : 1, fadeOut = Fade(t, 1.05f, 1.8f);
                float body = fadeIn * fadeOut;
                Vector2 fig = at + new Vector2(-r * .55f, r * .75f) + Vector2.up * (1 - fadeIn) * -1.5f;
                float lean = t < 0 ? -pre * .5f : Mathf.Lerp(-.5f, .9f, EaseOut(Rise(t, 0, .35f)));
                // robe: layered dark smoke with a faint teal rim, a hood, ragged hem strips and two flickering eyes
                b.Quad(FxShape.Shade, fig + Vector2.down * 1.2f, new Vector2(1.4f, 1.8f), 0, A(DKit.Dark, .8f * body), 0, Seed);
                for (int k = 0; k < 6; k++)
                {
                    Vector2 p = fig + new Vector2(Mathf.Sin(now * 2 + k) * .12f + lean * (.2f - k * .12f), -k * .36f + .3f);
                    b.Quad(FxShape.Glow, p, .9f - k * .06f, A(DKit.Teal, .12f * body), 0, Seed);
                    b.Quad(FxShape.Smoke, p, new Vector2(.7f + k * .15f, .56f), 0, A(Robe, .98f * body), now * .3f, Seed + k);
                }
                for (int k = 0; k < 5; k++)
                {
                    Vector2 hem = fig + new Vector2((k - 2) * .32f + lean * -.4f, -1.95f), rag = hem + new Vector2(Mathf.Sin(now * 7 + k * 1.3f) * .25f - .2f, -.55f - .15f * H(Seed, k));
                    for (int j = 0; j < 4; j++) b.Quad(FxShape.Shade, Vector2.Lerp(hem, rag, j / 3f), new Vector2(.16f - j * .025f, .2f), 0, A(Robe, .9f * body), 0, Seed + k);
                    b.Quad(FxShape.Smoke, rag, .2f, 0, A(Mist, .3f * body), now, Seed + k);
                }
                b.Quad(FxShape.Smoke, fig + new Vector2(lean * .15f, .75f), .58f, A(new Color(.01f, .04f, .06f), body), 0, Seed + 9);
                float blink = .75f + .25f * Mathf.Sin(now * 23) * Mathf.Sin(now * 7);
                b.Quad(FxShape.Glow, fig + new Vector2(lean * .15f - .14f, .72f), .16f, A(White, body * blink), 0, Seed);
                b.Quad(FxShape.Glow, fig + new Vector2(lean * .15f + .12f, .72f), .16f, A(White, body * blink), 0, Seed);
                b.Quad(FxShape.Glow, fig + new Vector2(lean * .15f, .72f), .5f, A(DKit.Teal, .35f * body), 0, Seed);
                // the scythe: a long pale haft with a crescent head, swinging from behind the head to the front
                float ang = (t < 0 ? Mathf.Lerp(150f, 118f, pre) : Mathf.Lerp(118f, -62f, EaseOut(Rise(t, 0, .38f)))) * Mathf.Deg2Rad;
                Vector2 hand = fig + new Vector2(.5f, -.2f), tip = hand + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang) * .8f) * 3.2f;
                b.Link(hand - (tip - hand) * .25f, tip, .12f, A(DKit.Ghost, .95f * body), Seed, .5f);
                b.Quad(DKit.Scythe, tip + new Vector2(Mathf.Cos(ang + 1.57f), Mathf.Sin(ang + 1.57f) * .8f) * .62f, 1.0f, ang * Mathf.Rad2Deg + 200, A(DKit.Ghost, 1.2f * body), .8f, Seed + 3, 0);
                b.Quad(FxShape.Glow, tip, .8f, A(DKit.Teal, .5f * body), 0, Seed);
                Glint(b, tip + new Vector2(Mathf.Cos(ang + 1.57f), Mathf.Sin(ang + 1.57f) * .8f) * 1.1f, .35f, White, body * (.5f + .5f * Mathf.Sin(now * 9)), now * 90);
                if (t < 0) { b.Floor(FxShape.Glow, at, r * 1.1f * pre, A(DKit.Teal, .2f * pre), 0, Seed); return; }
                // the slash: a crescent trailing the blade, sparks peeling off its edge, then a cold flash and the souls
                float sweep = Rise(t, 0, .38f), fade = Fade(t, .38f, 1.1f), cut = t < .38f ? 1 : fade;
                Vector2 c = at + Vector2.up * .55f; Vector2 sz = new Vector2(r * 1.3f, r * .9f);
                b.Quad(DKit.Scythe, c, sz, 0, A(DKit.Ghost, 1.2f * cut), sweep, Seed, 0);
                b.Quad(DKit.Scythe, c, sz * 1.1f, 0, A(DKit.Teal, .55f * cut), Mathf.Clamp01(sweep - .06f), Seed, 0);
                for (int k = 0; k < 12; k++)
                {
                    float born = k * .028f, u = Rise(t, born, born + .45f); if (u <= 0 || u >= 1) continue;
                    float a = Mathf.Lerp(-2.3f, 2.3f, Mathf.Clamp01(born / .38f)); Vector2 e0 = c + new Vector2(Mathf.Cos(a) * sz.x * .93f, Mathf.Sin(a) * sz.y * .93f);
                    Vector2 p = e0 + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * .9f * EaseOut(u) + Vector2.down * 1.2f * u * u;
                    b.Link(Vector2.Lerp(e0, p, .6f), p, .05f, A(DKit.Teal, 1 - u), Seed + k, .8f); Glint(b, p, .18f, White, 1 - u, k * 40);
                }
                // second beat: the afterimage of the cut flashes once
                float echo = DKit.Bump(t, .45f, .8f);
                b.Quad(DKit.Scythe, c + Vector2.down * .15f, sz * .9f, 0, A(White, .6f * echo), .95f, Seed + 4, 0);
                b.Quad(FxShape.Glow, at, r * 1.3f, A(DKit.Teal, .45f * Fade(t, .3f, .8f)), 0, Seed);
                b.Floor(FxShape.Shock, at, r * 1.3f, A(DKit.Teal, .85f), Rise(t, .25f, 1.0f), Seed);
                b.Floor(FxShape.Shade, at, r * 1.0f, A(DKit.Dark, .4f * Fade(t, .6f, 1.6f) * Rise(t, .2f, .4f)), 0, Seed + 5);
                DKit.DustRing(b, at, r * 1.0f, t - .2f, Mist, Seed + 2, 10, 1.4f, .5f);
                for (int k = 0; k < 9; k++)
                {
                    float born = .3f + k * .035f, u = Rise(t, born, born + .95f); if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + new Vector2((H(Seed, k) - .5f) * r * 1.7f + Mathf.Sin(u * 6 + k) * .2f, (H(Seed, k + 9) - .5f) * r * .7f) + Vector2.up * EaseOut(u) * 2.6f;
                    Ghost(b, p, new Vector2(Mathf.Sin(k * 3f + u * 4) * .35f, 1), .48f, Fade(u, .5f, 1), Seed + k, now);
                }
                DKit.Motes(b, at, r * 1.8f, 2.4f, now, 10, DKit.Ghost, Seed + 7, .7f * Fade(t, .9f, 1.8f) * Rise(t, .3f, .5f), .12f, .8f);
            }
        }
    }
}
