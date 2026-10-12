using System;
using System.Collections.Generic;
using UnityEngine;
using static ArcaneBrigade.FxKit;

namespace ArcaneBrigade
{
    /// <summary>Feature 4 (owner 29/10/2026): the last half second before a big bond moment (step 6 and 8) lands, the ground under it
    /// takes on its element so the player can see it coming: a soft tint, gathering motes, darkening. Never a ring or a hard-edged
    /// zone (danger zones are the bosses'). Presentation only: it reads the moment's own Delay and never changes the rules' timing.</summary>
    public static class Forewarning
    {
        public const float Lead = .5f;
        private static readonly Dictionary<string, float> last = new Dictionary<string, float>();

        /// <summary>Adds the forewarning of <paramref name="m"/> to <paramref name="fx"/> (call it after the moment's own spawn, so it lies under it).</summary>
        public static void Spawn(FxDirector fx, BondMomentRequest m, float now)
        {
            if (fx == null || m.Step < 6 || m.Rule == null || m.Delay < .2f) return;
            if (m.Kind != BondMomentKind.Strike && m.Kind != BondMomentKind.Line) return;
            // rapid step-8 strikes (rain, pages, shield bolts) would carpet the floor: one forewarning per rule each .5 s
            if (last.TryGetValue(m.Rule, out float at) && now >= at && now - at < .5f) return;
            last[m.Rule] = now;
            ElementLook look = RulesV3Ink.LookFor(m.Rule);
            fx.Spawn(new BondMomentRequest(m.Bond, Key(look), m.Step, m.Kind, m.From, m.To, m.Radius, m.Delay, 0), now);
        }

        public static string Key(ElementLook look) => "forewarn-" + look.ToString().ToLowerInvariant();
    }

    /// <summary>Registers one forewarning per element with the effect director.</summary>
    public static class ForewarnFx
    {
        public static void Register(Dictionary<string, Func<FxEffect>> map)
        {
            foreach (ElementLook look in Enum.GetValues(typeof(ElementLook)))
                if (look != ElementLook.None) { ElementLook l = look; map[Forewarning.Key(l)] = () => new ForewarnEffect(l); }
        }
    }

    public sealed class ForewarnEffect : FxEffect
    {
        private readonly ElementLook look;
        public ForewarnEffect(ElementLook look) { this.look = look; }
        protected override float Life => .3f;

        public override void Draw(FxBatch b, float now, float pre, float t)
        {
            float window = Mathf.Min(Forewarning.Lead, Land - Born); if (window < .05f) return;
            // grows over the last half second, then melts away under the landing
            float k = t < 0 ? Mathf.Clamp01(1 + t / window) : Fade(t, 0, Life);
            k = k * k * (3 - 2 * k); if (k <= .01f) return;
            float g = t < 0 ? 1 + t / window : 1; // 0..1 gathering progress, held after landing
            if (Moment.Kind == BondMomentKind.Line)
            {
                Vector2 d = Moment.To - Moment.From; float len = d.magnitude;
                int n = Mathf.Clamp(Mathf.RoundToInt(len / 1.8f), 3, 8); float r = Mathf.Clamp(Moment.Radius * 1.3f, .9f, 1.6f);
                for (int i = 0; i < n; i++) Spot(b, Moment.From + d * ((i + .5f) / n), r, k * (.75f + .25f * H(Seed, i + 90)), g, now, Seed * 7 + i * 31);
            }
            else Spot(b, At, Radius, k, g, now, Seed);
        }

        /// <summary>A point inside the soft footprint (an ellipse squashed to the floor).</summary>
        private static Vector2 In(Vector2 at, float r, int s, int i, float shrink = 1)
        {
            float a = H(s, i) * 6.2832f, d = Mathf.Sqrt(H(s, i + 40)) * r * shrink;
            return at + new Vector2(Mathf.Cos(a) * d, Mathf.Sin(a) * d * .62f);
        }

        private void Spot(FxBatch b, Vector2 at, float r, float k, float g, float now, int s)
        {
            Color c = RulesV3Ink.LookColour(look), hi = Color.Lerp(c, White, .45f);
            int n = 7 + Mathf.RoundToInt(r * 3);
            switch (look)
            {
                case ElementLook.Fire: // the floor warms from the middle, embers lift off it and little tongues lick up
                    b.Floor(FxShape.Glow, at, r * 1.15f, A(new Color(1, .35f, .08f), 0.59f * k));
                    b.Floor(FxShape.Glow, at, r * .55f, A(new Color(1, .7f, .25f), .3f * k * (.8f + .2f * Mathf.Sin(now * 19 + s))));
                    for (int i = 0; i < n; i++)
                    {
                        float u = Mathf.Repeat(now * (.9f + .6f * H(s, i)) + H(s, i + 3), 1);
                        Vector2 p = In(at, r * .9f, s, i) + new Vector2(Mathf.Sin(now * 6 + i) * .06f, u * .9f);
                        b.Quad(FxShape.Glow, p, .09f + .05f * (1 - u), A(Color.Lerp(new Color(1, .85f, .35f), c, u), k * (1 - u)));
                    }
                    for (int i = 0; i < 4; i++) b.Quad(FxShape.Flame, In(at, r * .6f, s, i + 60) + Vector2.up * .15f, new Vector2(.14f, .3f * g + .05f), 0, A(new Color(1, .5f, .12f), 0.70f * k), now, s + i, 1);
                    break;
                case ElementLook.Ice: // frost creeps over the floor while snow sifts down onto it
                    b.Floor(FxShape.Dust, at, r * 1.1f, A(new Color(.85f, .97f, 1f), 0.45f * k), now * .3f, s);
                    b.Floor(FxShape.Glow, at, r, A(new Color(.5f, .85f, 1f), 0.42f * k));
                    for (int i = 0; i < n; i++)
                    {
                        float u = Mathf.Repeat(now * (.55f + .3f * H(s, i)) + H(s, i + 5), 1);
                        Vector2 p = In(at, r, s, i) + new Vector2(Mathf.Sin(now * 2.5f + i * 1.7f) * .15f, 1.4f * (1 - u));
                        Glint(b, p, .1f + .06f * H(s, i + 7), hi, k * Mathf.Sin(u * Mathf.PI), now * 40 + i * 30);
                    }
                    break;
                case ElementLook.Water: // the ground goes dark and wet, droplets bead up and run in to the middle
                    b.Floor(FxShape.Shade, at, r * 1.2f, A(new Color(.02f, .06f, .18f), 0.63f * k));
                    b.Floor(FxShape.Glow, at, r * .8f, A(new Color(.2f, .5f, 1f), .22f * k * (.8f + .2f * Mathf.Sin(now * 5))));
                    for (int i = 0; i < n; i++)
                    {
                        float u = Mathf.Repeat(now * .8f + H(s, i), 1);
                        Vector2 p = Vector2.Lerp(In(at, r, s, i), at, EaseOut(u) * .7f);
                        b.Quad(FxShape.Glow, p, new Vector2(.09f, .12f), 0, A(new Color(.55f, .8f, 1f), .8f * k * Mathf.Sin(u * Mathf.PI)));
                    }
                    break;
                case ElementLook.Storm: // the light drops, a dark cloud boils up overhead and static prickles over the ground
                    b.Floor(FxShape.Shade, at, r * 1.4f, A(new Color(.02f, .02f, .08f), 0.95f * k)); b.Floor(FxShape.Glow, at, r * .9f, A(new Color(.3f, .5f, 1f), .2f * k * (H(Mathf.FloorToInt(now * 10), s) > .6f ? 1 : .3f)));
                    for (int i = 0; i < 4; i++) b.Quad(FxShape.Smoke, at + new Vector2((i - 1.5f) * r * .5f * (1.4f - .4f * g), 2f + .2f * (i % 2)), r * (.5f + .3f * g), A(new Color(.12f, .13f, .24f), 0.95f * k), now * .4f, s + i);
                    for (int i = 0; i < n; i++)
                    {
                        float on = H(Mathf.FloorToInt(now * 14) + s, i); if (on < .55f) continue;
                        Glint(b, In(at, r, s + Mathf.FloorToInt(now * 14), i), .22f, new Color(.6f, .85f, 1f), k * (on - .4f) * 1.6f, on * 90);
                    }
                    break;
                case ElementLook.Wind: // dust is lifted and the motes start to turn, drawn in towards the eye
                    b.Floor(FxShape.Dust, at, r * 1.15f, A(new Color(.75f, .85f, .72f), 0.49f * k), now * .8f, s);
                    for (int i = 0; i < n; i++)
                    {
                        float u = Mathf.Repeat(now * .7f + H(s, i), 1), a = H(s, i + 2) * 6.28f + now * 4.5f + u * 2.5f, d = r * (1 - .75f * u);
                        Vector2 p = at + new Vector2(Mathf.Cos(a) * d, Mathf.Sin(a) * d * .62f + u * .5f);
                        b.Quad(FxShape.Glow, p, new Vector2(.16f, .06f), a * Mathf.Rad2Deg + 90, A(new Color(.75f, 1f, .85f), .8f * k * Mathf.Sin(u * Mathf.PI)));
                    }
                    break;
                case ElementLook.Poison: // a sick green haze creeps in, bubbles rise out of the ground
                    b.Floor(FxShape.Smoke, at, r * 1.1f, A(new Color(.32f, .5f, .12f), 0.53f * k), now * .25f, s);
                    for (int i = 0; i < n; i++)
                    {
                        float u = Mathf.Repeat(now * (.6f + .4f * H(s, i)) + H(s, i + 9), 1);
                        Vector2 p = In(at, r * .9f, s, i) + Vector2.up * u * .6f;
                        b.Quad(FxShape.Glow, p, .05f + .1f * u, A(new Color(.6f, 1f, .25f), .7f * k * (u < .85f ? 1 : (1 - u) / .15f)));
                    }
                    break;
                case ElementLook.Bloom: // the ground greens over and pollen drifts up out of it
                    b.Floor(FxShape.Glow, at, r * 1.1f, A(new Color(.35f, .8f, .3f), 0.42f * k));
                    b.Floor(FxShape.Glow, at, r * .5f, A(new Color(1f, .55f, .75f), 0.28f * k));
                    for (int i = 0; i < n; i++)
                    {
                        float u = Mathf.Repeat(now * .45f + H(s, i), 1);
                        Vector2 p = In(at, r, s, i) + new Vector2(Mathf.Sin(now * 2 + i) * .2f, u * 1.1f);
                        b.Quad(FxShape.Glow, p, .07f, A(i % 3 == 0 ? new Color(1f, .7f, .85f) : new Color(.85f, 1f, .5f), k * Mathf.Sin(u * Mathf.PI)));
                    }
                    break;
                case ElementLook.Light: // a pale shaft opens from above and motes of light sift down it
                    b.Beam(at + Vector2.up * 4.2f, at, r * .55f * (.4f + .6f * g), A(new Color(1f, .95f, .75f), 0.31f * k), now, s, .2f);
                    b.Floor(FxShape.Glow, at, r * 1.05f, A(new Color(1f, .9f, .6f), 0.49f * k));
                    for (int i = 0; i < n; i++)
                    {
                        float u = Mathf.Repeat(now * (.7f + .3f * H(s, i)) + H(s, i + 4), 1);
                        Vector2 p = In(at, r * .5f, s, i) + Vector2.up * 3.5f * (1 - u);
                        Glint(b, p, .09f, new Color(1f, .95f, .7f), k * Mathf.Sin(u * Mathf.PI), i * 40);
                    }
                    break;
                case ElementLook.Void: // the floor darkens to a bruise and specks are pulled into the middle
                    b.Floor(FxShape.Shade, at, r * 1.3f, A(new Color(.06f, 0f, .1f), 0.77f * k));
                    for (int i = 0; i < n + 3; i++)
                    {
                        float u = Mathf.Repeat(now * .9f + H(s, i), 1);
                        Vector2 p = Vector2.Lerp(In(at, r * 1.1f, s, i), at, u * u);
                        b.Quad(FxShape.Glow, p, .06f + .04f * u, A(new Color(.78f, .5f, 1f), k * u));
                    }
                    break;
                case ElementLook.Earth: // grit jumps on the shaking floor, dust lifts at the edges
                    b.Floor(FxShape.Dust, at, r * 1.15f, A(new Color(.55f, .42f, .3f), 0.63f * k), now * .6f, s);
                    for (int i = 0; i < n; i++)
                    {
                        float u = Mathf.Repeat(now * 3.2f + H(s, i), 1); Vector2 p = In(at, r * .9f, s, i) + Vector2.up * .22f * 4 * u * (1 - u) * g;
                        b.Quad(FxShape.Disc, p, new Vector2(.05f, .04f), H(s, i) * 180, A(new Color(.75f, .58f, .4f), 0.95f * k));
                    }
                    break;
                case ElementLook.Gold: // a gold sheen spreads and glitter spirals down onto it
                    b.Floor(FxShape.Glow, at, r * 1.1f, A(new Color(1f, .75f, .25f), 0.45f * k));
                    for (int i = 0; i < n; i++)
                    {
                        float u = Mathf.Repeat(now * .8f + H(s, i), 1), a = H(s, i + 1) * 6.28f + u * 5;
                        Vector2 p = at + new Vector2(Mathf.Cos(a) * r * .6f * (1 - u * .5f), Mathf.Sin(a) * r * .3f + 1.6f * (1 - u));
                        Glint(b, p, .08f + .05f * H(s, i + 2), new Color(1f, .88f, .45f), k * Mathf.Sin(u * Mathf.PI), now * 200 + i * 20);
                    }
                    break;
                case ElementLook.Paper: // the shadow of something big grows on the floor and loose leaves flutter round it
                    b.Floor(FxShape.Shade, at, r * (.6f + .6f * g), A(new Color(.08f, .05f, .03f), 0.70f * k));
                    for (int i = 0; i < n; i++)
                    {
                        float u = Mathf.Repeat(now * .6f + H(s, i), 1);
                        Vector2 p = In(at, r, s, i) + new Vector2(Mathf.Sin(now * 4 + i) * .25f, 1.2f * (1 - u));
                        b.Quad(FxShape.Disc, p, new Vector2(.11f, .07f * Mathf.Abs(Mathf.Cos(now * 6 + i))), Mathf.Sin(now * 3 + i) * 40, A(new Color(1f, .93f, .78f), .8f * k * Mathf.Sin(u * Mathf.PI)));
                    }
                    break;
                case ElementLook.Time: // the floor glows on a ticking beat while sand trickles down
                {
                    float tick = Mathf.Pow(1 - Mathf.Repeat(now * 4, 1), 3);
                    b.Floor(FxShape.Glow, at, r * 1.05f, A(new Color(.95f, .85f, .55f), (.18f + .2f * tick) * k));
                    for (int i = 0; i < n; i++)
                    {
                        float u = Mathf.Repeat(now * 1.4f + H(s, i), 1);
                        b.Quad(FxShape.Glow, at + new Vector2((H(s, i) - .5f) * .25f, 2.2f * (1 - u)), .045f, A(new Color(1f, .88f, .55f), k * .9f));
                    }
                    break;
                }
                case ElementLook.Crystal: // glints prick on, one by one, over a cold pale sheen
                    b.Floor(FxShape.Glow, at, r, A(new Color(.65f, .9f, 1f), 0.35f * k));
                    for (int i = 0; i < n; i++)
                    {
                        float on = Mathf.Clamp01(g * 1.4f - H(s, i + 6) * .6f), tw = .6f + .4f * Mathf.Sin(now * 11 + i * 2.1f);
                        Glint(b, In(at, r * .9f, s, i) + Vector2.up * .1f, .08f + .12f * on, new Color(.85f, .97f, 1f), k * on * tw, i * 22);
                    }
                    break;
                case ElementLook.Dream: // lavender mist pools and small stars wink in it
                    b.Floor(FxShape.Smoke, at, r * 1.05f, A(new Color(.6f, .5f, .85f), 0.42f * k), now * .2f, s);
                    for (int i = 0; i < n; i++)
                    {
                        float tw = Mathf.Max(0, Mathf.Sin(now * (3 + 2 * H(s, i)) + i * 1.3f));
                        Glint(b, In(at, r, s, i) + Vector2.up * (.2f + .4f * H(s, i + 8)), .1f, new Color(1f, .8f, 1f), k * tw, 45);
                    }
                    break;
                case ElementLook.Sound: // the ground throbs on the beat and motes hop in time
                {
                    float beat = Mathf.Pow(1 - Mathf.Repeat(now * 3, 1), 2);
                    b.Floor(FxShape.Glow, at, r * (1 + .08f * beat), A(new Color(1f, .5f, .9f), (.15f + .25f * beat) * k));
                    for (int i = 0; i < n; i++)
                    {
                        Vector2 p = In(at, r * .9f, s, i) + Vector2.up * .35f * beat * (.5f + .5f * H(s, i + 3));
                        b.Quad(FxShape.Glow, p, .08f, A(new Color(1f, .75f, 1f), k * (.4f + .6f * beat)));
                    }
                    break;
                }
                default: // a plain soft warmth in the element's colour
                    b.Floor(FxShape.Glow, at, r, A(hi, 0.42f * k));
                    break;
            }
        }
    }
}
