using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public sealed class K7ShopMotionCapture : MonoBehaviour
    {
        private ArenaController owner;
        private Vector2 target;
        private bool failed;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (!RunPaths.K7Capture || Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-k7-shop-video") < 0 || FindAnyObjectByType<K7ShopMotionCapture>() != null) return;
            var runner = new GameObject("K7ShopMotionCapture"); DontDestroyOnLoad(runner); runner.AddComponent<K7ShopMotionCapture>();
        }
        private void Update() { if (owner != null) owner.SetPresentationMoveTarget(target); }
        private void OnLog(string message, string trace, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) failed = true; }
        private void OnDestroy() { Application.logMessageReceived -= OnLog; }
        private IEnumerator Start()
        {
            Application.logMessageReceived += OnLog; Application.runInBackground = true;
            string[] args = Environment.GetCommandLineArgs(); int index = Array.IndexOf(args, "-capturePath");
            if (index < 0 || index + 1 >= args.Length || Application.isBatchMode) { Application.Quit(1); yield break; }
            string root = Path.GetFullPath(args[index + 1]); Directory.CreateDirectory(root);
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(1);
            K7PlayerCapture.ShowDiagnosticWindow();
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog"); var storage = new RunStorage(RunPaths.Root);
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 4313); session.State.Phase = SessionPhase.Shop; session.State.Gold = 10000;
                session.State.Roster.Clear(); session.State.Offers.Clear(); session.State.SpecialOffers.Clear();
                for (int entry = 0; entry < 2; entry++) session.State.Roster.Add(new RosterEntry { UnitId = catalog.Companions[0].Id, WeaponId = catalog.Weapons[entry].Id });
                session.State.Offers.Add(new ShopOffer { Token = 1, Id = catalog.Companions[0].Id, Kind = OfferKind.Companion, Price = 1 });
                session.State.Offers.Add(new ShopOffer { Token = 2, Id = catalog.Companions[1].Id, Kind = OfferKind.Companion, Price = 1 });
                session.State.Offers.Add(new ShopOffer { Token = 3, Id = catalog.Items[0].Id, Kind = OfferKind.Item, Price = 1 });
                session.State.NextOffer = 3; session.Validate(session.State); storage.SaveRun(session.State);
            }
            storage.SaveProfile(new PlayerProfile()); RunLaunch.Resume = true;
            SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(0.8f);
            owner = FindAnyObjectByType<ArenaController>();
            target = new Vector2(-5, -1); owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = target;
            yield return new WaitForSecondsRealtime(0.5f);
            K7FramebufferRecorder recorder = gameObject.AddComponent<K7FramebufferRecorder>(); recorder.Begin(12);
            File.WriteAllText(Path.Combine(root, "VideoReady.txt"), "Window-only realtime1920x1080 shop seed4313. Seeded gold/price/roster for short visual diagnostic, not natural run; movement-only target driver + real Shop.Interact transactions; timeScale1, no PNG during capture.");
            yield return new WaitForSecondsRealtime(3);
            target = new Vector2(-3, 0.5f); yield return new WaitForSecondsRealtime(1.8f);
            failed |= !owner.Shop.Interact(); yield return new WaitForSecondsRealtime(2);
            target = new Vector2(0, 0.5f); yield return new WaitForSecondsRealtime(1.6f);
            failed |= !owner.Shop.Interact(); yield return new WaitForSecondsRealtime(2);
            target = new Vector2(3, -2); yield return new WaitForSecondsRealtime(5);
            failed |= owner.Session.State.Roster.Count != 2 || owner.Shop.Reveal.CompletedCount != 1;
            while (!recorder.Complete) yield return null;
            failed |= !recorder.Passed;
            File.WriteAllText(Path.Combine(root, "CaptureNotes.txt"), (failed ? "FAIL" : "PASS") + " actual physical movement/proximity spring/merge/drop/walkout;2committed purchases,1merge;timeScale=" + Time.timeScale + ";surface=" + Screen.width + "x" + Screen.height + "; seeded visual fixture not balance or input-hardware certification.");
            Debug.Log(failed ? "K7_SHOP_MOTION_FAILED" : "K7_SHOP_MOTION_PASSED"); Application.Quit(failed ? 1 : 0);
        }
    }
}
