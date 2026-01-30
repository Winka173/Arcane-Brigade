Shader "ArcaneBrigade/MirrorWitness"
{
    Properties { [PerRendererData] _MainTex ("Reflection", 2D) = "white" {} _Color ("Tint", Color) = (1,1,1,1) }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Cull Off ZWrite Off Lighting Off Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment frag
            #include "UnitySprites.cginc"
            fixed4 frag(v2f input) : SV_Target
            {
                fixed4 color = SampleSpriteTexture(input.texcoord) * input.color;
                float light = dot(color.rgb, float3(.2126,.7152,.0722));
                color.rgb = lerp(light.xxx * float3(.45,.7,.85), color.rgb, .35);
                return color;
            }
            ENDCG
        }
    }
}
