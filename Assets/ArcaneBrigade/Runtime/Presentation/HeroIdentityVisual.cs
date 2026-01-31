using System.Collections.Generic;
using UnityEngine;
namespace ArcaneBrigade
{
    public sealed class HeroIdentityVisual : MonoBehaviour
    {
        private sealed class View{public PaintedMechanicView Paint;public SpriteRenderer Ghost;}
        private readonly List<View> pool=new List<View>();
        public int VisibleCount{get;private set;}
        public void Display(CombatSimulation simulation)
        {
            VisibleCount=0;var art=MechanicEffectArtwork.Current;
            if(simulation!=null && art!=null && simulation.HeroIdentityEnabled && (simulation.Phase==RunPhase.Combat || simulation.Phase==RunPhase.Paused))
            foreach(var glyph in simulation.HeroIdentityGlyphs)
            {
                if(VisibleCount>=EffectDensity.Limit(28))break;
                if(VisibleCount==pool.Count)
                {
                    var go=new GameObject("Dream identity portrait");go.transform.SetParent(transform,false);var ghost=go.AddComponent<SpriteRenderer>();ghost.sortingOrder=1090;
                    pool.Add(new View{Paint=new PaintedMechanicView(transform,"Hero mechanic "+VisibleCount,1089),Ghost=ghost});
                }
                var view=pool[VisibleCount++];Color color=HeroIdentities.For(simulation.Champion.Definition.Id).Color;
                view.Paint.Display(art.Hero(simulation.Champion.Definition.Id),glyph.Shape,glyph.Position,glyph.End,glyph.Direction,glyph.Radius,glyph.At,glyph.Until,simulation.Elapsed,color,PresentationSettings.ReducedMotion);
                view.Ghost.enabled=glyph.Portrait!=null;
                if(glyph.Portrait!=null)
                {view.Ghost.sprite=glyph.Portrait;Rect ink=SpriteInkMetrics.BoundsOf(glyph.Portrait);float scale=simulation.Champion.Definition.BodyHeight/Mathf.Max(.01f,ink.height);view.Ghost.transform.localScale=Vector3.one*scale;view.Ghost.transform.position=(Vector3)glyph.Position-(Vector3)(ink.center*scale);view.Ghost.color=new Color(color.r,color.g,color.b,Mathf.Clamp01((glyph.Until-simulation.Elapsed)/.3f)*.3f*EffectDensity.AlliedAlpha);}
            }
            for(int i=VisibleCount;i<pool.Count;i++){pool[i].Paint.Hide();pool[i].Ghost.enabled=false;}
        }
    }
}
