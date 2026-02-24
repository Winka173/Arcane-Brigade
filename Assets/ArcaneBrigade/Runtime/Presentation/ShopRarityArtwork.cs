using System;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class ShopRarityArtwork : ScriptableObject
    {
        public Sprite[] Frames = Array.Empty<Sprite>();
        private static ShopRarityArtwork cached;
        public static ShopRarityArtwork Current => cached != null ? cached : cached = Resources.Load<ShopRarityArtwork>("ShopRarityArtwork");
        public static int Row(ItemRarity rarity) => rarity == ItemRarity.Rainbow ? 3 : rarity >= ItemRarity.Mythic ? 2 : rarity == ItemRarity.Rare ? 1 : 0;
        /// <summary>Painted frames per rarity row (four rows: uncommon, rare, mythic, rainbow): 4 in RarityBloom_v1, 12 in v2.</summary>
        public int PerRarity => Frames.Length >= 16 && Frames.Length % 4 == 0 ? Frames.Length / 4 : 0;
        public Sprite Frame(ItemRarity rarity, int frame) => PerRarity > 0 ? Frames[Row(rarity) * PerRarity + (int)Mathf.Repeat(frame, PerRarity)] : null;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Invalidate() { cached = null; }
    }
}
