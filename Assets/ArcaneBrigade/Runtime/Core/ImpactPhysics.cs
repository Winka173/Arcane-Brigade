using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>A defeated enemy sent flying by a heavy blow: it travels, spins, glances off the arena's edge once (or is
    /// pinned there by a pointed shot) and knocks into up to three living enemies on its way, then bursts where it lands.
    /// A body it knocks down may fly on in turn (one more link), and a chain that fells three is a STRIKE.</summary>
    public sealed class FlungBody
    {
        public UnitDefinition Definition; public int UnitId; public Faction Source; public CombatCause Cause;
        public Vector2 Position, Velocity; public float Started, Until, Weight, Damage, Spin; public int Hits, Depth, Kills;
        public bool Bounced, Low, Pointed, Pinned, Struck3;
        public FlungBody Root;
        public readonly List<int> Struck = new List<int>(4);
        public float Progress(float now) => Mathf.Clamp01((now - Started) / Mathf.Max(.05f, Until - Started));
    }

    /// <summary>How hard our blows land (owner, 19/10/2026: B1-B5, V2, V6, V9, R9-R11, R13-R15). Each weapon has a weight
    /// (light pokes nudge, heavy swings and heavy shots shove); heavy blows stagger; a shove into the arena's edge splats (a
    /// pointed shot pins the enemy there for a moment); a heavy or crushing killing blow flings the body; blasts throw
    /// enemies away from their centre; pulls knock pulled enemies into each other. Ranged weapons have a sweet range.</summary>
    public static class ImpactRules
    {
        public const float WallSplatShare = .25f, FlungShare = .3f, StaggerSeconds = .22f, StaggerSpeed = .4f, PinSeconds = .5f, BlastWeight = .6f;
        public const int FlungHits = 3, FlungLimit = 24, FlungDepth = 1, StrikeKills = 3, StrikeCoins = 3;
        private static readonly HashSet<ArsenalWeapon> Heavy = new HashSet<ArsenalWeapon>
        {
            ArsenalWeapon.HomeRunBat, ArsenalWeapon.StarforgeClaymore, ArsenalWeapon.VerdictGavel, ArsenalWeapon.RollingPin, ArsenalWeapon.SnowShovel,
        };
        private static readonly string[] Points = { "Bow", "Arrow", "Dart", "Kunai", "Quill", "Needle", "Pin", "Harpoon", "Spear", "Javelin", "Lance" };
        /// <summary>0 (featherweight) to 1 (the heaviest swings); heavy shots (big, slow, recoiling) weigh in the middle.</summary>
        public static float Weight(WeaponDefinition w)
        {
            if (w == null) return 0;
            if (Heavy.Contains(w.Arsenal) || w.Motion == WeaponMotion.Slam) return 1;
            if (w.Kind == WeaponKind.Melee) return w.Motion == WeaponMotion.Slash ? .65f : .55f;
            if (w.Motion == WeaponMotion.Bow) return .2f;
            return HeavyShot(w) ? .6f : .3f;
        }
        /// <summary>A heavy shot: a recoiling launcher with a big or slow, hard-hitting round.</summary>
        public static bool HeavyShot(WeaponDefinition w) => w != null && w.Kind == WeaponKind.Projectile && w.Motion == WeaponMotion.Recoil
            && (w.ProjectileRadius >= .1f || w.ProjectileVisualWidth >= .4f || w.Cooldown >= 1.1f && w.Damage >= 12);
        /// <summary>Arrows, darts, kunai, quills, needles, pins, harpoons: shots that stick (and pin a body to the edge).</summary>
        public static bool Pointed(WeaponDefinition w)
        {
            if (w == null || w.Kind != WeaponKind.Projectile) return false;
            if (w.Motion == WeaponMotion.Bow) return true;
            // whole words of the id (KunaiFan, SonarHarpoon), so PinataBomb, Pinwheel or JugglingPins never count
            string id = w.Id ?? "";
            int start = 0;
            for (int i = 1; i <= id.Length; i++)
            {
                if (i < id.Length && !char.IsUpper(id[i])) continue;
                if (System.Array.IndexOf(Points, id.Substring(start, i - start)) >= 0) return true;
                start = i;
            }
            return false;
        }
        public static float PushScale(float weight) => Mathf.Lerp(.7f, 1.6f, Mathf.Clamp01(weight));
        /// <summary>A body flies only when the killing blow dwarfs it (owner, 19/10/2026): a heavy blow dealing over twice the
        /// enemy's health, anything else over three times; bows and darts never fling. overkillShare = (damage − health left) / max health.</summary>
        public static bool Launches(float weight, float overkillShare, bool critical) => weight >= .3f && overkillShare >= (weight >= .75f ? 1f : 2f);

        /// <summary>R10, each ranged weapon's sweet range: short guns hit harder up close and a little softer far away; long
        /// guns the other way round. distance is shooter to target when the shot lands.</summary>
        public static float RangeFactor(WeaponDefinition w, float distance)
        {
            if (w == null || w.Kind != WeaponKind.Projectile) return 1;
            if (w.Range < 4.5f) return distance <= 1.8f ? 1.25f : distance >= w.Range * .75f ? .9f : 1;
            if (w.Range >= 6f) return distance <= 2f ? .85f : distance >= w.Range * .6f ? 1.2f : 1;
            return 1;
        }
        /// <summary>R11, how a ranged weapon prefers its target (lower is better, added to the usual distance score): long
        /// single shots pick the toughest, spreads pick the crowd, bows pick what is closest to the hero.</summary>
        public static float TargetBias(WeaponDefinition w, UnitState enemy, Vector2 hero, int crowd)
        {
            if (w == null || w.Kind != WeaponKind.Projectile) return 0;
            if (w.Motion == WeaponMotion.Bow) return .5f * (enemy.Position - hero).magnitude;
            if (w.ProjectileCount >= 3 || w.ProjectileSpread >= .35f) return -.6f * Mathf.Min(5, crowd);
            if (w.Range >= 6f) return -Mathf.Min(2.5f, enemy.Health * .012f);
            return 0;
        }
    }

    public sealed partial class CombatSimulation
    {
        private readonly List<FlungBody> flung = new List<FlungBody>();
        private readonly List<UnitState> crowdCandidates = new List<UnitState>();
        public IReadOnlyList<FlungBody> Flung => flung;
        public int Strikes { get; private set; }
        /// <summary>While a blast resolves, its hits weigh at least this much (R15: blasts throw enemies away from their centre).</summary>
        private float impactWeightFloor = -1;

        /// <summary>Our blow lands on a living enemy: a weighted shove, a stagger for heavy blows, and a splat against the
        /// arena's edge (its extra damage is taken straight from health here, so no second hit resolves inside this one).
        /// A pointed shot that drives an enemy into the edge pins it there for a moment.</summary>
        private void ImpactPush(UnitState target, Vector2 direction, WeaponDefinition weapon, Faction source, float damage, CombatCause cause)
        {
            float weight = Mathf.Max(ImpactRules.Weight(weapon), impactWeightFloor);
            target.HitWeight = weight;
            TryKnockUp(target, weight, damage, source, cause); // N2: a heavy slam sends a small enemy up
            // a melee blow always shoves away from the one who struck it (some swings report their aim, not the blow's way)
            UnitState striker = weapon.Kind == WeaponKind.Melee ? UnitWithId(cause.SourceUnitId) : null;
            if (striker != null && striker != target)
            {
                Vector2 away = target.Position - striker.Position;
                if (away.sqrMagnitude > .0001f && Vector2.Dot(away, direction) < 0) direction = away.normalized;
            }
            if (weight >= .8f && !target.Definition.IsBoss) target.StaggerUntil = Mathf.Max(target.StaggerUntil, Elapsed + ImpactRules.StaggerSeconds);
            float push = OctoberBalance.Push(target.Definition, impactWeightFloor > 0 ? Mathf.Max(weapon.PushDistance, .45f) : weapon.PushDistance) * ImpactRules.PushScale(weight);
            if (push <= 0) return;
            Vector2 wish = target.Position + direction.normalized * push, kept = ClampPosition(wish, target.Definition.Radius);
            target.Position = kept; spatial.Move(target);
            if (push >= .35f) events.Add(new CombatEvent(CombatEventKind.Knockback, target.Position, source, push, direction, weapon, subjectId: target.Id, subject: target, cause: cause));
            bool edge = (wish - kept).sqrMagnitude > .0025f;
            if (edge && ImpactRules.Pointed(weapon) && !target.Definition.IsBoss) target.PinnedUntil = Mathf.Max(target.PinnedUntil, Elapsed + ImpactRules.PinSeconds);
            if (edge && weight >= .5f && Elapsed >= target.WallSplatAt + .5f)
            {
                target.WallSplatAt = Elapsed;
                float extra = Mathf.Min(target.Health, Mathf.Max(1, damage * ImpactRules.WallSplatShare));
                target.Health -= extra;
                if (source != Faction.Enemy) RecordDamage(cause.Child("wall-splat"), extra);
                events.Add(new CombatEvent(CombatEventKind.WallSplat, kept, source, extra, direction, weapon, subjectId: target.Id, subject: target, cause: cause.Child("wall-splat")));
            }
        }

        /// <summary>A heavy or crushing killing blow flings the body instead of popping it where it stood.</summary>
        private void TryFling(UnitState target, Vector2 direction, WeaponDefinition weapon, Faction source, float damage, float overkillShare, bool critical, CombatCause cause)
        {
            if (source == Faction.Enemy || target.Faction != Faction.Enemy || target.Definition.IsBoss || weapon == null || direction.sqrMagnitude < .001f || flung.Count >= ImpactRules.FlungLimit) return;
            float weight = Mathf.Max(ImpactRules.Weight(weapon), impactWeightFloor);
            if (!ImpactRules.Launches(weight, overkillShare, critical)) return;
            Fling(target, direction, weight, source, Mathf.Min(damage, target.Definition.Health) * ImpactRules.FlungShare, cause.Child("flung-body"), null, weapon.Kind == WeaponKind.Projectile, ImpactRules.Pointed(weapon));
            events.Add(new CombatEvent(CombatEventKind.BodyLaunched, target.Position, target.Faction, weight, direction, weapon, critical, subjectId: target.Id, subject: target, cause: cause));
        }
        private void Fling(UnitState target, Vector2 direction, float weight, Faction source, float damage, CombatCause cause, FlungBody parent, bool low, bool pointed)
        {
            // shorter flights than at first (owner, 19/10/2026): about a third of the old distance
            float speed = (3.2f + 2.6f * weight) * (1 - Mathf.Clamp01(target.Definition.KnockbackResistance) * .5f) * (parent != null ? .75f : 1);
            var body = new FlungBody
            {
                Definition = target.Definition, UnitId = target.Id, Source = source, Cause = cause, Position = target.Position, Velocity = direction.normalized * speed,
                Started = Elapsed, Until = Elapsed + .35f + .25f * weight, Weight = weight, Damage = Mathf.Max(1, damage), Spin = (target.Id % 2 == 0 ? 1 : -1) * (540 + 360 * weight),
                Depth = parent == null ? 0 : parent.Depth + 1, Low = low, Pointed = pointed
            };
            body.Root = parent == null ? body : parent.Root;
            flung.Add(body);
            target.Flung = true;
        }

        private void StepFlung(float delta)
        {
            for (int index = flung.Count - 1; index >= 0; index--)
            {
                FlungBody body = flung[index];
                if (Elapsed >= body.Until)
                {
                    // it lands and bursts (presentation only: Value is the burst's size)
                    events.Add(new CombatEvent(CombatEventKind.Detonation, body.Position, Faction.Companion, .7f + .3f * body.Weight, body.Velocity.sqrMagnitude > .01f ? body.Velocity.normalized : Vector2.up, cause: body.Cause));
                    flung.Remove(body); index = Mathf.Min(index, flung.Count); continue;
                }
                if (body.Pinned) continue;
                float radius = body.Definition.Radius;
                Vector2 wish = body.Position + body.Velocity * delta, kept = ClampPosition(wish, radius);
                if ((wish - kept).sqrMagnitude > .0001f)
                {
                    if (body.Pointed)
                    {
                        // R14: a pointed shot pins the body to the edge; it hangs there a moment before it falls
                        body.Pinned = true; body.Position = kept; body.Until = Mathf.Max(body.Until, Elapsed + .45f);
                        events.Add(new CombatEvent(CombatEventKind.WallSplat, kept, body.Source, 0, body.Velocity.normalized, cause: body.Cause.Child("pinned")));
                        continue;
                    }
                    if (!Mathf.Approximately(kept.x, wish.x)) body.Velocity.x = -body.Velocity.x * .6f;
                    if (!Mathf.Approximately(kept.y, wish.y)) body.Velocity.y = -body.Velocity.y * .6f;
                    if (!body.Bounced) { body.Bounced = true; events.Add(new CombatEvent(CombatEventKind.WallSplat, kept, body.Source, 0, body.Velocity.normalized, cause: body.Cause)); }
                }
                body.Position = kept;
                body.Velocity *= Mathf.Pow(.35f, delta);
                if (body.Hits >= ImpactRules.FlungHits) continue;
                // it bowls into the living enemies it meets (a snapshot of the count: a hit may add units)
                for (int u = 0, count = units.Count; u < count && body.Hits < ImpactRules.FlungHits; u++)
                {
                    UnitState enemy = units[u];
                    if (!enemy.Alive || enemy.Faction != Faction.Enemy || enemy.Id == body.UnitId || enemy.EntranceUntil > Elapsed || body.Struck.Contains(enemy.Id)) continue;
                    float reach = radius * .8f + enemy.Definition.Radius;
                    if ((enemy.Position - body.Position).sqrMagnitude > reach * reach) continue;
                    body.Struck.Add(enemy.Id); body.Hits++;
                    Vector2 along = body.Velocity.sqrMagnitude > .01f ? body.Velocity.normalized : (enemy.Position - body.Position).normalized;
                    if (!enemy.Definition.IsBoss)
                    {
                        enemy.Position = ClampPosition(enemy.Position + along * .4f * (1 - Mathf.Clamp01(enemy.Definition.KnockbackResistance)), enemy.Definition.Radius);
                        spatial.Move(enemy);
                    }
                    events.Add(new CombatEvent(CombatEventKind.BodyImpact, Vector2.Lerp(body.Position, enemy.Position, .5f), body.Source, body.Weight, along, cause: body.Cause));
                    ApplyDamage(enemy, body.Damage, body.Source, "Flung body", along, null, cause: body.Cause);
                    body.Velocity *= .7f;
                    if (enemy.Alive) continue;
                    // V2: the body it knocks down flies on (one more link); V9: a chain that fells three is a STRIKE
                    body.Root.Kills++;
                    if (body.Depth < ImpactRules.FlungDepth && !enemy.Definition.IsBoss && !enemy.Flung && flung.Count < ImpactRules.FlungLimit)
                        Fling(enemy, along, body.Weight * .8f, body.Source, body.Damage * .7f, body.Cause.Child("chain"), body, body.Low, false);
                    if (body.Root.Kills >= ImpactRules.StrikeKills && !body.Root.Struck3)
                    {
                        body.Root.Struck3 = true; Strikes++;
                        events.Add(new CombatEvent(CombatEventKind.Strike, enemy.Position, body.Source, body.Root.Kills, along, cause: body.Root.Cause.Child("strike")));
                        DropLoot(LootKind.Coin, enemy.Position, ImpactRules.StrikeCoins);
                    }
                }
            }
        }

        /// <summary>V6: an enemy dragged by a pull (magnet bolt, grabber, projector) that is hauled into another enemy knocks
        /// into it: both take a small knock (a fifth of the puller's hit, at least 1), at most once each per 0.4 s.</summary>
        private void PullBump(UnitState pulled, float hit, Faction source, CombatCause cause)
        {
            if (pulled == null || !pulled.Alive || pulled.Faction != Faction.Enemy || Elapsed < pulled.BumpAt) return;
            for (int u = 0, count = units.Count; u < count; u++)
            {
                UnitState other = units[u];
                if (other == pulled || !other.Alive || other.Faction != Faction.Enemy || Elapsed < other.BumpAt || other.EntranceUntil > Elapsed) continue;
                float reach = pulled.Definition.Radius + other.Definition.Radius;
                if ((other.Position - pulled.Position).sqrMagnitude > reach * reach) continue;
                pulled.BumpAt = other.BumpAt = Elapsed + .4f;
                Vector2 along = (other.Position - pulled.Position).sqrMagnitude > .0001f ? (other.Position - pulled.Position).normalized : Vector2.right;
                float knock = Mathf.Max(1, hit * .2f);
                events.Add(new CombatEvent(CombatEventKind.BodyImpact, Vector2.Lerp(pulled.Position, other.Position, .5f), source, .5f, along, cause: cause.Child("pull-bump")));
                ApplyDamage(other, knock, source, "Knocked together", along, null, cause: cause.Child("pull-bump"));
                ApplyDamage(pulled, knock, source, "Knocked together", -along, null, cause: cause.Child("pull-bump"));
                return;
            }
        }

        /// <summary>R10: the ranged weapon's sweet-range factor for a hit (shooter to target now).</summary>
        private float RangedHitFactor(UnitState target, WeaponDefinition weapon, Faction source, CombatCause cause)
        {
            if (source == Faction.Enemy || weapon == null || weapon.Kind != WeaponKind.Projectile || target.Faction != Faction.Enemy) return 1;
            // the friends' guns (each has its own sweet range); the hero's own weapon and its kit keep their authored damage
            UnitState shooter = UnitWithId(cause.SourceUnitId);
            return shooter == null || shooter.Faction != Faction.Companion ? 1 : ImpactRules.RangeFactor(weapon, (shooter.Position - target.Position).magnitude);
        }
        /// <summary>R11: how many enemies stand around this one (for spreads that like a crowd).</summary>
        private int CrowdAround(UnitState enemy)
        {
            spatial.Query(enemy.Position - Vector2.one * 1.6f, enemy.Position + Vector2.one * 1.6f, crowdCandidates);
            int crowd = 0;
            foreach (UnitState other in crowdCandidates) if (other != enemy && other.Alive && other.Faction == Faction.Enemy && (other.Position - enemy.Position).sqrMagnitude <= 2.56f) crowd++;
            return crowd;
        }
    }
}
