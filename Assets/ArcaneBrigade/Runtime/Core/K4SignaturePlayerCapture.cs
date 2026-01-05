using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public sealed class K4SignaturePlayerCapture : MonoBehaviour
    {
        private bool failed;
        private string root;
        private int supersize;
        private readonly List<string> notes = new List<string>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (!RunPaths.K4Capture || FindAnyObjectByType<K4SignaturePlayerCapture>() != null) return;
            var runner = new GameObject("K4SignaturePlayerCapture"); DontDestroyOnLoad(runner); runner.AddComponent<K4SignaturePlayerCapture>();
        }
        private void Awake() { Application.runInBackground = true; Application.logMessageReceived += OnLog; }
        private void OnDestroy() { Application.logMessageReceived -= OnLog; }
        private void OnLog(string message, string stack, LogType kind) { if (kind == LogType.Error || kind == LogType.Exception || kind == LogType.Assert) failed = true; }
        private IEnumerator Start()
        {
            string[] args = Environment.GetCommandLineArgs(); int path = Array.IndexOf(args, "-capturePath");
            if (Application.isBatchMode || path < 0 || path + 1 >= args.Length) { Application.Quit(1); yield break; }
            root = Path.GetFullPath(args[path + 1]); Directory.CreateDirectory(root);
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(1);
            supersize = Screen.width == 1920 && Screen.height == 1080 ? 2 : 1;
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 13104); session.State.Roster.Clear();
                string[] weapons = { "UmbrellaLance", "BellFork", "DreamStamp" };
                for (int index = 0; index < weapons.Length; index++) session.State.Roster.Add(new RosterEntry { UnitId = catalog.Companions[index].Id, WeaponId = weapons[index], Tier = 2 });
                new RunStorage(RunPaths.Root).SaveRun(session.State);
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(0.3f);
            ArenaController owner = FindAnyObjectByType<ArenaController>();
            if (owner == null) { Application.Quit(1); yield break; }
            owner.Simulation.ConfigureLootPresentationFixture();
            foreach (UnitState unit in owner.Simulation.Units)
            {
                if (unit.Faction != Faction.Companion) continue;
                unit.Position = unit.PreviousPosition = new Vector2(0.25f, (unit.Id - 3) * 0.5f);
                unit.Velocity = Vector2.zero; unit.RoamOffset = unit.Position; unit.NextRoamTime = float.PositiveInfinity;
            }
            for (int index = 0; index < 4; index++)
            {
                UnitState enemy = owner.Simulation.AddPresentationFixture(new Vector2(1.25f + index * 0.25f, (index - 1) * 0.65f), 20000);
                enemy.Definition.Speed = 0; enemy.NextAttack = enemy.NextContact = float.PositiveInfinity;
            }
            yield return new WaitForEndOfFrame(); Capture("Signature_Roster_4K");
            float end = Time.realtimeSinceStartup + 14;
            bool bell = false; bool umbrella = false; bool stamp = false;
            while (Time.realtimeSinceStartup < end && (!bell || !umbrella || !stamp))
            {
                yield return null;
                string name = null;
                if (!bell && owner.Simulation.BellWaves > 0) { bell = true; name = "Bell_Payoff_4K"; }
                else if (!umbrella && owner.Simulation.UmbrellaOpenings > 0) { umbrella = true; name = "Umbrella_Payoff_4K"; }
                else if (!stamp && owner.Simulation.StampBursts > 0) { stamp = true; name = "Stamp_Payoff_4K"; }
                if (name != null) { yield return new WaitForEndOfFrame(); Capture(name); }
            }
            if (!bell || !umbrella || !stamp) { failed = true; Debug.LogError("K4 autocombat failed to produce all three signature payoffs"); }
            owner.ToggleDetails(); owner.Details.SelectTab(1); owner.Details.MoveSelection(2);
            yield return new WaitForEndOfFrame(); Capture("Stamp_Details_4K");
            notes.Add("Native Windows D3D11. Live " + Screen.width + "x" + Screen.height + "; Unity supersize " + supersize + ". PNG dimensions asserted 3840x2160; not a realtime 4K performance benchmark.");
            notes.Add("Seed 13104; seeded tier-2 roster and 20000-HP enemies, enemy attacks/AI and ally patrol frozen solely in diagnostic fixture. Actual auto-target, windup, hit, marks, waves, tier and VFX/SFX run through gameplay simulation; no manual proc injection.");
            notes.Add("Observed counters: bell=" + owner.Simulation.BellWaves + "; umbrella=" + owner.Simulation.UmbrellaOpenings + "; stamp=" + owner.Simulation.StampBursts + ". Capture timing may alter number of elapsed attacks; these are not contribution/DPS metrics.");
            notes.Add("Isolated diagnostic save. Font/UI redesign deferred K6/K7. K4.2-K4.5 not complete.");
            notes.Add("Result: " + (failed ? "FAIL" : "PASS")); File.WriteAllLines(Path.Combine(root, "Capture_Notes.txt"), notes);
            Debug.Log(failed ? "ARCANE_K4_SIGNATURE_CAPTURE_FAILED" : "ARCANE_K4_SIGNATURE_CAPTURE_PASSED"); Application.Quit(failed ? 1 : 0);
        }
        private void Capture(string name)
        {
            Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture(supersize);
            if (texture == null || texture.width != 3840 || texture.height != 2160) { failed = true; Debug.LogError("K4 capture resolution invalid"); }
            else File.WriteAllBytes(Path.Combine(root, name + ".png"), texture.EncodeToPNG());
            if (texture != null) Destroy(texture);
        }
    }
}
