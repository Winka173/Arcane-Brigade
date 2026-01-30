using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    // Separate, walkable interludes. Saves the same run at a shop boundary; no combat rewards.
    public sealed class WaitingRoomController : MonoBehaviour
    {
        private RunSession session;
        private RunStorage storage;
        private RunCatalog content;
        private Camera view;
        private SpriteRenderer traveller,backdrop,chair,window;
        private SpriteRenderer[] friends,markers;
        private FloatingStoryProp[] floatingRecords;
        private FloatingStoryProp floatingChair;
        private Vector2 chairFloor=new Vector2(-4,2.6f);
        private TextMesh[] labels;
        private Material floorMaterial;
        private AudioSource sound;
        private int room,selected=-1;
        private float clock,nextFootstep;
        private Vector2 position=new Vector2(0,-3.8f);
        private readonly Vector2[] points={new Vector2(-4,1.8f),new Vector2(0,2.8f),new Vector2(4,1.8f)};
        private GUIStyle heading,copy,button,small;
        private Vector2 scroll;
        private string error="";
        private bool paused;
        public RunSession Session=>session;
        public int Room=>room;
        public int Selection=>selected;
        public Vector2 Position=>position;
        public bool Paused=>paused;
        public float Clock=>clock;
        private bool Modal=>selected>=0 || paused;
        private void Start()
        {
            storage=new RunStorage(RunPaths.Root);content=Resources.Load<RunCatalog>("RunCatalog");session=new RunSession(content);
            var state=storage.LoadRun(session);
            if(state==null || state.WaitingCampaign?.Room<1){RunLaunch.Resume=true;SceneManager.LoadScene("RunArena");return;}
            session.Restore(state);WaitingCampaign.Ensure(session.State);room=session.State.WaitingCampaign.Room;
            PresentationSettings.Apply(storage.LoadProfile());
            view=Camera.main;if(view==null){var go=new GameObject("JourneyRoomCamera");go.tag="MainCamera";view=go.AddComponent<Camera>();}
            view.clearFlags=CameraClearFlags.SolidColor;view.orthographic=true;view.orthographicSize=6.94f;view.transform.position=new Vector3(0,0,-10);view.backgroundColor=new Color(.015f,.02f,.035f);
            if(FindAnyObjectByType<AudioListener>()==null)view.gameObject.AddComponent<AudioListener>();
            AudioListener.pause=false;AudioListener.volume=storage.LoadProfile().Volume;
            sound=gameObject.AddComponent<AudioSource>();sound.playOnAwake=false;
            backdrop=Make("A Room With An Inside Release",Resources.Load<Sprite>("MienCho/Journey/"+WaitingRooms.Art(room)+"_Painted_v1"),Vector2.zero,18.5f,-100);
            var material=Resources.Load<Material>("MienCho/JourneyRoomLight");if(material!=null){floorMaterial=new Material(material);backdrop.sharedMaterial=floorMaterial;}
            traveller=Make("The Traveller",content.Unit(state.ChampionId).Body,position,1.1f,50);
            int count=Mathf.Min(15,state.Roster.Count);friends=new SpriteRenderer[count];
            for(int i=0;i<count;i++)friends[i]=Make("A Living Companion",content.Unit(state.Roster[i].UnitId).Body,position+Vector2.left, .9f,49);
            markers=new SpriteRenderer[3];labels=new TextMesh[3];floatingRecords=new FloatingStoryProp[3];
            for(int i=0;i<3;i++)
            {
                string[] evidence={"CurioHungryLedger","CurioMuteClock","CurioManyDoorKey"};
                markers[i]=Make("A physical record",content.Item(evidence[i]).Icon,points[i],.75f,15);
                floatingRecords[i]=markers[i].gameObject.AddComponent<FloatingStoryProp>();floatingRecords[i].Configure(markers[i],251+i);
                var go=new GameObject("A Verifiable Address");go.transform.position=(Vector3)points[i]+Vector3.up*.8f;
                labels[i]=go.AddComponent<TextMesh>();labels[i].text=WaitingRooms.Label(room,i);labels[i].characterSize=.09f;labels[i].fontSize=30;labels[i].anchor=TextAnchor.MiddleCenter;labels[i].color=new Color(.9f,.83f,.65f);
                go.GetComponent<MeshRenderer>().sortingOrder=100;
            }
            chair=Make("The Chair Can Be Moved",Resources.Load<Sprite>("MienCho/EmptyChair"),new Vector2(-4,2.6f),1.2f,10);
            floatingChair=chair.gameObject.AddComponent<FloatingStoryProp>();floatingChair.Configure(chair,254);
            window=Make("A Light From Another House",content.Unit(state.ChampionId).Body,new Vector2(0,4.1f),.5f,10);window.sharedMaterial=Resources.Load<Material>("MienCho/WaitingGlow");window.transform.localScale=new Vector3(2.4f,.4f,1);
            var audio=Resources.Load<AudioClip>("MienCho/Audio/Room"+room);if(audio!=null){sound.clip=audio;sound.loop=true;sound.volume=PresentationSettings.ReducedAudio?0:PresentationSettings.MusicVolume*.22f;sound.Play();}
            ApplyChoices();DisplayProps();
        }
        private SpriteRenderer Make(string name,Sprite sprite,Vector2 point,float width,int sort)
        {
            var go=new GameObject(name);go.transform.SetParent(transform,false);var r=go.AddComponent<SpriteRenderer>();r.sprite=sprite;r.transform.position=point;r.sortingOrder=sort;
            if(sprite!=null)r.transform.localScale=Vector3.one*(width/Mathf.Max(.01f,sprite.bounds.size.x));return r;
        }
        private void Update()
        {
            if(session==null || traveller==null)return;
            if(Input.GetKeyDown(KeyCode.Escape)||Input.GetKeyDown(KeyCode.JoystickButton1)){if(selected>=0){selected=-1;scroll=Vector2.zero;}else paused=!paused;}
            for(int i=0;i<3;i++)labels[i].gameObject.SetActive(Vector2.Distance(position,points[i])<2.4f);
            if(Modal)
            {
                if(selected>=0){if(Input.GetKeyDown(KeyCode.Alpha1))Choose(1);if(Input.GetKeyDown(KeyCode.Alpha2))Choose(2);if(Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.JoystickButton0))CloseRecord();}
                sound.Pause();return;
            }sound.UnPause();clock+=Time.deltaTime;
            DisplayProps();
            var input=new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));input+=ArcadeNavigation.Pad;
            position+=Vector2.ClampMagnitude(input,1)*Time.deltaTime*3.6f;position.x=Mathf.Clamp(position.x,-6.8f,6.8f);position.y=Mathf.Clamp(position.y,-4.8f,3.5f);
            traveller.transform.position=position;traveller.flipX=input.x<0;
            for(int i=0;i<friends.Length;i++)
            {
                Vector2 target=position+new Vector2(Mathf.Cos(i*2.39996f)*(.9f+i*.035f),Mathf.Sin(i*2.39996f)*.65f);
                friends[i].transform.position=Vector2.Lerp(friends[i].transform.position,target,Time.deltaTime*4);
            }
            if(floorMaterial!=null)
            {
                var lights=new Vector4[16];lights[0]=new Vector4(position.x,position.y,4,1);
                for(int i=0;i<friends.Length;i++){var p=friends[i].transform.position;lights[i+1]=new Vector4(p.x,p.y,2.4f,1);}
                floorMaterial.SetVectorArray("_RoomLights",lights);floorMaterial.SetInt("_RoomLightCount",friends.Length+1);floorMaterial.SetFloat("_RoomAmbient",PresentationSettings.HighReadability?.55f:.3f);
            }
            if(room==3 && session.State.WaitingCampaign.RoomChoices[7]==0 && clock>=nextFootstep)
            {
                nextFootstep=clock+2.7f;
                if(!PresentationSettings.ReducedAudio)sound.PlayOneShot(Resources.Load<AudioClip>("MienCho/Audio/ExtraFootstep"),PresentationSettings.SfxVolume*.16f);
            }
            if(Input.GetKeyDown(KeyCode.E)||Input.GetKeyDown(KeyCode.R)||Input.GetKeyDown(KeyCode.JoystickButton2))for(int i=0;i<3;i++)if(TryInspect(i))break;
            if(Input.GetKeyDown(KeyCode.Tab)||Input.GetKeyDown(KeyCode.JoystickButton7))ReturnToShop();
        }
        public bool TryInspect(int index)
        {
            if(session==null || Modal || index<0 || index>2 || Vector2.Distance(position,points[index])>1.65f)return false;
            selected=index;scroll=Vector2.zero;
            if(!PresentationSettings.ReducedAudio && room==4)sound.PlayOneShot(Resources.Load<AudioClip>("MienCho/Audio/Stay"),PresentationSettings.SfxVolume*.25f);
            return true;
        }
        public bool Choose(int option)
        {
            if(selected<0 || option<1 || option>2)return false;
            var before=JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(session.State));
            if(!session.ChooseWaitingRoom(selected,option))return false;
            if(!Save()){session.Restore(before);return false;}
            ApplyChoices();return true;
        }
        private bool Save()
        {try{storage.SaveRun(session.State);error="";return true;}catch(Exception e){error="Could not save your choice. Try again before leaving. "+e.Message;return false;}}
        public void CloseRecord(){selected=-1;scroll=Vector2.zero;}
        public bool ReturnToShop()
        {
            if(session==null)return false;session.State.WaitingCampaign.Room=0;
            if(!Save()){session.State.WaitingCampaign.Room=room;return false;}
            RunLaunch.Resume=true;SceneManager.LoadScene("RunArena");return true;
        }
        private void ApplyChoices()
        {
            var c=session.State.WaitingCampaign;
            for(int i=0;i<3;i++){bool done=c.RoomChoices[(room-1)*3+i]>0;markers[i].color=done?new Color(.35f,.95f,.8f):new Color(.95f,.55f,.28f);labels[i].text=WaitingRooms.Label(room,i)+(done?" / RECORDED":" / E");}
            chair.enabled=room==2;
            if(room==2){chairFloor=c.RoomChoices[3]==1?new Vector2(-3,-1):c.RoomChoices[3]==2?new Vector2(2,0):new Vector2(-4,2.6f);}
            window.color=c.RoomChoices[4]>0?new Color(.1f,.7f,1,.7f):new Color(1,.45f,.1f,.35f);
            if(room==1){labels[1].text=c.RoomChoices[1]==1?"COAST / VERIFIED ROUTE":c.RoomChoices[1]==2?"REED SETTLEMENT / VERIFIED ROUTE":"WHEN YOU ARE GOOD ENOUGH";}
            DisplayProps();
        }
        private void DisplayProps()
        {
            float depth=NightmareRules.Depth(session.State.Wave);
            for(int i=0;i<3;i++)floatingRecords[i].Display(points[i],clock,depth);
            floatingChair.Display(chairFloor,clock,depth);
        }
        public void DiagnosticApproach(int index)
        {if(!MienChoCapture.Requested)throw new InvalidOperationException("Isolated capture required");position=points[index]+Vector2.down*.3f;}
        private void OnGUI()
        {
            if(session==null || view==null)return;
            var matrix=ArcadeTypography.BeginViewport(view.pixelRect,1920,1080);
            heading??=ArcadeTypography.Style(32,true);copy??=ArcadeTypography.Style(25);copy.wordWrap=true;
            button??=ArcadeTypography.Style(22,true);button.wordWrap=true;button.alignment=TextAnchor.MiddleCenter;small??=ArcadeTypography.Style(20);small.wordWrap=true;
            ArcadeSkin.Plate(new Rect(45,20,1830,105),ArcadeSkin.Teal);
            GUI.Label(new Rect(75,28,1750,48),WaitingCampaign.RoomName(room),heading);
            GUI.Label(new Rect(75,80,1750,42),"Walk: WASD / arrows / stick. Examine nearby records: E / R / X. Return: TAB / START. Pause: ESC / B.",small);
            if(GUI.Button(new Rect(1450,955,410,70),"RETURN TO THE SHOP",button))ReturnToShop();
            string consequences="";
            for(int i=0;i<6;i++){var q=session.State.WaitingCampaign.CompanionChoices;if(q[i*3+2]>0)consequences+=WaitingCompanionQuests.Routes[i].Outcomes[4+q[i*3+2]-1]+"\n";}
            if(!Modal && consequences.Length>0){ArcadeSkin.Plate(new Rect(60,750,1300,200),ArcadeSkin.Teal);GUI.Label(new Rect(80,770,1260,170),consequences,small);}
            if(paused){RunHud.Fill(new Rect(450,390,1020,250),new Color(.02f,.03f,.06f));GUI.Label(new Rect(500,420,900,70),"PAUSED / THE ROOM WAITS",heading);if(GUI.Button(new Rect(650,530,620,65),"RESUME",button))paused=false;}
            if(selected>=0)
            {
                RunHud.Fill(new Rect(220,220,1480,700),new Color(.025f,.03f,.055f));
                GUI.Label(new Rect(270,250,1370,70),WaitingRooms.Label(room,selected),heading);
                int choice=session.State.WaitingCampaign.RoomChoices[(room-1)*3+selected];
                string text=WaitingRooms.Question(room,selected)+(choice>0?"\n\n"+WaitingRooms.Result(room,selected,choice):"");
                scroll=GUI.BeginScrollView(new Rect(270,350,1370,320),scroll,new Rect(0,0,1330,Mathf.Max(310,copy.CalcHeight(new GUIContent(text),1310))));GUI.Label(new Rect(0,0,1310,600),text,copy);GUI.EndScrollView();
                if(choice==0)
                {
                    if(GUI.Button(new Rect(270,700,660,110),WaitingRooms.Option(room,selected,1),button))Choose(1);
                    if(GUI.Button(new Rect(970,700,660,110),WaitingRooms.Option(room,selected,2),button))Choose(2);
                }
                if(GUI.Button(new Rect(1000,840,630,55),"KEEP THE RECORD / BACK [B]",button))selected=-1;
            }
            if(error.Length>0)GUI.Label(new Rect(50,1015,1800,55),error,small);
            GUI.matrix=matrix;
        }
        private void OnDestroy(){session?.Dispose();if(floorMaterial!=null)Destroy(floorMaterial);}
    }
}
