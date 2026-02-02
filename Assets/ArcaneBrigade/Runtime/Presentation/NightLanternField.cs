using UnityEngine;

namespace ArcaneBrigade
{
    // Eight drawn sprite fixtures, a Unity light/shadow field and a low-resolution fog pass.
    // No colliders or damage rules. Party illumination remains an independent system.
    [DefaultExecutionOrder(55)]
    public sealed class NightLanternField : MonoBehaviour
    {
        public const int Capacity=8;
        public const float MinimumHeight=1.6f,MaximumHeight=1.9f;
        private static readonly int[] BreakOrder={2,7,4,1,6,0,5,3};
        private ArenaController owner;
        private readonly SpriteRenderer[] sprites=new SpriteRenderer[8],halos=new SpriteRenderer[8];
        private readonly Vector4[] ruptures=new Vector4[8];
        private readonly float[] brightness=new float[8];
        private readonly GroundShadow[] shadows=new GroundShadow[8];
        private readonly DreamSuspension[] suspensions=new DreamSuspension[8];
        private readonly Vector4[] sources=new Vector4[8],blockers=new Vector4[8];
        private readonly Sprite[] pictures=new Sprite[3];
        private readonly int[] modelStates={-1,-1,-1,-1,-1,-1,-1,-1};
        private RenderTexture field,fog;
        private Material fieldPainter,fogPainter,fogDisplay;
        private Renderer fogView;
        private Sprite white;
        private Mesh fogMesh;
        private bool configured;
        private AudioSource wind;
        private AudioClip breakClip;
        private int wave=-1;
        private bool previousShop;
        public static NightLanternField Active {get;private set;}
        public int BrokenCount {get;private set;}
        public int LitCount {get;private set;}
        public int PracticalCount=>0; // Sprite illumination is supplied by the Unity light field.
        public float Clock {get;private set;}
        public RenderTexture FogTexture=>fog;
        public RenderTexture LightTexture=>field;
        public int FogSamples=>PresentationSettings.ReducedEffects?4:8;
        public static int BrokenFor(int wave)=>Mathf.Clamp((Mathf.Max(1,wave)-1)/4,0,4)*2;
        public static int StateFor(int index,int wave,bool shop)
        {
            if(shop && (index==0 || index==1))return 0;
            int rank=System.Array.IndexOf(BreakOrder,index),broken=BrokenFor(wave);
            return rank<broken?2:rank<broken+2 && wave>2?1:0;
        }
        // Presentation-only noise: never consume the run/shop/combat random streams.
        private static float Variation(int index,int seed,uint channel)
        {
            unchecked
            {
                uint n=(uint)seed ^ (uint)(index+1)*0x9e3779b9u ^ channel*0x85ebca6bu;
                n^=n>>16;n*=0x7feb352du;n^=n>>15;n*=0x846ca68bu;n^=n>>16;
                return (n & 0x00ffffffu)/16777215f;
            }
        }
        public static float HeightFor(int index,int seed)=>Mathf.Lerp(MinimumHeight,MaximumHeight,Variation(index,seed,3));
        public static float RestTiltFor(int index,int seed)
            =>(((index ^ seed)&1)==0?-1:1)*Mathf.Lerp(4.5f,10.5f,Variation(index,seed,4));
        private static float MotionIdentity(int index,int seed)=>211+index+Variation(index,seed,5)*997;
        public static float TiltFor(int index,int seed,float clock,bool reduced)
            =>RestTiltFor(index,seed)+(reduced?0:DreamLevitation.Tilt(clock,MotionIdentity(index,seed),false,.65f));
        public static Vector2 PositionFor(int index,Vector2 size,int seed=0,bool shop=false)
        {
            var h=size*.5f;
            Vector2 jitter=new Vector2(Variation(index,seed,1)*2-1,Variation(index,seed,2)*2-1);
            // Shop fixtures stay in their outer margins, clear of offers and interaction points.
            if(shop && index<2)return new Vector2(index==0?-7.5f:7.5f,index==0?-4.65f:-2.25f)+Vector2.Scale(jitter,new Vector2(.12f,.15f));
            Vector2 anchor=index switch
            {
                0=>new Vector2(-Mathf.Min(6,h.x-2),-3.3f),1=>new Vector2(Mathf.Min(6,h.x-2),-3.3f),
                2=>new Vector2(-Mathf.Min(6,h.x-2),2.2f),3=>new Vector2(Mathf.Min(6,h.x-2),2.2f),
                4=>new Vector2(-h.x+3,h.y-3),5=>new Vector2(h.x-3,h.y-3),
                6=>new Vector2(-h.x*.4f,-h.y+2.2f),_=>new Vector2(h.x*.4f,-h.y+2.2f)
            };
            Vector2 spread=index<4?new Vector2(1.35f,.85f):new Vector2(1.5f,.7f);
            Vector2 position=anchor+Vector2.Scale(jitter,spread);
            return new Vector2(Mathf.Clamp(position.x,-h.x+1.25f,h.x-1.25f),Mathf.Clamp(position.y,-h.y+1.3f,h.y-1.3f));
        }
        public Vector2 FixtureAnchor(int index)=>PositionFor(index,owner.WorldBounds,owner.Session.State.Seed,owner.Session.State.Phase==SessionPhase.Shop);
        public float FlameFloorDrop(int index)=>HeightFor(index,owner.Session.State.Seed)*(1.15f/2.7f);
        public void Configure(ArenaController value)
        {
            owner=value;
            var paint=Resources.Load<Material>("MienCho/LanternField");var mist=Resources.Load<Material>("MienCho/NightFogVolume");
            if(paint==null || mist==null || !paint.shader.isSupported || !mist.shader.isSupported)return;
            fieldPainter=new Material(paint);fogPainter=new Material(mist);
            field=new RenderTexture(256,192,0,RenderTextureFormat.ARGB32){name="Eight Antique Lanterns - light and soft occlusion",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};field.Create();
            fog=new RenderTexture(320,240,0,RenderTextureFormat.ARGB32){name="Night Fog - eight depth samples",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};fog.Create();
            white=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,Texture2D.whiteTexture.width,Texture2D.whiteTexture.height),Vector2.one*.5f,Texture2D.whiteTexture.width);
            string[] names={"Intact","Cracked","Broken"};
            for(int i=0;i<3;i++)pictures[i]=Resources.Load<Sprite>("MienCho/Lanterns/"+names[i]);
            for(int i=0;i<8;i++)
            {
                var go=new GameObject("AntiqueLanternSprite"+i);go.transform.SetParent(transform,false);sprites[i]=go.AddComponent<SpriteRenderer>();
                shadows[i]=go.AddComponent<GroundShadow>();shadows[i].PinCastToFoot=true;
                var air=new GameObject("LanternSuspension"+i);air.transform.SetParent(transform,false);suspensions[i]=air.AddComponent<DreamSuspension>();
                var halo=new GameObject("Flame Bloom "+i);halo.transform.SetParent(transform,false);halos[i]=halo.AddComponent<SpriteRenderer>();halos[i].sprite=white;halos[i].sharedMaterial=Resources.Load<Material>("MienCho/WaitingGlow");
            }
            var fogObject=new GameObject("Black Fog Between The Lights");fogObject.transform.SetParent(transform,false);
            fogMesh=new Mesh{name="Fog screen quad"};fogMesh.vertices=new[]{new Vector3(-.5f,-.5f,0),new Vector3(.5f,-.5f,0),new Vector3(.5f,.5f,0),new Vector3(-.5f,.5f,0)};fogMesh.uv=new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up};fogMesh.triangles=new[]{0,2,1,0,3,2};fogMesh.RecalculateBounds();fogObject.AddComponent<MeshFilter>().sharedMesh=fogMesh;fogView=fogObject.AddComponent<MeshRenderer>();fogView.sortingOrder=800;
            fogDisplay=new Material(Resources.Load<Material>("MienCho/NightFogDisplay"));fogDisplay.mainTexture=fog;fogView.sharedMaterial=fogDisplay;
            wind=gameObject.AddComponent<AudioSource>();wind.playOnAwake=false;wind.loop=true;wind.clip=Resources.Load<AudioClip>("MienCho/Audio/DarkWind");if(wind.clip!=null)wind.Play();breakClip=Resources.Load<AudioClip>("MienCho/Audio/LanternBreak");
            configured=true;Active=this;
        }
        private void LateUpdate()
        {
            if(!configured || owner?.Simulation==null)return;
            bool allowed=!owner.Session.SecretRoomOpen && !owner.NightmareEndingVisible && owner.Session.State.Phase!=SessionPhase.Defeat;
            if(!allowed){wind.Pause();SetVisible(false);Shader.SetGlobalFloat("_NightLanternEnabled",0);return;}
            if(owner.PresentationPaused && wave>=0){wind.Pause();return;}
            wind.UnPause();
            SetVisible(true);Clock=owner.PresentationClock;
            int current=owner.Session.State.Wave;bool shop=owner.Session.State.Phase==SessionPhase.Shop;
            bool changed=current!=wave || shop!=previousShop;
            if(wave>0 && current>wave && BrokenFor(current)>BrokenFor(wave) && !PresentationSettings.ReducedAudio && breakClip!=null)wind.PlayOneShot(breakClip,PresentationSettings.SfxVolume*.2f);
            wave=current;previousShop=shop;wind.volume=PresentationSettings.ReducedAudio?0:PresentationSettings.SfxVolume*(shop?.015f:Mathf.Lerp(.025f,.13f,NightmareRules.Depth(wave)/4f));
            BrokenCount=LitCount=0;
            float dawn=NightmareLighting.Active?.Dawn??0;
            int seed=owner.Session.State.Seed;
            for(int i=0;i<8;i++)
            {
                int state=StateFor(i,wave,shop);bool fixtureVisible=!shop || i<2;
                Vector2 basePosition=FixtureAnchor(i);
                float height=HeightFor(i,seed),sizeRatio=height/2.7f,identity=MotionIdentity(i,seed);
                if(fixtureVisible){if(state==2)BrokenCount++;else LitCount++;}
                bool firstFixture=modelStates[i]<0;
                if(modelStates[i]!=state)
                {
                    sprites[i].sprite=pictures[state];modelStates[i]=state;
                }
                float light=!fixtureVisible || state==2?0:state==1?.5f:1;
                if(!PresentationSettings.ReducedFlashes && state==1)light*=.94f+Mathf.Sin(Clock*.8f+i)*.025f;
                brightness[i]=firstFixture || PresentationSettings.ReducedMotion?light:Mathf.MoveTowards(brightness[i],light,Time.deltaTime*1.6f);
                Vector3 hover=DreamLevitation.Offset(Clock,identity,NightmareRules.Depth(wave),PresentationSettings.ReducedMotion);
                Vector3 foot=(Vector3)basePosition+hover;
                Quaternion lean=Quaternion.Euler(0,0,TiltFor(i,seed,Clock,PresentationSettings.ReducedMotion));
                ruptures[i]=new Vector4(basePosition.x,basePosition.y,1.6f,state==2 && fixtureVisible?1:0);
                sprites[i].enabled=fixtureVisible;
                if(sprites[i].sprite!=null)
                {
                    sprites[i].transform.localScale=Vector3.one*(height/sprites[i].sprite.bounds.size.y);
                    sprites[i].transform.rotation=lean;
                    sprites[i].transform.position=foot-sprites[i].transform.TransformVector(LampLocal(sprites[i].sprite,new Vector2(.40f,.025f)));
                    sprites[i].sortingOrder=WorldSpriteGrounding.SortAt(basePosition.y);
                    sprites[i].color=Color.Lerp(new Color(.42f,.46f,.55f),Color.white,Mathf.Max(light,dawn));
                }
                Vector3 flame=sprites[i].transform.TransformPoint(LampLocal(sprites[i].sprite,new Vector2(.66f,.49f)));
                sources[i]=new Vector4(flame.x,flame.y-FlameFloorDrop(i),5,brightness[i]*(1-dawn*.75f));
                halos[i].enabled=fixtureVisible && brightness[i]>.01f;halos[i].transform.position=new Vector3(flame.x,flame.y,0);halos[i].transform.rotation=lean;halos[i].transform.localScale=new Vector3(.9f*sizeRatio,1.1f*sizeRatio,1);halos[i].color=new Color(1,.52f,.16f,brightness[i]*.19f);halos[i].sortingOrder=WorldSpriteGrounding.SortAt(basePosition.y)+7;
                if(fixtureVisible)
                {
                    int order=WorldSpriteGrounding.SortAt(basePosition.y);
                    shadows[i].SortingBase=order-10;shadows[i].Display(sprites[i].sprite,basePosition,.6f*sizeRatio,hover.y,.35f);
                    suspensions[i].Display(basePosition,foot,.85f*sizeRatio,Clock,identity,order-1,.9f,state!=2);
                }
                else {shadows[i].Hide();suspensions[i].Hide();}
            }
            // Known scenery bounds cast soft analytical occlusion into the floor field.
            if(changed)
            {
                int b=0;foreach(var r in owner.GetComponentsInChildren<SpriteRenderer>())
                    if(b<8 && r.gameObject.name.StartsWith("DreamLandmark") && r.enabled && r.sprite!=null)
                    {var p=r.bounds.center;blockers[b++]=new Vector4(p.x,r.bounds.min.y+.3f,Mathf.Clamp(r.bounds.size.x*.28f,.18f,.75f),1);}
                while(b<8)blockers[b++]=Vector4.zero;
            }
            Vector2 extent=owner.WorldBounds+Vector2.one*12;var rect=new Vector4(-extent.x/2,-extent.y/2,extent.x,extent.y);
            fieldPainter.SetVectorArray("_LanternSources",sources);fieldPainter.SetVectorArray("_LanternBlockers",blockers);fieldPainter.SetVector("_LanternRect",rect);
            Paint(field,fieldPainter);
            Shader.SetGlobalTexture("_NightLanternField",field);Shader.SetGlobalVector("_NightLanternRect",rect);Shader.SetGlobalFloat("_NightLanternEnabled",1);
            fogPainter.SetVectorArray("_FogRuptures",ruptures);fogPainter.SetVector("_FogRect",rect);fogPainter.SetFloat("_FogClock",PresentationSettings.ReducedMotion?0:Clock);
            float depth=NightmareRules.Depth(wave)/4f*(1-dawn);float density=Mathf.Lerp(.12f,1.12f,depth)*(shop?.2f:1);
            if(PresentationSettings.HighReadability)density*=.55f;if(PresentationSettings.ReducedEffects)density*=.6f;
            density*=1-dawn;
            fogPainter.SetFloat("_FogDensity",density);fogPainter.SetInt("_FogSamples",FogSamples);
            Paint(fog,fogPainter);
            fogView.transform.position=Vector3.zero;fogView.transform.localScale=new Vector3(extent.x,extent.y,1);
        }
        private static void Paint(RenderTexture target,Material painter)
        {
            var previous=RenderTexture.active;
            try {Graphics.Blit(Texture2D.blackTexture,target,painter);}
            finally {RenderTexture.active=previous;}
        }
        private static Vector3 LampLocal(Sprite sprite,Vector2 uv)
        {
            if(sprite==null)return Vector3.zero;
            return new Vector3((uv.x-.5f)*sprite.bounds.size.x,(uv.y-.5f)*sprite.bounds.size.y,0);
        }
        public Vector3 FixtureFoot(int index)=>sprites[index].transform.TransformPoint(LampLocal(sprites[index].sprite,new Vector2(.40f,.025f)));
        private void SetVisible(bool value)
        {
            for(int i=0;i<8;i++)if(!value){sprites[i].enabled=false;halos[i].enabled=false;shadows[i].Hide();suspensions[i].Hide();}
            fogView.enabled=value;
        }
        public float Illumination(Vector2 point)
        {
            float best=0;foreach(var source in sources)
            {float d=(point-(Vector2)source).sqrMagnitude;float edge=Mathf.Clamp01(1-d/(source.z*source.z));best=Mathf.Max(best,source.w*edge*edge/(1+d*.15f));}return Mathf.Clamp01(best);
        }
        public Vector2 SourcePosition(int index)=>(Vector2)sources[index];
        private void OnDestroy()
        {
            if(Active==this){Active=null;Shader.SetGlobalFloat("_NightLanternEnabled",0);}
            if(field!=null){field.Release();Destroy(field);}if(fog!=null){fog.Release();Destroy(fog);}
            if(fieldPainter!=null)Destroy(fieldPainter);if(fogPainter!=null)Destroy(fogPainter);if(fogDisplay!=null)Destroy(fogDisplay);if(white!=null)Destroy(white);if(fogMesh!=null)Destroy(fogMesh);
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]private static void Reset(){Active=null;Shader.SetGlobalFloat("_NightLanternEnabled",0);}
    }
}
