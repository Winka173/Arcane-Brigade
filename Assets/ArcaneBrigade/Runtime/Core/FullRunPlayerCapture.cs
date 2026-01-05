using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public sealed class FullRunPlayerCapture : MonoBehaviour
    {
        private bool failed;
        public static bool Requested => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-ui-capture") >= 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (!Requested) return;
            Application.runInBackground = true;
            var runner = new GameObject("FullRunPlayerCapture");
            DontDestroyOnLoad(runner);
            runner.AddComponent<FullRunPlayerCapture>();
        }

        private void Awake() { Application.logMessageReceived += OnLog; }
        private void OnDestroy() { Application.logMessageReceived -= OnLog; }
        private void OnLog(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) failed = true;
        }

        private IEnumerator Start()
        {
            if (Application.isBatchMode) { Debug.LogError("UI capture requires a rendered player; diagnostic saves are isolated."); Application.Quit(1); yield break; }
            string[] args = Environment.GetCommandLineArgs();
            int pathIndex = Array.IndexOf(args, "-capturePath");
            if (pathIndex < 0 || pathIndex + 1 >= args.Length) { Application.Quit(1); yield break; }
            string root = Path.GetFullPath(args[pathIndex + 1]);
            Directory.CreateDirectory(root);
            yield return new WaitForSecondsRealtime(1f);
            yield return new WaitForEndOfFrame();
            CaptureFrame(Path.Combine(root, "Unity_FullRun_Menu_UI.png"));
            SceneManager.LoadScene("RunArena");
            yield return new WaitForSecondsRealtime(3f);
            ArenaController owner = FindAnyObjectByType<ArenaController>();
            if (owner == null || owner.Session == null) { Application.Quit(1); yield break; }
            yield return new WaitForEndOfFrame();
            CaptureFrame(Path.Combine(root, "Unity_FullRun_Combat_UI.png"));
            yield return new WaitForSecondsRealtime(2f);
            owner.Session.State.Phase = SessionPhase.Shop;
            owner.Session.State.Gold = 180;
            owner.Session.Reroll();
            owner.EnterShopPresentation();
            owner.Shop.Display();
            if (owner.Shop.Interact()) { failed = true; Debug.LogError("Shop allowed interaction outside station radius"); }
            ShopOffer recruit = owner.Session.State.Offers.Find(offer => offer.Kind == OfferKind.Companion);
            ShopOffer upgrade = owner.Session.State.Offers.Find(offer => offer.Kind == OfferKind.Item);
            if (recruit != null)
            {
                foreach (WorldShop.Station station in owner.Shop.Stations)
                    if (station.Offer == recruit) { owner.Simulation.Champion.Position = station.Position; break; }
                if (!owner.Shop.Interact()) { failed = true; Debug.LogError("Spatial recruit purchase failed"); }
            }
            if (upgrade != null)
            {
                foreach (WorldShop.Station station in owner.Shop.Stations)
                    if (station.Offer == upgrade) { owner.Simulation.Champion.Position = station.Position; break; }
                if (!owner.Shop.Interact()) { failed = true; Debug.LogError("Spatial item purchase failed"); }
            }
            yield return new WaitForSecondsRealtime(1f);
            yield return new WaitForEndOfFrame();
            CaptureFrame(Path.Combine(root, "Unity_FullRun_Shop_UI.png"));
            owner.Session.State.Eggs = 6;
            owner.Session.State.SpecialOffers.Add(new ShopOffer { Token = ++owner.Session.State.NextOffer, Kind = OfferKind.Item, Id = owner.Content.Items[0].Id, Price = 2, Currency = ShopCurrency.Eggs });
            owner.Session.State.SpecialOffers.Add(new ShopOffer { Token = ++owner.Session.State.NextOffer, Kind = OfferKind.Item, Id = owner.Content.Items[1].Id, Price = 2, Currency = ShopCurrency.Eggs });
            owner.Session.State.SpecialOffers.Add(new ShopOffer { Token = ++owner.Session.State.NextOffer, Kind = OfferKind.Item, Id = owner.Content.Items[2].Id, Price = 2, Currency = ShopCurrency.Eggs });
            owner.Shop.Display();
            owner.Simulation.Champion.Position = new Vector2(-7, 1);
            owner.Shop.Interact();
            int previousGold = owner.Session.State.Gold;
            owner.Simulation.Champion.Position = new Vector2(-3, 0.5f);
            if (!owner.Shop.Interact() || owner.Session.State.Gold != previousGold || owner.Session.State.Eggs != 4) { failed = true; Debug.LogError("Special purchase did not use separate egg wallet"); }
            yield return new WaitForSecondsRealtime(0.5f);
            yield return new WaitForEndOfFrame();
            CaptureFrame(Path.Combine(root, "Unity_FullRun_Egg_Shop_UI.png"));
            yield return new WaitForSecondsRealtime(2f);
            owner.Session.State.Wave = 3;
            foreach (UnitDefinition unit in owner.Content.Companions)
                if (!owner.Session.State.Roster.Exists(entry => entry.UnitId == unit.Id))
                    owner.Session.State.Roster.Add(new RosterEntry { UnitId = unit.Id, WeaponId = unit.Weapon.Id });
            owner.NextWave();
            yield return new WaitForSecondsRealtime(2.4f);
            yield return new WaitForEndOfFrame();
            CaptureFrame(Path.Combine(root, "Unity_FullRun_Boss_UI.png"));
            if (Array.IndexOf(args, "-arcane-motion-capture") >= 0)
            {
                string frames = Path.Combine(root, "PresentationMotion");
                Directory.CreateDirectory(frames);
                for (int index = 0; index < 48; index++)
                {
                    yield return new WaitForSecondsRealtime(0.08f);
                    yield return new WaitForEndOfFrame();
                    CaptureFrame(Path.Combine(frames, "frame_" + index.ToString("000") + ".png"));
                }
            }
            yield return new WaitForSecondsRealtime(2f);
            Debug.Log(failed ? "ARCANE_UI_CAPTURE_FAILED" : "ARCANE_UI_CAPTURE_PASSED (shop is a diagnostic fixture)");
            Application.Quit(failed ? 1 : 0);
        }

        private void CaptureFrame(string path)
        {
            Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
            if (screenshot == null) { failed = true; Debug.LogError("Missing rendered screenshot"); return; }
            Color32[] pixels = screenshot.GetPixels32();
            int minimum = 255;
            int maximum = 0;
            for (int index = 0; index < pixels.Length; index += 173)
            {
                Color32 color = pixels[index];
                int brightness = (color.r + color.g + color.b) / 3;
                minimum = Math.Min(minimum, brightness);
                maximum = Math.Max(maximum, brightness);
            }
            if (maximum - minimum < 20) { failed = true; Debug.LogError("Blank screenshot rejected: " + path); }
            else File.WriteAllBytes(path, screenshot.EncodeToPNG());
            Destroy(screenshot);
        }
    }
}
