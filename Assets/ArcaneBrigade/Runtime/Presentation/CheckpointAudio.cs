using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class CheckpointAudio : MonoBehaviour
    {
        private readonly AudioSource[] voices = new AudioSource[8];
        private readonly AudioClip[] clips = new AudioClip[7];
        private readonly float[] nextPlayback = new float[7];
        private int nextVoice;
        private bool presentation;

        private void Awake()
        {
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
            int cue = value.Kind == CombatEventKind.Kill ? value.Faction == Faction.Champion || value.Subject != null && value.Subject.Definition.IsBoss ? 6 : 5 : value.Kind == CombatEventKind.Hit ? 3 : value.Kind == CombatEventKind.Pickup ? 4 : value.Kind == CombatEventKind.Fire && value.Weapon != null ? value.Weapon.Kind == WeaponKind.Melee ? 0 : value.Weapon.Motion == WeaponMotion.Bow ? 1 : 2 : -1;
            if (cue < 0 || Time.unscaledTime < nextPlayback[cue]) return;
            nextPlayback[cue] = Time.unscaledTime + (cue == 4 ? 0.08f : 0.04f);
            AudioSource voice = voices[nextVoice];
            nextVoice = (nextVoice + 1) % voices.Length;
            voice.clip = clips[cue];
            voice.volume = cue == 6 ? 0.23f : value.Faction == Faction.Champion && value.Kind == CombatEventKind.Hit ? 0.25f : 0.12f;
            voice.pitch = value.Critical ? 1.15f : 0.94f + nextVoice * 0.015f;
            voice.Play();
        }

        private void OnDestroy()
        {
            for (int index = 0; index < clips.Length; index++) if (clips[index] != null) Destroy(clips[index]);
        }
    }
}
