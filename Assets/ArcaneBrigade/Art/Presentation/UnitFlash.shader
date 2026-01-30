Shader "ArcaneBrigade/UnitFlash"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Flash ("White flash", Range(0,1)) = 0
        _WeaponFlex ("Weapon secondary motion", Vector) = (0,0,0,0)
        _WeaponMask ("Articulated weapon source", Float) = 0
        _WeaponCut ("Articulated region", Vector) = (0,0,1,1)
        _LivingMotion ("Face mouth fabric response", Vector) = (0,0,0,0)
        _MechanicMotion ("Mechanical tension release", Vector) = (0,0,0,0)
        _Dissolve ("Species departure", Vector) = (0,0,0,0)
        _UvRect ("Sprite rectangle", Vector) = (0,0,1,1)
        _ShadeColor ("Ambient shade", Color) = (0.62,0.74,0.84,1)
        _RimColor ("Warm edge light", Color) = (1,0.88,0.62,1)
        _ShadeAmount ("Paint shading", Range(0,1)) = 0
        _RimAmount ("Edge strength", Range(0,1)) = 0
        _Saturation ("Paint saturation", Range(0,2)) = 1
        _NightVisibility ("Night visibility", Range(0,1)) = 1
        [HideInInspector] _PropEye ("Decorative eye region", Vector) = (0,0,0,0)
        [HideInInspector] _PropLook ("Decorative eye look", Vector) = (0,0,0,0)
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Cull Off Lighting Off ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnitySprites.cginc"
            float _Flash;
            float4 _UvRect;
            float4 _MainTex_TexelSize;
            fixed4 _ShadeColor;
            fixed4 _RimColor;
            float _ShadeAmount, _RimAmount, _Saturation;
            float _NightVisibility;
            float4 _PropEye, _PropLook;
            float4 _WeaponFlex;
            float _WeaponMask;
            float4 _WeaponCut;
            float4 _LivingMotion, _MechanicMotion;
            float4 _Dissolve;
            fixed4 frag(v2f input) : SV_Target
            {
                float2 sampleUV = input.texcoord;
                float2 weaponLocal = (sampleUV - _UvRect.xy) / max(_UvRect.zw, .00001);
                float2 deformation = float2(_WeaponFlex.x * pow(abs(weaponLocal.y - .5) * 2, 2), _WeaponFlex.z * (weaponLocal.x - .5) * sign(weaponLocal.y - .5));
                deformation += (weaponLocal - .5) * _WeaponFlex.y;
                float border = smoothstep(0, .12, weaponLocal.x) * smoothstep(0, .12, 1 - weaponLocal.x) * smoothstep(0, .12, weaponLocal.y) * smoothstep(0, .12, 1 - weaponLocal.y);
                if (dot(abs(_LivingMotion.xyz), float3(1,1,1)) > 0)
                {
                    float face = exp(-dot((weaponLocal - float2(.5,.61)) / float2(.3,.19), (weaponLocal - float2(.5,.61)) / float2(.3,.19)) * 2);
                    float mouth = exp(-dot((weaponLocal - float2(.5,.43)) / float2(.2,.1), (weaponLocal - float2(.5,.43)) / float2(.2,.1)) * 2);
                    deformation += float2(_LivingMotion.x * face, _LivingMotion.y * mouth) * border;
                    deformation.x += sin(weaponLocal.y * 9 + _LivingMotion.w) * _LivingMotion.z * border * (1 - face);
                }
                deformation.x += (_MechanicMotion.x + _MechanicMotion.y) * smoothstep(.25, .85, weaponLocal.x) * border;
                if (_MechanicMotion.z != 0) deformation.y += _MechanicMotion.z * sin(weaponLocal.x * 8) * border;
                sampleUV -= deformation * _UvRect.zw;
                float2 deformedLocal = (sampleUV - _UvRect.xy) / max(_UvRect.zw, .00001);
                float withinSprite = step(0, deformedLocal.x) * step(0, deformedLocal.y) * step(deformedLocal.x, 1) * step(deformedLocal.y, 1);
                if (_PropEye.w > 0)
                {
                    float distanceToEye = length((input.texcoord - _PropEye.xy) / max(_PropEye.zw, .00001));
                    float iris = 1 - smoothstep(.45, 1, distanceToEye);
                    sampleUV = clamp(sampleUV - _PropLook.xy * iris, _UvRect.xy, _UvRect.xy + _UvRect.zw);
                }
                fixed4 color = SampleSpriteTexture(sampleUV) * input.color;
                color.a *= withinSprite;
                float cut = step(_WeaponCut.x, weaponLocal.x) * step(_WeaponCut.y, weaponLocal.y) * step(weaponLocal.x, _WeaponCut.z) * step(weaponLocal.y, _WeaponCut.w);
                color.a *= 1 - _WeaponMask * cut;
                float2 local = saturate((input.texcoord - _UvRect.xy) / max(_UvRect.zw, 0.00001));
                if (_Dissolve.x > 0)
                {
                    float2 grain = floor(local * (58 + _Dissolve.w * 13));
                    float noise = frac(sin(dot(grain, float2(12.9898,78.233)) + _Dissolve.y) * 43758.5453);
                    float pattern = _Dissolve.z < .5 ? (1 - local.y) * .7 + noise * .3 : _Dissolve.z < 1.5 ? local.x * .6 + sin(local.y * 19 + _Dissolve.y) * .09 + noise * .3 : _Dissolve.z < 2.5 ? floor(local.y * 9) / 9 * .65 + noise * .35 : length(local - .5) * 1.2 + noise * .35;
                    color.a *= smoothstep(_Dissolve.x - .08, _Dissolve.x + .08, pattern);
                }
                color.rgb *= lerp(fixed3(1,1,1), _ShadeColor.rgb, (1 - local.y) * _ShadeAmount);
                float2 neighbor = input.texcoord + float2(-2,2) * _MainTex_TexelSize.xy;
                float inside = step(_UvRect.x, neighbor.x) * step(_UvRect.y, neighbor.y) * step(neighbor.x, _UvRect.x + _UvRect.z) * step(neighbor.y, _UvRect.y + _UvRect.w);
                float edge = saturate(color.a - SampleSpriteTexture(neighbor).a * inside);
                color.rgb = lerp(color.rgb, _RimColor.rgb, edge * _RimAmount);
                float luminance = dot(color.rgb, fixed3(0.2126,0.7152,0.0722));
                color.rgb = saturate(lerp(luminance.xxx, color.rgb, _Saturation));
                color.rgb = lerp(color.rgb, fixed3(1,1,1), _Flash);
                color.rgb *= _NightVisibility;
                return color;
            }
            ENDCG
        }
    }
}
