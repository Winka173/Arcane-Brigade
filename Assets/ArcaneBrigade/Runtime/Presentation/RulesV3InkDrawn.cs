using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Bond moments drawn stroke by stroke in code, the way Storm Fencer's lightning is: each rule its own shapes and
    /// motion (never another rule's animation recoloured or resized), sized to the area it really hits.</summary>
    public sealed partial class RulesV3Ink
    {
        private struct Drawn { public string Rule; public Vector2 From, To; public float Radius, Born, Land, Life; public int Seed; }
        private readonly List<Drawn> drawn = new List<Drawn>();
        private readonly List<Vector2> drawPath = new List<Vector2>(96);

        private static readonly InkPalette BoltInk = ArsenalInk.Cyan, Flame = ArsenalInk.Amber, Wind = ArsenalInk.Cotton, Rock = ArsenalInk.Dust, Soot = ArsenalInk.Midnight;

        /// <summary>The rules drawn here, with how long each plays after it lands.</summary>
        private static float DrawnLife(string rule) => rule switch
        {
            "storm-thunderbolt" => 1f, "storm-thunderstorm" => .7f, "piercing-volley" => .45f,
            "burn-burst" => .75f, "inferno-burst" => 1f, "ember-meteor" => 1.3f, "phoenix-dive" => 1.1f,
            _ => 0,
        };
        private static bool IsDrawn(string rule) => rule != null && DrawnLife(rule) > 0;

        private void AddDrawn(string rule, Vector2 from, Vector2 to, float radius, float born, float land)
        {
            if (drawn.Count >= 72) drawn.RemoveAt(0);
            drawn.Add(new Drawn { Rule = rule, From = from, To = to, Radius = Mathf.Max(.6f, radius), Born = born, Land = land, Life = DrawnLife(rule), Seed = ++seed });
        }

        private static float H(int seed, int i) { float v = Mathf.Sin(seed * 12.9898f + i * 78.233f) * 43758.5453f; return v - Mathf.Floor(v); }
        private static Vector2 Dir(int seed, int i, int n) { float a = (i + H(seed, i) * .6f) / n * Mathf.PI * 2; return new Vector2(Mathf.Cos(a), Mathf.Sin(a) * .62f); }
        private static float EaseOut(float t) => 1 - (1 - t) * (1 - t);
        private List<Vector2> Copy(List<Vector2> path) { drawPath.Clear(); drawPath.AddRange(path); return drawPath; }

        private void DrawDrawn(float now)
        {
            for (int i = drawn.Count - 1; i >= 0; i--)
            {
                Drawn d = drawn[i];
                if (now > d.Land + d.Life) { drawn.RemoveAt(i); continue; }
                if (now < d.Born) continue;
                float pre = d.Land > d.Born ? Mathf.Clamp01((now - d.Born) / (d.Land - d.Born)) : 1, q = Mathf.Clamp01((now - d.Land) / d.Life);
                bool landed = now >= d.Land;
                switch (d.Rule)
                {
                    case "storm-thunderbolt": Thunderbolt(d, landed, pre, q); break;
                    case "storm-thunderstorm": StormCloudBolt(d, landed, pre, q); break;
                    case "piercing-volley": WindBlade(d, q); break;
                    case "burn-burst": if (landed) Fireball(d.To, d.Radius, q, d.Seed); break;
                    case "inferno-burst": if (landed) FirePillar(d.To, d.Radius, q, d.Seed); break;
                    case "ember-meteor": Meteor(d, landed, pre, q); break;
                    case "phoenix-dive": PhoenixDive(d, q); break;
                }
            }
        }

        // ------------------------------------------------------------------ Storm
        /// <summary>Storm 6: the sky charges over the crowd, then three thick forked bolts crash down together, the ground
        /// flashes white, forks of lightning crawl out across the whole area, sparks fly and a scorch remains.</summary>
        private void Thunderbolt(Drawn d, bool landed, float pre, float q)
        {
            Vector2 at = d.To; float r = d.Radius; Color c = NumberColour(ElementLook.Storm);
            if (!landed)
            {
                // charging: crackles gather on the spot and the light under the storm grows
                light.Sector(at, r * (.4f + .6f * pre), 0, 360, Faded(c, .35f * pre), Faded(c, 0));
                for (int k = 0; k < 4; k++)
                {
                    Vector2 a = at + Dir(d.Seed, k, 4) * r * (1.1f - pre * .8f);
                    ink.Ribbon(Copy(InkCanvas.Line(a, at, .12f, 0, true, d.Seed + k + Mathf.Floor(pre * 10))), BoltInk, .07f, 1, 0, .8f * pre);
                }
                return;
            }
            float strike = 1 - Mathf.Clamp01(q / .35f);
            if (q < .14f) ink.Wash(at, r * 2.4f, Color.white, .75f * (1 - q / .14f), true);
            for (int k = 0; k < 3; k++)
            {
                Vector2 top = at + new Vector2((H(d.Seed, k) - .5f) * r * 1.2f, 9.5f), foot = at + new Vector2((H(d.Seed, k + 9) - .5f) * r * .5f, 0);
                List<Vector2> path = Copy(InkCanvas.Line(top, foot, .55f, 0, true, d.Seed * 3 + k + Mathf.Floor(q * 14)));
                if (strike > 0) { light.GlowRibbon(path, 1, 0, 1.8f, Faded(c, .8f * strike)); ink.Ribbon(path, BoltInk, .42f * (k == 0 ? 1.3f : 1), 1, 0, strike); }
            }
            // forks crawl out over the whole area, then fade from the centre
            for (int k = 0; k < 8; k++)
            {
                Vector2 end = at + Dir(d.Seed, k, 8) * r * (.75f + .35f * H(d.Seed, k + 20));
                List<Vector2> fork = Copy(InkCanvas.Line(at, end, .22f, 0, true, d.Seed + k * 5 + Mathf.Floor(q * 12)));
                ink.Ribbon(fork, BoltInk, .13f, Mathf.Min(1, q * 5), Mathf.Clamp01((q - .45f) / .55f), 1);
            }
            ink.Wash(at, r * .9f, Soot.Ink, .45f * (1 - q), false);
            for (int k = 0; k < 12; k++)
            {
                float t = EaseOut(q); Vector2 dir = Dir(d.Seed, k + 40, 12);
                ink.Token(InkToken.Bolt, BoltInk, at + dir * r * 1.25f * t + Vector2.up * (2.2f * q * (1 - q)), .32f * (1 - q), k * 40 + q * 300, 1 - q);
            }
            light.Sector(at, r * 1.3f, 0, 360, Faded(c, .5f * (1 - q)), Faded(c, 0));
        }

        /// <summary>Storm 8: a scowling storm cloud puffs up over a crowd and spits one thin bolt with a small crackle.</summary>
        private void StormCloudBolt(Drawn d, bool landed, float pre, float q)
        {
            Vector2 at = d.To, cloud = at + Vector2.up * 5.5f; float grow = landed ? 1 - Mathf.Clamp01((q - .6f) / .4f) : pre;
            for (int k = 0; k < 6; k++)
            {
                Vector2 puff = cloud + new Vector2((k - 2.5f) * .45f, Mathf.Sin(k * 1.7f) * .18f);
                ink.Wash(puff, (.55f + .2f * H(d.Seed, k)) * grow, Soot.Body, .95f * grow, true);
                ink.Wash(puff + new Vector2(-.08f, .12f), (.32f + .1f * H(d.Seed, k)) * grow, Soot.Light, .35f * grow, true);
            }
            if (!landed) return;
            float strike = 1 - Mathf.Clamp01(q / .3f);
            if (strike > 0)
            {
                List<Vector2> path = Copy(InkCanvas.Line(cloud + Vector2.down * .3f, at, .3f, 0, true, d.Seed + Mathf.Floor(q * 16)));
                light.GlowRibbon(path, 1, 0, .8f, Faded(NumberColour(ElementLook.Storm), .7f * strike)); ink.Ribbon(path, BoltInk, .16f, 1, 0, strike);
            }
            for (int k = 0; k < 4; k++)
                ink.Ribbon(Copy(InkCanvas.Line(at, at + Dir(d.Seed, k, 4) * d.Radius * .7f, .1f, 0, true, d.Seed + k)), BoltInk, .07f, Mathf.Min(1, q * 6), Mathf.Clamp01((q - .3f) / .5f), 1);
        }

        // ------------------------------------------------------------------ Windborne
        /// <summary>Windborne 2: three crescent blades of wind slice down the lane, speed lines streaming behind them.</summary>
        private void WindBlade(Drawn d, float q)
        {
            Vector2 dir = (d.To - d.From).sqrMagnitude > .01f ? (d.To - d.From).normalized : Vector2.right;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            for (int k = 0; k < 3; k++)
            {
                float t = Mathf.Clamp01(q * 1.3f - k * .12f); if (t <= 0 || t >= 1) continue;
                Vector2 centre = Vector2.Lerp(d.From, d.To, EaseOut(t)) + Vector2.up * .4f;
                float span = .8f + .25f * k, a = 1 - t;
                ink.Ribbon(Copy(InkCanvas.Arc(centre, span, span, angle - 65, angle + 65)), Wind, .2f - k * .04f, 1, 0, a);
                for (int s = 0; s < 3; s++)
                {
                    Vector2 side = new Vector2(-dir.y, dir.x) * ((s - 1) * span * .55f);
                    ink.Stroke(centre - dir * (.4f + s * .2f) + side, centre - dir * (1.6f + s * .3f) + side, Wind, .05f, 1, 0, .7f * a);
                }
            }
        }

        /// <summary>Windborne 6 and 8: a real funnel of wind where the tornado stands, stacked rings of air widening upward and
        /// spinning, leaves and dust whirling round it, a dust cloud at its foot.</summary>
        private void Tornado(Vector2 at, float radius, float now, int seed)
        {
            ink.Wash(at, radius * 1.1f, Rock.Body, .4f, false);
            for (int i = 0; i < 9; i++)
            {
                float h = i * .42f, rx = radius * (.22f + i * .11f), spin = now * 520 + i * 47 + seed * 31;
                ink.FloorArc(at + Vector2.up * h, rx, rx * .32f, spin % 360, spin % 360 + 230, Wind, .1f + i * .015f, .9f, true);
            }
            for (int k = 0; k < 8; k++)
            {
                float h = (k * .45f + now * 1.4f) % 3.8f, ang = now * 7 + k * 2.1f, rad = radius * (.3f + h * .28f);
                Vector2 p = at + new Vector2(Mathf.Cos(ang) * rad, Mathf.Sin(ang) * rad * .32f + h);
                ink.Token(k % 3 == 0 ? InkToken.Puff : InkToken.Leaf, k % 3 == 0 ? Rock : ArsenalInk.Green, p, .32f, ang * 57, .9f);
            }
        }

        // ------------------------------------------------------------------ Ember
        /// <summary>Ember 2 and 4: a fireball blows up: a white-hot flash, tongues of flame licking out and up, burning embers
        /// thrown out and falling, then dark smoke rolling up.</summary>
        private void Fireball(Vector2 at, float r, float q, int s)
        {
            Color glow = NumberColour(ElementLook.Fire);
            light.Sector(at, r * (.6f + .8f * EaseOut(q)), 0, 360, Faded(Color.Lerp(glow, Color.white, .5f), .9f * (1 - q)), Faded(glow, 0));
            float fire = 1 - Mathf.Clamp01((q - .15f) / .6f);
            ink.Wash(at + Vector2.up * .3f, r * (.55f + .5f * EaseOut(q)), Flame.Body, .95f * fire, true);
            ink.Wash(at + Vector2.up * .4f, r * (.3f + .3f * EaseOut(q)), Flame.Light, fire, true);
            for (int k = 0; k < 7; k++)
            {
                Vector2 dir = Dir(s, k, 7), tip = at + dir * r * (.6f + .5f * EaseOut(q)) + Vector2.up * (.6f + .5f * q);
                ink.Ribbon(Copy(InkCanvas.Lob(at + Vector2.up * .2f, tip, .7f, 10)), Flame, .3f * (1 - q * .6f), Mathf.Min(1, q * 4), Mathf.Clamp01((q - .35f) / .6f), fire);
            }
            for (int k = 0; k < 14; k++)
            {
                Vector2 dir = Dir(s, k + 30, 14); float sp = r * (1 + H(s, k) * .8f);
                Vector2 p = at + dir * sp * q + Vector2.up * (3.2f * q - 4.2f * q * q + .3f);
                ink.Token(InkToken.Ember, Flame, p, .26f * (1 - q * .5f), k * 33 + q * 400, 1 - q * q);
            }
            if (q > .35f) for (int k = 0; k < 4; k++)
            {
                float t = (q - .35f) / .65f; Vector2 p = at + new Vector2((k - 1.5f) * r * .35f, .6f + t * 1.8f + k * .15f);
                ink.Wash(p, r * (.25f + .25f * t), Soot.Body, .55f * (1 - t), true);
            }
        }

        /// <summary>Ember's chain of bursts (the inferno): a pillar of flame roars straight up from the enemy, embers streaming up it.</summary>
        private void FirePillar(Vector2 at, float r, float q, int s)
        {
            float rise = EaseOut(Mathf.Min(1, q * 2.5f)), fade = 1 - Mathf.Clamp01((q - .5f) / .5f), w = Mathf.Max(.5f, r * .45f);
            ink.Wash(at, w * 1.6f, Flame.Body, .6f * fade, false);
            for (int k = 0; k < 5; k++)
            {
                Vector2 foot = at + new Vector2((k - 2) * w * .3f, 0), top = foot + Vector2.up * (3.2f + H(s, k)) * rise;
                ink.Ribbon(Copy(InkCanvas.Line(foot, top, .25f, 0, false, s + k + q * 6)), Flame, w * (.5f - Mathf.Abs(k - 2) * .1f), 1, Mathf.Clamp01((q - .55f) / .45f), fade);
            }
            light.Sector(at + Vector2.up * 1.4f, w * 2.2f, 0, 360, Faded(NumberColour(ElementLook.Fire), .6f * fade), Faded(NumberColour(ElementLook.Fire), 0));
            for (int k = 0; k < 10; k++)
            {
                float t = (q * 1.4f + H(s, k)) % 1; Vector2 p = at + new Vector2((H(s, k + 5) - .5f) * w * 1.6f, t * 4.2f);
                ink.Token(InkToken.Ember, Flame, p, .22f * (1 - t), k * 50 + q * 500, fade * (1 - t));
            }
        }

        /// <summary>Ember 6: a flaming rock streaks down from the sky, slams into the crowd and goes up in a mushroom of fire,
        /// rocks thrown out, a glowing crater of lava left behind.</summary>
        private void Meteor(Drawn d, bool landed, float pre, float q)
        {
            Vector2 at = d.To; float r = d.Radius;
            if (!landed)
            {
                Vector2 sky = at + new Vector2(-3.5f, 10), rock = Vector2.Lerp(sky, at, pre * pre);
                ink.Wash(at, r * .7f * pre, Soot.Ink, .35f * pre, false); // its shadow grows on the ground
                drawPath.Clear(); for (int k = 0; k <= 8; k++) drawPath.Add(Vector2.Lerp(sky, rock, .55f + .45f * k / 8f));
                light.GlowRibbon(drawPath, 1, 0, 1.4f, Faded(NumberColour(ElementLook.Fire), .7f));
                ink.Ribbon(drawPath, Flame, .55f, 1, 0, 1);
                ink.Wash(rock, .55f, Flame.Body, 1, true); ink.Token(InkToken.Ember, Rock, rock, .8f, pre * 720, 1);
                return;
            }
            float boom = 1 - Mathf.Clamp01((q - .2f) / .65f);
            ink.FloorZone(at, r * (1.1f + .2f * q), r * .55f, new Color(1, .45f, .1f, .55f * (1 - q * .5f)), new Color(.4f, .1f, .05f, 0)); // lava crater
            if (q < .1f) ink.Wash(at, r * 2.6f, Color.white, .7f * (1 - q / .1f), true);
            float stem = EaseOut(Mathf.Min(1, q * 2));
            ink.Ribbon(Copy(InkCanvas.Line(at, at + Vector2.up * 2.4f * stem, .15f, 0, false, d.Seed)), Flame, r * .4f, 1, 0, boom);
            for (int k = 0; k < 6; k++)
            {
                Vector2 cap = at + Vector2.up * (2.4f * stem) + new Vector2((k - 2.5f) * r * .28f * stem, Mathf.Sin(k) * .2f);
                ink.Wash(cap, r * (.35f + .2f * stem), k % 2 == 0 ? Flame.Body : Soot.Body, .95f * boom, true);
            }
            ink.Wash(at + Vector2.up * 2.4f * stem, r * .5f * stem, Flame.Light, boom, true);
            for (int k = 0; k < 10; k++)
            {
                Vector2 dir = Dir(d.Seed, k, 10); Vector2 p = at + dir * r * 1.6f * EaseOut(q) + Vector2.up * (4 * q * (1 - q) * 1.4f);
                ink.Token(InkToken.Sand, Rock, p, .36f, k * 70 + q * 600, 1 - q);
            }
            light.Sector(at, r * 1.8f, 0, 360, Faded(NumberColour(ElementLook.Fire), .7f * boom), Faded(NumberColour(ElementLook.Fire), 0));
        }

        /// <summary>Ember 8: the phoenix dives through the crowd, a broad streak of fire behind it, feathers of flame shedding
        /// and drifting down, and the ground along its path catching light.</summary>
        private void PhoenixDive(Drawn d, float q)
        {
            float head = Mathf.Clamp01(q / .4f), tail = Mathf.Clamp01((q - .3f) / .7f);
            drawPath.Clear(); for (int k = 0; k <= 16; k++) { float t = k / 16f; drawPath.Add(Vector2.Lerp(d.From, d.To, t) + Vector2.up * (.8f + Mathf.Sin(t * Mathf.PI) * .9f)); }
            light.GlowRibbon(drawPath, head, tail, 2.2f, Faded(NumberColour(ElementLook.Fire), .8f));
            ink.Ribbon(drawPath, Flame, .75f, head, tail, 1);
            Vector2 bird = InkCanvas.Sample(drawPath, head);
            if (head < 1) { ink.Wash(bird, .9f, Flame.Light, 1, true); ink.Token(InkToken.Petal, Flame, bird, .9f, -30, 1); }
            for (int k = 0; k < 12; k++)
            {
                float born = k / 12f * .4f; if (q < born) continue;
                float t = (q - born) / (1 - born); Vector2 p = InkCanvas.Sample(drawPath, k / 12f) + new Vector2(Mathf.Sin(k + t * 4) * .4f, -t * 1.6f);
                ink.Token(InkToken.Petal, Flame, p, .4f * (1 - t), k * 40 + t * 200, 1 - t);
            }
            for (int k = 0; k < 6; k++)
            {
                Vector2 foot = Vector2.Lerp(d.From, d.To, (k + .5f) / 6); float t = Mathf.Clamp01((q - k * .05f - .1f) / .7f);
                if (t > 0 && t < 1) ink.Ribbon(Copy(InkCanvas.Line(foot, foot + Vector2.up * 1.3f * EaseOut(t), .12f, 0, false, d.Seed + k)), Flame, .28f, 1, 0, 1 - t);
            }
        }
    }
}
