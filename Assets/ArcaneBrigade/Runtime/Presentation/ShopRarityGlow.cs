using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class ShopRarityGlow : MonoBehaviour
    {
        private static Sprite radial;
        private static Texture2D texture;
        private readonly SpriteRenderer[] lights = new SpriteRenderer[3];
        private Color tint;
        private ArenaController owner;
        private float phase;
        public void Configure(ItemRarity rarity, ArenaController controller, float offset)
        {
            owner = controller; phase = offset; tint = RunModifierRules.RarityColor(rarity);
            if (radial == null)
            {
                texture = new Texture2D(96, 96, TextureFormat.RGBA32, false) { name = "NightMarketSoftLight", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
                var pixels = new Color[96 * 96];
                for (int vertical = 0; vertical < 96; vertical++)
                    for (int horizontal = 0; horizontal < 96; horizontal++)
                    {
                        float distance = Vector2.Distance(new Vector2(horizontal, vertical), new Vector2(47.5f, 47.5f)) / 47.5f;
                        pixels[vertical * 96 + horizontal] = new Color(1, 1, 1, Mathf.Pow(Mathf.Clamp01(1 - distance), 2.2f));
                    }
                texture.SetPixels(pixels); texture.Apply(false, true);
                radial = Sprite.Create(texture, new Rect(0, 0, 96, 96), new Vector2(0.5f, 0.5f), 96);
            }
            for (int index = 0; index < lights.Length; index++)
            {
                var child = new GameObject("RarityLight" + index); child.transform.SetParent(transform, false);
                lights[index] = child.AddComponent<SpriteRenderer>(); lights[index].sprite = radial; lights[index].sortingOrder = 309 + index;
            }
        }
        private void Update()
        {
            if (owner == null || owner.Simulation.Phase == RunPhase.Paused) return;
            bool reduced = owner.Profile != null && owner.Profile.ReducedMotion;
            float time = owner.Simulation.VisualElapsed + phase;
            float pulse = reduced ? 1 : 0.88f + Mathf.Sin(time * 2.6f) * 0.12f;
            for (int index = 0; index < lights.Length; index++)
            {
                lights[index].color = new Color(tint.r, tint.g, tint.b, (index == 0 ? 0.85f : 0.5f) * pulse);
                lights[index].transform.localScale = index == 0 ? new Vector3(2.1f, 1.5f, 1) * pulse : new Vector3(0.42f, 2.2f, 1);
                lights[index].transform.localPosition = index == 0 ? Vector3.zero : new Vector3(index == 1 ? -0.2f : 0.2f, 0.38f, 0);
                lights[index].transform.localRotation = Quaternion.Euler(0, 0, index == 0 ? 0 : (index == 1 ? -1 : 1) * (16 + (reduced ? 0 : Mathf.Sin(time * 1.4f) * 8)));
            }
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache() { if (radial != null) Object.Destroy(radial); if (texture != null) Object.Destroy(texture); radial = null; texture = null; }
    }
}
