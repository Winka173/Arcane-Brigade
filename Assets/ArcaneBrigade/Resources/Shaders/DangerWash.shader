// Danger zones on the floor: one uniform solid fill however many zones overlap (requirement 31/10/2026, review 3/11/2026:
// overlapping discs stacked into darker patches). Sprites/Default with a stencil test: the first zone to cover a pixel marks it
// and later zones in the same frame leave it alone, so a pixel is tinted once. Fully clear texels do not mark.
Shader "ArcaneBrigade/DangerWash"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        Stencil
        {
            Ref 23
            Comp NotEqual
            Pass Replace
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; fixed4 color : COLOR; float2 uv : TEXCOORD0; };
            sampler2D _MainTex; fixed4 _Color;
            v2f vert (appdata v) { v2f o; o.vertex = UnityObjectToClipPos(v.vertex); o.uv = v.uv; o.color = v.color * _Color; return o; }
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv) * i.color;
                clip(c.a - 0.03);
                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}
