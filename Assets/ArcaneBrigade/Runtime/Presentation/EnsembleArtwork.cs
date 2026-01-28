using System;
using UnityEngine;

namespace ArcaneBrigade
{
    [Serializable]
    public sealed class EnsembleClip
    {
        public string Id;
        public Sprite[] Frames = Array.Empty<Sprite>();
        public float ReferenceWidth = 1;
        public Sprite At(float progress) => CharacterActingEntry.Frame(Frames, progress);
    }
    public sealed class EnsembleArtwork : ScriptableObject
    {
        public EnsembleClip[] Merchants = Array.Empty<EnsembleClip>();
        public EnsembleClip[] Weapons = Array.Empty<EnsembleClip>();
        private static EnsembleArtwork cached;
        public static EnsembleArtwork Current => cached != null ? cached : cached = Resources.Load<EnsembleArtwork>("EnsembleArtwork");
        public EnsembleClip Merchant(string id) => Find(Merchants, id);
        public EnsembleClip Weapon(string id) => Find(Weapons, id);
        private static EnsembleClip Find(EnsembleClip[] entries, string id)
        {
            foreach (EnsembleClip entry in entries) if (entry.Id == id) return entry;
            return null;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState() { cached = null; }
    }
}
