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
        // owner 29/10/2026: small glints and sparks were drawn far too big: small ones shrink, big set-piece flashes keep their size
        private static void Glint(FxBatch b, Vector2 at, float size, Color tint, float alpha, float turn = 0)
            => FxKit.Glint(b, at, size <= .08f ? size : size < .5f ? .08f + (size - .08f) * .45f : size - .231f, tint, alpha, turn);
        private const FxShape PageS = (FxShape)41, RuneS = (FxShape)43, Half = (FxShape)41, Closed = (FxShape)41;
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
            b.Quad(FxShape.Glow, p, s * 1.4f, A(Sepia, c.a * .08f), 0, seed);
            b.Quad(PageS, p, new Vector2(s * w, s * 1.1f), rot, c, 0, seed, cover);
        }

        private static Vector2 Squash(Vector2 v, float k = .62f) => new Vector2(v.x, v.y * k);

        /// <summary>Step 2 rune trait. No ring (owner 29/10/2026: too many yellow rings on enemies): a big rune is written stroke by stroke
        /// in the air over the enemy by a glowing nib, in dark wet ink with a gold sheen; the finished glyph flashes, drops and stamps flat
        /// onto the enemy as a splash of ink that spatters drops and curls of ink smoke.</summary>
        private sealed class RuneStamp : FxEffect
        {
            protected override float Life => 1.3f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                if (t < 0) return;
                Vector2 at = At; float r = Mathf.Max(1.15f, Radius) * 1.5f;
                float draw = Rise(t, 0, .45f), drop = EaseOut3(Rise(t, .55f, .68f)) , fade = Fade(t, .85f, 1.3f), flash = Fade(t, .45f, .6f) * Rise(t, .42f, .46f);
                int glyph = Seed % 11 + 3;
                // the glyph in the air: a dark ink body under a gold sheen, tilting forward as it falls onto the enemy
                Vector2 c = at + Vector2.up * (1.6f * (1 - drop) + .1f);
                Vector2 sz = new Vector2(r, r * Mathf.Lerp(1f, .55f, drop));
                if (drop < 1)
                {
                    b.Quad(FxShape.Shade, c, sz * 1.05f, 0, A(InkDark, .85f), 0, Seed);
                    b.Quad(FxShape.Glow, c, r * 1.2f, A(Gold, .2f * draw), 0, Seed);
                    b.Quad(RuneS, c, sz, 0, A(Gold, 1.1f), draw, glyph, 1);
                    b.Quad(FxShape.Glow, c, r * 1.1f, A(Sepia, .25f * draw), 0, Seed);
                    if (draw < 1) Glint(b, c + new Vector2(Mathf.Sin(t * 40) * r * .45f, Mathf.Cos(t * 29) * r * .4f), .4f, White, 1, now * 400);
                }
                if (flash > 0) Glint(b, c, r * .9f, Cream, flash, 45);
                if (t < .68f) { b.Floor(FxShape.Shade, at, r * .9f * Rise(t, 0, .6f), A(InkDark, .4f), 0, Seed); return; }
                // the stamp: the glyph lies flat in a pool of ink, the ink splashes out in long drops, then it all dries and fades
                float s = t - .68f, splash = EaseOut3(Rise(s, 0, .15f));
                b.Floor(FxShape.Shade, at, r * (1.0f + .25f * splash), A(InkDark, .8f * fade), 0, Seed);
                b.Floor(RuneS, at, r * (.95f + .1f * Fade(s, 0, .2f)), A(Gold, .9f * fade), 1, glyph, 1);
                for (int k = 0; k < 14; k++)
                {
                    float u = s / (.4f + .25f * H(Seed, k)); if (u <= 0 || u >= 1) continue;
                    Vector2 d = Dir(Seed, k, 14, .6f), p = at + d * r * (.6f + 1.0f * EaseOut(u)) + Vector2.up * (1.4f * H(Seed, k + 9) * 4 * u * (1 - u));
                    b.Link(p - d * .25f * (1 - u), p, .07f * (1 - u * .5f), A(InkDark, .95f * (1 - u * u)), Seed + k, .2f);
                    b.Quad(FxShape.Smoke, p, .12f + .08f * H(Seed, k + 3), A(InkDark, .9f * (1 - u)), u, Seed + k);
                    if (k % 3 == 0) Glint(b, p, .14f, Gold, .8f * (1 - u), k * 40);
                }
                for (int k = 0; k < 6; k++)
                {
                    float u = Rise(s, .05f + k * .05f, .6f + k * .04f); if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + new Vector2((H(Seed, k + 70) - .5f) * r * 1.2f + Mathf.Sin(u * 6 + k) * .15f, (H(Seed, k + 71) - .5f) * r * .4f + 1.3f * EaseOut(u));
                    b.Quad(FxShape.Smoke, p, .2f + .35f * u, 0, A(new Color(.3f, .19f, .12f), .75f * Mathf.Min(1, u * 4) * (1 - u) * fade), u + k, Seed + k + 70);
                }
            }
        }

        /// <summary>Records page (step 4): a page tears itself off the creature with a rip of paper dust, and a chapter of six big pages
        /// flaps up on a spiral, ink dots falling off them, torn scraps fluttering down; a gold sparkle crowns the first page.</summary>
        private sealed class RecordPage : FxEffect
        {
            protected override float Life => 1.4f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                if (t < 0) return;
                Vector2 at = At; float r = Mathf.Max(1f, Radius);
                b.Floor(FxShape.Glow, at, r * .9f, A(Sepia, .3f * Fade(t, 0, .5f)), 0, Seed);
                for (int k = 0; k < 6; k++)
                {
                    float u = (t - k * .08f) / 1.0f; if (u <= 0 || u >= 1) continue;
                    float ang = u * 6.5f + k * 1.05f, rad = r * .75f * EaseOut(u);
                    Vector2 p = at + new Vector2(Mathf.Cos(ang) * rad, .3f + 2.6f * EaseOut(u) + Mathf.Sin(ang) * rad * .3f);
                    Page(b, p, 1.05f - .05f * k, Mathf.Sin(u * 9 + k) * 28, u * 12 + k, A(Cream, Fade(u, .65f, 1)), Seed + k);
                    for (int d = 1; d <= 3; d++)
                    {
                        float back = Mathf.Max(0, u - d * .06f);
                        Vector2 q = at + new Vector2(Mathf.Cos(back * 6.5f + k * 1.05f) * r * .75f * EaseOut(back), .3f + 2.6f * EaseOut(back) - d * .15f * u);
                        b.Quad(FxShape.Smoke, q, .1f, A(InkDark, .8f * (1 - u)), u, Seed + d);
                    }
                    if (k == 0) Glint(b, p + Vector2.up * .7f, .5f, Gold, 1 - u, now * 60);
                }
                // torn scraps from the tear flutter down, and a puff of paper dust where it ripped
                b.Quad(FxShape.Dust, at + Vector2.up * .2f, new Vector2(1.1f, .6f) * (.6f + .6f * Rise(t, 0, .8f)), 0, A(new Color(.95f, .86f, .66f), .75f * Fade(t, .1f, .9f)), t, Seed + 9);
                for (int k = 0; k < 12; k++)
                {
                    float u = (t - .05f) / (.8f + .3f * H(Seed, k + 20)); if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + Dir(Seed + 2, k, 12, .7f) * 1.5f * EaseOut(u) + new Vector2(Mathf.Sin(u * 9 + k) * .15f, .9f * u - 1.5f * u * u + .3f);
                    b.Quad(PageS, p, new Vector2(.18f, .22f) * (.5f + .5f * Mathf.Abs(Mathf.Cos(u * 12 + k))), u * 500 + k * 40, A(Cream, 1 - u), 0, Seed + k, 0);
                }
            }
        }

        /// <summary>Step 6. An enormous open book tumbles out of the sky, pages fanning, its shadow growing; it lands open on the crowd and
        /// SLAMS shut: both halves swing up and together over the spine, and the closed book (leather cover, gold tooling, cream page block
        /// showing its thickness, rounded spine) thumps down and bounces. Dust and ink burst out of the spine, loose pages blow across the
        /// floor, then the book settles and fades.</summary>
        private sealed class GiantBook : FxEffect
        {
            protected override float Life => 1.9f;
            private static void Open(FxBatch b, Vector2 c, float W, float Hh, float fold, float alpha, int seed, float tilt)
            {
                // two halves with the spine at c; fold 0 = lying open, 1 = standing up (narrow)
                float k = Mathf.Max(.04f, Mathf.Cos(fold * Mathf.PI * .5f)), lift = Mathf.Sin(fold * Mathf.PI * .5f) * Hh * .35f;
                for (int s = -1; s <= 1; s += 2)
                    b.Quad(Half, c + new Vector2(s * W * .5f * k, lift), new Vector2(s * W * .5f * k, Hh), s * tilt, A(new Color(1f, .9f, .74f), alpha), 0, seed + (s > 0 ? 3 : 0), 3);
            }
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Radius * .95f, W = r * 1.9f, Hh = r * .78f;
                if (t < 0)
                {
                    float g = EaseOut(pre), fall = 1 - Mathf.Pow(pre, 2.4f), size = 1 + .3f * fall;
                    Vector2 c = at + new Vector2(0, 5.5f * fall + .3f);
                    b.Floor(FxShape.Shade, at, r * 1.6f * (.55f + .45f * g), A(InkDark, .6f * g), 0, Seed);
                    Open(b, c, W * size, Hh * size, .25f * fall + .1f * Mathf.Sin(now * 7) * fall, g, Seed, 7 * fall);
                    // pages riffling off the book as it falls
                    for (int k = 0; k < 7; k++)
                    {
                        float u = (pre * 1.8f + H(Seed, k)) % 1;
                        Page(b, c + new Vector2((H(Seed, k + 8) - .5f) * W * 1.1f, 1.2f - u * 2.5f), .6f, u * 380 + k * 40, u * 17 + k, A(Cream, .9f * g * (1 - u)), Seed + k);
                    }
                    return;
                }
                float close = Mathf.Clamp01(t / .2f), after = Fade(t, 1.0f, 1.9f);
                Vector2 c0 = at + Vector2.up * .3f;
                b.Floor(FxShape.Shade, at, r * 1.5f, A(InkDark, .45f * after), 0, Seed);
                if (close < 1) Open(b, c0, W, Hh, close * close, 1, Seed, 0);
                else
                {
                    // the closed book thumps down, bounces once and settles; its gold clasp glints
                    float s = t - .2f, bounce = Mathf.Abs(Mathf.Sin(Mathf.Min(s, .45f) * 7)) * .35f * Fade(s, 0, .45f), squash = 1 + .1f * Fade(s, 0, .12f);
                    Vector2 bc = c0 + Vector2.up * bounce;
                    b.Quad(Closed, bc, new Vector2(W * .55f * squash, Hh * 1.05f / squash), 0, A(Leather, after), 0, Seed, 2);
                    Glint(b, bc + new Vector2(W * .5f, .1f), .7f, Gold, Fade(s, .05f, .7f) * Rise(s, 0, .05f), now * 30);
                }
                if (t > .17f)
                {
                    float u = t - .17f;
                    b.Quad(FxShape.Glow, at, r * 1.2f, A(Cream, .35f * Fade(u, 0, .3f)), 0, Seed);
                    // the shut spine blows a low ring of grainy library dust across the floor, old dust billowing up off it, ink spitting out
                    for (int k = 0; k < 14; k++)
                    {
                        float d = Rise(u, 0, 1.1f); if (d <= 0 || d >= 1) continue;
                        Vector2 p = at + Dir(Seed, k, 14, .55f) * r * (.45f + 1.0f * EaseOut(d));
                        b.Quad(FxShape.Dust, p, new Vector2(r * .45f, r * .28f) * (.6f + d), 0, A(new Color(.85f, .72f, .52f), .85f * (1 - d)), d, Seed + k);
                        if (k % 2 == 0) b.Quad(FxShape.Smoke, p + Vector2.up * (.3f + .6f * d), r * .3f * (.6f + d), 0, A(new Color(.62f, .5f, .36f), .65f * (1 - d)), d + k, Seed + k + 30);
                    }
                    for (int k = 0; k < 10; k++)
                    {
                        float d = u / (.45f + .25f * H(Seed, k + 60)); if (d <= 0 || d >= 1) continue;
                        Vector2 p = at + Dir(Seed + 8, k, 10, .6f) * r * 1.1f * EaseOut(d) + Vector2.up * (2f * H(Seed, k + 61) * d - 3f * d * d + .3f);
                        b.Quad(FxShape.Shade, p, .14f + .08f * H(Seed, k), A(InkDark, .95f * (1 - d)), 0, Seed + k);
                    }
                    for (int k = 0; k < 14; k++)
                    {
                        float life = .9f + .6f * H(Seed, k + 40), d = u / life; if (d <= 0 || d >= 1) continue;
                        Vector2 p = at + Dir(Seed + 5, k, 14, .6f) * r * 1.5f * EaseOut3(d) + Vector2.up * (Mathf.Sin(d * Mathf.PI) * 1.2f);
                        Page(b, p, .55f, d * 500 + k * 25, d * 22 + k, A(Cream, 1 - d * d), Seed + k);
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
                // every cut snips confetti off the pages, and the whirl lifts paper dust off the floor
                b.Quad(FxShape.Dust, at, new Vector2(r * 1.4f, r * .7f) * (.7f + .5f * Rise(t, 0, 1)), 0, A(new Color(.95f, .86f, .66f), .6f * Fade(t, .3f, 1.1f)), t, Seed + 3);
                for (int k = 0; k < 14; k++)
                {
                    float u = (t - .05f - (k % 4) * .08f) / .7f; if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + Dir(Seed + 4, k, 14, .6f) * r * (.3f + 1.4f * EaseOut(u)) + Vector2.up * (1.2f * u - 1.6f * u * u + .4f);
                    b.Quad(PageS, p, new Vector2(.09f, .12f) * (.4f + .6f * Mathf.Abs(Mathf.Cos(u * 14 + k))), u * 700 + k * 50, A(k % 3 == 0 ? Gold : Cream, 1.1f * (1 - u)), 0, Seed + k, 0);
                }
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
                b.Quad(FxShape.Glow, c, r * 1.6f, A(Sepia, .15f * fade), 0, Seed);
                // the card spins in, sways while it is written on, and tips away as it is filed
                float rot = -4 + (1 - drop) * 170 + Mathf.Sin(t * 6) * 3 * (1 - flip) + flip * 30;
                b.Floor(FxShape.Shade, c + new Vector2(.15f, -.2f), r * 1.2f, A(InkDark, .4f * fade), 0, Seed + 3);
                b.Quad(PageS, c, size * (1 + .15f * Fade(t, .12f, .3f) * Rise(t, 0, .12f)), rot, A(Cream, fade), Mathf.Repeat(rot / 360f, 1), Seed + 21, 0);
                if (flip > 0 && flip < 1) for (int k = 0; k < 6; k++) Glint(b, c + new Vector2((k - 2.5f) * .3f, -.6f - k * .15f * flip), .18f, Gold, (1 - flip), k * 30);
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
                if (seal > 0)
                {
                    Vector2 sp = c + new Vector2(r * .8f, -r * .45f); b.Quad(RuneS, sp, new Vector2(.5f, .5f) * (1.6f - .6f * seal), 0, A(Gold, 1.1f * fade), 1, Seed % 11); b.Quad(FxShape.Glow, sp, .9f, A(Gold, .8f * Fade(t, .85f, 1.1f)), 0, Seed);
                    // the stamp spits wax drops and a puff of dust off the card
                    float su = Rise(t, .88f, 1.25f);
                    if (su > 0 && su < 1) for (int k = 0; k < 6; k++)
                    {
                        Vector2 p = sp + Dir(Seed + 7, k, 6, .7f) * .9f * EaseOut(su) + Vector2.up * (.6f * su - 1.2f * su * su);
                        b.Quad(FxShape.Glow, p, .09f, A(new Color(1f, .35f, .15f), 1 - su), 0, Seed);
                        b.Quad(FxShape.Dust, sp + Dir(Seed + 3, k, 6, .5f) * .5f * su, new Vector2(.3f, .2f) * (.6f + su), 0, A(Cream, .55f * (1 - su)), su, Seed + k);
                    }
                }
                // ink drips off the end of each written line
                for (int line = 0; line < 3; line++)
                {
                    float u = Rise(t, .35f + line * .22f, .9f + line * .22f); if (u <= 0 || u >= 1) continue;
                    Vector2 p = new Vector2(c.x - r * 1.05f + r * 2.1f * (line == 2 ? .6f : 1), c.y + r * (.35f - line * .3f) - u * u * .9f);
                    b.Quad(FxShape.Glow, p, .07f, A(Gold, fade * (1 - u)), 0, Seed);
                }

            }
        }
    }
}
