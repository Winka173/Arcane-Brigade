// The chime over a stall when its purchase would star two copies up (requirement 27/10/2026): a warm bell of light that breathes,
// soft rings rolling out like a struck bell's sound, a turning glint and motes lifting from it. Additive over a quad on the stall.
Shader "ArcaneBrigade/MergeChime"
{
    Properties { _Clock ("Clock", Float) = 0 _Tint ("Tint", Color) = (1, .82, .4, 1) _Amount ("Amount", Float) = 1 }
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
            float _Clock, _Amount; float4 _Tint;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };
            v2f vert(appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o; }
            float hash(float2 p) { p = frac(p * float2(123.34, 456.21)); p += dot(p, p + 45.32); return frac(p.x * p.y); }
            fixed4 frag(v2f i) : SV_Target
            {
                float2 p = (i.uv - .5) * 2;
                float r = length(p), t = _Clock;
                float3 col = 0;
                col += _Tint.rgb * exp(-r * r * 7) * (.55 + .2 * sin(t * 3.4));
                for (int k = 0; k < 3; k++)
                {
                    float q = frac(t * .45 + k / 3.0), rad = q * .95;
                    float d = length(float2(p.x, p.y * 1.25)) - rad;
                    col += _Tint.rgb * exp(-d * d * 90) * (1 - q) * (1 - q) * .8;
                }
                float a = t * .9, c = cos(a), s = sin(a);
                float2 rp = float2(c * p.x - s * p.y, s * p.x + c * p.y);
                float cross = exp(-rp.x * rp.x * 900 - rp.y * rp.y * 12) + exp(-rp.y * rp.y * 900 - rp.x * rp.x * 12);
                col += float3(1, .95, .8) * cross * exp(-r * 2.4) * (.5 + .5 * sin(t * 5));
                float2 g = float2(p.x * 7, p.y * 7 - t * .8), cell = floor(g), f = frac(g) - .5;
                float h = hash(cell), twinkle = pow(saturate(sin(t * (2 + h * 3) + h * 40) * .5 + .5), 8);
                col += float3(1, .95, .8) * exp(-dot(f - (float2(h, hash(cell + 3)) - .5) * .4, f - (float2(h, hash(cell + 3)) - .5) * .4) * 160) * twinkle * step(.6, h) * (1 - r);
                col *= smoothstep(1, .6, r);
                return fixed4(col * _Amount, 1);
            }
            ENDCG
        }
    }
}
