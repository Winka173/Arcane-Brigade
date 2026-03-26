using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public enum ArenaToyKind { StageBell, ToyCart, InkBottle, HangingHammer, SleepMirror, FloorMusicBox }
    public enum ArenaToyMode { Charging, Warning, Rolling, Cooldown }

    public sealed class ArenaToyState
    {
        public int Id { get; internal set; }
        public ArenaToyKind Kind { get; internal set; }
        public ArenaToyMode Mode { get; internal set; }
        public Vector2 Position { get; internal set; }
        public Vector2 PreviousPosition { get; internal set; }
        public Vector2 LaneStart { get; internal set; }
        public Vector2 LaneEnd { get; internal set; }
        public Vector2 ShiftDestination { get; internal set; }
        public int Charge { get; internal set; }
        public int RequiredCharge => Kind == ArenaToyKind.StageBell ? 3 : 2;
        public float Until { get; internal set; }
        public float Started { get; internal set; }
        public float Radius => Kind == ArenaToyKind.StageBell ? 2.4f : Kind == ArenaToyKind.InkBottle ? 2.5f : Kind == ArenaToyKind.HangingHammer ? 1.8f : Kind == ArenaToyKind.FloorMusicBox ? 4f : 0.55f;
        internal CombatCause Cause;
        internal readonly HashSet<int> Roots = new HashSet<int>();
        internal readonly HashSet<int> Victims = new HashSet<int>();
        internal float NextAutomatic;
    }

    public sealed partial class CombatSimulation
    {
        public const float ArenaToyWarning = 1.2f;
        public const float ArenaToyCooldown = 5f;
        public const float ArenaToyRollDuration = 0.8f;
        private readonly List<ArenaToyState> arenaToys = new List<ArenaToyState>();
        private IReadOnlyList<ArenaToyState> arenaToyView;
        private int arenaToyWave;
        private bool arenaShiftConsumed;
        public IReadOnlyList<ArenaToyState> ArenaToys => arenaToyView ?? (arenaToyView = arenaToys.AsReadOnly());
        public bool ArenaShiftWarning { get; private set; }
        public bool ArenaShiftComplete { get; private set; }
        public float ArenaShiftUntil { get; private set; }
        public int ArenaToyActivations { get; private set; }

        public void ClearArenaToys()
        {
            arenaToys.Clear();
            ArenaShiftWarning = ArenaShiftComplete = arenaShiftConsumed = false;
            ArenaShiftUntil = 0f;
            ArenaToyActivations = 0;
        }

        /// <summary>The arena's fixed props (chair, landmarks, lanterns, architecture...): the same plan the presentation
        /// draws, computed from plain catalog data. Null when the catalog carries no scenery (sandbox, tests).</summary>
        public ArenaLayoutPlan ArenaLayout { get; private set; }
        /// <summary>A toy's painted body and shadow reach this far from its position.</summary>
        public const float ArenaToyFootprint = 0.85f;

        public void ResetArenaToys(int wave = 1)
        {
            ClearArenaToys();
            arenaToyWave = Mathf.Max(1, wave);
            ArenaLayout = catalog.Scenery is ArenaScenery scenery ? ArenaLayoutPlan.For(scenery.Resized(catalog.ArenaSize)) : null;
            if (!catalog.EnableArenaToys || Champion == null) return;
            var random = new System.Random(unchecked(catalog.Seed ^ arenaToyWave * 486187739 ^ 0x4A44));
            int count = arenaToyWave == 1 ? 1 : 2;
            if (catalog.ExpandedContent) count = Mathf.Min(6, 2 + (arenaToyWave - 1) / 2);
            for (int index = 0; index < count; index++)
            {
                if (!TryArenaPlacement(random, out Vector2 position)) continue;
                arenaToys.Add(new ArenaToyState { Id = index + 1, Kind = (ArenaToyKind)index, Position = position, PreviousPosition = position, ShiftDestination = position });
            }
        }

        private bool SafeArenaPlacement(Vector2 position, ArenaToyState ignored = null)
        {
            Vector2 half = catalog.ArenaSize * 0.5f - Vector2.one * 1.1f;
            if (half.x <= 0f || half.y <= 0f || !FiniteArenaVector(position) || Mathf.Abs(position.x) > half.x || Mathf.Abs(position.y) > half.y) return false;
            if (ArenaLayout != null && ArenaLayout.Blocks(position, ArenaToyFootprint)) return false; // never on a prop
            foreach (UnitState unit in units)
            {
                if (!unit.Alive) continue;
                float clearance = unit.Definition.Radius + (unit.Faction == Faction.Champion ? 2f : 0.85f);
                if ((position - unit.Position).sqrMagnitude < clearance * clearance) return false;
                if (unit.Definition.IsBoss && unit.TelegraphUntil > Elapsed) return false;
            }
            foreach (PickupState pickup in pickups)
                if ((position - pickup.Position).sqrMagnitude < 1.2f * 1.2f) return false;
            foreach (ArenaToyState toy in arenaToys)
                if (toy != ignored && ((position - toy.Position).sqrMagnitude < 3f * 3f || (position - toy.ShiftDestination).sqrMagnitude < 3f * 3f)) return false;
            return true;
        }

        private bool TryArenaPlacement(System.Random random, out Vector2 position, ArenaToyState ignored = null)
        {
            Vector2 half = catalog.ArenaSize * 0.5f - Vector2.one * 1.1f;
            for (int attempt = 0; attempt < 96; attempt++)
            {
                position = new Vector2(((float)random.NextDouble() * 2f - 1f) * half.x, ((float)random.NextDouble() * 2f - 1f) * half.y);
                if (SafeArenaPlacement(position, ignored)) return true;
            }
            position = Vector2.zero;
            return false;
        }

        private static bool FiniteArenaVector(Vector2 value) => float.IsFinite(value.x) && float.IsFinite(value.y);

        public void NotifyArenaToyHit(CombatEvent impact)
        {
            if (impact.Kind != CombatEventKind.Hit || impact.Faction != Faction.Enemy || !float.IsFinite(impact.Value) || impact.Value <= 0f) return;
            NotifyArenaToyHit(impact.Position, impact.Direction, impact.Cause);
        }

        public void NotifyArenaToyHit(Vector2 point, Vector2 direction, CombatCause cause)
        {
            UnitState source = null;
            foreach (UnitState unit in units)
                if (unit.Id == cause.SourceUnitId) { source = unit; break; }
            NotifyArenaToyHit(source, point, point, 0f, direction, cause);
        }

        public void NotifyArenaToyHit(Vector2 point, CombatCause cause) => NotifyArenaToyHit(point, Vector2.right, cause);

        public void NotifyArenaToyHit(UnitState source, Vector2 start, Vector2 end, float impactRadius, Vector2 direction, CombatCause cause)
        {
            if (!catalog.EnableArenaToys || Phase != RunPhase.Combat || ArenaShiftWarning || source == null || !source.Alive ||
                (source.Faction != Faction.Champion && source.Faction != Faction.Companion) || cause.RootId == 0 || cause.Depth != 0 ||
                cause.SourceUnitId != source.Id || !FiniteArenaVector(start) || !FiniteArenaVector(end) || !FiniteArenaVector(direction) ||
                !float.IsFinite(impactRadius) || impactRadius < 0f) return;
            foreach (ArenaToyState toy in arenaToys)
            {
                if (toy.Mode != ArenaToyMode.Charging || SegmentCircle(start, end, toy.Position, impactRadius + 0.45f) < 0f || !toy.Roots.Add(cause.RootId)) continue;
                toy.Charge++;
                if (toy.Charge < toy.RequiredCharge) continue;
                toy.Cause = new CombatCause(cause.RootId, 0, "arena-" + toy.Kind.ToString().ToLowerInvariant(), 1);
                if (toy.Kind == ArenaToyKind.StageBell) toy.Cause = new CombatCause(cause.RootId, 0, "arena-stage-bell", 1);
                if (toy.Kind == ArenaToyKind.ToyCart) toy.Cause = new CombatCause(cause.RootId, 0, "arena-toy-cart", 1);
                toy.Mode = ArenaToyMode.Warning;
                toy.Started = Elapsed;
                toy.Until = Elapsed + ArenaToyWarning;
                toy.LaneStart = toy.Position;
                Vector2 heading = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
                toy.LaneEnd = ClampPosition(toy.Position + heading * 4f, 0.65f);
            }
        }

        public void ProcessArenaToys(float deltaTime)
        {
            if (!catalog.EnableArenaToys || Phase != RunPhase.Combat || !float.IsFinite(deltaTime) || deltaTime <= 0f || deltaTime > 0.1f) return;
            ProcessArenaShift();
            if (ArenaShiftWarning) return;
            foreach (ArenaToyState toy in arenaToys)
            {
                toy.PreviousPosition = toy.Position;
                if (catalog.PatchSpring && toy.Mode == ArenaToyMode.Charging && Elapsed >= toy.NextAutomatic)
                {
                    toy.NextAutomatic = Elapsed + 3f;
                    NotifyArenaToyHit(Champion, toy.Position, toy.Position, 0, Vector2.right, NewCause(Champion.Id));
                }
                if (toy.Mode == ArenaToyMode.Cooldown && Elapsed >= toy.Until)
                {
                    toy.Mode = ArenaToyMode.Charging;
                    toy.Charge = 0;
                }
                if (toy.Mode == ArenaToyMode.Warning && Elapsed >= toy.Until)
                {
                    ArenaToyActivations++;
                    RecordActivation(toy.Cause);
                    toy.Victims.Clear();
                    if (toy.Kind >= ArenaToyKind.InkBottle)
                    {
                        ActivateExpandedToy(toy);
                        CoolArenaToy(toy);
                        continue;
                    }
                    if (toy.Kind == ArenaToyKind.StageBell)
                    {
                        RingStageBell(toy);
                        CoolArenaToy(toy);
                    }
                    else
                    {
                        toy.Mode = ArenaToyMode.Rolling;
                        toy.Started = Elapsed;
                        toy.Until = Elapsed + ArenaToyRollDuration;
                    }
                }
                if (toy.Mode != ArenaToyMode.Rolling) continue;
                float progress = Mathf.Clamp01((Elapsed - toy.Started) / ArenaToyRollDuration);
                toy.Position = Vector2.Lerp(toy.LaneStart, toy.LaneEnd, 1f - (1f - progress) * (1f - progress));
                DamageArenaToyArea(toy, toy.PreviousPosition, toy.Position, toy.Radius);
                if (Elapsed >= toy.Until) CoolArenaToy(toy);
            }
        }

        private void CoolArenaToy(ArenaToyState toy)
        {
            toy.Mode = ArenaToyMode.Cooldown;
            toy.Started = Elapsed;
            toy.Until = Elapsed + ArenaToyCooldown;
            toy.Charge = 0;
        }

        /// <summary>The stage bell rings out three chimes that fly at the nearest enemies (no ring round the bell).</summary>
        private void RingStageBell(ArenaToyState toy)
        {
            targetCandidates.Clear();
            foreach (UnitState u in units) if (u.Alive && u.Faction == Faction.Enemy && (u.Position - toy.Position).sqrMagnitude <= 36f) targetCandidates.Add(u);
            targetCandidates.Sort((a, b) => (a.Position - toy.Position).sqrMagnitude.CompareTo((b.Position - toy.Position).sqrMagnitude));
            Sprite chime = CombatVfxArtwork.Frame("BellChime", 0);
            var ring = new HashSet<int>(); // each enemy is struck once per ring
            for (int i = 0; i < 3; i++)
            {
                Vector2 aim = targetCandidates.Count > 0 ? targetCandidates[i % targetCandidates.Count].Position - toy.Position : new Vector2(Mathf.Cos(i * 2.1f), Mathf.Sin(i * 2.1f));
                aim = (aim.sqrMagnitude > 0.0001f ? aim.normalized : Vector2.right);
                if (targetCandidates.Count <= i) aim = (Vector2)(Quaternion.Euler(0, 0, (i - 1) * 14) * aim);
                projectiles.Add(new ProjectileState { Id = ++nextId, Faction = Faction.Companion, Position = toy.Position, PreviousPosition = toy.Position, Velocity = aim * 7f,
                    LaunchVelocity = aim * 7f, Remaining = 0.9f, Damage = 14f, Color = new Color(1, 0.86f, 0.5f), Element = DamageElement.Physical, SourceName = "Stage bell",
                    Sprite = chime, VisualWidth = 0.4f, Radius = 0.16f, IsFragment = true, Cause = toy.Cause, Volley = ring });
            }
            events.Add(new CombatEvent(CombatEventKind.Support, toy.Position + Vector2.up * 0.5f, Faction.Companion, cause: toy.Cause));
        }

        private void ActivateExpandedToy(ArenaToyState toy)
        {
            if (toy.Kind == ArenaToyKind.InkBottle)
            {
                Field(ExpandedFieldKind.Ink, Champion, toy.Position, Vector2.zero, 2.5f, 0, 0, 2f, toy.Cause);
            }
            else if (toy.Kind == ArenaToyKind.HangingHammer)
            {
                // the hammer falls where it hangs: a painted slam, not a ring
                DamageArenaToyArea(toy, toy.Position, toy.Position, 1.8f);
                events.Add(new CombatEvent(CombatEventKind.Detonation, toy.Position, Faction.Companion, 1.8f / 1.4f, Vector2.down, cause: toy.Cause));
            }
            else if (toy.Kind == ArenaToyKind.SleepMirror)
            {
                UnitState owner = units.Find(unit => unit.Alive && unit.Faction != Faction.Enemy && unit.Definition.Weapon.Kind == WeaponKind.Projectile);
                UnitState target = units.Find(unit => unit.Alive && unit.Faction == Faction.Enemy && (unit.Position - toy.Position).sqrMagnitude < 36f);
                if (owner != null && target != null)
                    projectiles.Add(new ProjectileState { Id = ++nextId, Faction = Faction.Companion, Position = toy.Position, PreviousPosition = toy.Position, Velocity = (target.Position - toy.Position).normalized * 6f, Remaining = 1.5f, Damage = owner.Definition.Weapon.Damage * 0.4f, SourceUnitId = owner.Id, Cause = toy.Cause, Color = owner.Definition.Weapon.ProjectileColor, Sprite = owner.Definition.Weapon.ProjectileSprite, VisualWidth = 0.18f, Element = DamageElement.Physical, SourceName = "Sleep mirror" });
            }
            else
            {
                // the music box plays to each friend nearby: a note gathers on each of them (no ring round the box)
                foreach (UnitState ally in units)
                    if (ally.Alive && ally.Faction != Faction.Enemy && (ally.Position - toy.Position).sqrMagnitude < 16f)
                    {
                        ally.NextAttack = Mathf.Max(Elapsed, ally.NextAttack - 0.2f);
                        events.Add(new CombatEvent(CombatEventKind.Support, ally.Position + Vector2.up * 0.25f, ally.Faction, subject: ally, cause: toy.Cause));
                    }
            }
        }

        private void DamageArenaToyArea(ArenaToyState toy, Vector2 start, Vector2 end, float radius)
        {
            foreach (UnitState target in units)
            {
                if (!target.Alive || target.Faction != Faction.Enemy || toy.Victims.Contains(target.Id) ||
                    SegmentCircle(start, end, target.Position, radius + target.Definition.Radius) < 0f) continue;
                toy.Victims.Add(target.Id);
                float damage = toy.Kind == ArenaToyKind.StageBell ? 14f : 18f;
                if (target.Definition.IsBoss) damage = Mathf.Min(damage * 0.35f, target.Definition.Health * 0.08f);
                ApplyDamage(target, damage, Faction.Companion, toy.Cause.ItemId, (end - start).normalized, cause: toy.Cause);
            }
        }

        private void ProcessArenaShift()
        {
            if (!catalog.EnableBossArenaPhase) return;
            if (Boss == null || !Boss.Alive)
            {
                ArenaShiftWarning = false;
                return;
            }
            if (!arenaShiftConsumed && Boss.Health <= Boss.Definition.Health * 0.5f)
            {
                arenaShiftConsumed = true;
                ArenaShiftWarning = true;
                ArenaShiftUntil = Elapsed + ArenaToyWarning;
                var random = new System.Random(unchecked(catalog.Seed ^ arenaToyWave * 486187739 ^ 0xB044));
                foreach (ArenaToyState toy in arenaToys)
                {
                    toy.ShiftDestination = TryArenaPlacement(random, out Vector2 destination, toy) ? destination : toy.Position;
                    CoolArenaToy(toy);
                }
                return;
            }
            if (!ArenaShiftWarning || Elapsed < ArenaShiftUntil) return;
            foreach (ArenaToyState toy in arenaToys)
            {
                if (SafeArenaPlacement(toy.ShiftDestination, toy)) toy.Position = toy.ShiftDestination;
                toy.PreviousPosition = toy.Position;
                toy.ShiftDestination = toy.Position;
                CoolArenaToy(toy);
            }
            ArenaShiftWarning = false;
            ArenaShiftComplete = true;
        }

#if UNITY_EDITOR
        public void ArenaToyTickFixture(float deltaTime)
        {
            if (Phase != RunPhase.Combat || !float.IsFinite(deltaTime) || deltaTime <= 0f || deltaTime > 0.1f) return;
            Elapsed += deltaTime;
            ProcessArenaToys(deltaTime);
            spatial.Rebuild(units); MoveProjectiles(deltaTime); // the stage bell's chimes fly to their targets
        }
#endif
    }
}
