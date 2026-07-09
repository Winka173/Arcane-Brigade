using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>
    /// Adaptive soundtrack. Each composition is four sample-aligned looping stems (base / rhythm / melody / intensity)
    /// started on one dspTime, so they cannot drift. Scene changes crossfade at once (a short fade, no wait for a bar line).
    /// If the Arcane music manifest or its clips are missing, the original four licensed tracks play as before.
    /// </summary>
    public sealed class ArcadeMusic : MonoBehaviour
    {
        private const int StemCount = 4;   // base, rhythm, melody, intensity
        /// <summary>Beat clock of the audible piece for UI that moves with the music (see BeatPulse).</summary>
        public static double BeatOriginDsp { get; private set; }
        public static double BeatSeconds { get; private set; } = 0.5;
        public static int BeatsPerBar { get; private set; } = 4;
        public static bool BeatLive { get; private set; }
        /// <summary>0..1 low-health danger: melody/intensity stems fall away so heartbeat and rhythm carry the moment.</summary>
        public static float Danger { get; set; }
        private float dangerSmooth;
        private int dreamDepth;
        private bool dreamDawn;
        public void SetDreamDepth(int depth, bool dawn) { dreamDepth = Mathf.Clamp(depth,0,4); dreamDawn = dawn; }
        public static float DreamStemGain(int depth, int stem, bool dawn)
            => dawn ? 1 : stem == 2 ? Mathf.Lerp(1,.5f,Mathf.Clamp01(depth / 4f)) : stem == 1 ? Mathf.Lerp(1,.85f,Mathf.Clamp01(depth / 4f)) : 1;
        private static readonly List<ArcadeMusic> owners = new List<ArcadeMusic>();

        private sealed class Deck
        {
            public readonly AudioSource[] sources = new AudioSource[StemCount];
            public readonly float[] stemLevel = new float[StemCount];
            public ArcanePieceDef piece;
            public string scene;
            public double startDsp;
            public double fadeStart;
            public float fadeSeconds = 1f;
            public bool fadingOut;
            public float level;
            public bool Live => piece != null;
        }

        // ---- legacy fallback (original licensed tracks)
        private readonly AudioSource[] tracks = new AudioSource[4];
        private int selected;
        private bool legacyLoaded;
        private bool useLegacy;

        // ---- shared state
        private bool paused;
        private bool focused = true;
        private bool backgrounded;
        private bool wasSuspended;
        private double suspendedAt;

        // ---- adaptive state
        private readonly Deck[] decks = new Deck[2];
        private int current = -1;
        private SessionPhase phase = SessionPhase.Defeat;
        private bool hasPhase;
        private bool boss;
        private int wave = 1;
        private int normalWaves = 20;
        private bool endless;
        private bool dreamShop;
        private string forcedScene;
        private string scene = "";
        private float density;
        private float densityTarget;
        private float nextDensityRead;
        private float silentUntil;
        private float busSmooth = 1f;
        private AudioSource ambience;
        private string ambienceId;
        private float bossIntroUntil;

        public string CurrentScene => scene;
        public float Density => density;

        public static string TrackResource(int track)
        {
            switch (track)
            {
                case 0: return "Audio/Licensed/K8-Menu-FusionJazz";
                case 1: return "Audio/Licensed/K8-Shop-CoffeeHouse";
                case 2: return "Audio/Licensed/K8-Combat-JazzClub";
                case 3: return "Audio/Licensed/K8-Boss-BattleTheme";
                default: return null;
            }
        }

        public static int PhaseTrack(SessionPhase phase, bool boss) => phase == SessionPhase.Combat ? (boss ? 3 : 2) : phase == SessionPhase.Shop ? 1 : 0;
        public static float BusGain(bool paused, bool reduced) => (paused ? 0.28f : 1f) * (reduced ? 0.75f : 1f);
        public static float TrackLevel(int track) => track == 1 ? 0.26f : track == 2 ? 0.08f : track == 3 ? 0.13f : 0.18f;

        /// <summary>Relative music level per scene (tuning table; ears decide, see Docs/Audio/Audio_QA_Checklist.md).</summary>
        /// <summary>Requirement 23/10/2026: the music a third louder everywhere.</summary>
        public const float MusicBoost = 1.35f;
        public static float SceneLevel(string scene)
        {
            switch (scene)
            {
                case ArcaneCueLibrary.SceneMenu:
                case ArcaneCueLibrary.SceneHub: return 0.4f;
                case ArcaneCueLibrary.SceneSelect: return 0.35f;
                case ArcaneCueLibrary.SceneBoss: return 0.36f;
                case ArcaneCueLibrary.SceneShopGold: return 0.38f;
                case ArcaneCueLibrary.SceneShopDream: return 0.35f;
                default: return 0.3f;
            }
        }

        /// <summary>Stem mix for a scene at a given enemy density (0..1). Pure function so it can be unit-checked.</summary>
        /// <remarks>The arrangement itself lives in <see cref="MusicArrangement"/> (combat: base + rhythm only).</remarks>
        public static void StemTargets(string scene, float density, float[] levels) => MusicArrangement.Targets(scene, density, false, levels);

        private void Awake()
        {
            for (int index = 0; index < decks.Length; index++) decks[index] = new Deck();
            if (ArcaneCueLibrary.MusicAvailable)
            {
                for (int index = 0; index < decks.Length; index++)
                    for (int stem = 0; stem < StemCount; stem++) decks[index].sources[stem] = MakeSource(true, 210);
                ambience = AudioMix.Route(MakeSource(true, 240), MixBus.Sfx);
            }
            else LoadLegacy();
        }

        private AudioSource MakeSource(bool loop, int priority)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = 0;
            source.volume = 0;
            source.pitch = 1;
            source.priority = priority;
            return AudioMix.Route(source, MixBus.Music);
        }

        private void LoadLegacy()
        {
            if (legacyLoaded) return;
            legacyLoaded = true;
            for (int index = 0; index < tracks.Length; index++)
            {
                AudioSource track = MakeSource(true, 210);
                track.clip = Resources.Load<AudioClip>(TrackResource(index));
                tracks[index] = track;
                if (track.clip == null) Debug.LogWarning("Missing licensed soundtrack: " + TrackResource(index), this);
            }
        }

        private void OnEnable() { owners.Add(this); }

        public void SetPhase(SessionPhase phase, bool paused, bool boss)
        {
            selected = PhaseTrack(phase, boss);
            this.paused = paused;
            if (IsActiveOwner()) CheckpointAudio.SimulationPaused = paused;
            bool bossStarted = boss && !this.boss && hasPhase && phase == SessionPhase.Combat;
            if (hasPhase && phase != this.phase && IsActiveOwner())
            {
                if (phase == SessionPhase.Victory) { CheckpointAudio.PlayInterfaceId("sting.victory"); silentUntil = Time.unscaledTime + 3.6f; }
                else if (phase == SessionPhase.Defeat) { CheckpointAudio.PlayInterfaceId("sting.defeat"); silentUntil = Time.unscaledTime + 3.8f; }
                else if (phase == SessionPhase.Shop && this.phase == SessionPhase.Combat) CheckpointAudio.PlayInterfaceId("sting.waveclear");
            }
            if (bossStarted && IsActiveOwner()) CheckpointAudio.PlayInterfaceId("boss.enter");
            this.phase = phase;
            this.boss = boss;
            hasPhase = true;
        }

        /// <summary>
        /// Adaptive context. Call every frame from the arena (cheap): wave progress picks early/mid/late combat,
        /// <paramref name="dreamShop"/> picks the dream shop piece, enemy count drives layer intensity.
        /// </summary>
        public void SetContext(int wave, int normalWaves, bool endless, bool dreamShop, IReadOnlyList<UnitState> units)
        {
            this.wave = wave; this.normalWaves = normalWaves; this.endless = endless; this.dreamShop = dreamShop;
            if (units != null && Time.unscaledTime >= nextDensityRead)
            {
                nextDensityRead = Time.unscaledTime + 0.25f;
                int alive = 0;
                for (int index = 0; index < units.Count; index++) if (units[index].Faction == Faction.Enemy && units[index].Alive) alive++;
                densityTarget = Mathf.Clamp01((alive - 8) / 50f);
            }
        }

        /// <summary>Menu screens (e.g. character select) can pin a scene; pass null to return to phase-driven selection.</summary>
        public void ForceScene(string sceneId) { forcedScene = sceneId; }

        private bool IsActiveOwner() => owners.Count > 0 && owners[owners.Count - 1] == this;

        private string DesiredScene()
        {
            if (Time.unscaledTime < silentUntil) return "";
            if (!string.IsNullOrEmpty(forcedScene)) return forcedScene;
            return ArcaneCueLibrary.SceneFor(hasPhase ? phase : SessionPhase.Defeat, boss, wave, normalWaves, endless, dreamShop);
        }

        private void Update()
        {
            AudioMix.Tick();
            bool active = IsActiveOwner();
            bool suspended = !focused || backgrounded || AudioListener.pause;
            double dsp = AudioSettings.dspTime;
            if (suspended && !wasSuspended) suspendedAt = dsp;
            if (!suspended && wasSuspended) ShiftClocks(dsp - suspendedAt);
            wasSuspended = suspended;

            float dt = Time.unscaledDeltaTime;
            float busTarget = MusicBoost * AudioMix.SourceGain(MixBus.Music) * PresentationSettings.MusicDuck * BusGain(paused, PresentationSettings.ReducedAudio) * (NightmareHaunting.Quiet ? .3f : 1);
            if (WaitingRealmStage.BedsideLullabyActive) busTarget = 0; // the familiar, incomplete lullaby takes over this encounter
            if (PresentationSettings.MusicVolume <= 0f) busSmooth = 0f;
            else busSmooth += (busTarget - busSmooth) * (1f - Mathf.Exp(-dt * (busTarget < busSmooth ? 7f : 3f)));

            if (!active) { StopAll(); return; }
            if (suspended) return;

            useLegacy = !ArcaneCueLibrary.MusicAvailable || ambience == null;
            if (useLegacy) { StopAdaptive(); UpdateLegacy(busSmooth); UpdateAmbience(dt); return; }

            density = density < densityTarget ? Mathf.MoveTowards(density, densityTarget, dt * 0.35f) : Mathf.MoveTowards(density, densityTarget, dt * 0.08f);
            string desired = DesiredScene();
            if (desired != scene) ChangeScene(desired, dsp);
            UpdateDecks(dsp, busSmooth, dt);
            UpdateAmbience(dt);
        }

        // ------------------------------------------------------------------ adaptive
        /// <summary>The run's seed: each run starts its map's rotation of pieces in a different place.</summary>
        public int RunSeed;
        /// <summary>The sound key of the boss on the field (ArcaneCueLibrary.BossKey), or null.</summary>
        public string BossKey;
        private readonly Dictionary<string, int> entries = new Dictionary<string, int>();
        private int launchOffset = -1;

        /// <summary>Which of a scene's pieces to play (requirement 26/10/2026: a scene now has several). A map's fights go through its
        /// pieces by the night's place in the map (boss nights do not count), the boss nights by the boss's number, shops and endless
        /// by the wave, all shifted by the run's seed; menus, hero select and the hub alternate each time they are entered.</summary>
        private int PieceVariant(string scene)
        {
            int shift = Mathf.Abs(RunSeed % 1000);
            if (scene == ArcaneCueLibrary.SceneBoss) return wave / 4 + shift;
            if (scene.StartsWith("combat_map_", System.StringComparison.Ordinal))
            {
                int ordinal = 0;
                for (int night = MapIdentity.FirstWave(wave); night < wave; night++) if (!ActOneCanon.BossWave(night)) ordinal++;
                return ordinal + shift;
            }
            if (scene == ArcaneCueLibrary.SceneMenu || scene == ArcaneCueLibrary.SceneSelect || scene == ArcaneCueLibrary.SceneHub)
            {
                if (launchOffset < 0) launchOffset = UnityEngine.Random.Range(0, 1000);
                entries.TryGetValue(scene, out int visits); entries[scene] = visits + 1;
                return launchOffset + visits;
            }
            return wave + shift;
        }

        private void ChangeScene(string desired, double dsp)
        {
            scene = desired;
            if (desired == ArcaneCueLibrary.SceneBoss) bossIntroUntil = Time.unscaledTime + MusicArrangement.BossIntroSeconds; // the boss's melody only in its intro
            // a boss with a piece of its own (boss_<key>) plays that one, any other boss one of the shared boss pieces
            ArcanePieceDef piece = desired == ArcaneCueLibrary.SceneBoss && !string.IsNullOrEmpty(BossKey) ? ArcaneCueLibrary.PieceForScene("boss_" + BossKey, 0) : null;
            if (piece == null && !string.IsNullOrEmpty(desired)) piece = ArcaneCueLibrary.PieceForScene(desired, PieceVariant(desired));
            Deck live = current >= 0 && decks[current].Live ? decks[current] : null;
            if (piece == null)
            {
                if (live != null) BeginFade(live, dsp, 0.7f, true);
                return;
            }
            if (live != null && live.piece == piece && !live.fadingOut) { live.scene = desired; return; }
            AudioClip[] stems = ArcaneCueLibrary.LoadStems(piece);
            if (stems == null) { StopAdaptive(); return; } // clips missing: MusicAvailable is now false, legacy tracks take over next frame

            // requirement 25/10/2026 (the change lagged behind the scene): the new piece starts at once with a short crossfade,
            // never waiting for the departing piece's bar line
            double start = dsp + 0.05;
            float fade = 0.35f;
            int slot = current < 0 ? 0 : 1 - current;
            Deck next = decks[slot];
            if (next.Live) KillDeck(next);
            next.piece = piece; next.scene = desired; next.startDsp = start; next.fadingOut = false; next.level = 0f;
            next.fadeStart = start; next.fadeSeconds = fade;
            for (int stem = 0; stem < StemCount; stem++)
            {
                AudioSource source = next.sources[stem];
                source.Stop();
                source.clip = stems[stem];
                source.loop = true;
                source.volume = 0f;
                next.stemLevel[stem] = 0f;
                source.PlayScheduled(start);
            }
            if (live != null) BeginFade(live, start, fade, true);
            current = slot;
        }

        private static void BeginFade(Deck deck, double at, float seconds, bool outgoing)
        {
            deck.fadeStart = at; deck.fadeSeconds = Mathf.Max(0.05f, seconds); deck.fadingOut = outgoing;
        }

        private void KillDeck(Deck deck)
        {
            foreach (AudioSource source in deck.sources) if (source != null) { source.Stop(); source.volume = 0f; }
            deck.piece = null; deck.level = 0f;
        }

        private readonly float[] targets = new float[StemCount];

        private void UpdateDecks(double dsp, float bus, float dt)
        {
            Deck audible = current >= 0 && current < decks.Length && decks[current].Live ? decks[current] : null;
            BeatLive = audible != null;
            if (audible != null)
            {
                BeatOriginDsp = audible.startDsp;
                BeatsPerBar = Mathf.Max(1, audible.piece.beatsPerBar);
                BeatSeconds = Math.Max(0.05, audible.piece.BarSeconds / BeatsPerBar);
            }
            dangerSmooth = Mathf.MoveTowards(dangerSmooth, Mathf.Clamp01(Danger), dt * 1.5f);
            for (int index = 0; index < decks.Length; index++)
            {
                Deck deck = decks[index];
                if (!deck.Live) continue;
                float progress = Mathf.Clamp01((float)((dsp - deck.fadeStart) / deck.fadeSeconds));
                float envelope = deck.fadingOut ? Mathf.Cos(progress * Mathf.PI * 0.5f) : Mathf.Sin(progress * Mathf.PI * 0.5f);
                if (deck.fadingOut && progress >= 1f) { KillDeck(deck); if (current == index) current = -1; continue; }
                MusicArrangement.Targets(deck.scene, density, Time.unscaledTime < bossIntroUntil, targets);
                for (int stem = 0; stem < StemCount; stem++) targets[stem] *= DreamStemGain(dreamDepth,stem,dreamDawn);
                if (dangerSmooth > 0f) { targets[2] *= 1f - 0.65f * dangerSmooth; targets[3] *= 1f - 0.85f * dangerSmooth; targets[0] *= 1f - 0.2f * dangerSmooth; }
                float sceneLevel = SceneLevel(deck.scene);
                for (int stem = 0; stem < StemCount; stem++)
                {
                    deck.stemLevel[stem] = Mathf.MoveTowards(deck.stemLevel[stem], targets[stem], dt * (targets[stem] > deck.stemLevel[stem] ? 0.45f : 0.2f));
                    deck.sources[stem].volume = envelope * deck.stemLevel[stem] * sceneLevel * bus;
                }
            }
        }

        private void ShiftClocks(double suspendedFor)
        {
            foreach (Deck deck in decks) if (deck.Live) { deck.startDsp += suspendedFor; deck.fadeStart += suspendedFor; }
        }

        private void StopAdaptive()
        {
            for (int index = 0; index < decks.Length; index++) if (decks[index].Live) KillDeck(decks[index]);
            current = -1; scene = "";
        }

        // ------------------------------------------------------------------ ambience (one looping bed under the music)
        private static string AmbienceFor(string scene)
        {
            switch (scene)
            {
                case ArcaneCueLibrary.SceneShopGold: return "amb.goldshop";
                case ArcaneCueLibrary.SceneShopDream: return "amb.dreamshop";
                case ArcaneCueLibrary.SceneMenu:
                case ArcaneCueLibrary.SceneHub:
                case ArcaneCueLibrary.SceneSelect:
                case "": return null;
                default: return "amb.stage";
            }
        }

        private void UpdateAmbience(float dt)
        {
            if (ambience == null) return;
            string wanted = AmbienceFor(scene);
            ArcaneCueDef def = ArcaneCueLibrary.Get(wanted ?? ambienceId);
            float target = 0f;
            if (wanted != null && def != null)
            {
                target = def.gain * CheckpointAudio.BusTrim(AudioBus.Ambience) * AudioMix.SourceGain(MixBus.Sfx) * (PresentationSettings.ReducedAudio ? 0.5f : 1f)
                    * (paused ? 0.3f : 1f) * (Time.unscaledTime < PresentationSettings.DangerUntil ? 0.7f : 1f);
            }
            if (wanted != ambienceId)
            {
                ambience.volume = Mathf.MoveTowards(ambience.volume, 0f, dt * 0.25f);
                if (ambience.volume > 0.001f) return;
                ambience.Stop();
                ambienceId = wanted;
                ArcaneCueDef next = ArcaneCueLibrary.Get(wanted);
                AudioClip clip = next != null ? ArcaneCueLibrary.Pick(next) : null;
                if (clip == null) { ambienceId = wanted; return; }
                ambience.clip = clip; ambience.loop = true; ambience.volume = 0f; ambience.Play();
                return;
            }
            if (ambienceId == null) { if (ambience.isPlaying) ambience.Stop(); return; }
            ambience.volume = Mathf.MoveTowards(ambience.volume, target, dt * (target < ambience.volume ? 0.5f : 0.12f));
        }

        // ------------------------------------------------------------------ legacy fallback
        private void UpdateLegacy(float busGainValue)
        {
            LoadLegacy();
            for (int index = 0; index < tracks.Length; index++)
            {
                AudioSource track = tracks[index];
                float target = index == selected ? TrackLevel(index) * busGainValue : 0;
                if (target > 0 && !track.isPlaying && track.clip != null) track.Play();
                track.volume = Mathf.MoveTowards(track.volume, target, Time.unscaledDeltaTime * (target < track.volume ? 3f : 2.5f)); // the new track comes in at once (requirement 25/10/2026)
                if (target == 0 && track.volume == 0 && track.isPlaying) track.Stop();
            }
        }

        private void StopAll()
        {
            StopAdaptive();
            if (ambience != null) { ambience.Stop(); ambience.volume = 0f; ambienceId = null; }
            foreach (AudioSource track in tracks) if (track != null) { track.Stop(); track.volume = 0; }
        }

        private void OnApplicationFocus(bool focus) { focused = focus; SetSuspended(); }
        private void OnApplicationPause(bool pause) { backgrounded = pause; SetSuspended(); }
        private void SetSuspended()
        {
            bool suspend = !focused || backgrounded;
            foreach (Deck deck in decks)
                foreach (AudioSource source in deck.sources) { if (source == null) continue; if (suspend) source.Pause(); else source.UnPause(); }
            if (ambience != null) { if (suspend) ambience.Pause(); else ambience.UnPause(); }
            foreach (AudioSource track in tracks)
            {
                if (track == null) continue;
                if (suspend) track.Pause();
                else track.UnPause();
            }
        }

        private void OnDisable()
        {
            owners.Remove(this);
            CheckpointAudio.SimulationPaused = false;
            StopAll();
        }
    }
}
