using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public sealed class K3FinalPlayerCapture : MonoBehaviour
    {
        private bool failed;
        private string root;
        private readonly List<Texture2D> frames = new List<Texture2D>();
        private readonly List<string> timings = new List<string> { "frame,realtime_seconds" };
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (!RunPaths.K3FinalCapture || FindAnyObjectByType<K3FinalPlayerCapture>() != null) return;
            var runner = new GameObject("K3FinalPlayerCapture"); DontDestroyOnLoad(runner); runner.AddComponent<K3FinalPlayerCapture>();
        }
        private void Awake() { Application.runInBackground = true; Application.logMessageReceived += OnLog; }
        private void OnDestroy() { Application.logMessageReceived -= OnLog; }
        private void OnLog(string message, string stack, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) failed = true; }
        private IEnumerator Start()
        {
            string[] args = Environment.GetCommandLineArgs(); int path = Array.IndexOf(args, "-capturePath");
            if (Application.isBatchMode || path < 0 || path + 1 >= args.Length) { Application.Quit(1); yield break; }
            root = Path.GetFullPath(args[path + 1]); Directory.CreateDirectory(root);
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 131031); session.State.Health = 40;
                session.State.Items.Add(new ItemStack { Id = "coin-salve", Count = 2 }); session.State.Items.Add(new ItemStack { Id = "chest-thunder", Count = 1 });
                new RunStorage(RunPaths.Root).SaveRun(session.State);
            }
            yield return new WaitForSecondsRealtime(0.5f); RunLaunch.Resume = true; SceneManager.LoadScene("RunArena");
            yield return new WaitForSecondsRealtime(0.35f);
            ArenaController owner = FindAnyObjectByType<ArenaController>();
            if (owner == null) { Application.Quit(1); yield break; }
            owner.Simulation.ConfigureLootPresentationFixture();
            owner.Simulation.AddLootPresentationFixture(LootKind.Chest, new Vector2(2.1f, 0), 12);
            owner.Simulation.AddLootPresentationFixture(LootKind.Heart, new Vector2(-1.5f, 1.2f), 8);
            owner.Simulation.AddLootPresentationFixture(LootKind.Egg, new Vector2(1.6f, 1.5f), 1);
            owner.Simulation.AddLootPresentationFixture(LootKind.Chest, new Vector2(15, 6), 12);
            owner.Simulation.AddLootPresentationFixture(LootKind.Egg, new Vector2(-15, 6), 1);
            owner.Simulation.AddLootPresentationFixture(LootKind.Egg, new Vector2(-14, 6), 1);
            yield return new WaitForSecondsRealtime(0.4f); yield return new WaitForEndOfFrame(); Capture("Loot_Icons_Indicators_Fixture");
            owner.ToggleDetails(); owner.Details.SelectTab(2);
            yield return new WaitForEndOfFrame(); Capture("Proc_Details_Fixture");
            owner.ToggleDetails();
            owner.SetPresentationMoveTarget(new Vector2(2.1f, 0));
            float deadline = Time.realtimeSinceStartup + 3;
            while (owner.Simulation.ChestsOpened == 0 && Time.realtimeSinceStartup < deadline) yield return null;
            if (owner.Simulation.ChestsOpened != 1) { failed = true; Debug.LogError("Native chest proximity open failed"); }
            owner.SetPresentationMoveTarget(new Vector2(2.1f, 0));
            for (int frame = 0; frame < 72; frame++)
            {
                yield return new WaitForEndOfFrame();
                frames.Add(ScreenCapture.CaptureScreenshotAsTexture());
                timings.Add(frame + "," + Time.realtimeSinceStartup.ToString("0.000000", CultureInfo.InvariantCulture));
            }
            for (int frame = 0; frame < frames.Count; frame++)
            {
                if (frames[frame] == null) { failed = true; continue; }
                File.WriteAllBytes(Path.Combine(root, "Chest_Motion_" + frame.ToString("D3") + ".png"), frames[frame].EncodeToPNG()); Destroy(frames[frame]);
            }
            File.WriteAllLines(Path.Combine(root, "Chest_Motion_Timestamps.csv"), timings);
            if (owner.Simulation.Gold < 12) { failed = true; Debug.LogError("Native chest reward was not collected through movement"); }
            owner.SetPresentationMoveTarget(null);
            owner.Session.State.Phase = SessionPhase.Shop; owner.Session.State.Gold = 100; owner.Session.Reroll(); owner.EnterShopPresentation();
            owner.Session.State.Offers[1].Id = "altar-ember"; owner.Session.State.Offers[1].Price = 10; owner.Session.State.Offers[1].Currency = ShopCurrency.Health;
            owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = new Vector2(0, 0.5f); owner.Shop.Display();
            yield return new WaitForEndOfFrame(); Capture("Final_Item_Preview_Fixture");
            owner.ShowOfferPreviewFixture();
            yield return new WaitForEndOfFrame(); Capture("Final_Item_Delta_Preview_Fixture");
            if (!owner.Shop.Interact()) { failed = true; Debug.LogError("Native final shop purchase failed"); }
            using (var loaded = new RunSession(catalog))
            {
                loaded.Restore(new RunStorage(RunPaths.Root).LoadRun(loaded));
                if (loaded.StackCount("altar-ember") != 1 || loaded.PrepareCombat().Procs.Length != 2) { failed = true; Debug.LogError("Native final purchase resume failed"); }
            }
            File.WriteAllText(Path.Combine(root, "Capture_Notes.txt"), "Native Windows D3D11 1280x720. Seeded wallet/items/loot positions and disabled ambient spawning are isolated fixtures. Chest proximity, reward drops, movement, proc, physical-shop purchase and disk resume use normal runtime. Motion PNGs buffered before encoding; timestamps supplied. Not a natural full-run or mobile certification.\nResult: " + (failed ? "FAIL" : "PASS"));
            Debug.Log(failed ? "ARCANE_K3_FINAL_CAPTURE_FAILED" : "ARCANE_K3_FINAL_CAPTURE_PASSED"); Application.Quit(failed ? 1 : 0);
        }
        private void Capture(string name)
        {
            Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture();
            if (texture == null) { failed = true; return; }
            File.WriteAllBytes(Path.Combine(root, name + ".png"), texture.EncodeToPNG()); Destroy(texture);
        }
    }
}
