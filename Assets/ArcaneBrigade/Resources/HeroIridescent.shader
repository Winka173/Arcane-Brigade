// Thin-film colour (owner 24/10/2026): soap film, dream glass, petals lit from behind. Hues drift with world position
// and time; _Edge 1 makes a bubble (bright at its rim, clear in its middle), 0 a sheen laid over a sprite. Added light.
Shader "ArcaneBrigade/HeroIridescent"
{
    Properties { [PerRendererData] _MainTex ("Mask", 2D) = "white" {} _Edge ("Bubble edge", Float) = 0 }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" "PreviewType" = "Plane" "CanUseSpriteAtlas" = "True" }
        Cull Off Lighting Off ZWrite Off
        Blend SrcAlpha One
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex; float _Edge;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; float2 w : TEXCOORD1; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; o.color = v.color; o.w = mul(unity_ObjectToWorld, v.vertex).xy; return o; }
            fixed3 hue(float h) { return saturate(abs(frac(h + float3(0, .667, .333)) * 6 - 3) - 1); }
            fixed4 frag (v2f i) : SV_Target
            {
                float m = tex2D(_MainTex, i.uv).a;
                float2 d = i.uv * 2 - 1; float r = length(d);
                float rim = lerp(1, smoothstep(.35, .95, r) * (1 - smoothstep(.95, 1.05, r)) * 1.6 + .08, _Edge);
                float h = frac(dot(i.w, float2(.7, .45)) * 1.1 + _Time.y * .22 + r * .6);
                fixed3 c = lerp(hue(h), 1, .45) * i.color.rgb;
                return fixed4(c, saturate(m * rim * i.color.a));
            }
            ENDCG
        }
    }
}
