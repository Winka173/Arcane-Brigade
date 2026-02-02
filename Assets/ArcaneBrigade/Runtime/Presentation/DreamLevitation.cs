using UnityEngine;

namespace ArcaneBrigade
{
    public static class DreamLevitation
    {
        public static Vector3 Offset(float time, float identity, float depth, bool reduced, float strength = 1)
        {
            float phase = identity * 2.399963f;
            float height = (.36f + Mathf.Clamp01(depth / 4) * .13f) * strength;
            if (reduced) return Vector3.up * height;
            // Different periods as well as phases avoid a roomful of synchronized pistons.
            float breath=time*Mathf.Lerp(.48f,.66f,Mathf.Repeat(identity*.618034f,1));
            return new Vector3((Mathf.Sin(time*.27f+phase)*.12f+Mathf.Sin(breath*.63f+phase*.7f)*.04f)*strength,
                height+(Mathf.Sin(breath+phase)*.16f+Mathf.Sin(time*.21f+phase*1.7f)*.04f)*strength,0);
        }
        public static float Tilt(float time, float identity, bool reduced, float strength = 1)
            => reduced ? 0 : (Mathf.Sin(identity*1.71f)*1.3f+Mathf.Sin(time*.34f+identity*2.399963f)*2.4f+Mathf.Sin(time*.16f+identity)*.65f)*strength;
    }
    // Only edge motes remain. The under-object wind, smoke and ribbons were removed.
    public sealed class DreamSuspension : MonoBehaviour
    {
        private readonly SpriteRenderer[] motes=new SpriteRenderer[2];
        private static Sprite moteSprite;
        private bool initialized;
        public void Display(Vector3 floor,Vector3 foot,float width,float time,float identity,int order,float opacity,bool warm=false)
        {
            if(!initialized)
            {
                initialized=true;
                var glow=Resources.Load<Material>("MienCho/WaitingGlow");
                if(glow!=null)
                {
                    if(moteSprite==null)moteSprite=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,Texture2D.whiteTexture.width,Texture2D.whiteTexture.height),Vector2.one*.5f,Texture2D.whiteTexture.width);
                    for(int i=0;i<motes.Length;i++){var child=new GameObject("DriftingDreamMote"+i);child.transform.SetParent(transform,false);motes[i]=child.AddComponent<SpriteRenderer>();motes[i].sprite=moteSprite;motes[i].sharedMaterial=glow;}
                }
            }
            Color tint=warm?new Color(.95f,.72f,.39f):new Color(.55f,.72f,.96f);
            for(int i=0;i<motes.Length;i++)
            {
                var mote=motes[i];if(mote==null)continue;
                mote.enabled=opacity>.01f && !PresentationSettings.ReducedEffects && AnimationDetailBudget.Level<2;
                if(!mote.enabled)continue;
                float age=PresentationSettings.ReducedMotion?.45f:Mathf.Repeat(time*.12f+identity*.37f+i*.5f,1);
                float clock=PresentationSettings.ReducedMotion?0:time;
                mote.transform.position=foot+new Vector3((i==0?-1:1)*Mathf.Min(width*.34f,.9f)+Mathf.Sin(clock*.4f+identity+i)*.055f,.15f+age*.55f,0);
                mote.transform.rotation=Quaternion.identity;
                float size=.045f+Mathf.Sin(age*Mathf.PI)*.035f;Vector3 scale=transform.lossyScale;
                mote.transform.localScale=new Vector3(size/Mathf.Max(.001f,Mathf.Abs(scale.x)),size/Mathf.Max(.001f,Mathf.Abs(scale.y)),1);
                mote.color=new Color(tint.r,tint.g,tint.b,Mathf.Sin(age*Mathf.PI)*opacity*.42f);mote.sortingOrder=order+1;
            }
        }
        public void Hide(){foreach(var mote in motes)if(mote!=null)mote.enabled=false;}
        private void OnDisable()=>Hide();
    }
}
