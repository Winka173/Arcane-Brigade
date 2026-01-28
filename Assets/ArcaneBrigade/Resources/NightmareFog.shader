Shader "ArcaneBrigade/NightmareFog"
{
    Properties { _FogAmount ("Fog", Float) = 0 _ShadowAmount ("Shadow", Float) = 0 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off ZWrite Off Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            float _FogAmount, _ShadowAmount, _FogClock, _Ceremony;
            float4 _RoomHalf, _PlayerAnchor;
            float4 _PlayerDrift;
            float4 _StageImpulse;
            struct VertexInput { float4 vertex : POSITION; };
            struct VertexOutput { float4 vertex : SV_POSITION; float2 world : TEXCOORD0; };
            VertexOutput vert(VertexInput input)
            {
                VertexOutput output; output.vertex = UnityObjectToClipPos(input.vertex);
                output.world = mul(unity_ObjectToWorld, input.vertex).xy; return output;
            }
            fixed4 frag(VertexOutput input) : SV_Target
            {
                float2 world = input.world;
                float mist = sin(world.x * .53 + _FogClock * .085 + sin(world.y * .37)) * cos(world.y * .68 - _FogClock * .055);
                float wisps = sin(world.y * 1.3 + sin(world.x * .61 + _FogClock * .04) * 1.4);
                float banks = smoothstep(-.4, .8, mist + wisps * .26);
                float2 displacement = world - _PlayerAnchor.xy;
                float wake = exp(-dot(displacement, displacement) * .22);
                float2 wakeOffset = displacement + _PlayerDrift.xy * .18;
                float movementWake = exp(-dot(wakeOffset, wakeOffset) * .6) * saturate(length(_PlayerDrift.xy) * .12);
                banks *= 1 - wake * .2 - movementWake * .4;
                float2 impactOffset = world - _StageImpulse.xy;
                banks *= 1 - exp(-dot(impactOffset, impactOffset) * .12) * _StageImpulse.z * .65;
                float shade = smoothstep(.25, .85, sin(world.x * .23 - _FogClock * .025) * cos(world.y * .31 + _FogClock * .018));
                float distanceToPlayer = length(world - _PlayerAnchor.xy);
                float safeCentre = smoothstep(1.8, 5.5, distanceToPlayer);
                float periphery = smoothstep(.25, .9, max(abs(world.x) / max(1, _RoomHalf.x), abs(world.y) / max(1, _RoomHalf.y)));
                float fog = banks * _FogAmount * lerp(.2, 1, safeCentre);
                float darkness = shade * _ShadowAmount * lerp(.15, 1, periphery) * safeCentre;
                float shaftDistance = min(abs(world.x - 7.5 - world.y * .18), abs(world.x + 7.5 + world.y * .18));
                float shafts = exp(-shaftDistance * shaftDistance * 2.2) * smoothstep(-8, 7, world.y);
                shafts *= (.72 + sin(world.y * .7 + world.x * .9 + _FogClock * .13) * .12) * _Ceremony * .16;
                float total = fog + darkness + shafts;
                float3 tint = lerp(float3(.025, .018, .04), float3(.34, .31, .43), fog / max(.001, total));
                tint = lerp(tint, float3(.76, .55, .3), shafts / max(.001, total));
                return fixed4(tint, min(.24, total));
            }
            ENDCG
        }
    }
}
