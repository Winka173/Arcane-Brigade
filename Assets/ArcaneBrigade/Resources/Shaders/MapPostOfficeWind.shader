// The Post Office's paper wind (requirement 25/10/2026): long streaks of draught racing across the whole view, world-anchored,
// in lanes of their own speed, some empty. Additive cream; _Extra.xy is the wind's direction, _Extra.z its strength (0..1).
// _Clock is frozen under Reduced Motion.
Shader "ArcaneBrigade/MapPostOfficeWind"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Clock ("Clock", Float) = 0
        _Seed ("Seed", Float) = 0
        _Extra ("Extra", Vector) = (1, 0, 1, 0)
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" "PreviewType" = "Plane" "CanUseSpriteAtlas" = "True" }
        Cull Off Lighting Off ZWrite Off
        Blend SrcAlpha One
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            float _Clock, _Seed;
            float4 _Extra;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; float2 w : TEXCOORD1; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; o.color = v.color; o.w = mul(unity_ObjectToWorld, v.vertex).xy; return o; }
            float hash (float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }
            fixed4 frag (v2f i) : SV_Target
            {
                float2 d = normalize(_Extra.xy + float2(1e-4, 0)), n = float2(-d.y, d.x);
                float u = dot(i.w, d), v = dot(i.w, n);
                float lane = floor(v / .5);
                float r1 = hash(float2(lane, _Seed)), r2 = hash(float2(lane, _Seed + 7.3));
                float lv = frac(v / .5) - .5 + sin(u * .7 + lane * 1.7) * .08;
                float len = 3.5 + 3 * r2, speed = 7 + 7 * r1;
                float run = (u - _Clock * speed) / len + r2;
                float s = frac(run);
                float streak = smoothstep(0, .55, s) * (1 - smoothstep(.55, .6, s)); // a long fading tail and a sharp head
                float thin = smoothstep(.07, 0, abs(lv - (r1 - .5) * .5));
                float gate = step(.4, hash(float2(lane, floor(run) + _Seed)));
                float2 q = i.uv * 2 - 1;
                float a = streak * thin * gate * _Extra.z * saturate(1.5 - dot(q, q) * .7);
                return fixed4(i.color.rgb, saturate(a * i.color.a));
            }
            ENDCG
        }
    }
}
