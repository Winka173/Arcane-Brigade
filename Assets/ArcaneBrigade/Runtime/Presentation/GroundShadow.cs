using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class GroundShadow : MonoBehaviour
    {
        private SpriteRenderer contact;
        private SpriteRenderer cast;
        public void Configure(Sprite silhouette)
        {
            if (contact == null) contact = ShopFloorShadow.Create(transform, Vector2.zero, 1, 0.4f);
            if (cast == null)
            {
                var child = new GameObject("DirectionalGroundShadow"); child.transform.SetParent(transform, false);
                cast = child.AddComponent<SpriteRenderer>();
            }
            cast.sprite = silhouette;
        }
        public void Display(Sprite silhouette, Vector2 ground, float width, float elevation = 0, float opacity = 0.3f)
        {
            if (contact == null) Configure(silhouette);
            Vector3 parentScale = transform.lossyScale;
            float horizontal = Mathf.Max(0.0001f, Mathf.Abs(parentScale.x));
            float vertical = Mathf.Max(0.0001f, Mathf.Abs(parentScale.y));
            contact.transform.position = ground;
            contact.enabled = true;
            contact.transform.rotation = Quaternion.identity;
            float softness = 1 + Mathf.Clamp(elevation, 0, 2) * 0.35f;
            contact.transform.localScale = new Vector3(width * softness / horizontal, width * 0.25f * softness / vertical, 1);
            contact.color = new Color(0.015f, 0.025f, 0.045f, opacity / softness);
            contact.sortingOrder = 410;
            cast.sprite = silhouette;
            cast.enabled = silhouette != null;
            if (silhouette != null)
            {
                float scale = width / Mathf.Max(0.001f, silhouette.bounds.size.x);
                cast.transform.position = ground + new Vector2(0.13f + elevation * 0.06f, -0.045f - elevation * 0.025f);
                cast.transform.rotation = Quaternion.Euler(0, 0, -18);
                cast.transform.localScale = new Vector3(scale / horizontal, scale * 0.22f / vertical, 1);
                cast.color = new Color(0.025f, 0.035f, 0.06f, opacity * 0.4f / softness);
                cast.sortingOrder = 405;
            }
        }
        public void Hide() { if (contact != null) contact.enabled = false; if (cast != null) cast.enabled = false; }
        public void Fade(float alpha)
        {
            if (contact != null) { Color tint = contact.color; tint.a *= alpha; contact.color = tint; }
            if (cast != null) { Color tint = cast.color; tint.a *= alpha; cast.color = tint; }
        }
    }
}
