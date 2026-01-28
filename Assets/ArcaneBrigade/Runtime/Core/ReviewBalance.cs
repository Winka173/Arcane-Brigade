using UnityEngine;

namespace ArcaneBrigade
{
    public static class ReviewBalance
    {
        public static ShopPool ExclusivePool(ItemDefinition item)
        {
            if (item.Pools != ShopPool.Both) return item.Pools;
            return item.AltarCurrency == ShopCurrency.Health || item.SignatureRole != SignatureRelicKind.None ? ShopPool.Eggs : ShopPool.Gold;
        }
        public static float RelicValue(ItemEffect effect, float value)
        {
            if (value <= 0) return value;
            return effect switch
            {
                ItemEffect.Damage or ItemEffect.ChampionDamage or ItemEffect.CompanionDamage or ItemEffect.DamageCorrection => Mathf.Max(0.2f, value * 2),
                ItemEffect.AttackSpeed or ItemEffect.CompanionAttackSpeed => Mathf.Max(0.18f, value * 2),
                ItemEffect.MeleeDamage or ItemEffect.RangedDamage => Mathf.Max(4, value * 2),
                ItemEffect.CollisionDamage => Mathf.Max(10, value * 2),
                ItemEffect.Health => Mathf.Max(24, value * 2),
                ItemEffect.Armor => Mathf.Max(0.22f, value * 1.8f),
                ItemEffect.Dodge => Mathf.Max(0.08f, value * 1.5f),
                ItemEffect.MoveSpeed => Mathf.Max(0.1f, value * 1.5f),
                ItemEffect.Crit => Mathf.Max(0.1f, value * 1.6f),
                ItemEffect.AttackRange or ItemEffect.BulletSize or ItemEffect.Knockback => Mathf.Max(0.25f, value * 1.8f),
                ItemEffect.Heal => Mathf.Max(1, value * 1.8f),
                ItemEffect.Pickup => Mathf.Max(1.25f, value * 1.8f),
                ItemEffect.BulletSplit or ItemEffect.BulletPierce or ItemEffect.Projectiles => Mathf.Max(1, Mathf.Ceil(value * 1.5f)),
                _ => value
            };
        }
        public static Vector2 ShopHalfExtents(float viewHeight) => new Vector2(viewHeight * 16 / 9f - 0.8f, viewHeight - 0.65f);
    }
}
