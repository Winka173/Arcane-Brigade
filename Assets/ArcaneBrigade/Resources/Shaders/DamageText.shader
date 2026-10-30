// Damage numbers in their element (owner 29/10/2026: every element its own colour and a touch of its own shape). _Mode is the
// ElementLook index. The digits are painted top to bottom in the element's two colours, move like the element (fire licks up,
// lightning jerks, water ripples, venom drips, wind leans, earth thuds, sound throbs...) and carry a pattern inside them (a hot
// core in fire, a running frost glint, bubbles in venom, a starry glitter in light, grain in earth...). Alpha fades the number.
Shader "ArcaneBrigade/DamageText"
{
    Properties
    {
        _MainTex ("Font", 2D) = "white" {}
        _Top ("Top", Color) = (1, 1, 1, 1)
        _Bottom ("Bottom", Color) = (1, 1, 1, 1)
        _Height ("Digit height (object space)", Float) = 0.25
        _Mode ("ElementLook index (0 plain)", Float) = 0
        _Seed ("Per-number seed", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Lighting Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex; float4 _MainTex_ST; fixed4 _Top, _Bottom; float _Height, _Mode, _Seed;
            struct appdata { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; float k : TEXCOORD1; float x : TEXCOORD2; };
            float hash(float n) { return frac(sin(n) * 43758.5453); }
            float hash2(float2 p) { float3 p3 = frac(float3(p.xyx) * .1031); p3 += dot(p3, p3.yzx + 33.33); return frac((p3.x + p3.y) * p3.z); }
            float vnoise(float2 x) { float2 i = floor(x), f = frac(x); f = f * f * (3 - 2 * f); return lerp(lerp(hash2(i), hash2(i + float2(1, 0)), f.x), lerp(hash2(i + float2(0, 1)), hash2(i + float2(1, 1)), f.x), f.y); }

            v2f vert (appdata v)
            {
                v2f o;
                float h = max(_Height, 0.001), t = _Time.y + _Seed, x = v.vertex.x / h, top = v.vertex.y > 0 ? 1 : 0, y = v.vertex.y / h;
                float4 p = v.vertex;
                int m = (int)round(_Mode);
                if (m == 1) { p.y += top * h * (0.2 + 0.15 * sin(t * 17 + x * 13) + 0.06 * sin(t * 29 + x * 7)); p.x += top * h * 0.12 * sin(t * 11 + x * 9); } // fire licks up
                else if (m == 4) { float s = floor(t * 20), c = floor(x * 2.5); p.x += h * 0.15 * (hash(s * 12.9 + c * 78.2) - 0.5); p.y += h * 0.13 * (hash(s * 39.3 + c * 11.7) - 0.5); } // lightning jerks
                else if (m == 3) p.y += h * 0.1 * sin(t * 7 + x * 4);                                  // water ripples
                else if (m == 8) p.y -= (1 - top) * h * (0.12 + 0.1 * sin(t * 5 + x * 9));             // venom drips
                else if (m == 5 || m == 17) p.x += h * 0.1 * sin(t * 5 + y * 6);                      // shadow twists
                else if (m == 7) p.x += top * h * (0.18 + 0.05 * sin(t * 9 + x));                      // wind leans the tops forward
                else if (m == 9) p.y -= h * 0.06 * abs(sin(t * 9));                                    // earth thuds down
                else if (m == 10) p.y += h * 0.07 * sin(t * 4 + x * 2);                                // bloom sways
                else if (m == 15) { float beat = pow(abs(sin(t * 6)), 8); p.xy *= 1 + 0.12 * beat; }   // sound throbs to a beat
                else if (m == 16) p.x += h * 0.05 * sign(sin(t * 4)) ;                                // time ticks side to side
                else if (m == 2 || m == 11) p.x += h * 0.03 * (hash(floor(t * 3) + floor(x * 2)) - .5); // ice / crystal: a brittle shiver
                o.pos = UnityObjectToClipPos(p); o.color = v.color; o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.k = saturate(y + 0.5); o.x = x;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed a = tex2D(_MainTex, i.uv).a * i.color.a;
                fixed3 c = lerp(_Bottom.rgb, _Top.rgb, i.k);
                float t = _Time.y + _Seed; int m = (int)round(_Mode); float2 g = float2(i.x, i.k * 1.6);
                if (m == 1) { float n = vnoise(g * float2(3, 2) - float2(0, t * 4)); c = lerp(c, float3(1, .97, .7), saturate((1 - i.k) * n * 1.4 - .25)); }          // a hot core flickers up
                else if (m == 4) c = lerp(c, 1, step(.86, hash(floor(t * 20) + floor(i.x * 2.5))));                                                                    // a flash now and then
                else if (m == 2 || m == 11) c = lerp(c, 1, .75 * saturate(1 - abs(frac(t * .8 - i.x * .25) - .5) * 8));                                              // a cold glint runs across
                else if (m == 3) c = lerp(c, float3(.8, .95, 1), .5 * smoothstep(.8, 1, sin(i.x * 5 - t * 6) * .5 + .5) * i.k);                                      // light on the wave crests
                else if (m == 8) { float b = vnoise(g * 5 + float2(0, -t * 2)); c = lerp(c, float3(.95, 1, .6), smoothstep(.72, .85, b)); }                          // bubbles in the venom
                else if (m == 5 || m == 17) c = lerp(c, float3(.05, .0, .1), .45 * vnoise(g * 3 + t * .7));                                                           // ink swirls through
                else if (m == 6 || m == 12) c = lerp(c, 1, step(.93, hash2(floor(g * 9) + floor(t * 8))));                                                            // glitter
                else if (m == 7) c = lerp(c, 1, .4 * smoothstep(.6, 1, sin(i.x * 3 - t * 14 + i.k * 4)));                                                             // gusts streak through
                else if (m == 9 || m == 13) c *= .82 + .3 * vnoise(g * 14);                                                                                            // grain of rock / paper
                else if (m == 10) c = lerp(c, float3(1, .85, .95), .4 * smoothstep(.75, .95, vnoise(g * 4 + t * .5)));                                                 // petals of colour
                else if (m == 15) c = lerp(c, 1, .35 * pow(abs(sin(t * 6)), 8));                                                                                      // flashes on the beat
                else if (m == 16) c = lerp(c, float3(1, .95, .7), .4 * step(.5, frac(t * 2)) * step(.5, frac(i.x * .5)));                                             // a ticking checker
                return fixed4(c * i.color.rgb, a);
            }
            ENDCG
        }
    }
}
