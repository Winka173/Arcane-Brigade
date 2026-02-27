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
        private static readonly Dictionary<string, Sprite> pictures = new Dictionary<string, Sprite>();
        public static Sprite Picture(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            if (pictures.TryGetValue(name, out var sprite)) return sprite;
            var e = Find(name);
            sprite = Resources.Load<Sprite>("MienCho/ActOne/" + name) ?? (e?.Frames != null && e.Frames.Length > 0 ? e.Frames[0] : null);
            pictures[name] = sprite; return sprite;
        }
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
            if (unit?.Definition.Id == "OutsideStageKeeper")
                return Picture(!unit.Alive ? "Keeper_Kneel" : "Keeper_BodyShell");
            if (unit?.Definition.Id != NightmareRules.CustomerId) return null;
            // The borrowed face holds through a threat and its recovery; it is not a flashing timer effect.
            bool threatens = unit.PendingAttack || unit.TelegraphUntil > time || unit.BossRecoveryUntil > time;
            bool wounded = unit.Health < unit.Definition.Health * .35f;
            string key = !unit.Alive ? "LastCustomer_Blank" : threatens || wounded ? "LastCustomer_BorrowedFace" : "LastCustomer_Pleasant";
            var entry = Find(key);
            return entry?.Motion != null ? entry.Motion.Select(unit, time, PresentationSettings.ReducedMotion) : Picture(key);
        }
        private static readonly string[] KeeperIntact = { "Keeper_Hand_Intact", "Keeper_Carpet_Intact", "Keeper_Eye_Open" };
        private static readonly string[] KeeperBroken = { "Keeper_Hand_Broken", "Keeper_Carpet_Broken", "Keeper_Eye_Closed" };
        public static Sprite KeeperPart(int part, bool broken, Sprite fallback)
        {
            string[] names = broken ? KeeperBroken : KeeperIntact;
            return part >= 0 && part < names.Length ? Picture(names[part]) ?? fallback : fallback;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void Reset() { cached = null; lookup = null; pictures.Clear(); }
    }
}
