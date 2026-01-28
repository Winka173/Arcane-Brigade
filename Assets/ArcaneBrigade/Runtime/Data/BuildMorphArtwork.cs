using UnityEngine;

namespace ArcaneBrigade
{
    [CreateAssetMenu(menuName = "Arcane Brigade/Build Morph Artwork")]
    public sealed class BuildMorphArtwork : ScriptableObject
    {
        public Sprite[] Parts = new Sprite[0];
        public Sprite Back(BuildMorphKind kind) => Parts.Length == 24 && BuildMorphs.Get(kind) != null ? Parts[(int)kind - 1] : null;
        public Sprite Charm(BuildMorphKind kind) => Parts.Length == 24 && BuildMorphs.Get(kind) != null ? Parts[12 + (int)kind - 1] : null;
    }
}
