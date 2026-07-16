Shader "ArcaneBrigade/UnitFlash"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Flash ("White flash", Range(0,1)) = 0
        _FlashColor ("Flash colour", Color) = (1,1,1,1)
        _FlashLine ("Flash reveal line (sprite height)", Float) = 2
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
        _StarTone ("Star tone", Color) = (1,1,1,1)
        _StarAmount ("Star sheen", Range(0,1)) = 0
        _StarRainbow ("Star rainbow", Range(0,1)) = 0
        _Saturation ("Paint saturation", Range(0,2)) = 1
        _NightVisibility ("Night visibility", Range(0,1)) = 1
        _HostileShift ("Warm colours pushed to violet (enemies)", Range(0,1)) = 0
        _Exposure ("Brightness (friends lifted, enemies dimmed)", Range(0,2)) = 1
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
            fixed4 _FlashColor;
            float _FlashLine;
            float4 _UvRect;
            float4 _MainTex_TexelSize;
            fixed4 _ShadeColor;
            fixed4 _RimColor;
            float _ShadeAmount, _RimAmount, _Saturation;
            fixed4 _StarTone;
            float _StarAmount, _StarRainbow;
            float3 starHue(float h) { float3 k = saturate(abs(frac(h + float3(0, 2.0 / 3.0, 1.0 / 3.0)) * 6 - 3) - 1); return lerp(float3(1, 1, 1), k, .7); }
            float _NightVisibility;
            float _HostileShift;
            float _Exposure;
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
                // a starred friend carries its star in the body itself (requirement 27/10/2026): a band of star-coloured light sweeps over
                // the painting and a faint glow breathes inside it, so the rank reads from far away (rainbow at five stars)
                if (_StarAmount > 0)
                {
                    float3 tone = lerp(_StarTone.rgb, starHue(local.y * .5 + local.x * .3 + _Time.y * .25), _StarRainbow);
                    float band = pow(saturate(sin((local.x * 1.5 + local.y * 1.1 - _Time.y * .7) * 6.2832) * .5 + .5), 5);
                    float breathe = .5 + .5 * sin(_Time.y * 2.2 + local.y * 4);
                    color.rgb = saturate(color.rgb + tone * (band * .55 + breathe * .12) * _StarAmount * color.a);
                }
                float luminance = dot(color.rgb, fixed3(0.2126,0.7152,0.0722));
                color.rgb = saturate(lerp(luminance.xxx, color.rgb, _Saturation));
                // enemies (requirement 24/10/2026): gold, amber, brass and cream are the friends' colours, so on an enemy they turn to a
                // violet pewter of the same value; reds (eyes, velvet, wax) and the cool colours stay as painted
                if (_HostileShift > 0)
                {
                    float3 c = color.rgb; float hi = max(c.r, max(c.g, c.b)), lo = min(c.r, min(c.g, c.b)), span = hi - lo;
                    float hue = 0;
                    if (span > 1e-4) hue = hi == c.r ? frac((c.g - c.b) / span / 6) : hi == c.g ? ((c.b - c.r) / span + 2) / 6 : ((c.r - c.g) / span + 4) / 6;
                    float sat = hi > 0 ? span / hi : 0;
                    float warm = smoothstep(.04, .075, hue) * (1 - smoothstep(.17, .22, hue)) * smoothstep(.12, .3, sat);
                    float cream = (1 - smoothstep(.08, .3, sat)) * smoothstep(.55, .85, hi) * step(c.b, c.r);
                    float w = saturate(warm + cream * .6) * _HostileShift;
                    float value = dot(c, fixed3(.2126, .7152, .0722));
                    color.rgb = lerp(c, saturate(value * float3(.86, .74, 1.12) + float3(.02, 0, .05)), w * .8);
                }
                // the flash covers the sprite below _FlashLine only (a wet-brush edge), so a reveal can wipe it away top-down
                float covered = saturate((_FlashLine + sin(local.x * 17 + _FlashLine * 5) * .035 - local.y) * 16);
                color.rgb = lerp(color.rgb, _FlashColor.rgb, _Flash * covered);
                color.rgb *= _NightVisibility * _Exposure;
                return color;
            }
            ENDCG
        }
    }
}
