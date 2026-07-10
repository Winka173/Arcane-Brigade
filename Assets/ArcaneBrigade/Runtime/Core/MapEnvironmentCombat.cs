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
    /// Times are simulation seconds. Every element is telegraphed and mild, and none is a pool on the floor. Each map adds its
    /// own fields in its file under Core/Maps (requirement 25/10/2026); only what every map shares is here.</summary>
    public sealed partial class MapEnvironmentState
    {
        public MapTheme Theme;
        public Vector2 Anchor;
        public readonly HashSet<MapIdentity.Element> Shown = new HashSet<MapIdentity.Element>();

        /// <summary>The map's fixed shape (requirement 23/10/2026): rows of fixed chairs (Waiting Room) or shelves (Archive Vault)
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

        public readonly Dictionary<int, float> Tripped = new Dictionary<int, float>();
    }

    /// <summary>Tunings of the living elements (D11 v2): mild, telegraphed, readable. Each map's tunings are in its file under
    /// Core/Maps (requirement 25/10/2026).</summary>
    public static partial class MapEnvironmentRules
    {
    }

    /// <summary>The map's living elements in the fight (play-test item 24, D11 v2), deterministic and headless so the bot sweeps
    /// play them too. Each runs in ordinary (not boss) waves of its own map only. Requirement 25/10/2026: this file only dispatches
    /// to each map's hooks (Core/Maps/Map{Theme}.cs: Init, Step, StandingBoost, EnemyFactor, AllyCycle, HeroPace, Coins,
    /// CaptureFixture) and keeps the helpers they share.</summary>
    public sealed partial class CombatSimulation
    {
        private MapEnvironmentState mapEnv;
        private System.Random mapRandom;
        public MapEnvironmentState MapEnvironment => mapEnv;
        private float MapNext(float min, float max) => min + (float)mapRandom.NextDouble() * (max - min);

        private void InitMapEnvironment()
        {
            mapRandom = new System.Random(unchecked(runSeed * 31 + catalog.RunWave * 7907 + 11));
            int theme = catalog.MapEnvironment;
            var s = new MapEnvironmentState { Theme = System.Enum.IsDefined(typeof(MapTheme), theme) ? (MapTheme)theme : MapTheme.WaitingRoom };
            Vector2 room = catalog.ArenaSize * .5f - new Vector2(9.6f, 6.2f);
            s.Anchor = new Vector2(Mathf.Clamp(Champion.Position.x, -Mathf.Max(0, room.x), Mathf.Max(0, room.x)), Mathf.Clamp(Champion.Position.y, -Mathf.Max(0, room.y), Mathf.Max(0, room.y)));
            switch (s.Theme)
            {
                case MapTheme.WaitingRoom: InitWaitingRoom(s); break;
                case MapTheme.PostOffice: InitPostOffice(s); break;
                case MapTheme.Glasshouse: InitGlasshouse(s); break;
                case MapTheme.LampHall: InitLampHall(s); break;
                case MapTheme.Theatre: InitTheatre(s); break;
                case MapTheme.ArchiveVault: InitArchiveVault(s); break;
            }
            mapEnv = s;
        }

        private void ProcessMapEnvironment(float deltaTime)
        {
            if (catalog.MapEnvironment < 0 || Phase != RunPhase.Combat || Champion == null) return;
            if (mapEnv == null) InitMapEnvironment();
            switch (mapEnv.Theme)
            {
                case MapTheme.WaitingRoom: StepWaitingRoom(deltaTime); break;
                case MapTheme.PostOffice: StepPostOffice(deltaTime); break;
                case MapTheme.Glasshouse: StepGlasshouse(deltaTime); break;
                case MapTheme.LampHall: StepLampHall(deltaTime); break;
                case MapTheme.Theatre: StepTheatre(deltaTime); break;
                case MapTheme.ArchiveVault: StepArchiveVault(deltaTime); break;
            }
        }

        // ------------------------------------------------------------------ per-map dispatch
        private float MapAllyCycle(UnitState unit) => mapEnv.Theme switch
        {
            MapTheme.WaitingRoom => WaitingRoomAllyCycle(unit),
            MapTheme.PostOffice => PostOfficeAllyCycle(unit),
            MapTheme.Glasshouse => GlasshouseAllyCycle(unit),
            MapTheme.LampHall => LampHallAllyCycle(unit),
            MapTheme.Theatre => TheatreAllyCycle(unit),
            MapTheme.ArchiveVault => ArchiveVaultAllyCycle(unit),
            _ => 1
        };
        private float MapThemeHeroPace() => mapEnv.Theme switch
        {
            MapTheme.WaitingRoom => WaitingRoomHeroPace(),
            MapTheme.PostOffice => PostOfficeHeroPace(),
            MapTheme.Glasshouse => GlasshouseHeroPace(),
            MapTheme.LampHall => LampHallHeroPace(),
            MapTheme.Theatre => TheatreHeroPace(),
            MapTheme.ArchiveVault => ArchiveVaultHeroPace(),
            _ => 1
        };
        private int MapThemeCoins(UnitState target, int coins) => mapEnv.Theme switch
        {
            MapTheme.WaitingRoom => WaitingRoomCoins(target, coins),
            MapTheme.PostOffice => PostOfficeCoins(target, coins),
            MapTheme.Glasshouse => GlasshouseCoins(target, coins),
            MapTheme.LampHall => LampHallCoins(target, coins),
            MapTheme.Theatre => TheatreCoins(target, coins),
            MapTheme.ArchiveVault => ArchiveVaultCoins(target, coins),
            _ => coins
        };

        // ------------------------------------------------------------------ blockers, speeds, damage, coins
        /// <summary>Walls are solid: anyone standing in one is moved to its edge (bosses ignore them).</summary>
        private void KeepOutOfMapBlockers()
        {
            MapEnvironmentState s = mapEnv;
            if (s.Walls.Count == 0) return;
            foreach (UnitState u in StableUnits)
            {
                if (!u.Alive || u.Definition.IsBoss || u.BodyLift > .4f) continue;
                Vector2 p = u.Position; float r = u.Definition.Radius;
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

        /// <summary>After an enemy has moved: the map changes how far (each map's EnemyFactor on the Speed axis; a chair trips them).</summary>
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
            return scale * MapEnemyFactor(unit, MapEnemyAxis.Speed);
        }
        /// <summary>An attack cooldown factor: an enemy's from its map's rules; the hero's from the map's attack-speed boosts; and
        /// each map's own (a friend in the Lamp Hall's light attacks 10% faster).</summary>
        private float MapCycle(UnitState unit)
        {
            if (mapEnv == null) return 1;
            if (unit.Faction == Faction.Enemy) return MapEnemyFactor(unit, MapEnemyAxis.Cycle);
            float cycle = 1;
            if (unit == Champion) cycle /= 1 + MapBoostValue(MapStat.AttackSpeed); // a map's attack-speed boost
            return cycle * MapAllyCycle(unit);
        }
        private float MapHeroScale
        {
            get
            {
                MapEnvironmentState s = mapEnv; if (s == null) return OctoberHeroPace;
                float scale = MapThemeHeroPace();
                scale *= 1 + MapBoostValue(MapStat.MoveSpeed); // a map's speed boost
                return scale * OctoberHeroPace;
            }
        }
        /// <summary>Damage on an enemy: the map's damage boosts (and standing in a lamp's light), then the map's rules for that
        /// enemy (stamped by a letter...); an enemy's blow on our side: the map's rules for the attacker.</summary>
        private float MapDamage(UnitState target, float damage, Faction source, int attackerId = 0)
        {
            MapEnvironmentState s = mapEnv;
            if (s == null) return damage;
            // an enemy's blow on our side: the map may strengthen or weaken that enemy's hits
            if (source == Faction.Enemy) return target.Faction == Faction.Enemy ? damage : damage * MapEnemyFactor(UnitWithId(attackerId), MapEnemyAxis.DamageDealt);
            if (target.Faction != Faction.Enemy) return damage;
            damage *= 1 + MapBoostValue(MapStat.Damage); // the map's damage boosts (and standing in a lamp's light)
            return damage * MapEnemyFactor(target, MapEnemyAxis.DamageTaken);
        }
        /// <summary>A fallen enemy's coin: one more with a map's gold-a-kill boost, then each map's own rule (a door's pack drops
        /// none, double inside the ledger's ring, one more with the letter's rich boost).</summary>
        private int MapCoins(UnitState target, int coins)
        {
            MapEnvironmentState s = mapEnv;
            if (s == null || coins <= 0 || target.Faction != Faction.Enemy) return coins;
            if (!target.SwarmMinion) coins += Mathf.RoundToInt(MapBoostValue(MapStat.Gold)); // a map's gold-a-kill boost
            return MapThemeCoins(target, coins);
        }

        /// <summary>For captures: every element of the map on screen at once (called, sliding, falling, gusting, reaching...).</summary>
        public void MapEnvironmentCaptureFixture()
        {
            if (catalog.MapEnvironment < 0 || Champion == null) return;
            if (mapEnv == null) InitMapEnvironment();
            switch (mapEnv.Theme)
            {
                case MapTheme.WaitingRoom: WaitingRoomCaptureFixture(); break;
                case MapTheme.PostOffice: PostOfficeCaptureFixture(); break;
                case MapTheme.Glasshouse: GlasshouseCaptureFixture(); break;
                case MapTheme.LampHall: LampHallCaptureFixture(); break;
                case MapTheme.Theatre: TheatreCaptureFixture(); break;
                case MapTheme.ArchiveVault: ArchiveVaultCaptureFixture(); break;
            }
        }
    }
}
