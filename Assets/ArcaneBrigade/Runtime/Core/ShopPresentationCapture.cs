using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public sealed class ShopPresentationCapture : MonoBehaviour
    {
        private string root;
        private bool failed;
        private readonly List<string> notes = new List<string>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            string[] args = Environment.GetCommandLineArgs(); int at = Array.IndexOf(args, "-capturePath");
            if (!RunPaths.K7Capture || Array.IndexOf(args, "-arcane-shop-presentation-capture") < 0 || at < 0 || at + 1 >= args.Length || FindAnyObjectByType<ShopPresentationCapture>() != null) return;
            var child = new GameObject("ShopPresentationNativeReview"); DontDestroyOnLoad(child);
            child.AddComponent<ShopPresentationCapture>().root = Path.GetFullPath(args[at + 1]);
        }
        private void Check(bool valid, string caption)
        {
            notes.Add((valid ? "PASS " : "FAIL ") + caption); failed |= !valid;
            if (!valid) { File.WriteAllLines(Path.Combine(root, "Native_Checks.txt"), notes); throw new InvalidOperationException("Shop presentation: " + caption); }
        }
        private void Log(string message, string trace, LogType type)
        {
            if (type != LogType.Exception && type != LogType.Error && type != LogType.Assert) return;
            failed = true; notes.Add("FAIL Runtime " + message); File.WriteAllLines(Path.Combine(root, "Native_Checks.txt"), notes); Application.Quit(1);
        }
        private void OnDestroy() { Application.logMessageReceived -= Log; }
        private IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            foreach (ShopRarityGlow glow in FindObjectsByType<ShopRarityGlow>())
            {
                Check(glow.Target != null && ShopRarityGlow.CanDecorate(glow.Target), "Only shop/display targets carry rarity " + name);
                Check(!glow.StockPresentation || glow.UsesPaintedAura, "Actual stock rarity uses painted animation " + name);
                Check(glow.GetComponentsInChildren<LineRenderer>().Length == 0, "Rarity has no target circles " + name);
                Check(Vector3.Distance(glow.transform.position, glow.Target.bounds.center) < .001f, "Rarity follows animated merchandise " + name);
            }
            Texture2D frame = ScreenCapture.CaptureScreenshotAsTexture();
            Check(frame.width == 1920 && frame.height == 1080, "Native dimensions " + name);
            File.WriteAllBytes(Path.Combine(root, name + ".png"), frame.EncodeToPNG()); Destroy(frame);
            File.WriteAllLines(Path.Combine(root, "Native_Checks.txt"), notes);
        }
        private IEnumerator Walk(ArenaController owner, Vector2 target)
        {
            owner.SetPresentationMoveTarget(target); float until = Time.realtimeSinceStartup + 7;
            while (Vector2.Distance(owner.Simulation.Champion.Position, target) > .3f && Time.realtimeSinceStartup < until) yield return null;
            owner.SetPresentationMoveTarget(null); yield return new WaitForSecondsRealtime(.4f);
            yield return new WaitForEndOfFrame();
            Check(Vector2.Distance(owner.Simulation.Champion.Position, target) < .5f, "Player reaches physical interaction " + target);
        }
        private IEnumerator Start()
        {
            Directory.CreateDirectory(root); Application.logMessageReceived += Log; Application.runInBackground = true;
            yield return new WaitForSecondsRealtime(2.6f);
            bool reduced = Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-shop-presentation-reduced") >= 0;
            var storage = new RunStorage(RunPaths.Root);
            storage.SaveProfile(new PlayerProfile { ReducedMotion = reduced, ReducedEffects = reduced, ReducedFlashes = reduced });
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            using (var session = new RunSession(catalog))
            {
                session.Start("LanternKnight", 171026); session.State.Wave = 7; session.State.Phase = SessionPhase.Shop; session.State.Gold = 20000;
                session.State.SpecialShopVisited = true; session.State.SpecialOffers.Clear(); session.State.Offers.Clear();
                UnitDefinition ally = catalog.Companions.First(unit => unit.Rarity == ItemRarity.Rare && !unit.GiftOnly);
                ItemDefinition blue = catalog.Items.First(item => item.Rarity == ItemRarity.Uncommon && (item.Pools & ShopPool.Gold) != 0);
                ItemDefinition gold = catalog.Items.First(item => item.Rarity == ItemRarity.Mythic && (item.Pools & ShopPool.Gold) != 0);
                session.State.Offers.Add(new ShopOffer { Token = ++session.State.NextOffer, Kind = OfferKind.Item, Id = blue.Id, Currency = ShopCurrency.Gold, Price = 10 });
                session.State.Offers.Add(new ShopOffer { Token = ++session.State.NextOffer, Kind = OfferKind.Companion, Id = ally.Id, Currency = ShopCurrency.Gold, Price = 10 });
                session.State.Offers.Add(new ShopOffer { Token = ++session.State.NextOffer, Kind = OfferKind.Item, Id = gold.Id, Currency = ShopCurrency.Gold, Price = 10 });
                storage.SaveRun(session.State);
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return null; yield return new WaitForSecondsRealtime(3);
            ArenaController owner = FindAnyObjectByType<ArenaController>();
            Check(owner != null && owner.Session.State.Phase == SessionPhase.Shop, "Actual physical shop opens");
            yield return Shot("shop-overview");
            WorldShop.Station exit = owner.Shop.Stations.First(station => station.Action == "NEXT HALL");
            Check(Vector2.Distance(exit.Position, WorldShop.ExitPosition + Vector2.up * WorldShop.LayoutLift) < .01f, "Actual continue door uses separate right-hand lane");
            foreach (WorldShop.Station offer in owner.Shop.Stations.Where(station => station.Offer != null))
            {
                Check(Vector2.Distance(offer.Position, exit.Position) > offer.Reach + exit.Reach + .3f, "Stock and exit have disjoint interaction regions");
                yield return Walk(owner, offer.Position);
                Check(owner.Shop.Nearby == offer && !owner.HallExitPending, "Approaching stock never selects continue");
                Check(offer.Actor.FocusAmount > .9f, "Actual merchandise focus animates on approach");
                ShopDisplayActor stand = offer.Marker.GetComponentsInChildren<ShopDisplayActor>().First(actor => actor != offer.Actor);
                Vector3 foot = offer.Actor.transform.TransformPoint(WorldSpriteGrounding.Foot(offer.Actor.View.sprite));
                Check(Vector3.Distance(foot, stand.SeatPosition) < .04f, "Highlight preserves painted support contact");
                yield return Shot("focused-" + offer.Offer.Kind + "-" + offer.Offer.Id);
            }
            WorldShop.Station recruit = owner.Shop.Stations.First(station => station.Offer != null && station.Offer.Kind == OfferKind.Companion);
            yield return Walk(owner, recruit.Position); int count = owner.Session.State.Roster.Count;
            Check(owner.Shop.Interact(), "Actual rare ally purchase commits");
            yield return new WaitForSecondsRealtime(1.4f);
            Check(owner.Session.State.Roster.Count == count + 1, "Bought ally enters owned roster");
            Check(!FindObjectsByType<ShopRarityGlow>().Any(glow => glow.Target != null && glow.Target.GetComponentInParent<UnitVisual>() != null), "Purchased ally loses stock rarity immediately");
            yield return Shot("owned-ally-no-rarity");
            yield return Walk(owner, exit.Position);
            Check(owner.Shop.Nearby != null && owner.Shop.Nearby.Action == "NEXT HALL", "Separate doorway remains reachable");
            yield return Shot("separate-exit-focused");
            Check(owner.Shop.Interact(), "Deliberate continue interaction commits");
            yield return new WaitForSecondsRealtime(.2f); yield return Shot("doorway-departure");
            float deadline = Time.realtimeSinceStartup + 8;
            while (owner.Session.State.Phase != SessionPhase.Combat && Time.realtimeSinceStartup < deadline) yield return null;
            yield return new WaitForSecondsRealtime(.5f);
            Check(owner.Session.State.Phase == SessionPhase.Combat, "Continue returns to real combat without scene loading");
            Check(FindObjectsByType<ShopRarityGlow>().Length == 0, "No shop rarity aura leaks into combat");
            yield return Shot("combat-no-shop-aura");
            Check(!failed, "Shop presentation native review complete"); File.WriteAllLines(Path.Combine(root, "Native_Checks.txt"), notes); Application.Quit(0);
        }
    }
}
