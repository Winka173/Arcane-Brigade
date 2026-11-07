using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class LocalizationChecks
    {
        [MenuItem("Arcane Brigade/Localization/Check English and Vietnamese")]
        public static void Run()
        {
            string previous = GameLocalization.Language;
            string directory = Path.Combine(Application.temporaryCachePath, "ArcaneLocalizationChecks", Guid.NewGuid().ToString("N"));
            var results = new List<string>();
            void Check(bool condition, string name)
            {
                results.Add((condition ? "PASS " : "FAIL ") + name);
                if (!condition) throw new InvalidOperationException(name);
            }
            try
            {
                var profile = new PlayerProfile { BestWave = 7, Wins = 2, TotalKills = 123 };
                Check(profile.Language == "en", "New profile defaults to English");
                var storage = new RunStorage(directory);
                storage.SaveProfile(profile);
                profile = storage.LoadProfile();
                Check(profile.Language == "en", "English survives profile round trip");
                GameLocalization.SetLanguage("en");
                Check(GameLocalization.Text("Master volume") == "Master volume", "English strings are byte-for-byte unchanged");
                Check(SettingsSheet.TrySetLanguage(profile, "vi", () => { storage.SaveProfile(profile); return true; }), "Settings saves Vietnamese");
                Check(GameLocalization.Language == "vi", "Settings applies language immediately");
                Check(GameLocalization.Text("Language") == "Ngôn ngữ" && GameLocalization.Text("SETTINGS") == "CÀI ĐẶT", "Vietnamese settings labels");
                Check(GameLocalization.Text("English") == "English" && GameLocalization.Text("Tiếng Việt") == "Tiếng Việt", "Both language choices keep their native names");
                Check(GameLocalization.Text("Wave 12") == "Đợt 12", "Composed HUD text preserves numbers");
                Check(GameLocalization.Text("<color=#ffcc00>Wave 12</color>") == "<color=#ffcc00>Đợt 12</color>", "Unity rich-text markup is preserved");
                Check(GameLocalization.Text("{mark}Damage\n{bonus:Health|Health|+5}") == "{mark}Sát thương\n{bonus:Health|Health|+5}", "Glossary markup and stat IDs survive whole-sentence translation");
                Check(GameLocalization.Text("not-in-the-catalogue-xyz") == "not-in-the-catalogue-xyz", "Unknown text falls back to source");
                Check(GameLocalization.Text(GameLocalization.Text("Master volume")) == "Âm lượng tổng", "Measured text is not translated twice");
                var loaded = storage.LoadProfile();
                Check(loaded.Language == "vi", "Vietnamese survives restart");
                Check(loaded.BestWave == 7 && loaded.Wins == 2 && loaded.TotalKills == 123, "Language selection preserves progression");
                Check(!SettingsSheet.TrySetLanguage(profile, "en", () => false) && profile.Language == "vi" && GameLocalization.Language == "vi", "Failed save rolls language back");
                Check(SettingsSheet.TrySetLanguage(profile, "en", () => { storage.SaveProfile(profile); return true; }), "Settings switches back to English");
                Check(GameLocalization.Text("SETTINGS") == "SETTINGS" && storage.LoadProfile().Language == "en", "Switching back clears Vietnamese display and persists English");
                profile.Language = "unsupported"; storage.SaveProfile(profile);
                Check(storage.LoadProfile().Language == "en", "Unsupported saved language falls back to English");
                string oldJson = JsonUtility.ToJson(new PlayerProfile()).Replace("\"Language\":\"en\",", "");
                Check(GameLocalization.NormalizeLanguage(JsonUtility.FromJson<PlayerProfile>(oldJson).Language) == "en", "Older profile without language defaults to English");
                var table = JsonUtility.FromJson<GameLocalization.Table>(Resources.Load<TextAsset>("Localization/vi").text);
                Check(table.entries.Length > 7000, "Offline Vietnamese catalogue covers authored runtime and content text");
                Check(table.entries.Select(e => e.source).Distinct().Count() == table.entries.Length, "Translation keys are unique");
                Check(table.entries.All(e => !string.IsNullOrWhiteSpace(e.source) && !string.IsNullOrWhiteSpace(e.target)), "No empty translation entries");
                var glyphs = table.entries.SelectMany(e => e.target).Where(c => char.IsLetter(c) && c > 127).Distinct().ToArray();
                foreach (Font font in new[] { ArcadeTypography.Body, ArcadeTypography.Heading, ArcadeTypography.Display, ArcadeTypography.Hand, ArcadeTypography.Numbers })
                {
                    font.RequestCharactersInTexture(new string(glyphs), 32);
                    Check(glyphs.All(font.HasCharacter), "Vietnamese glyph coverage: " + font.name);
                }
                var catalog = Resources.Load<RunCatalog>("RunCatalog");
                string name = catalog.Champions[0].DisplayName;
                GameLocalization.SetLanguage("vi"); GameLocalization.Text(name);
                Check(catalog.Champions[0].DisplayName == name, "Localization never mutates gameplay assets");
                results.Add("Entries: " + table.entries.Length);
            }
            finally
            {
                GameLocalization.SetLanguage(previous);
                Directory.CreateDirectory("Docs/Verification");
                File.WriteAllLines("Docs/Verification/LocalizationChecks.txt", results);
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
            Debug.Log("LOCALIZATION_CHECKS_PASS\n" + string.Join("\n", results));
        }

        public static void CheckAndBuild()
        {
            Run();
            Directory.CreateDirectory("Builds/Localization");
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(),
                locationPathName = "Builds/Localization/ArcaneBrigade.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });
            File.WriteAllText("Docs/Verification/LocalizationBuild.txt", "Result: " + report.summary.result + "\nErrors: " + report.summary.totalErrors + "\nWarnings: " + report.summary.totalWarnings + "\n");
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Localized Windows build failed");
        }
    }
}
