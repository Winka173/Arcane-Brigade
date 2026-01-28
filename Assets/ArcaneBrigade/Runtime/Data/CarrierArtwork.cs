using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class CarrierArtwork : ScriptableObject
    {
        public Sprite[] Hand;
        public Sprite[] Tongue;
        public Sprite[] Eye;
        private static CarrierArtwork cached;
        public static CarrierArtwork Current => cached != null ? cached : cached = Resources.Load<CarrierArtwork>("CarrierArtwork");
        public Sprite[] For(StageCarrier kind) => kind == StageCarrier.CarpetTongue ? Tongue : kind == StageCarrier.KeyholeEye ? Eye : Hand;
    }
}
