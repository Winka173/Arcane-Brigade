using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public sealed class RareVisitorCapture : MonoBehaviour
    {
        public static bool Requested => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-rare-visitors-capture") >= 0;
        private readonly List<string> checks = new List<string>();
        private string root; private bool failed;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (!Requested || FindAnyObjectByType<RareVisitorCapture>() != null) return;
            var go = new GameObject("RareVisitorCapture"); DontDestroyOnLoad(go); go.AddComponent<RareVisitorCapture>();
        }
        private void Check(bool ok, string label)
        {
            failed |= !ok; checks.Add((ok ? "PASS " : "FAIL ") + label);
            File.WriteAllLines(Path.Combine(root, "Checks.txt"), checks);
        }
        private IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
            bool cameraOnly = false;
            if (!Visible(screenshot))
            {
                if (screenshot != null) Destroy(screenshot);
                // Hidden Windows swapchains can return all-zero pixels. Render the real scene explicitly;
                // legacy OnGUI overlays are excluded, so these images are never labelled as UI screenshots.
                cameraOnly = true;
                Camera camera = Camera.main;
                RenderTexture previous = camera.targetTexture, active = RenderTexture.active;
                var target = RenderTexture.GetTemporary(Screen.width, Screen.height, 24);
                camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
                screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0); screenshot.Apply();
                camera.targetTexture = previous; RenderTexture.active = active; RenderTexture.ReleaseTemporary(target);
            }
            Check(Visible(screenshot), (cameraOnly ? "Camera render, GUI excluded: " : "Screen render: ") + name);
            if (cameraOnly) name += "-camera-only";
            File.WriteAllBytes(Path.Combine(root, name + ".png"), screenshot.EncodeToPNG()); Destroy(screenshot);
        }
        private static bool Visible(Texture2D texture)
        {
            if (texture == null) return false;
            int visible = 0;
            for (int y = 0; y < texture.height; y += 24) for (int x = 0; x < texture.width; x += 24)
                if (texture.GetPixel(x, y).maxColorComponent > .16f) visible++;
            return visible > 40;
        }
        private IEnumerator Start()
        {
            string[] args = Environment.GetCommandLineArgs(); int at = Array.IndexOf(args, "-capturePath");
            if (at < 0 || at + 1 >= args.Length) { Application.Quit(1); yield break; }
            root = Path.GetFullPath(args[at + 1]); Directory.CreateDirectory(root);
            Application.runInBackground = true; Application.logMessageReceived += Log;
            Check(RunPaths.Root.Contains("ArcaneBrigadeDiagnostics"), "Capture uses isolated profile and save");
            bool reduced = args.Contains("-rare-visitors-reduced");
            var storage = new RunStorage(RunPaths.Root);
            storage.SaveProfile(new PlayerProfile { TutorialSeen = true, BestWave = 40, ReducedMotion = reduced, ReducedEffects = reduced, ReducedFlashes = reduced });
            var catalog = Resources.Load<RunCatalog>("RunCatalog");
            foreach (string id in new[] { "StarMender", "ThirdBellKeeper", "UnclaimedGiftKeeper" })
            {
                using (var run = new RunSession(catalog))
                {
                    run.Start("LanternKnight", 221026); run.State.Wave = 12; run.State.Phase = SessionPhase.Shop;
                    run.State.Gold = 200; run.State.SpecialShopVisited = true;
                    run.State.MienCho.ReadMask = (1 << MienChoStory.Available(run.State)) - 1;
                    run.State.ActOne ??= new ActOneProgress(); run.State.ActOne.Offer = ActOneProgress.Refused;
                    run.ForceVisitor(id); storage.SaveRun(run.State);
                }
                RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(3);
                var owner = FindAnyObjectByType<ArenaController>();
                Check(owner != null && !owner.StoryConsumesInput && owner.Session.Visitor?.Id == id, "Real shop loads " + id);
                var station = owner.Shop.Stations.FirstOrDefault(s => s.Action == "VISITOR");
                Check(station != null, id + " has a world interaction station");
                if (station == null) continue;
                owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = station.Position + Vector2.down * .8f;
                FindAnyObjectByType<ArenaCamera>().Snap(); yield return new WaitForSecondsRealtime(.6f);
                yield return Shot(id + "-world");
                owner.GetComponent<VisitorCard>().Show(); yield return new WaitForSecondsRealtime(.5f);
                yield return Shot(id + "-card");
                Check(owner.VisitorChoose(0), id + " gift commits in the Windows player");
                Check(owner.Session.Visitor == null && !owner.VisitorChoose(0), id + " gift cannot be claimed again");
                yield return new WaitForSecondsRealtime(.5f); yield return Shot(id + "-received");
            }
            Debug.Log("RARE_VISITORS_NATIVE_" + (failed ? "FAILED" : "PASSED") + " " + checks.Count);
            Application.Quit(failed ? 1 : 0);
        }
        private void Log(string line, string trace, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert) Check(false, line);
        }
        private void OnDestroy() { Application.logMessageReceived -= Log; }
    }
}
