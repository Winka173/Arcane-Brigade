using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class CombatSimulation
    {
        private bool AdvanceCombatProjectile(ProjectileState projectile, float deltaTime)
        {
            if (projectile.Faction == Faction.Enemy && catalog.RunWave == 1) return false;
            if (projectile.Remaining <= 0f) return false;
            if (projectile.Cause.RootId == 0) projectile.Cause = NewCause(projectile.SourceUnitId);
            UnitState source = units.Find(unit => unit.Id == projectile.SourceUnitId);
            Vector2 start = projectile.LaunchSegment ? projectile.PreviousPosition : projectile.Position;
            Vector2 destination = projectile.Position + projectile.Velocity * Mathf.Min(deltaTime, projectile.Remaining);
            projectile.PreviousPosition = start;
            projectile.LaunchSegment = false;
            projectile.Remaining -= deltaTime;
            Vector2 half = catalog.ArenaSize * 0.5f;
            while (true)
            {
                float wallFraction = ProjectileWallFraction(start, destination, half, out Vector2 normal);
                UnitState hit = null;
                float hitFraction = wallFraction;
                Vector2 padding = Vector2.one * (maximumRadius + projectile.Radius);
                spatial.Query(Vector2.Min(start, destination) - padding, Vector2.Max(start, destination) + padding, projectileCandidates);
                foreach (UnitState target in projectileCandidates)
                {
                    bool interceptor = projectile.Faction == Faction.Enemy && target.Alive && target.Faction == Faction.Companion && catalog.CompanionInterceptor;
                    if ((!CanHit(projectile.Faction, target) && !interceptor) || projectile.Victims.Contains(target.Id)) continue;
                    float fraction = SegmentCircle(start, destination, target.Position, target.Definition.Radius + projectile.Radius);
                    if (fraction >= 0f && fraction < hitFraction) { hitFraction = fraction; hit = target; }
                }
                Vector2 impact = hit == null && wallFraction > 1f ? destination : Vector2.Lerp(start, destination, hitFraction);
                projectile.Position = impact;
                NotifyArenaToyHit(source, start, impact, projectile.Radius, projectile.Velocity.normalized, projectile.Cause);
                if (hit == null && wallFraction > 1f) return projectile.Remaining > 0f;
                float travelLeft = (destination - impact).magnitude;
                if (hit == null)
                {
                    if (projectile.RicochetsRemaining <= 0 || projectile.Returning) return false;
                    projectile.RicochetsRemaining--;
                    projectile.Velocity = new Vector2(normal.x == 0f ? projectile.Velocity.x : -projectile.Velocity.x, normal.y == 0f ? projectile.Velocity.y : -projectile.Velocity.y);
                    projectile.Cause = projectile.Cause.Child("projectile-bounce");
                }
                else
                {
                    int previousVictims = projectile.Victims.Count;
                    projectile.Victims.Add(hit.Id);
                    ApplyWeaponDamage(hit, projectile.Damage, projectile.Faction, projectile.Element, projectile.SourceName, projectile.Velocity.normalized, projectile.Weapon, projectile.Critical, projectile.Cause);
                    OnExpandedProjectileHit(projectile, hit);
                    if (Phase != RunPhase.Combat) return false;
                    if (hit.Faction == Faction.Companion) return false;
                    if (projectile.SplitOnImpact) SplitCombatProjectile(projectile);
                    if (projectile.Weapon != null && projectile.Weapon.Family == ExpandedWeapon.Fish && projectile.Faction != Faction.Enemy)
                    {
                        projectile.Returning = true;
                        return projectile.Remaining > 0f;
                    }
                    bool guaranteed = projectile.Weapon != null && projectile.Weapon.Family == ExpandedWeapon.TetherBow;
                    bool randomBounce = projectile.Weapon != null && projectile.RandomBounces > 0 && projectile.Weapon.ProjectileBounceChance > 0 && combatRandom.NextDouble() < projectile.Weapon.ProjectileBounceChance;
                    if (randomBounce && projectile.RicochetsRemaining <= 0) projectile.RicochetsRemaining = 1;
                    bool ricochet = (guaranteed || randomBounce) && TryCombatRicochet(projectile);
                    if (ricochet && randomBounce) projectile.RandomBounces--;
                    if (!ricochet && (projectile.Weapon == null || previousVictims >= projectile.Weapon.ProjectilePierce)) return false;
                }
                start = projectile.Position;
                destination = start + projectile.Velocity.normalized * travelLeft;
                if (travelLeft <= 0.000001f) return projectile.Remaining > 0f;
            }
        }

        private bool TryCombatRicochet(ProjectileState projectile)
        {
            if (projectile.RicochetsRemaining <= 0) return false;
            UnitState nearest = null;
            float best = 9f;
            foreach (UnitState target in units)
            {
                if (!CanHit(projectile.Faction, target) || projectile.Victims.Contains(target.Id)) continue;
                float distance = (target.Position - projectile.Position).sqrMagnitude;
                if (distance >= best) continue;
                best = distance;
                nearest = target;
            }
            if (nearest == null) return false;
            projectile.RicochetsRemaining--;
            projectile.Velocity = (nearest.Position - projectile.Position).normalized * projectile.Velocity.magnitude;
            projectile.Damage *= 0.7f;
            projectile.Cause = projectile.Cause.Child("projectile-ricochet");
            return true;
        }

        private void SplitCombatProjectile(ProjectileState projectile)
        {
            projectile.SplitOnImpact = false;
            if (projectile.Remaining <= 0f) return;
            int count = Mathf.Clamp(projectile.SplitCount > 0 ? projectile.SplitCount : 2, 1, 6);
            for (int branch = 0; branch < count && projectiles.Count < 4096; branch++)
            {
                float spread = projectile.SplitCount > 0 ? 45 : 30;
                float angle = count == 1 ? 25 : Mathf.Lerp(-spread, spread, branch / (float)(count - 1));
                Vector2 velocity = Quaternion.Euler(0, 0, angle) * projectile.Velocity;
                var fragment = new ProjectileState
                {
                    Id = ++nextId, Faction = projectile.Faction, Position = projectile.Position, PreviousPosition = projectile.Position,
                    Velocity = velocity, Damage = projectile.Damage * 0.35f, Remaining = projectile.Remaining,
                    Color = projectile.Color, Element = projectile.Element, SourceName = projectile.SourceName,
                    Sprite = projectile.Sprite, VisualWidth = projectile.VisualWidth * 0.7f, Critical = projectile.Critical,
                    SourceUnitId = projectile.SourceUnitId, Cause = projectile.Cause.Child("projectile-split"),
                    Weapon = projectile.Weapon, IsFragment = true, Radius = projectile.Radius * (projectile.SplitCount > 0 ? 0.7f : 1), SplitCount = 0, RandomBounces = 0
                };
                fragment.Victims.UnionWith(projectile.Victims);
                projectiles.Add(fragment);
            }
        }

        private static float ProjectileWallFraction(Vector2 start, Vector2 end, Vector2 half, out Vector2 normal)
        {
            Vector2 delta = end - start;
            float nearest = 2f;
            normal = Vector2.zero;
            if (Mathf.Abs(start.x) > half.x || Mathf.Abs(start.y) > half.y) return 0f;
            if (delta.x != 0f && Mathf.Abs(end.x) >= half.x)
            {
                float fraction = (Mathf.Sign(delta.x) * half.x - start.x) / delta.x;
                if (fraction >= 0f && fraction <= 1f) { nearest = fraction; normal = Vector2.right; }
            }
            if (delta.y != 0f && Mathf.Abs(end.y) >= half.y)
            {
                float fraction = (Mathf.Sign(delta.y) * half.y - start.y) / delta.y;
                if (fraction >= 0f && fraction <= nearest)
                {
                    normal = Mathf.Abs(fraction - nearest) < 0.000001f ? (normal + Vector2.up).normalized : Vector2.up;
                    nearest = fraction;
                }
            }
            return nearest;
        }

#if UNITY_EDITOR
        public void K8CombatIncomingDamageFixture(float damage) => ApplyDamage(Champion, damage, Faction.Enemy);
        public void K8CombatSignatureFixture(UnitState source, CombatCause cause) => AdvanceSignatureBeat(source, source.Definition.Weapon.Damage, cause, source.Definition.Weapon.SignatureBeats);

        public ProjectileState K8CombatProjectileFixture(UnitState source, Vector2 position, Vector2 velocity, float damage, int ricochets = 0, bool split = false, CombatCause cause = default)
        {
            var projectile = new ProjectileState
            {
                Id = ++nextId, Faction = source.Faction, Position = position, PreviousPosition = position,
                Velocity = velocity, Damage = damage, Remaining = 2f, SourceUnitId = source.Id,
                Weapon = source.Definition.Weapon, Element = source.Definition.Weapon.Element,
                Cause = cause.RootId == 0 ? NewCause(source.Id) : cause, RicochetsRemaining = ricochets, SplitOnImpact = split
            };
            projectiles.Add(projectile);
            return projectile;
        }
#endif
    }
}
