using System;
using UnityEngine;

namespace ArcaneBrigade
{
    [Serializable]
    public sealed class WaitingCampaignState
    {
        public int Revision = 1;
        public int Act;
        public int CompletedActs;
        public int Room; // 0: ordinary arena, 1..3: explorable interludes.
        public int[] RoomChoices = new int[9];
        public int[] CompanionChoices = new int[18];
        public int BoundaryChoice;
        public int EvidenceMask;
    }
    public sealed partial class RunSnapshot { public WaitingCampaignState WaitingCampaign = new WaitingCampaignState(); }

    public static class WaitingCampaign
    {
        public static int Act(RunSnapshot state) => 0;
        public static string Title(int act) => "SOMEONE IS WAITING";
        public static string Objective(RunSnapshot state) => "Break the Customer's contract. Find who keeps renewing your name.";
        public static void Ensure(RunSnapshot state)
        {
            state.WaitingCampaign ??= new WaitingCampaignState();
            var c=state.WaitingCampaign;
            if (c.RoomChoices == null || c.RoomChoices.Length == 0) c.RoomChoices=new int[9];
            if (c.CompanionChoices == null || c.CompanionChoices.Length == 0) c.CompanionChoices=new int[18];
        }
        public static void Validate(RunSnapshot state)
        {
            var c=state?.WaitingCampaign; if(c==null)return;
            if(c.Revision!=1 || c.Act<0 || c.Act>0 || c.CompletedActs<0 || c.CompletedActs>1 || (c.CompletedActs & ((1<<c.Act)-1))!=((1<<c.Act)-1) || c.Room<0 || c.Room>3 || c.BoundaryChoice<0 || c.BoundaryChoice>2 || c.EvidenceMask<0 || c.EvidenceMask>511) throw new InvalidOperationException("Invalid Waiting Realm campaign");
            if(c.Room!=0 && (state.Phase!=SessionPhase.Shop || state.Endless || state.Secrets?.Stage==SecretEventStage.Fighting || state.OutsideStage==OutsideStageStatus.Fighting))throw new InvalidOperationException("Waiting room requires a safe shop boundary");
            foreach(var choices in new[]{c.RoomChoices,c.CompanionChoices})
                if(choices!=null && choices.Length!=0 && (choices.Length!=(ReferenceEquals(choices,c.RoomChoices)?9:18) || Array.Exists(choices,x=>x<0 || x>2)))throw new InvalidOperationException("Invalid Waiting Realm choice journal");
        }
        public static int RoomFor(int act,int wave) => wave switch
        {
            8 or 9 => 1,
            14 or 15 => 3,
            16 or 17 => 2,
            _ => 0
        };
        public static string RoomName(int room) => room switch {1=>"THE STATION WITHOUT ARRIVALS",2=>"THE PERFECT ROOM",3=>"THE EXTRA FOOTSTEPS",4=>"THE LAST CHAIR",_=>"THE CROSSING"};
    }

    public sealed partial class RunSession
    {
        public bool ChooseWaitingCompanion(int route,int option)
        {
            if(State.Phase!=SessionPhase.Shop || route<0 || route>=6 || option<1 || option>2)return false;
            WaitingCampaign.Ensure(State);
            if(!State.Roster.Exists(x=>x.UnitId==WaitingCompanionQuests.Routes[route].Id))return false;
            int step=WaitingCompanionQuests.Next(State,route);
            if(step<0 || State.Wave<WaitingCompanionQuests.Waves[step])return false;
            State.WaitingCampaign.CompanionChoices[route*3+step]=option; return true;
        }
        public bool ChooseWaitingRoom(int index,int option)
        {
            WaitingCampaign.Ensure(State); int room=State.WaitingCampaign.Room;
            if(State.Phase!=SessionPhase.Shop || room<1 || room>3 || index<0 || index>2 || option<1 || option>2)return false;
            int key=(room-1)*3+index;
            if(State.WaitingCampaign.RoomChoices[key]!=0)return false;
            State.WaitingCampaign.RoomChoices[key]=option; State.WaitingCampaign.EvidenceMask|=1<<key;
            if(room==4 && index==2)State.WaitingCampaign.BoundaryChoice=option;
            return true;
        }
    }
}
