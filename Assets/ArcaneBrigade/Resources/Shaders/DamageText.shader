// Damage numbers in their element: the digits painted top to bottom in the element's two colours and shaped like it -
// fire's digits lick upward like flames, lightning's jerk about in sudden jumps, water's ripple, venom's drip, dusk's
// twist, frost's stand still with a cold glint running across. The vertex colour's alpha fades the number.
Shader "ArcaneBrigade/DamageText"
{
    Properties
    {
        _MainTex ("Font", 2D) = "white" {}
        _Top ("Top", Color) = (1, 1, 1, 1)
        _Bottom ("Bottom", Color) = (1, 1, 1, 1)
        _Height ("Digit height (object space)", Float) = 0.25
        _Mode ("Shape: 0 plain, 1 fire, 2 lightning, 3 frost, 4 water, 5 venom, 6 dusk", Float) = 0
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
            sampler2D _MainTex; float4 _MainTex_ST; fixed4 _Top, _Bottom; float _Height, _Mode;
            struct appdata { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; float k : TEXCOORD1; float x : TEXCOORD2; };
            float hash(float n) { return frac(sin(n) * 43758.5453); }
            v2f vert (appdata v)
            {
                v2f o;
                float h = max(_Height, 0.001), t = _Time.y, x = v.vertex.x / h, top = v.vertex.y > 0 ? 1 : 0;
                float4 p = v.vertex;
                int mode = (int)round(_Mode);
                if (mode == 1)
                {
                    // flames: each digit's top stretches up and sways, flickering
                    p.y += top * h * (0.18 + 0.14 * sin(t * 17 + x * 13) + 0.06 * sin(t * 29 + x * 7));
                    p.x += top * h * 0.12 * sin(t * 11 + x * 9);
                }
                else if (mode == 2)
                {
                    // lightning: the digits jerk in sudden jumps twenty times a second
                    float step = floor(t * 20), cell = floor(x * 2.5);
                    p.x += h * 0.14 * (hash(step * 12.9 + cell * 78.2) - 0.5);
                    p.y += h * 0.12 * (hash(step * 39.3 + cell * 11.7) - 0.5);
                }
                else if (mode == 4) p.y += h * 0.1 * sin(t * 7 + x * 4);                         // water ripples
                else if (mode == 5) p.y -= (1 - top) * h * (0.1 + 0.1 * sin(t * 5 + x * 9));     // venom drips
                else if (mode == 6) p.x += h * 0.1 * sin(t * 5 + v.vertex.y / h * 6);            // dusk twists
                o.pos = UnityObjectToClipPos(p); o.color = v.color; o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.k = saturate(v.vertex.y / h + 0.5); o.x = x;
                return o;
            }
            fixed4 frag (v2f i) : SV_Target
            {
                fixed a = tex2D(_MainTex, i.uv).a * i.color.a;
                fixed3 c = lerp(_Bottom.rgb, _Top.rgb, i.k);
                int mode = (int)round(_Mode);
                if (mode == 3) c = lerp(c, fixed3(1, 1, 1), 0.7 * saturate(1 - abs(frac(_Time.y * 0.8 - i.x * 0.25) - 0.5) * 8)); // a cold glint runs across
                if (mode == 2) c = lerp(c, fixed3(1, 1, 1), step(0.85, hash(floor(_Time.y * 20) + floor(i.x * 2.5)))); // a flash now and then
                return fixed4(c * i.color.rgb, a);
            }
            ENDCG
        }
    }
}
