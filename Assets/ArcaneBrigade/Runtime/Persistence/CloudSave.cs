using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Why a sync could not finish, in words the settings row can explain.</summary>
    public enum CloudFailure { Offline, AuthNeeded, Quota, NewerVersion, Other }

    public sealed class CloudSyncException : Exception
    {
        public readonly CloudFailure Failure;
        public CloudSyncException(CloudFailure failure, string detail = "") : base(failure + (string.IsNullOrEmpty(detail) ? "" : ": " + detail)) { Failure = failure; }
    }

    /// <summary>Storage the sync writes to: one file per device. Each device only ever writes its own file, so two devices syncing
    /// at the same moment never overwrite each other.</summary>
    public interface ICloudBackend
    {
        /// <summary>Every device file: device id → a version tag that changes whenever the file does.</summary>
        Task<Dictionary<string, string>> ListDevices();
        Task<string> ReadDevice(string deviceId);
        Task WriteDevice(string deviceId, string document);
    }

    /// <summary>A backend in memory shared by simulated devices in the editor checks. Nothing leaves the process.</summary>
    public sealed class MemoryCloudBackend : ICloudBackend
    {
        public readonly Dictionary<string, string> Devices = new Dictionary<string, string>();
        private readonly Dictionary<string, string> versions = new Dictionary<string, string>();
        private int version;
        public CloudFailure? FailWith;
        private void Check() { if (FailWith.HasValue) throw new CloudSyncException(FailWith.Value); }
        public Task<Dictionary<string, string>> ListDevices() { Check(); return Task.FromResult(new Dictionary<string, string>(versions)); }
        public Task<string> ReadDevice(string deviceId) { Check(); Devices.TryGetValue(deviceId, out string d); return Task.FromResult(d); }
        public Task WriteDevice(string deviceId, string document) { Check(); Devices[deviceId] = document; versions[deviceId] = (++version).ToString(); return Task.CompletedTask; }
    }


    /// <summary>One synced part of a device file: the Lamport revision of the change it carries and the device that made it (ties
    /// go to the larger id, the same answer on every device; wall-clock time never decides), and its content.</summary>
    [Serializable]
    public sealed class CloudRecord { public long Revision; public string Origin = ""; public string Payload = ""; }

    /// <summary>One device's file: its whole merged view of the three synced parts.</summary>
    [Serializable]
    public sealed class CloudDeviceFile
    {
        public const int CurrentFormat = 1;
        public int Format = CurrentFormat;
        public int ProfileVersion = PlayerProfile.CurrentVersion;
        public string DeviceName = "";
        public string WrittenUtc = "";
        public CloudRecord Progress = new CloudRecord(), Settings = new CloudRecord(), Run = new CloudRecord();
    }

    [Serializable]
    public sealed class CloudPartState { public long Revision; public string Origin = ""; public string LastHash = ""; }

    /// <summary>The device's own sync state (never uploaded): connected or not, automatic or only on Sync now, its id, what each part
    /// last synced, and the version tag of every device file it has read.</summary>
    [Serializable]
    public sealed class CloudSyncState
    {
        public bool Enabled;
        public bool Auto = true;
        public string DeviceId = "";
        public CloudPartState Progress = new CloudPartState(), Settings = new CloudPartState(), Run = new CloudPartState();
        public string LastSyncUtc = "";
        public List<string> Seen = new List<string>(); // "deviceId=version"
        public void Forget() { Progress = new CloudPartState(); Settings = new CloudPartState(); Run = new CloudPartState(); Seen.Clear(); LastSyncUtc = ""; }
    }

    /// <summary>What this device holds now: its profile and the run in play (its save payload, empty when none is in play).</summary>
    public sealed class CloudLocal { public PlayerProfile Profile; public string Run = ""; }

    public enum CloudChoice { None, KeepThisDevice, UseCloud }

    /// <summary>Both this device and another changed the progress or the run since they last synced: nothing is written until the
    /// player picks a side; the other side is kept as a backup.</summary>
    public sealed class CloudConflict { public PlayerProfile CloudProfile; public string CloudRun = ""; public string CloudDevice = ""; public string CloudWritten = ""; public bool Progress, Run; }

    public sealed class CloudResult
    {
        /// <summary>The profile to write over the local one (this device's own screen settings already in it), null when unchanged.</summary>
        public PlayerProfile Profile;
        /// <summary>The run to put in play, "" when the run in play ended elsewhere, null when unchanged.</summary>
        public string Run;
        public bool NewerSeen, Deferred;
        public CloudConflict Conflict;
        /// <summary>The sync state before this sync, put back by <see cref="CloudSave.Abandon"/> when the result is never applied.</summary>
        public string StateBefore;
    }

    /// <summary>The merge (requirement 7/11/2026, after Sticky Wall's Drive sync). Three parts travel, each with its own revision:
    /// the progress, the settings that suit any device, and the run in play. Things that only grow (finds, achievements, finished
    /// runs, records, best wave, story flags) are combined from every device; what is spent or chosen (Lamp Oil, Keepsakes, ranks,
    /// lanterns, picks) comes from the most recent change. When two devices changed the progress or the run since they last synced,
    /// nothing is overwritten: the player chooses, and the other side is kept as a backup. Screen, window, frame rate, sharpness and
    /// the last difficulty stay on their device; purchases never travel (the store restores them).</summary>
    public static class CloudSave
    {
        /// <summary>Settings that suit any device: they travel as the Settings part.</summary>
        public static readonly string[] PortableFields =
        {
            nameof(PlayerProfile.Volume), nameof(PlayerProfile.Language), nameof(PlayerProfile.ReducedMotion), nameof(PlayerProfile.MusicVolume),
            nameof(PlayerProfile.SfxVolume), nameof(PlayerProfile.VoiceVolume), nameof(PlayerProfile.ReducedAudio), nameof(PlayerProfile.ShowDamageNumbers),
            nameof(PlayerProfile.ShowLootIndicators), nameof(PlayerProfile.HighReadability), nameof(PlayerProfile.ReducedShake), nameof(PlayerProfile.ShakeScale),
            nameof(PlayerProfile.ReducedFlashes), nameof(PlayerProfile.ReducedEffects), nameof(PlayerProfile.UiCrafted), nameof(PlayerProfile.HighlightHero),
            nameof(PlayerProfile.StoryPagesAutoOpen),
        };
        /// <summary>Settings of this screen only: they never leave the device.</summary>
        public static readonly string[] LocalFields =
        {
            nameof(PlayerProfile.WindowMode), nameof(PlayerProfile.ScreenWidth), nameof(PlayerProfile.ScreenHeight), nameof(PlayerProfile.VSync),
            nameof(PlayerProfile.FrameCap), nameof(PlayerProfile.LastDifficulty), nameof(PlayerProfile.DifficultyLayoutRevision), nameof(PlayerProfile.MobileSharpness),
        };

        private static FieldInfo[] portableInfo, localInfo;
        private static IEnumerable<FieldInfo> PortableInfo => portableInfo ??= PortableFields.Select(n => typeof(PlayerProfile).GetField(n)).ToArray();
        private static IEnumerable<FieldInfo> LocalInfo => localInfo ??= LocalFields.Select(n => typeof(PlayerProfile).GetField(n)).ToArray();

        public static PlayerProfile Clone(PlayerProfile p) => JsonUtility.FromJson<PlayerProfile>(JsonUtility.ToJson(p));
        public static void CopyPortable(PlayerProfile from, PlayerProfile to) { foreach (FieldInfo f in PortableInfo) f.SetValue(to, f.GetValue(from)); }
        public static void CopyLocal(PlayerProfile from, PlayerProfile to) { foreach (FieldInfo f in LocalInfo) f.SetValue(to, f.GetValue(from)); }

        /// <summary>The profile with every setting at its default: the Progress part.</summary>
        public static string ProgressJson(PlayerProfile p)
        {
            PlayerProfile copy = Clone(p), blank = new PlayerProfile();
            CopyPortable(blank, copy); CopyLocal(blank, copy);
            return JsonUtility.ToJson(copy);
        }
        /// <summary>A blank profile carrying only the portable settings: the Settings part.</summary>
        public static string SettingsJson(PlayerProfile p)
        {
            var copy = new PlayerProfile();
            CopyPortable(p, copy);
            return JsonUtility.ToJson(copy);
        }
        public static string Hash(string text)
        {
            using (SHA256 sha = SHA256.Create()) return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(text ?? "")));
        }

        /// <summary>The newer of two changes: higher revision, then the larger origin id.</summary>
        public static bool Newer(long revision, string origin, long thanRevision, string thanOrigin) =>
            revision != thanRevision ? revision > thanRevision : string.CompareOrdinal(origin ?? "", thanOrigin ?? "") > 0;

        /// <summary>A run payload that is still being played ("" otherwise: a finished run is credited to the profile and travels there).</summary>
        public static string InPlay(string payload)
        {
            if (string.IsNullOrEmpty(payload)) return "";
            try { RunSnapshot run = JsonUtility.FromJson<RunSnapshot>(payload); return run != null && run.Phase != SessionPhase.Victory && run.Phase != SessionPhase.Defeat ? payload : ""; }
            catch { return ""; }
        }
        /// <summary>How much a profile has played: oil and keepsakes ever earned, and the best wave.</summary>
        public static long Earned(PlayerProfile p) => p?.Meta == null ? 0 : (long)p.Meta.LampOilEarned + p.Meta.KeepsakesEarned * 10L + p.BestWave;

        /// <summary>The winner's progress with every growing part of the others added.</summary>
        public static PlayerProfile Merge(PlayerProfile winner, IEnumerable<PlayerProfile> others)
        {
            PlayerProfile m = Clone(winner);
            MetaProgression.Ensure(m);
            bool addedRuns = false;
            foreach (PlayerProfile o in others)
            {
                if (o == null) continue;
                MetaProgression.Ensure(o);
                m.BestWave = Mathf.Max(m.BestWave, o.BestWave);
                m.Wins = Mathf.Max(m.Wins, o.Wins);
                m.TotalKills = Mathf.Max(m.TotalKills, o.TotalKills);
                Union(m.CompletedRuns, o.CompletedRuns); Union(m.SeenUnits, o.SeenUnits); Union(m.SeenItems, o.SeenItems);
                Union(m.SeenWeapons, o.SeenWeapons); Union(m.Achievements, o.Achievements); Union(m.SeenTiers, o.SeenTiers);
                Union(m.SeenChains, o.SeenChains); Union(m.SeenBosses, o.SeenBosses); Union(m.TipsSeen, o.TipsSeen);
                m.TutorialSeen |= o.TutorialSeen; m.NightmareLampPromise |= o.NightmareLampPromise; m.NightmareShopClosed |= o.NightmareShopClosed;
                m.ActOneChairSpoke |= o.ActOneChairSpoke; m.StoryArrivalShown |= o.StoryArrivalShown;
                foreach (AchievementMetric e in o.AchievementEvidence ?? new List<AchievementMetric>())
                {
                    AchievementMetric mine = m.AchievementEvidence.Find(x => x.Id == e.Id);
                    if (mine == null) m.AchievementEvidence.Add(new AchievementMetric { Id = e.Id, Value = e.Value }); else mine.Value = Mathf.Max(mine.Value, e.Value);
                }
                foreach (RunRecord r in o.History ?? new List<RunRecord>()) if (!m.History.Exists(x => x.Id == r.Id)) { m.History.Add(r); addedRuns = true; }
                foreach (ScoreRecord r in o.Records ?? new List<ScoreRecord>())
                {
                    int at = m.Records.FindIndex(x => x.Key == r.Key);
                    if (at < 0) m.Records.Add(r); else if (r.Score > m.Records[at].Score) m.Records[at] = r;
                }
                MetaProgress a = m.Meta, b = o.Meta;
                a.LampOilEarned = Mathf.Max(a.LampOilEarned, b.LampOilEarned); a.KeepsakesEarned = Mathf.Max(a.KeepsakesEarned, b.KeepsakesEarned);
                a.DuetsFound = Mathf.Max(a.DuetsFound, b.DuetsFound);
                Union(a.Found, b.Found); Union(a.Letters, b.Letters);
                a.WonMask |= b.WonMask; a.TreeAllDone |= b.TreeAllDone;
                foreach (HeroMastery h in b.Mastery ?? new List<HeroMastery>())
                {
                    HeroMastery mine = a.Mastery.Find(x => x.Hero == h.Hero);
                    if (mine == null) a.Mastery.Add(new HeroMastery { Hero = h.Hero, Waves = h.Waves, Wins = h.Wins, AltStarter = h.AltStarter, SecondHabit = h.SecondHabit });
                    else { mine.Waves = Mathf.Max(mine.Waves, h.Waves); mine.Wins = Mathf.Max(mine.Wins, h.Wins); }
                }
                BestScores(a.PactBest, b.PactBest); BestScores(a.DailyBest, b.DailyBest);
            }
            // newest first, as many as a profile keeps (the order RunMeta.Credit writes); a record or a completed id is never dropped
            if (addedRuns) m.History = m.History.OrderByDescending(r => r.FinishedUtc ?? "", StringComparer.Ordinal).ThenBy(r => r.Id, StringComparer.Ordinal).Take(RunMeta.HistoryLimit).ToList();
            return m;
        }
        private static void Union(List<string> into, List<string> from) { if (from == null) return; foreach (string s in from) if (!into.Contains(s)) into.Add(s); }
        private static void BestScores(List<MetaScore> into, List<MetaScore> from)
        {
            if (from == null) return;
            foreach (MetaScore s in from)
            {
                int at = into.FindIndex(x => x.Key == s.Key);
                if (at < 0) into.Add(new MetaScore { Key = s.Key, Score = s.Score, Wave = s.Wave, Heat = s.Heat });
                else if (s.Score > into[at].Score) into[at] = new MetaScore { Key = s.Key, Score = s.Score, Wave = s.Wave, Heat = s.Heat };
            }
        }

        /// <summary>Reads a profile through the save's own checks (the same rules as a profile on disk); null when it is refused.</summary>
        public static PlayerProfile Validated(string json)
        {
            string folder = Path.Combine(Application.temporaryCachePath, "cloud-check-" + Guid.NewGuid().ToString("N"));
            try
            {
                var storage = new RunStorage(folder);
                PlayerProfile raw = JsonUtility.FromJson<PlayerProfile>(json);
                if (raw == null) return null;
                storage.SaveProfile(raw);
                PlayerProfile read = storage.LoadProfile();
                return string.IsNullOrEmpty(storage.LastError) ? read : null;
            }
            catch { return null; }
            finally { try { Directory.Delete(folder, true); } catch { } }
        }

        private delegate CloudRecord Pick(CloudDeviceFile f);

        /// <summary>One sync: reads the other devices' files that changed, merges each part, and writes this device's file when it
        /// has something new. Never touches the local save: the caller applies the result. With <paramref name="canApply"/> false
        /// (during a run) it only uploads this device's own changes, and stops without writing (Deferred) only on a clash.</summary>
        public static async Task<CloudResult> Sync(ICloudBackend backend, CloudSyncState state, CloudLocal local, Dictionary<string, CloudDeviceFile> cache, bool canApply = true, CloudChoice choice = CloudChoice.None)
        {
            var result = new CloudResult();
            if (string.IsNullOrEmpty(state.DeviceId)) state.DeviceId = Guid.NewGuid().ToString("N");
            string me = state.DeviceId;
            Dictionary<string, string> listed = await backend.ListDevices();
            foreach (var entry in listed)
            {
                if (entry.Key == me) continue;
                string seen = entry.Key + "=" + entry.Value;
                if (cache.ContainsKey(entry.Key) && state.Seen.Contains(seen)) continue;
                string text = await backend.ReadDevice(entry.Key);
                CloudDeviceFile file = null;
                try { file = string.IsNullOrEmpty(text) ? null : JsonUtility.FromJson<CloudDeviceFile>(text); } catch { }
                if (file == null || file.Format > CloudDeviceFile.CurrentFormat || file.ProfileVersion > PlayerProfile.CurrentVersion) { result.NewerSeen |= file != null; cache.Remove(entry.Key); continue; }
                file.Progress ??= new CloudRecord(); file.Settings ??= new CloudRecord(); file.Run ??= new CloudRecord();
                cache[entry.Key] = file;
                state.Seen.RemoveAll(s => s.StartsWith(entry.Key + "=", StringComparison.Ordinal));
                state.Seen.Add(seen);
            }
            foreach (string gone in cache.Keys.Where(k => !listed.ContainsKey(k)).ToList()) cache.Remove(gone);

            string before = JsonUtility.ToJson(state); // put back whenever this sync stops without writing
            result.StateBefore = before;
            List<CloudDeviceFile> files = cache.Values.ToList();
            var profiles = new Dictionary<CloudDeviceFile, PlayerProfile>();
            foreach (CloudDeviceFile f in files) { PlayerProfile p = string.IsNullOrEmpty(f.Progress.Payload) ? null : Validated(f.Progress.Payload); if (p != null) profiles[f] = p; }

            // this device's profile goes through the same checks and upgrades as every file read, so both sides compare alike
            PlayerProfile mine = Validated(JsonUtility.ToJson(local.Profile)) ?? local.Profile;
            CopyPortable(local.Profile, mine); CopyLocal(local.Profile, mine);
            string[] payload = { ProgressJson(mine), SettingsJson(mine), InPlay(local.Run) };
            CloudPartState[] parts = { state.Progress, state.Settings, state.Run };
            Pick[] pick = { f => f.Progress, f => f.Settings, f => f.Run };
            bool[] changed = new bool[3], remoteNewer = new bool[3];
            for (int i = 0; i < 3; i++)
            {
                string hash = Hash(payload[i]);
                bool joining = string.IsNullOrEmpty(parts[i].LastHash);
                // joining: the progress counts as a change only when this device has played, the settings only when no device has
                // shared any yet, a run when one is in play
                bool settingsShared = files.Exists(f => f.Settings.Revision > 0);
                changed[i] = joining ? (i == 0 ? Earned(mine) > 0 : i == 1 ? !settingsShared : payload[2] != "") : hash != parts[i].LastHash;
                foreach (CloudDeviceFile f in files)
                {
                    CloudRecord r = pick[i](f);
                    if (i == 0 && !profiles.ContainsKey(f)) continue;
                    if (r.Revision > parts[i].Revision && r.Origin != me && Hash(r.Payload) != hash) remoteNewer[i] = true;
                }
            }
            // the progress and the run are never overwritten on a clash: the player decides (settings: the newest change wins)
            bool clashProgress = changed[0] && remoteNewer[0], clashRun = changed[2] && remoteNewer[2];
            if (clashProgress)
            {
                // only a real clash when the sides disagree on what is spent or chosen: finds that only grow combine without asking
                PlayerProfile newest = null; long newestRev = -1;
                foreach (var kv in profiles) if (kv.Key.Progress.Revision > newestRev) { newestRev = kv.Key.Progress.Revision; newest = kv.Value; }
                var remotes = profiles.Values.ToList();
                var rest = new List<PlayerProfile> { Clone(mine) };
                rest.AddRange(remotes.Where(p => !ReferenceEquals(p, newest)));
                clashProgress = newest != null && ProgressJson(Merge(mine, remotes)) != ProgressJson(Merge(newest, rest));
            }
            if ((clashProgress || clashRun) && choice == CloudChoice.None)
            {
                JsonUtility.FromJsonOverwrite(before, state);
                if (!canApply) { result.Deferred = true; return result; } // during a run: the menu asks
                CloudDeviceFile best = null;
                foreach (CloudDeviceFile f in files)
                {
                    long rev = Math.Max(clashProgress ? f.Progress.Revision : 0, clashRun ? f.Run.Revision : 0);
                    long bestRev = best == null ? -1 : Math.Max(clashProgress ? best.Progress.Revision : 0, clashRun ? best.Run.Revision : 0);
                    if (rev > bestRev) best = f;
                }
                result.Conflict = new CloudConflict
                {
                    CloudProfile = best != null && profiles.TryGetValue(best, out PlayerProfile bp) ? bp : null,
                    CloudRun = best?.Run.Payload ?? "", CloudDevice = best?.DeviceName ?? "", CloudWritten = best?.WrittenUtc ?? "",
                    Progress = clashProgress, Run = clashRun,
                };
                return result;
            }
            for (int i = 0; i < 3; i++)
            {
                bool clash = i == 0 ? clashProgress : i == 2 && clashRun;
                if (clash && choice == CloudChoice.UseCloud) { changed[i] = false; parts[i].Revision = 0; parts[i].Origin = ""; }
                if (!changed[i]) continue;
                long top = parts[i].Revision;
                foreach (CloudDeviceFile f in files) top = Math.Max(top, pick[i](f).Revision);
                parts[i].Revision = top + 1; parts[i].Origin = me;
            }

            if (!canApply)
            {
                // during a run: this device's own changes go up exactly as it holds them; nothing from the other devices is merged in or
                // applied (the menu does that). An unchanged part keeps the record this device last wrote, so a setting or a find made
                // elsewhere never holds back the run's uploads.
                bool upload = !listed.ContainsKey(me);
                for (int i = 0; i < 3; i++) { upload |= changed[i]; parts[i].LastHash = Hash(payload[i]); }
                if (upload) await backend.WriteDevice(me, JsonUtility.ToJson(DeviceFile(parts, payload)));
                state.LastSyncUtc = DateTime.UtcNow.ToString("o");
                return result;
            }

            // each part's winner: the newest change
            var winners = new CloudRecord[3];
            var winnerFile = new CloudDeviceFile[3];
            for (int i = 0; i < 3; i++)
            {
                winners[i] = new CloudRecord { Revision = parts[i].Revision, Origin = parts[i].Origin, Payload = payload[i] };
                foreach (CloudDeviceFile f in files)
                {
                    if (i == 0 && !profiles.ContainsKey(f)) continue;
                    CloudRecord r = pick[i](f);
                    if (Newer(r.Revision, r.Origin, winners[i].Revision, winners[i].Origin)) { winners[i] = r; winnerFile[i] = f; }
                }
            }
            PlayerProfile progressWinner = winnerFile[0] != null ? profiles[winnerFile[0]] : mine;
            var others = new List<PlayerProfile>();
            if (winnerFile[0] != null) others.Add(Clone(mine));
            foreach (var kv in profiles) if (kv.Key != winnerFile[0]) others.Add(kv.Value);
            PlayerProfile merged = Merge(progressWinner, others);
            PlayerProfile settings = null;
            if (winnerFile[1] != null) try { settings = JsonUtility.FromJson<PlayerProfile>(winners[1].Payload); } catch { }
            CopyPortable(settings ?? mine, merged);
            CopyLocal(mine, merged);
            if (Validated(JsonUtility.ToJson(merged)) == null) merged = Clone(mine); // a merge the save would refuse: keep this device's own
            string[] mergedPayload = { ProgressJson(merged), SettingsJson(merged), InPlay(winners[2].Payload) };
            bool needWrite = !listed.ContainsKey(me);
            for (int i = 0; i < 3; i++)
            {
                parts[i].Revision = winners[i].Revision; parts[i].Origin = winners[i].Origin;
                string hash = Hash(mergedPayload[i]);
                needWrite |= hash != parts[i].LastHash || changed[i];
                parts[i].LastHash = hash;
            }
            if (needWrite) await backend.WriteDevice(me, JsonUtility.ToJson(DeviceFile(parts, mergedPayload)));
            state.LastSyncUtc = DateTime.UtcNow.ToString("o");
            if (mergedPayload[0] != payload[0] || mergedPayload[1] != payload[1]) result.Profile = merged;
            if (mergedPayload[2] != payload[2]) result.Run = mergedPayload[2];
            return result;
        }

        private static CloudDeviceFile DeviceFile(CloudPartState[] parts, string[] payload) => new CloudDeviceFile
        {
            DeviceName = SystemInfo.deviceModel ?? "", WrittenUtc = DateTime.UtcNow.ToString("o"),
            Progress = new CloudRecord { Revision = parts[0].Revision, Origin = parts[0].Origin, Payload = payload[0] },
            Settings = new CloudRecord { Revision = parts[1].Revision, Origin = parts[1].Origin, Payload = payload[1] },
            Run = new CloudRecord { Revision = parts[2].Revision, Origin = parts[2].Origin, Payload = payload[2] },
        };

        /// <summary>A result that was never applied (the player left the menu, or changed the save while it synced): the sync state goes
        /// back to before it, so the next sync still treats the other devices' changes as new instead of taking this device's older copy
        /// for a newer change and winning with it.</summary>
        public static void Abandon(CloudSyncState state, CloudResult result)
        {
            if (state == null || result == null || string.IsNullOrEmpty(result.StateBefore) || result.Profile == null && result.Run == null) return;
            JsonUtility.FromJsonOverwrite(result.StateBefore, state);
        }
    }
}
