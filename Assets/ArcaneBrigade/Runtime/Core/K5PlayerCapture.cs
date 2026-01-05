using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public sealed class K5PlayerCapture : MonoBehaviour
    {
        private bool failed;
        private string root;
        private readonly List<string> notes = new List<string>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (!RunPaths.K5Capture || FindAnyObjectByType<K5PlayerCapture>() != null) return;
            var runner = new GameObject("K5PlayerCapture"); DontDestroyOnLoad(runner); runner.AddComponent<K5PlayerCapture>();
        }
        private void Awake() { Application.runInBackground = true; Application.logMessageReceived += OnLog; }
        private void OnDestroy() { Application.logMessageReceived -= OnLog; }
        private void OnLog(string message, string trace, LogType kind) { if (kind == LogType.Error || kind == LogType.Exception || kind == LogType.Assert) failed = true; }
        private void Check(bool condition, string name) { notes.Add((condition ? "PASS " : "FAIL ") + name); failed |= !condition; }
        private IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture(2);
            Check(texture != null && texture.width == 3840 && texture.height == 2160, "4K Unity render " + name);
            if (texture != null) { File.WriteAllBytes(Path.Combine(root, name + ".png"), texture.EncodeToPNG()); Destroy(texture); }
            yield return new WaitForSecondsRealtime(0.4f);
            Check(File.Exists(Path.Combine(root, name + ".png")), "Native capture " + name);
        }
        private IEnumerator Start()
        {
            string[] args = Environment.GetCommandLineArgs(); int path = Array.IndexOf(args, "-capturePath");
            if (Application.isBatchMode || path < 0 || path + 1 >= args.Length) { Application.Quit(1); yield break; }
            root = Path.GetFullPath(args[path + 1]); Directory.CreateDirectory(root);
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(1);
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog"); catalog.WaveSeconds = 120;
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 5505); session.State.Wave = 10; session.State.Roster.Clear();
                int index = 0;
                foreach (WeaponDefinition weapon in catalog.Weapons.Where(weapon => weapon.Family != ExpandedWeapon.None || weapon.Signature != SignatureWeapon.None))
                    session.State.Roster.Add(new RosterEntry { UnitId = catalog.Companions[index++ % catalog.Companions.Length].Id, WeaponId = weapon.Id, Tier = 1 });
                foreach (ItemDefinition item in catalog.Items.Where(item => item.SignatureRole != SignatureRelicKind.None)) session.State.Items.Add(new ItemStack { Id = item.Id, Count = 1 });
                new RunStorage(RunPaths.Root).SaveRun(session.State);
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(0.4f);
            ArenaController owner = FindAnyObjectByType<ArenaController>(); owner.Simulation.ConfigureLootPresentationFixture();
            Check(owner.Session.State.Roster.Count == 12 && owner.Simulation.ArenaToys.Count == 6, "Twelve weapon squad plus six seeded props in actual Windows scene");
            foreach (UnitDefinition definition in catalog.Enemies.Where(enemy => enemy.Behavior != EnemyBehavior.Standard))
            {
                Vector2 position = owner.Simulation.Champion.Position + new Vector2(2.3f, (int)definition.Behavior - 2.5f);
                UnitState enemy = owner.Simulation.AddExpandedPresentationFixture(definition, position, 1500); enemy.NextAttack = enemy.NextContact = float.PositiveInfinity;
            }
            owner.Simulation.AddLootPresentationFixture(LootKind.Chest, owner.Simulation.Champion.Position, 12);
            yield return new WaitForSecondsRealtime(0.65f); yield return Shot("01-expanded-combat");
            yield return new WaitForSecondsRealtime(3f); yield return Shot("02-family-payoffs");
            Check(owner.Simulation.ExpandedPayoffs > 0 && owner.Simulation.Contributions.Any(entry => entry.Damage > 0), "Family fields and measured combat damage execute natively");
            int propIndex = 0;
            foreach (ArenaToyState toy in owner.Simulation.ArenaToys)
            {
                owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = toy.Position + Vector2.down * 1.8f;
                Camera.main.GetComponent<ArenaCamera>().Snap();
                for (int charge = 0; charge < toy.RequiredCharge; charge++) owner.Simulation.NotifyArenaToyHit(owner.Simulation.Champion, toy.Position, toy.Position, 0, Vector2.right, new CombatCause(60000 + toy.Id * 10 + charge, owner.Simulation.Champion.Id));
                yield return Shot("03-prop-" + propIndex++ + "-" + toy.Kind); yield return new WaitForSecondsRealtime(1.3f);
            }
            Check(owner.Simulation.ArenaToyActivations >= 6, "Six distinct props charge and resolve in player runtime");
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 5505); session.State.Wave = 12; session.State.Roster.Clear();
                new RunStorage(RunPaths.Root).SaveRun(session.State);
            }
            foreach (BossMotif motif in Enum.GetValues(typeof(BossMotif)))
            {
                RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(0.3f);
                owner = FindAnyObjectByType<ArenaController>(); owner.Simulation.ConfigureLootPresentationFixture();
                UnitState boss = owner.Simulation.Boss; boss.Definition.Motif = motif; boss.Health = boss.Definition.Health * 0.49f; boss.NextBossPattern = float.PositiveInfinity;
                yield return new WaitForSecondsRealtime(0.12f); yield return Shot("04-boss-" + motif);
                Check(owner.Simulation.ExpandedFields.Any(field => field.At > owner.Simulation.Elapsed), motif + " real warned phase appears");
                yield return new WaitForSecondsRealtime(2f); Check(owner.Simulation.Champion.Alive, motif + " player survives base-health phase");
            }
            SceneManager.LoadScene("RunFrontend"); yield return new WaitForSecondsRealtime(0.3f);
            RunMenu menu = FindAnyObjectByType<RunMenu>(); var profile = new PlayerProfile();
            foreach (WeaponDefinition weapon in catalog.Weapons) profile.SeenWeapons.Add(weapon.Id);
            foreach (string id in new[] { "CloudScissors", "SleepCamera", "MusicBox", "CrescentBow" })
            {
                menu.OpenWeaponSandboxForDiagnostics(catalog, profile, null, id, 2, false);
                yield return new WaitForSecondsRealtime(1.6f); yield return Shot("05-sandbox-" + id); menu.CloseWeaponSandbox();
            }
            Check(profile.SeenWeapons.Count == catalog.Weapons.Length && profile.Wins == 0, "Sandbox leaves supplied profile untouched");
            notes.Add("Fixture seed5505: hall10 seeded 12-family roster, relics and chest; ambient spawns disabled, four original enemy types retain movement behavior but contact/ordinary attacks frozen and HP1500 for presentation. For prop screenshots the Champion is relocated and valid primary root charges are injected through NotifyArenaToyHit; actual seeded layout, charge deduplication, warning and activation resolve in runtime. Hall12 empty squad snapshots each actual boss phase; HP set49%, ordinary boss bullets frozen. Sandbox uses a read-only fixture discovery list. Profile/save root isolated. Screens render live Unity1080p with 2x supersampling to3840x2160; raster sources are not native4K. This is functional Windows evidence, not natural playtest/mobile acceptance.");
            File.WriteAllLines(Path.Combine(root, "CaptureNotes.txt"), notes);
            Debug.Log(failed ? "K5_NATIVE_CAPTURE_FAILED" : "K5_NATIVE_CAPTURE_PASSED"); Application.Quit(failed ? 1 : 0);
        }
    }
}
