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
                case ItemEffect.Health: case ItemEffect.Heal: case ItemEffect.Armor: case ItemEffect.ChampionDamage: return ModifierScope.Champion;
                case ItemEffect.CompanionDamage: case ItemEffect.CompanionAttackSpeed: return ModifierScope.Companions;
                case ItemEffect.Pickup: case ItemEffect.Roam: case ItemEffect.Interceptor: case ItemEffect.Interest: case ItemEffect.Discount: return ModifierScope.Run;
                default: return ModifierScope.Team;
            }
        }

        public static bool Applies(ModifierScope source, ModifierScope target) => source == target || (source == ModifierScope.Team && (target == ModifierScope.Champion || target == ModifierScope.Companions));

        public static string ScopeName(ModifierScope scope)
        {
            switch (scope)
            {
                case ModifierScope.Champion: return "Champion";
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
                case ItemRarity.Uncommon: return new Color(0.45f, 0.95f, 0.82f);
                case ItemRarity.Rare: return new Color(0.75f, 0.6f, 1f);
                case ItemRarity.Mythic: return new Color(1f, 0.76f, 0.35f);
                default: return new Color(0.82f, 0.85f, 0.92f);
            }
        }

        public static string Describe(ItemDefinition item, int stacks)
        {
            string description = RarityName(item.Rarity) + " · " + (item.Stacking == StackPolicy.Unique ? "Duy nhất" : "Cộng dồn") + "\n";
            foreach (StatModifier modifier in item.EffectiveModifiers)
                description += ScopeName(modifier.Scope) + " / " + ModifierName(modifier.Effect) + ": " + ValueText(modifier.Effect, modifier.Value) + " × " + item.EffectiveStacks(stacks) + " = " + ValueText(modifier.Effect, modifier.Value * item.EffectiveStacks(stacks)) + "\n";
            return description;
        }

        public static string ModifierName(ItemEffect effect)
        {
            switch (effect)
            {
                case ItemEffect.Damage: case ItemEffect.ChampionDamage: case ItemEffect.CompanionDamage: return "Damage";
                case ItemEffect.AttackSpeed: case ItemEffect.CompanionAttackSpeed: return "Attack speed";
                case ItemEffect.MoveSpeed: return "Move speed";
                case ItemEffect.Crit: return "Crit";
                case ItemEffect.Projectiles: return "Projectile";
                default: return RunBuildReadModel.EffectName(effect);
            }
        }

        private static string ValueText(ItemEffect effect, float value)
        {
            bool percent = effect == ItemEffect.Damage || effect == ItemEffect.ChampionDamage || effect == ItemEffect.CompanionDamage || effect == ItemEffect.AttackSpeed || effect == ItemEffect.CompanionAttackSpeed || effect == ItemEffect.MoveSpeed || effect == ItemEffect.Crit || effect == ItemEffect.Discount;
            return (percent ? value * 100 : value).ToString("+0.###;-0.###;0") + (percent ? "%" : "");
        }

        public static void Validate(RunCatalog catalog)
        {
            if (catalog.RerollPrice < 0 || catalog.RerollPriceStep < 0 || catalog.EconomyRevision < 0) throw new InvalidOperationException("Invalid economy tuning");
            if (catalog.AltarRevision < 0 || !float.IsFinite(catalog.AltarBaseChance) || catalog.AltarBaseChance < 0 || catalog.AltarBaseChance > 1 || !float.IsFinite(catalog.AltarChancePerEgg) || catalog.AltarChancePerEgg < 0) throw new InvalidOperationException("Invalid altar tuning");
            foreach (ItemDefinition item in catalog.Items)
            {
                if (!Enum.IsDefined(typeof(ItemRarity), item.Rarity) || !Enum.IsDefined(typeof(StackPolicy), item.Stacking) || item.ShopWeight < 0 || item.MaximumStacks < 0 || item.MaximumStacks > 100000 || (catalog.EconomyRevision > 0 && (item.Pools == 0 || (item.Pools & ~ShopPool.Both) != 0))) throw new InvalidOperationException("Invalid item policy: " + item.Id);
                if (catalog.AltarRevision > 0 && (item.AltarPrice < 1 || (item.AltarCurrency != ShopCurrency.Eggs && item.AltarCurrency != ShopCurrency.Health))) throw new InvalidOperationException("Invalid altar price/currency: " + item.Id);
                foreach (StatModifier modifier in item.EffectiveModifiers)
                {
                    if (modifier == null || !Enum.IsDefined(typeof(ItemEffect), modifier.Effect) || !Enum.IsDefined(typeof(ModifierScope), modifier.Scope) || !float.IsFinite(modifier.Value)) throw new InvalidOperationException("Invalid modifier: " + item.Id);
                    ModifierScope expected = DefaultScope(modifier.Effect);
                    if (expected != ModifierScope.Team && modifier.Scope != expected || expected == ModifierScope.Team && modifier.Scope == ModifierScope.Run) throw new InvalidOperationException("Unsupported modifier scope: " + item.Id);
                }
            }
            var ids = new System.Collections.Generic.HashSet<string>();
            foreach (BuildLinkRule rule in catalog.BuildLinks ?? new BuildLinkRule[0])
            {
                if (rule == null || string.IsNullOrEmpty(rule.Id) || !ids.Add(rule.Id) || string.IsNullOrEmpty(rule.Name) || rule.ItemIds == null || rule.ItemIds.Length < 2) throw new InvalidOperationException("Invalid build link");
                var sources = new System.Collections.Generic.HashSet<string>();
                foreach (string id in rule.ItemIds) if (catalog.Item(id) == null || !sources.Add(id)) throw new InvalidOperationException("Invalid build link source");
            }
        }
    }
}
