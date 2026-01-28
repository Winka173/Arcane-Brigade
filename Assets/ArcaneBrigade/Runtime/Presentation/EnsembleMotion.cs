using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public enum EnsembleGesture { Float, Fold, Spring, Wing, Shell, Stalk, Clock, Jelly, Flame, Cloth, Heavy }
    public static class EnsembleMotion
    {
        private static readonly Dictionary<string, EnsembleGesture> profiles = new Dictionary<string, EnsembleGesture>();
        public static EnsembleGesture For(UnitDefinition unit)
        {
            string id = unit.Id ?? string.Empty;
            if (!profiles.TryGetValue(id, out EnsembleGesture result)) { result = Classify(id); profiles.Add(id, result); }
            return result;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState() { profiles.Clear(); }
        private static EnsembleGesture Classify(string id)
        {
            if (id.Contains("Moth") || id.Contains("Bat") || id.Contains("Bird") || id.Contains("Owl")) return EnsembleGesture.Wing;
            if (id.Contains("Clock") || id.Contains("Tick") || id.Contains("Dice") || id.Contains("Compass") || id.Contains("Reel")) return EnsembleGesture.Clock;
            if (id.Contains("Snail") || id.Contains("Crab") || id.Contains("Shell") || id.Contains("Egg") || id.Contains("Acorn") || id.Contains("Beetle")) return EnsembleGesture.Shell;
            if (id.Contains("Spring") || id.Contains("Coil") || id.Contains("Keg") || id.Contains("Duck") || id.Contains("Toad")) return EnsembleGesture.Spring;
            if (id.Contains("Kite") || id.Contains("Book") || id.Contains("Mail") || id.Contains("Paper")) return EnsembleGesture.Fold;
            if (id.Contains("Sock") || id.Contains("Ribbon") || id.Contains("Pillow") || id.Contains("Curtain") || id.Contains("Hood")) return EnsembleGesture.Cloth;
            if (id.Contains("Jelly") || id.Contains("Koi") || id.Contains("Squid") || id.Contains("Bubble")) return EnsembleGesture.Jelly;
            if (id.Contains("Flame") || id.Contains("Ember") || id.Contains("Furnace") || id.Contains("Candle")) return EnsembleGesture.Flame;
            if (id.Contains("Mushroom") || id.Contains("Pear") || id.Contains("Radish") || id.Contains("Seed") || id.Contains("Spore")) return EnsembleGesture.Stalk;
            if (id.Contains("Golem") || id.Contains("Door") || id.Contains("Wardrobe") || id.Contains("Ram") || id.Contains("Coffin")) return EnsembleGesture.Heavy;
            return EnsembleGesture.Float;
        }
        public static Vector4 Surface(UnitState unit, float time, bool reduced)
        {
            if (reduced || !unit.Alive || !AnimationDetailBudget.SurfaceDetail || unit.PendingAttack || K8Motion.Attacking(unit, time) || unit.TelegraphUntil > time || time - unit.LastHit < .2f) return Vector4.zero;
            EnsembleGesture kind = For(unit.Definition);
            float phase = AnimationTiming.Phase(unit.Definition.Id) * 6.283185f + unit.Id * .71f;
            float beat = Mathf.Sin(time * (kind == EnsembleGesture.Wing ? 7 : kind == EnsembleGesture.Clock ? 2.5f : 2) + phase);
            float cloth = kind == EnsembleGesture.Cloth || kind == EnsembleGesture.Wing || kind == EnsembleGesture.Fold ? .006f * beat : kind == EnsembleGesture.Flame ? .004f * beat : .0015f * beat;
            float glance = Mathf.Sin(time * .7f + phase) * .0025f;
            float mouth = kind == EnsembleGesture.Jelly || kind == EnsembleGesture.Spring ? beat * .002f : 0;
            return new Vector4(glance, mouth, cloth, phase + time * 3);
        }
        public static Vector3 Goodbye(UnitDefinition unit, float elapsed, int index, bool reduced)
        {
            if (reduced) return Vector3.zero;
            float age = elapsed - index * .22f;
            float looking = AnimationTiming.Pulse(age - .8f, .85f);
            float farewell = AnimationTiming.Pulse(age - 1.65f, .65f);
            float turn = For(unit) == EnsembleGesture.Clock ? Mathf.Round(looking * 3) / 3 : looking;
            float bow = For(unit) == EnsembleGesture.Wing ? farewell * -5 : For(unit) == EnsembleGesture.Heavy ? farewell * 2 : farewell * -3;
            return new Vector3(turn, bow, For(unit) == EnsembleGesture.Jelly || For(unit) == EnsembleGesture.Spring ? farewell * 5 : 0);
        }
        public static Vector4 WeaponSurface(WeaponDefinition weapon, float age, float windup, bool reduced)
        {
            if (reduced || age < 0 || age >= windup + weapon.Recovery) return Vector4.zero;
            float tension = age < windup ? Mathf.SmoothStep(0, 1, age / Mathf.Max(.01f, windup)) : 0;
            float settle = AnimationTiming.Settle(age - windup, Mathf.Max(.08f, weapon.Recovery), 3);
            float amplitude = weapon.Motion == WeaponMotion.Recoil ? .009f : weapon.Motion == WeaponMotion.Cast ? .006f : weapon.Motion == WeaponMotion.Music ? .005f : .003f;
            return new Vector4(tension * amplitude, settle * amplitude, weapon.Mechanic == CarnivalWeapon.RibbonWheel ? settle * .008f : 0, 0);
        }
    }
}
