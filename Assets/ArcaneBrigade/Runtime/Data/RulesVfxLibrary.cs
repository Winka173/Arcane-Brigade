using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The painted effect art the rules effects use, by key: "&lt;Look&gt;_Burst" (frames), "&lt;Look&gt;_Ground",
    /// "&lt;Look&gt;_Bits" (particle pieces), "Shield", "Heal_Bits", "Snow_Bits", "Rain_Bits", "Danger_Ground",
    /// "Danger_Burst". Built by RulesVfxLibraryBuilder from Art/RulesVFX, falling back to the existing CombatVFX paintings.</summary>
    public sealed class RulesVfxLibrary : ScriptableObject
    {
        [Serializable] public sealed class Entry { public string Key; public Sprite[] Frames = new Sprite[0]; }
        public List<Entry> Entries = new List<Entry>();

        private Dictionary<string, Sprite[]> lookup;
        public Sprite[] Get(string key)
        {
            if (lookup == null)
            {
                lookup = new Dictionary<string, Sprite[]>(StringComparer.OrdinalIgnoreCase);
                foreach (Entry entry in Entries) if (entry != null && entry.Key != null && entry.Frames != null && entry.Frames.Length > 0) lookup[entry.Key] = entry.Frames;
            }
            return lookup.TryGetValue(key, out Sprite[] frames) ? frames : null;
        }

        private static RulesVfxLibrary loaded; private static bool tried;
        public static RulesVfxLibrary Load() { if (!tried) { tried = true; loaded = Resources.Load<RulesVfxLibrary>("RulesVfxLibrary"); } return loaded; }
    }
}
