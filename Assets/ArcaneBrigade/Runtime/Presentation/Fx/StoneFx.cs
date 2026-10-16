using System;
using System.Collections.Generic;
using UnityEngine;
using static ArcaneBrigade.FxKit;

namespace ArcaneBrigade
{
    /// <summary>Stone bond: rock chunks, dust clouds, cracks and a heavy ground shake. The shake is drawn in the pieces themselves (chunks
    /// hop, dust shudders, cracks jump) and never in the camera.</summary>
    public static class StoneFx
    {
        // owner 29/10/2026: small glints and sparks were drawn far too big: small ones shrink, big set-piece flashes keep their size
        private static void Glint(FxBatch b, Vector2 at, float size, Color tint, float alpha, float turn = 0)
            => FxKit.Glint(b, at, size <= .08f ? size : size < .5f ? .08f + (size - .08f) * .45f : size - .231f, tint, alpha, turn);
        private const FxShape Rock = (FxShape)42;
        private static readonly Color Granite = new Color(.68f, .6f, .52f, 1), Slate = new Color(.56f, .57f, .64f, 1), Dust = new Color(.8f, .7f, .54f, 1), Pale = new Color(1f, .9f, .68f, 1);

        public static void Register(Dictionary<string, Func<FxEffect>> map)
        {
            map["stone-shove"] = () => new Shove();
            map["stone-spike"] = () => new Spikes();
            map["stone-boulder"] = () => new Boulder();
            map["colossus-stomp"] = () => new Stomp();
        }

        private static readonly Color Under = new Color(.07f, .045f, .03f, 1);
        /// <summary>A solid-looking rock: a dark body on the alpha layer under a lit, faceted rock on the light layer, so it reads as stone
        /// and not as a glow.</summary>
        private static void Solid(FxBatch b, Vector2 p, Vector2 size, float rot, Color c, int seed, float par)
        {
            // the rock shape is solid itself now (shaded stone, ink edge, light at the top-left whatever its turn)
            b.Quad(Rock, p, size, rot, A(c, Mathf.Min(1, c.a)), Mathf.Repeat(rot / 360f, 1), seed, par);
        }
        private static void Chunk(FxBatch b, Vector2 p, float s, float rot, Color c, int seed) => Solid(b, p, new Vector2(s, s), rot, c, seed, 0);

        /// <summary>A jagged crack line on the floor that grows from a point; lit edge, drawn as short bright links.</summary>
        private static void Crack(FxBatch b, Vector2 from, Vector2 to, float width, float alpha, int seed)
        {
            Vector2 d = to - from; float len = d.magnitude; if (len < .05f || alpha <= .01f) return; Vector2 n = new Vector2(-d.y, d.x) / len; Vector2 prev = from;
            for (int s = 1; s <= 6; s++)
            {
                float t = s / 6f; Vector2 p = s == 6 ? to : from + d * t + n * (H(seed, s) - .5f) * len * .22f;
                b.Link(prev, p, width * 2.4f, A(new Color(1, .7f, .3f), alpha * .3f), seed + s, 1);
                b.Link(prev, p, width, A(Pale, alpha), seed + s, .7f);
                prev = p;
            }
        }

        /// <summary>Step 2 shove. A square slab of rock rams in from the side in one hit and shoves the stunned enemy: speed lines streak
        /// behind it, pebbles spray ahead, and dust is thrown up along the floor where it stops.</summary>
        private sealed class Shove : FxEffect
        {
            protected override float Life => .85f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                if (t < 0) return;
                Vector2 at = At; float r = Mathf.Max(1f, Radius);
                float ang = H(Seed, 1) * Mathf.PI * 2; Vector2 dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang) * .6f).normalized, perp = new Vector2(-dir.y, dir.x);
                float ram = EaseOut3(Rise(t, 0, .16f)), back = Rise(t, .35f, .8f), fade = Fade(t, .45f, .85f);
                Vector2 slab = at - dir * (3.2f * (1 - ram) + .9f) + dir * back * .5f;
                b.Floor(FxShape.Shade, at, r * 1.1f, A(new Color(.1f, .06f, .03f), .45f * fade), 0, Seed);
                // speed lines behind the slab
                for (int k = 0; k < 7; k++)
                {
                    float off = (k - 3) * .36f, len = 2.2f + 1.4f * H(Seed, k);
                    Vector2 s0 = slab - dir * .6f + perp * off;
                    b.Link(s0 - dir * len * Fade(t, 0, .4f), s0, .09f, A(Pale, .8f * Fade(t, .05f, .42f)), Seed + k, .8f);
                }
                // the slab: three blocks stacked side by side so it reads as a wall of stone
                for (int k = -1; k <= 1; k++) Chunk(b, slab + perp * k * .95f + Vector2.up * (.25f + (k == 0 ? .25f : 0)), k == 0 ? 1.25f : .95f, k * 17 + t * 30, A(k == 0 ? Granite : Slate, 1.1f * fade), Seed + k * 3);
                b.Floor(FxShape.Shade, slab, 1.6f, A(Under, .5f * fade), 0, Seed + 9);
                b.Quad(FxShape.Glow, at, r * 1.2f, A(Pale, .35f * Fade(t, .1f, .3f)), 0, Seed);
                // the shove: a wall of dust thrown ahead of the slab and the crowd skidding in it
                for (int k = 0; k < 7; k++)
                {
                    float u = Rise(t, .1f + k * .02f, .7f + k * .02f); if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + dir * (.3f + 2.2f * EaseOut(u)) + perp * (k - 3) * .45f * (1 + u);
                    b.Quad(FxShape.Smoke, p + Vector2.up * .3f * u, new Vector2(.6f, .45f) * (.7f + .9f * u), 0, A(Dust, .75f * (1 - u)), u, Seed + k + 40);
                }
                // the slab skids: grainy dust ploughed up along its track, a puff billows where it stops
                for (int k = 0; k < 6; k++)
                {
                    float s = k / 5f, u = Rise(t, .04f + s * .1f, .75f + s * .1f); if (u <= 0 || u >= 1) continue;
                    Vector2 p = at - dir * (3.6f - 3.4f * s) + perp * (k % 2 == 0 ? .7f : -.7f) * (1 + u);
                    b.Quad(FxShape.Dust, p, new Vector2(.75f, .42f) * (.6f + .8f * u), 0, A(Dust, .8f * (1 - u)), u, Seed + k + 20);
                }
                b.Floor(FxShape.Smoke, at + dir * .4f, r * 1.2f * (.4f + .6f * Rise(t, .1f, .7f)), A(Dust, .75f * Fade(t, .15f, .8f)), Rise(t, 0, 1), Seed + 4);
                b.Quad(FxShape.Smoke, at + dir * .2f + Vector2.up * (.4f + .9f * Rise(t, .1f, .85f)), r * (.45f + .4f * Rise(t, .1f, .85f)), 0, A(new Color(.7f, .6f, .46f), .6f * Fade(t, .3f, .85f)), t, Seed + 5);
                // chips knocked off the slab's face at the hit, and impact stars
                for (int k = 0; k < 5; k++) Glint(b, at - dir * .3f + perp * (k - 2) * .4f + Vector2.up * .5f, .3f, Pale, Fade(t, .12f, .3f) * Rise(t, .1f, .14f), k * 37);
                for (int k = 0; k < 16; k++)
                {
                    float u = (t - .12f) / (.45f + .25f * H(Seed, k + 8)); if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + dir * (.3f + 2.4f * EaseOut(u)) * (.6f + .6f * H(Seed, k)) + perp * (H(Seed, k + 3) - .5f) * 2.2f * u + Vector2.up * (1.2f * H(Seed, k + 9) * u - 2.2f * u * u);
                    Chunk(b, p, .2f + .14f * H(Seed, k + 5), u * 400 + k * 30, A(Granite, 1.1f * (1 - u)), Seed + k);
                }
            }
        }

        /// <summary>Step 4 spike. The floor splits; a tall stone stalagmite stabs up through the middle with a ring of shorter ones that
        /// pop up in a ripple, rubble flies off their tips and the cracks glow pale where the rock was torn.</summary>
        private sealed class Spikes : FxEffect
        {
            protected override float Life => 1.25f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                if (t < 0) return;
                Vector2 at = At; float r = Mathf.Max(1.2f, Radius) * 1.15f, fade = Fade(t, .75f, 1.25f);
                b.Floor(FxShape.Shade, at, r * 1.3f, A(new Color(.08f, .05f, .02f), .6f * Rise(t, 0, .1f) * fade), 0, Seed);
                for (int k = 0; k < 7; k++) Crack(b, at, at + Dir(Seed, k, 7, .55f) * r * (.9f + .5f * H(Seed, k + 5)) * EaseOut3(Rise(t, 0, .15f)), .035f, .9f * fade, Seed + k * 3);
                // ring spikes first (outer ripple), then the king spike
                for (int k = 0; k < 8; k++)
                {
                    float born = .03f + .06f * (k % 4), up = EaseOut3(Rise(t - born, 0, .12f)) ; if (up <= 0) continue;
                    Vector2 foot = at + Dir(Seed + 2, k, 8, .6f) * r * (.78f + .12f * H(Seed, k + 12));
                    float h = (.9f + .5f * H(Seed, k + 3)) * up, w = .38f + .08f * H(Seed, k);
                    Solid(b, foot + Vector2.up * h * .9f, new Vector2(w, h), (H(Seed, k + 20) - .5f) * 18, A(Granite, 1.15f * fade), Seed + k, 1);
                }
                float king = EaseOut3(Rise(t, .07f, .2f)) * (1 + .03f * Mathf.Sin(t * 40));
                b.Quad(FxShape.Glow, at + Vector2.up * .3f, r * 1.4f, A(Pale, .6f * Fade(t, .05f, .3f)), 0, Seed);
                Solid(b, at + Vector2.up * 1.9f * king, new Vector2(1.1f, 2.1f * king), 0, A(Granite, 1.2f * fade), Seed + 40, 1);
                // rubble thrown from the break
                for (int k = 0; k < 16; k++)
                {
                    float u = (t - .06f) / (.55f + .3f * H(Seed, k + 30)); if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + Dir(Seed + 9, k, 16, .6f) * r * 1.3f * EaseOut(u) + Vector2.up * (2.8f * H(Seed, k + 60) * u - 4.4f * u * u + .2f);
                    Chunk(b, p, .12f + .1f * H(Seed, k + 6), u * 500 + k * 40, A(Granite, 1.1f * (1 - u * u)), Seed + k);
                }
                // a ring of grainy dust rolls off the base, and grit trickles off the spike tips
                for (int k = 0; k < 9; k++)
                {
                    float u = Rise(t, .05f, 1.1f); if (u <= 0 || u >= 1) break;
                    Vector2 p = at + Dir(Seed + 4, k, 9, .55f) * r * (.6f + .7f * EaseOut(u));
                    b.Quad(FxShape.Dust, p, new Vector2(.7f, .4f) * (.7f + .7f * u), 0, A(Dust, .85f * (1 - u)), u, Seed + k + 50);
                }
                b.Quad(FxShape.Smoke, at + Vector2.up * (.6f + 1.2f * Rise(t, .1f, 1.1f)), r * (.5f + .3f * Rise(t, .1f, 1.1f)), 0, A(new Color(.62f, .54f, .42f), .55f * Fade(t, .2f, 1.1f)), t, Seed + 6);
                for (int k = 0; k < 10; k++)
                {
                    float u = ((t - .25f) * 2.2f + H(Seed, k + 80)) % 1; if (t < .25f || t > 1f) break;
                    Vector2 tip = k < 2 ? at + Vector2.up * (3.4f - u * 3.2f) : at + Dir(Seed + 2, k, 8, .6f) * r * .8f + Vector2.up * (1.8f - u * 1.6f);
                    b.Quad(FxShape.Glow, tip + Vector2.right * (H(Seed, k) - .5f) * .3f, .07f, A(Pale, .9f * fade), 0, Seed);
                }
            }
        }

        /// <summary>Step 6 boulder (line). A rumble shakes pebbles along the lane, then a house-sized boulder rolls its whole length, turning,
        /// bouncing, ploughing a dark furrow and throwing rocks to both sides, trailing a rolling wall of dust.</summary>
        private sealed class Boulder : FxEffect
        {
            protected override float Life => 1.55f;
            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 a = From, z = At, d = z - a; float len = d.magnitude; if (len < .1f) return; Vector2 dir = d / len, perp = new Vector2(-dir.y, dir.x);
                float w = Mathf.Max(.8f, Moment.Radius);
                if (t < 0)
                {
                    // the lane trembles: dust puffs and pebbles hop along it, the far end glows
                    for (int k = 0; k < 18; k++)
                    {
                        float s = (k + .5f) / 18f; Vector2 p = a + d * s + perp * (H(Seed, k) - .5f) * w * 1.6f;
                        float hop = Mathf.Abs(Mathf.Sin(now * 22 + k * 3)) * .22f;
                        Chunk(b, p + Vector2.up * hop, .1f + .06f * H(Seed, k + 2), now * 200 + k * 30, A(Granite, 1.1f * pre), Seed + k);
                        if (k % 3 == 0) b.Quad(FxShape.Dust, p, new Vector2(.5f, .3f), 0, A(Dust, .6f * pre), now * .3f, Seed + k);
                    }
                    b.Floor(FxShape.Glow, a, 1.6f, A(Pale, .4f * pre), 0, Seed);
                    return;
                }
                float run = Mathf.Clamp01(t / 1.0f), e = run * run * (3 - 2 * run) * .4f + run * .6f;
                Vector2 pos = a + d * e;
                float fade = Fade(t, 1.0f, 1.55f);
                // furrow: dark gouge behind the boulder
                int fur = 22;
                for (int k = 0; k < fur; k++)
                {
                    float s = k / (float)fur * e; Vector2 p = a + d * s;
                    b.Floor(FxShape.Shade, p, w * .9f, A(new Color(.07f, .04f, .02f), .55f * fade), 0, Seed + k);
                }
                // dust wall rolling behind
                for (int k = 0; k < 16; k++)
                {
                    float back = (k / 16f) * Mathf.Min(e, .35f), s = e - back; if (s < 0) continue;
                    Vector2 p = a + d * s + perp * Mathf.Sin(k * 2.1f + now * 6) * w * .9f + Vector2.up * back * 3;
                    b.Quad(FxShape.Smoke, p, new Vector2(w * 1.1f, w * .8f) * (.8f + back * 2), 0, A(Dust, .9f * (1 - k / 16f) * fade), run, Seed + k);
                }
                // the boulder: huge rock, spinning, bouncing; shadow beneath
                float bounce = Mathf.Abs(Mathf.Sin(t * 13)) * .35f * (run < 1 ? 1 : 0), rad = w * 1.45f;
                b.Floor(FxShape.Shade, pos, rad * 1.05f, A(new Color(.05f, .03f, .02f), .6f * fade), 0, Seed);
                // it rolls: the turn follows the distance covered, so the cracks and facets roll over while the light stays top-left
                float spin = -e * len / (rad * .78f) * Mathf.Rad2Deg * Mathf.Sign(dir.x + .001f);
                Vector2 bp = pos + Vector2.up * (rad * .62f + bounce);
                Solid(b, bp, new Vector2(rad, rad), spin, A(Granite, fade), Seed + 2, 4);
                // rocks flung to both sides along the whole lane
                for (int k = 0; k < 24; k++)
                {
                    float shed = H(Seed, k + 90) * .95f, u = (e - shed) / .3f * .7f; if (e < shed || u <= 0 || u >= 1) continue;
                    float side = k % 2 == 0 ? 1 : -1;
                    Vector2 p = a + d * shed + perp * side * (w * .5f + 1.8f * EaseOut(u)) + Vector2.up * (1.9f * u - 2.6f * u * u + .2f);
                    Chunk(b, p, .09f + .08f * H(Seed, k + 4), u * 600 + k * 20, A(Granite, 1.1f * (1 - u * u)), Seed + k);
                }
                // every bounce slaps a puff of grainy dust out from under the boulder
                for (int k = 0; k < 6; k++)
                {
                    float hitT = (k + 1) * Mathf.PI / 13f, u = (t - hitT) / .5f; if (u <= 0 || u >= 1 || hitT > 1f) continue;
                    float rr = hitT / 1.0f, ee = rr * rr * (3 - 2 * rr) * .4f + rr * .6f; Vector2 p = a + d * ee;
                    for (int s = -1; s <= 1; s += 2) b.Quad(FxShape.Dust, p + perp * s * w * (.7f + .8f * u), new Vector2(w * .7f, w * .4f) * (.6f + u), 0, A(Dust, .85f * (1 - u)), u, Seed + k * 2 + s);
                }
                if (run >= 1)
                {
                    b.Quad(FxShape.Glow, z, 1.5f, A(Pale, .5f * Fade(t, 1, 1.2f)), 0, Seed);
                    b.Quad(FxShape.Smoke, z + Vector2.up * (.8f + Rise(t, 1, 1.55f)), w * 1.2f, 0, A(Dust, .7f * Fade(t, 1.05f, 1.55f)), t, Seed + 3);
                }
            }
        }

        /// <summary>Step 8 colossus. A giant stone foot (heel block, five toe stones) with a swelling shadow stamps down on the crowd; star
        /// cracks blast across the floor, slabs of ground jump and fall back, a ring of dust rolls out and the footprint stays cracked.</summary>
        private sealed class Stomp : FxEffect
        {
            protected override float Life => 1.7f;
            private static void Foot(FxBatch b, Vector2 c, float s, float alpha, int seed, float lift)
            {
                // a chiselled stone foot seen from above, toes toward the viewer, its sole over the crowd
                Vector2 o = c + Vector2.up * (lift + s * .25f);
                Solid(b, o, new Vector2(1.15f * s, 1.25f * s), 0, A(Granite, alpha), seed + 1, 2);
            }

            public override void Draw(FxBatch b, float now, float pre, float t)
            {
                Vector2 at = At; float r = Radius * 1.1f;
                if (t < 0)
                {
                    float g = pre * pre;
                    b.Floor(FxShape.Shade, at, r * (.5f + 1.1f * g), A(new Color(.05f, .03f, .02f), .7f * g), 0, Seed);
                    Foot(b, at, r * .62f * (1 + (1 - g) * .5f), .95f, Seed, 6f * (1 - g) + .3f);
                    // grit and pebbles shaken off the falling foot
                    for (int k = 0; k < 10; k++)
                    {
                        float u = (pre * 2.5f + H(Seed, k + 30)) % 1;
                        Vector2 p = at + new Vector2((H(Seed, k + 31) - .5f) * r * 1.4f, 6f * (1 - g) + .3f - u * 2.2f);
                        Chunk(b, p, .07f + .05f * H(Seed, k + 32), now * 300 + k * 40, A(Granite, (1 - u) * g), Seed + k);
                    }
                    // the air pressed down under it lifts the floor dust
                    b.Quad(FxShape.Dust, at, new Vector2(r * 1.2f, r * .5f) * (.6f + .4f * g), 0, A(Dust, .5f * g * g), now * .4f, Seed + 70);
                    return;
                }
                float fade = Fade(t, 1.0f, 1.7f), hit = Fade(t, 0, .25f);
                float press = 1 - .06f * Fade(t, 0, .3f);
                b.Floor(FxShape.Shade, at, r * 1.35f, A(new Color(.05f, .03f, .02f), .55f * fade), 0, Seed);
                // star cracks and the hot rim of broken rock
                b.Floor(FxShape.Cracks, at, r * 1.45f, A(new Color(1f, .6f, .25f), .55f * fade), Rise(t, 0, .2f), Seed, 1);
                float grow = EaseOut3(Rise(t, 0, .18f));
                for (int k = 0; k < 10; k++) Crack(b, at, at + Dir(Seed, k, 10, .56f) * r * (1.1f + .7f * H(Seed, k + 5)) * grow, .045f, .95f * fade, Seed + k * 5);
                Foot(b, at, r * .62f * press, Mathf.Min(1, fade * 1.2f), Seed, Mathf.Max(0, .25f * Fade(t, 0, .2f)));

                b.Quad(FxShape.Glow, at, r * 1.2f, A(Pale, .25f * hit * hit), 0, Seed);
                // ground slabs jump up and fall back: the shake, drawn in the floor itself
                for (int k = 0; k < 10; k++)
                {
                    float u = (t - .05f) / (.7f + .2f * H(Seed, k + 3)); if (u <= 0 || u >= 1) continue;
                    Vector2 foot = at + Dir(Seed + 4, k, 10, .58f) * r * (1.1f + .5f * H(Seed, k + 8));
                    float hop = 4f * u * (1 - u) * (1.1f + H(Seed, k + 11));
                    Chunk(b, foot + Vector2.up * hop, .42f + .2f * H(Seed, k + 14), u * 120 * (k % 2 == 0 ? 1 : -1), A(Granite, 1.15f * (1 - u * .6f)), Seed + k);
                }
                // flying debris
                for (int k = 0; k < 22; k++)
                {
                    float u = (t - .03f) / (.6f + .35f * H(Seed, k + 30)); if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + Dir(Seed + 7, k, 22, .55f) * r * 1.6f * EaseOut(u) + Vector2.up * (3.4f * H(Seed, k + 60) * u - 5f * u * u + .2f);
                    Chunk(b, p, .08f + .08f * H(Seed, k + 6), u * 700 + k * 20, A(Granite, 1.1f * (1 - u * u)), Seed + k);
                }
                // dust ring rolling out low and grainy, billows riding on it, and a mushroom column
                for (int k = 0; k < 12; k++)
                {
                    float u = Rise(t, .03f, 1.2f); if (u <= 0 || u >= 1) continue;
                    Vector2 p = at + Dir(Seed + 1, k, 12, .55f) * r * (.7f + 1.1f * EaseOut(u));
                    b.Quad(FxShape.Dust, p, new Vector2(r * .55f, r * .32f) * (.7f + u), 0, A(Dust, .9f * (1 - u)), u, Seed + k);
                    if (k % 2 == 0) b.Quad(FxShape.Smoke, p + Vector2.up * (.3f + .5f * u), r * .32f * (.7f + u), 0, A(new Color(.66f, .56f, .42f), .6f * (1 - u)), u, Seed + k + 20);
                }
                SmokeColumn(b, at, r * .5f, 2.4f, t, new Color(.62f, .52f, .4f), Seed, 6);

            }
        }
    }
}
