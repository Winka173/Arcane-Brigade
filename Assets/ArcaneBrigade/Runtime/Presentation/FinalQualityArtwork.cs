using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class FinalQualityArtwork : ScriptableObject
    {
        [Serializable] public sealed class Entry { public string Id; public Sprite[] Frames; }
        public Entry[] Props = Array.Empty<Entry>();
        private Dictionary<string, Entry> lookup;
        private static FinalQualityArtwork cached;
        public static FinalQualityArtwork Current => cached != null ? cached : cached = Resources.Load<FinalQualityArtwork>("FinalQualityArtwork");
        public Sprite Frame(string identity, float time, bool reduced)
        {
            if (lookup == null) { lookup = new Dictionary<string, Entry>(StringComparer.Ordinal); foreach (Entry entry in Props) lookup.Add(entry.Id, entry); }
            if (!lookup.TryGetValue(identity, out Entry item) || item.Frames.Length != 4) return null;
            float phase = Mathf.Repeat(time + AnimationTiming.Phase(identity) * 9, 9);
            int frame = reduced || phase > 2 ? 0 : phase < .5f ? 1 : phase < 1 ? 2 : 3;
            return item.Frames[frame];
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache() { cached = null; }
    }
}
