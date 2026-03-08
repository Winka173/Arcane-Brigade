using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public enum ExpandedFieldKind { Tea, Iron, Fuse, Strike, Link, Photo, Echo, OfficeHazard, ToyHazard, Curtain, Ink, Projector, MagnetCone, YoYoTether, SewingLink, Tentacle, Shockwave, FallingProp, ShadowPool }
    public sealed class ExpandedField
    {
        public ExpandedFieldKind Kind;
        public Vector2 Position;
        public Vector2 Start;
        public Vector2 Direction;
        public float Radius;
        public float At;
        public float Created;
        public float Until;
        public float NextTick;
        public float Damage;
        public UnitState Source;
        public UnitState Target;
        public CombatCause Cause;
        public bool Resolved;
        public ProjectileState Projectile;
        public WeaponDefinition ImpactWeapon;
        public bool Critical;
        public UnitState Anchor;
        public int SourceOrgan = -1;
        public readonly HashSet<int> CollateralVictims = new HashSet<int>();
    }

    public sealed partial class CombatSimulation
    {
        private readonly List<ExpandedField> fields = new List<ExpandedField>();
        public IReadOnlyList<ExpandedField> ExpandedFields => fields;
        public int ExpandedPayoffs { get; private set; }
        private float treasureUntil;
        private float nextRepair;
        private bool bossMotifConsumed;
        private void ResetExpansion() { ClearExpansion(); ExpandedPayoffs = 0; bossMotifConsumed = false; }
        private void ClearExpansion() { ClearArsenal(); fields.Clear(); treasureUntil = nextRepair = 0; ClearSpectacle(); deathReactions.Clear(); ClearCooperation(); ClearEnemyVariants(); ClearBuildMorphs(); ClearOctoberBosses(); }

        private void Field(ExpandedFieldKind kind, UnitState source, Vector2 position, Vector2 direction, float radius, float damage, float delay, float duration, CombatCause cause, UnitState target = null)
        {
            int depth = kind == ExpandedFieldKind.Fuse || kind == ExpandedFieldKind.Strike ? cause.Depth : cause.Depth + 1;
            fields.Add(new ExpandedField { Kind = kind, Source = source, Position = position, Start = source == null ? position : source.Position, Direction = direction, Radius = radius, Damage = damage, Created = Elapsed, At = Elapsed + delay, Until = Elapsed + delay + duration, NextTick = Elapsed + delay, Cause = new CombatCause(cause.RootId, cause.SourceUnitId, "expanded-" + kind.ToString().ToLowerInvariant(), depth), Target = target });
        }

        private bool ResolveExpandedAttack(UnitState source, float damage, bool critical, CombatCause cause)
        {
            ResolveCooperationAttack(source, damage, cause);
            WeaponDefinition weapon = source.Definition.Weapon;
            if (weapon.Arsenal != ArsenalWeapon.None && source.Faction != Faction.Enemy) return ResolveArsenalAttack(source, damage, critical, cause);
            if (weapon.Mechanic != CarnivalWeapon.None && source.Faction != Faction.Enemy) return ResolveCarnivalAttack(source, damage, critical, cause);
            if (weapon.Family == ExpandedWeapon.None || source.Faction == Faction.Enemy) return false;
            source.ExpandedBeat++;
            if (weapon.Family == ExpandedWeapon.Tea || weapon.Family == ExpandedWeapon.TetherBow || weapon.Family == ExpandedWeapon.Fish || weapon.Family == ExpandedWeapon.MusicBox)
            {
                EmitProjectile(source, damage, critical, cause); return true;
            }
            if (weapon.Family == ExpandedWeapon.Alarm)
            {
                UnitState target = UnitWithId(source.AttackTargetId);
                if (target != null && !target.Alive) target = null;
                if (target != null) Field(ExpandedFieldKind.Fuse, source, ClampPosition(target.Position, 0.3f), source.AttackDirection, source.Definition.CombatTier >= 2 ? 1.8f : 1.5f, damage, 0.9f, 0.25f, cause);
                return true;
            }
            if (weapon.Family == ExpandedWeapon.Camera)
            {
                int count = 0;
                foreach (UnitState target in units)
                {
                    Vector2 delta = target.Position - source.Position;
                    if (!CanHit(source.Faction, target) || delta.sqrMagnitude > weapon.Range * weapon.Range || Vector2.Dot(delta.normalized, source.AttackDirection) < 0.55f || count >= (source.Definition.CombatTier >= 2 ? 4 : 3)) continue;
                    target.CameraWeakUntil = target.ExposedUntil = Elapsed + 2f; target.WeaknessSourceId = source.Id;
                    ApplyDamage(target, damage, source.Faction, "Sleep camera", source.AttackDirection, weapon, critical, cause: cause);
                    Field(ExpandedFieldKind.Photo, source, target.Position, source.AttackDirection, 0.42f, 0, 0, 0.22f, cause); count++;
                }
                RecordActivation(new CombatCause(cause.RootId, source.Id, "camera-weakspot", 1)); return true;
            }
            if (weapon.Family == ExpandedWeapon.Scissors)
            {
                Field(ExpandedFieldKind.Strike, source, source.Position, Quaternion.Euler(0, 0, -18) * source.AttackDirection, weapon.Range, damage * 0.65f, 0, 0.05f, cause);
                Field(ExpandedFieldKind.Strike, source, source.Position, Quaternion.Euler(0, 0, 18) * source.AttackDirection, weapon.Range, damage * 0.65f, 0.12f, 0.1f, cause); return true;
            }
            foreach (UnitState target in units)
            {
                Vector2 delta = target.Position - source.Position;
                if (!CanHit(source.Faction, target) || delta.magnitude > MeleeTactics.Reach(source,target) || Vector2.Dot(delta.normalized, source.AttackDirection) < weapon.ConeDot) continue;
                bool burning = target.BurnUntil > Elapsed;
                ApplyWeaponDamage(target, damage, source.Faction, weapon.Element, weapon.Id, source.AttackDirection, weapon, critical, cause);
                if (weapon.Family == ExpandedWeapon.Hammer && burning)
                    Field(ExpandedFieldKind.Echo, source, target.Position, source.AttackDirection, 1.1f, damage * 0.1f, 0.1f, 0.12f, cause);
            }
            if (weapon.Family == ExpandedWeapon.Iron)
                Field(ExpandedFieldKind.Iron, source, source.Position + source.AttackDirection, source.AttackDirection, source.Definition.CombatTier >= 2 ? 0.55f : 0.4f, damage * 0.3f, 0, 1.4f, cause);
            else if (weapon.Family == ExpandedWeapon.Hammer)
                Field(ExpandedFieldKind.Echo, source, source.Position + source.AttackDirection * 1.5f, source.AttackDirection, 1.2f, damage * 0.2f, 0.1f, 0.16f, cause); // the slam lands ahead, not round the wielder
            return true;
        }

        private int MoveExpandedProjectile(ProjectileState projectile, float delta)
        {
            if (projectile.Weapon == null) return 0;
            ExpandedWeapon family = projectile.Weapon.Family;
            projectile.Age += delta;
            if ((family == ExpandedWeapon.MusicBox || projectile.Weapon.HomingOutbound && !projectile.Returning) && projectile.Faction != Faction.Enemy)
            {
                UnitState nearest = null; float best = 16;
                foreach (UnitState enemy in units)
                {
                    float distance = (enemy.Position - projectile.Position).sqrMagnitude;
                    if (!enemy.Alive || enemy.Faction != Faction.Enemy || projectile.Victims.Contains(enemy.Id) || distance >= best) continue;
                    nearest = enemy; best = distance;
                }
                if (nearest != null) projectile.Velocity = Vector2.Lerp(projectile.Velocity, (nearest.Position - projectile.Position).normalized * projectile.Weapon.ProjectileSpeed, Mathf.Clamp01(delta * 4));
            }
            if ((family != ExpandedWeapon.Fish && !projectile.Weapon.SecretRecall) || projectile.Faction == Faction.Enemy) return 0;
            if (projectile.Weapon.Mechanic == CarnivalWeapon.SatelliteDisc)
            {
                if (projectile.Age < 0.55f) return 0;
                if (projectile.Age < 1.55f)
                {
                    UnitState target = UnitWithId(projectile.OrbitTargetId);
                    if (target != null && !target.Alive) target = null;
                    if (target != null) projectile.OrbitCenter = target.Position;
                    Vector2 radius = projectile.Position - projectile.OrbitCenter;
                    Vector2 outward = radius.sqrMagnitude < 0.001f ? Vector2.right : radius.normalized;
                    Vector2 tangent = new Vector2(-outward.y, outward.x);
                    projectile.Velocity = (tangent + outward * Mathf.Clamp((0.85f - radius.magnitude) * 2, -1.5f, 1.5f)).normalized * projectile.Weapon.ProjectileSpeed;
                    return 0;
                }
            }
            if (projectile.Age > 0.7f && !projectile.Returning)
            {
                projectile.Returning = true;
                ProjectileAudioEvent(CombatEventKind.ProjectileReturn, projectile);
            }
            if (!projectile.Returning) return 0;
            UnitState owner = UnitWithId(projectile.SourceUnitId);
            if (owner != null && !owner.Alive) owner = null;
            if (owner == null) return -1;
            Vector2 offset = owner.Position - projectile.Position;
            if (offset.sqrMagnitude < Mathf.Pow(Mathf.Max(0.2f, projectile.Weapon.ProjectileSpeed * delta), 2) || projectile.Remaining <= 0) return -1;
            projectile.Velocity = offset.normalized * projectile.Weapon.ProjectileSpeed;
            return 0;
        }

        private void OnExpandedProjectileHit(ProjectileState projectile, UnitState target)
        {
            if (projectile.Weapon == null || projectile.Faction == Faction.Enemy) return;
            UnitState source = UnitWithId(projectile.SourceUnitId);
            if (source == null) return;
            ApplyCarnivalProjectilePayoff(projectile, target, source);
            if (projectile.Weapon.Mechanic == CarnivalWeapon.Magnet && !projectile.IsFragment && projectile.Cause.Depth == 0) MagnetBoltHit(projectile, target, source);
            if (projectile.Weapon.Mechanic == CarnivalWeapon.BloomOrb && !projectile.IsFragment)
            {
                Field(ExpandedFieldKind.Echo, source, target.Position, Vector2.zero, 1.5f, projectile.Damage * 0.45f, 0.28f, 0.35f, projectile.Cause.Child("moonseed-bloom"));
                fields[fields.Count - 1].Critical = projectile.Critical;
                fields[fields.Count - 1].ImpactWeapon = projectile.Weapon;
            }
            ExpandedWeapon family = projectile.Weapon.Family;
            if (projectile.Weapon.Mechanic == CarnivalWeapon.Sewing && target.Alive)
            {
                bool freshlyPinned = target.ChillUntil <= Elapsed;
                target.ChillUntil = Mathf.Max(target.ChillUntil, Elapsed + (target.Definition.IsBoss ? 0.15f : 0.35f));
                if (freshlyPinned) events.Add(new CombatEvent(CombatEventKind.StatusApplied, target.Position, target.Faction, element: DamageElement.Frost, cause: projectile.Cause));
            }
            if (family == ExpandedWeapon.Tea && !projectile.IsFragment)
                Field(ExpandedFieldKind.Tea, source, target.Position, Vector2.zero, 0.7f * (catalog.TeaCoasterStacks > 0 ? 1.4f : 1), projectile.Damage * 0.25f * (catalog.TeaCoasterStacks > 0 ? 0.8f : 1), 0, 1.8f, projectile.Cause);
            if (family == ExpandedWeapon.TetherBow && projectile.Victims.Count == 1)
            {
                UnitState previous = target; var visited = new HashSet<int> { target.Id };
                for (int link = 0; link < (source.Definition.CombatTier >= 2 ? 3 : 2); link++)
                {
                    UnitState next = null; float best = 4;
                    foreach (UnitState enemy in units)
                    {
                        float distance = (enemy.Position - previous.Position).sqrMagnitude;
                        if (!enemy.Alive || enemy.Faction != Faction.Enemy || visited.Contains(enemy.Id) || distance >= best) continue;
                        next = enemy; best = distance;
                    }
                    if (next == null) break;
                    var linkField = new ExpandedField { Kind = ExpandedFieldKind.Link, Start = previous.Position, Position = next.Position, At = Elapsed, Until = Elapsed + 0.24f, Resolved = true };
                    fields.Add(linkField); visited.Add(next.Id);
                    ApplyDamage(next, projectile.Damage * 0.25f, source.Faction, "Zipper tether", cause: projectile.Cause.Child("zipper-link")); previous = next;
                }
            }
            if (family == ExpandedWeapon.MusicBox && projectile.FinalNote)
            {
                StampState ownedMark = null;
                foreach (StampState mark in stamps.Values) if (mark.Target == target && mark.Expires > Elapsed) { ownedMark = mark; break; }
                if (ownedMark != null) ownedMark.Expires = Mathf.Max(ownedMark.Expires, Elapsed + 0.6f);
                if (catalog.ChorusStacks > 0)
                    Field(ExpandedFieldKind.Echo, source, target.Position, Vector2.zero, 1.3f, projectile.Damage * Mathf.Min(0.24f, 0.12f * catalog.ChorusStacks), 0.3f, 0.18f, projectile.Cause);
            }
        }

        /// <summary>The magnet bolt sticks in the foe it strikes and yanks it toward the wielder; up to 2 + tier of its
        /// neighbours (within 1.8) are dragged in after it and struck for 60%, each with a spark line to the bolt. Bosses resist.</summary>
        private void MagnetBoltHit(ProjectileState bolt, UnitState target, UnitState source)
        {
            WeaponDefinition weapon = bolt.Weapon;
            Vector2 heading = bolt.Velocity.sqrMagnitude > 0.0001f ? bolt.Velocity.normalized : source.AttackDirection;
            Vector2 anchor = target.Position;
            if (target.Alive)
            {
                float resistance = target.Definition.IsBoss ? 1 : Mathf.Clamp01(target.Definition.KnockbackResistance);
                Vector2 before = target.Position;
                target.Position = ClampPosition(Vector2.MoveTowards(target.Position, source.Position + heading * 1.4f, 0.65f * (1 - resistance)), target.Definition.Radius);
                spatial.Move(target); anchor = target.Position;
                RegisterCooperationPull(source, target, before, bolt.Cause);
            }
            int pulled = 0, most = 2 + Mathf.Clamp(source.Definition.CombatTier, 1, 3);
            foreach (UnitState enemy in units)
            {
                if (enemy == target || !CanHit(source.Faction, enemy) || (enemy.Position - anchor).sqrMagnitude > 1.8f * 1.8f) continue;
                if (pulled++ >= most) break;
                float resistance = enemy.Definition.IsBoss ? 1 : Mathf.Clamp01(enemy.Definition.KnockbackResistance);
                Vector2 before = enemy.Position;
                enemy.Position = ClampPosition(Vector2.MoveTowards(enemy.Position, anchor, 0.65f * (1 - resistance)), enemy.Definition.Radius);
                spatial.Move(enemy);
                fields.Add(new ExpandedField { Kind = ExpandedFieldKind.Link, Start = before, Position = anchor, At = Elapsed, Until = Elapsed + 0.26f, Resolved = true });
                ApplyWeaponDamage(enemy, bolt.Damage * 0.6f, source.Faction, weapon.Element, weapon.Id, (anchor - before).normalized, weapon, bolt.Critical, bolt.Cause);
                RegisterCooperationPull(source, enemy, before, bolt.Cause);
            }
        }

        private void ProcessExpansion(float delta)
        {
            ProcessCooperation();
            ProcessDeathReactions();
            ProcessExpandedRelics(); ProcessBossMotif(); ProcessArsenal(delta);
            for (int index = fields.Count - 1; index >= 0; index--)
            {
                ExpandedField field = fields[index];
                if (Elapsed > field.Until) { fields.RemoveAt(index); continue; }
                bool ink = field.Kind == ExpandedFieldKind.Ink;
                if (Elapsed < field.At || field.Resolved || Elapsed < field.NextTick || (field.Damage <= 0 && !ink)) continue;
                bool heat = field.Kind == ExpandedFieldKind.Tea || field.Kind == ExpandedFieldKind.Iron;
                if (field.Kind == ExpandedFieldKind.Strike && field.Source != null && field.Source.Definition.Weapon.Mechanic == CarnivalWeapon.RibbonWheel) field.Position = field.Source.Position;
                field.NextTick = Elapsed + (field.Kind == ExpandedFieldKind.Shockwave ? 0.05f : 0.4f); field.Resolved = !heat && !ink && field.Kind != ExpandedFieldKind.Projector && field.Kind != ExpandedFieldKind.Shockwave && field.Kind != ExpandedFieldKind.ShadowPool;
                ExpandedPayoffs++;
                if (field.Source == null || field.Source.Faction != Faction.Enemy) RecordActivation(field.Cause);
                // fuse and echo blasts are heard and seen as explosions (presentation only; Value = blast radius)
                if ((field.Kind == ExpandedFieldKind.Fuse || field.Kind == ExpandedFieldKind.Echo) && field.Radius >= 1f && (field.Source == null || field.Source.Faction != Faction.Enemy))
                    events.Add(new CombatEvent(CombatEventKind.Detonation, field.Position, field.Source == null ? Faction.Companion : field.Source.Faction, field.Radius, field.Direction, field.ImpactWeapon, cause: field.Cause));
                if (CarnivalHazards.IsHostile(field.Kind))
                {
                    ProcessHostileCollateral(field);
                    if (Champion.Alive && CarnivalHazards.Contains(field, Champion.Position, Champion.Definition.Radius, Elapsed))
                    {
                        ApplyDamage(Champion, Mathf.Min(field.Damage, Champion.Definition.Health * 0.08f), Faction.Enemy, field.Kind.ToString(), cause: field.Cause);
                        field.Resolved = true;
                    }
                    continue;
                }
                foreach (UnitState enemy in units)
                {
                    if (!enemy.Alive || enemy.Faction != Faction.Enemy) continue;
                    Vector2 offset = enemy.Position - field.Position;
                    bool inside = field.Kind == ExpandedFieldKind.Iron ? SegmentCircle(field.Position - field.Direction, field.Position + field.Direction, enemy.Position, field.Radius + enemy.Definition.Radius) >= 0 : offset.sqrMagnitude <= (field.Radius + enemy.Definition.Radius) * (field.Radius + enemy.Definition.Radius);
                    if (field.Kind == ExpandedFieldKind.Strike) inside &= Vector2.Dot(offset.normalized, field.Direction) >= 0.8f;
                    if (!inside) continue;
                    if (ink) { enemy.ChillUntil = Mathf.Max(enemy.ChillUntil, Elapsed + 0.6f); continue; }
                    if (field.Kind == ExpandedFieldKind.Projector && field.Source != null && field.Source.Definition.CombatTier >= 2 && !enemy.Definition.IsBoss)
                    {
                        enemy.Position = ClampPosition(Vector2.MoveTowards(enemy.Position, field.Position, 0.12f * (1 - Mathf.Clamp01(enemy.Definition.KnockbackResistance))), enemy.Definition.Radius);
                        enemy.ChillUntil = Mathf.Max(enemy.ChillUntil, Elapsed + 0.25f); spatial.Move(enemy);
                    }
                    ApplyDamage(enemy, field.Damage, field.Source == null ? Faction.Companion : field.Source.Faction, field.Cause.ItemId, field.Direction, field.ImpactWeapon != null ? field.ImpactWeapon : field.Cause.Depth == 0 && field.Source != null ? field.Source.Definition.Weapon : null, critical: field.Critical, cause: field.Cause);
                    if (heat && enemy.Alive)
                    {
                        enemy.BurnUntil = Mathf.Max(enemy.BurnUntil, Elapsed + 0.6f); enemy.BurnCause = field.Cause.Child("heat-status");
                        if (catalog.ThermalInk && enemy.VisibleMarks > 0 && Elapsed >= enemy.NextThermal)
                        {
                            enemy.NextThermal = Elapsed + 0.9f; ApplyDamage(enemy, field.Damage * 0.4f, Faction.Companion, "Thermal ink", cause: field.Cause.Child("thermal-ink"));
                        }
                    }
                }
            }
        }

        private void ProcessExpandedRelics()
        {
            int original = events.Count;
            for (int index = 0; index < original; index++)
            {
                CombatEvent trigger = events[index];
                if (catalog.TreasureStacks > 0 && trigger.Kind == CombatEventKind.ChestOpened && trigger.Cause.Depth == 0)
                {
                    treasureUntil = Elapsed + 4; Heal(3 * catalog.TreasureStacks, trigger.Cause.Child("treasure-ribbon")); RecordActivation(trigger.Cause.Child("treasure-ribbon"));
                }
                if (catalog.RepairStacks > 0 && trigger.Kind == CombatEventKind.Kill && trigger.Faction == Faction.Enemy && trigger.Cause.ItemId != null && trigger.Cause.ItemId.StartsWith("arena-") && Elapsed >= nextRepair)
                {
                    nextRepair = Elapsed + 0.6f; Heal(catalog.RepairStacks, trigger.Cause.Child("repair-coupon")); RecordActivation(trigger.Cause.Child("repair-coupon"));
                }
            }
        }
    }
}
