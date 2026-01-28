using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class EnemyVariantState
    {
        public EnemyAffix Affix;
        public bool CastingAffix;
        public float ReleaseAt;
        public float RecoveryUntil;
        public float NextAffix;
        public float NextSpecial;
        public float TravelStart = -100;
        public float TravelEnd = -100;
        public Vector2 Origin;
        public Vector2 Target;
        public Vector2 Heading;
        public int AffixCasts;
        public int SpecialCasts;
        public int AshMeals;
        public bool BlackfireSpent;
        public bool Summoned;
        public bool ProcessionLeader;
        public int LeaderId;
        public bool LeaderLost;
        public bool CoffinBroken;
        public float DecoyUntil;
        public Vector2 DecoyPosition;
        public float AssignedAt;
        public float LastMovement;
        public bool Busy(float time) => ReleaseAt > 0 || RecoveryUntil > time || TravelEnd > time;
    }
}
