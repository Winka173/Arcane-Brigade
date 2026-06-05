using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>One lantern of the Lamp Tree: a branch, a tier (0 at the trunk), its price and what it adds to a run.
    /// Two nodes on one tier are a fork: one of the two.</summary>
    public sealed class LampNode
    {
        public string Id, Name, Line, Icon;
        public int Branch, Tier, Cost;
        public MetaCurrency Currency;
        public bool Capstone;
        /// <summary>The stat icon shown on the lantern (when the node is a plain stat).</summary>
        public ItemEffect? Stat;
        /// <summary>Permanent combat power it adds, in percent (all lit nodes are scaled down to the +25% cap).</summary>
        public float Power;
        public Action<MetaRunState> Apply;
    }

    /// <summary>The Lamp Tree (owner 24/10/2026, "3. chốt"): the permanent upgrades in five branches - the hero, friends (never
    /// healing or armour), gold and the shop, luck and chests, dreams - replacing the Lantern's, Ledger's, Nursery's and Clock's
    /// rank lines. Plain lanterns cost Lamp Oil; forks and the crowning lantern cost Keepsakes. A branch opens with three lit in
    /// the branch before it, or with wins on that many difficulties. Taking the tree back is free (everything is refunded).</summary>
    public static partial class MetaProgression
    {
        public static readonly string[] BranchNames = { "HERO", "FRIENDS", "GOLD", "LUCK", "DREAMS" };
        public const int TreeTiers = 7, BranchOpensAt = 3;

        private static LampNode N(string id, int branch, int tier, string name, string line, int cost, ItemEffect? stat, string icon, float power, Action<MetaRunState> apply, MetaCurrency currency = MetaCurrency.LampOil)
            => new LampNode { Id = id, Branch = branch, Tier = tier, Name = name, Line = line, Cost = cost, Currency = currency, Stat = stat, Icon = icon, Power = power, Apply = apply, Capstone = tier == TreeTiers - 1 };
        private const MetaCurrency K = MetaCurrency.Keepsakes;

        public static readonly LampNode[] Tree =
        {
            // the hero
            N("hero.wick", 0, 0, "Wick", "+5% max HP", 20, ItemEffect.Health, null, 2.5f, s => s.Health += .05f),
            N("hero.stride", 0, 1, "Stride", "+4% move speed", 40, ItemEffect.MoveSpeed, null, 1, s => s.MoveSpeed += .04f),
            N("hero.reach", 0, 2, "Long reach", "+35% pickup range", 60, ItemEffect.Pickup, null, 0, s => s.Pickup += .35f),
            N("hero.spark", 0, 3, "Spark", "+3% crit, you and friends", 3, ItemEffect.Crit, null, 3, s => s.Crit += .03f, K),
            N("hero.thick", 0, 3, "Thick wick", "+6% max HP", 3, ItemEffect.Health, null, 3, s => s.Health += .06f, K),
            N("hero.wick2", 0, 4, "Wick II", "+5% max HP", 100, ItemEffect.Health, null, 2.5f, s => s.Health += .05f),
            N("hero.candle", 0, 5, "Second Candle", "Once a run, get back up at 40% HP", 140, null, "clock", 0, s => s.CandleShare = .4f),
            N("hero.crown", 0, 6, "Ready flame", "Every wave starts with ⅓ gauge", 6, null, "gauge", 0, null, K),
            // friends: offence and help, never healing or armour
            N("friends.stork", 1, 0, "Stork", "+3% chance a friend comes at 2★", 25, null, "egg", 1, s => s.TwoStarChance += .03f),
            N("friends.cradle", 1, 1, "Cradle", "Start with 1 of 3 friends", 45, null, "nursery", 4, null),
            N("friends.stork2", 1, 2, "Stork II", "+3% chance of 2★", 60, null, "egg", 1, s => s.TwoStarChance += .03f),
            N("friends.heirloom", 1, 3, "Heirloom", "Your family bonds one friend sooner", 4, null, "family", 2, s => s.Heirloom = "*", K),
            N("friends.sharp", 1, 3, "Sharp friends", "+3% damage, you and friends", 4, ItemEffect.Damage, null, 3, s => s.Damage += .03f, K),
            N("friends.stork3", 1, 4, "Stork III", "+3% chance of 2★", 95, null, "egg", 1, s => s.TwoStarChance += .03f),
            N("friends.drill", 1, 5, "Drill", "+2% damage, you and friends", 130, ItemEffect.Damage, null, 2, s => s.Damage += .02f),
            N("friends.crown", 1, 6, "Gold-star welcome", "First friend bought each run: +1★", 6, null, "star", 0, s => s.FirstFriendStar = true, K),
            // gold and the shop
            N("gold.purse", 2, 0, "Purse", "+8 starting gold", 20, null, "coin", 0, s => s.StartGold += 8),
            N("gold.pockets", 2, 1, "Deep pockets", "+1 interest cap", 45, ItemEffect.Interest, null, 0, s => s.Interest += 1),
            N("gold.purse2", 2, 2, "Purse II", "+8 starting gold", 60, null, "coin", 0, s => s.StartGold += 8),
            N("gold.look", 2, 3, "Free look", "First refresh in every shop is free", 4, null, "ledger", 0, s => s.FreeRefresh = true, K),
            N("gold.shelf", 2, 3, "Extra shelf", "One more offer in every shop", 4, null, "ledger", 0, s => s.ExtraSlots = 1, K),
            N("gold.pockets2", 2, 4, "Deep pockets II", "+1 interest cap", 100, ItemEffect.Interest, null, 0, s => s.Interest += 1),
            N("gold.purse3", 2, 5, "Purse III", "+10 starting gold", 130, null, "coin", 0, s => s.StartGold += 10),
            N("gold.crown", 2, 6, "Rare find", "First shop each run holds a rare relic", 6, null, "rare", 0, s => s.FirstShopRare = true, K),
            // luck and chests
            N("luck.chest", 3, 0, "Full chests", "+6% gold in chests", 25, ItemEffect.ChestQuality, null, 0, s => s.ChestGold += .06f),
            N("luck.clover", 3, 1, "Clover", "+1% crit, you and friends", 40, ItemEffect.Crit, null, 1, s => s.Crit += .01f),
            N("luck.chest2", 3, 2, "Full chests II", "+8% gold in chests", 60, ItemEffect.ChestQuality, null, 0, s => s.ChestGold += .08f),
            N("luck.star", 3, 3, "Lucky star", "+4% chance a friend comes at 2★", 5, null, "egg", 1.33f, s => s.TwoStarChance += .04f, K),
            N("luck.nose", 3, 3, "Treasure nose", "Better chests", 5, ItemEffect.TreasureDetection, null, 0, s => s.ChestQuality += 1, K),
            N("luck.chest3", 3, 4, "Full chests III", "+10% gold in chests", 95, ItemEffect.ChestQuality, null, 0, s => s.ChestGold += .1f),
            N("luck.clover2", 3, 5, "Clover II", "+2% crit, you and friends", 130, ItemEffect.Crit, null, 2, s => s.Crit += .02f),
            N("luck.crown", 3, 6, "Rewind", "Once a run, replay a lost wave", 6, null, "clock", 0, s => s.RewindOwned = true, K),
            // dreams: the oil and keepsakes a run brings home, the gauge
            N("dream.oil", 4, 0, "Bright oil", "+5% Lamp Oil from runs", 25, null, "oil", 0, null),
            N("dream.quick", 4, 1, "Quick dream", "Gauge starts +10%", 45, null, "gauge", 0, s => s.GaugeStart += .1f),
            N("dream.oil2", 4, 2, "Bright oil II", "+5% Lamp Oil from runs", 65, null, "oil", 0, null),
            N("dream.glow", 4, 3, "Candle glow", "Second Candle: 2s untouchable", 5, null, "clock", 0, s => s.CandleShield = 2, K),
            N("dream.finds", 4, 3, "Collector", "New finds pay double oil", 5, null, "oil", 0, null, K),
            N("dream.oil3", 4, 4, "Bright oil III", "+10% Lamp Oil from runs", 100, null, "oil", 0, null),
            N("dream.keen", 4, 5, "Keen dream", "Gauge starts +10%", 130, null, "gauge", 0, s => s.GaugeStart += .1f),
            N("dream.crown", 4, 6, "Keepsake dreams", "+1 Keepsake for a run of 10+ waves", 6, null, "keep", 0, null, K),
        };

        public static LampNode Node(string id) => Array.Find(Tree, n => n.Id == id);
        public static bool Lit(PlayerProfile p, string id) => p?.Meta?.Tree != null && p.Meta.Tree.Contains(id);
        public static int LitIn(PlayerProfile p, int branch) { int n = 0; foreach (LampNode node in Tree) if (node.Branch == branch && Lit(p, node.Id)) n++; return n; }
        /// <summary>Difficulties won at least once (each opens one more branch).</summary>
        public static int DifficultiesWon(PlayerProfile p) { int mask = p?.Meta?.WonMask ?? 0, n = 0; for (; mask != 0; mask &= mask - 1) n++; return n; }
        public static bool BranchOpen(PlayerProfile p, int branch) => branch <= 0 || LitIn(p, branch - 1) >= BranchOpensAt || DifficultiesWon(p) >= branch;
        public static LampNode Fork(LampNode node) => Array.Find(Tree, n => n != node && n.Branch == node.Branch && n.Tier == node.Tier);

        /// <summary>Why a lantern cannot be lit now (null: it can).</summary>
        public static string NodeBlocked(PlayerProfile p, LampNode node)
        {
            Ensure(p);
            if (Lit(p, node.Id)) return "LIT";
            if (!BranchOpen(p, node.Branch)) return "Locked";
            if (Fork(node) is LampNode other && Lit(p, other.Id)) return "Other path taken";
            if (node.Tier > 0 && !Array.Exists(Tree, n => n.Branch == node.Branch && n.Tier == node.Tier - 1 && Lit(p, n.Id))) return "Light the one below";
            if (Wallet(p, node.Currency) < node.Cost) return node.Currency == MetaCurrency.LampOil ? "Not enough oil" : "Not enough keepsakes";
            return null;
        }
        public static bool Light(PlayerProfile p, string id)
        {
            LampNode node = Node(id);
            if (node == null || NodeBlocked(p, node) != null) return false;
            if (node.Currency == MetaCurrency.LampOil) p.Meta.LampOil -= node.Cost; else p.Meta.Keepsakes -= node.Cost;
            p.Meta.Tree.Add(id);
            if (TreeFull(p)) p.Meta.TreeDone = true;
            return true;
        }
        /// <summary>Takes the whole tree back for free: every lantern's price returns to the purse.</summary>
        public static bool Respec(PlayerProfile p)
        {
            MetaProgress m = Ensure(p);
            if (m.Tree.Count == 0) return false;
            foreach (string id in m.Tree)
            {
                LampNode node = Node(id); if (node == null) continue;
                if (node.Currency == MetaCurrency.LampOil) m.LampOil = RunSession.BoundedCounter((long)m.LampOil + node.Cost);
                else m.Keepsakes = RunSession.BoundedCounter((long)m.Keepsakes + node.Cost);
            }
            m.Tree.Clear();
            return true;
        }
        /// <summary>Every tier of every branch lit (a fork by either side).</summary>
        public static bool TreeFull(PlayerProfile p)
        {
            foreach (LampNode n in Tree) if (!Lit(p, n.Id) && !(Fork(n) is LampNode f && Lit(p, f.Id))) return false;
            return true;
        }
        public static bool TreeDone(PlayerProfile p) => p?.Meta != null && (p.Meta.TreeDone || TreeFull(p));
        public static bool AnyLightable(PlayerProfile p) { foreach (LampNode n in Tree) if (NodeBlocked(p, n) == null) return true; return false; }

        /// <summary>Lamp Oil a finished run's haul gains from Bright oil (a share).</summary>
        private static float TreeOil(PlayerProfile p) => (Lit(p, "dream.oil") ? .05f : 0) + (Lit(p, "dream.oil2") ? .05f : 0) + (Lit(p, "dream.oil3") ? .1f : 0);

        /// <summary>The old rank lines (Lantern, Ledger, Clock, Nursery; retired 24/10/2026), kept only to refund a profile that
        /// bought ranks on them: every rank's price goes back to the purse once, to be spent on the tree.</summary>
        private static readonly (string id, MetaCurrency currency, int[] cost)[] RetiredTracks =
        {
            ("lantern.wick", MetaCurrency.LampOil, new[] { 15, 30, 50, 75, 105 }), ("lantern.stride", MetaCurrency.LampOil, new[] { 20, 40, 65, 95 }),
            ("lantern.reach", MetaCurrency.LampOil, new[] { 15, 30, 50, 75 }), ("lantern.spark", MetaCurrency.LampOil, new[] { 25, 45, 70, 100, 135 }),
            ("ledger.purse", MetaCurrency.LampOil, new[] { 15, 30, 50, 75 }), ("ledger.look", MetaCurrency.LampOil, new[] { 120 }),
            ("ledger.pockets", MetaCurrency.LampOil, new[] { 60, 120 }), ("ledger.chest", MetaCurrency.LampOil, new[] { 20, 40, 65, 95 }),
            ("ledger.shelf", MetaCurrency.Keepsakes, new[] { 6 }), ("clock.candle", MetaCurrency.Keepsakes, new[] { 3, 5, 7 }),
            ("clock.rewind", MetaCurrency.Keepsakes, new[] { 10 }), ("nursery.cradle", MetaCurrency.Keepsakes, new[] { 4 }),
            ("nursery.heirloom", MetaCurrency.Keepsakes, new[] { 5 }), ("nursery.stork", MetaCurrency.LampOil, new[] { 40, 80, 120 }),
        };
        private static void RefundRetired(MetaProgress m)
        {
            foreach (MetaRank r in m.Ranks)
            {
                if (r == null) continue;
                int at = Array.FindIndex(RetiredTracks, t => t.id == r.Id); if (at < 0) continue;
                var (_, currency, cost) = RetiredTracks[at];
                long sum = 0; for (int k = 0; k < Mathf.Clamp(r.Rank, 0, cost.Length); k++) sum += cost[k];
                if (currency == MetaCurrency.LampOil) m.LampOil = RunSession.BoundedCounter(m.LampOil + sum); else m.Keepsakes = RunSession.BoundedCounter(m.Keepsakes + sum);
                r.Rank = 0;
            }
        }
    }
}
