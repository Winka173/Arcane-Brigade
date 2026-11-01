using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    // The Waiting Room (waves 1-4; Docs/Map_Mechanics_2026-10-25.md section 1). Requirement 25/10/2026: each map's state, tunings
    // and rules live in its own file; MapEnvironmentCombat and MapBoosts only dispatch to the hooks below. Five slots:
    // queue tickets (boost), ceiling dust (atmosphere), impatient waiters (enemy), the number board (unique 1), the wall clock
    // (unique 2). The sliding chairs are gone.

    public sealed partial class MapEnvironmentState
    {
        // ---- the number board and its four doors
        public Vector2 Board;
        public readonly Vector2[] Doors = new Vector2[4];
        /// <summary>The door called now (-1 none) and the last one; the number on the board (1-4: it stays up until the next call,
        /// and a queue ticket with this number gives double).</summary>
        public int CalledDoor = -1, LastDoor = -1, BoardNumber = 1, PackLeft;
        public float CallAt = -100, NextCall, NextPackMember;
        /// <summary>The called door's body: shots and blades break it during the 3-2-1 countdown.</summary>
        public readonly StruckProp RoomDoorProp = new StruckProp();
        public bool RoomDoorBroken; public float RoomDoorBrokenAt = -100;
        /// <summary>When the countdown ends and the pack pours out (a door broken in time keeps it behind).</summary>
        public float OpenAt => CallAt + MapEnvironmentRules.DoorCall;
        public float CloseAt => OpenAt + MapEnvironmentRules.DoorOpen;

        // ---- queue tickets (colours: 0 red, 1 yellow, 2 grey, 3 white, 4 blue)
        public sealed class RoomTicket { public Vector2 Position; public float Born, Until; public int Colour, Number; }
        public readonly List<RoomTicket> RoomTickets = new List<RoomTicket>();
        public float RoomNextTicket;
        /// <summary>The last ticket taken (the presentation's moment): when, where, its colour and whether its number matched.</summary>
        public float RoomTicketTakenAt = -100; public Vector2 RoomTicketTakenWhere; public int RoomTicketTakenColour; public bool RoomTicketTakenMatched;

        // ---- ceiling dust: patches that glide to new spots every 15 s
        public sealed class RoomDustPatch
        {
            public Vector2 From, To; public float MovedAt = -100; public int Seed;
            public Vector2 At(float t) => Vector2.Lerp(From, To, Mathf.SmoothStep(0, 1, Mathf.Clamp01((t - MovedAt) / MapEnvironmentRules.RoomDustGlide)));
        }
        public readonly List<RoomDustPatch> RoomDust = new List<RoomDustPatch>();
        public float RoomNextDust;
        public bool RoomInDust(Vector2 p, float t)
        {
            for (int i = 0; i < RoomDust.Count; i++)
            {
                Vector2 d = p - RoomDust[i].At(t);
                if (d.x * d.x / (MapEnvironmentRules.RoomDustRadiusX * MapEnvironmentRules.RoomDustRadiusX) + d.y * d.y / (MapEnvironmentRules.RoomDustRadiusY * MapEnvironmentRules.RoomDustRadiusY) < 1) return true;
            }
            return false;
        }

        // ---- impatient waiters
        public sealed class RoomWaiter { public Vector2 Anchor; public float Since, AngryAt = -100, AngryUntil = -100; }
        public readonly Dictionary<int, RoomWaiter> RoomWaiters = new Dictionary<int, RoomWaiter>();
        public float RoomNextSweep;
        public bool RoomAngry(int id, float t) => RoomWaiters.TryGetValue(id, out RoomWaiter w) && w.AngryUntil > t;

        // ---- the wall clock: the hand goes round every 30 s; at twelve it glows for 6 s
        public Vector2 RoomClock; public float RoomClockStart, RoomClockGlowAt = -100, RoomClockStruckAt = -100, RoomClockStunUntil = -100;
        public int RoomClockLap; public bool RoomClockStruck;
        /// <summary>0..1 round the dial from twelve.</summary>
        public float RoomClockHand(float t) => Mathf.Repeat((t - RoomClockStart) / MapEnvironmentRules.RoomClockTurn, 1);
        public bool RoomClockGlowing(float t) => !RoomClockStruck && t >= RoomClockGlowAt && t < RoomClockGlowAt + MapEnvironmentRules.RoomClockGlow;

        // requirement 25/10/2026: the sliding chairs are removed. The type and its (always empty) list stay only because
        // MapEnvironmentCombat's blocker loop and the play-test checks still name them.
    }

    public static partial class MapEnvironmentRules
    {
        // the number board: a call every 15 s, a 3-2-1 countdown, a door of about three ordinary enemies' health
        public const float DoorEvery = 15, DoorCall = 3, DoorOpen = 2.2f, RoomDoorHealth = 3, RoomDoorRadius = .95f, RoomDoorMiddle = 1.05f, RoomPackGap = .16f;
        public const int RoomPackMin = 6, RoomPackMax = 10, RoomDoorGoldMin = 3, RoomDoorGoldMax = 8;
        // queue tickets: one every 12 s near the hero, lying 8 s; 10 s each (the same colour again adds 10 s, at most 20 s ahead)
        public const float RoomTicketEvery = 12, RoomTicketLies = 8, RoomTicketSeconds = 10, RoomTicketReach = .55f;
        public const float RoomTicketDamage = .2f, RoomTicketHaste = .2f, RoomTicketArmor = 3, RoomTicketGold = 1, RoomTicketSpeed = .15f;
        // ceiling dust: 3-4 patches moving every 15 s; enemies inside 15% slower and attacking 10% slower
        public const float RoomDustEvery = 15, RoomDustGlide = 2.5f, RoomDustRadiusX = 1.7f, RoomDustRadiusY = 1.1f, RoomDustPace = .85f, RoomDustCycle = 1 / .9f;
        // impatient waiters: barely moved for 4 s, angry for 6 s (+25% pace, +20% damage; take +25%, double gold)
        public const float RoomWaitStill = 4, RoomWaitAngry = 6, RoomWaitBudge = .6f, RoomAngryPace = 1.25f, RoomAngryDamage = 1.2f, RoomAngryTaken = 1.25f;
        // the wall clock: round every 30 s, glowing 6 s at twelve; stand within 2 units: every enemy stunned 5 s (bosses 1.5 s)
        public const float RoomClockTurn = 30, RoomClockGlow = 6, RoomClockReachX = 2, RoomClockReachY = 1.3f, RoomClockStun = 5, RoomClockBossStun = 1.5f;
        /// <summary>Kept for the play-test checks only (the chairs are removed).</summary>
    }

    public sealed partial class CombatSimulation
    {
        private const int RoomDoorVictim = -7101;
        private static readonly string[] RoomTicketSources = { "ticket-red", "ticket-yellow", "ticket-grey", "ticket-white", "ticket-blue" };
        private readonly List<int> roomGone = new List<int>();

        private void InitWaitingRoom(MapEnvironmentState s)
        {
            Vector2 a = s.Anchor;
            s.Board = a + new Vector2(0, 4.4f);
            s.Doors[0] = a + new Vector2(-8, 3); s.Doors[1] = a + new Vector2(8, 3); s.Doors[2] = a + new Vector2(-8, -4.2f); s.Doors[3] = a + new Vector2(8, -4.2f);
            s.RoomClock = a + new Vector2(4.6f, 4.2f); // on the wall beside the board
            s.RoomClockStart = 20 - MapEnvironmentRules.RoomClockTurn; // the hand first reaches twelve 20 s in, then every 30 s
            s.RoomClockLap = Mathf.FloorToInt((0 - s.RoomClockStart) / MapEnvironmentRules.RoomClockTurn);
            s.NextCall = 6; s.BoardNumber = 1 + mapRandom.Next(4);
            s.RoomNextTicket = 3;
            if (s.Side == MapSide.RoomDust) RoomLayDust(s); // the dust lies only on its own waves (MapPacing)
            s.RoomNextDust = MapEnvironmentRules.RoomDustEvery;
        }

        private void StepWaitingRoom(float deltaTime)
        {
            RoomStepBoard();
            MapSideHold(MapSide.RoomTickets, ref mapEnv.RoomNextTicket);
            RoomStepTickets();
            MapSideHold(MapSide.RoomDust, ref mapEnv.RoomNextDust);
            if (Elapsed >= mapEnv.RoomNextDust) { mapEnv.RoomNextDust = Elapsed + MapEnvironmentRules.RoomDustEvery; RoomPlaceDust(mapEnv, false); }
            if (MapSideOn(MapSide.RoomWaiters)) RoomStepWaiters();
            if (MapSideShown(MapSide.RoomClock)) RoomStepClock(); // the hero's helper: it keeps running while a boss is out
        }

        /// <summary>A stat the hero gets while standing somewhere on this map (MapBoosts.MapStandingBoost).</summary>
        private float WaitingRoomStandingBoost(MapStat stat) => 0;
        /// <summary>How this map changes one enemy along one axis (1 = unchanged): ceiling dust slows it 15% and its attacks 10%;
        /// an impatient (angry) waiter is 25% quicker and hits 20% harder, but takes 25% more damage.</summary>
        private float WaitingRoomEnemyFactor(UnitState unit, MapEnemyAxis axis)
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            bool angry = !unit.Definition.IsBoss && s.RoomAngry(unit.Id, t);
            switch (axis)
            {
                case MapEnemyAxis.Speed: return (s.RoomInDust(unit.Position, t) ? MapEnvironmentRules.RoomDustPace : 1) * (angry ? MapEnvironmentRules.RoomAngryPace : 1);
                case MapEnemyAxis.Cycle: return s.RoomInDust(unit.Position, t) ? MapEnvironmentRules.RoomDustCycle : 1;
                case MapEnemyAxis.DamageDealt: return angry ? MapEnvironmentRules.RoomAngryDamage : 1;
                case MapEnemyAxis.DamageTaken: return angry ? MapEnvironmentRules.RoomAngryTaken : 1;
                default: return 1;
            }
        }
        /// <summary>The attack cooldown factor of the hero or a friend on this map (below 1 = faster).</summary>
        private float WaitingRoomAllyCycle(UnitState unit) => 1;
        /// <summary>The hero's pace on this map beyond the move-speed boosts.</summary>
        private float WaitingRoomHeroPace() => 1;
        /// <summary>A fallen enemy's coins on this map: a door's pack drops no gold; an angry waiter drops double.</summary>
        private int WaitingRoomCoins(UnitState target, int coins) => target.DoorPack ? 0 : mapEnv.RoomAngry(target.Id, Elapsed) ? coins * 2 : coins;

        // ------------------------------------------------------------------ the number board
        private void RoomStepBoard()
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            if (t >= s.NextCall && s.CalledDoor < 0)
            {
                // one of the two doors nearest the hero (never the same twice running), so the countdown can be answered
                int first = -1, second = -1; float d1 = float.MaxValue, d2 = float.MaxValue;
                for (int d = 0; d < 4; d++)
                {
                    if (d == s.LastDoor) continue;
                    float dd = (s.Doors[d] - Champion.Position).sqrMagnitude;
                    if (dd < d1) { second = first; d2 = d1; first = d; d1 = dd; }
                    else if (dd < d2) { second = d; d2 = dd; }
                }
                int door = second >= 0 && mapRandom.Next(3) == 0 ? second : first;
                s.CalledDoor = s.LastDoor = door; s.BoardNumber = door + 1; s.CallAt = t; s.NextCall = t + MapEnvironmentRules.DoorEvery;
                s.RoomDoorBroken = false; s.RoomDoorBrokenAt = -100;
                s.PackLeft = MapEnvironmentRules.RoomPackMin + mapRandom.Next(MapEnvironmentRules.RoomPackMax - MapEnvironmentRules.RoomPackMin + 1);
                s.NextPackMember = s.OpenAt;
                s.RoomDoorProp.Reset(s.Doors[door] + Vector2.up * MapEnvironmentRules.RoomDoorMiddle, MapEnvironmentRules.RoomDoorRadius, MapEnvironmentRules.RoomDoorHealth * RoomOrdinaryHealth());
                s.Shown.Add(MapIdentity.Element.Board);
            }
            if (s.CalledDoor < 0) return;
            Vector2 at = s.Doors[s.CalledDoor];
            if (!s.RoomDoorBroken && t < s.OpenAt)
            {
                RoomStrikeProp(s.RoomDoorProp, RoomDoorVictim);
                if (s.RoomDoorProp.Health <= 0)
                {
                    // broken in time: no pack, 3-8 gold and a ticket carrying the called number (so it gives double)
                    s.RoomDoorBroken = true; s.RoomDoorBrokenAt = t; s.PackLeft = 0;
                    RoomSpillGold(at + Vector2.down * .6f, MapEnvironmentRules.RoomDoorGoldMin + mapRandom.Next(MapEnvironmentRules.RoomDoorGoldMax - MapEnvironmentRules.RoomDoorGoldMin + 1), 3);
                    RoomAddTicket(ClampPosition(at + new Vector2(MapNext(-.4f, .4f), -1.1f), .6f), s.BoardNumber);
                }
            }
            if (!s.RoomDoorBroken && t >= s.OpenAt && s.PackLeft > 0 && t >= s.NextPackMember)
            {
                // not in time: the pack pours out, with no gold
                int alive = 0; foreach (UnitState u in StableUnits) if (u.Alive && u.Faction == Faction.Enemy) alive++;
                if (alive < catalog.EnemyBudget)
                {
                    Vector2 spot = ClampPosition(at + new Vector2(MapNext(-.55f, .55f), -.7f), .5f);
                    UnitDefinition herd = catalog.EnemyPool != null && catalog.EnemyPool.Length > 0 ? SwarmDefinition() : catalog.MeleeEnemy;
                    if (herd != null)
                    {
                        UnitState spawned = Spawn(herd, Faction.Enemy, spot);
                        spawned.SwarmMinion = true; spawned.DoorPack = true; spawned.Health *= .45f;
                        AssignSwarmRole(spawned, true);
                    }
                }
                s.PackLeft--; s.NextPackMember = t + MapEnvironmentRules.RoomPackGap;
            }
            if (t >= s.CloseAt) s.CalledDoor = -1;
        }

        // ------------------------------------------------------------------ queue tickets
        private void RoomAddTicket(Vector2 at, int number)
        {
            MapEnvironmentState s = mapEnv;
            s.RoomTickets.Add(new MapEnvironmentState.RoomTicket { Position = at, Born = Elapsed, Until = Elapsed + MapEnvironmentRules.RoomTicketLies, Colour = mapRandom.Next(5), Number = number });
        }
        private void RoomStepTickets()
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            if (t >= s.RoomNextTicket)
            {
                s.RoomNextTicket = t + MapEnvironmentRules.RoomTicketEvery;
                float angle = MapNext(0, Mathf.PI * 2), reach = MapNext(2, 3.4f);
                RoomAddTicket(ClampPosition(Champion.Position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * .75f) * reach, .7f), 1 + mapRandom.Next(4));
            }
            float grab = Champion.Definition.Radius + MapEnvironmentRules.RoomTicketReach;
            for (int i = s.RoomTickets.Count - 1; i >= 0; i--)
            {
                MapEnvironmentState.RoomTicket ticket = s.RoomTickets[i];
                if (t >= ticket.Until) { s.RoomTickets.RemoveAt(i); continue; }
                if ((ticket.Position - Champion.Position).sqrMagnitude > grab * grab) continue;
                bool matched = ticket.Number == s.BoardNumber; // a ticket matching the number on the board gives double
                float value = RoomTicketValue(ticket.Colour) * (matched ? 2 : 1);
                GrantMapBoost(RoomTicketStat(ticket.Colour), value, MapEnvironmentRules.RoomTicketSeconds, RoomTicketSources[ticket.Colour], matched ? "No. " + ticket.Number + " ×2" : null);
                s.RoomTicketTakenAt = t; s.RoomTicketTakenWhere = ticket.Position; s.RoomTicketTakenColour = ticket.Colour; s.RoomTicketTakenMatched = matched;
                s.RoomTickets.RemoveAt(i);
            }
        }
        public static MapStat RoomTicketStat(int colour) => colour switch { 0 => MapStat.Damage, 1 => MapStat.AttackSpeed, 2 => MapStat.Armor, 3 => MapStat.Gold, _ => MapStat.MoveSpeed };
        private static float RoomTicketValue(int colour) => colour switch
        {
            0 => MapEnvironmentRules.RoomTicketDamage, 1 => MapEnvironmentRules.RoomTicketHaste, 2 => MapEnvironmentRules.RoomTicketArmor,
            3 => MapEnvironmentRules.RoomTicketGold, _ => MapEnvironmentRules.RoomTicketSpeed
        };

        // ------------------------------------------------------------------ ceiling dust
        private void RoomLayDust(MapEnvironmentState s)
        {
            int patches = 3 + mapRandom.Next(2);
            for (int i = 0; i < patches; i++) s.RoomDust.Add(new MapEnvironmentState.RoomDustPatch { Seed = mapRandom.Next(1000) });
            RoomPlaceDust(s, true);
        }
        /// <summary>Sends every patch to a new spot round the hero (apart from each other); the first time they simply appear.</summary>
        private void RoomPlaceDust(MapEnvironmentState s, bool first)
        {
            float t = Elapsed;
            for (int i = 0; i < s.RoomDust.Count; i++)
            {
                MapEnvironmentState.RoomDustPatch patch = s.RoomDust[i];
                Vector2 best = Champion.Position; float bestGap = -1;
                for (int k = 0; k < 10; k++)
                {
                    float angle = MapNext(0, Mathf.PI * 2), reach = MapNext(2.2f, 6.2f);
                    Vector2 c = ClampPosition(Champion.Position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * .7f) * reach, 1.4f);
                    float gap = float.MaxValue;
                    for (int j = 0; j < i; j++) gap = Mathf.Min(gap, (s.RoomDust[j].To - c).magnitude);
                    if (gap > bestGap) { bestGap = gap; best = c; }
                    if (gap >= 3.4f) break;
                }
                patch.From = first ? best : patch.At(t); patch.To = best; patch.MovedAt = first ? -100 : t;
            }
        }

        // ------------------------------------------------------------------ impatient waiters
        private void RoomStepWaiters()
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            float budge = MapEnvironmentRules.RoomWaitBudge * MapEnvironmentRules.RoomWaitBudge;
            foreach (UnitState u in StableUnits)
            {
                if (!u.Alive || u.Faction != Faction.Enemy || u.Definition.IsBoss) continue;
                if (!s.RoomWaiters.TryGetValue(u.Id, out MapEnvironmentState.RoomWaiter w)) { s.RoomWaiters[u.Id] = new MapEnvironmentState.RoomWaiter { Anchor = u.Position, Since = t }; continue; }
                if (w.AngryUntil > t) continue;
                // held still by a stun, a freeze or its entrance is not waiting: the count starts again
                if (u.SleepUntil > t || u.FrozenUntil > t || u.EntranceUntil > t || (u.Position - w.Anchor).sqrMagnitude > budge) { w.Anchor = u.Position; w.Since = Mathf.Max(t, w.AngryUntil); continue; }
                if (t - w.Since >= MapEnvironmentRules.RoomWaitStill)
                {
                    w.AngryAt = t; w.AngryUntil = t + MapEnvironmentRules.RoomWaitAngry;
                    w.Anchor = u.Position; w.Since = w.AngryUntil; // after its temper it waits 4 s again before the next
                }
            }
            if (t >= s.RoomNextSweep)
            {
                s.RoomNextSweep = t + 3;
                roomGone.Clear();
                foreach (KeyValuePair<int, MapEnvironmentState.RoomWaiter> pair in s.RoomWaiters) { UnitState u = UnitWithId(pair.Key); if (u == null || !u.Alive) roomGone.Add(pair.Key); }
                foreach (int id in roomGone) s.RoomWaiters.Remove(id);
            }
        }

        // ------------------------------------------------------------------ the wall clock
        private void RoomStepClock()
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            int lap = Mathf.FloorToInt((t - s.RoomClockStart) / MapEnvironmentRules.RoomClockTurn);
            if (lap > s.RoomClockLap) { s.RoomClockLap = lap; s.RoomClockGlowAt = t; s.RoomClockStruck = false; }
            if (!s.RoomClockGlowing(t)) return;
            Vector2 d = Champion.Position - s.RoomClock;
            if (d.x * d.x / (MapEnvironmentRules.RoomClockReachX * MapEnvironmentRules.RoomClockReachX) + d.y * d.y / (MapEnvironmentRules.RoomClockReachY * MapEnvironmentRules.RoomClockReachY) >= 1) return;
            // the hero by the glowing clock: time stands still for every enemy (bosses only briefly)
            s.RoomClockStruck = true; s.RoomClockStruckAt = t; s.RoomClockStunUntil = t + MapEnvironmentRules.RoomClockStun;
            foreach (UnitState u in StableUnits)
                if (u.Alive && u.Faction == Faction.Enemy) RoomStun(u, MapEnvironmentRules.RoomClockStun, MapEnvironmentRules.RoomClockBossStun);
            MapSay("Time stands still", new Color(1f, .93f, .72f));
        }

        /// <summary>For captures: the board calling a cracked door, tickets (one matching), the dust, an angry waiter and the
        /// glowing clock gathered round the hero, so one frame shows them all.</summary>
        private void WaitingRoomCaptureFixture()
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed; Vector2 h = Champion.Position;
            if (s.RoomDust.Count == 0) RoomLayDust(s);
            s.Board = h + new Vector2(-.6f, 2.4f);
            s.Doors[0] = h + new Vector2(-5.6f, 1.4f); s.Doors[1] = h + new Vector2(5.4f, 1.4f);
            s.Doors[2] = h + new Vector2(-6.4f, -3.4f); s.Doors[3] = h + new Vector2(6.2f, -3.4f);
            s.CalledDoor = s.LastDoor = 1; s.BoardNumber = 2; s.CallAt = t - 1.2f; s.PackLeft = 7; s.NextPackMember = s.OpenAt; s.NextCall = t + 30; s.RoomDoorBroken = false;
            s.RoomDoorProp.Reset(s.Doors[1] + Vector2.up * MapEnvironmentRules.RoomDoorMiddle, MapEnvironmentRules.RoomDoorRadius, MapEnvironmentRules.RoomDoorHealth * RoomOrdinaryHealth());
            s.RoomDoorProp.Health = s.RoomDoorProp.MaxHealth * .4f; s.RoomDoorProp.HitAt = t - .05f; s.RoomDoorProp.HitFrom = h;
            s.RoomTickets.Clear();
            s.RoomTickets.Add(new MapEnvironmentState.RoomTicket { Position = h + new Vector2(1.6f, -.8f), Born = t - 1, Until = t + 7, Colour = 0, Number = 2 });
            s.RoomTickets.Add(new MapEnvironmentState.RoomTicket { Position = h + new Vector2(-1.8f, -1.2f), Born = t - 3, Until = t + 5, Colour = 4, Number = 3 });
            s.RoomNextTicket = t + 30;
            for (int i = 0; i < s.RoomDust.Count; i++) { MapEnvironmentState.RoomDustPatch p = s.RoomDust[i]; p.From = p.To = h + new Vector2(-3.4f + i * 3.1f, i % 2 == 0 ? -2.4f : 2.2f); p.MovedAt = -100; }
            s.RoomNextDust = t + 30;
            foreach (UnitState u in StableUnits)
                if (u.Alive && u.Faction == Faction.Enemy && !u.Definition.IsBoss && (u.Position - h).sqrMagnitude < 25)
                { s.RoomWaiters[u.Id] = new MapEnvironmentState.RoomWaiter { Anchor = u.Position, Since = t + 6, AngryAt = t - .4f, AngryUntil = t + 5 }; break; }
            s.RoomClock = h + new Vector2(2.6f, 2.2f);
            s.RoomClockStart = t - 2 - MapEnvironmentRules.RoomClockTurn; s.RoomClockLap = Mathf.FloorToInt((t - s.RoomClockStart) / MapEnvironmentRules.RoomClockTurn);
            s.RoomClockGlowAt = t - 2; s.RoomClockStruck = false;
            s.Shown.Add(MapIdentity.Element.Board);
        }
    }
}
