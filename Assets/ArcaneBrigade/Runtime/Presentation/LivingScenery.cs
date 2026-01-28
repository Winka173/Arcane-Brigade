using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class LivingScenery : MonoBehaviour
    {
        private Vector3 origin;
        private Quaternion rotation;
        private ArenaController owner;
        private void Start() { origin = transform.localPosition; rotation = transform.localRotation; owner = FindAnyObjectByType<ArenaController>(); }
        private void Update()
        {
            if (owner == null || owner.Simulation == null || owner.Simulation.Phase == RunPhase.Paused) return;
            float time = owner.PresentationClock + origin.x * 0.7f + origin.y * 0.4f;
            float identity = origin.x * .7f + origin.y * .4f;
            transform.localRotation = rotation * Quaternion.Euler(0, 0, DreamLevitation.Tilt(time, identity, PresentationSettings.ReducedMotion, .55f));
            transform.localPosition = origin + DreamLevitation.Offset(time, identity, NightmareRules.Depth(owner.Session.State.Wave), PresentationSettings.ReducedMotion, .5f);
        }
    }
}
