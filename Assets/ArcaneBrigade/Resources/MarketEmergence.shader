Shader "ArcaneBrigade/MarketEmergence"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _FloorY ("Ground Cut", Float) = -10000
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="True" }
        Cull Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            fixed4 _Color;
            float _FloorY;
            struct Input { float4 position : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct Output { float4 position : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; float worldY : TEXCOORD1; };
            Output Vertex(Input input)
            {
                Output output;
                output.position = UnityObjectToClipPos(input.position);
                output.uv = input.uv;
                output.color = input.color * _Color;
                output.worldY = mul(unity_ObjectToWorld, input.position).y;
                return output;
            }
            fixed4 Fragment(Output input) : SV_Target
            {
                clip(input.worldY - _FloorY);
                fixed4 color = tex2D(_MainTex, input.uv) * input.color;
                color.rgb *= color.a;
                return color;
            }
            ENDCG
        }
    }
}
