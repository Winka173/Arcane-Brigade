using System;
using System.Collections.Generic;
using UnityEngine;
using static ArcaneBrigade.FxKit;

namespace ArcaneBrigade
{
    /// <summary>Lantern Guild: warm honey lamplight, glass, dust motes and beams. The wick is a candle flame that lights one enemy; the
    /// lighthouse is a turning beam in a dusky arena full of drifting lanterns.</summary>
    public static class LanternFx
    {
        // owner 29/10/2026: small glints and sparks were drawn far too big: small ones shrink, big set-piece flashes keep their size
        private static void Glint(FxBatch b, Vector2 at, float size, Color tint, float alpha, float turn = 0)
            => FxKit.Glint(b, at, size <= .08f ? size : size < .5f ? .08f + (size - .08f) * .45f : size - .231f, tint, alpha, turn);
        private const FxShape Prism = (FxShape)47, Cone = (FxShape)47, Rod = (FxShape)48;
        private static readonly Color Honey = new Color(1f, .72f, .24f, 1), Lamp = new Color(1f, .86f, .5f, 1), Amber = new Color(1f, .55f, .12f, 1), Dusk = new Color(.06f, .04f, .12f, 1);

        /// <summary>Set by the renderer each frame while the lighthouse beam is on (hero position and the bond's beam angle in radians), so
        /// the drawn beam is the one that really burns. Without it the effect turns its own beam around where it started.</summary>
        public static bool BeamLive; public static Vector2 BeamCentre; public static float BeamAngle;

        public static void Register(Dictionary<string, Func<FxEffect>> map)
        {
            map["wick"] = () => new Wick();
            map["lighthouse"] = () => new Lighthouse();
            map["lantern-stillness"] = () => new StillLamp();
        }

        /// <summary>Step 4 wick. A candle wick sparks and a tall honey flame swells with a flicker over the enemy; a pool of lamplight
        /// spreads over the floor to the size of the lit area, glass facets glitter round the flame and motes of dust float up out of it.</summary>
        private sealed class Wick : FxEffect
        {
            protected override float Life => 1.45f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                if (t < 0) return;
                Vector2 at = At; float r = Mathf.Max(1.1f, Radius) * 1.2f, fade = Fade(t, .85f, 1.45f);
                float grow = EaseOut3(Rise(t, 0, .22f)) * (1 + .18f * Mathf.Sin(t * 35) * Fade(t, .2f, .6f));
                float flick = .85f + .15f * Mathf.Sin(now * 31) * Mathf.Sin(now * 17 + 1);
                // the lit pool on the floor
                b.Floor(FxShape.Glow, at, r * 1.6f * EaseOut(Rise(t, 0, .4f)), A(Honey, .75f * fade), 0, Seed);
                b.Floor(FxShape.Glow, at, r * .9f, A(Lamp, .55f * fade), 0, Seed);
                b.Quad(FxShape.Rays, at + Vector2.up * .8f, r * 1.35f, A(Lamp, .45f * fade), t * .5f, Seed, 3);
                // the wick sparks before it catches
                for (int k = 0; k < 7; k++)
                {
                    float u = t / (.22f + .12f * H(Seed, k + 40)); if (u >= 1) continue;
                    Vector2 d0 = Dir(Seed + 6, k, 7, .9f); Glint(b, at + Vector2.up * .55f + d0 * .9f * EaseOut(u) + Vector2.down * u * u * .4f, .16f, White, 1 - u, k * 50);
                }
                // hot wax drips down the stalk and pools at its foot
                for (int k = 0; k < 3; k++)
                {
                    float u = ((t - .2f) * 1.4f + k / 3f) % 1; if (t < .2f) break;
                    b.Quad(FxShape.Glow, at + new Vector2((k - 1) * .07f, .5f - u * .45f), .07f, A(Lamp, .9f * fade * (1 - u * .5f)), 0, Seed);
                }
                // the candle: a brass dish and a thick cream wax stick with wax runs, a charred wick on top
                float cup = EaseOut3(Rise(t, 0, .12f));
                b.Floor(FxShape.Shade, at, .9f, A(new Color(.08f, .05f, .02f), .5f * fade), 0, Seed);
                b.Quad(FxShape.Disc, at + Vector2.up * .05f, new Vector2(.75f, .24f) * cup, 0, A(new Color(.8f, .58f, .22f), fade), 0, Seed);
                b.Quad(Rod, at + Vector2.up * (.1f + .62f * cup), new Vector2(.3f, .62f * cup), 0, A(new Color(1f, .93f, .8f), fade), 0, Seed, 20);
                b.Link(at + Vector2.up * (.1f + 1.22f * cup), at + Vector2.up * (.1f + 1.42f * cup), .035f, A(new Color(.15f, .1f, .08f), fade), Seed, .3f);
                at += Vector2.up * 1.3f * cup;
                // the flame: layered tongues, the tall one wobbling
                for (int k = 0; k < 3; k++)
                {
                    float sway = Mathf.Sin(now * (9 + k * 3) + k) * .08f * grow;
                    Color c = k == 0 ? Amber : k == 1 ? Honey : Lamp;
                    float fs = r * .55f; b.Quad(FxShape.Flame, at + new Vector2(sway, .2f + 1.3f * grow * fs * (1 - k * .12f)), new Vector2(.6f - k * .14f, 1.5f * grow * (1 - k * .15f)) * flick * fs, 0, A(c, (k == 2 ? .9f : .75f) * fade), t, Seed + k, 1.5f);
                }
                b.Quad(FxShape.Glow, at + Vector2.up * 1.1f, 1.9f * grow * flick, A(Lamp, .6f * fade), 0, Seed);
                // as the flame gutters out a ribbon of grey wick smoke curls up from where it burned
                for (int k = 0; k < 7; k++)
                {
                    float u = Rise(t, .8f + k * .06f, 1.45f); if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + new Vector2(Mathf.Sin(u * 7 + k * .9f) * .25f * (.4f + u), .7f + u * 2.4f + k * .1f);
                    b.Quad(FxShape.Smoke, p, .16f + .3f * u, 0, A(new Color(.6f, .56f, .55f), .8f * Mathf.Min(1, u * 5) * (1 - u)), u + k, Seed + k);
                }
                // glass facets orbit the flame like the panes of a lantern
                for (int k = 0; k < 6; k++)
                {
                    float ang = k / 6f * Mathf.PI * 2 + t * 3.2f, rad = r * (.75f + .35f * Rise(t, 0, .5f));
                    Vector2 p = at + new Vector2(Mathf.Cos(ang) * rad, 1f + Mathf.Sin(ang) * rad * .35f + .12f * Mathf.Sin(t * 8 + k));
                    float depth = .6f + .4f * Mathf.Sin(ang);
                    b.Quad(Prism, p, new Vector2(.42f, .42f) * (.7f + .5f * depth), t * 90 + k * 40, A(Lamp, .95f * fade * Rise(t, 0, .3f)), 0, Seed + k);
                }
                for (int k = 0; k < 14; k++)
                {
                    float u = (t * .9f + H(Seed, k)) % 1;
                    Vector2 p = at + new Vector2((H(Seed, k + 5) - .5f) * r * 1.3f + Mathf.Sin(u * 6 + k) * .15f, .4f + u * 2.4f);
                    Glint(b, p, .12f + .08f * H(Seed, k + 1), Lamp, .9f * (1 - u) * fade * (.5f + .5f * Mathf.Sin(now * 8 + k)), k * 30);
                }
            }
        }

        /// <summary>Step 8 weather. The arena dims to dusk; hundreds of dust motes drift and twinkle; honey lanterns rise slowly like
        /// spirit lamps. A great turning beam with a spinning lens flare rakes the floor, leaving a fading wedge of afterglow behind it.</summary>
        private sealed class Lighthouse : FxEffect
        {
            protected override float Life => 1;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                if (Moment.Kind != BondMomentKind.Weather) return;
                float age = now - Born, on = Rise(age, 0, 1.8f) * Fade(now, End - 1.2f, End);
                Rect a = Arena; Vector2 c = a.center;
                                // drifting lanterns
                for (int k = 0; k < 7; k++)
                {
                    float u = (H(Seed, k) + now * (.025f + .01f * H(Seed, k + 9))) % 1;
                    Vector2 p = new Vector2(a.xMin + a.width * ((H(Seed, k + 20) + Mathf.Sin(now * .3f + k) * .02f) % 1), a.yMin - 1 + u * (a.height + 2));
                    float s = .55f + .25f * H(Seed, k + 40), sw = Mathf.Sin(now + k) * 8;
                    b.Quad(FxShape.Glow, p, s * 3f, A(Honey, .3f * on * (.7f + .3f * Mathf.Sin(now * 3 + k))), 0, Seed);
                    b.Quad(Prism, p, new Vector2(s * 1.25f, s * 1.25f), sw, A(Honey, on), Mathf.Repeat(sw / 360f, 1), Seed + k, 1);
                    // two moths fluttering round each lantern
                    for (int m = 0; m < 2; m++)
                    {
                        float ang = now * (3.2f + m) + k + m * 3; Vector2 q = p + new Vector2(Mathf.Cos(ang) * s * 2.2f, Mathf.Sin(ang * 1.3f) * s * 1.4f);
                        b.Quad(FxShape.Sparkle, q, new Vector2(.14f, .09f * (.4f + .6f * Mathf.Abs(Mathf.Sin(now * 30 + m)))), 0, A(Lamp, .8f * on), 0, 0, 1);
                    }
                }
                // dust motes
                for (int k = 0; k < 90; k++)
                {
                    float sp = .12f + .2f * H(Seed, k + 300);
                    Vector2 p = new Vector2(a.xMin + a.width * ((H(Seed, k) + now * sp * .06f) % 1), a.yMin + a.height * ((H(Seed, k + 150) + now * sp * .05f) % 1));
                    float tw = .5f + .5f * Mathf.Sin(now * (2 + 4 * H(Seed, k + 60)) + k);
                    Glint(b, p, .08f + .1f * H(Seed, k + 7), Lamp, .55f * on * tw, now * 20 + k);
                }
                // the beam
                Vector2 hub = BeamLive ? BeamCentre : At;
                float angle = BeamLive ? BeamAngle : age * BondStepTuning.LighthouseTurn;
                BeamLive = false;
                float reach = BondStepTuning.LighthouseReach, wide = BondStepTuning.LighthouseWidth;
                for (int g = 7; g >= 0; g--)
                {
                    float ga = angle - g * .09f, fadeG = g == 0 ? 1 : .32f * (1 - g / 8f);
                    Vector2 dir = new Vector2(Mathf.Cos(ga), Mathf.Sin(ga));
                    Vector2 end = hub + dir * reach;
                    // a widening cone: nested beams from the lamp, each longer and wider, so the light spreads with distance
                    // the cone of light is one soft shape that fades to zero on every edge of its quad (wide beams showed straight edges)
                    if (g == 0 || g % 2 == 1) b.Quad(Cone, hub + dir * 4.4f, new Vector2(4.4f, wide * 2.6f), ga * Mathf.Rad2Deg, A(Honey, (g == 0 ? .6f : .55f) * on * fadeG), age, Seed + g, 10);
                    if (g == 0) for (int s = 1; s <= 4; s++)
                    {
                        float f = s / 4f; Vector2 p1 = hub + dir * reach * (f * 1.08f);
                        b.Beam(hub, p1, wide * (.3f + .35f * f), A(Lamp, .4f * on), age, Seed + s + 9, .4f);
                    }
                    if (g == 0)
                    {
                        for (int k = 0; k < 16; k++)
                        {
                            float d = (H(Seed, k + 500) + now * .5f * (.4f + H(Seed, k))) % 1;
                            Vector2 p = hub + dir * reach * d + new Vector2(-dir.y, dir.x) * (H(Seed, k + 5) - .5f) * wide * 2.2f * (.4f + d);
                            Glint(b, p, .2f, Lamp, .9f * on * (.5f + .5f * Mathf.Sin(now * 10 + k)), k * 25);
                        }
                        b.Floor(FxShape.Glow, end, 2.2f, A(Lamp, .35f * on), 0, Seed);
                        // the light catches floating dust in the beam as a warm haze
                        for (int k = 0; k < 4; k++) b.Quad(FxShape.Dust, hub + dir * reach * (.3f + .2f * k), new Vector2(1.2f + .3f * k, .45f + .15f * k), ga * Mathf.Rad2Deg, A(new Color(1f, .85f, .55f), .28f * on), now * .2f + k, Seed + k);
                    }
                }
                b.Quad(FxShape.Glow, hub, 1.7f, A(Lamp, .6f * on), 0, Seed);
                b.Quad(FxShape.Sparkle, hub, new Vector2(1.6f, 1.6f), angle * Mathf.Rad2Deg, A(Lamp, .9f * on), 0, 0, 3);
                b.Quad(Prism, hub, new Vector2(.5f, .5f), -angle * 160, A(Lamp, .9f * on), 0, Seed);
            }
        }
    
        /// <summary>The hero stands still (or moves again): a small glass lantern unfolds on a hook above the hero, its pane glows
        /// honey as it is lit and swings, a few motes float off. No ring and nothing bursting out of the hero.</summary>
        private sealed class StillLamp : FxEffect
        {
            protected override float Life => 1.3f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                if (t < 0) return;
                float on = Rise(t, 0, .2f) * Fade(t, .9f, 1.3f), swing = Mathf.Sin(t * 7) * 14 * Fade(t, 0, 1.2f);
                Vector2 hook = At + Vector2.up * 3.1f, lamp = hook + (Vector2)(Quaternion.Euler(0, 0, swing) * Vector2.down) * .95f;
                b.Link(hook, lamp + Vector2.up * .7f, .04f, A(new Color(.5f, .4f, .25f), on), Seed, .3f);
                for (int k = 1; k < 5; k++) b.Quad(FxShape.Disc, Vector2.Lerp(hook, lamp + Vector2.up * .7f, k / 5f), .06f, A(new Color(.75f, .6f, .3f), on), 0, Seed);
                // the lamp's warm light pours down in a soft cone in front of the hero's head, motes turning in it
                b.Quad(Cone, lamp + Vector2.down * 1.4f, new Vector2(1.4f, 1.1f), -90, A(Honey, .35f * on * (.85f + .15f * Mathf.Sin(now * 9))), now, Seed, 10);
                // a match flares to light it
                float match = Fade(t, .05f, .35f) * Rise(t, 0, .06f);
                if (match > 0) { Glint(b, lamp + new Vector2(.55f, -.3f), .45f, White, match, t * 300); b.Quad(FxShape.Flame, lamp + new Vector2(.57f, -.2f), new Vector2(.07f, .16f), 0, A(Amber, match), t, Seed, 1); }
                // a thread of warm smoke drifts from the chimney cap
                for (int k = 0; k < 3; k++) { float u = (t * .8f + k / 3f) % 1; b.Quad(FxShape.Smoke, lamp + new Vector2(Mathf.Sin(u * 6 + k) * .1f, .85f + u * .9f), .08f + .15f * u, 0, A(new Color(.7f, .62f, .55f), .6f * on * (1 - u)), u + k, Seed + k); }

                b.Quad(FxShape.Glow, lamp, 1.6f, A(Honey, .45f * on * (.8f + .2f * Mathf.Sin(now * 23))), 0, Seed);
                b.Quad(Prism, lamp, new Vector2(1.2f, 1.2f), swing, A(Honey, on), Mathf.Repeat(swing / 360f, 1), Seed, 1);
                b.Quad(FxShape.Flame, lamp + Vector2.down * .02f, new Vector2(.09f, .2f), 0, A(Lamp, on), t, Seed, 1);
                for (int k = 0; k < 10; k++)
                {
                    float u = (t * .9f + H(Seed, k)) % 1;
                    Glint(b, lamp + new Vector2((H(Seed, k + 3) - .5f) * 1.4f, -u * 2.2f), .12f, Lamp, .8f * on * Mathf.Sin(u * Mathf.PI), k * 30);
                }
            }
        }
    }
}
