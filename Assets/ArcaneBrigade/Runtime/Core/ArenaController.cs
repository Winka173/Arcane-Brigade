using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public sealed class ArenaController : MonoBehaviour
    {
        [SerializeField] private CheckpointCatalog catalog;
        [SerializeField] private Camera arenaCamera;
        [SerializeField] private RunCatalog runCatalog;
        private RunSession session;
        private RunStorage storage;
        private PlayerProfile profile;
        private readonly RunHud runHud = new RunHud();
        private readonly RunPauseGate pauseGate = new RunPauseGate();
        private readonly RunDetails details = new RunDetails();
        public RunDetails Details => details;
        public bool MenuConfirmation => pauseGate.Navigation;
        public Vector2 WorldBounds => catalog.ArenaSize;
        public Vector2 VisualChampionPosition => Vector2.Lerp(simulation.Champion.PreviousPosition, simulation.Champion.Position, Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime));
        public RunSession Session => session;
        public RunCatalog Content => runCatalog;
        public PlayerProfile Profile => profile;
        public string SaveError { get; private set; } = "";
        public string MetaNotice { get; private set; } = "";
        private CombatSimulation simulation;
        private readonly MovementInput movement = new MovementInput();
        private readonly Dictionary<int, UnitVisual> units = new Dictionary<int, UnitVisual>();
        private readonly Dictionary<int, SpriteRenderer> projectileViews = new Dictionary<int, SpriteRenderer>();
        private readonly Dictionary<int, SpriteRenderer> pickupViews = new Dictionary<int, SpriteRenderer>();
        private readonly Stack<UnitVisual> unitPool = new Stack<UnitVisual>();
        private readonly Stack<SpriteRenderer> spritePool = new Stack<SpriteRenderer>();
        private readonly HashSet<int> liveIds = new HashSet<int>();
        private readonly List<int> retiredIds = new List<int>();
        private Vector2 moveIntent;
        private Vector2? presentationMoveTarget;
        public void SetPresentationMoveTarget(Vector2? target)
        {
            if (!RunPaths.K2Capture && !RunPaths.K3FinalCapture) throw new InvalidOperationException("Movement fixture requires isolated capture mode");
            presentationMoveTarget = target;
        }
        private CheckpointAudio audioFeedback;
        private CombatFeedback combatFeedback;
        private BossTelegraph bossTelegraph;
        private ArcadeEffects arcadeEffects;
        private DeathFeedback deathFeedback;
        private ArenaToyVisual arenaToyVisual;
        private ExpandedVisual expandedVisual;
        private float transitionRemaining;
        private float resultRevealRemaining;
        private float terminalVisualAge;
        public bool TransitionPending => transitionRemaining > 0;
        public bool ResultsReady => resultRevealRemaining <= 0;
        public float WalletPulse { get; private set; }
        private ArcadeMusic music;
        public WorldShop Shop { get; private set; }
        private GUIStyle label;
        private GUIStyle heading;
        private GUIStyle button;
        private LineRenderer companionRadius;
        private Material radiusMaterial;
        private bool showCompanionRadius;
        public CombatSimulation Simulation => simulation;

        public void Configure(CheckpointCatalog definition, Camera camera) { catalog = definition; arenaCamera = camera; }
        public void ConfigureRun(RunCatalog definition, Camera camera) { runCatalog = definition; catalog = definition.Arena; arenaCamera = camera; }

        private void Start()
        {
            Time.fixedDeltaTime = 1f / 60f;
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            Application.targetFrameRate = 60;
            if (runCatalog != null)
            {
                storage = new RunStorage(RunPaths.Root);
                profile = storage.LoadProfile();
                AudioListener.volume = profile.Volume;
                session = new RunSession(runCatalog);
                RunSnapshot saved = RunLaunch.Resume ? storage.LoadRun(session) : null;
                if (saved != null && (saved.Phase == SessionPhase.Shop || saved.Phase == SessionPhase.Combat)) session.Restore(saved);
                else session.Start(string.IsNullOrEmpty(RunLaunch.ChampionId) ? runCatalog.Champions[0].Id : RunLaunch.ChampionId, RunLaunch.ChallengeIndex > 0 ? RunMeta.ChallengeSeed(RunLaunch.ChallengeIndex) : RunPaths.Diagnostic ? 131026 : Environment.TickCount, RunLaunch.Difficulty, RunMeta.ChallengeIds[RunLaunch.ChallengeIndex]);
                ArcadeSkin.Bundle = profile.Cosmetic;
                PersistBoundary();
            }
            if (GetComponent<AudioListener>() == null) gameObject.AddComponent<AudioListener>();
            audioFeedback = gameObject.AddComponent<CheckpointAudio>();
            if (runCatalog != null) { audioFeedback.ConfigurePresentation(); music = gameObject.AddComponent<ArcadeMusic>(); }
            combatFeedback = gameObject.AddComponent<CombatFeedback>();
            combatFeedback.Configure(catalog.ImpactSprite);
            deathFeedback = gameObject.AddComponent<DeathFeedback>();
            deathFeedback.Configure(catalog.ImpactSprite);
            bossTelegraph = gameObject.AddComponent<BossTelegraph>();
            if (runCatalog != null) arenaToyVisual = gameObject.AddComponent<ArenaToyVisual>();
            if (runCatalog != null) expandedVisual = gameObject.AddComponent<ExpandedVisual>();
            if (runCatalog != null) arcadeEffects = gameObject.AddComponent<ArcadeEffects>();
            if (runCatalog != null) { Shop = gameObject.AddComponent<WorldShop>(); Shop.Configure(this); }
            CreateRadiusGuide();
            Begin();
        }

        private void CreateRadiusGuide()
        {
            var child = new GameObject("CompanionRadiusGuide");
            child.transform.SetParent(transform, false);
            companionRadius = child.AddComponent<LineRenderer>();
            radiusMaterial = new Material(Shader.Find("Sprites/Default"));
            companionRadius.sharedMaterial = radiusMaterial;
            companionRadius.useWorldSpace = false;
            companionRadius.loop = true;
            companionRadius.widthMultiplier = 0.025f;
            companionRadius.startColor = companionRadius.endColor = new Color(0.5f, 0.9f, 0.95f, 0.4f);
            companionRadius.sortingOrder = 600;
            companionRadius.positionCount = 96;
            for (int index = 0; index < 96; index++)
            {
                float angle = index * Mathf.PI * 2f / 96f;
                companionRadius.SetPosition(index, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * catalog.CompanionLeash);
            }
            companionRadius.enabled = false;
        }

        private void OnDestroy() { if (radiusMaterial != null) Destroy(radiusMaterial); session?.Dispose(); }

        private void Begin(bool preserveEffects = false)
        {
            terminalVisualAge = 0;
            ClearViews();
            if (session != null)
            {
                catalog = session.PrepareCombat();
                simulation = new CombatSimulation(catalog);
                if (companionRadius != null)
                    for (int index = 0; index < 96; index++)
                    {
                        float angle = index * Mathf.PI * 2f / 96f;
                        companionRadius.SetPosition(index, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * catalog.CompanionLeash);
                    }
            }
            if (simulation == null) simulation = new CombatSimulation(catalog);
            simulation.Start(catalog.Seed);
            if (session != null) simulation.Champion.Health = Mathf.Min(session.State.Health, simulation.Champion.Definition.Health);
            if (session != null && session.State.Phase == SessionPhase.Shop) { simulation.EnterRest(); simulation.Champion.Position = simulation.Champion.PreviousPosition = new Vector2(0, -3); }
            movement.Reset();
            moveIntent = Vector2.zero;
            if (!preserveEffects)
            {
                if (combatFeedback != null) combatFeedback.Clear();
                if (arcadeEffects != null) arcadeEffects.Clear();
                if (deathFeedback != null) deathFeedback.Clear();
                WalletPulse = 0;
                transitionRemaining = resultRevealRemaining = 0;
            }
            Render(1f);
            if (session != null)
            {
                ArenaCamera follow = arenaCamera.GetComponent<ArenaCamera>();
                if (follow == null) follow = arenaCamera.gameObject.AddComponent<ArenaCamera>();
                follow.Follow(this);
                simulation.Pause(pauseGate.Paused);
            }
        }

        private void Update()
        {
            if (simulation == null) return;
            if (music != null && session != null) music.SetPhase(TransitionPending ? SessionPhase.Combat : session.State.Phase, pauseGate.Paused || simulation.Phase == RunPhase.Paused, simulation.Boss != null && simulation.Boss.Alive);
            if (Input.GetKeyDown(KeyCode.F3)) showCompanionRadius = !showCompanionRadius;
            if (session != null && Input.GetKeyDown(KeyCode.I)) ToggleDetails();
            if (details.Open && Input.GetKeyDown(KeyCode.Tab)) details.SelectTab((details.Tab + 1) % RunDetails.TabCount);
            if (details.Open && Input.GetKeyDown(KeyCode.DownArrow)) details.MoveSelection(1);
            if (details.Open && Input.GetKeyDown(KeyCode.UpArrow)) details.MoveSelection(-1);
            if (Input.GetKeyDown(KeyCode.Escape) && (session != null || simulation.Phase == RunPhase.Combat || simulation.Phase == RunPhase.Rest || simulation.Phase == RunPhase.Paused))
            {
                if (details.Open) ToggleDetails();
                else if (pauseGate.Navigation) CancelMenuReturn();
                else SetPaused(!pauseGate.Manual);
            }
            moveIntent = simulation.Phase == RunPhase.Combat || simulation.Phase == RunPhase.Rest ? movement.Read(arenaCamera.pixelRect) : Vector2.zero;
            if ((RunPaths.K2Capture || RunPaths.K3FinalCapture) && presentationMoveTarget.HasValue && simulation.Phase == RunPhase.Combat) moveIntent = Vector2.ClampMagnitude((presentationMoveTarget.Value - simulation.Champion.Position) * 4, 1);
            if (Shop != null && !TransitionPending) { Shop.Display(); if (Input.GetKeyDown(KeyCode.Space)) Shop.Interact(); }
            float interpolation = simulation.Phase == RunPhase.Combat || simulation.Phase == RunPhase.Rest ? Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime) : 1f;
            Render(interpolation);
            if (bossTelegraph != null) bossTelegraph.Display(simulation);
            if (companionRadius != null)
            {
                companionRadius.enabled = showCompanionRadius;
                companionRadius.transform.position = Vector2.Lerp(simulation.Champion.PreviousPosition, simulation.Champion.Position, interpolation);
            }
            if (!pauseGate.Paused && simulation.Phase != RunPhase.Paused)
            {
                if (simulation.Phase == RunPhase.Victory || simulation.Phase == RunPhase.Defeat) terminalVisualAge += Time.deltaTime;
                if (combatFeedback != null) combatFeedback.Advance(Time.deltaTime);
                if (arcadeEffects != null) arcadeEffects.Advance(Time.deltaTime);
                if (deathFeedback != null) deathFeedback.Advance(Time.deltaTime);
                WalletPulse = Mathf.Max(0, WalletPulse - Time.deltaTime * 3);
                resultRevealRemaining = Mathf.Max(0, resultRevealRemaining - Time.deltaTime);
                if (transitionRemaining > 0)
                {
                    transitionRemaining -= Time.deltaTime;
                    if (transitionRemaining <= 0) Begin(true);
                }
            }
        }

        private void FixedUpdate()
        {
            if (simulation == null) return;
            if (session != null && session.State.Phase == SessionPhase.Shop) { simulation.StepRest(Time.fixedDeltaTime, moveIntent); return; }
            if (session != null && session.State.Phase != SessionPhase.Combat) return;
            simulation.Step(Time.fixedDeltaTime, moveIntent);
            foreach (CombatEvent value in simulation.Events)
            {
                if (value.Kind == CombatEventKind.Pickup && value.Value > 0 && (value.Loot == LootKind.Coin || value.Loot == LootKind.Egg)) WalletPulse = 1;
                if (value.Kind != CombatEventKind.Kill || value.Subject == null || deathFeedback == null) continue;
                if (units.TryGetValue(value.SubjectId, out UnitVisual existing)) { existing.Display(simulation.VisualElapsed, 1); deathFeedback.Play(existing, value.Subject); }
                else
                {
                    UnitVisual temporary = unitPool.Count > 0 ? unitPool.Pop() : Instantiate(catalog.UnitPrefab, transform).GetComponent<UnitVisual>();
                    temporary.gameObject.SetActive(true);
                    temporary.Bind(value.Subject, catalog);
                    deathFeedback.Play(temporary, value.Subject);
                    temporary.gameObject.SetActive(false);
                    unitPool.Push(temporary);
                }
            }
            if (arcadeEffects != null)
                foreach (CombatEvent value in simulation.Events) arcadeEffects.Play(value);
            if (combatFeedback != null)
                for (int index = 0; index < simulation.Events.Count; index++) combatFeedback.Play(simulation.Events[index]);
            if (audioFeedback != null)
                for (int index = 0; index < simulation.Events.Count; index++) audioFeedback.Play(simulation.Events[index]);
            if (session != null && (simulation.Phase == RunPhase.Victory || simulation.Phase == RunPhase.Defeat))
            {
                session.Settle(simulation);
                PersistBoundary();
                movement.Reset();
                if (session.State.Phase == SessionPhase.Shop) transitionRemaining = simulation.BossDefeated ? 1.15f : 0.45f;
                else resultRevealRemaining = simulation.Phase == RunPhase.Defeat ? 1f : 1.15f;
            }
        }

        private void SetPaused(bool paused)
        {
            pauseGate.Manual = paused;
            ApplyPause();
        }

        private void ApplyPause()
        {
            simulation.Pause(pauseGate.Paused);
            movement.Reset();
            moveIntent = Vector2.zero;
        }

        public void TogglePause() { SetPaused(!pauseGate.Manual); }
        public void ToggleDetails()
        {
            if (session == null) return;
            if (details.Open) details.Close();
            else details.Show(session, runCatalog, session.State.Phase == SessionPhase.Combat ? simulation.Champion.Health : session.State.Health, session.State.Phase == SessionPhase.Combat ? simulation : null);
            pauseGate.Details = details.Open;
            ApplyPause();
        }
        public void RequestMenuReturn() { pauseGate.Navigation = true; ApplyPause(); }
        public void CancelMenuReturn() { pauseGate.Navigation = false; ApplyPause(); }

        private bool PersistBoundary()
        {
            if (storage == null || session == null) return true;
            try
            {
                storage.SaveRun(session.State);
                SaveError = "";
            }
            catch (Exception exception) { SaveError = "Save failed: " + exception.Message; return false; }
            try
            {
                int before = profile.Achievements.Count + profile.SeenWeapons.Count + profile.SeenBosses.Count + profile.SeenChains.Count;
                storage.RecordProgress(profile, session.State, runCatalog);
                int delta = profile.Achievements.Count + profile.SeenWeapons.Count + profile.SeenBosses.Count + profile.SeenChains.Count - before;
                MetaNotice = delta > 0 ? "ĐÃ LƯU " + delta + " KHÁM PHÁ / THÀNH TÍCH MỚI · XEM BẢO TÀNG" : "";
            }
            catch (Exception exception) { SaveError = "Run saved; profile save failed: " + exception.Message; }
            return true;
        }

        private bool Transaction(string title, Func<bool> operation)
        {
            if (pauseGate.Paused || TransitionPending) return false;
            bool committed = RunTransaction.TryCommit(session, runCatalog, title, operation, PersistBoundary, out string error);
            if (!committed)
            {
                if (!string.IsNullOrEmpty(error)) SaveError = error;
                return false;
            }
            if (session.State.Phase == SessionPhase.Shop)
            {
                Vector2 position = simulation.Champion.Position;
                Begin(true);
                simulation.Champion.Position = simulation.Champion.PreviousPosition = position;
            }
            return true;
        }

        public bool BuyOffer(int token) => Transaction("Mua tại quầy", () => session.Buy(token));
        public bool RerollShop() => Transaction("Đổi hàng", () => session.Reroll());
        public bool RerollAltar() => Transaction("Đổi hàng altar", () => session.RerollAltar());
        public bool FuseAltar(string id) => Transaction("Hợp nhất tại altar", () => session.Altar(id));
        public void ShowReceiptFixture() => runHud.ShowReceiptFixture();
        public void ShowInvoiceEvidenceFixture() => runHud.ShowInvoiceEvidenceFixture();
        public void ShowOfferPreviewFixture() => runHud.ShowOfferPreviewFixture();
        public void NextWave()
        {
            if (session.State.Phase != SessionPhase.Shop || TransitionPending) return;
            RunSnapshot before = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(session.State));
            session.ContinueWave();
            if (session.State.Phase != SessionPhase.Combat) return;
            if (!PersistBoundary()) { session.Restore(before); return; }
            Begin(true);
        }
        public void ContinueEndless()
        {
            if (session.State.Phase != SessionPhase.Victory || !ResultsReady) return;
            if (!PersistBoundary() || !profile.CompletedRuns.Contains(session.State.RunId)) return;
            RunSnapshot before = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(session.State));
            session.EnterEndless();
            if (!PersistBoundary()) { session.Restore(before); return; }
            Begin();
        }
        public void EnterShopPresentation() { if (session.State.Phase == SessionPhase.Shop) Begin(true); }
        public void RestartRun()
        {
            if (!PersistBoundary() || ((session.State.Phase == SessionPhase.Victory || session.State.Phase == SessionPhase.Defeat) && !profile.CompletedRuns.Contains(session.State.RunId))) return;
            RunSnapshot before = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(session.State));
            string challenge = session.State.ChallengeId;
            int challengeIndex = Array.IndexOf(RunMeta.ChallengeIds, challenge);
            session.Start(session.State.ChampionId, challengeIndex > 0 ? RunMeta.ChallengeSeed(challengeIndex) : Environment.TickCount, session.State.Difficulty, challenge);
            if (!PersistBoundary()) { session.Restore(before); return; }
            details.Close(); pauseGate.ResetSession(); Begin();
        }
        public void ReturnToMenu() { if (session != null && !PersistBoundary()) return; SceneManager.LoadScene(session == null ? "Frontend" : "RunFrontend"); }

        private void OnApplicationFocus(bool focus) { if (!RunPaths.Diagnostic) { pauseGate.FocusLost = !focus; if (simulation != null) ApplyPause(); } }
        private void OnApplicationPause(bool paused) { pauseGate.Backgrounded = paused; if (simulation != null) ApplyPause(); }

        private void ClearViews()
        {
            foreach (UnitVisual view in units.Values) { view.gameObject.SetActive(false); unitPool.Push(view); }
            units.Clear();
            ReleaseAll(projectileViews);
            ReleaseAll(pickupViews);
        }

        private void ReleaseAll(Dictionary<int, SpriteRenderer> views)
        {
            foreach (SpriteRenderer view in views.Values) { view.gameObject.SetActive(false); spritePool.Push(view); }
            views.Clear();
        }

        private SpriteRenderer AcquireSprite(string name, Sprite sprite)
        {
            SpriteRenderer renderer;
            if (spritePool.Count > 0) renderer = spritePool.Pop();
            else
            {
                var child = new GameObject(name);
                child.transform.SetParent(transform, false);
                renderer = child.AddComponent<SpriteRenderer>();
            }
            renderer.gameObject.SetActive(true);
            CoinVisual coinVisual = renderer.GetComponent<CoinVisual>();
            if (coinVisual != null) coinVisual.Hide();
            renderer.name = name;
            renderer.sprite = sprite;
            renderer.sortingOrder = 2000;
            return renderer;
        }

        private void Render(float interpolation)
        {
            if (arenaToyVisual != null) arenaToyVisual.Display(simulation, interpolation);
            if (expandedVisual != null) { expandedVisual.ReducedMotion = profile != null && profile.ReducedMotion; expandedVisual.Display(simulation); }
            liveIds.Clear();
            for (int index = 0; index < simulation.Units.Count; index++)
            {
                UnitState state = simulation.Units[index];
                if (!state.Alive) continue;
                liveIds.Add(state.Id);
                if (!units.TryGetValue(state.Id, out UnitVisual view))
                {
                    view = unitPool.Count > 0 ? unitPool.Pop() : Instantiate(catalog.UnitPrefab, transform).GetComponent<UnitVisual>();
                    view.gameObject.SetActive(true);
                    view.Bind(state, catalog);
                    if (profile != null) view.SetChampionCosmetic(profile.Cosmetic);
                    units.Add(state.Id, view);
                }
                view.ReducedMotion = profile != null && profile.ReducedMotion;
                float poseTime = simulation.VisualElapsed + terminalVisualAge - (simulation.Phase == RunPhase.Combat || simulation.Phase == RunPhase.Rest ? Time.fixedDeltaTime * (1 - interpolation) : 0);
                view.Display(poseTime, interpolation);
                if (terminalVisualAge > 0 && state.Faction == Faction.Enemy) view.SetTerminalFade(Mathf.Clamp01(1 - terminalVisualAge / 0.35f));
            }
            retiredIds.Clear();
            foreach (int identifier in units.Keys) if (!liveIds.Contains(identifier)) retiredIds.Add(identifier);
            for (int index = 0; index < retiredIds.Count; index++)
            {
                UnitVisual view = units[retiredIds[index]];
                view.gameObject.SetActive(false);
                unitPool.Push(view);
                units.Remove(retiredIds[index]);
            }

            liveIds.Clear();
            for (int index = 0; index < simulation.Projectiles.Count; index++)
            {
                ProjectileState state = simulation.Projectiles[index];
                liveIds.Add(state.Id);
                if (!projectileViews.TryGetValue(state.Id, out SpriteRenderer view))
                {
                    Sprite disc = runCatalog != null && runCatalog.ProjectileDisc != null ? runCatalog.ProjectileDisc : catalog.Projectile;
                    view = AcquireSprite("Projectile", state.Sprite != null ? state.Sprite : disc);
                    ProjectileVisual visual = view.GetComponent<ProjectileVisual>();
                    if (visual == null) visual = view.gameObject.AddComponent<ProjectileVisual>();
                    visual.Configure(disc);
                    projectileViews.Add(state.Id, view);
                }
                view.transform.position = Vector2.Lerp(state.PreviousPosition, state.Position, interpolation);
                float projectileSize = state.Faction == Faction.Enemy ? 0.2f : state.VisualWidth > 0 ? state.VisualWidth : 0.22f;
                view.transform.localScale = Vector3.one * (projectileSize / view.sprite.bounds.size.x);
                view.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(state.Velocity.y, state.Velocity.x) * Mathf.Rad2Deg);
                Color projectileColor = state.Color;
                projectileColor.a *= Mathf.Clamp01(1 - terminalVisualAge / 0.25f);
                view.color = projectileColor;
                ProjectileVisual projectileVisual = view.GetComponent<ProjectileVisual>();
                projectileVisual.Display(state, view, view.transform.position, simulation.Elapsed + terminalVisualAge);
                if (terminalVisualAge > 0) projectileVisual.SetFade(projectileColor.a);
            }
            RetireSprites(projectileViews);

            liveIds.Clear();
            for (int index = 0; index < simulation.Pickups.Count; index++)
            {
                PickupState state = simulation.Pickups[index];
                Sprite lootSprite = state.Kind == LootKind.Egg ? runCatalog?.EggSprite : state.Kind == LootKind.Heart ? runCatalog?.HeartSprite : state.Kind == LootKind.Chest ? runCatalog?.ChestSprite : catalog.Coin;
                if (lootSprite == null) lootSprite = catalog.Coin;
                liveIds.Add(state.Id);
                if (!pickupViews.TryGetValue(state.Id, out SpriteRenderer view))
                {
                    view = AcquireSprite(state.Kind.ToString(), lootSprite);
                    ProjectileVisual visual = view.GetComponent<ProjectileVisual>();
                    if (visual != null) visual.Hide();
                    pickupViews.Add(state.Id, view);
                }
                CoinVisual coin = view.GetComponent<CoinVisual>();
                view.sprite = lootSprite;
                if (coin == null) coin = view.gameObject.AddComponent<CoinVisual>();
                float coinTime = simulation.Elapsed + terminalVisualAge - (simulation.Phase == RunPhase.Combat ? Time.fixedDeltaTime * (1 - interpolation) : 0);
                Sprite disc = runCatalog != null && runCatalog.ProjectileDisc != null ? runCatalog.ProjectileDisc : catalog.Projectile;
                coin.Display(state, view, disc, catalog.ImpactSprite, coinTime, interpolation);
                if (terminalVisualAge > 0) coin.SetFade(view, Mathf.Clamp01(1 - terminalVisualAge / 0.4f));
            }
            RetireSprites(pickupViews);
        }

        private void RetireSprites(Dictionary<int, SpriteRenderer> views)
        {
            retiredIds.Clear();
            foreach (int identifier in views.Keys) if (!liveIds.Contains(identifier)) retiredIds.Add(identifier);
            for (int index = 0; index < retiredIds.Count; index++)
            {
                SpriteRenderer view = views[retiredIds[index]];
                view.gameObject.SetActive(false);
                spritePool.Push(view);
                views.Remove(retiredIds[index]);
            }
        }

        private void OnGUI()
        {
            if (simulation == null) return;
            if (session != null)
            {
                bool previousEnabled = GUI.enabled;
                GUI.enabled = previousEnabled && !details.Open && !pauseGate.Navigation;
                runHud.Draw(this, arenaCamera);
                GUI.enabled = previousEnabled;
                if (details.Open) details.Draw(this, arenaCamera);
                else if (pauseGate.Navigation) runHud.DrawMenuConfirmation(this, arenaCamera);
                else DrawTouch();
                return;
            }
            if (label == null)
            {
                label = new GUIStyle(GUI.skin.label) { fontSize = 23, alignment = TextAnchor.MiddleLeft };
                heading = new GUIStyle(label) { fontSize = 38, alignment = TextAnchor.MiddleCenter };
                button = new GUIStyle(GUI.skin.button) { fontSize = 24 };
            }
            Rect viewport = arenaCamera.pixelRect;
            float scale = viewport.width / 1920f;
            Matrix4x4 previous = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(new Vector3(viewport.x, Screen.height - viewport.yMax, 0f), Quaternion.identity, Vector3.one * scale);
            GUI.Box(new Rect(52, 30, 1816, 80), GUIContent.none);
            GUI.Label(new Rect(80, 43, 470, 50), "HP " + Mathf.CeilToInt(simulation.Champion.Health) + " / " + catalog.Champion.Health + "    GOLD " + simulation.Gold, label);
            GUI.Label(new Rect(660, 43, 970, 50), "WAVE 1   " + Mathf.Max(0, Mathf.CeilToInt(catalog.WaveDuration - simulation.Elapsed)) + "s   KILLS " + simulation.Kills + "   ALLIES " + simulation.CompanionCount + "   RADIUS " + catalog.CompanionLeash.ToString("0.0"), label);
            if (GUI.Button(new Rect(1695, 45, 145, 48), "PAUSE", button)) SetPaused(true);
            GUI.Label(new Rect(70, 1015, 1780, 42), "Move: WASD / arrows / lower-left touch drag.  F3: ally radius guide.  Auto-combat; shop and merge pending.", label);
            if (simulation.Phase != RunPhase.Combat)
            {
                GUI.Box(new Rect(590, 340, 740, 390), GUIContent.none);
                string text = simulation.Phase == RunPhase.Paused ? "PAUSED" : simulation.Phase == RunPhase.Victory ? "WAVE SURVIVED" : "BRIGADE FALLEN";
                GUI.Label(new Rect(620, 365, 680, 65), text, heading);
                GUI.Label(new Rect(675, 445, 650, 50), "Kills: " + simulation.Kills + "    Collected gold: " + simulation.Gold, label);
                if (GUI.Button(new Rect(760, 520, 400, 60), simulation.Phase == RunPhase.Paused ? "RESUME" : "PLAY AGAIN", button))
                {
                    if (simulation.Phase == RunPhase.Paused) SetPaused(false); else Begin();
                }
                if (GUI.Button(new Rect(760, 600, 400, 60), "MAIN MENU", button)) SceneManager.LoadScene("Frontend");
            }
            GUI.matrix = previous;
            DrawTouch();
        }

        private void DrawTouch()
        {
            if (movement.TouchActive)
            {
                Vector2 origin = movement.TouchOrigin;
                Vector2 offset = movement.TouchOffset;
                float radius = movement.TouchRadius;
                GUI.Box(new Rect(origin.x - radius, Screen.height - origin.y - radius, radius * 2f, radius * 2f), GUIContent.none);
                GUI.Box(new Rect(origin.x + offset.x - 18f, Screen.height - origin.y - offset.y - 18f, 36f, 36f), GUIContent.none);
            }
        }

#if UNITY_EDITOR
        public void CaptureRunFixture(RunCatalog definition, int seed, float seconds)
        {
            runCatalog = definition;
            session = new RunSession(definition);
            session.Start(definition.Champions[0].Id, seed);
            profile = new PlayerProfile();
            CaptureFixture(seconds);
        }

        public void CaptureFixture(float seconds, bool stationary = false)
        {
            if (combatFeedback == null)
            {
                combatFeedback = gameObject.AddComponent<CombatFeedback>();
                combatFeedback.Configure(catalog.ImpactSprite);
            }
            Begin();
            for (int tick = 0; tick < Mathf.RoundToInt(seconds * 60f); tick++)
            {
                float time = tick / 60f;
                Vector2 direction = new Vector2(Mathf.Cos(time * 0.55f), Mathf.Sin(time * 0.8f));
                simulation.Step(1f / 60f, stationary ? Vector2.zero : direction * 0.6f);
                for (int index = 0; index < simulation.Events.Count; index++) combatFeedback.Play(simulation.Events[index]);
                combatFeedback.Advance(1f / 60f);
            }
            Render(1f);
        }
#endif
    }
}
