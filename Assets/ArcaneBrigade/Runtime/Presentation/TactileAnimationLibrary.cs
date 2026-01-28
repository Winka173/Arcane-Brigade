using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class TactileAnimationLibrary : ScriptableObject
    {
        public Sprite[] Paper;
        public Sprite[] Metal;
        public Sprite[] Wax;
        public Sprite[] Dust;
        public Sprite[] Chests;
        public Sprite[] For(BodyMaterial material) => material == BodyMaterial.Metal || material == BodyMaterial.Glass ? Metal : material == BodyMaterial.Wax || material == BodyMaterial.Wood ? Wax : Paper;
        public static TactileAnimationLibrary Current => Resources.Load<TactileAnimationLibrary>("TactileAnimationLibrary");
    }
}
