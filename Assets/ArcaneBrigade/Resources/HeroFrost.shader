// Frost on a frozen enemy (owner 24/10/2026): its sprite whitened and laid with crystal facets, brightest at the facet
// edges; vertex alpha melts it away.
Shader "ArcaneBrigade/HeroFrost"
{
    Properties { [PerRendererData] _MainTex ("Sprite", 2D) = "white" {} }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" "PreviewType" = "Plane" "CanUseSpriteAtlas" = "True" }
        Cull Off Lighting Off ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; float2 w : TEXCOORD1; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; o.color = v.color; o.w = mul(unity_ObjectToWorld, v.vertex).xy; return o; }
            float2 h2(float2 p) { return frac(sin(float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)))) * 43758.5453); }
            fixed4 frag (v2f i) : SV_Target
            {
                float m = tex2D(_MainTex, i.uv).a;
                float2 p = i.w * 7, c = floor(p); float d1 = 9, d2 = 9;
                for (int y = -1; y <= 1; y++) for (int x = -1; x <= 1; x++)
                {
                    float2 o = float2(x, y), q = o + h2(c + o) - frac(p); float d = dot(q, q);
                    if (d < d1) { d2 = d1; d1 = d; } else if (d < d2) d2 = d;
                }
                float edge = 1 - smoothstep(0, .08, sqrt(d2) - sqrt(d1));
                fixed3 col = lerp(fixed3(.82, .94, 1), fixed3(1, 1, 1), edge) * i.color.rgb;
                return fixed4(col, saturate(m * (.38 + .55 * edge) * i.color.a));
            }
            ENDCG
        }
    }
}
