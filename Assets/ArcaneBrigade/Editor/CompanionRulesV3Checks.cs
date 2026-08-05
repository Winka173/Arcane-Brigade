using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Deterministic section 6/7 coverage, time bars, protections and crowd geometry without scenes.</summary>
    public static class CompanionRulesV3Checks
    {
        private const int EnemyCount=100;
        private const float Health=100, WeaponDamage=10, BeforeReady=.01f, AfterReady=.02f;
        private static void Require(bool condition,string message)
        { if(!condition) throw new InvalidOperationException("CompanionRulesV3: "+message); }
        private sealed class Fixture : IDisposable
        {
            public readonly CompanionSkillRuntime Runtime=new CompanionSkillRuntime();
            public readonly List<UnitState> Units=new List<UnitState>();
            public readonly List<SkillDamageRequest> Damage=new List<SkillDamageRequest>();
            public readonly List<SkillFriendRequest> Friends=new List<SkillFriendRequest>();
            public readonly List<SkillConstructRequest> Constructs=new List<SkillConstructRequest>();
            public readonly List<SkillEnemyEffectRequest> Effects=new List<SkillEnemyEffectRequest>();
            public readonly UnitState Hero, Source, Fallen;
            public int Casts;
            private readonly UnitDefinition friend,enemy;
            private readonly WeaponDefinition weapon;
            public Fixture(string id)
            {
                weapon=ScriptableObject.CreateInstance<WeaponDefinition>(); weapon.Damage=WeaponDamage;
                friend=ScriptableObject.CreateInstance<UnitDefinition>(); friend.Id=id; friend.Health=Health; friend.Weapon=weapon;
                enemy=ScriptableObject.CreateInstance<UnitDefinition>(); enemy.Id="fixture-species"; enemy.Health=Health; enemy.Weapon=weapon;
                Hero=new UnitState { Id=1, Definition=friend, Faction=Faction.Champion, Health=Health };
                Source=new UnitState { Id=2, Definition=friend, Faction=Faction.Companion, Health=Health, Facing=Vector2.right };
                Units.Add(Hero); Units.Add(Source);
                // Extra allies use no skill table entry, allowing exact one-caster bar assertions.
                for(int i=0;i<8;i++) Units.Add(new UnitState { Id=3+i, Definition=enemy, Faction=Faction.Companion, Health=Health/2, Position=new Vector2(1,i*.1f) });
                Fallen=new UnitState { Id=11, Definition=enemy, Faction=Faction.Companion, Health=0, Alive=false, Position=Vector2.one };
                Units.Add(Fallen);
                for(int i=0;i<EnemyCount;i++) Units.Add(new UnitState { Id=20+i, Definition=enemy, Faction=Faction.Enemy, Health=Health, Position=new Vector2(1+(i%10)*.1f,(i/10)*.1f) });
                Runtime.CastRequested+=r=>Casts++;
                Runtime.DamageRequested+=r=>Damage.Add(r);
                Runtime.FriendRequested+=r=>Friends.Add(r);
                Runtime.ConstructRequested+=r=>Constructs.Add(r);
                Runtime.EnemyEffectRequested+=r=>Effects.Add(r);
                Runtime.ResetWave(Units,Hero,8);
            }
            public void Dispose()
            { UnityEngine.Object.DestroyImmediate(friend); UnityEngine.Object.DestroyImmediate(enemy); UnityEngine.Object.DestroyImmediate(weapon); }
        }
        public static void Run()
        {
            Require(CompanionSkillTable.All.Length==CompanionSkillTuning.CompanionCount,"120 authored skills");
            var ids=new HashSet<string>(); int protection=0;
            for(int i=0;i<CompanionSkillTable.All.Length;i++)
            {
                CompanionSkillDefinition skill=CompanionSkillTable.All[i];
                Require(ids.Add(skill.CompanionId),"unique skill for "+skill.CompanionId);
                Require(!string.IsNullOrEmpty(skill.Requirement) && !string.IsNullOrEmpty(skill.BodySource),"authored rule and creature source retained");
                if(skill.Protection) protection++;
                using(var f=new Fixture(skill.CompanionId))
                {
                    f.Runtime.Tick(skill.Tuning.Bar-BeforeReady);
                    Require(f.Casts==0,"bar waits for time: "+skill.CompanionId);
                    f.Runtime.Tick(AfterReady);
                    Require(f.Casts==1,"every companion casts at its authored bar: "+skill.CompanionId);
                    if(skill.Protection && skill.CompanionId!="PhoenixChick")
                        Require(f.Friends.Count>0 || f.Constructs.Count>0,"protection request: "+skill.CompanionId);
                    if(skill.CompanionId=="PillowOracle") Require(f.Effects.Count>1,"lullaby covers a crowd");
                    if(skill.CompanionId=="RainHeron") Require(f.Damage.Count>1,"spear pierces a line of enemies");
                    f.Runtime.ResetWave(f.Units,f.Hero,8);
                    Require(f.Runtime.CastCount==0 && f.Runtime.BarFraction(f.Source.Id)==0,"wave resets bars and active rules");
                }
            }
            Require(protection==CompanionSkillTuning.ProtectionCount,"27 protections");
            PauseAndRevive(); Geometry(); RandomIsDeterministic(); HistoriesAndHealing();
            Debug.Log("CompanionRulesV3Checks: PASS (120 skills, 27 protections, 100-enemy fixtures, time-only bars)");
        }
        private static void PauseAndRevive()
        {
            using(var f=new Fixture("StapleMole"))
            {
                f.Runtime.Pause(f.Source.Id,20); f.Runtime.Tick(10);
                Require(f.Casts==0 && f.Runtime.BarFraction(f.Source.Id)==0,"silence pauses time fill");
                f.Runtime.Tick(20); f.Runtime.Tick(20);
                int revives=0;
                for(int i=0;i<f.Friends.Count;i++) if(f.Friends[i].Kind==SkillFriendEffect.ReviveNow)
                { revives++; Require(f.Friends[i].Target==f.Fallen && Mathf.Abs(f.Friends[i].Value-.2f)<.001f,"mole revives fallen ally at 20%"); }
                Require(revives==1,"mole rescue once per wave");
            }
            using(var f=new Fixture("PhoenixChick"))
            {
                f.Runtime.OnFriendFell(f.Fallen); f.Runtime.OnFriendFell(f.Fallen);
                Require(f.Constructs.Count==1 && f.Constructs[0].Kind=="rebirth-feather","one collectible feather per wave");
                Require(f.Friends.Count==0,"feather does not revive before collection");
            }
        }
        private static void Geometry()
        {
            Require(CompanionSkillRuntime.Contains(SkillShape.Line,Vector2.zero,Vector2.right*10,3,new Vector2(5,0)),"line includes middle");
            Require(!CompanionSkillRuntime.Contains(SkillShape.Line,Vector2.zero,Vector2.right*10,3,new Vector2(5,3)),"line excludes side");
            Require(!CompanionSkillRuntime.Contains(SkillShape.Cone,Vector2.zero,Vector2.right,3,Vector2.left),"cone excludes behind");
        }
        private static void RandomIsDeterministic()
        {
            using(var a=new Fixture("DiceCrab")) using(var b=new Fixture("DiceCrab"))
            {
                a.Runtime.Tick(8); b.Runtime.Tick(8);
                Require(a.Damage.Count==b.Damage.Count,"seeded rolls request same crowd coverage");
                for(int i=0;i<a.Damage.Count;i++) Require(a.Damage[i].Damage==b.Damage[i].Damage,"seeded damage");
            }
        }
        private static void HistoriesAndHealing()
        {
            using(var f=new Fixture("ReelOwl"))
            {
                f.Runtime.RecordAttack(f.Hero,WeaponDamage,Vector2.right); f.Runtime.Tick(6);
                Require(f.Damage.Count>3,"reel replays the hero attack through a crowd");
                Require(f.Runtime.BarFraction(f.Source.Id)==0,"attack history gives no bar charge");
            }
            using(var f=new Fixture("HarpSwan"))
            {
                f.Runtime.Tick(8); float total=0;
                for(int i=0;i<f.Friends.Count;i++) if(f.Friends[i].Target==f.Hero && f.Friends[i].Kind==SkillFriendEffect.HealFraction) total+=f.Friends[i].Value;
                Require(Mathf.Abs(total-.08f)<.001f,"harp heals 8% on first tick");
                f.Runtime.Tick(1); f.Runtime.Tick(1); f.Runtime.Tick(1);
                total=0; for(int i=0;i<f.Friends.Count;i++) if(f.Friends[i].Target==f.Hero && f.Friends[i].Kind==SkillFriendEffect.HealFraction) total+=f.Friends[i].Value;
                Require(Mathf.Abs(total-.32f)<.001f,"harp heals 8% each second over four seconds");
            }
            using(var f=new Fixture("GrandSprout"))
            {
                f.Runtime.Tick(10); Require(f.Runtime.IsRooted(f.Source.Id),"guard roots during taunt");
                f.Runtime.Tick(5); Require(!f.Runtime.IsRooted(f.Source.Id),"guard root expires");
            }
        }
    }
}
