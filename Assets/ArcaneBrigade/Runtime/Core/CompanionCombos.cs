using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>What a combo has left on the field, for the ink to draw: a mark on an enemy, a patch of floor, a thing in motion.</summary>
    public enum ComboMarkKind { Bubble, Film, Thread, Dream, Lit, Chill, Crumb, Water, Star, Boba, Steeped, Clocked }
    public sealed class ComboMark { public UnitState Target; public ComboMarkKind Kind; public float Until; public int Stacks; }
    public enum ComboPatchKind { Steam, Dust, Lane, Thorns, TeaCone }
    public sealed class ComboPatch { public ComboPatchKind Kind; public Vector2 A, B; public float Radius, Born, Until, NextTick; }
    public enum ComboActorKind { Box, Star, Letter, Carousel, Ship, Book, Cloud, Oven, Loaf, Parade, Clock, Avalanche }
    public sealed class ComboActor
    {
        public int Id; public ComboActorKind Kind; public ComboId Combo; public Vector2 From, To; public float Born, Duration;
        public bool Landed; public int Shots; public readonly HashSet<int> Hit = new HashSet<int>();
        public Vector2 At(float now) => Vector2.Lerp(From, To, Mathf.Clamp01((now - Born) / Mathf.Max(.01f, Duration)));
    }

    public sealed partial class CombatSimulation
    {
        // ---- friend combos (requirement 25/10/2026; Data/Combos.cs) --------------------------------------------------------
        private bool combosReady;
        private readonly List<ComboId> liveCombos = new List<ComboId>();
        private readonly Dictionary<string, UnitState> comboMembers = new Dictionary<string, UnitState>();
        private readonly List<ComboMark> comboMarks = new List<ComboMark>();
        private readonly List<ComboPatch> comboPatches = new List<ComboPatch>();
        private readonly List<ComboActor> comboActors = new List<ComboActor>();
        private readonly Dictionary<long, float> comboCooldowns = new Dictionary<long, float>();
        private readonly Dictionary<ComboId, float> comboNext = new Dictionary<ComboId, float>();
        private readonly Dictionary<long, float> comboTouch = new Dictionary<long, float>();
        private float comboDelta;
        private int comboPiperNotes, comboPages, comboActorIds;
        private float comboBeatAt = -10;
        public IReadOnlyList<ComboMark> ComboMarks => comboMarks;
        public IReadOnlyList<ComboPatch> ComboPatches => comboPatches;
        public IReadOnlyList<ComboActor> ComboActors => comboActors;
        public IReadOnlyList<ComboId> LiveCombos => liveCombos;
        public int ComboPages => comboPages;

        private bool ComboLive(ComboId id) => liveCombos.Contains(id);
        private UnitState Member(string id) => comboMembers.TryGetValue(id, out UnitState u) ? u : null;
        private bool Is(UnitState source, string id) => source != null && comboMembers.TryGetValue(id, out UnitState u) && u == source;
        private float MemberDamage(string id) { UnitState u = Member(id); return u != null ? Mathf.Max(1, u.Definition.Weapon.Damage) : 1; }
        private float ComboStrength(ComboId id) { float sum = 0; string[] m = Combos.Of(id).Members; foreach (string s in m) sum += MemberDamage(s); return sum / m.Length; }
        private CombatCause ComboCause(ComboId id, string member) { UnitState u = Member(member); return NewCause(u != null ? u.Id : 0).Child("combo-" + id); }
        private bool Ready(ComboId id, int key, float seconds)
        {
            long k = (long)id << 32 | (uint)key;
            if (comboCooldowns.TryGetValue(k, out float until) && until > Elapsed) return false;
            comboCooldowns[k] = Elapsed + seconds; return true;
        }
        private void ComboMoment(ComboId id, Vector2 at, Vector2 other, float value = 0) => events.Add(new CombatEvent(CombatEventKind.ComboMoment, at, Faction.Companion, (int)id + value, other));
        private void ComboNamed(ComboId id, Vector2 at) => events.Add(new CombatEvent(CombatEventKind.ComboNamed, at, Faction.Companion, (int)id));

        /// <summary>The combos this battle can play: every friend of the group fielded.</summary>
        private void PrepareCombos()
        {
            combosReady = true; liveCombos.Clear(); comboMembers.Clear();
            foreach (UnitState u in StableUnits) if (u.Faction == Faction.Companion && !comboMembers.ContainsKey(u.Definition.Id)) comboMembers[u.Definition.Id] = u;
            foreach (ComboDefinition c in Combos.All)
            {
                bool all = true; foreach (string m in c.Members) if (!comboMembers.ContainsKey(m)) { all = false; break; }
                if (!all) continue;
                liveCombos.Add(c.Id);
                comboNext[c.Id] = Elapsed + 3 + c.Members.Length; // the bigger moves wait a little longer for their first turn
            }
        }

        private ComboMark MarkOf(UnitState target, ComboMarkKind kind) { foreach (ComboMark m in comboMarks) if (m.Target == target && m.Kind == kind && m.Until > Elapsed) return m; return null; }
        private ComboMark Mark(UnitState target, ComboMarkKind kind, float seconds)
        {
            ComboMark m = MarkOf(target, kind);
            if (m == null) { if (comboMarks.Count >= 160) return null; m = new ComboMark { Target = target, Kind = kind }; comboMarks.Add(m); }
            m.Until = Elapsed + seconds; return m;
        }
        private void Unmark(ComboMark m) { if (m != null) m.Until = 0; }

        private IEnumerable<UnitState> EnemiesNear(Vector2 at, float radius, UnitState except = null)
        {
            float r2 = radius * radius;
            for (int i = 0; i < units.Count; i++) { UnitState u = units[i]; if (u.Alive && u.Faction == Faction.Enemy && u != except && (u.Position - at).sqrMagnitude <= r2) yield return u; }
        }
        private UnitState NearestEnemy(Vector2 at, float radius, UnitState except = null, HashSet<UnitState> skip = null)
        {
            UnitState best = null; float bestD = radius * radius;
            for (int i = 0; i < units.Count; i++)
            {
                UnitState u = units[i]; if (!u.Alive || u.Faction != Faction.Enemy || u == except || skip != null && skip.Contains(u)) continue;
                float d = (u.Position - at).sqrMagnitude; if (d <= bestD) { bestD = d; best = u; }
            }
            return best;
        }
        /// <summary>Where the crowd in view is thickest (an enemy with the most others within 2), else just ahead of the hero.</summary>
        private Vector2 CrowdSpot()
        {
            Vector2 best = Champion.Position + Vector2.up * 2; int most = -1, looked = 0;
            for (int i = 0; i < units.Count && looked < 70; i++)
            {
                UnitState u = units[i]; if (!u.Alive || u.Faction != Faction.Enemy || Mathf.Abs(u.Position.x - Champion.Position.x) > 10 || Mathf.Abs(u.Position.y - Champion.Position.y) > 6) continue;
                looked++; int n = 0; foreach (UnitState _ in EnemiesNear(u.Position, 2)) n++;
                if (n > most) { most = n; best = u.Position; }
            }
            return best;
        }
        private void ComboHit(ComboId id, string member, UnitState target, float damage, Vector2 from)
        {
            if (target == null || !target.Alive) return;
            Vector2 dir = target.Position - from; dir = dir.sqrMagnitude > .0001f ? dir.normalized : Vector2.up;
            ApplyDamage(target, damage, Faction.Companion, Combos.Of(id).Name, dir, cause: ComboCause(id, member));
        }
        private void Shove(UnitState u, Vector2 away, float distance)
        {
            if (u.Definition.IsBoss) distance *= .15f;
            u.Position = ClampPosition(u.Position + away * distance, u.Definition.Radius); spatial.Move(u);
            events.Add(new CombatEvent(CombatEventKind.Knockback, u.Position, Faction.Companion, distance, away, subjectId: u.Id, subject: u));
        }
        private void Chain(ComboId id, string member, UnitState from, int jumps, float damage)
        {
            var done = new HashSet<UnitState> { from }; UnitState at = from;
            for (int i = 0; i < jumps; i++)
            {
                UnitState next = NearestEnemy(at.Position, 3.5f, null, done); if (next == null) break;
                ComboMoment(id, at.Position, next.Position); ComboHit(id, member, next, damage, at.Position);
                done.Add(next); at = next;
            }
        }

        /// <summary>A combo's share of an ordinary blow: the bonus a mark or a beat gives.</summary>
        private float ComboDamage(UnitState target, float damage, Faction source, CombatCause cause)
        {
            if (liveCombos.Count == 0 || source == Faction.Enemy || target.Faction != Faction.Enemy) return damage;
            UnitState from = UnitWithId(cause.SourceUnitId);
            if (ComboLive(ComboId.SlowMotion) && Is(from, "ReelOwl") && MarkOf(target, ComboMarkKind.Film) != null) damage *= 1.15f;
            if (ComboLive(ComboId.IntoTheLight) && Is(from, "LampGoat") && MarkOf(target, ComboMarkKind.Lit) != null) damage *= 1.3f;
            if (ComboLive(ComboId.ScaldingSteam) && Is(from, "SaffronMechanist")) foreach (ComboPatch p in comboPatches) if (p.Kind == ComboPatchKind.Steam && p.Until > Elapsed && (target.Position - p.A).sqrMagnitude <= p.Radius * p.Radius) { damage *= 1.35f; break; }
            if (ComboLive(ComboId.BeaconLane) && Is(from, "KiteDragon")) foreach (ComboPatch p in comboPatches) if (p.Kind == ComboPatchKind.Lane && p.Until > Elapsed && InLane(target.Position, p.A, p.B, p.Radius)) { damage *= 1.4f; break; }
            if (ComboLive(ComboId.TeaParty) && MarkOf(target, ComboMarkKind.Steeped) != null) damage *= 1.2f;
            if (ComboLive(ComboId.MidnightClocktower) && MarkOf(target, ComboMarkKind.Clocked) != null) damage *= 1.3f;
            if (ComboLive(ComboId.ClockworkBand) && Elapsed - comboBeatAt < .4f && (Is(from, "ClockworkPiper") || Is(from, "RainbowBell") || Is(from, "TinTick"))) damage *= 1.25f;
            return damage;
        }
        private static bool InLane(Vector2 p, Vector2 a, Vector2 b, float half)
        {
            Vector2 ab = b - a; float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(.0001f, ab.sqrMagnitude));
            return (p - (a + ab * t)).sqrMagnitude <= half * half;
        }

        /// <summary>A friend's blow has landed: the pair and group moves it starts or finishes.</summary>
        private void ComboAfterHit(UnitState target, CombatCause cause)
        {
            if (liveCombos.Count == 0 || target == null || target.Faction != Faction.Enemy) return;
            if (cause.ItemId != null && cause.ItemId.StartsWith("combo-", System.StringComparison.Ordinal)) return; // a combo never sets off another
            UnitState from = UnitWithId(cause.SourceUnitId);
            if (from == null || from.Faction != Faction.Companion) return;
            string who = from.Definition.Id;
            bool alive = target.Alive;
            Vector2 at = target.Position;
            ComboMark m;
            if (ComboLive(ComboId.ChargedBubbles))
            {
                if (who == "BubbleKoi" && alive) Mark(target, ComboMarkKind.Bubble, 3);
                if (who == "VoltEel" && (m = MarkOf(target, ComboMarkKind.Bubble)) != null) { Unmark(m); ComboMoment(ComboId.ChargedBubbles, at, at, .5f); Chain(ComboId.ChargedBubbles, "VoltEel", target, 3, MemberDamage("VoltEel") * .6f); }
            }
            if (ComboLive(ComboId.SlowMotion))
            {
                if (who == "LensOwl" && alive) Mark(target, ComboMarkKind.Film, 3);
                if (who == "ReelOwl" && alive && MarkOf(target, ComboMarkKind.Film) != null) { target.KitSlowUntil = Mathf.Max(target.KitSlowUntil, Elapsed + 2); target.KitSlow = Mathf.Max(target.KitSlow, .3f); }
            }
            if (ComboLive(ComboId.StitchedLetter))
            {
                if (who == "ThimbleMoth" && alive) Mark(target, ComboMarkKind.Thread, 2);
                if (who == "MailboxImp" && alive && (m = MarkOf(target, ComboMarkKind.Thread)) != null && !target.Definition.IsBoss) { Unmark(m); target.PinnedUntil = Mathf.Max(target.PinnedUntil, Elapsed + .6f); ComboMoment(ComboId.StitchedLetter, at, at); }
            }
            if (ComboLive(ComboId.CometStrike) && who == "CometKite" && Ready(ComboId.CometStrike, 0, 1.5f))
            {
                for (int i = 1; i <= 3; i++)
                {
                    Vector2 p = Vector2.Lerp(from.Position, at, i / 3f);
                    ComboMoment(ComboId.CometStrike, p + Vector2.up * 3, p);
                    foreach (UnitState e in new List<UnitState>(EnemiesNear(p, .8f))) ComboHit(ComboId.CometStrike, "StormKitten", e, MemberDamage("StormKitten") * .7f, p);
                }
            }
            if (ComboLive(ComboId.Lullaby) && alive && (who == "MoonJelly" || who == "PillowOracle"))
            {
                // struck by one, then by the other within 2 s: it falls asleep (6 s before it can again)
                long mine = (long)target.Id << 1 | (who == "MoonJelly" ? 0u : 1u), theirs = mine ^ 1;
                comboTouch[mine] = Elapsed;
                if (comboTouch.TryGetValue(theirs, out float when) && Elapsed - when <= 2 && !target.Definition.IsBoss && Ready(ComboId.Lullaby, target.Id, 6))
                { Sleep(target, 1.2f); Mark(target, ComboMarkKind.Dream, 1.2f); ComboMoment(ComboId.Lullaby, at, at); }
            }
            if (ComboLive(ComboId.StaticNotes) && who == "ClockworkPiper" && ++comboPiperNotes % 4 == 0) Chain(ComboId.StaticNotes, "ClockworkPiper", target, 2, MemberDamage("ClockworkPiper") * .6f);
            if (ComboLive(ComboId.IntoTheLight) && who == "SpringDuck" && alive) Mark(target, ComboMarkKind.Lit, 2);
            if (ComboLive(ComboId.BrainFreeze))
            {
                if (who == "SnowSpoon" && alive) { Mark(target, ComboMarkKind.Chill, 2); target.KitSlowUntil = Mathf.Max(target.KitSlowUntil, Elapsed + 2); target.KitSlow = Mathf.Max(target.KitSlow, .2f); }
                if (who == "IceCreamOwl" && (m = MarkOf(target, ComboMarkKind.Chill)) != null)
                {
                    Unmark(m); int shards = 0;
                    foreach (UnitState e in new List<UnitState>(EnemiesNear(at, 2.5f, target))) { if (shards++ >= 3) break; ComboMoment(ComboId.BrainFreeze, at, e.Position); ComboHit(ComboId.BrainFreeze, "IceCreamOwl", e, MemberDamage("IceCreamOwl") * .5f, at); }
                }
            }
            if (ComboLive(ComboId.ScaldingSteam) && who == "KettleBat" && Ready(ComboId.ScaldingSteam, 0, .7f) && comboPatches.Count < 40)
                comboPatches.Add(new ComboPatch { Kind = ComboPatchKind.Steam, A = at, Radius = 1, Born = Elapsed, Until = Elapsed + 2.5f });
            if (ComboLive(ComboId.ThornDust))
            {
                if (who == "PearMoth" && Ready(ComboId.ThornDust, 0, .8f) && comboPatches.Count < 40) comboPatches.Add(new ComboPatch { Kind = ComboPatchKind.Dust, A = at, Radius = .9f, Born = Elapsed, Until = Elapsed + 3 });
                if (who == "SeedHedgehog")
                    foreach (ComboPatch p in comboPatches)
                    {
                        if (p.Kind != ComboPatchKind.Dust || p.Until <= Elapsed || (at - p.A).sqrMagnitude > p.Radius * p.Radius) continue;
                        p.Until = 0; ComboMoment(ComboId.ThornDust, p.A, p.A);
                        foreach (UnitState e in new List<UnitState>(EnemiesNear(p.A, p.Radius + .2f))) ComboHit(ComboId.ThornDust, "SeedHedgehog", e, MemberDamage("SeedHedgehog") * .5f, p.A);
                        break;
                    }
            }
            if (ComboLive(ComboId.ScribbleBox) && who == "CrayonGremlin" && Ready(ComboId.ScribbleBox, 0, 4))
                comboActors.Add(new ComboActor { Id = ++comboActorIds, Kind = ComboActorKind.Box, Combo = ComboId.ScribbleBox, From = at, To = at, Born = Elapsed, Duration = .6f });
            if (ComboLive(ComboId.Airmail) && who == "ShellPostman" && Ready(ComboId.Airmail, 0, .8f))
            {
                ComboMoment(ComboId.Airmail, at, at);
                foreach (UnitState e in new List<UnitState>(EnemiesNear(at, 1.2f, target))) ComboHit(ComboId.Airmail, "ShellPostman", e, MemberDamage("ShellPostman") * .4f, at);
            }
            if (ComboLive(ComboId.Crumbs))
            {
                if (who == "BakerMouse" && alive) Mark(target, ComboMarkKind.Crumb, 3);
                if (who == "BreadCrab" && (m = MarkOf(target, ComboMarkKind.Crumb)) != null)
                {
                    Unmark(m); ComboMoment(ComboId.Crumbs, at, at);
                    foreach (UnitState e in EnemiesNear(at, 1.5f)) { e.KitSlowUntil = Mathf.Max(e.KitSlowUntil, Elapsed + 2); e.KitSlow = Mathf.Max(e.KitSlow, .3f); }
                }
            }
            if (ComboLive(ComboId.SproutingVines))
            {
                if (who == "WateringSnail" && alive) Mark(target, ComboMarkKind.Water, 3);
                if (who == "FirstSeed" && alive && (m = MarkOf(target, ComboMarkKind.Water)) != null && !target.Definition.IsBoss) { Unmark(m); target.PinnedUntil = Mathf.Max(target.PinnedUntil, Elapsed + 1); ComboMoment(ComboId.SproutingVines, at, at); }
            }
            if (ComboLive(ComboId.MirrorSpice) && who == "MaskPepper" && Ready(ComboId.MirrorSpice, 0, .6f))
            {
                UnitState echo = NearestEnemy(at, 3, target);
                if (echo != null) { ComboMoment(ComboId.MirrorSpice, at, echo.Position); ComboHit(ComboId.MirrorSpice, "MaskPepper", echo, MemberDamage("MaskPepper") * .5f, at); }
            }
            if (ComboLive(ComboId.Constellation) && alive && (who == "MoonJelly" || who == "StarSock" || who == "OrbitMoth") && Ready(ComboId.Constellation, from.Id, .3f) && MarkOf(target, ComboMarkKind.Star) == null)
            {
                Mark(target, ComboMarkKind.Star, 6);
                var stars = new List<ComboMark>(); foreach (ComboMark s in comboMarks) if (s.Kind == ComboMarkKind.Star && s.Until > Elapsed && s.Target.Alive) stars.Add(s);
                if (stars.Count >= 3)
                {
                    Vector2 c = Vector2.zero; for (int i = 0; i < 3; i++) { c += stars[i].Target.Position; Unmark(stars[i]); }
                    c /= 3; ComboNamed(ComboId.Constellation, c);
                    comboActors.Add(new ComboActor { Id = ++comboActorIds, Kind = ComboActorKind.Star, Combo = ComboId.Constellation, From = c + new Vector2(-2, 6), To = c, Born = Elapsed, Duration = .5f });
                }
            }
            if (ComboLive(ComboId.FrostSundae) && alive && (who == "SnowSpoon" || who == "IceCreamOwl" || who == "BobaTadpole"))
            {
                m = MarkOf(target, ComboMarkKind.Boba) ?? Mark(target, ComboMarkKind.Boba, 4);
                if (m != null && ++m.Stacks >= 3) { Unmark(m); if (!target.Definition.IsBoss) Freeze(target, 1.5f); ComboMoment(ComboId.FrostSundae, at, at); }
                else if (m != null) m.Until = Elapsed + 4;
            }
        }

        /// <summary>A defeat turns a page of the Midnight Library.</summary>
        private void ComboKill()
        {
            if (!ComboLive(ComboId.MidnightLibrary) || ++comboPages < 20) return;
            comboPages = 0; Vector2 spot = CrowdSpot(); ComboNamed(ComboId.MidnightLibrary, spot);
            comboActors.Add(new ComboActor { Id = ++comboActorIds, Kind = ComboActorKind.Book, Combo = ComboId.MidnightLibrary, From = spot + Vector2.up * 5, To = spot, Born = Elapsed, Duration = .6f });
        }

        private bool Due(ComboId id, float every)
        {
            if (!ComboLive(id) || !comboNext.TryGetValue(id, out float at) || Elapsed < at) return false;
            comboNext[id] = Elapsed + every; return true;
        }

        /// <summary>The timed group moves, the things in motion and the patches on the floor.</summary>
        private void ComboTick(float deltaTime)
        {
            if (!combosReady) PrepareCombos();
            comboDelta = deltaTime;
            comboMarks.RemoveAll(m => m.Until <= Elapsed || !m.Target.Alive);
            comboPatches.RemoveAll(p => p.Until <= Elapsed);
            if (liveCombos.Count == 0) return;
            Vector2 hero = Champion.Position;
            if (Due(ComboId.BeaconLane, 7))
            {
                UnitState whale = Member("LighthouseWhale"); Vector2 a = whale != null ? whale.Position : hero, d = (CrowdSpot() - a); d = d.sqrMagnitude > .01f ? d.normalized : Vector2.right;
                comboPatches.Add(new ComboPatch { Kind = ComboPatchKind.Lane, A = a, B = a + d * 7, Radius = .45f, Born = Elapsed, Until = Elapsed + 2 });
                comboActors.Add(new ComboActor { Id = ++comboActorIds, Kind = ComboActorKind.Box, Combo = ComboId.BeaconLane, From = a, To = a + d * 7, Born = Elapsed + .5f, Duration = .6f });
            }
            if (Due(ComboId.TeaParty, 10))
            {
                UnitState dragon = Member("TeaDragon"); Vector2 a = dragon != null ? dragon.Position : hero, d = CrowdSpot() - a; d = d.sqrMagnitude > .01f ? d.normalized : Vector2.right;
                ComboNamed(ComboId.TeaParty, a + d * 2);
                comboPatches.Add(new ComboPatch { Kind = ComboPatchKind.TeaCone, A = a, B = a + d * 4.5f, Radius = 30, Born = Elapsed, Until = Elapsed + 1.2f });
                foreach (UnitState e in new List<UnitState>(EnemiesNear(a, 4.5f)))
                    if (Vector2.Angle(d, e.Position - a) <= 30) { Mark(e, ComboMarkKind.Steeped, 3); ComboHit(ComboId.TeaParty, "TeaDragon", e, MemberDamage("TeaDragon"), a); }
            }
            if (Due(ComboId.ExpressDelivery, 8)) { ComboNamed(ComboId.ExpressDelivery, hero); comboActors.Add(new ComboActor { Id = ++comboActorIds, Kind = ComboActorKind.Letter, Combo = ComboId.ExpressDelivery, Born = Elapsed, Duration = 1.8f }); }
            if (Due(ComboId.ClockworkBand, 2)) { comboBeatAt = Elapsed; foreach (string s in Combos.Of(ComboId.ClockworkBand).Members) { UnitState u = Member(s); if (u != null) ComboMoment(ComboId.ClockworkBand, u.Position, u.Position); } }
            if (Due(ComboId.ThornGarden, 5)) { Vector2 s = CrowdSpot(); ComboNamed(ComboId.ThornGarden, s); comboPatches.Add(new ComboPatch { Kind = ComboPatchKind.Thorns, A = s, Radius = 1.2f, Born = Elapsed, Until = Elapsed + 6, NextTick = Elapsed + .3f }); }
            if (Due(ComboId.Carousel, 12)) { Vector2 s = CrowdSpot(); ComboNamed(ComboId.Carousel, s); comboActors.Add(new ComboActor { Id = ++comboActorIds, Kind = ComboActorKind.Carousel, Combo = ComboId.Carousel, From = s, To = s, Born = Elapsed, Duration = 1.2f }); }
            if (Due(ComboId.GhostFleet, 15)) { Vector2 s = CrowdSpot(); ComboNamed(ComboId.GhostFleet, s); comboActors.Add(new ComboActor { Id = ++comboActorIds, Kind = ComboActorKind.Ship, Combo = ComboId.GhostFleet, From = new Vector2(hero.x - 12, s.y), To = new Vector2(hero.x + 12, s.y), Born = Elapsed, Duration = 2.5f }); }
            if (Due(ComboId.StormFront, 8)) { ComboNamed(ComboId.StormFront, hero + Vector2.up * 3); comboActors.Add(new ComboActor { Id = ++comboActorIds, Kind = ComboActorKind.Cloud, Combo = ComboId.StormFront, From = hero + Vector2.up * 3, To = hero + Vector2.up * 3, Born = Elapsed, Duration = 1.6f }); }
            if (Due(ComboId.BakeryRush, 12))
            {
                Vector2 s = CrowdSpot(); float side = s.x < hero.x ? 1 : -1; // the oven stands on the far side and fires across the crowd
                ComboNamed(ComboId.BakeryRush, s);
                comboActors.Add(new ComboActor { Id = ++comboActorIds, Kind = ComboActorKind.Oven, Combo = ComboId.BakeryRush, From = new Vector2(hero.x + side * 9.5f, s.y), To = new Vector2(hero.x - side * 12, s.y), Born = Elapsed, Duration = 2.2f });
            }
            if (Due(ComboId.GrandParade, 20)) { Vector2 s = CrowdSpot(); ComboNamed(ComboId.GrandParade, s); comboActors.Add(new ComboActor { Id = ++comboActorIds, Kind = ComboActorKind.Parade, Combo = ComboId.GrandParade, From = new Vector2(hero.x - 12, s.y), To = new Vector2(hero.x + 12, s.y), Born = Elapsed, Duration = 3 }); }
            if (Due(ComboId.MidnightClocktower, 24)) { ComboNamed(ComboId.MidnightClocktower, hero + Vector2.up * 3.5f); comboActors.Add(new ComboActor { Id = ++comboActorIds, Kind = ComboActorKind.Clock, Combo = ComboId.MidnightClocktower, From = hero + Vector2.up * 3.5f, To = hero + Vector2.up * 3.5f, Born = Elapsed, Duration = 2.6f }); }
            if (Due(ComboId.WinterFeast, 18)) { Vector2 s = CrowdSpot(); ComboNamed(ComboId.WinterFeast, s); comboActors.Add(new ComboActor { Id = ++comboActorIds, Kind = ComboActorKind.Avalanche, Combo = ComboId.WinterFeast, From = new Vector2(s.x, hero.y + 8), To = new Vector2(s.x, s.y), Born = Elapsed, Duration = 1 }); }

            foreach (ComboPatch p in comboPatches)
                if (p.Kind == ComboPatchKind.Thorns && Elapsed >= p.NextTick)
                {
                    p.NextTick = Elapsed + 1;
                    foreach (UnitState e in new List<UnitState>(EnemiesNear(p.A, p.Radius))) ComboHit(ComboId.ThornGarden, "SeedHedgehog", e, ComboStrength(ComboId.ThornGarden) * .4f, p.A);
                }
            for (int i = comboActors.Count - 1; i >= 0; i--)
            {
                ComboActor a = comboActors[i];
                if (Elapsed < a.Born) continue;
                if (PlayActor(a)) comboActors.RemoveAt(i);
            }
        }

        /// <summary>One thing in motion this step; true when it is done.</summary>
        private bool PlayActor(ComboActor a)
        {
            float t = (Elapsed - a.Born) / Mathf.Max(.01f, a.Duration), power = ComboStrength(a.Combo);
            Vector2 now = a.At(Elapsed);
            switch (a.Kind)
            {
                case ComboActorKind.Box when a.Combo == ComboId.ScribbleBox:
                    if (t < 1) return false;
                    ComboMoment(ComboId.ScribbleBox, a.To, a.To);
                    foreach (UnitState e in new List<UnitState>(EnemiesNear(a.To, 1.6f))) { Vector2 away = e.Position - a.To; Shove(e, away.sqrMagnitude > .001f ? away.normalized : Vector2.up, 1.6f); ComboHit(ComboId.ScribbleBox, "JackInTheBox", e, MemberDamage("JackInTheBox") * .8f, a.To); }
                    return true;
                case ComboActorKind.Box: // the Dragon's dive down the lit lane
                    foreach (UnitState e in new List<UnitState>(EnemiesNear((a.From + a.To) * .5f, Vector2.Distance(a.From, a.To) * .5f + .5f)))
                        if (InLane(e.Position, a.From, now, .55f) && a.Hit.Add(e.Id)) ComboHit(ComboId.BeaconLane, "KiteDragon", e, MemberDamage("KiteDragon") * 1.2f, a.From);
                    if (!a.Landed) { a.Landed = true; ComboMoment(ComboId.BeaconLane, a.From, a.To); }
                    return t >= 1;
                case ComboActorKind.Star:
                case ComboActorKind.Book:
                    if (t < 1) return false;
                    float reach = a.Kind == ComboActorKind.Star ? 2 : 2.4f, hit = a.Kind == ComboActorKind.Star ? 2.5f : 4;
                    ComboMoment(a.Combo, a.To, a.To, .5f);
                    foreach (UnitState e in new List<UnitState>(EnemiesNear(a.To, reach))) { if (a.Kind == ComboActorKind.Book && !e.Definition.IsBoss) e.StaggerUntil = Mathf.Max(e.StaggerUntil, Elapsed + .8f); ComboHit(a.Combo, Combos.Of(a.Combo).Members[0], e, power * hit, a.To); }
                    return true;
                case ComboActorKind.Letter:
                {
                    // the letter flies round its three friends and back, cutting what it passes
                    string[] m = Combos.Of(a.Combo).Members; var path = new List<Vector2>();
                    foreach (string s in m) { UnitState u = Member(s); path.Add(u != null ? u.Position : Champion.Position); }
                    path.Add(path[0]);
                    float f = Mathf.Clamp01(t) * (path.Count - 1); int seg = Mathf.Min(path.Count - 2, (int)f);
                    a.From = Vector2.Lerp(path[seg], path[seg + 1], f - seg); a.To = path[seg + 1];
                    foreach (UnitState e in new List<UnitState>(EnemiesNear(a.From, .8f))) if (a.Hit.Add(e.Id)) ComboHit(a.Combo, m[0], e, power * 1.2f, a.From);
                    return t >= 1;
                }
                case ComboActorKind.Carousel:
                    foreach (UnitState e in EnemiesNear(a.From, 2.5f))
                        if (!e.Definition.IsBoss && t < .67f) { e.Position = ClampPosition(Vector2.MoveTowards(e.Position, a.From, 2.5f * comboDelta), e.Definition.Radius); spatial.Move(e); }
                    if (t < 1) return false;
                    ComboMoment(ComboId.Carousel, a.From, a.From);
                    foreach (UnitState e in new List<UnitState>(EnemiesNear(a.From, 2.6f))) { Vector2 away = e.Position - a.From; Shove(e, away.sqrMagnitude > .001f ? away.normalized : Random2(e.Id), 2); ComboHit(ComboId.Carousel, "JackInTheBox", e, power * 1.5f, a.From); }
                    return true;
                case ComboActorKind.Ship:
                case ComboActorKind.Parade:
                {
                    float band = a.Kind == ComboActorKind.Ship ? 1.2f : 1.4f;
                    foreach (UnitState e in new List<UnitState>(EnemiesNear(now, band + .4f)))
                        if (Mathf.Abs(e.Position.y - now.y) <= band && Mathf.Abs(e.Position.x - now.x) <= 1 && a.Hit.Add(e.Id))
                        {
                            Shove(e, Vector2.right * Mathf.Sign(a.To.x - a.From.x), 1);
                            if (a.Kind == ComboActorKind.Parade && !e.Definition.IsBoss) e.StaggerUntil = Mathf.Max(e.StaggerUntil, Elapsed + 2);
                            ComboHit(a.Combo, Combos.Of(a.Combo).Members[0], e, power * (a.Kind == ComboActorKind.Ship ? 2 : 1.5f), now);
                        }
                    return t >= 1;
                }
                case ComboActorKind.Cloud:
                {
                    int due = Mathf.Min(4, Mathf.FloorToInt(t * 1.6f / .4f) + (t > 0 ? 1 : 0));
                    while (a.Shots < due)
                    {
                        a.Shots++; UnitState e = NearestEnemy(Champion.Position, 6.5f);
                        if (e == null) break;
                        ComboMoment(ComboId.StormFront, now, e.Position); ComboHit(ComboId.StormFront, "StormKitten", e, power * 1.5f, now);
                    }
                    return t >= 1;
                }
                case ComboActorKind.Oven:
                {
                    // six loaves leave the oven .15 s apart and cross the row
                    int loaves = Mathf.Min(6, Mathf.FloorToInt((Elapsed - a.Born) / .15f) + 1);
                    while (a.Shots < loaves) { a.Shots++; comboActors.Add(new ComboActor { Id = ++comboActorIds, Kind = ComboActorKind.Loaf, Combo = ComboId.BakeryRush, From = a.From, To = a.To, Born = Elapsed, Duration = 1.2f }); }
                    return t >= 1;
                }
                case ComboActorKind.Loaf:
                    foreach (UnitState e in new List<UnitState>(EnemiesNear(now, .8f))) if (a.Hit.Add(e.Id)) ComboHit(ComboId.BakeryRush, "BakerMouse", e, power * 1.2f, now);
                    return t >= 1;
                case ComboActorKind.Clock:
                    if (a.Landed || t < .45f) return t >= 1;
                    a.Landed = true; ComboMoment(ComboId.MidnightClocktower, now, now, .5f);
                    for (int i = 0; i < units.Count; i++)
                    {
                        UnitState e = units[i]; if (!e.Alive || e.Faction != Faction.Enemy || Mathf.Abs(e.Position.x - Champion.Position.x) > 10.6f || Mathf.Abs(e.Position.y - Champion.Position.y) > 6.2f) continue;
                        if (e.Definition.IsBoss) { e.KitSlowUntil = Mathf.Max(e.KitSlowUntil, Elapsed + 2); e.KitSlow = Mathf.Max(e.KitSlow, .5f); }
                        else Freeze(e, 2);
                        Mark(e, ComboMarkKind.Clocked, 2);
                    }
                    return false;
                case ComboActorKind.Avalanche:
                    if (t < 1) return false;
                    ComboMoment(ComboId.WinterFeast, a.To, a.To, .5f);
                    for (int i = 0; i < units.Count; i++)
                    {
                        UnitState e = units[i]; if (!e.Alive || e.Faction != Faction.Enemy || Mathf.Abs(e.Position.x - a.To.x) > 1.2f || Mathf.Abs(e.Position.y - Champion.Position.y) > 7) continue;
                        if (!e.Definition.IsBoss) Freeze(e, 2);
                        ComboHit(ComboId.WinterFeast, "IceCreamOwl", e, power * 2, a.To);
                    }
                    return true;
            }
            return true;
        }
        private static Vector2 Random2(int seed) { float a = seed * 2.399963f; return new Vector2(Mathf.Cos(a), Mathf.Sin(a)); }
    }
}
