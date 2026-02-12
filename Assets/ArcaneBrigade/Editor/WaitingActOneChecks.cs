using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class WaitingActOneChecks
    {
        private static readonly List<string> notes=new List<string>();
        private static void Check(bool value,string label)
        {notes.Add((value?"PASS ":"FAIL ")+label);Directory.CreateDirectory("PreviewExports/ActOne");File.WriteAllLines("PreviewExports/ActOne/Checks.txt",notes);if(!value)throw new InvalidOperationException(label);}
        public static void Run()
        {
            notes.Clear();var catalog=Resources.Load<RunCatalog>("RunCatalog");
            var randomBefore=UnityEngine.Random.state;
            for(int sample=0;sample<64;sample++)
            {
                int seed=unchecked(sample*486187739-170026);
                var saved=JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(new RunSnapshot{Seed=seed}));
                var positions=new Vector2[8];bool bounded=true,separated=true,restored=true,smaller=true;
                for(int i=0;i<8;i++)
                {
                    positions[i]=NightLanternField.PositionFor(i,catalog.WorldSize,seed);
                    bounded&=Mathf.Abs(positions[i].x)<=catalog.WorldSize.x*.5f-1.25f && Mathf.Abs(positions[i].y)<=catalog.WorldSize.y*.5f-1.3f;
                    restored&=positions[i]==NightLanternField.PositionFor(i,catalog.WorldSize,saved.Seed);
                    smaller&=NightLanternField.HeightFor(i,seed)>=1.6f && NightLanternField.HeightFor(i,seed)<=1.9f;
                    for(int j=0;j<i;j++)separated&=Vector2.Distance(positions[i],positions[j])>3.5f;
                }
                Check(bounded && separated && restored && smaller,"Scattered lamps stay separated, within the map and stable across save/load seed="+seed);
                Check(Enumerable.Range(0,8).Any(i=>positions[i]!=NightLanternField.PositionFor(i,catalog.WorldSize,seed+1)),"Different runs get different lamp arrangements seed="+seed);
                Check(Enumerable.Range(0,8).Any(i=>NightLanternField.RestTiltFor(i,seed)<-4) && Enumerable.Range(0,8).Any(i=>NightLanternField.RestTiltFor(i,seed)>4),"Every run includes left and right lamp lean seed="+seed);
                for(int i=0;i<2;i++)
                {
                    var shop=NightLanternField.PositionFor(i,catalog.WorldSize,seed,true);
                    // only a wish in the shop: ShopLayout moves each lamp to the nearest spot clear of stalls, chair path and props
                    Vector2 wish=i==0?new Vector2(-5.0f,-4.72f):new Vector2(-3.55f,.55f);
                    Check(Mathf.Abs(shop.x-wish.x)<=.041f && Mathf.Abs(shop.y-wish.y)<=.061f && Vector2.Distance(shop,WorldShop.ExitPosition)>1.5f,"Shop lamps wish for open corners inside the room seed="+seed+" / "+i);
                }
            }
            Check(JsonUtility.ToJson(randomBefore)==JsonUtility.ToJson(UnityEngine.Random.state),"Lantern decoration never advances Unity's gameplay random state");
            for(int wave=1;wave<=60;wave++)
            {
                int broken=0;for(int i=0;i<8;i++)if(NightLanternField.StateFor(i,wave,false)==2)broken++;
                Check(broken==NightLanternField.BrokenFor(wave),"Deterministic lamp damage progression wave="+wave);
                Check(NightLanternField.StateFor(0,wave,true)==0 && NightLanternField.StateFor(1,wave,true)==0,"Shop retains two practical lamps wave="+wave);
            }
            foreach(string state in new[]{"Intact","Cracked","Broken"})
            {
                Check(Resources.Load<GameObject>("MienCho/Lanterns/"+state+"Runtime")==null,"Retired Blender lamp prefab excluded from player "+state);
                Check(Resources.Load<Sprite>("MienCho/Lanterns/"+state)!=null,"Drawn production lamp sprite "+state);
            }
            foreach(string name in new[]{"LanternField","NightFogVolume","NightFogDisplay","JourneyRoomLight"})Check(Resources.Load<Material>("MienCho/"+name)?.shader!=null,"Player included material "+name);
            using(var s=new RunSession(catalog))
            {
                s.Start("LanternKnight",161026);s.State.Phase=SessionPhase.Shop;s.State.Wave=18;
                s.State.Roster.Clear();foreach(var r in WaitingCompanionQuests.Routes)s.State.Roster.Add(new RosterEntry{UnitId=r.Id,WeaponId="SpoonSabre"});
                int gold=s.State.Gold,items=s.State.Items.Count;
                for(int route=0;route<6;route++)for(int stage=0;stage<3;stage++)
                {Check(s.ChooseWaitingCompanion(route,1+((route+stage)&1)),"Real optional companion choice "+route+" / "+stage);Check(WaitingCompanionQuests.Consequences(s.State,WaitingCompanionQuests.Routes[route].Id).Length>50,"Choice reaches authored consequence "+route+" / "+stage);}
                Check(s.State.Gold==gold && s.State.Items.Count==items && s.State.Roster.Count==6,"Relationships never consume currency, relics, bodies or quota");
                Check(!s.ChooseWaitingCompanion(0,1),"Completed choice cannot be farmed or rewritten");
                s.Restore(JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(s.State)));Check(WaitingCompanionQuests.Next(s.State,5)==-1,"All six routes survive save validation");
                for(int room=1;room<=3;room++)
                {s.State.WaitingCampaign.Room=room;for(int point=0;point<3;point++){Check(s.ChooseWaitingRoom(point,2),"Room response saved "+room+" / "+point);Check(!s.ChooseWaitingRoom(point,1),"Room choice cannot overwrite " +room+" / "+point);}}
                s.Restore(JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(s.State)));Check(s.State.WaitingCampaign.EvidenceMask==511,"All nine physical room records survive serialization");
                s.State.WaitingCampaign.Room=0;s.State.WaitingCampaign.CompanionChoices[0]=9;bool rejected=false;try{s.Restore(s.State);}catch(InvalidOperationException){rejected=true;}Check(rejected,"Corrupt branch choice rejected");
            }
            using(var s=new RunSession(catalog))
            {s.Start("LanternKnight",16);s.State.WaitingCampaign=null;s.Restore(JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(s.State)));WaitingCampaign.Ensure(s.State);Check(s.State.WaitingCampaign.RoomChoices.Length==9,"Legacy run without new journey fields remains loadable");}
            Check(MienChoStory.Chapters.Length==21 && WaitingCampaign.Act(null)==0 && ActOneCanon.Waves==20,"Scope remains Act I only / twenty waves");
            Debug.Log("ACT_ONE_CHECKS "+notes.Count+" PASS");
        }
    }
}
