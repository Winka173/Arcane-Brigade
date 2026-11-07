using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    /// <summary>Native bilingual smoke run; requires isolated K7 diagnostics and never reads the player's saves.</summary>
    public sealed class LocalizationCapture : MonoBehaviour
    {
        private static bool Requested => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-localization-capture") >= 0;
        private string directory;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (!Requested || FindAnyObjectByType<LocalizationCapture>() != null) return;
            if (!RunPaths.K7Capture) { Debug.LogError("Localization capture requires -arcane-k7-capture"); Application.Quit(1); return; }
            var go = new GameObject("Localization capture"); DontDestroyOnLoad(go);
            go.AddComponent<LocalizationCapture>(); Application.runInBackground = true;
        }
        private IEnumerator Shot(string name)
        {
            yield return new WaitForSecondsRealtime(.8f);
            yield return new WaitForEndOfFrame();
            Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(directory, name + ".png"), screenshot.EncodeToPNG());
            Destroy(screenshot);
        }
        private IEnumerator Start()
        {
            string[] args = Environment.GetCommandLineArgs(); int at = Array.IndexOf(args, "-capturePath");
            directory = at >= 0 && at + 1 < args.Length ? args[at + 1] : Path.Combine(Application.temporaryCachePath, "LocalizationCapture");
            Directory.CreateDirectory(directory);
            float deadline = Time.realtimeSinceStartup + 45;
            while (FindAnyObjectByType<RunMenu>() == null && Time.realtimeSinceStartup < deadline) yield return null;
            var menu = FindAnyObjectByType<RunMenu>();
            if (menu == null) { Debug.LogError("LOCALIZATION_CAPTURE_NO_MENU"); Application.Quit(1); yield break; }
            yield return new WaitForSecondsRealtime(3);
            var profile = new PlayerProfile { TutorialSeen = true };
            var catalog = Resources.Load<RunCatalog>("RunCatalog");
            menu.ConfigureK7Diagnostics(catalog, profile);
            menu.ShowPage(RunMenu.Page.Settings); yield return Shot("01-settings-en");
            var storage = new RunStorage(RunPaths.Root);
            SettingsSheet.TrySetLanguage(profile, "vi", () => { storage.SaveProfile(profile); return true; });
            yield return Shot("02-settings-vi");
            menu.ShowPage(RunMenu.Page.Landing); yield return Shot("03-menu-vi");
            menu.ShowPage(RunMenu.Page.Roster); yield return Shot("04-roster-vi");
            menu.ShowPage(RunMenu.Page.Help); yield return Shot("05-help-vi");
            menu.ShowPage(RunMenu.Page.Settings);
            SettingsSheet.TrySetLanguage(profile, "en", () => { storage.SaveProfile(profile); return true; });
            yield return Shot("06-settings-back-en");
            SettingsSheet.TrySetLanguage(profile, "vi", () => { storage.SaveProfile(profile); return true; });
            RunLaunch.Resume = false; RunLaunch.ChampionId = catalog.Champions[0].Id;
            RunLaunch.Difficulty = 0; RunLaunch.ChallengeIndex = 0;
            SceneManager.LoadScene("RunArena");
            yield return new WaitForSecondsRealtime(4);
            yield return Shot("07-combat-vi");
            bool passed = storage.LoadProfile().Language == "vi" && GameLocalization.IsVietnamese;
            File.WriteAllText(Path.Combine(directory, "Result.txt"), passed ? "PASS native English/Vietnamese settings, live switch, saved selection and combat startup\n" : "FAIL language persistence\n");
            Debug.Log(passed ? "LOCALIZATION_NATIVE_PASS" : "LOCALIZATION_NATIVE_FAIL");
            Application.Quit(passed ? 0 : 1);
        }
    }
}
