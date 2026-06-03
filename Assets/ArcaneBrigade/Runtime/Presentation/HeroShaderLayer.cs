using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Every hero's own shader moment (owner 24/10/2026: "each hero should have one"). Built on the material layer
    /// (HeroDistortVisual): refraction (water, gel, the push of a blow, ink vortices, a local eclipse, time draining the
    /// colour), thin-film colour (soap, dream glass, petals), metal sheen and seams (brass, gold, ivory, kintsugi, sap),
    /// frost on frozen enemies, paper puppets and real light. Off with Reduced Effects, weak devices and crowds.</summary>
    public sealed partial class HeroIdentityVisual
    {
        private static Material iridescentMaterial, metalMaterial, frostMaterial, paperMaterial;
        private static readonly Color BrassSheen = new Color(1f, .82f, .5f), GoldLeaf = new Color(1f, .86f, .42f), Ivory = new Color(1f, .96f, .86f);

        // ------------------------------------------------------------------ the pieces
        private void Metal(SpriteRenderer source, Color tint, float veins = 0, float speed = .6f)
        {
            SpriteRenderer r = Overlay(source, Load(ref metalMaterial, "HeroMetal"), tint); if (r == null) return;
            warpBlock ??= new MaterialPropertyBlock(); warpBlock.Clear(); warpBlock.SetFloat("_Veins", veins); warpBlock.SetFloat("_Speed", still ? 0 : speed);
            r.SetPropertyBlock(warpBlock);
        }
        private void Iris(SpriteRenderer source, Color tint)
        {
            SpriteRenderer r = Overlay(source, Load(ref iridescentMaterial, "HeroIridescent"), tint); if (r == null) return;
            warpBlock ??= new MaterialPropertyBlock(); warpBlock.Clear(); warpBlock.SetFloat("_Edge", 0); r.SetPropertyBlock(warpBlock);
        }
        /// <summary>A soap bubble: clear in the middle, its rim running with thin-film colour.</summary>
        private void Bubble(Vector2 at, float size, float alpha)
        {
            if (!WarpReady || alpha <= .01f || Load(ref iridescentMaterial, "HeroIridescent") == null) return;
            SpriteRenderer r = NextWarp(); r.sharedMaterial = iridescentMaterial; r.sprite = Blob; r.color = new Color(1, 1, 1, alpha * strength); r.sortingOrder = WarpOrder + 1;
            r.flipX = r.flipY = false; float s = size / Mathf.Max(.001f, Blob.bounds.size.x);
            r.transform.position = at; r.transform.rotation = Quaternion.identity; r.transform.localScale = new Vector3(s, s, 1);
            warpBlock ??= new MaterialPropertyBlock(); warpBlock.Clear(); warpBlock.SetFloat("_Edge", 1); r.SetPropertyBlock(warpBlock);
        }
        private void Frost(SpriteRenderer source, float alpha) { var r = Overlay(source, Load(ref frostMaterial, "HeroFrost"), new Color(1, 1, 1, alpha)); r?.SetPropertyBlock(null); }
        /// <summary>A recorded enemy as a paper cut-out standing among the brigade (drawn even when the material layer is off).</summary>
        private void Puppet(Sprite body, Vector2 feet, float height, bool flip, float alpha)
        {
            if (body == null || Load(ref paperMaterial, "HeroPaper") == null) { Token(InkToken.Page, feet + Vector2.up * height * .5f, height * .6f, 0, alpha, true); return; }
            SpriteRenderer r = NextWarp(); r.sharedMaterial = paperMaterial; r.sprite = body; r.color = new Color(1, 1, 1, alpha * strength);
            r.flipX = flip; r.flipY = false; r.sortingOrder = 1000 - Mathf.RoundToInt(feet.y * 12);
            Rect ink = SpriteInkMetrics.BoundsOf(body); float s = height / Mathf.Max(.01f, ink.height);
            r.transform.rotation = Quaternion.identity; r.transform.localScale = new Vector3(s, s, 1);
            r.transform.position = (Vector3)(feet + Vector2.up * height * .5f) - (Vector3)(ink.center * s);
            r.SetPropertyBlock(null);
        }
        private SpriteRenderer HeroBody(UnitState u) { UnitVisual v = UnitVisual.Live(u.Id); return v != null ? v.BodyRenderer : null; }
        private SpriteRenderer HeroWeapon(UnitState u) { UnitVisual v = UnitVisual.Live(u.Id); return v != null && v.WeaponRenderer != null && v.WeaponRenderer.enabled ? v.WeaponRenderer : null; }

        // ------------------------------------------------------------------ each hero's moment, every frame
        private void ShaderLayer()
        {
            UnitState hero = sim.Champion; HeroKit kit = sim.HeroKitInfo; bool awake = sim.HeroAwakening;
            Vector2 at = DrawnAt(hero), facing = hero.Facing.sqrMagnitude > .01f ? hero.Facing.normalized : Vector2.right;
            float body = hero.Definition.BodyHeight, heading = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg, since = t - sim.HeroAwakenStarted;
            Vector2 chest = at + Vector2.up * body * .08f, feet = at - Vector2.up * body * .5f;
            bool moving = (hero.Position - hero.PreviousPosition).sqrMagnitude > 1e-5f;
            // frost on every frozen enemy near a Sundae Duchess, melting from its edges as the chill runs out
            if (style.Id == "SundaeDuchess")
                foreach (UnitState u in sim.Units)
                {
                    if (!u.Alive || u.Faction != Faction.Enemy || u.ChillUntil <= t || (u.Position - hero.Position).sqrMagnitude > 100) continue;
                    SpriteRenderer b = HeroBody(u); if (b != null) Frost(b, Mathf.Clamp01((u.ChillUntil - t) / .4f) * .9f);
                }
            if (!awake || kit == null) return;
            switch (kit.Action)
            {
                case KitAction.Gatling: // the barrels shimmer with heat as they fire
                {
                    Vector2 tip = UnitVisual.TryMuzzle(hero.Id, out Vector2 m) ? m : chest + facing * .5f; Vector2 aim = sim.HeroAwakenAim.sqrMagnitude > .01f ? sim.HeroAwakenAim : facing;
                    Refract(Wedge, tip + aim * .35f, new Vector2(.9f, .5f), Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg, new Color(1, .95f, .85f, .8f), .014f, aim.x, aim.y);
                    break;
                }
                case KitAction.Dreams: if (dreamEchoes != null) foreach (var e in dreamEchoes) if (e != null && e.enabled) Iris(e, new Color(.8f, .78f, 1f, .4f)); break; // dream glass
                case KitAction.Casino: Iris(HeroBody(hero), new Color(1f, .9f, .95f, .32f)); break; // the glass die catches rainbow light
                case KitAction.Meteors: if (moving) Refract(Wedge, chest - facing * .9f, new Vector2(1.4f, .7f), heading + 180, new Color(1, .9f, .75f, .8f), .02f, -facing.x, -facing.y); break; // heat behind the comet
                case KitAction.AcornGun: Metal(HeroWeapon(hero), BrassSheen, 0, .9f); break;
                case KitAction.Spring:
                    foreach (UnitState f in sim.Units) if (f.Alive && f.Faction == Faction.Companion && (f.Definition.Id == "GrandSprout" || f.Definition.Id == "LittleSprout")) Metal(HeroBody(f), new Color(.75f, 1f, .55f, .7f), 1, .4f); // sap running in the leaves
                    break;
                case KitAction.Kintsugi: Metal(HeroBody(hero), new Color(GoldLeaf.r, GoldLeaf.g, GoldLeaf.b, .9f), 1, .5f); break; // gold seams through the porcelain
                case KitAction.Rink: if (moving && !sparse) Bubble(feet + Vector2.up * .2f - facing * .5f, .45f, .6f); break;
                case KitAction.Parade: foreach (UnitState f in sim.Units) if (f.Alive && f.Faction == Faction.Companion) Metal(HeroBody(f), new Color(GoldLeaf.r, GoldLeaf.g, GoldLeaf.b, .55f), 0, .5f); break; // knighted in gold
                case KitAction.Duel: if (sim.HeroAwakenFoe != null && sim.HeroAwakenFoe.Alive) Refract(Blob, DrawnAt(sim.HeroAwakenFoe), Vector2.one * 2.2f, 0, new Color(.9f, .9f, 1f, .9f), .006f, 0, 0, 0, .45f, .5f); break; // time slows round the foe
                case KitAction.Feast: Refract(Blob, at + new Vector2(facing.x < -.05f ? -body * .7f : body * .7f, 0), Vector2.one * 1.7f, 0, new Color(.6f, .4f, .8f, .9f), .012f, 0, 0, .35f, 0, 1.2f); break; // a vortex of ink at the mouth
                case KitAction.Throne: Metal(HeroBody(hero), new Color(BrassSheen.r, BrassSheen.g, BrassSheen.b, .6f), 0, .45f); break;
                case KitAction.Coronation: Metal(HeroBody(hero), new Color(Ivory.r, Ivory.g, Ivory.b, .75f), 0, .7f); break;
                case KitAction.Citadel: Iris(HeroBody(hero), new Color(1f, .82f, .9f, .28f)); break; // petals lit from behind
                case KitAction.Showtime:
                {
                    UnitState lead = sim.HeroIdentityLead != null && sim.HeroIdentityLead.Alive ? sim.HeroIdentityLead : hero; Vector2 la = DrawnAt(lead);
                    Light(BeamSprite(), la + Vector2.up * 4.2f, 1.7f, 4.4f, 180, A(new Color(1, .94f, .78f), .4f * strength)); // the spotlight
                    break;
                }
                case KitAction.Exhibit:
                    for (int i = 0; i < 3; i++)
                    {
                        UnitDefinition specimen = sim.HeroIdentitySpecimen(i); if (specimen == null && i > 0) break;
                        Vector2 spot = at + Dir(60 + i * 120) * 1.2f + Vector2.up * (still ? 0 : Mathf.Abs(Mathf.Sin(t * 3 + i * 2)) * .08f);
                        Sprite pose = specimen?.AuthoredMotion != null && specimen.AuthoredMotion.Idle != null && specimen.AuthoredMotion.Idle.Length > 0 ? specimen.AuthoredMotion.Idle[Mathf.FloorToInt(t * 3) % specimen.AuthoredMotion.Idle.Length] : specimen?.Body;
                        Puppet(pose, spot - Vector2.up * .45f, Mathf.Clamp(specimen != null ? specimen.BodyHeight : .8f, .5f, 1.1f) * 1.25f, spot.x > at.x, Mathf.Clamp01(since * 3) * Mathf.Clamp01(sim.HeroAwakenRemaining * 6));
                    }
                    break;
                case KitAction.BullMarket: Metal(HeroBody(hero), new Color(GoldLeaf.r, GoldLeaf.g, GoldLeaf.b, .65f), 0, .9f); break;
                case KitAction.Symphony:
                    Metal(HeroWeapon(hero), BrassSheen, 0, 1.2f);
                    if (!still) Shock(chest, Mathf.Repeat(t, 1f) / .45f, 1.6f); // each beat pushes the air
                    break;
                case KitAction.Eclipse: Refract(Blob, at, Vector2.one * 5.5f * Mathf.Clamp01(since * 2), 0, new Color(.6f, .65f, .9f, .9f), .004f, 0, 0, .42f, .15f); break; // the eclipse, round the hero only
                case KitAction.Kitchen: Refract(Wedge, at + Vector2.up * (body * .5f + .4f), new Vector2(.9f, .6f), 90, new Color(1, .93f, .85f, .7f), .014f, 0, .8f); break; // heat over the pots
                case KitAction.Tapestry:
                    for (int i = 0; i < sim.HeroIdentityMarkCount; i++) { UnitState u = sim.HeroIdentityMarked(i); if (u != null && u.Alive) Shock(Chest(u), Mathf.Repeat(t * 2 + i * .3f, 1) / .5f, .8f); } // each knot tightens
                    break;
                case KitAction.Foreclosure: Refract(Blob, at, Vector2.one * 7f * OutCubic(Mathf.Clamp01(since * 3)), 0, new Color(.85f, .85f, .95f, Mathf.Clamp01((1.7f - since) * 2)), .003f, 0, 0, .1f, .85f); break; // time stops: the colour drains
                case KitAction.Monsoon: if (!still && Mathf.Repeat(t * 2.3f, 1) < .25f) Shock(at + new Vector2(Mathf.Sin(Mathf.Floor(t * 2.3f) * 7.1f) * 2.2f, Mathf.Cos(Mathf.Floor(t * 2.3f) * 3.7f) * 1.2f), Mathf.Repeat(t * 2.3f, 1) / .25f, .9f); break; // drops striking the floor
                case KitAction.KiteFleet:
                    foreach (UnitState f in sim.HeroKiteFleet) if (f != null && f.Alive) Refract(Wedge, DrawnAt(f) + Vector2.up * (f.Definition.BodyHeight * .5f + .7f), new Vector2(.9f, .45f), 180, new Color(.85f, .95f, 1f, .7f), .012f, -1, 0); // wind under the kites
                    break;
                case KitAction.TimeGarden:
                {
                    Vector2 hand = sim.HeroGardenHand, pivot = sim.HeroAwakenPlot + Vector2.up * .45f; float ha = Mathf.Atan2(hand.y, hand.x) * Mathf.Rad2Deg;
                    Refract(Wedge, pivot + hand.normalized * CombatSimulation.GardenReach * .5f, new Vector2(CombatSimulation.GardenReach, .7f), ha, new Color(.95f, .95f, .85f, .8f), .01f, 0, 0, 0, .3f); // time ripples along the hand
                    break;
                }
                case KitAction.Circus: Light(BeamSprite(), at + Vector2.up * 4.2f, 1.5f, 4.4f, 180, A(new Color(1, .9f, .75f), .38f * strength)); break; // the big top's spotlight
                case KitAction.Train: if (moving) Refract(Wedge, chest + facing * 1f, new Vector2(1.2f, .8f), heading, new Color(1, .93f, .82f, .7f), .014f, facing.x, facing.y); break;
                case KitAction.Lighthouse: Refract(Wedge, at + Dir((t - sim.HeroAwakenStarted) * CombatSimulation.LighthouseSweep) * 2.6f, new Vector2(4.4f, 2.2f), (t - sim.HeroAwakenStarted) * CombatSimulation.LighthouseSweep, new Color(1, .95f, .8f, .55f), .006f); break; // the beam's warm air
                case KitAction.Treant: if (moving && !still && Mathf.Repeat(t * 2.8f, 1) < .3f) Shock(feet + facing * .3f, Mathf.Repeat(t * 2.8f, 1) / .3f, .9f); break; // each root-step heaves the floor
            }
        }

        /// <summary>A strike's own shader moment for the heroes whose material lives in their hits.</summary>
        private void SignatureLayer(HeroIdentityGlyph g, Vector2 at, float p, float fade)
        {
            switch (style.Id)
            {
                case "RootOracle": case "CometRunner": case "RainArchivist": case "SupperMortar": case "JugglingJester": case "ButtonCavalier": case "VelvetRegent":
                    Shock(at + Vector2.up * .1f, p * 1.8f, .9f); break; // the hit heaves the floor or the air
                case "SoapSkater": if (p < .5f) Bubble(at + Vector2.up * .3f, (.35f + .4f * p) * sigScale, (1 - p * 2) * .9f); break;
                case "NightMaw": Refract(Blob, at + Vector2.up * .2f, Vector2.one * 1.1f, 0, new Color(.6f, .4f, .8f, (1 - p) * .9f), .01f, 0, 0, .3f * (1 - p), 0, 1.4f); break;
                case "PennyBroker": case "TreasureAcorn": case "IvorySovereign": case "ThornTreasurer": case "PatchworkHost": case "ArchiveCurator": case "LanternWeaver":
                case "HourglassStag": case "KiteAdmiral": case "BonsaiClocksmith": case "PetalCitadel": case "GlassDice": case "DrowsySeer": case "VerdantMatron": case "MoonProwler": case "TinVirtuoso":
                    Shock(at + Vector2.up * .15f, p * 2.4f, .7f); break; // a small push where it lands
            }
        }
    }
}
