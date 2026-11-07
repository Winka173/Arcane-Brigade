using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace ArcaneBrigade
{
    public sealed partial class OctoberCapture : MonoBehaviour
    {
        public static bool Requested => RunPaths.K7Capture && Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-october-capture") >= 0;
        private string root; private bool failed, gallery; private int pose;
        private readonly List<string> notes = new List<string>();
        private readonly List<UnitVisual> portraits = new List<UnitVisual>();
        private readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (!Requested || FindAnyObjectByType<OctoberCapture>() != null) return;
            var go = new GameObject("OctoberCapture"); DontDestroyOnLoad(go); go.AddComponent<OctoberCapture>();
        }
        private void Check(bool valid, string caption)
        {
            failed |= !valid; notes.Add((valid ? "PASS " : "FAIL ") + caption);
            File.WriteAllLines(Path.Combine(root, "Native_Checks.txt"), notes);
        }
        private void Log(string line, string trace, LogType type)
        {
            if (type != LogType.Exception && type != LogType.Error && type != LogType.Assert) return;
            Check(false, line + "\n" + trace); Application.Quit(1);
        }
        private IEnumerator Shot(string file)
        {
            yield return new WaitForEndOfFrame(); var screen = ScreenCapture.CaptureScreenshotAsTexture();
            Check(screen.width >= 1280 && screen.height >= 720, "Native frame " + file);
            File.WriteAllBytes(Path.Combine(root, file + ".png"), screen.EncodeToPNG()); Destroy(screen);
        }
        private IEnumerator Start()
        {
            string[] args = Environment.GetCommandLineArgs(); int at = Array.IndexOf(args, "-capturePath");
            if (at < 0 || at + 1 >= args.Length) { Application.Quit(1); yield break; }
            root = Path.GetFullPath(args[at + 1]); Directory.CreateDirectory(root); Application.runInBackground = true;
            Application.logMessageReceived += Log;
            yield return K7PlayerCapture.ResizeDiagnostic(1920, 1080, Check);
            RunCatalog c = Resources.Load<RunCatalog>("RunCatalog"); var storage = new RunStorage(RunPaths.Root);
            bool reduced = Array.IndexOf(args, "-arcane-october-reduced") >= 0;
            if (Array.IndexOf(args, "-arcane-companion-visibility") >= 0)
            {
                yield return ReviewCompanionVisibility(c, reduced);
                Application.Quit(failed ? 1 : 0); yield break;
            }
            var profile = new PlayerProfile { BestWave = 40, ReducedMotion = reduced, ReducedEffects = reduced, ReducedFlashes = reduced };
            for (int i = 0; i < RunMeta.DifficultyNames.Length; i++) RunMeta.Add(profile.Achievements, "clear-difficulty-" + i);
            foreach (var d in RunAchievements.Definitions(c)) RunAchievements.Mark(profile.AchievementEvidence, d.Metric, d.Goal * .5f);
            storage.SaveProfile(profile);
            RunMenu menu = FindAnyObjectByType<RunMenu>(); menu.ConfigureK7Diagnostics(c, profile);
            menu.SelectChampionForDiagnostics(10); yield return new WaitForSecondsRealtime(.4f); yield return Shot("01-roster-modes-1080");
            menu.ShowPage(RunMenu.Page.Achievements); yield return new WaitForSecondsRealtime(.4f); yield return Shot("02-achievement-journal");
            yield return K7PlayerCapture.ResizeDiagnostic(1280, 720, Check);
            menu.ShowPage(RunMenu.Page.Roster); yield return new WaitForSecondsRealtime(.4f); yield return Shot("03-roster-modes-720");
            yield return K7PlayerCapture.ResizeDiagnostic(1920, 1080, Check);
            Scene old = SceneManager.GetActiveScene(); Scene galleryScene = SceneManager.CreateScene("OctoberPortraits"); SceneManager.SetActiveScene(galleryScene); yield return SceneManager.UnloadSceneAsync(old);
            var camera = new GameObject("PortraitCamera").AddComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = 3.6f; camera.transform.position = new Vector3(0, 0, -10); camera.backgroundColor = new Color(.07f, .055f, .11f);
            gallery = true; int id = 0;
            foreach (var row in OctoberContent.Roster.newHeroes)
            {
                var u = Instantiate(c.Unit(row.id)); owned.Add(u); u.Weapon = Instantiate(u.Weapon); owned.Add(u.Weapon); u.BodyHeight = 1.15f;
                var state = new UnitState { Id = ++id, Definition = u, Faction = Faction.Champion, Alive = true, Health = u.Health, Position = new Vector2((id - 1) % 7 * 1.75f - 5.25f, 1.75f - (id - 1) / 7 * 2.15f), Facing = Vector2.right };
                state.PreviousPosition = state.Position;
                UnitVisual view = Instantiate(c.Arena.UnitPrefab).GetComponent<UnitVisual>(); view.Bind(state, c.Arena, true); portraits.Add(view);
                Check(view.ActingArt != null, "Authored expressive poses " + u.Id);
            }
            for (pose = 0; pose < 4; pose++)
            {
                foreach (UnitVisual v in portraits) v.PreviewActing(pose, .45f);
                yield return new WaitForSecondsRealtime(.25f); yield return Shot("04-new-heroes-reaction-" + pose);
            }
            gallery = false; foreach (UnitVisual v in portraits) Destroy(v.gameObject); portraits.Clear();
            foreach (UnityEngine.Object o in owned) Destroy(o); owned.Clear(); Destroy(camera.gameObject);
            using (var s = new RunSession(c))
            {
                s.Start("VerdantMatron", 106616); s.State.Wave = 12; s.State.Phase = SessionPhase.Shop; s.State.Gold = 1000; s.State.Eggs = 20;
                s.State.Roster.Clear(); s.State.Roster.Add(new RosterEntry { UnitId = "CarouselPony", WeaponId = "InkPistol" }); s.State.Roster.Add(new RosterEntry { UnitId = "GrandSprout", WeaponId = "SpoonSabre", Tier = 3 });
                s.State.Roster.Add(new RosterEntry { UnitId = "LittleSprout", WeaponId = "TinFish", Tier = 2 });
                if (Array.IndexOf(args, "-arcane-bond-summary-capture") >= 0) s.State.Roster.Add(new RosterEntry { UnitId = "VelvetChampion", WeaponId = "InkPistol" });
                foreach (ItemDefinition item in c.Items.Where(i => !string.IsNullOrEmpty(i.ReferenceName)).Take(20)) s.State.Items.Add(new ItemStack { Id = item.Id, Count = 1 });
                foreach (ItemRarity rarity in new[] { ItemRarity.Rare, ItemRarity.Mythic, ItemRarity.Crimson })
                { ItemDefinition item = c.Items.First(i => i.Rarity == rarity); s.State.Offers.Add(new ShopOffer { Token = ++s.State.NextOffer, Id = item.Id, Kind = OfferKind.Item, Price = 25 }); }
                storage.SaveRun(s.State);
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(3);
            ArenaController owner = FindAnyObjectByType<ArenaController>();
            owner.SetPresentationMoveTarget(owner.Shop.Stations.First(s => s.Offer != null).Position); yield return new WaitForSecondsRealtime(2.5f); yield return Shot("05-nightmare-shop-grounding");
            if (Array.IndexOf(args, "-arcane-bond-summary-capture") >= 0)
            {
                var summary = new BondHudSummary(); summary.Refresh(c.Bonds, owner.Session.BondCounts);
                Check(summary.Rows.Count == 29 && summary.VisibleCount == 5 && summary.Height <= 224 && summary.HiddenCount == 24, "All 29 present bonds fit a five-row overview with 24 more accessible");
                yield return K7PlayerCapture.ResizeDiagnostic(1280, 720, Check); yield return new WaitForSecondsRealtime(.3f); yield return Shot("05c-bonds-720");
                owner.ToggleDetails(); owner.Details.SelectBond(summary.Rows[0].Name);
                Check(owner.Details.Open && owner.Details.Tab == 3 && owner.Simulation.Phase == RunPhase.Paused, "View all bonds opens Links at the selected bond and pauses combat");
                yield return new WaitForSecondsRealtime(.3f); yield return Shot("05d-bonds-full-list-720"); owner.ToggleDetails();
                yield return K7PlayerCapture.ResizeDiagnostic(1920, 1080, Check);
            }
            yield return new WaitForEndOfFrame();
            Material floor = GameObject.Find("HD_TiledFloor").GetComponent<MeshRenderer>().sharedMaterial;
            Check(floor.GetTexture("_NightmareFloor") == OctoberArtwork.Current.Floor && floor.GetFloat("_NightmareStrength") == 1, "Wave 12 uses the actual painted nightmare carpet");
            var block = new MaterialPropertyBlock();
            Check(FindObjectsByType<UnitVisual>().All(v => v.GetComponentsInChildren<SpriteRenderer>().All(r => { r.GetPropertyBlock(block); return block.GetFloat("_Flash") == 0; })), "Friendly presentation has no damage or attack flash");
            Check(FindObjectsByType<EvolutionAdornment>().Length == 0 && FindObjectsByType<BuildMorphAppearance>().Length == 0, "Evolved allies use body size without added accessories");
            ShopDisplayActor merchant = FindObjectsByType<ShopDisplayActor>().First(a => a.GetComponent<MerchantPortraitRig>() != null);
            Rect merchantInk = SpriteInkMetrics.BoundsOf(merchant.View.sprite);
            GroundShadow merchantShadow = merchant.GetComponent<GroundShadow>();
            float merchantGap = merchant.View.transform.TransformPoint(new Vector3(merchantInk.center.x, merchantInk.yMin)).y - merchantShadow.Contact.transform.position.y;
            Check(merchantGap > .15f && merchantGap < .6f, "Merchant deliberately hovers above a separate floor shadow");
            Vector2 threshold = owner.Shop.Stations.First(s => s.Action == "NEXT HALL").Position;
            owner.SetPresentationMoveTarget(threshold + Vector2.right * 1.5f); yield return new WaitForSecondsRealtime(3);
            yield return new WaitForEndOfFrame();
            OctoberDoor door = FindAnyObjectByType<OctoberDoor>(); SpriteRenderer doorView = door.GetComponent<SpriteRenderer>();
            Check(Array.IndexOf(OctoberArtwork.Current.Door, doorView.sprite) >= 1, "Painted door opens as the champion approaches");
            Rect ink = SpriteInkMetrics.BoundsOf(doorView.sprite);
            float doorGap = doorView.transform.TransformPoint(new Vector3(ink.center.x, ink.yMin)).y - doorView.transform.parent.position.y;
            Check(doorGap > .15f && doorGap < .5f, "Open door floats above its fixed reachable threshold");
            yield return Shot("05b-next-hall-door");
            owner.ToggleDetails(); owner.Details.SelectTab(2); yield return new WaitForSecondsRealtime(.4f); yield return Shot("06-owned-relics"); owner.ToggleDetails();
            owner.Session.State.OutsideStage = OutsideStageStatus.Reward; owner.Session.State.OutsideRewards = new List<string> { "RainbowHeart", "RainbowBell", "RainbowPrism" };
            yield return new WaitForSecondsRealtime(.5f); yield return Shot("07-rainbow-description");
            owner.Session.State.OutsideStage = OutsideStageStatus.Completed; owner.Session.State.OutsideRewards.Clear();
            storage.SaveRun(owner.Session.State);
            using (var s = new RunSession(c))
            {
                s.Restore(owner.Session.State); s.State.Phase = SessionPhase.Combat; s.State.OutsideStage = OutsideStageStatus.Fighting; storage.SaveRun(s.State);
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(1.6f); owner = FindAnyObjectByType<ArenaController>();
            owner.Simulation.OctoberCaptureHealth(); yield return new WaitForSecondsRealtime(4); yield return Shot("08-keeper-bullet-pattern");
            owner.Simulation.AddLootPresentationFixture(LootKind.Chest, owner.Simulation.Champion.Position + Vector2.right * 3, 1);
            yield return new WaitForSecondsRealtime(.6f);
            Check(FindObjectsByType<SpriteRenderer>().Any(r => r.name == "ChestGroundGlow" && r.gameObject.activeInHierarchy && r.color.a >= .18f), "Unopened chest has a persistent readable ground glow");
            yield return Shot("08b-chest-glow");
            owner.TogglePause(); yield return null; yield return Shot("09-pause-padding");
            Check(!failed, "October native presentation checks complete"); Application.Quit(failed ? 1 : 0);
        }
        private void Update()
        {
            if (!gallery) return; foreach (UnitVisual v in portraits) v.Display(Time.unscaledTime, 1);
        }
        private void OnGUI()
        {
            if (!gallery) return;
            Matrix4x4 old = GUI.matrix; GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 1920f, Screen.height / 1080f, 1));
            LocalizedGUI.Label(new Rect(70, 35, 1750, 70), new[] { "CURIOSITY", "CELEBRATION", "RECOIL WITHOUT FLASH", "EXHAUSTION" }[Mathf.Clamp(pose, 0, 3)], ArcadeTypography.Style(36, true));
            GUI.matrix = old;
        }
        private void OnDestroy() => Application.logMessageReceived -= Log;
    }
    public sealed partial class CombatSimulation
    {
        public void OctoberCaptureHealth()
        {
            if (!OctoberCapture.Requested) throw new InvalidOperationException("Isolated October capture required");
            Champion.Health = Champion.Definition.Health; catalog.Armor = 100000;
        }
    }
}
