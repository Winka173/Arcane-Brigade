using UnityEngine;

namespace ArcaneBrigade
{
    public enum ItemEffect { Damage, AttackSpeed, Health, MoveSpeed, Pickup, Roam, Interceptor, Heal, Crit, Projectiles, Interest, Armor, ChampionDamage, CompanionDamage, CompanionAttackSpeed, Discount }
    public enum ModifierScope { Team, Champion, Companions, Run }
    public enum ItemRarity { Common, Uncommon, Rare, Mythic }
    public enum StackPolicy { Additive, Unique }
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
        public int StackLimit => Stacking == StackPolicy.Unique ? 1 : (MaximumStacks > 0 ? MaximumStacks : 100000);
        public int EffectiveStacks(int count) => Mathf.Min(count, StackLimit);
        public StatModifier[] EffectiveModifiers => Modifiers != null && Modifiers.Length > 0 ? Modifiers : new[] { new StatModifier { Effect = Effect, Scope = RunModifierRules.DefaultScope(Effect), Value = Value } };
    }

    [System.Serializable]
    public sealed class BondDefinition
    {
        public string Id;
        public string Name;
        public int Threshold = 2;
        public ItemEffect Effect;
        public float Value = 0.06f;
    }

    [System.Serializable]
    public sealed class AltarRecipe
    {
        public string Id;
        public string FirstUnit;
        public string SecondUnit;
        public string ResultUnit;
        public int EggCost = 3;
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
        public int NormalWaves = 20;
        public float WaveSeconds = 84f;
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
        public int MaximumTier = 3;
        public float TierDamageMultiplier = 3.6f;
        public float TierSupportMultiplier = 3f;

        public UnitDefinition Unit(string id)
        {
            foreach (UnitDefinition unit in Champions) if (unit.Id == id) return unit;
            foreach (UnitDefinition unit in Companions) if (unit.Id == id) return unit;
            return null;
        }
        public WeaponDefinition Weapon(string id) { foreach (WeaponDefinition weapon in Weapons) if (weapon.Id == id) return weapon; return null; }
        public ItemDefinition Item(string id) { foreach (ItemDefinition item in Items) if (item.Id == id) return item; return null; }
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
