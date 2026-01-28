using UnityEngine;

namespace ArcaneBrigade
{
    public static class SpeciesDefeatPerformance
    {
        public static Vector4 Dissolve(EnsembleGesture kind, float progress, float identity, int evolution, bool reduced)
        {
            if (reduced || progress < .12f) return Vector4.zero;
            float erosion = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.12f, .93f, progress));
            int pattern = kind == EnsembleGesture.Flame || kind == EnsembleGesture.Float ? 0 : kind == EnsembleGesture.Cloth || kind == EnsembleGesture.Wing ? 1 : kind == EnsembleGesture.Clock || kind == EnsembleGesture.Heavy ? 2 : 3;
            return new Vector4(erosion, identity * 97 + evolution * 13, pattern, evolution > 0 ? 1.35f : 1);
        }
        public static Vector3 Drift(EnsembleGesture kind, float progress, float identity, int evolution, bool reduced)
        {
            if (reduced) return Vector3.zero;
            float release = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.12f, .85f, progress));
            float weight = evolution > 0 ? 1.12f : 1;
            if (kind == EnsembleGesture.Float || kind == EnsembleGesture.Flame) return new Vector3(Mathf.Sin(progress * 5 + identity * 6) * release * .05f, release * .18f * weight, -release * 5);
            if (kind == EnsembleGesture.Wing || kind == EnsembleGesture.Cloth) return new Vector3(Mathf.Sin(progress * 7 + identity * 6) * release * .06f, -release * .04f, Mathf.Sin(progress * 6) * 8);
            if (kind == EnsembleGesture.Spring || kind == EnsembleGesture.Jelly) return new Vector3(0, AnimationTiming.Pulse(progress - .05f, .6f) * .08f, release * 6);
            if (kind == EnsembleGesture.Clock || kind == EnsembleGesture.Shell || kind == EnsembleGesture.Heavy) return new Vector3(0, -release * .025f, Mathf.Round(release * 4) * 2);
            return new Vector3(release * .025f, -release * .035f, release * 7);
        }
    }
}
