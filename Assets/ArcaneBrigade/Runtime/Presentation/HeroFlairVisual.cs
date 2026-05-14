using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The hero's life between and around its skills (owner 23/10/2026): each hero shows what it is gathering while it
    /// waits (a lamp flame that grows, a die showing its next face, notes, steam, pages...), a row of its tokens over its
    /// head counts the steps to the next gauge third and pops as each one lights, a strike gathers its tokens before it
    /// lands, the transformation wears a living rim (hero, weapon, empowered friends and the gauge) that flickers as it runs
    /// out and ends in a finale, and a friend says something as it begins.</summary>
    public sealed partial class HeroIdentityVisual
    {
        private int crownLit, crownTier; private float crownLitAt = -10, crownTierAt = -10;
        private float banterFor = -100; private int banterLast = -1;
        private SpriteRenderer gaugeArt;

        // ------------------------------------------------------------------ a strike gathers before it lands
        /// <summary>The hero's tokens spiralling in to where a strike is about to land (q: 0..1 of its anticipation).</summary>
        private void Gather(Vector2 at, float q)
        {
            if (sparse || q <= 0) return;
            float e = still ? .6f : OutCubic(q);
            for (int i = 0; i < 4; i++)
            {
                float a = i * 90 + 45 + (still ? 0 : (1 - e) * 140);
                Token(style.Token, at + Dir(a) * Mathf.Lerp(.9f, .08f, e), .13f + .07f * e, a, .85f * Mathf.Clamp01(q * 2), true);
            }
        }
        /// <summary>The finale's sign falling from the sky onto its spot during its anticipation.</summary>
        private void FinaleDrop(Vector2 at, float q)
        {
            FinaleArrive(at, q); // each hero's own (HeroFinaleVisual)
        }

        // ------------------------------------------------------------------ the count over the head
        private void ChargeCrown()
        {
            int tier = sim.HeroAwakenTier;
            if (tier > crownTier) crownTierAt = t;
            crownTier = tier;
            float k = (t - crownTierAt) / .45f;
            if (k >= 0 && k < 1 && !still) { sigScale = 1; sigReach = 1; Spray(style.Token, GaugeTop, 5, .55f, .17f, k, 1, tier * 40); } // a gauge third lights: it bursts
            // (the row of counting tokens over the head is gone: one tell per hero, Docs/Hero_Effect_Suggestions.md)
            if (sim.HeroAwakening || sim.HeroAwakenPrimed || true) return;
#pragma warning disable CS0162
            int n = Mathf.RoundToInt(sim.HeroIdentityRequired); if (n < 2 || n > 8) return;
            int lit = Mathf.Clamp(Mathf.FloorToInt(sim.HeroIdentityCharge + .001f), 0, n);
            if (lit > crownLit) crownLitAt = t;
            crownLit = lit;
            if (lit == 0) return;
            UnitState hero = sim.Champion;
            Vector2 over = DrawnAt(hero) + Vector2.up * (hero.Definition.BodyHeight * .5f + .3f);
            for (int i = 0; i < n; i++)
            {
                float x = i - (n - 1) * .5f; Vector2 p = over + new Vector2(x * .17f, -Mathf.Abs(x) * .03f);
                if (i < lit)
                {
                    float pop = i == lit - 1 && !still ? 1 + .8f * (1 - Mathf.Clamp01((t - crownLitAt) / .25f)) : 1;
                    Token(style.Token, p, .13f * pop, 0, .95f, true);
                }
                else DimToken(style.Token, p, .1f, .45f);
            }
#pragma warning restore CS0162
        }

        // ------------------------------------------------------------------ the living rim
        private float Breathe(bool primed)
        {
            if (still) return .85f;
            if (primed) return Mathf.Repeat(t * 3, 1) < .5f ? 1 : .4f; // a full gauge flashes
            float b = .62f + .38f * Mathf.Sin(t * 5.2f);
            float left = sim.HeroAwakenRemaining;
            if (left < .2f) b *= Mathf.Repeat(t * Mathf.Lerp(11, 4, left / .2f), 1) < .5f ? 1 : .25f; // running out: faster and faster
            return b;
        }
        private void Rims()
        {
            UnitState hero = sim.Champion; bool awake = sim.HeroAwakening, primed = sim.HeroAwakenPrimed;
            if (!awake && !primed) return;
            float breathe = Breathe(primed);
            Color c = Color.Lerp(ink.Glow, ink.Light, .25f);
            UnitVisual view = UnitVisual.Live(hero.Id);
            if (view != null)
            {
                float body = hero.Definition.BodyHeight, feet = DrawnAt(hero).y - body * .5f;
                float sweep = still || primed ? float.NaN : feet - .3f + Mathf.Repeat(t * 1.1f, 1.7f) * (body + .6f);
                bool storm = awake && sim.HeroKitInfo != null && sim.HeroKitInfo.Action == KitAction.Thunder;
                SkillRim.For(view.BodyRenderer)?.Show(A(c, .95f * breathe * strength), .034f, sweep, .28f, storm ? 1 : 0);
                SpriteRenderer weapon = view.WeaponRenderer;
                if (awake && weapon != null && weapon.enabled && weapon.sprite != null)
                {
                    float w = Mathf.Repeat(t, 2.2f); // a flash runs over the weapon every couple of seconds
                    if (w < .7f || still) SkillRim.For(weapon)?.Show(A(c, .85f * (still ? .6f : Mathf.Sin(w / .7f * Mathf.PI)) * strength), .026f, float.NaN);
                }
            }
            if (awake && gaugeArt != null) SkillRim.For(gaugeArt)?.Show(A(c, .9f * breathe * strength), .03f, float.NaN);
            else if (awake && !sparse)
                for (int i = 0; i < 3; i++) { float a = (still ? 0 : t * 160) + i * 120; Token(InkToken.Glint, (GaugeBottom + GaugeTop) * .5f + new Vector2(Mathf.Cos(a * Mathf.Deg2Rad) * .2f, Mathf.Sin(a * Mathf.Deg2Rad) * (GaugeTop.y - GaugeBottom.y) * .55f), .12f, a, breathe, true, 0, GlintPalette(ink)); }
            if (!awake) return;
            // the friends this transformation empowers wear a fainter rim of the same light
            HeroKit kit = sim.HeroKitInfo; if (kit == null) return;
            foreach (UnitState f in sim.Units)
            {
                if (!f.Alive || f.Faction != Faction.Companion) continue;
                bool lit = kit.Action switch
                {
                    KitAction.Lighthouse => (f.Position - hero.Position).sqrMagnitude <= 25,
                    KitAction.Parade or KitAction.Symphony => true,
                    KitAction.KiteFleet => Contains(sim.HeroKiteFleet, f),
                    KitAction.Showtime => f == sim.HeroIdentityLead,
                    KitAction.Spring => f.Definition.Id == "GrandSprout" || f.Definition.Id == "LittleSprout",
                    KitAction.TimeGarden => (f.Position - sim.HeroAwakenPlot).sqrMagnitude <= 9,
                    _ => false
                };
                if (!lit) continue;
                UnitVisual fv = UnitVisual.Live(f.Id);
                if (fv != null) SkillRim.For(fv.BodyRenderer)?.Show(A(c, .5f * breathe * strength), .026f, float.NaN);
            }
        }
        private static bool Contains(IReadOnlyList<UnitState> list, UnitState u) { for (int i = 0; i < list.Count; i++) if (list[i] == u) return true; return false; }

        // ------------------------------------------------------------------ what each hero shows while it waits
        private void HeroFlair()
        {
            UnitState hero = sim.Champion; if (sparse) return;
            bool awake = sim.HeroAwakening, primed = sim.HeroAwakenPrimed;
            Vector2 at = DrawnAt(hero), facing = hero.Facing.sqrMagnitude > .01f ? hero.Facing.normalized : Vector2.right;
            float body = hero.Definition.BodyHeight, side = facing.x < -.05f ? -1 : 1;
            Vector2 head = at + Vector2.up * (body * .5f + .1f), chest = at + Vector2.up * body * .08f, feet = at - Vector2.up * body * .5f;
            Vector2 back = chest - new Vector2(side * (body * .35f + .15f), 0);
            Vector2 tip = UnitVisual.TryMuzzle(hero.Id, out Vector2 m) ? m : chest + facing * .5f;
            float fill = Mathf.Clamp01(sim.HeroIdentityCharge / Mathf.Max(.01f, sim.HeroIdentityRequired)), r;
            if (awake) return;
            switch (style.Id)
            {
                case "LanternKnight": // the lamp's flame grows with the sparks gathered
                    { float d = .32f + .38f * fill + (primed ? .12f : 0), flicker = still ? 1 : .85f + .15f * Mathf.Sin(t * 9) * Mathf.Sin(t * 3.7f); Light(MechanicInk.Glow, tip, d, d, 0, A(new Color(1, .86f, .55f), .7f * flicker * strength)); Light(MechanicInk.Glow, tip, d * .35f, d * .35f, 0, A(Color.white, .6f * flicker * strength)); }
                    break;
                case "Postmaster": // letters circle every stamped address
                    for (int i = 0; i < sim.HeroIdentityMarkCount; i++)
                    {
                        UnitState u = sim.HeroIdentityMarked(i); if (u == null || !u.Alive) continue;
                        float a = (still ? 0 : t * 120) + i * 60; Token(InkToken.Envelope, Head(u) + new Vector2(Mathf.Cos(a * Mathf.Deg2Rad) * .35f, .1f + Mathf.Sin(a * Mathf.Deg2Rad) * .1f), .14f, 0, .85f, true);
                    }
                    break;
                case "DrowsySeer": // standing still it dozes: little stars float up like z's
                    if (sim.HeroIdentityStillTime > .4f || still) for (int i = 0; i < 3; i++) { r = still ? i / 3f : Mathf.Repeat(t * .5f + i / 3f, 1); Token(InkToken.Star, head + new Vector2(side * (.2f + r * .35f) + Mathf.Sin(r * 6) * .06f, r * .6f), .08f + .07f * r, r * 60, Mathf.Sin(r * Mathf.PI) * .9f, true); }
                    if (primed && !HasArt("DrowsySeer_MoonPhase")) Token(InkToken.Moon, head + Vector2.up * .45f, .3f, 0, .95f, true);
                    break;
                case "GlassDice": // the next face, shown over its shoulder (the painted die when it exists)
                    if (!HasArt("GlassDice_Die")) Token(InkToken.Die, head + new Vector2(-side * .4f, .05f + Bob(2) * 2), .22f + (primed ? .06f : 0), still ? 0 : Mathf.Sin(t * (primed ? 20 : 1.5f)) * (primed ? 18 : 10), .95f, true, Mathf.Clamp(sim.HeroIdentityNextFace, 1, 6)); break;
                case "TideSinger":
                    for (int i = 0; i < 2; i++) { float a = (still ? 0 : t * 80) + i * 180; Token(InkToken.Note, chest + new Vector2(Mathf.Cos(a * Mathf.Deg2Rad) * .55f, .25f + Mathf.Sin(a * Mathf.Deg2Rad) * .18f), .16f, Mathf.Sin(t * 3 + i) * 15, .85f, true); }
                    if (primed) for (int i = 0; i < 3; i++) { r = Mathf.Repeat(t * 1.5f + i / 3f, 1); Token(InkToken.Bubble, feet + new Vector2((i - 1) * .3f, .05f + r * .3f), .1f, 0, (1 - r) * .9f, true, 0, ArsenalInk.Water); }
                    break;
                case "MirrorKeeper":
                    if (sim.HeroIdentityStored > 0 && !still) { float tw = Mathf.Abs(Mathf.Sin(t * 7)); Token(InkToken.Glint, chest + new Vector2(side * .5f, .3f), .12f + .1f * tw, 0, tw, true, 0, GlintPalette(ink)); }
                    break;
                case "TreasureAcorn":
                    if (sim.HeroIdentityBounty != null && t - crownLitAt < .35f) { sigScale = 1; sigReach = 1; Fountain(InkToken.Coin, Head(sim.HeroIdentityBounty) + Vector2.up * .3f, 2, .5f, .4f, .13f, (t - crownLitAt) / .35f, 1, 0); }
                    break;
                case "VerdantMatron": // the sprouts sway; a full gauge opens their buds
                    foreach (UnitState f in sim.Units)
                    {
                        if (!f.Alive || f.Faction != Faction.Companion || (f.Definition.Id != "GrandSprout" && f.Definition.Id != "LittleSprout")) continue;
                        Vector2 fh = DrawnAt(f) + Vector2.up * (f.Definition.BodyHeight * .5f + .12f);
                        Token(primed ? InkToken.Petal : InkToken.Leaf, fh, primed ? .22f : .15f, still ? 0 : Mathf.Sin(t * 2.5f + f.Id) * 20, .9f, true);
                    }
                    break;
                case "CopperCauldron": // steam rises faster as it builds
                    for (int i = 0; i < 1 + Mathf.RoundToInt(fill * 3); i++) { r = Mathf.Repeat(t * (.8f + fill * 1.4f) + i / 4f, 1); Token(InkToken.Puff, head + new Vector2(side * .15f + Mathf.Sin(r * 5 + i) * .08f, r * .55f), .12f + r * .14f, 0, (1 - r) * .75f, true, 0, ArsenalInk.Cotton); }
                    break;
                case "ChinaMarionette": // its cracks glint gold, one per shard
                    for (int i = 0; i < Mathf.FloorToInt(sim.HeroIdentityCharge + .001f) && i < 3; i++) { float tw = still ? .8f : .5f + .5f * Mathf.Sin(t * 5 + i * 2); Token(InkToken.Glint, chest + new Vector2((i - 1) * .16f, ((i * 7) % 3 - 1) * .14f), .1f + .05f * tw, i * 30, tw, true, 0, GoldPalette); }
                    break;
                case "SoapSkater": // bubbles drift off behind it as it glides
                    for (int i = 0; i < 3; i++) { r = Mathf.Repeat(t * .9f + i / 3f, 1); Token(InkToken.Bubble, feet + Vector2.up * .15f - facing * (.25f + r * .7f) + Vector2.up * r * .3f, .09f + .04f * r, 0, (1 - r) * .7f, true); }
                    break;
                case "VelvetRegent" when primed:
                    Token(InkToken.Seal, head + Vector2.up * .15f, .26f, 0, Mathf.Repeat(t * 3, 1) < .5f ? 1 : .5f, true, 0, GoldPalette);
                    break;
                case "IvorySovereign": // a decree in hand (the painted decree when it exists)
                    if (!HasArt("IvorySovereign_Decree")) Token(InkToken.Page, chest + new Vector2(-side * .42f, -.05f + Bob(1)), .2f + (primed ? .06f : 0), -side * 15, .9f, true); break;
                case "PetalCitadel":
                    for (int i = 0; i < 4; i++) { float a = (still ? 0 : t * 25) + i * 90; Token(InkToken.Petal, feet + new Vector2(Mathf.Cos(a * Mathf.Deg2Rad) * .5f, .05f + Mathf.Sin(a * Mathf.Deg2Rad) * .14f), .15f, a + 90, .8f, false); }
                    break;
                case "PatchworkHost": // a spotlight on the featured friend
                    if (sim.HeroIdentityLead != null && sim.HeroIdentityLead.Alive) for (int i = 0; i < 3; i++) { r = Mathf.Repeat(t * .7f + i / 3f, 1); Token(InkToken.Glint, DrawnAt(sim.HeroIdentityLead) + new Vector2((i - 1) * .2f, sim.HeroIdentityLead.Definition.BodyHeight * .5f + .9f - r * .9f), .1f, 0, Mathf.Sin(r * Mathf.PI) * .8f, true, 0, GoldPalette); }
                    break;
                case "ArchiveCurator": // one page per recorded species
                    for (int i = 0; i < Mathf.Min(3, sim.HeroIdentitySpecimens); i++) { float a = (still ? 0 : t * 50) + i * 120; Token(InkToken.Page, chest + new Vector2(Mathf.Cos(a * Mathf.Deg2Rad) * .6f, .2f + Mathf.Sin(a * Mathf.Deg2Rad) * .2f), .16f, Mathf.Sin(t * 2 + i) * 15, .85f, true); }
                    break;
                case "PennyBroker":
                    r = still ? .5f : Mathf.Repeat(t * 1.3f, 1); Token(InkToken.Coin, chest + new Vector2(-side * .4f, -.1f + 4 * r * (1 - r) * .3f), .15f, r * 360, .9f, true, 0, Brassy); break;
                case "StormFencer": // static crawls on the blade
                    { UnitVisual sv = UnitVisual.Live(hero.Id); if (sv != null && sv.WeaponRenderer != null && fill > .25f) SkillRim.For(sv.WeaponRenderer)?.Show(A(ink.Glow, (.4f + .5f * fill) * strength), .02f, float.NaN, .28f, fill); }
                    if (!still && Mathf.Repeat(t * 4 + fill, 1) < .35f + .4f * fill) Token(InkToken.Bolt, tip + Dir(t * 700) * .08f, .12f + .08f * fill, t * 900, .9f, true);
                    if (primed && !still) Crackle(chest + Vector2.up * .2f, 3, .5f, .3f, .7f, Mathf.FloorToInt(t * 6), ink, true, .025f);
                    break;
                case "TinVirtuoso": // the baton beats time
                    r = still ? .5f : Mathf.Repeat(t * 2, 1); Token(InkToken.Note, tip + Vector2.up * (.1f + r * .3f), .13f, Mathf.Sin(r * 6) * 20, (1 - r) * .9f, true); break;
                case "MoonProwler":
                    Token(InkToken.Moon, head + new Vector2(-side * .35f, .05f + Bob(1) * 2), .17f + .06f * fill, -20, .85f, true); break;
                case "SupperMortar": // steam from the pot
                    for (int i = 0; i < 2; i++) { r = Mathf.Repeat(t * 1.1f + i * .5f, 1); Token(InkToken.Puff, tip + new Vector2(Mathf.Sin(r * 5 + i) * .06f, .05f + r * .45f), .1f + r * .12f, 0, (1 - r) * .7f, true, 0, ArsenalInk.Cotton); }
                    break;
                case "LanternWeaver": // a needle circles; little lanterns hang on the threads
                    { float a = still ? 30 : t * 150; Token(InkToken.Shard, chest + new Vector2(Mathf.Cos(a * Mathf.Deg2Rad) * .55f, .15f + Mathf.Sin(a * Mathf.Deg2Rad) * .18f), .15f, a + 90, .9f, true); }
                    for (int i = 1; i < sim.HeroIdentityMarkCount; i++)
                    {
                        UnitState u0 = sim.HeroIdentityMarked(i - 1), u1 = sim.HeroIdentityMarked(i); if (u0 == null || u1 == null || !u0.Alive || !u1.Alive) continue;
                        if (!HasArt("LanternWeaver_Lantern")) Token(InkToken.Ember, (Chest(u0) + Chest(u1)) * .5f - Vector2.up * .1f + Vector2.up * Bob(i), .14f, 0, .9f, true);
                    }
                    break;
                case "HourglassStag": // sand trickles from its antlers
                    for (int i = 0; i < 3; i++) { r = Mathf.Repeat(t * 1.4f + i / 3f, 1); Token(InkToken.Glint, head + new Vector2(((i * 5) % 3 - 1) * .15f, .2f - r * .6f), .07f, 0, (1 - r) * .8f, true, 0, ArsenalInk.Dust); }
                    break;
                case "RainArchivist":
                    for (int i = 0; i < 3; i++) { r = Mathf.Repeat(t * 1.6f + i / 3f, 1); Token(InkToken.Drop, head + new Vector2((i - 1) * .2f, .55f - r * .5f), .08f, 180, Mathf.Sin(r * Mathf.PI) * .8f, true); }
                    break;
                case "KiteAdmiral": // a little kite on a string
                    { Vector2 kite = head + new Vector2(-side * .55f + (still ? 0 : Mathf.Sin(t * 1.7f) * .1f), .5f + Bob(3) * 2); Thread(chest, kite, .5f, 3); Token(InkToken.Spark, kite, .2f, still ? 0 : Mathf.Sin(t * 2.3f) * 12, .9f, true); }
                    break;
                case "SundaeDuchess": // it melts
                    for (int i = 0; i < 2; i++) { r = Mathf.Repeat(t * .8f + i * .5f, 1); Token(InkToken.Drop, head + new Vector2((i - .5f) * .25f, -.05f - r * .4f), .09f, 180, (1 - r) * .85f, true, 0, ArsenalInk.Candy); }
                    break;
                case "BonsaiClocksmith": // the clock on its back ticks round
                    Token(InkToken.Sand, back + Vector2.up * .2f, .2f, still ? 0 : -Mathf.Floor(t * 2) * 30, .9f, true); break;
                case "JugglingJester": // three little balls in the air
                    for (int i = 0; i < 3; i++) { float q = still ? i / 3f : Mathf.Repeat(t * 1.2f + i / 3f, 1); Token(InkToken.Star, tip + new Vector2(Mathf.Cos(q * Mathf.PI * 2) * .18f, .25f + Mathf.Abs(Mathf.Sin(q * Mathf.PI * 2)) * .35f), .12f, q * 360, .9f, true, 0, GoldPalette); }
                    break;
                case "LoneJelly": // a remembered organ glows inside the gel, beating
                    { float beat = still ? .7f : .55f + .45f * Mathf.Pow(Mathf.Abs(Mathf.Sin(t * 2.2f)), 6); Token(InkToken.Drop, chest + Vector2.up * .05f, .12f + .06f * beat, 180, .75f * beat, true); Glow(chest, .4f, ink.Light, .35f * beat, true); }
                    break;
                case "ButtonCavalier": // one button lit on its chest for each point of poise
                    if (!HasArt("ButtonCavalier_Buttons")) for (int i = 0; i < 3; i++) { bool on = i < Mathf.FloorToInt(sim.HeroIdentityCharge + .001f); Vector2 b = chest + new Vector2(side * .12f, .18f - i * .14f); if (on) Token(InkToken.Button, b, .12f, 0, .95f, true); else DimToken(InkToken.Button, b, .1f, .45f); }
                    break;
                case "ThornTreasurer": // its crown of collateral thorns, one per thorn held
                    if (!HasArt("ThornTreasurer_ThornCrown")) { int n = Mathf.Clamp(Mathf.FloorToInt(sim.HeroIdentityCharge + .001f), 0, 4); for (int i = 0; i < n; i++) Token(InkToken.Thorn, head + new Vector2((i - (n - 1) * .5f) * .14f, .08f), .13f, 90, .9f, true); }
                    break;
                case "VelvetRegent": // a sceptre held at its side, glinting as orders gather
                    Token(InkToken.Spear, chest + new Vector2(-side * .4f, .05f + Bob(1)), .26f, 90 + (still ? 0 : Mathf.Sin(t * 1.5f) * 6), .9f, true, 0, GoldPalette);
                    if (!still && Mathf.Repeat(t * .8f, 1) < .2f) Token(InkToken.Glint, chest + new Vector2(-side * .4f, .3f), .12f, 0, 1 - Mathf.Repeat(t * .8f, 1) * 5, true, 0, GoldPalette);
                    break;
                case "NightMaw": // its belly grumbles with each meal held: wisps of ink drifting off it
                    for (int i = 0; i < Mathf.Clamp(Mathf.FloorToInt(sim.HeroIdentityCharge + .001f), 0, 3); i++) { r = Mathf.Repeat(t * .9f + i / 3f, 1); Token(InkToken.Puff, chest + new Vector2(side * (.1f + i * .1f), -.1f + r * .45f), .1f + r * .08f, 0, (1 - r) * .6f, true, 0, ArsenalInk.Violet); }
                    if (primed) for (int i = 0; i < 2; i++) { r = Mathf.Repeat(t * 1.2f + i * .5f, 1); Token(InkToken.Drop, chest + new Vector2(side * .3f + (i - .5f) * .1f, -r * .5f), .1f, 180, (1 - r) * .85f, true, 0, ArsenalInk.Wine); }
                    break;
                case "BellDiver":
                    for (int i = 0; i < 3; i++) { r = Mathf.Repeat(t * 1f + i / 3f, 1); Token(InkToken.Bubble, head + new Vector2(((i * 3) % 3 - 1) * .12f, r * .6f), .07f + .05f * r, 0, (1 - r) * .85f, true, 0, ArsenalInk.Water); }
                    break;
            }
        }

        // ------------------------------------------------------------------ a friend speaks as the transformation begins
        private void AwakenBanter()
        {
            if (!sim.HeroAwakening || sim.HeroAwakenStarted <= banterFor + .01f) return;
            banterFor = sim.HeroAwakenStarted;
            HeroKit kit = sim.HeroKitInfo; if (kit == null) return;
            var friends = new List<UnitState>();
            foreach (UnitState f in sim.Units) if (f.Alive && f.Faction == Faction.Companion) friends.Add(f);
            if (friends.Count == 0) return;
            UnitState who = friends[Random.Range(0, friends.Count)];
            string[] lines = AwakenLines(kit.Action);
            int pick = Random.Range(0, lines.Length); if (pick == banterLast && lines.Length > 1) pick = (pick + 1) % lines.Length;
            banterLast = pick;
            SpeechBubbles.Show("Companion:" + who.Definition.Id, who.Definition.DisplayName, lines[pick],
                () => who.Alive && sim != null && sim.Phase == RunPhase.Combat ? SpeechBubbles.Head(who) : (Vector3?)null, SpeechTone.Companion, 2.6f, 0);
        }
        private static readonly string[] Cheers = { "Now!", "Here it comes!", "Go, go!", "That's our cue!", "Light them up!", "Hold nothing back!" };
        private static string[] AwakenLines(KitAction action) => action switch
        {
            KitAction.Lighthouse => new[] { "Stay in the light!", "The lamp is lit!", "Follow the beam!", "Brighter than morning!", "I can see them all now!" },
            KitAction.Gatling => new[] { "Mail call!", "Special delivery!", "Sign here, please!", "No return to sender!", "Express post, coming through!" },
            KitAction.Dreams => new[] { "Three of you? Lovely.", "Sweet dreams, monsters.", "The moon is full!", "Dream with us!", "Which one is the real you?" },
            KitAction.Treant => new[] { "The tree walks!", "Roots, rise!", "Mind the thorns!", "Now that's a big branch!", "The forest is with us!" },
            KitAction.Mitosis => new[] { "There's more of you!", "Split and squish!", "Jelly time!", "Three jellies, no waiting!", "Wobble, wobble, wham!" },
            KitAction.Casino => new[] { "Roll the dice!", "Feeling lucky!", "All in!", "House always wins!", "Snake eyes? Never!" },
            KitAction.HighTide => new[] { "Surf's up!", "Ride the wave!", "Here comes the tide!", "Wash them away!", "Sing us home!" },
            KitAction.Meteors => new[] { "Make a wish!", "Stars are falling!", "Heads up, sky's coming down!", "Shooting stars!", "Burn bright!" },
            KitAction.Mirrors => new[] { "Right back at you!", "Mirror, mirror!", "Catch and return!", "Nothing gets through!", "Reflect on that!" },
            KitAction.AcornGun => new[] { "Pay up!", "Bounty's due!", "Acorns away!", "Collecting now!", "That one's worth a lot!" },
            KitAction.Spring => new[] { "Everything's blooming!", "Spring is here!", "Grow, little ones!", "Flowers, now!", "Smells like victory!" },
            KitAction.Engine => new[] { "All aboard!", "Full steam ahead!", "Choo choo!", "Pressure's rising!", "Clear the tracks!" },
            KitAction.Kintsugi => new[] { "Golden cracks!", "Mended stronger!", "You can't break us!", "Shine, porcelain!", "Beautiful scars!" },
            KitAction.Rink => new[] { "Skate time!", "Mind the ice!", "Slippery!", "Glide, glide!", "Watch them slide!" },
            KitAction.Parade => new[] { "Long live the crown!", "Knighted? Me?", "To the parade!", "For the Regent!", "I feel taller!" },
            KitAction.Duel => new[] { "En garde!", "A duel!", "Touché!", "Bow, then strike!", "Six thrusts, one breath!" },
            KitAction.Feast => new[] { "Dinner's served!", "Big bite!", "Hungry, hungry!", "Don't chew, swallow!", "Leave some for me!" },
            KitAction.Throne => new[] { "Bow to the thorns!", "The throne rises!", "Touch it and bleed!", "Royal thorns!", "Sit tight, ruler!" },
            KitAction.Train => new[] { "All aboard the citrus line!", "Next stop: them!", "Choo choo, zest!", "Couple up!", "Fresh off the tracks!" },
            KitAction.Coronation => new[] { "The decree is written!", "Long live the queen!", "By royal order!", "Kneel!", "Sentence: gone!" },
            KitAction.Citadel => new[] { "Raise the walls!", "Petal fortress!", "Safe behind the bloom!", "Hold the citadel!", "Petals, fire!" },
            KitAction.Showtime => new[] { "Curtain up!", "Showtime!", "I'm the star now!", "Encore! Encore!", "Take a bow!" },
            KitAction.Exhibit => new[] { "Exhibits, awaken!", "From the archive!", "Paper beasts!", "Page by page!", "History bites back!" },
            KitAction.BullMarket => new[] { "Market's up!", "Double the coins!", "Buy low, hit high!", "Profit time!", "Cha-ching!" },
            KitAction.Thunder => new[] { "Thunder!", "Feel the storm!", "Lightning blade!", "Crackle and strike!", "Here comes the bolt!" },
            KitAction.Symphony => new[] { "On the beat!", "All together now!", "Hit it!", "From the top!", "Play it loud!" },
            KitAction.Eclipse => new[] { "Lights out!", "The moon hides!", "From the shadows!", "They won't see it coming!", "Eclipse!" },
            KitAction.Kitchen => new[] { "Order up!", "Six courses!", "Hot, hot, hot!", "Dessert's on them!", "Bon appétit!" },
            KitAction.Tapestry => new[] { "Weave it tight!", "Caught in the net!", "Every knot counts!", "Tighten!", "Stitch them up!" },
            KitAction.Foreclosure => new[] { "Time's up!", "Debt collected!", "Pay with interest!", "The clock stops!", "Every loan comes due!" },
            KitAction.Monsoon => new[] { "Storm's coming!", "Rain, rain!", "Write the weather!", "Thunder's on our side!", "Get wet!" },
            KitAction.KiteFleet => new[] { "Kites up!", "Fly, fleet!", "Wind at our backs!", "Double shots!", "Sail the sky!" },
            KitAction.SundaeRain => new[] { "Scoops incoming!", "Brain freeze!", "Sweet and cold!", "Extra sprinkles!", "Ice cream rain!" },
            KitAction.TimeGarden => new[] { "Tick tock!", "Plant the clock!", "Wind the garden!", "Right on time!", "Every second blooms!" },
            KitAction.Circus => new[] { "Roll up, roll up!", "Five-ball act!", "Juggle them!", "Big top time!", "Ta-da!" },
            KitAction.DivingBell => new[] { "Dive, dive!", "Down we go!", "Depth charge!", "Bubbles away!", "Ding, ding, boom!" },
            _ => Cheers
        };
    }
}
