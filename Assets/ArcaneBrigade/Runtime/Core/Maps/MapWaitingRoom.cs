using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    // The Waiting Room (waves 1-4; Docs/Map_Mechanics_2026-10-25.md section 1). Requirement 25/10/2026: each map's state, tunings
    // and rules live in its own file; MapEnvironmentCombat and MapBoosts only dispatch to the hooks below.

    public sealed partial class MapEnvironmentState
    {
        // Waiting Room: the Number Board and its four doors; the restless chairs
        public Vector2 Board;
        public readonly Vector2[] Doors = new Vector2[4];
        public int CalledDoor = -1, LastDoor = -1, PackLeft;
        public float CallAt = -100, NextCall, NextPackMember;
        /// <summary>0..1: the hero standing at the called door shuts it (requirement 23/10/2026); ShutAt marks the last one shut.</summary>
        public float DoorShut, ShutAt = -100; public int ShutDoor = -1;
        public float OpenAt => CallAt + MapEnvironmentRules.DoorCall;
        public float CloseAt => OpenAt + MapEnvironmentRules.DoorOpen;
        public sealed class Chair { public Vector2 Home, Position, From, To; public float WarnAt = -100, SlideAt = -100, SlideEnd = -100, StayUntil = -100, ReturnEnd = -100; public readonly HashSet<int> Tripped = new HashSet<int>(); }
        public readonly List<Chair> Chairs = new List<Chair>();
        public float NextChair;
        public int ChairTurn;
    }

    public static partial class MapEnvironmentRules
    {
        public const float DoorEvery = 15, DoorCall = 2.4f, DoorOpen = 2.2f, ChairEvery = 8, ChairWarn = .9f, ChairSlide = .7f, ChairStay = 4, ChairReturn = 1, ChairRadius = .55f;
        // requirement 23/10/2026: shut a door by standing at it
        public const float DoorReach = 1.8f, DoorShutSeconds = 1.5f;
    }

    public sealed partial class CombatSimulation
    {
        private void InitWaitingRoom(MapEnvironmentState s)
        {
            Vector2 a = s.Anchor;
            s.Board = a + new Vector2(0, 4.4f);
            s.Doors[0] = a + new Vector2(-8, 3); s.Doors[1] = a + new Vector2(8, 3); s.Doors[2] = a + new Vector2(-8, -4.2f); s.Doors[3] = a + new Vector2(8, -4.2f);
            foreach (Vector2 home in new[] { new Vector2(-3.6f, 1.6f), new Vector2(4, -1.4f), new Vector2(-.6f, -3.6f) })
                s.Chairs.Add(new MapEnvironmentState.Chair { Home = a + home, Position = a + home });
            s.NextCall = 6; s.NextChair = 3.5f;
        }

        // requirement 23/10/2026: nothing on a map blocks or pushes anyone - the sliding chairs stay where they stand as scenery
        private void StepWaitingRoom(float deltaTime) => StepNumberBoard(deltaTime);

        /// <summary>A stat the hero gets while standing somewhere on this map (MapBoosts.MapStandingBoost).</summary>
        private float WaitingRoomStandingBoost(MapStat stat) => 0;
        /// <summary>How this map changes one enemy along one axis (1 = unchanged; MapBoosts.MapEnemyFactor).</summary>
        private float WaitingRoomEnemyFactor(UnitState unit, MapEnemyAxis axis) => 1;
        /// <summary>The attack cooldown factor of the hero or a friend on this map (below 1 = faster).</summary>
        private float WaitingRoomAllyCycle(UnitState unit) => 1;
        /// <summary>The hero's pace on this map beyond the move-speed boosts.</summary>
        private float WaitingRoomHeroPace() => 1;
        /// <summary>A fallen enemy's coins on this map: a door's pack drops no gold.</summary>
        private int WaitingRoomCoins(UnitState target, int coins) => target.DoorPack ? 0 : coins;

        private void StepNumberBoard(float deltaTime)
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            if (t >= s.NextCall && s.CalledDoor < 0)
            {
                int door; do door = mapRandom.Next(4); while (door == s.LastDoor);
                s.CalledDoor = s.LastDoor = door; s.CallAt = t; s.NextCall = t + MapEnvironmentRules.DoorEvery;
                s.PackLeft = 3 + (catalog.RunWave - MapIdentity.FirstWave(catalog.RunWave)) / 2; s.NextPackMember = s.OpenAt;
                s.Shown.Add(MapIdentity.Element.Board);
            }
            if (s.CalledDoor < 0) { s.DoorShut = 0; return; }
            // the hero standing at the called door shuts it: the rest of the pack stays behind it
            if (s.PackLeft > 0 && (Champion.Position - s.Doors[s.CalledDoor]).sqrMagnitude < MapEnvironmentRules.DoorReach * MapEnvironmentRules.DoorReach)
            {
                s.DoorShut = Mathf.Min(1, s.DoorShut + deltaTime / MapEnvironmentRules.DoorShutSeconds);
                if (s.DoorShut >= 1) { s.ShutDoor = s.CalledDoor; s.ShutAt = t; s.PackLeft = 0; s.CalledDoor = -1; s.DoorShut = 0; return; }
            }
            else s.DoorShut = Mathf.Max(0, s.DoorShut - deltaTime);
            if (t >= s.OpenAt && s.PackLeft > 0 && t >= s.NextPackMember)
            {
                int alive = 0; foreach (UnitState u in units) if (u.Alive && u.Faction == Faction.Enemy) alive++;
                if (alive < catalog.EnemyBudget)
                {
                    Vector2 door = s.Doors[s.CalledDoor];
                    Vector2 at = ClampPosition(door + new Vector2(MapNext(-.5f, .5f), -.7f), .5f);
                    UnitDefinition herd = catalog.EnemyPool != null && catalog.EnemyPool.Length > 0 ? SwarmDefinition() : catalog.MeleeEnemy;
                    if (herd != null)
                    {
                        UnitState spawned = Spawn(herd, Faction.Enemy, at);
                        spawned.SwarmMinion = true; spawned.DoorPack = true; spawned.Health *= .45f;
                        AssignSwarmRole(spawned, true);
                    }
                }
                s.PackLeft--; s.NextPackMember = t + .25f;
            }
            if (t >= s.CloseAt) s.CalledDoor = -1;
        }

        private void StepChairs()
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            if (t >= s.NextChair)
            {
                s.NextChair = t + MapEnvironmentRules.ChairEvery;
                for (int k = 0; k < s.Chairs.Count; k++)
                {
                    MapEnvironmentState.Chair chair = s.Chairs[(s.ChairTurn + k) % s.Chairs.Count];
                    if (t < chair.ReturnEnd) continue;
                    s.ChairTurn = (s.ChairTurn + k + 1) % s.Chairs.Count;
                    Vector2 toward = Champion.Position - chair.Home; float distance = toward.magnitude;
                    Vector2 direction = distance > .01f ? toward / distance : Vector2.right;
                    chair.From = chair.Home; chair.To = ClampPosition(chair.Home + direction * Mathf.Clamp(distance - 1.4f, 1.2f, 4), .8f);
                    chair.WarnAt = t; chair.SlideAt = t + MapEnvironmentRules.ChairWarn; chair.SlideEnd = chair.SlideAt + MapEnvironmentRules.ChairSlide;
                    chair.StayUntil = chair.SlideEnd + MapEnvironmentRules.ChairStay; chair.ReturnEnd = chair.StayUntil + MapEnvironmentRules.ChairReturn;
                    chair.Tripped.Clear();
                    s.Shown.Add(MapIdentity.Element.Chairs);
                    break;
                }
            }
            foreach (MapEnvironmentState.Chair chair in s.Chairs)
            {
                if (t < chair.SlideAt || t >= chair.ReturnEnd) { chair.Position = chair.Home; continue; }
                if (t < chair.SlideEnd)
                {
                    chair.Position = Vector2.Lerp(chair.From, chair.To, Mathf.SmoothStep(0, 1, (t - chair.SlideAt) / MapEnvironmentRules.ChairSlide));
                    // enemies it sweeps into trip for a moment
                    foreach (UnitState u in units)
                        if (u.Alive && u.Faction == Faction.Enemy && !u.Definition.IsBoss && !chair.Tripped.Contains(u.Id) && (u.Position - chair.Position).sqrMagnitude < Mathf.Pow(MapEnvironmentRules.ChairRadius + u.Definition.Radius + .15f, 2))
                        { chair.Tripped.Add(u.Id); mapEnv.Tripped[u.Id] = t + .9f; }
                }
                else if (t < chair.StayUntil) chair.Position = chair.To;
                else chair.Position = Vector2.Lerp(chair.To, chair.Home, Mathf.SmoothStep(0, 1, (t - chair.StayUntil) / MapEnvironmentRules.ChairReturn));
            }
        }

        /// <summary>For captures: the board and its doors gathered round the hero, so one frame shows them all.</summary>
        private void WaitingRoomCaptureFixture()
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            s.Board = Champion.Position + new Vector2(-.6f, 2.4f);
            s.Doors[0] = Champion.Position + new Vector2(-5.6f, 1.4f); s.Doors[1] = Champion.Position + new Vector2(5.4f, 1.4f);
            s.Doors[2] = Champion.Position + new Vector2(-6.4f, -3.4f); s.Doors[3] = Champion.Position + new Vector2(6.2f, -3.4f);
            s.CalledDoor = s.LastDoor = 1; s.CallAt = t - MapEnvironmentRules.DoorCall - .4f; s.PackLeft = 2; s.NextPackMember = t; s.NextCall = t + 30;
            MapEnvironmentState.Chair c = s.Chairs[0];
            c.From = c.Home; c.To = ClampPosition(Champion.Position + new Vector2(-1.6f, .4f), .8f); c.WarnAt = t - .3f; c.SlideAt = t + .6f; c.SlideEnd = c.SlideAt + MapEnvironmentRules.ChairSlide; c.StayUntil = c.SlideEnd + 4; c.ReturnEnd = c.StayUntil + 1;
            MapEnvironmentState.Chair d = s.Chairs[1];
            d.From = d.Home; d.To = ClampPosition(Champion.Position + new Vector2(1.8f, -.8f), .8f); d.WarnAt = t - 3; d.SlideAt = t - 2; d.SlideEnd = t - 1.3f; d.StayUntil = t + 4; d.ReturnEnd = t + 5;
            s.NextChair = t + 30;
        }
    }
}
