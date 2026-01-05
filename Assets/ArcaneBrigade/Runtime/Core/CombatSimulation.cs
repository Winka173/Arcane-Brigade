using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public enum RunPhase { Preparing, Combat, Paused, Victory, Defeat, Rest }
    public enum CombatEventKind { Windup, Fire, Hit, Kill, Pickup, Heal }

    public readonly struct CombatEvent
    {
        public readonly CombatEventKind Kind;
        public readonly Vector2 Position;
        public readonly Faction Faction;
        public readonly float Value;
        public readonly Vector2 Direction;
        public readonly WeaponDefinition Weapon;
        public readonly bool Critical;
        public readonly DamageElement Element;
        public readonly int SubjectId;
        public readonly UnitState Subject;
        public CombatEvent(CombatEventKind kind, Vector2 position, Faction faction, float value = 0f, Vector2 direction = default, WeaponDefinition weapon = null, bool critical = false, DamageElement element = DamageElement.Physical, int subjectId = 0, UnitState subject = null)
        {
            Kind = kind;
            Position = position;
            Faction = faction;
            Value = value;
            Direction = direction;
            Weapon = weapon;
            Critical = critical;
            Element = weapon == null ? element : weapon.Element;
            SubjectId = subjectId;
            Subject = subject;
        }
    }

    public sealed class UnitState
    {
        public int Id;
        public UnitDefinition Definition;
        public Faction Faction;
        public Vector2 Position;
        public Vector2 PreviousPosition;
        public Vector2 Facing = Vector2.right;
        public float Health;
        public bool Alive = true;
        public float AttackStarted = -100f;
        public float LastHit = -100f;
        public float NextAttack;
        public float ImpactTime;
        public float NextContact;
        public bool PendingAttack;
        public Vector2 AttackDirection;
        public Vector2 RoamOffset;
        public Vector2 Velocity;
        public float NextRoamTime;
        public System.Random RoamRandom;
        public float BurnUntil;
        public float ChillUntil;
        public float NextBurn;
        public float NextPassive;
        public float TelegraphUntil;
        public float NextBossPattern = 2f;
        public Vector2 TelegraphDirection;
        public Vector2 HitDirection;
    }

    public sealed class ProjectileState
    {
        public int Id;
        public Faction Faction;
        public Vector2 Position;
        public Vector2 PreviousPosition;
        public Vector2 Velocity;
        public float Damage;
        public float Remaining;
        public Color Color;
        public DamageElement Element;
        public string SourceName;
        public bool LaunchSegment;
        public Sprite Sprite;
        public float VisualWidth;
        public bool Critical;
    }

    public sealed class PickupState
    {
        public int Id;
        public Vector2 Position;
        public Vector2 PreviousPosition;
        public Vector2 Velocity;
        public float SpawnTime;
        public bool Attracting;
        public int Value;
    }

    public sealed class CombatSimulation
    {
        private readonly CheckpointCatalog catalog;
        private readonly List<UnitState> units = new List<UnitState>();
        private readonly List<ProjectileState> projectiles = new List<ProjectileState>();
        private readonly List<PickupState> pickups = new List<PickupState>();
        private readonly List<CombatEvent> events = new List<CombatEvent>();
        private readonly SpatialUnitIndex spatial = new SpatialUnitIndex();
        private readonly List<UnitState> targetCandidates = new List<UnitState>();
        private readonly List<UnitState> projectileCandidates = new List<UnitState>();
        private readonly List<UnitState> separationCandidates = new List<UnitState>();
        private float maximumRadius;
        private float maximumSpeed;
        private System.Random spawnRandom;
        private System.Random combatRandom;
        private int nextId;
        private int runSeed;
        private float nextSpawn;
        private RunPhase phaseBeforePause;
        private float restElapsed;
        public IReadOnlyList<UnitState> Units => units;
        public IReadOnlyList<ProjectileState> Projectiles => projectiles;
        public IReadOnlyList<PickupState> Pickups => pickups;
        public IReadOnlyList<CombatEvent> Events => events;
        public RunPhase Phase { get; private set; } = RunPhase.Preparing;
        public UnitState Champion { get; private set; }
        public float Elapsed { get; private set; }
        public float VisualElapsed => Elapsed + restElapsed;
        public int Gold { get; private set; }
        public int Kills { get; private set; }
        public int Generation { get; private set; }
        public int CompanionCount { get; private set; }
        public bool BossDefeated { get; private set; }
        public UnitState Boss { get; private set; }
        public string LastDamageSource { get; private set; } = "";
        public float LastDamageAmount { get; private set; }

        public CombatSimulation(CheckpointCatalog definition) { catalog = definition; }

        public void Start(int seed)
        {
            units.Clear();
            projectiles.Clear();
            pickups.Clear();
            events.Clear();
            spawnRandom = new System.Random(seed);
            combatRandom = new System.Random(unchecked(seed ^ 0x51B31));
            runSeed = seed;
            nextId = 0;
            nextSpawn = 0.6f;
            Elapsed = 0f;
            restElapsed = 0f;
            Gold = 0;
            Kills = 0;
            CompanionCount = 0;
            BossDefeated = false;
            Boss = null;
            LastDamageSource = "";
            LastDamageAmount = 0f;
            maximumRadius = maximumSpeed = 0f;
            Generation++;
            Champion = Spawn(catalog.Champion, Faction.Champion, Vector2.zero);
            if (!catalog.ExplicitStartingRoster && (catalog.StartingCompanions == null || catalog.StartingCompanions.Length == 0))
                AddCompanion(catalog.Companion);
            else if (catalog.StartingCompanions != null)
                for (int index = 0; index < catalog.StartingCompanions.Length; index++) AddCompanion(catalog.StartingCompanions[index]);
            if (catalog.Boss != null) Boss = Spawn(catalog.Boss, Faction.Enemy, new Vector2(0f, catalog.ArenaSize.y * 0.35f));
            spatial.Rebuild(units);
            Phase = RunPhase.Combat;
        }

        public UnitState AddCompanion(UnitDefinition definition)
        {
            if (Champion == null) throw new InvalidOperationException("Start the run before adding companions");
            if (definition == null || definition.Weapon == null) throw new ArgumentException("Companion requires a unit and weapon definition", nameof(definition));
            float angle = nextId * 2.399963f;
            float radius = Mathf.Min(catalog.CompanionRoamInnerRadius + 0.35f, catalog.CompanionLeash);
            Vector2 position = Champion.Position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            return Spawn(definition, Faction.Companion, ClampPosition(position, definition.Radius));
        }

        public void Pause(bool paused)
        {
            if ((Phase == RunPhase.Combat || Phase == RunPhase.Rest) && paused) { phaseBeforePause = Phase; Phase = RunPhase.Paused; }
            else if (Phase == RunPhase.Paused && !paused) Phase = phaseBeforePause;
        }

        public void EnterRest()
        {
            units.RemoveAll(unit => unit.Faction == Faction.Enemy);
            projectiles.Clear();
            pickups.Clear();
            events.Clear();
            Boss = null;
            restElapsed = 0f;
            foreach (UnitState unit in units) { unit.PendingAttack = false; unit.AttackStarted = -100f; unit.BurnUntil = unit.ChillUntil = 0f; }
            spatial.Rebuild(units);
            Phase = RunPhase.Rest;
        }

        public void StepRest(float deltaTime, Vector2 movement)
        {
            if (Phase != RunPhase.Rest || !float.IsFinite(deltaTime) || deltaTime <= 0f || deltaTime > 0.1f) return;
            if (!float.IsFinite(movement.x) || !float.IsFinite(movement.y)) movement = Vector2.zero;
            restElapsed += deltaTime;
            foreach (UnitState unit in units) unit.PreviousPosition = unit.Position;
            movement = Vector2.ClampMagnitude(movement, 1f);
            Champion.Position = ClampPosition(Champion.Position + movement * Champion.Definition.Speed * deltaTime, Champion.Definition.Radius);
            if (movement.sqrMagnitude > 0.001f) Champion.Facing = movement.normalized;
            foreach (UnitState unit in units)
            {
                if (unit.Faction != Faction.Companion) continue;
                unit.Position = ClampPosition(CompanionMotion.Step(unit, Champion, units, null, catalog, Elapsed + restElapsed, deltaTime), unit.Definition.Radius);
                unit.Position = Champion.Position + Vector2.ClampMagnitude(unit.Position - Champion.Position, catalog.CompanionLeash);
            }
        }

        public Vector2 ClampPosition(Vector2 position, float radius)
        {
            Vector2 half = catalog.ArenaSize * 0.5f - Vector2.one * radius;
            return new Vector2(Mathf.Clamp(position.x, -half.x, half.x), Mathf.Clamp(position.y, -half.y, half.y));
        }

        public void Step(float deltaTime, Vector2 movement)
        {
            events.Clear();
            if (Phase != RunPhase.Combat || !float.IsFinite(deltaTime) || deltaTime <= 0f || deltaTime > 0.1f) return;
            if (!float.IsFinite(movement.x) || !float.IsFinite(movement.y)) movement = Vector2.zero;
            movement = Vector2.ClampMagnitude(movement, 1f);
            Elapsed += deltaTime;
            for (int index = 0; index < units.Count; index++) units[index].PreviousPosition = units[index].Position;
            Champion.Position = ClampPosition(Champion.Position + movement * Champion.Definition.Speed * deltaTime, Champion.Definition.Radius);
            Champion.Health = Mathf.Min(Champion.Definition.Health, Champion.Health + catalog.PassiveHeal * deltaTime);
            if (movement.sqrMagnitude > 0.001f) Champion.Facing = movement.normalized;
            SpawnWaveEnemies();
            spatial.Rebuild(units);

            for (int index = 0; index < units.Count && Phase == RunPhase.Combat; index++)
            {
                UnitState unit = units[index];
                if (!unit.Alive) continue;
                if (unit.Faction == Faction.Companion) FollowChampion(unit, deltaTime);
                else if (unit.Faction == Faction.Enemy) MoveEnemy(unit, deltaTime);
                spatial.Move(unit);
                ProcessStatusAndPassive(unit);
                if (!unit.Alive) continue;
                if (unit.Definition.IsBoss) ProcessBoss(unit);
                ProcessAttack(unit);
                if (unit.Faction == Faction.Enemy) ProcessContact(unit);
            }

            if (Phase == RunPhase.Combat)
            {
                MoveProjectiles(deltaTime);
                CollectPickups(deltaTime);
                if (Phase == RunPhase.Combat && (catalog.Boss == null ? Elapsed >= catalog.WaveDuration : BossDefeated)) Phase = RunPhase.Victory;
            }
            for (int index = units.Count - 1; index >= 0; index--)
                if (!units[index].Alive && units[index].Faction != Faction.Champion) units.RemoveAt(index);
        }

        private UnitState Spawn(UnitDefinition definition, Faction faction, Vector2 position)
        {
            var unit = new UnitState
            {
                Id = ++nextId,
                Definition = definition,
                Faction = faction,
                Position = position,
                PreviousPosition = position,
                Health = faction == Faction.Companion ? float.PositiveInfinity : definition.Health,
                RoamRandom = faction == Faction.Companion ? new System.Random(unchecked(runSeed ^ nextId * 397)) : null
            };
            units.Add(unit);
            maximumRadius = Mathf.Max(maximumRadius, definition.Radius);
            maximumSpeed = Mathf.Max(maximumSpeed, definition.Speed);
            if (faction == Faction.Companion) CompanionCount++;
            return unit;
        }

        private void SpawnWaveEnemies()
        {
            if (Elapsed < nextSpawn) return;
            nextSpawn = Elapsed + Mathf.Max(0.3f, catalog.SpawnInterval * (1f - Elapsed / catalog.WaveDuration * 0.4f));
            int enemyCount = 0;
            for (int index = 0; index < units.Count; index++) if (units[index].Faction == Faction.Enemy && units[index].Alive) enemyCount++;
            if (enemyCount >= catalog.EnemyBudget) return;
            Vector2 half = catalog.ArenaSize * 0.5f - Vector2.one * 0.4f;
            Vector2 position = Vector2.zero;
            bool found = false;
            for (int attempt = 0; attempt < 12; attempt++)
            {
                float random = (float)spawnRandom.NextDouble() * 2f - 1f;
                switch (spawnRandom.Next(4))
                {
                    case 0: position = new Vector2(-half.x, random * half.y); break;
                    case 1: position = new Vector2(half.x, random * half.y); break;
                    case 2: position = new Vector2(random * half.x, -half.y); break;
                    default: position = new Vector2(random * half.x, half.y); break;
                }
                if ((position - Champion.Position).sqrMagnitude >= catalog.SpawnSafetyRadius * catalog.SpawnSafetyRadius) { found = true; break; }
            }
            if (found) Spawn(spawnRandom.Next(3) == 0 ? catalog.RangedEnemy : catalog.MeleeEnemy, Faction.Enemy, position);
        }

        private void FollowChampion(UnitState unit, float deltaTime)
        {
            UnitState target = FindTarget(unit, unit.Definition.Weapon.Range);
            float neighborhood = Mathf.Max(catalog.CompanionSeparation, unit.Definition.Radius + maximumRadius + 0.35f) + maximumSpeed * deltaTime;
            spatial.Query(unit.PreviousPosition - Vector2.one * neighborhood, unit.PreviousPosition + Vector2.one * neighborhood, separationCandidates);
            Vector2 next = CompanionMotion.Step(unit, Champion, separationCandidates, target, catalog, Elapsed, deltaTime);
            next = Champion.Position + Vector2.ClampMagnitude(next - Champion.Position, catalog.CompanionLeash);
            unit.Position = ClampPosition(next, unit.Definition.Radius);
            if (!unit.PendingAttack && unit.Velocity.sqrMagnitude > 0.01f) unit.Facing = unit.Velocity.normalized;
        }

        private void MoveEnemy(UnitState unit, float deltaTime)
        {
            if (unit.TelegraphUntil > Elapsed) return;
            Vector2 direction = Champion.Position - unit.Position;
            float distance = direction.magnitude;
            unit.Facing = distance > 0.001f ? direction / distance : unit.Facing;
            bool ranged = unit.Definition.Weapon.Kind == WeaponKind.Projectile;
            float stopDistance = ranged ? 4.8f : unit.Definition.Radius + Champion.Definition.Radius;
            float speed = unit.Definition.Speed * (unit.ChillUntil > Elapsed ? 0.55f : 1f);
            if (distance > stopDistance)
                unit.Position = ClampPosition(unit.Position + unit.Facing * Mathf.Min(speed * deltaTime, distance - stopDistance), unit.Definition.Radius);
            else if (ranged && distance < 3f)
                unit.Position = ClampPosition(unit.Position - unit.Facing * speed * deltaTime, unit.Definition.Radius);
        }

        private void ProcessStatusAndPassive(UnitState unit)
        {
            if (unit.BurnUntil > Elapsed && Elapsed >= unit.NextBurn)
            {
                unit.NextBurn = Elapsed + 0.5f;
                ApplyDamage(unit, unit.Definition.IsBoss ? 3f : 5f, Faction.Companion);
            }
            if (unit.Faction == Faction.Enemy || Elapsed < unit.NextPassive) return;
            unit.NextPassive = Elapsed + 4f;
            switch (unit.Definition.Passive)
            {
                case PassiveKind.HealChampion:
                    float healthBefore = Champion.Health;
                    Champion.Health = Mathf.Min(Champion.Definition.Health, Champion.Health + unit.Definition.PassiveStrength * 3f);
                    if (Champion.Health > healthBefore) events.Add(new CombatEvent(CombatEventKind.Heal, Champion.Position, Faction.Champion, Champion.Health - healthBefore, subjectId: Champion.Id));
                    break;
                case PassiveKind.Scavenge: Gold += Mathf.RoundToInt(unit.Definition.PassiveStrength); break;
                case PassiveKind.ChillAura:
                    float auraRadius = 2f + unit.Definition.PassiveStrength * 0.625f;
                    foreach (UnitState enemy in units)
                        if (enemy.Alive && enemy.Faction == Faction.Enemy && (enemy.Position - unit.Position).sqrMagnitude < auraRadius * auraRadius) enemy.ChillUntil = Elapsed + (enemy.Definition.IsBoss ? 0.6f : 1.5f);
                    break;
            }
        }

        private void ProcessBoss(UnitState boss)
        {
            if (boss.TelegraphUntil > 0f && Elapsed >= boss.TelegraphUntil)
            {
                float launchTime = boss.TelegraphUntil;
                boss.TelegraphUntil = 0f;
                int pattern = boss.Definition.BossPattern;
                int count = BossBulletCount(pattern);
                for (int index = 0; index < count; index++)
                {
                    float angle = BossBulletAngle(boss, index, launchTime);
                    Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    float speed = pattern == 3 ? 7f : pattern == 2 ? 3.5f + index % 4 * 0.7f : pattern == 4 ? 3.5f + index % 2 * 1.8f : 4.2f;
                    projectiles.Add(new ProjectileState { Id = ++nextId, Faction = Faction.Enemy, Position = boss.Position, PreviousPosition = boss.Position, Velocity = direction * speed, Damage = boss.Definition.Weapon.Damage, Remaining = 7f, Color = new Color(1f, 0.38f, 0.3f), SourceName = boss.Definition.DisplayName + " pattern" });
                }
                boss.NextBossPattern = Elapsed + 3.5f;
                events.Add(new CombatEvent(CombatEventKind.Fire, boss.Position, Faction.Enemy));
            }
            if (Elapsed < boss.NextBossPattern || boss.TelegraphUntil > 0f) return;
            boss.TelegraphUntil = Elapsed + 0.85f;
            boss.TelegraphDirection = (Champion.Position - boss.Position).normalized;
            events.Add(new CombatEvent(CombatEventKind.Windup, boss.Position, Faction.Enemy));
        }

        public static int BossBulletCount(int pattern) => pattern == 0 ? 12 : pattern == 1 ? 7 : pattern == 2 ? 16 : pattern == 3 ? 5 : 20;

        public static float BossBulletAngle(UnitState boss, int index, float launchTime)
        {
            int pattern = boss.Definition.BossPattern;
            int count = BossBulletCount(pattern);
            if (pattern == 2) return index / 4 * Mathf.PI * 0.5f + launchTime * 0.15f;
            if (pattern == 4) return index / 2 * Mathf.PI * 2f / (count / 2) + launchTime * 0.3f;
            return pattern == 1 || pattern == 3 ? Mathf.Atan2(boss.TelegraphDirection.y, boss.TelegraphDirection.x) + (index - count / 2) * 0.16f : index * Mathf.PI * 2f / count + launchTime * 0.3f;
        }

        private UnitState FindTarget(UnitState source, float range)
        {
            UnitState best = null;
            float bestDistance = range * range;
            spatial.Query(source.Position - Vector2.one * range, source.Position + Vector2.one * range, targetCandidates);
            for (int index = 0; index < targetCandidates.Count; index++)
            {
                UnitState candidate = targetCandidates[index];
                if (!CanHit(source.Faction, candidate)) continue;
                float distance = (candidate.Position - source.Position).sqrMagnitude;
                if (distance < bestDistance) { bestDistance = distance; best = candidate; }
            }
            return best;
        }

        private static bool CanHit(Faction source, UnitState target)
        {
            return target.Alive && (source == Faction.Enemy ? target.Faction == Faction.Champion : target.Faction == Faction.Enemy);
        }

        private void ProcessAttack(UnitState unit)
        {
            WeaponDefinition weapon = unit.Definition.Weapon;
            if (unit.Definition.IsBoss) return;
            if (weapon.Kind == WeaponKind.Contact) return;
            if (unit.PendingAttack && Elapsed >= unit.ImpactTime)
            {
                unit.PendingAttack = false;
                float damage = weapon.Damage;
                bool critical = combatRandom.NextDouble() < unit.Definition.CriticalChance;
                if (critical) damage *= 1.5f;
                if (weapon.Kind == WeaponKind.Projectile)
                {
                    EmitProjectile(unit, damage, critical);
                }
                else
                {
                    for (int index = 0; index < units.Count; index++)
                    {
                        UnitState target = units[index];
                        Vector2 delta = target.Position - unit.Position;
                        if (CanHit(unit.Faction, target) && delta.magnitude <= weapon.Range + target.Definition.Radius && Vector2.Dot(delta.normalized, unit.AttackDirection) >= weapon.ConeDot)
                            ApplyWeaponDamage(target, damage, unit.Faction, weapon.Element, unit.Definition.DisplayName, unit.AttackDirection, weapon, critical);
                    }
                }
                events.Add(new CombatEvent(CombatEventKind.Fire, unit.Position, unit.Faction, 0f, unit.AttackDirection, weapon, critical));
            }
            if (Elapsed < unit.NextAttack || unit.PendingAttack || Phase != RunPhase.Combat) return;
            UnitState nearest = FindTarget(unit, weapon.Range);
            if (nearest == null) return;
            unit.AttackDirection = (nearest.Position - unit.Position).normalized;
            unit.Facing = unit.AttackDirection;
            unit.AttackStarted = Elapsed;
            unit.ImpactTime = Elapsed + weapon.Windup;
            unit.NextAttack = Elapsed + weapon.Cooldown;
            unit.PendingAttack = true;
            events.Add(new CombatEvent(CombatEventKind.Windup, unit.Position, unit.Faction, 0f, unit.AttackDirection, weapon));
        }

        private void EmitProjectile(UnitState unit, float damage, bool critical)
        {
            WeaponDefinition weapon = unit.Definition.Weapon;
            Vector2 muzzleOffset = weapon.HasMuzzleOffset ? weapon.ProjectileMuzzleOffset : Vector2.right * weapon.MuzzleDistance;
            Vector2 rotatedMuzzle = Quaternion.Euler(0f, 0f, Mathf.Atan2(unit.AttackDirection.y, unit.AttackDirection.x) * Mathf.Rad2Deg) * muzzleOffset;
            Vector2 position = unit.Position + unit.AttackDirection * (weapon.MountDistance - weapon.RecoilDistance) + rotatedMuzzle;
            for (int index = 0; index < Mathf.Max(1, weapon.ProjectileCount); index++)
            {
                float angle = (index - (weapon.ProjectileCount - 1) * 0.5f) * 0.14f;
                Vector2 direction = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg) * unit.AttackDirection;
                projectiles.Add(new ProjectileState
                {
                    Id = ++nextId,
                    Faction = unit.Faction,
                    Position = position,
                    PreviousPosition = unit.Position,
                    Velocity = direction * weapon.ProjectileSpeed,
                    Damage = damage,
                    Remaining = weapon.ProjectileLifetime,
                    Color = weapon.ProjectileColor,
                    Element = weapon.Element,
                    SourceName = unit.Definition.DisplayName + " projectile",
                    LaunchSegment = true,
                    Sprite = weapon.ProjectileSprite,
                    VisualWidth = weapon.ProjectileVisualWidth,
                    Critical = critical
                });
            }
        }

        private void ProcessContact(UnitState enemy)
        {
            if (!enemy.Alive || Elapsed < enemy.NextContact) return;
            float radius = enemy.Definition.Radius + Champion.Definition.Radius + 0.05f;
            if ((enemy.Position - Champion.Position).sqrMagnitude > radius * radius) return;
            enemy.NextContact = Elapsed + enemy.Definition.ContactInterval;
            ApplyDamage(Champion, enemy.Definition.ContactDamage, Faction.Enemy, enemy.Definition.DisplayName + " contact", (Champion.Position - enemy.Position).normalized);
        }

        private void MoveProjectiles(float deltaTime)
        {
            for (int index = projectiles.Count - 1; index >= 0 && Phase == RunPhase.Combat; index--)
            {
                ProjectileState projectile = projectiles[index];
                Vector2 start = projectile.LaunchSegment ? projectile.PreviousPosition : projectile.Position;
                projectile.PreviousPosition = start;
                projectile.LaunchSegment = false;
                Vector2 destination = projectile.Position + projectile.Velocity * deltaTime;
                UnitState hit = null;
                float nearestFraction = 2f;
                Vector2 padding = Vector2.one * (maximumRadius + 0.055f);
                spatial.Query(Vector2.Min(start, destination) - padding, Vector2.Max(start, destination) + padding, projectileCandidates);
                for (int unitIndex = 0; unitIndex < projectileCandidates.Count; unitIndex++)
                {
                    UnitState target = projectileCandidates[unitIndex];
                    bool interceptor = projectile.Faction == Faction.Enemy && target.Faction == Faction.Companion && catalog.CompanionInterceptor;
                    if (!CanHit(projectile.Faction, target) && !interceptor) continue;
                    float fraction = SegmentCircle(start, destination, target.Position, target.Definition.Radius + 0.055f);
                    if (fraction >= 0f && fraction < nearestFraction) { nearestFraction = fraction; hit = target; }
                }
                projectile.Position = destination;
                projectile.Remaining -= deltaTime;
                if (hit != null)
                {
                    ApplyWeaponDamage(hit, projectile.Damage, projectile.Faction, projectile.Element, projectile.SourceName, projectile.Velocity.normalized, null, projectile.Critical);
                    projectiles.RemoveAt(index);
                }
                else if (projectile.Remaining <= 0f || Mathf.Abs(destination.x) > catalog.ArenaSize.x || Mathf.Abs(destination.y) > catalog.ArenaSize.y)
                    projectiles.RemoveAt(index);
            }
        }

        private static float SegmentCircle(Vector2 start, Vector2 end, Vector2 centre, float radius)
        {
            Vector2 offset = start - centre;
            Vector2 delta = end - start;
            float squared = delta.sqrMagnitude;
            if (offset.sqrMagnitude <= radius * radius) return 0f;
            if (squared < 0.000001f) return -1f;
            float projection = Vector2.Dot(offset, delta);
            float discriminant = projection * projection - squared * (offset.sqrMagnitude - radius * radius);
            if (discriminant < 0f) return -1f;
            float fraction = (-projection - Mathf.Sqrt(discriminant)) / squared;
            return fraction >= 0f && fraction <= 1f ? fraction : -1f;
        }

        private void ApplyDamage(UnitState target, float damage, Faction source, string sourceName = "", Vector2 direction = default, WeaponDefinition weapon = null, bool critical = false, DamageElement element = DamageElement.Physical)
        {
            if (!target.Alive || target.Faction == Faction.Companion || !float.IsFinite(damage) || damage <= 0f) return;
            if (target.Faction == Faction.Champion) damage *= 1f / (1f + Mathf.Max(0f, catalog.Armor));
            if (target.Faction == Faction.Champion) { LastDamageSource = string.IsNullOrEmpty(sourceName) ? "Enemy damage" : sourceName; LastDamageAmount = damage; }
            target.Health = Mathf.Max(0f, target.Health - damage);
            target.LastHit = Elapsed;
            target.HitDirection = direction;
            events.Add(new CombatEvent(CombatEventKind.Hit, target.Position, target.Faction, damage, direction, weapon, critical, element));
            if (target.Health > 0f) return;
            target.Alive = false;
            target.PendingAttack = false;
            events.Add(new CombatEvent(CombatEventKind.Kill, target.Position, target.Faction, direction: direction, subjectId: target.Id, subject: target));
            if (target.Faction == Faction.Champion) Phase = RunPhase.Defeat;
            else
            {
                if (target == Boss) BossDefeated = true;
                Kills++;
                int pickupId = ++nextId;
                float angle = pickupId * 2.399963f;
                pickups.Add(new PickupState { Id = pickupId, Position = target.Position, PreviousPosition = target.Position, SpawnTime = Elapsed, Velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 2.4f, Value = catalog.GoldPerKill });
            }
        }

        private void ApplyWeaponDamage(UnitState target, float damage, Faction source, DamageElement element, string sourceName = "", Vector2 direction = default, WeaponDefinition weapon = null, bool critical = false)
        {
            if (!target.Alive) return;
            bool reaction = target.Faction == Faction.Enemy && ((element == DamageElement.Ember && target.ChillUntil > Elapsed) || (element == DamageElement.Frost && target.BurnUntil > Elapsed));
            ApplyDamage(target, damage, source, sourceName, direction, weapon, critical, element);
            if (target.Faction != Faction.Enemy || (!target.Alive && element != DamageElement.Arcane)) return;
            if (reaction)
            {
                target.BurnUntil = target.ChillUntil = 0f;
                ApplyDamage(target, damage * 0.5f, source);
            }
            else if (element == DamageElement.Ember) target.BurnUntil = Elapsed + 2f;
            else if (element == DamageElement.Frost) target.ChillUntil = Elapsed + (target.Definition.IsBoss ? 0.6f : 1.5f);
            else if (element == DamageElement.Arcane)
            {
                foreach (UnitState enemy in units)
                    if (enemy != target && enemy.Alive && enemy.Faction == Faction.Enemy && (enemy.Position - target.Position).sqrMagnitude < 2.25f) { ApplyDamage(enemy, damage * 0.35f, source); break; }
            }
        }

        private void CollectPickups(float deltaTime)
        {
            for (int index = pickups.Count - 1; index >= 0; index--)
            {
                PickupState pickup = pickups[index];
                pickup.PreviousPosition = pickup.Position;
                float age = Elapsed - pickup.SpawnTime;
                if (age < 0.26f)
                {
                    pickup.Position = ClampPosition(pickup.Position + pickup.Velocity * deltaTime, 0.15f);
                    pickup.Velocity *= Mathf.Exp(-8f * deltaTime);
                    continue;
                }
                Vector2 difference = Champion.Position - pickup.Position;
                if (difference.sqrMagnitude <= catalog.PickupRadius * catalog.PickupRadius) pickup.Attracting = true;
                if (!pickup.Attracting) continue;
                pickup.Position = Vector2.MoveTowards(pickup.Position, Champion.Position, (7f + age * 2f) * deltaTime);
                if ((pickup.Position - Champion.Position).sqrMagnitude > 0.045f) continue;
                Gold = RunSession.BoundedCounter((long)Gold + pickup.Value);
                events.Add(new CombatEvent(CombatEventKind.Pickup, pickup.Position, Faction.Champion, pickup.Value, subjectId: pickup.Id));
                pickups.RemoveAt(index);
            }
        }

        public UnitState AddPresentationFixture(Vector2 position, float health)
        {
            if (!RunPaths.K2Capture) throw new InvalidOperationException("Presentation fixture requires isolated capture mode");
            UnitState unit = Spawn(catalog.MeleeEnemy, Faction.Enemy, position);
            unit.Health = health;
            return unit;
        }

#if UNITY_EDITOR
        public UnitState AddFixture(UnitDefinition definition, Faction faction, Vector2 position) => Spawn(definition, faction, position);
        public void DamageFixture(UnitState target, float damage) => ApplyDamage(target, damage, Faction.Enemy);
        public void WeaponDamageFixture(UnitState target, float damage, DamageElement element) => ApplyWeaponDamage(target, damage, Faction.Companion, element);
        public void ProjectileFixture(Vector2 position, Vector2 velocity, float damage, Faction faction, DamageElement element = DamageElement.Physical)
        {
            projectiles.Add(new ProjectileState { Id = ++nextId, Faction = faction, Position = position, PreviousPosition = position, Velocity = velocity, Damage = damage, Remaining = 2f, Color = Color.white, Element = element });
        }
#endif
    }
}
