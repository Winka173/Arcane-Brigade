using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class CombatSimulation
    {
        private readonly System.Collections.Generic.Dictionary<int, int> tacticalClaims = new System.Collections.Generic.Dictionary<int, int>();
        private readonly System.Collections.Generic.Dictionary<int, float> tacticalDamage = new System.Collections.Generic.Dictionary<int, float>();
        private float tacticalClaimTime = -1;
        private int tacticalGeneration = -1;
        /// <summary>How much nearer a strong foe counts for a shooter, in world units: elites and bosses most, then the tough.</summary>
        private static float RangedThreat(UnitState enemy) => (enemy.Definition.IsBoss ? 1.2f : enemy.Elite ? .9f : 0) + Mathf.Min(.5f, enemy.Health * .003f);

        private UnitState SelectCompanionTarget(UnitState source, float radius, bool attackOnly = false)
        {
            if (V3AttackFaction(source)==Faction.Enemy) return V3HostileFriendTarget(source,radius);
            radius *= 1 + (Bonds?.RangeBonusFor(source) ?? 0);
            bool melee = source.Definition.Weapon.Kind == WeaponKind.Melee;
            if (!attackOnly && melee && source.PendingAttack)
            {
                UnitState locked = units.Find(unit => unit.Id == source.AttackTargetId);
                if (locked != null && CanHit(source.Faction, locked) && (locked.Position - Champion.Position).magnitude <= CompanionMotion.Leash(source, catalog) + source.Definition.Weapon.Range * 0.65f) return locked;
            }
            if (tacticalClaimTime != Elapsed || tacticalGeneration != Generation)
            {
                tacticalClaimTime = Elapsed; tacticalGeneration = Generation; tacticalClaims.Clear(); tacticalDamage.Clear();
                foreach (UnitState ally in StableUnits)
                {
                    if (!ally.Alive || ally.Faction != Faction.Companion) continue;
                    if (ally.TacticalTargetId != 0) tacticalClaims[ally.TacticalTargetId] = tacticalClaims.TryGetValue(ally.TacticalTargetId, out int count) ? count + 1 : 1;
                    if (ally.PendingAttack) tacticalDamage[ally.AttackTargetId] = (tacticalDamage.TryGetValue(ally.AttackTargetId, out float committed) ? committed : 0) + ally.Definition.Weapon.Damage;
                }
            }
            Vector2 center = attackOnly || source.Definition.Weapon.Kind == WeaponKind.Projectile ? source.Position : Champion.Position;
            if(attackOnly && melee)radius=Mathf.Max(radius,source.Definition.Radius+HitPadding+.08f);
            spatial.Query(center - Vector2.one * radius, center + Vector2.one * radius, targetCandidates);
            UnitState best = null;
            float bestScore = float.PositiveInfinity;
            foreach (UnitState enemy in targetCandidates)
            {
                if (!CanHit(source.Faction, enemy) || (enemy.Position - center).sqrMagnitude > radius * radius) continue;
                if (attackOnly && melee && !MeleeReaches(source,enemy)) continue;
                float claims = tacticalClaims.TryGetValue(enemy.Id, out int assignments) ? assignments : 0;
                if (source.TacticalTargetId == enemy.Id) claims = Mathf.Max(0, claims - 1);
                float committedDamage = tacticalDamage.TryGetValue(enemy.Id, out float expected) ? expected : 0;
                if (source.PendingAttack && source.AttackTargetId == enemy.Id) committedDamage -= source.Definition.Weapon.Damage;
                WeaponDefinition held = source.Definition.Weapon;
                bool ranged = held.Kind == WeaponKind.Projectile;
                // requirement 24/10/2026: a shooter takes the nearest foe, or a near and strong one - not one far off because a friend
                // already aims at the closer one (claims weigh little for shooters); spreads still like a crowd
                float score = (enemy.Position - source.Position).magnitude + claims * (ranged ? .15f : .55f) + IdentityTargetScore(enemy);
                if (ranged) score -= RangedThreat(enemy) + (held.ProjectileCount >= 3 || held.ProjectileSpread >= .35f ? .25f * Mathf.Min(3, CrowdAround(enemy)) : 0);
                if (!enemy.Definition.IsBoss && committedDamage >= enemy.Health) score += 3;
                if (enemy.Id == source.TacticalTargetId) score -= Elapsed < source.TacticalRetargetAt ? 1.5f : 0.6f;
                if (score > bestScore || score == bestScore && best != null && enemy.Id >= best.Id) continue;
                bestScore = score; best = enemy;
            }
            if (!attackOnly)
            {
                int identifier = best == null ? 0 : best.Id;
                if (identifier != source.TacticalTargetId)
                {
                    if (tacticalClaims.TryGetValue(source.TacticalTargetId, out int oldCount)) tacticalClaims[source.TacticalTargetId] = Mathf.Max(0, oldCount - 1);
                    if (identifier != 0) tacticalClaims[identifier] = tacticalClaims.TryGetValue(identifier, out int newCount) ? newCount + 1 : 1;
                    source.TacticalRetargetAt = Elapsed + 0.55f;
                }
                source.TacticalTargetId = identifier;
                if (melee && source.PendingAttack && best != null) source.AttackTargetId = best.Id;
            }
            return best;
        }
    }
}
