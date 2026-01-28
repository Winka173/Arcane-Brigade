using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class GroundShadow : MonoBehaviour
    {
        private SpriteRenderer contact;
        private SpriteRenderer penumbra;
        private SpriteRenderer cast;
        private Transform stage;
        public int SortingBase { get; set; } = 405;
        public bool Essential { get; set; }
        public bool PinCastToFoot { get; set; }
        public SpriteRenderer Contact => contact;
        public SpriteRenderer Penumbra => penumbra;
        public SpriteRenderer Cast => cast;
        public void AddRenderers(System.Collections.Generic.List<Renderer> renderers)
        {
            if (contact != null) renderers.Add(contact);
            if (penumbra != null) renderers.Add(penumbra);
            if (cast != null) renderers.Add(cast);
        }
        public void Configure(Sprite silhouette)
        {
            if (stage == null)
            {
                stage = new GameObject("WorldGroundShadow").transform;
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(stage.gameObject, gameObject.scene);
            }
            if (contact == null) contact = ShopFloorShadow.Create(stage, Vector2.zero, 1, 0.4f);
            if (penumbra == null) { penumbra = ShopFloorShadow.Create(stage, Vector2.zero, 1, 0.3f); penumbra.name = "SoftGroundPenumbra"; }
            if (cast == null)
            {
                var child = new GameObject("DirectionalGroundShadow"); child.transform.SetParent(stage, false);
                cast = child.AddComponent<SpriteRenderer>();
            }
            cast.sprite = silhouette;
        }
        public void Display(Sprite silhouette, Vector2 ground, float width, float elevation = 0, float opacity = 0.3f)
        {
            if (contact == null) Configure(silhouette);
            if (!stage.gameObject.activeSelf) stage.gameObject.SetActive(true);
            width = Mathf.Max(0.04f, width);
            float strength = Mathf.Clamp01(opacity * 1.9f);
            contact.transform.position = ground;
            contact.enabled = true;
            contact.transform.rotation = Quaternion.identity;
            float softness = 1 + Mathf.Clamp(elevation, 0, 2) * 0.35f;
            contact.transform.localScale = new Vector3(width * 0.82f, width * 0.24f, 1);
            contact.color = new Color(0.005f, 0.008f, 0.018f, strength / softness);
            contact.sortingOrder = SortingBase + 7;
            penumbra.enabled = AnimationDetailBudget.Level == 0 || Essential;
            if (penumbra.enabled)
            {
                penumbra.transform.position = ground + new Vector2(width * 0.06f, -width * 0.025f);
                penumbra.transform.rotation = Quaternion.identity;
                penumbra.transform.localScale = new Vector3(width * 1.35f * softness, width * 0.38f * softness, 1);
                penumbra.color = new Color(0.012f, 0.014f, 0.025f, strength * 0.55f / softness);
                penumbra.sortingOrder = SortingBase + 5;
            }
            cast.sprite = silhouette;
            cast.enabled = silhouette != null && (Essential || AnimationDetailBudget.Level < 2);
            if (cast.enabled)
            {
                float scale = width / Mathf.Max(0.001f, silhouette.bounds.size.x);
                Vector2 direction = NightmareLighting.Active != null ? NightmareLighting.Active.ShadowDirection(ground) : new Vector2(.8f, -.35f);
                cast.transform.position = ground + direction * (width * (PinCastToFoot ? .05f : .2f) + elevation * .06f);
                cast.transform.rotation = Quaternion.Euler(0, 0, Mathf.Clamp(direction.x * -15, -15, 15));
                cast.transform.localScale = new Vector3(scale * 1.12f, scale * 0.3f, 1);
                if (PinCastToFoot) cast.transform.position -= cast.transform.TransformVector(WorldSpriteGrounding.Foot(silhouette));
                cast.color = new Color(0.008f, 0.01f, 0.025f, strength * 0.5f / softness);
                cast.sortingOrder = SortingBase;
            }
        }
        public void Hide() { if (contact != null) contact.enabled = false; if (penumbra != null) penumbra.enabled = false; if (cast != null) cast.enabled = false; }
        public void Fade(float alpha)
        {
            if (contact != null) { Color tint = contact.color; tint.a *= alpha; contact.color = tint; }
            if (penumbra != null) { Color tint = penumbra.color; tint.a *= alpha; penumbra.color = tint; }
            if (cast != null) { Color tint = cast.color; tint.a *= alpha; cast.color = tint; }
        }
        private void OnDisable() { Hide(); if (stage != null) stage.gameObject.SetActive(false); }
        private void OnDestroy()
        {
            if (stage == null) return;
            if (Application.isPlaying) Destroy(stage.gameObject);
            else DestroyImmediate(stage.gameObject);
        }
    }
}
