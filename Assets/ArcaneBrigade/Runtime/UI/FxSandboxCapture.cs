using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Native check of the effects sandbox: -arcane-fx-sandbox-capture DIR opens COLLECTION, then the sandbox, plays a few
    /// entries and writes PNG screenshots and FxSandbox.txt into DIR, closes it again and quits (isolated diagnostics save).</summary>
    public sealed class FxSandboxCapture : MonoBehaviour
    {
        public static bool Requested => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-fx-sandbox-capture") >= 0;
        private string root; private readonly List<string> log = new List<string>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (!Requested || FindAnyObjectByType<FxSandboxCapture>() != null) return;
            var go = new GameObject("FxSandboxCapture"); DontDestroyOnLoad(go); go.AddComponent<FxSandboxCapture>();
        }
        private IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            Texture2D image = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(root, name + ".png"), image.EncodeToPNG()); Destroy(image); log.Add("shot " + name);
        }
        private void OnLog(string message, string trace, LogType type) { if (type == LogType.Exception || type == LogType.Error) log.Add("ERROR " + message + " | " + (trace ?? "").Split('\n')[0]); }
        private IEnumerator Wait(float seconds) { yield return new WaitForSecondsRealtime(seconds); }

        private IEnumerator Start()
        {
            string[] a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, "-arcane-fx-sandbox-capture");
            root = Path.GetFullPath(i + 1 < a.Length ? a[i + 1] : "FxSandboxCapture"); Directory.CreateDirectory(root);
            Application.runInBackground = true; Application.logMessageReceived += OnLog;
            log.Add("save root isolated: " + RunPaths.Root.Contains("ArcaneBrigadeDiagnostics"));
            RunMenu menu = null;
            for (float t = 0; t < 20 && menu == null; t += .25f) { yield return Wait(.25f); menu = FindAnyObjectByType<RunMenu>(); }
            if (menu == null) { log.Add("NO MENU"); Finish(1); yield break; }
            yield return Wait(4f);
            menu.ShowPage(RunMenu.Page.Museum); yield return Wait(1.5f); yield return Shot("01-collection");
            FxSandbox box = FxSandbox.Open(); yield return Wait(.6f);
            log.Add("entries " + FxSandbox.Entries.Count + ", registered rules " + new List<string>(FxDirector.Rules).Count + ", listed rules without effect " + box.Missing);
            box.Expand("Storm"); box.Expand("Ember");
            box.Play(box.Find("Storm", "storm-thunderbolt")); yield return Wait(.75f); yield return Shot("02-thunderbolt");
            box.Play(box.Find("Ember", "ember-meteor")); yield return Wait(.9f); yield return Shot("03-meteor");
            box.ClearAll(); box.ToggleWeather("frost-blizzard"); box.Play(box.Find("Damage numbers", "All elements")); yield return Wait(.35f); yield return Shot("04-blizzard-numbers");
            box.ClearAll(); box.Expand("Element combos");
            box.Play(box.Find("Element combos", "SteamBurst")); box.Play(box.Find("Element deaths", "Fire")); box.Play(box.Find("Enemy states", "Ice")); box.Play(box.Find("Status words", "All words"));
            yield return Wait(.5f); yield return Shot("05-reactions");
            box.ClearAll(); box.SetSlow(true); box.PlayWholeBond("Tide"); yield return Wait(4.2f); yield return Shot("06-tide-slow");
            box.Close(); yield return Wait(.6f); yield return Shot("07-closed");
            log.Add("closed cleanly: " + !FxSandbox.IsOpen + ", menu page " + menu.CurrentPage);
            Finish(0);
        }
        private void Finish(int code) { File.WriteAllLines(Path.Combine(root, "FxSandbox.txt"), log); Application.Quit(code); }
        private void OnDestroy() { Application.logMessageReceived -= OnLog; }
    }
}
