// The rim of a hero skill (owner 23/10/2026): a solid-colour silhouette of the sprite, drawn in eight offset copies
// behind it so only a thin outline shows. A band of light (_SweepY, world height) runs up the body; vertex colour is
// the rim's tint and breathing alpha.
Shader "ArcaneBrigade/SkillRim"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _SweepY ("Sweep height (world)", Float) = -9999
        _SweepWidth ("Sweep width (world)", Float) = .25
        _Electric ("Electric crackle", Float) = 0
    }
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
            sampler2D _MainTex; float _SweepY, _SweepWidth, _Electric;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; float2 w : TEXCOORD1; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; o.color = v.color; o.w = mul(unity_ObjectToWorld, v.vertex).xy; return o; }
            fixed4 frag (v2f i) : SV_Target
            {
                float a = tex2D(_MainTex, i.uv).a * i.color.a;
                float band = saturate(1 - abs(i.w.y - _SweepY) / max(.001, _SweepWidth));
                // electric (Storm Fencer): sparks crawl along the edge, cells flicking on and off many times a second
                float2 cell = floor(i.w * 22 + float2(_Time.y * 13, -_Time.y * 9));
                float spark = step(.62, frac(sin(dot(cell, float2(12.9898, 78.233))) * 43758.5453));
                a *= lerp(1, spark * 1.6 + .15, saturate(_Electric));
                return fixed4(lerp(i.color.rgb, 1, saturate(band * .65 + spark * _Electric * .5)), saturate(a * (.8 + .6 * band)));
            }
            ENDCG
        }
    }
}
