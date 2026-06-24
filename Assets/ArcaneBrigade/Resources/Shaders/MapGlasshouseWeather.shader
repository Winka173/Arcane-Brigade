// The Glasshouse's weather (requirement 25/10/2026, Docs/Map_Mechanics_2026-10-25.md section 3), drawn on one view-sized quad per
// layer and anchored to the world so it never slides with the camera.
// _Layer 0 (the floor, under the floor marks): sun shafts from the upper left (the one key light), the roof's iron frame as soft
// shadow lines, glass caustics dappling the floor; in the rain a wet sheen, puddles that mirror the sky, drop ripples and glints.
// _Layer 1 (the air, over the bodies): falling rain streaks in two depths; in the sun a few warm motes turning in the shafts.
// Premultiplied blending: light adds (rgb with no alpha), shade darkens (alpha), so nothing is ever outlined.
Shader "ArcaneBrigade/MapGlasshouseWeather"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Sun ("Sun", Range(0, 1)) = 1
        _Rain ("Rain", Range(0, 1)) = 0
        _Clock ("Clock", Float) = 0
        _Layer ("Layer", Float) = 0
        _Calm ("Calm", Float) = 0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" "PreviewType" = "Plane" }
        Cull Off Lighting Off ZWrite Off
        Blend One OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            float _Sun, _Rain, _Clock, _Layer, _Calm;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 pos : SV_POSITION; float2 world : TEXCOORD0; fixed4 color : COLOR; };
            v2f vert (appdata v)
            {
                v2f o; o.pos = UnityObjectToClipPos(v.vertex);
                o.world = mul(unity_ObjectToWorld, v.vertex).xy; o.color = v.color; return o;
            }
            float hash (float2 p) { p = frac(p * float2(123.34, 456.21)); p += dot(p, p + 45.32); return frac(p.x * p.y); }
            float noise (float2 p)
            {
                float2 i = floor(p), f = frac(p); f = f * f * (3 - 2 * f);
                return lerp(lerp(hash(i), hash(i + float2(1, 0)), f.x), lerp(hash(i + float2(0, 1)), hash(i + 1), f.x), f.y);
            }
            float fbm (float2 p) { return noise(p) * .6 + noise(p * 2.07 + 7.3) * .28 + noise(p * 4.13 + 3.1) * .12; }

            // the floor: shafts, frame shadows and caustics in the sun; sheen, puddles, ripples and glints in the rain
            float4 Floor (float2 p, float t)
            {
                const float2 lightWay = float2(.56, -.83);          // the light travels down and to the right
                const float2 across = float2(.83, .56);
                float u = dot(p, across), v = dot(p, lightWay);
                float3 add = 0; float shade = 0;
                if (_Sun > .001)
                {
                    // long soft shafts: bands across the light that breathe slowly and thin along their length
                    float bands = smoothstep(.48, .92, noise(float2(u * .32 + t * .035, 2.7))) * .75 + smoothstep(.62, .95, noise(float2(u * .9 - t * .05, 9.1))) * .35;
                    float along = .55 + .45 * noise(float2(u * .15, v * .07 - t * .04));
                    add += float3(1, .9, .62) * bands * along * .17;
                    // the glass roof's iron frame laid on the floor as soft shadow lines, skewed by the same light
                    float2 g = float2(p.x + p.y * .38, p.y) / float2(3.4, 2.5);
                    float2 e = abs(frac(g) - .5);
                    float frame = 1 - smoothstep(.455, .49, max(e.x, e.y * .98));
                    shade += (1 - frame) * .13 * (.6 + .4 * bands);
                    // caustics: light bent by the old panes, dappling the floor
                    float2 q = p * 1.15 + float2(t * .11, -t * .07);
                    float c = noise(q) * noise(q * 1.7 + float2(-t * .09, t * .13) + 4.1);
                    add += float3(1, .95, .75) * smoothstep(.36, .62, c) * .11 * (.4 + .6 * bands);
                    add *= _Sun; shade *= _Sun;
                }
                if (_Rain > .001)
                {
                    // a wet sheen over everything, a little cooler and darker
                    float3 wet = float3(.015, .03, .04);
                    float puddle = smoothstep(.56, .66, fbm(p * .2 + 3.3));
                    float sky = smoothstep(.35, 1, noise(float2(p.x * .5 + t * .06, p.y * 3.1)));
                    float3 r = wet + float3(.32, .45, .52) * puddle * (.08 + .1 * sky);
                    float rs = .1 + puddle * .1;
                    // drop rings on the puddles (squashed into the floor), fewer on the open floor
                    float2 cellSize = float2(1.15, .75);
                    float2 cell = floor(p / cellSize), local = frac(p / cellSize) - .5;
                    float h = hash(cell), period = .9 + h * .7;
                    float age = frac(t / period + h * 7.1);
                    float2 centre = (float2(hash(cell + 3.1), hash(cell + 8.7)) - .5) * .6;
                    float2 d = (local - centre) * cellSize * float2(1, 1.7);
                    float ring = exp(-abs(length(d) - age * .38) * 38) * (1 - age) * (1 - age);
                    float rings = ring * lerp(.25, 1, puddle) * step(.35, hash(cell + floor(t / period + h * 7.1)));
                    r += float3(.75, .9, 1) * rings * .32;
                    // glints that wink on the wet floor
                    float2 gc = floor(p * 1.9); float gh = hash(gc + 1.7);
                    float wink = pow(saturate(sin(t * (1.3 + gh * 2.2) + gh * 40)), _Calm > .5 ? 2 : 14);
                    float2 gd = (frac(p * 1.9) - .5 - (float2(hash(gc + 5.2), hash(gc + 9.4)) - .5) * .5) * float2(1, 1.6);
                    r += float3(.95, 1, 1) * exp(-dot(gd, gd) * 900) * wink * step(.72, gh) * (.35 + .65 * puddle) * .9;
                    add += r * _Rain; shade += rs * _Rain;
                }
                return float4(add, shade);
            }

            // the air: rain streaks in two depths, falling a little slanted with the light's way; motes in the sun
            float Streaks (float2 p, float t, float width, float spacing, float speed, float length, float density)
            {
                float2 q = float2(p.x - p.y * .18, p.y);
                float col = floor(q.x / spacing), h = hash(float2(col, 3.7));
                float fall = q.y + t * speed * (.8 + .4 * h) + h * 31;
                float seg = fall / length, slot = floor(seg), f = frac(seg);
                float on = step(1 - density, hash(float2(col, slot)));
                float dx = abs(frac(q.x / spacing) - .5 - (hash(float2(col, slot + 4.4)) - .5) * .5) * spacing;
                float stem = smoothstep(width, width * .2, dx);
                float body = smoothstep(0, .06, f) * smoothstep(.34, .06, f); // a short streak with a soft head and a long tail
                return stem * body * on;
            }
            float4 Air (float2 p, float t)
            {
                float3 add = 0; float shade = 0;
                if (_Rain > .001)
                {
                    float rainNear = Streaks(p, t, .022, .31, 13, 2.7, .55), rainFar = Streaks(p * 1.6 + 11, t * 1.25, .014, .27, 11, 3.1, .5);
                    add += float3(.82, .92, 1) * (rainNear * .26 + rainFar * .12) * _Rain;
                    shade += rainNear * .03 * _Rain;
                }
                if (_Sun > .001)
                {
                    const float2 across = float2(.83, .56);
                    float u = dot(p, across);
                    float shaft = smoothstep(.48, .92, noise(float2(u * .32 + t * .035, 2.7)));
                    float2 m = p * .9 + float2(t * .05, t * .09);
                    float2 cell = floor(m), d = frac(m) - .5 - (float2(hash(cell), hash(cell + 2.3)) - .5) * .6;
                    float twinkle = _Calm > .5 ? .6 : .5 + .5 * sin(t * 2.1 + hash(cell + 7.7) * 30);
                    add += float3(1, .9, .6) * exp(-dot(d, d) * 600) * step(.8, hash(cell + 4.4)) * shaft * twinkle * .5 * _Sun;
                }
                return float4(add, shade);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 c = _Layer < .5 ? Floor(i.world, _Clock) : Air(i.world, _Clock);
                return c * i.color.a;
            }
            ENDCG
        }
    }
}
