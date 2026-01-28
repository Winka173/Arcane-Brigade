using System;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class CombatSimulation
    {
        public UnitState AddAnimationStressUnit(UnitDefinition definition, Vector2 position)
        {
            if (!RunPaths.K7Capture) throw new InvalidOperationException("Stress fixture requires isolated capture mode");
            return Spawn(definition, Faction.Enemy, ClampPosition(position, definition.Radius));
        }
        public void AddAnimationStressShot(Vector2 position, Vector2 velocity)
        {
            if (!RunPaths.K7Capture) throw new InvalidOperationException("Stress fixture requires isolated capture mode");
            projectiles.Add(new ProjectileState { Id = ++nextId, Faction = Faction.Enemy, Position = position, PreviousPosition = position, Velocity = velocity, Damage = .1f, Remaining = 6, Color = new Color(1, .3f, .4f), Element = DamageElement.Physical });
        }
    }
}
