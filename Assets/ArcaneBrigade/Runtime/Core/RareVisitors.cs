using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public static class RareVisitorRules
    {
        // One exclusive roll per completed wave's shop. Ineligible ranges stay empty, never boost another gift.
        public const int RollSize = 10000, TwoStarChance = 400, ThreeStarChance = 75, RelicChance = 225;
        public const int FirstShop = 3, FirstThreeStarShop = 9, VisitInterval = 3;
        public static bool IsRare(string id) => id == "StarMender" || id == "ThirdBellKeeper" || id == "UnclaimedGiftKeeper";
        public static string Select(int roll) => roll < TwoStarChance ? "StarMender"
            : roll < TwoStarChance + ThreeStarChance ? "ThirdBellKeeper"
            : roll < TwoStarChance + ThreeStarChance + RelicChance ? "UnclaimedGiftKeeper" : null;
    }

    public sealed partial class RunSession
    {
        private List<RosterEntry> VisitorStarCandidates(int target)
        {
            var choices = new List<RosterEntry>();
            foreach (RosterEntry entry in ActiveRoster)
                if (entry.Tier < target && !StorySpecies(entry.UnitId) && catalog.Unit(entry.UnitId).ReferenceCompanion != "Evil") choices.Add(entry);
            return choices;
        }
        private List<ItemDefinition> VisitorRelicCandidates()
        {
            var choices = new List<ItemDefinition>();
            foreach (ItemDefinition item in catalog.Items)
                if (item.Rarity <= ItemRarity.Mythic && item.ShopWeight > 0 && item.Mechanic != RelicMechanic.CompanionGift
                    && (Eligible(item, ShopPool.Gold) || Eligible(item, ShopPool.Eggs))) choices.Add(item);
            return choices;
        }
        private bool RareVisitorFits(string id) => id == "StarMender" ? VisitorStarCandidates(2).Count > 0
            : id == "ThirdBellKeeper" ? State.Wave >= RareVisitorRules.FirstThreeStarShop && VisitorStarCandidates(3).Count > 0
            : id == "UnclaimedGiftKeeper" && VisitorRelicCandidates().Count > 0;

        private bool TryRollRareVisitor()
        {
            if (State.Wave < RareVisitorRules.FirstShop || (long)State.Wave - State.RareVisitorLastWave < RareVisitorRules.VisitInterval) return false;
            string id = RareVisitorRules.Select(Next(ref State.VisitorRandom, RareVisitorRules.RollSize));
            if (id == null || !VisitorPainted(id) || !RareVisitorFits(id)) return false;
            State.Visitor = id; State.VisitorWave = State.Wave; State.VisitorLastWave = State.Wave; State.RareVisitorLastWave = State.Wave;
            return true;
        }
        private bool GiveVisitorStars(int target)
        {
            var choices = VisitorStarCandidates(target);
            if (choices.Count == 0) return false;
            RosterEntry chosen = choices[Next(ref State.VisitorRandom, choices.Count)];
            chosen.Tier = target; RememberVariant(chosen);
            Notice = catalog.Unit(chosen.UnitId).DisplayName + " is now " + target + "-star. Its weapon stays the same.";
            return true;
        }
        private bool GiveVisitorRelic()
        {
            var choices = VisitorRelicCandidates();
            if (choices.Count == 0) return false;
            // Conditional rarity weights among available tiers: 45 / 30 / 20 / 5. Never Crimson or Rainbow.
            int[] weights = { 45, 30, 20, 5 }; int total = 0;
            for (int tier = 0; tier < weights.Length; tier++) if (choices.Exists(i => (int)i.Rarity == tier)) total += weights[tier];
            int ticket = Next(ref State.VisitorRandom, total), rarity = 0;
            for (; rarity < weights.Length; rarity++)
            {
                if (!choices.Exists(i => (int)i.Rarity == rarity)) continue;
                ticket -= weights[rarity]; if (ticket < 0) break;
            }
            choices.RemoveAll(i => (int)i.Rarity != rarity);
            ItemDefinition gift = choices[Next(ref State.VisitorRandom, choices.Count)];
            AddItem(gift);
            Notice = "A gift to keep: " + gift.Name + " (" + RunModifierRules.RarityName(gift.Rarity) + ").";
            return true;
        }
    }
}
