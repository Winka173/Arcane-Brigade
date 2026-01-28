using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class GroundedSceneryArtwork : ScriptableObject
    {
        public Sprite[] Cabinets = new Sprite[0];
        private static GroundedSceneryArtwork cached;
        public static GroundedSceneryArtwork Current => cached != null ? cached : cached = Resources.Load<GroundedSceneryArtwork>("GroundedSceneryArtwork");
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Invalidate() { cached = null; }
    }
}
