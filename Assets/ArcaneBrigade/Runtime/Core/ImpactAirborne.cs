using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>N2 knock-up and N15 chain blasts (owner, 19/10/2026).
    /// <list type="bullet">
    /// <item>A heavy slam (weapon weight ≥ 0.95) knocks a small enemy straight up for 0.4 s; it hangs helpless, then crashes
    /// down and the landing knocks the enemies around it (a fifth of the slam's damage, radius 0.9).</item>
    /// <item>A Blast Puffer's death burst now also blows up the enemies around it (40% of its health); a puffer felled by a
    /// burst bursts in turn, and from the second link on the chain is called out (CombatEventKind.ChainBlast).</item>
    /// </list></summary>
    public static class AirborneRules
    {
        public const float KnockUpWeight = .95f, KnockUpSeconds = .4f, KnockUpHeight = .9f, LandingShare = .2f, LandingRadius = .9f;
        public const float PufferBlastRadius = 1.6f, PufferBlastShare = .4f, PufferDelay = .6f;
        public const int ChainLimit = 8;
        public static bool Small(UnitDefinition d) => d != null && !d.IsBoss && d.BodyHeight < .95f && d.KnockbackResistance < .5f;
    }

    public sealed partial class CombatSimulation
    {
        private struct PufferBurst { public Vector2 At; public float When, Damage; public int Depth; public CombatCause Cause; }
        private readonly List<PufferBurst> pufferBursts = new List<PufferBurst>();
        private readonly Dictionary<int, int> blastDepth = new Dictionary<int, int>();
        private int blastingDepth = -1;
        public int ChainBlasts { get; private set; }

        /// <summary>N2: a heavy slam on a small living enemy sends it up.</summary>
        private void TryKnockUp(UnitState target, float weight, float damage, Faction source, CombatCause cause)
        {
            if (weight < AirborneRules.KnockUpWeight || !target.Alive || !AirborneRules.Small(target.Definition) || target.Flung || Elapsed < target.AirborneUntil + .3f) return;
            target.AirborneAt = Elapsed; target.AirborneUntil = Elapsed + AirborneRules.KnockUpSeconds;
            target.AirborneDamage = Mathf.Max(1, damage * AirborneRules.LandingShare); target.AirborneSource = source; target.AirborneCause = cause.Child("knock-up");
        }

        private void StepAirborne()
        {
            for (int u = 0, count = units.Count; u < count; u++)
            {
                UnitState unit = units[u];
                if (unit.AirborneDamage <= 0) continue;
                if (!unit.Alive) { unit.AirborneDamage = 0; unit.BodyLift = 0; continue; }
                float progress = Mathf.Clamp01((Elapsed - unit.AirborneAt) / AirborneRules.KnockUpSeconds);
                if (progress < 1) { unit.BodyLift = Mathf.Sin(progress * Mathf.PI) * AirborneRules.KnockUpHeight; continue; }
                // the crash down: the landing knocks the enemies around it, and itself
                unit.BodyLift = 0;
                float damage = unit.AirborneDamage; unit.AirborneDamage = 0;
                events.Add(new CombatEvent(CombatEventKind.BodyImpact, unit.Position, unit.AirborneSource, .7f, Vector2.down, cause: unit.AirborneCause));
                for (int o = 0, others = units.Count; o < others; o++)
                {
                    UnitState other = units[o];
                    if (other == unit || !other.Alive || other.Faction != Faction.Enemy) continue;
                    float reach = AirborneRules.LandingRadius + other.Definition.Radius;
                    if ((other.Position - unit.Position).sqrMagnitude > reach * reach) continue;
                    Vector2 away = (other.Position - unit.Position).sqrMagnitude > .0001f ? (other.Position - unit.Position).normalized : Vector2.right;
                    ApplyDamage(other, damage, unit.AirborneSource, "Knock-up landing", away, null, cause: unit.AirborneCause);
                }
                if (unit.Alive) ApplyDamage(unit, damage, unit.AirborneSource, "Knock-up landing", Vector2.down, null, cause: unit.AirborneCause);
            }
        }

        /// <summary>N15: a puffer's burst is scheduled with the link of the chain it belongs to.</summary>
        private void SchedulePufferBurst(UnitState puffer)
        {
            int depth = blastDepth.TryGetValue(puffer.Id, out int d) ? d : 0;
            blastDepth.Remove(puffer.Id);
            if (pufferBursts.Count >= 32 || depth >= AirborneRules.ChainLimit) return;
            pufferBursts.Add(new PufferBurst { At = puffer.Position, When = Elapsed + AirborneRules.PufferDelay, Damage = Mathf.Max(4, puffer.Definition.Health * AirborneRules.PufferBlastShare), Depth = depth, Cause = NewCause(puffer.Id).Child("puffer-chain") });
        }

        private void StepPufferBursts()
        {
            for (int index = pufferBursts.Count - 1; index >= 0; index--)
            {
                PufferBurst burst = pufferBursts[index];
                if (Elapsed < burst.When) continue;
                pufferBursts.RemoveAt(index);
                // seen and heard as a blast (presentation only, Value = its size); from the second link the chain is called out
                events.Add(new CombatEvent(CombatEventKind.Detonation, burst.At, Faction.Companion, AirborneRules.PufferBlastRadius, Vector2.up, cause: burst.Cause));
                if (burst.Depth >= 1) { ChainBlasts++; events.Add(new CombatEvent(CombatEventKind.ChainBlast, burst.At, Faction.Companion, burst.Depth + 1, Vector2.up, cause: burst.Cause)); }
                blastingDepth = burst.Depth + 1;
                for (int u = 0, count = units.Count; u < count; u++)
                {
                    UnitState enemy = units[u];
                    if (!enemy.Alive || enemy.Faction != Faction.Enemy) continue;
                    float reach = AirborneRules.PufferBlastRadius + enemy.Definition.Radius;
                    if ((enemy.Position - burst.At).sqrMagnitude > reach * reach) continue;
                    Vector2 away = (enemy.Position - burst.At).sqrMagnitude > .0001f ? (enemy.Position - burst.At).normalized : Vector2.right;
                    // a puffer felled here bursts as the next link (QueueDeathReaction reads blastingDepth)
                    ApplyDamage(enemy, enemy.Definition.IsBoss ? burst.Damage * .25f : burst.Damage, Faction.Companion, "Puffer chain", away, null, cause: burst.Cause);
                }
                blastingDepth = -1;
            }
        }
        private void ClearAirborne() { pufferBursts.Clear(); blastDepth.Clear(); blastingDepth = -1; }
    }
}
