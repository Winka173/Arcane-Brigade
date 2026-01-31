using System.Collections.Generic;
using UnityEngine;
namespace ArcaneBrigade
{
    public sealed class BuildMorphVisual : MonoBehaviour
    {
        private readonly List<PaintedMechanicView> pool=new List<PaintedMechanicView>();
        public int VisibleCount{get;private set;}
        public void Display(CombatSimulation simulation,bool reduced)
        {
            VisibleCount=0;var art=MechanicEffectArtwork.Current;
            if(simulation!=null && art!=null && (simulation.Phase==RunPhase.Combat || simulation.Phase==RunPhase.Paused))
            foreach(var strike in simulation.MorphStrikes)
            {
                if(VisibleCount>=EffectDensity.Limit(48))break;
                if(VisibleCount==pool.Count)pool.Add(new PaintedMechanicView(transform,"Build mechanic "+VisibleCount,1100));
                pool[VisibleCount++].Display(art.Form(strike.Kind),strike.Geometry,strike.Position,strike.End,strike.Direction,strike.Radius,strike.At,strike.Until,simulation.Elapsed,BuildMorphs.Get(strike.Kind).Color,reduced);
            }
            for(int i=VisibleCount;i<pool.Count;i++)pool[i].Hide();
        }
    }
}
