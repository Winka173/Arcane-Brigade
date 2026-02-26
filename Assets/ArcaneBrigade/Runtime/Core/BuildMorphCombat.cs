using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public enum MorphGeometry { Line, Disc, Ring, Crescent, Seal }
    public sealed class MorphStrike
    {
        public BuildMorphKind Kind;
        public MorphGeometry Geometry;
        public UnitState Source;
        public Vector2 Position, End, Direction;
        public float Radius, Width, At, Until, Created, Budget, Slow, Pull, Cone = -.2f;
        public int Stage, MaxTargets;
        public bool Resolved;
        public CombatCause Cause;
    }

    public sealed partial class CombatSimulation
    {
        private readonly List<MorphStrike> morphStrikes = new List<MorphStrike>();
        private readonly List<UnitState> morphVictims = new List<UnitState>();
        private readonly List<UnitState> morphPins = new List<UnitState>();
        public IReadOnlyList<MorphStrike> MorphStrikes => morphStrikes;
        public int BuildMorphReleases { get; private set; }
        private void ClearBuildMorphs()
        {
            morphStrikes.Clear(); morphVictims.Clear(); morphPins.Clear(); BuildMorphReleases = 0;
            foreach (UnitState unit in units) { unit.MorphAttacks = 0; unit.NextMorph = 0; unit.LastMorphAt = -100; }
        }

        private float ReleaseBuildMorph(UnitState source, float damage, CombatCause cause)
        {
            if (source.Faction == Faction.Enemy || !source.Alive || source.Definition.BuildMorphStage <= 0 || cause.Depth != 0) return damage;
            source.MorphAttacks = Mathf.Min(3, source.MorphAttacks + 1);
            if (source.MorphAttacks < 3 || source.NextMorph > Elapsed || morphStrikes.Count > 88) return damage;
            BuildMorphKind kind = source.Definition.BuildMorph;
            if (BuildMorphs.Get(kind) == null) return damage;
            UnitState target = units.Find(u => u.Id == source.AttackTargetId && CanHit(source.Faction, u));
            if (target == null) return damage;
            int stage = Mathf.Clamp(source.Definition.BuildMorphStage, 1, 3), start = morphStrikes.Count;
            Vector2 origin = source.Position, point = target.Position;
            Vector2 direction = source.AttackDirection.sqrMagnitude > .001f ? source.AttackDirection.normalized : Vector2.right;
            Vector2 side = new Vector2(-direction.y, direction.x);
            CombatCause root = new CombatCause(cause.RootId, source.Id, "build-" + kind.ToString().ToLowerInvariant(), 1);
            void Strike(MorphGeometry geometry, Vector2 at, Vector2 end, float radius, float delay, float width = .16f, float slow = 0, float pull = 0)
            {
                morphStrikes.Add(new MorphStrike { Kind = kind, Geometry = geometry, Source = source, Position = at, End = end, Direction = direction,
                    Radius = radius, Width = width, Created = Elapsed, At = Elapsed + delay, Until = Elapsed + delay + .34f, Stage = stage,
                    MaxTargets = 2 + stage, Slow = slow, Pull = pull, Cause = root });
            }
            switch (kind)
            {
                case BuildMorphKind.Thread:
                    morphPins.Clear(); morphPins.Add(target);
                    for (int pin = 0; pin < stage + 1; pin++)
                    {
                        UnitState next = null; float best = 2.7f * 2.7f;
                        foreach (UnitState enemy in units)
                        {
                            float d = (enemy.Position - morphPins[morphPins.Count - 1].Position).sqrMagnitude;
                            if (!CanHit(source.Faction, enemy) || morphPins.Contains(enemy) || d >= best) continue;
                            next = enemy; best = d;
                        }
                        if (next == null) break;
                        morphPins.Add(next);
                    }
                    Strike(MorphGeometry.Disc, point, point, .42f, .12f);
                    for (int i = 1; i < morphPins.Count; i++) Strike(MorphGeometry.Line, morphPins[i - 1].Position, morphPins[i].Position, 0, .18f + i * .08f, .15f, .18f);
                    if (stage >= 2 && morphPins.Count > 2) Strike(MorphGeometry.Line, morphPins[morphPins.Count - 1].Position, point, 0, .54f, .17f);
                    break;
                case BuildMorphKind.Cinder:
                    for (int i = 0; i < 2 + stage; i++) Strike(MorphGeometry.Disc, Vector2.Lerp(origin, point, (i + 1f) / (2 + stage)), point, .42f + stage * .07f, .08f + i * .1f);
                    if (stage == 3)
                        for (int sign = -1; sign <= 1; sign += 2) Strike(MorphGeometry.Line, point, point + direction * 1.5f + side * sign * 1.15f, 0, .65f, .24f);
                    break;
                case BuildMorphKind.Clockwork:
                    Strike(MorphGeometry.Disc, point - side * .45f, point, .78f, .18f);
                    Strike(MorphGeometry.Disc, point + side * .45f, point, .78f, .38f);
                    if (stage >= 2) Strike(MorphGeometry.Line, point - direction * 1.3f, point + direction * 1.3f, 0, .56f, .25f);
                    if (stage == 3) Strike(MorphGeometry.Line, point - side * 1.6f, point + side * 1.6f, 0, .65f, .25f);
                    break;
                case BuildMorphKind.Porcelain:
                    for (int i = 0; i < 3; i++) Strike(MorphGeometry.Disc, point, point, .75f + stage * .17f, .12f + i * .24f, slow: .35f);
                    if (stage >= 2)
                        for (int sign = -1; sign <= 1; sign += 2) Strike(MorphGeometry.Disc, point + side * sign * 1.2f, point, .58f, .46f, slow: .35f);
                    if (stage == 3) Strike(MorphGeometry.Ring, point, point, 1.9f, .72f, .26f, .35f);
                    break;
                case BuildMorphKind.Constellation:
                    for (int i = 0; i < stage + 2; i++)
                    {
                        Vector2 ray = Quaternion.Euler(0, 0, i * 360f / (stage + 2) + source.Id * 17 % 60) * Vector2.right;
                        Strike(MorphGeometry.Line, point + ray * (1.5f + stage * .2f), point, 0, .2f + i * .08f, .13f);
                    }
                    if (stage == 3) Strike(MorphGeometry.Ring, point, point, 1.3f, .7f, .2f);
                    break;
                case BuildMorphKind.Paper:
                    for (int i = 0; i < 1 + stage; i++)
                    {
                        Strike(MorphGeometry.Crescent, origin, origin, .9f + i * .72f, .12f + i * .12f, .3f, pull: -.2f);
                        morphStrikes[morphStrikes.Count - 1].Cone = stage == 3 ? -.35f : .15f;
                    }
                    break;
                case BuildMorphKind.Mirror:
                    for (int i = 0; i < 1 + stage; i++)
                    {
                        Vector2 from = origin + side * ((i % 2 == 0 ? 1 : -1) * (.6f + .15f * i));
                        Strike(MorphGeometry.Line, from, point + direction * .7f, 0, .18f + i * .16f, .15f);
                    }
                    break;
                case BuildMorphKind.Chorus:
                    // a sung phrase thrown at the target: notes land one after another along the way, rising and falling
                    for (int i = 0; i < stage + 2; i++) Strike(MorphGeometry.Disc, Vector2.Lerp(origin + direction * .7f, point, (i + 1f) / (stage + 2)) + side * ((i % 2 == 0 ? 1 : -1) * .22f), point, .5f + stage * .06f, .1f + i * .12f);
                    if (stage == 3) Strike(MorphGeometry.Ring, point, point, .8f, .78f, .35f);
                    break;
                case BuildMorphKind.Magnet:
                    Strike(MorphGeometry.Disc, point, point, 1 + stage * .2f, .12f, slow: .2f, pull: .3f);
                    Strike(MorphGeometry.Ring, point, point, .7f + stage * .2f, .48f, .45f);
                    if (stage >= 2) Strike(MorphGeometry.Line, point - side * 1.6f, point + side * 1.6f, 0, .62f, .22f);
                    if (stage == 3) Strike(MorphGeometry.Line, point - direction * 1.8f, point + direction * 1.8f, 0, .74f, .22f);
                    break;
                case BuildMorphKind.Tide:
                    Strike(MorphGeometry.Line, origin, point + direction, 0, .12f, .28f);
                    Strike(MorphGeometry.Line, point + direction + side * .45f, origin + side * .45f, 0, .48f, .28f);
                    if (stage >= 2) Strike(MorphGeometry.Crescent, origin, origin, Mathf.Clamp((point - origin).magnitude, 1, 4), .64f, .35f);
                    if (stage == 3) Strike(MorphGeometry.Line, point - side * 1.8f, point + side * 1.8f, 0, .78f, .33f);
                    break;
                case BuildMorphKind.Frost:
                    for (int i = 0; i < 2 + stage; i++)
                    {
                        Vector2 ray = Quaternion.Euler(0, 0, i * 360f / (2 + stage)) * Vector2.right;
                        Strike(MorphGeometry.Line, point, point + ray * (1.2f + stage * .25f), 0, .18f + i * .06f, .17f, .45f);
                    }
                    if (stage == 3) Strike(MorphGeometry.Ring, point, point, 2.1f, .68f, .3f, .45f);
                    break;
                case BuildMorphKind.Seal:
                    Strike(MorphGeometry.Seal, point, point, .8f + stage * .12f, .26f);
                    if (stage >= 2)
                    {
                        Strike(MorphGeometry.Line, point - side * 1.2f, point + side * 1.2f, 0, .48f, .18f);
                        Strike(MorphGeometry.Line, point - direction * 1.2f, point + direction * 1.2f, 0, .6f, .18f);
                    }
                    if (stage == 3) Strike(MorphGeometry.Seal, point, point, 1.7f, .8f);
                    break;
            }
            int count = morphStrikes.Count - start;
            if (count == 0) return damage;
            float budget = Mathf.Max(0, damage) * (.6f + stage * .2f) / count;
            for (int i = start; i < morphStrikes.Count; i++) morphStrikes[i].Budget = budget;
            source.MorphAttacks = 0; source.NextMorph = Elapsed + 2.4f; source.LastMorphAt = Elapsed;
            BuildMorphReleases++; RecordActivation(root);
            events.Add(new CombatEvent(CombatEventKind.BuildMorphCast, point, source.Faction, stage, direction, subject: source, cause: root));
            return damage * .72f;
        }

        public static bool MorphContains(MorphStrike strike, UnitState enemy)
        {
            float body = enemy.Definition.Radius;
            Vector2 offset = enemy.Position - strike.Position;
            if (strike.Geometry == MorphGeometry.Line)
            {
                Vector2 line = strike.End - strike.Position;
                Vector2 closest = strike.Position + line * Mathf.Clamp01(Vector2.Dot(offset, line) / Mathf.Max(.0001f, line.sqrMagnitude));
                return (enemy.Position - closest).sqrMagnitude <= Mathf.Pow(strike.Width + body, 2);
            }
            if (strike.Geometry == MorphGeometry.Seal)
            {
                Vector2 side = new Vector2(-strike.Direction.y, strike.Direction.x);
                return Mathf.Abs(Vector2.Dot(offset, strike.Direction)) <= strike.Radius + body && Mathf.Abs(Vector2.Dot(offset, side)) <= strike.Radius + body;
            }
            float distance = offset.magnitude;
            if (strike.Geometry == MorphGeometry.Disc) return distance <= strike.Radius + body;
            return Mathf.Abs(distance - strike.Radius) <= strike.Width + body && (strike.Geometry != MorphGeometry.Crescent || Vector2.Dot(offset.normalized, strike.Direction) >= strike.Cone);
        }

        private void ProcessBuildMorphs()
        {
            for (int i = morphStrikes.Count - 1; i >= 0; i--)
            {
                MorphStrike strike = morphStrikes[i];
                if (!strike.Source.Alive || Elapsed >= strike.Until) { morphStrikes.RemoveAt(i); continue; }
                if (strike.Resolved || Elapsed < strike.At) continue;
                strike.Resolved = true; morphVictims.Clear();
                foreach (UnitState enemy in units)
                {
                    if (!CanHit(strike.Source.Faction, enemy) || !MorphContains(strike, enemy)) continue;
                    // Nearest victims win deterministically; do not depend on spawn-list ordering.
                    int slot = morphVictims.Count;
                    float distance = (enemy.Position - strike.Position).sqrMagnitude;
                    while (slot > 0)
                    {
                        UnitState previous = morphVictims[slot - 1]; float prior = (previous.Position - strike.Position).sqrMagnitude;
                        if (prior < distance || prior == distance && previous.Id < enemy.Id) break;
                        slot--;
                    }
                    if (slot >= strike.MaxTargets) continue;
                    morphVictims.Insert(slot, enemy);
                    if (morphVictims.Count > strike.MaxTargets) morphVictims.RemoveAt(morphVictims.Count - 1);
                }
                float share = strike.Budget / Mathf.Max(1, morphVictims.Count);
                foreach (UnitState enemy in morphVictims)
                {
                    // Secondary damage carries no weapon and cannot feed weapon hit recipes or itself.
                    ApplyDamage(enemy, share, strike.Source.Faction, BuildMorphs.Get(strike.Kind).Name, cause: strike.Cause);
                    if (!enemy.Alive || enemy.Definition.IsBoss) continue;
                    enemy.ChillUntil = Mathf.Max(enemy.ChillUntil, Elapsed + strike.Slow);
                    if (strike.Pull == 0) continue;
                    Vector2 pull = (strike.Position - enemy.Position).normalized;
                    if (pull.sqrMagnitude < .001f) pull = strike.Direction;
                    enemy.Position = ClampPosition(enemy.Position + pull * strike.Pull * (1 - Mathf.Clamp01(enemy.Definition.KnockbackResistance)), enemy.Definition.Radius);
                    spatial.Move(enemy);
                }
            }
        }
#if UNITY_EDITOR
        public float MorphAttackFixture(UnitState unit, float damage = 30) => ReleaseBuildMorph(unit, damage, NewCause(unit.Id));
#endif
    }
}
