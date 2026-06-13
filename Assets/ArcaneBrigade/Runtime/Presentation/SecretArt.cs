using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The secret rooms' art, one painting per room and per relic (requirement 24/10/2026: the first sheets were blurry when
    /// drawn large, and the Tailor and the Understudy had each other's doorway). Resources/SecretEvents/{Kind}_Closed, {Kind}_Open,
    /// {Kind}_Artifact and Item_{id}; the old sheets stand in for any painting not there yet.</summary>
    public static class SecretArt
    {
        private static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();
        /// <summary>The relic each room is known for (shown at its left).</summary>
        private static readonly string[] RoomItem = { "SecretWoodHeart", "SecretThread", "SecretMirror", "SecretTicket", "SecretHour", "SecretDream", "SecretCrown", "SecretLetter" };
        private static Sprite Load(string name)
        {
            if (!cache.TryGetValue(name, out Sprite sprite)) cache[name] = sprite = Resources.Load<Sprite>("SecretEvents/" + name);
            return sprite;
        }
        public static Sprite Portal(SecretEventKind kind, bool open) => Load(kind + (open ? "_Open" : "_Closed")) ?? SecretEventAssets.Current.Portals[(int)kind * 2 + (open ? 1 : 0)];
        public static Sprite Relic(SecretEventKind kind) => Load("Item_" + RoomItem[(int)kind]) ?? SecretEventAssets.Current.Relics[(int)kind];
        public static Sprite Artifact(SecretEventKind kind) => Load(kind + "_Artifact") ?? SecretEventAssets.Current.Relics[(int)kind + 8];
        /// <summary>Gives the secret relics their new paintings everywhere they are shown (shop lists, cards, the room).</summary>
        public static void UseItemPaintings(RunCatalog catalog)
        {
            if (catalog == null) return;
            foreach (string id in SecretEventAssets.RewardItems)
            {
                Sprite sprite = Load("Item_" + id); ItemDefinition item = catalog.Item(id);
                if (sprite != null && item != null) item.Icon = sprite;
            }
        }
    }
}
