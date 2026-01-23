using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public sealed class K17PresentationCapture : MonoBehaviour
    {
        private ArenaController owner;
        private string root;
        private bool failed;
        private readonly List<string> notes = new List<string>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (!RunPaths.K7Capture || Array.IndexOf(args, "-arcane-k17-presentation") < 0 || FindAnyObjectByType<K17PresentationCapture>() != null) return;
            int path = Array.IndexOf(args, "-capturePath"); if (path < 0 || path + 1 >= args.Length) return;
            var child = new GameObject("K17PresentationCapture"); DontDestroyOnLoad(child);
            child.AddComponent<K17PresentationCapture>().root = Path.GetFullPath(args[path + 1]);
        }
        private void Check(bool condition, string caption) { failed |= !condition; notes.Add((condition ? "PASS " : "FAIL ") + caption); }
        private void Log(string message, string trace, LogType kind) { if (kind == LogType.Error || kind == LogType.Exception || kind == LogType.Assert) { failed = true; notes.Add("FAIL " + message); } }
        private void OnDestroy() { Application.logMessageReceived -= Log; }
        private IEnumerator Capture(string name)
        {
            yield return new WaitForEndOfFrame();
            Texture2D image = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(root, name + ".png"), image.EncodeToPNG()); Destroy(image);
        }
        private IEnumerator Start()
        {
            Application.logMessageReceived += Log; Application.runInBackground = true; Directory.CreateDirectory(root);
            yield return K7PlayerCapture.ResizeDiagnostic(3840, 2160, Check);
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            var storage = new RunStorage(RunPaths.Root);
            using (var session = new RunSession(catalog))
            {
                session.Start("LanternKnight", 171077); session.State.Wave = 4; session.State.Phase = SessionPhase.Shop;
                session.State.SpecialShopVisited = true; session.State.SpecialOffers.Clear(); session.State.Offers.Clear();
                session.State.Gold = 500;
                ItemDefinition rare = catalog.Items.First(item => item.Rarity == ItemRarity.Rare);
                ItemDefinition gold = catalog.Items.First(item => item.Rarity == ItemRarity.Mythic);
                session.State.Offers.Add(new ShopOffer { Token = ++session.State.NextOffer, Kind = OfferKind.Item, Id = rare.Id, Price = 50 });
                session.State.Offers.Add(new ShopOffer { Token = ++session.State.NextOffer, Kind = OfferKind.Item, Id = gold.Id, Price = 90 });
                session.State.Offers.Add(new ShopOffer { Token = ++session.State.NextOffer, Kind = OfferKind.Companion, Id = "CoilKettle", Price = 55 });
                storage.SaveRun(session.State);
                bool reduced = Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-k17-reduced") >= 0;
                storage.SaveProfile(new PlayerProfile { ReducedMotion = reduced, ReducedEffects = reduced, ReducedShake = reduced, ReducedFlashes = reduced });
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return null;
            owner = FindAnyObjectByType<ArenaController>();
            var scene = SceneManager.GetActiveScene().handle;
            yield return new WaitForSecondsRealtime(0.15f); yield return Capture("shop-arrival-early");
            yield return new WaitForSecondsRealtime(0.6f); yield return Capture("shop-arrival-curl");
            yield return new WaitForSecondsRealtime(1.3f); yield return Capture("shop-ready");
            Check(FindObjectsByType<ShopRarityGlow>().Length >= 3, "Rare, gold and rare-companion auras exist in actual shop");
            Check(FindObjectsByType<ShopDisplayActor>().Count(actor => actor.GetComponent<SpriteRenderer>()?.sprite?.texture.name == "MarketStands_v1") == 3, "Three authored animated pedestals render in actual shop");
            Check(owner.Shop.Stations.Count(station => station.Offer != null) == 3 && owner.Shop.Stations.All(station => station.Recipe == null), "Normal shop has three stock choices and no unusable recipe clutter");
            foreach (WorldShop.Station station in owner.Shop.Stations.Where(station => station.Offer != null))
                Check(station.Actor.GetComponent<SpriteRenderer>().bounds.max.y < owner.Shop.MerchantCenter.y - owner.Shop.MerchantHeight * 0.5f, "Stock body stays below merchant " + station.Offer.Token);
            WorldShop.Station middle = owner.Shop.Stations.First(station => station.Offer != null && station.Offer.Kind == OfferKind.Item && catalog.Item(station.Offer.Id).Rarity == ItemRarity.Mythic);
            owner.SetPresentationMoveTarget(middle.Position); yield return new WaitForSecondsRealtime(1);
            Check(owner.Shop.Nearby == middle, "Physical proximity focuses middle stock");
            yield return Capture("shop-focus-gold");
            int count = owner.Session.State.Items.Sum(item => item.Count);
            Check(owner.Shop.Interact(), "Item purchase commits through normal interaction");
            yield return new WaitForSecondsRealtime(1.7f);
            Check(owner.Session.State.Items.Sum(item => item.Count) == count + 1, "Purchased item appears in inventory");
            WorldShop.Station doorway = owner.Shop.Stations.First(station => station.Action == "NEXT HALL");
            owner.SetPresentationMoveTarget(doorway.Position); yield return new WaitForSecondsRealtime(1.3f);
            Check(owner.Shop.Interact(), "Physical downward exit starts choreography");
            owner.SetPresentationMoveTarget(Vector2.zero);
            yield return new WaitForSecondsRealtime(0.4f); yield return Capture("shop-exit");
            yield return new WaitForSecondsRealtime(1.2f); yield return Capture("courier-pickup");
            yield return new WaitForSecondsRealtime(0.8f); yield return Capture("courier-flight");
            Vector3 visible = Camera.main.WorldToViewportPoint(owner.VisualChampionPosition);
            Check(visible.x > 0.05f && visible.x < 0.95f && visible.y > 0.05f && visible.y < 0.95f, "Camera follows passenger during flight");
            yield return new WaitForSecondsRealtime(1.9f); yield return Capture("new-map-combat");
            Check(owner.Session.State.Wave == 5 && owner.Session.State.Phase == SessionPhase.Combat, "Map transfer completes and next combat starts");
            Check(SceneManager.GetActiveScene().handle == scene, "Same loaded scene throughout shop, exit and map transfer");
            Material floor = GameObject.Find("HD_TiledFloor").GetComponent<MeshRenderer>().material;
            Check(floor.GetTexture("_NextTex").name == "ClockworkFloor_v1" && floor.GetFloat("_FloorBlend") > 0.99f, "Different painted map material finishes dissolving");
            if (!PresentationSettings.ReducedMotion)
            {
                owner.Session.CombatCatalog.EnemyBudget = 0;
                owner.Simulation.Champion.Definition.Health = owner.Simulation.Champion.Health = 5000;
                foreach (UnitState unit in owner.Simulation.Units) if (unit.Faction != Faction.Enemy) unit.NextAttack = float.PositiveInfinity;
                int bossIndex = 0;
                foreach (UnitDefinition boss in catalog.Bosses.Where(unit => unit.BossPattern >= 5))
                {
                    Vector2 position = new Vector2((bossIndex % 3 - 1) * 3.8f, bossIndex < 3 ? 1.7f : -1.5f);
                    owner.Simulation.AddExpandedPresentationFixture(boss, position, 10000); bossIndex++;
                }
                bool rendered = false;
                for (int sample = 0; sample < 60 && !rendered; sample++)
                {
                    yield return new WaitForSecondsRealtime(0.1f);
                    rendered = owner.Simulation.ExpandedFields.Any(field => field.Kind == ExpandedFieldKind.FallingProp && field.At > owner.Simulation.Elapsed);
                }
                yield return Capture("boss-body-attacks");
                Check(rendered && FindAnyObjectByType<ExpandedVisual>().VisibleCount > 0, "Actual body-attack fields render in native player");
                Check(FindObjectsByType<SpriteRenderer>().Any(renderer => renderer.name == "ThrownLuggage" && renderer.enabled && renderer.isVisible), "Thrown objects have visible native sprites");
                notes.Add("Final effect frame uses five simultaneous boss fixtures and boosted fixture health. It is not a normal encounter, difficulty or performance claim.");
            }
            notes.Add("Seeded presentation fixture; screenshots and runtime invariants are not human animation/audio approval.");
            Check(!failed, "No runtime errors in shop and seamless transport");
            File.WriteAllLines(Path.Combine(root, "CaptureNotes.txt"), notes); Application.Quit(failed ? 1 : 0);
        }
    }
}
