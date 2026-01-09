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
            using var readSession = new RunSession(catalog);
            readSession.Restore(session.State);
            CheckpointCatalog compiled = readSession.PrepareCombat();
            UnitDefinition champion = catalog.Unit(session.State.ChampionId);
            WeaponDefinition weapon = champion.Weapon;
            model.Stat("Champion health", health.ToString("0.0") + " / " + Mathf.Max(1f, champion.Health + session.Bonus(ItemEffect.Health)).ToString("0.0"), "Maximum health includes base HP, run bonuses and health correction, minimum 1. No permanent profile stat growth. Every cleared wave restores full HP.");
            model.Stat("Champion damage", compiled.Champion.Weapon.Damage.ToString("0.0"), "Base " + weapon.Damage.ToString("0.0") + "; includes melee balance, damage bonuses and roster-based passive. Excludes critical hits, target armor and elemental reactions.");
            model.Stat("Champion attack cycle", (weapon.Cooldown / Mathf.Max(0.1f, 1f + session.Bonus(ItemEffect.AttackSpeed, ModifierScope.Champion))).ToString("0.00") + " s", "Base cycle " + weapon.Cooldown.ToString("0.00") + " seconds; every attack is automatic.");
            model.Stat("Movement speed", (champion.Speed * Mathf.Max(0.1f, 1f + session.Bonus(ItemEffect.MoveSpeed, ModifierScope.Champion))).ToString("0.00"), "Base " + champion.Speed.ToString("0.00") + "; bonus " + Percent(session.Bonus(ItemEffect.MoveSpeed, ModifierScope.Champion)) + "; minimum multiplier 0.1.");
            model.Stat("Champion critical chance", Percent(Mathf.Clamp01(champion.CriticalChance + session.Bonus(ItemEffect.Crit, ModifierScope.Champion))), "Base " + Percent(champion.CriticalChance) + "; bonuses add before the 0–100% cap.");
            model.Stat("Champion projectile count", (weapon.ProjectileCount + Mathf.FloorToInt(session.Bonus(ItemEffect.Projectiles, ModifierScope.Champion))).ToString(), "Applies only to projectile weapons; does not turn melee attacks into bullets.");
            model.Stat("Champion armor", session.Bonus(ItemEffect.Armor).ToString("0.00"), "Positive armor reduces incoming damage; negative armor increases it, capped before the per-hit limit.");
            model.Stat("Passive regeneration", (session.Bonus(ItemEffect.Heal) + (session.ActiveModifier == null ? 0f : session.ActiveModifier.Healing)).ToString("0.00") + " / s", "HP per second from items and wave effects. Individual unit healing triggers separately every 4 seconds and is not included here.");
            model.Stat("Pickup radius", (catalog.Arena.PickupRadius + session.Bonus(ItemEffect.Pickup)).ToString("0.00"), "Base " + catalog.Arena.PickupRadius.ToString("0.00") + "; arena distance, not screen pixels.");
            model.Stat("Companion roaming range", "Ranged " + compiled.CompanionLeash.ToString("0.00") + " / Melee " + (compiled.CompanionLeash + MeleeBalance.RoamBonus).ToString("0.00"), "Companions patrol around the player automatically. Melee units have wider roaming and faster enemy pursuit; no formation controls required.");
            model.Stat("Companion bullet interception", session.Bonus(ItemEffect.Interceptor) > 0 ? "Enabled by item" : "No", "Companions are immortal but do not block bullets by default.");
            model.Stat("Next shop interest", Mathf.Min(catalog.InterestCap + Mathf.FloorToInt(session.Bonus(ItemEffect.Interest)), session.State.Gold / Mathf.Max(1, catalog.InterestDivisor)).ToString() + " gold", "Forecast from saved gold; excludes uncommitted pickups or next-wave income.");
            model.Stat("Next reroll cost", session.RerollCost + " gold", "Rerolled " + session.State.RerollsThisShop + " times in this shop. Base cost " + catalog.RerollPrice + "; each roll adds " + catalog.RerollPriceStep + ". Coupons discount displayed wares, not reroll fees.");
            if (catalog.AltarRevision > 0) model.Stat("Next rest's special shop", Percent(session.AltarActivationChance), "Appearance chance increases with held eggs, up to 100%. Rolled once when creating rest, not again on loading. Opening the shop does not spend eggs.");
            if (catalog.LootRevision > 0)
            {
                model.Stat("Luck", session.Bonus(ItemEffect.Luck).ToString("0.###"), "Increases egg/heart drops and chest rewards. Does not change combat or shop RNG outcomes.");
                model.Stat("Treasure detection", session.Bonus(ItemEffect.TreasureDetection).ToString("0.###"), "Makes chests appear sooner, at least 4 seconds apart. Approach to open; no skill button required.");
            }
            int companionIndex = 0;
            foreach (RosterEntry entry in session.State.Roster)
            {
                UnitDefinition unit = catalog.Unit(entry.UnitId);
                WeaponDefinition actual = catalog.Weapon(entry.WeaponId);
                UnitDefinition applied = compiled.StartingCompanions[companionIndex++];
                float damage = applied.Weapon.Damage;
                float cooldown = applied.Weapon.Cooldown;
                int tierSteps = catalog.ContentRevision > 0 ? Mathf.Max(0, entry.Tier - 1) : 0;
                float range = applied.Weapon.Range;
                float passive = (unit.Passive == PassiveKind.Scavenge ? Mathf.Round(unit.PassiveStrength) : unit.PassiveStrength) * Mathf.Pow(catalog.TierSupportMultiplier, entry.Tier - 1);
                model.Companions.Add(new Row { Name = RunUICopy.UnitName(unit), Icon = unit.Body, WeaponIcon = actual.Sprite, Summary = "Tier " + entry.Tier + " · " + RunUICopy.WeaponName(actual.Id) + "\n" + RunUICopy.WeaponKindName(actual.Kind) + " / " + RunUICopy.ElementName(actual.Element),
                    CombatValues = new Dictionary<string, string> { { "Damage", damage.ToString("0.###") }, { "Cycle", cooldown.ToString("0.###") + " s" }, { "Crit", Percent(applied.CriticalChance) }, { "Speed", applied.Speed.ToString("0.###") }, { "Projectile", applied.Weapon.ProjectileCount.ToString() }, { "Passive", passive.ToString("0.###") } },
                    Explanation = "Assigned weapon: " + RunUICopy.WeaponName(actual.Id) + "\nDamage: " + damage.ToString("0.0") + " / cycle " + cooldown.ToString("0.00") + " seconds\nRange: " + range.ToString("0.0") + "\n" + (tierSteps > 0 ? actual.Kind == WeaponKind.Projectile ? "Tier bonus: adds " + tierSteps + " projectiles and " + tierSteps + " pierce.\n" : "Tier bonus: increased range, sweep arc and attack speed.\n" : "") + SignatureDescription.Describe(actual, entry.Tier, session.StackCount("reflective-ink") > 0) + "\nPassive: " + RunUICopy.PassiveDescription(unit.Passive) + " · effective " + passive.ToString("0.0") + "\nImmortal. Each companion counts as one unit toward bonds, regardless of tier." });
                model.Companions[model.Companions.Count - 1].Explanation += "\n\nSUPPORT TRAITS\n" + RunModifierRules.DescribeCompanion(unit, entry.Tier);
            }
            if (catalog.StatRevision > 0) K10BuildReadModel.Append(model, session, catalog, health, compiled);
            foreach (ItemStack stack in session.State.Items)
            {
                ItemDefinition item = catalog.Item(stack.Id);
                string procDescription = "";
                foreach (ProcRule rule in catalog.ProcRules ?? new ProcRule[0])
                    if (rule.ItemId == item.Id) procDescription += "\nOn " + RunUICopy.TriggerName(rule.Trigger) + " → " + RunUICopy.ActionName(rule.Action) + "\nEffective " + (rule.Power * item.EffectiveStacks(stack.Count)).ToString("0.###") + " · radius " + rule.Radius + " · cooldown " + rule.Cooldown + " seconds · chance " + Percent(rule.Chance) + "\nThis effect does not trigger other item effects.\n";
                model.Relics.Add(new Row { Name = item.Name, Icon = item.Icon, Summary = "Owned ×" + stack.Count + " · " + RunModifierRules.RarityName(item.Rarity) + "\n" + (item.EffectiveModifiers.Length > 1 ? item.EffectiveModifiers.Length + " effects · includes benefits/tradeoffs" : RunModifierRules.ScopeName(item.EffectiveModifiers[0].Scope)), Explanation = item.Description + "\n\nOwned stacks: " + stack.Count + " / effective: " + item.EffectiveStacks(stack.Count) + "\n" + RunModifierRules.Describe(item, stack.Count) + "\nRun upgrade. Cannot sell, swap or remove items here." });
                model.Relics[model.Relics.Count - 1].Explanation += procDescription;
                if (!string.IsNullOrEmpty(procDescription)) model.Relics[model.Relics.Count - 1].Summary = "Owned ×" + stack.Count + " · " + RunModifierRules.RarityName(item.Rarity) + "\nAutomatically triggers under its conditions";
            }
            foreach (BuildLinkRule rule in catalog.BuildLinks ?? new BuildLinkRule[0])
            {
                int owned = 0;
                string sources = "";
                string explanation = rule.Description + "\n\nACTUAL SOURCES IN THIS RUN\n\n";
                foreach (string id in rule.ItemIds)
                {
                    int count = session.StackCount(id);
                    if (count > 0) owned++;
                    sources += catalog.Item(id).Name + " ×" + count + "\n";
                    explanation += catalog.Item(id).Name + " ×" + count + "\n" + (count > 0 ? RunModifierRules.Describe(catalog.Item(id), count) : "Not owned yet.\n") + "\n";
                }
                model.BuildLinks += rule.Name + " · " + owned + "/" + rule.ItemIds.Length + "\n" + rule.Description + "\n" + sources + "Links are read from owned items; no hidden stat bonuses.\n\n";
                bool signature = rule.Id.StartsWith("signature-");
                string status = signature ? SignatureBuildGraph.Status(rule.Id, session, catalog) : (owned == rule.ItemIds.Length ? "SOURCES READY" : "IN PROGRESS") + " · " + owned + "/" + rule.ItemIds.Length;
                string measured = signature ? "\n\nMEASURED CONTRIBUTIONS\n" + ContributionLedger.Describe(session.State.Contributions) + (simulation == null ? "" : "\nCURRENT HALL (not settled)\n" + ContributionLedger.Describe(simulation.Contributions)) : "";
                model.Links.Add(new Row { Name = rule.Name, Icon = catalog.Item(rule.ItemIds[0]).Icon, Summary = status + "\n" + (signature ? "Bounded chains · actual measurements" : "No separate set bonus"), Explanation = explanation + "Links explain stat sources without hidden bonuses. Counts are shown before combat caps." + measured });
            }
            foreach (BondDefinition bond in catalog.Bonds)
            {
                int count = session.BondCounts.TryGetValue(bond.Id, out int value) ? value : 0;
                int activations = bond.ActiveLevel(count);
                string description = RunUICopy.BondDescription(bond, count);
                model.Bonds += description + "\n\n";
                model.Links.Add(new Row { Name = "Bond " + bond.Name, Icon = bond.Icon, Summary = count + "/" + bond.NextThreshold(count) + " units · " + (activations > 0 ? "activations ×" + activations : "not ready"), Explanation = description });
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
                string sources = "\n\nRUN MODIFIER SOURCES\n";
                int sourceCount = 0;
                foreach (RosterEntry entry in session.State.Roster)
                    foreach (StatModifier modifier in catalog.Unit(entry.UnitId).CompanionModifiers ?? new StatModifier[0])
                        if (modifier.Value != 0 && System.Array.IndexOf(effects, modifier.Effect) >= 0 && RunModifierRules.Applies(modifier.Scope, target))
                        {
                            sources += catalog.Unit(entry.UnitId).DisplayName + " / tier " + entry.Tier + " / " + RunModifierRules.ScopeName(modifier.Scope) + " / adds " + (modifier.Value * UnitDefinition.ModifierScale(entry.Tier)).ToString("+0.###;-0.###;0") + "\n";
                            sourceCount++;
                        }
                foreach (StatModifier modifier in champion.ChampionModifiers ?? new StatModifier[0])
                    if (modifier.Value != 0 && System.Array.IndexOf(effects, modifier.Effect) >= 0 && RunModifierRules.Applies(modifier.Scope, target))
                    {
                        sources += RunUICopy.UnitName(champion) + " / personal trait / " + RunModifierRules.ScopeName(modifier.Scope) + " / adds " + modifier.Value.ToString("+0.###;-0.###;0") + "\n";
                        sourceCount++;
                    }
                foreach (ItemStack stack in session.State.Items)
                {
                    ItemDefinition item = catalog.Item(stack.Id);
                    foreach (StatModifier modifier in item.EffectiveModifiers)
                        if (modifier.Value != 0 && System.Array.IndexOf(effects, modifier.Effect) >= 0 && RunModifierRules.Applies(modifier.Scope, target))
                        {
                            sources += item.Name + " ×" + item.EffectiveStacks(stack.Count) + " / " + RunModifierRules.ScopeName(modifier.Scope) + " / adds " + (modifier.Value * item.EffectiveStacks(stack.Count)).ToString("+0.###;-0.###;0") + "\n";
                            sourceCount++;
                        }
                }
                foreach (BondDefinition bond in catalog.Bonds)
                    if (System.Array.IndexOf(effects, bond.Effect) >= 0 && RunModifierRules.Applies(RunModifierRules.DefaultScope(bond.Effect), target) && session.BondCounts.TryGetValue(bond.Id, out int count) && count >= bond.Threshold)
                    {
                        sources += bond.Name + " / " + count + " companions / level " + bond.ActiveLevel(count) + " / adds " + bond.Reward(count).ToString("+0.###;-0.###;0") + "\n"; sourceCount++;
                    }
                row.Explanation += sources + (sourceCount == 0 ? "No additional sources for this stat.\n" : "Sources are shown before caps, tiers and passives; applied values appear in the stat table.\n");
            }
            return model;
        }

        private static ItemEffect[] SourceEffects(string name)
        {
            switch (name)
            {
                case "Champion health": return new[] { ItemEffect.Health };
                case "Champion damage": return new[] { ItemEffect.Damage, ItemEffect.ChampionDamage };
                case "Champion attack cycle": return new[] { ItemEffect.AttackSpeed };
                case "Movement speed": return new[] { ItemEffect.MoveSpeed };
                case "Champion critical chance": return new[] { ItemEffect.Crit };
                case "Champion projectile count": return new[] { ItemEffect.Projectiles };
                case "Champion armor": return new[] { ItemEffect.Armor };
                case "Passive regeneration": return new[] { ItemEffect.Heal };
                case "Pickup radius": return new[] { ItemEffect.Pickup };
                case "Companion roaming range": return new[] { ItemEffect.Roam };
                case "Companion bullet interception": return new[] { ItemEffect.Interceptor };
                case "Next shop interest": return new[] { ItemEffect.Interest };
                case "Luck": return new[] { ItemEffect.Luck };
                case "Treasure detection": return new[] { ItemEffect.TreasureDetection };
                default: return new ItemEffect[0];
            }
        }

        private void Stat(string name, string value, string explanation) { Stats.Add(new Row { Name = name, Summary = value, Explanation = explanation }); }
        private static string Percent(float value) => (value * 100f).ToString("0.#") + "%";
        public static string EffectName(ItemEffect effect)
        {
            switch (effect)
            {
                case ItemEffect.Damage: return "Team damage";
                case ItemEffect.ChampionDamage: return "Champion damage";
                case ItemEffect.CompanionDamage: return "Companion damage";
                case ItemEffect.AttackSpeed: return "Team attack speed";
                case ItemEffect.CompanionAttackSpeed: return "Companion attack speed";
                case ItemEffect.Health: return "Champion maximum health";
                case ItemEffect.MoveSpeed: return "Team movement speed";
                case ItemEffect.Pickup: return "Pickup radius";
                case ItemEffect.Roam: return "Companion roaming range";
                case ItemEffect.Interceptor: return "Companion bullet interception";
                case ItemEffect.Heal: return "Champion regeneration";
                case ItemEffect.Crit: return "Team critical chance";
                case ItemEffect.Projectiles: return "Team bonus projectiles";
                case ItemEffect.Interest: return "Gold interest cap";
                case ItemEffect.Armor: return "Champion armor";
                case ItemEffect.Discount: return "Shop discount";
                case ItemEffect.Luck: return "Luck";
                case ItemEffect.TreasureDetection: return "Treasure detection";
                default: return RunModifierRules.ModifierName(effect);
            }
        }
    }
}
