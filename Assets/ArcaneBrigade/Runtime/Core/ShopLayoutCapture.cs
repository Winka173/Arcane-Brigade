using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    /// <summary>Native evidence that nothing in the shop sits on anything else. Runs inside the Act I capture
    /// (-arcane-actone-capture -actone-layout -capturePath DIR): gold shops at waves 1, 4, 8, 12, 16 and 19 and a dream
    /// shop, each measured from the real sprite ink bounds of every prop, station, the merchant and the warm chair (with
    /// all 19 of its positions), written to layout.txt with screen boxes in boxes-*.csv, plus the journal's sealed note
    /// and the settings page.</summary>
    public static class ShopLayoutCapture
    {
        public static bool Requested => Array.IndexOf(Environment.GetCommandLineArgs(), "-actone-layout") >= 0;
        private sealed class Item { public string Group; public Rect World; public bool Reserved; }
        private static readonly string[] Skip = { "Shadow", "Contact", "Glow", "Halo", "Bloom", "Fog", "Mist", "Floor", "Rug", "Pennant", "Suspension", "Aura", "Cast", "Smudge", "Thread", "Sparkle", "Light", "Explosion", "Painted", "Dust", "Ember", "Spark", "Ring", "Reflection", "Tear" };

        public static IEnumerator Run(MonoBehaviour host, string root, Func<string, IEnumerator> shot)
        {
            var catalog = Resources.Load<RunCatalog>("RunCatalog"); var storage = new RunStorage(RunPaths.Root);
            var log = new List<string>();
            int failures = 0;
            var cases = new (int wave, bool dream, bool card)[] { (1, false, false), (4, false, false), (8, false, false), (12, false, false), (16, false, false), (19, false, false), (12, true, false), (4, true, false), (8, false, true) };
            foreach (var c in cases)
            {
                using (var session = new RunSession(catalog))
                {
                    session.Start("LanternKnight", 171300 + c.wave); session.State.Wave = c.wave; session.State.Phase = SessionPhase.Shop; session.State.Gold = 900; session.State.Eggs = 20;
                    session.State.MienCho.ReadMask = (1 << MienChoStory.Available(session.State)) - 1;
                    if (c.wave == 16) { session.State.ActOne ??= new ActOneProgress(); session.State.ActOne.HobRescued = 1; }
                    // shop 8: the room itself (offer already refused), then once more with the Last Customer's card up
                    if (c.wave == 8 && !c.card) { session.State.ActOne ??= new ActOneProgress(); session.State.ActOne.Offer = ActOneProgress.Refused; }
                    typeof(RunSession).GetMethod("RollShop", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(session, new object[] { c.dream });
                    if (c.dream && session.State.SpecialOffers.Count == 0)
                        for (int i = 0; i < 3; i++) session.State.SpecialOffers.Add(new ShopOffer { Token = ++session.State.NextOffer, Kind = OfferKind.Item, Id = catalog.Items[i].Id, Price = 2, Currency = ShopCurrency.Eggs });
                    if (!c.dream) session.State.SpecialOffers.Clear();
                    session.State.SpecialShopVisited = !c.dream;
                    storage.SaveRun(session.State);
                }
                RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(1.2f);
                var owner = UnityEngine.Object.FindAnyObjectByType<ArenaController>();
                // stand the hero in open floor so no stall tooltip covers the room
                owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = new Vector2(-1.35f, -0.95f);
                yield return new WaitForSecondsRealtime(2.4f);
                string label = (c.dream ? "dream-" : "shop-") + c.wave + (c.card ? "-offer-card" : "");
                var items = Collect(owner);
                int bad = Report(owner, items, label, log, root);
                failures += bad;
                yield return shot("layout-" + label);
            }
            // the sealed note for unread pages, in a shop and in a fight
            using (var session = new RunSession(catalog))
            {
                session.Start("LanternKnight", 171390); session.State.Wave = 8; session.State.Phase = SessionPhase.Shop;
                session.State.MienCho.ReadMask = (1 << 5) - 1; // pages 6.. waiting
                typeof(RunSession).GetMethod("RollShop", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(session, new object[] { false });
                session.State.SpecialOffers.Clear(); session.State.SpecialShopVisited = true;
                storage.SaveRun(session.State);
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(3f);
            var arena = UnityEngine.Object.FindAnyObjectByType<ArenaController>();
            var journal = arena.GetComponent<MienChoPresentation>();
            log.Add("notice: unread=" + journal.Unread + " visible=" + journal.NoticeVisible + " open=" + journal.Open + " paused=" + arena.PresentationPaused);
            if (journal.Open || journal.Unread == 0 || !journal.NoticeVisible) { failures++; log.Add("FAIL journal must not open by itself; the note must show"); }
            yield return shot("notice-shop");
            journal.OpenNext(); yield return new WaitForSecondsRealtime(.4f);
            log.Add("after J: open=" + journal.Open + " chapter=" + journal.Chapter + " paused=" + arena.PresentationPaused);
            if (!journal.Open || journal.Chapter != 5) { failures++; log.Add("FAIL J opens the oldest unread page"); }
            yield return shot("notice-opened-journal");
            journal.Close(); yield return new WaitForSecondsRealtime(.3f);
            log.Add("after close: unread=" + journal.Unread + " open=" + journal.Open);
            if (journal.Open || journal.Unread == 0) { failures++; log.Add("FAIL closing marks only what was read; the rest still waits"); }
            arena.NextWave();
            float until = Time.realtimeSinceStartup + 8;
            while ((arena.TransitionPending || arena.Session.State.Phase != SessionPhase.Combat) && Time.realtimeSinceStartup < until) yield return null;
            yield return new WaitForSecondsRealtime(1.5f);
            log.Add("combat: phase=" + arena.Session.State.Phase + " wave=" + arena.Session.State.Wave + " open=" + journal.Open + " unread=" + journal.Unread + " notice=" + journal.NoticeVisible);
            if (journal.Open) { failures++; log.Add("FAIL a new wave never opens the journal by itself"); }
            yield return shot("notice-combat");
            // the settings page with the new story-pages switch, and the help page that explains the sealed note
            SceneManager.LoadScene("RunFrontend"); yield return new WaitForSecondsRealtime(3f);
            var menu = UnityEngine.Object.FindAnyObjectByType<RunMenu>();
            if (menu != null)
            {
                menu.ShowPage(RunMenu.Page.Settings); yield return new WaitForSecondsRealtime(.6f); yield return shot("settings-story-pages");
                menu.ShowPage(RunMenu.Page.Help); yield return new WaitForSecondsRealtime(.6f); yield return shot("help-journal");
            }
            log.Add(failures == 0 ? "LAYOUT PASS" : "LAYOUT FAIL " + failures);
            File.WriteAllLines(Path.Combine(root, "layout.txt"), log);
        }

        private static List<Item> Collect(ArenaController owner)
        {
            var groups = new Dictionary<string, Rect>();
            foreach (SpriteRenderer view in owner.GetComponentsInChildren<SpriteRenderer>())
            {
                if (!view.enabled || view.sprite == null || view.color.a < 0.2f || !view.gameObject.activeInHierarchy) continue;
                if (view.GetComponentInParent<UnitVisual>() != null) continue;
                if (Skip.Any(word => view.gameObject.name.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0 && !view.gameObject.name.StartsWith("PaintedNightWatcher") && !view.gameObject.name.StartsWith("PaintedMarket"))) continue;
                if (view.sortingOrder <= -60 || view.sortingOrder >= 2000) continue;
                Transform t = view.transform;
                while (t.parent != null && t.parent != owner.transform && t.parent.name != "NightMarketStations") t = t.parent;
                if (t.parent == null) continue;
                string key = (t.parent.name == "NightMarketStations" ? "station:" : "") + t.name;
                if (key.StartsWith("DeathRemnant") || key.StartsWith("Projectile") || key.StartsWith("Pooled") || key.Contains("Suspension")) continue;
                Rect ink = SpriteInkMetrics.BoundsOf(view.sprite);
                if (view.flipX) ink.x = -ink.xMax;
                Vector3 a = view.transform.TransformPoint(new Vector3(ink.xMin, ink.yMin)), b = view.transform.TransformPoint(new Vector3(ink.xMax, ink.yMax));
                Vector3 d = view.transform.TransformPoint(new Vector3(ink.xMin, ink.yMax)), e = view.transform.TransformPoint(new Vector3(ink.xMax, ink.yMin));
                Rect world = Rect.MinMaxRect(Mathf.Min(a.x, b.x, d.x, e.x), Mathf.Min(a.y, b.y, d.y, e.y), Mathf.Max(a.x, b.x, d.x, e.x), Mathf.Max(a.y, b.y, d.y, e.y));
                if (world.width < 0.05f || world.height < 0.05f) continue;
                groups[key] = groups.TryGetValue(key, out Rect was) ? Rect.MinMaxRect(Mathf.Min(was.xMin, world.xMin), Mathf.Min(was.yMin, world.yMin), Mathf.Max(was.xMax, world.xMax), Mathf.Max(was.yMax, world.yMax)) : world;
            }
            var items = groups.Select(pair => new Item { Group = pair.Key, World = pair.Value }).ToList();
            // the screen furniture: HUD wallet column, bonds card, the CONTINUE plate, the journal note
            Camera camera = Camera.main;
            foreach (var (name, gui) in new[] { ("hud:wallet", new Rect(0, 0, 380, 300)), ("hud:bonds", new Rect(1585, 10, 320, 140)), ("hud:continue", new Rect(840, 930, 240, 110)), ("hud:wave", new Rect(380, 20, 290, 60)) })
            {
                Vector3 p = camera.ScreenToWorldPoint(new Vector3(camera.pixelRect.x + gui.xMin / 1920f * camera.pixelWidth, camera.pixelRect.y + (1 - gui.yMax / 1080f) * camera.pixelHeight, 0));
                Vector3 q = camera.ScreenToWorldPoint(new Vector3(camera.pixelRect.x + gui.xMax / 1920f * camera.pixelWidth, camera.pixelRect.y + (1 - gui.yMin / 1080f) * camera.pixelHeight, 0));
                items.Add(new Item { Group = name, World = Rect.MinMaxRect(p.x, p.y, q.x, q.y), Reserved = true });
            }
            // every position the warm chair will ever take this run, and the walk between them
            var chair = owner.GetComponent<WaitingRealmStage>();
            Item seat = items.Find(item => item.Group == "TheUnoccupiedChair");
            if (seat != null && chair != null && chair.ChairVisible)
                for (int wave = 1; wave <= 19; wave++)
                {
                    if (wave == owner.Session.State.Wave) continue;
                    Vector2 shift = ActOneCanon.ChairPosition(wave) - chair.ChairAnchor;
                    items.Add(new Item { Group = "chair-path:" + wave, World = new Rect(seat.World.position + shift, seat.World.size), Reserved = true });
                }
            return items;
        }

        private static float Overlap(Rect a, Rect b)
        {
            float w = Mathf.Min(a.xMax, b.xMax) - Mathf.Max(a.xMin, b.xMin), h = Mathf.Min(a.yMax, b.yMax) - Mathf.Max(a.yMin, b.yMin);
            return w > 0 && h > 0 ? w * h : 0;
        }

        private static int Report(ArenaController owner, List<Item> items, string label, List<string> log, string root)
        {
            int bad = 0;
            log.Add("== " + label + " (" + items.Count(i => !i.Reserved) + " props)");
            foreach (Item item in items.Where(i => !i.Reserved).OrderBy(i => i.Group)) log.Add("  " + item.Group + " " + Fmt(item.World));
            for (int i = 0; i < items.Count; i++)
                for (int j = i + 1; j < items.Count; j++)
                {
                    Item a = items[i], b = items[j];
                    if (a.Reserved && b.Reserved) continue;
                    if (a.Group.StartsWith("chair-path") && b.Group == "TheUnoccupiedChair" || b.Group.StartsWith("chair-path") && a.Group == "TheUnoccupiedChair") continue;
                    // the CONTINUE plate belongs to the door; the merchant presents its stock on its own rug
                    if (Pair(a, b, "station:NEXT HALL", "hud:continue") || Pair(a, b, "APlaceReservedForYou", "DreamLandmark5")) continue; // the little silhouette sits on the reserved place
                    // the Last Customer only stands in the shop on its own visits; it may use where the chair stands on other visits
                    if (a.Group.StartsWith("ActOne-LastCustomer") && b.Group.StartsWith("chair-path") || b.Group.StartsWith("ActOne-LastCustomer") && a.Group.StartsWith("chair-path")) continue;
                    bool designed = a.Group == "station:AnimatedShopActor" && b.Group.StartsWith("station:") || b.Group == "station:AnimatedShopActor" && a.Group.StartsWith("station:");
                    float area = Overlap(a.World, b.World);
                    float smaller = Mathf.Min(a.World.width * a.World.height, b.World.width * b.World.height);
                    // touching edges and grazing corners are fine; a real overlap covers part of a prop
                    if (area < 0.04f || area < smaller * 0.04f) continue;
                    if (designed) { log.Add("  (stock on the merchant's rug) " + a.Group + " x " + b.Group + " area=" + area.ToString("0.00")); continue; }
                    bad++;
                    log.Add("  OVERLAP " + a.Group + " x " + b.Group + " area=" + area.ToString("0.00") + " (" + (100 * area / smaller).ToString("0") + "% of smaller)");
                }
            Camera camera = Camera.main;
            var csv = new List<string> { "group,reserved,x0,y0,x1,y1" };
            foreach (Item item in items)
            {
                Vector3 p = camera.WorldToScreenPoint(item.World.min), q = camera.WorldToScreenPoint(item.World.max);
                float sx = 1920f / camera.pixelWidth, sy = 1080f / camera.pixelHeight;
                csv.Add(item.Group + "," + (item.Reserved ? 1 : 0) + "," + ((p.x - camera.pixelRect.x) * sx).ToString("0") + "," + (1080 - (q.y - camera.pixelRect.y) * sy).ToString("0") + "," + ((q.x - camera.pixelRect.x) * sx).ToString("0") + "," + (1080 - (p.y - camera.pixelRect.y) * sy).ToString("0"));
            }
            File.WriteAllLines(Path.Combine(root, "boxes-" + label + ".csv"), csv);
            if (ShopLayout.Active != null) foreach (string line in ShopLayout.Active.Describe()) log.Add("    " + line);
            if (ShopLayout.Active != null) log.Add("  layout: plans=" + ShopLayout.Active.Plans + " left out of this shop=" + ShopLayout.Active.Hidden + " reserved=" + ShopLayout.Active.Reserved.Count);
            log.Add("  overlaps: " + bad);
            return bad;
        }
        private static bool Pair(Item a, Item b, string x, string y) => a.Group == x && b.Group == y || a.Group == y && b.Group == x;
        private static string Fmt(Rect r) => "[" + r.xMin.ToString("0.00") + "," + r.yMin.ToString("0.00") + " .. " + r.xMax.ToString("0.00") + "," + r.yMax.ToString("0.00") + "]";
    }
}
