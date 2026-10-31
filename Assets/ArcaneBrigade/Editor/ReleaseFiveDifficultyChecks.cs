using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class ReleaseFiveDifficultyChecks
    {
        public static void Run()
        {
            var notes = new List<string>(); int failures = 0;
            void Check(bool condition, string label)
            {
                notes.Add((condition ? "PASS " : "FAIL ") + label);
                if (!condition) failures++;
            }
            var catalog = Resources.Load<RunCatalog>("RunCatalog");
            Check(RunMeta.DifficultyNames.SequenceEqual(new[] { "Peaceful", "Wandering", "Nightmare", "Sleepless", "The Last Night" }), "Five named difficulties in display order");
            Check(JourneyBalance.HealthFactor.Length == 5 && JourneyBalance.DamageFactor.Length == 5 && JourneyBalance.DensityFactor.Length == 5, "Every difficulty table has exactly five entries");
            foreach (int wave in new[] { 1, 4, 8, 15, 16, 20, 30, 50 })
                for (int d = 1; d < 5; d++)
                    Check(JourneyBalance.EnemyHealth(d, wave) > JourneyBalance.EnemyHealth(d - 1, wave)
                        && JourneyBalance.EnemyDamage(d, wave) > JourneyBalance.EnemyDamage(d - 1, wave)
                        && JourneyBalance.EnemyDensity(d, wave) > JourneyBalance.EnemyDensity(d - 1, wave), "Strict health/damage/density ladder " + d + "/" + wave);
            var fresh = new PlayerProfile();
            Check(RunMeta.UnlockedDifficulty(fresh) == 3, "New profile can select four ordinary difficulties; last waits for tree");
            var full = MetaProgression.BotProfile(1, catalog, "LanternKnight");
            Check(MetaProgression.TreeAllDone(full) && RunMeta.UnlockedDifficulty(full) == 4, "Full diagnostic Lamp Tree actually unlocks The Last Night");
            using (var run = new RunSession(catalog))
            {
                foreach (int d in Enumerable.Range(0, 5))
                {
                    run.Start("LanternKnight", 271026, d);
                    var saved = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(run.State));
                    using var restored = new RunSession(catalog); restored.Restore(saved);
                    Check(restored.State.Difficulty == d, "Current save round-trip difficulty " + d);
                }
                foreach (int d in new[] { -1, 5, 6, 7, 8 })
                {
                    bool rejected = false;
                    try { run.Start("LanternKnight", 271026, d); } catch (ArgumentException) { rejected = true; }
                    Check(rejected, "Reject invalid new-run difficulty " + d);
                }
                run.Start("LanternKnight", 271026);
                run.State.Wave = 18;
                Check(!run.NewAlliesClosed, "New friends still possible before the final recruitment boundary");
                run.State.Wave = 19;
                Check(!run.NewAlliesClosed, "The shop before wave 20 is the last open to new friends");
                run.State.Wave = 20;
                Check(run.NewAlliesClosed && run.NewRecruitsRemaining == 0 && run.SpecialNewRecruitsRemaining == 0, "Regular and dream new-species recruitment close after wave 20's shop");
            }
            int[] remap = { 0, 1, 1, 2, 3, 3, 4, 4 };
            for (int old = 0; old < remap.Length; old++)
            {
                var profile = new PlayerProfile { Meta = new MetaProgress { Revision = 4, WonMask = 1 << old } };
                MetaProgression.Ensure(profile);
                Check(profile.Meta.WonMask == 1 << remap[old], "Legacy profile victory bit remaps " + old + " -> " + remap[old]);
                int once = profile.Meta.WonMask; MetaProgression.Ensure(profile);
                Check(profile.Meta.WonMask == once, "Profile migration is idempotent " + old);
                using var run = new RunSession(catalog); run.Start("LanternKnight", 271026);
                var legacy = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(run.State));
                legacy.SchemaRevision = 5; legacy.Difficulty = old;
                legacy.DifficultyLayoutRevision = 0;
                string original = JsonUtility.ToJson(legacy);
                try
                {
                    using var restored = new RunSession(catalog); restored.Restore(legacy);
                    Check(restored.State.Difficulty == remap[old], "Legacy unfinished run remaps " + old + " -> " + remap[old]);
                    Check(JsonUtility.ToJson(legacy) == original, "Migration leaves input snapshot unchanged " + old);
                }
                catch (Exception e) { Check(false, "Legacy unfinished run " + old + " rejected: " + e.Message); }
            }
            string temporary = Path.Combine(Path.GetTempPath(), "AB-FiveMigration-" + Guid.NewGuid().ToString("N"));
            foreach (int schema in new[] { 3, 4 })
            {
                using var run = new RunSession(catalog); run.Start("LanternKnight", 271026);
                var legacy = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(run.State));
                legacy.SchemaRevision = schema; legacy.DifficultyLayoutRevision = 0; legacy.Difficulty = 6;
                legacy.Wave = 7; legacy.NextSpecialShopWave = 0;
                run.Restore(legacy);
                Check(run.State.Difficulty == 4 && run.State.SchemaRevision == RunSnapshot.CurrentSchemaRevision
                    && run.State.NextSpecialShopWave > run.State.Wave,
                    "Difficulty migration preserves older dream-shop migration schema " + schema);
            }
            try
            {
                var storage = new RunStorage(temporary);
                for (int old = 0; old < remap.Length; old++)
                {
                    using var run = new RunSession(catalog); run.Start("LanternKnight", 271026);
                    var legacy = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(run.State));
                    legacy.SchemaRevision = 5; legacy.DifficultyLayoutRevision = 0; legacy.Difficulty = old;
                    storage.SaveRun(legacy);
                    var loaded = storage.LoadRun(run);
                    Check(loaded != null && loaded.Difficulty == remap[old], "Checksummed legacy run loads from disk " + old);
                    if (loaded == null) continue;
                    run.Restore(loaded); storage.SaveRun(run.State);
                    var again = storage.LoadRun(run);
                    Check(again != null && again.Difficulty == remap[old] && again.SchemaRevision == RunSnapshot.CurrentSchemaRevision,
                        "Migrated run remains stable after save/load " + old);
                }
                using (var run = new RunSession(catalog))
                {
                    run.Start("LanternKnight", 271026, 0);
                    var history = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(run.State));
                    history.SchemaRevision = 5; history.DifficultyLayoutRevision = 0; history.Difficulty = 7;
                    history.Phase = SessionPhase.Victory; history.Wave = 20;
                    var legacyProfile = new PlayerProfile { Version = 4, LastDifficulty = 6,
                        Meta = new MetaProgress { Revision = 4, WonMask = 1 << 7, LampOil = 123, Keepsakes = 7 } };
                    legacyProfile.CompletedRuns.Add(history.RunId);
                    legacyProfile.History.Add(new RunRecord { Id = history.RunId, Nickname = "Migration fixture", Score = 200, Build = history });
                    legacyProfile.Records.Add(new ScoreRecord { Key = "LanternKnight/4/normal/", RunId = history.RunId, Hall = 20, Score = 100 });
                    legacyProfile.Records.Add(new ScoreRecord { Key = "LanternKnight/5/normal/", RunId = history.RunId, Hall = 20, Score = 200 });
                    legacyProfile.Achievements.AddRange(new[] { "clear-difficulty-4", "clear-difficulty-5", "journal-difficulty-7" });
                    legacyProfile.AchievementEvidence.Add(new AchievementMetric { Id = "difficulty", Value = 8 });
                    legacyProfile.Meta.Found.AddRange(new[] { "boss/4/CoinWidow", "boss/5/CoinWidow", "keeper/7" });
                    storage.SaveProfile(legacyProfile);
                    var migrated = storage.LoadProfile();
                    Check(migrated.Version == PlayerProfile.CurrentVersion && migrated.History.Count == 1 && migrated.History[0].Build.Difficulty == 4,
                        "Profile disk migration preserves legacy high-difficulty history");
                    Check(migrated.LastDifficulty == 4 && migrated.Meta.WonMask == 1 << 4, "Profile selection and victory bits agree after migration");
                    Check(migrated.Records.Count == 1 && migrated.Records[0].Key == "LanternKnight/3/normal/" && migrated.Records[0].Score == 200,
                        "Colliding difficulty score keys retain the best score");
                    Check(migrated.Achievements.Contains("clear-difficulty-3") && migrated.Achievements.Contains("journal-difficulty-4")
                        && migrated.AchievementEvidence.Single(m => m.Id == "difficulty").Value == 5,
                        "Difficulty achievements and progress migrate together");
                    Check(migrated.Meta.Found.Count == 2 && migrated.Meta.Found.Contains("boss/3/CoinWidow") && migrated.Meta.Found.Contains("keeper/4"),
                        "Previously paid boss/Keeper rewards remain paid after merged difficulties");
                    Check(migrated.Meta.LampOil == 123 && migrated.Meta.Keepsakes == 7 && migrated.CompletedRuns.SequenceEqual(legacyProfile.CompletedRuns),
                        "Migration preserves currencies and completed-run identity without paying rewards again");
                    string first = JsonUtility.ToJson(migrated); storage.SaveProfile(migrated);
                    Check(JsonUtility.ToJson(storage.LoadProfile()) == first, "Profile migration remains stable after repeated disk save/load");
                    Check(legacyProfile.LastDifficulty == 6 && history.Difficulty == 7, "Profile disk migration never edits the caller's old snapshot");
                }
            }
            finally
            {
                string resolved = Path.GetFullPath(temporary);
                if (resolved.StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase)
                    && Path.GetFileName(resolved).StartsWith("AB-FiveMigration-", StringComparison.Ordinal) && Directory.Exists(resolved))
                    Directory.Delete(resolved, true);
            }
            Directory.CreateDirectory("PreviewExports/ReleaseBalance/FiveDifficultyChecks");
            File.WriteAllLines("PreviewExports/ReleaseBalance/FiveDifficultyChecks/Checks.txt", notes);
            if (failures > 0) throw new InvalidOperationException("Five difficulties: " + failures + "/" + notes.Count + " checks failed; see FiveDifficultyChecks/Checks.txt");
            Debug.Log("FIVE_DIFFICULTY_CHECKS PASS " + notes.Count);
        }
    }
}
