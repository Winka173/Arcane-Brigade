using UnityEngine;

namespace ArcaneBrigade
{
    public static class CarnivalBalance
    {
        public static float ShopPrice(int wave)
        {
            float stage = Mathf.Max(0, wave - 1);
            return Mathf.Min(10000, (1 + stage * 0.12f + stage * stage * 0.009f) * (1 + Mathf.Min(0.2f, Mathf.Max(0, wave - 3) * 0.01f)) * (1 + Mathf.Max(0, wave - 7) * .065f) * SwarmCoinPressure(wave));
        }
        /// <summary>Balance pass 19/10/2026: swarm coins grow far faster than prices after wave 5 (bot wallet / mean offer 5.6 at
        /// waves 1–5, 12.5 at 6–10, 16 at 11–19; 18 relics bought per late shop). Prices catch up to x2.2 by wave 13, keeping
        /// roughly the early game's "three slots and a couple of rerolls" per shop. Docs/Balance_Pass_2026-10-19.md.</summary>
        // round 2 (owner target: wallet / mean offer ~5-6 in waves 11-19): x1.9 at wave 8, x2.5 at wave 10, x3.4 from wave 13
        /// <summary>Round 3 (owner play-test: still too rich): gold credited per coin value, 0.75 (was 1). Coin drops and visuals unchanged.</summary>
        public const float CoinGoldValue = .65f; // owner 22/10/2026: the economy a notch tighter overall (was .75)
        public static int KillCoinBatch(int wave) => wave >= 15 ? 12 : wave >= 10 ? 8 : 4;
        public static float SwarmCoinPressure(int wave) => 1 + Mathf.Min(2.4f, Mathf.Max(0, wave - 5) * .3f);

        // 22/10 play-test: +20% base health for every enemy, including bosses, on top of the previous 1.2 scale.
        public static float EnemyHealth(int wave) => 1.44f * (1 + Mathf.Pow(Mathf.Max(0, wave - 1) / 12f, 1.45f)) * (1 + Mathf.Pow(Mathf.Max(0, wave - 4) / 16f, 1.4f) * 1.4f);
        // 22/10 play-test: larger crowds at every wave, including the opening nights.
        public const float EnemyCountScale = 1.25f;
        public static float EnemyDensity(int wave) => EnemyCountScale * (1 + Mathf.Min(3, Mathf.Max(0, wave - 3) * 0.13f));
        public static float SpawnInterval(int wave) => Mathf.Max(0.25f, (1.4f - Mathf.Min(3, Mathf.Max(1, wave)) * 0.035f) / (1 + Mathf.Max(0, wave - 3) * 0.18f));
        public static float BossBulk(int wave) => Mathf.Lerp(4.6f, 7.5f, Mathf.Clamp01((wave - 4) / 16f));
        // Keep wave-one pace; substantially reduce both the growth and its eventual ceiling.
        public static float EnemySpeed(int wave) => 1.08f * (1 + .25f * (1 - Mathf.Exp(-Mathf.Max(0, wave - 1) * .025f)));
        public static float EnemyDamage(int wave) => 1.15f * (1 + Mathf.Min(3, Mathf.Max(0, wave - 3) * 0.09f)) * (1 + Mathf.Min(1.4f, Mathf.Max(0, wave - 5) * 0.07f));
        public static float RecruitDamage(ItemRarity rarity) => 1 + (int)rarity * 0.18f;
    }
}
