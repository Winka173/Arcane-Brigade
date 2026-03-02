using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The painted pieces of the Bizarre Arsenal (Art/Arsenal, Docs/Bizarre_Arsenal.md §5.1), by name, frames in order:
    /// "CherryBomb" -> CherryBomb_f0..f2, "FeltBall" -> one frame. Filled by ArsenalBuilder from the art that exists.</summary>
    public sealed class ArsenalArtwork : ScriptableObject
    {
        public CombatVfxEntry[] Entries = Array.Empty<CombatVfxEntry>();
        private static ArsenalArtwork current;
        private static Dictionary<string, Sprite[]> lookup;
        public static ArsenalArtwork Current => current != null ? current : current = Resources.Load<ArsenalArtwork>("ArsenalArtwork");
        public static Sprite[] Frames(string name)
        {
            if (name == null || Current == null) return null;
            if (lookup == null) { lookup = new Dictionary<string, Sprite[]>(StringComparer.Ordinal); foreach (var e in Current.Entries) lookup[e.Name] = e.Frames; }
            return lookup.TryGetValue(name, out var frames) && frames.Length > 0 ? frames : null;
        }
        /// <summary>Frame by 0..1 progress (clamped), or looping by time when fps &gt; 0.</summary>
        public static Sprite Frame(string name, float progress, float fps = 0, int offset = 0)
        {
            Sprite[] frames = Frames(name);
            if (frames == null) return null;
            if (PresentationSettings.ReducedMotion) return frames[0];
            int index = fps > 0 ? Mathf.Abs(Mathf.FloorToInt(progress * fps) + offset) % frames.Length : Mathf.Clamp(Mathf.FloorToInt(Mathf.Clamp01(progress) * frames.Length), 0, frames.Length - 1);
            return frames[index];
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void Reset() { current = null; lookup = null; }
    }
}
