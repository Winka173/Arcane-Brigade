using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class MerchantVoice : MonoBehaviour
    {
        private AudioSource source;
        private AudioClip syllable;
        private bool focused = true;
        private bool backgrounded;
        private int utterance = -1;
        private int previousGlyph;
        private float nextSyllable;
        private float gestureUntil;
        public int PlayedSyllables { get; private set; }

        private void Awake()
        {
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false; source.spatialBlend = 0; source.priority = 180;
            const int rate = 22050;
            var samples = new float[1764];
            for (int index = 0; index < samples.Length; index++)
            {
                float time = index / (float)rate;
                float envelope = Mathf.Sin(index / (float)(samples.Length - 1) * Mathf.PI);
                float carrier = Mathf.Sin(2 * Mathf.PI * (180 * time + 160 * time * time));
                float formant = Mathf.Sin(2 * Mathf.PI * 640 * time) * 0.24f + Mathf.Sin(2 * Mathf.PI * 1050 * time) * 0.12f;
                samples[index] = (carrier + formant) * envelope * envelope * 0.2f;
            }
            syllable = AudioClip.Create("Original merchant murmurs", samples.Length, 1, rate, false);
            syllable.SetData(samples, 0);
        }

        private int wordLetters;
        private int utteranceSyllables;
        private float pauseUntil;
        private const int MaxSyllablesPerLine = 14;

        // Gibberish is paced by words and punctuation, never per glyph: one syllable at each word start,
        // a second inside long words, a breath after commas/stops, and a hard cap per spoken line.
        public void Advance(MerchantDialogue dialogue, bool special, float now, bool paused)
        {
            if (source == null) return;
            source.volume = PresentationSettings.SfxVolume * (PresentationSettings.ReducedAudio ? 0.08f : 0.24f);
            int glyph = dialogue.VisibleGlyphs(now, false);
            if (utterance != dialogue.SpokenCount) { utterance = dialogue.SpokenCount; previousGlyph = 0; nextSyllable = now; wordLetters = 0; utteranceSyllables = 0; pauseUntil = 0; }
            bool quiet = paused || !focused || backgrounded || now >= dialogue.ExpiresAt || PresentationSettings.ReducedAudio;
            if (quiet) { source.Stop(); previousGlyph = glyph; return; }
            ArcaneCueDef def = ArcaneCueLibrary.Get(special ? "voice.dream.syl" : "voice.gold.syl");
            string line = dialogue.CurrentLine;
            for (int index = previousGlyph; line != null && index < glyph && index < line.Length; index++)
            {
                char c = line[index];
                if (char.IsLetterOrDigit(c))
                {
                    bool trigger = wordLetters == 0 || wordLetters == 5;
                    wordLetters++;
                    if (trigger && Time.unscaledTime >= gestureUntil && now >= nextSyllable && now >= pauseUntil && utteranceSyllables < MaxSyllablesPerLine) Speak(def, special, index, line);
                    continue;
                }
                wordLetters = 0;
                if (c == ',' || c == ';' || c == '-') pauseUntil = Mathf.Max(pauseUntil, now + 0.16f);
                else if (c == '.' || c == '!' || c == '?' || c == ':') pauseUntil = Mathf.Max(pauseUntil, now + 0.3f);
            }
            previousGlyph = glyph;
        }

        private void Speak(ArcaneCueDef def, bool special, int index, string line)
        {
            AudioClip clip = def != null ? ArcaneCueLibrary.Pick(def) : null;
            float spread = (index % 5 - 2) * 0.035f;
            bool question = line.IndexOf('?') > index;
            if (clip != null)
            {
                source.pitch = 1f + spread + (question ? 0.08f : 0f);
                source.PlayOneShot(clip, def.gain * CheckpointAudio.BusTrim(AudioBus.Voice) * PresentationSettings.SfxVolume);
                nextSyllable = Time.unscaledTime + 0.14f;
            }
            else
            {
                source.pitch = (special ? 0.78f : 1.14f) + spread;
                source.PlayOneShot(syllable);
                nextSyllable = Time.unscaledTime + 0.105f;
            }
            utteranceSyllables++;
            PlayedSyllables++;
        }

        /// <summary>Non-verbal reaction (laugh, hmm, gasp, farewell) from the same keeper voice bank.</summary>
        public void Gesture(string kind, bool special)
        {
            ArcaneCueDef def = ArcaneCueLibrary.Get((special ? "voice.dream." : "voice.gold.") + kind);
            AudioClip clip = def != null ? ArcaneCueLibrary.Pick(def) : null;
            if (clip == null || source == null || !focused || backgrounded || AudioListener.pause || CheckpointAudio.SimulationPaused || PresentationSettings.ReducedAudio) return;
            source.Stop();
            gestureUntil = Time.unscaledTime + clip.length;
            source.pitch = 1f;
            source.PlayOneShot(clip, def.gain * CheckpointAudio.BusTrim(AudioBus.Voice) * PresentationSettings.SfxVolume);
        }

        public void Silence() { if (source != null) source.Stop(); }
        private void OnApplicationFocus(bool value) { focused = value; if (!value) Silence(); }
        private void OnApplicationPause(bool value) { backgrounded = value; if (value) Silence(); }
        private void OnDisable() { Silence(); }
        private void OnDestroy() { if (syllable != null) Destroy(syllable); }
    }
}
