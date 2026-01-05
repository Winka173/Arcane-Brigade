using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public sealed class K2PlayerCapture : MonoBehaviour
    {
        private string root;
        private bool failed;
        private readonly List<float> frameTimes = new List<float>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (!RunPaths.K2Capture || FindAnyObjectByType<K2PlayerCapture>() != null) return;
            var runner = new GameObject("K2PlayerCapture");
            DontDestroyOnLoad(runner);
            runner.AddComponent<K2PlayerCapture>();
        }
        private void Awake() { Application.runInBackground = true; Application.logMessageReceived += OnLog; }
        private void OnDestroy() { Application.logMessageReceived -= OnLog; }
        private void OnLog(string message, string stack, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) failed = true; }
        private IEnumerator Start()
        {
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-capturePath");
            if (Application.isBatchMode || index < 0 || index + 1 >= args.Length) { Application.Quit(1); yield break; }
            root = Path.GetFullPath(args[index + 1]);
            Directory.CreateDirectory(root);
            SceneManager.LoadScene("RunArena");
            yield return new WaitForSecondsRealtime(1);
            ArenaController owner = FindAnyObjectByType<ArenaController>();
            if (owner == null) { Application.Quit(1); yield break; }
            float sampleEnd = Time.realtimeSinceStartup + 3;
            while (Time.realtimeSinceStartup < sampleEnd) { frameTimes.Add(Time.unscaledDeltaTime * 1000); yield return null; }
            frameTimes.Sort();
            var report = new List<string> { "Rendered Windows D3D11 1280x720; isolated seeded diagnostic run.", "Injected enemies/HP/roster are presentation fixtures; attacks, kill, loot and settlement use normal runtime. No screenshot frames used for FPS measurement." };
            if (frameTimes.Count > 0) report.Add("Uncaptured 3s early-wave sample, frames=" + frameTimes.Count + ", median_ms=" + frameTimes[frameTimes.Count / 2].ToString("0.00", CultureInfo.InvariantCulture) + ", p95_ms=" + frameTimes[Mathf.Min(frameTimes.Count - 1, Mathf.FloorToInt(frameTimes.Count * 0.95f))].ToString("0.00", CultureInfo.InvariantCulture) + ". Not a stress/device certification.");
            int kills = owner.Simulation.Kills;
            owner.Simulation.AddPresentationFixture(owner.Simulation.Champion.Position + new Vector2(0.70f, 0), 1);
            float deadline = Time.realtimeSinceStartup + 4;
            while (owner.Simulation.Kills == kills && Time.realtimeSinceStartup < deadline) yield return null;
            if (owner.Simulation.Kills == kills) { failed = true; Debug.LogError("Enemy death fixture failed"); }
            if (owner.Simulation.Pickups.Count > 0) owner.SetPresentationMoveTarget(owner.Simulation.Pickups[0].Position);
            yield return CaptureSequence("Enemy_Death_Coin", 42);
            owner.SetPresentationMoveTarget(null);
            yield return new WaitForSecondsRealtime(0.5f);
            if (owner.Simulation.Gold < 1) { failed = true; Debug.LogError("Real coin fixture did not collect"); }
            report.Add("Enemy kill + player walks toward dropped coin through normal movement + coin credit observed; wallet=" + owner.Simulation.Gold + ".");
            owner.ToggleDetails();
            float elapsed = owner.Simulation.Elapsed;
            int wallet = owner.Simulation.Gold;
            yield return new WaitForSecondsRealtime(0.3f);
            if (elapsed != owner.Simulation.Elapsed || wallet != owner.Simulation.Gold) { failed = true; Debug.LogError("Paused pickup/timer changed"); }
            owner.ToggleDetails();
            owner.Session.State.Phase = SessionPhase.Shop;
            owner.Session.State.Wave = 3;
            owner.Session.State.Roster.Clear();
            foreach (UnitDefinition unit in owner.Content.Companions) owner.Session.State.Roster.Add(new RosterEntry { UnitId = unit.Id, WeaponId = unit.Weapon.Id });
            owner.EnterShopPresentation();
            owner.NextWave();
            yield return new WaitForSecondsRealtime(0.3f);
            UnitState boss = owner.Simulation.Boss;
            if (boss == null) { failed = true; Debug.LogError("Missing boss fixture"); }
            else
            {
                boss.Position = boss.PreviousPosition = new Vector2(0.65f, 0);
                boss.Health = 1;
                deadline = Time.realtimeSinceStartup + 4;
                while (!owner.Simulation.BossDefeated && Time.realtimeSinceStartup < deadline) yield return null;
                if (!owner.Simulation.BossDefeated) { failed = true; Debug.LogError("Boss death fixture failed"); }
                yield return CaptureSequence("Boss_Death_Transition", 72);
                yield return new WaitForSecondsRealtime(1.2f);
                if (owner.Session.State.Phase != SessionPhase.Shop || owner.Simulation.Phase != RunPhase.Rest) { failed = true; Debug.LogError("Boss did not transition into rest"); }
            }
            owner.NextWave();
            yield return new WaitForSecondsRealtime(0.25f);
            owner.Simulation.Champion.Health = 1;
            owner.Simulation.AddPresentationFixture(owner.Simulation.Champion.Position + new Vector2(0.2f, 0), 1000);
            deadline = Time.realtimeSinceStartup + 4;
            while (owner.Simulation.Phase != RunPhase.Defeat && Time.realtimeSinceStartup < deadline) yield return null;
            if (owner.Simulation.Phase != RunPhase.Defeat) { failed = true; Debug.LogError("Champion death fixture failed"); }
            yield return CaptureSequence("Champion_Death_Results", 72);
            yield return new WaitForSecondsRealtime(1.1f);
            yield return new WaitForEndOfFrame();
            Capture("Results_After_Death");
            report.Add("Sequences store frame timestamp CSV for truthful playback; PNG capture cost affects cadence. No production animation/FPS approval implied.");
            report.Add("Result: " + (failed ? "FAIL" : "PASS"));
            File.WriteAllLines(Path.Combine(root, "Capture_Report.txt"), report);
            Debug.Log(failed ? "ARCANE_K2_CAPTURE_FAILED" : "ARCANE_K2_CAPTURE_PASSED");
            Application.Quit(failed ? 1 : 0);
        }
        private IEnumerator CaptureSequence(string name, int count)
        {
            Directory.CreateDirectory(Path.Combine(root, name));
            var times = new List<string> { "frame,realtime_seconds" };
            var frames = new List<Texture2D>();
            float start = Time.realtimeSinceStartup;
            for (int frame = 0; frame < count; frame++)
            {
                yield return new WaitForEndOfFrame();
                times.Add(frame + "," + (Time.realtimeSinceStartup - start).ToString("0.000000", CultureInfo.InvariantCulture));
                Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
                if (screenshot == null) { failed = true; continue; }
                frames.Add(screenshot);
            }
            File.WriteAllLines(Path.Combine(root, name, "timestamps.csv"), times);
            for (int frame = 0; frame < frames.Count; frame++)
            {
                File.WriteAllBytes(Path.Combine(root, name, "frame_" + frame.ToString("000") + ".png"), frames[frame].EncodeToPNG());
                Destroy(frames[frame]);
                yield return null;
            }
        }
        private void Capture(string name)
        {
            Texture2D frame = ScreenCapture.CaptureScreenshotAsTexture();
            if (frame == null) { failed = true; return; }
            File.WriteAllBytes(Path.Combine(root, name + ".png"), frame.EncodeToPNG());
            Destroy(frame);
        }
    }
}
