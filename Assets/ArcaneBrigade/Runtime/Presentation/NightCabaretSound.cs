using UnityEngine;

namespace ArcaneBrigade
{
    public static class NightCabaretSound
    {
        private const int Rate = ArcadeSound.SampleRate;
        public static AudioClip Foley(int cue)
        {
            if (cue >= 27 && cue <= 38) return MaterialFoley((BodyMaterial)((cue - 27) % 6), cue < 33);
            float length = cue == 6 ? 0.65f : cue == 23 ? 0.55f : cue == 4 ? 0.14f : cue >= 27 ? 0.3f : 0.2f;
            var samples = new float[Mathf.CeilToInt(Rate * length)];
            uint state = (uint)(701 + cue * 8191); float low = 0; float previous = 0;
            for (int index = 0; index < samples.Length; index++)
            {
                state ^= state << 13; state ^= state >> 17; state ^= state << 5;
                float noise = (state & 65535) / 32767.5f - 1;
                low += (noise - low) * (cue == 0 || cue == 1 ? 0.13f : 0.28f);
                float time = index / (float)Rate; float progress = time / length;
                float air = low - previous; previous = low;
                float attack = Mathf.Min(1, time / 0.003f); float tail = Mathf.Exp(-time * (cue == 6 ? 8 : 28));
                float signal;
                if (cue == 0 || cue == 1 || cue == 10 || cue == 12 || cue == 14 || cue == 15 || cue == 16)
                    signal = (low * 0.85f + air * 0.4f) * Mathf.Sin(progress * Mathf.PI) * 1.2f;
                else if (cue == 3 || cue == 5 || cue == 6 || cue == 22 || cue >= 27)
                {
                    float frequency = cue == 28 ? 330 : cue == 31 ? 1300 : cue == 3 ? 115 : 180;
                    signal = low * 0.65f + Mathf.Sin(time * frequency * Mathf.PI * 2 - progress * progress * 10) * 0.35f;
                    if (cue == 28 || cue == 31) signal += Mathf.Sin(time * frequency * 2.71f * Mathf.PI * 2) * 0.15f;
                }
                else
                {
                    float pitch = cue == 4 ? 1680 : cue == 19 ? 850 : cue == 24 || cue == 25 ? 390 : 620;
                    float phase = time * pitch * Mathf.PI * 2;
                    signal = Mathf.Sin(phase) * 0.28f + Mathf.Sin(phase * 2.756f) * Mathf.Exp(-time * 42) * 0.16f + air * 0.12f;
                    if (cue == 23 || cue == 20) signal += Mathf.Sin(phase * 1.5f) * 0.12f;
                }
                samples[index] = signal * attack * tail * 0.7f * Mathf.Clamp01((length - time) / 0.018f);
            }
            AudioClip clip = AudioClip.Create("CabaretFoley-" + cue, samples.Length, 1, Rate, false); clip.SetData(samples, 0); return clip;
        }

        public static AudioClip MaterialFoley(BodyMaterial material, bool death)
        {
            float length = death ? 0.32f : 0.16f;
            var samples = new float[Mathf.CeilToInt(Rate * length)];
            uint state = 701u + (uint)material * 8191u;
            float low = 0;
            for (int index = 0; index < samples.Length; index++)
            {
                state ^= state << 13; state ^= state >> 17; state ^= state << 5;
                float noise = (state & 65535) / 32767.5f - 1;
                low += (noise - low) * 0.09f;
                float time = index / (float)Rate;
                float signal;
                switch (material)
                {
                    case BodyMaterial.Paper: signal = (noise - low) * (0.3f + 0.2f * Mathf.Sin(time * 240)); break;
                    case BodyMaterial.Metal: signal = Resonance(time, 330, 2.71f, 13) + (noise - low) * Mathf.Exp(-time * 100) * 0.25f; break;
                    case BodyMaterial.Wax: signal = low * 0.8f + Resonance(time, 105, 1.4f, 38) * 0.3f; break;
                    case BodyMaterial.Fabric: signal = low * Mathf.Sin(Mathf.PI * time / length) * 0.85f; break;
                    case BodyMaterial.Glass: signal = Resonance(time, 1300, 3.17f, 24) * 0.7f + (noise - low) * Mathf.Exp(-time * 55) * 0.2f; break;
                    default: signal = Resonance(time, 190, 1.83f, 42) * 0.65f + low * Mathf.Exp(-time * 70); break;
                }
                samples[index] = signal * Mathf.Min(1, time / 0.003f) * Mathf.Exp(-time * (death ? 9 : 18)) * Mathf.Clamp01((length - time) / 0.018f) * 0.55f;
            }
            AudioClip clip = AudioClip.Create("CabaretMaterial-" + material + (death ? "-Death" : "-Hit"), samples.Length, 1, Rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static float Resonance(float time, float frequency, float overtone, float decay)
        {
            float phase = time * frequency * Mathf.PI * 2;
            return Mathf.Sin(phase) * 0.45f + Mathf.Sin(phase * overtone) * Mathf.Exp(-time * decay) * 0.25f;
        }
        public static float[] Compose(int layer, bool shop = false)
        {
            float beat = 60f / (shop ? 100f : 116f);
            var samples = new float[Mathf.RoundToInt(Rate * beat * 128)];
            int[] roots = { 50, 55, 48, 53, 46, 51, 45, 50 };
            int[] phrase = { 12, 15, 19, 17, 15, 12, 10, 7, 10, 12, 15, 14, 10, 7, 3, 7 };
            for (int bar = 0; bar < 32; bar++)
            {
                int root = roots[bar % roots.Length]; int section = bar / 8;
                if (layer == 0)
                {
                    for (int pulse = 0; pulse < 4; pulse++)
                    {
                        float at = (bar * 4 + pulse) * beat;
                        Pluck(samples, at, beat * 0.75f, root - 12 + (pulse == 2 ? 7 : pulse == 3 ? 10 : 0), shop ? 0.11f : 0.16f, 0);
                        Brush(samples, at, pulse % 2 == 0 ? 0 : 1, shop ? 0.07f : 0.11f);
                        Brush(samples, at + beat * 0.66f, 2, 0.025f);
                    }
                    for (int step = 0; step < 8; step++)
                    {
                        if ((step + section) % 4 == 3) continue;
                        float at = (bar * 4 + step * 0.5f + (step % 2 == 1 ? 0.08f : 0)) * beat;
                        Pluck(samples, at, beat * (step == 6 ? 0.9f : 0.4f), root + phrase[(bar * 3 + step + section * 4) % phrase.Length], shop ? 0.065f : 0.095f, 1);
                    }
                }
                else if (layer == 1)
                {
                    foreach (int interval in new[] { 0, 3, 7, 10 })
                        for (int chord = 0; chord < 2; chord++) Pluck(samples, (bar * 4 + chord * 2 + 0.28f) * beat, beat * 1.4f, root + interval, 0.035f, 2);
                }
                else
                {
                    for (int step = 0; step < 8; step++) Pluck(samples, (bar * 4 + step * 0.5f) * beat, beat * 0.35f, root + (step % 2 == 0 ? 0 : 7), 0.07f, 3);
                    Brush(samples, bar * 4 * beat, 0, 0.15f);
                }
            }
            Room(samples);
            return samples;
        }
        private static void Pluck(float[] samples, float at, float length, int note, float gain, int voice)
        {
            int start = Mathf.RoundToInt(at * Rate); int duration = Mathf.RoundToInt(length * Rate);
            float frequency = 440 * Mathf.Pow(2, (note - 69) / 12f);
            for (int offset = 0; offset < duration && start + offset < samples.Length; offset++)
            {
                float time = offset / (float)Rate; float phase = time * frequency * Mathf.PI * 2;
                float envelope = Mathf.Min(1, time / 0.005f) * Mathf.Exp(-time * (voice == 0 ? 6 : voice == 1 ? 5 : 3)) * Mathf.Clamp01((length - time) / 0.035f);
                float tone = Mathf.Sin(phase) + Mathf.Sin(phase * 2.002f) * (voice == 0 ? 0.2f : 0.3f) * Mathf.Exp(-time * 7);
                tone += voice == 1 ? Mathf.Sin(phase * 3.99f) * 0.15f * Mathf.Exp(-time * 12) : voice == 2 ? Mathf.Sin(phase * 3) * 0.08f : Mathf.Sin(phase * 3.005f) * 0.1f;
                samples[start + offset] += tone * envelope * gain;
            }
        }
        private static void Brush(float[] samples, float at, int kind, float gain)
        {
            int start = Mathf.RoundToInt(at * Rate); uint state = (uint)(start + kind * 1777 + 19); float filtered = 0;
            int duration = Mathf.RoundToInt(Rate * (kind == 0 ? 0.14f : 0.09f));
            for (int offset = 0; offset < duration && start + offset < samples.Length; offset++)
            {
                state ^= state << 13; state ^= state >> 17; state ^= state << 5;
                float noise = (state & 65535) / 32767.5f - 1; float time = offset / (float)Rate;
                filtered += (noise - filtered) * 0.2f;
                float sound = kind == 0 ? Mathf.Sin((65 * time - 95 * time * time) * Mathf.PI * 2) : kind == 1 ? filtered : noise - filtered;
                samples[start + offset] += sound * Mathf.Min(1, time / 0.002f) * Mathf.Exp(-time * (kind == 0 ? 32 : 58)) * gain;
            }
        }
        private static void Room(float[] samples)
        {
            int delay = Mathf.RoundToInt(Rate * 0.071f);
            for (int index = delay; index < samples.Length; index++) samples[index] += samples[index - delay] * 0.12f;
        }
    }
}
