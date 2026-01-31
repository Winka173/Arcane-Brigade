using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class ModeBalanceDiagnostics
    {
        public static void Baseline() => Run("Before", 8);
        public static void Candidate() => Run("After", 8);
        public static void Campaign() => Run("Campaign", 20);
        public static void BaselineCampaign() => Run("BeforeCampaign", 20);
        public static void NaturalEndless() => Run("NaturalCampaign", 20);
        public static void Endless()
        {
            const string root = "PreviewExports/ModeBalance"; Directory.CreateDirectory(root);
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            var rows = new List<string> { "difficulty,wave,result,seconds,kills,health_remaining,enemy_hp,budget" };
            for (int difficulty = 0; difficulty < RunMeta.DifficultyNames.Length; difficulty++)
                foreach (int wave in new[] { 21, 24, 30 })
                    using (var session = new RunSession(catalog))
                    {
                        session.Start("LanternKnight", 161026, difficulty); session.State.Wave = wave; session.State.Endless = true;
                        session.State.Roster.Clear();
                        UnitDefinition hero = catalog.Unit("LanternKnight");
                        foreach (UnitDefinition ally in catalog.Companions.Where(unit => !unit.GiftOnly && !unit.SpecialRecruit && unit.UnlockWave <= 8 && K11HeroRules.Eligible(hero, unit)).Take(6))
                            session.State.Roster.Add(new RosterEntry { UnitId = ally.Id, WeaponId = ally.Weapon.Id, Tier = 2 });
                        foreach (string id in new[] { "warm-tea", "pillow-mail", "starlight-quilt", "brigade-banner", "royal-key", "twin-seal" })
                            session.State.Items.Add(new ItemStack { Id = id, Count = 2 });
                        CheckpointCatalog combat = session.PrepareCombat();
                        var simulation = new CombatSimulation(combat); simulation.Start(161026);
                        for (int tick = 0; tick < 3600 && simulation.Phase == RunPhase.Combat; tick++) simulation.Step(.05f, K13MovementPolicy.Steer(simulation));
                        rows.Add(string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3:F2},{4},{5:F2},{6:F2},{7}", difficulty, wave, simulation.Phase, simulation.Elapsed, simulation.Kills, simulation.Champion.Health, combat.MeleeEnemy.Health, combat.EnemyBudget));
                        File.WriteAllLines(root + "/Endless_Controlled.csv", rows);
                    }
            File.WriteAllText(root + "/Endless_Method.txt", "18 controlled sandbox encounters, all six difficulties at waves 21/24/30, identical six eligible tier-2 companions and twelve fixed relic stacks per case. Explicit injected fixture loadout, not a naturally earned build or an endless win-rate estimate. No injected immortality, score or victory. Three-minute cutoff; Combat means unresolved. Production enemy scaling and K13 movement.\n");
            Debug.Log("MODE_ENDLESS_COMPLETE 18 controlled encounters");
        }
        private static void Run(string label, int lastWave)
        {
            string root = "PreviewExports/ModeBalance/" + label; Directory.CreateDirectory(root);
            if (label == "NaturalCampaign") File.WriteAllText(root + "/Natural_Endless.csv", "hero,difficulty,seed,wave,result,seconds,kills,earned,allies,items,hp\n");
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            string[] heroes = { "LanternKnight", "LoneJelly", "RootOracle", "CometRunner", "GlassDice", "TideSinger" };
            var rows = new List<string> { "mode,hero,difficulty,seed,wave,result,seconds,kills,earned,gold_before_shop,gold_after_shop,allies,items,hp,max_hp,boss_hp,boss_remaining,mean_price" };
            int runs = 0;
            foreach (string mode in RunMeta.ChallengeIds)
                foreach (string hero in heroes)
                    foreach (int difficulty in mode == "" ? Enumerable.Range(0, RunMeta.DifficultyNames.Length) : new[] { 0 })
                        foreach (int seed in mode == "" ? new[] { 161026, 77138 } : new[] { RunMeta.ChallengeSeed(Array.IndexOf(RunMeta.ChallengeIds, mode)) })
                            using (var session = new RunSession(catalog))
                            {
                                session.Start(hero, seed, difficulty);
                                for (int encounter = 0; encounter < lastWave; encounter++)
                                {
                                    CheckpointCatalog combat = session.PrepareCombat();
                                    var simulation = new CombatSimulation(combat); simulation.Start(unchecked(seed + session.State.Wave * 397));
                                    float bossHealth = combat.Boss == null ? 0 : combat.Boss.Health;
                                    int limit = combat.Boss == null ? 1200 : 3600;
                                    for (int tick = 0; tick < limit && simulation.Phase == RunPhase.Combat; tick++) simulation.Step(.05f, K13MovementPolicy.Steer(simulation));
                                    if (!float.IsFinite(simulation.Champion.Health) || simulation.Gold < 0) throw new InvalidOperationException("Invalid matrix state");
                                    bool settled = simulation.Phase == RunPhase.Victory || simulation.Phase == RunPhase.Defeat;
                                    if (settled) session.Settle(simulation);
                                    int before = session.State.Gold;
                                    double mean = session.State.Offers.Count == 0 ? 0 : session.State.Offers.Average(offer => offer.Price);
                                    if (session.State.Phase == SessionPhase.Shop) Purchase(session, catalog, mode == "small-brigade" ? 6 : OctoberBalance.AllySoftLimit);
                                    rows.Add(string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},{5},{6:F2},{7},{8},{9},{10},{11},{12},{13:F2},{14:F2},{15:F2},{16:F2},{17:F2}", mode == "" ? "journey" : mode, hero, difficulty, seed, session.State.Wave, simulation.Phase, simulation.Elapsed, simulation.Kills, simulation.Gold, before, session.State.Gold, session.State.Roster.Count, session.State.Items.Sum(stack => stack.Count), simulation.Champion.Health, simulation.Champion.Definition.Health, bossHealth, simulation.Boss == null ? 0 : Mathf.Max(0, simulation.Boss.Health), mean));
                                    File.WriteAllLines(root + "/Runs.csv", rows);
                                    if (!settled || session.State.Phase != SessionPhase.Shop) break;
                                    session.ContinueWave();
                                }
                                if (label == "NaturalCampaign" && mode == "" && session.State.Phase == SessionPhase.Victory)
                                {
                                    if (session.EndingChoicePending) session.ChooseFarewell(NightmareFarewell.CloseShop);
                                    while (session.EndingPending) if (!session.AdvanceEnding()) throw new InvalidOperationException("Natural endless ending boundary");
                                    session.EnterEndless(); Purchase(session, catalog, OctoberBalance.AllySoftLimit); session.ContinueWave();
                                    var endlessRows = new List<string>();
                                    for (int encounter = 0; encounter < 4; encounter++)
                                    {
                                        CheckpointCatalog combat = session.PrepareCombat();
                                        var simulation = new CombatSimulation(combat); simulation.Start(unchecked(seed + session.State.Wave * 397));
                                        for (int tick = 0; tick < 3600 && simulation.Phase == RunPhase.Combat; tick++) simulation.Step(.05f, K13MovementPolicy.Steer(simulation));
                                        bool settled = simulation.Phase == RunPhase.Victory || simulation.Phase == RunPhase.Defeat;
                                        if (settled) session.Settle(simulation);
                                        endlessRows.Add(string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},{5:F2},{6},{7},{8},{9},{10:F2}", hero, difficulty, seed, session.State.Wave, simulation.Phase, simulation.Elapsed, simulation.Kills, simulation.Gold, session.State.Roster.Count, session.State.Items.Sum(stack => stack.Count), simulation.Champion.Health));
                                        if (!settled || session.State.Phase != SessionPhase.Shop) break;
                                        Purchase(session, catalog, OctoberBalance.AllySoftLimit); session.ContinueWave();
                                    }
                                    string path = root + "/Natural_Endless.csv";
                                    File.AppendAllLines(path, endlessRows);
                                }
                                runs++; if (runs % 6 == 0) Debug.Log("MODE_BALANCE " + label + " " + runs + "/90");
                            }
            File.WriteAllText(root + "/Method.txt", runs + " runs; six representative heroes, all six journey difficulties, two seeds; three challenges at their required fixed seed/difficulty; production shop and income, no injected gold/HP. K13 steering and scoring; small-brigade buyer stops at six bodies while allowing immediate merges. Normal waves timeout at 60s, boss waves at 180s. Timeout is unresolved, not a win/loss. Natural coverage stops at wave " + lastWave + ". No human balance acceptance.\n");
            Debug.Log("MODE_BALANCE_COMPLETE " + label + " " + runs + " runs " + (rows.Count - 1) + " wave samples");
        }
        private static void Purchase(RunSession session, RunCatalog catalog, int capacity)
        {
            bool Allowed(ShopOffer offer) => offer.Kind != OfferKind.Companion || session.State.Roster.Count < capacity || ShopMergePreview.Ready(session, offer);
            float Score(ShopOffer offer) => K13MovementPolicy.Score(session, catalog, offer);
            foreach (ShopOffer offer in session.State.SpecialOffers.OrderByDescending(Score).ToArray())
                if (Allowed(offer) && Score(offer) > 0 && session.CanBuy(offer)) session.Buy(offer.Token);
            session.State.SpecialShopVisited = true;
            for (int pass = 0; pass < 12; pass++)
            {
                foreach (ShopOffer offer in session.State.Offers.OrderByDescending(Score).ToArray())
                    if (Allowed(offer) && Score(offer) > 0 && session.CanBuy(offer)) session.Buy(offer.Token);
                int cheapest = session.State.Offers.Where(offer => !offer.Sold && Allowed(offer) && Score(offer) > 0).Select(offer => offer.Price).DefaultIfEmpty(20).Min();
                if (session.State.Gold <= session.RerollCost + cheapest || !session.Reroll()) break;
            }
        }
    }
}
