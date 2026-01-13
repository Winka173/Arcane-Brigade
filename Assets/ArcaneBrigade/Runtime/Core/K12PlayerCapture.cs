using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public sealed class K12PlayerCapture : MonoBehaviour
    {
        private readonly List<string> notes = new List<string>();
        private string root;
        private bool failed;
        private ArenaController owner;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (!RunPaths.K7Capture || Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-k12") < 0 || FindAnyObjectByType<K12PlayerCapture>() != null) return;
            var child = new GameObject("K12PlayerCapture"); DontDestroyOnLoad(child); child.AddComponent<K12PlayerCapture>();
        }
        private void Check(bool condition, string caption) { notes.Add((condition ? "PASS " : "FAIL ") + caption); failed |= !condition; }
        private void OnLog(string message, string trace, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) failed = true; }
        private void OnDestroy() { Application.logMessageReceived -= OnLog; }
        private IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture();
            Check(texture.width == 3840 && texture.height == 2160, "Native4K " + name);
            File.WriteAllBytes(Path.Combine(root, name + ".png"), texture.EncodeToPNG()); Destroy(texture);
        }
        private IEnumerator Load(RunCatalog catalog, RunStorage storage, int wave, bool shop)
        {
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 12077); session.State.Wave = wave;
                session.State.Roster.Clear();
                string[] weapons = { "SpoonSabre", "ToastCleaver", "ForkTrident", "InkPistol", "ShellCannon", "SnowGlobeOrb" };
                for (int index = 0; index < weapons.Length; index++) session.State.Roster.Add(new RosterEntry { UnitId = "TeapotPilgrim", WeaponId = weapons[index], Tier = 2 });
                session.State.Gold = 1000; session.State.Eggs = 20;
                if (shop)
                {
                    session.State.Roster.RemoveRange(1, session.State.Roster.Count - 1);
                    session.State.Phase = SessionPhase.Shop;
                    session.State.Offers.Clear(); session.State.SpecialOffers.Clear(); session.State.SpecialShopVisited = true;
                    session.State.Offers.Add(new ShopOffer { Token = ++session.State.NextOffer, Kind = OfferKind.Companion, Id = "CandleSnail", Price = 1 });
                    session.State.Offers.Add(new ShopOffer { Token = ++session.State.NextOffer, Kind = OfferKind.Item, Id = catalog.Items.First(item => item.Rarity == ItemRarity.Mythic).Id, Price = 1 });
                    session.State.Offers.Add(new ShopOffer { Token = ++session.State.NextOffer, Kind = OfferKind.Item, Id = catalog.Items.First(item => item.Rarity == ItemRarity.Rare).Id, Price = 1 });
                }
                session.Validate(session.State); storage.SaveRun(session.State); storage.SaveProfile(new PlayerProfile());
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return null;
            owner = FindAnyObjectByType<ArenaController>();
        }
        private IEnumerator Start()
        {
            Application.logMessageReceived += OnLog; Application.runInBackground = true;
            string[] args = Environment.GetCommandLineArgs(); int path = Array.IndexOf(args, "-capturePath");
            if (path < 0 || path + 1 >= args.Length) { Application.Quit(1); yield break; }
            root = Path.GetFullPath(args[path + 1]); Directory.CreateDirectory(root);
            yield return K7PlayerCapture.ResizeDiagnostic(3840, 2160, Check);
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog"); var storage = new RunStorage(RunPaths.Root);
            yield return Load(catalog, storage, 5, false);
            int peak = 0; bool toad = false, crab = false; bool pauseChecked = false;
            var frameIntervals = new List<float>();
            for (int frame = 0; frame < 4; frame++)
            {
                float until = Time.realtimeSinceStartup + 6;
                float measureAfter = Time.realtimeSinceStartup + 0.5f;
                while (Time.realtimeSinceStartup < until && owner.Session.State.Phase == SessionPhase.Combat)
                {
                    float time = owner.Simulation.Elapsed;
                    owner.SetPresentationMoveTarget(new Vector2(Mathf.Cos(time * 0.8f) * 4.2f, Mathf.Sin(time * 0.8f) * 3));
                    peak = Mathf.Max(peak, owner.Simulation.Units.Count(unit => unit.Faction == Faction.Enemy));
                    toad |= owner.Simulation.Units.Any(unit => unit.Definition.Id == "FurnaceToad");
                    crab |= owner.Simulation.Units.Any(unit => unit.Definition.Id == "WardrobeCrab");
                    if (Time.realtimeSinceStartup > measureAfter && Time.unscaledDeltaTime > 0) frameIntervals.Add(Time.unscaledDeltaTime * 1000);
                    yield return null;
                }
                yield return Shot("01-horde-" + frame);
                if (!pauseChecked && owner.Session.State.Phase == SessionPhase.Combat)
                {
                    owner.TogglePause(); float elapsed = owner.Simulation.Elapsed;
                    yield return new WaitForSecondsRealtime(0.2f);
                    Check(owner.Simulation.Elapsed == elapsed, "Dense combat freezes on pause"); owner.TogglePause(); pauseChecked = true;
                }
            }
            Check(peak >= 15, "Native wave5 peak enemies=" + peak);
            Check(toad && crab, "Both new large hostile identities naturally spawned");
            if (frameIntervals.Count > 0)
            {
                frameIntervals.Sort();
                File.WriteAllText(Path.Combine(root, "Frame_Intervals.txt"), "Windows Unity framebuffer4K; single diagnostic wave5 tier2 party, not stress/device certification. Samples exclude first0.5s after screenshots and pause.\nSamples=" + frameIntervals.Count + " median_ms=" + frameIntervals[frameIntervals.Count / 2].ToString("0.00") + " p95_ms=" + frameIntervals[Mathf.Min(frameIntervals.Count - 1, Mathf.FloorToInt(frameIntervals.Count * 0.95f))].ToString("0.00") + " peak_enemies=" + peak + " GPU=" + SystemInfo.graphicsDeviceName);
            }
            foreach (int wave in new[] { 4, 8 })
            {
                yield return Load(catalog, storage, wave, false);
                Check(owner.Simulation.Boss.EntranceUntil > owner.Simulation.Elapsed, "Boss starts protected entrance wave" + wave);
                yield return new WaitForSecondsRealtime(0.45f); yield return Shot("02-boss-entry-" + wave);
                yield return new WaitForSecondsRealtime(1);
                Check(owner.Simulation.Boss.EntranceUntil == 0, "Boss entrance completes without reload wave" + wave);
                yield return Shot("03-boss-landed-" + wave);
            }
            yield return Load(catalog, storage, 5, true);
            yield return new WaitForSecondsRealtime(0.2f); yield return Shot("04-market-carpet-start");
            Check(owner.Shop.Animating, "Shop interactions locked during staged arrival");
            yield return new WaitForSecondsRealtime(0.7f); yield return Shot("04-market-stock-flight");
            yield return new WaitForSecondsRealtime(0.8f);
            owner.SetPresentationMoveTarget(new Vector2(-3, 0.5f)); yield return new WaitForSecondsRealtime(2);
            Check(owner.Shop.Nearby != null && owner.Shop.Nearby.Offer != null, "Native world shop focus fixture");
            yield return Shot("04-compact-shop");
            Check(owner.Shop.DescribeOffer(owner.Shop.Nearby.Offer).Length < 400, "Native short offer summary");
            Check(!string.IsNullOrEmpty(owner.Shop.BondPreview(owner.Shop.Nearby.Offer)), "Native candidate purchase highlights newly activated bond");
            var scene = SceneManager.GetActiveScene().handle;
            owner.Session.State.SpecialShopVisited = false;
            owner.Session.State.SpecialOffers.Add(new ShopOffer { Token = ++owner.Session.State.NextOffer, Kind = OfferKind.Item, Id = catalog.Items[0].Id, Currency = ShopCurrency.Eggs, Price = 2 });
            owner.EnterShopPresentation();
            yield return new WaitForSecondsRealtime(0.45f); yield return Shot("05-dream-market-rising");
            yield return new WaitForSecondsRealtime(1.2f);
            owner.SetPresentationMoveTarget(new Vector2(0, -4.3f)); yield return new WaitForSecondsRealtime(2.5f);
            Check(owner.Shop.Special && owner.Shop.Nearby != null && owner.Shop.Nearby.Action == "NEXT HALL", "Dream exit focus");
            Check(owner.Shop.Interact(), "Dream exit begins staged handoff");
            yield return new WaitForSecondsRealtime(0.45f); yield return Shot("05-dream-stock-retreat");
            Check(!owner.Session.State.SpecialShopVisited, "Dream market not swapped before departure completes");
            yield return new WaitForSecondsRealtime(0.75f); yield return Shot("05-dream-carpet-retreat");
            yield return new WaitForSecondsRealtime(0.55f); yield return Shot("05-gold-market-handoff");
            Check(owner.Session.State.SpecialShopVisited && !owner.Shop.Special, "Gold market arrives after dream exit");
            Check(SceneManager.GetActiveScene().handle == scene, "Dream-to-gold handoff keeps scene loaded");
            yield return new WaitForSecondsRealtime(1.6f);
            int previousWave = owner.Session.State.Wave;
            Check(owner.Shop.Interact(), "Gold doorway starts world-space departure");
            yield return new WaitForSecondsRealtime(0.6f); yield return Shot("06-hall-departure");
            Check(owner.Session.State.Phase == SessionPhase.Shop && owner.HallExitPending, "Combat waits for carpet and merchant exit");
            yield return new WaitForSecondsRealtime(0.85f); yield return Shot("06-next-wave-no-loading");
            Check(owner.Session.State.Phase == SessionPhase.Combat && owner.Session.State.Wave == previousWave + 1, "Doorway continues next combat wave");
            Check(SceneManager.GetActiveScene().handle == scene, "Next wave keeps scene loaded");
            notes.Add("Isolated saved wave5 tier2 six-variant party; natural enemies/combat, movement-only circle driver. Boss4/8 and shop5 use explicit separate saved fixtures. No HP/damage immunity/enemy budget/timer overrides. Not full-run victory, FPS certification, audio or human quality approval.");
            Check(!failed, "No Unity errors or failed runtime assertions during capture");
            File.WriteAllLines(Path.Combine(root, "CaptureNotes.txt"), notes); Application.Quit(failed ? 1 : 0);
        }
    }
}
