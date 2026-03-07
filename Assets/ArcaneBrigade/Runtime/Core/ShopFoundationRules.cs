namespace ArcaneBrigade
{
    public static class ShopFoundationRules
    {
        // One of the existing gold-shop item slots supplies a useful basic before the first boss.
        // Eligibility/theme filtering still happens in RunSession before this shortlist is built.
        public static bool Useful(UnitDefinition hero, ItemDefinition item)
        {
            if (item.Rarity > ItemRarity.Uncommon) return false;
            foreach (var modifier in item.EffectiveModifiers)
                if (modifier.Value < 0 || modifier.Value > 0 && (modifier.Effect == ItemEffect.EnemyHealth || modifier.Effect == ItemEffect.EnemyCount || modifier.Effect == ItemEffect.EnemySpeed)) return false;
            foreach (var modifier in item.EffectiveModifiers)
            {
                if (modifier.Value <= 0 || !RunModifierRules.Applies(modifier.Scope, ModifierScope.Champion)) continue;
                switch (modifier.Effect)
                {
                    case ItemEffect.Damage: case ItemEffect.ChampionDamage: case ItemEffect.AttackSpeed:
                    case ItemEffect.Crit: case ItemEffect.Heal: case ItemEffect.Dodge: case ItemEffect.DamageCorrection:
                        return true;
                    case ItemEffect.MeleeDamage:
                        if (hero.Weapon.Kind != WeaponKind.Projectile) return true;
                        break;
                    case ItemEffect.RangedDamage:
                        if (hero.Weapon.Kind == WeaponKind.Projectile) return true;
                        break;
                    case ItemEffect.Health:
                        if (hero.ReferenceHero != "Card Queen" && hero.ReferenceHero != "Periwinky") return true;
                        break;
                    case ItemEffect.Armor:
                        if (hero.ReferenceHero != "Card Queen") return true;
                        break;
                }
            }
            return false;
        }
    }
}
