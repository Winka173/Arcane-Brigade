using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class RunBuildReadModel
    {
        public sealed class Row { public string Name; public string Summary; public string Explanation; public Dictionary<string, string> CombatValues; public Sprite Icon; public Sprite WeaponIcon; }
        public readonly List<Row> Stats = new List<Row>();
        public readonly List<Row> Companions = new List<Row>();
        public readonly List<Row> Relics = new List<Row>();
        public readonly List<Row> Links = new List<Row>();
        public string Bonds;
        public string Receipts;
        public string BuildLinks;

        public static RunBuildReadModel Capture(RunSession session, RunCatalog catalog, float health, CombatSimulation simulation = null)
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
            if (catalog.LootRevision > 0)
            {
                model.Stat("Luck", session.Bonus(ItemEffect.Luck).ToString("0.###"), "Tăng chance egg/heart enemy và reward chest, clamp 0–100%. RNG loot riêng, không đổi roll combat/shop.");
                model.Stat("Treasure Detection", session.Bonus(ItemEffect.TreasureDetection).ToString("0.###"), "Rương xuất hiện mỗi max(4, 24 / max(0.25, 1 + detection)) giây. Đi tới gần để mở, không thêm nút skill.");
            }
            foreach (RosterEntry entry in session.State.Roster)
            {
                UnitDefinition unit = catalog.Unit(entry.UnitId);
                WeaponDefinition actual = catalog.Weapon(entry.WeaponId);
                float damage = actual.Damage * session.DamageMultiplier(unit, entry.Tier, true);
                float cooldown = actual.Cooldown / Mathf.Max(0.1f, 1f + session.Bonus(ItemEffect.AttackSpeed, ModifierScope.Companions) + session.Bonus(ItemEffect.CompanionAttackSpeed));
                float passive = (unit.Passive == PassiveKind.Scavenge ? Mathf.Round(unit.PassiveStrength) : unit.PassiveStrength) * Mathf.Pow(catalog.TierSupportMultiplier, entry.Tier - 1);
                model.Companions.Add(new Row { Name = unit.DisplayName, Icon = unit.Body, WeaponIcon = actual.Sprite, Summary = "Cấp " + entry.Tier + " · " + actual.Id + "\n" + actual.Kind + " / " + actual.Element,
                    CombatValues = new Dictionary<string, string> { { "Damage", damage.ToString("0.###") }, { "Chu kỳ", cooldown.ToString("0.###") + " s" }, { "Crit", Percent(Mathf.Clamp01(unit.CriticalChance + session.Bonus(ItemEffect.Crit, ModifierScope.Companions))) }, { "Speed", (unit.Speed * Mathf.Max(0.1f, 1f + session.Bonus(ItemEffect.MoveSpeed, ModifierScope.Companions))).ToString("0.###") }, { "Projectile", (actual.ProjectileCount + Mathf.FloorToInt(session.Bonus(ItemEffect.Projectiles, ModifierScope.Companions))).ToString() }, { "Passive", passive.ToString("0.###") } },
                    Explanation = unit.Description + "\n\nWeapon đã roll: " + actual.Id + "\nDamage: " + damage.ToString("0.0") + " / chu kỳ " + cooldown.ToString("0.00") + " s\nRange: " + actual.Range.ToString("0.0") + "\n" + SignatureDescription.Describe(actual, entry.Tier, session.StackCount("reflective-ink") > 0) + "\nPassive: " + unit.Passive + " / strength " + passive.ToString("0.0") + "\nBond: " + string.Join(" / ", unit.Tags) + "\nBất tử. Một body tính một đơn vị bond, không nhân theo tier." });
            }
            foreach (ItemStack stack in session.State.Items)
            {
                ItemDefinition item = catalog.Item(stack.Id);
                string procDescription = "";
                foreach (ProcRule rule in catalog.ProcRules ?? new ProcRule[0])
                    if (rule.ItemId == item.Id) procDescription += "\nTRIGGER " + rule.Trigger + " → " + rule.Action + "\nPower " + (rule.Power * item.EffectiveStacks(stack.Count)).ToString("0.###") + " · radius " + rule.Radius + " · cooldown " + rule.Cooldown + " s · chance " + Percent(rule.Chance) + "\nProc không kích hoạt proc khác.\n";
                model.Relics.Add(new Row { Name = item.Name, Icon = item.Icon, Summary = "Sở hữu ×" + stack.Count + " · " + RunModifierRules.RarityName(item.Rarity) + "\n" + (item.EffectiveModifiers.Length > 1 ? item.EffectiveModifiers.Length + " hiệu ứng · xem cả lợi/hại" : RunModifierRules.ScopeName(item.EffectiveModifiers[0].Scope)), Explanation = item.Description + "\n\nStack sở hữu: " + stack.Count + " / hiệu lực: " + item.EffectiveStacks(stack.Count) + "\n" + RunModifierRules.Describe(item, stack.Count) + "\nNâng cấp trong lượt. Không bán, đổi hoặc tháo đồ tại đây." });
                model.Relics[model.Relics.Count - 1].Explanation += procDescription;
                if (!string.IsNullOrEmpty(procDescription)) model.Relics[model.Relics.Count - 1].Summary = "Sở hữu ×" + stack.Count + " · " + RunModifierRules.RarityName(item.Rarity) + "\nPassive proc · theo trigger";
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
                bool signature = rule.Id.StartsWith("signature-");
                string status = signature ? SignatureBuildGraph.Status(rule.Id, session, catalog) : (owned == rule.ItemIds.Length ? "ĐỦ NGUỒN" : "ĐANG GHÉP") + " · " + owned + "/" + rule.ItemIds.Length;
                string measured = signature ? "\n\nĐÓNG GÓP ĐÃ ĐO\n" + ContributionLedger.Describe(session.State.Contributions) + (simulation == null ? "" : "\nHALL HIỆN TẠI (chưa settle)\n" + ContributionLedger.Describe(simulation.Contributions)) : "";
                model.Links.Add(new Row { Name = rule.Name, Icon = catalog.Item(rule.ItemIds[0]).Icon, Summary = status + "\n" + (signature ? "Graph hữu hạn · số đo thật" : "Không phải bonus set"), Explanation = explanation + "Liên kết là cách đọc nguồn stat; không cộng bonus ẩn. Stack hiển thị trước clamp combat." + measured });
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
            foreach (Row row in model.Stats)
            {
                ItemEffect[] effects = SourceEffects(row.Name);
                if (effects.Length == 0) continue;
                ModifierScope target = effects[0] == ItemEffect.Luck || effects[0] == ItemEffect.TreasureDetection || effects[0] == ItemEffect.Pickup || effects[0] == ItemEffect.Roam || effects[0] == ItemEffect.Interest || effects[0] == ItemEffect.Interceptor ? ModifierScope.Run : ModifierScope.Champion;
                string sources = "\n\nNGUỒN MODIFIER TRONG LƯỢT\n";
                int sourceCount = 0;
                foreach (ItemStack stack in session.State.Items)
                {
                    ItemDefinition item = catalog.Item(stack.Id);
                    foreach (StatModifier modifier in item.EffectiveModifiers)
                        if (modifier.Value != 0 && System.Array.IndexOf(effects, modifier.Effect) >= 0 && RunModifierRules.Applies(modifier.Scope, target))
                        {
                            sources += item.Name + " ×" + item.EffectiveStacks(stack.Count) + " / " + RunModifierRules.ScopeName(modifier.Scope) + " / additive " + (modifier.Value * item.EffectiveStacks(stack.Count)).ToString("+0.###;-0.###;0") + "\n";
                            sourceCount++;
                        }
                }
                foreach (BondDefinition bond in catalog.Bonds)
                    if (System.Array.IndexOf(effects, bond.Effect) >= 0 && RunModifierRules.Applies(RunModifierRules.DefaultScope(bond.Effect), target) && session.BondCounts.TryGetValue(bond.Id, out int count) && count >= bond.Threshold)
                    {
                        sources += bond.Name + " / " + count + " body / activation ×" + count / bond.Threshold + " / additive " + (bond.Value * (count / bond.Threshold)).ToString("+0.###;-0.###;0") + "\n"; sourceCount++;
                    }
                row.Explanation += sources + (sourceCount == 0 ? "Không có modifier item/bond cho scope này.\n" : "Giá trị nguồn trước clamp/tier/passive; số effective nằm ở bảng stat.\n");
            }
            return model;
        }

        private static ItemEffect[] SourceEffects(string name)
        {
            switch (name)
            {
                case "Máu Champion": return new[] { ItemEffect.Health };
                case "Sát thương Champion": return new[] { ItemEffect.Damage, ItemEffect.ChampionDamage };
                case "Chu kỳ đánh Champion": return new[] { ItemEffect.AttackSpeed };
                case "Tốc độ di chuyển": return new[] { ItemEffect.MoveSpeed };
                case "Crit Champion": return new[] { ItemEffect.Crit };
                case "Số projectile Champion": return new[] { ItemEffect.Projectiles };
                case "Giáp Champion": return new[] { ItemEffect.Armor };
                case "Hồi máu thụ động": return new[] { ItemEffect.Heal };
                case "Bán kính nhặt loot": return new[] { ItemEffect.Pickup };
                case "Leash đồng minh": return new[] { ItemEffect.Roam };
                case "Đồng minh chặn đạn": return new[] { ItemEffect.Interceptor };
                case "Lãi shop kế tiếp": return new[] { ItemEffect.Interest };
                case "Luck": return new[] { ItemEffect.Luck };
                case "Treasure Detection": return new[] { ItemEffect.TreasureDetection };
                default: return new ItemEffect[0];
            }
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
                case ItemEffect.Luck: return "Luck";
                case ItemEffect.TreasureDetection: return "Treasure Detection";
                default: return effect.ToString();
            }
        }
    }
}
