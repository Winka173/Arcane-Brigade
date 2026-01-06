using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class CheckpointAudio : MonoBehaviour
    {
        private readonly AudioSource[] voices = new AudioSource[8];
        private readonly AudioClip[] clips = new AudioClip[33];
        private readonly float[] nextPlayback = new float[33];
        private readonly float[] nextDangerPlayback = new float[33];
        private readonly float[] voiceGain = new float[8];
        private static CheckpointAudio interfaceAudio;
        private int dangerVoice;
        public int PlayedCues { get; private set; }
        public int DroppedCosmeticCues { get; private set; }
        private int nextVoice;
        private bool presentation;

        private void Awake()
        {
            if (interfaceAudio == null) interfaceAudio = this;
            for (int index = 0; index < voices.Length; index++)
            {
                voices[index] = gameObject.AddComponent<AudioSource>();
                voices[index].playOnAwake = false;
                voices[index].volume = 0.12f;
            }
            clips[0] = Tone("Shot", 480f, 0.055f);
            clips[1] = Tone("Hit", 180f, 0.075f);
            clips[2] = Tone("Pickup", 900f, 0.1f);
        }

        private static AudioClip Tone(string name, float frequency, float duration)
        {
            int count = Mathf.CeilToInt(22050f * duration);
            var samples = new float[count];
            for (int index = 0; index < count; index++)
            {
                float progress = index / (float)count;
                samples[index] = Mathf.Sin(index * frequency * (1f + progress * 0.4f) * Mathf.PI * 2f / 22050f) * Mathf.Sin(progress * Mathf.PI) * (1f - progress);
            }
            AudioClip clip = AudioClip.Create(name, count, 1, 22050, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public void Play(CombatEventKind kind)
        {
            int index = kind == CombatEventKind.Pickup ? 2 : kind == CombatEventKind.Hit ? 1 : kind == CombatEventKind.Fire ? 0 : -1;
            if (index < 0) return;
            if (Time.unscaledTime < nextPlayback[index]) return;
            nextPlayback[index] = Time.unscaledTime + (index == 2 ? 0.08f : 0.035f);
            AudioSource voice = voices[nextVoice];
            nextVoice = (nextVoice + 1) % voices.Length;
            voice.clip = clips[index];
            voice.Play();
        }

        public void ConfigurePresentation()
        {
            presentation = true;
            for (int index = 0; index < clips.Length; index++)
            {
                if (clips[index] != null) Destroy(clips[index]);
                clips[index] = ArcadeSound.Cue(index);
            }
        }

        public void Play(CombatEvent value)
        {
            if (!presentation) { Play(value.Kind); return; }
            int cue = value.Kind == CombatEventKind.Kill ? value.Faction == Faction.Champion || value.Subject != null && value.Subject.Definition.IsBoss ? 6 : 5 : value.Kind == CombatEventKind.Hit ? 3 : value.Kind == CombatEventKind.ChestOpened ? 2 : value.Kind == CombatEventKind.Pickup ? 4 : value.Kind == CombatEventKind.Fire && value.Weapon != null ? value.Weapon.Kind == WeaponKind.Melee ? 0 : value.Weapon.Motion == WeaponMotion.Bow ? 1 : 2 : -1;
            if (value.Kind == CombatEventKind.SignaturePulse) cue = value.Weapon != null && value.Weapon.Signature == SignatureWeapon.Bell ? 7 : 8;
            if (value.Kind == CombatEventKind.SignatureBurst) cue = 9;
            if (value.Kind == CombatEventKind.Fire && value.Weapon != null && value.Weapon.Family != ExpandedWeapon.None) cue = 9 + (int)value.Weapon.Family;
            if (value.Kind == CombatEventKind.Kill && value.Faction != Faction.Champion && value.Subject != null && !value.Subject.Definition.IsBoss) cue = 27 + (int)value.Subject.Definition.Material;
            bool danger = value.Faction == Faction.Champion && value.Kind == CombatEventKind.Hit || cue == 6;
            PlayCue(cue, danger, value.Critical);
        }

        public static bool IsDangerCue(int cue) => cue == 6 || cue == 24 || cue == 25 || cue == 26;
        public static float GroupDelay(int cue, bool reduced) => (cue >= 10 ? 0.09f : cue == 4 ? 0.08f : 0.04f) * (reduced ? 2.5f : 1);
        public void PlayCue(int cue, bool danger = false, bool critical = false)
        {
            if (cue < 0 || cue >= clips.Length || clips[cue] == null) return;
            danger |= IsDangerCue(cue);
            float[] clock = danger ? nextDangerPlayback : nextPlayback;
            if (Time.unscaledTime < clock[cue]) { if (!danger) DroppedCosmeticCues++; return; }
            clock[cue] = Time.unscaledTime + (danger ? 0.035f : GroupDelay(cue, PresentationSettings.ReducedAudio));
            int voiceIndex = danger ? dangerVoice : 2 + nextVoice % 6;
            if (danger) dangerVoice = (dangerVoice + 1) % 2; else nextVoice = (nextVoice + 1) % 6;
            AudioSource voice = voices[voiceIndex];
            if (!danger && voice.isPlaying && Time.unscaledTime < PresentationSettings.DangerUntil) { DroppedCosmeticCues++; return; }
            voice.clip = clips[cue];
            voiceGain[voiceIndex] = danger ? 0.25f : cue == 4 ? 0.09f : cue == 19 ? 0.08f : 0.14f;
            voice.volume = voiceGain[voiceIndex] * PresentationSettings.SfxVolume;
            voice.priority = danger ? 16 : 180;
            voice.pitch = critical ? 1.1f : 1f;
            voice.Play();
            PlayedCues++;
            if (danger) PresentationSettings.Danger();
        }
        public static void PlayInterface(int cue)
        {
            if (interfaceAudio != null) interfaceAudio.PlayCue(cue);
        }
        private void Update()
        {
            if (!presentation) return;
            for (int index = 0; index < voices.Length; index++) voices[index].volume = voiceGain[index] * PresentationSettings.SfxVolume;
        }

        private void OnDestroy()
        {
            if (interfaceAudio == this) interfaceAudio = null;
            for (int index = 0; index < clips.Length; index++) if (clips[index] != null) Destroy(clips[index]);
        }
    }
}
