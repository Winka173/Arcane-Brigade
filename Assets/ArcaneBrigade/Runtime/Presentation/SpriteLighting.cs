using UnityEngine;

namespace ArcaneBrigade
{
    public static class SpriteLighting
    {
        private static readonly int UvRectId = Shader.PropertyToID("_UvRect");
        private static readonly int VisibilityId = Shader.PropertyToID("_NightVisibility");
        private static readonly int ShadeColorId = Shader.PropertyToID("_ShadeColor");
        private static readonly int RimColorId = Shader.PropertyToID("_RimColor");
        private static readonly int ShadeAmountId = Shader.PropertyToID("_ShadeAmount");
        private static readonly int RimAmountId = Shader.PropertyToID("_RimAmount");
        private static readonly int SaturationId = Shader.PropertyToID("_Saturation");
        public static void SetSpriteRect(MaterialPropertyBlock properties, Sprite sprite)
        {
            Rect rectangle = sprite.rect;
            Texture texture = sprite.texture;
            properties.SetVector(UvRectId, new Vector4(rectangle.x / texture.width, rectangle.y / texture.height, rectangle.width / texture.width, rectangle.height / texture.height));
        }
        public static void Apply(SpriteRenderer renderer, MaterialPropertyBlock properties, bool hostile, bool prop = false)
        {
            if (renderer.sprite == null) return;
            SetSpriteRect(properties, renderer.sprite);
            properties.SetFloat(VisibilityId, 1);
            NightmareLighting lighting = NightmareLighting.Active;
            if (lighting == null)
            {
                properties.SetColor(ShadeColorId, hostile ? new Color(.48f, .51f, .69f) : new Color(.62f, .74f, .84f));
                properties.SetColor(RimColorId, hostile ? new Color(.89f, .54f, .66f) : new Color(1, .88f, .62f));
                properties.SetFloat(ShadeAmountId, prop ? .045f : .085f);
                properties.SetFloat(RimAmountId, prop ? .06f : .12f);
                properties.SetFloat(SaturationId, prop ? 1.06f : .93f);
            }
            else
            {
                Color spill = lighting.Spill(renderer.transform.position);
                float amount = Mathf.Clamp01(spill.maxColorComponent);
                Color rim = Color.Lerp(hostile ? new Color(.89f, .54f, .66f) : new Color(1, .88f, .62f), spill / Mathf.Max(.01f, spill.maxColorComponent), amount * .65f);
                properties.SetColor(RimColorId, rim);
                properties.SetColor(ShadeColorId, Color.Lerp(hostile ? new Color(.48f, .51f, .69f) : new Color(.62f, .74f, .84f), spill / Mathf.Max(.01f, spill.maxColorComponent), amount * .28f));
                properties.SetFloat(RimAmountId, (prop ? .09f : .16f) + amount * .2f);
                properties.SetFloat(ShadeAmountId, (prop ? .06f : .09f) + lighting.Depth * .015f);
                properties.SetFloat(SaturationId, prop ? .97f : .93f);
            }
        }
    }
}
