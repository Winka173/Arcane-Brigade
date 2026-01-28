using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class MerchantPortraitRig : MonoBehaviour
    {
        private sealed class Cut
        {
            public Sprite Upper;
            public Sprite Lower;
            public Vector2 UpperPosition;
            public Vector2 LowerPosition;
        }
        private readonly Dictionary<Sprite, Cut> cuts = new Dictionary<Sprite, Cut>();
        private SpriteRenderer upper;
        private SpriteRenderer lower;
        private readonly MaterialPropertyBlock properties = new MaterialPropertyBlock();
        private SpriteRenderer[] pieces;
        public static Vector3 GesturePose(float purchaseAge, float denyAge, bool dream, bool reduced)
        {
            if (reduced) return Vector3.zero;
            float prepare = AnimationTiming.Pulse(purchaseAge, .22f);
            float flourish = AnimationTiming.Pulse(purchaseAge - .18f, .42f);
            float recovery = AnimationTiming.Settle(purchaseAge - .5f, .3f, 3);
            float refusal = AnimationTiming.Settle(denyAge, .55f, 5);
            return new Vector3((dream ? -5 : 3) * prepare + (dream ? 4 : -4) * flourish + recovery + refusal * 5, prepare * -.006f + flourish * (dream ? -.009f : .01f), prepare * -.015f + flourish * .012f);
        }
        public void Display(SpriteRenderer source, float time, bool speaking, float purchaseAge, float denyAge, bool dream, float attention, bool reduced)
        {
            if (source.sprite == null) return;
            if (reduced) { source.enabled = true; if (upper != null) upper.enabled = lower.enabled = false; return; }
            if (upper == null) { upper = Create("MerchantUpperActing"); lower = Create("MerchantLowerWeight"); pieces = new[] { upper, lower }; }
            Sprite sprite = source.sprite;
            if (!cuts.TryGetValue(sprite, out Cut cut))
            {
                Rect rectangle = sprite.rect;
                float dividing = rectangle.y + rectangle.height * .44f;
                Rect top = new Rect(rectangle.x, dividing, rectangle.width, rectangle.yMax - dividing);
                Rect bottom = new Rect(rectangle.x, rectangle.y, rectangle.width, dividing - rectangle.y + 1);
                Vector2 origin = rectangle.position + sprite.pivot;
                cut = new Cut {
                    Upper = Sprite.Create(sprite.texture, top, new Vector2(.5f, 0), sprite.pixelsPerUnit, 0, SpriteMeshType.FullRect),
                    Lower = Sprite.Create(sprite.texture, bottom, new Vector2(.5f, 1), sprite.pixelsPerUnit, 0, SpriteMeshType.FullRect),
                    UpperPosition = (new Vector2(top.center.x, top.yMin) - origin) / sprite.pixelsPerUnit,
                    LowerPosition = (new Vector2(bottom.center.x, bottom.yMax) - origin) / sprite.pixelsPerUnit };
                cuts.Add(sprite, cut);
            }
            upper.sprite = cut.Upper; lower.sprite = cut.Lower;
            float breath = Mathf.Sin(time * 1.25f) * .009f;
            float speech = speaking ? Mathf.Sin(time * 17) * .006f : 0;
            Vector3 gesture = GesturePose(purchaseAge, denyAge, dream, reduced);
            upper.transform.localPosition = cut.UpperPosition + Vector2.up * gesture.y * sprite.bounds.size.y;
            lower.transform.localPosition = cut.LowerPosition;
            upper.transform.localRotation = Quaternion.Euler(0, 0, attention * -.8f + gesture.x);
            lower.transform.localRotation = Quaternion.Euler(0, 0, -gesture.x * .15f);
            upper.transform.localScale = new Vector3(1 - breath * .25f - gesture.z * .2f, 1 + breath + speech + gesture.z, 1);
            lower.transform.localScale = new Vector3(1 + breath * .15f + gesture.z * .1f, 1 - breath * .2f - gesture.z * .15f, 1);
            source.GetPropertyBlock(properties);
            Rect full = sprite.rect;
            properties.SetVector("_UvRect", new Vector4(full.x / sprite.texture.width, full.y / sprite.texture.height, full.width / sprite.texture.width, full.height / sprite.texture.height));
            foreach (SpriteRenderer piece in pieces)
            {
                piece.color = source.color; piece.sharedMaterial = source.sharedMaterial; piece.sortingLayerID = source.sortingLayerID;
                piece.sortingOrder = source.sortingOrder; piece.SetPropertyBlock(properties); piece.enabled = source.color.a > 0;
            }
            source.enabled = false;
        }
        private SpriteRenderer Create(string label)
        {
            var child = new GameObject(label); child.transform.SetParent(transform, false); return child.AddComponent<SpriteRenderer>();
        }
        private void OnDestroy()
        {
            foreach (Cut cut in cuts.Values) { Destroy(cut.Upper); Destroy(cut.Lower); }
            cuts.Clear();
        }
    }
}
