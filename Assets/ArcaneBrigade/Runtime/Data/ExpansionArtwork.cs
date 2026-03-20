using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Units the new friends bring into a fight (content expansion): Eclipse Egg's three forms, The First Seed's sapling
    /// and Specimen Slime's little slime. Filled by ExpansionUnits from Art/Expansion.</summary>
    public sealed class ExpansionArtwork : ScriptableObject
    {
        public UnitDefinition[] EclipseForms = new UnitDefinition[0];
        public UnitDefinition Sapling, MiniSlime;
        /// <summary>The shop visitors, two poses each (waiting, offering) in ShopVisitors.All order; empty while not painted.</summary>
        public Sprite[] Visitors = new Sprite[0];
        public Sprite Visitor(string id, int pose = 0)
        {
            int i = System.Array.FindIndex(ShopVisitors.All, v => v.Id == id) * 2 + pose;
            return i >= 0 && i < Visitors.Length ? Visitors[i] : null;
        }
        public Sprite[] VisitorPoses(string id) => Visitor(id, 0) != null && Visitor(id, 1) != null ? new[] { Visitor(id, 0), Visitor(id, 1) } : null;
        private static ExpansionArtwork current;
        public static ExpansionArtwork Current => current != null ? current : current = Resources.Load<ExpansionArtwork>("ExpansionArtwork");
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void Reset() { current = null; }
    }
}
