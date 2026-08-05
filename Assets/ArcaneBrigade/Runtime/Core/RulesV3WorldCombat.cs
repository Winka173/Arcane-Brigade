using System;
using UnityEngine;
namespace ArcaneBrigade
{
    public enum RulesV3EntityKind { Ally, Snack, Clover, Feather, Page, Pumpkin, Skull, Banner, Seed, WindLane }
    public enum RulesV3EntityChange { Created, Collected, Expired }
    public enum RulesV3MountMode { Guard, Shoot, Sweep }
    public readonly struct RulesV3EntityRequest
    {
        public readonly int Id, SourceId, TargetId;
        public readonly RulesV3EntityKind Kind;
        public readonly RulesV3EntityChange Change;
        public readonly string Rule;
        public readonly Vector2 Position, End;
        public readonly float Radius, Duration, Value;
        public RulesV3EntityRequest(int id,int source,int target,RulesV3EntityKind kind,RulesV3EntityChange change,string rule,Vector2 position,Vector2 end,float radius,float duration,float value)
        { Id=id;SourceId=source;TargetId=target;Kind=kind;Change=change;Rule=rule;Position=position;End=end;Radius=radius;Duration=duration;Value=value; }
    }
    public sealed partial class CombatSimulation
    {
        private struct RuleEntity
        {
            public bool Active;
            public int Id,Source,Target,Coins,Rerolls,Upgrades;
            public RulesV3EntityKind Kind;
            public RulesV3MountMode Mode;
            public string Rule;
            public Vector2 Position,End;
            public float Radius,Until,Next,Value,Heal;
        }
        private readonly RuleEntity[] v3Entities=new RuleEntity[RulesV3Tuning.ConstructCapacity];
        private readonly int[] v3ClusterBins=new int[HeroRulesV3Tuning.ClusterBins];
        private readonly Vector2[] v3ClusterSums=new Vector2[HeroRulesV3Tuning.ClusterBins];
        public event Action<RulesV3EntityRequest> RulesV3EntityChanged;
        public bool TryGetRulesV3Entity(int index,out RulesV3EntityRequest request)
        {
            request=default;if(index<0 || index>=v3Entities.Length || !v3Entities[index].Active)return false;
            RuleEntity e=v3Entities[index];request=new RulesV3EntityRequest(e.Id,e.Source,e.Target,e.Kind,RulesV3EntityChange.Created,e.Rule,e.Position,e.End,e.Radius,Mathf.Max(0,e.Until-Elapsed),e.Value);return true;
        }
        private void StartRulesV3World()
        {
            Array.Clear(v3Entities,0,v3Entities.Length);
            Bonds.SpawnRequested+=ApplyV3BondSpawn;
            for(int i=0;i<Bonds.SpawnRequests.Count;i++)ApplyV3BondSpawn(Bonds.SpawnRequests[i]);
            Bonds.ClearSpawnRequests();
            Bonds.RuleTriggered+=request=>
            { if(request.Rule=="beat")TraitsV3?.OnMelodyBeat(); };
        }
        private void EntityEvent(RuleEntity entity,RulesV3EntityChange change)
        { RulesV3EntityChanged?.Invoke(new RulesV3EntityRequest(entity.Id,entity.Source,entity.Target,entity.Kind,change,entity.Rule,entity.Position,entity.End,entity.Radius,Mathf.Max(0,entity.Until-Elapsed),entity.Value)); }
        private void AddV3Entity(RulesV3EntityKind kind,string rule,Vector2 position,int source=0,int target=0,float duration=0,float value=0,float heal=0,int coins=0,int rerolls=0,Vector2 end=default,float radius=RulesV3Tuning.ConstructRadius)
        {
            for(int i=0;i<v3Entities.Length;i++)if(!v3Entities[i].Active || v3Entities[i].Until<=Elapsed)
            {
                var entity=new RuleEntity { Active=true,Id=++nextId,Kind=kind,Rule=rule,Position=position,End=end,Source=source,Target=target,
                    Until=Elapsed+(duration>0?duration:RulesV3Tuning.WaveSeconds),Radius=radius,Value=value,Heal=heal,Coins=coins,Rerolls=rerolls,Next=Elapsed };
                v3Entities[i]=entity;EntityEvent(entity,RulesV3EntityChange.Created);return;
            }
        }
        private void ApplyV3BondSpawn(BondSpawnRequest request)
        {
            if(request.Kind==BondSpawnKind.Snack || request.Kind==BondSpawnKind.Clover)
            {
                AddV3Entity(request.Kind==BondSpawnKind.Snack?RulesV3EntityKind.Snack:RulesV3EntityKind.Clover,request.Kind.ToString(),request.Position,
                    request.SourceId,duration:request.Lifetime,heal:request.HealMaxHealthFraction,coins:request.Coins,rerolls:request.FreeRerolls);return;
            }
            float damage=request.Damage>0?request.Damage:Champion.Definition.Weapon.Damage;
            AddV3Entity(RulesV3EntityKind.Ally,request.Kind.ToString(),request.Position,request.SourceId,duration:request.Lifetime,value:damage);
        }
        private Vector2 V3CrowdCenter(Vector2 origin)
        {
            Array.Clear(v3ClusterBins,0,v3ClusterBins.Length);Array.Clear(v3ClusterSums,0,v3ClusterSums.Length);int best=0;
            foreach(UnitState enemy in StableUnits)
            {
                if(!enemy.Alive || enemy.Faction!=Faction.Enemy)continue;
                Vector2 d=enemy.Position-origin;
                int x=Mathf.Clamp(Mathf.FloorToInt((d.x+HeroRulesV3Tuning.ClusterExtent)/HeroRulesV3Tuning.ClusterCell),0,HeroRulesV3Tuning.ClusterSide-1);
                int y=Mathf.Clamp(Mathf.FloorToInt((d.y+HeroRulesV3Tuning.ClusterExtent)/HeroRulesV3Tuning.ClusterCell),0,HeroRulesV3Tuning.ClusterSide-1);
                int key=x+y*HeroRulesV3Tuning.ClusterSide;v3ClusterBins[key]++;v3ClusterSums[key]+=enemy.Position;
                if(v3ClusterBins[key]>v3ClusterBins[best])best=key;
            }
            return v3ClusterBins[best]>0?v3ClusterSums[best]/v3ClusterBins[best]:origin;
        }
        public bool SetRulesV3WindBeastMode(int entityId,RulesV3MountMode mode)
        { for(int i=0;i<v3Entities.Length;i++)if(v3Entities[i].Active && v3Entities[i].Id==entityId && v3Entities[i].Rule=="WindBeast"){v3Entities[i].Mode=mode;return true;}return false; }
        public bool UpgradeRulesV3Turret(int entityId)
        { for(int i=0;i<v3Entities.Length;i++)if(v3Entities[i].Active && v3Entities[i].Id==entityId && v3Entities[i].Rule=="Turret" && v3Entities[i].Upgrades<RulesV3Tuning.TurretUpgradeLimit){v3Entities[i].Upgrades++;return true;}return false; }
        private float V3Heal(UnitState unit,float amount)
        { float actual=Mortality.Heal(unit,amount);if(unit==Champion && actual>0)Bonds.OnHeal(actual);return actual; }
        private void TickRulesV3World()
        {
            for(int i=0;i<v3Entities.Length;i++)
            {
                RuleEntity entity=v3Entities[i];if(!entity.Active)continue;
                if(entity.Until<=Elapsed){v3Entities[i].Active=false;EntityEvent(entity,RulesV3EntityChange.Expired);continue;}
                if(entity.Kind==RulesV3EntityKind.Ally || entity.Kind==RulesV3EntityKind.Skull || entity.Kind==RulesV3EntityKind.Seed)
                {
                    if(entity.Next>Elapsed)continue;v3Entities[i].Next=Elapsed+RulesV3Tuning.ConstructTick;
                    Vector2 cluster=V3CrowdCenter(entity.Position);
                    if(entity.Rule=="Spirit" && Bonds.Active("spirit",4))V3Heal(Champion,Champion.Definition.Health*BondTuning.SpiritHeal);
                    if(entity.Rule=="WindBeast" && entity.Mode==RulesV3MountMode.Guard || entity.Rule=="Orchestra")
                        for(int f=0;f<Mortality.FriendCount;f++)Mortality.Shield(Mortality.FriendAt(f),RulesV3Tuning.AllyShield,RulesV3Tuning.ConstructTick);
                    foreach(UnitState enemy in StableUnits)
                    {
                        if(!enemy.Alive || enemy.Faction!=Faction.Enemy || (enemy.Position-cluster).sqrMagnitude>entity.Radius*entity.Radius)continue;
                        if(entity.Kind==RulesV3EntityKind.Skull){enemy.TauntBy=Champion.Id;enemy.TauntUntil=entity.Until;continue;}
                        ApplyDamage(enemy,entity.Value*(1+entity.Upgrades*RulesV3Tuning.TurretUpgradeDamage),Faction.Companion,cause:NewCause(entity.Source).Child("v3-ally"));
                    }
                    continue;
                }
                if(entity.Kind==RulesV3EntityKind.WindLane)
                {
                    for(int f=0;f<Mortality.FriendCount;f++)
                    { UnitState friend=Mortality.FriendAt(f);if(friend.Alive && CompanionSkillRuntime.Contains(SkillShape.Line,entity.Position,entity.End,entity.Radius,friend.Position))friend.HasteUntil=Elapsed+RulesV3Tuning.ConstructTick; }
                    continue;
                }
                UnitState collector=null;
                for(int f=0;f<Mortality.FriendCount;f++)
                { UnitState friend=Mortality.FriendAt(f);if(friend.Alive && (friend.Position-entity.Position).sqrMagnitude<=RulesV3Tuning.RulePickupRadius*RulesV3Tuning.RulePickupRadius){collector=friend;break;} }
                if(collector==null)continue;
                v3Entities[i].Active=false;EntityEvent(entity,RulesV3EntityChange.Collected);
                if(entity.Kind==RulesV3EntityKind.Feather)
                { UnitState fallen=UnitWithId(entity.Target);if(Mortality.ReviveNow(fallen,entity.Value))TraitsV3.OnFeatherRevive(UnitWithId(entity.Source),fallen); }
                else if(entity.Kind==RulesV3EntityKind.Page)QueueV3Copy(collector.Faction==Faction.Companion?collector:V3BestCompanion(),entity.Value);
                else if(entity.Kind==RulesV3EntityKind.Banner)
                {V3Status state=V3State(collector);state.Damage=entity.Value;state.DamageUntil=entity.Until;}
                else V3Heal(entity.Rule=="cherry"?Champion:collector,MortalityRuntime.MaxHealth(entity.Rule=="cherry"?Champion:collector)*entity.Heal);
                Gold=RunSession.BoundedCounter((long)Gold+entity.Coins);RulesV3Rerolls+=entity.Rerolls;
            }
            Bonds.ClearSpawnRequests();
        }
        private UnitState V3BestCompanion()
        { UnitState best=null;for(int i=0;i<Mortality.FriendCount;i++){UnitState unit=Mortality.FriendAt(i);if(unit.Alive && unit.Faction==Faction.Companion && (best==null || unit.Definition.Weapon.Damage>best.Definition.Weapon.Damage))best=unit;}return best; }
    }
}
