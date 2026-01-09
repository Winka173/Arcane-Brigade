using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    [Serializable]
    public sealed class ContributionEntry
    {
        public string UnitId;
        public string WeaponId;
        public string EffectId;
        public int Tier;
        public float Damage;
        public float Healing;
        public int Coins;
        public int Activations;
        public int Kills;
        public string Key => UnitId + "/" + WeaponId + "/" + Tier + "/" + EffectId;
    }

    public static class ContributionLedger
    {
        public static void Merge(List<ContributionEntry> destination, IReadOnlyList<ContributionEntry> source)
        {
            foreach (ContributionEntry entry in source)
            {
                ContributionEntry existing = destination.Find(candidate => candidate.Key == entry.Key);
                if (existing == null)
                {
                    existing = new ContributionEntry { UnitId = entry.UnitId, WeaponId = entry.WeaponId, EffectId = entry.EffectId, Tier = entry.Tier }; destination.Add(existing);
                }
                existing.Damage = Mathf.Min(1e12f, existing.Damage + entry.Damage);
                existing.Healing = Mathf.Min(1e12f, existing.Healing + entry.Healing);
                existing.Coins = RunSession.BoundedCounter((long)existing.Coins + entry.Coins);
                existing.Activations = RunSession.BoundedCounter((long)existing.Activations + entry.Activations);
                existing.Kills = RunSession.BoundedCounter((long)existing.Kills + entry.Kills);
            }
        }

        public static string Describe(IReadOnlyList<ContributionEntry> entries)
        {
            if (entries == null || entries.Count == 0) return "No measured contributions yet.\n";
            var lines = new List<string>();
            foreach (ContributionEntry entry in entries)
                lines.Add(entry.UnitId + " / " + entry.WeaponId + " / " + entry.EffectId + "\nEffective damage " + entry.Damage.ToString("0.#") + " · heal " + entry.Healing.ToString("0.#") + " · kills " + entry.Kills + " · coin " + entry.Coins + " · activations " + entry.Activations);
            return string.Join("\n\n", lines) + "\nCounts actual HP lost/restored only; excludes overkill, VFX and duplicate connector damage.\n";
        }
    }

    public sealed partial class CombatSimulation
    {
        private readonly List<ContributionEntry> contributions = new List<ContributionEntry>();
        private readonly Dictionary<string, ContributionEntry> contributionIndex = new Dictionary<string, ContributionEntry>();
        public IReadOnlyList<ContributionEntry> Contributions => contributions;

        private ContributionEntry Contribution(CombatCause cause)
        {
            UnitState source = null;
            foreach (UnitState candidate in units) if (candidate.Id == cause.SourceUnitId) { source = candidate; break; }
            string unitId = source == null ? "ARENA" : source.Definition.Id;
            string weaponId = source == null ? "" : source.Definition.Weapon.Id;
            string effectId = string.IsNullOrEmpty(cause.ItemId) ? "primary" : cause.ItemId;
            int tier = source == null ? 0 : source.Definition.CombatTier;
            string key = unitId + "/" + weaponId + "/" + tier + "/" + effectId;
            if (contributionIndex.TryGetValue(key, out ContributionEntry entry)) return entry;
            entry = new ContributionEntry { UnitId = unitId, WeaponId = weaponId, EffectId = effectId, Tier = tier };
            contributionIndex.Add(key, entry); contributions.Add(entry); return entry;
        }

        private void RecordDamage(CombatCause cause, float actual) { Contribution(cause).Damage += actual; }
        private void RecordHealing(CombatCause cause, float actual) { if (actual > 0) Contribution(cause).Healing += actual; }
        private void RecordActivation(CombatCause cause) { ContributionEntry entry = Contribution(cause); entry.Activations = RunSession.BoundedCounter((long)entry.Activations + 1); }
        private void RecordKill(CombatCause cause) { ContributionEntry entry = Contribution(cause); entry.Kills = RunSession.BoundedCounter((long)entry.Kills + 1); }
    }
}
