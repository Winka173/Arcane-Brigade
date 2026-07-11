// The thread and the halo of a star-up (requirement 27/10/2026: the thread winding round the friend and the crown above its head
// were ink strokes and a painted crown; now they are light). One world-space quad over the friend:
// - strands of light wind up the body in a helix (front passes bright, back passes dim), then knot and flash;
// - above the head a halo: a soft ring of light with a fan of rays, turning slowly, brightening in a burst when it appears.
// Colours come from _Tone (silver, gold, purple) or the spectrum when _Rainbow is 1. Additive, no outline.
Shader "ArcaneBrigade/StarWeave"
{
    Properties { }
    SubShader
    {
        Tags { "Queue"="Transparent+18" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off ZWrite Off ZTest Always Lighting Off
        Blend One One
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            float4 _Rect;    // x centre, y feet, z half width, w height of the friend
            float4 _Weave;   // x head (0..1), y tail (0..1), z strands, w turns
            float4 _Halo;    // x centre, y centre, z size, w strength
            float4 _Tone;
            float _Clock, _Knot, _Rainbow, _Fade;

            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; float2 world : TEXCOORD0; };
            v2f vert(appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.world = mul(unity_ObjectToWorld, v.vertex).xy; return o; }

            float3 hsv(float h, float s, float v)
            {
                float3 k = saturate(abs(frac(h + float3(0, 2.0 / 3.0, 1.0 / 3.0)) * 6 - 3) - 1);
                return v * lerp(float3(1, 1, 1), k, s);
            }
            float3 tone(float x) { return lerp(_Tone.rgb, hsv(x, .6, 1), _Rainbow); }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 col = 0;
                float2 p = i.world;
                // ---- the thread
                float y = (p.y - _Rect.y) / _Rect.w;
                float u = (p.x - _Rect.x) / _Rect.z;
                if (_Weave.x > _Weave.y && y > -.05 && y < 1.25)
                {
                    float live = smoothstep(_Weave.y - .02, _Weave.y + .06, y) * (1 - smoothstep(_Weave.x - .06, _Weave.x, y));
                    float n = max(1, _Weave.z);
                    float squeeze = 1 - _Knot * .85;
                    for (int s = 0; s < 4; s++)
                    {
                        if (s >= n) break;
                        float phase = y * 6.2832 * _Weave.w - _Clock * 5 + s * 6.2832 / n;
                        float x = sin(phase) * 1.25 * squeeze;
                        float depth = cos(phase) * .5 + .5;
                        float d = u - x;
                        float strand = exp(-d * d * 150) * (.35 + .9 * depth);
                        col += tone(y * .6 + s * .18 + _Clock * .1) * strand * live * 1.3;
                        col += float3(1, 1, 1) * exp(-d * d * 900) * depth * live * .6;
                    }
                    // the head of the thread glows
                    float head = exp(-pow((y - _Weave.x) * 9, 2)) * exp(-u * u * 1.2);
                    col += tone(.3) * head * .9;
                }
                // the knot's flash round the whole body
                float2 q = float2(u * .8, (y - .5) * .9);
                col += tone(.5) * exp(-dot(q, q) * 4) * _Knot * .9;
                // ---- the halo above the head
                if (_Halo.w > 0)
                {
                    float2 h = (p - _Halo.xy) / _Halo.z;
                    float2 e = float2(h.x, h.y * 2.1);
                    float r = length(e), a = atan2(e.y, e.x);
                    float ring = exp(-pow(r - .8, 2) * 60);
                    float rays = pow(saturate(sin(a * 7 + _Clock * .8) * .5 + .5), 6) * exp(-r * 1.6) * smoothstep(.35, .8, r);
                    float core = exp(-r * r * 5) * .5;
                    float3 c = tone(a / 6.2832 + _Clock * .1);
                    col += c * (ring * 1.1 + rays * .8 + core) * _Halo.w;
                    col += float3(1, .96, .9) * exp(-r * r * 40) * _Halo.w * .6;
                }
                col = 1 - exp(-col * 1.2);
                return fixed4(col * _Fade, 1);
            }
            ENDCG
        }
    }
}
