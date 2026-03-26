using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public static class EnemyInteractionRules
    {
        public static bool CanBlock(ProjectileState projectile, UnitState target) => projectile.Faction == Faction.Enemy && target.Alive && target.Faction == Faction.Enemy && target.Id != projectile.SourceUnitId && target.EntranceUntil <= 0 && (target.Definition.IsBoss || target.Definition.BodyHeight >= 1.2f);
        public static bool HasCollateral(ExpandedFieldKind kind) => kind == ExpandedFieldKind.FallingProp || kind == ExpandedFieldKind.Shockwave || kind == ExpandedFieldKind.Tentacle;
    }

    public sealed partial class CombatSimulation
    {
        private readonly List<UnitState> deathReactions = new List<UnitState>();
        private readonly List<UnitState> heavyNeighbours = new List<UnitState>();
        private void SeparateFromHeavyEnemy(UnitState unit, float delta)
        {
            if (!catalog.ExpandedContent || unit.Definition.IsBoss || unit.Definition.BodyHeight >= 1.2f || unit.BodyLift != 0 || unit.EntranceUntil > Elapsed) return;
            Vector2 extent = Vector2.one * (maximumRadius + unit.Definition.Radius);
            spatial.Query(unit.Position - extent, unit.Position + extent, heavyNeighbours, true);
            foreach (UnitState neighbour in heavyNeighbours)
            {
                if (neighbour == unit || !neighbour.Alive || neighbour.Faction != Faction.Enemy || neighbour.EntranceUntil > Elapsed || !neighbour.Definition.IsBoss && neighbour.Definition.BodyHeight < 1.2f) continue;
                Vector2 offset = unit.Position - neighbour.Position;
                float clearance = unit.Definition.Radius + neighbour.Definition.Radius;
                if (offset.sqrMagnitude >= clearance * clearance) continue;
                Vector2 direction = offset.sqrMagnitude < 0.00001f ? (unit.Id % 2 == 0 ? Vector2.left : Vector2.right) : offset.normalized;
                unit.Position = ClampPosition(unit.Position + direction * Mathf.Min(clearance - offset.magnitude, delta * 1.4f), unit.Definition.Radius);
            }
        }
        private void QueueDeathReaction(UnitState target)
        {
            if (catalog.ExpandedContent && target.Faction == Faction.Enemy && target.Definition.Behavior == EnemyBehavior.Puffer && deathReactions.Count < 32)
            {
                deathReactions.Add(target);
                if (blastingDepth >= 0) blastDepth[target.Id] = blastingDepth; // felled by another puffer's burst: the next link of a chain
                SchedulePufferBurst(target); // N15: its burst blows up the enemies around it too
            }
        }
        private void ProcessDeathReactions()
        {
            int count = Mathf.Min(8, deathReactions.Count);
            for (int index = 0; index < count; index++)
            {
                UnitState source = deathReactions[index];
                Danger(ExpandedFieldKind.FallingProp, source, source.Position, 1.6f, 0.6f, 0.3f);
            }
            deathReactions.RemoveRange(0, count);
        }
        private void ProcessHostileCollateral(ExpandedField field)
        {
            if (!catalog.ExpandedContent || !EnemyInteractionRules.HasCollateral(field.Kind)) return;
            foreach (UnitState target in units)
            {
                if (!target.Alive || target.Faction != Faction.Enemy || target == field.Source || field.CollateralVictims.Contains(target.Id) || !CarnivalHazards.Contains(field, target.Position, target.Definition.Radius, Elapsed)) continue;
                field.CollateralVictims.Add(target.Id);
                float damage = Mathf.Min(field.Damage * 0.6f, target.Definition.Health * (target.Definition.IsBoss ? 0.015f : 0.12f));
                ApplyDamage(target, damage, Faction.Enemy, "Hostile collision", (target.Position - field.Position).normalized, cause: field.Cause.Child("hostile-collateral"));
                if (target.Alive && !target.Definition.IsBoss)
                {
                    target.Position = ClampPosition(target.Position + (target.Position - field.Position).normalized * 0.2f * (1 - target.Definition.KnockbackResistance), target.Definition.Radius);
                    spatial.Move(target);
                }
            }
        }
    }
}
