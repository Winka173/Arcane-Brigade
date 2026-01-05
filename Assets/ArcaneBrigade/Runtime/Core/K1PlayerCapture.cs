using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public sealed class K1PlayerCapture : MonoBehaviour
    {
        private bool failed;
        private string root;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-k1-capture") < 0 || FindAnyObjectByType<K1PlayerCapture>() != null) return;
            var runner = new GameObject("K1PlayerCapture");
            DontDestroyOnLoad(runner);
            runner.AddComponent<K1PlayerCapture>();
        }
        private void Awake() { Application.runInBackground = true; Application.logMessageReceived += OnLog; }
        private void OnDestroy() { Application.logMessageReceived -= OnLog; }
        private void OnLog(string message, string stack, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) failed = true; }
        private IEnumerator Start()
        {
            string[] args = Environment.GetCommandLineArgs();
            int path = Array.IndexOf(args, "-capturePath");
            if (Application.isBatchMode || path < 0 || path + 1 >= args.Length) { Application.Quit(1); yield break; }
            root = Path.GetFullPath(args[path + 1]);
            Directory.CreateDirectory(root);
            yield return new WaitForSecondsRealtime(1);
            RunMenu menu = FindAnyObjectByType<RunMenu>();
            if (menu == null) { Application.Quit(1); yield break; }
            using (var session = new RunSession(Resources.Load<RunCatalog>("RunCatalog")))
            {
                session.Start(Resources.Load<RunCatalog>("RunCatalog").Champions[0].Id, 131026);
                new RunStorage(RunPaths.Root).SaveRun(session.State);
            }
            menu.ReloadSaveSummary();
            foreach (RunMenu.Page page in Enum.GetValues(typeof(RunMenu.Page)))
            {
                menu.ShowPage(page);
                yield return new WaitForEndOfFrame();
                Capture("Menu_" + page);
            }
            SceneManager.LoadScene("RunArena");
            yield return new WaitForSecondsRealtime(2);
            ArenaController owner = FindAnyObjectByType<ArenaController>();
            if (owner == null) { Application.Quit(1); yield break; }
            yield return new WaitForEndOfFrame();
            Capture("Combat");
            owner.ToggleDetails();
            string before = JsonUtility.ToJson(owner.Session.State);
            float elapsed = owner.Simulation.Elapsed;
            float health = owner.Simulation.Champion.Health;
            foreach (int tab in new[] { 0, 1, 2 })
            {
                owner.Details.SelectTab(tab);
                yield return new WaitForSecondsRealtime(0.3f);
                yield return new WaitForEndOfFrame();
                Capture("Details_" + tab);
            }
            if (before != JsonUtility.ToJson(owner.Session.State) || elapsed != owner.Simulation.Elapsed || health != owner.Simulation.Champion.Health) { failed = true; Debug.LogError("Details changed runtime state"); }
            owner.ToggleDetails();
            owner.TogglePause();
            owner.ToggleDetails();
            owner.ToggleDetails();
            if (owner.Simulation.Phase != RunPhase.Paused) { failed = true; Debug.LogError("Details released manual pause"); }
            ArenaCamera camera = Camera.main.GetComponent<ArenaCamera>();
            Vector2 original = owner.Simulation.Champion.Position;
            foreach (Vector2 edge in new[] { new Vector2(-17, -11), new Vector2(17, -11), new Vector2(-17, 11), new Vector2(17, 11) })
            {
                owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = edge;
                camera.Snap();
                yield return new WaitForEndOfFrame();
                Capture("Camera_" + edge.x + "_" + edge.y + "_Fixture");
            }
            owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = original;
            camera.Snap();
            owner.RequestMenuReturn();
            yield return new WaitForEndOfFrame();
            Capture("Return_Confirmation");
            owner.CancelMenuReturn();
            owner.TogglePause();
            owner.Session.State.Phase = SessionPhase.Shop;
            owner.Session.State.Gold = 180;
            owner.Session.Reroll();
            owner.EnterShopPresentation();
            yield return new WaitForSecondsRealtime(0.4f);
            yield return new WaitForEndOfFrame();
            Capture("World_Shop_Fixture");
            owner.Session.State.Roster.Clear();
            foreach (UnitDefinition unit in owner.Content.Companions)
                foreach (WeaponDefinition weapon in owner.Content.Weapons) owner.Session.State.Roster.Add(new RosterEntry { UnitId = unit.Id, WeaponId = weapon.Id, Tier = 2 });
            foreach (ItemDefinition item in owner.Content.Items) owner.Session.State.Items.Add(new ItemStack { Id = item.Id, Count = 2 });
            owner.EnterShopPresentation();
            owner.ToggleDetails();
            owner.Details.SelectTab(1);
            yield return new WaitForEndOfFrame();
            Capture("Details_Large_Roster_First_Fixture");
            for (int row = 0; row < owner.Session.State.Roster.Count; row++) owner.Details.MoveSelection(1);
            yield return new WaitForEndOfFrame();
            Capture("Details_Large_Roster_Last_Fixture");
            owner.Details.SelectTab(2);
            yield return new WaitForEndOfFrame();
            Capture("Details_Relic_Stacks_Fixture");
            using (var loaded = new RunSession(owner.Content))
            {
                new RunStorage(RunPaths.Root).SaveRun(owner.Session.State);
                RunSnapshot snapshot = new RunStorage(RunPaths.Root).LoadRun(loaded);
                if (snapshot == null) { failed = true; Debug.LogError("Diagnostic resume failed"); }
            }
            File.WriteAllText(Path.Combine(root, "Capture_Notes.txt"), "Rendered Windows player; diagnostic saves isolated. Camera corners, saved summary, shop, large roster and relic stacks use fixtures, not a full-run playtest. Details state/timer/health freeze and manual pause ownership checked.\nResult: " + (failed ? "FAIL" : "PASS"));
            Debug.Log(failed ? "ARCANE_K1_CAPTURE_FAILED" : "ARCANE_K1_CAPTURE_PASSED");
            Application.Quit(failed ? 1 : 0);
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
