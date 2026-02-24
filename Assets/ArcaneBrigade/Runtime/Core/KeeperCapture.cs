using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    /// <summary>Native evidence for the Keeper's arc. Run with -arcane-keeper-capture -capturePath DIR (isolated diagnostics save):
    /// the Coin Widow at the wave-5 gate; the carpet carrying the shop into wave 11 with the Keeper's body looming; the prism
    /// counter and a rainbow gift from three shards; THE KEEPER, WHOLE at shop 17, then kneeling once its parts are broken.</summary>
    public sealed class KeeperCapture : MonoBehaviour
    {
        public static bool Requested => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-keeper-capture") >= 0;
        private string root;
        private readonly List<string> log = new List<string>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (!Requested || FindAnyObjectByType<KeeperCapture>() != null) return;
            var go = new GameObject("KeeperCapture"); DontDestroyOnLoad(go); go.AddComponent<KeeperCapture>();
        }
        private IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            Texture2D image = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(root, name + ".png"), image.EncodeToPNG()); Destroy(image);
        }
        private void Write() => File.WriteAllLines(Path.Combine(root, "Keeper.txt"), log);
        private void OnLog(string message, string trace, LogType type) { if (type == LogType.Error || type == LogType.Exception) { log.Add("ERROR " + message + " | " + trace.Split('\n')[0]); Write(); } }
        private IEnumerator Load(Action<RunSession> setup, RunCatalog catalog, RunStorage storage)
        {
            using (var session = new RunSession(catalog)) { session.Start("LanternKnight", 171705); setup(session); storage.SaveRun(session.State); }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena");
            yield return new WaitForSecondsRealtime(2.2f);
        }
        private IEnumerator Start()
        {
            string[] args = Environment.GetCommandLineArgs(); int at = Array.IndexOf(args, "-capturePath");
            root = at >= 0 && at + 1 < args.Length ? Path.GetFullPath(args[at + 1]) : null; if (root == null) { Application.Quit(1); yield break; }
            Directory.CreateDirectory(root); Application.runInBackground = true; Application.logMessageReceived += OnLog;
            if (!RunPaths.Root.Contains("ArcaneBrigadeDiagnostics")) { Application.Quit(1); yield break; }
            var catalog = Resources.Load<RunCatalog>("RunCatalog"); var storage = new RunStorage(RunPaths.Root);
            storage.SaveProfile(new PlayerProfile { BestWave = 40, TutorialSeen = true });
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(.5f);

            // 1. the wave-5 gate: the Coin Widow
            yield return Load(s => { s.State.Wave = 5; s.State.Phase = SessionPhase.Combat; }, catalog, storage);
            var owner = FindAnyObjectByType<ArenaController>();
            yield return new WaitForSecondsRealtime(3f);
            log.Add("wave 5 boss: " + owner.Simulation.Boss?.Definition.Id + " / " + owner.Simulation.Boss?.Definition.DisplayName);
            yield return Shot("01-wave5-coin-widow");

            // 2. the carpet carries the shop into wave 11; the Keeper's body looms above the room
            yield return Load(s => { s.State.Wave = 10; s.State.Phase = SessionPhase.Shop; s.State.SpecialShopVisited = true; s.State.SpecialOffers.Clear(); }, catalog, storage);
            owner = FindAnyObjectByType<ArenaController>();
            owner.NextWave();
            float started = Time.realtimeSinceStartup;
            StageTransfer transfer = null;
            while (transfer == null && Time.realtimeSinceStartup - started < 6) { yield return null; transfer = FindAnyObjectByType<StageTransfer>(); }
            log.Add("transfer: " + (transfer == null ? "none" : transfer.Kind.ToString()));
            if (transfer != null)
            {
                while (transfer != null && transfer.ElapsedSeconds < 0.9f) yield return null;
                var looms = GameObject.Find("TheKeeperLooms")?.GetComponent<SpriteRenderer>();
                log.Add("looming body: " + (looms == null ? "none" : looms.sprite?.name + " bounds " + looms.bounds + " colour " + looms.color + " order " + looms.sortingOrder));
                yield return Shot("02-wave11-carpet-and-keeper");
                while (transfer != null && transfer.ElapsedSeconds < 1.6f) yield return null;
                yield return Shot("03-wave11-carpet-later");
            }
            yield return new WaitForSecondsRealtime(2f);

            // 3. the prism counter in a fight, then three shards opening a rainbow gift in the shop
            owner = FindAnyObjectByType<ArenaController>();
            if (owner?.Simulation != null) { owner.Simulation.PrismFixtureDrop(owner.Simulation.Champion.Position + Vector2.right * 0.3f); yield return new WaitForSecondsRealtime(1.2f); yield return Shot("04-prism-pickup-and-counter"); }
            yield return Load(s => { s.State.Wave = 12; s.State.Phase = SessionPhase.Shop; s.State.PrismShards = 3; s.State.SpecialShopVisited = true; s.State.SpecialOffers.Clear(); s.OfferPrismGift(); }, catalog, storage);
            owner = FindAnyObjectByType<ArenaController>();
            log.Add("prism gift: " + owner.Session.State.OutsideStage + " " + string.Join(",", owner.Session.State.OutsideRewards));
            yield return Shot("05-prism-gift-choices");

            // 4. THE KEEPER, WHOLE at shop 17, then kneeling
            yield return Load(s => { s.State.Wave = ActOneCanon.KeeperReturnShop; s.State.Phase = SessionPhase.Shop; s.State.SpecialShopVisited = true; s.State.SpecialOffers.Clear(); s.State.PrismShards = 2; }, catalog, storage);
            owner = FindAnyObjectByType<ArenaController>();
            yield return Shot("06-shop17-the-hand");
            log.Add("challenge offered: " + owner.Session.OutsideChallengeAvailable + " accepted: " + owner.AcceptOutsideChallenge());
            started = Time.realtimeSinceStartup;
            while ((owner.Simulation == null || owner.Simulation.Phase != RunPhase.Combat || owner.Simulation.Boss == null) && Time.realtimeSinceStartup - started < 8) { yield return null; owner = FindAnyObjectByType<ArenaController>(); }
            yield return new WaitForSecondsRealtime(3.5f);
            var keeper = owner.Simulation.Boss;
            log.Add("keeper: " + keeper?.Definition.DisplayName + " hp " + keeper?.Health.ToString("0") + "/" + keeper?.Definition.Health.ToString("0") + " guard " + (keeper == null ? 0 : CombatSimulation.KeeperGuard(keeper, owner.Simulation.Elapsed)).ToString("0.00"));
            yield return Shot("07-keeper-whole-fight");
            owner.Simulation.KeeperFixtureBreakAll();
            yield return new WaitForSecondsRealtime(1.2f);
            log.Add("kneeling: guard " + CombatSimulation.KeeperGuard(keeper, owner.Simulation.Elapsed).ToString("0.00"));
            yield return Shot("08-keeper-kneels");
            Write(); Application.Quit(0);
        }
        private void OnDestroy() => Application.logMessageReceived -= OnLog;
    }
}
