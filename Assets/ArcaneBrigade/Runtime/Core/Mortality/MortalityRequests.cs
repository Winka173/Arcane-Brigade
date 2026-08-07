using UnityEngine;

namespace ArcaneBrigade
{
    public readonly struct FriendMortalityEvent
    {
        public readonly int FriendId, SourceId;
        public readonly Vector2 Position;
        public readonly float Time, Health, MaxHealth, RiseAt;
        public readonly bool Hero;

        public FriendMortalityEvent(UnitState friend, int sourceId, float time, float maxHealth, float riseAt)
        {
            FriendId = friend.Id; SourceId = sourceId; Position = friend.Position;
            Time = time; Health = friend.Health; MaxHealth = maxHealth; RiseAt = riseAt;
            Hero = friend.Faction == Faction.Champion;
        }
    }

    public readonly struct FriendWaveLostEvent
    {
        public readonly float Time;
        public readonly int FallenCount, LastFriendId;
        public FriendWaveLostEvent(float time, int fallenCount, int lastFriendId)
        { Time = time; FallenCount = fallenCount; LastFriendId = lastFriendId; }
    }
}
