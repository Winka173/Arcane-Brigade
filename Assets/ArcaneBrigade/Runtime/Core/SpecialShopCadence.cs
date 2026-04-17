namespace ArcaneBrigade
{
    public sealed partial class RunSnapshot
    {
        public int NextSpecialShopWave;
        public int SpecialShopRollWave = -1;
    }

    public sealed partial class RunSession
    {
        public bool SpecialShopEnabled => !MetaNoDreamShop;
        private void InitializeSpecialShopSchedule(bool migrating)
        {
            // Pure seeded initialization keeps restore/preview from consuming the shop RNG.
            uint seed = unchecked((uint)State.Seed ^ (uint)State.Wave * 2654435761u ^ 0xD3EA4u);
            int origin = migrating ? State.Wave : 0;
            State.NextSpecialShopWave = BoundedCounter((long)origin + 3 + (seed & 1));
            State.SpecialShopRollWave = migrating && State.Phase == SessionPhase.Shop ? State.Wave : -1;
        }
    }
}
