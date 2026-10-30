// A painted effect area: an organic, slowly boiling edge (never a rim line), colour swirling inside, light streaks for friendly
// areas and one uniform colour from centre to edge for enemy danger. Shapes: 0 ellipse, 1 lane (capsule), 2 cone (apex at left).
Shader "ArcaneBrigade/InkArea"
{
    Properties
    {
        _MainTex ("Painted ground", 2D) = "white" {}
        _ColorA ("Core colour", Color) = (1,1,1,1)
        _ColorB ("Outer colour", Color) = (1,1,1,1)
        _Params ("grow, fade, boil, seed", Vector) = (1,1,.12,0)
        _Params2 ("swirl, uniform, texture amount, streaks", Vector) = (1,0,0,1)
        _Shape ("kind, lane straight part, cone half angle, -", Vector) = (0,0,.5,0)
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Source blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Destination blend", Float) = 10
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" }
        Blend [_SrcBlend] [_DstBlend]
        ZWrite Off
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _ColorA, _ColorB, _Params, _Params2, _Shape;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o; }

            float hash (float2 p) { p = frac(p * float2(123.34, 456.21)); p += dot(p, p + 45.32); return frac(p.x * p.y); }
            float noise (float2 p)
            {
                float2 i = floor(p), f = frac(p); f = f * f * (3 - 2 * f);
                return lerp(lerp(hash(i), hash(i + float2(1,0)), f.x), lerp(hash(i + float2(0,1)), hash(i + float2(1,1)), f.x), f.y);
            }
            float fbm (float2 p) { float v = 0, a = .5; for (int k = 0; k < 4; k++) { v += a * noise(p); p *= 2.03; a *= .5; } return v; }

            fixed4 frag (v2f i) : SV_Target
            {
                float t = _Time.y, seed = _Params.w, grow = max(.001, _Params.x);
                float2 p = i.uv * 2 - 1;
                float d;
                if (_Shape.x < .5) d = length(p);
                else if (_Shape.x < 1.5) d = length(float2(max(abs(p.x) - _Shape.y, 0) / max(.001, 1 - _Shape.y), p.y));
                else
                {
                    float2 c = float2(i.uv.x, p.y * tan(_Shape.z));
                    float ang = atan2(c.y, max(.0001, c.x));
                    d = max(length(c), abs(ang) / max(.01, _Shape.z));
                }
                float n = fbm(p * 2.3 + float2(t * .18, -t * .12) + seed * 7.1);
                float edge = d + (n - .5) * _Params.z * 2;
                float alpha = smoothstep(grow, grow - .1, edge);
                float swirl = fbm(p * 3.2 + float2(sin(t * .7 + seed), cos(t * .55 + seed)) * .7 + n * 1.5);
                float k = saturate(pow(saturate(edge / grow), 1.6) + (swirl - .5) * _Params2.x * .8);
                float3 col = lerp(_ColorA.rgb, _ColorB.rgb, k);
                // bright flowing streaks and a soft inner glow, so a friendly area reads as moving light
                float streak = smoothstep(.58, .68, swirl) * smoothstep(.95, .4, edge / grow) * _Params2.w;
                col += streak * .6 * (1 - _Params2.y);
                col = lerp(col, _ColorA.rgb, _Params2.y);
                alpha *= lerp(lerp(_ColorA.a, _ColorB.a, k), _ColorA.a, _Params2.y);
                fixed4 tex = tex2D(_MainTex, i.uv);
                col = lerp(col, col * tex.rgb * 1.3, _Params2.z);
                alpha *= lerp(1, tex.a, _Params2.z);
                return float4(col, alpha * _Params.y);
            }
            ENDCG
        }
    }
}
