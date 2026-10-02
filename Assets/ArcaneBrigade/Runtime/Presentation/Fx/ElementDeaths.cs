using UnityEngine;
using static ArcaneBrigade.FxKit;

namespace ArcaneBrigade
{
    /// <summary>An enemy killed by an element dies its way, drawn over the ordinary squash-and-burst (DeathFeedback stays as it
    /// is): ash and embers, ice shards, ink, water, sparks, goo, petals, rubble, a gust, light, notes, sand, stars, prism glass,
    /// paper scraps. Each one a short pure function of time.</summary>
    public static class ElementDeaths
    {
        public const float Life = 1.6f;
        private static Vector2 V(float x, float y) => new Vector2(x, y);

        /// <summary>A piece thrown from <paramref name="from"/> that falls under gravity and comes to rest on the floor line.</summary>
        public static Vector2 Toss(Vector2 from, Vector2 vel, float g, float t, float floorY, out bool landed)
        {
            float drop = Mathf.Max(0, from.y - floorY), land = (vel.y + Mathf.Sqrt(vel.y * vel.y + 2 * g * drop)) / g;
            landed = t >= land; if (landed) t = land;
            Vector2 p = from + vel * t + Vector2.down * (.5f * g * t * t);
            if (landed) p.y = floorY; return p;
        }

        public static void Draw(FxBatch b, ElementLook look, Vector2 c, float h, Vector2 dir, float t, float now, int seed)
        {
            float s = h / 1.1f, w = h * .36f, floor = c.y - h * .46f; Vector2 feet = V(c.x, floor);
            float end = Fade(t, Life - .45f, Life); int k;
            switch (look)
            {
                case ElementLook.Fire: // crumbles to ash and embers: a charred figure sinks into a smoking heap, embers fly up
                {
                    float crumble = EaseOut(t / 1.0f);
                    b.Quad(FxShape.Glow, c, V(h * .8f, h * .8f), 0, A(new Color(1f, .45f, .1f), .8f * Fade(t, 0, .3f)), 0, seed);
                    float fig = h * .5f * (1 - crumble);
                    if (fig > .02f)
                    {
                        b.Quad(FxShape.Shade, feet + V(0, fig), V(w * (.9f + .4f * crumble), fig), 0, A(new Color(.07f, .05f, .05f), .95f), 0, seed);
                        b.Quad(FxShape.Cracks, feet + V(0, fig), V(w * (.9f + .4f * crumble), fig), 0, A(new Color(1f, .45f, .08f), .9f * Fade(t, .5f, .9f)), 1, seed, 2);
                        b.Quad(FxShape.Glow, feet + V(0, fig * 1.9f), V(w * .9f, .12f * s), 0, A(new Color(1f, .55f, .15f), .7f), 0, seed);
                    }
                    b.Quad(FxShape.Dust, feet, V(w * (.6f + 1.1f * crumble), .16f * s * (.4f + .6f * crumble)), 0, A(new Color(.45f, .4f, .38f), .9f * end), t * .3f, seed);
                    b.Floor(FxShape.Glow, feet, w * 1.6f, A(new Color(1f, .3f, .05f), .45f * Fade(t, .3f, 1.3f)), 0, seed);
                    for (k = 0; k < 4; k++)
                    {
                        float fl = Fade(t, .25f + k * .1f, .9f + k * .1f) * (.75f + .3f * Mathf.Sin(now * (13 + k * 4) + k));
                        b.Quad(FxShape.Flame, feet + V((k - 1.5f) * w * .45f, .14f * s * fl), V(.16f, .3f * fl) * s, Mathf.Sin(now * 6 + k) * 12, A(new Color(1f, .45f, .08f), .95f), now * 1.5f + k, seed + k, 2);
                    }
                    for (k = 0; k < 6; k++)
                        Glint(b, feet + V((H(seed, k + 60) - .5f) * w * 2f * crumble, .02f + H(seed, k + 61) * .06f), .06f * s, new Color(1f, .45f, .1f), Rise(t, .3f, .6f) * end * (.5f + .5f * Mathf.Sin(now * 7 + k * 2)));
                    for (k = 0; k < 14; k++) // ash flakes peel off the figure and drift down
                    {
                        float born = H(seed, k) * .7f, u = t - born; if (u < 0) continue;
                        Vector2 p = Toss(c + V((H(seed, k + 1) - .5f) * w * 2, (H(seed, k + 2) - .3f) * h * .6f * (1 - crumble)), V((H(seed, k + 3) - .5f) * 1.2f, .3f), 1.5f, u, floor - .05f + H(seed, k + 4) * .1f, out bool landed);
                        b.Quad(FxShape.Shade, p + V(Mathf.Sin(u * 6 + k) * .05f, 0), V(.045f, .03f) * s, u * 200 + k * 40, A(new Color(.2f, .18f, .18f), .9f * end), 0, seed + k);
                    }
                    for (k = 0; k < 14; k++) // embers
                    {
                        float u = t - H(seed, k + 30) * .25f; if (u < 0) continue;
                        Vector2 p = c + V((H(seed, k + 31) - .5f) * (1.6f + u), 0) * s + V(Mathf.Sin(u * 5 + k) * .1f, (1.2f + 1.4f * H(seed, k + 32)) * u - .45f * u * u);
                        Glint(b, p, (.07f + .05f * H(seed, k)) * s, k % 3 == 0 ? new Color(1f, .9f, .4f) : new Color(1f, .5f, .12f), Fade(u, .3f, 1.25f) * (.6f + .4f * Mathf.Sin(now * 23 + k)), u * 200);
                    }
                    SmokeColumn(b, feet + V(0, .2f), .3f * s, 1.3f * s, t - .1f, new Color(.13f, .11f, .1f), seed, 5);
                    break;
                }
                case ElementLook.Ice: // shatters into ice shards: a frozen flash, shards fly and skid on the floor, a cold mist
                {
                    b.Quad(FxShape.Glow, c, V(h * .8f, h * .8f), 0, A(new Color(.55f, .9f, 1f), .6f * Fade(t, 0, .25f)), 0, seed);
                    if (t < .08f) b.Quad(FxShape.Disc, c, V(w * 1.05f, h * .5f), 0, A(new Color(.55f, .88f, 1f), .3f), 0, seed);
                    b.Quad(FxShape.Dust, feet, V(w * (1.2f + 1.6f * EaseOut(t * 2)), .16f * s), 0, A(new Color(.8f, .95f, 1f), .7f * end), 0, seed);
                    for (k = 0; k < 16; k++)
                    {
                        float an = (k + H(seed, k) * .7f) / 16 * Mathf.PI * 2, sp = 1.1f + 1.5f * H(seed, k + 1);
                        Vector2 v0 = V(Mathf.Cos(an) * sp, Mathf.Abs(Mathf.Sin(an)) * sp * .7f + 1.2f) * s;
                        Vector2 p = Toss(c + V(Mathf.Cos(an) * w * .5f, Mathf.Sin(an) * h * .3f), v0, 9, t, floor + Mathf.Sin(an) * .45f + (H(seed, k + 2) - .5f) * .2f, out bool landed);
                        float len = (.08f + .1f * H(seed, k + 3)) * s, spin = landed ? H(seed, k) * 180 : an * Mathf.Rad2Deg + t * (300 + 400 * H(seed, k + 4));
                        b.Quad(FxShape.Disc, p, V(len * .28f, len), spin, A(new Color(.62f, .9f, 1f), .95f * end), 0, seed + k);
                        if (k % 4 == 0) Glint(b, p, .13f * s, Color.white, end * (.5f + .5f * Mathf.Sin(now * 17 + k)), t * 200);
                    }
                    for (k = 0; k < 3; k++)
                    {
                        float u = Mathf.Clamp01(t / 1.2f);
                        b.Quad(FxShape.Smoke, feet + V((k - 1) * w * 1.3f * (1 + u), .15f + u * .3f), V(.3f + .35f * u, .2f + .2f * u) * s, 0, A(new Color(.72f, .86f, .95f), .45f * (1 - u)), u, seed + k);
                    }
                    break;
                }
                case ElementLook.Void: // dissolves into ink: the body melts down into a spreading black-violet pool, drips run, wisps rise
                {
                    float melt = EaseOut(t / .9f), fig = h * .5f * (1 - melt);
                    b.Quad(FxShape.Shade, feet, V(w * (1 + 1.8f * melt), .2f * s * (.5f + .5f * melt)), 0, A(new Color(.04f, .01f, .07f), .95f * end), 0, seed);
                    b.Quad(FxShape.Glow, feet, V(w * (1.3f + 2f * melt), .24f * s), 0, A(new Color(.45f, .15f, .9f), .45f * end), 0, seed);
                    for (k = 0; k < 5; k++)
                    {
                        Vector2 sp = feet + V((k - 2) * w * .55f * melt, (H(seed, k + 70) - .5f) * .12f * s);
                        b.Quad(FxShape.Shade, sp, V(.2f, .09f) * s * (.4f + .6f * melt) * (.7f + .6f * H(seed, k + 71)), 0, A(new Color(.03f, 0f, .06f), .95f * end), 0, seed + k);
                        b.Quad(FxShape.Glow, sp + V(-.05f, .03f) * s, V(.06f, .025f) * s, 0, A(new Color(.85f, .65f, 1f), .7f * end * melt), 0, seed + k);
                    }
                    if (fig > .02f)
                    {
                        b.Quad(FxShape.Glow, feet + V(0, fig), V(w * (1.3f + .5f * melt), fig * 1.15f), 0, A(new Color(.55f, .25f, 1f), .55f), 0, seed);
                        b.Quad(FxShape.Shade, feet + V(0, fig), V(w * (1 + .5f * melt), fig), 0, A(new Color(.03f, 0f, .06f), 1f), 0, seed);
                    }
                    for (k = 0; k < 6; k++) // drips running down the melting figure
                    {
                        float x = (k - 2.5f) * w * .35f, u = Mathf.Clamp01((t - H(seed, k) * .3f) / .7f);
                        b.Quad(FxShape.Shade, feet + V(x, fig * 2 * (1 - u) + .05f), V(.035f, .09f) * s, 0, A(new Color(.05f, .01f, .09f), .9f * (1 - u)), 0, seed + k);
                    }
                    for (k = 0; k < 8; k++) // splashed droplets
                    {
                        float an = H(seed, k + 10) * Mathf.PI;
                        Vector2 p = Toss(c, V(Mathf.Cos(an) * 2.2f, Mathf.Sin(an) * 2.4f) * s, 10, t, floor + (H(seed, k) - .5f) * .3f, out bool landed);
                        b.Quad(FxShape.Shade, p, landed ? V(.09f, .04f) * s : V(.05f, .06f) * s, 0, A(new Color(.05f, .01f, .1f), .95f * end), 0, seed + k);
                    }
                    for (k = 0; k < 3; k++)
                    {
                        float u = Mathf.Clamp01((t - .2f - k * .15f) / 1f); if (u <= 0) continue;
                        b.Quad(FxShape.Smoke, feet + V((k - 1) * w * .8f, .2f + u * 1f * s), V(.18f + .2f * u, .22f + .2f * u) * s, 0, A(new Color(.3f, .15f, .5f), .55f * (1 - u)), u, seed + k);
                        Glint(b, feet + V((k - 1) * w * .8f + Mathf.Sin(u * 7) * .1f, .3f + u * 1.2f * s), .1f * s, new Color(.75f, .5f, 1f), (1 - u));
                    }
                    break;
                }
                case ElementLook.Paper: // bursts into paper scraps that flutter down, a few glyphs flaring as they go
                {
                    b.Quad(FxShape.Glow, c, V(h * .7f, h * .7f), 0, A(new Color(1f, .9f, .7f), .7f * Fade(t, 0, .25f)), 0, seed);
                    for (k = 0; k < 16; k++)
                    {
                        float an = H(seed, k) * Mathf.PI * 2, sp = 1.4f + 1.8f * H(seed, k + 1), burst = 1 - Mathf.Exp(-t * 5);
                        Vector2 p = c + V(Mathf.Cos(an), Mathf.Sin(an) * .8f + .5f) * sp * .42f * burst * s;
                        float fall = Mathf.Max(0, t - .25f); p += V(Mathf.Sin(fall * (3 + k % 3) + k) * .22f, -fall * fall * .55f - fall * .35f);
                        p.y = Mathf.Max(p.y, floor + (H(seed, k + 2) - .5f) * .4f);
                        float flip = Mathf.Cos(t * (6 + 5 * H(seed, k + 3)) + k);
                        Color paper = k % 5 == 0 ? new Color(1f, .82f, .6f) : new Color(1f, .96f, .86f);
                        b.Quad(FxShape.Disc, p, V(.08f * (.2f + .8f * Mathf.Abs(flip)), .06f) * s, t * 90 * (k % 2 == 0 ? 1 : -1) + k * 30, A(paper, .85f * end), 0, seed + k);
                        if (k % 4 == 0) b.Quad(FxShape.Glow, p, V(.08f, .08f) * s, 0, A(new Color(1f, .6f, .2f), .8f * Fade(t, .3f, 1f)), 0, seed + k);
                    }
                    b.Quad(FxShape.Dust, feet, V(w * 2, .12f * s), 0, A(new Color(.85f, .78f, .65f), .4f * Fade(t, 0, .8f)), t, seed);
                    break;
                }
                case ElementLook.Water: // splashes into water: the body slumps, a crown of drops leaps up and rains down, a puddle spreads
                {
                    float slump = EaseOut(t / .22f), fig = h * .5f * (1 - slump);
                    if (fig > .02f) b.Quad(FxShape.Disc, feet + V(0, fig), V(w * (1 + slump * .6f), fig), 0, A(new Color(.3f, .6f, 1f), .6f), 0, seed);
                    float pool = EaseOut(t / .5f);
                    b.Quad(FxShape.Shade, feet, V(w * (1 + 1.8f * pool), .17f * s), 0, A(new Color(.05f, .17f, .5f), .7f * end), 0, seed);
                    b.Quad(FxShape.Glow, feet + V(-w * .3f, .03f), V(w * (.7f + 1.3f * pool), .08f * s), 0, A(new Color(.45f, .75f, 1f), .5f * end), 0, seed);
                    for (k = 0; k < 18; k++)
                    {
                        float an = Mathf.Lerp(.25f, Mathf.PI - .25f, H(seed, k)), sp = (2.6f + 1.8f * H(seed, k + 1)) * s, u = t - .06f;
                        if (u < 0) continue;
                        Vector2 p = Toss(feet + V(Mathf.Cos(an) * w * .6f, .1f), V(Mathf.Cos(an) * sp * .55f, Mathf.Sin(an) * sp), 11, u, floor + (H(seed, k + 2) - .5f) * .35f, out bool landed);
                        float sz = (.035f + .03f * H(seed, k + 3)) * s;
                        if (!landed) b.Quad(FxShape.Disc, p, V(sz, sz * 1.4f), 0, A(new Color(.55f, .82f, 1f), .95f), 0, seed + k);
                        else Glint(b, p, .1f * s, new Color(.7f, .9f, 1f), Fade(u, .45f, .8f) * end);
                    }
                    b.Quad(FxShape.Smoke, feet + V(0, .3f + t * .3f), V(.45f, .3f) * s * (1 + t), 0, A(new Color(.6f, .8f, 1f), .35f * Fade(t, 0, .8f)), t, seed);
                    break;
                }
                case ElementLook.Storm: // sparks out: a crackle of arcs, a spray of sparks bouncing away, a last fizz and smoke
                {
                    b.Quad(FxShape.Glow, c, V(h * .9f, h * .9f), 0, A(new Color(.6f, .85f, 1f), Fade(t, 0, .2f)), 0, seed);
                    int tick = Mathf.FloorToInt(t * 20);
                    if (t < .45f)
                        for (k = 0; k < 4; k++)
                        {
                            float an = (k + H(seed + tick, k)) * 1.57f;
                            Bolt(b, c, c + V(Mathf.Cos(an) * (.6f + .5f * H(seed, k + tick)), Mathf.Sin(an) * .55f) * s, .025f * s, new Color(.45f, .8f, 1f), seed + tick * 7 + k, .14f, Fade(t, .2f, .45f), 5);
                        }
                    for (k = 0; k < 16; k++)
                    {
                        float an = H(seed, k + 20) * Mathf.PI * 2, sp = (2.5f + 3f * H(seed, k + 21)) * s;
                        Vector2 v0 = V(Mathf.Cos(an) * sp, Mathf.Abs(Mathf.Sin(an)) * sp * .6f + 1f);
                        Vector2 p = Toss(c, v0, 12, t, floor + (H(seed, k) - .5f) * .4f, out bool landed), q = landed ? p : Toss(c, v0, 12, Mathf.Max(0, t - .035f), -99, out _);
                        float al = Fade(t, .35f, .8f + .4f * H(seed, k));
                        if (!landed) b.Link(q, p, .03f * s, A(new Color(.75f, .95f, 1f), al), seed + k, .7f);
                        else Glint(b, p, .08f * s, new Color(.6f, .9f, 1f), al * .8f);
                    }
                    b.Quad(FxShape.Shade, feet, V(w * 1.5f, .14f * s), 0, A(new Color(.05f, .05f, .08f), .7f * end), 0, seed);
                    SmokeColumn(b, feet + V(0, .25f), .25f * s, 1f * s, t - .25f, new Color(.2f, .2f, .25f), seed, 4);
                    for (k = 0; k < 3; k++) { float at = .55f + k * .22f + H(seed, k) * .1f; if (t > at && t < at + .08f) Glint(b, feet + V((H(seed, k + 40) - .5f) * w * 2, .15f), .2f * s, Color.white, 1, k * 30); }
                    break;
                }
                case ElementLook.Poison: // melts into bubbling toxic goo: a bright green pool that boils and gives off gas
                {
                    float melt = EaseOut(t / .6f), fig = h * .45f * (1 - melt);
                    if (fig > .02f) b.Quad(FxShape.Shade, feet + V(0, fig), V(w * (1 + .3f * melt), fig), 0, A(new Color(.18f, .35f, .04f), .9f), 0, seed);
                    b.Quad(FxShape.Shade, feet, V(w * (1 + 1.4f * melt), .18f * s), 0, A(new Color(.12f, .28f, .02f), .8f * end), 0, seed);
                    b.Quad(FxShape.Glow, feet, V(w * (1 + 1.4f * melt), .16f * s), 0, A(new Color(.5f, 1f, .1f), .5f * end), 0, seed);
                    for (k = 0; k < 7; k++)
                    {
                        float u = Mathf.Repeat(t * 1.6f + H(seed, k), 1);
                        Vector2 p = feet + V((H(seed, k + 3) - .5f) * w * 2.2f * melt, .02f + u * .12f);
                        float r = (.03f + .07f * u) * s;
                        if (u < .85f) b.Quad(FxShape.Shock, p, V(r, r * .9f), 0, A(new Color(.6f, 1f, .25f), .9f * end), .25f, seed + k);
                        else Glint(b, p, r * 2, new Color(.8f, 1f, .4f), end * (1 - u) * 6);
                    }
                    for (k = 0; k < 4; k++)
                    {
                        float u = Mathf.Clamp01((t - k * .15f) / 1.2f); if (u <= 0 || u >= 1) continue;
                        b.Quad(FxShape.Smoke, feet + V((H(seed, k + 9) - .5f) * w * 2, .25f + u * 1.1f * s), V(.25f + .3f * u, .22f + .25f * u) * s, 0, A(new Color(.35f, .55f, .1f), .55f * (1 - u)), u, seed + k);
                    }
                    break;
                }
                case ElementLook.Bloom: // bursts into petals and leaves, and a sprout pushes up where it stood
                {
                    b.Quad(FxShape.Glow, c, V(h * .7f, h * .7f), 0, A(new Color(1f, .6f, .8f), .6f * Fade(t, 0, .3f)), 0, seed);
                    for (k = 0; k < 14; k++)
                    {
                        float an = H(seed, k) * Mathf.PI * 2, sp = (1.8f + 1.8f * H(seed, k + 1)) * s;
                        Vector2 v0 = V(Mathf.Cos(an) * sp * .7f, Mathf.Sin(an) * sp * .5f + 1.5f);
                        Vector2 p = Toss(c, v0, 4.5f, t, floor + (H(seed, k + 2) - .5f) * .4f, out bool landed);
                        if (!landed) p.x += Mathf.Sin(t * 6 + k) * .08f;
                        bool leaf = k % 3 == 0;
                        b.Quad(FxShape.Disc, p, (leaf ? V(.04f, .08f) : V(.045f, .05f)) * s, landed ? k * 25 : t * 360 + k * 40, A(leaf ? new Color(.45f, .85f, .25f) : new Color(1f, .55f, .78f), .9f * end), 0, seed + k);
                    }
                    float sprout = EaseOut3((t - .25f) / .5f);
                    if (sprout > 0)
                    {
                        Vector2 top = feet + V(Mathf.Sin(t * 3) * .03f, .38f * s * sprout);
                        b.Link(feet, top, .035f * s, A(new Color(.35f, .75f, .2f), end), seed, .5f);
                        b.Quad(FxShape.Disc, Vector2.Lerp(feet, top, .5f) + V(.07f, 0) * s, V(.07f, .035f) * s * sprout, 25, A(new Color(.45f, .85f, .25f), end), 0, seed);
                        b.Quad(FxShape.Disc, Vector2.Lerp(feet, top, .65f) + V(-.07f, 0) * s, V(.07f, .035f) * s * sprout, -25, A(new Color(.45f, .85f, .25f), end), 0, seed);
                        for (k = 0; k < 5; k++) { float an = k * 1.2566f + t; b.Quad(FxShape.Disc, top + V(Mathf.Cos(an), Mathf.Sin(an)) * .06f * s * sprout, V(.045f, .03f) * s * sprout, an * Mathf.Rad2Deg, A(new Color(1f, .5f, .75f), end), 0, seed + k); }
                        Glint(b, top, .12f * s * sprout, new Color(1f, .95f, .6f), end);
                    }
                    break;
                }
                case ElementLook.Earth: // crumbles to rubble: chunks drop into a heap in a cloud of dust over a cracked floor
                {
                    b.Floor(FxShape.Cracks, feet, w * 2.4f, A(new Color(1f, .55f, .2f), .6f * end), Mathf.Clamp01(t * 3), seed, 1);
                    for (k = 0; k < 12; k++)
                    {
                        float born = H(seed, k) * .25f, u = t - born; if (u < 0) continue;
                        Vector2 from = c + V((H(seed, k + 1) - .5f) * w * 1.6f, (H(seed, k + 2) - .3f) * h * .5f);
                        Vector2 p = Toss(from, V((H(seed, k + 3) - .5f) * 1.6f, .6f) * s, 10, u, floor + (H(seed, k + 4) - .5f) * .25f, out bool landed);
                        float sz = (.06f + .06f * H(seed, k + 5)) * s;
                        b.Quad(FxShape.Smoke, p, V(sz, sz * .85f) * 1.4f, landed ? k * 33 : u * 400, A(k % 3 == 0 ? new Color(.78f, .58f, .38f) : new Color(.6f, .44f, .3f), end), .5f, seed + k);
                        if (k % 4 == 0 && !landed) b.Quad(FxShape.Glow, p, V(sz, sz) * 1.2f, 0, A(new Color(1f, .6f, .25f), .5f), 0, seed + k);
                    }
                    for (k = 0; k < 4; k++)
                    {
                        float u = Mathf.Clamp01((t - .1f - k * .06f) / 1.1f); if (u <= 0 || u >= 1) continue;
                        b.Quad(FxShape.Dust, feet + V((k - 1.5f) * w * .9f * (1 + u), .1f + u * .25f), V(.35f + .45f * u, .2f + .15f * u) * s, 0, A(new Color(.55f, .42f, .3f), .75f * (1 - u)), u, seed + k);
                    }
                    break;
                }
                case ElementLook.Wind: // blown away: the body tears into specks swept off along the blow, wind streaks race past
                {
                    Vector2 d = dir.x >= 0 ? Vector2.right : Vector2.left;
                    for (k = 0; k < 18; k++)
                    {
                        float born = H(seed, k) * .3f, u = Mathf.Max(0, t - born);
                        Vector2 from = c + V((H(seed, k + 1) - .5f) * w * 2, (H(seed, k + 2) - .5f) * h * .8f);
                        Vector2 p = from + d * (u * 1.2f + u * u * 2.2f) * s + V(0, Mathf.Sin(u * 7 + k) * .18f + u * .4f);
                        b.Quad(FxShape.Dust, p, V(.09f, .05f) * s * (1 - u * .4f), 0, A(new Color(.7f, .78f, .72f), .9f * Fade(u, .5f, 1.1f)), u, seed + k);
                        if (k % 3 == 0) b.Quad(FxShape.Disc, p, V(.05f * Mathf.Abs(Mathf.Cos(u * 9 + k)) + .01f, .035f) * s, u * 500, A(new Color(.6f, .9f, .45f), Fade(u, .6f, 1.2f)), 0, seed + k);
                    }
                    for (k = 0; k < 5; k++)
                    {
                        float u = Mathf.Clamp01((t - k * .08f) / .7f); if (u <= 0 || u >= 1) continue;
                        Vector2 mid = c + V(0, (k - 2) * h * .2f) + d * Mathf.Lerp(-1.2f, 2.2f, u) * s;
                        b.Beam(mid - d * .45f * s, mid + d * .45f * s, .05f * s, A(new Color(.85f, 1f, .95f), .7f * Mathf.Sin(u * Mathf.PI)), 0, seed + k, .5f);
                    }
                    b.Quad(FxShape.Vortex, c, V(w * 1.6f, h * .5f), 0, A(new Color(.75f, 1f, .9f), .45f * Fade(t, .1f, .6f)), t * 2, seed, 2);
                    break;
                }
                case ElementLook.Light: // rises into light: a shaft lifts it, motes float up, the glow thins away
                {
                    float up = Rise(t, 0, .15f) * Fade(t, .6f, 1.2f);
                    b.Beam(feet, feet + V(0, 2.4f * s), .3f * s, A(new Color(1f, .95f, .75f), .75f * up), t, seed, .3f);
                    b.Quad(FxShape.Rays, c, V(h * .8f, h * .8f), t * 30, A(new Color(1f, .9f, .6f), .6f * Fade(t, .2f, .9f)), t, seed, 3);
                    b.Quad(FxShape.Glow, c + V(0, t * .6f), V(w * 1.4f, h * .6f) * (1 - t * .4f), 0, A(Color.white, .7f * Fade(t, 0, .7f)), 0, seed);
                    for (k = 0; k < 12; k++)
                    {
                        float u = t - H(seed, k) * .4f; if (u < 0) continue;
                        Vector2 p = feet + V((H(seed, k + 1) - .5f) * w * 2.2f + Mathf.Sin(u * 4 + k) * .1f, u * (1.1f + .8f * H(seed, k + 2)) * s);
                        Glint(b, p, (.08f + .05f * H(seed, k + 3)) * s, new Color(1f, .92f, .6f), Fade(u, .5f, 1.1f), u * 120);
                    }
                    break;
                }
                case ElementLook.Sound: // pops into notes: a bright note-burst, notes sail off on arcs and bob away
                {
                    b.Quad(FxShape.Glow, c, V(h * .6f, h * .6f), 0, A(new Color(1f, .5f, .95f), .7f * Fade(t, 0, .25f)), 0, seed);
                    for (k = 0; k < 8; k++)
                    {
                        float an = (k + .5f) / 8 * Mathf.PI * 2, u = EaseOut(t / 1.1f);
                        Vector2 p = c + V(Mathf.Cos(an) * 1.1f, Mathf.Sin(an) * .6f + .4f) * u * s + V(0, Mathf.Sin(t * 7 + k) * .08f);
                        Color col = A(k % 2 == 0 ? new Color(1f, .55f, .95f) : new Color(.7f, .6f, 1f), Fade(t, .7f, 1.3f));
                        float sz = (.9f + .3f * Mathf.Sin(t * 12 + k)) * s;
                        b.Quad(FxShape.Disc, p, V(.07f, .055f) * sz, -20, col, 0, seed + k);
                        b.Link(p + V(.06f, 0) * sz, p + V(.06f, .22f) * sz, .022f * sz, col, seed + k, .8f);
                        b.Link(p + V(.06f, .22f) * sz, p + V(.15f, .13f) * sz, .02f * sz, col, seed + k, .8f);
                    }
                    break;
                }
                case ElementLook.Time: // runs out as sand: the figure pours down into a cone, clock hands whirl and stop
                {
                    float pour = EaseOut(t / 1.1f), fig = h * .5f * (1 - pour);
                    if (fig > .02f) b.Quad(FxShape.Dust, feet + V(0, fig), V(w * 1.05f, fig), 0, A(new Color(.95f, .8f, .5f), 1f), t, seed);
                    if (fig > .02f) b.Quad(FxShape.Glow, feet + V(0, fig), V(w * 1.1f, fig), 0, A(new Color(1f, .8f, .45f), .35f), 0, seed);
                    b.Quad(FxShape.Dust, feet, V(w * (.5f + 1.1f * pour), .15f * s * (.4f + .7f * pour)), 0, A(new Color(.9f, .75f, .45f), .9f * end), 0, seed + 1);
                    for (k = 0; k < 10; k++)
                    {
                        float u = Mathf.Repeat(t * 2.5f + H(seed, k), 1);
                        Vector2 p = feet + V((H(seed, k + 1) - .5f) * w * 1.6f * (1 - u * .5f), fig * 2 * (1 - u) + .05f);
                        b.Quad(FxShape.Glow, p, V(.03f, .05f) * s, 0, A(new Color(1f, .88f, .55f), Fade(t, .8f, 1.2f)), 0, seed + k);
                    }
                    float spin = (1 - Mathf.Exp(-t * 3)) * 25, al = Fade(t, .7f, 1.2f);
                    Vector2 face = c + V(0, h * .2f);
                    b.Quad(FxShape.Glow, face, V(.35f, .35f) * s, 0, A(new Color(1f, .85f, .5f), .45f * al), 0, seed);
                    b.Link(face, face + V(Mathf.Sin(spin), Mathf.Cos(spin)) * .3f * s, .03f * s, A(new Color(1f, .95f, .75f), al), seed, .8f);
                    b.Link(face, face + V(Mathf.Sin(spin * .2f), Mathf.Cos(spin * .2f)) * .18f * s, .04f * s, A(new Color(1f, .8f, .4f), al), seed + 1, .8f);
                    break;
                }
                case ElementLook.Dream: // drifts off as stars: the body dims to a violet haze that lifts away in twinkling stars
                {
                    b.Quad(FxShape.Smoke, c + V(0, t * .5f), V(w * 1.4f, h * .5f) * (1 + t * .4f), 0, A(new Color(.4f, .28f, .65f), .6f * Fade(t, .1f, 1.1f)), t, seed);
                    b.Quad(FxShape.Glow, c + V(0, t * .5f), V(w * 1.5f, h * .55f), 0, A(new Color(.65f, .5f, 1f), .5f * Fade(t, 0, .8f)), 0, seed);
                    for (k = 0; k < 12; k++)
                    {
                        float u = t - H(seed, k) * .35f; if (u < 0) continue;
                        Vector2 p = c + V((H(seed, k + 1) - .5f) * w * 2.4f + Mathf.Sin(u * 3 + k) * .15f, (H(seed, k + 2) - .5f) * h * .8f + u * .9f * s);
                        float tw = .55f + .45f * Mathf.Sin(now * 13 + k * 2);
                        Glint(b, p, (.07f + .07f * H(seed, k + 3)) * s, k % 3 == 0 ? Color.white : new Color(.8f, .7f, 1f), Fade(u, .6f, 1.2f) * tw, u * 60);
                    }
                    break;
                }
                case ElementLook.Crystal: // splits into prism glass: facets tumble out flashing through the colours, white rays
                {
                    b.Quad(FxShape.Rays, c, V(h * .9f, h * .9f), t * 50, A(Color.white, .7f * Fade(t, 0, .5f)), t, seed, 5);
                    for (k = 0; k < 10; k++)
                    {
                        float an = (k + H(seed, k)) / 10 * Mathf.PI * 2, u = EaseOut(t / .8f);
                        Vector2 p = c + V(Mathf.Cos(an), Mathf.Sin(an) * .7f) * u * 1.1f * s + V(0, -t * t * .4f);
                        Color hue = Color.HSVToRGB(Mathf.Repeat(k * .1f + t * .8f, 1), .5f, 1);
                        b.Quad(FxShape.Disc, p, V(.05f, .12f) * s, an * Mathf.Rad2Deg + t * 400, A(hue, .85f * Fade(t, .7f, 1.2f)), 0, seed + k);
                        if (k % 2 == 0) Glint(b, p, .14f * s, hue, Fade(t, .6f, 1.1f) * (.5f + .5f * Mathf.Sin(now * 19 + k)));
                    }
                    break;
                }
            }
        }
    }
}
