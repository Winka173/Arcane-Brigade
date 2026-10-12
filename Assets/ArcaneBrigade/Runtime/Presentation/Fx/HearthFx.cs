using System;
using System.Collections.Generic;
using UnityEngine;
using static ArcaneBrigade.FxKit;

namespace ArcaneBrigade
{
    /// <summary>Hearth bond: steam, a bubbling pot, a pie and a banquet in warm orange. Food that does something: the pot boils over,
    /// the pie is thrown and splatters, the feast rises from the floor.</summary>
    public static class HearthFx
    {
        private const FxShape Pot = (FxShape)44, PieS = (FxShape)45, Food = (FxShape)49;
        private static readonly Color Orange = new Color(1f, .5f, .14f, 1), Broth = new Color(1f, .62f, .2f, 1), Steam = new Color(1f, .93f, .84f, 1), Gold = new Color(1f, .8f, .35f, 1), Red = new Color(.95f, .22f, .1f, 1);

        public static void Register(Dictionary<string, Func<FxEffect>> map)
        {
            map["hearth-hot-pot"] = () => new HotPot();
            map["chef-pie"] = () => new ChefPie();
            map["hearth-feast"] = () => new Feast();
            map["feast"] = () => new Platter();
        }

        /// <summary>Step 6 zone. A cauldron the size of the area sits on the crowd over a ring of cooking flames; the lid rattles and hops on
        /// jets of steam, bubbles swell and pop, and the broth slops over the rim in waves that spread into a bubbling puddle across the floor.</summary>
        private sealed class HotPot : FxEffect
        {
            protected override float Life => 3.2f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                float age = now - Born; if (age < 0) return;
                float total = Mathf.Max(1, End - Born), on = Rise(age, 0, .25f) * Fade(age, total - .45f, total);
                Vector2 at = At; float r = Mathf.Max(2f, Radius);
                // the slop-over puddle on the floor
                float puddle = EaseOut(Rise(age, .3f, 1.1f));
                b.Floor(FxShape.Glow, at, r * 1.15f * puddle, A(Broth, .45f * on), 0, Seed);
                b.Floor(FxShape.Disc, at, r * .95f * puddle, A(new Color(.7f, .26f, .06f), .45f * on), 0, Seed);
                for (int k = 0; k < 10; k++)
                {
                    float u = (age * .8f + H(Seed, k)) % 1; Vector2 p = at + Dir(Seed, k, 10, .6f) * r * (.3f + .65f * H(Seed, k + 4)) * puddle;
                    float pop = Mathf.Sin(u * Mathf.PI);
                    b.Quad(FxShape.Sparkle, p, new Vector2(.28f, .28f) * pop, 0, A(Gold, .85f * on * pop), 0, 0, 0);
                }
                // flames licking round the base, sooty smoke curling off their tips and embers spiralling up
                for (int k = 0; k < 8; k++)
                {
                    Vector2 p = at + new Vector2(Mathf.Cos(k / 8f * 6.2832f) * r * .62f, Mathf.Sin(k / 8f * 6.2832f) * r * .3f - r * .35f);
                    b.Quad(FxShape.Flame, p + Vector2.up * .45f, new Vector2(.3f, .55f) * (1 + .25f * Mathf.Sin(now * 14 + k * 2)), 0, A(k % 2 == 0 ? Orange : Gold, .65f * on), age, Seed + k, 1.2f);
                    if (k % 2 == 1) { float v = (age * .9f + H(Seed, k + 90)) % 1; b.Quad(FxShape.Smoke, p + new Vector2(Mathf.Sin(v * 5 + k) * .2f, .9f + v * 1.1f), .22f + .35f * v, 0, A(new Color(.22f, .16f, .12f), .6f * on * (1 - v) * Mathf.Min(1, v * 5)), v + k, Seed + k + 90); }
                }
                for (int k = 0; k < 12; k++)
                {
                    float v = (age * (.6f + .3f * H(Seed, k + 70)) + H(Seed, k + 71)) % 1;
                    Vector2 p = at + new Vector2((H(Seed, k + 72) - .5f) * r * 1.5f + Mathf.Sin(v * 7 + k) * .25f, -r * .3f + v * 2.2f);
                    b.Quad(FxShape.Glow, p, .07f + .05f * H(Seed, k), A(k % 3 == 0 ? Gold : Orange, 1f * on * (1 - v)), 0, Seed);
                }
                // the cauldron
                float s = r * 0.95f, lid = Mathf.Abs(Mathf.Sin(age * 14)) * .22f * (Mathf.Sin(age * 3.1f) > -.3f ? 1 : .3f);
                Vector2 pc = at + Vector2.up * (s * .55f);
                b.Floor(FxShape.Shade, at, r * 1.0f, A(new Color(.1f, .04f, .01f), .5f * on), 0, Seed);
                b.Quad(Pot, pc, new Vector2(s, s), 0, A(new Color(.95f, .45f, .22f), 1.2f * on), 0, Seed, 0);
                // broth boiling over the rim: foamy drips down both sides
                for (int k = 0; k < 6; k++)
                {
                    float u = (age * .9f + k / 6f) % 1, side = k % 2 == 0 ? 1 : -1;
                    Vector2 p = pc + new Vector2(side * s * (.6f + .2f * H(Seed, k)), s * .38f - u * s * .8f);
                    b.Quad(FxShape.Glow, p, .22f + .1f * H(Seed, k + 3), A(Broth, 1f * on * (1 - u * .4f)), 0, Seed);
                }
                // the lid hops on its own steam
                b.Quad(Pot, pc + new Vector2(Mathf.Sin(age * 25) * .06f, s * (.42f + lid)), new Vector2(s * .95f, s * .95f), Mathf.Sin(age * 20) * 6, A(new Color(1f, .8f, .5f), 1.15f * on), 0, Seed + 3, 2);
                for (int k = 0; k < 9; k++)
                {
                    float u = (age * .7f + H(Seed, k)) % 1;
                    Vector2 p = pc + new Vector2((H(Seed, k + 7) - .5f) * s * 1.4f + Mathf.Sin(u * 6 + k) * .25f, s * .55f + u * 3.4f);
                    b.Quad(FxShape.Smoke, p, new Vector2(.55f + .6f * u, .5f + .7f * u), 0, A(Steam, .6f * on * (1 - u)), u, Seed + k);
                }
                // every big hop of the lid slops broth over in fat drops that arc out and splash on the floor
                float hopPhase = (age * 14 / Mathf.PI) % 1; int hop = Mathf.FloorToInt(age * 14 / Mathf.PI);
                if (hop % 3 == 0) for (int k = 0; k < 5; k++)
                {
                    float v = hopPhase, side = (k % 2 == 0 ? 1 : -1) * (.5f + .5f * H(Seed + hop, k));
                    Vector2 p = pc + new Vector2(side * (s * .7f + 1.1f * v), s * .45f + 1.2f * v - 2.4f * v * v);
                    b.Quad(FxShape.Glow, p, .14f + .06f * H(Seed, k), A(Broth, on * (1 - v * .5f)), 0, Seed);
                    b.Quad(FxShape.Disc, p, .07f, A(Gold, on * (1 - v)), 0, Seed);
                }
                // bubbles popping above the rim through the lid gap
                for (int k = 0; k < 8; k++)
                {
                    float u = (age * 1.3f + H(Seed, k + 50)) % 1;
                    Vector2 p = pc + new Vector2((H(Seed, k + 60) - .5f) * s * 1.1f, s * .5f + u * 1.3f);
                    b.Quad(FxShape.Disc, p, .09f + .1f * u, A(Gold, .6f * on * (1 - u)), 0, Seed);
                }
            }
        }

        /// <summary>Step 8 pie. The grand chef lobs a lattice pie in a high tumbling arc; it lands with a flat SPLAT: the pie bursts open,
        /// hot filling and torn crust fly, flour and steam puff up, and the sticky filling sizzles on the floor.</summary>
        private sealed class ChefPie : FxEffect
        {
            protected override float Life => 1.1f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 from = From, at = At; float r = Mathf.Max(1f, Radius) * 1.15f;
                if (t < 0)
                {
                    float u = pre; Vector2 p = Vector2.Lerp(from, at, u) + Vector2.up * Mathf.Sin(u * Mathf.PI) * 2.8f;
                    b.Floor(FxShape.Shade, Vector2.Lerp(from, at, u), .55f, A(new Color(.08f, .04f, .02f), .5f * u), 0, Seed);
                    b.Quad(PieS, p, new Vector2(.62f, .62f * (.45f + .55f * Mathf.Abs(Mathf.Cos(u * 11)))), u * 40, A(new Color(1f, .75f, .4f), 1.25f), 0, Seed);
                    for (int k = 1; k <= 4; k++) Glint(b, p + Vector2.up * k * .12f + new Vector2(Mathf.Sin(now * 12 + k) * .1f, 0), .16f, Steam, .6f / k, 0);
                    b.Floor(FxShape.Glow, at, r * .9f * u, A(Orange, .35f * u), 0, Seed);
                    return;
                }
                float fade = Fade(t, .65f, 1.1f);
                float splat = EaseOut3(Rise(t, 0, .14f));
                b.Floor(FxShape.Glow, at, r * 1.7f, A(Orange, .45f * fade), 0, Seed);
                b.Floor(FxShape.Disc, at, r * .85f * splat, A(new Color(.85f, .2f, .08f), .6f * fade), 0, Seed);
                b.Floor(FxShape.Disc, at, r * .6f * splat, A(Broth, .35f * fade), 0, Seed);
                // the splat: red filling streaks thrown out flat across the floor, each ending in a blob
                for (int k = 0; k < 9; k++)
                {
                    Vector2 tip = at + Dir(Seed + 11, k, 9, .55f) * r * (1.2f + .7f * H(Seed, k + 70)) * splat;
                    b.Link(at, tip, .16f + .06f * H(Seed, k), A(Red, .8f * fade), Seed + k, .3f);
                    b.Floor(FxShape.Disc, tip, .22f + .12f * H(Seed, k + 2), A(Red, .7f * fade), 0, Seed);
                }
                // the pie itself, squashed flat and cracked open
                b.Quad(PieS, at + Vector2.up * .15f, new Vector2(r * .7f * (1 + .5f * splat), r * .38f * (1 - .25f * splat)), 0, A(new Color(1f, .7f, .35f), .8f * fade * Fade(t, .2f, .7f)), 0, Seed);
                // filling globs and crust shards
                for (int k = 0; k < 16; k++)
                {
                    float u = t / (.55f + .3f * H(Seed, k)); if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + Dir(Seed, k, 16, .55f) * r * 1.9f * EaseOut(u) + Vector2.up * (3.1f * H(Seed, k + 9) * u - 4.8f * u * u + .2f);
                    if (k % 3 == 0) b.Quad(PieS, p, new Vector2(.2f, .15f), u * 600, A(new Color(1f, .75f, .4f), 1.1f * (1 - u)), 0, Seed + k);
                    else { b.Quad(FxShape.Glow, p, .2f + .1f * H(Seed, k), A(k % 2 == 0 ? Red : Broth, .8f * (1 - u * u)), 0, Seed); b.Quad(FxShape.Disc, p, .09f, A(Gold, .9f * (1 - u)), 0, Seed); }
                }
                // a low ring of grainy flour blown out by the splat, steam billowing up out of the filling
                for (int k = 0; k < 8; k++)
                {
                    float u = Rise(t, .02f, .9f); Vector2 p = at + Dir(Seed + 3, k, 8, .6f) * r * (.3f + .9f * EaseOut(u));
                    b.Quad(FxShape.Dust, p, new Vector2(.6f, .36f) * (.7f + u), 0, A(new Color(.95f, .9f, .82f), .7f * (1 - u)), u, Seed + k);
                }
                for (int k = 0; k < 5; k++)
                {
                    float u = Rise(t, .05f + k * .06f, .95f + k * .03f); if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + new Vector2((k - 2) * .3f + Mathf.Sin(u * 5 + k) * .2f, .3f + 1.6f * EaseOut(u));
                    b.Quad(FxShape.Smoke, p, .35f + .5f * u, 0, A(Steam, .7f * Mathf.Min(1, u * 4) * (1 - u)), u + k, Seed + k + 30);
                }
                // sizzling embers on the sticky floor
                for (int k = 0; k < 10; k++)
                {
                    float u = (t * 1.5f + H(Seed, k + 40)) % 1;
                    Glint(b, at + Dir(Seed, k + 20, 10, .55f) * r * .8f + Vector2.up * u * .8f, .13f, Gold, .9f * (1 - u) * fade, k * 40);
                }
            }
        }

        /// <summary>Step 8 feast (weather). The whole arena warms to an orange hearth glow; steam columns rise from the floor and a banquet
        /// of loaves, apples, pies and pots rains slowly upward-and-down in drifting arcs, each glinting; golden sparks float like cinders.</summary>
        private sealed class Feast : FxEffect
        {
            protected override float Life => 5;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                float age = now - Born, total = Mathf.Max(1, End - Born), on = Rise(age, 0, .6f) * Fade(age, total - .9f, total);
                if (Moment.Kind != BondMomentKind.Weather) on = Rise(age, 0, .3f) * Fade(age, .8f, 1.4f);
                Rect a = Arena; Vector2 c = a.center;
                b.Quad(FxShape.Shade, c, new Vector2(a.width * .8f, a.height * .8f), 0, A(new Color(.18f, .05f, .0f), .12f * on), 0, Seed);
                b.Quad(FxShape.Glow, c, new Vector2(a.width * .65f, a.height * .6f), 0, A(Orange, .07f * on), 0, Seed);
                // steam columns from the floor
                for (int k = 0; k < 7; k++)
                {
                    float x = a.xMin + a.width * (k + .5f) / 7f;
                    for (int s = 0; s < 4; s++)
                    {
                        float u = (age * .22f + s * .25f + H(Seed, k)) % 1;
                        Vector2 p = new Vector2(x + Mathf.Sin(u * 7 + k) * .6f, a.yMin + u * (a.height + 1));
                        b.Quad(FxShape.Smoke, p, new Vector2(.6f + .7f * u, .7f + .7f * u), 0, A(Steam, .3f * on * Mathf.Sin(u * Mathf.PI)), u, Seed + k * 4 + s);
                    }
                }
                // banquet
                for (int k = 0; k < 8; k++)
                {
                    float u = (age * .12f + H(Seed, k)) % 1, kind = k % 4;
                    Vector2 p = new Vector2(a.xMin + a.width * ((H(Seed, k + 20) + Mathf.Sin(age * .5f + k) * .02f) % 1), a.yMin - .5f + u * (a.height + 1));
                    float s = .55f + .25f * H(Seed, k + 5), rot = Mathf.Sin(age * 1.4f + k) * 25;
                    // crumbs trickle off each dish as it floats
                    for (int m = 0; m < 3; m++) { float v = (age * 1.3f + m / 3f + H(Seed, k + m)) % 1; Glint(b, p + new Vector2((m - 1) * s * .4f, -s * .4f - v * .9f), .07f, Gold, .9f * on * (1 - v) * Mathf.Min(1, Mathf.Sin(u * Mathf.PI) * 3), m * 40); }
                    float al = 1.2f * on * Mathf.Min(1, Mathf.Sin(u * Mathf.PI) * 3);
                    b.Quad(FxShape.Glow, p, s * 1.7f, A(Orange, .3f * al), 0, Seed + k);
                    if (kind == 0) b.Quad(Food, p, new Vector2(s * 1.2f, s * 1.2f), rot, A(new Color(1f, .8f, .5f), al), 0, Seed + k, 0);
                    else if (kind == 1) b.Quad(Food, p, new Vector2(s, s), rot, A(new Color(1f, .5f, .4f), al), 0, Seed + k, 1);
                    else if (kind == 2) b.Quad(PieS, p, new Vector2(s * .9f, s * .6f), rot, A(new Color(1f, .75f, .4f), al), 0, Seed + k);
                    else b.Quad(Pot, p, new Vector2(s, s), rot * .5f, A(new Color(.95f, .55f, .3f), al), 0, Seed + k, 0);
                }
                for (int k = 0; k < 50; k++)
                {
                    float u = (age * (.1f + .1f * H(Seed, k)) + H(Seed, k + 80)) % 1;
                    Vector2 p = new Vector2(a.xMin + a.width * H(Seed, k + 120) + Mathf.Sin(age + k) * .4f, a.yMin + u * a.height);
                    Glint(b, p, .09f + .09f * H(Seed, k + 9), k % 2 == 0 ? Gold : Orange, .85f * on * Mathf.Sin(u * Mathf.PI), age * 30 + k * 20);
                }
            }
        }
    
        /// <summary>The hearth's regular meal (trait): a silver platter floats in above the hero, the cloche lifts off in a puff of steam
        /// to show a roast loaf and apples, crumbs of gold sprinkle down and the dish fades.</summary>
        private sealed class Platter : FxEffect
        {
            protected override float Life => 1.5f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                if (t < 0) return;
                float on = Rise(t, 0, .25f) * Fade(t, 1.1f, 1.5f), lift = EaseOut(Rise(t, .3f, .7f));
                Vector2 c = At + Vector2.up * (2.2f + .2f * Mathf.Sin(t * 4));
                b.Quad(FxShape.Glow, c, 1.6f, A(Orange, .4f * on), 0, Seed);
                b.Quad(FxShape.Disc, c + Vector2.down * .25f, new Vector2(.95f, .22f), 0, A(new Color(.9f, .85f, .75f), .55f * on), 0, Seed);
                b.Quad(Food, c, new Vector2(.6f, .6f), 0, A(new Color(1f, .78f, .45f), 1.05f * on * lift), 0, Seed, 0);
                b.Quad(Food, c + new Vector2(.55f, -.05f), new Vector2(.3f, .3f), 0, A(new Color(1f, .45f, .35f), on * lift), 0, Seed, 1);
                b.Quad(Food, c + new Vector2(-.55f, -.05f), new Vector2(.28f, .28f), 0, A(new Color(1f, .45f, .35f), on * lift), 0, Seed + 1, 1);
                b.Quad(Pot, c + new Vector2(.3f * lift, .3f + 1.1f * lift), new Vector2(.85f, .85f), 25 * lift, A(new Color(.9f, .9f, .95f), 1f * on * (1 - lift * .7f)), 0, Seed, 2);
                for (int k = 0; k < 6; k++)
                {
                    float u = Rise(t, .3f + k * .05f, 1.25f); if (u <= 0 || u >= 1) continue;
                    b.Quad(FxShape.Smoke, c + new Vector2((k - 2.5f) * .25f + Mathf.Sin(u * 6 + k) * .15f, .3f + u * 1.4f), .3f + .4f * u, A(Steam, .75f * Mathf.Min(1, u * 4) * (1 - u)), u + k, Seed + k);
                }
                // the cloche flashes as it lifts, and a ding sparkles off its rim
                Glint(b, c + new Vector2(.3f * lift - .45f, .35f + 1.1f * lift), .55f, White, Fade(t, .35f, .7f) * Rise(t, .3f, .38f), t * 120);

                for (int k = 0; k < 10; k++)
                {
                    float u = (t * 1.1f + H(Seed, k)) % 1; if (t < .4f) break;
                    Glint(b, c + new Vector2((H(Seed, k + 5) - .5f) * 1.8f, -.3f - u * 1.2f), .1f, Gold, .9f * on * (1 - u), k * 40);
                }
            }
        }
    }
}
