using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>The 45 shop visitors (Docs/Events_Ideas.md): each one, forced into a shop, offers something that can be taken;
    /// taking it changes only what it says (this shop, or the next wave), never heals, and every next-wave effect reaches the
    /// fight and is gone after it. Their cards (feedback 19/10/2026) summarise every choice as DO / PAY / GET / WHEN, short and
    /// iconic, and the numbers on the card are the numbers the rules pay.</summary>
    public static class VisitorChecks
    {
        private static readonly List<string> notes = new List<string>();
        private static void Check(bool ok, string caption) { if (!ok) throw new InvalidOperationException("Visitors: " + caption); notes.Add("PASS " + caption); }
        private static readonly Regex healing = new Regex(@"\bheal(s|ed|ing)?\b|\bregen", RegexOptions.IgnoreCase);
        private static readonly Regex lasting = new Regex(@"\b(forever|permanent(ly)?|always|every wave|rest of the run|this run)\b", RegexOptions.IgnoreCase);
        private static readonly string[] defensive = { "Health", "Heal", "HeartHealing", "HealthCorrection", "HealCorrection", "Armor", "ArmorCorrection", "Dodge", "Interceptor" };

        [MenuItem("Arcane Brigade/Checks/Shop visitors")]
        public static void Run()
        {
            notes.Clear();
            RunCatalog c = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            Check(ShopVisitors.All.Length == 45 && ShopVisitors.All.Select(v => v.Id).Distinct().Count() == 45, "45 distinct visitors");
            Check(ShopVisitors.All.All(v => v.Line.Length > 5 && v.Offer.Length > 10 && !healing.IsMatch(v.Offer + " " + v.Line)), "Every visitor speaks, states its offer, and none heals");
            CardSummaries();
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
                    VisitorDeal deal = ShopVisitors.DealFor(v, v.Choice == VisitorChoice.Options ? options[pick].Value : 0, run.State.Wave, run.GoldScale);
                    int effectsBefore = run.State.VisitorEffects.Count, waveBefore = run.State.Wave;
                    Check(run.VisitorChoose(pick), v.Id + " is answered");
                    if (run.Visitor != null) { var second = run.VisitorOptions(); Check(second.Count > 0 && run.VisitorChoose(second.FindIndex(o => o.Enabled)), v.Id + " second step"); }
                    Check(run.Visitor == null && run.State.Health <= health + .001f, v.Id + " leaves, and nobody was healed");
                    Check(run.State.Gold <= gold + run.ScaledGold(60) && run.State.Eggs <= eggs + 2, v.Id + " pays only small amounts (" + (run.State.Gold - gold) + " gold, " + (run.State.Eggs - eggs) + " eggs)");
                    // the card's WHEN agrees with the rules: an effect waiting for a later wave or shop is announced with its time
                    var added = run.State.VisitorEffects.Skip(effectsBefore).ToList();
                    if (added.Any(e => e.Wave > waveBefore) || run.State.TwinShop == waveBefore)
                        Check(deal.When.Length > 0 && Regex.IsMatch(deal.When, @"\b(wave|waves|shop)\b"), v.Id + " card says when its effect happens: " + deal.When);
                    // a visitor's price on the card is the price the rules take
                    int paid = ShopVisitors.Amount(deal.Pay, "gold");
                    if ((v.Choice == VisitorChoice.Accept || v.Choice == VisitorChoice.PickCompanion) && v.Id != "ParcelCourier" && v.Id != "ClockRewinder")
                        Check(gold - run.State.Gold == paid, v.Id + " takes the gold its card shows (" + paid + ")");
                    // play-test 20/10/2026: the world tag, the card and the rules read one price, on the shop's gold curve
                    Check(paid == 0 || paid == ShopVisitors.Scale(v.Cost, run.GoldScale) || v.Choice == VisitorChoice.Options, v.Id + " price follows the gold curve (" + paid + " at x" + run.GoldScale.ToString("0.##") + ")");
                    // the next wave: prepare, fight briefly, settle - every next-wave effect is used up
                    run.ContinueWave();
                    var arena = run.PrepareCombat(); arena.WaveDuration = 6; arena.Boss = null;
                    var sim = new CombatSimulation(arena); sim.Start(4242);
                    for (int t = 0; t < 400 && sim.Phase == RunPhase.Combat; t++) { sim.Champion.Health = sim.Champion.Definition.Health; sim.Step(.05f, Vector2.right * Mathf.Sin(t * .05f)); }
                    if (sim.Phase == RunPhase.Combat) sim.ForceVictoryForChecks();
                    run.Settle(sim);
                    Check(run.State.VisitorEffects.All(e => e.Wave > run.State.Wave || e.Id == "Pawn" || e.Id == "Promise"), v.Id + " leaves nothing behind after its wave");
                }
                CardNumbers(c);
                // the visits come every 2-3 shops from wave 3, from the visitors' own roll
                using (var run = Fresh(c))
                {
                    int seen = 0;
                    for (int wave = 1; wave <= 20; wave++)
                    {
                        run.State.Wave = wave; run.State.Phase = SessionPhase.Shop;
                        typeof(RunSession).GetMethod("VisitorNewShop", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(run, null);
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

        /// <summary>Every card summary: one deal per option (or one for the offer), a short verb, a GET, a PAY or an explicit
        /// "free", known icons, short amounts, a WHEN that never outlasts the next waves, and no healing or defensive stat.</summary>
        private static void CardSummaries()
        {
            foreach (VisitorDefinition v in ShopVisitors.All)
            {
                int want = v.Choice == VisitorChoice.Options ? v.Options.Length : 1;
                Check(v.Deals.Length == want, v.Id + " has a card summary for each choice (" + v.Deals.Length + "/" + want + ")");
                foreach (VisitorDeal d in v.Deals)
                {
                    string all = d.Do + " | " + d.Pay + " | " + d.Get + " | " + d.When;
                    Check(!string.IsNullOrWhiteSpace(d.Do) && d.Do.Length <= 20 && d.Do.Split(' ').Length <= 3, v.Id + " DO is a short verb: " + d.Do);
                    Check(!string.IsNullOrWhiteSpace(d.Get) && !d.Get.Equals(VisitorDeal.Free) && ShopVisitors.Terms(d.Get).Count > 0, v.Id + " GET is filled: " + d.Get);
                    Check(d.IsFree || ShopVisitors.Terms(d.Pay).Count > 0, v.Id + " PAY is filled or explicitly free: " + d.Pay);
                    var terms = ShopVisitors.Terms(d.Pay).Concat(ShopVisitors.Terms(d.Get)).ToList();
                    Check(terms.All(t => ShopVisitors.KnownIcon(t.Icon) && t.Amount.Length > 0 && t.Amount.Length <= 28), v.Id + " every term has a known icon and a short amount: " + all);
                    Check(terms.All(t => !defensive.Contains(t.Icon)), v.Id + " gives no healing, armor or dodge");
                    Check(!healing.IsMatch(all) && !lasting.IsMatch(all) && d.When.Length <= 30, v.Id + " card has no healing words and nothing lasting: " + all);
                }
                // the card shows the price the visitor asks
                if (v.Cost > 0 && v.Choice != VisitorChoice.Options) Check(ShopVisitors.Amount(v.Deals[0].Pay, "gold") == v.Cost, v.Id + " card price is its cost (" + v.Cost + ")");
                if (v.Cost == 0 && v.Choice == VisitorChoice.Accept) Check(ShopVisitors.Amount(v.Deals[0].Pay, "gold") == 0, v.Id + " free visitor shows no gold price");
            }
            VisitorDefinition gate = ShopVisitors.For("GateKeeper");
            Check(ShopVisitors.DealFor(gate, 0, 6).Get == "relic:Common" && ShopVisitors.DealFor(gate, 0, 11).Get == "relic:Uncommon", "Gatekeeper card shows Common, Uncommon for a dared wave 12+");
        }

        /// <summary>The numbers on representative cards are the numbers the rules pay.</summary>
        private static void CardNumbers(RunCatalog c)
        {
            MethodInfo settleVisitors = typeof(RunSession).GetMethod("SettleVisitors", BindingFlags.Instance | BindingFlags.NonPublic);
            // Dice Elder: PAY is the bet taken now; GET is what the elder pays after a won wave above half health
            VisitorDefinition dice = ShopVisitors.For("DiceElder");
            for (int i = 0; i < 3; i++)
            {
                using var run = Fresh(c); run.ForceVisitor("DiceElder");
                VisitorDeal d = ShopVisitors.DealFor(dice, i, 6, run.GoldScale);
                run.State.Gold = ShopVisitors.Amount(d.Pay, "gold") - 1;
                Check(!run.VisitorOptions()[i].Enabled, "Dice Elder bet " + i + " is closed with less gold than its PAY");
                run.State.Gold = 2000;
                Check(run.VisitorChoose(i) && 2000 - run.State.Gold == ShopVisitors.Amount(d.Pay, "gold"), "Dice Elder bet " + i + " takes PAY " + d.Pay);
                CombatSimulation sim = Fight(run);
                Check(sim.Phase == RunPhase.Victory && sim.LowestChampionShare >= .5f, "Dice Elder fixture wave is won above half health");
                int before = run.State.Gold; settleVisitors.Invoke(run, new object[] { sim });
                Check(run.State.Gold - before == ShopVisitors.Amount(d.Get, "gold"), "Dice Elder bet " + i + " pays GET " + d.Get + " (" + (run.State.Gold - before) + ")");
            }
            // Parcel: PAY 10 gold, GET exactly one of 15 gold / an egg / a Common relic / nothing
            VisitorDeal parcel; using (var probe = Fresh(c)) parcel = ShopVisitors.DealFor(ShopVisitors.For("ParcelCourier"), 0, 6, probe.GoldScale);
            int fee = ShopVisitors.Amount(parcel.Pay, "gold");
            var seen = new HashSet<string>();
            for (uint seed = 1; seed < 40; seed += 3)
            {
                using var run = Fresh(c); run.ForceVisitor("ParcelCourier"); run.State.VisitorRandom = seed * 2654435761u | 1;
                int gold = run.State.Gold, eggs = run.State.Eggs, relics = run.State.Items.Sum(s => s.Count);
                var had = run.State.Items.ToDictionary(s => s.Id, s => s.Count);
                Check(run.VisitorChoose(0), "Parcel opens");
                int dg = run.State.Gold - gold, de = run.State.Eggs - eggs, dr = run.State.Items.Sum(s => s.Count) - relics;
                ItemStack fresh = run.State.Items.FirstOrDefault(s => s.Count > (had.TryGetValue(s.Id, out int n) ? n : 0));
                ItemDefinition got = dr == 1 && fresh != null ? c.Item(fresh.Id) : null;
                string outcome = dg == -fee + ShopVisitors.Amount(parcel.Get, "gold") && de == 0 && dr == 0 ? "gold" : dg == -fee && de == ShopVisitors.Amount(parcel.Get, "egg") && dr == 0 ? "egg"
                    : dg == -fee && de == 0 && dr == 1 && got != null && got.Rarity == ItemRarity.Common ? "relic" : dg == -fee && de == 0 && dr == 0 ? "empty" : dg == 0 && de == 0 && dr == 0 ? "fallback" : "unexpected " + dg + "/" + de + "/" + dr;
                Check(!outcome.StartsWith("unexpected"), "Parcel outcome is on its card: " + outcome);
                seen.Add(outcome);
            }
            Check(fee > 10 && parcel.Get.Contains("relic:Common") && seen.Count >= 3, "Parcel card: PAY " + fee + " gold (10 on the wave-6 curve), GET gold / egg / Common relic / empty (" + string.Join(",", seen) + ")");
            // Junk Collector and Pawnbroker: each relic plate shows the gold it pays, and choosing pays exactly that
            foreach (string id in new[] { "JunkCollector", "Pawnbroker" })
            {
                using var run = Fresh(c); run.ForceVisitor(id);
                VisitorDeal d = ShopVisitors.DealFor(ShopVisitors.For(id), 0, run.State.Wave, run.GoldScale);
                var options = run.VisitorOptions();
                foreach (VisitorOption o in options)
                {
                    ItemDefinition item = c.Item(run.State.Items[o.Value].Id);
                    int want = run.ScaledGold(id == "JunkCollector" ? Mathf.RoundToInt(item.Price * .6f) : item.Price / 2);
                    Check(o.Gold == want && o.Label.EndsWith("(+" + want + ")"), id + " plate for " + item.Name + " shows +" + want);
                }
                int gold = run.State.Gold, relics = run.State.Items.Sum(s => s.Count), shown = options[0].Gold, wave = run.State.Wave;
                Check(run.VisitorChoose(0) && run.State.Gold - gold == shown && run.State.Items.Sum(s => s.Count) == relics - 1, id + " pays the plate's gold for one relic");
                Check(d.Pay == "relic:1 relic" && d.Get.StartsWith("gold:") && d.Get.Contains(id == "JunkCollector" ? "60%" : "half"), id + " card: PAY a relic, GET " + d.Get);
                if (id == "Pawnbroker") Check(run.State.VisitorEffects.Any(e => e.Id == "Pawn" && e.Wave == wave + 2) && d.When.Contains("2 waves"), "Pawnbroker: buy back within 2 waves, as the card says");
            }
            // Lender: free, a Rare relic, for the next wave only
            {
                using var run = Fresh(c); run.ForceVisitor("Lender");
                VisitorDeal d = ShopVisitors.DealFor(ShopVisitors.For("Lender"), 0, run.State.Wave);
                int gold = run.State.Gold, wave = run.State.Wave; var before = run.State.Items.Select(s => s.Id + s.Count).ToList();
                Check(run.VisitorChoose(0), "Lender lends");
                VisitorEffect lent = run.State.VisitorEffects.FirstOrDefault(e => e.Id == "Lent");
                Check(d.IsFree && run.State.Gold == gold, "Lender is free, as the card says");
                Check(lent != null && c.Item(lent.Target).Rarity == ItemRarity.Rare && d.Get == "relic:Rare", "Lender lends a Rare relic, as the card says");
                Check(lent.Wave == wave + 1 && d.When.Contains("next wave"), "Lender's relic is for the next wave, as the card says");
            }
            // Future Post: 10 gold now, 15 gold in 3 waves
            {
                using var run = Fresh(c); run.ForceVisitor("FuturePost");
                VisitorDeal d = ShopVisitors.DealFor(ShopVisitors.For("FuturePost"), 0, run.State.Wave, run.GoldScale);
                int wave = run.State.Wave; run.VisitorChoose(0);
                VisitorEffect post = run.State.VisitorEffects.First(e => e.Id == "Post");
                Check(post.Wave == wave + 3 && Mathf.RoundToInt(post.Value) == ShopVisitors.Amount(d.Get, "gold") && d.When.Contains("3 waves"), "Future Post pays GET " + d.Get + " " + d.When);
            }
        }

        private static CombatSimulation Fight(RunSession run)
        {
            run.ContinueWave();
            var arena = run.PrepareCombat(); arena.WaveDuration = 6; arena.Boss = null;
            var sim = new CombatSimulation(arena); sim.Start(4242);
            for (int t = 0; t < 400 && sim.Phase == RunPhase.Combat; t++) { sim.Champion.Health = sim.Champion.Definition.Health; sim.Step(.05f, Vector2.right * Mathf.Sin(t * .05f)); }
            if (sim.Phase == RunPhase.Combat) sim.ForceVictoryForChecks();
            return sim;
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
