using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Units the new friends bring into a fight (content expansion): Eclipse Egg's three forms, The First Seed's sapling
    /// and Specimen Slime's little slime. Filled by ExpansionUnits from Art/Expansion.</summary>
    public sealed class ExpansionArtwork : ScriptableObject
    {
        public UnitDefinition[] EclipseForms = new UnitDefinition[0];
        public UnitDefinition Sapling, MiniSlime;
        private static ExpansionArtwork current;
        public static ExpansionArtwork Current => current != null ? current : current = Resources.Load<ExpansionArtwork>("ExpansionArtwork");
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void Reset() { current = null; }
    }
}
