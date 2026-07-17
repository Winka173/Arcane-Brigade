using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    [Serializable]
    public sealed class RunRecord
    {
        public string Id;
        public string ParentId;
        public string Nickname;
        public string FinishedUtc;
        public int Score;
        public RunSnapshot Build;
        public string Key => Build.ChampionId + "/" + Build.Difficulty + "/" + (Build.Endless ? "endless" : "normal") + "/" + Build.ChallengeId;
    }

    [Serializable]
    public sealed class ScoreRecord
    {
        public string Key;
        public string RunId;
        public int Score;
        public int Hall;
    }

    public static class RunMeta
    {
        public const int HistoryLimit = 50;
        public static readonly string[] DifficultyNames = { "Peaceful", "Wandering", "Nightmare", "Sleepless", "The Last Night" };
        /// <summary>The first difficulty that waits for a whole Lamp Tree (requirement 24/10/2026: two extreme ones, opened only when
        /// every permanent upgrade is done).</summary>
        public const int TreeDifficulty = 4; // the last one waits for the whole Lamp Tree
        public static readonly string[] ChallengeIds = { "", "night-shift", "first-light", "small-brigade" };
        public static readonly string[] ChallengeNames = { "Free journey", "Night shift · until dawn", "First light · clear hall 10", "Small brigade · win with ≤ 6 companions" };
        public static int ChallengeSeed(int index) => 610026 + index * 7919;
        /// <summary>Lightless opens with the five first branches lit, The Last Night with all nine.</summary>
        public static int UnlockedDifficulty(PlayerProfile profile) => MetaProgression.TreeAllDone(profile) ? DifficultyNames.Length - 1 : TreeDifficulty - 1;
        /// <summary>Each difficulty's colour, calm to dangerous (requirement 24/10/2026): green, moon blue, amber, orange, red, a deep
        /// crimson-violet for Beyond the Lamp, then blood red and the enemies' own violet for the two after the whole tree.</summary>
        public static Color DifficultyColor(int difficulty) => Mathf.Clamp(difficulty, 0, 4) switch
        {
            0 => new Color(.56f, .9f, .62f), 1 => new Color(.56f, .8f, 1f), 2 => new Color(1f, .55f, .24f),
            3 => new Color(.96f, .26f, .24f), _ => new Color(.62f, .32f, 1f),
        };
        /// <summary>The ranking number (requirement 27/10/2026: the old points are gone): the farthest night the run reached, Endless included.</summary>
        public static int Score(RunSnapshot run, int liveKills = 0, bool liveBoss = false) => Mathf.Max(0, run.Wave);
        public static string Nickname(RunSnapshot run)
        {
            ContributionEntry strongest = null;
            foreach (ContributionEntry entry in run.Contributions)
                if (entry.Damage > 0 && (strongest == null || entry.Damage > strongest.Damage || entry.Damage == strongest.Damage && string.CompareOrdinal(entry.Key, strongest.Key) < 0)) strongest = entry;
            string weapon = strongest?.WeaponId;
            if (weapon == "TeapotPopgun" || weapon == "DreamIron") return "Scorched tea party";
            if (weapon == "BellFork" || weapon == "MusicBox") return "Debt-collecting bells";
            if (weapon == "DreamStamp") return "Relentless returns office";
            if (weapon == "SleepCamera") return "Midnight photo booth";
            if (weapon == "CrescentBow") return "Threads through dreams";
            if (weapon == "RubberHammer") return "Dream repair workshop";
            return "Things that refuse to sleep";
        }
        public static string ChainEvidence(string effect)
        {
            switch (effect)
            {
                case "harvest-receipt": return "signature-harvest";
                case "reflective-ink": case "office-lens": case "office-lens-exposure": case "carbon-exposure": return "signature-office";
                case "thermal-ink": return "signature-tea";
                case "scratch-record": return "signature-chorus";
                case "treasure-ribbon": return "signature-treasure";
                case "repair-coupon": return "signature-toys";
                default: return null;
            }
        }
        public static void Discover(PlayerProfile profile, RunSnapshot run, RunCatalog catalog = null)
        {
            foreach (RosterEntry entry in run.Roster)
            {
                Add(profile.SeenUnits, entry.UnitId); Add(profile.SeenWeapons, entry.WeaponId);
                Add(profile.SeenTiers, entry.WeaponId + "/" + entry.Tier);
            }
            foreach (RosterEntry entry in run.AcquiredVariants)
            {
                Add(profile.SeenUnits, entry.UnitId); Add(profile.SeenWeapons, entry.WeaponId);
                Add(profile.SeenTiers, entry.WeaponId + "/" + entry.Tier);
            }
            if (catalog != null)
            {
                string weapon = catalog.Unit(run.ChampionId).Weapon.Id;
                Add(profile.SeenWeapons, weapon); Add(profile.SeenTiers, weapon + "/1");
            }
            foreach (ItemStack stack in run.Items) Add(profile.SeenItems, stack.Id);
            foreach (string boss in run.DefeatedBosses) Add(profile.SeenBosses, boss);
            foreach (ContributionEntry entry in run.Contributions)
            {
                string chain = ChainEvidence(entry.EffectId);
                if (chain != null && (entry.Activations > 0 || entry.Damage > 0 || entry.Healing > 0 || entry.Coins > 0)) Add(profile.SeenChains, chain);
            }
        }
        public static void Credit(PlayerProfile profile, RunSnapshot run)
        {
            if (run.Phase != SessionPhase.Victory && run.Phase != SessionPhase.Defeat || profile.CompletedRuns.Contains(run.RunId)) return;
            Add(profile.CompletedRuns, run.RunId);
            profile.TotalKills = RunSession.BoundedCounter((long)profile.TotalKills + run.TotalKills);
            if (run.Phase == SessionPhase.Victory && !run.Endless)
            {
                profile.Wins = RunSession.BoundedCounter((long)profile.Wins + 1);
                Add(profile.Achievements, "Brigade dawn");
                if (string.IsNullOrEmpty(run.ChallengeId)) Add(profile.Achievements, "clear-difficulty-" + run.Difficulty);
            }
            bool challenge = run.ChallengeId == "night-shift" && run.Phase == SessionPhase.Victory || run.ChallengeId == "first-light" && (run.Wave > 10 || run.Wave == 10 && run.Phase == SessionPhase.Victory) || run.ChallengeId == "small-brigade" && run.Phase == SessionPhase.Victory && CompanionDeployment.Select(run.Roster).Count <= 6;
            if (challenge) Add(profile.Achievements, "challenge-" + run.ChallengeId);
            var record = new RunRecord { Id = run.RunId, ParentId = run.ParentRunId, Nickname = Nickname(run), FinishedUtc = DateTime.UtcNow.ToString("O"), Score = Score(run), Build = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(run)) };
            if (record.Build.Meta != null) record.Build.Meta.RewindShop = ""; // the rewind point is not history
            profile.History.Insert(0, record);
            if (profile.History.Count > HistoryLimit) profile.History.RemoveAt(profile.History.Count - 1);
            ScoreRecord best = profile.Records.Find(entry => entry.Key == record.Key);
            if (best == null) { best = new ScoreRecord { Key = record.Key }; profile.Records.Add(best); }
            if (best.RunId == null || record.Score > best.Score) { best.Score = record.Score; best.Hall = run.Wave; best.RunId = run.RunId; }
        }
        public static void Add(List<string> destination, string id) { if (!string.IsNullOrEmpty(id) && !destination.Contains(id)) destination.Add(id); }
        public static bool CosmeticUnlocked(PlayerProfile profile, int bundle) => bundle == 0 || bundle == 1 && profile.BestWave >= 5 || bundle == 2 && profile.SeenBosses.Count >= 3;
        public static string CosmeticName(int bundle) => bundle == 1 ? "Night office" : bundle == 2 ? "Paper stage" : "Lost-and-found shop";
    }
}
