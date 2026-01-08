using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public static class CompanionMotion
    {
        public static Vector2 Step(UnitState unit, UnitState champion, IReadOnlyList<UnitState> units, UnitState target, CheckpointCatalog catalog, float elapsed, float deltaTime)
        {
            float outer = Mathf.Max(0.1f, Mathf.Min(catalog.CompanionRoamOuterRadius, catalog.CompanionLeash * 0.9f));
            float inner = Mathf.Clamp(catalog.CompanionRoamInnerRadius, 0f, outer);
            Vector2 destination = champion.Position + unit.RoamOffset;
            bool championMoving = (champion.Position - champion.PreviousPosition).sqrMagnitude > 0.000001f;
            bool resting = !championMoving && elapsed < unit.RoamRestUntil;
            if (!resting && (elapsed >= unit.NextRoamTime || (destination - unit.Position).sqrMagnitude < 0.04f))
            {
                if (!championMoving && unit.RoamRandom.NextDouble() < 0.4)
                {
                    unit.RoamRestUntil = elapsed + Mathf.Lerp(0.6f, 1.8f, (float)unit.RoamRandom.NextDouble());
                    unit.NextRoamTime = unit.RoamRestUntil;
                    unit.RoamOffset = unit.Position - champion.Position;
                    resting = true;
                }
                else
                {
                float angle = (float)unit.RoamRandom.NextDouble() * Mathf.PI * 2f;
                float radius = Mathf.Sqrt(Mathf.Lerp(inner * inner, outer * outer, (float)unit.RoamRandom.NextDouble()));
                unit.RoamOffset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                float minimumTime = Mathf.Max(0.2f, catalog.CompanionRetargetMin);
                float maximumTime = Mathf.Max(minimumTime, catalog.CompanionRetargetMax);
                unit.NextRoamTime = elapsed + Mathf.Lerp(minimumTime, maximumTime, (float)unit.RoamRandom.NextDouble());
                destination = champion.Position + unit.RoamOffset;
                }
            }
            bool pursuing = target != null && unit.Definition.Weapon.Kind == WeaponKind.Melee && (target.Position - champion.Position).magnitude <= catalog.CompanionLeash - target.Definition.Radius;
            if (pursuing)
                destination = Vector2.MoveTowards(target.Position, champion.Position, unit.Definition.Radius + target.Definition.Radius);
            Vector2 toDestination = destination - unit.Position;
            float fromChampion = (unit.Position - champion.Position).magnitude;
            float catchup = Mathf.InverseLerp(outer * 0.75f, catalog.CompanionLeash * 0.9f, fromChampion);
            float maximumSpeed = Mathf.Max(0f, unit.Definition.Speed);
            float speed = Mathf.Lerp(Mathf.Min(catalog.CompanionPatrolSpeed, maximumSpeed), maximumSpeed, pursuing ? 1f : catchup);
            Vector2 desired = toDestination.normalized * Mathf.Min(speed, toDestination.magnitude * 3f);
            if (resting && !pursuing) desired = Vector2.zero;
            float separation = Mathf.Max(0.05f, catalog.CompanionSeparation);
            for (int index = 0; index < units.Count; index++)
            {
                UnitState other = units[index];
                if (other == unit || !other.Alive || other.Faction == Faction.Enemy) continue;
                Vector2 offset = unit.PreviousPosition - other.PreviousPosition;
                float distance = offset.magnitude;
                float personalSpace = other.Faction == Faction.Champion ? Mathf.Max(separation, unit.Definition.Radius + other.Definition.Radius + 0.35f) : separation;
                if (distance >= personalSpace) continue;
                Vector2 direction = distance > 0.001f ? offset / distance : new Vector2(Mathf.Cos(unit.Id * 2.4f), Mathf.Sin(unit.Id * 2.4f));
                float strength = other.Faction == Faction.Champion ? 5f : 3f;
                desired += direction * (1f - distance / personalSpace) * strength;
            }
            desired = Vector2.ClampMagnitude(desired, maximumSpeed);
            unit.Velocity = Vector2.MoveTowards(unit.Velocity, desired, Mathf.Max(0.1f, catalog.CompanionAcceleration) * deltaTime);
            return unit.Position + unit.Velocity * deltaTime;
        }
    }
}
