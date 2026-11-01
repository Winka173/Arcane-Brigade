using UnityEngine;

namespace ArcaneBrigade
{
    // The Glasshouse's look (requirement 25/10/2026: each map draws in its own file; MapEnvironmentVisual and MapAtmosphere
    // dispatch here). Props come from Resources/Environment/Glasshouse (Pot_Bud, Bloom_Red/Yellow/Brown/Orange/Blue, Bud_Big,
    // Flower_Turret, GlassPane, GlassPaneCracked, GlassShardA/B/C); until they are painted each is drawn in ink. Every effect is
    // code: the sun and rain (MapGlasshouseSky's shader, or sprites without it), the pollen aura, the flower beds, the seeds.

    public sealed partial class MapEnvironmentVisual
    {
        private static readonly string[] GlassBloomArt = { "Glasshouse/Bloom_Red", "Glasshouse/Bloom_Yellow", "Glasshouse/Bloom_Brown", "Glasshouse/Bloom_Orange", "Glasshouse/Bloom_Blue" };
        private static readonly InkPalette[] GlassBloomInk =
        {
            new InkPalette(new Color(.25f, .05f, .07f), new Color(.93f, .3f, .32f), new Color(1, .86f, .82f), new Color(1, .55f, .5f), new Color(1, .85f, .4f)),
            new InkPalette(new Color(.28f, .2f, .04f), new Color(1, .84f, .3f), new Color(1, .97f, .8f), new Color(1, .9f, .5f), new Color(.95f, .55f, .2f)),
            new InkPalette(new Color(.18f, .1f, .05f), new Color(.66f, .45f, .27f), new Color(.96f, .85f, .68f), new Color(.85f, .65f, .42f), new Color(.95f, .8f, .45f)),
            new InkPalette(new Color(.28f, .1f, .03f), new Color(1, .6f, .22f), new Color(1, .92f, .7f), new Color(1, .72f, .38f), new Color(.95f, .85f, .35f)),
            new InkPalette(new Color(.06f, .12f, .28f), new Color(.45f, .68f, 1), new Color(.9f, .96f, 1), new Color(.65f, .82f, 1), new Color(.98f, .92f, .6f)),
        };
        // friendly areas are teal (the flower bed, where to stand by a bud); danger is red; the pollen aura is gold
        private static readonly InkPalette GlassTeal = new InkPalette(new Color(.05f, .18f, .16f), new Color(.32f, .78f, .68f), new Color(.9f, 1, .96f), new Color(.55f, .95f, .85f), new Color(.98f, .9f, .6f));
        private static readonly InkPalette GlassIron = new InkPalette(new Color(.04f, .05f, .06f), new Color(.2f, .24f, .26f), new Color(.6f, .68f, .7f), new Color(.5f, .6f, .65f), new Color(.85f, .7f, .4f));
        private static readonly InkPalette GlassPaneFill = new InkPalette(new Color(.7f, .9f, .95f), new Color(.7f, .9f, .95f), new Color(1, 1, 1), new Color(.85f, 1, 1), new Color(.6f, .9f, .7f));
        private static readonly InkPalette GlassAdmiralWax = new InkPalette(new Color(.25f, .1f, .08f), new Color(.92f, .7f, .45f), new Color(1, .94f, .8f), new Color(1, .7f, .45f), new Color(.6f, .14f, .2f));
        private int glassTurnSeen = -1;

        private void DrawGlasshouse(MapEnvironmentState s, float t)
        {
            float sun = s.GlassSunBlend(t);
            GlassBeds(s, t);
            GlassPanes(s, t);
            GlassPots(s, t);
            GlassCarriers(s, t);
            GlassBuds(s, t);
            GlassSeeds(s, t);
            GlassWeatherOnFoes(s, t, sun);
        }
        /// <summary>The sun lifts standing props a little where its shafts fall (0 none, 1 fully lit).</summary>
        private float GlasshousePropLight(MapEnvironmentState s, Vector2 at) => .18f * s.GlassSunBlend(sim != null ? sim.Elapsed : 0);

        // ------------------------------------------------------------------ helpers shared by the Glasshouse and the Lamp Hall
        private static float GlassEaseOut(float x) { x = Mathf.Clamp01(x); return 1 - (1 - x) * (1 - x) * (1 - x); }
        private static float GlassEaseBack(float x) { x = Mathf.Clamp01(x); const float c = 1.6f; float y = x - 1; return 1 + (c + 1) * y * y * y + c * y * y; }
        private static float GlassHash(int i, int k) => Mathf.Repeat(Mathf.Sin(i * 12.9898f + k * 78.233f) * 43758.5453f, 1);
        /// <summary>A standing prop on the floor whose cast shadow meets its foot (requirement 25/10/2026: GroundShadow.PinCastToFoot).</summary>
        private SpriteRenderer GlassLampStand(string path, Vector2 at, float height, float alpha = 1, bool flip = false, float angle = 0, Color? tint = null)
        {
            if (used < shadows.Count) shadows[used].PinCastToFoot = true;
            SpriteRenderer r = Put(path, at, height, int.MinValue, alpha, flip, angle, tint);
            if (r != null) shadows[used - 1].PinCastToFoot = true;
            return r;
        }
        /// <summary>A hold or time bar in the air: a dark track and a bright fill with a bead riding its head.</summary>
        private void GlassLampHoldBar(Vector2 centre, float width, float fill, InkPalette pal, float alpha)
        {
            if (alpha <= .01f) return;
            Vector2 a = centre + Vector2.left * width * .5f, b = centre + Vector2.right * width * .5f;
            ink.Stroke(a, b, ArsenalInk.Midnight, .17f, 1, 0, .8f * alpha);
            fill = Mathf.Clamp01(fill);
            if (fill > .01f) ink.Stroke(a, b, pal, .11f, fill, 0, alpha);
            if (fill > .01f && fill < .99f) ink.TokenAt(InkToken.Glint, pal, Vector2.Lerp(a, b, fill), .22f, 0, alpha);
        }

        // ------------------------------------------------------------------ blooms
        private void GlassPots(MapEnvironmentState s, float t)
        {
            for (int i = 0; i < s.GlassPots.Count; i++)
            {
                MapEnvironmentState.GlassPot pot = s.GlassPots[i];
                float sway = still ? 0 : Mathf.Sin(t * 1.1f + i * 1.7f) * 2.5f;
                if (pot.Colour >= 0)
                {
                    InkPalette pal = GlassBloomInk[pot.Colour];
                    float open = still ? 1 : GlassEaseBack((t - pot.BloomedAt) / .45f);
                    float breathe = still || calm ? 1 : .8f + .2f * Mathf.Sin(t * 2.2f + i);
                    ink.Light(pot.Position + Vector2.up * .8f, 1.5f, pal.Glow, .26f * breathe * open);
                    if (GlassLampStand(GlassBloomArt[pot.Colour], pot.Position, 1.3f * Mathf.Lerp(.8f, 1, open), angle: sway) == null) GlassInkPot(pot.Position, 1, pot.Colour, t, i, open);
                    // sparkles of its colour winking round the flower: walk through it to pick it
                    for (int k = 0; k < 3; k++)
                    {
                        float a = (still ? 0 : t * 50) + k * 120 + i * 40, blink = still || calm ? .7f : Mathf.Pow(Mathf.Abs(Mathf.Sin(t * 2.6f + k * 1.9f + i)), 3);
                        Vector2 at = pot.Position + new Vector2(Mathf.Cos(a * Mathf.Deg2Rad) * .5f, .85f + Mathf.Sin(a * Mathf.Deg2Rad) * .3f);
                        ink.TokenAt(InkToken.Glint, pal, at, .2f, a, blink * open);
                    }
                    // now and then a petal lets go and drifts down
                    if (!still)
                    {
                        float cycle = Mathf.Repeat(t * .35f + i * .23f, 1);
                        Vector2 at = pot.Position + new Vector2(.25f + cycle * .5f + Mathf.Sin(cycle * 9) * .12f, 1.05f - cycle * 1.1f);
                        ink.TokenAt(InkToken.Petal, pal, at, .18f, cycle * 300, Mathf.Sin(cycle * Mathf.PI) * .9f * open);
                    }
                }
                else
                {
                    float height = Mathf.Lerp(.95f, 1.15f, pot.Growth);
                    if (GlassLampStand("Glasshouse/Pot_Bud", pot.Position, height, angle: sway) == null) GlassInkPot(pot.Position, pot.Growth, -1, t, i, 1);
                    // close to blooming: a glint swelling at the bud's tip
                    if (pot.Growth > .75f)
                    {
                        float swell = Mathf.InverseLerp(.75f, 1, pot.Growth), pulse = still || calm ? 1 : .7f + .3f * Mathf.Sin(t * 6 + i);
                        ink.TokenAt(InkToken.Glint, ArsenalInk.Cream, pot.Position + Vector2.up * (height * .82f), .16f + .14f * swell, still ? 0 : t * 40, swell * pulse);
                    }
                }
                // just picked: its petals burst up and flutter down
                float since = t - pot.PickedAt;
                if (pot.PickedColour >= 0 && since >= 0 && since < 1)
                {
                    InkPalette pal = GlassBloomInk[pot.PickedColour];
                    for (int k = 0; k < 9; k++)
                    {
                        float a = k * 40 + i * 17, reach = .55f + .5f * GlassHash(i, k);
                        float p = still ? .5f : since;
                        Vector2 at = pot.Position + Vector2.up * .8f + new Vector2(Mathf.Cos(a * Mathf.Deg2Rad), Mathf.Sin(a * Mathf.Deg2Rad) * .6f + .6f) * reach * GlassEaseOut(p * 1.6f) + Vector2.down * p * p * .9f;
                        ink.TokenAt(InkToken.Petal, pal, at, .24f * (1 - .3f * p), a + p * 420, 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.55f, 1, p)));
                    }
                    if (since < .3f) ink.Light(pot.Position + Vector2.up * .8f, 2.2f, pal.Glow, (1 - since / .3f) * .5f);
                }
            }
        }
        /// <summary>A pot and its plant in ink while the art is not in: a terracotta pot, a stem, leaves, a bud or an open bloom.</summary>
        private void GlassInkPot(Vector2 at, float growth, int colour, float t, int seed, float open)
        {
            float sway = still ? 0 : Mathf.Sin(t * 1.3f + seed) * 6;
            ink.FloorStroke(at + new Vector2(-.17f, .1f), at + new Vector2(.17f, .1f), ArsenalInk.Terracotta, .3f, 1, 0, 1);
            ink.FloorStroke(at + new Vector2(-.24f, .28f), at + new Vector2(.24f, .28f), ArsenalInk.Leather, .09f, 1, 0, 1);
            Vector2 foot = at + Vector2.up * .3f;
            float h = Mathf.Lerp(.25f, .7f, growth);
            Vector2 top = foot + new Vector2(Mathf.Sin(sway * Mathf.Deg2Rad) * h, h);
            ink.FloorStroke(foot, top, ArsenalInk.Green, .06f, 1, 0, 1);
            ink.Token(InkToken.Leaf, ArsenalInk.Green, Vector2.Lerp(foot, top, .4f) + Vector2.left * .13f, .26f, 150 + sway, 1, false);
            ink.Token(InkToken.Leaf, ArsenalInk.Green, Vector2.Lerp(foot, top, .6f) + Vector2.right * .13f, .24f, 30 + sway, 1, false);
            if (colour < 0) { ink.Token(InkToken.Leaf, ArsenalInk.Green, top, .14f + .16f * growth, 90 + sway, 1, false); return; }
            InkPalette pal = GlassBloomInk[colour];
            for (int k = 0; k < 5; k++)
            {
                float a = k * 72 + sway * 2 + seed * 30;
                Vector2 o = new Vector2(Mathf.Cos(a * Mathf.Deg2Rad), Mathf.Sin(a * Mathf.Deg2Rad) * .8f) * .15f * open;
                ink.Token(InkToken.Petal, pal, top + o, .27f * open, a - 90, 1, false);
            }
            ink.Token(InkToken.Coin, ArsenalInk.Gold, top, .12f * open, 0, 1, false);
        }

        // ------------------------------------------------------------------ pollen carriers and their flower beds
        private void GlassCarriers(MapEnvironmentState s, float t)
        {
            const float reach = MapEnvironmentRules.GlassAuraRadius;
            foreach (MapEnvironmentState.GlassCarrier c in s.GlassCarriers)
            {
                UnitState u = sim.UnitForPresentation(c.Id); if (u == null || !u.Alive) continue;
                float grow = still ? 1 : GlassEaseOut((t - c.Since) / .6f), breathe = still || calm ? 1 : .85f + .15f * Mathf.Sin(t * 2.4f + c.Id);
                // the aura: its whole reach in one soft gold, no rim
                Color fill = new Color(1f, .8f, .32f, .15f * grow * breathe);
                ink.FloorZone(u.Position, reach * grow, reach * grow, fill, fill); Marks++;
                // pollen: gold grains wheeling slowly through it and rising
                for (int k = 0; k < (calm ? 5 : 9); k++)
                {
                    float a = (still ? 0 : t * (18 + 6 * GlassHash(c.Id, k))) + k * 40, r = reach * (.2f + .75f * GlassHash(k, c.Id));
                    float rise = still ? .3f : Mathf.Repeat(t * .4f + GlassHash(k, 3), 1);
                    Vector2 at = u.Position + new Vector2(Mathf.Cos(a * Mathf.Deg2Rad) * r, Mathf.Sin(a * Mathf.Deg2Rad) * r * .55f + rise * .8f);
                    ink.TokenAt(k % 3 == 0 ? InkToken.Spark : InkToken.Sand, ArsenalInk.Gold, at, .16f, a, grow * Mathf.Sin(rise * Mathf.PI));
                }
                // the carrier itself: a crown of pollen and a warm halo
                ink.Light(u.Position + Vector2.up * .5f, 1.7f, new Color(1f, .82f, .35f), .3f * grow);
                ink.Orbit(Head(u) + Vector2.up * .15f, .3f, t * .5f, InkToken.Spark, ArsenalInk.Gold, 3, .9f * grow);
            }
            // the foes it quickens: a small violet chevron over each head
            if (s.GlassCarriers.Count == 0) return;
            int shown = 0;
            foreach (UnitState u in sim.Units)
            {
                if (shown >= 16) break;
                if (!u.Alive || u.Faction != Faction.Enemy) continue;
                bool inside = false, carrier = false;
                foreach (MapEnvironmentState.GlassCarrier c in s.GlassCarriers)
                {
                    if (c.Id == u.Id) carrier = true;
                    if ((u.Position - c.Last).sqrMagnitude < reach * reach) inside = true;
                }
                if (!inside || carrier) continue;
                Vector2 head = Head(u) + Vector2.up * (.16f + (still ? 0 : Mathf.Repeat(t * .9f + u.Id * .13f, 1) * .12f));
                ink.Stroke(head + new Vector2(-.11f, -.06f), head + new Vector2(0, .05f), ArsenalInk.Violet, .06f, 1, 0, .9f);
                ink.Stroke(head + new Vector2(.11f, -.06f), head + new Vector2(0, .05f), ArsenalInk.Violet, .06f, 1, 0, .9f);
                shown++;
            }
        }
        private void GlassBeds(MapEnvironmentState s, float t)
        {
            const float radius = MapEnvironmentRules.GlassBedRadius, life = MapEnvironmentRules.GlassBedSeconds;
            foreach (MapEnvironmentState.GlassBed bed in s.GlassBeds)
            {
                float age = t - bed.Born; if (age < 0 || age >= life) continue;
                float grow = still ? 1 : GlassEaseOut(age / .45f), fade = Mathf.Clamp01((life - age) / 1.2f);
                // the bed: one soft teal over its whole 2 units, no rim
                Color fill = new Color(.38f, .8f, .68f, .27f * fade);
                ink.FloorZone(bed.Position, radius * grow, radius * grow, fill, fill); Marks++;
                // flowers spring up across it in the five bloom colours, nodding, and wilt as it fades
                int flowers = calm ? 7 : 11;
                for (int k = 0; k < flowers; k++)
                {
                    float a = GlassHash(bed.Seed, k) * 360, d = Mathf.Sqrt(GlassHash(k, bed.Seed + 5)) * radius * .82f;
                    Vector2 at = bed.Position + new Vector2(Mathf.Cos(a * Mathf.Deg2Rad), Mathf.Sin(a * Mathf.Deg2Rad)) * d * grow;
                    float sprout = still ? 1 : GlassEaseBack((age - k * .05f) / .4f) * fade;
                    if (sprout <= .02f) continue;
                    InkPalette pal = GlassBloomInk[(k + bed.Seed) % 5];
                    float nod = still ? 0 : Mathf.Sin(t * 1.8f + k) * 10;
                    ink.Token(InkToken.Leaf, ArsenalInk.Green, at + new Vector2(-.08f, -.06f), .2f * sprout, 160 + nod, fade, false);
                    for (int p = 0; p < 4; p++)
                    {
                        float pa = p * 90 + nod + k * 23;
                        ink.Token(InkToken.Petal, pal, at + new Vector2(Mathf.Cos(pa * Mathf.Deg2Rad), Mathf.Sin(pa * Mathf.Deg2Rad) * .75f) * .09f * sprout, .18f * sprout, pa - 90, fade, false);
                    }
                    ink.Token(InkToken.Coin, ArsenalInk.Gold, at, .08f * sprout, 0, fade, false);
                }
                // a few petals lifting off it on the warm air
                if (!still)
                    for (int k = 0; k < 4; k++)
                    {
                        float cycle = Mathf.Repeat(t * .3f + k * .25f + bed.Seed * .01f, 1);
                        Vector2 at = bed.Position + new Vector2((GlassHash(k, bed.Seed) - .5f) * radius * 1.4f + Mathf.Sin(cycle * 7 + k) * .2f, cycle * 1.4f);
                        ink.TokenAt(InkToken.Petal, GlassBloomInk[(k + 2) % 5], at, .16f, cycle * 360, Mathf.Sin(cycle * Mathf.PI) * .8f * fade);
                    }
            }
        }

        // ------------------------------------------------------------------ the cracking roof
        private void GlassPanes(MapEnvironmentState s, float t)
        {
            const float radius = MapEnvironmentRules.GlassPaneRadius;
            foreach (MapEnvironmentState.GlassPane pane in s.GlassPanes)
            {
                if (!pane.Broken)
                {
                    float p = Mathf.Clamp01((t - pane.WarnAt) / MapEnvironmentRules.GlassPaneWarn);
                    // where it will land: one solid red area, brightening as the glass gives
                    Ring(pane.Position, radius, radius, p, BossTelegraph.Danger, .95f);
                    // the pane overhead in its iron frame, cracking, trembling, then dropping in the last moment
                    float drop = Mathf.Clamp01((t - (pane.BreakAt - .28f)) / .28f);
                    float lift = Mathf.Lerp(2.8f, .2f, drop * drop);
                    float shake = still || p < .55f ? 0 : Mathf.Sin(t * 47 + pane.Seed) * .035f * (p - .55f) / .45f;
                    // the painted pane (requirement 1/11/2026: the ink pane looked poor), cracked once the warning is half run
                    Vector2 centre = pane.Position + new Vector2(shake, lift);
                    if (Put(p < .45f ? "Glasshouse/GlassPane" : "Glasshouse/GlassPaneCracked", centre + Vector2.down * GlassPaneHeight * .5f, GlassPaneHeight, Air - 8, 1, pane.Seed % 2 == 0) == null)
                        GlassPaneInk(centre, p, pane.Seed, 1);
                }
                else
                {
                    float since = t - pane.BreakAt; if (since < 0 || since >= 2) continue;
                    GlassShatter(pane.Position, since, pane.Seed);
                }
            }
        }
        private const float GlassPaneHeight = 2f;
        private static readonly string[] GlassShardArt = { "Glasshouse/GlassShardA", "Glasshouse/GlassShardB", "Glasshouse/GlassShardC" };
        /// <summary>A roof pane in ink, while its art is not in: brushed glass, an iron frame with its mullion, two glare strokes, and
        /// cracks spreading from one point.</summary>
        private void GlassPaneInk(Vector2 c, float crack, int seed, float alpha)
        {
            const float w = 1.2f, h = .78f, skew = .26f;
            Vector2 bl = c + new Vector2(-w, -h), br = c + new Vector2(w, -h), tr = c + new Vector2(w + skew, h), tl = c + new Vector2(-w + skew, h);
            // the glass: three broad soft strokes, sky-tinted
            for (int k = 0; k < 3; k++)
            {
                float f = (k + .5f) / 3;
                ink.Stroke(Vector2.Lerp(bl, tl, f), Vector2.Lerp(br, tr, f), GlassPaneFill, h * .72f, 1, 0, .26f * alpha);
            }
            // glare
            ink.Stroke(Vector2.Lerp(bl, br, .18f) + Vector2.up * .2f, Vector2.Lerp(tl, tr, .42f) + Vector2.down * .15f, ArsenalInk.Cotton, .05f, 1, 0, .7f * alpha);
            ink.Stroke(Vector2.Lerp(bl, br, .3f) + Vector2.up * .2f, Vector2.Lerp(tl, tr, .52f) + Vector2.down * .3f, ArsenalInk.Cotton, .03f, 1, 0, .5f * alpha);
            // the iron frame and its mullion
            ink.Stroke(bl, br, GlassIron, .09f, 1, 0, alpha); ink.Stroke(br, tr, GlassIron, .09f, 1, 0, alpha);
            ink.Stroke(tr, tl, GlassIron, .09f, 1, 0, alpha); ink.Stroke(tl, bl, GlassIron, .09f, 1, 0, alpha);
            ink.Stroke(Vector2.Lerp(bl, br, .5f), Vector2.Lerp(tl, tr, .5f), GlassIron, .06f, 1, 0, alpha);
            // cracks: jagged lines racing out from one point as the warning runs
            if (crack <= .05f) return;
            Vector2 hit = c + new Vector2((GlassHash(seed, 1) - .5f) * w, (GlassHash(seed, 2) - .5f) * h);
            for (int k = 0; k < 7; k++)
            {
                float a = k * 51 + seed % 40, reach = (.45f + .75f * GlassHash(seed, k + 3)) * Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.05f + k * .06f, .95f, crack));
                if (reach <= .02f) continue;
                Vector2 end = hit + new Vector2(Mathf.Cos(a * Mathf.Deg2Rad) * w, Mathf.Sin(a * Mathf.Deg2Rad) * h) * reach;
                ink.Stroke(hit, end, ArsenalInk.Cotton, .035f, 1, 0, .95f * alpha, .05f, seed + k);
            }
            ink.TokenAt(InkToken.Glint, ArsenalInk.Glass, hit, .3f * crack, 0, crack * alpha);
        }
        /// <summary>The pane breaking on the floor: a white flash, shards flying out and skidding, glints on the pieces left lying.</summary>
        private void GlassShatter(Vector2 c, float since, int seed)
        {
            if (since < .25f) ink.Light(c, 4.2f, new Color(.92f, 1, 1), (1 - since / .25f) * .8f);
            int shards = calm ? 10 : 16;
            float p = still ? .6f : Mathf.Clamp01(since / .55f), fade = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(1.2f, 2, since));
            for (int k = 0; k < shards; k++)
            {
                float a = k * (360f / shards) + GlassHash(seed, k) * 20, reach = MapEnvironmentRules.GlassPaneRadius * (.35f + .7f * GlassHash(k, seed));
                Vector2 dir = new Vector2(Mathf.Cos(a * Mathf.Deg2Rad), Mathf.Sin(a * Mathf.Deg2Rad) * .6f);
                Vector2 at = c + dir * reach * GlassEaseOut(p) + Vector2.up * Mathf.Sin(p * Mathf.PI) * .45f * GlassHash(seed, k + 9);
                bool flying = p < 1;
                float size = .26f * (.7f + .5f * GlassHash(k, 2)), spin = a + (still ? 0 : p * 540 * (k % 2 == 0 ? 1 : -1));
                // the painted shards: flying over the fight, then lying on the floor under the units
                if (Put(GlassShardArt[k % 3], at, size * (k % 3 == 0 ? 2.1f : 1.6f), flying ? Air - 7 : Floor + 6, fade, k % 2 == 0, spin) == null)
                    ink.Token(InkToken.Shard, ArsenalInk.Glass, at, size, spin, fade, flying);
                if (!flying && !calm && GlassHash(k, 7) > .55f)
                    ink.TokenAt(InkToken.Glint, ArsenalInk.Glass, at + Vector2.up * .05f, .18f, 0, fade * Mathf.Pow(Mathf.Abs(Mathf.Sin(since * 7 + k)), 3));
            }
        }

        // ------------------------------------------------------------------ the shooting flowers
        private void GlassBuds(MapEnvironmentState s, float t)
        {
            if (!sim.MapSideShown(MapSide.GlassBuds)) return; // the turret flowers grow only on their own waves (MapPacing)
            for (int i = 0; i < s.GlassBuds.Count; i++)
            {
                MapEnvironmentState.GlassBud bud = s.GlassBuds[i];
                float sway = still ? 0 : Mathf.Sin(t * .9f + i * 2) * 3;
                if (bud.Open(t))
                {
                    float age = t - bud.OpenAt, left = bud.OpenAt + MapEnvironmentRules.GlassTurretSeconds - t;
                    float pop = still ? 1 : GlassEaseBack(age / .4f), recoil = still ? 0 : Mathf.Clamp01(1 - (t - bud.LastShot) / .16f);
                    float lean = Mathf.Clamp(-bud.Aim.x * 10, -10, 10);
                    Vector2 mouth = bud.Position + Vector2.up * 1.2f;
                    if (GlassLampStand("Glasshouse/Flower_Turret", bud.Position, 1.95f * (.6f + .4f * pop) * (1 - .05f * recoil), flip: bud.Aim.x < 0, angle: lean) == null)
                        GlassInkTurret(bud.Position, pop, bud.Aim, recoil, t);
                    if (age < .5f) ink.Light(mouth, 3, new Color(.75f, 1, .7f), (1 - age / .5f) * .6f);
                    if (recoil > 0) ink.TokenAt(InkToken.Puff, ArsenalInk.Green, mouth + bud.Aim * (.35f + .2f * (1 - recoil)), .3f + .2f * (1 - recoil), 0, recoil * .85f);
                    GlassLampHoldBar(bud.Position + Vector2.down * .38f, 1.1f, left / MapEnvironmentRules.GlassTurretSeconds, GlassTeal, .85f * Mathf.Clamp01(left / .4f));
                    continue;
                }
                if (t < bud.ReadyAt)
                {
                    // spent: a closed bud regrowing, dimmer, its regrowth filling a faint bar
                    if (GlassLampStand("Glasshouse/Bud_Big", bud.Position, 1.45f, .92f, angle: sway, tint: new Color(.66f, .72f, .64f)) == null) GlassInkBud(bud.Position, .8f, sway, .7f);
                    float wait = bud.ReadyAt - t;
                    if (wait <= MapEnvironmentRules.GlassTurretRegrow) GlassLampHoldBar(bud.Position + Vector2.down * .38f, 1, 1 - wait / MapEnvironmentRules.GlassTurretRegrow, ArsenalInk.Dust, .5f);
                    continue;
                }
                // ready: swaying and glinting; stand by it to open it
                float breathe = still ? 1 : 1 + .03f * Mathf.Sin(t * 2.1f + i);
                if (GlassLampStand("Glasshouse/Bud_Big", bud.Position, 1.7f * breathe * (1 + .06f * bud.Hold), angle: sway + (still ? 0 : Mathf.Sin(t * 30) * 2 * bud.Hold)) == null) GlassInkBud(bud.Position, breathe, sway, 1);
                Ring(bud.Position, MapEnvironmentRules.GlassBudReach, MapEnvironmentRules.GlassBudReach, Mathf.Max(.25f, bud.Hold), GlassTeal, .5f + .4f * bud.Hold);
                float blink = still || calm ? .7f : Mathf.Pow(Mathf.Abs(Mathf.Sin(t * 1.7f + i)), 3);
                ink.TokenAt(InkToken.Glint, ArsenalInk.Cream, bud.Position + new Vector2(.12f, 1.55f), .26f, still ? 0 : t * 30, blink);
                if (bud.Hold > .01f) GlassLampHoldBar(bud.Position + Vector2.up * 2.05f, 1.2f, bud.Hold, GlassTeal, 1);
            }
        }
        /// <summary>A big closed bud in ink while its art is not in.</summary>
        private void GlassInkBud(Vector2 at, float scale, float sway, float alpha)
        {
            Vector2 top = at + new Vector2(Mathf.Sin(sway * Mathf.Deg2Rad) * .3f, 1.1f * scale);
            ink.FloorStroke(at, top, ArsenalInk.Green, .12f, 1, 0, alpha);
            ink.Token(InkToken.Leaf, ArsenalInk.Green, at + new Vector2(-.3f, .3f), .5f, 160 + sway, alpha, false);
            ink.Token(InkToken.Leaf, ArsenalInk.Green, at + new Vector2(.3f, .4f), .46f, 20 + sway, alpha, false);
            ink.Token(InkToken.Leaf, ArsenalInk.Green, top, .62f * scale, 90 + sway, alpha, false);
            ink.Token(InkToken.Petal, GlassBloomInk[3], top + Vector2.up * .1f, .3f * scale, sway, alpha * .9f, false);
        }
        /// <summary>The opened turret flower in ink while its art is not in: a ring of petals round a seed mouth turned to its aim.</summary>
        private void GlassInkTurret(Vector2 at, float pop, Vector2 aim, float recoil, float t)
        {
            Vector2 head = at + Vector2.up * 1.15f * pop - aim * .08f * recoil;
            ink.FloorStroke(at, head, ArsenalInk.Green, .14f, 1, 0, 1);
            ink.Token(InkToken.Leaf, ArsenalInk.Green, at + new Vector2(-.34f, .32f), .55f, 160, 1, false);
            ink.Token(InkToken.Leaf, ArsenalInk.Green, at + new Vector2(.34f, .42f), .5f, 20, 1, false);
            for (int k = 0; k < 8; k++)
            {
                float a = k * 45 + (still ? 0 : t * 12);
                Vector2 o = new Vector2(Mathf.Cos(a * Mathf.Deg2Rad), Mathf.Sin(a * Mathf.Deg2Rad) * .85f) * .3f * pop;
                ink.Token(InkToken.Petal, GlassBloomInk[k % 2 == 0 ? 3 : 1], head + o, .4f * pop, a - 90, 1, false);
            }
            ink.Token(InkToken.Coin, GlassIron, head + aim * .06f, .26f * pop, 0, 1, false);
        }
        private void GlassSeeds(MapEnvironmentState s, float t)
        {
            foreach (MapEnvironmentState.GlassSeed seed in s.GlassSeeds)
            {
                float span = Mathf.Max(.01f, seed.Arrive - seed.Born), p = Mathf.Clamp01((t - seed.Born) / span);
                Vector2 Arc(float q) => Vector2.Lerp(seed.From, seed.To + Vector2.up * .4f, q) + Vector2.up * Mathf.Sin(q * Mathf.PI) * .45f;
                Vector2 at = Arc(p), back = Arc(Mathf.Max(0, p - .3f));
                ink.Stroke(back, at, ArsenalInk.Green, .07f, 1, 0, .7f);
                Vector2 dir = at - back;
                ink.TokenAt(InkToken.Thorn, ArsenalInk.Green, at, .3f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg, 1);
                // the seed bursting on its target in a puff of leaves
                if (p > .8f)
                {
                    float q = (p - .8f) / .2f;
                    for (int k = 0; k < 4; k++)
                    {
                        float a = k * 90 + seed.TargetId * 31;
                        ink.TokenAt(InkToken.Leaf, ArsenalInk.Green, seed.To + Vector2.up * .4f + new Vector2(Mathf.Cos(a * Mathf.Deg2Rad), Mathf.Sin(a * Mathf.Deg2Rad)) * .35f * q, .2f, a, 1 - q * .5f);
                    }
                }
            }
        }

        // ------------------------------------------------------------------ the weather on the foes
        private void GlassWeatherOnFoes(MapEnvironmentState s, float t, float sun)
        {
            UnitState hero = sim.Champion; if (hero == null) return;
            int turn = Mathf.FloorToInt(Mathf.Max(0, t + s.GlassWeatherShift) / MapEnvironmentRules.GlassWeatherSeconds);
            bool turned = turn != glassTurnSeen; glassTurnSeen = turn;
            bool sunny = s.GlassSunny(t);
            int shown = 0;
            foreach (UnitState u in sim.Units)
            {
                if (!u.Alive || u.Faction != Faction.Enemy) continue;
                bool admiral = u.Definition.IsBoss && u.Definition.Id != null && u.Definition.Id.StartsWith("WaxAdmiral");
                if (admiral)
                {
                    GlassAdmiralLook(u, t, sun, sunny, turned);
                    continue;
                }
                if (shown >= 18 || (u.Position - hero.Position).sqrMagnitude > 80) continue;
                shown++;
                Vector2 head = Head(u);
                if (sun > .5f)
                {
                    // dazzled: a sun glint winking by the eyes
                    float blink = still || calm ? .6f : Mathf.Pow(Mathf.Abs(Mathf.Sin(t * 3.1f + u.Id * .7f)), 6);
                    ink.TokenAt(InkToken.Glint, ArsenalInk.Cream, head + new Vector2(.16f, -.04f), .24f, still ? 0 : t * 60, blink * (sun - .5f) * 2);
                }
                else if (!still)
                {
                    // rain: drops landing on the head and splashing off
                    float cycle = Mathf.Repeat(t * 1.4f + u.Id * .37f, 1);
                    ink.TokenAt(InkToken.Drop, ArsenalInk.Water, head + new Vector2(.05f, .5f - cycle * .45f), .15f, 0, (1 - sun * 2) * (1 - cycle));
                }
            }
        }
        /// <summary>The Wax Admiral in the weather: wax dripping and a warm glow in the sun (MELTING), a cold sheen in the rain (HARDENED);
        /// each turn says so at the boss.</summary>
        private void GlassAdmiralLook(UnitState u, float t, float sun, bool sunny, bool turned)
        {
            Vector2 head = (Vector2)SpeechBubbles.Head(u);
            if (turned)
            {
                if (sunny) ImpactWords.Pop("MELTING · +" + Mathf.RoundToInt((MapEnvironmentRules.GlassAdmiralSunTaken - 1) * 100) + "% DAMAGE TAKEN", head + Vector2.up * .4f, new Color(1f, .7f, .4f), 3, .85f);
                else ImpactWords.Pop("HARDENED · -" + Mathf.RoundToInt((1 - MapEnvironmentRules.GlassAdmiralRainTaken) * 100) + "% DAMAGE TAKEN", head + Vector2.up * .4f, new Color(.7f, .85f, 1f), 3, .85f);
            }
            float body = Mathf.Max(.8f, u.Definition.BodyHeight * .5f), width = Mathf.Max(.6f, u.Definition.Radius * 1.2f);
            if (sun > .05f)
            {
                ink.Light(u.Position + Vector2.up * body * .6f, body * 2.4f, new Color(1f, .7f, .4f), .28f * sun);
                for (int k = 0; k < (calm ? 3 : 6); k++)
                {
                    float cycle = still ? .4f : Mathf.Repeat(t * .7f + k * .17f, 1);
                    Vector2 at = u.Position + new Vector2((GlassHash(k, 4) - .5f) * width * 1.6f, body * (1.1f - cycle * 1.05f));
                    ink.TokenAt(InkToken.Drop, GlassAdmiralWax, at, .2f * (1 - .4f * cycle), 0, sun * Mathf.Sin(cycle * Mathf.PI));
                }
            }
            if (sun < .95f)
            {
                float rain = 1 - sun;
                ink.Light(u.Position + Vector2.up * body * .6f, body * 2.2f, new Color(.65f, .82f, 1f), .22f * rain);
                float sweep = still ? .5f : Mathf.Repeat(t * .45f, 1);
                Vector2 a = u.Position + new Vector2(-width + sweep * width * 2, body * .2f), b = a + new Vector2(.35f, body * 1.1f);
                ink.Stroke(a, b, ArsenalInk.Silver, .05f, 1, 0, .6f * rain * Mathf.Sin(sweep * Mathf.PI));
            }
        }
    }

    public sealed partial class MapAtmosphere
    {
        private MapGlasshouseSky glassSky;
        /// <summary>The Glasshouse's air: a green-gold grade in the sun, cool and wet in the rain, easing across each turn. Sun shafts,
        /// the roof's frame shadows, caustics, rain streaks, puddles, ripples and glints come from MapGlasshouseSky's shader; without it
        /// (or with Reduced Effects) soft sprite shafts and streaks stand in. Pollen turns in the sun.</summary>
        private void AirGlasshouse(MapEnvironmentState s, int motes, float clock)
        {
            float sun = s.GlassSunBlend(clock), rain = 1 - sun;
            Grade(Color.Lerp(new Color(.6f, .76f, .86f, .13f), new Color(1f, .95f, .76f, .06f), sun), Color.Lerp(new Color(.02f, .07f, .11f, .44f), new Color(.08f, .14f, .05f, .3f), sun));
            if (!sparse && MapGlasshouseSky.Supported)
            {
                if (glassSky == null) { glassSky = GetComponent<MapGlasshouseSky>(); if (glassSky == null) glassSky = gameObject.AddComponent<MapGlasshouseSky>(); }
                glassSky.Show(centre, new Vector2(halfW, halfH) * 2.2f, sun, rain, t, calm, HazeOrder + 1, AirOrder - 6);
            }
            else
            {
                // sun shafts falling from the upper left, the same key light as every shadow
                for (int i = 0; i < 4; i++)
                {
                    float breathe = still || calm ? 1 : .8f + .2f * Mathf.Sin(t * .35f + i * 1.9f);
                    Vector2 at = centre + new Vector2((-.7f + i * .48f) * halfW, .05f * halfH);
                    Put(MechanicInk.Glow, at, new Vector2(1.3f + .5f * (i % 2), halfH * 3.2f), 34, new Color(1f, .92f, .66f, .13f * sun * breathe), HazeOrder);
                }
                // rain: slanted streaks falling across the view, and glints winking on the wet floor
                if (rain > .02f)
                    for (int i = 0; i < motes * 2; i++)
                    {
                        Vector2 at = Wrap(new Vector2(H(i, 11) * 40, H(i, 12) * 40), new Vector2(1.6f, -10.5f - 3 * H(i, 13)));
                        Put(flat, at, new Vector2(.025f, .5f + .3f * H(i, 14)), 9, new Color(.85f, .93f, 1f, .32f * rain), AirOrder);
                        if (i < motes)
                        {
                            Vector2 glint = new Vector2(centre.x + (H(i, 15) - .5f) * halfW * 2, centre.y + (H(i, 16) - .5f) * halfH * 2);
                            float wink = still || calm ? .5f : Mathf.Pow(Mathf.Abs(Mathf.Sin(t * (1.2f + H(i, 17)) + i)), 8);
                            Put(MechanicInk.Glow, glint, Vector2.one * .14f, 0, new Color(.95f, 1, 1, .6f * wink * rain), HazeOrder + 1);
                        }
                    }
            }
            // pollen and warm dust turning in the sun; now and then a leaf drifting down from the vines
            for (int i = 0; i < motes; i++)
            {
                bool leaf = i % 5 == 0;
                Vector2 at = Wrap(new Vector2(H(i, 1) * 40, H(i, 2) * 40), leaf ? new Vector2(.2f, -.3f - .1f * H(i, 3)) : new Vector2(.08f * (H(i, 6) - .5f), .06f + .05f * H(i, 7)));
                at.x += still ? 0 : Mathf.Sin(t * (leaf ? 1.4f : .7f) + i) * (leaf ? .3f : .12f);
                if (leaf) Put(flat, at, new Vector2(.14f, .08f * (still ? 1 : .4f + .6f * Mathf.Abs(Mathf.Cos(t * 1.9f + i)))), still ? 25 : t * 70 + i * 40, new Color(.45f, .7f, .32f, .55f), AirOrder);
                else
                {
                    float twinkle = calm || still ? .7f : .55f + .45f * Mathf.Sin(t * 1.3f + i * 2.1f);
                    Put(MechanicInk.Glow, at, Vector2.one * (.07f + .06f * H(i, 8)), 0, new Color(1, .9f, .6f, .45f * twinkle * sun), AirOrder);
                }
            }
        }
    }
}
