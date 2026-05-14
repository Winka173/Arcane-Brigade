using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>A map's named elite announces itself as it walks in (owner 24/10/2026): its name and one line in a boss bubble
    /// over its head, once per elite.</summary>
    public static class EliteAnnouncer
    {
        private static readonly HashSet<int> told = new HashSet<int>();
        private static readonly Dictionary<string, string> Lines = new Dictionary<string, string>
        {
            ["HeadUsher"] = "Your number has been called.",
            ["DeadLetterCourier"] = "Return to sender. All of you.",
            ["WickWarden"] = "No light leaves this hall.",
            ["ArchiveSentinel"] = "You are not in the index.",
        };
        public static void Watch(CombatSimulation simulation)
        {
            if (simulation == null) return;
            if (simulation.Elapsed < .5f) told.Clear();
            foreach (UnitState unit in simulation.Units)
            {
                if (!unit.Alive || !unit.Elite || unit.Faction != Faction.Enemy || told.Contains(unit.Id) || !Lines.TryGetValue(unit.Definition.Id, out string line)) continue;
                told.Add(unit.Id); UnitState held = unit;
                SpeechBubbles.Show("Elite:" + unit.Id, unit.Definition.DisplayName, line, () => held.Alive ? SpeechBubbles.Head(held) : (Vector3?)null, SpeechTone.Boss, 3.2f, 2);
            }
        }
    }
}
