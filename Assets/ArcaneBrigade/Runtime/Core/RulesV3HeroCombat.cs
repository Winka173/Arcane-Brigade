using System;
using UnityEngine;
namespace ArcaneBrigade
{
    public sealed partial class CheckpointCatalog
    {
        [NonSerialized] public BondRunProgress RulesV3BondProgress;
        [NonSerialized] public HeroRulesV3RunState RulesV3HeroProgress;
    }
    public sealed partial class CombatSimulation
    {
        public HeroRulesV3Runtime HeroRulesV3 { get; private set; }
        public PersonalTraitV3Runtime TraitsV3 { get; private set; }
        public event Action<HeroRuleRequest> RulesV3HeroApplied;
        private int v3JuggleBounces;
        private struct HeroZone { public bool Active; public HeroRuleRequest Request; public float Next, Until; }
        private readonly HeroZone[] heroZones = new HeroZone[RulesV3Tuning.ConstructCapacity];
        private void StartHeroRulesV3()
        {
            Array.Clear(heroZones,0,heroZones.Length);
            v3JuggleBounces=0;
            HeroRulesV3 = new HeroRulesV3Runtime();
            HeroRulesV3.Requested += ApplyV3Hero;
            HeroRulesV3.ResetWave(Champion,units,catalog.RunWave,catalog.RulesV3HeroProgress);
            TraitsV3 = new PersonalTraitV3Runtime(HeroRulesV3.RunState);
            TraitsV3.Requested += ApplyV3Hero;
            TraitsV3.ResetWave(units);
            Mortality.Fell += request =>
            { UnitState friend=UnitWithId(request.FriendId); HeroRulesV3.OnFriendFell(friend); SkillsV3.OnFriendFell(friend); };
            Mortality.Rose += request => HeroRulesV3.OnFriendRose(UnitWithId(request.FriendId));
            SkillsV3.CastRequested += request =>
            {
                UnitState source=UnitWithId(request.SourceId);
                HeroRulesV3.OnFriendCast(request.SourceId,request.SkillId,request.Strength,v3FreeCast);
                TraitsV3.OnSkill(source,request.SkillId,request.Strength,v3FreeCast);
                if(request.SkillId=="TeaDragon") TraitsV3.OnCoals(source,request.Position,source.Definition.Weapon.Damage);
            };
        }
        private void TickHeroRulesV3(float dt)
        {
            HeroRulesV3.Tick(dt,(Champion.Position-Champion.PreviousPosition).magnitude,Bonds.SkillFillBonusFor(Champion));
            TraitsV3.Tick(dt);
            for(int i=0;i<heroZones.Length;i++)
            {
                HeroZone zone=heroZones[i]; if(!zone.Active)continue;
                if(zone.Until<=Elapsed){heroZones[i].Active=false;continue;}
                if(zone.Next>Elapsed)continue;
                heroZones[i].Next=Elapsed+RulesV3Tuning.ConstructTick;
                HeroRuleRequest r=zone.Request;
                foreach(UnitState enemy in StableUnits)
                    if(enemy.Alive && enemy.Faction==Faction.Enemy && V3HeroContains(r,enemy))
                    {
                        if(r.Action==HeroRuleAction.CarouselRing)enemy.Position=ClampPosition(r.Position+(enemy.Position-r.Position).normalized*(r.Radius+enemy.Definition.Radius),enemy.Definition.Radius);
                        else ApplyDamage(enemy,r.Value,Faction.Champion,cause:NewCause(r.SourceId).Child("hero-v3-zone"));
                    }
            }
        }
        private bool V3HeroContains(HeroRuleRequest request,UnitState unit)
        {
            if(request.FriendId!=0)return unit.Id==request.FriendId;
            if(request.Species!=null && request.Action!=HeroRuleAction.CopySkill && unit.Faction==Faction.Enemy && unit.Definition.Id!=request.Species)return false;
            return request.Shape==HeroRuleShape.Arena || CompanionSkillRuntime.Contains(
                request.Shape==HeroRuleShape.Line?SkillShape.Line:request.Shape==HeroRuleShape.Ring?SkillShape.Ring:SkillShape.Disc,
                request.Position,request.End,request.Radius,unit.Position);
        }
        private void AddHeroZone(HeroRuleRequest request)
        {
            for(int i=0;i<heroZones.Length;i++)if(!heroZones[i].Active || heroZones[i].Until<=Elapsed)
            { heroZones[i]=new HeroZone { Active=true,Request=request,Next=Elapsed,Until=Elapsed+(request.Duration>0?request.Duration:RulesV3Tuning.ConstructSeconds) };return; }
        }
        private void ApplyV3Hero(HeroRuleRequest request)
        {
            RulesV3HeroApplied?.Invoke(request);
            UnitState source=UnitWithId(request.SourceId)??Champion;
            if(request.Rule=="gravity-well-bullets")
            {
                for(int i=0;i<projectiles.Count;i++)
                {ProjectileState shot=projectiles[i];if(shot.Faction==Faction.Enemy && (shot.Position-request.Position).sqrMagnitude<=request.Radius*request.Radius)
                    shot.Velocity=Vector2.MoveTowards(shot.Velocity,(request.Position-shot.Position).normalized*shot.Velocity.magnitude,shot.Velocity.magnitude*request.Duration);}
                return;
            }
            switch(request.Action)
            {
                case HeroRuleAction.Coins: Gold=RunSession.BoundedCounter((long)Gold+request.Count); return;
                case HeroRuleAction.CopySkill:
                    if(request.Rule=="patchwork" && request.Species!=null && v3Copies.Count<RulesV3Tuning.CopyCapacity)
                        v3Copies.Enqueue(new V3Copy { Id=Champion.Id,Skill=request.Species,Strength=request.Value });
                    else if(request.FriendId!=0)QueueV3Copy(UnitWithId(request.FriendId),request.Value);
                    else foreach(UnitState friend in StableUnits)if(friend.Alive && friend.Faction==Faction.Companion && (request.Species==null || friend.Definition.Id==request.Species))QueueV3Copy(friend,request.Value);
                    return;
                case HeroRuleAction.BlockBullets: case HeroRuleAction.ReflectBullets: case HeroRuleAction.RemoveBullets:
                    int handled=0;
                    for(int i=projectiles.Count-1;i>=0;i--)
                    {
                        ProjectileState shot=projectiles[i];
                        if(shot.Faction!=Faction.Enemy || (shot.Position-request.Position).sqrMagnitude>request.Radius*request.Radius)continue;
                        if(request.Action==HeroRuleAction.ReflectBullets)
                        {
                            shot.Faction=Faction.Champion;shot.SourceUnitId=source.Id;shot.Velocity=-shot.Velocity;shot.Cause=NewCause(source.Id).Child("hero-v3-reflect");
                            if((request.Options&HeroRuleOptions.Critical)!=0){shot.Critical=true;shot.Damage*=HeroRulesV3Tuning.CritMultiplier;}
                            if((request.Options&HeroRuleOptions.SplitReflections)!=0)
                                for(int split=1;split<HeroRulesV3.ReflectedShotCopies;split++)
                                    projectiles.Add(new ProjectileState {Id=++nextId,Faction=Faction.Champion,SourceUnitId=source.Id,Position=shot.Position,PreviousPosition=shot.Position,
                                        Velocity=new Vector2(-shot.Velocity.y,shot.Velocity.x),Damage=shot.Damage,Remaining=shot.Remaining,Radius=shot.Radius,Cause=shot.Cause,Weapon=shot.Weapon});
                        }
                        else projectiles.RemoveAt(i);
                        handled++;if(request.Count>0 && handled>=request.Count)break;
                    }
                    HeroRulesV3?.OnBulletsHandled(handled);return;
                case HeroRuleAction.PetalWall: case HeroRuleAction.Signpost:
                    AddV3Wall(request.Position-Vector2.right*request.Radius,request.Position+Vector2.right*request.Radius,RulesV3Tuning.ContactPadding,request.Duration,source.Id);
                    return;
                case HeroRuleAction.SummonSprout: case HeroRuleAction.PersistentCoals: case HeroRuleAction.LightLine:
                case HeroRuleAction.Beam: case HeroRuleAction.CarouselRing:
                    if(request.Value>0 || request.Action==HeroRuleAction.CarouselRing)AddHeroZone(request);return;
                case HeroRuleAction.Fleet: case HeroRuleAction.Juggle:
                    if(request.Action==HeroRuleAction.Juggle)for(int pin=0;pin<request.Count;pin++)HeroRulesV3.OnJuggleBounce(++v3JuggleBounces);
                    foreach(UnitState enemy in StableUnits)if(enemy.Alive && enemy.Faction==Faction.Enemy && V3HeroContains(request,enemy))
                        ApplyDamage(enemy,request.Value*((request.Options&HeroRuleOptions.Critical)!=0?HeroRulesV3Tuning.CritMultiplier:1),Faction.Champion,cause:NewCause(source.Id).Child("hero-v3-fleet"));
                    return;
                case HeroRuleAction.TraitSignal:
                    if(request.Rule=="strings")return;
                    if(request.Rule=="strings-shared" && request.FriendId!=0)
                    { bool old=v3Sharing;v3Sharing=true;ApplyDamage(UnitWithId(request.FriendId),request.Value,Faction.Enemy,cause:NewCause(source.Id).Child("strings-share"));v3Sharing=old; }
                    return;
                case HeroRuleAction.Counter:
                    Bonds.OnTraitCounter(request.Rule);
                    if(request.Rule=="third-toll")foreach(UnitState friend in StableUnits)if(friend.Alive && friend.Faction==Faction.Companion && Array.IndexOf(friend.Definition.Tags,"clock")>=0){Bonds.OnCast(friend);QueueV3Copy(friend,request.Value);}
                    return;
                case HeroRuleAction.WindLane: AddV3Entity(RulesV3EntityKind.WindLane,request.Rule,request.Position,source.Id,duration:request.Duration,end:request.End,radius:request.Radius);return;
                case HeroRuleAction.SeedPod:
                    AddV3Entity(RulesV3EntityKind.Seed,request.Rule,request.Position,source.Id,duration:request.Duration,value:source.Definition.Weapon.Damage);
                    if(request.Count>0)AddV3Entity(RulesV3EntityKind.Snack,"seed-fruit",request.Position,source.Id,heal:RulesV3Tuning.SeedHeal,coins:RulesV3Tuning.SeedCoins);
                    return;
            }
            int affectedEnemies=0;
            foreach(UnitState unit in StableUnits)
            {
                if(!V3HeroContains(request,unit))continue;
                V3Status state=V3State(unit); if(state==null)continue;
                if(unit.Faction==Faction.Enemy)
                {
                    if(!unit.Alive)continue;
                    switch(request.Action)
                    {
                        case HeroRuleAction.Damage: affectedEnemies++;ApplyDamage(unit,request.Value*((request.Options&HeroRuleOptions.Critical)!=0?HeroRulesV3Tuning.CritMultiplier:1),Faction.Champion,cause:NewCause(source.Id).Child("hero-v3"));break;
                        case HeroRuleAction.Slow: if(!(EnemyRulesV3?.ImmuneToControl(unit)??false)){state.Slow=request.Value;state.SlowUntil=Elapsed+request.Duration;}break;
                        case HeroRuleAction.Stun: case HeroRuleAction.Freeze: if(!(EnemyRulesV3?.ImmuneToControl(unit)??false))Sleep(unit,request.Duration);break;
                        case HeroRuleAction.Mark: state.Mark=request.Value>0?request.Value:RulesV3Tuning.MarkBonus;state.MarkUntil=Elapsed+request.Duration;break;
                        case HeroRuleAction.Rain: unit.KitWetUntil=Elapsed+request.Duration;Bonds.OnSoak();break;
                        case HeroRuleAction.Burn: case HeroRuleAction.Poison:
                            state.Dot=source.Definition.Weapon.Damage*RulesV3Tuning.DotDamage;state.DotSource=source;state.DotUntil=Elapsed+request.Duration;break;
                        case HeroRuleAction.Push: case HeroRuleAction.Pull:
                            unit.Position=ClampPosition(Vector2.MoveTowards(unit.Position,request.Position,request.Action==HeroRuleAction.Pull?request.Value:-request.Value),unit.Definition.Radius);break;
                    }
                }
                else switch(request.Action)
                {
                    case HeroRuleAction.Heal: V3Heal(unit,MortalityRuntime.MaxHealth(unit)*request.Value);break;
                    case HeroRuleAction.Shield: Mortality.Shield(unit,request.Value,request.Duration);break;
                    case HeroRuleAction.Cleanse: V3CleanseFriend(unit);break;
                    case HeroRuleAction.ReviveSooner: Mortality.ReviveSooner(unit,request.Value);break;
                    case HeroRuleAction.RefillBar: SkillsV3.Refund(unit.Id,request.Value);break;
                    case HeroRuleAction.SkillSpeed: SkillsV3.Accelerate(unit.Id,request.Value,request.Duration);break;
                    case HeroRuleAction.DamageBuff: state.Damage=request.Value;state.DamageUntil=Elapsed+request.Duration;break;
                    case HeroRuleAction.Untargetable: state.UntouchableUntil=Elapsed+request.Duration;break;
                    case HeroRuleAction.SpeedBuff: state.MoveSpeed=request.Value;state.MoveUntil=Elapsed+request.Duration;break;
                    case HeroRuleAction.Rain: state.MoveSpeed=request.Value;state.MoveUntil=Elapsed+request.Duration;break;
                }
            }
            if(request.Action==HeroRuleAction.Damage && affectedEnemies==0 && (request.Options&HeroRuleOptions.ReturnIfMissed)!=0)
                HeroRulesV3.OnCoins(HeroRulesV3Tuning.ParcelCoins,true);
        }
    }
}
