using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class RunSnapshot
    {
        // Separate from SchemaRevision so older shop/roster migrations still run after remapping.
        public int DifficultyLayoutRevision;
    }

    public static class DifficultySaveMigration
    {
        private static readonly int[] OldToFive = { 0, 1, 1, 2, 3, 3, 4, 4 };

        public static int LegacyIndex(int difficulty)
        {
            if (difficulty < 0 || difficulty >= OldToFive.Length) throw new InvalidOperationException("Invalid legacy difficulty");
            return OldToFive[difficulty];
        }

        public static RunSnapshot ForRead(RunSnapshot snapshot)
        {
            if (snapshot == null || snapshot.SchemaRevision < 3 || snapshot.SchemaRevision >= 6 || snapshot.DifficultyLayoutRevision >= 1) return snapshot;
            var migrated = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(snapshot));
            migrated.Difficulty = LegacyIndex(snapshot.Difficulty);
            migrated.DifficultyLayoutRevision = 1;
            RemapEvidence(migrated.AchievementEvidence);
            return migrated;
        }

        // Profile version 5 marks the new layout. Meta revision 5 already remaps WonMask;
        // leave that responsibility there to avoid shifting previously migrated bits twice.
        public static void ProfileForRead(PlayerProfile profile)
        {
            if (profile.Version >= 5 || profile.DifficultyLayoutRevision >= 1) return;
            profile.LastDifficulty = LegacyIndex(profile.LastDifficulty);
            foreach (var record in profile.History ?? new List<RunRecord>())
                if (record?.Build != null) record.Build = ForRead(record.Build);

            var best = new Dictionary<string, ScoreRecord>();
            foreach (var record in profile.Records ?? new List<ScoreRecord>())
            {
                if (record == null || record.Key == null) throw new InvalidOperationException("Invalid legacy score record");
                var parts = record.Key.Split('/');
                if (parts.Length == 4 && int.TryParse(parts[1], out int old))
                { parts[1] = LegacyIndex(old).ToString(); record.Key = string.Join("/", parts); }
                if (!best.TryGetValue(record.Key, out var previous) || record.Score > previous.Score || record.Score == previous.Score && record.Hall > previous.Hall)
                    best[record.Key] = record;
            }
            profile.Records = best.Values.ToList();
            profile.Achievements = (profile.Achievements ?? new List<string>()).Select(RemapAchievement).Distinct().ToList();
            if (profile.Meta?.Found != null)
                profile.Meta.Found = profile.Meta.Found.Select(RemapRewardKey).Distinct().ToList();
            RemapEvidence(profile.AchievementEvidence);
            profile.DifficultyLayoutRevision = 1;
        }

        private static string RemapAchievement(string id)
        {
            foreach (string prefix in new[] { "clear-difficulty-", "journal-difficulty-" })
                if (id != null && id.StartsWith(prefix, StringComparison.Ordinal) && int.TryParse(id.Substring(prefix.Length), out int old))
                    return prefix + LegacyIndex(old);
            return id;
        }

        private static string RemapRewardKey(string key)
        {
            if (key == null) return key;
            var parts = key.Split('/');
            if (parts.Length >= 2 && (parts[0] == "boss" || parts[0] == "keeper") && int.TryParse(parts[1], out int old))
            { parts[1] = LegacyIndex(old).ToString(); return string.Join("/", parts); }
            return key;
        }

        private static void RemapEvidence(List<AchievementMetric> evidence)
        {
            if (evidence == null) return;
            foreach (var metric in evidence)
                if (metric != null && metric.Id == "difficulty" && float.IsFinite(metric.Value) && metric.Value >= 1 && metric.Value <= 8)
                    metric.Value = LegacyIndex((int)metric.Value - 1) + 1;
        }
    }
}
