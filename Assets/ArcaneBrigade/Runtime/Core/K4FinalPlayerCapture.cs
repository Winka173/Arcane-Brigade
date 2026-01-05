using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public sealed class K4FinalPlayerCapture : MonoBehaviour
    {
        private bool failed;
        private string root;
        private readonly List<string> notes = new List<string>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (!RunPaths.K4FinalCapture || FindAnyObjectByType<K4FinalPlayerCapture>() != null) return;
            var runner = new GameObject("K4FinalPlayerCapture"); DontDestroyOnLoad(runner); runner.AddComponent<K4FinalPlayerCapture>();
        }
        private void Awake() { Application.runInBackground = true; Application.logMessageReceived += OnLog; }
        private void OnDestroy() { Application.logMessageReceived -= OnLog; }
        private void OnLog(string message, string stack, LogType kind) { if (kind == LogType.Error || kind == LogType.Exception || kind == LogType.Assert) failed = true; }
        private void Check(bool condition, string name) { notes.Add((condition ? "PASS " : "FAIL ") + name); if (!condition) failed = true; }
        private IEnumerator Wait(Func<bool> condition, float seconds, string name)
        {
            float until = Time.realtimeSinceStartup + seconds;
            while (!condition() && Time.realtimeSinceStartup < until) yield return null;
            Check(condition(), name);
        }
        private IEnumerator Start()
        {
            string[] args = Environment.GetCommandLineArgs(); int path = Array.IndexOf(args, "-capturePath");
            if (Application.isBatchMode || path < 0 || path + 1 >= args.Length) { Application.Quit(1); yield break; }
            root = Path.GetFullPath(args[path + 1]); Directory.CreateDirectory(root);
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(1);
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog"); catalog.WaveSeconds = 18;
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 13104); session.State.Wave = 3; session.State.Gold = 500;
                session.State.Roster.Clear(); string unit = catalog.Companions[0].Id;
                session.State.Roster.Add(new RosterEntry { UnitId = unit, WeaponId = "BellFork" });
                session.State.Roster.Add(new RosterEntry { UnitId = unit, WeaponId = "UmbrellaLance" });
                session.State.Roster.Add(new RosterEntry { UnitId = catalog.Companions[2].Id, WeaponId = "DreamStamp", Tier = 2 });
                session.State.Roster.Add(new RosterEntry { UnitId = catalog.Companions[3].Id, WeaponId = "RuneStaff", Tier = 2 });
                foreach (string id in new[] { "harvest-receipt", "reflective-ink", "office-lens", "carbon-ribbon" }) session.State.Items.Add(new ItemStack { Id = id, Count = 1 });
                new RunStorage(RunPaths.Root).SaveRun(session.State);
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(0.25f);
            ArenaController owner = FindAnyObjectByType<ArenaController>();
            if (owner == null) { Check(false, "ArenaController present"); Finish(); yield break; }
            owner.Simulation.ConfigureLootPresentationFixture();
            Check(owner.Simulation.ArenaToys.Count == 2, "Two seeded props in ordinary arena Start at hall3");
            if (owner.Simulation.ArenaToys.Count < 2) { Finish(); yield break; }
            ArenaToyState bell = owner.Simulation.ArenaToys[0]; ArenaToyState cart = owner.Simulation.ArenaToys[1];
            PlaceSquad(owner, bell.Position);
            for (int index = 0; index < 3; index++) AddEnemy(owner, bell.Position + new Vector2(0.12f, (index - 1) * 0.45f), index == 0 ? 90 : 10000);
            owner.Simulation.AddLootPresentationFixture(LootKind.Coin, owner.Simulation.Champion.Position, 2);
            yield return Wait(() => bell.Mode == ArenaToyMode.Warning, 5, "Auto allied impact charges bell with warning");
            yield return new WaitForEndOfFrame(); Capture("Stage_Bell_Warning_4K");
            yield return Wait(() => owner.Simulation.ArenaToyActivations > 0, 4, "Warned bell payoff resolves through combat Step");
            PlaceSquad(owner, cart.Position);
            for (int index = 0; index < 2; index++) AddEnemy(owner, cart.Position + new Vector2(0.12f, index * 0.4f), 10000);
            yield return Wait(() => cart.Mode == ArenaToyMode.Warning, 5, "Auto allied impact charges cart without manual skill");
            yield return new WaitForEndOfFrame(); Capture("Toy_Cart_Lane_4K");
            yield return Wait(() => owner.Simulation.ArenaToyActivations >= 2 && owner.Simulation.HarvestBeats > 0 && owner.Simulation.StampBursts > 0, 6, "Both props and two signature chains produce real payoff");
            Check(owner.Simulation.Contributions.Any(entry => entry.EffectId == "office-lens" && entry.Damage > 0), "Office weakness yields measured critical bonus");
            owner.ToggleDetails(); owner.Details.SelectTab(3); owner.Details.MoveSelection(100);
            yield return new WaitForEndOfFrame(); Capture("Build_Graph_Contribution_4K"); owner.ToggleDetails();
            yield return Wait(() => owner.Session.State.Phase == SessionPhase.Shop && !owner.TransitionPending, 24, "Hall ends normally and settles loot/contributions into rest");
            if (owner.Session.State.Phase != SessionPhase.Shop) { Finish(); yield break; }
            Check(owner.Session.State.TotalKills > 0 && owner.Session.State.Contributions.Count > 0, "Logical kill/loot and contribution saved at boundary");
            owner.Session.State.Offers = new List<ShopOffer>
            {
                new ShopOffer { Token = ++owner.Session.State.NextOffer, Kind = OfferKind.Companion, Id = catalog.Companions[0].Id, Price = 20 },
                new ShopOffer { Token = ++owner.Session.State.NextOffer, Kind = OfferKind.Item, Id = "office-lens", Price = 38 },
                new ShopOffer { Token = ++owner.Session.State.NextOffer, Kind = OfferKind.Item, Id = "coin-salve", Price = 28 }
            };
            owner.Shop.Display(); WorldShop.Station stand = owner.Shop.Stations.First(station => station.Offer != null);
            owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = stand.Position;
            Camera.main.GetComponent<ArenaCamera>().Snap(); owner.Shop.Display();
            Check(owner.Shop.Interact(), "Physical stand purchase uses normal transaction and random weapon roll");
            Check(owner.Session.State.Roster.Count == 3 && owner.Session.State.Roster.Any(entry => entry.UnitId == catalog.Companions[0].Id && entry.Tier == 2), "Three same-unit materials become one actual tier-two result");
            yield return new WaitForSecondsRealtime(0.42f); yield return new WaitForEndOfFrame(); Capture("Merge_Reveal_4K");
            yield return Wait(() => owner.Shop.Reveal.PendingCount == 0 && owner.Shop.Reveal.CompletedCount > 0, 4, "Merge reveal finishes once after committed save");
            string afterMerge = JsonUtility.ToJson(owner.Session.State); owner.Shop.Reveal.Skip();
            Check(afterMerge == JsonUtility.ToJson(owner.Session.State), "Skip does not consume, roll or change committed state");
            owner.NextWave(); yield return new WaitForSecondsRealtime(0.15f);
            Check(owner.Session.State.Wave == 4 && owner.Simulation.Boss != null, "Normal rest continuation reaches boss hall4");
            if (owner.Simulation.Boss == null) { Finish(); yield break; }
            owner.Simulation.ConfigureLootPresentationFixture(); UnitState boss = owner.Simulation.Boss;
            boss.Definition.Speed = 0; boss.NextBossPattern = boss.NextContact = float.PositiveInfinity;
            boss.Definition.Health = boss.Health = 3000;
            PlaceSquad(owner, boss.Position);
            yield return Wait(() => owner.Simulation.ArenaShiftWarning, 35, "Boss half-health starts arena shift warning");
            yield return new WaitForEndOfFrame(); Capture("Boss_Arena_Shift_Warning_4K");
            yield return Wait(() => owner.Simulation.ArenaShiftComplete, 5, "Boss phase completes once after fair warning");
            yield return new WaitForEndOfFrame(); Capture("Boss_Second_Phase_4K");
            yield return Wait(() => owner.Session.State.Phase == SessionPhase.Shop && !owner.TransitionPending, 45, "Real auto combat defeats boss and returns to saved rest");
            Check(owner.Session.State.BossKills == 1 && owner.Session.State.Eggs >= 3, "Boss settlement gives real capped rewards once");
            string runFile = File.ReadAllText(Path.Combine(RunPaths.Root, "run.json")); string profileFile = File.ReadAllText(Path.Combine(RunPaths.Root, "profile.json"));
            SceneManager.LoadScene("RunFrontend"); yield return new WaitForSecondsRealtime(0.3f);
            RunMenu menu = FindAnyObjectByType<RunMenu>();
            if (menu == null) { Check(false, "Main menu present for sandbox"); Finish(); yield break; }
            menu.OpenWeaponSandbox(); WeaponSandbox sandbox = menu.Sandbox;
            Check(sandbox != null && sandbox.IsOpen && sandbox.WeaponCount > 0, "Menu opens discovered-only sandbox");
            if (sandbox == null || !sandbox.IsOpen) { Finish(); yield break; }
            Check(sandbox.SelectWeapon("DreamStamp"), "Owned/discovered stamp is eligible for preview"); sandbox.SetTier(3); sandbox.SetMovingTarget(true);
            yield return Wait(() => sandbox.FireEvents > 0 && sandbox.SignatureEvents > 0, 8, "Sandbox uses actual attack and tier cadence on moving preset");
            yield return new WaitForEndOfFrame(); Capture("Isolated_Sandbox_4K");
            Check(sandbox.PreviewTexture != null && sandbox.PreviewTexture.width >= 1760 && sandbox.PreviewTexture.antiAliasing == 4, "Desktop sandbox has HD MSAA render target");
            sandbox.ResetPreview(); menu.CloseWeaponSandbox(); yield return null;
            Check(runFile == File.ReadAllText(Path.Combine(RunPaths.Root, "run.json")) && profileFile == File.ReadAllText(Path.Combine(RunPaths.Root, "profile.json")), "Sandbox selection/reset/exit leave actual run/profile save bytes identical");
            Check(sandbox.Simulation == null && sandbox.PreviewTexture == null, "Sandbox exit releases runtime data and render target");
            notes.Add("Windows D3D11 live1920x1080; every PNG Unity supersize2 asserted3840x2160. Not realtime4K benchmark, not natural run balance approval.");
            notes.Add("Seed13104; diagnostic starts hall3 with seeded roster/relics/currency/loot. Hall duration18s; ambient spawns and enemyAI/attacks/allypatrol frozen, squad relocated near seeded props. Stamp crit forced1 to deterministically verify weakness attribution; bossHP3000 and boss bullets frozen. Actual targeting/impacts/root causes/loot/procs/cadence/toys/phase/transactions/merge/continuation/settlement/save and sandbox execute production code. Fixture shop selects unit offer, never selects rolled weapon. User save/profile isolated.");
            Finish();
        }
        private static void PlaceSquad(ArenaController owner, Vector2 target)
        {
            owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = target + Vector2.left * 1.2f;
            int index = 0;
            foreach (UnitState unit in owner.Simulation.Units)
            {
                if (unit.Faction != Faction.Companion) continue;
                unit.Position = unit.PreviousPosition = target + new Vector2(-1, (index++ - 1.5f) * 0.28f);
                unit.RoamOffset = unit.Position - owner.Simulation.Champion.Position; unit.Velocity = Vector2.zero; unit.NextRoamTime = float.PositiveInfinity;
                if (unit.Definition.Weapon.Signature == SignatureWeapon.Stamp) unit.Definition.CriticalChance = 1;
            }
            Camera.main.GetComponent<ArenaCamera>().Snap();
        }
        private static void AddEnemy(ArenaController owner, Vector2 position, float health)
        {
            UnitState enemy = owner.Simulation.AddPresentationFixture(position, health);
            enemy.Definition.Speed = 0; enemy.NextAttack = enemy.NextContact = float.PositiveInfinity;
        }
        private void Capture(string name)
        {
            Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture(2);
            Check(texture != null && texture.width == 3840 && texture.height == 2160, "4K native capture " + name);
            if (texture != null) { File.WriteAllBytes(Path.Combine(root, name + ".png"), texture.EncodeToPNG()); Destroy(texture); }
        }
        private void Finish()
        {
            notes.Add("Result: " + (failed ? "FAIL" : "PASS")); File.WriteAllLines(Path.Combine(root, "Capture_Notes.txt"), notes);
            Debug.Log(failed ? "ARCANE_K4_FINAL_CAPTURE_FAILED" : "ARCANE_K4_FINAL_CAPTURE_PASSED"); Application.Quit(failed ? 1 : 0);
        }
    }
}
