using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Hero kits (18/10/2026): every hero has one mechanic (a gauge that climbs three tiers from the hero's own actions
    /// and transforms the hero by itself, then rests), one or two hero passives and two to four team or run passives. Checks
    /// every hero's transformation in a real simulation, a few transformations' own behaviour, and the run passives in a session.</summary>
    public static class HeroAwakeningChecks
    {
        [MenuItem("Arcane Brigade/Checks/Hero awakenings")]
        public static void Run()
        {
            var notes = new List<string>();
            void Require(bool condition, string caption) { if (!condition) throw new InvalidOperationException("HeroAwakening " + caption); notes.Add("PASS " + caption); }
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            FieldInfo charge = typeof(CombatSimulation).GetField("identityCharge", flags);

            // the kits themselves
            Require(HeroKits.All.Length == catalog.Champions.Length && catalog.Champions.All(h => HeroKits.For(h.Id) != null), "Every hero has a kit (" + HeroKits.All.Length + ")");
            foreach (HeroKit kit in HeroKits.All)
            {
                int hero = kit.HeroTraits.Count(), team = kit.TeamTraits.Count();
                Require(hero >= 1 && hero <= 2 && team >= 2 && team <= 4, kit.Id + " has 1-2 hero passives and 2-4 team or run passives (" + hero + "/" + team + ")");
                Require(!string.IsNullOrEmpty(kit.Transformation) && kit.Effect.Length > 40 && HeroKits.Describe(kit).Contains(kit.Transformation), kit.Id + " describes its transformation");
            }
            Require(HeroKits.All.Select(k => k.ArtKey).Distinct().Count() == HeroKits.All.Length && HeroKits.All.Select(k => k.Transformation).Distinct().Count() == HeroKits.All.Length, "Every transformation and its art is the hero's own");
            string requests = File.Exists("Tools/Art/hero_awakenings.json") ? File.ReadAllText("Tools/Art/hero_awakenings.json") : "";
            foreach (HeroKit kit in HeroKits.All.Where(k => requests.Contains("\"" + k.Id + "\": {")))
                Require(kit.Held == RequestHeld(requests, kit.Id), kit.Id + " expects the kind of art that was requested (held weapon or object)");

            foreach (UnitDefinition definition in catalog.Champions)
            {
                string hero = definition.Id; HeroKit kit = HeroKits.For(hero);
                using (var run = new RunSession(catalog))
                {
                    run.Start(hero, 18020); run.State.Wave = 4;
                    var sim = new CombatSimulation(run.PrepareCombat()); sim.Start(18020);
                    Require(sim.HeroAwakenEnabled && sim.HeroAwakenMeter == 0, hero + " starts with an empty gauge");
                    UnitState champion = sim.Champion;
                    Vector2 walk = Vector2.right;
                    void Step(float seconds)
                    {
                        for (float s = 0; s < seconds; s += 1 / 60f)
                        {
                            champion.Health = champion.Definition.Health;
                            if (champion.Position.x > 4) walk = Vector2.left; else if (champion.Position.x < -4) walk = Vector2.right;
                            sim.Step(1 / 60f, walk);
                        }
                    }
                    // one full charge climbs a tier; three transform once an enemy is near
                    charge.SetValue(sim, sim.HeroIdentityRequired + .01f); sim.Step(1 / 60f, Vector2.zero);
                    Require(sim.HeroAwakenTier >= 1 || sim.HeroAwakening, hero + ": a full charge climbs a tier");
                    int activations = sim.HeroIdentityActivations;
                    for (int i = 0; i < 900 && !sim.HeroAwakening; i++) { if (sim.HeroAwakenTier < 3) charge.SetValue(sim, sim.HeroIdentityRequired + .01f); Step(1 / 60f); }
                    Require(sim.HeroAwakening && sim.HeroIdentityActivations > activations, hero + ": a full gauge transforms by itself (" + kit.Transformation + ")");
                    Require(champion.AwakenArt == (kit.Held ? kit.ArtKey : null), hero + " holds its transformed weapon only when it has one");
                    if (hero == "Postmaster")
                    {
                        int basics = 0, muzzles = 0;
                        for (float s = 0; s < 1.5f; s += 1 / 60f)
                        {
                            champion.Health = champion.Definition.Health; sim.Step(1 / 60f, walk);
                            foreach (var e in sim.Events) if (e.Kind == CombatEventKind.Fire && e.Subject == champion) { if (e.Cause.ItemId == null || !e.Cause.ItemId.StartsWith("hero-identity-")) basics++; else muzzles++; }
                        }
                        Require(basics == 0 && muzzles >= 5 && sim.HeroAwakenAim.sqrMagnitude > .5f, "While the sorting gatling runs it is the only gun, each shot a muzzle flash (" + muzzles + " shots, " + basics + " basics, aim " + sim.HeroAwakenAim + ", awake " + sim.HeroAwakening + ")");
                    }
                    if (hero == "CopperCauldron")
                    {
                        Vector2 from = champion.Position; walk = champion.Position.x > 0 ? Vector2.left : Vector2.right;
                        sim.Step(1 / 60f, walk);
                        Require(Vector2.Distance(champion.Position, from) > champion.Definition.Speed / 60f * 1.4f, "The steam engine runs faster than the kettle walks");
                        Step(1.5f); Require(sim.HeroAwakenPressure > .15f, "Boiler pressure rises while the engine runs");
                    }
                    if (hero == "DrowsySeer")
                    {
                        Step(2.5f);
                        var echoes = sim.HeroAwakenEchoes;
                        Require(echoes.Count == 3 && (echoes[2] - champion.Position).sqrMagnitude > (echoes[0] - champion.Position).sqrMagnitude, "Three dreams walk the seer's route, the oldest furthest behind");
                    }
                    for (int i = 0; i < 12 * 60 && sim.HeroAwakening; i++) Step(1 / 60f);
                    Require(!sim.HeroAwakening && sim.HeroAwakenCooling && sim.HeroAwakenMeter < 1 && champion.AwakenArt == null, hero + ": the transformation ends and rests");
                    Require(sim.Units.All(u => float.IsFinite(u.Position.x) && float.IsFinite(u.Position.y)), hero + ": everything stays finite");
                }
            }

            // run passives in a session
            using (var run = new RunSession(catalog))
            {
                run.Start("CopperCauldron", 18022); run.State.Wave = 3; run.State.Phase = SessionPhase.Shop; run.State.Gold = 200; run.State.RerollsThisShop = 0;
                Require(run.RerollCost == 0, "CopperCauldron: the first reroll in a shop is free");
                run.Reroll(); Require(run.RerollCost > 0, "CopperCauldron: later rerolls cost again");
            }
            using (var run = new RunSession(catalog))
            {
                run.Start("Postmaster", 18023); run.State.Wave = 3; run.State.Phase = SessionPhase.Shop; run.State.Gold = 500;
                typeof(RunSession).GetMethod("RollShop", flags).Invoke(run, new object[] { true });
                ShopOffer offer = run.State.Offers.First(o => o.Currency == ShopCurrency.Gold);
                Require(offer.BasePrice > 0 && offer.Price <= Mathf.CeilToInt(offer.BasePrice * .8f) + 1, "Postmaster: the first purchase of a shop is 20% cheaper (" + offer.BasePrice + " -> " + offer.Price + ")");
            }
            using (var run = new RunSession(catalog))
            {
                run.Start("DrowsySeer", 18024); run.State.Wave = 3; run.State.Phase = SessionPhase.Shop; run.State.Gold = 500;
                typeof(RunSession).GetMethod("RollShop", flags).Invoke(run, new object[] { true });
                Require(run.State.Offers.Count == catalog.ShopSlots + 1, "DrowsySeer: every shop shows one more offer");
            }
            using (var run = new RunSession(catalog))
            {
                run.Start("VelvetRegent", 18025);
                Require(run.NewRecruitLimit == 2, "VelvetRegent: one more new species a wave");
            }
            using (var run = new RunSession(catalog))
            {
                run.Start("LanternKnight", 18026); run.State.Roster.Clear();
                run.State.Roster.Add(new RosterEntry { UnitId = "TeapotPilgrim", WeaponId = "InkPistol", Tier = 1 });
                typeof(RunSession).GetMethod("RecountBonds", flags).Invoke(run, null);
                string[] tags = catalog.Unit("TeapotPilgrim").Tags;
                Require(tags.Length == 0 || run.BondCounts.Values.Max() >= 2, "LanternKnight: the largest bond counts one extra member");
            }
            Directory.CreateDirectory("PreviewExports/HeroAwakening");
            File.WriteAllLines("PreviewExports/HeroAwakening/Checks.txt", notes);
            Debug.Log("HERO_AWAKENING_CHECKS_PASSED " + notes.Count);
        }

        private static bool RequestHeld(string json, string id)
        {
            int at = json.IndexOf("\"" + id + "\": {", StringComparison.Ordinal);
            int kind = json.IndexOf("\"kind\":", at, StringComparison.Ordinal);
            return json.Substring(kind, 20).Contains("held");
        }
    }
}
