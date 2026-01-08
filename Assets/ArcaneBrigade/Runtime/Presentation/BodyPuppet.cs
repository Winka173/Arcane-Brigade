using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class BodyPuppet : MonoBehaviour
    {
        private sealed class Parts { public Sprite Upper; public Sprite Lower; public Vector2 UpperOffset; public Vector2 LowerOffset; }
        private static readonly Dictionary<Sprite, Parts> cache = new Dictionary<Sprite, Parts>();
        private SpriteRenderer upper;
        private SpriteRenderer lower;
        private MaterialPropertyBlock properties;
        public void Display(SpriteRenderer reference, UnitState state, float time, bool reduced)
        {
            if (properties == null) properties = new MaterialPropertyBlock();
            if (state.Definition.AnimationFrames.Length < 4)
            {
                reference.enabled = true;
                if (upper != null) { upper.enabled = false; lower.enabled = false; }
                return;
            }
            if (upper == null) { upper = Create("UpperBody"); lower = Create("LowerBody"); }
            bool moving = (state.Position - state.PreviousPosition).sqrMagnitude > 0.000001f;
            int legFrame = UnitVisual.AnimationFrame(time, state.Id, moving, reduced);
            int idleFrame = UnitVisual.AnimationFrame(time, state.Id, false, reduced);
            int headFrame = idleFrame == 1 ? 1 : legFrame;
            Parts head = GetParts(state.Definition.AnimationFrames[headFrame]);
            Parts feet = GetParts(state.Definition.AnimationFrames[legFrame]);
            upper.sprite = head.Upper; lower.sprite = feet.Lower;
            upper.transform.localPosition = head.UpperOffset;
            lower.transform.localPosition = feet.LowerOffset;
            float breathing = reduced ? 0 : Mathf.Sin(time * 2.8f + state.Id * 0.8f) * 0.045f;
            upper.transform.localScale = new Vector3(1 - breathing * 0.3f, 1 + breathing, 1);
            upper.transform.localPosition += new Vector3(reduced ? 0 : Mathf.Sin(time * 1.8f + state.Id) * 0.018f, breathing * 0.18f, 0);
            upper.transform.localRotation = Quaternion.Euler(0, 0, reduced ? 0 : Mathf.Sin(time * (moving ? 7 : 2.2f) + state.Id) * (moving ? 1.6f : 2.4f));
            lower.transform.localScale = Vector3.one;
            upper.sharedMaterial = lower.sharedMaterial = reference.sharedMaterial;
            upper.color = lower.color = reference.color;
            upper.flipX = lower.flipX = reference.flipX;
            upper.sortingOrder = reference.sortingOrder + 1; lower.sortingOrder = reference.sortingOrder;
            reference.GetPropertyBlock(properties); upper.SetPropertyBlock(properties); lower.SetPropertyBlock(properties);
            reference.enabled = false; upper.enabled = lower.enabled = true;
        }
        private SpriteRenderer Create(string caption)
        {
            var child = new GameObject(caption); child.transform.SetParent(transform, false); return child.AddComponent<SpriteRenderer>();
        }
        private static Parts GetParts(Sprite frame)
        {
            if (cache.TryGetValue(frame, out Parts result)) return result;
            Rect full = frame.rect; float split = full.y + full.height * 0.37f;
            Rect top = new Rect(full.x, split - 1, full.width, full.yMax - split + 1);
            Rect bottom = new Rect(full.x, full.y, full.width, split - full.y + 1);
            result = new Parts { Upper = Sprite.Create(frame.texture, top, new Vector2(0.5f, 0.5f), frame.pixelsPerUnit, 0, SpriteMeshType.FullRect), Lower = Sprite.Create(frame.texture, bottom, new Vector2(0.5f, 0.5f), frame.pixelsPerUnit, 0, SpriteMeshType.FullRect), UpperOffset = (top.center - full.center) / frame.pixelsPerUnit, LowerOffset = (bottom.center - full.center) / frame.pixelsPerUnit };
            cache.Add(frame, result); return result;
        }
        public void Fade(float alpha)
        {
            if (upper == null) return;
            Color color = upper.color; color.a *= alpha; upper.color = lower.color = color;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache()
        {
            foreach (Parts parts in cache.Values) { Object.Destroy(parts.Upper); Object.Destroy(parts.Lower); }
            cache.Clear();
        }
    }
}
