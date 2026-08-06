using System;
using UnityEngine;
namespace ArcaneBrigade
{
    public enum EnemyKindV3 { CrumbSwarm, Marcher, Pillbug, Flanker, Smasher, Stitcher, Grower, Decoy, Shielder, Splitter, Blinker, SniperDoll, Leech, Bomber, Healer, Buffer, BannerBearer, Charger, Thief, Trapper, Marker, ConeBrute, EggLayer, ShieldWall, MagnetBrute, Mourner, Silencer, TimeEater, Cleanser, Ambusher, PhaseWalker, Burrower, KamikazeCandle, GravityImp, Shrieker, GluttonousMouth, Rallier, MirrorImp, Reviver, Summoner, Darkener, Binder, Drainer, Charmer, Immune, StormEye, ArmourGrower, MeteorCaller, TideBrute, FireBrute, IceWisp, StormWisp, VenomWisp }
    [Flags]
    public enum EnemyAffixV3 { None=0, Swift=1, Armored=2, Vampiric=4, Explosive=8, Shielded=16, Splitting=32, Burning=64, Frozen=128, Teleporting=256, Regenerating=512, Commanding=1024, Reflective=2048 }
    public enum EnemyBossV3 { None, PastryTyrant, ClockworkWarden, StormHerald, HollowChoir, GravityMaw, LanternQueen }
    public enum EnemyBodyTier { Small, MediumPlain, MediumSkilled, LargeSpecial }
    public enum EnemyDangerShape { Circle, Line, Cone, Ring, Arena }
    public enum EnemyRuleAction { Damage, Poison, Burn, Chill, Slow, Stun, Fear, Pull, PauseSkills, SkillFill, Cleanse, MissShots, Charm, DrainCoins, DrainPickups, MarkHero, Swallow, Darken, Banner, Chain, ChainCut, Countdown, Phase, HealEnemy, ReviveEnemy, Spawn, Despawn, ShieldEnemy, Reflect, LeechLink, ArmorBreak }
    /// <summary>Warning geometry is immutable; delayed hits resolve this geometry after WarningTime.</summary>
    public readonly struct EnemyDangerRequest
    {
        public readonly int RequestId, SourceId;
        public readonly EnemyDangerShape Shape;
        public readonly EnemyRuleAction Action;
        public readonly string Rule;
        public readonly Vector2 Position, End;
        public readonly float Radius, WarningTime, Duration, Value;
        public EnemyDangerRequest(int request,int source,EnemyDangerShape shape,EnemyRuleAction action,string rule,Vector2 position,Vector2 end,float radius,float warning,float duration,float value)
        { RequestId=request; SourceId=source; Shape=shape; Action=action; Rule=rule; Position=position; End=end; Radius=radius; WarningTime=warning; Duration=duration; Value=value; }
    }
    public readonly struct EnemyRuleRequest
    {
        public readonly EnemyRuleAction Action;
        public readonly int SourceId, TargetId, Count, GroupId;
        public readonly string Rule, Species;
        public readonly EnemyDangerShape Shape;
        public readonly Vector2 Position, End;
        public readonly float Radius, Duration, Value;
        public EnemyRuleRequest(EnemyRuleAction action,int source,string rule,Vector2 position,float radius=0,float duration=0,float value=0,int target=0,int count=0,string species=null,EnemyDangerShape shape=EnemyDangerShape.Circle,Vector2 end=default,int group=0)
        { Action=action; SourceId=source; Rule=rule; Position=position; Radius=radius; Duration=duration; Value=value; TargetId=target; Count=count; Species=species; Shape=shape; End=end; GroupId=group; }
    }
    public sealed class EnemyRuleDefinition
    {
        public EnemyKindV3 Kind; public string Id, Name, Role, Skill, Body, Telegraph; public int Night;
        public EnemyBodyTier Tier => EnemyRulesV3Catalog.Tier(Kind);
        public bool HasSkill => EnemyRulesV3Catalog.HasSkill(Kind);
    }
    public sealed class EnemyBossDefinition
    {
        public EnemyBossV3 Kind; public string Id, Name; public int FirstNight, RepeatNight;
        public float SecondPhase, FinalPhase;
    }
    public sealed class EnemyAffixDefinition
    {
        public EnemyAffixV3 Flag;
        public string Id, Name, Description;
        public EnemyAffixDefinition(EnemyAffixV3 flag,string id,string name,string description)
        {Flag=flag;Id=id;Name=name;Description=description;}
    }
}
