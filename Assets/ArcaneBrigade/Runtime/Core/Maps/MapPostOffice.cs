using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    // The Post Office (waves 5-8; Docs/Map_Mechanics_2026-10-25.md section 2). Requirement 25/10/2026: each map's state, tunings
    // and rules live in its own file; MapEnvironmentCombat and MapBoosts only dispatch to the hooks below. Five slots:
    // chute letters (boost), paper wind (atmosphere), stamped enemies (enemy), the giant parcel (unique 1), paper planes
    // (unique 2). The old gust that pushed bodies is gone.

    public sealed partial class MapEnvironmentState
    {
        // ---- chute letters: each gives one of the five base stats (Stat = (int)MapStat)
        public sealed class Letter { public Vector2 Position; public float Born, Until; public bool Gold, Gone, Grounded; public int Style, Stat; }
        public float ChuteAt = -100, NextChute, ChuteX, ChuteTop, ChuteBottom;
        public int LettersToDrop; public float NextLetter;
        public readonly List<Letter> Letters = new List<Letter>();
        /// <summary>Letters caught from the chute's current drop, and every catch so far; until when the last letter's boost runs.</summary>
        public int LetterStreak, LettersCaught; public float BoostUntil;
        /// <summary>The stats caught from the current drop (a full delivery lengthens each), and when the last full delivery was.</summary>
        public readonly List<int> PostDropStats = new List<int>();
        public float PostDeliveredAt = -100;

        // ---- paper wind: the whole room, one way, for 4 s every 11 s (warned 1 s before)
        public float GustWarnAt = -100, NextGust; public Vector2 PostWind = Vector2.right;
        public float GustAt => GustWarnAt + MapEnvironmentRules.GustWarning;
        public float GustUntil => GustAt + MapEnvironmentRules.GustSeconds;
        public bool PostWindBlowing(float t) => t >= GustAt && t < GustUntil;

        // ---- stamped enemies: id -> until when the stamp burns, and when it was stamped
        public readonly Dictionary<int, float> Stamped = new Dictionary<int, float>();
        public readonly Dictionary<int, float> PostStampedAt = new Dictionary<int, float>();
        public readonly Dictionary<int, UnitState> PostStampHolders = new Dictionary<int, UnitState>();
        public float PostNextStamp;
        public sealed class PostBurst { public Vector2 Position; public float At, Radius; }
        /// <summary>The latest stamp bursts (the presentation's moments).</summary>
        public readonly List<PostBurst> PostBursts = new List<PostBurst>();

        // ---- the giant parcel (about a third of nights)
        public int ParcelState; // 0 not yet (or not tonight), 1 falling, 2 sitting, 3 broken open (gold), 4 opened into an elite
        public Vector2 Parcel; public float ParcelDue, ParcelAt, HatchAt;
        public bool PostParcelNight;
        /// <summary>The parcel's body: shots and blades break it from any range.</summary>
        public readonly StruckProp ParcelProp = new StruckProp();

        // ---- paper planes: a pass every 18 s along a lane 1.5 units wide (warned 1.5 s before)
        public Vector2 PostLaneFrom, PostLaneTo; public float PostPlanesWarnAt = -100, PostNextPlanes; public int PostPlaneCount;
        public float PostPlanesAt => PostPlanesWarnAt + MapEnvironmentRules.PostPlaneWarn;
        public float PostLaneLength => (PostLaneTo - PostLaneFrom).magnitude;
        public Vector2 PostLaneDirection => (PostLaneTo - PostLaneFrom).sqrMagnitude > 1e-4f ? (PostLaneTo - PostLaneFrom).normalized : Vector2.right;
        public float PostPlanesEnd => PostPlanesAt + (PostPlaneCount - 1) * MapEnvironmentRules.PostPlaneGap + PostLaneLength / MapEnvironmentRules.PostPlaneSpeed;
        /// <summary>How far along the lane plane i is at time t (negative before it is launched, past the length once through).</summary>
        public float PostPlaneAlong(int i, float t) => (t - PostPlanesAt - i * MapEnvironmentRules.PostPlaneGap) * MapEnvironmentRules.PostPlaneSpeed;
        /// <summary>Plane i's sideways place in the lane.</summary>
        public static float PostPlaneSide(int i) => (((i * 37) % 9) - 4) / 4f * MapEnvironmentRules.PostPlaneSpread;
        public sealed class PostPlaneStrike { public Vector2 Position; public float At; }
        public readonly List<PostPlaneStrike> PostPlaneHits = new List<PostPlaneStrike>();
        public readonly Dictionary<int, int> PostPassHits = new Dictionary<int, int>();
        public readonly List<HashSet<int>> PostPlaneVictims = new List<HashSet<int>>();
    }

    public static partial class MapEnvironmentRules
    {
        // chute letters: 5 every 13 s, the third gold; 8 s each (gold 11 s); three from one drop: all three +4 s
        public const int FullDelivery = 3; public const float FullDeliveryBonus = 4;
        public const float ChuteEvery = 13, ChuteWarn = 1.2f, LetterSpeed = 3.1f, BoostSeconds = 8, GoldBoostSeconds = 11, PostLetterReach = .45f, PostLetterLies = 8;
        public const float PostLetterDamage = .15f, PostLetterHaste = .15f, PostLetterArmor = 2, PostLetterGold = 1, PostLetterSpeed = .15f;
        // paper wind: 4 s every 11 s, warned 1 s; walking against it 20% slower; enemy shots drift and fly 25% slower
        public const float GustEvery = 11, GustWarning = 1, GustSeconds = 4, PostWindAgainst = .8f, PostWindShotPace = .75f, PostWindDrift = 1.5f;
        // stamped enemies: one every 10 s for 8 s (+20% pace, +20% damage dealt); killed stamped, it bursts (1.5 units, 30% of its max health)
        public const float PostStampEvery = 10, StampSeconds = 8, PostStampPace = 1.2f, StampDamage = 1.2f, PostStampRadius = 1.5f, PostStampBurst = .3f, PostStampReach = 7.5f;
        // the giant parcel: a third of nights, warned 1.3 s, an elite after 11 s; about 4 ordinary enemies' health;
        // breaking it bursts in 2.8 units for 4x the hero hit and spills 10-25 gold
        public const float ParcelShare = .3f, ParcelWarn = 1.3f, ParcelHatch = 11, PostParcelChance = 1 / 3f, PostParcelHealth = 4, PostParcelRadius = .95f;
        public const float ParcelBurstRadius = 2.8f, ParcelBurstHits = 4f;
        public const int PostParcelGoldMin = 10, PostParcelGoldMax = 25;
        // paper planes: every 18 s, 8-12 along a lane 1.5 wide, warned 1.5 s; 50% of the hero hit each, at most 3 per enemy a pass
        // requirement 31/10/2026: slower (5 units/s), flying the wind's way, and each strike bursts over 1.3 units
        public const float PostPlaneBurst = 1.3f, PostPlaneBurstShare = .6f;
        public const float PostPlaneEvery = 18, PostPlaneWarn = 1.5f, PostPlaneHalf = .75f, PostPlaneSpeed = 5, PostPlaneGap = .3f, PostPlaneSpread = .42f, PostPlaneHit = .5f, PostLaneHalfLength = 13;
        public const int PostPlanesMin = 8, PostPlanesMax = 12, PostPlaneHitsPerEnemy = 3;
    }

    public sealed partial class CombatSimulation
    {
        private const int PostParcelVictim = -7201;
        private static readonly string[] PostLetterSources = { "letter-damage", "letter-haste", "letter-armor", "letter-gold", "letter-speed" };
        private readonly List<int> postGone = new List<int>();
        private CombatCause postPlaneCause;

        private void InitPostOffice(MapEnvironmentState s)
        {
            s.NextChute = 5; s.NextGust = 8; s.PostNextStamp = 6; s.PostNextPlanes = 9;
            // requirement 25/10/2026: a giant parcel lands on about a third of nights
            s.PostParcelNight = mapRandom.NextDouble() < MapEnvironmentRules.PostParcelChance;
            s.ParcelDue = s.PostParcelNight ? Mathf.Max(6, catalog.WaveDuration * MapEnvironmentRules.ParcelShare) : float.MaxValue;
        }

        private void StepPostOffice(float deltaTime)
        {
            PostStepChute(deltaTime);
            PostStepWind(deltaTime);
            PostStepStamps();
            PostStepParcel();
            PostStepPlanes(deltaTime);
        }

        /// <summary>A stat the hero gets while standing somewhere on this map (MapBoosts.MapStandingBoost).</summary>
        private float PostOfficeStandingBoost(MapStat stat) => 0;
        /// <summary>How this map changes one enemy along one axis (1 = unchanged): a stamped enemy is 20% quicker and hits 20%
        /// harder; walking against the paper wind is 20% slower.</summary>
        private float PostOfficeEnemyFactor(UnitState unit, MapEnemyAxis axis)
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            bool stamped = s.Stamped.Count > 0 && s.Stamped.TryGetValue(unit.Id, out float until) && until > t;
            switch (axis)
            {
                case MapEnemyAxis.Speed:
                    float pace = stamped ? MapEnvironmentRules.PostStampPace : 1;
                    if (s.PostWindBlowing(t))
                    {
                        Vector2 step = unit.Position - unit.PreviousPosition;
                        if (step.sqrMagnitude > 1e-8f && Vector2.Dot(step.normalized, s.PostWind) < -.3f) pace *= MapEnvironmentRules.PostWindAgainst;
                    }
                    return pace;
                case MapEnemyAxis.DamageDealt: return stamped ? MapEnvironmentRules.StampDamage : 1;
                default: return 1;
            }
        }
        /// <summary>The attack cooldown factor of the hero or a friend on this map (below 1 = faster).</summary>
        private float PostOfficeAllyCycle(UnitState unit) => 1;
        /// <summary>The hero's pace on this map beyond the move-speed boosts.</summary>
        private float PostOfficeHeroPace() => 1;
        /// <summary>A fallen enemy's coins on this map.</summary>
        private int PostOfficeCoins(UnitState target, int coins) => coins;

        // ------------------------------------------------------------------ chute letters
        private void PostStepChute(float deltaTime)
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            if (t >= s.NextChute)
            {
                s.NextChute = t + MapEnvironmentRules.ChuteEvery; s.ChuteAt = t;
                float side = MapNext(1.5f, 4) * (mapRandom.Next(2) == 0 ? -1 : 1);
                Vector2 half = catalog.ArenaSize * .5f - Vector2.one;
                s.ChuteX = Mathf.Clamp(Champion.Position.x + side, -half.x, half.x);
                s.ChuteTop = Mathf.Min(half.y, Champion.Position.y + 4.6f); s.ChuteBottom = Mathf.Max(-half.y, Champion.Position.y - 5.2f);
                s.LettersToDrop = 5; s.NextLetter = t + MapEnvironmentRules.ChuteWarn; s.LetterStreak = 0; s.PostDropStats.Clear();
                s.Shown.Add(MapIdentity.Element.Chute);
            }
            if (s.LettersToDrop > 0 && t >= s.NextLetter)
            {
                s.Letters.Add(new MapEnvironmentState.Letter { Position = new Vector2(s.ChuteX + MapNext(-.25f, .25f), s.ChuteTop), Born = t, Gold = s.LettersToDrop == 3, Style = mapRandom.Next(3), Stat = mapRandom.Next(5) });
                s.LettersToDrop--; s.NextLetter = t + .45f;
            }
            float grab = Champion.Definition.Radius + MapEnvironmentRules.PostLetterReach;
            for (int i = s.Letters.Count - 1; i >= 0; i--)
            {
                MapEnvironmentState.Letter letter = s.Letters[i];
                if (letter.Grounded) { if (t >= letter.Until) { s.Letters.RemoveAt(i); continue; } }
                else
                {
                    letter.Position += new Vector2(Mathf.Sin((t - letter.Born) * 3 + i) * .35f * deltaTime, -MapEnvironmentRules.LetterSpeed * deltaTime);
                    if (s.PostWindBlowing(t)) letter.Position += s.PostWind * MapEnvironmentRules.PostWindDrift * .5f * deltaTime; // the wind carries the letters a little
                    if (letter.Position.y < s.ChuteBottom) { s.Letters.RemoveAt(i); continue; }
                }
                if ((letter.Position - Champion.Position).sqrMagnitude >= grab * grab) continue;
                PostCatchLetter(letter);
                MapCaught(letter.Position, letter.Gold ? new Color(1f, .84f, .4f) : MapBoostText.Colour((MapStat)Mathf.Clamp(letter.Stat, 0, 4)));
                s.Letters.RemoveAt(i);
            }
        }
        private void PostCatchLetter(MapEnvironmentState.Letter letter)
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            MapStat stat = (MapStat)Mathf.Clamp(letter.Stat, 0, 4);
            float seconds = letter.Gold ? MapEnvironmentRules.GoldBoostSeconds : MapEnvironmentRules.BoostSeconds;
            GrantMapBoost(stat, PostLetterValue(stat), seconds, PostLetterSources[(int)stat], letter.Gold ? "GOLD LETTER" : null);
            s.BoostUntil = Mathf.Max(s.BoostUntil, t + seconds); s.LettersCaught++;
            if (letter.Grounded) return; // a stamp's letter is not part of a drop
            s.LetterStreak++; s.PostDropStats.Add((int)stat);
            if (s.LetterStreak != MapEnvironmentRules.FullDelivery) return;
            // a full delivery: the three letters' boosts all run together, each 4 s longer
            s.PostDeliveredAt = t;
            foreach (MapBoost b in s.Boosts)
                if (b.Until > t && System.Array.IndexOf(PostLetterSources, b.Source) >= 0 && s.PostDropStats.Contains((int)b.Stat))
                    b.Until = Mathf.Min(t + MapBoostCap, b.Until + MapEnvironmentRules.FullDeliveryBonus);
            MapSay("Full delivery!", new Color(1f, .82f, .35f));
        }
        private static float PostLetterValue(MapStat stat) => stat switch
        {
            MapStat.Damage => MapEnvironmentRules.PostLetterDamage, MapStat.AttackSpeed => MapEnvironmentRules.PostLetterHaste,
            MapStat.Armor => MapEnvironmentRules.PostLetterArmor, MapStat.Gold => MapEnvironmentRules.PostLetterGold, _ => MapEnvironmentRules.PostLetterSpeed
        };

        // ------------------------------------------------------------------ paper wind
        private void PostStepWind(float deltaTime)
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            if (t >= s.NextGust)
            {
                s.NextGust = t + MapEnvironmentRules.GustEvery; s.GustWarnAt = t;
                float angle = MapNext(-15, 15) + (mapRandom.Next(2) == 0 ? 0 : 180);
                s.PostWind = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                s.Shown.Add(MapIdentity.Element.Gust);
            }
            if (!s.PostWindBlowing(t)) return;
            // enemy shots drift with the wind and fly 25% slower (nothing on our side is pushed)
            for (int i = 0; i < projectiles.Count; i++)
            {
                ProjectileState p = projectiles[i];
                if (p.Faction != Faction.Enemy) continue;
                Vector2 step = p.Position - p.PreviousPosition;
                if (step.sqrMagnitude > 4) continue;
                p.Position = p.PreviousPosition + step * MapEnvironmentRules.PostWindShotPace + s.PostWind * MapEnvironmentRules.PostWindDrift * deltaTime;
            }
        }

        // ------------------------------------------------------------------ stamped enemies
        private void PostStepStamps()
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            if (t >= s.PostNextStamp)
            {
                s.PostNextStamp = t + MapEnvironmentRules.PostStampEvery;
                int seen = 0; UnitState pick = null; float reach = MapEnvironmentRules.PostStampReach * MapEnvironmentRules.PostStampReach;
                foreach (UnitState u in StableUnits)
                {
                    if (!u.Alive || u.Faction != Faction.Enemy || u.Definition.IsBoss || u.EntranceUntil > t || s.PostStampHolders.ContainsKey(u.Id)) continue;
                    if ((u.Position - Champion.Position).sqrMagnitude > reach) continue;
                    if (mapRandom.Next(++seen) == 0) pick = u;
                }
                if (pick != null)
                {
                    s.Stamped[pick.Id] = t + MapEnvironmentRules.StampSeconds; s.PostStampedAt[pick.Id] = t; s.PostStampHolders[pick.Id] = pick;
                }
            }
            if (s.PostStampHolders.Count == 0) return;
            postGone.Clear();
            foreach (KeyValuePair<int, UnitState> pair in s.PostStampHolders)
            {
                UnitState u = pair.Value;
                float until = s.Stamped.TryGetValue(pair.Key, out float x) ? x : 0;
                if (!u.Alive) { if (u.Health <= 0 && until >= t - .1f) PostBurstStamp(u); postGone.Add(pair.Key); }
                else if (until <= t) postGone.Add(pair.Key);
            }
            foreach (int id in postGone) { s.PostStampHolders.Remove(id); s.Stamped.Remove(id); s.PostStampedAt.Remove(id); }
        }
        /// <summary>A stamped enemy killed before its stamp burns out bursts: 30% of its max health on the enemies within 1.5
        /// units (bosses at most 5% of theirs), and it drops a letter.</summary>
        private void PostBurstStamp(UnitState dead)
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            float blast = MapEnvironmentRules.PostStampBurst * Mathf.Max(1, dead.Definition.Health);
            CombatCause cause = NewCause(Champion.Id).Child("map-stamp-burst");
            foreach (UnitState u in RoomUnitsSnapshot())
            {
                if (u == dead || !u.Alive || u.Faction != Faction.Enemy) continue;
                float reach = MapEnvironmentRules.PostStampRadius + u.Definition.Radius * .5f;
                if ((u.Position - dead.Position).sqrMagnitude > reach * reach) continue;
                ApplyDamage(u, u.Definition.IsBoss ? Mathf.Min(blast, u.Definition.Health * .05f) : blast, Faction.Champion, "Stamp burst", u.Position - dead.Position, cause: cause);
            }
            s.PostBursts.Add(new MapEnvironmentState.PostBurst { Position = dead.Position, At = t, Radius = MapEnvironmentRules.PostStampRadius });
            if (s.PostBursts.Count > 10) s.PostBursts.RemoveAt(0);
            s.Letters.Add(new MapEnvironmentState.Letter { Position = ClampPosition(dead.Position, .4f), Born = t, Until = t + MapEnvironmentRules.PostLetterLies, Grounded = true, Style = mapRandom.Next(3), Stat = mapRandom.Next(5) });
        }

        // ------------------------------------------------------------------ the giant parcel
        private void PostStepParcel()
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            switch (s.ParcelState)
            {
                case 0:
                    if (!s.PostParcelNight || t < s.ParcelDue) return;
                    float angle = MapNext(0, Mathf.PI * 2);
                    s.Parcel = ClampPosition(Champion.Position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * .7f) * 4.2f, 1.5f);
                    s.ParcelAt = t; s.ParcelState = 1; s.Shown.Add(MapIdentity.Element.Parcel);
                    break;
                case 1:
                    if (t < s.ParcelAt + MapEnvironmentRules.ParcelWarn) return;
                    s.ParcelState = 2; s.ParcelAt = t; s.HatchAt = t + MapEnvironmentRules.ParcelHatch; // it lands; nothing is pushed
                    s.ParcelProp.Reset(s.Parcel + Vector2.up * .55f, MapEnvironmentRules.PostParcelRadius, MapEnvironmentRules.PostParcelHealth * RoomOrdinaryHealth());
                    break;
                case 2:
                    RoomStrikeProp(s.ParcelProp, PostParcelVictim);
                    if (s.ParcelProp.Health <= 0)
                    {
                        s.ParcelState = 3; s.ParcelAt = t;
                        RoomSpillGold(s.Parcel, MapEnvironmentRules.PostParcelGoldMin + mapRandom.Next(MapEnvironmentRules.PostParcelGoldMax - MapEnvironmentRules.PostParcelGoldMin + 1), 5);
                        // it bursts: splinters of the crate hit every enemy close by (four of the hero's own hits)
                        CombatCause burst = NewCause(Champion.Id).Child("map-parcel-burst");
                        float splinters = RoomHeroHit * MapEnvironmentRules.ParcelBurstHits;
                        foreach (UnitState u in RoomUnitsSnapshot())
                            if (u.Alive && u.Faction == Faction.Enemy && (u.Position - s.Parcel).sqrMagnitude < MapEnvironmentRules.ParcelBurstRadius * MapEnvironmentRules.ParcelBurstRadius)
                                ApplyDamage(u, splinters, Faction.Champion, "Parcel", u.Position - s.Parcel, cause: burst);
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

        // ------------------------------------------------------------------ paper planes
        private void PostStepPlanes(float deltaTime)
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            if (t >= s.PostNextPlanes)
            {
                s.PostNextPlanes = t + MapEnvironmentRules.PostPlaneEvery;
                PostChooseLane(s);
                s.PostPlanesWarnAt = t;
                s.PostPlaneCount = MapEnvironmentRules.PostPlanesMin + mapRandom.Next(MapEnvironmentRules.PostPlanesMax - MapEnvironmentRules.PostPlanesMin + 1);
                s.PostPassHits.Clear();
                while (s.PostPlaneVictims.Count < s.PostPlaneCount) s.PostPlaneVictims.Add(new HashSet<int>());
                for (int i = 0; i < s.PostPlaneVictims.Count; i++) s.PostPlaneVictims[i].Clear();
                postPlaneCause = NewCause(Champion.Id).Child("map-paper-plane");
            }
            if (t < s.PostPlanesAt || t > s.PostPlanesEnd + deltaTime) return;
            Vector2 dir = s.PostLaneDirection, normal = new Vector2(-dir.y, dir.x);
            float length = s.PostLaneLength, hit = RoomHeroHit * MapEnvironmentRules.PostPlaneHit;
            foreach (UnitState u in RoomUnitsSnapshot())
            {
                if (!u.Alive || u.Faction != Faction.Enemy || u.EntranceUntil > t) continue;
                Vector2 rel = u.Position - s.PostLaneFrom;
                float along = Vector2.Dot(rel, dir), side = Vector2.Dot(rel, normal);
                if (along < -.5f || along > length + .5f || Mathf.Abs(side) > MapEnvironmentRules.PostPlaneHalf + u.Definition.Radius * .5f) continue;
                s.PostPassHits.TryGetValue(u.Id, out int struck);
                for (int i = 0; i < s.PostPlaneCount && i < s.PostPlaneVictims.Count && struck < MapEnvironmentRules.PostPlaneHitsPerEnemy && u.Alive; i++)
                {
                    float now = s.PostPlaneAlong(i, t), before = now - MapEnvironmentRules.PostPlaneSpeed * deltaTime;
                    if (now < 0 || before > length || along < before - .3f || along > now + .3f || !s.PostPlaneVictims[i].Add(u.Id)) continue;
                    struck++;
                    ApplyDamage(u, hit, Faction.Champion, "Paper plane", dir, cause: postPlaneCause);
                    // the plane bursts where it strikes: everything else close by takes a share
                    Vector2 burst = u.Position;
                    foreach (UnitState o in RoomUnitsSnapshot())
                        if (o != u && o.Alive && o.Faction == Faction.Enemy && (o.Position - burst).sqrMagnitude <= MapEnvironmentRules.PostPlaneBurst * MapEnvironmentRules.PostPlaneBurst)
                            ApplyDamage(o, hit * MapEnvironmentRules.PostPlaneBurstShare, Faction.Champion, "Paper plane", (o.Position - burst).normalized, cause: postPlaneCause);
                    s.PostPlaneHits.Add(new MapEnvironmentState.PostPlaneStrike { Position = burst, At = t });
                    if (s.PostPlaneHits.Count > 24) s.PostPlaneHits.RemoveAt(0);
                }
                s.PostPassHits[u.Id] = struck;
            }
        }
        /// <summary>A lane across the room through the thickest knot of enemies near the hero, mostly side to side.</summary>
        private void PostChooseLane(MapEnvironmentState s)
        {
            Vector2 centre = Champion.Position; int best = -1, looked = 0;
            foreach (UnitState u in StableUnits)
            {
                if (!u.Alive || u.Faction != Faction.Enemy || (u.Position - Champion.Position).sqrMagnitude > 64) continue;
                if (++looked > 48) break;
                int near = 0;
                foreach (UnitState o in StableUnits)
                    if (o.Alive && o.Faction == Faction.Enemy && (o.Position - u.Position).sqrMagnitude < 4) near++;
                if (near > best) { best = near; centre = u.Position; }
            }
            // the planes ride the paper wind: the lane runs the way it last blew (a little play either side)
            Vector2 wind = s.PostWind.sqrMagnitude > 1e-4f ? s.PostWind.normalized : Vector2.right;
            float angle = Mathf.Atan2(wind.y, wind.x) + MapNext(-8, 8) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            s.PostLaneFrom = centre - dir * MapEnvironmentRules.PostLaneHalfLength; s.PostLaneTo = centre + dir * MapEnvironmentRules.PostLaneHalfLength;
        }

        /// <summary>For captures: the chute and its letters, the wind blowing, a parcel half broken, stamped enemies and a pass of
        /// planes in flight, on one frame.</summary>
        private void PostOfficeCaptureFixture()
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed; Vector2 h = Champion.Position;
            s.ChuteAt = t - 1.6f; s.ChuteX = h.x + 2.2f; s.ChuteTop = h.y + 4.4f; s.ChuteBottom = h.y - 5; s.NextChute = t + 30;
            s.Letters.Clear();
            for (int i = 0; i < 4; i++) s.Letters.Add(new MapEnvironmentState.Letter { Position = new Vector2(s.ChuteX + (i % 2) * .3f - .15f, s.ChuteTop - .4f - i * 1.6f), Born = t - i, Gold = i == 2, Style = i % 3, Stat = i });
            s.Letters.Add(new MapEnvironmentState.Letter { Position = h + new Vector2(-1.4f, -1.6f), Born = t - 1, Until = t + 7, Grounded = true, Stat = 4, Style = 1 });
            s.LettersToDrop = 0;
            s.GustWarnAt = t - MapEnvironmentRules.GustWarning - .5f; s.PostWind = Vector2.right; s.NextGust = t + 30;
            s.PostParcelNight = true; s.Parcel = ClampPosition(h + new Vector2(-3.2f, .6f), 1.5f); s.ParcelState = 2; s.ParcelAt = t - 2; s.HatchAt = t + 6;
            s.ParcelProp.Reset(s.Parcel + Vector2.up * .55f, MapEnvironmentRules.PostParcelRadius, MapEnvironmentRules.PostParcelHealth * RoomOrdinaryHealth());
            s.ParcelProp.Health = s.ParcelProp.MaxHealth * .45f; s.ParcelProp.HitAt = t - .05f; s.ParcelProp.HitFrom = h;
            int stamped = 0;
            foreach (UnitState u in StableUnits)
                if (u.Alive && u.Faction == Faction.Enemy && !u.Definition.IsBoss && (u.Position - h).sqrMagnitude < 36 && stamped++ < 2)
                { s.Stamped[u.Id] = t + 6; s.PostStampedAt[u.Id] = t - 2; s.PostStampHolders[u.Id] = u; }
            s.PostNextStamp = t + 30;
            s.PostLaneFrom = h + new Vector2(-13, -1.2f); s.PostLaneTo = h + new Vector2(13, -1.2f);
            s.PostPlanesWarnAt = t - MapEnvironmentRules.PostPlaneWarn - .9f; s.PostPlaneCount = 10; s.PostNextPlanes = t + 30;
            while (s.PostPlaneVictims.Count < s.PostPlaneCount) s.PostPlaneVictims.Add(new HashSet<int>());
            s.Shown.Add(MapIdentity.Element.Chute); s.Shown.Add(MapIdentity.Element.Gust); s.Shown.Add(MapIdentity.Element.Parcel);
        }
    }
}
