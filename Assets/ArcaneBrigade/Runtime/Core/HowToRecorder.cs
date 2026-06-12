using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    /// <summary>Records How to Play's clips from real play (requirement 24/10/2026: "the guide clips should be our own gameplay"):
    /// -arcane-howto-record -capturePath DIR. Each clip is staged as a real run (a night, a boss night, a transformation, a
    /// shop purchase, a merge, a brigade with a bond, a relic) and photographed at its best moment (requirement: "take pictures
    /// rather than clips"), cropped round the hero to a 960x540 JPG (clipN.jpg) for Resources/HowTo/ClipN.</summary>
    public sealed class HowToRecorder : MonoBehaviour
    {
        public static bool Requested => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-howto-record") >= 0;
        public const int Width = 960, Height = 540;
        private string root;
        private readonly List<string> notes = new List<string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (!Requested || FindAnyObjectByType<HowToRecorder>() != null) return;
            var go = new GameObject("HowToRecorder"); DontDestroyOnLoad(go); go.AddComponent<HowToRecorder>();
        }

        private IEnumerator Grab(string clip, int index)
        {
            yield return new WaitForEndOfFrame();
            Texture2D full = ScreenCapture.CaptureScreenshotAsTexture();
            RenderTexture rt = RenderTexture.GetTemporary(Width, Height, 0);
            bool whole = Array.IndexOf(Environment.GetCommandLineArgs(), "-howto-full") >= 0; // review: the whole screen
            if (whole) Graphics.Blit(full, rt); else Graphics.Blit(full, rt, new Vector2(.62f, .62f), new Vector2(.19f, cropY)); // round the hero (in the shop: the stalls' row)
            RenderTexture before = RenderTexture.active; RenderTexture.active = rt;
            var small = new Texture2D(Width, Height, TextureFormat.RGB24, false); small.ReadPixels(new Rect(0, 0, Width, Height), 0, 0); small.Apply();
            RenderTexture.active = before; RenderTexture.ReleaseTemporary(rt);
            File.WriteAllBytes(Path.Combine(root, clip + ".jpg"), small.EncodeToJPG(90));
            Destroy(full); Destroy(small);
        }
        /// <summary>Plays the scene for its seconds (each frame running the staging) and takes one picture at the end, or as
        /// soon as ready() turns true plus a short beat.</summary>
        private IEnumerator Film(string clip, Action<float> each = null, float seconds = 2.2f, Func<bool> ready = null)
        {
            float start = Time.realtimeSinceStartup, readyAt = -1;
            while (Time.realtimeSinceStartup - start < 12)
            {
                float t = Time.realtimeSinceStartup - start; each?.Invoke(t);
                if (ready != null && readyAt < 0 && ready()) readyAt = t;
                if (ready == null ? t >= seconds : readyAt >= 0 && t - readyAt >= seconds) break;
                yield return null;
            }
            yield return Grab(clip, 0);
            notes.Add("OK " + clip);
        }
        private IEnumerator Load(RunCatalog catalog, RunStorage storage, string hero, int wave, bool shop, string[] roster)
        {
            Arrived = false;
            using (var session = new RunSession(catalog))
            {
                session.Start(hero, 240000 + wave); session.State.Wave = wave;
                session.State.Roster.Clear();
                foreach (string id in roster) if (catalog.Unit(id) != null) session.State.Roster.Add(new RosterEntry { UnitId = id, WeaponId = catalog.Unit(id).Weapon.Id });
                if (shop) { session.State.Phase = SessionPhase.Shop; session.State.Gold = 600; session.Reroll(); session.State.Gold = 300; } // a reroll stocks the stalls
                storage.SaveRun(session.State);
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena");
            yield return new WaitForSecondsRealtime(shop ? 3f : 2.2f);
        }
        private static ArenaController Owner => FindAnyObjectByType<ArenaController>();

        private IEnumerator Start()
        {
            string[] args = Environment.GetCommandLineArgs();
            int at = Array.IndexOf(args, "-capturePath"); root = at >= 0 && at + 1 < args.Length ? Path.GetFullPath(args[at + 1]) : null;
            if (root == null || !RunPaths.Root.Contains("ArcaneBrigadeDiagnostics")) { Application.Quit(1); yield break; }
            Directory.CreateDirectory(root); Application.runInBackground = true;
            var catalog = Resources.Load<RunCatalog>("RunCatalog"); var storage = new RunStorage(RunPaths.Root);
            storage.SaveProfile(new PlayerProfile { BestWave = 40, TutorialSeen = true });
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(.6f);
            string[] team = { "TeapotPilgrim", "HoodWisp", "BrassAcorn", "PillowOracle" };

            cropY = .17f;
            if (Wanted(0))
            {
            // 0: a boss night (the goal)
            yield return Load(catalog, storage, "LanternKnight", 5, false, team);
            yield return new WaitForSecondsRealtime(2.5f);
            yield return Film("clip0", t => ArenaController.ScriptedMove = new Vector2(Mathf.Cos(t * .9f), Mathf.Sin(t * 1.3f) * .6f));
            }
            cropY = .17f;
            if (Wanted(1))
            {
            // 1: just move - the hero walks a loop and everyone fights by themselves
            yield return Load(catalog, storage, "LanternKnight", 4, false, team);
            yield return new WaitForSecondsRealtime(1.5f);
            yield return Film("clip1", t => ArenaController.ScriptedMove = new Vector2(Mathf.Cos(t * 1.2f), Mathf.Sin(t * 1.2f)));
            ArenaController.ScriptedMove = null;
            }
            cropY = .17f;
            if (Wanted(2))
            {
            // 2: the gauge fills and the hero transforms
            yield return Load(catalog, storage, "StormFencer", 4, false, team);
            yield return new WaitForSecondsRealtime(1f);
            { var sim = Owner?.Simulation; if (sim != null) { sim.PrepareMechanicCapture(catalog, 0); } }
            yield return Film("clip2", t => { var sim = Owner?.Simulation; if (sim != null && t > .3f && !sim.HeroAwakening) sim.FillGaugeForCapture(); }, .55f, () => Owner?.Simulation != null && Owner.Simulation.HeroAwakening);
            }
            cropY = .04f;
            if (Wanted(3))
            {
            // 3: the shop - walk to a stall, its card opens, the purchase lands
            yield return Load(catalog, storage, "LanternKnight", 6, true, team);
            yield return Film("clip3", t => WalkTo(OfferKind.Companion), .9f, () => Arrived);
            ArenaController.ScriptedMove = null; bought = false;
            }
            cropY = .04f;
            if (Wanted(4))
            {
            // 4: a merge - buying a friend the brigade already has
            yield return Load(catalog, storage, "LanternKnight", 6, true, team);
            {
                var o = Owner;
                ShopOffer friend = o?.Session?.State.Offers.Find(x => x.Kind == OfferKind.Companion);
                if (friend != null) { o.Session.State.Roster.Add(new RosterEntry { UnitId = friend.Id, WeaponId = catalog.Unit(friend.Id).Weapon.Id }); mergeToken = friend.Token; }
            }
            yield return Film("clip4", t => { WalkTo(OfferKind.Companion); var o = Owner; if (o != null && Arrived && mergeToken >= 0) { o.BuyOffer(mergeToken); mergeToken = -1; merged = true; } }, 1.1f, () => merged);
            ArenaController.ScriptedMove = null;
            }
            cropY = .17f;
            if (Wanted(5))
            {
            // 5: a brigade with a bond awake (four friends of one family)
            string[] family = FamilyOf(catalog);
            yield return Load(catalog, storage, "TinVirtuoso", 4, false, family);
            yield return new WaitForSecondsRealtime(1.5f);
            yield return Film("clip5", t => ArenaController.ScriptedMove = new Vector2(Mathf.Sin(t * .8f) * .5f, Mathf.Cos(t * .8f) * .4f));
            ArenaController.ScriptedMove = null;
            }
            cropY = .04f;
            if (Wanted(6))
            {
            // 6: relics - buying one in the shop
            yield return Load(catalog, storage, "PennyBroker", 8, true, team);
            yield return Film("clip6", t => WalkTo(OfferKind.Item), .9f, () => Arrived);
            ArenaController.ScriptedMove = null;
            }
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-howto-gate") >= 0)
            {
                // review: the hero walks up to the next night's gate in the shop (far, then near: it opens); -howto-gate-wave N picks the map
                int gateWave = Array.IndexOf(Environment.GetCommandLineArgs(), "-howto-gate-wave") is int gw && gw >= 0 && int.TryParse(Environment.GetCommandLineArgs()[gw + 1], out int parsed) ? parsed : 4;
                yield return Load(catalog, storage, "LanternKnight", gateWave, true, team);
                yield return Film("gate-far", t => ArenaController.ScriptedMove = Vector2.zero, .8f);
                ArenaController.ScriptedMove = null;
                yield return Film("gate-near", t => { var o = Owner; if (o?.Simulation != null) o.Simulation.Champion.Position = WorldShop.ExitPosition + new Vector2(1.3f, .3f); }, 2.4f);
                ArenaController.ScriptedMove = null;
            }
            File.WriteAllLines(Path.Combine(root, "Notes.txt"), notes);
            Application.Quit(0);
        }
        /// <summary>Steers the hero to the first stall holding an offer of this kind; Arrived once its card is open.</summary>
        private void WalkTo(OfferKind kind)
        {
            var o = Owner; if (o == null || o.Shop == null || o.Simulation == null) return;
            WorldShop.Station target = null;
            foreach (WorldShop.Station st in o.Shop.Stations) if (st.Offer != null && st.Offer.Kind == kind && !st.Offer.Sold) { target = st; break; }
            if (target == null) { ArenaController.ScriptedMove = null; return; }
            Arrived = o.Shop.Nearby == target;
            Vector2 to = target.Position + Vector2.down * .5f - o.Simulation.Champion.Position;
            ArenaController.ScriptedMove = Arrived || to.sqrMagnitude < .02f ? Vector2.zero : Vector2.ClampMagnitude(to * 2, 1);
        }
        private bool Arrived;
        private bool bought, merged; private int mergeToken = -1; private float cropY = .17f;
        private bool Wanted(int clip) { string[] a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, "-howto-only"); return i < 0 || i + 1 >= a.Length || Array.IndexOf(a[i + 1].Split(','), clip.ToString()) >= 0; }
        private static string[] FamilyOf(RunCatalog c)
        {
            if (c.Bonds != null)
                foreach (BondDefinition bond in c.Bonds)
                {
                    var ids = new List<string>();
                    foreach (UnitDefinition u in c.Companions) if (u.Tags != null && Array.IndexOf(u.Tags, bond.Id) >= 0 && ids.Count < 4) ids.Add(u.Id);
                    if (ids.Count >= 3) return ids.ToArray();
                }
            return new[] { "TeapotPilgrim", "HoodWisp", "BrassAcorn", "PillowOracle" };
        }
    }
}
