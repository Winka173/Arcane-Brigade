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
            if (Array.IndexOf(args, "-arcane-k18-experience") >= 0) return;
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
            foreach (ShopRarityGlow glow in FindObjectsByType<ShopRarityGlow>())
            {
                if (glow.Target == null) continue;
                Check(Vector3.Distance(glow.transform.position, glow.Target.bounds.center) < 0.001f, "Rarity follows merchandise: " + name);
                foreach (SpriteRenderer light in glow.GetComponentsInChildren<SpriteRenderer>())
                {
                    Check(light.sortingOrder < glow.Target.sortingOrder, "Aura stays behind merchandise: " + name);
                    if (!glow.Target.enabled || glow.Target.color.a == 0) Check(light.color.a == 0, "Hidden merchandise has no orphan aura: " + name);
                }
            }
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
            yield return new WaitForSecondsRealtime(0.3f); yield return Capture("carpet-left-to-right");
            yield return new WaitForSecondsRealtime(0.3f); yield return Capture("shop-arrival-curl");
            yield return new WaitForSecondsRealtime(1.3f); yield return Capture("shop-ready");
            Check(FindObjectsByType<ShopRarityGlow>().Length >= 3, "Rare, gold and rare-companion auras exist in actual shop");
            Check(FindObjectsByType<ShopDisplayActor>().Count(actor => actor.GetComponent<SpriteRenderer>()?.sprite?.texture.name == "MarketStands_v1") == 3, "Three authored animated pedestals render in actual shop");
            Check(owner.Shop.Stations.Count(station => station.Offer != null) == 3 && owner.Shop.Stations.All(station => station.Recipe == null), "Normal shop has three stock choices and no unusable recipe clutter");
            foreach (WorldShop.Station station in owner.Shop.Stations.Where(station => station.Offer != null))
            {
                Check(station.Actor.GetComponent<SpriteRenderer>().bounds.max.y < owner.Shop.MerchantCenter.y - owner.Shop.MerchantHeight * 0.5f, "Stock body stays below merchant " + station.Offer.Token);
                foreach (SpriteRenderer stand in station.Marker.GetComponentsInChildren<SpriteRenderer>().Where(renderer => renderer.sprite != null && renderer.sprite.texture.name == "MarketStands_v1"))
                {
                    Check(station.Actor.View.sortingOrder > stand.sortingOrder, "Pedestal cannot paint over merchandise " + station.Offer.Token);
                    Rect ink = SpriteInkMetrics.BoundsOf(station.Actor.View.sprite);
                    Vector3 foot = station.Actor.transform.TransformPoint(new Vector3(ink.center.x, ink.yMin, 0));
                    ShopDisplayActor support = stand.GetComponent<ShopDisplayActor>();
                    Check(Mathf.Abs(foot.x - support.SeatPosition.x) < 0.07f && Mathf.Abs(foot.y - support.SeatPosition.y) < 0.2f, "Painted merchandise foot aligns to painted cushion " + station.Offer.Token);
                }
            }
            Transform rug = GameObject.Find("PaintedMarketRug").transform;
            Check(Mathf.Abs(rug.localScale.x - rug.localScale.y) < 0.001f, "Carpet preserves illustrated aspect ratio without stretching");
            SpriteRenderer beacon = FindObjectsByType<SpriteRenderer>().First(renderer => renderer.name == "ChampionBeacon");
            Check(beacon.transform.localPosition.y < -K8Motion.BodyHeight(owner.Simulation.Champion) * 0.5f && beacon.transform.localScale.x > beacon.transform.localScale.y, "Player identification rests below feet, not above head");
            ShopRarityGlow livingGlow = FindObjectsByType<ShopRarityGlow>().First();
            Transform mote = livingGlow.transform.Find("RarityMote0"); Vector3 previousMote = mote.localPosition;
            yield return new WaitForSecondsRealtime(0.2f);
            Check(PresentationSettings.ReducedMotion || Vector3.Distance(previousMote, mote.localPosition) > 0.001f, "Rarity motes animate during shop rest");
            WorldShop.Station middle = owner.Shop.Stations.First(station => station.Offer != null && station.Offer.Kind == OfferKind.Item && catalog.Item(station.Offer.Id).Rarity == ItemRarity.Mythic);
            owner.SetPresentationMoveTarget(middle.Position); yield return new WaitForSecondsRealtime(1);
            Check(owner.Shop.Nearby == middle, "Physical proximity focuses middle stock");
            yield return Capture("shop-focus-gold");
            Check(owner.Shop.DescribeOffer(middle.Offer).Contains("Owned:"), "Focused shop defaults to actual purchase delta");
            owner.SetShopDetails(true); yield return Capture("shop-expanded-details");
            Check(owner.ShopDetailsOpen, "Optional details remain accessible without opening another shop screen");
            owner.SetShopDetails(false);
            int count = owner.Session.State.Items.Sum(item => item.Count);
            Check(owner.Shop.Interact(), "Item purchase commits through normal interaction");
            yield return new WaitForSecondsRealtime(0.3f); yield return Capture("purchased-item-flight");
            yield return new WaitForSecondsRealtime(1.7f);
            Check(owner.Session.State.Items.Sum(item => item.Count) == count + 1, "Purchased item appears in inventory");
            WorldShop.Station ally = owner.Shop.Stations.First(station => station.Offer != null && station.Offer.Kind == OfferKind.Companion);
            owner.SetPresentationMoveTarget(ally.Position); yield return new WaitForSecondsRealtime(1.4f);
            Check(owner.Shop.Interact(), "Rare ally purchase commits");
            yield return new WaitForSecondsRealtime(0.3f); yield return Capture("recruited-ally-arrival");
            owner.SetPresentationMoveTarget(Vector2.zero); yield return new WaitForSecondsRealtime(0.9f); yield return Capture("recruited-ally-walking");
            Check(FindObjectsByType<ShopRarityGlow>().Any(glow => glow.name == "RecruitedAllyRarity"), "Purchased rare ally carries aura while walking");
            foreach (UnitVisual visual in FindObjectsByType<UnitVisual>().Where(candidate => candidate.State != null && candidate.State.Faction == Faction.Companion))
            {
                SpriteRenderer weapon = visual.WeaponRenderer;
                if (weapon.sprite == null || visual.State.Definition.Weapon.Kind == WeaponKind.Contact) continue;
                Rect ink = SpriteInkMetrics.BoundsOf(weapon.sprite);
                Vector3 scale = weapon.transform.localScale;
                float longest = Mathf.Max(ink.width * scale.x, ink.height * scale.y);
                bool melee = visual.State.Definition.Weapon.Kind == WeaponKind.Melee;
                Check(longest >= K8Motion.BodyHeight(visual.State) * (melee ? 1.05f : 0.65f) - 0.001f, "Actual ally weapon avoids undersized painted silhouette");
            }
            WorldShop.Station reroll = owner.Shop.Stations.First(station => station.Action == "REROLL");
            owner.SetPresentationMoveTarget(reroll.Position); yield return new WaitForSecondsRealtime(2.5f);
            Check(owner.Shop.Interact(), "Physical reroll commits");
            yield return new WaitForSecondsRealtime(0.08f); yield return Capture("rarity-reroll-retirement");
            if (!PresentationSettings.ReducedMotion)
                Check(FindObjectsByType<ShopRarityGlow>().Any(glow => glow.name == "RetiringStockRarity"), "Retiring rare stock retains its moving aura");
            yield return new WaitForSecondsRealtime(2);
            Check(!FindObjectsByType<ShopRarityGlow>().Any(glow => glow.name == "RetiringStockRarity"), "Retired stock aura disappears with stock");
            WorldShop.Station doorway = owner.Shop.Stations.First(station => station.Action == "NEXT HALL");
            owner.SetPresentationMoveTarget(doorway.Position); yield return new WaitForSecondsRealtime(2.5f);
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
