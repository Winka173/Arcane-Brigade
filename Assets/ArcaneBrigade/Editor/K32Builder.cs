using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K32Builder
    {
        [MenuItem("Arcane Brigade/K3/Apply modifier economy slice and build")]
        public static void ApplyAndBuild() { Apply(); K32Checks.Run(); FullRunBuilder.BuildWindows(); }

        public static void Apply()
        {
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            if (catalog == null) throw new System.InvalidOperationException("Run catalog missing");
            if (catalog.EconomyRevision > 0) return;
            catalog.RerollPriceStep = 2;
            foreach (ItemDefinition item in catalog.Items)
            {
                item.MaximumStacks = 100000;
                item.Pools = ShopPool.Both;
                item.Stacking = StackPolicy.Additive;
                item.Rarity = item.UnlockWave >= 8 ? ItemRarity.Mythic : item.UnlockWave >= 4 ? ItemRarity.Rare : item.Price >= 26 ? ItemRarity.Uncommon : ItemRarity.Common;
                item.ShopWeight = item.Rarity == ItemRarity.Mythic ? 8 : item.Rarity == ItemRarity.Rare ? 25 : item.Rarity == ItemRarity.Uncommon ? 60 : 100;
                item.Modifiers = new[] { new StatModifier { Effect = item.Effect, Scope = RunModifierRules.DefaultScope(item.Effect), Value = item.Value } };
                if (item.Id == "coupon-book") item.Description = "Future gold offer prices -5% per stack (cap 50%). Does not reduce reroll fees.";
                if (item.Id == "long-atlas")
                {
                    item.Modifiers = new[] { item.Modifiers[0], new StatModifier { Effect = ItemEffect.MoveSpeed, Scope = ModifierScope.Team, Value = -0.04f } };
                    item.Description = "+1.0 ally leash radius; -4% team movement speed. Wider patrols, slower travel.";
                }
                if (item.Id == "echo-postage")
                {
                    item.Modifiers = new[] { item.Modifiers[0], new StatModifier { Effect = ItemEffect.AttackSpeed, Scope = ModifierScope.Team, Value = -0.08f } };
                    item.Description = "+2 projectiles per ranged attack; -8% team attack speed. More shots, longer attack cycle.";
                }
            }
            catalog.BuildLinks = new[]
            {
                new BuildLinkRule { Id = "engine-clockwork", Name = "BỘ MÁY LÊN DÂY", ItemIds = new[] { "key-oil", "clock-spring" }, Description = "Damage và attack speed từ hai nguồn độc lập; xem stat sau scope/tier, không phải bonus set." },
                new BuildLinkRule { Id = "engine-economy", Name = "QUỸ BƯU ĐIỆN", ItemIds = new[] { "piggy-mail", "coupon-book" }, Description = "Giữ vàng để lấy lãi, giảm giá offer ở roll tiếp theo. Coupon không giảm reroll." },
                new BuildLinkRule { Id = "engine-patrol", Name = "ĐOÀN THÁM HIỂM", ItemIds = new[] { "coin-compass", "wandering-map" }, Description = "Pickup thuộc Champion; leash là vùng tuần tra đồng minh. Hai bán kính không thay thế nhau." }
            };
            catalog.EconomyRevision = 1;
            RunModifierRules.Validate(catalog);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log("ARCANE_K32_AUTHORING_APPLIED");
        }
    }
}
