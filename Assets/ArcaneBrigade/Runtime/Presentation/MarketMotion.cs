using UnityEngine;

namespace ArcaneBrigade
{
    public static class MarketMotion
    {
        public const float ArrivalDuration = 1.55f;
        public const float DepartureDuration = 1.35f;
        public const float HandoffDuration = 1.6f;
        public const float MerchantArrivalDelay = 0.18f;
        public const float MerchantDepartureDelay = 0.38f;
        public const float StockArrivalDelay = 0.7f;
        public const float StockStagger = 0.12f;
        public const float CarpetDepartureDelay = 0.55f;

        public static float Progress(float elapsed, float delay, float duration) => Mathf.Clamp01((elapsed - delay) / duration);

        public static float Settle(float elapsed, float landing, float strength)
        {
            float age = elapsed - landing;
            return age < 0 || age > 0.45f ? 0 : Mathf.Sin(age * 23) * Mathf.Exp(-age * 9) * strength;
        }
    }
}
