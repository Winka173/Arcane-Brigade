using UnityEngine;

namespace ArcaneBrigade
{
    public static class ShopFloorShadow
    {
        private static Texture2D texture;
        private static Sprite sprite;
        public static SpriteRenderer Create(Transform parent, Vector2 floor, float width, float opacity)
        {
            if (sprite == null)
            {
                texture = new Texture2D(96, 96, TextureFormat.RGBA32, false) { name = "NightMarketGroundShadow", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
                var pixels = new Color[96 * 96];
                for (int vertical = 0; vertical < 96; vertical++)
                    for (int horizontal = 0; horizontal < 96; horizontal++)
                    {
                        float distance = Vector2.Distance(new Vector2(horizontal, vertical), new Vector2(47.5f, 47.5f)) / 47.5f;
                        pixels[vertical * 96 + horizontal] = new Color(1, 1, 1, Mathf.Pow(Mathf.Clamp01(1 - distance), 1.5f));
                    }
                texture.SetPixels(pixels); texture.Apply(false, true);
                sprite = Sprite.Create(texture, new Rect(0, 0, 96, 96), new Vector2(0.5f, 0.5f), 96);
            }
            var child = new GameObject("ShopFloorContact"); child.transform.SetParent(parent, false); child.transform.localPosition = floor;
            child.transform.localScale = new Vector3(width, width * 0.22f, 1);
            SpriteRenderer renderer = child.AddComponent<SpriteRenderer>(); renderer.sprite = sprite;
            renderer.color = new Color(0.01f, 0.018f, 0.035f, opacity); renderer.sortingOrder = 280; return renderer;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache() { if (sprite != null) Object.Destroy(sprite); if (texture != null) Object.Destroy(texture); texture = null; sprite = null; }
    }
}
