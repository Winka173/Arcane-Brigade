using System;
using UnityEngine;

namespace ArcaneBrigade
{
    public static class RunModifierRules
    {
        public static ModifierScope DefaultScope(ItemEffect effect)
        {
            if (effect == ItemEffect.ChestReach) return ModifierScope.Run;
            switch (effect)
            {
                case ItemEffect.Health: case ItemEffect.Heal: case ItemEffect.Armor: case ItemEffect.ChampionDamage: case ItemEffect.Dodge: case ItemEffect.BodySize: case ItemEffect.HeartHealing: case ItemEffect.HealthCorrection: case ItemEffect.ArmorCorrection: case ItemEffect.HealCorrection: return ModifierScope.Champion;
                case ItemEffect.CompanionDamage: case ItemEffect.CompanionAttackSpeed: case ItemEffect.CompanionSize: return ModifierScope.Companions;
                case ItemEffect.Pickup: case ItemEffect.Roam: case ItemEffect.Interceptor: case ItemEffect.Interest: case ItemEffect.Discount: case ItemEffect.Luck: case ItemEffect.TreasureDetection: case ItemEffect.ScoreMultiplier: case ItemEffect.Income: case ItemEffect.EnemyCount: case ItemEffect.EnemyHealth: case ItemEffect.EnemySpeed: case ItemEffect.ChestQuality: case ItemEffect.RecruitDiscount: case ItemEffect.RelicDiscount: case ItemEffect.ShopRarity: return ModifierScope.Run;
                default: return ModifierScope.Team;
            }
        }

        public static bool Applies(ModifierScope source, ModifierScope target) => source == target || (source == ModifierScope.Team && (target == ModifierScope.Champion || target == ModifierScope.Companions));

        public static string ScopeName(ModifierScope scope)
        {
            switch (scope)
            {
                case ModifierScope.Champion: return "You";
                case ModifierScope.Companions: return "Friends";
                case ModifierScope.Run: return "Run";
                default: return "Everyone";
            }
        }

        public static string RarityName(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Uncommon: return "UNCOMMON";
                case ItemRarity.Rare: return "RARE";
                case ItemRarity.Mythic: return "MYTHIC";
                case ItemRarity.Crimson: return "CRIMSON";
                case ItemRarity.Rainbow: return "PRISMATIC";
                default: return "COMMON";
            }
        }

        public static Color RarityColor(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Uncommon: return new Color(0.25f, 0.7f, 1f);
                case ItemRarity.Rare: return new Color(0.75f, 0.6f, 1f);
                case ItemRarity.Mythic: return new Color(1f, 0.76f, 0.35f);
                case ItemRarity.Crimson: return new Color(1f, 0.16f, 0.27f);
                case ItemRarity.Rainbow: return new Color(0.58f, 1f, 0.92f);
                default: return new Color(0.82f, 0.85f, 0.92f);
            }
        }

        public static string DescribeCompanion(UnitDefinition unit, int tier)
        {
            string description = "";
            foreach (StatModifier modifier in unit.CompanionModifiers ?? new StatModifier[0])
                description += Line(modifier.Effect, modifier.Scope, modifier.Value * UnitDefinition.ModifierScale(tier)) + "\n";
            return description + "Bonuses grow ×1/2/4 at 1/2/3 stars.\n";
        }

        public static string Describe(ItemDefinition item, int stacks)
            => RelicDescriptions.Describe(item, stacks);
        private static string LegacyDescribe(ItemDefinition item, int stacks)
        {
            if (!string.IsNullOrEmpty(item.ReferenceName)) return item.Description + "\nOwned: " + item.EffectiveStacks(stacks);
            string description = RarityName(item.Rarity) + " · " + (item.Stacking == StackPolicy.Unique ? "Unique" : "Stackable") + "\n";
            foreach (StatModifier modifier in item.EffectiveModifiers)
                if (modifier.Value != 0) description += WhoFor(modifier.Effect, modifier.Scope) + ModifierName(modifier.Effect) + ": " + ValueText(modifier.Effect, modifier.Value) + " × " + item.EffectiveStacks(stacks) + " = " + ValueText(modifier.Effect, modifier.Value * item.EffectiveStacks(stacks)) + "\n";
            return description;
        }

        public static string ModifierName(ItemEffect effect)
        {
            switch (effect)
            {
                case ItemEffect.Damage: case ItemEffect.ChampionDamage: case ItemEffect.CompanionDamage: return "Damage";
                case ItemEffect.AttackSpeed: case ItemEffect.CompanionAttackSpeed: return "Attack speed";
                case ItemEffect.MoveSpeed: return "Move speed";
                case ItemEffect.ChainChance: return "Chain chance";
                case ItemEffect.RecordedDamage: return "Damage vs known kinds";
                case ItemEffect.Crit: return "Crit chance";
                case ItemEffect.Projectiles: return "Extra shots";
                case ItemEffect.Health: return "Max HP";
                case ItemEffect.Pickup: return "Pickup range";
                case ItemEffect.Roam: return "Friend roam range";
                case ItemEffect.Interceptor: return "Friends block shots";
                case ItemEffect.Heal: return "HP regen per second";
                case ItemEffect.Interest: return "Max interest";
                case ItemEffect.Armor: return "Armor";
                case ItemEffect.Discount: return "Shop discount";
                case ItemEffect.Luck: return "Luck";
                case ItemEffect.TreasureDetection: return "Chest finding";
                default: return K10StatRules.Name(effect);
            }
        }

        /// <summary>Armour in whole points (requirement 25/10/2026: "+0.2" read as nothing): one point is 0.1 of the stored value, and
        /// a hit is multiplied by 8 / (8 + points), so 3 points take off 27%, 8 points half. Never less than one point shown.</summary>
        public const float ArmorPoint = .1f, ArmorStrength = 1.25f;
        public static int ArmorPoints(float value) => value == 0 ? 0 : (int)Mathf.Sign(value) * Mathf.Max(1, Mathf.RoundToInt(Mathf.Abs(value) / ArmorPoint));
        public static string ArmorText(float value) => (value >= 0 ? "+" : "−") + Mathf.Abs(ArmorPoints(value));
        public static string ValueText(ItemEffect effect, float value)
        {
            if (effect == ItemEffect.Armor) return ArmorText(value);
            bool percent = effect == ItemEffect.Damage || effect == ItemEffect.ChampionDamage || effect == ItemEffect.CompanionDamage || effect == ItemEffect.AttackSpeed || effect == ItemEffect.CompanionAttackSpeed || effect == ItemEffect.MoveSpeed || effect == ItemEffect.Crit || effect == ItemEffect.Discount;
            if (effect >= ItemEffect.ScoreMultiplier) percent = K10StatRules.Percentage(effect);
            // the "final" multipliers read as a factor: Final max HP x1.1
            if (Final(effect)) return "×" + (1 + value).ToString("0.##");
            return (percent ? value * 100 : value).ToString("+0.##;-0.##;0") + (percent ? "%" : "");
        }
        public static bool Final(ItemEffect effect) => effect == ItemEffect.HealthCorrection || effect == ItemEffect.DamageCorrection || effect == ItemEffect.SpeedCorrection || effect == ItemEffect.ArmorCorrection || effect == ItemEffect.HealCorrection;
        /// <summary>Who a stat line is for, in player words: "You · ", "Friends · ", or nothing when it is for everyone or the run.</summary>
        public static string Who(ModifierScope scope) => scope == ModifierScope.Champion ? "You · " : scope == ModifierScope.Companions ? "Friends · " : "";
        /// <summary>Who a card's stat line is for, always said (requirement 22/10/2026: never a bare stat): "Hero · ", "Friends · ",
        /// "Everyone · ", "Run · " for the run's economy; enemy stats already name their target.</summary>
        public static string WhoFor(ItemEffect effect, ModifierScope scope)
        {
            if (effect == ItemEffect.EnemyCount || effect == ItemEffect.EnemyHealth || effect == ItemEffect.EnemySpeed) return "";
            return scope == ModifierScope.Champion ? "Hero · " : scope == ModifierScope.Companions ? "Friends · " : scope == ModifierScope.Run ? "Run · " : "Everyone · ";
        }
        /// <summary>One stat line as the player reads it: who, the stat's name, its value (e.g. "You · Max HP -5").</summary>
        // a move-speed bonus is shown as it counts (CarnivalBalance.MoveBonus)
        public static string Line(ItemEffect effect, ModifierScope scope, float value) => WhoFor(effect, scope) + ModifierName(effect) + " " + ValueText(effect, effect == ItemEffect.MoveSpeed ? CarnivalBalance.MoveBonus(value) : value);
        /// <summary>Whether a stat change helps the player (enemy stats are the other way round).</summary>
        public static bool Helps(ItemEffect effect, float value)
        {
            bool enemy = effect == ItemEffect.EnemyCount || effect == ItemEffect.EnemyHealth || effect == ItemEffect.EnemySpeed;
            return value > 0 != enemy;
        }

        public static void Validate(RunCatalog catalog)
        {
            if (catalog.ContentRevision < 0) throw new InvalidOperationException("Invalid content revision");
            foreach (UnitDefinition champion in catalog.Champions)
                foreach (StatModifier modifier in champion.ChampionModifiers ?? new StatModifier[0]) ValidateModifier(modifier, champion.Id);
            foreach (UnitDefinition companion in catalog.Companions)
                foreach (StatModifier modifier in companion.CompanionModifiers ?? new StatModifier[0]) ValidateModifier(modifier, companion.Id);
            foreach (BondDefinition bond in catalog.Bonds)
            {
                if (bond == null || bond.Threshold < 1 || !float.IsFinite(bond.Value) || !Enum.IsDefined(typeof(ItemEffect), bond.Effect)) throw new InvalidOperationException("Invalid bond");
                int previous = bond.Threshold;
                foreach (BondThresholdBonus bonus in bond.ExtraBonuses ?? new BondThresholdBonus[0])
                {
                    if (bonus == null || bonus.Threshold <= previous || !float.IsFinite(bonus.Value)) throw new InvalidOperationException("Invalid bond milestone");
                    previous = bonus.Threshold;
                }
            }
            if (catalog.RerollPrice < 0 || catalog.RerollPriceStep < 0 || catalog.EconomyRevision < 0) throw new InvalidOperationException("Invalid economy tuning");
            if (catalog.AltarRevision < 0 || !float.IsFinite(catalog.AltarBaseChance) || catalog.AltarBaseChance < 0 || catalog.AltarBaseChance > 1 || !float.IsFinite(catalog.AltarChancePerEgg) || catalog.AltarChancePerEgg < 0) throw new InvalidOperationException("Invalid altar tuning");
            foreach (ItemDefinition item in catalog.Items)
            {
                if (!Enum.IsDefined(typeof(SignatureRelicKind), item.SignatureRole)) throw new InvalidOperationException("Invalid signature relic role: " + item.Id);
                if (!Enum.IsDefined(typeof(ItemRarity), item.Rarity) || !Enum.IsDefined(typeof(StackPolicy), item.Stacking) || item.ShopWeight < 0 || item.MaximumStacks < 0 || item.MaximumStacks > 100000 || (catalog.EconomyRevision > 0 && (item.Pools == 0 || (item.Pools & ~ShopPool.Both) != 0))) throw new InvalidOperationException("Invalid item policy: " + item.Id);
                if (catalog.AltarRevision > 0 && (item.AltarPrice < 1 || (item.AltarCurrency != ShopCurrency.Eggs && item.AltarCurrency != ShopCurrency.Health))) throw new InvalidOperationException("Invalid altar price/currency: " + item.Id);
                foreach (StatModifier modifier in item.EffectiveModifiers)
                {
                    ValidateModifier(modifier, item.Id);
                }
            }
            var ids = new System.Collections.Generic.HashSet<string>();
            foreach (ProcRule rule in catalog.ProcRules ?? new ProcRule[0])
            {
                if (rule == null || string.IsNullOrEmpty(rule.Id) || !ids.Add(rule.Id) || catalog.Item(rule.ItemId) == null || !Enum.IsDefined(typeof(ProcTrigger), rule.Trigger) || !Enum.IsDefined(typeof(ProcAction), rule.Action) || !float.IsFinite(rule.Power) || rule.Power <= 0 || !float.IsFinite(rule.Radius) || rule.Radius < 0 || !float.IsFinite(rule.Cooldown) || rule.Cooldown < 0 || !float.IsFinite(rule.Chance) || rule.Chance < 0 || rule.Chance > 1) throw new InvalidOperationException("Invalid proc authoring");
            }
            ids.Clear();
            foreach (AltarRecipe recipe in catalog.Recipes)
                if (recipe == null || string.IsNullOrEmpty(recipe.Id) || !ids.Add(recipe.Id) || !Array.Exists(catalog.Companions, unit => unit.Id == recipe.FirstUnit) || !Array.Exists(catalog.Companions, unit => unit.Id == recipe.SecondUnit) || !Array.Exists(catalog.Companions, unit => unit.Id == recipe.ResultUnit) || !string.IsNullOrEmpty(recipe.ThirdUnit) && !Array.Exists(catalog.Companions, unit => unit.Id == recipe.ThirdUnit) || recipe.EggCost < 0 || recipe.MinimumMaterialTier < 1 || recipe.MinimumMaterialTier > catalog.MaximumTier || recipe.ResultTier < 1 || recipe.ResultTier > catalog.MaximumTier) throw new InvalidOperationException("Invalid recipe identity/tier/cost");
            ids.Clear();
            foreach (BuildLinkRule rule in catalog.BuildLinks ?? new BuildLinkRule[0])
            {
                if (rule == null || string.IsNullOrEmpty(rule.Id) || !ids.Add(rule.Id) || string.IsNullOrEmpty(rule.Name) || rule.ItemIds == null || rule.ItemIds.Length < 2) throw new InvalidOperationException("Invalid build link");
                var sources = new System.Collections.Generic.HashSet<string>();
                foreach (string id in rule.ItemIds) if (catalog.Item(id) == null || !sources.Add(id)) throw new InvalidOperationException("Invalid build link source");
            }
        }

        private static void ValidateModifier(StatModifier modifier, string id)
        {
            if (modifier == null || !Enum.IsDefined(typeof(ItemEffect), modifier.Effect) || !Enum.IsDefined(typeof(ModifierScope), modifier.Scope) || !float.IsFinite(modifier.Value)) throw new InvalidOperationException("Invalid modifier: " + id);
            ModifierScope expected = DefaultScope(modifier.Effect);
            if (expected != ModifierScope.Team && modifier.Scope != expected || expected == ModifierScope.Team && modifier.Scope == ModifierScope.Run) throw new InvalidOperationException("Unsupported modifier scope: " + id);
        }
    }
}
