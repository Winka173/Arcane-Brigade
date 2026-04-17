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
        private UnitDefinition[] friendDefinitions;
        private Vector2[] friendPositions;
        private UnitDefinition heroDefinition;
        private FloatingStoryProp[] floatingRecords;
        private FloatingStoryProp floatingChair;
        private Vector2 chairFloor=new Vector2(-4,2.6f);
        private InkedText[] labels;
        private Material floorMaterial;
        private readonly Vector4[] roomLights=new Vector4[16];
        private readonly float[] roomLightRadius=new float[15];
        private AudioSource sound, sfx;
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
        // Act I, shop 16: the perfect room where Hob sat down in a chair of its own (Docs/Mien_Cho_Act_One_Story.md)
        private bool rescue,rescued,pulling;
        private SpriteRenderer hob,hobChair;
        private FloatingStoryProp floatingHobChair;
        private SpriteRenderer[] seated=new SpriteRenderer[0];
        private Sprite turnedHead;
        private readonly Vector2 hobFloor=new Vector2(0,1.5f);
        private Vector2 hobSeat;
        private float pull,helperAt,risenAt=-1;
        private int helpers;
        private GUIStyle hand;
        public const float PullSeconds=2.4f;
        public bool Rescue=>rescue;
        public bool Rescued=>rescued;
        public int Helpers=>helpers;
        private void Start()
        {
            storage=new RunStorage(RunPaths.Root);content=Resources.Load<RunCatalog>("RunCatalog");session=new RunSession(content);
            var state=storage.LoadRun(session);
            if(state==null || state.WaitingCampaign?.Room<1){RunLaunch.Resume=true;SceneManager.LoadScene("RunArena");return;}
            session.Restore(state);WaitingCampaign.Ensure(session.State);room=session.State.WaitingCampaign.Room;
            // Room 2 at the Act I rescue boundary remains a rescue scene after its saved success.
            rescue=ActOneCanon.StoryAlliesTravel && room==2 && session.State.Wave==16 && !session.State.Endless; // the Hob rescue is gone with the story allies (20/10/2026)
            rescued=rescue && (session.State.ActOne?.HobRescued ?? 0)>0;
            if(rescued)risenAt=-10;
            PresentationSettings.Apply(storage.LoadProfile());
            view=Camera.main;if(view==null){var go=new GameObject("JourneyRoomCamera");go.tag="MainCamera";view=go.AddComponent<Camera>();}
            view.clearFlags=CameraClearFlags.SolidColor;view.orthographic=true;view.orthographicSize=6.94f;view.transform.position=new Vector3(0,0,-10);view.backgroundColor=new Color(.015f,.02f,.035f);
            if(FindAnyObjectByType<AudioListener>()==null)view.gameObject.AddComponent<AudioListener>();
            AudioListener.pause=false;AudioListener.volume=storage.LoadProfile().Volume;
            sound=AudioMix.Route(gameObject.AddComponent<AudioSource>(),MixBus.Music);sound.playOnAwake=false;sfx=AudioMix.Route(gameObject.AddComponent<AudioSource>(),MixBus.Sfx);sfx.playOnAwake=false;
            // The composed Hob image belongs to the journal; the live rescue needs movable actors over the empty room.
            backdrop=Make("A Room With An Inside Release",Resources.Load<Sprite>("MienCho/Journey/"+WaitingRooms.Art(room)+"_Painted_v1"),Vector2.zero,18.5f,-100);
            var material=Resources.Load<Material>("MienCho/JourneyRoomLight");if(material!=null){floorMaterial=new Material(material);backdrop.sharedMaterial=floorMaterial;}
            heroDefinition=content.Unit(state.ChampionId);
            traveller=Make("The Traveller",heroDefinition.Body,position,1.1f,50);
            // in the perfect room the travelling companions come first: they are the ones who pull
            var bodies=new System.Collections.Generic.List<Sprite>();
            var definitions=new System.Collections.Generic.List<UnitDefinition>();
            var names=new System.Collections.Generic.List<string>();
            if(rescue)for(int i=1;i<ActOneCanon.StoryIds.Length;i++)if(ActOneCanon.StoryPresent(i,state.Wave,true,state.Endless) && content.Unit(ActOneCanon.StoryIds[i])!=null)
            { bodies.Add(ActOneArtwork.Picture(ActOneCanon.StoryNames[i]) ?? content.Unit(ActOneCanon.StoryIds[i]).Body);definitions.Add(content.Unit(ActOneCanon.StoryIds[i]));names.Add(ActOneCanon.StoryNames[i]); }
            var deployed = CompanionDeployment.Select(state.Roster);
            for(int i=0;i<deployed.Count;i++)
            { var definition=content.Unit(deployed[i].UnitId);bodies.Add(definition.Body);definitions.Add(definition);names.Add(definition.DisplayName); }
            int count=bodies.Count;friends=new SpriteRenderer[count];
            friendDefinitions=definitions.ToArray();friendPositions=new Vector2[count];
            for(int i=0;i<count;i++){friendPositions[i]=position+Vector2.left;friends[i]=Make(names[i],bodies[i],friendPositions[i], .9f,49);}
            markers=new SpriteRenderer[3];labels=new InkedText[3];floatingRecords=new FloatingStoryProp[3];
            for(int i=0;i<3;i++)
            {
                string[][] evidence={new[]{"Prop_Ledger","Prop_DepartureBoard","Prop_InsideRelease"},new[]{"Prop_OrdinaryStool","Prop_InwardWindow","Prop_InsideRelease"},new[]{"Prop_HeightMarks","Prop_ExtraFootprints","Prop_WaitingNote"},new[]{"WarmChair","Prop_SewnReply","Prop_InsideRelease"}};
                markers[i]=Make("A physical record",ActOneArtwork.Picture(evidence[Mathf.Clamp(room-1,0,3)][i]),points[i],.75f,15);
                floatingRecords[i]=markers[i].gameObject.AddComponent<FloatingStoryProp>();floatingRecords[i].Configure(markers[i],251+i);
                // signs in thick inked lettering (they were the engine's default thin font)
                labels[i]=InkedText.Create(null,"A Verifiable Address",ArcadeTypography.Punch,30,.09f,100,.022f);labels[i].transform.position=(Vector3)points[i]+Vector3.up*.8f;
                labels[i].Text=WaitingRooms.Label(room,i);labels[i].Color=new Color(.95f,.88f,.68f);
            }
            chair=Make("The Chair Can Be Moved",ActOneArtwork.Picture("Prop_OrdinaryStool"),new Vector2(-4,2.6f),1.2f,10);
            floatingChair=chair.gameObject.AddComponent<FloatingStoryProp>();floatingChair.Configure(chair,254);
            window=Make("A Light From Another House",ActOneArtwork.Picture("Prop_InwardWindow"),new Vector2(0,4.1f),1.9f,10);
            if(rescue)BuildRescue();
            var audio=Resources.Load<AudioClip>("MienCho/Audio/Room"+room);if(audio!=null){sound.clip=audio;sound.loop=true;sound.volume=PresentationSettings.ReducedAudio?0:AudioMix.SourceGain(MixBus.Music)*.22f;sound.Play();}
            ApplyChoices();DisplayProps();
        }
        private void BuildRescue()
        {
            for(int i=0;i<3;i++){markers[i].enabled=false;labels[i].gameObject.SetActive(false);}
            chair.enabled=false;window.enabled=false;
            // Its own comfortable armchair is deliberately distinct from both the warm child's chair and the chosen stool in shop 17.
            Sprite own=Resources.Load<Sprite>("MienCho/EmptyChair");
            hobChair=Make("A Chair Of Hob's Own",own,hobFloor,1.7f,20);
            floatingHobChair=hobChair.gameObject.AddComponent<FloatingStoryProp>();floatingHobChair.Configure(hobChair,255);
            Rect chairInk=SpriteInkMetrics.BoundsOf(own);
            hobSeat=hobChair.transform.TransformPoint(new Vector2(chairInk.center.x,Mathf.Lerp(chairInk.yMin,chairInk.yMax,.42f)));
            var body=content.Unit(ActOneCanon.StoryIds[0]);
            hob=Make("Hob",ActOneArtwork.Picture("Hob") ?? (body!=null?body.Body:null),hobFloor+new Vector2(0,.35f),.95f,21);
            WorldSpriteGrounding.Place(hob,hobSeat);
            // the travellers who sat down line the walls, facing the bed; one will turn its head when you leave
            string[] names={"SeatedOne_A","SeatedOne_B","SeatedOne_C","SeatedOne_D","SeatedOne_E"};
            Vector2[] along={new Vector2(-6.2f,2.6f),new Vector2(6.2f,2.6f),new Vector2(-6.2f,-.4f),new Vector2(6.2f,-.4f),new Vector2(-3.4f,3.3f),new Vector2(3.4f,3.3f)};
            var list=new System.Collections.Generic.List<SpriteRenderer>();
            for(int i=0;i<along.Length;i++)
            {
                Sprite figure=Resources.Load<Sprite>("MienCho/ActOne/"+names[i%names.Length]);
                if(figure==null)continue;
                var view=Make("One Who Sat Down",figure,along[i],1.5f,18);view.flipX=along[i].x>0;view.color=new Color(.78f,.76f,.8f);list.Add(view);
            }
            seated=list.ToArray();turnedHead=Resources.Load<Sprite>("MienCho/ActOne/SeatedOne_A_Turned");
            if(rescued && seated.Length>0 && turnedHead!=null)seated[0].sprite=turnedHead;
        }
        /// <summary>One press starts the whole party's pull; the animation does not require holding a button.</summary>
        private void UpdateRescue()
        {
            if(rescued)
            {
                float age=clock-risenAt;
                Vector2 landing=hobFloor+new Vector2(-.7f,-.6f);
                Vector2 floor=Vector2.Lerp(hobSeat,landing,Mathf.SmoothStep(0,1,Mathf.Clamp01(age/.85f)));
                if(age>1.1f)floor=Vector2.Lerp(landing,position+new Vector2(-.8f,-.2f),Mathf.Clamp01((age-1.1f)/1.8f));
                RoomActorPerformance.Display(hob,content.Unit(ActOneCanon.StoryIds[0]),floor,age>1.1f?position-floor:Vector2.zero,clock,4);
                hob.color=Color.white;
                return;
            }
            bool near=Vector2.Distance(position,hobFloor)<1.9f;
            if(near && (Input.GetKeyDown(KeyCode.E)||Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.JoystickButton0)))pulling=true;
            bool hold=pulling;
            if(hold)
            {
                pull+=Time.deltaTime;
                if(clock>=helperAt && helpers<friends.Length){helpers++;helperAt=clock+.45f;CheckpointAudio.PlayInterfaceId("ui.select",.08f);}
            }
            else pull=Mathf.Max(0,pull-Time.deltaTime*.6f);
            int required=Mathf.Min(3,friends.Length);
            float share=Mathf.Clamp01(pull/PullSeconds);
            hob.color=Color.Lerp(new Color(.55f,.55f,.6f),Color.white,share*.7f);
            float strain=hold && !PresentationSettings.ReducedMotion?Mathf.Sin(clock*40)*.03f*share:0;
            WorldSpriteGrounding.Place(hob,hobSeat+new Vector2(strain,share*.12f));
            if(pull>=PullSeconds && helpers>=required)
            {
                var before=JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(session.State));
                if(!session.PullHobUp())return;
                if(!Save()){session.Restore(before);return;}
                rescued=true;pulling=false;risenAt=clock;CheckpointAudio.PlayInterfaceId("up.star3",.2f);
                if(seated.Length>0 && turnedHead!=null)seated[0].sprite=turnedHead;
            }
        }
        /// <summary>Diagnostics: pulls as if the button were held with the whole brigade beside you.</summary>
        public void DiagnosticPull(float seconds)
        {
            if(!RunPaths.Diagnostic || !rescue)return;
            position=hobFloor+Vector2.down*.9f;pull=seconds;helpers=friends.Length;UpdateRescue();
        }
        private SpriteRenderer Make(string name,Sprite sprite,Vector2 point,float width,int sort)
        {
            var go=new GameObject(name);go.transform.SetParent(transform,false);var r=go.AddComponent<SpriteRenderer>();r.sprite=sprite;r.transform.position=point;r.sortingOrder=sort;
            if(sprite!=null)r.transform.localScale=Vector3.one*(width/Mathf.Max(.01f,SpriteInkMetrics.BoundsOf(sprite).width));return r;
        }
        private void Update()
        {
            if(session==null || traveller==null)return;
            if(Input.GetKeyDown(KeyCode.Escape)||Input.GetKeyDown(KeyCode.JoystickButton1)){if(selected>=0){selected=-1;scroll=Vector2.zero;}else paused=!paused;}
            for(int i=0;i<3;i++)labels[i].gameObject.SetActive(!rescue && Vector2.Distance(position,points[i])<2.4f);
            if(Modal)
            {
                if(selected>=0){if(Input.GetKeyDown(KeyCode.Alpha1))Choose(1);if(Input.GetKeyDown(KeyCode.Alpha2))Choose(2);if(Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.JoystickButton0))CloseRecord();}
                sound.Pause();return;
            }sound.UnPause();clock+=Time.deltaTime;
            DisplayProps();
            var input=new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));input+=ArcadeNavigation.Pad;
            if(pulling)input=Vector2.zero;
            position+=Vector2.ClampMagnitude(input,1)*Time.deltaTime*3.6f;position.x=Mathf.Clamp(position.x,-6.8f,6.8f);position.y=Mathf.Clamp(position.y,-4.8f,3.5f);
            RoomActorPerformance.Display(traveller,heroDefinition,position,input,clock,0,pulling);
            if(rescue)UpdateRescue();
            for(int i=0;i<friends.Length;i++)
            {
                Vector2 target=position+new Vector2(Mathf.Cos(i*2.39996f)*(.9f+i*.035f),Mathf.Sin(i*2.39996f)*.65f);
                // those who have joined the pull hold on around Hob's chair
                if(rescue && !rescued && i<helpers)target=hobFloor+new Vector2(Mathf.Cos(i*1.9f+.4f)*1.05f,Mathf.Sin(i*1.9f+.4f)*.55f-.25f);
                Vector2 movement=target-friendPositions[i];friendPositions[i]=Vector2.Lerp(friendPositions[i],target,Time.deltaTime*4);
                RoomActorPerformance.Display(friends[i],friendDefinitions[i],friendPositions[i],movement,clock,i+1,pulling && i<helpers);
            }
            if(floorMaterial!=null)
            {
                // Every companion contributes; merge nearby lights into fixed room cells instead of truncating the cast.
                Array.Clear(roomLightRadius,0,roomLightRadius.Length);roomLights[0]=new Vector4(position.x,position.y,4,1);
                for(int i=0;i<friends.Length;i++)
                {
                    Vector2 p=friendPositions[i];int x=Mathf.Clamp(Mathf.FloorToInt((p.x+7)/2.8f),0,4),y=Mathf.Clamp(Mathf.FloorToInt((p.y+5)/3),0,2);
                    Vector2 centre=new Vector2(-7+(x+.5f)*2.8f,-5+(y+.5f)*3);
                    int cell=y*5+x;roomLightRadius[cell]=Mathf.Max(roomLightRadius[cell],2.4f+Vector2.Distance(p,centre));
                }
                int lightCount=1;
                for(int cell=0;cell<15;cell++)if(roomLightRadius[cell]>0)roomLights[lightCount++]=new Vector4(-7+(cell%5+.5f)*2.8f,-5+(cell/5+.5f)*3,roomLightRadius[cell],1);
                floorMaterial.SetVectorArray("_RoomLights",roomLights);floorMaterial.SetInt("_RoomLightCount",lightCount);floorMaterial.SetFloat("_RoomAmbient",PresentationSettings.HighReadability?.55f:.3f);
            }
            if(room==3 && session.State.WaitingCampaign.RoomChoices[7]==0 && clock>=nextFootstep)
            {
                nextFootstep=clock+2.7f;
                if(!PresentationSettings.ReducedAudio)sfx.PlayOneShot(Resources.Load<AudioClip>("MienCho/Audio/ExtraFootstep"),AudioMix.SourceGain(MixBus.Sfx)*.16f);
            }
            if(!rescue && (Input.GetKeyDown(KeyCode.E)||Input.GetKeyDown(KeyCode.R)||Input.GetKeyDown(KeyCode.JoystickButton2)))for(int i=0;i<3;i++)if(TryInspect(i))break;
            if(Input.GetKeyDown(KeyCode.Tab)||Input.GetKeyDown(KeyCode.JoystickButton7))ReturnToShop();
        }
        public bool TryInspect(int index)
        {
            if(session==null || Modal || index<0 || index>2 || Vector2.Distance(position,points[index])>1.65f)return false;
            selected=index;scroll=Vector2.zero;
            if(!PresentationSettings.ReducedAudio && room==4)sfx.PlayOneShot(Resources.Load<AudioClip>("MienCho/Audio/Stay"),AudioMix.SourceGain(MixBus.Sfx)*.25f);
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
            for(int i=0;i<3;i++){bool done=c.RoomChoices[(room-1)*3+i]>0;markers[i].color=done?new Color(.35f,.95f,.8f):new Color(.95f,.55f,.28f);labels[i].Text=WaitingRooms.Label(room,i)+(done?" / RECORDED":" / E");}
            chair.enabled=room==2 && !rescue;
            if(room==2){chairFloor=c.RoomChoices[3]==1?new Vector2(-3,-1):c.RoomChoices[3]==2?new Vector2(2,0):new Vector2(-4,2.6f);}
            window.color=c.RoomChoices[4]>0?new Color(.1f,.7f,1,.7f):new Color(1,.45f,.1f,.35f);
            if(room==1){labels[1].Text=c.RoomChoices[1]==1?"COAST / VERIFIED ROUTE":c.RoomChoices[1]==2?"REED SETTLEMENT / VERIFIED ROUTE":"WHEN YOU ARE GOOD ENOUGH";}
            DisplayProps();
        }
        private void DisplayProps()
        {
            float depth=NightmareRules.Depth(session.State.Wave);
            if(rescue)
            {
                floatingHobChair.Display(hobFloor,clock,depth);
                Rect ink=SpriteInkMetrics.BoundsOf(hobChair.sprite);
                hobSeat=hobChair.transform.TransformPoint(new Vector2(ink.center.x,Mathf.Lerp(ink.yMin,ink.yMax,.42f)));
                return;
            }
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
            GUI.Label(new Rect(75,28,1750,48),rescue?"THE PERFECT ROOM":WaitingCampaign.RoomName(room),heading);
            GUI.Label(new Rect(75,80,1750,42),rescue?"Walk: WASD / arrows / stick. Beside Hob, press Space / E / A once to help. Return: TAB / START. Pause: ESC / B.":"Walk: WASD / arrows / stick. Examine nearby records: E / R / X. Return: TAB / START. Pause: ESC / B.",small);
            if(rescue)DrawRescue();
            if(GUI.Button(new Rect(1450,955,410,70),"RETURN TO THE SHOP",button))ReturnToShop();
            string consequences="";
            for(int i=0;i<6;i++){var q=session.State.WaitingCampaign.CompanionChoices;if(q[i*3+2]>0)consequences+=WaitingCompanionQuests.Routes[i].Outcomes[4+q[i*3+2]-1]+"\n";}
            if(!rescue && !Modal && consequences.Length>0){ArcadeSkin.Plate(new Rect(60,750,1300,200),ArcadeSkin.Teal);GUI.Label(new Rect(80,770,1260,170),consequences,small);}
            if(paused){RunHud.Fill(new Rect(450,390,1020,250),new Color(.02f,.03f,.06f));GUI.Label(new Rect(500,420,900,70),"PAUSED / THE ROOM WAITS",heading);if(GUI.Button(new Rect(650,530,620,65),"RESUME",button))paused=false;}
            if(selected>=0)
            {
                RunHud.Fill(new Rect(220,220,1480,700),new Color(.025f,.03f,.055f));
                GUI.Label(new Rect(270,250,1370,70),WaitingRooms.Label(room,selected),heading);
                int choice=session.State.WaitingCampaign.RoomChoices[(room-1)*3+selected];
                string text=WaitingRooms.Question(room,selected)+(choice>0?"\n\n"+WaitingRooms.Result(room,selected,choice):"");
                scroll=ScrollPanel.Begin(new Rect(270,350,1370,320),scroll,new Rect(0,0,1330,Mathf.Max(310,copy.CalcHeight(new GUIContent(text),1310))));GUI.Label(new Rect(0,0,1310,600),text,copy);ScrollPanel.End();
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
        private void DrawRescue()
        {
            if(hand==null){hand=ArcadeTypography.Style(34);hand.font=ArcadeTypography.Hand;hand.alignment=TextAnchor.MiddleCenter;hand.wordWrap=true;}
            Rect card=new Rect(460,800,1000,145);
            ArcadeSkin.Ticket(card,ArcadeSkin.Paper,ArcadeSkin.Accent);
            hand.normal.textColor=new Color(.25f,.19f,.14f);
            string line=rescued?"Hob comes up out of the chair like something pulled out of deep water."
                : pulling?(helpers<Mathf.Min(3,friends.Length)?"You reach for Hob. The others come to help.":"Everyone has hold of Hob. Nobody is pulling alone.")
                : Vector2.Distance(position,hobFloor)<1.9f?"Press Space once. Reach for Hob; the others will help."
                : "Hob is sitting in a chair of its own. Dust is settling on its shoulders.";
            GUI.Label(new Rect(card.x+30,card.y+8,card.width-60,72),line,hand);
            if(!rescued)
            {
                ArcadeSkin.Line(new Rect(card.x+120,card.y+86,card.width-240,10),new Color(.1f,.08f,.08f,.6f));
                ArcadeSkin.Line(new Rect(card.x+120,card.y+86,(card.width-240)*Mathf.Clamp01(pull/PullSeconds),10),ArcadeSkin.Accent);
                Color was=small.normal.textColor;small.normal.textColor=new Color(.2f,.15f,.1f);
                GUI.Label(new Rect(card.x+120,card.y+100,card.width-240,26),helpers+" PULLING",small);
                small.normal.textColor=was;
            }
        }
        private void OnDestroy(){session?.Dispose();if(floorMaterial!=null)Destroy(floorMaterial);}
    }
}
