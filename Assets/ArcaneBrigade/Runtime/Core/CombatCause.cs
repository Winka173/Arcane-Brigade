using System;

namespace ArcaneBrigade
{
    public enum LootKind { Coin, Egg, Heart, Chest, Flare, Bell, Prism, Magnet, Spring, Globe }
    public enum ProcTrigger { CoinCollected, ChestOpened, EnemyKilled }
    public enum ProcAction { HealChampion, DamagePulse }

    public readonly struct CombatCause
    {
        public readonly int RootId;
        public readonly int SourceUnitId;
        public readonly string ItemId;
        public readonly int Depth;
        // Measurement metadata never changes proc, execute or recursion semantics.
        public readonly string AttributionId;
        public CombatCause(int rootId, int sourceUnitId, string itemId = "", int depth = 0, string attributionId = null)
        {
            RootId = rootId; SourceUnitId = sourceUnitId; ItemId = itemId; Depth = depth; AttributionId = attributionId;
        }
        public CombatCause Child(string itemId) => new CombatCause(RootId, SourceUnitId, itemId, Depth + 1);
        public CombatCause WithAttribution(string id) => new CombatCause(RootId, SourceUnitId, ItemId, Depth, id);
    }

    [Serializable]
    public sealed class ProcRule
    {
        public string Id;
        public string ItemId;
        public ProcTrigger Trigger;
        public ProcAction Action;
        public float Power;
        public float Radius;
        public float Cooldown;
        public float Chance = 1f;
    }

    public sealed class ActiveProc
    {
        public ProcRule Rule;
        public int Stacks;
    }
}
