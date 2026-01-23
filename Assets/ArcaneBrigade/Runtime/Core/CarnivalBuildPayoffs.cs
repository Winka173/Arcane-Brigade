using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class CombatSimulation
    {
        private void ApplyCarnivalProjectilePayoff(ProjectileState projectile, UnitState target, UnitState source)
        {
            int tier = Mathf.Clamp(source.Definition.CombatTier, 1, 3);
            if (projectile.IsFragment || projectile.Cause.Depth > 0) return;
            if (projectile.Weapon.Mechanic == CarnivalWeapon.YoYo && target.Alive && !target.Definition.IsBoss)
            {
                target.Position = ClampPosition(Vector2.MoveTowards(target.Position, source.Position, (0.12f + tier * 0.07f) * (1 - Mathf.Clamp01(target.Definition.KnockbackResistance))), target.Definition.Radius);
                target.ChillUntil = Mathf.Max(target.ChillUntil, Elapsed + 0.18f); spatial.Move(target);
                if (tier >= 2)
                {
                    int caught = 0;
                    foreach (UnitState enemy in units)
                    {
                        if (!CanHit(source.Faction, enemy) || enemy == target || enemy.Definition.IsBoss || SegmentCircle(source.Position, target.Position, enemy.Position, enemy.Definition.Radius + 0.25f) < 0) continue;
                        enemy.ChillUntil = Mathf.Max(enemy.ChillUntil, Elapsed + 0.3f);
                        if (++caught >= tier) break;
                    }
                }
            }
            if (projectile.Weapon.Mechanic != CarnivalWeapon.Sewing || tier < 2 || projectile.Victims.Count != 1) return;
            UnitState previous = target; var visited = new HashSet<int>(projectile.Victims);
            for (int link = 0; link < tier - 1; link++)
            {
                UnitState next = null; float best = 1.8f * 1.8f;
                foreach (UnitState enemy in units)
                {
                    float distance = (enemy.Position - previous.Position).sqrMagnitude;
                    if (!CanHit(source.Faction, enemy) || visited.Contains(enemy.Id) || distance >= best) continue;
                    next = enemy; best = distance;
                }
                if (next == null) break;
                visited.Add(next.Id);
                CombatCause stitch = projectile.Cause.Child("sewing-stitch");
                fields.Add(new ExpandedField { Kind = ExpandedFieldKind.SewingLink, Source = source, Anchor = previous, Target = next, Start = previous.Position, Position = next.Position, At = Elapsed, Until = Elapsed + 0.38f, Resolved = true, Cause = stitch });
                ApplyDamage(next, projectile.Damage * 0.18f, source.Faction, "Sewing stitch", cause: stitch);
                if (!next.Definition.IsBoss) next.ChillUntil = Mathf.Max(next.ChillUntil, Elapsed + 0.25f);
                previous = next;
            }
        }
    }
}
