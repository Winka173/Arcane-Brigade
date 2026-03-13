using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The awakening heroes' own gauges and transformations (18/10/2026). Each pilot hero's gauge looks like its
    /// own vessel, standing just left of the hero: painted art (Gauge_&lt;Hero&gt;_Empty / _Full, the full one revealed from
    /// the bottom up) when it exists, otherwise an inked vessel of the hero's own: a mail sack stacking letters, a boiler
    /// tube with a red line, a column of moons. It fills a third per mechanic cast (and creeps up with the mechanic's own
    /// charge), drains while transformed, and rests dim while cooling.</summary>
    public sealed partial class HeroIdentityVisual
    {
        private static readonly InkPalette Leather = new InkPalette(new Color(.2f, .11f, .06f), new Color(.62f, .4f, .22f), new Color(.96f, .85f, .65f), new Color(.95f, .75f, .5f), new Color(.85f, .2f, .25f));
        private static readonly InkPalette Ember = new InkPalette(new Color(.25f, .08f, .05f), new Color(.98f, .55f, .22f), new Color(1, .9f, .62f), new Color(1, .65f, .3f), new Color(.75f, .18f, .2f));
        private static readonly InkPalette Night = new InkPalette(new Color(.08f, .05f, .16f), new Color(.45f, .36f, .78f), new Color(.88f, .84f, 1), new Color(.7f, .62f, 1), new Color(.98f, .9f, .55f));
        private static readonly InkPalette Brass = new InkPalette(new Color(.24f, .15f, .06f), new Color(.86f, .66f, .3f), new Color(1, .94f, .7f), new Color(1, .85f, .45f), new Color(.6f, .4f, .16f));
        private readonly Dictionary<long, Sprite> fillCache = new Dictionary<long, Sprite>();
        private SpriteRenderer[] dreamEchoes;
        private InkedText awakenBanner;
        private float bannerFor = -100;

        /// <summary>The column the gauge stands in, just left of the hero, no taller than the hero.</summary>
        private void GaugeColumn(out Vector2 bottom, out Vector2 top, out float half)
        {
            UnitState hero = sim.Champion; float body = hero.Definition.BodyHeight;
            float height = Mathf.Clamp(body * .95f, .5f, 1.15f); half = plain ? .12f : .105f;
            bottom = Feet(hero) + new Vector2(-(body * .38f + .26f), .08f); top = bottom + Vector2.up * height;
            GaugeBottom = bottom; GaugeTop = top;
        }

        /// <summary>How full the awakening gauge reads: the tier reached plus the mechanic's own progress toward the next.</summary>
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
            Sprite empty = ArsenalArtwork.Frame("Gauge_" + id + "_Empty", 0), full = ArsenalArtwork.Frame("Gauge_" + id + "_Full", 0);
            if (empty != null && full != null) { PaintedGauge(empty, full, bottom, top, fill, dim, GaugeWindow(id)); }
            else
            {
                InkPalette pal = id == "Postmaster" ? Leather : id == "CopperCauldron" ? Brass : Night;
                InkPalette liquid = id == "Postmaster" ? Leather : id == "CopperCauldron" ? Ember : Night;
                Color glass = Color.Lerp(pal.Ink, new Color(.05f, .05f, .09f), .45f);
                air.Band(bottom, top, half, A(glass, .6f * strength * dim), A(glass, .4f * strength * dim));
                Vector2 surface = Vector2.Lerp(bottom, top, fill);
                if (fill > .01f && id != "Postmaster")
                    air.Band(bottom, surface, half * .78f, A(liquid.Body, .95f * strength * dim), A(Color.Lerp(liquid.Body, liquid.Ink, .25f), .95f * strength * dim));
                if (id == "Postmaster")
                {
                    // a mail sack: letters stack up inside, one more for each sixth of the gauge
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
                    // a boiler tube: brass bolts at the tiers, a red line at the top, steam over it when it is hot
                    air.Stroke(InkCanvas.Line(top + new Vector2(-half * 1.3f, -height * .08f), top + new Vector2(half * 1.3f, -height * .08f)), InkStroke.Of(ArsenalInkWine, .04f, .95f * strength));
                    if (fill > .03f) Token(InkToken.Puff, surface + Vector2.up * .05f, .2f, 0, .9f * dim, true, 0, Ember);
                    if ((awake || sim.HeroAwakenMeter >= 1) && !still)
                        for (int i = 0; i < 2; i++) { float r = Mathf.Repeat(t * 1.3f + i * .5f, 1); Token(InkToken.Puff, top + new Vector2((i - .5f) * .1f, .1f + r * .35f), .2f + r * .12f, 0, (1 - r) * .8f, true, 0, ArsenalInk.Cotton); }
                }
                else
                {
                    // a column of moons: each tier reached lights one, the top one full
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 at = bottom + Vector2.up * (height * (i + 1) / 3.2f);
                        bool lit = awake || sim.HeroAwakenTier > i;
                        if (lit) Token(InkToken.Moon, at + Vector2.up * pulse * .02f, .24f + i * .03f, -20 + i * 20, .95f * dim, true, 0, Night);
                        else Place(MechanicInk.Token(InkToken.Moon, Night), at, Vector2.one * .2f, -20 + i * 20, new Color(.55f, .55f, .66f, .55f * strength), true);
                    }
                    Token(InkToken.Star, top + Vector2.up * .1f, .18f, still ? 0 : t * 30, .9f * dim, true, 0, Night);
                }
                // the vessel's own inked outline and its tier notches
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
            if ((awake || sim.HeroAwakenMeter >= 1) && !calm) Glow((bottom + top) * .5f, height * 1.3f, ink.Glow, .1f + pulse * .1f, true);
        }
        private static readonly InkPalette ArsenalInkWine = ArsenalInk.Wine;

        /// <summary>Painted gauge: the empty vessel, and over it the full one revealed from the bottom up to the fill line.</summary>
        /// <summary>Where each painted vessel's window runs (fractions of its painted height from the bottom), measured from
        /// where the empty and full paintings differ: the fill line travels this window, not the whole vessel.</summary>
        private static Vector2 GaugeWindow(string id) => id == "Postmaster" ? new Vector2(.136f, .693f) : id == "CopperCauldron" ? new Vector2(.177f, .759f) : new Vector2(.141f, .771f);
        private void PaintedGauge(Sprite empty, Sprite full, Vector2 bottom, Vector2 top, float fill, float dim, Vector2 window)
        {
            Rect ink = SpriteInkMetrics.BoundsOf(empty);
            float scale = (top.y - bottom.y) / Mathf.Max(.01f, ink.height);
            Vector2 at = (bottom + top) * .5f - ink.center * scale;
            PlaceRaw(empty, at, scale, new Color(1, 1, 1, dim * strength));
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
        /// <summary>A sprite at its own scale; above lifts it over the vessel drawn underneath (same layer otherwise).</summary>
        private void PlaceRaw(Sprite sprite, Vector2 at, float scale, Color color, int above = 0)
        {
            var r = Place(sprite, at, Vector2.one, 0, color, true);
            if (r != null) { r.transform.localScale = new Vector3(scale, scale, 1); r.sortingOrder += above; }
        }

        /// <summary>The transformations' own painted-ink moments around the hero.</summary>
        private void DrawAwakening()
        {
            UnitState hero = sim.Champion; string id = style.Id;
            bool awake = sim.HeroAwakening;
            if (dreamEchoes != null) foreach (var e in dreamEchoes) if (e != null) e.enabled = false;
            Banner(awake);
            if (!awake) return;
            float since = t - sim.HeroAwakenStarted, remaining = sim.HeroAwakenRemaining;
            Vector2 facing = hero.Facing.sqrMagnitude > .01f ? hero.Facing.normalized : Vector2.right;
            if (id == "Postmaster")
            {
                // letters stream out of the sorter's barrels toward the aim
                Vector2 muzzle = Chest(hero) + facing * .55f;
                for (int i = 0; i < (sparse ? 2 : 4); i++)
                {
                    float r = Mathf.Repeat(t * 5 + i * .25f, 1);
                    Token(InkToken.Envelope, muzzle + facing * r * .9f + new Vector2(-facing.y, facing.x) * Mathf.Sin(r * 9 + i) * .08f, .2f * (1 - r * .4f), still ? 0 : r * 360, (1 - r) * .9f, true, 0, Leather);
                }
                if (!still) for (int i = 0; i < 6; i++) { float a = t * 720 + i * 60; Token(InkToken.Glint, muzzle + Dir(a) * .1f * new Vector2(.4f, 1), .1f, a, .5f, true, 0, Brass); }
            }
            else if (id == "CopperCauldron")
            {
                Vector2 heading = facing;
                Sprite engine = ArsenalArtwork.Frame("CopperCauldron_Engine", 0);
                // the locomotive front sits low across the kettle's body on the side it faces (left or right), like a cowcatcher
                float side = heading.x < -.05f ? -1 : 1;
                Vector2 front = Feet(hero) + new Vector2(side * (hero.Definition.Radius + .18f), hero.Definition.BodyHeight * .3f);
                if (engine != null)
                {
                    var r = Place(engine, front, Vector2.one * Mathf.Max(.8f, hero.Definition.BodyHeight * 1.05f), 0, new Color(1, 1, 1, strength), true);
                    if (r != null) r.flipX = side < 0;
                }
                // a smokestack plume rising over the hero and steam rolling off behind the engine
                if (!still)
                {
                    for (int i = 0; i < 3; i++) { float r = Mathf.Repeat(t * 1.6f + i / 3f, 1); Token(InkToken.Puff, Head(hero) + new Vector2(-heading.x * r * .4f, .1f + r * .6f), .22f + r * .25f, 0, (1 - r) * .85f, true, 0, ArsenalInk.Cotton); }
                    for (int i = 0; i < 3; i++) { float r = Mathf.Repeat(t * 2.2f + i / 3f, 1); Token(InkToken.Puff, Feet(hero) - heading * (.3f + r * .9f) + Vector2.up * .1f, .2f + r * .2f, 0, (1 - r) * .7f, false, 0, ArsenalInk.Cotton); }
                }
                if (sim.HeroAwakenPressure > .75f && !still && !calm) Glow(Chest(hero), 1.1f, new Color(1, .5f, .3f), .15f * Mathf.Abs(Mathf.Sin(t * 14)), true);
            }
            else if (id == "DrowsySeer")
            {
                // the full moon over the seer, and the three dreams walking its route behind
                Token(InkToken.Moon, Head(hero) + Vector2.up * .35f + Vector2.up * Bob(0), .42f, 0, .95f, true, 0, Night);
                if (!calm) Glow(Head(hero) + Vector2.up * .35f, .9f, Night.Glow, .2f, true);
                if (dreamEchoes == null)
                {
                    dreamEchoes = new SpriteRenderer[3];
                    for (int i = 0; i < 3; i++) { var go = new GameObject("Full moon dream " + i); go.transform.SetParent(transform, false); dreamEchoes[i] = go.AddComponent<SpriteRenderer>(); dreamEchoes[i].sortingOrder = 1085; }
                }
                Sprite body = hero.Definition.Body;
                var echoes = sim.HeroAwakenEchoes;
                for (int i = 0; i < 3 && body != null; i++)
                {
                    SpriteRenderer e = dreamEchoes[i];
                    Vector2 at = echoes[i];
                    if ((at - hero.Position).sqrMagnitude < .04f) continue; // a dream standing on the seer is not drawn
                    e.enabled = true; e.sprite = body;
                    float unit = Mathf.Max(.01f, body.bounds.size.y), size = hero.Definition.BodyHeight * .95f / unit;
                    e.transform.position = at + Vector2.up * Bob(i + 1) * 2; e.transform.localScale = new Vector3(hero.Facing.x < 0 ? -size : size, size, 1);
                    e.color = new Color(.72f, .66f, 1f, (.5f - i * .1f) * Mathf.Clamp01(remaining * 4) * strength);
                    Token(InkToken.Moon, at + Vector2.up * (hero.Definition.BodyHeight * .5f + .25f), .2f, 0, .8f - i * .15f, true, 0, Night);
                    if (i > 0) Thread(echoes[i - 1] + Vector2.up * .1f, at + Vector2.up * .1f, .25f, i);
                    else Thread(hero.Position + Vector2.up * .1f, at + Vector2.up * .1f, .25f, i);
                }
            }
        }

        /// <summary>The transformation's name pops up over the hero for a moment as it begins.</summary>
        private void Banner(bool awake)
        {
            if (awake && sim.HeroAwakenStarted > bannerFor) bannerFor = sim.HeroAwakenStarted;
            float age = t - bannerFor;
            bool show = awake && age < 1.4f;
            if (awakenBanner == null)
            {
                if (!show) return;
                awakenBanner = InkedText.Create(transform, "Awakening banner", ArcadeTypography.Punch, 48, .045f, 2400, .028f);
            }
            awakenBanner.gameObject.SetActive(show);
            if (!show) return;
            string id = style.Id;
            awakenBanner.Text = id == "Postmaster" ? "SORTING GATLING!" : id == "CopperCauldron" ? "STEAM ENGINE!" : "FULL MOON!";
            float pop = still ? 1 : OutBack(Mathf.Clamp01(age / .25f));
            awakenBanner.transform.position = Head(sim.Champion) + Vector2.up * (.55f + (still ? 0 : age * .15f));
            awakenBanner.transform.localScale = Vector3.one * Mathf.Max(.01f, pop);
            Color c = Color.Lerp(ink.Body, Color.white, .35f); c.a = 1 - Smooth(1f, 1.4f, age); awakenBanner.Color = c;
        }
    }
}
