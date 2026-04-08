using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public sealed class K3PlayerCapture : MonoBehaviour
    {
        private bool failed;
        private string root;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (!RunPaths.K3Capture || FindAnyObjectByType<K3PlayerCapture>() != null) return;
            var runner = new GameObject("K3PlayerCapture");
            DontDestroyOnLoad(runner);
            runner.AddComponent<K3PlayerCapture>();
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
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 131026);
                session.State.Phase = SessionPhase.Shop;
                session.State.Gold = 180;
                session.State.Eggs = 12; session.State.SpecialShopVisited = true; // the gold-shop fixtures; the dream stage is shown below
                if (RunPaths.K32Capture) session.State.Wave = 10;
                session.Reroll();
                session.State.RerollsThisShop = 0;
                if (RunPaths.K32Capture)
                {
                    session.State.Offers[1].Id = "echo-postage";
                    session.State.Offers[1].Price = catalog.Item("echo-postage").Price;
                    session.State.Items.Add(new ItemStack { Id = "key-oil", Count = 1 });
                    session.State.Items.Add(new ItemStack { Id = "clock-spring", Count = 1 });
                }
                new RunStorage(RunPaths.Root).SaveRun(session.State);
            }
            yield return new WaitForSecondsRealtime(1);
            RunLaunch.Resume = true;
            SceneManager.LoadScene("RunArena");
            yield return new WaitForSecondsRealtime(2);
            ArenaController owner = FindAnyObjectByType<ArenaController>();
            if (owner == null) { Debug.LogError("K3 capture: arena controller missing"); Application.Quit(1); yield break; }
            WorldShop.Station station = null;
            owner.Shop.Display();
            foreach (WorldShop.Station candidate in owner.Shop.Stations)
                if (candidate.Offer != null && candidate.Offer.Kind == OfferKind.Item) { station = candidate; break; }
            if (station == null) { Debug.LogError("K3 capture: item stand missing"); Application.Quit(1); yield break; }
            owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = station.Position;
            if (RunPaths.K32Capture)
            {
                owner.Shop.Display();
                yield return new WaitForSecondsRealtime(0.2f);
                yield return new WaitForEndOfFrame();
                Capture("Tradeoff_Proximity_Tooltip_Fixture");
            }
            if (!owner.Shop.Interact()) { failed = true; Debug.LogError("Native physical stand purchase failed"); }
            yield return new WaitForEndOfFrame();
            owner.ShowReceiptFixture();
            yield return new WaitForEndOfFrame();
            Capture("World_Stand_Receipt_Fixture");
            owner.ToggleDetails();
            owner.Details.SelectTab(2);
            if (RunPaths.K32Capture) owner.Details.MoveSelection(2);
            yield return new WaitForEndOfFrame();
            Capture("Details_Receipt_History_Fixture");
            if (RunPaths.K32Capture)
            {
                owner.Details.SelectTab(3);
                yield return new WaitForEndOfFrame();
                Capture("Details_Build_Links_Fixture");
            }
            owner.ToggleDetails();
            if (RunPaths.K32Capture)
            {
                owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = new Vector2(0, 2.3f);
                int cost = owner.Session.RerollCost;
                int gold = owner.Session.State.Gold;
                if (!owner.Shop.Interact() || owner.Session.State.Gold != gold - cost || owner.Session.RerollCost != cost + catalog.RerollPriceStep) { failed = true; Debug.LogError("Native reroll sign/payment mismatch"); }
                yield return new WaitForSecondsRealtime(0.2f);
                yield return new WaitForEndOfFrame();
                Capture("Reroll_Increased_Price_Fixture");
            }
            using (var loaded = new RunSession(catalog))
            {
                if (RunPaths.AltarCapture)
                {
                    owner.Session.State.SpecialOffers.Clear();
                    for (int index = 0; index < 3; index++) owner.Session.State.SpecialOffers.Add(new ShopOffer { Token = ++owner.Session.State.NextOffer, Id = index == 0 ? "long-atlas" : "honey-pot", Kind = OfferKind.Item, Price = index == 0 ? 12 : 2, Currency = index == 0 ? ShopCurrency.Health : ShopCurrency.Eggs });
                    owner.Session.State.SpecialShopVisited = false; owner.Shop.Display(); // two-stage shop: the dream market is its own stage, no portal
                    if (!owner.Shop.Special) { failed = true; Debug.LogError("Native dream stage failed"); }
                    owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = new Vector2(-3, 0.5f);
                    owner.Shop.Display();
                    yield return new WaitForSecondsRealtime(0.2f);
                    yield return new WaitForEndOfFrame();
                    Capture("Altar_HP_Before_Fixture");
                    float hp = owner.Session.State.Health;
                    int gold = owner.Session.State.Gold;
                    int eggs = owner.Session.State.Eggs;
                    if (!owner.Shop.Interact() || owner.Session.State.Health != hp - 12 || owner.Session.State.Gold != gold || owner.Session.State.Eggs != eggs) { failed = true; Debug.LogError("Native HP altar purchase failed"); }
                    yield return new WaitForEndOfFrame();
                    owner.ShowReceiptFixture();
                    yield return new WaitForEndOfFrame();
                    Capture("Altar_HP_Receipt_Fixture");
                }
            }
            using (var loaded = new RunSession(catalog))
            {
                RunSnapshot saved = new RunStorage(RunPaths.Root).LoadRun(loaded);
                if (saved == null || JsonUtility.ToJson(saved) != JsonUtility.ToJson(owner.Session.State)) { failed = true; Debug.LogError("Native receipt boundary differs from disk"); }
            }
            File.WriteAllText(Path.Combine(root, "Capture_Notes.txt"), "Rendered Windows Unity player. Isolated diagnostic save, seeded shop/wallet, optional tradeoff/owned-item, mixed-currency altar and position fixtures; purchase/reroll uses WorldShop.Interact and real save boundary. Not a full-run playtest or mobile certification.\nResult: " + (failed ? "FAIL" : "PASS"));
            Debug.Log(failed ? "ARCANE_K3_CAPTURE_FAILED" : "ARCANE_K3_CAPTURE_PASSED");
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
