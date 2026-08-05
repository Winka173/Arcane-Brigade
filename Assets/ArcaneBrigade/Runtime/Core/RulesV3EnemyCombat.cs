using System;
using System.Collections.Generic;
using UnityEngine;
namespace ArcaneBrigade
{
    public sealed partial class CombatSimulation
    {
        public EnemyRulesV3Runtime EnemyRulesV3 { get; private set; }
        private readonly Dictionary<string,UnitDefinition> v3EnemyDefinitions=new Dictionary<string,UnitDefinition>(StringComparer.Ordinal);
        private void StartEnemyRulesV3()
        {
            v3EnemyDefinitions.Clear();
            foreach(UnitDefinition enemy in catalog.EnemyPool)if(enemy!=null)v3EnemyDefinitions[enemy.Id]=enemy;
            if(catalog.MeleeEnemy!=null)v3EnemyDefinitions[catalog.MeleeEnemy.Id]=catalog.MeleeEnemy;
            if(catalog.RangedEnemy!=null)v3EnemyDefinitions[catalog.RangedEnemy.Id]=catalog.RangedEnemy;
            if(catalog.Boss!=null)v3EnemyDefinitions[catalog.Boss.Id]=catalog.Boss;
            EnemyRulesV3=new EnemyRulesV3Runtime();
            EnemyRulesV3.Requested+=ApplyV3Enemy;
            EnemyRulesV3.ResetWave(units,Champion,catalog.RunWave,unchecked((uint)runSeed));
            StartRulesV3Objectives();
        }
        private void ApplyV3Enemy(EnemyRuleRequest request)
        {
            UnitState source=UnitWithId(request.SourceId), target=UnitWithId(request.TargetId);
            if(request.Action==EnemyRuleAction.Damage && target!=null && target.Faction==Faction.Enemy)
            { ApplyDamage(target,request.Value,Faction.Companion,cause:NewCause(request.SourceId).Child("enemy-v3-link"));return; }
            if(request.Action==EnemyRuleAction.Banner)
            { AddV3Entity(RulesV3EntityKind.Banner,request.Rule,request.Position,request.SourceId,duration:request.Duration,value:request.Value);return; }
            if(request.Action==EnemyRuleAction.Countdown)
            { Mortality.DefeatAll(request.SourceId);foreach(UnitState unit in StableUnits)if(unit.Faction==Faction.Enemy){unit.Health=0;unit.Alive=false;}return; }
            switch(request.Action)
            {
                case EnemyRuleAction.Spawn:
                    UnitDefinition definition;
                    if(!v3EnemyDefinitions.TryGetValue(request.Species??string.Empty,out definition))
                    {
                        EnemyRuleDefinition row=EnemyRulesV3Catalog.Find(request.Species);
                        EnemyBossDefinition boss=EnemyRulesV3Catalog.FindBoss(request.Species);
                        definition=row!=null?EnemyRulesV3Catalog.Create(row):boss!=null?EnemyRulesV3Catalog.CreateBoss(boss):catalog.MeleeEnemy;
                        if(definition!=null)v3EnemyDefinitions[definition.Id]=definition;
                    }
                    if(definition!=null)for(int i=0;i<request.Count;i++)
                    {
                        UnitState child=Spawn(definition,Faction.Enemy,ClampPosition(request.Position+Vector2.right*(i*definition.Radius),definition.Radius));
                        EnemyRulesV3.RegisterSpawned(child,request.SourceId,request.Rule,request.GroupId);
                        if(request.Rule=="warden-split")AddV3Objective(-request.SourceId,request.SourceId,child.Id,request.Position,RulesV3Tuning.WaveSeconds,RulesV3Tuning.ContactPadding,false);
                    }
                    return;
                case EnemyRuleAction.ReviveEnemy:
                    UnitState corpse=UnitWithId(request.Count);
                    if(corpse!=null)EnemyRulesV3.Revive(corpse);
                    return;
                case EnemyRuleAction.Despawn:
                    if(source!=null){source.Health=0;source.Alive=false;}return;
                case EnemyRuleAction.HealEnemy:
                    if(target!=null)target.Health=Mathf.Min(target.Definition.Health,target.Health+request.Value);
                    return;
                case EnemyRuleAction.DrainCoins:
                    if(request.Rule=="refund"){DropLoot(LootKind.Coin,request.Position,Mathf.RoundToInt(request.Value));return;}
                    int coins=Mathf.Min(Gold,Mathf.RoundToInt(request.Value));Gold-=coins;
                    if(source!=null)EnemyRulesV3.RecordStolen(source,coins);return;
                case EnemyRuleAction.DrainPickups:
                    int stolen=0;
                    for(int i=pickups.Count-1;i>=0;i--)if((pickups[i].Position-request.Position).sqrMagnitude<=request.Radius*request.Radius)
                    { stolen+=pickups[i].Value;pickups.RemoveAt(i); }
                    if(source!=null)EnemyRulesV3.RecordStolen(source,stolen);return;
            }
            for(int i=0;i<Mortality.FriendCount;i++)
            {
                UnitState friend=Mortality.FriendAt(i);
                if(!friend.Alive || (request.TargetId!=0 ? friend!=target : !EnemyRulesV3Runtime.Contains(request.Shape,request.Position,request.End,request.Radius,friend.Position)))continue;
                V3Status state=V3State(friend);if(state==null)continue;
                switch(request.Action)
                {
                    case EnemyRuleAction.Damage: case EnemyRuleAction.Reflect:
                        ApplyDamage(friend,request.Value,Faction.Enemy,cause:NewCause(request.SourceId).Child("enemy-v3"));break;
                    case EnemyRuleAction.Poison: case EnemyRuleAction.Burn:
                        state.Dot=request.Value>0?request.Value:EnemyRulesV3Tuning.DefaultDamage;state.DotSource=source;state.DotUntil=Elapsed+request.Duration;break;
                    case EnemyRuleAction.Chill: case EnemyRuleAction.Slow:
                        state.Slow=Mathf.Clamp01(state.SlowUntil>Elapsed?state.Slow+request.Value:request.Value);state.SlowUntil=Elapsed+request.Duration;break;
                    case EnemyRuleAction.Stun: friend.SleepUntil=Elapsed+request.Duration;break;
                    case EnemyRuleAction.Fear: state.FearUntil=Elapsed+request.Duration;state.FearOrigin=request.Position;break;
                    case EnemyRuleAction.Pull:
                        friend.Position=ClampPosition(Vector2.MoveTowards(friend.Position,request.Position,request.Value),friend.Definition.Radius);break;
                    case EnemyRuleAction.PauseSkills: SkillsV3.Pause(friend.Id,request.Duration);break;
                    case EnemyRuleAction.SkillFill: SkillsV3.Accelerate(friend.Id,request.Value-1,request.Duration);break;
                    case EnemyRuleAction.MissShots: case EnemyRuleAction.Darken:
                        state.Miss=request.Value;state.MissUntil=Elapsed+request.Duration;break;
                    case EnemyRuleAction.Charm: state.CharmUntil=Elapsed+request.Duration;SkillsV3.Pause(friend.Id,request.Duration);break;
                    case EnemyRuleAction.Swallow: state.SwallowUntil=Elapsed+request.Duration;SkillsV3.Pause(friend.Id,request.Duration);break;
                    case EnemyRuleAction.Cleanse:
                        Mortality.RemoveProtection(friend);state.DamageUntil=state.AttackUntil=state.CritUntil=state.ReflectUntil=state.ArmorUntil=state.UntouchableUntil=0;break;
                }
            }
        }
        private bool V3Targetable(UnitState unit)
        { V3Status state=V3State(unit);return (state==null || state.SwallowUntil<=Elapsed) && (unit!=Champion || !(HeroRulesV3?.Untargetable ?? false)); }
        private bool V3BlocksAction(UnitState unit)
        { V3Status state=V3State(unit);return (!(EnemyRulesV3?.ImmuneToControl(unit)??false) && unit.SleepUntil>Elapsed) || state!=null && state.SwallowUntil>Elapsed; }
        private Faction V3AttackFaction(UnitState unit)
        { V3Status state=V3State(unit);return state!=null && state.CharmUntil>Elapsed?Faction.Enemy:unit.Faction; }
        private UnitState V3HostileFriendTarget(UnitState unit,float range)
        {
            UnitState result=null;float square=range*range;
            for(int i=0;i<Mortality.FriendCount;i++)
            { UnitState friend=Mortality.FriendAt(i);float distance=(friend.Position-unit.Position).sqrMagnitude;
                if(friend!=unit && friend.Alive && distance<=square){square=distance;result=friend;} }
            return result;
        }
        private bool V3BossesDefeated()
        { foreach(UnitState unit in StableUnits)if(unit.Alive && unit.Definition.IsBoss)return false;return true; }
        private bool SpawnRulesV3WaveEnemies()
        {
            if(EnemyRulesV3==null)return false;
            if(Elapsed<nextSpawn || catalog.EnemyPool.Length==0)return true;
            nextSpawn=Elapsed+Mathf.Max(RulesV3Tuning.SpawnPeriodMinimum,catalog.SpawnInterval);
            int alive=0;foreach(UnitState unit in StableUnits)if(unit.Alive && unit.Faction==Faction.Enemy)alive++;
            if(alive>=catalog.EnemyBudget)return true;
            UnitDefinition definition=catalog.EnemyPool[spawnRandom.Next(catalog.EnemyPool.Length)];
            int batch=EnemyRulesV3.SpawnPackCount(definition);
            if(batch==1)batch=RulesV3Tuning.SpawnBatch;
            Vector2 half=catalog.ArenaSize*.5f-Vector2.one*RulesV3Tuning.SpawnEdgePadding;
            int edge=spawnRandom.Next(4);
            for(int i=0;i<batch && alive+i<catalog.EnemyBudget;i++)
            {
                float along=(float)spawnRandom.NextDouble()*2-1;
                Vector2 position=edge==0?new Vector2(-half.x,along*half.y):edge==1?new Vector2(half.x,along*half.y):edge==2?new Vector2(along*half.x,-half.y):new Vector2(along*half.x,half.y);
                UnitState enemy=Spawn(definition,Faction.Enemy,ClampPosition(position,definition.Radius));
                enemy.Elite=catalog.RunWave>=RulesV3Tuning.EliteFirstWave && ++swarmSpawns%RulesV3Tuning.EliteEvery==0;
            }
            return true;
        }
    }
}
