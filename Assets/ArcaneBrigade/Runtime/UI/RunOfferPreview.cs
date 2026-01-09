namespace ArcaneBrigade
{
    public static class RunOfferPreview
    {
        public static string Capture(RunSession session, RunCatalog catalog, ShopOffer offer)
        {
            if (!session.CanBuy(offer)) return "Cannot purchase: check currency/HP, stack limit or weapon variants.";
            if (offer.Kind == OfferKind.Companion) return "Weapons roll after purchase. Three matching species/tiers merge; each body counts once for bonds. No preview pretends a random weapon is already assigned.\n" + RunModifierRules.DescribeCompanion(catalog.Unit(offer.Id), 1);
            RunBuildReadModel before = RunBuildReadModel.Capture(session, catalog, session.State.Health);
            using (var preview = new RunSession(catalog))
            {
                preview.Restore(session.State);
                if (!preview.Buy(offer.Token)) return "Offer is no longer valid.";
                RunBuildReadModel after = RunBuildReadModel.Capture(preview, catalog, preview.State.Health);
                string result = "IF PURCHASED · actual data\n";
                for (int index = 0; index < before.Stats.Count; index++)
                    if (before.Stats[index].Summary != after.Stats[index].Summary) result += before.Stats[index].Name + ": " + before.Stats[index].Summary + " → " + after.Stats[index].Summary + "\n";
                for (int index = 0; index < before.Companions.Count; index++)
                    foreach (var stat in before.Companions[index].CombatValues)
                        if (stat.Value != after.Companions[index].CombatValues[stat.Key]) result += before.Companions[index].Name + " / " + stat.Key + ": " + stat.Value + " → " + after.Companions[index].CombatValues[stat.Key] + "\n";
                foreach (ProcRule rule in catalog.ProcRules ?? new ProcRule[0])
                    if (rule.ItemId == offer.Id) result += "Proc " + rule.Trigger + " → " + rule.Action + " · power " + rule.Power * preview.StackCount(offer.Id) + " · CD " + rule.Cooldown + "s\n";
                return result;
            }
        }
    }
}
