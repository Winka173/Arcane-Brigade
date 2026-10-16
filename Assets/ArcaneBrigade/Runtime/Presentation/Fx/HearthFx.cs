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
        // owner 29/10/2026: small glints and sparks were drawn far too big: small ones shrink, big set-piece flashes keep their size
        private static void Glint(FxBatch b, Vector2 at, float size, Color tint, float alpha, float turn = 0)
            => FxKit.Glint(b, at, size <= .08f ? size : size < .5f ? .08f + (size - .08f) * .45f : size - .231f, tint, alpha, turn);
        private const FxShape Pot = (FxShape)44, PieS = (FxShape)45, Food = (FxShape)49;
        private static readonly Color Orange = new Color(1f, .5f, .14f, 1), Broth = new Color(1f, .62f, .2f, 1), Steam = new Color(1f, .93f, .84f, 1), Gold = new Color(1f, .8f, .35f, 1), Red = new Color(.95f, .22f, .1f, 1),
            Crust = new Color(1f, .7f, .36f, 1), Cherry = new Color(.85f, .1f, .16f, 1), Iron = new Color(.27f, .26f, .3f, 1);

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
                    b.Quad(FxShape.Sparkle, p, new Vector2(.16f, .16f) * pop, 0, A(Gold, .85f * on * pop), 0, 0, 0);
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
                b.Quad(Pot, pc, new Vector2(s, s), 0, A(Iron, on), 0, Seed, 0);
                // the hot broth inside the rim: the only glow of the pot
                b.Quad(FxShape.Glow, pc + Vector2.up * s * .38f, new Vector2(s * .62f, s * .13f), 0, A(Broth, .9f * on), 0, Seed);
                b.Quad(FxShape.Glow, pc + Vector2.up * s * .5f, new Vector2(s * .7f, s * .3f), 0, A(Orange, .25f * on), 0, Seed);
                // broth boiling over the rim: foamy drips down both sides
                for (int k = 0; k < 6; k++)
                {
                    float u = (age * .9f + k / 6f) % 1, side = k % 2 == 0 ? 1 : -1;
                    Vector2 p = pc + new Vector2(side * s * (.6f + .2f * H(Seed, k)), s * .38f - u * s * .8f);
                    b.Quad(FxShape.Glow, p, .22f + .1f * H(Seed, k + 3), A(Broth, 1f * on * (1 - u * .4f)), 0, Seed);
                }
                // the lid hops on its own steam
                b.Quad(Pot, pc + new Vector2(Mathf.Sin(age * 25) * .06f, s * (.42f + lid)), new Vector2(s * .95f, s * .95f), Mathf.Sin(age * 20) * 6, A(new Color(.32f, .31f, .35f), on), Mathf.Repeat(Mathf.Sin(age * 20) * 6 / 360f, 1), Seed + 3, 2);
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

        /// <summary>A lattice pie: the cherry filling under a golden crust whose gaps let it show. <paramref name="flip"/> squashes it
        /// as it tumbles.</summary>
        private static void PieAt(FxBatch b, Vector2 p, float s, float flip, float rot, float alpha, int seed)
        {
            Vector2 sz = new Vector2(s, s * Mathf.Max(.12f, flip));
            b.Quad(PieS, p, sz * .9f, rot, A(Cherry, alpha), Mathf.Repeat(rot / 360f, 1), seed, 1);
            b.Quad(PieS, p, sz, rot, A(Crust, alpha), Mathf.Repeat(rot / 360f, 1), seed, 0);
        }
        /// <summary>Step 8 pie. The grand chef lobs a lattice pie in a high tumbling arc; it lands on the crowd with a flat SPLAT: the pie
        /// squashes, its crust breaks into wedges that fly off spinning, the cherry filling bursts out across the floor in a splat with
        /// thrown blobs, flour puffs up in a low ring and steam billows off the hot filling.</summary>
        private sealed class ChefPie : FxEffect
        {
            protected override float Life => 1.25f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 from = From, at = At; float r = Mathf.Max(1f, Radius) * 1.15f;
                if (t < 0)
                {
                    float u = pre; Vector2 g = Vector2.Lerp(from, at, u), p = g + Vector2.up * (Mathf.Sin(u * Mathf.PI) * 2.8f + .3f);
                    b.Floor(FxShape.Shade, g, .7f + .3f * u, A(new Color(.08f, .04f, .02f), .5f * u), 0, Seed);
                    PieAt(b, p, .85f + .2f * u, .35f + .65f * Mathf.Abs(Mathf.Cos(u * 9)), u * 70, 1, Seed);
                    for (int k = 0; k < 4; k++)
                    {
                        float v = (now * 1.6f + k / 4f) % 1;
                        b.Quad(FxShape.Smoke, p + new Vector2(Mathf.Sin(v * 6 + k) * .2f - .3f * v, .3f + v * .9f), .12f + .2f * v, 0, A(Steam, .55f * (1 - v)), v, Seed + k);
                    }
                    return;
                }
                float fade = Fade(t, .75f, 1.25f), splat = EaseOut3(Rise(t, 0, .12f));
                // the filling splat spreading flat over the floor, glossy, with a dark stain under it
                b.Floor(FxShape.Shade, at, r * 1.25f * splat, A(new Color(.12f, .02f, .03f), .5f * fade), 0, Seed);
                b.Quad(PieS, at, new Vector2(r * 1.15f, r * .7f) * (.5f + .5f * splat), 0, A(Cherry, fade), 0, Seed + 1, 1);
                // the pie itself, squashed flat and cracked, its crust sinking into the filling
                float keep = Fade(t, .15f, .45f), sq = 1 + .3f * splat;
                b.Quad(PieS, at + Vector2.up * .12f, new Vector2(r * .75f * sq, r * .75f * .62f / sq), 0, A(Crust, keep), 0, Seed, 0);
                // crust wedges fly off spinning, each landing and skidding
                for (int k = 0; k < 7; k++)
                {
                    float u = (t - .06f) / (.75f + .2f * H(Seed, k)); if (u <= 0 || u >= 1) continue;
                    Vector2 d = Dir(Seed + 3, k, 7, .62f), p = at + d * r * (.4f + 1.4f * EaseOut(u)) + Vector2.up * (2.4f * H(Seed, k + 9) * u * (1 - u) * 2 + .15f);
                    float rot = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg + u * 540 * (k % 2 == 0 ? 1 : -1);
                    b.Quad(PieS, p, new Vector2(.42f, .42f) * (.8f + .4f * H(Seed, k + 2)), rot, A(Crust, 1.1f * (1 - u * u)), Mathf.Repeat(rot / 360f, 1), Seed + k, 2);
                }
                // cherry blobs thrown out, each leaving a small splash where it lands
                for (int k = 0; k < 14; k++)
                {
                    float lf = .45f + .3f * H(Seed, k + 30), u = t / lf; Vector2 d = Dir(Seed, k, 14, .58f);
                    Vector2 land = at + d * r * (1.05f + .7f * H(Seed, k + 31));
                    if (u < 1)
                    {
                        Vector2 p = Vector2.Lerp(at, land, EaseOut(u)) + Vector2.up * (1.8f * H(Seed, k + 9) * 4 * u * (1 - u) + .2f);
                        b.Quad(FxShape.Disc, p, .13f + .07f * H(Seed, k + 3), A(Cherry, 1), 0, Seed + k);
                        b.Quad(FxShape.Glow, p + new Vector2(-.04f, .04f), .05f, A(White, .7f), 0, Seed);
                    }
                    else b.Floor(FxShape.Disc, land, .2f + .1f * H(Seed, k + 3), A(Cherry, .9f * fade), 0, Seed + k);
                }
                // a low ring of flour blown out by the splat, steam billowing up out of the hot filling
                for (int k = 0; k < 10; k++)
                {
                    float u = Rise(t, .02f, .9f); Vector2 p = at + Dir(Seed + 3, k, 10, .6f) * r * (.4f + 1.0f * EaseOut(u));
                    b.Quad(FxShape.Dust, p, new Vector2(.7f, .4f) * (.7f + u), 0, A(new Color(.98f, .95f, .88f), .75f * (1 - u)), u, Seed + k);
                }
                for (int k = 0; k < 6; k++)
                {
                    float u = Rise(t, .05f + k * .06f, 1.0f + k * .04f); if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + new Vector2((k - 2.5f) * .35f + Mathf.Sin(u * 5 + k) * .2f, .3f + 1.8f * EaseOut(u));
                    b.Quad(FxShape.Smoke, p, .35f + .55f * u, 0, A(Steam, .7f * Mathf.Min(1, u * 4) * (1 - u)), u + k, Seed + k + 30);
                }
                // the filling sizzles: a few hot glints winking on the splat
                for (int k = 0; k < 8; k++)
                {
                    float u = (t * 1.5f + H(Seed, k + 40)) % 1;
                    Glint(b, at + Dir(Seed, k + 20, 8, .55f) * r * .7f + Vector2.up * u * .5f, .12f, Gold, .8f * (1 - u) * fade, k * 40);
                }
            }
        }
        /// <summary>Step 8 feast (weather, a light touch): a banquet floats up across the field: loaves, apples, pies and little pots drift
        /// upward on gentle arcs, turning, each dropping crumbs; thin threads of kitchen steam rise from the floor and gold cinders twinkle.
        /// Every few seconds one dish gets a gleam. No wash over the arena.</summary>
        private sealed class Feast : FxEffect
        {
            protected override float Life => 5;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                float age = now - Born, total = Mathf.Max(1, End - Born), on = Rise(age, 0, .6f) * Fade(age, total - .9f, total);
                if (Moment.Kind != BondMomentKind.Weather) on = Rise(age, 0, .3f) * Fade(age, .8f, 1.4f);
                Rect a = Arena;
                // threads of kitchen steam from the floor
                for (int k = 0; k < 6; k++)
                {
                    float x = a.xMin + a.width * (k + .3f + .4f * H(Seed, k + 3)) / 6f, y0 = a.yMin + a.height * (.15f + .7f * H(Seed, k + 4));
                    for (int s = 0; s < 4; s++)
                    {
                        float u = (age * .3f + s * .25f + H(Seed, k)) % 1;
                        Vector2 p = new Vector2(x + Mathf.Sin(u * 7 + k) * .35f, y0 + u * 2.6f);
                        b.Quad(FxShape.Smoke, p, new Vector2(.3f + .45f * u, .35f + .5f * u), 0, A(Steam, .35f * on * Mathf.Sin(u * Mathf.PI)), u, Seed + k * 4 + s);
                    }
                }
                // the banquet
                for (int k = 0; k < 12; k++)
                {
                    float u = (age * .1f + H(Seed, k)) % 1, kind = k % 4;
                    Vector2 p = new Vector2(a.xMin + 1 + (a.width - 2) * ((H(Seed, k + 20) + Mathf.Sin(age * .5f + k) * .02f) % 1), a.yMin - .5f + u * (a.height + 1));
                    float s = .95f + .35f * H(Seed, k + 5), rot = Mathf.Sin(age * 1.4f + k) * 22;
                    float al = on * Mathf.Min(1, Mathf.Sin(u * Mathf.PI) * 3);
                    if (al <= .01f) continue;
                    for (int m = 0; m < 3; m++) { float v = (age * 1.3f + m / 3f + H(Seed, k + m)) % 1; b.Quad(FxShape.Disc, p + new Vector2((m - 1) * s * .4f, -s * .5f - v * .9f), .04f, A(Gold, al * (1 - v)), 0, Seed); }
                    b.Quad(FxShape.Glow, p, s * 1.5f, A(Orange, .14f * al), 0, Seed + k);
                    if (kind == 0) b.Quad(Food, p, new Vector2(s * 1.2f, s * 1.2f), rot, A(new Color(.9f, .62f, .32f), al), Mathf.Repeat(rot / 360f, 1), Seed + k, 0);
                    else if (kind == 1) b.Quad(Food, p, new Vector2(s * .85f, s * .85f), rot, A(new Color(.95f, .2f, .16f), al), Mathf.Repeat(rot / 360f, 1), Seed + k, 1);
                    else if (kind == 2) PieAt(b, p, s * 1.05f, .62f, rot, al, Seed + k);
                    else { b.Quad(Pot, p, new Vector2(s, s), rot * .5f, A(Iron, al), Mathf.Repeat(rot * .5f / 360f, 1), Seed + k, 0); b.Quad(FxShape.Glow, p + Vector2.up * s * .38f, new Vector2(s * .55f, s * .12f), rot * .5f, A(Broth, .8f * al), 0, Seed); }
                    // now and then a dish catches a gleam
                    float gl = Mathf.Pow(Mathf.Max(0, Mathf.Sin(age * 1.3f + k * 2.4f)), 30);
                    if (gl > .02f) Glint(b, p + new Vector2(-s * .35f, s * .3f), .45f, White, gl * al, age * 60);
                }
                for (int k = 0; k < 36; k++)
                {
                    float u = (age * (.1f + .1f * H(Seed, k)) + H(Seed, k + 80)) % 1;
                    Vector2 p = new Vector2(a.xMin + a.width * H(Seed, k + 120) + Mathf.Sin(age + k) * .4f, a.yMin + u * a.height);
                    Glint(b, p, .08f + .07f * H(Seed, k + 9), k % 2 == 0 ? Gold : Orange, .8f * on * Mathf.Sin(u * Mathf.PI), age * 30 + k * 20);
                }
            }
        }
        /// <summary>The hearth's regular meal (trait): a big silver platter floats in above the hero, the cloche lifts off in a puff of steam
        /// with a bright ding to show a roast loaf, a pie and apples; gold crumbs sprinkle down, the dish bobs and fades.</summary>
        private sealed class Platter : FxEffect
        {
            protected override float Life => 1.6f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                if (t < 0) return;
                float on = Rise(t, 0, .2f) * Fade(t, 1.2f, 1.6f), lift = EaseOut(Rise(t, .3f, .7f)), drop = EaseOut3(Rise(t, 0, .25f));
                Vector2 c = At + Vector2.up * (2.4f + .9f * (1 - drop) + .12f * Mathf.Sin(t * 5));
                b.Quad(FxShape.Glow, c, 2.0f, A(Orange, .2f * on), 0, Seed);
                // the platter: a silver oval with a bright rim, the food on it
                b.Quad(FxShape.Disc, c + Vector2.down * .32f, new Vector2(1.45f, .34f), 0, A(new Color(.62f, .62f, .68f), on), 0, Seed);
                b.Quad(FxShape.Disc, c + Vector2.down * .27f, new Vector2(1.3f, .26f), 0, A(new Color(.88f, .88f, .94f), on), 0, Seed);
                b.Quad(Food, c + new Vector2(-.15f, .08f), new Vector2(.85f, .85f), 0, A(new Color(.9f, .6f, .3f), on), 0, Seed, 0);
                PieAt(b, c + new Vector2(.8f, -.05f), .5f, .6f, 0, on, Seed + 2);
                b.Quad(Food, c + new Vector2(-.95f, -.04f), new Vector2(.4f, .4f), 0, A(new Color(.95f, .2f, .16f), on), 0, Seed, 1);
                b.Quad(Food, c + new Vector2(-.6f, -.14f), new Vector2(.36f, .36f), 0, A(new Color(.5f, .85f, .25f), on), 0, Seed + 1, 1);
                // the cloche lifts and tilts away
                Vector2 cl = c + new Vector2(.45f * lift, .32f + 1.3f * lift);
                b.Quad(Pot, cl, new Vector2(1.45f, 1.3f), 25 * lift, A(new Color(.82f, .84f, .9f), on * (1 - lift * .8f)), Mathf.Repeat(25 * lift / 360f, 1), Seed, 2);
                for (int k = 0; k < 7; k++)
                {
                    float u = Rise(t, .3f + k * .05f, 1.3f); if (u <= 0 || u >= 1) continue;
                    b.Quad(FxShape.Smoke, c + new Vector2((k - 3f) * .3f + Mathf.Sin(u * 6 + k) * .15f, .3f + u * 1.6f), .3f + .5f * u, A(Steam, .75f * Mathf.Min(1, u * 4) * (1 - u)), u + k, Seed + k);
                }
                // the cloche flashes as it lifts, and a ding sparkles off its rim
                Glint(b, cl + new Vector2(-.6f, .2f), .7f, White, Fade(t, .35f, .7f) * Rise(t, .3f, .38f), t * 120);
                for (int k = 0; k < 12; k++)
                {
                    float u = (t * 1.1f + H(Seed, k)) % 1; if (t < .4f) break;
                    b.Quad(FxShape.Disc, c + new Vector2((H(Seed, k + 5) - .5f) * 2.2f, -.45f - u * 1.4f), .045f, A(Gold, on * (1 - u)), 0, Seed);
                }
            }
        }
    }
}
