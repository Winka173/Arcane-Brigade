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

        public void Advance(MerchantDialogue dialogue, bool special, float now, bool paused)
        {
            if (source == null) return;
            source.volume = PresentationSettings.SfxVolume * (PresentationSettings.ReducedAudio ? 0.08f : 0.24f);
            int glyph = dialogue.VisibleGlyphs(now, false);
            if (utterance != dialogue.SpokenCount) { utterance = dialogue.SpokenCount; previousGlyph = 0; nextSyllable = now; }
            bool quiet = paused || !focused || backgrounded || now >= dialogue.ExpiresAt || PresentationSettings.ReducedAudio;
            if (quiet) { source.Stop(); previousGlyph = glyph; return; }
            if (glyph > previousGlyph && now >= nextSyllable)
            {
                string line = dialogue.CurrentLine;
                bool audible = false;
                for (int index = previousGlyph; line != null && index < glyph; index++) audible |= char.IsLetterOrDigit(line[index]);
                if (audible)
                {
                    source.pitch = (special ? 0.78f : 1.14f) + (glyph % 5 - 2) * 0.035f;
                    source.PlayOneShot(syllable);
                    PlayedSyllables++;
                    nextSyllable = now + 0.105f;
                }
            }
            previousGlyph = glyph;
        }

        public void Silence() { if (source != null) source.Stop(); }
        private void OnApplicationFocus(bool value) { focused = value; if (!value) Silence(); }
        private void OnApplicationPause(bool value) { backgrounded = value; if (value) Silence(); }
        private void OnDisable() { Silence(); }
        private void OnDestroy() { if (syllable != null) Destroy(syllable); }
    }
}
