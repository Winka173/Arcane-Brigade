using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class OutsideStageAssets : ScriptableObject
    {
        public Sprite[] BossFrames = new Sprite[0];
        public Sprite[] RewardFrames = new Sprite[0];
        public UnitDefinition Boss;
        public BossAnatomyEntry Anatomy;
        public static OutsideStageAssets Current => Resources.Load<OutsideStageAssets>("OutsideStageAssets");
    }
}
