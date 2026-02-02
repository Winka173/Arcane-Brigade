using UnityEngine;
namespace ArcaneBrigade
{
    [DefaultExecutionOrder(-40)]
    public sealed class ShopSceneryLanding : MonoBehaviour
    {
        private ArenaController owner;
        private bool wasShop;
        private string visitedRun;
        private int visitedWave=-1;
        public float StartedAt{get;private set;}=-100;
        public static ShopSceneryLanding Active{get;private set;}
        public void Configure(ArenaController value){owner=value;Active=this;}
        private bool InShop=>owner?.Session!=null && owner.Session.State.Phase==SessionPhase.Shop && owner.Simulation?.Phase==RunPhase.Rest;
        public bool Settling=>InShop && owner.PresentationClock-StartedAt<1.45f;
        private void Update()
        {
            if(owner==null || owner.PresentationPaused)return;
            bool shop=InShop;
            if(shop && (!wasShop || visitedRun!=owner.Session.State.RunId || visitedWave!=owner.Session.State.Wave))
            {
                StartedAt=owner.PresentationClock;
                visitedRun=owner.Session.State.RunId;visitedWave=owner.Session.State.Wave;
            }
            // Purchases rebuild the simulation to compile stats. They are still the same shop visit.
            wasShop=shop;
        }
        public static Vector3 LandingOffset(Vector3 hover,float age,bool reduced,float identity)
        {
            if(reduced || age<0 || age>=1.45f)return hover;
            float t=age-Mathf.Repeat(identity*.618f,1)*.09f;
            if(t<.56f){float fall=Mathf.Clamp01(t/.56f);return new Vector3(hover.x*(1-fall),Mathf.Lerp(hover.y+1.4f,0,fall*fall),0);}
            if(t<.88f)return Vector3.up*Mathf.Sin((t-.56f)/.32f*Mathf.PI)*.14f;
            float rise=Mathf.SmoothStep(0,1,Mathf.Clamp01((t-.88f)/.48f));return hover*rise;
        }
        public Vector3 Apply(Vector3 hover,float identity)=>InShop?LandingOffset(hover,owner.PresentationClock-StartedAt,PresentationSettings.ReducedMotion,identity):hover;
        public float Tilt(float normal,float identity)
        {
            if(!InShop || PresentationSettings.ReducedMotion)return normal;
            float age=owner.PresentationClock-StartedAt;
            return normal*Mathf.SmoothStep(0,1,Mathf.Clamp01((age-.7f)/.65f));
        }
        private void OnDestroy(){if(Active==this)Active=null;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]private static void Reset()=>Active=null;
    }
}
