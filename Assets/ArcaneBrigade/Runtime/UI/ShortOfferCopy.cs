using System.Linq;

namespace ArcaneBrigade
{
    public static class ShortOfferCopy
    {
        private static string Modifiers(StatModifier[] modifiers) => string.Join(" · ", modifiers.Where(modifier => modifier.Value != 0).Select(modifier => RunModifierRules.ScopeName(modifier.Scope) + " " + RunModifierRules.ModifierName(modifier.Effect) + " " + RunModifierRules.ValueText(modifier.Effect, modifier.Value)));
        public static string Companion(UnitDefinition unit) => (unit.SpecialRecruit ? "Dream elite · " : "") + Modifiers(unit.CompanionModifiers) + "\n" + RunUICopy.PassiveDescription(unit.Passive);
        public static string BondPreview(RunSession session, RunCatalog catalog, ShopOffer offer)
        {
            if (offer == null || offer.Kind != OfferKind.Companion || !session.CanBuy(offer)) return "";
            using var preview = new RunSession(catalog); preview.Restore(session.State);
            if (!preview.Buy(offer.Token)) return "";
            return string.Join(" · ", catalog.Bonds.Where(bond =>
            {
                int before = session.BondCounts.TryGetValue(bond.Id, out int first) ? first : 0;
                int after = preview.BondCounts.TryGetValue(bond.Id, out int next) ? next : 0;
                return bond.Reward(after) > bond.Reward(before);
            }).Select(bond => "BOND UP: " + bond.Name));
        }
        public static string Item(ItemDefinition item) => RunModifierRules.RarityName(item.Rarity) + " · " + (item.Stacking == StackPolicy.Unique ? "Unique" : "Stacks") + "\n" + Modifiers(item.EffectiveModifiers) + (item.SignatureRole == SignatureRelicKind.None ? item.Id == "velvet-first-aid" ? "\nChest: heal4 HP/stack · 2s CD" : item.Id == "brass-avalanche" ? "\nKill: nearby pulse3 DMG/stack · 2s CD" : "" : "\n" + Signature(item.SignatureRole));
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
