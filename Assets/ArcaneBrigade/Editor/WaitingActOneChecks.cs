using System;
using System.Collections.Generic;
using System.IO;
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
            Check(MienChoStory.Chapters.Length==10 && WaitingCampaign.Act(null)==0,"Scope remains Act I only / twenty waves");
            Debug.Log("ACT_ONE_CHECKS "+notes.Count+" PASS");
        }
    }
}
