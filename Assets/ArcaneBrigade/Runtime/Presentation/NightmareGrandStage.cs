using UnityEngine;
namespace ArcaneBrigade
{
    // Persistent map fixtures, not parallax borders. Camera movement never moves their anchors.
    [DefaultExecutionOrder(30)]
    public sealed class NightmareGrandStage : MonoBehaviour
    {
        public const int Capacity=12;
        private ArenaController owner;
        private readonly SpriteRenderer[] layers=new SpriteRenderer[Capacity];
        private readonly GroundShadow[] shadows=new GroundShadow[Capacity];
        private readonly DreamSuspension[] suspensions=new DreamSuspension[Capacity];
        private static readonly Vector2[] Slots={new Vector2(-.77f,.67f),new Vector2(-.24f,.74f),new Vector2(.3f,.67f),new Vector2(.77f,.64f),new Vector2(-.72f,0),new Vector2(.72f,-.04f),new Vector2(-.77f,-.64f),new Vector2(-.24f,-.73f),new Vector2(.24f,-.68f),new Vector2(.77f,-.64f),new Vector2(-.53f,.35f),new Vector2(.53f,-.35f)};
        public int VisibleCount{get;private set;}
        public float Grandeur{get;private set;}
        public static float StrengthFor(int wave)=>Mathf.Lerp(.35f,1,Mathf.Clamp01((wave-1)/19f));
        public static int DamageFor(int index,int wave,int seed)
            =>wave>=12+index%4+(seed&1)?2:wave>=5+index%3+((seed>>index)&1)?1:0;
        public static Vector2 AnchorFor(int index,Vector2 size,int seed)
        {
            unchecked
            {
                uint n=(uint)seed^(uint)(index+1)*2654435761u;n^=n>>16;n*=2246822519u;n^=n>>13;
                return Vector2.Scale(Slots[index%Capacity],size*.5f)+new Vector2((n&65535)/65535f-.5f,((n>>16)&65535)/65535f-.5f)*.9f;
            }
        }
        public Vector2 FixtureAnchor(int index)=>AnchorFor(index,owner.WorldBounds,owner.Session.State.Seed);
        public void Configure(ArenaController controller)
        {
            owner=controller;
            for(int i=0;i<Capacity;i++)
            {
                var go=new GameObject("DreamArchitecture"+i);go.transform.SetParent(transform,false);
                layers[i]=go.AddComponent<SpriteRenderer>();
                shadows[i]=go.AddComponent<GroundShadow>();shadows[i].PinCastToFoot=true;
                suspensions[i]=go.AddComponent<DreamSuspension>();
            }
        }
        private void LateUpdate()
        {
            if(owner?.Session==null || owner.Simulation==null || owner.PresentationPaused)return;
            var state=owner.Session.State;var art=WaitingSceneryArtwork.Current;
            bool visible=art!=null && !owner.Session.SecretRoomOpen && !owner.NightmareEndingVisible && state.Phase!=SessionPhase.Defeat;
            Grandeur=StrengthFor(state.Wave);VisibleCount=0;
            bool shop=state.Phase==SessionPhase.Shop && !owner.Session.SecretRoomOpen;
            for(int i=0;i<Capacity;i++)
            {
                var view=layers[i];view.enabled=visible;
                if(!visible){shadows[i].Hide();suspensions[i].Hide();continue;}
                int damage=DamageFor(i,state.Wave,state.Seed);
                view.sprite=art.Frame(i%4,damage);if(view.sprite==null){view.enabled=false;continue;}
                Vector2 floor=FixtureAnchor(i);
                float width=i%4==0?3.1f:i%4==1?1.7f:i%4==2?2.7f:2.8f;
                float scale=width/Mathf.Max(.01f,SpriteInkMetrics.BoundsOf(view.sprite).width);
                // the shop keeps only things with a story you can click and read (ShopInspect): this map architecture
                // (arch, letter cabinet, screen, bench) has none, so it stays out of the shop room
                if(shop){view.enabled=false;shadows[i].Hide();suspensions[i].Hide();continue;}
                view.transform.localScale=Vector3.one*scale;
                Vector3 hover=DreamLevitation.Offset(owner.PresentationClock,80+i,NightmareRules.Depth(state.Wave),PresentationSettings.ReducedMotion,.65f);
                float tilt=DreamLevitation.Tilt(owner.PresentationClock,80+i,PresentationSettings.ReducedMotion,.7f)+(damage==2?(i%2==0?-1:1)*5:0);
                if(ShopSceneryLanding.Active!=null){hover=ShopSceneryLanding.Active.Apply(hover,80+i);tilt=ShopSceneryLanding.Active.Tilt(tilt,80+i);}
                view.transform.rotation=Quaternion.Euler(0,0,tilt);
                WorldSpriteGrounding.Place(view,(Vector3)floor+hover);
                view.sortingOrder=WorldSpriteGrounding.SortAt(floor.y);
                float light=SharedLightField.Active?.SurfaceVisibility(floor)??1;
                view.color=new Color(light,light,light,1);
                shadows[i].SortingBase=view.sortingOrder-8;shadows[i].Display(view.sprite,floor,width*.65f,hover.y,.3f);
                suspensions[i].Display(floor,(Vector3)floor+hover,width,owner.PresentationClock,80+i,view.sortingOrder-1,.65f);
                VisibleCount++;
            }
        }
    }
}
