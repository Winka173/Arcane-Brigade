using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class K8MotionDeath : MonoBehaviour
    {
        private Transform shadow;
        private Transform grip;
        private Vector3 bodyPosition;
        private Vector3 bodyScale;
        private Quaternion bodyRotation;
        private Vector3 rootPosition;
        private Vector3 shadowScale;
        private Vector3 gripPosition;
        private Quaternion gripRotation;
        private SpriteRenderer[] fragments;

        public void Capture(Transform shadowPart, Transform gripPart, Vector2 position)
        {
            rootPosition = position;
            shadow = shadowPart; grip = gripPart;
            bodyPosition = transform.localPosition; bodyScale = transform.localScale; bodyRotation = transform.localRotation;
            shadowScale = shadow.localScale; gripPosition = grip.localPosition; gripRotation = grip.localRotation;
            if (fragments == null) fragments = transform.parent.GetComponentsInChildren<SpriteRenderer>(true);
        }

        private void LateUpdate()
        {
            if (!PresentationSettings.ReducedMotion || grip == null) return;
            transform.parent.position = rootPosition;
            transform.localPosition = bodyPosition; transform.localScale = bodyScale; transform.localRotation = bodyRotation;
            shadow.localScale = shadowScale;
            grip.localPosition = gripPosition; grip.localRotation = gripRotation;
            foreach (SpriteRenderer fragment in fragments) if (fragment != null && fragment.name.StartsWith("PaperChip_")) fragment.enabled = false;
        }
    }
}
