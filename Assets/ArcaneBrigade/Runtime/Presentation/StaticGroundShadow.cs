using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class StaticGroundShadow : MonoBehaviour
    {
        private SpriteRenderer view;
        private GroundShadow grounded;
        private void Start()
        {
            view = GetComponent<SpriteRenderer>();
            grounded = gameObject.AddComponent<GroundShadow>(); grounded.Configure(view.sprite);
        }
        private void LateUpdate()
        {
            if (view == null || view.sprite == null || !view.enabled) { if (grounded != null) grounded.Hide(); return; }
            Bounds bounds = view.bounds;
            grounded.Display(view.sprite, new Vector2(bounds.center.x, bounds.min.y + 0.05f), bounds.size.x * 0.9f, 0, 0.36f * view.color.a);
        }
    }
}
