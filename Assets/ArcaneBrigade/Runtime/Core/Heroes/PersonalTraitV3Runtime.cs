using System;
using System.Collections.Generic;
using UnityEngine;
namespace ArcaneBrigade
{
    /// <summary>The sixteen unique companion traits modify rules through requests and persistent run data.</summary>
    public sealed class PersonalTraitV3Runtime
    {
        private HeroRulesV3RunState run;
        private IList<UnitState> units;
        private readonly Dictionary<int,int> casts=new Dictionary<int,int>();
        private readonly Dictionary<int,int> piperStacks=new Dictionary<int,int>();
        private readonly Dictionary<int,float> piperUntil=new Dictionary<int,float>();
        private readonly Dictionary<int,float> spotlightUntil=new Dictionary<int,float>();
        private float time;
        private int crits,kills;
        public event Action<HeroRuleRequest> Requested;
        public PersonalTraitV3Runtime(HeroRulesV3RunState persistence=null){run=persistence??new HeroRulesV3RunState();}
        public HeroRulesV3RunState RunState=>run;
        public void ResetWave(IList<UnitState> all)
        {
            units=all;time=0;crits=kills=0;casts.Clear();piperStacks.Clear();piperUntil.Clear();spotlightUntil.Clear();
            for(int i=0;i<run.Coals.Count;i++){TraitCoalPatch p=run.Coals[i];Emit(HeroRuleAction.PersistentCoals,"undying-flame",p.SourceId,p.Position,HeroRulesV3Tuning.Radius,HeroRulesV3Tuning.ArenaRadius,p.Damage);}
            foreach(var entry in run.SeedAges)Emit(HeroRuleAction.SeedPod,"thorn-garden",entry.Key,Vector2.zero,value:entry.Value,count:entry.Value>=HeroRulesV3Tuning.FruitAge?1:0);
            if(run.MarginaliaSkill!=null)Emit(HeroRuleAction.CopySkill,"marginalia",0,Vector2.zero,value:1,species:run.MarginaliaSkill);
            run.MarginaliaSkill=null;
            if(run.WavesSurvived>0 && units!=null)for(int i=0;i<units.Count;i++)if(units[i].Faction==Faction.Companion && units[i].Definition.Id=="EclipseEgg")Emit(HeroRuleAction.Counter,run.EggBright?"dawn-wishes":"dusk-form",units[i].Id,units[i].Position,value:1);
        }
        public void Tick(float dt)
        {
            if(float.IsNaN(dt)||float.IsInfinity(dt)||dt<=0)return;time+=dt;
            if(units==null)return;
            for(int i=0;i<units.Count;i++){UnitState f=units[i];if(f.Alive&&f.Faction==Faction.Companion&&f.Definition.Id=="PlanetHamster")Emit(HeroRuleAction.Pull,"gravity-well-bullets",f.Id,f.Position,HeroRulesV3Tuning.SpiritPullRadius,dt,1);}
        }
        public void OnCritical(){crits++;}
        public void OnKill(){kills++;}
        public void OnWaveSurvived()
        {
            run.EggBright=crits>kills;
            if(units==null)return;
            for(int i=0;i<units.Count;i++)
            {
                var friend=units[i]; if(friend.Faction!=Faction.Companion)continue;
                if(friend.Definition.Id=="FirstSeed") {run.SeedAges.TryGetValue(friend.Id,out int age);run.SeedAges[friend.Id]=age+1;}
                if(friend.Definition.Id=="EclipseEgg")Emit(HeroRuleAction.Counter,run.EggBright?"dawn-wishes":"dusk-form",friend.Id,friend.Position,value:1);
            }
        }
        public void OnCoals(UnitState source,Vector2 position,float damage)
        {
            if(source?.Definition?.Id!="TeaDragon")return;
            if(run.Coals.Count>=HeroRulesV3Tuning.CoalsCap)run.Coals.RemoveAt(0);
            TraitCoalPatch patch=new TraitCoalPatch{Position=position,Damage=damage*HeroRulesV3Tuning.CoalPower,SourceId=source.Id};run.Coals.Add(patch);
            Emit(HeroRuleAction.PersistentCoals,"undying-flame",source.Id,position,HeroRulesV3Tuning.Radius,HeroRulesV3Tuning.ArenaRadius,patch.Damage);
        }
        public void OnSkill(UnitState source,string skillId,float strength=1,bool copy=false)
        {
            if(source==null||source.Definition==null||source.Faction!=Faction.Companion||!source.Alive||copy)return;
            casts.TryGetValue(source.Id,out int count);casts[source.Id]=++count;
            switch(source.Definition.Id)
            {
                case "CoilKettle": Emit(HeroRuleAction.Damage,"coil-shot",source.Id,source.Position,HeroRulesV3Tuning.Radius,value:source.Definition.Weapon.Damage*strength,count:HeroRulesV3Tuning.CoilClusters);break;
                case "KiteDragon":Emit(HeroRuleAction.WindLane,"gale-ribbon",source.Id,source.Position,HeroRulesV3Tuning.LineWidth,HeroRulesV3Tuning.WindLaneSeconds,shape:HeroRuleShape.Line,end:source.Position+source.Facing*HeroRulesV3Tuning.LineLength);break;
                case "FirstSeed":if(!run.SeedAges.ContainsKey(source.Id))run.SeedAges.Add(source.Id,0);Emit(HeroRuleAction.SeedPod,"thorn-garden",source.Id,source.Position,HeroRulesV3Tuning.Radius,HeroRulesV3Tuning.ArenaRadius);break;
                case "ClockworkPiper":piperStacks.TryGetValue(source.Id,out int stack);stack=Mathf.Min(stack+1,HeroRulesV3Tuning.PiperStacks);piperStacks[source.Id]=stack;piperUntil[source.Id]=time+HeroRulesV3Tuning.PiperSeconds;Emit(HeroRuleAction.SkillSpeed,"tuning-fork",source.Id,source.Position,HeroRulesV3Tuning.Radius,HeroRulesV3Tuning.PiperSeconds,stack*HeroRulesV3Tuning.PiperSpeed);break;
                case "EndlessIndex":Emit(HeroRuleAction.TraitSignal,"marginalia-record",source.Id,source.Position,species:skillId);break;
                case "JackInTheBox":spotlightUntil[source.Id]=time+HeroRulesV3Tuning.SpotlightSeconds;break;
                case "LostCarousel":Emit(HeroRuleAction.CarouselRing,"arena-spins",source.Id,source.Position,HeroRulesV3Tuning.Radius,HeroRulesV3Tuning.TraitRingSeconds);break;
                case "LighthouseWhale":Emit(HeroRuleAction.Beam,"sweeping-beam",source.Id,source.Position,HeroRulesV3Tuning.ArenaRadius,HeroRulesV3Tuning.DefaultDuration,source.Definition.Weapon.Damage*strength,HeroRuleShape.Arena);break;
                case "RainbowBell":if(count%HeroRulesV3Tuning.BellEvery==0)Emit(HeroRuleAction.Counter,"third-toll",source.Id,source.Position,HeroRulesV3Tuning.ArenaRadius,value:1,species:"clock");break;
            }
            foreach(var s in spotlightUntil)if(s.Value>time)Emit(HeroRuleAction.Counter,"centre-stage",s.Key,source.Position,value:1);
        }
        public void OnCopiedSkill(UnitState source,string copiedSkillId)
        {if(source?.Definition?.Id=="EndlessIndex"&&!string.IsNullOrEmpty(copiedSkillId))run.MarginaliaSkill=copiedSkillId;}
        public void OnCherry(UnitState source,Vector2 position)
        {if(source?.Definition?.Id!="SundaeGolem")return;Emit(HeroRuleAction.Counter,"cherry-warmth",source.Id,position,value:1);Emit(HeroRuleAction.Coins,"cherry-coin",source.Id,position,count:1);}
        public void OnFeatherRevive(UnitState phoenix,UnitState revived)
        {
            if(phoenix?.Definition?.Id!="PhoenixChick"||revived==null)return;
            revived.Health=MortalityRuntime.MaxHealth(revived)*HeroRulesV3Tuning.FeatherHealth;
            Emit(HeroRuleAction.DamageBuff,"second-wind",phoenix.Id,revived.Position,duration:HeroRulesV3Tuning.FeatherSeconds,value:HeroRulesV3Tuning.FeatherDamage,friendId:revived.Id);
        }
        public void OnMelodyBeat()
        {
            if(units==null)return;
            for(int i=0;i<units.Count;i++)if(units[i].Alive&&units[i].Faction==Faction.Companion&&units[i].Definition.Id=="Metronome")
            {Emit(HeroRuleAction.RefillBar,"perfect-pitch",units[i].Id,units[i].Position,HeroRulesV3Tuning.ArenaRadius,value:HeroRulesV3Tuning.PitchRefill);break;}
        }
        public float BastionDurationScale
        {get{if(units!=null)for(int i=0;i<units.Count;i++)if(units[i].Alive&&units[i].Faction==Faction.Companion&&units[i].Definition.Id=="TowerRook")return HeroRulesV3Tuning.BastionDuration;return 1;}}
        public float PiperFillBonus(UnitState friend)
        {
            if(friend==null)return 0;float bonus=0;
            foreach(var pair in piperStacks)if(piperUntil.TryGetValue(pair.Key,out float until)&&until>time)bonus+=pair.Value*HeroRulesV3Tuning.PiperSpeed;
            return bonus;
        }
        private void Emit(HeroRuleAction action,string rule,int source,Vector2 position,float radius=0,float duration=0,float value=0,HeroRuleShape shape=HeroRuleShape.Circle,Vector2 end=default,int friendId=0,int count=0,string species=null)
        {Requested?.Invoke(new HeroRuleRequest(action,rule,source,position,radius,duration,value,shape,end,friendId,count,species));}
    }
}

