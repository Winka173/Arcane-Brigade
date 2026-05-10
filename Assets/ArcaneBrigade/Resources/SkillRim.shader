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
            sampler2D _MainTex; float _SweepY, _SweepWidth;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; float wy : TEXCOORD1; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; o.color = v.color; o.wy = mul(unity_ObjectToWorld, v.vertex).y; return o; }
            fixed4 frag (v2f i) : SV_Target
            {
                float a = tex2D(_MainTex, i.uv).a * i.color.a;
                float band = saturate(1 - abs(i.wy - _SweepY) / max(.001, _SweepWidth));
                return fixed4(lerp(i.color.rgb, 1, band * .65), saturate(a * (.8 + .6 * band)));
            }
            ENDCG
        }
    }
}
