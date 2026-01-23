using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class CombatSimulation
    {
        private bool MoveCarnivalEnemy(UnitState enemy, Vector2 offset, float delta)
        {
            EnemyBehavior behavior = enemy.Definition.Behavior;
            if (Elapsed >= enemy.NextBehavior)
            {
                enemy.NextBehavior = Elapsed + (behavior == EnemyBehavior.Charger ? 4.2f : 5.2f) + enemy.Id % 4 * 0.23f;
                enemy.AbilityOrigin = enemy.Position;
                enemy.AbilityTarget = ClampPosition(enemy.Position + Vector2.ClampMagnitude(offset, 4), enemy.Definition.Radius);
                enemy.AttackDirection = offset.normalized;
                enemy.AttackStarted = Elapsed; enemy.AttackWindup = behavior == EnemyBehavior.Hopper || behavior == EnemyBehavior.Burrower ? 0.42f : 0.7f;
                if (behavior == EnemyBehavior.Hopper || behavior == EnemyBehavior.Burrower)
                {
                    enemy.BehaviorUntil = Elapsed + 1.2f;
                    enemy.NextContact = enemy.NextAttack = enemy.BehaviorUntil;
                    Danger(ExpandedFieldKind.FallingProp, enemy, enemy.AbilityTarget, 0.8f, 1.2f, 0.4f);
                }
                else if (behavior == EnemyBehavior.Charger)
                {
                    enemy.BehaviorUntil = Elapsed + 1.7f; enemy.TelegraphUntil = Elapsed + 0.7f;
                    Danger(ExpandedFieldKind.Tentacle, enemy, enemy.AbilityTarget, 0.28f, 0.7f, 0.5f);
                }
                else if (behavior == EnemyBehavior.CoinThief)
                {
                    for (int index = pickups.Count - 1; index >= 0; index--)
                    {
                        PickupState pickup = pickups[index];
                        if (pickup.Kind != LootKind.Coin || pickup.Attracting || pickup.Value <= 0 || pickup.Value > 1000 - enemy.StolenCoins || (pickup.Position - enemy.Position).sqrMagnitude > 4) continue;
                        enemy.StolenCoins += pickup.Value; pickups.RemoveAt(index);
                    }
                    if (enemy.StolenCoins > 0) enemy.AbilityFireAt = Elapsed + enemy.AttackWindup;
                }
                else if (behavior == EnemyBehavior.Puffer)
                {
                    enemy.TelegraphUntil = Elapsed + 1;
                    Danger(ExpandedFieldKind.Shockwave, enemy, enemy.Position, 2.5f, 1, 0.9f);
                }
                else if (behavior == EnemyBehavior.Spore)
                    Danger(ExpandedFieldKind.ShadowPool, enemy, ClampPosition(Champion.Position, 1), 1, 1.1f, 1.8f);
                else if (behavior == EnemyBehavior.Sweeper)
                    for (int index = 0; index < 3; index++)
                        Danger(ExpandedFieldKind.FallingProp, enemy, Champion.Position + Vector2.right * ((index - 1) * 1.4f), 0.65f, 0.8f + index * 0.18f, 0.3f);
                else if (behavior == EnemyBehavior.Hydra)
                    enemy.AbilityFireAt = Elapsed + enemy.AttackWindup;
                events.Add(new CombatEvent(CombatEventKind.Windup, enemy.Position, Faction.Enemy, subject: enemy, weapon: enemy.Definition.Weapon));
            }
            if (enemy.AbilityFireAt > 0 && Elapsed >= enemy.AbilityFireAt)
            {
                enemy.AbilityFireAt = 0;
                EmitProjectile(enemy, enemy.Definition.Weapon.Damage, false, NewCause(enemy.Id));
                events.Add(new CombatEvent(CombatEventKind.Fire, enemy.Position, Faction.Enemy, subject: enemy, weapon: enemy.Definition.Weapon));
            }
            if (behavior == EnemyBehavior.Hopper || behavior == EnemyBehavior.Burrower)
            {
                if (enemy.BehaviorUntil <= Elapsed) { enemy.BodyLift = 0; return false; }
                float progress = Mathf.Clamp01((Elapsed - (enemy.BehaviorUntil - 1.2f)) / 1.2f);
                float travel = Mathf.Clamp01((progress - 0.35f) / 0.65f);
                enemy.Position = Vector2.Lerp(enemy.AbilityOrigin, enemy.AbilityTarget, Mathf.SmoothStep(0, 1, travel));
                enemy.BodyLift = Mathf.Sin(travel * Mathf.PI) * (behavior == EnemyBehavior.Hopper ? 1.1f : -0.45f);
                return true;
            }
            if (behavior == EnemyBehavior.Charger && enemy.BehaviorUntil > Elapsed)
            {
                if (enemy.TelegraphUntil > Elapsed) return true;
                Vector2 direction = (enemy.AbilityTarget - enemy.AbilityOrigin).normalized;
                enemy.Position = ClampPosition(enemy.Position + direction * enemy.Definition.Speed * delta * 2.7f, enemy.Definition.Radius);
                enemy.Facing = direction;
                return true;
            }
            return false;
        }
    }
}
