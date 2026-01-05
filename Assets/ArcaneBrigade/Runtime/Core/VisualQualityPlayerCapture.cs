using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public sealed class VisualQualityPlayerCapture : MonoBehaviour
    {
        private bool failed;
        private string root;
        private int supersize = 1;
        private readonly List<string> notes = new List<string>();
        private readonly List<string> frames = new List<string>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (!RunPaths.QualityCapture || FindAnyObjectByType<VisualQualityPlayerCapture>() != null) return;
            var runner = new GameObject("VisualQualityPlayerCapture"); DontDestroyOnLoad(runner); runner.AddComponent<VisualQualityPlayerCapture>();
        }
        private void Awake() { Application.runInBackground = true; Application.logMessageReceived += OnLog; }
        private void OnDestroy() { Application.logMessageReceived -= OnLog; }
        private void OnLog(string message, string stack, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) failed = true; }
        private IEnumerator Start()
        {
            string[] args = Environment.GetCommandLineArgs(); int path = Array.IndexOf(args, "-capturePath");
            if (Application.isBatchMode || path < 0 || path + 1 >= args.Length) { Application.Quit(1); yield break; }
            root = Path.GetFullPath(args[path + 1]); Directory.CreateDirectory(root);
            yield return new WaitForSecondsRealtime(1);
            if (Screen.width != 3840 || Screen.height != 2160)
            {
                Screen.SetResolution(1920, 1080, FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(1);
                supersize = 2;
            }
            notes.Add("Unity native Windows D3D11. Live screen " + Screen.width + "x" + Screen.height + "; Unity screenshot supersize=" + supersize + ". All PNGs asserted 3840x2160, not externally resized.");
            notes.Add("HD repaint source PNGs are 1672x941, not native 4K; map repeats mirrored source tiles at world texel density rather than stretching one whole-map image. Source PNGs preserve generated alpha. 4x MSAA desktop.");
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 131041); session.State.Health = 90; session.State.Roster.Clear();
                for (int index = 0; index < 6; index++) session.State.Roster.Add(new RosterEntry { UnitId = catalog.Companions[index].Id, WeaponId = catalog.Weapons[(index + 1) % catalog.Weapons.Length].Id });
                new RunStorage(RunPaths.Root).SaveRun(session.State);
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(0.35f);
            ArenaController owner = FindAnyObjectByType<ArenaController>();
            if (owner == null) { Application.Quit(1); yield break; }
            owner.Simulation.ConfigureLootPresentationFixture();
            for (int index = 0; index < 5; index++)
            {
                UnitState enemy = owner.Simulation.AddPresentationFixture(new Vector2(2.3f + index * 0.5f, (index - 2) * 0.8f), 10000);
                enemy.Definition.Speed = 0; enemy.NextAttack = enemy.NextContact = float.PositiveInfinity;
            }
            owner.Simulation.AddLootPresentationFixture(LootKind.Chest, new Vector2(-3, 1.4f), 12);
            owner.Simulation.AddLootPresentationFixture(LootKind.Egg, new Vector2(-1.8f, 1.2f), 1);
            yield return new WaitForSecondsRealtime(1);
            var frameTimes = new List<float>(); float sampleEnd = Time.realtimeSinceStartup + 3;
            while (Time.realtimeSinceStartup < sampleEnd) { frameTimes.Add(Time.unscaledDeltaTime * 1000); yield return null; }
            frameTimes.Sort();
            if (frameTimes.Count > 0) notes.Add("Uncaptured 3s fixture sample at live screen resolution, median=" + frameTimes[frameTimes.Count / 2].ToString("0.00", CultureInfo.InvariantCulture) + " ms; p95=" + frameTimes[Mathf.Min(frameTimes.Count - 1, Mathf.FloorToInt(frameTimes.Count * 0.95f))].ToString("0.00", CultureInfo.InvariantCulture) + " ms. Not mobile/stress certification.");
            yield return new WaitForEndOfFrame(); Capture("Combat_HD_4K");
            for (int frame = 0; frame < 8; frame++)
            {
                yield return new WaitForSecondsRealtime(0.1f); yield return new WaitForEndOfFrame();
                Capture("Effects_HD_4K_" + frame.ToString("D2"));
            }
            owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = new Vector2(15.8f, -9.9f);
            Camera.main.GetComponent<ArenaCamera>().Snap(); yield return new WaitForEndOfFrame(); Capture("Map_Boundary_HD_4K");
            owner.Session.State.Phase = SessionPhase.Shop; owner.Session.State.Gold = 500; owner.Session.Reroll(); owner.EnterShopPresentation();
            owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = new Vector2(0, -1.5f);
            yield return new WaitForSecondsRealtime(0.3f); yield return new WaitForEndOfFrame(); Capture("World_Shop_HD_4K");
            owner.ToggleDetails(); owner.Details.SelectTab(1); yield return new WaitForEndOfFrame(); Capture("Models_Details_HD_4K"); owner.ToggleDetails();
            notes.Add("Seeded roster/HP/loot/enemy positions, frozen enemy AI and shop are diagnostic fixtures. Auto attacks/effects use real simulation. Diagnostic save isolated. No gameplay tuning changes or claim that all models/music are final production.");
            File.WriteAllLines(Path.Combine(root, "Frame_Timestamps.txt"), frames);
            notes.Add("Result: " + (failed ? "FAIL" : "PASS")); File.WriteAllLines(Path.Combine(root, "Capture_Notes.txt"), notes);
            Debug.Log(failed ? "ARCANE_VISUAL_QUALITY_CAPTURE_FAILED" : "ARCANE_VISUAL_QUALITY_CAPTURE_PASSED"); Application.Quit(failed ? 1 : 0);
        }
        private void Capture(string name)
        {
            Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture(supersize);
            if (texture == null || texture.width != 3840 || texture.height != 2160)
            {
                failed = true; Debug.LogError("4K capture dimensions invalid: " + (texture == null ? "null" : texture.width + "x" + texture.height));
            }
            else
            {
                File.WriteAllBytes(Path.Combine(root, name + ".png"), texture.EncodeToPNG());
                frames.Add(name + "," + Time.realtimeSinceStartup.ToString("0.000000", CultureInfo.InvariantCulture));
            }
            if (texture != null) Destroy(texture);
        }
    }
}
