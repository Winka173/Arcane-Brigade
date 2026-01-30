Shader "ArcaneBrigade/WaitingGlow"
{
    Properties { [PerRendererData] _MainTex ("Sprite", 2D) = "white" {} _Color ("Tint", Color) = (1,1,1,1) }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off ZWrite Off Lighting Off Blend SrcAlpha One
        Pass
        {
            CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment frag
            #include "UnitySprites.cginc"
            fixed4 frag(v2f i) : SV_Target
            {
                float2 p = (i.texcoord - .5) * 2;
                float d = dot(p,p);
                float soft = pow(saturate(1-d), 3);
                return fixed4(i.color.rgb, i.color.a * soft);
            }
            ENDCG
        }
    }
}
