using System.Collections.Generic;
using UnityEngine;
namespace ArcaneBrigade
{
    public static class RulesV5ProjectileTuning
    {
        public const float ReturnSeconds=4, CatchRadius=.25f, CellSize=2, SpiralDegrees=10;
        public const int CrossingCapacity=4096;
    }
    public sealed partial class CombatSimulation
    {
        private struct ReturnSegment { public int Id; public Vector2 Start,End; public ProjectileState Shot; }
        private readonly Dictionary<long,ReturnSegment> v5ReturnCells=new Dictionary<long,ReturnSegment>(RulesV5ProjectileTuning.CrossingCapacity);
        // Each shot enters a board zone once; repeated frames cannot multiply its bonus again.
        private bool AdvanceV5Projectile(ProjectileState shot,float dt)
        {
            if(shot.Faction==Faction.Enemy || Bonds==null)return true;
            UnitState source=UnitWithId(shot.SourceUnitId);
            if(!shot.Returning && Bonds.ProjectileDistanceBonusFor(source)>0)shot.Velocity=Quaternion.Euler(0,0,RulesV5ProjectileTuning.SpiralDegrees*dt)*shot.Velocity;
            Vector2 end=shot.Position+shot.Velocity*dt;
            if(!shot.BondLane && (Bonds.ProjectileZonePierce(shot.Position)>0 || Bonds.ProjectileZonePierce(end)>0))
            {shot.BondLane=true;shot.ExtraPierce++;shot.Velocity*=Bonds.ProjectileZoneSpeed(shot.Position)>1?Bonds.ProjectileZoneSpeed(shot.Position):Bonds.ProjectileZoneSpeed(end);}
            if(!shot.BondTrail && (Bonds.ProjectileZoneDamage(shot.Position)>1 || Bonds.ProjectileZoneDamage(end)>1))
            {shot.BondTrail=true;shot.Damage*=Mathf.Max(Bonds.ProjectileZoneDamage(shot.Position),Bonds.ProjectileZoneDamage(end));}
            if(shot.BondReturnsRemaining>0 && !(shot.Weapon!=null && FriendlyReach.Returns(shot.Weapon)))
            {
                if(!shot.BondReturnFlight && (shot.Remaining<=dt || shot.TravelLeft>=0 && shot.TravelLeft<=shot.Velocity.magnitude*dt))
                {
                    shot.BondReturnFlight=shot.Returning=true;shot.TravelLeft=-1;shot.Remaining=RulesV5ProjectileTuning.ReturnSeconds;
                    shot.Victims.Clear();if(!shot.BondReturnDamage){shot.Damage*=Bonds.ReturningDamageScale(source);shot.BondReturnDamage=true;}
                }
                if(shot.BondReturnFlight)
                {
                    if(source==null || !source.Alive)return false;
                    Vector2 offset=source.Position-shot.Position;
                    if(offset.magnitude<=Mathf.Max(RulesV5ProjectileTuning.CatchRadius,shot.Velocity.magnitude*dt))
                    {
                        if(--shot.BondReturnsRemaining<=0)return false;
                        shot.BondReturnFlight=shot.Returning=false;shot.Position=source.Position;
                        if(shot.BondReturnDamage){shot.Damage/=Bonds.ReturningDamageScale(source);shot.BondReturnDamage=false;}
                        shot.Velocity=source.Facing.normalized*shot.Velocity.magnitude;
                        shot.TravelLeft=FriendlyReach.Flight(shot.Weapon,0);shot.Remaining=RulesV5ProjectileTuning.ReturnSeconds;
                        shot.Victims.Clear();
                    }
                    else shot.Velocity=offset.normalized*shot.Velocity.magnitude;
                }
            }
            if(shot.Returning && !shot.BondReturnDamage){shot.Damage*=Bonds.ReturningDamageScale(source);shot.BondReturnDamage=true;}
            if(shot.Returning && !shot.BondCrane)
            {
                int x=Mathf.FloorToInt(shot.Position.x/RulesV5ProjectileTuning.CellSize),y=Mathf.FloorToInt(shot.Position.y/RulesV5ProjectileTuning.CellSize);
                long key=((long)x<<32)^(uint)y;
                end=shot.Position+shot.Velocity*dt;
                if(v5ReturnCells.TryGetValue(key,out ReturnSegment other) && other.Id!=shot.Id && V3SegmentsCross(other.Start,other.End,shot.Position,end))
                {shot.BondCrane=true;shot.Remaining=0;if(other.Shot!=null){other.Shot.BondCrane=true;other.Shot.Remaining=0;}Bonds.OnReturningShotsCross(source,shot.Position);return false;}
                if(v5ReturnCells.Count<RulesV5ProjectileTuning.CrossingCapacity)v5ReturnCells[key]=new ReturnSegment{Id=shot.Id,Start=shot.Position,End=end,Shot=shot};
            }
            return true;
        }
    }
}
