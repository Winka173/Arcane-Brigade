using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Per-enemy state of the roster v2 enemies (Docs/Enemy_Roster_v2.md). Any enemy can also carry a PAID stamp.</summary>
    public sealed class WaitingEnemyState
    {
        public WaitingEnemyKind Kind;
        public int Step;               // what it is doing (per kind): 0 walking, then its wind-up and release steps
        public float StepAt;           // when the current step began
        public float NextAction;       // when it may start its move again
        public float ReleaseAt;        // end of the current wind-up (0 = none)
        public Vector2 Aim, Origin;    // locked aim (ring centre, cord end, landing spot) and leap origin
        public float Radius;           // the seat ring's radius
        public int Count;              // knocks landed, tally marks laid, casts made
        public int TargetId;           // the Notary's chosen ally
        public int Lantern = -1;       // the Snuffer's lantern
        public float Angle, Spin;      // the Hound's place on its circle and its turning sense
        public readonly Vector2[] Tallies = new Vector2[4];
        public readonly float[] KnockAt = { -1, -1, -1 };
        public int Slashes;
        public bool Tethered;          // the Sitter's ink thread is on the hero
        public float TetherUntil, NextTetherTick;
        public bool Paid;              // PAID: the next hit this enemy takes is halved
        public float PaidAt = -100;
    }

    public enum WaitingMarkKind { SeatRing, ClosedPatch, CordPreview, Cord, KnockRing, Tally, SlashLine, Slash, Thread }

    /// <summary>Something a roster v2 enemy draws on the floor: a warning (before At) or a live mark (At..Until).</summary>
    public sealed class WaitingMark
    {
        public WaitingMarkKind Kind;
        public UnitState Source;
        public Vector2 A, B;           // centre (A) or segment (A to B)
        public float Radius;
        public float Created, At, Until;
        public float NextTick;
        public int Index;
        public bool Hit;               // the ring caught the hero / the slash or cord struck
    }

    /// <summary>The Waiting Realm's eight new enemies. Every move targets the hero or the floor (the companions are immortal):
    /// Spare Chair pins, Waking Sitter tethers, Ink Notary stamps PAID on an ally and CLOSED on the floor, Sewn Letter bursts,
    /// Party Line pins a live cord, Door Knocker knocks three waves, Lamp Snuffer dims a lantern, Ledger Hound tallies then
    /// slashes. Each reads first (a wind-up pose and an inked mark), then lands.</summary>
    public sealed partial class CombatSimulation
    {
        public const float SeatTelegraph = 1f, SeatRoot = .6f, TetherSlow = .7f, TetherBreak = 5f, ClosedSlow = .75f, ClosedSeconds = 3f;
        public const float PaidShare = .5f, HalfShare = .35f, StampShare = .12f, CordRing = .9f, CordSeconds = 3f, KnockBeat = .7f, TallyBeat = .8f;
        private sealed class PendingPiece { public UnitDefinition Root; public bool Stamp; public Vector2 Position, Away; }
        private readonly List<WaitingMark> waitingMarks = new List<WaitingMark>();
        private readonly List<PendingPiece> waitingPieces = new List<PendingPiece>();
        private readonly Dictionary<UnitDefinition, UnitDefinition> letterHalves = new Dictionary<UnitDefinition, UnitDefinition>(), letterStamps = new Dictionary<UnitDefinition, UnitDefinition>();
        private readonly Dictionary<UnitDefinition, UnitDefinition> letterRoots = new Dictionary<UnitDefinition, UnitDefinition>();
        private readonly float[] lanternDim = new float[8];
        private Vector2[] lanternFixture;
        private int waitingGeneration = -1;
        private bool heroTethered;
        public IReadOnlyList<WaitingMark> WaitingMarks => waitingMarks;
        public float HeroRootedUntil { get; private set; }
        public float HeroClosedUntil { get; private set; }
        public bool HeroTethered => heroTethered;

        private void SyncWaiting()
        {
            if (waitingGeneration == Generation) return;
            waitingGeneration = Generation;
            waitingMarks.Clear(); waitingPieces.Clear(); System.Array.Clear(lanternDim, 0, lanternDim.Length);
            HeroRootedUntil = HeroClosedUntil = 0; heroTethered = false;
        }

        /// <summary>The hero's walk under the roster's holds: pinned by a seat ring, slowed by a taut thread or CLOSED ink.</summary>
        private float WaitingHeroScale
        {
            get
            {
                SyncWaiting();
                if (HeroRootedUntil > Elapsed) return 0;
                float scale = 1;
                if (heroTethered) scale *= TetherSlow;
                if (HeroClosedUntil > Elapsed) scale *= ClosedSlow;
                return scale;
            }
        }

        // ------------------------------------------------------------------ lanterns (Lamp Snuffer)
        /// <summary>How far a lantern has been dimmed by Lamp Snuffers (0 normal, 1 dark); the lantern field multiplies its light.</summary>
        public float LanternDimming(int index) => (Phase == RunPhase.Combat || Phase == RunPhase.Paused) && waitingGeneration == Generation && index >= 0 && index < lanternDim.Length ? lanternDim[index] : 0;
        public bool LanternDimmed(int index) => LanternDimming(index) > 0;
        /// <summary>Check fixture: these spots are the arena's lit lanterns (no scenery needed).</summary>
        public void ConfigureWaitingLanternFixture(Vector2[] spots) { lanternFixture = spots; }
        /// <summary>A lit lantern's floor spot: the arena layout's lantern foot, when its canon lamp still burns this wave.</summary>
        public bool WaitingLantern(int index, out Vector2 at)
        {
            at = Vector2.zero;
            if (index < 0 || index >= 8) return false;
            if (lanternFixture != null) { if (index >= lanternFixture.Length) return false; at = lanternFixture[index]; return true; }
            if (!(catalog.Scenery is ArenaScenery scenery)) return false;
            if (NightLanternField.StateFor(index, catalog.RunWave, false, catalog.LanternRelitMask) == 2) return false;
            ArenaLayoutPlan plan = ArenaLayoutPlan.For(scenery.Resized(catalog.ArenaSize));
            if (!plan.TryGet("lantern" + index, out ArenaLayoutPlan.Spot spot) || !spot.Visible) return false;
            at = spot.Foot; return true;
        }

        // ------------------------------------------------------------------ spawning and damage hooks
        /// <summary>At most three chairs, letters or sitters and two of each other new kind at once, six in all.</summary>
        private bool WaitingSpawnBlocked(UnitDefinition definition)
        {
            WaitingEnemyKind kind = definition == null ? WaitingEnemyKind.None : definition.WaitingKind;
            if (kind == WaitingEnemyKind.None) return false;
            int same = 0, all = 0;
            foreach (UnitState unit in units)
            {
                if (!unit.Alive || unit.Faction != Faction.Enemy) continue;
                WaitingEnemyKind other = unit.Definition.WaitingKind;
                if (other == kind) same++;
                if (other != WaitingEnemyKind.None && other != WaitingEnemyKind.LetterHalf && other != WaitingEnemyKind.LetterStamp) all++;
            }
            int cap = kind == WaitingEnemyKind.SpareChair || kind == WaitingEnemyKind.SewnLetter || kind == WaitingEnemyKind.WakingSitter ? 3 : 2;
            return same >= cap || all >= 6;
        }

        /// <summary>PAID: an enemy stamped by an Ink Notary takes half of the next hit we deal it.</summary>
        private float WaitingDamage(UnitState target, float damage, Faction source)
        {
            if (target.Faction != Faction.Enemy || source == Faction.Enemy || target.Waiting == null || !target.Waiting.Paid) return damage;
            target.Waiting.Paid = false;
            return damage * PaidShare;
        }

        /// <summary>A Sewn Letter bursts into two halves, a half into two stamps (spawned on the next roster step, never inside
        /// a damage loop); stamps do not split.</summary>
        private void WaitingDeath(UnitState target)
        {
            if (target.Faction != Faction.Enemy) return;
            WaitingEnemyKind kind = target.Definition.WaitingKind;
            if (kind != WaitingEnemyKind.SewnLetter && kind != WaitingEnemyKind.LetterHalf) return;
            SyncWaiting();
            UnitDefinition root = kind == WaitingEnemyKind.SewnLetter ? target.Definition : letterRoots.TryGetValue(target.Definition, out UnitDefinition found) ? found : target.Definition;
            Vector2 away = (target.Position - Champion.Position).sqrMagnitude > .0001f ? (target.Position - Champion.Position).normalized : Vector2.up;
            for (int side = -1; side <= 1; side += 2)
                waitingPieces.Add(new PendingPiece { Root = root, Stamp = kind == WaitingEnemyKind.LetterHalf, Position = target.Position, Away = new Vector2(-away.y, away.x) * side });
        }

        private UnitDefinition LetterPiece(UnitDefinition root, bool stamp)
        {
            Dictionary<UnitDefinition, UnitDefinition> cache = stamp ? letterStamps : letterHalves;
            if (cache.TryGetValue(root, out UnitDefinition piece) && piece != null) return piece;
            piece = Object.Instantiate(root);
            piece.name = root.name + (stamp ? " stamp" : " half");
            piece.WaitingKind = stamp ? WaitingEnemyKind.LetterStamp : WaitingEnemyKind.LetterHalf;
            piece.Health = root.Health * (stamp ? StampShare : HalfShare);
            piece.BodyHeight = root.BodyHeight * (stamp ? .5f : .72f); piece.Radius = root.Radius * (stamp ? .6f : .8f);
            piece.Speed = root.Speed * (stamp ? 1.3f : 1.15f); piece.ContactDamage = root.ContactDamage * (stamp ? .5f : .75f);
            piece.KnockbackResistance = 0; piece.DisplayName = stamp ? "Postage Stamp" : "Torn Letter";
            WaitingEnemyRoster roster = WaitingEnemyRoster.Current;
            AuthoredMotionSet motion = roster == null ? null : stamp ? roster.LetterStamp : roster.LetterHalf;
            if (motion != null && motion.Idle != null && motion.Idle.Length >= 2 && motion.Walk != null && motion.Walk.Length >= 2)
            {
                piece.AuthoredMotion = motion; piece.Body = motion.Idle[0];
                piece.AnimationFrames = new[] { motion.Idle[0], motion.Idle[1], motion.Walk[0], motion.Walk[1] };
            }
            cache[root] = piece; letterRoots[piece] = root;
            return piece;
        }

        // ------------------------------------------------------------------ the roster's own step (after the arena's fields)
        private void ProcessWaitingEnemies(float deltaTime)
        {
            SyncWaiting();
            for (int index = 0; index < waitingPieces.Count; index++)
            {
                PendingPiece pending = waitingPieces[index];
                UnitDefinition definition = LetterPiece(pending.Root, pending.Stamp);
                UnitState piece = Spawn(definition, Faction.Enemy, ClampPosition(pending.Position + pending.Away * .45f, definition.Radius));
                piece.SwarmMinion = true; // a piece drops a small coin, like a swarm body
                piece.Waiting = new WaitingEnemyState { Kind = definition.WaitingKind, NextAction = Elapsed + .4f };
                piece.NextContact = piece.NextAttack = Elapsed + .5f;
                piece.Facing = pending.Away;
            }
            waitingPieces.Clear();
            bool anyThread = false;
            for (int index = waitingMarks.Count - 1; index >= 0; index--)
            {
                WaitingMark mark = waitingMarks[index];
                bool sourceGone = mark.Source == null || !mark.Source.Alive;
                if (Elapsed > mark.Until || sourceGone && (mark.Kind == WaitingMarkKind.Cord || mark.Kind == WaitingMarkKind.Thread || Elapsed < mark.At && mark.Kind != WaitingMarkKind.ClosedPatch))
                { waitingMarks.RemoveAt(index); continue; }
                if (Elapsed < mark.At || !Champion.Alive) continue;
                switch (mark.Kind)
                {
                    case WaitingMarkKind.ClosedPatch:
                        if ((Champion.Position - mark.A).magnitude <= mark.Radius + Champion.Definition.Radius) { HeroClosedUntil = Elapsed + ClosedSeconds; mark.Hit = true; }
                        break;
                    case WaitingMarkKind.Cord:
                        if (Elapsed >= mark.NextTick && SegmentDistance(Champion.Position, mark.A, mark.B) <= mark.Radius + Champion.Definition.Radius)
                        {
                            mark.NextTick = Elapsed + .6f; mark.Hit = true;
                            ApplyDamage(Champion, mark.Source.Definition.Weapon.Damage * .8f, Faction.Enemy, mark.Source.Definition.DisplayName + " cord", (Champion.Position - mark.A).normalized, cause: NewCause(mark.Source.Id).Child("party-line-cord"));
                        }
                        break;
                    case WaitingMarkKind.Thread:
                        mark.A = mark.Source.Position; mark.B = Champion.Position;
                        anyThread |= mark.Source.Waiting != null && mark.Source.Waiting.Tethered && (mark.Source.Position - Champion.Position).magnitude <= TetherBreak;
                        break;
                }
            }
            heroTethered = anyThread;
            // a lantern no Snuffer is pinching slowly finds its flame again
            for (int lamp = 0; lamp < lanternDim.Length; lamp++)
            {
                if (lanternDim[lamp] <= 0) continue;
                bool pinched = false;
                foreach (UnitState unit in units)
                    if (unit.Alive && unit.Faction == Faction.Enemy && unit.Waiting != null && unit.Waiting.Kind == WaitingEnemyKind.LampSnuffer && unit.Waiting.Step == 1 && unit.Waiting.Lantern == lamp) { pinched = true; break; }
                if (!pinched) lanternDim[lamp] = Mathf.Max(0, lanternDim[lamp] - deltaTime / 2.5f);
            }
        }

        private static float SegmentDistance(Vector2 point, Vector2 a, Vector2 b)
        {
            Vector2 segment = b - a;
            float t = segment.sqrMagnitude < 1e-6f ? 0 : Mathf.Clamp01(Vector2.Dot(point - a, segment) / segment.sqrMagnitude);
            return Vector2.Distance(point, a + segment * t);
        }

        private WaitingMark Mark(WaitingMarkKind kind, UnitState source, Vector2 a, Vector2 b, float radius, float delay, float duration, int index = 0)
        {
            var mark = new WaitingMark { Kind = kind, Source = source, A = a, B = b, Radius = radius, Created = Elapsed, At = Elapsed + delay, Until = Elapsed + delay + duration, NextTick = Elapsed + delay, Index = index };
            if (waitingMarks.Count >= 160) waitingMarks.RemoveAt(0);
            waitingMarks.Add(mark);
            return mark;
        }

        // ------------------------------------------------------------------ movement and moves
        private float WaitingSpeedScale(UnitState unit)
            => (unit.ChillUntil > Elapsed ? 0.55f : 1f) * (unit.StaggerUntil > Elapsed ? ImpactRules.StaggerSpeed : 1f) * KitEnemySpeed(unit) * EnemySleepScale(unit) * VisitorEnemyScale(unit);

        private void WaitingWalk(UnitState unit, Vector2 goal, float stop, float deltaTime, float pace = 1)
        {
            Vector2 offset = goal - unit.Position; float distance = offset.magnitude;
            if (distance > .001f) unit.Facing = offset / distance;
            if (distance <= stop) return;
            unit.Position = ClampPosition(unit.Position + unit.Facing * Mathf.Min(unit.Definition.Speed * pace * WaitingSpeedScale(unit) * deltaTime, distance - stop), unit.Definition.Radius);
        }

        private bool WaitingCanWindUp()
        {
            int charging = 0;
            foreach (UnitState other in units) if (other.Alive && (other.EvolutionReleaseAt > Elapsed || other.Variant != null && other.Variant.ReleaseAt > Elapsed)) charging++;
            return charging < 4;
        }

        /// <summary>The wind-up pose (crouched then charged frames) plays through the authored motion's evolution timing.</summary>
        private void WaitingWindup(UnitState unit, float seconds, Vector2 aim)
        {
            Vector2 direction = (aim - unit.Position).sqrMagnitude > .0001f ? (aim - unit.Position).normalized : unit.Facing;
            unit.AttackStarted = Elapsed; unit.AttackWindup = seconds; unit.AttackDirection = unit.Facing = direction;
            unit.EvolutionReleaseAt = unit.TelegraphUntil = unit.Waiting.ReleaseAt = Elapsed + seconds;
            events.Add(new CombatEvent(CombatEventKind.Windup, unit.Position, Faction.Enemy, direction: direction, weapon: unit.Definition.Weapon, subject: unit));
        }
        private void WaitingRelease(UnitState unit, float recovery)
        {
            unit.EvolutionReleaseAt = 0; unit.TelegraphUntil = 0; unit.Waiting.ReleaseAt = 0;
            unit.EvolutionRecoveryUntil = Elapsed + recovery;
            events.Add(new CombatEvent(CombatEventKind.Fire, unit.Position, Faction.Enemy, direction: unit.AttackDirection, weapon: unit.Definition.Weapon, subject: unit));
        }

        private bool MoveWaitingEnemy(UnitState unit, float deltaTime)
        {
            if (unit.Faction != Faction.Enemy || unit.Definition.IsBoss) return false;
            WaitingEnemyKind kind = unit.Definition.WaitingKind;
            if (kind == WaitingEnemyKind.None) return false;
            SyncWaiting();
            WaitingEnemyState state = unit.Waiting ??= new WaitingEnemyState();
            if (state.Kind != kind) { state.Kind = kind; state.Step = 0; state.NextAction = Elapsed + 1.2f + unit.Id % 5 * .25f; }
            if (EnemySleepScale(unit) <= 0) return true;
            if (unit.EvolutionRecoveryUntil > Elapsed && state.Step == 0) return true;
            bool evolved = unit.Definition.EvolutionStage > 0;
            Vector2 hero = Champion.Position, goal = OctoberEnemyTarget(unit);
            float reach = (hero - unit.Position).magnitude;
            bool ready = Elapsed >= state.NextAction && unit.EntranceUntil <= Elapsed;
            switch (kind)
            {
                case WaitingEnemyKind.SpareChair:
                    if (state.Step == 1)
                    {
                        if (Elapsed < state.ReleaseAt) return true;
                        // the seat snaps shut: still inside it, the hero is held 0.6 s
                        bool caught = (hero - state.Aim).magnitude <= state.Radius;
                        if (caught) HeroRootedUntil = Mathf.Max(HeroRootedUntil, Elapsed + SeatRoot);
                        foreach (WaitingMark mark in waitingMarks) if (mark.Source == unit && mark.Kind == WaitingMarkKind.SeatRing && !mark.Hit && mark.At <= Elapsed + .001f) mark.Hit = caught;
                        WaitingRelease(unit, .55f); state.Step = 0; state.NextAction = Elapsed + 5 + unit.Id % 3 * .4f;
                        return true;
                    }
                    if (ready && reach <= 3f && WaitingCanWindUp())
                    {
                        state.Step = 1; state.Aim = hero; state.Radius = evolved ? 1.1f : .9f;
                        Mark(WaitingMarkKind.SeatRing, unit, hero, unit.Position, state.Radius, SeatTelegraph, SeatRoot + .25f);
                        WaitingWindup(unit, SeatTelegraph, hero);
                        return true;
                    }
                    WaitingWalk(unit, goal, unit.Definition.Radius + Champion.Definition.Radius, deltaTime);
                    return true;

                case WaitingEnemyKind.WakingSitter:
                    if (state.Step == 1)
                    {
                        if (Elapsed < state.ReleaseAt) return true;
                        WaitingRelease(unit, .4f);
                        if (reach <= TetherBreak)
                        {
                            state.Step = 2; state.Tethered = true; state.TetherUntil = Elapsed + (evolved ? 7 : 6); state.NextTetherTick = Elapsed + 1;
                            Mark(WaitingMarkKind.Thread, unit, unit.Position, hero, .05f, 0, state.TetherUntil - Elapsed);
                            heroTethered = true;
                        }
                        else { state.Step = 0; state.NextAction = Elapsed + 2; }
                        return true;
                    }
                    if (state.Step == 2)
                    {
                        unit.Facing = reach > .001f ? (hero - unit.Position) / reach : unit.Facing;
                        if (reach > TetherBreak || Elapsed > state.TetherUntil)
                        {
                            state.Tethered = false; state.Step = 0; state.NextAction = Elapsed + 4;
                            foreach (WaitingMark mark in waitingMarks) if (mark.Source == unit && mark.Kind == WaitingMarkKind.Thread) mark.Until = Elapsed;
                            return true;
                        }
                        if (Elapsed >= state.NextTetherTick)
                        {
                            state.NextTetherTick = Elapsed + 1;
                            ApplyDamage(Champion, unit.Definition.Weapon.Damage * .3f, Faction.Enemy, unit.Definition.DisplayName + " thread", unit.Facing, cause: NewCause(unit.Id).Child("waking-thread"));
                        }
                        return true;
                    }
                    if (ready && reach <= 4.2f && WaitingCanWindUp()) { state.Step = 1; WaitingWindup(unit, .8f, hero); return true; }
                    WaitingWalk(unit, goal, 3.2f, deltaTime);
                    return true;

                case WaitingEnemyKind.InkNotary:
                {
                    UnitState ally = UnitWithId(state.TargetId);
                    if (ally != null && (!ally.Alive || ally.Faction != Faction.Enemy)) ally = null;
                    if (state.Step == 1 || state.Step == 2)
                    {
                        if (Elapsed < state.ReleaseAt) return true;
                        if (state.Step == 1 && ally != null)
                        {
                            ally.Waiting ??= new WaitingEnemyState();
                            ally.Waiting.Paid = true; ally.Waiting.PaidAt = Elapsed;
                        }
                        WaitingRelease(unit, .45f); state.Count++; state.Step = 0; state.NextAction = Elapsed + 3.5f + unit.Id % 3 * .3f;
                        return true;
                    }
                    if (ally == null || ally.Waiting != null && ally.Waiting.Paid) { ally = NotaryAlly(unit); state.TargetId = ally?.Id ?? 0; }
                    if (ready && WaitingCanWindUp())
                    {
                        if (ally != null && state.Count % 2 == 0 && (ally.Position - unit.Position).magnitude <= 2.2f)
                        { state.Step = 1; WaitingWindup(unit, .7f, ally.Position); return true; }
                        if (reach <= 7f && (ally == null || state.Count % 2 == 1))
                        {
                            state.Step = 2;
                            Mark(WaitingMarkKind.ClosedPatch, unit, hero, unit.Position, evolved ? 1f : .85f, .7f, .5f); // stamped once, never a lingering patch
                            WaitingWindup(unit, .7f, hero);
                            return true;
                        }
                    }
                    // it works from behind the others: just past its ally, away from the hero; alone, it keeps its distance
                    if (ally != null) WaitingWalk(unit, ally.Position + ((ally.Position - hero).sqrMagnitude > .0001f ? (ally.Position - hero).normalized : Vector2.up) * 1.1f, .2f, deltaTime);
                    else if (reach < 3.4f) WaitingWalk(unit, unit.Position + (unit.Position - hero).normalized * 2, .1f, deltaTime, .8f);
                    else WaitingWalk(unit, goal, 4f, deltaTime);
                    return true;
                }

                case WaitingEnemyKind.PartyLine:
                    // a telephone never walks: it rings, then pins its cord across the floor toward the hero
                    unit.Facing = reach > .001f ? (hero - unit.Position) / reach : unit.Facing;
                    if (state.Step == 1)
                    {
                        if (Elapsed < state.ReleaseAt) return true;
                        Mark(WaitingMarkKind.Cord, unit, unit.Position, state.Aim, .22f, 0, CordSeconds);
                        WaitingRelease(unit, .5f); state.Count++; state.Step = 0; state.NextAction = Elapsed + CordSeconds + 2.4f;
                        return true;
                    }
                    if (ready && reach <= 9f && reach > .8f && WaitingCanWindUp())
                    {
                        Vector2 along = (hero - unit.Position) / reach, side = new Vector2(-along.y, along.x) * (state.Count % 2 == 0 ? 1 : -1);
                        Vector2 near = hero + side * .35f;
                        Vector2 end = ClampPosition(unit.Position + (near - unit.Position).normalized * ((near - unit.Position).magnitude + 1.6f), .1f);
                        state.Step = 1; state.Aim = end;
                        Mark(WaitingMarkKind.CordPreview, unit, unit.Position, end, .22f, CordRing, 0);
                        WaitingWindup(unit, CordRing, near);
                    }
                    return true;

                case WaitingEnemyKind.DoorKnocker:
                    if (state.Step == 1)
                    {
                        // the leap: up and over, onto the marked landing spot
                        float progress = Mathf.Clamp01((Elapsed - state.StepAt) / .6f);
                        unit.Position = ClampPosition(Vector2.Lerp(state.Origin, state.Aim, Mathf.SmoothStep(0, 1, progress)), unit.Definition.Radius);
                        unit.BodyLift = Mathf.Sin(progress * Mathf.PI) * .9f;
                        if (progress < 1) return true;
                        unit.BodyLift = 0; state.Step = 2; state.StepAt = Elapsed; state.Count = 0;
                        for (int knock = 0; knock < 3; knock++)
                            Mark(WaitingMarkKind.KnockRing, unit, unit.Position, unit.Position, KnockRadius(knock, evolved), KnockBeat * (knock + 1), .12f, knock);
                        WaitingWindup(unit, KnockBeat, hero);
                        return true;
                    }
                    if (state.Step == 2)
                    {
                        if (Elapsed < state.StepAt + KnockBeat * (state.Count + 1)) return true;
                        Danger(ExpandedFieldKind.Shockwave, unit, unit.Position, KnockRadius(state.Count, evolved), 0, .55f);
                        state.KnockAt[state.Count] = Elapsed; state.Count++;
                        if (state.Count < 3) WaitingWindup(unit, KnockBeat, hero);
                        else { WaitingRelease(unit, .6f); state.Step = 0; state.NextAction = Elapsed + 5.5f + unit.Id % 3 * .4f; }
                        return true;
                    }
                    if (ready && reach <= 5f && WaitingCanWindUp())
                    {
                        Vector2 back = reach > .001f ? (unit.Position - hero) / reach : Vector2.up;
                        state.Step = 1; state.StepAt = Elapsed; state.Origin = unit.Position;
                        state.Aim = ClampPosition(hero + back * 1.6f, unit.Definition.Radius);
                        unit.Facing = -back;
                        return true;
                    }
                    WaitingWalk(unit, goal, unit.Definition.Radius + Champion.Definition.Radius, deltaTime);
                    return true;

                case WaitingEnemyKind.LampSnuffer:
                {
                    if (state.Lantern < 0 || !WaitingLantern(state.Lantern, out Vector2 lamp) || state.Step == 0 && lanternDim[state.Lantern] >= 1)
                    {
                        state.Lantern = NearestLitLantern(unit.Position); state.Step = 0;
                        if (state.Lantern < 0) { WaitingWalk(unit, goal, unit.Definition.Radius + Champion.Definition.Radius, deltaTime); return true; }
                        WaitingLantern(state.Lantern, out lamp);
                    }
                    Vector2 perch = lamp + Vector2.up * .35f;
                    if (state.Step == 0)
                    {
                        WaitingWalk(unit, perch, .05f, deltaTime, .9f);
                        if ((unit.Position - perch).magnitude <= .5f) { state.Step = 1; state.StepAt = Elapsed; }
                        return true;
                    }
                    // pinching the flame: the lantern dims until the Snuffer falls
                    unit.Facing = (lamp - unit.Position).sqrMagnitude > .0001f ? (lamp - unit.Position).normalized : unit.Facing;
                    lanternDim[state.Lantern] = Mathf.Min(1, lanternDim[state.Lantern] + deltaTime / (evolved ? 3.5f : 5f));
                    if ((unit.Position - perch).magnitude > .8f) state.Step = 0;
                    return true;
                }

                case WaitingEnemyKind.LedgerHound:
                    if (state.Step == 1)
                    {
                        // circling the hero, laying a tally mark every beat
                        state.Angle += state.Spin * 75f * deltaTime;
                        Vector2 lap = hero + new Vector2(Mathf.Cos(state.Angle * Mathf.Deg2Rad), Mathf.Sin(state.Angle * Mathf.Deg2Rad)) * 2.6f;
                        WaitingWalk(unit, ClampPosition(lap, unit.Definition.Radius), 0, deltaTime, 3f);
                        if (state.Count < 4 && Elapsed >= state.StepAt + TallyBeat * state.Count)
                        {
                            state.Tallies[state.Count] = unit.Position;
                            Vector2 outward = (unit.Position - hero).sqrMagnitude > .0001f ? (unit.Position - hero).normalized : Vector2.up;
                            Mark(WaitingMarkKind.Tally, unit, unit.Position - outward * .3f, unit.Position + outward * .3f, .06f, 0, 6, state.Count);
                            state.Count++;
                        }
                        if (state.Count >= 4 && Elapsed >= state.StepAt + TallyBeat * 4)
                        {
                            // the fifth stroke: a diagonal slash along the marked line, first tally to last
                            Vector2 axis = state.Tallies[3] - state.Tallies[0];
                            Vector2 dir = axis.sqrMagnitude > .0001f ? axis.normalized : Vector2.right;
                            state.Origin = state.Tallies[0] - dir * .8f; state.Aim = state.Tallies[3] + dir * .8f;
                            state.Step = 2;
                            Mark(WaitingMarkKind.SlashLine, unit, state.Origin, state.Aim, .38f, .7f, 0);
                            WaitingWindup(unit, .7f, state.Aim);
                        }
                        return true;
                    }
                    if (state.Step == 2)
                    {
                        if (Elapsed < state.ReleaseAt) return true;
                        WaitingMark slash = Mark(WaitingMarkKind.Slash, unit, state.Origin, state.Aim, .38f, 0, .45f);
                        state.Slashes++;
                        if (SegmentDistance(hero, state.Origin, state.Aim) <= .38f + Champion.Definition.Radius)
                        {
                            slash.Hit = true;
                            ApplyDamage(Champion, unit.Definition.Weapon.Damage * 1.6f, Faction.Enemy, unit.Definition.DisplayName + " slash", (state.Aim - state.Origin).normalized, cause: NewCause(unit.Id).Child("ledger-slash"));
                        }
                        unit.Position = ClampPosition(state.Aim, unit.Definition.Radius);
                        WaitingRelease(unit, .6f); state.Step = 0; state.Count = 0; state.NextAction = Elapsed + 5 + unit.Id % 3 * .4f;
                        return true;
                    }
                    if (ready && reach >= 1.6f && reach <= 3.6f && WaitingCanWindUp())
                    {
                        state.Step = 1; state.StepAt = Elapsed; state.Count = 0;
                        Vector2 from = unit.Position - hero;
                        state.Angle = Mathf.Atan2(from.y, from.x) * Mathf.Rad2Deg; state.Spin = unit.Id % 2 == 0 ? 1 : -1;
                        return true;
                    }
                    WaitingWalk(unit, goal, 2.6f, deltaTime, 1.1f);
                    return true;

                default: // a Sewn Letter and its pieces simply come for the hero
                    WaitingWalk(unit, goal, unit.Definition.Radius + Champion.Definition.Radius, deltaTime);
                    return true;
            }
        }

        private static float KnockRadius(int knock, bool evolved) => (knock >= 2 ? 2.6f : 1.7f) * (evolved ? 1.12f : 1);

        private UnitState NotaryAlly(UnitState notary)
        {
            UnitState best = null; float bestDistance = 36;
            foreach (UnitState other in units)
            {
                if (other == notary || !other.Alive || other.Faction != Faction.Enemy || other.Definition.IsBoss || other.Definition.WaitingKind == WaitingEnemyKind.InkNotary) continue;
                if (other.Waiting != null && other.Waiting.Paid) continue;
                float distance = (other.Position - notary.Position).sqrMagnitude;
                if (distance < bestDistance) { best = other; bestDistance = distance; }
            }
            return best;
        }

        private int NearestLitLantern(Vector2 from)
        {
            int best = -1; float bestDistance = float.PositiveInfinity;
            for (int index = 0; index < 8; index++)
            {
                if (!WaitingLantern(index, out Vector2 at) || lanternDim[index] >= 1) continue;
                float distance = (at - from).sqrMagnitude;
                if (distance < bestDistance) { best = index; bestDistance = distance; }
            }
            return best;
        }
    }
}
