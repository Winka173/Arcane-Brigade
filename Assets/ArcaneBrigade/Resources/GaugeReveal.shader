// A hero gauge revealed in its own order (content expansion, shaped gauges): _OrderTex.r is when each pixel of the full painting
// shows (0..1, against _Fill); pixels with _OrderTex.g set drain instead (an hourglass's upper chamber, a melting scoop).
Shader "ArcaneBrigade/GaugeReveal"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _OrderTex ("Order", 2D) = "black" {}
        _Fill ("Fill", Range(0, 1)) = 0
        _Color ("Tint", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" "PreviewType" = "Plane" "CanUseSpriteAtlas" = "True" }
        Cull Off Lighting Off ZWrite Off
        Blend One OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex, _OrderTex; float _Fill; fixed4 _Color;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; o.color = v.color * _Color; return o; }
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv) * i.color;
                fixed4 o = tex2D(_OrderTex, i.uv);
                float shown = o.g > 0.5 ? step(_Fill, o.r) : step(o.r, _Fill);
                c.a *= shown; c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}
