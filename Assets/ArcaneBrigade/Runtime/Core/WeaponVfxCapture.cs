using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    /// <summary>Native review of every weapon's look in a real fight. Run with -arcane-weapon-vfx-capture -capturePath DIR
    /// [-weapons Id,Id] (isolated diagnostics save): for each weapon, three companions (tier 2) carry it into a wave-8
    /// night at half speed; four frames are saved and every drawn effect renderer near the brigade is listed (art, size,
    /// tint), so plain shapes (a lone ring, a flat disc, a tinted dot) can be found weapon by weapon.</summary>
    public sealed class WeaponVfxCapture : MonoBehaviour
    {
        public static bool Requested => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-weapon-vfx-capture") >= 0;
        private string root;
        private readonly List<string> log = new List<string>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (!Requested || FindAnyObjectByType<WeaponVfxCapture>() != null) return;
            var go = new GameObject("WeaponVfxCapture"); DontDestroyOnLoad(go); go.AddComponent<WeaponVfxCapture>();
        }
        private static string Arg(string key) { string[] a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, key); return i >= 0 && i + 1 < a.Length ? a[i + 1] : null; }
        private IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            Texture2D image = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(root, name + ".png"), image.EncodeToJPG(80)); Destroy(image);
        }
        private void Write() => File.WriteAllLines(Path.Combine(root, "Weapons.txt"), log);
        private void OnLog(string message, string trace, LogType type) { if (type == LogType.Exception) { log.Add("ERROR " + message + " | " + trace.Split('\n')[0]); Write(); } }
        // what each effect renderer is drawing (skips bodies, weapons held in hand, shadows, HUD and the floor)
        private void Survey(string weapon, ArenaController owner)
        {
            var seen = new Dictionary<string, int>();
            foreach (Renderer view in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if (!view.enabled || !view.gameObject.activeInHierarchy || view.GetComponentInParent<UnitVisual>() != null) continue;
                string name = view.name;
                if (name.Contains("Shadow") || name.Contains("Floor") || name.Contains("Lantern") || name.Contains("Fog") || name.Contains("Landmark") || name.Contains("Watcher") || name.Contains("Mist") || name.Contains("Halo") || name.Contains("Bloom ")) continue;
                string art = view is SpriteRenderer s ? s.sprite != null ? s.sprite.name : "-" : view is LineRenderer ? "line" : view is TrailRenderer ? "trail" : view.sharedMaterial != null ? view.sharedMaterial.name : "-";
                string key = name.Split('/')[0] + " :: " + art;
                seen[key] = seen.TryGetValue(key, out int count) ? count + 1 : 1;
            }
            foreach (var pair in seen.OrderByDescending(p => p.Value)) log.Add("  " + weapon + "  " + pair.Value + "x  " + pair.Key);
        }
        private IEnumerator Start()
        {
            root = Arg("-capturePath"); if (root == null) { Application.Quit(1); yield break; }
            root = Path.GetFullPath(root); Directory.CreateDirectory(root); Application.runInBackground = true; Application.logMessageReceived += OnLog;
            if (!RunPaths.Root.Contains("ArcaneBrigadeDiagnostics")) { Application.Quit(1); yield break; }
            var catalog = Resources.Load<RunCatalog>("RunCatalog"); var storage = new RunStorage(RunPaths.Root);
            storage.SaveProfile(new PlayerProfile { BestWave = 40, TutorialSeen = true });
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(.5f);
            string filter = Arg("-weapons");
            var weapons = catalog.Weapons.Where(w => w != null && w.Kind != WeaponKind.Contact && (filter == null || filter.Split(',').Contains(w.Id))).Select(w => w.Id).Distinct().ToList();
            UnitDefinition[] bearers = catalog.Companions.Where(u => !u.SpecialRecruit && !RunSession.StorySpecies(u.Id)).Take(3).ToArray(); // one unit + weapon per roster entry
            int index = 0;
            foreach (string id in weapons)
            {
                index++;
                using (var session = new RunSession(catalog))
                {
                    session.Start("LanternKnight", 171800 + index); session.State.Wave = 8; session.State.Phase = SessionPhase.Combat;
                    session.State.Roster.Clear();
                    foreach (UnitDefinition bearer in bearers)
                    {
                        session.State.Roster.Add(new RosterEntry { UnitId = bearer.Id, WeaponId = id, Tier = 2 });
                        session.State.AcquiredVariants.Add(new RosterEntry { UnitId = bearer.Id, WeaponId = id, Tier = 2 });
                    }
                    try { session.Validate(session.State); } catch (Exception problem) { log.Add("INVALID RUN " + id + ": " + problem.Message); Write(); continue; }
                    storage.SaveRun(session.State);
                }
                RunLaunch.Resume = true; SceneManager.LoadScene("RunArena");
                ArenaController owner = null;
                for (float t = 0; t < 6 && (owner == null || owner.Simulation == null || owner.Simulation.Phase != RunPhase.Combat); t += .1f) { yield return new WaitForSecondsRealtime(.1f); owner = FindAnyObjectByType<ArenaController>(); }
                if (owner?.Simulation == null) { log.Add("NO COMBAT " + id); continue; }
                yield return new WaitForSecondsRealtime(3.5f); // let the first packs arrive
                Time.timeScale = .5f;
                log.Add("== " + index.ToString("00") + " " + id + " (" + catalog.Weapon(id).Kind + ", " + catalog.Weapon(id).Family + ", " + catalog.Weapon(id).Mechanic + ", signature " + catalog.Weapon(id).Signature + ")");
                for (int shot = 0; shot < 4; shot++)
                {
                    yield return new WaitForSecondsRealtime(.45f);
                    if (shot == 1) Survey(id, owner);
                    yield return Shot(index.ToString("00") + "-" + id + "-" + shot);
                }
                Time.timeScale = 1; Write();
            }
            Write(); Application.Quit(0);
        }
        private void OnDestroy() { Application.logMessageReceived -= OnLog; Time.timeScale = 1; }
    }
}
