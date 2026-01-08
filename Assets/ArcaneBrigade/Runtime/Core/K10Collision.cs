using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class CombatSimulation
    {
        private void ProcessOutgoingCollision(UnitState source)
        {
            if (source.Definition.CollisionDamage <= 0 || Elapsed < source.NextCollision) return;
            bool struck = false;
            foreach (UnitState enemy in units)
            {
                if (!enemy.Alive || enemy.Faction != Faction.Enemy) continue;
                float reach = source.Definition.Radius + enemy.Definition.Radius;
                if ((source.Position - enemy.Position).sqrMagnitude > reach * reach) continue;
                ApplyDamage(enemy, source.Definition.CollisionDamage, source.Faction, "Collision", (enemy.Position - source.Position).normalized, cause: NewCause(source.Id));
                struck = true;
            }
            if (struck) source.NextCollision = Elapsed + source.Definition.CollisionFrequency;
        }
    }
}
