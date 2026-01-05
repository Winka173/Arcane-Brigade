using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class CoinVisual : MonoBehaviour
    {
        private SpriteRenderer shadow;
        private SpriteRenderer glint;
        public void Display(PickupState state, SpriteRenderer renderer, Sprite disc, Sprite spark, float time, float interpolation)
        {
            if (shadow == null)
            {
                shadow = Create("CoinShadow");
                glint = Create("CoinGlint");
            }
            float age = Mathf.Max(0, time - state.SpawnTime);
            float landing = Mathf.Clamp01(age / 0.26f);
            float height = Mathf.Sin(landing * Mathf.PI) * 0.32f;
            Vector2 ground = Vector2.Lerp(state.PreviousPosition, state.Position, interpolation);
            renderer.transform.position = ground + Vector2.up * height;
            float spin = 0.42f + Mathf.Abs(Mathf.Cos(age * (state.Attracting ? 25 : 8) + state.Id)) * 0.58f;
            float scale = 0.24f / renderer.sprite.bounds.size.x;
            renderer.transform.localScale = new Vector3(scale * spin, scale, 1);
            renderer.transform.rotation = Quaternion.Euler(0, 0, state.Attracting ? Mathf.Sin(age * 12) * 12 : 0);
            renderer.color = Color.white;
            renderer.sortingOrder = 1700;
            shadow.gameObject.SetActive(true); shadow.sprite = disc;
            shadow.transform.position = ground;
            Vector3 parentScale = renderer.transform.lossyScale;
            shadow.transform.localScale = new Vector3(0.22f / parentScale.x, 0.07f / parentScale.y, 1) / disc.bounds.size.x;
            shadow.color = new Color(0.04f, 0.025f, 0.08f, 0.30f * (1 - height));
            shadow.sortingOrder = 490;
            float pulse = Mathf.Pow(Mathf.Max(0, Mathf.Cos(age * 4 + state.Id * 1.3f)), 12);
            glint.gameObject.SetActive(pulse > 0.05f); glint.sprite = spark;
            glint.transform.position = ground + new Vector2(0.06f * spin, height + 0.07f);
            float glintSize = (0.04f + pulse * 0.065f) / spark.bounds.size.x;
            glint.transform.localScale = new Vector3(glintSize / parentScale.x, glintSize / parentScale.y, 1);
            glint.color = new Color(1, 0.96f, 0.7f, pulse);
            glint.sortingOrder = 1701;
        }
        private SpriteRenderer Create(string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(transform, false);
            return child.AddComponent<SpriteRenderer>();
        }
        public void Hide() { if (shadow != null) shadow.gameObject.SetActive(false); if (glint != null) glint.gameObject.SetActive(false); }
        public void SetFade(SpriteRenderer renderer, float alpha)
        {
            Color body = renderer.color; body.a *= alpha; renderer.color = body;
            if (shadow != null) { Color color = shadow.color; color.a *= alpha; shadow.color = color; }
            if (glint != null) { Color color = glint.color; color.a *= alpha; glint.color = color; }
        }
    }
}
