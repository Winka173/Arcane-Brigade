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
                { indices.Add(entry.UnitId, result.Count); result.Add(entry); }
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
        private List<RosterEntry> compilingRoster;
        public List<RosterEntry> ActiveRoster => compilingRoster ?? CompanionDeployment.Select(State.Roster);
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
            if (!CanStarUp(id)) return false;
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

        /// <summary>The star a bought offer joins at: a copy counts at the owned friend's star; a new friend at its own.</summary>
        public int CopyTier(ShopOffer offer) => offer == null ? 1 : offer.Kind == OfferKind.Companion && OwnsCompanionSpecies(offer.Id) ? Mathf.Max(1, HighestOwnedTier(offer.Id)) : Mathf.Clamp(offer.Tier, 1, 3);
        /// <summary>The friend's top star once this offer is bought (1-1.5-2-...-3.5-4: three at a star make the next; 4 is the most a copy reaches).</summary>
        public int ProjectTier(ShopOffer offer)
        {
            if (offer == null) return 1;
            int top = HighestOwnedTier(offer.Id);
            if (top <= 0) return CopyTier(offer);
            if (!CanStarUp(offer.Id)) return top;
            int copies = State.Roster.FindAll(entry => entry.UnitId == offer.Id && entry.Tier == top).Count + 1;
            return copies >= CompanionDeployment.MergeCopies ? top + 1 : top;
        }
        /// <summary>The star a friend joins at: a copy of an owned friend joins at that friend's star (each copy is half a star),
        /// a new one at its own (requirement 31/10/2026). Used by gifts, visitors and kits as well as the shop.</summary>
        public int JoinTier(string id, int tier) => OwnsCompanionSpecies(id) ? Mathf.Max(tier, Mathf.Min(HighestOwnedTier(id), CompanionDeployment.ShopMaxTier)) : tier;
        public bool StarsUpOnBuy(ShopOffer offer) => offer != null && offer.Kind == OfferKind.Companion && OwnsCompanionSpecies(offer.Id) && ProjectTier(offer) > HighestOwnedTier(offer.Id);

        public string UpgradeProgress(ShopOffer offer)
        {
            if (offer == null || offer.Kind != OfferKind.Companion) return "";
            // in plain words (requirement 25/10/2026: "upgrade material", "extra fighter" told nothing)
            if (OctoberAbsorbs) return "Your hero takes this friend's skill.";
            if (!OwnsCompanionSpecies(offer.Id)) return "New friend.";
            if (!CanStarUp(offer.Id)) return "Already at max stars.";
            float now = StarProgress(offer.Id), next = now + .5f;
            return "Stars " + StarText(now) + " → " + StarText(next) + (StarsUpOnBuy(offer) ? " · stars up now!" : ".");
        }
    }
}
