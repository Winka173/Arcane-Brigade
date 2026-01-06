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
        public static readonly string[] DifficultyNames = { "Êm đềm", "Lạc lối", "Chập chờn", "Ác mộng", "Không ngủ" };
        public static readonly string[] ChallengeIds = { "", "night-shift", "first-light", "small-brigade" };
        public static readonly string[] ChallengeNames = { "Chuyến đi tự do", "Ca đêm · tới bình minh", "Ánh sáng đầu · clear hall 10", "Đội nhỏ · thắng với ≤ 6 đồng minh" };
        public static int ChallengeSeed(int index) => 610026 + index * 7919;
        public static int UnlockedDifficulty(PlayerProfile profile)
        {
            int unlocked = 0;
            while (unlocked < 4 && profile.Achievements.Contains("clear-difficulty-" + unlocked)) unlocked++;
            return unlocked;
        }
        public static int Score(RunSnapshot run, int liveKills = 0, bool liveBoss = false)
        {
            int cleared = Mathf.Max(0, run.Wave - Mathf.Max(1, run.StartHall));
            if (run.Wave >= run.StartHall && (run.Phase == SessionPhase.Shop || run.Phase == SessionPhase.Victory)) cleared++;
            long value = (long)cleared * 1000 + ((long)run.TotalKills + liveKills) * 10 + ((long)run.BossKills + (liveBoss ? 1 : 0)) * 500;
            if (run.Phase == SessionPhase.Victory) value += 5000;
            return RunSession.BoundedCounter(value * (100 + run.Difficulty * 20) / 100);
        }
        public static string Nickname(RunSnapshot run)
        {
            ContributionEntry strongest = null;
            foreach (ContributionEntry entry in run.Contributions)
                if (entry.Damage > 0 && (strongest == null || entry.Damage > strongest.Damage || entry.Damage == strongest.Damage && string.CompareOrdinal(entry.Key, strongest.Key) < 0)) strongest = entry;
            string weapon = strongest?.WeaponId;
            if (weapon == "TeapotPopgun" || weapon == "DreamIron") return "Tiệc trà cháy bếp";
            if (weapon == "BellFork" || weapon == "MusicBox") return "Dàn chuông đòi nợ";
            if (weapon == "DreamStamp") return "Phòng hoàn trả không nghỉ";
            if (weapon == "SleepCamera") return "Ảnh thẻ lúc nửa đêm";
            if (weapon == "CrescentBow") return "Đường chỉ qua giấc mơ";
            if (weapon == "RubberHammer") return "Xưởng sửa những cơn mơ";
            return "Những món đồ chưa chịu ngủ";
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
            bool challenge = run.ChallengeId == "night-shift" && run.Phase == SessionPhase.Victory || run.ChallengeId == "first-light" && (run.Wave > 10 || run.Wave == 10 && run.Phase == SessionPhase.Victory) || run.ChallengeId == "small-brigade" && run.Phase == SessionPhase.Victory && run.Roster.Count <= 6;
            if (challenge) Add(profile.Achievements, "challenge-" + run.ChallengeId);
            var record = new RunRecord { Id = run.RunId, ParentId = run.ParentRunId, Nickname = Nickname(run), FinishedUtc = DateTime.UtcNow.ToString("O"), Score = Score(run), Build = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(run)) };
            profile.History.Insert(0, record);
            if (profile.History.Count > HistoryLimit) profile.History.RemoveAt(profile.History.Count - 1);
            ScoreRecord best = profile.Records.Find(entry => entry.Key == record.Key);
            if (best == null) { best = new ScoreRecord { Key = record.Key }; profile.Records.Add(best); }
            if (best.RunId == null || record.Score > best.Score) { best.Score = record.Score; best.Hall = run.Wave; best.RunId = run.RunId; }
        }
        public static void Add(List<string> destination, string id) { if (!string.IsNullOrEmpty(id) && !destination.Contains(id)) destination.Add(id); }
        public static bool CosmeticUnlocked(PlayerProfile profile, int bundle) => bundle == 0 || bundle == 1 && profile.BestWave >= 5 || bundle == 2 && profile.SeenBosses.Count >= 3;
        public static string CosmeticName(int bundle) => bundle == 1 ? "Văn phòng đêm" : bundle == 2 ? "Sân khấu giấy" : "Cửa hàng thất lạc";
    }
}
