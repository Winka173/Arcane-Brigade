// The thread that ties two copies to the stall when a purchase would star them up (requirement 27/10/2026: light and shader).
// A ribbon of gold light along a LineRenderer (u runs along it, v across): a soft halo, a fine bright core, and pulses of light
// travelling from the copy to the stall, with sparks twinkling along it. Additive, no outline.
Shader "ArcaneBrigade/MergeThread"
{
    Properties { _Clock ("Clock", Float) = 0 _Tint ("Tint", Color) = (1, .82, .4, 1) }
    SubShader
    {
        Tags { "Queue"="Transparent+15" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off ZWrite Off ZTest Always Lighting Off
        Blend One One
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            float _Clock; float4 _Tint;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };
            v2f vert(appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o; }
            float hash(float2 p) { p = frac(p * float2(123.34, 456.21)); p += dot(p, p + 45.32); return frac(p.x * p.y); }
            fixed4 frag(v2f i) : SV_Target
            {
                float u = i.uv.x, v = (i.uv.y - .5) * 2;
                float ends = sin(saturate(u) * 3.14159);
                float core = exp(-v * v * 60), halo = exp(-v * v * 5) * .35;
                float pulse = pow(saturate(sin((u * 3.2 - _Clock * 1.1) * 6.2832) * .5 + .5), 9);
                float wob = .75 + .25 * sin(u * 30 - _Clock * 6);
                float cell = floor(u * 36 + _Clock * 2), h = hash(float2(cell, 3.1));
                float spark = step(.8, h) * pow(saturate(sin(_Clock * 6 + h * 50) * .5 + .5), 6) * exp(-pow(v - (h - .5) * 1.4, 2) * 40);
                float3 col = _Tint.rgb * (halo * wob + core * (.55 + pulse * 1.6)) + float3(1, .95, .85) * (core * pulse * .8 + spark * 1.4);
                return fixed4(col * ends * .9, 1);
            }
            ENDCG
        }
    }
}
