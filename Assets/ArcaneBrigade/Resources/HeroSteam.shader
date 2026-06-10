// Steam with body (requirement 24/10/2026): a soft blob whose density is broken by drifting procedural noise, so a puff swirls
// and frays at its edge instead of being a flat disc. Vertex colour is its tint and opacity.
Shader "ArcaneBrigade/HeroSteam"
{
    Properties { [PerRendererData] _MainTex ("Blob", 2D) = "white" {} }
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
            fixed4 frag (v2f i) : SV_Target
            {
                float m = tex2D(_MainTex, i.uv).a;
                float2 w = i.w * 3.1 + float2(0, -_Time.y * .9);
                float n = .5 + .25 * (sin(w.x * 1.7 + sin(w.y * 1.3 + _Time.y)) + sin(w.y * 2.3 + sin(w.x * 2.1 - _Time.y * 1.4)));
                float a = saturate(m * 1.6 - (1 - n) * .9) * i.color.a;
                return fixed4(lerp(i.color.rgb * .82, i.color.rgb, n), a);
            }
            ENDCG
        }
    }
}
