using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Balance pass of 19/10/2026 (Docs/Balance_Pass_2026-10-19.md): item stat budgets follow rarity (BalanceAudit VU
    /// targets), flat max-HP gifts are proportionate, HP drawbacks and HP prices cost something real, and gold prices inside each
    /// rarity band follow the item's budget. Applied at the end of ContentExpansionBuilder.Apply (the last builder that writes item
    /// rarity/price), so re-running the builders re-applies it; re-runnable on its own (applying twice changes nothing).
    /// Bond values live in Tools/Content/expansion_lists.json (bondEffects) and Truce Bottle / Fish Cracker in
    /// ContentExpansionBuilder, their original sources.</summary>
    public static class BalancePass
    {
        private static (ItemEffect effect, float value, ModifierScope? scope) M(ItemEffect effect, float value, ModifierScope? scope = null) => (effect, value, scope);
        private const ModifierScope C = ModifierScope.Champion;

        /// <summary>New modifier lists. A scope left out keeps the item's previous scope for that effect (or the default).</summary>
        public static readonly Dictionary<string, (ItemEffect effect, float value, ModifierScope? scope)[]> Items = new Dictionary<string, (ItemEffect, float, ModifierScope?)[]>
        {
            // ---- Common (target 12 VU)
            ["coin-compass"] = new[] { M(ItemEffect.Pickup, 1.6f) },
            ["felt-boots"] = new[] { M(ItemEffect.MoveSpeed, .22f) },
            ["wandering-map"] = new[] { M(ItemEffect.Roam, 1f), M(ItemEffect.Pickup, .8f) },
            ["piggy-mail"] = new[] { M(ItemEffect.Interest, 4) },
            ["k10-income"] = new[] { M(ItemEffect.Income, 5), M(ItemEffect.Health, -5) },
            ["k10-bulletsize"] = new[] { M(ItemEffect.BulletSize, .8f), M(ItemEffect.Health, -2) },
            ["k10-enemyspeed"] = new[] { M(ItemEffect.EnemySpeed, .1f), M(ItemEffect.ScoreMultiplier, .2f), M(ItemEffect.Health, -5) },
            ["k10-relicdiscount"] = new[] { M(ItemEffect.RelicDiscount, .13f), M(ItemEffect.Health, -5) },
            ["clock-spring"] = new[] { M(ItemEffect.AttackSpeed, .12f) },
            ["pillow-mail"] = new[] { M(ItemEffect.Armor, .15f) },
            ["k10-bulletpierce"] = new[] { M(ItemEffect.BulletPierce, 2), M(ItemEffect.Health, -8) },
            ["k10-hearthealing"] = new[] { M(ItemEffect.HeartHealing, 8), M(ItemEffect.Health, -2) },
            ["honey-pot"] = new[] { M(ItemEffect.Health, 15) },
            ["oracle-lens"] = new[] { M(ItemEffect.Crit, .2f) },
            ["warm-tea"] = new[] { M(ItemEffect.Heal, .8f) },
            ["coupon-book"] = new[] { M(ItemEffect.Discount, .08f) },
            ["fortune-egg"] = new[] { M(ItemEffect.Luck, .3f) },
            ["k10-attackrange"] = new[] { M(ItemEffect.AttackRange, .3f), M(ItemEffect.Health, -5) },
            ["k10-enemycount"] = new[] { M(ItemEffect.EnemyCount, .1f), M(ItemEffect.ScoreMultiplier, .2f), M(ItemEffect.Health, -5) },
            ["k10-healcorrection"] = new[] { M(ItemEffect.HealCorrection, .6f) },
            ["velvet-first-aid"] = new[] { M(ItemEffect.TreasureDetection, .8f), M(ItemEffect.MoveSpeed, -.02f, C) },
            ["OctoberRelic0"] = new[] { M(ItemEffect.AttackRange, .3f), M(ItemEffect.AttackSpeed, -.04f) },
            ["k10-collisiondamage"] = new[] { M(ItemEffect.CollisionDamage, 12), M(ItemEffect.Health, -5) },
            ["k10-knockback"] = new[] { M(ItemEffect.Knockback, .5f), M(ItemEffect.CollisionDamage, 6) },
            ["k10-damagecorrection"] = new[] { M(ItemEffect.DamageCorrection, .14f), M(ItemEffect.Health, -5) },
            ["K26Defense0"] = new[] { M(ItemEffect.Health, 12), M(ItemEffect.Heal, .3f) },
            ["K26Defense1"] = new[] { M(ItemEffect.Armor, .2f), M(ItemEffect.MoveSpeed, -.06f) },
            ["OctoberRelic2"] = new[] { M(ItemEffect.CollisionDamage, 10), M(ItemEffect.MoveSpeed, -.03f) },
            ["CurioOrchardFizz"] = new[] { M(ItemEffect.Health, 18), M(ItemEffect.Income, -2) },
            ["CurioGraveChime"] = new[] { M(ItemEffect.CollisionDamage, 18, C), M(ItemEffect.MoveSpeed, -.04f, C) },
            ["CurioBreathJar"] = new[] { M(ItemEffect.Damage, .16f), M(ItemEffect.AttackSpeed, -.04f) },
            ["CurioCoralBroth"] = new[] { M(ItemEffect.Heal, 1), M(ItemEffect.Health, -6) },
            ["CurioRicochetSeltzer"] = new[] { M(ItemEffect.RangedDamage, 4), M(ItemEffect.BulletBounce, .2f), M(ItemEffect.Damage, -.05f) },
            ["CurioNoonSpark"] = new[] { M(ItemEffect.MeleeDamage, 3), M(ItemEffect.Crit, .08f) },
            ["CurioMeadowTonic"] = new[] { M(ItemEffect.Dodge, .1f), M(ItemEffect.Luck, .08f) },
            ["CurioBentTape"] = new[] { M(ItemEffect.AttackSpeed, .14f) },
            // ---- Uncommon (target 19 VU)
            ["k10-scoremultiplier"] = new[] { M(ItemEffect.ScoreMultiplier, .08f), M(ItemEffect.Health, -6) },
            ["k10-meleedamage"] = new[] { M(ItemEffect.MeleeDamage, 7), M(ItemEffect.Health, -6) },
            ["k10-rangeddamage"] = new[] { M(ItemEffect.RangedDamage, 7), M(ItemEffect.Health, -6) },
            ["k10-bulletsplit"] = new[] { M(ItemEffect.BulletSplit, 2), M(ItemEffect.Health, -8) },
            ["k10-bulletspeed"] = new[] { M(ItemEffect.BulletSpeed, .5f), M(ItemEffect.BulletDistance, .5f), M(ItemEffect.AttackRange, .2f) },
            ["k10-bulletdistance"] = new[] { M(ItemEffect.BulletDistance, .6f), M(ItemEffect.AttackRange, .14f) },
            ["k10-bodysize"] = new[] { M(ItemEffect.BodySize, .12f), M(ItemEffect.Health, 15), M(ItemEffect.CollisionDamage, 4, C) },
            ["k10-companionsize"] = new[] { M(ItemEffect.CompanionSize, .12f), M(ItemEffect.CompanionDamage, .25f) },
            ["k10-chestquality"] = new[] { M(ItemEffect.ChestQuality, .6f), M(ItemEffect.TreasureDetection, .5f) },
            ["k10-shoprarity"] = new[] { M(ItemEffect.ShopRarity, .32f) },
            ["k10-healthcorrection"] = new[] { M(ItemEffect.HealthCorrection, .2f) },
            ["k10-speedcorrection"] = new[] { M(ItemEffect.SpeedCorrection, .35f) },
            ["k10-armorcorrection"] = new[] { M(ItemEffect.ArmorCorrection, .4f), M(ItemEffect.Armor, .07f) },
            ["k10-dodge"] = new[] { M(ItemEffect.Dodge, .24f), M(ItemEffect.Health, -6) },
            ["k10-bulletbounce"] = new[] { M(ItemEffect.BulletBounce, .65f) },
            ["k10-enemyhealth"] = new[] { M(ItemEffect.EnemyHealth, .1f), M(ItemEffect.ScoreMultiplier, .2f), M(ItemEffect.Health, -6) },
            ["k10-recruitdiscount"] = new[] { M(ItemEffect.RecruitDiscount, .16f) },
            ["k10-collisionfrequency"] = new[] { M(ItemEffect.CollisionFrequency, .6f), M(ItemEffect.CollisionDamage, 4) },
            ["long-pocket"] = new[] { M(ItemEffect.ChestReach, .8f), M(ItemEffect.TreasureDetection, .6f), M(ItemEffect.Pickup, .6f) },
            ["marching-bell"] = new[] { M(ItemEffect.CompanionAttackSpeed, .3f) },
            ["royal-key"] = new[] { M(ItemEffect.ChampionDamage, .36f) },
            ["brigade-banner"] = new[] { M(ItemEffect.CompanionDamage, .32f) },
            ["SecretMirror"] = new[] { M(ItemEffect.Crit, .12f), M(ItemEffect.AttackRange, .2f) },
            ["SecretHour"] = new[] { M(ItemEffect.AttackSpeed, .1f), M(ItemEffect.Damage, .08f) },
            ["SecretDream"] = new[] { M(ItemEffect.Health, 16), M(ItemEffect.HeartHealing, 4) },
            ["SecretLetter"] = new[] { M(ItemEffect.Pickup, 1.2f), M(ItemEffect.TreasureDetection, .4f), M(ItemEffect.Luck, .1f) },
            ["CurioStormDryer"] = new[] { M(ItemEffect.Damage, .18f), M(ItemEffect.Knockback, .2f) },
            ["CurioShatteredPager"] = new[] { M(ItemEffect.Income, 7), M(ItemEffect.BulletSplit, 1, C) },
            ["CurioSunfireFlask"] = new[] { M(ItemEffect.AttackRange, .4f), M(ItemEffect.Projectiles, 1, C), M(ItemEffect.AttackSpeed, -.35f, C) },
            ["CurioEmberEspresso"] = new[] { M(ItemEffect.AttackSpeed, .3f, C), M(ItemEffect.CollisionDamage, -1, C) },
            ["CurioMoonMilk"] = new[] { M(ItemEffect.HeartHealing, 8), M(ItemEffect.Heal, .5f), M(ItemEffect.MoveSpeed, -.05f, C) },
            ["CurioPocketSun"] = new[] { M(ItemEffect.BodySize, -.1f), M(ItemEffect.Luck, .45f) },
            ["CurioMulberryRiot"] = new[] { M(ItemEffect.Income, 10), M(ItemEffect.TreasureDetection, .6f), M(ItemEffect.Armor, -.15f) },
            ["CurioSleepyMist"] = new[] { M(ItemEffect.Damage, .25f), M(ItemEffect.MoveSpeed, -.25f), M(ItemEffect.EnemySpeed, -.15f) },
            ["CurioDaylightBonnet"] = new[] { M(ItemEffect.Health, 24) },
            ["CurioNeedleTarget"] = new[] { M(ItemEffect.BulletPierce, 1), M(ItemEffect.Crit, .2f), M(ItemEffect.MeleeDamage, -2) },
            ["CurioCrookedEight"] = new[] { M(ItemEffect.Luck, .5f), M(ItemEffect.BulletBounce, .25f), M(ItemEffect.Damage, -.04f) },
            ["CurioRetortCup"] = new[] { M(ItemEffect.Armor, .25f), M(ItemEffect.Luck, -.08f) },
            ["CurioWatchfulTea"] = new[] { M(ItemEffect.Heal, 1.6f) },
            // ---- Rare (target 28 VU)
            ["treasure-whisper"] = new[] { M(ItemEffect.TreasureDetection, 1f), M(ItemEffect.Luck, .25f) },
            ["OctoberRelic1"] = new[] { M(ItemEffect.Heal, 1.8f), M(ItemEffect.Income, -2) },
            ["twin-seal"] = new[] { M(ItemEffect.Projectiles, 1), M(ItemEffect.AttackSpeed, -.1f) },
            ["long-atlas"] = new[] { M(ItemEffect.Roam, 2), M(ItemEffect.AttackRange, .32f), M(ItemEffect.MoveSpeed, -.04f) },
            ["altar-ember"] = new[] { M(ItemEffect.ChampionDamage, .5f), M(ItemEffect.MoveSpeed, -.04f) },
            ["OctoberRelic3"] = new[] { M(ItemEffect.HeartHealing, 5), M(ItemEffect.Health, 25) },
            ["guardian-canopy"] = new[] { M(ItemEffect.Interceptor, 1), M(ItemEffect.CompanionAttackSpeed, .15f) },
            ["K26Defense4"] = new[] { M(ItemEffect.HealthCorrection, .2f), M(ItemEffect.Heal, 1) },
            ["K26Defense5"] = new[] { M(ItemEffect.Heal, 2.2f), M(ItemEffect.Income, -2) },
            ["SecretThread"] = new[] { M(ItemEffect.AttackRange, .3f), M(ItemEffect.Knockback, .3f), M(ItemEffect.Damage, .08f) },
            ["SecretTicket"] = new[] { M(ItemEffect.Discount, .12f), M(ItemEffect.Luck, .15f) },
            ["CurioWildPigment"] = new[] { M(ItemEffect.MoveSpeed, .15f), M(ItemEffect.MeleeDamage, 9), M(ItemEffect.RangedDamage, -3) },
            ["CurioBitterHoney"] = new[] { M(ItemEffect.Heal, 2), M(ItemEffect.TreasureDetection, .6f), M(ItemEffect.EnemySpeed, .1f) },
            ["CurioPetalTonic"] = new[] { M(ItemEffect.Health, 55), M(ItemEffect.Heal, -1.2f) },
            ["CurioWakefulCordial"] = new[] { M(ItemEffect.CollisionDamage, 35, C), M(ItemEffect.Knockback, .3f, C), M(ItemEffect.Crit, -.1f, C) },
            ["CurioVictorDraught"] = new[] { M(ItemEffect.Crit, .3f), M(ItemEffect.BulletPierce, 1), M(ItemEffect.Damage, -.05f) },
            // ---- Mythic (target 40 VU; mechanic items keep their mechanic, only the stat part is trimmed)
            ["starlight-quilt"] = new[] { M(ItemEffect.Health, 50) },
            ["tarnished-crown"] = new[] { M(ItemEffect.ChampionDamage, .6f) },
            ["echo-postage"] = new[] { M(ItemEffect.Projectiles, 2), M(ItemEffect.AttackSpeed, -.3f) },
            ["K26Defense6"] = new[] { M(ItemEffect.Armor, .55f), M(ItemEffect.MoveSpeed, -.08f) },
            ["CurioOrbitPastry"] = new[] { M(ItemEffect.BodySize, .25f), M(ItemEffect.BulletSize, .75f, C), M(ItemEffect.BulletSpeed, -.4f, C), M(ItemEffect.ChampionDamage, .7f) },
            ["CurioBatteringHelm"] = new[] { M(ItemEffect.CollisionDamage, 70, C), M(ItemEffect.CollisionFrequency, .7f, C), M(ItemEffect.RangedDamage, -6) },
            ["CurioWishWick"] = new[] { M(ItemEffect.Health, -12), M(ItemEffect.Income, -5) },
            // round 2: Silent Alarm's bleed stacked (8 copies = 8 HP/s); one copy, stat part at the Mythic target (61 → 40 VU)
            ["CurioMuteClock"] = new[] { M(ItemEffect.AttackSpeed, .3f), M(ItemEffect.MoveSpeed, .25f) },
        };

        /// <summary>Gold price bands per rarity (Tools/Content/expansion_design.py PRICE).</summary>
        private static readonly (int lo, int hi)[] Band = { (15, 32), (24, 46), (30, 60), (48, 75), (48, 75), (200, 200) };
        /// <summary>Lowest altar HP price (before the altar's rarity factor 1 + 0.25 x rarity): Common 8 / Uncommon 13 / Rare 18 /
        /// Mythic 27 current HP, about 7–25% of a 110 HP hero. Was 3 / 4 for the Common and Uncommon HP offers.</summary>
        private static readonly int[] HealthAltar = { 8, 10, 12, 15, 15, 15 };
        private static readonly int[] EggAltar = { 2, 3, 3, 4, 4, 4 };

        /// <summary>Items that may be owned once (round 2).</summary>
        public static readonly string[] Unique = { "CurioMuteClock" };

        public static int Apply(RunCatalog catalog)
        {
            int changed = 0;
            foreach (string id in Unique)
            {
                ItemDefinition item = catalog.Item(id) ?? throw new InvalidOperationException("Balance pass: item missing " + id);
                if (item.Stacking != StackPolicy.Unique || item.MaximumStacks != 1) { item.Stacking = StackPolicy.Unique; item.MaximumStacks = 1; changed++; }
            }
            foreach (var entry in Items)
            {
                ItemDefinition item = catalog.Item(entry.Key) ?? throw new InvalidOperationException("Balance pass: item missing " + entry.Key);
                var mods = entry.Value.Select(m => new StatModifier { Effect = m.effect, Value = m.value,
                    Scope = m.scope ?? item.EffectiveModifiers.FirstOrDefault(o => o.Effect == m.effect)?.Scope ?? RunModifierRules.DefaultScope(m.effect) }).ToArray();
                if (Same(item.Modifiers, mods)) continue;
                item.Modifiers = mods; changed++;
            }
            foreach (ItemDefinition item in catalog.Items)
            {
                // an item that raises max HP is never sold for current HP (it paid for itself on the spot)
                bool givesHealth = item.EffectiveModifiers.Any(m => (m.Effect == ItemEffect.Health || m.Effect == ItemEffect.HealthCorrection) && m.Value > 0);
                if (item.AltarCurrency == ShopCurrency.Health && givesHealth) { item.AltarCurrency = ShopCurrency.Eggs; item.AltarPrice = EggAltar[(int)item.Rarity]; changed++; }
                if (item.AltarCurrency == ShopCurrency.Health && item.AltarPrice < HealthAltar[(int)item.Rarity]) { item.AltarPrice = HealthAltar[(int)item.Rarity]; changed++; }
                // stat-only items: the gold price inside the rarity band follows the budget (ratio 0.6 → bottom, 1.4 → top)
                if (!BalanceAudit.MechanicItem(catalog, item) && !BalanceAudit.ScoreItem(item) && item.Rarity < ItemRarity.Rainbow)
                {
                    var (lo, hi) = Band[(int)item.Rarity];
                    float ratio = BalanceAudit.Budget(item.EffectiveModifiers) / BalanceAudit.Target(item.Rarity);
                    int price = Mathf.RoundToInt(lo + (hi - lo) * Mathf.Clamp01((ratio - .6f) / .8f));
                    if (item.Price != price) { item.Price = price; changed++; }
                }
            }
            foreach (string id in Items.Keys) { ItemDefinition item = catalog.Item(id); item.Description = RelicDescriptions.Describe(item, 1, catalog).Replace("\r\n", "\n"); }
            return changed;
        }
        private static bool Same(StatModifier[] a, StatModifier[] b) => a != null && a.Length == b.Length && a.Zip(b, (x, y) => x.Effect == y.Effect && x.Scope == y.Scope && Mathf.Approximately(x.Value, y.Value)).All(v => v);

        [MenuItem("Arcane Brigade/Balance/Apply balance pass (19 Oct 2026)")]
        public static void Run()
        {
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            int changed = Apply(catalog);
            RunModifierRules.Validate(catalog);
            EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets();
            Debug.Log("BALANCE_PASS_PASSED changes=" + changed);
        }
    }
}
