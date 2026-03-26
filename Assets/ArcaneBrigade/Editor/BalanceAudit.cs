using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Quantitative balance audit (Docs/Balance_Pass_2026-10-19.md). Every stat is converted to a common "value unit" (VU):
    /// 1 VU is worth +1% team damage. Weights are authored estimates against this game's own formulas (crit hits x1.5, armor
    /// 1/(1+a), ~110 HP heroes, hero power x3, ~45 s waves, gold income vs kill coins), not Bizarre Brigade numbers.
    /// Batch: -executeMethod ArcaneBrigade.Editor.BalanceAudit.Run [-balance-label=Before]  (static tables)
    ///        -executeMethod ArcaneBrigade.Editor.BalanceAudit.Sim [-balance-label=Before]  (bot campaigns, all six difficulties)</summary>
    public static class BalanceAudit
    {
        /// <summary>Stat-only budget each rarity should carry (Common, Uncommon, Rare, Mythic, Crimson=unused, Prismatic).</summary>
        public static readonly float[] RarityTarget = { 12, 19, 28, 40, 40, 55 };
        public const float Tolerance = .3f;
        public const float HpPaidWeight = .75f; // round 4: an altar HP price is lost from MAXIMUM HP for the run, worth as much as an HP gift

        /// <summary>VU per 1.0 of the modifier's value.</summary>
        public static float Weight(ItemEffect effect)
        {
            switch (effect)
            {
                case ItemEffect.Damage: case ItemEffect.AttackSpeed: case ItemEffect.DamageCorrection: return 100;
                case ItemEffect.ChampionDamage: case ItemEffect.CompanionDamage: case ItemEffect.CompanionAttackSpeed: return 55;
                case ItemEffect.Crit: return 50;                         // a critical hit deals x1.5
                case ItemEffect.MeleeDamage: case ItemEffect.RangedDamage: return 3; // per flat point; half the brigade, base ~12
                case ItemEffect.MoveSpeed: case ItemEffect.SpeedCorrection: return 40;
                case ItemEffect.AttackRange: return 50;
                case ItemEffect.Health: return .75f;                     // per HP; 110 HP hero: +1 HP ~ +0.9% effective HP
                case ItemEffect.Heal: return 15;                          // per HP/s
                case ItemEffect.Armor: return 80;                         // +0.1 armor ~ +10% effective HP at low armor
                case ItemEffect.Dodge: return 80;
                case ItemEffect.HealthCorrection: return 80;
                case ItemEffect.HeartHealing: return 1;
                case ItemEffect.Pickup: return 4;
                case ItemEffect.Roam: return 3;
                case ItemEffect.Interceptor: return 15;
                case ItemEffect.Interest: return 2;
                case ItemEffect.Discount: return 120;
                case ItemEffect.RelicDiscount: return 80;
                case ItemEffect.RecruitDiscount: return 60;
                case ItemEffect.Luck: return 25;
                case ItemEffect.TreasureDetection: return 8;
                case ItemEffect.Income: return 2.5f;                     // per gold each wave
                case ItemEffect.CollisionDamage: return 1.2f;           // contact hits every 0.8 s; base contact is 0 for most heroes
                case ItemEffect.CollisionFrequency: return 15;
                case ItemEffect.Projectiles: return 40;
                case ItemEffect.BulletPierce: return 10;
                case ItemEffect.BulletSplit: return 12;
                case ItemEffect.BulletBounce: return 15;
                case ItemEffect.BulletSize: return 10;
                case ItemEffect.BulletSpeed: case ItemEffect.BulletDistance: case ItemEffect.Knockback: return 5;
                case ItemEffect.EnemyCount: return -30;                   // more enemies: more coins, more danger
                case ItemEffect.EnemyHealth: case ItemEffect.EnemySpeed: return -50;
                case ItemEffect.ChestQuality: return 10;
                case ItemEffect.ShopRarity: return 30;
                case ItemEffect.ArmorCorrection: case ItemEffect.HealCorrection: return 10;
                case ItemEffect.ChestReach: return 3;
                case ItemEffect.ChainChance: return 50;
                case ItemEffect.RecordedDamage: return 40;
                default: return 0; // ScoreMultiplier, BodySize, CompanionSize: no fighting value
            }
        }
        private static bool Offensive(ItemEffect e) => e == ItemEffect.Damage || e == ItemEffect.AttackSpeed || e == ItemEffect.Crit || e == ItemEffect.MeleeDamage || e == ItemEffect.RangedDamage || e == ItemEffect.AttackRange
            || e == ItemEffect.Projectiles || e == ItemEffect.BulletPierce || e == ItemEffect.BulletSplit || e == ItemEffect.BulletBounce || e == ItemEffect.BulletSize || e == ItemEffect.BulletSpeed || e == ItemEffect.BulletDistance
            || e == ItemEffect.Knockback || e == ItemEffect.CollisionDamage || e == ItemEffect.CollisionFrequency || e == ItemEffect.MoveSpeed || e == ItemEffect.DamageCorrection || e == ItemEffect.SpeedCorrection;
        /// <summary>A team-wide offensive stat limited to the hero or to the companions is worth about half.</summary>
        public static float Value(StatModifier m) => Weight(m.Effect) * m.Value * (Offensive(m.Effect) && m.Scope != ModifierScope.Team && m.Scope != ModifierScope.Run ? .55f : 1);
        public static float Budget(IEnumerable<StatModifier> mods) => mods?.Sum(Value) ?? 0;
        public static float Positive(IEnumerable<StatModifier> mods) => mods?.Select(Value).Where(v => v > 0).Sum() ?? 0;
        public static float Negative(IEnumerable<StatModifier> mods) => mods?.Select(Value).Where(v => v < 0).Sum() ?? 0;
        public static float Target(ItemRarity rarity) => RarityTarget[Mathf.Clamp((int)rarity, 0, RarityTarget.Length - 1)];

        public static bool Proc(RunCatalog c, ItemDefinition item) => (c.ProcRules ?? new ProcRule[0]).Any(rule => rule.ItemId == item.Id);
        /// <summary>Score/challenge items buy points with enemy strength; they are kept out of the rarity budget.</summary>
        public static bool ScoreItem(ItemDefinition item) => item.EffectiveModifiers.Any(m => m.Effect == ItemEffect.ScoreMultiplier && m.Value > 0);
        public static bool MechanicItem(RunCatalog c, ItemDefinition item) => item.Mechanic != RelicMechanic.None || item.SignatureRole != SignatureRelicKind.None || Proc(c, item);
        private static readonly HashSet<ItemEffect> UtilityEffects = new HashSet<ItemEffect> { ItemEffect.Pickup, ItemEffect.Roam, ItemEffect.Interest, ItemEffect.Luck, ItemEffect.TreasureDetection,
            ItemEffect.ChestQuality, ItemEffect.ShopRarity, ItemEffect.ChestReach, ItemEffect.Discount, ItemEffect.RelicDiscount, ItemEffect.RecruitDiscount, ItemEffect.Income, ItemEffect.BulletSpeed,
            ItemEffect.BulletDistance, ItemEffect.BulletSize, ItemEffect.Knockback, ItemEffect.BodySize, ItemEffect.CompanionSize, ItemEffect.HeartHealing, ItemEffect.ArmorCorrection, ItemEffect.HealCorrection, ItemEffect.CollisionFrequency, ItemEffect.SpeedCorrection };
        /// <summary>Economy/loot/obscure-stat items: their value is the hardest to price in VU, so their band floor is 50% of the target.</summary>
        public static bool UtilityItem(ItemDefinition item) { var mods = item.EffectiveModifiers; float positive = Positive(mods); return positive > 0 && mods.Where(m => UtilityEffects.Contains(m.Effect)).Select(Value).Where(v => v > 0).Sum() > positive * .5f; }
        public static float Floor(ItemDefinition item) => UtilityItem(item) ? .5f : 1 - Tolerance;
        public static bool InShop(ItemDefinition item) => item.ShopWeight > 0 && item.UnlockWave < 1000;
        public static int GoldOffer(ItemDefinition item, float stage = 1) => Mathf.CeilToInt(item.Price * .85f * (1 + (int)item.Rarity * .4f) * stage);
        public static int AltarOffer(ItemDefinition item) => Mathf.Max(1, Mathf.CeilToInt(item.AltarPrice * (1 + (int)item.Rarity * .25f)));
        public static float HpGain(ItemDefinition item) => item.EffectiveModifiers.Where(m => m.Effect == ItemEffect.Health && m.Value > 0).Sum(m => m.Value);
        public static float HpCost(ItemDefinition item) => -item.EffectiveModifiers.Where(m => m.Effect == ItemEffect.Health && m.Value < 0).Sum(m => m.Value);

        private static string Label()
        {
            foreach (string arg in Environment.GetCommandLineArgs()) if (arg.StartsWith("-balance-label=")) return arg.Substring(15);
            return "Current";
        }
        private static string F(float v) => v.ToString("0.##", CultureInfo.InvariantCulture);
        private static string Q(string s) => "\"" + (s ?? "").Replace("\"", "'") + "\"";
        private static string Mods(IEnumerable<StatModifier> mods) => string.Join("; ", (mods ?? new StatModifier[0]).Select(m => m.Effect + "/" + m.Scope.ToString()[0] + " " + F(m.Value)));
        private static float Percentile(List<float> sorted, float p) => sorted.Count == 0 ? 0 : sorted[Mathf.Clamp(Mathf.RoundToInt(p * (sorted.Count - 1)), 0, sorted.Count - 1)];

        [MenuItem("Arcane Brigade/Balance/Audit (static tables)")]
        public static void Run()
        {
            string root = "PreviewExports/Balance/" + Label();
            Directory.CreateDirectory(root);
            RunCatalog c = Resources.Load<RunCatalog>("RunCatalog");
            var summary = new StringBuilder();
            summary.AppendLine("# Balance audit — " + Label() + " (" + DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) + ")");
            summary.AppendLine("1 VU = +1% team damage. Rarity stat targets C/U/R/M/P = " + string.Join("/", new[] { 0, 1, 2, 3, 5 }.Select(i => F(RarityTarget[i]))) + " VU, tolerance ±" + Tolerance * 100 + "%.\n");

            // ---------------- items
            var items = new List<string> { "id,name,rarity,category,kind,in_shop,pools,base_price,gold_offer_w1,altar_currency,altar_offer,unlock,stacking,max_stacks,weight,mechanic,modifiers,vu_positive,vu_negative,vu_net,hp_gain,hp_cost,target,ratio,flag,vu_per_gold" };
            var byRarity = new Dictionary<ItemRarity, List<float>>();
            var flagged = new List<(float ratio, string line)>();
            foreach (ItemDefinition item in c.Items)
            {
                var mods = item.EffectiveModifiers;
                float net = Budget(mods), target = Target(item.Rarity), ratio = net / target;
                bool mech = MechanicItem(c, item), score = ScoreItem(item);
                string kind = score ? "score" : mech ? "mechanic" : UtilityItem(item) ? "utility" : "stat";
                string flag = kind == "score" || kind == "mechanic" ? (Positive(mods) > target * (1 + Tolerance) ? "STAT_PART_OVER" : kind.ToUpperInvariant()) : ratio > 1 + Tolerance ? "OVER" : ratio < Floor(item) ? "UNDER" : "OK";
                if (kind == "stat" || kind == "utility") { if (!byRarity.ContainsKey(item.Rarity)) byRarity[item.Rarity] = new List<float>(); byRarity[item.Rarity].Add(net); }
                int offer = GoldOffer(item);
                items.Add(string.Join(",", item.Id, Q(item.Name), item.Rarity, item.Category, kind, InShop(item), item.Pools, item.Price, offer, item.AltarCurrency, AltarOffer(item), item.UnlockWave, item.Stacking, item.StackLimit,
                    item.ShopWeight, item.Mechanic, Q(Mods(mods)), F(Positive(mods)), F(Negative(mods)), F(net), F(HpGain(item)), F(HpCost(item)), F(target), F(ratio), flag, F(net / Mathf.Max(1, offer))));
                if (flag == "OVER" || flag == "UNDER" || flag == "STAT_PART_OVER") flagged.Add((ratio, item.Id + " (" + item.Rarity + ", " + Mods(mods) + "): " + F(net) + " VU vs " + F(target) + " → " + flag));
            }
            File.WriteAllLines(root + "/Items.csv", items);
            var rarity = new List<string> { "rarity,stat_items,min,p25,median,p75,max,mean,target,within_tolerance" };
            summary.AppendLine("## Stat-only item budget by rarity (stat + utility items; mechanic and score items listed in Items.csv)\n\n| Rarity | n | min | median | max | target | within ±30% |\n|---|---:|---:|---:|---:|---:|---:|");
            foreach (var pair in byRarity.OrderBy(p => p.Key))
            {
                var s = pair.Value.OrderBy(v => v).ToList(); float t = Target(pair.Key);
                int ok = s.Count(v => v >= t * (1 - Tolerance) && v <= t * (1 + Tolerance)); // strict band, utility items included
                rarity.Add(string.Join(",", pair.Key, s.Count, F(s[0]), F(Percentile(s, .25f)), F(Percentile(s, .5f)), F(Percentile(s, .75f)), F(s[s.Count - 1]), F(s.Average()), F(t), ok));
                summary.AppendLine("| " + pair.Key + " | " + s.Count + " | " + F(s[0]) + " | " + F(Percentile(s, .5f)) + " | " + F(s[s.Count - 1]) + " | " + F(t) + " | " + ok + "/" + s.Count + " |");
            }
            File.WriteAllLines(root + "/RarityBudget.csv", rarity);
            summary.AppendLine("\n## Flagged items (" + flagged.Count + ")\n");
            foreach (var f in flagged.OrderByDescending(f => Mathf.Abs(f.ratio - 1))) summary.AppendLine("- " + f.line);
            summary.AppendLine("\n## Max HP gifts and HP costs\n");
            foreach (ItemDefinition item in c.Items.Where(i => HpGain(i) >= 20 || HpCost(i) > 0 || i.AltarCurrency == ShopCurrency.Health && (i.Pools & ShopPool.Eggs) != 0).OrderBy(i => i.Rarity))
                summary.AppendLine("- " + item.Id + " " + item.Rarity + ": +" + F(HpGain(item)) + " max HP, −" + F(HpCost(item)) + " max HP" + ((item.Pools & ShopPool.Eggs) != 0 && item.AltarCurrency == ShopCurrency.Health ? ", altar price " + AltarOffer(item) + " current HP" : "") + ", net " + F(Budget(item.EffectiveModifiers)) + " VU");

            // ---------------- companions
            var comps = new List<string> { "id,rarity,special,price,passive,passive_strength,weapon,weapon_damage,cooldown,support_modifiers,vu_tier1,vu_tier2,vu_tier3,defensive_support" };
            var compByRarity = new Dictionary<ItemRarity, List<float>>();
            foreach (UnitDefinition u in c.Companions)
            {
                float vu = Budget(u.CompanionModifiers);
                bool defensive = (u.CompanionModifiers ?? new StatModifier[0]).Any(m => m.Value > 0 && (m.Effect == ItemEffect.Heal || m.Effect == ItemEffect.Armor || m.Effect == ItemEffect.Dodge || m.Effect == ItemEffect.Health));
                comps.Add(string.Join(",", u.Id, u.Rarity, u.SpecialRecruit, u.Price, u.Passive, F(u.PassiveStrength), u.Weapon != null ? u.Weapon.Id : "", u.Weapon != null ? F(u.Weapon.Damage) : "", u.Weapon != null ? F(u.Weapon.Cooldown) : "",
                    Q(Mods(u.CompanionModifiers)), F(vu), F(vu * UnitDefinition.ModifierScale(2)), F(vu * UnitDefinition.ModifierScale(3)), defensive));
                if (!compByRarity.ContainsKey(u.Rarity)) compByRarity[u.Rarity] = new List<float>();
                compByRarity[u.Rarity].Add(vu);
            }
            File.WriteAllLines(root + "/Companions.csv", comps);
            summary.AppendLine("\n## Companion support modifiers (tier 1; tiers x2/x4)\n\n| Rarity | n | min | median | max |\n|---|---:|---:|---:|---:|");
            foreach (var pair in compByRarity.OrderBy(p => p.Key)) { var s = pair.Value.OrderBy(v => v).ToList(); summary.AppendLine("| " + pair.Key + " | " + s.Count + " | " + F(s[0]) + " | " + F(Percentile(s, .5f)) + " | " + F(s[s.Count - 1]) + " |"); }

            // ---------------- bonds
            var bonds = new List<string> { "id,name,effect,threshold,value,extras,reward_2,reward_4,reward_6,reward_8,vu_2,vu_4,vu_6,vu_8,vu_per_member_at_threshold" };
            summary.AppendLine("\n## Bonds (VU of the reward at 2/4/6 members)\n");
            foreach (BondDefinition b in c.Bonds)
            {
                float R(int n) => b.Reward(n);
                float V(int n) => Value(new StatModifier { Effect = b.Effect, Scope = RunModifierRules.DefaultScope(b.Effect), Value = R(n) });
                bonds.Add(string.Join(",", b.Id, Q(b.Name), b.Effect, b.Threshold, F(b.Value), Q(string.Join(" ", (b.ExtraBonuses ?? new BondThresholdBonus[0]).Select(x => x.Threshold + ":" + F(x.Value)))),
                    F(R(2)), F(R(4)), F(R(6)), F(R(8)), F(V(2)), F(V(4)), F(V(6)), F(V(8)), F(V(b.Threshold) / Mathf.Max(1, b.Threshold))));
            }
            File.WriteAllLines(root + "/Bonds.csv", bonds);
            var bondVu = c.Bonds.Select(b => (b, v: Value(new StatModifier { Effect = b.Effect, Scope = RunModifierRules.DefaultScope(b.Effect), Value = b.Reward(b.Threshold) }) / Mathf.Max(1, b.Threshold))).OrderBy(x => x.v).ToList();
            summary.AppendLine("VU per member at first threshold: min " + F(bondVu.First().v) + " (" + bondVu.First().b.Id + "), median " + F(bondVu[bondVu.Count / 2].v) + ", max " + F(bondVu.Last().v) + " (" + bondVu.Last().b.Id + ").");

            // ---------------- heroes
            var heroes = new List<string> { "id,hp,speed,crit,mechanic,weapon,kind,damage,cooldown,range,projectiles,hero_power,base_dps,champion_modifiers,vu_modifiers" };
            foreach (UnitDefinition h in c.Champions)
            {
                WeaponDefinition w = h.Weapon; float power = PlayerBuildBalance.HeroPower(h.Id);
                heroes.Add(string.Join(",", h.Id, F(h.Health), F(h.Speed), F(h.CriticalChance), h.HeroMechanic, w.Id, w.Kind, F(w.Damage), F(w.Cooldown), F(w.Range), w.ProjectileCount, F(power),
                    F(w.Damage * power * Mathf.Max(1, w.ProjectileCount) / Mathf.Max(.05f, w.Cooldown)), Q(Mods(h.ChampionModifiers)), F(Budget(h.ChampionModifiers))));
            }
            File.WriteAllLines(root + "/Heroes.csv", heroes);
            var hp = c.Champions.Select(h => h.Health).OrderBy(v => v).ToList();
            summary.AppendLine("\n## Heroes\n\nBase HP min/median/max " + F(hp[0]) + "/" + F(Percentile(hp, .5f)) + "/" + F(hp[hp.Count - 1]) + ". See Heroes.csv for DPS and kit modifiers.");

            // ---------------- enemies (compiled through the production RunSession)
            var enemies = new List<string> { "difficulty,wave,melee_id,hp_multiplier,melee_hp,contact_damage,weapon_damage,enemy_budget,spawn_interval,boss_id,boss_hp" };
            var hpTable = new float[RunMeta.DifficultyNames.Length, c.NormalWaves + 1];
            for (int d = 0; d < RunMeta.DifficultyNames.Length; d++)
                for (int wave = 1; wave <= c.NormalWaves; wave++)
                    using (var s = new RunSession(c))
                    {
                        s.Start("LanternKnight", 4242, d); s.State.Wave = wave;
                        CheckpointCatalog arena = s.PrepareCombat();
                        UnitDefinition source = c.Enemies.FirstOrDefault(e => e.Id == arena.MeleeEnemy.Id);
                        float mult = source == null ? 0 : arena.MeleeEnemy.Health / Mathf.Max(.01f, source.Health);
                        hpTable[d, wave] = mult;
                        enemies.Add(string.Join(",", d, wave, arena.MeleeEnemy.Id, F(mult), F(arena.MeleeEnemy.Health), F(arena.MeleeEnemy.ContactDamage), F(arena.MeleeEnemy.Weapon.Damage), arena.EnemyBudget, F(arena.SpawnInterval), arena.Boss != null ? arena.Boss.Id : "", arena.Boss != null ? F(arena.Boss.Health) : ""));
                    }
            File.WriteAllLines(root + "/Enemies.csv", enemies);
            summary.AppendLine("\n## Enemy HP multiplier (compiled / species base)\n\n| Difficulty | w1 | w5 | w10 | w15 | w20 | max wave-to-wave step | ratio to previous difficulty at w20 |\n|---|---:|---:|---:|---:|---:|---:|---:|");
            for (int d = 0; d < RunMeta.DifficultyNames.Length; d++)
            {
                float step = 0; for (int w = 2; w <= c.NormalWaves; w++) step = Mathf.Max(step, hpTable[d, w] / Mathf.Max(.01f, hpTable[d, w - 1]));
                summary.AppendLine("| " + d + " " + RunMeta.DifficultyNames[d] + " | " + F(hpTable[d, 1]) + " | " + F(hpTable[d, 5]) + " | " + F(hpTable[d, 10]) + " | " + F(hpTable[d, 15]) + " | " + F(hpTable[d, 20]) + " | " + F(step) + " | " + (d == 0 ? "-" : F(hpTable[d, 20] / hpTable[d - 1, 20])) + " |");
            }

            // ---------------- static economy
            var econ = new List<string> { "wave,stage_price_factor,common_offer,uncommon_offer,rare_offer,mythic_offer,companion_offer,reroll_1,reroll_3,base_income,interest_cap" };
            using (var s = new RunSession(c))
            {
                s.Start("LanternKnight", 4242, 0);
                for (int wave = 1; wave <= c.NormalWaves; wave++)
                {
                    s.State.Wave = wave; float stage = s.StagePriceFactor;
                    string Mean(ItemRarity r) { var list = c.Items.Where(i => i.Rarity == r && InShop(i) && (i.Pools & ShopPool.Gold) != 0).ToList(); return list.Count == 0 ? "" : F((float)list.Average(i => GoldOffer(i, stage))); }
                    float companion = (float)c.Companions.Where(u => !u.SpecialRecruit).Average(u => u.Price) * stage;
                    s.State.RerollsThisShop = 0; int r1 = s.RerollCost; s.State.RerollsThisShop = 2; int r3 = s.RerollCost; s.State.RerollsThisShop = 0;
                    econ.Add(string.Join(",", wave, F(stage), Mean(ItemRarity.Common), Mean(ItemRarity.Uncommon), Mean(ItemRarity.Rare), Mean(ItemRarity.Mythic), F(companion), r1, r3, c.Income * 3 / 4 + wave, Mathf.Min(10, c.InterestCap)));
                }
            }
            File.WriteAllLines(root + "/Economy_Static.csv", econ);
            summary.AppendLine("\nEconomy constants: starting gold " + c.StartingGold + ", wave income " + c.Income * 3 / 4 + " + wave, interest gold/" + c.InterestDivisor + " capped at " + Mathf.Min(10, c.InterestCap) + ", reroll (2 + 2n) x max(1, stage factor / 2) since 19/10/2026 (was 2 + 2n), gold offer = price x 0.85 x (1 + 0.4 rarity) x stage factor, altar offer = altar price x (1 + 0.25 rarity). See Economy_Static.csv and the Sim outputs.");
            File.WriteAllText(root + "/Summary.md", summary.ToString());
            Debug.Log("BALANCE_AUDIT_PASSED " + root + " items=" + c.Items.Length + " flagged=" + flagged.Count);
        }

        // ============================================================ bot campaigns
        private static readonly string[] SimHeroes = { "LanternKnight", "Postmaster", "DrowsySeer", "CometRunner", "GlassDice", "TideSinger" };
        private static readonly int[] SimSeeds = { 161026, 77138, 90210, 424242, 5005 };

        /// <summary>Value of an offer for the audit bot: VU of the stats; mechanic items count 60% of their rarity target on top.</summary>
        public static float OfferValue(RunSession s, RunCatalog c, ShopOffer o)
        {
            if (o.Kind == OfferKind.Companion)
            {
                // round 3: absorb heroes (Wise, Capped Gallant) turn a bought friend into a skill; only the last three are worn as
                // memory organs, and every Wise absorption adds +4% enemy health, so the bot stops once three distinct skills are held
                string reference = c.Unit(s.State.ChampionId).ReferenceHero;
                if (reference == "Wise" || reference == "Capped Gallant")
                {
                    int distinct = s.State.AbsorbedSkills.Distinct().Count();
                    return distinct < 3 && !s.State.AbsorbedSkills.Contains(o.Id) ? 40 : reference == "Wise" ? -1 : 5;
                }
                return ShopMergePreview.Ready(s, o) ? 60 : s.State.Roster.Count < OctoberBalance.AllySoftLimit ? 25 : -1;
            }
            ItemDefinition item = c.Item(o.Id);
            if (ScoreItem(item)) return -1;
            float v = Budget(item.EffectiveModifiers);
            if (MechanicItem(c, item)) v = Mathf.Max(0, v) + .6f * Target(item.Rarity) - MechanicCost(item, s.StackCount(item.Id));
            if (o.Currency == ShopCurrency.Health)
            {
                // round 2: never pay the altar below 70% of max HP in one shop (the bot once entered waves at half health and died in seconds)
                float maximum = c.StatRevision > 0 ? K10StatRules.MaximumHealth(s, c) : s.State.Health;
                // round 4: the cut is permanent, so the bot never gives up more than 30% of its uncut maximum over a run
                if (o.Price > s.State.Health * .3f || s.State.Health - o.Price < .7f * maximum || s.State.BloodPaid + o.Price > .3f * s.UncutMaximumHealth) return -1;
                v -= o.Price * HpPaidWeight;
            }
            return v;
        }

        /// <summary>Round 2: self-harm and decaying mechanics the stat budget cannot see. Without it the audit bot stacked Silent
        /// Alarm (1 HP/s per copy) and bled to death in long waves at every difficulty.</summary>
        public static float MechanicCost(ItemDefinition item, int owned)
        {
            switch (item.Mechanic)
            {
                case RelicMechanic.BleedingAlarm: return 30 * (owned + 1);           // lose 1 HP per second per copy
                case RelicMechanic.HalfHealthSeed: return 25;                        // every battle starts at half health
                case RelicMechanic.CollisionOnly: return 60;                         // the hero stops attacking
                case RelicMechanic.SixWeapons: return 30;                            // hero and allies stop attacking
                case RelicMechanic.FragilePower: return .8f * Budget(item.EffectiveModifiers); // the +100% damage decays per hit taken
                case RelicMechanic.ChestOnlyRecovery: return 20;                     // regeneration and hearts stop healing
                default: return 0;
            }
        }

        [MenuItem("Arcane Brigade/Balance/Bot campaigns")]
        public static void Sim() => SimWith(1);
        /// <summary>Three seeds per hero x difficulty cell: 108 campaigns (Round 2 of the balance pass).</summary>
        public static void Sim3() => SimWith(3);
        /// <summary>Round 3: every hero at Wandering and Nightmare, three seeds (216 campaigns), with a per-hero summary.</summary>
        public static void SweepHeroes() => SimWith(3, Resources.Load<RunCatalog>("RunCatalog").Champions.Select(h => h.Id).ToArray(), new[] { 1, 3 });
        /// <summary>Round 3 diagnosis: the weakest heroes at Wandering, three seeds, with the last damage source of each run.</summary>
        public static void SweepWeakHeroes() => SimWith(3, new[] { "ArchiveCurator", "IvorySovereign", "LoneJelly", "DrowsySeer", "TideSinger", "CometRunner", "PatchworkHost", "NightMaw" }, new[] { 1 });
        private static void SimWith(int seedCount, string[] heroList = null, int[] difficultyList = null)
        {
            foreach (string arg in Environment.GetCommandLineArgs()) if (arg.StartsWith("-balance-seeds=")) seedCount = int.Parse(arg.Substring(15));
            int[] seeds = SimSeeds.Take(Mathf.Clamp(seedCount, 1, SimSeeds.Length)).ToArray();
            string root = "PreviewExports/Balance/" + Label();
            Directory.CreateDirectory(root);
            RunCatalog c = Resources.Load<RunCatalog>("RunCatalog");
            var rows = new List<string> { "hero,difficulty,wave,result,seconds,kills,combat_gold,income,interest,wallet_at_shop,spent_items,spent_recruits,spent_rerolls,items_bought,recruits_bought,rerolls,hp_paid,mean_offer_price,max_hp,allies,seed,damage_taken,boss_wave" };
            var runs = new List<string> { "hero,difficulty,outcome,last_wave,total_gold,items,recruits,rerolls,seed,last_damage_source,last_damage_kind,last_damage" };
            heroList ??= SimHeroes; difficultyList ??= Enumerable.Range(0, RunMeta.DifficultyNames.Length).ToArray();
            foreach (string hero in heroList)
                foreach (int difficulty in difficultyList)
                    foreach (int SimSeed in seeds)
                    using (var session = new RunSession(c))
                    {
                        session.Start(hero, SimSeed, difficulty);
                        string outcome = "TIMEOUT"; long total = 0; int items = 0, recruits = 0, rerolls = 0;
                        for (int encounter = 0; encounter < c.NormalWaves; encounter++)
                        {
                            CheckpointCatalog combat = session.PrepareCombat();
                            var sim = new CombatSimulation(combat); sim.Start(unchecked(SimSeed + session.State.Wave * 397));
                            int limit = combat.Boss == null ? 1800 : 6000;
                            float taken = 0, lastHealth = sim.Champion.Health;
                            for (int tick = 0; tick < limit && sim.Phase == RunPhase.Combat; tick++)
                            {
                                sim.Step(.05f, K13MovementPolicy.Steer(sim));
                                if (sim.Champion.Health < lastHealth) taken += lastHealth - sim.Champion.Health;
                                lastHealth = sim.Champion.Health;
                            }
                            bool settled = sim.Phase == RunPhase.Victory || sim.Phase == RunPhase.Defeat;
                            int wave = session.State.Wave;
                            if (settled) session.Settle(sim);
                            if (session.State.Phase == SessionPhase.Defeat) outcome = "DEFEAT";
                            if (session.State.Phase == SessionPhase.Victory) outcome = "VICTORY";
                            total += sim.Gold + (session.State.Phase == SessionPhase.Shop ? session.State.LastIncome + session.State.LastInterest : 0);
                            if (session.State.Phase == SessionPhase.Shop)
                            {
                                // prism gifts and the Keeper's offer hold the shop until answered: take the first gift, decline the Keeper
                                if (session.State.OutsideStage == OutsideStageStatus.Reward) for (int i = 0; i < 3 && !session.ClaimOutsideStageReward(i); i++) { }
                                if (session.SecretRoomOpen) { session.ClaimSecretBattleReward(); session.LeaveSecretEvent(); }
                                if (session.OutsideChallengeAvailable) session.DeclineOutsideStage();
                            }
                            var t = new Tally { Wallet = session.State.Gold };
                            double mean = session.State.Offers.Count == 0 ? 0 : session.State.Offers.Average(o => o.Price);
                            if (session.State.Phase == SessionPhase.Shop) { session.PlaceHeroOrder(); Purchase(session, c, t); } // Penny Broker's forward order (no-op for others)
                            items += t.Items; recruits += t.Recruits; rerolls += t.Rerolls;
                            rows.Add(string.Join(",", hero, difficulty, wave, sim.Phase, F(sim.Elapsed), sim.Kills, sim.Gold, session.State.LastIncome, session.State.LastInterest, t.Wallet, t.SpentItems, t.SpentRecruits, t.SpentRerolls, t.Items, t.Recruits, t.Rerolls, t.HpPaid,
                                F((float)mean), F(combat.Champion.Health), session.State.Roster.Count, SimSeed, F(taken), combat.Boss != null ? 1 : 0));
                            if (!settled || session.State.Phase != SessionPhase.Shop) break;
                            session.ContinueWave();
                        }
                        runs.Add(string.Join(",", hero, difficulty, outcome, session.State.Wave, total, items, recruits, rerolls, SimSeed, Q(session.State.LastDamageSource), Q(session.State.LastDamageKind), F(session.State.LastDamageAmount)));
                        File.WriteAllLines(root + "/Sim_Waves.csv", rows); File.WriteAllLines(root + "/Sim_Runs.csv", runs);
                        Debug.Log("BALANCE_SIM " + runs[runs.Count - 1]);
                    }
            // summary
            var sb = new StringBuilder("# Bot campaigns — " + Label() + "\n\n" + heroList.Length + " heroes x " + difficultyList.Length + " difficulties x " + seeds.Length + " seed(s) (" + string.Join(", ", seeds) + "), VU-scoring buyer (buys every positive offer it can afford, rerolls while gold > reroll + cheapest wanted offer, at most 12 rerolls/shop). Normal waves cut at 90 s, boss waves 300 s; TIMEOUT is unresolved. Bot evidence, not human win rates.\n\n| Difficulty | Runs | Wins | Win % | Defeats | Timeouts | Median last wave |\n|---|---:|---:|---:|---:|---:|---:|\n");
            var parsed = runs.Skip(1).Select(r => r.Split(',')).ToList();
            for (int d = 0; d < RunMeta.DifficultyNames.Length; d++)
            {
                var set = parsed.Where(p => p[1] == d.ToString()).ToList();
                var waves = set.Select(p => int.Parse(p[3])).OrderBy(v => v).ToList();
                sb.AppendLine("| " + d + " " + RunMeta.DifficultyNames[d] + " | " + set.Count + " | " + set.Count(p => p[2] == "VICTORY") + " | " + (100.0 * set.Count(p => p[2] == "VICTORY") / Math.Max(1, set.Count)).ToString("0", CultureInfo.InvariantCulture) + "% | " + set.Count(p => p[2] == "DEFEAT") + " | " + set.Count(p => p[2] == "TIMEOUT") + " | " + (waves.Count == 0 ? "-" : waves[waves.Count / 2].ToString()) + " |");
            }
            sb.AppendLine("\n| Waves | Shops | Mean gold earned/wave | Mean wallet at shop | Mean offer price | Items/shop | Recruits/shop | Buys/shop | Rerolls/shop | Wallet / offer price |\n|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|");
            var waveRows = rows.Skip(1).Select(r => r.Split(',')).Where(p => p[3] == "Victory" && p[17] != "0").ToList();
            foreach (var band in new[] { (1, 5), (6, 10), (11, 15), (16, 19) })
            {
                var set = waveRows.Where(p => int.Parse(p[2]) >= band.Item1 && int.Parse(p[2]) <= band.Item2).ToList();
                if (set.Count == 0) continue;
                double D(string[] p, int i) => double.Parse(p[i], CultureInfo.InvariantCulture);
                double earned = set.Average(p => D(p, 6) + D(p, 7) + D(p, 8)), wallet = set.Average(p => D(p, 9)), price = set.Average(p => D(p, 17));
                sb.AppendLine("| " + band.Item1 + "–" + band.Item2 + " | " + set.Count + " | " + earned.ToString("0", CultureInfo.InvariantCulture) + " | " + wallet.ToString("0", CultureInfo.InvariantCulture) + " | " + price.ToString("0", CultureInfo.InvariantCulture) + " | "
                    + set.Average(p => D(p, 13)).ToString("0.0", CultureInfo.InvariantCulture) + " | " + set.Average(p => D(p, 14)).ToString("0.0", CultureInfo.InvariantCulture) + " | " + set.Average(p => D(p, 13) + D(p, 14)).ToString("0.0", CultureInfo.InvariantCulture) + " | " + set.Average(p => D(p, 15)).ToString("0.0", CultureInfo.InvariantCulture) + " | " + (wallet / Math.Max(1, price)).ToString("0.0", CultureInfo.InvariantCulture) + " |");
            }
            // round 3: damage taken per wave as a share of max HP, boss waves vs normal waves (completed waves only)
            var hurt = rows.Skip(1).Select(r => r.Split(',')).Where(p => p.Length >= 23 && (p[3] == "Victory" || p[3] == "Defeat")).ToList();
            double Share(string[] p) => Math.Min(1, double.Parse(p[21], CultureInfo.InvariantCulture) / Math.Max(1, double.Parse(p[18], CultureInfo.InvariantCulture)));
            var bossHurt = hurt.Where(p => p[22] == "1").ToList(); var normalHurt = hurt.Where(p => p[22] == "0").ToList();
            if (bossHurt.Count > 0 && normalHurt.Count > 0)
            {
                double b = bossHurt.Average(Share), n = normalHurt.Average(Share), bd = bossHurt.Count(p => p[3] == "Defeat") / (double)bossHurt.Count, nd = normalHurt.Count(p => p[3] == "Defeat") / (double)normalHurt.Count;
                string P(double v) => v.ToString("0.000", CultureInfo.InvariantCulture);
                sb.AppendLine("\n| Resolved waves (won or lost) | Count | Mean HP lost / max HP (death = 1) | Death rate per wave |\n|---|---:|---:|---:|\n| Boss waves | " + bossHurt.Count + " | " + P(b) + " | " + P(bd) + " |\n| Normal waves | " + normalHurt.Count + " | " + P(n) + " | " + P(nd) + " |\n| Ratio boss / normal | | " + (b / Math.Max(1e-6, n)).ToString("0.00", CultureInfo.InvariantCulture) + " | " + (bd / Math.Max(1e-6, nd)).ToString("0.00", CultureInfo.InvariantCulture) + " |");
            }
            if (heroList.Length > SimHeroes.Length)
            {
                sb.AppendLine("\n## Per hero\n\n| Hero | " + string.Join(" | ", difficultyList.Select(d => RunMeta.DifficultyNames[d] + " wins")) + " | Median last wave |\n|---|" + string.Concat(difficultyList.Select(d => "---:|")) + "---:|");
                foreach (string hero in heroList)
                {
                    var set = parsed.Where(p => p[0] == hero).ToList();
                    var waves = set.Select(p => int.Parse(p[3])).OrderBy(v => v).ToList();
                    sb.AppendLine("| " + hero + " | " + string.Join(" | ", difficultyList.Select(d => set.Count(p => p[1] == d.ToString() && p[2] == "VICTORY") + "/" + set.Count(p => p[1] == d.ToString()))) + " | " + (waves.Count == 0 ? "-" : waves[waves.Count / 2].ToString()) + " |");
                }
            }
            File.WriteAllText(root + "/Sim_Summary.md", sb.ToString());
            Debug.Log("BALANCE_SIM_PASSED " + root + " runs=" + (runs.Count - 1));
        }
        /// <summary>Round 3 diagnosis: per wave, the hero's share of damage, kit casts, awakenings, damage taken and lowest HP for
        /// the weak heroes and two strong references (Wandering, two seeds). Output: Diagnose_Waves.csv.</summary>
        public static void SweepDiagnose()
        {
            string root = "PreviewExports/Balance/" + Label();
            Directory.CreateDirectory(root);
            RunCatalog c = Resources.Load<RunCatalog>("RunCatalog");
            string[] heroes = { "ArchiveCurator", "LoneJelly", "TideSinger", "DrowsySeer", "IvorySovereign", "CometRunner", "PatchworkHost", "NightMaw", "Postmaster", "GlassDice" };
            var rows = new List<string> { "hero,seed,wave,result,seconds,kills,hero_damage,team_damage,hero_share,kit_casts,morph_casts,awakenings,damage_taken,lowest_hp_pct,max_hp,allies,absorbed,items,last_damage_source" };
            foreach (string hero in heroes)
                foreach (int seed in SimSeeds.Take(2))
                    using (var session = new RunSession(c))
                    {
                        session.Start(hero, seed, 1);
                        for (int encounter = 0; encounter < c.NormalWaves; encounter++)
                        {
                            CheckpointCatalog combat = session.PrepareCombat();
                            var sim = new CombatSimulation(combat); sim.Start(unchecked(seed + session.State.Wave * 397));
                            int limit = combat.Boss == null ? 1800 : 6000, casts = 0, morphs = 0, awakenings = 0; bool awake = false;
                            float taken = 0, lowest = 1, last = sim.Champion.Health;
                            for (int tick = 0; tick < limit && sim.Phase == RunPhase.Combat; tick++)
                            {
                                sim.Step(.05f, K13MovementPolicy.Steer(sim));
                                foreach (CombatEvent e in sim.Events) { if (e.Kind == CombatEventKind.HeroIdentityCast) casts++; else if (e.Kind == CombatEventKind.BuildMorphCast) morphs++; }
                                if (sim.HeroAwakening && !awake) awakenings++;
                                awake = sim.HeroAwakening;
                                if (sim.Champion.Health < last) taken += last - sim.Champion.Health;
                                last = sim.Champion.Health; lowest = Mathf.Min(lowest, sim.Champion.Health / Mathf.Max(1, sim.Champion.Definition.Health));
                            }
                            float heroDamage = sim.Contributions.Where(x => x.UnitId == hero).Sum(x => x.Damage), team = sim.Contributions.Sum(x => x.Damage);
                            bool settled = sim.Phase == RunPhase.Victory || sim.Phase == RunPhase.Defeat;
                            int wave = session.State.Wave;
                            if (settled) session.Settle(sim);
                            rows.Add(string.Join(",", hero, seed, wave, sim.Phase, F(sim.Elapsed), sim.Kills, F(heroDamage), F(team), F(heroDamage / Mathf.Max(1, team)), casts, morphs, awakenings, F(taken), F(lowest), F(sim.Champion.Definition.Health),
                                session.State.Roster.Count, session.State.AbsorbedSkills.Count, session.State.Items.Sum(x => x.Count), Q(sim.LastDamageSource)));
                            if (!settled || session.State.Phase != SessionPhase.Shop) break;
                            if (session.State.OutsideStage == OutsideStageStatus.Reward) for (int i = 0; i < 3 && !session.ClaimOutsideStageReward(i); i++) { }
                            if (session.SecretRoomOpen) { session.ClaimSecretBattleReward(); session.LeaveSecretEvent(); }
                            if (session.OutsideChallengeAvailable) session.DeclineOutsideStage();
                            session.PlaceHeroOrder(); Purchase(session, c, new Tally());
                            session.ContinueWave();
                        }
                        File.WriteAllLines(root + "/Diagnose_Waves.csv", rows);
                    }
            Debug.Log("BALANCE_DIAGNOSE_PASSED " + root + " rows=" + (rows.Count - 1));
        }
        private sealed class Tally { public int Wallet, SpentItems, SpentRecruits, SpentRerolls, Items, Recruits, Rerolls, HpPaid; }
        private static void Purchase(RunSession s, RunCatalog c, Tally t)
        {
            void BuyAll(IEnumerable<ShopOffer> offers)
            {
                foreach (ShopOffer o in offers.Where(o => !o.Sold).OrderByDescending(o => OfferValue(s, c, o) / Mathf.Max(1, o.Currency == ShopCurrency.Gold ? o.Price : o.Price * 3)).ToArray())
                {
                    if (OfferValue(s, c, o) <= 0 || !s.CanBuy(o)) continue;
                    int gold = s.State.Gold; float health = s.State.Health;
                    if (!s.Buy(o.Token)) continue;
                    if (o.Kind == OfferKind.Companion) { t.Recruits++; t.SpentRecruits += gold - s.State.Gold; } else { t.Items++; t.SpentItems += gold - s.State.Gold; }
                    if (o.Currency == ShopCurrency.Health) t.HpPaid += o.Price;
                }
            }
            BuyAll(s.State.SpecialOffers);
            s.State.SpecialShopVisited = true;
            for (int pass = 0; pass < 12; pass++)
            {
                BuyAll(s.State.Offers);
                int cheapest = s.State.Offers.Where(o => !o.Sold && OfferValue(s, c, o) > 0).Select(o => o.Price).DefaultIfEmpty(20).Min();
                int cost = s.RerollCost;
                if (s.State.Gold <= cost + cheapest || !s.Reroll()) break;
                t.Rerolls++; t.SpentRerolls += cost;
            }
        }
    }
}
