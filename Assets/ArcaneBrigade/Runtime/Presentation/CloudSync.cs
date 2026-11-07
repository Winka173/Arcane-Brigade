using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade
{
    public enum CloudStatus { Off, Idle, Working, Offline, AuthNeeded, Quota, NewerVersion, Conflict, Failed }

    /// <summary>A restore point kept on this device: the progress and the run in play as they were before something replaced them.</summary>
    [Serializable]
    public sealed class CloudBackup { public string CreatedUtc = "", Reason = "", Device = "", Progress = "", Run = ""; [NonSerialized] public string Path; }

    /// <summary>Cloud save (requirement 7/11/2026, the same design as Sticky Wall): the progress, the settings that suit any device and
    /// the run in play go to the hidden app folder of the player's own Google Drive; there is no game server, and nothing needs a
    /// sign-in to play. Off until the player connects Google. With Auto sync it runs when the menu opens, when the game comes back to
    /// the front, a minute after a change, at checkpoints in a run (upload only) and when the game goes to the background; Sync now
    /// always works. A clash between two devices is never settled silently: the player picks a side and the other is kept as a
    /// restore point.</summary>
    public sealed class CloudSync : MonoBehaviour
    {
        private const string StateFile = "cloudsync.json", BackupFolder = "CloudBackups";
        private const int BackupsKept = 20;
        private static CloudSync instance;
        private static CloudSyncState state;
        private static PlayerProfile profile;
        private static RunStorage storage;
        private static bool menu; // the main menu is attached: merged data may be applied
        private static Action applied;
        private static Action<string> runArrived;
        private static ICloudBackend backend;
        private static readonly Dictionary<string, CloudDeviceFile> cache = new Dictionary<string, CloudDeviceFile>();
        private static bool working, again;
        private static CloudChoice nextChoice;
        private static float nextCheck, lastCheckpoint = -999;

        public static CloudStatus Status { get; private set; } = CloudStatus.Off;
        /// <summary>The clash waiting for the player's choice (Status Conflict).</summary>
        public static CloudConflict Conflict { get; private set; }
        /// <summary>Android, and iPhone once its OAuth client id is set; never in a diagnostic run (it would touch a real Drive).</summary>
        public static bool Offered => DriveAuth.Supported && !RunPaths.Diagnostic;
        public static bool Connected => State.Enabled;
        public static bool Auto => State.Auto;
        public static bool Busy => working;

        private static CloudSyncState State
        {
            get
            {
                if (state != null) return state;
                try
                {
                    string path = Path.Combine(RunPaths.Root, StateFile);
                    if (File.Exists(path)) state = JsonUtility.FromJson<CloudSyncState>(File.ReadAllText(path));
                }
                catch (Exception exception) { Debug.LogWarning("Sync state unreadable: " + exception.Message); }
                state ??= new CloudSyncState();
                state.Seen ??= new List<string>();
                state.Progress ??= new CloudPartState(); state.Settings ??= new CloudPartState(); state.Run ??= new CloudPartState();
                if (string.IsNullOrEmpty(state.DeviceId)) state.DeviceId = Guid.NewGuid().ToString("N");
                if (state.Enabled && Status == CloudStatus.Off) Status = CloudStatus.Idle;
                return state;
            }
        }
        private static void SaveState()
        {
            try
            {
                Directory.CreateDirectory(RunPaths.Root);
                string path = Path.Combine(RunPaths.Root, StateFile), temporary = path + ".tmp";
                File.WriteAllText(temporary, JsonUtility.ToJson(State));
                if (File.Exists(path)) File.Delete(path);
                File.Move(temporary, path);
            }
            catch (Exception exception) { Debug.LogWarning("Sync state not saved: " + exception.Message); }
        }
        private static void EnsureInstance()
        {
            if (instance != null) return;
            var holder = new GameObject("Cloud sync"); DontDestroyOnLoad(holder); instance = holder.AddComponent<CloudSync>();
        }

        /// <summary>The main menu hands over its profile; merged data is written into that same object and saved, then
        /// <paramref name="onApplied"/> lets the menu refresh; a run from another device goes to <paramref name="onRun"/> ("" = the run
        /// in play ended elsewhere).</summary>
        public static void Attach(PlayerProfile current, RunStorage currentStorage, Action onApplied, Action<string> onRun)
        {
            if (!Offered) return;
            profile = current; storage = currentStorage; applied = onApplied; runArrived = onRun; menu = true;
            EnsureInstance();
            if (State.Enabled && State.Auto) Request();
        }
        public static void Detach(PlayerProfile current) { if (ReferenceEquals(profile, current)) { profile = null; storage = null; applied = null; runArrived = null; menu = false; } }

        /// <summary>A checkpoint in a run (a wave, a purchase, the game going to the background): uploads at most once a minute, or at
        /// once when <paramref name="force"/>. Nothing from other devices is applied during a run.</summary>
        public static void Checkpoint(PlayerProfile current, RunStorage currentStorage, bool force)
        {
            if (!Offered || !State.Enabled || !State.Auto || menu || working) return;
            if (!force && Time.unscaledTime - lastCheckpoint < 60) return;
            lastCheckpoint = Time.unscaledTime;
            profile = current; storage = currentStorage;
            EnsureInstance();
            Run();
        }

        /// <summary>Connect Google: Google's own sheet; on yes, the save syncs at once.</summary>
        public static async void Connect()
        {
            if (!Offered || working) return;
            working = true; Status = CloudStatus.Working;
            string error = await DriveCloudBackend.Authorize();
            working = false;
            if (error != null)
            {
                Debug.LogWarning("Drive access not given: " + error);
                Status = State.Enabled ? CloudStatus.AuthNeeded : CloudStatus.Off;
                return;
            }
            if (!State.Enabled) { State.Forget(); State.Enabled = true; State.Auto = true; }
            SaveState();
            Request();
        }
        /// <summary>Disconnect: Google takes the access back and this device stops syncing. The Drive copy stays until the player
        /// deletes it (Drive settings, Manage apps); the save on this device is untouched.</summary>
        public static async void Disconnect()
        {
            State.Enabled = false; State.Forget(); cache.Clear(); Conflict = null; SaveState();
            Status = CloudStatus.Off;
            try { await DriveAuth.Revoke(); } catch (Exception exception) { Debug.LogWarning("Drive revoke: " + exception.Message); }
            backend = null;
        }
        public static void SetAuto(bool on) { State.Auto = on; SaveState(); if (on) Request(); }
        public static void SyncNow() { if (Status == CloudStatus.AuthNeeded) Connect(); else Request(); }
        /// <summary>The player's answer to a clash; the side not chosen becomes a restore point.</summary>
        public static void Resolve(CloudChoice choice)
        {
            if (Conflict == null || choice == CloudChoice.None || profile == null) return;
            if (choice == CloudChoice.UseCloud) Backup("Before using the Drive save", SystemInfo.deviceModel, CloudSave.ProgressJson(profile), CurrentRun());
            else Backup("Drive save not used", Conflict.CloudDevice, Conflict.CloudProfile != null ? CloudSave.ProgressJson(Conflict.CloudProfile) : "", Conflict.CloudRun);
            Conflict = null; nextChoice = choice;
            Request();
        }

        public static void Request()
        {
            if (!Offered || !State.Enabled || profile == null) return;
            if (working) { again = true; return; }
            Run();
        }

        private static string CurrentRun() => CloudSave.InPlay(storage?.ReadRunPayload());
        /// <summary>This device's side of a clash, for the choice card.</summary>
        public static PlayerProfile CurrentProfile => profile;
        public static string CurrentRunPayload => CurrentRun();

        private static async void Run()
        {
            working = true; again = false;
            Status = CloudStatus.Working;
            bool apply = menu;
            CloudChoice choice = nextChoice; nextChoice = CloudChoice.None;
            try
            {
                backend ??= new DriveCloudBackend();
                PlayerProfile current = profile;
                string run = CurrentRun();
                string before = CloudSave.ProgressJson(current) + CloudSave.SettingsJson(current) + run;
                CloudResult result = await CloudSave.Sync(backend, State, new CloudLocal { Profile = CloudSave.Clone(current), Run = run }, cache, apply, choice);
                SaveState();
                if (result.Conflict != null)
                {
                    Conflict = result.Conflict; Status = CloudStatus.Conflict;
                    if (menu) CloudSaveCard.OpenConflict();
                    return;
                }
                if (!result.Deferred && apply && ReferenceEquals(current, profile))
                {
                    if (CloudSave.ProgressJson(current) + CloudSave.SettingsJson(current) + CurrentRun() != before) again = true; // changed while syncing: it goes next
                    else ApplyResult(current, result);
                }
                Status = result.NewerSeen ? CloudStatus.NewerVersion : CloudStatus.Idle;
            }
            catch (CloudSyncException exception)
            {
                Debug.LogWarning("Sync: " + exception.Message);
                Status = exception.Failure switch
                {
                    CloudFailure.Offline => CloudStatus.Offline,
                    CloudFailure.AuthNeeded => CloudStatus.AuthNeeded,
                    CloudFailure.Quota => CloudStatus.Quota,
                    CloudFailure.NewerVersion => CloudStatus.NewerVersion,
                    _ => CloudStatus.Failed,
                };
            }
            catch (Exception exception) { Debug.LogWarning("Sync failed: " + exception); Status = CloudStatus.Failed; }
            finally
            {
                working = false;
                nextCheck = Time.unscaledTime + 60;
                if (again && State.Enabled && profile != null && Status != CloudStatus.Conflict) Run();
            }
        }

        private static void ApplyResult(PlayerProfile current, CloudResult result)
        {
            if (result.Profile != null)
            {
                string backup = JsonUtility.ToJson(current);
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(result.Profile), current);
                try { storage?.SaveProfile(current); PresentationSettings.Apply(current); applied?.Invoke(); }
                catch (Exception exception) { JsonUtility.FromJsonOverwrite(backup, current); throw new CloudSyncException(CloudFailure.Other, "local save: " + exception.Message); }
            }
            if (result.Run != null) runArrived?.Invoke(result.Run);
        }

        // ---- restore points ----

        private static string BackupDirectory => Path.Combine(RunPaths.Root, BackupFolder);
        /// <summary>Keeps a restore point (newest kept: 20; never one that is the same as the newest).</summary>
        public static void Backup(string reason, string device, string progress, string run)
        {
            if (string.IsNullOrEmpty(progress) && string.IsNullOrEmpty(run)) return;
            try
            {
                Directory.CreateDirectory(BackupDirectory);
                var item = new CloudBackup { CreatedUtc = DateTime.UtcNow.ToString("o"), Reason = reason, Device = device ?? "", Progress = progress ?? "", Run = run ?? "" };
                CloudBackup newest = Backups().FirstOrDefault();
                if (newest != null && newest.Progress == item.Progress && newest.Run == item.Run) return;
                File.WriteAllText(Path.Combine(BackupDirectory, DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + ".json"), JsonUtility.ToJson(item));
                backupCount = -1;
                foreach (string old in Directory.GetFiles(BackupDirectory, "*.json").OrderByDescending(f => f, StringComparer.Ordinal).Skip(BackupsKept)) File.Delete(old);
            }
            catch (Exception exception) { Debug.LogWarning("Restore point not kept: " + exception.Message); }
        }
        private static int backupCount = -1;
        /// <summary>How many restore points there are (read once, kept until one is added).</summary>
        public static int BackupCount { get { if (backupCount < 0) backupCount = Backups().Count; return backupCount; } }
        /// <summary>Restore points, newest first.</summary>
        public static List<CloudBackup> Backups()
        {
            var list = new List<CloudBackup>();
            try
            {
                if (!Directory.Exists(BackupDirectory)) return list;
                foreach (string file in Directory.GetFiles(BackupDirectory, "*.json").OrderByDescending(f => f, StringComparer.Ordinal))
                {
                    try { CloudBackup b = JsonUtility.FromJson<CloudBackup>(File.ReadAllText(file)); if (b != null) { b.Path = file; list.Add(b); } } catch { }
                }
            }
            catch { }
            return list;
        }
        /// <summary>Puts a restore point back (the menu only): what is replaced becomes a restore point itself; the restored save then
        /// syncs as this device's newest change.</summary>
        public static bool Restore(CloudBackup item)
        {
            if (item == null || profile == null || !menu) return false;
            PlayerProfile restored = string.IsNullOrEmpty(item.Progress) ? null : CloudSave.Validated(item.Progress);
            if (restored == null && string.IsNullOrEmpty(item.Run)) return false;
            Backup("Before restoring", SystemInfo.deviceModel, CloudSave.ProgressJson(profile), CurrentRun());
            if (restored != null)
            {
                CloudSave.CopyPortable(profile, restored); CloudSave.CopyLocal(profile, restored);
                string backup = JsonUtility.ToJson(profile);
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(restored), profile);
                try { storage?.SaveProfile(profile); applied?.Invoke(); }
                catch (Exception exception) { JsonUtility.FromJsonOverwrite(backup, profile); Debug.LogWarning("Restore not saved: " + exception.Message); return false; }
            }
            if (!string.IsNullOrEmpty(item.Run)) runArrived?.Invoke(item.Run);
            if (State.Enabled) { nextChoice = CloudChoice.KeepThisDevice; Request(); }
            return true;
        }

        // ---- the settings row ----

        public static string StatusLine()
        {
            switch (Status)
            {
                case CloudStatus.Off: return "Not connected";
                case CloudStatus.Working: return "Syncing…";
                case CloudStatus.Offline: return "Offline · syncs later";
                case CloudStatus.AuthNeeded: return "Allow access again";
                case CloudStatus.Quota: return "Google Drive is full";
                case CloudStatus.NewerVersion: return "Update the game to sync";
                case CloudStatus.Conflict: return "Two saves differ";
                case CloudStatus.Failed: return "Couldn't sync · save safe";
            }
            return LastSynced();
        }
        public static string LastSynced()
        {
            if (!DateTime.TryParse(State.LastSyncUtc, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime at)) return "Not synced yet";
            DateTime local = at.ToLocalTime();
            return "Synced " + (local.Date == DateTime.Now.Date ? local.ToString("HH:mm") : local.ToString("dd/MM HH:mm"));
        }

        private void Update()
        {
            if (profile == null || !menu || !State.Enabled || !State.Auto || working || Status == CloudStatus.Conflict || Time.unscaledTime < nextCheck) return;
            nextCheck = Time.unscaledTime + 60;
            if (CloudSave.Hash(CloudSave.ProgressJson(profile)) != State.Progress.LastHash || CloudSave.Hash(CloudSave.SettingsJson(profile)) != State.Settings.LastHash) Request();
        }
        private void OnApplicationPause(bool paused)
        {
            if (profile == null || !menu || !State.Enabled || !State.Auto || Status == CloudStatus.Conflict) return;
            if (!paused) Request(); // back to the front: another device may have played
            else if (!working) Request(); // best effort before the app sleeps
        }
    }
}
