using UnityEngine;

namespace ArcaneBrigade
{
    public static class SpriteLighting
    {
        public static void Apply(SpriteRenderer renderer, MaterialPropertyBlock properties, bool hostile, bool prop = false)
        {
            if (renderer.sprite == null) return;
            Rect rectangle = renderer.sprite.rect;
            Texture texture = renderer.sprite.texture;
            properties.SetVector("_UvRect", new Vector4(rectangle.x / texture.width, rectangle.y / texture.height, rectangle.width / texture.width, rectangle.height / texture.height));
            properties.SetColor("_ShadeColor", hostile ? new Color(0.48f, 0.51f, 0.69f) : new Color(0.62f, 0.74f, 0.84f));
            properties.SetColor("_RimColor", hostile ? new Color(0.89f, 0.54f, 0.66f) : new Color(1, 0.88f, 0.62f));
            properties.SetFloat("_ShadeAmount", prop ? 0.045f : 0.085f);
            properties.SetFloat("_RimAmount", prop ? 0.06f : 0.12f);
            properties.SetFloat("_Saturation", 1.06f);
        }
    }
}
