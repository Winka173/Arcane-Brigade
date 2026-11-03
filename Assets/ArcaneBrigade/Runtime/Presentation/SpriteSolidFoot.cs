using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Where a painted thing really stands: the lowest row of solid paint, in the sprite's local units. The ink bounds
    /// count faint pixels too, so a shopkeeper's glow or a creature's dust under its feet pulled its shadow down off its feet
    /// (requirement 4/11/2026: shadows looked missing). Built by Editor/SpriteSolidFootBuilder.</summary>
    public sealed class SpriteSolidFoot : ScriptableObject
    {
        [Serializable]
        public struct Entry { public Sprite Sprite; public float Foot; }
        public Entry[] Entries = Array.Empty<Entry>();
        private static Dictionary<Sprite, float> lookup;
        public static bool TryGet(Sprite sprite, out float foot)
        {
            foot = 0;
            if (sprite == null) return false;
            if (lookup == null)
            {
                lookup = new Dictionary<Sprite, float>();
                SpriteSolidFoot data = Resources.Load<SpriteSolidFoot>("SpriteSolidFoot");
                if (data != null) foreach (Entry entry in data.Entries) if (entry.Sprite != null) lookup[entry.Sprite] = entry.Foot;
            }
            return lookup.TryGetValue(sprite, out foot);
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Invalidate() { lookup = null; }
    }
}
