using UnityEngine;

namespace ArcaneBrigade
{
    public static class K10StatRules
    {
        public static readonly string[] Names = { "Score multiplier", "Collision damage", "Flat melee damage", "Flat ranged damage", "Attack range", "Dodge", "Income", "Bullet pierce", "Bullet split", "Bullet bounce chance", "Bullet size", "Bullet speed", "Bullet travel distance", "Knockback", "Champion size", "Companion size", "Enemy count", "Enemy health", "Enemy speed", "Heart healing", "Chest quality", "Recruit discount", "Relic discount", "Shop rarity", "Health correction", "Damage correction", "Speed correction", "Armor correction", "Healing correction", "Collision frequency" };
        public static bool Percentage(ItemEffect effect) => effect != ItemEffect.CollisionDamage && effect != ItemEffect.MeleeDamage && effect != ItemEffect.RangedDamage && effect != ItemEffect.Income && effect != ItemEffect.BulletPierce && effect != ItemEffect.BulletSplit && effect != ItemEffect.HeartHealing;
        public static string Name(ItemEffect effect)
        {
            if (effect == ItemEffect.ChestReach) return "Chest opening radius";
            int index = (int)effect - (int)ItemEffect.ScoreMultiplier;
            return index >= 0 && index < Names.Length ? Names[index] : "Invalid effect";
        }
        public static float Factor(float bonus, float minimum = 0.1f, float maximum = 10f) => Mathf.Clamp(1 + bonus, minimum, maximum);
        public static float MaximumHealth(RunSession session, RunCatalog catalog)
            => catalog.Unit(session.State.ChampionId).ReferenceHero == "Card Queen" ? PlayerBuildBalance.CardQueenHealth : catalog.Unit(session.State.ChampionId).ReferenceHero == "Periwinky" ? Mathf.Clamp(session.State.Gold, 20, 400) : OctoberBalance.MaximumHealth((catalog.Unit(session.State.ChampionId).Health * PlayerBuildBalance.HeroVitality(session.State.ChampionId) + session.Bonus(ItemEffect.Health)) * Factor(session.Bonus(ItemEffect.HealthCorrection)), session.State.Wave);

        public static void ApplyUnit(RunSession session, RunCatalog catalog, UnitDefinition unit, UnitDefinition source, WeaponDefinition original, int tier, bool companion)
        {
            if (catalog.StatRevision == 0) return;
            ModifierScope scope = companion ? ModifierScope.Companions : ModifierScope.Champion;
            WeaponDefinition weapon = unit.Weapon;
            ItemEffect flat = weapon.Kind == WeaponKind.Projectile ? ItemEffect.RangedDamage : ItemEffect.MeleeDamage;
            weapon.Damage = Mathf.Max(0, original.Damage + OctoberBalance.FlatDamage(session.Bonus(flat, scope), session.State.Wave)) * session.DamageMultiplier(source, tier, companion) * Factor(session.Bonus(ItemEffect.DamageCorrection, scope));
            float range = Factor(OctoberBalance.Soften(session.Bonus(ItemEffect.AttackRange, scope), .5f, .15f), .4f, 1.8f);
            weapon.Range *= range;
            weapon.ProjectileSpeed *= Factor(session.Bonus(ItemEffect.BulletSpeed, scope));
            weapon.ProjectileLifetime *= range * Factor(session.Bonus(ItemEffect.BulletDistance, scope)) / Factor(session.Bonus(ItemEffect.BulletSpeed, scope));
            float size = Factor(session.Bonus(ItemEffect.BulletSize, scope), 0.25f, 4);
            weapon.ProjectileVisualWidth *= size;
            weapon.ProjectileRadius *= size;
            weapon.ProjectilePierce = Mathf.Clamp(weapon.ProjectilePierce + Mathf.FloorToInt(session.Bonus(ItemEffect.BulletPierce, scope)), 0, 64);
            weapon.ProjectileSplit = Mathf.Clamp(weapon.ProjectileSplit + Mathf.FloorToInt(session.Bonus(ItemEffect.BulletSplit, scope)), 0, 6);
            weapon.ProjectileBounceChance = Mathf.Clamp01(weapon.ProjectileBounceChance + session.Bonus(ItemEffect.BulletBounce, scope));
            weapon.PushDistance = Mathf.Min(.45f, weapon.PushDistance * Factor(session.Bonus(ItemEffect.Knockback, scope), .1f, 2));
            float body = Factor(session.Bonus(companion ? ItemEffect.CompanionSize : ItemEffect.BodySize), 0.4f, 2.5f);
            unit.BodyHeight *= body;
            unit.BodyScale = body;
            unit.Radius *= body;
            unit.Speed *= Factor(session.Bonus(ItemEffect.SpeedCorrection, scope));
            unit.CollisionDamage = Mathf.Max(0, unit.CollisionDamage + session.Bonus(ItemEffect.CollisionDamage, scope)) * Factor(session.Bonus(ItemEffect.Damage, scope) + session.Bonus(companion ? ItemEffect.CompanionDamage : ItemEffect.ChampionDamage, scope)) * Factor(session.Bonus(ItemEffect.DamageCorrection, scope));
            unit.CollisionFrequency = Mathf.Clamp(unit.CollisionFrequency / Factor(session.Bonus(ItemEffect.CollisionFrequency, scope)), 0.15f, 4);
            if (!companion) unit.Health = MaximumHealth(session, catalog);
        }
    }
}
