// A paper puppet (owner 24/10/2026, Archive Curator's Living Exhibit): a recorded enemy drawn as a cream paper cut-out
// with an ink edge, fluttering a little; vertex colour tints it and fades it.
Shader "ArcaneBrigade/HeroPaper"
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
            sampler2D _MainTex; float4 _MainTex_TexelSize;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; float2 w : TEXCOORD1; };
            v2f vert (appdata v)
            {
                v2f o; float4 wp = mul(unity_ObjectToWorld, v.vertex);
                wp.x += sin(_Time.y * 5 + wp.y * 3) * .025;   // the paper flutters
                o.pos = mul(UNITY_MATRIX_VP, wp); o.uv = v.uv; o.color = v.color; o.w = wp.xy; return o;
            }
            fixed4 frag (v2f i) : SV_Target
            {
                float2 t = _MainTex_TexelSize.xy * 2.5;
                fixed4 s = tex2D(_MainTex, i.uv);
                float n = min(min(tex2D(_MainTex, i.uv + float2(t.x, 0)).a, tex2D(_MainTex, i.uv - float2(t.x, 0)).a), min(tex2D(_MainTex, i.uv + float2(0, t.y)).a, tex2D(_MainTex, i.uv - float2(0, t.y)).a));
                float edge = saturate((s.a - n) * 3);
                float grain = .93 + .07 * frac(sin(dot(floor(i.w * 60), float2(12.9898, 78.233))) * 43758.5453);
                fixed3 paper = lerp(fixed3(.96, .9, .78), s.rgb, .18) * grain * i.color.rgb;
                return fixed4(lerp(paper, fixed3(.16, .11, .08), edge), s.a * i.color.a);
            }
            ENDCG
        }
    }
}
