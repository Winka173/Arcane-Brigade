namespace ArcaneBrigade
{
    public static class RunOfferPreview
    {
        public static string Capture(RunSession session, RunCatalog catalog, ShopOffer offer)
        {
            if (!session.CanBuy(offer)) return "Không thể mua: kiểm tiền/HP, stack limit hoặc weapon variant.";
            if (offer.Kind == OfferKind.Companion) return "Vũ khí roll sau khi mua. Ba cùng loại/cấp sẽ merge; một body tính một bond. Không preview một weapon như đã chốt.";
            RunBuildReadModel before = RunBuildReadModel.Capture(session, catalog, session.State.Health);
            using (var preview = new RunSession(catalog))
            {
                preview.Restore(session.State);
                if (!preview.Buy(offer.Token)) return "Offer không còn hợp lệ.";
                RunBuildReadModel after = RunBuildReadModel.Capture(preview, catalog, preview.State.Health);
                string result = "NẾU MUA · dữ liệu thật\n";
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
