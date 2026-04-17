using System.Linq;

namespace ArcaneBrigade
{
    public static class ShortOfferCopy
    {
        public static string PurchaseDelta(RunSession session, RunCatalog catalog, ShopOffer offer)
        {
            if (offer.Kind == OfferKind.Companion)
            {
                UnitDefinition unit = catalog.Unit(offer.Id);
                var tiers = new int[catalog.MaximumTier + 1];
                foreach (RosterEntry entry in session.State.Roster) if (entry.UnitId == unit.Id) tiers[entry.Tier]++;
                int beforeTier = 0; for (int tier = 1; tier < tiers.Length; tier++) if (tiers[tier] > 0) beforeTier = tier;
                float before = beforeTier > 0 ? UnitDefinition.ModifierScale(beforeTier) : 0;
                int projected = UnityEngine.Mathf.Clamp(offer.Tier, 1, 3); tiers[projected]++;
                for (int tier = 1; tier < catalog.MaximumTier && unit.ReferenceCompanion != "Evil"; tier++)
                    if (tiers[tier] >= CompanionDeployment.MergeCopies) { tiers[tier + 1] += tiers[tier] / CompanionDeployment.MergeCopies; tiers[tier] %= CompanionDeployment.MergeCopies; projected = tier + 1; }
                int afterTier = 0; for (int tier = 1; tier < tiers.Length; tier++) if (tiers[tier] > 0) afterTier = tier;
                float after = afterTier > 0 ? UnitDefinition.ModifierScale(afterTier) : 0;
                string result = session.UpgradeProgress(offer) + (afterTier > beforeTier && beforeTier > 0 ? "\nUpgrades the fielded friend to " + afterTier + " stars." : "");
                StatModifier[] modifiers = unit.CompanionModifiers;
                string stats = string.Join(". ", modifiers.Where(value => value.Value * (after - before) != 0).Select(value => ScopeLabel(value.Scope) + StatName(value.Effect) + " " + RunModifierRules.ValueText(value.Effect, value.Value * (after - before))));
                return result + (stats.Length == 0 ? "" : "\n" + stats) + "\n" + CompanionDescriptions.Describe(unit, offer.Tier, catalog.TierSupportMultiplier);
            }
            ItemDefinition item = catalog.Item(offer.Id);
            return RelicDescriptions.Describe(item, session.StackCount(item.Id) + 1, catalog) + RelicDescriptions.PurchaseReactions(session, catalog, item);
        }
        private static string LegacyItemDelta(RunSession session, ItemDefinition item)
        {
            int held = session.State.Items.Find(stack => stack.Id == item.Id)?.Count ?? 0;
            float delta = item.EffectiveStacks(held + 1) - item.EffectiveStacks(held);
            string change = string.Join(". ", item.EffectiveModifiers.Where(modifier => modifier.Value != 0).Take(3).Select(modifier => ScopeLabel(modifier.Scope) + StatName(modifier.Effect) + " " + RunModifierRules.ValueText(modifier.Effect, modifier.Value * delta)));
            string passive = item.SignatureRole != SignatureRelicKind.None ? Signature(item.SignatureRole) : item.Id == "velvet-first-aid" ? "Chests heal 4 HP per stack" : item.Id == "brass-avalanche" ? "Kills trigger a nearby damage pulse" : "Adds a passive effect";
            return (change.Length > 0 ? change : passive) + "\nOwned: " + held + " to " + (held + 1);
        }
        private static string ScopeLabel(ModifierScope scope) => RunModifierRules.Who(scope);
        public static string StatName(ItemEffect effect) => RunModifierRules.ModifierName(effect); // one name per stat everywhere
        private static string Modifiers(StatModifier[] modifiers) => string.Join(". ", modifiers.Where(modifier => modifier.Value != 0).GroupBy(modifier => modifier.Scope).Select(group => (group.Key == ModifierScope.Champion ? "Your " : group.Key == ModifierScope.Companions ? "Friends: " : group.Key == ModifierScope.Team ? "Team: " : "") + string.Join(", ", group.Select(modifier => RunModifierRules.ModifierName(modifier.Effect) + " " + RunModifierRules.ValueText(modifier.Effect, modifier.Value)))));
        public static string Companion(UnitDefinition unit) => CompanionDescriptions.Describe(unit);
        private static string LegacyCompanion(UnitDefinition unit) => (unit.Rarity == ItemRarity.Common ? "" : RunModifierRules.RarityName(unit.Rarity) + " friend. Attacks deal " + ((int)unit.Rarity * 18) + "% more damage.\n") + Modifiers(unit.CompanionModifiers) + "\n" + (unit.Passive switch
        {
            PassiveKind.HealChampion => "Heal you every 4s", PassiveKind.ChillAura => "Slow nearby foes",
            PassiveKind.Rally => "More friends increase attack", PassiveKind.Scavenge => "Extra loot from kills", _ => "Auto attacks"
        });
        public static string BondPreview(RunSession session, RunCatalog catalog, ShopOffer offer)
        {
            if (offer == null || offer.Kind != OfferKind.Companion || !session.CanBuy(offer)) return "";
            using var preview = new RunSession(catalog); preview.Restore(session.State);
            if (!preview.Buy(offer.Token)) return "";
            return string.Join(" · ", catalog.Bonds.Where(bond =>
            {
                int before = session.BondCounts.TryGetValue(bond.Id, out int first) ? first : 0;
                int after = preview.BondCounts.TryGetValue(bond.Id, out int next) ? next : 0;
                return bond.Reward(after) != bond.Reward(before);
            }).Select(bond =>
            {
                int before = session.BondCounts.TryGetValue(bond.Id, out int first) ? first : 0;
                int after = preview.BondCounts.TryGetValue(bond.Id, out int next) ? next : 0;
                return (bond.Reward(after) > bond.Reward(before) ? "BOND UP: " : "BOND DOWN: ") + bond.Name;
            }));
        }
        public static string Item(ItemDefinition item) => RelicDescriptions.Describe(item);
        private static string LegacyItem(ItemDefinition item) => RunModifierRules.RarityName(item.Rarity) + " · " + (item.Stacking == StackPolicy.Unique ? "Unique" : "Stacks") + "\n" + Modifiers(item.EffectiveModifiers) + (item.SignatureRole == SignatureRelicKind.None ? item.Id == "velvet-first-aid" ? "\nChest: heal4 HP/stack · 2s CD" : item.Id == "brass-avalanche" ? "\nKill: nearby pulse3 DMG/stack · 2s CD" : "" : "\n" + Signature(item.SignatureRole));
        private static string Signature(SignatureRelicKind role) => role switch
        {
            SignatureRelicKind.HarvestReceipt => "Chest harvest bonus",
            SignatureRelicKind.ReflectiveInk => "Reflected attacks empower marks",
            SignatureRelicKind.CarbonRibbon => "Spread marks; expose enemies",
            SignatureRelicKind.OfficeLens => "Critical hits punish exposed targets",
            SignatureRelicKind.TeaCoaster => "Wider, weaker tea trails",
            SignatureRelicKind.ScratchRecord => "One delayed musical echo",
            SignatureRelicKind.TreasureRibbon => "Chest opening empowers bows",
            SignatureRelicKind.PatchSpring => "Repair connects signature effects",
            SignatureRelicKind.ThermalInk => "Connect active hot trails",
            _ => "Repair synergy"
        };
    }
}
