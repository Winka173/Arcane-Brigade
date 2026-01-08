using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class ArcadeMusic : MonoBehaviour
    {
        private static readonly List<ArcadeMusic> owners = new List<ArcadeMusic>();
        private readonly AudioSource[] tracks = new AudioSource[4];
        private int selected;
        private bool paused;
        private bool focused = true;
        private bool backgrounded;

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

        private void Awake()
        {
            for (int index = 0; index < tracks.Length; index++)
            {
                AudioSource track = gameObject.AddComponent<AudioSource>();
                track.playOnAwake = false;
                track.loop = true;
                track.spatialBlend = 0;
                track.volume = 0;
                track.pitch = 1;
                track.priority = 210;
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
        }

        private void Update()
        {
            bool active = owners.Count > 0 && owners[owners.Count - 1] == this;
            bool suspended = !focused || backgrounded || AudioListener.pause;
            float gain = PresentationSettings.MusicVolume * PresentationSettings.MusicDuck * BusGain(paused, PresentationSettings.ReducedAudio);
            for (int index = 0; index < tracks.Length; index++)
            {
                AudioSource track = tracks[index];
                if (!active) { track.Stop(); track.volume = 0; continue; }
                if (suspended) continue;
                float target = index == selected ? TrackLevel(index) * gain : 0;
                if (target > 0 && !track.isPlaying && track.clip != null) track.Play();
                track.volume = Mathf.MoveTowards(track.volume, target, Time.unscaledDeltaTime * (target < track.volume ? 0.6f : 0.22f));
                if (target == 0 && track.volume == 0 && track.isPlaying) track.Stop();
            }
        }

        private void OnApplicationFocus(bool focus) { focused = focus; SetSuspended(); }
        private void OnApplicationPause(bool pause) { backgrounded = pause; SetSuspended(); }
        private void SetSuspended()
        {
            foreach (AudioSource track in tracks)
            {
                if (track == null) continue;
                if (!focused || backgrounded) track.Pause();
                else track.UnPause();
            }
        }

        private void OnDisable()
        {
            owners.Remove(this);
            foreach (AudioSource track in tracks) if (track != null) { track.Stop(); track.volume = 0; }
        }
    }
}
