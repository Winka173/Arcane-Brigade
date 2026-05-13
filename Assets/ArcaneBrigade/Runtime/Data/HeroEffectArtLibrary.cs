using System;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The painted physical objects of the hero skills (Docs/Hero_Effect_Art_Delivery.md, 24/10/2026): an acorn gun's
    /// crank, the sorter's barrels, the engine's piston, carriage couplings, mirror panes, citadel walls, juggling pins and
    /// balls, fleet kites, the diving bell, the bonsai's gear branch, the mortar's pot lid and the stag's hourglass. Each
    /// sheet's frames by its file name (Resources/HeroEffectArtLibrary, built by HeroEffectArtLibraryBuilder).</summary>
    public sealed class HeroEffectArtLibrary : ScriptableObject
    {
        [Serializable] public sealed class Entry { public string Key; public Sprite[] Frames; }
        public Entry[] Entries = Array.Empty<Entry>();

        private static HeroEffectArtLibrary current; private static bool tried;
        /// <summary>The frames of one sheet (for example "TreasureAcorn_Crank"), or null while the art is missing.</summary>
        public static Sprite[] Frames(string key)
        {
            if (!tried) { tried = true; current = Resources.Load<HeroEffectArtLibrary>("HeroEffectArtLibrary"); }
            if (current == null) return null;
            foreach (Entry e in current.Entries) if (e != null && e.Key == key && e.Frames != null && e.Frames.Length > 0) return e.Frames;
            return null;
        }
        /// <summary>One frame of a sheet, wrapped into range (null while the art is missing).</summary>
        public static Sprite Frame(string key, int index) { Sprite[] f = Frames(key); return f == null ? null : f[((index % f.Length) + f.Length) % f.Length]; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void Reset() { current = null; tried = false; }
    }
}
