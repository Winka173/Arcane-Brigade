using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public sealed class K13PlayerCapture : MonoBehaviour
    {
        private readonly List<string> notes = new List<string>();
        private string root;
        private bool failed;
        private ArenaController owner;
        private readonly string[] arms = { "DreamOrbStaff", "FireExtinguisher", "PlasmaTrident", "MoonFeatherFan", "TwinSabres", "ClockCrescent", "ParasolNeedle", "StampHammer" };
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (!RunPaths.K7Capture || Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-k13") < 0 || FindAnyObjectByType<K13PlayerCapture>() != null) return;
            var child = new GameObject("K13PlayerCapture"); DontDestroyOnLoad(child); child.AddComponent<K13PlayerCapture>();
        }
        private void Check(bool condition, string label) { notes.Add((condition ? "PASS " : "FAIL ") + label); failed |= !condition; }
        private void Log(string message, string trace, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) { failed = true; notes.Add("FAIL Unity " + message); } }
        private void OnDestroy() { Application.logMessageReceived -= Log; }
        private IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(root, name + ".png"), texture.EncodeToPNG()); Destroy(texture);
        }
        private IEnumerator Load(RunCatalog catalog, RunStorage storage, bool shop, int weapon = -1)
        {
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 13077); session.State.Wave = shop ? 5 : 2;
                session.State.Gold = 1000; session.State.Eggs = 30; session.State.Roster.Clear();
                if (shop)
                {
                    session.State.Roster.Add(new RosterEntry { UnitId = "TeapotPilgrim", WeaponId = "SpoonSabre" });
                    session.State.Roster.Add(new RosterEntry { UnitId = "TeapotPilgrim", WeaponId = "InkPistol" });
                    session.State.Phase = SessionPhase.Shop; session.State.SpecialShopVisited = true;
                    session.State.Offers.Add(new ShopOffer { Token = ++session.State.NextOffer, Id = "TeapotPilgrim", Kind = OfferKind.Companion, Price = 1 });
                    session.State.Offers.Add(new ShopOffer { Token = ++session.State.NextOffer, Id = catalog.Items.First(item => item.Rarity == ItemRarity.Mythic).Id, Kind = OfferKind.Item, Price = 1 });
                    session.State.Offers.Add(new ShopOffer { Token = ++session.State.NextOffer, Id = catalog.Items.First(item => item.Rarity == ItemRarity.Rare).Id, Kind = OfferKind.Item, Price = 999999 });
                }
                else
                {
                    string id = weapon >= 0 ? arms[weapon] : "TwinSabres";
                    session.State.Roster.Add(new RosterEntry { UnitId = catalog.Weapon(id).SpecialOnly ? "EclipseEgg" : "TeapotPilgrim", WeaponId = id, Tier = 2 });
                }
                session.Validate(session.State); storage.SaveRun(session.State); storage.SaveProfile(new PlayerProfile());
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return null;
            owner = FindAnyObjectByType<ArenaController>();
        }
        private IEnumerator Start()
        {
            Application.logMessageReceived += Log; Application.runInBackground = true;
            string[] args = Environment.GetCommandLineArgs(); int path = Array.IndexOf(args, "-capturePath");
            if (path < 0 || path + 1 >= args.Length) { Application.Quit(1); yield break; }
            root = Path.GetFullPath(args[path + 1]); Directory.CreateDirectory(root);
            yield return K7PlayerCapture.ResizeDiagnostic(3840, 2160, Check);
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog"); var storage = new RunStorage(RunPaths.Root);
            RunMenu menu = FindAnyObjectByType<RunMenu>();
            if (menu != null)
            {
                menu.ShowPage(RunMenu.Page.Landing); yield return new WaitForSecondsRealtime(0.35f); yield return Shot("00-main-menu");
                foreach (RunMenu.Page page in new[] { RunMenu.Page.Roster, RunMenu.Page.Settings, RunMenu.Page.Help, RunMenu.Page.Museum })
                {
                    menu.ShowPage(page); yield return new WaitForSecondsRealtime(0.35f); yield return Shot("00-menu-" + page);
                    if (page == RunMenu.Page.Roster)
                        for (int index = 1; index < catalog.Champions.Length; index++)
                        {
                            menu.SelectChampionForDiagnostics(index); yield return new WaitForSecondsRealtime(0.35f); yield return Shot("00-roster-" + catalog.Champions[index].Id);
                        }
                }
                Check(true, "Main menu, roster, settings, help and museum rendered");
            }
            yield return Load(catalog, storage, true); yield return new WaitForSecondsRealtime(1.7f);
            owner.SetPresentationMoveTarget(new Vector2(-3, 0.5f)); yield return new WaitForSecondsRealtime(2.8f);
            Check(owner.Shop.Nearby != null && owner.Shop.Nearby.Offer?.Kind == OfferKind.Companion, "Merge stock focus");
            Check(owner.Shop.MergePreview.ApproachingCount == 2, "Two actual allies approach merge stock");
            yield return Shot("01-merge-preview");
            string before = JsonUtility.ToJson(owner.Session.State);
            yield return new WaitForSecondsRealtime(0.25f);
            Check(JsonUtility.ToJson(owner.Session.State) == before, "Approach preview leaves saved roster/wallet/random unchanged");
            Check(owner.Shop.Interact(), "Actual merge purchase commits");
            Check(owner.Session.State.Roster.Count == 1 && owner.Session.State.Roster[0].Tier == 2, "Purchase creates one two-star ally");
            yield return new WaitForSecondsRealtime(0.25f); yield return Shot("02-merge-collapse");
            Check(owner.Shop.Reveal.PendingCount > 0, "Committed two-star reveal active");
            yield return new WaitForSecondsRealtime(1.5f); yield return Shot("03-two-star-result");
            owner.SetPresentationMoveTarget(new Vector2(0, 0.5f)); yield return new WaitForSecondsRealtime(1.5f);
            Check(owner.Shop.Interact() && owner.Shop.ItemPickup.ActiveCount > 0, "Relic purchase has committed flight");
            yield return new WaitForSecondsRealtime(0.25f); yield return Shot("04-relic-flight");
            owner.SetPresentationMoveTarget(new Vector2(3, 0.5f)); yield return new WaitForSecondsRealtime(1.5f);
            Check(!owner.Shop.Interact() && owner.Shop.DeniedCount > 0, "Unaffordable relic rejects with reaction");
            yield return Shot("05-denied-purchase");
            owner.ToggleDetails(); owner.Details.SelectTab(1); yield return new WaitForSecondsRealtime(0.4f); yield return Shot("06-companion-ledger"); owner.ToggleDetails();
            foreach (Vector2 size in new[] { new Vector2(1280, 720), new Vector2(1600, 1200), new Vector2(2560, 1080) })
            {
                yield return K7PlayerCapture.ResizeDiagnostic((int)size.x, (int)size.y, Check);
                yield return Shot("07-shop-" + (int)size.x + "x" + (int)size.y);
                owner.ToggleDetails(); yield return new WaitForSecondsRealtime(0.35f); yield return Shot("07-ledger-" + (int)size.x + "x" + (int)size.y); owner.ToggleDetails();
            }
            yield return K7PlayerCapture.ResizeDiagnostic(3840, 2160, Check);
            for (int index = 0; index < arms.Length; index++)
            {
                yield return Load(catalog, storage, false, index);
                UnitState target = owner.Simulation.AddExpandedPresentationFixture(owner.Session.CombatCatalog.MeleeEnemy, new Vector2(1.8f, 0), 10000);
                target.Health = 10000; target.Definition.Speed = 0; target.NextAttack = target.NextContact = 1000;
                owner.Simulation.Champion.NextAttack = 1000;
                yield return new WaitForSecondsRealtime(0.9f); yield return Shot("08-weapon-" + arms[index]);
                Check(owner.Simulation.Units.Any(unit => unit.Faction == Faction.Companion && unit.Definition.Weapon.Id == arms[index]), "Native weapon body and delivery " + arms[index]);
            }
            yield return Load(catalog, storage, false); owner.TogglePause(); float time = owner.PresentationClock;
            yield return new WaitForSecondsRealtime(0.3f); Check(owner.PresentationClock == time, "Motion clock freezes on pause"); owner.TogglePause();
            notes.Add("Isolated saved shop/weapon fixtures. Weapon showroom uses a stationary durable enemy and delays the champion attack to reveal each companion delivery; the champion has no HP immunity. Not a natural full-run victory or human aesthetic approval. Audio/release excluded.");
            Check(!failed, "No runtime errors or failed assertions"); File.WriteAllLines(Path.Combine(root, "CaptureNotes.txt"), notes); Application.Quit(failed ? 1 : 0);
        }
    }
}
