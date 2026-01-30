using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class LivingScenery : MonoBehaviour
    {
        private Vector3 origin;
        private Quaternion rotation;
        private ArenaController owner;
        private DreamSuspension suspension;
        private SpriteRenderer view;
        private void Start() { origin = transform.localPosition; rotation = transform.localRotation; owner = FindAnyObjectByType<ArenaController>();view=GetComponent<SpriteRenderer>();suspension=gameObject.AddComponent<DreamSuspension>(); }
        private void Update()
        {
            if (owner == null || owner.Simulation == null || owner.Simulation.Phase == RunPhase.Paused) return;
            float time = owner.PresentationClock + origin.x * 0.7f + origin.y * 0.4f;
            float identity = origin.x * .7f + origin.y * .4f;
            transform.localRotation = rotation * Quaternion.Euler(0, 0, DreamLevitation.Tilt(time, identity, PresentationSettings.ReducedMotion, .55f));
            transform.localPosition = origin + DreamLevitation.Offset(time, identity, NightmareRules.Depth(owner.Session.State.Wave), PresentationSettings.ReducedMotion, .5f);
            if(view!=null && view.enabled)
            {
                Vector3 foot=new Vector3(view.bounds.center.x,view.bounds.min.y,0);
                suspension.Display(foot-Vector3.up*.22f,foot,view.bounds.size.x,time,identity,view.sortingOrder-1,view.color.a*.65f,true);
            }
            else suspension.Hide();
        }
    }
}
