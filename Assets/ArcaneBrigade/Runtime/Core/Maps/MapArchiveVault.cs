using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    // The Archive Vault (waves 18-20; Docs/Map_Mechanics_2026-10-25.md section 6): flying pages and the CHAPTER, ink fog lanes,
    // bookworms, the spilled inkpot, the stamp from the sky. Requirement 25/10/2026: each map's state, tunings and rules live in
    // its own file; MapEnvironmentCombat and MapBoosts only dispatch to the hooks below. The old clock chime and rush, the sliding
    // shelves and the ledger ring are gone (the spec removes them).

    public sealed partial class MapEnvironmentState
    {
        // Archive Vault: pages crossing the room (flying, then lying on the floor), and the hero's catches for the CHAPTER
        public sealed class VaultPage { public Vector2 Position, From, To; public float Born, LandAt; public int Kind; public bool Spat; }
        public readonly List<VaultPage> VaultPages = new List<VaultPage>();
        public float VaultNextPage;
        public readonly List<Vector2> VaultCatches = new List<Vector2>(); // x: when, y: the page's kind
        public float VaultCaughtAt = -100, VaultChapterAt = -100; public int VaultCaughtKind, VaultCaught; public Vector2 VaultCaughtWhere;
        // Archive Vault: the ink fog lanes (they glide to new places every 20 s)
        public sealed class VaultLane { public Vector2 A, B, FromA, FromB, ToA, ToB; }
        public readonly List<VaultLane> VaultFog = new List<VaultLane>();
        public float VaultFogMovedAt = -100, VaultNextFog;
        // Archive Vault: bookworms (enemies that crawl to pages on the floor and eat them) and the pages they spat back
        public sealed class VaultWorm { public int Pages; public readonly List<int> Kinds = new List<int>(); public Vector2 Last; public float BornAt, AteAt = -100; }
        public readonly Dictionary<int, VaultWorm> VaultWorms = new Dictionary<int, VaultWorm>();
        public float VaultNextWorm; public readonly List<Vector3> VaultSpits = new List<Vector3>(); // x, y, when (and the pages: in VaultPages)
        // Archive Vault: the giant inkpot that tips into a pool, and the chain of ink bursts (x, y, when)
        public Vector2 VaultPot, VaultPool; public float VaultPotAt = -100, VaultNextPot;
        public float VaultSpillAt => VaultPotAt + MapEnvironmentRules.VaultPotWarn;
        public float VaultPoolUntil => VaultSpillAt + MapEnvironmentRules.VaultPoolSeconds;
        public readonly Dictionary<int, Vector3> VaultInPool = new Dictionary<int, Vector3>(); // x, y, max health
        public readonly HashSet<int> VaultBurst = new HashSet<int>();
        public readonly List<Vector3> VaultBursts = new List<Vector3>();
        // Archive Vault: the stamp from the sky and the enemies it marked PROCESSED
        public Vector2 VaultStamp; public float VaultStampAt = -100, VaultNextStamp; public bool VaultStamped;
        public float VaultStampHitAt => VaultStampAt + MapEnvironmentRules.VaultStampWarn;
        public readonly Dictionary<int, float> VaultProcessed = new Dictionary<int, float>();
        /// <summary>The vault's old clock, now only scenery (its chime is gone).</summary>
        public Vector2 VaultClock;

        // kept only because MapEnvironmentCombat's blockers still read them: there are no shelves any more (always empty)
        public sealed class Shelf { public Vector2 Position; }
        public readonly List<Shelf> Shelves = new List<Shelf>();
    }

    public static partial class MapEnvironmentRules
    {
        // flying pages: one every 10 s; each lends one of five stats for 10 s; three within 30 s is a CHAPTER (all three, 20 s)
        public const float VaultPageEvery = 10, VaultPageSeconds = 10, VaultChapterWindow = 30, VaultChapterSeconds = 20, VaultPageSpeed = 2.2f, VaultPageFloor = 12, VaultPageReach = .55f;
        public const int VaultChapterPages = 3;
        public const float VaultPageDamage = .15f, VaultPageHaste = .15f, VaultPageArmor = 2, VaultPageGold = 1, VaultPageSpeedBoost = .15f;
        // ink fog: 2-3 lanes moving every 20 s; enemies inside are 10% slower and deal 10% less damage
        public const float VaultFogEvery = 20, VaultFogMove = 2.5f, VaultFogHalf = .95f, VaultFogLength = 15, VaultFogSlow = .9f, VaultFogWeak = .9f;
        // bookworms: +30% max health and +15% damage per page eaten, up to 3; killed, they spit every page back and 5 gold a page
        public const float VaultWormHealth = .3f, VaultWormDamage = .15f, VaultWormSeek = 10, VaultWormEvery = 4;
        public const int VaultWormPages = 3, VaultWormGold = 5, VaultWormsAtOnce = 3, VaultWormsHunting = 2;
        // the spilled inkpot: every 25 s, warned 3 s, a 3-unit pool for 10 s; enemies in it +30% speed and +15% damage; one dying
        // in it bursts for 50% of its max health on enemies within 1.5 units (a boss takes at most 5% of its own), and it chains
        public const float VaultPotEvery = 25, VaultPotWarn = 3, VaultPoolSeconds = 10, VaultPoolRadius = 3, VaultPoolSpeed = 1.3f, VaultPoolDamage = 1.15f;
        public const float VaultBurstShare = .5f, VaultBurstRadius = 1.5f, VaultBurstBossCap = .05f;
        // the stamp from the sky: every 15 s on the densest crowd, a 2.5-unit area warned 1.5 s; 3x the hero hit, survivors
        // PROCESSED (+25% damage taken) for 8 s; never the hero
        public const float VaultStampEvery = 15, VaultStampWarn = 1.5f, VaultStampRadius = 2.5f, VaultStampHits = 3, VaultProcessedSeconds = 8, VaultProcessedDamage = 1.25f;
        // kept only because MapEnvironmentCombat (blockers) and MapIdentityRun (the ledger's debt) still read them
        public const float ShelfHalf = .7f, ShelfRadius = .4f;
        public const int LedgerMaxDebt = 8;
    }

    public sealed partial class CombatSimulation
    {
        private void InitArchiveVault(MapEnvironmentState s)
        {
            s.VaultNextPage = 3; s.VaultNextWorm = 5; s.VaultNextPot = 12; s.VaultNextStamp = 7;
            vaultBurstQueue.Clear();
            s.VaultClock = s.Anchor + new Vector2(0, 4.4f);
            int lanes = catalog.RunWave - MapIdentity.FirstWave(catalog.RunWave) >= 1 ? 3 : 2;
            for (int i = 0; i < lanes; i++) s.VaultFog.Add(new MapEnvironmentState.VaultLane());
            PlaceVaultFog(s, s.Anchor, true);
            s.VaultNextFog = MapEnvironmentRules.VaultFogEvery;
        }

        private void StepArchiveVault(float deltaTime)
        {
            StepVaultPages();
            StepVaultFog();
            StepVaultWorms(deltaTime);
            StepVaultInk();
            StepVaultStamp();
        }

        /// <summary>A stat the hero gets while standing somewhere on this map (MapBoosts.MapStandingBoost).</summary>
        private float ArchiveVaultStandingBoost(MapStat stat) => 0;
        /// <summary>How this map changes one enemy along one axis (1 = unchanged): ink fog slows and weakens it, the ink pool
        /// quickens and strengthens it, each page a bookworm ate makes it tougher (+30% health) and stronger (+15%), and the stamp's
        /// PROCESSED mark makes it take 25% more damage.</summary>
        private float ArchiveVaultEnemyFactor(UnitState unit, MapEnemyAxis axis)
        {
            MapEnvironmentState s = mapEnv;
            int pages = s.VaultWorms.TryGetValue(unit.Id, out MapEnvironmentState.VaultWorm worm) ? worm.Pages : 0;
            switch (axis)
            {
                case MapEnemyAxis.Speed:
                    return (InVaultFog(unit.Position) ? MapEnvironmentRules.VaultFogSlow : 1) * (InVaultPool(unit.Position) ? MapEnvironmentRules.VaultPoolSpeed : 1);
                case MapEnemyAxis.DamageDealt:
                    return (InVaultFog(unit.Position) ? MapEnvironmentRules.VaultFogWeak : 1) * (InVaultPool(unit.Position) ? MapEnvironmentRules.VaultPoolDamage : 1) * (1 + MapEnvironmentRules.VaultWormDamage * pages);
                case MapEnemyAxis.DamageTaken:
                    // +30% max health a page: the same blow takes a smaller share of it (its health bar keeps its proportion)
                    float taken = 1 / (1 + MapEnvironmentRules.VaultWormHealth * pages);
                    if (s.VaultProcessed.Count > 0 && s.VaultProcessed.TryGetValue(unit.Id, out float until) && until > Elapsed) taken *= MapEnvironmentRules.VaultProcessedDamage;
                    return taken;
                default: return 1;
            }
        }
        /// <summary>The attack cooldown factor of the hero or a friend on this map (below 1 = faster).</summary>
        private float ArchiveVaultAllyCycle(UnitState unit) => 1;
        /// <summary>The hero's pace on this map beyond the move-speed boosts.</summary>
        private float ArchiveVaultHeroPace() => 1;
        /// <summary>A fallen enemy's coins on this map (unchanged): a bookworm spits its pages back, one dying in the ink pool bursts.</summary>
        private int ArchiveVaultCoins(UnitState target, int coins)
        {
            VaultFallen(target.Id, target.Position, target.Definition.Health);
            return coins;
        }
        /// <summary>The ledger's debt is gone with the ledger (MapIdentityRun still reads it).</summary>
        public int LedgerDebt => 0;

        // ------------------------------------------------------------------ flying pages and the CHAPTER
        private static MapStat VaultPageStat(int kind) => kind switch { 0 => MapStat.Damage, 1 => MapStat.AttackSpeed, 2 => MapStat.Armor, 3 => MapStat.Gold, _ => MapStat.MoveSpeed };
        private static float VaultPageValue(int kind) => kind switch
        {
            0 => MapEnvironmentRules.VaultPageDamage, 1 => MapEnvironmentRules.VaultPageHaste, 2 => MapEnvironmentRules.VaultPageArmor,
            3 => MapEnvironmentRules.VaultPageGold, _ => MapEnvironmentRules.VaultPageSpeedBoost
        };
        /// <summary>A page lying on the floor (landed, not in the air).</summary>
        public static bool VaultOnFloor(MapEnvironmentState.VaultPage page, float t) => t >= page.LandAt;
        private void StepVaultPages()
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            if (t >= s.VaultNextPage)
            {
                // a page flies in from one side, crosses past the hero and settles on the floor beyond it
                s.VaultNextPage = t + MapEnvironmentRules.VaultPageEvery;
                float side = mapRandom.Next(2) == 0 ? -1 : 1;
                Vector2 hero = Champion.Position;
                Vector2 from = ClampPosition(hero + new Vector2(-side * 10, MapNext(-2.5f, 2.5f)), .3f);
                Vector2 to = ClampPosition(hero + new Vector2(side * MapNext(3.5f, 5.5f), MapNext(-2.5f, 2.5f)), .6f);
                s.VaultPages.Add(new MapEnvironmentState.VaultPage { From = from, To = to, Position = from, Born = t, LandAt = t + Vector2.Distance(from, to) / MapEnvironmentRules.VaultPageSpeed, Kind = mapRandom.Next(5) });
            }
            float reach = Champion.Definition.Radius + MapEnvironmentRules.VaultPageReach;
            for (int i = s.VaultPages.Count - 1; i >= 0; i--)
            {
                MapEnvironmentState.VaultPage page = s.VaultPages[i];
                if (t >= page.LandAt + MapEnvironmentRules.VaultPageFloor) { s.VaultPages.RemoveAt(i); continue; }
                page.Position = VaultPageAt(page, t);
                if ((page.Position - Champion.Position).sqrMagnitude >= reach * reach || t < page.Born + .15f) continue;
                s.VaultPages.RemoveAt(i);
                VaultCatch(page);
            }
        }
        /// <summary>Where a page is: fluttering along its way while it flies (a sway across it), at its spot once on the floor.</summary>
        public static Vector2 VaultPageAt(MapEnvironmentState.VaultPage page, float t)
        {
            if (t >= page.LandAt) return page.To;
            float f = Mathf.Clamp01((t - page.Born) / Mathf.Max(.01f, page.LandAt - page.Born));
            Vector2 way = page.To - page.From, across = way.sqrMagnitude > .01f ? new Vector2(-way.y, way.x).normalized : Vector2.up;
            float sway = Mathf.Min(.6f, way.magnitude * .12f);
            return Vector2.Lerp(page.From, page.To, f) + across * Mathf.Sin(f * Mathf.PI * 3) * sway * (1 - f);
        }
        private void VaultCatch(MapEnvironmentState.VaultPage page)
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            GrantMapBoost(VaultPageStat(page.Kind), VaultPageValue(page.Kind), MapEnvironmentRules.VaultPageSeconds, "Page " + page.Kind);
            s.VaultCaughtAt = t; s.VaultCaughtKind = page.Kind; s.VaultCaughtWhere = page.Position; s.VaultCaught++;
            s.VaultCatches.RemoveAll(c => c.x < t - MapEnvironmentRules.VaultChapterWindow);
            s.VaultCatches.Add(new Vector2(t, page.Kind));
            if (s.VaultCatches.Count < MapEnvironmentRules.VaultChapterPages) return;
            // a CHAPTER: the last three pages' boosts all run together for 20 s
            s.VaultChapterAt = t;
            var kinds = new HashSet<int>();
            foreach (Vector2 c in s.VaultCatches) kinds.Add(Mathf.RoundToInt(c.y));
            foreach (int kind in kinds) GrantMapBoost(VaultPageStat(kind), VaultPageValue(kind), MapEnvironmentRules.VaultChapterSeconds, "Page " + kind, "CHAPTER");
            s.VaultCatches.Clear();
        }

        // ------------------------------------------------------------------ ink fog
        public bool InVaultFog(Vector2 position)
        {
            MapEnvironmentState s = mapEnv;
            if (s == null || s.Theme != MapTheme.ArchiveVault) return false;
            float half = MapEnvironmentRules.VaultFogHalf;
            foreach (MapEnvironmentState.VaultLane lane in s.VaultFog)
            {
                Vector2 ab = lane.B - lane.A; float f = ab.sqrMagnitude < 1e-4f ? 0 : Mathf.Clamp01(Vector2.Dot(position - lane.A, ab) / ab.sqrMagnitude);
                if ((position - (lane.A + ab * f)).sqrMagnitude < half * half) return true;
            }
            return false;
        }
        /// <summary>Lays the lanes across the room round a centre: rows apart, each at a slight slant.</summary>
        private void PlaceVaultFog(MapEnvironmentState s, Vector2 centre, bool at)
        {
            int n = s.VaultFog.Count;
            for (int i = 0; i < n; i++)
            {
                MapEnvironmentState.VaultLane lane = s.VaultFog[i];
                Vector2 mid = ClampPosition(centre + new Vector2(MapNext(-3, 3), (i - (n - 1) * .5f) * 3.2f + MapNext(-.8f, .8f)), 1);
                float slant = MapNext(-25, 25) * Mathf.Deg2Rad; Vector2 half = new Vector2(Mathf.Cos(slant), Mathf.Sin(slant)) * MapEnvironmentRules.VaultFogLength * .5f;
                lane.FromA = lane.A; lane.FromB = lane.B; lane.ToA = mid - half; lane.ToB = mid + half;
                if (at) { lane.A = lane.FromA = lane.ToA; lane.B = lane.FromB = lane.ToB; }
            }
        }
        private void StepVaultFog()
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            if (t >= s.VaultNextFog) { s.VaultNextFog = t + MapEnvironmentRules.VaultFogEvery; s.VaultFogMovedAt = t; PlaceVaultFog(s, Champion.Position, false); }
            float p = Mathf.SmoothStep(0, 1, Mathf.Clamp01((t - s.VaultFogMovedAt) / MapEnvironmentRules.VaultFogMove));
            foreach (MapEnvironmentState.VaultLane lane in s.VaultFog) { lane.A = Vector2.Lerp(lane.FromA, lane.ToA, p); lane.B = Vector2.Lerp(lane.FromB, lane.ToB, p); }
        }

        // ------------------------------------------------------------------ bookworms
        private void StepVaultWorms(float deltaTime)
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            // a bookworm that fell since the last step spits its pages back (the coin hook catches most at the very spot)
            List<int> fallen = null;
            foreach (KeyValuePair<int, MapEnvironmentState.VaultWorm> pair in s.VaultWorms)
            {
                UnitState u = UnitWithId(pair.Key);
                if (u == null || !u.Alive) (fallen ??= new List<int>()).Add(pair.Key);
                else pair.Value.Last = u.Position;
            }
            if (fallen != null) foreach (int id in fallen) VaultSpit(id, s.VaultWorms[id].Last);
            int hunting = 0;
            foreach (MapEnvironmentState.VaultWorm worm in s.VaultWorms.Values) if (worm.Pages < MapEnvironmentRules.VaultWormPages) hunting++;
            // a page on the floor draws an ordinary enemy near it to it as a bookworm (a few at a time)
            if (t >= s.VaultNextWorm && hunting < MapEnvironmentRules.VaultWormsHunting && s.VaultWorms.Count < MapEnvironmentRules.VaultWormsAtOnce)
            {
                UnitState best = null; float bestDistance = MapEnvironmentRules.VaultWormSeek * MapEnvironmentRules.VaultWormSeek;
                foreach (MapEnvironmentState.VaultPage page in s.VaultPages)
                {
                    if (!VaultOnFloor(page, t)) continue;
                    foreach (UnitState u in units)
                    {
                        if (!u.Alive || u.Faction != Faction.Enemy || u.Definition.IsBoss || u.Elite || u.SwarmMinion || s.VaultWorms.ContainsKey(u.Id)) continue;
                        float d = (u.Position - page.To).sqrMagnitude;
                        if (d < bestDistance) { best = u; bestDistance = d; }
                    }
                }
                if (best != null) { s.VaultWorms[best.Id] = new MapEnvironmentState.VaultWorm { Last = best.Position, BornAt = t }; s.VaultNextWorm = t + MapEnvironmentRules.VaultWormEvery; }
            }
            // each hungry bookworm crawls to the nearest page on the floor and eats it
            foreach (KeyValuePair<int, MapEnvironmentState.VaultWorm> pair in s.VaultWorms)
            {
                MapEnvironmentState.VaultWorm worm = pair.Value;
                if (worm.Pages >= MapEnvironmentRules.VaultWormPages) continue;
                UnitState u = UnitWithId(pair.Key); if (u == null || !u.Alive) continue;
                MapEnvironmentState.VaultPage target = null; float near = MapEnvironmentRules.VaultWormSeek * MapEnvironmentRules.VaultWormSeek * 1.5f;
                foreach (MapEnvironmentState.VaultPage page in s.VaultPages)
                {
                    if (!VaultOnFloor(page, t)) continue;
                    float d = (page.To - u.Position).sqrMagnitude;
                    if (d < near) { target = page; near = d; }
                }
                if (target == null) continue;
                if (near < Mathf.Pow(u.Definition.Radius + .45f, 2))
                {
                    s.VaultPages.Remove(target);
                    worm.Pages++; worm.Kinds.Add(target.Kind); worm.AteAt = t;
                    continue;
                }
                if (u.SleepUntil > t || u.FrozenUntil > t || u.BodyLift > .1f || u.StaggerUntil > t) continue;
                // it turns its own step toward the page (at least half its pace, so a worm held up at the hero still crawls off)
                float step = Mathf.Max((u.Position - u.PreviousPosition).magnitude, u.Definition.Speed * deltaTime * .5f);
                u.Position = ClampPosition(u.PreviousPosition + (target.To - u.PreviousPosition).normalized * Mathf.Min(step, Mathf.Sqrt(near)), u.Definition.Radius);
            }
        }
        /// <summary>A bookworm fell: every page it ate comes back out onto the floor round it, with 5 gold a page.</summary>
        private void VaultSpit(int id, Vector2 at)
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            if (!s.VaultWorms.TryGetValue(id, out MapEnvironmentState.VaultWorm worm)) return;
            s.VaultWorms.Remove(id);
            if (worm.Pages <= 0) return;
            for (int i = 0; i < worm.Kinds.Count; i++)
            {
                float a = (i * 120 + 30) * Mathf.Deg2Rad;
                Vector2 to = ClampPosition(at + new Vector2(Mathf.Cos(a), Mathf.Sin(a) * .7f) * .9f, .3f);
                s.VaultPages.Add(new MapEnvironmentState.VaultPage { From = at, To = to, Position = at, Born = t, LandAt = t + .5f, Kind = worm.Kinds[i], Spat = true });
            }
            float perCoin = Mathf.Max(.01f, KitCoinScale * Mathf.Max(.01f, catalog.CoinValueScale));
            DropLoot(LootKind.Coin, at, Mathf.Max(1, Mathf.RoundToInt(MapEnvironmentRules.VaultWormGold * worm.Pages / perCoin)));
            if (s.VaultSpits.Count >= 8) s.VaultSpits.RemoveAt(0);
            s.VaultSpits.Add(new Vector3(at.x, at.y, t));
        }

        // ------------------------------------------------------------------ the spilled inkpot
        public bool InVaultPool(Vector2 position)
        {
            MapEnvironmentState s = mapEnv;
            if (s == null || s.Theme != MapTheme.ArchiveVault || Elapsed < s.VaultSpillAt || Elapsed >= s.VaultPoolUntil) return false;
            return (position - s.VaultPool).sqrMagnitude < MapEnvironmentRules.VaultPoolRadius * MapEnvironmentRules.VaultPoolRadius;
        }
        /// <summary>An enemy fell (the coin hook, or seen gone since the last step): a bookworm spits its pages back; one that
        /// fell in the ink pool bursts (once).</summary>
        private void VaultFallen(int id, Vector2 at, float maxHealth)
        {
            MapEnvironmentState s = mapEnv;
            if (s.VaultWorms.ContainsKey(id)) VaultSpit(id, at);
            if (InVaultPool(at) && s.VaultBurst.Add(id))
            {
                if (s.VaultBursts.Count >= 40) s.VaultBursts.RemoveAt(0);
                s.VaultBursts.Add(new Vector3(at.x, at.y, Elapsed));
                vaultBurstQueue.Add(new Vector4(at.x, at.y, maxHealth, id));
            }
        }
        private readonly List<Vector4> vaultBurstQueue = new List<Vector4>();
        private void StepVaultInk()
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            if (t >= s.VaultNextPot)
            {
                // the pot stands beside where the ink will run, toward the enemies round the hero, and tips sideways into it
                s.VaultNextPot = t + MapEnvironmentRules.VaultPotEvery; s.VaultPotAt = t;
                Vector2 toward = Vector2.zero; int count = 0;
                foreach (UnitState u in units)
                    if (u.Alive && u.Faction == Faction.Enemy && (u.Position - Champion.Position).sqrMagnitude < 64) { toward += u.Position - Champion.Position; count++; }
                Vector2 dir = count > 0 && toward.sqrMagnitude > .01f ? toward.normalized : new Vector2(MapNext(-1, 1), MapNext(-1, 1)).normalized;
                if (dir.sqrMagnitude < .01f) dir = Vector2.right;
                s.VaultPool = ClampPosition(Champion.Position + dir * MapNext(3, 4.5f), MapEnvironmentRules.VaultPoolRadius * .6f);
                float side = mapRandom.Next(2) == 0 ? -1 : 1;
                s.VaultPot = ClampPosition(s.VaultPool + new Vector2(side * (MapEnvironmentRules.VaultPoolRadius + .4f), .3f), .8f);
                s.VaultInPool.Clear();
            }
            bool pool = t >= s.VaultSpillAt && t < s.VaultPoolUntil;
            if (!pool && vaultBurstQueue.Count == 0) { s.VaultInPool.Clear(); return; }
            // an enemy seen in the pool last step and fallen since bursts too (coinless ones as well)
            if (pool)
            {
                foreach (KeyValuePair<int, Vector3> seen in s.VaultInPool)
                {
                    UnitState u = UnitWithId(seen.Key);
                    if (u == null || !u.Alive) VaultFallen(seen.Key, u != null ? u.Position : new Vector2(seen.Value.x, seen.Value.y), seen.Value.z);
                }
                s.VaultInPool.Clear();
                foreach (UnitState u in units)
                    if (u.Alive && u.Faction == Faction.Enemy && InVaultPool(u.Position)) s.VaultInPool[u.Id] = new Vector3(u.Position.x, u.Position.y, u.Definition.Health);
            }
            // each burst: half the fallen enemy's max health on the enemies within 1.5 units; those it fells in the pool burst next
            CombatCause cause = default; bool caused = false;
            for (int i = 0; i < vaultBurstQueue.Count && i < 48; i++)
            {
                Vector4 b = vaultBurstQueue[i]; Vector2 at = new Vector2(b.x, b.y);
                if (!caused) { cause = NewCause(Champion.Id).Child("map-ink-burst"); caused = true; }
                float r = MapEnvironmentRules.VaultBurstRadius;
                foreach (UnitState u in units.ToArray())
                {
                    if (!u.Alive || u.Faction != Faction.Enemy || u.Id == (int)b.w || (u.Position - at).sqrMagnitude > r * r) continue;
                    float hit = b.z * MapEnvironmentRules.VaultBurstShare;
                    if (u.Definition.IsBoss) hit = Mathf.Min(hit, u.Definition.Health * MapEnvironmentRules.VaultBurstBossCap);
                    ApplyDamage(u, hit, Faction.Champion, "Ink burst", u.Position - at, cause: cause);
                    if (!u.Alive) VaultFallen(u.Id, u.Position, u.Definition.Health); // (the coin hook has usually queued it already)
                    if (Phase != RunPhase.Combat) { vaultBurstQueue.Clear(); return; }
                }
            }
            vaultBurstQueue.RemoveRange(0, Mathf.Min(48, vaultBurstQueue.Count));
        }

        // ------------------------------------------------------------------ the stamp from the sky
        private void StepVaultStamp()
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            if (t >= s.VaultNextStamp)
            {
                // the densest crowd near the hero: the enemy with the most others within the stamp's reach
                UnitState best = null; int most = 0; float r2 = MapEnvironmentRules.VaultStampRadius * MapEnvironmentRules.VaultStampRadius;
                foreach (UnitState u in units)
                {
                    if (!u.Alive || u.Faction != Faction.Enemy || (u.Position - Champion.Position).sqrMagnitude > 100) continue;
                    int around = 0;
                    foreach (UnitState v in units) if (v.Alive && v.Faction == Faction.Enemy && (v.Position - u.Position).sqrMagnitude < r2) around++;
                    if (around > most) { most = around; best = u; }
                }
                if (best == null) { s.VaultNextStamp = t + 2; return; }
                Vector2 centre = Vector2.zero; int n = 0;
                foreach (UnitState v in units) if (v.Alive && v.Faction == Faction.Enemy && (v.Position - best.Position).sqrMagnitude < r2) { centre += v.Position; n++; }
                s.VaultStamp = ClampPosition(n > 0 ? centre / n : best.Position, .5f);
                s.VaultStampAt = t; s.VaultStamped = false; s.VaultNextStamp = t + MapEnvironmentRules.VaultStampEvery;
            }
            if (s.VaultStamped || s.VaultStampAt < 0 || t < s.VaultStampHitAt) return;
            // it lands: three of the hero's hits on every enemy under it; the ones left standing are PROCESSED
            s.VaultStamped = true;
            CombatCause cause = NewCause(Champion.Id).Child("map-stamp");
            float hit = Champion.Definition.Weapon.Damage * MapEnvironmentRules.VaultStampHits, radius = MapEnvironmentRules.VaultStampRadius;
            foreach (UnitState u in units.ToArray())
            {
                if (!u.Alive || u.Faction != Faction.Enemy || (u.Position - s.VaultStamp).sqrMagnitude > Mathf.Pow(radius + u.Definition.Radius * .5f, 2)) continue;
                ApplyDamage(u, hit, Faction.Champion, "Stamp", Vector2.down, cause: cause);
                if (u.Alive) s.VaultProcessed[u.Id] = t + MapEnvironmentRules.VaultProcessedSeconds;
                if (Phase != RunPhase.Combat) return;
            }
        }

        /// <summary>For captures: a page flying past the hero and two on the floor, a bookworm with a page, the fog round the hero,
        /// the pot tipping into its pool, and the stamp about to land.</summary>
        private void ArchiveVaultCaptureFixture()
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed; Vector2 hero = Champion.Position;
            s.VaultPages.Clear();
            s.VaultPages.Add(new MapEnvironmentState.VaultPage { From = hero + new Vector2(-6, 1.6f), To = hero + new Vector2(4, 1.2f), Born = t - 2.2f, LandAt = t + 2.4f, Kind = 0 });
            s.VaultPages.Add(new MapEnvironmentState.VaultPage { From = hero + new Vector2(-2.6f, -1.6f), To = hero + new Vector2(-2.6f, -1.6f), Born = t - 3, LandAt = t - 2, Kind = 3 });
            s.VaultPages.Add(new MapEnvironmentState.VaultPage { From = hero + new Vector2(3, -2.2f), To = hero + new Vector2(3, -2.2f), Born = t - 3, LandAt = t - 2, Kind = 1 });
            foreach (MapEnvironmentState.VaultPage page in s.VaultPages) page.Position = VaultPageAt(page, t);
            s.VaultNextPage = t + 30;
            PlaceVaultFog(s, hero, true); s.VaultNextFog = t + 30;
            foreach (UnitState u in units)
                if (u.Alive && u.Faction == Faction.Enemy && !u.Definition.IsBoss && (u.Position - hero).sqrMagnitude < 36)
                {
                    var worm = new MapEnvironmentState.VaultWorm { Pages = 2, Last = u.Position, BornAt = t - 3, AteAt = t - .3f };
                    worm.Kinds.Add(2); worm.Kinds.Add(4); s.VaultWorms[u.Id] = worm; break;
                }
            s.VaultNextWorm = t + 30;
            s.VaultPool = hero + new Vector2(-3.4f, 1.8f); s.VaultPot = s.VaultPool + new Vector2(-(MapEnvironmentRules.VaultPoolRadius + .4f), .3f);
            s.VaultPotAt = t - MapEnvironmentRules.VaultPotWarn - .8f; s.VaultNextPot = t + 30;
            s.VaultClock = hero + new Vector2(-1.2f, 4.2f);
            s.VaultStamp = hero + new Vector2(3.4f, 1.6f); s.VaultStampAt = t - .9f; s.VaultStamped = false; s.VaultNextStamp = t + 30;
        }
    }
}
