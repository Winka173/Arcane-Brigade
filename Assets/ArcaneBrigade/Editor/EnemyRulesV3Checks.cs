using System;
using System.Collections.Generic;
using UnityEngine;
namespace ArcaneBrigade.Editor
{
    public static class EnemyRulesV3Checks
    {
        private const int Crowd=100;
        private const float Health=100, Damage=10, Far=100, Spacing=4, Epsilon=.001f;
        private static void Require(bool value,string message)
        {if(!value)throw new InvalidOperationException("EnemyRulesV3: "+message);}
        private static void Near(float actual,float expected,string message)
        {Require(Mathf.Abs(actual-expected)<Epsilon,message+" ("+actual+" / "+expected+")");}
        private sealed class Fight:IDisposable
        {
            public readonly List<UnitState> Units=new List<UnitState>();
            public readonly List<EnemyRuleRequest> Events=new List<EnemyRuleRequest>();
            public readonly List<EnemyDangerRequest> Warnings=new List<EnemyDangerRequest>();
            public readonly EnemyRulesV3Runtime Runtime=new EnemyRulesV3Runtime();
            public readonly UnitState Hero,Friend,Enemy;
            private readonly List<UnityEngine.Object> assets=new List<UnityEngine.Object>();
            private T Asset<T>()where T:ScriptableObject
            {T asset=ScriptableObject.CreateInstance<T>();assets.Add(asset);return asset;}
            public Fight(string enemyId)
            {
                UnitDefinition ally=Definition("ally");
                Hero=new UnitState {Id=1,Faction=Faction.Champion,Definition=ally,Health=Health,Position=Vector2.zero};
                Friend=new UnitState {Id=2,Faction=Faction.Companion,Definition=ally,Health=Health,Position=Vector2.right};
                Units.Add(Hero);Units.Add(Friend);
                UnitDefinition def=Definition(enemyId);def.IsBoss=EnemyRulesV3Catalog.FindBoss(enemyId)!=null;
                Enemy=new UnitState {Id=3,Faction=Faction.Enemy,Definition=def,Health=Health,Position=Vector2.zero,Facing=Vector2.right};Units.Add(Enemy);
                UnitDefinition plain=Definition("Marcher");
                for(int i=1;i<Crowd;i++)Units.Add(new UnitState {Id=i+3,Faction=Faction.Enemy,Definition=plain,Health=Health,Position=new Vector2(Far+i*Spacing,Far)});
                Runtime.Requested+=value=>Events.Add(value);Runtime.DangerRequested+=value=>Warnings.Add(value);
                Runtime.ResetWave(Units,Hero,1,17);
            }
            public UnitDefinition Definition(string id)
            {UnitDefinition def=Asset<UnitDefinition>();def.Id=id;def.Health=Health;def.Speed=EnemyRulesV3Tuning.DefaultSpeed;def.Radius=EnemyRulesV3Tuning.DefaultRadiusBody;def.Weapon=Asset<WeaponDefinition>();def.Weapon.Damage=Damage;return def;}
            public int Count(EnemyRuleAction action,string rule=null)
            {int count=0;foreach(var request in Events)if(request.Action==action && (rule==null||request.Rule==rule))count++;return count;}
            public void Dispose(){foreach(var asset in assets){UnityEngine.Object.DestroyImmediate(asset);}}
        }
        public static void Run()
        {
            Catalog();Warnings();Affixes();Passives();Bosses();AllKinds();
            Debug.Log("EnemyRulesV3Checks: PASS (53 kinds, 12 affixes, 6 bosses, 100-enemy fixtures)");
        }
        private static void Catalog()
        {
            Require(EnemyRulesV3Catalog.Definitions.Length==EnemyRulesV3Tuning.KindCount,"53 enemy skills");
            Require(EnemyRulesV3Catalog.Bosses.Length==EnemyRulesV3Tuning.BossCount,"six bosses");
            Require(EnemyRulesV3Catalog.Affixes.Length==EnemyRulesV3Tuning.AffixCount,"twelve affix definitions");
            var ids=new HashSet<string>();foreach(var row in EnemyRulesV3Catalog.Definitions){Require(ids.Add(row.Id),"unique kind ids");Require(!string.IsNullOrEmpty(row.Skill),"every kind has an authored skill");}
            Require(EnemyRulesV3Catalog.BossForNight(10).Kind==EnemyBossV3.PastryTyrant,"night 10 boss");
            Require(EnemyRulesV3Catalog.BossForNight(80).Kind==EnemyBossV3.LanternQueen,"night 80 boss");
            Require(EnemyRulesV3Catalog.BossForNight(11)==null,"no invented boss night");
        }
        private static void Warnings()
        {
            using(var fight=new Fight("Smasher"))
            {
                fight.Runtime.Tick(EnemyRulesV3Tuning.DefaultPeriod);
                Require(fight.Warnings.Count==2 && fight.Events.Count==0,"slam warns damage and stun before resolution");
                Vector2 fixedPoint=fight.Warnings[0].Position;fight.Hero.Position=new Vector2(Far,Far);
                fight.Runtime.Tick(EnemyRulesV3Tuning.SlamWarning-Epsilon);
                Require(fight.Events.Count==0,"no damage before warning time");
                fight.Runtime.Tick(Epsilon);
                Require(fight.Count(EnemyRuleAction.Damage,"slam")==1,"warned slam resolves once");
                Require(fight.Events[0].Position==fixedPoint,"warning geometry does not chase a moved friend");
            }
            using(var fight=new Fight("EggLayer"))
            {
                fight.Runtime.Tick(EnemyRulesV3Tuning.DefaultPeriod);
                Require(fight.Warnings.Count==1 && fight.Runtime.CancelEgg(fight.Warnings[0].RequestId),"egg can be destroyed");
                fight.Runtime.Tick(EnemyRulesV3Tuning.EggSeconds);
                Require(fight.Count(EnemyRuleAction.Spawn,"egg")==0,"destroyed egg does not hatch");
            }
            Require(EnemyRulesV3Runtime.Contains(EnemyDangerShape.Line,Vector2.zero,new Vector2(10,0),1,new Vector2(5,.5f)),"line hits a crowd corridor");
            Require(!EnemyRulesV3Runtime.Contains(EnemyDangerShape.Line,Vector2.zero,new Vector2(10,0),1,new Vector2(5,2)),"line excludes outside friend");
        }
        private static void Affixes()
        {
            using(var fight=new Fight("Marcher"))
            {
                fight.Runtime.AssignAffixes(fight.Enemy,EnemyAffixV3.Swift|EnemyAffixV3.Armored|EnemyAffixV3.Shielded);
                Near(fight.Runtime.SpeedScale(fight.Enemy),1+EnemyRulesV3Tuning.Swift,"Swift speed");
                Require(fight.Runtime.ImmuneToKnockback(fight.Enemy),"Armored knockback immunity");
                Near(fight.Runtime.ModifyIncoming(fight.Enemy,Damage),0,"30 percent shield absorbs small hit");
                fight.Runtime.AssignAffixes(fight.Enemy,EnemyAffixV3.Reflective);
                fight.Runtime.ModifyIncoming(fight.Enemy,Damage,fight.Hero);fight.Runtime.ModifyIncoming(fight.Enemy,Damage,fight.Hero);
                Require(fight.Count(EnemyRuleAction.Reflect,"reflective-affix")==1,"Reflective once per wave");
                fight.Runtime.AssignAffixes(fight.Enemy,(EnemyAffixV3)((1<<EnemyRulesV3Tuning.AffixCount)-1));
                int flags=(int)fight.Runtime.Affixes(fight.Enemy),count=0;while(flags!=0){count+=flags&1;flags>>=1;}
                Require(count==EnemyRulesV3Tuning.MaxAffixes,"elite affixes capped at three");
            }
            using(var fight=new Fight("Marcher"))
            {
                fight.Runtime.AssignAffixes(fight.Enemy,EnemyAffixV3.Vampiric);fight.Enemy.Health=Health-Damage;
                fight.Runtime.OnDamageDealt(fight.Enemy,fight.Hero,Damage);fight.Runtime.Tick(EnemyRulesV3Tuning.AuraTick);
                Near(fight.Enemy.Health,Health-Damage+Damage*EnemyRulesV3Tuning.Vampire,"Vampiric damage accumulation");
            }
        }
        private static void Passives()
        {
            using(var fight=new Fight("Grower"))
            {fight.Runtime.Tick(EnemyRulesV3Tuning.StageSeconds);Near(fight.Runtime.MaxHealth(fight.Enemy),Health*(1+EnemyRulesV3Tuning.GrowHealth),"Grower max health stage");}
            using(var fight=new Fight("Immune")){Require(fight.Runtime.ImmuneToControl(fight.Enemy),"Immune ignores slow/stun");}
            using(var fight=new Fight("PhaseWalker")){Require(fight.Runtime.IgnoresWallsAndTaunt(fight.Enemy),"Phase Walker ignores walls/taunt");}
            using(var fight=new Fight("Splitter"))
            {fight.Runtime.OnEnemyDeath(fight.Enemy);fight.Runtime.OnEnemyDeath(fight.Enemy);Require(fight.Count(EnemyRuleAction.Spawn,"split")==1,"split death processed once");}
            using(var fight=new Fight("ShieldWall"))
            {Near(fight.Runtime.ModifyIncoming(fight.Enemy,Damage,direction:Vector2.left),0,"front shield immunity");Near(fight.Runtime.ModifyIncoming(fight.Enemy,Damage,direction:Vector2.right),Damage,"back takes full damage");}
            using(var fight=new Fight("VenomWisp"))
            {
                fight.Hero.Position=new Vector2(2,0);fight.Friend.Position=new Vector2(Far,Far);
                fight.Runtime.Tick(EnemyRulesV3Tuning.DefaultPeriod);fight.Runtime.Tick(EnemyRulesV3Tuning.DefaultWarning);
                fight.Friend.Position=new Vector2(2.5f,0);fight.Runtime.Tick(EnemyRulesV3Tuning.AuraTick);
                Require(fight.Count(EnemyRuleAction.Poison,"venom-spread")==1,"venom spreads to a touching friend after the original ring");
            }
        }
        private static void Bosses()
        {
            foreach(var boss in EnemyRulesV3Catalog.Bosses)
                using(var fight=new Fight(boss.Id))
                {
                    fight.Enemy.Health=Health*boss.SecondPhase;fight.Runtime.Tick(Epsilon);
                    Require(fight.Count(EnemyRuleAction.Phase,"boss-phase")==1,boss.Id+" phase two");
                    fight.Enemy.Health=Health*boss.FinalPhase;fight.Runtime.Tick(Epsilon);
                    Require(fight.Count(EnemyRuleAction.Phase,"boss-phase")==2,boss.Id+" final phase");
                }
            using(var fight=new Fight("ClockworkWarden"))
            {
                fight.Enemy.Health=Health*EnemyRulesV3Tuning.WardenFinal;fight.Runtime.Tick(Epsilon);
                Require(fight.Runtime.CutChain(fight.Enemy.Id),"warden chain can be cut");
                fight.Runtime.Tick(EnemyRulesV3Tuning.BossCountdown);
                Require(fight.Count(EnemyRuleAction.Countdown,"final-countdown")==0,"cut chain cancels final countdown");
            }
            using(var fight=new Fight("HollowChoir"))
            {
                fight.Enemy.Health=Health*EnemyRulesV3Tuning.OtherSecond;fight.Runtime.Tick(Epsilon);
                var singers=new UnitState[EnemyRulesV3Tuning.Singers];
                for(int i=0;i<singers.Length;i++)
                {
                    singers[i]=new UnitState { Id=Crowd+10+i,Faction=Faction.Enemy,Definition=fight.Enemy.Definition,Health=Health };
                    fight.Units.Add(singers[i]);fight.Runtime.RegisterSpawned(singers[i],fight.Enemy.Id,"choir-singers",fight.Enemy.Id);
                }
                Near(fight.Runtime.ModifyIncoming(fight.Enemy,Damage),0,"living singers protect choir");
                for(int i=0;i<singers.Length;i++)fight.Runtime.OnEnemyDeath(singers[i]);
                Near(fight.Runtime.ModifyIncoming(fight.Enemy,Damage),Damage,"simultaneous singer deaths break protection");
            }
        }
        private static void AllKinds()
        {
            foreach(var row in EnemyRulesV3Catalog.Definitions)
                using(var fight=new Fight(row.Id))
                {Require(fight.Runtime.Recognizes(fight.Enemy),row.Id+" registered");fight.Runtime.Tick(EnemyRulesV3Tuning.SummonSeconds);fight.Runtime.Tick(EnemyRulesV3Tuning.MeteorWarning);Require(fight.Runtime.DroppedWarnings==0,row.Id+" keeps warning capacity");}
        }
    }
}
