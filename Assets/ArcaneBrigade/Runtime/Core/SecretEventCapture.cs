using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public sealed class SecretEventCapture : MonoBehaviour
    {
        private readonly List<string> notes = new List<string>();
        private string root;
        private bool failed;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (!RunPaths.K7Capture || Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-secret-capture") < 0 || FindAnyObjectByType<SecretEventCapture>() != null) return;
            var go = new GameObject("SecretEventNativeCapture"); DontDestroyOnLoad(go); go.AddComponent<SecretEventCapture>();
        }
        private void Check(bool condition, string caption)
        {
            failed |= !condition; notes.Add((condition ? "PASS " : "FAIL ") + caption); File.WriteAllLines(Path.Combine(root, "Native_Checks.txt"), notes);
        }
        private void Log(string text, string trace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
            Check(false, text); Application.Quit(1);
        }
        private IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            Texture2D image = ScreenCapture.CaptureScreenshotAsTexture();
            Color32[] pixels = image.GetPixels32(); int visible = 0; long total = 0;
            for (int i = 0; i < pixels.Length; i += 347) { int v = pixels[i].r + pixels[i].g + pixels[i].b; total += v; if (v > 65) visible++; }
            Check(visible > 100 && total > 100000, "Nonblank actual rendered capture " + name);
            File.WriteAllBytes(Path.Combine(root, name + ".png"), image.EncodeToPNG()); Destroy(image);
        }
        private IEnumerator Start()
        {
            string[] args = Environment.GetCommandLineArgs(); int at = Array.IndexOf(args, "-capturePath");
            if (at < 0 || at + 1 >= args.Length) { Application.Quit(1); yield break; }
            root = Path.GetFullPath(args[at + 1]); Directory.CreateDirectory(root); Application.runInBackground = true;
            Application.logMessageReceived += Log;
            // The launcher sets the surface before startup. Resizing a window
            // after moving it off the desktop can fail the DX11 display switch.
            yield return new WaitForSecondsRealtime(1);
            Check(Screen.width == 1920 && Screen.height == 1080, "Actual diagnostic render surface 1920x1080");
            RunCatalog c = Resources.Load<RunCatalog>("RunCatalog");
            var storage = new RunStorage(RunPaths.Root);
            bool reduced = Array.IndexOf(args, "-arcane-secret-reduced") >= 0;
            storage.SaveProfile(new PlayerProfile { ReducedMotion = reduced, ReducedEffects = reduced, ReducedFlashes = reduced });
            for (int k = 0; k < 8; k++)
            {
                bool saved = false;
                for (int seed = 0; seed < 500 && !saved; seed++)
                using (var s = new RunSession(c))
                {
                    s.Start("LanternKnight", seed); s.State.Wave = 8; s.State.Phase = SessionPhase.Shop; // a secret shop (ActOneCanon.SecretShop: 3, 8, 13, 18) s.State.Gold = 200;
                    s.State.Items.Add(new ItemStack { Id = c.Items.First(i => !i.Id.StartsWith("Secret") && i.Rarity < ItemRarity.Crimson && i.EffectiveModifiers.Any(m => m.Value > 0)).Id, Count = 1 });
                    s.State.Roster.Add(new RosterEntry { UnitId = c.Companions.First(u => u.Id != s.State.Roster[0].UnitId && !u.Id.StartsWith("Secret") && u.Rarity < ItemRarity.Crimson).Id, WeaponId = c.Weapons.First(w => w.Kind == WeaponKind.Projectile && !w.SpecialOnly).Id });
                    if (!Array.Exists(s.SecretPortals(), kind => (int)kind == k)) continue;
                    storage.SaveRun(s.State); saved = true;
                }
                Check(saved, "Seeded portal exists " + k);
                RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return null; yield return new WaitForSecondsRealtime(1.6f);
                ArenaController owner = FindAnyObjectByType<ArenaController>();
                Check(owner.SecretRoom != null, "Native portal component " + k);
                if (k == 0) yield return Shot("00-secret-invitations");
                Check(owner.EnterSecret((SecretEventKind)k), "Enter actual secret room " + k); yield return new WaitForSecondsRealtime(.3f);
                Check(owner.Shop.Nearby == null && owner.Shop.Stations.Count >= 0 && owner.SecretRoom.MenuOpen, "Shop interaction replaced by room choices " + k);
                yield return Shot("room-" + k + "-choices");
                int option = k == 0 || k == 2 || k == 6 ? 0 : k == 1 ? 2 : k == 7 ? 2 : 0;
                owner.Session.State.Gold = Mathf.Max(owner.Session.State.Gold, 500); // the fixture's purse (the brass crate costs 172 at shop 8)
                Check(owner.ChooseSecret(option), "Native commits choice " + k);
                if (owner.Session.SecretFighting)
                {
                    owner.Simulation.Champion.Definition.Health = owner.Simulation.Champion.Health = 100000;
                    owner.Session.CombatCatalog.PassiveHeal = 10000;
                    yield return new WaitForSecondsRealtime(2.5f);
                    yield return Shot("room-" + k + "-battle");
                    Check(owner.Simulation.Units.Any(u => u.Faction == Faction.Enemy), "Optional encounter has real actors " + k);
                    // Deliberately accelerated diagnostic fixture, not normal balance evidence.
                    owner.Session.CombatCatalog.WaveDuration = 0;
                    if (owner.Simulation.Boss != null)
                    {
                        owner.Simulation.Champion.Definition.Weapon.Range = 40;
                        owner.Simulation.Champion.Definition.Weapon.Damage = 1000000;
                        owner.Simulation.Champion.Definition.Weapon.Cooldown = .1f;
                    }
                    float deadline = Time.realtimeSinceStartup + 45;
                    while (owner.Session.State.Phase != SessionPhase.Shop && Time.realtimeSinceStartup < deadline) yield return null;
                    yield return new WaitForSecondsRealtime(4f); // a boss fall plays DeathFeedback.DurationBoss + 1 s before the room returns
                    Check(owner.ClaimSecret(), "Native battle reward claims " + k);
                }
                yield return new WaitForSecondsRealtime(.3f);
                yield return Shot("room-" + k + "-result");
                Check(owner.SecretRoom.RiftOpen, "The way back opens with the deal's card " + k);
                if (k == 0)
                {
                    // walking into the open rift goes back to the shop (requirement 25/10/2026)
                    owner.Simulation.Champion.Position = SecretEventRoom.ReturnSpot + Vector2.up * 2.5f; yield return new WaitForSecondsRealtime(1f);
                    owner.Simulation.Champion.Position = SecretEventRoom.ReturnSpot; yield return new WaitForSecondsRealtime(3f);
                    Check(!owner.Session.SecretRoomOpen && owner.Session.State.Phase == SessionPhase.Shop, "Walking into the rift returns to the shop");
                    continue;
                }
                Check(owner.LeaveSecret(), "Native returns to saved shop " + k);
                yield return new WaitForSecondsRealtime(.4f);
                Check(owner.Session.State.Phase == SessionPhase.Shop && owner.Session.State.Wave == 8 && !owner.Session.SecretRoomOpen, "Normal flow preserved " + k);
            }
            Check(!failed, "All native secret-event checks passed"); Application.Quit(failed ? 1 : 0);
        }
        private void OnDestroy() { Application.logMessageReceived -= Log; }
    }
}
