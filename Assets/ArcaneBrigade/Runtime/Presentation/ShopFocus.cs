using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>One answer, each frame, to "what is the traveller standing at?". Every shop interactable — the stalls,
    /// the reroll machine, the way out, the fusion altar, the secret invitations, the warm chair, the journal window and
    /// the outside stage's hand and gifts — offers its spots (where it stands and how far it reaches). The nearest spot
    /// within its own reach holds the focus and only it answers a press, so two things standing close together can
    /// never swallow each other's input (the invitation by the reroll machine used to block every reroll).</summary>
    public static class ShopFocus
    {
        public interface ISource
        {
            int SpotCount { get; }
            /// <summary>False when this spot cannot be used right now (sold, hidden, out of its phase).</summary>
            bool Spot(int index, out Vector2 position, out float reach);
        }
        private static readonly List<ISource> sources = new List<ISource>();
        private static int resolvedFrame = -1;
        private static Vector2 resolvedFor;
        private static ISource holder;
        private static int held = -1;

        public static void Register(ISource source) { if (source != null && !sources.Contains(source)) sources.Add(source); resolvedFrame = -1; }
        public static void Unregister(ISource source) { sources.Remove(source); if (holder == source) holder = null; resolvedFrame = -1; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void Reset() { sources.Clear(); holder = null; held = -1; resolvedFrame = -1; }

        /// <summary>True when this source's spot is the one the traveller at <paramref name="traveller"/> is standing at.</summary>
        public static bool Has(ISource source, int index, Vector2 traveller) { Resolve(traveller); return holder == source && held == index; }
        /// <summary>The source and spot holding the focus for a traveller standing here (null when nothing is in reach).</summary>
        public static ISource Holder(Vector2 traveller, out int index) { Resolve(traveller); index = held; return holder; }

        /// <summary>Every usable spot right now (diagnostics: spacing and "standing here selects this" checks).</summary>
        public static List<(ISource source, int index, Vector2 position, float reach)> Spots()
        {
            var all = new List<(ISource, int, Vector2, float)>();
            foreach (ISource source in sources)
                if (source != null && !(source is Object unity && unity == null))
                    for (int i = 0; i < source.SpotCount; i++) if (source.Spot(i, out Vector2 position, out float reach)) all.Add((source, i, position, reach));
            return all;
        }
        private static void Resolve(Vector2 traveller)
        {
            if (resolvedFrame == Time.frameCount && resolvedFor == traveller) return;
            resolvedFrame = Time.frameCount; resolvedFor = traveller; holder = null; held = -1;
            float best = float.PositiveInfinity;
            for (int s = sources.Count - 1; s >= 0; s--)
            {
                ISource source = sources[s];
                if (source == null || source is Object unity && unity == null) { sources.RemoveAt(s); continue; }
                for (int i = 0; i < source.SpotCount; i++)
                {
                    if (!source.Spot(i, out Vector2 position, out float reach)) continue;
                    float distance = Vector2.Distance(traveller, position);
                    if (distance <= reach && distance < best) { best = distance; holder = source; held = i; }
                }
            }
        }
    }
}
