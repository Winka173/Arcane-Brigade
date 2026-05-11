using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>A living outline round a sprite while a skill is on (owner 23/10/2026): the hero and its transformed weapon,
    /// the friends a skill empowers, the skill's gauge. Eight solid-colour copies of the sprite sit just behind it, offset by
    /// the rim's width, so only a thin edge shows; it breathes, a band of light runs up it, and it flickers faster as the
    /// skill runs out. Callers ask for it every frame they want it; a frame without a request hides it.</summary>
    [DisallowMultipleComponent]
    public sealed class SkillRim : MonoBehaviour
    {
        private static readonly Vector2[] Offsets =
        {
            new Vector2(1, 0), new Vector2(-1, 0), new Vector2(0, 1), new Vector2(0, -1),
            new Vector2(.71f, .71f), new Vector2(-.71f, .71f), new Vector2(.71f, -.71f), new Vector2(-.71f, -.71f),
        };
        private static Material material;
        private SpriteRenderer source;
        private SpriteRenderer[] copies;
        private MaterialPropertyBlock block;
        private int shownFrame = -10;
        private Color color; private float width, sweepY, sweepWidth, electric;

        /// <summary>The rim on this sprite (added the first time).</summary>
        public static SkillRim For(SpriteRenderer target)
        {
            if (target == null) return null;
            SkillRim rim = target.GetComponent<SkillRim>();
            if (rim == null) { rim = target.gameObject.AddComponent<SkillRim>(); rim.source = target; }
            return rim;
        }

        /// <param name="tint">Rim colour; its alpha is the rim's strength this frame (the caller breathes it).</param>
        /// <param name="worldWidth">How far the rim stands out from the sprite, in world units.</param>
        /// <param name="sweep">Height of the travelling band of light in world units (NaN: none).</param>
        /// <param name="crackle">0..1: sparks crawling along the rim (Storm Fencer's static).</param>
        public void Show(Color tint, float worldWidth, float sweep, float sweepBand = .28f, float crackle = 0)
        {
            electric = PresentationSettings.ReducedFlashes ? crackle * .4f : crackle;
            if (PresentationSettings.ReducedFlashes) tint.a *= .6f;
            color = tint; width = worldWidth; sweepY = float.IsNaN(sweep) ? -9999 : sweep; sweepWidth = sweepBand;
            shownFrame = Time.frameCount;
            Sync();
        }

        private void Ensure()
        {
            if (copies != null) return;
            if (material == null && Resources.Load<Shader>("SkillRim") is Shader shader) material = new Material(shader) { name = "Skill rim", hideFlags = HideFlags.DontSave };
            block = new MaterialPropertyBlock();
            copies = new SpriteRenderer[Offsets.Length];
            for (int i = 0; i < copies.Length; i++)
            {
                var go = new GameObject("Skill rim " + i); go.transform.SetParent(transform, false);
                copies[i] = go.AddComponent<SpriteRenderer>();
                if (material != null) copies[i].sharedMaterial = material;
                copies[i].enabled = false;
            }
        }

        private void Sync()
        {
            if (source == null) source = GetComponent<SpriteRenderer>();
            Ensure();
            bool on = material != null && source != null && source.enabled && source.sprite != null && color.a > .01f && Time.frameCount - shownFrame <= 1;
            Vector3 lossy = transform.lossyScale;
            float sx = Mathf.Max(.0001f, Mathf.Abs(lossy.x)), sy = Mathf.Max(.0001f, Mathf.Abs(lossy.y));
            if (on) { block.SetFloat("_SweepY", sweepY); block.SetFloat("_SweepWidth", sweepWidth); block.SetFloat("_Electric", electric); }
            for (int i = 0; i < copies.Length; i++)
            {
                SpriteRenderer c = copies[i];
                c.enabled = on;
                if (!on) continue;
                c.sprite = source.sprite; c.flipX = source.flipX; c.flipY = source.flipY;
                c.sortingLayerID = source.sortingLayerID; c.sortingOrder = source.sortingOrder - 1;
                c.color = color;
                c.transform.localPosition = new Vector3(Offsets[i].x * width / sx, Offsets[i].y * width / sy, 0);
                c.SetPropertyBlock(block);
            }
        }

        private void LateUpdate() { if (copies != null) Sync(); }
        private void OnDisable() { if (copies != null) foreach (var c in copies) if (c != null) c.enabled = false; }
    }
}
