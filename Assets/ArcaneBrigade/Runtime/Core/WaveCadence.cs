using UnityEngine;

namespace ArcaneBrigade
{
    public static class WaveCadence
    {
        public static int SwarmBatch(int wave, float time)
        {
            if (wave < 3) return wave == 2 ? 2 : 1;
            float beat = Mathf.Repeat(Mathf.Max(0, time - 0.6f), 12);
            int crest = Mathf.Min(36, Mathf.CeilToInt((3 + (wave - 3) / 2) * SwarmPressure(wave)));
            return beat < 6 ? crest : beat < 8 ? Mathf.Max(2, crest / 2) : 1;
        }
        public static float SwarmPressure(int wave) => 1 + 2 * Mathf.Clamp01((wave - 3) / 9f);
        public static float SwarmInterval(int wave, float time)
        {
            if (wave < 3) return 1;
            return Mathf.Repeat(Mathf.Max(0, time - 0.6f), 12) < 8 ? 0.9f : 2.8f;
        }
        public static float IntervalMultiplier(int wave, float time)
        {
            if (wave < 3) return 1;
            float phase = Mathf.Repeat(Mathf.Max(0, time), 12);
            return phase < 7 ? 0.88f : phase < 9 ? Mathf.Lerp(0.88f, 1.45f, (phase - 7) / 2) : Mathf.Lerp(1.45f, 0.88f, (phase - 9) / 3);
        }
        public static float Envelope(int wave, float time, float duration)
        {
            if (wave < 3) return 1;
            float progress = Mathf.Clamp01(time / Mathf.Max(1, duration));
            float envelope = progress < 0.16f ? 1.35f : progress < 0.42f ? Mathf.Lerp(1.15f, 0.95f, (progress - 0.16f) / 0.26f) : progress < 0.67f ? 0.85f : progress < 0.84f ? 1.6f : progress < 0.94f ? 1 : 1.45f;
            return envelope * 0.7f;
        }
        public static Vector2 Attract(Vector2 position, Vector2 target, float travel, int identity)
        {
            Vector2 difference = target - position;
            if (difference.magnitude <= travel * 2) return Vector2.MoveTowards(position, target, travel);
            Vector2 forward = difference.normalized;
            Vector2 tangent = new Vector2(-forward.y, forward.x) * (identity % 2 == 0 ? 0.24f : -0.24f);
            return position + (forward + tangent).normalized * travel;
        }
    }
}
