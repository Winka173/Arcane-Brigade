using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>One lantern of the Lamp Tree: a branch, a tier (0 at the trunk), its ranks and price, what one rank adds to a
    /// run. Two nodes on one tier are a fork: one of the two. Branch -1 is the roots (open once the whole tree is lit).</summary>
    public sealed class LampNode
    {
        public string Id, Name, Line, Icon;
        public int Branch, Tier, Cost, Ranks = 1;
        public MetaCurrency Currency;
        public bool Capstone;
        /// <summary>The stat icon shown on the lantern (when the node is a plain stat).</summary>
        public ItemEffect? Stat;
        /// <summary>Permanent combat power one rank adds, in percent (all lit lanterns are scaled down to the cap; the roots are
        /// outside it).</summary>
        public float Power;
        public Action<MetaRunState> Apply;
        /// <summary>The price of the next rank (each rank about 1.6 times the one before; the roots climb slowly).</summary>
        public int CostAt(int rank) => Branch < 0 ? Cost + rank * 30 : Mathf.RoundToInt(Cost * Mathf.Pow(1.6f, rank));
    }

    /// <summary>A stat a lantern adds to a run (a relic-like modifier with its scope), saved with the run's loadout.</summary>
    [Serializable] public sealed class MetaStat { public ItemEffect Effect; public ModifierScope Scope; public float Value; }

    /// <summary>The Lamp Tree (requirement 24/10/2026, "3. chốt"; ranks, four more branches and the roots: "làm hết"): the permanent
    /// upgrades in nine branches - the hero, friends (never healing or armour), gold, luck, dreams, the shield (the hero's own
    /// defence), the forge, the hunt (weaker enemies), the merchant - over the roots. Plain lanterns cost Lamp Oil and most take
    /// three ranks; forks and the crowning lantern cost Keepsakes. A branch opens with three lit in the branch before it, or with
    /// wins on that many difficulties. Taking the tree back is free (everything is refunded).</summary>
    public static partial class MetaProgression
    {
        public static readonly string[] BranchNames = { "HERO", "FRIENDS", "GOLD", "LUCK", "DREAMS", "SHIELD", "FORGE", "HUNT", "MERCHANT" };
        public const int TreeTiers = 7, BranchOpensAt = 3, BaseBranches = 5, RootRanks = 20;

        private static LampNode N(string id, int branch, int tier, string name, string line, int cost, ItemEffect? stat, string icon, float power, Action<MetaRunState> apply, MetaCurrency currency = MetaCurrency.LampOil, int ranks = 0)
            => new LampNode { Id = id, Branch = branch, Tier = tier, Name = name, Line = line, Cost = cost, Currency = currency, Stat = stat, Icon = icon, Power = power, Apply = apply, Capstone = tier == TreeTiers - 1,
                              Ranks = ranks > 0 ? ranks : currency == MetaCurrency.LampOil ? 3 : 1 };
        private const MetaCurrency K = MetaCurrency.Keepsakes;
        private static Action<MetaRunState> Add(ItemEffect effect, ModifierScope scope, float value) => s => AddStat(s, effect, scope, value);
        public static void AddStat(MetaRunState s, ItemEffect effect, ModifierScope scope, float value)
        {
            s.Stats ??= new List<MetaStat>();
            MetaStat have = s.Stats.Find(x => x.Effect == effect && x.Scope == scope);
            if (have == null) s.Stats.Add(new MetaStat { Effect = effect, Scope = scope, Value = value }); else have.Value += value;
        }
        private const ModifierScope Hero = ModifierScope.Champion, Team = ModifierScope.Team, Friends = ModifierScope.Companions, Run = ModifierScope.Run;

        public static readonly LampNode[] Tree =
        {
            // the hero
            N("hero.wick", 0, 0, "Wick", "+4% max HP", 20, ItemEffect.Health, null, 1, s => s.Health += .04f),
            N("hero.stride", 0, 1, "Stride", "+3% move speed", 35, ItemEffect.MoveSpeed, null, .4f, s => s.MoveSpeed += .03f),
            N("hero.reach", 0, 2, "Long reach", "+25% pickup range", 50, ItemEffect.Pickup, null, 0, s => s.Pickup += .25f),
            N("hero.spark", 0, 3, "Spark", "+3% crit, you and friends", 3, ItemEffect.Crit, null, 1.5f, s => s.Crit += .03f, K),
            N("hero.thick", 0, 3, "Thick wick", "+8% max HP", 3, ItemEffect.Health, null, 2, s => s.Health += .08f, K),
            N("hero.wick2", 0, 4, "Wick II", "+4% max HP", 80, ItemEffect.Health, null, 1, s => s.Health += .04f),
            N("hero.candle", 0, 5, "Second Candle", "Once a run, get back up at 40% HP", 140, null, "clock", 0, s => s.CandleShare = .4f, ranks: 1),
            N("hero.crown", 0, 6, "Ready flame", "Every wave starts with ⅓ gauge", 6, null, "gauge", 0, null, K),
            // friends: offence and help, never healing or armour
            N("friends.stork", 1, 0, "Stork", "+2% chance a friend comes at 2★", 25, null, "egg", .3f, s => s.TwoStarChance += .02f),
            N("friends.cradle", 1, 1, "Cradle", "Start with 1 of 3 friends", 45, null, "nursery", 3, null, ranks: 1),
            N("friends.stork2", 1, 2, "Stork II", "+2% chance of 2★", 55, null, "egg", .3f, s => s.TwoStarChance += .02f),
            N("friends.heirloom", 1, 3, "Heirloom", "Your family bonds one friend sooner", 4, null, "family", 2, s => s.Heirloom = "*", K),
            N("friends.sharp", 1, 3, "Sharp friends", "+4% damage, you and friends", 4, ItemEffect.Damage, null, 2, s => s.Damage += .04f, K),
            N("friends.stork3", 1, 4, "Stork III", "+2% chance of 2★", 85, null, "egg", .3f, s => s.TwoStarChance += .02f),
            N("friends.drill", 1, 5, "Drill", "+3% friends' attack speed", 110, ItemEffect.CompanionAttackSpeed, null, .8f, Add(ItemEffect.CompanionAttackSpeed, Friends, .03f)),
            N("friends.crown", 1, 6, "Gold-star welcome", "First friend bought each run: +1★", 6, null, "star", 0, s => s.FirstFriendStar = true, K),
            // gold and the shop
            N("gold.purse", 2, 0, "Purse", "+6 starting gold", 20, null, "coin", 0, s => s.StartGold += 6),
            N("gold.pockets", 2, 1, "Deep pockets", "+1 interest cap", 45, ItemEffect.Interest, null, 0, s => s.Interest += 1, ranks: 2),
            N("gold.purse2", 2, 2, "Purse II", "+6 starting gold", 55, null, "coin", 0, s => s.StartGold += 6),
            N("gold.look", 2, 3, "Free look", "First refresh in every shop is free", 4, null, "ledger", 0, s => s.FreeRefresh = true, K),
            N("gold.shelf", 2, 3, "Extra shelf", "One more offer in every shop", 4, null, "ledger", 0, s => s.ExtraSlots = 1, K),
            N("gold.pockets2", 2, 4, "Deep pockets II", "+1 interest cap", 100, ItemEffect.Interest, null, 0, s => s.Interest += 1, ranks: 2),
            N("gold.purse3", 2, 5, "Purse III", "+8 starting gold", 110, null, "coin", 0, s => s.StartGold += 8),
            N("gold.crown", 2, 6, "Rare find", "First shop each run holds a rare relic", 6, null, "rare", 0, s => s.FirstShopRare = true, K),
            // luck and chests
            N("luck.chest", 3, 0, "Full chests", "+5% gold in chests", 25, ItemEffect.ChestQuality, null, 0, s => s.ChestGold += .05f),
            N("luck.clover", 3, 1, "Clover", "+1% crit, you and friends", 40, ItemEffect.Crit, null, .5f, s => s.Crit += .01f),
            N("luck.chest2", 3, 2, "Full chests II", "+5% gold in chests", 55, ItemEffect.ChestQuality, null, 0, s => s.ChestGold += .05f),
            N("luck.star", 3, 3, "Lucky star", "+4% chance a friend comes at 2★", 5, null, "egg", .6f, s => s.TwoStarChance += .04f, K),
            N("luck.nose", 3, 3, "Treasure nose", "Better chests", 5, ItemEffect.TreasureDetection, null, 0, s => s.ChestQuality += 1, K),
            N("luck.chest3", 3, 4, "Full chests III", "+6% gold in chests", 85, ItemEffect.ChestQuality, null, 0, s => s.ChestGold += .06f),
            N("luck.clover2", 3, 5, "Clover II", "+1% crit, you and friends", 110, ItemEffect.Crit, null, .5f, s => s.Crit += .01f),
            N("luck.crown", 3, 6, "Rewind", "Once a run, replay a lost wave", 6, null, "clock", 0, s => s.RewindOwned = true, K),
            // dreams: the oil and keepsakes a run brings home, the gauge
            N("dream.oil", 4, 0, "Bright oil", "+3% Lamp Oil from runs", 25, null, "oil", 0, null),
            N("dream.quick", 4, 1, "Quick dream", "Gauge starts +5%", 45, null, "gauge", 0, s => s.GaugeStart += .05f, ranks: 2),
            N("dream.oil2", 4, 2, "Bright oil II", "+3% Lamp Oil from runs", 60, null, "oil", 0, null),
            N("dream.glow", 4, 3, "Candle glow", "Second Candle: 2s untouchable", 5, null, "clock", 0, s => s.CandleShield = 2, K),
            N("dream.finds", 4, 3, "Collector", "New finds pay double oil", 5, null, "oil", 0, null, K),
            N("dream.oil3", 4, 4, "Bright oil III", "+4% Lamp Oil from runs", 90, null, "oil", 0, null),
            N("dream.keen", 4, 5, "Keen dream", "Gauge starts +5%", 120, null, "gauge", 0, s => s.GaugeStart += .05f, ranks: 2),
            N("dream.crown", 4, 6, "Keepsake dreams", "+1 Keepsake for a run of 10+ waves", 6, null, "keep", 0, null, K),
            // the shield: the hero's own defence (friends never get armour)
            N("shield.plate", 5, 0, "Plate", "+1 armor", 30, ItemEffect.Armor, null, .8f, Add(ItemEffect.Armor, Hero, .1f)),
            N("shield.heart", 5, 1, "Stout heart", "+4% max HP", 45, ItemEffect.Health, null, 1, s => s.Health += .04f),
            N("shield.plate2", 5, 2, "Plate II", "+1 armor", 60, ItemEffect.Armor, null, .8f, Add(ItemEffect.Armor, Hero, .1f)),
            N("shield.nimble", 5, 3, "Nimble", "+5% dodge", 5, ItemEffect.Dodge, null, 2, Add(ItemEffect.Dodge, Hero, .05f), K),
            N("shield.oak", 5, 3, "Oak skin", "+2 armor", 5, ItemEffect.Armor, null, 1.6f, Add(ItemEffect.Armor, Hero, .2f), K),
            N("shield.plate3", 5, 4, "Plate III", "+1 armor", 90, ItemEffect.Armor, null, .8f, Add(ItemEffect.Armor, Hero, .1f)),
            N("shield.bulwark", 5, 5, "Bulwark", "+1 armor, +3% max HP", 120, ItemEffect.Armor, null, 1.3f, s => { AddStat(s, ItemEffect.Armor, Hero, .1f); s.Health += .03f; }),
            N("shield.crown", 5, 6, "Lamp shell", "Each wave, the first hit on you is blocked", 7, null, "shell", 0, s => s.WaveShield = true, K),
            // the forge: the hero's weapon and the brigade's blows
            N("forge.edge", 6, 0, "Edge", "+3% damage, you", 30, ItemEffect.ChampionDamage, null, .8f, Add(ItemEffect.ChampionDamage, Hero, .03f)),
            N("forge.tempo", 6, 1, "Tempo", "+3% attack speed, you", 45, ItemEffect.AttackSpeed, null, .8f, Add(ItemEffect.AttackSpeed, Hero, .03f)),
            N("forge.reach", 6, 2, "Long arm", "+5% attack range, you", 60, ItemEffect.AttackRange, null, .3f, Add(ItemEffect.AttackRange, Hero, .05f)),
            N("forge.pierce", 6, 3, "Piercing", "Your shots pierce +1", 5, ItemEffect.BulletPierce, null, 1.5f, Add(ItemEffect.BulletPierce, Hero, 1), K),
            N("forge.heavy", 6, 3, "Heavy hand", "+7% damage, you", 5, ItemEffect.ChampionDamage, null, 1.8f, Add(ItemEffect.ChampionDamage, Hero, .07f), K),
            N("forge.temper", 6, 4, "Temper", "+2% damage, you and friends", 90, ItemEffect.Damage, null, 1, s => s.Damage += .02f),
            N("forge.drum", 6, 5, "War drum", "+3% friends' attack speed", 120, ItemEffect.CompanionAttackSpeed, null, .8f, Add(ItemEffect.CompanionAttackSpeed, Friends, .03f)),
            N("forge.crown", 6, 6, "Forge fire", "+8% damage and attack speed, you", 7, ItemEffect.AttackSpeed, "forge", 4, s => { AddStat(s, ItemEffect.ChampionDamage, Hero, .08f); AddStat(s, ItemEffect.AttackSpeed, Hero, .08f); }, K),
            // the hunt: weaker enemies
            N("hunt.snare", 7, 0, "Snare", "Enemies −2% speed", 30, ItemEffect.EnemySpeed, null, .5f, Add(ItemEffect.EnemySpeed, Run, -.02f)),
            N("hunt.thin", 7, 1, "Thin the herd", "Enemies −2% HP", 45, ItemEffect.EnemyHealth, null, .8f, Add(ItemEffect.EnemyHealth, Run, -.02f)),
            N("hunt.bait", 7, 2, "Bait", "+3% gold from fights", 60, null, "coin", 0, s => s.Gold += .03f),
            N("hunt.hobble", 7, 3, "Hobble", "Enemies −5% speed", 5, ItemEffect.EnemySpeed, null, 1.2f, Add(ItemEffect.EnemySpeed, Run, -.05f), K),
            N("hunt.bane", 7, 3, "Bane", "Enemies −5% HP", 5, ItemEffect.EnemyHealth, null, 2, Add(ItemEffect.EnemyHealth, Run, -.05f), K),
            N("hunt.thin2", 7, 4, "Thin the herd II", "Enemies −2% HP", 90, ItemEffect.EnemyHealth, null, .8f, Add(ItemEffect.EnemyHealth, Run, -.02f)),
            N("hunt.snare2", 7, 5, "Snare II", "Enemies −2% speed", 120, ItemEffect.EnemySpeed, null, .5f, Add(ItemEffect.EnemySpeed, Run, -.02f)),
            N("hunt.crown", 7, 6, "Night hunter", "Enemies deal −8% damage", 7, null, "hunt", 3, s => s.EnemyWeaken = .08f, K),
            // the merchant: the shop's prices and the dream market
            N("shop.haggle", 8, 0, "Haggle", "−2% shop prices", 30, ItemEffect.Discount, null, 0, Add(ItemEffect.Discount, Run, .02f)),
            N("shop.coupon", 8, 1, "Coupon", "Refreshes cost 1 gold less", 45, null, "ledger", 0, s => s.RerollDiscount += 1),
            N("shop.haggle2", 8, 2, "Haggle II", "−2% shop prices", 60, ItemEffect.Discount, null, 0, Add(ItemEffect.Discount, Run, .02f)),
            N("shop.early", 8, 3, "Early market", "The dream market comes a wave sooner", 5, null, "dream", 0, s => s.DreamSooner = true, K),
            N("shop.wide", 8, 3, "Wide stall", "+1 offer in the dream market", 5, null, "dream", 0, s => s.DreamExtra = 1, K),
            N("shop.tip", 8, 4, "Tip jar", "+1 interest cap", 90, ItemEffect.Interest, null, 0, s => s.Interest += 1, ranks: 2),
            N("shop.haggle3", 8, 5, "Haggle III", "−2% shop prices", 120, ItemEffect.Discount, null, 0, Add(ItemEffect.Discount, Run, .02f)),
            N("shop.crown", 8, 6, "Regular", "One offer in every shop is 30% off", 7, null, "coin", 0, s => s.RegularDeal = true, K),
        };

        /// <summary>The roots: open once every lantern of all nine branches is lit; twenty small ranks each, outside the cap.</summary>
        public static readonly LampNode[] Roots =
        {
            N("root.heart", -1, 0, "Heart root", "+1% max HP", 120, ItemEffect.Health, null, 0, s => s.Health += .01f, ranks: RootRanks),
            N("root.bark", -1, 1, "Bark root", "+1 armor", 120, ItemEffect.Armor, null, 0, Add(ItemEffect.Armor, Hero, .1f), ranks: RootRanks),
            N("root.thorn", -1, 2, "Thorn root", "+1% damage, you and friends", 120, ItemEffect.Damage, null, 0, s => s.Damage += .01f, ranks: RootRanks),
            N("root.gold", -1, 3, "Gold root", "+1% gold from fights", 120, null, "coin", 0, s => s.Gold += .01f, ranks: RootRanks),
        };

        public static LampNode Node(string id) => Array.Find(Tree, n => n.Id == id) ?? Array.Find(Roots, n => n.Id == id);
        public static int LampRank(PlayerProfile p, string id) => p?.Meta?.Lamps?.Find(r => r.Id == id)?.Rank ?? 0;
        public static bool Lit(PlayerProfile p, string id) => LampRank(p, id) > 0;
        public static int LitIn(PlayerProfile p, int branch) { int n = 0; foreach (LampNode node in Tree) if (node.Branch == branch && Lit(p, node.Id)) n++; return n; }
        /// <summary>Difficulties won at least once (each opens one more branch).</summary>
        public static int DifficultiesWon(PlayerProfile p) { int mask = p?.Meta?.WonMask ?? 0, n = 0; for (; mask != 0; mask &= mask - 1) n++; return n; }
        public static bool BranchOpen(PlayerProfile p, int branch) => branch == 0 || branch < 0 && TreeAllDone(p) || branch > 0 && (LitIn(p, branch - 1) >= BranchOpensAt || DifficultiesWon(p) >= branch);
        public static LampNode Fork(LampNode node) => node.Branch < 0 ? null : Array.Find(Tree, n => n != node && n.Branch == node.Branch && n.Tier == node.Tier);

        /// <summary>Why a lantern cannot be lit (or raised a rank) now (null: it can).</summary>
        public static string NodeBlocked(PlayerProfile p, LampNode node)
        {
            Ensure(p);
            int rank = LampRank(p, node.Id);
            if (rank >= node.Ranks) return "MAX";
            if (!BranchOpen(p, node.Branch)) return "Locked";
            if (Fork(node) is LampNode other && Lit(p, other.Id)) return "Other path taken";
            if (node.Branch >= 0 && node.Tier > 0 && !Array.Exists(Tree, n => n.Branch == node.Branch && n.Tier == node.Tier - 1 && Lit(p, n.Id))) return "Light the one below";
            if (Wallet(p, node.Currency) < node.CostAt(rank)) return node.Currency == MetaCurrency.LampOil ? "Not enough oil" : "Not enough keepsakes";
            return null;
        }
        public static bool Light(PlayerProfile p, string id)
        {
            LampNode node = Node(id);
            if (node == null || NodeBlocked(p, node) != null) return false;
            MetaProgress m = p.Meta;
            MetaRank r = m.Lamps.Find(x => x.Id == id);
            if (r == null) m.Lamps.Add(r = new MetaRank { Id = id });
            int cost = node.CostAt(r.Rank);
            if (node.Currency == MetaCurrency.LampOil) m.LampOil -= cost; else m.Keepsakes -= cost;
            r.Rank++;
            if (TreeFull(p, BaseBranches)) m.TreeDone = true;
            if (TreeFull(p, BranchNames.Length)) m.TreeAllDone = true;
            return true;
        }
        /// <summary>What a lantern's ranks cost so far (the refund).</summary>
        private static int Spent(LampNode node, int rank) { int sum = 0; for (int k = 0; k < Mathf.Min(rank, node.Ranks); k++) sum += node.CostAt(k); return sum; }
        /// <summary>Takes the whole tree back for free (roots too): every rank's price returns to the purse.</summary>
        public static bool Respec(PlayerProfile p)
        {
            MetaProgress m = Ensure(p);
            if (m.Lamps.Count == 0) return false;
            foreach (MetaRank r in m.Lamps)
            {
                LampNode node = Node(r.Id); if (node == null) continue;
                if (node.Currency == MetaCurrency.LampOil) m.LampOil = RunSession.BoundedCounter((long)m.LampOil + Spent(node, r.Rank));
                else m.Keepsakes = RunSession.BoundedCounter((long)m.Keepsakes + Spent(node, r.Rank));
            }
            m.Lamps.Clear();
            return true;
        }
        /// <summary>Every tier of the first <paramref name="branches"/> branches lit (a fork by either side; any rank).</summary>
        public static bool TreeFull(PlayerProfile p, int branches)
        {
            foreach (LampNode n in Tree) if (n.Branch < branches && !Lit(p, n.Id) && !(Fork(n) is LampNode f && Lit(p, f.Id))) return false;
            return true;
        }
        /// <summary>The five first branches were once all lit (opens Lightless; stays true after the Bell).</summary>
        public static bool TreeDone(PlayerProfile p) => p?.Meta != null && (p.Meta.TreeDone || TreeFull(p, BaseBranches));
        /// <summary>All nine branches were once lit (opens The Last Night and the roots).</summary>
        public static bool TreeAllDone(PlayerProfile p) => p?.Meta != null && (p.Meta.TreeAllDone || TreeFull(p, BranchNames.Length));
        public static bool AnyLightable(PlayerProfile p)
        {
            foreach (LampNode n in Tree) if (NodeBlocked(p, n) == null) return true;
            foreach (LampNode n in Roots) if (NodeBlocked(p, n) == null) return true;
            return false;
        }

        /// <summary>Lamp Oil a finished run's haul gains from Bright oil (a share).</summary>
        private static float TreeOil(PlayerProfile p) => .03f * LampRank(p, "dream.oil") + .03f * LampRank(p, "dream.oil2") + .04f * LampRank(p, "dream.oil3");

        /// <summary>The tree's part of a run's loadout: every lit lantern (each rank) into one state, its combat stats scaled to
        /// the cap, then the roots on top (outside the cap).</summary>
        private static MetaRunState TreeLoadout(PlayerProfile p, out float scale)
        {
            var tree = new MetaRunState();
            foreach (LampNode n in Tree) for (int k = LampRank(p, n.Id); k > 0; k--) n.Apply?.Invoke(tree);
            float power = CombatPower(p); scale = power > PowerCap ? PowerCap / power : 1;
            tree.Health *= scale; tree.MoveSpeed *= scale; tree.Crit *= scale; tree.TwoStarChance *= scale; tree.Damage *= scale;
            foreach (MetaStat s in tree.Stats) if (Scaled(s.Effect)) s.Value *= scale;
            foreach (LampNode n in Roots) for (int k = LampRank(p, n.Id); k > 0; k--) n.Apply?.Invoke(tree);
            return tree;
        }
        private static bool Scaled(ItemEffect e) => e != ItemEffect.Discount && e != ItemEffect.Interest && e != ItemEffect.BulletPierce;

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
        /// <summary>The first tree (lit ids, one rank each) moves into the ranked list.</summary>
        private static void MoveLitIntoRanks(MetaProgress m)
        {
            if (m.Tree == null) return;
            foreach (string id in m.Tree) if (Node(id) != null && !m.Lamps.Exists(r => r.Id == id)) m.Lamps.Add(new MetaRank { Id = id, Rank = 1 });
            m.Tree.Clear();
        }
    }
}
