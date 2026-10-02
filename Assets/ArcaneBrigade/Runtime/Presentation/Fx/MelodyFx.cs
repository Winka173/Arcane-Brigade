using System;
using System.Collections.Generic;
using UnityEngine;
using static ArcaneBrigade.FxKit;

namespace ArcaneBrigade
{
    /// <summary>Melody bond: musical notes, sound waves, drum pulses, pastel colours.</summary>
    public static class MelodyFx
    {
        public static void Register(Dictionary<string, Func<FxEffect>> map)
        {
            map["melody-notes"] = () => new Notes();
            map["melody-sound-wave"] = () => new SoundWave();
            map["melody-war-drum"] = () => new WarDrum();
            map["melody-note-rain"] = () => new NoteRain();
            map["orchestra-damage-note"] = () => new OrchestraNote();
        }

        private static void NoteAt(FxBatch b, Vector2 p, float size, float deg, Color c, float alpha, int kind, int seed)
        {
            if (alpha < .004f) return;
            b.Quad(FxShape.Glow, p, size * 1.4f, A(c, .3f * alpha), 0, seed);
            b.Quad(DKit.Note, p, size, deg, A(c, 1.2f * alpha), 0, seed, kind);
        }

        /// <summary>Pastel confetti flecks that pop out, flutter and drift down.</summary>
        private static void Confetti(FxBatch b, Vector2 at, float t, int n, float spread, int seed, float life = .8f)
        {
            if (t <= 0) return;
            for (int k = 0; k < n; k++)
            {
                float lf = life * (.7f + .5f * H(seed, k + 70)), u = t / lf; if (u <= 0 || u >= 1) continue;
                Vector2 d = Dir(seed, k + 70, n, .8f); float s = u * lf;
                Vector2 p = at + d * spread * EaseOut(u) + new Vector2(Mathf.Sin(u * 12 + k) * .15f, 2.2f * s - 2.4f * s * s);
                b.Quad(FxShape.Disc, p, new Vector2(.09f, .05f + .04f * Mathf.Abs(Mathf.Sin(u * 14 + k))), k * 50 + u * 600, A(DKit.Pastel(k + seed), .95f * (1 - u * u)), 0, seed);
            }
        }

        /// <summary>One pastel note is thrown in a high arc, tumbling, a wavy ribbon of staff behind it and sparkles falling off; it bonks
        /// into the crowd, squashes with comic bonk lines, hops away as five tiny notes and pops a burst of pastel confetti.</summary>
        private sealed class Notes : FxEffect
        {
            protected override float Life => .95f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; Color c = DKit.Pastel(Seed); int kind = Seed % 3;
                if (t < 0)
                {
                    Vector2 prev = DKit.Arc(From, at, Mathf.Max(0, pre - .3f), 2.2f);
                    for (int k = 1; k <= 8; k++)
                    {
                        float v = Mathf.Max(0, pre - .3f + k * .0375f); Vector2 q = DKit.Arc(From, at, v, 2.2f) + Vector2.up * Mathf.Sin(v * 30) * .12f;
                        b.Link(prev, q, .05f, A(c, .7f * k / 8f), Seed + k, .7f); prev = q;
                    }
                    for (int k = 1; k <= 6; k++) { float v = Mathf.Max(0, pre - k * .05f); Glint(b, DKit.Arc(From, at, v, 2.2f) + Vector2.down * (k * .08f), .18f * (1 - k / 7f), DKit.Pastel(Seed + k), .9f * (1 - k / 7f), k * 55); }
                    NoteAt(b, DKit.Arc(From, at, pre, 2.2f), .8f, Mathf.Sin(pre * 14) * 30, c, 1, kind, Seed);
                    b.Floor(FxShape.Shade, at, .6f * pre, A(DKit.Dark, .35f * pre), 0, Seed);
                    return;
                }
                float squash = Fade(t, 0, .2f);
                Vector2 hop = at + Vector2.up * (.15f + .2f * Mathf.Abs(Mathf.Sin(t * 14)) * Fade(t, 0, .5f));
                b.Quad(DKit.Note, hop, new Vector2(.8f * (1 + .4f * squash), .8f * (1 - .3f * squash)), 0, A(c, 1.2f * Fade(t, .45f, .9f)), 0, Seed, kind);
                b.Quad(FxShape.Glow, hop, 1.0f, A(c, .35f * Fade(t, .3f, .9f)), 0, Seed);
                for (int k = 0; k < 6; k++)
                {
                    float a = (k * 30 + 15) * Mathf.Deg2Rad, l = DKit.Bump(t, 0, .3f); Vector2 d = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                    b.Link(hop + d * .7f, hop + d * (.7f + .45f * l), .05f, A(White, .9f * l), Seed + k, .8f);
                }
                b.Quad(DKit.Pulse, at, DKit.Sq(Mathf.Max(1f, Radius) * 1.6f), 0, A(c, 1.1f), Rise(t, 0, .6f), Seed, 14);
                b.Floor(FxShape.Glow, at, 1.0f, A(DKit.Butter, .45f * Fade(t, 0, .35f)), 0, Seed);
                for (int k = 0; k < 5; k++)
                {
                    float u = t / (.55f + .05f * k); if (u <= 0 || u >= 1) continue;
                    NoteAt(b, at + new Vector2((k - 2) * .5f * EaseOut(u), 1.8f * u - 2f * u * u + .1f), .36f, (k - 2) * 25 + Mathf.Sin(u * 10) * 15, DKit.Pastel(Seed + k + 1), 1 - u * u, (k + Seed) % 4, Seed + k);
                }
                Confetti(b, at + Vector2.up * .3f, t, 12, 1.2f, Seed);
            }
        }

        /// <summary>Notes spiral in from four sides, a speaker-cone thrums in the middle (fast tight rings), then four curved fans of sound
        /// arcs, one per compass point in its own pastel, travel out across the crowd kicking a low dust wave; a second, fainter wave train
        /// follows a beat later and little notes ride the wave.</summary>
        private sealed class SoundWave : FxEffect
        {
            protected override float Life => 1.4f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Mathf.Max(1.5f, Radius) * 1.0f;
                if (t < 0)
                {
                    for (int k = 0; k < 8; k++)
                    {
                        float a = k / 8f * 6.2832f + pre * 4, rad = r * 1.5f * (1 - EaseOut(pre));
                        Vector2 p = at + new Vector2(Mathf.Cos(a), Mathf.Sin(a) * .62f) * rad;
                        NoteAt(b, p, .34f, 0, DKit.Pastel(k), .9f * Mathf.Min(1, pre * 4), k % 4, Seed + k);
                        Glint(b, p + new Vector2(Mathf.Sin(a), -Mathf.Cos(a) * .62f) * .35f, .12f, DKit.Pastel(k), .8f * pre, k * 30);
                    }
                    b.Floor(FxShape.Glow, at, r * .7f * pre, A(DKit.Lilac, .3f * pre), 0, Seed);
                    return;
                }
                // the speaker cone: tight fast rings at the centre
                for (int k = 0; k < 3; k++) { float u = (t * 3.2f + k / 3f) % 1f; b.Floor(FxShape.Shock, at, .3f + .6f * u, A(White, .5f * (1 - u) * Fade(t, .5f, .9f)), .3f, Seed + k); }
                for (int w = 0; w < 2; w++)
                {
                    float u = Rise(t, w * .28f, 1.0f + w * .28f), fade = Fade(t, .45f + w * .28f, 1.25f + w * .15f) * (w == 0 ? 1 : .55f);
                    for (int k = 0; k < 4; k++)
                    {
                        float ang = k * 90 + 45 + w * 12; Vector2 d = new Vector2(Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad));
                        float size = r * .95f;
                        b.Quad(DKit.Arcs, at + d * size * .93f, new Vector2(size, size * .8f), ang, A(DKit.Pastel(k + w), .95f * fade), u, Seed + k + w * 5, 1.0f);
                    }
                }
                b.Floor(FxShape.Glow, at, r * 1.0f, A(DKit.Butter, .3f * Fade(t, 0, .35f)), 0, Seed);
                b.Floor(FxShape.Shock, at, r * 1.4f, A(DKit.Pink, .7f), Rise(t, 0, .9f), Seed);
                DKit.DustRing(b, at, r * 1.2f, t, DKit.DustBrown, Seed, 10, 1.0f, .55f);
                for (int k = 0; k < 10; k++)
                {
                    float v = Rise(t, k * .03f, .9f + k * .02f); if (v <= 0 || v >= 1) continue;
                    Vector2 dir = Dir(Seed + 5, k, 10, .62f);
                    NoteAt(b, at + dir * r * 1.4f * EaseOut(v) + Vector2.up * Mathf.Sin(v * Mathf.PI) * .7f, .3f, Mathf.Sin(v * 9 + k) * 25, DKit.Pastel(k), 1 - v * v, k % 4, Seed + k);
                }
                for (int k = 0; k < 12; k++)
                {
                    float v = Rise(t, .05f + k * .02f, .7f + k * .02f); if (v <= 0 || v >= 1) continue;
                    Glint(b, at + Dir(Seed + 9, k, 12, .62f) * r * 1.6f * EaseOut(v), .14f, White, 1 - v, k * 30);
                }
            }
        }

        /// <summary>A giant drum drops onto the crowd in a cloud of dust (only the first of the three instances draws it); each of the three
        /// beats squashes the skin, flashes a new pastel, throws a dashed pulse ring and dust off the drum's foot, sparks fly off the sticks
        /// and the skin keeps rippling between beats.</summary>
        private sealed class WarDrum : FxEffect
        {
            private int Index => Mathf.Max(0, Mathf.RoundToInt((Moment.Delay - .4f) / .4f));
            protected override float Life => Index == 0 ? 1.7f : .9f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Mathf.Max(2f, Radius) * .9f; int idx = Index;
                if (idx == 0) Drum(b, at, r, t, pre, now);
                if (t < 0) return;
                Color c = DKit.Pastel(idx * 2 + 1);
                float hit = Fade(t, 0, .16f);
                b.Quad(FxShape.Glow, at, r * 1.2f, A(c, .4f * hit), 0, Seed);
                b.Quad(DKit.Pulse, at, DKit.Sq(r * 1.45f), 0, A(c, 1.2f), Rise(t, 0, .7f), Seed + idx, 16 + idx * 2);
                b.Quad(DKit.Pulse, at, DKit.Sq(r * 1.15f), 0, A(DKit.Ice, .8f), Rise(t, .05f, .6f), Seed + idx + 7, 10);
                b.Floor(FxShape.Shock, at, r * 1.3f, A(c, .8f), Rise(t, 0, .6f), Seed);
                DKit.DustRing(b, at, r * .9f, t, DKit.DustBrown, Seed + idx * 13, 8, .8f, .6f);
                float top = r * .72f * .45f * 2 + .2f;
                DKit.Sparks(b, at + Vector2.up * top, t, 8, 3.5f, 3f, DKit.Butter, Seed + idx * 7, .45f, .09f);
                for (int k = 0; k < 3; k++)
                {
                    float u = Rise(t, k * .05f, .75f); if (u <= 0 || u >= 1) continue;
                    NoteAt(b, at + Dir(Seed + idx * 3, k, 3, .62f) * r * 1.0f * EaseOut(u) + Vector2.up * (top + 1.0f * Mathf.Sin(u * 3)), .36f, 0, DKit.Pastel(idx + k), 1 - u, (k + idx) % 4, Seed + k);
                }
            }

            private void Drum(FxBatch b, Vector2 at, float r, float t, float pre, float now)
            {
                float h = t < 0 ? (1 - DKit.EaseIn(pre)) * 6 : 0, alpha = Fade(t, 1.2f, 1.7f);
                float squash = 0; for (int j = 0; j < 3; j++) if (t >= j * .4f) squash = Mathf.Max(squash, Fade(t - j * .4f, 0, .2f));
                float dr = r * .72f * (1 + .06f * squash), hh = dr * .45f;
                Vector2 c = at + Vector2.up * (h + hh + .2f), top = c + Vector2.up * hh * (1 - .25f * squash), bot = c - Vector2.up * hh;
                b.Floor(FxShape.Shade, at, dr * 1.4f * (t < 0 ? pre : 1), A(DKit.Dark, .6f * alpha), 0, Seed);
                b.Quad(FxShape.Shade, c, new Vector2(dr * 1.4f, hh * 2.5f), 0, A(new Color(.08f, .02f, .06f), alpha), 0, Seed);
                b.Quad(FxShape.Shade, c, new Vector2(dr * 1.2f, hh * 2.1f), 0, A(new Color(.12f, .03f, .08f), alpha), 0, Seed + 1);
                Color red = new Color(1f, .35f, .45f), gold = DKit.Butter;
                b.Quad(FxShape.Disc, bot, new Vector2(dr, dr * .5f), 0, A(red, .3f * alpha), 0, Seed);
                b.Link(top + Vector2.left * dr, bot + Vector2.left * dr, .07f, A(red, .9f * alpha), Seed, .5f);
                b.Link(top + Vector2.right * dr, bot + Vector2.right * dr, .07f, A(red, .9f * alpha), Seed + 1, .5f);
                for (int k = 0; k < 8; k++)
                {
                    float a0 = Mathf.PI + k / 8f * Mathf.PI, a1 = Mathf.PI + (k + 1) / 8f * Mathf.PI;
                    Vector2 p0 = new Vector2(Mathf.Cos(a0) * dr, Mathf.Sin(a0) * dr * .5f), p1 = new Vector2(Mathf.Cos(a1) * dr, Mathf.Sin(a1) * dr * .5f);
                    b.Link((k % 2 == 0 ? top : bot) + p0, (k % 2 == 0 ? bot : top) + p1, .05f, A(gold, .95f * alpha), Seed + k, .5f);
                    b.Link(bot + p0, bot + p1, .06f, A(red, .9f * alpha), Seed + k, .5f);
                    Glint(b, bot + p0, .12f, gold, .8f * alpha, k * 20);
                }
                b.Quad(FxShape.Disc, top, new Vector2(dr, dr * .5f), 0, A(new Color(1f, .85f, .7f), (.32f + .2f * squash) * alpha), 0, Seed);
                b.Quad(FxShape.Shock, top, new Vector2(dr * 1.05f, dr * .53f), 0, A(gold, 1.1f * alpha), .3f, Seed);
                // the skin ripples between beats
                if (t >= 0) for (int k = 0; k < 2; k++) { float u = (t * 2.5f + k * .5f) % 1f; b.Quad(FxShape.Shock, top, new Vector2(dr * .9f * u + .1f, dr * .45f * u + .05f), 0, A(DKit.Pink, .6f * (1 - u) * alpha), .3f, Seed + k); }
                b.Quad(DKit.Pulse, top, new Vector2(dr * 1.25f, dr * .63f), 0, A(DKit.Pink, 1.2f * alpha * squash), 1 - squash, Seed, 10);
                float swing = Mathf.Abs(Mathf.Sin(Mathf.Max(0, t) * 7.85f)) * (t >= 0 && t < 1.25f ? 1 : 0), sa = alpha * (t >= 0 ? 1 : 0);
                b.Link(top + new Vector2(-dr * .9f, 2.2f - swing * 1.5f), top + new Vector2(-dr * .15f, .1f), .1f, A(gold, sa), Seed, .5f);
                b.Link(top + new Vector2(dr * .9f, 2.2f - (1 - swing) * 1.5f), top + new Vector2(dr * .15f, .1f), .1f, A(gold, sa), Seed + 1, .5f);
                // the landing: a dust cloud rolls out from under the drum
                if (t >= 0) { DKit.DustRing(b, at, dr * 1.1f, t, DKit.DustBrown, Seed + 77, 10, 1.1f, .7f); FxKit.SmokeColumn(b, at + Vector2.up * .2f, dr * .6f, 1.2f, t * 1.2f, new Color(.45f, .38f, .34f), Seed + 5, 4); }
            }
        }

        /// <summary>A short musical staff of five pastel lines is written over the spot with a sharp sign at its head, and notes pour from
        /// above onto it trailing sparkle streaks; each lands on a line with a ripple and a puff of pastel dust, then dances in place.</summary>
        private sealed class NoteRain : FxEffect
        {
            protected override float Life => 1.5f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float age = now - Born, w = 2.6f * Mathf.Max(1f, Radius), fade = Fade(t, .9f, 1.5f);
                float write = EaseOut(Rise(age, 0, .35f));
                for (int k = 0; k < 5; k++)
                {
                    Vector2 a = at + new Vector2(-w * .5f, (k - 2) * .3f), z = a + new Vector2(w * write, 0);
                    b.Beam(a, z, .06f, A(DKit.Pastel(k), 1.1f * fade), 0, Seed + k, .3f);
                    if (write < 1) Glint(b, z, .2f, White, .9f, age * 300 + k * 30);
                }
                b.Quad(DKit.Note, at + new Vector2(-w * .5f - .25f, 0), .5f, 0, A(DKit.Lilac, 1.1f * fade * write), 0, Seed, 3);
                b.Floor(FxShape.Glow, at, w * .5f, A(DKit.Lilac, .25f * write * fade), 0, Seed);
                for (int k = 0; k < 8; k++)
                {
                    float x = at.x + (k / 7f - .5f) * w * .9f; int ln = Mathf.RoundToInt(H(Seed, k) * 4 - 2); float y = at.y + ln * .3f;
                    float start = .05f + k * .07f, fall = .4f, u = (age - start) / fall;
                    if (u <= 0) continue;
                    Color c = DKit.Pastel(k + Seed);
                    if (u < 1)
                    {
                        float hgt = 4.5f * (1 - DKit.EaseIn(u));
                        NoteAt(b, new Vector2(x, y + .5f + hgt), .55f, Mathf.Sin(age * 9 + k) * 15, c, 1, k % 4, Seed + k);
                        b.Beam(new Vector2(x, y + .9f + hgt), new Vector2(x, y + 1.9f + hgt + u), .05f, A(c, .6f), 0, Seed + k, .5f);
                        Glint(b, new Vector2(x + .15f, y + 1.3f + hgt), .14f, White, .8f, k * 20);
                    }
                    else
                    {
                        float s = age - start - fall; float bounce = Mathf.Abs(Mathf.Sin(s * 9)) * .3f * Fade(s, 0, 1f);
                        NoteAt(b, new Vector2(x, y + .5f + bounce), .55f, Mathf.Sin(s * 9) * 12, c, fade * Fade(s, .6f, 1.2f), k % 4, Seed + k);
                        float rp = Rise(s, 0, .4f); if (rp > 0 && rp < 1) { b.Quad(FxShape.Shock, new Vector2(x, y), DKit.Sq(.7f), 0, A(c, .9f), rp, Seed + k); b.Quad(FxShape.Dust, new Vector2(x, y + .1f + .2f * rp), new Vector2(.35f + .3f * rp, .2f + .12f * rp), 0, A(Color.Lerp(c, White, .4f), .55f * (1 - rp)), rp, Seed + k); }
                        if (s < .35f) for (int j = 0; j < 3; j++) Glint(b, new Vector2(x, y + .2f) + Dir(Seed + k, j, 3, .7f) * .5f * EaseOut(s / .35f), .1f, c, 1 - s / .35f, j * 60);
                    }
                }
            }
        }

        /// <summary>The orchestra's damage note: a white baton sweeps a looping pastel ribbon through the air over the crowd, then three
        /// dotted conducting rings land on it with a flash, a fan of notes is flung up and confetti rains back down.</summary>
        private sealed class OrchestraNote : FxEffect
        {
            protected override float Life => 1.2f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                if (t < 0) return; Vector2 at = At; float r = Mathf.Max(1.1f, Radius) * .95f;
                // the baton conducts a figure-of-eight; the ribbon is its recent path
                float cond = Rise(t, 0, .35f), bf = Fade(t, .35f, .55f);
                Vector2 Path(float v) => at + new Vector2(Mathf.Sin(v * 6.2832f) * r * .9f, Mathf.Sin(v * 12.566f) * r * .35f + 1.4f);
                if (bf > 0)
                {
                    Vector2 prev = Path(Mathf.Max(0, cond - .5f));
                    for (int k = 1; k <= 12; k++) { float v = Mathf.Max(0, cond - .5f + k / 24f); Vector2 p = Path(v); b.Link(prev, p, .16f, A(DKit.Pastel(k / 3), .9f * bf * k / 12f), Seed + k, .4f); if (k % 3 == 0) Glint(b, p, .16f, White, bf * k / 12f, k * 30); prev = p; }
                    Vector2 tip = Path(cond), hand = tip + new Vector2(-.6f, -.9f);
                    b.Link(hand, tip, .05f, A(White, bf), Seed, .8f); Glint(b, tip, .4f, White, bf, t * 600);
                }
                float land = Rise(t, .3f, .42f), s = t - .38f;
                if (s < 0) return;
                b.Quad(FxShape.Glow, at, r * 1.5f, A(DKit.Butter, .55f * Fade(s, 0, .3f)), 0, Seed);
                for (int k = 0; k < 3; k++)
                {
                    float u = Rise(s, k * .08f, .6f + k * .08f); if (u <= 0 || u >= 1) continue;
                    b.Quad(DKit.Pulse, at, DKit.Sq(r * (1.0f + k * .3f)), k * 20, A(DKit.Pastel(k * 2), 1.1f * land), u, Seed + k, 8 + k * 3);
                }
                for (int k = 0; k < 6; k++)
                {
                    float u = Rise(s, k * .04f, .75f); if (u <= 0 || u >= 1) continue;
                    NoteAt(b, at + new Vector2((k - 2.5f) * r * .4f, 0) + Vector2.up * (EaseOut(u) * 1.9f - .8f * u * u), .45f, (k - 2.5f) * 10, DKit.Pastel(k), 1 - u * u, k % 4, Seed + k);
                }
                Confetti(b, at + Vector2.up * .5f, s, 16, 1.5f, Seed + 3, .8f);
                DKit.Sparks(b, at + Vector2.up * .3f, s, 8, 3f, 2.8f, DKit.Butter, Seed, .45f, .08f);
            }
        }
    }
}
