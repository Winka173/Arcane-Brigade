using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    [DefaultExecutionOrder(-70)]
    public sealed class WaitingJourney : MonoBehaviour
    {
        private ArenaController owner;
        private GUIStyle title,copy,button,small;
        private bool nextAct;
        private int route;
        private Vector2 scroll;
        public bool Open {get;private set;}
        public int InputConsumedFrame {get;private set;}=-1;
        public void Configure(ArenaController value){owner=value;}
        private void Consume()=>InputConsumedFrame=Time.frameCount;
        public void Show()
        {
            if(owner?.Session==null || owner.StoryOpen || owner.PresentationPaused || owner.TransitionPending || owner.HallExitPending || owner.Session.SecretRoomOpen || owner.Session.State.Phase!=SessionPhase.Shop)return;
            nextAct=false;Open=true;Consume();owner.SetStoryOpen(true);
        }
        public void Close(){Open=false;Consume();owner.SetStoryOpen(false);}
        private void Update()
        {
            if(owner?.Session==null)return;
            if(!Application.isFocused && !RunPaths.Diagnostic)return;
            if(Open)
            {
                if(Input.GetKeyDown(KeyCode.Escape)||Input.GetKeyDown(KeyCode.JoystickButton1)){Close();return;}
                if(Input.GetKeyDown(KeyCode.LeftArrow)||Input.GetKeyDown(KeyCode.JoystickButton4)){route=(route+5)%6;scroll=Vector2.zero;Consume();}
                if(Input.GetKeyDown(KeyCode.RightArrow)||Input.GetKeyDown(KeyCode.JoystickButton5)){route=(route+1)%6;scroll=Vector2.zero;Consume();}
                if(!nextAct && Input.GetKeyDown(KeyCode.Alpha1))Choose(1);
                if(!nextAct && Input.GetKeyDown(KeyCode.Alpha2))Choose(2);
                float vertical=ArcadeNavigation.Pad.y;
                if(Input.GetKey(KeyCode.UpArrow))vertical=1;if(Input.GetKey(KeyCode.DownArrow))vertical=-1;
                scroll.y=Mathf.Max(0,scroll.y-vertical*Time.unscaledDeltaTime*400);return;
            }

        }
        public bool Choose(int option)
        {
            if(!Open || nextAct)return false;
            string before=JsonUtility.ToJson(owner.Session.State);
            if(!owner.Session.ChooseWaitingCompanion(route,option))return false;
            if(!owner.SaveNightmareBoundary()){owner.Session.Restore(JsonUtility.FromJson<RunSnapshot>(before));return false;}
            scroll=Vector2.zero;Consume();return true;
        }
        public bool EnterRoom()
        {
            var state=owner.Session.State;int room=WaitingCampaign.RoomFor(WaitingCampaign.Act(state),state.Wave);
            if(!Open || nextAct || room==0 || state.Endless)return false;
            WaitingCampaign.Ensure(state);state.WaitingCampaign.Room=room;
            if(!owner.SaveNightmareBoundary()){state.WaitingCampaign.Room=0;return false;}
            Close();SceneManager.LoadScene("WaitingRoom");return true;
        }
        public void Draw(Camera camera)
        {
            if(owner?.Session==null)return;
            var matrix=ArcadeTypography.BeginViewport(camera.pixelRect,1920,1080);
            title??=ArcadeTypography.Style(34,true);copy??=ArcadeTypography.Style(25);copy.wordWrap=true;
            button??=ArcadeTypography.Style(21,true);button.alignment=TextAnchor.MiddleCenter;button.wordWrap=true;
            small??=ArcadeTypography.Style(19);small.wordWrap=true;
            var state=owner.Session.State;
            if(!Open)
            {
                GUI.matrix=matrix;return;
            }
            RunHud.Fill(new Rect(0,0,1920,1080),new Color(.018f,.023f,.045f));
            GUI.Label(new Rect(70,45,1760,65),"ACT "+(WaitingCampaign.Act(state)+1)+" / "+WaitingCampaign.Title(WaitingCampaign.Act(state)),title);
            int room=WaitingCampaign.RoomFor(WaitingCampaign.Act(state),state.Wave);
            GUI.Label(new Rect(70,130,1770,80),WaitingCampaign.Objective(state),small);
            if(room>0 && !state.Endless)
            {if(Press(new Rect(70,230,1770,70),"ENTER WALKABLE ROOM / "+WaitingCampaign.RoomName(room)))EnterRoom();}
            else GUI.Label(new Rect(80,230,1740,70),"Walkable rooms open after waves 8-9, 14-15, 16-17.",small);
            for(int i=0;i<6;i++)if(Press(new Rect(70+i*295,335,280,75),owner.Content.Unit(WaitingCompanionQuests.Routes[i].Id).DisplayName)){route=i;scroll=Vector2.zero;}
            var quest=WaitingCompanionQuests.Routes[route];int stage=WaitingCompanionQuests.Next(state,route);
            RunHud.Sprite(new Rect(90,470,330,320),owner.Content.Unit(quest.Id).Body);
            GUI.Label(new Rect(80,810,350,120),quest.Title,title);
            string text=stage<0?"THEIR CHOSEN FUTURE\nThis route is complete. Their decisions remain in the room and epilogue.":"PART "+(stage+1)+" / SHOP AFTER WAVE "+WaitingCompanionQuests.Waves[stage]+"\n"+quest.Scenes[stage];
            text+=WaitingCompanionQuests.Consequences(state,quest.Id);
            float height=Mathf.Max(340,copy.CalcHeight(new GUIContent(text),1280)+20);
            scroll=ScrollPanel.Begin(new Rect(500,450,1330,340),scroll,new Rect(0,0,1290,height));GUI.Label(new Rect(0,0,1280,height),text,copy);ScrollPanel.End();
            bool ready=stage>=0 && state.Wave>=WaitingCompanionQuests.Waves[stage] && state.Roster.Exists(x=>x.UnitId==quest.Id);
            if(ready)
            {
                if(Press(new Rect(500,815,640,95),"1 / "+quest.First[stage]))Choose(1);
                if(Press(new Rect(1170,815,640,95),"2 / "+quest.Second[stage]))Choose(2);
            }
            else GUI.Label(new Rect(500,815,1300,100),stage<0?"Their story continues through the consequences you chose. No stat reward or loyalty debt.":"Travel with this companion and reach the stated shop. You can leave now and return later; the main route never requires this quest.",small);
            if(Press(new Rect(1330,960,480,65),"RETURN TO THE SHOP [ESC / B]"))Close();
            GUI.Label(new Rect(80,980,1180,60),"Paused. Choices change scenes and futures, never recruit quotas.",small);
            GUI.matrix=matrix;
        }
        private bool Press(Rect rect,string text){ArcadeSkin.Plate(rect,ArcadeSkin.Teal);return GUI.Button(rect,text,button);}
    }
}
