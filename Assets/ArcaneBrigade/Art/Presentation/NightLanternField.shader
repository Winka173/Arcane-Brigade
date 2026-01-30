Shader "ArcaneBrigade/NightLanternField"
{
 SubShader { Cull Off ZWrite Off ZTest Always Pass { CGPROGRAM
 #pragma vertex vert_img
 #pragma fragment frag
 #pragma target 3.0
 #include "UnityCG.cginc"
 float4 _LanternSources[8],_LanternBlockers[8],_LanternRect;
 fixed4 frag(v2f_img i):SV_Target
 {
  float2 p=_LanternRect.xy+i.uv*_LanternRect.zw;float illumination=0;
  for(int j=0;j<8;j++)
  {
   float4 l=_LanternSources[j];if(l.w<=0)continue;
   float2 delta=p-l.xy;float distance2=dot(delta,delta);float edge=saturate(1-distance2/(l.z*l.z));
   float light=l.w*edge*edge/(1+distance2*.15);float shadow=1;
   for(int k=0;k<8;k++)
   {
    float4 b=_LanternBlockers[k];if(b.w<=0)continue;
    float along=dot(b.xy-l.xy,delta)/max(.01,distance2);
    if(along>.04 && along<.96){float closest=length(l.xy+delta*along-b.xy);float penumbra=.12+along*.5;shadow*=lerp(.2,1,smoothstep(b.z-penumbra,b.z+penumbra,closest));}
   }
   illumination=max(illumination,light*shadow);
  }
  return float4(float3(1,.55,.22)*illumination,illumination);
 }
 ENDCG } }
}
