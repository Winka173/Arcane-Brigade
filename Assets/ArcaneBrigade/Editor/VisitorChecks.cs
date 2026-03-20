using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>The 45 shop visitors (Docs/Events_Ideas.md): each one, forced into a shop, offers something that can be taken;
    /// taking it changes only what it says (this shop, or the next wave), never heals, and every next-wave effect reaches the
    /// fight and is gone after it.</summary>
    public static class VisitorChecks
    {
        private static readonly List<string> notes = new List<string>();
        private static void Check(bool ok, string caption) { if (!ok) throw new InvalidOperationException("Visitors: " + caption); notes.Add("PASS " + caption); }

        [MenuItem("Arcane Brigade/Checks/Shop visitors")]
        public static void Run()
        {
            notes.Clear();
            RunCatalog c = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            Check(ShopVisitors.All.Length == 45 && ShopVisitors.All.Select(v => v.Id).Distinct().Count() == 45, "45 distinct visitors");
            Check(ShopVisitors.All.All(v => v.Line.Length > 5 && v.Offer.Length > 10 && !System.Text.RegularExpressions.Regex.IsMatch(v.Offer + " " + v.Line, @"\bheal(s|ed|ing)?\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase)), "Every visitor speaks, states its offer, and none heals");
            var paint = RunSession.VisitorPainted; RunSession.VisitorPainted = id => true;
            try
            {
                foreach (VisitorDefinition v in ShopVisitors.All)
                {
                    using var run = Fresh(c);
                    int gold = run.State.Gold, eggs = run.State.Eggs; float health = run.State.Health;
                    run.ForceVisitor(v.Id);
                    var options = run.VisitorOptions();
                    Check(options.Count > 0 && options.Any(o => o.Enabled), v.Id + " offers something that can be taken");
                    int pick = options.FindIndex(o => o.Enabled);
                    Check(run.VisitorChoose(pick), v.Id + " is answered");
                    if (run.Visitor != null) { var second = run.VisitorOptions(); Check(second.Count > 0 && run.VisitorChoose(second.FindIndex(o => o.Enabled)), v.Id + " second step"); }
                    Check(run.Visitor == null && run.State.Health <= health + .001f, v.Id + " leaves, and nobody was healed");
                    Check(run.State.Gold <= gold + 60 && run.State.Eggs <= eggs + 2, v.Id + " pays only small amounts (" + (run.State.Gold - gold) + " gold, " + (run.State.Eggs - eggs) + " eggs)");
                    // the next wave: prepare, fight briefly, settle - every next-wave effect is used up
                    run.ContinueWave();
                    var arena = run.PrepareCombat(); arena.WaveDuration = 6; arena.Boss = null;
                    var sim = new CombatSimulation(arena); sim.Start(4242);
                    for (int t = 0; t < 400 && sim.Phase == RunPhase.Combat; t++) { sim.Champion.Health = sim.Champion.Definition.Health; sim.Step(.05f, Vector2.right * Mathf.Sin(t * .05f)); }
                    if (sim.Phase == RunPhase.Combat) sim.ForceVictoryForChecks();
                    run.Settle(sim);
                    Check(run.State.VisitorEffects.All(e => e.Wave > run.State.Wave || e.Id == "Pawn" || e.Id == "Promise"), v.Id + " leaves nothing behind after its wave");
                }
                // the visits come every 2-3 shops from wave 3, from the visitors' own roll
                using (var run = Fresh(c))
                {
                    int seen = 0;
                    for (int wave = 1; wave <= 20; wave++)
                    {
                        run.State.Wave = wave; run.State.Phase = SessionPhase.Shop;
                        typeof(RunSession).GetMethod("VisitorNewShop", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(run, null);
                        if (run.Visitor != null) { seen++; Check(wave >= 3, "No visitor before wave 3"); run.VisitorDecline(); }
                    }
                    Check(seen >= 5 && seen <= 9, "Visitors come every 2-3 shops (" + seen + " in 20)");
                }
            }
            finally { RunSession.VisitorPainted = paint; }
            Directory.CreateDirectory("PreviewExports/Visitors");
            File.WriteAllLines("PreviewExports/Visitors/Checks.txt", notes);
            Debug.Log("VISITOR_CHECKS_PASSED " + notes.Count);
        }
        private static RunSession Fresh(RunCatalog c)
        {
            var run = new RunSession(c); run.Start("LanternKnight", 7171);
            run.State.Wave = 6; run.State.Phase = SessionPhase.Shop; run.State.Gold = 200; run.State.Eggs = 4;
            run.State.Items.Add(new ItemStack { Id = "key-oil", Count = 1 }); run.State.Items.Add(new ItemStack { Id = "honey-pot", Count = 1 });
            run.Reroll(); run.Reroll(); run.State.Gold = 200; // a shelf, then a refresh to remember it
            return run;
        }
    }
}
