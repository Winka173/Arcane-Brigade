using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public sealed class K11PlayerCapture : MonoBehaviour
    {
        private ArenaController owner;
        private string root;
        private bool failed;
        private bool video;
        private K7FramebufferRecorder recorder;
        private readonly List<string> notes = new List<string>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (!RunPaths.K7Capture || Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-k11") < 0 || FindAnyObjectByType<K11PlayerCapture>() != null) return;
            var child = new GameObject("K11PlayerCapture"); DontDestroyOnLoad(child); child.AddComponent<K11PlayerCapture>();
        }
        private void OnLog(string message, string trace, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) failed = true; }
        private void OnDestroy() { Application.logMessageReceived -= OnLog; }
        private void Check(bool condition, string caption) { notes.Add((condition ? "PASS " : "FAIL ") + caption); failed |= !condition; }
        private void CheckMerchantFits()
        {
            Camera camera = FindAnyObjectByType<ArenaCamera>().GetComponent<Camera>(); Bounds bounds = owner.Shop.MerchantBounds;
            Vector3 minimum = camera.WorldToViewportPoint(bounds.min); Vector3 maximum = camera.WorldToViewportPoint(bounds.max);
            Check(minimum.x >= 0 && minimum.y >= 0 && maximum.x <= 1 && maximum.y <= 1, "Whole merchant silhouette fits viewport: " + owner.Shop.MerchantIdentity + " height " + owner.Shop.MerchantHeight);
        }
        private IEnumerator Shot(string name)
        {
            if (video) yield break;
            yield return new WaitForEndOfFrame();
            Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture();
            Check(texture.width == 3840 && texture.height == 2160, "Native 4K " + name);
            File.WriteAllBytes(Path.Combine(root, name + ".png"), texture.EncodeToPNG()); Destroy(texture);
        }
        private IEnumerator Start()
        {
            Application.logMessageReceived += OnLog; Application.runInBackground = true;
            string[] args = Environment.GetCommandLineArgs(); int index = Array.IndexOf(args, "-capturePath");
            if (index < 0 || index + 1 >= args.Length || Application.isBatchMode) { Application.Quit(1); yield break; }
            root = Path.GetFullPath(args[index + 1]); Directory.CreateDirectory(root);
            video = Array.IndexOf(args, "-arcane-k11-video") >= 0;
            yield return K7PlayerCapture.ResizeDiagnostic(video ? 1920 : 3840, video ? 1080 : 2160, Check);
            K7PlayerCapture.ShowDiagnosticWindow();
            RunCatalog content = Resources.Load<RunCatalog>("RunCatalog"); var storage = new RunStorage(RunPaths.Root);
            using (var session = new RunSession(content))
            {
                session.Start(content.Champions[0].Id, 111077); session.State.Phase = SessionPhase.Shop;
                session.State.Gold = 1000; session.State.Eggs = 10; session.State.Offers.Clear(); session.State.SpecialOffers.Clear();
                for (int offer = 0; offer < 3; offer++) session.State.Offers.Add(new ShopOffer { Token = ++session.State.NextOffer, Kind = OfferKind.Companion, Id = content.Companions[offer].Id, Price = 1 });
                session.State.SpecialOffers.Add(new ShopOffer { Token = ++session.State.NextOffer, Kind = OfferKind.Item, Id = content.Items[0].Id, Currency = ShopCurrency.Eggs, Price = 1 });
                session.Validate(session.State); storage.SaveRun(session.State); storage.SaveProfile(new PlayerProfile());
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(0.1f);
            owner = FindAnyObjectByType<ArenaController>();
            if (video) { recorder = gameObject.AddComponent<K7FramebufferRecorder>(); recorder.Begin(38); }
            var scene = SceneManager.GetActiveScene().handle; ArenaController identity = owner;
            owner.SetPresentationMoveTarget(Vector2.zero);
            yield return Shot("01-dream-arrival"); yield return new WaitForSecondsRealtime(1);
            Check(owner.Shop.Special && !owner.Session.State.SpecialShopVisited, "Dream merchant appears before regular shop");
            Check(owner.Shop.MerchantIdentity == "GiantEggMerchant" && owner.Shop.MerchantHeight >= 5, "New giant dream oracle is loaded at colossal scale");
            CheckMerchantFits();
            Check(!owner.Shop.Stations.AnyStation("GOLD SHOP") && !owner.Shop.Stations.AnyStation("DREAM ALTAR"), "No alternate-shop switch stations");
            yield return Shot("02-dream-open");
            owner.SetPresentationMoveTarget(new Vector2(-3, 0.5f)); yield return new WaitForSecondsRealtime(2);
            Check(owner.Shop.Nearby != null && owner.Shop.Nearby.Offer != null && owner.Shop.Interact(), "Actual egg purchase at dream merchant");
            owner.SetPresentationMoveTarget(new Vector2(0, -4.3f)); yield return new WaitForSecondsRealtime(2);
            Check(owner.Shop.Nearby != null && owner.Shop.Nearby.Action == "NEXT HALL" && owner.Shop.Interact(), "Dream exit starts animated departure");
            yield return new WaitForSecondsRealtime(0.25f); yield return Shot("03-dream-fold");
            yield return new WaitForSecondsRealtime(1.2f);
            Check(!owner.Shop.Special && owner.Session.State.SpecialShopVisited && owner.Session.State.Phase == SessionPhase.Shop, "Regular merchant follows dream merchant without advancing wave");
            Check(owner.Shop.MerchantIdentity == "GiantGoldMerchant" && owner.Shop.MerchantHeight > 4, "New many-footed gold merchant has distinct large scale");
            CheckMerchantFits();
            using (var validation = new RunSession(content)) Check(storage.LoadRun(validation).SpecialShopVisited, "Committed regular stage survives storage");
            yield return Shot("04-gold-open");
            owner.SetPresentationMoveTarget(new Vector2(-3, 0.5f)); yield return new WaitForSecondsRealtime(2);
            Check(owner.Shop.Nearby != null && owner.Shop.Nearby.Offer != null && owner.Shop.Interact(), "Actual recruit purchase in following regular shop");
            yield return Shot("05-recruit-arrival");
            owner.SetPresentationMoveTarget(new Vector2(0, -4.3f)); yield return new WaitForSecondsRealtime(2);
            Vector2 before = owner.Simulation.Champion.Position;
            Check(owner.Shop.Nearby != null && owner.Shop.Nearby.Action == "NEXT HALL" && owner.Shop.Interact(), "Regular exit starts shop fold instead of loading");
            yield return new WaitForSecondsRealtime(0.35f); yield return Shot("06-gold-fold");
            Check(owner.HallCurtain == 0, "No opaque transition curtain");
            yield return new WaitForSecondsRealtime(0.8f);
            Check(owner.Session.State.Wave == 2 && owner.Session.State.Phase == SessionPhase.Combat, "Same-scene transition starts wave two");
            Check(Vector2.Distance(before, owner.Simulation.Champion.Position) < 2, "Player does not teleport to new arena origin");
            Check(SceneManager.GetActiveScene().handle == scene && ReferenceEquals(owner, identity), "Scene and controller identity unchanged through both merchants and next wave");
            yield return new WaitForSecondsRealtime(3); yield return Shot("07-next-wave");
            bool hostileSeen = false; bool hostileGripsHidden = true;
            foreach (UnitVisual visual in FindObjectsByType<UnitVisual>())
                if (visual.State != null && visual.State.Faction == Faction.Enemy)
                {
                    hostileSeen = true; Transform grip = visual.transform.Find("GripRoot"); hostileGripsHidden &= grip != null && !grip.gameObject.activeSelf;
                }
            Check(hostileSeen && hostileGripsHidden, "Live hostile visuals have no held weapons");
            owner.SetPresentationMoveTarget(Vector2.zero);
            while (owner.Session.State.Phase == SessionPhase.Combat && owner.Simulation.Elapsed < owner.Simulation.WaveDuration + 1) yield return null;
            yield return new WaitForSecondsRealtime(0.65f); yield return Shot("08-wave-end-arrival");
            Check(SceneManager.GetActiveScene().handle == scene && ReferenceEquals(owner, identity), "Wave end also keeps scene/controller");
            Check(owner.Session.State.Phase == SessionPhase.Shop, "Natural combat completion returns to living market");
            if (!video && owner.Session.State.Phase == SessionPhase.Shop)
            {
                owner.Session.State.SpecialShopVisited = true; owner.Shop.Display(); yield return new WaitForSecondsRealtime(0.8f);
                Check(owner.Shop.MerchantIdentity == "ReceiptMaw" && owner.Shop.MerchantHeight == WorldShop.MerchantSize(false, 2), "Different merchant silhouette and size in hall two");
                yield return Shot("09-classic-large");
                owner.Session.State.Wave = 3; owner.Session.State.SpecialShopVisited = false;
                if (owner.Session.State.SpecialOffers.Count == 0) owner.Session.State.SpecialOffers.Add(new ShopOffer { Token = ++owner.Session.State.NextOffer, Kind = OfferKind.Item, Id = content.Items[0].Id, Currency = ShopCurrency.Eggs, Price = 1 });
                owner.EnterShopPresentation(); yield return new WaitForSecondsRealtime(0.9f);
                Check(owner.Shop.MerchantHeight > 5.5f, "Dream merchant reaches biggest authored size in hall three showcase");
                CheckMerchantFits();
                yield return Shot("10-colossal-dream");
                Check(owner.FinishSpecialShop(), "Showcase regular stage still uses committed transaction");
                yield return new WaitForSecondsRealtime(0.9f);
                Check(owner.Shop.MerchantHeight > 5 && !owner.Shop.Special, "Gold merchant also has colossal size variant");
                CheckMerchantFits();
                yield return Shot("11-colossal-gold");
            }
            if (video) { while (!recorder.Complete) yield return null; Check(recorder.Passed, "Realtime transition framebuffer movie completed"); }
            notes.Add("Isolated saved fixture, actual purchases/transitions and natural wave2 combat. Last three stills are explicit hall2/3 merchant-size showcases, not claimed natural hall3 progression. No human art/feel approval or mobile certification. No OS/desktop capture.");
            File.WriteAllLines(Path.Combine(root, "CaptureNotes.txt"), notes); Application.Quit(failed ? 1 : 0);
        }
    }
    public static class K11StationChecks
    {
        public static bool AnyStation(this IReadOnlyList<WorldShop.Station> stations, string action)
        {
            foreach (WorldShop.Station station in stations) if (station.Action == action) return true;
            return false;
        }
    }
}
