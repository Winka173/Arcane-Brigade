using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    /// <summary>Native art integration proof. Uses an isolated save and the requested player resolution.</summary>
    public sealed class ArtMasterCapture : MonoBehaviour
    {
        public static bool Requested => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-art-master") >= 0;
        private string root; private ArenaController owner; private RunCatalog catalog; private bool failed;
        private readonly List<string> checks = new List<string>(); private string[] labels;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (!Requested || FindAnyObjectByType<ArtMasterCapture>() != null) return;
            var go = new GameObject("ArtMasterCapture"); DontDestroyOnLoad(go); go.AddComponent<ArtMasterCapture>();
        }
        private void Check(bool pass, string message)
        {
            failed |= !pass; checks.Add((pass ? "PASS " : "FAIL ") + message);
            File.WriteAllLines(Path.Combine(root, "Checks.txt"), checks);
        }
        private void Log(string message, string trace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
            Check(false, message + "\n" + trace); Application.Quit(1);
        }
        private IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            var frame = ScreenCapture.CaptureScreenshotAsTexture();
            int visible = 0;
            for (int y = 0; y < frame.height; y += 24) for (int x = 0; x < frame.width; x += 24) if (frame.GetPixel(x, y).maxColorComponent > .12f) visible++;
            Check(frame.width == 1920 && frame.height == 1080 && visible > 60, "Actual Windows frame: " + name);
            File.WriteAllBytes(Path.Combine(root, name + ".png"), frame.EncodeToPNG()); Destroy(frame);
            if (name.StartsWith("Boss-", StringComparison.Ordinal))
                File.WriteAllLines(Path.Combine(root, name + "-renderers.tsv"), FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)
                    .Where(v => v.enabled && v.sprite != null).Select(v => v.name + "\t" + v.sprite.texture.name + "\t" + v.sprite.name + "\t" + v.bounds));
        }
        private IEnumerator Load(int wave, bool shop = false)
        {
            using (var run = new RunSession(catalog))
            {
                run.Start("LanternKnight", 171026); run.State.Wave = wave; run.State.Phase = shop ? SessionPhase.Shop : SessionPhase.Combat;
                run.State.Gold = 1200; run.State.Eggs = 20; run.State.Roster.Clear();
                foreach (var u in catalog.Companions.Where(u => !RunSession.StorySpecies(u.Id)).Take(5))
                    run.State.Roster.Add(new RosterEntry { UnitId = u.Id, WeaponId = u.Weapon.Id });
                new RunStorage(RunPaths.Root).SaveRun(run.State);
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(.7f);
            owner = FindAnyObjectByType<ArenaController>();
            Check(owner?.Simulation != null, "Real saved session loads at wave " + wave);
            if (!shop) owner.Simulation.PrepareMienChoCapture(catalog);
            yield return new WaitForSecondsRealtime(3);
        }
        private IEnumerator Start()
        {
            string[] args = Environment.GetCommandLineArgs(); int at = Array.IndexOf(args, "-capturePath");
            if (at < 0 || at + 1 >= args.Length) { Application.Quit(1); yield break; }
            root = Path.GetFullPath(args[at + 1]); Directory.CreateDirectory(root); Application.runInBackground = true;
            Application.logMessageReceived += Log; catalog = Resources.Load<RunCatalog>("RunCatalog");
            bool reduced = Array.IndexOf(args, "-art-reduced") >= 0;
            bool available = Array.IndexOf(args, "-art-available") >= 0;
            Check(RunPaths.Root.Contains("ArcaneBrigadeDiagnostics"), "Real player saves remain isolated");
            var combatArt = CombatVfxArtwork.Current;
            Check(combatArt != null && combatArt.Entries.Length == 62 && combatArt.Entries.Sum(e => e.Frames.Length) == 102 && combatArt.Entries.All(e => e.Frames.All(s => s != null)), "The built player contains all 102 VFX frames in 62 families");
            Check(MechanicEffectArtwork.Current.Form(BuildMorphKind.Porcelain)?.texture.name == "Form_TeaSpill", "The built tea mechanic uses painted amber tea, never broken dishes");
            foreach (string actor in new[] { "ChairPorter", "DoorThatStayedShut", "TheBedside", "LastCustomer_Pleasant", "LastCustomer_BorrowedFace" })
            {
                var entry = ActOneArtwork.Find(actor);
                if (available && actor.StartsWith("LastCustomer_", StringComparison.Ordinal))
                    Check(entry?.Frames.Length == 16 && entry.Frames.Select(s => s.texture).Distinct().Count() == 1, "Available Customer retains its full sixteen-pose atlas; HD art remains pending: " + actor);
                else
                    Check(entry?.Frames.Length == 16 && entry.Frames.Select(s => s.texture).Distinct().Count() == 16 && entry.Frames.All(s => Mathf.Max(s.rect.width, s.rect.height) >= 1024), "Built boss keeps full-resolution pose textures: " + actor);
            }
            new RunStorage(RunPaths.Root).SaveProfile(new PlayerProfile { TutorialSeen = true, BestWave = 40, ReducedMotion = reduced, ReducedEffects = reduced, ReducedFlashes = reduced });
            foreach (int wave in new[] { 1, 7, 12, 17 })
            {
                yield return Load(wave); yield return Shot("Combat-" + wave.ToString("D2"));
                Check(GameObject.Find("ActOne-" + (wave == 1 ? "Prop_BoundaryGate" : wave == 7 ? "Prop_PlatformBench" : wave == 12 ? "Prop_DiningTable" : "Prop_SingleBed")) != null, "Canon zone prop exists in the real arena: " + wave);
                foreach (var visual in FindObjectsByType<UnitVisual>(FindObjectsSortMode.None))
                    if (ActOneArtwork.Has(visual.State?.Definition)) Check(ActOneArtwork.Find(ActOneArtwork.Identity(visual.State.Definition)).Frames.Contains(visual.DisplayedBody), "Live canon actor uses new sheet: " + visual.State.Definition.DisplayName);
                owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = new Vector2(-6, 2);
                FindAnyObjectByType<ArenaCamera>().Snap(); yield return new WaitForSecondsRealtime(.25f);
                yield return Shot("Zone-props-" + wave.ToString("D2"));
            }
            yield return Load(12);
            var explosion = FindAnyObjectByType<ExplosionFeedback>();
            explosion.Observe(new CombatEvent(CombatEventKind.Detonation, new Vector2(-3, 0), Faction.Companion, 1.6f), false);
            explosion.Observe(new CombatEvent(CombatEventKind.Detonation, new Vector2(3, 0), Faction.Enemy, 1.6f), false);
            yield return Shot("Painted-blasts-both-factions");
            Check(!FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None).Any(v => v.enabled && v.sprite != null && v.sprite.texture.name.StartsWith("HostileBlast", StringComparison.Ordinal)), "A real hostile detonation is drawn in code, not painted ember art (owner 22/10/2026)");
            var fields = (IList<ExpandedField>)owner.Simulation.ExpandedFields; fields.Clear();
            var source = owner.Simulation.Units.First(u => u.Alive && u.Faction == Faction.Enemy);
            int hazard = 0;
            foreach (var kind in new[] { ExpandedFieldKind.OfficeHazard, ExpandedFieldKind.Curtain, ExpandedFieldKind.ToyHazard })
                fields.Add(new ExpandedField { Kind = kind, Source = source, Position = new Vector2(-4 + hazard++ * 4, -1), Radius = 1.4f, Created = owner.Simulation.Elapsed, At = owner.Simulation.Elapsed + 20, Until = owner.Simulation.Elapsed + 40 });
            fields.Add(new ExpandedField { Kind = ExpandedFieldKind.Tentacle, Source = source, Start = new Vector2(-4, 2.5f), Position = new Vector2(4, 2.5f), Radius = .35f, Created = owner.Simulation.Elapsed, At = owner.Simulation.Elapsed + 20, Until = owner.Simulation.Elapsed + 40 });
            fields.Add(new ExpandedField { Kind = ExpandedFieldKind.ShadowPool, Source = source, Position = new Vector2(-2, -3.2f), Radius = 1.1f, Created = owner.Simulation.Elapsed, At = owner.Simulation.Elapsed + 20, Until = owner.Simulation.Elapsed + 40 });
            fields.Add(new ExpandedField { Kind = ExpandedFieldKind.Shockwave, Source = source, Position = new Vector2(2.5f, -3.2f), Radius = 1.6f, Created = owner.Simulation.Elapsed, At = owner.Simulation.Elapsed + 20, Until = owner.Simulation.Elapsed + 40 });
            yield return new WaitForSecondsRealtime(.1f); yield return Shot("Painted-hazards-warning");
            foreach (var field in fields) field.At = owner.Simulation.Elapsed - .1f;
            yield return new WaitForSecondsRealtime(.1f); yield return Shot("Painted-hazards-active");
            Check(FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None).Any(v => v.enabled && v.sprite != null && v.sprite.texture.name == "OfficeHazard_f1_v1"), "The live office hazard has a visible authored impact");
            fields.Clear();
            foreach (int wave in new[] { 10, 15, 20 })
            {
                yield return Load(wave); yield return Shot("Boss-" + wave);
                Check(owner.Simulation.Boss != null && ActOneArtwork.Has(owner.Simulation.Boss.Definition), "Actual story boss selects authored art: " + wave);
                if (wave == 20) { yield return new WaitForSecondsRealtime(2.3f); yield return Shot("Customer-second-mask"); }
            }
            yield return Load(13, true); yield return Shot("Shop-kind-eyes");
            var merchantFrames = Resources.Load<PaintedAnimationLibrary>("PaintedAnimationLibrary").GiantGoldMerchant;
            var merchant = FindObjectsByType<ShopDisplayActor>(FindObjectsSortMode.None).FirstOrDefault(a => a.View != null && merchantFrames.Contains(a.View.sprite));
            Check(merchant != null, "The actual shopkeeper uses the gentle new art");
            if (merchant != null) { merchant.Celebrate(); yield return new WaitForSecondsRealtime(.25f); Check(merchantFrames.Contains(merchant.View.sprite), "A purchase gesture cannot restore the old monster handoff"); yield return Shot("Shop-gentle-handoff"); }
            var journal = owner.GetComponent<MienChoPresentation>(); journal.Show(12); yield return Shot("Journal-sewn-letter"); journal.Close();
            yield return Load(16, true); journal = owner.GetComponent<MienChoPresentation>(); journal.Show(ActOneCanon.HobChapter); yield return Shot("Journal-Hob"); journal.Close();
            WaitingCampaign.Ensure(owner.Session.State); owner.Session.State.WaitingCampaign.Room = 2; new RunStorage(RunPaths.Root).SaveRun(owner.Session.State);
            SceneManager.LoadScene("WaitingRoom"); yield return new WaitForSecondsRealtime(1.5f); yield return Shot("Hob-sitting");
            var room = FindAnyObjectByType<WaitingRoomController>(); room.DiagnosticPull(WaitingRoomController.PullSeconds + .1f);
            yield return new WaitForSecondsRealtime(.9f); Check(room.Rescued, "Hob can leave the illustrated chair"); yield return Shot("Hob-standing");
            var profile = new RunStorage(RunPaths.Root).LoadProfile(); profile.NightmareShopClosed = true; new RunStorage(RunPaths.Root).SaveProfile(profile);
            SceneManager.LoadScene("RunFrontend"); yield return new WaitForSecondsRealtime(2); yield return Shot("Menu-chair-turned");
            yield return Gallery();
            File.WriteAllText(Path.Combine(root, "Hardware.txt"), SystemInfo.graphicsDeviceName + "\n" + SystemInfo.processorType + "\nReduced: " + reduced + "\n");
            Application.Quit(failed ? 1 : 0);
        }
        private IEnumerator Gallery()
        {
            foreach (var behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)) if (behaviour != this) behaviour.enabled = false;
            foreach (var render in FindObjectsByType<Renderer>(FindObjectsSortMode.None)) render.enabled = false;
            foreach (var camera in FindObjectsByType<Camera>(FindObjectsSortMode.None)) camera.enabled = false;
            var cameraObject = new GameObject("IllustrationReviewCamera"); var review = cameraObject.AddComponent<Camera>();
            review.orthographic = true; review.orthographicSize = 6; review.transform.position = new Vector3(0, 0, -10);
            review.clearFlags = CameraClearFlags.SolidColor; review.backgroundColor = new Color(.055f, .065f, .08f);
            var slots = new SpriteRenderer[16];
            for (int i = 0; i < slots.Length; i++) { var go = new GameObject("ReviewedArtwork"); slots[i] = go.AddComponent<SpriteRenderer>(); }
            var all = new List<(string name, Sprite image)>();
            foreach (var entry in ActOneArtwork.Current.Entries) all.Add((entry.Name, entry.Frames[0]));
            foreach (var entry in CombatVfxArtwork.Current.Entries) all.Add((entry.Name, entry.Frames[0]));
            foreach (var entry in ActOneArtwork.Current.Entries.Where(e => e.Motion != null))
                for (int i = 1; i < entry.Frames.Length; i++) all.Add((entry.Name + " pose " + i, entry.Frames[i]));
            for (int start = 0; start < all.Count; start += 16)
            {
                labels = new string[16];
                for (int i = 0; i < slots.Length; i++)
                {
                    var slot = slots[i]; slot.enabled = start + i < all.Count; if (!slot.enabled) continue;
                    var entry = all[start + i]; labels[i] = entry.name; slot.sprite = entry.image;
                    Rect ink = SpriteInkMetrics.BoundsOf(entry.image);
                    slot.transform.localScale = Vector3.one * Mathf.Min(4 / Mathf.Max(.01f, ink.width), 2.15f / Mathf.Max(.01f, ink.height));
                    slot.transform.position = new Vector3(-8 + i % 4 * 5.3f, 4.3f - i / 4 * 2.8f, 0) - slot.transform.TransformVector(ink.center);
                }
                yield return Shot("Gallery-" + (start / 16).ToString("D2"));
            }
        }
        private void OnGUI()
        {
            if (labels == null) return;
            var previous = GUI.matrix; GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 1920f, Screen.height / 1080f, 1));
            var style = new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            for (int i = 0; i < labels.Length; i++) if (labels[i] != null) LocalizedGUI.Label(new Rect(i % 4 * 480, 251 + i / 4 * 252, 480, 40), labels[i], style);
            GUI.matrix = previous;
        }
    }
}
