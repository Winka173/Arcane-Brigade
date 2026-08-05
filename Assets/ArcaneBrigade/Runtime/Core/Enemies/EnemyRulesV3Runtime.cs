using System;
using System.Collections.Generic;
using UnityEngine;
namespace ArcaneBrigade
{
    /// <summary>Warnings and area effects are pooled; incoming hits read cached aura values in constant time.</summary>
    public sealed class EnemyRulesV3Runtime
    {
        private sealed class State
        {
            public UnitState Unit;
            public EnemyRuleDefinition Row;
            public EnemyBossDefinition Boss;
            public EnemyAffixV3 Affixes;
            public float Next, Shield, SpeedBonus, SpeedUntil, DamageBonus, ArmorUntil, ImmuneUntil;
            public float ChargeUntil, DizzyUntil, LastDamage, NextTeleport, Countdown, DiedAt, StageAt;
            public Vector2 From, To;
            public int Attacks, Phase, Generation, ParentId, GroupId, Stages, Stolen, SingerTotal, SingersAlive;
            public float FirstSingerDeath, LastSingerDeath;
            public bool Fell, Rallied, Reflected, ChainCut, Singer, CrumbGiant, SingerDeathSeen;
            public int LeechId, BinderId;
            public float AuraSpeed, AuraDamage, AuraShield, AuraArmor, DamageSinceAura;
        }
        private struct Pending
        {
            public bool Active;
            public EnemyDangerRequest Warning;
            public float At, Until;
            public bool Repeat;
            public string Species;
            public int Count, Group;
        }
        private sealed class FriendVenom { public UnitState Unit;public float Until;public int Source; }
        private IList<UnitState> units;
        private UnitState hero;
        private readonly Dictionary<int,State> byId=new Dictionary<int,State>();
        private readonly List<State> states=new List<State>();
        private readonly List<UnitState> friends=new List<UnitState>(MortalityTuning.TeamCapacity);
        private readonly Dictionary<int,List<State>> leeches=new Dictionary<int,List<State>>();
        private readonly Dictionary<int,FriendVenom> venom=new Dictionary<int,FriendVenom>();
        private readonly Pending[] pending=new Pending[EnemyRulesV3Tuning.MaxWarnings];
        private float time, auraAt, markUntil;
        private int wave, nextWarning;
        private uint random;
        public event Action<EnemyDangerRequest> DangerRequested;
        public event Action<EnemyRuleRequest> Requested;
        public float Time => time;
        public int DroppedWarnings { get; private set; }

        public void ResetWave(IList<UnitState> all,UnitState champion,int night,uint seed=1)
        {
            units=all; hero=champion; wave=night; time=auraAt=markUntil=0; nextWarning=0;
            random=seed==0?1:seed; DroppedWarnings=0; states.Clear(); byId.Clear(); friends.Clear();leeches.Clear();venom.Clear();Array.Clear(pending,0,pending.Length);
            if(all!=null)for(int i=0;i<all.Count;i++){RegisterEnemy(all[i]);RegisterFriend(all[i]);}
        }
        public bool Recognizes(UnitState unit) => unit!=null && byId.ContainsKey(unit.Id);
        public EnemyAffixV3 Affixes(UnitState unit) => Get(unit)?.Affixes??EnemyAffixV3.None;
        private State Get(UnitState unit) => unit!=null && byId.TryGetValue(unit.Id,out State state)?state:null;
        public void RegisterEnemy(UnitState unit)
        {
            if(unit==null || unit.Faction!=Faction.Enemy || byId.ContainsKey(unit.Id))return;
            EnemyRuleDefinition row=EnemyRulesV3Catalog.Find(unit.Definition.Id);
            EnemyBossDefinition boss=EnemyRulesV3Catalog.FindBoss(unit.Definition.Id);
            if(row==null && boss==null)return;
            var state=new State { Unit=unit, Row=row, Boss=boss, Next=time+Period(row), StageAt=time+EnemyRulesV3Tuning.StageSeconds, LastDamage=-EnemyRulesV3Tuning.OutOfCombat };
            byId.Add(unit.Id,state); states.Add(state);
            if(row!=null && (row.Kind==EnemyKindV3.CrumbSwarm || row.Kind==EnemyKindV3.Decoy)) unit.Health=EnemyRulesV3Tuning.OneHitHealth;
            if(unit.Elite)AssignAffixes(unit,ChooseAffixes());
            if(row!=null && row.Kind==EnemyKindV3.Flanker && hero!=null)
            { Vector2 back=hero.Facing.sqrMagnitude>EnemyRulesV3Tuning.Epsilon?-hero.Facing.normalized:Vector2.left; unit.Position=hero.Position+back*EnemyRulesV3Tuning.BehindDistance; Emit(EnemyRuleAction.Phase,state,"flank",unit.Position); }
        }
        public void RegisterSpawned(UnitState unit,int parentId,string rule,int groupId=0)
        {
            RegisterEnemy(unit); State child=Get(unit); if(child==null)return;
            child.ParentId=parentId; child.GroupId=groupId;
            State parent=byId.TryGetValue(parentId,out State found)?found:null;
            if(rule=="split" && parent!=null) child.Generation=parent.Generation+1;
            if(rule=="choir-singers")
            {child.Singer=true;if(parent!=null){parent.SingerTotal++;parent.SingersAlive++;}}
            if(rule=="warden-split" && parent!=null)
            { child.Phase=parent.Phase; child.BinderId=parentId; parent.BinderId=unit.Id; child.ChainCut=parent.ChainCut; }
        }
        public void RegisterFriend(UnitState unit)
        {
            if(unit==null || unit.Faction==Faction.Enemy || leeches.ContainsKey(unit.Id))return;
            friends.Add(unit);leeches.Add(unit.Id,new List<State>());venom.Add(unit.Id,new FriendVenom {Unit=unit});
        }
        public int SpawnPackCount(UnitDefinition definition)
        { return EnemyRulesV3Catalog.Find(definition?.Id)?.Kind==EnemyKindV3.CrumbSwarm?EnemyRulesV3Tuning.CrumbPackMin+Roll(EnemyRulesV3Tuning.CrumbPackMax-EnemyRulesV3Tuning.CrumbPackMin+1):1; }
        public void AssignAffixes(UnitState unit,EnemyAffixV3 flags)
        {
            State state=Get(unit); if(state==null)return;
            EnemyAffixV3 accepted=EnemyAffixV3.None; int count=0;
            for(int i=0;i<EnemyRulesV3Tuning.AffixCount && count<EnemyRulesV3Tuning.MaxAffixes;i++)
            { EnemyAffixV3 bit=(EnemyAffixV3)(1<<i); if((flags&bit)!=0){accepted|=bit;count++;} }
            state.Affixes=accepted;
            if((accepted&EnemyAffixV3.Shielded)!=0)state.Shield=MaxHealth(unit)*EnemyRulesV3Tuning.BaseShield;
        }
        private EnemyAffixV3 ChooseAffixes()
        {
            EnemyAffixV3 result=EnemyAffixV3.None; int wanted=1+Roll(EnemyRulesV3Tuning.MaxAffixes);
            while(wanted>0){ EnemyAffixV3 next=(EnemyAffixV3)(1<<Roll(EnemyRulesV3Tuning.AffixCount)); if((result&next)==0){result|=next;wanted--;} }
            return result;
        }
        private int Roll(int limit) { random^=random<<13;random^=random>>17;random^=random<<5;return (int)(random%(uint)limit); }
        public float MaxHealth(UnitState unit)
        {
            State state=Get(unit);
            if(state!=null && state.CrumbGiant)return EnemyRulesV3Tuning.DefaultHealth;
            return Mathf.Max(EnemyRulesV3Tuning.OneHitHealth,unit.Definition.Health)*(1+(state==null?0:state.Stages)*EnemyRulesV3Tuning.GrowHealth);
        }
        private static bool Has(State state,EnemyAffixV3 flag) => (state.Affixes&flag)!=0;
        private static float Period(EnemyRuleDefinition row)
        { return row!=null && row.Kind==EnemyKindV3.Summoner?EnemyRulesV3Tuning.SummonSeconds:EnemyRulesV3Tuning.DefaultPeriod; }

        public void Tick(float dt)
        {
            if(units==null || float.IsNaN(dt) || float.IsInfinity(dt) || dt<=0)return;
            time+=dt;
            if(time>=auraAt){auraAt=time+EnemyRulesV3Tuning.AuraTick;RefreshAuras();}
            int count=states.Count;
            for(int i=0;i<count;i++)
            {
                State state=states[i]; UnitState unit=state.Unit;
                if(!unit.Alive){ReturnSinger(state);continue;}
                if(unit.Elite && state.Affixes==EnemyAffixV3.None)AssignAffixes(unit,ChooseAffixes());
                if(state.Boss!=null){if(!state.Singer)BossTick(state);continue;}
                if(Has(state,EnemyAffixV3.Regenerating)&&time-state.LastDamage>=EnemyRulesV3Tuning.OutOfCombat)
                    unit.Health=Mathf.Min(MaxHealth(unit),unit.Health+MaxHealth(unit)*EnemyRulesV3Tuning.Regeneration*dt);
                if(Has(state,EnemyAffixV3.Teleporting)&&time>=state.NextTeleport)
                { state.NextTeleport=time+EnemyRulesV3Tuning.BlinkSeconds; UnitState weak=SelectFriend(unit.Position,true); if(weak!=null)unit.Position=weak.Position; }
                if(state.Row.Kind==EnemyKindV3.Grower && time>=state.StageAt)
                { state.StageAt=time+EnemyRulesV3Tuning.StageSeconds; state.Stages++; unit.Health+=unit.Definition.Health*EnemyRulesV3Tuning.GrowHealth; }
                if(state.Row.Kind==EnemyKindV3.Rallier && !state.Rallied && unit.Health/MaxHealth(unit)<EnemyRulesV3Tuning.LowHealth)
                { state.Rallied=true;ShieldArea(state,EnemyRulesV3Tuning.GroupRadius,EnemyRulesV3Tuning.RallierShield); }
                if(time>=state.Next && state.DizzyUntil<=time)
                { state.Next=time+Period(state.Row); Begin(state); }
            }
            for(int i=0;i<pending.Length;i++)
            {
                Pending action=pending[i]; if(!action.Active || action.At>time)continue;
                State source=byId.TryGetValue(action.Warning.SourceId,out State value)?value:null;
                if(source!=null && ((!source.Unit.Alive && action.Warning.Rule!="explosive" && action.Warning.Rule!="egg" && !action.Repeat) || (action.Warning.Rule=="final-countdown" && source.ChainCut)))
                { pending[i].Active=false;continue; }
                EnemyDangerRequest warning=action.Warning;
                Emit(new EnemyRuleRequest(warning.Action,warning.SourceId,warning.Rule,warning.Position,warning.Radius,warning.Duration,warning.Value,count:action.Count,species:action.Species,shape:warning.Shape,end:warning.End,group:action.Group));
                if(warning.Rule=="venom-ring")
                    for(int friendIndex=0;friendIndex<friends.Count;friendIndex++)
                    {UnitState friend=friends[friendIndex];if(friend.Alive && Contains(warning.Shape,warning.Position,warning.End,warning.Radius,friend.Position)){FriendVenom poison=venom[friend.Id];poison.Until=time+warning.Duration;poison.Source=warning.SourceId;}}
                if(warning.Rule=="pillbug-charge" || warning.Rule=="charger-charge")
                { if(source!=null){source.From=warning.Position;source.To=warning.End;source.ChargeUntil=time+EnemyRulesV3Tuning.Dizzy;source.DizzyUntil=source.ChargeUntil+EnemyRulesV3Tuning.Dizzy;} }
                action.At+=EnemyRulesV3Tuning.DotTick; action.Active=action.Repeat && action.At<=action.Until;
                pending[i]=action;
            }
        }
        private void RefreshAuras()
        {
            SpreadVenom();
            foreach(var entry in leeches)entry.Value.Clear();
            for(int i=0;i<states.Count;i++) { State state=states[i];state.AuraSpeed=state.AuraDamage=state.AuraShield=state.AuraArmor=0; }
            int count=states.Count;
            for(int i=0;i<count;i++)
            {
                State source=states[i]; if(!source.Unit.Alive)continue;
                if(source.Row!=null && source.Row.Kind==EnemyKindV3.CrumbSwarm && !source.CrumbGiant)TryMerge(source);
                for(int j=0;j<count;j++)
                {
                    State target=states[j]; if(!target.Unit.Alive)continue;
                    float square=(target.Unit.Position-source.Unit.Position).sqrMagnitude;
                    if(Has(source,EnemyAffixV3.Commanding) && square<=EnemyRulesV3Tuning.BannerRadius*EnemyRulesV3Tuning.BannerRadius)target.AuraDamage+=EnemyRulesV3Tuning.Commanding;
                    if(Has(source,EnemyAffixV3.Vampiric) && square<=EnemyRulesV3Tuning.GroupRadius*EnemyRulesV3Tuning.GroupRadius)
                        source.Unit.Health=Mathf.Min(MaxHealth(source.Unit),source.Unit.Health+target.DamageSinceAura*EnemyRulesV3Tuning.Vampire);
                    if(source.Row==null)continue;
                    switch(source.Row.Kind)
                    {
                        case EnemyKindV3.Marcher:
                            if(source!=target && square<=EnemyRulesV3Tuning.RankRadius*EnemyRulesV3Tuning.RankRadius)target.AuraSpeed=Mathf.Min(EnemyRulesV3Tuning.MarcherCap,target.AuraSpeed+EnemyRulesV3Tuning.MarcherSpeed);break;
                        case EnemyKindV3.BannerBearer:
                            if(square<=EnemyRulesV3Tuning.BannerRadius*EnemyRulesV3Tuning.BannerRadius)target.AuraDamage+=EnemyRulesV3Tuning.BannerDamage;break;
                        case EnemyKindV3.Shielder:
                            if(square<=EnemyRulesV3Tuning.DefaultRadius*EnemyRulesV3Tuning.DefaultRadius && Vector2.Dot(target.Unit.Position-source.Unit.Position,source.Unit.Facing)<0)target.AuraShield=EnemyRulesV3Tuning.BaseShield;break;
                    }
                }
                if(source.Row==null)continue;
                switch(source.Row.Kind)
                {
                    case EnemyKindV3.TimeEater: Emit(EnemyRuleAction.SkillFill,source,"time-eater",source.Unit.Position,EnemyRulesV3Tuning.TimeRadius,EnemyRulesV3Tuning.AuraTick,EnemyRulesV3Tuning.HalfSkill);break;
                    case EnemyKindV3.Leech:
                        UnitState victim=SelectFriend(source.Unit.Position,true);
                        if(victim!=null && (victim.Position-source.Unit.Position).sqrMagnitude<=EnemyRulesV3Tuning.TouchRadius*EnemyRulesV3Tuning.TouchRadius)
                        { source.LeechId=victim.Id; if(leeches.TryGetValue(victim.Id,out List<State> linked))linked.Add(source); source.Unit.Position=victim.Position; Emit(EnemyRuleAction.Damage,source,"leech-drain",victim.Position,EnemyRulesV3Tuning.TouchRadius,0,EnemyRulesV3Tuning.DefaultDamage*EnemyRulesV3Tuning.AuraTick,target:victim.Id); source.Unit.Health=Mathf.Min(MaxHealth(source.Unit),source.Unit.Health+EnemyRulesV3Tuning.DefaultDamage*EnemyRulesV3Tuning.AuraTick); Emit(EnemyRuleAction.LeechLink,source,"leech-link",victim.Position,target:victim.Id); } break;
                }
            }
            for(int i=0;i<count;i++)states[i].DamageSinceAura=0;
        }
        private void SpreadVenom()
        {
            for(int i=0;i<friends.Count;i++)
            {
                FriendVenom source=venom[friends[i].Id];if(!source.Unit.Alive || source.Until<=time)continue;
                for(int j=0;j<friends.Count;j++)
                {
                    FriendVenom target=venom[friends[j].Id];
                    if(!target.Unit.Alive || target.Until>=source.Until || (target.Unit.Position-source.Unit.Position).sqrMagnitude>EnemyRulesV3Tuning.TouchRadius*EnemyRulesV3Tuning.TouchRadius)continue;
                    target.Until=source.Until;target.Source=source.Source;
                    Emit(new EnemyRuleRequest(EnemyRuleAction.Poison,source.Source,"venom-spread",target.Unit.Position,EnemyRulesV3Tuning.TouchRadius,target.Until-time,EnemyRulesV3Tuning.DefaultDamage,target:target.Unit.Id));
                }
            }
        }
        public void CleanseFriend(UnitState friend)
        {if(friend!=null && venom.TryGetValue(friend.Id,out FriendVenom state))state.Until=0;}
        private void TryMerge(State source)
        {
            int count=0;
            for(int i=0;i<states.Count;i++)if(states[i].Unit.Alive && states[i].Row!=null && states[i].Row.Kind==EnemyKindV3.CrumbSwarm && !states[i].CrumbGiant && (states[i].Unit.Position-source.Unit.Position).sqrMagnitude<=EnemyRulesV3Tuning.RankRadius*EnemyRulesV3Tuning.RankRadius)count++;
            if(count<EnemyRulesV3Tuning.CrumbMerge)return;
            source.CrumbGiant=true; source.Unit.Health=EnemyRulesV3Tuning.DefaultHealth;
            Emit(EnemyRuleAction.Phase,source,"crumb-giant",source.Unit.Position,value:EnemyRulesV3Tuning.CrumbMerge);
            int remove=EnemyRulesV3Tuning.CrumbMerge-1;
            for(int i=0;i<states.Count && remove>0;i++)
            { State other=states[i]; if(other==source || !other.Unit.Alive || other.Row==null || other.Row.Kind!=EnemyKindV3.CrumbSwarm || other.CrumbGiant || (other.Unit.Position-source.Unit.Position).sqrMagnitude>EnemyRulesV3Tuning.RankRadius*EnemyRulesV3Tuning.RankRadius)continue;
                other.Unit.Alive=false; other.Unit.Health=0;other.Fell=true;remove--;Emit(EnemyRuleAction.Despawn,other,"crumb-merge",other.Unit.Position,target:other.Unit.Id); }
        }
        private void Begin(State state)
        {
            UnitState unit=state.Unit;
            UnitState target=SelectFriend(unit.Position,state.Row.Kind==EnemyKindV3.SniperDoll || state.Row.Kind==EnemyKindV3.Ambusher || state.Row.Kind==EnemyKindV3.Charmer);
            Vector2 point=target==null?unit.Position:target.Position;
            switch(state.Row.Kind)
            {
                case EnemyKindV3.CrumbSwarm: if(state.CrumbGiant)Warn(state,EnemyRuleAction.Damage,"crumb-slam",unit.Position);break;
                case EnemyKindV3.Pillbug: Warn(state,EnemyRuleAction.Damage,"pillbug-charge",unit.Position,EnemyRulesV3Tuning.ChargeWidth,EnemyRulesV3Tuning.DefaultWarning,shape:EnemyDangerShape.Line,end:point);break;
                case EnemyKindV3.Smasher: Warn(state,EnemyRuleAction.Damage,"slam",unit.Position,warning:EnemyRulesV3Tuning.SlamWarning);Warn(state,EnemyRuleAction.Stun,"slam-stun",unit.Position,duration:EnemyRulesV3Tuning.SlamStun);break;
                case EnemyKindV3.Blinker: if(hero!=null)unit.Position=hero.Position-hero.Facing*EnemyRulesV3Tuning.BehindDistance;break;
                case EnemyKindV3.SniperDoll: Warn(state,EnemyRuleAction.Damage,"sniper-line",unit.Position,EnemyRulesV3Tuning.ChargeWidth,EnemyRulesV3Tuning.SniperWarning,shape:EnemyDangerShape.Line,end:point);break;
                case EnemyKindV3.Bomber: Warn(state,EnemyRuleAction.Damage,"bomber-burst",point,EnemyRulesV3Tuning.BurstRadius);Warn(state,EnemyRuleAction.Poison,"bomber-pool",point,EnemyRulesV3Tuning.BurstRadius,duration:EnemyRulesV3Tuning.PoolSeconds,repeat:true);break;
                case EnemyKindV3.Healer:
                    for(int i=0;i<states.Count;i++)if(states[i].Unit.Alive && (states[i].Unit.Position-unit.Position).sqrMagnitude<=EnemyRulesV3Tuning.GroupRadius*EnemyRulesV3Tuning.GroupRadius)states[i].Unit.Health=Mathf.Min(MaxHealth(states[i].Unit),states[i].Unit.Health+MaxHealth(states[i].Unit)*EnemyRulesV3Tuning.HealFraction);
                    state.Next=time+EnemyRulesV3Tuning.DotTick;break;
                case EnemyKindV3.Buffer: SpeedArea(state,EnemyRulesV3Tuning.BufferSpeed,EnemyRulesV3Tuning.BufferSeconds);break;
                case EnemyKindV3.Charger:
                    target=SelectFarthest(unit.Position);point=target==null?point:target.Position;
                    Warn(state,EnemyRuleAction.Damage,"charger-charge",unit.Position,EnemyRulesV3Tuning.ChargeWidth,shape:EnemyDangerShape.Line,end:point);break;
                case EnemyKindV3.Thief: case EnemyKindV3.GluttonousMouth: Emit(EnemyRuleAction.DrainPickups,state,state.Row.Kind==EnemyKindV3.Thief?"coin-thief":"glutton",unit.Position,EnemyRulesV3Tuning.StealRadius,target:unit.Id);break;
                case EnemyKindV3.Trapper: Warn(state,EnemyRuleAction.Damage,"trap",point);Warn(state,EnemyRuleAction.Stun,"cocoon",point,duration:EnemyRulesV3Tuning.DefaultStatus);break;
                case EnemyKindV3.Marker: markUntil=time+EnemyRulesV3Tuning.DefaultStatus;Emit(EnemyRuleAction.MarkHero,state,"hero-mark",hero==null?point:hero.Position,duration:EnemyRulesV3Tuning.DefaultStatus);break;
                case EnemyKindV3.ConeBrute: Warn(state,EnemyRuleAction.Damage,"cone-splash",unit.Position,shape:EnemyDangerShape.Cone,end:point);break;
                case EnemyKindV3.EggLayer: Warn(state,EnemyRuleAction.Spawn,"egg",unit.Position,warning:EnemyRulesV3Tuning.EggSeconds,species:"CrumbSwarm",count:EnemyRulesV3Tuning.EggCount);break;
                case EnemyKindV3.MagnetBrute: Emit(EnemyRuleAction.Pull,state,"projectile-magnet",unit.Position,EnemyRulesV3Tuning.DefaultRadius);break;
                case EnemyKindV3.Silencer: Warn(state,EnemyRuleAction.PauseSkills,"silence",unit.Position,duration:EnemyRulesV3Tuning.SilenceSeconds,shape:EnemyDangerShape.Ring);break;
                case EnemyKindV3.Cleanser: Warn(state,EnemyRuleAction.Cleanse,"cleanse",point);break;
                case EnemyKindV3.Ambusher: if(target!=null){unit.Position=point;Warn(state,EnemyRuleAction.Damage,"ambush",point,warning:EnemyRulesV3Tuning.AuraTick);}break;
                case EnemyKindV3.Burrower: point=FriendCluster();Warn(state,EnemyRuleAction.Damage,"burrow",point);unit.Position=point;break;
                case EnemyKindV3.KamikazeCandle: point=FriendCluster();Warn(state,EnemyRuleAction.Damage,"kamikaze",point,EnemyRulesV3Tuning.BurstRadius);break;
                case EnemyKindV3.GravityImp: Warn(state,EnemyRuleAction.Pull,"gravity-pull",FriendCluster(),value:EnemyRulesV3Tuning.PullDistance);break;
                case EnemyKindV3.Shrieker: Warn(state,EnemyRuleAction.Fear,"shriek",unit.Position,duration:EnemyRulesV3Tuning.FearSeconds,shape:EnemyDangerShape.Ring);break;
                case EnemyKindV3.Reviver: ReviveNearest(state);break;
                case EnemyKindV3.Summoner:
                    int alive=0;for(int i=0;i<states.Count;i++)if(states[i].Unit.Alive && states[i].ParentId==unit.Id)alive++;
                    if(alive<EnemyRulesV3Tuning.SummonCap)Spawn(state,"summon",unit.Position,"CrumbSwarm",Mathf.Min(EnemyRulesV3Tuning.SummonCount,EnemyRulesV3Tuning.SummonCap-alive));break;
                case EnemyKindV3.Darkener: Warn(state,EnemyRuleAction.MissShots,"dark-zone",point,duration:EnemyRulesV3Tuning.DefaultStatus,value:EnemyRulesV3Tuning.MissChance,repeat:true);break;
                case EnemyKindV3.Binder:
                    for(int i=0;i<states.Count;i++)if(states[i]!=state && states[i].Unit.Alive && states[i].BinderId==0){state.BinderId=states[i].Unit.Id;states[i].BinderId=unit.Id;Emit(EnemyRuleAction.Chain,state,"binder-chain",states[i].Unit.Position,target:states[i].Unit.Id);break;}break;
                case EnemyKindV3.Drainer: Warn(state,EnemyRuleAction.DrainCoins,"drain",point,duration:EnemyRulesV3Tuning.DefaultStatus,value:EnemyRulesV3Tuning.DrainFraction,repeat:true);break;
                case EnemyKindV3.Charmer: if(target!=null)Emit(EnemyRuleAction.Charm,state,"charm",point,duration:EnemyRulesV3Tuning.CharmSeconds,target:target.Id);break;
                case EnemyKindV3.ArmourGrower: Warn(state,EnemyRuleAction.Damage,"armour-slam",unit.Position);state.ArmorUntil=time+EnemyRulesV3Tuning.ArmorSeconds;break;
                case EnemyKindV3.MeteorCaller: Warn(state,EnemyRuleAction.Damage,"meteor",FriendCluster(),warning:EnemyRulesV3Tuning.MeteorWarning);break;
                case EnemyKindV3.TideBrute: Warn(state,EnemyRuleAction.Slow,"flood",unit.Position,duration:EnemyRulesV3Tuning.DefaultStatus,value:EnemyRulesV3Tuning.TideSlow,repeat:true);break;
                case EnemyKindV3.FireBrute: Warn(state,EnemyRuleAction.Burn,"fire-trail",unit.Position,duration:EnemyRulesV3Tuning.PoolSeconds,repeat:true);break;
                case EnemyKindV3.IceWisp: Warn(state,EnemyRuleAction.Chill,"ice-ring",unit.Position,duration:EnemyRulesV3Tuning.DefaultStatus,value:EnemyRulesV3Tuning.ChillSlow,shape:EnemyDangerShape.Ring);break;
                case EnemyKindV3.StormWisp: ChainFriends(state,"contact-lightning");break;
                case EnemyKindV3.VenomWisp: Warn(state,EnemyRuleAction.Poison,"venom-ring",unit.Position,duration:EnemyRulesV3Tuning.PoisonSeconds,shape:EnemyDangerShape.Ring);break;
            }
            if(Has(state,EnemyAffixV3.Burning))Warn(state,EnemyRuleAction.Burn,"burning-affix",unit.Position,duration:EnemyRulesV3Tuning.PoolSeconds,repeat:true);
        }

        private void ReviveNearest(State source)
        {
            State corpse=null;float square=EnemyRulesV3Tuning.GroupRadius*EnemyRulesV3Tuning.GroupRadius;
            for(int i=0;i<states.Count;i++)if(!states[i].Unit.Alive && states[i].Fell && (states[i].Unit.Position-source.Unit.Position).sqrMagnitude<=square)
            { corpse=states[i];square=(corpse.Unit.Position-source.Unit.Position).sqrMagnitude; }
            if(corpse!=null)Warn(source,EnemyRuleAction.ReviveEnemy,"revive-enemy",corpse.Unit.Position,warning:EnemyRulesV3Tuning.ReviveSeconds,species:corpse.Unit.Definition.Id,count:corpse.Unit.Id);
        }
        private void ShieldArea(State source,float radius,float fraction)
        {
            for(int i=0;i<states.Count;i++)if(states[i].Unit.Alive && (states[i].Unit.Position-source.Unit.Position).sqrMagnitude<=radius*radius)
            { states[i].Shield=Mathf.Max(states[i].Shield,MaxHealth(states[i].Unit)*fraction);Emit(EnemyRuleAction.ShieldEnemy,source,"group-shield",states[i].Unit.Position,value:fraction,target:states[i].Unit.Id); }
        }
        private void SpeedArea(State source,float amount,float seconds)
        { for(int i=0;i<states.Count;i++)if(states[i].Unit.Alive && (states[i].Unit.Position-source.Unit.Position).sqrMagnitude<=EnemyRulesV3Tuning.GroupRadius*EnemyRulesV3Tuning.GroupRadius){states[i].SpeedBonus=amount;states[i].SpeedUntil=time+seconds;} }

        public float ModifyIncoming(UnitState target,float damage,UnitState attacker=null,WeaponDefinition weapon=null,Vector2 direction=default)
        {
            State state=Get(target);if(state==null)return damage;
            state.LastDamage=time;
            if(state.ImmuneUntil>time || ChoirProtected(state))return 0;
            if(state.Row!=null && state.Row.Kind==EnemyKindV3.ShieldWall && Vector2.Dot(-direction,target.Facing)>EnemyRulesV3Tuning.FrontDot)return 0;
            if(state.Row!=null && (state.Row.Kind==EnemyKindV3.Decoy || state.Row.Kind==EnemyKindV3.CrumbSwarm && !state.CrumbGiant))return target.Health;
            if(state.Row!=null && state.Row.Kind==EnemyKindV3.MirrorImp && weapon!=null && weapon.Kind==WeaponKind.Projectile && Vector2.Dot(-direction,target.Facing)>EnemyRulesV3Tuning.ReflectFrontDot && attacker!=null)
                Emit(EnemyRuleAction.Reflect,state,"mirror-reflect",attacker.Position,value:damage*EnemyRulesV3Tuning.MirrorReflect,target:attacker.Id);
            if(Has(state,EnemyAffixV3.Reflective) && !state.Reflected && attacker!=null)
            {state.Reflected=true;Emit(EnemyRuleAction.Reflect,state,"reflective-affix",attacker.Position,value:damage*EnemyRulesV3Tuning.Reflect,target:attacker.Id);}
            if(Has(state,EnemyAffixV3.Armored)||state.ArmorUntil>time)damage=RunModifierRules.AfterArmor(damage,EnemyRulesV3Tuning.Armor);
            damage*=1-state.AuraShield;
            float absorbed=Mathf.Min(state.Shield,damage);state.Shield-=absorbed;return damage-absorbed;
        }
        public float ModifyOutgoing(UnitState attacker,UnitState target,float damage)
        {
            State state=Get(attacker);if(state==null)return damage;
            damage*=1+state.AuraDamage+state.DamageBonus+state.Stages*EnemyRulesV3Tuning.GrowDamage;
            if(state.Row!=null && state.Row.Kind==EnemyKindV3.Immune && state.Attacks%EnemyRulesV3Tuning.AttackEvery==0)damage*=EnemyRulesV3Tuning.ChargedScale;
            return damage;
        }
        public void OnDamageDealt(UnitState attacker,UnitState victim,float damage)
        {
            if(damage<=0)return;
            // Nearby vampirism is accumulated in constant time and resolved during the next aura pass.
            State state=Get(attacker);
            if(state!=null)state.DamageSinceAura+=damage;
            if(state!=null && Has(state,EnemyAffixV3.Frozen) && victim!=null)Emit(EnemyRuleAction.Chill,state,"frozen-touch",victim.Position,EnemyRulesV3Tuning.TouchRadius,EnemyRulesV3Tuning.DefaultStatus,EnemyRulesV3Tuning.ChillSlow,target:victim.Id);
        }
        public void OnFriendHit(UnitState friend,float damage)
        {
            // Attached bodies are cached in a reusable list; damage never searches the enemy population.
            if(!leeches.TryGetValue(friend.Id,out List<State> linked))return;
            for(int i=0;i<linked.Count;i++)if(linked[i].Unit.Alive)
            { Emit(new EnemyRuleRequest(EnemyRuleAction.Damage,friend.Id,"leech-shared-hit",linked[i].Unit.Position,value:damage,target:linked[i].Unit.Id)); }
        }
        public void OnAttack(UnitState attacker,UnitState target=null)
        {
            State state=Get(attacker);if(state==null)return;state.Attacks++;
            if(state.Row!=null && state.Row.Kind==EnemyKindV3.StormEye && state.Attacks%EnemyRulesV3Tuning.AttackEvery==0)ChainFriends(state,"storm-eye-chain");
            if(state.Boss!=null && state.Boss.Kind==EnemyBossV3.StormHerald && state.Phase>=2)ChainFriends(state,"charged-arena-chain");
        }
        private void ChainFriends(State state,string rule)
        {
            int count=0;Vector2 last=state.Unit.Position;
            for(int i=0;i<friends.Count && count<=EnemyRulesV3Tuning.ChainTargets;i++)
            {UnitState friend=friends[i];if(!friend.Alive)continue;
                if(rule=="contact-lightning" && count>0 && (friend.Position-last).sqrMagnitude>EnemyRulesV3Tuning.TouchRadius*EnemyRulesV3Tuning.TouchRadius)continue;
                Emit(EnemyRuleAction.Damage,state,rule,friend.Position,EnemyRulesV3Tuning.TouchRadius,value:EnemyRulesV3Tuning.DefaultDamage,target:friend.Id);last=friend.Position;count++;}
        }
        public void OnEnemyDeath(UnitState unit)
        {
            State state=Get(unit);if(state==null || state.Fell)return;
            state.Fell=true;state.DiedAt=time;unit.Alive=false;
            if(state.Singer && byId.TryGetValue(state.ParentId,out State conductor))
            {
                conductor.SingersAlive=Mathf.Max(0,conductor.SingersAlive-1);
                if(!conductor.SingerDeathSeen){conductor.FirstSingerDeath=time;conductor.SingerDeathSeen=true;}
                conductor.LastSingerDeath=time;
            }
            if(state.BinderId!=0 && byId.TryGetValue(state.BinderId,out State partner) && partner.Unit.Alive)
            {Emit(EnemyRuleAction.Damage,state,"binder-shared-hit",partner.Unit.Position,value:MaxHealth(partner.Unit)*EnemyRulesV3Tuning.BinderShare,target:partner.Unit.Id);}
            if(state.Row!=null)
            {
                if(state.Row.Kind==EnemyKindV3.Stitcher)
                    for(int i=0;i<states.Count;i++)if(states[i].Unit.Alive && states[i].Row!=null && states[i].Row.Kind==EnemyKindV3.Stitcher && (states[i].Unit.Position-unit.Position).sqrMagnitude<=EnemyRulesV3Tuning.GroupRadius*EnemyRulesV3Tuning.GroupRadius)states[i].Unit.Health=MaxHealth(states[i].Unit);
                if(state.Row.Kind==EnemyKindV3.Mourner)SpeedArea(state,EnemyRulesV3Tuning.MournerSpeed,EnemyRulesV3Tuning.MournerSeconds);
                if(state.Row.Kind==EnemyKindV3.BannerBearer)Emit(EnemyRuleAction.Banner,state,"fallen-banner",unit.Position,EnemyRulesV3Tuning.BannerRadius,EnemyRulesV3Tuning.PoolSeconds,EnemyRulesV3Tuning.BannerDamage);
                if(state.Row.Kind==EnemyKindV3.Splitter && state.Generation<EnemyRulesV3Tuning.MaxSplitGeneration)Spawn(state,"split",unit.Position,state.Row.Id,EnemyRulesV3Tuning.SplitCount);
                if((state.Row.Kind==EnemyKindV3.Thief || state.Row.Kind==EnemyKindV3.GluttonousMouth)&&state.Stolen>0)Emit(EnemyRuleAction.DrainCoins,state,"refund",unit.Position,value:Mathf.CeilToInt(state.Stolen*EnemyRulesV3Tuning.ThiefRefund));
            }
            if(Has(state,EnemyAffixV3.Splitting)&&state.Generation<EnemyRulesV3Tuning.MaxSplitGeneration)Spawn(state,"split",unit.Position,unit.Definition.Id,EnemyRulesV3Tuning.SplitCount);
            if(Has(state,EnemyAffixV3.Explosive))Warn(state,EnemyRuleAction.Damage,"explosive",unit.Position,EnemyRulesV3Tuning.BurstRadius);
        }
        public void RecordStolen(UnitState thief,int value) { State state=Get(thief);if(state!=null && value>0)state.Stolen+=value; }
        public void Revive(UnitState enemy)
        {
            State state=Get(enemy);if(state==null)return;
            if(state.Singer && state.Fell && byId.TryGetValue(state.ParentId,out State conductor))
            {conductor.SingersAlive++;if(conductor.SingersAlive>=conductor.SingerTotal)conductor.SingerDeathSeen=false;}
            state.Fell=false;enemy.Alive=true;enemy.Health=MaxHealth(enemy);
        }
        public bool CancelEgg(int requestId)
        {for(int i=0;i<pending.Length;i++)if(pending[i].Active && pending[i].Warning.RequestId==requestId && pending[i].Warning.Rule=="egg"){pending[i].Active=false;return true;}return false;}
        public bool ImmuneToControl(UnitState enemy) => Get(enemy)?.Row?.Kind==EnemyKindV3.Immune;
        public bool ImmuneToKnockback(UnitState enemy) => Get(enemy) is State state && (Has(state,EnemyAffixV3.Armored)||state.Row?.Kind==EnemyKindV3.Immune);
        public bool IgnoresWallsAndTaunt(UnitState enemy) => Get(enemy)?.Row?.Kind==EnemyKindV3.PhaseWalker;
        public bool Invisible(UnitState enemy) => Get(enemy)?.Row?.Kind==EnemyKindV3.Ambusher && DistanceToFriend(enemy.Position)>EnemyRulesV3Tuning.AmbushReveal;
        public float SpeedScale(UnitState enemy)
        { State state=Get(enemy);if(state==null)return 1;if(state.DizzyUntil>time && state.ChargeUntil<=time)return 0;return 1+state.AuraSpeed+(state.SpeedUntil>time?state.SpeedBonus:0)+(Has(state,EnemyAffixV3.Swift)?EnemyRulesV3Tuning.Swift:0); }
        public bool MoveOverride(UnitState enemy,float dt)
        {
            State state=Get(enemy);if(state==null)return false;
            if(state.ChargeUntil>time){enemy.Position=Vector2.MoveTowards(enemy.Position,state.To,enemy.Definition.Speed*EnemyRulesV3Tuning.ChargedScale*dt);return true;}
            return state.DizzyUntil>time || state.ImmuneUntil>time || state.Row?.Kind==EnemyKindV3.SniperDoll;
        }
        public void OnWallHit(UnitState enemy)
        {
            State state=Get(enemy);if(state==null || state.Row?.Kind!=EnemyKindV3.Charger)return;
            state.ChargeUntil=time;state.DizzyUntil=time+EnemyRulesV3Tuning.Dizzy;
        }
        public UnitState PreferredFriend(UnitState enemy)
        {
            State state=Get(enemy);if(state==null)return null;
            if(markUntil>time && hero!=null && hero.Alive)return hero;
            if(state.Row?.Kind==EnemyKindV3.Charger)return SelectFarthest(enemy.Position);
            return SelectFriend(enemy.Position,state.Row?.Kind==EnemyKindV3.SniperDoll || state.Row?.Kind==EnemyKindV3.Ambusher || state.Row?.Kind==EnemyKindV3.Leech);
        }
        public bool AbsorbProjectile(ProjectileState shot,float dt)
        {
            if(shot.Faction==Faction.Enemy)return false;
            State nearest=null;float square=EnemyRulesV3Tuning.DefaultRadius*EnemyRulesV3Tuning.DefaultRadius;
            for(int i=0;i<states.Count;i++)if(states[i].Unit.Alive && states[i].Row?.Kind==EnemyKindV3.MagnetBrute && (states[i].Unit.Position-shot.Position).sqrMagnitude<=square){nearest=states[i];square=(nearest.Unit.Position-shot.Position).sqrMagnitude;}
            if(nearest==null)return false;
            Vector2 delta=nearest.Unit.Position-shot.Position;
            if(delta.sqrMagnitude<=nearest.Unit.Definition.Radius*nearest.Unit.Definition.Radius)return true;
            shot.Velocity=Vector2.Lerp(shot.Velocity,delta.normalized*shot.Velocity.magnitude,Mathf.Clamp01(dt/EnemyRulesV3Tuning.AuraTick));return false;
        }

        private void BossTick(State state)
        {
            UnitState unit=state.Unit;float share=unit.Health/MaxHealth(unit);
            if(state.Phase==0 && share<=state.Boss.SecondPhase){state.Phase=1;BossTransition(state);}
            if(state.Phase==1 && share<=state.Boss.FinalPhase){state.Phase=2;BossTransition(state);}
            if(time>=state.Next){state.Next=time+EnemyRulesV3Tuning.DefaultPeriod;BossPattern(state);}
            if(state.Singer)ReturnSinger(state);
        }
        private void BossTransition(State state)
        {
            Emit(EnemyRuleAction.Phase,state,"boss-phase",state.Unit.Position,value:state.Phase);
            bool final=state.Phase>=2;
            switch(state.Boss.Kind)
            {
                case EnemyBossV3.PastryTyrant:
                    if(!final){state.ImmuneUntil=time+EnemyRulesV3Tuning.BossReturn;state.DamageBonus=EnemyRulesV3Tuning.ChargedScale-1;Emit(EnemyRuleAction.Phase,state,"rolling-pin-return",state.Unit.Position,duration:EnemyRulesV3Tuning.BossReturn);}
                    else Spawn(state,"entire-kitchen",state.Unit.Position,"Stitcher",EnemyRulesV3Tuning.KitchenCount);break;
                case EnemyBossV3.ClockworkWarden:
                    if(!final){Spawn(state,"warden-split",state.Unit.Position,state.Boss.Id,1,state.Unit.Id);Emit(EnemyRuleAction.Chain,state,"warden-chain",state.Unit.Position,target:state.Unit.Id,group:state.Unit.Id);}
                    else if(!state.ChainCut){state.Countdown=time+EnemyRulesV3Tuning.BossCountdown;Warn(state,EnemyRuleAction.Countdown,"final-countdown",state.Unit.Position,EnemyRulesV3Tuning.ArenaRadius,EnemyRulesV3Tuning.BossCountdown,value:float.MaxValue,shape:EnemyDangerShape.Arena);}break;
                case EnemyBossV3.StormHerald:
                    if(!final){state.ImmuneUntil=time+EnemyRulesV3Tuning.BossImmunity;Spawn(state,"storm-wisps",state.Unit.Position,"StormWisp",EnemyRulesV3Tuning.SummonCount);}
                    else Emit(EnemyRuleAction.Phase,state,"charged-arena",state.Unit.Position,EnemyRulesV3Tuning.ArenaRadius);break;
                case EnemyBossV3.HollowChoir:
                    if(!final)Spawn(state,"choir-singers",state.Unit.Position,"HollowChoir",EnemyRulesV3Tuning.Singers,state.Unit.Id);
                    else Warn(state,EnemyRuleAction.Stun,"lullaby",state.Unit.Position,EnemyRulesV3Tuning.ArenaRadius,duration:EnemyRulesV3Tuning.ChoirSleep,shape:EnemyDangerShape.Arena);break;
                case EnemyBossV3.GravityMaw:
                    if(!final)Warn(state,EnemyRuleAction.Swallow,"swallow",FriendCluster(),EnemyRulesV3Tuning.BossRadius,duration:EnemyRulesV3Tuning.SwallowSeconds);
                    else Emit(EnemyRuleAction.Pull,state,"black-hole",state.Unit.Position,EnemyRulesV3Tuning.ArenaRadius,EnemyRulesV3Tuning.DefaultStatus,EnemyRulesV3Tuning.PullDistance);break;
                case EnemyBossV3.LanternQueen:
                    if(!final)Emit(EnemyRuleAction.Darken,state,"lights-out",state.Unit.Position,EnemyRulesV3Tuning.ArenaRadius,EnemyRulesV3Tuning.DefaultStatus);
                    else {UnitState best=StrongestFriend();if(best!=null)Emit(EnemyRuleAction.Charm,state,"queen-charm",best.Position,duration:EnemyRulesV3Tuning.QueenCharm,target:best.Id);}break;
            }
        }
        private void BossPattern(State state)
        {
            switch(state.Boss.Kind)
            {
                case EnemyBossV3.PastryTyrant:Warn(state,EnemyRuleAction.Damage,"pastry-smash",FriendCluster(),EnemyRulesV3Tuning.BossRadius);Spawn(state,"crumb-swarm",state.Unit.Position,"CrumbSwarm",EnemyRulesV3Tuning.CrumbWaveCount);break;
                case EnemyBossV3.ClockworkWarden:Warn(state,EnemyRuleAction.PauseSkills,"time-bubble",FriendCluster(),EnemyRulesV3Tuning.BossRadius,duration:EnemyRulesV3Tuning.SilenceSeconds);break;
                case EnemyBossV3.StormHerald:Warn(state,EnemyRuleAction.Damage,"lightning-rain",FriendCluster(),EnemyRulesV3Tuning.BossRadius,EnemyRulesV3Tuning.MeteorWarning);break;
                case EnemyBossV3.HollowChoir:Warn(state,EnemyRuleAction.PauseSkills,"choir-silence",FriendCluster(),EnemyRulesV3Tuning.BossRadius,duration:EnemyRulesV3Tuning.SilenceSeconds,shape:EnemyDangerShape.Ring);break;
                case EnemyBossV3.GravityMaw:Warn(state,EnemyRuleAction.Pull,"maw-pull",state.Unit.Position,state.Phase>=2?EnemyRulesV3Tuning.ArenaRadius:EnemyRulesV3Tuning.BossRadius,value:EnemyRulesV3Tuning.PullDistance);break;
                case EnemyBossV3.LanternQueen:Warn(state,EnemyRuleAction.MissShots,"queen-dark",FriendCluster(),EnemyRulesV3Tuning.BossRadius,duration:EnemyRulesV3Tuning.DefaultStatus,value:EnemyRulesV3Tuning.MissChance);Spawn(state,"queen-summon",state.Unit.Position,"Darkener",EnemyRulesV3Tuning.EggCount);break;
            }
        }
        public bool CutChain(int wardenId)
        {
            if(!byId.TryGetValue(wardenId,out State state)||state.Boss?.Kind!=EnemyBossV3.ClockworkWarden)return false;
            state.ChainCut=true;if(state.BinderId!=0 && byId.TryGetValue(state.BinderId,out State partner))partner.ChainCut=true;
            Emit(EnemyRuleAction.ChainCut,state,"chain-cut",state.Unit.Position);return true;
        }
        private bool ChoirProtected(State state)
        {
            if(state.Boss?.Kind!=EnemyBossV3.HollowChoir || state.Singer || state.Phase<1)return false;
            return state.SingerTotal>0 && (state.SingersAlive>0 || state.LastSingerDeath-state.FirstSingerDeath>EnemyRulesV3Tuning.SingTogetherSeconds);
        }
        private void ReturnSinger(State state)
        {
            if(state.Unit.Alive || !state.Singer || time-state.DiedAt<EnemyRulesV3Tuning.SingTogetherSeconds)return;
            bool anyAlive=false;float earliest=float.PositiveInfinity,latest=0;
            for(int i=0;i<states.Count;i++)if(states[i].Singer && states[i].GroupId==state.GroupId)
            {if(states[i].Unit.Alive)anyAlive=true;else{earliest=Mathf.Min(earliest,states[i].DiedAt);latest=Mathf.Max(latest,states[i].DiedAt);}}
            if(anyAlive || latest-earliest>EnemyRulesV3Tuning.SingTogetherSeconds){Emit(EnemyRuleAction.ReviveEnemy,state,"singer-return",state.Unit.Position,count:state.Unit.Id,species:state.Unit.Definition.Id,group:state.GroupId);Revive(state.Unit);}
        }

        private void Warn(State source,EnemyRuleAction action,string rule,Vector2 at,float radius=EnemyRulesV3Tuning.DefaultRadius,float warning=EnemyRulesV3Tuning.DefaultWarning,float duration=0,float value=EnemyRulesV3Tuning.DefaultDamage,EnemyDangerShape shape=EnemyDangerShape.Circle,Vector2 end=default,bool repeat=false,string species=null,int count=0)
        {
            EnemyDangerRequest request=new EnemyDangerRequest(++nextWarning,source.Unit.Id,shape,action,rule,at,end,radius,warning,duration,value);
            for(int i=0;i<pending.Length;i++)if(!pending[i].Active)
            {pending[i]=new Pending {Active=true,Warning=request,At=time+warning,Until=time+warning+duration,Repeat=repeat,Species=species,Count=count};DangerRequested?.Invoke(request);return;}
            DroppedWarnings++;
        }
        private void Spawn(State source,string rule,Vector2 position,string species,int count,int group=0)
        {Emit(new EnemyRuleRequest(EnemyRuleAction.Spawn,source.Unit.Id,rule,position,count:count,species:species,group:group));}
        private void Emit(EnemyRuleAction action,State source,string rule,Vector2 at,float radius=0,float duration=0,float value=0,int target=0,int group=0,int count=0,string species=null)
        {Emit(new EnemyRuleRequest(action,source.Unit.Id,rule,at,radius,duration,value,target:target,group:group,count:count,species:species));}
        private void Emit(EnemyRuleRequest request) => Requested?.Invoke(request);
        private UnitState SelectFriend(Vector2 origin,bool weakest)
        {
            UnitState best=null;float score=float.PositiveInfinity;
            if(units==null)return null;
            for(int i=0;i<friends.Count;i++)
            {UnitState unit=friends[i];if(!unit.Alive)continue;
                float value=weakest?unit.Health/Mathf.Max(1,unit.Definition.Health):(unit.Position-origin).sqrMagnitude;
                if(value<score || value==score && (best==null || unit.Id<best.Id)){best=unit;score=value;}}
            return best;
        }
        private UnitState SelectFarthest(Vector2 origin)
        {UnitState best=null;float square=-1;for(int i=0;i<friends.Count;i++){UnitState unit=friends[i];if(unit.Alive && (unit.Position-origin).sqrMagnitude>square){best=unit;square=(unit.Position-origin).sqrMagnitude;}}return best;}
        private UnitState StrongestFriend()
        {UnitState best=null;float damage=-1;for(int i=0;i<friends.Count;i++){UnitState unit=friends[i];if(unit.Alive && unit.Definition.Weapon!=null && unit.Definition.Weapon.Damage>damage){best=unit;damage=unit.Definition.Weapon.Damage;}}return best;}
        private float DistanceToFriend(Vector2 position)
        {UnitState friend=SelectFriend(position,false);return friend==null?float.PositiveInfinity:(friend.Position-position).magnitude;}
        private Vector2 FriendCluster()
        {
            Vector2 best=hero==null?Vector2.zero:hero.Position;int most=-1;
            for(int i=0;i<friends.Count;i++)
            {UnitState center=friends[i];if(!center.Alive)continue;int count=0;
                for(int j=0;j<friends.Count;j++){UnitState friend=friends[j];if(friend.Alive && (friend.Position-center.Position).sqrMagnitude<=EnemyRulesV3Tuning.DefaultRadius*EnemyRulesV3Tuning.DefaultRadius)count++;}
                if(count>most){most=count;best=center.Position;}}
            return best;
        }
        public static bool Contains(EnemyDangerShape shape,Vector2 start,Vector2 end,float radius,Vector2 point)
        {
            if(shape==EnemyDangerShape.Arena)return true;
            Vector2 delta=point-start;float square=radius*radius;
            if(shape==EnemyDangerShape.Line){Vector2 line=end-start;float length=line.sqrMagnitude;float t=length>EnemyRulesV3Tuning.Epsilon?Mathf.Clamp01(Vector2.Dot(delta,line)/length):0;return (point-start-line*t).sqrMagnitude<=square;}
            if(delta.sqrMagnitude>square)return false;
            if(shape==EnemyDangerShape.Ring)return delta.sqrMagnitude>=square*EnemyRulesV3Tuning.RingInnerFraction;
            if(shape==EnemyDangerShape.Cone){Vector2 direction=end-start;return delta.sqrMagnitude<=EnemyRulesV3Tuning.Epsilon || Vector2.Dot(delta.normalized,direction.normalized)>=EnemyRulesV3Tuning.ConeCosine;}
            return true;
        }
    }
}
