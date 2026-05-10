using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class RunBuildReadModel
    {
        public sealed class Row { public string Name; public string Summary; public string Explanation; public Dictionary<string, string> CombatValues; public Sprite Icon; public Sprite WeaponIcon; public Color Accent; public string UnitId; public ItemRarity? Rarity; }
        public readonly List<Row> Stats = new List<Row>();
        public readonly List<Row> Companions = new List<Row>();
        public readonly List<Row> Relics = new List<Row>();
        public readonly List<Row> Links = new List<Row>();
        public string Bonds;
        public string Receipts;
        public string BuildLinks;
        public ShopStatSnapshot ShopStats;

        public static RunBuildReadModel Capture(RunSession session, RunCatalog catalog, float health, CombatSimulation simulation = null)
        {
            var model = new RunBuildReadModel();
            using var readSession = new RunSession(catalog);
            readSession.Restore(session.State);
            CheckpointCatalog compiled = readSession.PrepareCombat();
            UnitDefinition champion = catalog.Unit(session.State.ChampionId);
            WeaponDefinition weapon = champion.Weapon;
            model.ShopStats = ShopStatSnapshot.Capture(readSession, catalog, compiled);
            model.Stat("Hero health", health.ToString("0.0") + " / " + compiled.Champion.Health.ToString("0.0"), "Maximum health includes base HP, run bonuses and health correction, minimum 1. The Waiting Room's Lantern adds a little. Every cleared wave restores full HP.");
            model.Stat("Hero damage", compiled.Champion.Weapon.Damage.ToString("0.0"), "Weapon base " + FriendlyWeaponBalance.BaseDamage(weapon).ToString("0.0") + "; hero power x" + PlayerBuildBalance.HeroPower(champion.Id).ToString("0.0") + "; includes melee balance, damage bonuses and roster-based passive. Excludes critical hits, target armor and elemental reactions.");
            model.Stat("Hero attack cycle", compiled.Champion.Weapon.Cooldown.ToString("0.00") + " s", "Base cycle " + weapon.Cooldown.ToString("0.00") + " seconds; every attack is automatic.");
            model.Stat("Movement speed", compiled.Champion.Speed.ToString("0.00"), "Base " + champion.Speed.ToString("0.00") + "; bonus " + Percent(session.Bonus(ItemEffect.MoveSpeed, ModifierScope.Champion)) + "; minimum multiplier 0.1.");
            model.Stat("Hero critical chance", Percent(compiled.Champion.CriticalChance), "Base " + Percent(champion.CriticalChance) + "; bonuses add before the 0–100% cap.");
            model.Stat("Hero projectile count", compiled.Champion.Weapon.ProjectileCount.ToString(), "Applies only to projectile weapons; does not turn melee attacks into bullets.");
            model.Stat("Hero armor", compiled.Armor.ToString("0.00"), "Positive armor reduces incoming damage; negative armor increases it, capped before the per-hit limit.");
            model.Stat("Passive regeneration", compiled.PassiveHeal.ToString("0.00") + " / s", "HP per second from items and wave effects. Individual unit healing triggers separately every 4 seconds and is not included here.");
            model.Stat("Pickup radius", compiled.PickupRadius.ToString("0.00"), "Base " + catalog.Arena.PickupRadius.ToString("0.00") + "; arena distance, not screen pixels.");
            model.Stat("Friend roaming range", "Ranged " + compiled.CompanionLeash.ToString("0.00") + " / Melee " + (compiled.CompanionLeash + MeleeBalance.RoamBonus).ToString("0.00"), "Friends patrol around the player automatically. Melee units have wider roaming and faster enemy pursuit; no formation controls required.");
            model.Stat("Friend bullet interception", session.Bonus(ItemEffect.Interceptor) > 0 ? "Enabled by item" : "No", "Friends are immortal but do not block bullets by default.");
            model.Stat("Next shop interest", Mathf.Min(catalog.InterestCap + Mathf.FloorToInt(session.Bonus(ItemEffect.Interest)), session.State.Gold / Mathf.Max(1, catalog.InterestDivisor)).ToString() + " gold", "Forecast from saved gold; excludes uncommitted pickups or next-wave income.");
            model.Stat("Next reroll cost", session.RerollCost + " gold", "Rerolled " + session.State.RerollsThisShop + " times in this shop. Base cost " + catalog.RerollPrice + "; each roll adds " + catalog.RerollPriceStep + ". Coupons discount displayed wares, not reroll fees.");
            model.Stat("Next dream shop", session.SpecialShopEnabled ? "After wave " + session.State.NextSpecialShopWave : "Disabled by this pact",
                "A dream shop opens every 3 or 4 waves. Its schedule is saved; eggs do not speed it up. Opening is free; purchases still use their listed currency.");
            if (catalog.LootRevision > 0)
            {
                model.Stat("Luck", session.Bonus(ItemEffect.Luck).ToString("0.###"), "Increases egg/heart drops and chest rewards. Does not change combat or shop RNG outcomes.");
                model.Stat("Treasure detection", session.Bonus(ItemEffect.TreasureDetection).ToString("0.###"), "Makes chests appear sooner, at least 4 seconds apart. Approach to open; no skill button required.");
            }
            int companionIndex = 0;
            foreach (RosterEntry entry in session.ActiveRoster)
            {
                UnitDefinition unit = catalog.Unit(entry.UnitId);
                WeaponDefinition actual = catalog.Weapon(entry.WeaponId);
                UnitDefinition applied = compiled.StartingCompanions[companionIndex++];
                float damage = applied.Weapon.Damage;
                float cooldown = applied.Weapon.Cooldown;
                int tierSteps = catalog.ContentRevision > 0 ? Mathf.Max(0, entry.Tier - 1) : 0;
                float range = applied.Weapon.Range;
                float passive = unit.Passive == PassiveKind.Scavenge ? session.ScavengeGold(unit, entry.Tier) : unit.PassiveStrength * Mathf.Pow(catalog.TierSupportMultiplier, entry.Tier - 1);
                model.Companions.Add(new Row { Name = session.Stranger(unit.Id) ? "???" : RunUICopy.UnitName(unit), UnitId = unit.Id, Icon = unit.Body, WeaponIcon = actual.Sprite, Summary = "Stars " + entry.Tier + " · " + RunUICopy.WeaponName(actual.Id) + "\n" + RunUICopy.WeaponKindName(actual.Kind) + " / " + RunUICopy.ElementName(actual.Element),
                    CombatValues = new Dictionary<string, string> { { "Damage", damage.ToString("0.###") }, { "Cycle", cooldown.ToString("0.###") + " s" }, { "Crit", Percent(applied.CriticalChance) }, { "Speed", applied.Speed.ToString("0.###") }, { "Projectile", applied.Weapon.ProjectileCount.ToString() }, { "Passive", passive.ToString("0.###") } },
                    Explanation = "Assigned weapon: " + RunUICopy.WeaponName(actual.Id) + "\nDamage: " + damage.ToString("0.0") + " / cycle " + cooldown.ToString("0.00") + " seconds\nRange: " + range.ToString("0.0") + "\n" + (tierSteps > 0 ? actual.Kind == WeaponKind.Projectile ? "Stars bonus: adds " + tierSteps + " projectiles and " + tierSteps + " pierce.\n" : "Stars bonus: increased range, sweep arc and attack speed.\n" : "") + SignatureDescription.Describe(actual, entry.Tier, session.StackCount("reflective-ink") > 0) + "\nImmortal. Each friend body counts once toward bonds." });
                model.Companions[model.Companions.Count - 1].Explanation += "\n\n" + CompanionDescriptions.Describe(unit,entry.Tier,catalog.TierSupportMultiplier,session.GoldScale);
                int savedValue = CompanionDeployment.OwnedValue(session.State.Roster, unit.Id);
                int reserveCopies = session.State.Roster.FindAll(r => r.UnitId == unit.Id).Count - 1;
                model.Companions[model.Companions.Count - 1].Summary += "\nOne fighter · " + reserveCopies + " saved materials · "
                    + (entry.Tier >= 3 ? "maximum stars" : savedValue + "/" + (entry.Tier < 2 ? 2 : 4) + " upgrade value");
                model.Companions[model.Companions.Count - 1].Explanation += "\nSaved materials do not attack, add bond members or provide passive stats. All purchased weapons are kept until used in a merge.";
            }
            foreach (UnitDefinition story in compiled.StoryCompanions ?? new UnitDefinition[0])
            {
                int tier = ActOneCanon.StoryTier(session.State.Wave);
                model.Companions.Add(new Row
                {
                    Name = story.DisplayName, UnitId = story.Id, Icon = story.Body, WeaponIcon = story.Weapon.Sprite,
                    Summary = "Travels with you · " + RunUICopy.WeaponName(story.Weapon.Id) + "\nIn their own place · grows with the road (stars " + tier + ")",
                    CombatValues = new Dictionary<string, string> { { "Damage", story.Weapon.Damage.ToString("0.###") }, { "Cycle", story.Weapon.Cooldown.ToString("0.###") + " s" }, { "Speed", story.Speed.ToString("0.##") } },
                    Explanation = story.DisplayName.ToUpperInvariant() + "\n" + CreatureLives.Arc(story.Id, session.State) + "\n\nWeapon: " + RunUICopy.WeaponName(story.Weapon.Id) + "\nDamage: " + story.Weapon.Damage.ToString("0.0") + " / cycle " + story.Weapon.Cooldown.ToString("0.00") + " seconds\nRange: " + story.Weapon.Range.ToString("0.0")
                        + "\nA travelling friend of the story: never bought, merged or sold, and not counted toward bonds. Their strength follows the zone of the road."
                });
            }
            foreach (Row row in model.Companions) if (row.UnitId != null && session.Stranger(row.UnitId) && row.Name == "???") row.Explanation = "You no longer remember who they are. You sold that memory to the Last Customer. They still walk with you, and they do not count toward bonds.\n\n" + row.Explanation;
            if (catalog.StatRevision > 0) K10BuildReadModel.Append(model, session, catalog, health, compiled);
            BuildMorphReadModel.Append(model, session, catalog, compiled);
            foreach (ItemStack stack in session.State.Items)
            {
                ItemDefinition item = catalog.Item(stack.Id);
                string procDescription = "";
                foreach (ProcRule rule in catalog.ProcRules ?? new ProcRule[0])
                    if (rule.ItemId == item.Id) procDescription += "\nOn " + RunUICopy.TriggerName(rule.Trigger) + " → " + RunUICopy.ActionName(rule.Action) + "\nEffective " + (rule.Power * item.EffectiveStacks(stack.Count)).ToString("0.###") + " · radius " + rule.Radius + " · cooldown " + rule.Cooldown + " seconds · chance " + Percent(rule.Chance) + "\nThis effect does not trigger other item effects.\n";
                model.Relics.Add(new Row { Name = item.Name, Icon = item.Icon, Accent = RunModifierRules.RarityColor(item.Rarity), Rarity = item.Rarity, Summary = "Owned ×" + stack.Count + " · " + RunModifierRules.RarityName(item.Rarity) + "\n" + (item.EffectiveModifiers.Length > 1 ? item.EffectiveModifiers.Length + " effects · includes benefits/tradeoffs" : RunModifierRules.ScopeName(item.EffectiveModifiers[0].Scope)), Explanation = RelicDescriptions.Describe(item, stack.Count, catalog) + "\nRun upgrade. Cannot sell, swap or remove items here." });

                if (!string.IsNullOrEmpty(procDescription)) model.Relics[model.Relics.Count - 1].Summary = "Owned ×" + stack.Count + " · " + RunModifierRules.RarityName(item.Rarity) + "\nAutomatically triggers under its conditions";
                if (session.SecretItemSuspended(stack.Id))
                {
                    model.Relics[model.Relics.Count - 1].Summary = "ALTERED / AWAY · original effects inactive";
                    model.Relics[model.Relics.Count - 1].Explanation = "Original effects below are inactive while this relic is stitched, lent or deposited. See secret agreements in the receipt journal.\n\n" + model.Relics[model.Relics.Count - 1].Explanation;
                }
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
            if (session.State.Secrets != null)
            {
                model.Receipts += "SECRET AGREEMENTS\n";
                foreach (SecretContract agreement in session.State.Secrets.Contracts)
                    model.Receipts += agreement.Kind + " · " + agreement.Remaining + " normal victories left · " + agreement.ItemId + "\n";
                foreach (SecretStitch stitch in session.State.Secrets.Stitches)
                    model.Receipts += "Stitched " + catalog.Item(stitch.ItemId).Name + " · " + new[] { "Recall / no pierce", "Melee aftershock / slower cycle", "Shelter / damage tradeoff" }[stitch.Pattern] + "\n";
                foreach (string entry in session.State.Secrets.Journal) model.Receipts += entry + "\n";
            }
            foreach (Row row in model.Stats)
            {
                ItemEffect[] effects = SourceEffects(row.Name);
                if (effects.Length == 0) continue;
                ModifierScope target = effects[0] == ItemEffect.Luck || effects[0] == ItemEffect.TreasureDetection || effects[0] == ItemEffect.Pickup || effects[0] == ItemEffect.Roam || effects[0] == ItemEffect.Interest || effects[0] == ItemEffect.Interceptor ? ModifierScope.Run : ModifierScope.Champion;
                string sources = "\n\nRUN MODIFIER SOURCES\n";
                int sourceCount = 0;
                foreach (RosterEntry entry in session.ActiveRoster)
                    foreach (StatModifier modifier in catalog.Unit(entry.UnitId).CompanionModifiers ?? new StatModifier[0])
                        if (modifier.Value != 0 && System.Array.IndexOf(effects, modifier.Effect) >= 0 && RunModifierRules.Applies(modifier.Scope, target))
                        {
                            sources += catalog.Unit(entry.UnitId).DisplayName + " / stars " + entry.Tier + " / " + RunModifierRules.ScopeName(modifier.Scope) + " / adds " + (modifier.Value * UnitDefinition.ModifierScale(entry.Tier)).ToString("+0.###;-0.###;0") + "\n";
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
                    if (session.SecretItemSuspended(stack.Id)) continue;
                    ItemDefinition item = catalog.Item(stack.Id);
                    foreach (StatModifier modifier in item.EffectiveModifiers)
                        if (modifier.Value != 0 && System.Array.IndexOf(effects, modifier.Effect) >= 0 && RunModifierRules.Applies(modifier.Scope, target))
                        {
                            sources += item.Name + " ×" + item.EffectiveStacks(stack.Count) + " / " + RunModifierRules.ScopeName(modifier.Scope) + " / adds " + (modifier.Value * item.EffectiveStacks(stack.Count) * (modifier.Value > 0 && session.SecretItemImproved(stack.Id) ? 1.25f : 1)).ToString("+0.###;-0.###;0") + "\n";
                            sourceCount++;
                        }
                }
                foreach (BondDefinition bond in catalog.Bonds)
                    if (System.Array.IndexOf(effects, bond.Effect) >= 0 && RunModifierRules.Applies(RunModifierRules.DefaultScope(bond.Effect), target) && session.BondCounts.TryGetValue(bond.Id, out int count) && count >= bond.Threshold)
                    {
                        sources += bond.Name + " / " + count + " friends / level " + bond.ActiveLevel(count) + " / adds " + bond.Reward(count).ToString("+0.###;-0.###;0") + "\n"; sourceCount++;
                    }
                row.Explanation += sources + (sourceCount == 0 ? "No additional sources for this stat.\n" : "Sources are shown before limits, stars and passives; applied values appear in the stat table.\n");
            }
            if (HeroIdentities.For(champion.Id) != null) model.Stats.Insert(0,new Row { Name = "Hero / " + HeroIdentities.For(champion.Id).Name,
                Summary = simulation != null && simulation.HeroIdentityEnabled ? simulation.HeroIdentityStatus : session.HeroIdentityShopSummary,
                Explanation = HeroIdentities.Describe(champion.Id), Icon = champion.Body });
            return model;
        }

        private static ItemEffect[] SourceEffects(string name)
        {
            switch (name)
            {
                case "Hero health": return new[] { ItemEffect.Health };
                case "Hero damage": return new[] { ItemEffect.Damage, ItemEffect.ChampionDamage };
                case "Hero attack cycle": return new[] { ItemEffect.AttackSpeed };
                case "Movement speed": return new[] { ItemEffect.MoveSpeed };
                case "Hero critical chance": return new[] { ItemEffect.Crit };
                case "Hero projectile count": return new[] { ItemEffect.Projectiles };
                case "Hero armor": return new[] { ItemEffect.Armor };
                case "Passive regeneration": return new[] { ItemEffect.Heal };
                case "Pickup radius": return new[] { ItemEffect.Pickup };
                case "Friend roaming range": return new[] { ItemEffect.Roam };
                case "Friend bullet interception": return new[] { ItemEffect.Interceptor };
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
                case ItemEffect.Damage: return "Damage";
                case ItemEffect.ChampionDamage: return "Hero damage";
                case ItemEffect.CompanionDamage: return "Friend damage";
                case ItemEffect.AttackSpeed: return "Attack speed";
                case ItemEffect.CompanionAttackSpeed: return "Friend attack speed";
                case ItemEffect.Health: return "Max HP";
                case ItemEffect.MoveSpeed: return "Move speed";
                case ItemEffect.Pickup: return "Pickup range";
                case ItemEffect.Roam: return "Friend roam range";
                case ItemEffect.Interceptor: return "Friends block shots";
                case ItemEffect.Heal: return "HP regen per second";
                case ItemEffect.Crit: return "Crit chance";
                case ItemEffect.Projectiles: return "Extra shots";
                case ItemEffect.Interest: return "Max interest";
                case ItemEffect.Armor: return "Armor";
                case ItemEffect.Discount: return "Shop discount";
                case ItemEffect.Luck: return "Luck";
                case ItemEffect.TreasureDetection: return "Chest finding";
                default: return RunModifierRules.ModifierName(effect);
            }
        }
    }
}
