using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Roster stores owned merge ingredients; only the highest-star entry of each species is deployed.
    /// Equal-star ties keep the first owned weapon, so buying an ingredient never silently swaps the fighter.</summary>
    public static class CompanionDeployment
    {
        public static List<RosterEntry> Select(IReadOnlyList<RosterEntry> owned)
        {
            var result = new List<RosterEntry>();
            var indices = new Dictionary<string, int>(StringComparer.Ordinal);
            if (owned == null) return result;
            foreach (RosterEntry entry in owned)
            {
                if (!indices.TryGetValue(entry.UnitId, out int index))
                { indices.Add(entry.UnitId, result.Count); result.Add(entry); }
                else if (entry.Tier > result[index].Tier) result[index] = entry;
            }
            return result;
        }

        public static int MaterialValue(int tier) => tier >= 3 ? 9 : tier == 2 ? 3 : 1;
        public static int OwnedValue(IReadOnlyList<RosterEntry> owned, string id)
        {
            long value = 0;
            foreach (RosterEntry entry in owned)
                if (entry.UnitId == id) value += MaterialValue(entry.Tier);
            return RunSession.BoundedCounter(value);
        }
    }

    public sealed partial class RunSession
    {
        public List<RosterEntry> ActiveRoster => CompanionDeployment.Select(State.Roster);
        public bool AbsorbsRecruitCopies => OctoberAbsorbs;
        private bool resolvingStoredCopies;

        private void ResolveStoredCopies()
        {
            if (resolvingStoredCopies) return;
            resolvingStoredCopies = true;
            try
            {
                // Also handles gifts/altar products and old saves; a chain may produce another species (the nursery).
                var ids = new List<string>();
                foreach (RosterEntry entry in State.Roster) if (!ids.Contains(entry.UnitId)) ids.Add(entry.UnitId);
                foreach (string id in ids) MergeTriples(id);
            }
            finally { resolvingStoredCopies = false; }
        }

        private bool MergeReady(string id)
        {
            if (HighestOwnedTier(id) >= catalog.MaximumTier || catalog.Unit(id).ReferenceCompanion == "Evil") return false;
            for (int tier = 1; tier < catalog.MaximumTier; tier++)
                if (State.Roster.FindAll(entry => entry.UnitId == id && entry.Tier == tier).Count >= 2) return true;
            return false;
        }

        public string UpgradeProgress(ShopOffer offer)
        {
            if (offer == null || offer.Kind != OfferKind.Companion) return "";
            int before = CompanionDeployment.OwnedValue(State.Roster, offer.Id);
            int added = CompanionDeployment.MaterialValue(offer.Tier);
            int after = RunSession.BoundedCounter((long)before + added);
            int target = before < 3 ? 3 : 9;
            if (OctoberAbsorbs) return "Absorb this friend's skill; no extra fighter.";
            if (before >= 9) return "Maximum stars · one fighter. Extra materials remain saved.";
            return before == 0 ? "New friend · one fighter of this species."
                : "Upgrade material · " + before + "/" + target + " → " + after + "/" + target
                    + " · no extra fighter.";
        }
    }
}
