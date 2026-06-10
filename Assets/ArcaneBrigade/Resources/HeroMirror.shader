// Mirror Keeper's panes (requirement 24/10/2026): the pane's alpha shows a reflection of what is round it (the grabbed screen
// mirrored about a line through the pane, _Center in screen uv), silvered, with a band of light sweeping across and a
// ripple (_Ripple) when it catches a shot.
Shader "ArcaneBrigade/HeroMirror"
{
    Properties
    {
        [PerRendererData] _MainTex ("Pane", 2D) = "white" {}
        _Center ("Pane centre (screen uv)", Vector) = (.5, .5, 0, 0)
        _Ripple ("Ripple", Float) = 0
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
            sampler2D _MainTex, _HeroGrab; float4 _Center; float _Ripple;
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
                float2 s = i.grab.xy / i.grab.w;
                float2 r = float2(2 * _Center.x - s.x, s.y + (s.y - _Center.y) * .6 + .04);   // mirrored, a little foreshortened
                r += sin(i.w.yx * 18 + _Time.y * 9) * .006 * _Ripple;
                fixed3 g = tex2D(_HeroGrab, r).rgb;
                fixed3 silver = lerp(g, dot(g, fixed3(.3, .59, .11)).xxx, .35) * .85 + fixed3(.08, .09, .11);
                float band = saturate(1 - abs(frac((i.w.x + i.w.y) * .6 - _Time.y * .5) - .5) * 8);
                return fixed4(silver + band * .35 * i.color.rgb, m * .7);
            }
            ENDCG
        }
    }
}
