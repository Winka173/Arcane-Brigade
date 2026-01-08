using System;
using UnityEngine;

namespace ArcaneBrigade
{
    public static class K10BuildReadModel
    {
        public static void Append(RunBuildReadModel model, RunSession session, RunCatalog catalog, float health)
        {
            model.Stats[0].Summary = health.ToString("0.0") + " / " + K10StatRules.MaximumHealth(session, catalog).ToString("0.0");
            UnitDefinition champion = catalog.Unit(session.State.ChampionId);
            WeaponDefinition weapon = champion.Weapon;
            float damage = Mathf.Max(0, weapon.Damage + session.Bonus(weapon.Kind == WeaponKind.Projectile ? ItemEffect.RangedDamage : ItemEffect.MeleeDamage, ModifierScope.Champion)) * session.DamageMultiplier(champion, 1, false) * K10StatRules.Factor(session.Bonus(ItemEffect.DamageCorrection, ModifierScope.Champion));
            model.Stats[1].Summary = damage.ToString("0.0");
            model.Stats[3].Summary = (champion.Speed * Mathf.Max(0.1f, 1 + session.Bonus(ItemEffect.MoveSpeed, ModifierScope.Champion)) * K10StatRules.Factor(session.Bonus(ItemEffect.SpeedCorrection, ModifierScope.Champion))).ToString("0.00");
            model.Stats[6].Explanation = "Giáp dương giảm damage theo 1/(1+giáp). Giáp âm tăng damage nhận, tối đa ×2 trước giới hạn mỗi đòn.";
            model.Stats[6].Summary = (session.Bonus(ItemEffect.Armor) * K10StatRules.Factor(session.Bonus(ItemEffect.ArmorCorrection))).ToString("0.00");
            foreach (RunBuildReadModel.Row row in model.Stats)
            {
                if (row.Name == "May mắn") row.Explanation = "Tăng loot và góp 25% giá trị Luck vào trọng số độ hiếm hàng khi roll shop. Không tăng damage hoặc xác suất chí mạng.";
                if (row.Name == "Tìm kho báu") row.Explanation = "Rương xuất hiện sớm hơn; thêm 25% giá trị detection vào chất lượng vàng rương, có sàn/cap.";
            }
            foreach (ItemEffect effect in Enum.GetValues(typeof(ItemEffect)))
            {
                if (effect < ItemEffect.ScoreMultiplier) continue;
                ModifierScope scope = RunModifierRules.DefaultScope(effect);
                float value = session.Bonus(effect, scope);
                string sources = "Giữ trong lượt, kể cả qua wave/vô tận. Lượt mới reset.\n";
                foreach (StatModifier modifier in champion.ChampionModifiers)
                    if (modifier.Effect == effect && (scope == ModifierScope.Team || RunModifierRules.Applies(modifier.Scope, scope))) sources += RunUICopy.UnitName(champion) + " / đặc tính riêng / " + RunModifierRules.ScopeName(modifier.Scope) + ": " + RunModifierRules.ValueText(effect, modifier.Value) + "\n";
                foreach (RosterEntry entry in session.State.Roster)
                    foreach (StatModifier modifier in catalog.Unit(entry.UnitId).CompanionModifiers)
                        if (modifier.Effect == effect && (scope == ModifierScope.Team || RunModifierRules.Applies(modifier.Scope, scope))) sources += catalog.Unit(entry.UnitId).DisplayName + " cấp " + entry.Tier + " / " + RunModifierRules.ScopeName(modifier.Scope) + ": " + RunModifierRules.ValueText(effect, modifier.Value * UnitDefinition.ModifierScale(entry.Tier)) + "\n";
                foreach (ItemStack stack in session.State.Items)
                    foreach (StatModifier modifier in catalog.Item(stack.Id).EffectiveModifiers)
                        if (modifier.Effect == effect && (scope == ModifierScope.Team || RunModifierRules.Applies(modifier.Scope, scope))) sources += catalog.Item(stack.Id).Name + " ×" + stack.Count + " / " + RunModifierRules.ScopeName(modifier.Scope) + ": " + RunModifierRules.ValueText(effect, modifier.Value * catalog.Item(stack.Id).EffectiveStacks(stack.Count)) + "\n";
                foreach (BondDefinition bond in catalog.Bonds)
                    if (bond.Effect == effect && session.BondCounts.TryGetValue(bond.Id, out int count)) sources += "Hệ " + bond.Name + ": " + bond.Reward(count) + "\n";
                string summary = (K10StatRules.Percentage(effect) ? value * 100 : value).ToString("+0.###;-0.###;0") + (K10StatRules.Percentage(effect) ? "%" : "");
                if (scope == ModifierScope.Team) summary = "Thủ lĩnh " + RunModifierRules.ValueText(effect, session.Bonus(effect, ModifierScope.Champion)) + " / Đồng minh " + RunModifierRules.ValueText(effect, session.Bonus(effect, ModifierScope.Companions));
                string limit = effect == ItemEffect.Dodge ? "\nNé tránh hiệu lực 0–60%; roll riêng mỗi đòn, không thêm i-frame." : effect == ItemEffect.BulletSplit ? "\n0–6 nhánh; chỉ split thế hệ đầu, không đệ quy. Nhánh giữ 35% damage và 70% kích thước." : effect == ItemEffect.BulletBounce ? "\nXác suất 0–100%, tối đa 8 lần nảy xác suất; không hồi lại lifetime." : effect == ItemEffect.BulletSize ? "\nKích thước render và collision cùng nhân, hiệu lực ×0,25–4." : "\nGiá trị cộng trước sàn/cap của từng công thức. Bonus vũ khí/cấp riêng không nằm trong dòng này.";
                model.Stats.Add(new RunBuildReadModel.Row { Name = RunModifierRules.ModifierName(effect), Summary = summary, Explanation = RunModifierRules.ScopeName(scope) + "\n" + sources + limit });
            }
        }
    }
}
