using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public enum RunPhase { Preparing, Combat, Paused, Victory, Defeat, Rest }
    public enum CompanionTravelMode { Patrol, Pursuit }
    public enum CombatEventKind { Windup, Fire, Hit, Kill, Pickup, Heal, ChestOpened, Proc, SignatureMark, SignaturePulse, SignatureBurst, Support, Dodge, BossLanded, ProjectileReturn, ProjectilePierce, ProjectileSplit, ProjectileBounce, Knockback, LootDrop, BossPhase, ContactWindup, StatusApplied, LootAttract, MimicWake, BuildMorphCast, HeroIdentityCast, Detonation, Flare, Bell, BodyLaunched, BodyImpact, WallSplat, Strike, ChainBlast }

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
        public int PackCoinBonus; // escorts carry the rewards of the small bodies removed from their pack
        /// <summary>While a hero is transformed: the painted art key of its transformed weapon (presentation only).</summary>
        public string AwakenArt;
        /// <summary>Hero-kit conditions on an enemy: slowed, shocked, soaked or cracked until these times; struck by a friend yet.</summary>
        public float KitSlowUntil, KitShockedUntil, KitWetUntil, KitCrackedUntil;
        /// <summary>Mythic companion skills (CompanionSkills): asleep or frozen, hastened, anchored until; sprung by Coil Kettle yet.</summary>
        public float SleepUntil, HasteUntil, AnchorUntil, FreezeReadyAt;
        /// <summary>Relic item state: Crimson Seal's burst, Crimson Spring's next double attack, Living Thread's pull.</summary>
        public float SealAt = -1, SealDamage, SpringAt, ThreadAt;
        /// <summary>New items: frozen (Freeze), Cuckoo Pendulum's called crit, Wind Chime's piercing shots, Duelist's Ribbon count,
        /// Funhouse Mirror's warp.</summary>
        public float FrozenUntil, FunhouseUntil; public bool CuckooReady; public int ChimeShots, DuelHits;
        /// <summary>Sonar-probed by a Sonar Harpoon or its buoy (Bell Diver's marks); Juggling Pins' last target.</summary>
        public float ProbedUntil; public int ArsenalLastTarget;
        /// <summary>The new friends: Ringmaster Jack's taunt, Sundae Golem's cream, the pin and fossil stun, Court Herald's fanfare.</summary>
        public float TauntUntil, CreamTime, CreamSeen, PinReadyAt; public int TauntBy, SummonedBy; public bool HeraldShot;
        /// <summary>Shop visitors: lured to a sweet; a Gatekeeper's elite.</summary>
        public float LureUntil; public Vector2 LurePoint; public bool Elite;
        public bool CoilStruck;
        public float KitSlow;
        public bool KitStruck;
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
        /// <summary>Presentation only: when a hero skill last struck this unit, and the hero's colour it flashes.</summary>
        public float SkillHitAt = -100f; public Color SkillHitTint;
        public float NextAttack;
        public float ImpactTime;
        public float NextContact;
        public float ContactImpactAt;
        public bool PendingAttack;
        public bool CriticalEncore; // Billy: a friend's queued response to the hero's critical attack
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
        public int TacticalTargetId;
        public float TacticalRetargetAt;
        public int MeleeApproachTarget;
        public float MeleeApproachAngle;
        public int BossVolley;
        public float BossRecoveryUntil;
        public int VisibleMarks;
        public float MarkVisibleUntil;
        public float ExposedUntil;
        public float CameraWeakUntil;
        public int WeaknessSourceId;
        public int ExpandedBeat;
        public int ArsenalBeat;              // Bizarre Arsenal combo / plant counter
        public float ArsenalHeat;            // Gumball Gatling barrel heat (0..1)
        public float ArsenalLastShot = -100;
        public WeaponDefinition Borrowed;    // Glitch Toybox: the weapon this attack borrowed
        public WeaponDefinition BorrowNext;  // Glitch Toybox: the next pick, shown above the box
        public int MorphAttacks;
        public float NextMorph;
        public float LastMorphAt = -100;
        public float AttackWindup;
        public float BehaviorUntil;
        public float NextBehavior;
        public bool Awakened;
        public float NextThermal;
        public float RelicPoisonUntil;
        public float RelicPoisonAt;
        public Vector2 AbilityOrigin;
        public Vector2 AbilityTarget;
        public float BodyLift;
        public int StolenCoins;
        /// <summary>Enemy variety (EnemyVariety): the summoner that released it, a splitter's half (it does not split again) or
        /// the second of a tethered pair, its tethered partner, when that partner stands back up, and where a sniper aimed.</summary>
        public int TetherPartner; public bool IsHalf; public float TetherReviveAt; public Vector2 SniperAim; // (SummonedBy above also marks a summoner's Clip Mites)
        public float AbilityFireAt;
        public float LeftPartDamage;
        public float RightPartDamage;
        public float LeftPartBrokenAt = -100;
        public float RightPartBrokenAt = -100;
        public float CenterPartDamage;
        public float CenterPartBrokenAt = -100;
        public float KneelUntil = -1;   // THE KEEPER, WHOLE: all three parts broken, it kneels and takes the brigade's full weight
        public int ActiveBossOrgan = -1;
        public float BossOrganReleaseAt = -100;
        public float LastBossVacuumTime;
        public float BossPoseTime;
        public int ActiveBossSignature = -1;
        public bool ActiveTrademark; // this wind-up ends in the boss's own move (BossTrademarks)
        // the weight of the last blow (ImpactRules): 0 light .. 1 heavy; heavy blows stagger, edges splat, kills may fling
        public float HitWeight, StaggerUntil, WallSplatAt = -10, PinnedUntil, BumpAt;
        // N2 knock-up (AirborneRules): up at AirborneAt, down at AirborneUntil, landing for AirborneDamage
        public float AirborneAt, AirborneUntil, AirborneDamage;
        public Faction AirborneSource;
        public CombatCause AirborneCause;
        /// <summary>Enemy roster v2 (WaitingEnemies): its own move state, or just a PAID stamp on any enemy.</summary>
        public WaitingEnemyState Waiting;
        public bool Flung;
        public float BossSkillStarted = -100;
        public float BossSkillUntil = -100;
        public float NextEvolutionAttack = -1;
        public float EvolutionReleaseAt;
        public float EvolutionRecoveryUntil;
        public float EvolutionTravelStarted = -100;
        public float EvolutionTravelUntil = -100;
        public Vector2 EvolutionOrigin;
        public Vector2 EvolutionTarget;
        public int EvolutionRepeats;
        public int EvolutionCasts;
        public bool SwarmMinion;
        /// <summary>Came out of a Waiting Room door: drops no gold (requirement 23/10/2026).</summary>
        public bool DoorPack;
        public SwarmRole Role;          // ink bloater / fuse runner
        public float FuseAt;            // a lit fuse runner blows at this time
        public Vector2 SurgeDirection;  // a surge body marches this way across the screen
        public EnemyVariantState Variant;
        public float OctoberLungeAt, OctoberLungeUntil;
        public float EncoreWindupUntil, EncoreRecoveryUntil;
        public Vector2 OctoberLungeFrom, OctoberLungeTo;
        public int MountId, RiderId, OctoberAttacks, OctoberKills;
        public float OctoberNext;
        public bool LeftPartBroken => LeftPartBrokenAt >= 0;
        public bool RightPartBroken => RightPartBrokenAt >= 0;
    }

    public sealed class ProjectileState
    {
        public int Id;
        /// <summary>Mirror Newt has already tried to turn this hostile shot back.</summary>
        public bool MirrorChecked;
        /// <summary>Lantern Weaver's pinned-enemy pierce has been granted to this shot.</summary>
        public bool PinnedPierced;
        /// <summary>Bell Diver's probed-enemy bounce has been granted to this shot.</summary>
        public bool ProbedBounced;
        /// <summary>Kite Tail Ribbon: extra bounces of a critical friendly shot.</summary>
        public int RelicBounces;
        /// <summary>The visitor's umbrella has already had its chance at this shot.</summary>
        public bool UmbrellaChecked;
        public Faction Faction;
        public Vector2 Position;
        public Vector2 PreviousPosition;
        public Vector2 Velocity;
        public float Damage;
        public float Remaining;
        /// <summary>Presentation only: where a weapon's shot left its muzzle and when (0 = not a weapon shot). A shot is never
        /// drawn behind its muzzle, and its first frames leave the barrel as it is drawn.</summary>
        public Vector2 Muzzle; public float FiredAt = -1;
        // Negative means a staged/hostile shot owns its flight. Ordinary friendly shots consume actual distance,
        // so acceleration, splitting and ricochets cannot silently extend the compiled weapon reach.
        public float TravelLeft = -1;
        public Color Color;
        /// <summary>Optional tint for the body (alpha 0 = none, every other shot unchanged); Drowsy Seer's dream shots fade like the dreams.</summary>
        public Color Tint;
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
        /// <summary>An empowered shot bursts where it lands: half its damage to the enemies within this radius (0: none).</summary>
        public float Blast;
        public float Radius = 0.055f;
        public int SplitCount;
        public int RandomBounces = 8;
        public HostileShot HostileMotion;
        public float HostileAge;
        public int OrbitTargetId;
        public int IdentityTargetId;
        public Vector2 OrbitCenter;
        public Vector2 LaunchVelocity;
        public readonly HashSet<int> Victims = new HashSet<int>();
        public HashSet<int> Volley;       // shared by the shots of one fan: each foe is struck once per volley
        public int ExtraPierce;           // foes passed through beyond the weapon's own pierce
        public bool SignatureShot;        // a bell chime or umbrella drop carrying the signature payoff
        public ArsenalWeapon Arsenal;     // a Bizarre Arsenal shot (racket ball, claw, drill, shark...)
        public int ArsenalStage;          // its phase: out, hover, return...
        public float ArsenalTimer;
        public int ArsenalHops;
        public Vector2 ArsenalHome;
        public float ArsenalTick;
        public ShotPattern Pattern;       // how it flies (ShotPatterns): wave, curve, helix, orbit...
        public float PatternAge, PatternSpeed; public Vector2 PatternAxis; public int PatternSide;
        public Vector2 PatternOffset;     // where a swaying pattern draws the shot, off its true (hit-tested) line
    }

    public sealed class PickupState
    {
        public int Id;
        public Vector2 Position;
        public Vector2 PreviousPosition;
        public Vector2 Velocity;
        public float SpawnTime;
        public bool Attracting;
        public bool Vacuumed;   // called by the shopkeeper's bell: comes running from anywhere
        public int Value;
        public LootKind Kind;
    }

    public sealed partial class CombatSimulation
    {
        private readonly CheckpointCatalog catalog;
        private readonly List<UnitState> units = new List<UnitState>();
        private readonly Dictionary<int, UnitState> unitLookup = new Dictionary<int, UnitState>();
        private readonly HashSet<int> bossDamageSources = new HashSet<int>();
        private readonly List<ProjectileState> projectiles = new List<ProjectileState>();
        private readonly List<PickupState> pickups = new List<PickupState>();
        private readonly List<CombatEvent> events = new List<CombatEvent>();
        private readonly SpatialUnitIndex spatial = new SpatialUnitIndex();
        private readonly List<UnitState> targetCandidates = new List<UnitState>();
        private readonly List<UnitState> projectileCandidates = new List<UnitState>();
        private readonly List<UnitState> separationCandidates = new List<UnitState>();
        private float maximumRadius;
        private float coinCredit;
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
        private float finaleElapsed;
        public IReadOnlyList<UnitState> Units => units;
        public UnitState UnitForPresentation(int id) => UnitWithId(id);
        public IReadOnlyList<ProjectileState> Projectiles => projectiles;
        public IReadOnlyList<PickupState> Pickups => pickups;
        public IReadOnlyList<CombatEvent> Events => events;
        /// <summary>Enemy species defeated in this fight (Specimen Pin keeps them for the run).</summary>
        public readonly HashSet<string> KilledSpecies = new HashSet<string>();
        public RunPhase Phase { get; private set; } = RunPhase.Preparing;
        public UnitState Champion { get; private set; }
        public float Elapsed { get; private set; }
        public float VisualElapsed => Elapsed + restElapsed;
        public float WaveDuration => catalog.WaveDuration;
        public int Gold { get; private set; }
        public int Eggs { get; private set; }
        public int ChestsOpened { get; private set; }
        public const int EliteEggsPerWave = 2;
        private int eliteEggs;
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
            unitLookup.Clear();
            bossDamageSources.Clear();
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
            swarmSpawns = swarmGoldKills = 0;
            pendingKillCoinHundredths = 0; pendingKillCoinKills = 0; pendingKillCoinPosition = Vector2.zero;
            Elapsed = 0f;
            restElapsed = 0f;
            finaleElapsed = 0;
            Gold = 0; coinCredit = 0;
            Eggs = ChestsOpened = 0;
            Kills = 0;
            CompanionCount = 0;
            BossDefeated = false;
            Boss = null;
            LastDamageSource = "";
            LastDamageAmount = 0f;
            LastDamageKind = "";
            maximumRadius = maximumSpeed = 0f; bossBodyReach = 0f;
            Generation++;
            Champion = Spawn(catalog.Champion, Faction.Champion, Vector2.zero);
            if (!catalog.ExplicitStartingRoster && (catalog.StartingCompanions == null || catalog.StartingCompanions.Length == 0))
                AddCompanion(catalog.Companion);
            else if (catalog.StartingCompanions != null)
                for (int index = 0; index < catalog.StartingCompanions.Length; index++) AddCompanion(catalog.StartingCompanions[index]);
            if (catalog.StoryCompanions != null) foreach (UnitDefinition story in catalog.StoryCompanions) AddCompanion(story);
            if (catalog.Boss != null)
            {
                Boss = Spawn(catalog.Boss, Faction.Enemy, ClampPosition(Champion.Position + Vector2.up * (catalog.Boss.Id == "OutsideStageKeeper" ? 2f : 2.8f), catalog.Boss.Radius));
                Boss.EntranceUntil = 1.2f;
                Boss.NextContact = Boss.NextAttack = 1.45f;
                Boss.NextBossPattern = 2.2f;
            }
            spatial.Rebuild(units);
            Phase = RunPhase.Combat;
            ResetArenaToys(catalog.RunWave);
            StartSecretCombat();
            ResetExpansion();
            ResetOutsideSummons();
            ResetRelics(); ResetVisitors();
            ResetOctoberCombat();
            ResetHeroIdentity();
            ResetSwarmPacing();
        }

        public float KnockRemaining => catalog.KnockDeadline > 0 ? Mathf.Max(0, catalog.KnockDeadline - Elapsed) : -1;

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
            ClearHeroIdentity();
            ClearEnemyVariants();
            for (int index = units.Count - 1; index >= 0; index--)
                if (units[index].Faction == Faction.Enemy) { unitLookup.Remove(units[index].Id); units.RemoveAt(index); }
            projectiles.Clear();
            pickups.Clear();
            events.Clear();
            Boss = null;
            restElapsed = 0f;
            ClearSignatures();
            ClearArenaToys();
            ClearExpansion();
            foreach (UnitState unit in units) { unit.PendingAttack = unit.CriticalEncore = false; unit.AttackStarted = -100f; unit.BurnUntil = unit.ChillUntil = 0f; }
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
            Vector2 current = Champion.Position;
            Vector2 bounded = RestrictToShop(current);
            Champion.Position = (current - bounded).sqrMagnitude > 0.0001f ? Vector2.MoveTowards(current, bounded, Mathf.Max(8, Champion.Definition.Speed * 2) * deltaTime) : RestrictToShop(current + movement * Champion.Definition.Speed * deltaTime);
            if (movement.sqrMagnitude > 0.001f) Champion.Facing = movement.normalized;
            foreach (UnitState unit in units)
            {
                if (unit.Faction != Faction.Companion) continue;
                if (FollowOctoberMount(unit)) continue;
                unit.Position = ClampPosition(CompanionMotion.Step(unit, Champion, units, null, catalog, Elapsed + restElapsed, deltaTime), unit.Definition.Radius);
                unit.Position = RestrictToShop(Champion.Position + Vector2.ClampMagnitude(unit.Position - Champion.Position, CompanionMotion.Leash(unit, catalog)));
            }
        }
        private Vector2 shopHalfExtents = Vector2.positiveInfinity;
        public void SetShopBounds(Vector2 halfExtents) { shopHalfExtents = Vector2.Max(Vector2.one, halfExtents); }
        public Vector2 RestrictToShop(Vector2 position)
        {
            Vector2 half = Vector2.Min(shopHalfExtents, catalog.ArenaSize * 0.5f - Vector2.one * Champion.Definition.Radius);
            return new Vector2(Mathf.Clamp(position.x, -half.x, half.x), Mathf.Clamp(position.y, -half.y, half.y));
        }

        public Vector2 ClampPosition(Vector2 position, float radius)
        {
            Vector2 half = catalog.ArenaSize * 0.5f - Vector2.one * radius;
            return new Vector2(Mathf.Clamp(position.x, -half.x, half.x), Mathf.Clamp(position.y, -half.y, half.y));
        }

        public void StepFinale(float deltaTime, Vector2 movement)
        {
            if (Phase != RunPhase.Victory || !Champion.Alive || !float.IsFinite(deltaTime) || deltaTime <= 0 || deltaTime > 0.1f) return;
            if (!float.IsFinite(movement.x) || !float.IsFinite(movement.y)) movement = Vector2.zero;
            finaleElapsed += deltaTime;
            foreach (UnitState unit in units) unit.PreviousPosition = unit.Position;
            movement = Vector2.ClampMagnitude(movement, 1);
            if (Champion.Definition.HeroMechanic == HeroMechanic.RootedOracle && !HeroIdentityEnabled) movement = Vector2.zero; // with identities on, Root Oracle walks (its identity roots it)
            Champion.Position = ClampPosition(Champion.Position + movement * Champion.Definition.Speed * KitHeroSpeed() * VisitorHeroScale * deltaTime, Champion.Definition.Radius);
            if (movement.sqrMagnitude > 0.001f) Champion.Facing = movement.normalized;
            foreach (UnitState unit in units)
                if (unit.Alive && unit.Faction == Faction.Companion)
                {
                    unit.Position = ClampPosition(CompanionMotion.Step(unit, Champion, units, null, catalog, Elapsed + finaleElapsed, deltaTime), unit.Definition.Radius);
                    unit.Position = Champion.Position + Vector2.ClampMagnitude(unit.Position - Champion.Position, CompanionMotion.Leash(unit, catalog));
                }
        }

        public void Step(float deltaTime, Vector2 movement)
        {
            events.Clear();
            if (Phase != RunPhase.Combat || !float.IsFinite(deltaTime) || deltaTime <= 0f || deltaTime > 0.1f) return;
            if (!float.IsFinite(movement.x) || !float.IsFinite(movement.y)) movement = Vector2.zero;
            movement = Vector2.ClampMagnitude(movement, 1f);
            Elapsed += deltaTime;
            if (catalog.KnockDeadline > 0)
            {
                // SIT CLOSER: the door must be answered before the knocking stops
                if (BossDefeated) { Phase = RunPhase.Victory; return; }
                if (Elapsed >= catalog.KnockDeadline)
                {
                    LastDamageSource = "The knocking stopped"; LastDamageKind = "story"; LastDamageAmount = Champion.Health;
                    Champion.Health = 0; Champion.Alive = false; Phase = RunPhase.Defeat; return;
                }
            }
            if (Champion.Definition.HeroMechanic == HeroMechanic.RootedOracle && !HeroIdentityEnabled) movement = Vector2.zero;
            StepRelics(deltaTime, movement);
            if (Phase != RunPhase.Combat) return;
            for (int index = 0; index < units.Count; index++) units[index].PreviousPosition = units[index].Position;
            Champion.Position = ClampPosition(Champion.Position + movement * Champion.Definition.Speed * KitHeroSpeed() * VisitorHeroScale * WaitingHeroScale * MapHeroScale * deltaTime, Champion.Definition.Radius);
            float beforeRegeneration = Champion.Health;
            Champion.Health = Mathf.Min(Champion.Definition.Health, Champion.Health + Mathf.Max(0,catalog.PassiveHeal) * RelicMendingScale * deltaTime);
            if (Champion.Health > beforeRegeneration) RecordHealing(NewCause(Champion.Id).Child("passive-regen"), Champion.Health - beforeRegeneration);
            if (movement.sqrMagnitude > 0.001f) Champion.Facing = movement.normalized;
            SpawnWaveEnemies(); ProcessElites(); ProcessVariety();
            spatial.Rebuild(units);

            for (int index = 0; index < units.Count && Phase == RunPhase.Combat; index++)
            {
                UnitState unit = units[index];
                if (!unit.Alive) continue;
                if (unit.EntranceUntil > Elapsed) continue;
                if (unit.EntranceUntil > 0) events.Add(new CombatEvent(CombatEventKind.BossLanded, unit.Position, unit.Faction, subjectId: unit.Id, subject: unit));
                unit.EntranceUntil = 0;
                if (unit.Faction == Faction.Companion) FollowChampion(unit, deltaTime);
                else if (unit.Faction == Faction.Enemy)
                {
                    MoveEnemy(unit, deltaTime);
                    MapMotion(unit); // the map's living elements: dark, chimes, moths, chairs (play-test item 24)
                    SeparateFromHeavyEnemy(unit, deltaTime);
                    unit.Velocity = (unit.Position - unit.PreviousPosition) / deltaTime;
                }
                spatial.Move(unit);
                ProcessStatusAndPassive(unit);
                if (!unit.Alive) continue;
                if (unit.Definition.IsBoss) ProcessBoss(unit);
                if(unit.PendingAttack && unit.Definition.Weapon.Kind==WeaponKind.Melee)MeleeTactics.Aim(unit,UnitWithId(unit.AttackTargetId),deltaTime);
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
                    ProcessSpectacle();
                    ProcessSignatureRelics();
                    ProcessSignatures();
                    ProcessArenaToys(deltaTime);
                    ProcessExpansion(deltaTime);
                    StepFlung(deltaTime);
                    StepAirborne();
                    StepPufferBursts();
                    ProcessWaitingEnemies(deltaTime);
                    ProcessMapEnvironment(deltaTime);
                    ProcessSwarmPacing(deltaTime);
                    ProcessBuildMorphs();
                    ProcessOctoberShots();
                    ProcessRelicEchoes();
                    ProcessProcs();
                    ProcessHeroIdentity(deltaTime, movement);
                    ProcessCompanionSkills();
                    ProcessVisitors(deltaTime);
                    ProcessOctoberCombat(deltaTime, movement);
                }
                ProcessSecretEchoes();
                if (Phase == RunPhase.Combat && SecretVictoryReady()) Phase = RunPhase.Victory;
                // the whole Keeper's test has an end: three minutes without falling and it sets the traveller down, gift unkept
                if (Phase == RunPhase.Combat && catalog.WholeKeeper && Elapsed >= KeeperTestSeconds && Champion.Alive) { KeeperWithdrew = true; Phase = RunPhase.Victory; }
            }
            for (int index = units.Count - 1; index >= 0; index--)
                if (!units[index].Alive && units[index].Faction != Faction.Champion) { unitLookup.Remove(units[index].Id); units.RemoveAt(index); }
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
            if (faction == Faction.Enemy && !definition.IsBoss) unit.Variant = NewEnemyVariant(unit);
            units.Add(unit);
            unitLookup.Add(unit.Id, unit);
            if (faction == Faction.Enemy && definition.IsBoss) bossDamageSources.Add(unit.Id);
            maximumRadius = Mathf.Max(maximumRadius, definition.Radius); NoteBossBody(definition);
            maximumSpeed = Mathf.Max(maximumSpeed, definition.Speed);
            if (faction == Faction.Companion) CompanionCount++;
            return unit;
        }

        private void SpawnWaveEnemies()
        {
            bool overtime = BossOvertimeSwarm;
            if (overtime && (catalog.BossSwarmBudget <= 0 || Boss == null || !Boss.Alive)) return;
            if (Elapsed < nextSpawn) return;
            nextSpawn = Elapsed + Mathf.Max(0.12f, catalog.SpawnInterval * VisitorSpawnScale * WaveCadence.Envelope(catalog.RunWave, Elapsed, catalog.WaveDuration) * WaveCadence.SwarmInterval(catalog.RunWave, Elapsed)) * (overtime ? 2.2f : 1f);
            int enemyCount = 0;
            for (int index = 0; index < units.Count; index++) if (units[index].Faction == Faction.Enemy && units[index].Alive) enemyCount++;
            int budget = overtime ? catalog.BossSwarmBudget : catalog.EnemyBudget;
            if (enemyCount >= budget) return;
            int batch = catalog.ExpandedContent ? WaveCadence.SwarmBatch(catalog.RunWave, Elapsed) : 1;
            int originalSmall = Mathf.Max(0, batch - 1);
            bool mixedPack = catalog.ExpandedContent && catalog.RunWave >= 10 && !overtime;
            int leaders = mixedPack ? 2 : 1;
            if (mixedPack) batch = leaders + Mathf.CeilToInt(Mathf.Max(0, batch - 1) * .5f);
            if (overtime) batch = Mathf.Min(batch, 4);
            Vector2 packAnchor = Vector2.zero;
            UnitDefinition herd = catalog.ExpandedContent ? HerdDefinition() : catalog.MeleeEnemy;
            bool packStarted = false;
            for (int member = 0; member < batch && enemyCount + member < budget; member++)
            {
                // bodies arrive just past the edge of the screen from the first wave on, so the hall is never empty
                Vector2 half = catalog.ArenaSize * 0.5f - Vector2.one * 0.4f;
                half = Vector2.Min(half, new Vector2(9, 5.6f));
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
                    position = ClampPosition(position + Champion.Position, 0.6f);
                    if (packStarted)
                    {
                        Vector2 outward = (packAnchor - Champion.Position).normalized;
                        Vector2 tangent = new Vector2(-outward.y, outward.x);
                        position = ClampPosition(packAnchor + tangent * ((member - 1) % 6 - 2.5f) * 0.48f + outward * ((member - 1) / 6) * 0.5f, 0.6f);
                    }
                    if ((position - Champion.Position).sqrMagnitude >= catalog.SpawnSafetyRadius * catalog.SpawnSafetyRadius) { found = true; break; }
                }
                if (found)
                {
                    if (!packStarted) { packAnchor = position; packStarted = true; }
                    UnitDefinition definition = catalog.ExpandedContent && catalog.EnemyPool.Length > 0 ? catalog.EnemyPool[spawnRandom.Next(catalog.EnemyPool.Length)] : spawnRandom.Next(3) == 0 ? catalog.RangedEnemy : catalog.MeleeEnemy;
                    if (mixedPack && member < leaders) definition = PackEscortDefinition(herd);
                    if (!mixedPack && catalog.RunWave >= 5 && member == 0 && swarmSpawns % 9 == 0)
                    {
                        UnitDefinition support = System.Array.Find(catalog.EnemyPool, enemy => enemy.Weapon.Kind == WeaponKind.Projectile);
                        if (support != null) definition = support;
                    }
                    if (!mixedPack && catalog.RunWave >= 5 && member == 0 && swarmSpawns % 13 == 0)
                    {
                        UnitDefinition shield = System.Array.Find(catalog.EnemyPool, enemy => enemy.Behavior == EnemyBehavior.Door);
                        if (shield != null) definition = shield;
                    }
                    bool minion = catalog.ExpandedContent && (member >= leaders || overtime);
                    if (!minion && definition.SpecialEnemy != SpecialEnemyKind.None && ActiveSpecialists() >= 3) definition = SwarmDefinition();
                    if (!minion && WaitingSpawnBlocked(definition)) definition = SwarmDefinition();
                    if (minion) definition = herd;
                    UnitState spawned = Spawn(definition, Faction.Enemy, position);
                    spawned.SwarmMinion = minion;
                    if (mixedPack && !minion) spawned.PackCoinBonus = EscortCoinBonus(originalSmall, batch - leaders, 1, leaders, member);
                    if (minion) spawned.Health *= 0.45f;
                    else InitializeEnemyVariant(spawned);
                    AssignSwarmRole(spawned, minion);
                    swarmSpawns++;
                }
            }
        }

        private void FollowChampion(UnitState unit, float deltaTime)
        {
            if (FollowOctoberMount(unit)) return;
            if (FollowIdentityConvoy(unit, deltaTime)) return;
            float leash = CompanionMotion.Leash(unit, catalog);
            UnitState target = SelectCompanionTarget(unit, unit.Definition.Weapon.Kind == WeaponKind.Melee ? leash + unit.Definition.Weapon.Range * 0.65f : unit.Definition.Weapon.Range);
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
            if (MoveOctoberBoss(unit)) return;
            if (unit.Definition.Id == "OutsideStageKeeper") return;
            if (!unit.Definition.IsBoss && MoveSwarmRole(unit, deltaTime)) return;
            if (!unit.Definition.IsBoss && MoveEnemyVariant(unit)) return;
            if (!unit.Definition.IsBoss && MoveWaitingEnemy(unit, deltaTime)) return;
            if (unit.Definition.IsBoss && MoveBossSignature(unit)) return;
            if (!unit.Definition.IsBoss && MoveMutatedEnemy(unit)) return;
            if (MoveExpandedEnemy(unit, deltaTime)) return;
            if (unit.TelegraphUntil > Elapsed) return;
            if (unit.Definition.IsBoss && unit.BossRecoveryUntil > Elapsed) return;
            Vector2 direction = OctoberEnemyTarget(unit) - unit.Position;
            float distance = direction.magnitude;
            unit.Facing = distance > 0.001f ? direction / distance : unit.Facing;
            Vector2 travelDirection = unit.Facing;
            if (catalog.RunWave >= 5 && !unit.Definition.IsBoss && unit.Definition.Weapon.Kind == WeaponKind.Contact && unit.Definition.Behavior == EnemyBehavior.Standard && distance > 2.2f && unit.Id % 3 == 1)
            {
                Vector2 tangent = new Vector2(-unit.Facing.y, unit.Facing.x) * (unit.Id % 2 == 0 ? 1 : -1);
                travelDirection = (unit.Facing + tangent * 0.45f).normalized;
            }
            bool ranged = unit.Definition.Weapon.Kind == WeaponKind.Projectile;
            float stopDistance = ranged ? 4.8f : unit.Definition.Radius + Champion.Definition.Radius;
            float speed = unit.Definition.Speed * (unit.ChillUntil > Elapsed ? 0.55f : 1f) * (unit.StaggerUntil > Elapsed ? ImpactRules.StaggerSpeed : 1f) * (unit.PinnedUntil > Elapsed || unit.AirborneDamage > 0 ? 0f : 1f) * (unit.Definition.IsBoss ? BossPartRules.MovementMultiplier(unit) : 1) * KitEnemySpeed(unit) * EnemySleepScale(unit) * VisitorEnemyScale(unit);
            if (distance > stopDistance)
                unit.Position = ClampPosition(unit.Position + travelDirection * Mathf.Min(speed * deltaTime, distance - stopDistance), unit.Definition.Radius);
        }

        private void ProcessStatusAndPassive(UnitState unit)
        {
            if (unit.BurnUntil > Elapsed && Elapsed >= unit.NextBurn)
            {
                unit.NextBurn = Elapsed + 0.5f;
                ApplyDamage(unit, (unit.Definition.IsBoss ? 3f : 5f) * RelicLateFee, Faction.Companion, cause: unit.BurnCause);
            }
            if (unit.Faction == Faction.Enemy || Elapsed < unit.NextPassive) return;
            unit.NextPassive = Elapsed + 4f;
            switch (unit.Definition.Passive)
            {
                case PassiveKind.HealChampion:
                    float healthBefore = Champion.Health;
                    Heal(unit.Definition.PassiveStrength * 3f, NewCause(unit.Id).Child("unit-passive"));
                    if (Champion.Health > healthBefore)
                    {
                        CombatCause healingCause = NewCause(unit.Id);
                        // Heal() already recorded the effective restoration.
                        // One healing event per restoration.
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
            ProcessOctoberBoss(boss);
            if (ProcessLastCustomer(boss)) return;
            if (ProcessSecretBoss(boss)) return;
            if (ProcessOutsideStage(boss)) return;
            if (catalog.RunWave == 1) return;
            boss.BossPoseTime = Elapsed;
            ProcessBossVacuum(boss);
            if (boss.TelegraphUntil > 0f && Elapsed >= boss.TelegraphUntil)
            {
                float launchTime = boss.TelegraphUntil;
                boss.TelegraphUntil = 0f;
                int pattern = boss.Definition.BossPattern;
                int count = BossPartRules.BulletCount(boss);
                bool signature = catalog.ExpandedContent && boss.ActiveBossSignature >= 0;
                if (signature) { ResolveBossSignature(boss); count = 0; }
                else if (boss.ActiveTrademark) { boss.ActiveTrademark = false; ResolveBossTrademark(boss); count = 0; }
                else if (catalog.ExpandedContent && ResolveBossBodyAttack(boss)) count = 0;
                for (int index = 0; index < count && projectiles.Count < 4096; index++)
                {
                    float angle = BossPartRules.VolleyAngle(boss, index, count, launchTime);
                    Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    float speed = catalog.RunWave > 0 ? pattern == 2 ? 1.8f + index % 4 * 0.2f : pattern == 4 ? 2f + index % 2 * 0.4f : 2.4f : pattern == 3 ? 7f : pattern == 2 ? 3.5f + index % 4 * 0.7f : pattern == 4 ? 3.5f + index % 2 * 1.8f : 4.2f;
                    Vector2 origin = ClampPosition(BossPartRules.VolleyOrigin(boss), 0.1f);
                    projectiles.Add(new ProjectileState { Id = ++nextId, Faction = Faction.Enemy, Position = origin, PreviousPosition = origin, Velocity = direction * speed, LaunchVelocity = direction * speed, HostileMotion = catalog.RunWave > 0 ? boss.Definition.Shot : HostileShot.Straight, Damage = boss.Definition.Weapon.Damage, Remaining = 7f, VisualWidth = 0.34f, Radius = 0.1f, Color = new Color(1f, 0.38f, 0.3f), SourceName = boss.Definition.DisplayName + " pattern", SourceUnitId = boss.Id });
                }
                boss.BossVolley++;
                boss.BossOrganReleaseAt = Elapsed;
                boss.AttackWindup = BossEncounterRules.Windup(boss); boss.AttackStarted = Elapsed - boss.AttackWindup;
                bool enraged = boss.Health < boss.Definition.Health * 0.5f;
                float recovery = signature ? 1.8f : boss.BossVolley % 3 == 0 ? 2.2f : 0.55f;
                boss.BossRecoveryUntil = Elapsed + recovery;
                boss.NextBossPattern = Elapsed + (enraged ? 2.7f : 3.5f) + (boss.BossVolley % 3 == 0 ? 0.8f : 0) + BossPartRules.CycleAdjustment(boss);
                events.Add(new CombatEvent(CombatEventKind.Fire, boss.Position, Faction.Enemy));
            }
            if (Elapsed < boss.NextBossPattern || boss.TelegraphUntil > 0f) return;
            boss.ActiveBossSignature = catalog.ExpandedContent && BossEncounterRules.IsSignature(boss.BossVolley) ? (boss.BossVolley / 4) % 2 : -1;
            boss.ActiveTrademark = catalog.ExpandedContent && boss.ActiveBossSignature < 0 && BossTrademarks.Fires(boss.Definition, boss.BossVolley);
            boss.TelegraphUntil = Elapsed + BossEncounterRules.Windup(boss);
            boss.ActiveBossOrgan = BossPartRules.NextOrgan(boss);
            boss.TelegraphDirection = (Champion.Position - boss.Position).normalized;
            boss.AbilityTarget = ClampPosition(Champion.Position, boss.Definition.Radius + 0.4f);
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

        private UnitState FindTarget(UnitState source, float range, bool weaponReach=false)
        {
            bool melee=weaponReach && source.Definition.Weapon.Kind==WeaponKind.Melee;
            if (source.Faction == Faction.Enemy)
                return range >= 0 && Champion != null && CanHit(source.Faction, Champion) && (Champion.Position - source.Position).sqrMagnitude <= Mathf.Pow(melee?MeleeTactics.Reach(source,Champion):range,2) ? Champion : null;
            UnitState best = null;
            float bestDistance = float.PositiveInfinity;
            float search=melee?Mathf.Max(range+HitPadding,source.Definition.Radius+HitPadding+.08f):range;
            spatial.Query(source.Position - Vector2.one * search, source.Position + Vector2.one * search, targetCandidates);
            for (int index = 0; index < targetCandidates.Count; index++)
            {
                UnitState candidate = targetCandidates[index];
                if (!CanHit(source.Faction, candidate)) continue;
                float distance = (candidate.Position - source.Position).sqrMagnitude;
                if(melee?!MeleeReaches(source,candidate):distance>range*range)continue;
                // a shooting hero takes the nearest foe, or a near and strong one (requirement 24/10/2026)
                if (!melee && source.Definition.Weapon.Kind == WeaponKind.Projectile) { float reach = Mathf.Max(0, Mathf.Sqrt(distance) - RangedThreat(candidate)); distance = reach * reach; }
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
            if (RelicBlocksAttack(unit) || OctoberBlocksAttack(unit)) { unit.PendingAttack = false; return; }
            if (unit.Variant != null && unit.Variant.Busy(Elapsed)) return;
            if (unit.Faction == Faction.Enemy && (unit.EvolutionReleaseAt > 0 || unit.EvolutionRecoveryUntil > Elapsed || unit.EvolutionTravelUntil > Elapsed)) return;
            WeaponDefinition weapon = unit.Definition.Weapon;
            if (unit.Definition.IsBoss) return;
            if (weapon.Kind == WeaponKind.Contact) return;
            if (catalog.ExpandedContent && unit.Definition.Behavior == EnemyBehavior.Mimic && (!unit.Awakened || unit.BehaviorUntil > Elapsed)) return;
            if (unit.PendingAttack && Elapsed >= unit.ImpactTime)
            {
                unit.PendingAttack = false;
                float damage = IdentityAttackDamage(unit, OctoberAttackDamage(unit, RelicAttackDamage(unit, weapon.Damage)));
                CombatCause attackCause = unit.CriticalEncore ? NewCause(unit.Id).Child("critical-encore") : NewCause(unit.Id);
                UnitState locked = UnitWithId(unit.AttackTargetId);
                bool critical = combatRandom.NextDouble() < Mathf.Clamp01(unit.Definition.CriticalChance + (locked != null && locked.CameraWeakUntil > Elapsed && unit.Faction != Faction.Enemy ? 0.15f : 0) + KitCrit(unit, locked) + RelicCrit(unit));
                if (critical) damage *= 1.5f;
                damage = ReleaseBuildMorph(unit, damage, attackCause);
                int kitFirst = projectiles.Count; // the hero kit may split or sharpen the shots this attack fires (KitEmitted)
                if (ResolveOctoberAttack(unit, damage, attackCause)) { }
                else if (ResolveExpandedAttack(unit, damage, critical, attackCause)) { }
                else if (weapon.Kind == WeaponKind.Projectile)
                {
                    EmitProjectile(unit, damage, critical, attackCause);
                }
                else if (weapon.Signature == SignatureWeapon.Stamp && unit.Faction != Faction.Enemy)
                {
                    UnitState target = UnitWithId(unit.AttackTargetId);
                    if (target != null && CanHit(unit.Faction, target) && MeleeReaches(unit, target))
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
                        if (CanHit(unit.Faction, target) && InArc(target, unit.Position, unit.AttackDirection, MeleeTactics.Reach(unit,target) - target.Definition.Radius, weapon.ConeDot))
                            ApplyWeaponDamage(target, damage, unit.Faction, weapon.Element, unit.Definition.DisplayName, unit.AttackDirection, weapon, critical, attackCause);
                    }
                }
                KitEmitted(unit, kitFirst);
                AdvanceSignatureBeat(unit, damage, attackCause);
                unit.CriticalEncore = false;
                events.Add(new CombatEvent(CombatEventKind.Fire, unit.Position, unit.Faction, 0f, unit.AttackDirection, weapon, critical, subject: unit, cause: attackCause));
            }
            if (Elapsed < unit.NextAttack || unit.PendingAttack || Phase != RunPhase.Combat) return;
            if (unit == Champion && HeroAwakenReplacesAttacks) return; // a transformed weapon fires instead of the usual one
            UnitState nearest = unit.Faction == Faction.Companion ? SelectCompanionTarget(unit, weapon.Range + (weapon.Kind == WeaponKind.Melee ? HitPadding : 0), true) : FindTarget(unit, weapon.Range,true);
            if (unit.Faction == Faction.Companion) nearest = LeadGuardTarget(unit, weapon.Range + (weapon.Kind == WeaponKind.Melee ? HitPadding : 0)) ?? nearest; // round 6: Patchwork Host's lead guards the hero
            if (nearest == null) return;
            unit.AttackDirection = (nearest.Position - unit.Position).normalized;
            unit.AttackTargetId = nearest.Id;
            unit.Facing = unit.AttackDirection;
            unit.AttackStarted = Elapsed;
            unit.AttackWindup = weapon.Windup * (weapon.Family == ExpandedWeapon.Hammer ? 1.17f : 1); // every slam alike (was: every third heavier)
            unit.ImpactTime = Elapsed + unit.AttackWindup;
            unit.NextAttack = Elapsed + Mathf.Max(.28f, weapon.Cooldown * OctoberCycle(unit) * KitCycle(unit) * CompanionHaste(unit) * RelicCycle(unit)) * (weapon.Family == ExpandedWeapon.Hammer ? 1.12f : 1);
            unit.PendingAttack = true;
            events.Add(new CombatEvent(CombatEventKind.Windup, unit.Position, unit.Faction, 0f, unit.AttackDirection, weapon, cause: NewCause(unit.Id)));
        }

        private void EmitProjectile(UnitState unit, float damage, bool critical, CombatCause cause, WeaponDefinition borrowed = null)
        {
            if (unit.Faction == Faction.Enemy && catalog.RunWave == 1) return;
            WeaponDefinition weapon = borrowed ?? unit.Definition.Weapon;
            Vector2 muzzleOffset = weapon.HasMuzzleOffset ? weapon.ProjectileMuzzleOffset : Vector2.right * weapon.MuzzleDistance;
            Vector2 rotatedMuzzle = Quaternion.Euler(0f, 0f, Mathf.Atan2(unit.AttackDirection.y, unit.AttackDirection.x) * Mathf.Rad2Deg) * muzzleOffset;
            Vector2 position = unit.Position + unit.AttackDirection * (weapon.MountDistance - weapon.RecoilDistance) + rotatedMuzzle;
            if (unit.Faction == Faction.Enemy) position = unit.Position + unit.AttackDirection * unit.Definition.Radius * 0.9f;
            int relicFirst = projectiles.Count;
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
                    Muzzle = position, FiredAt = Elapsed,
                    Velocity = direction * (unit.Faction == Faction.Enemy && catalog.RunWave > 0 ? Mathf.Min(2.4f, weapon.ProjectileSpeed) : weapon.ProjectileSpeed),
                    Damage = damage,
                    Remaining = weapon.ProjectileLifetime,
                    TravelLeft = unit.Faction == Faction.Enemy || FriendlyReach.Returns(weapon) ? -1 : FriendlyReach.Flight(weapon, rotatedMuzzle.magnitude, unit.Definition.Weapon.Range),
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
                    OrbitTargetId = unit.AttackTargetId,
                    OrbitCenter = position + direction,
                    Cause = cause,
                    Weapon = weapon,
                    RicochetsRemaining = weapon.Family == ExpandedWeapon.TetherBow ? unit.Definition.CombatTier >= 2 ? 3 : 2 : weapon.Family == ExpandedWeapon.Fish ? 1 : 0,
                    SplitOnImpact = weapon.Family == ExpandedWeapon.Tea || weapon.ProjectileSplit > 0,
                    FinalNote = weapon.Family == ExpandedWeapon.MusicBox, // every note rings the last (lighter) note
                    HostileMotion = unit.Faction == Faction.Enemy ? unit.Definition.Shot : HostileShot.Straight,
                    Pattern = unit.Faction == Faction.Enemy ? ShotPattern.None : ShotPatterns.For(weapon),
                    LaunchVelocity = direction * Mathf.Min(2.4f, weapon.ProjectileSpeed)
                });
            }
            RelicEmitted(unit, relicFirst);
        }

        private void ProcessContact(UnitState enemy)
        {
            if (enemy.Variant != null && (enemy.Variant.ReleaseAt > 0 || enemy.Variant.RecoveryUntil > Elapsed || enemy.Variant.TravelEnd > Elapsed)) return;
            if (enemy.EvolutionReleaseAt > 0) return;
            if (enemy.Definition.Mutation == EnemyMutation.BellEruption && enemy.EvolutionTravelUntil > Elapsed && enemy.BodyLift < -0.1f) return;
            if (!enemy.Alive || !Champion.Alive) return;
            if (enemy.BodyLift > 0.4f) { enemy.ContactImpactAt = 0; return; }
            if (catalog.ExpandedContent && enemy.Definition.Behavior == EnemyBehavior.Mimic && (!enemy.Awakened || enemy.BehaviorUntil > Elapsed)) return;
            float radius = enemy.Definition.Radius + Champion.Definition.Radius + 0.05f;
            // a boss hurts wherever its painted body is touched (play-test item 22): an ellipse ~85% of its ink box
            if (enemy.Definition.IsBoss ? !BossBodyShape.Touches(enemy, Champion.Position, Champion.Definition.Radius) : (enemy.Position - Champion.Position).sqrMagnitude > radius * radius)
            {
                if (enemy.ContactImpactAt > 0) { enemy.ContactImpactAt = 0; enemy.NextContact = Elapsed + 0.15f; }
                return;
            }
            if (Elapsed < enemy.NextContact)
            {
                if (enemy.ContactImpactAt == 0 && enemy.NextCollision > 0) StrikeChampionCollision(enemy);
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
            Vector2 impactDirection = (Champion.Position - enemy.Position).normalized;
            float contactDamage = enemy.Definition.ContactDamage * (enemy.Definition.IsBoss ? BossBodyShape.ContactDamageScale : 1) * HeroBodyGuard();
            StrikeChampionCollision(enemy);
            if (contactDamage > 0) ApplyDamage(Champion, contactDamage, Faction.Enemy, enemy.Definition.DisplayName + " contact", impactDirection, cause: NewCause(enemy.Id));
        }

        private void MoveProjectiles(float deltaTime)
        {
            for (int index = projectiles.Count - 1; index >= 0 && Phase == RunPhase.Combat; index--)
            {
                ProjectileState projectile = projectiles[index];
                if (projectile.TravelLeft == 0) projectile.Remaining = Mathf.Min(projectile.Remaining, deltaTime);
                if (VisitorUmbrella(projectile)) { projectiles.RemoveAt(index); continue; }
                if (projectile.IdentityTargetId > 0)
                {
                    UnitState address = UnitWithId(projectile.IdentityTargetId);
                    if (address != null && !CanHit(Faction.Champion, address)) address = null;
                    if (address != null) projectile.Velocity = Vector2.Lerp(projectile.Velocity, (address.Position - projectile.Position).normalized * 5.5f, Mathf.Clamp01(deltaTime * 9));
                }
                if (projectile.Faction == Faction.Enemy) HostileBallistics.Advance(projectile, deltaTime * KitHostileShotScale);
                if (projectile.Pattern != ShotPattern.None && projectile.Faction != Faction.Enemy) ShotPatterns.Steer(projectile, deltaTime);
                int arsenalMotion = MoveArsenalProjectile(projectile, deltaTime);
                if (arsenalMotion != 0) { if (arsenalMotion < 0) projectiles.RemoveAt(index); continue; }
                int expandedMotion = MoveExpandedProjectile(projectile, deltaTime);
                if (expandedMotion != 0) { if (expandedMotion < 0) projectiles.RemoveAt(index); continue; }
                if (!AdvanceCombatProjectile(projectile, deltaTime)) projectiles.RemoveAt(index);
            }
        }

        private UnitState UnitWithId(int identity)
        {
            return unitLookup.TryGetValue(identity, out UnitState unit) ? unit : null;
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

        private void ApplyDamage(UnitState target, float damage, Faction source, string sourceName = "", Vector2 direction = default, WeaponDefinition weapon = null, bool critical = false, DamageElement element = DamageElement.Physical, CombatCause cause = default, bool injectedDamage = false, bool healthCost = false)
        {
            if (!target.Alive || target.Faction == Faction.Companion || !float.IsFinite(damage) || damage <= 0f) return;
            if (target.EntranceUntil > Elapsed && !injectedDamage) return;
            if (!healthCost) damage = ExpandedDamage(target, damage, source, direction, weapon);
            if (cause.RootId == 0) cause = NewCause(0);
            damage = IdentityDamage(target, damage, source, cause);
            damage = CompanionSkillDamage(target, damage, source, cause);
            damage = RelicItemDamage(target, damage, source, cause);
            damage = WaitingDamage(target, damage, source);
            damage = MapDamage(target, damage, source);
            if (target.Faction == Faction.Champion && !healthCost)
            {
                if (relicQuiltUntil > Elapsed) return;
                if (CandleSafe) return; // Second Candle: a moment untouchable after getting back up
                if (!injectedDamage && MetaShell(target)) return; // the Lamp Tree's Lamp shell
                if (!injectedDamage && catalog.Dodge + KitDodge > 0 && combatRandom.NextDouble() < Mathf.Clamp(catalog.Dodge + KitDodge, 0, ReferenceHero == "Sir Purrcival" ? .9f : .6f))
                {
                    events.Add(new CombatEvent(CombatEventKind.Dodge, target.Position, target.Faction, subject: target));
                    if (RelicCount(RelicMechanic.DodgeMending) > 0 && combatRandom.NextDouble() < .3) Heal(10 * RelicCount(RelicMechanic.DodgeMending), NewCause(target.Id).Child("curio-dodge"));
                    return;
                }
                if (!injectedDamage) damage = Mathf.Min(damage, target.Definition.Health * 0.22f);
                damage *= catalog.Armor >= 0 ? 1f / (1f + catalog.Armor) : 1 + Mathf.Min(1, -catalog.Armor);
                if (catalog.SecretWard && !injectedDamage) damage *= .85f;
                if (!injectedDamage) damage *= KitDamageTaken(cause);
                if (!injectedDamage) damage = OctoberIncoming(damage);
            }
            // All hostile contact, weapon and skill damage gets the same relief, including hits reaching the HP cap.
            if (source == Faction.Enemy && !injectedDamage && !healthCost)
            {
                damage *= OctoberBalance.EnemyDamageRelief;
                // Keep boss contact, projectiles and lingering skills consistent, even when capped above.
                if (bossDamageSources.Contains(cause.SourceUnitId)) damage *= OctoberBalance.BossDamageRelief;
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
            DamageBossPart(target, damage, source, direction, cause);
            if (target == Champion && !healthCost) damage = RelicQuilt(damage);
            float overkill = damage - target.Health;
            if (catalog.WholeKeeper && target.Definition.Id == "OutsideStageKeeper") damage *= KeeperGuard(target);
            target.Health = Mathf.Max(0f, target.Health - damage);
            if (target == Champion && target.Definition.Health > 0) LowestChampionShare = Mathf.Min(LowestChampionShare, target.Health / target.Definition.Health);
            target.LastHit = Elapsed;
            target.HitDirection = direction;
            if (target.Health > 0 && target.Faction == Faction.Enemy && source != Faction.Enemy && weapon != null && direction.sqrMagnitude > 0.001f)
                ImpactPush(target, direction, weapon, source, damage, cause);
            events.Add(new CombatEvent(CombatEventKind.Hit, target.Position, target.Faction, damage, direction, weapon, critical, element, target.Id, target, cause));
            NotifyArenaToyHit(events[events.Count - 1]);
            RelicHit(target, source, cause);
            if (target.Health <= 0f && SecondCandle(target)) return; // the Waiting Room's Clock: once a run
            if (target.Health > 0f) return;
            target.Alive = false;
            RememberVariantDeath(target);
            WaitingDeath(target);
            target.PendingAttack = false;
            QueueDeathReaction(target);
            events.Add(new CombatEvent(CombatEventKind.Kill, target.Position, target.Faction, Mathf.Clamp(overkill / Mathf.Max(1f, target.Definition.Health), 0f, 9f), direction, weapon, critical, element, target.Id, target, cause));
            TryFling(target, direction, weapon, source, damage, Mathf.Clamp(overkill / Mathf.Max(1f, target.Definition.Health), 0f, 9f), critical, cause);
            if (target.Faction == Faction.Champion) Phase = RunPhase.Defeat;
            else
            {
                if (target == Boss) BossDefeated = true;
                EliteFell(target); VarietyFell(target);
                Kills++;
                if (source != Faction.Enemy) RecordKill(cause);
                SwarmDeath(target, source, overkill, cause);
                PacingLoot(target);
                int coinValue = SwarmCoinValue(target); if (coinValue > 0) coinValue += VisitorCoins * coinValue; coinValue = MapCoins(target, coinValue); // a visitor's coin bonus doubles the kill's own coin (requirement 22/10/2026: a flat +1 stayed 1 gold late)
                DropKillCoins(target, coinValue);
                if (target.StolenCoins > 0) { DropLoot(LootKind.Coin, target.Position, target.StolenCoins + Mathf.Max(1, target.StolenCoins / 10)); target.StolenCoins = 0; } // a toll is refunded with 10% (nothing is taken, only bought)
                if (catalog.ExtendedLoot && !target.SwarmMinion)
                {
                    // play-test 20/10/2026 (item 16): bosses and elites always drop an egg (the late x2 egg chance was removed: eggs were already plentiful)
                    // bosses always drop one; elites too, but at most EliteEggsPerWave in a wave (the economy sweep: uncapped
                    // elite eggs piled up ~50 a wave late, far more than the dream shop can take)
                    bool sure = target.Definition.IsBoss || target.Elite && eliteEggs < EliteEggsPerWave;
                    if (sure && target.Elite && !target.Definition.IsBoss) eliteEggs++;
                    if (sure || lootRandom.NextDouble() < Mathf.Clamp01(catalog.EggDropChance * (1f + catalog.Luck))) DropLoot(LootKind.Egg, target.Position, 1);
                    if (lootRandom.NextDouble() < Mathf.Clamp01(catalog.HeartDropChance * (1f + catalog.Luck))) DropLoot(LootKind.Heart, target.Position, 8);
                }
            }
        }

        private void ApplyWeaponDamage(UnitState target, float damage, Faction source, DamageElement element, string sourceName = "", Vector2 direction = default, WeaponDefinition weapon = null, bool critical = false, CombatCause cause = default)
        {
            if (!target.Alive) return;
            if (cause.RootId == 0) cause = NewCause(0);
            damage *= RangedHitFactor(target, weapon, source, cause); // R10: each ranged weapon's sweet range
            damage *= GuardFactor(target, direction, weapon, source); // a shielder's ward, an armored front (EnemyVariety)
            bool soaked = target.Faction == Faction.Enemy && source != Faction.Enemy && element == DamageElement.Frost && target.KitWetUntil > Elapsed; // a soaked enemy hit by frost freezes
            bool reaction = target.Faction == Faction.Enemy && ((element == DamageElement.Ember && target.ChillUntil > Elapsed) || (element == DamageElement.Frost && target.BurnUntil > Elapsed));
            ApplyDamage(target, damage, source, sourceName, direction, weapon, critical, element, cause);
            if (target.Faction != Faction.Enemy || (!target.Alive && element != DamageElement.Arcane)) return;
            if (soaked && target.Alive) { target.KitWetUntil = 0; Freeze(target, .6f); }
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
                if (!pickup.Attracting && difference.sqrMagnitude <= RelicPickupRadius * RelicPickupRadius)
                {
                    pickup.Attracting = true;
                    if (pickup.Kind == LootKind.Coin) events.Add(new CombatEvent(CombatEventKind.LootAttract, pickup.Position, Faction.Champion, subjectId: pickup.Id, loot: pickup.Kind));
                }
                if (!pickup.Attracting) continue;
                pickup.Position = WaveCadence.Attract(pickup.Position, Champion.Position, (pickup.Vacuumed ? 15f + age * 3f : 7f + age * 2f) * deltaTime, pickup.Id);
                if ((pickup.Position - Champion.Position).sqrMagnitude > 0.045f) continue;
                CombatCause pickupCause = NewCause(Champion.Id);
                if (pickup.Kind == LootKind.Coin)
                {
                    CreditCoin(pickup, pickupCause);
                }
                else if (pickup.Kind == LootKind.Egg) Eggs = RunSession.BoundedCounter((long)Eggs + pickup.Value);
                else if (pickup.Kind == LootKind.Heart) Heal(Mathf.Max(0, pickup.Value + catalog.HeartHealing), NewCause(Champion.Id));
                events.Add(new CombatEvent(CombatEventKind.Pickup, pickup.Position, Faction.Champion, pickup.Value, subjectId: pickup.Id, cause: pickupCause, loot: pickup.Kind));
                RelicPickup(pickup);
                pickups.RemoveAt(index);
                if (pickup.Kind == LootKind.Flare) { LanternFlare(); if (Phase != RunPhase.Combat) return; }
                else if (pickup.Kind == LootKind.Bell) RingBell();
                else if (pickup.Kind == LootKind.Prism) PrismShards++;
            }
        }

        private void CreditCoin(PickupState pickup, CombatCause cause)
        {
            coinCredit += pickup.Value * KitCoinScale * Mathf.Max(0, catalog.CoinValueScale);
            int whole = Mathf.FloorToInt(coinCredit + 1e-4f); coinCredit -= whole;
            int nextGold = RunSession.BoundedCounter((long)Gold + whole);
            CombatCause credit = cause.Child("coin-credit");
            Contribution(credit).Coins = RunSession.BoundedCounter((long)Contribution(credit).Coins + nextGold - Gold);
            Gold = nextGold;
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
            DropLoot(LootKind.Chest, position, Mathf.Max(1, Mathf.RoundToInt((5 + lootRandom.Next(0, 7)) * K10StatRules.Factor(catalog.ChestQuality, 0.2f, 5))));
        }

        private void OpenChest(PickupState chest)
        {
            ChestsOpened++;
            RelicChest();
            CombatCause cause = NewCause(Champion.Id);
            bool egg = lootRandom.NextDouble() < Mathf.Clamp01(0.35f + catalog.Luck * 0.2f);
            bool heart = lootRandom.NextDouble() < Mathf.Clamp01(0.5f + catalog.Luck * 0.2f);
            // the chest's coin follows the shop's gold curve (play-test 20/10/2026), so a chest stays worth opening late
            // an average chest (value 8) holds about one mid-priced offer of the shop's wave (play-test 20/10/2026, items 12/20)
            float perCoin = Mathf.Max(.01f, KitCoinScale * Mathf.Max(.01f, catalog.CoinValueScale));
            int coins = catalog.ChestGold > 0 ? Mathf.Max(1, Mathf.RoundToInt(catalog.ChestGold * (chest.Value + KitChestBonus()) / 8f / perCoin))
                : Mathf.Max(1, Mathf.RoundToInt((chest.Value + KitChestBonus()) * Mathf.Max(1, catalog.ChestGoldScale)));
            int gold = Mathf.Max(1, Mathf.RoundToInt(coins * KitCoinScale * Mathf.Max(0, catalog.CoinValueScale)));
            float healing = heart ? Mathf.Max(1, 12 + catalog.HeartHealing) : 0;
            // the popup reads: Value = gold credited, Direction = (egg 1/0, heart healing or 0)
            events.Add(new CombatEvent(CombatEventKind.ChestOpened, chest.Position, Faction.Champion, gold, new Vector2(egg ? 1 : 0, healing), subjectId: chest.Id, cause: cause, loot: LootKind.Chest));
            DropLoot(LootKind.Coin, chest.Position, coins);
            if (egg) DropLoot(LootKind.Egg, chest.Position, 1);
            if (heart) DropLoot(LootKind.Heart, chest.Position, 12);
        }

        private void Heal(float amount, CombatCause cause)
        {
            if (catalog.EnableHeroMechanics && (ReferenceHero == "Card Queen" || ReferenceHero == "Omnom" && cause.ItemId != "night-maw-meal")) return;
            if (!Champion.Alive) return;
            float previous = Champion.Health;
            Champion.Health = Mathf.Min(Champion.Definition.Health, Champion.Health + Mathf.Max(0, amount) * catalog.OctoberHealingReceived);
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
            if (!RunPaths.K3FinalCapture && !RunPaths.QualityCapture && !RunPaths.K4Capture && !RunPaths.K4FinalCapture && !RunPaths.K5Capture && !RunPaths.K7Capture && !PlaytestFixCapture.Requested) throw new InvalidOperationException("Loot fixture requires isolated capture mode");
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
