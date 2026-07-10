using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Release balance evidence. Production combat/shop; diagnostic fixtures are labelled separately.</summary>
    public static class ReleaseBalanceAudit
    {
        private static string Arg(string key, string fallback)
        {
            var a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, key);
            return i >= 0 && i + 1 < a.Length ? a[i + 1] : fallback;
        }
        private static string Root => "PreviewExports/ReleaseBalance/" + Arg("-release-label", "Baseline");
        private static string F(float v) => v.ToString("0.####", CultureInfo.InvariantCulture);
        private static int[] Difficulties => Arg("-release-difficulties", string.Join(",", Enumerable.Range(0, RunMeta.DifficultyNames.Length))).Split(',').Select(int.Parse).ToArray();
        private static int[] Seeds => Arg("-release-seeds", "271026").Split(',').Select(int.Parse).ToArray();
        private static string[] Policies => Arg("-release-policies", "Hybrid").Split(',');
        private static string MetaMode => Arg("-release-meta", "required");
        public static void Run()
        {
            Directory.CreateDirectory(Root);
            var c = Resources.Load<RunCatalog>("RunCatalog");
            if (c == null) throw new InvalidOperationException("Missing production catalog");
            if (Difficulties.Any(d => d < 0 || d >= RunMeta.DifficultyNames.Length) || Policies.Any(p => p != "Buff" && p != "Recruit" && p != "Hybrid") || !new[] { "required", "none", "full" }.Contains(MetaMode)) throw new ArgumentException("Invalid release matrix");
            if (Difficulties.Distinct().Count() != Difficulties.Length || Seeds.Distinct().Count() != Seeds.Length || Policies.Distinct().Count() != Policies.Length) throw new ArgumentException("Duplicate release matrix axis");
            Provenance();
            Export(c);
            Checks();
            Campaigns(c);
            Debug.Log("RELEASE_BALANCE_COMPLETE " + Root);
        }
        private static void Provenance()
        {
            string provenance = string.Join("\n", new[] { typeof(RunSession).Assembly.Location, typeof(ReleaseBalanceAudit).Assembly.Location }.Select(p =>
            {
                using var sha = SHA256.Create();
                return Path.GetFileName(p) + " " + BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(p))).Replace("-", "");
            }));
            string stamp = Root + "/Assembly.txt";
            if (File.Exists(stamp) && File.ReadAllText(stamp) != provenance) throw new InvalidOperationException("Assembly changed: use a new release-label");
            File.WriteAllText(stamp, provenance);
        }
        public static void Static()
        {
            Directory.CreateDirectory(Root); Provenance(); Export(Resources.Load<RunCatalog>("RunCatalog")); Checks();
        }
        private static void Export(RunCatalog c)
        {
            var rows = new List<string> { "hero\tdifficulty\twave\theroHp\theroHit\theroCooldown\tenemyHp\tenemyHit\tenemySpeed\tspawnInterval\tbudget\tboss\tbossHp\tbossHit\tduration\tpriceScale\treroll\tspecialPriceScale" };
            foreach (var h in c.Champions)
                for (int d = 0; d < RunMeta.DifficultyNames.Length; d++)
                    using (var s = new RunSession(c))
                    {
                        s.Start(h.Id, 271026, d);
                        foreach (int wave in new[] { 1, 3, 4, 5, 8, 10, 12, 15, 16, 17, 19, 20, 21, 30, 50 })
                        {
                            s.State.Wave = wave; s.State.Endless = wave > 20;
                            var a = s.PrepareCombat();
                            rows.Add(string.Join("\t", h.Id, d, wave, F(a.Champion.Health), F(a.Champion.Weapon.Damage), F(a.Champion.Weapon.Cooldown), F(a.MeleeEnemy.Health), F(a.MeleeEnemy.Weapon.Damage), F(a.MeleeEnemy.Speed), F(a.SpawnInterval), a.EnemyBudget, a.Boss?.Id ?? "", F(a.Boss?.Health ?? 0), F(a.Boss?.Weapon.Damage ?? 0), F(a.WaveDuration), F(s.StagePriceFactor), s.RerollCost, F(s.SpecialStagePriceFactor)));
                        }
                    }
            File.WriteAllLines(Root + "/Compiled.tsv", rows);
            File.WriteAllLines(Root + "/Difficulties.tsv", new[] { "index\tname\trequiresTree" }.Concat(RunMeta.DifficultyNames.Select((name, index) => index + "\t" + name + "\t" + (index >= RunMeta.TreeDifficulty))));
            File.WriteAllLines(Root + "/Items.tsv", new[] { "id\trarity\tprice\taltarPrice\tunlock\tbudget\tmechanic\tmaxStacks\tmodifiers" }.Concat(c.Items.Select(i => string.Join("\t", i.Id, i.Rarity, i.Price, i.AltarPrice, i.UnlockWave, F(BalanceAudit.Budget(i.EffectiveModifiers)), i.Mechanic, i.StackLimit, string.Join(";", i.EffectiveModifiers.Select(m => m.Effect + "/" + m.Scope + "/" + F(m.Value)))))));
            File.WriteAllLines(Root + "/Companions.tsv", new[] { "id\trarity\tprice\tweapon\tpassive\tstats" }.Concat(c.Companions.Select(u => string.Join("\t", u.Id, u.Rarity, u.Price, u.Weapon.Id, u.Passive, F(u.Weapon.Damage) + "/" + F(u.Weapon.Cooldown)))));
            File.WriteAllText(Root + "/Method.txt", "Production catalog; compiled coverage all heroes, all difficulties, selected waves 1-50. Campaigns use real income, prices, rerolls, quotas, merge, earned prism rewards and story parties; no injected combat HP/gold/items. Compile probes have no meta. Campaign meta mode: " + MetaMode + ". Required mode applies a deterministic full Lamp Tree/mastery loadout only to tree-gated difficulties. Full applies it everywhere; none nowhere. These owned-profile fixtures are separate from fresh-profile outcomes; exact loadouts saved under Meta/. Ordinary encounter cutoff90s, boss240s. TIMEOUT stays unresolved. Bot outcomes are not human win probabilities. Six-body preference applies only to Buff buyers. Optional events are declined; transaction tests do not establish event balance. allyDamage is ALL non-hero ledger damage, including ARENA, not pure companion damage. Spending is net gold wallet change, never egg/HP value. bossFellAt is real boss-death time (-100 when not killed), not encounter duration.\nCampaign difficulties: " + string.Join(",", Difficulties) + "\nSeeds: " + string.Join(",", Seeds) + "\nPolicies: " + string.Join(",", Policies) + "\nHeroes: " + Arg("-release-heroes", "ALL") + "\n");
        }
        private static void Checks()
        {
            var rows = new List<string>();
            void Gate(string name, Action action)
            {
                try { action(); rows.Add(name + "\tPASS"); }
                catch (Exception e) { rows.Add(name + "\tFAIL\t" + e.Message.Replace('\n', ' ')); Debug.LogWarning("RELEASE_GATE " + name + " " + e.Message); }
                File.WriteAllLines(Root + "/Checks.tsv", rows);
            }
            Gate("BuildBalance", BuildBalanceChecks.Run);
            Gate("Recruitment", RecruitmentChecks.Run);
            Gate("HeroIdentity", HeroIdentityChecks.Run);
            Gate("HeroAwakening", HeroAwakeningChecks.Run);
            Gate("SecretEvents", SecretEventChecks.Run);
            Gate("Visitors", VisitorChecks.Run);
            Gate("RareVisitors", RareVisitorChecks.Run);
            Gate("Modes", ModeBalanceChecks.Run);
            Gate("ReleaseCombat", ReleaseCombatChecks.Run);
            if (RunMeta.DifficultyNames.Length == 5) Gate("FiveDifficulties", ReleaseFiveDifficultyChecks.Run);
        }
        private static void Campaigns(RunCatalog c)
        {
            string runs = Root + "/Campaigns.tsv", waves = Root + "/Waves.tsv";
            if (!File.Exists(runs)) File.WriteAllText(runs, "key\thero\tdifficulty\tseed\tpolicy\tresult\twave\tseconds\theroDamage\tallyDamage\tallies\titems\tgold\tmetaLevel\n");
            if (!File.Exists(waves)) File.WriteAllText(waves, "key\twave\tresult\tseconds\thpShare\tkills\tcombatGold\tincome\twallet\tmeanOffer\trecruitSpent\titemSpent\trerollSpent\tbuys\tallies\ttwoStar\tthreeStar\titems\tboss\tbossHp\tbossLeft\theroDamage\tallyDamage\tbossFellAt\tmetaLevel\n");
            var done = new HashSet<string>(File.ReadLines(runs).Skip(1).Select(l => l.Split('\t')[0]));
            int count = 0;
            string filter = Arg("-release-heroes", "");
            File.WriteAllLines(Root + "/CampaignPlan.tsv", new[] { "key" }.Concat(
                c.Champions.Where(h => filter.Length == 0 || filter.Split(',').Contains(h.Id))
                    .SelectMany(h => Difficulties.SelectMany(d => Seeds.SelectMany(seed => Policies.Select(policy => h.Id + "/" + d + "/" + seed + "/" + policy))))));
            foreach (var hero in c.Champions.Where(h => filter.Length == 0 || filter.Split(',').Contains(h.Id)))
                foreach (int d in Difficulties)
                    foreach (int seed in Seeds)
                        foreach (string policy in Policies)
                        {
                            string key = hero.Id + "/" + d + "/" + seed + "/" + policy;
                            if (done.Contains(key)) continue;
                            using var s = new RunSession(c); s.Start(hero.Id, seed, d);
                            int metaLevel = MetaMode == "full" || MetaMode == "required" && d >= RunMeta.TreeDifficulty ? 1 : 0;
                            if (metaLevel > 0)
                            {
                                var profile = MetaProgression.BotProfile(1, c, hero.Id);
                                if (RunMeta.UnlockedDifficulty(profile) < d) throw new InvalidOperationException("Diagnostic tree does not unlock " + d);
                                var loadout = MetaProgression.BotLoadout(1, c, hero.Id);
                                s.ApplyMeta(loadout);
                                Directory.CreateDirectory(Root + "/Meta");
                                File.WriteAllText(Root + "/Meta/" + hero.Id + ".json", JsonUtility.ToJson(loadout, true));
                            }
                            float seconds = 0, own = 0, ally = 0; string outcome = "TIMEOUT";
                            var waveRows = new List<string>();
                            for (int n = 0; n < c.NormalWaves; n++)
                            {
                                int wave = s.State.Wave; var a = s.PrepareCombat();
                                var sim = new CombatSimulation(a); sim.Start(unchecked(seed + wave * 397));
                                int limit = (a.Boss == null ? 90 : 240) * 20;
                                for (int tick = 0; tick < limit && sim.Phase == RunPhase.Combat; tick++) sim.Step(.05f, BuildBalanceDiagnostics.Steer(sim));
                                float hd = sim.Contributions.Where(x => x.UnitId == hero.Id).Sum(x => x.Damage), ad = sim.Contributions.Where(x => x.UnitId != hero.Id).Sum(x => x.Damage);
                                if (!float.IsFinite(hd + ad + sim.Champion.Health)) throw new InvalidOperationException("Nonfinite campaign " + key);
                                own += hd; ally += ad; seconds += sim.Elapsed;
                                if (sim.Phase != RunPhase.Combat) s.Settle(sim);
                                int wallet = s.State.Gold; float price = s.State.Offers.Where(o => !o.Sold && o.Currency == ShopCurrency.Gold).Select(o => (float)o.Price).DefaultIfEmpty(0).Average();
                                int rs = 0, its = 0, roll = 0, buys = 0;
                                if (s.State.Phase == SessionPhase.Shop && sim.Phase == RunPhase.Victory) Buy(s, c, policy, ref rs, ref its, ref roll, ref buys);
                                waveRows.Add(string.Join("\t", key, wave, sim.Phase, F(sim.Elapsed), F(sim.Champion.Health / a.Champion.Health), sim.Kills, sim.Gold, s.State.LastIncome, wallet, F(price), rs, its, roll, buys, s.State.Roster.Count, s.State.Roster.Count(r => r.Tier == 2), s.State.Roster.Count(r => r.Tier == 3), s.State.Items.Sum(x => x.Count), a.Boss?.Id ?? "", F(a.Boss?.Health ?? 0), F(sim.Boss?.Health ?? 0), F(hd), F(ad), F(sim.BossFellAt), metaLevel));
                                if (sim.Phase == RunPhase.Combat) break;
                                if (s.State.Phase != SessionPhase.Shop) { outcome = s.State.Phase.ToString(); break; }
                                s.ContinueWave();
                                if (s.State.Phase != SessionPhase.Combat) { outcome = "BLOCKED_SHOP"; break; }
                            }
                            File.AppendAllLines(waves, waveRows);
                            File.AppendAllText(runs, string.Join("\t", key, hero.Id, d, seed, policy, outcome, s.State.Wave, F(seconds), F(own), F(ally), s.State.Roster.Count, s.State.Items.Sum(x => x.Count), s.State.Gold, metaLevel) + "\n");
                            done.Add(key);
                            if (++count % 6 == 0) Debug.Log("RELEASE_CAMPAIGNS " + count + " " + key + " " + outcome + " wave=" + s.State.Wave);
                        }
        }
        private static void Buy(RunSession s, RunCatalog c, string policy, ref int recruits, ref int items, ref int rerolls, ref int buys)
        {
            if (s.State.OutsideStage == OutsideStageStatus.Reward)
            {
                int best = s.State.OutsideRewards.FindIndex(id => policy == "Recruit" ? c.Unit(id) != null : c.Item(id) != null);
                if (!s.ClaimOutsideStageReward(Mathf.Max(0, best))) throw new InvalidOperationException("Cannot claim earned reward");
            }
            if (s.Visitor != null) s.VisitorDecline();
            float Score(ShopOffer o)
            {
                float score = BalanceAudit.OfferValue(s, c, o);
                if (o.Kind == OfferKind.Companion)
                {
                    if (policy == "Buff" && s.State.Roster.Count >= 6 && !ShopMergePreview.Ready(s, o)) return -1;
                    score *= policy == "Recruit" ? 2 : policy == "Buff" ? .2f : 1;
                }
                return score / Mathf.Max(1, o.Currency == ShopCurrency.Gold ? o.Price : o.Price * 3);
            }
            int r = 0, it = 0, b = 0;
            void Offers(IEnumerable<ShopOffer> list)
            {
                foreach (var o in list.OrderByDescending(Score).ToArray())
                {
                    if (Score(o) <= 0 || !s.CanBuy(o)) continue;
                    int before = s.State.Gold;
                    if (!s.Buy(o.Token)) continue;
                    if (o.Kind == OfferKind.Companion) r += before-s.State.Gold; else it += before-s.State.Gold;
                    b++;
                }
            }
            Offers(s.State.SpecialOffers); s.State.SpecialShopVisited = true;
            for (int pass = 0; pass < 12; pass++)
            {
                Offers(s.State.Offers);
                int cheapest = s.State.Offers.Where(o => !o.Sold && Score(o) > 0).Select(o => o.Price).DefaultIfEmpty(20).Min();
                if (s.State.Gold <= s.RerollCost + cheapest) break;
                int before = s.State.Gold; if (!s.Reroll()) break; rerolls += before-s.State.Gold;
            }
            recruits += r; items += it; buys += b;
        }
    }
}
