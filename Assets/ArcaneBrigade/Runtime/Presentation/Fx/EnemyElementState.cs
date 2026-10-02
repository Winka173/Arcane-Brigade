using UnityEngine;
using static ArcaneBrigade.FxKit;

namespace ArcaneBrigade
{
    /// <summary>An enemy struck by an element carries it: charred and smouldering, a frost crust, dripping wet, crackling static,
    /// poison bubbles, wrapped vines, ink stains, glitter, a gust, cracks and grit, paper tags, notes, a ticking clock, dream
    /// stars, prism glints. Small (inside the body), a handful of quads each, following the enemy; Tint is the cheap fallback.</summary>
    public static class EnemyElementState
    {
        private static Vector2 V(float x, float y) => new Vector2(x, y);

        /// <summary>The fallback past the detail cap: one soft glow of the element over the body.</summary>
        public static void Tint(FxBatch b, ElementLook look, Vector2 c, float h, float alpha)
            => b.Quad(FxShape.Glow, c, V(h * .42f, h * .55f), 0, A(RulesV3Ink.LookColour(look), .32f * alpha), 0, 0, 0);

        /// <param name="c">Body centre.</param><param name="h">Body height.</param><param name="age">Seconds since the mark began.</param>
        public static void Draw(FxBatch b, ElementLook look, Vector2 c, float h, float a, float age, float now, int seed)
        {
            float w = h * .36f, s = h / 1.1f; Vector2 feet = c + Vector2.down * h * .46f;
            float grow = EaseOut(age / .35f); int k;
            switch (look)
            {
                case ElementLook.Fire: // charred and smouldering: a scorched lower body, licking flames, embers and a smoke thread
                {
                    b.Quad(FxShape.Shade, c + Vector2.down * h * .18f, V(w * 1.05f, h * .32f), 0, A(new Color(.09f, .03f, .015f), .6f * a), 0, seed);
                    b.Quad(FxShape.Glow, c + Vector2.down * h * .2f, V(w * 1.1f, h * .3f), 0, A(new Color(1f, .35f, .05f), .45f * a * (.75f + .25f * Mathf.Sin(now * 9 + seed))), 0, seed);
                    for (k = 0; k < 4; k++)
                    {
                        float x = (k - 1.5f) * w * .5f + (H(seed, k) - .5f) * w * .2f, fl = .75f + .3f * Mathf.Sin(now * (11 + k * 3) + seed + k), big = k == 1 || k == 2 ? 1.3f : .85f;
                        Vector2 fp = c + V(x, -h * .02f - Mathf.Abs(k - 1.5f) * h * .1f);
                        b.Quad(FxShape.Flame, fp + V(0, .12f * s * big * fl), V(.15f * s * big, .26f * s * big * fl) * grow, (H(seed, k + 9) - .5f) * 24 + Mathf.Sin(now * 5 + k) * 8, A(new Color(1f, .42f, .06f), .95f * a), now * 1.4f + k, seed + k, 2);
                        b.Quad(FxShape.Flame, fp + V(0, .07f * s * big * fl), V(.07f * s * big, .14f * s * big * fl) * grow, 0, A(new Color(1f, .85f, .3f), .9f * a), now * 1.7f + k, seed + k + 5, 2);
                    }
                    for (k = 0; k < 4; k++)
                    {
                        float u = Mathf.Repeat(now * .75f + k * .25f + H(seed, k + 3), 1);
                        Vector2 p = c + V((H(seed, k + 20) - .5f) * w * 1.4f + Mathf.Sin(u * 7 + k) * .06f, -h * .05f + u * h * .85f);
                        Glint(b, p, .07f * s, new Color(1f, .7f, .2f), a * (1 - u) * 1.2f, u * 90);
                    }
                    float sm = Mathf.Repeat(now * .45f + H(seed, 30), 1);
                    b.Quad(FxShape.Smoke, c + V(Mathf.Sin(sm * 4) * .08f, h * .45f + sm * .5f * s), V(.13f + .14f * sm, .16f + .16f * sm) * s, 0, A(new Color(.16f, .13f, .12f), .5f * a * (1 - sm) * Mathf.Clamp01(sm * 5)), sm, seed);
                    break;
                }
                case ElementLook.Ice: // frost crust: crystals grown over the feet and shoulders, a cold sheen, a glint
                {
                    b.Quad(FxShape.Glow, c, V(w * 1.3f, h * .6f), 0, A(new Color(.55f, .9f, 1f), .32f * a), 0, seed);
                    b.Quad(FxShape.Glow, c + V(-w * .3f, h * .2f), V(w * .5f, h * .25f), 0, A(new Color(.85f, .97f, 1f), .3f * a), 0, seed);
                    b.Quad(FxShape.Dust, feet, V(w * 1.5f, .14f * s), 0, A(new Color(.75f, .92f, 1f), .5f * a), 0, seed);
                    for (k = 0; k < 9; k++)
                    {
                        bool shoulder = k >= 7; float side = k % 2 == 0 ? -1 : 1, fan = shoulder ? 0 : (k - 3f) / 3.5f;
                        Vector2 root = shoulder ? c + V(side * w * .55f, h * .28f) : feet + V(fan * w * 1.15f, .03f + (1 - Mathf.Abs(fan)) * .05f);
                        float len = (.09f + .08f * H(seed, k)) * s * grow * (shoulder ? .8f : 1.2f - .4f * Mathf.Abs(fan)), tilt = shoulder ? side * 35 : -fan * 62 + (H(seed, k + 4) - .5f) * 14;
                        Vector2 dir = new Vector2(-Mathf.Sin(tilt * Mathf.Deg2Rad), Mathf.Cos(tilt * Mathf.Deg2Rad));
                        b.Quad(FxShape.Disc, root + dir * len * .8f, V(.024f * s, len * (1.1f + .3f * H(seed, k + 8))), tilt, A(new Color(.6f, .9f, 1f), .85f * a), 0, seed + k);
                        b.Quad(FxShape.Glow, root + dir * len * 1.6f, V(.05f, .05f) * s, 0, A(Color.white, .6f * a), 0, seed + k);
                    }
                    float gl = Mathf.Repeat(now * .7f + H(seed, 7), 1);
                    if (gl < .2f) Glint(b, c + V((H(seed, (int)(now * .7f)) - .5f) * w * 1.6f, -h * .3f + .1f), .16f * s, Color.white, a * Mathf.Sin(gl / .2f * Mathf.PI), gl * 120);
                    break;
                }
                case ElementLook.Water: // dripping wet: a sheen, drops sliding off and falling into a puddle at the feet
                {
                    b.Quad(FxShape.Shade, feet, V(w * 1.4f, .12f * s), 0, A(new Color(.06f, .2f, .55f), .55f * a), 0, seed);
                    b.Quad(FxShape.Glow, feet + V(-w * .2f, .02f), V(w * .9f, .06f * s), 0, A(new Color(.4f, .7f, 1f), .35f * a), 0, seed);
                    b.Quad(FxShape.Glow, c + V(-w * .35f, h * .22f), V(.12f, .2f) * s, 25, A(new Color(.6f, .85f, 1f), .3f * a), 0, seed);
                    for (k = 0; k < 4; k++)
                    {
                        float u = Mathf.Repeat(now * 1.1f + k * .29f + H(seed, k), 1), x = (H(seed, k + 5) - .5f) * w * 1.6f;
                        float form = Mathf.Clamp01(u / .25f), fall = Mathf.Clamp01((u - .25f) / .75f);
                        Vector2 p = c + V(x, h * (.25f - .1f * (k % 2)) - fall * fall * (h * .25f + h * .46f - (k % 2) * .1f));
                        b.Quad(FxShape.Disc, p, V(.03f, .03f + .025f * fall) * s * (.5f + .5f * form), 0, A(new Color(.55f, .82f, 1f), .9f * a), 0, seed + k);
                        if (fall > .9f) Glint(b, feet + V(x, .02f), .07f * s, new Color(.7f, .9f, 1f), a * (1 - fall) * 8);
                    }
                    break;
                }
                case ElementLook.Storm: // crackling static: short arcs flick across the body, sparks blink
                {
                    b.Quad(FxShape.Glow, c, V(w * 1.1f, h * .5f), 0, A(new Color(.3f, .6f, 1f), .16f * a * (.6f + .4f * Mathf.Sin(now * 31 + seed))), 0, seed);
                    int tick = Mathf.FloorToInt(now * 14);
                    for (k = 0; k < 2; k++)
                    {
                        if (H(seed + k, tick) < .3f) continue;
                        float an = H(seed + k * 3, tick + 7) * 6.28f;
                        Vector2 p0 = c + V(Mathf.Cos(an) * w * .8f, Mathf.Sin(an) * h * .38f), p1 = c + V(-Mathf.Cos(an + .9f) * w * .9f, -Mathf.Sin(an + .9f) * h * .3f);
                        Bolt(b, p0, Vector2.Lerp(p0, p1, .7f), .016f * s, new Color(.45f, .8f, 1f), seed + tick * 5 + k, .09f * s, .9f * a, 4);
                    }
                    for (k = 0; k < 2; k++)
                    {
                        float u = H(seed + 11 * k, tick / 2);
                        Glint(b, c + V((u - .5f) * w * 2.2f, (H(seed + 5 * k, tick / 2 + 3) - .5f) * h * .9f), .11f * s, new Color(.8f, .95f, 1f), a * (u > .45f ? .9f : 0), u * 90);
                    }
                    break;
                }
                case ElementLook.Poison: // poison bubbles rising out of the body and popping, a sickly drip at the feet
                {
                    b.Quad(FxShape.Glow, c + Vector2.down * h * .1f, V(w * 1.1f, h * .45f), 0, A(new Color(.45f, .9f, .1f), .3f * a), 0, seed);
                    b.Quad(FxShape.Shade, feet, V(w * 1.1f, .1f * s), 0, A(new Color(.15f, .3f, .03f), .55f * a), 0, seed);
                    for (k = 0; k < 4; k++)
                    {
                        float u = Mathf.Repeat(now * .85f + k * .26f + H(seed, k), 1);
                        Vector2 p = c + V((H(seed, k + 2) - .5f) * w * 1.5f + Mathf.Sin(u * 9 + k) * .04f, -h * .3f + u * h * .85f);
                        float r = (.05f + .07f * u) * s;
                        if (u < .86f)
                        {
                            b.Quad(FxShape.Shock, p, V(r, r), 0, A(new Color(.55f, 1f, .2f), .95f * a), .25f, seed + k);
                            Glint(b, p + V(-r * .35f, r * .35f), r * .5f, new Color(.9f, 1f, .7f), .6f * a);
                        }
                        else Glint(b, p, r * 1.8f, new Color(.7f, 1f, .3f), a * (1 - u) * 7, 45);
                    }
                    break;
                }
                case ElementLook.Bloom: // vines wrapped round the body, a few leaves on them and one bud
                {
                    Vector2 prev = default; int n = 7;
                    for (k = 0; k <= n; k++)
                    {
                        float u = k / (float)n; if (u > grow + .001f) break;
                        Vector2 p = c + V(Mathf.Sin(u * 9.4f + seed) * w * .95f, -h * .42f + u * h * .78f);
                        if (k > 0)
                        {
                            float front = Mathf.Cos(u * 9.4f + seed);
                            b.Link(prev, p, .045f * s, A(front > 0 ? new Color(.35f, .75f, .2f) : new Color(.16f, .38f, .1f), (front > 0 ? .95f : .55f) * a), seed + k, .5f);
                            if (k % 2 == 0) b.Quad(FxShape.Disc, p + V(.05f, .02f) * s, V(.045f, .085f) * s, -40 + 80 * (k % 4) / 2f + Mathf.Sin(now * 3 + k) * 8, A(new Color(.45f, .85f, .25f), .85f * a), 0, seed + k);
                        }
                        prev = p;
                    }
                    if (grow > .95f) Glint(b, c + V(Mathf.Sin(9.4f + seed) * w * .95f, h * .38f), .12f * s, new Color(1f, .55f, .8f), .9f * a, now * 20);
                    break;
                }
                case ElementLook.Void: // ink stains: dark blots on the body with a slow drip, a stain on the floor, a violet glimmer
                {
                    b.Quad(FxShape.Glow, c, V(w * 1.2f, h * .5f), 0, A(new Color(.5f, .25f, .9f), .22f * a), 0, seed);
                    b.Quad(FxShape.Shade, feet + V(w * .2f, 0), V(w * .9f, .09f * s), 0, A(new Color(.04f, .01f, .08f), .7f * a), 0, seed);
                    for (k = 0; k < 3; k++)
                    {
                        Vector2 p = c + V((H(seed, k) - .5f) * w * 1.3f, (H(seed, k + 3) - .3f) * h * .6f);
                        float r = (.09f + .07f * H(seed, k + 6)) * s * grow;
                        b.Quad(FxShape.Shade, p, V(r, r * .85f), H(seed, k) * 90, A(new Color(.03f, .0f, .06f), .95f * a), 0, seed + k);
                        b.Quad(FxShape.Glow, p + V(-r * .3f, r * .3f), V(r * .45f, r * .35f), 0, A(new Color(.8f, .6f, 1f), .75f * a), 0, seed + k);
                        b.Quad(FxShape.Glow, p, V(r * 1.4f, r * 1.2f), 0, A(new Color(.45f, .15f, .9f), .35f * a), 0, seed + k);
                        float u = Mathf.Repeat(now * .5f + H(seed, k + 9), 1);
                        Vector2 drip = p + V(0, -r * .6f - u * h * .3f);
                        b.Quad(FxShape.Shade, drip, V(.035f, .08f * (1 - u * .5f)) * s, 0, A(new Color(.03f, 0f, .06f), .9f * a * (1 - u)), 0, seed + k);
                        b.Quad(FxShape.Glow, drip + V(0, -.04f) * s, V(.04f, .04f) * s, 0, A(new Color(.75f, .45f, 1f), .7f * a * (1 - u)), 0, seed + k);
                    }
                    break;
                }
                case ElementLook.Light: // glitter: motes circling the body, a soft halo at the crown
                {
                    b.Quad(FxShape.Rays, c + V(0, h * .35f), V(.42f, .42f) * s, now * 20, A(new Color(1f, .95f, .7f), .65f * a), now * .3f, seed, 2);
                    b.Quad(FxShape.Glow, c, V(w * 1.2f, h * .55f), 0, A(new Color(1f, .9f, .6f), .2f * a), 0, seed);
                    for (k = 0; k < 4; k++)
                    {
                        float an = now * 2.2f + k * 1.57f + seed;
                        Vector2 p = c + V(Mathf.Cos(an) * w * 1.1f, Mathf.Sin(an) * h * .12f + (k - 1.5f) * h * .14f);
                        Glint(b, p, (.11f + .05f * Mathf.Sin(now * 9 + k)) * s, new Color(1f, .92f, .6f), a * (Mathf.Sin(an) < 0 ? 1 : .55f), an * 30);
                    }
                    break;
                }
                case ElementLook.Wind: // a gust coiled round the body: two pale streaks orbiting and a tumbling leaf
                {
                    for (int strand = 0; strand < 2; strand++)
                    {
                        float base0 = now * 5 + strand * 3.14f; Vector2 prev = default;
                        for (k = 0; k <= 4; k++)
                        {
                            float an = base0 - k * .32f;
                            Vector2 p = c + V(Mathf.Cos(an) * w * 1.25f, Mathf.Sin(an) * h * .16f + (strand == 0 ? -h * .15f : h * .18f));
                            if (k > 0) b.Link(prev, p, .045f * s * (1 - k * .16f), A(new Color(.8f, 1f, .92f), .9f * a * (1 - k * .18f)), seed + k, .7f);
                            prev = p;
                        }
                    }
                    float lt = Mathf.Repeat(now * .9f, 1);
                    b.Quad(FxShape.Disc, c + V(Mathf.Cos(lt * 6.28f) * w * 1.3f, Mathf.Sin(lt * 6.28f) * h * .2f + h * .05f), V(.06f * Mathf.Abs(Mathf.Cos(now * 7)) + .01f, .04f) * s, now * 300, A(new Color(.6f, .9f, .45f), .8f * a), 0, seed);
                    break;
                }
                case ElementLook.Earth: // cracked and gritty: glowing cracks, grit trickling, a little dust at the feet
                {
                    b.Quad(FxShape.Cracks, c + V(0, -h * .05f), V(w * 1.1f, h * .42f), 0, A(new Color(1f, .6f, .25f), .55f * a), 1, seed, 1);
                    b.Quad(FxShape.Dust, feet, V(w * 1.4f, .13f * s), 0, A(new Color(.55f, .4f, .28f), .6f * a), now * .2f, seed);
                    for (k = 0; k < 3; k++)
                    {
                        float u = Mathf.Repeat(now * 1.3f + k * .33f + H(seed, k), 1);
                        Vector2 p = c + V((H(seed, k + 4) - .5f) * w * 1.4f, h * .1f - u * u * h * .55f);
                        b.Quad(FxShape.Disc, p, V(.035f, .03f) * s, u * 200, A(new Color(.75f, .55f, .35f), .9f * a * (1 - u * u)), 0, seed + k);
                    }
                    break;
                }
                case ElementLook.Paper: // paper tags stuck on the body, flapping, each with a glowing glyph
                {
                    for (k = 0; k < 3; k++)
                    {
                        Vector2 p = c + V((k - 1) * w * .7f, (H(seed, k) - .4f) * h * .5f);
                        float flap = .55f + .45f * Mathf.Abs(Mathf.Sin(now * (4 + k) + k * 2)), turn = (H(seed, k + 3) - .5f) * 50;
                        b.Quad(FxShape.Disc, p, V(.075f * flap, .1f) * s * grow, turn, A(new Color(1f, .93f, .78f), .75f * a), 0, seed + k);
                        b.Quad(FxShape.Glow, p, V(.06f, .06f) * s * grow, 0, A(new Color(1f, .65f, .2f), .7f * a * (.6f + .4f * Mathf.Sin(now * 5 + k))), 0, seed + k);
                    }
                    break;
                }
                case ElementLook.Sound: // two notes bobbing over the head
                {
                    for (k = 0; k < 2; k++)
                    {
                        Vector2 p = c + V((k - .5f) * w * 1.3f, h * .5f + .12f * s + Mathf.Sin(now * 4 + k * 2) * .06f);
                        Color col = new Color(1f, .55f, .95f), lit = A(col, .95f * a * grow);
                        b.Quad(FxShape.Disc, p, V(.055f, .042f) * s, -20, lit, 0, seed + k);
                        b.Link(p + V(.045f, 0) * s, p + V(.045f, .17f) * s, .018f * s, lit, seed + k, .8f);
                        if (k == 1) b.Link(p + V(.045f, .17f) * s, p + V(-.09f, .2f) * s, .018f * s, lit, seed + k, .8f);
                        else b.Link(p + V(.045f, .17f) * s, p + V(.1f, .1f) * s, .016f * s, lit, seed + k, .8f);
                    }
                    break;
                }
                case ElementLook.Time: // a small clock face over the head, its hands spinning
                {
                    Vector2 p = c + V(0, h * .5f + .2f * s);
                    b.Quad(FxShape.Glow, p, V(.3f, .3f) * s * grow, 0, A(new Color(.95f, .85f, .55f), .6f * a), 0, seed);
                    b.Quad(FxShape.Disc, p, V(.17f, .17f) * s * grow, 0, A(new Color(.3f, .25f, .12f), .5f * a), 0, seed);
                    float m = now * 6, hr = now * .6f;
                    b.Link(p, p + V(Mathf.Sin(m), Mathf.Cos(m)) * .15f * s, .02f * s, A(new Color(1f, .95f, .75f), a * grow), seed, .8f);
                    b.Link(p, p + V(Mathf.Sin(hr), Mathf.Cos(hr)) * .1f * s, .026f * s, A(new Color(1f, .8f, .4f), a * grow), seed + 1, .8f);
                    break;
                }
                case ElementLook.Dream: // drowsy: violet haze, little stars drifting off the head
                {
                    b.Quad(FxShape.Smoke, c + V(0, h * .3f), V(w * 1.1f, h * .22f), 0, A(new Color(.45f, .3f, .7f), .5f * a), now * .2f, seed);
                    b.Quad(FxShape.Glow, c + V(0, h * .3f), V(w * 1.2f, h * .3f), 0, A(new Color(.6f, .45f, 1f), .25f * a), 0, seed);
                    for (k = 0; k < 3; k++)
                    {
                        float u = Mathf.Repeat(now * .5f + k * .33f, 1);
                        Glint(b, c + V((k - 1) * .12f + Mathf.Sin(u * 6 + k) * .1f, h * .4f + u * .5f * s), (.1f + .06f * u) * s, new Color(.85f, .75f, 1f), a * Mathf.Sin(u * Mathf.PI), u * 180);
                    }
                    break;
                }
                case ElementLook.Crystal: // prism glints: facets flashing through the colours over the body
                {
                    for (k = 0; k < 3; k++)
                    {
                        Vector2 p = c + V((k - 1) * w * .65f, (H(seed, k) - .5f) * h * .5f);
                        Color hue = Color.HSVToRGB(Mathf.Repeat(now * .4f + k * .33f, 1), .45f, 1);
                        b.Quad(FxShape.Disc, p, V(.035f, .09f) * s * grow, 30 + k * 50 + now * 40, A(hue, .7f * a), 0, seed + k);
                        Glint(b, p, .1f * s, hue, a * Mathf.Max(0, Mathf.Sin(now * 3 + k * 2)));
                    }
                    break;
                }
                default: Tint(b, look, c, h, a); break;
            }
        }
    }
}
