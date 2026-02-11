using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Sprites for the Lantern Flare and the shopkeeper's bell pickups. Painted art loads from MienCho/ActOne/Pickup_* when it
    /// arrives; until then the lantern icon and the Marching Bell relic icon stand in.</summary>
    public static class PacingArtwork
    {
        private static Sprite flare, bell;
        private static bool loaded;
        public static Sprite Flare(RunCatalog catalog) { Load(catalog); return flare; }
        public static Sprite Bell(RunCatalog catalog) { Load(catalog); return bell; }
        public static Sprite For(LootKind kind, RunCatalog catalog) => kind == LootKind.Flare ? Flare(catalog) : kind == LootKind.Bell ? Bell(catalog) : null;

        private static void Load(RunCatalog catalog)
        {
            if (loaded && (bell != null || catalog == null)) return;
            loaded = true;
            flare = Resources.Load<Sprite>("MienCho/ActOne/Pickup_LanternFlare") ?? Resources.Load<Sprite>("MienCho/Lanterns/Intact");
            bell = Resources.Load<Sprite>("MienCho/ActOne/Pickup_ShopkeeperBell") ?? catalog?.Item("marching-bell")?.Icon;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void Reset() { flare = bell = null; loaded = false; }
    }
}
