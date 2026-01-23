Shader "ArcaneBrigade/UnitFlash"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Flash ("White flash", Range(0,1)) = 0
        _UvRect ("Sprite rectangle", Vector) = (0,0,1,1)
        _ShadeColor ("Ambient shade", Color) = (0.62,0.74,0.84,1)
        _RimColor ("Warm edge light", Color) = (1,0.88,0.62,1)
        _ShadeAmount ("Paint shading", Range(0,1)) = 0
        _RimAmount ("Edge strength", Range(0,1)) = 0
        _Saturation ("Paint saturation", Range(0,2)) = 1
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Cull Off Lighting Off ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnitySprites.cginc"
            float _Flash;
            float4 _UvRect;
            float4 _MainTex_TexelSize;
            fixed4 _ShadeColor;
            fixed4 _RimColor;
            float _ShadeAmount, _RimAmount, _Saturation;
            fixed4 frag(v2f input) : SV_Target
            {
                fixed4 color = SampleSpriteTexture(input.texcoord) * input.color;
                float2 local = saturate((input.texcoord - _UvRect.xy) / max(_UvRect.zw, 0.00001));
                color.rgb *= lerp(fixed3(1,1,1), _ShadeColor.rgb, (1 - local.y) * _ShadeAmount);
                float2 neighbor = input.texcoord + float2(-2,2) * _MainTex_TexelSize.xy;
                float inside = step(_UvRect.x, neighbor.x) * step(_UvRect.y, neighbor.y) * step(neighbor.x, _UvRect.x + _UvRect.z) * step(neighbor.y, _UvRect.y + _UvRect.w);
                float edge = saturate(color.a - SampleSpriteTexture(neighbor).a * inside);
                color.rgb = lerp(color.rgb, _RimColor.rgb, edge * _RimAmount);
                float luminance = dot(color.rgb, fixed3(0.2126,0.7152,0.0722));
                color.rgb = saturate(lerp(luminance.xxx, color.rgb, _Saturation));
                color.rgb = lerp(color.rgb, fixed3(1,1,1), _Flash);
                return color;
            }
            ENDCG
        }
    }
}
