using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public sealed partial class ArenaController : MonoBehaviour
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
        public Vector2 VisualChampionPosition => stageTransfer != null && stageTransfer.Active ? stageTransfer.VisualPlayerPosition : Vector2.Lerp(simulation.Champion.PreviousPosition, simulation.Champion.Position, Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime));
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
            if (!RunPaths.K2Capture && !RunPaths.K3FinalCapture && !RunPaths.K7Capture && !HowToRecorder.Requested) throw new InvalidOperationException("Movement fixture requires isolated capture mode");
            presentationMoveTarget = target;
        }
        private CheckpointAudio audioFeedback;
        private readonly ToyboxAudioObserver toyboxAudio = new ToyboxAudioObserver();
        private CombatFeedback combatFeedback;
        private HitWords hitWords; // comic sound words over struck enemies (requirement 24/10/2026)
        private BossTelegraph bossTelegraph;
        private MapEnvironmentVisual mapVisual;
        private ArcadeEffects arcadeEffects;
        private DeathFeedback deathFeedback;
        private ExplosionFeedback explosions;
        private ElementInk elementInk; // each hero's blows land with its own look (fire, ice, water, storm, void...)
        private PacingFeedback pacing;
        private ArenaToyVisual arenaToyVisual;
        private ExpandedVisual expandedVisual;
        private ArsenalVisual arsenalVisual;
        private float transitionRemaining;
        private float resultRevealRemaining;
        private float terminalVisualAge;
        private bool hallExitPending;
        private float hallExitElapsed;
        private Vector2 hallExitTarget;
        private StageTransfer stageTransfer;
        public bool PresentationPaused => pauseGate.Paused;
        public bool TransitionPending => transitionRemaining > 0 || SceneDissolve.Busy || stageTransfer != null && stageTransfer.Active;
        /// <summary>The won fight is still on screen (the last falls, a boss's death) before it melts into the shop: the view stays on the fight.</summary>
        public bool FightStillShown => session != null && session.State.Phase == SessionPhase.Shop && transitionRemaining > 0;
        /// <summary>Where the boss fell, while its death plays before the shop.</summary>
        public Vector2? BossFallPoint => simulation != null && simulation.BossDefeated && simulation.Boss != null && transitionRemaining > 0 ? simulation.Boss.Position : (Vector2?)null;
        public bool HallExitPending => hallExitPending;
        public float HallCurtain => 0;
        public bool TerminalDelayComplete => resultRevealRemaining <= 0;
        public bool ResultsReady => TerminalDelayComplete && (session == null || !session.EndingPending);
        private NightmarePresentation nightmare;
        public bool NightmareEndingVisible => nightmare != null && nightmare.EndingVisible;
        /// <summary>The journey's last wave (wave 20 of a normal run): the one boss whose fall plays in slow motion.</summary>
        public bool FinalBossWave => session != null && runCatalog != null && !session.State.Endless && session.State.Wave >= runCatalog.NormalWaves;
        public bool SaveNightmareBoundary() => PersistBoundary();
        public bool ChooseNightmareEnding(NightmareFarewell choice)
        {
            if (!TerminalDelayComplete || PresentationPaused || !session.EndingChoicePending) return false;
            string previous = JsonUtility.ToJson(session.State);
            if (!session.ChooseFarewell(choice)) return false;
            if (PersistBoundary()) return true;
            session.Restore(JsonUtility.FromJson<RunSnapshot>(previous));
            return false;
        }
        public void AdvanceNightmareEnding()
        {
            if (!TerminalDelayComplete || PresentationPaused || !session.EndingPending) return;
            string previous = JsonUtility.ToJson(session.State);
            if (session.AdvanceEnding() && !PersistBoundary()) session.Restore(JsonUtility.FromJson<RunSnapshot>(previous));
        }
        public float WalletPulse { get; private set; }
        public float PresentationClock { get; private set; }
        private ArcadeMusic music;
        public WorldShop Shop { get; private set; }
        private GUIStyle label;
        private GUIStyle heading;
        private GUIStyle button;
        private LineRenderer companionRadius;
        private Material radiusMaterial;
        private bool warningAudio;
        private bool lowHealthAudio;
        private FootstepDust footDust;
        private StagePerformance stagePerformance;
        private PaintedEffects paintedEffects;
        private MidwaveSpectacleVisual spectacleVisual;
        private readonly CombatFeel combatFeel = new CombatFeel();
        public Vector2 CameraAccent => combatFeel.CameraOffset;
        public CombatFeel Feel => combatFeel;
        public CombatSimulation Simulation => simulation;
        /// <summary>The drawn view of a unit (HeroFocus finds the hero and the friends around it).</summary>
        public bool TryView(int id, out UnitVisual view) => units.TryGetValue(id, out view);
        public IEnumerable<UnitVisual> Views => units.Values;

        public void Configure(CheckpointCatalog definition, Camera camera) { catalog = definition; arenaCamera = camera; }
        public void ConfigureRun(RunCatalog definition, Camera camera) { runCatalog = definition; catalog = definition.Arena; arenaCamera = camera; CurrencyIcons.Use(definition); }

        private void Start()
        {
            Time.fixedDeltaTime = 1f / 60f;
            Application.targetFrameRate = 60; // the profile's frame pace replaces this below (PresentationSettings.ApplyDisplay)
            if (runCatalog != null)
            {
                storage = new RunStorage(RunPaths.Root);
                profile = storage.LoadProfile();
                AudioListener.volume = profile.Volume;
                session = new RunSession(runCatalog);
                RunSnapshot saved = RunLaunch.Resume ? storage.LoadRun(session) : null;
                if (saved != null) session.Restore(saved);
                else session.Start(string.IsNullOrEmpty(RunLaunch.ChampionId) ? runCatalog.Champions[0].Id : RunLaunch.ChampionId, !string.IsNullOrEmpty(RunLaunch.Daily) ? MetaProgression.DailySeed(RunLaunch.Daily) : RunLaunch.ChallengeIndex > 0 ? RunMeta.ChallengeSeed(RunLaunch.ChallengeIndex) : RunPaths.Diagnostic ? 131026 : Environment.TickCount, RunLaunch.Difficulty, RunMeta.ChallengeIds[RunLaunch.ChallengeIndex]);
                // a new run takes the Waiting Room's loadout and the choices made in hero select (play-test item 27)
                if (saved == null) session.ApplyMeta(MetaProgression.Loadout(profile, runCatalog, session.State.ChampionId, RunLaunch.StartFriend, RunLaunch.AltStarter, RunLaunch.SecondHabit, keepsake: RunLaunch.Keepsake, daily: RunLaunch.Daily));
                if (session.State.WaitingCampaign?.Room > 0) { SceneManager.LoadScene("WaitingRoom"); return; }
                ArcadeSkin.Bundle = profile.Cosmetic;
                PresentationSettings.Apply(profile); PresentationSettings.ApplyDisplay(profile);
                AudioListener.pause = false;
                PersistBoundary();
            }
            if (GetComponent<AudioListener>() == null) gameObject.AddComponent<AudioListener>();
            audioFeedback = gameObject.AddComponent<CheckpointAudio>();
            if (runCatalog != null) { audioFeedback.ConfigurePresentation(); music = gameObject.AddComponent<ArcadeMusic>(); }
            combatFeedback = gameObject.AddComponent<CombatFeedback>();
            combatFeedback.Configure(catalog.ImpactSprite);
            footDust = gameObject.AddComponent<FootstepDust>(); footDust.Configure(catalog.ImpactSprite);
            stagePerformance = gameObject.AddComponent<StagePerformance>(); stagePerformance.Configure(this);
            paintedEffects = gameObject.AddComponent<PaintedEffects>();
            spectacleVisual = gameObject.AddComponent<MidwaveSpectacleVisual>();
            deathFeedback = gameObject.AddComponent<DeathFeedback>();
            deathFeedback.Configure(catalog.ImpactSprite, null, Resources.Load<PaintedAnimationLibrary>("PaintedAnimationLibrary"));
            explosions = gameObject.AddComponent<ExplosionFeedback>();
            pacing = gameObject.AddComponent<PacingFeedback>(); pacing.Configure(this, arenaCamera);
            bossTelegraph = gameObject.AddComponent<BossTelegraph>();
            mapVisual = gameObject.AddComponent<MapEnvironmentVisual>();
            if (runCatalog != null) arenaToyVisual = gameObject.AddComponent<ArenaToyVisual>();
            if (runCatalog != null) expandedVisual = gameObject.AddComponent<ExpandedVisual>();
            if (runCatalog != null) arsenalVisual = gameObject.AddComponent<ArsenalVisual>();
            if (runCatalog != null) arcadeEffects = gameObject.AddComponent<ArcadeEffects>();
            if (runCatalog != null) { elementInk = new GameObject("Element looks").AddComponent<ElementInk>(); elementInk.Configure(this); }
            if (runCatalog != null) { Shop = gameObject.AddComponent<WorldShop>(); Shop.Configure(this); }
            if (runCatalog != null) gameObject.AddComponent<OutsideStageChoice>().Configure(this);
            if (runCatalog != null) { SecretRoom = gameObject.AddComponent<SecretEventRoom>(); SecretRoom.Configure(this); }
            gameObject.AddComponent<PrismShardHud>().Configure(this);
            if (runCatalog != null) gameObject.AddComponent<HeroFocus>().Configure(this);
            if (runCatalog != null) gameObject.AddComponent<K8ArenaScenery>().Configure(this);
            if (runCatalog != null) gameObject.AddComponent<OctoberScenery>().Configure(this);
            if (runCatalog != null) gameObject.AddComponent<OctoberDialogue>().Configure(this);
            if (runCatalog != null) { nightmare = gameObject.AddComponent<NightmarePresentation>(); nightmare.Configure(this); }
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

        private void OnDestroy() { AnimationDetailBudget.SetLoad(0, 0); if (radiusMaterial != null) Destroy(radiusMaterial); session?.Dispose(); }

        private void Begin(bool preserveEffects = false)
        {
            combatFeel.Clear();
            var positions = new Dictionary<string, Vector2>();
            Vector2? championPosition = null;
            if (preserveEffects && simulation != null)
            {
                championPosition = simulation.Champion.Position;
                foreach (UnitState unit in simulation.Units)
                    if (unit.Faction == Faction.Companion) positions[unit.Definition.Id + "/" + unit.Definition.Weapon.Id] = unit.Position;
            }
            if (footDust != null) footDust.Clear();
            if (paintedEffects != null) paintedEffects.Clear();
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
            if (championPosition.HasValue)
            {
                simulation.Champion.Position = simulation.Champion.PreviousPosition = championPosition.Value;
                foreach (UnitState unit in simulation.Units)
                    if (unit.Faction == Faction.Companion && positions.TryGetValue(unit.Definition.Id + "/" + unit.Definition.Weapon.Id, out Vector2 position)) unit.Position = unit.PreviousPosition = position;
            }
            movement.Reset();
            if (session != null && session.State.Phase == SessionPhase.Shop) simulation.SetShopBounds(ReviewBalance.ShopHalfExtents(runCatalog.RestViewHeight));
            moveIntent = Vector2.zero;
            if (!preserveEffects)
            {
                if (combatFeedback != null) combatFeedback.Clear();
                if (hitWords != null) hitWords.Clear();
                if (arcadeEffects != null) arcadeEffects.Clear();
                if (deathFeedback != null) deathFeedback.Clear();
                if (explosions != null) explosions.Clear();
                if (pacing != null) pacing.Clear();
                WalletPulse = 0;
                transitionRemaining = resultRevealRemaining = 0;
            }
            Render(1f);
            if (session != null)
            {
                ArenaCamera follow = arenaCamera.GetComponent<ArenaCamera>();
                if (follow == null) follow = arenaCamera.gameObject.AddComponent<ArenaCamera>();
                follow.Follow(this, !preserveEffects);
                simulation.Pause(pauseGate.Paused);
            }
        }

        private void Update()
        {
            if (simulation == null) return;
            if (StoryConsumesInput) { Render(1); return; }
            runHud.ReadNavigation(pauseGate.Manual || pauseGate.Navigation || session != null && (session.State.Phase == SessionPhase.Victory || session.State.Phase == SessionPhase.Defeat));
            if (music != null && session != null)
            {
                bool dawn = session.State.Nightmare?.CustomerDefeated == true && !session.State.Endless;
                music.SetDreamDepth(NightmareRules.Depth(session.State.Wave),dawn);
                music.ForceScene(dawn && session.State.Nightmare.EndingPage >= 2 ? "menu" : null);
            }
            if (music != null && session != null) music.SetPhase(TransitionPending ? SessionPhase.Combat : session.State.Phase, pauseGate.Paused || simulation.Phase == RunPhase.Paused, simulation.Boss != null && simulation.Boss.Alive);
            if (music != null && session != null) music.SetContext(session.State.Wave, runCatalog.NormalWaves, session.State.Endless, Shop != null && Shop.Special, simulation.Units);
            if (session != null && Input.GetKeyDown(KeyCode.I)) ToggleDetails();
            if (details.Open && Input.GetKeyDown(KeyCode.Tab)) details.SelectTab((details.Tab + 1) % RunDetails.TabCount);
            if (details.Open && Input.GetKeyDown(KeyCode.DownArrow)) details.MoveSelection(1);
            if (details.Open && Input.GetKeyDown(KeyCode.UpArrow)) details.MoveSelection(-1);
            if (details.Open && Input.GetKeyDown(KeyCode.JoystickButton5)) details.SelectTab((details.Tab + 1) % RunDetails.TabCount);
            if (details.Open && Input.GetKeyDown(KeyCode.JoystickButton4)) details.SelectTab((details.Tab + RunDetails.TabCount - 1) % RunDetails.TabCount);
            if (details.Open && Input.GetKeyDown(KeyCode.JoystickButton0)) details.MoveSelection(1);
            if (details.Open && Input.GetKeyDown(KeyCode.JoystickButton2)) { if (details.HeroSelected && session.CanConfigureHero) ConfigureHeroIdentity(() => session.ChooseHeroApproach((session.State.HeroIdentity.Approach + 1) % 3)); else details.MoveSelection(-1); }
            if (details.HeroSelected && session.CanConfigureHero && (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.JoystickButton8))) ConfigureHeroIdentity(() => session.State.ChampionId == "PatchworkHost" ? session.CycleFeaturedFriend() : session.State.ChampionId == "GlassDice" ? session.RerollHeroDie() : session.State.ChampionId == "PennyBroker" ? session.PlaceHeroOrder() : session.State.ChampionId == "LoneJelly" && session.CycleMemoryOrgan(session.State.HeroIdentity.Approach));
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1) || Input.GetKeyDown(KeyCode.JoystickButton7))
            {
                if (details.Open) ToggleDetails();
                else if (pauseGate.Navigation) CancelMenuReturn();
                else SetPaused(!pauseGate.Manual);
            }
            moveIntent = simulation.Phase == RunPhase.Combat || simulation.Phase == RunPhase.Rest || simulation.Phase == RunPhase.Victory && (transitionRemaining > 0 || session != null && session.EndingChoicePending) ? movement.Read(arenaCamera.pixelRect) : Vector2.zero;
            if ((RunPaths.K2Capture || RunPaths.K3FinalCapture || RunPaths.K7Capture) && presentationMoveTarget.HasValue && (simulation.Phase == RunPhase.Combat || RunPaths.K7Capture && simulation.Phase == RunPhase.Rest)) moveIntent = Vector2.ClampMagnitude((presentationMoveTarget.Value - simulation.Champion.Position) * 4, 1);
            if (hallExitPending) moveIntent = Vector2.ClampMagnitude((hallExitTarget - simulation.Champion.Position) * 4, 1);
            if (SecretRoom != null && SecretRoom.MenuOpen) moveIntent = Vector2.zero;
            if (ScriptedMove.HasValue && RunPaths.Diagnostic) moveIntent = Vector2.ClampMagnitude(ScriptedMove.Value, 1); // recorded guide clips (HowToRecorder)
            if (Shop != null && !TransitionPending && !hallExitPending && simulation.Phase == RunPhase.Rest && !StoryConsumesInput && (SecretRoom == null || !SecretRoom.MenuOpen)) moveIntent = Shop.TapToWalk(moveIntent, arenaCamera);
            if (Shop != null && !TransitionPending) { Shop.Display(); Shop.WalkIntoExit(); if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.JoystickButton0)) Shop.Interact(); } // Space or E, like every other thing in the room
            bool warning = simulation.Boss != null && simulation.Boss.Alive && simulation.Boss.TelegraphUntil > simulation.Elapsed;
            if (warning && !warningAudio && !pauseGate.Paused && !ArcaneCueLibrary.Available) audioFeedback.PlayCue(24, true);
            warningAudio = warning;
            bool lowHealth = simulation.Champion.Alive && simulation.Champion.Health < simulation.Champion.Definition.Health * 0.25f;
            if (lowHealth && !lowHealthAudio && !pauseGate.Paused) audioFeedback.PlayCue(25, true);
            lowHealthAudio = lowHealth;
            float interpolation = simulation.Phase == RunPhase.Combat || simulation.Phase == RunPhase.Rest ? Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime) : 1f;
            Render(interpolation);
            if (footDust != null) footDust.Display(simulation, simulation.Phase == RunPhase.Paused ? 0 : Time.deltaTime);
            if (bossTelegraph != null) bossTelegraph.Display(simulation);
            if (mapVisual != null) mapVisual.Display(simulation);
            if (companionRadius != null)
            {
                companionRadius.enabled = false;
                companionRadius.transform.position = Vector2.Lerp(simulation.Champion.PreviousPosition, simulation.Champion.Position, interpolation);
            }
            if (!pauseGate.Paused && simulation.Phase != RunPhase.Paused)
            {
                // effects, numbers and deaths slow to a third during the final boss's slow motion (there is no hit stop)
                float feltDelta = Time.deltaTime * combatFeel.PresentationSpeed;
                PresentationClock += feltDelta;
                combatFeel.Advance(Time.deltaTime);
                if (simulation.Phase == RunPhase.Victory || simulation.Phase == RunPhase.Defeat) terminalVisualAge += feltDelta;
                if (combatFeedback != null) combatFeedback.Advance(feltDelta);
                if (arcadeEffects != null) arcadeEffects.Advance(feltDelta);
                if (paintedEffects != null) paintedEffects.Advance(feltDelta);
                if (deathFeedback != null) deathFeedback.Advance(feltDelta);
                if (explosions != null) explosions.Advance(feltDelta);
                WalletPulse = Mathf.Max(0, WalletPulse - Time.deltaTime * 3);
                resultRevealRemaining = Mathf.Max(0, resultRevealRemaining - Time.deltaTime);
                if (hallExitPending)
                {
                    hallExitElapsed += Time.deltaTime;
                    if (hallExitElapsed >= MarketMotion.DepartureDuration) { hallExitPending = false; NextWave(); }
                }
                if (transitionRemaining > 0)
                {
                    transitionRemaining -= Time.deltaTime;
                    if (transitionRemaining <= 0) SceneryCrossfade.Around(() => Begin(true), ActorViews()); // on the same floor: the last enemies are gone, the shopkeeper drops in and the stock lands (requirement 24/10/2026)
                }
            }
        }

        private void FixedUpdate()
        {
            if (simulation == null) return;
            if (stageTransfer != null && stageTransfer.Active) return;
            if (pauseGate.Paused) return;
            if (simulation.Phase == RunPhase.Victory && (transitionRemaining > 0 || session != null && session.EndingChoicePending)) { simulation.StepFinale(Time.fixedDeltaTime, moveIntent); return; }
            if (session != null && session.State.Phase == SessionPhase.Shop)
            {
                simulation.StepRest(Time.fixedDeltaTime, moveIntent);
                if (Shop != null) Shop.ResolveCompanionPositions();
                return;
            }
            if (session != null && session.State.Phase != SessionPhase.Combat) return;
            long simulationStarted = AnimationStressMeasurement.Recording ? System.Diagnostics.Stopwatch.GetTimestamp() : 0;
            // the final boss's fall plays in slow motion: the same simulation, smaller steps
            simulation.Step(Time.fixedDeltaTime * (combatFeel.SlowMotion ? CombatFeel.SlowMotionSpeed : 1f), moveIntent);
            if (AnimationStressMeasurement.Recording) AnimationStressMeasurement.SimulationMilliseconds += (System.Diagnostics.Stopwatch.GetTimestamp() - simulationStarted) * 1000d / System.Diagnostics.Stopwatch.Frequency;
            combatFeel.Play(simulation.Events, units, simulation.VisualElapsed, simulation.Champion != null ? simulation.Champion.Id : 0, FinalBossWave);
            if (pacing != null) pacing.Observe(simulation.Events, simulation.VisualElapsed);
            ToyInk.Observe(simulation.Events);
            ComboInk.Observe(simulation.Events);
            foreach (CombatEvent value in simulation.Events)
            {
                if (stagePerformance != null) stagePerformance.Observe(value);
                if (value.Kind == CombatEventKind.Pickup && value.Value > 0 && (value.Loot == LootKind.Coin || value.Loot == LootKind.Egg)) WalletPulse = 1;
                if (value.Kind != CombatEventKind.Kill || value.Subject == null || deathFeedback == null || value.Subject.Flung) continue; // a flung body flies (ImpactVisual) and bursts where it lands
                float force = 1f + (value.Critical ? 0.7f : 0f) + Mathf.Min(1.3f, value.Value);
                bool shown;
                if (units.TryGetValue(value.SubjectId, out UnitVisual existing)) { existing.Display(simulation.VisualElapsed, 1); shown = deathFeedback.Play(existing, value.Subject, force); }
                else
                {
                    UnitVisual temporary = unitPool.Count > 0 ? unitPool.Pop() : Instantiate(catalog.UnitPrefab, transform).GetComponent<UnitVisual>();
                    temporary.gameObject.SetActive(true);
                    temporary.Bind(value.Subject, catalog);
                    shown = deathFeedback.Play(temporary, value.Subject, force);
                    temporary.gameObject.SetActive(false);
                    unitPool.Push(temporary);
                }
                if (explosions != null) explosions.Observe(value, shown);
            }
            if (explosions != null)
                foreach (CombatEvent value in simulation.Events)
                    if (value.Kind == CombatEventKind.Detonation || value.Kind == CombatEventKind.BossLanded) explosions.Observe(value, true);
            if (arcadeEffects != null)
                foreach (CombatEvent value in simulation.Events)
                {
                    if (paintedEffects == null || !paintedEffects.Play(value)) arcadeEffects.Play(value);
                    if (arsenalVisual != null) arsenalVisual.Play(value, Time.time);
                    if (elementInk != null) elementInk.Observe(value);
                }
            if (combatFeedback != null)
                for (int index = 0; index < simulation.Events.Count; index++) combatFeedback.Play(simulation.Events[index]);
            if (hitWords == null) hitWords = gameObject.AddComponent<HitWords>();
            for (int index = 0; index < simulation.Events.Count; index++) hitWords.Play(simulation, simulation.Events[index]);
            // stats gained for good mid-wave rise over the hero the way they do in the shop (requirement 24/10/2026)
            if (combatFeedback != null && simulation.Champion != null)
                foreach (var gain in simulation.StatGainsToShow)
                    combatFeedback.Label(VisualChampionPosition + Vector2.up * .95f, StatGainText(gain.Key, gain.Value), gain.Value > 0 ? ArcadeSkin.Better : ArcadeSkin.Worse, .6f, 1.3f);
            simulation.StatGainsToShow.Clear();
            if (audioFeedback != null)
            {
                audioFeedback.PlayBatch(simulation.Events);
                toyboxAudio.Observe(simulation);
            }
            if (session != null && (simulation.Phase == RunPhase.Victory || simulation.Phase == RunPhase.Defeat))
            {
                // a won night turns into the shop's room as it settles: its pieces crossfade rather than swap in one frame
                if (simulation.Phase == RunPhase.Victory) SceneryCrossfade.Around(() => session.Settle(simulation), ActorViews()); else session.Settle(simulation);
                if (simulation.Phase == RunPhase.Victory)
                {
                    foreach (UnitVisual visual in units.Values)
                        if (visual.State.Faction != Faction.Enemy) visual.Celebrate(simulation.VisualElapsed);
                    float restored = session.State.Health - simulation.Champion.Health;
                    simulation.Champion.Health = session.State.Health;
                    if (restored > 0 && paintedEffects != null) paintedEffects.Play(new CombatEvent(CombatEventKind.Heal, simulation.Champion.Position, Faction.Champion, restored));
                }
                PersistBoundary();
                movement.Reset();
                if (session.State.Phase == SessionPhase.Shop) transitionRemaining = simulation.BossDefeated ? DeathFeedback.DurationBoss + BossFallHold : 0.45f;
                else resultRevealRemaining = simulation.Phase == RunPhase.Defeat ? 1f : simulation.BossDefeated ? DeathFeedback.DurationBoss : 1.15f;
            }
        }

        private void SetPaused(bool paused)
        {
            if (pauseGate.Manual != paused) CheckpointAudio.PlayInterfaceId(paused ? "ui.pause" : "ui.resume");
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
        public bool ConfigureHeroIdentity(Func<bool> operation)
        {
            if (session == null || !session.CanConfigureHero || TransitionPending || hallExitPending) return false;
            if (!RunTransaction.TryCommit(session, runCatalog, "HERO APPROACH", operation, PersistBoundary, out string error)) { if (!string.IsNullOrEmpty(error)) SaveError = error; return false; }
            CheckpointAudio.PlayInterfaceId("ui.confirm");
            details.Show(session,runCatalog,session.State.Health); return true;
        }
        public void ToggleDetails()
        {
            if (session == null) return;
            if (details.Open) details.Close();
            else details.Show(session, runCatalog, session.State.Phase == SessionPhase.Combat ? simulation.Champion.Health : session.State.Health, session.State.Phase == SessionPhase.Combat ? simulation : null);
            pauseGate.Details = details.Open;
            ApplyPause();
        }
        /// <summary>Saves a settings change made from the pause menu.</summary>
        public bool SaveSettings()
        {
            if (storage == null || profile == null) return true;
            try { storage.SaveProfile(profile); return true; }
            catch (Exception exception) { SaveError = "Settings save failed: " + exception.Message; CheckpointAudio.PlayInterfaceId("ui.error"); return false; }
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
                int previousBestWave = profile.BestWave;
                storage.RecordProgress(profile, session.State, runCatalog);
                int delta = profile.Achievements.Count + profile.SeenWeapons.Count + profile.SeenBosses.Count + profile.SeenChains.Count - before;
                if (delta > 0) CheckpointAudio.PlayInterfaceId("ui.achievement");
                foreach (UnitDefinition champion in runCatalog.Champions)
                    if (previousBestWave < champion.UnlockWave && profile.BestWave >= champion.UnlockWave) { CheckpointAudio.PlayInterfaceId("ui.unlock", 0.15f); break; }
                MetaNotice = delta > 0 ? "SAVED " + delta + " NEW DISCOVERIES / ACHIEVEMENTS · VISIT MUSEUM" : "";
            }
            catch (Exception exception) { SaveError = "Run saved; profile save failed: " + exception.Message; }
            return true;
        }

        private bool Transaction(string title, Func<bool> operation, int cue = 20)
        {
            if (pauseGate.Paused || TransitionPending || hallExitPending) return false;
            bool committed = RunTransaction.TryCommit(session, runCatalog, title, operation, PersistBoundary, out string error);
            if (!committed)
            {
                if (!string.IsNullOrEmpty(error)) SaveError = error;
                if (!string.IsNullOrEmpty(error)) CheckpointAudio.PlayInterfaceId("ui.error");
                return false;
            }
            if (cue >= 0) CheckpointAudio.PlayInterface(cue);
            if (session.State.Phase == SessionPhase.Shop)
            {
                Vector2 position = simulation.Champion.Position;
                Begin(true);
                simulation.Champion.Position = simulation.Champion.PreviousPosition = position;
            }
            return true;
        }

        public bool BuyOffer(int token)
        {
            ShopOffer offer = session.State.Offers.Find(o => o.Token == token) ?? session.State.SpecialOffers.Find(o => o.Token == token);
            RunSnapshot before = MergeReveal.Capture(session.State);
            bool committed = Transaction("Shop purchase", () => session.Buy(token), -1);
            if (committed) GetComponent<SpeechBubbleCast>()?.CommittedPurchase(before, offer);
            return committed;
        }
        public void SetShopDetails(bool expanded) { runHud.SetShopDetails(expanded); }
        public bool ShopDetailsOpen => runHud.ShopDetailsOpen;
        public void PlayCommittedArrival(RunSnapshot before, Vector2 origin)
        {
            Render(1);
            foreach (UnitVisual visual in units.Values)
            {
                UnitState unit = visual.State;
                if (unit.Faction != Faction.Companion) continue;
                bool existed = before.Roster.Exists(entry => entry.UnitId == unit.Definition.Id && entry.WeaponId == unit.Definition.Weapon.Id && entry.Tier == unit.Definition.CombatTier);
                if (!existed)
                {
                    visual.BeginArrival(origin);
                }
            }
        }
        public bool RerollShop() => Transaction("Reroll stock", () => session.Reroll(), 21);
        public bool RefundRelic() => Transaction("Refund relic", () => session.RefundLastRelic(), 21);
        public bool VisitorChoose(int option) => Transaction("Visitor", () => session.VisitorChoose(option), 21);
        public bool VisitorDecline() => Transaction("Visitor leaves", () => session.VisitorDecline(), -1);
        public bool RedeemPawn() => Transaction("Redeem relic", () => session.RedeemPawn(), 21);
        public bool RerollAltar() => Transaction("Reroll altar stock", () => session.RerollAltar(), 21);
        public bool FuseAltar(string id) => Transaction("Altar fusion", () => session.Altar(id), -1);
        public void ShowReceiptFixture() => runHud.ShowReceiptFixture();
        public void ShowInvoiceEvidenceFixture() => runHud.ShowInvoiceEvidenceFixture();
        public void ShowOfferPreviewFixture() => runHud.ShowOfferPreviewFixture();
        public bool BeginHallExit(Vector2 doorway)
        {
            if (session.SecretRoomOpen) return false;
            if (pauseGate.Paused || hallExitPending || TransitionPending || session.State.Phase != SessionPhase.Shop) return false;
            hallExitPending = true; hallExitElapsed = 0; hallExitTarget = doorway + Vector2.down * 1.1f;
            HallDepartureEffect.Play(this, doorway);
            if (Shop != null) Shop.Depart();
            CheckpointAudio.PlayInterfaceId("shop.descend"); return true;
        }
        public bool FinishSpecialShop() => Transaction("Leave dream market", () =>
        {
            if (session.State.Phase != SessionPhase.Shop || session.State.SpecialShopVisited) return false;
            session.State.SpecialShopVisited = true; return true;
        }, -1);
        public void NextWave()
        {
            if (session.SecretRoomOpen) return;
            if (session.State.Phase != SessionPhase.Shop || TransitionPending) return;
            RunSnapshot before = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(session.State));
            if (session.OutsideChallengeAvailable) session.DeclineOutsideStage();
            SceneryCrossfade fade = SceneryCrossfade.Hold(ActorViews()); // the shop's pieces fade out as the night's fade in
            session.ContinueWave();
            if (StageTransfer.ChangesMap(session.State.Wave)) session.RegisterOutsideTransfer();
            if (session.State.Phase != SessionPhase.Combat) { fade?.Cancel(); if (Shop != null) Shop.CancelDeparture(); return; }
            if (!PersistBoundary()) { fade?.Cancel(); session.Restore(before); if (Shop != null) Shop.CancelDeparture(); return; }
            if (Shop != null) Shop.Display();
            if (StageTransfer.ChangesMap(session.State.Wave))
            {
                var carrier = new GameObject("KeeperHandStageTransfer");
                stageTransfer = carrier.AddComponent<StageTransfer>();
                stageTransfer.Configure(this, () => { stageTransfer = null; Begin(true); });
                fade?.Cancel();
            }
            else { ArrivalRift.Expect(); Begin(true); fade?.Release(); ArriveThroughRift(); } // the market has flown off; the brigade steps out of a rift onto the same floor
        }
        /// <summary>Requirement 24/10/2026: the shop came before the boss's death was seen; the fall now plays out with a breath after it.</summary>
        private const float BossFallHold = 1f;
        /// <summary>The start of a night: the hero and friends spring out of a rift of the map's own kind, which then stitches shut.</summary>
        private void ArriveThroughRift()
        {
            if (simulation?.Champion == null || session == null) return;
            Render(1); // the brigade's views exist before they spring out
            Vector2 centre = simulation.Champion.Position + Vector2.up * .9f;
            if (ArrivalRift.Play(MapIdentity.For(session.State.Wave), centre, 1000 - Mathf.RoundToInt(simulation.Champion.Position.y * 12f) - 20) == null) return;
            bool first = true;
            foreach (UnitVisual visual in units.Values)
                if (visual.State != null && visual.State.Faction != Faction.Enemy) { visual.BeginArrival(centre, first); first = false; }
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
        public bool AcceptOutsideChallenge()
        {
            if (PresentationPaused || TransitionPending || hallExitPending) return false;
            RunSnapshot before = MergeReveal.Capture(session.State);
            if (!session.AcceptOutsideStage()) return false;
            if (!PersistBoundary()) { session.Restore(before); return false; }
            var carrier = new GameObject("KeeperHandChallengeEntry");
            stageTransfer = carrier.AddComponent<StageTransfer>();
            stageTransfer.Configure(this, () => { stageTransfer = null; Begin(); });
            return true;
        }
        public bool AcceptSitCloser()
        {
            if (PresentationPaused || TransitionPending || hallExitPending) return false;
            RunSnapshot before = MergeReveal.Capture(session.State);
            if (!session.AcceptSitCloser()) return false;
            if (!PersistBoundary()) { session.Restore(before); return false; }
            var carrier = new GameObject("WarmChairSitsCloser");
            stageTransfer = carrier.AddComponent<StageTransfer>();
            stageTransfer.Configure(this, () => { stageTransfer = null; Begin(); });
            return true;
        }
        public bool ClaimOutsideReward(int index)
        {
            if (PresentationPaused || TransitionPending) return false;
            RunSnapshot before = MergeReveal.Capture(session.State);
            var previousStats = RunBuildReadModel.Capture(session,runCatalog,before.Health).ShopStats;
            if (!session.ClaimOutsideStageReward(index)) return false;
            if (!PersistBoundary()) { session.Restore(before); return false; }
            session.PublishShopStatChanges(previousStats,RunBuildReadModel.Capture(session,runCatalog,session.State.Health).ShopStats);
            Begin(true); return true;
        }
        public void RestartRun()
        {
            if (!PersistBoundary() || ((session.State.Phase == SessionPhase.Victory || session.State.Phase == SessionPhase.Defeat) && !profile.CompletedRuns.Contains(session.State.RunId))) return;
            RunSnapshot before = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(session.State));
            string challenge = session.State.ChallengeId;
            int challengeIndex = Array.IndexOf(RunMeta.ChallengeIds, challenge);
            session.Start(session.State.ChampionId, challengeIndex > 0 ? RunMeta.ChallengeSeed(challengeIndex) : Environment.TickCount, session.State.Difficulty, challenge);
            session.ApplyMeta(PlayAgainLoadout(session.State.ChampionId));
            if (!PersistBoundary()) { session.Restore(before); return; }
            details.Close(); pauseGate.ResetSession(); Begin();
        }
        /// <summary>Requirement 24/10/2026: back to the menu without writing this moment; the run resumes from its last save boundary.</summary>
        /// <summary>Diagnostics only: a scripted stick for recorded clips.</summary>
        public static Vector2? ScriptedMove;
        public void ExitWithoutSaving() { RunLaunch.OpenHub = false; SceneDissolve.Load(session == null ? "Frontend" : "RunFrontend"); }
        public void ReturnToMenu() { if (session != null && !PersistBoundary()) return; RunLaunch.OpenHub = session != null && (session.State.Phase == SessionPhase.Victory || session.State.Phase == SessionPhase.Defeat); SceneDissolve.Load(session == null ? "Frontend" : "RunFrontend"); }
        /// <summary>Play again: the same hero with the Waiting Room's loadout; the Nursery hands over its first friend and the
        /// Wardrobe keeps the choices last made for this hero.</summary>
        private MetaRunState PlayAgainLoadout(string hero)
        {
            MetaProgression.Ensure(profile);
            HeroMastery mastery = MetaProgression.Mastery(profile, hero);
            var friends = MetaProgression.NurseryChoices(profile, runCatalog, runCatalog.Unit(hero));
            return MetaProgression.Loadout(profile, runCatalog, hero, friends.Count > 0 ? friends[0].Id : null, mastery?.AltStarter == true, mastery?.SecondHabit == true, keepsake: profile.Meta.Keepsake);
        }
        /// <summary>The Clock's Rewind: a lost run goes back to the shop before the lost wave (once a run).</summary>
        public bool RewindRun()
        {
            if (session == null || !ResultsReady || !session.RewindReady) return false;
            RunSnapshot lost = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(session.State));
            if (!session.Rewind()) return false;
            MetaProgression.Reopen(profile, lost);
            if (!PersistBoundary()) { session.Restore(lost); return false; }
            CheckpointAudio.PlayInterfaceId("ui.confirm");
            details.Close(); pauseGate.ResetSession(); Begin();
            return true;
        }

        private void OnApplicationFocus(bool focus) { if (!RunPaths.Diagnostic) { pauseGate.FocusLost = !focus; AudioListener.pause = pauseGate.FocusLost || pauseGate.Backgrounded; if (simulation != null) ApplyPause(); } }
        private void OnApplicationPause(bool paused) { pauseGate.Backgrounded = paused; AudioListener.pause = pauseGate.FocusLost || pauseGate.Backgrounded; if (simulation != null) ApplyPause(); if (paused) PersistBoundary(); }

        /// <summary>The views of everything that moves (units, shots, coins): never part of the room.</summary>
        private bool OnScreen(Vector2 at) { Vector3 v = arenaCamera.WorldToViewportPoint(at); return v.x > -.05f && v.x < 1.05f && v.y > -.05f && v.y < 1.05f; }
        private HashSet<Transform> ActorViews()
        {
            var set = new HashSet<Transform>();
            foreach (UnitVisual v in units.Values) if (v != null) set.Add(v.transform);
            foreach (UnitVisual v in unitPool) if (v != null) set.Add(v.transform);
            foreach (SpriteRenderer r in projectileViews.Values) if (r != null) set.Add(r.transform);
            foreach (SpriteRenderer r in pickupViews.Values) if (r != null) set.Add(r.transform);
            return set;
        }
        // the last enemies of a won night: who has started its quiet defeat, and who is playing it (its view hidden meanwhile)
        private readonly HashSet<int> nightEndSeen = new HashSet<int>(), nightEndPlayed = new HashSet<int>();
        private void ClearViews()
        {
            nightEndSeen.Clear(); nightEndPlayed.Clear();
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

        private readonly HashSet<int> heldInCeremony = new HashSet<int>();
        private WaveClearInk waveClearInk;
        private static string StatGainText(ItemEffect effect, float value)
        {
            string name = effect switch { ItemEffect.Health => "Max HP", ItemEffect.Heal => "Regen", ItemEffect.Armor => "Armor", ItemEffect.Damage => "Damage", ItemEffect.Luck => "Luck", ItemEffect.Crit => "Crit", _ => effect.ToString() };
            return name + " " + RunModifierRules.ValueText(effect, value);
        }

        private void Render(float interpolation)
        {
            long renderStarted = AnimationStressMeasurement.Recording ? System.Diagnostics.Stopwatch.GetTimestamp() : 0;
            AnimationDetailBudget.SetLoad(simulation.Units.Count, simulation.Projectiles.Count);
            EffectDensity.SetSceneLoad(simulation.Projectiles.Count, simulation.Units.Count);
            if (spectacleVisual != null) spectacleVisual.Display(simulation);
            if (arenaToyVisual != null) arenaToyVisual.Display(simulation, interpolation);
            EnemyRifts.Draw(80); // over the floor's ink, under every body
            ToyInk.Draw(simulation, simulation.Champion.Position);
            ComboInk.Draw(simulation);
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
                    if (state.Faction == Faction.Enemy && simulation.Phase == RunPhase.Combat && simulation.Elapsed > .3f) EnemyRifts.Play(state, arenaCamera);
                }
                view.ReducedMotion = profile != null && profile.ReducedMotion;
                RaritySparkle.Ensure(view, state);
                // a companion being starred up waits inside its ceremony and walks out of it afterwards
                MergeReveal reveal = Shop != null ? Shop.Reveal : null;
                if (state.Faction == Faction.Companion && reveal != null && reveal.Holds(state, out Vector2 stage))
                {
                    state.Position = state.PreviousPosition = stage;
                    if (heldInCeremony.Add(state.Id)) view.SetTerminalFade(0);
                }
                else if (heldInCeremony.Remove(state.Id)) view.SetTerminalFade(1);
                view.RestAttention = session != null && (session.State.Phase == SessionPhase.Shop || session.EndingChoicePending);
                view.AttentionPoint = state.Faction == Faction.Companion ? simulation.Champion.Position : new Vector2(0, 2);
                float poseTime = simulation.VisualElapsed + terminalVisualAge - (simulation.Phase == RunPhase.Combat || simulation.Phase == RunPhase.Rest ? Time.fixedDeltaTime * (1 - interpolation) : 0);
                view.Display(poseTime, interpolation);
                if (terminalVisualAge > 0 && state.Faction == Faction.Enemy)
                {
                    if (simulation.Phase == RunPhase.Victory)
                    {
                        // requirement 25/10/2026: the enemies left when a night is won play their own defeat as the sweep reaches
                        // them (no burst, no ink pool); off screen, or beyond what the death pool holds, they simply fade
                        float sink = Mathf.Clamp01((terminalVisualAge - WaveClearInk.SweepDelay(state.Position, arenaCamera)) / 0.3f);
                        if (sink > 0 && nightEndSeen.Add(state.Id) && deathFeedback != null && OnScreen(state.Position) && deathFeedback.Play(view, state, 1, true)) nightEndPlayed.Add(state.Id);
                        view.SetTerminalFade(nightEndPlayed.Contains(state.Id) ? 0 : 1 - sink);
                    }
                    else view.SetTerminalFade(Mathf.Clamp01(1 - terminalVisualAge / 0.35f));
                }
            }
            // painted mechanics and weapon moments follow the units as drawn this frame (gauges, muzzles), so they come after them
            if (expandedVisual != null) { expandedVisual.ReducedMotion = profile != null && profile.ReducedMotion; expandedVisual.Display(simulation); }
            if (arsenalVisual != null) arsenalVisual.Display(simulation, Time.time);
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
                Vector2 shotAt = Vector2.Lerp(state.PreviousPosition, state.Position, interpolation);
                if (state.FiredAt >= 0)
                {
                    // requirement 24/10/2026: shots seemed to come out of the hero's body - its first frame was drawn between the
                    // body and the muzzle. A shot is never drawn behind its muzzle, and for its first moments it leaves the
                    // weapon's barrel as that barrel is drawn (painted weapons differ from their simulated muzzle)
                    if (Vector2.Dot(shotAt - state.Muzzle, state.Velocity) < 0) shotAt = state.Muzzle;
                    float shotAge = simulation.Elapsed - state.FiredAt;
                    if (shotAge < .15f && UnitVisual.TryMuzzle(state.SourceUnitId, out Vector2 barrel) && (barrel - state.Muzzle).sqrMagnitude < 1.5f)
                        shotAt += (barrel - state.Muzzle) * (1 - shotAge / .15f);
                }
                view.transform.position = shotAt + state.PatternOffset; // a swaying shot is drawn off its true line
                float projectileSize = state.Faction == Faction.Enemy ? 0.2f : ProjectileVisual.Allied(state.VisualWidth > 0 ? state.VisualWidth : 0.22f);
                Sprite missile = HeroIdentityVisual.Missile(state, catalog.Projectile, simulation.Elapsed, out float missileTurn, out float missileSize);
                if (missile != null) { view.sprite = missile; projectileSize = missileSize; }
                // our painted shots are sized by their painted ink, not their transparent canvas
                float drawn = state.Faction == Faction.Enemy || missile != null ? view.sprite.bounds.size.x : Mathf.Max(0.01f, SpriteInkMetrics.BoundsOf(view.sprite).width);
                view.transform.localScale = Vector3.one * (projectileSize / drawn);
                view.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(state.Velocity.y, state.Velocity.x) * Mathf.Rad2Deg + missileTurn);
                Color projectileColor = state.Color;
                if (state.Faction != Faction.Enemy) projectileColor.a *= EffectDensity.AlliedAlpha;
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
                Sprite lootSprite = state.Kind == LootKind.Egg ? runCatalog?.EggSprite : state.Kind == LootKind.Heart ? runCatalog?.HeartSprite : state.Kind == LootKind.Chest ? runCatalog?.ChestSprite : state.Kind >= LootKind.Flare ? PacingArtwork.For(state.Kind, runCatalog) : catalog.Coin;
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
            if (AnimationStressMeasurement.Recording) AnimationStressMeasurement.RenderMilliseconds += (System.Diagnostics.Stopwatch.GetTimestamp() - renderStarted) * 1000d / System.Diagnostics.Stopwatch.Frequency;
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
            if (StoryOpen) { if (GetComponent<WaitingJourney>()?.Open == true) GetComponent<WaitingJourney>().Draw(arenaCamera); else GetComponent<MienChoPresentation>().Draw(arenaCamera); return; }
            if (session != null)
            {
                bool previousEnabled = GUI.enabled;
                GUI.enabled = previousEnabled && !details.Open && !pauseGate.Navigation;
                runHud.Draw(this, arenaCamera);
                if (!details.Open && !pauseGate.Navigation && nightmare != null) nightmare.Draw(arenaCamera);
                GUI.enabled = previousEnabled;
                if (details.Open) details.Draw(this, arenaCamera);
                else if (pauseGate.Navigation) runHud.DrawMenuConfirmation(this, arenaCamera);
                else DrawTouch();
                GetComponent<MienChoPresentation>()?.Draw(arenaCamera);
                GetComponent<WaitingJourney>()?.Draw(arenaCamera);
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
            GUI.Label(new Rect(70, 1015, 1780, 42), "Move: WASD / arrows / lower-left touch drag. Auto-combat.", label);
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
