using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    /// <summary>Native review sheets of the Bizarre Arsenal's painted-ink effects over the real arena floor: every moment at
    /// three points of its life (action, peak, settle), then the room pieces, then live fights with Arsenal weapons in hand.
    /// Run with -arcane-arsenal-capture -capturePath DIR [-arsenal-weapons Id,Id]. Uses the isolated diagnostics save.</summary>
    public sealed class ArsenalInkCapture : MonoBehaviour
    {
        public static bool Requested => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-arsenal-capture") >= 0;
        private string root;
        private readonly List<string> notes = new List<string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (!Requested || FindAnyObjectByType<ArsenalInkCapture>() != null) return;
            var go = new GameObject("ArsenalInkCapture"); DontDestroyOnLoad(go); go.AddComponent<ArsenalInkCapture>();
        }
        private static string Arg(string[] args, string key) { int i = Array.IndexOf(args, key); return i >= 0 && i + 1 < args.Length ? args[i + 1] : null; }
        private IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            Texture2D image = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(root, name + ".png"), image.EncodeToPNG());
            Destroy(image);
        }
        private void Log(string message, string trace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception) { notes.Add("ERROR " + message + " " + trace); File.WriteAllLines(Path.Combine(root, "Notes.txt"), notes); }
        }

        private IEnumerator Start()
        {
            string[] args = Environment.GetCommandLineArgs();
            root = Arg(args, "-capturePath"); if (root == null) { Application.Quit(1); yield break; }
            root = Path.GetFullPath(root); Directory.CreateDirectory(root); Application.runInBackground = true;
            Application.logMessageReceived += Log;
            var catalog = Resources.Load<RunCatalog>("RunCatalog"); var storage = new RunStorage(RunPaths.Root);
            if (!RunPaths.Root.Contains("ArcaneBrigadeDiagnostics")) { Application.Quit(1); yield break; }
            storage.SaveProfile(new PlayerProfile { BestWave = 40, TutorialSeen = true });
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(.6f);
            if (Array.IndexOf(args, "-combat-look") >= 0)
            {
                // an ordinary wave-4 fight with a ranged brigade, to judge shots and dropped coins at play size
                using (var session = new RunSession(catalog))
                {
                    string hero = Arg(args, "-look-hero") ?? catalog.Champions[0].Id;
                    session.Start(hero, 171020); session.State.Wave = 4;
                    if (Array.IndexOf(args, "-look-own-brigade") < 0)
                    {
                        session.State.Roster.Clear();
                        foreach (string w in (Arg(args, "-look-weapons") ?? "GumballGatling,DreamRacket,SquirtTrio,CorkRifle,SaucerBlaster").Split(','))
                            if (catalog.Weapon(w) != null) session.State.Roster.Add(new RosterEntry { UnitId = "TeapotPilgrim", WeaponId = w, Tier = 1 });
                    }
                    storage.SaveRun(session.State);
                }
                RunLaunch.Resume = true; SceneManager.LoadScene("RunArena");
                float started = Time.realtimeSinceStartup;
                int frames = int.TryParse(Arg(args, "-look-frames"), out int n) ? n : 4; float gap = float.TryParse(Arg(args, "-look-gap"), out float g) ? g : 2.5f;
                bool awaken = Array.IndexOf(args, "-look-awaken") >= 0, primed = false;
                for (int frame = 0; frame < frames; frame++)
                {
                    while (Time.realtimeSinceStartup - started < 7f + frame * gap)
                    {
                        // -look-awaken: fill the hero's awakening gauge a second before the first frame
                        if (awaken && !primed && Time.realtimeSinceStartup - started > 5.5f)
                        { var arena = FindAnyObjectByType<ArenaController>(); if (arena != null && arena.Simulation != null) { arena.Simulation.PrimeAwakeningForCapture(); primed = true; } }
                        if (awaken) { var arena = FindAnyObjectByType<ArenaController>(); if (arena != null && arena.Simulation != null && arena.Simulation.Champion != null) arena.Simulation.Champion.Health = arena.Simulation.Champion.Definition.Health; }
                        yield return null;
                    }
                    yield return Shot("combat-look-" + frame.ToString("00"));
                }
                Application.Quit(0); yield break;
            }
            if (Array.IndexOf(args, "-shop-approach") >= 0) { yield return ShopApproach(catalog, storage); Application.Quit(0); yield break; }
            if (Array.IndexOf(args, "-star-up") >= 0) { yield return StarUp(catalog, storage, args); Application.Quit(0); yield break; }
            if (Array.IndexOf(args, "-boss-telegraph") >= 0) { yield return BossWindups(catalog, storage, args); Application.Quit(0); yield break; }
            string filter = Arg(args, "-arsenal-weapons");
            if (Array.IndexOf(args, "-arsenal-live-only") < 0)
            {
                ArenaController owner = null;
                yield return Fight(catalog, storage, null, o => owner = o);
                if (owner == null) { notes.Add("NO COMBAT"); File.WriteAllLines(Path.Combine(root, "Notes.txt"), notes); Application.Quit(1); yield break; }
                Time.timeScale = 0;
                yield return new WaitForSecondsRealtime(.4f);
                yield return Sheets();
                Time.timeScale = 1;
                notes.Add("OK sheets");
            }
            // live: a companion carrying each Arsenal weapon in the catalog fights four still dummies in front of the hero
            int index = 0;
            foreach (WeaponDefinition weapon in catalog.Weapons)
            {
                if (weapon == null || weapon.Arsenal == ArsenalWeapon.None) continue;
                index++;
                if (filter != null && Array.IndexOf(filter.Split(','), weapon.Id) < 0) continue;
                ArenaController owner = null;
                yield return Fight(catalog, storage, weapon.Id, o => owner = o);
                if (owner == null) { notes.Add("NO COMBAT " + weapon.Id); continue; }
                CombatSimulation sim = owner.Simulation;
                sim.PrepareArsenalCapture(catalog);
                Time.timeScale = .5f;
                float started = Time.realtimeSinceStartup;
                for (int frame = 0; frame < 3; frame++)
                {
                    while (Time.realtimeSinceStartup - started < 1.6f + frame * .45f) yield return null;
                    yield return Shot("live-" + index.ToString("00") + "-" + weapon.Id + "-" + frame);
                }
                Time.timeScale = 1;
                notes.Add("OK live " + weapon.Id + " props " + sim.ArsenalProps.Count);
                File.WriteAllLines(Path.Combine(root, "Notes.txt"), notes);
            }
            File.WriteAllLines(Path.Combine(root, "Notes.txt"), notes);
            Application.Quit(0);
        }

        private IEnumerator Fight(RunCatalog catalog, RunStorage storage, string weapon, Action<ArenaController> found)
        {
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 171018); session.State.Wave = 3; session.State.Roster.Clear();
                if (weapon != null) session.State.Roster.Add(new RosterEntry { UnitId = "TeapotPilgrim", WeaponId = weapon, Tier = 1 });
                storage.SaveRun(session.State);
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena");
            ArenaController owner = null;
            for (float t = 0; t < 8 && (owner == null || owner.Simulation == null || owner.Simulation.Phase != RunPhase.Combat); t += .1f) { yield return new WaitForSecondsRealtime(.1f); owner = FindAnyObjectByType<ArenaController>(); }
            found(owner != null && owner.Simulation != null ? owner : null);
        }

        /// <summary>-star-up [-star-unit Id -star-weapon Id]: the two-star and the three-star ceremonies in a shop, scrubbed to
        /// fixed moments (star2-SS / star3-SS, SS = hundredths of a second into the ceremony).</summary>
        private IEnumerator StarUp(RunCatalog catalog, RunStorage storage, string[] args)
        {
            string unit = Arg(args, "-star-unit") ?? "TeapotPilgrim", weapon = Arg(args, "-star-weapon") ?? "KeyCleaver";
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 171021); session.State.Wave = 3; session.State.Phase = SessionPhase.Shop;
                session.State.Roster.Clear(); session.State.Roster.Add(new RosterEntry { UnitId = unit, WeaponId = weapon, Tier = 3 });
                storage.SaveRun(session.State);
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena");
            yield return new WaitForSecondsRealtime(2.6f);
            var owner = FindAnyObjectByType<ArenaController>();
            if (owner == null || owner.Shop == null || owner.Shop.Reveal == null) { notes.Add("NO SHOP"); File.WriteAllLines(Path.Combine(root, "Notes.txt"), notes); yield break; }
            MergeReveal reveal = owner.Shop.Reveal;
            Vector2 at = Camera.main != null ? (Vector2)Camera.main.transform.position + Vector2.down * 1.4f : Vector2.zero;
            foreach (int tier in new[] { 2, 3 })
            {
                var materials = new List<RosterEntry>();
                for (int i = 0; i < 3; i++) materials.Add(new RosterEntry { UnitId = unit, WeaponId = weapon, Tier = tier - 1 });
                reveal.Stage(new RosterEntry { UnitId = unit, WeaponId = weapon, Tier = tier }, materials, at);
                float[] moments = tier == 2 ? new[] { .15f, .32f, .5f, .66f, .74f, .8f, .88f, 1f, 1.25f, 1.55f } : new[] { .15f, .4f, .7f, .95f, 1.08f, 1.22f, 1.32f, 1.45f, 1.6f, 1.8f, 2.05f, 2.4f, 2.8f };
                foreach (float moment in moments)
                {
                    reveal.Scrub = moment;
                    yield return null; yield return null;
                    yield return Shot("star" + tier + "-" + Mathf.RoundToInt(moment * 100).ToString("000"));
                }
                reveal.Scrub = -1; reveal.Skip(); yield return null;
                notes.Add("OK star " + tier);
            }
            File.WriteAllLines(Path.Combine(root, "Notes.txt"), notes);
        }

        /// <summary>-boss-telegraph [-boss-wave N]: a boss fight; each of the boss's first wind-ups is shot three times as its ink
        /// spreads (windup-V-a/b/c). The hero is kept alive so the fight runs long enough.</summary>
        private IEnumerator BossWindups(RunCatalog catalog, RunStorage storage, string[] args)
        {
            int wave = int.TryParse(Arg(args, "-boss-wave"), out int w) ? w : 5;
            using (var session = new RunSession(catalog)) { session.Start(catalog.Champions[0].Id, 171022); session.State.Wave = wave; storage.SaveRun(session.State); }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena");
            ArenaController owner = null;
            for (float t = 0; t < 10 && (owner == null || owner.Simulation == null || owner.Simulation.Boss == null || owner.Simulation.Phase != RunPhase.Combat); t += .1f) { yield return new WaitForSecondsRealtime(.1f); owner = FindAnyObjectByType<ArenaController>(); }
            if (owner == null || owner.Simulation == null || owner.Simulation.Boss == null) { notes.Add("NO BOSS"); File.WriteAllLines(Path.Combine(root, "Notes.txt"), notes); yield break; }
            CombatSimulation sim = owner.Simulation; UnitState boss = sim.Boss;
            notes.Add("boss " + boss.Definition.Id + " pattern " + boss.Definition.BossPattern);
            float deadline = Time.realtimeSinceStartup + 60;
            for (int volley = 0; volley < 5 && Time.realtimeSinceStartup < deadline && boss.Alive; volley++)
            {
                while (boss.TelegraphUntil <= sim.Elapsed && Time.realtimeSinceStartup < deadline && boss.Alive) { sim.Champion.Health = sim.Champion.Definition.Health; yield return null; }
                float until = boss.TelegraphUntil, windup = BossEncounterRules.Windup(boss);
                notes.Add("windup " + volley + " signature " + boss.ActiveBossSignature + " windup " + windup);
                foreach (var (label, at) in new[] { ("a", .3f), ("b", .7f), ("c", .93f) })
                {
                    while (sim.Elapsed < until - windup * (1 - at) && boss.TelegraphUntil > 0 && boss.Alive) { sim.Champion.Health = sim.Champion.Definition.Health; yield return null; }
                    Time.timeScale = 0; yield return null;
                    yield return Shot("windup-" + volley + "-" + label);
                    Time.timeScale = 1;
                }
                while (boss.TelegraphUntil == until && Time.realtimeSinceStartup < deadline && boss.Alive) { sim.Champion.Health = sim.Champion.Definition.Health; yield return null; }
            }
            File.WriteAllLines(Path.Combine(root, "Notes.txt"), notes);
        }

        /// <summary>-shop-approach: the hero walks up to each stall of a wave-1 shop in turn (as the player does); one frame each.</summary>
        private IEnumerator ShopApproach(RunCatalog catalog, RunStorage storage)
        {
            using (var session = new RunSession(catalog)) { session.Start(catalog.Champions[0].Id, 171019); session.State.Wave = 2; session.State.Phase = SessionPhase.Shop; session.State.Gold = 200; session.Reroll(); session.State.Gold = 200; storage.SaveRun(session.State); }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena");
            yield return new WaitForSecondsRealtime(2.6f);
            var owner = FindAnyObjectByType<ArenaController>();
            if (owner == null || owner.Shop == null) { notes.Add("NO SHOP"); yield break; }
            int i = 0;
            foreach (var station in owner.Shop.Stations)
            {
                i++;
                Vector2 at = station.Position + Vector2.down * .9f;
                for (float t = 0; t < 1.2f; t += Time.unscaledDeltaTime)
                {
                    owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = at;
                    yield return null;
                }
                // the hover cue exactly as ShopInspect draws it: the entry ShopInspect would pick under the item's centre
                var inspect = owner.GetComponent<ShopInspect>();
                Rect shown = default;
                foreach (SpriteRenderer view in station.Marker.GetComponentsInChildren<SpriteRenderer>())
                    if (view != null && view.enabled && view.sprite != null && view.color.a > 0.2f && view.name == "AnimatedShopActor")
                    { Rect r = ShopInspect.Screen(ShopInspect.Ink(view), Camera.main); shown = shown.width == 0 ? r : Rect.MinMaxRect(Mathf.Min(shown.xMin, r.xMin), Mathf.Min(shown.yMin, r.yMin), Mathf.Max(shown.xMax, r.xMax), Mathf.Max(shown.yMax, r.yMax)); }
                var pick = inspect != null ? inspect.At(shown.center, Camera.main) : null;
                focusBox = pick != null ? ShopInspect.Screen(pick.World, Camera.main) : default;
                notes.Add("   hover picks " + (pick != null ? pick.Key + " " + focusBox : "nothing"));
                notes.Add("station " + i + " " + (station.Offer != null ? station.Offer.Kind + " " + station.Offer.Id : station.Description) + " nearby=" + (owner.Shop.Nearby == station));
                yield return Shot("approach-" + i.ToString("00"));
            }
            File.WriteAllLines(Path.Combine(root, "Notes.txt"), notes);
        }

        private Rect focusBox;
        private void OnGUI()
        {
            if (focusBox.width <= 0 || Camera.main == null) return;
            Matrix4x4 previous = ArcadeTypography.BeginViewport(Camera.main.pixelRect, 1920, 1080);
            // the same rule as ShopInspect: the marks stop above the shop card
            Rect box = focusBox, card = RunHud.ShownShopCard;
            if (card.height > 0 && box.Overlaps(card) && box.yMin < card.yMin) box.yMax = card.yMin - 4;
            if (card.height <= 0 || box.yMin < card.yMin) ArcadeSkin.WorldFocus(box);
            GUI.matrix = previous;
        }

        private Camera ViewCamera()
        {
            Camera cam = null; foreach (var c in Camera.allCameras) if (c.orthographic && (cam == null || c.orthographicSize < cam.orthographicSize)) cam = c;
            return cam;
        }

        /// <summary>Seven moments a page, each at three points of its life, laid out over the live arena (the fight is paused).</summary>
        private IEnumerator Sheets()
        {
            Camera cam = ViewCamera(); if (cam == null) { notes.Add("NO CAMERA"); yield break; }
            var host = new GameObject("Arsenal ink review"); var ink = new ArsenalInk(host.transform);
            Vector2 c = cam.transform.position; float h = cam.orthographicSize * 2, w = h * cam.aspect;
            var moments = (ArsenalInk.Moment[])Enum.GetValues(typeof(ArsenalInk.Moment));
            float[] phase = { .18f, .42f, .72f };
            Sprite gavel = ArsenalArtwork.Frame("VerdictGavel_Held", 0);
            const int rows = 4, columns = 2;
            for (int page = 0; page * rows * columns < moments.Length; page++)
            {
                ink.Begin();
                for (int k = 0; k < rows * columns; k++)
                {
                    int index = page * rows * columns + k; if (index >= moments.Length) break;
                    int row = k % rows, column = k / rows;
                    for (int s = 0; s < phase.Length; s++)
                    {
                        Vector2 at = c + new Vector2(-w * .5f + w * (column * .5f + (s + .5f) / 6f), h * .5f - h * (row + .55f) / rows);
                        ink.Draw(moments[index], at, Vector2.right, phase[s], 1f, 23 + index * 31, gavel);
                    }
                    notes.Add($"page {page} row {row} col {column}: {moments[index]}");
                }
                ink.End();
                yield return new WaitForSecondsRealtime(.25f);
                yield return Shot("ink-moments-" + page);
            }
            // the room pieces
            ink.Begin();
            float now = 1.3f; Vector2 o = c + new Vector2(-w * .36f, h * .28f);
            ink.Pool(o, .8f, 1, ArsenalInk.Green, 3); ink.Bubbles(o, .8f, now, ArsenalInk.Green, 5, 1);
            ink.Pool(o + new Vector2(2.4f, 0), .7f, 1, ArsenalInk.Violet, 9, .9f, .45f);
            ink.Spiral(o + new Vector2(4.8f, 0), .45f, .5f, now, 3);
            ink.Coil(o + new Vector2(6.4f, -.3f), o + new Vector2(8.6f, .3f));
            Vector2 r2 = c + new Vector2(-w * .36f, 0);
            ink.Cord(r2, r2 + new Vector2(2.2f, .2f), ArsenalInk.Wine, .06f, false);
            ink.Cord(r2 + new Vector2(2.8f, -.2f), r2 + new Vector2(5f, 0), ArsenalInk.Midnight, .04f, true);
            ink.Stripes(r2 + new Vector2(5.6f, 0), r2 + new Vector2(7.2f, 0), .25f, now);
            ink.Orbit(r2 + new Vector2(8.4f, .2f), .26f, now, InkToken.Star, ArsenalInk.Gold, 3);
            Vector2 r3 = c + new Vector2(-w * .36f, -h * .28f);
            ink.Wake(r3, Vector2.right * 6, ArsenalInk.Cyan, now, 1, false);
            ink.Wake(r3 + new Vector2(2.6f, 0), Vector2.right * 6, ArsenalInk.Water, now, 2, true);
            ink.Streaks(r3 + new Vector2(5f, 0), Vector2.right, 1);
            ink.Shards(r3 + new Vector2(7f, 0), .4f, 5);
            ink.End();
            yield return new WaitForSecondsRealtime(.25f);
            yield return Shot("ink-room-pieces");
            ink.Begin(); ink.End(); ink.Destroy(); Destroy(host);
        }
        private void OnDestroy() => Application.logMessageReceived -= Log;
    }

    public sealed partial class CombatSimulation
    {
        /// <summary>Capture-only: four still, sturdy dummies in front of the hero; the hero holds its attack so the companion's
        /// Arsenal weapon is what you see.</summary>
        public void PrepareArsenalCapture(RunCatalog c)
        {
            if (!ArsenalInkCapture.Requested) throw new InvalidOperationException("Isolated arsenal capture required");
            catalog.EnemyBudget = 0; catalog.Armor = 100;
            foreach (UnitState u in units.ToArray()) if (u.Faction == Faction.Enemy && u.Alive) ApplyDamage(u, 1e7f, Faction.Champion, injectedDamage: true);
            var dummy = UnityEngine.Object.Instantiate(c.Enemies[0]); dummy.Speed = 0; dummy.Health = 100000; dummy.ContactDamage = 0; dummy.Behavior = EnemyBehavior.Standard;
            Vector2 p = Champion.Position;
            foreach (Vector2 o in new[] { new Vector2(2.2f, .3f), new Vector2(3.1f, -.7f), new Vector2(3.5f, .9f), new Vector2(1.6f, -1.2f) })
            {
                var u = Spawn(dummy, Faction.Enemy, p + o); u.NextAttack = u.NextContact = float.PositiveInfinity;
            }
            Champion.NextAttack = float.PositiveInfinity;
        }
    }
}
