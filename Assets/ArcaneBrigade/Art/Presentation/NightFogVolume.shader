Shader "ArcaneBrigade/NightFogVolume"
{
 SubShader { Cull Off ZWrite Off ZTest Always Pass { CGPROGRAM
 #pragma vertex vert_img
 #pragma fragment frag
 #pragma target 3.0
 #include "UnityCG.cginc"
 float4 _FogRect,_SharedLightRect,_NightLanternRect,_FogRuptures[8];
 float _FogClock,_FogDensity,_SharedLightEnabled,_NightLanternEnabled;int _FogSamples;
 sampler2D _SharedLightField,_NightLanternField;
 float hash(float3 p){p=frac(p*.1031);p+=dot(p,p.yzx+33.33);return frac((p.x+p.y)*p.z);}
 float noise(float3 p)
 {
  float3 c=floor(p),f=frac(p);f=f*f*(3-2*f);
  return lerp(lerp(lerp(hash(c),hash(c+float3(1,0,0)),f.x),lerp(hash(c+float3(0,1,0)),hash(c+float3(1,1,0)),f.x),f.y),lerp(lerp(hash(c+float3(0,0,1)),hash(c+float3(1,0,1)),f.x),lerp(hash(c+float3(0,1,1)),hash(c+1),f.x),f.y),f.z);
 }
 float fbm(float3 p){return noise(p)*.57+noise(p*2.03+11)*.28+noise(p*4.07+23)*.15;}
 fixed4 frag(v2f_img i):SV_Target
 {
  float2 p=_FogRect.xy+i.uv*_FogRect.zw;
  float4 party=tex2D(_SharedLightField,saturate((p-_SharedLightRect.xy)/max(.01,_SharedLightRect.zw)))*_SharedLightEnabled;
  float4 lamps=tex2D(_NightLanternField,saturate((p-_NightLanternRect.xy)/max(.01,_NightLanternRect.zw)))*_NightLanternEnabled;
  float light=saturate(max(max(party.r,party.g),max(party.b,lamps.a)));
  float2 flow=float2(_FogClock*.055,_FogClock*-.025);
  float warp=noise(float3(p*.11,_FogClock*.009));
  float rupture=0;for(int j=0;j<8;j++){float2 delta=p-_FogRuptures[j].xy;rupture+=exp(-dot(delta,delta)*.18)*_FogRuptures[j].w;}rupture=saturate(rupture);
  float transmission=1;float scattering=0;
  for(int step=0;step<8;step++)
  {
   if(step>=_FogSamples)break;
   float z=(step+.5)/max(1,_FogSamples);
   float3 q=float3(p*.32+flow+float2(warp,-warp)*1.8,z*2.4+_FogClock*.017);
   float density=smoothstep(.30,.76,fbm(q))*_FogDensity*(1+rupture*.65);
   density*=lerp(1,.18,light); // The party's safe light disperses the densest layer.
   float absorb=exp(-density*2.5/max(1,_FogSamples));
   scattering+=(1-absorb)*transmission*(.22+.78*z);transmission*=absorb;
  }
  float3 tint=float3(.012,.017,.035)+max(party.rgb*.15,lamps.rgb*.38)*scattering;
  float alpha=min(.62,1-transmission);
  return float4(tint,alpha);
 }
 ENDCG } }
}
