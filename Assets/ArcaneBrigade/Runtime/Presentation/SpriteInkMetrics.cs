using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class SpriteInkMetrics : ScriptableObject
    {
        [Serializable]
        public struct Entry { public Sprite Sprite; public Rect Ink; }
        public Entry[] Entries = Array.Empty<Entry>();
        private static Dictionary<Sprite, Rect> lookup;
        public static Rect BoundsOf(Sprite sprite)
        {
            if (lookup == null)
            {
                lookup = new Dictionary<Sprite, Rect>();
                SpriteInkMetrics data = Resources.Load<SpriteInkMetrics>("SpriteInkMetrics");
                if (data != null) foreach (Entry entry in data.Entries) if (entry.Sprite != null) lookup[entry.Sprite] = entry.Ink;
            }
            if (sprite != null && lookup.TryGetValue(sprite, out Rect ink)) return ink;
            return sprite == null ? Rect.zero : new Rect(sprite.bounds.min, sprite.bounds.size);
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Invalidate() { lookup = null; }
    }
}
