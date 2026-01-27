using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class CombatSimulation
    {
        private bool ResolveCarnivalAttack(UnitState source, float damage, bool critical, CombatCause cause)
        {
            WeaponDefinition weapon = source.Definition.Weapon;
            source.ExpandedBeat++;
            if (weapon.Mechanic == CarnivalWeapon.BloomOrb || weapon.Mechanic == CarnivalWeapon.SatelliteDisc)
            {
                EmitProjectile(source, damage, critical, cause); return true;
            }
            if (weapon.Mechanic == CarnivalWeapon.EchoBlade)
            {
                foreach (UnitState target in units)
                {
                    Vector2 offset = target.Position - source.Position;
                    if (CanHit(source.Faction, target) && offset.magnitude <= weapon.Range + target.Definition.Radius && Vector2.Dot(offset.normalized, source.AttackDirection) >= weapon.ConeDot)
                        ApplyWeaponDamage(target, damage * 0.7f, source.Faction, weapon.Element, weapon.Id, source.AttackDirection, weapon, critical, cause);
                }
                Field(ExpandedFieldKind.Strike, source, source.Position, source.AttackDirection, weapon.Range, damage * 0.45f, 0.24f, 0.2f, cause);
                fields[fields.Count - 1].ImpactWeapon = weapon; fields[fields.Count - 1].Critical = critical;
                return true;
            }
            if (weapon.Mechanic == CarnivalWeapon.YoYo)
            {
                int previous = projectiles.Count;
                EmitProjectile(source, damage, critical, cause);
                for (int index = previous; index < projectiles.Count; index++)
                    fields.Add(new ExpandedField { Kind = ExpandedFieldKind.YoYoTether, Source = source, Projectile = projectiles[index], Start = source.Position, Position = projectiles[index].Position, At = Elapsed, Until = Elapsed + weapon.ProjectileLifetime, Resolved = true });
            }
            else if (weapon.Mechanic == CarnivalWeapon.Sewing)
            {
                EmitProjectile(source, damage, critical, cause);
                UnitState target = units.Find(candidate => candidate.Id == source.AttackTargetId && candidate.Alive);
                if (target != null) fields.Add(new ExpandedField { Kind = ExpandedFieldKind.Link, Source = source, Target = target, Start = source.Position, Position = target.Position, At = Elapsed, Until = Elapsed + 0.3f, Resolved = true });
            }
            else if (weapon.Mechanic == CarnivalWeapon.Magnet)
            {
                int pulled = 0;
                foreach (UnitState enemy in units)
                {
                    Vector2 offset = enemy.Position - source.Position;
                    if (!CanHit(source.Faction, enemy) || offset.magnitude > weapon.Range || Vector2.Dot(offset.normalized, source.AttackDirection) < 0.55f) continue;
                    if (pulled++ >= 3 + Mathf.Clamp(source.Definition.CombatTier, 1, 3)) break;
                    float resistance = enemy.Definition.IsBoss ? 1 : Mathf.Clamp01(enemy.Definition.KnockbackResistance);
                    Vector2 before = enemy.Position;
                    enemy.Position = ClampPosition(Vector2.MoveTowards(enemy.Position, source.Position + source.AttackDirection * 1.4f, 0.65f * (1 - resistance)), enemy.Definition.Radius);
                    spatial.Move(enemy);
                    ApplyWeaponDamage(enemy, damage, source.Faction, weapon.Element, weapon.Id, source.AttackDirection, weapon, critical, cause);
                    RegisterCooperationPull(source, enemy, before, cause);
                }
                Field(ExpandedFieldKind.MagnetCone, source, source.Position, source.AttackDirection, weapon.Range, 0, 0, 0.35f, cause);
            }
            else if (weapon.Mechanic == CarnivalWeapon.SpringHammer)
            {
                Vector2 point = source.Position + source.AttackDirection * Mathf.Min(weapon.Range * 0.7f, 2.3f);
                Field(ExpandedFieldKind.Echo, source, point, source.AttackDirection, weapon.Range * 0.6f, damage, 0, 0.18f, cause);
                fields[fields.Count - 1].ImpactWeapon = weapon;
                fields[fields.Count - 1].Cause = cause; fields[fields.Count - 1].Critical = critical;
            }
            else if (weapon.Mechanic == CarnivalWeapon.Projector)
            {
                UnitState target = units.Find(candidate => candidate.Id == source.AttackTargetId && candidate.Alive);
                Vector2 point = target == null ? source.Position + source.AttackDirection * weapon.Range * 0.6f : target.Position;
                Field(ExpandedFieldKind.Projector, source, point, source.AttackDirection, 1.15f * CompanionTierScaling.Body(source.Definition.CombatTier), damage * 0.4f, 0, 1.15f, cause);
                fields[fields.Count - 1].Critical = critical;
            }
            else
            {
                Field(ExpandedFieldKind.Strike, source, source.Position, Quaternion.Euler(0, 0, -35) * source.AttackDirection, weapon.Range, damage * 0.55f, 0, 0.08f, cause);
                fields[fields.Count - 1].Critical = critical;
                Field(ExpandedFieldKind.Strike, source, source.Position, Quaternion.Euler(0, 0, 35) * source.AttackDirection, weapon.Range, damage * 0.55f, 0.18f, 0.1f, cause);
                fields[fields.Count - 1].Critical = critical;
            }
            return true;
        }
    }
}
