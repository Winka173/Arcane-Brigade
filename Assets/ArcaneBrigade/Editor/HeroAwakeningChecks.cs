using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>The pilot awakenings (Postmaster, CopperCauldron, DrowsySeer): three tiers climbed by mechanic casts, an
    /// automatic six-second transformation, a three-second cool-down, and each hero's own transformation.</summary>
    public static class HeroAwakeningChecks
    {
        [MenuItem("Arcane Brigade/Checks/Hero awakenings")]
        public static void Run()
        {
            var notes = new List<string>();
            void Require(bool condition, string caption) { if (!condition) throw new InvalidOperationException("HeroAwakening " + caption); notes.Add("PASS " + caption); }
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            MethodInfo cast = typeof(CombatSimulation).GetMethod("IdentityCast", BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (string hero in new[] { "Postmaster", "CopperCauldron", "DrowsySeer" })
            {
                using (var run = new RunSession(catalog))
                {
                    run.Start(hero, 18020); run.State.Wave = 4;
                    var sim = new CombatSimulation(run.PrepareCombat()); sim.Start(18020);
                    Require(sim.HeroAwakenEnabled && sim.HeroAwakenMeter == 0, hero + " has an awakening, starting empty");
                    cast.Invoke(sim, null);
                    Require(sim.HeroAwakenTier == 1 && !sim.HeroAwakening, hero + ": a cast climbs one tier");
                    cast.Invoke(sim, null); cast.Invoke(sim, null);
                    Require(sim.HeroAwakenTier == 3, hero + ": three casts fill the gauge");
                    UnitState champion = sim.Champion;
                    float clock = 0; Vector2 walk = Vector2.right;
                    void Step(float seconds)
                    {
                        for (float s = 0; s < seconds; s += 1 / 60f)
                        {
                            champion.Health = champion.Definition.Health;
                            if (champion.Position.x > 4) walk = Vector2.left; else if (champion.Position.x < -4) walk = Vector2.right;
                            sim.Step(1 / 60f, walk); clock += 1 / 60f;
                        }
                    }
                    for (int i = 0; i < 600 && !sim.HeroAwakening; i++) Step(1 / 60f);
                    Require(sim.HeroAwakening, hero + ": a full gauge transforms by itself once an enemy is near");
                    if (hero == "Postmaster")
                    {
                        Require(champion.AwakenArt == "Postmaster_Awake", "Postmaster holds the sorting gatling");
                        int before = sim.Projectiles.Count; Step(.5f);
                        Require(sim.Projectiles.Count > before || sim.HeroIdentityMarkCount > 0, "The sorting gatling fires parcels and stamps addresses");
                        int basics = 0, muzzles = 0;
                        for (float s = 0; s < 1.5f; s += 1 / 60f)
                        {
                            champion.Health = champion.Definition.Health; sim.Step(1 / 60f, walk);
                            foreach (var e in sim.Events) if (e.Kind == CombatEventKind.Fire && e.Subject == champion) { if (e.Cause.ItemId == null || !e.Cause.ItemId.StartsWith("hero-identity-")) basics++; else muzzles++; }
                        }
                        Require(basics == 0 && sim.HeroAwakenAim.sqrMagnitude > .5f, "While the gatling runs it is the only gun: no basic attacks, and it aims each parcel");
                        Require(muzzles >= 5, "Every gatling parcel is a shot the barrel flashes and sounds for (" + muzzles + " in 1.5 s)");
                    }
                    if (hero == "CopperCauldron")
                    {
                        Vector2 from = champion.Position; walk = champion.Position.x > 0 ? Vector2.left : Vector2.right;
                        sim.Step(1 / 60f, walk);
                        float moved = Vector2.Distance(champion.Position, from);
                        Require(moved > champion.Definition.Speed / 60f * 1.4f, "The steam engine runs faster than the kettle walks");
                        Step(1.5f); Require(sim.HeroAwakenPressure > 0.15f, "Boiler pressure rises while the engine runs");
                    }
                    if (hero == "DrowsySeer")
                    {
                        Step(2.5f);
                        var echoes = sim.HeroAwakenEchoes;
                        Require(echoes.Count == 3 && (echoes[2] - champion.Position).sqrMagnitude > (echoes[0] - champion.Position).sqrMagnitude, "Three dreams walk the seer's route, the oldest furthest behind");
                    }
                    Step(CombatSimulation.AwakenSeconds + .3f);
                    Require(!sim.HeroAwakening && sim.HeroAwakenCooling && sim.HeroAwakenMeter == 0 && champion.AwakenArt == null, hero + ": the transformation ends and cools");
                    cast.Invoke(sim, null);
                    Require(sim.HeroAwakenMeter == 0, hero + ": casts do not climb while cooling");
                    Step(CombatSimulation.AwakenCooldown + .1f);
                    float before2 = sim.HeroAwakenMeter; cast.Invoke(sim, null);
                    Require(sim.HeroAwakenMeter > before2 + .3f || sim.HeroAwakening, hero + ": after cooling the gauge climbs again");
                }
            }
            using (var run = new RunSession(catalog))
            {
                run.Start("LanternKnight", 18021); run.State.Wave = 4;
                var sim = new CombatSimulation(run.PrepareCombat()); sim.Start(18021);
                Require(!sim.HeroAwakenEnabled, "Heroes without an awakening yet keep their usual gauge");
            }
            System.IO.Directory.CreateDirectory("PreviewExports/HeroAwakening");
            System.IO.File.WriteAllLines("PreviewExports/HeroAwakening/Checks.txt", notes);
            Debug.Log("HERO_AWAKENING_CHECKS_PASSED " + notes.Count);
        }
    }
}
