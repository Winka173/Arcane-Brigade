using System;
using UnityEngine;

namespace ArcaneBrigade
{
    public static class RunModifierRules
    {
        public static ModifierScope DefaultScope(ItemEffect effect)
        {
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
                case ModifierScope.Champion: return "Thủ lĩnh";
                case ModifierScope.Companions: return "Đồng minh";
                case ModifierScope.Run: return "Lượt chơi";
                default: return "Toàn đội";
            }
        }

        public static string RarityName(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Uncommon: return "ĐỘC LẠ";
                case ItemRarity.Rare: return "HIẾM";
                case ItemRarity.Mythic: return "HUYỀN THOẠI";
                default: return "PHỔ THÔNG";
            }
        }

        public static Color RarityColor(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Uncommon: return new Color(0.25f, 0.7f, 1f);
                case ItemRarity.Rare: return new Color(0.75f, 0.6f, 1f);
                case ItemRarity.Mythic: return new Color(1f, 0.76f, 0.35f);
                default: return new Color(0.82f, 0.85f, 0.92f);
            }
        }

        public static string DescribeCompanion(UnitDefinition unit, int tier)
        {
            string description = "";
            foreach (StatModifier modifier in unit.CompanionModifiers ?? new StatModifier[0])
                description += ScopeName(modifier.Scope) + " / " + ModifierName(modifier.Effect) + ": " + ValueText(modifier.Effect, modifier.Value * UnitDefinition.ModifierScale(tier)) + "\n";
            return description + "Đặc tính cấp 1/2/3 ×1/2/4; ghép ba người làm giảm số nguồn hỗ trợ.\n";
        }

        public static string Describe(ItemDefinition item, int stacks)
        {
            string description = RarityName(item.Rarity) + " · " + (item.Stacking == StackPolicy.Unique ? "Duy nhất" : "Cộng dồn") + "\n";
            foreach (StatModifier modifier in item.EffectiveModifiers)
                if (modifier.Value != 0) description += ScopeName(modifier.Scope) + " / " + ModifierName(modifier.Effect) + ": " + ValueText(modifier.Effect, modifier.Value) + " × " + item.EffectiveStacks(stacks) + " = " + ValueText(modifier.Effect, modifier.Value * item.EffectiveStacks(stacks)) + "\n";
            return description;
        }

        public static string ModifierName(ItemEffect effect)
        {
            switch (effect)
            {
                case ItemEffect.Damage: case ItemEffect.ChampionDamage: case ItemEffect.CompanionDamage: return "Sát thương";
                case ItemEffect.AttackSpeed: case ItemEffect.CompanionAttackSpeed: return "Tốc độ đánh";
                case ItemEffect.MoveSpeed: return "Tốc độ di chuyển";
                case ItemEffect.Crit: return "Tỷ lệ chí mạng";
                case ItemEffect.Projectiles: return "Số đạn";
                case ItemEffect.Health: return "Máu tối đa";
                case ItemEffect.Pickup: return "Bán kính nhặt đồ";
                case ItemEffect.Roam: return "Bán kính tuần tra";
                case ItemEffect.Interceptor: return "Đồng minh chặn đạn";
                case ItemEffect.Heal: return "Hồi máu mỗi giây";
                case ItemEffect.Interest: return "Giới hạn tiền lãi";
                case ItemEffect.Armor: return "Giáp";
                case ItemEffect.Discount: return "Giảm giá hàng bằng vàng";
                case ItemEffect.Luck: return "May mắn";
                case ItemEffect.TreasureDetection: return "Phát hiện kho báu";
                default: return K10StatRules.Name(effect);
            }
        }

        public static string ValueText(ItemEffect effect, float value)
        {
            bool percent = effect == ItemEffect.Damage || effect == ItemEffect.ChampionDamage || effect == ItemEffect.CompanionDamage || effect == ItemEffect.AttackSpeed || effect == ItemEffect.CompanionAttackSpeed || effect == ItemEffect.MoveSpeed || effect == ItemEffect.Crit || effect == ItemEffect.Discount;
            if (effect >= ItemEffect.ScoreMultiplier) percent = K10StatRules.Percentage(effect);
            return (percent ? value * 100 : value).ToString("+0.###;-0.###;0") + (percent ? "%" : "");
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
                if (recipe == null || string.IsNullOrEmpty(recipe.Id) || !ids.Add(recipe.Id) || !Array.Exists(catalog.Companions, unit => unit.Id == recipe.FirstUnit) || !Array.Exists(catalog.Companions, unit => unit.Id == recipe.SecondUnit) || !Array.Exists(catalog.Companions, unit => unit.Id == recipe.ResultUnit) || recipe.EggCost < 0 || recipe.MinimumMaterialTier < 1 || recipe.MinimumMaterialTier > catalog.MaximumTier || recipe.ResultTier < 1 || recipe.ResultTier > catalog.MaximumTier) throw new InvalidOperationException("Invalid recipe identity/tier/cost");
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
