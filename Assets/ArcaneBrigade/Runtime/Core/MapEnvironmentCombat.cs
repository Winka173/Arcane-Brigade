using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class CheckpointCatalog
    {
        /// <summary>Filled by RunSession.PrepareCombat (MapIdentityRun): the map whose living elements run this wave, or -1.</summary>
        [System.NonSerialized] public int MapEnvironment = -1;
    }

    /// <summary>What the map's living elements are doing right now (read by MapEnvironmentVisual, the guide and the checks).
    /// Times are simulation seconds. Every element is telegraphed and mild, and none is a pool on the floor.</summary>
    public sealed class MapEnvironmentState
    {
        public MapTheme Theme;
        public Vector2 Anchor;
        public readonly HashSet<MapIdentity.Element> Shown = new HashSet<MapIdentity.Element>();

        // Waiting Room: the Number Board and its four doors; the restless chairs
        public Vector2 Board;
        public readonly Vector2[] Doors = new Vector2[4];
        public int CalledDoor = -1, LastDoor = -1, PackLeft;
        public float CallAt = -100, NextCall, NextPackMember;
        /// <summary>0..1: the hero standing at the called door shuts it (owner 23/10/2026); ShutAt marks the last one shut.</summary>
        public float DoorShut, ShutAt = -100; public int ShutDoor = -1;
        public float OpenAt => CallAt + MapEnvironmentRules.DoorCall;
        public float CloseAt => OpenAt + MapEnvironmentRules.DoorOpen;
        public sealed class Chair { public Vector2 Home, Position, From, To; public float WarnAt = -100, SlideAt = -100, SlideEnd = -100, StayUntil = -100, ReturnEnd = -100; public readonly HashSet<int> Tripped = new HashSet<int>(); }
        public readonly List<Chair> Chairs = new List<Chair>();
        public float NextChair;
        public int ChairTurn;

        // Post Office: the mail chute, the paper gust, the giant parcel; the letter's boost and the stamped enemies
        public sealed class Letter { public Vector2 Position; public float Born; public bool Gold, Gone; public int Style; }
        public float ChuteAt = -100, NextChute, ChuteX, ChuteTop, ChuteBottom;
        public int LettersToDrop; public float NextLetter;
        public readonly List<Letter> Letters = new List<Letter>();
        public float GustWarnAt = -100, NextGust, GustY; public int GustDirection = 1;
        public float GustAt => GustWarnAt + MapEnvironmentRules.GustWarning;
        public float GustUntil => GustAt + MapEnvironmentRules.GustSeconds;
        public int ParcelState; // 0 not yet, 1 falling, 2 sitting, 3 broken open (gold), 4 opened into an elite
        public Vector2 Parcel; public float ParcelDue, ParcelAt, HatchAt, ParcelBreak;
        public int BoostKind = -1; public float BoostUntil;
        public readonly Dictionary<int, float> Stamped = new Dictionary<int, float>();

        // Lamp Hall: the lamps and their sweeping light, the snuffer's round, the moths
        public sealed class Lamp { public Vector2 Base, Light; public float Phase, SnuffedAt = -100, SnuffedUntil = -100; public bool Lit = true; }
        public readonly List<Lamp> Lamps = new List<Lamp>();
        public int SnuffTarget = -1; public float ReachAt = -100, NextSnuff;
        public float SnuffAt => ReachAt + MapEnvironmentRules.SnuffReach;
        public readonly List<Vector2> Moths = new List<Vector2>();
        public bool HeroLit, HeroRevealed;

        // Archive Vault: the clock's chime, the shifting shelves, the ledger's ink ring
        public Vector2 Clock; public float NextChime, ChimeAt = -100, SlowUntil = -100, RushUntil = -100; public int Strikes; public bool Twelfth;
        public sealed class Shelf { public Vector2 Home, Position, From, To; public float WarnAt = -100, MoveAt = -100, MoveEnd = -100; public int Moves; }
        public readonly List<Shelf> Shelves = new List<Shelf>();
        /// <summary>The map's fixed shape (owner 23/10/2026): rows of fixed chairs (Waiting Room) or shelves (Archive Vault)
        /// that make aisles and corridors; solid like the moving ones. WallProps are where each piece is drawn.</summary>
        public sealed class Wall { public Vector2 A, B; public float Radius; }
        public readonly List<Wall> Walls = new List<Wall>();
        public readonly List<Vector2> WallProps = new List<Vector2>();
        public void AddRow(Vector2 at, int pieces, float spacing, float radius)
        {
            float half = (pieces - 1) * spacing * .5f;
            for (int i = 0; i < pieces; i++) WallProps.Add(at + new Vector2(-half + i * spacing, 0));
            Walls.Add(new Wall { A = at + Vector2.left * (half + spacing * .5f), B = at + Vector2.right * (half + spacing * .5f), Radius = radius });
        }
        public float NextShelf; public int ShelfTurn;
        public Vector2 Ledger; public int LedgerKills;

        public readonly Dictionary<int, float> Tripped = new Dictionary<int, float>();
    }

    /// <summary>Tunings of the living elements (D11 v2): mild, telegraphed, readable.</summary>
    public static class MapEnvironmentRules
    {
        public const float DoorEvery = 15, DoorCall = 2.4f, DoorOpen = 2.2f, ChairEvery = 8, ChairWarn = .9f, ChairSlide = .7f, ChairStay = 4, ChairReturn = 1, ChairRadius = .55f;
        public const float ChuteEvery = 13, ChuteWarn = 1.2f, LetterSpeed = 3.1f, BoostSeconds = 8, GoldBoostSeconds = 11, StampSeconds = 6, StampDamage = 1.2f;
        // the Post Office is the open floor with the strong draught (owner 23/10/2026): wider, stronger gusts (were 1.3 / 1.4 / 1.1)
        public const float GustEvery = 11, GustWarning = 1, GustSeconds = 4, GustHalf = 1.7f, GustShotPush = 1.8f, GustBodyPush = 1.4f, LightBody = .38f;
        public const float ParcelShare = .3f, ParcelWarn = 1.3f, ParcelHatch = 11, ParcelBreakSeconds = 1.8f, ParcelReach = 1.5f;
        public const float LightRadiusX = 2.8f, LightRadiusY = 1.8f, LightDamage = 1.15f, DarkSpeed = 1.03f, SnuffEvery = 15, SnuffReach = 1.4f, SnuffSeconds = 9, MothSpeed = 2.3f, RevealSpeed = 1.05f;
        public const float ChimeEvery = 20, ChimeWarn = 1, ChimeSlow = .45f, ChimeSlowSeconds = 1.5f, RushSpeed = 1.08f, RushHero = 1.15f, RushSeconds = 3;
        public const float ShelfEvery = 16, ShelfWarn = 1.2f, ShelfMove = 1.6f, ShelfHalf = .7f, ShelfRadius = .4f, LedgerRadius = 2.4f;
        public const int LedgerKillsPerDebt = 5, LedgerMaxDebt = 8;
        public const float BoostSpeed = 1.15f, BoostDamage = 1.15f;
        // owner 23/10/2026 map rules: shut a door by standing at it; a broken parcel bursts; friends quicker in the lamp light,
        // enemies tougher in the dark, a snuffed lamp relit by walking to it; the chime slows longer with the hero at the clock
        public const float DoorReach = 1.8f, DoorShutSeconds = 1.5f, ParcelBurstRadius = 2.8f, ParcelBurstHits = 4f;
        public const float LightHaste = 1.1f, DarkGuard = .85f, RelightReach = 1.3f, ClockReach = 3f, ClockSlowLonger = 2.5f;
    }

    /// <summary>The map's living elements in the fight (play-test item 24, D11 v2), deterministic and headless so the bot sweeps
    /// play them too. Each runs in ordinary (not boss) waves of its own map only.</summary>
    public sealed partial class CombatSimulation
    {
        private MapEnvironmentState mapEnv;
        private System.Random mapRandom;
        public MapEnvironmentState MapEnvironment => mapEnv;
        private float MapNext(float min, float max) => min + (float)mapRandom.NextDouble() * (max - min);

        private void InitMapEnvironment()
        {
            mapRandom = new System.Random(unchecked(runSeed * 31 + catalog.RunWave * 7907 + 11));
            var s = new MapEnvironmentState { Theme = (MapTheme)Mathf.Clamp(catalog.MapEnvironment, 0, 3) };
            Vector2 room = catalog.ArenaSize * .5f - new Vector2(9.6f, 6.2f);
            s.Anchor = new Vector2(Mathf.Clamp(Champion.Position.x, -Mathf.Max(0, room.x), Mathf.Max(0, room.x)), Mathf.Clamp(Champion.Position.y, -Mathf.Max(0, room.y), Mathf.Max(0, room.y)));
            Vector2 a = s.Anchor;
            switch (s.Theme)
            {
                case MapTheme.WaitingRoom:
                    s.Board = a + new Vector2(0, 4.4f);
                    s.Doors[0] = a + new Vector2(-8, 3); s.Doors[1] = a + new Vector2(8, 3); s.Doors[2] = a + new Vector2(-8, -4.2f); s.Doors[3] = a + new Vector2(8, -4.2f);
                    foreach (Vector2 home in new[] { new Vector2(-3.6f, 1.6f), new Vector2(4, -1.4f), new Vector2(-.6f, -3.6f) })
                        s.Chairs.Add(new MapEnvironmentState.Chair { Home = a + home, Position = a + home });
                    s.NextCall = 6; s.NextChair = 3.5f;
                    break;
                case MapTheme.PostOffice:
                    s.NextChute = 5; s.NextGust = 8;
                    s.ParcelDue = Mathf.Max(6, catalog.WaveDuration * MapEnvironmentRules.ParcelShare);
                    break;
                case MapTheme.LampHall:
                    Vector2[] bases = { new Vector2(-6.2f, 2.6f), new Vector2(6.2f, 2.6f), new Vector2(0, -3.4f) };
                    for (int i = 0; i < bases.Length; i++) s.Lamps.Add(new MapEnvironmentState.Lamp { Base = a + bases[i], Phase = i * 2.1f + MapNext(0, 1), Light = a + bases[i] });
                    int into = catalog.RunWave - MapIdentity.FirstWave(catalog.RunWave);
                    for (int i = 0; i < (into >= 3 ? 2 : 1); i++) s.Moths.Add(s.Lamps[i].Base + Vector2.up * 2);
                    s.NextSnuff = 7;
                    break;
                default:
                    s.Clock = a + new Vector2(0, 4.4f);
                    foreach (Vector2 home in new[] { new Vector2(-4.6f, -.6f), new Vector2(4.6f, .8f) })
                        s.Shelves.Add(new MapEnvironmentState.Shelf { Home = a + home, Position = a + home, From = a + home, To = a + home });
                    s.Ledger = a + new Vector2(mapRandom.Next(2) == 0 ? -6.6f : 6.6f, -3.2f);

                    s.NextChime = 9; s.NextShelf = 6;
                    break;
            }
            mapEnv = s;
        }

        private void ProcessMapEnvironment(float deltaTime)
        {
            if (catalog.MapEnvironment < 0 || Phase != RunPhase.Combat || Champion == null) return;
            if (mapEnv == null) InitMapEnvironment();
            switch (mapEnv.Theme)
            {
                // owner 23/10/2026: nothing on a map blocks or pushes anyone - the sliding chairs, the gusts, the moving shelves
                // and the solid rows are gone; chairs and shelves stay where they stand as scenery
                case MapTheme.WaitingRoom: StepNumberBoard(deltaTime); break;
                case MapTheme.PostOffice: StepChute(deltaTime); StepParcel(deltaTime); break;
                case MapTheme.LampHall: StepLamps(deltaTime); break;
                default: StepClock(); break;
            }
        }

        // ------------------------------------------------------------------ Waiting Room
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

        // ------------------------------------------------------------------ Post Office
        private void StepChute(float deltaTime)
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            if (t >= s.NextChute)
            {
                s.NextChute = t + MapEnvironmentRules.ChuteEvery; s.ChuteAt = t;
                float side = MapNext(1.5f, 4) * (mapRandom.Next(2) == 0 ? -1 : 1);
                Vector2 half = catalog.ArenaSize * .5f - Vector2.one;
                s.ChuteX = Mathf.Clamp(Champion.Position.x + side, -half.x, half.x);
                s.ChuteTop = Mathf.Min(half.y, Champion.Position.y + 4.6f); s.ChuteBottom = Mathf.Max(-half.y, Champion.Position.y - 5.2f);
                s.LettersToDrop = 5; s.NextLetter = t + MapEnvironmentRules.ChuteWarn;
                s.Shown.Add(MapIdentity.Element.Chute);
            }
            if (s.LettersToDrop > 0 && t >= s.NextLetter)
            {
                s.Letters.Add(new MapEnvironmentState.Letter { Position = new Vector2(s.ChuteX + MapNext(-.25f, .25f), s.ChuteTop), Born = t, Gold = s.LettersToDrop == 3, Style = mapRandom.Next(3) });
                s.LettersToDrop--; s.NextLetter = t + .45f;
            }
            for (int i = s.Letters.Count - 1; i >= 0; i--)
            {
                MapEnvironmentState.Letter letter = s.Letters[i];
                letter.Position += new Vector2(Mathf.Sin((t - letter.Born) * 3 + i) * .35f * deltaTime, -MapEnvironmentRules.LetterSpeed * deltaTime);
                if (letter.Position.y < s.ChuteBottom) { s.Letters.RemoveAt(i); continue; }
                if ((letter.Position - Champion.Position).sqrMagnitude < Mathf.Pow(Champion.Definition.Radius + .45f, 2))
                {
                    s.BoostKind = mapRandom.Next(3); s.BoostUntil = t + (letter.Gold ? MapEnvironmentRules.GoldBoostSeconds : MapEnvironmentRules.BoostSeconds);
                    s.Letters.RemoveAt(i); continue;
                }
                foreach (UnitState u in units)
                    if (u.Alive && u.Faction == Faction.Enemy && (u.Position - letter.Position).sqrMagnitude < Mathf.Pow(u.Definition.Radius + .35f, 2))
                        s.Stamped[u.Id] = t + MapEnvironmentRules.StampSeconds;
            }
        }

        private void StepGust(float deltaTime)
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            if (t >= s.NextGust)
            {
                s.NextGust = t + MapEnvironmentRules.GustEvery; s.GustWarnAt = t;
                s.GustY = Mathf.Clamp(Champion.Position.y + MapNext(-2, 2), -catalog.ArenaSize.y * .5f + 1.5f, catalog.ArenaSize.y * .5f - 1.5f);
                s.GustDirection = mapRandom.Next(2) == 0 ? -1 : 1;
                s.Shown.Add(MapIdentity.Element.Gust);
            }
            if (t < s.GustAt || t >= s.GustUntil) return;
            float push = s.GustDirection * deltaTime;
            foreach (ProjectileState p in projectiles)
                if (Mathf.Abs(p.Position.y - s.GustY) < MapEnvironmentRules.GustHalf) p.Position += new Vector2(push * MapEnvironmentRules.GustShotPush, 0);
            foreach (UnitState u in units)
                if (u.Alive && u.Faction == Faction.Enemy && !u.Definition.IsBoss && !u.Elite && u.Definition.Radius <= MapEnvironmentRules.LightBody && Mathf.Abs(u.Position.y - s.GustY) < MapEnvironmentRules.GustHalf)
                    u.Position = ClampPosition(u.Position + new Vector2(push * MapEnvironmentRules.GustBodyPush, 0), u.Definition.Radius);
        }

        private void StepParcel(float deltaTime)
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            switch (s.ParcelState)
            {
                case 0:
                    if (t < s.ParcelDue) return;
                    float angle = MapNext(0, Mathf.PI * 2);
                    s.Parcel = ClampPosition(Champion.Position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * .7f) * 4.2f, 1.5f);
                    s.ParcelAt = t; s.ParcelState = 1; s.Shown.Add(MapIdentity.Element.Parcel);
                    break;
                case 1:
                    if (t < s.ParcelAt + MapEnvironmentRules.ParcelWarn) return;
                    s.ParcelState = 2; s.ParcelAt = t; s.HatchAt = t + MapEnvironmentRules.ParcelHatch; // it lands; nothing is pushed
                    break;
                case 2:
                    if ((Champion.Position - s.Parcel).sqrMagnitude < MapEnvironmentRules.ParcelReach * MapEnvironmentRules.ParcelReach)
                        s.ParcelBreak = Mathf.Min(1, s.ParcelBreak + deltaTime / MapEnvironmentRules.ParcelBreakSeconds);
                    if (s.ParcelBreak >= 1)
                    {
                        s.ParcelState = 3; s.ParcelAt = t;
                        float perCoin = Mathf.Max(.01f, KitCoinScale * Mathf.Max(.01f, catalog.CoinValueScale));
                        int coins = Mathf.Max(4, Mathf.RoundToInt(Mathf.Max(8, catalog.ChestGold) * .5f / perCoin));
                        for (int i = 0; i < 4; i++) DropLoot(LootKind.Coin, s.Parcel + new Vector2((i % 2) * .5f - .25f, (i / 2) * .4f - .2f), coins / 4 + (i == 0 ? coins % 4 : 0));
                        // it bursts: splinters of the crate hit every enemy close by (four of the hero's own hits)
                        CombatCause burst = NewCause(Champion.Id).Child("map-parcel-burst");
                        float splinters = Champion.Definition.Weapon.Damage * MapEnvironmentRules.ParcelBurstHits;
                        foreach (UnitState u in units.ToArray())
                            if (u.Alive && u.Faction == Faction.Enemy && (u.Position - s.Parcel).sqrMagnitude < MapEnvironmentRules.ParcelBurstRadius * MapEnvironmentRules.ParcelBurstRadius)
                                ApplyDamage(u, u.Definition.IsBoss ? splinters * .5f : splinters, Faction.Champion, "Parcel", u.Position - s.Parcel, cause: burst);
                        events.Add(new CombatEvent(CombatEventKind.SignaturePulse, s.Parcel, Faction.Champion, MapEnvironmentRules.ParcelBurstRadius, subject: Champion, element: DamageElement.Physical, cause: burst));
                    }
                    else if (t >= s.HatchAt)
                    {
                        s.ParcelState = 4; s.ParcelAt = t;
                        UnitDefinition kind = null;
                        if (catalog.EnemyPool != null) foreach (UnitDefinition d in catalog.EnemyPool) if (d != null && !d.IsBoss && (kind == null || d.Health > kind.Health)) kind = d;
                        kind ??= catalog.MeleeEnemy;
                        if (kind == null) return;
                        UnitDefinition elite = Object.Instantiate(kind);
                        elite.Health = kind.Health * 4.5f; elite.Radius = kind.Radius * 1.2f; elite.BodyScale = kind.BodyScale * 1.3f;
                        elite.Tint = Color.Lerp(kind.Tint, new Color(1f, .82f, .45f), .45f); elite.KnockbackResistance = Mathf.Max(kind.KnockbackResistance, .6f);
                        elite.DisplayName = "Indebted " + RunUICopy.UnitName(kind);
                        UnitState unit = Spawn(elite, Faction.Enemy, s.Parcel + Vector2.down * .3f); unit.Elite = true;
                        if (unit.Variant != null && catalog.ExpandedContent) unit.Variant.Affix = MapIdentity.Affix(s.Theme);
                    }
                    break;
            }
        }

        // ------------------------------------------------------------------ Lamp Hall
        private void StepLamps(float deltaTime)
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            s.Shown.Add(MapIdentity.Element.Light);
            foreach (MapEnvironmentState.Lamp lamp in s.Lamps)
            {
                float phase = lamp.Phase + t * .32f;
                lamp.Light = ClampPosition(lamp.Base + new Vector2(Mathf.Cos(phase), Mathf.Sin(phase) * .55f) * 2.4f + Vector2.up * .3f, .5f);
                lamp.Lit = !(t >= lamp.SnuffedAt && t < lamp.SnuffedUntil);
                // the hero walking up to a snuffed lamp lights it again
                if (!lamp.Lit && t >= lamp.SnuffedAt + .6f && ((Champion.Position - lamp.Base).sqrMagnitude < MapEnvironmentRules.RelightReach * MapEnvironmentRules.RelightReach || (Champion.Position - lamp.Light).sqrMagnitude < MapEnvironmentRules.RelightReach * MapEnvironmentRules.RelightReach))
                { lamp.SnuffedUntil = t; lamp.Lit = true; }
            }
            if (t >= s.NextSnuff)
            {
                s.NextSnuff = t + MapEnvironmentRules.SnuffEvery;
                int best = -1; float bestDistance = float.MaxValue;
                for (int i = 0; i < s.Lamps.Count; i++)
                {
                    float d = (s.Lamps[i].Light - Champion.Position).sqrMagnitude;
                    if (s.Lamps[i].Lit && d < bestDistance) { best = i; bestDistance = d; }
                }
                if (best >= 0)
                {
                    s.SnuffTarget = best; s.ReachAt = t;
                    s.Lamps[best].SnuffedAt = s.SnuffAt; s.Lamps[best].SnuffedUntil = s.SnuffAt + MapEnvironmentRules.SnuffSeconds;
                    s.Shown.Add(MapIdentity.Element.Snuffer);
                }
            }
            s.HeroLit = InMapLight(Champion.Position);
            s.HeroRevealed = false;
            if (t >= 4)
            {
                s.Shown.Add(MapIdentity.Element.Moths);
                for (int i = 0; i < s.Moths.Count; i++)
                {
                    Vector2 goal = Champion.Position + new Vector2(Mathf.Sin(t * 1.7f + i * 2) * .35f, .9f + Mathf.Cos(t * 2.3f + i) * .2f);
                    s.Moths[i] = Vector2.MoveTowards(s.Moths[i], goal, MapEnvironmentRules.MothSpeed * deltaTime);
                    if ((s.Moths[i] - goal).sqrMagnitude < 1) s.HeroRevealed = true;
                }
            }
        }
        private bool InMapLight(Vector2 position)
        {
            if (mapEnv == null || mapEnv.Theme != MapTheme.LampHall) return false;
            foreach (MapEnvironmentState.Lamp lamp in mapEnv.Lamps)
            {
                if (!lamp.Lit) continue;
                Vector2 d = position - lamp.Light; // the painted light is an oval on the floor; the bonus follows its edge
                if (d.x * d.x / (MapEnvironmentRules.LightRadiusX * MapEnvironmentRules.LightRadiusX) + d.y * d.y / (MapEnvironmentRules.LightRadiusY * MapEnvironmentRules.LightRadiusY) < 1) return true;
            }
            return false;
        }

        // ------------------------------------------------------------------ Archive Vault
        private void StepClock()
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            s.Shown.Add(MapIdentity.Element.Ledger);
            if (t >= s.NextChime - MapEnvironmentRules.ChimeWarn && s.ChimeAt < s.NextChime - MapEnvironmentRules.ChimeWarn - .01f)
            {
                s.ChimeAt = s.NextChime; s.Twelfth = s.Strikes == 1; // the second chime of a wave is the twelfth strike
                s.Shown.Add(MapIdentity.Element.Clock);
            }
            if (t >= s.NextChime)
            {
                s.Strikes++; s.NextChime = t + MapEnvironmentRules.ChimeEvery;
                bool atClock = (Champion.Position - s.Clock).sqrMagnitude < MapEnvironmentRules.ClockReach * MapEnvironmentRules.ClockReach;
                if (s.Twelfth) s.RushUntil = t + MapEnvironmentRules.RushSeconds; else s.SlowUntil = t + MapEnvironmentRules.ChimeSlowSeconds * (atClock ? MapEnvironmentRules.ClockSlowLonger : 1);
            }
        }

        private void StepShelves()
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            if (t >= s.NextShelf && s.Shelves.Count > 0)
            {
                s.NextShelf = t + MapEnvironmentRules.ShelfEvery;
                MapEnvironmentState.Shelf shelf = s.Shelves[s.ShelfTurn++ % s.Shelves.Count];
                Vector2[] steps = { new Vector2(0, 2.4f), new Vector2(2.6f, 0), new Vector2(0, -2.4f), new Vector2(-2.6f, 0) };
                Vector2 to = shelf.Home + (shelf.Moves % 2 == 0 ? steps[(shelf.Moves / 2 + (shelf.Home.x < s.Anchor.x ? 0 : 2)) % 4] : Vector2.zero);
                shelf.From = shelf.Position; shelf.To = ClampPosition(to, 1); shelf.Moves++;
                shelf.WarnAt = t; shelf.MoveAt = t + MapEnvironmentRules.ShelfWarn; shelf.MoveEnd = shelf.MoveAt + MapEnvironmentRules.ShelfMove;
                s.Shown.Add(MapIdentity.Element.Shelves);
            }
            foreach (MapEnvironmentState.Shelf shelf in s.Shelves)
                if (t >= shelf.MoveAt && shelf.MoveEnd > shelf.MoveAt)
                    shelf.Position = Vector2.Lerp(shelf.From, shelf.To, Mathf.SmoothStep(0, 1, Mathf.Clamp01((t - shelf.MoveAt) / MapEnvironmentRules.ShelfMove)));
        }

        // ------------------------------------------------------------------ blockers, speeds, damage, coins
        /// <summary>Chairs and shelves are solid: anyone standing in one is moved to its edge (bosses ignore them).</summary>
        private void KeepOutOfMapBlockers()
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            if (s.Theme != MapTheme.WaitingRoom && s.Theme != MapTheme.ArchiveVault && s.Walls.Count == 0) return;
            foreach (UnitState u in units)
            {
                if (!u.Alive || u.Definition.IsBoss || u.BodyLift > .4f) continue;
                Vector2 p = u.Position; float r = u.Definition.Radius;
                if (s.Theme == MapTheme.WaitingRoom)
                {
                    foreach (MapEnvironmentState.Chair chair in s.Chairs)
                    {
                        if (t < chair.SlideAt || t >= chair.ReturnEnd) continue;
                        p = PushOutOfSegment(p, chair.Position, chair.Position, MapEnvironmentRules.ChairRadius + r);
                    }
                }
                else
                    foreach (MapEnvironmentState.Shelf shelf in s.Shelves)
                        p = PushOutOfSegment(p, shelf.Position + Vector2.left * MapEnvironmentRules.ShelfHalf, shelf.Position + Vector2.right * MapEnvironmentRules.ShelfHalf, MapEnvironmentRules.ShelfRadius + r);
                foreach (MapEnvironmentState.Wall wall in s.Walls) p = PushOutOfSegment(p, wall.A, wall.B, wall.Radius + r);
                if (p != u.Position) u.Position = ClampPosition(p, r);
            }
        }
        private static Vector2 PushOutOfSegment(Vector2 p, Vector2 a, Vector2 b, float radius)
        {
            Vector2 ab = b - a; float f = ab.sqrMagnitude < 1e-6f ? 0 : Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
            Vector2 near = a + ab * f, away = p - near; float d = away.magnitude;
            if (d >= radius) return p;
            Vector2 dir = d > 1e-4f ? away / d : Vector2.down;
            return near + dir * radius;
        }

        /// <summary>After an enemy has moved: the map changes how far (dark is faster, a chime slows, the twelfth strike rushes,
        /// moths lead them on, a chair trips them).</summary>
        private void MapMotion(UnitState unit)
        {
            if (mapEnv == null || unit.Faction != Faction.Enemy || unit.Definition.IsBoss) return;
            float scale = MapEnemySpeed(unit);
            if (Mathf.Abs(scale - 1) < .001f) return;
            Vector2 step = unit.Position - unit.PreviousPosition;
            if (step.sqrMagnitude < 1e-8f || step.sqrMagnitude > 1) return; // a teleport or a fling is left alone
            unit.Position = ClampPosition(unit.PreviousPosition + step * scale, unit.Definition.Radius);
        }
        public float MapEnemySpeed(UnitState unit)
        {
            MapEnvironmentState s = mapEnv; if (s == null) return 1;
            float t = Elapsed, scale = 1;
            if (s.Tripped.TryGetValue(unit.Id, out float tripped) && tripped > t) scale *= .3f;
            switch (s.Theme)
            {
                case MapTheme.LampHall:
                    if (!InMapLight(unit.Position)) scale *= MapEnvironmentRules.DarkSpeed;
                    if (s.HeroRevealed && (unit.Position - Champion.Position).sqrMagnitude < 64) scale *= MapEnvironmentRules.RevealSpeed;
                    break;
                case MapTheme.ArchiveVault:
                    if (s.SlowUntil > t) scale *= MapEnvironmentRules.ChimeSlow;
                    else if (s.RushUntil > t) scale *= MapEnvironmentRules.RushSpeed;
                    break;
            }
            return scale;
        }
        /// <summary>A friend standing in the Lamp Hall's light attacks 10% faster (a cooldown factor).</summary>
        private float MapCycle(UnitState unit) => mapEnv != null && mapEnv.Theme == MapTheme.LampHall && unit.Faction == Faction.Companion && InMapLight(unit.Position) ? 1 / MapEnvironmentRules.LightHaste : 1;
        private float MapHeroScale
        {
            get
            {
                MapEnvironmentState s = mapEnv; if (s == null) return OctoberHeroPace;
                float scale = 1;
                if (s.BoostKind == 0 && s.BoostUntil > Elapsed) scale *= MapEnvironmentRules.BoostSpeed;
                if (s.Theme == MapTheme.ArchiveVault && s.RushUntil > Elapsed && s.SlowUntil <= Elapsed) scale *= MapEnvironmentRules.RushHero;
                return scale * OctoberHeroPace;
            }
        }
        /// <summary>Damage on an enemy: stamped by a letter, struck from inside the lamp light, or by a hero with the sharp boost.</summary>
        private float MapDamage(UnitState target, float damage, Faction source)
        {
            MapEnvironmentState s = mapEnv;
            if (s == null || target.Faction != Faction.Enemy || source == Faction.Enemy) return damage;
            if (s.Stamped.Count > 0 && s.Stamped.TryGetValue(target.Id, out float stamped) && stamped > Elapsed) damage *= MapEnvironmentRules.StampDamage;
            if (s.Theme == MapTheme.LampHall && s.HeroLit) damage *= MapEnvironmentRules.LightDamage;
            if (s.Theme == MapTheme.LampHall && !InMapLight(target.Position)) damage *= MapEnvironmentRules.DarkGuard; // tougher in the dark
            if (s.BoostKind == 1 && s.BoostUntil > Elapsed) damage *= MapEnvironmentRules.BoostDamage;
            return damage;
        }
        /// <summary>A fallen enemy's coin: more in the dark of the Lamp Hall, double inside the ledger's ring (which adds debt),
        /// one more with the rich boost.</summary>
        private int MapCoins(UnitState target, int coins)
        {
            MapEnvironmentState s = mapEnv;
            if (s == null || coins <= 0 || target.Faction != Faction.Enemy) return coins;
            if (target.DoorPack) return 0; // a door's pack drops no gold
            if (s.Theme == MapTheme.LampHall && !InMapLight(target.Position) && (!target.SwarmMinion || target.Id % 4 == 0)) coins += 1;
            if (s.Theme == MapTheme.ArchiveVault && (target.Position - s.Ledger).sqrMagnitude < MapEnvironmentRules.LedgerRadius * MapEnvironmentRules.LedgerRadius) { coins *= 2; s.LedgerKills++; }
            if (s.BoostKind == 2 && s.BoostUntil > Elapsed && !target.SwarmMinion) coins += 1;
            return coins;
        }
        /// <summary>Debt owed by the ledger after this wave (each point: +1% enemies next wave).</summary>
        public int LedgerDebt => mapEnv == null ? 0 : Mathf.Min(MapEnvironmentRules.LedgerMaxDebt, mapEnv.LedgerKills / MapEnvironmentRules.LedgerKillsPerDebt);

        /// <summary>For captures: every element of the map on screen at once (called, sliding, falling, gusting, reaching...).</summary>
        public void MapEnvironmentCaptureFixture()
        {
            if (catalog.MapEnvironment < 0 || Champion == null) return;
            if (mapEnv == null) InitMapEnvironment();
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            switch (s.Theme)
            {
                case MapTheme.WaitingRoom:
                    // the board and its doors gathered round the hero, so one frame shows them all
                    s.Board = Champion.Position + new Vector2(-.6f, 2.4f);
                    s.Doors[0] = Champion.Position + new Vector2(-5.6f, 1.4f); s.Doors[1] = Champion.Position + new Vector2(5.4f, 1.4f);
                    s.Doors[2] = Champion.Position + new Vector2(-6.4f, -3.4f); s.Doors[3] = Champion.Position + new Vector2(6.2f, -3.4f);
                    s.CalledDoor = s.LastDoor = 1; s.CallAt = t - MapEnvironmentRules.DoorCall - .4f; s.PackLeft = 2; s.NextPackMember = t; s.NextCall = t + 30;
                    MapEnvironmentState.Chair c = s.Chairs[0];
                    c.From = c.Home; c.To = ClampPosition(Champion.Position + new Vector2(-1.6f, .4f), .8f); c.WarnAt = t - .3f; c.SlideAt = t + .6f; c.SlideEnd = c.SlideAt + MapEnvironmentRules.ChairSlide; c.StayUntil = c.SlideEnd + 4; c.ReturnEnd = c.StayUntil + 1;
                    MapEnvironmentState.Chair d = s.Chairs[1];
                    d.From = d.Home; d.To = ClampPosition(Champion.Position + new Vector2(1.8f, -.8f), .8f); d.WarnAt = t - 3; d.SlideAt = t - 2; d.SlideEnd = t - 1.3f; d.StayUntil = t + 4; d.ReturnEnd = t + 5;
                    s.NextChair = t + 30;
                    break;
                case MapTheme.PostOffice:
                    s.ChuteAt = t - 1.6f; s.ChuteX = Champion.Position.x + 2.2f; s.ChuteTop = Champion.Position.y + 4.4f; s.ChuteBottom = Champion.Position.y - 5; s.NextChute = t + 30;
                    s.Letters.Clear(); for (int i = 0; i < 4; i++) s.Letters.Add(new MapEnvironmentState.Letter { Position = new Vector2(s.ChuteX + (i % 2) * .3f - .15f, s.ChuteTop - .4f - i * 1.6f), Born = t - i, Gold = i == 2, Style = i % 3 });
                    s.LettersToDrop = 0;
                    s.GustWarnAt = t - MapEnvironmentRules.GustWarning - .5f; s.GustY = Champion.Position.y - 1.6f; s.GustDirection = 1; s.NextGust = t + 30;
                    s.Parcel = ClampPosition(Champion.Position + new Vector2(-3.2f, .6f), 1.5f); s.ParcelState = 2; s.ParcelAt = t - 2; s.HatchAt = t + 6; s.ParcelBreak = .45f;
                    foreach (UnitState u in units) if (u.Alive && u.Faction == Faction.Enemy && (u.Position - Champion.Position).sqrMagnitude < 36) s.Stamped[u.Id] = t + 6;
                    s.BoostKind = 1; s.BoostUntil = t + 6;
                    s.Shown.Add(MapIdentity.Element.Chute); s.Shown.Add(MapIdentity.Element.Gust); s.Shown.Add(MapIdentity.Element.Parcel);
                    break;
                case MapTheme.LampHall:
                    s.Lamps[0].Base = Champion.Position + new Vector2(-3.4f, 1.2f); s.Lamps[1].Base = Champion.Position + new Vector2(3.6f, 1.4f);
                    s.Lamps[0].Phase = -t * .32f - 0.3f; s.Lamps[1].Phase = -t * .32f + 2.4f;
                    s.SnuffTarget = 1; s.ReachAt = t - .7f; s.Lamps[1].SnuffedAt = s.SnuffAt; s.Lamps[1].SnuffedUntil = s.SnuffAt + 9; s.NextSnuff = t + 30;
                    for (int i = 0; i < s.Moths.Count; i++) s.Moths[i] = Champion.Position + new Vector2(.6f * i, 1.1f);
                    break;
                default:
                    s.Clock = Champion.Position + new Vector2(-5.2f, .4f);
                    s.ChimeAt = t + .4f; s.NextChime = t + .4f; s.Twelfth = true; s.Strikes = 1;
                    s.Shelves[0].Home = s.Shelves[0].From = s.Shelves[0].Position = Champion.Position + new Vector2(-3.6f, -1);
                    s.Shelves[0].To = s.Shelves[0].From + new Vector2(0, 2.4f); s.Shelves[0].WarnAt = t - .4f; s.Shelves[0].MoveAt = t + .8f; s.Shelves[0].MoveEnd = t + 2.4f;
                    s.Shelves[1].Home = s.Shelves[1].From = s.Shelves[1].To = s.Shelves[1].Position = Champion.Position + new Vector2(4.2f, .2f);
                    s.Ledger = Champion.Position + new Vector2(2.6f, -2.6f); s.NextShelf = t + 30;
                    s.Shown.Add(MapIdentity.Element.Shelves); s.Shown.Add(MapIdentity.Element.Clock);
                    break;
            }
        }
    }
}
