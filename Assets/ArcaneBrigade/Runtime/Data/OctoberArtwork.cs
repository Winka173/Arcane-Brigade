using UnityEngine;
namespace ArcaneBrigade
{
    public sealed class OctoberArtwork : ScriptableObject
    {
        public Texture2D Floor;
        public Sprite[] Watchers, Door, Keeper;
        public static OctoberArtwork Current => Resources.Load<OctoberArtwork>("October/Artwork");
    }
}
