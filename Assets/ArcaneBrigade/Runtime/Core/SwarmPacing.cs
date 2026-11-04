using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Swarm bodies with a job: ink bloaters burst for the brigade (and chain), fuse runners rush the traveller and blow.</summary>
    public enum SwarmRole { None, Bloater, Fuse }

    public sealed partial class CheckpointCatalog
    {
        public int BossSwarmBudget;   // a light swarm that keeps coming while a boss stands (and through the whole final duel)
        public bool FinalDuel;        // the Last Customer: walls of the waiting come at two thresholds of its health
        public int MinionCoin = 1;    // raw reward per swarm body, before batching
        public int KillCoinBatchSize = 1, KillCoinRewardPercent = 100;
        public bool WholeKeeper;      // THE KEEPER, WHOLE (shop 17): its body shrugs off damage while its hand, carpet and eye hold
        public int PrismDrops;        // prism shards an elite may still drop this night (the run allows two)
    }

    public static class SwarmPacingRules
    {
        public const int ChainDepth = 3;
        public const float BloaterRadius = 1.5f, FuseRadius = 1.7f, FuseLight = 0.7f, FuseReach = 2.1f;
        public const float SurgeWarning = 1.4f;
        /// <summary>Surges replace the old lulls: rare, a wall of weak bodies from one side. Never more often than every 20 s.</summary>
        public static float SurgeEvery(int wave) => Mathf.Lerp(28f, 21f, Mathf.Clamp01((wave - 2) / 16f));
        // requirement 4/11/2026: fewer small enemies (was min(44, 10 + 1.7 per wave))
        public static int SurgeSize(int wave) => Mathf.Min(24, 6 + wave);
        public static float BloaterChance(int wave) => wave < 2 ? 0 : Mathf.Min(0.07f, 0.025f + wave * 0.0025f);
        public static float FuseChance(int wave) => wave < 3 ? 0 : Mathf.Min(0.12f, 0.04f + wave * 0.004f);
        /// <summary>Damage left over after a kill spills into the nearest bodies (never chains).</summary>
        public const float OverkillShare = 0.5f;
    }

    public sealed partial class CombatSimulation
    {
        private struct Burst { public Vector2 Position; public float Radius, Damage, At; public int Depth, SourceId; public CombatCause Cause; }
        private struct Splash { public Vector2 Position; public float Radius, Damage; public int ExcludeId; public CombatCause Cause; }
        private readonly List<Burst> bursts = new List<Burst>();
        private readonly List<Splash> splashes = new List<Splash>();
        private readonly List<UnitState> pacingNeighbours = new List<UnitState>();
        private int burstDepth;          // >0 while a bloater burst is landing, so a bloater it kills knows its link
        private float nextSurge, surgeAt = -1;
        private Vector2 surgeDirection;
        private int surgeSide = -1, surgesFired;
        private float flareAt, bellAt;
        private int flaresDropped, bellsDropped, prismsDropped;
        public int PrismShards { get; private set; }   // picked up this night; the session banks them
        public float LowestChampionShare { get; private set; } = 1; // the lowest the traveller's health fell, as a share of its maximum
        private float lastBossFraction = 1;
        public float SurgeAt => surgeAt;
        public Vector2 SurgeDirection => surgeDirection;
        public int SweptGold { get; private set; }
        public int SweptCoins { get; private set; }
        public int SurgesFired => surgesFired;

        private void ResetSwarmPacing()
        {
            bursts.Clear(); splashes.Clear(); burstDepth = 0;
            float duration = Mathf.Max(20, catalog.WaveDuration);
            // surges belong to ordinary nights; a boss's own fight keeps only its light swarm
            nextSurge = catalog.RunWave >= 2 && catalog.ExpandedContent && catalog.Boss == null && catalog.EnemyBudget > 0 ? Mathf.Max(12f, duration * 0.32f) : float.PositiveInfinity;
            surgeAt = -1; surgeSide = -1; surgesFired = 0;
            flareAt = catalog.RunWave >= 3 && catalog.ExtendedLoot ? duration * (0.35f + (float)lootRandom.NextDouble() * 0.3f) : float.PositiveInfinity;
            bellAt = catalog.RunWave >= 2 && catalog.ExtendedLoot ? duration * (0.5f + (float)lootRandom.NextDouble() * 0.25f) : float.PositiveInfinity;
            flaresDropped = bellsDropped = 0;
            toyAt = catalog.RunWave >= 2 && catalog.ExtendedLoot ? duration * (0.2f + (float)lootRandom.NextDouble() * 0.25f) : float.PositiveInfinity;
            toysDropped = 0; MagnetUntil = 0;
            SweptGold = SweptCoins = 0; sweptPieces.Clear();
            lastBossFraction = 1;
        }

        // ---- spawning roles -------------------------------------------------------------------------------------------
        private void AssignSwarmRole(UnitState spawned, bool minion)
        {
            if (!catalog.ExpandedContent || spawned.Definition.IsBoss || spawned.Definition.SpecialEnemy != SpecialEnemyKind.None) return;
            if (minion)
            {
                if (spawnRandom.NextDouble() < SwarmPacingRules.BloaterChance(catalog.RunWave)) { spawned.Role = SwarmRole.Bloater; spawned.Health *= 1.6f; }
                return;
            }
            if (spawned.Definition.Behavior == EnemyBehavior.Standard && spawned.Definition.Weapon.Kind == WeaponKind.Contact && spawned.Variant == null
                && spawnRandom.NextDouble() < SwarmPacingRules.FuseChance(catalog.RunWave))
                spawned.Role = SwarmRole.Fuse;
        }

        /// <summary>Fuse runners sprint; near the traveller they stop, swell and blow. A lit fuse cannot be put out.</summary>
        private bool MoveSwarmRole(UnitState unit, float deltaTime)
        {
            if (unit.SurgeDirection != Vector2.zero) return MoveSurge(unit, deltaTime);
            if (unit.Role != SwarmRole.Fuse) return false;
            if (unit.FuseAt > 0)
            {
                if (Elapsed < unit.FuseAt) return true;
                unit.FuseAt = 0;
                unit.Alive = false; unit.Health = 0;
                events.Add(new CombatEvent(CombatEventKind.Detonation, unit.Position, Faction.Enemy, SwarmPacingRules.FuseRadius, subjectId: unit.Id, subject: unit));
                events.Add(new CombatEvent(CombatEventKind.Kill, unit.Position, Faction.Enemy, 0.8f, subjectId: unit.Id, subject: unit));
                return true;
            }
            Vector2 offset = Champion.Position - unit.Position;
            float distance = offset.magnitude;
            if (distance <= SwarmPacingRules.FuseReach)
            {
                unit.FuseAt = Elapsed + SwarmPacingRules.FuseLight;
                unit.TelegraphUntil = unit.FuseAt;
                Danger(ExpandedFieldKind.Shockwave, unit, unit.Position, SwarmPacingRules.FuseRadius, SwarmPacingRules.FuseLight, 0.25f);
                events.Add(new CombatEvent(CombatEventKind.Windup, unit.Position, Faction.Enemy, subject: unit, weapon: unit.Definition.Weapon));
                return true;
            }
            unit.Facing = distance > 0.001f ? offset / distance : unit.Facing;
            float speed = unit.Definition.Speed * 1.4f * (unit.ChillUntil > Elapsed ? 0.55f : 1f);
            unit.Position = ClampPosition(unit.Position + unit.Facing * Mathf.Min(speed * deltaTime, distance), unit.Definition.Radius);
            return true;
        }

        // ---- deaths: bursts, chains and overkill ------------------------------------------------------------------------
        private void SwarmDeath(UnitState target, Faction source, float overkill, CombatCause cause)
        {
            if (target.Faction != Faction.Enemy) return;
            if (target.Role == SwarmRole.Bloater || target.Role == SwarmRole.Fuse && target.FuseAt <= 0)
            {
                int depth = burstDepth;
                if (depth <= SwarmPacingRules.ChainDepth && bursts.Count < 48)
                {
                    float maximum = target.Definition.Health * (target.SwarmMinion ? 0.45f * 1.6f : 1f);
                    bursts.Add(new Burst
                    {
                        Position = target.Position, Radius = target.Role == SwarmRole.Bloater ? SwarmPacingRules.BloaterRadius : 1.3f,
                        Damage = Mathf.Max(6f, maximum * (target.Role == SwarmRole.Bloater ? 1.3f : 0.8f)) * (1f - depth * 0.15f),
                        At = Elapsed + 0.06f + depth * 0.05f, Depth = depth, SourceId = target.Id, Cause = NewCause(Champion.Id).Child("ink-bloater")
                    });
                }
            }
            if (source == Faction.Enemy || target.Definition.IsBoss || overkill <= 0 || cause.ItemId == "overkill" || cause.ItemId == "lantern-flare" || cause.ItemId == "ink-bloater" || splashes.Count >= 64) return;
            if (overkill < Mathf.Max(4f, target.Definition.Health * 0.25f)) return;
            splashes.Add(new Splash { Position = target.Position, Radius = 1.05f + target.Definition.Radius, Damage = overkill * SwarmPacingRules.OverkillShare, ExcludeId = target.Id, Cause = cause.Child("overkill") });
        }

        private void ProcessSwarmPacing(float deltaTime)
        {
            // overkill: the two nearest bodies share what was left over
            int splashCount = Mathf.Min(24, splashes.Count);
            for (int index = 0; index < splashCount; index++)
            {
                Splash splash = splashes[index];
                Vector2 extent = Vector2.one * (splash.Radius + maximumRadius);
                spatial.Query(splash.Position - extent, splash.Position + extent, pacingNeighbours);
                UnitState first = null, second = null; float firstDistance = float.MaxValue, secondDistance = float.MaxValue;
                foreach (UnitState candidate in pacingNeighbours)
                {
                    if (!candidate.Alive || candidate.Faction != Faction.Enemy || candidate.Id == splash.ExcludeId) continue;
                    float distance = (candidate.Position - splash.Position).sqrMagnitude;
                    float reach = splash.Radius + candidate.Definition.Radius;
                    if (distance > reach * reach) continue;
                    if (distance < firstDistance) { second = first; secondDistance = firstDistance; first = candidate; firstDistance = distance; }
                    else if (distance < secondDistance) { second = candidate; secondDistance = distance; }
                }
                if (first != null) ApplyDamage(first, splash.Damage * (second == null ? 1f : 0.6f), Faction.Champion, "Overkill", (first.Position - splash.Position).normalized, cause: splash.Cause);
                if (second != null) ApplyDamage(second, splash.Damage * 0.4f, Faction.Champion, "Overkill", (second.Position - splash.Position).normalized, cause: splash.Cause);
            }
            splashes.RemoveRange(0, splashCount);

            // ink bursts: friendly to the brigade; a bloater caught in one bursts in turn, three links at most
            int processed = 0;
            for (int index = 0; index < bursts.Count && processed < 12; )
            {
                Burst burst = bursts[index];
                if (burst.At > Elapsed) { index++; continue; }
                bursts.RemoveAt(index); processed++;
                events.Add(new CombatEvent(CombatEventKind.Detonation, burst.Position, Faction.Champion, burst.Radius, subjectId: burst.SourceId, cause: burst.Cause));
                Vector2 extent = Vector2.one * (burst.Radius + maximumRadius);
                spatial.Query(burst.Position - extent, burst.Position + extent, pacingNeighbours);
                burstDepth = burst.Depth + 1;
                foreach (UnitState target in pacingNeighbours)
                {
                    if (!target.Alive || target.Faction != Faction.Enemy || target.Id == burst.SourceId) continue;
                    float reach = burst.Radius + target.Definition.Radius;
                    if ((target.Position - burst.Position).sqrMagnitude > reach * reach) continue;
                    float damage = target.Definition.IsBoss ? Mathf.Min(burst.Damage, target.Definition.Health * 0.01f) : burst.Damage;
                    ApplyDamage(target, damage, Faction.Champion, "Ink burst", (target.Position - burst.Position).normalized, cause: burst.Cause);
                }
                burstDepth = 0;
            }

            ProcessSurge();
            ProcessFinalDuelWalls();
        }

        // ---- surges -------------------------------------------------------------------------------------------------
        private void ProcessSurge()
        {
            if (surgeAt < 0 && Elapsed >= nextSurge - SwarmPacingRules.SurgeWarning)
            {
                if (catalog.Boss == null && Elapsed > catalog.WaveDuration - 6f) { nextSurge = float.PositiveInfinity; return; }
                surgeSide = PickSurgeSide();
                if (surgeSide < 0) { nextSurge += 3f; return; }
                surgeDirection = -SideVector(surgeSide);
                surgeAt = nextSurge;
            }
            if (surgeAt >= 0 && Elapsed >= surgeAt)
            {
                SpawnWall(surgeSide, SwarmPacingRules.SurgeSize(catalog.RunWave));
                surgeAt = -1; surgesFired++;
                nextSurge = Elapsed + SwarmPacingRules.SurgeEvery(catalog.RunWave);
            }
        }

        private void ProcessFinalDuelWalls()
        {
            if (!catalog.FinalDuel || Boss == null || !Boss.Alive || surgeAt >= 0) return;
            float fraction = Boss.Health / Mathf.Max(1, Boss.Definition.Health);
            foreach (float threshold in new[] { 0.7f, 0.35f })
                if (lastBossFraction > threshold && fraction <= threshold)
                {
                    // the waiting stand up from both sides at once
                    int side = PickSurgeSide();
                    if (side >= 0) { SpawnWall(side, 34); SpawnWall((side + 2) % 4, 34); surgesFired++; surgeDirection = -SideVector(side); }
                }
            lastBossFraction = fraction;
        }

        private static Vector2 SideVector(int side) => side == 0 ? Vector2.left : side == 1 ? Vector2.right : side == 2 ? Vector2.down : Vector2.up;

        private int PickSurgeSide()
        {
            Vector2 half = catalog.ArenaSize * 0.5f;
            var open = new List<int>(4);
            for (int side = 0; side < 4; side++)
            {
                Vector2 vector = SideVector(side);
                float room = vector.x != 0 ? half.x - vector.x * Champion.Position.x : half.y - vector.y * Champion.Position.y;
                if (room >= (vector.x != 0 ? 8f : 5.2f)) open.Add(side);
            }
            return open.Count == 0 ? -1 : open[spawnRandom.Next(open.Count)];
        }

        private void SpawnWall(int side, int size)
        {
            UnitDefinition herd = HerdDefinition();
            if (herd == null) return;
            bool mixedWall = catalog.RunWave >= 10;
            int originalSmall = size;
            if (mixedWall) size = 2 + Mathf.CeilToInt(size * .5f);
            int remainingSmall = size - 2;
            Vector2 outward = SideVector(side), across = new Vector2(-outward.y, outward.x);
            float distance = outward.x != 0 ? 10.2f : 6.4f, span = outward.x != 0 ? 6.2f : 10.5f;
            int rows = 2, perRow = Mathf.CeilToInt(size / (float)rows);
            int alive = 0; foreach (UnitState unit in StableUnits) if (unit.Alive && unit.Faction == Faction.Enemy) alive++;
            int room = Mathf.Max(0, Mathf.CeilToInt(Mathf.Max(catalog.EnemyBudget, catalog.BossSwarmBudget) * 1.3f) + 40 - alive);
            size = Mathf.Min(size, Mathf.Min(room, 640 - alive));
            for (int index = 0; index < size; index++)
            {
                int row = index / perRow, column = index % perRow;
                float t = perRow <= 1 ? 0 : column / (float)(perRow - 1) * 2 - 1;
                Vector2 position = Champion.Position + outward * (distance + row * 0.7f) + across * (t * span + (row % 2) * 0.35f);
                position = ClampPosition(position, 0.5f);
                if ((position - Champion.Position).sqrMagnitude < catalog.SpawnSafetyRadius * catalog.SpawnSafetyRadius) continue;
                bool escort = mixedWall && index < 2;
                UnitState spawned = Spawn(escort ? PackEscortDefinition(herd) : herd, Faction.Enemy, position);
                spawned.SwarmMinion = !escort;
                if (escort) spawned.PackCoinBonus = EscortCoinBonus(originalSmall, remainingSmall, 0, 2, index);
                if (escort) InitializeEnemyVariant(spawned);
                else { spawned.Health *= 0.32f; spawned.SurgeDirection = -outward; }
                spawned.Facing = -outward;
                swarmSpawns++;
            }
        }

        /// <summary>A surge body marches across the screen, leaning a little toward the traveller; it leaves when it is far past.</summary>
        private bool MoveSurge(UnitState unit, float deltaTime)
        {
            Vector2 toward = Champion.Position - unit.Position;
            float along = Vector2.Dot(toward, unit.SurgeDirection);
            if (along < -13f) { unit.Alive = false; unit.Health = 0; return true; } // walked off screen: no coin, no corpse
            if (along < -3f && OnEdge(unit)) { unit.SurgeDirection = Vector2.zero; return false; } // stopped by the far wall: it joins the swarm
            Vector2 lean = along > 0 ? toward.normalized * 0.32f : Vector2.zero;
            Vector2 direction = (unit.SurgeDirection + lean).normalized;
            unit.Facing = direction;
            float speed = unit.Definition.Speed * 1.12f * (unit.ChillUntil > Elapsed ? 0.55f : 1f);
            unit.Position = ClampPosition(unit.Position + direction * speed * deltaTime, unit.Definition.Radius);
            return true;
        }

        private bool OnEdge(UnitState unit)
        {
            Vector2 half = catalog.ArenaSize * 0.5f - Vector2.one * (unit.Definition.Radius + 0.05f);
            return Mathf.Abs(unit.Position.x) >= half.x || Mathf.Abs(unit.Position.y) >= half.y;
        }

        // ---- boss waves keep a light swarm ------------------------------------------------------------------------------
        private bool BossOvertimeSwarm => catalog.Boss != null && Elapsed >= catalog.WaveDuration;

        // ---- Lantern Flare and the shopkeeper's bell -----------------------------------------------------------------------
        private void PacingLoot(UnitState target)
        {
            if (!catalog.ExtendedLoot || target.Faction != Faction.Enemy || target.Definition.IsBoss) return;
            // a very rare prism shard from an elite (an affixed enemy): at most one a night and two a run
            if (prismsDropped < Mathf.Min(1, catalog.PrismDrops) && target.Variant != null && target.Variant.Affix != EnemyAffix.None && !target.SwarmMinion && combatRandom.NextDouble() < 0.04)
            { DropLoot(LootKind.Prism, target.Position, 1); prismsDropped++; return; }
            int flares = catalog.RunWave >= 12 ? 2 : 1, bells = catalog.RunWave >= 10 ? 2 : 1;
            if (flaresDropped < flares && Elapsed >= flareAt && !target.SwarmMinion)
            {
                DropLoot(LootKind.Flare, target.Position, 1); flaresDropped++;
                flareAt = Elapsed + Mathf.Max(18f, catalog.WaveDuration * 0.35f);
            }
            else if (toysDropped < (catalog.RunWave >= 8 ? 2 : 1) && Elapsed >= toyAt && !target.SwarmMinion)
            {
                DropLoot(NextToy(), target.Position, 1); toysDropped++;
                toyAt = Elapsed + Mathf.Max(15f, catalog.WaveDuration * 0.3f);
            }
            else if (bellsDropped < bells && Elapsed >= bellAt)
            {
                int coins = 0; foreach (PickupState pickup in pickups) if (pickup.Kind == LootKind.Coin) coins++;
                if (coins < 12) return; // a bell with nothing to call is no gift
                DropLoot(LootKind.Bell, target.Position, 1); bellsDropped++;
                bellAt = Elapsed + Mathf.Max(16f, catalog.WaveDuration * 0.3f);
            }
        }

        // ---- the toy drops (requirement 25/10/2026): a horseshoe magnet, a jack-in-the-box and a snow globe ---------------
        public const float MagnetSeconds = 8, SpringRadius = 3.4f, SpringPush = 2.4f, GlobeRadius = 3.8f, GlobeFreeze = 2.2f;
        private float toyAt; private int toysDropped;
        /// <summary>While the horseshoe pulls, every coin on the floor flies to the hero.</summary>
        public float MagnetUntil { get; private set; }
        /// <summary>The toys take turns over the run; a magnet with few coins about gives its turn to the jack-in-the-box.</summary>
        private LootKind NextToy()
        {
            LootKind kind = (LootKind)((int)LootKind.Magnet + (catalog.RunWave + toysDropped) % 3);
            if (kind == LootKind.Magnet) { int coins = 0; foreach (PickupState p in pickups) if (p.Kind == LootKind.Coin) coins++; if (coins < 8) kind = LootKind.Spring; }
            return kind;
        }
        private void PlayToy(LootKind kind)
        {
            CombatCause cause = NewCause(Champion.Id).Child("toy-" + kind.ToString().ToLowerInvariant());
            Vector2 at = Champion.Position;
            if (kind == LootKind.Magnet)
            {
                MagnetUntil = Elapsed + MagnetSeconds;
                events.Add(new CombatEvent(CombatEventKind.MagnetPull, at, Faction.Champion, MagnetSeconds, cause: cause));
                return;
            }
            int touched = 0;
            for (int index = 0; index < units.Count; index++)
            {
                UnitState enemy = units[index];
                if (!enemy.Alive || enemy.Faction != Faction.Enemy) continue;
                Vector2 away = enemy.Position - at; float distance = away.magnitude;
                if (kind == LootKind.Spring)
                {
                    // the jack springs out: everything near is flung away, the nearest the furthest; a boss only rocks
                    if (distance > SpringRadius) continue;
                    away = distance > .001f ? away / distance : Vector2.up;
                    float push = enemy.Definition.IsBoss ? .25f : SpringPush * (1 - .5f * distance / SpringRadius);
                    enemy.Position = ClampPosition(enemy.Position + away * push, enemy.Definition.Radius); spatial.Move(enemy);
                    if (!enemy.Definition.IsBoss) enemy.StaggerUntil = Mathf.Max(enemy.StaggerUntil, Elapsed + .45f);
                    events.Add(new CombatEvent(CombatEventKind.Knockback, enemy.Position, Faction.Champion, push, away, subjectId: enemy.Id, subject: enemy, cause: cause));
                }
                else
                {
                    // the globe is shaken: everything near freezes in place; a boss is only slowed
                    if (distance > GlobeRadius) continue;
                    if (enemy.Definition.IsBoss) { enemy.KitSlowUntil = Mathf.Max(enemy.KitSlowUntil, Elapsed + GlobeFreeze); enemy.KitSlow = Mathf.Max(enemy.KitSlow, .4f); }
                    else Freeze(enemy, GlobeFreeze);
                }
                touched++;
            }
            events.Add(new CombatEvent(kind == LootKind.Spring ? CombatEventKind.SpringBurst : CombatEventKind.GlobeFreeze, at, Faction.Champion, touched, cause: cause));
        }

        /// <summary>The Lantern Flare: everything small on screen is burnt away; bigger things are scorched; hostile shots go out.</summary>
        private void LanternFlare()
        {
            CombatCause cause = NewCause(Champion.Id).Child("lantern-flare");
            events.Add(new CombatEvent(CombatEventKind.Flare, Champion.Position, Faction.Champion, 1, cause: cause));
            Vector2 view = new Vector2(10.6f, 6.2f);
            for (int index = 0; index < units.Count; index++)
            {
                UnitState target = units[index];
                if (!target.Alive || target.Faction != Faction.Enemy || target.EntranceUntil > Elapsed) continue;
                Vector2 offset = target.Position - Champion.Position;
                if (Mathf.Abs(offset.x) > view.x || Mathf.Abs(offset.y) > view.y) continue;
                bool fodder = !target.Definition.IsBoss && target.Definition.SpecialEnemy == SpecialEnemyKind.None && (target.SwarmMinion || target.Definition.BodyHeight < 1.2f && target.Variant == null);
                float damage = fodder ? target.Health + 0.01f : target.Definition.Health * (target.Definition.IsBoss ? 0.03f : 0.15f);
                ApplyDamage(target, damage, Faction.Champion, "Lantern Flare", offset.normalized, cause: cause, injectedDamage: true);
            }
            for (int index = projectiles.Count - 1; index >= 0; index--)
            {
                Vector2 offset = projectiles[index].Position - Champion.Position;
                if (projectiles[index].Faction == Faction.Enemy && Mathf.Abs(offset.x) <= view.x && Mathf.Abs(offset.y) <= view.y) projectiles.RemoveAt(index);
            }
        }

        /// <summary>The shopkeeper's bell: every coin in the hall comes running.</summary>
        private void RingBell()
        {
            int called = 0;
            foreach (PickupState pickup in pickups)
                if (pickup.Kind == LootKind.Coin) { pickup.Attracting = true; pickup.Vacuumed = true; called++; }
            events.Add(new CombatEvent(CombatEventKind.Bell, Champion.Position, Faction.Champion, called));
        }

        // ---- the end-of-wave sweep -------------------------------------------------------------------------------------
        /// <summary>The end-of-wave sweep: when a wave is won, every coin and egg left on the floor is counted. The flight home
        /// is drawn from <see cref="SweptPieces"/> (position, value) after the wave has already been settled.</summary>
        public void SweepRemaining()
        {
            if (Phase != RunPhase.Victory) return;
            FlushKillCoins(); // pay the unfinished final batch too, without needing another kill
            sweptPieces.Clear();
            for (int index = pickups.Count - 1; index >= 0; index--)
            {
                PickupState pickup = pickups[index];
                if (pickup.Kind != LootKind.Coin && pickup.Kind != LootKind.Egg) continue;
                Bank(pickup); pickups.RemoveAt(index);
            }
        }

        private readonly List<Vector3> sweptPieces = new List<Vector3>();
        public IReadOnlyList<Vector3> SweptPieces => sweptPieces;
        private void Bank(PickupState pickup)
        {
            if (sweptPieces.Count < 120) sweptPieces.Add(new Vector3(pickup.Position.x, pickup.Position.y, pickup.Kind == LootKind.Egg ? -pickup.Value : pickup.Value));
            if (pickup.Kind == LootKind.Egg) { Eggs = RunSession.BoundedCounter((long)Eggs + pickup.Value); return; }
            int before = Gold;
            CreditCoin(pickup, NewCause(Champion.Id));
            SweptGold = RunSession.BoundedCounter((long)SweptGold + Gold - before); SweptCoins++;
        }

        // ---- capture and check fixtures (diagnostics saves only) ---------------------------------------------------------
        private static void RequireFixture() { if (!RunPaths.Diagnostic) throw new System.InvalidOperationException("Pacing fixtures need the diagnostics save"); }
        public void PacingFixtureSurge(float lead) { RequireFixture(); surgeAt = -1; nextSurge = Elapsed + lead; }
        /// <summary>Capture fixture: breaks the whole Keeper's hand, carpet and eye at once, so it kneels.</summary>
        public void KeeperFixtureBreakAll()
        {
            RequireFixture(); if (Boss == null || !catalog.WholeKeeper) return;
            Boss.LeftPartBrokenAt = Boss.RightPartBrokenAt = Boss.CenterPartBrokenAt = Elapsed;
            Boss.KneelUntil = Elapsed + KeeperKneelSeconds; Boss.TelegraphUntil = 0; Boss.NextBossPattern = Boss.KneelUntil + 0.6f;
        }
        public void PrismFixtureDrop(Vector2 position) { RequireFixture(); DropLoot(LootKind.Prism, position, 1); }
        public UnitState PacingFixtureRole(SwarmRole role, Vector2 position, float health)
        {
            RequireFixture();
            UnitDefinition definition = role == SwarmRole.Bloater ? HerdDefinition() : catalog.MeleeEnemy;
            UnitState unit = Spawn(definition, Faction.Enemy, ClampPosition(position, definition.Radius));
            unit.SwarmMinion = role == SwarmRole.Bloater; unit.Role = role; unit.Variant = null; unit.Health = health;
            spatial.Rebuild(units);
            return unit;
        }
        public void PacingFixtureLoot(LootKind kind, Vector2 position, int value) { RequireFixture(); DropLoot(kind, position, value); }
        public void PacingFixtureWin() { RequireFixture(); if (Phase == RunPhase.Combat) Phase = RunPhase.Victory; }
        public void PacingFixtureKill(UnitState target) { RequireFixture(); ApplyDamage(target, target.Health + 1f, Faction.Champion, "Fixture", Vector2.right, cause: NewCause(Champion.Id)); }
    }
}
