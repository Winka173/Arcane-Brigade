using System;
using System.Collections.Generic;
using UnityEngine;
namespace ArcaneBrigade.Editor
{
    public static class BondV5RulesChecks
    {
        private static void Require(bool value,string message){if(!value)throw new InvalidOperationException("BondV5Rules: "+message);}
        public static void Run()
        {
            WeaponDefinition weapon=ScriptableObject.CreateInstance<WeaponDefinition>();
            UnitDefinition definition=ScriptableObject.CreateInstance<UnitDefinition>();
            UnitDefinition enemyDefinition=ScriptableObject.CreateInstance<UnitDefinition>();
            try
            {
                weapon.Damage=10;definition.Health=100;definition.Weapon=weapon;definition.Tags=new[]{"gear","clock","rune"};enemyDefinition.Health=10000;
                UnitState friend=new UnitState{Id=1,Faction=Faction.Companion,Definition=definition,Health=100};
                UnitState hero=new UnitState{Id=2,Faction=Faction.Champion,Definition=enemyDefinition,Health=100};
                var units=new List<UnitState>{friend,hero};
                for(int i=0;i<100;i++)units.Add(new UnitState{Id=3+i,Faction=Faction.Enemy,Definition=enemyDefinition,Health=10000,Position=new Vector2(i*4,0)});
                BondRuntime runtime=new BondRuntime(BondV3Catalog.CreateDefinitions(),new Dictionary<string,int>{{"gear",4},{"clock",6},{"rune",6}});
                runtime.ResetWave(hero,units);
                for(int i=0;i<BondV5Tuning.CogCap-1;i++)runtime.OnBonusShotHit(friend);
                Require(runtime.Cogs==BondV5Tuning.CogCap-1,"bonus shots build Cogs");
                runtime.OnBonusShotHit(friend);Require(runtime.Cogs==0,"ten Cogs spent");
                bool filled=false;foreach(BondSupportRequest request in runtime.SupportRequests)if(request.Rule=="cog-fill"&&request.Amount==BondV5Tuning.CogFill)filled=true;
                Require(filled,"Cog payoff fills bars");
                for(int i=0;i<BondTuning.EchoEvery;i++)runtime.OnCast(friend);
                Require(runtime.Springs>0,"Clockwork echoes build Springs");
                runtime.Tick(BondTuning.ClockStopPeriod);Require(runtime.Springs==0,"time stop spends Springs");
                int glyphs=0;runtime.ZoneRequested+=request=>{if(request.Rule=="glyph")glyphs++;};
                int runes=0;runtime.DamageRequested+=request=>{if(request.Rule=="rune")runes++;};
                for(int i=0;i<BondTuning.RuneSixEvery*2;i++)runtime.OnHit(friend,units[2],10,false);
                Require(glyphs==1,"first rune builds Glyph; next consumes it");
                runtime.ResetWave(hero,units);Require(runtime.Cogs==0&&runtime.Springs==0,"wave resources reset");
                definition.Tags=new[]{"ember"};
                runtime=new BondRuntime(BondV3Catalog.CreateDefinitions(),new Dictionary<string,int>{{"ember",8}});
                runtime.ResetWave(hero,units);int infernoBursts=0;
                runtime.DamageRequested+=request=>{if(request.Rule=="inferno-burst")infernoBursts++;};
                for(int enemy=2;enemy<102;enemy++)for(int hit=0;hit<BondV5Tuning.HighBurnStacks;hit++)runtime.OnHit(friend,units[enemy],10,false);
                runtime.Tick(.01f);units[3].Position=units[2].Position+Vector2.right;runtime.Tick(BondV5Tuning.BurstCooldown);
                for(int hit=0;hit<BondV5Tuning.HighBurnStacks;hit++)runtime.OnHit(friend,units[2],10,false);
                Require(infernoBursts>0,"Inferno spends built Heat on neighbour bursts");
                definition.Tags=new[]{"tidal"};runtime=new BondRuntime(BondV3Catalog.CreateDefinitions(),new Dictionary<string,int>{{"tidal",8}});
                int leviathans=0;runtime.SpawnRequested+=request=>{if(request.Kind==BondSpawnKind.Leviathan)leviathans++;};runtime.ResetWave(hero,units);
                Require(leviathans==0,"Leviathan waits for built pools");
                for(int hit=0;hit<BondTuning.TideEvery;hit++)runtime.OnHit(friend,units[2],10,false);
                runtime.Tick(.01f);Require(leviathans==1,"largest pool releases Leviathan");Require(runtime.State(units[101]).SoakedUntil>runtime.Time,"Leviathan Soaks whole screen");
                definition.Tags=new[]{"melody"};runtime=new BondRuntime(BondV3Catalog.CreateDefinitions(),new Dictionary<string,int>{{"melody",8}});runtime.ResetWave(hero,units);
                int notes=0;runtime.SupportRequested+=request=>{if(request.Rule=="orchestra-shield-note")notes++;};
                for(int cast=0;cast<BondTuning.MelodyEvery-1;cast++)runtime.OnCast(friend);
                Require(notes==0,"Orchestra waits for Beat");runtime.OnCast(friend);Require(notes>0,"Beat releases Orchestra notes");
                definition.Tags=new[]{"archive"};runtime=new BondRuntime(BondV3Catalog.CreateDefinitions(),new Dictionary<string,int>{{"archive",6}});runtime.ResetWave(hero,units);
                int chapters=0;runtime.SupportRequested+=request=>{if(request.Rule=="chapter")chapters++;};
                for(int page=0;page<BondV5Tuning.ChapterPages;page++){enemyDefinition.Id="species-"+page;runtime.OnHit(friend,units[2+page],10,false);runtime.OnKill(units[2+page]);if(page<BondV5Tuning.ChapterPages-1)Require(chapters==0,"Pages build before Chapter");}
                Require(chapters==BondV5Tuning.ChapterPages&&runtime.Pages==0,"Chapter releases and spends exactly three Pages");
                definition.Tags=new[]{"desperado"};runtime=new BondRuntime(BondV3Catalog.CreateDefinitions(),new Dictionary<string,int>{{"desperado",4}});runtime.ResetWave(hero,units);
                int shots=0;runtime.DamageRequested+=request=>{if(request.Rule=="desperado-volley")shots++;};
                for(int hit=0;hit<BondTuning.DesperadoHighEvery;hit++)runtime.OnHit(friend,units[5],10,false);
                Require(shots==BondTuning.DesperadoShots,"Desperado emits exactly five forty-percent shots");
                units[2].Alive=false;runtime.OnGeneratedKill("desperado-volley",friend,units[2],4);Require(shots==BondTuning.DesperadoShots+1,"Desperado shot kill fires one additional shot");units[2].Alive=true;
                definition.Tags=new[]{"sniper"};runtime=new BondRuntime(BondV3Catalog.CreateDefinitions(),new Dictionary<string,int>{{"sniper",4}});runtime.ResetWave(hero,units);
                int carries=0;runtime.DamageRequested+=request=>{if(request.Rule=="sniper-carry")carries++;};
                friend.Position=Vector2.zero;units[2].Position=new Vector2(8,0);units[3].Position=new Vector2(10,0);units[2].Alive=false;
                runtime.OnGeneratedKill("ordinary",friend,units[2],10);Require(carries>0,"Distant Sniper kill carries through its line");units[2].Alive=true;
                definition.Tags=new[]{"clover"};runtime=new BondRuntime(BondV3Catalog.CreateDefinitions(),new Dictionary<string,int>{{"clover",4}});runtime.ResetWave(hero,units);
                for(int clover=0;clover<BondV5Tuning.CloversBloom-1;clover++)runtime.OnPickup(BondSpawnKind.Clover);
                BondRunProgress progress=runtime.CaptureProgress();runtime.ResetWave(hero,units);Require(runtime.HeldClovers==BondV5Tuning.CloversBloom-1,"Clovers held across waves");
                runtime=new BondRuntime(BondV3Catalog.CreateDefinitions(),new Dictionary<string,int>{{"clover",4}},progress);runtime.OnCloverSpent();Require(runtime.HeldClovers==BondV5Tuning.CloversBloom-2,"Shop reroll spends persisted Clover");
                runtime.ResetWave(hero,units);runtime.OnPickup(BondSpawnKind.Clover);runtime.OnPickup(BondSpawnKind.Clover);Require(progress.PendingFourLeaves==1&&runtime.HeldClovers==0,"Four-Leaf payoff persists for next shop");

            }
            finally{UnityEngine.Object.DestroyImmediate(weapon);UnityEngine.Object.DestroyImmediate(definition);UnityEngine.Object.DestroyImmediate(enemyDefinition);}
            Debug.Log("BondV5RulesChecks: PASS");
        }
    }
}
