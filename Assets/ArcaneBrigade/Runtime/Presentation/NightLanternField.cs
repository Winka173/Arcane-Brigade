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
        public const float ShopLampHeight=1.62f; // the shop is a small room: its two lamps stand no taller than this
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
        // Act I: each lamp is one visit of the Thursday visitor and breaks on its canon wave (ActOneCanon.BreakWave).
        public static int BrokenFor(int wave)=>ActOneCanon.Broken(wave);
        public static int StateFor(int index,int wave,bool shop)=>StateFor(index,wave,shop,0);
        public static int StateFor(int index,int wave,bool shop,int relitMask)
        {
            if(shop && (index==0 || index==1))return 0;
            return ActOneCanon.LampState(System.Array.IndexOf(BreakOrder,index),wave,relitMask);
        }
        /// <summary>The story lamp (0..7, in break order) that went out most recently, and when; drives the HUD caption.</summary>
        public static int LastLoss {get;private set;}=-1;
        public static float LastLossAt {get;private set;}=-100;
        // Presentation-only noise: never consume the run/shop/combat random streams.
        private static float Variation(int index,int seed,uint channel)=>ArenaLayoutPlan.LanternNoise(index,seed,channel); // shared with the combat layout
        public static float HeightFor(int index,int seed)=>Mathf.Lerp(MinimumHeight,MaximumHeight,Variation(index,seed,3));
        public static float RestTiltFor(int index,int seed)
            =>(((index ^ seed)&1)==0?-1:1)*Mathf.Lerp(4.5f,10.5f,Variation(index,seed,4));
        private static float MotionIdentity(int index,int seed)=>211+index+Variation(index,seed,5)*997;
        public static float TiltFor(int index,int seed,float clock,bool reduced)
            =>RestTiltFor(index,seed)+(reduced?0:DreamLevitation.Tilt(clock,MotionIdentity(index,seed),false,.65f));
        public static Vector2 PositionFor(int index,Vector2 size,int seed=0,bool shop=false)
        {
            Vector2 jitter=new Vector2(Variation(index,seed,1)*2-1,Variation(index,seed,2)*2-1);
            // Shop fixtures stay in their outer margins, clear of offers and interaction points.
            // (only a wish there: ShopLayout gives each shop lamp the nearest spot clear of stalls, chair path and props)
            if(shop && index<2)return new Vector2(index==0?-5.0f:-3.55f,index==0?-4.72f:.55f)+Vector2.Scale(jitter,new Vector2(.04f,.06f));
            // in combat only a wish too: ArenaLayout gives each lamp the nearest spot clear of the other props
            return ArenaLayoutPlan.LanternWish(index,size,seed);
        }
        public Vector2 FixtureAnchor(int index)
        {
            bool shop=owner.Session.State.Phase==SessionPhase.Shop;
            Vector2 wish=PositionFor(index,owner.WorldBounds,owner.Session.State.Seed,shop);
            if(!shop)return ArenaLayout.Foot(owner,"lantern"+index,wish);
            return index<2 && ShopLayout.Active!=null && ShopLayout.Active.TryFoot("lantern"+index,out Vector2 foot)?foot:wish;
        }
        /// <summary>Asks the shop layout for room for a shop lamp; false when this shop has none (the lamp stays dark and hidden).</summary>
        private bool ShopPlace(int index,int seed)
        {
            if(ShopLayout.Active==null || sprites[index].sprite==null)return true;
            float height=Mathf.Min(HeightFor(index,seed),ShopLampHeight);
            Vector2 size=SpriteInkMetrics.BoundsOf(sprites[index].sprite).size*(height/Mathf.Max(.01f,sprites[index].sprite.bounds.size.y))+new Vector2(.3f,0); // lean
            return ShopLayout.Active.Place("lantern"+index,PositionFor(index,owner.WorldBounds,seed,true),size,2+index,out _);
        }
        public float FlameFloorDrop(int index)=>HeightFor(index,owner.Session.State.Seed)*(1.15f/2.7f);
        public static float RuptureInfluence(Vector2 offset)=>Mathf.Exp(-offset.sqrMagnitude*.18f);
        public static float SourceTransmission(float darkness,float party,float dawn,bool shop)
            =>shop?1:Mathf.Lerp(1,.22f,Mathf.Clamp01(darkness)*(1-Mathf.Clamp01(party)*.7f)*(1-Mathf.Clamp01(dawn)));
        public float RuptureStrength(Vector2 position)
        {
            float value=0;
            foreach(var zone in ruptures)value+=RuptureInfluence(position-(Vector2)zone)*zone.w;
            return Mathf.Clamp01(value);
        }
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
            if(wave>0 && current>wave && BrokenFor(current)>BrokenFor(wave))
            {
                LastLoss=BrokenFor(current)-1;LastLossAt=Time.unscaledTime;
                if(!PresentationSettings.ReducedAudio && breakClip!=null)wind.PlayOneShot(breakClip,PresentationSettings.SfxVolume*.2f);
            }
            wave=current;previousShop=shop;wind.volume=PresentationSettings.ReducedAudio?0:PresentationSettings.SfxVolume*(shop?.015f:Mathf.Lerp(.025f,.13f,NightmareRules.Depth(wave)/4f));
            BrokenCount=LitCount=0;
            float dawn=NightmareLighting.Active?.Dawn??0;
            int seed=owner.Session.State.Seed;
            // Establish every dark pocket before evaluating any source. Array order must not decide which lamps dim.
            for(int i=0;i<8;i++)
            {
                Vector2 anchor=FixtureAnchor(i);
                bool broken=StateFor(i,wave,shop,owner.Session.State.ActOne?.RelitMask??0)==2;
                ruptures[i]=new Vector4(anchor.x,anchor.y,1.6f,!shop && broken?1:0);
            }
            for(int i=0;i<8;i++)
            {
                int state=StateFor(i,wave,shop,owner.Session.State.ActOne?.RelitMask??0);Vector2 basePosition=FixtureAnchor(i);float fit=1;
                bool fixtureVisible=shop?i<2 && ShopPlace(i,seed):ArenaLayout.Place(owner,"lantern"+i,ref basePosition,out fit);
                float height=(shop?Mathf.Min(HeightFor(i,seed),ShopLampHeight):HeightFor(i,seed))*fit,sizeRatio=height/2.7f,identity=MotionIdentity(i,seed);
                if(fixtureVisible){if(state==2)BrokenCount++;else LitCount++;}
                bool firstFixture=modelStates[i]<0;
                if(modelStates[i]!=state)
                {
                    sprites[i].sprite=pictures[state];modelStates[i]=state;
                }
                float light=!fixtureVisible || state==2?0:state==1?.5f:1;
                if(!shop)light*=1-.85f*(owner.Simulation?.LanternDimming(i)??0); // a Lamp Snuffer pinching this flame (WaitingEnemies)
                if(!PresentationSettings.ReducedFlashes && state==1)light*=.94f+Mathf.Sin(Clock*.8f+i)*.025f;
                brightness[i]=firstFixture || PresentationSettings.ReducedMotion?light:Mathf.MoveTowards(brightness[i],light,Time.deltaTime*1.6f);
                Vector3 hover=DreamLevitation.Offset(Clock,identity,NightmareRules.Depth(wave),PresentationSettings.ReducedMotion);
                Vector3 foot=(Vector3)basePosition+hover;
                Quaternion lean=Quaternion.Euler(0,0,TiltFor(i,seed,Clock,PresentationSettings.ReducedMotion));
                sprites[i].enabled=fixtureVisible;
                if(sprites[i].sprite!=null)
                {
                    sprites[i].transform.localScale=Vector3.one*(height/sprites[i].sprite.bounds.size.y);
                    sprites[i].transform.rotation=lean;
                    sprites[i].transform.position=foot-sprites[i].transform.TransformVector(LampLocal(sprites[i].sprite,new Vector2(.40f,.025f)));
                    sprites[i].sortingOrder=WorldSpriteGrounding.SortAt(basePosition.y);
                }
                Vector3 flame=sprites[i].transform.TransformPoint(LampLocal(sprites[i].sprite,new Vector2(.66f,.49f)));
                float party=SharedLightField.Active?.Strength(basePosition)??0;
                float transmission=SourceTransmission(RuptureStrength(basePosition),party,dawn,shop);
                sources[i]=new Vector4(flame.x,flame.y-FlameFloorDrop(i),5,brightness[i]*(1-dawn*.75f)*transmission);
                halos[i].enabled=fixtureVisible && brightness[i]>.01f;halos[i].transform.position=new Vector3(flame.x,flame.y,0);halos[i].transform.rotation=lean;halos[i].transform.localScale=new Vector3(.9f*sizeRatio,1.1f*sizeRatio,1);halos[i].sortingOrder=WorldSpriteGrounding.SortAt(basePosition.y)+7;
                if(fixtureVisible)
                {
                    int order=WorldSpriteGrounding.SortAt(basePosition.y);
                    shadows[i].SortingBase=order-10;shadows[i].Display(sprites[i].sprite,basePosition,.6f*sizeRatio,hover.y,.35f);
                }
                else {shadows[i].Hide();suspensions[i].Hide();}
            }
            // Sprite art and additive bloom belong to the same world light as their surroundings.
            // Evaluate after ALL source strengths are current; the tint never feeds back into emitted energy.
            for(int i=0;i<8;i++)
            {
                Vector2 anchor=FixtureAnchor(i);
                float surface=shop?1:(SharedLightField.Active?.SurfaceVisibility(anchor)??NightmareLighting.Active?.Ambient??1);
                float visibility=Mathf.Clamp(surface,PresentationSettings.HighReadability?.16f:.025f,1);
                int state=modelStates[i];
                Color tint=Color.Lerp(new Color(.42f,.46f,.55f),Color.white,Mathf.Max(state==2?0:state==1?.5f:1,dawn))*visibility;
                tint.a=1;sprites[i].color=tint;
                halos[i].color=new Color(1,.52f,.16f,sources[i].w*.19f*visibility);
                if(sprites[i].enabled)suspensions[i].Display(anchor,FixtureFoot(i),.85f*HeightFor(i,seed)/2.7f,Clock,MotionIdentity(i,seed),sprites[i].sortingOrder-1,.9f*visibility,state!=2);
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
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]private static void Reset(){Active=null;LastLoss=-1;LastLossAt=-100;Shader.SetGlobalFloat("_NightLanternEnabled",0);}
    }
}
