Shader "ArcaneBrigade/FloorHD"
{
    Properties
    {
        _MainTex ("Painted floor tile", 2D) = "white" {}
        _TileScale ("World tiling", Vector) = (1,1,0,0)
        _Color ("Palette", Color) = (1,1,1,1)
        _Saturation ("Saturation", Range(0,2)) = 1.15
        _BaseColor ("Quiet ground", Color) = (0.12,0.25,0.34,1)
        _Detail ("Paint contrast", Range(0,1)) = 0.6
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Opaque" }
        Cull Off ZWrite Off Lighting Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _TileScale;
            fixed4 _Color;
            float _Saturation;
            fixed4 _BaseColor;
            float _Detail;
            struct VertexInput { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct VertexOutput { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; };
            VertexOutput vert(VertexInput input)
            {
                VertexOutput output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv * _TileScale.xy;
                return output;
            }
            fixed4 frag(VertexOutput input) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, input.uv) * _Color;
                color.rgb = lerp(_BaseColor.rgb, color.rgb, _Detail);
                float luminance = dot(color.rgb, float3(0.2126,0.7152,0.0722));
                color.rgb = saturate(lerp(luminance.xxx, color.rgb, _Saturation));
                color.a = 1;
                return color;
            }
            ENDCG
        }
    }
}
