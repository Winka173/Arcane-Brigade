using UnityEngine;

namespace ArcaneBrigade
{
    public enum ItemEffect { Damage, AttackSpeed, Health, MoveSpeed, Pickup, Roam, Interceptor, Heal, Crit, Projectiles, Interest, Armor, ChampionDamage, CompanionDamage, CompanionAttackSpeed, Discount, Luck, TreasureDetection, ScoreMultiplier, CollisionDamage, MeleeDamage, RangedDamage, AttackRange, Dodge, Income, BulletPierce, BulletSplit, BulletBounce, BulletSize, BulletSpeed, BulletDistance, Knockback, BodySize, CompanionSize, EnemyCount, EnemyHealth, EnemySpeed, HeartHealing, ChestQuality, RecruitDiscount, RelicDiscount, ShopRarity, HealthCorrection, DamageCorrection, SpeedCorrection, ArmorCorrection, HealCorrection, CollisionFrequency, ChestReach, ChainChance, RecordedDamage }
    public enum ModifierScope { Team, Champion, Companions, Run }
    public enum ItemRarity { Common, Uncommon, Rare, Mythic, Crimson, Rainbow }
    public enum StackPolicy { Additive, Unique }
    public enum SignatureRelicKind { None, HarvestReceipt, ReflectiveInk, CarbonRibbon, OfficeLens, TeaCoaster, ScratchRecord, TreasureRibbon, PatchSpring, ThermalInk, RepairCoupon }
    [System.Flags]
    public enum ShopPool { Gold = 1, Eggs = 2, Both = Gold | Eggs }

    [System.Serializable]
    public sealed class StatModifier
    {
        public ItemEffect Effect;
        public ModifierScope Scope;
        public float Value;
    }

    [System.Serializable]
    public sealed class BuildLinkRule
    {
        public string Id;
        public string Name;
        public string Description;
        public string[] ItemIds;
    }

    [System.Serializable]
    public sealed class ItemDefinition
    {
        public string Id;
        public string Name;
        public string Description;
        public int Price;
        public ItemEffect Effect;
        public float Value;
        public Sprite Icon;
        public int UnlockWave;
        public StatModifier[] Modifiers = new StatModifier[0];
        public ItemRarity Rarity;
        public StackPolicy Stacking;
        public int MaximumStacks = 100000;
        public int ShopWeight = 100;
        public ShopPool Pools = ShopPool.Both;
        public ShopCurrency AltarCurrency = ShopCurrency.Eggs;
        public int AltarPrice = 2;
        public SignatureRelicKind SignatureRole;
        public RelicMechanic Mechanic;
        public RelicCategory Category;
        public string ReferenceName;
        public string[] ShopThemes = new string[0];
        public int StackLimit => Stacking == StackPolicy.Unique ? 1 : (MaximumStacks > 0 ? MaximumStacks : 100000);
        public int EffectiveStacks(int count) => Mathf.Min(count, StackLimit);
        public StatModifier[] EffectiveModifiers
        {
            get
            {
                StatModifier[] authored = Modifiers != null && Modifiers.Length > 0 ? Modifiers : new[] { new StatModifier { Effect = Effect, Scope = RunModifierRules.DefaultScope(Effect), Value = Value } };
                if (Rarity != ItemRarity.Rainbow) return authored;
                // requirement 22/10/2026: the rainbow gifts felt weak; their positive power bonuses are worth 1.5x (counts like projectiles stay)
                if (rainbow != null && rainbowFrom == authored) return rainbow;
                rainbowFrom = authored; rainbow = new StatModifier[authored.Length];
                for (int i = 0; i < authored.Length; i++)
                {
                    StatModifier m = authored[i];
                    rainbow[i] = new StatModifier { Effect = m.Effect, Scope = m.Scope, Value = m.Value > 0 && RainbowScaled(m.Effect) ? m.Value * RainbowPower : m.Value };
                }
                return rainbow;
            }
        }
        public const float RainbowPower = 1.5f;
        [System.NonSerialized] private StatModifier[] rainbow, rainbowFrom;
        private static bool RainbowScaled(ItemEffect e) => e is ItemEffect.Damage or ItemEffect.AttackSpeed or ItemEffect.Health or ItemEffect.Heal or ItemEffect.Crit
            or ItemEffect.ChampionDamage or ItemEffect.CompanionDamage or ItemEffect.CompanionAttackSpeed or ItemEffect.MeleeDamage or ItemEffect.RangedDamage
            or ItemEffect.CollisionDamage or ItemEffect.Knockback or ItemEffect.AttackRange or ItemEffect.Luck;
    }

    [System.Serializable]
    public sealed class BondThresholdBonus
    {
        public int Threshold;
        public float Value;
    }

    [System.Serializable]
    public sealed class BondDefinition
    {
        public const float PositiveRewardScale = .9f;
        public string Id;
        public string Name;
        public int Threshold = 2;
        public Sprite Icon;
        public ItemEffect Effect;
        public float Value = 0.06f;
        public BondThresholdBonus[] ExtraBonuses = new BondThresholdBonus[0];

        public int ActiveLevel(int count) => Mathf.Max(0, count) / Mathf.Max(1, Threshold);

        public int NextThreshold(int count)
        {
            long next = ((long)ActiveLevel(count) + 1) * Mathf.Max(1, Threshold);
            foreach (BondThresholdBonus bonus in ExtraBonuses ?? new BondThresholdBonus[0])
                if (bonus.Threshold > count) next = System.Math.Min(next, bonus.Threshold);
            return (int)System.Math.Min(int.MaxValue, next);
        }

        public float Reward(int count)
        {
            float reward = ActiveLevel(count) * Value;
            foreach (BondThresholdBonus bonus in ExtraBonuses ?? new BondThresholdBonus[0])
                if (count >= bonus.Threshold) reward += bonus.Value;
            return BalanceReward(reward);
        }
        /// <summary>Whole-number stats (pierce, split, shots) are floored when compiled: scaling them by 0.9 turned the first level's
        /// +1 into +0.9 = nothing (Runic School, Little Horns did nothing at two members until 19/10/2026), so they are not scaled.</summary>
        public static bool WholeNumber(ItemEffect effect) => effect == ItemEffect.BulletPierce || effect == ItemEffect.BulletSplit || effect == ItemEffect.Projectiles;
        public float BalanceReward(float reward) => reward > 0 && Effect != ItemEffect.EnemyCount && Effect != ItemEffect.EnemyHealth && Effect != ItemEffect.EnemySpeed && !WholeNumber(Effect) ? reward * PositiveRewardScale : reward;
    }

    [System.Serializable]
    public sealed class AltarRecipe
    {
        public string Id;
        public string FirstUnit;
        public string SecondUnit;
        public string ThirdUnit;
        public string ResultUnit;
        public int EggCost = 3;
        public int MinimumMaterialTier = 1;
        public int ResultTier = 2;
    }

    [CreateAssetMenu(menuName = "Arcane Brigade/Run Catalog")]
    public sealed class RunCatalog : ScriptableObject
    {
        public string Version = "run-0.1";
        public CheckpointCatalog Arena;
        public Sprite ProjectileDisc;
        public Vector2 WorldSize = new Vector2(34f, 22f);
        public float CombatViewHeight = 5.4f;
        public float RestViewHeight = 6f;
        public UnitDefinition[] Champions;
        public UnitDefinition[] Companions;
        public WeaponDefinition[] Weapons;
        public UnitDefinition[] Enemies;
        /// <summary>One named elite per map in map order (Waiting Room, Post Office, Lamp Hall, Archive Vault); built by
        /// EnemyVarietyBuilder when its art exists, null until then.</summary>
        public UnitDefinition[] MapElites = new UnitDefinition[4];
        public UnitDefinition[] Bosses;
        public ItemDefinition[] Items;
        public BondDefinition[] Bonds;
        public AltarRecipe[] Recipes;
        public WaveModifier[] WaveModifiers = new WaveModifier[0];
        public BuildLinkRule[] BuildLinks = new BuildLinkRule[0];
        public int EconomyRevision;
        public int AltarRevision;
        public float AltarBaseChance;
        public float AltarChancePerEgg;
        public int LootRevision;
        public int SignatureRevision;
        public int ExpansionRevision;
        /// <summary>1 once the content expansion's fixed hero pools are applied (ContentExpansionBuilder, 18/10/2026).</summary>
        public int PoolRevision;
        public int ContentRevision;
        public int ReviewRevision;
        public int StatRevision;
        public int HeroRevision;
        public Sprite[] ArenaPropSprites = new Sprite[0];
        public ProcRule[] ProcRules = new ProcRule[0];
        public Sprite EggSprite;
        public Sprite HeartSprite;
        public Sprite ChestSprite;
        public int NormalWaves = 20;
        public float FirstWaveSeconds = 35f;
        public float WaveIncrement = 2f;
        public float WaveSeconds = 55f;
        public float BossHealthMultiplier = 0.55f;
        public int BossEvery = 4;
        public int StartingGold = 35;
        public int Income = 20;
        public int InterestDivisor = 10;
        public int InterestCap = 15;
        public int RerollPrice = 4;
        public int RerollPriceStep;
        public int ShopSlots = 3;
        public int SpecialShopEvery = 2;
        public int SpecialShopPrice = 2;
        public int MaximumTier = 5;
        public float TierDamageMultiplier = 3.6f;
        public float TierSupportMultiplier = 3f;

        public UnitDefinition Unit(string id)
        {
            foreach (UnitDefinition unit in Champions) if (unit.Id == id) return unit;
            foreach (UnitDefinition unit in Companions) if (unit.Id == id) return unit;
            return null;
        }
        public WeaponDefinition Weapon(string id) { foreach (WeaponDefinition weapon in Weapons) if (weapon.Id == id) return weapon; return null; }
        public ItemDefinition Item(string id) { foreach (ItemDefinition item in Items) if (item.Id == id) return item; return ActOneCanon.StoryItem(id) ?? WiseHeroItems.For(this, id); }
    }

    [System.Serializable]
    public sealed class WaveModifier
    {
        public string Name;
        public int Every;
        public float SpawnMultiplier = 1f;
        public float BudgetMultiplier = 1f;
        public int GoldPerKill = 2;
        public float Healing;
    }
}
