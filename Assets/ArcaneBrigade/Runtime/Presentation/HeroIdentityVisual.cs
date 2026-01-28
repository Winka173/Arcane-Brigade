using System.Collections.Generic;
using UnityEngine;
namespace ArcaneBrigade
{
    public sealed class HeroIdentityVisual : MonoBehaviour
    {
        private sealed class View { public LineRenderer Ink, Colour; public SpriteRenderer Ghost; }
        private readonly List<View> pool=new List<View>();
        private Material material;
        public int VisibleCount { get; private set; }
        public void Display(CombatSimulation s)
        {
            VisibleCount=0;
            if(s!=null && s.HeroIdentityEnabled && (s.Phase==RunPhase.Combat || s.Phase==RunPhase.Paused))foreach(var g in s.HeroIdentityGlyphs)
            {
                if(VisibleCount>=EffectDensity.Limit(28))break;
                View v=Acquire(VisibleCount++);bool line=g.Shape==MorphGeometry.Line,seal=g.Shape==MorphGeometry.Seal,arc=g.Shape==MorphGeometry.Crescent;
                float fade=Mathf.Clamp01((g.Until-s.Elapsed)/.3f),alpha=(s.Elapsed<g.At?.35f:.75f)*fade*EffectDensity.AlliedAlpha;
                Color c=HeroIdentities.For(s.Champion.Definition.Id).Color;c.a=alpha;
                v.Colour.startColor=v.Colour.endColor=c;v.Ink.startColor=v.Ink.endColor=new Color(.035f,.055f,.09f,alpha);
                int count=line?2:seal?4:arc?25:40;
                v.Colour.positionCount=v.Ink.positionCount=count;v.Colour.loop=v.Ink.loop=!line&&!arc;
                v.Colour.widthMultiplier=PresentationSettings.HighReadability?.035f:.05f;v.Ink.widthMultiplier=.085f;
                for(int i=0;i<count;i++)
                {
                    Vector2 at;
                    if(line)at=i==0?g.Position:g.End;
                    else if(seal)at=g.Position+(Vector2)(Quaternion.Euler(0,0,45+i*90)*Vector2.right)*g.Radius;
                    else {float a=arc?Mathf.Atan2(g.Direction.y,g.Direction.x)-1.4f+i*2.8f/(count-1):i*Mathf.PI*2/count;at=g.Position+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*g.Radius;}
                    v.Ink.SetPosition(i,at);v.Colour.SetPosition(i,at);
                }
                v.Ink.enabled=v.Colour.enabled=true;v.Ghost.enabled=g.Portrait!=null;
                if(g.Portrait!=null){v.Ghost.sprite=g.Portrait;Rect ink=SpriteInkMetrics.BoundsOf(g.Portrait);float scale=s.Champion.Definition.BodyHeight/Mathf.Max(.01f,ink.height);v.Ghost.transform.localScale=Vector3.one*scale;v.Ghost.transform.position=(Vector3)g.Position-(Vector3)(ink.center*scale);v.Ghost.color=new Color(c.r,c.g,c.b,alpha*.4f);}
            }
            for(int i=VisibleCount;i<pool.Count;i++)pool[i].Ink.enabled=pool[i].Colour.enabled=pool[i].Ghost.enabled=false;
        }
        private View Acquire(int index)
        {
            if(index<pool.Count)return pool[index];
            if(material==null)material=new Material(Shader.Find("Sprites/Default"));
            LineRenderer Make(string name,int layer){var go=new GameObject(name);go.transform.SetParent(transform,false);var r=go.AddComponent<LineRenderer>();r.sharedMaterial=material;r.useWorldSpace=true;r.numCapVertices=3;r.sortingOrder=layer;return r;}
            var ghost=new GameObject("HeroDream");ghost.transform.SetParent(transform,false);var sprite=ghost.AddComponent<SpriteRenderer>();sprite.sortingOrder=1090;
            var view=new View{Ink=Make("HeroInk",1088),Colour=Make("HeroColour",1089),Ghost=sprite};pool.Add(view);return view;
        }
        private void OnDestroy(){if(material!=null)Destroy(material);}
    }
}
