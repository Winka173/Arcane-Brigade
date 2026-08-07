using UnityEngine;

namespace ArcaneBrigade
{
    public enum Faction { Champion, Companion, Enemy }

    [CreateAssetMenu(menuName = "Arcane Brigade/Unit")]
    public sealed class UnitDefinition : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public Sprite Body;
        public Sprite[] AnimationFrames = new Sprite[0];
        public AuthoredMotionSet AuthoredMotion;
        public WeaponDefinition Weapon;
        public float Health = 100f;
        // Run-compiled friend defense and time-only skill fill; armor is measured in points.
        public float FriendArmorPoints, SkillHaste, BondLuck;
        public float Speed = 4.8f;
        public float Radius = 0.25f;
        public float BodyHeight = 0.8f;
        public float BodyScale = 1f;
        [Range(0, 1)] public float KnockbackResistance;
        public bool SpecialRecruit;
        /// <summary>Set on a deployed friend's built body when it is lucid (LucidRite); never authored.</summary>
        [System.NonSerialized] public bool Lucid;
        public ItemRarity Rarity;
        public bool RecruitThemeLocked;
        public float ContactDamage = 4f;
        public float ContactInterval = 0.8f;
        public float CollisionDamage;
        public float CollisionFrequency = 0.8f;
        public float CriticalChance;
        public Color Tint = Color.white;
        public string[] Tags = new string[0];
        public PassiveKind Passive;
        public float PassiveStrength = 1f;
        public string Description;
        public string ReferenceHero;
        public string ReferenceCompanion;
        public string ExclusiveHero;
        public bool GiftOnly;
        public StatModifier[] ChampionModifiers = new StatModifier[0];
        public string[] RecruitThemes = new string[0];
        public HeroMechanic HeroMechanic;
        public StatModifier[] CompanionModifiers = new StatModifier[0];
        public static float ModifierScale(int tier) => Mathf.Pow(2f, Mathf.Clamp(tier, 1, 3) - 1);
        public int Price = 20;
        public int UnlockWave;
        public bool IsBoss;
        public int BossPattern;
        public int CombatTier = 1;
        // Run-compiled form: source assets and saved roster identities stay intact.
        public BuildMorphKind BuildMorph;
        public int BuildMorphStage;
        public EnemyBehavior Behavior;
        public HostileShot Shot;
        public BodyMaterial Material;
        public BossMotif Motif;
        public int EvolutionStage;
        public EnemyMutation Mutation;
        public SpecialEnemyKind SpecialEnemy;
        public WaitingEnemyKind WaitingKind; // enemy roster v2 (WaitingEnemyRoster); None on every older species
    }

    public enum PassiveKind { None, HealChampion, Scavenge, Rally, ChillAura }
    public enum HeroMechanic { None, LanternStride, ParcelExpress, DreamBreath, RootedOracle, SoloMaw, GlassGambler, TideCollector, ClockRunner, MirrorGuard, TreasureTax }
    public enum EnemyBehavior { Standard, Door, Magnet, Mimic, Curtain, Hopper, Charger, CoinThief, Puffer, Spore, Sweeper, Hydra, Burrower,
        // enemy variety (24/10/2026, EnemyVariety)
        Shielder, Splitter, Blinker, Sniper, Summoner, Armored, Tethered, Stomper }
    public enum HostileShot { Straight, Weave, Hook, Drift, Accelerate }
    public enum BodyMaterial { Paper, Metal, Wax, Fabric, Glass, Wood }
    public enum BossMotif { Stage, Office, Toys }
}
