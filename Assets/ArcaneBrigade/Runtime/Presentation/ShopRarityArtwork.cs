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
        public Sprite Frame(ItemRarity rarity, int frame) => Frames.Length == 16 ? Frames[Row(rarity) * 4 + Mathf.Clamp(frame, 0, 3)] : null;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Invalidate() { cached = null; }
    }
}
