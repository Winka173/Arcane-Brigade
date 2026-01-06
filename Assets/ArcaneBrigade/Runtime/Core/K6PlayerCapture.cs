using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public sealed class K6PlayerCapture : MonoBehaviour
    {
        private bool failed;
        private string root;
        private readonly List<string> notes = new List<string>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (!RunPaths.K6Capture || FindAnyObjectByType<K6PlayerCapture>() != null) return;
            var runner = new GameObject("K6PlayerCapture"); DontDestroyOnLoad(runner); runner.AddComponent<K6PlayerCapture>();
        }
        private void Awake() { Application.runInBackground = true; Application.logMessageReceived += OnLog; }
        private void OnDestroy() { Application.logMessageReceived -= OnLog; }
        private void OnLog(string message, string trace, LogType kind) { if (kind == LogType.Error || kind == LogType.Exception || kind == LogType.Assert) failed = true; }
        private void Check(bool condition, string name) { notes.Add((condition ? "PASS " : "FAIL ") + name); failed |= !condition; }
        private IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture(2);
            Check(texture != null && texture.width == 3840 && texture.height == 2160, "4K Unity frame " + name);
            if (texture != null) { File.WriteAllBytes(Path.Combine(root, name + ".png"), texture.EncodeToPNG()); Destroy(texture); }
            yield return new WaitForSecondsRealtime(0.25f);
        }
        private IEnumerator Start()
        {
            string[] arguments = Environment.GetCommandLineArgs(); int index = Array.IndexOf(arguments, "-capturePath");
            if (Application.isBatchMode || index < 0 || index + 1 >= arguments.Length) { Application.Quit(1); yield break; }
            root = Path.GetFullPath(arguments[index + 1]); Directory.CreateDirectory(root);
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(0.8f);
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            var storage = new RunStorage(RunPaths.Root);
            RunSnapshot snapshot;
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 610026, 1); session.State.Wave = 10; session.State.Roster.Clear();
                int body = 0;
                foreach (WeaponDefinition weapon in catalog.Weapons.Where(weapon => weapon.Family != ExpandedWeapon.None || weapon.Signature != SignatureWeapon.None))
                    session.State.Roster.Add(new RosterEntry { UnitId = catalog.Companions[body++ % catalog.Companions.Length].Id, WeaponId = weapon.Id, Tier = body % 3 == 0 ? 2 : 1 });
                foreach (ItemDefinition item in catalog.Items.Where(item => item.SignatureRole != SignatureRelicKind.None)) session.State.Items.Add(new ItemStack { Id = item.Id, Count = 1 });
                snapshot = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(session.State));
                storage.SaveRun(snapshot);
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(0.5f);
            ArenaController owner = FindAnyObjectByType<ArenaController>();
            yield return new WaitForSecondsRealtime(3f);
            Check(owner.Session.State.Difficulty == 1 && owner.Session.State.Roster.Count == 12, "Windows resumes saved mode and uncapped roster");
            yield return Shot("01-hud-live-score");
            owner.ToggleDetails(); owner.Details.SelectTab(1); yield return Shot("02-companion-files");
            owner.Details.SelectTab(2); yield return Shot("03-relic-drawers");
            owner.Details.SelectTab(3); yield return Shot("04-build-inspection");
            owner.ToggleDetails();
            snapshot = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(owner.Session.State));
            ContributionLedger.Merge(snapshot.Contributions, owner.Simulation.Contributions);
            snapshot.TotalKills += owner.Simulation.Kills; snapshot.CombatSeconds += owner.Simulation.Elapsed;
            snapshot.Wave = 20; snapshot.Phase = SessionPhase.Victory;
            snapshot.DefeatedBosses.AddRange(catalog.Bosses.Take(3).Select(boss => boss.Id));
            snapshot.BossKills = 3;
            owner.Session.Restore(snapshot);
            storage.RecordProgress(owner.Profile, owner.Session.State, catalog);
            Check(owner.Profile.History.Count == 1 && owner.Profile.Wins == 1, "Seeded terminal fixture credits one frozen history");
            string resultId = owner.Session.State.RunId;
            storage.SaveRun(owner.Session.State); RunLaunch.Resume = true;
            SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(0.5f);
            owner = FindAnyObjectByType<ArenaController>();
            Check(owner.Session.State.Phase == SessionPhase.Victory && owner.Session.State.RunId == resultId && owner.Profile.History.Count == 1 && owner.Profile.Wins == 1, "Windows terminal Victory resume reopens invoice without new run or duplicated credit");
            yield return Shot("05-victory-invoice");
            owner.ShowInvoiceEvidenceFixture(); yield return Shot("05-invoice-contribution-evidence");
            for (int bundle = 1; bundle <= 2; bundle++)
            {
                owner.Profile.Cosmetic = bundle; ArcadeSkin.Bundle = bundle;
                yield return Shot("06-bundle-" + bundle);
            }
            int rosterCount = owner.Session.State.Roster.Count;
            owner.ContinueEndless();
            Check(owner.Session.State.Endless && owner.Session.State.ParentRunId == snapshot.RunId && owner.Session.State.Roster.Count == rosterCount && owner.Session.State.Contributions.Count == 0 && RunMeta.Score(owner.Session.State) == 0, "Native Endless starts clean segment with build and parent intact");
            owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = Vector2.zero;
            Camera.main.GetComponent<ArenaCamera>().Snap();
            yield return new WaitForSecondsRealtime(0.45f);
            Check(owner.Shop.Stations.Count(station => station.Offer != null) == 3, "Physical rest has three real stand offers not modal shop");
            yield return Shot("07-physical-rest-endless");
            WorldShop.Station offerStand = owner.Shop.Stations.First(station => station.Offer != null);
            owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = offerStand.Position;
            Camera.main.GetComponent<ArenaCamera>().Snap(); yield return new WaitForSecondsRealtime(0.15f);
            yield return Shot("07-stand-price-preview");
            snapshot = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(owner.Session.State)); snapshot.Wave = 21; snapshot.Phase = SessionPhase.Defeat; snapshot.Health = 0;
            snapshot.LastDamageSource = "Door Bearer"; snapshot.LastDamageKind = "Physical / Contact"; snapshot.LastDamageAmount = 8;
            owner.Session.Restore(snapshot); storage.RecordProgress(owner.Profile, snapshot, catalog);
            resultId = snapshot.RunId; storage.SaveRun(snapshot); RunLaunch.Resume = true;
            SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(0.5f);
            owner = FindAnyObjectByType<ArenaController>();
            Check(owner.Session.State.Phase == SessionPhase.Defeat && owner.Session.State.RunId == resultId && owner.Profile.History.Count == 2 && owner.Profile.Wins == 1, "Windows terminal Defeat resume keeps final cause build and single credit");
            yield return Shot("08-defeat-invoice");
            owner.ShowInvoiceEvidenceFixture(); yield return Shot("08-death-source-evidence");
            PlayerProfile profile = owner.Profile; profile.Cosmetic = 0; ArcadeSkin.Bundle = 0;
            string profileBefore = JsonUtility.ToJson(profile); string saveBefore = File.ReadAllText(Path.Combine(RunPaths.Root, "run.json"));
            SceneManager.LoadScene("RunFrontend"); yield return new WaitForSecondsRealtime(0.4f);
            RunMenu menu = FindAnyObjectByType<RunMenu>(); menu.ConfigureMetaDiagnostics(catalog, profile, snapshot);
            menu.ShowPage(RunMenu.Page.Landing); yield return Shot("09-main-menu");
            menu.ShowPage(RunMenu.Page.Roster); yield return Shot("10-champion-mode-selection");
            menu.ShowPage(RunMenu.Page.Settings); yield return Shot("11-cosmetic-settings");
            menu.ShowPage(RunMenu.Page.Records); yield return Shot("12-history-receipts");
            menu.SelectHistoryForDiagnostics(1, false); yield return Shot("12-history-normal-record");
            menu.SelectHistoryForDiagnostics(1, true); yield return Shot("12-history-measured-contribution");
            menu.ShowPage(RunMenu.Page.Museum);
            for (int tab = 0; tab < 5; tab++) { menu.Museum.SelectTab(tab); yield return Shot("13-museum-tab-" + tab); }
            menu.Museum.SelectTab(0); yield return new WaitForSecondsRealtime(0.15f);
            menu.OpenWeaponSandbox(); yield return new WaitForSecondsRealtime(0.4f);
            Check(menu.Sandbox.IsOpen && menu.Sandbox.WeaponCount == profile.SeenWeapons.Distinct().Count(), "Museum discovery list opens gameplay sandbox");
            menu.Sandbox.SetTier(3); menu.Sandbox.SetMovingTarget(true); yield return new WaitForSecondsRealtime(0.9f);
            yield return Shot("14-museum-tier3-preview");
            menu.CloseWeaponSandbox();
            Check(menu.CurrentPage == RunMenu.Page.Museum && profileBefore == JsonUtility.ToJson(profile), "Museum history sandbox and cosmetic rendering do not mutate read-only profile");
            Check(saveBefore == File.ReadAllText(Path.Combine(RunPaths.Root, "run.json")), "Preview/reset/tier/close leaves durable saved run byte-identical");
            Check(profile.History.Count == 2 && profile.Records.Count == 2 && profile.Wins == 1, "Native normal and Endless grouped records stay separate");
            notes.Add("Seed610026, hall10 with seeded12-family tier roster/relics. HUD and Details use live gameplay pipeline, measured contribution copied from its actual combat. Victory/death screens are explicitly seeded terminal fixtures, not claimed natural20hall clears: wave/outcome/three boss identities and last lethal hit assigned for layout coverage. Native ContinueEndless, record writes, museum and sandbox use real runtime methods. Discovery gaps remain locked. Captures render live1920x1080 with2x supersampling3840x2160, not native4K source art. Root isolated under diagnostic tempcache, never player persistent profile. No mobile/performance/fullrun/audio approval.");
            File.WriteAllLines(Path.Combine(root, "CaptureNotes.txt"), notes);
            Debug.Log(failed ? "K6_NATIVE_CAPTURE_FAILED" : "K6_NATIVE_CAPTURE_PASSED"); Application.Quit(failed ? 1 : 0);
        }
    }
}
