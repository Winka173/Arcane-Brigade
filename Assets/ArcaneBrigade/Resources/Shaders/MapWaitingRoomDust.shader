// The Waiting Room's ceiling dust (requirement 25/10/2026): a soft shaft of dusty light sifting down onto a dust patch, slow
// drifting veils and motes falling through it. Additive, so it only ever lightens; the vertex colour is its tint and strength.
// _Clock is frozen under Reduced Motion; _Seed keeps each shaft its own.
Shader "ArcaneBrigade/MapWaitingRoomDust"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Clock ("Clock", Float) = 0
        _Seed ("Seed", Float) = 0
        _Extra ("Extra", Vector) = (0, 0, 0, 0)
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
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; o.color = v.color; return o; }
            float hash (float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }
            float noise (float2 p)
            {
                float2 i = floor(p), f = frac(p); f = f * f * (3 - 2 * f);
                return lerp(lerp(hash(i), hash(i + float2(1, 0)), f.x), lerp(hash(i + float2(0, 1)), hash(i + float2(1, 1)), f.x), f.y);
            }
            fixed4 frag (v2f i) : SV_Target
            {
                float x = i.uv.x * 2 - 1, y = i.uv.y; // y: 0 on the floor, 1 toward the ceiling
                float w = lerp(1.0, .72, y);           // the shaft narrows a little toward the ceiling
                float side = saturate(1 - (x / w) * (x / w)); side *= side;
                float ends = smoothstep(0, .1, y) * (1 - smoothstep(.5, 1, y));
                float veil = noise(float2(x * 2.2 + _Seed, y * 3 + _Clock * .35)) * .6 + noise(float2(x * 5 - _Seed, y * 7 + _Clock * .8)) * .4;
                float beam = side * ends * (.3 + .7 * veil);
                // motes: a grid scrolled downward, one speck in some of its cells
                float2 g = float2(x * 7 + _Seed * 3.1, y * 12 + _Clock * 1.4);
                float2 c = floor(g), f = frac(g) - .5;
                float2 o = float2(hash(c + 3.7) - .5, hash(c + 9.1) - .5) * .6;
                float mote = step(.62, hash(c + _Seed)) * smoothstep(.16, .03, length(f - o)) * side * smoothstep(0, .2, y);
                float a = saturate(beam * .55 + mote * .85) * i.color.a;
                return fixed4(i.color.rgb, a);
            }
            ENDCG
        }
    }
}
