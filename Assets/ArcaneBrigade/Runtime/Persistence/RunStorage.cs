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
        public int Version = 1;
        public int BestWave;
        public int Wins;
        public int TotalKills;
        public int Cosmetic;
        public float Volume = 0.7f;
        public List<string> CompletedRuns = new List<string>();
        public List<string> SeenUnits = new List<string>();
        public List<string> SeenItems = new List<string>();
        public List<string> Achievements = new List<string>();
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
                    if (profile == null || profile.Version != 1 || profile.BestWave < 0 || profile.CompletedRuns == null || profile.SeenUnits == null || profile.SeenItems == null || profile.Achievements == null || !float.IsFinite(profile.Volume)) throw new InvalidOperationException("Invalid profile");
                    if (profile.Wins < 0 || profile.TotalKills < 0 || profile.Cosmetic < 0 || profile.Cosmetic > 1) throw new InvalidOperationException("Invalid profile counters");
                    profile.Volume = Mathf.Clamp01(profile.Volume);
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

        public void RecordProgress(PlayerProfile profile, RunSnapshot run)
        {
            profile.BestWave = Mathf.Max(profile.BestWave, run.Wave);
            foreach (RosterEntry entry in run.Roster) if (!profile.SeenUnits.Contains(entry.UnitId)) profile.SeenUnits.Add(entry.UnitId);
            foreach (ItemStack stack in run.Items) if (!profile.SeenItems.Contains(stack.Id)) profile.SeenItems.Add(stack.Id);
            if (run.BossKills > 0 && !profile.Achievements.Contains("First bell silenced")) profile.Achievements.Add("First bell silenced");
            if (run.Wave >= 10 && !profile.Achievements.Contains("Ten halls crossed")) profile.Achievements.Add("Ten halls crossed");
            bool terminal = run.Phase == SessionPhase.Defeat || run.Phase == SessionPhase.Victory;
            if (terminal && !profile.CompletedRuns.Contains(run.RunId))
            {
                profile.CompletedRuns.Add(run.RunId);
                profile.TotalKills = RunSession.BoundedCounter((long)profile.TotalKills + run.TotalKills);
                if (run.Phase == SessionPhase.Victory) { profile.Wins = RunSession.BoundedCounter((long)profile.Wins + 1); if (!profile.Achievements.Contains("Brigade dawn")) profile.Achievements.Add("Brigade dawn"); }
            }
            SaveProfile(profile);
        }
    }
}
