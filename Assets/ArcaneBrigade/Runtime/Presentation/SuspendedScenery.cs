using UnityEngine;

namespace ArcaneBrigade
{
    [DefaultExecutionOrder(80)]
    public sealed class SuspendedScenery : MonoBehaviour
    {
        private ArenaController owner;
        private SpriteRenderer view;
        private GroundShadow shadow;
        private DreamSuspension suspension;
        private Vector3 origin;
        private float identity;
        public void Configure(ArenaController controller, SpriteRenderer renderer, float phase)
        {
            owner = controller; view = renderer; identity = phase; origin = transform.localPosition;
            shadow = gameObject.AddComponent<GroundShadow>(); shadow.SortingBase = view.sortingOrder - 10; shadow.PinCastToFoot = true;
            suspension = gameObject.AddComponent<DreamSuspension>();
        }
        private void LateUpdate()
        {
            if (owner == null || view == null || owner.PresentationPaused) return;
            if (!view.enabled) { shadow.Hide(); suspension.Hide(); return; }
            Vector3 floor = transform.parent.TransformPoint(origin);
            Vector3 hovering = DreamLevitation.Offset(owner.PresentationClock, identity, NightmareRules.Depth(owner.Session.State.Wave), PresentationSettings.ReducedMotion);
            transform.rotation = Quaternion.Euler(0, 0, DreamLevitation.Tilt(owner.PresentationClock, identity, PresentationSettings.ReducedMotion));
            WorldSpriteGrounding.Place(view, floor + hovering);
            shadow.Display(view.sprite, floor, view.bounds.size.x * .8f, hovering.y, .3f * view.color.a);
            suspension.Display(floor, floor + hovering, view.bounds.size.x, owner.PresentationClock, identity, view.sortingOrder - 1, view.color.a);
        }
    }
}
