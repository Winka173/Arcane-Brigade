using UnityEngine;

namespace ArcaneBrigade
{
    public static class CarnivalBalance
    {
        public static float ShopPrice(int wave)
        {
            float stage = Mathf.Max(0, wave - 1);
            return Mathf.Min(10000, (1 + stage * 0.12f + stage * stage * 0.009f) * (1 + Mathf.Min(0.2f, Mathf.Max(0, wave - 3) * 0.01f)) * (1 + Mathf.Max(0, wave - 7) * .04f) * SwarmCoinPressure(wave)); // requirement 25/10/2026: late prices a notch lower (was .065 a wave from wave 7)
        }
        /// <summary>Balance pass 19/10/2026: swarm coins grow far faster than prices after wave 5 (bot wallet / mean offer 5.6 at
        /// waves 1–5, 12.5 at 6–10, 16 at 11–19; 18 relics bought per late shop). Prices catch up to x2.2 by wave 13, keeping
        /// roughly the early game's "three slots and a couple of rerolls" per shop. Docs/Balance_Pass_2026-10-19.md.</summary>
        // round 2 (target: wallet / mean offer ~5-6 in waves 11-19): x1.9 at wave 8, x2.5 at wave 10, x3.4 from wave 13
        /// <summary>Round 3 (play-test: still too rich): gold credited per coin value, 0.75 (was 1). Coin drops and visuals unchanged.</summary>
        public const float CoinGoldValue = .65f; // requirement 22/10/2026: the economy a notch tighter overall (was .75)
        public static int KillCoinBatch(int wave) => wave >= 15 ? 12 : wave >= 10 ? 8 : 4;
        public static float SwarmCoinPressure(int wave) => 1 + Mathf.Min(2.4f, Mathf.Max(0, wave - 5) * .3f);

        // 22/10 play-test: +20% base health for every enemy, including bosses, on top of the previous 1.2 scale.
        public static float EnemyHealth(int wave) => 1.44f * (1 + Mathf.Pow(Mathf.Max(0, wave - 1) / 12f, 1.45f)) * (1 + Mathf.Pow(Mathf.Max(0, wave - 4) / 16f, 1.4f) * 1.4f);
        // 22/10 play-test: larger crowds at every wave, including the opening nights.
        public const float EnemyCountScale = 1.25f;
        public static float EnemyDensity(int wave) => EnemyCountScale * (1 + Mathf.Min(3, Mathf.Max(0, wave - 3) * 0.15f)); // .13 -> .15 (requirement 22/10/2026: steeper wave growth)
        public static float SpawnInterval(int wave) => Mathf.Max(0.25f, (1.4f - Mathf.Min(3, Mathf.Max(1, wave)) * 0.035f) / (1 + Mathf.Max(0, wave - 3) * 0.18f));
        public static float BossBulk(int wave) => Mathf.Lerp(4.6f, 7.5f, Mathf.Clamp01((wave - 4) / 16f));
        // Keep wave-one pace; substantially reduce both the growth and its eventual ceiling.
        // requirement 22/10/2026: enemies never speed up with the waves and stay slow (was 1.08 rising to ~1.18 by wave 20)
        public static float EnemySpeed(int wave) => 1f;
        /// <summary>Requirement 22/10/2026: the game felt too fast; every body walks at 88% (enemies, friends, the hero).</summary>
        public const float UnitPace = .8f; // .88 -> .8 (requirement 22/10/2026: still too hurried)
        /// <summary>Enemies a further 90% on top: still fast next to the brigade (requirement 22/10/2026).</summary>
        public const float EnemyPace = .9f;
        /// <summary>Move-speed bonuses count 60% (requirement 22/10/2026: they scaled too much); penalties count in full.</summary>
        public static float MoveBonus(float bonus) => bonus > 0 ? bonus * .6f : bonus;
        /// <summary>Ranged friends reach 85% as far (requirement 22/10/2026).</summary>
        public const float RangedAllyReach = .85f;
        /// <summary>The fast ones are reined in: speed above 1.3 counts 40% (Sleep Mimic 2.8 -> 1.9, Siren Seal 1.85 -> 1.52).</summary>
        public static float CapEnemySpeed(float speed) => speed > 1.3f ? 1.3f + (speed - 1.3f) * .4f : speed;
        public static float EnemyDamage(int wave) => 1.15f * (1 + Mathf.Min(3, Mathf.Max(0, wave - 3) * 0.11f)) * (1 + Mathf.Min(1.4f, Mathf.Max(0, wave - 5) * 0.085f)); // .09/.07 -> .11/.085 (requirement 22/10/2026: about +25% damage by wave 20)
        public static float RecruitDamage(ItemRarity rarity) => 1 + (int)rarity * 0.18f;
    }
}
