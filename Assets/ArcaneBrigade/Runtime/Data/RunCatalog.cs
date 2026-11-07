using UnityEngine;
using System.Collections.Generic;

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
                if (Modifiers == null || Modifiers.Length == 0)
                {
                    ModifierScope scope = RunModifierRules.DefaultScope(Effect);
                    if (legacy == null || legacy[0].Effect != Effect || legacy[0].Scope != scope || legacy[0].Value != Value)
                        legacy = new[] { new StatModifier { Effect = Effect, Scope = scope, Value = Value } };
                }
                StatModifier[] authored = Modifiers != null && Modifiers.Length > 0 ? Modifiers : legacy;
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
        [System.NonSerialized] private StatModifier[] rainbow, rainbowFrom, legacy;
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
            return RunModifierRules.Nice(Effect, BalanceReward(reward)); // a round number, shown and played
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
        /// <summary>One named elite per map, by MapTheme (Waiting Room, Post Office, Lamp Hall, Archive Vault, Glasshouse, Theatre); built by
        /// EnemyVarietyBuilder when its art exists, null until then.</summary>
        public UnitDefinition[] MapElites = new UnitDefinition[6];
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

        private UnitDefinition[] indexedHeroes, indexedFriends;
        private WeaponDefinition[] indexedWeapons;
        private ItemDefinition[] indexedItems;
        private readonly Dictionary<string, (UnitDefinition value, UnitDefinition[] array, int slot)> unitIndex = new Dictionary<string, (UnitDefinition, UnitDefinition[], int)>(System.StringComparer.Ordinal);
        private readonly Dictionary<string, (WeaponDefinition value, int slot)> weaponIndex = new Dictionary<string, (WeaponDefinition, int)>(System.StringComparer.Ordinal);
        private readonly Dictionary<string, (ItemDefinition value, int slot)> itemIndex = new Dictionary<string, (ItemDefinition, int)>(System.StringComparer.Ordinal);
        private void EnsureIndexes()
        {
            if (indexedHeroes == Champions && indexedFriends == Companions && indexedWeapons == Weapons && indexedItems == Items) return;
            indexedHeroes = Champions; indexedFriends = Companions; indexedWeapons = Weapons; indexedItems = Items;
            unitIndex.Clear(); weaponIndex.Clear(); itemIndex.Clear();
            foreach (var array in new[] { Champions, Companions })
                for (int i = 0; i < array.Length; i++)
                    if (array[i] != null && !string.IsNullOrEmpty(array[i].Id) && !unitIndex.ContainsKey(array[i].Id)) unitIndex.Add(array[i].Id, (array[i], array, i));
            for (int i = 0; i < Weapons.Length; i++) if (Weapons[i] != null && !string.IsNullOrEmpty(Weapons[i].Id) && !weaponIndex.ContainsKey(Weapons[i].Id)) weaponIndex.Add(Weapons[i].Id, (Weapons[i], i));
            for (int i = 0; i < Items.Length; i++) if (Items[i] != null && !string.IsNullOrEmpty(Items[i].Id) && !itemIndex.ContainsKey(Items[i].Id)) itemIndex.Add(Items[i].Id, (Items[i], i));
        }
        private void Reindex() { indexedHeroes = null; EnsureIndexes(); }
        public UnitDefinition Unit(string id)
        {
            if (id == null) return null;
            EnsureIndexes();
            if (unitIndex.TryGetValue(id, out var found) && found.value != null && found.value.Id == id)
            {
                if (found.array[found.slot] == found.value) return found.value;
                Reindex(); return Unit(id);
            }
            foreach (var array in new[] { Champions, Companions })
                for (int i = 0; i < array.Length; i++) if (array[i].Id == id) { unitIndex[id] = (array[i], array, i); return array[i]; }
            return null;
        }
        public WeaponDefinition Weapon(string id)
        {
            if (id == null) return null;
            EnsureIndexes();
            if (weaponIndex.TryGetValue(id, out var found) && found.value != null && found.value.Id == id)
            {
                if (Weapons[found.slot] == found.value) return found.value;
                Reindex(); return Weapon(id);
            }
            for (int i = 0; i < Weapons.Length; i++) if (Weapons[i].Id == id) { weaponIndex[id] = (Weapons[i], i); return Weapons[i]; }
            return null;
        }
        public ItemDefinition Item(string id)
        {
            if (id == null) return null;
            EnsureIndexes();
            if (itemIndex.TryGetValue(id, out var found) && found.value != null && found.value.Id == id)
            {
                if (Items[found.slot] == found.value) return found.value;
                Reindex(); return Item(id);
            }
            for (int i = 0; i < Items.Length; i++) if (Items[i].Id == id) { itemIndex[id] = (Items[i], i); return Items[i]; }
            return ActOneCanon.StoryItem(id) ?? WiseHeroItems.For(this, id) ?? StarlightThreadItem.For(id);
        }

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
