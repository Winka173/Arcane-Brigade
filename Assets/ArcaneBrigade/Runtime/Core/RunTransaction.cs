using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    [Serializable]
    public sealed class RunTransactionReceipt
    {
        public const int HistoryLimit = 8;
        public int Id;
        public int Hall;
        public string Title;
        public List<string> Changes = new List<string>();
        public string Description => "Hall " + Hall + " · #" + Id + "\n" + string.Join("\n", Changes);
    }

    public static class RunTransaction
    {
        public static bool TryCommit(RunSession session, RunCatalog catalog, string title, Func<bool> operation, Func<bool> persist, out string error)
        {
            error = "";
            RunSnapshot before = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(session.State));
            try
            {
                if (before.Phase != SessionPhase.Shop) return false;
                if (before.NextReceipt == int.MaxValue) throw new InvalidOperationException("Transaction counter exhausted");
                RunBuildReadModel previous = RunBuildReadModel.Capture(session, catalog, before.Health);
                if (!operation()) { session.Restore(before); return false; }
                session.Validate(session.State);
                RunBuildReadModel current = RunBuildReadModel.Capture(session, catalog, session.State.Health);
                var receipt = new RunTransactionReceipt { Id = ++session.State.NextReceipt, Hall = session.State.Wave, Title = title };
                Change(receipt, "Gold", before.Gold.ToString(), session.State.Gold.ToString());
                Change(receipt, "Eggs", before.Eggs.ToString(), session.State.Eggs.ToString());
                Change(receipt, "Current HP", before.Health.ToString("0.###"), session.State.Health.ToString("0.###"));
                Change(receipt, "Shop reroll", before.RerollsThisShop.ToString(), session.State.RerollsThisShop.ToString());
                RosterChanges(receipt, before.Roster, session.State.Roster, catalog);
                for (int index = 0; index < before.Roster.Count; index++)
                {
                    int currentIndex = session.State.Roster.FindIndex(entry => Same(before.Roster[index], entry));
                    if (currentIndex >= 0)
                        foreach (KeyValuePair<string, string> stat in previous.Companions[index].CombatValues)
                            Change(receipt, previous.Companions[index].Name + " / " + before.Roster[index].WeaponId + " / " + stat.Key, stat.Value, current.Companions[currentIndex].CombatValues[stat.Key]);
                }
                var itemIds = new HashSet<string>();
                foreach (ItemStack stack in before.Items) itemIds.Add(stack.Id);
                foreach (ItemStack stack in session.State.Items) itemIds.Add(stack.Id);
                var orderedIds = new List<string>(itemIds);
                orderedIds.Sort(StringComparer.Ordinal);
                foreach (string id in orderedIds)
                    Change(receipt, catalog.Item(id).Name + " ×", Count(before.Items, id).ToString(), Count(session.State.Items, id).ToString());
                foreach (var row in previous.Stats)
                { var next = current.Stats.Find(r => r.Name == row.Name); if (next != null) Change(receipt,row.Name,row.Summary,next.Summary); }
                foreach(var delta in ShopStatSnapshot.Compare(previous.ShopStats,current.ShopStats))receipt.Changes.Add(delta.ActorName+" / "+delta.Caption);
                if (previous.Bonds != current.Bonds) receipt.Changes.Add("Bonds before:\n" + previous.Bonds + "Bonds sau:\n" + current.Bonds);
                session.State.Receipts.Add(receipt);
                while (session.State.Receipts.Count > RunTransactionReceipt.HistoryLimit) session.State.Receipts.RemoveAt(0);
                session.Validate(session.State);
                if (!persist()) { session.Restore(before); return false; }
                session.PublishShopStatChanges(previous.ShopStats,current.ShopStats);
                return true;
            }
            catch (Exception exception)
            {
                session.Restore(before);
                error = "Transaction failed: " + exception.Message;
                return false;
            }
        }

        private static int Count(List<ItemStack> stacks, string id) => stacks.Find(stack => stack.Id == id)?.Count ?? 0;

        private static void Change(RunTransactionReceipt receipt, string label, string before, string after)
        {
            if (before != after) receipt.Changes.Add(label + ": " + before + " → " + after);
        }

        private static void RosterChanges(RunTransactionReceipt receipt, List<RosterEntry> before, List<RosterEntry> after, RunCatalog catalog)
        {
            foreach (RosterEntry entry in before)
                if (!after.Exists(candidate => Same(entry, candidate))) receipt.Changes.Add("− " + RosterLabel(entry, catalog));
            foreach (RosterEntry entry in after)
                if (!before.Exists(candidate => Same(entry, candidate))) receipt.Changes.Add("+ " + RosterLabel(entry, catalog));
        }

        private static bool Same(RosterEntry first, RosterEntry second) => first.UnitId == second.UnitId && first.WeaponId == second.WeaponId && first.Tier == second.Tier;
        private static string RosterLabel(RosterEntry entry, RunCatalog catalog) => catalog.Unit(entry.UnitId).DisplayName + " · " + entry.WeaponId + " · tier " + entry.Tier;
    }
}
