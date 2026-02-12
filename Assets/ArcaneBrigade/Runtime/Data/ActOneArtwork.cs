using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    [Serializable]
    public sealed class ActOneArtEntry
    {
        public string Name;
        public Sprite[] Frames = Array.Empty<Sprite>();
        public AuthoredMotionSet Motion;
    }

    /// <summary>Canon artwork is separate from species assets: a bought teapot is never silently turned into Hob.</summary>
    public sealed class ActOneArtwork : ScriptableObject
    {
        public ActOneArtEntry[] Entries = Array.Empty<ActOneArtEntry>();
        private static ActOneArtwork cached;
        private static Dictionary<string, ActOneArtEntry> lookup;
        public static ActOneArtwork Current => cached != null ? cached : cached = Resources.Load<ActOneArtwork>("ActOneArtwork");
        public static ActOneArtEntry Find(string name)
        {
            if (name == null || Current == null) return null;
            if (lookup == null) { lookup = new Dictionary<string, ActOneArtEntry>(StringComparer.Ordinal); foreach (var e in Current.Entries) lookup[e.Name] = e; }
            return lookup.TryGetValue(name, out var entry) ? entry : null;
        }
        public static Sprite Picture(string name) { var e = Find(name); return e?.Frames != null && e.Frames.Length > 0 ? e.Frames[0] : null; }
        public static string Identity(UnitDefinition unit) => unit == null ? null : unit.DisplayName switch
        {
            "Hob" => "Hob", "Latch" => "Latch", "Postscript" => "Postscript", "Tally" => "Tally",
            "THE CHAIR PORTER" => "ChairPorter", "THE DOOR THAT STAYED SHUT" => "DoorThatStayedShut",
            "THE BEDSIDE" => "TheBedside", _ => unit.Id == NightmareRules.CustomerId ? "LastCustomer_Pleasant" : null
        };
        public static bool Has(UnitDefinition unit) => Find(Identity(unit))?.Motion != null;
        public static void Apply(UnitDefinition unit)
        {
            var entry = Find(Identity(unit));
            if (entry?.Motion == null || entry.Frames.Length == 0) return;
            unit.Body = entry.Frames[0]; unit.AnimationFrames = entry.Frames; unit.AuthoredMotion = entry.Motion;
        }
        public static Sprite CustomerFace(UnitState unit, float time)
        {
            if (unit?.Definition.Id != NightmareRules.CustomerId) return null;
            string key = !unit.Alive ? "LastCustomer_Blank" : (Mathf.FloorToInt(time * .45f) % 3 == 1 ? "LastCustomer_BorrowedFace" : "LastCustomer_Pleasant");
            var entry = Find(key);
            return entry?.Motion != null ? entry.Motion.Select(unit, time, PresentationSettings.ReducedMotion) : Picture(key);
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void Reset() { cached = null; lookup = null; }
    }
}
