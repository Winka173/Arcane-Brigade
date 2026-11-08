using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Cloud save, simulated: devices share one in-memory backend (nothing leaves the process).</summary>
    public static class CloudSyncChecks
    {
        private sealed class Device
        {
            public readonly CloudSyncState State = new CloudSyncState { Enabled = true };
            public readonly Dictionary<string, CloudDeviceFile> Cache = new Dictionary<string, CloudDeviceFile>();
            public PlayerProfile Profile = new PlayerProfile();
            public string Run = "";
            public CloudResult Last;
            public Device(string id) { State.DeviceId = id; MetaProgression.Ensure(Profile); }
            public CloudResult Sync(ICloudBackend backend, bool canApply = true, CloudChoice choice = CloudChoice.None)
            {
                Last = CloudSave.Sync(backend, State, new CloudLocal { Profile = CloudSave.Clone(Profile), Run = Run }, Cache, canApply, choice).GetAwaiter().GetResult();
                if (Last.Profile != null) JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(Last.Profile), Profile);
                if (Last.Run != null) Run = Last.Run;
                return Last;
            }
            /// <summary>A menu sync whose result never lands (the player started a run while it was in flight).</summary>
            public void Dropped(ICloudBackend backend)
            {
                CloudResult result = CloudSave.Sync(backend, State, new CloudLocal { Profile = CloudSave.Clone(Profile), Run = Run }, Cache, true).GetAwaiter().GetResult();
                CloudSave.Abandon(State, result);
            }
            /// <summary>A wave's end: the run and its earnings saved, then the in-run upload.</summary>
            public CloudResult Wave(ICloudBackend backend, string runId, int wave, int oil, SessionPhase phase = SessionPhase.Shop)
            {
                Run = RunAt(runId, wave, phase); Profile.BestWave = Mathf.Max(Profile.BestWave, wave);
                Profile.Meta.LampOil += oil; Profile.Meta.LampOilEarned += oil;
                return Sync(backend, false);
            }
            /// <summary>The progress as the game reads it back (the save's own load upgrades included).</summary>
            public string Progress => CloudSave.Hash(CloudSave.ProgressJson(CloudSave.Validated(JsonUtility.ToJson(Profile)) ?? Profile));
        }
        private static string RunAt(string id, int wave, SessionPhase phase = SessionPhase.Shop) => JsonUtility.ToJson(new RunSnapshot { RunId = id, Wave = wave, Phase = phase });
        private static int WaveOf(string run) => string.IsNullOrEmpty(run) ? 0 : JsonUtility.FromJson<RunSnapshot>(run).Wave;
        private static CloudDeviceFile Uploaded(MemoryCloudBackend drive, string device) => JsonUtility.FromJson<CloudDeviceFile>(drive.Devices[device]);

        [MenuItem("Arcane Brigade/Checks/Cloud sync")]
        public static void Run()
        {
            var notes = new List<string>();
            void Check(bool ok, string caption)
            {
                if (!ok) throw new InvalidOperationException("Cloud sync: " + caption);
                notes.Add("PASS " + caption);
            }
            var drive = new MemoryCloudBackend();
            var a = new Device("aaaa"); var b = new Device("bbbb");

            // a device with progress, a new one: the new one takes the progress and the portable settings; its screen settings stay
            a.Profile.BestWave = 7; a.Profile.SeenUnits.Add("lantern-knight"); a.Profile.Meta.LampOil = 120; a.Profile.Volume = .2f; a.Profile.FrameCap = 60;
            b.Profile.FrameCap = 30; b.Profile.MobileSharpness = 1;
            a.Sync(drive); b.Sync(drive);
            Check(b.Profile.BestWave == 7 && b.Profile.SeenUnits.Contains("lantern-knight") && b.Profile.Meta.LampOil == 120, "a new device takes the progress");
            Check(Mathf.Approximately(b.Profile.Volume, .2f), "portable settings travel");
            Check(b.Profile.FrameCap == 30 && b.Profile.MobileSharpness == 1, "screen settings stay on their device");
            var uploaded = JsonUtility.FromJson<CloudDeviceFile>(drive.Devices["aaaa"]);
            Check(JsonUtility.FromJson<PlayerProfile>(uploaded.Progress.Payload).FrameCap == new PlayerProfile().FrameCap && JsonUtility.FromJson<PlayerProfile>(uploaded.Settings.Payload).FrameCap == new PlayerProfile().FrameCap, "screen settings are never uploaded");
            Check(a.Progress == b.Progress, "both devices hold the same progress");

            // a setting changed on one device reaches the other
            b.Profile.Language = GameLocalization.Vietnamese; b.Sync(drive); a.Sync(drive);
            Check(a.Profile.Language == GameLocalization.Vietnamese, "a changed setting reaches the other device");

            // a spend reaches the other device; the other's old, unchanged copy never wins it back
            b.Profile.Meta.LampOil = 40; b.Sync(drive); a.Sync(drive);
            Check(a.Profile.Meta.LampOil == 40, "a spend reaches the other device");
            a.Sync(drive); b.Sync(drive);
            Check(a.Profile.Meta.LampOil == 40 && b.Profile.Meta.LampOil == 40, "a stale copy never undoes the spend");

            // nothing changed: nothing written
            string version = drive.ListDevices().Result["aaaa"];
            a.Sync(drive);
            Check(drive.ListDevices().Result["aaaa"] == version, "an unchanged device writes nothing");

            // both changed the progress: nothing is overwritten until the player picks; finds from both are kept either way
            a.Profile.SeenItems.Add("moth-lamp"); a.Profile.Meta.LampOil = 300;
            b.Profile.SeenItems.Add("tin-crown"); b.Profile.Meta.LampOil = 10; b.Profile.BestWave = 12;
            a.Sync(drive);
            string bFile = drive.Devices["bbbb"];
            CloudResult clash = b.Sync(drive);
            Check(clash.Conflict != null && clash.Conflict.Progress && clash.Profile == null && drive.Devices["bbbb"] == bFile && b.Profile.Meta.LampOil == 10, "a clash is reported and nothing is overwritten");
            Check(clash.Conflict.CloudProfile != null && clash.Conflict.CloudProfile.Meta.LampOil == 300, "the clash shows the Drive side");
            b.Sync(drive, true, CloudChoice.UseCloud);
            Check(b.Profile.Meta.LampOil == 300 && b.Profile.SeenItems.Contains("tin-crown") && b.Profile.SeenItems.Contains("moth-lamp") && b.Profile.BestWave == 12, "use the Drive save: its oil, finds from both");
            a.Sync(drive);
            Check(a.Progress == b.Progress && a.Profile.BestWave == 12, "both devices agree after the choice");

            a.Profile.Meta.LampOil = 5; b.Profile.Meta.LampOil = 77;
            a.Sync(drive); Check(b.Sync(drive).Conflict != null, "a second clash is reported");
            b.Sync(drive, true, CloudChoice.KeepThisDevice); a.Sync(drive);
            Check(a.Profile.Meta.LampOil == 77 && b.Profile.Meta.LampOil == 77, "keep this device: it leads on every device");

            // the run in play travels; an older moment of it never wins back
            a.Run = RunAt("run-1", 3); a.Sync(drive); b.Sync(drive);
            Check(b.Run.Contains("run-1") && JsonUtility.FromJson<RunSnapshot>(b.Run).Wave == 3, "the run in play reaches the other device");
            b.Run = RunAt("run-1", 5); b.Sync(drive); a.Sync(drive);
            Check(JsonUtility.FromJson<RunSnapshot>(a.Run).Wave == 5, "the run continued elsewhere comes back");
            b.Run = ""; b.Sync(drive); a.Sync(drive);
            Check(a.Run == "", "a run ended elsewhere leaves play here too");
            a.Run = RunAt("run-2", 2); b.Run = RunAt("run-3", 4); a.Sync(drive);
            Check(b.Sync(drive).Conflict?.Run == true, "two different runs in play: the player picks");
            b.Sync(drive, true, CloudChoice.KeepThisDevice); a.Sync(drive);
            Check(a.Run.Contains("run-3"), "the picked run reaches the other device");

            // during a run: this device's own changes go up, nothing from elsewhere is applied; a find made elsewhere holds nothing back
            b.Profile.SeenBosses.Add("bell"); b.Sync(drive);
            a.Profile.BestWave = 14;
            CloudResult inRun = a.Sync(drive, false);
            Check(!inRun.Deferred && inRun.Profile == null && inRun.Run == null && !a.Profile.SeenBosses.Contains("bell") && JsonUtility.FromJson<PlayerProfile>(Uploaded(drive, "aaaa").Progress.Payload).BestWave == 14, "in a run: own change uploaded, nothing applied");
            a.Sync(drive); b.Sync(drive);
            Check(a.Profile.SeenBosses.Contains("bell") && a.Profile.BestWave == 14 && b.Profile.BestWave == 14 && b.Profile.SeenBosses.Contains("bell"), "back in the menu both changes meet on both devices");
            b.Sync(drive);
            a.Profile.Wins = 3;
            CloudResult checkpoint = a.Sync(drive, false);
            Check(!checkpoint.Deferred && JsonUtility.FromJson<CloudDeviceFile>(drive.Devices["aaaa"]).Progress.Payload.Contains("\"Wins\":3"), "in a run: a checkpoint uploads");

            // a third device joins late and gets everything
            var c = new Device("cccc"); c.Sync(drive);
            Check(c.Progress == a.Progress, "a third device catches up");

            // offline: an error, and the device's save untouched
            drive.FailWith = CloudFailure.Offline;
            c.Profile.BestWave = 30;
            bool threw = false;
            try { c.Sync(drive); } catch (CloudSyncException exception) { threw = exception.Failure == CloudFailure.Offline; }
            Check(threw && c.Profile.BestWave == 30, "offline: reported, save untouched");
            drive.FailWith = null;
            c.Sync(drive); a.Sync(drive);
            Check(a.Profile.BestWave == 30, "the offline change goes once back online");

            // a newer version's file is left alone and reported; a damaged file never reaches a profile
            drive.WriteDevice("zzzz", JsonUtility.ToJson(new CloudDeviceFile { Format = CloudDeviceFile.CurrentFormat + 1 })).Wait();
            a.Sync(drive);
            Check(a.Last.NewerSeen && a.Profile.BestWave == 30, "a newer version's file is reported and ignored");
            drive.WriteDevice("yyyy", JsonUtility.ToJson(new CloudDeviceFile { Progress = new CloudRecord { Revision = 5000, Origin = "yyyy", Payload = "{\"Version\":1,\"BestWave\":-4}" } })).Wait();
            a.Sync(drive);
            Check(a.Profile.BestWave == 30 && a.Last.Conflict == null, "a damaged file is refused");

            // joining: an empty phone first, the phone that played second, it leads; two played phones joining: the player picks
            var fresh = new MemoryCloudBackend();
            var empty = new Device("ffff"); var played = new Device("0000");
            played.Profile.Meta.LampOil = 77; played.Profile.Meta.LampOilEarned = 500; played.Profile.BestWave = 9;
            empty.Sync(fresh); played.Sync(fresh); empty.Sync(fresh);
            Check(played.Profile.Meta.LampOil == 77 && empty.Profile.Meta.LampOil == 77 && empty.Profile.BestWave == 9, "the phone that played leads when it joins second");
            var other = new Device("1111"); other.Profile.Meta.LampOilEarned = 50; other.Profile.Meta.LampOil = 20; other.Profile.BestWave = 3;
            Check(other.Sync(fresh).Conflict != null && other.Profile.Meta.LampOil == 20, "two played phones joining: the player picks");

            // ---- journeys players make ----
            var home = new MemoryCloudBackend();
            var phone = new Device("p100"); var tablet = new Device("t200");
            // the requirement 8/11: a run left at wave 10 is continued on the other device to wave 20 and won; back on the first, the reward is there
            for (int wave = 1; wave <= 10; wave++) phone.Wave(home, "journey", wave, 6);
            phone.Sync(home); // out to the menu
            tablet.Sync(home);
            Check(WaveOf(tablet.Run) == 10 && tablet.Profile.Meta.LampOil == 60 && tablet.Profile.BestWave == 10, "journey: the run left at wave 10 continues on the other device with its oil");
            bool everyWaveUp = true;
            for (int wave = 11; wave < 20; wave++) everyWaveUp &= !tablet.Wave(home, "journey", wave, 10).Deferred && WaveOf(Uploaded(home, "t200").Run.Payload) == wave;
            Check(everyWaveUp, "journey: every wave on the second device reaches Drive");
            tablet.Profile.Meta.Keepsakes += 3; tablet.Profile.Meta.KeepsakesEarned += 3; tablet.Profile.Wins += 1;
            tablet.Wave(home, "journey", 20, 400, SessionPhase.Victory); // the win credited, then the app closed on the results screen
            phone.Sync(home);
            Check(phone.Profile.Meta.LampOil == 550 && phone.Profile.Meta.Keepsakes == 3 && phone.Profile.Wins == 1 && phone.Profile.BestWave == 20 && phone.Run == "", "journey: back on the first device the oil, keepsakes and win are there and the finished run left play");
            Check(phone.Progress == tablet.Progress, "journey: both devices hold the same progress");

            // the first device opens and the player starts a run before the sync lands: the spend made elsewhere is not undone
            tablet.Profile.Meta.LampOil -= 200; tablet.Sync(home); // a Lamp Tree purchase on the tablet
            phone.Dropped(home);
            phone.Wave(home, "next", 1, 0);
            phone.Sync(home);
            Check(phone.Profile.Meta.LampOil == 350 && WaveOf(phone.Run) == 1, "a sync that never landed: the other device's spend still arrives, the new run stays");
            tablet.Sync(home);
            Check(tablet.Profile.Meta.LampOil == 350 && WaveOf(tablet.Run) == 1, "a sync that never landed: the other device agrees");

            // a run finished on one device and a new one started there at once: the other device gets the new run, its old one leaves play
            tablet.Wave(home, "next", 4, 30, SessionPhase.Defeat); tablet.Sync(home);
            tablet.Wave(home, "after", 2, 5); tablet.Sync(home);
            phone.Sync(home);
            Check(phone.Run.Contains("after") && WaveOf(phone.Run) == 2 && phone.Profile.Meta.LampOil == 385, "a new run started right after a finished one reaches the other device");

            // a setting changed on one device while the other plays: the run's uploads continue, the setting arrives in the menu
            phone.Profile.Volume = .55f; phone.Sync(home);
            CloudResult wave3 = tablet.Wave(home, "after", 3, 5);
            Check(!wave3.Deferred && WaveOf(Uploaded(home, "t200").Run.Payload) == 3, "a setting changed elsewhere never holds back a run's uploads");
            tablet.Sync(home);
            Check(Mathf.Approximately(tablet.Profile.Volume, .55f) && WaveOf(tablet.Run) == 3, "the setting arrives back in the menu, the run kept");
            phone.Sync(home);

            // a spend on one device while the other plays: the run's device holds its uploads and the menu asks; nothing is overwritten
            phone.Profile.Meta.LampOil -= 100; phone.Sync(home);
            string phoneFile = home.Devices["p100"];
            Check(tablet.Wave(home, "after", 4, 50).Deferred && home.Devices["p100"] == phoneFile, "a spend elsewhere during a run: the run's device waits, nothing overwritten");
            CloudResult ask = tablet.Sync(home);
            Check(ask.Conflict != null && ask.Conflict.Progress && tablet.Profile.Meta.LampOil == 440, "a spend elsewhere during a run: the menu asks");
            tablet.Sync(home, true, CloudChoice.KeepThisDevice); phone.Sync(home);
            Check(phone.Profile.Meta.LampOil == 440 && WaveOf(phone.Run) == 4, "keep the device that played: its oil and run lead");

            // the same run played on both devices (one offline): never merged, the player picks
            home.FailWith = CloudFailure.Offline;
            try { phone.Wave(home, "after", 5, 10); } catch (CloudSyncException) { }
            home.FailWith = null;
            tablet.Wave(home, "after", 7, 10); tablet.Sync(home);
            CloudResult twice = phone.Sync(home);
            Check(twice.Conflict != null && twice.Conflict.Run && WaveOf(phone.Run) == 5, "the same run moved on both devices: the player picks");
            phone.Sync(home, true, CloudChoice.UseCloud);
            Check(WaveOf(phone.Run) == 7, "use the Drive save: its later wave");

            // a run ended while offline: it reaches Drive once online
            home.FailWith = CloudFailure.Offline;
            try { phone.Wave(home, "after", 9, 70, SessionPhase.Defeat); } catch (CloudSyncException) { }
            home.FailWith = null;
            phone.Sync(home); tablet.Sync(home);
            Check(tablet.Run == "" && tablet.Profile.Meta.LampOil == phone.Profile.Meta.LampOil && tablet.Profile.BestWave == 20, "a run ended offline reaches the other device once online");

            // reinstalled: a new install with an empty save takes everything back
            var again = new Device("r300"); again.Sync(home);
            Check(again.Progress == phone.Progress && again.Profile.Meta.LampOil == phone.Profile.Meta.LampOil, "a reinstalled game takes the whole save back");

            // disconnect and connect again on a device that played: same save, no question
            phone.State.Forget(); phone.Cache.Clear();
            Check(phone.Sync(home).Conflict == null && phone.Progress == tablet.Progress, "connecting again after a disconnect: no question when nothing differs");

            // restore points: newest first, the same save twice kept once (the diagnostic profile folder, never the player's)
            Check(RunPaths.Diagnostic, "restore points are tried in the diagnostic folder");
            string folder = System.IO.Path.Combine(RunPaths.Root, "CloudBackups");
            if (System.IO.Directory.Exists(folder)) System.IO.Directory.Delete(folder, true);
            CloudSync.Backup("first", "test", CloudSave.ProgressJson(a.Profile), "");
            System.Threading.Thread.Sleep(5);
            CloudSync.Backup("second", "test", CloudSave.ProgressJson(b.Profile), a.Run);
            CloudSync.Backup("second again", "test", CloudSave.ProgressJson(b.Profile), a.Run);
            List<CloudBackup> points = CloudSync.Backups();
            Check(points.Count == 2 && points[0].Reason == "second" && CloudSync.BackupCount == 2, "restore points: newest first, no repeats");
            System.IO.Directory.Delete(folder, true);

            Debug.Log("Cloud sync checks\n" + string.Join("\n", notes));
        }
    }
}
