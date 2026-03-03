using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class WeaponMechanism : MonoBehaviour
    {
        private Sprite source;
        private Sprite upperSprite;
        private Sprite lowerSprite;
        private SpriteRenderer upper;
        private SpriteRenderer lower;
        private Vector2 hinge;
        private Vector4 region;
        private bool scissors;
        private MaterialPropertyBlock block;
        private MaterialPropertyBlock properties => block ??= new MaterialPropertyBlock(); // created on first use: a block made in a field initializer is null when the component is cloned in the editor
        public int VisibleParts => (upper != null && upper.enabled ? 1 : 0) + (lower != null && lower.enabled ? 1 : 0);
        public static bool Supports(WeaponDefinition definition) => definition.Motion == WeaponMotion.Scissors || definition.Id == "SleepCamera" || definition.Id == "MusicBox" || definition.Id == "MailboxLauncher";
        public void Display(SpriteRenderer reference, WeaponDefinition definition, float age, float windup, bool reduced)
        {
            bool supported = !reduced && Supports(definition) && reference.sprite != null && reference.sharedMaterial != null && reference.sharedMaterial.HasProperty("_WeaponMask");
            if (!supported)
            {
                if (upper != null) upper.enabled = lower.enabled = false;
                reference.GetPropertyBlock(properties); properties.SetFloat("_WeaponMask", 0); reference.SetPropertyBlock(properties);
                return;
            }
            if (source != reference.sprite)
            {
                Clear(); source = reference.sprite;
                Rect full = source.rect; float split = full.y + full.height * .5f;
                scissors = definition.Motion == WeaponMotion.Scissors;
                Vector2 pivot = new Vector2(.32f, .5f);
                region = new Vector4(0, 0, 1, 1);
                if (scissors)
                {
                    upperSprite = Sprite.Create(source.texture, new Rect(full.x, split, full.width, full.yMax - split), new Vector2(.32f, 0), source.pixelsPerUnit, 0, SpriteMeshType.FullRect);
                    lowerSprite = Sprite.Create(source.texture, new Rect(full.x, full.y, full.width, split - full.y), new Vector2(.32f, 1), source.pixelsPerUnit, 0, SpriteMeshType.FullRect);
                }
                else
                {
                    region = definition.Id == "SleepCamera" ? new Vector4(.56f, .04f, 1, .77f) : definition.Id == "MusicBox" ? new Vector4(.28f, .66f, .55f, 1) : new Vector4(.29f, .44f, .61f, 1);
                    pivot = definition.Id == "SleepCamera" ? new Vector2(.6f, .38f) : definition.Id == "MusicBox" ? new Vector2(.415f, .68f) : new Vector2(.51f, .48f);
                    Rect cut = new Rect(full.x + full.width * region.x, full.y + full.height * region.y, full.width * (region.z - region.x), full.height * (region.w - region.y));
                    Vector2 localPivot = new Vector2((pivot.x - region.x) / (region.z - region.x), (pivot.y - region.y) / (region.w - region.y));
                    upperSprite = Sprite.Create(source.texture, cut, localPivot, source.pixelsPerUnit, 0, SpriteMeshType.FullRect);
                }
                hinge = (Vector2.Scale(pivot, full.size) - source.pivot) / source.pixelsPerUnit;
                if (upper == null) { upper = Create("WeaponMovingPart"); lower = Create("WeaponCounterPart"); }
            }
            float charge = age >= 0 && age < windup ? Mathf.SmoothStep(0, 1, age / Mathf.Max(.01f, windup)) : 0;
            float snap = AnimationTiming.Settle(age - windup, .25f, 3);
            float angle = charge * 13 - snap * 4;
            reference.GetPropertyBlock(properties);
            properties.SetFloat("_WeaponMask", 1); properties.SetVector("_WeaponCut", region); reference.SetPropertyBlock(properties);
            properties.SetFloat("_WeaponMask", 0); properties.SetVector("_WeaponFlex", Vector4.zero);
            if (scissors) { Apply(upper, upperSprite, reference, angle); Apply(lower, lowerSprite, reference, -angle); }
            else
            {
                float travel = charge * .018f - snap * .025f;
                float rotation = definition.Id == "SleepCamera" ? 0 : definition.Id == "MusicBox" ? charge * 9 + snap * 6 : charge * -8 + snap * 5;
                Apply(upper, upperSprite, reference, rotation); lower.enabled = false;
                if (definition.Id == "SleepCamera") upper.transform.localPosition += Vector3.right * travel * source.bounds.size.x;
            }
        }
        private void Apply(SpriteRenderer view, Sprite sprite, SpriteRenderer reference, float angle)
        {
            view.sprite = sprite; view.enabled = reference.enabled && reference.gameObject.activeInHierarchy;
            view.sharedMaterial = reference.sharedMaterial; view.color = reference.color;
            view.sortingLayerID = reference.sortingLayerID; view.sortingOrder = reference.sortingOrder;
            view.transform.localPosition = hinge; view.transform.localRotation = Quaternion.Euler(0, 0, angle);
            view.SetPropertyBlock(properties);
        }
        private SpriteRenderer Create(string caption)
        {
            var child = new GameObject(caption); child.transform.SetParent(transform, false); return child.AddComponent<SpriteRenderer>();
        }
        public void Hide()
        {
            if (upper != null) upper.enabled = false;
            if (lower != null) lower.enabled = false;
        }
        public void Clear()
        {
            if (upper != null) upper.enabled = lower.enabled = false;
            if (upperSprite != null) Destroy(upperSprite); if (lowerSprite != null) Destroy(lowerSprite);
            upperSprite = lowerSprite = source = null;
        }
        private void OnDestroy() { Clear(); }
    }
}
