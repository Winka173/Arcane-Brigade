using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K13MovementPolicy
    {
        public static Vector2 Steer(CombatSimulation simulation)
        {
            Vector2 position = simulation.Champion.Position;
            Vector2 target = new Vector2(Mathf.Cos(simulation.Elapsed * 0.27f) * 7, Mathf.Sin(simulation.Elapsed * 0.27f) * 4.2f);
            float nearestPickup = 4;
            foreach (PickupState pickup in simulation.Pickups)
            {
                float distance = (pickup.Position - position).sqrMagnitude;
                if (distance >= nearestPickup) continue;
                target = pickup.Position; nearestPickup = distance;
            }
            Vector2 direction = Vector2.ClampMagnitude(target - position, 1);
            foreach (UnitState unit in simulation.Units)
            {
                if (unit.Faction != Faction.Enemy || !unit.Alive) continue;
                Vector2 offset = position - unit.Position; float distance = offset.magnitude;
                float danger = unit.Definition.IsBoss ? 3.2f : unit.Definition.Radius + 1.6f;
                if (distance < danger && distance > 0.01f) direction += offset.normalized * (danger - distance) * 3;
            }
            foreach (ProjectileState projectile in simulation.Projectiles)
            {
                if (projectile.Faction != Faction.Enemy) continue;
                Vector2 offset = position - (projectile.Position + projectile.Velocity * 0.18f);
                float distance = offset.magnitude;
                if (distance < 1.1f && distance > 0.01f) direction += offset.normalized * (1.1f - distance) * 4;
            }
            return Vector2.ClampMagnitude(direction, 1);
        }
        public static void Purchase(RunSession session, RunCatalog catalog)
        {
            foreach (ShopOffer offer in session.State.SpecialOffers.OrderByDescending(offer => Score(session, catalog, offer)).ToArray())
                if (Score(session, catalog, offer) > 0 && session.CanBuy(offer)) session.Buy(offer.Token);
            session.State.SpecialShopVisited = true;
            for (int pass = 0; pass < 12; pass++)
            {
                foreach (ShopOffer offer in session.State.Offers.OrderByDescending(offer => Score(session, catalog, offer)).ToArray())
                    if (Score(session, catalog, offer) > 0 && session.CanBuy(offer)) session.Buy(offer.Token);
                int cheapest = session.State.Offers.Where(offer => !offer.Sold && Score(session, catalog, offer) > 0).Select(offer => offer.Price).DefaultIfEmpty(20).Min();
                if (session.State.Gold <= session.RerollCost + cheapest || !session.Reroll()) break;
            }
        }
        internal static float Score(RunSession session, RunCatalog catalog, ShopOffer offer)
        {
            if (offer.Kind == OfferKind.Companion) return ShopMergePreview.Ready(session, offer) ? 100 : 50;
            float value = 0;
            foreach (StatModifier modifier in catalog.Item(offer.Id).EffectiveModifiers)
            {
                float weight = modifier.Effect switch
                {
                    ItemEffect.EnemyCount or ItemEffect.EnemyHealth or ItemEffect.EnemySpeed => -80,
                    ItemEffect.Damage or ItemEffect.CompanionDamage or ItemEffect.AttackSpeed or ItemEffect.Projectiles or ItemEffect.BulletPierce or ItemEffect.BulletSplit => 30,
                    ItemEffect.Armor or ItemEffect.Dodge or ItemEffect.Heal or ItemEffect.Health => 18,
                    ItemEffect.MoveSpeed => 12,
                    ItemEffect.Luck or ItemEffect.Discount or ItemEffect.Income => 8,
                    _ => 2
                };
                value += modifier.Value * weight;
            }
            if (offer.Currency == ShopCurrency.Health && offer.Price > session.State.Health * 0.25f) return -1;
            return value;
        }
    }
}
