using System;
using UnityEngine;

namespace ArcaneBrigade
{
    [Flags]
    public enum BossOrganRole { None = 0, Lash = 1, Volley = 2, Props = 4, Pool = 8, Pulse = 16, Vacuum = 32, Locomotion = 64, Tempo = 128, Rupture = 256 }

    [Serializable]
    public sealed class BossOrganDefinition
    {
        public string Id;
        public string Name;
        public string BreakSummary;
        public BossOrganRole Role;
        public Vector2 Offset;
        public float Height = 0.25f;
        public float Durability = 0.18f;
        public Sprite Rest;
        public Sprite Charged;
        public Sprite Release;
        public Sprite Scar;
    }

    [Serializable]
    public sealed class BossAnatomyEntry
    {
        public string BossId;
        public BossOrganDefinition[] Organs = new BossOrganDefinition[0];
    }

    public sealed class BossAnatomy : ScriptableObject
    {
        public BossAnatomyEntry[] Bosses = new BossAnatomyEntry[0];
        private static BossAnatomy cached;
        public static BossAnatomyEntry For(UnitDefinition boss)
        {
            if (boss == null || !boss.IsBoss) return null;
            if (boss.Id == NightmareRules.CustomerId) return NightmareAssets.Current?.Anatomy;
            if (boss.Id == "OutsideStageKeeper") return OutsideStageAssets.Current == null ? null : OutsideStageAssets.Current.Anatomy;
            if (cached == null) cached = Resources.Load<BossAnatomy>("BossAnatomy");
            if (cached == null) return null;
            foreach (BossAnatomyEntry entry in cached.Bosses) if (entry.BossId == boss.Id) return entry;
            return null;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Invalidate() => cached = null;
    }
}
