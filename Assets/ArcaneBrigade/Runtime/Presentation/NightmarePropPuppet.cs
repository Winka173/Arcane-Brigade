using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class NightmarePropPuppet : MonoBehaviour
    {
        private Sprite source;
        private SpriteRenderer upper;
        private SpriteRenderer lower;
        private Vector3 hinge;
        private Material material;
        private SpriteRenderer uncountedLeg;
        private readonly MaterialPropertyBlock properties = new MaterialPropertyBlock();
        public void Display(SpriteRenderer reference, int identity, float time, int depth, bool shop, Vector2 player)
        {
            if (upper == null)
            {
                var top = new GameObject("HingedUpper"); top.transform.SetParent(transform, false); upper = top.AddComponent<SpriteRenderer>();
                var bottom = new GameObject("GroundedLower"); bottom.transform.SetParent(transform, false); lower = bottom.AddComponent<SpriteRenderer>();
                material = Resources.Load<Material>("UnitFlash");
                if (identity == 0)
                {
                    var leg = new GameObject("UncountedChairLeg"); leg.transform.SetParent(transform, false); uncountedLeg = leg.AddComponent<SpriteRenderer>();
                }
            }
            if (source != reference.sprite)
            {
                ReleaseSprites(); source = reference.sprite;
                Rect rect = source.rect;
                float split = Mathf.Round(rect.height * (identity == 4 ? .28f : .47f));
                upper.sprite = Sprite.Create(source.texture, new Rect(rect.x, rect.y + split - 1, rect.width, rect.height - split + 1), new Vector2(.5f, 0), source.pixelsPerUnit, 0, SpriteMeshType.FullRect);
                lower.sprite = Sprite.Create(source.texture, new Rect(rect.x, rect.y, rect.width, split), new Vector2(.5f, 1), source.pixelsPerUnit, 0, SpriteMeshType.FullRect);
                hinge = new Vector3((rect.width * .5f - source.pivot.x) / source.pixelsPerUnit, (split - source.pivot.y) / source.pixelsPerUnit, 0);
                if (uncountedLeg != null)
                    uncountedLeg.sprite = Sprite.Create(source.texture, new Rect(rect.x + rect.width * .12f, rect.y, rect.width * .23f, rect.height * .2f), new Vector2(.5f, 0), source.pixelsPerUnit, 0, SpriteMeshType.FullRect);
            }
            upper.enabled = lower.enabled = reference.enabled;
            if (!reference.enabled) return;
            if (uncountedLeg != null)
            {
                uncountedLeg.enabled = depth > 0; uncountedLeg.sortingOrder = reference.sortingOrder - 1;
                uncountedLeg.color = reference.color; uncountedLeg.sharedMaterial = material;
                ApplyLight(uncountedLeg, identity, player, PresentationSettings.ReducedMotion);
            }
            bool reduced = PresentationSettings.ReducedMotion;
            float phase = Mathf.Repeat(time + identity * 2.13f, 9);
            float gesture = reduced || !shop || phase > 1.8f ? 0 : Mathf.Sin(phase / 1.8f * Mathf.PI);
            float breath = reduced ? 0 : Mathf.Sin(time * (identity == 4 ? .65f : 1.05f) + identity) * (.008f + depth * .002f);
            float look = reduced || identity != 2 ? 0 : Mathf.Clamp(player.x - transform.position.x, -2, 2) * .006f;
            upper.transform.localPosition = hinge + new Vector3(look, breath * .08f, 0);
            lower.transform.localPosition = hinge;
            upper.transform.localRotation = Quaternion.Euler(0, 0, identity == 1 ? 0 : gesture * (identity == 5 ? 9 : 1.4f) + look * 30);
            if (!reduced && StagePerformance.Active != null)
                upper.transform.localRotation *= Quaternion.Euler(0, 0, StagePerformance.Active.Impact * (identity == 1 ? 3 : .8f));
            upper.transform.localScale = new Vector3(1 + breath * .3f, 1 + breath + gesture * (identity == 4 ? .025f : .008f), 1);
            lower.transform.localScale = new Vector3(1 - breath * .15f, 1, 1);
            upper.sharedMaterial = lower.sharedMaterial = material;
            upper.sortingOrder = reference.sortingOrder + 1; lower.sortingOrder = reference.sortingOrder;
            upper.color = lower.color = reference.color;
            ApplyLight(upper, identity, player, reduced);
            ApplyLight(lower, identity, player, reduced);
            reference.enabled = false;
        }
        private void ApplyLight(SpriteRenderer view, int identity, Vector2 player, bool reduced)
        {
            properties.Clear(); SpriteLighting.Apply(view, properties, false, true);
            Rect full = source.rect;
            properties.SetVector("_UvRect", new Vector4(full.x / source.texture.width, full.y / source.texture.height, full.width / source.texture.width, full.height / source.texture.height));
            if (identity == 2 && source.name.EndsWith("_06", System.StringComparison.Ordinal) || identity == 4 && source.name.EndsWith("_11", System.StringComparison.Ordinal))
            {
                Rect rectangle = source.rect;
                Vector2 texture = new Vector2(source.texture.width, source.texture.height);
                Vector2 centre = new Vector2(rectangle.x + rectangle.width * .5f, rectangle.y + rectangle.height * (identity == 2 ? .59f : .51f));
                Vector2 direction = Vector2.ClampMagnitude(player - (Vector2)transform.position, 2) * (reduced ? 0 : .005f);
                if (!reduced && Vector2.Distance(player, transform.position) < 2.2f) direction = -direction;
                properties.SetVector("_PropEye", new Vector4(centre.x / texture.x, centre.y / texture.y, rectangle.width * .1f / texture.x, rectangle.height * .09f / texture.y));
                properties.SetVector("_PropLook", new Vector4(direction.x * rectangle.width / texture.x, direction.y * rectangle.height / texture.y, 0, 0));
            }
            view.SetPropertyBlock(properties);
        }
        public void Hide() { if (upper != null) upper.enabled = lower.enabled = false; if (uncountedLeg != null) uncountedLeg.enabled = false; }
        private void ReleaseSprites()
        {
            if (upper != null && upper.sprite != null) Destroy(upper.sprite);
            if (lower != null && lower.sprite != null) Destroy(lower.sprite);
            if (uncountedLeg != null && uncountedLeg.sprite != null) Destroy(uncountedLeg.sprite);
        }
        private void OnDestroy() => ReleaseSprites();
    }
}
