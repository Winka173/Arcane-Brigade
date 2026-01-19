using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public enum RunPhase { Preparing, Combat, Paused, Victory, Defeat, Rest }
    public enum CompanionTravelMode { Patrol, Pursuit }
    public enum CombatEventKind { Windup, Fire, Hit, Kill, Pickup, Heal, ChestOpened, Proc, SignatureMark, SignaturePulse, SignatureBurst, Support, Dodge, BossLanded, ProjectileReturn, ProjectilePierce, ProjectileSplit, ProjectileBounce, Knockback, LootDrop, BossPhase, ContactWindup, StatusApplied, LootAttract, MimicWake }

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
        public readonly CombatCause Cause;
        public readonly LootKind Loot;
        public CombatEvent(CombatEventKind kind, Vector2 position, Faction faction, float value = 0f, Vector2 direction = default, WeaponDefinition weapon = null, bool critical = false, DamageElement element = DamageElement.Physical, int subjectId = 0, UnitState subject = null, CombatCause cause = default, LootKind loot = LootKind.Coin)
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
            Cause = cause;
            Loot = loot;
        }
    }

    public sealed class UnitState
    {
        public int Id;
        public UnitDefinition Definition;
        public float LastSupport = -100;
        public float NextCollision;
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
        public float ContactImpactAt;
        public bool PendingAttack;
        public Vector2 AttackDirection;
        public Vector2 RoamOffset;
        public Vector2 Velocity;
        public CompanionTravelMode TravelMode;
        public float NextRoamTime;
        public float RoamRestUntil;
        public System.Random RoamRandom;
        public float BurnUntil;
        public float ChillUntil;
        public float NextBurn;
        public float NextPassive;
        public float TelegraphUntil;
        public float NextBossPattern = 2f;
        public float EntranceUntil;
        public Vector2 TelegraphDirection;
        public Vector2 HitDirection;
        public CombatCause BurnCause;
        public int AttackBeat;
        public int AttackTargetId;
        public int VisibleMarks;
        public float MarkVisibleUntil;
        public float ExposedUntil;
        public float CameraWeakUntil;
        public int WeaknessSourceId;
        public int ExpandedBeat;
        public float AttackWindup;
        public float BehaviorUntil;
        public float NextBehavior;
        public bool Awakened;
        public float NextThermal;
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
        public int SourceUnitId;
        public CombatCause Cause;
        public WeaponDefinition Weapon;
        public bool Returning;
        public float Age;
        public bool FinalNote;
        public int RicochetsRemaining;
        public bool SplitOnImpact;
        public bool IsFragment;
        public float Radius = 0.055f;
        public int SplitCount;
        public int RandomBounces = 8;
        public readonly HashSet<int> Victims = new HashSet<int>();
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
        public LootKind Kind;
    }

    public sealed partial class CombatSimulation
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
        private System.Random lootRandom;
        private System.Random procRandom;
        private int nextCause;
        private float nextChest;
        private readonly Dictionary<string, float> procCooldowns = new Dictionary<string, float>();
        private readonly HashSet<string> procRoots = new HashSet<string>();
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
        public float WaveDuration => catalog.WaveDuration;
        public int Gold { get; private set; }
        public int Eggs { get; private set; }
        public int ChestsOpened { get; private set; }
        public int Kills { get; private set; }
        public int Generation { get; private set; }
        public int CompanionCount { get; private set; }
        public bool BossDefeated { get; private set; }
        public UnitState Boss { get; private set; }
        public bool IsBossOvertime => Boss != null && Boss.Alive && Elapsed >= WaveDuration && (Phase == RunPhase.Combat || Phase == RunPhase.Paused && phaseBeforePause == RunPhase.Combat);
        public float BossOvertimeSeconds => IsBossOvertime ? Mathf.Max(0f, Elapsed - WaveDuration) : 0f;
        public string LastDamageSource { get; private set; } = "";
        public float LastDamageAmount { get; private set; }
        public string LastDamageKind { get; private set; } = "";

        public CombatSimulation(CheckpointCatalog definition) { catalog = definition; }

        public void Start(int seed)
        {
            ResetHeroMechanic();
            units.Clear();
            projectiles.Clear();
            pickups.Clear();
            events.Clear();
            spawnRandom = new System.Random(seed);
            ClearSignatures();
            contributions.Clear(); contributionIndex.Clear(); nextHarvestBeat = 0; HarvestBeats = 0;
            combatRandom = new System.Random(unchecked(seed ^ 0x51B31));
            lootRandom = new System.Random(unchecked(seed ^ 0xC4357));
            procRandom = new System.Random(unchecked(seed ^ 0xA1703));
            procCooldowns.Clear(); procRoots.Clear(); nextCause = 0;
            nextChest = Mathf.Max(4f, catalog.ChestInterval / Mathf.Max(0.25f, 1f + catalog.TreasureDetection));
            runSeed = seed;
            nextId = 0;
            nextSpawn = 0.6f;
            Elapsed = 0f;
            restElapsed = 0f;
            Gold = 0;
            Eggs = ChestsOpened = 0;
            Kills = 0;
            CompanionCount = 0;
            BossDefeated = false;
            Boss = null;
            LastDamageSource = "";
            LastDamageAmount = 0f;
            LastDamageKind = "";
            maximumRadius = maximumSpeed = 0f;
            Generation++;
            Champion = Spawn(catalog.Champion, Faction.Champion, Vector2.zero);
            if (!catalog.ExplicitStartingRoster && (catalog.StartingCompanions == null || catalog.StartingCompanions.Length == 0))
                AddCompanion(catalog.Companion);
            else if (catalog.StartingCompanions != null)
                for (int index = 0; index < catalog.StartingCompanions.Length; index++) AddCompanion(catalog.StartingCompanions[index]);
            if (catalog.Boss != null)
            {
                Boss = Spawn(catalog.Boss, Faction.Enemy, ClampPosition(Champion.Position + Vector2.up * 2.8f, catalog.Boss.Radius));
                Boss.EntranceUntil = 1.2f;
                Boss.NextContact = Boss.NextAttack = 1.45f;
                Boss.NextBossPattern = 2.2f;
            }
            spatial.Rebuild(units);
            Phase = RunPhase.Combat;
            ResetArenaToys(catalog.RunWave);
            ResetExpansion();
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
            ClearSignatures();
            ClearArenaToys();
            ClearExpansion();
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
                unit.Position = Champion.Position + Vector2.ClampMagnitude(unit.Position - Champion.Position, CompanionMotion.Leash(unit, catalog));
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
            float beforeRegeneration = Champion.Health;
            Champion.Health = Mathf.Min(Champion.Definition.Health, Champion.Health + catalog.PassiveHeal * deltaTime);
            if (Champion.Health > beforeRegeneration) RecordHealing(NewCause(Champion.Id).Child("passive-regen"), Champion.Health - beforeRegeneration);
            if (movement.sqrMagnitude > 0.001f) Champion.Facing = movement.normalized;
            SpawnWaveEnemies();
            spatial.Rebuild(units);

            for (int index = 0; index < units.Count && Phase == RunPhase.Combat; index++)
            {
                UnitState unit = units[index];
                if (!unit.Alive) continue;
                if (unit.EntranceUntil > Elapsed) continue;
                if (unit.EntranceUntil > 0) events.Add(new CombatEvent(CombatEventKind.BossLanded, unit.Position, unit.Faction, subjectId: unit.Id, subject: unit));
                unit.EntranceUntil = 0;
                if (unit.Faction == Faction.Companion) FollowChampion(unit, deltaTime);
                else if (unit.Faction == Faction.Enemy) MoveEnemy(unit, deltaTime);
                spatial.Move(unit);
                ProcessStatusAndPassive(unit);
                if (!unit.Alive) continue;
                if (unit.Definition.IsBoss) ProcessBoss(unit);
                ProcessAttack(unit);
                if (unit.Faction == Faction.Enemy) ProcessContact(unit);
                else ProcessOutgoingCollision(unit);
            }

            if (Phase == RunPhase.Combat)
            {
                MoveProjectiles(deltaTime);
                if (Phase == RunPhase.Combat)
                {
                    CollectPickups(deltaTime);
                    SpawnTreasure();
                    ProcessHeroMechanic(deltaTime);
                    ProcessSignatureRelics();
                    ProcessSignatures();
                    ProcessArenaToys(deltaTime);
                    ProcessExpansion(deltaTime);
                    ProcessProcs();
                }
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
            if (catalog.Boss != null && Elapsed >= catalog.WaveDuration) return;
            if (Elapsed < nextSpawn) return;
            nextSpawn = Elapsed + Mathf.Max(0.08f, catalog.SpawnInterval * (1f - Elapsed / catalog.WaveDuration * 0.4f));
            int enemyCount = 0;
            for (int index = 0; index < units.Count; index++) if (units[index].Faction == Faction.Enemy && units[index].Alive) enemyCount++;
            if (enemyCount >= catalog.EnemyBudget) return;
            int batch = catalog.RunWave >= 5 ? 3 : catalog.RunWave == 4 ? 2 : 1;
            for (int member = 0; member < batch && enemyCount + member < catalog.EnemyBudget; member++)
            {
                Vector2 half = catalog.ArenaSize * 0.5f - Vector2.one * 0.4f;
                if (catalog.RunWave >= 4) half = Vector2.Min(half, new Vector2(9, 5.6f));
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
                    if (catalog.RunWave >= 4) position = ClampPosition(position + Champion.Position, 0.6f);
                    if ((position - Champion.Position).sqrMagnitude >= catalog.SpawnSafetyRadius * catalog.SpawnSafetyRadius) { found = true; break; }
                }
                if (found) Spawn(catalog.ExpandedContent && catalog.EnemyPool.Length > 0 ? catalog.EnemyPool[spawnRandom.Next(catalog.EnemyPool.Length)] : spawnRandom.Next(3) == 0 ? catalog.RangedEnemy : catalog.MeleeEnemy, Faction.Enemy, position);
            }
        }

        private void FollowChampion(UnitState unit, float deltaTime)
        {
            float leash = CompanionMotion.Leash(unit, catalog);
            UnitState target = unit.Definition.Weapon.Kind == WeaponKind.Melee ? FindPursuitTarget(unit, leash + unit.Definition.Weapon.Range * 0.65f) : FindTarget(unit, unit.Definition.Weapon.Range);
            float neighborhood = Mathf.Max(catalog.CompanionSeparation, unit.Definition.Radius + maximumRadius + 0.35f) + maximumSpeed * deltaTime;
            spatial.Query(unit.PreviousPosition - Vector2.one * neighborhood, unit.PreviousPosition + Vector2.one * neighborhood, separationCandidates);
            Vector2 next = CompanionMotion.Step(unit, Champion, separationCandidates, target, catalog, Elapsed, deltaTime);
            next = Champion.Position + Vector2.ClampMagnitude(next - Champion.Position, leash);
            unit.Position = ClampPosition(next, unit.Definition.Radius);
            if (!unit.PendingAttack && unit.Velocity.sqrMagnitude > 0.01f) unit.Facing = unit.Velocity.normalized;
        }

        private UnitState FindPursuitTarget(UnitState unit, float radius)
        {
            spatial.Query(Champion.Position - Vector2.one * radius, Champion.Position + Vector2.one * radius, targetCandidates);
            UnitState nearest = null; float bestDistance = float.PositiveInfinity;
            foreach (UnitState candidate in targetCandidates)
            {
                if (!CanHit(unit.Faction, candidate) || (candidate.Position - Champion.Position).sqrMagnitude > radius * radius) continue;
                float distance = (candidate.Position - unit.Position).sqrMagnitude;
                if (distance < bestDistance) { nearest = candidate; bestDistance = distance; }
            }
            return nearest;
        }

        private void MoveEnemy(UnitState unit, float deltaTime)
        {
            if (MoveExpandedEnemy(unit, deltaTime)) return;
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
                ApplyDamage(unit, unit.Definition.IsBoss ? 3f : 5f, Faction.Companion, cause: unit.BurnCause);
            }
            if (unit.Faction == Faction.Enemy || Elapsed < unit.NextPassive) return;
            unit.NextPassive = Elapsed + 4f;
            switch (unit.Definition.Passive)
            {
                case PassiveKind.HealChampion:
                    float healthBefore = Champion.Health;
                    Champion.Health = Mathf.Min(Champion.Definition.Health, Champion.Health + unit.Definition.PassiveStrength * 3f);
                    if (Champion.Health > healthBefore)
                    {
                        CombatCause healingCause = NewCause(unit.Id);
                        RecordHealing(healingCause.Child("unit-passive"), Champion.Health - healthBefore);
                        events.Add(new CombatEvent(CombatEventKind.Heal, Champion.Position, Faction.Champion, Champion.Health - healthBefore, subjectId: Champion.Id, cause: healingCause));
                        unit.LastSupport = Elapsed;
                        events.Add(new CombatEvent(CombatEventKind.Support, unit.Position, unit.Faction, subject: unit, element: DamageElement.Arcane));
                    }
                    break;
                case PassiveKind.Scavenge:
                    int nextGold = RunSession.BoundedCounter((long)Gold + Mathf.RoundToInt(unit.Definition.PassiveStrength));
                    if (nextGold > Gold)
                    {
                        unit.LastSupport = Elapsed;
                        events.Add(new CombatEvent(CombatEventKind.Support, unit.Position, unit.Faction, subject: unit));
                    }
                    Contribution(NewCause(unit.Id).Child("scavenge")).Coins += nextGold - Gold; Gold = nextGold; break;
                case PassiveKind.ChillAura:
                    float auraRadius = 2f + unit.Definition.PassiveStrength * 0.625f;
                    bool chilled = false;
                    foreach (UnitState enemy in units)
                        if (enemy.Alive && enemy.Faction == Faction.Enemy && (enemy.Position - unit.Position).sqrMagnitude < auraRadius * auraRadius) { enemy.ChillUntil = Elapsed + (enemy.Definition.IsBoss ? 0.6f : 1.5f); chilled = true; }
                    if (chilled)
                    {
                        unit.LastSupport = Elapsed;
                        events.Add(new CombatEvent(CombatEventKind.Support, unit.Position, unit.Faction, element: DamageElement.Frost, subject: unit));
                    }
                    break;
            }
        }

        private void ProcessBoss(UnitState boss)
        {
            if (catalog.RunWave == 1) return;
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
                    float speed = catalog.RunWave > 0 ? pattern == 2 ? 1.8f + index % 4 * 0.2f : pattern == 4 ? 2f + index % 2 * 0.4f : 2.4f : pattern == 3 ? 7f : pattern == 2 ? 3.5f + index % 4 * 0.7f : pattern == 4 ? 3.5f + index % 2 * 1.8f : 4.2f;
                    projectiles.Add(new ProjectileState { Id = ++nextId, Faction = Faction.Enemy, Position = boss.Position, PreviousPosition = boss.Position, Velocity = direction * speed, Damage = boss.Definition.Weapon.Damage, Remaining = 7f, Color = new Color(1f, 0.38f, 0.3f), SourceName = boss.Definition.DisplayName + " pattern", SourceUnitId = boss.Id });
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
            return target.Alive && target.EntranceUntil <= 0 && (source == Faction.Enemy ? target.Faction == Faction.Champion : target.Faction == Faction.Enemy);
        }

        private void ProcessAttack(UnitState unit)
        {
            WeaponDefinition weapon = unit.Definition.Weapon;
            if (unit.Definition.IsBoss) return;
            if (weapon.Kind == WeaponKind.Contact) return;
            if (catalog.ExpandedContent && unit.Definition.Behavior == EnemyBehavior.Mimic && (!unit.Awakened || unit.BehaviorUntil > Elapsed)) return;
            if (unit.PendingAttack && Elapsed >= unit.ImpactTime)
            {
                unit.PendingAttack = false;
                float damage = weapon.Damage;
                CombatCause attackCause = NewCause(unit.Id);
                UnitState locked = units.Find(candidate => candidate.Id == unit.AttackTargetId);
                bool critical = combatRandom.NextDouble() < Mathf.Clamp01(unit.Definition.CriticalChance + (locked != null && locked.CameraWeakUntil > Elapsed && unit.Faction != Faction.Enemy ? 0.15f : 0));
                if (critical) damage *= 1.5f;
                if (ResolveExpandedAttack(unit, damage, critical, attackCause)) { }
                else if (weapon.Kind == WeaponKind.Projectile)
                {
                    EmitProjectile(unit, damage, critical, attackCause);
                }
                else if (weapon.Signature == SignatureWeapon.Stamp && unit.Faction != Faction.Enemy)
                {
                    UnitState target = units.Find(candidate => candidate.Id == unit.AttackTargetId);
                    if (target != null && CanHit(unit.Faction, target) && (target.Position - unit.Position).magnitude <= weapon.Range + target.Definition.Radius)
                    {
                        ApplyDamage(target, damage, unit.Faction, unit.Definition.DisplayName, unit.AttackDirection, weapon, critical, cause: attackCause);
                        AddStamp(unit, target, damage, attackCause);
                    }
                }
                else
                {
                    for (int index = 0; index < units.Count; index++)
                    {
                        UnitState target = units[index];
                        Vector2 delta = target.Position - unit.Position;
                        if (CanHit(unit.Faction, target) && delta.magnitude <= weapon.Range + target.Definition.Radius && Vector2.Dot(delta.normalized, unit.AttackDirection) >= weapon.ConeDot)
                            ApplyWeaponDamage(target, damage, unit.Faction, weapon.Element, unit.Definition.DisplayName, unit.AttackDirection, weapon, critical, attackCause);
                    }
                }
                AdvanceSignatureBeat(unit, damage, attackCause);
                events.Add(new CombatEvent(CombatEventKind.Fire, unit.Position, unit.Faction, 0f, unit.AttackDirection, weapon, critical, cause: attackCause));
            }
            if (Elapsed < unit.NextAttack || unit.PendingAttack || Phase != RunPhase.Combat) return;
            UnitState nearest = FindTarget(unit, weapon.Range);
            if (nearest == null) return;
            unit.AttackDirection = (nearest.Position - unit.Position).normalized;
            unit.AttackTargetId = nearest.Id;
            unit.Facing = unit.AttackDirection;
            unit.AttackStarted = Elapsed;
            unit.AttackWindup = weapon.Windup * (weapon.Family == ExpandedWeapon.Hammer && unit.ExpandedBeat % 3 == 2 ? 1.5f : 1);
            unit.ImpactTime = Elapsed + unit.AttackWindup;
            unit.NextAttack = Elapsed + weapon.Cooldown * (weapon.Family == ExpandedWeapon.Hammer && unit.ExpandedBeat % 3 == 2 ? 1.35f : 1);
            unit.PendingAttack = true;
            events.Add(new CombatEvent(CombatEventKind.Windup, unit.Position, unit.Faction, 0f, unit.AttackDirection, weapon, cause: NewCause(unit.Id)));
        }

        private void EmitProjectile(UnitState unit, float damage, bool critical, CombatCause cause)
        {
            if (unit.Faction == Faction.Enemy && catalog.RunWave == 1) return;
            WeaponDefinition weapon = unit.Definition.Weapon;
            Vector2 muzzleOffset = weapon.HasMuzzleOffset ? weapon.ProjectileMuzzleOffset : Vector2.right * weapon.MuzzleDistance;
            Vector2 rotatedMuzzle = Quaternion.Euler(0f, 0f, Mathf.Atan2(unit.AttackDirection.y, unit.AttackDirection.x) * Mathf.Rad2Deg) * muzzleOffset;
            Vector2 position = unit.Position + unit.AttackDirection * (weapon.MountDistance - weapon.RecoilDistance) + rotatedMuzzle;
            if (unit.Faction == Faction.Enemy) position = unit.Position + unit.AttackDirection * unit.Definition.Radius * 0.9f;
            for (int index = 0; index < Mathf.Clamp(weapon.ProjectileCount, 1, 96) && projectiles.Count < 4096; index++)
            {
                float angle = (index - (weapon.ProjectileCount - 1) * 0.5f) * weapon.ProjectileSpread;
                Vector2 direction = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg) * unit.AttackDirection;
                projectiles.Add(new ProjectileState
                {
                    Id = ++nextId,
                    Faction = unit.Faction,
                    Position = position,
                    PreviousPosition = unit.Position,
                    Velocity = direction * (unit.Faction == Faction.Enemy && catalog.RunWave > 0 ? Mathf.Min(2.4f, weapon.ProjectileSpeed) : weapon.ProjectileSpeed),
                    Damage = damage,
                    Remaining = weapon.ProjectileLifetime,
                    Color = weapon.ProjectileColor,
                    Element = weapon.Element,
                    SourceName = unit.Definition.DisplayName + " projectile",
                    LaunchSegment = true,
                    Sprite = weapon.ProjectileSprite,
                    VisualWidth = weapon.ProjectileVisualWidth,
                    Radius = weapon.ProjectileRadius,
                    SplitCount = weapon.ProjectileSplit,
                    Critical = critical,
                    SourceUnitId = unit.Id,
                    Cause = cause,
                    Weapon = weapon,
                    RicochetsRemaining = weapon.Family == ExpandedWeapon.TetherBow ? unit.Definition.CombatTier >= 2 ? 3 : 2 : weapon.Family == ExpandedWeapon.Fish ? 1 : 0,
                    SplitOnImpact = weapon.Family == ExpandedWeapon.Tea || weapon.ProjectileSplit > 0,
                    FinalNote = weapon.Family == ExpandedWeapon.MusicBox && unit.ExpandedBeat % 3 == 0
                });
            }
        }

        private void ProcessContact(UnitState enemy)
        {
            if (!enemy.Alive || Elapsed < enemy.NextContact) return;
            if (catalog.ExpandedContent && enemy.Definition.Behavior == EnemyBehavior.Mimic && (!enemy.Awakened || enemy.BehaviorUntil > Elapsed)) return;
            float radius = enemy.Definition.Radius + Champion.Definition.Radius + 0.05f;
            if ((enemy.Position - Champion.Position).sqrMagnitude > radius * radius)
            {
                if (enemy.ContactImpactAt > 0) { enemy.ContactImpactAt = 0; enemy.NextContact = Elapsed + 0.15f; }
                return;
            }
            if (enemy.ContactImpactAt == 0)
            {
                enemy.ContactImpactAt = Elapsed + 0.22f;
                events.Add(new CombatEvent(CombatEventKind.ContactWindup, enemy.Position, enemy.Faction, subject: enemy));
                if (enemy.Definition.Weapon.Kind == WeaponKind.Contact)
                {
                    enemy.AttackStarted = Elapsed; enemy.AttackWindup = 0.22f;
                    enemy.AttackDirection = (Champion.Position - enemy.Position).normalized;
                }
                return;
            }
            if (Elapsed < enemy.ContactImpactAt) return;
            enemy.ContactImpactAt = 0;
            enemy.NextContact = Elapsed + enemy.Definition.ContactInterval;
            ApplyDamage(Champion, enemy.Definition.ContactDamage, Faction.Enemy, enemy.Definition.DisplayName + " contact", (Champion.Position - enemy.Position).normalized, cause: NewCause(enemy.Id));
        }

        private void MoveProjectiles(float deltaTime)
        {
            for (int index = projectiles.Count - 1; index >= 0 && Phase == RunPhase.Combat; index--)
            {
                ProjectileState projectile = projectiles[index];
                int expandedMotion = MoveExpandedProjectile(projectile, deltaTime);
                if (expandedMotion != 0) { if (expandedMotion < 0) projectiles.RemoveAt(index); continue; }
                if (!AdvanceCombatProjectile(projectile, deltaTime)) projectiles.RemoveAt(index);
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

        private void ApplyDamage(UnitState target, float damage, Faction source, string sourceName = "", Vector2 direction = default, WeaponDefinition weapon = null, bool critical = false, DamageElement element = DamageElement.Physical, CombatCause cause = default, bool injectedDamage = false)
        {
            if (!target.Alive || target.Faction == Faction.Companion || !float.IsFinite(damage) || damage <= 0f) return;
            if (target.EntranceUntil > Elapsed && !injectedDamage) return;
            damage = ExpandedDamage(target, damage, source, direction, weapon);
            if (cause.RootId == 0) cause = NewCause(0);
            if (target.Faction == Faction.Champion)
            {
                if (!injectedDamage && catalog.Dodge > 0 && combatRandom.NextDouble() < Mathf.Clamp(catalog.Dodge, 0, 0.6f))
                {
                    events.Add(new CombatEvent(CombatEventKind.Dodge, target.Position, target.Faction, subject: target));
                    return;
                }
                damage *= catalog.Armor >= 0 ? 1f / (1f + catalog.Armor) : 1 + Mathf.Min(1, -catalog.Armor);
                if (!injectedDamage) damage = Mathf.Min(damage, Mathf.Min(14f, target.Definition.Health * 0.25f));
            }
            float baseDamage = damage;
            if (target.Faction == Faction.Enemy && weapon != null && weapon.Motion == WeaponMotion.Bow && treasureUntil > Elapsed && source != Faction.Enemy)
                damage *= 1f + Mathf.Min(0.45f, catalog.TreasureStacks * 0.15f);
            float treasureDamage = damage;
            if (target.Faction == Faction.Enemy && critical && target.ExposedUntil > Elapsed && catalog.OfficeLensStacks > 0) damage *= 1.25f + Mathf.Min(0.25f, catalog.OfficeLensStacks * 0.05f);
            if (target.Faction == Faction.Enemy && source != Faction.Enemy)
            {
                float primary = Mathf.Min(target.Health, baseDamage);
                RecordDamage(cause, primary);
                float treasureExtra = Mathf.Min(target.Health, treasureDamage) - primary;
                if (treasureExtra > 0) RecordDamage(cause.Child("treasure-ribbon"), treasureExtra);
                float extra = Mathf.Min(target.Health, damage) - Mathf.Min(target.Health, treasureDamage);
                if (extra > 0) RecordDamage(cause.Child("office-lens"), extra);
            }
            if (target.Faction == Faction.Champion) { LastDamageSource = string.IsNullOrEmpty(sourceName) ? "Enemy damage" : sourceName; LastDamageAmount = damage; LastDamageKind = element + " / " + (weapon == null ? "Contact / arena" : weapon.Kind.ToString()); }
            target.Health = Mathf.Max(0f, target.Health - damage);
            target.LastHit = Elapsed;
            target.HitDirection = direction;
            if (target.Health > 0 && target.Faction == Faction.Enemy && source != Faction.Enemy && weapon != null && direction.sqrMagnitude > 0.001f)
            {
                target.Position = ClampPosition(target.Position + direction.normalized * Mathf.Clamp(weapon.PushDistance, 0, 0.8f) * (1 - Mathf.Clamp01(target.Definition.KnockbackResistance)), target.Definition.Radius);
                spatial.Move(target);
                if (weapon.PushDistance >= 0.5f) events.Add(new CombatEvent(CombatEventKind.Knockback, target.Position, source, direction: direction, weapon: weapon, cause: cause));
            }
            events.Add(new CombatEvent(CombatEventKind.Hit, target.Position, target.Faction, damage, direction, weapon, critical, element, target.Id, target, cause));
            NotifyArenaToyHit(events[events.Count - 1]);
            if (target.Health > 0f) return;
            target.Alive = false;
            target.PendingAttack = false;
            events.Add(new CombatEvent(CombatEventKind.Kill, target.Position, target.Faction, direction: direction, subjectId: target.Id, subject: target, cause: cause));
            if (target.Faction == Faction.Champion) Phase = RunPhase.Defeat;
            else
            {
                if (target == Boss) BossDefeated = true;
                Kills++;
                if (source != Faction.Enemy) RecordKill(cause);
                DropLoot(LootKind.Coin, target.Position, catalog.GoldPerKill);
                if (catalog.ExtendedLoot)
                {
                    if (lootRandom.NextDouble() < Mathf.Clamp01(catalog.EggDropChance * (1f + catalog.Luck))) DropLoot(LootKind.Egg, target.Position, 1);
                    if (lootRandom.NextDouble() < Mathf.Clamp01(catalog.HeartDropChance * (1f + catalog.Luck))) DropLoot(LootKind.Heart, target.Position, 8);
                }
            }
        }

        private void ApplyWeaponDamage(UnitState target, float damage, Faction source, DamageElement element, string sourceName = "", Vector2 direction = default, WeaponDefinition weapon = null, bool critical = false, CombatCause cause = default)
        {
            if (!target.Alive) return;
            if (cause.RootId == 0) cause = NewCause(0);
            bool reaction = target.Faction == Faction.Enemy && ((element == DamageElement.Ember && target.ChillUntil > Elapsed) || (element == DamageElement.Frost && target.BurnUntil > Elapsed));
            ApplyDamage(target, damage, source, sourceName, direction, weapon, critical, element, cause);
            if (target.Faction != Faction.Enemy || (!target.Alive && element != DamageElement.Arcane)) return;
            if (reaction)
            {
                target.BurnUntil = target.ChillUntil = 0f;
                ApplyDamage(target, damage * 0.5f, source, cause: cause.Child("element-reaction"));
            }
            else if (element == DamageElement.Ember) { target.BurnUntil = Elapsed + 2f; target.BurnCause = cause.Child("ember-status"); }
            else if (element == DamageElement.Frost)
            {
                bool newlyChilled = target.ChillUntil <= Elapsed;
                target.ChillUntil = Elapsed + (target.Definition.IsBoss ? 0.6f : 1.5f);
                if (newlyChilled) events.Add(new CombatEvent(CombatEventKind.StatusApplied, target.Position, target.Faction, element: element, cause: cause));
            }
            else if (element == DamageElement.Arcane)
            {
                foreach (UnitState enemy in units)
                    if (enemy != target && enemy.Alive && enemy.Faction == Faction.Enemy && (enemy.Position - target.Position).sqrMagnitude < 2.25f) { ApplyDamage(enemy, damage * 0.35f, source, cause: cause.Child("arcane-chain")); break; }
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
                if (pickup.Kind == LootKind.Chest)
                {
                    if (difference.sqrMagnitude > catalog.ChestOpenRadius * catalog.ChestOpenRadius) continue;
                    OpenChest(pickup);
                    pickups.RemoveAt(index);
                    continue;
                }
                if (!pickup.Attracting && difference.sqrMagnitude <= catalog.PickupRadius * catalog.PickupRadius)
                {
                    pickup.Attracting = true;
                    if (pickup.Kind == LootKind.Coin) events.Add(new CombatEvent(CombatEventKind.LootAttract, pickup.Position, Faction.Champion, subjectId: pickup.Id, loot: pickup.Kind));
                }
                if (!pickup.Attracting) continue;
                pickup.Position = Vector2.MoveTowards(pickup.Position, Champion.Position, (7f + age * 2f) * deltaTime);
                if ((pickup.Position - Champion.Position).sqrMagnitude > 0.045f) continue;
                CombatCause pickupCause = NewCause(Champion.Id);
                if (pickup.Kind == LootKind.Coin)
                {
                    int nextGold = RunSession.BoundedCounter((long)Gold + pickup.Value);
                    Contribution(pickupCause.Child("coin-credit")).Coins = RunSession.BoundedCounter((long)Contribution(pickupCause.Child("coin-credit")).Coins + nextGold - Gold);
                    Gold = nextGold;
                }
                else if (pickup.Kind == LootKind.Egg) Eggs = RunSession.BoundedCounter((long)Eggs + pickup.Value);
                else if (pickup.Kind == LootKind.Heart) Heal(Mathf.Max(0, pickup.Value + catalog.HeartHealing), NewCause(Champion.Id));
                events.Add(new CombatEvent(CombatEventKind.Pickup, pickup.Position, Faction.Champion, pickup.Value, subjectId: pickup.Id, cause: pickupCause, loot: pickup.Kind));
                pickups.RemoveAt(index);
            }
        }

        private CombatCause NewCause(int sourceUnitId) => new CombatCause(++nextCause, sourceUnitId);

        private void DropLoot(LootKind kind, Vector2 position, int value)
        {
            int identifier = ++nextId;
            float angle = identifier * 2.399963f;
            Vector2 origin = ClampPosition(position, 0.3f);
            pickups.Add(new PickupState { Id = identifier, Kind = kind, Position = origin, PreviousPosition = origin, SpawnTime = Elapsed, Velocity = kind == LootKind.Chest ? Vector2.zero : new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 2.4f, Value = value });
            events.Add(new CombatEvent(CombatEventKind.LootDrop, origin, Faction.Champion, subjectId: identifier, loot: kind));
        }

        private void SpawnTreasure()
        {
            if (!catalog.ExtendedLoot || Elapsed < nextChest) return;
            nextChest = Elapsed + Mathf.Max(4f, catalog.ChestInterval / Mathf.Max(0.25f, 1f + catalog.TreasureDetection));
            int chestCount = 0;
            foreach (PickupState pickup in pickups) if (pickup.Kind == LootKind.Chest) chestCount++;
            if (chestCount >= 4) return;
            Vector2 half = catalog.ArenaSize * 0.5f - Vector2.one;
            Vector2 position = new Vector2(((float)lootRandom.NextDouble() * 2 - 1) * half.x, ((float)lootRandom.NextDouble() * 2 - 1) * half.y);
            DropLoot(LootKind.Chest, position, Mathf.Max(1, Mathf.RoundToInt((8 + lootRandom.Next(0, 9)) * K10StatRules.Factor(catalog.ChestQuality, 0.2f, 5))));
        }

        private void OpenChest(PickupState chest)
        {
            ChestsOpened++;
            CombatCause cause = NewCause(Champion.Id);
            events.Add(new CombatEvent(CombatEventKind.ChestOpened, chest.Position, Faction.Champion, chest.Value, subjectId: chest.Id, cause: cause, loot: LootKind.Chest));
            DropLoot(LootKind.Coin, chest.Position, chest.Value);
            if (lootRandom.NextDouble() < Mathf.Clamp01(0.35f + catalog.Luck * 0.2f)) DropLoot(LootKind.Egg, chest.Position, 1);
            if (lootRandom.NextDouble() < Mathf.Clamp01(0.5f + catalog.Luck * 0.2f)) DropLoot(LootKind.Heart, chest.Position, 12);
        }

        private void Heal(float amount, CombatCause cause)
        {
            if (!Champion.Alive) return;
            float previous = Champion.Health;
            Champion.Health = Mathf.Min(Champion.Definition.Health, Champion.Health + Mathf.Max(0, amount));
            RecordHealing(cause, Champion.Health - previous);
            if (Champion.Health > previous) events.Add(new CombatEvent(CombatEventKind.Heal, Champion.Position, Faction.Champion, Champion.Health - previous, subjectId: Champion.Id, cause: cause));
        }

        private void ProcessProcs()
        {
            procRoots.Clear();
            int originalEvents = events.Count;
            for (int index = 0; index < originalEvents; index++)
            {
                CombatEvent trigger = events[index];
                if (trigger.Cause.RootId == 0 || trigger.Cause.Depth != 0 || !Champion.Alive) continue;
                foreach (ActiveProc active in catalog.Procs ?? new ActiveProc[0])
                {
                    ProcRule rule = active.Rule;
                    bool matches = rule.Trigger == ProcTrigger.CoinCollected ? trigger.Kind == CombatEventKind.Pickup && trigger.Loot == LootKind.Coin : rule.Trigger == ProcTrigger.ChestOpened ? trigger.Kind == CombatEventKind.ChestOpened : trigger.Kind == CombatEventKind.Kill && trigger.Faction == Faction.Enemy;
                    if (!matches || active.Stacks <= 0 || !procRoots.Add(rule.Id + "/" + trigger.Cause.RootId)) continue;
                    if (procCooldowns.TryGetValue(rule.Id, out float readyAt) && readyAt > Elapsed) continue;
                    if (procRandom.NextDouble() >= rule.Chance) continue;
                    procCooldowns[rule.Id] = Elapsed + rule.Cooldown;
                    CombatCause cause = new CombatCause(trigger.Cause.RootId, Champion.Id, rule.ItemId, 1);
                    events.Add(new CombatEvent(CombatEventKind.Proc, trigger.Position, Faction.Champion, rule.Power * active.Stacks, cause: cause));
                    if (rule.Action == ProcAction.HealChampion) Heal(rule.Power * active.Stacks, cause);
                    else
                        foreach (UnitState target in units)
                            if (target.Alive && target.Faction == Faction.Enemy && (target.Position - trigger.Position).sqrMagnitude <= rule.Radius * rule.Radius) ApplyDamage(target, rule.Power * active.Stacks, Faction.Champion, rule.Id, cause: cause);
                }
            }
        }

        public void AddLootPresentationFixture(LootKind kind, Vector2 position, int value)
        {
            if (!RunPaths.K3FinalCapture && !RunPaths.QualityCapture && !RunPaths.K4Capture && !RunPaths.K4FinalCapture && !RunPaths.K5Capture) throw new InvalidOperationException("Loot fixture requires isolated capture mode");
            DropLoot(kind, position, value);
        }

        public void ConfigureLootPresentationFixture()
        {
            if (!RunPaths.K3FinalCapture && !RunPaths.QualityCapture && !RunPaths.K4Capture && !RunPaths.K4FinalCapture && !RunPaths.K5Capture) throw new InvalidOperationException("Loot fixture requires isolated capture mode");
            catalog.EnemyBudget = 0;
            nextChest = float.PositiveInfinity;
            if (RunPaths.K4Capture || RunPaths.K4FinalCapture) { catalog.CompanionPatrolSpeed = 0; catalog.CompanionAcceleration = 0; }
        }

        public UnitState AddPresentationFixture(Vector2 position, float health)
        {
            if (!RunPaths.K2Capture && !RunPaths.K3FinalCapture && !RunPaths.QualityCapture && !RunPaths.K4Capture && !RunPaths.K4FinalCapture) throw new InvalidOperationException("Presentation fixture requires isolated capture mode");
            UnitState unit = Spawn(catalog.MeleeEnemy, Faction.Enemy, position);
            unit.Health = health;
            return unit;
        }

        public UnitState AddExpandedPresentationFixture(UnitDefinition definition, Vector2 position, float health)
        {
            if (!RunPaths.K5Capture && !(RunPaths.K7Capture && Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-k13") >= 0)) throw new InvalidOperationException("Expanded fixture requires isolated capture mode");
            UnitState enemy = Spawn(definition, Faction.Enemy, position); enemy.Health = health; return enemy;
        }

#if UNITY_EDITOR
        public void ProcessProcFixture() => ProcessProcs();
        public void LootFixture(LootKind kind, Vector2 position, int value) => DropLoot(kind, position, value);
        public UnitState AddFixture(UnitDefinition definition, Faction faction, Vector2 position) => Spawn(definition, faction, position);
        public void DamageFixture(UnitState target, float damage) => ApplyDamage(target, damage, Faction.Enemy, injectedDamage: true);
        public void WeaponDamageFixture(UnitState target, float damage, DamageElement element) => ApplyWeaponDamage(target, damage, Faction.Companion, element);
        public void ProjectileFixture(Vector2 position, Vector2 velocity, float damage, Faction faction, DamageElement element = DamageElement.Physical)
        {
            projectiles.Add(new ProjectileState { Id = ++nextId, Faction = faction, Position = position, PreviousPosition = position, Velocity = velocity, Damage = damage, Remaining = 2f, Color = Color.white, Element = element });
        }
#endif
    }
}
