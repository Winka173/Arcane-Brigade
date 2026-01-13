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
            float wind = PresentationSettings.ReducedMotion ? 0 : Mathf.Sin(time * 1.4f) * Mathf.Sin(time * 0.37f) * 0.8f;
            transform.localRotation = rotation * Quaternion.Euler(0, 0, wind);
            transform.localPosition = origin + Vector3.up * (wind * 0.008f);
        }
    }
}
