Shader "ArcaneBrigade/SharedLightField"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            float4 _PartyLights[16], _PartyColors[16], _PartyRect;
            int _PartyCount;
            fixed4 frag(v2f_img i) : SV_Target
            {
                float2 world = _PartyRect.xy + i.uv * _PartyRect.zw;
                float3 color = 0;
                float strongest = 0;
                for (int j = 0; j < _PartyCount; j++)
                {
                    float2 offset = world - _PartyLights[j].xy;
                    float f = saturate(1 - dot(offset, offset) / (_PartyLights[j].z * _PartyLights[j].z));
                    f *= f;
                    color = max(color, _PartyColors[j].rgb * f);
                    strongest = max(strongest, f);
                }
                return fixed4(color, strongest);
            }
            ENDCG
        }
    }
}
