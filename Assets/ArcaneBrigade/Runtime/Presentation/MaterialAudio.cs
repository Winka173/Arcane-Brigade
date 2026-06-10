using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Blows sound like what they land on (19/10/2026, requirements V10/R16): a shove into the arena's edge, a flung body
    /// bowling into another and a flung body bursting where it lands all use the enemy's body material (paper, metal, wax,
    /// fabric, glass, wood) from the existing cue library, pitched down for weight. Hits already sound by material
    /// (ArcaneCueLibrary.EventId: cmb.hit.&lt;material&gt;). A swarm never spams: at most Budget material impacts per Window.</summary>
    public static class MaterialAudio
    {
        public enum Impact { Hit, WallSplat, BodyImpact, Landing }
        public const int Budget = 3, ReducedBudget = 1;
        public const float Window = 0.1f;
        private static readonly string[] Names = { "paper", "metal", "wax", "fabric", "glass", "wood" };
        private static readonly Dictionary<int, BodyMaterial> flung = new Dictionary<int, BodyMaterial>();
        private static readonly float[] recent = new float[Budget];


        /// <summary>The library cue for a material impact: the material's hit for blows, splats and bumps; its death burst
        /// (the K12 material fragments' sound) for a flung body landing.</summary>
        public static string Cue(BodyMaterial material, Impact kind)
        {
            int index = Mathf.Clamp((int)material, 0, Names.Length - 1);
            return (kind == Impact.Landing ? "enemy.die." : "cmb.hit.") + Names[index];
        }
        public static float Gain(Impact kind) => kind == Impact.WallSplat ? 1.1f : kind == Impact.BodyImpact ? 0.8f : kind == Impact.Landing ? 0.85f : 1f;
        /// <summary>Heavier moments sit lower: a splat lowest, a bump and a landing a little under the plain hit.</summary>
        public static float Pitch(Impact kind) => kind == Impact.WallSplat ? 0.82f : kind == Impact.BodyImpact ? 0.92f : kind == Impact.Landing ? 0.88f : 1f;

        /// <summary>Remembers what a flung body is made of (BodyLaunched carries the enemy) so its bumps and landing can sound it.</summary>
        public static void Observe(CombatEvent value)
        {
            if (value.Kind != CombatEventKind.BodyLaunched || value.Cause.RootId == 0 || value.Subject == null || value.Subject.Definition == null) return;
            if (flung.Count >= 64) flung.Clear();
            flung[value.Cause.RootId] = value.Subject.Definition.Material;
        }

        /// <summary>True when this event is a material impact: a wall splat, a flung body's bump, or a flung body landing.</summary>
        public static bool Claims(CombatEvent value, out Impact kind, out BodyMaterial material)
        {
            kind = Impact.Hit; material = BodyMaterial.Wood;
            switch (value.Kind)
            {
                case CombatEventKind.WallSplat:
                    kind = Impact.WallSplat;
                    if (value.Subject != null && value.Subject.Definition != null) { material = value.Subject.Definition.Material; return true; }
                    return flung.TryGetValue(value.Cause.RootId, out material); // a flung body glancing off the edge
                case CombatEventKind.BodyImpact:
                    kind = Impact.BodyImpact;
                    return flung.TryGetValue(value.Cause.RootId, out material);
                case CombatEventKind.Detonation when value.Cause.ItemId == "flung-body":
                    kind = Impact.Landing;
                    return flung.TryGetValue(value.Cause.RootId, out material);
            }
            return false;
        }

        /// <summary>The cue, gain and pitch for a material impact event (null when the event is not one).</summary>
        public static string CueFor(CombatEvent value, out float gain, out float pitch)
        {
            gain = 1; pitch = 1;
            if (!Claims(value, out Impact kind, out BodyMaterial material)) return null;
            gain = Gain(kind); pitch = Pitch(kind);
            return Cue(material, kind);
        }

        /// <summary>The swarm throttle: at most Budget (Reduced Audio: one) material impacts inside any Window seconds.</summary>
        public static bool Allow(float now)
        {
            int budget = PresentationSettings.ReducedAudio ? ReducedBudget : Budget;
            int inside = 0;
            for (int i = 0; i < recent.Length; i++) if (recent[i] > 0 && now - recent[i] < Window && now >= recent[i]) inside++;
            if (inside >= budget) return false;
            // replace the oldest slot
            int oldest = 0;
            for (int i = 1; i < recent.Length; i++) if (recent[i] < recent[oldest]) oldest = i;
            recent[oldest] = Mathf.Max(now, 0.0001f);
            return true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Clear() { flung.Clear(); for (int i = 0; i < recent.Length; i++) recent[i] = 0; }
    }
}
