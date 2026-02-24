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
    public sealed class ShopDescriptionCapture : MonoBehaviour
    {
        public static bool Requested => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-description-capture") >= 0;
        private readonly List<string> checks = new List<string>();
        private string root; private bool failed, gallery; private RunCatalog catalog; private ArenaController owner;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (!Requested || FindAnyObjectByType<ShopDescriptionCapture>() != null) return;
            var go = new GameObject("ShopDescriptionCapture"); DontDestroyOnLoad(go); go.AddComponent<ShopDescriptionCapture>();
        }
        private void Check(bool pass, string label) { failed |= !pass; checks.Add((pass ? "PASS " : "FAIL ") + label); File.WriteAllLines(Path.Combine(root, "Checks.txt"), checks); }
        private IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            var image = ScreenCapture.CaptureScreenshotAsTexture();
            int visible = 0; for (int y = 0; y < image.height; y += 30) for (int x = 0; x < image.width; x += 30) if (image.GetPixel(x, y).maxColorComponent > .16f) visible++;
            Check(image.width == Screen.width && image.height == Screen.height && visible > 40, "Rendered Windows frame: " + name + " / " + image.width + "x" + image.height);
            File.WriteAllBytes(Path.Combine(root, name + ".png"), image.EncodeToPNG()); Destroy(image);
        }
        private IEnumerator Load(bool special)
        {
            using (var run = new RunSession(catalog))
            {
                run.Start("LanternKnight", 171017); run.State.Wave = 12; run.State.Phase = SessionPhase.Shop; run.State.Gold = 4000; run.State.Eggs = 50;
                run.State.MienCho.ReadMask = (1 << MienChoStory.Available(run.State)) - 1;
                run.State.ActOne ??= new ActOneProgress(); run.State.ActOne.Offer = ActOneProgress.Refused;
                run.State.Offers.Clear(); run.State.SpecialOffers.Clear(); run.State.SpecialShopVisited = !special;
                var stock = special ? run.State.SpecialOffers : run.State.Offers;
                var bunny = catalog.Companions.First(u => u.ReferenceCompanion == "Cheer Bunny");
                var downside = catalog.Companions.First(u => u.CompanionModifiers.Any(m => m.Value < 0));
                var item = catalog.Items.FirstOrDefault(i => i.ReferenceName == "Gold-Plated Ring") ?? catalog.Items.First(i => i.Mechanic == RelicMechanic.TreasureLease);
                stock.Add(new ShopOffer { Token = ++run.State.NextOffer, Kind = OfferKind.Companion, Id = special ? downside.Id : bunny.Id, Tier = special ? 3 : 1, Price = special ? 2 : 12, Currency = special ? ShopCurrency.Eggs : ShopCurrency.Gold });
                stock.Add(new ShopOffer { Token = ++run.State.NextOffer, Kind = OfferKind.Item, Id = item.Id, Price = 12 });
                var tradeoff = catalog.Items.First(i => i.EffectiveModifiers.Any(m => m.Value < 0) && i.EffectiveModifiers.Any(m => m.Value > 0));
                stock.Add(new ShopOffer { Token = ++run.State.NextOffer, Kind = OfferKind.Item, Id = tradeoff.Id, Price = special ? 2 : 10, Currency = special ? ShopCurrency.Health : ShopCurrency.Gold });
                new RunStorage(RunPaths.Root).SaveRun(run.State);
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(3);
            owner = FindAnyObjectByType<ArenaController>(); Check(owner != null && owner.Simulation.Phase == RunPhase.Rest && !owner.StoryConsumesInput, "Actual shop loads without modal story: " + special);
        }
        private IEnumerator Focus(int slot)
        {
            var station = owner.Shop.Stations.Where(s => s.Offer != null).ElementAt(slot);
            owner.GetComponent<ShopInspect>().Close();
            owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = station.Position + new Vector2(0, -.3f);
            FindAnyObjectByType<ArenaCamera>().Snap(); yield return new WaitForSecondsRealtime(.4f);
            Check(owner.Shop.Nearby?.Offer == station.Offer, "Focus selects the actual offer: " + station.Offer.Id);
        }
        private IEnumerator Start()
        {
            var args = Environment.GetCommandLineArgs(); int at = Array.IndexOf(args, "-capturePath");
            if (at < 0 || at + 1 >= args.Length) { Application.Quit(1); yield break; }
            root = Path.GetFullPath(args[at + 1]); Directory.CreateDirectory(root); Application.runInBackground = true;
            Application.logMessageReceived += Log;
            Check(RunPaths.Root.Contains("ArcaneBrigadeDiagnostics"), "Player profile and save are isolated");
            catalog = Resources.Load<RunCatalog>("RunCatalog"); bool reduced = args.Contains("-description-reduced");
            new RunStorage(RunPaths.Root).SaveProfile(new PlayerProfile { TutorialSeen = true, BestWave = 40, ReducedMotion = reduced, ReducedEffects = reduced, ReducedFlashes = reduced });
            yield return Load(false);
            for (int i = 0; i < 3; i++)
            {
                yield return Focus(i); yield return Shot("Regular-" + i + "-summary");
                owner.SetShopDetails(true); yield return Shot("Regular-" + i + "-details");
                var hud = (RunHud)typeof(ArenaController).GetField("runHud", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);
                typeof(RunHud).GetField("shopScroll", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(hud, new Vector2(0, 10000));
                yield return Shot("Regular-" + i + "-details-bottom"); owner.SetShopDetails(false);
            }
            var inspect = owner.GetComponent<ShopInspect>();
            owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = new Vector2(-1.35f, -.95f); yield return new WaitForSecondsRealtime(.3f);
            var offerEntry = inspect.Entries.First(e => e.Offer?.Kind == OfferKind.Companion);
            Check(inspect.Show(offerEntry.Key), "Click inspection opens the same complete companion card"); yield return Shot("Clicked-companion"); inspect.Close();
            yield return Load(true); yield return Focus(0); yield return Shot("Dream-three-star-companion");
            yield return Focus(2); yield return Shot("Dream-HP-cost-tradeoff");
            gallery = true; yield return Shot("All-stat-icons"); gallery = false;
            Check(checks.All(s => s.StartsWith("PASS ")), "All runtime observations pass"); Application.Quit(failed ? 1 : 0);
        }
        private void Log(string message, string stack, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
            Check(false, message + "\n" + stack); Application.Quit(1);
        }
        private void OnGUI()
        {
            if (!gallery) return;
            GUI.depth = -100; var previous = GUI.matrix; GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 1920f, Screen.height / 1080f, 1));
            ArcadeSkin.Panel(new Rect(200, 140, 1520, 800), ArcadeSkin.Ink); var style = ArcadeTypography.Style(18, true);
            int i = 0; foreach (ItemEffect effect in Enum.GetValues(typeof(ItemEffect))) { float x = 220 + i % 7 * 211, y = 165 + i / 7 * 104; StatGlyph.Draw(new Rect(x + 67, y, 44, 44), effect); GUI.Label(new Rect(x, y + 50, 205, 40), effect.ToString(), style); i++; }
            GUI.matrix = previous;
        }
    }
}
