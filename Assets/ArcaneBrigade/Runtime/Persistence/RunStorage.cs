using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace ArcaneBrigade
{
    [Serializable]
    public sealed class PlayerProfile
    {
        public const int CurrentVersion = 3;
        public int Version = CurrentVersion;
        public int BestWave;
        public int Wins;
        public int TotalKills;
        public int Cosmetic;
        public float Volume = 0.7f;
        public bool ReducedMotion;
        public float MusicVolume = 0.8f;
        public float SfxVolume = 0.85f;
        public bool ReducedAudio;
        public bool ShowDamageNumbers = true;
        public bool ShowLootIndicators = true;
        public bool HighReadability;
        public bool ReducedShake;
        public bool ReducedFlashes;
        public bool ReducedEffects;
        public bool TutorialSeen;
        public List<string> CompletedRuns = new List<string>();
        public List<string> SeenUnits = new List<string>();
        public List<string> SeenItems = new List<string>();
        public List<string> SeenWeapons = new List<string>();
        public List<string> Achievements = new List<string>();
        public List<string> SeenTiers = new List<string>();
        public List<string> SeenChains = new List<string>();
        public List<string> SeenBosses = new List<string>();
        public List<RunRecord> History = new List<RunRecord>();
        public List<ScoreRecord> Records = new List<ScoreRecord>();
    }

    [Serializable]
    internal sealed class SaveEnvelope { public string Payload; public string Hash; }

    public sealed class RunStorage
    {
        private readonly string directory;
        public string LastError { get; private set; } = "";
        public RunStorage(string root) { directory = root; }
        public void SaveRun(RunSnapshot run) => Write("run.json", JsonUtility.ToJson(run, true));
        public void SaveProfile(PlayerProfile profile) => Write("profile.json", JsonUtility.ToJson(profile, true));

        public RunSnapshot LoadRun(RunSession session)
        {
            foreach (string suffix in new[] { "", ".bak" })
            {
                try
                {
                    string payload = Read("run.json" + suffix);
                    if (payload == null) continue;
                    RunSnapshot run = JsonUtility.FromJson<RunSnapshot>(payload);
                    session.Validate(run);
                    return run;
                }
                catch (Exception exception) { LastError = "Run save rejected: " + exception.Message; }
            }
            return null;
        }

        public PlayerProfile LoadProfile()
        {
            foreach (string suffix in new[] { "", ".bak" })
            {
                try
                {
                    string payload = Read("profile.json" + suffix);
                    if (payload == null) continue;
                    PlayerProfile profile = JsonUtility.FromJson<PlayerProfile>(payload);
                    if (profile == null || profile.Version < 1 || profile.Version > PlayerProfile.CurrentVersion || profile.BestWave < 0 || profile.CompletedRuns == null || profile.SeenUnits == null || profile.SeenItems == null || profile.Achievements == null || !float.IsFinite(profile.Volume)) throw new InvalidOperationException("Invalid profile");
                    if (profile.Wins < 0 || profile.TotalKills < 0 || profile.Cosmetic < 0 || profile.Cosmetic > 2) throw new InvalidOperationException("Invalid profile counters");
                    profile.Volume = Mathf.Clamp01(profile.Volume);
                    if (profile.SeenWeapons == null) profile.SeenWeapons = new List<string>();
                    if (profile.SeenTiers == null) profile.SeenTiers = new List<string>();
                    if (profile.SeenChains == null) profile.SeenChains = new List<string>();
                    if (profile.SeenBosses == null) profile.SeenBosses = new List<string>();
                    if (profile.History == null) profile.History = new List<RunRecord>();
                    if (profile.Records == null) profile.Records = new List<ScoreRecord>();
                    var historyIds = new HashSet<string>();
                    if (profile.History.Count > RunMeta.HistoryLimit) throw new InvalidOperationException("Invalid history limit");
                    foreach (RunRecord record in profile.History)
                    {
                        if (record == null || string.IsNullOrEmpty(record.Id) || record.Build == null || record.Build.RunId != record.Id || record.Nickname == null || record.Score < 0 || !historyIds.Add(record.Id) || !profile.CompletedRuns.Contains(record.Id)) throw new InvalidOperationException("Invalid run history");
                        RunSnapshot build = record.Build;
                        if (build.Roster == null || build.Items == null || build.Contributions == null || build.Wave < 1 || build.Difficulty < 0 || build.Difficulty > 4 || build.ChallengeId == null || build.TotalKills < 0 || build.BossKills < 0 || !float.IsFinite(build.CombatSeconds) || build.CombatSeconds < 0 || !float.IsFinite(build.LastDamageAmount) || build.LastDamageAmount < 0 || build.Phase != SessionPhase.Victory && build.Phase != SessionPhase.Defeat) throw new InvalidOperationException("Invalid history build");
                        foreach (RosterEntry entry in build.Roster) if (entry == null || string.IsNullOrEmpty(entry.UnitId) || string.IsNullOrEmpty(entry.WeaponId) || entry.Tier < 1 || entry.Tier > 3) throw new InvalidOperationException("Invalid history roster");
                        foreach (ItemStack stack in build.Items) if (stack == null || string.IsNullOrEmpty(stack.Id) || stack.Count < 1) throw new InvalidOperationException("Invalid history relic");
                        foreach (ContributionEntry entry in build.Contributions) if (entry == null || entry.WeaponId == null || !float.IsFinite(entry.Damage) || entry.Damage < 0 || !float.IsFinite(entry.Healing) || entry.Healing < 0 || entry.Kills < 0 || entry.Coins < 0 || entry.Activations < 0) throw new InvalidOperationException("Invalid history contribution");
                    }
                    var recordKeys = new HashSet<string>();
                    foreach (ScoreRecord record in profile.Records)
                        if (record == null || string.IsNullOrEmpty(record.Key) || string.IsNullOrEmpty(record.RunId) || record.Score < 0 || record.Hall < 1 || !recordKeys.Add(record.Key)) throw new InvalidOperationException("Invalid score record");
                    if (profile.Version < 3) { profile.MusicVolume = 0.8f; profile.SfxVolume = 0.85f; profile.ShowDamageNumbers = profile.ShowLootIndicators = true; }
                    if (!float.IsFinite(profile.MusicVolume) || !float.IsFinite(profile.SfxVolume)) throw new InvalidOperationException("Invalid audio bus settings");
                    profile.MusicVolume = Mathf.Clamp01(profile.MusicVolume); profile.SfxVolume = Mathf.Clamp01(profile.SfxVolume);
                    profile.Version = PlayerProfile.CurrentVersion;
                    return profile;
                }
                catch (Exception exception) { LastError = "Profile save rejected: " + exception.Message; }
            }
            return new PlayerProfile();
        }

        private void Write(string name, string payload)
        {
            Directory.CreateDirectory(directory);
            string destination = Path.Combine(directory, name);
            string temporary = destination + ".tmp";
            string envelope = JsonUtility.ToJson(new SaveEnvelope { Payload = payload, Hash = Hash(payload) });
            byte[] bytes = Encoding.UTF8.GetBytes(envelope);
            using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None)) { stream.Write(bytes, 0, bytes.Length); stream.Flush(true); }
            if (File.Exists(destination)) File.Replace(temporary, destination, destination + ".bak"); else File.Move(temporary, destination);
            LastError = "";
        }

        private string Read(string name)
        {
            string path = Path.Combine(directory, name);
            if (!File.Exists(path)) return null;
            string text = File.ReadAllText(path, Encoding.UTF8);
            SaveEnvelope envelope = JsonUtility.FromJson<SaveEnvelope>(text);
            if (envelope == null || envelope.Payload == null || envelope.Hash != Hash(envelope.Payload)) throw new InvalidOperationException("Checksum mismatch");
            return envelope.Payload;
        }

        private static string Hash(string payload)
        {
            using (SHA256 algorithm = SHA256.Create()) return Convert.ToBase64String(algorithm.ComputeHash(Encoding.UTF8.GetBytes(payload)));
        }

        public void RecordProgress(PlayerProfile profile, RunSnapshot run, RunCatalog catalog = null)
        {
            string before = JsonUtility.ToJson(profile);
            try
            {
                profile.BestWave = Mathf.Max(profile.BestWave, run.Wave);
                RunMeta.Discover(profile, run, catalog);
                if (run.BossKills > 0 && !profile.Achievements.Contains("First bell silenced")) profile.Achievements.Add("First bell silenced");
                if (run.Wave >= 10 && !profile.Achievements.Contains("Ten halls crossed")) profile.Achievements.Add("Ten halls crossed");
                RunMeta.Credit(profile, run);
                SaveProfile(profile);
            }
            catch { JsonUtility.FromJsonOverwrite(before, profile); throw; }
        }
    }
}
