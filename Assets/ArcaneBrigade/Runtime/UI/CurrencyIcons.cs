using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>One picture per currency (play-test 20/10/2026, items 8 and 9): the HUD's coin, heart and egg are used for every
    /// price, stat line, popup and pickup, so gold, health and eggs each look the same everywhere.</summary>
    public static class CurrencyIcons
    {
        private static RunCatalog catalog;
        public static void Use(RunCatalog value) { if (value != null) catalog = value; }
        private static RunCatalog Catalog => catalog != null ? catalog : catalog = Resources.Load<RunCatalog>("RunCatalog");
        public static Sprite Coin => Catalog != null && Catalog.Arena != null ? Catalog.Arena.Coin : null;
        public static Sprite Heart => Catalog != null ? Catalog.HeartSprite : null;
        public static Sprite Egg => Catalog != null ? Catalog.EggSprite : null;
        /// <summary>The currency picture a stat line shows instead of its own glyph: gold for income, the heart for max HP.</summary>
        public static Sprite For(ItemEffect effect) => effect == ItemEffect.Income || effect == ItemEffect.Interest ? Coin
            : effect == ItemEffect.Health || effect == ItemEffect.HealthCorrection || effect == ItemEffect.HeartHealing ? Heart : null;
    }
}
