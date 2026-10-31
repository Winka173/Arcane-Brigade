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
        public const int CurrentVersion = 5; // 5: five-difficulty history, records and selection
        public int Version = CurrentVersion;
        public int BestWave;
        public int Wins;
        public int TotalKills;
        public int Cosmetic;
        public List<AchievementMetric> AchievementEvidence = new List<AchievementMetric>();
        public float Volume = 0.7f;
        public bool ReducedMotion;
        public float MusicVolume = 0.8f;
        public float SfxVolume = 0.85f;
        public float VoiceVolume = 1f; // shopkeeper / boss / companion voice lines (VoiceDirector); absent in older saves = 1
        public bool ReducedAudio;
        public bool ShowDamageNumbers = true;
        public bool ShowLootIndicators = true;
        public bool HighReadability;
        public bool ReducedShake;
        public float ShakeScale = 1f;   // screen-shake strength (Settings: 100/75/50/25/off); ReducedShake stays true at off
        public int WindowMode = -1;     // -1 keep the player's launch mode; 0 fullscreen, 1 borderless, 2 windowed
        public int ScreenWidth, ScreenHeight; // 0 keep the launch resolution
        public bool VSync = true;
        /// <summary>Frames a second when vsync is off (PC: 60, 120 or 0 for no cap; phones: 60 or 30 to save battery).</summary>
        public int FrameCap;
        /// <summary>The difficulty last chosen for a new run (hero select opens on it again).</summary>
        public int LastDifficulty;
        public int DifficultyLayoutRevision;
        /// <summary>Phones: 0 renders at the screen's full sharpness, 1 at 75% (lighter on the battery and the GPU).</summary>
        public int MobileSharpness;
        public bool ReducedFlashes;
        public bool ReducedEffects;
        public bool UiCrafted; // false: the painted kit (the default, requirement 21/10/2026); true: every UI piece drawn in code
        public bool HighlightHero = true; // play-test 20/10/2026: the hero's halo, show-through and edge arrow (older saves: on)
        public bool TutorialSeen;
        public List<string> TipsSeen = new List<string>(); // one-time tips already shown (PlayerGuide); Settings can clear them
        public bool NightmareLampPromise;
        public bool NightmareShopClosed;
        public bool ActOneChairSpoke; // after the first dawn, the menu armchair has asked its question once
        public bool StoryPagesAutoOpen; // false (default): new journal pages only show a sealed note; true: they open by themselves
        public bool StoryArrivalShown;  // the wave-1 arrival page has opened by itself once (first run start)
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
        /// <summary>The Waiting Room: Lamp Oil, Keepsakes, station ranks, hero mastery (absent before version 4: migrated once).</summary>
        public MetaProgress Meta = new MetaProgress();
    }

    [Serializable]
    internal sealed class SaveEnvelope { public string Payload; public string Hash; }

    public sealed class RunStorage
    {
        private readonly string directory;
        public string LastError { get; private set; } = "";
        public void ClearError() => LastError = "";
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
                    run = DifficultySaveMigration.ForRead(run);
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
                    profile.AchievementEvidence ??= new List<AchievementMetric>();
                    var evidenceKeys = new HashSet<string>();
                    if (profile.AchievementEvidence.Count > 512) throw new InvalidOperationException("Invalid achievement journal size");
                    foreach (AchievementMetric m in profile.AchievementEvidence) if (m == null || string.IsNullOrEmpty(m.Id) || !float.IsFinite(m.Value) || m.Value < 0 || !evidenceKeys.Add(m.Id)) throw new InvalidOperationException("Invalid achievement journal");
                    if (profile.SeenTiers == null) profile.SeenTiers = new List<string>();
                    if (profile.SeenChains == null) profile.SeenChains = new List<string>();
                    if (profile.SeenBosses == null) profile.SeenBosses = new List<string>();
                    if (profile.History == null) profile.History = new List<RunRecord>();
                    if (profile.Records == null) profile.Records = new List<ScoreRecord>();
                    DifficultySaveMigration.ProfileForRead(profile);
                    var historyIds = new HashSet<string>();
                    if (profile.History.Count > RunMeta.HistoryLimit) throw new InvalidOperationException("Invalid history limit");
                    foreach (RunRecord record in profile.History)
                    {
                        if (record == null || string.IsNullOrEmpty(record.Id) || record.Build == null || record.Build.RunId != record.Id || record.Nickname == null || record.Score < 0 || !historyIds.Add(record.Id) || !profile.CompletedRuns.Contains(record.Id)) throw new InvalidOperationException("Invalid run history");
                        RunSnapshot build = record.Build;
                        if (build.Roster == null || build.Items == null || build.Contributions == null || build.Wave < 1 || build.Difficulty < 0 || build.Difficulty >= RunMeta.DifficultyNames.Length || build.ChallengeId == null || build.TotalKills < 0 || build.BossKills < 0 || !float.IsFinite(build.CombatSeconds) || build.CombatSeconds < 0 || !float.IsFinite(build.LastDamageAmount) || build.LastDamageAmount < 0 || build.Phase != SessionPhase.Victory && build.Phase != SessionPhase.Defeat) throw new InvalidOperationException("Invalid history build");
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
                    profile.VoiceVolume = float.IsFinite(profile.VoiceVolume) ? Mathf.Clamp01(profile.VoiceVolume) : 1f;
                    MetaProgression.Ensure(profile); // older profiles: zero ranks, Lamp Oil for the best wave
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
                MetaProgression.Ensure(profile);
                profile.BestWave = Mathf.Max(profile.BestWave, run.Wave);
                MetaProgression.CountFinds(profile, run); // first-ever finds feed the run's Lamp Oil
                RunMeta.Discover(profile, run, catalog);
                if (run.Nightmare?.Ending == NightmareEndingStage.Complete && run.Nightmare.CustomerDefeated)
                {
                    if (run.Nightmare.Farewell == NightmareFarewell.LeaveLight) profile.NightmareLampPromise = true;
                    else profile.NightmareShopClosed = true;
                }
                if (run.BossKills > 0 && !profile.Achievements.Contains("First bell silenced")) profile.Achievements.Add("First bell silenced");
                if (run.Wave >= 10 && !profile.Achievements.Contains("Ten halls crossed")) profile.Achievements.Add("Ten halls crossed");
                bool hauled = MetaProgression.Credit(profile, run, catalog); // before RunMeta.Credit marks the run completed
                RunMeta.Credit(profile, run);
                RunAchievements.Credit(profile, run, catalog);
                // requirement 25/10/2026: the profile carries up to 50 finished runs (well over 1 MB); every shop purchase, reroll
                // and wave boundary records progress, so it is written only when it differs from what this path last wrote
                string after = JsonUtility.ToJson(profile);
                string path = Path.Combine(directory, "profile.json");
                if (!AlreadyWritten(path, after)) { SaveProfile(profile); Remember(path, after); }
                if (hauled) SaveRun(run); // the haul shows on the summary when the run is reopened
            }
            catch { JsonUtility.FromJsonOverwrite(before, profile); throw; }
        }

        // the compact profile last written by RecordProgress at each path and the file's write time just after; any other
        // write (settings, tips, another storage) changes that time, so the next record writes again
        private static readonly Dictionary<string, (string Json, DateTime Written)> recorded = new Dictionary<string, (string, DateTime)>();
        private static bool AlreadyWritten(string path, string json)
        {
            try { return recorded.TryGetValue(path, out var last) && last.Json == json && File.Exists(path) && File.GetLastWriteTimeUtc(path) == last.Written; }
            catch { return false; }
        }
        private static void Remember(string path, string json)
        {
            try { recorded[path] = (json, File.GetLastWriteTimeUtc(path)); }
            catch { recorded.Remove(path); }
        }
    }
}
