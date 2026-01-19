using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class CombatSimulation
    {
        private bool MoveExpandedEnemy(UnitState enemy, float delta)
        {
            if (!catalog.ExpandedContent || enemy.Definition.Behavior == EnemyBehavior.Standard) return false;
            Vector2 offset = Champion.Position - enemy.Position;
            if (enemy.Definition.Behavior == EnemyBehavior.Mimic && !enemy.Awakened)
            {
                if (offset.sqrMagnitude > 9f && enemy.Health >= enemy.Definition.Health) return true;
                enemy.Awakened = true;
                events.Add(new CombatEvent(CombatEventKind.MimicWake, enemy.Position, enemy.Faction, subject: enemy));
                enemy.BehaviorUntil = Elapsed + 0.8f;
                enemy.TelegraphUntil = enemy.BehaviorUntil;
            }
            if (enemy.Definition.Behavior == EnemyBehavior.Magnet && Elapsed >= enemy.NextBehavior)
            {
                enemy.NextBehavior = Elapsed + 4f;
                enemy.BehaviorUntil = Elapsed + 1.2f;
                Field(ExpandedFieldKind.Link, enemy, enemy.Position, Vector2.zero, 3f, 0, 0, 1.2f, NewCause(enemy.Id));
            }
            if (enemy.Definition.Behavior == EnemyBehavior.Magnet && enemy.BehaviorUntil > Elapsed)
                foreach (PickupState pickup in pickups)
                    if (pickup.Kind == LootKind.Coin && (pickup.Position - enemy.Position).sqrMagnitude < 9f)
                        pickup.Position = Vector2.MoveTowards(pickup.Position, enemy.Position, delta * 1.5f);
            if (enemy.Definition.Behavior == EnemyBehavior.Curtain && Elapsed >= enemy.NextBehavior)
            {
                enemy.NextBehavior = Elapsed + 6f;
                Field(ExpandedFieldKind.Curtain, enemy, ClampPosition(Champion.Position, 0.8f), Vector2.right, 1.2f, 5f, 1.3f, 0.3f, NewCause(enemy.Id));
            }
            return false;
        }

        private void ProcessBossMotif()
        {
            if (!catalog.ExpandedContent || Boss == null || !Boss.Alive || bossMotifConsumed || Boss.Health > Boss.Definition.Health * 0.5f) return;
            bossMotifConsumed = true;
            events.Add(new CombatEvent(CombatEventKind.BossPhase, Boss.Position, Boss.Faction, subject: Boss));
            CombatCause cause = NewCause(Boss.Id);
            if (Boss.Definition.Motif == BossMotif.Office)
            {
                for (int index = 0; index < 3; index++)
                    Field(ExpandedFieldKind.OfficeHazard, Boss, ClampPosition(Champion.Position + Vector2.right * ((index - 1) * 3f), 1.3f), Vector2.up, 1.1f, 8, 1.6f + index * 0.25f, 0.3f, cause);
            }
            else if (Boss.Definition.Motif == BossMotif.Toys)
            {
                for (int index = 0; index < 4; index++)
                {
                    Vector2 direction = Quaternion.Euler(0, 0, index * 90f) * Vector2.right;
                    Field(ExpandedFieldKind.ToyHazard, Boss, ClampPosition(Boss.Position + direction * 3f, 1.4f), direction, 1.2f, 7, 1.5f, 0.3f, cause);
                }
            }
            else
            {
                Field(ExpandedFieldKind.Curtain, Boss, ClampPosition(Champion.Position + Vector2.left * 1.8f, 1.2f), Vector2.up, 1f, 6, 1.8f, 0.3f, cause);
                Field(ExpandedFieldKind.Curtain, Boss, ClampPosition(Champion.Position + Vector2.right * 1.8f, 1.2f), Vector2.up, 1f, 6, 1.8f, 0.3f, cause);
            }
        }

        private float ExpandedDamage(UnitState target, float damage, Faction source, Vector2 direction, WeaponDefinition weapon)
        {
            if (!catalog.ExpandedContent) return damage;
            if (source != Faction.Enemy && weapon != null && target.Definition.Behavior == EnemyBehavior.Door && direction.sqrMagnitude > 0.01f && Vector2.Dot(direction.normalized, target.Facing) < -0.45f)
                damage *= 0.5f;
            return damage;
        }

#if UNITY_EDITOR
        public void ExpandedDamageFixture(UnitState target, float damage, Vector2 direction, WeaponDefinition weapon) => ApplyDamage(target, damage, Faction.Companion, direction: direction, weapon: weapon, cause: NewCause(Champion.Id));
#endif
    }
}
