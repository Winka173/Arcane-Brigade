using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class RunBuildReadModel
    {
        public sealed class Row { public string Name; public string Summary; public string Explanation; public Dictionary<string, string> CombatValues; public Sprite Icon; }
        public readonly List<Row> Stats = new List<Row>();
        public readonly List<Row> Companions = new List<Row>();
        public readonly List<Row> Relics = new List<Row>();
        public readonly List<Row> Links = new List<Row>();
        public string Bonds;
        public string Receipts;
        public string BuildLinks;

        public static RunBuildReadModel Capture(RunSession session, RunCatalog catalog, float health)
        {
            var model = new RunBuildReadModel();
            UnitDefinition champion = catalog.Unit(session.State.ChampionId);
            WeaponDefinition weapon = champion.Weapon;
            model.Stat("Máu Champion", health.ToString("0.0") + " / " + Mathf.Max(1f, champion.Health + session.Bonus(ItemEffect.Health)).ToString("0.0"), "Máu tối đa = max(1, gốc + nâng cấp trong lượt). Không có stat vĩnh viễn.");
            model.Stat("Sát thương Champion", (weapon.Damage * session.DamageMultiplier(champion, 1, false)).ToString("0.0"), "Gốc " + weapon.Damage.ToString("0.0") + "; gồm bonus damage và passive Rally nếu có. Chưa tính crit, giáp mục tiêu hoặc reaction.");
            model.Stat("Chu kỳ đánh Champion", (weapon.Cooldown / Mathf.Max(0.1f, 1f + session.Bonus(ItemEffect.AttackSpeed, ModifierScope.Champion))).ToString("0.00") + " s", "Cooldown gốc " + weapon.Cooldown.ToString("0.00") + " s; tất cả đòn tự động.");
            model.Stat("Tốc độ di chuyển", (champion.Speed * Mathf.Max(0.1f, 1f + session.Bonus(ItemEffect.MoveSpeed, ModifierScope.Champion))).ToString("0.00"), "Gốc " + champion.Speed.ToString("0.00") + "; bonus " + Percent(session.Bonus(ItemEffect.MoveSpeed, ModifierScope.Champion)) + "; hệ số tối thiểu 0.1.");
            model.Stat("Crit Champion", Percent(Mathf.Clamp01(champion.CriticalChance + session.Bonus(ItemEffect.Crit, ModifierScope.Champion))), "Gốc " + Percent(champion.CriticalChance) + "; bonus cộng trước khi clamp 0–100%.");
            model.Stat("Số projectile Champion", (weapon.ProjectileCount + Mathf.FloorToInt(session.Bonus(ItemEffect.Projectiles, ModifierScope.Champion))).ToString(), "Áp dụng khi weapon dùng projectile; không biến đòn melee thành đạn.");
            model.Stat("Giáp Champion", session.Bonus(ItemEffect.Armor).ToString("0.00"), "Policy hiện tại: damage nhận / (1 + max(0, giáp)). Đây là tuning prototype, không phải công thức reference đã xác nhận.");
            model.Stat("Hồi máu thụ động", (session.Bonus(ItemEffect.Heal) + (session.ActiveModifier == null ? 0f : session.ActiveModifier.Healing)).ToString("0.00") + " / s", "Regen từ item/wave. Passive HealChampion của từng đơn vị hồi riêng mỗi 4 giây, không gộp thành regen giả.");
            model.Stat("Bán kính nhặt loot", (catalog.Arena.PickupRadius + session.Bonus(ItemEffect.Pickup)).ToString("0.00"), "Gốc " + catalog.Arena.PickupRadius.ToString("0.00") + "; đơn vị world, không phải pixel màn hình.");
            model.Stat("Leash đồng minh", (catalog.Arena.CompanionLeash + session.Bonus(ItemEffect.Roam)).ToString("0.00"), "Đồng minh tự đi quanh người chơi; không điều khiển formation.");
            model.Stat("Đồng minh chặn đạn", session.Bonus(ItemEffect.Interceptor) > 0 ? "Đã bật bởi item" : "Không", "Đồng minh bất tử nhưng không chặn đạn mặc định.");
            model.Stat("Lãi shop kế tiếp", Mathf.Min(catalog.InterestCap + Mathf.FloorToInt(session.Bonus(ItemEffect.Interest)), session.State.Gold / Mathf.Max(1, catalog.InterestDivisor)).ToString() + " vàng", "Dự báo từ vàng đã lưu; không tính thêm loot chưa settle hoặc income hall kế tiếp.");
            model.Stat("Reroll kế tiếp", session.RerollCost + " vàng", "Đã đổi " + session.State.RerollsThisShop + " lần tại shop này. Giá gốc " + catalog.RerollPrice + "; mỗi lần thêm " + catalog.RerollPriceStep + ". Tuning riêng, không phải công thức reference đã xác nhận. Coupon giảm giá offer tương lai, không giảm phí reroll.");
            if (catalog.AltarRevision > 0) model.Stat("Altar tại rest mới", Percent(session.AltarActivationChance), "Dự báo từ trứng đang giữ: clamp(base + trứng × chance mỗi trứng). Roll một lần khi tạo rest mới; không reroll khi load. Trứng không bị tiêu để kích hoạt. Tuning riêng, chưa xác minh công thức reference.");
            foreach (RosterEntry entry in session.State.Roster)
            {
                UnitDefinition unit = catalog.Unit(entry.UnitId);
                WeaponDefinition actual = catalog.Weapon(entry.WeaponId);
                float damage = actual.Damage * session.DamageMultiplier(unit, entry.Tier, true);
                float cooldown = actual.Cooldown / Mathf.Max(0.1f, 1f + session.Bonus(ItemEffect.AttackSpeed, ModifierScope.Companions) + session.Bonus(ItemEffect.CompanionAttackSpeed));
                float passive = (unit.Passive == PassiveKind.Scavenge ? Mathf.Round(unit.PassiveStrength) : unit.PassiveStrength) * Mathf.Pow(catalog.TierSupportMultiplier, entry.Tier - 1);
                model.Companions.Add(new Row { Name = unit.DisplayName, Icon = unit.Body, Summary = "Cấp " + entry.Tier + " · " + actual.Id + "\n" + actual.Kind + " / " + actual.Element,
                    CombatValues = new Dictionary<string, string> { { "Damage", damage.ToString("0.###") }, { "Chu kỳ", cooldown.ToString("0.###") + " s" }, { "Crit", Percent(Mathf.Clamp01(unit.CriticalChance + session.Bonus(ItemEffect.Crit, ModifierScope.Companions))) }, { "Speed", (unit.Speed * Mathf.Max(0.1f, 1f + session.Bonus(ItemEffect.MoveSpeed, ModifierScope.Companions))).ToString("0.###") }, { "Projectile", (actual.ProjectileCount + Mathf.FloorToInt(session.Bonus(ItemEffect.Projectiles, ModifierScope.Companions))).ToString() }, { "Passive", passive.ToString("0.###") } },
                    Explanation = unit.Description + "\n\nWeapon đã roll: " + actual.Id + "\nDamage: " + damage.ToString("0.0") + " / chu kỳ " + cooldown.ToString("0.00") + " s\nRange: " + actual.Range.ToString("0.0") + "\nPassive: " + unit.Passive + " / strength " + passive.ToString("0.0") + "\nBond: " + string.Join(" / ", unit.Tags) + "\nBất tử. Một body tính một đơn vị bond, không nhân theo tier." });
            }
            foreach (ItemStack stack in session.State.Items)
            {
                ItemDefinition item = catalog.Item(stack.Id);
                model.Relics.Add(new Row { Name = item.Name, Icon = item.Icon, Summary = "Sở hữu ×" + stack.Count + " · " + RunModifierRules.RarityName(item.Rarity) + "\n" + (item.EffectiveModifiers.Length > 1 ? item.EffectiveModifiers.Length + " hiệu ứng · xem cả lợi/hại" : RunModifierRules.ScopeName(item.EffectiveModifiers[0].Scope)), Explanation = item.Description + "\n\nStack sở hữu: " + stack.Count + " / hiệu lực: " + item.EffectiveStacks(stack.Count) + "\n" + RunModifierRules.Describe(item, stack.Count) + "\nNâng cấp trong lượt. Không bán, đổi hoặc tháo đồ tại đây." });
            }
            foreach (BuildLinkRule rule in catalog.BuildLinks ?? new BuildLinkRule[0])
            {
                int owned = 0;
                string sources = "";
                string explanation = rule.Description + "\n\nNGUỒN THỰC TRONG LƯỢT\n\n";
                foreach (string id in rule.ItemIds)
                {
                    int count = session.StackCount(id);
                    if (count > 0) owned++;
                    sources += catalog.Item(id).Name + " ×" + count + "\n";
                    explanation += catalog.Item(id).Name + " ×" + count + "\n" + (count > 0 ? RunModifierRules.Describe(catalog.Item(id), count) : "Chưa sở hữu.\n") + "\n";
                }
                model.BuildLinks += rule.Name + " · " + owned + "/" + rule.ItemIds.Length + "\n" + rule.Description + "\n" + sources + "Liên kết đọc từ item đang sở hữu; không cộng stat ẩn.\n\n";
                model.Links.Add(new Row { Name = rule.Name, Icon = catalog.Item(rule.ItemIds[0]).Icon, Summary = (owned == rule.ItemIds.Length ? "ĐỦ NGUỒN" : "ĐANG GHÉP") + " · " + owned + "/" + rule.ItemIds.Length + "\nKhông phải bonus set", Explanation = explanation + "Liên kết là cách đọc nguồn stat; không cộng bonus ẩn. Stack hiển thị trước clamp combat." });
            }
            foreach (BondDefinition bond in catalog.Bonds)
            {
                int count = session.BondCounts.TryGetValue(bond.Id, out int value) ? value : 0;
                int activations = count / Mathf.Max(1, bond.Threshold);
                model.Bonds += bond.Name + "  " + count + "/" + bond.Threshold + "  · " + (activations > 0 ? "ACTIVE ×" + activations : "chưa đủ") + "\n" + EffectName(bond.Effect) + ": " + (bond.Value * activations).ToString("0.###") + "\n\n";
            }
            for (int index = session.State.Receipts.Count - 1; index >= 0; index--)
            {
                RunTransactionReceipt receipt = session.State.Receipts[index];
                model.Receipts += receipt.Title + "\n" + receipt.Description + "\n\n";
            }
            return model;
        }

        private void Stat(string name, string value, string explanation) { Stats.Add(new Row { Name = name, Summary = value, Explanation = explanation }); }
        private static string Percent(float value) => (value * 100f).ToString("0.#") + "%";
        public static string EffectName(ItemEffect effect)
        {
            switch (effect)
            {
                case ItemEffect.Damage: return "Damage toàn đội";
                case ItemEffect.ChampionDamage: return "Damage Champion";
                case ItemEffect.CompanionDamage: return "Damage đồng minh";
                case ItemEffect.AttackSpeed: return "Attack speed toàn đội";
                case ItemEffect.CompanionAttackSpeed: return "Attack speed đồng minh";
                case ItemEffect.Health: return "Máu tối đa Champion";
                case ItemEffect.MoveSpeed: return "Move speed toàn đội";
                case ItemEffect.Pickup: return "Bán kính nhặt loot";
                case ItemEffect.Roam: return "Leash đồng minh";
                case ItemEffect.Interceptor: return "Đồng minh chặn đạn";
                case ItemEffect.Heal: return "Regen Champion";
                case ItemEffect.Crit: return "Crit toàn đội";
                case ItemEffect.Projectiles: return "Projectile bonus toàn đội";
                case ItemEffect.Interest: return "Trần lãi vàng";
                case ItemEffect.Armor: return "Giáp Champion";
                case ItemEffect.Discount: return "Giảm giá shop";
                default: return effect.ToString();
            }
        }
    }
}
