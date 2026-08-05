using System;
using System.Collections.Generic;
using UnityEngine;
namespace ArcaneBrigade.Editor
{
    /// <summary>Deterministic crowd and persistence fixtures; no scene or presentation required.</summary>
    public static class HeroRulesV3Checks
    {
        private static void Require(bool value,string message){if(!value)throw new InvalidOperationException("HeroesV3: "+message);}
        private static void Near(float a,float b,string message){Require(Mathf.Abs(a-b)<.001f,message);}
        public static void Run()
        {
            Require(HeroRulesV3Catalog.Definitions.Length==HeroRulesV3Tuning.HeroCount,"36 heroes");
            Require(HeroRulesV3Catalog.Traits.Length==HeroRulesV3Tuning.TraitCount,"16 traits");
            var unique=new HashSet<string>();var relics=new HashSet<string>();
            foreach(var h in HeroRulesV3Catalog.Definitions)
            {
                Require(unique.Add(h.Id),"unique hero id");Require(h.BuildDirections.Length==HeroRulesV3Tuning.BuildsPerHero,"three build directions "+h.Id);
                Require(h.Relics.Length==HeroRulesV3Tuning.RelicsPerHero,"four relics "+h.Id);
                Require(HeroRulesV3Catalog.MinorIds(h.Id).Length==2,"two hero minors");
                foreach(var r in h.Relics)Require(relics.Add(r.Id)&&!string.IsNullOrEmpty(r.Description),"relic identity/description");
            }
            var assets=new List<UnityEngine.Object>();
            try
            {
                WeaponDefinition weapon=ScriptableObject.CreateInstance<WeaponDefinition>();assets.Add(weapon);weapon.Damage=10;
                UnitDefinition heroDef=ScriptableObject.CreateInstance<UnitDefinition>();assets.Add(heroDef);heroDef.Id="ButtonCavalier";heroDef.Health=100;heroDef.Weapon=weapon;
                UnitDefinition enemyDef=ScriptableObject.CreateInstance<UnitDefinition>();assets.Add(enemyDef);enemyDef.Id="Crowd";enemyDef.Health=100;enemyDef.Weapon=weapon;
                UnitState hero=new UnitState{Id=1,Faction=Faction.Champion,Definition=heroDef,Health=100};
                var units=new List<UnitState>{hero};
                for(int i=0;i<100;i++)units.Add(new UnitState{Id=i+2,Faction=Faction.Enemy,Definition=enemyDef,Health=100,Position=new Vector2(2+(i%10)*.1f,(i/10)*.1f)});
                var requests=new List<HeroRuleRequest>();var state=new HeroRulesV3RunState();var rules=new HeroRulesV3Runtime();rules.Requested+=requests.Add;
                rules.ResetWave(hero,units,1,state);rules.Tick(HeroRulesV3Tuning.ChargePeriod,0);
                Require(requests.Exists(r=>r.Rule=="charge"&&r.Shape==HeroRuleShape.Line&&r.Value==weapon.Damage),"charge uses line geometry in 100-enemy crowd");
                heroDef.Id="ArchiveCurator";rules.ResetWave(hero,units,1,state);rules.Tick(1,0);
                Require(state.Species.Count==1,"species counted once across crowd");rules.ResetWave(hero,units,2,state);rules.Tick(1,0);Require(state.Species.Count==1,"species persist across wave");
                Near(rules.ModifyOutgoing(hero,units[1],100),101,"catalogue run damage");
                heroDef.Id="MoonProwler";rules.ResetWave(hero,units,1);rules.Tick(HeroRulesV3Tuning.ProwlStill,0);Require(rules.Untargetable,"standing still enters stealth");
                Near(rules.ModifyOutgoing(hero,units[1],10),30,"stealth triple hit");Near(rules.ModifyOutgoing(hero,units[1],10),10,"stealth consumed once");
                heroDef.Id="Postmaster";requests.Clear();rules.ResetWave(hero,units,1);rules.OnCoins(HeroRulesV3Tuning.ParcelCoins,false);Require(requests.Count==0,"passive income does not charge parcels");rules.OnCoins(HeroRulesV3Tuning.ParcelCoins);Require(requests.Exists(r=>r.Rule=="parcel-express"),"collected gold charges parcels");
                heroDef.Id="BonsaiClocksmith";rules.ResetWave(hero,units,1,state);rules.OnWaveSurvived();rules.ResetWave(hero,units,2,state);Near(rules.SkillSpeed,HeroRulesV3Tuning.PruneSpeed,"skill growth persists");
                heroDef.Id="IvorySovereign";requests.Clear();rules.ResetWave(hero,units,1);Require(requests.Exists(r=>r.Action==HeroRuleAction.Choice),"decree requests choice");Require(rules.ChooseDecree(1),"decree accepts choice");Require(!rules.ChooseDecree(2),"decree cannot silently change");Near(rules.ModifyOutgoing(hero,units[1],100),110,"damage decree");
                UnitDefinition friendDef=ScriptableObject.CreateInstance<UnitDefinition>();assets.Add(friendDef);friendDef.Id="TeaDragon";friendDef.Health=100;friendDef.Weapon=weapon;
                UnitState friend=new UnitState{Id=200,Faction=Faction.Companion,Definition=friendDef,Health=100};units.Add(friend);
                var traitState=new HeroRulesV3RunState();var traits=new PersonalTraitV3Runtime(traitState);requests.Clear();traits.Requested+=requests.Add;traits.ResetWave(units);
                for(int i=0;i<20;i++)traits.OnCoals(friend,new Vector2(i,0),10);
                Require(traitState.Coals.Count==HeroRulesV3Tuning.CoalsCap,"coals cap");Near(traitState.Coals[0].Damage,15,"coals power");requests.Clear();traits.ResetWave(units);Require(requests.Count==HeroRulesV3Tuning.CoalsCap,"coals persist next wave");
                friendDef.Id="ClockworkPiper";traits.OnSkill(friend,"Tune the Clocks");traits.OnSkill(friend,"Tune the Clocks");traits.OnSkill(friend,"Tune the Clocks");traits.OnSkill(friend,"Tune the Clocks");Near(traits.PiperFillBonus(friend),.3f,"piper capped at three");traits.Tick(HeroRulesV3Tuning.PiperSeconds);Near(traits.PiperFillBonus(friend),0,"piper expires");
                friendDef.Id="TowerRook";Near(traits.BastionDurationScale,2,"Stronghold doubles wall");Require(HeroRulesV3Catalog.ExtraMembership("PlanetHamster","gravity")==1,"Gravity Well double membership");Require(HeroRulesV3Catalog.ExtraMembership("TowerRook","spirit")==1,"Stronghold double membership");
            }
            finally{foreach(var asset in assets)UnityEngine.Object.DestroyImmediate(asset);}
            Debug.Log("HeroRulesV3Checks: PASS (36 heroes, 144 relic definitions, 16 traits, 100-enemy fixtures)");
        }
    }
}
