using UnityEngine;

namespace ArcaneBrigade
{
    [CreateAssetMenu(menuName = "Arcane Brigade/Checkpoint Catalog")]
    public sealed class CheckpointCatalog : ScriptableObject
    {
        public string DataVersion = "checkpoint-0.1";
        public int Seed = 131026;
        public UnitDefinition Champion;
        public UnitDefinition Companion;
        public UnitDefinition[] StartingCompanions;
        public bool ExplicitStartingRoster;
        public UnitDefinition Boss;
        public bool CompanionInterceptor;
        public float PassiveHeal;
        public float Armor;
        public float Dodge;
        public float HeartHealing;
        public float ChestQuality;
        public bool EnableHeroMechanics;
        public UnitDefinition MeleeEnemy;
        public UnitDefinition RangedEnemy;
        public Sprite Hand;
        public Sprite Projectile;
        public Sprite Coin;
        public Sprite ImpactSprite;
        public GameObject UnitPrefab;
        public Vector2 ArenaSize = new Vector2(22f, 11f);
        public float WaveDuration = 60f;
        public float SpawnInterval = 1.5f;
        public int EnemyBudget = 18;
        public float SpawnSafetyRadius = 4f;
        public float CompanionLeash = 2.2f;
        public float ChestOpenRadius = 0.85f;
        public float CompanionRoamInnerRadius = 0.9f;
        public float CompanionRoamOuterRadius = 1.9f;
        public float CompanionPatrolSpeed = 1.4f;
        public float CompanionAcceleration = 8f;
        public float CompanionSeparation = 0.75f;
        public float CompanionRetargetMin = 1.2f;
        public float CompanionRetargetMax = 2.8f;
        public float PickupRadius = 0.7f;
        public int GoldPerKill = 5;
        public bool ExtendedLoot;
        public float Luck;
        public float TreasureDetection;
        public float EggDropChance = 0.025f;
        public float HeartDropChance = 0.04f;
        public float ChestInterval = 24f;
        public ActiveProc[] Procs = new ActiveProc[0];
        public bool EnableArenaToys;
        public bool EnableBossArenaPhase;
        public int RunWave = 1;
        public int HarvestStacks;
        public int ReflectiveInkStacks;
        public bool CarbonRibbon;
        public int OfficeLensStacks;
        public bool ExpandedContent;
        public UnitDefinition[] EnemyPool = new UnitDefinition[0];
        public int TeaCoasterStacks;
        public int ChorusStacks;
        public int TreasureStacks;
        public bool PatchSpring;
        public bool ThermalInk;
        public int RepairStacks;
    }
}
