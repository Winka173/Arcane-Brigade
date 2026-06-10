using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>One opening for every card (requirement 23/10/2026: many cards popped in with no transition): the first frame a card
    /// is drawn again after being away, it rises from a little below at 88% size with a small overshoot and fades in over
    /// 0.22s, like the visitor card. Reduced Motion: the fade only. Call it where the card starts drawing, inside the 1920x1080
    /// viewport; it returns the colour to restore when the card is done (the caller restores GUI.matrix as it already does).</summary>
    public static class CardPop
    {
        public const float OpenTime = .22f;
        private struct Seen { public int Frame; public float Since; }
        private static readonly Dictionary<string, Seen> seen = new Dictionary<string, Seen>();
        private static float Overshoot(float t) => 1 + 2.2f * Mathf.Pow(t - 1, 3) + 1.2f * Mathf.Pow(t - 1, 2);

        public static Color Apply(string key, Rect card)
        {
            Color keep = GUI.color;
            float now = Time.unscaledTime;
            if (!seen.TryGetValue(key, out Seen s) || Time.frameCount - s.Frame > 3) s.Since = now;
            s.Frame = Time.frameCount; seen[key] = s;
            float t = Mathf.Clamp01((now - s.Since) / OpenTime);
            if (t >= 1) return keep;
            GUI.color = new Color(keep.r, keep.g, keep.b, keep.a * Mathf.Clamp01(t * 1.6f));
            if (!PresentationSettings.ReducedMotion) ArcadeSkin.ScaleAround(Vector2.one * (.88f + .12f * Overshoot(t)), card.center + Vector2.up * ((1 - t) * 18));
            return keep;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() => seen.Clear();
    }
}
