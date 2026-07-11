// The light of the 4th (purple) and 5th (rainbow) star (requirement 27/10/2026: light and shader, not strokes).
// An additive quad over the view, centred on the friend: a hot core, a pillar of light, slowly turning god rays, caustic
// shimmer, twinkling sparks and shock rings rolling over the floor. Purple mixes two violets; rainbow turns every hue
// round the friend. Everything is soft light, with no outline.
Shader "ArcaneBrigade/StarAura"
{
    Properties { }
    SubShader
    {
        Tags { "Queue"="Transparent+20" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off ZWrite Off ZTest Always Lighting Off
        Blend One One
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            float4 _Heart;   // xy the friend's heart in the world
            float4 _Rings;   // ages of four shock rings (negative: not launched)
            float4 _Rings2;  // four more
            float _Age, _Rise, _Flash, _Rainbow, _Fade, _Calm, _Ground;

            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; float2 world : TEXCOORD0; };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.world = mul(unity_ObjectToWorld, v.vertex).xy;
                return o;
            }

            float hash(float2 p) { p = frac(p * float2(123.34, 456.21)); p += dot(p, p + 45.32); return frac(p.x * p.y); }
            float noise(float2 p)
            {
                float2 i = floor(p), f = frac(p); f = f * f * (3 - 2 * f);
                return lerp(lerp(hash(i), hash(i + float2(1, 0)), f.x), lerp(hash(i + float2(0, 1)), hash(i + float2(1, 1)), f.x), f.y);
            }
            float3 hsv(float h, float s, float v)
            {
                float3 k = saturate(abs(frac(h + float3(0, 2.0 / 3.0, 1.0 / 3.0)) * 6 - 3) - 1);
                return v * lerp(float3(1, 1, 1), k, s);
            }
            // the colour at x (0..1): two violets, or the whole spectrum
            float3 pal(float x)
            {
                float3 purple = lerp(float3(.46, .2, 1), float3(1, .5, 1), saturate(x));
                return lerp(purple, hsv(x, .68, 1), _Rainbow);
            }
            float ringTerm(float age, float2 p)
            {
                if (age < 0 || age > 2.4) return 0;
                float k = age / 2.4, rad = 1 + 7.5 * (1 - (1 - k) * (1 - k));
                float d = length(float2(p.x, p.y * 1.7)) - rad;
                return exp(-d * d * 5) * (1 - k) * .9;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 p = i.world - _Heart.xy;
                float r = length(p), a = atan2(p.y, p.x), t = _Age;
                float hue = frac(a / 6.2831853 + r * .035 - t * .07);
                float3 col = 0;

                // a hot core that breathes, and flares with each flash
                col += pal(hue + .1) * exp(-r * r * .55) * (.55 + .12 * sin(t * 3) + _Flash * 2.4);
                col += float3(1, .96, 1) * exp(-r * r * 3) * _Flash * 1.2;

                // the pillar: a column of light from the floor, narrowing as it climbs
                float width = lerp(.55, 1.5, _Flash) * (1 + .15 * noise(float2(p.y * .7, t)));
                float column = exp(-p.x * p.x / (width * width)) * smoothstep(_Ground, _Ground + .6, p.y) * exp(-max(p.y, 0) * .16);
                col += pal(hue + p.y * .05) * column * _Rise * (.55 + _Flash);

                // god rays, slowly turning, broken up by drifting noise
                float n1 = noise(float2(a * 2.2 + t * .15, t * .3));
                float rays = pow(saturate(sin(a * 7 + t * .45 + n1 * 5) * .5 + .5), 5) + .55 * pow(saturate(sin(a * 15 - t * .3 + n1 * 3) * .5 + .5), 8);
                rays *= exp(-r * .2) * smoothstep(.4, 2.2, r) * _Rise;
                col += pal(hue + .3) * rays * (.7 + _Flash * .9);

                // caustic shimmer, like light through water, over the floor and the air
                float shimmer = noise(p * 2.6 + float2(t * .5, -t * .3)) * noise(p * 4.1 - float2(t * .4, t * .6));
                col += pal(hue + .6) * shimmer * exp(-r * .25) * _Rise * .9 * (1 - _Calm * .6);

                // shock rings
                float ringSum = ringTerm(_Rings.x, p) + ringTerm(_Rings.y, p) + ringTerm(_Rings.z, p) + ringTerm(_Rings.w, p)
                              + ringTerm(_Rings2.x, p) + ringTerm(_Rings2.y, p) + ringTerm(_Rings2.z, p) + ringTerm(_Rings2.w, p);
                col += pal(hue + .45) * ringSum * 1.5;

                // twinkling sparks: each cell may hold one that blinks, with a small cross of light
                if (_Calm < .5)
                {
                    float2 g = p * 3.2, cell = floor(g), f = frac(g) - .5;
                    float h = hash(cell), h2 = hash(cell + 7.7);
                    float2 off = (float2(h, h2) - .5) * .5;
                    float twinkle = pow(saturate(sin(t * (1.5 + h * 3) + h * 40) * .5 + .5), 10);
                    float2 q = f - off;
                    float spark = exp(-dot(q, q) * 260) + .5 * (exp(-q.x * q.x * 900 - q.y * q.y * 14) + exp(-q.y * q.y * 900 - q.x * q.x * 14));
                    col += (float3(1, .95, 1) * .5 + pal(h)) * spark * twinkle * step(.55, h) * exp(-r * .1) * _Rise * 1.6;
                }

                // the whole view takes a faint tint of the star's colour while the light is up
                col += pal(hue) * .05 * _Rise;
                col = 1 - exp(-col * 1.25);
                return fixed4(col * _Fade * .8, 1);
            }
            ENDCG
        }
    }
}
