using UnityEngine;

namespace ArcaneBrigade
{
    public static class WaveCadence
    {
        /// <summary>Act I waves grow from a short first night to a full minute (Brotato-like): 30 s, +2.5 s a wave, 60 s from wave 13.</summary>
        /// <summary>Prices climb with the night: the hall is far busier than it was, and every swarm body drops a coin.</summary>
        public static float PriceRamp(int wave) => 1f + Mathf.Min(0.6f, Mathf.Max(0, wave - 2) * 0.045f);
        /// <summary>Act I story bosses stand long enough for their fight (and its light swarm) to happen: bot builds felled them in 4-14 s.</summary>
        public static float StoryBossBulk(int wave) => wave >= 20 ? 5f : wave >= 15 ? 3.5f : wave >= 10 ? 3f : 2f;
        /// <summary>Play-test 20/10/2026 (D9): waves 15% longer than before: 34.5 s, +2.875 s a wave, 69 s from wave 13.</summary>
        public const float LengthScale = 1.15f;
        public static float WaveLength(int wave) => Mathf.Min(69f, 34.5f + Mathf.Max(0, wave - 1) * 2.875f);
        public static int SwarmBatch(int wave, float time)
        {
            if (wave < 3) return wave == 2 ? 3 : 2; // the first nights already come in small packs
            float beat = Mathf.Repeat(Mathf.Max(0, time - 0.6f), 12);
            int crest = Mathf.Min(36, Mathf.CeilToInt((3 + (wave - 3) / 2) * SwarmPressure(wave)));
            return beat < 6 ? crest : Mathf.Max(2, Mathf.CeilToInt(crest * 0.6f)); // the beat eases but never empties: surges replace the lulls
        }
        public static float SwarmPressure(int wave) => 1 + 2 * Mathf.Clamp01((wave - 3) / 9f);
        public static float SwarmInterval(int wave, float time)
        {
            if (wave < 3) return 1;
            return Mathf.Repeat(Mathf.Max(0, time - 0.6f), 12) < 8 ? 0.9f : 1.25f;
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
            // a steady climb with no quiet stretch: the interval tightens from the first second to the last
            float progress = Mathf.Clamp01(time / Mathf.Max(1, duration));
            return Mathf.Lerp(1.12f, 0.84f, progress) * 0.7f;
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
