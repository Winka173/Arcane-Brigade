using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class WaitingRealmStage : MonoBehaviour
    {
        private ArenaController owner;
        private SpriteRenderer chair;
        private FloatingStoryProp floatingChair;
        private AudioSource song;
        private AudioClip waiting, morning;
        private int songWave = -1;
        private bool dawnPlayed;
        public bool ChairVisible => chair != null && chair.enabled;
        public void Configure(ArenaController value)
        {
            owner = value;
            var child = new GameObject("TheUnoccupiedChair"); child.transform.SetParent(transform, false);
            chair = child.AddComponent<SpriteRenderer>(); chair.sprite = Resources.Load<Sprite>("MienCho/EmptyChair");
            floatingChair=child.AddComponent<FloatingStoryProp>();floatingChair.Configure(chair,241);
            song = gameObject.AddComponent<AudioSource>(); song.playOnAwake = false; song.spatialBlend = 0;
            waiting = Compose(false); morning = Compose(true);
        }
        // A small authored phrase. The earlier version withholds its cadence; dawn answers it
        // with several slightly offset voices. This is synthesis, not a recorded orchestra.
        private static AudioClip Compose(bool dawn)
        {
            int[] notes = dawn ? new[] { 60, 64, 67, 69, 67, 64, 62, 60, 64, 67, 72, 71, 69, 67, 62, 60 } : new[] { 60, 63, 67, 68, 67, 63, 62, 0, 60, 63, 67, 68, 67, 63, 0, 0 };
            const int rate = 22050; const float step = .56f;
            float[] samples = new float[(int)((notes.Length * step + 1.4f) * rate)];
            for (int n = 0; n < notes.Length; n++)
            {
                if (notes[n] == 0) continue;
                double frequency = 440 * System.Math.Pow(2, (notes[n] - 69) / 12d);
                int start = (int)(n * step * rate);
                for (int i = 0; i < rate * 1.4f && start + i < samples.Length; i++)
                {
                    float t = i / (float)rate;
                    double phase = t * frequency * System.Math.PI * 2;
                    double note = System.Math.Sin(phase) * .5 + System.Math.Sin(phase * 2.003) * .16 + System.Math.Sin(phase * 3.99) * .045;
                    if (dawn) note += System.Math.Sin((t + n * .0017) * frequency * 1.003 * System.Math.PI * 2) * .11;
                    float envelope = Mathf.Min(1, t * 90) * Mathf.Exp(-t * 4.5f);
                    samples[start + i] += (float)note * envelope * .3f;
                }
            }
            var clip = AudioClip.Create(dawn ? "A Morning With Other Voices" : "The Withheld Last Verse", samples.Length, 1, rate, false); clip.SetData(samples, 0); return clip;
        }
        private void Update()
        {
            if (owner?.Session == null || owner.Simulation == null) return;
            var state = owner.Session.State;
            bool dawn = state.Nightmare.CustomerDefeated && !state.Endless;
            chair.enabled = chair.sprite != null && !owner.Session.SecretRoomOpen && !owner.NightmareEndingVisible && state.Phase != SessionPhase.Defeat;
            if (chair.enabled)
            {
                Vector2 position = state.Phase == SessionPhase.Shop ? new Vector2(7.4f, 3.6f) : new Vector2(0, owner.WorldBounds.y * .5f - .8f);
                chair.transform.localScale = Vector3.one * (2 / Mathf.Max(.01f, chair.sprite.bounds.size.y));
                chair.sortingOrder = WorldSpriteGrounding.SortAt(position.y);
                // The unreachable seat is a narrative landmark, never a hidden threat.
                chair.color = dawn ? new Color(1, .95f, .86f) : Color.Lerp(new Color(.85f, .78f, .69f), new Color(.24f, .22f, .3f), NightmareRules.Depth(state.Wave) / 4f);
                floatingChair.Display(position,owner.PresentationClock,NightmareRules.Depth(state.Wave));
            }
            else floatingChair.Display(Vector3.zero,owner.PresentationClock,0);
            if (owner.PresentationPaused) { song.Pause(); return; }
            song.UnPause();
            song.volume = PresentationSettings.ReducedAudio ? 0 : PresentationSettings.MusicVolume * .16f;
            bool restPhrase = state.Phase == SessionPhase.Shop && (state.Wave == 1 || state.Wave == 8 || state.Wave == 16) && state.Wave != songWave;
            if (dawn && !dawnPlayed && state.Nightmare.EndingPage >= 2) { song.clip = morning; song.Play(); dawnPlayed = true; }
            else if (restPhrase && !owner.TransitionPending) { song.clip = waiting; song.Play(); songWave = state.Wave; }
            if (state.Phase == SessionPhase.Combat && song.isPlaying) song.Stop();
        }
        private void OnDestroy() { if (waiting != null) Destroy(waiting); if (morning != null) Destroy(morning); }
    }
}
