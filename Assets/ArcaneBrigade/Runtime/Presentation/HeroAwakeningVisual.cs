using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Every hero's gauge and transformation, painted (18/10/2026). The gauge is the hero's own vessel standing just left
    /// of the hero: painted art (Gauge_&lt;Hero&gt;_Empty / _Full, the full one revealed from the bottom up along its measured
    /// window) when it exists, otherwise an inked vessel in the hero's colours with its own token riding the fill (the three
    /// pilot heroes keep their own inked vessels: a mail sack of letters, a boiler tube with a red line, a column of moons).
    /// It fills a third per tier, drains while transformed and rests dim while cooling. While transformed: the transformation's
    /// name pops over the hero, a held transformed weapon swaps in (UnitVisual), a worn or summoned object is placed where its
    /// transformation puts it, and dreams, jellies or performers stand where the simulation says.</summary>
    public sealed partial class HeroIdentityVisual
    {
        private static readonly InkPalette Leather = new InkPalette(new Color(.2f, .11f, .06f), new Color(.62f, .4f, .22f), new Color(.96f, .85f, .65f), new Color(.95f, .75f, .5f), new Color(.85f, .2f, .25f));
        private static readonly InkPalette Ember = new InkPalette(new Color(.25f, .08f, .05f), new Color(.98f, .55f, .22f), new Color(1, .9f, .62f), new Color(1, .65f, .3f), new Color(.75f, .18f, .2f));
        private static readonly InkPalette Night = new InkPalette(new Color(.08f, .05f, .16f), new Color(.45f, .36f, .78f), new Color(.88f, .84f, 1), new Color(.7f, .62f, 1), new Color(.98f, .9f, .55f));
        private static readonly InkPalette Brass = new InkPalette(new Color(.24f, .15f, .06f), new Color(.86f, .66f, .3f), new Color(1, .94f, .7f), new Color(1, .85f, .45f), new Color(.6f, .4f, .16f));
        private static readonly InkPalette ArsenalInkWine = ArsenalInk.Wine;
        private readonly Dictionary<long, Sprite> fillCache = new Dictionary<long, Sprite>();
        private SpriteRenderer[] dreamEchoes;
        private InkedText awakenBanner;
        private float bannerFor = -100;

        /// <summary>The hero's feet where the hero is drawn this frame (the simulation position lags between steps).</summary>
        private static Vector2 DrawnFeet(UnitState u) => DrawnAt(u) - Vector2.up * u.Definition.BodyHeight * .5f;
        private static Vector2 DrawnAt(UnitState u) => UnitVisual.TryDrawn(u.Id, out Vector2 at) ? at : u.Position;
        /// <summary>The column the gauge stands in, just left of the hero, no taller than the hero.</summary>
        private void GaugeColumn(out Vector2 bottom, out Vector2 top, out float half)
        {
            UnitState hero = sim.Champion; float body = hero.Definition.BodyHeight;
            float height = Mathf.Clamp(body * .95f, .5f, 1.15f); half = plain ? .12f : .105f;
            bottom = DrawnFeet(hero) + new Vector2(-(body * .38f + .26f), .08f); top = bottom + Vector2.up * height;
            GaugeBottom = bottom; GaugeTop = top;
        }

        /// <summary>How full the gauge reads: the tier reached plus the hero's own progress toward the next.</summary>
        private float AwakenFill()
        {
            if (sim.HeroAwakening) return sim.HeroAwakenRemaining;
            if (sim.HeroAwakenCooling) return 0;
            float partial = Mathf.Clamp01(sim.HeroIdentityCharge / Mathf.Max(.01f, sim.HeroIdentityRequired)) / 3f;
            return Mathf.Clamp01(sim.HeroAwakenMeter + (sim.HeroAwakenMeter >= 1 ? 0 : partial));
        }

        private void DrawAwakenGauge()
        {
            GaugeColumn(out Vector2 bottom, out Vector2 top, out float half);
            string id = style.Id; bool awake = sim.HeroAwakening, cooling = sim.HeroAwakenCooling;
            float fill = AwakenFill(), height = top.y - bottom.y;
            if (id == "CopperCauldron" && awake) fill = sim.HeroAwakenPressure; // while the engine runs the tube shows its pressure
            float pulse = still ? 0 : (awake ? .5f + .5f * Mathf.Sin(t * 9) : sim.HeroAwakenMeter >= 1 ? .5f + .5f * Mathf.Sin(t * 6) : 0);
            float dim = cooling ? .55f : 1;
            // the simplified gauges (Gauge2_*, content expansion) replace the first paintings once they arrive
            Sprite empty = ArsenalArtwork.Frame("Gauge2_" + id + "_Empty", 0), full = ArsenalArtwork.Frame("Gauge2_" + id + "_Full", 0);
            string key = "Gauge2_" + id;
            if (empty == null || full == null) { empty = ArsenalArtwork.Frame("Gauge_" + id + "_Empty", 0); full = ArsenalArtwork.Frame("Gauge_" + id + "_Full", 0); key = id; }
            if (empty != null && full != null) PaintedGauge(empty, full, bottom, top, fill, dim, HeroGaugeArtwork.For(key));
            else InkGauge(id, bottom, top, half, fill, pulse, dim, awake);
            if ((awake || sim.HeroAwakenMeter >= 1) && !calm) Glow((bottom + top) * .5f, height * 1.3f, ink.Glow, .1f + pulse * .1f, true);
            // primed: the full gauge flashes for the second before the transformation, readable at a glance in a crowd
            if (sim.HeroAwakenPrimed) Glow((bottom + top) * .5f, height * 1.6f, ink.Glow, still ? .35f : .22f + .28f * Mathf.Abs(Mathf.Sin(t * 14)), true);
        }

        /// <summary>An inked vessel until the painted one arrives: the three pilot heroes' own, otherwise the hero's colours.</summary>
        private void InkGauge(string id, Vector2 bottom, Vector2 top, float half, float fill, float pulse, float dim, bool awake)
        {
            float height = top.y - bottom.y;
            bool pilot = id == "Postmaster" || id == "CopperCauldron" || id == "DrowsySeer";
            InkPalette pal = id == "Postmaster" ? Leather : id == "CopperCauldron" ? Brass : id == "DrowsySeer" ? Night : ink;
            InkPalette liquid = id == "Postmaster" ? Leather : id == "CopperCauldron" ? Ember : id == "DrowsySeer" ? Night : ink;
            Color glass = Color.Lerp(pal.Ink, new Color(.05f, .05f, .09f), .45f);
            air.Band(bottom, top, half, A(glass, .6f * strength * dim), A(glass, .4f * strength * dim));
            Vector2 surface = Vector2.Lerp(bottom, top, fill);
            if (fill > .01f && id != "Postmaster")
                air.Band(bottom, surface, half * .78f, A(liquid.Body, .95f * strength * dim), A(Color.Lerp(liquid.Body, liquid.Ink, .25f), .95f * strength * dim));
            if (id == "Postmaster")
            {
                int letters = Mathf.FloorToInt(fill * 6 + .001f);
                for (int i = 0; i < 6; i++)
                {
                    Vector2 at = bottom + Vector2.up * (height * (i + .5f) / 6) + Vector2.right * ((i % 2 == 0 ? -1 : 1) * half * .2f);
                    if (i < letters) Token(InkToken.Envelope, at + Vector2.up * pulse * .02f, .26f, (i % 2 == 0 ? -10 : 10) + (awake && !still ? Mathf.Sin(t * 14 + i) * 8 : 0), .95f * dim, true, 0, Leather);
                    else Place(MechanicInk.Token(InkToken.Envelope, Leather), at, Vector2.one * .2f, 0, new Color(.55f, .55f, .62f, .55f * strength), true);
                }
                Token(InkToken.Knot, top + Vector2.up * .08f, .22f, 0, .95f * dim, true, 0, Leather);
            }
            else if (id == "CopperCauldron")
            {
                air.Stroke(InkCanvas.Line(top + new Vector2(-half * 1.3f, -height * .08f), top + new Vector2(half * 1.3f, -height * .08f)), InkStroke.Of(ArsenalInkWine, .04f, .95f * strength));
                if (fill > .03f) Token(InkToken.Puff, surface + Vector2.up * .05f, .2f, 0, .9f * dim, true, 0, Ember);
            }
            else if (id == "DrowsySeer")
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 at = bottom + Vector2.up * (height * (i + 1) / 3.2f);
                    bool lit = awake || sim.HeroAwakenTier > i;
                    if (lit) Token(InkToken.Moon, at + Vector2.up * pulse * .02f, .24f + i * .03f, -20 + i * 20, .95f * dim, true, 0, Night);
                    else Place(MechanicInk.Token(InkToken.Moon, Night), at, Vector2.one * .2f, -20 + i * 20, new Color(.55f, .55f, .66f, .55f * strength), true);
                }
            }
            if (!pilot) Token(style.Token, surface + Vector2.up * (.04f + pulse * .02f), (fill >= 1 ? .3f : .25f), still ? 0 : Mathf.Sin(t * 2.4f) * 8, fill > .01f ? .95f * dim : .55f, true);
            var outline = new List<Vector2>(20);
            for (int i = 0; i <= 8; i++) outline.Add(bottom + Dir(180 + i * 22.5f) * half);
            for (int i = 0; i <= 8; i++) outline.Add(top + Dir(i * 22.5f) * half);
            air.Stroke(outline, new InkStroke { Width = .035f, Head = 1, Tail = 0, Taper = 1, Alpha = .95f * strength * dim, Ink = pal.Ink, Body = pal.Ink, Light = pal.Ink, InkOnly = true }, true);
            for (int i = 1; i <= 2; i++)
            {
                Vector2 notch = Vector2.Lerp(bottom, top, i / 3f);
                air.Stroke(InkCanvas.Line(notch + Vector2.left * half * 1.25f, notch + Vector2.left * half * .55f), InkStroke.Of(pal, .03f, .9f * strength * dim));
            }
        }

        /// <summary>Painted gauge: the empty vessel, and over it the full one revealed from the bottom up to the fill line.</summary>
        private void PaintedGauge(Sprite empty, Sprite full, Vector2 bottom, Vector2 top, float fill, float dim, Vector2 window)
        {
            Rect ink = SpriteInkMetrics.BoundsOf(empty);
            // every gauge covers about the same area beside the hero, so a wide or round one is no bigger than a tall one
            float column = top.y - bottom.y, area = Mathf.Sqrt(Mathf.Max(.0001f, ink.width * ink.height));
            float scale = Mathf.Min(.45f * column / area, .78f * column / Mathf.Max(.01f, ink.height), .64f * column / Mathf.Max(.01f, ink.width));
            Vector2 at = (bottom + top) * .5f - ink.center * scale;
            PlaceRaw(empty, at, scale, new Color(1, 1, 1, dim * strength));
            Texture2D order = HeroGaugeArtwork.OrderFor("Gauge2_" + style.Id);
            if (order != null && empty.name.StartsWith("Gauge2_", System.StringComparison.Ordinal))
            {
                // shaped gauge: the full painting shows pixel by pixel in the hero's own order (GaugeReveal)
                if (fill <= .001f) return;
                var shaped = Place(full, at, Vector2.one, 0, new Color(1, 1, 1, dim * strength), true);
                if (shaped == null) return;
                shaped.transform.localScale = new Vector3(scale, scale, 1); shaped.sortingOrder += 1;
                revealMaterial ??= Resources.Load<Shader>("GaugeReveal") is Shader shader ? new Material(shader) : null;
                if (revealMaterial == null) return;
                shaped.sharedMaterial = revealMaterial;
                revealBlock ??= new MaterialPropertyBlock();
                shaped.GetPropertyBlock(revealBlock); revealBlock.SetTexture("_OrderTex", order); revealBlock.SetFloat("_Fill", Mathf.Clamp01(fill)); shaped.SetPropertyBlock(revealBlock);
                return;
            }
            if (fill <= .01f) return;
            if (fill >= .995f) { PlaceRaw(full, at, scale, new Color(1, 1, 1, dim * strength), 1); return; }
            Rect fullInk = SpriteInkMetrics.BoundsOf(full);
            float cutLocal = fullInk.yMin + fullInk.height * Mathf.Lerp(window.x, window.y, Mathf.Clamp01(fill));
            float ppu = full.pixelsPerUnit; Rect r = full.rect;
            float cutPixels = Mathf.Clamp((cutLocal - full.bounds.min.y) * ppu, 1, r.height);
            int step = Mathf.Clamp(Mathf.RoundToInt(cutPixels), 1, (int)r.height);
            long key = ((long)full.GetInstanceID() << 16) ^ step;
            if (!fillCache.TryGetValue(key, out Sprite part) || part == null)
            {
                var rect = new Rect(r.x, r.y, r.width, step);
                Vector2 pivot = new Vector2(full.pivot.x / r.width, full.pivot.y / step);
                part = Sprite.Create(full.texture, rect, pivot, ppu, 0, SpriteMeshType.FullRect);
                fillCache[key] = part;
            }
            PlaceRaw(part, at, scale, new Color(1, 1, 1, dim * strength), 1);
        }
        private Material revealMaterial; private MaterialPropertyBlock revealBlock;
        /// <summary>A sprite at its own scale; above lifts it over the vessel drawn underneath (same layer otherwise).</summary>
        private void PlaceRaw(Sprite sprite, Vector2 at, float scale, Color color, int above = 0)
        {
            var r = Place(sprite, at, Vector2.one, 0, color, true);
            if (r != null) { r.transform.localScale = new Vector3(scale, scale, 1); r.sortingOrder += above; }
        }

        /// <summary>The transformations' own painted-ink moments and objects around the hero.</summary>
        private void DrawAwakening()
        {
            UnitState hero = sim.Champion; HeroKit kit = sim.HeroKitInfo;
            bool awake = sim.HeroAwakening;
            Banner(awake, kit);
            if (!awake || kit == null) return;
            float remaining = sim.HeroAwakenRemaining;
            Vector2 at = DrawnAt(hero), facing = hero.Facing.sqrMagnitude > .01f ? hero.Facing.normalized : Vector2.right;
            float side = facing.x < -.05f ? -1 : 1, body = hero.Definition.BodyHeight;
            VisibleCount++;
            // the moment of transforming: a short bright backlight behind the hero (it fades in place, it never spreads out)
            float flash = Activation(), dim = calm ? .45f : 1;
            if (flash > 0) Glow(Chest(hero), Mathf.Max(1.3f, body * 1.6f), Color.Lerp(ink.Light, Color.white, .4f), .7f * flash);
            // a worn or summoned object, placed where its transformation puts it
            if (!kit.Held)
            {
                Sprite prop = ArsenalArtwork.Frame(kit.ArtKey, 0);
                if (prop != null) PlaceProp(kit.Action, prop, at, side, body, hero);
            }
            switch (kit.Action)
            {
                case KitAction.Gatling:
                {
                    Vector2 muzzle = UnitVisual.TryMuzzle(hero.Id, out Vector2 tip) ? tip : Chest(hero) + facing * .55f;
                    Vector2 aim = sim.HeroAwakenAim.sqrMagnitude > .01f ? sim.HeroAwakenAim : facing;
                    for (int i = 0; i < (sparse ? 2 : 4); i++)
                    {
                        float r = Mathf.Repeat(t * 5 + i * .25f, 1);
                        Token(InkToken.Envelope, muzzle + aim * r * .9f + new Vector2(-aim.y, aim.x) * Mathf.Sin(r * 9 + i) * .08f, .2f * (1 - r * .4f), still ? 0 : r * 360, (1 - r) * .9f, true, 0, Leather);
                    }
                    break;
                }
                case KitAction.Engine:
                    if (!still)
                    {
                        for (int i = 0; i < 3; i++) { float r = Mathf.Repeat(t * 1.6f + i / 3f, 1); Token(InkToken.Puff, Head(hero) + new Vector2(-side * r * .4f, .1f + r * .6f), .22f + r * .25f, 0, (1 - r) * .85f, true, 0, ArsenalInk.Cotton); }
                        for (int i = 0; i < 3; i++) { float r = Mathf.Repeat(t * 2.2f + i / 3f, 1); Token(InkToken.Puff, Feet(hero) - facing * (.3f + r * .9f) + Vector2.up * .1f, .2f + r * .2f, 0, (1 - r) * .7f, false, 0, ArsenalInk.Cotton); }
                    }
                    if (sim.HeroAwakenPressure > .75f && !still && !calm) Glow(Chest(hero), 1.1f, new Color(1, .5f, .3f), .22f * Mathf.Abs(Mathf.Sin(t * 14)), true);
                    break;
                case KitAction.Dreams:
                    Token(InkToken.Moon, Head(hero) + Vector2.up * .35f + Vector2.up * Bob(0), .42f, 0, .95f, true, 0, Night);
                    Glow(Head(hero) + Vector2.up * .35f, 1f, Night.Glow, .3f * dim, true);
                    Echoes(hero, hero.Definition.Body, new Color(.72f, .66f, 1f), remaining, InkToken.Moon, Night);
                    break;
                case KitAction.Mitosis:
                    Echoes(hero, ArsenalArtwork.Frame(kit.ArtKey, 0) ?? hero.Definition.Body, new Color(.6f, 1f, .9f), remaining, InkToken.Drop, ink);
                    break;
                case KitAction.Showtime:
                {
                    UnitState lead = sim.HeroIdentityLead;
                    Echoes(hero, lead != null ? lead.Definition.Body : hero.Definition.Body, new Color(1f, .9f, .75f), remaining, InkToken.Star, ink);
                    break;
                }
                case KitAction.Lighthouse:
                {
                    // the beam is the strike (its simulation strikes are marked Painted, no ink glyph). A volumetric ray of light
                    // (owner, 22/10/2026): an amber halo and a white-gold core, both a soft gaussian across their width, widening
                    // like a cone away from the lamp and fading out along their length, added onto the floor as light; a gentle
                    // shimmer, dust motes turning inside it, a soft bloom at the lamp. No stroke or hard edge anywhere.
                    float angle = (sim.Elapsed - sim.HeroAwakenStarted) * 120f;
                    Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.right, across = new Vector2(-dir.y, dir.x);
                    float reach = CombatSimulation.MaxLineLength + .6f, shimmer = still || calm ? 1 : .9f + .1f * Mathf.Sin(t * 7.3f) * Mathf.Sin(t * 3.1f + 1);
                    Vector2 lamp = at + dir * .3f;
                    Sprite ray = BeamSprite();
                    Color amber = Color.Lerp(ink.Glow, new Color(1, .62f, .2f), .35f), gold = new Color(1, .95f, .78f);
                    if (!sparse && !still)
                        for (int i = 2; i >= 1; i--) // the afterglow the sweep leaves behind it
                            Light(ray, lamp, 3.2f, reach, angle - i * 6f - 90, A(amber, .16f / i * dim * strength));
                    Light(ray, lamp, 3.6f + .6f * flash, reach, angle - 90, A(amber, (.5f + .25f * flash) * shimmer * dim * strength));
                    Light(ray, lamp, 1.25f + .3f * flash, reach * .96f, angle - 90, A(gold, (.62f + .3f * flash) * shimmer * dim * strength));
                    Light(ray, lamp, .45f, reach * .85f, angle - 90, A(Color.white, (.35f + .3f * flash) * dim * strength));
                    Light(MechanicInk.Glow, lamp, 1.5f + .6f * flash, 1.5f + .6f * flash, 0, A(gold, (.55f + .35f * flash) * dim * strength));
                    if (!sparse) for (int i = 0; i < 6; i++)
                    {
                        float r = still ? (i + .5f) / 6 : Mathf.Repeat(t * .35f + i / 6f, 1), d = .6f + r * (reach - 1.2f);
                        float drift = Mathf.Sin(i * 2.4f + (still ? 0 : t * .8f)) * .35f * Mathf.Lerp(.32f, 1f, d / reach) * 1.6f;
                        float twinkle = still || calm ? .7f : .5f + .5f * Mathf.Sin(t * 5 + i * 1.9f);
                        Light(MechanicInk.Glow, lamp + dir * d + across * drift, .14f, .14f, 0, A(gold, Mathf.Sin(r * Mathf.PI) * twinkle * .8f * dim * strength));
                    }
                    break;
                }
                case KitAction.Mirrors:
                {
                    Sprite pane = ArsenalArtwork.Frame(kit.ArtKey, 0);
                    for (int i = 0; i < 4; i++)
                    {
                        float a = (still ? 0 : t * 70) + i * 90;
                        Vector2 p = Chest(hero) + new Vector2(Mathf.Cos(a * Mathf.Deg2Rad) * 1.3f, Mathf.Sin(a * Mathf.Deg2Rad) * .7f);
                        if (pane != null) Place(pane, p, Vector2.one * .55f, 0, new Color(1, 1, 1, .95f * strength), true);
                        else Token(InkToken.Shard, p, .35f, a, .9f, true);
                    }
                    break;
                }
                case KitAction.Rink:
                case KitAction.Monsoon:
                    if (!still) for (int i = 0; i < (sparse ? 3 : 6); i++)
                    {
                        float r = Mathf.Repeat(t * (kit.Action == KitAction.Monsoon ? 2.4f : .8f) + i / 6f, 1);
                        Vector2 p = at + new Vector2(((i * 53) % 11 - 5) * .45f, kit.Action == KitAction.Monsoon ? 2.2f - r * 2.6f : -.5f + ((i * 31) % 7 - 3) * .15f);
                        Token(kit.Action == KitAction.Monsoon ? InkToken.Drop : InkToken.Glint, p, .2f, kit.Action == KitAction.Monsoon ? 180 : r * 180, (1 - r) * .95f, true, 0, ArsenalInk.Water);
                    }
                    break;
                case KitAction.KiteFleet:
                {
                    // a painted kite rides the wind over each escorted friend, tied to it by its string (one over the admiral when
                    // no ranged friend is near)
                    Sprite kite = ArsenalArtwork.Frame(kit.ArtKey, 0);
                    var fleet = sim.HeroKiteFleet; int n = fleet.Count == 0 ? 1 : fleet.Count;
                    for (int i = 0; i < n; i++)
                    {
                        UnitState friend = fleet.Count == 0 ? hero : fleet[i];
                        Vector2 hand = DrawnAt(friend) + Vector2.up * friend.Definition.BodyHeight * .15f;
                        Vector2 sky = DrawnAt(friend) + new Vector2(still ? .25f : .25f + Mathf.Sin(t * 1.7f + i * 1.3f) * .18f, friend.Definition.BodyHeight * .5f + .95f + Bob(i + 2) * 2);
                        Thread(hand, sky + Vector2.down * .2f, .85f, 11 + i);
                        if (kite != null) { var r = Place(kite, sky, Vector2.one * .62f, still ? 0 : Mathf.Sin(t * 2.3f + i) * 12, new Color(1, 1, 1, strength * Mathf.Clamp01(remaining * 5)), true); if (r != null) r.flipX = false; }
                        else Token(InkToken.Spark, sky, .4f, 0, .9f, true);
                        if (!sparse) for (int k = 0; k < 2; k++) { float q = Mathf.Repeat(t * .9f + k * .5f + i * .2f, 1); Token(InkToken.Glint, sky + new Vector2(-.3f - q * .5f, -.15f - q * .25f), .14f * (1 - q), 0, (1 - q) * .7f, true, 0, ArsenalInk.Water); }
                    }
                    break;
                }
                case KitAction.TimeGarden:
                {
                    // the bonsai stands where it was planted; its clock hand is painted at the angle the simulation strikes with
                    Vector2 plot = sim.HeroAwakenPlot, hand = sim.HeroGardenHand;
                    Sprite tree = ArsenalArtwork.Frame(kit.ArtKey, 0);
                    if (tree != null) Place(tree, plot + Vector2.up * .35f, Vector2.one * Mathf.Max(.8f, body * 1.05f), 0, new Color(1, 1, 1, strength * Mathf.Clamp01(remaining * 5)), false);
                    Vector2 pivot = plot + Vector2.up * .45f;
                    // the hand is the strike (its simulation strikes are marked Painted, no ink glyph over it): a blade of soft light
                    float handAngle = Mathf.Atan2(hand.y, hand.x) * Mathf.Rad2Deg - 90, handLength = CombatSimulation.GardenReach + .3f;
                    Light(BeamSprite(), pivot + hand * .2f, 1.3f, handLength, handAngle, A(Color.Lerp(ink.Glow, ink.Light, .3f), .55f * dim * strength));
                    Light(BeamSprite(), pivot + hand * .2f, .45f, handLength * .95f, handAngle, A(new Color(1, .97f, .85f), .5f * dim * strength));
                    Soft(air, InkCanvas.Line(pivot + hand * .3f, pivot + hand * CombatSimulation.GardenReach), 1, 0, .1f, .8f * dim);
                    Light(MechanicInk.Glow, pivot + hand * CombatSimulation.GardenReach, .6f, .6f, 0, A(ink.Light, .45f * dim * strength));
                    if (!sparse) for (int i = 0; i < 3; i++) { float q = Mathf.Repeat(t * .5f + i / 3f, 1); Token(InkToken.Leaf, pivot + Dir(i * 120 + (still ? 0 : t * 40)) * (.6f + q * .5f), .16f, q * 180, (1 - q) * .8f, true); }
                    break;
                }
                case KitAction.Circus:
                {
                    // three balls juggled over the jester's head
                    for (int i = 0; i < 3; i++)
                    {
                        float q = still ? i / 3f : Mathf.Repeat(t * 1.4f + i / 3f, 1);
                        Vector2 ball = Head(hero) + new Vector2(Mathf.Cos(q * Mathf.PI * 2) * .32f, .3f + Mathf.Abs(Mathf.Sin(q * Mathf.PI * 2)) * .45f);
                        Token(InkToken.Star, ball, .2f, still ? 0 : q * 360, .95f, true);
                    }
                    break;
                }
                case KitAction.DivingBell:
                    if (!sparse && !still) for (int i = 0; i < 4; i++)
                    {
                        float r = Mathf.Repeat(t * 1.3f + i / 4f, 1);
                        Token(InkToken.Bubble, Head(hero) + new Vector2(((i * 37) % 7 - 3) * .06f, .15f + r * .8f), .12f + r * .08f, 0, (1 - r) * .95f, true, 0, ArsenalInk.Water);
                    }
                    break;
            }
            // what the transformation drops from above: giant scoops falling on their enemies, depth charges about to burst
            if (kit.Action == KitAction.SundaeRain || kit.Action == KitAction.DivingBell)
            {
                Sprite scoop = kit.Action == KitAction.SundaeRain ? ArsenalArtwork.Frame(kit.ArtKey, 0) : null;
                foreach (var d in sim.HeroAwakenDrops)
                {
                    if (t > d.LandAt + .4f) continue;
                    float fall = Mathf.InverseLerp(d.Born, d.LandAt, t), after = Mathf.Clamp01((t - d.LandAt) / .4f);
                    if (d.Kind == 0)
                    {
                        // the scoop is the strike (marked Painted, no ink glyph): its shadow grows where it lands, then a bright splat
                        Glow(d.At, .7f * (.5f + .5f * fall), ink.Ink, .45f * (1 - after));
                        Vector2 p = d.At + Vector2.up * (2.6f * (1 - InQuad(fall)) + .15f);
                        if (scoop != null) Place(scoop, p, Vector2.one * .62f * (1 + .3f * after), 0, new Color(1, 1, 1, strength * (1 - after)), true);
                        else Token(InkToken.Dumpling, p, .5f, 0, 1 - after, true);
                        if (after > 0) Splash(d.At, after, ink, dim);
                    }
                    else
                    {
                        // the charge settles, ticks, and bubbles ever faster before it bursts (marked Painted, no ink glyph)
                        Token(InkToken.Seal, d.At + Vector2.up * .1f, .26f, still ? 0 : Mathf.Sin(t * 20 * fall) * 10, (1 - after) * .95f, false, 0, ArsenalInk.Water);
                        if (!calm && after <= 0 && fall > .5f) Glow(d.At + Vector2.up * .1f, .5f, ArsenalInk.Water.Glow, .35f * fall * (still ? 1 : .5f + .5f * Mathf.Abs(Mathf.Sin(t * 18 * fall))), true);
                        if (!sparse) for (int i = 0; i < 2; i++) { float q = Mathf.Repeat(t * (1 + 2 * fall) + i * .5f, 1); Token(InkToken.Bubble, d.At + new Vector2((i - .5f) * .14f, .2f + q * .5f), .1f, 0, (1 - q) * (1 - after) * .95f, true, 0, ArsenalInk.Water); }
                        if (after > 0) Splash(d.At, after, ArsenalInk.Water, dim);
                    }
                }
            }
        }
        private static Sprite beam;
        /// <summary>The lighthouse ray (one unit long, pivot at the lamp): a gaussian across its width that widens from a third
        /// at the lamp to the full width at the far end, rising softly from the lamp and fading out along its length.</summary>
        private static Sprite BeamSprite()
        {
            if (beam != null) return beam;
            const int w = 48, h = 128;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { name = "Lighthouse ray", wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave };
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
            {
                float v = (y + .5f) / h, spread = Mathf.Lerp(.32f, 1f, v);
                float along = Mathf.SmoothStep(0, 1, v / .1f) * (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.3f, 1f, v))) * (1 - .25f * v);
                for (int x = 0; x < w; x++)
                {
                    float u = ((x + .5f) / w * 2 - 1) / spread;
                    px[y * w + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(Mathf.Exp(-u * u * 4.5f) * along) * 255));
                }
            }
            tex.SetPixels32(px); tex.Apply(false, true);
            return beam = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(.5f, 0), h, 0, SpriteMeshType.FullRect);
        }
        /// <summary>A landed scoop or burst charge: a short bright flare at the spot and a few glints thrown up, never a pool.</summary>
        private void Splash(Vector2 at, float after, InkPalette pal, float dim)
        {
            Glow(at + Vector2.up * .15f, .95f, Color.Lerp(pal.Light, Color.white, .3f), .6f * (1 - after) * (1 - after) * dim, true);
            if (sparse) return;
            for (int i = 0; i < 4; i++)
            {
                float lift = still ? .5f : OutCubic(after);
                Vector2 p = at + new Vector2((i - 1.5f) * (.12f + .25f * lift), .15f + (.25f + .12f * (i % 2)) * lift);
                Token(InkToken.Glint, p, .15f * (1 - .5f * after), i * 45 + after * 120, (1 - after) * .95f * dim, true, 0, GlintPalette(pal));
            }
        }
        /// <summary>Where a transformation's worn or summoned object sits.</summary>
        private void PlaceProp(KitAction action, Sprite prop, Vector2 at, float side, float body, UnitState hero)
        {
            Vector2 feet = at - Vector2.up * body * .5f;
            switch (action)
            {
                case KitAction.Engine: case KitAction.Train:
                {
                    var r = Place(prop, feet + new Vector2(side * (hero.Definition.Radius + .18f), body * .3f), Vector2.one * Mathf.Max(.8f, body * 1.05f), 0, new Color(1, 1, 1, strength), true);
                    if (r != null) r.flipX = side < 0; break;
                }
                case KitAction.HighTide: case KitAction.Rink: case KitAction.Treant:
                {
                    var r = Place(prop, feet + Vector2.up * .05f, Vector2.one * Mathf.Max(.8f, body * 1.1f), 0, new Color(1, 1, 1, strength), false);
                    if (r != null) r.flipX = side < 0; break;
                }
                case KitAction.Throne: case KitAction.Citadel: case KitAction.Eclipse:
                {
                    var r = Place(prop, at + Vector2.up * body * .1f, Vector2.one * body * 1.6f, 0, new Color(1, 1, 1, strength), false);
                    if (r != null) { r.flipX = side < 0; r.sortingOrder = 1000 - Mathf.RoundToInt(hero.Position.y * 12f) - 4; } // just behind the hero (UnitVisual orders bodies by 1000 - y*12; its held weapon may sit at -2)
                    break;
                }
                case KitAction.Feast:
                {
                    var r = Place(prop, at + new Vector2(side * (body * .7f), 0), Vector2.one * body * 1.2f, 0, new Color(1, 1, 1, strength), true);
                    if (r != null) r.flipX = side < 0; break;
                }
                case KitAction.Mirrors: case KitAction.Mitosis: case KitAction.Showtime: break; // drawn as copies (DrawAwakening)
                case KitAction.KiteFleet: case KitAction.TimeGarden: case KitAction.SundaeRain: break; // over the friends, at the bonsai's spot, falling (DrawAwakening)
                case KitAction.DivingBell:
                {
                    // the hero rides inside the bell: drawn over the body, a little larger
                    var r = Place(prop, at + Vector2.up * body * .05f, Vector2.one * Mathf.Max(.85f, body * 1.3f), 0, new Color(1, 1, 1, strength), true);
                    if (r != null) r.flipX = side < 0; break;
                }
                default:
                    Place(prop, at + Vector2.up * (body * .5f + .55f + Bob(1)), Vector2.one * Mathf.Max(.6f, body * .8f), 0, new Color(1, 1, 1, strength), true);
                    break;
            }
        }
        /// <summary>Copies standing where the simulation puts them (dreams, jellies, performers), with a token over each.</summary>
        private void Echoes(UnitState hero, Sprite body, Color tint, float remaining, InkToken token, InkPalette pal)
        {
            if (dreamEchoes == null)
            {
                dreamEchoes = new SpriteRenderer[3];
                for (int i = 0; i < 3; i++) { var go = new GameObject("Transformation copy " + i); go.transform.SetParent(transform, false); dreamEchoes[i] = go.AddComponent<SpriteRenderer>(); dreamEchoes[i].sortingOrder = 1085; }
            }
            var echoes = sim.HeroAwakenEchoes;
            for (int i = 0; i < 3 && body != null; i++)
            {
                SpriteRenderer e = dreamEchoes[i];
                Vector2 at = echoes[i];
                if ((at - hero.Position).sqrMagnitude < .04f) continue;
                e.enabled = true; e.sprite = body;
                float unit = Mathf.Max(.01f, body.bounds.size.y), size = hero.Definition.BodyHeight * .95f / unit;
                e.transform.position = at + Vector2.up * Bob(i + 1) * 2; e.transform.localScale = new Vector3(hero.Facing.x < 0 ? -size : size, size, 1);
                e.color = new Color(tint.r, tint.g, tint.b, (.7f - i * .1f) * Mathf.Clamp01(remaining * 4) * strength);
                Token(token, at + Vector2.up * (hero.Definition.BodyHeight * .5f + .25f), .22f, 0, .95f - i * .15f, true, 0, pal);
                Thread(i > 0 ? echoes[i - 1] + Vector2.up * .1f : hero.Position + Vector2.up * .1f, at + Vector2.up * .1f, .25f, i);
            }
        }

        /// <summary>The transformation's name pops up over the hero for a moment as it begins.</summary>
        private void Banner(bool awake, HeroKit kit)
        {
            if (awake && sim.HeroAwakenStarted > bannerFor) bannerFor = sim.HeroAwakenStarted;
            float age = t - bannerFor;
            bool show = awake && kit != null && age < 1.4f;
            if (awakenBanner == null)
            {
                if (!show) return;
                awakenBanner = InkedText.Create(transform, "Awakening banner", ArcadeTypography.Punch, 48, .045f, 2400, .028f);
            }
            awakenBanner.gameObject.SetActive(show);
            if (!show) return;
            awakenBanner.Text = kit.Transformation + "!";
            float pop = still ? 1 : OutBack(Mathf.Clamp01(age / .25f));
            awakenBanner.transform.position = Head(sim.Champion) + Vector2.up * (.55f + (still ? 0 : age * .15f));
            awakenBanner.transform.localScale = Vector3.one * Mathf.Max(.01f, pop);
            Color c = Color.Lerp(ink.Body, Color.white, .35f); c.a = 1 - Smooth(1f, 1.4f, age); awakenBanner.Color = c;
        }
    }
}
