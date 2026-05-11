// Hero skills that bend what is behind them (owner 24/10/2026): water, gel and the push of a blow. One named grab of the
// screen is shared by every refracting piece in a frame; the sprite's alpha is where it bends, the vertex colour its tint
// and how strongly it shows, _Strength how far it bends (screen UV). A bright edge where the mask falls off.
Shader "ArcaneBrigade/HeroRefract"
{
    Properties
    {
        [PerRendererData] _MainTex ("Mask", 2D) = "white" {}
        _Strength ("Bend (screen uv)", Float) = .012
        _Speed ("Ripple speed", Float) = 1.5
        _Scale ("Ripple scale (world)", Float) = 5
        _Flow ("Flow (world/s)", Vector) = (0, 0, 0, 0)
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" "PreviewType" = "Plane" "CanUseSpriteAtlas" = "True" }
        Cull Off Lighting Off ZWrite Off
        GrabPass { "_HeroGrab" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex, _HeroGrab; float _Strength, _Speed, _Scale; float4 _Flow;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; float4 grab : TEXCOORD1; float2 w : TEXCOORD2; };
            v2f vert (appdata v)
            {
                v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; o.color = v.color;
                o.grab = ComputeGrabScreenPos(o.pos); o.w = mul(unity_ObjectToWorld, v.vertex).xy; return o;
            }
            fixed4 frag (v2f i) : SV_Target
            {
                float m = tex2D(_MainTex, i.uv).a * i.color.a;
                float tt = _Time.y * _Speed; float2 w = i.w * _Scale - _Flow.xy * _Time.y;
                float2 n = float2(sin(w.y * 1.7 + tt * 2.1) + sin(w.x * 2.3 - tt * 1.3), cos(w.x * 1.9 + tt * 1.7) + sin(w.y * 2.9 + tt)) * .5;
                float2 uv = i.grab.xy / i.grab.w + n * _Strength * m;
                fixed3 g = tex2D(_HeroGrab, uv).rgb;
                float edge = saturate(m * (1 - m) * 4);
                fixed3 col = lerp(g, g * i.color.rgb * 1.2, .3 * m) + edge * i.color.rgb * .3;
                return fixed4(col, saturate(m * 1.4));
            }
            ENDCG
        }
    }
}
