using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Roster stores owned merge ingredients; only the highest-star entry of each species is deployed.
    /// Equal-star ties keep the first owned weapon, so buying an ingredient never silently swaps the fighter.</summary>
    public static class CompanionDeployment
    {
        /// <summary>requirement 27/10/2026 (stars came too easily): three copies of a friend at the same star make one with a new
        /// star; with two of the three it shows half a star more (1.5, 2.5).</summary>
        public const int MergeCopies = 3;
        public static List<RosterEntry> Select(IReadOnlyList<RosterEntry> owned)
        {
            var result = new List<RosterEntry>();
            var indices = new Dictionary<string, int>(StringComparer.Ordinal);
            if (owned == null) return result;
            foreach (RosterEntry entry in owned)
            {
                if (!indices.TryGetValue(entry.UnitId, out int index))
                { if(result.Count>=RulesV3Tuning.CompanionLimit)continue;indices.Add(entry.UnitId, result.Count); result.Add(entry); }
                else if (entry.Tier > result[index].Tier) result[index] = entry;
            }
            return result;
        }

        public static int MaterialValue(int tier) => tier >= 5 ? 81 : tier == 4 ? 27 : tier == 3 ? 9 : tier == 2 ? 3 : 1;
        /// <summary>The highest star the shop's copies reach (4, purple); the fifth, rainbow, only comes from a Rainbow Thread.</summary>
        public const int ShopMaxTier = 4;
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
                foreach (string id in ids) MergeCompanions(id);
                WeaveRainbow();
            }
            finally { resolvingStoredCopies = false; }
        }

        private bool MergeReady(string id)
        {
            if (HighestOwnedTier(id) >= CompanionDeployment.ShopMaxTier || catalog.Unit(id).ReferenceCompanion == "Evil") return false;
            for (int tier = 1; tier < CompanionDeployment.ShopMaxTier; tier++)
                if (State.Roster.FindAll(entry => entry.UnitId == id && entry.Tier == tier).Count >= CompanionDeployment.MergeCopies - 1) return true;
            return false;
        }

        /// <summary>The stars a friend shows: its highest star, plus a half when two of the three copies for the next star are owned.</summary>
        public float StarProgress(string id)
        {
            int top = HighestOwnedTier(id);
            if (top <= 0) return 0;
            if (top >= CompanionDeployment.ShopMaxTier || catalog.Unit(id)?.ReferenceCompanion == "Evil") return top;
            int copies = State.Roster.FindAll(entry => entry.UnitId == id && entry.Tier == top).Count;
            return top + (copies >= CompanionDeployment.MergeCopies - 1 ? .5f : 0);
        }
        /// <summary>True when this friend's top star has two of its three copies (it shows a half star more and hits a little harder).</summary>
        public bool HalfStar(string id, int tier) => tier == HighestOwnedTier(id) && tier < CompanionDeployment.ShopMaxTier && catalog.Unit(id)?.ReferenceCompanion != "Evil"
            && State.Roster.FindAll(entry => entry.UnitId == id && entry.Tier == tier).Count == CompanionDeployment.MergeCopies - 1;
        public static string StarText(float stars) => Mathf.Approximately(stars, Mathf.Floor(stars)) ? ((int)stars).ToString() : stars.ToString("0.0");

        public string UpgradeProgress(ShopOffer offer)
        {
            if (offer == null || offer.Kind != OfferKind.Companion) return "";
            int before = CompanionDeployment.OwnedValue(State.Roster, offer.Id);
            int added = CompanionDeployment.MaterialValue(offer.Tier);
            int after = RunSession.BoundedCounter((long)before + added);
            int target = before < 3 ? 3 : before < 9 ? 9 : 27;
            // in plain words (requirement 25/10/2026: "upgrade material", "extra fighter" told nothing)
            if (OctoberAbsorbs) return "Your hero takes this friend's skill.";
            if (HighestOwnedTier(offer.Id) >= CompanionDeployment.ShopMaxTier) return "Already at max stars.";
            return before == 0 ? "New friend."
                : "Copy " + after + " of " + target + " for " + (target == 3 ? "2 stars" : target == 9 ? "3 stars" : "4 stars") + (after >= target ? " · stars up now!" : ".");
        }
    }
}
