using UnityEngine;

namespace ArcaneBrigade
{
    public static class SpriteLighting
    {
        public static void SetSpriteRect(MaterialPropertyBlock properties, Sprite sprite)
        {
            Rect rectangle = sprite.rect;
            Texture texture = sprite.texture;
            properties.SetVector("_UvRect", new Vector4(rectangle.x / texture.width, rectangle.y / texture.height, rectangle.width / texture.width, rectangle.height / texture.height));
        }
        public static void Apply(SpriteRenderer renderer, MaterialPropertyBlock properties, bool hostile, bool prop = false)
        {
            if (renderer.sprite == null) return;
            SetSpriteRect(properties, renderer.sprite);
            properties.SetColor("_ShadeColor", hostile ? new Color(0.48f, 0.51f, 0.69f) : new Color(0.62f, 0.74f, 0.84f));
            properties.SetColor("_RimColor", hostile ? new Color(0.89f, 0.54f, 0.66f) : new Color(1, 0.88f, 0.62f));
            properties.SetFloat("_ShadeAmount", prop ? 0.045f : 0.085f);
            properties.SetFloat("_RimAmount", prop ? 0.06f : 0.12f);
            properties.SetFloat("_Saturation", 1.06f);
            NightmareLighting lighting = NightmareLighting.Active;
            if (lighting != null)
            {
                Color spill = lighting.Spill(renderer.transform.position);
                float amount = Mathf.Clamp01(spill.maxColorComponent);
                Color rim = Color.Lerp(hostile ? new Color(.89f, .54f, .66f) : new Color(1, .88f, .62f), spill / Mathf.Max(.01f, spill.maxColorComponent), amount * .65f);
                properties.SetColor("_RimColor", rim);
                properties.SetColor("_ShadeColor", Color.Lerp(hostile ? new Color(.48f, .51f, .69f) : new Color(.62f, .74f, .84f), spill / Mathf.Max(.01f, spill.maxColorComponent), amount * .28f));
                properties.SetFloat("_RimAmount", (prop ? .09f : .16f) + amount * .2f);
                properties.SetFloat("_ShadeAmount", (prop ? .06f : .09f) + lighting.Depth * .015f);
                properties.SetFloat("_Saturation", prop ? .97f : 1.03f);
            }
        }
    }
}
