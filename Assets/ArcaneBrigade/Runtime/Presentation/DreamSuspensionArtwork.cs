using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class DreamSuspensionArtwork : ScriptableObject
    {
        public Sprite[] Frames;
        private static DreamSuspensionArtwork cached;
        public static DreamSuspensionArtwork Current => cached != null ? cached : cached = Resources.Load<DreamSuspensionArtwork>("DreamSuspensionArtwork");
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache() { cached = null; }
    }
}
