using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    [Serializable]
    public sealed class ExpandedPerformanceEntry
    {
        public string Id;
        public string Kind;
        public int Evolution;
        public Sprite[] Frames = Array.Empty<Sprite>();
        public float ReferenceHeight = 1;
        public Sprite Select(UnitState unit, float time, bool reduced)
        {
            if (reduced || Frames.Length < 4) return null;
            if (Kind == "locomotion")
            {
                if (unit.PendingAttack) return Frames[5];
                float age = time - unit.AttackStarted;
                float windup = unit.AttackWindup > 0 ? unit.AttackWindup : unit.Definition.Weapon.Windup;
                if (age >= windup && age < windup + unit.Definition.Weapon.Recovery)
                    return Frames[age - windup < unit.Definition.Weapon.Recovery * .55f ? 6 : 7];
                bool moving = (unit.Position - unit.PreviousPosition).sqrMagnitude > .000001f;
                if (moving) return Frames[2 + Mathf.FloorToInt(time * (unit.TravelMode == CompanionTravelMode.Pursuit ? 12 : 8) + unit.Id * .37f) % 3];
                return Frames[Mathf.Repeat(time + unit.Id * .73f, 4.7f) < .14f ? 1 : 0];
            }
            float hurt = time - unit.LastHit;
            if (hurt >= 0 && hurt < .14f) return Frames[2];
            if (unit.PendingAttack || unit.Definition.IsBoss && unit.TelegraphUntil > time || unit.EvolutionReleaseAt > time || unit.Variant != null && unit.Variant.ReleaseAt > time) return Frames[0];
            if (unit.Definition.IsBoss && unit.BossRecoveryUntil > time || unit.EvolutionRecoveryUntil > time || unit.Variant != null && unit.Variant.TravelEnd > time) return Frames[1];
            float attackAge = time - unit.AttackStarted;
            float attackWindup = unit.AttackWindup > 0 ? unit.AttackWindup : unit.Definition.Weapon.Windup;
            return attackAge >= attackWindup && attackAge < attackWindup + unit.Definition.Weapon.Recovery ? Frames[1] : null;
        }
    }

    public sealed class ExpandedPerformanceLibrary : ScriptableObject
    {
        public ExpandedPerformanceEntry[] Entries = Array.Empty<ExpandedPerformanceEntry>();
        private Dictionary<string, ExpandedPerformanceEntry> lookup;
        private static ExpandedPerformanceLibrary cached;
        public static ExpandedPerformanceLibrary Current => cached != null ? cached : cached = Resources.Load<ExpandedPerformanceLibrary>("ExpandedPerformanceLibrary");
        public ExpandedPerformanceEntry Find(string kind, string id, int evolution = 0)
        {
            if (lookup == null)
            {
                lookup = new Dictionary<string, ExpandedPerformanceEntry>(StringComparer.Ordinal);
                foreach (ExpandedPerformanceEntry entry in Entries) lookup[Key(entry.Kind, entry.Id, entry.Evolution)] = entry;
            }
            return lookup.TryGetValue(Key(kind, id, evolution), out ExpandedPerformanceEntry found) ? found : null;
        }
        private static string Key(string kind, string id, int evolution) => kind + ":" + id + ":" + (evolution > 0 ? 1 : 0);
        public static ExpandedPerformanceEntry Reaction(UnitDefinition unit) => Current == null ? null : Current.Find(unit.IsBoss ? "bosses" : "enemies", unit.Id, unit.EvolutionStage);
        public static void Invalidate() { cached = null; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState() { Invalidate(); }
    }
}
