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
            b.Quad(FxShape.Glow, p, size * 1.5f, A(c, .3f * alpha), 0, seed);
            b.Quad(DKit.Note, p, size, deg, A(c, 1.2f * alpha), 0, seed, kind);
        }

        /// <summary>One pastel note is thrown in a high arc, tumbling and shedding sparkles; it bonks into the crowd, squashes, and hops
        /// away as five tiny notes while a dotted pulse ring ticks.</summary>
        private sealed class Notes : FxEffect
        {
            protected override float Life => .95f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; Color c = DKit.Pastel(Seed); int kind = Seed % 3;
                if (t < 0)
                {
                    for (int k = 1; k <= 6; k++) { float v = Mathf.Max(0, pre - k * .05f); Glint(b, DKit.Arc(From, at, v, 2.4f), .2f * (1 - k / 7f), DKit.Pastel(Seed + k), .9f * (1 - k / 7f), k * 55); }
                    NoteAt(b, DKit.Arc(From, at, pre, 2.4f), .95f, Mathf.Sin(pre * 14) * 30, c, 1, kind, Seed);
                    return;
                }
                float squash = Fade(t, 0, .2f);
                NoteAt(b, at + Vector2.up * (.15f + .2f * Mathf.Abs(Mathf.Sin(t * 14)) * Fade(t, 0, .5f)), .95f * (1 + .35f * squash), 0, c, Fade(t, .45f, .9f), kind, Seed);
                b.Quad(DKit.Pulse, at, DKit.Sq(Mathf.Max(1.2f, Radius) * 2.2f), 0, A(c, 1.1f), Rise(t, 0, .6f), Seed, 14);
                b.Quad(FxShape.Rays, at, 2.2f, A(c, .7f * Fade(t, 0, .4f)), t, Seed, 3);
                b.Floor(FxShape.Glow, at, 1.3f, A(DKit.Butter, .5f * Fade(t, 0, .35f)), 0, Seed);
                for (int k = 0; k < 5; k++)
                {
                    float u = t / (.55f + .05f * k); if (u <= 0 || u >= 1) continue;
                    NoteAt(b, at + new Vector2((k - 2) * .55f * EaseOut(u), 2.0f * u - 2.2f * u * u + .1f), .42f, (k - 2) * 25, DKit.Pastel(Seed + k + 1), 1 - u * u, (k + Seed) % 4, Seed + k);
                }
                for (int k = 0; k < 8; k++) { float u = t / .45f; if (u > 0 && u < 1) Glint(b, at + Dir(Seed, k, 8, .6f) * 1.1f * EaseOut(u), .22f, k % 2 == 0 ? White : DKit.Butter, 1 - u, k * 45 + t * 200); }
            }
        }

        /// <summary>Notes spiral in from four sides, then four curved fans of sound arcs, one per compass point and each in its own
        /// pastel colour, travel out across the crowd; a wobbling ring and little notes ride the wave.</summary>
        private sealed class SoundWave : FxEffect
        {
            protected override float Life => 1.3f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Mathf.Max(1.8f, Radius) * 1.3f;
                if (t < 0)
                {
                    for (int k = 0; k < 8; k++)
                    {
                        float a = k / 8f * 6.2832f + pre * 4, rad = r * 1.6f * (1 - EaseOut(pre));
                        NoteAt(b, at + new Vector2(Mathf.Cos(a), Mathf.Sin(a) * .62f) * rad, .38f, 0, DKit.Pastel(k), .9f * Mathf.Min(1, pre * 4), k % 4, Seed + k);
                    }
                    b.Floor(FxShape.Glow, at, r * .8f * pre, A(DKit.Lilac, .3f * pre), 0, Seed);
                    return;
                }
                float u = Rise(t, 0, 1.0f), fade = Fade(t, .45f, 1.25f);
                for (int k = 0; k < 4; k++)
                {
                    float ang = k * 90 + 45; Vector2 d = new Vector2(Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad));
                    float size = r * 1.1f;
                    b.Quad(DKit.Arcs, at + d * size * .93f, new Vector2(size, size * .8f), ang, A(DKit.Pastel(k), 1.2f * fade), u, Seed + k, 1.0f);
                }
                b.Floor(FxShape.Glow, at, r * 1.4f, A(DKit.Butter, .55f * Fade(t, 0, .35f)), 0, Seed);
                b.Floor(FxShape.Shock, at, r * 1.55f, A(DKit.Pink, .7f), Rise(t, 0, .9f), Seed);
                for (int k = 0; k < 10; k++)
                {
                    float v = Rise(t, k * .03f, .9f + k * .02f); if (v <= 0 || v >= 1) continue;
                    Vector2 dir = Dir(Seed + 5, k, 10, .62f);
                    NoteAt(b, at + dir * r * 1.5f * EaseOut(v) + Vector2.up * Mathf.Sin(v * Mathf.PI) * .8f, .34f, Mathf.Sin(v * 9 + k) * 25, DKit.Pastel(k), 1 - v * v, k % 4, Seed + k);
                }
            }
        }

        /// <summary>A giant drum drops onto the crowd (only the first of the three instances draws it) and each of the three beats
        /// squashes the skin, flashes a new pastel and throws dashed pulse rings and arcs across the floor.</summary>
        private sealed class WarDrum : FxEffect
        {
            private int Index => Mathf.Max(0, Mathf.RoundToInt((Moment.Delay - .4f) / .4f));
            protected override float Life => Index == 0 ? 1.7f : .9f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Mathf.Max(2.2f, Radius) * 1.1f; int idx = Index;
                if (idx == 0) Drum(b, at, r, t, pre, now);
                if (t < 0) return;
                Color c = DKit.Pastel(idx * 2 + 1);
                float hit = Fade(t, 0, .16f);
                b.Quad(FxShape.Glow, at, r * 1.8f, A(c, .4f * hit), 0, Seed);
                b.Quad(DKit.Pulse, at, DKit.Sq(r * 1.9f), 0, A(c, 1.2f), Rise(t, 0, .7f), Seed + idx, 16 + idx * 2);
                b.Quad(DKit.Pulse, at, DKit.Sq(r * 1.55f), 0, A(DKit.Ice, .8f), Rise(t, .05f, .6f), Seed + idx + 7, 10);
                b.Floor(FxShape.Shock, at, r * 1.7f, A(c, .8f), Rise(t, 0, .6f), Seed);
                for (int k = 0; k < 3; k++)
                {
                    float u = Rise(t, k * .05f, .75f); if (u <= 0 || u >= 1) continue;
                    NoteAt(b, at + Dir(Seed + idx * 3, k, 3, .62f) * r * 1.3f * EaseOut(u) + Vector2.up * 1.4f * Mathf.Sin(u * 3), .4f, 0, DKit.Pastel(idx + k), 1 - u, (k + idx) % 4, Seed + k);
                }
            }

            private void Drum(FxBatch b, Vector2 at, float r, float t, float pre, float now)
            {
                float h = t < 0 ? (1 - DKit.EaseIn(pre)) * 7 : 0, alpha = Fade(t, 1.2f, 1.7f);
                float squash = 0; for (int j = 0; j < 3; j++) if (t >= j * .4f) squash = Mathf.Max(squash, Fade(t - j * .4f, 0, .2f));
                float dr = r * .72f * (1 + .06f * squash), hh = dr * .45f;
                Vector2 c = at + Vector2.up * (h + hh + .2f), top = c + Vector2.up * hh * (1 - .25f * squash), bot = c - Vector2.up * hh;
                b.Floor(FxShape.Shade, at, dr * 1.5f, A(DKit.Dark, .6f * alpha), 0, Seed);
                b.Quad(FxShape.Shade, c, new Vector2(dr * 1.45f, hh * 2.6f), 0, A(new Color(.08f, .02f, .06f), alpha), 0, Seed);
                b.Quad(FxShape.Shade, c, new Vector2(dr * 1.25f, hh * 2.2f), 0, A(new Color(.12f, .03f, .08f), alpha), 0, Seed + 1);
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
                }
                b.Quad(FxShape.Disc, top, new Vector2(dr, dr * .5f), 0, A(new Color(1f, .85f, .7f), (.32f + .2f * squash) * alpha), 0, Seed);
                b.Quad(FxShape.Shock, top, new Vector2(dr * 1.05f, dr * .53f), 0, A(gold, 1.1f * alpha), .3f, Seed);
                b.Quad(DKit.Pulse, top, new Vector2(dr * 1.3f, dr * .66f), 0, A(DKit.Pink, 1.2f * alpha * squash), 1 - squash, Seed, 10);
                float swing = Mathf.Abs(Mathf.Sin(Mathf.Max(0, t) * 7.85f)) * (t >= 0 && t < 1.25f ? 1 : 0), sa = alpha * (t >= 0 ? 1 : 0);
                b.Link(top + new Vector2(-dr * .9f, 2.4f - swing * 1.6f), top + new Vector2(-dr * .15f, .1f), .1f, A(gold, sa), Seed, .5f);
                b.Link(top + new Vector2(dr * .9f, 2.4f - (1 - swing) * 1.6f), top + new Vector2(dr * .15f, .1f), .1f, A(gold, sa), Seed + 1, .5f);
            }
        }

        /// <summary>A musical staff of five pastel lines is written across the crowd and notes pour from the sky onto it, each landing
        /// on a line with a ripple and then dancing in place.</summary>
        private sealed class NoteRain : FxEffect
        {
            protected override float Life => 1.5f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float age = now - Born, w = 3.4f * Mathf.Max(1.2f, Radius), fade = Fade(t, .9f, 1.5f);
                float write = EaseOut(Rise(age, 0, .35f));
                for (int k = 0; k < 5; k++)
                {
                    Vector2 a = at + new Vector2(-w * .5f, (k - 2) * .36f), z = a + new Vector2(w * write, 0);
                    b.Beam(a, z, .07f, A(DKit.Pastel(k), 1.1f * fade), 0, Seed + k, .3f);
                }
                b.Floor(FxShape.Glow, at, w * .55f, A(DKit.Lilac, .3f * write * fade), 0, Seed);
                for (int k = 0; k < 10; k++)
                {
                    float x = at.x + (k / 9f - .5f) * w * .92f, line = (H(Seed, k) * 5 - 2.5f); int ln = Mathf.RoundToInt(line); float y = at.y + ln * .36f;
                    float start = .05f + k * .07f, fall = .5f, u = (age - start) / fall;
                    if (u <= 0) continue;
                    Color c = DKit.Pastel(k + Seed);
                    if (u < 1) { NoteAt(b, new Vector2(x, y + 7 * (1 - DKit.EaseIn(u))), .75f, Mathf.Sin(age * 9 + k) * 15, c, 1, k % 4, Seed + k); Glint(b, new Vector2(x, y + 7.8f * (1 - DKit.EaseIn(u))), .2f, c, .8f, 0); }
                    else
                    {
                        float s = age - start - fall; float bounce = Mathf.Abs(Mathf.Sin(s * 9)) * .35f * Fade(s, 0, 1f);
                        NoteAt(b, new Vector2(x, y + .6f + bounce), .75f, Mathf.Sin(s * 9) * 12, c, fade * Fade(s, .6f, 1.2f), k % 4, Seed + k);
                        float rp = Rise(s, 0, .4f); if (rp > 0 && rp < 1) b.Quad(FxShape.Shock, new Vector2(x, y), DKit.Sq(.9f), 0, A(c, .9f), rp, Seed + k);
                    }
                }
            }
        }

        /// <summary>The orchestra's damage note: a big wavy treble swirl of three conducting arcs lands on the crowd with a baton flash.</summary>
        private sealed class OrchestraNote : FxEffect
        {
            protected override float Life => 1.1f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                if (t < 0) return; Vector2 at = At; float r = Mathf.Max(1.4f, Radius) * 1.3f;
                b.Quad(FxShape.Glow, at, r * 1.8f, A(DKit.Butter, .6f * Fade(t, 0, .3f)), 0, Seed);
                for (int k = 0; k < 3; k++)
                {
                    float u = Rise(t, k * .08f, .7f + k * .08f); if (u <= 0 || u >= 1) continue;
                    b.Quad(DKit.Pulse, at, DKit.Sq(r * (1.3f + k * .35f)), k * 20, A(DKit.Pastel(k * 2), 1.1f), u, Seed + k, 8 + k * 3);
                }
                for (int k = 0; k < 6; k++)
                {
                    float u = Rise(t, k * .04f, .9f); if (u <= 0 || u >= 1) continue;
                    NoteAt(b, at + new Vector2((k - 2.5f) * r * .5f, 0) + Vector2.up * (EaseOut(u) * 2.4f - 1f * u * u), .6f, 0, DKit.Pastel(k), 1 - u * u, k % 4, Seed + k);
                }
            }
        }
    }
}
