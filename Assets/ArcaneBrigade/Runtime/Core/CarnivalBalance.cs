using UnityEngine;

namespace ArcaneBrigade
{
    public static class CarnivalBalance
    {
        public static float ShopPrice(int wave)
        {
            float stage = Mathf.Max(0, wave - 1);
            return Mathf.Min(10000, (1 + stage * 0.12f + stage * stage * 0.009f) * (1 + Mathf.Min(0.2f, Mathf.Max(0, wave - 3) * 0.01f)) * (1 + Mathf.Max(0, wave - 7) * .065f));
        }

        public static float EnemyHealth(int wave) => 1.2f * (1 + Mathf.Pow(Mathf.Max(0, wave - 1) / 12f, 1.45f)) * (1 + Mathf.Pow(Mathf.Max(0, wave - 4) / 16f, 1.4f) * 1.4f);
        public static float EnemyDensity(int wave) => 1 + Mathf.Min(3, Mathf.Max(0, wave - 3) * 0.13f);
        public static float SpawnInterval(int wave) => Mathf.Max(0.25f, (1.4f - Mathf.Min(3, Mathf.Max(1, wave)) * 0.035f) / (1 + Mathf.Max(0, wave - 3) * 0.18f));
        public static float BossBulk(int wave) => Mathf.Lerp(4.6f, 7.5f, Mathf.Clamp01((wave - 4) / 16f));
        public static float EnemySpeed(int wave) => 1.08f * (1 + 1.8f * (1 - Mathf.Exp(-Mathf.Max(0, wave - 1) * 0.05f)));
        public static float EnemyDamage(int wave) => 1.15f * (1 + Mathf.Min(3, Mathf.Max(0, wave - 3) * 0.09f)) * (1 + Mathf.Min(1.4f, Mathf.Max(0, wave - 5) * 0.07f));
        public static float RecruitDamage(ItemRarity rarity) => 1 + (int)rarity * 0.18f;
    }
}
