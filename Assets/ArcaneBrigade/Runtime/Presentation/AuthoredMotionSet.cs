using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class AuthoredMotionSet : ScriptableObject
    {
        public Sprite[] Idle;
        public Sprite[] Walk;
        public Sprite[] Windup;
        public Sprite[] Release;
        public Sprite[] Recovery;
        public float ReferenceHeight => Idle != null && Idle.Length > 0 && Idle[0] != null ? Idle[0].bounds.size.y : 1;
        public Sprite Select(UnitState unit, float time, bool reduced)
        {
            if (reduced) return Idle[0];
            float windup = Mathf.Max(0.01f, unit.AttackWindup > 0 ? unit.AttackWindup : unit.Definition.Weapon.Windup);
            float age = time - unit.AttackStarted;
            if (unit.Definition.IsBoss && unit.TelegraphUntil > time)
                return Windup[Mathf.Clamp(Mathf.FloorToInt((1 - (unit.TelegraphUntil - time) / 0.85f) * Windup.Length), 0, Windup.Length - 1)];
            if (unit.PendingAttack && age >= 0 && age < windup) return Windup[Mathf.Min(Windup.Length - 1, Mathf.FloorToInt(age / windup * Windup.Length))];
            float recovery = Mathf.Max(0.06f, unit.Definition.Weapon.Recovery);
            if (age >= windup && age < windup + recovery)
            {
                float progress = (age - windup) / recovery;
                return progress < 0.6f ? Release[Mathf.Min(Release.Length - 1, Mathf.FloorToInt(progress / 0.6f * Release.Length))] : Recovery[Mathf.Min(Recovery.Length - 1, Mathf.FloorToInt((progress - 0.6f) / 0.4f * Recovery.Length))];
            }
            if (unit.Definition.IsBoss && unit.BossRecoveryUntil > time)
                return unit.BossRecoveryUntil - time > 0.32f ? Recovery[0] : Recovery[1];
            if (time - unit.LastHit >= 0 && time - unit.LastHit < 0.12f) return Recovery[0];
            bool moving = (unit.Position - unit.PreviousPosition).sqrMagnitude > 0.000001f;
            if (moving) return Walk[Mathf.FloorToInt(time * (unit.TravelMode == CompanionTravelMode.Pursuit ? 11 : 8) + unit.Id * 0.37f) % Walk.Length];
            return Mathf.Repeat(time + unit.Id * 0.73f, 4.7f) < 0.13f ? Idle[1] : Idle[0];
        }
    }
}
