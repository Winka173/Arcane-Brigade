using UnityEngine;

namespace ArcaneBrigade
{
    // The caller owns the floor anchor and clock; interactions never follow visual drift.
    public sealed class FloatingStoryProp : MonoBehaviour
    {
        private SpriteRenderer view;
        private GroundShadow shadow;
        private DreamSuspension suspension;
        private float identity;
        public void Configure(SpriteRenderer renderer,float phase)
        {
            view=renderer;identity=phase;
            shadow=gameObject.AddComponent<GroundShadow>();shadow.PinCastToFoot=true;
            suspension=gameObject.AddComponent<DreamSuspension>();
        }
        public void Display(Vector3 floor,float clock,float depth)
        {
            if(view==null || !view.enabled || view.sprite==null){shadow.Hide();suspension.Hide();return;}
            Vector3 hover=DreamLevitation.Offset(clock,identity,depth,PresentationSettings.ReducedMotion);
            view.transform.rotation=Quaternion.Euler(0,0,DreamLevitation.Tilt(clock,identity,PresentationSettings.ReducedMotion));
            WorldSpriteGrounding.Place(view,floor+hover);
            shadow.SortingBase=view.sortingOrder-10;
            shadow.Display(view.sprite,floor,view.bounds.size.x*.75f,hover.y,.3f*view.color.a);
            suspension.Display(floor,floor+hover,view.bounds.size.x,clock,identity,view.sortingOrder-1,view.color.a);
        }
    }
}
