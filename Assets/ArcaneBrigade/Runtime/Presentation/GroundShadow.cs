using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class GroundShadow : MonoBehaviour
    {
        /// <summary>The one light of the whole game (requirement 4/11/2026: shadows read as lit from straight overhead and kept vanishing):
        /// a low light in the upper left. A point one unit above the floor throws its shadow this far from the spot below it, so a
        /// standing thing lies on the floor from its feet toward the lower right at about two thirds of its height.</summary>
        public static readonly Vector2 Light = new Vector2(.52f, -.36f);
        private SpriteRenderer contact;
        private SpriteRenderer cast;
        private Transform castPivot;
        private Transform stage;
        /// <summary>Shadows lie over the floor and its night fog (order 800), so the fog never swallows them, and under everything that
        /// stands (sorted from about 880 up by its foot).</summary>
        public int SortingBase { get; set; } = 806;
        public bool Essential { get; set; }
        /// <summary>A standing thing: its shadow is its silhouette laid from its feet along the light. Otherwise (coins, shots, rugs,
        /// bodies on the floor) the shadow is a flat copy under it, moved along the light by its height.</summary>
        public bool PinCastToFoot { get; set; }
        /// <summary>The world scale and mirror of the silhouette when it is drawn by another renderer than this object's own.</summary>
        public Vector2 SilhouetteScale { get; set; }
        public bool FlipX { get; set; }
        public SpriteRenderer Contact => contact;
        public SpriteRenderer Cast => cast;
        public void AddRenderers(System.Collections.Generic.List<Renderer> renderers)
        {
            if (contact != null) renderers.Add(contact);
            if (cast != null) renderers.Add(cast);
        }
        public void Configure(Sprite silhouette)
        {
            if (stage == null)
            {
                stage = new GameObject("WorldGroundShadow").transform;
                if (gameObject.scene.IsValid() && gameObject.scene.isLoaded) UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(stage.gameObject, gameObject.scene); // a visual outside any loaded scene (editor fixtures) keeps the default scene
            }
            if (contact == null) contact = ShopFloorShadow.Create(stage, Vector2.zero, 1, 0.4f);
            if (cast == null)
            {
                castPivot = new GameObject("LightCastPivot").transform; castPivot.SetParent(stage, false);
                var child = new GameObject("DirectionalGroundShadow"); child.transform.SetParent(castPivot, false);
                cast = child.AddComponent<SpriteRenderer>();
            }
            cast.sprite = silhouette;
        }
        private Sprite lastSilhouette; private float lastWidth, lastElevation, lastOpacity;
        /// <summary>Shows the last shadow again at a new foot and size (a prop the shop layout moved, shrank, hid and brought back).</summary>
        public void Redisplay(Vector2 ground, float widthScale = 1)
        {
            if (lastSilhouette == null) return;
            float width = lastWidth;
            Display(lastSilhouette, ground, width * widthScale, lastElevation, lastOpacity);
            lastWidth = width;
        }
        public void Display(Sprite silhouette, Vector2 ground, float width, float elevation = 0, float opacity = 0.3f)
        {
            lastSilhouette = silhouette; lastWidth = width; lastElevation = elevation; lastOpacity = opacity;
            if (contact == null) Configure(silhouette);
            if (!stage.gameObject.activeSelf) stage.gameObject.SetActive(true);
            width = Mathf.Max(0.04f, width);
            elevation = Mathf.Max(0, elevation);
            float strength = Mathf.Clamp01(opacity * 1.9f);
            float softness = 1 + Mathf.Clamp(elevation, 0, 2) * 0.35f;
            // the dark contact where the thing meets the floor stays under it; lifted, it fades and widens
            contact.transform.position = ground;
            contact.enabled = true;
            contact.transform.rotation = Quaternion.identity;
            contact.transform.localScale = new Vector3(width * 0.78f * softness, width * 0.22f * softness, 1);
            contact.color = new Color(0.005f, 0.008f, 0.018f, strength / (softness * softness));
            contact.sortingOrder = SortingBase + 7;
            cast.sprite = silhouette;
            cast.enabled = silhouette != null;
            if (!cast.enabled) return;
            Vector2 scale = SilhouetteScale;
            bool flip = FlipX;
            if (scale.x <= 0 || scale.y <= 0)
            {
                SpriteRenderer own = GetComponent<SpriteRenderer>();
                if (own != null && own.sprite == silhouette) { Vector3 lossy = own.transform.lossyScale; scale = new Vector2(Mathf.Abs(lossy.x), Mathf.Abs(lossy.y)); flip = own.flipX; }
                else scale = Vector2.one * width / Mathf.Max(0.001f, silhouette.bounds.size.x);
            }
            // the silhouette is mapped by one linear map: its x stays along the floor, its height falls along the light (or, for a flat
            // thing, is pressed flat); the map is split into turn, stretch, turn so a sprite under two transforms can draw it
            float a = scale.x, b, d;
            if (PinCastToFoot) { b = scale.y * Light.x; d = scale.y * Light.y; }
            else { b = scale.y * .08f; d = scale.y * .3f; a *= 1.08f; }
            float e = (a + d) * .5f, f = (a - d) * .5f, g = b * .5f, h = -b * .5f;
            float q = Mathf.Sqrt(e * e + h * h), r = Mathf.Sqrt(f * f + g * g);
            float first = Mathf.Atan2(g, f), second = Mathf.Atan2(h, e);
            castPivot.position = ground + Light * elevation;
            castPivot.rotation = Quaternion.Euler(0, 0, (second + first) * .5f * Mathf.Rad2Deg);
            castPivot.localScale = new Vector3(q + r, q - r, 1);
            Quaternion inner = Quaternion.Euler(0, 0, (second - first) * .5f * Mathf.Rad2Deg);
            cast.transform.localRotation = inner;
            cast.flipX = flip;
            Vector3 foot = PinCastToFoot ? WorldSpriteGrounding.Foot(silhouette) : (Vector3)silhouette.bounds.center;
            if (flip) foot.x = -foot.x;
            cast.transform.localPosition = -(inner * foot);
            cast.color = new Color(0.008f, 0.01f, 0.025f, strength * (PinCastToFoot ? .8f : .5f) / softness);
            cast.sortingOrder = SortingBase;
        }
        public void Hide() { if (contact != null) contact.enabled = false; if (cast != null) cast.enabled = false; }
        public void TranslateGround(Vector3 displacement) { if (stage != null) stage.position += displacement; }
        public void Fade(float alpha)
        {
            if (contact != null) { Color tint = contact.color; tint.a *= alpha; contact.color = tint; }
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
