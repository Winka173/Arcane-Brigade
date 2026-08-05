using System;
using System.Collections.Generic;
using UnityEngine;
namespace ArcaneBrigade.Editor
{
    public static class RulesV3IntegrationChecks
    {
        private const int Enemies=100;
        private const float Health=100, Lethal=10000, Frame=.1f;
        private static void Require(bool value,string message)
        { if(!value)throw new InvalidOperationException("RulesV3 integration: "+message); }
        public static void Run()
        {
            var assets=new List<UnityEngine.Object>();
            try
            {
                WeaponDefinition weapon=ScriptableObject.CreateInstance<WeaponDefinition>();assets.Add(weapon);
                weapon.Id="FixtureWeapon";weapon.Kind=WeaponKind.Contact;weapon.Damage=0;weapon.Cooldown=1;
                UnitDefinition hero=ScriptableObject.CreateInstance<UnitDefinition>();assets.Add(hero);
                hero.Id="FixtureHero";hero.Weapon=weapon;hero.Health=Health;hero.Radius=.25f;
                UnitDefinition companion=ScriptableObject.CreateInstance<UnitDefinition>();assets.Add(companion);
                companion.Id="NurseHen";companion.Weapon=weapon;companion.Health=Health;companion.Radius=.25f;
                UnitDefinition enemy=ScriptableObject.CreateInstance<UnitDefinition>();assets.Add(enemy);
                enemy.Id="Marcher";enemy.Weapon=weapon;enemy.Health=Health;enemy.Radius=.25f;enemy.ContactDamage=0;
                CheckpointCatalog catalog=ScriptableObject.CreateInstance<CheckpointCatalog>();assets.Add(catalog);
                catalog.Champion=hero;catalog.StartingCompanions=new[]{companion};catalog.ExplicitStartingRoster=true;
                catalog.MeleeEnemy=catalog.RangedEnemy=enemy;catalog.ArenaSize=new Vector2(100,100);
                catalog.WaveDuration=100;catalog.RunWave=1;catalog.EnemyBudget=Enemies;
                catalog.CompanionLeash=2;catalog.CompanionRoamInnerRadius=1;catalog.CompanionRoamOuterRadius=2;
                catalog.CompanionRetargetMin=1;catalog.CompanionRetargetMax=2;
                catalog.RuntimeBonds=BondV3Catalog.CreateDefinitions();catalog.RuntimeBondCounts=new Dictionary<string,int>();
                var simulation=new CombatSimulation(catalog);simulation.Start(1979);
                UnitState friend=simulation.Units[1];
                for(int i=0;i<Enemies;i++)simulation.AddFixture(enemy,Faction.Enemy,new Vector2(20+i%10,20+i/10));
                Require(simulation.Mortality.FriendCount==2,"enemy bodies excluded from mortality");
                simulation.DamageFixture(friend,Lethal);
                Require(!friend.Alive && simulation.Champion.Alive && simulation.Phase==RunPhase.Combat,"companion takes normal combat damage and falls");
                simulation.Step(Frame,Vector2.zero);
                Require(simulation.UnitForPresentation(friend.Id)==friend,"fallen companion retained in unit lookup");
                simulation.Mortality.Tick(MortalityTuning.RiseSeconds);
                Require(friend.Alive && Mathf.Abs(friend.Health-Health*MortalityTuning.RiseHealthFraction)<.001f,"revival restores the actual combat body");
                simulation.DamageFixture(friend,Lethal);
                for(int i=0;i<Enemies && simulation.Champion.Alive;i++)simulation.DamageFixture(simulation.Champion,Lethal);
                Require(simulation.Phase==RunPhase.Defeat && simulation.Mortality.Lost,"normal damage loses only when every friend falls");
                simulation.Start(1979);
                Require(simulation.Champion.Alive && simulation.Units[1].Alive && simulation.Units[1].Health==Health,"wave restart restores full team health");
                catalog.RuntimeBondCounts["bulwark"]=2;
                simulation.Start(1979);
                friend=simulation.Units[1];simulation.DamageFixture(friend,Health*BondTuning.BulwarkShield);
                Require(friend.Health==Health,"wave-start shield survives zero authored duration");
                simulation.DamageFixture(friend,Health);
                Require(!friend.Alive,"spent shield does not prevent subsequent lethal hit");
                hero.Id="RootOracle";simulation.Start(1979);
                Require(simulation.HeroRulesV3.HeroId=="RootOracle" && !simulation.HeroRulesV3.CanMove,"hero wave-start requests can resolve before enemy runtime is created");
                var heroProgress=new HeroRulesV3RunState();heroProgress.Species.Add("Marcher");heroProgress.Relics.Add("RootOracle-2");heroProgress.SeedAges[1]=3;
                heroProgress.OnBeforeSerialize();heroProgress.Species.Clear();heroProgress.Relics.Clear();heroProgress.SeedAges.Clear();heroProgress.OnAfterDeserialize();
                Require(heroProgress.Species.Contains("Marcher") && heroProgress.Relics.Contains("RootOracle-2") && heroProgress.SeedAges[1]==3,"hero run collections survive serialization callbacks");
                var bondProgress=new BondRunProgress();bondProgress.Species.Add("Marcher");bondProgress.OnBeforeSerialize();bondProgress.Species.Clear();bondProgress.OnAfterDeserialize();
                Require(bondProgress.Species.Contains("Marcher"),"recorded bond species survive serialization callbacks");
                simulation.Mortality.DefeatAll();
                Require(simulation.Phase==RunPhase.Defeat,"unanswered final countdown defeats the complete friend roster");
                BondDefinition hearth=Array.Find(catalog.RuntimeBonds,b=>b.Id=="hearth");
                Require(Mathf.Abs(hearth.Reward(9)-hearth.Reward(8))<.001f,"spare ninth member does not create a fifth major step");
                Require(Mathf.Abs(hearth.Reward(2)-BondTuning.V3Definitions[0].Value)<.001f,"authored v3 stats are not reduced by legacy balance scaling");
                Debug.Log("RulesV3IntegrationChecks: PASS (100 enemies, combat damage, retention, revival, defeat, restart, canonical stats)");
            }
            finally { foreach(UnityEngine.Object asset in assets)UnityEngine.Object.DestroyImmediate(asset); }
        }
    }
}
