using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    // The Lamp Hall's look (requirement 25/10/2026: each map draws in its own file; MapEnvironmentVisual and MapAtmosphere dispatch
    // here). Props come from Resources/Environment/LampHall (Lamp_Lit, Lamp_Snuffed, LightGlow, Hand_Reach, Hand_Snuff,
    // Lantern_Carry, Oil_Vial); the lantern and the vial are drawn in ink until they are painted. Every effect is code.

    public sealed partial class MapEnvironmentVisual
    {
        private static readonly InkPalette LampFlame = new InkPalette(new Color(.25f, .08f, .03f), new Color(1f, .55f, .18f), new Color(1f, .93f, .62f), new Color(1f, .62f, .25f), new Color(1f, .85f, .4f));
        private static readonly InkPalette LampBrass = new InkPalette(new Color(.24f, .15f, .06f), new Color(.86f, .66f, .3f), new Color(1, .94f, .7f), new Color(1, .85f, .45f), new Color(.6f, .4f, .16f));
        private static readonly InkPalette LampSmoke = new InkPalette(new Color(.06f, .06f, .09f), new Color(.3f, .3f, .36f), new Color(.6f, .6f, .68f), new Color(.45f, .45f, .55f), new Color(.8f, .7f, .5f));
        private readonly Dictionary<int, float> lampDazzleSeen = new Dictionary<int, float>();

        private void DrawLampHall(MapEnvironmentState s, float t)
        {
            float rx = MapEnvironmentRules.LightRadiusX, ry = MapEnvironmentRules.LightRadiusY;
            for (int i = 0; i < s.Lamps.Count; i++)
            {
                MapEnvironmentState.Lamp lamp = s.Lamps[i];
                GlassLampStand(lamp.Lit ? "LampHall/Lamp_Lit" : "LampHall/Lamp_Snuffed", lamp.Base, 2.7f);
                Vector2 wick = lamp.Base + Vector2.up * 2.15f;
                if (lamp.Lit)
                {
                    // the light on the floor is the painted glow alone (no circle round a friendly area); relit, it swells back
                    float back = Mathf.Clamp01((t - lamp.SnuffedUntil) / .45f), flicker = still || calm ? 1 : .94f + .06f * Mathf.Sin(t * 7.3f + i * 2) * Mathf.Sin(t * 3.1f + i);
                    // the hand reaching for it: the light shivers before it goes
                    if (s.SnuffTarget == i && t >= s.ReachAt && t < s.SnuffAt && !still) flicker *= .75f + .25f * Mathf.Abs(Mathf.Sin(t * 23));
                    Put("LampHall/LightGlow", lamp.Light, 2 * ry * 1.06f * Mathf.Lerp(.4f, 1, GlassEaseOut(back)), Floor, .62f * flicker, tint: new Color(1, .93f, .78f));
                    ink.Light(wick, 1.1f, new Color(1f, .8f, .45f), .4f * flicker);
                    // the flare: relit in time, the lamp blazes white-gold and sparks fly up from its light
                    float since = t - lamp.FlareAt;
                    if (since >= 0 && since < 1)
                    {
                        float p = still ? .3f : since;
                        Put("LampHall/LightGlow", lamp.Light, 2 * ry * 1.06f * (1 + .25f * GlassEaseOut(p * 2)), Floor + 1, (1 - p) * .8f, tint: new Color(1, 1, .92f));
                        if (p < .3f) ink.Light(wick, 5, new Color(1, .95f, .8f), (1 - p / .3f) * .85f);
                        for (int k = 0; k < (calm ? 6 : 12); k++)
                        {
                            float a = GlassHash(i, k) * 360, d = Mathf.Sqrt(GlassHash(k, i + 3));
                            Vector2 at = lamp.Light + new Vector2(Mathf.Cos(a * Mathf.Deg2Rad) * rx, Mathf.Sin(a * Mathf.Deg2Rad) * ry) * d + Vector2.up * GlassEaseOut(p) * (1 + GlassHash(k, 9));
                            ink.TokenAt(k % 3 == 0 ? InkToken.Star : InkToken.Spark, ArsenalInk.Gold, at, .22f, a + p * 200, 1 - p);
                        }
                    }
                    continue;
                }
                // snuffed: a curl of smoke from the wick
                for (int k = 0; k < 3; k++)
                {
                    float cycle = still ? .3f + k * .2f : Mathf.Repeat(t * .5f + k * .33f, 1);
                    Vector2 at = wick + new Vector2(Mathf.Sin(cycle * 5 + k) * .18f, cycle * 1.2f);
                    ink.TokenAt(InkToken.Puff, LampSmoke, at, .3f + .35f * cycle, cycle * 90, .7f * (1 - cycle) * Mathf.Clamp01((t - lamp.SnuffedAt) / .3f));
                }
                // the relight window: an amber area at its foot and a bar running down over the lamp; walk in to make it flare
                float window = 1 - (t - lamp.SnuffedAt) / MapEnvironmentRules.FlareWindow;
                if (t >= lamp.SnuffedAt && window > 0)
                {
                    float pulse = still || calm ? 1 : .85f + .15f * Mathf.Sin(t * 9);
                    Ring(lamp.Base, MapEnvironmentRules.RelightReach, MapEnvironmentRules.RelightReach, window, ArsenalInk.Amber, (.45f + .45f * window) * pulse);
                    GlassLampHoldBar(lamp.Base + Vector2.up * 3.05f, 1.1f, window, ArsenalInk.Amber, 1);
                }
            }
            // the snuffing hand
            if (s.SnuffTarget >= 0 && s.SnuffTarget < s.Lamps.Count && t >= s.ReachAt && t < s.SnuffAt + .9f)
            {
                Vector2 head = s.Lamps[s.SnuffTarget].Base + Vector2.up * 2.45f;
                float p = Mathf.Clamp01((t - s.ReachAt) / MapEnvironmentRules.SnuffReach);
                if (p < 1) Put("LampHall/Hand_Reach", head + new Vector2(1.45f, .15f) + new Vector2(2.6f, 1.8f) * (1 - Mathf.SmoothStep(0, 1, p)), 1, Air + 40, Mathf.Clamp01(p * 2.5f));
                else Put("LampHall/Hand_Snuff", head + new Vector2(1.1f, -.15f), 1.5f, Air + 40, Mathf.Clamp01((s.SnuffAt + .9f - t) / .4f));
            }
            LampOils(s, t);
            LampLantern(s, t);
            LampFoes(s, t);
        }
        /// <summary>A lit lamp's oval (and the carried lantern's circle) lifts a standing prop back to full light (0 none, 1 fully lit).</summary>
        private float LampHallPropLight(MapEnvironmentState s, Vector2 at)
        {
            float lit = 0;
            foreach (MapEnvironmentState.Lamp lamp in s.Lamps)
            {
                if (!lamp.Lit) continue;
                Vector2 d = at - lamp.Light;
                float e = d.x * d.x / (MapEnvironmentRules.LightRadiusX * MapEnvironmentRules.LightRadiusX) + d.y * d.y / (MapEnvironmentRules.LightRadiusY * MapEnvironmentRules.LightRadiusY);
                lit = Mathf.Max(lit, 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.6f, 1.4f, e)));
            }
            if (sim != null && s.LanternHeld(sim.Elapsed))
            {
                float e = (at - s.LanternLightAt).sqrMagnitude / (MapEnvironmentRules.LanternRadius * MapEnvironmentRules.LanternRadius);
                lit = Mathf.Max(lit, 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.6f, 1.4f, e)));
            }
            return lit;
        }

        // ------------------------------------------------------------------ lamp oil
        private static Color LampOilColour(int kind) => MapBoostText.Colour(kind == 0 ? MapStat.AttackSpeed : kind == 1 ? MapStat.Gold : MapStat.MoveSpeed);
        private void LampOils(MapEnvironmentState s, float t)
        {
            for (int i = 0; i < s.LampOils.Count; i++)
            {
                MapEnvironmentState.LampOil oil = s.LampOils[i];
                float age = t - oil.Born, fall = Mathf.Clamp01(age / .5f), left = oil.Born + MapEnvironmentRules.LampOilLife - t;
                Vector2 at = fall < 1 ? Vector2.Lerp(oil.From, oil.Position, fall * fall) + Vector2.up * Mathf.Sin(fall * Mathf.PI) * .5f : oil.Position;
                float bob = still || fall < 1 ? 0 : Mathf.Sin(t * 2.4f + i * 1.7f) * .05f;
                // the last seconds it blinks before it soaks away
                float alpha = left > 2 || still ? 1 : .45f + .55f * Mathf.Abs(Mathf.Cos(t * 6));
                Color colour = LampOilColour(oil.Kind);
                ink.Light(at + Vector2.up * (.3f + bob), .9f, colour, .32f * alpha * (still || calm ? 1 : .8f + .2f * Mathf.Sin(t * 3 + i)));
                SpriteRenderer r = fall < 1 ? Put("LampHall/Oil_Vial", at, .62f, Air, alpha, angle: (1 - fall) * 200) : GlassLampStand("LampHall/Oil_Vial", at + Vector2.up * bob, .62f, alpha);
                if (r == null) LampInkVial(at + Vector2.up * bob, colour, alpha, fall < 1 ? (1 - fall) * 200 : 0);
                // what it gives, as a small token over it
                InkToken token = oil.Kind == 0 ? InkToken.Spark : oil.Kind == 1 ? InkToken.Coin : InkToken.Bolt;
                if (fall >= 1) ink.TokenAt(token, oil.Kind == 2 ? ArsenalInk.Water : ArsenalInk.Gold, at + Vector2.up * (.82f + bob * 2), .26f, 0, alpha);
            }
        }
        /// <summary>An oil vial in ink while its art is not in: a glass drop of coloured oil, a cork and a glint.</summary>
        private void LampInkVial(Vector2 at, Color colour, float alpha, float angle)
        {
            var pal = new InkPalette(new Color(colour.r * .25f, colour.g * .25f, colour.b * .25f), colour, Color.Lerp(colour, Color.white, .7f), colour, Color.white);
            ink.TokenAt(InkToken.Drop, pal, at + Vector2.up * .28f, .48f, angle, alpha);
            ink.Stroke(at + new Vector2(-.06f, .55f), at + new Vector2(.06f, .55f), ArsenalInk.Leather, .1f, 1, 0, alpha);
            ink.TokenAt(InkToken.Glint, ArsenalInk.Glass, at + new Vector2(-.08f, .34f), .16f, 0, alpha);
        }

        // ------------------------------------------------------------------ the carried lantern
        private void LampLantern(MapEnvironmentState s, float t)
        {
            UnitState hero = sim.Champion;
            if (s.LanternLying)
            {
                float age = t - s.LanternBorn, pop = still ? 1 : GlassEaseBack(age / .5f);
                float bob = still ? 0 : Mathf.Sin(t * 2) * .04f, flicker = still || calm ? 1 : .85f + .15f * Mathf.Sin(t * 8.3f) * Mathf.Sin(t * 3.7f);
                // where to stand: a brass-gold area brightening as it is held
                Ring(s.LanternAt, MapEnvironmentRules.LanternReach, MapEnvironmentRules.LanternReach, Mathf.Max(.25f, s.LanternHold), ArsenalInk.Gold, .45f + .45f * s.LanternHold);
                ink.Light(s.LanternAt + Vector2.up * .5f, 1.6f * pop, new Color(1f, .78f, .42f), .45f * flicker);
                if (GlassLampStand("LampHall/Lantern_Carry", s.LanternAt + Vector2.up * bob, 1.1f * pop, angle: still ? 0 : Mathf.Sin(t * 30) * 3 * s.LanternHold) == null)
                    LampInkLantern(s.LanternAt + Vector2.up * bob, 1.1f * pop, flicker, t, false);
                if (s.LanternHold > .01f) GlassLampHoldBar(s.LanternAt + Vector2.up * 1.5f, 1.1f, s.LanternHold, ArsenalInk.Gold, 1);
            }
            if (hero == null || !s.LanternHeld(t)) return;
            float left = s.LanternUntil - t, fade = Mathf.Clamp01(left / .5f);
            float glow = still || calm ? 1 : .92f + .08f * Mathf.Sin(t * 7.1f) * Mathf.Sin(t * 2.9f);
            // the hero is a moving light: the painted glow, fitted to its 2-unit circle
            SpriteRenderer light = Put("LampHall/LightGlow", hero.Position, 2 * MapEnvironmentRules.LightRadiusY * 1.06f, Floor, .62f * glow * fade, tint: new Color(1, .9f, .7f));
            if (light != null)
            {
                Vector3 scale = light.transform.localScale;
                light.transform.localScale = new Vector3(scale.x * MapEnvironmentRules.LanternRadius / MapEnvironmentRules.LightRadiusX, scale.y * MapEnvironmentRules.LanternRadius / MapEnvironmentRules.LightRadiusY, 1);
            }
            float side = hero.Facing.x < 0 ? -1 : 1;
            Vector2 hand = hero.Position + new Vector2(.45f * side, .55f + (still ? 0 : Mathf.Sin(t * 5) * .04f));
            ink.Light(hand + Vector2.up * .25f, 1.4f, new Color(1f, .78f, .42f), .5f * glow * fade);
            if (Put("LampHall/Lantern_Carry", hand, .8f, Air - 20, fade, angle: still ? 0 : Mathf.Sin(t * 4) * 6) == null) LampInkLantern(hand, .8f, glow, t, true);
            // embers rising off the flame
            if (!still)
                for (int k = 0; k < (calm ? 2 : 4); k++)
                {
                    float cycle = Mathf.Repeat(t * .9f + k * .25f, 1);
                    ink.TokenAt(InkToken.Ember, LampFlame, hand + new Vector2(Mathf.Sin(cycle * 6 + k) * .15f, .45f + cycle * .7f), .14f, 0, (1 - cycle) * fade);
                }
            GlassLampHoldBar(hero.Position + Vector2.down * .5f, 1, left / MapEnvironmentRules.LanternSeconds, ArsenalInk.Amber, .85f * fade);
            // each burn: a lick of flame from the lantern to every foe it burns
            float lick = t - s.LanternBurnAt;
            if (lick >= 0 && lick < .35f)
                foreach (KeyValuePair<int, float> burnt in s.LanternBurnt)
                {
                    if (burnt.Value <= t) continue;
                    UnitState u = sim.UnitForPresentation(burnt.Key); if (u == null || !u.Alive) continue;
                    float p = lick / .35f;
                    ink.Stroke(hand, u.Position + Vector2.up * .45f, LampFlame, .1f * (1 - p * .5f), Mathf.Min(1, p * 3), Mathf.Max(0, p * 1.4f - .4f), 1 - p, .12f, u.Id);
                }
        }
        /// <summary>A lantern in ink while its art is not in: a brass frame, a handle and a flame behind the glass.</summary>
        private void LampInkLantern(Vector2 at, float height, float flicker, float t, bool held)
        {
            float w = height * .28f, h = height * .62f;
            Vector2 bl = at + new Vector2(-w, .1f * height), br = at + new Vector2(w, .1f * height), tl = bl + Vector2.up * h, tr = br + Vector2.up * h;
            ink.Light(Vector2.Lerp(bl, tr, .5f), height * 1.2f, new Color(1f, .8f, .45f), .45f * flicker);
            ink.TokenAt(InkToken.Ember, LampFlame, Vector2.Lerp(bl, tr, .5f), height * .42f * flicker, still ? 0 : Mathf.Sin(t * 9) * 8, 1);
            ink.Stroke(bl, br, LampBrass, .07f * height, 1, 0, 1); ink.Stroke(tl, tr, LampBrass, .07f * height, 1, 0, 1);
            ink.Stroke(bl, tl, LampBrass, .05f * height, 1, 0, 1); ink.Stroke(br, tr, LampBrass, .05f * height, 1, 0, 1);
            Vector2 top = Vector2.Lerp(tl, tr, .5f) + Vector2.up * height * .12f;
            ink.Stroke(tl, top, LampBrass, .06f * height, 1, 0, 1); ink.Stroke(tr, top, LampBrass, .06f * height, 1, 0, 1);
            ink.Stroke(top, top + Vector2.up * height * .16f, LampBrass, .04f * height, 1, 0, 1, held ? 0 : .02f);
        }

        // ------------------------------------------------------------------ the foes: prowling eyes in the dark, dazzled, burnt
        private void LampFoes(MapEnvironmentState s, float t)
        {
            UnitState hero = sim.Champion; if (hero == null) return;
            int eyes = 0, words = 0;
            foreach (UnitState u in sim.Units)
            {
                if (!u.Alive || u.Faction != Faction.Enemy) continue;
                Vector2 head = Head(u);
                bool near = (u.Position - hero.Position).sqrMagnitude < 90;
                // dazzled stepping into a light: stars while it stands frozen, a cream glint while it takes more damage
                if (s.LampDazzled.TryGetValue(u.Id, out float dazzled) && dazzled > t)
                {
                    float since = MapEnvironmentRules.DazzleSeconds - (dazzled - t);
                    if (since < MapEnvironmentRules.DazzleFreeze) ink.Orbit(head + Vector2.up * .1f, .28f, t, InkToken.Star, ArsenalInk.Cream, 3, 1);
                    float blink = still || calm ? .7f : .5f + .5f * Mathf.Abs(Mathf.Sin(t * 5 + u.Id));
                    ink.TokenAt(InkToken.Glint, ArsenalInk.Cream, head + new Vector2(-.18f, -.1f), .26f, still ? 0 : t * 80, blink * Mathf.Clamp01((dazzled - t) / .4f));
                    if (words < 4 && (!lampDazzleSeen.TryGetValue(u.Id, out float seen) || seen != dazzled))
                    {
                        lampDazzleSeen[u.Id] = dazzled; words++;
                        if (near) ImpactWords.Pop("DAZZLED", head + Vector2.up * .35f, new Color(1f, .95f, .8f), 1, .6f);
                    }
                }
                // burnt by the lantern: little flames on its body
                if (s.LanternBurnt.TryGetValue(u.Id, out float burnt) && burnt > t)
                    for (int k = 0; k < 2; k++)
                    {
                        float flick = still ? 1 : .8f + .2f * Mathf.Sin(t * 14 + k * 2 + u.Id);
                        ink.TokenAt(InkToken.Ember, LampFlame, u.Position + new Vector2((k - .5f) * .26f, .3f + .15f * k), .24f * flick, still ? 0 : Mathf.Sin(t * 10 + k) * 10, Mathf.Clamp01((burnt - t) / .3f));
                    }
                // stunned by a lamp's flare
                if (u.SleepUntil > t && !s.LampDazzled.ContainsKey(u.Id))
                    foreach (MapEnvironmentState.Lamp lamp in s.Lamps)
                        if (t - lamp.FlareAt < MapEnvironmentRules.FlareStun && (u.Position - lamp.Light).sqrMagnitude < MapEnvironmentRules.LightRadiusX * MapEnvironmentRules.LightRadiusX)
                        { ink.Orbit(head + Vector2.up * .1f, .28f, t, InkToken.Star, ArsenalInk.Gold, 3, 1); break; }
                // night prowlers: violet eyes in the dark and a quick violet wake behind them
                if (eyes >= 24 || !near || s.LampLitFoes.Contains(u.Id)) continue;
                eyes++;
                float open = still || calm ? 1 : Mathf.Repeat(t * .7f + u.Id * .31f, 1) < .06f ? .2f : 1; // a blink now and then
                for (int e = -1; e <= 1; e += 2)
                    ink.TokenAt(InkToken.Glint, ArsenalInk.Violet, head + new Vector2(e * .08f + u.Facing.x * .05f, -.06f), .15f * open, 0, .95f);
                if (!still && u.Velocity.sqrMagnitude > .25f)
                {
                    Vector2 back = -u.Velocity.normalized;
                    ink.Stroke(u.Position + Vector2.up * .3f + back * .2f, u.Position + Vector2.up * .3f + back * .65f, ArsenalInk.Violet, .05f, 1, 0, .55f);
                }
            }
            if (lampDazzleSeen.Count > 256) lampDazzleSeen.Clear();
        }
    }

    public sealed partial class MapAtmosphere
    {
        /// <summary>Deep night on the floor that the lamps' own light cuts through; black ash drifting through the dark (the drowsy
        /// dark), parting round every light; warm dust turning inside the lights.</summary>
        private void AirLampHall(MapEnvironmentState s, int motes, float clock)
        {
            // deep night on the floor; the lamps' painted light (floor order -40) cuts through it
            // the round floor around the lamps: its edges sink deeper into the dark (requirement 23/10/2026; edge was .5)
            Grade(new Color(.04f, .04f, .12f, .36f), new Color(.01f, .01f, .05f, .8f));
            bool carried = s.LanternHeld(clock);
            // black ash drifting down through the dark, never inside a light
            for (int i = 0; i < motes * 2; i++)
            {
                Vector2 at = Wrap(new Vector2(H(i, 21) * 40, H(i, 22) * 40), new Vector2(.1f + .08f * H(i, 23), -.16f - .1f * H(i, 24)));
                at.x += still ? 0 : Mathf.Sin(t * .8f + i * 1.3f) * .2f;
                if (LampHallLit(s, at, carried)) continue;
                float tumble = still ? 1 : .35f + .65f * Mathf.Abs(Mathf.Cos(t * 1.7f + i));
                Put(flat, at, new Vector2(.09f + .05f * H(i, 25), .06f * tumble), still ? 20 : t * 50 + i * 37, new Color(.15f, .13f, .16f, .7f), AirOrder);
                if (i % 6 == 0) Put(MechanicInk.Glow, at, Vector2.one * .12f, 0, new Color(1f, .45f, .2f, calm || still ? .18f : .1f + .15f * Mathf.Abs(Mathf.Sin(t * 2 + i))), AirOrder);
            }
            // warm dust turning slowly inside each lit lamp's light
            int dust = 0;
            for (int i = 0; i < s.Lamps.Count && dust < Mathf.Max(motes, 4); i++)
            {
                MapEnvironmentState.Lamp lamp = s.Lamps[i];
                if (!lamp.Lit) continue;
                for (int k = 0; k < 3 && dust < Mathf.Max(motes, 4); k++, dust++)
                {
                    float rise = still || sparse ? .5f : Mathf.Repeat(t * .18f + H(i, k) , 1);
                    Vector2 at = lamp.Light + new Vector2((H(k, i + 2) - .5f) * MapEnvironmentRules.LightRadiusX * 1.2f + (still ? 0 : Mathf.Sin(t * .6f + k) * .2f), (H(k, i + 5) - .5f) * MapEnvironmentRules.LightRadiusY + rise * 1.4f);
                    Put(MechanicInk.Glow, at, Vector2.one * .13f, 0, new Color(1, .93f, .74f, .55f * Mathf.Sin(rise * Mathf.PI)), AirOrder);
                }
            }
        }
        private static bool LampHallLit(MapEnvironmentState s, Vector2 at, bool carried)
        {
            foreach (MapEnvironmentState.Lamp lamp in s.Lamps)
            {
                if (!lamp.Lit) continue;
                Vector2 d = at - lamp.Light;
                if (d.x * d.x / (MapEnvironmentRules.LightRadiusX * MapEnvironmentRules.LightRadiusX) + d.y * d.y / (MapEnvironmentRules.LightRadiusY * MapEnvironmentRules.LightRadiusY) < 1.1f) return true;
            }
            return carried && (at - s.LanternLightAt).sqrMagnitude < MapEnvironmentRules.LanternRadius * MapEnvironmentRules.LanternRadius * 1.1f;
        }
    }
}
