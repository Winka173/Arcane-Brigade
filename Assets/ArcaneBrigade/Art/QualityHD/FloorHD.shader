Shader "ArcaneBrigade/FloorHD"
{
    Properties
    {
        _MainTex ("Painted floor tile", 2D) = "white" {}
        _NextTex ("Next stage floor", 2D) = "white" {}
        _FloorBlend ("Stage material dissolve", Range(0,1)) = 0
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
        _NightmareFloor ("Nightmare painted carpet", 2D) = "black" {}
        _NightmareStrength ("Nightmare carpet blend", Range(0,1)) = 0
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
            #pragma target 3.0
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            sampler2D _NextTex;
            float4 _MainTex_TexelSize;
            float4 _NextTex_TexelSize;
            float _FloorBlend;
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
            sampler2D _NightmareFloor;
            float _NightmareStrength;
            float _DreamEnabled, _DreamDepth, _DreamAmbient, _DreamClock, _DreamDawn;
            float4 _DreamLight0, _DreamLight1, _DreamLight2, _DreamLight3;
            float4 _DreamColor0, _DreamColor1, _DreamColor2, _DreamColor3;
            struct VertexInput { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct VertexOutput { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; float2 mapUV : TEXCOORD1; float2 world : TEXCOORD2; };
            VertexOutput vert(VertexInput input)
            {
                VertexOutput output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv * _TileScale.xy;
                output.mapUV = input.uv;
                output.world = mul(unity_ObjectToWorld, input.vertex).xy;
                return output;
            }
            fixed4 ContinuousFloor(sampler2D floorSampler, float2 uv, float2 texel)
            {
                float2 folded = 1 - abs(frac(uv * 0.5) * 2 - 1);
                return tex2D(floorSampler, clamp(folded, texel * 0.5, 1 - texel * 0.5));
            }
            float3 DreamSpill(float2 world, float4 source, float3 tint)
            {
                float2 offset = world - source.xy;
                offset.y *= 1.25;
                float irregular = 1 + sin(world.x * 1.7 + world.y * .9) * .06;
                return tint * exp(-dot(offset, offset) * 2.7 * irregular / max(.01, source.z * source.z)) * source.w;
            }
            fixed4 frag(VertexOutput input) : SV_Target
            {
                fixed4 color = lerp(ContinuousFloor(_MainTex, input.uv, _MainTex_TexelSize.xy), ContinuousFloor(_NextTex, input.uv, _NextTex_TexelSize.xy), _FloorBlend) * _Color;
                color.rgb = lerp(_BaseColor.rgb, color.rgb, _Detail);
                color.rgb = lerp(color.rgb, tex2D(_NightmareFloor, input.mapUV).rgb * _Color.rgb, _NightmareStrength);
                fixed4 patina = tex2D(_PatinaTex, input.mapUV);
                color.rgb = lerp(color.rgb, patina.rgb * _Color.rgb, patina.a * _PatinaStrength);
                float grain = dot(ContinuousFloor(_MainTex, input.uv * 3.3 + float2(0.17,0.31), _MainTex_TexelSize.xy).rgb, float3(0.2126,0.7152,0.0722));
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
                if (_DreamEnabled > .5)
                {
                    float2 room = input.world;
                    float depth = saturate(_DreamDepth * .25) * (1 - _DreamDawn);
                    float warped = room.x + sin(room.y * .65 + _DreamClock * .06) * .28;
                    float grainLines = pow(saturate(sin(warped * 4.2 + sin(room.y * 1.3))), 18);
                    float ribs = pow(saturate(sin(abs(room.x) * 1.15 + sin(room.y * .4) * .5)), 24);
                    float veins = pow(saturate(sin(warped * 2.8 + sin(room.y * 2.1) * 1.4)), 30);
                    float margin = smoothstep(5, 13, abs(room.x)) * .6;
                    float stain = grainLines * .035 + veins * depth * .11 + ribs * smoothstep(.5, 1, depth) * margin * .18;
                    color.rgb *= 1 - stain;
                    float3 spill = DreamSpill(room, _DreamLight0, _DreamColor0.rgb)
                        + DreamSpill(room, _DreamLight1, _DreamColor1.rgb)
                        + DreamSpill(room, _DreamLight2, _DreamColor2.rgb)
                        + DreamSpill(room, _DreamLight3, _DreamColor3.rgb);
                    float floorHaze = (sin(room.x * .7 + _DreamClock * .09) * sin(room.y * .45 - _DreamClock * .07) * .5 + .5) * margin * depth * .035;
                    float3 dreamTint = lerp(float3(1.12, 1.03, .9), float3(.9, .86, 1.04), depth);
                    color.rgb = color.rgb * dreamTint * (_DreamAmbient + spill * .65) + spill * .07;
                    color.rgb = lerp(color.rgb, float3(.13, .11, .18), floorHaze);
                    color.rgb = lerp(color.rgb, color.rgb * float3(1.12, 1.03, .88), _DreamDawn * .35);
                }
                color.a = 1;
                return color;
            }
            ENDCG
        }
    }
}
