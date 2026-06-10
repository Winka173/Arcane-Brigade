using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Every hero complete at every moment (requirement 24/10/2026): a shader of its own while it waits, on each strike
    /// and while transformed (HeroShaderLayer), and the second set of painted objects (Docs/Hero_Effect_Art_Requests_2.md)
    /// wired in ahead of the art - a sheet plays as soon as it is imported, and until then the tokens stand in.</summary>
    public sealed partial class HeroIdentityVisual
    {
        private static readonly Color Silver = new Color(.88f, .94f, 1f), Copper = new Color(1f, .7f, .45f), Sap = new Color(.72f, 1f, .5f);

        // ------------------------------------------------------------------ spots of material at a point
        private SpriteRenderer Spot(Material material, Vector2 at, float size, Color tint, int order)
        {
            if (!WarpReady || material == null || tint.a <= .01f) return null;
            SpriteRenderer r = NextWarp(); r.sharedMaterial = material; r.sprite = Blob; r.color = A(tint, tint.a * strength); r.sortingOrder = order;
            r.flipX = r.flipY = false; float s = size / Mathf.Max(.001f, Blob.bounds.size.x);
            r.transform.position = at; r.transform.rotation = Quaternion.identity; r.transform.localScale = new Vector3(s, s, 1);
            return r;
        }
        /// <summary>A metal glint at a spot (wax, brass, gold, silver, ivory), optionally run through with seams.</summary>
        private void Sheen(Vector2 at, float size, Color tint, float veins = 0)
        {
            var r = Spot(Load(ref metalMaterial, "HeroMetal"), at, size, tint, WarpOrder + 1); if (r == null) return;
            warpBlock ??= new MaterialPropertyBlock(); warpBlock.Clear(); warpBlock.SetFloat("_Veins", veins); warpBlock.SetFloat("_Speed", still ? 0 : 1.4f); r.SetPropertyBlock(warpBlock);
        }
        /// <summary>A spot of thin-film colour (glass, soap, petals, dreams).</summary>
        private void IrisSpot(Vector2 at, float size, Color tint)
        {
            var r = Spot(Load(ref iridescentMaterial, "HeroIridescent"), at, size, tint, WarpOrder + 1); if (r == null) return;
            warpBlock ??= new MaterialPropertyBlock(); warpBlock.Clear(); warpBlock.SetFloat("_Edge", 0); r.SetPropertyBlock(warpBlock);
        }
        /// <summary>Frost crystallising on the floor at a spot.</summary>
        private void FrostSpot(Vector2 at, float size, float alpha) { Spot(Load(ref frostMaterial, "HeroFrost"), at, size, new Color(1, 1, 1, alpha), WarpOrder)?.SetPropertyBlock(null); }

        // ------------------------------------------------------------------ the strike's own material, for every hero
        private void StrikeMaterial(HeroIdentityGlyph g, Vector2 at, float p, float fade)
        {
            Vector2 up = at + Vector2.up * .2f; float a = fade * (1 - p);
            switch (style.Id)
            {
                case "LanternKnight": Refract(Blob, up, Vector2.one * .9f, 0, new Color(1, .95f, .82f, a), .012f); break; // warm air in the lamplight
                case "Postmaster": Sheen(up, .7f * sigScale, new Color(1f, .75f, .65f, a)); break; // the wax seal glints
                case "DrowsySeer": IrisSpot(up, .75f * sigScale, new Color(.85f, .82f, 1f, a * .8f)); break; // dream glass
                case "LoneJelly": Refract(Blob, up, Vector2.one * .8f * sigScale, 0, new Color(.75f, 1f, .92f, a), .022f); break; // a blob of gel
                case "GlassDice": IrisSpot(up, .65f * sigScale, new Color(1f, .9f, .95f, a)); break;
                case "MirrorKeeper": Sheen(up, .7f * sigScale, new Color(Silver.r, Silver.g, Silver.b, a)); break;
                case "TreasureAcorn": case "PennyBroker": case "ThornTreasurer": case "ButtonCavalier": Sheen(up, .6f * sigScale, new Color(Copper.r, Copper.g, Copper.b, a)); break;
                case "VerdantMatron": Sheen(up, .7f * sigScale, new Color(Sap.r, Sap.g, Sap.b, a), 1); break; // sap in the petals' veins
                case "ChinaMarionette": Sheen(up, .7f * sigScale, new Color(1f, .86f, .42f, a), 1); break; // gold seams
                case "VelvetRegent": case "PatchworkHost": case "LanternWeaver": Sheen(up, .65f * sigScale, new Color(1f, .86f, .42f, a)); break;
                case "IvorySovereign": Sheen(up, .8f * sigScale, new Color(1f, .96f, .86f, a)); break;
                case "CitrusMarshal": Refract(Blob, up, Vector2.one * .75f * sigScale, 0, new Color(1f, .8f, .45f, a), .018f); break; // clear juice
                case "PetalCitadel": case "JugglingJester": IrisSpot(up, .7f * sigScale, new Color(1f, .85f, .92f, a * .8f)); break;
                case "ArchiveCurator": Refract(Blob, up, Vector2.one * .8f, 0, new Color(.75f, .8f, 1f, a), .008f, 0, 0, .25f * a); break; // ink soaking the spot
                case "StormFencer": Refract(Blob, up, Vector2.one * .9f, 0, new Color(.8f, .9f, 1f, a), .02f, 0, 0, 0, 0, .8f); break; // air twisted by the bolt
                case "MoonProwler": Refract(Blob, up, Vector2.one * .9f, 0, new Color(.65f, .7f, 1f, a), .006f, 0, 0, .35f * a); break; // a moment of shadow
                case "HourglassStag": case "BonsaiClocksmith": Refract(Blob, up, Vector2.one * .85f, 0, new Color(.9f, .9f, .95f, a), .006f, 0, 0, 0, .7f * a); break; // time drained out of the spot
                case "RainArchivist": case "BellDiver": Refract(Blob, at + Vector2.up * .1f, Vector2.one * .9f * sigScale, 0, new Color(.7f, .9f, 1f, a), .025f); break; // water rippling
                case "KiteAdmiral": Refract(Wedge, up, new Vector2(1f, .5f), 0, new Color(.85f, .95f, 1f, a), .014f, 1, 0); break; // a gust
                case "SundaeDuchess": FrostSpot(at + Vector2.up * .05f, .9f * sigScale, a); break;
                case "CometRunner": Refract(Blob, up, Vector2.one * .8f, 0, new Color(1f, .9f, .75f, a), .02f); break; // heat
                case "SupperMortar": SteamPuff(up + Vector2.up * .2f * p, .3f, a * .8f); break;
                case "TinVirtuoso": case "RootOracle": Shock(up, p * 1.8f, 1f); break; // the beat / the heaving floor
                case "SoapSkater": case "NightMaw": case "TideSinger": case "CopperCauldron": break; // their bubble, vortex, wave and steam are drawn in their signatures
            }
            StrikeProps(g, at, p, fade);
        }

        // ------------------------------------------------------------------ a quiet shader of its own while it waits
        private void WaitingMaterial()
        {
            UnitState hero = sim.Champion; if (sim.HeroAwakening || sparse) return;
            SpriteRenderer body = HeroBody(hero), weapon = HeroWeapon(hero);
            Vector2 at = DrawnAt(hero), facing = hero.Facing.sqrMagnitude > .01f ? hero.Facing.normalized : Vector2.right;
            float h = hero.Definition.BodyHeight, fill = Mathf.Clamp01(sim.HeroIdentityCharge / Mathf.Max(.01f, sim.HeroIdentityRequired)), q = .22f + .18f * fill;
            Vector2 tip = UnitVisual.TryMuzzle(hero.Id, out Vector2 m) ? m : at + facing * .5f;
            bool moving = (hero.Position - hero.PreviousPosition).sqrMagnitude > 1e-5f;
            switch (style.Id)
            {
                case "LanternKnight": break; // the lamp's growing light (HeroFlair); no heat haze over the weapon
                case "Postmaster": Metal(body, new Color(1f, .8f, .65f, q), 0, .3f); break;
                case "DrowsySeer": Iris(body, new Color(.85f, .82f, 1f, q * .8f)); break;
                case "RootOracle": if (fill > .7f && !still) Shock(at + Dir(90 + sim.HeroIdentityRootTurn * 120) * 1.8f - Vector2.up * .1f, Mathf.Repeat(t * 2, 1) / .4f, .6f); break; // the next root heaves the floor
                case "LoneJelly": break; // its gel shows on its jellies, never over the hero itself
                case "GlassDice": Iris(body, new Color(1f, .9f, .95f, q)); break;
                case "TideSinger": if (!still) Refract(Blob, at - Vector2.up * h * .45f, new Vector2(1.2f, .45f), 0, new Color(.7f, .9f, 1f, .5f + .4f * fill), .012f); break; // a ripple round its feet
                case "CometRunner": if (moving) Refract(Wedge, at - facing * .7f, new Vector2(1f, .5f), Mathf.Atan2(-facing.y, -facing.x) * Mathf.Rad2Deg, new Color(1, .9f, .75f, .4f + .5f * fill), .012f, -facing.x, -facing.y); break;
                case "MirrorKeeper": Metal(body, new Color(Silver.r, Silver.g, Silver.b, q), 0, .45f); break;
                case "TreasureAcorn": case "PennyBroker": case "TinVirtuoso": case "ButtonCavalier": Metal(weapon, new Color(Copper.r, Copper.g, Copper.b, q + .1f), 0, .5f); break;
                case "VerdantMatron": foreach (UnitState f in sim.Units) if (f.Alive && f.Faction == Faction.Companion && (f.Definition.Id == "GrandSprout" || f.Definition.Id == "LittleSprout")) Metal(HeroBody(f), new Color(Sap.r, Sap.g, Sap.b, q), 1, .3f); break;
                case "CopperCauldron": Metal(body, new Color(Copper.r, Copper.g, Copper.b, q), 0, .4f); break;
                case "ChinaMarionette": Metal(body, new Color(1f, .86f, .42f, .15f + .25f * Mathf.Clamp01(sim.HeroIdentityCharge / 3)), 1, .35f); break; // gold in its cracks, one per shard
                case "SoapSkater": if (moving && !still && Mathf.Repeat(t * 1.5f, 1) < .5f) Bubble(at - Vector2.up * h * .3f - facing * .5f, .3f, .5f); break;
                case "VelvetRegent": case "LanternWeaver": case "PatchworkHost": Metal(weapon ?? body, new Color(1f, .86f, .42f, q), 0, .4f); break;
                case "NightMaw": if (sim.HeroIdentityCharge > .5f) Refract(Blob, at + new Vector2(facing.x < -.05f ? -.3f : .3f, 0), Vector2.one * .7f, 0, new Color(.6f, .45f, .8f, .5f), .006f, 0, 0, .15f * fill, 0, .5f); break;
                case "ThornTreasurer": Metal(body, new Color(Copper.r, Copper.g, Copper.b, q), 0, .3f); break;
                case "CitrusMarshal": Metal(body, new Color(1f, .78f, .4f, q), 0, .5f); break;
                case "IvorySovereign": Metal(body, new Color(1f, .96f, .86f, q), 0, .35f); break;
                case "PetalCitadel": case "JugglingJester": case "SundaeDuchess": Iris(body, new Color(1f, .86f, .92f, q * .8f)); break;
                case "ArchiveCurator": Iris(body, new Color(.8f, .85f, 1f, q * .7f)); break;
                case "StormFencer": break; // static on the blade (HeroFlair)
                case "MoonProwler": Iris(body, new Color(.75f, .8f, 1f, q * .8f)); break;
                case "SupperMortar": if (!still) SteamPuff(tip + Vector2.up * (.15f + Mathf.Repeat(t * .8f, 1) * .4f), .2f, (1 - Mathf.Repeat(t * .8f, 1)) * .6f); break;
                case "HourglassStag": if (sim.HeroIdentityBankAge >= 0) Refract(Blob, at, Vector2.one * 1.3f, 0, new Color(.9f, .9f, .95f, .6f), .003f, 0, 0, 0, .25f); break; // the debt drains the colour round it
                case "RainArchivist": if (!still) Refract(Blob, at + Vector2.up * (h * .5f + .4f), new Vector2(.8f, .4f), 0, new Color(.7f, .9f, 1f, .5f), .01f, 0, -.6f); break;
                case "KiteAdmiral": Refract(Wedge, at + new Vector2(facing.x < -.05f ? .55f : -.55f, h * .5f + .45f), new Vector2(.7f, .35f), 180, new Color(.85f, .95f, 1f, .5f), .01f, -1, 0); break;
                case "BonsaiClocksmith": case "BellDiver": Metal(body, new Color(Copper.r, Copper.g, Copper.b, q), 0, .35f); break;
            }
            WaitingProps();
        }

        // ------------------------------------------------------------------ the second set of painted objects
        private void WaitingProps()
        {
            UnitState hero = sim.Champion; Vector2 at = DrawnAt(hero), facing = hero.Facing.sqrMagnitude > .01f ? hero.Facing.normalized : Vector2.right;
            float h = hero.Definition.BodyHeight, side = facing.x < -.05f ? -1 : 1, fill = AwakenFill();
            Vector2 head = at + Vector2.up * (h * .5f + .1f), chest = at + Vector2.up * h * .08f;
            int charge = Mathf.FloorToInt(sim.HeroIdentityCharge + .001f); bool primed = sim.HeroAwakenPrimed;
            switch (style.Id)
            {
                case "DrowsySeer": Prop("DrowsySeer_MoonPhase", Mathf.Clamp(Mathf.FloorToInt(fill * 3.99f), 0, 3), head + new Vector2(-side * .35f, .25f + Bob(1)), .28f); break;
                case "GlassDice": Prop("GlassDice_Die", Mathf.Clamp(sim.HeroIdentityNextFace, 1, 6) - 1, head + new Vector2(-side * .4f, .05f + Bob(2) * 2), .26f, still ? 0 : Mathf.Sin(t * (primed ? 20 : 1.5f)) * (primed ? 18 : 8)); break;
                case "ButtonCavalier": for (int i = 0; i < 3; i++) Prop("ButtonCavalier_Buttons", i < charge ? 1 : 0, chest + new Vector2(side * .12f, .18f - i * .14f), .13f); break;
                case "ThornTreasurer": if (charge > 0) Prop("ThornTreasurer_ThornCrown", Mathf.Clamp(charge, 1, 4) - 1, head + Vector2.up * .12f, .32f); break;
                case "IvorySovereign": Prop("IvorySovereign_Decree", primed ? 1 : 0, chest + new Vector2(-side * .42f, -.05f + Bob(1)), .28f, -side * 12, side < 0); break;
                case "ArchiveCurator": Prop("ArchiveCurator_Book", still ? 2 : Mathf.Repeat(t, 4f) < .3f ? 3 : 2, chest + new Vector2(-side * .5f, .05f + Bob(1)), .32f, 0, side < 0); break;
                case "StormFencer": Prop("StormFencer_Sheath", primed ? 2 : fill > .5f ? 1 : 0, at + new Vector2(-side * .22f, -h * .12f), .36f, -side * 20, side < 0); break;
                case "TinVirtuoso": Prop("TinVirtuoso_MusicStand", charge % 3, at + new Vector2(-side * .6f, -h * .15f), .42f, 0, side < 0, 1000 - Mathf.RoundToInt(hero.Position.y * 12f) - 1, false); break;
                case "ChinaMarionette":
                    for (int i = 0; i < Mathf.Clamp(charge, 0, 3); i++) { float a = (still ? 0 : t * 60) + i * 120; Prop("ChinaMarionette_Shard", i, chest + new Vector2(Mathf.Cos(a * Mathf.Deg2Rad) * .55f, .15f + Mathf.Sin(a * Mathf.Deg2Rad) * .2f), .16f, a); }
                    break;
                case "VerdantMatron":
                    foreach (UnitState f in sim.Units)
                        if (f.Alive && f.Faction == Faction.Companion && (f.Definition.Id == "GrandSprout" || f.Definition.Id == "LittleSprout"))
                            Prop("VerdantMatron_Bud", primed ? 2 : fill > .5f ? 1 : 0, DrawnAt(f) + Vector2.up * (f.Definition.BodyHeight * .5f + .15f), .24f, still ? 0 : Mathf.Sin(t * 2.5f + f.Id) * 12);
                    break;
                case "LanternWeaver":
                    for (int i = 1; i < sim.HeroIdentityMarkCount; i++)
                    {
                        UnitState u0 = sim.HeroIdentityMarked(i - 1), u1 = sim.HeroIdentityMarked(i); if (u0 == null || u1 == null || !u0.Alive || !u1.Alive) continue;
                        Prop("LanternWeaver_Lantern", still ? 1 : 1 + Mathf.FloorToInt(t * 2 + i) % 2, (Chest(u0) + Chest(u1)) * .5f - Vector2.up * .12f + Vector2.up * Bob(i), .24f);
                    }
                    break;
                case "MoonProwler":
                    if (sim.HeroIdentityBounty != null && sim.HeroIdentityCharge >= 5)
                    {
                        Vector2 rear = -sim.HeroIdentityRearDirection; if (rear.sqrMagnitude < .01f) rear = Vector2.left;
                        Prop("MoonProwler_ShadowCat", Mathf.FloorToInt(t * 3) % 2, DrawnAt(sim.HeroIdentityBounty) + rear.normalized * .7f - Vector2.up * .2f, .45f, 0, rear.x < 0);
                    }
                    break;
            }
        }
        private void AwakenProps()
        {
            UnitState hero = sim.Champion; HeroKit kit = sim.HeroKitInfo; if (!sim.HeroAwakening || kit == null) return;
            Vector2 at = DrawnAt(hero), facing = hero.Facing.sqrMagnitude > .01f ? hero.Facing.normalized : Vector2.right;
            float h = hero.Definition.BodyHeight, side = facing.x < -.05f ? -1 : 1, since = t - sim.HeroAwakenStarted;
            Vector2 tip = UnitVisual.TryMuzzle(hero.Id, out Vector2 m) ? m : at + facing * .5f, chest = at + Vector2.up * h * .08f;
            bool moving = (hero.Position - hero.PreviousPosition).sqrMagnitude > 1e-5f;
            switch (kit.Action)
            {
                case KitAction.Lighthouse: Prop("LanternKnight_LighthouseLens", Mathf.FloorToInt(since * CombatSimulation.LighthouseSweep / 90f), tip, .36f); break;
                case KitAction.Mitosis: // each jelly carries a remembered organ
                    if (dreamEchoes != null) foreach (var e in dreamEchoes) if (e != null && e.enabled) Prop("LoneJelly_Organs", Mathf.Clamp(sim.HeroIdentityApproach, 0, 2), (Vector2)e.bounds.center, .26f);
                    break;
                case KitAction.Rink: if (moving) Prop("SoapSkater_IceChips", Mathf.FloorToInt(t * 10) % 3, at - Vector2.up * h * .4f - facing * .45f, .34f, 0, side < 0); break;
                case KitAction.Parade:
                    foreach (UnitState f in sim.Units) if (f.Alive && f.Faction == Faction.Companion) Prop("VelvetRegent_Banner", 1 + Mathf.FloorToInt(t * 3 + f.Id) % 2, DrawnAt(f) + new Vector2(.25f, f.Definition.BodyHeight * .5f + .35f), .34f);
                    break;
                case KitAction.Feast: Prop("NightMaw_Jaw", Mathf.FloorToInt(t * 2.5f) % 3, at + new Vector2(side * (h * .75f), .05f), h * 1.05f, 0, side < 0); break;
                case KitAction.Coronation: Prop("IvorySovereign_Decree", since < .3f ? 1 : 2, at + Vector2.up * (h * .5f + .75f + Bob(1)), .6f); break;
                case KitAction.Showtime:
                {
                    UnitState lead = sim.HeroIdentityLead != null && sim.HeroIdentityLead.Alive ? sim.HeroIdentityLead : hero;
                    Prop("PatchworkHost_Curtain", since < .25f ? 0 : since < .55f ? 1 : 2, DrawnAt(lead) + Vector2.up * (lead.Definition.BodyHeight * .5f + .55f), .75f);
                    break;
                }
                case KitAction.Exhibit: Prop("ArchiveCurator_Book", Mathf.Repeat(t, 1.2f) < .3f ? 3 : 2, chest + new Vector2(-side * .55f, .2f + Bob(1)), .42f, 0, side < 0); break;
                case KitAction.Thunder: Prop("StormFencer_Sheath", 2, at + new Vector2(-side * .22f, -h * .12f), .36f, -side * 20, side < 0); break;
                case KitAction.Symphony: Prop("TinVirtuoso_MusicStand", Mathf.FloorToInt(t) % 3, at + new Vector2(-side * .6f, -h * .15f), .42f, 0, side < 0, 1000 - Mathf.RoundToInt(hero.Position.y * 12f) - 1, false); break;
                case KitAction.Eclipse: if (sim.HeroAwakenFoe != null && sim.HeroAwakenFoe.Alive) Prop("MoonProwler_ShadowCat", 2, DrawnAt(sim.HeroAwakenFoe) - facing * .6f, .5f, 0, side > 0); break;
                case KitAction.Monsoon: Prop("RainArchivist_Cloud", 1 + Mathf.FloorToInt(t * 2) % 2, at + Vector2.up * (h * .5f + 1f + Bob(1)), .8f); break;
                case KitAction.Throne: Prop("ThornTreasurer_ThornCrown", 3, at + Vector2.up * (h * .5f + .2f), .36f); break;
                case KitAction.Kintsugi: for (int i = 0; i < 3; i++) { float a = (still ? 0 : t * 90) + i * 120; Prop("ChinaMarionette_Shard", i, chest + new Vector2(Mathf.Cos(a * Mathf.Deg2Rad) * .6f, .15f + Mathf.Sin(a * Mathf.Deg2Rad) * .22f), .18f, a); } break;
                case KitAction.Dreams: Prop("DrowsySeer_MoonPhase", 3, at + Vector2.up * (h * .5f + .5f + Bob(0)), .4f); break;
            }
        }
        /// <summary>Painted objects that ride on a strike: the root tip breaking the floor, the wave's crest, the comet, the jaws,
        /// the receipt, the cloud over the rain, the cherry on a landed scoop.</summary>
        private void StrikeProps(HeroIdentityGlyph g, Vector2 at, float p, float fade)
        {
            if (fade <= .05f) return;
            Vector2 up = at + Vector2.up * .2f; int f3 = p < .33f ? 0 : p < .66f ? 1 : 2;
            switch (style.Id)
            {
                case "RootOracle": Prop("RootOracle_RootTip", f3, at + Vector2.up * .25f, .45f * sigScale); break;
                case "TideSinger": Prop("TideSinger_WaveCrest", f3, up, .6f * sigScale, 0, g.Direction.x < 0); break;
                case "CometRunner": if (p < .4f) Prop("CometRunner_Comet", Mathf.FloorToInt(t * 12), up + Vector2.up * (1 - p / .4f) * 1.2f, .45f * sigScale, -60); break;
                case "NightMaw": Prop("NightMaw_Jaw", p < .15f ? 1 : 2, up, .6f * sigScale); break;
                case "PennyBroker": Prop("PennyBroker_Receipt", f3, up + Vector2.up * .1f, .32f * sigScale, -8); break;
                case "RainArchivist": if (p < .6f) Prop("RainArchivist_Cloud", p < .2f ? 0 : 2, at + Vector2.up * 1.3f, .55f * sigScale); break;
                case "SundaeDuchess": if (g.Finale || g.Radius >= .8f) Prop("SundaeDuchess_Cherry", p < .2f ? 1 : 0, up + Vector2.up * (.15f + (still ? 0 : Mathf.Abs(Mathf.Sin(p * Mathf.PI * 2)) * (1 - p) * .4f)), .22f * sigScale); break;
                case "ChinaMarionette": if (p < .5f) Prop("ChinaMarionette_Shard", g.Seed % 3, up, .22f * sigScale, g.Seed * 40); break;
                case "VelvetRegent": if (p < .4f) Prop("VelvetRegent_Banner", 2, up + Vector2.up * .3f, .32f * sigScale); break;
                case "ButtonCavalier": if (p < .6f) Prop("ButtonCavalier_Buttons", 2, up + Vector2.up * p * .6f, .16f * sigScale, p * 540); break;
                case "LoneJelly": if (p < .5f) Prop("LoneJelly_Organs", Mathf.Clamp(sim.HeroIdentityApproach, 0, 2), up, .3f * sigScale); break;
            }
        }
    }
}
