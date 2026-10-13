using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Native check of the effects sandbox: -arcane-fx-sandbox-capture DIR opens COLLECTION, then the sandbox, plays a few
    /// entries, runs the LIVE FIGHT for three bonds (Storm 6, Ember 8, Frost 4) a few seconds each, comes back, and writes PNG screenshots
    /// and FxSandbox.txt into DIR, closes it again and quits (isolated diagnostics save).</summary>
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
            box.ShowLiveTab(); yield return Wait(.4f); yield return Shot("07-live-tab");
            // the LIVE FIGHT: a real fight per bond and step; the save folder it reads must stay as it was
            var before = Files(RunPaths.Root);
            // Ember has six friends, so its 8 falls back to its highest step; Tide shows a step-8 weather
            (string house, int step)[] fights = { ("Storm", 6), ("Ember", 8), ("Frost", 4), ("Tide", 8) };
            FxSandboxLive live = null; int shot = 8;
            foreach (var f in fights)
            {
                if (live == null) live = box.StartLive(f.house, f.step); else live.Configure(f.house, f.step);
                for (float t = 0; t < 15 && !live.InFight; t += .25f) yield return Wait(.25f);
                if (!live.InFight) { log.Add("LIVE " + f.house + " " + f.step + ": NO FIGHT " + live.Failure); continue; }
                log.Add("LIVE " + f.house + " asked " + f.step + ", running step " + FxSandboxLive.Step + " with " + FxSandboxLive.Team(f.house, FxSandboxLive.Step) + " friends: sandbox save " + (RunPaths.SandboxRoot != null) + ", only bond " + BondRuntime.OnlyBond + ", damage numbers " + PresentationSettings.DamageNumbers + ", boss " + (live.Owner.Simulation.Boss != null));
                bool slowShot = f.house == "Frost";
                yield return Wait(4.5f); yield return Shot(shot.ToString("00") + "-live-" + f.house.ToLowerInvariant() + FxSandboxLive.Step + "-a");
                if (slowShot) live.SetSlow(true);
                yield return Wait(3f); yield return Shot(shot++.ToString("00") + "-live-" + f.house.ToLowerInvariant() + FxSandboxLive.Step + (slowShot ? "-b-slow" : "-b"));
                CombatSimulation sim = live.Owner.Simulation;
                if (f.house == "Ember") { live.SetPaused(true); yield return Wait(.3f); float t0 = sim.Elapsed; yield return Wait(.6f); log.Add("  pause holds the fight: " + (sim.Elapsed == t0)); live.SetPaused(false); }
                live.SetSlow(false);
                var friends = sim.Units.Where(u => u.Faction != Faction.Enemy).ToList();
                log.Add("  phase " + sim.Phase + ", elapsed " + sim.Elapsed.ToString("0.0") + " s, enemies alive " + live.EnemiesAlive + ", enemy hits " + live.Hits + ", kills " + live.Kills
                    + ", friends " + friends.Count(u => u.Alive) + "/" + friends.Count + ", hero " + sim.Champion.Health.ToString("0") + "/" + sim.Champion.Definition.Health.ToString("0"));
                log.Add("  rules fired: " + string.Join(", ", live.Rules.Select(r => r.Key + " x" + r.Value)));
            }
            if (live != null) live.Leave();
            for (float t = 0; t < 15 && !(FxSandbox.Current != null && FindAnyObjectByType<RunMenu>() != null); t += .25f) yield return Wait(.25f);
            yield return Wait(1f); yield return Shot(shot++.ToString("00") + "-back-in-sandbox");
            menu = FindAnyObjectByType<RunMenu>();
            log.Add("back in sandbox: " + (FxSandbox.Current != null) + ", live tab " + (FxSandbox.Current != null && FxSandbox.Current.LiveTab) + ", menu page " + (menu != null ? menu.CurrentPage.ToString() : "none")
                + ", sandbox save off " + (RunPaths.SandboxRoot == null) + ", only bond cleared " + (BondRuntime.OnlyBond == null) + ", time scale " + Time.timeScale + ", live gone " + (FxSandboxLive.Current == null));
            var after = Files(RunPaths.Root);
            log.Add("save folder untouched: " + (before.Count == after.Count && before.All(p => after.TryGetValue(p.Key, out DateTime w) && w == p.Value)) + " (" + before.Count + " files)");
            box = FxSandbox.Current;
            if (box != null) box.Close();
            yield return Wait(.6f); yield return Shot(shot.ToString("00") + "-closed");
            log.Add("closed cleanly: " + !FxSandbox.IsOpen + ", menu page " + (menu != null ? menu.CurrentPage.ToString() : "none"));
            Finish(0);
        }
        private static Dictionary<string, DateTime> Files(string dir)
        {
            var map = new Dictionary<string, DateTime>();
            if (Directory.Exists(dir)) foreach (string f in Directory.GetFiles(dir, "*", SearchOption.AllDirectories)) map[f] = File.GetLastWriteTimeUtc(f);
            return map;
        }
        private void Finish(int code) { File.WriteAllLines(Path.Combine(root, "FxSandbox.txt"), log); Application.Quit(code); }
        private void OnDestroy() { Application.logMessageReceived -= OnLog; }
    }
}
