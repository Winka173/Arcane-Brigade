using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    /// <summary>Opt-in isolated diagnostic: renders the production UnitVisual, forms and real arena.</summary>
    public sealed class BuildMorphCapture : MonoBehaviour
    {
        public static bool Requested => RunPaths.K7Capture && Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-morph-capture") >= 0;
        private readonly List<string> notes = new List<string>();
        private readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();
        private readonly List<UnitVisual> views = new List<UnitVisual>();
        private string root, title;
        private bool failed, reduced, gallery;
        private int shown;
        private CombatSimulation displayed;
        private BuildMorphVisual effects;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (!Requested || FindAnyObjectByType<BuildMorphCapture>() != null) return;
            var go = new GameObject("BuildMorphNativeCapture"); DontDestroyOnLoad(go); go.AddComponent<BuildMorphCapture>();
        }
        private T Copy<T>(T source) where T : UnityEngine.Object { T copy = Instantiate(source); owned.Add(copy); return copy; }
        private void Check(bool value, string text)
        {
            notes.Add((value ? "PASS " : "FAIL ") + text); failed |= !value;
            File.WriteAllLines(Path.Combine(root, "Native_Checks.txt"), notes);
        }
        private void Log(string text, string trace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
            Check(false, text); Application.Quit(1);
        }
        private IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            Texture2D screen = ScreenCapture.CaptureScreenshotAsTexture();
            Check(screen.width == 1920 && screen.height == 1080, "Native render 1920x1080: " + name);
            Color32[] pixels = screen.GetPixels32(); int bright = 0;
            for (int i = 0; i < pixels.Length; i += 157) if (pixels[i].r + pixels[i].g + pixels[i].b > 180) bright++;
            Check(bright > 100, "Nonblank production render: " + name);
            File.WriteAllBytes(Path.Combine(root, name + ".png"), screen.EncodeToPNG()); Destroy(screen);
        }
        private IEnumerator Start()
        {
            string[] args = Environment.GetCommandLineArgs(); int index = Array.IndexOf(args, "-capturePath");
            if (index < 0 || index + 1 >= args.Length) { Application.Quit(1); yield break; }
            root = Path.GetFullPath(args[index + 1]); Directory.CreateDirectory(root);
            reduced = Array.IndexOf(args, "-arcane-morph-reduced") >= 0;
            Application.runInBackground = true; Application.logMessageReceived += Log;
            yield return new WaitForSecondsRealtime(2);
            RunCatalog c = Resources.Load<RunCatalog>("RunCatalog");
            var storage = new RunStorage(RunPaths.Root);
            var captureProfile = new PlayerProfile { ReducedEffects = reduced, ReducedMotion = reduced, ReducedFlashes = reduced, HighReadability = reduced };
            storage.SaveProfile(captureProfile); PresentationSettings.Apply(captureProfile);
            Scene old = SceneManager.GetActiveScene(); Scene room = SceneManager.CreateScene("BuildFormPortraits"); SceneManager.SetActiveScene(room);
            yield return SceneManager.UnloadSceneAsync(old);
            var cameraGo = new GameObject("FormPortraitCamera"); Camera camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true; camera.orthographicSize = 3.6f; camera.transform.position = new Vector3(0, .3f, -10);
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.065f,.055f,.1f);
            effects = new GameObject("ProductionFormEffects").AddComponent<BuildMorphVisual>(); gallery = true;
            for (shown = 0; shown < BuildMorphs.All.Length; shown++)
            {
                BuildMorphDefinition d = BuildMorphs.All[shown]; title = d.Name;
                WeaponDefinition weapon = c.Weapons.First(w => !w.SpecialOnly && BuildMorphs.For(w) == d.Kind);
                CheckpointCatalog arena = Copy(c.Arena);
                arena.Champion = Copy(c.Champions[0]); arena.Champion.Weapon = Copy(weapon);
                arena.Champion.BuildMorph = d.Kind; arena.Champion.BuildMorphStage = 3;
                arena.Champion.HeroMechanic = HeroMechanic.None; arena.Champion.Passive = PassiveKind.None;
                arena.Boss = null; arena.ExplicitStartingRoster = true; arena.StartingCompanions = Array.Empty<UnitDefinition>();
                arena.EnemyBudget = 0; arena.RunWave = 2; arena.WaveDuration = 1000; arena.Procs = Array.Empty<ActiveProc>();
                arena.EnableArenaToys = arena.EnableHeroMechanics = arena.EnableSpectacles = arena.EnableBossArenaPhase = false;
                arena.RelicStacks = Array.Empty<int>(); arena.RelicWeapons = Array.Empty<UnitDefinition>();
                displayed = new CombatSimulation(arena); displayed.Start(8015);
                for (int stage = 1; stage <= 3; stage++)
                {
                    UnitDefinition unit = Copy(c.Companions.First(u => !u.SpecialRecruit && u.Body != null && u.Material == BodyMaterial.Fabric));
                    unit.Weapon = Copy(weapon); unit.BuildMorph = d.Kind; unit.BuildMorphStage = stage; unit.CombatTier = 1; unit.BodyHeight = 1.5f;
                    UnitState state = displayed.MorphGalleryUnit(unit, new Vector2((stage - 2) * 4.1f, -.05f));
                    UnitVisual view = Instantiate(c.Arena.UnitPrefab).GetComponent<UnitVisual>(); view.ReducedMotion = reduced; view.Bind(state, arena); views.Add(view);
                }
                yield return new WaitForSecondsRealtime(.2f);
                Check(views.All(v => v.GetComponent<BuildMorphAppearance>()?.PartCount == (v.State.Definition.BuildMorphStage == 1 ? 2 : v.State.Definition.BuildMorphStage == 2 ? 5 : 9)), "All three silhouettes use production attachments: " + d.Kind);
                yield return Shot("form-" + ((int)d.Kind).ToString("D2") + "-stages");
                foreach (UnitVisual view in views) Destroy(view.gameObject); views.Clear(); displayed = null;
                foreach (UnityEngine.Object value in owned) Destroy(value); owned.Clear();
                yield return null;
            }
            gallery = false; displayed = null;
            for (int k = 0; k < BuildMorphs.All.Length; k++)
            {
                BuildMorphDefinition d = BuildMorphs.All[k];
                WeaponDefinition weapon = c.Weapons.First(w => !w.SpecialOnly && BuildMorphs.For(w) == d.Kind);
                using (var s = new RunSession(c))
                {
                    s.Start("LanternKnight", 8015 + k); s.State.Roster.Clear(); s.State.Items.Clear(); s.State.Wave = 3;
                    s.State.Roster.Add(new RosterEntry { UnitId = c.Companions.First(u => !u.SpecialRecruit && u.Body != null).Id, WeaponId = weapon.Id, Tier = 3 });
                    foreach (ItemDefinition item in c.Items.Where(i => !i.Id.StartsWith("Secret") && BuildMorphs.Affinity(d.Kind, i, true)).Take(6))
                        s.State.Items.Add(new ItemStack { Id = item.Id, Count = 1 });
                    Check(s.MorphProfile(weapon, 3, true).Stage == 3, "Saved equipment opens actual masterwork: " + d.Kind);
                    storage.SaveRun(s.State);
                }
                RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return null; yield return new WaitForSecondsRealtime(.6f);
                ArenaController owner = FindAnyObjectByType<ArenaController>();
                Check(owner.Simulation.Units.Any(u => u.Faction == Faction.Companion && u.Definition.BuildMorph == d.Kind && u.Definition.BuildMorphStage == 3), "Real arena compiles correct form: " + d.Kind);
                owner.Simulation.MorphCaptureArena();
                float deadline = Time.realtimeSinceStartup + 12;
                while (owner.Simulation.BuildMorphReleases == 0 && Time.realtimeSinceStartup < deadline) yield return null;
                Check(owner.Simulation.BuildMorphReleases > 0 && owner.Simulation.MorphStrikes.Count > 0, "Native automatic attack releases choreography: " + d.Kind);
                yield return new WaitForSecondsRealtime(.2f);
                yield return Shot("combat-" + ((int)d.Kind).ToString("D2"));
                if (k == 0)
                {
                    owner.ToggleDetails(); owner.Details.SelectTab(3); yield return null;
                    yield return Shot("details-form-progression"); owner.ToggleDetails();
                }
            }
            Check(!failed, "Native form capture completed"); Application.Quit(failed ? 1 : 0);
        }
        private void Update()
        {
            if (!gallery || displayed == null) return;
            foreach (UnitVisual view in views) view.Display(Time.unscaledTime, 1);
            effects.Display(displayed, reduced);
        }
        private void OnGUI()
        {
            if (!gallery) return;
            Matrix4x4 old = GUI.matrix; GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 1920f, Screen.height / 1080f, 1));
            GUI.Label(new Rect(80, 70, 1700, 80), title, ArcadeTypography.Style(48, true));
            GUI.Label(new Rect(80, 150, 1700, 50), "BUILD FORMS / PRODUCTION CHARACTER RENDER / " + (reduced ? "REDUCED EFFECTS" : "STANDARD"), ArcadeTypography.Style(24));
            for (int i = 0; i < 3; i++)
            {
                float x = 195 + i * 610;
                GUI.Label(new Rect(x, 820, 540, 60), BuildMorphs.StageName(i + 1).ToUpperInvariant(), ArcadeTypography.Style(32, true));
                GUI.Label(new Rect(x, 880, 540, 90), new[] { "Weapon charm + stitched clasp", "Distinct back attachment", "Full silhouette + orbiting motifs" }[i], ArcadeTypography.Style(22));
            }
            GUI.matrix = old;
        }
        private void OnDestroy() { Application.logMessageReceived -= Log; }
    }

    public sealed partial class CombatSimulation
    {
        public UnitState MorphGalleryUnit(UnitDefinition definition, Vector2 position)
        {
            if (!BuildMorphCapture.Requested) throw new InvalidOperationException("Isolated form capture required");
            return Spawn(definition, Faction.Companion, position);
        }
        public void MorphCaptureArena()
        {
            if (!BuildMorphCapture.Requested) throw new InvalidOperationException("Isolated form capture required");
            catalog.EnemyBudget = 0; Champion.NextAttack = float.PositiveInfinity;
            Champion.Health = Champion.Definition.Health = 100000;
            foreach (UnitState source in units) if (source.Faction == Faction.Companion) source.Definition.Weapon.Cooldown = .35f;
            for (int i = 0; i < 14; i++)
            {
                Vector2 at = Champion.Position + new Vector2(2.4f + i % 4 * .48f, -.85f + i / 4 * .52f);
                UnitState enemy = Spawn(catalog.MeleeEnemy, Faction.Enemy, at); enemy.Health = 10000;
                enemy.Definition.Speed = 0; enemy.NextAttack = enemy.NextContact = float.PositiveInfinity;
            }
            spatial.Rebuild(units);
        }
    }
}
