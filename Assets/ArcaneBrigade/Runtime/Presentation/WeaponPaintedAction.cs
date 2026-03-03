using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class WeaponPaintedAction : MonoBehaviour
    {
        private SpriteRenderer part;
        private EnsembleClip clip;
        private string identity;
        private MaterialPropertyBlock block;
        private MaterialPropertyBlock properties => block ??= new MaterialPropertyBlock(); // created on first use: a block made in a field initializer is null when the component is cloned in the editor
        public bool Visible => part != null && part.enabled;
        public void Display(SpriteRenderer source, WeaponDefinition definition, float age, float windup, bool reduced)
        {
            if (identity != definition.Id) { identity = definition.Id; clip = EnsembleArtwork.Current?.Weapon(identity); }
            bool active = clip != null && !reduced && age >= 0 && age < windup + definition.Recovery;
            if (!active)
            {
                if (part != null) part.enabled = false;
                if (clip != null) { source.GetPropertyBlock(properties); properties.SetFloat("_WeaponMask", 0); source.SetPropertyBlock(properties); }
                return;
            }
            if (part == null)
            {
                var child = new GameObject("PaintedMechanicalAction"); child.transform.SetParent(transform, false); part = child.AddComponent<SpriteRenderer>();
            }
            float progress = age < windup ? .375f * age / Mathf.Max(.01f, windup) : .375f + .625f * (age - windup) / Mathf.Max(.01f, definition.Recovery);
            part.sprite = clip.At(progress); part.sharedMaterial = source.sharedMaterial; part.color = source.color;
            part.sortingLayerID = source.sortingLayerID; part.sortingOrder = source.sortingOrder; part.enabled = source.enabled && source.gameObject.activeInHierarchy;
            float scale = source.sprite.bounds.size.x / Mathf.Max(.01f, clip.ReferenceWidth);
            part.transform.localScale = Vector3.one * scale;
            part.transform.localPosition = source.sprite.bounds.center - part.sprite.bounds.center * scale;
            part.transform.localRotation = Quaternion.identity;
            properties.Clear(); SpriteLighting.Apply(part, properties, false, true); part.SetPropertyBlock(properties);
            source.GetPropertyBlock(properties); properties.SetFloat("_WeaponMask", 1); properties.SetVector("_WeaponCut", new Vector4(0, 0, 1, 1)); source.SetPropertyBlock(properties);
        }
        public void Clear() { if (part != null) part.enabled = false; identity = null; clip = null; }
    }
}
