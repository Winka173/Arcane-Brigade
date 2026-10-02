using UnityEngine;
using static ArcaneBrigade.FxKit;

namespace ArcaneBrigade
{
    /// <summary>Two elements landing on the same enemy within a moment react, presentation only: each pair its own reaction.
    /// Sized to a small group of enemies (about a unit and a half across), never more.</summary>
    public static class ElementCombos
    {
        public enum Kind { SteamBurst, ShockedWater, ToxicFlare, FireWhirl, Implosion, CrystalConductor, FlashFreeze, Wildfire, MudGeyser, PlasmaOrb, PlagueGust, Overgrowth }
        public const float Life = 1.6f;
        private static Vector2 V(float x, float y) => new Vector2(x, y);

        private static bool Is(ElementLook a, ElementLook b, ElementLook x, ElementLook y) => a == x && b == y || a == y && b == x;

        /// <summary>The reaction of two elements, if they have one. <paramref name="other"/> is the element a Void pull swallows.</summary>
        public static bool For(ElementLook a, ElementLook b, out Kind kind, out ElementLook other)
        {
            other = ElementLook.None; kind = Kind.SteamBurst;
            if (a == b) return false;
            if (a == ElementLook.Void || b == ElementLook.Void) { kind = Kind.Implosion; other = a == ElementLook.Void ? b : a; return true; }
            if (Is(a, b, ElementLook.Fire, ElementLook.Ice)) kind = Kind.SteamBurst;
            else if (Is(a, b, ElementLook.Storm, ElementLook.Water)) kind = Kind.ShockedWater;
            else if (Is(a, b, ElementLook.Fire, ElementLook.Poison)) kind = Kind.ToxicFlare;
            else if (Is(a, b, ElementLook.Wind, ElementLook.Fire)) kind = Kind.FireWhirl;
            else if (Is(a, b, ElementLook.Ice, ElementLook.Storm)) kind = Kind.CrystalConductor;
            else if (Is(a, b, ElementLook.Water, ElementLook.Ice)) kind = Kind.FlashFreeze;
            else if (Is(a, b, ElementLook.Bloom, ElementLook.Fire)) kind = Kind.Wildfire;
            else if (Is(a, b, ElementLook.Earth, ElementLook.Water)) kind = Kind.MudGeyser;
            else if (Is(a, b, ElementLook.Storm, ElementLook.Fire)) kind = Kind.PlasmaOrb;
            else if (Is(a, b, ElementLook.Poison, ElementLook.Wind)) kind = Kind.PlagueGust;
            else if (Is(a, b, ElementLook.Bloom, ElementLook.Water)) kind = Kind.Overgrowth;
            else return false;
            return true;
        }

        /// <param name="at">The floor under the struck enemy.</param><param name="s">Size scale (1 = an ordinary enemy).</param>
        public static void Draw(FxBatch b, Kind kind, ElementLook other, Vector2 at, float s, float t, float now, int seed)
        {
            float end = Fade(t, Life - .45f, Life); int k;
            switch (kind)
            {
                case Kind.SteamBurst: // fire meets frost: a hiss of white steam billows up in a mushroom, droplets condense and fall
                {
                    b.Floor(FxShape.Glow, at, 1f * s, A(new Color(1f, .45f, .15f), .5f * Fade(t, 0, .35f)), 0, seed);
                    for (k = 0; k < 6; k++) Glint(b, at + Dir(seed, k, 6) * .5f * s * EaseOut(t * 4), .16f * s, new Color(.7f, .95f, 1f), Fade(t, .05f, .3f), k * 15);
                    for (k = 0; k < 9; k++)
                    {
                        float born = k * .05f, u = Mathf.Clamp01((t - born) / 1.25f); if (u <= 0 || u >= 1) continue;
                        float side = (H(seed, k) - .5f) * 1.2f, rise = EaseOut(u) * (1.1f + .5f * H(seed, k + 1));
                        Vector2 p = at + V(side * (.3f + .9f * u), .15f + rise) * s;
                        b.Quad(FxShape.Smoke, p, V(.35f + .55f * u, .3f + .45f * u) * s, 0, A(new Color(.9f, .93f, .97f), .8f * (1 - u) * Rise(u, 0, .08f)), u, seed + k);
                    }
                    for (k = 0; k < 10; k++)
                    {
                        float u = t - .35f - H(seed, k + 5) * .4f; if (u < 0) continue;
                        Vector2 p = ElementDeaths.Toss(at + V((H(seed, k + 6) - .5f) * 1.6f, 1.3f + H(seed, k + 7) * .4f) * s, V(0, 0), 6, u, at.y + (H(seed, k) - .5f) * .4f, out bool landed);
                        if (!landed) b.Quad(FxShape.Disc, p, V(.025f, .04f) * s, 0, A(new Color(.7f, .9f, 1f), .9f * end), 0, seed + k);
                    }
                    break;
                }
                case Kind.ShockedWater: // storm meets tide: a puddle floods under the group and lightning skitters across its surface
                {
                    float pool = EaseOut(t / .25f);
                    b.Floor(FxShape.Shade, at, 1.4f * s * pool, A(new Color(.03f, .12f, .4f), .75f * end), 0, seed);
                    b.Floor(FxShape.Glow, at, 1.3f * s * pool, A(new Color(.25f, .6f, 1f), .5f * end * (.7f + .3f * Mathf.Sin(now * 37))), 0, seed);
                    int tick = Mathf.FloorToInt(t * 16);
                    if (t > .12f && t < 1.25f)
                        for (k = 0; k < 6; k++)
                        {
                            if (H(seed + k, tick) < .25f) continue;
                            float a0 = H(seed + k * 7, tick) * 6.28f, a1 = a0 + 1.4f + H(seed + k, tick + 3) * 2f;
                            Vector2 p0 = at + V(Mathf.Cos(a0) * 1.1f, Mathf.Sin(a0) * .62f) * s * (.3f + .7f * H(seed, k + tick)), p1 = at + V(Mathf.Cos(a1) * 1.1f, Mathf.Sin(a1) * .62f) * s * (.4f + .6f * H(seed + 1, k + tick));
                            Bolt(b, p0, p1, .03f * s, new Color(.5f, .85f, 1f), seed + tick * 11 + k, .18f * s, .95f * Fade(t, 1f, 1.25f), 6);
                            Glint(b, p1, .18f * s, Color.white, .9f, tick * 40);
                            b.Quad(FxShape.Disc, p0 + V(0, .1f + .25f * H(seed, tick + k)) * s, V(.03f, .045f) * s, 0, A(new Color(.6f, .85f, 1f), .9f), 0, seed + k);
                        }
                    break;
                }
                case Kind.ToxicFlare: // ember meets venom: the poison catches, a green fireball flares up into a sick mushroom of smoke
                {
                    float up = EaseOut(t / .5f);
                    b.Quad(FxShape.Glow, at + V(0, .5f * up) * s, V(.9f, .9f) * s * (.5f + .5f * up), 0, A(new Color(.75f, 1f, .2f), .8f * Fade(t, .2f, .8f)), 0, seed);
                    Vector2 ball = at + V(0, .45f + .35f * up) * s; float fb = Rise(t, 0, .1f) * Fade(t, .45f, 1f);
                    b.Quad(FxShape.Flame, ball + V(0, .25f) * s, V(.6f, .75f) * s * (.6f + .4f * up), 0, A(new Color(.5f, 1f, .12f), .95f * fb), now * 1.2f, seed, 2);
                    b.Quad(FxShape.Flame, ball + V(0, .12f) * s, V(.32f, .45f) * s * (.6f + .4f * up), 0, A(new Color(1f, .6f, .12f), .9f * fb), now * 1.5f + 3, seed + 9, 2);
                    b.Quad(FxShape.Glow, ball, V(.45f, .45f) * s, 0, A(new Color(.95f, 1f, .5f), .9f * fb), 0, seed);
                    for (k = 0; k < 4; k++)
                    {
                        float x = (k - 1.5f) * .3f * s, ht = (.3f + .15f * H(seed, k)) * s * fb;
                        b.Quad(FxShape.Flame, at + V(x, ht * .8f), V(.16f * s, ht), x * -40, A(new Color(.5f, 1f, .1f), .9f), now * 1.3f + k, seed + k, 2);
                    }
                    for (k = 0; k < 6; k++)
                    {
                        float u = Mathf.Clamp01((t - .25f - k * .06f) / 1.1f); if (u <= 0 || u >= 1) continue;
                        float an = (k / 5f - .5f) * 2.2f;
                        Vector2 p = at + V(Mathf.Sin(an) * (.3f + .5f * u), 1.1f + .55f * u - Mathf.Abs(Mathf.Sin(an)) * .3f) * s;
                        b.Quad(FxShape.Smoke, p, V(.35f + .3f * u, .28f + .2f * u) * s, 0, A(new Color(.25f, .38f, .08f), .8f * (1 - u)), u, seed + k);
                    }
                    for (k = 0; k < 10; k++)
                    {
                        float u = t - H(seed, k) * .3f; if (u < 0) continue;
                        Glint(b, at + V((H(seed, k + 1) - .5f) * (1 + u), u * (1.4f + H(seed, k + 2))) * s, .1f * s, new Color(.8f, 1f, .3f), Fade(u, .4f, 1f), u * 150);
                    }
                    break;
                }
                case Kind.FireWhirl: // wind meets ember: a twisting column of flame spins up, embers spiral round it
                {
                    float grow = EaseOut(t / .35f), fade = Fade(t, 1.05f, 1.5f);
                    b.Quad(FxShape.Vortex, at, V(.9f, .55f) * s * grow, 0, A(new Color(1f, .45f, .1f), .6f * fade), t * 2.5f, seed, 2);
                    for (k = 0; k < 10; k++)
                    {
                        float u = k / 9f, ht = u * 2.1f * s * grow, wide = (.12f + .4f * u) * s;
                        float sway = Mathf.Sin(now * 9 - u * 5 + seed) * wide * .6f;
                        b.Quad(FxShape.Flame, at + V(sway, ht + .15f * s), V(wide, .32f * s), sway * -60, A(Color.Lerp(new Color(1f, .6f, .15f), new Color(1f, .3f, .05f), u), .9f * fade * (1 - u * .35f)), now * 1.6f + k, seed + k, 2);
                    }
                    for (k = 0; k < 14; k++)
                    {
                        float u = Mathf.Repeat(t * .9f + k / 14f, 1), an = now * 8 + k * 2.1f;
                        Vector2 p = at + V(Mathf.Cos(an) * (.2f + .5f * u), u * 2.2f * grow + Mathf.Sin(an) * .1f) * s;
                        Glint(b, p, .09f * s, new Color(1f, .75f, .3f), fade * (Mathf.Sin(an) > 0 ? 1 : .45f), an * 40);
                    }
                    b.Quad(FxShape.Dust, at, V(1f, .3f) * s, 0, A(new Color(.4f, .3f, .25f), .6f * fade), t, seed);
                    break;
                }
                case Kind.Implosion: // the dark pull swallows another element: its fragments spiral into a black eye and pop
                {
                    Color col = RulesV3Ink.LookColour(other);
                    float pull = Mathf.Clamp01(t / .9f), eye = Rise(t, 0, .2f) * Fade(t, .9f, 1.1f);
                    Vector2 o = at + V(0, .45f) * s;
                    b.Quad(FxShape.Shade, o, V(.75f, .6f) * s * eye, 0, A(new Color(.03f, .01f, .06f), .85f), 0, seed);
                    b.Quad(FxShape.Vortex, o, V(1f, .75f) * s * eye, 0, A(new Color(.55f, .3f, 1f), .8f), t * 3, seed, 1);
                    for (k = 0; k < 16; k++)
                    {
                        float born = H(seed, k) * .35f, u = Mathf.Clamp01((t - born) / .6f); if (u <= 0 || u >= 1) continue;
                        float an = k * .39f + u * u * 5f, r = (1.5f + .4f * H(seed, k + 1)) * (1 - u * u) * s;
                        Vector2 p = o + V(Mathf.Cos(an) * r, Mathf.Sin(an) * r * .65f), q = o + V(Mathf.Cos(an - .25f) * r * 1.12f, Mathf.Sin(an - .25f) * r * .72f);
                        b.Link(q, p, .025f * s, A(col, .55f * u), seed + k, .7f);
                        b.Quad(k % 2 == 0 ? FxShape.Disc : FxShape.Sparkle, p, V(.05f, .08f) * s * (1 - u * .5f), an * Mathf.Rad2Deg, A(Color.Lerp(col, Color.white, .3f), .95f), 0, seed + k, 1.5f);
                    }
                    float pop = Rise(t, .9f, .98f) * Fade(t, 1f, 1.4f);
                    if (pop > 0)
                    {
                        b.Quad(FxShape.Glow, o, V(.9f, .9f) * s * (.5f + .7f * Rise(t, .9f, 1.3f)), 0, A(col, .9f * pop), 0, seed);
                        for (k = 0; k < 6; k++) Glint(b, o + Dir(seed, k, 6) * Rise(t, .9f, 1.4f) * 1f * s, .14f * s, col, pop, k * 20);
                    }
                    break;
                }
                case Kind.CrystalConductor: // frost meets storm: ice spikes burst from the floor and lightning leaps from tip to tip
                {
                    int n = 5; Vector2 prevTip = default;
                    float rise = EaseOut3((t - .05f) / .25f), sink = Fade(t, 1.1f, 1.5f);
                    for (k = 0; k < n; k++)
                    {
                        float an = (k + H(seed, k) * .5f) / n * 6.28f;
                        Vector2 root = at + V(Mathf.Cos(an) * .85f, Mathf.Sin(an) * .5f) * s, tip = root + V(0, (.45f + .25f * H(seed, k + 1)) * s * rise * sink);
                        if (rise <= 0) break;
                        b.Quad(FxShape.Disc, (root + tip) * .5f, V(.07f * s, (tip.y - root.y) * .5f + .01f), 0, A(new Color(.75f, .95f, 1f), .9f), 0, seed + k);
                        b.Quad(FxShape.Glow, tip, V(.18f, .18f) * s, 0, A(new Color(.6f, .9f, 1f), .7f * sink), 0, seed + k);
                        int hop = Mathf.FloorToInt(t * 12) % n;
                        if (k > 0 && t > .3f && t < 1.25f && (hop == k || hop == k - 1)) Bolt(b, prevTip, tip, .028f * s, new Color(.55f, .85f, 1f), seed + Mathf.FloorToInt(t * 24) + k, .15f * s, .95f, 6);
                        prevTip = tip;
                    }
                    b.Floor(FxShape.Dust, at, 1.2f * s, A(new Color(.8f, .95f, 1f), .55f * end), 0, seed);
                    if (t > 1.1f) for (k = 0; k < 8; k++) { Vector2 p = ElementDeaths.Toss(at + Dir(seed, k, 8) * .85f * s + V(0, .3f), Dir(seed, k + 3, 8) * 1.5f + V(0, 1.2f), 8, t - 1.1f, at.y - .3f, out _); b.Quad(FxShape.Disc, p, V(.025f, .07f) * s, k * 45 + t * 300, A(new Color(.8f, .96f, 1f), end), 0, seed + k); }
                    break;
                }
                case Kind.FlashFreeze: // tide meets frost: a splash leaps up and freezes solid in mid-air, then the ice sculpture cracks apart
                {
                    float freezeAt = .32f, ft = Mathf.Min(t, freezeAt); bool frozen = t >= freezeAt; float melt = Rise(t, 1.15f, 1.5f);
                    for (k = 0; k < 14; k++)
                    {
                        float an = Mathf.Lerp(.35f, Mathf.PI - .35f, H(seed, k)), sp = (2.4f + 1.4f * H(seed, k + 1)) * s;
                        Vector2 v0 = V(Mathf.Cos(an) * sp * .55f, Mathf.Sin(an) * sp);
                        Vector2 p = at + v0 * ft + V(0, -5.5f * ft * ft);
                        if (melt > 0) p = ElementDeaths.Toss(p, V(0, 0), 9, t - 1.15f, at.y - .2f, out _);
                        float sz = (.04f + .03f * H(seed, k + 2)) * s;
                        Vector2 vel = v0 + V(0, -11f * ft); float ang = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg - 90;
                        b.Quad(FxShape.Disc, p, frozen ? V(sz * .8f, sz * 2.6f) : V(sz, sz * 1.6f), ang, A(frozen ? new Color(.85f, .97f, 1f) : new Color(.35f, .65f, 1f), .95f * end), 0, seed + k);
                        if (frozen && k % 3 == 0) Glint(b, p, .15f * s, Color.white, Fade(t, freezeAt, freezeAt + .5f) + .3f * end * Mathf.Max(0, Mathf.Sin(now * 9 + k)), k * 20);
                    }
                    b.Floor(FxShape.Shade, at, .9f * s, A(new Color(.05f, .15f, .45f), .6f * Fade(t, freezeAt, freezeAt + .2f)), 0, seed);
                    b.Floor(FxShape.Dust, at, 1.3f * s * Rise(t, freezeAt, freezeAt + .25f), A(new Color(.85f, .96f, 1f), .7f * end), 0, seed);
                    if (frozen) b.Quad(FxShape.Glow, at + V(0, .6f) * s, V(1f, .8f) * s, 0, A(new Color(.7f, .95f, 1f), .7f * Fade(t, freezeAt, freezeAt + .3f)), 0, seed);
                    break;
                }
                case Kind.Wildfire: // bloom meets ember: the leaves catch and whirl upward burning, curling black, ash drifting
                {
                    b.Floor(FxShape.Glow, at, 1.1f * s, A(new Color(1f, .4f, .08f), .45f * end), 0, seed);
                    for (k = 0; k < 14; k++)
                    {
                        float u = Mathf.Clamp01((t - H(seed, k) * .3f) / 1.2f); if (u <= 0 || u >= 1) continue;
                        float an = k * 2.4f + u * 6;
                        Vector2 p = at + V(Mathf.Cos(an) * (.3f + .6f * u), u * 1.8f + Mathf.Sin(an) * .15f) * s;
                        Color leaf = u < .3f ? Color.Lerp(new Color(.45f, .85f, .25f), new Color(1f, .55f, .1f), u / .3f) : Color.Lerp(new Color(1f, .55f, .1f), new Color(.15f, .08f, .05f), (u - .3f) / .7f);
                        b.Quad(FxShape.Disc, p, V(.05f, .1f) * s, an * 60, A(leaf, .95f * (1 - u * .6f)), 0, seed + k);
                        b.Quad(FxShape.Flame, p + V(0, .06f) * s, V(.06f, .12f) * s, 0, A(new Color(1f, .55f, .12f), .9f * Fade(u, .3f, .7f)), now + k, seed + k, 1);
                    }
                    for (k = 0; k < 5; k++)
                        b.Quad(FxShape.Flame, at + V((k - 2) * .25f, .2f) * s, V(.14f, .32f * Fade(t, .5f, 1.3f) * Rise(t, 0, .2f)) * s, 0, A(new Color(1f, .5f, .1f), .9f), now, seed + k + 30, 1.5f);
                    SmokeColumn(b, at + V(0, .5f) * s, .3f * s, 1.2f * s, t - .2f, new Color(.15f, .12f, .1f), seed, 5);
                    break;
                }
                case Kind.MudGeyser: // stone meets tide: the ground spits a geyser of mud that splatters down in clods
                {
                    Color mud = new Color(.62f, .45f, .28f);
                    b.Floor(FxShape.Glow, at, 1f * s, A(new Color(1f, .6f, .3f), .35f * Fade(t, .1f, .7f)), 0, seed);
                    b.Floor(FxShape.Cracks, at, 1.1f * s, A(new Color(1f, .6f, .25f), .85f * Fade(t, .3f, .8f)), Rise(t, 0, .15f), seed, 1);
                    float jet = Rise(t, .05f, .25f) * Fade(t, .45f, .7f);
                    for (k = 0; k < 6; k++)
                    {
                        float y = k / 5f * 1.6f * s * jet;
                        b.Quad(FxShape.Smoke, at + V(Mathf.Sin(now * 13 + k) * .05f, y + .1f), V(.22f + .06f * k, .25f) * s * Mathf.Max(.2f, jet), 0, A(mud, .95f), now + k * .1f, seed + k);
                    }
                    for (k = 0; k < 12; k++)
                    {
                        float an = Mathf.Lerp(.5f, Mathf.PI - .5f, H(seed, k)), sp = (3.2f + 1.6f * H(seed, k + 1)) * s, u = t - .15f - H(seed, k + 2) * .15f;
                        if (u < 0) continue;
                        Vector2 p = ElementDeaths.Toss(at + V(0, .3f), V(Mathf.Cos(an) * sp * .45f, Mathf.Sin(an) * sp), 10, u, at.y + (H(seed, k + 3) - .5f) * .6f, out bool landed);
                        float sz = (.07f + .05f * H(seed, k + 4)) * s;
                        b.Quad(FxShape.Smoke, p, landed ? V(sz * 2f, sz * .8f) : V(sz, sz) * 1.3f, 0, A(mud, end), .4f, seed + k);
                        if (!landed) b.Quad(FxShape.Glow, p + V(-.02f, .02f), V(sz, sz) * .6f, 0, A(new Color(.75f, .85f, 1f), .5f), 0, seed + k);
                    }
                    b.Floor(FxShape.Dust, at, 1.1f * s * Rise(t, .2f, .8f), A(new Color(.5f, .36f, .22f), .8f * end), 0, seed);
                    break;
                }
                case Kind.PlasmaOrb: // storm meets ember: a crackling ball of plasma swells, spins rays, and bursts in sparks
                {
                    float swell = EaseOut(t / .6f), live = Fade(t, .75f, .85f); Vector2 o = at + V(0, .5f) * s;
                    b.Quad(FxShape.Glow, o, V(.7f, .7f) * s * (.3f + .7f * swell), 0, A(new Color(1f, .6f, .25f), .9f * live), 0, seed);
                    b.Quad(FxShape.Glow, o, V(.35f, .35f) * s * (.3f + .7f * swell), 0, A(new Color(.85f, .8f, 1f), live), 0, seed);
                    b.Quad(FxShape.Rays, o, V(1f, 1f) * s * swell, now * 90, A(new Color(1f, .75f, .4f), .6f * live), t, seed, 4);
                    int tick = Mathf.FloorToInt(t * 18);
                    for (k = 0; k < 3; k++)
                    {
                        float an = H(seed + k, tick) * 6.28f;
                        if (live > .1f) Bolt(b, o, o + V(Mathf.Cos(an), Mathf.Sin(an)) * .75f * s * swell, .022f * s, new Color(.55f, .8f, 1f), seed + tick * 3 + k, .12f, live, 5);
                    }
                    float burst = Rise(t, .78f, .9f);
                    if (burst > 0)
                    {
                        b.Quad(FxShape.Glow, o, V(1f, 1f) * s, 0, A(Color.white, .8f * Fade(t, .8f, 1.05f)), 0, seed);
                        for (k = 0; k < 14; k++)
                        {
                            float u = t - .8f; Vector2 d = Dir(seed, k, 14, .8f);
                            Vector2 p = o + d * (2.6f * u - 1.2f * u * u) * s;
                            b.Link(p - d * .12f * s, p, .03f * s, A(k % 2 == 0 ? new Color(1f, .7f, .3f) : new Color(.6f, .85f, 1f), Fade(u, .3f, .75f)), seed + k, .7f);
                        }
                    }
                    break;
                }
                case Kind.PlagueGust: // venom meets wind: a green squall of gas sweeps across the group, streaks and specks with it
                {
                    Vector2 d = V(1, .12f).normalized;
                    for (k = 0; k < 7; k++)
                    {
                        float u = Mathf.Clamp01((t - k * .07f) / 1.2f); if (u <= 0 || u >= 1) continue;
                        Vector2 p = at + d * Mathf.Lerp(-1.3f, 1.4f, u) * s + V(0, .3f + (k % 3 - 1) * .3f + Mathf.Sin(u * 6 + k) * .12f) * s;
                        b.Quad(FxShape.Smoke, p, V(.45f + .2f * u, .28f) * s, 0, A(new Color(.42f, .65f, .12f), .7f * Mathf.Sin(u * Mathf.PI)), u, seed + k);
                    }
                    for (k = 0; k < 6; k++)
                    {
                        float u = Mathf.Clamp01((t - k * .1f) / .6f); if (u <= 0 || u >= 1) continue;
                        Vector2 mid = at + V(0, .15f + k * .16f) * s + d * Mathf.Lerp(-1.5f, 1.8f, u) * s;
                        b.Beam(mid - d * .4f * s, mid + d * .4f * s, .05f * s, A(new Color(.75f, 1f, .5f), .8f * Mathf.Sin(u * Mathf.PI)), 0, seed + k, .5f);
                    }
                    for (k = 0; k < 12; k++)
                    {
                        float u = Mathf.Clamp01((t - H(seed, k) * .4f) / .9f); if (u <= 0 || u >= 1) continue;
                        Vector2 p = at + V((H(seed, k + 1) - .6f) * 1.4f, .1f + H(seed, k + 2) * .9f) * s + d * u * 1.6f * s;
                        b.Quad(FxShape.Shock, p, V(.06f, .06f) * s, 0, A(new Color(.65f, 1f, .25f), .9f * (1 - u)), .25f, seed + k);
                    }
                    break;
                }
                case Kind.Overgrowth: // bloom meets tide: soaked ground bursts into sprouts that shoot up, curl and open in flowers
                {
                    b.Floor(FxShape.Dust, at, 1.2f * s, A(new Color(.35f, .6f, .25f), .6f * end), 0, seed);
                    for (k = 0; k < 7; k++)
                    {
                        float grow = EaseOut3((t - k * .06f) / .55f); if (grow <= 0) continue;
                        Vector2 root = at + V(Mathf.Cos(k * 2.2f + seed) * .85f, Mathf.Sin(k * 2.2f + seed) * .5f) * s * (k == 0 ? 0 : 1);
                        float ht = (.45f + .35f * H(seed, k)) * s * grow * end, lean = (H(seed, k + 1) - .5f) * .5f;
                        Vector2 mid = root + V(lean * .4f, ht * .55f), top = root + V(lean, ht);
                        b.Link(root, mid, .035f * s, A(new Color(.3f, .7f, .2f), .95f), seed + k, .5f);
                        b.Link(mid, top, .03f * s, A(new Color(.35f, .78f, .22f), .95f), seed + k, .5f);
                        b.Quad(FxShape.Disc, mid + V(.07f, .02f) * s, V(.07f, .03f) * s * grow, 25, A(new Color(.45f, .88f, .3f), .95f * end), 0, seed + k);
                        float open = Rise(t, .5f + k * .05f, .8f + k * .05f);
                        if (open > 0)
                        {
                            Color petal = k % 2 == 0 ? new Color(1f, .55f, .8f) : new Color(1f, .85f, .4f);
                            for (int p = 0; p < 5; p++) { float an = p * 1.2566f; b.Quad(FxShape.Disc, top + V(Mathf.Cos(an), Mathf.Sin(an)) * .055f * s * open, V(.045f, .028f) * s * open, an * Mathf.Rad2Deg, A(petal, .95f * end), 0, seed + p); }
                            Glint(b, top, .1f * s * open, new Color(1f, 1f, .7f), end);
                        }
                    }
                    for (k = 0; k < 8; k++) { float u = Mathf.Repeat(t * 1.3f + k * .125f, 1); Glint(b, at + V((H(seed, k) - .5f) * 1.8f, u * .8f) * s, .07f * s, new Color(.6f, .85f, 1f), .7f * end * Mathf.Sin(u * Mathf.PI)); }
                    break;
                }
            }
        }
    }
}
