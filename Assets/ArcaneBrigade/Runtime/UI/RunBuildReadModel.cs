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
            model.Stat("Máu người dẫn đoàn", health.ToString("0.0") + " / " + Mathf.Max(1f, champion.Health + session.Bonus(ItemEffect.Health)).ToString("0.0"), "Máu tối đa bằng máu gốc cộng nâng cấp trong lượt, tối thiểu 1. Không tăng chỉ số vĩnh viễn.");
            model.Stat("Sát thương người dẫn đoàn", (weapon.Damage * session.DamageMultiplier(champion, 1, false)).ToString("0.0"), "Gốc " + weapon.Damage.ToString("0.0") + "; gồm tăng sát thương và nội tại theo số đồng minh. Chưa tính chí mạng, giáp mục tiêu hoặc phản ứng nguyên tố.");
            model.Stat("Chu kỳ đánh người dẫn đoàn", (weapon.Cooldown / Mathf.Max(0.1f, 1f + session.Bonus(ItemEffect.AttackSpeed, ModifierScope.Champion))).ToString("0.00") + " s", "Chu kỳ gốc " + weapon.Cooldown.ToString("0.00") + " giây; tất cả đòn đánh đều tự động.");
            model.Stat("Tốc độ di chuyển", (champion.Speed * Mathf.Max(0.1f, 1f + session.Bonus(ItemEffect.MoveSpeed, ModifierScope.Champion))).ToString("0.00"), "Gốc " + champion.Speed.ToString("0.00") + "; bonus " + Percent(session.Bonus(ItemEffect.MoveSpeed, ModifierScope.Champion)) + "; hệ số tối thiểu 0.1.");
            model.Stat("Chí mạng người dẫn đoàn", Percent(Mathf.Clamp01(champion.CriticalChance + session.Bonus(ItemEffect.Crit, ModifierScope.Champion))), "Gốc " + Percent(champion.CriticalChance) + "; cộng thưởng rồi giới hạn từ 0 đến 100%.");
            model.Stat("Số đạn người dẫn đoàn", (weapon.ProjectileCount + Mathf.FloorToInt(session.Bonus(ItemEffect.Projectiles, ModifierScope.Champion))).ToString(), "Chỉ áp dụng cho vũ khí bắn đạn, không biến đòn cận chiến thành đạn.");
            model.Stat("Giáp người dẫn đoàn", session.Bonus(ItemEffect.Armor).ToString("0.00"), "Sát thương nhận chia cho (1 + giáp). Giáp âm được tính là 0.");
            model.Stat("Hồi máu thụ động", (session.Bonus(ItemEffect.Heal) + (session.ActiveModifier == null ? 0f : session.ActiveModifier.Healing)).ToString("0.00") + " / s", "Hồi máu mỗi giây từ vật phẩm và hiệu ứng màn. Nội tại hồi máu của từng đơn vị kích hoạt riêng mỗi 4 giây, không gộp vào số này.");
            model.Stat("Bán kính nhặt đồ", (catalog.Arena.PickupRadius + session.Bonus(ItemEffect.Pickup)).ToString("0.00"), "Gốc " + catalog.Arena.PickupRadius.ToString("0.00") + "; khoảng cách trong đấu trường, không phải điểm ảnh màn hình.");
            model.Stat("Phạm vi đi lại đồng minh", (catalog.Arena.CompanionLeash + session.Bonus(ItemEffect.Roam)).ToString("0.00"), "Đồng minh tự đi quanh người chơi; không cần điều khiển đội hình.");
            model.Stat("Đồng minh chặn đạn", session.Bonus(ItemEffect.Interceptor) > 0 ? "Đã bật bởi item" : "Không", "Đồng minh bất tử nhưng không chặn đạn mặc định.");
            model.Stat("Lãi cửa hàng kế tiếp", Mathf.Min(catalog.InterestCap + Mathf.FloorToInt(session.Bonus(ItemEffect.Interest)), session.State.Gold / Mathf.Max(1, catalog.InterestDivisor)).ToString() + " vàng", "Dự báo từ vàng đã lưu; chưa tính đồ nhặt chưa chốt hoặc thu nhập màn kế tiếp.");
            model.Stat("Phí đổi hàng kế tiếp", session.RerollCost + " vàng", "Đã đổi " + session.State.RerollsThisShop + " lần tại cửa hàng này. Giá gốc " + catalog.RerollPrice + "; mỗi lần thêm " + catalog.RerollPriceStep + ". Phiếu giảm giá chỉ giảm món bày sau đó, không giảm phí đổi hàng.");
            if (catalog.AltarRevision > 0) model.Stat("Quầy đặc biệt ở lần nghỉ tới", Percent(session.AltarActivationChance), "Cơ hội xuất hiện tăng theo số trứng đang giữ, tối đa 100%. Chỉ chọn một lần khi tạo khu nghỉ, không chọn lại khi tải lượt. Không tiêu trứng để mở quầy.");
            if (catalog.LootRevision > 0)
            {
                model.Stat("May mắn", session.Bonus(ItemEffect.Luck).ToString("0.###"), "Tăng cơ hội nhặt trứng, tim từ kẻ địch và phần thưởng rương. Không thay kết quả ngẫu nhiên chiến đấu hoặc cửa hàng.");
                model.Stat("Tìm kho báu", session.Bonus(ItemEffect.TreasureDetection).ToString("0.###"), "Tăng chỉ số này để rương xuất hiện sớm hơn, tối thiểu cách nhau 4 giây. Đi tới gần để mở, không cần nút kỹ năng.");
            }
            foreach (RosterEntry entry in session.State.Roster)
            {
                UnitDefinition unit = catalog.Unit(entry.UnitId);
                WeaponDefinition actual = catalog.Weapon(entry.WeaponId);
                float damage = actual.Damage * session.DamageMultiplier(unit, entry.Tier, true);
                if (catalog.StatRevision > 0) damage = Mathf.Max(0, actual.Damage + session.Bonus(actual.Kind == WeaponKind.Projectile ? ItemEffect.RangedDamage : ItemEffect.MeleeDamage, ModifierScope.Companions)) * session.DamageMultiplier(unit, entry.Tier, true) * K10StatRules.Factor(session.Bonus(ItemEffect.DamageCorrection, ModifierScope.Companions));
                float cooldown = actual.Cooldown / Mathf.Max(0.1f, 1f + session.Bonus(ItemEffect.AttackSpeed, ModifierScope.Companions) + session.Bonus(ItemEffect.CompanionAttackSpeed));
                int tierSteps = catalog.ContentRevision > 0 ? Mathf.Max(0, entry.Tier - 1) : 0;
                if (actual.Kind != WeaponKind.Projectile) cooldown /= 1 + tierSteps * 0.12f;
                float range = actual.Range * (actual.Kind == WeaponKind.Projectile ? 1 : 1 + tierSteps * 0.1f);
                if (catalog.StatRevision > 0) range *= K10StatRules.Factor(session.Bonus(ItemEffect.AttackRange, ModifierScope.Companions));
                float passive = (unit.Passive == PassiveKind.Scavenge ? Mathf.Round(unit.PassiveStrength) : unit.PassiveStrength) * Mathf.Pow(catalog.TierSupportMultiplier, entry.Tier - 1);
                model.Companions.Add(new Row { Name = RunUICopy.UnitName(unit), Icon = unit.Body, WeaponIcon = actual.Sprite, Summary = "Cấp " + entry.Tier + " · " + RunUICopy.WeaponName(actual.Id) + "\n" + RunUICopy.WeaponKindName(actual.Kind) + " / " + RunUICopy.ElementName(actual.Element),
                    CombatValues = new Dictionary<string, string> { { "Damage", damage.ToString("0.###") }, { "Chu kỳ", cooldown.ToString("0.###") + " s" }, { "Crit", Percent(Mathf.Clamp01(unit.CriticalChance + session.Bonus(ItemEffect.Crit, ModifierScope.Companions))) }, { "Speed", (unit.Speed * Mathf.Max(0.1f, 1f + session.Bonus(ItemEffect.MoveSpeed, ModifierScope.Companions))).ToString("0.###") }, { "Projectile", (actual.ProjectileCount + (actual.Kind == WeaponKind.Projectile ? tierSteps : 0) + Mathf.FloorToInt(session.Bonus(ItemEffect.Projectiles, ModifierScope.Companions))).ToString() }, { "Passive", passive.ToString("0.###") } },
                    Explanation = "Vũ khí đã nhận: " + RunUICopy.WeaponName(actual.Id) + "\nSát thương: " + damage.ToString("0.0") + " / chu kỳ " + cooldown.ToString("0.00") + " giây\nTầm đánh: " + range.ToString("0.0") + "\n" + (tierSteps > 0 ? actual.Kind == WeaponKind.Projectile ? "Nâng cấp: thêm " + tierSteps + " đạn và " + tierSteps + " lượt xuyên.\n" : "Nâng cấp: tầm đánh, góc quét và tốc độ đánh tăng.\n" : "") + SignatureDescription.Describe(actual, entry.Tier, session.StackCount("reflective-ink") > 0) + "\nNội tại: " + RunUICopy.PassiveDescription(unit.Passive) + " · hiệu lực " + passive.ToString("0.0") + "\nBất tử. Mỗi đồng minh tính một người cho liên kết hệ, không nhân theo cấp." });
                model.Companions[model.Companions.Count - 1].Explanation += "\n\nĐẶC TÍNH HỖ TRỢ\n" + RunModifierRules.DescribeCompanion(unit, entry.Tier);
            }
            if (catalog.StatRevision > 0) K10BuildReadModel.Append(model, session, catalog, health);
            foreach (ItemStack stack in session.State.Items)
            {
                ItemDefinition item = catalog.Item(stack.Id);
                string procDescription = "";
                foreach (ProcRule rule in catalog.ProcRules ?? new ProcRule[0])
                    if (rule.ItemId == item.Id) procDescription += "\nKhi " + RunUICopy.TriggerName(rule.Trigger) + " → " + RunUICopy.ActionName(rule.Action) + "\nHiệu lực " + (rule.Power * item.EffectiveStacks(stack.Count)).ToString("0.###") + " · bán kính " + rule.Radius + " · hồi " + rule.Cooldown + " giây · cơ hội " + Percent(rule.Chance) + "\nHiệu ứng này không kích hoạt hiệu ứng vật phẩm khác.\n";
                model.Relics.Add(new Row { Name = item.Name, Icon = item.Icon, Summary = "Sở hữu ×" + stack.Count + " · " + RunModifierRules.RarityName(item.Rarity) + "\n" + (item.EffectiveModifiers.Length > 1 ? item.EffectiveModifiers.Length + " hiệu ứng · xem cả lợi/hại" : RunModifierRules.ScopeName(item.EffectiveModifiers[0].Scope)), Explanation = item.Description + "\n\nStack sở hữu: " + stack.Count + " / hiệu lực: " + item.EffectiveStacks(stack.Count) + "\n" + RunModifierRules.Describe(item, stack.Count) + "\nNâng cấp trong lượt. Không bán, đổi hoặc tháo đồ tại đây." });
                model.Relics[model.Relics.Count - 1].Explanation += procDescription;
                if (!string.IsNullOrEmpty(procDescription)) model.Relics[model.Relics.Count - 1].Summary = "Sở hữu ×" + stack.Count + " · " + RunModifierRules.RarityName(item.Rarity) + "\nTự kích hoạt theo điều kiện";
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
                model.Links.Add(new Row { Name = rule.Name, Icon = catalog.Item(rule.ItemIds[0]).Icon, Summary = status + "\n" + (signature ? "Chuỗi có giới hạn · số đo thật" : "Không có thưởng bộ riêng"), Explanation = explanation + "Liên kết giải thích nguồn chỉ số, không cộng thưởng ẩn. Số lượng hiển thị trước giới hạn chiến đấu." + measured });
            }
            foreach (BondDefinition bond in catalog.Bonds)
            {
                int count = session.BondCounts.TryGetValue(bond.Id, out int value) ? value : 0;
                int activations = bond.ActiveLevel(count);
                string description = RunUICopy.BondDescription(bond, count);
                model.Bonds += description + "\n\n";
                model.Links.Add(new Row { Name = "Hệ " + bond.Name, Icon = bond.Icon, Summary = count + "/" + bond.NextThreshold(count) + " người · " + (activations > 0 ? "kích hoạt ×" + activations : "chưa đủ"), Explanation = description });
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
                foreach (RosterEntry entry in session.State.Roster)
                    foreach (StatModifier modifier in catalog.Unit(entry.UnitId).CompanionModifiers ?? new StatModifier[0])
                        if (modifier.Value != 0 && System.Array.IndexOf(effects, modifier.Effect) >= 0 && RunModifierRules.Applies(modifier.Scope, target))
                        {
                            sources += catalog.Unit(entry.UnitId).DisplayName + " / cấp " + entry.Tier + " / " + RunModifierRules.ScopeName(modifier.Scope) + " / cộng " + (modifier.Value * UnitDefinition.ModifierScale(entry.Tier)).ToString("+0.###;-0.###;0") + "\n";
                            sourceCount++;
                        }
                foreach (StatModifier modifier in champion.ChampionModifiers ?? new StatModifier[0])
                    if (modifier.Value != 0 && System.Array.IndexOf(effects, modifier.Effect) >= 0 && RunModifierRules.Applies(modifier.Scope, target))
                    {
                        sources += RunUICopy.UnitName(champion) + " / đặc tính riêng / " + RunModifierRules.ScopeName(modifier.Scope) + " / cộng " + modifier.Value.ToString("+0.###;-0.###;0") + "\n";
                        sourceCount++;
                    }
                foreach (ItemStack stack in session.State.Items)
                {
                    ItemDefinition item = catalog.Item(stack.Id);
                    foreach (StatModifier modifier in item.EffectiveModifiers)
                        if (modifier.Value != 0 && System.Array.IndexOf(effects, modifier.Effect) >= 0 && RunModifierRules.Applies(modifier.Scope, target))
                        {
                            sources += item.Name + " ×" + item.EffectiveStacks(stack.Count) + " / " + RunModifierRules.ScopeName(modifier.Scope) + " / cộng " + (modifier.Value * item.EffectiveStacks(stack.Count)).ToString("+0.###;-0.###;0") + "\n";
                            sourceCount++;
                        }
                }
                foreach (BondDefinition bond in catalog.Bonds)
                    if (System.Array.IndexOf(effects, bond.Effect) >= 0 && RunModifierRules.Applies(RunModifierRules.DefaultScope(bond.Effect), target) && session.BondCounts.TryGetValue(bond.Id, out int count) && count >= bond.Threshold)
                    {
                        sources += bond.Name + " / " + count + " đồng minh / bậc " + bond.ActiveLevel(count) + " / cộng " + bond.Reward(count).ToString("+0.###;-0.###;0") + "\n"; sourceCount++;
                    }
                row.Explanation += sources + (sourceCount == 0 ? "Không có nguồn cộng thêm cho chỉ số này.\n" : "Nguồn hiển thị trước giới hạn, cấp và nội tại; giá trị áp dụng nằm ở bảng chỉ số.\n");
            }
            return model;
        }

        private static ItemEffect[] SourceEffects(string name)
        {
            switch (name)
            {
                case "Máu người dẫn đoàn": return new[] { ItemEffect.Health };
                case "Sát thương người dẫn đoàn": return new[] { ItemEffect.Damage, ItemEffect.ChampionDamage };
                case "Chu kỳ đánh người dẫn đoàn": return new[] { ItemEffect.AttackSpeed };
                case "Tốc độ di chuyển": return new[] { ItemEffect.MoveSpeed };
                case "Chí mạng người dẫn đoàn": return new[] { ItemEffect.Crit };
                case "Số đạn người dẫn đoàn": return new[] { ItemEffect.Projectiles };
                case "Giáp người dẫn đoàn": return new[] { ItemEffect.Armor };
                case "Hồi máu thụ động": return new[] { ItemEffect.Heal };
                case "Bán kính nhặt đồ": return new[] { ItemEffect.Pickup };
                case "Phạm vi đi lại đồng minh": return new[] { ItemEffect.Roam };
                case "Đồng minh chặn đạn": return new[] { ItemEffect.Interceptor };
                case "Lãi cửa hàng kế tiếp": return new[] { ItemEffect.Interest };
                case "May mắn": return new[] { ItemEffect.Luck };
                case "Tìm kho báu": return new[] { ItemEffect.TreasureDetection };
                default: return new ItemEffect[0];
            }
        }

        private void Stat(string name, string value, string explanation) { Stats.Add(new Row { Name = name, Summary = value, Explanation = explanation }); }
        private static string Percent(float value) => (value * 100f).ToString("0.#") + "%";
        public static string EffectName(ItemEffect effect)
        {
            switch (effect)
            {
                case ItemEffect.Damage: return "Sát thương toàn đội";
                case ItemEffect.ChampionDamage: return "Sát thương người dẫn đoàn";
                case ItemEffect.CompanionDamage: return "Sát thương đồng minh";
                case ItemEffect.AttackSpeed: return "Tốc độ đánh toàn đội";
                case ItemEffect.CompanionAttackSpeed: return "Tốc độ đánh đồng minh";
                case ItemEffect.Health: return "Máu tối đa người dẫn đoàn";
                case ItemEffect.MoveSpeed: return "Tốc độ di chuyển toàn đội";
                case ItemEffect.Pickup: return "Bán kính nhặt đồ";
                case ItemEffect.Roam: return "Phạm vi đi lại đồng minh";
                case ItemEffect.Interceptor: return "Đồng minh chặn đạn";
                case ItemEffect.Heal: return "Hồi máu người dẫn đoàn";
                case ItemEffect.Crit: return "Chí mạng toàn đội";
                case ItemEffect.Projectiles: return "Số đạn thêm toàn đội";
                case ItemEffect.Interest: return "Trần lãi vàng";
                case ItemEffect.Armor: return "Giáp người dẫn đoàn";
                case ItemEffect.Discount: return "Giảm giá cửa hàng";
                case ItemEffect.Luck: return "May mắn";
                case ItemEffect.TreasureDetection: return "Tìm kho báu";
                default: return RunModifierRules.ModifierName(effect);
            }
        }
    }
}
