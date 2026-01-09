using System;
using UnityEngine;

namespace ArcaneBrigade
{
    public static class K10BuildReadModel
    {
        public static void Append(RunBuildReadModel model, RunSession session, RunCatalog catalog, float health, CheckpointCatalog compiled)
        {
            model.Stats[0].Summary = health.ToString("0.0") + " / " + K10StatRules.MaximumHealth(session, catalog).ToString("0.0");
            UnitDefinition champion = catalog.Unit(session.State.ChampionId);
            WeaponDefinition weapon = champion.Weapon;
            float damage = compiled.Champion.Weapon.Damage;
            model.Stats[1].Summary = damage.ToString("0.0");
            model.Stats[3].Summary = (champion.Speed * Mathf.Max(0.1f, 1 + session.Bonus(ItemEffect.MoveSpeed, ModifierScope.Champion)) * K10StatRules.Factor(session.Bonus(ItemEffect.SpeedCorrection, ModifierScope.Champion))).ToString("0.00");
            model.Stats[6].Explanation = "Positive armor divides damage by 1+armor. Negative armor increases damage, up to ×2 before the per-hit cap.";
            model.Stats[6].Summary = (session.Bonus(ItemEffect.Armor) * K10StatRules.Factor(session.Bonus(ItemEffect.ArmorCorrection))).ToString("0.00");
            foreach (RunBuildReadModel.Row row in model.Stats)
            {
                if (row.Name == "Luck") row.Explanation = "Increases loot chance and adds 25% of Luck to shop-rarity weights. Does not increase damage or critical chance.";
                if (row.Name == "Treasure detection") row.Explanation = "Chests appear sooner; adds 25% of detection to chest gold quality, with floor/cap.";
            }
            foreach (ItemEffect effect in Enum.GetValues(typeof(ItemEffect)))
            {
                if (effect < ItemEffect.ScoreMultiplier) continue;
                ModifierScope scope = RunModifierRules.DefaultScope(effect);
                float value = session.Bonus(effect, scope);
                string sources = "Lasts for this run, including waves/endless. Resets in a new run.\n";
                foreach (StatModifier modifier in champion.ChampionModifiers)
                    if (modifier.Effect == effect && (scope == ModifierScope.Team || RunModifierRules.Applies(modifier.Scope, scope))) sources += RunUICopy.UnitName(champion) + " / personal trait / " + RunModifierRules.ScopeName(modifier.Scope) + ": " + RunModifierRules.ValueText(effect, modifier.Value) + "\n";
                foreach (RosterEntry entry in session.State.Roster)
                    foreach (StatModifier modifier in catalog.Unit(entry.UnitId).CompanionModifiers)
                        if (modifier.Effect == effect && (scope == ModifierScope.Team || RunModifierRules.Applies(modifier.Scope, scope))) sources += catalog.Unit(entry.UnitId).DisplayName + " tier " + entry.Tier + " / " + RunModifierRules.ScopeName(modifier.Scope) + ": " + RunModifierRules.ValueText(effect, modifier.Value * UnitDefinition.ModifierScale(entry.Tier)) + "\n";
                foreach (ItemStack stack in session.State.Items)
                    foreach (StatModifier modifier in catalog.Item(stack.Id).EffectiveModifiers)
                        if (modifier.Effect == effect && (scope == ModifierScope.Team || RunModifierRules.Applies(modifier.Scope, scope))) sources += catalog.Item(stack.Id).Name + " ×" + stack.Count + " / " + RunModifierRules.ScopeName(modifier.Scope) + ": " + RunModifierRules.ValueText(effect, modifier.Value * catalog.Item(stack.Id).EffectiveStacks(stack.Count)) + "\n";
                foreach (BondDefinition bond in catalog.Bonds)
                    if (bond.Effect == effect && session.BondCounts.TryGetValue(bond.Id, out int count)) sources += "Bond " + bond.Name + ": " + bond.Reward(count) + "\n";
                string summary = (K10StatRules.Percentage(effect) ? value * 100 : value).ToString("+0.###;-0.###;0") + (K10StatRules.Percentage(effect) ? "%" : "");
                if (scope == ModifierScope.Team) summary = "Champion " + RunModifierRules.ValueText(effect, session.Bonus(effect, ModifierScope.Champion)) + " / Companions " + RunModifierRules.ValueText(effect, session.Bonus(effect, ModifierScope.Companions));
                string limit = effect == ItemEffect.Dodge ? "\nEffective dodge 0–60%; rolled separately per hit, without added i-frames." : effect == ItemEffect.BulletSplit ? "\n0–6 branches; first generation only, no recursion. Children retain 35% damage and 70% size." : effect == ItemEffect.BulletBounce ? "\nChance 0–100%, up to 8 chance-based bounces; lifetime is not reset." : effect == ItemEffect.BulletSize ? "\nRendering and collision scale together, effective ×0.25–4." : "\nBonuses add before each formula's floor/cap. Separate weapon/tier bonuses are not included here.";
                model.Stats.Add(new RunBuildReadModel.Row { Name = RunModifierRules.ModifierName(effect), Summary = summary, Explanation = RunModifierRules.ScopeName(scope) + "\n" + sources + limit });
            }
        }
    }
}
