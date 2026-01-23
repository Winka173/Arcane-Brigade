using UnityEngine;

namespace ArcaneBrigade
{
    public static class WaveCadence
    {
        public static float IntervalMultiplier(int wave, float time)
        {
            if (wave < 3) return 1;
            float phase = Mathf.Repeat(Mathf.Max(0, time), 12);
            return phase < 7 ? 0.88f : phase < 9 ? Mathf.Lerp(0.88f, 1.45f, (phase - 7) / 2) : Mathf.Lerp(1.45f, 0.88f, (phase - 9) / 3);
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
