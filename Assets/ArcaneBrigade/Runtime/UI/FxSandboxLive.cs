using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    /// <summary>The effects sandbox's LIVE FIGHT (owner request 29/10/2026): the real wave-12 fight of the bond showcase
    /// (<see cref="RulesInkCapture"/>): a team of exactly N friends of one bond, only that bond firing (<see cref="BondRuntime.OnlyBond"/>),
    /// a crowd of real enemies that keeps coming back and a wave that never ends, so every trait, moment, weather, element state,
    /// combo, death, damage number and status word plays through the real game code. The hero and friends cannot fall. The run is
    /// written to a throwaway folder (<see cref="RunPaths.SandboxRoot"/>), never to the real save; leaving returns to the sandbox.</summary>
    [DefaultExecutionOrder(-200)]
    public sealed class FxSandboxLive : MonoBehaviour
    {
        public static readonly string[] Houses = { "Storm", "Windborne", "Ember", "Hearth", "Frost", "Tide", "Garden", "Venom", "Starlight", "Dusk", "Carnival", "Court", "Archive", "Lantern", "Stone", "Clockwork", "Bulwark", "Spirit", "Melody", "Gravity" };
        public static readonly string[] CrowdNames = { "LIGHT", "NORMAL", "CROWDED" };
        public static readonly int[] CrowdSizes = { 12, 26, 44 };
        public const int Wave = 12;
        // the last picks, shared with the sandbox's LIVE FIGHT tab
        public static string House = "Storm";
        public static int Step = 6, Crowd = 1;
        public static bool Slow;

        public static FxSandboxLive Current { get; private set; }
        public bool Paused { get; private set; }
        public bool InFight => ready && owner != null && owner.Simulation != null;
        public ArenaController Owner => owner;
        public int Hits { get; private set; }
        public int Kills { get; private set; }
        public int Restarts { get; private set; }
        public string Failure { get; private set; }
        public readonly SortedDictionary<string, int> Rules = new SortedDictionary<string, int>();

        private ArenaController owner;
        private CheckpointCatalog fightCatalog;
        private RunCatalog catalog;
        private PlayerProfile profile;
        private string folder;
        private bool ready, loading, leaving;
        private float nextTopUp;
        private string lastRule;
        private BondRuntime hooked;
        private static readonly MethodInfo spawn = typeof(CombatSimulation).GetMethod("Spawn", BindingFlags.NonPublic | BindingFlags.Instance, null,
            new[] { typeof(UnitDefinition), typeof(Faction), typeof(Vector2), typeof(bool), typeof(bool) }, null);
        private static readonly FieldInfo simCatalog = typeof(CombatSimulation).GetField("catalog", BindingFlags.NonPublic | BindingFlags.Instance);

        // ---- what can run ---------------------------------------------------------------------------------------------------
        private static readonly Dictionary<string, int> friendCount = new Dictionary<string, int>();
        /// <summary>How many friends carry a bond (the team the showcase can field).</summary>
        public static int Friends(string house)
        {
            if (friendCount.TryGetValue(house, out int n)) return n;
            var cat = Resources.Load<RunCatalog>("RunCatalog"); string bond = BondV3Catalog.BondId(house) ?? house;
            n = cat == null ? 0 : cat.Companions.Count(u => u != null && u.Body != null && !u.SpecialRecruit && (BondV3Catalog.Tags(u.Id).Contains(bond) || BondV3Catalog.Tags(u.Id).Contains(house)));
            friendCount[house] = n; return n;
        }
        private static readonly Dictionary<string, int[]> stepCache = new Dictionary<string, int[]>();
        /// <summary>The steps a bond has: 2 (its threshold) plus the counts of its own steps.</summary>
        public static int[] Steps(string house)
        {
            if (stepCache.TryGetValue(house, out int[] s)) return s;
            var list = new List<int>(RulesInkCapture.StepCounts(house)); if (!list.Contains(2)) list.Insert(0, 2);
            return stepCache[house] = list.Distinct().OrderBy(c => c).ToArray();
        }
        /// <summary>The team that switches a step on: step 2 needs the bond's threshold of friends, the others their own count.</summary>
        public static int Team(string house, int step)
        {
            if (step != 2) return step;
            string id = BondV3Catalog.BondId(house) ?? house;
            foreach (BondDefinition bond in BondTuning.V3Definitions) if (bond != null && (bond.Id == id || bond.Name == house)) return Mathf.Max(2, bond.Threshold);
            return 2;
        }
        /// <summary>Why the last fight could not start (shown on the sandbox's LIVE FIGHT tab).</summary>
        public static string LastFailure;
        public static bool CanRun(string house, int step) => Array.IndexOf(Steps(house), step) >= 0 && Friends(house) >= Team(house, step);
        /// <summary>The step itself when the bond can run it, otherwise the nearest one it can.</summary>
        public static int Fit(string house, int step)
        {
            if (CanRun(house, step)) return step;
            int best = 0; foreach (int s in Steps(house)) if (CanRun(house, s) && (best == 0 || Mathf.Abs(s - step) < Mathf.Abs(best - step))) best = s;
            return best > 0 ? best : step;
        }

        // ---- start / restart / leave ------------------------------------------------------------------------------------------
        public static FxSandboxLive Begin(string house, int step)
        {
            House = house; Step = Fit(house, step);
            if (Current == null) { var go = new GameObject("FxSandboxLive"); DontDestroyOnLoad(go); go.AddComponent<FxSandboxLive>(); }
            Current.Restart(); return Current;
        }
        /// <summary>Another bond or step: the fight starts again with the new team.</summary>
        public void Configure(string house, int step) { House = house; Step = Fit(house, step); Restart(); }

        private void Awake() { Current = this; SceneManager.sceneLoaded += OnScene; }
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnScene; Unhook();
            if (Current == this) { Cleanup(); Current = null; }
        }

        public void Restart()
        {
            if (loading || leaving) return;
            Failure = LastFailure = null;
            if (FxSandbox.Current != null) FxSandbox.Current.Close();
            catalog = catalog != null ? catalog : Resources.Load<RunCatalog>("RunCatalog");
            if (catalog == null) { Failure = "no RunCatalog"; return; }
            if (RunPaths.SandboxRoot == null)
            {
                // the player's own settings (read only), copied into a throwaway save folder for the fight
                profile = new RunStorage(RunPaths.Root).LoadProfile();
                folder = Path.Combine(Application.temporaryCachePath, "ArcaneBrigadeFxSandbox", Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(folder);
                RunPaths.SandboxRoot = folder;
            }
            var storage = new RunStorage(RunPaths.SandboxRoot);
            if (profile == null) profile = new PlayerProfile();
            profile.ShowDamageNumbers = true; profile.TutorialSeen = true; // the sandbox always shows the numbers
            storage.SaveProfile(profile);
            BondRuntime.OnlyBond = BondV3Catalog.BondId(House);
            RulesInkCapture.SaveHouseRun(catalog, storage, House, Wave, 290000 + Restarts * 7, Team(House, Step));
            Restarts++;
            Unhook(); ready = false; owner = null; fightCatalog = null; Hits = Kills = 0; Rules.Clear(); lastRule = null;
            Time.timeScale = 1;
            RunLaunch.Resume = true; loading = true;
            SceneManager.LoadScene("RunArena");
            StartCoroutine(WaitForFight());
        }

        private IEnumerator WaitForFight()
        {
            for (float t = 0; t < 12 && (owner == null || owner.Simulation == null || owner.Simulation.Phase != RunPhase.Combat); t += .1f)
            { yield return new WaitForSecondsRealtime(.1f); owner = FindAnyObjectByType<ArenaController>(); }
            loading = false; RunLaunch.Resume = false;
            if (owner == null || owner.Simulation == null || owner.Simulation.Phase != RunPhase.Combat) { Failure = LastFailure = "The fight did not start (" + House + " step " + Step + ")"; Leave(); yield break; }
            CombatSimulation sim = owner.Simulation;
            fightCatalog = simCatalog?.GetValue(sim) as CheckpointCatalog ?? owner.Session?.CombatCatalog;
            if (fightCatalog != null) { fightCatalog.WaveDuration = 1e6f; fightCatalog.EnemyBudget = CrowdSizes[Crowd]; } // the wave never ends
            if (sim.Bonds != null) { hooked = sim.Bonds; hooked.RuleTriggered += OnRule; hooked.MomentRequested += OnMoment; }
            ready = true; nextTopUp = 0;
            TopUp(CrowdSizes[Crowd]);
        }
        private void OnRule(BondRuleEvent e) { Rules[e.Rule] = Rules.TryGetValue(e.Rule, out int n) ? n + 1 : 1; lastRule = e.Rule; }
        private void OnMoment(BondMomentRequest m) { string key = "moment " + m.Rule; Rules[key] = Rules.TryGetValue(key, out int n) ? n + 1 : 1; lastRule = m.Rule; }
        private void Unhook() { if (hooked != null) { hooked.RuleTriggered -= OnRule; hooked.MomentRequested -= OnMoment; } hooked = null; }

        /// <summary>Back to the sandbox (the COLLECTION page), the real save untouched.</summary>
        public void Leave()
        {
            if (leaving) return;
            leaving = true; Cleanup();
            SceneManager.LoadScene("RunFrontend");
        }
        private void Cleanup()
        {
            Unhook(); ready = false; owner = null; fightCatalog = null;
            Time.timeScale = 1; BondRuntime.OnlyBond = null; RunLaunch.Resume = false; RunPaths.SandboxRoot = null; Paused = false;
        }
        private void OnScene(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "RunArena" && !leaving) return;
            // left the fight (BACK, or the arena's own menu): the real save is used again before the menu reads it
            Cleanup();
            if (scene.name == "RunFrontend") StartCoroutine(BackToSandbox()); else Finish();
        }
        private IEnumerator BackToSandbox()
        {
            RunMenu menu = null;
            for (float t = 0; t < 10 && menu == null; t += .1f) { yield return new WaitForSecondsRealtime(.1f); menu = FindAnyObjectByType<RunMenu>(); }
            yield return new WaitForSecondsRealtime(.2f);
            if (menu != null) { menu.ShowPage(RunMenu.Page.Museum); FxSandbox box = FxSandbox.Open(); box.ShowLiveTab(); }
            Finish();
        }
        private void Finish()
        {
            try { if (folder != null && Directory.Exists(folder)) Directory.Delete(folder, true); } catch (Exception) { }
            Destroy(gameObject);
        }

        // ---- the fight ------------------------------------------------------------------------------------------------------------
        public void SetPaused(bool on) { Paused = on; }
        public void SetSlow(bool on) { Slow = on; }
        public void SetCrowd(int index) { Crowd = Mathf.Clamp(index, 0, CrowdSizes.Length - 1); if (fightCatalog != null) fightCatalog.EnemyBudget = CrowdSizes[Crowd]; }
        public int EnemiesAlive => InFight ? owner.Simulation.Units.Count(u => u.Faction == Faction.Enemy && u.Alive) : 0;

        private void FixedUpdate()
        {
            if (!InFight) return;
            CombatSimulation sim = owner.Simulation;
            // runs before the arena's step: the hero and the friends cannot fall; the last step's blows are counted
            foreach (UnitState u in sim.Units) if (u.Faction != Faction.Enemy && u.Alive) u.Health = Mathf.Max(u.Health, MortalityRuntime.MaxHealth(u));
            foreach (CombatEvent e in sim.Events)
                if (e.Faction == Faction.Enemy) { if (e.Kind == CombatEventKind.Hit) Hits++; else if (e.Kind == CombatEventKind.Kill) Kills++; }
        }

        private void Update()
        {
            if (!InFight) return;
            CombatSimulation sim = owner.Simulation;
            if (Input.GetKeyDown(KeyCode.P)) Paused = !Paused;
            if (Input.GetKeyDown(KeyCode.H)) panelHidden = !panelHidden;
            Time.timeScale = Paused ? 0 : Slow ? .25f : 1;
            if (sim.Phase == RunPhase.Victory || sim.Phase == RunPhase.Defeat) { Restart(); return; } // never a wave end; should not happen
            if (Time.unscaledTime >= nextTopUp && !Paused) { nextTopUp = Time.unscaledTime + .25f; TopUp(3); }
        }
        private void LateUpdate() { if (InFight && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1))) Leave(); }

        /// <summary>Real wave enemies step in around the hero until the crowd is full: one in three a little tougher so the bond's
        /// moments find a body to land on, the rest ordinary, so kills and their deaths keep coming.</summary>
        private void TopUp(int most)
        {
            CombatSimulation sim = owner.Simulation; if (spawn == null || sim.Phase != RunPhase.Combat) return;
            int want = CrowdSizes[Crowd] - EnemiesAlive; if (want <= 0) return;
            UnitDefinition[] kinds = (fightCatalog != null && fightCatalog.EnemyPool.Length > 0 ? fightCatalog.EnemyPool : catalog.Enemies).Where(u => u != null && !u.IsBoss).ToArray();
            if (kinds.Length == 0) return;
            Vector2 half = (fightCatalog != null ? fightCatalog.ArenaSize : new Vector2(22, 11)) * .5f - Vector2.one;
            for (int i = 0; i < want && i < most; i++)
            {
                int k = UnityEngine.Random.Range(0, 1 << 20);
                float ang = UnityEngine.Random.value * 6.283f, dist = 2.6f + UnityEngine.Random.value * 4f;
                Vector2 at = sim.Champion.Position + new Vector2(Mathf.Cos(ang) * dist, Mathf.Sin(ang) * dist * .7f);
                at = new Vector2(Mathf.Clamp(at.x, -half.x, half.x), Mathf.Clamp(at.y, -half.y, half.y));
                UnitDefinition kind = kinds[k % kinds.Length];
                try
                {
                    var enemy = spawn.Invoke(sim, new object[] { kind, Faction.Enemy, at, false, true }) as UnitState;
                    if (enemy != null && k % 3 == 0) enemy.Health *= 4;
                }
                catch (Exception problem) { Failure = "spawn: " + (problem.InnerException ?? problem).Message; return; }
            }
        }

        // ---- panel ------------------------------------------------------------------------------------------------------------------
        private GUIStyle small, btn, on; private Vector2 scroll; private bool panelHidden;
        private void OnGUI()
        {
            if (!InFight && !loading) return;
            GUI.depth = -60;
            GUISkin previousSkin = GUI.skin; GUI.skin = ArcadeSkin.Skin;
            if (small == null)
            {
                small = new GUIStyle(GUI.skin.button) { fontSize = 13, alignment = TextAnchor.MiddleLeft, wordWrap = false };
                btn = new GUIStyle(GUI.skin.button) { fontSize = 13, font = ArcadeTypography.Heading };
                on = new GUIStyle(btn); on.normal.textColor = on.hover.textColor = new Color(1f, .85f, .3f);
            }
            Matrix4x4 previous = ArcadeTypography.BeginViewport(new Rect(0, 0, Screen.width, Screen.height), 1280, 720);
            if (panelHidden) { GUI.Label(new Rect(12, 108, 300, 20), "LIVE FIGHT · H: show the panel", ArcadeTypography.Style(12)); GUI.matrix = previous; GUI.skin = previousSkin; return; }
            float x = 12, w = 236; // left, under the hero's bars (the bond list sits top right)
            GUI.DrawTexture(new Rect(x - 6, 104, w + 12, 552), Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, new Color(.04f, .035f, .06f, .86f), 0, 0);
            GUI.Label(new Rect(x, 100, w, 26), "LIVE FIGHT", ArcadeTypography.Style(17, true));
            GUI.Label(new Rect(x, 124, w, 20), House + " · step " + Step + " (" + Team(House, Step) + " friends)" + (loading ? " · loading" : ""), ArcadeTypography.Style(12));
            Rect view = new Rect(x, 146, w, 196);
            scroll = GUI.BeginScrollView(view, scroll, new Rect(0, 0, w - 18, Houses.Length * 22));
            for (int i = 0; i < Houses.Length; i++)
            {
                string h = Houses[i]; bool any = Steps(h).Any(s => CanRun(h, s));
                GUI.enabled = any && !loading;
                if (GUI.Button(new Rect(0, i * 22, w - 20, 20), h + "  (" + Friends(h) + ")", h == House ? on : small) && h != House) Configure(h, Step);
            }
            GUI.enabled = true; GUI.EndScrollView();
            float y = 348; float bw = (w - 9) / 4f;
            int[] steps = { 2, 4, 6, 8 };
            for (int i = 0; i < 4; i++)
            {
                GUI.enabled = CanRun(House, steps[i]) && !loading;
                if (GUI.Button(new Rect(x + i * (bw + 3), y, bw, 28), "STEP " + steps[i], Step == steps[i] ? on : btn)) Configure(House, steps[i]);
            }
            GUI.enabled = true; y += 32; float cw = (w - 6) / 3f;
            for (int i = 0; i < 3; i++) if (GUI.Button(new Rect(x + i * (cw + 3), y, cw, 28), CrowdNames[i], Crowd == i ? on : btn)) SetCrowd(i);
            y += 32; float hw = (w - 6) / 3f;
            if (GUI.Button(new Rect(x, y, hw, 28), "1x", !Slow ? on : btn)) Slow = false;
            if (GUI.Button(new Rect(x + hw + 3, y, hw, 28), "0.25x", Slow ? on : btn)) Slow = true;
            if (GUI.Button(new Rect(x + 2 * (hw + 3), y, hw, 28), "PAUSE", Paused ? on : btn)) Paused = !Paused;
            y += 32; float half = (w - 3) / 2f;
            GUI.enabled = !loading;
            if (GUI.Button(new Rect(x, y, half, 30), "RESTART", btn)) Restart();
            if (GUI.Button(new Rect(x + half + 3, y, half, 30), "BACK", btn)) Leave();
            GUI.enabled = true; y += 36;
            if (InFight)
            {
                GUI.Label(new Rect(x, y, w, 20), "enemies " + EnemiesAlive + " · hits " + Hits + " · kills " + Kills, ArcadeTypography.Style(12)); y += 20;
                GUI.Label(new Rect(x, y, w, 20), "last rule: " + (lastRule ?? "-"), ArcadeTypography.Style(12)); y += 20;
                GUI.Label(new Rect(x, y, w, 40), Rules.Count + " rules fired · " + string.Join(", ", Rules.Take(4).Select(r => r.Key)), ArcadeTypography.Style(11)); y += 40;
            }
            if (Failure != null) { GUI.Label(new Rect(x, y, w, 20), Failure, ArcadeTypography.Style(12)); y += 20; }
            GUI.Label(new Rect(x, 632, w, 20), "P: pause · H: hide panel · Esc: back", ArcadeTypography.Style(11));
            GUI.matrix = previous; GUI.skin = previousSkin;
        }
    }
}
