using UnityEngine;

namespace ArcaneBrigade
{
    public static class MeleeTactics
    {
        // Weapon reach touches the target's body, rather than requiring its centre
        // to enter the blade. Large bodies must also be able to reach past contact.
        public static float Reach(UnitState unit,UnitState target)
            =>Mathf.Max(unit.Definition.Weapon.Range+target.Definition.Radius,unit.Definition.Radius+target.Definition.Radius+.08f);
        public static Vector2 Engage(UnitState unit, UnitState target)
        {
            Vector2 near = unit.Position - target.Position;
            if (near.sqrMagnitude < 0.0001f) near = -unit.Facing;
            if (near.sqrMagnitude < 0.0001f) near = Vector2.left;
            if (unit.PendingAttack && near.magnitude >= unit.Definition.Radius + target.Definition.Radius && near.magnitude <= Mathf.Max(unit.Definition.Radius + target.Definition.Radius, unit.Definition.Weapon.Range * 0.7f)) return unit.Position;
            float side = (unit.Id % 3 - 1) * 0.24f;
            Vector2 anchored = new Vector2(Mathf.Cos(unit.MeleeApproachAngle), Mathf.Sin(unit.MeleeApproachAngle));
            if (unit.MeleeApproachTarget != target.Id || near.magnitude > unit.Definition.Weapon.Range * 1.5f && Vector2.Dot(near.normalized, anchored) < 0.5f)
            {
                unit.MeleeApproachTarget = target.Id; unit.MeleeApproachAngle = Mathf.Atan2(near.y, near.x) + side;
            }
            Vector2 direction = new Vector2(Mathf.Cos(unit.MeleeApproachAngle), Mathf.Sin(unit.MeleeApproachAngle));
            float distance = Mathf.Min(Reach(unit,target)-.04f,Mathf.Max(unit.Definition.Radius + target.Definition.Radius + 0.04f, unit.Definition.Weapon.Range * 0.55f));
            if(unit.Definition.CollisionDamage>0)distance=Mathf.Min(distance,unit.Definition.Radius+target.Definition.Radius-.02f);
            return target.Position + direction * distance;
        }
        public static void Aim(UnitState unit, UnitState target, float delta)
        {
            if (!unit.PendingAttack || target == null || !target.Alive) return;
            float lead = Mathf.Clamp(unit.ImpactTime - unit.AttackStarted, 0, 0.18f);
            Vector2 desired = target.Position + Vector2.ClampMagnitude(target.Velocity * lead, 0.3f) - unit.Position;
            if (desired.sqrMagnitude < 0.0001f) return;
            float current = Mathf.Atan2(unit.AttackDirection.y, unit.AttackDirection.x) * Mathf.Rad2Deg;
            float next = Mathf.Atan2(desired.y, desired.x) * Mathf.Rad2Deg;
            float angle = Mathf.MoveTowardsAngle(current, next, 300 * delta) * Mathf.Deg2Rad;
            unit.AttackDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)); unit.Facing = unit.AttackDirection;
        }
    }
}
