using System;
using System.Collections.Generic;
using UnityEngine;
using static ArcaneBrigade.FxKit;
using static ArcaneBrigade.AKit;

namespace ArcaneBrigade
{
    /// <summary>Ember bond: flame tongues, embers, magma and heat glow.</summary>
    public static class EmberFx
    {
        public static void Register(Dictionary<string, Func<FxEffect>> map)
        {
            map["burn-burst"] = () => new BurnBurst();
            map["inferno-burst"] = () => new InfernoBurst();
            map["ember-meteor"] = () => new Meteor();
            map["phoenix-dive"] = () => new Phoenix();
        }
        private static readonly Color Fire = new Color(1f, .5f, .1f), Gold = new Color(1f, .82f, .25f), Red = new Color(.95f, .16f, .05f), Soot = new Color(.05f, .02f, .02f), Lava = new Color(1f, .4f, .05f);

        /// <summary>A burning enemy pops: a tall geyser of flame with a yellow heart and fanned side tongues, a flash, soot, and embers drifting up.</summary>
        private sealed class BurnBurst : FxEffect
        {
            protected override float Life => 1.15f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                float r = Mathf.Clamp(Radius, 1.2f, 2.3f), up = EaseOut3(Rise(t, 0, .2f)), fade = Fade(t, .4f, 1.05f); Vector2 at = At;
                b.Floor(FxShape.Shade, at, r * 1.3f, A(Soot, .6f * Fade(t, .3f, 1.1f)), 0, Seed);
                b.Floor(FxShape.Glow, at, r * 1.5f, A(Red, .6f * Fade(t, 0, .9f)), 0, Seed);
                b.Quad(FxShape.Glow, at + Vector2.up * .6f, r * 1.5f, A(Fire, .7f * Fade(t, 0, .55f)), 0, Seed);
                for (int k = 0; k < 4; k++)
                {
                    float u = Rise(t, .18f + k * .08f, 1.1f + k * .05f); if (u <= 0 || u >= 1) continue;
                    Puff(b, at + new Vector2((k - 1.5f) * r * .25f + Mathf.Sin(u * 4 + k) * .15f, r * (.9f + 1.3f * u)), r * (.35f + .35f * u), new Color(.16f, .1f, .08f), .6f * Mathf.Sin(u * 3.14f), u, Seed + 40 + k);
                }
                for (int k = 0; k < 10; k++)
                {
                    Vector2 dd = Dir(Seed + 2, k, 10, .7f);
                    Fleck(b, at + Vector2.up * r * .4f, new Vector2(dd.x * 3.2f, 3.5f + 2.5f * H(Seed, k + 60)), 9, t - .28f, .7f, .09f, k % 3 == 0 ? White : Gold, Seed + k);
                }
                for (int k = 0; k < 8; k++)
                {
                    float u = (t * .8f + H(Seed, k + 90)) % 1; if (t < .3f) continue;
                    Vector2 p = at + new Vector2((H(Seed, k + 91) - .5f) * r * 1.6f + Mathf.Sin(t * 3 + k) * .3f, r * .6f + u * 2.2f);
                    Solid(b, p, new Vector2(.08f, .05f), t * 200 + k * 40, new Color(.18f, .12f, .1f), (1 - u) * Fade(t, .8f, 1.15f));

                }
                b.Quad(FxShape.Glow, at + Vector2.up * .5f, r * 1.1f, A(White, .7f * Fade(t, 0, .12f)), 0, Seed);
                Tongue(b, at, 0, r * 2.1f * up, r * .5f, Fire, fade, t, Seed, 1);
                Tongue(b, at, 0, r * 1.5f * up, r * .3f, Gold, fade, t + .3f, Seed + 3, 2);
                for (int k = 0; k < 6; k++)
                {
                    float side = k % 2 == 0 ? -1 : 1, lean = side * (26 + 15 * (k / 2)), idx = k / 2;
                    Tongue(b, at + new Vector2(side * (idx + 1) * r * .4f, -.05f * k), lean, r * (1.25f - .15f * idx) * up, r * .3f, k % 3 == 0 ? Gold : Fire, fade * .95f, t + k * .1f, Seed + 10 + k);
                }
                for (int k = 0; k < 16; k++)
                {
                    float life = .6f + .5f * H(Seed, k), u = t / life; if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + new Vector2((H(Seed, k + 20) - .5f) * r * 1.4f + Mathf.Sin(t * 9 + k) * .2f, .3f + u * (1.8f + 1.8f * H(Seed, k + 40)));
                    Glint(b, p, .22f * (1 - u * .5f), k % 3 == 0 ? Gold : Fire, 1 - u, k * 30 + t * 200);
                }
                b.Quad(FxShape.Smoke, at + Vector2.up * (.8f + t * 1.5f), r * .6f * (1 + t), A(Soot, .55f * Fade(t, .15f, 1f)), t, Seed);
            }
        }

        /// <summary>Burning spreads: three rolling waves of ground flame tongues race out over the crowd's footprint, a hot white flash, lava veins in the floor, embers thrown in arcs.</summary>
        private sealed class InfernoBurst : FxEffect
        {
            protected override float Life => 1.9f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                float r = Mathf.Clamp(Radius, 1.4f, 3.2f); Vector2 at = At;
                b.Floor(FxShape.Shade, at, r * 1.5f, A(Soot, .55f * Fade(t, .5f, 1.8f)), 0, Seed);
                b.Floor(FxShape.Glow, at, r * 1.9f, A(Red, .65f * Fade(t, 0, 1.5f)), 0, Seed);
                b.Quad(FxShape.Cracks, at, new Vector2(r * 1.4f, r * .9f), 0, A(Lava, .7f * Fade(t, .5f, 1.8f)), 1, Seed, 2);
                b.Quad(FxShape.Glow, at + Vector2.up * .4f, r * 1.2f, A(White, .85f * Fade(t, 0, .18f)), 0, Seed);
                Grit(b, at, r * 1.3f, new Color(.1f, .06f, .05f), .45f * Rise(t, .4f, .9f) * Fade(t, 1.3f, 1.9f), t, Seed + 3);
                for (int w = 0; w < 3; w++)
                {
                    float t0 = w * .15f, R = r * (.25f + .95f * EaseOut(Rise(t, t0, t0 + .55f))), al = Fade(t, t0 + .45f, t0 + 1.3f) * Rise(t, t0, t0 + .06f);
                    if (al <= 0) continue;
                    int n = 9 + w * 5;
                    for (int k = 0; k < n; k++)
                    {
                        Vector2 dd = Dir(Seed + w * 7, k, n, .62f), bp = at + dd * R * (.9f + .2f * H(Seed + w, k));
                        Tongue(b, bp, dd.x * -12 + (H(Seed + w, k + 20) - .5f) * 14, (1.25f - .2f * w) * (.8f + .5f * H(Seed, k + w * 9)), .38f, (k + w) % 3 == 0 ? Gold : Fire, al * .95f, t + k * .17f, Seed + k + w * 31);
                        if (k % 2 == 0) Puff(b, bp + Vector2.up * (1.1f + .6f * (t - t0)), .5f + .3f * (t - t0), new Color(.14f, .09f, .07f), .45f * al, t, Seed + k + w * 13);
                        if (k % 3 == 0) Fleck(b, bp + Vector2.up * .8f, new Vector2(dd.x * 1.5f, 2.5f + H(Seed, k + w) * 2), 6, t - t0 - .1f, .7f, .07f, Gold, Seed + k + w * 50);
                    }
                }
                Tongue(b, at, 0, r * 1.5f * EaseOut3(Rise(t, 0, .3f)), r * .5f, Gold, Fade(t, .5f, 1.5f), t, Seed + 50, 2);
                for (int k = 0; k < 14; k++)
                {
                    float life = .8f + .6f * H(Seed, k + 70), u = t / life; if (u <= 0 || u >= 1) continue;
                    Vector2 dir = Dir(Seed, k + 40, 14, .5f), p = at + dir * r * 1.3f * EaseOut(u) + Vector2.up * (2.6f * H(Seed, k + 90) * u - 3.8f * u * u + .2f);
                    Glint(b, p, .27f, k % 2 == 0 ? Gold : Fire, 1 - u * u, k * 33 + t * 300);
                }
            }
        }

        /// <summary>A meteor with a long flame tail drops in from the upper left onto a glowing warning; the impact throws a flash, a column of fire and flying rocks, then cracked lava glows under the crowd.</summary>
        private sealed class Meteor : FxEffect
        {
            protected override float Life => 2.7f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                float r = Mathf.Max(1.6f, Radius); Vector2 at = At;
                if (t < 0)
                {
                    float g = EaseOut(pre), flick = .75f + .25f * Mathf.Sin(now * 28);
                    b.Floor(FxShape.Shade, at, r * 1.5f, A(Soot, .5f * g), 0, Seed);
                    b.Floor(FxShape.Glow, at, r * 1.4f, A(Red, .6f * g * flick), 0, Seed);
                    float u = 1 - (-t) / .6f; if (u <= 0) return;
                    Vector2 start = at + new Vector2(-6.5f, 10f), p = Vector2.Lerp(start, at + Vector2.up * .2f, u * u), back = (start - at).normalized; float s = r * .58f, bang = Ang(back) - 90;
                    for (int j = 0; j < 5; j++)
                    {
                        float spread = (j - 2) * 11, a2 = bang + spread; Vector2 tdir = Vec(a2 + 90);
                        b.Quad(FxShape.Flame, p + tdir * s * (2.2f + .4f * (j % 2)), new Vector2(s * (.75f - .08f * Mathf.Abs(j - 2)), s * (2.4f + .5f * (j % 2))), a2, A(j % 2 == 0 ? Fire : Gold, .95f), now, Seed + j, 1);
                    }
                    for (int k = 0; k < 12; k++)
                    {
                        float d = (.5f + k * .38f) * s; Vector2 q = p + back * d + new Vector2(Mathf.Sin(k * 5.3f + now * 20), Mathf.Cos(k * 3.1f)) * .3f * s;
                        Glint(b, q, .2f, k % 2 == 0 ? Gold : Fire, .9f * (1 - k / 13f), k * 40 + now * 200);
                        if (k % 2 == 0) b.Quad(FxShape.Smoke, p + back * d * 1.5f, s * (.6f + k * .05f), A(Soot, .5f * (1 - k / 13f)), k, Seed + k);
                    }
                    b.Quad(FxShape.Glow, p, s * 2.6f, A(Fire, .55f), 0, Seed);
                    b.Quad(FxShape.Smoke, p, s * 1.05f, A(new Color(.1f, .03f, .02f), 1f), 0, Seed + 4);
                    b.Quad(FxShape.Cracks, p, s * 1.05f, now * 120, A(Gold, 1f), 1, Seed, 1);
                    b.Quad(FxShape.Glow, p, s * .5f, A(Gold, .6f), 0, Seed);
                    return;
                }
                float fl = Fade(t, 0, .2f), up = EaseOut3(Rise(t, 0, .24f)), fd = Fade(t, .35f, 1.4f);
                b.Quad(FxShape.Glow, at, r * 2f, A(White, .9f * fl), 0, Seed);
                b.Quad(FxShape.Glow, at + Vector2.up * .5f, r * 1.8f, A(Fire, .75f * Fade(t, 0, .9f)), 0, Seed);
                for (int k = 0; k < 10; k++)
                {
                    Vector2 dd = Dir(Seed + 6, k, 10, .62f); float u = EaseOut(Rise(t, 0, .9f));
                    Grit(b, at + dd * r * (.4f + 1f * u), r * (.45f + .3f * u), new Color(.4f, .3f, .22f), .5f * Fade(t, .3f, 1.3f), t + k, Seed + k);
                }
                for (int k = 0; k < 14; k++)
                {
                    Vector2 dd = Dir(Seed + 8, k, 14, .7f);
                    Fleck(b, at + Vector2.up * .3f, new Vector2(dd.x * (3 + 2 * H(Seed, k + 30)), 4 + 3 * H(Seed, k + 31)), 10, t - .03f, .8f, .09f, k % 2 == 0 ? Gold : White, Seed + k + 100);
                }
                Tongue(b, at, 0, r * 2.4f * up, r * .85f, Fire, fd, t, Seed, 1);
                Tongue(b, at, 0, r * 1.7f * up, r * .5f, Gold, fd, t + .3f, Seed + 3, 2);
                for (int k = 0; k < 6; k++)
                {
                    float side = k % 2 == 0 ? -1 : 1, lean = side * (24 + 12 * (k / 2));
                    Tongue(b, at + new Vector2(side * (k / 2 + 1) * r * .33f, -.08f * k), lean, r * (1.3f - .2f * (k / 2)) * up, r * .36f, k % 3 == 0 ? Gold : Fire, fd, t + k * .13f, Seed + 20 + k);
                }
                float lava = Fade(t, .3f, 2.6f) * (.75f + .25f * Mathf.Sin(t * 7));
                b.Floor(FxShape.Shade, at, r * 1.6f, A(Soot, .55f * Fade(t, .2f, 2.6f)), 0, Seed);
                b.Floor(FxShape.Glow, at, r * 1.5f, A(Red, .6f * lava), 0, Seed);
                b.Quad(FxShape.Cracks, at, new Vector2(r * 1.5f, r * 1f), 0, A(Lava, lava), 1, Seed, 2);
                for (int k = 0; k < 9; k++)
                {
                    float life = .7f + .5f * H(Seed, k + 5), u = t / life; if (u <= 0 || u >= 1) continue;
                    Vector2 dir = Dir(Seed, k, 9, .55f), p = at + dir * r * 1.7f * EaseOut(u) + Vector2.up * (3.6f * H(Seed, k + 9) * u - 5f * u * u + .2f);
                    b.Quad(FxShape.Glow, p, .55f, A(Fire, .5f * (1 - u)), 0, Seed);
                    for (int j = 1; j < 4; j++) { float uj = Mathf.Max(0, u - j * .05f) * life; Vector2 q = at + dir * r * 1.7f * EaseOut(uj / life) + Vector2.up * (3.6f * H(Seed, k + 9) * uj / life - 5f * uj * uj / life / life + .2f); Puff(b, q, .14f + .07f * j, new Color(.2f, .14f, .1f), .5f * (1 - u) * (1 - j / 4f), u, Seed + k * 3 + j); }
                    b.Quad(FxShape.Smoke, p, .3f + .12f * H(Seed, k + 3), A(new Color(.12f, .04f, .02f), 1 - u * u), 0, Seed + k);
                    b.Quad(FxShape.Cracks, p, .32f, t * 300, A(Gold, 1 - u), 1, Seed + k, 1);
                }
                for (int k = 0; k < 16; k++)
                {
                    float life = 1f + .9f * H(Seed, k + 70), u = t / life; if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + new Vector2((H(Seed, k + 20) - .5f) * r * 1.8f + Mathf.Sin(t * 5 + k) * .2f, .2f + u * (2.2f + 2f * H(Seed, k + 40)));
                    Glint(b, p, .22f, k % 3 == 0 ? Gold : Fire, 1 - u, k * 30 + t * 200);
                }
                SmokeColumn(b, at, r * .6f, 3.4f, t, new Color(.08f, .05f, .05f), Seed, 7);
            }
        }

        /// <summary>A phoenix of fire sweeps along the line: six-feather wings that beat, a gold body and beak, five flame tails, a trail of ground fire and molten feathers falling.</summary>
        private sealed class Phoenix : FxEffect
        {
            protected override float Life => 2.5f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 d = At - From; float len = d.magnitude; if (len < .1f) return;
                Vector2 dir = d / len, nrm = new Vector2(-dir.y, dir.x); float ang = Ang(dir), s = Mathf.Max(1f, Radius) * 1.15f;
                if (t < 0)
                {
                    float g = EaseOut(pre);
                    b.Beam(From, At, .55f, A(Fire, .22f * g), t, Seed, .4f);
                    for (int k = 0; k < 26; k++)
                    {
                        float u = H(Seed, k), rise = (pre * 2 + H(Seed, k + 60)) % 1; Vector2 p = From + dir * len * u + nrm * (H(Seed, k + 30) - .5f) * 1.3f + Vector2.up * rise * 1.3f;
                        Glint(b, p, .27f, k % 3 == 0 ? Gold : Fire, .85f * g * (1 - rise), k * 40 + now * 100);
                    }
                    return;
                }
                float fly = Rise(t, 0, 1.05f), pos = len * (-.1f + 1.2f * fly); Vector2 hp = From + dir * pos;
                for (int i = 0; i < 20; i++)
                {
                    float path = len * (-.1f + 1.2f * i / 19f), a = t - (i / 19f) * 1.05f; if (a < 0 || a > 1.4f) continue;
                    float lifeU = a / 1.4f; Vector2 bp = From + dir * path + nrm * Mathf.Sin(i * 2.1f) * .35f;
                    Tongue(b, bp, ((i % 3) - 1) * 14, (.9f + .6f * H(Seed, i)) * s * Mathf.Min(1, a / .15f) * (1 - lifeU * .6f), .6f * s, i % 2 == 0 ? Fire : Gold, 1 - lifeU, t + i * .2f, Seed + i);
                    b.Floor(FxShape.Glow, bp, 1.1f * s, A(Red, .45f * (1 - lifeU)), 0, Seed);
                    if (i % 2 == 0) Puff(b, bp + Vector2.up * (1.2f + 1.3f * lifeU) * s, (.45f + .5f * lifeU) * s, new Color(.15f, .09f, .07f), .5f * Mathf.Sin(lifeU * 3.14f), lifeU, Seed + i);
                    if (i % 3 == 0) Grit(b, bp, .9f * s, new Color(.08f, .05f, .04f), .4f * Rise(lifeU, .3f, .7f), lifeU, Seed + i + 3);
                }
                for (int k = 0; k < 16; k++)
                {
                    float f = (k + .5f) / 16f, a = t - f * 1.05f; Vector2 sp = From + dir * len * (-.1f + 1.2f * f) + nrm * (H(Seed, k + 120) - .5f) * 1.2f * s + Vector2.up * .6f * s;
                    Fleck(b, sp, new Vector2((H(Seed, k + 121) - .5f) * 2, -1f - H(Seed, k + 122)), 5, a, .6f, .07f, k % 2 == 0 ? Gold : White, Seed + k + 300);
                }

                for (int k = 0; k < 14; k++)
                {
                    float f = H(Seed, k + 5), a = t - f * 1.05f - .1f; if (a < 0 || a > .95f) continue; float u = a / .95f;
                    Vector2 p = From + dir * len * f + nrm * (H(Seed, k + 80) - .5f) * 2.2f * (.4f + u) + Vector2.down * u * u * 1.5f;
                    Lf(b, p, .5f, k * 50 + u * 300, k % 2 == 0 ? Fire : Gold, 1 - u, Seed + k, true);
                }
                if (fly >= 1) return;
                float vis = Rise(fly, 0, .08f) * Fade(fly, .9f, 1f), sc = s * (.85f + .3f * Mathf.Sin(fly * Mathf.PI)), flap = Mathf.Sin(now * 15) * 16;
                b.Quad(FxShape.Glow, hp, sc * 3f, A(Fire, .5f * vis), 0, Seed);
                for (int j = 0; j < 5; j++)
                {
                    float ta = ang + 180 + (j - 2) * 15 + Mathf.Sin(now * 13 + j) * 7, h = (2.2f + .4f * (j % 2) - .2f * Mathf.Abs(j - 2)) * sc;
                    b.Quad(FxShape.Flame, hp - dir * .5f * sc + Vec(ta) * h, new Vector2(.32f * sc, h), ta - 90, A(j % 2 == 0 ? Fire : Gold, .95f * vis), now, Seed + j, 1);
                }
                for (int side = -1; side <= 1; side += 2)
                    for (int i = 0; i < 6; i++)
                    {
                        float fa = side * (60 + 11 * i + flap * (.6f + .1f * i)), len2 = (2.7f - .22f * i) * sc;
                        Vector2 fdir = Vec(ang + fa), bp = hp + nrm * side * .22f * sc + dir * (.5f - .12f * i) * sc;
                        b.Quad(Leaf, bp + fdir * len2 * .5f, new Vector2(len2 * .5f, len2 * .5f * .72f), ang + fa, A(Color.Lerp(new Color(1f, .7f, .2f), Red, i / 5f), .7f * vis), 0, Seed + i + side * 9, 1);
                    }
                b.Quad(Leaf, hp, new Vector2(1.15f * sc, .48f * sc), ang, A(Gold, vis), 0, Seed + 33, 0);
                b.Quad(FxShape.Glow, hp + dir * .3f * sc, sc * .8f, A(White, .9f * vis), 0, Seed);
                b.Quad(FxShape.Glow, hp + dir * 1f * sc, sc * .45f, A(Gold, vis), 0, Seed);
                b.Quad(Shard, hp + dir * 1.15f * sc, new Vector2(.4f * sc, .17f * sc), ang, A(White, vis), 0, Seed + 40);
                for (int j = -1; j <= 1; j++)
                {
                    float ca = ang + 180 + j * 35 + Mathf.Sin(now * 17 + j) * 10; Vector2 cb = hp + dir * .9f * sc;
                    b.Quad(FxShape.Flame, cb + Vec(ca) * .5f * sc, new Vector2(.14f * sc, .5f * sc), ca - 90, A(Gold, vis), now, Seed + j + 60, 1);
                }
            }
        }
    }
}
