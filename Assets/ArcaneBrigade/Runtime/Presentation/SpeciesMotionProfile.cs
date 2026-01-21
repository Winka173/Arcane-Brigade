using UnityEngine;

namespace ArcaneBrigade
{
    public readonly struct SpeciesMotionProfile
    {
        public readonly float IdleCadence;
        public readonly float GaitCadence;
        public readonly float GaitAmplitude;
        public readonly float HeadTurn;
        public readonly float Recoil;
        public readonly float IdleGesture;
        public readonly float LowerSquash;
        public readonly int GestureKind;
        public SpeciesMotionProfile(UnitDefinition unit)
        {
            uint identity = 2166136261;
            foreach (char letter in unit.Id ?? "") identity = unchecked((identity ^ letter) * 16777619);
            float variation = (identity % 1000) / 999f;
            float weight = Mathf.Max(1, unit.BodyHeight);
            IdleCadence = (unit.Material == BodyMaterial.Metal ? 1.6f : unit.Material == BodyMaterial.Wax ? 1.3f : 2.1f) + variation * 0.8f;
            GaitCadence = (9 + variation * 4) / Mathf.Sqrt(weight);
            GaitAmplitude = (unit.Material == BodyMaterial.Metal ? 0.018f : 0.025f + variation * 0.012f) / Mathf.Sqrt(weight);
            HeadTurn = unit.Material == BodyMaterial.Glass ? 0.6f : 1.4f + variation * 1.6f;
            Recoil = unit.Material == BodyMaterial.Metal ? 0.028f : unit.Material == BodyMaterial.Fabric ? 0.075f : 0.045f + variation * 0.025f;
            IdleGesture = unit.Material == BodyMaterial.Wood ? 0.018f : unit.Material == BodyMaterial.Glass ? 0.035f : 0.025f + variation * 0.025f;
            LowerSquash = unit.Material == BodyMaterial.Metal || unit.Material == BodyMaterial.Wood ? 0.018f : 0.05f + variation * 0.02f;
            GestureKind = (int)(identity % 4);
        }
    }
}
