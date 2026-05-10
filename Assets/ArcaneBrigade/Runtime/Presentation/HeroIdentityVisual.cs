using System.Collections.Generic;
using UnityEngine;
namespace ArcaneBrigade
{
    /// <summary>Paints every playable hero's identity mechanic. Two layers: a ground layer of watercolour washes and inked
    /// rims that show the exact area the simulation tests (never larger or smaller), and an air layer of brush strokes and
    /// signature tokens that animate anticipation, action and follow-through inside that area. Live mechanic state (marks,
    /// roots, dreams, stored reflections, routes) and the resource gauge are drawn from read-only simulation views.</summary>
    public sealed partial class HeroIdentityVisual : MonoBehaviour
    {
        private const int GroundOrder = 74, AirOrder = 2060, GroundTokenOrder = 77, AirTokenOrder = 2064;
        private const float CrescentHalf = 84.26f; // the simulation accepts dot(direction) >= .1
        private InkCanvas ground, air, vial, groundInk;
        private readonly List<SpriteRenderer> tokens = new List<SpriteRenderer>();
        private SpriteRenderer ghost;
        private int used;
        private float lastCast = -100, castShown = -100, gaugeFull = -100;
        private int lastStored, lastCharge;
        private float pipLitAt;
        private CombatSimulation sim;
        private HeroMechanicStyle style;
        private InkPalette ink;
        private float t, strength, screenCap = 3;
        /// <summary>Largest radius any identity shape may be painted at: the simulation caps the real area
        /// (CombatSimulation.MaxCrescentRadius and friends) and this is a safety net of a fraction of the visible screen.</summary>
        private float Fit(float radius) => Mathf.Min(radius, screenCap);
        private bool still, sparse, calm, plain;
        public int VisibleCount { get; private set; }
        public int TokenCount => used;

        private static float OutCubic(float x) { x = Mathf.Clamp01(x); return 1 - (1 - x) * (1 - x) * (1 - x); }
        private static float InQuad(float x) { x = Mathf.Clamp01(x); return x * x; }
        private static float OutBack(float x) { x = Mathf.Clamp01(x); const float c = 1.6f; float y = x - 1; return 1 + (c + 1) * y * y * y + c * y * y; }
        private static float Smooth(float a, float b, float x) => Mathf.SmoothStep(0, 1, Mathf.InverseLerp(a, b, x));
        private static Vector2 Dir(float degrees) => new Vector2(Mathf.Cos(degrees * Mathf.Deg2Rad), Mathf.Sin(degrees * Mathf.Deg2Rad));
        private static Color A(Color c, float alpha) { c.a = Mathf.Clamp01(alpha); return c; }

        private void Ensure()
        {
            if (ground != null) return;
            ground = new InkCanvas(transform, "Hero mechanic ground ink", GroundOrder, true); // light, like the lantern's
            air = new InkCanvas(transform, "Hero mechanic air ink", AirOrder, true);
            vial = new InkCanvas(transform, "Hero mechanic gauge ink", AirOrder); // the gauge stays an inked object
            groundInk = new InkCanvas(transform, "Hero mechanic ground objects", GroundOrder + 1); // roots are wood, not light
            var go = new GameObject("Dream identity portrait"); go.transform.SetParent(transform, false);
            ghost = go.AddComponent<SpriteRenderer>(); ghost.sortingOrder = 1090; ghost.enabled = false;
        }

        private Material spriteMaterial;
        private static Material glowLight;
        private SpriteRenderer Place(Sprite sprite, Vector2 at, Vector2 scale, float angle, Color color, bool airborne)
        {
            if (sprite == null || color.a <= .004f) return null;
            if (used == tokens.Count)
            {
                var go = new GameObject("Hero mechanic token " + used); go.transform.SetParent(transform, false);
                tokens.Add(go.AddComponent<SpriteRenderer>());
            }
            var r = tokens[used++]; r.enabled = true; r.sprite = sprite; r.color = color; r.sortingOrder = airborne ? AirTokenOrder : GroundTokenOrder;
            spriteMaterial ??= r.sharedMaterial;
            if (glowLight == null && Resources.Load<Shader>("SoftLight") is Shader soft) glowLight = new Material(soft);
            Material want = sprite == MechanicInk.Glow && glowLight != null ? glowLight : spriteMaterial; // glows are light, like the lantern's
            if (r.sharedMaterial != want) { r.sharedMaterial = want; r.SetPropertyBlock(null); } // a shaped gauge's reveal stays with its own token
            float unit = Mathf.Max(.01f, Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y)); // procedural tokens are 1 unit; painted art may differ
            r.transform.position = at; r.transform.rotation = Quaternion.Euler(0, 0, angle); r.transform.localScale = new Vector3(scale.x / unit, scale.y / unit, 1);
            return r;
        }
        private void Token(InkToken kind, Vector2 at, float size, float angle, float alpha, bool airborne = true, int face = 0, InkPalette? palette = null, float squash = 1)
            => Place(MechanicInk.Token(kind, palette ?? ink, face), at, new Vector2(size / squash, size * squash), still ? 0 : angle, new Color(1, 1, 1, alpha * strength), airborne);
        private void DimToken(InkToken kind, Vector2 at, float size, float alpha)
            => Place(MechanicInk.Token(kind, ink), at, Vector2.one * size, 0, new Color(.42f, .42f, .5f, alpha * strength), false);
        private void Glow(Vector2 at, float diameter, Color color, float alpha, bool airborne = false)
        {
            if (plain) alpha *= .55f;
            if (diameter > .75f) return; // owner 23/10/2026: a wide glow disc read as a plastic film round the hero; sparks stay
            Place(MechanicInk.Glow, at, Vector2.one * diameter, 0, A(color, alpha * strength), airborne);
        }
        private InkStroke Brush(float width, float alpha, float seed) => InkStroke.Of(ink, width, alpha * strength, seed);
        /// <summary>A stroke painted as soft light (22/10/2026): halo, body and hot thread, dimmer with Reduced Flashes.</summary>
        private void Soft(InkCanvas canvas, List<Vector2> path, float head, float tail, float width, float alpha)
        {
            // painted as light, not as a wireframe (owner, 22/10/2026): a wide soft halo, a brighter body and a hot thread,
            // each fading smoothly to nothing at its sides and its ends; no outline anywhere
            if (alpha <= .01f) return;
            float a = alpha * strength * (calm ? .7f : 1);
            canvas.GlowRibbon(path, head, tail, width * 3.2f, A(ink.Glow, .32f * a));
            canvas.GlowRibbon(path, head, tail, width * 1.4f, A(Color.Lerp(ink.Body, ink.Light, .35f), .72f * a));
            canvas.GlowRibbon(path, head, tail, width * .5f, A(Color.Lerp(ink.Light, Color.white, .45f), .9f * a));
        }
        private static Material lightMaterial;
        /// <summary>A sprite of soft light added onto what is under it (ArcaneBrigade/SoftLight), stretched to width x length.</summary>
        private SpriteRenderer Light(Sprite sprite, Vector2 at, float width, float length, float angle, Color color)
        {
            var r = Place(sprite, at, Vector2.one, angle, color, true);
            if (r == null) return null;
            Vector2 b = sprite.bounds.size; r.transform.localScale = new Vector3(width / Mathf.Max(.001f, b.x), length / Mathf.Max(.001f, b.y), 1);
            if (lightMaterial == null && Resources.Load<Shader>("SoftLight") is Shader shader) lightMaterial = new Material(shader) { name = "Hero soft light", hideFlags = HideFlags.DontSave };
            if (lightMaterial != null) r.sharedMaterial = lightMaterial;
            return r;
        }
        /// <summary>1 the moment the hero transforms, fading to 0 over 0.35s: the activation flash (a third as bright with
        /// Reduced Flashes).</summary>
        private float Activation()
        {
            float age = t - sim.HeroAwakenStarted;
            return age < 0 || age > .35f ? 0 : (calm ? .3f : 1) * (1 - age / .35f);
        }
        private float Bloom => (calm ? .14f : plain ? .27f : .46f) * strength; // lifted ~35% (22/10/2026): strikes read on every floor

        public void Display(CombatSimulation simulation)
        {
            Ensure();
            ground.Clear(); air.Clear(); vial?.Clear(); groundInk?.Clear(); used = 0; VisibleCount = 0; ghost.enabled = false;
            if (dreamEchoes != null) foreach (var e in dreamEchoes) if (e != null) e.enabled = false;
            if (awakenBanner != null) awakenBanner.gameObject.SetActive(false);
            bool live = simulation != null && simulation.HeroIdentityEnabled && simulation.Champion != null && (simulation.Phase == RunPhase.Combat || simulation.Phase == RunPhase.Paused);
            if (live)
            {
                sim = simulation; t = simulation.Elapsed;
                style = HeroMechanicStyle.For(simulation.Champion.Definition.Id); ink = style.PaletteFor(simulation.HeroIdentityApproach);
                still = PresentationSettings.ReducedMotion; sparse = PresentationSettings.ReducedEffects || AnimationDetailBudget.Level >= 2;
                calm = PresentationSettings.ReducedFlashes; plain = PresentationSettings.HighReadability;
                strength = Mathf.Lerp(1, .72f, EffectDensity.CrowdPressure) * (sparse ? .85f : 1);
                sparse |= EffectDensity.CrowdPressure > .6f; // in a crowd keep the washes and rims, drop the flurries of tokens
                var cam = Camera.main; screenCap = cam != null && cam.orthographic ? Mathf.Max(1.2f, cam.orthographicSize * .45f) : 3;
                if (simulation.Champion.Alive)
                {
                    DrawState();
                    // owner (22/10/2026): when the hero walks behind a map prop, the gauge beside it fades like the hero does
                    float full = strength; strength *= Mathf.Lerp(1, HeroFocus.PropAlpha, HeroFocus.Covered);
                    if (simulation.HeroAwakenEnabled) DrawAwakenGauge(); else DrawGauge();
                    strength = full;
                    if (simulation.HeroAwakenEnabled) DrawAwakening();
                    Rims(); // after the gauge is placed: the rim outlines this frame's vessel
                    DrawCastCue();
                }
                foreach (var glyph in simulation.HeroIdentityGlyphs)
                {
                    if (VisibleCount >= EffectDensity.Limit(28)) break;
                    if (DrawGlyph(glyph)) VisibleCount++;
                }
            }
            ground.Commit(); air.Commit(); vial?.Commit(); groundInk?.Commit();
            for (int i = used; i < tokens.Count; i++) tokens[i].enabled = false;
        }

        // ------------------------------------------------------------------ gauge
        private int GaugeCount()
        {
            if (style.Gauge != HeroGauge.Pips) return 0;
            int n = Mathf.RoundToInt(sim.HeroIdentityRequired);
            return n >= 1 && n <= 8 ? n : 0;
        }
        /// <summary>Where the gauge stands this frame (read by checks): the bottom and top of its column, beside the hero.</summary>
        public Vector2 GaugeBottom { get; private set; }
        public Vector2 GaugeTop { get; private set; }
        /// <summary>The hero's gauge (18/10/2026): an upright inked vial standing just left of the hero and following them,
        /// never taller than the hero (85% of the body). Pip resources stack the hero's own tokens bottom-up inside it, unlit
        /// ones as dim silhouettes; continuous resources (distance, steam, debt) fill it with the hero's ink, the hero's token
        /// riding the surface. Full, it glows and its contents bob. Tier notches can be added on the same column later.</summary>
        private void DrawGauge()
        {
            UnitState hero = sim.Champion;
            float charge = sim.HeroIdentityCharge, required = Mathf.Max(.01f, sim.HeroIdentityRequired), fraction = Mathf.Clamp01(charge / required);
            string id = style.Id;
            if (id == "PatchworkHost" && sim.HeroIdentityLead == null) return;
            int lit = Mathf.FloorToInt(charge + .001f);
            if (lit > lastCharge) pipLitAt = t; lastCharge = lit;
            if (fraction >= 1 && gaugeFull < 0) gaugeFull = t; else if (fraction < 1) gaugeFull = -1;
            float ready = fraction >= 1 && !still ? .5f + .5f * Mathf.Sin((t - gaugeFull) * 6) : 0;

            float body = hero.Definition.BodyHeight, height = Mathf.Clamp(body * .85f, .45f, 1.1f), half = plain ? .12f : .105f;
            Vector2 bottom = DrawnFeet(hero) + new Vector2(-(body * .38f + .26f), .08f), top = bottom + Vector2.up * height;
            GaugeBottom = bottom; GaugeTop = top;
            Color shade = Color.Lerp(ink.Ink, new Color(.05f, .05f, .09f), .5f);
            // the vial: a dark glass wash, then its inked outline (rounded at both ends)
            vial.Band(bottom, top, half, A(shade, .55f * strength), A(shade, .35f * strength));
            int count = GaugeCount();
            if (count > 0)
            {
                InkToken pip = id == "GlassDice" ? InkToken.Coin : style.Token;
                InkPalette? pal = id == "GlassDice" ? GoldPalette : (InkPalette?)null;
                float step = height / count, size = Mathf.Min(plain ? .34f : .3f, step * 1.35f);
                for (int i = 0; i < count; i++)
                {
                    Vector2 at = bottom + Vector2.up * (step * (i + .5f)) + Vector2.up * ready * .02f;
                    if (id == "PetalCitadel") { PetalPip(i, at, size, i < lit); continue; }
                    if (i < lit)
                    {
                        float pop = i == lit - 1 && !still ? 1 + .45f * (1 - OutCubic((t - pipLitAt) / .25f)) : 1;
                        Token(pip, at, size * pop, (i % 2 == 0 ? -1 : 1) * 6, .95f, true, 0, pal);
                    }
                    else Place(MechanicInk.Token(pip, pal ?? ink), at, Vector2.one * size * .8f, 0, new Color(.55f, .55f, .64f, .7f * strength), true);
                }
            }
            else
            {
                // continuous: the hero's ink rises in the vial, its token riding the surface
                Vector2 surface = Vector2.Lerp(bottom, top, fraction);
                if (fraction > .01f)
                {
                    vial.Band(bottom, surface, half * .78f, A(ink.Body, .95f * strength), A(Color.Lerp(ink.Body, ink.Ink, .3f), .95f * strength));
                    vial.Stroke(InkCanvas.Line(bottom + new Vector2(-half * .35f, .03f), surface + new Vector2(-half * .35f, -.02f)), new InkStroke { Width = half * .3f, Head = 1, Tail = 0, Taper = 1.2f, Alpha = .55f * strength, Ink = ink.Light, Body = ink.Light, Light = ink.Light, NoLight = true });
                }
                Token(style.Token, surface + Vector2.up * (.02f + ready * .03f), (fraction >= 1 ? .34f : .28f) * (1 + ready * .12f), still ? 0 : Mathf.Sin(t * 2.4f) * 8, fraction > .01f ? .95f : .55f);
            }
            var outline = new List<Vector2>(40);
            for (int i = 0; i <= 8; i++) outline.Add(bottom + Dir(180 + i * 22.5f) * half);
            for (int i = 0; i <= 8; i++) outline.Add(top + Dir(i * 22.5f) * half);
            vial.Stroke(outline, new InkStroke { Width = .035f, Head = 1, Tail = 0, Taper = 1, Alpha = .95f * strength, Ink = ink.Ink, Body = ink.Ink, Light = ink.Ink, InkOnly = true }, true);
            if (id == "GlassDice")
                Token(InkToken.Die, top + Vector2.up * .16f, .24f * (1 + ready * .1f), still ? 0 : Mathf.Sin(t * 2) * 6, 1, true, sim.HeroIdentityNextFace);
            if (fraction >= 1 && !calm) Glow((bottom + top) * .5f, height * 1.25f, ink.Glow, .1f + ready * .08f, true);
        }
        private static readonly InkPalette GoldPalette = new InkPalette(new Color(.24f, .13f, .03f), new Color(.96f, .74f, .28f), new Color(1, .95f, .74f), new Color(1, .8f, .4f), new Color(.6f, .38f, .16f));
        private static readonly Color[] PetalColours = { new Color(.96f, .48f, .56f), new Color(.34f, .7f, .78f), new Color(.5f, .86f, .5f), new Color(.5f, .66f, 1f) };
        private void PetalPip(int petal, Vector2 at, float size, bool lit)
        {
            int growth = sim.HeroIdentityPetal(petal);
            var p = ink; p.Body = PetalColours[petal % 4];
            float grown = growth <= 0 ? .7f : 1 + growth * .12f;
            if (lit && growth > 0) Token(InkToken.Petal, at, size * grown, (petal - 1.5f) * 18, .95f, true, 0, p);
            else Place(MechanicInk.Token(InkToken.Petal, p), at, Vector2.one * size * grown * .85f, (petal - 1.5f) * 18, new Color(.45f, .45f, .52f, (growth > 0 ? .6f : .3f) * strength), true);
        }

        // ------------------------------------------------------------------ live state
        // unit positions are body centres: feet sit half a body below, heads half a body above
        private static Vector2 Head(UnitState u) => u.Position + Vector2.up * (u.Definition.BodyHeight * .5f + .2f);
        private static Vector2 Chest(UnitState u) => u.Position + Vector2.up * u.Definition.BodyHeight * .05f;
        private static Vector2 Feet(UnitState u) => u.Position - Vector2.up * u.Definition.BodyHeight * .5f;
        private float Bob(float phase) => still ? 0 : Mathf.Sin(t * 3.1f + phase) * .045f;

        private void DrawState()
        {
            string id = style.Id; var hero = sim.Champion;
            // Marked addresses / knots
            if (id == "Postmaster" || id == "LanternWeaver")
            {
                int n = sim.HeroIdentityMarkCount; UnitState previous = null;
                for (int i = 0; i < n; i++)
                {
                    var u = sim.HeroIdentityMarked(i); if (u == null || !u.Alive) continue;
                    float life = Mathf.Clamp01((sim.HeroIdentityMarkUntil(i) - t) / .5f);
                    if (id == "Postmaster")
                    {
                        Token(InkToken.Seal, Head(u) + Vector2.up * Bob(i), .3f, -8 + i * 9, .95f * life);
                    }
                    else
                    {
                        Token(InkToken.Knot, Chest(u), .3f, still ? 0 : t * 40 + i * 50, .95f * life);
                        if (previous != null) Thread(Chest(previous), Chest(u), .55f * life, i);
                        previous = u;
                    }
                    VisibleCount++;
                }
                if (id == "LanternWeaver" && n > 0)
                {
                    int approach = sim.HeroIdentityApproach;
                    if (approach == 0 && n >= 3 && previous != null && sim.HeroIdentityMarked(0).Alive) Thread(Chest(previous), Chest(sim.HeroIdentityMarked(0)), .32f, 7);
                    if (approach == 1 && sim.HeroIdentityMarked(0).Alive) Thread(Chest(hero), Chest(sim.HeroIdentityMarked(0)), .32f, 9);
                    if (approach == 2) for (int i = 0; i < n; i++) if (sim.HeroIdentityMarked(i).Alive) Thread(Chest(hero), Chest(sim.HeroIdentityMarked(i)), .18f, 11 + i);
                }
            }
            var bounty = sim.HeroIdentityBounty;
            if (id == "TreasureAcorn" && bounty != null)
            {
                Vector2 tag = Head(bounty) + Vector2.up * Bob(1);
                Token(InkToken.Coin, tag, .32f, still ? 0 : Mathf.Sin(t * 2.4f) * 10, 1);
                int stamps = Mathf.FloorToInt(sim.HeroIdentityCharge + .01f);
                for (int i = 0; i < 3; i++)
                {
                    Vector2 at = tag + new Vector2((i - 1) * .17f, -.24f);
                    if (i < stamps) Token(InkToken.Seal, at, .13f, 0, 1, true, 0, StampPalette); else Place(MechanicInk.Token(InkToken.Seal, StampPalette), at, Vector2.one * .11f, 0, new Color(.4f, .4f, .46f, .55f * strength), true);
                }
                VisibleCount++;
            }
            if (id == "MoonProwler" && bounty != null && sim.HeroIdentityCharge >= 5)
            {
                // the rear cone the simulation accepts (dot <= -.25 behind the fixed facing)
                Vector2 rear = -sim.HeroIdentityRearDirection; if (rear.sqrMagnitude < .01f) rear = Vector2.left;
                float a = Mathf.Atan2(rear.y, rear.x) * Mathf.Rad2Deg, half = 75.5f, radius = 1.05f;
                float breathe = still ? 1 : .8f + .2f * Mathf.Sin(t * 3);
                Vector2 apex = Feet(bounty);
                ground.Sector(apex, radius, a - half, a + half, A(ink.Glow, .04f), A(ink.Body, .3f * breathe * strength));
                Soft(ground, InkCanvas.Arc(apex, radius - .03f, radius - .03f, a - half, a + half), 1, 0, .06f, .55f);
                Token(InkToken.Moon, bounty.Position + rear * .7f + Vector2.up * (.35f + Bob(2)), .3f, a, .95f);
                VisibleCount++;
            }
            if (id == "DrowsySeer" && sim.HeroIdentityDreaming) Dream();
            if (id == "RootOracle") Roots();
            if (id == "MirrorKeeper" && sim.HeroIdentityStored > 0)
            {
                int n = sim.HeroIdentityStored;
                for (int i = 0; i < n; i++)
                {
                    float a = (still ? 90 : t * 70) + i * 360f / n; Vector2 at = Chest(hero) + new Vector2(Mathf.Cos(a * Mathf.Deg2Rad) * .62f, Mathf.Sin(a * Mathf.Deg2Rad) * .3f);
                    Token(InkToken.Shard, at, .24f, a + 90, .95f);
                }
                VisibleCount++;
            }
            if ((id == "CometRunner" || id == "CitrusMarshal") && sim.HeroIdentityRoute.Count >= 2) Route();
            if (id == "PatchworkHost" && sim.HeroIdentityLead != null && sim.HeroIdentityLead.Alive)
            {
                var lead = sim.HeroIdentityLead; float ready = Mathf.Clamp01(sim.HeroIdentityCharge / 4);
                Token(InkToken.Star, Head(lead) + Vector2.up * Bob(3), .22f + .08f * ready, still ? 0 : t * 30, .9f, true, 0, GoldPalette);
            }
            if (id == "HourglassStag" && sim.HeroIdentityBankAge >= 0)
            {
                float due = sim.HeroIdentityApproach == 0 ? 4 : sim.HeroIdentityApproach == 1 ? 3 : 5, k = Mathf.Clamp01(sim.HeroIdentityBankAge / due);
                // the debt's hourglass hangs over the stag's head and turns as it comes due (no clock ring round its feet)
                Token(InkToken.Sand, Head(hero) + Vector2.up * (.2f + Bob(1)), .22f + .06f * k, still ? 0 : 180 * k, .9f, false);
            }
            HeroFlair(); ChargeCrown(); AwakenBanter();
        }
        private static readonly InkPalette StampPalette = new InkPalette(new Color(.22f, .05f, .06f), new Color(.72f, .16f, .2f), new Color(1, .9f, .8f), new Color(1, .6f, .5f), new Color(.72f, .16f, .2f));
        private void Thread(Vector2 a, Vector2 b, float alpha, int seed)
        {
            var path = InkCanvas.Lob(a, b, -.12f, 14);
            Soft(air, path, 1, 0, .045f, alpha);
        }
        private void Dream()
        {
            Vector2 at = sim.HeroIdentityDreamPosition; float left = Mathf.Clamp01((sim.HeroIdentityDreamUntil - t) / 4f);
            float fadeIn = Mathf.Clamp01((4 - (sim.HeroIdentityDreamUntil - t)) / .35f), fade = Mathf.Min(fadeIn, Mathf.Clamp01((sim.HeroIdentityDreamUntil - t) / .3f));
            Vector2 floor = at - Vector2.up * sim.Champion.Definition.BodyHeight * .5f;
            Glow(floor, 1.2f, ink.Glow, .2f * fade);
            var body = sim.Champion.Definition.Body;
            if (body != null)
            {
                Rect inkBox = SpriteInkMetrics.BoundsOf(body); float scale = sim.Champion.Definition.BodyHeight / Mathf.Max(.01f, inkBox.height);
                ghost.enabled = true; ghost.sprite = body; ghost.transform.localScale = Vector3.one * scale;
                ghost.transform.position = (Vector3)(at + Vector2.up * (.12f + Bob(0) * 2)) - (Vector3)(inkBox.center * scale);
                // the dream copy shows only once the hero has walked away from it: overlapping the hero it read as a doubled, smeared body
                float apart = Mathf.Clamp01((Vector2.Distance(at, sim.Champion.Position) - .35f) / .5f);
                ghost.color = new Color(ink.Glow.r, ink.Glow.g, ink.Glow.b, .42f * fade * apart * EffectDensity.AlliedAlpha / .72f);
                ghost.enabled = apart > .01f;
            }
            if (!sparse && !still)
                for (int i = 0; i < 3; i++)
                {
                    float k = Mathf.Repeat(t * .55f + i / 3f, 1);
                    Token(InkToken.Star, at + new Vector2(.22f + i * .08f + Mathf.Sin(k * 6 + i) * .08f, sim.Champion.Definition.BodyHeight * .8f + k * .6f), .12f + .05f * (1 - k), k * 90, fade * Mathf.Sin(k * Mathf.PI));
                }
            VisibleCount++;
        }
        private static readonly InkPalette Bark = new InkPalette(new Color(.15f, .08f, .04f), new Color(.46f, .3f, .16f), new Color(.78f, .62f, .4f), new Color(.6f, .45f, .25f), new Color(.4f, .62f, .3f));
        /// <summary>A root, a fork or a thorn painted in bark ink on the floor (owner 23/10/2026: drawn as light they read as electricity).</summary>
        private void Root(List<Vector2> path, float width, float alpha, InkPalette pal, float seed) => groundInk.Stroke(path, InkStroke.Of(pal, width, Mathf.Clamp01(alpha) * strength, seed));
        private static readonly InkPalette TreeFire = new InkPalette(new Color(.22f, .06f, .04f), new Color(.96f, .46f, .22f), new Color(1f, .9f, .6f), new Color(1f, .62f, .3f), new Color(.62f, .86f, .4f));
        private void Roots()
        {
            // owner 23/10/2026: gnarled, thorny roots that fork, not three straight lines; while the Living Tree walks they burn
            // amber and their thorns grow long
            var hero = sim.Champion; float charge = Mathf.Clamp01(sim.HeroIdentityCharge / 3); int next = sim.HeroIdentityRootTurn;
            bool grown = sim.HeroAwakening; InkPalette was = ink; if (grown) ink = TreeFire; InkPalette wood = grown ? TreeFire : Bark;
            for (int i = 0; i < 3; i++)
            {
                float heading = 90 + i * 120; Vector2 way = Dir(heading);
                Vector2 at = hero.Position + way * 1.8f, from = hero.Position + way * .38f;
                bool turn = i == next; float k = grown ? 1 : turn ? charge : 0;
                if (!grown && turn && k > .7f && !still) at += Dir(t * 900 + i * 50) * .035f * k; // the root about to strike quivers
                float pulse = still ? 1 : grown ? .85f + .15f * Mathf.Sin(t * 5 + i * 2) : 1, alpha = (.8f + .2f * k) * pulse;
                Root(InkCanvas.Line(from, at, .09f, 5, false, i * 2.1f), .13f + .03f * k + (grown ? .04f : 0), alpha, wood, i);
                if (grown && !calm) Glow(at, .6f, TreeFire.Glow, .35f * pulse, false); // its tip smoulders
                // two forks bending away from the main root
                for (int b = 0; b < 2; b++)
                {
                    Vector2 fork = Vector2.Lerp(from, at, .38f + b * .27f), tip = fork + Dir(heading + (b == 0 ? 38 : -42)) * (grown ? .62f : .48f - b * .1f);
                    Root(InkCanvas.Line(fork, tip, .05f, 9, false, i * 3 + b), .075f + (grown ? .02f : 0), alpha * .95f, wood, i * 3 + b);
                    Root(InkCanvas.Line(tip, tip + Dir(heading + (b == 0 ? 75 : -80)) * .16f), .045f, alpha, wood, i + b * 7); // its barbed tip
                }
                // thorns along the root, alternating sides, raked back toward the hero
                float barb = grown ? .26f : .15f;
                for (int j = 0; j < 5; j++)
                {
                    Vector2 p = Vector2.Lerp(from, at, .18f + j * .16f); float side = j % 2 == 0 ? 1 : -1;
                    Root(InkCanvas.Line(p, p + Dir(heading + side * 125) * barb * (1 - j * .08f)), .045f, alpha, wood, j + i * 5);
                }
                Token(grown ? InkToken.Thorn : InkToken.Leaf, at + Vector2.up * .06f, .26f + .08f * k * (still ? 0 : 1), heading - 90 + (still ? 0 : Mathf.Sin(t * 2 + i) * 8), .7f + .3f * k, false);
                if (grown && !sparse) Token(InkToken.Petal, at + way * .1f + Vector2.up * (.2f + Bob(i) * 2), .16f, t * 40 + i * 60, .8f, true);
            }
            ink = was;
            VisibleCount++;
        }
        private void Route()
        {
            var route = sim.HeroIdentityRoute; float fraction = Mathf.Clamp01(sim.HeroIdentityCharge / Mathf.Max(.01f, sim.HeroIdentityRequired));
            for (int i = 1; i < route.Count; i++)
            {
                Vector2 a = route[i - 1], b = route[i]; float length = Vector2.Distance(a, b); if (length < .05f) continue;
                if (sim.HeroIdentityApproach == 1)
                    for (int sign = -1; sign <= 1; sign += 2) Soft(ground, InkCanvas.Line(a + Vector2.right * .4f * sign, b + Vector2.right * .4f * sign), 1, 0, .035f, .18f + .3f * fraction);
                else Soft(ground, InkCanvas.Line(a, b), 1, 0, .04f, .18f + .32f * fraction);
                if (sparse) continue;
                int dots = Mathf.Clamp(Mathf.FloorToInt(length / .45f), 1, 10);
                for (int d = 0; d < dots; d++)
                {
                    float k = (d + .5f) / dots; float flicker = still ? 1 : .75f + .25f * Mathf.Sin(t * 9 + d * 1.7f + i);
                    Token(style.Token, Vector2.Lerp(a, b, k), .1f + .05f * fraction, d * 40, (.25f + .5f * fraction) * flicker, false);
                }
            }
        }

        // ------------------------------------------------------------------ cast cue
        private void DrawCastCue()
        {
            if (sim.HeroIdentityLastCast > lastCast + .001f) { lastCast = sim.HeroIdentityLastCast; castShown = lastCast; }
            float k = (t - castShown) / .5f; if (k < 0 || k >= 1) return;
            Vector2 feet = Feet(sim.Champion); float e = OutCubic(k);
            // the cast is the hero's own tokens springing up from it; no ring opening round its feet
            if (sparse) return;
            for (int i = 0; i < 3; i++)
            {
                float a = 60 + i * 30; Vector2 rise = still ? new Vector2((i - 1) * .3f, .9f) : new Vector2((i - 1) * .32f * e, .4f + .75f * e);
                Token(style.Token, feet + rise + Vector2.up * sim.Champion.Definition.BodyHeight * .3f, .24f * (1 - k * .35f), (i - 1) * 25 + k * 40, 1 - InQuad(k));
            }
        }

        // ------------------------------------------------------------------ glyphs
        private bool DrawGlyph(HeroIdentityGlyph g)
        {
            switch (g.Role)
            {
                case HeroGlyphRole.Mark: case HeroGlyphRole.Ambient: return false; // drawn from live state
                case HeroGlyphRole.Strike:
                    // a transformation that paints its own strike (beam, clock hand, scoop, depth charge) shows only that painting
                    if (g.Painted) return false;
                    if (g.Shape == MorphGeometry.Line) LineStrike(g);
                    else if (g.Shape == MorphGeometry.Crescent) CrescentStrike(g);
                    else if (g.Shape == MorphGeometry.Ring) RingStrike(g);
                    else DiscStrike(g, g.Shape == MorphGeometry.Seal);
                    return true;
                case HeroGlyphRole.Link: Link(g); return true;
                case HeroGlyphRole.Lob: Lob(g); return true;
                case HeroGlyphRole.Catch: Catch(g); return true;
                case HeroGlyphRole.Poise: Poise(g); return true;
                case HeroGlyphRole.Organ: Organ(g); return true;
                default: Emblem(g); return true;
            }
        }
        private void Phases(HeroIdentityGlyph g, out bool pre, out float q, out float p, out float fade)
        {
            float anticipation = g.At - g.Created;
            pre = t < g.At;
            q = anticipation > .001f ? Mathf.Clamp01((t - g.Created) / anticipation) : 1;
            p = Mathf.Clamp01((t - g.At) / Mathf.Max(.05f, g.Until - g.At));
            fade = 1 - Smooth(.55f, 1, p);
        }
        private float Seed(HeroIdentityGlyph g) => (g.Seed * 37) % 360;

        private void DiscStrike(HeroIdentityGlyph g, bool seal)
        {
            Phases(g, out bool pre, out float q, out float p, out float fade);
            Vector2 c = g.Position; float r = Fit(Mathf.Max(.2f, g.Radius)), s = Seed(g);
            if (pre)
            {
                // the spot warms from its middle out to nothing at the edge: no ring, no hard rim (17/10/2026)
                if (g.Finale) FinaleDrop(c, q);
                ground.Sector(c, r, 0, 360, A(ink.Body, (.08f + .14f * q) * strength), A(ink.Body, 0));
                if (!sparse)
                    for (int i = 0; i < 4; i++)
                    {
                        float a = s + i * 90 + (still ? 0 : q * 40); float d = still ? r * .6f : Mathf.Lerp(r * .9f, r * .3f, OutCubic(q));
                        Token(style.Token, c + Dir(a) * d, .15f * (.6f + .4f * q), a, .8f * q);
                    }
                return;
            }
            float flash = calm ? 0 : .14f * (1 - Smooth(0, .2f, p));
            ground.Sector(c, r, 0, 360, A(ink.Glow, (.32f + flash) * fade * strength), A(ink.Body, 0));
            if (!calm) Glow(c, r * 2, ink.Glow, .22f * (1 - p));
            int n = sparse ? 3 : 6;
            for (int i = 0; i < n; i++)
            {
                float a = s + i * 360f / n + (i % 2) * 17, size = Mathf.Min(.22f * (1 + .5f * Mathf.Pow(1 - p, 3)), r * .5f);
                if (style.Id == "SoapSkater") size *= .6f; // owner 23/10/2026: the turn bubble's foam covered the skater
                float d = Mathf.Min(r - size * .55f, r * .82f * (still ? .7f : OutCubic(p)));
                Token(style.Token, c + Dir(a) * Mathf.Max(0, d), size, a + p * 120, fade);
            }
            if (seal || style.Id == "SupperMortar")
            {
                float stamp = still ? 1 : OutBack(Mathf.Min(1, p / .28f)), size = Mathf.Min(r * 1.25f, 1.15f) * Mathf.Lerp(1.5f, 1, stamp);
                float squash = still ? 1 : 1 - .16f * Mathf.Sin(Mathf.Clamp01(p / .4f) * Mathf.PI);
                Token(seal ? InkToken.Seal : style.Token, c + Vector2.up * .05f, size * (seal ? 1 : .55f), -6, Mathf.Clamp01(p / .12f) * fade, true, 0, null, squash);
            }
            if (!seal) Signature(g, c, g.Direction, p, fade); // the hero's own impact at the spot
        }

        private void LineStrike(HeroIdentityGlyph g)
        {
            Phases(g, out bool pre, out float q, out float p, out float fade);
            Vector2 a = g.Position, b = g.End; float half = Mathf.Max(.1f, g.Width), length = Vector2.Distance(a, b);
            if (length < .05f) return;
            Vector2 axis = (b - a) / length, normal = new Vector2(-axis.y, axis.x);
            if (pre)
            {
                ground.Band(a, b, half, A(ink.Body, .02f * strength), A(ink.Body, (.05f + .1f * q) * strength));
                float reveal = still ? 1 : OutCubic(q);
                Soft(ground, InkCanvas.Line(a, Vector2.Lerp(a, b, reveal)), 1, 0, .05f, .3f + .35f * q);
                Gather(b, q);
                return;
            }
            ground.Band(a, b, half, A(ink.Glow, .16f * fade * strength), A(ink.Body, .42f * fade * strength));
            float head = still ? 1 : OutCubic(Mathf.Min(1, p / .3f)), tail = still ? 0 : InQuad(Mathf.Max(0, (p - .3f) / .7f));
            // a thread (China Marionette's needle) flies as a short stitch, never as a ruled line from end to end (owner 23/10/2026)
            if (style.Brush == InkBrush.Thread) tail = Mathf.Max(tail, head - .28f);
            float seed = Seed(g) * .1f;
            List<Vector2> path; float width;
            switch (style.Brush)
            {
                case InkBrush.Bolt: path = InkCanvas.Line(a, b, half * .48f, 0, true, g.Seed); width = .17f; break;
                case InkBrush.Wave: path = InkCanvas.Line(a, b, half * .3f, 9, false, seed); width = .3f; break;
                case InkBrush.Thread: path = InkCanvas.Line(a, b, half * .12f, 5, false, seed); width = .2f; break;
                case InkBrush.Silk: path = InkCanvas.Line(a, b, half * .22f, 4.5f, false, seed); width = .38f; break;
                case InkBrush.Flame: path = InkCanvas.Line(a, b, half * .16f, 12, false, seed); width = .36f; break;
                case InkBrush.Ink: path = InkCanvas.Line(a, b); width = .3f; break;
                default: path = InkCanvas.Line(a, b); width = .34f; break;
            }
            Soft(air, path, head, tail, width * .75f, fade);
            if (style.Brush == InkBrush.Bolt && !calm && !still)
            {
                var flicker = InkCanvas.Line(a, b, half * .4f, 0, true, g.Seed + 1 + Mathf.FloorToInt(t * 20));
                Soft(air, flicker, head, tail, .05f, .6f * fade);
            }
            if (!sparse)
                for (int i = 0; i < 4; i++)
                {
                    float f = .18f + i * .24f; if (head < f) continue;
                    float age = Mathf.Clamp01((head - f) * 2 + p * .6f);
                    Vector2 at = a + (b - a) * f + normal * (((i % 2) * 2 - 1) * half * .6f * (still ? .5f : age));
                    Token(style.Token, at, .17f * (1 - .4f * age), i * 70 + p * 160, (1 - age) * fade);
                }
            float impact = Mathf.Clamp01((p - .22f) / .5f);
            if (impact > 0 && impact < 1) Token(style.Token, b, .34f * (still ? 1 : OutBack(Mathf.Min(1, impact * 3))), Seed(g), (1 - impact) * fade);
            if (style.Brush == InkBrush.Thread) { Token(style.Token, a, .2f, 0, fade * .9f); Token(style.Token, b, .24f, 30, fade); }
            Signature(g, b, axis, Mathf.Clamp01((p - .15f) / .85f), fade); // the hero's own impact where the line lands
        }

        private void CrescentStrike(HeroIdentityGlyph g)
        {
            Phases(g, out bool pre, out float q, out float p, out float fade);
            Vector2 c = g.Position; float r = Fit(Mathf.Max(.3f, g.Radius)); Vector2 d = g.Direction.sqrMagnitude > .01f ? g.Direction : Vector2.right;
            float heading = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg, sweep = g.Seed % 2 == 0 ? 1 : -1;
            if (pre)
            {
                ground.Sector(c, r, heading - CrescentHalf, heading + CrescentHalf, A(ink.Glow, .01f), A(ink.Body, (.05f + .11f * q) * strength));
                float reveal = still ? 1 : OutCubic(q);
                Soft(ground, InkCanvas.Arc(c, r - .05f, r - .05f, heading - CrescentHalf * reveal, heading + CrescentHalf * reveal), 1, 0, .05f, .3f + .3f * q);
                Gather(c + d * r * .62f, q);
                return;
            }
            ground.Sector(c, r, heading - CrescentHalf, heading + CrescentHalf, A(ink.Glow, .1f * fade * strength), A(ink.Body, .38f * fade * strength));
            Soft(ground, InkCanvas.Arc(c, r - .05f, r - .05f, heading - CrescentHalf, heading + CrescentHalf), 1, 0, .06f, .6f * fade);
            float head = still ? 1 : OutCubic(Mathf.Min(1, p / .32f)), tail = still ? 0 : InQuad(Mathf.Max(0, (p - .25f) / .75f));
            // the swing passes through the middle of the area; the inked rim and wash mark its true edge
            float width = Mathf.Min(Mathf.Clamp(r * .26f, .16f, .48f), r * .3f), radius = r * .62f;
            var arc = InkCanvas.Arc(c, radius, radius, heading - sweep * CrescentHalf * .96f, heading + sweep * CrescentHalf * .96f);
            Soft(air, arc, head, tail, width * .75f, fade);
            Vector2 tip = InkCanvas.Sample(arc, head);
            if (!sparse)
            {
                float h2 = still ? 1 : OutCubic(Mathf.Clamp01((p - .08f) / .32f));
                var inner = InkCanvas.Arc(c, radius * .55f, radius * .55f, heading - sweep * CrescentHalf * .8f, heading + sweep * CrescentHalf * .8f);
                Soft(air, inner, h2, tail, width * .4f, .7f * fade);
                for (int i = 0; i < 3; i++)
                {
                    float f = Mathf.Clamp01(head - i * .14f); if (f <= 0) continue;
                    var at = InkCanvas.Arc(c, radius, radius, heading - sweep * CrescentHalf * .96f, heading + sweep * CrescentHalf * .96f);
                    Vector2 pos = InkCanvas.Sample(at, f); Vector2 outward = (pos - c).normalized;
                    Token(style.Token, pos - outward * (.1f + i * .06f), .18f * (1 - i * .2f), heading + i * 40 + p * 90, fade * (1 - i * .25f));
                }
            }
            if (head < 1 && !calm) Glow(tip, Mathf.Min(.6f, r * .5f), ink.Glow, .3f * fade, true);
            Signature(g, c + d * r * .62f, d, Mathf.Clamp01((p - .2f) / .8f), fade); // the hero's own impact in the swing's middle
        }

        private void RingStrike(HeroIdentityGlyph g)
        {
            // the true hit band is shown by a procession of the hero's tokens racing round it, not by a drawn ring (17/10/2026)
            Phases(g, out bool pre, out float q, out float p, out float fade);
            Vector2 c = g.Position; float r = Fit(Mathf.Max(.3f, g.Radius)), s = Seed(g);
            int n = sparse ? 6 : 12;
            if (pre)
            {
                for (int i = 0; i < n; i++)
                {
                    float f = i / (float)n; if (!still && f > OutCubic(q)) continue;
                    DimToken(style.Token, c + Dir(s + 360 * f) * r, .12f, .55f * q);
                }
                return;
            }
            float head = still ? 1 : OutCubic(Mathf.Min(1, p / .4f));
            for (int i = 0; i < n; i++)
            {
                float f = i / (float)n; if (f > head) continue;
                float age = Mathf.Clamp01((head - f) * 3);
                Vector2 pos = c + Dir(s + 360 * f) * r;
                Token(style.Token, pos, .22f * (1 - .35f * age), s + f * 360 + p * 120, fade * (1 - .5f * age));
                if (!sparse && i % 3 == 0) Glow(pos, .5f, ink.Glow, .2f * fade * (1 - age));
            }
        }

        private void Link(HeroIdentityGlyph g)
        {
            float p = Mathf.Clamp01((t - g.Created) / Mathf.Max(.05f, g.Until - g.Created));
            Vector2 a = g.Position + Vector2.up * .35f, b = g.End + Vector2.up * .35f; if ((b - a).sqrMagnitude < .01f) return;
            var path = InkCanvas.Lob(a, b, Mathf.Min(.5f, Vector2.Distance(a, b) * .12f), 16);
            float head = still ? 1 : OutCubic(Mathf.Min(1, p / .45f)), tail = still ? 0 : InQuad(Mathf.Max(0, (p - .4f) / .6f));
            Soft(air, path, head, tail, style.Brush == InkBrush.Silk ? .09f : .06f, .85f * (1 - Smooth(.7f, 1, p)));
            Token(InkToken.Glint, InkCanvas.Sample(InkCanvas.Lob(a, b, Mathf.Min(.5f, Vector2.Distance(a, b) * .12f), 16), head), .26f, still ? 0 : p * 180, 1 - Smooth(.75f, 1, p), true, 0, GlintPalette(ink));
        }
        /// <summary>The painted body of an identity bolt in flight (envelope, note, shard...); null for weapon-shaped echoes.</summary>
        public static Sprite Missile(ProjectileState state, Sprite generic, float time, out float turn, out float size)
        {
            turn = 0; size = 0;
            if (state == null || state.Faction == Faction.Enemy || state.Sprite != generic || state.Cause.ItemId == null || !state.Cause.ItemId.StartsWith("hero-identity-", System.StringComparison.Ordinal)) return null;
            var style = HeroMechanicStyle.For(state.Cause.ItemId.Substring(14)); InkToken kind = style.Missile;
            bool spin = kind == InkToken.Die || kind == InkToken.Coin || kind == InkToken.Seal || kind == InkToken.Star || kind == InkToken.Spark || kind == InkToken.Button || kind == InkToken.Citrus;
            turn = spin ? (PresentationSettings.ReducedMotion ? 0 : time * 520 + state.Id * 47) : kind == InkToken.Drop || kind == InkToken.Ember ? 90 : -90;
            size = (kind == InkToken.Envelope || kind == InkToken.Page ? .4f : .36f) * (PresentationSettings.HighReadability ? 1.1f : 1);
            return MechanicInk.Token(kind, style.Palette, kind == InkToken.Die ? 1 + state.Id % 6 : 0);
        }
        private static InkPalette GlintPalette(InkPalette p) => new InkPalette(p.Ink, p.Light, Color.white, p.Glow, p.Accent);

        private void Lob(HeroIdentityGlyph g)
        {
            float p = Mathf.Clamp01((t - g.Created) / Mathf.Max(.05f, g.Until - g.Created));
            Vector2 a = g.Position + Vector2.up * .3f, b = g.End; float dist = Vector2.Distance(a, b), height = Mathf.Clamp(dist * .35f + .6f, .7f, 2f);
            var path = InkCanvas.Lob(a, b, height, 20);
            if (!sparse)
            {
                Soft(air, path, p, Mathf.Max(0, p - .3f), .05f, .5f);
            }
            Vector2 shell = InkCanvas.Sample(InkCanvas.Lob(a, b, height, 20), p);
            Vector2 shadow = Vector2.Lerp(a - Vector2.up * .3f, b, p);
            Glow(shadow, .45f * (.6f + .4f * p), ink.Ink, .4f);
            float squash = still ? 1 : 1 + .12f * Mathf.Sin(p * Mathf.PI * 3);
            Token(style.Token, shell, .36f, still ? 0 : p * 420, 1, true, 0, null, squash);
        }

        /// <summary>Lone Jelly's organs land on the enemy itself (owner 23/10/2026), each with its own motion: the bite's jaws
        /// open over it and snap shut, the gland's drop arcs over and bursts into droplets, the threadwork's needle stitches
        /// three passes across it and pulls them tight.</summary>
        private void Organ(HeroIdentityGlyph g)
        {
            Phases(g, out bool pre, out float q, out float p, out float fade);
            Vector2 c = g.Position + Vector2.up * .15f, from = g.End + Vector2.up * .35f;
            float s = Seed(g); int kind = Mathf.Clamp(sim.HeroIdentityApproach, 0, 2);
            if (kind == 0)
            {
                // the jaws: wide open while it lunges, shut on the hit, then a spray of droplets
                float open = pre ? Mathf.Lerp(.55f, .42f, still ? 1 : OutCubic(q)) : Mathf.Lerp(.42f, .06f, still ? 1 : OutBack(Mathf.Min(1, p / .25f)));
                float a = pre ? q : 1 - Smooth(.6f, 1, p);
                Token(InkToken.Fang, c + Vector2.up * open, .34f, 180, a, true);
                Token(InkToken.Fang, c - Vector2.up * open, .34f, 0, a, true);
                if (!pre && !sparse) for (int i = 0; i < 4; i++) { float d = still ? .3f : .15f + .45f * OutCubic(p); Token(InkToken.Drop, c + Dir(s + i * 90 + 45) * d, .12f * (1 - p), s + i * 90, (1 - p) * .9f, true); }
            }
            else if (kind == 1)
            {
                // the gland's drop flies over in an arc, then bursts
                if (pre) { float h = still ? 1 : q; Vector2 arc = Vector2.Lerp(from, c, h) + Vector2.up * Mathf.Sin(h * Mathf.PI) * .7f; Token(InkToken.Drop, arc, .28f, 180, 1, true); return; }
                Token(InkToken.Drop, c, .3f * (1 - .5f * p) * (still ? 1 : 1 + .4f * Mathf.Sin(Mathf.Min(1, p / .2f) * Mathf.PI)), 180, 1 - Smooth(.2f, .5f, p), true);
                int n = sparse ? 4 : 7;
                for (int i = 0; i < n; i++) { float a = s + i * 360f / n, d = still ? .4f : .1f + .6f * OutCubic(p); Token(InkToken.Drop, c + Dir(a) * d + Vector2.up * Mathf.Sin(p * Mathf.PI) * .15f, .13f * (1 - .5f * p), a + 90, fade, true); }
                Glow(c, .6f, ink.Light, .45f * (1 - p), true);
            }
            else
            {
                // the needle crosses the enemy three times, the thread following it, then tightens with a knot
                float run = pre ? (still ? 0 : q * .35f) : .35f + .65f * (still ? 1 : Mathf.Min(1, p / .55f));
                Vector2 n0 = Dir(s % 60 + 20) * .42f, n1 = new Vector2(-n0.y, n0.x) * .5f;
                Vector2[] stitch = { c - n0 + n1, c + n0 + n1 * .2f, c - n0 - n1 * .2f, c + n0 - n1 };
                float tight = pre ? 1 : 1 - .35f * Smooth(.55f, .8f, p);
                for (int i = 0; i < 3; i++)
                {
                    float part = Mathf.Clamp01(run * 3 - i); if (part <= 0) break;
                    Vector2 a = c + (stitch[i] - c) * tight, b = c + (stitch[i + 1] - c) * tight;
                    Soft(air, InkCanvas.Line(a, Vector2.Lerp(a, b, part), .03f, 5, false, s + i), 1, 0, .045f, fade);
                    if (part < 1) Token(InkToken.Shard, Vector2.Lerp(a, b, part), .16f, Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg, fade, true);
                }
                if (!pre && p > .55f) Token(InkToken.Knot, c, .22f, 0, fade, true);
            }
        }
        private void Catch(HeroIdentityGlyph g)
        {
            float p = Mathf.Clamp01((t - g.Created) / Mathf.Max(.05f, g.Until - g.Created)); Vector2 c = g.Position; float r = Mathf.Max(.2f, g.Radius);
            float close = still ? .6f : 1.25f - .8f * OutCubic(p);
            // the catch: four glints closing in on the spot instead of a shrinking ring (17/10/2026)
            for (int i = 0; i < 4; i++) Token(InkToken.Glint, c + Dir(i * 90 + 45 + (still ? 0 : p * 60)) * r * close, .16f, p * 90, (1 - p) * .9f, true, 0, GlintPalette(ink));
            Token(InkToken.Glint, c, .34f * (1 - p) + .1f, p * 90, 1 - InQuad(p), true, 0, GlintPalette(ink));
            if ((g.End - g.Position).sqrMagnitude > .05f)
            {
                Vector2 to = g.End + Vector2.up * .45f; Vector2 at = Vector2.Lerp(c, to, still ? 1 : OutCubic(p));
                Token(style.Token, at, .24f, still ? 0 : p * 300, 1 - Smooth(.8f, 1, p));
            }
        }

        private void Poise(HeroIdentityGlyph g)
        {
            float p = Mathf.Clamp01((t - g.Created) / Mathf.Max(.05f, g.Until - g.Created)); Vector2 c = sim.Champion.Position + Vector2.up * .35f;
            float head = still ? 1 : OutCubic(p), from = 200;
            Soft(air, InkCanvas.Arc(c, .58f, .36f, from, from + 300 * head), 1, 0, .06f, 1 - Smooth(.6f, 1, p));
            Token(style.Token, c + Vector2.up * (.6f + (still ? 0 : .3f * OutCubic(p))), .22f, 0, 1 - InQuad(p));
        }

        private void Emblem(HeroIdentityGlyph g)
        {
            float p = Mathf.Clamp01((t - g.Created) / Mathf.Max(.05f, g.Until - g.Created));
            float size = Mathf.Clamp(g.Radius * 1.4f, .3f, 1.1f) * (still ? 1 : OutBack(Mathf.Min(1, p / .3f)));
            if (!calm) Glow(g.Position, size * 1.5f, ink.Glow, .34f * (1 - p), true);
            Token(style.Token, g.Position + Vector2.up * (.3f + (still ? 0 : .35f * p)), size, still ? 0 : Mathf.Sin(p * 6) * 12, 1 - Smooth(.6f, 1, p), true, style.Token == InkToken.Die ? 6 : 0);
        }
    }
}
