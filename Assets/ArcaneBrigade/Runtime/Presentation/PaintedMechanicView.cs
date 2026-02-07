using UnityEngine;
namespace ArcaneBrigade
{
    // Reusable illustrated layers; geometry remains in simulation, never in the art.
    public sealed class PaintedMechanicView
    {
        private readonly SpriteRenderer body,trail,echo,spark;
        public int VisibleSprites=>(body.enabled?1:0)+(trail.enabled?1:0)+(echo.enabled?1:0)+(spark.enabled?1:0);
        public PaintedMechanicView(Transform parent,string name,int order)
        {
            SpriteRenderer Make(string suffix,int layer)
            {var child=new GameObject(name+suffix);child.transform.SetParent(parent,false);var view=child.AddComponent<SpriteRenderer>();view.sortingOrder=layer;view.enabled=false;return view;}
            body=Make(" / painted motif",order);trail=Make(" / brush trail",order-1);echo=Make(" / drifting fragments",order+1);spark=Make(" / ink echo",order-2);
        }
        private static void Place(SpriteRenderer view,Sprite sprite,Vector2 at,float width,float angle,Color color,float height=0)
        {
            view.enabled=sprite!=null && color.a>.005f;if(!view.enabled)return;
            view.sprite=sprite;view.color=color;Rect ink=SpriteInkMetrics.BoundsOf(sprite);
            float scale=width/Mathf.Max(.01f,ink.width),vertical=height>0?height/Mathf.Max(.01f,ink.height):scale;
            view.transform.localScale=new Vector3(scale,vertical,1);view.transform.rotation=Quaternion.Euler(0,0,angle);
            view.transform.position=(Vector3)at-view.transform.TransformVector(new Vector3(ink.center.x,ink.center.y,0));
        }
        public void Display(Sprite motif,MorphGeometry geometry,Vector2 position,Vector2 end,Vector2 direction,float radius,float at,float until,float time,Color tint,bool reduced)
        {
            Hide();if(motif==null || time>=until)return;
            var art=MechanicEffectArtwork.Current;
            bool preparing=time<at,line=geometry==MorphGeometry.Line,crescent=geometry==MorphGeometry.Crescent;
            float phase=Mathf.Clamp01((time-at)/Mathf.Max(.08f,until-at));
            float fade=preparing?.22f:Mathf.Min(1,(until-time)/.22f);
            float motion=reduced?1:Mathf.Lerp(.86f,1.06f,Mathf.SmoothStep(0,1,phase));
            float alpha=fade*EffectDensity.AreaAlpha*(PresentationSettings.HighReadability?.75f:.92f);
            Color color=Color.Lerp(Color.white,tint,.12f);color.a=alpha;
            float angle=Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg;
            float width=line?.75f:Mathf.Clamp(radius*2,.65f,6.8f);
            Vector2 centre=line?end:position+(crescent?direction*radius*.25f:Vector2.zero);
            if(crescent)width*=.8f;
            float twist=reduced?0:(phase-.35f)*12;
            Place(body,motif,centre,width*motion,line||crescent?angle+twist:twist,color);
            if(line)
            {
                Vector2 axis=end-position;Color stroke=Color.Lerp(Color.white,tint,.4f);stroke.a=alpha*.75f;
                Place(trail,art?.Thrust,(position+end)*.5f,axis.magnitude,Mathf.Atan2(axis.y,axis.x)*Mathf.Rad2Deg,stroke,preparing?.12f:.25f);
            }
            if(preparing || PresentationSettings.ReducedEffects || AnimationDetailBudget.Level>=2)return;
            Vector2 drift=reduced?Vector2.zero:new Vector2(Mathf.Cos(angle*Mathf.Deg2Rad+1),Mathf.Sin(angle*Mathf.Deg2Rad+1))*phase*width*.09f;
            Color light=color;light.a*=.19f*(1-phase);
            Place(echo,motif,centre+drift,width*motion*1.12f,twist-8,light);
            if(crescent)
            {light=color;light.a*=.45f;Place(spark,art?.Slash,position+direction*radius*.35f,width*.9f,angle-25,light);}
        }
        public void Hide(){body.enabled=trail.enabled=echo.enabled=spark.enabled=false;}
    }
}
