using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public sealed class CompanionReactionCapture : MonoBehaviour
    {
        public static bool Requested => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-reaction-capture") >= 0;
        private string root; private bool failed, gallery;
        private RunCatalog catalog; private ArenaController owner;
        private readonly List<string> checks = new List<string>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (!Requested || FindAnyObjectByType<CompanionReactionCapture>() != null) return;
            var go = new GameObject("CompanionReactionCapture"); DontDestroyOnLoad(go); go.AddComponent<CompanionReactionCapture>();
        }
        private void Check(bool pass, string label) { failed |= !pass; checks.Add((pass ? "PASS " : "FAIL ") + label); File.WriteAllLines(Path.Combine(root, "Checks.txt"), checks); }
        private void Log(string text, string stack, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) { Check(false, text + "\n" + stack); Application.Quit(1); } }
        private IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame(); var image = ScreenCapture.CaptureScreenshotAsTexture();
            int ink = 0; for (int y = 0; y < image.height; y += 25) for (int x = 0; x < image.width; x += 25) if (image.GetPixel(x, y).maxColorComponent > .18f) ink++;
            Check(image.width == Screen.width && image.height == Screen.height && ink > 45, "Actual Windows frame " + name + " / " + image.width + "x" + image.height);
            File.WriteAllBytes(Path.Combine(root, name + ".png"), image.EncodeToPNG()); Destroy(image);
        }
        private IEnumerator Load(bool bond)
        {
            using (var run = new RunSession(catalog))
            {
                run.Start("LanternKnight", 171017); run.State.Wave = 6; run.State.Phase = SessionPhase.Shop; run.State.Gold = 5000; run.State.Roster.Clear(); run.State.Offers.Clear(); run.State.SpecialOffers.Clear(); run.State.SpecialShopVisited = true;
                run.State.MienCho.ReadMask = (1 << MienChoStory.Available(run.State)) - 1;
                run.State.ActOne ??= new ActOneProgress(); // the Customer has not made its wave-eight offer yet
                if (bond)
                {
                    run.State.Roster.Add(new RosterEntry { UnitId = "SpringDuck", WeaponId = "CrescentBow" });
                    run.State.Offers.Add(new ShopOffer { Token = ++run.State.NextOffer, Kind = OfferKind.Companion, Id = "SpringDuck", Price = 2 });
                }
                else
                {
                    run.State.Roster.Add(new RosterEntry { UnitId = "ButtonBeetle", WeaponId = "CrescentBow" });
                    run.State.Roster.Add(new RosterEntry { UnitId = "ButtonBeetle", WeaponId = "CorkscrewRapier" });
                    run.State.Roster.Add(new RosterEntry { UnitId = "CandleSnail", WeaponId = "CrescentBow" });
                    run.State.Roster.Add(new RosterEntry { UnitId = "CrayonGremlin", WeaponId = "CrescentBow", Tier = 2 });
                    run.State.Roster.Add(new RosterEntry { UnitId = "CrayonGremlin", WeaponId = "CorkscrewRapier", Tier = 2 });
                    var drink = catalog.Items.First(i => i.Category == RelicCategory.Drink);
                    run.State.Offers.Add(new ShopOffer { Token = ++run.State.NextOffer, Kind = OfferKind.Item, Id = drink.Id, Price = 2 });
                    run.State.Offers.Add(new ShopOffer { Token = ++run.State.NextOffer, Kind = OfferKind.Companion, Id = "ButtonBeetle", Price = 2 });
                    run.State.Offers.Add(new ShopOffer { Token = ++run.State.NextOffer, Kind = OfferKind.Companion, Id = "CrayonGremlin", Price = 2, Tier = 3 });
                }
                run.Validate(run.State);
                var storage = new RunStorage(RunPaths.Root); storage.SaveRun(run.State);
                Check(storage.LoadRun(run) != null, "Saved fixture validates before loading: " + storage.LastError);
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(3);
            owner = FindAnyObjectByType<ArenaController>();
            Check(owner != null && owner.Simulation.Phase == RunPhase.Rest && !owner.StoryConsumesInput, "Loaded actual saved shop fixture: " + bond);
            owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = new Vector2(-1.35f, -.95f);
        }
        private IEnumerator Purchase(int slot)
        {
            var station = owner.Shop.Stations.Where(s => s.Offer != null).ElementAt(slot);
            owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = station.Position + new Vector2(0, -.3f);
            FindAnyObjectByType<ArenaCamera>().Snap(); yield return new WaitForSecondsRealtime(.25f);
            Check(owner.Shop.Nearby?.Offer == station.Offer, "The real station is focused: " + station.Offer.Id);
            Check(owner.Shop.Interact(), "Purchase commits through the real shop: " + station.Offer.Id);
            owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = new Vector2(-1.35f, -.95f);
            yield return new WaitForSecondsRealtime(.1f);
        }
        private IEnumerator WaitFor(string speaker)
        {
            float end = Time.realtimeSinceStartup + 20;
            while (!SpeechBubbles.Showing(speaker) && Time.realtimeSinceStartup < end) yield return null;
            var bubble = SpeechBubbles.Find(speaker);
            Check(bubble != null && SpeechBubbles.Showing(speaker), "Expected actual companion speaks: " + speaker);
            if (bubble != null)
            {
                File.AppendAllText(Path.Combine(root, "Spoken.txt"), speaker + ": " + bubble.Text + "\n");
                Check(!SpeechBubbles.Showing("Shopkeeper"), "Reaction waits for the merchant to finish");
            }
            yield return new WaitForSecondsRealtime(1.6f);
        }
        private IEnumerator Start()
        {
            var args = Environment.GetCommandLineArgs(); int at = Array.IndexOf(args, "-capturePath");
            if (at < 0 || at + 1 >= args.Length) { Application.Quit(1); yield break; }
            root = Path.GetFullPath(args[at + 1]); Directory.CreateDirectory(root); Application.runInBackground = true; Application.logMessageReceived += Log;
            File.WriteAllText(Path.Combine(root, "Spoken.txt"), "");
            catalog = Resources.Load<RunCatalog>("RunCatalog"); bool reduced = args.Contains("-reaction-reduced");
            Check(RunPaths.Root.Contains("ArcaneBrigadeDiagnostics"), "Real profile and run save remain isolated");
            Check(PaintedStatIcons.Current?.Entries.Length == 49, "Player includes all 49 painted stat mappings");
            new RunStorage(RunPaths.Root).SaveProfile(new PlayerProfile { TutorialSeen = true, BestWave = 40, ReducedEffects = reduced, ReducedMotion = reduced, ReducedFlashes = reduced });
            // Let the boot scene finish its own Start before loading the diagnostic arena.
            yield return new WaitForSecondsRealtime(.75f);
            yield return Load(false); gallery = true; yield return Shot("Painted-icons-24-and-40px"); gallery = false;
            var cast = owner.GetComponent<SpeechBubbleCast>();
            yield return Purchase(0); yield return Shot("Merchant-before-companion");
            yield return WaitFor("Story:Hob");
            Check(SpeechBubbles.Find("Story:Hob")?.Text == CompanionReactions.Line("TeapotPilgrim", CompanionReactionKind.Purchase), "Hob uses its personal response as a separate canon traveller");
            yield return Shot("Hob-drink-response");
            yield return new WaitForSecondsRealtime(5);
            int stars = cast.StarUps, joins = cast.Joins;
            yield return Purchase(2); yield return WaitFor("Companion:CrayonGremlin");
            Check(cast.Joins == joins + 1 && cast.StarUps == stars && owner.Session.State.Roster.Count(r => r.UnitId == "CrayonGremlin" && r.Tier == 2) == 2, "Buying a three-star body with two lower-star bodies still present is a join, not a fabricated merge"); yield return Shot("Bought-three-star-joins");
            yield return new WaitForSecondsRealtime(5);
            yield return Purchase(1); yield return WaitFor("Companion:ButtonBeetle");
            Check(cast.StarUps == stars + 1 && SpeechBubbles.Find("Companion:ButtonBeetle")?.Text == CompanionReactions.Line("ButtonBeetle", CompanionReactionKind.Merge), "Real merge uses the beetle's personal line once"); yield return Shot("Beetle-merge-response");
            yield return Load(true); cast = owner.GetComponent<SpeechBubbleCast>();
            yield return Purchase(0); owner.TogglePause(); int contexts = cast.ContextLines;
            yield return new WaitForSecondsRealtime(3); Check(cast.ContextLines == contexts, "Pause holds pending contextual dialogue"); owner.TogglePause();
            yield return WaitFor("Companion:SpringDuck");
            // The join gets its own greeting before the bond response.
            float end = Time.realtimeSinceStartup + 20;
            while (cast.ContextLines == contexts && Time.realtimeSinceStartup < end) yield return null;
            Check(cast.ContextLines == contexts + 1 && cast.LastContext == CompanionReactionKind.Bond, "Only one contextual response follows a real bond threshold");
            yield return new WaitForSecondsRealtime(1.5f); yield return Shot("Duck-bond-response");
            int spoken = cast.ContextLines; var current = MergeReveal.Capture(owner.Session.State);
            cast.CommittedPurchase(current, owner.Session.State.Offers[0]);
            yield return new WaitForSecondsRealtime(4); Check(cast.ContextLines == spoken, "The same saved receipt never replays its response");
            Check(!owner.BuyOffer(-999) && cast.ContextLines == spoken, "An expired/invalid offer produces no success line");
            Check(!failed, "All native checks passed"); Application.Quit(failed ? 1 : 0);
        }
        private void OnGUI()
        {
            if (!gallery) return;
            GUI.depth = -100; var old = GUI.matrix; GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 1920f, Screen.height / 1080f, 1));
            ArcadeSkin.Panel(new Rect(200, 125, 1520, 820), ArcadeSkin.Ink); var style = ArcadeTypography.Style(17, true);
            int i = 0; foreach (ItemEffect effect in Enum.GetValues(typeof(ItemEffect)))
            {
                float x = 220 + i % 7 * 211, y = 145 + i / 7 * 110;
                StatGlyph.Draw(new Rect(x + 45, y + 8, 24, 24), effect); StatGlyph.Draw(new Rect(x + 100, y, 40, 40), effect);
                string label = ShortOfferCopy.StatName(effect); GUI.Label(new Rect(x, y + 52, 204, 48), label, style); i++;
            }
            GUI.matrix = old;
        }
    }
}
