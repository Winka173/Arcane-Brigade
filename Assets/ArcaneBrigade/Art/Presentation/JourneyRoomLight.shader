Shader "ArcaneBrigade/JourneyRoomLight"
{
 Properties { [PerRendererData] _MainTex("Room",2D)="white"{} _RoomAmbient("Ambient",Float)=.3 }
 SubShader { Tags {"Queue"="Transparent" "RenderType"="Transparent"} Cull Off Lighting Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex; float _RoomAmbient; int _RoomLightCount; float4 _RoomLights[16];
 struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
 struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;float2 world:TEXCOORD1;float4 color:COLOR;};
 v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xy;o.uv=v.uv;o.color=v.color;return o;}
 fixed4 frag(v2f i):SV_Target{fixed4 c=tex2D(_MainTex,i.uv)*i.color;float light=0;for(int j=0;j<16;j++){if(j>=_RoomLightCount)break;float t=saturate(1-distance(i.world,_RoomLights[j].xy)/_RoomLights[j].z);light=max(light,t*t*(3-2*t));}c.rgb*=lerp(_RoomAmbient,1,saturate(light));return c;}
 ENDCG }
 }
}
