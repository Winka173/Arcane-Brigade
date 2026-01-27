using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class CombatSimulation
    {
        private void ProcessOutgoingCollision(UnitState source)
        {
            if (source.Faction == Faction.Champion || source.Definition.CollisionDamage <= 0 || Elapsed < source.NextCollision) return;
            bool struck = false;
            foreach (UnitState enemy in units)
            {
                if (!enemy.Alive || enemy.Faction != Faction.Enemy || enemy.EntranceUntil > Elapsed || enemy.BodyLift > 0.4f) continue;
                float reach = source.Definition.Radius + enemy.Definition.Radius;
                if ((source.Position - enemy.Position).sqrMagnitude > reach * reach) continue;
                ApplyDamage(enemy, source.Definition.CollisionDamage, source.Faction, "Collision", (enemy.Position - source.Position).normalized, cause: NewCause(source.Id));
                struck = true;
            }
            if (struck) source.NextCollision = Elapsed + source.Definition.CollisionFrequency;
        }
        private void StrikeChampionCollision(UnitState enemy)
        {
            if (Elapsed < enemy.NextCollision || Champion.Definition.CollisionDamage <= 0) return;
            enemy.NextCollision = Elapsed + Mathf.Max(0.15f, Champion.Definition.CollisionFrequency);
            ApplyDamage(enemy, Champion.Definition.CollisionDamage, Faction.Champion, "Champion collision", (enemy.Position - Champion.Position).normalized, cause: NewCause(Champion.Id));
        }
    }
}
