using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class CheckpointAudio : MonoBehaviour
    {
        private static readonly List<CheckpointAudio> interfaceOwners = new List<CheckpointAudio>();
        private readonly AudioSource[] voices = new AudioSource[16];
        private readonly AudioClip[] clips = new AudioClip[39];
        private readonly bool[] ownedClips = new bool[39];
        private readonly float[] nextPlayback = new float[39];
        private readonly float[] nextDangerPlayback = new float[39];
        private readonly float[] voiceGain = new float[16];
        private readonly string[] voiceId = new string[16];
        private readonly Dictionary<string, float> nextById = new Dictionary<string, float>();
        private readonly Dictionary<string, float> nextByGroup = new Dictionary<string, float>();
        private readonly List<PendingCue> pending = new List<PendingCue>();
        private readonly Dictionary<int, float> recentHitRoots = new Dictionary<int, float>();
        private Camera cullCamera;
        private bool arenaOwner;

        private struct PendingCue { public string Id; public float At; public bool Interface; }
        private float scheduleClock;

        /// <summary>True while the simulation is paused (pause menu, details, focus loss). Set every frame by ArcadeMusic.SetPhase.</summary>
        public static bool SimulationPaused { get; set; }
        private float nextCosmetic;
        private float nextImpact;      // hits, kills and explosions have their own lane, so attack sounds never silence them
        private float coinStreakAt = -10;
        private int coinStreak;
        public const int CombatLanes = 12, ReducedCombatLanes = 4;
        private float nextDanger;
        private float nextInterface;
        private int dangerVoice;
        private int interfaceVoice;
        private bool presentation;
        private bool focused = true;
        private bool backgrounded;
        public int PlayedCues { get; private set; }
        public int DroppedCosmeticCues { get; private set; }

        private void Awake()
        {
            for (int index = 0; index < voices.Length; index++)
            {
                voices[index] = gameObject.AddComponent<AudioSource>();
                voices[index].playOnAwake = false;
                voices[index].spatialBlend = 0;
                voices[index].volume = 0;
                AudioMix.Route(voices[index], index >= 2 && index < 4 ? MixBus.UI : MixBus.Sfx); // re-routed per cue at play time
            }
            arenaOwner = GetComponent<ArenaController>() != null;
            ConfigurePresentation();
        }

        private void OnEnable() { interfaceOwners.Add(this); }

        public void ConfigurePresentation()
        {
            // the licensed Cue-* clips only back the legacy path: with the cue library live they are never loaded or played (playtest item 2a)
            if (presentation || ArcaneCueLibrary.Available) return;
            presentation = true;
            for (int index = 0; index < clips.Length; index++) clips[index] = ArcadeSound.AcquireCue(index, out ownedClips[index]);
        }

        public void Play(CombatEventKind kind)
        {
            PlayCue(kind == CombatEventKind.Pickup ? 4 : kind == CombatEventKind.Hit ? 3 : kind == CombatEventKind.Fire ? 0 : -1);
        }

        public static int EventCue(CombatEvent value)
        {
            bool boss = value.Subject != null && value.Subject.Definition != null && value.Subject.Definition.IsBoss;
            if (value.Kind == CombatEventKind.Kill)
                return value.Faction == Faction.Champion || boss ? 6 : value.Subject != null && value.Subject.Definition != null ? ArcadeSound.MaterialCue(value.Subject.Definition.Material, true) : 5;
            if (value.Kind == CombatEventKind.Hit)
                return value.Faction != Faction.Champion && value.Subject != null && value.Subject.Definition != null ? ArcadeSound.MaterialCue(value.Subject.Definition.Material, false) : 3;
            if (value.Kind == CombatEventKind.ChestOpened) return 20;
            if (value.Kind == CombatEventKind.Pickup) return 4;
            if (value.Kind == CombatEventKind.SignaturePulse) return value.Weapon != null && value.Weapon.Signature == SignatureWeapon.Bell ? 7 : 8;
            if (value.Kind == CombatEventKind.SignatureBurst) return 9;
            if (value.Kind != CombatEventKind.Fire || value.Weapon == null) return -1;
            if (value.Weapon.Family != ExpandedWeapon.None) return 9 + (int)value.Weapon.Family;
            return value.Weapon.Kind == WeaponKind.Melee ? 0 : value.Weapon.Motion == WeaponMotion.Bow ? 1 : 2;
        }

        public void Play(CombatEvent value)
        {
            MaterialAudio.Observe(value);
            if (ArcaneCueLibrary.Available)
            {
                float pitch = ArcaneCueLibrary.WeaponPitch(value);
                // a splat, a flung body's bump or its landing sounds like what the enemy is made of (MaterialAudio)
                string material = MaterialAudio.CueFor(value, out float materialGain, out float materialPitch);
                float scale = 1f;
                string id = material ?? ArcaneCueLibrary.EventId(value, out scale);
                if (material != null) { scale = materialGain; pitch = materialPitch; }
                if (value.Kind == CombatEventKind.ProjectilePierce) pitch = RangedFeel.PiercePitch(value, Time.unscaledTime); // each pierce a step higher
                if (value.Kind == CombatEventKind.Hit && value.Faction == Faction.Enemy && value.Weapon != null && value.Weapon.Kind == WeaponKind.Projectile && RangedFeel.TryShooter(value.Cause.SourceUnitId, out Vector2 shooter))
                {
                    float near = Mathf.InverseLerp(6f, 1.5f, (shooter - value.Position).magnitude); // 1 point-blank .. 0 far
                    scale *= Mathf.Lerp(.72f, 1.12f, near); pitch *= Mathf.Lerp(.94f, 1.06f, near);
                }
                if (id != null && ArcaneCueLibrary.Get(id) != null)
                {
                    ArcaneCueDef def = ArcaneCueLibrary.Get(id);
                    if (!def.danger && !OnScreen(value.Position)) { DroppedCosmeticCues++; return; }
                    if (value.Kind == CombatEventKind.Hit && !def.danger && IsDuplicateHit(value)) { DroppedCosmeticCues++; return; }
                    if (material != null && !MaterialAudio.Allow(Time.unscaledTime)) { DroppedCosmeticCues++; return; } // a swarm never spams
                    if (PlayLibrary(def, scale, Pan(value.Position), value.Critical, pitch, value.Faction == Faction.Champion)) return;
                }
            }
            // the library deliberately left these silent (pickups: requirement 22/10/2026; a pulse of a weapon without a signature):
            // never fall back to the old cue table for them
            if (ArcaneCueLibrary.Available && (value.Kind == CombatEventKind.Pickup || value.Kind == CombatEventKind.SignaturePulse || value.Kind == CombatEventKind.SignatureBurst)) return;
            bool danger = value.Faction == Faction.Champion && value.Kind == CombatEventKind.Hit;
            PlayCue(EventCue(value), danger, value.Critical);
        }

        /// <summary>
        /// Preferred entry for a simulation step: plays every event and adds the one sound the event stream cannot express,
        /// the hero's whiff (melee Fire with no Hit sharing its Cause.RootId in the same step).
        /// </summary>
        public void PlayBatch(IReadOnlyList<CombatEvent> events)
        {
            // what landed is heard before what was swung: kills, then critical hits, then hits, then attacks
            for (int pass = 0; pass < 4; pass++)
                for (int index = 0; index < events.Count; index++)
                {
                    CombatEvent value = events[index];
                    int order = value.Kind == CombatEventKind.Kill ? 0 : value.Kind == CombatEventKind.Hit ? (value.Critical ? 1 : 2) : value.Kind == CombatEventKind.Fire ? 3 : -1;
                    if (order == pass) Play(value);
                }
            for (int index = 0; index < events.Count; index++)
            {
                CombatEvent value = events[index];
                if (value.Kind != CombatEventKind.Fire && value.Kind != CombatEventKind.Kill && value.Kind != CombatEventKind.Hit) Play(value);
                if (value.Kind == CombatEventKind.LootDrop && value.Loot == LootKind.Coin && OnScreen(value.Position)) PlayInterfaceId("loot.coin.bounce", 0.1f);
                if (value.Kind == CombatEventKind.BossPhase) PlayInterfaceId("fx.hazard.warn", 0.1f);
                if (value.Kind == CombatEventKind.BossLanded && value.Subject != null && value.Subject.Definition != null)
                {   // the boss's own entrance plays first (ArcaneCueLibrary.EventId); its voice follows it
                    string voice = ArcaneCueLibrary.BossVoiceId(value.Subject.Definition.Id);
                    if (voice != null && ArcaneCueLibrary.EventId(value) != voice) PlayInterfaceId(voice, 0.9f);
                }
                if (value.Kind != CombatEventKind.Fire || value.Faction != Faction.Champion || value.Weapon == null || value.Weapon.Kind != WeaponKind.Melee || value.Cause.RootId == 0) continue;
                bool connected = false;
                for (int other = 0; other < events.Count && !connected; other++) connected = events[other].Kind == CombatEventKind.Hit && events[other].Cause.RootId == value.Cause.RootId;
                if (!connected && ArcaneCueLibrary.Available) PlayInterfaceId("cmb.miss", 0.07f);
            }
        }

        // One hit sound per attack: a cone swing that touches ten enemies shares one Cause.RootId.
        private bool IsDuplicateHit(CombatEvent value)
        {
            if (value.Cause.RootId == 0 || value.Critical) return false;
            float now = Time.unscaledTime;
            if (recentHitRoots.TryGetValue(value.Cause.RootId, out float previous) && now - previous < 0.09f) return true;
            if (recentHitRoots.Count >= 512) recentHitRoots.Clear();
            recentHitRoots[value.Cause.RootId] = now;
            return false;
        }

        /// <summary>Stereo placement from the event's horizontal screen position (centre = 0, edges = +-0.75).</summary>
        private float Pan(Vector2 position)
        {
            if (!arenaOwner || cullCamera == null) return 0f;
            float x = cullCamera.WorldToViewportPoint(new Vector3(position.x, position.y, 0f)).x;
            return Mathf.Clamp((x - 0.5f) * 1.5f, -0.75f, 0.75f);
        }

        /// <summary>Hits, kills and explosions: the sounds that say something landed.</summary>
        public static bool IsImpact(string id) => id.StartsWith("cmb.hit") || id == "cmb.crit" || id.StartsWith("enemy.die") || id == "enemy.hurt" || id.Contains("boom") || id.StartsWith("fx.explode");
        /// <summary>Only the hero being hurt and boss moments lower the music; ordinary enemy attacks keep their priority but do not duck.</summary>
        public static bool Ducks(string id) => id.StartsWith("plr.") || id.StartsWith("boss.") || id == "ui.cutin.boss" || id == "event.warn";

        // Arena only: sounds from outside the visible area (plus margin) are dropped unless they are danger cues.
        private bool OnScreen(Vector2 position)
        {
            if (!arenaOwner) return true;
            if (cullCamera == null)
            {
                ArenaCamera arena = FindAnyObjectByType<ArenaCamera>();
                if (arena == null) return true;
                cullCamera = arena.GetComponent<Camera>();
                if (cullCamera == null) return true;
            }
            Vector3 view = cullCamera.WorldToViewportPoint(new Vector3(position.x, position.y, 0f));
            return view.x > -0.12f && view.x < 1.12f && view.y > -0.12f && view.y < 1.12f;
        }

        public static bool IsDangerCue(int cue) => cue == 6 || cue == 24 || cue == 25 || cue == 26;
        public static float GroupDelay(int cue, bool reduced) => (cue >= 27 ? 0.12f : cue >= 10 ? 0.09f : cue == 4 ? 0.08f : 0.04f) * (reduced ? 2.5f : 1);
        public static float CosmeticInterval(bool reduced) => reduced ? 0.085f : 0.035f;
        public static float ImpactInterval(bool reduced) => reduced ? 0.04f : 0.012f;
        public static float CueGain(int cue, bool danger, bool reduced) => danger ? 0.25f : (cue == 4 ? 0.09f : cue == 19 ? 0.08f : cue >= 33 ? 0.1f : 0.14f) * (reduced ? 0.65f : 1f);

        public static int AllocateVoice(bool danger, bool ui, bool reduced, int cursor, int occupied)
        {
            if (danger) return cursor % 2;
            if (ui) return 2 + cursor % 2;
            int capacity = reduced ? ReducedCombatLanes : CombatLanes;
            for (int index = 4; index < 4 + capacity; index++)
                if ((occupied & (1 << index)) == 0) return index;
            return -1;
        }

        public static int VoicePriority(bool danger, bool ui, bool critical) => danger ? 16 : ui ? 64 : critical ? 120 : 180;

        public void PlayCue(int cue, bool danger = false, bool critical = false) { PlayCueOnBus(cue, danger, critical, cue >= 19 && cue <= 23); }

        private void PlayCueOnBus(int cue, bool danger, bool critical, bool ui)
        {
            if (interfaceOwners.Count == 0 || interfaceOwners[interfaceOwners.Count - 1] != this) return;
            if (ArcaneCueLibrary.Available)
            {
                string libraryId = ArcaneCueLibrary.LegacyId(cue);
                if (libraryId != null && PlayLibrary(ArcaneCueLibrary.Get(libraryId))) return;
            }
            if (!isActiveAndEnabled || !focused || backgrounded || AudioListener.pause || (!ui && Time.timeScale <= 0)) return;
            ConfigurePresentation();
            if (cue < 0 || cue >= clips.Length || clips[cue] == null) return;
            danger |= IsDangerCue(cue);
            ui &= !danger;
            float now = Time.unscaledTime;
            bool reduced = PresentationSettings.ReducedAudio;
            float[] clock = danger ? nextDangerPlayback : nextPlayback;
            if (now < clock[cue] || now < (danger ? nextDanger : ui ? nextInterface : nextCosmetic))
            {
                if (!danger) DroppedCosmeticCues++;
                return;
            }
            int occupied = 0;
            for (int index = 4; index < voices.Length; index++) if (voices[index].isPlaying) occupied |= 1 << index;
            int voiceIndex = AllocateVoice(danger, ui, reduced, danger ? dangerVoice : interfaceVoice, occupied);
            if (voiceIndex < 0) { DroppedCosmeticCues++; return; }
            if (danger) dangerVoice = (dangerVoice + 1) % 2;
            else if (ui) interfaceVoice = (interfaceVoice + 1) % 2;
            clock[cue] = now + (danger ? cue == 3 ? 0.18f : 0.4f : GroupDelay(cue, reduced));
            if (danger) nextDanger = now + 0.06f;
            else if (ui) nextInterface = now + (cue == 19 ? 0.1f : 0.06f);
            else nextCosmetic = now + CosmeticInterval(reduced);
            AudioSource voice = voices[voiceIndex];
            voice.Stop();
            voice.clip = clips[cue];
            voiceGain[voiceIndex] = CueGain(cue, danger, false);
            voice.volume = PlaybackGain(voiceIndex);
            voice.priority = VoicePriority(danger, ui, critical);
            voice.pitch = critical && !danger && !ui ? 1.06f : 1;
            AudioMix.Route(voice, ui ? MixBus.UI : MixBus.Sfx);
            voice.Play();
            AudioMix.ReportSfx(voiceGain[voiceIndex], ui ? MixBus.UI : MixBus.Sfx);
            PlayedCues++;
            if (danger) PresentationSettings.Danger();
        }

        /// <summary>Play any cue by stable id on the top audio owner (see Docs/Audio/Audio_Cue_Contract.md).</summary>
        public static void PlayInterfaceId(string id)
        {
            if (interfaceOwners.Count > 0) interfaceOwners[interfaceOwners.Count - 1].PlayLibrary(ArcaneCueLibrary.Get(id));
        }

        /// <summary>Same as PlayInterfaceId but starts after <paramref name="delay"/> seconds (unscaled), to land a clip's impact on an animation beat.</summary>
        public static void PlayInterfaceId(string id, float delay)
        {
            if (delay <= 0f) { PlayInterfaceId(id); return; }
            if (interfaceOwners.Count > 0)
            {
                CheckpointAudio owner = interfaceOwners[interfaceOwners.Count - 1];
                ArcaneCueDef def = ArcaneCueLibrary.Get(id);
                if (def == null) return;
                bool ui = def.busId == AudioBus.UI && !def.danger;
                float at = (ui ? Time.unscaledTime : owner.scheduleClock) + delay;
                if (owner.pending.Count >= 64 || owner.pending.Exists(cue => cue.Id == id && Mathf.Abs(cue.At - at) < 0.04f)) return;
                owner.pending.Add(new PendingCue { Id = id, At = at, Interface = ui });
            }
        }

        public static void CancelScheduledPrefix(string prefix)
        {
            if (interfaceOwners.Count > 0) interfaceOwners[interfaceOwners.Count - 1].pending.RemoveAll(cue => cue.Id.StartsWith(prefix, System.StringComparison.Ordinal));
        }

        /// <summary>World-positioned cue (culled when off-screen unless it is a danger cue). Use for projectile bounce/split/return, loot drop, knockback, hazards.</summary>
        public static void PlayWorldId(string id, Vector2 position)
        {
            if (interfaceOwners.Count == 0) return;
            CheckpointAudio owner = interfaceOwners[interfaceOwners.Count - 1];
            ArcaneCueDef def = ArcaneCueLibrary.Get(id);
            if (def == null) return;
            if (!def.danger && !owner.OnScreen(position)) { owner.DroppedCosmeticCues++; return; }
            owner.PlayLibrary(def);
        }

        /// <summary>World-positioned cue at a given gain and pitch (a home run's ting, a charged shot's thup).</summary>
        public static void PlayWorldId(string id, Vector2 position, float gainScale, float pitch)
        {
            if (interfaceOwners.Count == 0) return;
            CheckpointAudio owner = interfaceOwners[interfaceOwners.Count - 1];
            ArcaneCueDef def = ArcaneCueLibrary.Get(id);
            if (def == null) return;
            if (!def.danger && !owner.OnScreen(position)) { owner.DroppedCosmeticCues++; return; }
            owner.PlayLibrary(def, gainScale, owner.Pan(position), false, pitch);
        }

        public void PlayId(string id) { PlayLibrary(ArcaneCueLibrary.Get(id)); }

        // Returns true when the cue was handled (played or deliberately dropped); false asks the caller to use the legacy path.
        private bool PlayLibrary(ArcaneCueDef def, float gainScale = 1f, float pan = 0f, bool critical = false, float voicePitch = 1f, bool heroSource = false)
        {
            if (!ArcaneCueLibrary.Enabled) return false;
            if (def == null || def.clips == null || def.clips.Length == 0) return false;
            if (interfaceOwners.Count == 0 || interfaceOwners[interfaceOwners.Count - 1] != this) return true;
            bool danger = def.danger;
            bool ui = def.busId == AudioBus.UI && !danger;
            if (!isActiveAndEnabled || !focused || backgrounded || AudioListener.pause || (!ui && (Time.timeScale <= 0 || SimulationPaused))) return true;
            bool reduced = PresentationSettings.ReducedAudio;
            if (reduced && !danger && def.priority >= 180) return true;
            float now = Time.unscaledTime;
            if (nextById.TryGetValue(def.id, out float until) && now < until) { if (!danger) DroppedCosmeticCues++; return true; }
            bool choreography = def.id.StartsWith("shop.carpet.") || def.id.StartsWith("shop.keeper.") || def.id.StartsWith("up.ally.") || def.id.StartsWith("up.star") || def.id.StartsWith("up.merge");
            bool impact = !danger && !ui && IsImpact(def.id);
            if (!choreography && now < (danger ? nextDanger : ui ? nextInterface : impact ? nextImpact : nextCosmetic)) { if (!danger) DroppedCosmeticCues++; return true; }
            string group = null;
            if (!danger && !ui)
            {
                group = GroupOf(def.id);
                if (nextByGroup.TryGetValue(group, out float groupUntil) && now < groupUntil) { DroppedCosmeticCues++; return true; }
            }
            int occupied = 0;
            int sameCue = 0;
            for (int index = 0; index < voices.Length; index++)
            {
                if (!voices[index].isPlaying) continue;
                if (index >= 4) occupied |= 1 << index;
                if (voiceId[index] == def.id) sameCue++;
            }
            if (!danger && sameCue >= Mathf.Max(1, def.maxVoices)) { DroppedCosmeticCues++; return true; }
            AudioClip clip = ArcaneCueLibrary.Pick(def);
            if (clip == null) return false;
            // hero, boss and UI sounds outrank the swarm: they may take a swarm lane, a swarm hit never takes theirs (SfxGovernor)
            SfxRole role = SfxGovernor.RoleOf(def.id, def.busId, heroSource);
            int rolePriority = SfxGovernor.Priority(role, def.priority);
            int voiceIndex = AllocateVoice(danger, ui, reduced, danger ? dangerVoice : interfaceVoice, occupied);
            if (voiceIndex < 0 && !danger && !ui && rolePriority <= 120) voiceIndex = StealVoice(rolePriority, reduced);
            if (voiceIndex < 0) { DroppedCosmeticCues++; return true; }
            // per-clip voice limit (4 at once) and 40 ms minimum retrigger
            if (!danger && !SfxGovernor.Admit(clip, now, role)) { DroppedCosmeticCues++; return true; }
            if (danger) dangerVoice = (dangerVoice + 1) % 2;
            else if (ui) interfaceVoice = (interfaceVoice + 1) % 2;
            nextById[def.id] = now + def.cooldown * (reduced && !danger ? 2f : 1f);
            if (group != null) nextByGroup[group] = now + GroupInterval(group, reduced);
            if (danger) nextDanger = now + 0.06f;
            else if (ui) nextInterface = now + 0.04f;
            else if (impact) nextImpact = now + ImpactInterval(reduced);
            else nextCosmetic = now + CosmeticInterval(reduced);
            AudioSource voice = voices[voiceIndex];
            voice.Stop();
            voice.clip = clip;
            voiceId[voiceIndex] = def.id;
            // one loudness target for every clip (measured table), then a small ±2 dB variation so repeats never sound stamped
            voiceGain[voiceIndex] = def.gain * BusTrim(def.busId) * gainScale * SfxGovernor.Loudness(clip) * (ui ? 1f : SfxGovernor.VaryGain());
            voice.volume = PlaybackGain(voiceIndex);
            voice.priority = danger ? Mathf.Min(Mathf.Clamp(def.priority, 0, 255), rolePriority) : rolePriority;
            // one small pitch spread for every effect (±4%); a weapon's own voice keeps only ±2% so it stays recognisably its own pitch
            float jitter = ui ? Mathf.Min(def.pitchJitter, SfxGovernor.PitchSpread) : SfxGovernor.PitchSpread;
            if (!Mathf.Approximately(voicePitch, 1f)) jitter = Mathf.Min(jitter, 0.02f);
            float pitch = jitter > 0f ? SfxGovernor.VaryPitch(voicePitch, jitter) : voicePitch;
            if (critical && impact) pitch += 0.06f;
            if (def.id == "loot.coin.pickup")
            {
                // a run of coins climbs in pitch; a pause starts the scale again
                coinStreak = now - coinStreakAt < 0.7f ? coinStreak + 1 : 0; coinStreakAt = now;
                pitch = 1f + Mathf.Min(0.45f, coinStreak * 0.025f);
            }
            voice.pitch = pitch;
            voice.panStereo = ui ? 0f : pan;
            MixBus bus = SfxGovernor.BusOf(def.id, def.busId);
            AudioMix.Route(voice, bus);
            voice.Play();
            AudioMix.ReportSfx(voiceGain[voiceIndex], bus); // the music ducks under dense combat and boss cues
            PlayedCues++;
            if (danger && Ducks(def.id)) PresentationSettings.Danger();
            return true;
        }

        // Cue family = id without its last segment ("cmb.swing.blade" -> "cmb.swing"). A family never starts faster than its interval,
        // so six swing variants or six hit materials cannot stack into a wall of identical transients.
        private static string GroupOf(string id)
        {
            int dot = id.LastIndexOf('.');
            return dot > 0 ? id.Substring(0, dot) : id;
        }

        private static float GroupInterval(string group, bool reduced)
        {
            float interval;
            switch (group)
            {
                case "cmb.swing": interval = 0.045f; break;
                case "cmb.hit": interval = 0.03f; break;
                case "enemy.die": interval = 0.035f; break;
                case "loot.coin": interval = 0.03f; break;
                case "wpn.cast": case "sig": case "wpn": interval = 0.06f; break;
                default: interval = 0.03f; break;
            }
            return interval * (reduced ? 2f : 1f);
        }

        // Relative level of each cue family inside its mixer bus (AudioMix: the sliders live on the buses).
        public static float BusTrim(AudioBus bus)
        {
            switch (bus)
            {
                case AudioBus.Combat: return 1f;
                case AudioBus.UI: return 1f;
                case AudioBus.Foley: return 0.9f;
                case AudioBus.Voice: return 0.85f;
                case AudioBus.Ambience: return 0.8f;
                default: return 1f;
            }
        }

        // Lets an important cue (merge, chest, star-up...) take the least important cosmetic lane instead of being dropped.
        private int StealVoice(int priority, bool reduced)
        {
            int best = -1;
            int bestPriority = priority;
            for (int index = 4; index < 4 + (reduced ? ReducedCombatLanes : CombatLanes); index++)
            {
                if (voices[index].isPlaying && voices[index].priority > bestPriority) { best = index; bestPriority = voices[index].priority; }
            }
            return best;
        }

        public static void PlayInterface(int cue)
        {
            if (interfaceOwners.Count > 0) interfaceOwners[interfaceOwners.Count - 1].PlayCueOnBus(cue, false, false, true);
        }

        private void Update()
        {
            if (interfaceOwners.Count == 0 || interfaceOwners[interfaceOwners.Count - 1] != this) { StopVoices(); return; }
            AudioMix.Tick();
            if (!SimulationPaused && Time.timeScale > 0 && focused && !backgrounded && !AudioListener.pause) scheduleClock += Time.unscaledDeltaTime;
            if (pending.Count > 0)
            {
                for (int index = pending.Count - 1; index >= 0; index--)
                {
                    if ((pending[index].Interface ? Time.unscaledTime : scheduleClock) < pending[index].At) continue;
                    string id = pending[index].Id;
                    pending.RemoveAt(index);
                    PlayLibrary(ArcaneCueLibrary.Get(id));
                }
            }
            for (int index = 0; index < voices.Length; index++)
            {
                if ((Time.timeScale <= 0 || SimulationPaused) && (index < 2 || index >= 4)) voices[index].Stop();
                if (PresentationSettings.ReducedAudio && index >= 4 + ReducedCombatLanes && voices[index].isPlaying) voices[index].Stop();
                voices[index].volume = PlaybackGain(index);
            }
        }

        private float PlaybackGain(int index) => voiceGain[index] * AudioMix.SourceGain(MixBus.Sfx) * (index >= 2 && PresentationSettings.ReducedAudio ? 0.65f : 1f) * (index >= 4 && Time.unscaledTime < PresentationSettings.DangerUntil ? 0.55f : 1f);

        private void OnApplicationFocus(bool focus) { focused = focus; if (!focus) StopVoices(); }
        private void OnApplicationPause(bool pause) { backgrounded = pause; if (pause) StopVoices(); }
        private void StopVoices() { foreach (AudioSource voice in voices) if (voice != null) voice.Stop(); }
        private void OnDisable() { interfaceOwners.Remove(this); pending.Clear(); StopVoices(); }

        private void OnDestroy()
        {
            for (int index = 0; index < clips.Length; index++) if (ownedClips[index] && clips[index] != null) Destroy(clips[index]);
        }
    }
}
