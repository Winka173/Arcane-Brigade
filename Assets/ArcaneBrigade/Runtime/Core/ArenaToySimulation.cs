using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public enum ArenaToyKind { StageBell, ToyCart }
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
        public float Radius => Kind == ArenaToyKind.StageBell ? 2.4f : 0.55f;
        internal CombatCause Cause;
        internal readonly HashSet<int> Roots = new HashSet<int>();
        internal readonly HashSet<int> Victims = new HashSet<int>();
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

        public void ResetArenaToys(int wave = 1)
        {
            ClearArenaToys();
            arenaToyWave = Mathf.Max(1, wave);
            if (!catalog.EnableArenaToys || Champion == null) return;
            var random = new System.Random(unchecked(catalog.Seed ^ arenaToyWave * 486187739 ^ 0x4A44));
            int count = arenaToyWave == 1 ? 1 : 2;
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
                toy.Cause = new CombatCause(cause.RootId, 0, toy.Kind == ArenaToyKind.StageBell ? "arena-stage-bell" : "arena-toy-cart", 1);
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
                    if (toy.Kind == ArenaToyKind.StageBell)
                    {
                        DamageArenaToyArea(toy, toy.Position, toy.Position, toy.Radius);
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
        }
#endif
    }
}
