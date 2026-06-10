// Metal catching the light (requirement 24/10/2026): a bright band sweeping across a sprite (brass, gold, ivory), and with
// _Veins a web of thin metal seams through it (China Marionette's kintsugi, the sap in a sprout's leaves). Added light.
Shader "ArcaneBrigade/HeroMetal"
{
    Properties { [PerRendererData] _MainTex ("Sprite", 2D) = "white" {} _Veins ("Veins", Float) = 0 _Speed ("Sweep speed", Float) = .6 }
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
            sampler2D _MainTex; float _Veins, _Speed;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; float2 w : TEXCOORD1; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; o.color = v.color; o.w = mul(unity_ObjectToWorld, v.vertex).xy; return o; }
            fixed4 frag (v2f i) : SV_Target
            {
                float m = tex2D(_MainTex, i.uv).a;
                float s = frac((i.w.x * .8 + i.w.y) * .55 - _Time.y * _Speed);
                float band = pow(saturate(1 - abs(s - .5) * 6), 2);
                float2 p = i.w * 6;
                float v = abs(sin(p.x * 1.9 + sin(p.y * 1.3) * 2.2) * sin(p.y * 1.7 + sin(p.x * .9) * 2.4));
                float vein = (1 - smoothstep(0, .09, v)) * _Veins;
                float glint = vein * (.6 + .4 * sin(_Time.y * 3 + p.x));
                return fixed4(lerp(i.color.rgb, 1, band * .5), saturate(m * (band * .75 + glint) * i.color.a));
            }
            ENDCG
        }
    }
}
