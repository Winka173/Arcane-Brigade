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
        _PatinaTex ("Painted patina layer", 2D) = "black" {}
        _PatinaStrength ("Patina blend", Range(0,1)) = 0
        _MicroStrength ("Fine material grain", Range(0,1)) = 0.08
        _ArenaSize ("Arena world size", Vector) = (30,20,0,0)
        _StageTime ("Paused stage clock", Float) = 0
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
            sampler2D _PatinaTex;
            float _PatinaStrength;
            float _MicroStrength;
            float4 _ArenaSize;
            float _StageTime;
            struct VertexInput { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct VertexOutput { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; float2 mapUV : TEXCOORD1; };
            VertexOutput vert(VertexInput input)
            {
                VertexOutput output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv * _TileScale.xy;
                output.mapUV = input.uv;
                return output;
            }
            fixed4 frag(VertexOutput input) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, input.uv) * _Color;
                color.rgb = lerp(_BaseColor.rgb, color.rgb, _Detail);
                fixed4 patina = tex2D(_PatinaTex, input.mapUV);
                color.rgb = lerp(color.rgb, patina.rgb * _Color.rgb, patina.a * _PatinaStrength);
                float grain = dot(tex2D(_MainTex, input.uv * 3.3 + float2(0.17,0.31)).rgb, float3(0.2126,0.7152,0.0722));
                color.rgb *= 1 + (grain - 0.25) * _MicroStrength;
                float2 world = (input.mapUV - 0.5) * _ArenaSize.xy;
                float macro = sin(world.x * 0.32 + sin(world.y * 0.21)) * cos(world.y * 0.27 - world.x * 0.11);
                color.rgb *= 1 + macro * 0.065;
                float edgeDistance = min((0.5 - abs(input.mapUV.x - 0.5)) * _ArenaSize.x, (0.5 - abs(input.mapUV.y - 0.5)) * _ArenaSize.y);
                color.rgb *= lerp(0.72, 1, smoothstep(0, 1.6, edgeDistance));
                float seam = smoothstep(0.45, 0.55, edgeDistance) * (1 - smoothstep(0.65, 0.8, edgeDistance));
                color.rgb = lerp(color.rgb, _BaseColor.rgb * 1.35, seam * 0.32);
                float centerLight = exp(-dot(world, world) * 0.012);
                color.rgb *= 1 + centerLight * 0.055;
                float mist = sin(world.x * 0.15 + _StageTime * 0.025) * cos(world.y * 0.18 - _StageTime * 0.018);
                color.rgb = lerp(color.rgb, _BaseColor.rgb, (mist * 0.5 + 0.5) * 0.045);
                float luminance = dot(color.rgb, float3(0.2126,0.7152,0.0722));
                color.rgb = saturate(lerp(luminance.xxx, color.rgb, _Saturation));
                color.a = 1;
                return color;
            }
            ENDCG
        }
    }
}
