// The Archive Vault's ink fog lanes and its spilled ink pool (requirement 25/10/2026: fog and ink pools in a shader).
// A world-space quad over the view, drawn under the floor marks' ink and every body. Each fog lane is a band of curling ink
// mist with darker veins drifting along it (soft edges, no rim); the pool is one solid violet-black fill with a slightly
// living edge, a slow gloss sliding over it and a highlight at the one light direction (upper left).
Shader "ArcaneBrigade/MapArchiveVaultInk"
{
    Properties { }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off ZWrite Off ZTest Always Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            float4 _Lane0, _Lane1, _Lane2;  // xy one end, zw the other
            float4 _LaneHalf;               // xyz each lane's half width (0 = no lane)
            float4 _Pool;                   // xy centre, z radius now, w strength
            float4 _Ink, _Sheen, _PoolInk, _PoolGloss;
            float _Clock, _Amount;

            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; float2 world : TEXCOORD0; };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.world = mul(unity_ObjectToWorld, v.vertex).xy;
                return o;
            }

            float hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }
            float noise(float2 p)
            {
                float2 i = floor(p), f = frac(p);
                f = f * f * (3 - 2 * f);
                float a = hash(i), b = hash(i + float2(1, 0)), c = hash(i + float2(0, 1)), d = hash(i + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }
            float fbm(float2 p)
            {
                float s = 0, a = .5;
                for (int k = 0; k < 4; k++) { s += a * noise(p); p = p * 2.07 + float2(5.1, 1.3); a *= .5; }
                return s;
            }
            float lane(float2 w, float4 l, float halfWidth, float n)
            {
                if (halfWidth <= 0) return 0;
                float2 ab = l.zw - l.xy;
                float h = saturate(dot(w - l.xy, ab) / max(dot(ab, ab), .0001));
                float d = length(w - l.xy - ab * h);
                float ends = smoothstep(0, .07, h) * (1 - smoothstep(.93, 1, h));
                return (1 - smoothstep(halfWidth * .5, halfWidth * 1.15, d + (n - .5) * halfWidth * .8)) * ends;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 w = i.world;
                float t = _Clock;
                // ink fog: domain-warped mist curling along the floor, with darker veins
                float2 drift = float2(t * .09, -t * .05);
                float2 warp = float2(fbm(w * .5 + drift), fbm(w * .5 - drift + 5.2));
                float n = fbm(w * .9 + warp * 2 + float2(-t * .08, t * .04));
                float fog = max(max(lane(w, _Lane0, _LaneHalf.x, n), lane(w, _Lane1, _LaneHalf.y, n)), lane(w, _Lane2, _LaneHalf.z, n));
                float veins = smoothstep(.56, .63, fbm(w * 1.6 + warp * 3 + t * .05)) * fog;
                float fogA = fog * _Amount * (.55 + .4 * n);
                float3 fogColour = lerp(_Ink.rgb, _Sheen.rgb, saturate(n * 1.3 - .2)) * (1 - veins * .4);

                // the ink pool: one solid colour, its edge alive, a gloss sliding over it and a highlight from the upper left
                float pd = length(w - _Pool.xy);
                float wobble = (noise(w * 2.2 + t * .25) - .5) * .14;
                float inside = (1 - smoothstep(_Pool.z - .04 + wobble, _Pool.z + .03 + wobble, pd)) * _Pool.w;
                float centre = 1 - saturate(pd / max(.01, _Pool.z));
                float gloss = smoothstep(.78, .96, sin((w.x * .8 + w.y * 1.1) * 2.2 + t * .6 + n * 2)) * centre * .45;
                float2 shine = w - (_Pool.xy + float2(-.38, .3) * _Pool.z);
                float highlight = pow(saturate(1 - length(shine) / max(.01, .3 * _Pool.z)), 3) * .55;
                float3 poolColour = _PoolInk.rgb + _PoolGloss.rgb * (gloss + highlight);
                float poolA = inside * .8;

                float alpha = saturate(fogA + poolA - fogA * poolA);
                float3 colour = (fogColour * fogA * (1 - poolA) + poolColour * poolA) / max(alpha, .001);
                return fixed4(colour, alpha);
            }
            ENDCG
        }
    }
    Fallback Off
}
