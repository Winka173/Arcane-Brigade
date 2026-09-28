using System;
using System.Collections.Generic;
using UnityEngine;
using static ArcaneBrigade.FxKit;

namespace ArcaneBrigade
{
    /// <summary>Archive bond: sepia parchment, flying pages, glowing ink runes and a giant book. Every rule is a different piece of
    /// stationery: a rune is inked onto the floor, a record page rises, a book slams shut, a flock of pages cuts a whirl.</summary>
    public static class ArchiveFx
    {
        private const FxShape PageS = (FxShape)41, RuneS = (FxShape)43;
        private static readonly Color Sepia = new Color(1f, .74f, .36f, 1), Cream = new Color(1f, .93f, .72f, 1), Gold = new Color(1f, .82f, .3f, 1);
        private static readonly Color InkDark = new Color(.05f, .025f, .012f, 1), Leather = new Color(.62f, .3f, .14f, 1);

        public static void Register(Dictionary<string, Func<FxEffect>> map)
        {
            map["rune"] = () => new RuneStamp();
            map["record-page"] = () => new RecordPage();
            map["archive-giant-book"] = () => new GiantBook();
            map["archive-pages"] = () => new FlyingPages();
            map["species-recorded"] = () => new IndexCard();
        }

        /// <summary>One parchment page seen edge-on and flat as it flaps: x narrows with the flap, a soft light sits behind it.</summary>
        private static void Page(FxBatch b, Vector2 p, float s, float rot, float flap, Color c, int seed, float cover = 0)
        {
            float w = Mathf.Max(.14f, Mathf.Abs(Mathf.Cos(flap)));
            b.Quad(FxShape.Glow, p, s * 1.5f, A(Sepia, c.a * .22f), 0, seed);
            b.Quad(PageS, p, new Vector2(s * w, s * 1.1f), rot, c, 0, seed, cover);
        }

        private static Vector2 Squash(Vector2 v, float k = .62f) => new Vector2(v.x, v.y * k);

        /// <summary>Step 2 rune trait. A rune is inked onto the floor under the enemy: the ring and the strokes draw themselves one after
        /// another, the finished glyph flashes gold, ink flicks off it in drops and three small runes lift away.</summary>
        private sealed class RuneStamp : FxEffect
        {
            protected override float Life => 1.25f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                if (t < 0) return;
                Vector2 at = At; float r = Mathf.Max(1.15f, Radius) * 1.6f;
                float draw = Rise(t, 0, .5f), fade = Fade(t, .8f, 1.25f), flash = Fade(t, .5f, .8f) * Rise(t, .44f, .5f);
                // wet ink pooled under the glyph
                b.Floor(FxShape.Shade, at, r * 1.05f, A(InkDark, .55f * Rise(t, 0, .2f) * fade), 0, Seed);
                b.Quad(RuneS, at, new Vector2(r, r * .7f), 0, A(Sepia, 1.1f * fade), draw, Seed % 11);
                b.Quad(RuneS, at + Vector2.up * .03f, new Vector2(r, r * .7f), 0, A(Gold, .5f * fade * draw), draw, Seed % 11);
                b.Floor(FxShape.Glow, at, r * 1.4f, A(Sepia, .45f * draw * fade), 0, Seed);
                if (flash > 0)
                {
                    b.Quad(FxShape.Glow, at, r * 1.9f, A(Cream, .8f * flash), 0, Seed);
                    b.Quad(FxShape.Sparkle, at + Vector2.up * .2f, new Vector2(r * 1.2f, r * 1.2f), 0, A(Gold, flash), 0, 0, 2);
                }
                // ink flicked off the finished glyph
                for (int k = 0; k < 9; k++)
                {
                    float u = (t - .45f) / (.55f + .2f * H(Seed, k)); if (u <= 0 || u >= 1) continue;
                    Vector2 dir = Dir(Seed, k, 9, .7f), p = at + dir * r * (.5f + .7f * EaseOut(u)) + Vector2.up * (1.6f * H(Seed, k + 9) * u - 2.6f * u * u + .1f);
                    b.Quad(FxShape.Smoke, p, .16f + .12f * H(Seed, k + 3), A(InkDark, .9f * (1 - u)), u, Seed + k);
                    Glint(b, p, .16f, Gold, .8f * (1 - u), k * 40);
                }
                // three small runes lift off, turning
                for (int k = 0; k < 3; k++)
                {
                    float u = (t - .5f - k * .09f) / .6f; if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + new Vector2((k - 1) * r * .5f + Mathf.Sin(u * 5 + k) * .15f, .2f + u * 1.9f);
                    b.Quad(RuneS, p, new Vector2(.6f, .6f), 0, A(Cream, 1.1f * (1 - u)), 1, Seed + k * 5 + 1);
                }
            }
        }

        /// <summary>Records page (not emitted as a moment yet): a page tears itself off the creature, flaps up on a spiral with ink dots
        /// falling off it, and a chapter of three more follows; a sparkle crowns the first.</summary>
        private sealed class RecordPage : FxEffect
        {
            protected override float Life => 1.2f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                if (t < 0) return;
                Vector2 at = At; float r = Mathf.Max(1f, Radius);
                b.Floor(FxShape.Glow, at, r * .9f, A(Sepia, .45f * Fade(t, 0, .5f)), 0, Seed);
                for (int k = 0; k < 4; k++)
                {
                    float u = (t - k * .1f) / .85f; if (u <= 0 || u >= 1) continue;
                    float ang = u * 7 + k * 1.7f, rad = .5f * EaseOut(u);
                    Vector2 p = at + new Vector2(Mathf.Cos(ang) * rad, .2f + 2.6f * EaseOut(u) + Mathf.Sin(ang) * rad * .3f);
                    Page(b, p, .5f - .08f * k, Mathf.Sin(u * 9 + k) * 28, u * 14 + k, A(Cream, 1.15f * Fade(u, .6f, 1)), Seed + k);
                    for (int d = 1; d <= 3; d++)
                    {
                        float back = Mathf.Max(0, u - d * .06f);
                        Vector2 q = at + new Vector2(Mathf.Cos(back * 7 + k * 1.7f) * .5f * EaseOut(back), .2f + 2.6f * EaseOut(back) - d * .12f * u);
                        b.Quad(FxShape.Smoke, q, .09f, A(InkDark, .8f * (1 - u)), u, Seed + d);
                    }
                    if (k == 0) Glint(b, p + Vector2.up * .4f, .4f, Gold, 1 - u, now * 60);
                }
            }
        }

        /// <summary>Step 6. An enormous open book tumbles out of the sky with its pages fanning and its shadow growing; it snaps shut on
        /// the crowd, the two halves swinging up and together. Dust and ink burst out of the spine, loose pages blow across the floor,
        /// then the clasped cover glows and fades.</summary>
        private sealed class GiantBook : FxEffect
        {
            protected override float Life => 1.9f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Radius * 1.12f, halfW = r, halfH = r * 1.12f;
                Vector2 qs = new Vector2(halfW / .8f, halfH / .9f * .85f);
                if (t < 0)
                {
                    float g = EaseOut(pre), fall = 1 - Mathf.Pow(pre, 2.4f), size = 1 + .9f * fall;
                    Vector2 c = at + new Vector2(0, 5.5f * fall);
                    b.Floor(FxShape.Shade, at, r * 1.7f * (.55f + .45f * g), A(InkDark, .6f * g), 0, Seed);
                    b.Floor(FxShape.Glow, at, r * 1.4f, A(Sepia, .3f * g), 0, Seed);
                    float open = 1;
                    for (int s = -1; s <= 1; s += 2)
                    {
                        float sway = Mathf.Sin(now * 7 + s) * 4 * fall;
                        Vector2 p = c + new Vector2(s * halfW * size * open * .98f, 0);
                        b.Quad(PageS, p, new Vector2(qs.x, qs.y) * size, s * 7 * fall + sway, A(Cream, 1.2f * g), 0, Seed + (s > 0 ? 3 : 0), 0);
                        b.Quad(PageS, p, new Vector2(qs.x, qs.y) * size, s * 7 * fall + sway, A(Leather, .35f * g * fall), 0, Seed + 8, 1);
                    }
                    // pages riffling off the book as it falls
                    for (int k = 0; k < 7; k++)
                    {
                        float u = (pre * 1.8f + H(Seed, k)) % 1;
                        Page(b, c + new Vector2((H(Seed, k + 8) - .5f) * halfW * 2.3f, 1.4f - u * 2.5f), .55f, u * 380 + k * 40, u * 17 + k, A(Cream, .9f * g * (1 - u)), Seed + k);
                    }
                    return;
                }
                float close = Mathf.Clamp01(t / .2f), phi = close * Mathf.PI * .5f, cosP = Mathf.Cos(phi);
                float flash = Fade(t, .2f, .55f) * Rise(t, .17f, .21f), after = Fade(t, .9f, 1.9f);
                b.Floor(FxShape.Shade, at, r * 1.5f, A(InkDark, .4f * after), 0, Seed);
                if (close < 1)
                {
                    for (int s = -1; s <= 1; s += 2)
                    {
                        Vector2 p = at + new Vector2(s * halfW * cosP * .98f, Mathf.Sin(phi) * 1.3f * (1 - close * .3f));
                        b.Quad(PageS, p, new Vector2(Mathf.Max(.05f, qs.x * cosP), qs.y), 0, A(Cream, 1.2f), 0, Seed + (s > 0 ? 3 : 0), 0);
                    }
                }
                else
                {
                    float settle = 1 + .06f * Mathf.Sin(t * 30) * Fade(t, .2f, .5f);
                    b.Quad(PageS, at, qs * settle, 0, A(Leather, 1.15f * after), 0, Seed, 1);
                    b.Quad(PageS, at, qs * settle, 0, A(Gold, .5f * after), 0, Seed, 1);
                    Glint(b, at, 1.1f, Gold, .9f * Fade(t, .2f, 1.1f), now * 30);
                }
                if (t > .17f)
                {
                    float u = t - .17f;
                    b.Quad(FxShape.Glow, at, r * 2.7f, A(Cream, .85f * flash), 0, Seed);
                    b.Quad(FxShape.Rays, at, r * 2.6f, A(Gold, .7f * Fade(u, 0, .6f)), u, Seed, 5);
                    // the shut spine blows dust and ink out across the floor
                    for (int k = 0; k < 12; k++)
                    {
                        float d = Rise(u, 0, 1.1f); if (d <= 0 || d >= 1) continue;
                        Vector2 p = at + Dir(Seed, k, 12, .55f) * r * (.4f + 1.4f * EaseOut(d));
                        b.Quad(FxShape.Smoke, p, new Vector2(r * .55f, r * .4f) * (.6f + d), 0, A(new Color(.55f, .42f, .26f), .75f * (1 - d)), d, Seed + k);
                        if (k % 2 == 0) b.Quad(FxShape.Smoke, p * .9f + at * .1f, .45f, A(InkDark, .8f * (1 - d)), d, Seed + k + 30);
                    }
                    for (int k = 0; k < 14; k++)
                    {
                        float life = .9f + .6f * H(Seed, k + 40), d = u / life; if (d <= 0 || d >= 1) continue;
                        Vector2 p = at + Dir(Seed + 5, k, 14, .6f) * r * 1.9f * EaseOut3(d) + Vector2.up * (Mathf.Sin(d * Mathf.PI) * 1.2f);
                        Page(b, p, .5f, d * 500 + k * 25, d * 22 + k, A(Cream, 1.1f * (1 - d * d)), Seed + k);
                    }
                }
            }
        }

        /// <summary>Step 8. A flying library: nine pages leave the hero in a flock, each on its own flapping arc, and land on the crowd
        /// as a paper whirlwind that spirals tighter, slicing cream lines through the enemies, then scatters upward.</summary>
        private sealed class FlyingPages : FxEffect
        {
            protected override float Life => 1.15f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 from = From, at = At; float r = Mathf.Max(1f, Radius);
                const int n = 9;
                if (t < 0)
                {
                    Vector2 d = at - from, perp = new Vector2(-d.y, d.x).normalized;
                    for (int k = 0; k < n; k++)
                    {
                        float u = Mathf.Clamp01(pre * 1.35f - k * .045f); if (u <= 0) continue;
                        float swing = Mathf.Sin(u * Mathf.PI * 2 + k * 1.3f) * (.6f + .5f * H(Seed, k)) * (1 - u * .5f);
                        Vector2 p = Vector2.Lerp(from, at, EaseOut(u)) + perp * swing + Vector2.up * Mathf.Sin(u * Mathf.PI) * 1.4f;
                        Page(b, p, .5f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg + Mathf.Sin(u * 20 + k) * 45, u * 25 + k, A(Cream, 1.15f), Seed + k);
                        for (int tr = 1; tr <= 3; tr++) Glint(b, p - d.normalized * tr * .22f + perp * Mathf.Sin(now * 9 + tr) * .1f, .14f, Gold, .6f / tr, k * 20);
                    }
                    return;
                }
                float swirlFade = Fade(t, .55f, 1.15f);
                b.Floor(FxShape.Glow, at, r * 2.2f, A(Sepia, .45f * Fade(t, 0, .8f)), 0, Seed);
                for (int k = 0; k < n; k++)
                {
                    float ang = k * 2 * Mathf.PI / n + t * (9 + k * .3f), rad = r * (2.4f - 1.1f * Rise(t, 0, .5f)) * (.75f + .25f * H(Seed, k));
                    Vector2 p = at + new Vector2(Mathf.Cos(ang) * rad, Mathf.Sin(ang) * rad * .55f + Rise(t, .4f, 1.15f) * 2.2f);
                    Page(b, p, .66f * (1 - Rise(t, .8f, 1.15f) * .4f), ang * Mathf.Rad2Deg + 90, t * 22 + k, A(Cream, .95f * swirlFade), Seed + k);
                    // the cut: a thin cream slash from the page to the middle
                    if (t < .55f) b.Link(p, at, .05f, A(Cream, .6f * Fade(t, .1f, .55f)), Seed + k, .8f);
                }
                if (t < .35f) b.Quad(FxShape.Sparkle, at, new Vector2(r * 1.3f, r * 1.3f), t * 200, A(Gold, .9f * Fade(t, 0, .35f)), 0, 0, 2);
                for (int k = 0; k < 10; k++)
                {
                    float u = (t * 1.4f + H(Seed, k)) % 1; if (t > .9f) break;
                    Glint(b, at + Dir(Seed, k, 10, .6f) * r * (.4f + u) + Vector2.up * u * 1.3f, .15f, Cream, .8f * (1 - u) * swirlFade, k * 50);
                }
            }
        }
    
        /// <summary>A new species is recorded (trait, step 6): a big index card drops flat over the creature, a glowing quill scribbles
        /// three lines across it left to right, stamps a gold seal in the corner, and the card flips up and slides away.</summary>
        private sealed class IndexCard : FxEffect
        {
            protected override float Life => 1.4f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                if (t < 0) return;
                Vector2 at = At + Vector2.up * .9f; float r = Mathf.Max(1.2f, Radius) * .9f;
                float drop = EaseOut3(Rise(t, 0, .15f)), flip = Rise(t, 1f, 1.4f), fade = 1 - flip;
                Vector2 c = at + Vector2.up * (1.2f * (1 - drop) + 2.2f * flip * flip);
                Vector2 size = new Vector2(r * 1.35f * Mathf.Max(.08f, 1 - flip * 1.6f), r * .85f);
                b.Floor(FxShape.Shade, At, r * 1.3f, A(InkDark, .45f * fade), 0, Seed);
                b.Quad(FxShape.Glow, c, r * 2f, A(Sepia, .35f * fade), 0, Seed);
                b.Quad(PageS, c, size, -4, A(Cream, 1.05f * fade), 0, Seed + 21, 0);
                // the quill writing three lines
                for (int line = 0; line < 3; line++)
                {
                    float w = Rise(t, .15f + line * .22f, .35f + line * .22f); if (w <= 0) continue;
                    float y = c.y + r * (.35f - line * .3f);
                    Vector2 l0 = new Vector2(c.x - r * 1.05f, y), l1 = new Vector2(c.x - r * 1.05f + r * 2.1f * w * (line == 2 ? .6f : 1), y + Mathf.Sin(t * 40) * .02f);
                    b.Link(l0, l1, .07f, A(Gold, .9f * fade), Seed + line, .6f);
                    if (w < 1)
                    {
                        Vector2 tip = l1 + new Vector2(0, Mathf.Sin(t * 60) * .06f);
                        b.Link(tip, tip + new Vector2(.55f, 1.2f), .09f, A(Cream, 1f), Seed, .3f);
                        Glint(b, tip, .3f, Gold, 1, now * 300);
                    }
                }
                float seal = Rise(t, .82f, .9f);
                if (seal > 0) { Vector2 sp = c + new Vector2(r * .8f, -r * .45f); b.Quad(RuneS, sp, new Vector2(.5f, .5f) * (1.6f - .6f * seal), 0, A(Gold, 1.1f * fade), 1, Seed % 11); b.Quad(FxShape.Glow, sp, .9f, A(Gold, .8f * Fade(t, .85f, 1.1f)), 0, Seed); }
            }
        }
    }
}
