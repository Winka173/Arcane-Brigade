using UnityEngine;

namespace ArcaneBrigade
{
    public static class ArcadeSound
    {
        public const int SampleRate = 22050;

        public static float[] MusicStudy(bool shop)
        {
            float beat = 60f / (shop ? 104f : 132f);
            var samples = new float[Mathf.RoundToInt(SampleRate * beat * 32f)];
            int[] roots = { 50, 46, 53, 48, 50, 46, 55, 48 };
            int[] motif = { 0, 7, 12, 10, 7, 3, 5, 7, 0, 3, 7, 10, 12, 7, 5, 3 };
            for (int bar = 0; bar < 8; bar++)
            {
                int root = roots[bar];
                for (int step = 0; step < 8; step++)
                {
                    float time = (bar * 4f + step * 0.5f) * beat;
                    Note(samples, time, beat * 0.36f, root - 12 + (step % 4 == 3 ? 7 : 0), 0.14f, true);
                    Note(samples, time, beat * 0.43f, root + 12 + motif[(bar * 8 + step) % motif.Length], shop ? 0.07f : 0.10f, false);
                    if (step % 2 == 0) Drum(samples, time, 0, shop ? 0.06f : 0.12f);
                    if (step == 2 || step == 6) Drum(samples, time, 1, shop ? 0.035f : 0.075f);
                    Drum(samples, time, 2, shop ? 0.014f : 0.023f);
                }
                foreach (int interval in new[] { 0, 3, 7 }) Note(samples, bar * beat * 4f, beat * 3.8f, root + interval, 0.025f, false);
            }
            for (int index = 0; index < samples.Length; index++) samples[index] = Mathf.Clamp(samples[index], -0.85f, 0.85f);
            return samples;
        }

        private static float Frequency(int note) => 440f * Mathf.Pow(2f, (note - 69) / 12f);

        private static void Note(float[] samples, float start, float length, int note, float volume, bool bass)
        {
            int first = Mathf.RoundToInt(start * SampleRate);
            int count = Mathf.RoundToInt(length * SampleRate);
            float frequency = Frequency(note);
            for (int offset = 0; offset < count && first + offset < samples.Length; offset++)
            {
                float time = offset / (float)SampleRate;
                float envelope = Mathf.Min(1f, time / 0.006f) * Mathf.Clamp01((length - time) / 0.02f) * Mathf.Exp(-time * (bass ? 8f : 5f));
                float phase = time * frequency * Mathf.PI * 2f;
                float tone = bass ? Mathf.Sin(phase) + Mathf.Sin(phase * 2f) * 0.18f : Mathf.Sin(phase) + Mathf.Sin(phase * 2f) * 0.24f + Mathf.Sin(phase * 3f) * 0.09f;
                samples[first + offset] += tone * volume * envelope;
            }
        }

        private static void Drum(float[] samples, float start, int kind, float volume)
        {
            int first = Mathf.RoundToInt(start * SampleRate);
            float length = kind == 0 ? 0.16f : kind == 1 ? 0.11f : 0.04f;
            uint random = unchecked((uint)first + 931u);
            for (int offset = 0; offset < length * SampleRate && first + offset < samples.Length; offset++)
            {
                random ^= random << 13; random ^= random >> 17; random ^= random << 5;
                float noise = (random & 65535) / 32767.5f - 1f;
                float time = offset / (float)SampleRate;
                float tone = kind == 0 ? Mathf.Sin(Mathf.PI * 2f * (85f * time - 120f * time * time)) : noise * (kind == 1 ? 0.8f : 0.4f);
                samples[first + offset] += tone * Mathf.Exp(-time * (kind == 0 ? 25f : kind == 1 ? 38f : 90f)) * volume;
            }
        }

        public static AudioClip Cue(int kind)
        {
            float length = kind == 6 ? 0.5f : kind == 4 || kind == 5 ? 0.25f : 0.14f;
            var samples = new float[Mathf.CeilToInt(SampleRate * length)];
            uint random = (uint)(419 + kind * 173);
            for (int index = 0; index < samples.Length; index++)
            {
                random ^= random << 13; random ^= random >> 17; random ^= random << 5;
                float noise = (random & 65535) / 32767.5f - 1f;
                float time = index / (float)SampleRate;
                float progress = time / length;
                float frequency = kind == 5 ? 180f - progress * 110f : kind == 6 ? 300f - progress * 210f : kind == 0 ? 120f : kind == 1 ? 1400f : kind == 2 ? 650f + progress * 1200f : kind == 3 ? 90f : 950f + progress * 600f;
                float tonal = Mathf.Sin(time * frequency * Mathf.PI * 2f);
                float mix = kind == 5 ? noise * 0.5f + tonal * 0.3f : kind == 6 ? tonal * 0.65f + noise * 0.15f : kind == 0 || kind == 1 ? noise * 0.75f + tonal * 0.15f : kind == 3 ? noise * 0.35f + tonal * 0.65f : tonal + Mathf.Sin(time * frequency * 1.5f * Mathf.PI * 2f) * 0.2f;
                samples[index] = mix * Mathf.Sin(Mathf.Min(1f, progress * 10f) * Mathf.PI * 0.5f) * (1f - progress) * 0.5f;
            }
            AudioClip clip = AudioClip.Create("ArcadeCue" + kind, samples.Length, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
