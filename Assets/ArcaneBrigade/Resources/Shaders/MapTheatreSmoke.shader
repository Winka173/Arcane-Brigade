// The Forgotten Theatre's stage smoke and its spotlight on the floor (requirement 25/10/2026: smoke and light in a shader).
// A world-space quad over the view, drawn under the floor marks' ink and every body. Four low smoke banks curl and drift; the
// drum's beat opens a clear ring that travels out through them; the spotlight is a sharp-edged pool of coloured light that
// also lights the smoke inside it, with dust glinting in the beam. One colour per area, no rim.
Shader "ArcaneBrigade/MapTheatreSmoke"
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

            float4 _Bank0, _Bank1, _Bank2, _Bank3; // xy centre, zw radii
            float4 _BankDensity;                   // each bank's density 0..1 (thin while the drum parts it)
            float4 _Drum;                          // xy where the drum beat starts, z the ring's radius, w its strength
            float4 _Spot;                          // xy centre, z radius, w strength
            float4 _SpotColour, _SmokeLight, _SmokeShade;
            float _Clock, _Amount, _Dust;

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
                for (int k = 0; k < 4; k++) { s += a * noise(p); p = p * 2.03 + float2(1.7, 9.2); a *= .5; }
                return s;
            }
            float bank(float2 w, float4 b, float n)
            {
                float2 d = (w - b.xy) / max(b.zw, .01);
                return 1 - smoothstep(.42, 1.08, length(d) + (n - .5) * .6);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 w = i.world;
                float t = _Clock;
                // smoke: curling low banks (domain-warped noise drifting along the floor)
                float2 drift = float2(t * .16, sin(t * .19) * .2);
                float2 warp = float2(fbm(w * .42 + drift), fbm(w * .42 - drift + 3.1));
                float n = fbm(w * .75 + warp * 1.7 + float2(t * .11, -t * .03));
                float d = bank(w, _Bank0, n) * _BankDensity.x;
                d = max(d, bank(w, _Bank1, n) * _BankDensity.y);
                d = max(d, bank(w, _Bank2, n) * _BankDensity.z);
                d = max(d, bank(w, _Bank3, n) * _BankDensity.w);
                // the drum: a clear ring rolling out, smoke heaped at its front
                float dr = length(w - _Drum.xy);
                float cleared = 1 - smoothstep(_Drum.z - 1.6, _Drum.z - .2, dr);
                float edge = (dr - _Drum.z) * 1.3;
                float front = exp(-edge * edge);
                d = d * (1 - cleared * _Drum.w) + front * _Drum.w * .35 * saturate(d * 3);
                float smoke = saturate(d) * _Amount * (.55 + .45 * n);
                float3 smokeColour = lerp(_SmokeShade.rgb, _SmokeLight.rgb, saturate(n * 1.2 + warp.x * .3));

                // the spotlight: a sharp-edged pool of light, the smoke inside it lit in its colour, dust glinting in the beam
                float sd = length(w - _Spot.xy);
                float pool = (1 - smoothstep(_Spot.z * .9, _Spot.z, sd)) * _Spot.w;
                float halo = exp(-sd * sd / max(.01, _Spot.z * _Spot.z) * 1.6) * _Spot.w;
                float2 cell = floor(w * 7 + float2(0, t * .35));
                float sparkle = step(.972, hash(cell)) * (.5 + .5 * sin(t * 3.1 + hash(cell + 7) * 40)) * pool * _Dust;
                float light = pool * (.24 + .06 * halo) + sparkle * .5;
                smokeColour = lerp(smokeColour, _SpotColour.rgb * 1.15, saturate(pool * .85 + halo * .2));

                float alpha = saturate(smoke + light);
                float3 colour = (smokeColour * smoke + _SpotColour.rgb * light) / max(alpha, .001);
                return fixed4(colour, alpha);
            }
            ENDCG
        }
    }
    Fallback Off
}
