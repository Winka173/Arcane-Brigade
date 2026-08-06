using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Section 6/7 skills use elapsed time and crowd geometry. Events are gameplay requests, never visual objects.</summary>
    public sealed class CompanionSkillRuntime
    {
        private struct BarState
        {
            public UnitState Unit;
            public CompanionSkillDefinition Skill;
            public float Fill, Speed, SpeedUntil, PauseUntil;
            public float LastDamage, PreviousDamage, RootUntil;
            public Vector2 LastDirection, PreviousDirection;
            public int Casts;
            public bool Used, FeatherUsed;
        }
        private struct ActiveSkill
        {
            public bool Active;
            public UnitState Source;
            public CompanionSkillDefinition Skill;
            public Vector2 Position, End;
            public float Next, Until, Strength, Radius;
            public int Remaining;
            public bool Trap, Wake, PageCopy;
            public UnitState HealTarget;
            public float HealFraction;
            public Vector2 SourcePosition;
        }
        private readonly BarState[] bars=new BarState[CompanionSkillTuning.MaxFriends];
        private readonly ActiveSkill[] active=new ActiveSkill[CompanionSkillTuning.MaxZones];
        private readonly int[] selected=new int[CompanionSkillTuning.MaxFriends];
        private readonly int[] grid=new int[CompanionSkillTuning.GridSize*CompanionSkillTuning.GridSize];
        private IList<UnitState> units;
        private UnitState hero;
        private int count, wave;
        private uint randomState;
        private float time;
        private CompanionSkillDefinition lastSkill;
        private float lastHeroDamage;
        private Vector2 lastHeroDirection;
        public event Action<SkillCastRequest> CastRequested;
        public event Action<SkillDamageRequest> DamageRequested;
        public event Action<SkillEnemyEffectRequest> EnemyEffectRequested;
        public event Action<SkillFriendRequest> FriendRequested;
        public event Action<SkillConstructRequest> ConstructRequested;
        public Func<UnitState,float> BondFillBonus;
        public Func<bool> HoldCasts;
        public bool PageCopyContext;
        private struct HeldCast { public UnitState Source; public CompanionSkillDefinition Skill; public float Strength; public int Number; public Vector2? Position; public bool PageCopy; }
        private readonly HeldCast[] heldCasts=new HeldCast[CompanionSkillTuning.HeldCastCapacity];
        private int heldCount;
        private Vector2? castPosition;
        public int HeldCastCount => heldCount;
        public float Time => time;
        public int CastCount { get; private set; }
        public int DroppedZones { get; private set; }

        public void ResetWave(IList<UnitState> roster, UnitState champion, int waveNumber, uint seed=1)
        {
            heldCount=0; Array.Clear(heldCasts,0,heldCasts.Length);
            units=roster; hero=champion; wave=waveNumber; count=0; time=0; PageCopyContext=false; randomState=seed==0?1:seed;
            lastSkill=null; CastCount=0; DroppedZones=0; lastHeroDamage=0;
            Array.Clear(active,0,active.Length); Array.Clear(bars,0,bars.Length);
            for(int i=0;i<units.Count && count<bars.Length;i++)
            {
                UnitState unit=units[i];
                if(unit.Faction!=Faction.Companion || unit.Definition==null) continue;
                CompanionSkillDefinition skill;
                if(CompanionSkillTable.TryGet(unit.Definition.Id,out skill)) bars[count++]=new BarState { Unit=unit, Skill=skill };
            }
        }
        public float BarFraction(int id)
        { for(int i=0;i<count;i++) if(bars[i].Unit.Id==id) return bars[i].Fill/bars[i].Skill.Tuning.Bar; return 0; }
        /// <summary>Enemy silence and time auras affect fill; hits and kills never call this.</summary>
        public void Pause(int id,float seconds)
        { for(int i=0;i<count;i++) if(bars[i].Unit.Id==id) bars[i].PauseUntil=Mathf.Max(bars[i].PauseUntil,time+seconds); }
        public void Accelerate(int id,float amount,float seconds)
        { for(int i=0;i<count;i++) if(bars[i].Unit.Id==id) { bars[i].Speed=amount; bars[i].SpeedUntil=time+seconds; } }
        public void RefundSeconds(int id,float seconds)
        { for(int i=0;i<count;i++)if(bars[i].Unit.Id==id)bars[i].Fill=Mathf.Min(bars[i].Skill.Tuning.Bar,bars[i].Fill+Mathf.Max(0,seconds)); }
        public void Refund(int id,float fraction)
        { for(int i=0;i<count;i++) if(bars[i].Unit.Id==id) bars[i].Fill=Mathf.Min(bars[i].Skill.Tuning.Bar,bars[i].Fill+bars[i].Skill.Tuning.Bar*fraction); }
        public bool IsRooted(int id)
        { for(int i=0;i<count;i++) if(bars[i].Unit.Id==id) return bars[i].RootUntil>time; return false; }
        public void RecordHeroShot(float damage) { lastHeroDamage=damage; }
        /// <summary>Attack history records ordinary weapon attacks without charging a skill bar.</summary>
        public void RecordAttack(UnitState source,float damage,Vector2 direction)
        {
            if(source==hero) { lastHeroDamage=damage; lastHeroDirection=direction; return; }
            for(int i=0;i<count;i++) if(bars[i].Unit==source)
            {
                bars[i].PreviousDamage=bars[i].LastDamage; bars[i].PreviousDirection=bars[i].LastDirection;
                bars[i].LastDamage=damage; bars[i].LastDirection=direction; return;
            }
        }
        /// <summary>Kill pickups are emitted only after combat confirms a skill kill.</summary>
        public void OnKill(UnitState source,UnitState enemy,string skillId)
        {
            CompanionSkillDefinition skill;
            if(source==null || enemy==null || !CompanionSkillTable.TryGet(skillId,out skill)) return;
            if(skillId=="BobaTadpole") Construct(source,enemy,skill,"snack",enemy.Position,enemy.Position,CompanionSkillTuning.FallbackHeal,1);
            if(skillId=="BoneXylophone") Construct(source,enemy,skill,"taunt-skull",enemy.Position,enemy.Position,skill.Tuning.Status,1);
            if(skillId=="PumpkinReaper") Construct(source,enemy,skill,"healing-pumpkin",enemy.Position,enemy.Position,skill.Tuning.Heal,1);
        }
        /// <summary>Phoenix feathers revive on collection, once per Phoenix per wave.</summary>
        public void OnFriendFell(UnitState fallen)
        {
            if(fallen==null) return;
            for(int i=0;i<count;i++)
                if(bars[i].Unit.Alive && bars[i].Skill.CompanionId=="PhoenixChick" && !bars[i].FeatherUsed)
                {
                    bars[i].FeatherUsed=true;
                    Construct(bars[i].Unit,fallen,bars[i].Skill,"rebirth-feather",fallen.Position,fallen.Position,bars[i].Skill.Tuning.Amount,1);
                }
        }
        public void Tick(float dt,float globalFillBonus=0)
        {
            if(units==null || dt<=0) return;
            time+=dt;
            if(!(HoldCasts?.Invoke()??false) && heldCount>0)
            {
                int ready=heldCount;heldCount=0;
                for(int h=0;h<ready;h++){HeldCast held=heldCasts[h];heldCasts[h]=default;if(held.Source.Alive){castPosition=held.Position;PageCopyContext=held.PageCopy;Cast(held.Source,held.Skill,held.Strength,held.Number);castPosition=null;PageCopyContext=false;}}
            }
            for(int i=0;i<count;i++)
            {
                BarState state=bars[i];
                if(!state.Unit.Alive || state.PauseUntil>time) continue;
                state.Fill+=dt*Mathf.Max(0,1+state.Unit.Definition.SkillHaste+globalFillBonus+(BondFillBonus?.Invoke(state.Unit)??0)+(state.SpeedUntil>time?state.Speed:0));
                int safety=0;
                while(state.Fill>=state.Skill.Tuning.Bar && safety++<CompanionSkillTuning.MaxCastsPerTick)
                {
                    state.Fill-=state.Skill.Tuning.Bar; state.Casts++;
                    bars[i]=state;
                    if(!state.Skill.Tuning.Once || !state.Used)
                    { Cast(state.Unit,state.Skill,1,state.Casts); state.Used=true; }
                    // A cast can accelerate this bar or refund it; preserve that update.
                    state.Speed=bars[i].Speed; state.SpeedUntil=bars[i].SpeedUntil;
                    state.RootUntil=bars[i].RootUntil;
                    state.Fill=bars[i].Fill;
                }
                bars[i]=state;
            }
            for(int i=0;i<active.Length;i++)
            {
                ActiveSkill action=active[i];
                if(!action.Active || action.Next>time) continue;
                if(!action.Source.Alive && action.HealTarget==null) { active[i].Active=false; continue; }
                if(action.Trap && time>action.Until) { active[i].Active=false; continue; }
                if(action.Trap && !AnyEnemy(action.Position,action.Radius)) continue;
                if(action.HealTarget!=null)
                {
                    if(action.HealTarget.Alive) Friend(action.Source,action.HealTarget,action.Skill,SkillFriendEffect.HealFraction,action.HealFraction,0);
                }
                else
                {
                    if(action.Skill.Mechanic==SkillMechanic.Orbit || action.Skill.Mechanic==SkillMechanic.Transform || action.Skill.Mechanic==SkillMechanic.Tether)
                    {
                        Vector2 moved=action.Source.Position-action.SourcePosition;
                        action.Position+=moved; action.End+=moved; action.SourcePosition=action.Source.Position;
                        if(action.Skill.Mechanic==SkillMechanic.Tether) action.End=hero.Position;
                    }
                    if(action.Skill.CompanionId=="StormKitten") action.Position=Vector2.MoveTowards(action.Position,Cluster(action.Position),action.Skill.Tuning.Growth);
                    bool previousPage=PageCopyContext;PageCopyContext=action.PageCopy;
                    Execute(action.Source,action.Skill,action.Position,action.End,action.Radius,action.Strength,action.Wake);
                    PageCopyContext=previousPage;
                }
                action.Remaining--; action.Next+=CompanionSkillTuning.TickInterval;
                action.Radius+=action.Skill.Tuning.Growth;
                action.Active=action.Remaining>0 && action.Next<=action.Until;
                active[i]=action;
            }
        }
        /// <summary>Explicit free copies are permitted; they do not add hit-based bar charge or recurse.</summary>
        public void CastFreeAt(int sourceId,float strength,Vector2 position)
        { Vector2? previous=castPosition;castPosition=position;try{CastFree(sourceId,strength);}finally{castPosition=previous;} }
        public void CastCopyAt(UnitState source,string skillId,float strength,Vector2 position)
        { Vector2? previous=castPosition;castPosition=position;try{CastCopy(source,skillId,strength);}finally{castPosition=previous;} }
        public void CastFree(int sourceId,float strength)
        { for(int i=0;i<count;i++) if(bars[i].Unit.Id==sourceId && bars[i].Unit.Alive) Cast(bars[i].Unit,bars[i].Skill,strength,bars[i].Casts); }
        public void CastCopy(UnitState source,string skillId,float strength)
        { if(source!=null && source.Alive && CompanionSkillTable.TryGet(skillId,out CompanionSkillDefinition skill))Cast(source,skill,strength,1); }

        private void Cast(UnitState source,CompanionSkillDefinition skill,float strength,int castNumber)
        {
            if(HoldCasts?.Invoke()??false)
            {
                if(heldCount<heldCasts.Length)heldCasts[heldCount++]=new HeldCast{Source=source,Skill=skill,Strength=strength,Number=castNumber,Position=castPosition,PageCopy=PageCopyContext};
                return;
            }
            SkillProfile p=skill.Tuning;
            Vector2 origin=source.Position, center=castPosition??Cluster(origin), direction=castPosition.HasValue?(castPosition.Value-origin).normalized:source.Facing;
            if(direction.sqrMagnitude<CompanionSkillTuning.Epsilon) direction=Vector2.right;
            Vector2 end=origin+direction.normalized*p.Length;
            if(skill.Mechanic==SkillMechanic.Line || skill.Mechanic==SkillMechanic.Dash || skill.Mechanic==SkillMechanic.Channel) center=origin;
            if(p.Self || skill.Mechanic==SkillMechanic.Orbit || p.Shape==SkillShape.Ring || skill.Mechanic==SkillMechanic.Pull || (skill.Protection && skill.CompanionId!="AnvilOx")) center=origin;
            if(skill.Mechanic==SkillMechanic.Tether) { center=origin; end=hero.Position; }
            CastCount++;
            CastRequested?.Invoke(new SkillCastRequest(source.Id,skill.CompanionId,Shape(skill),center,end,p.Radius,p.Duration,p.Warning,strength));
            if(skill.Mechanic==SkillMechanic.Copy && skill.CompanionId=="EndlessIndex" && lastSkill!=null && lastSkill.Mechanic!=SkillMechanic.Copy)
            { Cast(source,lastSkill,strength*CompanionSkillTuning.CopyStrength,castNumber); return; }
            if(skill.Mechanic!=SkillMechanic.Copy) lastSkill=skill;
            if(skill.CompanionId=="GrandSprout")
                for(int i=0;i<count;i++) if(bars[i].Unit==source) bars[i].RootUntil=time+CompanionSkillTuning.RootDuration;
            if(skill.CompanionId=="ChoirCanary")
            {
                int friend=-1; float best=float.PositiveInfinity;
                for(int i=0;i<count;i++) if(bars[i].Unit!=source && bars[i].Unit.Alive && bars[i].LastDamage>0)
                { float distance=(bars[i].Unit.Position-source.Position).sqrMagnitude; if(distance<best) { best=distance; friend=i; } }
                if(friend>=0)
                {
                    Replay(source,skill,bars[friend].LastDamage,bars[friend].LastDirection,strength);
                    Replay(source,skill,bars[friend].PreviousDamage,bars[friend].PreviousDirection,strength);
                }
                return;
            }
            if(skill.CompanionId=="ReelOwl" && lastHeroDamage>0)
            {
                for(int i=0;i<p.Repeats;i++) Replay(source,skill,lastHeroDamage,lastHeroDirection,strength);
                return;
            }
            if(skill.CompanionId=="SaddleMoth") SwapFriend(source,skill);
            if(skill.Mechanic==SkillMechanic.Swap)
            {
                Vector2 before=hero.Position; hero.Position=source.Position; source.Position=before;
                Execute(source,skill,origin,end,p.Radius,strength,false);
                center=source.Position;
            }
            if(skill.CompanionId=="DreamCompass")
            { Construct(source,null,skill,"coins",hero.Position,end,Mathf.Min(wave,p.Economy),1); return; }
            if(skill.CompanionId=="CoralTeacup") Friend(source,hero,skill,SkillFriendEffect.RerollDiscount,p.Economy,0);
            if(skill.Mechanic==SkillMechanic.Random)
            {
                int roll=Roll(p.RandomSides)+1;
                if(skill.CompanionId=="CitrusToad")
                {
                    if(roll==p.RandomSides) Construct(source,null,skill,"coins",center,end,p.Economy+1,1);
                    else if(roll==1) Execute(source,skill,center,end,p.Radius,strength,false,SkillEnemyEffect.Slow,0);
                    else if(roll==2) Execute(source,skill,center,end,p.Radius,strength,false,SkillEnemyEffect.Blind,0);
                    else Execute(source,skill,center,end,p.Radius,strength*CompanionSkillTuning.CitrusBurst,false);
                    return;
                }
                strength*=roll*CompanionSkillTuning.RandomUnitDamage;
                if(skill.CompanionId=="DiceCrab" && roll==p.RandomSides) Execute(source,skill,center,end,p.Radius,strength,false);
            }
            if(skill.CompanionId=="RainbowBell" && castNumber%p.Every!=0) return;
            Protect(source,skill,center,end,strength,castNumber);
            if(skill.Mechanic==SkillMechanic.Dash) source.Position=end;
            if(IsWall(skill.CompanionId))
            {
                Construct(source,null,skill,"wall",origin,end,0,p.Friends,skill.CompanionId=="FossilArmadillo" || skill.CompanionId=="CrayonGremlin");
                return;
            }
            if(skill.Mechanic==SkillMechanic.Transform || skill.Mechanic==SkillMechanic.Summon || skill.Mechanic==SkillMechanic.Echo)
                Construct(source,null,skill,skill.Mechanic.ToString(),center,end,p.Damage*strength,p.Repeats);
            if(skill.Mechanic==SkillMechanic.Trap)
                Queue(source,skill,center,end,p.Radius,strength,time,1,true);
            else if(p.Warning>0) Queue(source,skill,center,end,p.Radius,strength,time+p.Warning,1,false);
            else Execute(source,skill,center,end,p.Radius,strength,false);
            bool persistent=skill.Mechanic==SkillMechanic.Zone || skill.Mechanic==SkillMechanic.Channel || skill.Mechanic==SkillMechanic.Orbit || skill.Mechanic==SkillMechanic.Tether || skill.Mechanic==SkillMechanic.Transform || skill.Mechanic==SkillMechanic.Summon;
            if(persistent && p.Duration>CompanionSkillTuning.TickInterval)
                Queue(source,skill,center,end,p.Radius,strength,time+CompanionSkillTuning.TickInterval,Mathf.CeilToInt(p.Duration/CompanionSkillTuning.TickInterval)-1,false);
            if(p.WakeDamage>0) Queue(source,skill,center,end,p.Radius,strength*p.WakeDamage,time+p.Status,1,false,true);
        }
        private void Queue(UnitState source,CompanionSkillDefinition skill,Vector2 position,Vector2 end,float radius,float strength,float next,int repeats,bool trap,bool wake=false)
        {
            for(int i=0;i<active.Length;i++) if(!active[i].Active)
            {
                active[i]=new ActiveSkill { Active=true,Source=source,SourcePosition=source.Position,Skill=skill,Position=position,End=end,Radius=radius,Strength=strength,Next=next,Until=next+Mathf.Max(skill.Tuning.Duration,repeats*CompanionSkillTuning.TickInterval),Remaining=repeats,Trap=trap,Wake=wake,PageCopy=PageCopyContext };
                return;
            }
            DroppedZones++;
        }
        private void Execute(UnitState source,CompanionSkillDefinition skill,Vector2 position,Vector2 end,float radius,float strength,bool wake,SkillEnemyEffect? effects=null,float? damageOverride=null)
        {
            SkillProfile p=skill.Tuning;
            SkillShape shape=Shape(skill);
            string species=null;
            if(shape==SkillShape.Species)
            {
                for(int i=0;i<units.Count;i++) if(IsEnemy(units[i]) && (units[i].Position-position).sqrMagnitude<=radius*radius)
                { species=units[i].Definition.Id; break; }
            }
            float weapon=source.Definition.Weapon==null?0:source.Definition.Weapon.Damage;
            if(skill.CompanionId=="ReelOwl" && lastHeroDamage>0) weapon=lastHeroDamage;
            int hits=0;
            for(int i=0;i<units.Count;i++)
            {
                UnitState target=units[i];
                if(!IsEnemy(target)) continue;
                if(shape==SkillShape.Species ? target.Definition.Id!=species : !Contains(shape,position,end,radius,target.Position)) continue;
                hits++;
                SkillEnemyEffect enemy=effects??p.Enemy;
                if(enemy!=SkillEnemyEffect.None && !wake)
                    EnemyEffectRequested?.Invoke(new SkillEnemyEffectRequest(source,target,skill.CompanionId,enemy,position,(target.Position-position).normalized,p.Status,enemy==SkillEnemyEffect.Blind?CompanionSkillTuning.CitrusBlind:p.Amount,p.Secondary,p.StackCap));
                float damage=damageOverride??p.Damage;
                if(damage>0)
                    DamageRequested?.Invoke(new SkillDamageRequest(source,target,skill.CompanionId,weapon*damage*strength*(p.Duration>0?1:p.Repeats),PageCopyContext));
                if(skill.CompanionId=="CourtHerald") Construct(source,target,skill,"coins",target.Position,end,p.Economy,1);
            }
            if(skill.CompanionId=="TeaDragon" && hits>0) Friend(source,hero,skill,SkillFriendEffect.HealFlat,hits*p.FlatHeal*strength,0);
            if(skill.CompanionId=="BakerMouse" && hits>=p.Repeats+1) Friend(source,hero,skill,SkillFriendEffect.HealFraction,p.Heal*strength,0);
        }
        private void Replay(UnitState source,CompanionSkillDefinition skill,float damage,Vector2 direction,float strength)
        {
            if(damage<=0) return;
            if(direction.sqrMagnitude<CompanionSkillTuning.Epsilon) direction=source.Facing;
            Vector2 end=source.Position+direction.normalized*skill.Tuning.Length;
            for(int i=0;i<units.Count;i++)
                if(IsEnemy(units[i]) && Contains(SkillShape.Line,source.Position,end,skill.Tuning.Radius,units[i].Position))
                    DamageRequested?.Invoke(new SkillDamageRequest(source,units[i],skill.CompanionId,damage*strength,PageCopyContext));
        }
        private void Protect(UnitState source,CompanionSkillDefinition skill,Vector2 position,Vector2 end,float strength,int casts)
        {
            SkillProfile p=skill.Tuning;
            int n=SelectFriends(source,p);
            for(int i=0;i<n;i++)
            {
                UnitState target=units[selected[i]];
                if(p.Buff!=SkillFriendEffect.None && p.Buff!=SkillFriendEffect.RebirthFeather)
                {
                    Friend(source,target,skill,p.Buff,p.Amount*strength,p.Duration>0?p.Duration:CompanionSkillTuning.DefaultShieldDuration);
                    if(p.Buff==SkillFriendEffect.SkillSpeed) Accelerate(target.Id,p.Amount*strength,p.Duration);
                }
                if(p.Heal>0 && skill.CompanionId!="PumpkinReaper" && skill.CompanionId!="BakerMouse" && skill.CompanionId!="SundaeGolem" && (p.Every<=1 || casts%p.Every==0))
                    Heal(source,target,skill,p.Heal*strength*(skill.CompanionId=="HarpSwan"?p.Duration:1),p.Duration);
                if(p.Cleanse) Friend(source,target,skill,SkillFriendEffect.Cleanse,0,0);
                if(skill.CompanionId=="SpindleWren") Friend(source,target,skill,SkillFriendEffect.ProjectileSpeed,p.Secondary,p.Duration);
                if(skill.CompanionId=="LittleSprout") Friend(source,target,skill,SkillFriendEffect.Crit,p.Secondary,p.Duration);
                if(skill.CompanionId=="BristleBall") Friend(source,target,skill,SkillFriendEffect.Reflect,p.Secondary,p.Duration);
                if(skill.CompanionId=="LifebuoyHippo") Friend(source,target,skill,SkillFriendEffect.PullToSafety,0,0);
            }
            if(skill.CompanionId=="SundaeGolem" && casts%p.Every==0)Construct(source,hero,skill,"cherry",source.Position,end,p.Heal*strength,1);
            if(p.Revive>0)
                for(int i=0;i<units.Count;i++)
                    if(units[i].Faction==Faction.Companion && !units[i].Alive && (units[i].Position-source.Position).sqrMagnitude<=p.Radius*p.Radius)
                    { Friend(source,units[i],skill,SkillFriendEffect.ReviveSooner,p.Revive,0); break; }
        }
        private void Heal(UnitState source,UnitState target,CompanionSkillDefinition skill,float fraction,float duration)
        {
            int ticks=Mathf.Max(1,Mathf.CeilToInt(duration/CompanionSkillTuning.TickInterval));
            Friend(source,target,skill,SkillFriendEffect.HealFraction,fraction/ticks,0);
            if(ticks<=1) return;
            for(int i=0;i<active.Length;i++) if(!active[i].Active)
            {
                active[i]=new ActiveSkill { Active=true, Source=source, Skill=skill, HealTarget=target, HealFraction=fraction/ticks,
                    Next=time+CompanionSkillTuning.TickInterval, Until=time+duration, Remaining=ticks-1 };
                return;
            }
            DroppedZones++;
        }
        private int SelectFriends(UnitState source,SkillProfile p)
        {
            int n=0;
            for(int i=0;i<units.Count;i++)
            {
                UnitState u=units[i];
                if(u.Faction==Faction.Enemy || (p.Self && u!=source) || (p.Selection==SkillFriendSelection.Hero && u!=hero) || (p.Selection==SkillFriendSelection.Fallen?u.Alive:!u.Alive)) continue;
                if((u.Position-source.Position).sqrMagnitude>p.Radius*p.Radius && p.Friends==CompanionSkillTuning.MaxFriends) continue;
                int at=n;
                float score=FriendScore(u,source,p.Selection);
                for(int j=0;j<n;j++) if(score<FriendScore(units[selected[j]],source,p.Selection)) { at=j; break; }
                int limit=Mathf.Min(p.Friends,selected.Length);
                if(at>=limit) continue;
                for(int j=Mathf.Min(n,limit-1);j>at;j--) selected[j]=selected[j-1];
                selected[at]=i; n=Mathf.Min(n+1,limit);
            }
            return n;
        }
        private float FriendScore(UnitState unit,UnitState source,SkillFriendSelection selection)
        {
            if(selection==SkillFriendSelection.LowestHealth) return unit.Health/Mathf.Max(CompanionSkillTuning.Epsilon,unit.Definition.Health);
            if(selection==SkillFriendSelection.Strongest) return unit.Definition.Weapon==null?0:-unit.Definition.Weapon.Damage;
            if(selection==SkillFriendSelection.Danger) return (unit.Position-Cluster(unit.Position)).sqrMagnitude;
            return (unit.Position-source.Position).sqrMagnitude;
        }
        private void SwapFriend(UnitState source,CompanionSkillDefinition skill)
        {
            if(SelectFriends(source,skill.Tuning)==0) return;
            UnitState target=units[selected[0]]; Vector2 before=target.Position; target.Position=source.Position; source.Position=before;
            Friend(source,target,skill,SkillFriendEffect.Swap,0,0);
        }
        private void Friend(UnitState source,UnitState target,CompanionSkillDefinition skill,SkillFriendEffect kind,float value,float duration)
        { FriendRequested?.Invoke(new SkillFriendRequest(source,target,skill.CompanionId,kind,value,duration)); }
        private void Construct(UnitState source,UnitState target,CompanionSkillDefinition skill,string kind,Vector2 position,Vector2 end,float value,int number,bool bodies=false)
        { ConstructRequested?.Invoke(new SkillConstructRequest(source.Id,target==null?0:target.Id,skill.CompanionId,kind,position,end,skill.Tuning.Radius,skill.Tuning.Duration,value,number,bodies,kind=="wall")); }
        private static bool IsWall(string id) => id=="PaperInventor" || id=="FossilArmadillo" || id=="TowerRook" || id=="CrayonGremlin";
        private static bool IsEnemy(UnitState unit) => unit.Alive && unit.Faction==Faction.Enemy && unit.Definition!=null;
        private bool AnyEnemy(Vector2 position,float radius)
        { for(int i=0;i<units.Count;i++) if(IsEnemy(units[i]) && (units[i].Position-position).sqrMagnitude<=radius*radius) return true; return false; }
        private int Roll(int sides)
        { randomState^=randomState<<13; randomState^=randomState>>17; randomState^=randomState<<5; return (int)(randomState%(uint)Mathf.Max(1,sides)); }
        private Vector2 Cluster(Vector2 fallback)
        {
            Array.Clear(grid,0,grid.Length);
            int best=-1, highest=0;
            for(int i=0;i<units.Count;i++) if(IsEnemy(units[i]))
            {
                Vector2 p=units[i].Position;
                int x=Mathf.Clamp(Mathf.FloorToInt(p.x/CompanionSkillTuning.CellSize)+CompanionSkillTuning.GridHalf,0,CompanionSkillTuning.GridSize-1);
                int y=Mathf.Clamp(Mathf.FloorToInt(p.y/CompanionSkillTuning.CellSize)+CompanionSkillTuning.GridHalf,0,CompanionSkillTuning.GridSize-1);
                int cell=y*CompanionSkillTuning.GridSize+x;
                if(++grid[cell]>highest) { highest=grid[cell]; best=cell; }
            }
            if(best<0) return fallback;
            return new Vector2(best%CompanionSkillTuning.GridSize-CompanionSkillTuning.GridHalf+.5f,best/CompanionSkillTuning.GridSize-CompanionSkillTuning.GridHalf+.5f)*CompanionSkillTuning.CellSize;
        }
        public static SkillShape Shape(CompanionSkillDefinition skill)
        {
            if(skill.Tuning.Shape!=SkillShape.Disc) return skill.Tuning.Shape;
            if(skill.Mechanic==SkillMechanic.Line || skill.Mechanic==SkillMechanic.Dash || skill.Mechanic==SkillMechanic.Tether || skill.Mechanic==SkillMechanic.Channel) return SkillShape.Line;
            return SkillShape.Disc;
        }
        public static bool Contains(SkillShape shape,Vector2 position,Vector2 end,float radius,Vector2 point)
        {
            Vector2 offset=point-position;
            if(shape==SkillShape.Line)
            {
                Vector2 line=end-position;
                float t=line.sqrMagnitude>CompanionSkillTuning.Epsilon?Mathf.Clamp01(Vector2.Dot(offset,line)/line.sqrMagnitude):0;
                return (point-position-line*t).sqrMagnitude<=CompanionSkillTuning.LineHalfWidth*CompanionSkillTuning.LineHalfWidth;
            }
            if(offset.sqrMagnitude>radius*radius) return false;
            if(shape==SkillShape.Cone) return offset.sqrMagnitude<CompanionSkillTuning.Epsilon || Vector2.Dot(offset.normalized,(end-position).normalized)>=CompanionSkillTuning.ConeCosine;
            return shape!=SkillShape.Ring || offset.sqrMagnitude>=radius*radius*CompanionSkillTuning.RingInnerFraction*CompanionSkillTuning.RingInnerFraction;
        }
    }
}
