using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class CheckpointAudio : MonoBehaviour
    {
        private static readonly List<CheckpointAudio> interfaceOwners = new List<CheckpointAudio>();
        private readonly AudioSource[] voices = new AudioSource[8];
        private readonly AudioClip[] clips = new AudioClip[39];
        private readonly bool[] ownedClips = new bool[39];
        private readonly float[] nextPlayback = new float[39];
        private readonly float[] nextDangerPlayback = new float[39];
        private readonly float[] voiceGain = new float[8];
        private readonly string[] voiceId = new string[8];
        private readonly Dictionary<string, float> nextById = new Dictionary<string, float>();
        private float nextCosmetic;
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
            }
            ConfigurePresentation();
        }

        private void OnEnable() { interfaceOwners.Add(this); }

        public void ConfigurePresentation()
        {
            if (presentation) return;
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
            string id = ArcaneCueLibrary.Available ? ArcaneCueLibrary.EventId(value) : null;
            if (id != null && PlayLibrary(ArcaneCueLibrary.Get(id))) return;
            bool danger = value.Faction == Faction.Champion && value.Kind == CombatEventKind.Hit;
            PlayCue(EventCue(value), danger, value.Critical);
        }

        public static bool IsDangerCue(int cue) => cue == 6 || cue == 24 || cue == 25 || cue == 26;
        public static float GroupDelay(int cue, bool reduced) => (cue >= 27 ? 0.12f : cue >= 10 ? 0.09f : cue == 4 ? 0.08f : 0.04f) * (reduced ? 2.5f : 1);
        public static float CosmeticInterval(bool reduced) => reduced ? 0.085f : 0.035f;
        public static float CueGain(int cue, bool danger, bool reduced) => danger ? 0.25f : (cue == 4 ? 0.09f : cue == 19 ? 0.08f : cue >= 33 ? 0.1f : 0.14f) * (reduced ? 0.65f : 1f);

        public static int AllocateVoice(bool danger, bool ui, bool reduced, int cursor, int occupied)
        {
            if (danger) return cursor % 2;
            if (ui) return 2 + cursor % 2;
            int capacity = reduced ? 2 : 4;
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
            voice.Play();
            PlayedCues++;
            if (danger) PresentationSettings.Danger();
        }

        /// <summary>Play any cue by stable id on the top audio owner (see Docs/Audio/Audio_Cue_Contract.md).</summary>
        public static void PlayInterfaceId(string id)
        {
            if (interfaceOwners.Count > 0) interfaceOwners[interfaceOwners.Count - 1].PlayLibrary(ArcaneCueLibrary.Get(id));
        }

        public void PlayId(string id) { PlayLibrary(ArcaneCueLibrary.Get(id)); }

        // Returns true when the cue was handled (played or deliberately dropped); false asks the caller to use the legacy path.
        private bool PlayLibrary(ArcaneCueDef def)
        {
            if (!ArcaneCueLibrary.Enabled) return false;
            if (def == null || def.clips == null || def.clips.Length == 0) return false;
            if (interfaceOwners.Count == 0 || interfaceOwners[interfaceOwners.Count - 1] != this) return true;
            bool danger = def.danger;
            bool ui = def.busId == AudioBus.UI && !danger;
            if (!isActiveAndEnabled || !focused || backgrounded || AudioListener.pause || (!ui && Time.timeScale <= 0)) return true;
            bool reduced = PresentationSettings.ReducedAudio;
            if (reduced && !danger && def.priority >= 180) return true;
            float now = Time.unscaledTime;
            if (nextById.TryGetValue(def.id, out float until) && now < until) { if (!danger) DroppedCosmeticCues++; return true; }
            if (now < (danger ? nextDanger : ui ? nextInterface : nextCosmetic)) { if (!danger) DroppedCosmeticCues++; return true; }
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
            int voiceIndex = AllocateVoice(danger, ui, reduced, danger ? dangerVoice : interfaceVoice, occupied);
            if (voiceIndex < 0 && !danger && !ui && def.priority <= 120) voiceIndex = StealVoice(def.priority, reduced);
            if (voiceIndex < 0) { DroppedCosmeticCues++; return true; }
            if (danger) dangerVoice = (dangerVoice + 1) % 2;
            else if (ui) interfaceVoice = (interfaceVoice + 1) % 2;
            nextById[def.id] = now + def.cooldown * (reduced && !danger ? 2f : 1f);
            if (danger) nextDanger = now + 0.06f;
            else if (ui) nextInterface = now + 0.04f;
            else nextCosmetic = now + CosmeticInterval(reduced);
            AudioSource voice = voices[voiceIndex];
            voice.Stop();
            voice.clip = clip;
            voiceId[voiceIndex] = def.id;
            voiceGain[voiceIndex] = def.gain * BusTrim(def.busId);
            voice.volume = PlaybackGain(voiceIndex);
            voice.priority = Mathf.Clamp(def.priority, 0, 255);
            voice.pitch = 1f + (def.pitchJitter > 0f ? UnityEngine.Random.Range(-def.pitchJitter, def.pitchJitter) : 0f);
            voice.Play();
            PlayedCues++;
            if (danger) PresentationSettings.Danger();
            return true;
        }

        // Software bus trims (no AudioMixer asset): relative level of each bus before master/SFX volume.
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
            for (int index = 4; index < 4 + (reduced ? 2 : 4); index++)
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
            for (int index = 0; index < voices.Length; index++)
            {
                if (Time.timeScale <= 0 && (index < 2 || index >= 4)) voices[index].Stop();
                if (PresentationSettings.ReducedAudio && index >= 6 && voices[index].isPlaying) voices[index].Stop();
                voices[index].volume = PlaybackGain(index);
            }
        }

        private float PlaybackGain(int index) => voiceGain[index] * PresentationSettings.SfxVolume * (index >= 2 && PresentationSettings.ReducedAudio ? 0.65f : 1f) * (index >= 4 && Time.unscaledTime < PresentationSettings.DangerUntil ? 0.55f : 1f);

        private void OnApplicationFocus(bool focus) { focused = focus; if (!focus) StopVoices(); }
        private void OnApplicationPause(bool pause) { backgrounded = pause; if (pause) StopVoices(); }
        private void StopVoices() { foreach (AudioSource voice in voices) if (voice != null) voice.Stop(); }
        private void OnDisable() { interfaceOwners.Remove(this); StopVoices(); }

        private void OnDestroy()
        {
            for (int index = 0; index < clips.Length; index++) if (ownedClips[index] && clips[index] != null) Destroy(clips[index]);
        }
    }
}
