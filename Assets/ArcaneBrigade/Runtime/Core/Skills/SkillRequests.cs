using System;
using UnityEngine;

namespace ArcaneBrigade
{
    public enum SkillShape { Disc, Ring, Line, Cone, Species }
    public enum SkillFriendSelection { Nearest, LowestHealth, Strongest, Fallen, Danger, Hero }
    [Flags]
    public enum SkillEnemyEffect
    {
        None=0, Burn=1, Chill=2, Soak=4, Poison=8, Slow=16, Freeze=32, Stun=64, Sleep=128,
        Silence=256, Weaken=512, Blind=1024, Root=2048, Pull=4096, Push=8192, Fear=16384,
        Mark=32768, ReduceHealing=65536, Lit=131072, Taunt=262144, Charge=524288, DuskMark=1048576, AttackSlow=2097152
    }
    public enum SkillFriendEffect
    {
        None, HealFraction, HealFlat, ShieldFraction, DamageReduction, ShareDamage, ReviveSooner, ReviveNow,
        Cleanse, Reflect, ReflectHits, AttackSpeed, Damage, SkillSpeed, ProjectileSpeed, Crit, Swap,
        PullToSafety, RerollDiscount, RebirthFeather, Retaliate, BarRefund, OnBeatDamage, AbsorbProjectiles
    }
    public readonly struct SkillCastRequest
    {
        public readonly int SourceId;
        public readonly string SkillId;
        public readonly SkillShape Shape;
        public readonly Vector2 Position, End;
        public readonly float Radius, Duration, WarningTime, Strength;
        public SkillCastRequest(int source, string skill, SkillShape shape, Vector2 position, Vector2 end, float radius, float duration, float warning, float strength)
        { SourceId=source; SkillId=skill; Shape=shape; Position=position; End=end; Radius=radius; Duration=duration; WarningTime=warning; Strength=strength; }
    }
    public readonly struct SkillDamageRequest
    {
        public readonly UnitState Attacker, Target;
        public readonly string SkillId;
        public readonly float Damage;
        public readonly bool PageCopy;
        public SkillDamageRequest(UnitState attacker, UnitState target, string skill, float damage,bool pageCopy=false)
        { Attacker=attacker; Target=target; SkillId=skill; Damage=damage; PageCopy=pageCopy; }
    }
    public readonly struct SkillEnemyEffectRequest
    {
        public readonly UnitState Source, Target;
        public readonly string SkillId;
        public readonly SkillEnemyEffect Effects;
        public readonly Vector2 Origin, Direction;
        public readonly float Duration, Amount, Secondary;
        public readonly int StackCap;
        public SkillEnemyEffectRequest(UnitState source, UnitState target, string skill, SkillEnemyEffect effects, Vector2 origin, Vector2 direction, float duration, float amount, float secondary, int cap)
        { Source=source; Target=target; SkillId=skill; Effects=effects; Origin=origin; Direction=direction; Duration=duration; Amount=amount; Secondary=secondary; StackCap=cap; }
    }
    public readonly struct SkillFriendRequest
    {
        public readonly UnitState Source, Target;
        public int SourceId => Source.Id;
        public int TargetId => Target == null ? 0 : Target.Id;
        public readonly string SkillId;
        public readonly SkillFriendEffect Kind;
        public readonly float Value, Duration;
        public SkillFriendRequest(UnitState source, UnitState target, string skill, SkillFriendEffect kind, float value, float duration)
        { Source=source; Target=target; SkillId=skill; Kind=kind; Value=value; Duration=duration; }
    }
    public readonly struct SkillConstructRequest
    {
        public readonly int SourceId, TargetId, Count;
        public readonly string SkillId, Kind;
        public readonly Vector2 Position, End;
        public readonly float Radius, Duration, Value;
        public readonly bool BlocksBodies, BlocksBullets;
        public SkillConstructRequest(int source, int target, string skill, string kind, Vector2 position, Vector2 end, float radius, float duration, float value, int count, bool bodies=false, bool bullets=false)
        { SourceId=source; TargetId=target; SkillId=skill; Kind=kind; Position=position; End=end; Radius=radius; Duration=duration; Value=value; Count=count; BlocksBodies=bodies; BlocksBullets=bullets; }
    }
}
