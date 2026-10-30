// Cartoon matter (flames, smoke puffs): the texture's shape painted in four flat colour bands, outline to heart, with
// crisp edges. The particle's alpha thins the shape, so a dying flame shrinks band by band instead of turning see-through.
Shader "ArcaneBrigade/RulesToon"
{
    Properties
    {
        _MainTex ("Shape", 2D) = "white" {}
        _C0 ("Outline", Color) = (0.55, 0.07, 0.06, 1)
        _C1 ("Outer", Color) = (0.93, 0.22, 0.1, 1)
        _C2 ("Inner", Color) = (1, 0.55, 0.1, 1)
        _C3 ("Heart", Color) = (1, 0.9, 0.45, 1)
        _Bands ("Band thresholds", Vector) = (0.08, 0.24, 0.45, 0.68)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex; float4 _MainTex_ST;
            fixed4 _C0, _C1, _C2, _C3; float4 _Bands;
            struct appdata { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.color = v.color; o.uv = TRANSFORM_TEX(v.uv, _MainTex); return o; }
            fixed4 frag (v2f i) : SV_Target
            {
                float s = tex2D(_MainTex, i.uv).a * i.color.a;
                float w = max(fwidth(s), 0.004);
                float b0 = smoothstep(_Bands.x - w, _Bands.x + w, s), b1 = smoothstep(_Bands.y - w, _Bands.y + w, s);
                float b2 = smoothstep(_Bands.z - w, _Bands.z + w, s), b3 = smoothstep(_Bands.w - w, _Bands.w + w, s);
                fixed3 c = lerp(lerp(lerp(_C0.rgb, _C1.rgb, b1), _C2.rgb, b2), _C3.rgb, b3);
                return fixed4(c * i.color.rgb, b0);
            }
            ENDCG
        }
    }
}
